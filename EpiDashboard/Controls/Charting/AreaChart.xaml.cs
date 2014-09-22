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
    /// Interaction logic for AreaChart.xaml
    /// </summary>
    public partial class AreaChart : ChartBase
    {
        public AreaChartSettings AreaChartSettings { get; set; }

        public AreaChart(DashboardHelper dashboardHelper, GadgetParameters parameters, AreaChartSettings settings, List<XYColumnChartData> dataList)
        {
            InitializeComponent();
            this.Settings = settings;
            this.AreaChartSettings = settings;
            this.Parameters = parameters;
            this.DashboardHelper = dashboardHelper;
            SetChartProperties();
            SetChartData(dataList);

            xyChart.Legend.BorderBrush = Brushes.Gray;
        }

        protected override void SetChartProperties()
        {
            xyChart.AnimationOnLoad = false;
            xyChart.Width = Settings.ChartWidth;
            xyChart.Height = Settings.ChartHeight;
            xyChart.Palette = Settings.Palette;
            xyChart.DefaultGridLinesVisible = Settings.ShowDefaultGridLines;
            xyChart.LegendDock = Settings.LegendDock;

            series0.BarRelativeBegin = double.NaN;
            series0.BarRelativeEnd = double.NaN;

            series0.LineKind = AreaChartSettings.AreaType;

            tblockChartTitle.Text = Settings.ChartTitle;
            tblockSubTitle.Text = Settings.ChartSubTitle;
            tblockStrataTitle.Text = Settings.ChartStrataTitle;

            if (string.IsNullOrEmpty(tblockChartTitle.Text)) tblockChartTitle.Visibility = System.Windows.Visibility.Collapsed;
            else tblockChartTitle.Visibility = System.Windows.Visibility.Visible;

            if (string.IsNullOrEmpty(tblockSubTitle.Text)) tblockSubTitle.Visibility = System.Windows.Visibility.Collapsed;
            else tblockSubTitle.Visibility = System.Windows.Visibility.Visible;

            if (string.IsNullOrEmpty(tblockStrataTitle.Text)) tblockStrataTitle.Visibility = System.Windows.Visibility.Collapsed;
            else tblockStrataTitle.Visibility = System.Windows.Visibility.Visible;

            yAxis.UseReferenceValue = Settings.UseRefValues;
            series0.SeriesLineVisible = AreaChartSettings.ShowSeriesLine;

            xAxisCoordinates.Angle = Settings.XAxisLabelRotation;

            series0.GradientTopColorShift = new ComponentArt.Win.DataVisualization.Common.ColorShift("A-" + AreaChartSettings.TransparencyTop.ToString());
            series0.GradientBottomColorShift = new ComponentArt.Win.DataVisualization.Common.ColorShift("A-" + AreaChartSettings.TransparencyBottom.ToString());

            switch (Settings.XAxisLabelType)
            {
                case XAxisLabelType.Custom:
                case XAxisLabelType.FieldPrompt:
                    tblockXAxisLabel.Text = Settings.XAxisLabel;
                    tblockXAxisLabel.Text = Settings.XAxisLabel;
                    break;
                case XAxisLabelType.None:
                    tblockXAxisLabel.Text = string.Empty;
                    break;
                default:
                    tblockXAxisLabel.Text = Settings.XAxisLabel;
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

            if (!string.IsNullOrEmpty(AreaChartSettings.YAxisFormattingString.Trim()))
            {
                YAxisCoordinates.FormattingString = AreaChartSettings.YAxisFormattingString;
            }
            if (!string.IsNullOrEmpty(AreaChartSettings.Y2AxisFormattingString.Trim()))
            {
                Y2AxisCoordinates.FormattingString = AreaChartSettings.Y2AxisFormattingString;
            }

            YAxisLabel = Settings.YAxisLabel;
            Y2AxisLabel = Settings.Y2AxisLabel;
            Y2AxisLegendTitle = Settings.Y2AxisLegendTitle;

            xyChart.CompositionKind = AreaChartSettings.Composition;

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

            xyChart.LegendVisible = Settings.ShowLegend;
            if (Settings.ShowLegendBorder == true)
            {
                xyChart.Legend.BorderThickness = new Thickness(1);
            }
            else
            {
                xyChart.Legend.BorderThickness = new Thickness(0);
            }

            series1.ShowPointAnnotations = Settings.ShowAnnotationsY2;
            series1.DashStyle = Settings.LineDashStyleY2;
            series1.LineKind = Settings.LineKindY2;
            series1.Thickness = Settings.Y2LineThickness;

            xyChart.Legend.FontSize = Settings.LegendFontSize;
        }

        private void xyChart_DataStructureCreated(object sender, EventArgs e)
        {
            SetLegendItems();
        }
    }
}
