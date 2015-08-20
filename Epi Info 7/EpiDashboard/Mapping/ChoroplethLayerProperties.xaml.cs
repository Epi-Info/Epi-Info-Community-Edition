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
    /// Interaction logic for ChoroplethLayerProperties.xaml
    /// </summary>
    public partial class ChoroplethLayerProperties : UserControl, ILayerProperties
    {
        private ESRI.ArcGIS.Client.Map myMap;
        private DashboardHelper dashboardHelper;
        public ChoroplethLayerProvider provider;

        public event EventHandler MapGenerated;
        public event EventHandler FilterRequested;
        public event EventHandler EditRequested;
        
        private IMapControl mapControl;
        public string shapeFilePath;
        public IDictionary<string, object> shapeAttributes;
        private bool flagrunedit;
        private int Numclasses;
        public bool flagQuantiles;
        public DataFilters datafilters { get; set; }
        public Dictionary<int, object> classAttribList;

        public ChoroplethLayerProperties(ESRI.ArcGIS.Client.Map myMap, DashboardHelper dashboardHelper, IMapControl mapControl)
        {
            InitializeComponent();
            this.myMap = myMap;
            this.dashboardHelper = dashboardHelper;
            this.mapControl = mapControl;

            provider = new ChoroplethLayerProvider(myMap);

            FillComboBoxes();
            mapControl.MapDataChanged += new EventHandler(mapControl_MapDataChanged);
            btnShapeFile.Click += new RoutedEventHandler(btnShapeFile_Click);
            cbxDataKey.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
            cbxShapeKey.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
            cbxValue.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
            cbxClasses.SelectionChanged +=new SelectionChangedEventHandler(keys_SelectionChanged);
            rctHighColor.MouseUp += new MouseButtonEventHandler(rctHighColor_MouseUp);
            rctLowColor.MouseUp += new MouseButtonEventHandler(rctLowColor_MouseUp); 
            rctFilter.MouseUp += new MouseButtonEventHandler(rctFilter_MouseUp);
            rctEdit.MouseUp += new MouseButtonEventHandler(rctEdit_MouseUp);
        }

        void rctFilter_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (FilterRequested != null)
            {
                FilterRequested(this, new EventArgs());
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

        void rctLowColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctLowColor.Fill = new SolidColorBrush(Color.FromArgb(0xF0, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                RenderMap();
            }
        }

        void rctHighColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctHighColor.Fill = new SolidColorBrush(Color.FromArgb(0xF0, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                RenderMap();
            }
        }
        public bool FlagRunEdit
        {
            set { flagrunedit = value; }
            get { return flagrunedit; }
        }

        private void rctEdit_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (EditRequested !=null)
            {
                flagrunedit = false;
                EditRequested(this, new EventArgs());
            }
        }

        private void RenderMap()
        {
            if (cbxDataKey.SelectedIndex != -1 && cbxShapeKey.SelectedIndex != -1 && cbxValue.SelectedIndex != -1)
            {
               provider.SetShapeRangeValues(dashboardHelper, cbxShapeKey.SelectedItem.ToString(), cbxDataKey.SelectedItem.ToString(), cbxValue.SelectedItem.ToString(), new List<SolidColorBrush>() { (SolidColorBrush)rctHighColor.Fill }, int.Parse(((ComboBoxItem)cbxClasses.SelectedItem).Content.ToString()),"");

               
                if (MapGenerated != null)
                {
                    MapGenerated(this, new EventArgs());
                }
             
            }
            
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
            object[] shapeFileProperties = provider.LoadShapeFile();
            if (shapeFileProperties != null)
            {
                if (shapeFileProperties.Length == 2)
                {
                    shapeFilePath = shapeFileProperties[0].ToString();
                    shapeAttributes = (IDictionary<string, object>)shapeFileProperties[1];
                    if (shapeAttributes != null)
                    {
                        cbxShapeKey.Items.Clear();
                        foreach (string key in shapeAttributes.Keys)
                        {
                            cbxShapeKey.Items.Add(key);
                        }
                    }
                }
            }
        }

        private void FillComboBoxes()
        {
            cbxDataKey.Items.Clear();
            cbxValue.Items.Clear();
            List<string> fields = dashboardHelper.GetFieldsAsList(); // dashboardHelper.GetFormFields();
            ColumnDataType columnDataType = ColumnDataType.Numeric;
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
            btnShapeFile.Visibility = Visibility.Collapsed;
            grdMain.Width = 700;
            lblTitle.Visibility = System.Windows.Visibility.Visible;
        }
                
        public void SetValues(string shapefilepath, string shapekey, string datakey, string val, string classes, Brush Highcolor, Brush Lowcolor, Brush Missingcolor, IDictionary<string, object> shapeAttributes, Dictionary<int, object> classAttrib, bool flagquintiles, int numclasses)
        {
            FillComboBoxes();
            if (shapeAttributes != null)
            {
                cbxShapeKey.Items.Clear();
                foreach (string key in shapeAttributes.Keys)
                {
                    cbxShapeKey.Items.Add(key);
                }
            }
            classAttribList = classAttrib;
            flagQuantiles = flagquintiles;
            Numclasses = numclasses;
            shapeFilePath = shapefilepath;
            rctHighColor.Fill = (SolidColorBrush)Highcolor;
            rctLowColor.Fill = (SolidColorBrush)Lowcolor;
            rctMissingColor.Fill = (SolidColorBrush)Missingcolor;
            cbxClasses.Text = classes;
            cbxShapeKey.Text = shapekey;
            cbxDataKey.Text = datakey;
            cbxValue.Text = val;
            grdMain.Width = 700;
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
            SolidColorBrush highColor = (SolidColorBrush)rctHighColor.Fill;
            SolidColorBrush lowColor = (SolidColorBrush)rctLowColor.Fill;
            SolidColorBrush missingColor = (SolidColorBrush)rctMissingColor.Fill;
            string xmlString = "<shapeFile>" + shapeFilePath + "</shapeFile><highColor>" + highColor.Color.ToString() + "</highColor><lowColor>" + lowColor.Color.ToString() + "</lowColor><missingColor>" + missingColor.Color.ToString() + "</missingColor><classes>" + cbxClasses.SelectedIndex.ToString() + "</classes><dataKey>" + dataKey + "</dataKey><shapeKey>" + shapeKey + "</shapeKey><value>" + value + "</value>";
            System.Xml.XmlElement element = doc.CreateElement("dataLayer");
            element.InnerXml = xmlString;
            element.AppendChild(dashboardHelper.Serialize(doc));

            System.Xml.XmlAttribute type = doc.CreateAttribute("layerType");
            type.Value = "EpiDashboard.Mapping.ChoroplethLayerProperties";
            element.Attributes.Append(type);

            return element;
        }

        public void SetdashboardHelper(DashboardHelper dash)
        {
            this.dashboardHelper = dash;
        }

        public void CreateFromXml(System.Xml.XmlElement element)
        {
            foreach (System.Xml.XmlElement child in element.ChildNodes)
            {
                if (child.Name.Equals("shapeFile"))
                {
                    object[] shapeFileProperties = provider.LoadShapeFile(child.InnerText);
                    if (shapeFileProperties != null)
                    {
                        if (shapeFileProperties.Length == 2)
                        {
                            shapeFilePath = shapeFileProperties[0].ToString();
                            IDictionary<string, object> shapeAttributes = (IDictionary<string, object>)shapeFileProperties[1];
                           
                            if (shapeAttributes != null)
                            {
                                cbxShapeKey.Items.Clear();
                                foreach (string key in shapeAttributes.Keys)
                                {
                                    cbxShapeKey.Items.Add(key);
                                }
                            }
                        }
                    }
                }
                if (child.Name.Equals("dataKey"))
                {
                    cbxDataKey.SelectedItem = child.InnerText;
                }
                if (child.Name.Equals("shapeKey"))
                {
                    cbxShapeKey.SelectedItem = child.InnerText;
                }
                if (child.Name.Equals("classes"))
                {
                    cbxClasses.SelectedIndex = int.Parse(child.InnerText);
                }
                if (child.Name.Equals("value"))
                {
                    cbxValue.SelectedItem = child.InnerText;
                }
                if (child.Name.Equals("highColor"))
                {
                    rctHighColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                }
                if (child.Name.Equals("lowColor"))
                {
                    rctLowColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                }
                if (child.Name.Equals("missingColor"))
                {
                    rctMissingColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                }
            }
            GenerateMap();
           //RenderMap();
        }

        private void GenerateMap()
        {
            
            EpiDashboard.Controls.ChoroplethProperties choroplethprop = new Controls.ChoroplethProperties(this.mapControl as StandaloneMapControl, this.myMap);
                                  
            choroplethprop.txtShapePath.Text = shapeFilePath;
            choroplethprop.txtProjectPath.Text = dashboardHelper.Database.DataSource;
            choroplethprop.SetDashboardHelper(dashboardHelper);
            choroplethprop.cmbClasses.Text = cbxClasses.Text;
            foreach (string str in cbxShapeKey.Items) { choroplethprop.cmbShapeKey.Items.Add(str); }
            foreach (string str in cbxDataKey.Items) { choroplethprop.cmbDataKey.Items.Add(str); }
            foreach (string str in cbxValue.Items) { choroplethprop.cmbValue.Items.Add(str); }
            choroplethprop.cmbShapeKey.SelectedItem = cbxShapeKey.Text;
            choroplethprop.cmbDataKey.SelectedItem = cbxDataKey.Text;
            choroplethprop.cmbValue.SelectedItem = cbxValue.Text;
            choroplethprop.rctHighColor.Fill = rctHighColor.Fill;
            choroplethprop.rctLowColor.Fill = rctLowColor.Fill;
            choroplethprop.rctMissingColor.Fill = rctMissingColor.Fill;
            choroplethprop.radShapeFile.IsChecked = true;
            choroplethprop._provider = provider;
            choroplethprop.SetDefaultRanges();
            choroplethprop.GetRangeValues(provider.RangeCount);
            provider.ListLegendText = choroplethprop.ListLegendText;
            choroplethprop.RenderMap();
          }
        #endregion
       
    }

  
}
