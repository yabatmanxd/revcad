using RevolutionCAD.Composition;
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
            string nevazhno = "";
            Scheme sch = ApplicationData.ReadScheme(out msg);
            var cmp = ApplicationData.ReadComposition(out nevazhno);
            if (msg != "")
                return;

            var R = sch.MatrixR;
            var Q = sch.MatrixQ;
            Matrix<int> cmpR = null;

            if (cmp != null)
            {
                if (cmp.MatrixR_AfterComposition != null)
                    cmpR = cmp.MatrixR_AfterComposition;
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


            if (cmpR != null)
            {
                var dt3 = new DataTable();

                dt3.Columns.Add(new DataColumn("X", typeof(string)));
                for (var i = 1; i < cmpR.ColsCount; i++)
                {
                    dt3.Columns.Add(new DataColumn("D" + i, typeof(string)));
                }

                for (var i = 0; i < cmpR.RowsCount; i++)
                {
                    var r = dt3.NewRow();

                    for (var j = 0; j < cmpR.ColsCount; j++)
                        r[j] = cmpR[i, j];
                    dt3.Rows.Add(r);
                }
                Matrix_R_Cmp.ItemsSource = dt3.DefaultView;
            }
        }

        private void Button_LoadMatrices_Click(object sender, RoutedEventArgs e)
        {
            UpdateMatrices();
        }
    }
}
