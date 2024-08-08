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
using FirmwareUploader.Services;

namespace FirmwareUploader
{
    public partial class DroneConfig : Window
    {
        ConfigService configService;
        public DroneConfig(string ipAddress)
        {
            InitializeComponent();
            configService = new ConfigService(ipAddress);
            LoadConfig();
        }
        private async void LoadConfig()
        {
            try
            {
                string config = await configService.GetConfig();
                configText.Text = config;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await configService.PutConfig(configText.Text);
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
