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

            switch (ComboBox_Method.SelectedIndex)
            {
                case 0:
                    steps = PosledMaxLastStepPlaced.Place();
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
            }

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

                var spBoard = new StackPanel();
                spBoard.Orientation = Orientation.Vertical;
                spBoard.Margin = new Thickness(5);

                var tb = new TextBlock();
                tb.Margin = new Thickness(5);
                tb.Text = $"Узел №{i + 1}:";

                spBoard.Children.Add(tb);

                for (int matrRow = 0; matrRow < matr.RowsCount; matrRow++)
                {
                    var spRow = new StackPanel();
                    spRow.Orientation = Orientation.Horizontal;

                    for (int matrCol = 0; matrCol < matr.ColsCount; matrCol++)
                    {
                        var position = new TextBlock();
                        tb.Margin = new Thickness(5);
                        tb.Padding = new Thickness(5);
                        tb.Text = "D" + matr[matrRow,matrCol].ToString();
                    }
                    spBoard.Children.Add(spRow);
                }
                
                StackPanel_Boards.Children.Add(spBoard);
            }

        }

        private void Button_NextStep_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_StartStepPlacement_Click(object sender, RoutedEventArgs e)
        {
            //TextBox_Log.Text = "";
            //StackPanel_Boards.Children.Clear();




            //Button_FullPlacement.IsEnabled = false;
            //Button_StartStepPlacement.IsEnabled = false;
            //Button_NextStep.IsEnabled = true;
            //Button_DropStepMode.IsEnabled = true;

            //CurrentStep = 0;

            //Button_NextStep_Click(null, null);
        }

        private void Button_FullPlacement_Click(object sender, RoutedEventArgs e)
        {
            TextBox_Log.Text = "";
            StackPanel_Boards.Children.Clear();
            StepsLog = DoPlacement();
            if (StepsLog.Count == 0)
            {
                MessageBox.Show("Метод размещения не сработал", "Revolution CAD");
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
