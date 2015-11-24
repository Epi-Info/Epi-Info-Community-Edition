using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using EpiDashboard.Mapping.ShapeFileReader;

namespace EpiDashboard.Mapping
{
    public class ChoroplethLayerProvider
    {
        private bool _rangesLoadedFromMapFile;

        public bool RangesLoadedFromMapFile
        {
            get { return _rangesLoadedFromMapFile; }
            set { _rangesLoadedFromMapFile = value; }
        }

        public List<double> RangeStartsFromMapFile { get; set; }
        
        public struct ThematicItem
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string CalcField { get; set; }
            public double Min { get; set; }
            public double Max { get; set; }
            public string MinName { get; set; }
            public string MaxName { get; set; }
            public List<double> RangeStarts { get; set; }
        }

        public List<double> CalculateThematicRange(int classCount, ThematicItem thematicItem, List<double> valueList)
        {
            List<double> rangeStarts = new List<double>();

            if (RangesLoadedFromMapFile && RangeStartsFromMapFile != null)
            {
                rangeStarts = this.RangeStartsFromMapFile;  
            }
            else
            {
                double totalRange = thematicItem.Max - thematicItem.Min;
                double portion = totalRange / classCount;
                rangeStarts.Add(thematicItem.Min);
                double startRangeValue = thematicItem.Min;
                
                IEnumerable<double> valueEnumerator =
                    from aValue in valueList
                    orderby aValue
                    select aValue;

                double increment = (double)valueList.Count / (double)classCount;

                for (double i = increment - 1.0; Math.Round(i, 4) < valueList.Count - 1; i += increment)
                {
                    double value0 = valueEnumerator.ElementAt(Convert.ToInt32(Math.Floor(i)));
                    double value1 = valueEnumerator.ElementAt(Convert.ToInt32(Math.Ceiling(i)));
                    double value = (value1 + value0) / 2.0;
                    
                    if (value < thematicItem.Min)
                    { 
                        value = thematicItem.Min;
                    }

                    rangeStarts.Add(value);
                }
            }

            return rangeStarts;
        }
    }
}
