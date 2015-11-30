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
using Epi.Fields;
using EpiDashboard;
using EpiDashboard.Gadgets.Charting;
using System.Collections;

namespace EpiDashboard.Controls.Charting
{
    /// <summary>
    /// Interaction logic for HistogramChart.xaml
    /// </summary>
    public partial class HistogramChart : HistogramChartBase
    {
        //public HistogramChartSettings HistogramChartSettings { get; set; }

        //public HistogramChart(DashboardHelper dashboardHelper, GadgetParameters parameters, HistogramChartSettings settings, List<XYColumnChartData> dataList)
        public HistogramChart(DashboardHelper dashboardHelper, HistogramChartParameters parameters, List<XYColumnChartData> dataList)
        {
            InitializeComponent();
            //this.Settings = settings;
            //this.HistogramChartSettings = settings;
            HistogramChartParameters = parameters;
            this.DashboardHelper = dashboardHelper;
            SetChartProperties();
            SetChartData(dataList);

            xyChart.Legend.BorderBrush = Brushes.Gray;
        }

        private class XYHistogramChartData
        {
            public string X { get; set; }
            public double Y { get; set; }
            public double? Y2 { get; set; }
            public string S { get; set; }
        }

        protected override void SetChartData(List<XYColumnChartData> dataList)
        {
            List<XYHistogramChartData> histogramDataList = new List<XYHistogramChartData>();

            //            var dList =.Select(x => (Convert.ToDateTime(x.S) < HistogramChartParameters.EndDate && Convert.ToDateTime( x.S) > HistogramChartParameters.StartDate))


            foreach (XYColumnChartData dataItem in  dataList)
            {
                XYHistogramChartData histogramDataListItem = new XYHistogramChartData();
                histogramDataListItem.X = dataItem.X.ToString();
                histogramDataListItem.Y = dataItem.Y;
                histogramDataListItem.S = dataItem.S.ToString();
                histogramDataList.Add(histogramDataListItem);
                
            }

            if (dataList.Count > 0 && dataList[0].Y2.HasValue && series1 != null)
            {
                series1.Visibility = System.Windows.Visibility.Visible;
            }
            xyChart.DataSource = histogramDataList; //dataList;
        }

        public override string SendDataToString()
        {
            StringBuilder sb = new StringBuilder();            
            List<XYHistogramChartData> dataList = xyChart.DataSource as List<XYHistogramChartData>;

            sb.Append("X" + "\t");
            sb.Append("Y" + "\t");
            sb.Append("S" + "\t");
            sb.AppendLine();

            foreach (XYHistogramChartData chartData in dataList)
            {
                sb.Append(chartData.X + "\t");
                sb.Append(chartData.Y + "\t");
                sb.Append(chartData.S + "\t");
                sb.AppendLine();
            }            

            return sb.ToString();
        }

        protected override void SetChartProperties()
        {
            xyChart.AnimationOnLoad = false;
            //xyChart.Width = Settings.ChartWidth;
            //xyChart.Height = Settings.ChartHeight;
            //xyChart.Palette = Settings.Palette;
            //xyChart.DefaultGridLinesVisible = Settings.ShowDefaultGridLines;
            //xyChart.LegendDock = Settings.LegendDock;

            xyChart.Width = HistogramChartParameters.ChartWidth;
            xyChart.Height = HistogramChartParameters.ChartHeight;
            xyChart.LegendDock = HistogramChartParameters.LegendDock;
            xyChart.DefaultGridLinesVisible = HistogramChartParameters.ShowGridLines;
            

            switch (HistogramChartParameters.Palette)
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
            if (HistogramChartParameters.PaletteColors.Count() == 12)
            {
                ComponentArt.Win.DataVisualization.Palette CorpColorPalette = new ComponentArt.Win.DataVisualization.Palette();
                CorpColorPalette.PaletteName = "CorpColorPalette";
                /////////////////////////////////////
                CorpColorPalette.ChartingDataPoints12 = new Object();
                var NewPalette12 = (IList)xyChart.Palette.ChartingDataPoints12;
                for (int j = 0; j < HistogramChartParameters.PaletteColors.Count(); j++)
                {
                    NewPalette12[j] = (Color)ColorConverter.ConvertFromString(HistogramChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints12 = (Object)NewPalette12;
                xyChart.Palette.ChartingDataPoints12 = CorpColorPalette.ChartingDataPoints12;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints11 = new Object();
                var NewPalette11 = (IList)xyChart.Palette.ChartingDataPoints11;
                for (int j = 0; j < HistogramChartParameters.PaletteColors.Count() - 1; j++)
                {
                    NewPalette11[j] = (Color)ColorConverter.ConvertFromString(HistogramChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints11 = (Object)NewPalette11;
                xyChart.Palette.ChartingDataPoints11 = CorpColorPalette.ChartingDataPoints11;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints10 = new Object();
                var NewPalette10 = (IList)xyChart.Palette.ChartingDataPoints10;
                for (int j = 0; j < HistogramChartParameters.PaletteColors.Count() - 2; j++)
                {
                    NewPalette10[j] = (Color)ColorConverter.ConvertFromString(HistogramChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints10 = (Object)NewPalette10;
                xyChart.Palette.ChartingDataPoints10 = CorpColorPalette.ChartingDataPoints10;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints9 = new Object();
                var NewPalette9 = (IList)xyChart.Palette.ChartingDataPoints9;
                for (int j = 0; j < HistogramChartParameters.PaletteColors.Count() - 3; j++)
                {
                    NewPalette9[j] = (Color)ColorConverter.ConvertFromString(HistogramChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints9 = (Object)NewPalette9;
                xyChart.Palette.ChartingDataPoints9 = CorpColorPalette.ChartingDataPoints9;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints8 = new Object();
                var NewPalette8 = (IList)xyChart.Palette.ChartingDataPoints8;
                for (int j = 0; j < HistogramChartParameters.PaletteColors.Count() - 4; j++)
                {
                    NewPalette8[j] = (Color)ColorConverter.ConvertFromString(HistogramChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints8 = (Object)NewPalette8;
                xyChart.Palette.ChartingDataPoints8 = CorpColorPalette.ChartingDataPoints8;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints7 = new Object();
                var NewPalette7 = (IList)xyChart.Palette.ChartingDataPoints7;
                for (int j = 0; j < HistogramChartParameters.PaletteColors.Count() - 5; j++)
                {
                    NewPalette7[j] = (Color)ColorConverter.ConvertFromString(HistogramChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints7 = (Object)NewPalette7;
                xyChart.Palette.ChartingDataPoints7 = CorpColorPalette.ChartingDataPoints7;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints6 = new Object();
                var NewPalette6 = (IList)xyChart.Palette.ChartingDataPoints6;
                for (int j = 0; j < HistogramChartParameters.PaletteColors.Count() - 6; j++)
                {
                    NewPalette6[j] = (Color)ColorConverter.ConvertFromString(HistogramChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints6 = (Object)NewPalette6;
                xyChart.Palette.ChartingDataPoints6 = CorpColorPalette.ChartingDataPoints6;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints5 = new Object();
                var NewPalette5 = (IList)xyChart.Palette.ChartingDataPoints5;
                for (int j = 0; j < HistogramChartParameters.PaletteColors.Count() - 7; j++)
                {
                    NewPalette5[j] = (Color)ColorConverter.ConvertFromString(HistogramChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints5 = (Object)NewPalette5;
                xyChart.Palette.ChartingDataPoints5 = CorpColorPalette.ChartingDataPoints5;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints4 = new Object();
                var NewPalette4 = (IList)xyChart.Palette.ChartingDataPoints4;
                for (int j = 0; j < HistogramChartParameters.PaletteColors.Count() - 8; j++)
                {
                    NewPalette4[j] = (Color)ColorConverter.ConvertFromString(HistogramChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints4 = (Object)NewPalette4;
                xyChart.Palette.ChartingDataPoints4 = CorpColorPalette.ChartingDataPoints4;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints3 = new Object();
                var NewPalette3 = (IList)xyChart.Palette.ChartingDataPoints3;
                for (int j = 0; j < HistogramChartParameters.PaletteColors.Count() - 9; j++)
                {
                    NewPalette3[j] = (Color)ColorConverter.ConvertFromString(HistogramChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints3 = (Object)NewPalette3;
                xyChart.Palette.ChartingDataPoints3 = CorpColorPalette.ChartingDataPoints3;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints2 = new Object();
                var NewPalette2 = (IList)xyChart.Palette.ChartingDataPoints2;
                for (int j = 0; j < HistogramChartParameters.PaletteColors.Count() - 10; j++)
                {
                    NewPalette2[j] = (Color)ColorConverter.ConvertFromString(HistogramChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints2 = (Object)NewPalette2;
                xyChart.Palette.ChartingDataPoints2 = CorpColorPalette.ChartingDataPoints2;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints1 = new Object();
                var NewPalette1 = (IList)xyChart.Palette.ChartingDataPoints1;
                for (int j = 0; j < HistogramChartParameters.PaletteColors.Count() - 11; j++)
                {
                    NewPalette1[j] = (Color)ColorConverter.ConvertFromString(HistogramChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints1 = (Object)NewPalette1;
                xyChart.Palette.ChartingDataPoints1 = CorpColorPalette.ChartingDataPoints1;
            }
            series0.BarRelativeBegin = double.NaN;
            series0.BarRelativeEnd = double.NaN;
            

            switch (HistogramChartParameters.BarSpace)
            {
                case BarSpacing.None:
                    series0.RelativePointSpace = 0;
                    series0.BarRelativeBegin = 0;
                    series0.BarRelativeEnd = 0;
                    break;
                case BarSpacing.Small:
                    series0.RelativePointSpace = 0.1;
                    break;
                case BarSpacing.Medium:
                    series0.RelativePointSpace = 0.4;
                    break;
                case BarSpacing.Large:
                    series0.RelativePointSpace = 0.8;
                    break;
                case BarSpacing.Default:
                default:
                    series0.RelativePointSpace = 0.15;
                    break;
            }

            //xAxisCoordinates.Angle = Settings.XAxisLabelRotation;
            xAxisCoordinates.Angle = HistogramChartParameters.XAxisAngle;
            
            

            //switch ((EpiDashboard.XAxisLabelType)Settings.XAxisLabelType)
            switch ((EpiDashboard.XAxisLabelType)HistogramChartParameters.XAxisLabelType)
            {
                case XAxisLabelType.Custom:
                case XAxisLabelType.FieldPrompt:
                    //tblockXAxisLabel.Text = Settings.XAxisLabel;
                    //tblockXAxisLabel.Text = Settings.XAxisLabel;
                    tblockXAxisLabel.Text = HistogramChartParameters.XAxisLabel;
                    tblockXAxisLabel.Text = HistogramChartParameters.XAxisLabel;
                    break;
                case XAxisLabelType.None:
                    tblockXAxisLabel.Text = string.Empty;
                    break;
                default:
                    tblockXAxisLabel.Text = HistogramChartParameters.XAxisLabel;
                    break;
            }

            switch (HistogramChartParameters.XAxisLabelType)
            {
                default:
                case 0:  //Automatic
                    if (!String.IsNullOrEmpty(HistogramChartParameters.XAxisLabel))
                    {
                        tblockXAxisLabel.Text = HistogramChartParameters.XAxisLabel;
                    }
                    else
                    {
                        tblockXAxisLabel.Text = HistogramChartParameters.ColumnNames[0];
                    }
                    break;
                case 1:  //Field Prompt
                    {
                        Field field = DashboardHelper.GetAssociatedField(HistogramChartParameters.ColumnNames[0]);
                        if (field != null)
                        {
                            RenderableField rField = field as RenderableField;
                            tblockXAxisLabel.Text = rField.PromptText;
                        }
                        else
                        {
                            tblockXAxisLabel.Text = HistogramChartParameters.ColumnNames[0];
                        }
                    }
                    break;
                case 2:  //None
                    tblockXAxisLabel.Text = string.Empty;
                    break;
                case 3:  //Custom
                    tblockXAxisLabel.Text = HistogramChartParameters.XAxisLabel;
                    break;
            }
            
            if (!string.IsNullOrEmpty(tblockXAxisLabel.Text.Trim()))
            {
                tblockXAxisLabel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                tblockXAxisLabel.Visibility = System.Windows.Visibility.Collapsed;
            }

            YAxisLabel = HistogramChartParameters.YAxisLabel;
            Y2AxisLabel = HistogramChartParameters.Y2AxisLabel;
            Y2AxisLegendTitle = HistogramChartParameters.Y2AxisLegendTitle;

            xyChart.LegendVisible = HistogramChartParameters.ShowLegend;
            xyChart.Legend.FontSize = HistogramChartParameters.LegendFontSize;

            Size textSize = new Size();
            Size chartSize = new Size();

            switch (xyChart.Orientation)
            {
                case Orientation.Horizontal:
                    labelXAxis.Orientation = ChartLabelOrientation.Vertical;
                    labelXAxis.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    labelYAxis.Orientation = ChartLabelOrientation.Horizontal;
                    labelY2Axis.Orientation = ChartLabelOrientation.Horizontal;

                    tblockXAxisLabel.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    textSize = new Size(tblockXAxisLabel.DesiredSize.Width, tblockXAxisLabel.DesiredSize.Height);

                    xyChart.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    chartSize = new Size(xyChart.DesiredSize.Width, xyChart.DesiredSize.Height);

                    tblockXAxisLabel.Padding = new Thickness(((chartSize.Height - 144) / 2) - (textSize.Width / 2), 2, 0, 2);
                    break;
                case Orientation.Vertical:
                    labelXAxis.Orientation = ChartLabelOrientation.Horizontal;
                    labelXAxis.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    labelYAxis.Orientation = ChartLabelOrientation.Vertical;
                    labelY2Axis.Orientation = ChartLabelOrientation.Vertical;

                    tblockYAxisLabel.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    textSize = new Size(tblockYAxisLabel.DesiredSize.Width, tblockYAxisLabel.DesiredSize.Height);

                    tblockY2AxisLabel.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    Size textSizeY2 = new Size(tblockY2AxisLabel.DesiredSize.Width, tblockY2AxisLabel.DesiredSize.Height);

                    xyChart.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    chartSize = new Size(xyChart.DesiredSize.Width, xyChart.DesiredSize.Height);

                    tblockYAxisLabel.Padding = new Thickness(((chartSize.Height - 144) / 2) - (textSize.Width / 2), 2, 0, 2);
                    tblockY2AxisLabel.Padding = new Thickness(((chartSize.Height - 144) / 2) - (textSizeY2.Width / 2), 2, 0, 2);
                    break;
            }

            tblockChartTitle.Text = HistogramChartParameters.ChartTitle;
            tblockSubTitle.Text = HistogramChartParameters.ChartSubTitle;
            tblockStrataTitle.Text = HistogramChartParameters.ChartStrataTitle;

            if (string.IsNullOrEmpty(tblockChartTitle.Text)) tblockChartTitle.Visibility = System.Windows.Visibility.Collapsed;
            else tblockChartTitle.Visibility = System.Windows.Visibility.Visible;

            if (string.IsNullOrEmpty(tblockSubTitle.Text)) tblockSubTitle.Visibility = System.Windows.Visibility.Collapsed;
            else tblockSubTitle.Visibility = System.Windows.Visibility.Visible;

            if (string.IsNullOrEmpty(tblockStrataTitle.Text)) tblockStrataTitle.Visibility = System.Windows.Visibility.Collapsed;
            else tblockStrataTitle.Visibility = System.Windows.Visibility.Visible;

            yAxis.UseReferenceValue = HistogramChartParameters.UseRefValues;

            if (HistogramChartParameters.YAxisTo != 0)
                numberCoordinates.To = HistogramChartParameters.YAxisTo;
            if (HistogramChartParameters.YAxisFrom != 0)
                numberCoordinates.From = HistogramChartParameters.YAxisFrom;
            if (HistogramChartParameters.YAxisStep != 0)
                numberCoordinates.Step = HistogramChartParameters.YAxisStep;

            series0.ShowPointAnnotations = HistogramChartParameters.ShowAnnotations;
            series0.BarKind = HistogramChartParameters.BarKind;

            series1.ShowPointAnnotations = HistogramChartParameters.Y2ShowAnnotations;
            series1.DashStyle = HistogramChartParameters.Y2LineDashStyle;
            series1.LineKind = HistogramChartParameters.Y2LineKind;

            // Epi Curve can't have a series1
            series1.Visibility = System.Windows.Visibility.Collapsed;

            if (HistogramChartParameters.ShowLegendBorder == true)
            {
                xyChart.Legend.BorderThickness = new Thickness(1);
            }
            else
            {
                xyChart.Legend.BorderThickness = new Thickness(0);
            }
			//EI-98
            YAxisCoordinates.FontSize = HistogramChartParameters.YAxisFontSize;
            xAxisCoordinates.FontSize = HistogramChartParameters.XAxisFontSize;

            tblockXAxisLabel.FontSize = HistogramChartParameters.XAxisLabelFontSize;
            tblockYAxisLabel.FontSize = HistogramChartParameters.YAxisLabelFontSize;
        }

        private void xyChart_DataStructureCreated(object sender, EventArgs e)
        {
            SetLegendItems();
        }
    }
}
