using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using ComponentArt.Win.DataVisualization.Charting;
using EpiDashboard;
using EpiDashboard.Gadgets.Charting;

namespace EpiDashboard.Controls.Charting
{
    public interface IChart
    {
        string ToHTML(string htmlFileName = "", int count = 0, bool includeImage = true, bool includeFullData = false,bool ForWeb =false);
        void ToImageFile(string fileName, bool includeGrid = true);
        BitmapSource ToBitmapSource(bool includeGrid = true);
        void SaveImageToFile();
        string SendDataToString();
        void CopyAllDataToClipboard();
        void CopyImageToClipboard();
        DashboardHelper DashboardHelper { get; set; }

        IChartSettings Settings { get; set; }
        GadgetParameters Parameters { get; set; }
        string SubTitle { get; set; }
        string StrataTitle { get; set; }
    }
}
