using System.Windows;

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
