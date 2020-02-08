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
            TabControl_Main.IsEnabled = false;
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
                TabControl_Main.IsEnabled = true;
                TextBox_Code.Text = "// пишите код здесь";
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
                TabControl_Main.IsEnabled = true;
                TextBox_Code.Text = File.ReadAllText(fullPath);
                TextBox_Code.SelectionStart = TextBox_Code.Text.Length;
            }
        }

        private void MenuItem_Save_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationData.FileName != "")
            {
                string writePath = $"{Environment.CurrentDirectory}\\{ApplicationData.FileName}.sch";
                try
                {
                    using (StreamWriter sw = new StreamWriter(writePath, false, System.Text.Encoding.Unicode))
                    {
                        sw.WriteLine(TextBox_Code.Text);
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show($"При записи в файл произошла ошибка: {exc.Message}", "Revolution CAD");
                }
            }
            
        }

        private void Button_CreateMatrices_Click(object sender, RoutedEventArgs e)
        {
            // Дмитрий Николаев, в этом месте мог бы быть ваш парсер
            // формирование матриц здесь же
        }
    }
}
