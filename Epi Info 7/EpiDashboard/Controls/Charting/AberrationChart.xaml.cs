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
using System.Windows.Threading;
using EpiDashboard;
using EpiDashboard.Gadgets.Charting;
using ComponentArt.Win.DataVisualization.Charting;

namespace EpiDashboard.Controls.Charting
{
    /// <summary>
    /// Interaction logic for AberrationChart.xaml
    /// </summary>
    public partial class AberrationChart : UserControl
    {
        private AberrationDetectionChartParameters _abChartParameters;
        public AberrationChart()
        {
            InitializeComponent();
        }

        public void SetChartSize(double width, double height)
        {
            xyChart.Width = width;
            xyChart.Height = height;
        }

        public void SetChartLabels(AberrationDetectionChartParameters Parameters)
        {
            //EI-98
            xAxisCoordinates.FontSize = Parameters.XAxisFontSize;
            yAxisCoordinates.FontSize = Parameters.YAxisFontSize;

            tblockXAxisLabel.FontSize = Parameters.XAxisLabelFontSize;
            tblockYAxisLabel.FontSize = Parameters.YAxisLabelFontSize;
        }

        public void SetChartData(List<AberrationChartData> dataList, DataTable aberrationDetails)
        {
            xyChart.DataSource = dataList;

            xyChart.Visibility = System.Windows.Visibility.Visible;
            panelData.Visibility = System.Windows.Visibility.Visible;
            //xAxisScrollBar.Visibility = System.Windows.Visibility.Visible;

            if (aberrationDetails.Rows.Count > 0)
            {
                dataGridMain.Visibility = System.Windows.Visibility.Visible;
                tblockAberrationsFound.Visibility = System.Windows.Visibility.Visible;
                tblockNoAberrationsFound.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                dataGridMain.Visibility = System.Windows.Visibility.Collapsed;
                tblockAberrationsFound.Visibility = System.Windows.Visibility.Collapsed;
                tblockNoAberrationsFound.Visibility = System.Windows.Visibility.Visible;
            }

            Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    SetDataGridContext(aberrationDetails.DefaultView);
                }));

            dataGridMain.DataContext = aberrationDetails;
        }

        private void SetDataGridContext(DataView dv)
        {
            dataGridMain.DataContext = dv;
        }

        public XYChart Chart
        {
            get { return xyChart; }
        }

        public string YAxisLabel
        {
            get
            {
                return tblockYAxisLabel.Text;
            }
            set
            {
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

        public EpiDashboard.DashboardHelper DashboardHelper { get; set; }

        public Visibility DataPanelVisibility
        {
            get
            {
                return panelData.Visibility;
            }
            set
            {
                panelData.Visibility = value;
            }
        }

        public string XAxisLabel
        {
            get
            {
                return tblockXAxisLabel.Text;
            }
            set
            {
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

        public string ChartTitle
        {
            get
            {
                return tblockChartTitle.Text;
            }
            set
            {
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

        public string IndicatorTitle
        {
            get
            {
                return tblockIndicatorTitle.Text;
            }
            set
            {
                tblockIndicatorTitle.Text = value;
                if (string.IsNullOrEmpty(value.Trim()))
                {
                    tblockIndicatorTitle.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    tblockIndicatorTitle.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        private void xyChart_DataStructureCreated(object sender, EventArgs e)
        {

        }

        private void dataGridMain_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            e.Cancel = true;
        }

        internal BitmapSource ToBitmapSource(bool includeGrid = true)
        {
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

        internal void ToImageFile(string fileName, bool includeGrid = true)
        {
            BitmapSource img = ToBitmapSource(false);

            System.IO.FileStream stream = new System.IO.FileStream(fileName, System.IO.FileMode.Create);
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

        public string ToHTML(string htmlFileName = "", int count = 0, bool includeImage = true, bool includeFullData = false)
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

            htmlBuilder.AppendLine("<h3>" + this.IndicatorTitle + "</h3>");
            if (includeImage) htmlBuilder.AppendLine("<img src=\"" + fi.Name + "\" />");


            DataView dv = dataGridMain.DataContext as DataView;
            if (dv.Count > 0)
            {
                htmlBuilder.AppendLine("<p style=\"font-weight: bold;\">" + tblockAberrationsFound.Text + "</p>");

                htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
                DataTable dt = dv.Table;

                htmlBuilder.AppendLine(" <tr>");

                htmlBuilder.AppendLine("  <th>");
                htmlBuilder.Append(dt.Columns[0].ColumnName);
                htmlBuilder.AppendLine("  </th>");

                htmlBuilder.AppendLine("  <th>");
                htmlBuilder.Append(dt.Columns[1].ColumnName);
                htmlBuilder.AppendLine("  </th>");

                htmlBuilder.AppendLine("  <th>");
                htmlBuilder.Append(dt.Columns[2].ColumnName);
                htmlBuilder.AppendLine("  </th>");

                htmlBuilder.AppendLine("  <th>");
                htmlBuilder.Append(dt.Columns[3].ColumnName);
                htmlBuilder.AppendLine("  </th>");

                htmlBuilder.AppendLine(" </tr>");

                foreach (DataRowView rowView in dv)
                {
                    DataRow row = rowView.Row;
                    htmlBuilder.AppendLine(" <tr>");
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        htmlBuilder.AppendLine("  <td>");
                        htmlBuilder.Append(row[i].ToString());
                        htmlBuilder.AppendLine("  </td>");
                    }
                    htmlBuilder.AppendLine(" </tr>");
                }
                htmlBuilder.AppendLine("</table>");
            }
            else
            {
                htmlBuilder.AppendLine("<p style=\"font-weight: bold;\">" + tblockNoAberrationsFound.Text + "</p>");
            }

            htmlBuilder.AppendLine("<p>&nbsp;</p>");

            if (includeFullData)
            {
                htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
                List<AberrationChartData> dataList = xyChart.DataSource as List<AberrationChartData>;

                htmlBuilder.AppendLine(" <tr>");

                htmlBuilder.AppendLine("  <th>");
                htmlBuilder.Append("Date");
                htmlBuilder.AppendLine("  </th>");

                htmlBuilder.AppendLine("  <th>");
                htmlBuilder.Append("Actual");
                htmlBuilder.AppendLine("  </th>");

                htmlBuilder.AppendLine("  <th>");
                htmlBuilder.Append("Expected");
                htmlBuilder.AppendLine("  </th>");

                htmlBuilder.AppendLine("  <th>");
                htmlBuilder.Append("Aberration");
                htmlBuilder.AppendLine("  </th>");

                htmlBuilder.AppendLine(" </tr>");

                foreach (AberrationChartData chartData in dataList)
                {
                    htmlBuilder.AppendLine(" <tr>");

                    htmlBuilder.AppendLine("  <td>");
                    htmlBuilder.Append(chartData.Date);
                    htmlBuilder.AppendLine("  </td>");

                    htmlBuilder.AppendLine("  <td>");
                    htmlBuilder.Append(chartData.Actual);
                    htmlBuilder.AppendLine("  </td>");

                    htmlBuilder.AppendLine("  <td>");
                    htmlBuilder.Append(chartData.Expected);
                    htmlBuilder.AppendLine("  </td>");

                    htmlBuilder.AppendLine("  <td>");
                    htmlBuilder.Append(chartData.Aberration);
                    htmlBuilder.AppendLine("  </td>");

                    htmlBuilder.AppendLine(" </tr>");
                }

                htmlBuilder.AppendLine("</table>");
            }

            htmlBuilder.AppendLine("<p>&nbsp;</p>");
            htmlBuilder.AppendLine("<p>&nbsp;</p>");

            return htmlBuilder.ToString();
        }

        private void mnuSave_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG Image (.png)|*.png|JPEG Image (.jpg)|*.jpg";

            if (dlg.ShowDialog().Value)
            {
                ToImageFile(dlg.FileName);

                MessageBox.Show("Image saved successfully.", "Save Image", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void mnuCopyImage_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource img = ToBitmapSource(false); // renderBitmap;
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(img));
            Clipboard.SetImage(encoder.Frames[0]);
        }

        private void mnuCopyFullData_Click(object sender, RoutedEventArgs e)
        {
            CopyAllDataToClipboard();
        }

        private void mnuSendDataToExcel_Click(object sender, RoutedEventArgs e)
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

        internal void CopyAberrationDataToClipboard()
        {
            DataView dv = dataGridMain.DataContext as DataView;
            DataTable dt = dv.Table;
            StringBuilder sb = new StringBuilder();

            sb.Append(dt.Columns[0].ColumnName + "\t");
            sb.Append(dt.Columns[1].ColumnName + "\t");
            sb.Append(dt.Columns[2].ColumnName + "\t");
            sb.Append(dt.Columns[3].ColumnName + "\t");
            sb.AppendLine();

            foreach (DataRowView rowView in dv)
            {
                DataRow row = rowView.Row;

                sb.Append(row[0].ToString() + "\t");
                sb.Append(row[1].ToString() + "\t");
                sb.Append(row[2].ToString() + "\t");
                sb.Append(row[3].ToString() + "\t");

                sb.AppendLine();
            }
            Clipboard.Clear();
            Clipboard.SetText(sb.ToString());
        }

        internal void CopyAllDataToClipboard()
        {
            List<AberrationChartData> dataList = xyChart.DataSource as List<AberrationChartData>;

            StringBuilder sb = new StringBuilder();

            sb.Append("Date" + "\t");
            sb.Append("Actual" + "\t");
            sb.Append("Expected" + "\t");
            sb.Append("Aberration" + "\t");
            sb.AppendLine();

            foreach (AberrationChartData chartData in dataList)
            {
                sb.Append(chartData.Date + "\t");
                sb.Append(chartData.Actual + "\t");
                sb.Append(chartData.Expected + "\t");
                sb.Append(chartData.Aberration + "\t");
                sb.AppendLine();
            }
            Clipboard.Clear();
            Clipboard.SetText(sb.ToString());
        }

        private void mnuCopyAberrationData_Click(object sender, RoutedEventArgs e)
        {
            CopyAberrationDataToClipboard();
        }

        private void xyChart_AnimationCompleted(object sender, EventArgs e)
        {
            Size size = new Size(xyChart.Width, xyChart.Height);
            // Measure and arrange the surface
            // VERY IMPORTANT
            xyChart.Measure(size);
            xyChart.Arrange(new Rect(size));
        }

        private void xyChart_ChartVisualUpdated(object sender, EventArgs e)
        {

        }
    }
}
