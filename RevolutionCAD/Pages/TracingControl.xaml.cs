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
                    steps = TestTracing.Trace(sch, plc, out err_msg);
                    break;
                case 1:
                    steps = TracingLi.Trace(sch, plc, out err_msg);
                    break;
                case 2:
                    steps = TracingAkers.Trace(sch, plc, out err_msg);
                    break;
                case 3:
                    steps = TracingLiMod.Trace(sch, plc, out err_msg);
                    break;
                case 4:
                    steps = TracingOncomingWave.Trace(sch, plc, out err_msg);
                    break;
                case 5:
                    steps = TracingConnectComplexes.Trace(sch, plc, out err_msg);
                    break;
                case 6:
                    steps = TracingTrackCoordinates.Trace(sch, plc, out err_msg);
                    break;
                case 7:
                    steps = TracingMinCrossing.Trace(sch, plc, out err_msg);
                    break;
                case 8:
                    steps = TracingUniformDistribution.Trace(sch, plc, out err_msg);
                    break;
                case 9:
                    steps = TracingTwoBeam.Trace(sch, plc, out err_msg);
                    break;
                case 10:
                    steps = TracingFourBeam.Trace(sch, plc, out err_msg);
                    break;
                case 11:
                    steps = TracingOptimized.Trace(sch, plc, out err_msg);
                    break;
                case 12:
                    steps = TracingHayes.Trace(sch, plc, out err_msg);
                    break;
            }
            
            // если не было ошибки - сериализуем результат
            if (err_msg == "")
            {
                if (steps.Count != 0)
                {
                    var result = steps.Last().BoardsDRPs;
                    ApplicationData.WriteTracing(result, out err_msg);
                    if (err_msg != "")
                    {
                        MessageBox.Show(err_msg, "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }
                }
            } else 
                MessageBox.Show(err_msg, "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
            return steps;
        }
        
        private void Button_FullTracing_Click(object sender, RoutedEventArgs e)
        {
            TextBox_Log.Text = "";
            StepsLog = DoTracing();
            if (StepsLog.Count == 0)
            {
                MessageBox.Show("Метод трассировки не сработал", "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            for (int step = 0; step < StepsLog.Count; step++)
            {
                TextBox_Log.Text += $"Шаг №{step + 1}:" + "\n";
                TextBox_Log.Text += StepsLog[step].Message + "\n";
            }
            TextBox_Log.ScrollToEnd();
            ShowStep(StepsLog.Count - 1); // отображаем только последний шаг графически, т.к. он будет результатом трассировки

        }

        private void ShowStep(int StepNumber)
        {
            var OneStep = StepsLog[StepNumber];

            Draw(OneStep.BoardsDRPs);
            
        }

        private void Draw(List<List<Matrix<Cell>>> boardsDRPs)
        {
            Grid_Parent.Children.Clear();
            for (int numBoard = 0; numBoard < boardsDRPs.Count; numBoard++)
            {
                var drp = ApplicationData.MergeLayersDRPs(boardsDRPs[numBoard]);

                var sp_BoardCard = new StackPanel();
                sp_BoardCard.Orientation = Orientation.Vertical;
                sp_BoardCard.Margin = new Thickness(25);

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

                var brd = new Border();
                brd.BorderThickness = new Thickness(1);
                brd.BorderBrush = new SolidColorBrush(Colors.Black);
                brd.Child = sp_Board;

                sp_BoardCard.Children.Add(brd);

                Grid_Parent.Children.Add(sp_BoardCard);
            }
        }

        private void Button_NextStep_Click(object sender, RoutedEventArgs e)
        {
            TextBox_Log.Text += $"Шаг №{CurrentStep + 1}:" + "\n";
            TextBox_Log.Text += StepsLog[CurrentStep].Message + "\n";
            TextBox_Log.ScrollToEnd();
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
                MessageBox.Show("Метод трассировки не сработал", "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
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

        

        public void Update()
        {
            string t = "";
            var boardsDRPs = ApplicationData.ReadTracing(out t);
            if (t == "")
                Draw(boardsDRPs);
        }
    }
}
