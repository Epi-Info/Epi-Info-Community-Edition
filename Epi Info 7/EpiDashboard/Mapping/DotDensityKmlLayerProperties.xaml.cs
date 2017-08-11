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
    /// Interaction logic for ChoroplethLayerProperties.xaml
    /// </summary>
    public partial class DotDensityKmlLayerProperties : UserControl, ILayerProperties
    {
        private MapView _mapView;
        private DashboardHelper dashboardHelper;
        public DotDensityKmlLayerProvider provider;
        private System.Xml.XmlElement currentElement;

        public event EventHandler MapGenerated;
        public event EventHandler FilterRequested;
        public event EventHandler EditRequested;
        
        private IMapControl mapControl;
        public string shapeFilePath;
        private bool flagrunedit;
        public DataFilters datafilters { get; set; }
        public IDictionary<string, object> curfeatureAttributes;

        public DotDensityKmlLayerProperties(MapView mapView, DashboardHelper dashboardHelper, IMapControl mapControl)
        {
            InitializeComponent();
            this._mapView = mapView;
            this.dashboardHelper = dashboardHelper;
            this.mapControl = mapControl;

           // provider = new DotDensityKmlLayerProvider(_mapView);
           // provider.FeatureLoaded += new FeatureLoadedHandler(provider_FeatureLoaded);

            FillComboBoxes();
            mapControl.MapDataChanged += new EventHandler(mapControl_MapDataChanged);
            cbxDataKey.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
            cbxShapeKey.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
            cbxValue.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
            rctDotColor.MouseUp += new MouseButtonEventHandler(rctDotColor_MouseUp);
            rctEdit.MouseUp += new MouseButtonEventHandler(rctEdit_MouseUp);

            #region translation;
            rctEditToolTip.Content = DashboardSharedStrings.MAP_LAYER_EDIT;
            #endregion; //translation

        }
        public bool FlagRunEdit
        {
            set { flagrunedit = value; }
            get { return flagrunedit; }
       }
       public void provider_FeatureLoaded(string serverName, IDictionary<string, object> featureAttributes)
        {
            if (!string.IsNullOrEmpty(serverName))
            {
                shapeFilePath = serverName;
                curfeatureAttributes = featureAttributes;
                if (featureAttributes != null)
                {
                    string txtshapekey = cbxShapeKey.Text;
                    cbxShapeKey.Items.Clear();
                    foreach (string key in featureAttributes.Keys)
                    {
                        cbxShapeKey.Items.Add(key);
                    }
                    if (cbxShapeKey.Items.Contains(txtshapekey)) { cbxShapeKey.Text = txtshapekey; }
                }
            }
            if (currentElement != null)
            {
                foreach (System.Xml.XmlElement child in currentElement.ChildNodes)
                {
                    if (child.Name.Equals("dataKey"))
                    {
                        cbxDataKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("shapeKey"))
                    {
                        cbxShapeKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("value"))
                    {
                        cbxValue.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("dotValue"))
                    {
                        txtDotValue.Text = child.InnerText;
                    }
                    if (child.Name.Equals("dotColor"))
                    {
                        rctDotColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                    }
                }
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
        void rctEdit_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (EditRequested != null)
            {
                flagrunedit = false;
                EditRequested(this, new EventArgs());
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

        void rctDotColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            Brush brush = new SolidColorBrush(((SolidColorBrush)rctDotColor.Fill).Color);
            dialog.Color = System.Drawing.Color.FromArgb(((brush as SolidColorBrush).Color).A, ((brush as SolidColorBrush).Color).R, ((brush as SolidColorBrush).Color).G, ((brush as SolidColorBrush).Color).B);         
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctDotColor.Fill = new SolidColorBrush(Color.FromArgb(0xF0, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                RenderMap();
            }
        }

        private void RenderMap()
        {
            //System.ComponentModel.BackgroundWorker worker = new System.ComponentModel.BackgroundWorker();
            //worker.DoWork += new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
            //worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            //worker.RunWorkerAsync();

            if (cbxDataKey.SelectedIndex != -1 && cbxShapeKey.SelectedIndex != -1 && cbxValue.SelectedIndex != -1)
            {               
                provider.SetShapeRangeValues(dashboardHelper, cbxShapeKey.SelectedItem.ToString(), cbxDataKey.SelectedItem.ToString(), cbxValue.SelectedItem.ToString(), ((SolidColorBrush)rctDotColor.Fill).Color, int.Parse(txtDotValue.Text));
                if (MapGenerated != null)
                {
                    MapGenerated(this, new EventArgs());
                }
            }
        }

        void worker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (cbxDataKey.SelectedIndex != -1 && cbxShapeKey.SelectedIndex != -1 && cbxValue.SelectedIndex != -1)
            {
                provider.SetShapeRangeValues(dashboardHelper, cbxShapeKey.SelectedItem.ToString(), cbxDataKey.SelectedItem.ToString(), cbxValue.SelectedItem.ToString(), ((SolidColorBrush)rctDotColor.Fill).Color, int.Parse(txtDotValue.Text));
                if (MapGenerated != null)
                {
                    MapGenerated(this, new EventArgs());
                }
            }
        }

        void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(10000);
        }

        public StackPanel LegendStackPanel
        {
            get
            {
                return provider.LegendStackPanel;
            }
        }

        public DashboardHelper GetDashboardHelper()
        {
            return this.dashboardHelper;
        }

        void mapControl_MapDataChanged(object sender, EventArgs e)
        {
            provider.Refresh();
        }

        void keys_SelectionChanged(object sender, EventArgs e)
        {
            RenderMap();
        }

        void btnShapeFile_Click(object sender, RoutedEventArgs e)
        {
            provider.LoadKml();
        }

        private void FillComboBoxes()
        {
            cbxDataKey.Items.Clear();
            cbxValue.Items.Clear();
            ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.DateTime | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.UserDefined;
            List<string> fields = dashboardHelper.GetFieldsAsList(columnDataType); // dashboardHelper.GetFormFields();
            columnDataType = ColumnDataType.Numeric;
            List<string> numericFields = dashboardHelper.GetFieldsAsList(columnDataType); //dashboardHelper.GetNumericFormFields();
            foreach (string field in fields)
            {
                cbxDataKey.Items.Add(field);
            }
            foreach (string field in numericFields)
            {
                cbxValue.Items.Add(field);
            }
            cbxValue.Items.Insert(0, "{Record Count}");
        }



        public void SetValues(string shapekey, string datakey, string val, string dottext, Brush selectedcolor)
        {
            FillComboBoxes();
            rctDotColor.Fill = (SolidColorBrush)selectedcolor;
            txtDotValue.Text = dottext;
            cbxShapeKey.Text = shapekey;           
            cbxDataKey.Text = datakey;
            cbxValue.Text = val;            
            grdMain.Width = 700;            
        }

        public void SetdashboardHelper(DashboardHelper dash)
        {
            this.dashboardHelper = dash;
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
                lblDataKey.Foreground = brush;
                lblShapeKey.Foreground = brush;
                lblValueField.Foreground = brush;
            }
        }

        public void MakeReadOnly()
        {
            this.FontColor = Colors.Black;
            cbxDataKey.IsEnabled = false;
            cbxShapeKey.IsEnabled = false;
            cbxValue.IsEnabled = false;
            txtDotValue.IsReadOnly = true;
            grdMain.Width = 700;
            lblTitle.Visibility = System.Windows.Visibility.Visible;
        }

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
            string dataKey = cbxDataKey.SelectedItem.ToString();
            string shapeKey = cbxShapeKey.SelectedItem.ToString();
            string value = cbxValue.SelectedItem.ToString();
            SolidColorBrush dotColor = (SolidColorBrush)rctDotColor.Fill;
            string xmlString = "<shapeFile>" + shapeFilePath + "</shapeFile><dotColor>" + dotColor.Color.ToString() + "</dotColor><dotValue>" + txtDotValue.Text + "</dotValue><dataKey>" + dataKey + "</dataKey><shapeKey>" + shapeKey + "</shapeKey><value>" + value + "</value>";
            System.Xml.XmlElement element = doc.CreateElement("dataLayer");
            element.InnerXml = xmlString;
            element.AppendChild(dashboardHelper.Serialize(doc));

            System.Xml.XmlAttribute type = doc.CreateAttribute("layerType");
            type.Value = "EpiDashboard.Mapping.DotDensityKmlLayerProperties";
            element.Attributes.Append(type);

            return element;
        }

        public void CreateFromXml(System.Xml.XmlElement element)
        {
            currentElement = element;
            foreach (System.Xml.XmlElement child in element.ChildNodes)
            {
                if (child.Name.Equals("shapeFile") && BoundryFilePathExists(child.InnerText))
                {
                    if (provider == null)
                    {
                        provider = new DotDensityKmlLayerProvider(_mapView);
                        provider.FeatureLoaded += new FeatureLoadedHandler(provider_FeatureLoaded);
                    }
                    provider.LoadKml(child.InnerText);
                }
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
