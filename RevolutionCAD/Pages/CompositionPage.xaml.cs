using RevolutionCAD.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RevolutionCAD.Pages
{
    /// <summary>
    /// Логика взаимодействия для CompositionPage.xaml
    /// </summary>
    public partial class CompositionPage : Page
    {
        List<StepCompositionLog> StepsLog;

        public int CurrentStep { get; set; }

        public CompositionPage()
        {
            InitializeComponent();
        }

        private List<StepCompositionLog> DoComposition()
        {
            switch(ComboBox_Method.SelectedIndex)
            {
                case 0:
                    return PosledGypergraph.Compose();
                case 1:
                    return PosledMultigraph.Compose();
                case 2:
                    return IterGypergraph.Compose();
                case 3:
                    return IterMultigraph.Compose();
                default:
                    return null;
            }
        }

        private void Button_FullComposition_Click(object sender, RoutedEventArgs e)
        {
            TextBox_Log.Text = "";
            StackPanel_Boards.Children.Clear();
            StepsLog = DoComposition();
            for (int step = 0; step < StepsLog.Count; step++)
            {
                TextBox_Log.Text += StepsLog[step].Message + "\n";
            }
            ShowStep(StepsLog.Count - 1); // отображаем только последний шаг графически, т.к. он будет результатом компоновки
        }

        private void Button_StartStepComposition_Click(object sender, RoutedEventArgs e)
        {
            TextBox_Log.Text = "";
            StackPanel_Boards.Children.Clear();

            StepsLog = DoComposition();

            Button_FullComposition.IsEnabled = false;
            Button_StartStepComposition.IsEnabled = false;
            Button_NextStep.IsEnabled = true;
            Button_DropStepMode.IsEnabled = true;

            CurrentStep = 0;

            ShowStep(CurrentStep);
            TextBox_Log.Text += StepsLog[CurrentStep].Message + "\n";
        }

        private void Button_NextStep_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentStep+1 >= StepsLog.Count)
            {
                DropStepMode();
            } else
            {
                CurrentStep++;
                ShowStep(CurrentStep);
                TextBox_Log.Text += StepsLog[CurrentStep].Message + "\n";
            }
        }

        private void ShowStep(int StepNumber)
        {
            StackPanel_Boards.Children.Clear();
            var OneStep = StepsLog[StepNumber];
            for (int i = 0; i < OneStep.BoardsList.Count; i++)
            {
                var sp = new StackPanel();
                sp.Orientation = Orientation.Vertical;
                sp.Margin = new Thickness(5);

                var tb = new TextBlock();
                tb.Margin = new Thickness(5);
                tb.Text = $"Узел №{i + 1}:";

                sp.Children.Add(tb);

                var lw = new ListView();
                lw.Margin = new Thickness(5);
                lw.ItemsSource = OneStep.BoardsList[i];

                sp.Children.Add(lw);

                StackPanel_Boards.Children.Add(sp);

            }
            
        }

        private void Button_DropStepMode_Click(object sender, RoutedEventArgs e)
        {
            DropStepMode();
        }

        private void DropStepMode()
        {
            Button_FullComposition.IsEnabled = true;
            Button_StartStepComposition.IsEnabled = true;
            Button_NextStep.IsEnabled = false;
            Button_DropStepMode.IsEnabled = false;
        }
    }
}
