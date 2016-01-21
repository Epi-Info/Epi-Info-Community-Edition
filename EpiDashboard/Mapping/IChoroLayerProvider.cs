using System;//88
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace EpiDashboard.Mapping
{
    public interface IChoroLayerProvider : ILayerProvider
    {
        byte Opacity { get; set; }

        CustomColorsDictionary CustomColorsDictionary { get; }
        ClassRangeDictionary ClassRangesDictionary { get; }

        string PopulateRangeValues();
        string PopulateRangeValues(DashboardHelper dashboardHelper, string toString, string s, string toString1, List<SolidColorBrush> brushList, int classCount, string text);

        string[,] RangeValues { get; set; }

        bool UseCustomColors { get; set; }
        StackPanel LegendStackPanel { get; set; }
        int RangeCount { get; set; }
        List<double> RangeStarts_FromControls { get; set; }
        ListLegendTextDictionary ListLegendText { get; set; }
        bool RangesLoadedFromMapFile { get; set; }
        bool UseQuantiles { get; set; }
        Guid LayerId { get; set; }
        void SetShapeRangeValues(DashboardHelper dashboardHelper, string toString, string s, string toString1, List<SolidColorBrush> brushList, int classCount, string missingText, string legendText);
        void ResetRangeValues(string toString, string s, string toString1, int classCount);
        object[] Load();
        object[] Load(string boundrySourceLocation);
        string LegendText { get; set; }

    }
}
















