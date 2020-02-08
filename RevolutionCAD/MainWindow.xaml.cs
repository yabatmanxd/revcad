using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Collections.Generic;
//using System.Text.Json;
using Newtonsoft.Json;

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
            TabControl_Main.IsEnabled = false;
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
                TabControl_Main.IsEnabled = true;
                TextBox_Code.Text = "// пишите код здесь";
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
                TabControl_Main.IsEnabled = true;
                TextBox_Code.Text = File.ReadAllText(fullPath);
                TextBox_Code.SelectionStart = TextBox_Code.Text.Length;
            }
        }

        private void MenuItem_Save_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationData.FileName != "")
            {
                string writePath = $"{Environment.CurrentDirectory}\\{ApplicationData.FileName}.sch";
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
            int N = 1; // количество микросхем (первая - разъём)
            bool IsSkipped = false; // флаг того, что ненужная часть файла пропущена
                                    // та, которая с dip

            List<string> mas = new List<string>();
            string path = $"{ApplicationData.FileName}.sch";
            using (StreamReader sr = new StreamReader(path, System.Text.Encoding.Default))
            {
                string line; // получаем строку из файла
                while ((line = sr.ReadLine()) != null)
                {
                    if (line == "#")
                        IsSkipped = true;

                    if (!IsSkipped)
                        N++;
                    else
                        mas.Add(line);

                }
            }
            mas.Remove("#"); // удаление лишнего разделителя
            int M = mas.Count; // число проводов

            Matrix<int> R = new Matrix<int>(N, N); // матрица соединений
            Matrix<int> Q = new Matrix<int>(N, M); // матрица элементных комплексов

            // обнуление матриц
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    R[i, j] = 0;
            for (int i = 0; i < N; i++)
                for (int j = 0; j < M; j++)
                    Q[i, j] = 0;

            // формирование матриц
            int numberOfContact = 0; // номер текущего провода
            foreach (string s in mas)
            {
                int i = 0, j = 0;
                try
                {
                    // жёсткая структура с двумя проводами и числом знаков
                    i = int.Parse(s[1].ToString());
                    j = int.Parse(s[8].ToString());
                }
                catch (Exception)
                {
                    MessageBox.Show("Ошибка формирования матриц! Проверьте исходный файл!");
                }
                R[i, j]++; R[j, i]++;

                Q[i, numberOfContact] = 1;
                Q[j, numberOfContact] = 1;

                numberOfContact++;
            }

            // сериализация данных в JSON
            using (StreamWriter file = File.CreateText("R.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, R);
            }
            using (StreamWriter file = File.CreateText("Q.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, Q);
            }
            MessageBox.Show("Матрицы R и Q были удачно созданы!");

        }
    }
}
