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
            StepsLog = DoComposition();
        }

        private void Button_StartStepComposition_Click(object sender, RoutedEventArgs e)
        {
            StepsLog = DoComposition();
            Button_FullComposition.IsEnabled = false;
            Button_StartStepComposition.IsEnabled = false;
        }

        private void Button_NextStep_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
