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

using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;

namespace EpiDashboard.Mapping
{
    /// <summary>
    /// Interaction logic for ClusterLayerProperties.xaml
    /// </summary>
    public partial class PointLayerProperties : UserControl, ILayerProperties
    {

        public PointLayerProvider provider;
        private MapView _mapView;
        private DashboardHelper dashboardHelper;

        public event EventHandler MapGenerated;
        public event EventHandler FilterRequested;

        public event EventHandler EditRequested;

        private IMapControl mapControl;
        private bool flagrunedit;
        private StackPanel legendStackPanel;

        public PointLayerProperties(MapView mapView, DashboardHelper dashboardHelper, IMapControl mapControl)
        {
            InitializeComponent();

            this._mapView = mapView;
            this.dashboardHelper = dashboardHelper;
            this.mapControl = mapControl;
            mapControl.TimeVariableSet += new TimeVariableSetHandler(mapControl_TimeVariableSet);
            mapControl.MapDataChanged += new EventHandler(mapControl_MapDataChanged);

            provider = new PointLayerProvider(_mapView);
            provider.DateRangeDefined += new DateRangeDefinedHandler(provider_DateRangeDefined);
            provider.RecordSelected += new RecordSelectedHandler(provider_RecordSelected);
            cbxLatitude.SelectionChanged += new SelectionChangedEventHandler(coord_SelectionChanged);
            cbxLongitude.SelectionChanged += new SelectionChangedEventHandler(coord_SelectionChanged);
            rctColor.MouseUp += new MouseButtonEventHandler(rctColor_MouseUp);
            //rctFilter.MouseUp += new MouseButtonEventHandler(rctFilter_MouseUp);
            rctEdit.MouseUp  += new MouseButtonEventHandler(rctEdit_MouseUp);
            FillComboBoxes();
            
            #region translation;
            lblTitle.Content = DashboardSharedStrings.GADGET_LABEL_SPOTS;
            lblDescription.Content = DashboardSharedStrings.GADGET_LABEL_DESCRIPTION;
            lblStyle.Content = DashboardSharedStrings.GADGET_LABEL_STYLE;
            lblLatitude.Content = DashboardSharedStrings.GADGET_LATITUDE_FIELD;
            lblLongitude.Content = DashboardSharedStrings.GADGET_LONGITUDE_FIELD;
            rctColorToolTip.Content = DashboardSharedStrings.MAP_POINT_COLOR; 
            rctEditToolTip.Content = DashboardSharedStrings.MAP_LAYER_EDIT;
            #endregion; //translation

        }

        public bool FlagRunEdit
        {
            set { flagrunedit = value; }
            get { return flagrunedit; }
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
                RenderMap();
            }
        }
        //--
        void rctEdit_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (EditRequested != null)
            {
                flagrunedit = false;
                EditRequested(this, new EventArgs());
            }
        }

       
        //--
        public void MoveUp()
        {
            provider.MoveUp();
        }

        public void MoveDown()
        {
            provider.MoveDown();
        }

        public DashboardHelper GetDashboardHelper()
        {
          return this.dashboardHelper;
        }

        //--
        public void SetdashboardHelper(DashboardHelper dash)
        {
            this.dashboardHelper = dash;
        }
               

        void provider_RecordSelected(int id)
        {
            mapControl.OnRecordSelected(id);
        }

        void mapControl_MapDataChanged(object sender, EventArgs e)
        {
            provider.Refresh();
        }

        void mapControl_TimeVariableSet(string timeVariable)
        {
            provider.TimeVar = timeVariable;
        }

        void provider_DateRangeDefined(DateTime start, DateTime end, List<KeyValuePair<DateTime, int>> intervalCounts)
        {
            mapControl.OnDateRangeDefined(start, end, intervalCounts);
        }

        private void RenderMap()
        {
            if (cbxLatitude.SelectedIndex != -1 && cbxLongitude.SelectedIndex != -1)
            {
                provider.RenderPointMap(dashboardHelper, cbxLatitude.SelectedItem.ToString(), cbxLongitude.SelectedItem.ToString(), rctColor.Fill, null, (SimpleMarkerStyle)Enum.Parse(typeof(SimpleMarkerStyle), cbxStyle.SelectedItem.ToString()), txtDescription.Text);
                if (MapGenerated != null)
                {
                    MapGenerated(this, new EventArgs());
                }
            }
        }
        
        void coord_SelectionChanged(object sender, EventArgs e)
        {
            //RenderMap();
        } 


        private void FillComboBoxes()
        {
            cbxLatitude.Items.Clear();
            cbxLongitude.Items.Clear();
            ColumnDataType columnDataType = ColumnDataType.Numeric;
            List<string> fields = dashboardHelper.GetFieldsAsList(columnDataType); //dashboardHelper.GetNumericFormFields();
            foreach (string f in fields)
            {
                cbxLatitude.Items.Add(f);
                cbxLongitude.Items.Add(f);
            }
            if (cbxLatitude.Items.Count > 0)
            {
                cbxLatitude.SelectedIndex = -1;
                cbxLongitude.SelectedIndex = -1;
            }

            cbxStyle.ItemsSource = Enum.GetNames(typeof(SimpleMarkerStyle));
            cbxStyle.SelectedIndex = 0;
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
                lblLatitude.Foreground = brush;
                lblLongitude.Foreground = brush;
            }
        }

        public void MakeReadOnly()
        {
            this.FontColor = Colors.Black;
            cbxLatitude.IsEnabled = false;
            cbxLongitude.IsEnabled = false;
            cbxStyle.IsEnabled = false;
            txtDescription.IsReadOnly = true;
            txtDescription.Width = 130;
           // btnChangeText.Visibility = System.Windows.Visibility.Visible;
            //btnChangeText.Visibility = System.Windows.Visibility.Hidden;
            grdMain.Width = 700;
            lblTitle.Visibility = System.Windows.Visibility.Visible;
        }

        
        //---
        public void SetValues(string description, string latitude, string longitude, string style, Brush selectedcolor)
        {
            FillComboBoxes();
            rctColor.Fill = (SolidColorBrush)selectedcolor;
            cbxStyle.Text = style;
            txtDescription.Text = description;
            cbxLatitude.Text = latitude;
            cbxLongitude.Text = longitude;
            cbxStyle.Text = style;
            grdMain.Width = 700;
            rctColor.Fill = selectedcolor;
          }
        //--
        
        public System.Xml.XmlNode Serialize(System.Xml.XmlDocument doc)
        {
            string connectionString = string.Empty;
            string tableName = string.Empty;
            string projectPath = string.Empty;
            string viewName = string.Empty;
            if (dashboardHelper.View == null)
            {
                connectionString = dashboardHelper.Database.ConnectionString;
                tableName = dashboardHelper.TableName;
            }
            else
            {
                projectPath = dashboardHelper.View.Project.FilePath;
                viewName = dashboardHelper.View.Name;
            }
            string latitude = cbxLatitude.SelectedItem.ToString();
            string longitude = cbxLongitude.SelectedItem.ToString();
            SolidColorBrush color = (SolidColorBrush)rctColor.Fill;
            string style = cbxStyle.SelectedItem.ToString();
            string description = txtDescription.Text;
            string xmlString = "<description>" + description + "</description><color>" + color.Color.ToString() + "</color><style>" + style + "</style><latitude>" + latitude + "</latitude><longitude>" + longitude + "</longitude>";
            System.Xml.XmlElement element = doc.CreateElement("dataLayer");
            element.InnerXml = xmlString;            
            element.AppendChild(dashboardHelper.Serialize(doc));

            System.Xml.XmlAttribute type = doc.CreateAttribute("layerType");
            type.Value = "EpiDashboard.Mapping.PointLayerProperties";
            element.Attributes.Append(type);

            return element;
        }

        public void CreateFromXml(System.Xml.XmlElement element)
        {
            foreach (System.Xml.XmlElement child in element.ChildNodes)
            {
                if (child.Name.Equals("latitude"))
                {
                    cbxLatitude.SelectedItem = child.InnerText;
                }
                if (child.Name.Equals("longitude"))
                {
                    cbxLongitude.SelectedItem = child.InnerText;
                }
                if (child.Name.Equals("color"))
                {
                    rctColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                }
                if (child.Name.Equals("style"))
                {
                    cbxStyle.SelectedItem = child.InnerText;
                }
                if (child.Name.Equals("description"))
                {
                    txtDescription.Text = child.InnerText;
                }
            }
            RenderMap();
        }

        #endregion

        #region ILayerProperties Members


        public StackPanel LegendStackPanel
        {
            get { return provider.LegendStackPanel; }
            set { legendStackPanel = value; }
        }

        #endregion

        private void btnChangeText_Click(object sender, RoutedEventArgs e)
        {
            Dialogs.LayerDescriptionDialog dialog = new Dialogs.LayerDescriptionDialog();
            dialog.Description = txtDescription.Text;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtDescription.Text = dialog.Description;
                RenderMap();
            }
        }
    }
}
