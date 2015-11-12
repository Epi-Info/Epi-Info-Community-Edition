using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EpiDashboard.Mapping
{
    /// <summary>
    /// Interaction logic for ChoroplethShapeLayerProperties.xaml
    /// </summary>
    public partial class ChoroplethKmlLayerProperties : UserControl, ILayerProperties
    {
        private ESRI.ArcGIS.Client.Map myMap;
        private DashboardHelper dashboardHelper;
        public ChoroplethKmlLayerProvider provider;
        private System.Xml.XmlElement currentElement;

        public event EventHandler MapGenerated;
        public event EventHandler FilterRequested;
        public event EventHandler EditRequested;

        private IMapControl mapControl;
        public string shapeFilePath;

        public IDictionary<string, object> shapeAttributes;
        private bool flagrunedit;
        public IDictionary<string, object> curfeatureAttributes;
        private int Numclasses;
        public bool flagQuantiles;
        public DataFilters datafilters { get; set; }
        public Dictionary<int, object> classAttribList;

        public ChoroplethKmlLayerProperties(ESRI.ArcGIS.Client.Map myMap, DashboardHelper dashboardHelper, IMapControl mapControl)
        {
            InitializeComponent();
            this.myMap = myMap;
            this.dashboardHelper = dashboardHelper;
            this.mapControl = mapControl;

            provider = new ChoroplethKmlLayerProvider(myMap);
            provider.FeatureLoaded += new FeatureLoadedHandler(provider_FeatureLoaded);

            FillComboBoxes();
            mapControl.MapDataChanged += new EventHandler(mapControl_MapDataChanged);
            cbxDataKey.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
            cbxShapeKey.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
            cbxValue.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
            cbxClasses.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
            rctMissingColor.MouseUp += new MouseButtonEventHandler(rctMissingColor_MouseUp);
            rctHighColor.MouseUp += new MouseButtonEventHandler(rctHighColor_MouseUp);
            rctLowColor.MouseUp += new MouseButtonEventHandler(rctLowColor_MouseUp);
            rctEdit.MouseUp += new MouseButtonEventHandler(rctEdit_MouseUp);

            #region translation;
            lblTitle.Content = DashboardSharedStrings.GADGET_CONFIG_TITLE_CHOROPLETH;
            lblClasses.Content = DashboardSharedStrings.GADGET_CLASSES;
            lblShapeKey.Content = DashboardSharedStrings.GADGET_MAP_FEATURE;
            lblDataKey.Content = DashboardSharedStrings.GADGET_MAP_DATA;
            lblValueField.Content = DashboardSharedStrings.GADGET_MAP_VALUE;
            lblMissingValueColor.Content = DashboardSharedStrings.GADGET_MAP_COLOR_MISSING;
            lblLoValueColor.Content = DashboardSharedStrings.GADGET_LOW_VALUE_COLOR;
            lblHiValueColor.Content = DashboardSharedStrings.GADGET_HIGH_VALUE_COLOR;
            rctEditToolTip.Content = DashboardSharedStrings.MAP_LAYER_EDIT;
            #endregion; //translation

        }

        private void rctMissingColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctMissingColor.Fill = new SolidColorBrush(Color.FromArgb(0xF0, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                RenderMap();
            }
        }

        public void provider_FeatureLoaded(string serverName, IDictionary<string, object> featureAttributes)
        {
            if (!string.IsNullOrEmpty(serverName))
            {
                shapeFilePath = serverName;
                if (featureAttributes != null)
                {
                    curfeatureAttributes = featureAttributes;
                    cbxShapeKey.Items.Clear();
                    foreach (string key in featureAttributes.Keys)
                    {
                        cbxShapeKey.Items.Add(key);
                    }
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
                    if (child.Name.Equals("classes"))
                    {
                        cbxClasses.SelectedIndex = int.Parse(child.InnerText);
                    }
                    if (child.Name.Equals("highColor"))
                    {
                        rctHighColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                    }
                    if (child.Name.Equals("lowColor"))
                    {
                        rctLowColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                    }
                }
                // //  RenderMap();
                // GenerateMap();
            }
            GenerateMap();
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
            flagrunedit = false;
            if (EditRequested != null)
            {
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

        private void RenderMap()
        {
            if (cbxDataKey.SelectedIndex != -1 && cbxShapeKey.SelectedIndex != -1 && cbxValue.SelectedIndex != -1)
            {
                // provider.SetShapeRangeValues(dashboardHelper, cbxShapeKey.SelectedItem.ToString(), cbxDataKey.SelectedItem.ToString(), cbxValue.SelectedItem.ToString(), ((SolidColorBrush)rctLowColor.Fill).Color, ((SolidColorBrush)rctHighColor.Fill).Color, int.Parse(((ComboBoxItem)cbxClasses.SelectedItem).Content.ToString()),"" );
                provider.SetShapeRangeValues(dashboardHelper, cbxShapeKey.SelectedItem.ToString(), cbxDataKey.SelectedItem.ToString(), cbxValue.SelectedItem.ToString(), new List<SolidColorBrush>() { (SolidColorBrush)rctHighColor.Fill }, int.Parse(((ComboBoxItem)cbxClasses.SelectedItem).Content.ToString()), "");

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

        public void SetdashboardHelper(DashboardHelper dash)
        {
            this.dashboardHelper = dash;
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

        public void SetValues(string shapekey, string datakey, string val, string classes, Brush Highcolor, Brush Lowcolor, Brush Missingcolor, IDictionary<string, object> shapeAttributes, Dictionary<int, object> classAttrib, bool flagquintiles, int numclasses)
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
            rctHighColor.Fill = (SolidColorBrush)Highcolor;
            rctLowColor.Fill = (SolidColorBrush)Lowcolor;
            rctMissingColor.Fill = (SolidColorBrush)Missingcolor;
            cbxClasses.Text = classes;
            cbxShapeKey.Text = shapekey;
            cbxDataKey.Text = datakey;
            cbxValue.Text = val;
            grdMain.Width = 700;
        }


        public void MakeReadOnly()
        {
            this.FontColor = Colors.Black;
            cbxDataKey.IsEnabled = false;
            cbxShapeKey.IsEnabled = false;
            cbxValue.IsEnabled = false;
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
            SolidColorBrush highColor = (SolidColorBrush)rctHighColor.Fill;
            SolidColorBrush lowColor = (SolidColorBrush)rctLowColor.Fill;

            string customColors = "";

            //    Class titles 
            string classTitles = "<classTitles> ";
            long classTitleCount = 0;
            string classTitleTagName = "";

            foreach (KeyValuePair<string, string> entry in provider.ListLegendText.Dict)
            {
                // classTitleTagName = "legendText" + classTitleCount.ToString();
                classTitleTagName = entry.Key;
                classTitles += string.Format("<{1}>{0}</{1}>", entry.Value, classTitleTagName);
                classTitleCount++;
            }

            classTitles += "</classTitles> ";


            //  Custom colors    
            if (provider.CustomColorsDictionary != null)
            {
                customColors = "<customColors>";



                foreach (KeyValuePair<string, Color> keyValuePair in provider.CustomColorsDictionary.Dict)
                {
                    string customColor = "<" + keyValuePair.Key + ">";
                    string color = keyValuePair.Value.R + "," + keyValuePair.Value.G + "," + keyValuePair.Value.B;
                    customColor += color + "</" + keyValuePair.Key + ">";
                    customColors += customColor;
                }

                customColors += "</customColors>";
            }

            string xmlString = "<shapeFile>" + shapeFilePath + "</shapeFile><highColor>" + highColor.Color.ToString() + "</highColor><lowColor>" + lowColor.Color.ToString() + "</lowColor>" + customColors + "<classes>" + cbxClasses.SelectedIndex.ToString() + "</classes><dataKey>" + dataKey + "</dataKey><shapeKey>" + shapeKey + "</shapeKey><value>" + value + "</value>";
            System.Xml.XmlElement element = doc.CreateElement("dataLayer");
            element.InnerXml = xmlString;
            element.AppendChild(dashboardHelper.Serialize(doc));

            System.Xml.XmlAttribute type = doc.CreateAttribute("layerType");
            type.Value = "EpiDashboard.Mapping.ChoroplethKmlLayerProperties";
            element.Attributes.Append(type);

            return element;
        }

        public void CreateFromXml(System.Xml.XmlElement element)
        {
            currentElement = element;

            if (provider == null)
            {
                provider = new ChoroplethKmlLayerProvider(myMap);
                provider.FeatureLoaded += new FeatureLoadedHandler(provider_FeatureLoaded);
            }

            foreach (System.Xml.XmlElement child in element.ChildNodes)
            {
                if (child.Name.Equals("shapeFile"))
                {
                    provider.LoadKml(child.InnerText);
                }

                if (child.Name.Equals("classTitles"))
                {
                    foreach (System.Xml.XmlElement classTitle in child)
                    {
                        provider.ListLegendText.Add(classTitle.Name, classTitle.InnerText);
                    }
                }

                if (child.Name.Equals("customColors"))
                {
                    provider.UseCustomColors = true;

                    foreach (System.Xml.XmlElement color in child)
                    {
                        string rgb = color.InnerText;
                        string[] co = rgb.Split(',');
                        Color newColor = Color.FromRgb(byte.Parse(co[0]), byte.Parse(co[1]), byte.Parse(co[2]));
                        provider.CustomColorsDictionary.Add(color.Name, newColor);
                    }
                }

                if (child.Name.Equals("classRanges"))
                {

                    provider.UseCustomRanges = false;

                    foreach (System.Xml.XmlElement classRangElement in child)
                    {
                        provider.ClassRangesDictionary.Add(classRangElement.Name, classRangElement.InnerText);
                    }

                    List<double> rangeStartsFromMapFile = new List<double>();

                    string doubleString = "";

                    try
                    {
                        doubleString = provider.ClassRangesDictionary.Dict["rampStart01"];
                        if (string.IsNullOrEmpty(doubleString) == false)
                        {
                            rangeStartsFromMapFile.Add(Convert.ToDouble(doubleString));
                        }
                        doubleString = provider.ClassRangesDictionary.Dict["rampStart02"];
                        if (string.IsNullOrEmpty(doubleString) == false)
                        {
                            rangeStartsFromMapFile.Add(Convert.ToDouble(doubleString));
                        }
                        doubleString = provider.ClassRangesDictionary.Dict["rampStart03"];
                        if (string.IsNullOrEmpty(doubleString) == false)
                        {
                            rangeStartsFromMapFile.Add(Convert.ToDouble(doubleString));
                        }
                        doubleString = provider.ClassRangesDictionary.Dict["rampStart04"];
                        if (string.IsNullOrEmpty(doubleString) == false)
                        {
                            rangeStartsFromMapFile.Add(Convert.ToDouble(doubleString));
                        }
                        doubleString = provider.ClassRangesDictionary.Dict["rampStart05"];
                        if (string.IsNullOrEmpty(doubleString) == false)
                        {
                            rangeStartsFromMapFile.Add(Convert.ToDouble(doubleString));
                        }
                        doubleString = provider.ClassRangesDictionary.Dict["rampStart06"];
                        if (string.IsNullOrEmpty(doubleString) == false)
                        {
                            rangeStartsFromMapFile.Add(Convert.ToDouble(doubleString));
                        }
                        doubleString = provider.ClassRangesDictionary.Dict["rampStart07"];
                        if (string.IsNullOrEmpty(doubleString) == false)
                        {
                            rangeStartsFromMapFile.Add(Convert.ToDouble(doubleString));
                        }
                        doubleString = provider.ClassRangesDictionary.Dict["rampStart08"];
                        if (string.IsNullOrEmpty(doubleString) == false)
                        {
                            rangeStartsFromMapFile.Add(Convert.ToDouble(doubleString));
                        }
                        doubleString = provider.ClassRangesDictionary.Dict["rampStart09"];
                        if (string.IsNullOrEmpty(doubleString) == false)
                        {
                            rangeStartsFromMapFile.Add(Convert.ToDouble(doubleString));
                        }
                        doubleString = provider.ClassRangesDictionary.Dict["rampStart10"];
                        if (string.IsNullOrEmpty(doubleString) == false)
                        {
                            rangeStartsFromMapFile.Add(Convert.ToDouble(doubleString));
                        }
                    }
                    catch { }

                    provider.RangeStartsFromMapFile = rangeStartsFromMapFile;
                    provider.RangesLoadedFromMapFile = true;
                }

            }
        }


        private void GenerateMap()
        {
            EpiDashboard.Controls.ChoroplethProperties choroplethprop = new Controls.ChoroplethProperties(this.mapControl as StandaloneMapControl, this.myMap);
            choroplethprop.txtProjectPath.Text = dashboardHelper.Database.DataSource;
            choroplethprop.SetDashboardHelper(dashboardHelper);
            choroplethprop.radKML.IsChecked = true;
            choroplethprop.txtShapePath.Text = shapeFilePath;
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

            choroplethprop.choroplethKmlLayerProvider = provider;
            choroplethprop.thisProvider = provider;

            choroplethprop.SetDefaultRanges();
            choroplethprop.GetRangeValues(provider.RangeCount);
            //   provider.ListLegendText = choroplethprop.ListLegendText;
            choroplethprop.RenderMap();

        }
        #endregion
    }
}
