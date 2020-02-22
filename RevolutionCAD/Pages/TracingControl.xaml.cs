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
        List<StepTracingLog> StepsLog;

        public int CurrentStep { get; set; }

        public TracingControl()
        {
            InitializeComponent();
        }

        private List<StepTracingLog> DoTracing()
        {
            var steps = new List<StepTracingLog>();

            string err_msg = "";

            var sch = ApplicationData.ReadScheme(out err_msg);

            var plc = ApplicationData.ReadPlacement(out err_msg);

            switch (ComboBox_Method.SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
                case 4:
                    break;
                case 5:
                    break;
                case 6:
                    break;
                case 7:
                    break;
                case 8:
                    break;
                case 9:
                    break;
                case 10:
                    break;
                case 11:
                    break;
                case 12:
                    steps = TestTracing.Trace(sch, plc, out err_msg);
                    break;
            }
            
            // если не было ошибки - сериализуем результат
            if (err_msg == "")
            {
                if (steps.Count != 0)
                {
                    if (steps.Count != 0)
                    {
                        var result = steps.Last().BoardsDRPs;
                        ApplicationData.WriteTracing(result, out err_msg);
                    }
                       
                }
            } else 
                MessageBox.Show(err_msg, "Revolution CAD");
            return steps;
        }
        
        private void Button_FullTracing_Click(object sender, RoutedEventArgs e)
        {
            TextBox_Log.Text = "";
            Grid_Parent.Children.Clear();
            StepsLog = DoTracing();
            if (StepsLog.Count == 0)
            {
                MessageBox.Show("Метод трассировки не сработал", "Revolution CAD");
                return;
            }
            for (int step = 0; step < StepsLog.Count; step++)
            {
                TextBox_Log.Text += $"Шаг №{step + 1}:" + "\n";
                TextBox_Log.Text += StepsLog[step].Message + "\n";
            }
            ShowStep(StepsLog.Count - 1); // отображаем только последний шаг графически, т.к. он будет результатом трассировки

        }

        private void ShowStep(int StepNumber)
        {
            TextBox_Log.ScrollToEnd();

            Grid_Parent.Children.Clear();
            var OneStep = StepsLog[StepNumber];

            for (int numBoard = 0; numBoard < OneStep.BoardsDRPs.Count; numBoard++)
            {
                var drp = MergeLayersDRP(OneStep.BoardsDRPs[numBoard]);

                var sp_BoardCard = new StackPanel();
                sp_BoardCard.Orientation = Orientation.Vertical;
                sp_BoardCard.Margin = new Thickness(5);

                var tb_HeaderBoard = new TextBlock();
                tb_HeaderBoard.Margin = new Thickness(5);
                tb_HeaderBoard.Text = $"Узел №{numBoard + 1}:";

                sp_BoardCard.Children.Add(tb_HeaderBoard);

                var sp_Board = new StackPanel();
                sp_Board.Orientation = Orientation.Vertical;

                for (int i = 0; i < drp.RowsCount; i++)
                {
                    var spRow = new StackPanel();
                    spRow.Orientation = Orientation.Horizontal;
                    // добавление строки элементов
                    for (int c = 0; c < drp.ColsCount; c++)
                    {
                        Image elem = new Image
                        {
                            Height = 12,
                            Width = 12,
                            Stretch = Stretch.Fill
                        };
                        switch (drp[i, c].State)
                        {
                            case CellState.ArrowDown:
                                elem.Source = new BitmapImage(
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
                        spRow.Children.Add(elem);
                    }
                    sp_Board.Children.Add(spRow);
                }

                sp_BoardCard.Children.Add(sp_Board);

                Grid_Parent.Children.Add(sp_BoardCard);
            }


        }

        private void Button_NextStep_Click(object sender, RoutedEventArgs e)
        {
            TextBox_Log.Text += $"Шаг №{CurrentStep + 1}:" + "\n";
            TextBox_Log.Text += StepsLog[CurrentStep].Message + "\n";
            ShowStep(CurrentStep);
            if (CurrentStep + 1 >= StepsLog.Count)
            {
                TextBox_Log.Text += "\n === Трассировка окончена ===\n";
                DropStepMode();
            }
            else
            {
                CurrentStep++;
            }
        }

        private void Button_DropStepMode_Click(object sender, RoutedEventArgs e)
        {
            DropStepMode();
        }

        private void Button_StartStepTracing_Click(object sender, RoutedEventArgs e)
        {
            TextBox_Log.Text = "";
            Grid_Parent.Children.Clear();

            StepsLog = DoTracing();

            if (StepsLog.Count == 0)
            {
                MessageBox.Show("Метод трассировки не сработал", "Revolution CAD");
                return;
            }


            Button_FullTracing.IsEnabled = false;
            Button_StartStepTracing.IsEnabled = false;
            Button_NextStep.IsEnabled = true;
            Button_DropStepMode.IsEnabled = true;

            CurrentStep = 0;

            Button_NextStep_Click(null, null);
        }

        private void DropStepMode()
        {
            Button_FullTracing.IsEnabled = true;
            Button_StartStepTracing.IsEnabled = true;
            Button_NextStep.IsEnabled = false;
            Button_DropStepMode.IsEnabled = false;
        }

        private Matrix<Cell> MergeLayersDRP(List<Matrix<Cell>> LayersDRPs)
        {
            int heightDRP = LayersDRPs.First().RowsCount;
            int widthDRP = LayersDRPs.First().ColsCount;

            var resDRP = new Matrix<Cell>(heightDRP, widthDRP);

            foreach (var layerDRP in LayersDRPs)
            {
                for (int i = 0; i < layerDRP.RowsCount; i++)
                {
                    for (int j = 0; j < layerDRP.ColsCount; j++)
                    {
                        if (layerDRP[i, j].State != CellState.Empty)
                            resDRP[i, j] = layerDRP[i, j].Clone();
                        else
                            resDRP[i, j] = new Cell();
                    }
                }
            }

            return resDRP;
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
            
        }
    }
}
