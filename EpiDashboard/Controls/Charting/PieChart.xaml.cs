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
using System.Collections;
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
        public List<XYColumnChartData> _dataList { get; set; }
        public PieChart(DashboardHelper dashboardHelper, PieChartParameters parameters, List<XYColumnChartData> dataList)
        {
            _dataList = dataList;
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
            if (PieChartParameters.PaletteColors.Count() == 12)
            {
                ComponentArt.Win.DataVisualization.Palette CorpColorPalette = new ComponentArt.Win.DataVisualization.Palette();
                CorpColorPalette.PaletteName = "CorpColorPalette";
                /////////////////////////////////////
                CorpColorPalette.ChartingDataPoints12 = new Object();
                var NewPalette12 = (IList)xyChart.Palette.ChartingDataPoints12;
                for (int j = 0; j < PieChartParameters.PaletteColors.Count(); j++)
                {
                    NewPalette12[j] = (Color)ColorConverter.ConvertFromString(PieChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints12 = (Object)NewPalette12;
                xyChart.Palette.ChartingDataPoints12 = CorpColorPalette.ChartingDataPoints12;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints11 = new Object();
                var NewPalette11 = (IList)xyChart.Palette.ChartingDataPoints11;
                for (int j = 0; j < PieChartParameters.PaletteColors.Count() - 1; j++)
                {
                    NewPalette11[j] = (Color)ColorConverter.ConvertFromString(PieChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints11 = (Object)NewPalette11;
                xyChart.Palette.ChartingDataPoints11 = CorpColorPalette.ChartingDataPoints11;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints10 = new Object();
                var NewPalette10 = (IList)xyChart.Palette.ChartingDataPoints10;
                for (int j = 0; j < PieChartParameters.PaletteColors.Count() - 2; j++)
                {
                    NewPalette10[j] = (Color)ColorConverter.ConvertFromString(PieChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints10 = (Object)NewPalette10;
                xyChart.Palette.ChartingDataPoints10 = CorpColorPalette.ChartingDataPoints10;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints9 = new Object();
                var NewPalette9 = (IList)xyChart.Palette.ChartingDataPoints9;
                for (int j = 0; j < PieChartParameters.PaletteColors.Count() - 3; j++)
                {
                    NewPalette9[j] = (Color)ColorConverter.ConvertFromString(PieChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints9 = (Object)NewPalette9;
                xyChart.Palette.ChartingDataPoints9 = CorpColorPalette.ChartingDataPoints9;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints8 = new Object();
                var NewPalette8 = (IList)xyChart.Palette.ChartingDataPoints8;
                for (int j = 0; j < PieChartParameters.PaletteColors.Count() - 4; j++)
                {
                    NewPalette8[j] = (Color)ColorConverter.ConvertFromString(PieChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints8 = (Object)NewPalette8;
                xyChart.Palette.ChartingDataPoints8 = CorpColorPalette.ChartingDataPoints8;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints7 = new Object();
                var NewPalette7 = (IList)xyChart.Palette.ChartingDataPoints7;
                for (int j = 0; j < PieChartParameters.PaletteColors.Count() - 5; j++)
                {
                    NewPalette7[j] = (Color)ColorConverter.ConvertFromString(PieChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints7 = (Object)NewPalette7;
                xyChart.Palette.ChartingDataPoints7 = CorpColorPalette.ChartingDataPoints7;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints6 = new Object();
                var NewPalette6 = (IList)xyChart.Palette.ChartingDataPoints6;
                for (int j = 0; j < PieChartParameters.PaletteColors.Count() - 6; j++)
                {
                    NewPalette6[j] = (Color)ColorConverter.ConvertFromString(PieChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints6 = (Object)NewPalette6;
                xyChart.Palette.ChartingDataPoints6 = CorpColorPalette.ChartingDataPoints6;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints5 = new Object();
                var NewPalette5 = (IList)xyChart.Palette.ChartingDataPoints5;
                for (int j = 0; j < PieChartParameters.PaletteColors.Count() - 7; j++)
                {
                    NewPalette5[j] = (Color)ColorConverter.ConvertFromString(PieChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints5 = (Object)NewPalette5;
                xyChart.Palette.ChartingDataPoints5 = CorpColorPalette.ChartingDataPoints5;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints4 = new Object();
                var NewPalette4 = (IList)xyChart.Palette.ChartingDataPoints4;
                for (int j = 0; j < PieChartParameters.PaletteColors.Count() - 8; j++)
                {
                    NewPalette4[j] = (Color)ColorConverter.ConvertFromString(PieChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints4 = (Object)NewPalette4;
                xyChart.Palette.ChartingDataPoints4 = CorpColorPalette.ChartingDataPoints4;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints3 = new Object();
                var NewPalette3 = (IList)xyChart.Palette.ChartingDataPoints3;
                for (int j = 0; j < PieChartParameters.PaletteColors.Count() - 9; j++)
                {
                    NewPalette3[j] = (Color)ColorConverter.ConvertFromString(PieChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints3 = (Object)NewPalette3;
                xyChart.Palette.ChartingDataPoints3 = CorpColorPalette.ChartingDataPoints3;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints2 = new Object();
                var NewPalette2 = (IList)xyChart.Palette.ChartingDataPoints2;
                for (int j = 0; j < PieChartParameters.PaletteColors.Count() - 10; j++)
                {
                    NewPalette2[j] = (Color)ColorConverter.ConvertFromString(PieChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints2 = (Object)NewPalette2;
                xyChart.Palette.ChartingDataPoints2 = CorpColorPalette.ChartingDataPoints2;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints1 = new Object();
                var NewPalette1 = (IList)xyChart.Palette.ChartingDataPoints1;
                for (int j = 0; j < PieChartParameters.PaletteColors.Count() - 11; j++)
                {
                    NewPalette1[j] = (Color)ColorConverter.ConvertFromString(PieChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints1 = (Object)NewPalette1;
                xyChart.Palette.ChartingDataPoints1 = CorpColorPalette.ChartingDataPoints1;
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

        public override string ToHTML(string htmlFileName = "", int count = 0, bool includeImage = true, bool includeFullData = false, bool ForWeb = false)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            ForWeb = true;
            if (!ForWeb)
            {
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
            }
            else {
                double top =  Canvas.GetTop(this);
                double left = Canvas.GetBottom(this);
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + this.PieChartParameters.GadgetTitle + "</h2>");
                htmlBuilder.AppendLine("<div id=\"piechart"+count+"\"></div>");
                
                htmlBuilder.AppendLine("<script>");


                  htmlBuilder.AppendLine(" var piechart"+count+" = c3.generate({bindto: '#piechart"+count+"',data: { columns: ["); 
                var temp =_dataList;
                var color = PieChartParameters.PaletteColors;
                foreach (var item in _dataList)
                {
                    htmlBuilder.AppendLine("['"+ item.S + "', " + item.Y + "], ");

                }
                //string colorString = "";
                if (this.PieChartParameters.PieChartKind.ToString() == "Donut2D") {
                    htmlBuilder.AppendLine(" ],  type : 'donut'  },");
                }
                else {
                    htmlBuilder.AppendLine(" ],  type : 'pie'  },");

                }

                htmlBuilder.AppendLine(" color: { pattern: [");
                for (int i =0; i< _dataList.Count();i++)
                {
                    htmlBuilder.AppendLine(" '"+ color[i].Remove(1,2)+ "' ,");
                }


                htmlBuilder.AppendLine(" ]},");

               htmlBuilder.AppendLine(" legend: { show: true , position: top }, size: { width: " + this.ActualWidth + ", height: "+ this.ActualHeight + " },label: { format: function(value, ratio, id){return d3.format('$')(value);}}});");

             
                htmlBuilder.AppendLine(" </script> ");

            }
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
