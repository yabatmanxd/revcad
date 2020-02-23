using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Composition
{
    class PosledMultigraph
    {
        /// <summary>
        /// Последовательная компоновка по мультиграфу
        /// </summary>
        /// <param name="countOfBoards">Ограничение на количество узлов</param>
        /// <param name="limitsOfWires">Ограничение на количество связей</param>
        /// <returns>Полный лог действий</returns>
        public static List<StepCompositionLog> Compose(int countOfElements, int limitsOfWires, out string error_msg)
        {
            var log = new List<StepCompositionLog>();

            var sch = ApplicationData.ReadScheme(out error_msg);
            // считываем матрицу R
            Matrix<int> R = sch.MatrixR;

            var EE = new List<int>();  // множество всех элементов кроме раъёма (для логирования)
            var E = new List<int>(); // множество распределённых элементов
            var nE = new List<int>(); // множество нераспределённых элементов
            var Ro = new List<int>();  // локальные степени вершин
            var RoConst = new List<int>(); // неизменные локальные степени вершин
            
            for (int i = 0; i < R.ColsCount - 1; i++)
            {
                Ro.Add(0); RoConst.Add(0);
            }
            // заполняем массив Ro суммой по столбцам (не учитывая разъём)
            for (int i = 0; i < R.ColsCount - 1; i++)
                for (int j = 0; j < R.ColsCount - 1; j++)
                {
                    Ro[i] += R[i + 1, j + 1];
                    RoConst[i] += R[i + 1, j + 1];
                }
                    
            // формируем множество распределённых элементов
            E.Add(0); // только разъём
            // формируем множество нераспределённых элементов
            for (int i = 1; i < R.RowsCount; i++)
            {
                nE.Add(i); // все элементы кроме разъёма
                EE.Add(i);
            }

            // создаём список элементов в узлах
            var boards = new List<List<int>>();
            int elem;
            int wires; // количество внешних связей

            while (nE.Count != 0) // пока не распределили все элементы
            {
                boards.Add(new List<int>()); // добавляем новую плату

                List<int> min = new List<int>();  // массив элементов с минимальным значением локальной степени
                foreach(int e in nE)
                {
                    if (Ro[e - 1] == Ro.Min())
                        min.Add(e);
                }
                if (min.Count == 1)
                    elem = min[0]; // если такой элемент один, то он отправляется на плату
                else
                {
                    // если таких элементов несколько, нужно выбрать такой, который имеет наименьшее количество инцедентных вершин
                    List<int> incidents = new List<int>();
                    foreach (int m in min)
                    {
                        incidents.Add(GetIncidents(m, R).Count);
                    }
                    elem = min[incidents.IndexOf(incidents.Min())];
                }

                
                // начинаем логирование действия
                // формируем строку - обоснование
                string msg = "";
                for(int i = 0; i < Ro.Count; i++)
                    if(Ro[i] != int.MaxValue)
                        msg += "Для D" + EE[i] + " - локальная степень вершины = " + Ro[i] + "\n";
                msg += "Поместили элемент D" + elem + " на " + boards.Count + 
                    " плату - он имеет минимальную локальную степень и минимальное число инцидентных вершин";

                boards.Last().Add(elem); // помещаем элемент на плату
                E.Add(elem); nE.Remove(elem); // корректируем списки
                wires = Ro[elem - 1];
                Ro[elem - 1] = int.MaxValue;
                
                var step = new StepCompositionLog(boards, msg);
                log.Add(step);

                
                while (boards.Last().Count < countOfElements)
                {
                    if (nE.Count == 0)
                        break;
                    
                    List<int> incidents = new List<int>();
                    foreach (int number in boards.Last())
                    {
                        List<int> l = GetIncidents(number, R); // инцидентные вершины
                        foreach (int buf in E)
                        {
                            l.Remove(buf); // удаляем вершины из множества распределённых элементов
                        }
                        incidents.AddRange(l);
                    }
                    incidents = incidents.Distinct().ToList();
                    
                    if(incidents.Count == 0)
                    {
                        msg = "Нет инцидентных вершин";
                        step = new StepCompositionLog(boards, msg);
                        log.Add(step);
                        break;
                    }

                    List<int> Delta = new List<int>(); // приращения
                    foreach(int it in incidents)
                    {
                        var X = boards.Last().GetRange(0, boards.Last().Count);

                        int delta = RoConst[it - 1] - 2 * GetCountOfWires(it, R, X);
                        Delta.Add(delta);
                    }
                    
                    if(Delta.Min() + wires <= limitsOfWires) // выбираем элемент с минимальным приращением
                    {
                        elem = incidents[Delta.IndexOf(Delta.Min())];
                        msg = "Количество внешних связей = " + wires + "\n";
                        for (int i = 0; i < incidents.Count; i++)
                            msg += "Для D" + incidents[i] + " приращение равно " + Delta[i] + "\n";

                        msg += "Поместили элемент D" + elem + " на " + boards.Count + " плату - он имеет минимальное приращение";

                        boards.Last().Add(elem); // помещаем элемент на плату
                        E.Add(elem); nE.Remove(elem); // корректируем списки
                        wires += Delta.Min();
                        Ro[elem - 1] = int.MaxValue;

                        step = new StepCompositionLog(boards, msg);
                        log.Add(step);
                    }
                    else
                    {
                        msg = "Количество внешних связей = " + wires + "\n";
                        for (int i = 0; i < incidents.Count; i++)
                            msg += "Для D" + incidents[i] + " приращение равно " + Delta[i] + "\n";

                        msg += "Ни один из элементов нельзя поместить на плату, потому что не соблюдается условие на ограничение связей";
                        step = new StepCompositionLog(boards, msg);
                        log.Add(step);
                        break;
                    }
                }
            }

            // в качестве результата выполнения метода возвращаем целый пошаговый лог
            return log;
        }

        /// <summary>
        /// Возвращает список вершин, инцидентных вершине под номером number
        /// </summary>
        private static List<int> GetIncidents(int number, Matrix<int> R)
        {
            List <int> list = new List<int>();

            for (int i = 0; i < R.ColsCount; i++)
                if(R[i, number] != 0)
                    list.Add(i);

            return list;
        }

        /// <summary>
        /// Возвращает количество связей элемента под номером number с множеством элементов узла X
        /// </summary>
        private static int GetCountOfWires(int number, Matrix<int> R, List<int> X)
        {
            int count = 0;
            foreach(int it in X)
            {
                count += R[it, number];
            }
            return count;
        }
    }
}
