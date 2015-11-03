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
using System.Xml;
using Epi;
using Epi.Fields;
// ReSharper disable All

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

        public enum ColorRampType
        {
            Auto,
            Custom
        }

        private ColorRampType _colorRampType = new ColorRampType();

        private bool _useCustomColors;


        public ColorRampType CurrentColorRampType
        {
            get { return _colorRampType; }
        }

        public bool UseCustomColors
        {
            get { return _useCustomColors; }
        }


        public ChoroplethLayerProperties(ESRI.ArcGIS.Client.Map myMap, DashboardHelper dashboardHelper, IMapControl mapControl)
        {
            InitializeComponent();
            this.myMap = myMap;
            this.dashboardHelper = dashboardHelper;
            this.mapControl = mapControl;

            provider = new ChoroplethLayerProvider(myMap);

            FillComboBoxes();
            mapControl.MapDataChanged += new EventHandler(mapControl_MapDataChanged);
            //btnShapeFile.Click += new RoutedEventHandler(btnShapeFile_Click);
            cbxDataKey.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
            cbxShapeKey.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
            cbxValue.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
            cbxClasses.SelectionChanged += new SelectionChangedEventHandler(keys_SelectionChanged);
            rctHighColor.MouseUp += new MouseButtonEventHandler(rctHighColor_MouseUp);
            rctLowColor.MouseUp += new MouseButtonEventHandler(rctLowColor_MouseUp);
            //rctFilter.MouseUp += new MouseButtonEventHandler(rctFilter_MouseUp);
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
            //System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            //if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    rctLowColor.Fill = new SolidColorBrush(Color.FromArgb(0xF0, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            //    RenderMap();
            //}
        }

        void rctHighColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            //if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    rctHighColor.Fill = new SolidColorBrush(Color.FromArgb(0xF0, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            //    RenderMap();
            //}
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

        private void RenderMap()
        {
            // why is this called  ======================================
            //      
            if (cbxDataKey.SelectedIndex != -1 && cbxShapeKey.SelectedIndex != -1 && cbxValue.SelectedIndex != -1)
            {
                provider.SetShapeRangeValues(dashboardHelper, cbxShapeKey.SelectedItem.ToString(),
                    cbxDataKey.SelectedItem.ToString(),
                    cbxValue.SelectedItem.ToString(), new List<SolidColorBrush>() { (SolidColorBrush)rctHighColor.Fill },
                    int.Parse(((ComboBoxItem)cbxClasses.SelectedItem).Content.ToString()), "");


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
            //btnShapeFile.Visibility = Visibility.Collapsed;
            grdMain.Width = 700;
            lblTitle.Visibility = System.Windows.Visibility.Visible;
        }

        public void SetValues(string shapefilepath, string shapekey, string datakey, string val,
            string classes, Brush Highcolor, Brush Lowcolor, Brush Missingcolor, IDictionary<string, object> shapeAttributes,
            Dictionary<int, object> classAttrib, bool flagquintiles, int numclasses, string LegendText)
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
            provider.LegendText = LegendText;

        }


        //public XmlNode Serialize(System.Xml.XmlDocument doc /* ,
        //    List<Color> customColorsList = null, bool useCustomColors = false*/    )    

        public XmlNode Serialize(System.Xml.XmlDocument doc)
        {

            try
            {
                string dataKey = cbxDataKey.SelectedItem.ToString();
                string shapeKey = cbxShapeKey.SelectedItem.ToString();
                string value = cbxValue.SelectedItem.ToString();
                SolidColorBrush highColor = (SolidColorBrush)rctHighColor.Fill;
                SolidColorBrush lowColor = (SolidColorBrush)rctLowColor.Fill;
                SolidColorBrush missingColor = (SolidColorBrush)rctMissingColor.Fill;

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

                string customColors = "";

                //  Custom colors    
                if (provider.CustomColorsDictionary != null)
                {
                    customColors = "<customColors>";

                    //  foreach (Color color in provider.ClassesColorsDictionary)


                  

                    foreach (KeyValuePair<string, Color> keyValuePair in provider.CustomColorsDictionary.Dict)
                    {
                        string customColor = "<" + keyValuePair.Key + ">";
                        string color = keyValuePair.Value.R + "," + keyValuePair.Value.G + "," + keyValuePair.Value.B;
                        customColor += color + "</" + keyValuePair.Key + ">";
                        customColors += customColor;
                    }

                    customColors += "</customColors>";
                }


                //  Use custom colors   
                string useCustomColorsTag = "<useCustomColors>" + provider.UseCustomColors + "</useCustomColors>";

                string classRanges = "";


                // Class ranges    
                if (provider.ClassRangesDictionary != null)
                {
                    classRanges = "<classRanges>";

                    foreach (KeyValuePair<string, string> keyValuePair in provider.ClassRangesDictionary.Dict)
                    {
                        string rangeName = "<" + keyValuePair.Key + ">";
                        string rangeValue = keyValuePair.Value;
                        rangeName += rangeValue + "</" + keyValuePair.Key + ">";
                        classRanges += rangeName;
                    }

                    classRanges += "</classRanges>";
                }



                string xmlString = "<shapeFile>" + shapeFilePath + "</shapeFile><highColor>" + highColor.Color.ToString() +
                                   "</highColor><legTitle>" + provider.LegendText + "</legTitle>" + classTitles + classRanges + useCustomColorsTag + customColors + "<lowColor>" + lowColor.Color.ToString() +
                                   "</lowColor><missingColor>" + missingColor.Color.ToString() + "</missingColor><classes>" + cbxClasses.SelectedIndex.ToString() +
                                   "</classes><dataKey>" + dataKey + "</dataKey><shapeKey>" + shapeKey + "</shapeKey><value>" + value + "</value>";


                doc.PreserveWhitespace = true;

                XmlElement element = doc.CreateElement("dataLayer");
                element.InnerXml = xmlString;
                element.AppendChild(dashboardHelper.Serialize(doc));

                XmlAttribute type = doc.CreateAttribute("layerType");
                type.Value = "EpiDashboard.Mapping.ChoroplethLayerProperties";
                element.Attributes.Append(type);

                return element;
            }
            catch (Exception e)
            {


                throw new Exception(e.Message);

            }
            // return new XmlElement();          


        }

        public void SetdashboardHelper(DashboardHelper dash)
        {
            this.dashboardHelper = dash;
        }


        private string[] classTitles;


        public void CreateFromXml(System.Xml.XmlElement element)
        {
            foreach (System.Xml.XmlElement child in element.ChildNodes)
            {
                if (child.Name.Equals("classTitles"))
                {
                    foreach (System.Xml.XmlElement classTitle in child)
                    {
                        // get control named like classTitles.Name    

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
                    //  entries from the UI  
                    foreach (System.Xml.XmlElement classRangElement in child)
                    {
                        provider.ClassRangesDictionary.Add(classRangElement.Name, classRangElement.InnerText);

                    }
                    // list of douhles for "rangeStarts" list    
                    List<double> rangeStartsFromMapFile = new List<double>();

                    try
                    {
                        rangeStartsFromMapFile.Add(Convert.ToDouble(provider.ClassRangesDictionary.Dict["rampStart01"]));
                        rangeStartsFromMapFile.Add(Convert.ToDouble(provider.ClassRangesDictionary.Dict["rampStart02"]));
                        rangeStartsFromMapFile.Add(Convert.ToDouble(provider.ClassRangesDictionary.Dict["rampStart03"]));
                        rangeStartsFromMapFile.Add(Convert.ToDouble(provider.ClassRangesDictionary.Dict["rampStart04"]));
                        rangeStartsFromMapFile.Add(Convert.ToDouble(provider.ClassRangesDictionary.Dict["rampStart05"]));
                        rangeStartsFromMapFile.Add(Convert.ToDouble(provider.ClassRangesDictionary.Dict["rampStart06"]));
                        rangeStartsFromMapFile.Add(Convert.ToDouble(provider.ClassRangesDictionary.Dict["rampStart07"]));
                        rangeStartsFromMapFile.Add(Convert.ToDouble(provider.ClassRangesDictionary.Dict["rampStart08"]));
                        rangeStartsFromMapFile.Add(Convert.ToDouble(provider.ClassRangesDictionary.Dict["rampStart09"]));
                        rangeStartsFromMapFile.Add(Convert.ToDouble(provider.ClassRangesDictionary.Dict["rampStart10"]));
                    }
                    catch (Exception e) { }


                    provider.RangeStartsFromMapFile = rangeStartsFromMapFile;
                    provider.RangesLoadedFromMapFile = true;


                }






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

                if (child.Name.Equals("legTitle"))
                {
                    provider.LegendText = child.InnerText;
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
            choroplethprop.legTitle.Text = provider.LegendText;
            choroplethprop.SetDefaultRanges();
            choroplethprop.GetRangeValues(provider.RangeCount);
            choroplethprop.ListLegendText = provider.ListLegendText;
            //  choroplethprop.CustomColors = provider.CustomColors;     
            choroplethprop.RenderMap();
        }
        #endregion

    }


}
