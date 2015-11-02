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
    /// Interaction logic for TextProperties.xaml
    /// </summary>
    public partial class TextProperties : UserControl, ILayerProperties
    {

        private TextProvider provider;
        private ESRI.ArcGIS.Client.Map myMap;
        private System.Drawing.Font font;
        private ESRI.ArcGIS.Client.Geometry.MapPoint point;
        private bool isReadOnlyMode;

        public event EventHandler MapGenerated;
        public event EventHandler FilterRequested;
        public event EventHandler EditRequested;


        public TextProperties(ESRI.ArcGIS.Client.Map myMap, ESRI.ArcGIS.Client.Geometry.MapPoint point)
        {
            InitializeComponent();
            isReadOnlyMode = false;

            this.myMap = myMap;
            this.point = point;

            provider = new TextProvider(myMap, point);
            txtFont.TextChanged += new TextChangedEventHandler(config_TextChanged);
            txtText.TextChanged += new TextChangedEventHandler(config_TextChanged);
            rctColor.MouseUp += new MouseButtonEventHandler(rctColor_MouseUp);
            btnFont.Click += new RoutedEventHandler(btnFont_Click);
            btnOK.Click += new RoutedEventHandler(btnOK_Click);

            #region Translation
            lblTitle.Content = DashboardSharedStrings.GADGET_MAP_LABEL;
            lblText.Content = DashboardSharedStrings.GADGET_MAP_LABEL;
            lblFont.Content = DashboardSharedStrings.GADGET_MAP_LABEL_COLOR;
            btnFont.Content = DashboardSharedStrings.GADGET_MAP_LABEL_FONT;
            btnOK.Content = DashboardSharedStrings.BUTTON_OK;
            #endregion //Translation
        }

        void btnOK_Click(object sender, RoutedEventArgs e)
        {
            RenderMap();
        }

        void btnFont_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FontDialog dialog = new System.Windows.Forms.FontDialog();
            if (font != null)
            {
                dialog.Font = font;
            }
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                font = dialog.Font;
                txtFont.Text = dialog.Font.Name;
                if (isReadOnlyMode)
                {
                    RenderMap();
                }
            }
        }

        void config_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtText.Text) && !string.IsNullOrEmpty(txtFont.Text))
            {
                btnOK.IsEnabled = true;
            }
            else
            {
                btnOK.IsEnabled = false;
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
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor.Fill = new SolidColorBrush(Color.FromArgb(0xFF, dialog.Color.R, dialog.Color.G, dialog.Color.B));
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
            if (!string.IsNullOrEmpty(txtText.Text) && !string.IsNullOrEmpty(txtFont.Text))
            {
                provider.RenderText(font, rctColor.Fill, txtText.Text);
                if (MapGenerated != null)
                {
                    MapGenerated(this, new EventArgs());
                }
            }
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
                lblFont.Foreground = brush;
                lblText.Foreground = brush;
            }
        }

        public void MakeReadOnly()
        {
            isReadOnlyMode = true;
            this.FontColor = Colors.Black;
            txtFont.IsEnabled = false;
            txtText.TextChanged += new TextChangedEventHandler(txtText_TextChanged);
            //btnFont.Visibility = System.Windows.Visibility.Collapsed;
            btnOK.Visibility = System.Windows.Visibility.Collapsed;
            rctColor.Margin = new Thickness(0, 0, 30, 0);
            lblTitle.Visibility = System.Windows.Visibility.Visible;
            grdMain.Width = 700;
        }

        void txtText_TextChanged(object sender, TextChangedEventArgs e)
        {
            RenderMap();
        }

        #endregion

        #region ILayerProperties Members


        public StackPanel LegendStackPanel
        {
            get { return null; }
        }

        #endregion

        public DashboardHelper GetDashboardHelper()
        {
            return null;
        }

        public System.Xml.XmlNode Serialize(System.Xml.XmlDocument doc)
        {
            string text = txtText.Text;
            string fontName = font.Name;
            string fontSize = font.Size.ToString();
            SolidColorBrush color = (SolidColorBrush)rctColor.Fill;
            string xmlString = "<graphicsLayer><text>" + text + "</text><color>" + color.Color.ToString() + "</color><font>" + ConvertToString(this.font) + "</font></graphicsLayer>";
            System.Xml.XmlElement element = doc.CreateElement("graphicsLayer");
            element.InnerXml = xmlString;
            System.Xml.XmlAttribute type = doc.CreateAttribute("layerType");
            type.Value = "EpiDashboard.Mapping.TextProperties";
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
            foreach (System.Xml.XmlElement child in element.ChildNodes[0].ChildNodes)
            {
                if (child.Name.Equals("text"))
                {
                    txtText.Text = child.InnerText;
                }
                if (child.Name.Equals("color"))
                {
                    rctColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                }
                if (child.Name.Equals("font"))
                {
                    this.font = ConvertToFont(child.InnerText);
                    txtFont.Text = this.font.Name;
                }
            }
            RenderMap();
        }

        private string ConvertToString(System.Drawing.Font font)
        {
            try
            {
                if (font != null)
                {
                    System.ComponentModel.TypeConverter converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(System.Drawing.Font));
                    return converter.ConvertToString(font);
                }
                else
                    return null;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Unable to convert");
            }
            return null;
        }

        private System.Drawing.Font ConvertToFont(string fontString)
        {
            try
            {
                System.ComponentModel.TypeConverter converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(System.Drawing.Font));
                return (System.Drawing.Font)converter.ConvertFromString(fontString);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Unable to convert");
            }
            return null;
        }
    }
}
