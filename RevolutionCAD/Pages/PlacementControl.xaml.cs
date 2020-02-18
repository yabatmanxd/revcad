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

        }

        private void Button_NextStep_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_StartStepPlacement_Click(object sender, RoutedEventArgs e)
        {
            TextBox_Log.Text = "";
            StackPanel_Boards.Children.Clear();




            Button_FullPlacement.IsEnabled = false;
            Button_StartStepPlacement.IsEnabled = false;
            Button_NextStep.IsEnabled = true;
            Button_DropStepMode.IsEnabled = true;

            CurrentStep = 0;

            Button_NextStep_Click(null, null);
        }

        private void Button_FullPlacement_Click(object sender, RoutedEventArgs e)
        {
            StepsLog = DoPlacement();
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
