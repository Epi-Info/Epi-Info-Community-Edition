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
    /// Interaction logic for ColumnChart.xaml
    /// </summary>
    public partial class ColumnChart : ChartBase
    {
        public ColumnChartSettings ColumnChartSettings { get; set; }

        public ColumnChart(DashboardHelper dashboardHelper, GadgetParameters parameters, ColumnChartSettings settings, List<XYColumnChartData> dataList)
        {
            InitializeComponent();
            this.Settings = settings;
            this.ColumnChartSettings = settings;
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

            switch (ColumnChartSettings.BarSpacing)
            {
                case BarSpacing.None:
                    series0.RelativePointSpace = 0;
                    series0.BarRelativeBegin = 0;
                    series0.BarRelativeEnd = 0;
                    if (ColumnChartSettings.Composition == CompositionKind.SideBySide) ColumnChartSettings.Composition = CompositionKind.Stacked;
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

            if (!string.IsNullOrEmpty(ColumnChartSettings.YAxisFormattingString.Trim()))
            {
                YAxisCoordinates.FormattingString = ColumnChartSettings.YAxisFormattingString;
            }
            if (!string.IsNullOrEmpty(ColumnChartSettings.Y2AxisFormattingString.Trim()))
            {
                Y2AxisCoordinates.FormattingString = ColumnChartSettings.Y2AxisFormattingString;
            }
            xyChart.CompositionKind = ColumnChartSettings.Composition;
            xyChart.Orientation = ColumnChartSettings.Orientation;
            xAxisCoordinates.Angle = Settings.XAxisLabelRotation;

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

            YAxisLabel = Settings.YAxisLabel;
            Y2AxisLabel = Settings.Y2AxisLabel;
            Y2AxisLegendTitle = Settings.Y2AxisLegendTitle;

            xyChart.UseDifferentBarColors = ColumnChartSettings.UseDiffColors;
            xyChart.LegendVisible = Settings.ShowLegend;
            xyChart.Legend.FontSize = Settings.LegendFontSize;

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

            if ((Settings as ColumnChartSettings).YAxisTo != 0)
                numberCoordinates.To = (Settings as ColumnChartSettings).YAxisTo;

            if ((Settings as ColumnChartSettings).YAxisFrom != 0)
                numberCoordinates.From = (Settings as ColumnChartSettings).YAxisFrom;

            if ((Settings as ColumnChartSettings).YAxisStep != 0)
                numberCoordinates.Step = (Settings as ColumnChartSettings).YAxisStep;

            series0.ShowPointAnnotations = ColumnChartSettings.ShowAnnotations;
            series0.BarKind = ColumnChartSettings.BarKind;

            series1.ShowPointAnnotations = Settings.ShowAnnotationsY2;
            series1.DashStyle = Settings.LineDashStyleY2;
            series1.LineKind = Settings.LineKindY2;
            series1.Thickness = Settings.Y2LineThickness;

            if (Settings.ShowLegendBorder == true)
            {
                xyChart.Legend.BorderThickness = new Thickness(1);
            }
            else
            {
                xyChart.Legend.BorderThickness = new Thickness(0);
            }
        }

        private void xyChart_DataStructureCreated(object sender, EventArgs e)
        {
            if (ColumnChartSettings.Y2IsCumulativePercent)
            {
                NumericCoordinates nc = new NumericCoordinates() { From = 0, To = 1, Step = 0.1 };
                Y2AxisCoordinates.Coordinates = nc;
            }

            SetLegendItems();
        }
    }
}
