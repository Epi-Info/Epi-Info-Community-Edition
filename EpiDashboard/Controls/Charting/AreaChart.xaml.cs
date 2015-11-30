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
    /// Interaction logic for AreaChart.xaml
    /// </summary>
    public partial class AreaChart : AreaChartBase
    {
        public AreaChartSettings AreaChartSettings { get; set; }

        public AreaChart(DashboardHelper dashboardHelper, AreaChartParameters parameters, List<XYColumnChartData> dataList)
        {
            InitializeComponent();
            //this.Settings = settings;
            //this.AreaChartSettings = settings;
            AreaChartParameters = parameters;
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

            xyChart.Width = AreaChartParameters.ChartWidth;
            xyChart.Height = AreaChartParameters.ChartHeight;

            //xyChart.Palette = Settings.Palette;
            switch (AreaChartParameters.Palette)
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
            if (AreaChartParameters.PaletteColors.Count() == 12)
            {
                ComponentArt.Win.DataVisualization.Palette CorpColorPalette = new ComponentArt.Win.DataVisualization.Palette();
                CorpColorPalette.PaletteName = "CorpColorPalette";
                /////////////////////////////////////
                CorpColorPalette.ChartingDataPoints12 = new Object();
                var NewPalette12 = (IList)xyChart.Palette.ChartingDataPoints12;
                for (int j = 0; j < AreaChartParameters.PaletteColors.Count(); j++)
                {
                    NewPalette12[j] = (Color)ColorConverter.ConvertFromString(AreaChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints12 = (Object)NewPalette12;
                xyChart.Palette.ChartingDataPoints12 = CorpColorPalette.ChartingDataPoints12;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints11 = new Object();
                var NewPalette11 = (IList)xyChart.Palette.ChartingDataPoints11;
                for (int j = 0; j < AreaChartParameters.PaletteColors.Count() - 1; j++)
                {
                    NewPalette11[j] = (Color)ColorConverter.ConvertFromString(AreaChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints11 = (Object)NewPalette11;
                xyChart.Palette.ChartingDataPoints11 = CorpColorPalette.ChartingDataPoints11;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints10 = new Object();
                var NewPalette10 = (IList)xyChart.Palette.ChartingDataPoints10;
                for (int j = 0; j < AreaChartParameters.PaletteColors.Count() - 2; j++)
                {
                    NewPalette10[j] = (Color)ColorConverter.ConvertFromString(AreaChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints10 = (Object)NewPalette10;
                xyChart.Palette.ChartingDataPoints10 = CorpColorPalette.ChartingDataPoints10;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints9 = new Object();
                var NewPalette9 = (IList)xyChart.Palette.ChartingDataPoints9;
                for (int j = 0; j < AreaChartParameters.PaletteColors.Count() - 3; j++)
                {
                    NewPalette9[j] = (Color)ColorConverter.ConvertFromString(AreaChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints9 = (Object)NewPalette9;
                xyChart.Palette.ChartingDataPoints9 = CorpColorPalette.ChartingDataPoints9;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints8 = new Object();
                var NewPalette8 = (IList)xyChart.Palette.ChartingDataPoints8;
                for (int j = 0; j < AreaChartParameters.PaletteColors.Count() - 4; j++)
                {
                    NewPalette8[j] = (Color)ColorConverter.ConvertFromString(AreaChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints8 = (Object)NewPalette8;
                xyChart.Palette.ChartingDataPoints8 = CorpColorPalette.ChartingDataPoints8;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints7 = new Object();
                var NewPalette7 = (IList)xyChart.Palette.ChartingDataPoints7;
                for (int j = 0; j < AreaChartParameters.PaletteColors.Count() - 5; j++)
                {
                    NewPalette7[j] = (Color)ColorConverter.ConvertFromString(AreaChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints7 = (Object)NewPalette7;
                xyChart.Palette.ChartingDataPoints7 = CorpColorPalette.ChartingDataPoints7;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints6 = new Object();
                var NewPalette6 = (IList)xyChart.Palette.ChartingDataPoints6;
                for (int j = 0; j < AreaChartParameters.PaletteColors.Count() - 6; j++)
                {
                    NewPalette6[j] = (Color)ColorConverter.ConvertFromString(AreaChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints6 = (Object)NewPalette6;
                xyChart.Palette.ChartingDataPoints6 = CorpColorPalette.ChartingDataPoints6;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints5 = new Object();
                var NewPalette5 = (IList)xyChart.Palette.ChartingDataPoints5;
                for (int j = 0; j < AreaChartParameters.PaletteColors.Count() - 7; j++)
                {
                    NewPalette5[j] = (Color)ColorConverter.ConvertFromString(AreaChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints5 = (Object)NewPalette5;
                xyChart.Palette.ChartingDataPoints5 = CorpColorPalette.ChartingDataPoints5;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints4 = new Object();
                var NewPalette4 = (IList)xyChart.Palette.ChartingDataPoints4;
                for (int j = 0; j < AreaChartParameters.PaletteColors.Count() - 8; j++)
                {
                    NewPalette4[j] = (Color)ColorConverter.ConvertFromString(AreaChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints4 = (Object)NewPalette4;
                xyChart.Palette.ChartingDataPoints4 = CorpColorPalette.ChartingDataPoints4;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints3 = new Object();
                var NewPalette3 = (IList)xyChart.Palette.ChartingDataPoints3;
                for (int j = 0; j < AreaChartParameters.PaletteColors.Count() - 9; j++)
                {
                    NewPalette3[j] = (Color)ColorConverter.ConvertFromString(AreaChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints3 = (Object)NewPalette3;
                xyChart.Palette.ChartingDataPoints3 = CorpColorPalette.ChartingDataPoints3;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints2 = new Object();
                var NewPalette2 = (IList)xyChart.Palette.ChartingDataPoints2;
                for (int j = 0; j < AreaChartParameters.PaletteColors.Count() - 10; j++)
                {
                    NewPalette2[j] = (Color)ColorConverter.ConvertFromString(AreaChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints2 = (Object)NewPalette2;
                xyChart.Palette.ChartingDataPoints2 = CorpColorPalette.ChartingDataPoints2;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints1 = new Object();
                var NewPalette1 = (IList)xyChart.Palette.ChartingDataPoints1;
                for (int j = 0; j < AreaChartParameters.PaletteColors.Count() - 11; j++)
                {
                    NewPalette1[j] = (Color)ColorConverter.ConvertFromString(AreaChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints1 = (Object)NewPalette1;
                xyChart.Palette.ChartingDataPoints1 = CorpColorPalette.ChartingDataPoints1;
            }
            xyChart.DefaultGridLinesVisible = AreaChartParameters.ShowGridLines;
            xyChart.LegendDock = AreaChartParameters.LegendDock;


            series0.BarRelativeBegin = double.NaN;
            series0.BarRelativeEnd = double.NaN;

            //series0.LineKind = AreaChartSettings.AreaType;
            series0.LineKind = AreaChartParameters.AreaKind;

            //tblockChartTitle.Text = Settings.ChartTitle;
            //tblockSubTitle.Text = Settings.ChartSubTitle;
            //tblockStrataTitle.Text = Settings.ChartStrataTitle;
            tblockChartTitle.Text = AreaChartParameters.ChartTitle;
            tblockSubTitle.Text = AreaChartParameters.ChartSubTitle;
            tblockStrataTitle.Text = AreaChartParameters.ChartStrataTitle;

            if (string.IsNullOrEmpty(tblockChartTitle.Text)) tblockChartTitle.Visibility = System.Windows.Visibility.Collapsed;
            else tblockChartTitle.Visibility = System.Windows.Visibility.Visible;

            if (string.IsNullOrEmpty(tblockSubTitle.Text)) tblockSubTitle.Visibility = System.Windows.Visibility.Collapsed;
            else tblockSubTitle.Visibility = System.Windows.Visibility.Visible;

            if (string.IsNullOrEmpty(tblockStrataTitle.Text)) tblockStrataTitle.Visibility = System.Windows.Visibility.Collapsed;
            else tblockStrataTitle.Visibility = System.Windows.Visibility.Visible;

            //yAxis.UseReferenceValue = Settings.UseRefValues;
            //series0.SeriesLineVisible = AreaChartSettings.ShowSeriesLine;
            yAxis.UseReferenceValue = AreaChartParameters.UseRefValues;
            series0.SeriesLineVisible = AreaChartParameters.ShowSeriesLine;

            //xAxisCoordinates.Angle = Settings.XAxisLabelRotation;
            xAxisCoordinates.Angle = AreaChartParameters.XAxisAngle;

            //series0.GradientTopColorShift = new ComponentArt.Win.DataVisualization.Common.ColorShift("A-" + AreaChartSettings.TransparencyTop.ToString());
            //series0.GradientBottomColorShift = new ComponentArt.Win.DataVisualization.Common.ColorShift("A-" + AreaChartSettings.TransparencyBottom.ToString());
            series0.GradientTopColorShift = new ComponentArt.Win.DataVisualization.Common.ColorShift("A-" + AreaChartParameters.TransTop.ToString());
            series0.GradientBottomColorShift = new ComponentArt.Win.DataVisualization.Common.ColorShift("A-" + AreaChartParameters.TransBottom.ToString());

            //switch ((EpiDashboard.XAxisLabelType)Settings.XAxisLabelType)
            //{
            //    case XAxisLabelType.Custom:
            //    case XAxisLabelType.FieldPrompt:
            //        tblockXAxisLabel.Text = AreaChartParameters.XAxisLabel;
            //        tblockXAxisLabel.Text = AreaChartParameters.XAxisLabel;
            //        break;
            //    case XAxisLabelType.None:
            //        tblockXAxisLabel.Text = string.Empty;
            //        break;
            //    default:
            //        tblockXAxisLabel.Text = AreaChartParameters.XAxisLabel;
            //        break;
            //}
            switch (AreaChartParameters.XAxisLabelType)
            {
                default:
                case 0:  //Automatic
                    if (!String.IsNullOrEmpty(AreaChartParameters.XAxisLabel))
                    {
                        tblockXAxisLabel.Text = AreaChartParameters.XAxisLabel;
                    }
                    else
                    {
                        tblockXAxisLabel.Text = AreaChartParameters.ColumnNames[0];
                    }
                    break;
                case 1:  //Field Prompt
                    {
                        Field field = DashboardHelper.GetAssociatedField(AreaChartParameters.ColumnNames[0]);
                        if (field != null)
                        {
                            RenderableField rField = field as RenderableField;
                            tblockXAxisLabel.Text = rField.PromptText;
                        }
                        else
                        {
                            tblockXAxisLabel.Text = AreaChartParameters.ColumnNames[0];
                        }
                    }
                    break;
                case 2:  //None
                    tblockXAxisLabel.Text = string.Empty;
                    break;
                case 3:  //Custom
                    tblockXAxisLabel.Text = AreaChartParameters.XAxisLabel;
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

            if (!string.IsNullOrEmpty(AreaChartParameters.YAxisFormat.Trim()))
            {
                YAxisCoordinates.FormattingString = AreaChartParameters.YAxisFormat;
            }
            if (!string.IsNullOrEmpty(AreaChartParameters.YAxisFormat.Trim()))
            {
                Y2AxisCoordinates.FormattingString = AreaChartParameters.YAxisFormat;
            }

            //YAxisLabel = Settings.YAxisLabel;
            //Y2AxisLabel = Settings.Y2AxisLabel;
            //Y2AxisLegendTitle = Settings.Y2AxisLegendTitle;
            YAxisLabel = AreaChartParameters.YAxisLabel;
            Y2AxisLabel = AreaChartParameters.Y2AxisLabel;
            Y2AxisLegendTitle = AreaChartParameters.Y2AxisLegendTitle;

            //xyChart.CompositionKind = AreaChartSettings.Composition;
            xyChart.CompositionKind = AreaChartParameters.Composition;

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

            //xyChart.LegendVisible = Settings.ShowLegend;
            //if (Settings.ShowLegendBorder == true)
            xyChart.LegendVisible = AreaChartParameters.ShowLegend;
            if (AreaChartParameters.ShowLegendBorder == true)
            {
                xyChart.Legend.BorderThickness = new Thickness(1);
            }
            else
            {
                xyChart.Legend.BorderThickness = new Thickness(0);
            }

            //series1.ShowPointAnnotations = Settings.ShowAnnotationsY2;
            //series1.DashStyle = Settings.LineDashStyleY2;
            //series1.LineKind = Settings.LineKindY2;
            //series1.Thickness = Settings.Y2LineThickness;

            //xyChart.Legend.FontSize = Settings.LegendFontSize;
            series1.ShowPointAnnotations = AreaChartParameters.Y2ShowAnnotations;
            series1.DashStyle = AreaChartParameters.Y2LineDashStyle;
            series1.LineKind = AreaChartParameters.Y2LineKind;
            series1.Thickness = AreaChartParameters.Y2LineThickness;

            xyChart.Legend.FontSize = AreaChartParameters.LegendFontSize;

            //EI-98
            YAxisCoordinates.FontSize = AreaChartParameters.YAxisFontSize;
            xAxisCoordinates.FontSize = AreaChartParameters.XAxisFontSize;

            tblockXAxisLabel.FontSize = AreaChartParameters.XAxisLabelFontSize;
            tblockYAxisLabel.FontSize = AreaChartParameters.YAxisLabelFontSize;

        }

        private void xyChart_DataStructureCreated(object sender, EventArgs e)
        {
            SetLegendItems();
        }
    }
}
