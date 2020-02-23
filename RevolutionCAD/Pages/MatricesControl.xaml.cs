using System.Data;
using System.Windows;
using System.Windows.Controls;

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
            string msg = "";
            Scheme sch = ApplicationData.ReadScheme(out msg);
            if (msg != "")
            {
                MessageBox.Show(msg, "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var R = sch.MatrixR;
            var Q = sch.MatrixQ;
            
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
