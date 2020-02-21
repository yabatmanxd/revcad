using Newtonsoft.Json;
using RevolutionCAD.Composition;
using RevolutionCAD.Placement;
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

        public static int PinDistance = 1;
        public static int RowDistance = 3;
        public static int ElementsDistance = 4;

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

        public static List<List<int>> ReadComposition(out string msg)
        {
            msg = "";
            List<List<int>> cmp = null;
            if (IsFileExists(".cmp", out msg))
            {
                try
                {
                    using (StreamReader file = File.OpenText(FileName + ".cmp"))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        cmp = (List<List<int>>)serializer.Deserialize(file, typeof(List<List<int>>));
                    }
                }
                catch (Exception exp)
                {
                    msg = $"Произошла ошибка при попытке чтения файла {FileName}.cmp: {exp.Message}";
                }
            }
            return cmp;
        }

        public static void WriteComposition(List<List<int>> cmp, out string errWrite)
        {
            errWrite = "";
            if (FileName != "")
            {
                try
                {
                    using (StreamWriter file = File.CreateText(FileName + ".cmp"))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(file, cmp);
                    }
                }
                catch (Exception exc)
                {
                    errWrite = $"При записи в файл компоновки произошла ошибка: {exc.Message}";
                }
            }
        }

        public static Scheme ReadScheme(out string msg)
        {
            msg = "";
            Scheme sch = null;
            if (IsFileExists(".sch", out msg))
            {
                try
                {
                    using (StreamReader file = File.OpenText(FileName + ".sch"))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        sch = (Scheme)serializer.Deserialize(file, typeof(Scheme));
                    }
                } catch(Exception exp)
                {
                    msg = $"Произошла ошибка при попытке чтения файла {FileName}.sch: {exp.Message}";
                }
            }
            return sch;
        }

        public static void WriteScheme(string textSchemeDefinition, out string errWrite)
        {
            Matrix<int> MatrixR = null;
            Matrix<int> MatrixQ = null;
            List<int> dipsNumbers;

            CreateMatrices(textSchemeDefinition, out MatrixR, out MatrixQ, out dipsNumbers, out errWrite);

            // сериализация данных в JSON
            var sch = new Scheme();
            sch.SchemeDefinition = textSchemeDefinition;
            sch.MatrixQ = MatrixQ;
            sch.MatrixR = MatrixR;
            sch.DIPNumbers = dipsNumbers;

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
                    errWrite = $"При записи в файл схемы произошла ошибка: {exc.Message}";
                }
            }
            
        }

        public static PlacementResult ReadPlacement(out string msg)
        {
            msg = "";
            PlacementResult plc = null;
            if (IsFileExists(".plc", out msg))
            {
                try
                {
                    using (StreamReader file = File.OpenText(FileName + ".plc"))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        plc = (PlacementResult)serializer.Deserialize(file, typeof(PlacementResult));
                    }
                }
                catch (Exception exp)
                {
                    msg = $"Произошла ошибка при попытке чтения файла {FileName}.plc: {exp.Message}";
                }
            }
            return plc;
        }

        public static void WritePlacement(PlacementResult plc, out string errWrite)
        {
            errWrite = "";
            if (FileName != "")
            {
                try
                {
                    using (StreamWriter file = File.CreateText(FileName + ".plc"))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(file, plc);
                    }
                }
                catch (Exception exc)
                {
                    errWrite = $"При записи в файл размещения произошла ошибка: {exc.Message}";
                }
            }
        }

        public static bool CreateMatrices(string textSchemeDefinition, out Matrix<int> R, out Matrix<int> Q, out List<int> dipsNumbers, out string errMsg)
        {
            errMsg = "";
            dipsNumbers = new List<int>();
            dipsNumbers.Add(0);
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

                if (!IsSkipped) // значит это фрагмент до # с номером Dip
                {
                    N++;
                    // считываем номер Dip и записываем в список
                    int dipNumber;
                    string dipNumberStr = line.Replace("dip", "");
                    if (Int32.TryParse(dipNumberStr, out dipNumber))
                    {
                        if (dipNumber % 2 == 0)
                        {
                            dipsNumbers.Add(dipNumber);
                        } else
                        {
                            errMsg = $"DIP корпуса может быть только чётный в строке {N - 2}";
                            R = Q = null;
                            return false;
                        }
                        
                    }
                        
                    else
                    {
                        errMsg = $"Ошибка в именовании типа корпуса dip в строке {N - 2}";
                        R = Q = null;
                        return false;
                    }
                }
                    
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
