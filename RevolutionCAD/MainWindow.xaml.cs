using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;


namespace RevolutionCAD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItem_Create_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new TextInputWindow("Введите название файла:");
            wnd.Owner = this;
            if (wnd.ShowDialog() == true)
            {
                ApplicationData.FileName = wnd.Text;
                TextBlock_NameOpenedFile.Text = wnd.Text;
                TabControl_Main.Visibility = Visibility.Visible;
                // при создании файла откроется пример заполнения
                TextBox_Code.Text = "dip14\r\ndip14\r\ndip14\r\ndip18\r\n#\r\nX-D1.1-D2.1\r\nD1.1-D2.1-D3.2-X\r\nD4.1-D2.1\r\nD3.1-D1.1";
                TextBox_Code.SelectionStart = TextBox_Code.Text.Length;
            }
            
        }

        private void MenuItem_Open_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new OpenFileDialog();
            wnd.InitialDirectory = Environment.CurrentDirectory;
            wnd.Multiselect = false;
            wnd.Filter = "Revolution CAD files (*.sch)|*.sch";

            if (wnd.ShowDialog() == true)
            {
                string fullPath = wnd.FileName;
                string fileName = Path.GetFileNameWithoutExtension(fullPath);

                ApplicationData.FileName = fileName;
                TextBlock_NameOpenedFile.Text = fileName;
                TabControl_Main.Visibility = Visibility.Visible;

                string msg;
                var sch = ApplicationData.ReadScheme(out msg);
                if (msg != "")
                {
                    MessageBox.Show(msg, "Revolution CAD", MessageBoxButton.OK,MessageBoxImage.Error);
                    return;
                }

                TextBox_Code.Text = sch.SchemeDefinition;

                MatrControl.UpdateMatrices();
                CompControl.Update();
                PlaceControl.Update();
                TraceControl.Update();

                TextBox_Code.SelectionStart = TextBox_Code.Text.Length;
            }
        }

        private void MenuItem_Save_Click(object sender, RoutedEventArgs e)
        {
            string error = "";
            ApplicationData.WriteScheme(TextBox_Code.Text, out error);
            if (error != "")
                MessageBox.Show(error, "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
            
        }

        private void Button_CreateMatrices_Click(object sender, RoutedEventArgs e)
        {
            // сохраняем схему
            string error = "";
            ApplicationData.WriteScheme(TextBox_Code.Text, out error);
            if (error != "")
            {
                MessageBox.Show(error, "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
               
            MatrControl.UpdateMatrices();
            TabControl_Main.SelectedIndex = 1;

        }

        private void MenuItem_Info_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Все жалобы, вопросы, предложения по поводу работы приложения в деканат", "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
