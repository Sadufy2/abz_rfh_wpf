using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirmwareUploader.Models
{
    public class DroneLog: INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int Size { get; set; }
        public long UnixTimeStamp { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public DroneLog(int id, int size, long unixTimeStamp)
        {
            Id = id;
            Size = size;
            UnixTimeStamp = unixTimeStamp;
            IsSelected = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
