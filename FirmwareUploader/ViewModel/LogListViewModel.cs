using FirmwareUploader.Models;
using FirmwareUploader.Services;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace FirmwareUploader.ViewModel
{
    public class LogListViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<DroneLog> DroneLogs { get; set; }

        private bool isEnabled;
        private string _textToDisplay;
        public string TextToDisplay
        {
            get { return _textToDisplay; }
            set
            {
                if (_textToDisplay != value)
                {
                    _textToDisplay = value;
                    OnPropertyChanged();
                }
            }
        }

        //private string IpAddress;
        //private LogService logService;
        public ICommand GetLogsCommand { get; }
        public ICommand SelectAllCommand { get; }

        public LogListViewModel()
        {
            //DroneLogs = new ObservableCollection<DroneLog>();
            //logService = new LogService();
            //IpAddress = "192.168.1.27";
            //isEnabled = true;

            //GetLogsCommand = new RelayCommand(ExecuteGetLogs, CanExecute);
            //SelectAllCommand = new RelayCommand(ExecuteSelectAll, TrueExecute);
            //MockData();
            //RefreshLogs();
        }

        private async void ExecuteGetLogs(object parameter)
        {
            isEnabled = false;
            //isEnabled = await RefreshLogs();
        }
        private bool CanExecute(object parameter)
        {
            return isEnabled;
        }

        private void ExecuteSelectAll(object parameter)
        {
           

        }
        private bool TrueExecute(object parameter)
        {
            return true;
        }
        
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
