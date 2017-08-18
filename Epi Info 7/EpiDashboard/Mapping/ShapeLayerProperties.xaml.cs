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
using ESRI.ArcGIS.Client.Symbols;
using Epi;
using Epi.Fields;

namespace EpiDashboard.Mapping
{
    /// <summary>
    /// Interaction logic for MarkerProperties.xaml
    /// </summary>
    public partial class ShapeLayerProperties : UserControl, ILayerProperties, IReferenceLayerProperties
    {

        public ShapeLayerProvider provider;
        private ESRI.ArcGIS.Client.Map myMap;

        public event EventHandler MapGenerated;
        public event EventHandler FilterRequested;
        public event EventHandler EditRequested;
        private bool flagrunedit;

        private bool isReadOnlyMode;

        public ShapeLayerProperties(ESRI.ArcGIS.Client.Map myMap)
        {
            InitializeComponent();

            this.myMap = myMap;

            // provider = new ShapeLayerProvider(myMap);

            #region Translation
            lblTitle.Content = DashboardSharedStrings.GADGET_SHAPEFILE;

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

        public void AddShapeFile()
        {
            string retval = provider.RenderShape();
            if (retval != null)
            {
                lblFileName.Content = retval;
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
            string xmlString = "<referenceLayer><fileName>" + lblFileName.Content + "</fileName></referenceLayer>";
            System.Xml.XmlElement element = doc.CreateElement("referenceLayer");
            element.InnerXml = xmlString;

            System.Xml.XmlAttribute type = doc.CreateAttribute("layerType");
            type.Value = "EpiDashboard.Mapping.ShapeLayerProperties";
            element.Attributes.Append(type);

            return element;
        }

        public void CreateFromXml(System.Xml.XmlElement element)
        {
            string fileName = string.Empty;
            foreach (System.Xml.XmlElement child in element.ChildNodes[0].ChildNodes)
            {
                if (child.Name.Equals("fileName") && BoundryFilePathExists(child.InnerText))
                {
                    lblFileName.Content = child.InnerText;
                }
            }
            if (provider == null)
            {
                provider = new ShapeLayerProvider(myMap);
            }
            provider.RenderShape(lblFileName.Content.ToString());
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

        public bool BoundryFilePathExists(string path)
        {
            if (System.IO.File.Exists(path))
            {
                return true;
            }
            else
            {
                Epi.Windows.MsgBox.ShowInformation("The boundry file does not exist with the given path.");
                return false;
            }
        }

        #endregion        

    }
}
