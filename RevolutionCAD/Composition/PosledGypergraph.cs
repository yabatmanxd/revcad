using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Composition
{
    class PosledGypergraph
    {
        /// <summary>
        /// Последовательная компоновка по гиперграфу
        /// </summary>
        /// <param name="countOfBoards">Ограничение на количество узлов</param>
        /// <param name="limitsOfWires">Ограничение на количество связей</param>
        /// <returns>Полный лог действий</returns>
        public static List<StepCompositionLog> Compose(int countOfElements, int limitsOfWires)
        {
            var log = new List<StepCompositionLog>();

            // считываем матрицу Q
            Matrix<int> Q = new Matrix<int>(1, 1);
            using (StreamReader file = File.OpenText(ApplicationData.FileName + ".q"))
            {
                JsonSerializer serializer = new JsonSerializer();
                Q = (Matrix<int>)serializer.Deserialize(file, typeof(Matrix<int>));
            }

            var E = new List<int>(); // множество распределённых элементов
            var nE = new List<int>(); // множество нераспределённых элементов
            var L1 = new List<int>();
            var L2 = new List<int>();
            var L3 = new List<int>();

            // формируем множество распределённых элементов
            E.Add(0); // только разъём
            // формируем множество нераспределённых элементов
            for (int i = 1; i < Q.RowsCount; i++)
                nE.Add(i); // все элементы кроме разъёма
            
            // создаём список элементов в узлах
            var boards = new List<List<int>>();
            int elem;
                        
            while(nE.Count != 0) // пока не распределили все элементы
            {
                boards.Add(new List<int>()); // добавляем новую плату
                L1.Clear(); 

                foreach (int it in nE)
                    L1.Add(DefineL1(it, E, Q)); // вычисляем для каждого элемента L1
                elem = nE[L1.IndexOf(L1.Max())]; // на плату отправляется элемент с максимальным L1
                
                boards.Last().Add(elem); // элемент - на плату
                E.Add(elem); nE.Remove(elem); // корректируем списки

                // начинаем логирование действия
                // формируем строку - обоснование
                string msg = "";
                for(int i = 0; i < nE.Count; i++)
                    msg += "Для D" + nE[i] + " - L1 = " + L1[i] + "\n";
                msg += "Поместили элемент D" + elem + " на " + boards.Count + " плату - он имеет максимальный L1";

                var step = new StepCompositionLog(boards, msg);
                log.Add(step);
                
                while(boards.Last().Count < countOfElements)
                {
                    if (nE.Count == 0)
                        break;
                    L2.Clear(); L3.Clear();

                    int previousElem = elem;
                    foreach (int it in nE)
                    {
                        var Z = boards.Last().GetRange(0, boards.Last().Count);

                        L2.Add(DefineL2(previousElem, it, Z, Q)); // вычисляем для каждого элемента L2
                        if (L2.Last() <= limitsOfWires)
                            L3.Add(DefineL3(previousElem, it, Q)); // и L3, если соблюдено условие о проводах
                        else
                            L3.Add(-1);
                    }

                    elem = nE[L3.IndexOf(L3.Max())];
                    boards.Last().Add(elem); // элемент - на плату
                    E.Add(elem); nE.Remove(elem); // корректируем списки

                    // начинаем логирование действия
                    // формируем строку - обоснование
                    msg = "";
                    for (int i = 0; i < nE.Count; i++)
                        msg += "Для D" + nE[i] + " - L2 = " + L2[i] + ", L3 = " + L3[i] + "\n";
                    msg += "Поместили элемент D" + elem + " на " + boards.Count + " плату - он имеет максимальный L3";
                    step = new StepCompositionLog(boards, msg);
                    log.Add(step);
                }
                
            }

            // формирование файла компоновки *.cmp
            using (StreamWriter file = File.CreateText(ApplicationData.FileName + ".cmp"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, log.Last().BoardsList);
            }

            // в качестве результата выполнения метода возвращаем целый пошаговый лог
            return log;
        }

        /// <summary>
        /// L1 - количество связей элемента number с множеством нераспределённых элементов
        /// </summary>
        private static int DefineL1(int number, List<int> E, Matrix<int> Q)
        {
            int count = 0;
            for (int i = 0; i < Q.ColsCount; i++)
                if (Q[number, i] == 1) // связь есть
                {
                    bool Failed = false; // флаг того, что есть связь с уже распределёнными элементами
                    foreach (int j in E)
                        if (Q[j, i] == 1)
                            Failed = true;
                    if (!Failed)
                        count++;
                }
            return count;
        }

        /// <summary>
        /// L2 - количество связей элементов number1 и number2 с множеством элементов, не входящих в данный узел
        /// </summary>
        private static int DefineL2(int number1, int number2, List<int> Z, Matrix<int> Q)
        {
            // Z - множество элементов текущего узла
            List<int> buf = Z;
            buf.Add(number1);
            buf.Add(number2);
            List<int> Numbers = buf.Distinct().ToList(); // удаление дубликатов
            // Numbers - множество элементов, связи с которыми не считаются
            
            int count = 0;
            foreach (int number in Numbers)
                for (int i = 0; i < Q.ColsCount; i++)
                    if (Q[number, i] == 1) // связь есть
                    {
                        bool Failed = false; // флаг того, что эта связь - с элементами в узле
                        foreach (int j in Numbers)
                            if(j != number)
                                if (Q[j, i] == 1)
                                    Failed = true;
                        if (!Failed)
                            count++;
                    }
            return count;
        }

        /// <summary>
        /// L3 - количество связей элемента number1 с элементом number2
        /// </summary>
        private static int DefineL3(int number1, int number2, Matrix<int> Q)
        {
            int count = 0;
            for (int i = 0; i < Q.ColsCount; i++)
                if (Q[number1, i] == 1 && Q[number2, i] == 1) // связь есть
                    count++;
            return count;
        }
    }
}
