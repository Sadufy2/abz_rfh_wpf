using FirmwareUploader.Models;
using FirmwareUploader.Services;
using FirmwareUploader.ViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace FirmwareUploader
{
    public partial class LogList : Window, INotifyPropertyChanged
    {
        private DispatcherTimer timer;
        int bar_duration = 0;
        int bar_elapsed = 0;

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<DroneLog> _droneLogs;
        public ObservableCollection<DroneLog> DroneLogs
        {
            get => _droneLogs;
            set
            {
                if (_droneLogs != value)
                {
                    _droneLogs = value;
                    OnPropertyChanged();
                }
            }
        }

        private string IpAddress;
        private LogService logService;
        private MainWindow _mainWindow;
        private void ToggleSharedBoolButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.UpdateSharedBool(!_mainWindow.IsLoading);
        }

        public void UpdateSharedBool(bool value)
        {
            Dispatcher.Invoke(() =>
            {
                _mainWindow.UpdateSharedBool(value);
                IsLoading = value;
            });
        }


        public LogList(string ipAddress, MainWindow mainWindow)
        {
            InitializeComponent();
            DataContext = this;
            IpAddress = ipAddress;
            DroneLogs = new ObservableCollection<DroneLog>();
            logService = new LogService();
            _mainWindow = mainWindow;

            RefreshLogs();
        }

        private void UpdateStatus(string message, Color color)
        {
            Dispatcher.Invoke(() =>
            {
                DebugText.Text = message;
                DebugText.Foreground = new SolidColorBrush(color);
            });
        }
        private void UpdateStatus(string message = "")
        {
            Dispatcher.Invoke(() =>
            {
                DebugText.Text = message;
                DebugText.Foreground = new SolidColorBrush(Colors.Black);
            });
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
            UpdateStatus();
        }


        void StartLoadingBar(int size)
        {
            int estimatedTime = (int)Math.Round((double)size / 700000);
            estimatedTime = estimatedTime + 4;
            int i = 0;
            Task.Run(() =>
            {
                while (i < 95 && IsLoading)
                {
                    i++;
                    Thread.Sleep(estimatedTime * 10);
                    UpdateLoadingBar(i, timeConverter(i, estimatedTime) + "s");
                }
                UpdateLoadingBar(i, "Finishing...");
                while (IsLoading) { }
                while (i < 100)
                {
                    i++;
                    Thread.Sleep(10);
                    UpdateLoadingBar(i);
                }
            });
        }
        string timeConverter(int i, int estimatedTime)
        {
            double ret = (double)estimatedTime - ((double)estimatedTime / 100 * i);
            if (60 <= ret)
            {
                return $"{Math.Round((ret / 60), 0)}:{(ret % 60).ToString("F2").PadLeft(5, '0')}"; 
            }
            return ret.ToString("F2");
        }
        void UpdateLoadingBar(int progress, string time = "")
        {
            Dispatcher.Invoke(() =>
            {
                downloadText.Text = time;
                loadingBar.Value = progress;
            });
        }
        

        public async Task<bool> RefreshLogs()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            var animationTask = Task.Run(() => LoadingAnimation("Getting Logs", cts.Token));
            try
            {
                UpdateSharedBool(true);
                _droneLogs.Clear();
                List<DroneLog> logs = await logService.GetLogInfo(IpAddress);
                logs = logs.OrderByDescending(x => x.UnixTimeStamp).ToList();
                UpdateSharedBool(false);

                foreach (DroneLog log in logs)
                {
                    _droneLogs.Add(log);
                }
                cts.Cancel();
                await animationTask;
                return true;
            }
            catch (Exception ex)
            {
                cts.Cancel();
                await animationTask;
                UpdateSharedBool(false);
                UpdateStatus(ex.Message, Colors.Red);
                return false;
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            bool allDisabled = true;
            foreach (var log in _droneLogs)
            {
                if (log.IsSelected)
                {
                    allDisabled = false;
                }
            }
            foreach (var log in _droneLogs)
            {
                log.IsSelected = allDisabled;
            }
        }
        private async void DownloadSelected_Click(object sender, RoutedEventArgs e)
        {
            bool isEmpty = true;
            foreach (var log in DroneLogs)
            {
                if (log.IsSelected)
                {
                    isEmpty = false;
                }
            }
            if (isEmpty)
            {
                UpdateStatus("Please select at least one log");
                return;
            }

            List<int> selectedIds = new List<int>();
            int downloadSize = 0;
            foreach (var log in DroneLogs)
            {
                if (log.IsSelected)
                {
                    downloadSize += log.Size;
                    selectedIds.Add(log.Id);
                }
            }
            CancellationTokenSource cts = new CancellationTokenSource();
            var animationTask = Task.Run(() => LoadingAnimation("Downloading Logs", cts.Token));
            try
            {
                StartLoadingBar(downloadSize);
                UpdateSharedBool(true);
                await logService.DownloadLog(IpAddress, selectedIds);
                cts.Cancel();
                await animationTask;
                UpdateStatus("Download Completed", Colors.Green);
            }
            catch (Exception ex)
            {
                cts.Cancel();
                await animationTask;
                UpdateStatus(ex.Message);
            }
            finally
            {
                UpdateSharedBool(false);
            }
        }
        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await RefreshLogs();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
