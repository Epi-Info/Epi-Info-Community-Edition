using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ComponentArt.Win.DataVisualization.Charting;
using EpiDashboard;
using EpiDashboard.Gadgets.Charting;

namespace EpiDashboard.Controls.Charting
{
    /// <summary>
    /// Interaction logic for PieChart.xaml
    /// </summary>
    public partial class PieChart : PieChartBase
    {
        //public PieChartSettings PieChartSettings { get; set; }

        /// <summary>
        /// The percent at which to show annotations outside of the slice it is associated with.
        /// </summary>
        public double AnnotationPercentCutoff { get; set; }

        public PieChart(DashboardHelper dashboardHelper, PieChartParameters parameters, List<XYColumnChartData> dataList)
        {
            InitializeComponent();
            PieChartParameters = parameters;
            this.DashboardHelper = dashboardHelper;
            SetChartProperties();
            SetChartData(dataList);

            xyChart.Legend.BorderBrush = Brushes.Gray;
        }

        protected override void SetChartProperties()
        {
            xyChart.AnimationOnLoad = false;
            //xyChart.Width = Settings.ChartWidth;
            //xyChart.Height = Settings.ChartHeight;
            //xyChart.Palette = Settings.Palette;

            tblockChartTitle.Text = PieChartParameters.ChartTitle;
            tblockSubTitle.Text = PieChartParameters.ChartSubTitle;
            tblockStrataTitle.Text = PieChartParameters.ChartStrataTitle;

            xyChart.Width = PieChartParameters.ChartWidth;
            xyChart.Height = PieChartParameters.ChartHeight;

            switch (PieChartParameters.Palette)
            {
                case 0:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Atlantic");
                    break;
                case 1:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Breeze");
                    break;
                case 2:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("ComponentArt");
                    break;
                case 3:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Deep");
                    break;
                case 4:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Earth");
                    break;
                case 5:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Evergreen");
                    break;
                case 6:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Heatwave");
                    break;
                case 7:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Montreal");
                    break;
                case 8:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Pastel");
                    break;
                case 9:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Renaissance");
                    break;
                case 10:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("SharePoint");
                    break;
                case 11:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Study");
                    break;
                default:
                case 12:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("VibrantA");
                    break;
                case 13:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("VibrantB");
                    break;
                case 14:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("VibrantC");
                    break;
            }

            if (string.IsNullOrEmpty(tblockChartTitle.Text)) tblockChartTitle.Visibility = System.Windows.Visibility.Collapsed;
            else tblockChartTitle.Visibility = System.Windows.Visibility.Visible;

            if (string.IsNullOrEmpty(tblockSubTitle.Text)) tblockSubTitle.Visibility = System.Windows.Visibility.Collapsed;
            else tblockSubTitle.Visibility = System.Windows.Visibility.Visible;

            if (string.IsNullOrEmpty(tblockStrataTitle.Text)) tblockStrataTitle.Visibility = System.Windows.Visibility.Collapsed;
            else tblockStrataTitle.Visibility = System.Windows.Visibility.Visible;

            series0.ShowPointAnnotations = PieChartParameters.ShowAnnotations;

            AnnotationPercentCutoff = PieChartParameters.AnnotationPercent;

            xyChart.ChartKind = PieChartParameters.PieChartKind;
            xyChart.LegendDock = PieChartParameters.LegendDock;

            xyChart.LegendVisible = PieChartParameters.ShowLegend;
            if (PieChartParameters.ShowLegendBorder == true)
            {
                xyChart.Legend.BorderThickness = new Thickness(1);
            }
            else
            {
                xyChart.Legend.BorderThickness = new Thickness(0);
            }
            tblockXAxisLabel.Text = ((EpiDashboard.GadgetParametersBase)(PieChartParameters)).ColumnNames[0].ToString();//EI-40
            xyChart.Legend.FontSize = PieChartParameters.LegendFontSize;
        }

        protected override void SetChartData(List<XYColumnChartData> dataList)
        {
            series0.DataSource = dataList;
        }

        private void xyChart_DataStructureCreated(object sender, EventArgs e)
        {
            series0.DataPointAnnotations.Clear();

            if (PieChartParameters.ShowAnnotations == true)
            {
                if (PieChartParameters.ShowAnnotationLabel == false && PieChartParameters.ShowAnnotationPercent == false && PieChartParameters.ShowAnnotationValue == false)
                {
                    return;
                }

                string xaml = "<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:dvCommon=\"clr-namespace:ComponentArt.Win.DataVisualization.Common;assembly=ComponentArt.Win.DataVisualization.Common\" "
                 + "xmlns:charting=\"clr-namespace:ComponentArt.Win.DataVisualization.Charting;assembly=ComponentArt.Win.DataVisualization.Charting\" "
                 + "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" "
                 + "xmlns:sys=\"clr-namespace:System;assembly=mscorlib\">"
                    + "<Border Background=\"#70ffcece\" CornerRadius=\"4\" Padding=\"6,3,6,3\" BorderBrush=\"#FFc4c4c4\" BorderThickness=\"0.5\">"
                    + "<StackPanel Orientation=\"Vertical\" HorizontalAlignment=\"Center\">";

                if (PieChartParameters.ShowAnnotationLabel == true)
                {
                    xaml = xaml + "<charting:FormattedTextBlock Margin=\".5,.5,0,0\" HorizontalAlignment=\"Center\" TextAlignment=\"Center\" Data=\"{Binding DataPoint.ActualLabel}\" Foreground=\"Black\" FontWeight=\"Bold\" FontSize=\"10.5\" />";
                }

                xaml = xaml + "<StackPanel Orientation=\"Horizontal\" HorizontalAlignment=\"Center\">";

                if (PieChartParameters.ShowAnnotationValue == true)
                {
                    xaml = xaml + "<TextBlock Text=\"{Binding DataPoint.Y}\" Foreground=\"Black\" FontWeight=\"Bold\" FontSize=\"10.5\" HorizontalAlignment=\"Center\"/>";
                }
                if (PieChartParameters.ShowAnnotationPercent == true)
                {
                    xaml += "<dvCommon:CalcContainer>"
                    + "<dvCommon:CalcContainer.Computing>"
                    + "<sys:String>"
                    + "var sum = 0;"
                    + "var i = 0;"
                    + "while (DataPoint.Chart.DataSeries['S0'].DataPoints.Count > i)"
                    + "{"
                    + "sum = sum + DataPoint.Chart.DataSeries['S0'].DataPoints[i].Y;"
                    + "i = i + 1"
                    + "};"
                    + "var data = '??';"
                                    + "if (sum > 0)"
                                    + "{"
                                    + "    data = DataPoint.Y / sum "
                                    + "};"
                                    + "Data = data;"
                + "</sys:String>"
            + "</dvCommon:CalcContainer.Computing>"
            + "<charting:FormattedTextBlock Format=\" ( 0% )\" FontSize=\"10.5\" HorizontalAlignment=\"Center\" x:Name=\"percentText\">"
            + "</charting:FormattedTextBlock>"
        + "</dvCommon:CalcContainer>";
                }
                xaml = xaml + "</StackPanel></StackPanel>" + "</Border></DataTemplate>";

                DataPointAnnotation dpa1 = new DataPointAnnotation();
                dpa1.MinimumPercentage = AnnotationPercentCutoff;// 20;

                dpa1.LineStroke = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                dpa1.Template = (DataTemplate)System.Windows.Markup.XamlReader.Parse(xaml);
                series0.DataPointAnnotations.Add(dpa1);

                DataPointAnnotation dpa2 = new DataPointAnnotation();
                dpa2.RelativeX = 0.8;
                dpa2.RelativeY = 0.5;
                dpa2.RelativeRadialOffset = 0.3;
                dpa2.HorizontalOffset = 5;
                dpa2.MaximumPercentage = AnnotationPercentCutoff;// 20;

                dpa2.LineStroke = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                dpa2.Template = (DataTemplate)System.Windows.Markup.XamlReader.Parse(xaml);
                series0.DataPointAnnotations.Add(dpa2);  
            }
        }

        public override string ToHTML(string htmlFileName = "", int count = 0, bool includeImage = true, bool includeFullData = false)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            string imageFileName = string.Empty;

            if (htmlFileName.EndsWith(".html"))
            {
                imageFileName = htmlFileName.Remove(htmlFileName.Length - 5, 5);
            }
            else if (htmlFileName.EndsWith(".htm"))
            {
                imageFileName = htmlFileName.Remove(htmlFileName.Length - 4, 4);
            }

            imageFileName = imageFileName + "_" + count.ToString() + ".png";

            System.IO.FileInfo fi = new System.IO.FileInfo(imageFileName);

            ToImageFile(imageFileName, false);

            htmlBuilder.AppendLine("<h3>" + this.StrataTitle + "</h3>");
            if (includeImage) htmlBuilder.AppendLine("<img src=\"" + fi.Name + "\" />");

            htmlBuilder.AppendLine("<p>&nbsp;</p>");
            htmlBuilder.AppendLine("<p>&nbsp;</p>");
            htmlBuilder.AppendLine("<p>&nbsp;</p>");

            return htmlBuilder.ToString();
        }

        public override void CopyAllDataToClipboard()
        {
            List<XYColumnChartData> dataList = series0.DataSource as List<XYColumnChartData>;

            StringBuilder sb = new StringBuilder();

            sb.Append("Variable" + "\t");
            sb.Append("Count" + "\t");
            sb.Append("Value" + "\t");
            sb.AppendLine();

            foreach (XYColumnChartData chartData in dataList)
            {
                sb.Append(chartData.X + "\t");
                sb.Append(chartData.Y + "\t");
                sb.Append(chartData.S + "\t");
                sb.AppendLine();
            }
            Clipboard.Clear();
            Clipboard.SetText(sb.ToString());
        }
    }
}
