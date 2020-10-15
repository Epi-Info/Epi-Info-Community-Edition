using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ComponentArt.Win.DataVisualization.Charting;
using EpiDashboard;
using EpiDashboard.Gadgets.Charting;
using System.IO;
using System.Globalization;

namespace EpiDashboard.Controls.Charting
{
    public class ChartBase : UserControl, IChart
    {
        public IChartSettings Settings { get; set; }
        public GadgetParameters Parameters { get; set; }
        public DashboardHelper DashboardHelper { get; set; }
       
        public string ChartTitle
        {
            get
            {
                TextBlock tblockChartTitle = null;
                object el = FindName("tblockChartTitle");
                if (el is TextBlock)
                {
                    tblockChartTitle = el as TextBlock;
                }

                if (tblockChartTitle == null) return string.Empty;

                return tblockChartTitle.Text;
            }
            set
            {
                TextBlock tblockChartTitle = null;
                object el = FindName("tblockChartTitle");
                if (el is TextBlock)
                {
                    tblockChartTitle = el as TextBlock;
                }

                if (tblockChartTitle == null) return;

                tblockChartTitle.Text = value;
                if (string.IsNullOrEmpty(value.Trim()))
                {
                    tblockChartTitle.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    tblockChartTitle.Visibility = System.Windows.Visibility.Visible;
                }
            }

        }
        
        public string SubTitle
        {
            get
            {
                TextBlock tblockSubtitle = null;
                object el = FindName("tblockSubTitle");
                if (el is TextBlock)
                {
                    tblockSubtitle = el as TextBlock;
                }

                if (tblockSubtitle == null) return string.Empty;

                return tblockSubtitle.Text;
            }
            set
            {
                TextBlock tblockSubtitle = null;
                object el = FindName("tblockSubTitle");
                if (el is TextBlock)
                {
                    tblockSubtitle = el as TextBlock;
                }

                if (tblockSubtitle == null) return;

                tblockSubtitle.Text = value;
                if (string.IsNullOrEmpty(value.Trim()))
                {
                    tblockSubtitle.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    tblockSubtitle.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }
        public string StrataTitle
        {
            get
            {
                TextBlock tblockStrataTitle = null;
                object el = FindName("tblockStrataTitle");
                if (el is TextBlock)
                {
                    tblockStrataTitle = el as TextBlock;
                }

                if (tblockStrataTitle == null) return string.Empty;

                return tblockStrataTitle.Text;
            }
            set
            {
                TextBlock tblockStrataTitle = null;
                object el = FindName("tblockStrataTitle");
                if (el is TextBlock)
                {
                    tblockStrataTitle = el as TextBlock;
                }

                if (tblockStrataTitle == null) return;

                tblockStrataTitle.Text = value;
                if (string.IsNullOrEmpty(value.Trim()))
                {
                    tblockStrataTitle.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    tblockStrataTitle.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        public XYChart Chart
        {
            get 
            {
                XYChart xyChart = null;
                object el = FindName("xyChart");
                if (el is XYChart)
                {
                    xyChart = el as XYChart;
                }

                return xyChart; 
            }
        }

        public string YAxisLabel
        {
            get
            {
                TextBlock tblockYAxisLabel = null;
                object el = FindName("tblockYAxisLabel");
                if (el is TextBlock)
                {
                    tblockYAxisLabel = el as TextBlock;
                }

                if (tblockYAxisLabel == null) return string.Empty;

                return tblockYAxisLabel.Text;
            }
            set
            {
                TextBlock tblockYAxisLabel = null;
                object el = FindName("tblockYAxisLabel");
                if (el is TextBlock)
                {
                    tblockYAxisLabel = el as TextBlock;
                }

                if (tblockYAxisLabel == null) return;

                tblockYAxisLabel.Text = value;
                if (string.IsNullOrEmpty(value.Trim()))
                {
                    tblockYAxisLabel.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    tblockYAxisLabel.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        public string XAxisLabel
        {
            get
            {
                TextBlock tblockXAxisLabel = null;
                object el = FindName("tblockXAxisLabel");
                if (el is TextBlock)
                {
                    tblockXAxisLabel = el as TextBlock;
                }

                if (tblockXAxisLabel == null) return string.Empty;

                return tblockXAxisLabel.Text;
            }
            set
            {
                TextBlock tblockXAxisLabel = null;
                object el = FindName("tblockXAxisLabel");
                if (el is TextBlock)
                {
                    tblockXAxisLabel = el as TextBlock;
                }

                if (tblockXAxisLabel == null) return;

                tblockXAxisLabel.Text = value;
                if (string.IsNullOrEmpty(value.Trim()))
                {
                    tblockXAxisLabel.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    tblockXAxisLabel.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        public string Y2AxisLabel
        {
            get
            {
                TextBlock tblockY2AxisLabel = null;
                object el = FindName("tblockY2AxisLabel");
                if (el is TextBlock)
                {
                    tblockY2AxisLabel = el as TextBlock;
                }

                if (tblockY2AxisLabel == null) return string.Empty;

                return tblockY2AxisLabel.Text;
            }
            set
            {
                TextBlock tblockY2AxisLabel = null;
                object el = FindName("tblockY2AxisLabel");
                if (el is TextBlock)
                {
                    tblockY2AxisLabel = el as TextBlock;
                }

                if (tblockY2AxisLabel == null) return;

                tblockY2AxisLabel.Text = value;
                if (string.IsNullOrEmpty(value.Trim()))
                {
                    tblockY2AxisLabel.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    tblockY2AxisLabel.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        public string Y2AxisLegendTitle { get; set; }
            

        protected virtual void SetChartProperties()
        {
        }

        protected void MeasureAndArrange()
        {
            XYChart xyChart = null;
            object el = FindName("xyChart");
            if (el is XYChart)
            {
                xyChart = el as XYChart;
                Size size = new Size(xyChart.Width, xyChart.Height);
                // Measure and arrange the surface
                // VERY IMPORTANT
                xyChart.Measure(size);
                xyChart.Arrange(new Rect(size));
            }
        }

        protected virtual void SetChartData(List<XYColumnChartData> dataList)
        {
            #region Input Validation
            if (dataList == null)
            {
                throw new ArgumentNullException("dataList");                
            }
            #endregion // Input Validation

            Series series1 = null;
            object el = FindName("series1");
            if (el is Series)
            {
                series1 = el as Series;
            }

            XYChart xyChart = null;
            el = FindName("xyChart");
            if (el is XYChart)
            {
                xyChart = el as XYChart;
            }

            bool shouldShowY2 = false;

            foreach (XYColumnChartData xyc in dataList)
            {
                if (xyc.Y2.HasValue)
                {
                    shouldShowY2 = true;
                    break;
                }
            }

            if (shouldShowY2 && series1 != null)
            {
                series1.Visibility = System.Windows.Visibility.Visible;
            }
            xyChart.DataSource = dataList;            
        }

        protected void SetChartData(List<XYParetoChartData> dataList)
        {
            #region Input Validation
            if (dataList == null)
            {
                throw new ArgumentNullException("dataList");
            }
            #endregion // Input Validation

            Series series1 = null;
            object el = FindName("series1");
            if (el is Series)
            {
                series1 = el as Series;
            }

            XYChart xyChart = null;
            el = FindName("xyChart");
            if (el is XYChart)
            {
                xyChart = el as XYChart;
            }

            if (series1 != null)
            {
                series1.Visibility = System.Windows.Visibility.Visible;
            }
            xyChart.DataSource = dataList;
        }

        public virtual void CopyImageToClipboard()
        {
            BitmapSource img = ToBitmapSource(false); // renderBitmap;
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(img));
            Clipboard.SetImage(encoder.Frames[0]);
        }

        public virtual void CopyAllDataToClipboard()
        {
            object el = FindName("xyChart");
            if (el is XYChart)
            {                
                Clipboard.Clear();
                Clipboard.SetText(SendDataToString());
            }
        }

        public virtual string SendDataToString()
        {
            StringBuilder sb = new StringBuilder();            
            XYChart xyChart = null;
            object el = FindName("xyChart");
            if (el is XYChart)
            {
                xyChart = el as XYChart;
                if (xyChart.DataSource.GetType().ToString().Contains("XYParetoChartData"))
                {
                    List<XYParetoChartData> dataList = xyChart.DataSource as List<XYParetoChartData>;

                    sb.Append("X" + "\t");
                    sb.Append("Y1" + "\t");
                    sb.Append("Y2" + "\t");
                    sb.AppendLine();

                    foreach (XYParetoChartData chartData in dataList)
                    {
                        sb.Append(chartData.X + "\t");
                        sb.Append(chartData.Y + "\t");
                        sb.Append(chartData.Z + "\t");
                        sb.AppendLine();
                    }
                }
            else
            {
                List<XYColumnChartData> dataList = xyChart.DataSource as List<XYColumnChartData>;

                sb.Append("X" + "\t");
                sb.Append("Y" + "\t");
                sb.Append("S" + "\t");
                sb.AppendLine();

                foreach (XYColumnChartData chartData in dataList)
                {
                    sb.Append(chartData.X + "\t");
                    sb.Append(chartData.Y + "\t");
                    sb.Append(chartData.S + "\t");
                    sb.AppendLine();
                }     
            }
        }

            return sb.ToString();
        }

        protected void SendDataToExcel()
        {
            try
            {
                string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString("N") + ".html";//GetHTMLLineListing();

                System.IO.FileStream stream = System.IO.File.OpenWrite(fileName);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(stream);
                sw.WriteLine("<html><head>");
                sw.WriteLine("<meta http-equiv=\"content-type\" content=\"text/html;charset=UTF-8\" />");
                sw.WriteLine("<meta name=\"author\" content=\"Epi Info 7\" />");
                sw.WriteLine(DashboardHelper.GenerateStandardHTMLStyle().ToString());

                sw.WriteLine(this.ToHTML("", 0, false, true));
                sw.Close();
                sw.Dispose();

                if (!string.IsNullOrEmpty(fileName))
                {
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = "excel";
                    proc.StartInfo.Arguments = "\"" + fileName + "\"";
                    proc.StartInfo.UseShellExecute = true;
                    proc.Start();
                }
            }
            finally
            {
            }
        }

        public void SaveImageToFile()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG Image (.png)|*.png|JPEG Image (.jpg)|*.jpg";

            if (dlg.ShowDialog().Value)
            {
                ToImageFile(dlg.FileName);
                if ((ChartTitle.Length > 0) || (SubTitle.Length > 0)) { WriteTexttoImage(dlg.FileName); } //ei-59
                MessageBox.Show("Image saved successfully.", "Save Image", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
                
        public virtual string ToHTML(string htmlFileName = "", int count = 0, bool includeImage = true, bool includeFullData = false, bool ForWeb = false)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            StringBuilder _color = new StringBuilder();
            StringBuilder _Groups = new StringBuilder();
            string Composition = "";
            
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
            else
            {
               

                List<string> color = new List<string>();
                var ChartType = this.GetType().Name;

                if (ChartType == "LineChart")
                {
                    htmlBuilder.AppendLine("<h2  class=\"gadgetHeading\">" + ((EpiDashboard.Controls.Charting.LineChartBase)this).LineChartParameters.GadgetTitle + "</h2>");
                    color = ((EpiDashboard.Controls.Charting.LineChartBase)this).LineChartParameters.PaletteColors;
                }
                else if(ChartType  == "ParetoChart")
                {
                    htmlBuilder.AppendLine("<h2  class=\"gadgetHeading\">" + ((EpiDashboard.Controls.Charting.ParetoChart)this).ParetoChartParameters.GadgetTitle + "</h2>");
                    color = ((EpiDashboard.Controls.Charting.ParetoChart)this).ParetoChartParameters.PaletteColors;
                }
                else if (ChartType == "ColumnChart")
                {
                    htmlBuilder.AppendLine("<h2  class=\"gadgetHeading\">" + ((EpiDashboard.Controls.Charting.ColumnChartBase)this).ColumnChartParameters.GadgetTitle + "</h2>");
                    color = ((EpiDashboard.Controls.Charting.ColumnChartBase)this).ColumnChartParameters.PaletteColors;
                    Composition = ((EpiDashboard.Controls.Charting.ColumnChartBase)this).ColumnChartParameters.Composition.ToString();
                    bool UseDiffColors = ((EpiDashboard.Controls.Charting.ColumnChartBase)this).ColumnChartParameters.UseDiffColors;
                    if (UseDiffColors) {
                        _color.AppendLine(" var colors =  [");
                        for (int i = 0; i < color.Count(); i++)
                        {
                            if (color[i].Length == 9)
                            {
                                _color.AppendLine(" '" + color[i].Remove(1, 2) + "' ,");
                            }
                        }


                        _color.AppendLine(" ];");
                    }
                }
                else if (ChartType == "AreaChart")
                {
                    htmlBuilder.AppendLine("<h2  class=\"gadgetHeading\">" + ((EpiDashboard.Controls.Charting.AreaChartBase)this).AreaChartParameters.GadgetTitle + "</h2>");
                    color = ((EpiDashboard.Controls.Charting.AreaChartBase)this).AreaChartParameters.PaletteColors;
                } else if (ChartType == "HistogramChart")
                {
                    color = ((EpiDashboard.Controls.Charting.HistogramChartBase)this).HistogramChartParameters.PaletteColors;

                    htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + ((EpiDashboard.Controls.Charting.HistogramChartBase)this).HistogramChartParameters.GadgetTitle + "</h2>");

                }
                htmlBuilder.AppendLine("<div id=\"Linechart"+count+"\" style=\"float:left\"></div>");

                if (!string.IsNullOrEmpty(_color.ToString()))
                {
                   
                    htmlBuilder.AppendLine("<script>  " + _color.ToString() );
                }
                else
                {
                    htmlBuilder.AppendLine("<script> ");
                }

              

              

              
             
                htmlBuilder.AppendLine(" var Linechart"+ count + " = c3.generate({bindto: '#Linechart"+ count + "',data: {  columns: [");

                try{
                    try
                    {


                        if (ChartType == "HistogramChart")
                        {
                            List<EpiDashboard.Controls.Charting.HistogramChart.XYHistogramChartData> _HChartdataList = (List<EpiDashboard.Controls.Charting.HistogramChart.XYHistogramChartData>)this.Chart.DataSource;
                            var HChartGroups = _HChartdataList.GroupBy(x => x.X);

                            foreach (var DataItem in HChartGroups)
                            {
                                htmlBuilder.AppendLine("['" + DataItem.ToList()[0].X + "', ");
                                _Groups.AppendLine("'" + DataItem.ToList()[0].X + "', ");
                                foreach (var item in DataItem)
                                {
                                    htmlBuilder.AppendLine(item.Y + ", ");

                                }


                                htmlBuilder.AppendLine("],");
                            }

                            htmlBuilder.AppendLine("]");
                        }
                        else if (ChartType == "ParetoChart")
                        {
                            List<EpiDashboard.Gadgets.Charting.XYParetoChartData> _ParetoChartList = (List<EpiDashboard.Gadgets.Charting.XYParetoChartData>)this.Chart.DataSource;
                            var ParetoChartList = _ParetoChartList.GroupBy(x => x.X);
                            htmlBuilder.AppendLine("['Percentage', ");
                            foreach (var DataItem in ParetoChartList)
                            {
                                
                              
                                foreach (var item in DataItem)
                                {
                                    htmlBuilder.AppendLine(item.Z+ ", ");

                                }


                                
                            }
                            htmlBuilder.AppendLine("],");
                            htmlBuilder.AppendLine("['Count', ");
                            foreach (var DataItem in ParetoChartList)
                            {
                                
                             
                                foreach (var item in DataItem)
                                {
                                    htmlBuilder.AppendLine(item.Y + ", ");

                                }


                                
                            }
                            htmlBuilder.AppendLine("]");
                            htmlBuilder.AppendLine("]");

                        }
                        else
                        {
                            List<EpiDashboard.Gadgets.Charting.XYColumnChartData> _dataList = (List<EpiDashboard.Gadgets.Charting.XYColumnChartData>)this.Chart.DataSource;
                            var Groups = _dataList.GroupBy(x => x.X);

                            foreach (var DataItem in Groups)
                            {
                                htmlBuilder.AppendLine("['" + DataItem.ToList()[0].X + "', ");
                                _Groups.AppendLine("'" + DataItem.ToList()[0].X + "', ");
                                foreach (var item in DataItem)
                                {
                                    htmlBuilder.AppendLine(item.Y + ", ");

                                }


                                htmlBuilder.AppendLine("],");
                            }

                            htmlBuilder.AppendLine("]");
                        }
                        if (ChartType == "LineChart")
                        {

                            htmlBuilder.AppendLine(" ,  type : 'line'  }, legend: { show: true }, size: { width: " + this.ActualWidth + ", height: " + this.ActualHeight + " },");
                        }
                        else if (ChartType == "ColumnChart")
                        {
                            bool UseDiffColors = ((EpiDashboard.Controls.Charting.ColumnChartBase)this).ColumnChartParameters.UseDiffColors;

                            if (UseDiffColors)
                            {



                                htmlBuilder.AppendLine(" ,  type : 'bar' ,color: function (color, d) { return colors[d.index];} }, legend: { show: true }, size: { width: " + this.ActualWidth + ", height: " + this.ActualHeight + " },bar: {width: {ratio: .8}},");
                            }
                            else {

                                htmlBuilder.AppendLine(" ,  type : 'bar'   }, legend: { show: true }, size: { width: " + this.ActualWidth + ", height: " + this.ActualHeight + " },bar: {width: {ratio: .8}},");
                            }
                        }
                        else if (ChartType == "ParetoChart")
                        {
                            htmlBuilder.AppendLine(" ,  type : 'bar' ,axes: { Percentage: 'y2'},types: {Percentage: 'line'}  }, legend: { show: true }, size: { width: " + this.ActualWidth + ", height: " + this.ActualHeight + " },");





                        }
                        else if (ChartType == "AreaChart")
                        {
                            htmlBuilder.AppendLine(" ,  type : 'area'  }, legend: { show: true }, size: { width: " + this.ActualWidth + ", height: " + this.ActualHeight + " },");
                        }
                        else if (ChartType == "HistogramChart")
                        {


                            htmlBuilder.AppendLine(" ,  type : 'bar'   }, legend: { show: true }, size: { width: " + this.ActualWidth + ", height: " + this.ActualHeight + " }, bar: {width: {ratio: .8}},");

                        }

                        if (ChartType == "HistogramChart")
                        {
                            List<EpiDashboard.Controls.Charting.HistogramChart.XYHistogramChartData> _HChartdataList = (List<EpiDashboard.Controls.Charting.HistogramChart.XYHistogramChartData>)this.Chart.DataSource;
                            HistogramChartParameters HistogramChartParameters = new HistogramChartParameters();
                            //  (List<EpiDashboard.Controls.Charting.HistogramChart.XYHistogramChartData>)this.Parameters.Parameters = new HistogramChartParameters();
                            //this.Parameters = new HistogramChartParameters();
                            EpiDashboard.Controls.Charting.HistogramChart _HCharList = (EpiDashboard.Controls.Charting.HistogramChart)this;
                            if (color.Count() > 0)
                            {
                                htmlBuilder.AppendLine(" color: { pattern: [");
                                for (int i = 0; i < color.Count(); i++)
                                {
                                    if (color[i].Length == 9)
                                    {
                                        htmlBuilder.AppendLine(" '" + color[i].Remove(1, 2) + "' ,");
                                    }
                                }


                                htmlBuilder.AppendLine(" ]},");
                            }
                            // htmlBuilder.AppendLine("axis: { x : {type: 'timeseries',  tick:  {  format: '%m/%d/%Y', rotate: 90, multiline: false}}}");
                            htmlBuilder.AppendLine("axis: { x : {type: 'category' ,tick:  {   rotate: 90, multiline: false} , categories:  [");
                            foreach (var item in _HChartdataList)
                            {
                                htmlBuilder.AppendLine("'" + item.S + "', ");

                            }
                        }

                        else if (ChartType == "ParetoChart")
                        {
                            List<EpiDashboard.Gadgets.Charting.XYParetoChartData> _dataList = (List<EpiDashboard.Gadgets.Charting.XYParetoChartData>)this.Chart.DataSource;
                            htmlBuilder.AppendLine(" color: { pattern: [");
                            for (int i = 0; i < color.Count(); i++)
                            {
                                if (color[i].Length == 9)
                                {
                                    htmlBuilder.AppendLine(" '" + color[i].Remove(1, 2) + "' ,");
                                }
                            }


                            htmlBuilder.AppendLine(" ]},");

                            htmlBuilder.AppendLine("axis: { x : { label:{ text:'" + ((EpiDashboard.Controls.Charting.ParetoChart)this).ParetoChartParameters.XAxisLabel + "' , position: 'outer-center'}, type: 'category'  , categories:  [");
                            foreach (var item in _dataList)
                            {
                                htmlBuilder.AppendLine("'" + item.X + "', ");

                            }
                        }
                        else
                        {
                            List<EpiDashboard.Gadgets.Charting.XYColumnChartData> _dataList = (List<EpiDashboard.Gadgets.Charting.XYColumnChartData>)this.Chart.DataSource;
                            htmlBuilder.AppendLine(" color: { pattern: [");
                            for (int i = 0; i < color.Count(); i++)
                            {
                                if (color[i].Length == 9)
                                {
                                    htmlBuilder.AppendLine(" '" + color[i].Remove(1, 2) + "' ,");
                                }
                            }


                            htmlBuilder.AppendLine(" ]},");

                            htmlBuilder.AppendLine("axis: { x : {type: 'category'  , categories:  [");
                            foreach (var item in _dataList)
                            {
                                htmlBuilder.AppendLine("'" + item.S + "', ");

                            }
                        }

                       

                        htmlBuilder.AppendLine("]}");
                        if (ChartType == "LineChart")
                        {
                            htmlBuilder.AppendLine(" , y: {  label: { text:'" + ((EpiDashboard.Controls.Charting.LineChartBase)this).LineChartParameters.YAxisLabel + "' , position: 'outer-middle'}}");

                        }
                        else if (ChartType == "ColumnChart")
                        {
                            htmlBuilder.AppendLine(", y: {  label: { text: '" + ((EpiDashboard.Controls.Charting.ColumnChartBase)this).ColumnChartParameters.YAxisLabel + "', position: 'outer-middle'}}");

                        }
                        else if (ChartType == "AreaChart")
                        {
                            htmlBuilder.AppendLine(", y: {  label: { text:'" + ((EpiDashboard.Controls.Charting.AreaChartBase)this).AreaChartParameters.YAxisLabel + "', position: 'outer-middle'}}");


                        } else if (ChartType == "ParetoChart") {

                            htmlBuilder.AppendLine(", y: {  label:{ text:'" + ((EpiDashboard.Controls.Charting.ParetoChart)this).ParetoChartParameters.YAxisLabel + "' , position: 'outer-middle'}},y2: {show: true,label: {text: 'Percentage',position: 'outer-middle'},tick:{format:d3.format('100.0%')} }");
                        }
                        // tick:  {  format: '%m/%d/%Y', rotate: 90, multiline: false}
                       // htmlBuilder.AppendLine(",tick: {  x:{ multiline:true, culling: { max: 1 }, }, label : { text: 'Days', position: 'center-bottom', }, },");
                       // htmlBuilder.AppendLine(",tick:  {  format: '%m/%d/%Y', rotate: 90, multiline: false},");
                        htmlBuilder.AppendLine("}}); ");
                        if (!string.IsNullOrEmpty(_Groups.ToString()) && Composition== "Stacked")
                        {
                            htmlBuilder.AppendLine("setTimeout(function() {  Linechart" + count +".groups([[" + _Groups + "]])}, 000);");
                        }
                       
                        htmlBuilder.AppendLine("</script> ");


                    }
                    catch (Exception ex) {

                        //var _dataList = this.Chart.DataSource;
                    //    if (ChartType == "HistogramChart")
                    //    {
                    //        List<EpiDashboard.Controls.Charting.HistogramChart.XYHistogramChartData> _dataList = (List<EpiDashboard.Controls.Charting.HistogramChart.XYHistogramChartData>)this.Chart.DataSource;

                    //        htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + this.ChartTitle + "</h2>");
                    //        htmlBuilder.AppendLine("<div id=\"piechart" + count + "\"></div>");

                    //        htmlBuilder.AppendLine("<script>");


                    //        htmlBuilder.AppendLine(" var piechart" + count + " = c3.generate({bindto: '#piechart" + count + "',data: { columns: [");
                    //        var temp = _dataList;

                    //        foreach (var item in _dataList)
                    //        {
                    //            htmlBuilder.AppendLine("['" + item.S + "', " + item.Y + "], ");

                    //        }
                    //        //string colorString = "";

                    //        htmlBuilder.AppendLine(" ],  type : 'bar'  },");

                    //        htmlBuilder.AppendLine(" color: { pattern: [");
                    //        if (color.Count() > 0)
                    //        {
                    //            for (int i = 0; i < _dataList.Count(); i++)
                    //        {
                                 
                                
                    //            htmlBuilder.AppendLine(" '" + color[i].Remove(1,2) + "' ,");
                    //        }
                    //        }


                    //        htmlBuilder.AppendLine(" ]},");

                    //        htmlBuilder.AppendLine(" legend: { show: true , position: top }, size: { width: " + this.ActualWidth + ", height: " + this.ActualHeight + " },label: { format: function(value, ratio, id){return d3.format('$')(value);}}});");
                          
                    //        htmlBuilder.AppendLine(" </script> ");



                    //    }
                    }

                }
                catch (Exception ex) { throw ex; }
            }
            return htmlBuilder.ToString();
        }

        public virtual void ToImageFile(string fileName, bool includeGrid = true)
        {
            BitmapSource img = ToBitmapSource(false);
             
            System.IO.FileStream stream = new System.IO.FileStream(fileName, System.IO.FileMode.Create, FileAccess.ReadWrite );
            BitmapEncoder encoder = null; // new BitmapEncoder();

            if (fileName.EndsWith(".png"))
            {
                encoder = new PngBitmapEncoder();
            }
            else
            {
                encoder = new JpegBitmapEncoder();
            }

            encoder.Frames.Add(BitmapFrame.Create(img));
            encoder.Save(stream);
            stream.Close();
         }

        public virtual BitmapSource ToBitmapSource(bool includeGrid = true)
        {
            ComponentArt.Win.DataVisualization.Charting.ChartBase xyChart = null;
            object el = FindName("xyChart");
            if (el is XYChart)
            {
                xyChart = el as XYChart;
            }
            else if (el is ComponentArt.Win.DataVisualization.Charting.PieChart)
            {
                xyChart = el as ComponentArt.Win.DataVisualization.Charting.PieChart;
            }

            if (xyChart == null) throw new ApplicationException();

           

            Transform transform = xyChart.LayoutTransform;
            Thickness margin = new Thickness(xyChart.Margin.Left, xyChart.Margin.Top, xyChart.Margin.Right, xyChart.Margin.Bottom);
            // reset current transform (in case it is scaled or rotated)
            xyChart.LayoutTransform = null;
            xyChart.Margin = new Thickness(0);

            Size size = new Size(xyChart.Width, xyChart.Height);
            // Measure and arrange the surface
            // VERY IMPORTANT
            xyChart.Measure(size);
            xyChart.Arrange(new Rect(size));
           
            RenderTargetBitmap renderBitmap =
              new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);

            renderBitmap.Render(xyChart);

            xyChart.LayoutTransform = transform;
            xyChart.Margin = margin;
            xyChart.Measure(size);
            xyChart.Arrange(new Rect(size));

            return renderBitmap;
        }

        protected virtual void SetLegendItems()
        {
            XYChart xyChart = null;
            object el = FindName("xyChart");
            if (el is XYChart)
            {
                xyChart = el as XYChart;
            }

            if (xyChart == null) throw new ApplicationException();

            string sName = "";

            if (Parameters.StrataVariableNames.Count > 0)
            {
                foreach (Series s0 in xyChart.DataSeries)
                {
                    if (s0.Label != null)
                    {
                        sName = s0.Label.Split('.')[1];
                        if (Settings.ShowLegendVarNames == false)
                        {
                            int index = sName.IndexOf(" = ");
                            s0.Label = sName.Substring(index + 3);
                        }
                        else
                        {
                            s0.Label = sName;
                        }
                    }
                }
            }
            else
            {
                foreach (Series series in xyChart.DataSeries)
                {
                    if(series.Name == "series0")
                    {
                        series.Label = Settings.YAxisLabel;
                    }
                    else if(series.Name == "series1")
                    {
                        if (!string.IsNullOrEmpty(Y2AxisLegendTitle))
                        {
                            series.Label = Settings.Y2AxisLegendTitle;
                        }
                        else if (!string.IsNullOrEmpty(Y2AxisLabel))
                        {
                            series.Label = Settings.Y2AxisLabel;
                        }
                    }
                }
            }
        }

        protected void xyChart_AnimationCompleted(object sender, EventArgs e)
        {
            MeasureAndArrange();
        }

        protected void mnuSave_Click(object sender, RoutedEventArgs e)
        {
            SaveImageToFile();
        }

        protected void mnuCopyImage_Click(object sender, RoutedEventArgs e)
        {
            CopyImageToClipboard();
        }

        protected void mnuCopyFullData_Click(object sender, RoutedEventArgs e)
        {
            CopyAllDataToClipboard();
        }

        protected void mnuSendDataToExcel_Click(object sender, RoutedEventArgs e)
        {
            SendDataToExcel();
        }

        
        private void WriteTexttoImage(string Imgfilename = "")
        {
            string strtempbackupfilename = Imgfilename.Substring(0, Imgfilename.Length - 4) + "Tmp" + Imgfilename.Substring(Imgfilename.Length - 4, 4);
            File.Copy(Imgfilename, strtempbackupfilename, true);

            BitmapImage bitmap = new BitmapImage(new Uri(strtempbackupfilename));
            DrawingVisual visual = new DrawingVisual();
            string sString = "tmp1";
            Point curposition = new Point(250, 0);
              
            string outputfilename = Imgfilename.Substring(0, Imgfilename.Length - 4) + sString;
            string Titles = ChartTitle + Environment.NewLine + Environment.NewLine + SubTitle;
            Typeface typeFace = new Typeface(new FontFamily("Global User Interface"), FontStyles.Normal, FontWeights.DemiBold, FontStretches.Normal);
            FormattedText ftext = new FormattedText(Titles, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeFace, 20, Brushes.Black);
               
            using (DrawingContext dc = visual.RenderOpen())
            {
                dc.DrawImage(bitmap, new Rect(0, 75, bitmap.PixelWidth, bitmap.PixelHeight  ));
                dc.DrawText(ftext, curposition);

            }

            RenderTargetBitmap target = new RenderTargetBitmap(bitmap.PixelWidth, bitmap.PixelHeight + 75 ,
                                                               bitmap.DpiX, bitmap.DpiY, PixelFormats.Default);
            target.Render(visual);
                        
            BitmapEncoder encoder = null;

            if (Imgfilename.EndsWith(".png"))
            {
                encoder = new PngBitmapEncoder();
                outputfilename = outputfilename + ".png";
            }
            else
            {
                encoder = new JpegBitmapEncoder();
                outputfilename = outputfilename + ".jpg";
           }

            if (encoder != null)
            {
                encoder.Frames.Add(BitmapFrame.Create(target));
                using (FileStream outputStream = new FileStream(outputfilename, FileMode.Create))
                {
                    encoder.Save(outputStream);
                }
                         
            }

            bitmap = null;
            File.Delete(Imgfilename);
            File.Copy(outputfilename, Imgfilename, true);
            File.Delete(outputfilename);                  
            
       }

     }

}
