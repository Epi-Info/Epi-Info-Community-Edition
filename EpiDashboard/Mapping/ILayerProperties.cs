using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls;

namespace EpiDashboard.Mapping
{
    public interface ILayerProperties
    {
        void CloseLayer();
        void MakeReadOnly();
        void MoveUp();
        void MoveDown();
        event EventHandler MapGenerated;
        event EventHandler FilterRequested;
        Color FontColor { set; }
        DashboardHelper GetDashboardHelper();
        StackPanel LegendStackPanel { get; }
        System.Xml.XmlNode Serialize(System.Xml.XmlDocument doc);
        void CreateFromXml(System.Xml.XmlElement element);
    }
}
