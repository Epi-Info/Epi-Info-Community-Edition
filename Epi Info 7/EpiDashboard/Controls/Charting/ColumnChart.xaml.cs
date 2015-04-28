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
    /// Interaction logic for ColumnChart.xaml
    /// </summary>
    public partial class ColumnChart : ColumnChartBase
    {
        //public ColumnChartSettings ColumnChartSettings { get; set; }
        
        public ColumnChart(DashboardHelper dashboardHelper, ColumnChartParameters parameters, List<XYColumnChartData> dataList)
        {
            InitializeComponent();
            //this.Settings = settings;
            //this.ColumnChartSettings = settings;
            ColumnChartParameters = parameters;
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

            xyChart.Width = ColumnChartParameters.ChartWidth;
            xyChart.Height = ColumnChartParameters.ChartHeight;

            //xyChart.Palette = ColumnChartParameters.Palette;
            switch (ColumnChartParameters.Palette)
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

            xyChart.DefaultGridLinesVisible = ColumnChartParameters.ShowGridLines;
            xyChart.LegendDock = ColumnChartParameters.LegendDock;

            series0.BarRelativeBegin = double.NaN;
            series0.BarRelativeEnd = double.NaN;

            switch ((BarSpacing)ColumnChartParameters.BarSpace)
            {
                case BarSpacing.None:                
                    series0.RelativePointSpace = 0;
                    series0.BarRelativeBegin = 0;
                    series0.BarRelativeEnd = 0;
                    if (ColumnChartParameters.Composition == CompositionKind.SideBySide) ColumnChartParameters.Composition = CompositionKind.Stacked;
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

            //if (!string.IsNullOrEmpty(ColumnChartSettings.YAxisFormattingString.Trim()))
            //{
            //    YAxisCoordinates.FormattingString = ColumnChartSettings.YAxisFormattingString;
            //}
            //if (!string.IsNullOrEmpty(ColumnChartSettings.Y2AxisFormattingString.Trim()))
            //{
            //    Y2AxisCoordinates.FormattingString = ColumnChartSettings.Y2AxisFormattingString;
            //}
            //xyChart.CompositionKind = ColumnChartSettings.Composition;
            //xyChart.Orientation = ColumnChartSettings.Orientation;
            //xAxisCoordinates.Angle = Settings.XAxisLabelRotation;

            if (!string.IsNullOrEmpty(ColumnChartParameters.YAxisFormat.Trim()))
            {
                YAxisCoordinates.FormattingString = ColumnChartParameters.YAxisFormat.Trim();
            }
            if (!string.IsNullOrEmpty(ColumnChartParameters.Y2AxisFormat.Trim()))
            {
                Y2AxisCoordinates.FormattingString = ColumnChartParameters.Y2AxisFormat.Trim();
            }
            xyChart.CompositionKind = ColumnChartParameters.Composition;
            xyChart.Orientation = ColumnChartParameters.Orientation;
            xAxisCoordinates.Angle = ColumnChartParameters.XAxisAngle;

            //switch (Settings.XAxisLabelType)
            //{
            //    case XAxisLabelType.Custom:
            //    case XAxisLabelType.FieldPrompt:
            //        tblockXAxisLabel.Text = Settings.XAxisLabel;
            //        tblockXAxisLabel.Text = Settings.XAxisLabel;        ???  WHY WAS THIS IN THERE TWICE ???
            //        break;
            //    case XAxisLabelType.None:
            //        tblockXAxisLabel.Text = string.Empty;
            //        break;
            //    default:
            //        tblockXAxisLabel.Text = Settings.XAxisLabel;
            //        break;
            //}

            switch (ColumnChartParameters.XAxisLabelType)
            {
                default:
                case 0:  //Automatic
                    if (!String.IsNullOrEmpty(ColumnChartParameters.XAxisLabel))
                    {
                        tblockXAxisLabel.Text = ColumnChartParameters.XAxisLabel;
                    }
                    else
                    {
                        tblockXAxisLabel.Text = ColumnChartParameters.ColumnNames[0];
                    }
                    break;
                case 1:  //Field Prompt
                    {
                        Field field = DashboardHelper.GetAssociatedField(ColumnChartParameters.ColumnNames[0]);
                        if (field != null)
                        {
                            RenderableField rField = field as RenderableField;
                            tblockXAxisLabel.Text = rField.PromptText;
                        }
                        else
                        {
                            tblockXAxisLabel.Text = ColumnChartParameters.ColumnNames[0];
                        }
                    }
                    break;
                case 2:  //None
                    tblockXAxisLabel.Text = string.Empty;
                    break;
                case 3:  //Custom
                    tblockXAxisLabel.Text = ColumnChartParameters.XAxisLabel;
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

            //YAxisLabel = Settings.YAxisLabel;
            //Y2AxisLabel = Settings.Y2AxisLabel;
            //Y2AxisLegendTitle = Settings.Y2AxisLegendTitle;

            //xyChart.UseDifferentBarColors = ColumnChartSettings.UseDiffColors;
            //xyChart.LegendVisible = Settings.ShowLegend;
            //xyChart.Legend.FontSize = Settings.LegendFontSize;

            YAxisLabel = ColumnChartParameters.YAxisLabel;
            Y2AxisLabel = ColumnChartParameters.Y2AxisLabel;
            Y2AxisLegendTitle = ColumnChartParameters.Y2AxisLegendTitle;

            xyChart.UseDifferentBarColors = ColumnChartParameters.UseDiffColors;
            xyChart.LegendVisible = ColumnChartParameters.ShowLegend;
            xyChart.Legend.FontSize = ColumnChartParameters.LegendFontSize;

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
            
            //tblockChartTitle.Text = Settings.ChartTitle;
            //tblockSubTitle.Text = Settings.ChartSubTitle;
            //tblockStrataTitle.Text = Settings.ChartStrataTitle;

            tblockChartTitle.Text = ColumnChartParameters.ChartTitle;
            tblockSubTitle.Text = ColumnChartParameters.ChartSubTitle;
            tblockStrataTitle.Text = ColumnChartParameters.ChartStrataTitle;
            //--ei59
            tblockGadgetTitle.Text = ColumnChartParameters.GadgetTitle;
            //--
            //if (ColumnChartParameters.StrataVariableNames.Count > 0)
            //{
            //    tblockStrataTitle.Text = ColumnChartParameters.StrataVariableNames[0].ToString();
            //}

            if (string.IsNullOrEmpty(tblockChartTitle.Text)) tblockChartTitle.Visibility = System.Windows.Visibility.Collapsed;
            else tblockChartTitle.Visibility = System.Windows.Visibility.Visible;

            if (string.IsNullOrEmpty(tblockSubTitle.Text)) tblockSubTitle.Visibility = System.Windows.Visibility.Collapsed;
            else tblockSubTitle.Visibility = System.Windows.Visibility.Visible;

            if (string.IsNullOrEmpty(tblockStrataTitle.Text)) tblockStrataTitle.Visibility = System.Windows.Visibility.Collapsed;
            else tblockStrataTitle.Visibility = System.Windows.Visibility.Visible;
            
            //yAxis.UseReferenceValue = Settings.UseRefValues;

            //series0.ShowPointAnnotations = ColumnChartSettings.ShowAnnotations;
            //series0.BarKind = ColumnChartSettings.BarKind;            

            //series1.ShowPointAnnotations = Settings.ShowAnnotationsY2;
            //series1.DashStyle = Settings.LineDashStyleY2;
            //series1.LineKind = Settings.LineKindY2;
            //series1.Thickness = Settings.Y2LineThickness;

            yAxis.UseReferenceValue = ColumnChartParameters.UseRefValues;

            series0.ShowPointAnnotations = ColumnChartParameters.ShowAnnotations;
            series0.BarKind = ColumnChartParameters.BarKind;

            series1.ShowPointAnnotations = ColumnChartParameters.Y2ShowAnnotations;
            series1.DashStyle = ColumnChartParameters.Y2LineDashStyle;
            series1.LineKind = ColumnChartParameters.Y2LineKind;
            series1.Thickness = ColumnChartParameters.Y2LineThickness;
           
            if (ColumnChartParameters.ShowLegendBorder == true) 
            {
                xyChart.Legend.BorderThickness = new Thickness(1);
            }
            else 
            {
                xyChart.Legend.BorderThickness = new Thickness(0);
            }

            //EI-98
            YAxisCoordinates.FontSize = ColumnChartParameters.YAxisFontSize;
            xAxisCoordinates.FontSize = ColumnChartParameters.XAxisFontSize;
                                        
            tblockXAxisLabel.FontSize = ColumnChartParameters.XAxisLabelFontSize;
            tblockYAxisLabel.FontSize = ColumnChartParameters.YAxisLabelFontSize;
        }

        private void xyChart_DataStructureCreated(object sender, EventArgs e)
        {
            if (ColumnChartParameters.Y2AxisType == 3)
            {
                NumericCoordinates nc = new NumericCoordinates() { From = 0, To = 1, Step = 0.1 };
                Y2AxisCoordinates.Coordinates = nc;
            }

            SetLegendItems();
        }
    }
}
