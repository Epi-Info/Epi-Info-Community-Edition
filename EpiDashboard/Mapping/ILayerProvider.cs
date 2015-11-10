using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace EpiDashboard.Mapping
{
    public interface ILayerProvider
    {
        void CloseLayer();
        void MoveUp();
        void MoveDown();
        //CustomColorsDictionary CustomColorsDictionary { get; set; }
        //byte Opacity { get; set; }
        //ClassRangesDictionary ClassRangesDictionary { get; }        
        //void PopulateRangeValues(DashboardHelper dashboardHelper, string toString, string s, string toString1, List<SolidColorBrush> brushList, int classCount, string text);
    }
}
