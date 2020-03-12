using Newtonsoft.Json;
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
using System.Windows.Shapes;

namespace RevolutionCAD
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            NumericSelect_PinDistance.Value = ApplicationData.PinDistance;
            NumericSelect_RowDistance.Value = ApplicationData.RowDistance;
            NumericSelect_ElemenDistance.Value = ApplicationData.ElementsDistance;
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            var settings = new Settings(NumericSelect_PinDistance.Value, NumericSelect_RowDistance.Value, NumericSelect_ElemenDistance.Value);
            try
            {
                using (StreamWriter file = File.CreateText("app.settings"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, settings);
                    ApplicationData.PinDistance = settings.ContactsDist;
                    ApplicationData.RowDistance = settings.RowsDist;
                    ApplicationData.ElementsDistance = settings.ElementsDist;
                }
            }
            catch
            {
                MessageBox.Show("При сохранении произошла ошибка", "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            MessageBox.Show("Значения успешно сохранены, необходимо выполнить этап РАЗМЕЩЕНИЯ для переформирования ДРП", "Revolution CAD", MessageBoxButton.OK, MessageBoxImage.Information);
        }



    }
}
