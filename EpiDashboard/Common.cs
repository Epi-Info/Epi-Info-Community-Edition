using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EpiDashboard
{
    public class Common
    {

        public static void Print(FrameworkElement visual)
        {
            PrintDialog dialog = new PrintDialog();
            if (dialog.ShowDialog() == true)
            {

                System.Printing.PrintCapabilities capabilities = dialog.PrintQueue.GetPrintCapabilities(dialog.PrintTicket);
                double scale = Math.Min(capabilities.PageImageableArea.ExtentWidth / visual.ActualWidth, capabilities.PageImageableArea.ExtentHeight / visual.ActualHeight);
                Transform originalTransform = visual.LayoutTransform;
                visual.LayoutTransform = new ScaleTransform(scale, scale);
                Size sz = new Size(capabilities.PageImageableArea.ExtentWidth, capabilities.PageImageableArea.ExtentHeight);
                visual.Measure(sz);
                visual.Arrange(new Rect(new Point(capabilities.PageImageableArea.OriginWidth, capabilities.PageImageableArea.OriginHeight), sz));
                dialog.PrintVisual(visual, "Chart");
                visual.LayoutTransform = originalTransform;
            }
        }

        public static void SaveAsImage(FrameworkElement visual)
        {            
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG Image (.png)|*.png|JPEG Image (.jpg)|*.jpg";

            if (dlg.ShowDialog().Value)
            {
                BitmapSource img = (BitmapSource)ToImageSource(visual);

                FileStream stream = new FileStream(dlg.FileName, FileMode.Create);
                BitmapEncoder encoder = null; // new BitmapEncoder();

                if (dlg.SafeFileName.ToLower().EndsWith(".png"))
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
        }

        public static ImageSource ToImageSource(FrameworkElement obj)
        {
            Transform transform = obj.LayoutTransform;
            Thickness margin = obj.Margin;
            obj.Margin = new Thickness(0, 0, margin.Right - margin.Left, margin.Bottom - margin.Top);
            Size size = new Size(obj.ActualWidth, obj.ActualHeight);
            obj.Measure(size);
            obj.Arrange(new Rect(size));
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)obj.ActualWidth, (int)obj.ActualHeight, 96, 96, PixelFormats.Pbgra32);

            bmp.Render(obj);
            obj.LayoutTransform = transform;
            obj.Margin = margin;
            return bmp;
        }

        public static void CopyDataViewToClipboard(DataView dv)
        {
            StringBuilder sb = new StringBuilder();
            DataTable dt = dv.Table;

            foreach (DataColumn dc in dt.Columns)
            {
                string columnName = dc.ColumnName;
                sb.Append(columnName + "\t");
            }
            sb.AppendLine();

            foreach(DataRow row in dt.Rows)
            {
                foreach (DataColumn dc in dt.Columns)
                {
                    object value = row[dc];
                    sb.Append(value.ToString() + "\t");
                }
                sb.AppendLine();
            }

            sb.Replace("\n", "");
            Clipboard.Clear();
            Clipboard.SetText(sb.ToString());
        }

        public static string ConvertDataViewToHtmlString(DataView dv)
        {
            StringBuilder sb = new StringBuilder();
            DataTable dt = dv.Table;

            sb.AppendLine(" <tr>");
            foreach (DataColumn dc in dt.Columns)
            {
                sb.AppendLine("  <th style=\"width: auto;\">");
                sb.AppendLine("  " + dc.ColumnName);
                sb.AppendLine("  </th>");
            }
            sb.AppendLine("</tr>");

            foreach (DataRow row in dt.Rows)
            {
                sb.AppendLine(" <tr>");
                foreach (DataColumn dc in dt.Columns)
                {
                    sb.AppendLine("  <td class=\"value\">");
                    object value = row[dc];
                    string strValue = value.ToString().Trim();

                    if (String.IsNullOrEmpty(strValue))
                    {
                        strValue = "&nbsp;";
                    }

                    sb.Append(strValue);
                    sb.AppendLine("  </td>");
                }
                sb.AppendLine(" </tr>");
            }

            sb.Replace("\n", "");
            
            return sb.ToString();
        }
    }
}
