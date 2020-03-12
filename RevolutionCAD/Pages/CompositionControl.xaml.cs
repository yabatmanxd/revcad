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
        public MainWindow mw;

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
            
            string err_msg = "";
            switch (ComboBox_Method.SelectedIndex)
            {
                case 0:
                    if (int.TryParse(tbCountOfElements.Text, out countOfElements) == false ||
                    int.TryParse(tbLimitsOfWires.Text, out limitsOfWires) == false)
                    {
                        err_msg = "Поля ввода заполнены неверно";
                        break;
                    }
                    steps = PosledGypergraph.Compose(countOfElements, limitsOfWires, out err_msg);
                    break;
                case 1:
                    if (int.TryParse(tbCountOfElements.Text, out countOfElements) == false ||
                        int.TryParse(tbLimitsOfWires.Text, out limitsOfWires) == false)
                    {
                        err_msg = "Поля ввода заполнены неверно";
                        break;
                    }
                    steps = PosledMultigraph.Compose(countOfElements, limitsOfWires, out err_msg);
                    break;
                case 2:
                    steps = IterGypergraph.Compose(out err_msg);
                    break;
                case 3:
                    steps = IterMultigraph.Compose(out err_msg);
                    break;
            }
            
            // если не было ошибки - сериализуем результат
            if (err_msg == "")
            {
                if (steps.Count != 0)
                {
                    var result = new CompositionResult();
                    result.BoardsElements = steps.Last().BoardsList;

                    var sch = ApplicationData.ReadScheme(out err_msg);

                    if (err_msg != "")
                    {
                        MessageBox.Show(err_msg, "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }

                    result.CreateBoardsWires(sch, out err_msg);

                    if (err_msg != "")
                    {
                        MessageBox.Show(err_msg, "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }

                    // формируем новую матрицу R на основе сформированных проводов, которые были разделены по платам
                    result.MatrixR_AfterComposition = ApplicationData.CreateMatrixR(result.BoardsWires, sch.MatrixR.RowsCount, sch.MatrixR.ColsCount);
                    
                    ApplicationData.WriteComposition(result, out err_msg);

                    if (err_msg != "")
                    {
                        MessageBox.Show(err_msg, "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }

                    mw.MatrControl.UpdateMatrices();

                }
            }
            if (err_msg != "")
                MessageBox.Show(err_msg, "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
            return steps;

        }

        private void Button_FullComposition_Click(object sender, RoutedEventArgs e)
        {
            TextBox_Log.Text = "";
            StepsLog = DoComposition();
            if (StepsLog.Count == 0)
            {
                MessageBox.Show("Метод компоновки не сработал", "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            for (int step = 0; step < StepsLog.Count; step++)
            {
                TextBox_Log.Text += $"Шаг №{step + 1}:" + "\n";
                TextBox_Log.Text += StepsLog[step].Message + "\n";
            }
            TextBox_Log.ScrollToEnd();
            ShowStep(StepsLog.Count - 1); // отображаем только последний шаг графически, т.к. он будет результатом компоновки

            string str;
            var cmp = ApplicationData.ReadComposition(out str);
            if (str != "")
                return;
            ShowWires(cmp);
        }

        private void Button_StartStepComposition_Click(object sender, RoutedEventArgs e)
        {
            TextBox_Log.Text = "";
            StackPanel_Boards.Children.Clear();

            StepsLog = DoComposition();
            
            if (StepsLog.Count == 0)
            {
                MessageBox.Show("Метод компоновки не сработал", "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
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
            TextBox_Log.ScrollToEnd();
            ShowStep(CurrentStep);
            if (CurrentStep + 1 >= StepsLog.Count)
            {
                TextBox_Log.Text += "\n === Компоновка закончена ===\n";

                Button_FullComposition.IsEnabled = true;
                Button_StartStepComposition.IsEnabled = true;
                Button_NextStep.IsEnabled = false;
                Button_DropStepMode.IsEnabled = false;

                string str;
                var cmp = ApplicationData.ReadComposition(out str);
                if (str != "")
                    return;
                ShowWires(cmp);
            }
            else
            {
                CurrentStep++;
            }
        }

        private void ShowStep(int StepNumber)
        {
            var OneStep = StepsLog[StepNumber];

            Draw(OneStep.BoardsList);
        }

        private void Draw(List<List<int>> boardsList)
        {
            StackPanel_Boards.Children.Clear();
            for (int i = 0; i < boardsList.Count; i++)
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
                lw.ItemsSource = boardsList[i];

                sp.Children.Add(lw);

                StackPanel_Boards.Children.Add(sp);

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
            ShowStep(StepsLog.Count - 1); // отображаем только последний шаг графически, т.к. он будет результатом компоновки

            string str;
            var cmp = ApplicationData.ReadComposition(out str);
            if (str != "")
                return;
            ShowWires(cmp);

            CurrentStep = 0;
            Button_FullComposition.IsEnabled = true;
            Button_StartStepComposition.IsEnabled = true;
            Button_NextStep.IsEnabled = false;
            Button_DropStepMode.IsEnabled = false;
        }
        
        public void Update()
        {
            string t = "";
            var cmp = ApplicationData.ReadComposition(out t);
            if (t == "")
            {
                var boardsElements = cmp.BoardsElements;
                Draw(boardsElements);
            } else
            {
                TextBox_Log.Text = "";

                Button_FullComposition.IsEnabled = true;
                Button_StartStepComposition.IsEnabled = true;
                Button_NextStep.IsEnabled = false;
                Button_DropStepMode.IsEnabled = false;

                StackPanel_Boards.Children.Clear();
            }
        }

        private void ShowWires(CompositionResult cmp)
        {
            TreeViewItem_Nodes.Items.Clear();

            for (int boardNum = 0; boardNum<cmp.BoardsElements.Count; boardNum++)
            {
                var boardItem = new TreeViewItem();
                boardItem.Header = $"Узел {boardNum + 1}";

                var boardElements = new TreeViewItem();
                boardElements.Header = "Элементы";
                foreach(var element in cmp.BoardsElements[boardNum])
                {
                    var boardElement = new TreeViewItem();
                    if (element != 0)
                        boardElement.Header = $"D{element}";
                    else
                        boardElement.Header = "X";
                    boardElements.Items.Add(boardElement);
                }

                boardElements.IsExpanded = true;
                boardItem.Items.Add(boardElements);


                var boardWires = new TreeViewItem();
                boardWires.Header = "Провода";
                int wireIterator = 1;
                foreach (var wire in cmp.BoardsWires[boardNum])
                {
                    var itemWire = new TreeViewItem();
                    itemWire.Header = $"Провод {wireIterator}";
                    foreach (var contact in wire)
                    {
                        var itemContact = new TreeViewItem();
                        if (contact.ElementNumber != 0)
                            itemContact.Header = $"D{contact.ElementNumber}.{contact.ElementContact}";
                        else
                            itemContact.Header = "X";
                        itemWire.Items.Add(itemContact);
                    }
                    boardWires.Items.Add(itemWire);
                    wireIterator++;
                }

                boardWires.IsExpanded = true;
                boardItem.Items.Add(boardWires);


                TreeViewItem_Nodes.Items.Add(boardItem);
            }

        }
    }
}
