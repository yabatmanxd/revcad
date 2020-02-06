using System;
using System.Collections.Generic;
using System.Data;
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
    /// Логика взаимодействия для MatricesPage.xaml
    /// </summary>
    public partial class MatricesPage : Page
    {
        public MatricesPage()
        {
            InitializeComponent();
            
        }

        public void UpdateMatrices()
        {
            // тут должно быть считывание матриц Q и R из файла

            var R = new Matrix<int>(5, 5);
            for (int i = 0; i<5; i++)
            {
                for (int j = 0; j<5; j++)
                {
                    R[i, j] = i + j;
                }
            }

            var Q = new Matrix<int>(20, 5);
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    Q[i, j] = (i + j)%2;
                }
            }

            // запись матрицы R
            var dt1 = new DataTable();
            
            for (var i = 0; i < R.Width; i++)
            {
                if (i == 0)
                    dt1.Columns.Add(new DataColumn("X"));
                else
                    dt1.Columns.Add(new DataColumn("D" + i, typeof(string)));
            }
                
            for (var i = 0; i < R.Height; i++)
            {
                var r = dt1.NewRow();
                

                for (var j = 0; j < R.Width; j++)
                    r[j] = R[i,j];
                dt1.Rows.Add(r);
            }
            Matrix_R.ItemsSource = dt1.DefaultView;

            // запись матрицы Q
            var dt2 = new DataTable();

            for (var i = 0; i < Q.Width; i++)
            {
                dt2.Columns.Add(new DataColumn($"V{i+1}", typeof(string)));
            }

            for (var i = 0; i < Q.Height; i++)
            {
                var r = dt2.NewRow();


                for (var j = 0; j < Q.Width; j++)
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
