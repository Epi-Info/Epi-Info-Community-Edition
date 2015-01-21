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

namespace EpiDashboard.Controls.Charting
{
    /// <summary>
    /// Interaction logic for LineChart.xaml
    /// </summary>
    public partial class LineChart : LineChartBase
    {
        //public LineChartSettings LineChartSettings { get; set; }

        public LineChart(DashboardHelper dashboardHelper, LineChartParameters parameters, List<XYColumnChartData> dataList)
        {
            InitializeComponent();
            //this.Settings = settings;
            //this.LineChartSettings = settings;
            LineChartParameters = parameters;
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
            //xyChart.DefaultGridLinesVisible = Settings.ShowDefaultGridLines;
            //xyChart.LegendDock = Settings.LegendDock;

            xyChart.Width = LineChartParameters.ChartWidth;
            xyChart.Height = LineChartParameters.ChartHeight;

            switch (LineChartParameters.Palette)
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

            xyChart.DefaultGridLinesVisible = LineChartParameters.ShowGridLines;
            xyChart.LegendDock = LineChartParameters.LegendDock;

            series0.BarRelativeBegin = double.NaN;
            series0.BarRelativeEnd = double.NaN;

            //series0.LineKind = LineChartSettings.LineKind;
            switch (LineChartParameters.LineKind)
            {
                case LineKind.Auto:
                    series0.LineKind = ComponentArt.Win.DataVisualization.Charting.LineKind.Auto;
                    break;
                case LineKind.Polygon:
                    series0.LineKind = ComponentArt.Win.DataVisualization.Charting.LineKind.Polygon;
                    break;
                case LineKind.Smooth:
                    series0.LineKind = ComponentArt.Win.DataVisualization.Charting.LineKind.Smooth;
                    break;
                case LineKind.Step:
                    series0.LineKind = ComponentArt.Win.DataVisualization.Charting.LineKind.Step;
                    break;
            }

            //tblockChartTitle.Text = Settings.ChartTitle;
            //tblockSubTitle.Text = Settings.ChartSubTitle;
            //tblockStrataTitle.Text = Settings.ChartStrataTitle;

            tblockChartTitle.Text = LineChartParameters.ChartTitle;
            tblockSubTitle.Text = LineChartParameters.ChartSubTitle;
            tblockStrataTitle.Text = LineChartParameters.ChartStrataTitle;

            if (string.IsNullOrEmpty(tblockChartTitle.Text)) tblockChartTitle.Visibility = System.Windows.Visibility.Collapsed;
            else tblockChartTitle.Visibility = System.Windows.Visibility.Visible;

            if (string.IsNullOrEmpty(tblockSubTitle.Text)) tblockSubTitle.Visibility = System.Windows.Visibility.Collapsed;
            else tblockSubTitle.Visibility = System.Windows.Visibility.Visible;

            if (string.IsNullOrEmpty(tblockStrataTitle.Text)) tblockStrataTitle.Visibility = System.Windows.Visibility.Collapsed;
            else tblockStrataTitle.Visibility = System.Windows.Visibility.Visible;

            //yAxis.UseReferenceValue = Settings.UseRefValues;
            yAxis.UseReferenceValue = LineChartParameters.UseRefValues;

            //xAxisCoordinates.Angle = Settings.XAxisLabelRotation;
            xAxisCoordinates.Angle = LineChartParameters.XAxisAngle;

            //switch ((EpiDashboard.XAxisLabelType)Settings.XAxisLabelType)
            switch (LineChartParameters.XAxisLabelType)
            {
                default:
                case 0:
                    if (!String.IsNullOrEmpty(LineChartParameters.XAxisLabel))
                    {
                        tblockXAxisLabel.Text = LineChartParameters.XAxisLabel;
                    }
                    else
                    {
                        tblockXAxisLabel.Text = LineChartParameters.ColumnNames[0];
                    }
                    break;
                case 1:
                    {
                        Field field = DashboardHelper.GetAssociatedField(LineChartParameters.ColumnNames[0]);
                        if (field != null)
                        {
                            RenderableField rField = field as RenderableField;
                            tblockXAxisLabel.Text = rField.PromptText;
                        }
                        else
                        {
                            tblockXAxisLabel.Text = LineChartParameters.ColumnNames[0];
                        }
                    }
                    break;
                case 2:
                    tblockXAxisLabel.Text = string.Empty;
                    break;
                case 3:
                    //tblockXAxisLabel.Text = Settings.XAxisLabel;
                    tblockXAxisLabel.Text = LineChartParameters.XAxisLabel;
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

            if (!string.IsNullOrEmpty(LineChartParameters.YAxisFormat.Trim()))
            {
                //YAxisCoordinates.FormattingString = LineChartSettings.YAxisFormattingString;
                YAxisCoordinates.FormattingString = LineChartParameters.YAxisFormat;

            }
            if (!string.IsNullOrEmpty(LineChartParameters.Y2AxisFormat.Trim()))
            {
                Y2AxisCoordinates.FormattingString = LineChartParameters.Y2AxisFormat;
            }

            //YAxisLabel = Settings.YAxisLabel;
            //Y2AxisLabel = Settings.Y2AxisLabel;
            //Y2AxisLegendTitle = Settings.Y2AxisLegendTitle;
            YAxisLabel = LineChartParameters.YAxisLabel;
            Y2AxisLabel = LineChartParameters.Y2AxisLabel;
            Y2AxisLegendTitle = LineChartParameters.Y2AxisLegendTitle;

            labelXAxis.Orientation = ChartLabelOrientation.Horizontal;
            labelXAxis.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            labelYAxis.Orientation = ChartLabelOrientation.Vertical;
            labelY2Axis.Orientation = ChartLabelOrientation.Vertical;

            tblockYAxisLabel.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            Size textSize = new Size(tblockYAxisLabel.DesiredSize.Width, tblockYAxisLabel.DesiredSize.Height);

            tblockY2AxisLabel.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            Size textSizeY2 = new Size(tblockY2AxisLabel.DesiredSize.Width, tblockY2AxisLabel.DesiredSize.Height);

            xyChart.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            Size chartSize = new Size(xyChart.DesiredSize.Width, xyChart.DesiredSize.Height);

            tblockYAxisLabel.Padding = new Thickness(((chartSize.Height - 144) / 2) - (textSize.Width / 2), 2, 0, 2);
            tblockY2AxisLabel.Padding = new Thickness(((chartSize.Height - 144) / 2) - (textSizeY2.Width / 2), 2, 0, 2);

            series0.ShowPointAnnotations = LineChartParameters.ShowAnnotations;
            //series0.Thickness = ((LineChartSettings)Settings).LineThickness;
            //series1.ShowPointAnnotations = Settings.ShowAnnotationsY2;
            //series1.DashStyle = Settings.LineDashStyleY2;
            //series1.LineKind = Settings.LineKindY2;
            //series1.Thickness = Settings.Y2LineThickness;
            //xyChart.LegendVisible = Settings.ShowLegend;

            series0.Thickness = LineChartParameters.LineThickness;

            series1.ShowPointAnnotations = LineChartParameters.Y2ShowAnnotations;
            series1.DashStyle = LineChartParameters.Y2LineDashStyle;
            series1.LineKind = LineChartParameters.Y2LineKind;
            series1.Thickness = LineChartParameters.Y2LineThickness;

            xyChart.LegendVisible = LineChartParameters.ShowLegend;
            //if (Settings.ShowLegendBorder == true)
            if (LineChartParameters.ShowLegendBorder == true)
            {
                xyChart.Legend.BorderThickness = new Thickness(1);
            }
            else 
            {
                xyChart.Legend.BorderThickness = new Thickness(0);
            }

            //xyChart.Legend.FontSize = Settings.LegendFontSize;
            xyChart.Legend.FontSize = LineChartParameters.LegendFontSize;

            //EI-98
            YAxisCoordinates.FontSize = LineChartParameters.YAxisFontSize;
            xAxisCoordinates.FontSize = LineChartParameters.XAxisFontSize;

            tblockXAxisLabel.FontSize = LineChartParameters.XAxisLabelFontSize;
            tblockYAxisLabel.FontSize = LineChartParameters.YAxisLabelFontSize;
        }

        private void xyChart_DataStructureCreated(object sender, EventArgs e)
        {
            SetLegendItems();
            ////SetChartProperties(Settings);

            //string sName = "";

            //if (Parameters.StrataVariableNames.Count > 0)
            //{
            //    foreach (Series s0 in xyChart.DataSeries)
            //    {
            //        sName = s0.Label.Split('.')[1];
            //        if (Settings.ShowLegendVarNames == false)
            //        {
            //            int index = sName.IndexOf(" = ");
            //            s0.Label = sName.Substring(index + 3);
            //        }
            //        else
            //        {
            //            s0.Label = sName;
            //        }
            //    }
            //}
            //else
            //{
            //    foreach (Series series in xyChart.DataSeries)
            //    {
            //        if (series is BarSeries)
            //        {
            //            series.Label = Settings.YAxisLabel;
            //        }

            //        if (series is LineSeries)
            //        {
            //            if (!string.IsNullOrEmpty(Y2AxisLegendTitle))
            //            {
            //                series.Label = Settings.Y2AxisLegendTitle;
            //            }
            //            else if (!string.IsNullOrEmpty(Y2AxisLabel))
            //            {
            //                series.Label = Settings.Y2AxisLabel;
            //            }
            //        }
            //    }
            //}
        }
    }
}
