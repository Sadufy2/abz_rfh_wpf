using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirmwareUploader
{
    public class ScannedDrone
    {
        public string IP { get; set; }
        public string Name { get; set; }

        public ScannedDrone(string ip, string name)
        {
            this.IP = ip;
            this.Name = name;
        }
    }
    public class SelectableDrone : ScannedDrone
    {
        public bool IsSelected { get; set; }

        public SelectableDrone(string ip, string name) : base(ip, name) { }
    }
}
