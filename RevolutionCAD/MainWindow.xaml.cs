using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Windows.Navigation;
using System.Linq;


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
                TextBox_Code.Text = "D1 dip14\r\nD2 dip14\r\nD3 dip14\r\nD4 dip18\r\n#\r\nD1.1-D2.1-D3.2-X.1\r\nD4.1-D2.1\r\nD3.1-D1.1";
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
                TextBox_Code.Text = File.ReadAllText(fullPath);
                TextBox_Code.SelectionStart = TextBox_Code.Text.Length;
            }
        }

        private void MenuItem_Save_Click(object sender, RoutedEventArgs e)
        {
            string error;
            ApplicationData.WriteScheme(TextBox_Code.Text, out error);
            if (error != "")
                MessageBox.Show(error, "Revolution CAD");
            
        }

        private void Button_CreateMatrices_Click(object sender, RoutedEventArgs e)
        {
            // сохраняем схему
            MenuItem_Save_Click(null, null);

            

            MatrControl.UpdateMatrices();
            TabControl_Main.SelectedIndex = 1;

        }
    }
}
