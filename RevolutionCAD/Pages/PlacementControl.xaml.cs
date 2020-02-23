using RevolutionCAD.Placement;
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
    /// Логика взаимодействия для PlacementControl.xaml
    /// </summary>
    public partial class PlacementControl : UserControl
    {
        List<StepPlacementLog> StepsLog;

        public int CurrentStep { get; set; }

        public PlacementControl()
        {
            InitializeComponent();
        }

        private List<StepPlacementLog> DoPlacement()
        {
            var steps = new List<StepPlacementLog>();

            string err_msg = "";
            
            var cmp = ApplicationData.ReadComposition(out err_msg);

            var sch = ApplicationData.ReadScheme(out err_msg);
            var matrR = sch.MatrixR;

            if (err_msg != "")
            {
                MessageBox.Show(err_msg, "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
                return steps;
            }

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
                    steps = TestPlacement.Place(cmp, out err_msg);
                    break;
            }
            // на последнем шаге получили результат размещения



            // если не было ошибки - сериализуем результат
            if (err_msg == "")
            {
                if (steps.Count != 0)
                {
                    //var result = steps.Last().BoardsList;
                    var result = new PlacementResult();
                    result.BoardsMatrices = steps.Last().BoardsList;
                    
                    var matrQ = sch.MatrixQ;
                    var dips = sch.DIPNumbers;
                    var elementsInBoards = ApplicationData.ReadComposition(out err_msg).BoardsElements;
                    
                    result.CreateBoardsDRPs(matrQ, elementsInBoards, dips, out err_msg);

                    if (err_msg != "")
                    {
                        MessageBox.Show(err_msg, "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }

                    ApplicationData.WritePlacement(result, out err_msg);

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

        private void ShowStep(int StepNumber)
        {
            TextBox_Log.ScrollToEnd();

            StackPanel_Boards.Children.Clear();
            var OneStep = StepsLog[StepNumber];
            for (int i = 0; i < OneStep.BoardsList.Count; i++)
            {
                var matr = OneStep.BoardsList[i];

                var sp_BoardCard = new StackPanel();
                sp_BoardCard.Orientation = Orientation.Vertical;
                sp_BoardCard.Margin = new Thickness(5);

                var tb_HeaderBoard = new TextBlock();
                tb_HeaderBoard.Margin = new Thickness(5);
                tb_HeaderBoard.Text = $"Узел №{i + 1}:";

                sp_BoardCard.Children.Add(tb_HeaderBoard);

                var sp_Board = new StackPanel();
                sp_Board.Orientation = Orientation.Vertical;

                for (int matrRow = 0; matrRow < matr.RowsCount; matrRow++)
                {
                    var spRow = new StackPanel();
                    spRow.Orientation = Orientation.Horizontal;

                    for (int matrCol = 0; matrCol < matr.ColsCount; matrCol++)
                    {
                        var tb_position = new TextBlock();
                        tb_position.Style = this.FindResource("TextBlockTemplate") as Style;

                        var tb_border = new Border();
                        tb_border.Style = this.FindResource("BorderPosTemplate") as Style;

                        if (matr[matrRow, matrCol] == -1)
                        {
                            tb_border.Background = new SolidColorBrush(Colors.Gray);
                            tb_border.Background.Opacity = 0.5;
                        } else
                        {
                            tb_position.Text = "D" + matr[matrRow, matrCol].ToString();
                        }
                        
                        tb_border.Child = tb_position;

                        spRow.Children.Add(tb_border);
                    }
                    sp_Board.Children.Add(spRow);
                }

                var border_Board = new Border();
                border_Board.Style = this.FindResource("BorderPlateTemplate") as Style;
                border_Board.Child = sp_Board;

                sp_BoardCard.Children.Add(border_Board);

                StackPanel_Boards.Children.Add(sp_BoardCard);
            }

        }

        private void Button_NextStep_Click(object sender, RoutedEventArgs e)
        {
            TextBox_Log.Text += $"Шаг №{CurrentStep + 1}:" + "\n";
            TextBox_Log.Text += StepsLog[CurrentStep].Message + "\n";
            ShowStep(CurrentStep);
            if (CurrentStep + 1 >= StepsLog.Count)
            {
                TextBox_Log.Text += "\n === Размещение закончено ===\n";
                DropStepMode();
            }
            else
            {
                CurrentStep++;
            }
        }

        private void Button_StartStepPlacement_Click(object sender, RoutedEventArgs e)
        {
            TextBox_Log.Text = "";
            StackPanel_Boards.Children.Clear();

            StepsLog = DoPlacement();

            if (StepsLog.Count == 0)
            {
                MessageBox.Show("Метод размещения не сработал", "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            Button_FullPlacement.IsEnabled = false;
            Button_StartStepPlacement.IsEnabled = false;
            Button_NextStep.IsEnabled = true;
            Button_DropStepMode.IsEnabled = true;

            CurrentStep = 0;

            Button_NextStep_Click(null, null);
        }

        private void Button_FullPlacement_Click(object sender, RoutedEventArgs e)
        {
            TextBox_Log.Text = "";
            StackPanel_Boards.Children.Clear();
            StepsLog = DoPlacement();
            if (StepsLog.Count == 0)
            {
                MessageBox.Show("Метод размещения не сработал", "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            for (int step = 0; step < StepsLog.Count; step++)
            {
                TextBox_Log.Text += $"Шаг №{step + 1}:" + "\n";
                TextBox_Log.Text += StepsLog[step].Message + "\n";
            }
            ShowStep(StepsLog.Count - 1); // отображаем только последний шаг графически, т.к. он будет результатом компоновки

        }

        private void Button_DropStepMode_Click(object sender, RoutedEventArgs e)
        {
            DropStepMode();
        }

        private void DropStepMode()
        {
            Button_FullPlacement.IsEnabled = true;
            Button_StartStepPlacement.IsEnabled = true;
            Button_NextStep.IsEnabled = false;
            Button_DropStepMode.IsEnabled = false;
        }
    }
}
