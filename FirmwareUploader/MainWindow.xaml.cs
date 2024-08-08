using FirmwareUploader.Services;
using Microsoft.Win32;
using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FirmwareUploader
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnSharedBoolChanged();
                    OnPropertyChanged();
                }
            }
        }
        private void OnSharedBoolChanged()
        {
            if (_listLogWindow != null)
            {
                _listLogWindow.UpdateSharedBool(IsLoading);
            }
        }
        public void UpdateSharedBool(bool value)
        {
            IsLoading = value;
        }

        private LogList _listLogWindow;
        private DroneConfig _droneConfigWindow;

        ObservableCollection<ScannedDrone> scannedDrones;
        DroneSearchService droneSearch;
        LogService logService;

        public MainWindow()
        {
            InitializeComponent();
            scannedDrones = new ObservableCollection<ScannedDrone>();
            droneSearch = new DroneSearchService();
            logService = new LogService();

            DataContext = this;
        }
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "APJ files (*.apj)|*.apj";
            if (openFileDialog.ShowDialog() == true)
            {
                FilePathTextBox.Text = openFileDialog.FileName;
            }
        }

        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            IsLoading = true;
            string filePath = FilePathTextBox.Text;
            string ipAddress = IpAddressTextBox.Text;
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                UpdateStatus("Please select a valid file to upload.", Colors.Black);
                IsLoading = false;
                return;
            }
            CancellationTokenSource cts = new CancellationTokenSource();
            var animationTask = Task.Run(() => LoadingAnimation("Processing", cts.Token));
            try
            {
                await UploadService.UploadFileAsync(filePath, ipAddress);
                cts.Cancel();
                await animationTask;
                UpdateStatus("Firmware Updated", Colors.Green);
            }
            catch (Exception ex)
            {
                cts.Cancel();
                await animationTask;
                UpdateStatus("Http Response Error: " + ex.Message, Colors.Red);
            }
            finally
            {
                IsLoading = false;
            }
        }



        private void UpdateStatus(string message, Color color)
        {
            Dispatcher.Invoke(() =>
            {
                UploadStatusText.Text = message;
                UploadStatusText.Foreground = new SolidColorBrush(color);
            });
        }
        private void UpdateStatus(string message = "")
        {
            Dispatcher.Invoke(() =>
            {
                UploadStatusText.Text = message;
                UploadStatusText.Foreground = new SolidColorBrush(Colors.Black);
            });
        }

        private async void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            var animationTask = Task.Run(() => LoadingAnimation("Scanning", cts.Token));
            try
            {
                scannedDrones = new ObservableCollection<ScannedDrone>(await droneSearch.StartScanning());
                RefreshComboBox();
                cts.Cancel();
                await animationTask;
            }
            catch (Exception ex)
            {
                cts.Cancel();
                await animationTask;
                scannedDrones.Clear();
                UpdateStatus(ex.Message, Colors.Red);
                RefreshComboBox();
            }
        }
        void LoadingAnimation(string message, CancellationToken token)
        {
            string[] animation = new[] { "", ".", "..", "..." };
            int i = 0;
            while (!token.IsCancellationRequested)
            {
                i = (i + 1) % animation.Length;
                UpdateStatus(message + animation[i]);
                Thread.Sleep(500);
            }
        }
        private void RefreshComboBox()
        {
            IsLoading = true;
            droneComboBox.SelectedItem = null;
            droneComboBox.Items.Clear();
            if (scannedDrones.Count() != 0)
            {
                foreach (var drone in scannedDrones)
                {
                    droneComboBox.Items.Add(drone);
                }
                droneComboBox.SelectedItem = scannedDrones.FirstOrDefault();
                UpdateStatus($"{scannedDrones.Count()} Device was found", Colors.Green);
            }
            else
            {
                UpdateStatus("No device was found");
            }
            IsLoading = false;
        }

        private void scanBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (droneComboBox.SelectedItem is ScannedDrone)
            {
                IpAddressTextBox.Text = (droneComboBox.SelectedItem as ScannedDrone).IP;
            }
            else
            {
                IpAddressTextBox.Text = string.Empty;
            }
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            scannedDrones.Add(new ScannedDrone("1.1.1.1", "drone1"));
            //droneNames.Add(("192.168.4.21", "Drone2"));
        }

        private async void LastLogButton_Click(object sender, RoutedEventArgs e)
        {
            IsLoading = true;
            CancellationTokenSource cts = new CancellationTokenSource();
            var animationTask = Task.Run(() => LoadingAnimation("Getting Last Log", cts.Token));
            string ipAddress = IpAddressTextBox.Text;
            try
            {
                await logService.DownloadLastLog(ipAddress);
                cts.Cancel();
                await animationTask;
                UpdateStatus("Log Downloaded!", Colors.Green);
            }
            catch (Exception ex)
            {
                cts.Cancel();
                await animationTask;
                UpdateStatus(ex.Message, Colors.Red);
            }
            finally
            {
                IsLoading = false;
            }
        }
        private void OpenLogWindow_Click(object sender, RoutedEventArgs e)
        {
            string ipAddress = IpAddressTextBox.Text;
            if (ipAddress != null && ipAddress != "")
            {
                if (_listLogWindow == null || !_listLogWindow.IsLoaded)
                {
                    _listLogWindow = new LogList(ipAddress, this);
                    _listLogWindow.Closed += (s, args) => _listLogWindow = null;
                    _listLogWindow.Show();
                }
                else
                {
                    _listLogWindow.Activate();
                }
            }
        }
        private void OpenConfigWindow_Click(object sender, RoutedEventArgs e)
        {
            string ipAddress = IpAddressTextBox.Text;
            if (ipAddress != null && ipAddress != "")
            {
                if (_droneConfigWindow == null || !_droneConfigWindow.IsLoaded)
                {
                    _droneConfigWindow = new DroneConfig(ipAddress);
                    _droneConfigWindow.Closed += (s, args) => _droneConfigWindow = null;
                    _droneConfigWindow.Show();
                }
                else
                {
                    _droneConfigWindow.Activate();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}