using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD
{
    public static class ApplicationData
    {
        public static string FileName { get; set; }
        

        public static bool IsFileExists(string extension, out string error)
        {
            if (File.Exists(FileName + extension))
            {
                error = "";
                return true;
            }
            error = $"Файл {FileName}{extension} не существует";
            return false;
        }

        public static void ReadFileCmp()
        {

        }

        public static void WriteFileCmp()
        {

        }

        public static Scheme ReadScheme()
        {
            return new Scheme();
        }

        public static void WriteScheme(string textSchemeDefinition, out string errWrite)
        {
            Matrix<int> MatrixR = null;
            Matrix<int> MatrixQ = null;

            CreateMatrices(textSchemeDefinition, out MatrixR, out MatrixQ, out errWrite);

            // сериализация данных в JSON
            var sch = new Scheme();
            sch.SchemeDefinition = textSchemeDefinition;
            sch.MatrixQ = MatrixQ;
            sch.MatrixR = MatrixR;
            if (FileName != "")
            {
                try
                {
                    using (StreamWriter file = File.CreateText(FileName + ".sch"))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(file, sch);
                    }
                }
                catch(Exception exc)
                {
                    errWrite = $"При записи в файл произошла ошибка: {exc.Message}";
                }
            }

            
            
        }

        public static bool CreateMatrices(string textSchemeDefinition, out Matrix<int> R, out Matrix<int> Q, out string errMsg)
        {
            errMsg = "";
            int N = 1; // количество микросхем (первая - разъём)
            bool IsSkipped = false; // флаг того, что ненужная часть файла пропущена
                                    // та, которая с dip

            // массив строк всего кода
            string[] lines = textSchemeDefinition.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            // массив для строк с проводниками
            List<string> mas = new List<string>();

            foreach (string line in lines)
            {
                if (line == "#")
                {
                    IsSkipped = true;
                    continue;
                }

                if (!IsSkipped)
                    N++;
                else
                    mas.Add(line);
            }
            mas.Remove("#"); // удаление лишнего разделителя
            int M = mas.Count; // число проводов

            R = new Matrix<int>(N, N); // матрица соединений
            Q = new Matrix<int>(N, M); // матрица элементных комплексов

            // обнуление матриц
            R.Fill(0);
            Q.Fill(0);

            // формирование матриц
            for (int numberOfContact = 0; numberOfContact < mas.Count; numberOfContact++)
            {
                string s = mas[numberOfContact];

                List<string> elements = new List<string>();

                // делим строку с элементами на список элементов ("D1.2", "D8.3")
                elements.AddRange(s.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries));

                // запускаем цикл по этим строкам, чтобы обрезать часть с номером ножки
                for (int i = 0; i < elements.Count; i++)
                {
                    int posOfPoint = elements[i].IndexOf("."); // находим точку в элементе после которой идёт номер ножки
                    if (posOfPoint == -1)
                    {
                        errMsg = "У одного из элементов не указан контакт подключения";
                        return false;
                    }
                    elements[i] = elements[i].Substring(0, posOfPoint).Replace("D", ""); // обрезаем эту часть, оставляем только номер платы
                    if (elements[i] == "X")
                    {
                        elements[i] = "0"; // так будет удобнее дальше для формирования матриц
                    }
                }

                // переходим от строк с названиями элементов ("0", "1") к непосредственным номерам int (0, 1)
                List<int> elementNumbers = new List<int>();
                foreach (string element in elements)
                {
                    int number;
                    if (Int32.TryParse(element, out number) == false)
                    {
                        errMsg = $"Ошибка в именовании элементов в {N + 1 + numberOfContact} строке";
                        return false;
                    }
                    if (number + 1 > N) // +1 чтобы учесть разъём
                    {
                        errMsg = $"Ошибка в номере элемента в {N + 1 + numberOfContact} строке. Все элементы должны быть описаны в начале файла до #";
                        return false;
                    }
                    elementNumbers.Add(number);
                }

                // так как у элемента могут быть внутренние связи, а в матрице мы их не учитываем
                elementNumbers = elementNumbers.Distinct().ToList(); // избавляемся от повторений номеров элементов

                for (int i = 0; i < elementNumbers.Count; i++)
                {
                    for (int j = 0; j < elementNumbers.Count; j++)
                    {
                        if (i != j)
                        {
                            R[elementNumbers[i], elementNumbers[j]]++;
                        }
                    }
                    Q[elementNumbers[i], numberOfContact] = 1;
                }

            }
            return true;
        }


    }
}
