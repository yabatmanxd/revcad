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
using System.Windows.Shapes;

namespace RevolutionCAD
{
    /// <summary>
    /// Логика взаимодействия для TextInputWindow.xaml
    /// </summary>
    public partial class TextInputWindow : Window
    {
        public string Text
        {
            get
            {
                return TextBox_Text.Text;
            }
        }

        public TextInputWindow(string question)
        {
            InitializeComponent();
            TextBlock_Message.Text = question;
            TextBox_Text.Focus();
        }

        private void Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
