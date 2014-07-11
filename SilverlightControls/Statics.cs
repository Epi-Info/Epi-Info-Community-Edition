using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SilverlightControls
{
    public class Statics
    {
        /// <summary>
        /// Delimiters
        /// </summary>
        public static string LineSeriesTitle = "LineSeriesTitle";
        public static string LineSeriesDataString = "LineSeriesDataString";
        public static string IndependentValueFormat = "IndependentValueFormat";
        public static string DependentValueFormat = "DependentValueFormat";
        public static string SeparateDataPoints = "^&^";
        public static string SeparateIndDepValues = "^sidv^";
        public static string SeparateLineSeries = ")^(";
        public static string Comma = "::comma::";

        public static string areaSeries = "areaSeries";
        public static string barSeries = "barSeries";
        public static string bubbleSeries = "bubbleSeries";
        public static string columnSeries = "columnSeries";
        public static string lineSeries = "lineSeries";
        public static string pieSeries = "pieSeries";

        /// <summary>
        /// Standard Options
        /// </summary>
        public const string Area = "AREA";
        public const string Bar = "BAR";
        public const string Bubble = "BUBBLE";
        public const string Histogram = "HISTOGRAM";
        public const string Line = "LINE";
        public const string Pie = "PIE";
        public const string RotatedBar = "ROTATEDBAR";
        public const string Scatter = "SCATTER";
        public const string Stacked = "STACKED";
        public const string TreeMap = "TREEMAP";

        /// <summary>
        /// Aggrigate Options
        /// </summary>
        public const string AVG = "AVG";
        public const string COUNT = "COUNT";
        public const string SUM = "SUM";
        public const string MIN = "MIN";
        public const string MAX = "MAX";
        public const string PERCENT = "PERCENT";
        public const string SUMPCT = "SUMPCT";
    }
}
