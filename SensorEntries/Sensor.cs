using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorEntries
{
    public class Sensor
    {
        public int id { get; set; }
        public double temperature { get; set; }

        public double humidity { get; set; }
        public int battery { get; set; }

        public int timestamp { get; set; }


    }
}
