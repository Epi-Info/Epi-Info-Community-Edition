using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Epi.WPF.Dashboard;

namespace Epi.WPF.Dashboard.Gadgets
{
    public interface IGadget
    {
        event GadgetCheckForCancellationHandler GadgetCheckForCancellation;
        event GadgetRefreshedHandler GadgetRefreshed;
        event GadgetClosingHandler GadgetClosing;
        event GadgetProcessingFinishedHandler GadgetProcessingFinished;
        event GadgetRepositionEventHandler GadgetReposition;

        void RefreshResults();
        XmlNode Serialize(XmlDocument doc);
        void CreateFromXml(XmlElement element);
        string ToHTML(string htmlFileName = "", int count = 0);
        bool IsProcessing { get; set; }
        void SetGadgetToProcessingState();
        void SetGadgetToFinishedState();
        void UpdateVariableNames();
        //void UpdateStatusMessage(string statusMessage);
        string CustomOutputHeading { get; set; }
        string CustomOutputDescription { get; set; }
        string CustomOutputCaption { get; set; }        
        //bool ShowBorders { get; set; }
        bool DrawBorders { get; set; }        
    }
}
