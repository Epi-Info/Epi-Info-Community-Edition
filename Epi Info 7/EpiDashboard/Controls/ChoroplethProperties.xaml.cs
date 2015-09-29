using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Epi;
using EpiDashboard.Mapping;
using System.Windows.Forms;
using System.Net;

namespace EpiDashboard.Controls
{
    /// <summary>
    /// Interaction logic for DashboardProperties.xaml
    /// </summary>
    public partial class ChoroplethProperties : System.Windows.Controls.UserControl
    {
        private EpiDashboard.Mapping.StandaloneMapControl _mapControl;
        private ESRI.ArcGIS.Client.Map _myMap;
        private DashboardHelper _dashboardHelper;
        public EpiDashboard.Mapping.ChoroplethLayerProvider _provider;
        private int _currentStratCount;
        private SolidColorBrush _currentColor_rampMissing;
        private SolidColorBrush _currentColor_rampStart;
        private SolidColorBrush _currentColor_rampEnd;
        private bool _initialRampCalc;
        public ChoroplethLayerProperties layerprop;

        public RowFilterControl rowFilterControl { get; set; }
        public DataFilters datafilters { get; set; }

        private System.Xml.XmlElement currentElement;
        public EpiDashboard.Mapping.ChoroplethKmlLayerProvider choroKMLprovider;
        public EpiDashboard.Mapping.ChoroplethServerLayerProvider choroMapprovider;
        public EpiDashboard.Mapping.ChoroplethServerLayerProperties choroserverlayerprop;
        public EpiDashboard.Mapping.ChoroplethKmlLayerProperties chorokmllayerprop;
        private string shapeFilePath;

        public List<string> ListLegendText = new List<string>();

        public IDictionary<string, object> shapeAttributes;
        private Dictionary<int, object> ClassAttribList = new Dictionary<int, object>();

        string _shapeKey;
        string _dataKey;
        string _value;

        private struct classAttributes
        {
           public Brush rctColor;
           public string quintile;
           public string rampStart;
           public string rampEnd;
           public string legendText;
        }

       
        public ChoroplethProperties(EpiDashboard.Mapping.StandaloneMapControl mapControl, ESRI.ArcGIS.Client.Map myMap)
        {
            InitializeComponent();
            _mapControl = mapControl;
            _myMap = myMap;
                     

            Epi.ApplicationIdentity appId = new Epi.ApplicationIdentity(typeof(Configuration).Assembly);
            tblockCurrentEpiVersion.Text = "Epi Info " + appId.Version;

            if (int.TryParse(cmbClasses.Text, out _currentStratCount) == false)
            {
                _currentStratCount = 4;
            }

            _currentColor_rampMissing = (SolidColorBrush)rctMissingColor.Fill;
            _currentColor_rampStart = (SolidColorBrush)rctLowColor.Fill;
            _currentColor_rampEnd = (SolidColorBrush)rctHighColor.Fill;
            _initialRampCalc = true;

            #region Translation

            
            tbtnFilters.Title = DashboardSharedStrings.GADGET_TABBUTTON_FILTERS;
            tbtnFilters.Description = DashboardSharedStrings.GADGET_TABDESC_FILTERS;
            tblockColorsSubheader.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_COLORS;

            #endregion // Translation
        }

        public event EventHandler Cancelled;
        public event EventHandler ChangesAccepted;

        public DashboardHelper DashboardHelper { get; private set; }

        public string DataSource { get; set; }

        private byte _opacity = 240;

        public byte Opacity
        {
            get { return _opacity; }
            set { _opacity = value; }
        }


        public FileInfo ProjectFileInfo
        {
            get
            {
                FileInfo fi = new FileInfo(txtProjectPath.Text);
                return fi;
            }
            set
            {
                txtProjectPath.Text = value.FullName;
                panelDataSourceProject.Visibility = Visibility.Visible;
                panelDataSourceOther.Visibility = Visibility.Collapsed;
                panelDataSourceAdvanced.Visibility = Visibility.Collapsed;
            }
        }

        public string ConnectionString
        {
            get
            {
                return txtDataSource.Text;
            }
            set
            {
                txtDataSource.Text = value;
                panelDataSourceProject.Visibility = Visibility.Collapsed;
                panelDataSourceOther.Visibility = Visibility.Visible;
                panelDataSourceAdvanced.Visibility = Visibility.Collapsed;
            }
        }

        public string SqlQuery
        {
            get
            {
                return txtSQLQuery.Text;
            }
            set
            {
                txtSQLQuery.Text = value;
                panelDataSourceProject.Visibility = Visibility.Collapsed;
                panelDataSourceOther.Visibility = Visibility.Collapsed;
                panelDataSourceAdvanced.Visibility = Visibility.Visible;
            }
        }

        public string MapServerName { get; set; }
        public int MapVisibleLayer { get; set; }
        public string KMLMapServerName { get; set; }
        public int KMLMapVisibleLayer { get; set; }

        private void tbtnInfo_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Visible;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnCharts_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Visible;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
            SetDefaultRanges();
        }

        public void SetDefaultRanges()
        {
            if (cmbShapeKey.SelectedItem != null &&
                cmbDataKey.SelectedItem != null &&
                cmbValue.SelectedItem != null)
            {

                if (_provider != null && _provider.Opacity > 0)
                {
                    Opacity = _provider.Opacity;
                    sliderOpacity.Value = Opacity;
                }
                else if (choroKMLprovider != null && choroKMLprovider.Opacity > 0)
                {
                    Opacity = choroKMLprovider.Opacity;
                    sliderOpacity.Value = Opacity;
                }
                else if (choroserverlayerprop != null && choroserverlayerprop.Opacity > 0)
                {

                    Opacity = choroserverlayerprop.Opacity;
                   // Opacity = choroMapprovider.Opacity;
                    sliderOpacity.Value = Opacity;
                }
                else
                {
                    Opacity = Convert.ToByte(sliderOpacity.Value);
                }


                SetOpacity();

                List<SolidColorBrush> brushList = new List<SolidColorBrush>() { 
                    (SolidColorBrush)rctColor01.Fill, 
                    (SolidColorBrush)rctColor02.Fill, 
                    (SolidColorBrush)rctColor03.Fill, 
                    (SolidColorBrush)rctColor04.Fill, 
                    (SolidColorBrush)rctColor05.Fill, 
                    (SolidColorBrush)rctColor06.Fill, 
                    (SolidColorBrush)rctColor07.Fill, 
                    (SolidColorBrush)rctColor08.Fill, 
                    (SolidColorBrush)rctColor09.Fill, 
                    (SolidColorBrush)rctColor10.Fill,  
                    (SolidColorBrush)rctColor00.Fill };

                int classCount;
                if (!int.TryParse(((ComboBoxItem)cmbClasses.SelectedItem).Content.ToString(), out classCount))
                {
                    classCount = 4;
                }

                if (_provider != null)
                {
                    _provider.PopulateRangeValues(_dashboardHelper,
                       cmbShapeKey.SelectedItem.ToString(),
                       cmbDataKey.SelectedItem.ToString(),
                       cmbValue.SelectedItem.ToString(),
                       brushList,
                       classCount);
                }
                else
                    if (choroKMLprovider != null)
                    {
                        choroKMLprovider.PopulateRangeValues(_dashboardHelper,
                           cmbShapeKey.SelectedItem.ToString(),
                           cmbDataKey.SelectedItem.ToString(),
                           cmbValue.SelectedItem.ToString(),
                           brushList,
                           classCount);
                    }
                SetRangeUISection();
            }
        }

        private void tbtnHTML_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelHTML.Visibility = System.Windows.Visibility.Visible;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
            if (choroserverlayerprop != null) 
                if (choroserverlayerprop.provider.FlagUpdateToGLFailed) { ResetShapeCombo(); }
           
        }
        private void tbtnFilters_Checked(object sender, RoutedEventArgs e)
        {
            if (panelDataSource == null) return;
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Visible;
        }

        private void tbtnDataSource_Checked(object sender, RoutedEventArgs e)
        {
            if (panelDataSource == null) return;
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Visible;
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void CheckButtonStates(ToggleButton sender)
        {
            foreach (UIElement element in panelSidebar.Children)
            {
                if (element is ToggleButton)
                {
                    ToggleButton tbtn = element as ToggleButton;
                    if (tbtn != sender)
                    {
                        tbtn.IsChecked = false;
                    }
                }
            }
        }
        private void txtProjectPath_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (cmbDataKey.SelectedIndex > -1 && cmbShapeKey.SelectedIndex > -1 && cmbValue.SelectedIndex > -1)
            {
                Addfilters();
                
                if ((_provider != null && !_provider.AreRangesSet) || 
                    (choroMapprovider != null && !choroMapprovider.AreRangesSet) ||
                    (choroKMLprovider != null && !choroKMLprovider.AreRangesSet)
                    )
                {
                    SetDefaultRanges();
                }

                ResetLegend_Click(this, new RoutedEventArgs());        

                RenderMap();


                if (_provider != null && !ValidateRangeInput())
                    return;

                AddClassAttributes();

                int numclasses = Convert.ToInt32(cmbClasses.Text);
                bool flagquintiles = (bool)quintilesOption.IsChecked;

                if (radShapeFile.IsChecked == true && _provider != null)
                {
                    

                    layerprop.SetValues(txtShapePath.Text, cmbShapeKey.Text, cmbDataKey.Text, cmbValue.Text, cmbClasses.Text, rctHighColor.Fill, rctLowColor.Fill, rctMissingColor.Fill, shapeAttributes, ClassAttribList, flagquintiles, numclasses); 
                }
                else if (radMapServer.IsChecked == true && choroMapprovider != null)
                {
                    choroserverlayerprop.SetValues(cmbShapeKey.Text, cmbDataKey.Text, cmbValue.Text, cmbClasses.Text, rctHighColor.Fill, rctLowColor.Fill, rctMissingColor.Fill, shapeAttributes, ClassAttribList, flagquintiles, numclasses, Opacity);
                    choroserverlayerprop.cbxMapserverText = cbxmapserver.Text;
                    choroserverlayerprop.txtMapserverText = txtMapSeverpath.Text;
                    choroserverlayerprop.cbxMapFeatureText = cbxmapfeature.Text;
                }

                else if (radKML.IsChecked == true && choroKMLprovider != null)
                { chorokmllayerprop.SetValues(cmbShapeKey.Text, cmbDataKey.Text, cmbValue.Text, cmbClasses.Text, rctHighColor.Fill, rctLowColor.Fill, rctMissingColor.Fill, shapeAttributes, ClassAttribList, flagquintiles, numclasses); }

                if (ChangesAccepted != null)
                {
                    ChangesAccepted(this, new EventArgs());
                }
            }
            else
                tbtnDataSource.IsChecked = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
           /* if (radShapeFile.IsChecked == true && _provider != null && layerprop.classAttribList == null) 
                { layerprop.CloseLayer(); }
            else if (radMapServer.IsChecked == true && choroMapprovider != null && choroserverlayerprop.classAttribList == null)
                { choroserverlayerprop.CloseLayer(); }
            else if (radKML.IsChecked == true && choroKMLprovider != null && chorokmllayerprop.classAttribList == null)
                { chorokmllayerprop.CloseLayer(); } */

            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            _dashboardHelper = _mapControl.GetNewDashboardHelper();
            if (_dashboardHelper != null)
            {
                this.DashboardHelper = _dashboardHelper;
                txtProjectPath.Text = _mapControl.ProjectFilepath;
                FillComboBoxes();
                panelBoundaries.IsEnabled = true;
                this.datafilters = new DataFilters(_dashboardHelper);
                rowFilterControl = new RowFilterControl(_dashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, datafilters, true);
                rowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left; rowFilterControl.FillSelectionComboboxes();
                panelFilters.Children.Add(rowFilterControl);
                txtNote.Text = "Note: Any filters set here are applied to this gadget only.";
            } 

         }

        public void FillComboBoxes()
        {
            cmbDataKey.Items.Clear();
            cmbValue.Items.Clear();
            List<string> fields = _dashboardHelper.GetFieldsAsList(); // dashboardHelper.GetFormFields();
            ColumnDataType columnDataType = ColumnDataType.Numeric;
            List<string> numericFields = _dashboardHelper.GetFieldsAsList(columnDataType); //dashboardHelper.GetNumericFormFields();
            foreach (string field in fields)
            {
                if (!(field.ToUpper() == "RECSTATUS" || field.ToUpper() == "FKEY" || field.ToUpper() == "GLOBALRECORDID" || field.ToUpper() == "UNIQUEKEY" || field.ToUpper() == "FIRSTSAVETIME" || field.ToUpper() == "LASTSAVETIME" || field.ToUpper() == "SYSTEMDATE"))
                { cmbDataKey.Items.Add(field); }
            }
            foreach (string field in numericFields)
            {
                if (!(field.ToUpper() == "RECSTATUS" || field.ToUpper() == "UNIQUEKEY"))
                { cmbValue.Items.Add(field); }
            }
            cmbValue.Items.Insert(0, "{Record Count}");
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }

        private void btnBrowseShapeFile_Click(object sender, RoutedEventArgs e)
        {
            _provider = new Mapping.ChoroplethLayerProvider(_myMap);
            object[] shapeFileProperties = _provider.LoadShapeFile();
            if (shapeFileProperties != null)
            {
                if (layerprop == null)
                {
                    ILayerProperties layerProperties = null;
                    layerProperties = new ChoroplethLayerProperties(_myMap, this.DashboardHelper, this._mapControl);
                    layerProperties.MapGenerated += new EventHandler(this._mapControl.ILayerProperties_MapGenerated);
                    layerProperties.FilterRequested += new EventHandler(this._mapControl.ILayerProperties_FilterRequested);
                    layerProperties.EditRequested += new EventHandler(this._mapControl.ILayerProperties_EditRequested);
                    this.layerprop = (ChoroplethLayerProperties)layerProperties;
                    this._mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                }

                layerprop.provider = _provider;
                if (this.DashboardHelper != null)
                    layerprop.SetdashboardHelper(DashboardHelper);

                if (shapeFileProperties.Length == 2)
                {
                    txtShapePath.Text = shapeFileProperties[0].ToString();
                    layerprop.shapeFilePath = shapeFileProperties[0].ToString();
                    layerprop.shapeAttributes = (IDictionary<string, object>)shapeFileProperties[1];
                    shapeAttributes = (IDictionary<string, object>)shapeFileProperties[1];
                    if (shapeAttributes != null)
                    {
                        cmbShapeKey.Items.Clear();
                        layerprop.cbxShapeKey.Items.Clear();
                        foreach (string key in shapeAttributes.Keys)
                        {
                            cmbShapeKey.Items.Add(key);
                            layerprop.cbxShapeKey.Items.Add(key);
                        }
                    }
                }
            }
        }

        private void AddClassAttributes()
        {

            ClassAttribList.Clear();

            classAttributes ca1 = new classAttributes();
            ca1.rctColor = rctColor01.Fill;
            ca1.rampStart = rampStart01.Text;
            ca1.rampEnd = rampEnd01.Text;
            ca1.quintile = quintile01.Text;
            ca1.legendText = legendText01.Text;
            ClassAttribList.Add(1, ca1);

            classAttributes ca2 = new classAttributes();
            ca2.rctColor = rctColor02.Fill;
            ca2.rampStart = rampStart02.Text;
            ca2.rampEnd = rampEnd02.Text;
            ca2.quintile = quintile02.Text;
            ca2.legendText = legendText02.Text;
            ClassAttribList.Add(2, ca2);

            classAttributes ca3 = new classAttributes();
            ca3.rctColor = rctColor03.Fill;
            ca3.rampStart = rampStart03.Text;
            ca3.rampEnd = rampEnd03.Text;
            ca3.quintile = quintile03.Text;
            ca3.legendText = legendText03.Text;
            ClassAttribList.Add(3, ca3);

            classAttributes ca4 = new classAttributes();
            ca4.rctColor = rctColor04.Fill;
            ca4.rampStart = rampStart04.Text;
            ca4.rampEnd = rampEnd04.Text;
            ca4.quintile = quintile04.Text;
            ca4.legendText = legendText04.Text;
            ClassAttribList.Add(4, ca4);

            classAttributes ca5 = new classAttributes();
            ca5.rctColor = rctColor05.Fill;
            ca5.rampStart = rampStart05.Text;
            ca5.rampEnd = rampEnd05.Text;
            ca5.quintile = quintile05.Text;
            ca5.legendText = legendText05.Text;
            ClassAttribList.Add(5, ca5);

            classAttributes ca6 = new classAttributes();
            ca6.rctColor = rctColor06.Fill;
            ca6.rampStart = rampStart06.Text;
            ca6.rampEnd = rampEnd06.Text;
            ca6.quintile = quintile06.Text;
            ca6.legendText = legendText06.Text;
            ClassAttribList.Add(6, ca6);

            classAttributes ca7 = new classAttributes();
            ca7.rctColor = rctColor07.Fill;
            ca7.rampStart = rampStart07.Text;
            ca7.rampEnd = rampEnd07.Text;
            ca7.quintile = quintile07.Text;
            ca7.legendText = legendText07.Text;
            ClassAttribList.Add(7, ca7);

            classAttributes ca8 = new classAttributes();
            ca8.rctColor = rctColor08.Fill;
            ca8.rampStart = rampStart08.Text;
            ca8.rampEnd = rampEnd08.Text;
            ca8.quintile = quintile08.Text;
            ca8.legendText = legendText08.Text;
            ClassAttribList.Add(8, ca8);

            classAttributes ca9 = new classAttributes();
            ca9.rctColor = rctColor09.Fill;
            ca9.rampStart = rampStart09.Text;
            ca9.rampEnd = rampEnd09.Text;
            ca9.quintile = quintile09.Text;
            ca9.legendText = legendText09.Text;
            ClassAttribList.Add(9, ca9);

            classAttributes ca10 = new classAttributes();
            ca10.rctColor = rctColor10.Fill;
            ca10.rampStart = rampStart10.Text;
            ca10.rampEnd = rampEnd10.Text;
            ca10.quintile = quintile10.Text;
            ca10.legendText = legendText10.Text;
            ClassAttribList.Add(10, ca10);
         }

        private bool ValidateInputValue(System.Windows.Controls.TextBox textBox)
        {
            double Value;
            if (double.TryParse(textBox.Text, out Value))
            {
                if (Value >= double.Parse(_provider.RangeValues[0, 0].ToString()) && Value <= double.Parse(_provider.RangeValues[_provider.RangeCount - 1, 1].ToString()))
                {
                    return true;
                }
            }
            return false;
        }

        private bool ValidateRangeInput()
        {
            int _rangeCount = _provider.RangeCount;
            
            if (_rangeCount >= 2)
            {
                if (!(ValidateInputValue(rampStart01) && ValidateInputValue(rampEnd01)
                    && ValidateInputValue(rampStart02) && ValidateInputValue(rampEnd02)))
                {
                    System.Windows.MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }

            if (_rangeCount >= 3)
            {
                if (!(ValidateInputValue(rampStart03) && ValidateInputValue(rampEnd03)))
                {
                    System.Windows.MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }

            if (_rangeCount >= 4)
            {
                if (!(ValidateInputValue(rampStart04) && ValidateInputValue(rampEnd04)))
                {
                    System.Windows.MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }
            if (_rangeCount >= 5)
            {
                if (!(ValidateInputValue(rampStart05) && ValidateInputValue(rampEnd05)))
                {
                    System.Windows.MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }
            if (_rangeCount >= 6)
            {
                if (!(ValidateInputValue(rampStart06) && ValidateInputValue(rampEnd06)))
                {
                    System.Windows.MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }
            if (_rangeCount >= 7)
            {
                if (!(ValidateInputValue(rampStart07) && ValidateInputValue(rampEnd07)))
                {
                    System.Windows.MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }
            if (_rangeCount >= 8)
            {
                if (!(ValidateInputValue(rampStart08) && ValidateInputValue(rampEnd08)))
                {
                    System.Windows.MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }
            if (_rangeCount >= 9)
            {
                if (!(ValidateInputValue(rampStart09) && ValidateInputValue(rampEnd09)))
                {
                    System.Windows.MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }
            if (_rangeCount >= 10)
            {
                if (!(ValidateInputValue(rampStart10) && ValidateInputValue(rampEnd10)))
                {
                    System.Windows.MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }
            return true;
        }

        public void SetClassAttributes(Dictionary<int, object> classAttrib)
        {

            foreach (int key in classAttrib.Keys)
            {
                var item = classAttrib.ElementAt(key - 1);
                classAttributes itemvalue = (classAttributes)item.Value;

               if (key == 1)
               {
                    rctColor01.Fill = (Brush)itemvalue.rctColor;
                    rampStart01.Text = itemvalue.rampStart;
                    rampEnd01.Text = itemvalue.rampEnd;
                    quintile01.Text = itemvalue.quintile;
                    legendText01.Text = itemvalue.legendText;
               }
               else if (key == 2)
               {
                   rctColor02.Fill = (Brush)itemvalue.rctColor;
                   rampStart02.Text = itemvalue.rampStart;
                   rampEnd02.Text = itemvalue.rampEnd;
                   quintile02.Text = itemvalue.quintile;
                   legendText02.Text = itemvalue.legendText;
               }
               else if (key == 3)
               {
                   rctColor03.Fill = (Brush)itemvalue.rctColor;
                   rampStart03.Text = itemvalue.rampStart;
                   rampEnd03.Text = itemvalue.rampEnd;
                   quintile03.Text = itemvalue.quintile;
                   legendText03.Text = itemvalue.legendText;
               }
               else if (key == 4)
               {
                   rctColor04.Fill = (Brush)itemvalue.rctColor;
                   rampStart04.Text = itemvalue.rampStart;
                   rampEnd04.Text = itemvalue.rampEnd;
                   quintile04.Text = itemvalue.quintile;
                   legendText04.Text = itemvalue.legendText;
               }
               else if (key == 5)
               {
                  rctColor05.Fill = (Brush)itemvalue.rctColor;
                  rampStart05.Text = itemvalue.rampStart;
                  rampEnd05.Text = itemvalue.rampEnd;
                  quintile05.Text = itemvalue.quintile;
                  legendText05.Text = itemvalue.legendText;
                
               }
               else if (key == 6)
               {
                   rctColor06.Fill = (Brush)itemvalue.rctColor;
                   rampStart06.Text = itemvalue.rampStart;
                   rampEnd06.Text = itemvalue.rampEnd;
                   quintile06.Text = itemvalue.quintile;
                   legendText06.Text = itemvalue.legendText; 
                 
               }
               else if (key == 7)
               {
                   rctColor07.Fill = (Brush)itemvalue.rctColor;
                   rampStart07.Text = itemvalue.rampStart;
                   rampEnd07.Text = itemvalue.rampEnd;
                   quintile07.Text = itemvalue.quintile;
                   legendText07.Text = itemvalue.legendText;
               }
               else if (key == 8)
               {
                    rctColor08.Fill = (Brush)itemvalue.rctColor;
                   rampStart08.Text = itemvalue.rampStart;
                   rampEnd08.Text = itemvalue.rampEnd;
                   quintile08.Text = itemvalue.quintile;
                   legendText08.Text = itemvalue.legendText;
                 
               }
               else if (key == 9)
               {
                  rctColor09.Fill = (Brush)itemvalue.rctColor;
                  rampStart09.Text = itemvalue.rampStart;
                  rampEnd09.Text = itemvalue.rampEnd;
                  quintile09.Text = itemvalue.quintile;
                  legendText09.Text = itemvalue.legendText;
              
               }
               else if (key == 10)
               {
                   rctColor10.Fill = (Brush)itemvalue.rctColor;
                   rampStart10.Text = itemvalue.rampStart;
                   rampEnd10.Text = itemvalue.rampEnd;
                   quintile10.Text = itemvalue.quintile;
                   legendText10.Text = itemvalue.legendText;
               }
            }

        }


        public void SetDashboardHelper(DashboardHelper dash)
        {
            _dashboardHelper = dash;
        }

        private HttpWebRequest CreateWebRequest(string endPoint)
        {
            var request = (HttpWebRequest)WebRequest.Create(endPoint);
            request.Method = "GET";
            request.ContentLength = 0;
            request.ContentType = "text/xml";
            return request;
        }

        public string GetMessage(string endPoint)
        {
            HttpWebRequest request = CreateWebRequest(endPoint);
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseValue = string.Empty;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }
                using (var responseStream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(responseStream))
                    {
                        responseValue = reader.ReadToEnd();
                    }
                }
                return responseValue;
            }
        }
        public void RenderMap()
        {
            if (cmbDataKey.SelectedIndex != -1 && cmbShapeKey.SelectedIndex != -1 && cmbValue.SelectedIndex != -1)
            {
                string shapeKey = cmbShapeKey.SelectedItem.ToString();
                string dataKey = cmbDataKey.SelectedItem.ToString();
                string value = cmbValue.SelectedItem.ToString();
                string missingText = legendText00.Text.ToString();
                Opacity = Convert.ToByte(sliderOpacity.Value);
                SetOpacity();

                List<SolidColorBrush> brushList = new List<SolidColorBrush>() { 
                    (SolidColorBrush)rctColor01.Fill, 
                    (SolidColorBrush)rctColor02.Fill, 
                    (SolidColorBrush)rctColor03.Fill, 
                    (SolidColorBrush)rctColor04.Fill, 
                    (SolidColorBrush)rctColor05.Fill, 
                    (SolidColorBrush)rctColor06.Fill, 
                    (SolidColorBrush)rctColor07.Fill, 
                    (SolidColorBrush)rctColor08.Fill, 
                    (SolidColorBrush)rctColor09.Fill, 
                    (SolidColorBrush)rctColor10.Fill,  
                    (SolidColorBrush)rctColor00.Fill};

                int classCount;
                if (!int.TryParse(cmbClasses.Text, out classCount))
                {
                    classCount = 4;
                }



                if (radShapeFile.IsChecked == true && _provider != null)
                {
                    _provider.Range = GetRangeValues(_provider.RangeCount);
                    _provider.ListLegendText = ListLegendText;
                    _provider.Opacity = this.Opacity;
                _provider.SetShapeRangeValues(_dashboardHelper, 
                    cmbShapeKey.SelectedItem.ToString(),
                    cmbDataKey.SelectedItem.ToString(),
                    cmbValue.SelectedItem.ToString(),
                    brushList,
                    classCount,
                    missingText);
                }
                else if (radMapServer.IsChecked == true && choroMapprovider != null)
                {
                    choroMapprovider.Range = GetRangeValues(choroMapprovider.RangeCount);
                    choroMapprovider.ListLegendText = ListLegendText;
                    choroMapprovider.Opacity = this.Opacity;
                    choroMapprovider.SetShapeRangeValues(_dashboardHelper,
                    cmbShapeKey.SelectedItem.ToString(),
                    cmbDataKey.SelectedItem.ToString(),
                    cmbValue.SelectedItem.ToString(),
                    brushList,
                    classCount,
                    missingText);
                }
                else if (radKML.IsChecked == true && choroKMLprovider != null)
                {
                    choroKMLprovider.Range = GetRangeValues(choroKMLprovider.RangeCount);
                    choroKMLprovider.ListLegendText = ListLegendText;
                    choroKMLprovider.Opacity = this.Opacity;
                    choroKMLprovider.SetShapeRangeValues(_dashboardHelper,
                    cmbShapeKey.SelectedItem.ToString(),
                    cmbDataKey.SelectedItem.ToString(),
                    cmbValue.SelectedItem.ToString(),
                    brushList,
                    classCount,
                    missingText);
                }
               /* _provider.SetShapeRangeValues(_dashboardHelper, 
                    cmbShapeKey.SelectedItem.ToString(),
                    cmbDataKey.SelectedItem.ToString(),
                    cmbValue.SelectedItem.ToString(),
                    brushList,
                    classCount); */
           }
        }
        private void SetOpacity()
        {
            Color col = (rctColor00.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctColor00.Fill = new SolidColorBrush(col);

            col = (rctColor01.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctColor01.Fill = new SolidColorBrush(col);

            col = (rctColor02.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctColor02.Fill = new SolidColorBrush(col);

            col = (rctColor03.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctColor03.Fill = new SolidColorBrush(col);

            col = (rctColor04.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctColor04.Fill = new SolidColorBrush(col);

            col = (rctColor05.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctColor05.Fill = new SolidColorBrush(col);

            col = (rctColor06.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctColor06.Fill = new SolidColorBrush(col);

            col = (rctColor07.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctColor07.Fill = new SolidColorBrush(col);

            col = (rctColor08.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctColor08.Fill = new SolidColorBrush(col);

            col = (rctColor09.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctColor09.Fill = new SolidColorBrush(col);

            col = (rctColor10.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctColor10.Fill = new SolidColorBrush(col);

            col = (rctMissingColor.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctMissingColor.Fill = new SolidColorBrush(col);
        }

        public List<double> GetRangeValues(int RangeCount)
        {
            List<double> Range = new List<double>();

            //int _rangeCount = _provider.RangeCount;
            double value = 0.0;

            if (RangeCount >= 2)
            {
                double.TryParse(rampStart01.Text, out value);
                Range.Add(value);

                ListLegendText.Add(legendText01.Text);

                double.TryParse(rampStart02.Text, out value);
                Range.Add(value);

                ListLegendText.Add(legendText02.Text);
            }


            if (RangeCount >= 3)
            {
                double.TryParse(rampStart03.Text, out value);
                Range.Add(value);
                ListLegendText.Add(legendText03.Text);
            }
            if (RangeCount >= 4)
            {
                double.TryParse(rampStart04.Text, out value);
                Range.Add(value);
                ListLegendText.Add(legendText04.Text);
            }
            if (RangeCount >= 5)
            {
                double.TryParse(rampStart05.Text, out value);
                Range.Add(value);
                ListLegendText.Add(legendText05.Text);
            }
            if (RangeCount >= 6)
            {
                double.TryParse(rampStart06.Text, out value);
                Range.Add(value);
                ListLegendText.Add(legendText06.Text);
            }
            if (RangeCount >= 7)
            {
                double.TryParse(rampStart07.Text, out value);
                Range.Add(value);
                ListLegendText.Add(legendText07.Text);
            }
            if (RangeCount >= 8)
            {
                double.TryParse(rampStart08.Text, out value);
                Range.Add(value);
                ListLegendText.Add(legendText08.Text);
        }
            if (RangeCount >= 9)
            {
                double.TryParse(rampStart09.Text, out value);
                Range.Add(value);
                ListLegendText.Add(legendText09.Text);
            }
            if (RangeCount >= 10)
            {
                double.TryParse(rampStart10.Text, out value);
                Range.Add(value);
                ListLegendText.Add(legendText10.Text);
            }

            return Range;

        }
        public StackPanel LegendStackPanel
        {
            get
            {
                return _provider.LegendStackPanel;
            }
        }
        private void rctColor0_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor00.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }
        private void rctColor1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor01.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor2_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor02.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor3_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor03.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor4_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor04.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor5_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor05.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor6_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor06.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor7_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor07.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor8_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor08.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor9_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor09.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor10_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor10.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctMissingColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctMissingColor.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctLowColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctLowColor.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctHighColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctHighColor.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void ResetLegend_Click(object sender, RoutedEventArgs e)
        {
            if (rctLowColor == null || rctHighColor == null)
            {
                return;
            }

            int stratCount;

            if (int.TryParse(cmbClasses.Text, out stratCount) == false)
            {
                stratCount = 4;
            }
            
            int classCount;
            if (!int.TryParse(((ComboBoxItem)cmbClasses.SelectedItem).Content.ToString(), out classCount))
            {
                classCount = 4;
            }

            if ((cmbShapeKey.SelectedItem != null && cmbDataKey.SelectedItem != null && cmbValue.SelectedItem != null) && (_provider != null))
            {
                _provider.ResetRangeValues(
                          cmbShapeKey.SelectedItem.ToString(),
                          cmbDataKey.SelectedItem.ToString(),
                          cmbValue.SelectedItem.ToString(),
                          classCount
                  );
            }
            else if ((cmbShapeKey.SelectedItem != null && cmbDataKey.SelectedItem != null && cmbValue.SelectedItem != null) && (choroMapprovider != null))
                 { choroMapprovider.ResetRangeValues(); }
            else if ((cmbShapeKey.SelectedItem != null && cmbDataKey.SelectedItem != null && cmbValue.SelectedItem != null) && (choroKMLprovider != null))
                 { choroKMLprovider.ResetRangeValues(); }

            SolidColorBrush rampStart = (SolidColorBrush)rctLowColor.Fill;
            SolidColorBrush rampEnd = (SolidColorBrush)rctHighColor.Fill;

            if (cmbShapeKey.SelectedItem != null &&
                cmbDataKey.SelectedItem != null &&
                cmbValue.SelectedItem != null)
            {
                SetRangeUISection();
            }
        }

        public void SetVisibility(int stratCount, SolidColorBrush rampStart, SolidColorBrush rampEnd)
        {
            bool isNewColorRamp = true;

            //Color col = (Color)ColorConverter.ConvertFromString("Yellow");

            //SolidColorBrush rampMissing = new SolidColorBrush(col);

            SolidColorBrush rampMissing = (SolidColorBrush)rctMissingColor.Fill;

            if (rampStart == _currentColor_rampStart && 
                rampEnd == _currentColor_rampEnd && 
                rampMissing == _currentColor_rampMissing &&
                stratCount == _currentStratCount)
            {
                if (_initialRampCalc == false)
                {
                    isNewColorRamp = false;
                }
            }

            _currentStratCount = stratCount;
            _currentColor_rampStart = rampStart;
            _currentColor_rampMissing = rampMissing;
            _currentColor_rampEnd = rampEnd;

            int rd = rampStart.Color.R - rampEnd.Color.R;
            int gd = rampStart.Color.G - rampEnd.Color.G;
            int bd = rampStart.Color.B - rampEnd.Color.B;

            byte ri = (byte)(rd / (stratCount - 1));
            byte gi = (byte)(gd / (stratCount - 1));
            byte bi = (byte)(bd / (stratCount - 1));

            if (isNewColorRamp) rctColor00.Fill = rampMissing;
            if (isNewColorRamp) rctColor01.Fill = rampStart;

            if (radShapeFile.IsChecked == true && _provider != null)
            {
                rampStart01.Text = _provider.RangeValues[0, 0];
                rampEnd01.Text = _provider.RangeValues[0, 1];
                quintile01.Text = _provider.QuantileValues[0].ToString();
            }
            else if (radMapServer.IsChecked == true && choroMapprovider != null)
            {
                rampStart01.Text = choroMapprovider.RangeValues[0, 0];
                rampEnd01.Text = choroMapprovider.RangeValues[0, 1];
                quintile01.Text = choroMapprovider.QuantileValues[0].ToString();
            }
            else if (radKML.IsChecked == true && choroKMLprovider != null)
            {
                rampStart01.Text = choroKMLprovider.RangeValues[0, 0];
                rampEnd01.Text = choroKMLprovider.RangeValues[0, 1];
                quintile01.Text = choroKMLprovider.QuantileValues[0].ToString();
            }
             

            Color coo = Color.FromArgb(Opacity, (byte)(rampStart.Color.R - ri), (byte)(rampStart.Color.G - gi), (byte)(rampStart.Color.B - bi));
            if (isNewColorRamp) rctColor02.Fill = new SolidColorBrush(coo);

            if (radShapeFile.IsChecked == true && _provider != null)
            {
                rampStart02.Text = _provider.RangeValues[1, 0];
                rampEnd02.Text = _provider.RangeValues[1, 1];
                quintile02.Text = _provider.QuantileValues[1].ToString();
            }
            else if (radMapServer.IsChecked == true && choroMapprovider != null)
            {
                rampStart02.Text = choroMapprovider.RangeValues[1, 0];
                rampEnd02.Text = choroMapprovider.RangeValues[1, 1];
                quintile02.Text = choroMapprovider.QuantileValues[1].ToString();
            }
            else if (radKML.IsChecked == true && choroKMLprovider != null)
            {
                rampStart02.Text = choroKMLprovider.RangeValues[1, 0];
                rampEnd02.Text = choroKMLprovider.RangeValues[1, 1];
                quintile02.Text = choroKMLprovider.QuantileValues[1].ToString();
            }

            int i = 3;


            coo = Color.FromArgb(Opacity, (byte)(rampStart.Color.R - ri * 2), (byte)(rampStart.Color.G - gi * 2), (byte)(rampStart.Color.B - bi * 2));
            rctColor03.Visibility = System.Windows.Visibility.Visible;
            rampStart03.Visibility = System.Windows.Visibility.Visible;
            centerText03.Visibility = System.Windows.Visibility.Visible;
            rampEnd03.Visibility = System.Windows.Visibility.Visible;
            quintile03.Visibility = System.Windows.Visibility.Visible;
            legendText03.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(Opacity, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor03.Visibility = System.Windows.Visibility.Hidden;
                rctColor03.Visibility = System.Windows.Visibility.Hidden;
                rampStart03.Visibility = System.Windows.Visibility.Hidden;
                centerText03.Visibility = System.Windows.Visibility.Hidden;
                rampEnd03.Visibility = System.Windows.Visibility.Hidden;
                quintile03.Visibility = System.Windows.Visibility.Hidden;
                legendText03.Visibility = System.Windows.Visibility.Hidden;
            }
            if (isNewColorRamp) rctColor03.Fill = new SolidColorBrush(coo);

            if (radShapeFile.IsChecked == true && _provider != null)
            {
                rampStart03.Text = _provider.RangeValues[i - 2, 0];
                rampEnd03.Text = _provider.RangeValues[i - 2, 1];
                quintile03.Text = _provider.QuantileValues[i - 2].ToString();
            }
            else if (radMapServer.IsChecked == true && choroMapprovider != null)
            {
                rampStart03.Text = choroMapprovider.RangeValues[i - 2, 0];
                rampEnd03.Text = choroMapprovider.RangeValues[i - 2, 1];
                quintile03.Text = choroMapprovider.QuantileValues[i - 2].ToString();
            }
            else if (radKML.IsChecked == true && choroKMLprovider != null)
            {
                rampStart03.Text = choroKMLprovider.RangeValues[i - 2, 0];
                rampEnd03.Text = choroKMLprovider.RangeValues[i - 2, 1];
                quintile03.Text = choroKMLprovider.QuantileValues[i - 2].ToString();
            }

            coo = Color.FromArgb(Opacity, (byte)(rampStart.Color.R - ri * 3), (byte)(rampStart.Color.G - gi * 3), (byte)(rampStart.Color.B - bi * 3));
            rctColor04.Visibility = System.Windows.Visibility.Visible;
            rampStart04.Visibility = System.Windows.Visibility.Visible;
            centerText04.Visibility = System.Windows.Visibility.Visible;
            rampEnd04.Visibility = System.Windows.Visibility.Visible;
            quintile04.Visibility = System.Windows.Visibility.Visible;
            legendText04.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(Opacity, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor04.Visibility = System.Windows.Visibility.Hidden;
                rampStart04.Visibility = System.Windows.Visibility.Hidden;
                centerText04.Visibility = System.Windows.Visibility.Hidden;
                rampEnd04.Visibility = System.Windows.Visibility.Hidden;
                quintile04.Visibility = System.Windows.Visibility.Hidden;
                legendText04.Visibility = System.Windows.Visibility.Hidden;
            }
            if (isNewColorRamp) rctColor04.Fill = new SolidColorBrush(coo);

            if (radShapeFile.IsChecked == true && _provider != null)
            {
                rampStart04.Text = _provider.RangeValues[i - 2, 0];
                rampEnd04.Text = _provider.RangeValues[i - 2, 1];
                quintile04.Text = _provider.QuantileValues[i - 2].ToString();
            }
            else if (radMapServer.IsChecked == true && choroMapprovider != null)
            {
                rampStart04.Text = choroMapprovider.RangeValues[i - 2, 0];
                rampEnd04.Text = choroMapprovider.RangeValues[i - 2, 1];
                quintile04.Text = choroMapprovider.QuantileValues[i - 2].ToString();
            }
            else if (radKML.IsChecked == true && choroKMLprovider != null)
            {
                rampStart04.Text = choroKMLprovider.RangeValues[i - 2, 0];
                rampEnd04.Text = choroKMLprovider.RangeValues[i - 2, 1];
                quintile04.Text = choroKMLprovider.QuantileValues[i - 2].ToString();
            }

            coo = Color.FromArgb(Opacity, (byte)(rampStart.Color.R - ri * 4), (byte)(rampStart.Color.G - gi * 4), (byte)(rampStart.Color.B - bi * 4));
            rctColor05.Visibility = System.Windows.Visibility.Visible;
            rampStart05.Visibility = System.Windows.Visibility.Visible;
            centerText05.Visibility = System.Windows.Visibility.Visible;
            rampEnd05.Visibility = System.Windows.Visibility.Visible;
            quintile05.Visibility = System.Windows.Visibility.Visible;
            legendText05.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(Opacity, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor05.Visibility = System.Windows.Visibility.Hidden;
                rampStart05.Visibility = System.Windows.Visibility.Hidden;
                centerText05.Visibility = System.Windows.Visibility.Hidden;
                rampEnd05.Visibility = System.Windows.Visibility.Hidden;
                quintile05.Visibility = System.Windows.Visibility.Hidden;
                legendText05.Visibility = System.Windows.Visibility.Hidden;
            }
            if (isNewColorRamp) rctColor05.Fill = new SolidColorBrush(coo);

            if (radShapeFile.IsChecked == true && _provider != null)
            {
                rampStart05.Text = _provider.RangeValues[i - 2, 0];
                rampEnd05.Text = _provider.RangeValues[i - 2, 1];
                quintile05.Text = _provider.QuantileValues[i - 2].ToString();
            }
            else if (radMapServer.IsChecked == true && choroMapprovider != null)
            {
                rampStart05.Text = choroMapprovider.RangeValues[i - 2, 0];
                rampEnd05.Text = choroMapprovider.RangeValues[i - 2, 1];
                quintile05.Text = choroMapprovider.QuantileValues[i - 2].ToString();
            }
            else if (radKML.IsChecked == true && choroKMLprovider != null)
            {
                rampStart05.Text = choroKMLprovider.RangeValues[i - 2, 0];
                rampEnd05.Text = choroKMLprovider.RangeValues[i - 2, 1];
                quintile05.Text = choroKMLprovider.QuantileValues[i - 2].ToString();
            }

            coo = Color.FromArgb(Opacity, (byte)(rampStart.Color.R - ri * 5), (byte)(rampStart.Color.G - gi * 5), (byte)(rampStart.Color.B - bi * 5));
            rctColor06.Visibility = System.Windows.Visibility.Visible;
            rampStart06.Visibility = System.Windows.Visibility.Visible;
            centerText06.Visibility = System.Windows.Visibility.Visible;
            rampEnd06.Visibility = System.Windows.Visibility.Visible;
            quintile06.Visibility = System.Windows.Visibility.Visible;
            legendText06.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(Opacity, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor06.Visibility = System.Windows.Visibility.Hidden;
                rampStart06.Visibility = System.Windows.Visibility.Hidden;
                centerText06.Visibility = System.Windows.Visibility.Hidden;
                rampEnd06.Visibility = System.Windows.Visibility.Hidden;
                quintile06.Visibility = System.Windows.Visibility.Hidden;
                legendText06.Visibility = System.Windows.Visibility.Hidden;
            }
            if (isNewColorRamp) rctColor06.Fill = new SolidColorBrush(coo);

            if (radShapeFile.IsChecked == true && _provider != null)
            {
                rampStart06.Text = _provider.RangeValues[i - 2, 0];
                rampEnd06.Text = _provider.RangeValues[i - 2, 1];
                quintile06.Text = _provider.QuantileValues[i - 2].ToString();
            }
            else if (radMapServer.IsChecked == true && choroMapprovider != null)
            {
                rampStart06.Text = choroMapprovider.RangeValues[i - 2, 0];
                rampEnd06.Text = choroMapprovider.RangeValues[i - 2, 1];
                quintile06.Text = choroMapprovider.QuantileValues[i - 2].ToString();
            }
            else if (radKML.IsChecked == true && choroKMLprovider != null)
            {
                rampStart06.Text = choroKMLprovider.RangeValues[i - 2, 0];
                rampEnd06.Text = choroKMLprovider.RangeValues[i - 2, 1];
                quintile06.Text = choroKMLprovider.QuantileValues[i - 2].ToString();
            }

            coo = Color.FromArgb(Opacity, (byte)(rampStart.Color.R - ri * 6), (byte)(rampStart.Color.G - gi * 6), (byte)(rampStart.Color.B - bi * 6));
            rctColor07.Visibility = System.Windows.Visibility.Visible;
            rampStart07.Visibility = System.Windows.Visibility.Visible;
            centerText07.Visibility = System.Windows.Visibility.Visible;
            rampEnd07.Visibility = System.Windows.Visibility.Visible;
            quintile07.Visibility = System.Windows.Visibility.Visible;
            legendText07.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(Opacity, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor07.Visibility = System.Windows.Visibility.Hidden;
                rampStart07.Visibility = System.Windows.Visibility.Hidden;
                centerText07.Visibility = System.Windows.Visibility.Hidden;
                rampEnd07.Visibility = System.Windows.Visibility.Hidden;
                quintile07.Visibility = System.Windows.Visibility.Hidden;
                legendText07.Visibility = System.Windows.Visibility.Hidden;
            }
            if (isNewColorRamp) rctColor07.Fill = new SolidColorBrush(coo);

            if (radShapeFile.IsChecked == true && _provider != null)
            {
                rampStart07.Text = _provider.RangeValues[i - 2, 0];
                rampEnd07.Text = _provider.RangeValues[i - 2, 1];
                quintile07.Text = _provider.QuantileValues[i - 2].ToString();
            }
            else if (radMapServer.IsChecked == true && choroMapprovider != null)
            {
                rampStart07.Text = choroMapprovider.RangeValues[i - 2, 0];
                rampEnd07.Text = choroMapprovider.RangeValues[i - 2, 1];
                quintile07.Text = choroMapprovider.QuantileValues[i - 2].ToString();
            }
            else if (radKML.IsChecked == true && choroKMLprovider != null)
            {
                rampStart07.Text = choroKMLprovider.RangeValues[i - 2, 0];
                rampEnd07.Text = choroKMLprovider.RangeValues[i - 2, 1];
                quintile07.Text = choroKMLprovider.QuantileValues[i - 2].ToString();
            }

            coo = Color.FromArgb(Opacity, (byte)(rampStart.Color.R - ri * 7), (byte)(rampStart.Color.G - gi * 7), (byte)(rampStart.Color.B - bi * 7));
            rctColor08.Visibility = System.Windows.Visibility.Visible;
            rampStart08.Visibility = System.Windows.Visibility.Visible;
            centerText08.Visibility = System.Windows.Visibility.Visible;
            rampEnd08.Visibility = System.Windows.Visibility.Visible;
            quintile08.Visibility = System.Windows.Visibility.Visible;
            legendText08.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(Opacity, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor08.Visibility = System.Windows.Visibility.Hidden;
                rampStart08.Visibility = System.Windows.Visibility.Hidden;
                centerText08.Visibility = System.Windows.Visibility.Hidden;
                rampEnd08.Visibility = System.Windows.Visibility.Hidden;
                quintile08.Visibility = System.Windows.Visibility.Hidden;
                legendText08.Visibility = System.Windows.Visibility.Hidden;
            }
            if (isNewColorRamp) rctColor08.Fill = new SolidColorBrush(coo);

            if (radShapeFile.IsChecked == true && _provider != null)
            {
                rampStart08.Text = _provider.RangeValues[i - 2, 0];
                rampEnd08.Text = _provider.RangeValues[i - 2, 1];
                quintile08.Text = _provider.QuantileValues[i - 2].ToString();
            }
            else if (radMapServer.IsChecked == true && choroMapprovider != null)
            {
                rampStart08.Text = choroMapprovider.RangeValues[i - 2, 0];
                rampEnd08.Text = choroMapprovider.RangeValues[i - 2, 1];
                quintile08.Text = choroMapprovider.QuantileValues[i - 2].ToString();
            }
            else if (radKML.IsChecked == true && choroKMLprovider != null)
            {
                rampStart08.Text = choroKMLprovider.RangeValues[i - 2, 0];
                rampEnd08.Text = choroKMLprovider.RangeValues[i - 2, 1];
                quintile08.Text = choroKMLprovider.QuantileValues[i - 2].ToString();
            }

            coo = Color.FromArgb(Opacity, (byte)(rampStart.Color.R - ri * 8), (byte)(rampStart.Color.G - gi * 8), (byte)(rampStart.Color.B - bi * 8));
            rctColor09.Visibility = System.Windows.Visibility.Visible;
            rampStart09.Visibility = System.Windows.Visibility.Visible;
            centerText09.Visibility = System.Windows.Visibility.Visible;
            rampEnd09.Visibility = System.Windows.Visibility.Visible;
            quintile09.Visibility = System.Windows.Visibility.Visible;
            legendText09.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(Opacity, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor09.Visibility = System.Windows.Visibility.Hidden;
                rampStart09.Visibility = System.Windows.Visibility.Hidden;
                centerText09.Visibility = System.Windows.Visibility.Hidden;
                rampEnd09.Visibility = System.Windows.Visibility.Hidden;
                quintile09.Visibility = System.Windows.Visibility.Hidden;
                legendText09.Visibility = System.Windows.Visibility.Hidden;
            }
            if (isNewColorRamp) rctColor09.Fill = new SolidColorBrush(coo);

            if (radShapeFile.IsChecked == true && _provider != null)
            {
                rampStart09.Text = _provider.RangeValues[i - 2, 0];
                rampEnd09.Text = _provider.RangeValues[i - 2, 1];
                quintile09.Text = _provider.QuantileValues[i - 2].ToString();
            }
            else if (radMapServer.IsChecked == true && choroMapprovider != null)
            {
                rampStart09.Text = choroMapprovider.RangeValues[i - 2, 0];
                rampEnd09.Text = choroMapprovider.RangeValues[i - 2, 1];
                quintile09.Text = choroMapprovider.QuantileValues[i - 2].ToString();
            }
            else if (radKML.IsChecked == true && choroKMLprovider != null)
            {
                rampStart09.Text = choroKMLprovider.RangeValues[i - 2, 0];
                rampEnd09.Text = choroKMLprovider.RangeValues[i - 2, 1];
                quintile09.Text = choroKMLprovider.QuantileValues[i - 2].ToString();
            }

            coo = Color.FromArgb(Opacity, (byte)(rampStart.Color.R - ri * 6), (byte)(rampStart.Color.G - gi * 9), (byte)(rampStart.Color.B - bi * 9));
            rctColor10.Visibility = System.Windows.Visibility.Visible;
            rampStart10.Visibility = System.Windows.Visibility.Visible;
            centerText10.Visibility = System.Windows.Visibility.Visible;
            rampEnd10.Visibility = System.Windows.Visibility.Visible;
            quintile10.Visibility = System.Windows.Visibility.Visible;
            legendText10.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(Opacity, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor10.Visibility = System.Windows.Visibility.Hidden;
                rampStart10.Visibility = System.Windows.Visibility.Hidden;
                centerText10.Visibility = System.Windows.Visibility.Hidden;
                rampEnd10.Visibility = System.Windows.Visibility.Hidden;
                quintile10.Visibility = System.Windows.Visibility.Hidden;
                legendText10.Visibility = System.Windows.Visibility.Hidden;
            }
            if (isNewColorRamp) rctColor10.Fill = new SolidColorBrush(coo);

            if (radShapeFile.IsChecked == true && _provider != null)
            {
                rampStart10.Text = _provider.RangeValues[i - 2, 0];
                rampEnd10.Text = _provider.RangeValues[i - 2, 1];
                quintile10.Text = _provider.QuantileValues[i - 2].ToString();
            }
            else if (radMapServer.IsChecked == true && choroMapprovider != null)
            {
                 rampStart10.Text = choroMapprovider.RangeValues[i - 2, 0];
                rampEnd10.Text = choroMapprovider.RangeValues[i - 2, 1];
                quintile10.Text = choroMapprovider.QuantileValues[i - 2].ToString();
            }
            else if (radKML.IsChecked == true && choroKMLprovider != null)
            {
                rampStart10.Text = choroKMLprovider.RangeValues[i - 2, 0];
                rampEnd10.Text = choroKMLprovider.RangeValues[i - 2, 1];
                quintile10.Text = choroKMLprovider.QuantileValues[i - 2].ToString();
            }
            
            _initialRampCalc = false;
        }

        private void CheckBox_Quantiles_Click(object sender, RoutedEventArgs e)
        {
            OnQuintileOptionChanged();
        }

        public void OnQuintileOptionChanged()
        {
            int widthQuintile = 100;
            int widthMinMax = 0;
            int widthCompare = 0;

            if (quintilesOption.IsChecked == false)
            {
                widthQuintile = 0;
                widthMinMax = 75;
                widthCompare = 50;
            }

            quintileColumn.Width = new GridLength(widthQuintile, GridUnitType.Pixel);

            rampStartColumn.Width = new GridLength(widthMinMax, GridUnitType.Pixel);
            rampCompareColumn.Width = new GridLength(widthCompare, GridUnitType.Pixel);
            rampEndColumn.Width = new GridLength(widthMinMax, GridUnitType.Pixel);
        }
        public void cmbShapeKey_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbShapeKey.SelectedItem != null)
                { _shapeKey = cmbShapeKey.SelectedItem.ToString(); }
            else
            { _shapeKey = ""; }
        }

        private void cmbDataKey_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDataKey.SelectedItem != null)
            { _dataKey = cmbDataKey.SelectedItem.ToString(); }
        }

        private void cmbValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbValue.SelectedItem != null)
             { _value = cmbValue.SelectedItem.ToString(); }
        }  

        private void SetRangeUISection()
        {
            SetOpacity();
            List<SolidColorBrush> brushList = new List<SolidColorBrush>() { 
                    (SolidColorBrush)rctColor01.Fill, 
                    (SolidColorBrush)rctColor02.Fill, 
                    (SolidColorBrush)rctColor03.Fill, 
                    (SolidColorBrush)rctColor04.Fill, 
                    (SolidColorBrush)rctColor05.Fill, 
                    (SolidColorBrush)rctColor06.Fill, 
                    (SolidColorBrush)rctColor07.Fill, 
                    (SolidColorBrush)rctColor08.Fill, 
                    (SolidColorBrush)rctColor09.Fill, 
                    (SolidColorBrush)rctColor10.Fill,  
                    (SolidColorBrush)rctColor00.Fill };

            int classCount;
            if (!int.TryParse(((ComboBoxItem)cmbClasses.SelectedItem).Content.ToString(), out classCount))
            {
                classCount = 4;
            }
          
            if (radShapeFile.IsChecked == true && _provider != null)
            {
                //_provider.PopulateRangeValues(_dashboardHelper,
                //        cmbShapeKey.SelectedItem.ToString(),
                //        cmbDataKey.SelectedItem.ToString(),
                //        cmbValue.SelectedItem.ToString(),
                //        brushList,
                //        classCount);
                _provider.PopulateRangeValues();
                _provider.AreRangesSet = true;
            }
            else if (radMapServer.IsChecked == true && choroMapprovider != null)
            {
                choroMapprovider.Range = GetRangeValuesFromAttributeList(choroserverlayerprop.classAttribList, classCount);
                choroMapprovider.PopulateRangeValues(_dashboardHelper,
                         cmbShapeKey.SelectedItem.ToString(),
                         cmbDataKey.SelectedItem.ToString(),
                         cmbValue.SelectedItem.ToString(),
                         brushList,
                         classCount);
                choroMapprovider.AreRangesSet = true;
            }
            else if (radKML.IsChecked == true && choroKMLprovider != null)
            {
                choroKMLprovider.PopulateRangeValues();
                choroKMLprovider.AreRangesSet = true;
            }

                SolidColorBrush rampStart = (SolidColorBrush)rctLowColor.Fill;
                SolidColorBrush rampEnd = (SolidColorBrush)rctHighColor.Fill;
                SetVisibility(classCount, rampStart, rampEnd);
                
            }

        private List<double> GetRangeValuesFromAttributeList(Dictionary<int, object> dictionary, int classCount)
        {
            List<double> RangeAttr = new List<double>();
            double value = 0.0;

            if (dictionary == null)
                return RangeAttr; 

            if (classCount >= 2 && dictionary.Count > 1)
            {
                classAttributes class1 = (classAttributes)dictionary[1];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value); 
                
                class1 = (classAttributes)dictionary[2];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }
                
            if (classCount >= 3 && dictionary.Count > 2)
            {
                classAttributes class1 = (classAttributes)dictionary[3];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }
            if (classCount >= 4 && dictionary.Count > 3)
            {
                classAttributes class1 = (classAttributes)dictionary[4];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }
            if (classCount >= 5 && dictionary.Count > 4)
            {
                classAttributes class1 = (classAttributes)dictionary[5];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }
            if (classCount >= 6 && dictionary.Count > 5)
            {
                classAttributes class1 = (classAttributes)dictionary[6];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }
            if (classCount >= 7 && dictionary.Count > 6)
            {
                classAttributes class1 = (classAttributes)dictionary[7];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }
            if (classCount >= 8 && dictionary.Count > 7)
            {
                classAttributes class1 = (classAttributes)dictionary[8];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }
            if (classCount >= 9 && dictionary.Count > 8)
            {
                classAttributes class1 = (classAttributes)dictionary[9];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }
            
            return RangeAttr;

        }
        private void btnKMLFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "KML Files (*.kml)|*.kml|KMZ Files (*.kmz)|*.kmz";
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtKMLpath.Text = dialog.FileName;
                KMLMapServerName = txtKMLpath.Text;
            }

            if (choroKMLprovider == null)
            {
                choroKMLprovider = new Mapping.ChoroplethKmlLayerProvider(_myMap);
                choroKMLprovider.FeatureLoaded += new FeatureLoadedHandler(choroKMLprovider_FeatureLoaded);
            }

            object[] kmlFileProperties = choroKMLprovider.LoadKml(KMLMapServerName);
            if (kmlFileProperties != null)
            {
                if (chorokmllayerprop == null)
                {
                    ILayerProperties layerProperties = null;
                    layerProperties = new ChoroplethKmlLayerProperties(_myMap, this.DashboardHelper, this._mapControl);
                    layerProperties.MapGenerated += new EventHandler(this._mapControl.ILayerProperties_MapGenerated);
                    layerProperties.FilterRequested += new EventHandler(this._mapControl.ILayerProperties_FilterRequested);
                    layerProperties.EditRequested += new EventHandler(this._mapControl.ILayerProperties_EditRequested);
                    this.chorokmllayerprop = (ChoroplethKmlLayerProperties)layerProperties;
                    this._mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                }
                chorokmllayerprop.shapeFilePath = KMLMapServerName;
                chorokmllayerprop.provider = choroKMLprovider;
                chorokmllayerprop.provider.FeatureLoaded += new FeatureLoadedHandler(chorokmllayerprop.provider_FeatureLoaded);
                if (this.DashboardHelper != null)
                    chorokmllayerprop.SetdashboardHelper(DashboardHelper);

            }
        }
                
        public void choroMapprovider_FeatureLoaded(string serverName, IDictionary<string, object> featureAttributes)
        {
            if (!string.IsNullOrEmpty(serverName))
            {
                shapeFilePath = serverName;
                if (featureAttributes != null)
                {
                    cmbShapeKey.Items.Clear();
                    cmbShapeKey.SelectedIndex = -1;
                    choroserverlayerprop.cbxShapeKey.Items.Clear();
                    foreach (string key in featureAttributes.Keys)
                    {
                        cmbShapeKey.Items.Add(key);
                        choroserverlayerprop.cbxShapeKey.Items.Add(key);
                    }
                 }
             }
            if (currentElement != null)
            {
                foreach (System.Xml.XmlElement child in currentElement.ChildNodes)
                {
                    if (child.Name.Equals("dataKey"))
                    {
                        cmbDataKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("shapeKey"))
                    {
                        cmbShapeKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("value"))
                    {
                        cmbValue.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("dotValue"))
                    {
                       // txtDotValue.Text = child.InnerText;
                    }
                    if (child.Name.Equals("dotColor"))
                    {
                        //rctDotColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                    }
                }
              //  RenderMap();
            }
        }

        private void Addfilters()
        {
            this.datafilters = rowFilterControl.DataFilters;
            if (radShapeFile.IsChecked == true && this.layerprop != null) { layerprop.datafilters = rowFilterControl.DataFilters; }
            if (radMapServer.IsChecked == true && this.choroserverlayerprop != null) { choroserverlayerprop.datafilters = rowFilterControl.DataFilters; }
            if (radKML.IsChecked == true && chorokmllayerprop != null) { chorokmllayerprop.datafilters = rowFilterControl.DataFilters; }
            
            
            this.datafilters = rowFilterControl.DataFilters;
            _dashboardHelper.SetDatafilters(this.datafilters);

            /*
            string sfilterOperand = string.Empty;
            string[] shilowvars;
            string svarname;

            List<string> sconditionval = datafilters.GetFilterConditionsAsList();
           
            string strreadablecondition = datafilters.GenerateReadableDataFilterString().Trim();
            if (!(string.IsNullOrEmpty(strreadablecondition)))
            {
                if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_GREATER_THAN))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_GREATER_THAN;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_GREATER_THAN_OR_EQUAL))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_GREATER_THAN_OR_EQUAL;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_LESS_THAN))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_LESS_THAN;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_LESS_THAN_OR_EQUAL))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_LESS_THAN_OR_EQUAL;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_MISSING))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_MISSING;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_NOT_EQUAL_TO))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_NOT_EQUAL_TO;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_OR))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_OR;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_LIKE))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_LIKE;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_AND))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_AND;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_IS_ANY_OF))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_IS_ANY_OF;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_IS_NOT_ANY_OF))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_IS_NOT_ANY_OF;
                }
                if (!(strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_BETWEEN)))
                {
                    svarname = strreadablecondition.Substring(strreadablecondition.IndexOf("[") + 1, strreadablecondition.IndexOf("]") - strreadablecondition.IndexOf("[") - 1);
                    _dashboardHelper.AddDataFilterCondition(sfilterOperand, sconditionval[0].ToString(), svarname, ConditionJoinType.And);
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_BETWEEN))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_BETWEEN;
                    string strcondition = strreadablecondition.Substring(0, strreadablecondition.IndexOf(sfilterOperand)).Trim();
                    string[] strVarstrings = strcondition.Split(' ');
                    svarname = strVarstrings[3].ToString();
                    string sValues = strreadablecondition.ToString().Substring(strreadablecondition.IndexOf(sfilterOperand) + sfilterOperand.Length, (strreadablecondition.ToString().Length) - (strreadablecondition.ToString().IndexOf(sfilterOperand) + sfilterOperand.Length)).Trim();
                    shilowvars = sValues.Split(' ');
                    _dashboardHelper.AddDataFilterCondition(sfilterOperand, shilowvars[0].ToString(), shilowvars[2].ToString(), svarname, ConditionJoinType.And);
                }
            } */

        }

        void choroKMLprovider_FeatureLoaded(string serverName, IDictionary<string, object> featureAttributes)
        {
            if (!string.IsNullOrEmpty(serverName))
            {
                shapeFilePath = serverName;
                if (featureAttributes != null)
                {
                    cmbShapeKey.Items.Clear();
                    chorokmllayerprop.cbxShapeKey.Items.Clear();
                    foreach (string key in featureAttributes.Keys)
                    {
                        cmbShapeKey.Items.Add(key);
                        chorokmllayerprop.cbxShapeKey.Items.Add(key);
                    }
                }
            }
            if (currentElement != null)
            {
                foreach (System.Xml.XmlElement child in currentElement.ChildNodes)
                {
                    if (child.Name.Equals("dataKey"))
                    {
                        cmbDataKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("shapeKey"))
                    {
                        cmbShapeKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("value"))
                    {
                        cmbValue.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("dotValue"))
                    {
                        //txtDotValue.Text = child.InnerText;
                    }
                    if (child.Name.Equals("dotColor"))
                    {
                        //rctDotColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                    }
                }
               // RenderMap();
            }
        }

      
        private void radShapeFile_Checked(object sender, RoutedEventArgs e)
        {
           //check for edit mode
           //if (ClassAttribList.Count != 0) { return; }
           if (!string.IsNullOrEmpty(txtKMLpath.Text))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("You are about to change to a different boundary format. Are you sure you want to clear the current boundary settings?", "Alert", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radShapeFile.IsChecked = false;
                    radKML.IsChecked = true;
                    return;
                }
                else
                    ClearonShapeFile();
            }
            else if (!string.IsNullOrEmpty(txtMapSeverpath.Text) || cbxmapserver.SelectedIndex != -1)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("You are about to change to a different boundary format. Are you sure you want to clear the current boundary settings?", "Alert", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radShapeFile.IsChecked = false;
                    radMapServer.IsChecked = true;
                    return;
                }
                else
                    ClearonShapeFile();
            }
            else if (!string.IsNullOrEmpty(txtShapePath.Text))
            {
                return;
            }
            else
                ClearonShapeFile();                  
        }
        
        private void ClearonKML()
        {
            panelshape.IsEnabled = false;
            panelmap.IsEnabled = false;
            panelKml.IsEnabled = true;
            cmbShapeKey.Items.Clear();
            txtShapePath.Text = string.Empty;
            txtKMLpath.Text = string.Empty;
            cbxmapfeature.SelectedIndex = -1;
            cbxmapserver.SelectedIndex = -1;
            radlocatemapserver.IsChecked = false;
            radconnectmapserver.IsChecked = false;
            txtMapSeverpath.Text = string.Empty;
            if (choroMapprovider != null)
            {
                choroMapprovider.FeatureLoaded -= new FeatureLoadedHandler(choroMapprovider_FeatureLoaded);
                choroMapprovider = null;
            }
            if (_provider != null)
            {
                layerprop.CloseLayer(); 
                _provider = null;
            }
        }

        public void ClearonShapeFile()
        {
            panelshape.IsEnabled = true;
            panelmap.IsEnabled = false;
            panelKml.IsEnabled = false;
            cmbShapeKey.Items.Clear();
            txtShapePath.Text = string.Empty;
            txtKMLpath.Text = string.Empty;
            cbxmapfeature.SelectedIndex = -1;
            cbxmapserver.SelectedIndex = -1;
            radlocatemapserver.IsChecked = false;
            radconnectmapserver.IsChecked = false;
            txtMapSeverpath.Text = string.Empty;
            if (choroMapprovider != null)
            {
                choroMapprovider.FeatureLoaded -= new FeatureLoadedHandler(choroMapprovider_FeatureLoaded);
                choroMapprovider = null;
            }
            if (choroKMLprovider != null)
            {
                choroKMLprovider.FeatureLoaded -= new FeatureLoadedHandler(choroKMLprovider_FeatureLoaded);
                choroKMLprovider = null;
            }
        }

        public void ClearonMapServer()
        {
            panelshape.IsEnabled = false;
            panelmap.IsEnabled = true;
            panelKml.IsEnabled = false;
            cmbShapeKey.Items.Clear();
            txtShapePath.Text = string.Empty;
            txtKMLpath.Text = string.Empty;
            cbxmapfeature.SelectedIndex = -1;
            cbxmapserver.SelectedIndex = -1;
            txtMapSeverpath.Text = string.Empty;
            if (choroKMLprovider != null)
            {
                choroKMLprovider.FeatureLoaded -= new FeatureLoadedHandler(choroKMLprovider_FeatureLoaded);
                choroKMLprovider = null;
            }
            if (_provider != null)
            {
                 layerprop.CloseLayer(); 
                _provider = null;
            }
        }

      
        private void radKML_Checked(object sender, RoutedEventArgs e)
        {
           
            if (!string.IsNullOrEmpty(txtShapePath.Text))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("You are about to change to a different boundary format. Are you sure you want to clear the current boundary settings?", "Alert", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radKML.IsChecked = false;
                    radShapeFile.IsChecked = true;
                    return;
                }
                else
                    ClearonKML();
            }
            else if (!string.IsNullOrEmpty(txtMapSeverpath.Text) || cbxmapserver.SelectedIndex != -1)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("You are about to change to a different boundary format. Are you sure you want to clear the current boundary settings?", "Alert", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radKML.IsChecked = false;
                    radMapServer.IsChecked = true;
                    return;
                }
                else
                    ClearonKML();
            }
            else if (!string.IsNullOrEmpty(txtKMLpath.Text))
            {
                return;
            }
            else
                ClearonKML();            
        }

        private void radMapServer_Checked(object sender, RoutedEventArgs e)
        {
           
            if (!string.IsNullOrEmpty(txtShapePath.Text))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("You are about to change to a different boundary format. Are you sure you want to clear the current boundary settings?", "Alert", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radMapServer.IsChecked = false;
                    radShapeFile.IsChecked = true;
                    return;
                }
                else
                    ClearonMapServer();
            }
            else if (!string.IsNullOrEmpty(txtKMLpath.Text))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("You are about to change to a different boundary format. Are you sure you want to clear the current boundary settings?", "Alert", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radMapServer.IsChecked = false;
                    radKML.IsChecked = true;
                    return;
                }
                else
                    ClearonMapServer();
            }
            else if (!string.IsNullOrEmpty(txtMapSeverpath.Text) || cbxmapserver.SelectedIndex != -1)
            {
                return;
            }
            else
                ClearonMapServer();      
        }
           
        private void radconnectmapserver_Checked(object sender, RoutedEventArgs e)
        {
            panelmapconnect.IsEnabled = true;
            panelmapserver.IsEnabled = false;
            panelmapconnect.IsEnabled = true;
            txtMapSeverpath.Text = string.Empty;
        }

        public void cbxmapserver_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetMapServer();
        }

        public void ResetMapServer()
        {
            if (cbxmapserver.SelectedIndex > -1)
            {
                MapServerName = ((ComboBoxItem)cbxmapserver.SelectedItem).Content.ToString();
              
                if (choroMapprovider == null)
                {
                    choroMapprovider = new Mapping.ChoroplethServerLayerProvider(_myMap);
                    choroMapprovider.FeatureLoaded += new FeatureLoadedHandler(choroMapprovider_FeatureLoaded);
                }
                object[] mapFileProperties = choroMapprovider.LoadShapeFile(MapServerName + "/" + MapVisibleLayer);
                if (mapFileProperties != null)
                {
                    if (this.choroserverlayerprop == null)
                    {
                        ILayerProperties layerProperties = null;
                        layerProperties = new ChoroplethServerLayerProperties(_myMap, this.DashboardHelper, this._mapControl);
                        layerProperties.MapGenerated += new EventHandler(this._mapControl.ILayerProperties_MapGenerated);
                        layerProperties.FilterRequested += new EventHandler(this._mapControl.ILayerProperties_FilterRequested);
                        layerProperties.EditRequested += new EventHandler(this._mapControl.ILayerProperties_EditRequested);
                        this.choroserverlayerprop = (ChoroplethServerLayerProperties)layerProperties;
                        this._mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                    }
                    choroserverlayerprop.shapeFilePath = MapServerName;
                    choroserverlayerprop.provider = choroMapprovider;
                    choroserverlayerprop.provider.FeatureLoaded += new FeatureLoadedHandler(choroserverlayerprop.provider_FeatureLoaded);
                    if (this.DashboardHelper != null)
                        choroserverlayerprop.SetdashboardHelper(DashboardHelper);
                   
                }
            }
        }

        private void radlocatemapserver_Checked(object sender, RoutedEventArgs e)
        {
            panelmapconnect.IsEnabled = false;
            panelmapserver.IsEnabled = true;
            panelmapconnect.IsEnabled = false;
            cbxmapserver.SelectedIndex = -1;
        }

        private void ResetShapeCombo()
        {
            //In case of map server
            if (radMapServer.IsChecked == true)
            {
                cmbShapeKey.Items.Clear();
                cmbShapeKey.SelectedIndex = -1;
                if (choroserverlayerprop != null)
                {
                    choroserverlayerprop.cbxShapeKey.Items.Clear();
                    choroserverlayerprop.cbxShapeKey.SelectedIndex = -1;
                 }
                /*
                //set back choroMapprovider
                if (choroMapprovider == null)
                {
                    choroMapprovider = new Mapping.ChoroplethServerLayerProvider(_myMap);
                    choroMapprovider.FeatureLoaded += new FeatureLoadedHandler(choroMapprovider_FeatureLoaded);
                    choroserverlayerprop.provider.FeatureLoaded -= new FeatureLoadedHandler(choroserverlayerprop.provider_FeatureLoaded);
                 } */
            }
        }

        private void btnMapserverlocate_Click(object sender, RoutedEventArgs e)
        {
            MapServerConnect();
        }

        public void MapServerConnect()
        {
            try
            {
                string message = GetMessage(txtMapSeverpath.Text + "?f=json");
                System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();
                Rest rest = ser.Deserialize<Rest>(message);
                cbxmapfeature.ItemsSource = rest.layers;
                if (rest.layers.Count > 0)
                    cbxmapfeature.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                cbxmapfeature.DataContext = null;
                System.Windows.Forms.MessageBox.Show("Invalid map server");
            }
        }

        private void txtMapSeverpath_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnMapserverlocate.IsEnabled = txtMapSeverpath.Text.Length > 0;
        }

        public void cbxmapfeature_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
             MapfeatureSelectionChange();
        }

        public void MapfeatureSelectionChange()
        {
            if (cbxmapfeature.Items.Count > 0)
            {
                MapServerName = txtMapSeverpath.Text;
                if (cbxmapfeature.SelectedIndex > -1)
                {
                    int visibleLayer = ((SubObject)cbxmapfeature.SelectedItem).id;
                    MapVisibleLayer = visibleLayer;
                    if (choroMapprovider == null)
                    {
                        choroMapprovider = new Mapping.ChoroplethServerLayerProvider(_myMap);
                        choroMapprovider.FeatureLoaded += new FeatureLoadedHandler(choroMapprovider_FeatureLoaded);
                    }
                    object[] mapFileProperties = choroMapprovider.LoadShapeFile(MapServerName + "/" + MapVisibleLayer);                           
                    if (mapFileProperties != null)
                    {
                        if (choroserverlayerprop == null)
                        {
                            ILayerProperties layerProperties = null;
                            layerProperties = new ChoroplethServerLayerProperties(_myMap, this.DashboardHelper, this._mapControl);
                            layerProperties.MapGenerated += new EventHandler(this._mapControl.ILayerProperties_MapGenerated);
                            layerProperties.FilterRequested += new EventHandler(this._mapControl.ILayerProperties_FilterRequested);
                            layerProperties.EditRequested += new EventHandler(this._mapControl.ILayerProperties_EditRequested);
                            this.choroserverlayerprop = (ChoroplethServerLayerProperties)layerProperties;
                            this._mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                        }
                        choroserverlayerprop.shapeFilePath = MapServerName;
                        choroserverlayerprop.provider = choroMapprovider;
                        choroserverlayerprop.provider.FeatureLoaded += new FeatureLoadedHandler(choroserverlayerprop.provider_FeatureLoaded);
                        if (this.DashboardHelper != null)
                            choroserverlayerprop.SetdashboardHelper(DashboardHelper);
                       
                    }
                }
                else
                {
                    MapVisibleLayer = -1;
                }
            }
        }

        private void Slider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender == null || txtOpacity == null)
            {
                return;
            }
            double value = ((Slider)sender).Value;
            double calculatedPercentage = (value - 100) / (255 - 100) ;
            txtOpacity.Text = string.Format("{0:N2}", calculatedPercentage * 100)  + "%";

            brush.Opacity = calculatedPercentage;

        }
              

    }
    
}
        

