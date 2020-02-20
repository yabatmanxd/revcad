using RevolutionCAD.Tracing;
using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для TracingControl.xaml
    /// </summary>
    public partial class TracingControl : UserControl
    {
        public TracingControl()
        {
            InitializeComponent();
        }

        public void Draw(Matrix<Cell> DRP)
        {
            // Создание вертикальной стекпанели
            StackPanel mainsp = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Background = new SolidColorBrush(Colors.Azure)
            };
            Grid.SetRow(mainsp, 1);
            Grid_Parent.Children.Add(mainsp);
            // добавление стекпанелей по строкам
            for(int i = 0; i<DRP.RowsCount; i++)
            {
                StackPanel rowsp = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Background = new SolidColorBrush(Colors.BlanchedAlmond)
                };
                mainsp.Children.Add(rowsp);
                // добавление строки элементов
                for(int c = 0; c<DRP.ColsCount; c++)
                {
                    Image elem = new Image
                    {
                        Height = 20,
                        Width = 20,
                        Tag = DateTime.Now
                    };
                    rowsp.Children.Add(elem);
                    switch (DRP[i, c].State)
                    {
                        case CellState.ArrowDown:
                            elem.Source =  new BitmapImage(
                                new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imArrowDown.png"));
                            break;
                        case CellState.ArrowUp:
                            elem.Source = new BitmapImage(
                                new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imArrowUp.png"));
                            break;
                        default:
                            elem.Source = new BitmapImage(
                                new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imEmpty.png"));
                            break;
                    }
                }
            }
        }

        private void Button_FullTracing_Click(object sender, RoutedEventArgs e)
        {
            Matrix<Cell> dpr = new Matrix<Cell>(2, 10);

            for (int i = 0; i < dpr.RowsCount; i++)
                for (int j = 0; j < dpr.ColsCount; j++)
                    dpr[i, j] = new Cell();

            dpr[0, 0].State = CellState.ArrowUp;
            dpr[0, 3].State = CellState.ArrowDown;
            dpr[0, 5].State = CellState.ArrowDown;
            dpr[0, 7].State = CellState.ArrowDown;
            dpr[0, 9].State = CellState.ArrowDown;
            dpr[1, 2].State = CellState.ArrowUp;
            dpr[1, 4].State = CellState.ArrowUp;
            dpr[1, 6].State = CellState.ArrowUp;
            dpr[1, 8].State = CellState.ArrowDown;

            Draw(dpr);
        }
    }
}
