using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RevolutionCAD.Pages
{
    /// <summary>
    /// Логика взаимодействия для MatricesControl.xaml
    /// </summary>
    public partial class MatricesControl : UserControl
    {
        public MatricesControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Считывание матриц R и Q из JSON и их отображение
        /// </summary>
        public void UpdateMatrices()
        {
            // считывание матриц Q и R из файлов JSON .r и .q
            Matrix<int> R = new Matrix<int>(1, 1);
            Matrix<int> Q = new Matrix<int>(1, 1);
            string msg;
            if (ApplicationData.IsFileExists(".r", out msg) && ApplicationData.IsFileExists(".q", out msg))
            {
                try
                {
                    using (StreamReader file = File.OpenText(ApplicationData.FileName + ".r"))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        R = (Matrix<int>)serializer.Deserialize(file, typeof(Matrix<int>));
                    }
                    using (StreamReader file = File.OpenText(ApplicationData.FileName + ".q"))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        Q = (Matrix<int>)serializer.Deserialize(file, typeof(Matrix<int>));
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show($"Произошла ошибка при попытке считывания матриц из файлов: {exc.Message}", "Revolution CAD");
                }
            }
            else
            {
                MessageBox.Show(msg, "Revolution CAD");
            }


            // запись матрицы R
            var dt1 = new DataTable();

            dt1.Columns.Add(new DataColumn("X", typeof(string)));
            for (var i = 1; i < R.ColsCount; i++)
            {
                dt1.Columns.Add(new DataColumn("D" + i, typeof(string)));
            }

            for (var i = 0; i < R.RowsCount; i++)
            {
                var r = dt1.NewRow();

                for (var j = 0; j < R.ColsCount; j++)
                    r[j] = R[i, j];
                dt1.Rows.Add(r);
            }
            Matrix_R.ItemsSource = dt1.DefaultView;

            // запись матрицы Q
            var dt2 = new DataTable();

            for (var i = 0; i < Q.ColsCount; i++)
            {
                dt2.Columns.Add(new DataColumn($"V{i + 1}", typeof(string)));
            }

            for (var i = 0; i < Q.RowsCount; i++)
            {
                var r = dt2.NewRow();

                for (var j = 0; j < Q.ColsCount; j++)
                    r[j] = Q[i, j];
                dt2.Rows.Add(r);
            }
            Matrix_Q.ItemsSource = dt2.DefaultView;
        }

        private void Button_LoadMatrices_Click(object sender, RoutedEventArgs e)
        {
            UpdateMatrices();
        }
    }
}
