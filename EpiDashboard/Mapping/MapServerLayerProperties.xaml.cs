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
    public partial class MapServerLayerProperties : UserControl, ILayerProperties, IReferenceLayerProperties
    {

        public MapServerLayerProvider provider;
        private Esri.ArcGISRuntime.Mapping.Map myMap;
        private string serverName;
        private int[] visibleLayers;
        private bool flagrunedit;
        public event EventHandler MapGenerated;
        public event EventHandler FilterRequested;
        public event EventHandler EditRequested;
        
        private bool isReadOnlyMode;

        public MapServerLayerProperties(Esri.ArcGISRuntime.Mapping.Map myMap)
        {
            InitializeComponent();

            this.myMap = myMap;

           // provider = new MapServerLayerProvider(myMap);
            #region Translation
            lblTitle.Content = DashboardSharedStrings.GADGET_MAPSERVER;
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
            MapServerDialog retval = provider.RenderServerImage();
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

        public bool FlagRunEdit
        {
            set { flagrunedit = value; }
            get { return flagrunedit; }
        }

        private void rctEdit_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (EditRequested != null)
            {
                flagrunedit = false;
                EditRequested(this, new EventArgs());
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
                xmlString = "<referenceLayer><serverName>" + lblServerName.Content + "</serverName><visibleLayers>" + visibleLayersCsv + "</visibleLayers></referenceLayer>";
            }
            else
            {
                xmlString = "<referenceLayer><serverName>" + lblServerName.Content + "</serverName><visibleLayers/></referenceLayer>";
            }
            System.Xml.XmlElement element = doc.CreateElement("referenceLayer");
            element.InnerXml = xmlString;

            System.Xml.XmlAttribute type = doc.CreateAttribute("layerType");
            type.Value = "EpiDashboard.Mapping.MapServerLayerProperties";
            element.Attributes.Append(type);

            return element;
        }

        public void CreateFromXml(System.Xml.XmlElement element)
        {
            string fileName = string.Empty;
            foreach (System.Xml.XmlElement child in element.ChildNodes[0].ChildNodes)
            {
                if (child.Name.Equals("serverName"))
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
                provider = new MapServerLayerProvider(myMap);
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
