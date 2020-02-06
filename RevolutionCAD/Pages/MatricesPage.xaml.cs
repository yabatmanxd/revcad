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
            UpdateMatrices();
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

            

        }
    }
}
