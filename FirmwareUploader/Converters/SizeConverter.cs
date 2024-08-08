using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FirmwareUploader.Converters
{
    public class SizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int size)
            {
                if (size < 1024)
                    return $"{size} B";
                else if (size < 1024 * 1024)
                    return $"{size / 1024.0:F2} KB";
                else if (size < 1024 * 1024 * 1024)
                    return $"{size / (1024.0 * 1024.0):F2} MB";
                else
                    return $"{size / (1024.0 * 1024.0 * 1024.0):F2} GB";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
