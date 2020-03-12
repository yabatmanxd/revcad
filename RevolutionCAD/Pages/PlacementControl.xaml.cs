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

            if (err_msg != "")
            {
                MessageBox.Show(err_msg, "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
                return steps;
            }

            var sch = ApplicationData.ReadScheme(out err_msg);

            if (err_msg != "")
            {
                MessageBox.Show(err_msg, "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
                return steps;
            }

            var matrR = cmp.MatrixR_AfterComposition;

            switch (ComboBox_Method.SelectedIndex)
            {
                case 0:
                    steps = PosledMaxLastStepPlaced.Place(cmp, matrR, out err_msg);
                    break;
                case 1:
                    steps = PosledMaxLastAllStepPlaced.Place(cmp, matrR, out err_msg);
                    break;
                case 2:
                    steps = PosledMaxPlacedMinUnplaced.Place(cmp, matrR, out err_msg);
                    break;
                case 3:
                    steps = IterFull.Place(matrR, out err_msg);
                    break;
                case 4:
                    steps = IterPair.Place(matrR, out err_msg);
                    break;
                case 5:
                    steps = IterShaffer.Place(matrR, out err_msg);
                    break;
                case 6:
                    steps = TestPlacement.Place(cmp, matrR, out err_msg);
                    break;
            }

            // если не было ошибки - сериализуем результат
            if (err_msg == "")
            {
                if (steps.Count != 0)
                {
                    var result = new PlacementResult();
                    result.BoardsMatrices = steps.Last().BoardsList;
                    
                    var dips = sch.DIPNumbers;

                    result.CreateBoardsDRPs(cmp, dips, out err_msg);
                    result.CreateWires(cmp.BoardsWires);
                    
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
            var OneStep = StepsLog[StepNumber];
            Draw(OneStep.BoardsList);
        }

        private void Draw(List<Matrix<int>> boardsMatrices)
        {
            StackPanel_Boards.Children.Clear();
            for (int i = 0; i < boardsMatrices.Count; i++)
            {
                var matr = boardsMatrices[i];

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
                        }
                        else
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
            TextBox_Log.ScrollToEnd();
            ShowStep(CurrentStep);
            if (CurrentStep + 1 >= StepsLog.Count)
            {
                TextBox_Log.Text += "\n === Размещение закончено ===\n";
                Button_FullPlacement.IsEnabled = true;
                Button_StartStepPlacement.IsEnabled = true;
                Button_NextStep.IsEnabled = false;
                Button_DropStepMode.IsEnabled = false;
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
            TextBox_Log.ScrollToEnd();
            ShowStep(StepsLog.Count - 1); // отображаем только последний шаг графически, т.к. он будет результатом компоновки

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
            ShowStep(StepsLog.Count - 1); // отображаем только последний шаг графически, т.к. он будет результатом компоновки


            Button_FullPlacement.IsEnabled = true;
            Button_StartStepPlacement.IsEnabled = true;
            Button_NextStep.IsEnabled = false;
            Button_DropStepMode.IsEnabled = false;
        }
        
        public void Update()
        {
            string t = "";
            var plc = ApplicationData.ReadPlacement(out t);
            if (t == "")
            {
                var boardsMatrices = plc.BoardsMatrices;
                Draw(boardsMatrices);
            } else
            {
                TextBox_Log.Text = "";

                Button_FullPlacement.IsEnabled = true;
                Button_StartStepPlacement.IsEnabled = true;
                Button_NextStep.IsEnabled = false;
                Button_DropStepMode.IsEnabled = false;

                StackPanel_Boards.Children.Clear();
            }
                
        }
    }
}
