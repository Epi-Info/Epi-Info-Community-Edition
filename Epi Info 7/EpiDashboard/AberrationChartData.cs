using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpiDashboard
{
    public class AberrationChartData
    {
        public DateTime Date { get; set; }
        public double Actual { get; set; }
        public double? Expected { get; set; }
        public double? Aberration { get; set; }
    }
}
