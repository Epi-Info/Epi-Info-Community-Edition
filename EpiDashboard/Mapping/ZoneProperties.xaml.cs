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
    /// Interaction logic for ZoneProperties.xaml
    /// </summary>
    public partial class ZoneProperties : UserControl, ILayerProperties
    {

        private ZoneProvider provider;
        private ESRI.ArcGIS.Client.Map myMap;
        private ESRI.ArcGIS.Client.Geometry.MapPoint point;

        public event EventHandler MapGenerated;
        public event EventHandler FilterRequested;
        public event EventHandler EditRequested;
        
        private bool isReadOnlyMode;

        public ZoneProperties(ESRI.ArcGIS.Client.Map myMap, ESRI.ArcGIS.Client.Geometry.MapPoint point)
        {
            InitializeComponent();

            this.myMap = myMap;
            this.point = point;
            
            provider = new ZoneProvider(myMap, point);
            cbxUnits.SelectionChanged += new SelectionChangedEventHandler(config_SelectionChanged);
            txtRadius.TextChanged+=new TextChangedEventHandler(txtRadius_TextChanged);
            rctColor.MouseUp += new MouseButtonEventHandler(rctColor_MouseUp);
            btnOK.Click += new RoutedEventHandler(btnOK_Click);

            FillComboBoxes();

            #region translation
            lblTitle.Content = DashboardSharedStrings.GADGET_MAP_ZONE;
            lblRadius.Content = DashboardSharedStrings.GADGET_MAP_ZONE_RADIUS;
            lblUnits.Content = DashboardSharedStrings.GADGET_MAP_ZONE_UNITS;
            rctColorToolTip.Content = DashboardSharedStrings.GADGET_MAP_ZONE_COLOR;
            btnOK.Content = DashboardSharedStrings.BUTTON_OK;
            #endregion //translation

        }

        void btnOK_Click(object sender, RoutedEventArgs e)
        {
            RenderMap();
        }

        void txtRadius_TextChanged(object sender, TextChangedEventArgs e)
        {
            double radius = 0;
            if (cbxUnits.SelectedIndex != -1 && !String.IsNullOrEmpty(txtRadius.Text) && double.TryParse(txtRadius.Text, out radius))
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
            double radius = 0;
            if (cbxUnits.SelectedIndex != -1 && !String.IsNullOrEmpty(txtRadius.Text) && double.TryParse(txtRadius.Text,out radius))
            {
                provider.RenderZone(radius, rctColor.Fill, cbxUnits.SelectedItem.ToString());
                if (MapGenerated != null)
                {
                    MapGenerated(this, new EventArgs());
                }
            }
        }

        void config_SelectionChanged(object sender, EventArgs e)
        {
            double radius = 0;
            if (cbxUnits.SelectedIndex != -1 && !String.IsNullOrEmpty(txtRadius.Text) && double.TryParse(txtRadius.Text, out radius))
            {
                btnOK.IsEnabled = true;
            }
            else
            {
                btnOK.IsEnabled = false;
            }
        }


        private void FillComboBoxes()
        {
            cbxUnits.Items.Clear();
            cbxUnits.ItemsSource = new string[] { "Kilometer", "Meter", "Mile", "Yard", "Foot" };// Enum.GetNames(typeof(ESRI.ArcGIS.Client.Tasks.LinearUnit));
            cbxUnits.SelectedIndex = -1;
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
                lblRadius.Foreground = brush;
                lblUnits.Foreground = brush;
            }
        }

        public void MakeReadOnly()
        {
            this.FontColor = Colors.Black;
            //cbxUnits.IsEnabled = false;
            //txtRadius.IsEnabled = false;
            grdMain.Width = 700;
            btnOK.Visibility = System.Windows.Visibility.Collapsed;
            rctColor.Margin = new Thickness(0, 0, 30, 0);
            lblTitle.Visibility = System.Windows.Visibility.Visible;
            isReadOnlyMode = true;
        }

        #endregion

        #region ILayerProperties Members


        public StackPanel LegendStackPanel
        {
            get { return null; }
        }

        #endregion


        public System.Xml.XmlNode Serialize(System.Xml.XmlDocument doc)
        {
            string unit = cbxUnits.SelectedItem.ToString();
            string radius = txtRadius.Text;
            SolidColorBrush color = (SolidColorBrush)rctColor.Fill;
            string xmlString = "<graphicsLayer><unit>" + unit + "</unit><color>" + color.Color.ToString() + "</color><radius>" + radius + "</radius></graphicsLayer>";
            System.Xml.XmlElement element = doc.CreateElement("graphicsLayer");
            element.InnerXml = xmlString;
            
            System.Xml.XmlAttribute type = doc.CreateAttribute("layerType");
            type.Value = "EpiDashboard.Mapping.ZoneProperties";
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
                if (child.Name.Equals("unit"))
                {
                    cbxUnits.SelectedItem = child.InnerText;
                }
                if (child.Name.Equals("color"))
                {
                    rctColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                }
                if (child.Name.Equals("radius"))
                {
                    txtRadius.Text = child.InnerText;
                }
            }
            RenderMap();
        }

        public DashboardHelper GetDashboardHelper()
        {
            return null;
        }
    }
}
