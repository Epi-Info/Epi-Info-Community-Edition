using System;
using System.Collections.Generic;
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
using Epi;
using Epi.Fields;

namespace EpiDashboard.Mapping
{
    /// <summary>
    /// Interaction logic for MarkerProperties.xaml
    /// </summary>
    public partial class KmlLayerProperties : UserControl, ILayerProperties, IReferenceLayerProperties
    {

        public KmlLayerProvider provider;
        private Esri.ArcGISRuntime.Mapping.Map myMap;
        private string serverName;
        private int[] visibleLayers;

        public event EventHandler MapGenerated;
        public event EventHandler FilterRequested;
        public event EventHandler EditRequested;

        private bool isReadOnlyMode;

        public KmlLayerProperties(Esri.ArcGISRuntime.Mapping.Map myMap)
        {
            InitializeComponent();

            this.myMap = myMap;

            //  provider = new KmlLayerProvider(myMap);
            #region Translation
            lblTitle.Content = DashboardSharedStrings.GADGET_KMLFILE;
            #endregion //translation

        }

        public void MoveUp()
        {
            provider.MoveUp();
        }

        public void MoveDown()
        {
            provider.MoveDown();
        }

        public void AddServerImage()
        {
            KmlDialog retval = provider.RenderServerImage();
            if (retval != null)
            {
                lblServerName.Content = retval.ServerName;
                serverName = retval.ServerName;
                visibleLayers = retval.VisibleLayers;
                if (MapGenerated != null)
                {
                    MapGenerated(this, new EventArgs());
                }
            }
        }

        public void CheckMapGenerated()
        {
            if (MapGenerated != null)
            {
                MapGenerated(this, new EventArgs());
            }

        }

        #region ILayerProperties Members

        public void CloseLayer()
        {
            provider.CloseLayer();
        }

        public void MakeReadOnly()
        {
            grdMain.Width = 700;
            isReadOnlyMode = true;
        }

        public StackPanel LegendStackPanel
        {
            get { return null; }
        }

        public DashboardHelper GetDashboardHelper()
        {
            return null;
        }

        public System.Xml.XmlNode Serialize(System.Xml.XmlDocument doc)
        {
            string visibleLayersCsv = string.Empty;
            string xmlString = string.Empty;
            if (visibleLayers != null)
            {
                foreach (int x in visibleLayers)
                {
                    visibleLayersCsv += x.ToString() + ",";
                }
                visibleLayersCsv = visibleLayersCsv.Substring(0, visibleLayersCsv.Length - 1);
                xmlString = "<referenceLayer><url>" + lblServerName.Content + "</url><visibleLayers>" + visibleLayersCsv + "</visibleLayers></referenceLayer>";
            }
            else
            {
                xmlString = "<referenceLayer><url>" + lblServerName.Content + "</url><visibleLayers/></referenceLayer>";
            }
            System.Xml.XmlElement element = doc.CreateElement("referenceLayer");
            element.InnerXml = xmlString;

            System.Xml.XmlAttribute type = doc.CreateAttribute("layerType");
            type.Value = "EpiDashboard.Mapping.KmlLayerProperties";
            element.Attributes.Append(type);

            return element;
        }

        public void CreateFromXml(System.Xml.XmlElement element)
        {
            string fileName = string.Empty;
            foreach (System.Xml.XmlElement child in element.ChildNodes[0].ChildNodes)
            {
                if (child.Name.Equals("url"))
                {
                    lblServerName.Content = child.InnerText;
                    serverName = child.InnerText;
                }
                if (child.Name.Equals("visibleLayers"))
                {
                    if (child.InnerText.Equals(string.Empty))
                    {
                        visibleLayers = null;
                    }
                    else
                    {
                        string[] parts = child.InnerText.Split(',');
                        visibleLayers = new int[parts.Length];
                        for (int x = 0; x < parts.Length; x++)
                        {
                            visibleLayers[x] = int.Parse(parts[x]);
                        }
                    }
                }
            }
            if (provider == null)
            {
                provider = new KmlLayerProvider(myMap);
            }
            provider.RenderServerImage(serverName, visibleLayers);
            if (MapGenerated != null)
            {
                MapGenerated(this, new EventArgs());
            }
        }

        public Color FontColor
        {
            set
            {
                //do nothing
            }
        }

        #endregion

    }
}
