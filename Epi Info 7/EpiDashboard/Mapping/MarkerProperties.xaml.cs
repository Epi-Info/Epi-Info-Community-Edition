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

using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;

using Epi;
using Epi.Fields;

namespace EpiDashboard.Mapping
{
    /// <summary>
    /// Interaction logic for MarkerProperties.xaml
    /// </summary>
    public partial class MarkerProperties : UserControl, ILayerProperties
    {

        private MarkerProvider provider;
        private ESRI.ArcGIS.Client.Map myMap;
        private ESRI.ArcGIS.Client.Geometry.MapPoint point;

        public event EventHandler MapGenerated;
        public event EventHandler FilterRequested;
        public event EventHandler EditRequested;
        
        private bool isReadOnlyMode;

        public MarkerProperties(ESRI.ArcGIS.Client.Map myMap, ESRI.ArcGIS.Client.Geometry.MapPoint point)
        {
            InitializeComponent();

            this.myMap = myMap;
            this.point = point;
            
            provider = new MarkerProvider(myMap, point);
            cbxSize.SelectionChanged += new SelectionChangedEventHandler(config_SelectionChanged);
            cbxStyle.SelectionChanged += new SelectionChangedEventHandler(config_SelectionChanged);
            rctColor.MouseUp += new MouseButtonEventHandler(rctColor_MouseUp);
            btnOK.Click += new RoutedEventHandler(btnOK_Click);

            FillComboBoxes();

            #region translation
            lblTitle.Content = DashboardSharedStrings.GADGET_MAP_MARKER;
            lblStyle.Content = DashboardSharedStrings.GADGET_LABEL_STYLE;
            lblSize.Content = DashboardSharedStrings.GADGET_MAP_SIZE;
            rctColorToolTip.Content = DashboardSharedStrings.MAP_POINT_COLOR; 
            btnOK.Content = DashboardSharedStrings.BUTTON_OK;
            #endregion //translation
        }

        void btnOK_Click(object sender, RoutedEventArgs e)
        {
            RenderMap();
        }

        void rctFilter_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (FilterRequested != null)
            {
                FilterRequested(this, new EventArgs());
            }
        }

        void rctColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            Brush brush = new SolidColorBrush(((SolidColorBrush)rctColor.Fill).Color);
            dialog.Color = System.Drawing.Color.FromArgb(((brush as SolidColorBrush).Color).A, ((brush as SolidColorBrush).Color).R, ((brush as SolidColorBrush).Color).G, ((brush as SolidColorBrush).Color).B);         
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor.Fill = new SolidColorBrush(Color.FromArgb(0x99, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                if (isReadOnlyMode)
                {
                    RenderMap();
                }
            }
        }

        public void MoveUp()
        {
            provider.MoveUp();
        }

        public void MoveDown()
        {
            provider.MoveDown();
        }

        private void RenderMap()
        {
            if (cbxStyle.SelectedIndex != -1 && cbxSize.SelectedIndex != -1)
            {
                provider.RenderMarker(int.Parse(cbxSize.SelectedItem.ToString()), rctColor.Fill, (SimpleMarkerSymbol.SimpleMarkerStyle)Enum.Parse(typeof(SimpleMarkerSymbol.SimpleMarkerStyle), cbxStyle.SelectedItem.ToString()));
                if (MapGenerated != null)
                {
                    MapGenerated(this, new EventArgs());
                }
            }
        }

        void config_SelectionChanged(object sender, EventArgs e)
        {
            if (cbxStyle.SelectedIndex != -1 && cbxSize.SelectedIndex != -1)
            {
                btnOK.IsEnabled = true;
            }
            else
            {
                btnOK.IsEnabled = false;
            }
            if (isReadOnlyMode)
            {
                RenderMap();
            }
        }


        private void FillComboBoxes()
        {
            cbxStyle.Items.Clear();
            cbxSize.Items.Clear();
            cbxStyle.ItemsSource = Enum.GetNames(typeof(SimpleMarkerSymbol.SimpleMarkerStyle));
            for (int x = 10; x < 31; x++)
            {
                cbxSize.Items.Add(x.ToString());
            }
            cbxStyle.SelectedIndex = -1;
            cbxSize.SelectedIndex = -1;
        }

        #region ILayerProperties Members

        public void CloseLayer()
        {
            provider.CloseLayer();
        }
        
        public Color FontColor
        {
            set 
            {
                SolidColorBrush brush = new SolidColorBrush(value);
                lblStyle.Foreground = brush;
                lblSize.Foreground = brush;
            }
        }

        public void MakeReadOnly()
        {
            this.FontColor = Colors.Black;
            //cbxStyle.IsEnabled = false;
            //cbxSize.IsEnabled = false;
            grdMain.Width = 700;
            btnOK.Visibility = System.Windows.Visibility.Collapsed;
            rctColor.Margin = new Thickness(0, 0, 30, 0);
            lblTitle.Visibility = System.Windows.Visibility.Visible;
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
            int size = int.Parse(cbxSize.SelectedItem.ToString());
            SolidColorBrush color = (SolidColorBrush)rctColor.Fill;
            string style = cbxStyle.SelectedItem.ToString();
            string xmlString = "<graphicsLayer><size>" + size + "</size><color>" + color.Color.ToString() + "</color><style>" + style + "</style></graphicsLayer>";
            System.Xml.XmlElement element = doc.CreateElement("graphicsLayer");
            element.InnerXml = xmlString;

            System.Xml.XmlAttribute type = doc.CreateAttribute("layerType");
            type.Value = "EpiDashboard.Mapping.MarkerProperties";
            element.Attributes.Append(type);

            System.Xml.XmlAttribute x = doc.CreateAttribute("locationX");
            x.Value = point.X.ToString();
            element.Attributes.Append(x);

            System.Xml.XmlAttribute y = doc.CreateAttribute("locationY");
            y.Value = point.Y.ToString();
            element.Attributes.Append(y);
            return element;
        }

        public void CreateFromXml(System.Xml.XmlElement element)
        {
            string fontName = string.Empty;
            foreach (System.Xml.XmlElement child in element.ChildNodes[0].ChildNodes)
            {
                if (child.Name.Equals("style"))
                {
                    cbxStyle.SelectedItem = child.InnerText;
                }
                if (child.Name.Equals("color"))
                {
                    rctColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                }
                if (child.Name.Equals("size"))
                {
                    cbxSize.SelectedItem = child.InnerText;
                }
            }
            RenderMap();
        }

        #endregion

    }
}
