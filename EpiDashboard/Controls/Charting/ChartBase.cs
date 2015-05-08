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
                
        public virtual string ToHTML(string htmlFileName = "", int count = 0, bool includeImage = true, bool includeFullData = false)
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
