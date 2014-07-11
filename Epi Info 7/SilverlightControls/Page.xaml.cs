using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Browser;
using System.Windows.Controls.DataVisualization.Charting;
using System.ComponentModel;

namespace SilverlightControls
{
    public partial class Page : UserControl
    {
        string _chartTitle = string.Empty;
        string _chartType = string.Empty;
        string _chartLineSeries = string.Empty;
        string _independentLabel = string.Empty;
        string _dependentLabel = string.Empty;
        string _failOverMessage = string.Empty;
        object _maxIndependentValue;
        bool _maxIndependentValueIsValid = false;
        object _minIndependentValue;
        bool _minIndependentValueIsValid = false;

        Style _rotateStyle = new Style(typeof(CategoryAxis));

        public Page()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(Page_Loaded);
        }

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RotateTransform rotateTransform = new RotateTransform();
            rotateTransform.Angle = 0.90;

            _rotateStyle.Setters.Add(new Setter(CategoryAxis.RenderTransformProperty, rotateTransform));

            if (App.Current.Resources.Contains("chartTitle"))
            {
                _chartTitle = App.Current.Resources["chartTitle"].ToString();
                Chart.Title = _chartTitle;
            }
            if (App.Current.Resources.Contains("independentLabel"))
            {
                _independentLabel = App.Current.Resources["independentLabel"].ToString();
            }

            if (App.Current.Resources.Contains("dependentLabel"))
            {
                _dependentLabel = App.Current.Resources["dependentLabel"].ToString();
            }

            if (App.Current.Resources.Contains("chartLineSeries"))
            {
                _chartLineSeries = App.Current.Resources["chartLineSeries"].ToString();
                _chartLineSeries = _chartLineSeries.Replace(Statics.Comma, ",");
            }

            if (App.Current.Resources.Contains("chartType"))
            {
                _chartType = App.Current.Resources["chartType"].ToString();
                List<object> objectList = new List<object>();

                if (ProcessSeriesData(ref objectList, _chartType, _chartLineSeries, ref _failOverMessage))
                {
                    try
                    {
                        switch (_chartType.ToUpper().Replace(" ", ""))
                        {
                            case Statics.Area:
                                foreach (object line in objectList)
                                {
                                    Chart.Series.Add(line as AreaSeries);
                                }
                                break;

                            case Statics.Bar:

                                foreach (object columnSeries in objectList)
                                {
                                    #region REM rotate label candidate
                                    //if (independentValue.GetType() == Type.GetType("System.String"))
                                    //{
                                    //    //Style style = new Style(typeof(CategoryAxis));
                                    //    //RotateTransform rotateTransform = new RotateTransform();
                                    //    //rotateTransform.Angle = 0.90;

                                    //    //style.Setters.Add(new Setter(CategoryAxis.RenderTransformProperty, rotateTransform)); 

                                    //    //((ColumnSeries)series).IndependentAxis = new CategoryAxis
                                    //    //{
                                    //    //    Title = _independentLabel,
                                    //    //    Orientation = AxisOrientation.X,
                                    //    //    AxisLabelStyle = _rotateStyle
                                    //    //};

                                    //    ((ColumnSeries)series).IndependentAxis = new CategoryAxis
                                    //    {
                                    //        Title = _independentLabel,
                                    //        Orientation = AxisOrientation.X
                                    //    };
                                    //}
                                    //else if (independentValue.GetType() == Type.GetType("System.DateTime"))
                                    //{
                                    //    ((ColumnSeries)series).IndependentAxis = new DateTimeAxis
                                    //    {
                                    //        Title = _independentLabel,
                                    //        Orientation = AxisOrientation.X
                                    //    };
                                    //}
                                    //else
                                    //{
                                    //    ((ColumnSeries)series).IndependentAxis = new LinearAxis
                                    //    {
                                    //        Title = _independentLabel,
                                    //        Orientation = AxisOrientation.X
                                    //    };
                                    //}
                                    #endregion
                                    Chart.Series.Add(columnSeries as ColumnSeries);
                                }
                                #region REM rotate label candidate
                                //((ColumnSeries)objectList[0] as ColumnSeries).DependentRangeAxis = new LinearAxis
                                //{
                                //    Title = _dependentLabel,
                                //    ShowGridLines = true,
                                //    Orientation = AxisOrientation.Y
                                //};
                                #endregion
                                break;

                            case Statics.Bubble:
                                foreach (object line in objectList)
                                {
                                    Chart.Series.Add(line as BubbleSeries);
                                }
                                break;

                            case Statics.RotatedBar:
                                foreach (object line in objectList)
                                {
                                    Chart.Series.Add(line as BarSeries);
                                }
                                break;

                            case Statics.Histogram:
                                foreach (object line in objectList)
                                {
                                    System.Windows.Controls.DataVisualization.ResourceDictionaryCollection pallete = new ResourceDictionaryCollection();
                                    ResourceDictionary rd = new ResourceDictionary();
                                    Style style = new Style();
                                    style.TargetType = typeof(ColumnDataPoint);

                                    style.Setters.Clear();
                                    //style.Setters.Add(new Setter() { Property = ColumnDataPoint.BackgroundProperty, Value = new SolidColorBrush(Colors.Orange) });
                                    style.Setters.Add(new Setter() { Property = ColumnDataPoint.MarginProperty, Value = new Thickness(0) });
                                    style.Setters.Add(new Setter() { Property = ColumnDataPoint.PaddingProperty, Value = new Thickness(0, 0, 0, 0) });
                                    style.Setters.Add(new Setter() { Property = ColumnDataPoint.MinWidthProperty, Value = 256 });

                                    rd.Clear();
                                    rd.Add("DataPointStyle", style);
                                    pallete.Clear();
                                    pallete.Add(rd);
                                    Chart.Palette = pallete;

                                    Chart.Series.Add(line as ColumnSeries);
                                }
                                break;

                            case Statics.Line:
                                foreach (object line in objectList)
                                {
                                    Chart.Series.Add(line as LineSeries);
                                }
                                break;

                            case Statics.Pie:
                                foreach (object column in objectList)
                                {
                                    Chart.Series.Add(column as PieSeries);
                                }
                                break;

                            case Statics.Scatter:
                                foreach (object line in objectList)
                                {
                                    Chart.Series.Add(line as ScatterSeries);
                                }
                                break;

                            case Statics.Stacked:
                                foreach (object line in objectList)
                                {
                                    Chart.Series.Add(line as StackedBarSeries);
                                }
                                break;

                            default:
                                _failOverMessage = "The specified graph type supplied in the input parameters (initParams) could not be parsed or is not supported.";
                                break;
                        }
                    }
                    catch (Exception exception)
                    {
                        _failOverMessage = exception.Message;
                    }
                }
                else
                {
                    if (_failOverMessage == string.Empty)
                    {
                        _failOverMessage = "The input parameters (initParams) could not be parsed.";
                    }
                }

                if (_failOverMessage.Length > 0)
                {
                    Chart.Visibility = Visibility.Collapsed;
                    FailOverMessage.Text = _failOverMessage;
                }
            }
        }

        bool ProcessSeriesData(ref List<object> listOfSeries, string chartType, string seriesDataString, ref string failOverMessage)
        {
            bool parsed = false;
            bool isSingleSeries = false;

            List<ColumnSeries> columnSeriesList = new List<ColumnSeries>();

            string[] seriesDataStringArray = seriesDataString.Split(new string[] { Statics.SeparateLineSeries }, StringSplitOptions.RemoveEmptyEntries);

            if (seriesDataStringArray.Length < 1)
            {
                isSingleSeries = true;
            }

            foreach (string lineSeriesItemString in seriesDataStringArray)
            {
                object series;
                switch (chartType.ToUpper().Replace(" ",""))
                {
                    case Statics.Area:
                        series = new AreaSeries();
                        ((AreaSeries)series).IndependentValuePath = "Key";
                        ((AreaSeries)series).DependentValuePath = "Value";
                        break;

                    case Statics.Bar:
                        series = new ColumnSeries();
                        ((ColumnSeries)series).IndependentValuePath = "Key";
                        ((ColumnSeries)series).DependentValuePath = "Value";
                        break;

                    case Statics.Bubble:
                        series = new BubbleSeries();
                        ((BubbleSeries)series).IndependentValuePath = "Key";
                        ((BubbleSeries)series).DependentValuePath = "Value";
                        break;

                    case Statics.RotatedBar:
                        series = new BarSeries();
                        ((BarSeries)series).IndependentValuePath = "Key";
                        ((BarSeries)series).DependentValuePath = "Value";
                        break;

                    case Statics.Histogram:
                        series = new ColumnSeries();
                        ((ColumnSeries)series).IndependentValuePath = "Key";
                        ((ColumnSeries)series).DependentValuePath = "Value";
                        break;

                    case Statics.Line:
                        series = new LineSeries();
                        ((LineSeries)series).IndependentValuePath = "Key";
                        ((LineSeries)series).DependentValuePath = "Value";
                        break;

                    case Statics.Pie:
                        series = new PieSeries();
                        ((PieSeries)series).IndependentValuePath = "Key";
                        ((PieSeries)series).DependentValuePath = "Value";
                        break;

                    case Statics.Scatter:
                        series = new ScatterSeries();
                        ((ScatterSeries)series).IndependentValuePath = "Key";
                        ((ScatterSeries)series).DependentValuePath = "Value";
                        break;

                    case Statics.Stacked:
                        //series = new StackedBarSeries();
                        //((StackedBarSeries)series).IndependentValuePath = "Key";
                        //((StackedBarSeries)series).DependentValuePath = "Value";

                    case Statics.TreeMap:
                    default:
                        failOverMessage = "The specified graph type supplied in the input parameters (initParams) could not be parsed.";
                        return false;
                }

                string[] titleSplit = lineSeriesItemString.Split(new string[] { Statics.LineSeriesTitle }, StringSplitOptions.None);

                if (titleSplit.Length == 3)
                {
                    switch (chartType.ToUpper().Replace(" ", ""))
                    {
                        case Statics.Area:
                            ((AreaSeries)series).Title = titleSplit[1];
                            break;

                        case Statics.Bar:
                            ((ColumnSeries)series).Title = titleSplit[1];
                            break;

                        case Statics.Bubble:
                            ((BubbleSeries)series).Title = titleSplit[1];
                            break;

                        case Statics.RotatedBar:
                            ((BarSeries)series).Title = titleSplit[1];
                            break;

                        case Statics.Histogram:
                            ((ColumnSeries)series).Title = titleSplit[1];
                            break;

                        case Statics.Line:
                            ((LineSeries)series).Title = titleSplit[1];
                            break;

                        case Statics.Pie:
                            ((PieSeries)series).Title = titleSplit[1];
                            break;

                        case Statics.Scatter:
                            ((ScatterSeries)series).Title = titleSplit[1];
                            break;

                        case Statics.Stacked:
                        case Statics.TreeMap:
                        default:
                            break;
                    }
                }

                Dictionary<object, object> pointList = new Dictionary<object, object>();

                string[] dataSplit = lineSeriesItemString.Split(new string[] { Statics.LineSeriesDataString }, StringSplitOptions.None);

                object independentValue = string.Empty;
                object dependentValue = 0.0;

                if (dataSplit.Length == 3)
                {
                    string dataString = dataSplit[1];

                    string[] dataPairStringArray = dataString.Split(new string[] { Statics.SeparateDataPoints }, StringSplitOptions.None);

                    foreach (string pair in dataPairStringArray)
                    {
                        if (pair.Length > 0)
                        {
                            string[] set = pair.Split(new string[] { Statics.SeparateIndDepValues }, StringSplitOptions.None);

                            try
                            {
                                if (set.Length == 2)
                                {
                                    Double doubleCandidate;
                                    DateTime dateTimeCandidate;

                                    // < independent >
                                    if (Double.TryParse(set[0], out doubleCandidate))
                                    {
                                        independentValue = doubleCandidate;

                                        if (_minIndependentValueIsValid)
                                        {
                                            if ((double)independentValue < (double)_minIndependentValue)
                                            {
                                                _minIndependentValue = independentValue;
                                            }
                                        }
                                        else
                                        {
                                            _minIndependentValue = independentValue;
                                            _minIndependentValueIsValid = true;
                                        }

                                        if (_maxIndependentValueIsValid)
                                        {
                                            if ((double)independentValue > (double)_maxIndependentValue)
                                            {
                                                _maxIndependentValue = independentValue;
                                            }
                                        }
                                        else
                                        {
                                            _maxIndependentValue = independentValue;
                                            _maxIndependentValueIsValid = true;
                                        }
                                    }
                                    else if (DateTime.TryParse(set[0], out dateTimeCandidate))
                                    {
                                        independentValue = dateTimeCandidate;

                                        if (_minIndependentValueIsValid)
                                        {
                                            if ((DateTime)independentValue < (DateTime)_minIndependentValue)
                                            {
                                                _minIndependentValue = independentValue;
                                            }
                                        }
                                        else
                                        {
                                            _minIndependentValue = independentValue;
                                            _minIndependentValueIsValid = true;
                                        }

                                        if (_maxIndependentValueIsValid)
                                        {
                                            if ((DateTime)independentValue > (DateTime)_maxIndependentValue)
                                            {
                                                _maxIndependentValue = independentValue;
                                            }
                                        }
                                        else
                                        {
                                            _maxIndependentValue = independentValue;
                                            _maxIndependentValueIsValid = true;
                                        }
                                    }
                                    else
                                    {
                                        independentValue = set[0].ToString();
                                    }

                                    // < dependent >
                                    if (Double.TryParse(set[1], out doubleCandidate))
                                    {
                                        dependentValue = doubleCandidate;
                                    }
                                    else if (DateTime.TryParse(set[1], out dateTimeCandidate))
                                    {
                                        dependentValue = dateTimeCandidate;
                                    }
                                    else
                                    {
                                        dependentValue = set[1].ToString();
                                    }

                                    pointList.Add(independentValue, dependentValue);
                                }
                            }
                            catch
                            {
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    failOverMessage = string.Format("Parse fail with '{0}'", lineSeriesItemString);
                    return false;
                }

                AddItemSourceToSeries(chartType, series, pointList);

                if (isSingleSeries)
                {
                    AddAxesToSeries(chartType, series, independentValue);
                }
                else
                {
                    //((ColumnSeries)series).IndependentAxis = new LinearAxis
                    //{
                    //    Title = _independentLabel,
                    //    Orientation = AxisOrientation.X,
                    //    Minimum = (double)_minIndependentValue,
                    //    Maximum = (double)_maxIndependentValue
                    //};
                }

                listOfSeries.Add(series);
                parsed = true;
            }
            return parsed;
        }

        private void AddAxesToSeries(string chartType, object series, object independentValue)
        {
            switch (chartType)
            {
                case Statics.Line:
                    break;
                case Statics.Pie:
                    break;
                case Statics.Bar:
                default:
                    if (independentValue.GetType() == Type.GetType("System.String"))
                    {
                        #region rem rotate label candidate
                        //Style style = new Style(typeof(CategoryAxis));
                        //RotateTransform rotateTransform = new RotateTransform();
                        //rotateTransform.Angle = 0.90;
                        //style.Setters.Add(new Setter(CategoryAxis.RenderTransformProperty, rotateTransform)); 
                        //((ColumnSeries)series).IndependentAxis = new CategoryAxis
                        //{
                        //    Title = _independentLabel,
                        //    Orientation = AxisOrientation.X,
                        //    AxisLabelStyle = _rotateStyle
                        //};
                        #endregion

                        ((ColumnSeries)series).IndependentAxis = new CategoryAxis
                        {
                            Title = _independentLabel,
                            Orientation = AxisOrientation.X
                        };
                    }
                    else if (independentValue.GetType() == Type.GetType("System.DateTime"))
                    {
                        ((ColumnSeries)series).IndependentAxis = new DateTimeAxis
                        {
                            Title = _independentLabel,
                            Orientation = AxisOrientation.X
                        };
                    }
                    else
                    {
                        ((ColumnSeries)series).IndependentAxis = new LinearAxis
                        {
                            Title = _independentLabel,
                            Orientation = AxisOrientation.X
                        };
                    }

                    ((ColumnSeries)series).DependentRangeAxis = new LinearAxis
                    {
                        Title = _dependentLabel,
                        ShowGridLines = true,
                        Orientation = AxisOrientation.Y
                    };

                    break;
            }
        }

        private static void AddItemSourceToSeries(string chartType, object series, Dictionary<object, object> pointList)
        {
            switch (chartType.ToUpper().Replace(" ", ""))
            {
                case Statics.Area:
                    ((AreaSeries)series).ItemsSource = (KeyValuePair<object, object>[])pointList.ToArray();
                    break;

                case Statics.Bar:
                    ((ColumnSeries)series).ItemsSource = (KeyValuePair<object, object>[])pointList.ToArray();
                    break;

                case Statics.Bubble:
                    ((BubbleSeries)series).ItemsSource = (KeyValuePair<object, object>[])pointList.ToArray();
                    break;

                case Statics.RotatedBar:
                    ((BarSeries)series).ItemsSource = (KeyValuePair<object, object>[])pointList.ToArray();
                    break;

                case Statics.Histogram:
                    ((ColumnSeries)series).ItemsSource = (KeyValuePair<object, object>[])pointList.ToArray();
                    break;

                case Statics.Line:
                    ((LineSeries)series).ItemsSource = (KeyValuePair<object, object>[])pointList.ToArray();
                    break;

                case Statics.Pie:
                    ((PieSeries)series).ItemsSource = (KeyValuePair<object, object>[])pointList.ToArray();
                    break;

                case Statics.Scatter:
                    ((ScatterSeries)series).ItemsSource = (KeyValuePair<object, object>[])pointList.ToArray();
                    break;

                case Statics.Stacked:
                case Statics.TreeMap:
                default:
                    ((ColumnSeries)series).ItemsSource = (KeyValuePair<object, object>[])pointList.ToArray();
                    break;
            }
        }
    }
}
