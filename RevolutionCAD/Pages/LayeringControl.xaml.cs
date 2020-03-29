using RevolutionCAD.Layering;
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
    /// Логика взаимодействия для LayeringControl.xaml
    /// </summary>
    public partial class LayeringControl : UserControl
    {
        List<StepLayeringLog> StepsLog;

        public MainWindow mw;

        public int CurrentStep { get; set; }

        public LayeringControl()
        {
            InitializeComponent();
        }

        private List<StepLayeringLog> DoLayering()
        {
            var steps = new List<StepLayeringLog>();

            string err_msg = "";

            var trc = ApplicationData.ReadTracing(out err_msg);

            if (err_msg != "")
            {
                MessageBox.Show(err_msg, "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
                return steps;
            }

            switch (ComboBox_Method.SelectedIndex)
            {
                case 0:
                    steps = PosledLayering.Layer(trc, out err_msg);
                    break;
            }

            // если не было ошибки - сериализуем результат
            if (err_msg == "")
            {
                if (steps.Count != 0)
                {
                    var result = steps.Last().BoardsLayersDRPs;
                    ApplicationData.WriteLayering(result, out err_msg);

                    if (err_msg != "")
                    {
                        MessageBox.Show(err_msg, "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }
                }
            }
            else
                MessageBox.Show(err_msg, "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
            return steps;
        }

        private void Button_FullLayering_Click(object sender, RoutedEventArgs e)
        {
            TextBox_Log.Text = "";

            StepsLog = DoLayering();

            if (StepsLog.Count == 0)
            {
                MessageBox.Show("Метод расслоения не сработал", "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            for (int step = 0; step < StepsLog.Count; step++)
            {
                TextBox_Log.Text += $"Шаг №{step + 1}:" + "\n";
                TextBox_Log.Text += StepsLog[step].Message + "\n";
            }
            TextBox_Log.ScrollToEnd();
            ShowStep(StepsLog.Count - 1); // отображаем только последний шаг графически, т.к. он будет результатом расслоения

        }

        private void ShowStep(int StepNumber)
        {
            var OneStep = StepsLog[StepNumber];

            Draw(OneStep.BoardsLayersDRPs);

        }

        private void Draw(List<List<Matrix<Cell>>> boardsDRPs)
        {
            Grid_Parent.Children.Clear();
            for (int numBoard = 0; numBoard < boardsDRPs.Count; numBoard++)
            {

                var sp_BoardCard = new StackPanel();
                sp_BoardCard.Orientation = Orientation.Vertical;
                sp_BoardCard.Margin = new Thickness(50);

                var tb_HeaderBoard = new TextBlock();
                tb_HeaderBoard.Margin = new Thickness(5);
                tb_HeaderBoard.FontSize = 20;
                tb_HeaderBoard.FontWeight = FontWeights.Bold;
                tb_HeaderBoard.Text = $"Узел №{numBoard + 1}:";

                sp_BoardCard.Children.Add(tb_HeaderBoard);

                for(int layerNumber = 0; layerNumber < boardsDRPs[numBoard].Count; layerNumber++)
                {
                    var drp = boardsDRPs[numBoard][layerNumber];

                    var tb_HeaderLayer = new TextBlock();
                    tb_HeaderLayer.Margin = new Thickness(5);
                    tb_HeaderLayer.FontSize = 14;
                    tb_HeaderLayer.Text = $"Слой №{layerNumber + 1}:";

                    sp_BoardCard.Children.Add(tb_HeaderLayer);

                    var sp_Board = new StackPanel();
                    sp_Board.Orientation = Orientation.Vertical;

                    for (int i = 0; i < boardsDRPs[numBoard][layerNumber].RowsCount; i++)
                    {
                        // вью для отображения id элемента
                        TextBlock descriptionElem = new TextBlock();

                        var spRow = new StackPanel();
                        spRow.Orientation = Orientation.Horizontal;
                        // добавление строки элементов
                        for (int c = 0; c < boardsDRPs[numBoard][layerNumber].ColsCount; c++)
                        {
                            Grid elem = new Grid();
                            if (drp[i, c].Description != null)
                            {
                                descriptionElem = new TextBlock
                                {
                                    Text = drp[i, c].Description,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    FontSize = 15,
                                    FontWeight = FontWeights.Bold,
                                    Foreground = new SolidColorBrush(Colors.DarkOrange)
                                };
                                Canvas.SetZIndex(descriptionElem, 4);
                            }
                            Image picElem = new Image
                            {
                                Height = 26,
                                Width = 26
                            };
                            switch (drp[i, c].State)
                            {
                                case CellState.Contact:
                                    picElem.Source = new BitmapImage(
                                        new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imContact.png"));
                                    break;
                                case CellState.WireBottomLeft:
                                    picElem.Source = new BitmapImage(
                                        new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imWireBottomLeft.png"));
                                    break;
                                case CellState.WireBottomRight:
                                    picElem.Source = new BitmapImage(
                                        new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imWireBottomRight.png"));
                                    break;
                                case CellState.WireCross:
                                    picElem.Source = new BitmapImage(
                                        new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imWireCross.png"));
                                    break;
                                case CellState.WireHorizontal:
                                    picElem.Source = new BitmapImage(
                                        new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imWireHorizontal.png"));
                                    break;
                                case CellState.WireTopLeft:
                                    picElem.Source = new BitmapImage(
                                        new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imWireTopLeft.png"));
                                    break;
                                case CellState.WireTopRight:
                                    picElem.Source = new BitmapImage(
                                        new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imWireTopRight.png"));
                                    break;
                                case CellState.WireVertical:
                                    picElem.Source = new BitmapImage(
                                        new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imWireVertical.png"));
                                    break;
                                case CellState.WireLeftRightBottom:
                                    picElem.Source = new BitmapImage(
                                        new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imWireLeftRightBottom.png"));
                                    break;
                                case CellState.WireLeftRightTop:
                                    picElem.Source = new BitmapImage(
                                        new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imWireLeftRightTop.png"));
                                    break;
                                case CellState.WireLeftTopBottom:
                                    picElem.Source = new BitmapImage(
                                        new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imWireLeftTopBottom.png"));
                                    break;
                                case CellState.WireRightTopBottom:
                                    picElem.Source = new BitmapImage(
                                        new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imWireRightTopBottom.png"));
                                    break;
                                default:
                                    picElem.Source = new BitmapImage(
                                        new Uri("pack://application:,,,/RevolutionCAD;component/Resources/imEmpty.png"));
                                    break;
                            }
                            if (drp[i, c].Description != null)
                            {
                                Canvas.SetZIndex(picElem, 3);
                                elem.Children.Add(picElem);
                                if (drp[i, c].Description != null)
                                    elem.Children.Add(descriptionElem);
                                spRow.Children.Add(elem);
                            }
                            else
                            {
                                spRow.Children.Add(picElem);
                            }
                        }
                        sp_Board.Children.Add(spRow);
                    }

                    var brd = new Border();
                    brd.BorderThickness = new Thickness(1);
                    brd.BorderBrush = new SolidColorBrush(Colors.Black);
                    brd.Child = sp_Board;

                    sp_BoardCard.Children.Add(brd);
                }
                
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
                TextBox_Log.Text += "\n === Расслоение окончено ===\n";
                Button_FullLayering.IsEnabled = true;
                Button_StartStepLayering.IsEnabled = true;
                Button_NextStep.IsEnabled = false;
                Button_DropStepMode.IsEnabled = false; ;
            }
            else
            {
                CurrentStep++;
            }
        }

        private void Button_DropStepMode_Click(object sender, RoutedEventArgs e)
        {
            TextBox_Log.Text = "";
            for (int step = 0; step < StepsLog.Count; step++)
            {
                TextBox_Log.Text += $"Шаг №{step + 1}:" + "\n";
                TextBox_Log.Text += StepsLog[step].Message + "\n";
            }
            TextBox_Log.ScrollToEnd();
            ShowStep(StepsLog.Count - 1); // отображаем только последний шаг графически, т.к. он будет результатом трассировки

            Button_FullLayering.IsEnabled = true;
            Button_StartStepLayering.IsEnabled = true;
            Button_NextStep.IsEnabled = false;
            Button_DropStepMode.IsEnabled = false;
        }

        private void Button_StartStepLayering_Click(object sender, RoutedEventArgs e)
        {
            TextBox_Log.Text = "";
            Grid_Parent.Children.Clear();

            StepsLog = DoLayering();

            if (StepsLog.Count == 0)
            {
                MessageBox.Show("Метод трассировки не сработал", "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Button_FullLayering.IsEnabled = false;
            Button_StartStepLayering.IsEnabled = false;
            Button_NextStep.IsEnabled = true;
            Button_DropStepMode.IsEnabled = true;

            CurrentStep = 0;

            Button_NextStep_Click(null, null);
        }

        public void Update()
        {
            string t = "";
            var boardsDRPs = ApplicationData.ReadLayering(out t);
            if (t == "")
                Draw(boardsDRPs);
            else
            {
                TextBox_Log.Text = "";

                Button_FullLayering.IsEnabled = true;
                Button_StartStepLayering.IsEnabled = true;
                Button_NextStep.IsEnabled = false;
                Button_DropStepMode.IsEnabled = false;

                Grid_Parent.Children.Clear();
            }
        }
    }
}
