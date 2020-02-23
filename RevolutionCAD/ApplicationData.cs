using Newtonsoft.Json;
using RevolutionCAD.Composition;
using RevolutionCAD.Placement;
using RevolutionCAD.Tracing;
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

        public static CompositionResult ReadComposition(out string msg)
        {
            msg = "";
            CompositionResult cmp = null;
            if (IsFileExists(".cmp", out msg))
            {
                try
                {
                    using (StreamReader file = File.OpenText(FileName + ".cmp"))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        cmp = (CompositionResult)serializer.Deserialize(file, typeof(CompositionResult));
                    }
                }
                catch (Exception exp)
                {
                    msg = $"Произошла ошибка при попытке чтения файла {FileName}.cmp: {exp.Message}";
                }
            }
            return cmp;
        }

        public static void WriteComposition(CompositionResult cmp, out string errWrite)
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
            List<List<Contact>> WiresContacts;

            CreateMatrices(textSchemeDefinition, out MatrixR, out MatrixQ, out dipsNumbers, out WiresContacts, out errWrite);

            // сериализация данных в JSON
            var sch = new Scheme();
            sch.SchemeDefinition = textSchemeDefinition;
            sch.MatrixQ = MatrixQ;
            sch.MatrixR = MatrixR;
            sch.DIPNumbers = dipsNumbers;
            sch.WiresContacts = WiresContacts;

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

        public static List<List<Matrix<Cell>>> ReadTracing(out string msg)
        {
            msg = "";
            List<List<Matrix<Cell>>> trs = null;
            if (IsFileExists(".trs", out msg))
            {
                try
                {
                    using (StreamReader file = File.OpenText(FileName + ".trs"))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        trs = (List<List<Matrix<Cell>>>)serializer.Deserialize(file, typeof(List<List<Matrix<Cell>>>));
                    }
                }
                catch (Exception exp)
                {
                    msg = $"Произошла ошибка при попытке чтения файла {FileName}.trs: {exp.Message}";
                }
            }
            return trs;
        }

        public static void WriteTracing(List<List<Matrix<Cell>>> trs, out string msg)
        {
            msg = "";
            if (FileName != "")
            {
                try
                {
                    using (StreamWriter file = File.CreateText(FileName + ".trs"))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(file, trs);
                    }
                }
                catch (Exception exc)
                {
                    msg = $"При записи в файл трассировки произошла ошибка: {exc.Message}";
                }
            }
        }

        public static bool CreateMatrices(string textSchemeDefinition, out Matrix<int> R, out Matrix<int> Q, out List<int> dipsNumbers, out List<List<Contact>> wiresContactsList, out string errMsg)
        {
            errMsg = "";
            dipsNumbers = new List<int>();
            wiresContactsList = new List<List<Contact>>(); // список контактов для каждого провода
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
            int M = mas.Count; // число проводов

            R = new Matrix<int>(N, N); // матрица соединений
            Q = new Matrix<int>(N, M); // матрица элементных комплексов

            // обнуление матриц
            R.Fill(0);
            Q.Fill(0);

            // формирование матриц
            // запускам цикл по всем строкам, содержащим описание контактов провода
            int connectorCurrentContact = 0;
            for (int numberOfContact = 0; numberOfContact < mas.Count; numberOfContact++)
            {
                string s = mas[numberOfContact];

                var elements = new List<string>();
                var contacts = new List<Contact>();

                // делим строку на подстроки ("D1.2", "D8.3")
                elements = s.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                
                for (int i = 0; i < elements.Count; i++)
                {
                    string err;
                    var contact = GetContactInfoByText(elements[i], connectorCurrentContact, N, out err);
                    if (err != "")
                    {
                        errMsg = $"Ошибка в строке {N + numberOfContact + 1}: {err}";
                        return false;
                    }

                    if (contact.ElementNumber == 0)
                        connectorCurrentContact++;

                    contacts.Add(contact);
                }
                // добавляем список контактов только что сформированного проводника в общий список проводников
                wiresContactsList.Add(contacts);

                // получаем список номеров элементов
                var elementNumbers = contacts.Select(x => x.ElementNumber).ToList();
                
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

        public static Contact GetContactInfoByText(string str, int connectorContact, int countElements, out string err)
        {
            err = "";
            var contact = new Contact();

            if (str.Contains("X"))
            {
                if (str.Length != 1)
                {
                    err = $"Недопустимо назначение контакта разъёму";
                    return null;
                }
                contact.ElementNumber = 0;
                contact.ElementContact = connectorContact;
                return contact;
            }

            int posOfPoint = str.IndexOf("."); // находим точку в элементе после которой идёт номер ножки

            if (posOfPoint == -1)
            {
                err = $"У элемента не указан контакт подключения";
                return null;
            }

            int numberContact;

            if (Int32.TryParse(str.Substring(posOfPoint + 1), out numberContact) == false)
            {
                err = $"У элемента неверно указан номер контакта";
                return null;
            }

            contact.ElementContact = numberContact;

            str = str.Substring(0, posOfPoint).Replace("D", ""); // обрезаем эту часть, оставляем только номер платы
            if (str == "0")
            {
                err = $"Нумерация элементов должна начинаться с 1";
                return null;
            }

            int numberElement;

            if (Int32.TryParse(str, out numberElement) == false)
            {
                err = $"Ошибка в именовании элемента";
                return null;
            }

            if (numberElement + 1 > countElements) // +1 чтобы учесть разъём
            {
                err = $"Элементу не назначен тип корпуса DIP вначале файла";
                return null;
            }

            contact.ElementNumber = numberElement;

            return contact;


        }


    }
}
