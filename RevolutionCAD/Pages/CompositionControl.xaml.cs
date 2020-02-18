using Newtonsoft.Json;
using RevolutionCAD.Composition;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Логика взаимодействия для CompositionControl.xaml
    /// </summary>
    public partial class CompositionControl : UserControl
    {
        List<StepCompositionLog> StepsLog;

        public int CurrentStep { get; set; }

        public CompositionControl()
        {
            InitializeComponent();
        }

        private List<StepCompositionLog> DoComposition()
        {
            var steps = new List<StepCompositionLog>();

            int countOfElements, limitsOfWires; // Николаев

            string msg = "";
            if (int.TryParse(tbCountOfElements.Text, out countOfElements) == false || 
                    int.TryParse(tbLimitsOfWires.Text, out limitsOfWires) == false)
            {
                msg = "Вы написали какие-то бредни вместо цифр в полях для ввода";
            }
            else
            switch (ComboBox_Method.SelectedIndex)
            {
                case 0:
                    if (ApplicationData.IsFileExists(".q", out msg))
                        steps = PosledGypergraph.Compose(countOfElements, limitsOfWires);
                    break;
                case 1:
                    if (ApplicationData.IsFileExists(".q", out msg))
                        steps = PosledMultigraph.Compose(countOfElements, limitsOfWires);
                    break;
                case 2:
                    if (ApplicationData.IsFileExists(".cmp", out msg) && ApplicationData.IsFileExists(".r", out msg))
                        steps = IterGypergraph.Compose();
                    break;
                case 3:
                    if (ApplicationData.IsFileExists(".cmp", out msg) && ApplicationData.IsFileExists(".r", out msg))
                        steps = IterMultigraph.Compose();
                    break;

            }
            // если была ошибка - сообщаем
            if (msg != "")
                MessageBox.Show(msg, "Revolution CAD");
            else
            {
                // сериализуем результат
                // формирование файла компоновки *.cmp
                using (StreamWriter file = File.CreateText(ApplicationData.FileName + ".cmp"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, steps.Last().BoardsList);
                }
            }
            

            return steps;

        }

        private void Button_FullComposition_Click(object sender, RoutedEventArgs e)
        {
            TextBox_Log.Text = "";
            StackPanel_Boards.Children.Clear();
            StepsLog = DoComposition();
            if (StepsLog.Count == 0)
            {
                MessageBox.Show("Метод компоновки не сработал", "Revolution CAD");
                return;
            }
            for (int step = 0; step < StepsLog.Count; step++)
            {
                TextBox_Log.Text += $"Шаг №{step + 1}:" + "\n";
                TextBox_Log.Text += StepsLog[step].Message + "\n";
            }
            ShowStep(StepsLog.Count - 1); // отображаем только последний шаг графически, т.к. он будет результатом компоновки
            
        }

        private void Button_StartStepComposition_Click(object sender, RoutedEventArgs e)
        {
            TextBox_Log.Text = "";
            StackPanel_Boards.Children.Clear();

            StepsLog = DoComposition();
            
            if (StepsLog.Count == 0)
            {
                MessageBox.Show("Метод компоновки не сработал", "Revolution CAD");
                return;
            }
            
            Button_FullComposition.IsEnabled = false;
            Button_StartStepComposition.IsEnabled = false;
            Button_NextStep.IsEnabled = true;
            Button_DropStepMode.IsEnabled = true;

            CurrentStep = 0;

            Button_NextStep_Click(null, null);
        }

        private void Button_NextStep_Click(object sender, RoutedEventArgs e)
        {
            TextBox_Log.Text += $"Шаг №{CurrentStep + 1}:" + "\n";
            TextBox_Log.Text += StepsLog[CurrentStep].Message + "\n";
            ShowStep(CurrentStep);
            if (CurrentStep + 1 >= StepsLog.Count)
            {
                DropStepMode();
            }
            else
            {
                CurrentStep++;
            }
        }

        private void ShowStep(int StepNumber)
        {
            TextBox_Log.ScrollToEnd();

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
