using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;

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
            CompControl.mw = this;
            PlaceControl.mw = this;
            TraceControl.mw = this;
            LayerControl.mw = this;

            
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
                TextBlock_NameOpenedFile.Text = $"Файл: {wnd.Text}";
                TabControl_Main.Visibility = Visibility.Visible;
                // при создании файла откроется пример заполнения
                TextBox_Code.Text = "dip14\r\ndip14\r\ndip14\r\ndip18\r\n#\r\nX-D1.1\r\nD1.1-D2.1\r\nD1.2-D2.2\r\nD2.2-D3.2\r\nD3.2-X\r\nD4.1-D2.1\r\nD3.1-D1.1\r\n";
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
                TextBlock_NameOpenedFile.Text = $"Файл: {fileName}";
                TabControl_Main.Visibility = Visibility.Visible;

                string msg;
                var sch = ApplicationData.ReadScheme(out msg);
                if (msg != "")
                {
                    MessageBox.Show(msg, "Revolution CAD", MessageBoxButton.OK,MessageBoxImage.Error);
                    return;
                }

                TextBox_Code.Text = sch.SchemeDefinition;

                ShowWires(sch);

                UpdatePages();

                TextBox_Code.SelectionStart = TextBox_Code.Text.Length;
            }
        }

        private void MenuItem_Save_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationData.FileName != "")
            {
                string error = "";
                ApplicationData.WriteScheme(TextBox_Code.Text, out error);
                if (error != "")
                {
                    MessageBox.Show(error, "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    string str;
                    var sch = ApplicationData.ReadScheme(out str);
                    if (str != "")
                        return;
                    ShowWires(sch);
                }
                UpdatePages();
            }
            
            
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
            
            string str;
            var sch = ApplicationData.ReadScheme(out str);
            if (str != "")
                return;
            ShowWires(sch);
            UpdatePages();

        }

        private void MenuItem_Info_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Революционная система автоматического проектирования\nРазработана студентами группы КС-16 ФКНТ ГОУВПО \"ДонНТУ\" 2020г.\nРуководитель проекта: Николаев Дмитрий\nВедущий разработчик: Стамбула Дмитрий\nДизайн страниц: Стамбула Дмитрий, Алутин Евгений\nВсе права защищены единым арестантским уставом (АУЕ)\nОсновные положения устава изложены в Жёлтой Книге (или Оранжевой Dostlev Edition)", "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void UpdatePages()
        {
            MatrControl.UpdateMatrices();
            CompControl.Update();
            PlaceControl.Update();
            TraceControl.Update();
            LayerControl.Update();
        }

        private void ShowWires(Scheme sch)
        {
            TreeViewItem_Wires.Items.Clear();
            int wireIterator = 1;
            foreach (var wire in sch.WiresContacts)
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
                TreeViewItem_Wires.Items.Add(itemWire);
                wireIterator++;
            }
            TreeViewItem_Wires.IsExpanded = true;


            TreeViewItem_Dips.Items.Clear();
            int dipIterator = 0;
            foreach (var dip in sch.DIPNumbers)
            {
                if (dipIterator != 0)
                {
                    var dipItem = new TreeViewItem();
                    dipItem.Header = $"D{dipIterator} - dip{dip}";
                    TreeViewItem_Dips.Items.Add(dipItem);
                }
                dipIterator++;
            }
            TreeViewItem_Dips.IsExpanded = true;
        }

        private void MenuItem_Settings_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new SettingsWindow();
            wnd.ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using (StreamReader file = File.OpenText("app.settings"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    Settings settings = (Settings)serializer.Deserialize(file, typeof(Settings));
                    ApplicationData.PinDistance = settings.ContactsDist;
                    ApplicationData.RowDistance = settings.RowsDist;
                    ApplicationData.ElementsDistance = settings.ElementsDist;
                }
            }
            catch
            {
                ApplicationData.PinDistance = 1;
                ApplicationData.RowDistance = 3;
                ApplicationData.ElementsDistance = 4;
            }
        }

        private void MenuItem_AboutMethods_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"Help\Metodicheskie_ukazania.docx");
        }
    }
}
