using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Windows.Navigation;
using System.Linq;


namespace RevolutionCAD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItem_Create_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new TextInputWindow("Введите название файла:");
            wnd.Owner = this;
            if (wnd.ShowDialog() == true)
            {
                ApplicationData.FileName = wnd.Text;
                TextBlock_NameOpenedFile.Text = wnd.Text;
                TabControl_Main.Visibility = Visibility.Visible;
                // при создании файла откроется пример заполнения
                TextBox_Code.Text = "D1 dip14\r\nD2 dip14\r\nD3 dip14\r\nD4 dip18\r\n#\r\nD1.1-D2.1-D3.2-X.1\r\nD4.1-D2.1\r\nD3.1-D1.1";
                TextBox_Code.SelectionStart = TextBox_Code.Text.Length;
            }
            
        }

        private void MenuItem_Open_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new OpenFileDialog();
            wnd.InitialDirectory = Environment.CurrentDirectory;
            wnd.Multiselect = false;
            wnd.Filter = "Revolution CAD files (*.sch)|*.sch";

            if (wnd.ShowDialog() == true)
            {
                string fullPath = wnd.FileName;
                string fileName = Path.GetFileNameWithoutExtension(fullPath);

                ApplicationData.FileName = fileName;
                TextBlock_NameOpenedFile.Text = fileName;
                TabControl_Main.Visibility = Visibility.Visible;
                TextBox_Code.Text = File.ReadAllText(fullPath);
                TextBox_Code.SelectionStart = TextBox_Code.Text.Length;
            }
        }

        private void MenuItem_Save_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationData.FileName != "")
            {
                string writePath = $"{ApplicationData.FileName}.sch";
                try
                {
                    using (StreamWriter sw = new StreamWriter(writePath, false, System.Text.Encoding.Unicode))
                    {
                        sw.WriteLine(TextBox_Code.Text);
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show($"При записи в файл произошла ошибка: {exc.Message}", "Revolution CAD");
                }
            }
            
        }

        private void Button_CreateMatrices_Click(object sender, RoutedEventArgs e)
        {
            // сохраняем схему
            MenuItem_Save_Click(null, null);

            int N = 1; // количество микросхем (первая - разъём)
            bool IsSkipped = false; // флаг того, что ненужная часть файла пропущена
                                    // та, которая с dip

            // массив строк всего кода
            string[] lines = TextBox_Code.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            // массив для строк с проводниками
            List<string> mas = new List<string>();
            
            foreach(string line in lines)
            {
                if (line == "#") {
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

            Matrix<int> R = new Matrix<int>(N, N); // матрица соединений
            Matrix<int> Q = new Matrix<int>(N, M); // матрица элементных комплексов

            // обнуление матриц
            R.Fill(0);
            Q.Fill(0);

            // формирование матриц
            for (int numberOfContact = 0; numberOfContact<mas.Count; numberOfContact++)
            {
                string s = mas[numberOfContact];
               
                List<string> elements = new List<string>();

                // делим строку с элементами на список элементов ("D1.2", "D8.3")
                elements.AddRange(s.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries));

                // запускаем цикл по этим строкам, чтобы обрезать часть с номером ножки
                for (int i = 0; i<elements.Count; i++)
                {
                    int posOfPoint = elements[i].IndexOf("."); // находим точку в элементе после которой идёт номер ножки
                    if (posOfPoint == -1)
                    {
                        MessageBox.Show("У одного из элементов не указан контакт подключения", "Revolution CAD");
                        return;
                    }
                    elements[i] = elements[i].Substring(0, posOfPoint).Replace("D",""); // обрезаем эту часть, оставляем только номер платы
                    if (elements[i] == "X")
                    {
                        elements[i] = "0"; // так будет удобнее дальше для формирования матриц
                    }
                }

                // переходим от строк с названиями элементов ("0", "1") к непосредственным номерам int (0, 1)
                List<int> elementNumbers = new List<int>();
                foreach(string element in elements)
                {
                    int number;
                    if (Int32.TryParse(element,out number) == false)
                    {
                        MessageBox.Show($"Ошибка в именовании элементов в {N+1+ numberOfContact} строке", "Revolution CAD");
                        return;
                    }
                    if (number + 1 > N) // +1 чтобы учесть разъём
                    {
                        MessageBox.Show($"Ошибка в номере элемента в {N+1+ numberOfContact} строке. Все элементы должны быть описаны в начале файла до #", "Revolution CAD");
                        return;
                    }
                    elementNumbers.Add(number);
                }

                // так как у элемента могут быть внутренние связи, а в матрице мы их не учитываем
                elementNumbers = elementNumbers.Distinct().ToList(); // избавляемся от повторений номеров элементов

                for (int i = 0; i<elementNumbers.Count; i++)
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
            // сериализация данных в JSON

            using (StreamWriter file = File.CreateText(ApplicationData.FileName + ".r"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, R);
            }
            using (StreamWriter file = File.CreateText(ApplicationData.FileName + ".q"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, Q);
            }

            MatrControl.UpdateMatrices();
            TabControl_Main.SelectedIndex = 1;

        }
    }
}
