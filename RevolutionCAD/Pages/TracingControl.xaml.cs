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
            //Grid.SetRow(mainsp, 1);
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
                        Height = 12,
                        Width = 12,
                        Stretch = Stretch.Fill
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
                        case CellState.Contact:
                            elem.Source = new BitmapImage(
                                new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imContact.png"));
                            break;
                        case CellState.WireBottomLeft:
                            elem.Source = new BitmapImage(
                                new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imWireBottomLeft.png"));
                            break;
                        case CellState.WireBottomRight:
                            elem.Source = new BitmapImage(
                                new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imWireBottomRight.png"));
                            break;
                        case CellState.WireCross:
                            elem.Source = new BitmapImage(
                                new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imWireCross.png"));
                            break;
                        case CellState.WireHorizontal:
                            elem.Source = new BitmapImage(
                                new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imWireHorizontal.png"));
                            break;
                        case CellState.WireTopLeft:
                            elem.Source = new BitmapImage(
                                new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imWireTopLeft.png"));
                            break;
                        case CellState.WireTopRight:
                            elem.Source = new BitmapImage(
                                new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imWireTopRight.png"));
                            break;
                        case CellState.WireVertical:
                            elem.Source = new BitmapImage(
                                new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imWireVertical.png"));
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
            Matrix<Cell> dpr = new Matrix<Cell>(20, 40);

            for (int i = 0; i < dpr.RowsCount; i++)
                for (int j = 0; j < dpr.ColsCount; j++)
                    dpr[i, j] = new Cell();

            dpr[2, 2].State = CellState.Contact;
            dpr[2, 4].State = CellState.Contact;
            dpr[2, 6].State = CellState.Contact;
            dpr[2, 8].State = CellState.Contact;
            dpr[2, 10].State = CellState.Contact;
            dpr[2, 12].State = CellState.Contact;
            dpr[2, 14].State = CellState.Contact;
            dpr[6, 2].State = CellState.Contact;
            dpr[6, 4].State = CellState.Contact;
            dpr[6, 6].State = CellState.Contact;
            dpr[6, 8].State = CellState.Contact;
            dpr[6, 10].State = CellState.Contact;
            dpr[6, 12].State = CellState.Contact;
            dpr[6, 14].State = CellState.Contact;
            dpr[11, 2].State = CellState.Contact;
            dpr[11, 4].State = CellState.Contact;
            dpr[11, 6].State = CellState.Contact;
            dpr[11, 8].State = CellState.Contact;
            dpr[11, 10].State = CellState.Contact;
            dpr[11, 12].State = CellState.Contact;
            dpr[11, 14].State = CellState.Contact;
            dpr[15, 2].State = CellState.Contact;
            dpr[15, 4].State = CellState.Contact;
            dpr[15, 6].State = CellState.Contact;
            dpr[15, 8].State = CellState.Contact;
            dpr[15, 10].State = CellState.Contact;
            dpr[15, 12].State = CellState.Contact;
            dpr[15, 14].State = CellState.Contact;

            dpr[3, 2].State = CellState.WireVertical;
            dpr[4, 2].State = CellState.WireTopRight;
            dpr[4, 3].State = CellState.WireHorizontal;
            dpr[4, 4].State = CellState.WireBottomLeft;
            dpr[5, 4].State = CellState.WireVertical;
            dpr[7, 4].State = CellState.WireVertical;
            dpr[8, 4].State = CellState.WireTopRight;
            dpr[8, 5].State = CellState.WireHorizontal;
            dpr[8, 6].State = CellState.WireBottomLeft;
            dpr[9, 6].State = CellState.WireVertical;
            dpr[10, 6].State = CellState.WireVertical;

            dpr[2, 7].State = CellState.WireBottomLeft;
            dpr[3, 7].State = CellState.WireVertical;
            dpr[4, 7].State = CellState.WireVertical;
            dpr[5, 7].State = CellState.WireVertical;
            dpr[6, 7].State = CellState.WireVertical;
            dpr[7, 7].State = CellState.WireVertical;
            dpr[8, 7].State = CellState.WireVertical;
            dpr[9, 7].State = CellState.WireTopRight;
            dpr[9, 8].State = CellState.WireHorizontal;
            dpr[9, 9].State = CellState.WireHorizontal;
            dpr[9, 10].State = CellState.WireBottomLeft;
            dpr[10, 10].State = CellState.WireVertical;

            dpr[2, 19].State = CellState.Contact;
            dpr[2, 21].State = CellState.Contact;
            dpr[2, 23].State = CellState.Contact;
            dpr[2, 25].State = CellState.Contact;
            dpr[2, 27].State = CellState.Contact;
            dpr[2, 29].State = CellState.Contact;
            dpr[2, 31].State = CellState.Contact;
            dpr[6, 19].State = CellState.Contact;
            dpr[6, 21].State = CellState.Contact;
            dpr[6, 23].State = CellState.Contact;
            dpr[6, 25].State = CellState.Contact;
            dpr[6, 27].State = CellState.Contact;
            dpr[6, 29].State = CellState.Contact;
            dpr[6, 31].State = CellState.Contact;
            dpr[11, 19].State = CellState.Contact;
            dpr[11, 21].State = CellState.Contact;
            dpr[11, 23].State = CellState.Contact;
            dpr[11, 25].State = CellState.Contact;
            dpr[11, 27].State = CellState.Contact;
            dpr[11, 29].State = CellState.Contact;
            dpr[11, 31].State = CellState.Contact;
            dpr[15, 19].State = CellState.Contact;
            dpr[15, 21].State = CellState.Contact;
            dpr[15, 23].State = CellState.Contact;
            dpr[15, 25].State = CellState.Contact;
            dpr[15, 27].State = CellState.Contact;
            dpr[15, 29].State = CellState.Contact;
            dpr[15, 31].State = CellState.Contact;

            Draw(dpr);
        }
    }
}
