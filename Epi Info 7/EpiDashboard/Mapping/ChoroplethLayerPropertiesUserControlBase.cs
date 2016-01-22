using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Media;

namespace EpiDashboard.Mapping
{
    public class ChoroplethLayerPropertiesUserControlBase : UserControl
    {
        public ESRI.ArcGIS.Client.Map myMap;
        public DashboardHelper dashboardHelper;
        public string boundryFilePath;
        public IMapControl mapControl;
        public bool partitionSetUsingQuantiles;

        private Dictionary<int, object> _classAttribList;
        public Dictionary<int, object> ClassAttributeList
        {
            get { return _classAttribList; }
            set { _classAttribList = value; }
        }

        public void CreateFromXml(System.Xml.XmlElement element, ChoroplethLayerProvider provider)
        {
            foreach (System.Xml.XmlElement child in element.ChildNodes)
            {
                if (child.Name.Equals("classTitles"))
                {
                    foreach (System.Xml.XmlElement classTitle in child)
                    {
                        provider.ListLegendText.Add(classTitle.Name, classTitle.InnerText);
                    }
                }

                if (child.Name.Equals("partitionUsingQuantiles"))
                {
                    bool asQuintiles = false;
                    bool.TryParse(child.InnerText, out asQuintiles);
                    provider.UseQuantiles = asQuintiles;
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
                    foreach (System.Xml.XmlElement classRangElement in child)
                    {
                        provider.ClassRangesDictionary.Add(classRangElement.Name, classRangElement.InnerText);
                    }

                    List<double> rangeStartsFromMapFile = new List<double>();
                    string doubleString = "";

                    try
                    {
                        doubleString = provider.ClassRangesDictionary.RangeDictionary["rampStart01"];
                        if (string.IsNullOrEmpty(doubleString) == false)
                        {
                            rangeStartsFromMapFile.Add(Convert.ToDouble(doubleString));
                        }
                        doubleString = provider.ClassRangesDictionary.RangeDictionary["rampStart02"];
                        if (string.IsNullOrEmpty(doubleString) == false)
                        {
                            rangeStartsFromMapFile.Add(Convert.ToDouble(doubleString));
                        }
                        doubleString = provider.ClassRangesDictionary.RangeDictionary["rampStart03"];
                        if (string.IsNullOrEmpty(doubleString) == false)
                        {
                            rangeStartsFromMapFile.Add(Convert.ToDouble(doubleString));
                        }
                        doubleString = provider.ClassRangesDictionary.RangeDictionary["rampStart04"];
                        if (string.IsNullOrEmpty(doubleString) == false)
                        {
                            rangeStartsFromMapFile.Add(Convert.ToDouble(doubleString));
                        }
                        doubleString = provider.ClassRangesDictionary.RangeDictionary["rampStart05"];
                        if (string.IsNullOrEmpty(doubleString) == false)
                        {
                            rangeStartsFromMapFile.Add(Convert.ToDouble(doubleString));
                        }
                        doubleString = provider.ClassRangesDictionary.RangeDictionary["rampStart06"];
                        if (string.IsNullOrEmpty(doubleString) == false)
                        {
                            rangeStartsFromMapFile.Add(Convert.ToDouble(doubleString));
                        }
                        doubleString = provider.ClassRangesDictionary.RangeDictionary["rampStart07"];
                        if (string.IsNullOrEmpty(doubleString) == false)
                        {
                            rangeStartsFromMapFile.Add(Convert.ToDouble(doubleString));
                        }
                        doubleString = provider.ClassRangesDictionary.RangeDictionary["rampStart08"];
                        if (string.IsNullOrEmpty(doubleString) == false)
                        {
                            rangeStartsFromMapFile.Add(Convert.ToDouble(doubleString));
                        }
                        doubleString = provider.ClassRangesDictionary.RangeDictionary["rampStart09"];
                        if (string.IsNullOrEmpty(doubleString) == false)
                        {
                            rangeStartsFromMapFile.Add(Convert.ToDouble(doubleString));
                        }
                        doubleString = provider.ClassRangesDictionary.RangeDictionary["rampStart10"];
                        if (string.IsNullOrEmpty(doubleString) == false)
                        {
                            rangeStartsFromMapFile.Add(Convert.ToDouble(doubleString));
                        }
                    }
                    catch { }

                    provider.RangeStartsFromMapFile = rangeStartsFromMapFile;
                    provider.RangesLoadedFromMapFile = true;
                }

                if (child.Name.Equals("legTitle"))
                {
                    provider.LegendText = child.InnerText;
                }
            }

        }
        
        public static XmlNode Serialize(System.Xml.XmlDocument doc, ChoroplethLayerProvider provider, string dataKey, string shapeKey, string value, string classCount, SolidColorBrush highColor, SolidColorBrush lowColor, SolidColorBrush missingColor, string uniqueXmlString, DashboardHelper dashboardHelper, XmlAttribute type)
        {
            try
            {
                string classTitles = "<classTitles>" + Environment.NewLine;
                long classTitleCount = 0;
                string classTitleTagName = "";

                foreach (KeyValuePair<string, string> entry in provider.ListLegendText.Dict)
                {
                    classTitleTagName = entry.Key;
                    classTitles += string.Format("<{1}>{0}</{1}>", entry.Value, classTitleTagName) + Environment.NewLine;
                    classTitleCount++;
                }

                classTitles += "</classTitles>" + Environment.NewLine;

                string customColors = "";

                if (provider.CustomColorsDictionary != null)
                {
                    customColors = "<customColors>" + Environment.NewLine;

                    foreach (KeyValuePair<string, Color> keyValuePair in provider.CustomColorsDictionary.Dict)
                    {
                        string customColor = "<" + keyValuePair.Key + ">";
                        string color = keyValuePair.Value.R + "," + keyValuePair.Value.G + "," + keyValuePair.Value.B;
                        customColor += color + "</" + keyValuePair.Key + ">" + Environment.NewLine;
                        customColors += customColor;
                    }

                    customColors += "</customColors>" + Environment.NewLine;
                }

                string useCustomColorsTag = "<useCustomColors>" + provider.UseCustomColors + "</useCustomColors>" + Environment.NewLine;
                string asQuintileTag = "<partitionUsingQuantiles>" + provider.UseQuantiles + "</partitionUsingQuantiles>" + Environment.NewLine;
                string classRanges = "";

                if (provider.UseQuantiles == false)
                {
                    if (provider.ClassRangesDictionary != null)
                    {
                        classRanges = "<classRanges>" + Environment.NewLine;

                        foreach (KeyValuePair<string, string> keyValuePair in provider.ClassRangesDictionary.RangeDictionary)
                        {
                            string rangeName = "<" + keyValuePair.Key + ">";
                            string rangeValue = keyValuePair.Value;
                            rangeName += rangeValue + "</" + keyValuePair.Key + ">" + Environment.NewLine;
                            classRanges += rangeName;
                        }

                        classRanges += "</classRanges>" + Environment.NewLine;
                    }
                }

                string xmlString = uniqueXmlString +
                                   "<highColor>" + highColor.Color.ToString() + "</highColor>" + Environment.NewLine +
                                   "<legTitle>" + provider.LegendText + "</legTitle>" + Environment.NewLine +
                                   classTitles +
                                   classRanges +
                                   useCustomColorsTag +
                                   asQuintileTag +
                                   customColors +
                                   "<lowColor>" + lowColor.Color.ToString() + "</lowColor>" + Environment.NewLine +
                                   "<missingColor>" + missingColor.Color.ToString() + "</missingColor>" + Environment.NewLine +
                                   "<classes>" + classCount + "</classes>" + Environment.NewLine +
                                   "<dataKey>" + dataKey + "</dataKey>" + Environment.NewLine +
                                   "<shapeKey>" + shapeKey + "</shapeKey>" + Environment.NewLine +
                                   "<value>" + value + "</value>" + Environment.NewLine;

                doc.PreserveWhitespace = true;

                XmlElement element = doc.CreateElement("dataLayer");
                element.InnerXml = xmlString;
                element.AppendChild(dashboardHelper.Serialize(doc));

                element.Attributes.Append(type);

                return element;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        protected void ChoroplethProperties_RenderMap(ChoroplethLayerProvider provider)
        {
            dynamic layerPropertiesControl = this;

            EpiDashboard.Controls.ChoroplethProperties choroplethprop = new Controls.ChoroplethProperties(mapControl as StandaloneMapControl, myMap);

            if (provider is ChoroplethShapeLayerProvider)
            {
                choroplethprop.txtShapePath.Text = boundryFilePath;
            }
            else if (provider is ChoroplethKmlLayerProvider)
            {
                choroplethprop.txtKMLpath.Text = boundryFilePath;
            }
            else if (provider is ChoroplethServerLayerProvider)
            {
                choroplethprop.txtMapSeverpath.Text = boundryFilePath;
            }

            choroplethprop.txtProjectPath.Text = dashboardHelper.Database.DataSource;
            choroplethprop.SetDashboardHelper(dashboardHelper);
            choroplethprop.cmbClasses.Text = layerPropertiesControl.cbxClasses.Text;

            foreach (string str in layerPropertiesControl.cbxShapeKey.Items)
            { 
                choroplethprop.cmbShapeKey.Items.Add(str); 
            }

            foreach (string str in layerPropertiesControl.cbxDataKey.Items)
            {
                choroplethprop.cmbDataKey.Items.Add(str);
            }

            foreach (string str in layerPropertiesControl.cbxValue.Items)
            {
                choroplethprop.cmbValue.Items.Add(str);
            }

            choroplethprop.cmbShapeKey.SelectedItem = layerPropertiesControl.cbxShapeKey.Text;
            choroplethprop.cmbDataKey.SelectedItem = layerPropertiesControl.cbxDataKey.Text;

            choroplethprop.cmbValue.SelectionChanged -= new System.Windows.Controls.SelectionChangedEventHandler(choroplethprop.cmbValue_SelectionChanged);
            choroplethprop.cmbValue.SelectedItem = layerPropertiesControl.cbxValue.Text;
            choroplethprop.cmbValue.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(choroplethprop.cmbValue_SelectionChanged);

            choroplethprop.rctHighColor.Fill = layerPropertiesControl.rctHighColor.Fill;
            choroplethprop.rctLowColor.Fill = layerPropertiesControl.rctLowColor.Fill;
            choroplethprop.rctMissingColor.Fill = layerPropertiesControl.rctMissingColor.Fill;

            if(provider is ChoroplethShapeLayerProvider)
            {
                choroplethprop.radShapeFile.IsChecked = true;
                choroplethprop.choroplethShapeLayerProvider = (ChoroplethShapeLayerProvider)provider;
            }
            else if (provider is ChoroplethKmlLayerProvider)
            {
                choroplethprop.radKML.IsChecked = true;
                choroplethprop.choroplethKmlLayerProvider = (ChoroplethKmlLayerProvider)provider;
            }
            else if (provider is ChoroplethServerLayerProvider)
            {
                choroplethprop.radMapServer.IsChecked = true;
                choroplethprop.choroplethServerLayerProvider = (ChoroplethServerLayerProvider)provider;
            }

            choroplethprop.LayerProvider = provider;

            choroplethprop.legTitle.Text = provider.LegendText;
            choroplethprop.ListLegendText = provider.ListLegendText;

            choroplethprop.SetProperties();

            choroplethprop.RenderMap();
        }
    }
}
