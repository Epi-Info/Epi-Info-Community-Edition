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

using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;

using EpiDashboard.Mapping;

namespace EpiDashboard.Controls
{
    /// <summary>
    /// Interaction logic for PointofInterestProperties.xaml
    /// </summary>
   

     public partial class PointofInterestProperties : UserControl, ILayerProperties
     {
            
        private MapView _mapView;
        private DashboardHelper dashboardHelper;
        public event EventHandler MapGenerated;
        public event EventHandler FilterRequested;
        public event EventHandler EditRequested;
        private EpiDashboard.Mapping.PointLayerProvider provider;
        private EpiDashboard.Mapping.StandaloneMapControl mapControl;
        private IMapControl imapcontrol;
        private PointLayerProperties layerprop;
        private Brush colorselected;
        private RowFilterControl rowfiltercontrol;
        public DataFilters datafilters;


        public PointofInterestProperties(EpiDashboard.Mapping.StandaloneMapControl mapControl, MapView mapView, PointLayerProperties pointlayerprop)
        {
            InitializeComponent();

            this._mapView = mapView;
            this.mapControl = mapControl;
          
            mapControl.TimeVariableSet += new TimeVariableSetHandler(mapControl_TimeVariableSet);
            mapControl.MapDataChanged += new EventHandler(mapControl_MapDataChanged);
            layerprop = pointlayerprop;
            provider = pointlayerprop.provider;
            rctSelectColor.Fill = new SolidColorBrush(Color.FromArgb(120,0,0,255));
            colorselected = new SolidColorBrush(Color.FromArgb(120, 0, 0, 255));
            mapControl.SizeChanged += mapControl_SizeChanged;
            #region Translation

            //Point of Interest Left Panel
            lblConfigExpandedTitle.Content = DashboardSharedStrings.GADGET_CONFIG_TITLE_SPOTMAP;
            tbtnDataSource.Title = DashboardSharedStrings.GADGET_DATA_SOURCE;
            tbtnDataSource.Description = DashboardSharedStrings.GADGET_TABDESC_DATASOURCE;
            tbtnVariables.Title = DashboardSharedStrings.GADGET_TABBUTTON_VARIABLES;
            tbtnVariables.Description = DashboardSharedStrings.GADGET_TABDESC_COORD_VARIABLES;
            tbtnDisplay.Title = DashboardSharedStrings.GADGET_TABBUTTON_DISPLAY;
            tbtnDisplay.Description = DashboardSharedStrings.GADGET_TABDESC_DISPLAY;
            //tbtnCharts.Title = DashboardSharedStrings.GADGET_TAB_COLORS_STYLES;
            //tbtnCharts.Description = DashboardSharedStrings.GADGET_TABDESC_SETCOLORS;
            tbtnFilter.Title = DashboardSharedStrings.GADGET_TABBUTTON_FILTERS;
            tbtnFilter.Description = DashboardSharedStrings.GADGET_TABDESC_FILTERS_MAPS;

            //Data Source Panel
            tblockPanelDataSource.Content = DashboardSharedStrings.GADGET_DATA_SOURCE;
            tblockDataSource.Content = DashboardSharedStrings.GADGET_DATA_SOURCE;
            btnBrowse.Content = DashboardSharedStrings.BUTTON_BROWSE;
            tblockConnectionString.Content = DashboardSharedStrings.GADGET_CONNECTION_STRING;
            tblockSQLQuery.Content = DashboardSharedStrings.GADGET_SQL_QUERY;
            tblockLoadingData.Text = DashboardSharedStrings.GADGET_LOADING_DATA;
            //Variables Panel
            tblockPanelVariables.Content = DashboardSharedStrings.GADGET_TABBUTTON_VARIABLES;
            tblockSelectVarData.Content = DashboardSharedStrings.GADGET_POF_COORD_VARIABLES;
            lblLatitude.Content = DashboardSharedStrings.GADGET_LATITUDE_FIELD;
            lblLongitude.Content = DashboardSharedStrings.GADGET_LONGITUDE_FIELD;

            //Display Panel
            tblockPanelDisplay.Content = DashboardSharedStrings.GADGET_TABBUTTON_DISPLAY;
            tblockTitleNDescSubheader.Content = DashboardSharedStrings.GADGET_LABEL_DESCRIPTION;
            tblockMapDesc.Content = DashboardSharedStrings.GADGET_LEGEND_DESCRIPTION;

            //Colors and Styles Panel
            lblPanelHdrColorsAndStyles.Content = DashboardSharedStrings.GADGET_TAB_COLORS_STYLES;
            lblColor.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_COLOR;
            tblcolor.Text = DashboardSharedStrings.GADGET_SELECT_COLOR;
            lblStyle.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_STYLES;
            lblShape.Content = DashboardSharedStrings.GADGET_SELECT_SHAPE;

            //Filters Panel
            tblockPanelDataFilter.Content = DashboardSharedStrings.GADGET_PANELHEADER_DATA_FILTER;
            tblockSetDataFilter.Content = DashboardSharedStrings.GADGET_TABDESC_FILTERS_MAPS;

            btnOK.Content = DashboardSharedStrings.BUTTON_OK;
            btnCancel.Content = DashboardSharedStrings.BUTTON_CANCEL;

            #endregion // Translation
        }
        public void mapControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            mapControl.ResizedWidth = e.NewSize.Width;
            mapControl.ResizedHeight = e.NewSize.Height;
            if (mapControl.ResizedWidth != 0 & mapControl.ResizedHeight != 0)
            {
                double i_StandardHeight = System.Windows.SystemParameters.PrimaryScreenHeight;//Developer Desktop Width Where the Form is Designed
                double i_StandardWidth = System.Windows.SystemParameters.PrimaryScreenWidth; ////Developer Desktop Height Where the Form is Designed
                float f_HeightRatio = new float();
                float f_WidthRatio = new float();
                f_HeightRatio = (float)((float)mapControl.ResizedHeight / (float)i_StandardHeight);
                f_WidthRatio = (float)((float)mapControl.ResizedWidth / (float)i_StandardWidth);

                this.Height = (Convert.ToInt32(i_StandardHeight * f_HeightRatio)) / 1.16;
                this.Width = (Convert.ToInt32(i_StandardWidth * f_WidthRatio)) / 1.13;

            }
            else
            {
                this.Width = (System.Windows.SystemParameters.PrimaryScreenWidth / 1.2);
                this.Height = (System.Windows.SystemParameters.PrimaryScreenHeight / 1.2);
            }
        }

        public event EventHandler Cancelled;
        public event EventHandler ChangesAccepted;
     
        public Brush ColorSelected
        {
            set
            {
                colorselected = value;
            }
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

        private void tbtnInfo_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Visible;
            panelFilter.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnCharts_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            //panelCharts.Visibility = System.Windows.Visibility.Visible;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilter.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnVariables_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
           
                panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
                panelVariables.Visibility = System.Windows.Visibility.Visible;
                panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
                panelCharts.Visibility = System.Windows.Visibility.Collapsed;
                panelInfo.Visibility = System.Windows.Visibility.Collapsed;
                panelFilter.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnFilter_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelFilter.Visibility = System.Windows.Visibility.Visible;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnDataSource_Checked(object sender, RoutedEventArgs e)
        {
            if (panelDataSource == null) return;
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Visible;
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilter.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnDisplay_Checked(object sender, RoutedEventArgs e)
        {
            if (panelDataSource == null) return;
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Visible;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilter.Visibility = System.Windows.Visibility.Collapsed;
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
            this.PropertyChanged_EnableDisable();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            waitCursor.Visibility = Visibility.Visible;
            tblockLoadingData.Visibility = Visibility.Visible;
          dashboardHelper = mapControl.GetNewDashboardHelper();
         // this.DashboardHelper = dashboardHelper;
          layerprop.SetdashboardHelper(dashboardHelper);
          waitCursor.Visibility = Visibility.Collapsed;
          tblockLoadingData.Visibility = Visibility.Collapsed;
          if (dashboardHelper != null)
            {
                txtProjectPath.Text = mapControl.ProjectFilepath;
                FillComboBoxes();
                SetFilter();
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
                lblLatitude.Foreground = brush;
                lblLongitude.Foreground = brush;
            }
        }

        public void MakeReadOnly()
        {
           
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
            string latitude = cmbLatitude.SelectedItem.ToString();
            string longitude = cmbLongitude.SelectedItem.ToString();
            SolidColorBrush color = (SolidColorBrush)rctSelectColor.Fill;
            string style = cmbStyle.SelectedItem.ToString();
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
                    cmbLatitude.SelectedItem = child.InnerText;
                }
                if (child.Name.Equals("longitude"))
                {
                    cmbLongitude.SelectedItem = child.InnerText;
                }
                if (child.Name.Equals("color"))
                {
                    rctSelectColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                }
                if (child.Name.Equals("style"))
                {
                    cmbStyle.SelectedItem = child.InnerText;
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
            
        }

        #endregion

       
        public void MoveUp()
        {
            provider.MoveUp();
        }

        public void MoveDown()
        {
            provider.MoveDown();
        }
        

        void coord_SelectionChanged(object sender, EventArgs e)
        {
            //RenderMap();
        }
        
        public DashboardHelper GetDashboardHelper()
        {
            return this.dashboardHelper;
        }

        public void SetDashboardHelper(DashboardHelper dash)
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
        
        public void SetFilter()
        {
            //--for filters
            rowfiltercontrol = new RowFilterControl(dashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, dashboardHelper.DataFilters, true);
            rowfiltercontrol.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            rowfiltercontrol.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            if (HasFilter())
            {
                RemoveFilter();

            }
            panelFilter.Children.Add(rowfiltercontrol);
        }

        private void RemoveFilter()
        {
            int ChildIndex = 0;
            foreach (var child in panelFilter.Children)
            {
                if (child.GetType().FullName.Contains("EpiDashboard.RowFilterControl"))
                {

                    panelFilter.Children.RemoveAt(ChildIndex);
                    break;
                }
                ChildIndex++;
            }
        }

        private bool HasFilter()
        {
            bool HasFilter = false;
            foreach (var child in panelFilter.Children)
            {
                if (child.GetType().FullName.Contains("EpiDashboard.RowFilterControl"))
                {

                    HasFilter = true;
                }

            }
            return HasFilter;
        } 
        public void FillComboBoxes()
        {
            cmbLatitude.Items.Clear();
            cmbLongitude.Items.Clear();
            ColumnDataType columnDataType = ColumnDataType.Numeric;
            List<string> fields = dashboardHelper.GetFieldsAsList(columnDataType); //dashboardHelper.GetNumericFormFields();
            foreach (string f in fields)
            {
                if (!(f.ToUpperInvariant() == "RECSTATUS" || f.ToUpperInvariant() == "FKEY" || f.ToUpperInvariant() == "GLOBALRECORDID" || f.ToUpperInvariant() == "UNIQUEKEY"))
                {
                    cmbLatitude.Items.Add(f);
                    cmbLongitude.Items.Add(f);
                }
            }
            if (cmbLatitude.Items.Count > 0)
            {
                cmbLatitude.SelectedIndex = -1;
                cmbLongitude.SelectedIndex = -1;
          
            }

            cmbStyle.ItemsSource = Enum.GetNames(typeof(SimpleMarkerStyle));
            cmbStyle.SelectedIndex = 0;
        }
        public void ReFillLongitudeComboBoxes()
        {

            cmbLongitude.Items.Clear();
            ColumnDataType columnDataType = ColumnDataType.Numeric;
            List<string> fields = dashboardHelper.GetFieldsAsList(columnDataType); //dashboardHelper.GetNumericFormFields();
            foreach (string f in fields)
            {
                if (!(f.ToUpperInvariant() == "RECSTATUS" || f.ToUpperInvariant() == "FKEY" || f.ToUpperInvariant() == "GLOBALRECORDID" || f.ToUpperInvariant() == "UNIQUEKEY"))
                {

                    cmbLongitude.Items.Add(f);
                }
            }

            cmbStyle.ItemsSource = Enum.GetNames(typeof(SimpleMarkerStyle));
            cmbStyle.SelectedIndex = 0;
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (cmbLongitude.SelectedIndex > -1 && cmbLongitude.SelectedIndex > -1)
            {
                if (txtProjectPath.Text.Length > 0 && provider != null)
                {
                    Addfilters();
                    RenderMap();
                    layerprop.SetValues(txtDescription.Text, cmbLatitude.Text, cmbLongitude.Text, cmbStyle.Text, colorselected);
                }

                if (ChangesAccepted != null)
                {
                    ChangesAccepted(this, new EventArgs());
                }
            }
            else
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(DashboardSharedStrings.GADGET_MAP_ADD_VARIABLES, DashboardSharedStrings.ALERT, MessageBoxButton.OK);
            }
        }
       
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }

             
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }
       private void RenderMap()
        {
            
            if (cmbLatitude.SelectedIndex != -1 && cmbLongitude.SelectedIndex != -1)
            {
                provider.RenderPointMap
                    (
                        dashboardHelper,
                        cmbLatitude.SelectedItem.ToString(),
                        cmbLongitude.SelectedItem.ToString(),
                        colorselected,
                        string.Empty,
                        (SimpleMarkerStyle)Enum.Parse(typeof(SimpleMarkerStyle),  cmbStyle.SelectedItem.ToString()),
                        txtDescription.Text
                        );
                
                if (MapGenerated != null)
                {
                    MapGenerated(layerprop, new EventArgs());
                }
             }
        }    

        private void rctSelectColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctSelectColor.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                colorselected = new SolidColorBrush (Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctAddFilter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetFilter();
        }
       
 
        private void Addfilters()
        {
            string sfilterOperand = string.Empty;
            string[] shilowvars;
            string svarname;

            this.datafilters = rowfiltercontrol.DataFilters;

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

                if (!(strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_BETWEEN)))
                {
                    svarname = strreadablecondition.Substring(strreadablecondition.IndexOf("[") + 1, strreadablecondition.IndexOf("]") - strreadablecondition.IndexOf("[") - 1);
                    dashboardHelper.AddDataFilterCondition(sfilterOperand, sconditionval[0].ToString(), svarname, ConditionJoinType.And);
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_BETWEEN))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_BETWEEN;
                    string strcondition = strreadablecondition.Substring(0, strreadablecondition.IndexOf(sfilterOperand)).Trim();
                    string[] strVarstrings = strcondition.Split(' ');
                    svarname = strVarstrings[3].ToString();
                    string sValues = strreadablecondition.ToString().Substring(strreadablecondition.IndexOf(sfilterOperand) + sfilterOperand.Length, (strreadablecondition.ToString().Length) - (strreadablecondition.ToString().IndexOf(sfilterOperand) + sfilterOperand.Length)).Trim();
                    shilowvars = sValues.Split(' ');
                    dashboardHelper.AddDataFilterCondition(sfilterOperand, shilowvars[0].ToString(), shilowvars[2].ToString(), svarname, ConditionJoinType.And);
                }
            }
             
        }


        private void PropertyChanged_EnableDisable()
        {           
            btnOK.IsEnabled = false;

            if (!string.IsNullOrEmpty(txtProjectPath.Text))
            {               
                if (tbtnVariables.IsEnabled == false)
                {
                    tbtnVariables.IsEnabled = true;
                    tbtnVariables_Checked(this, new RoutedEventArgs());
                }                
                if (cmbLatitude.SelectedIndex != -1 && cmbLongitude.SelectedIndex != -1)
                {                 
                    if (tbtnDisplay.IsEnabled == false)
                    {
                        tbtnDisplay.IsEnabled = true;
                        tbtnDisplay_Checked(this, new RoutedEventArgs());
                    }
                    tbtnFilter.IsEnabled = true;                  
                    tbtnFilter.Visibility = Visibility.Visible;
                    btnOK.IsEnabled = true;
                    return;
                }
            }
        }

        public void cmbLatitude_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbLatitude.SelectedItem != null)
            {
                cmbLongitude.IsEnabled = true;
                PropertyChanged_EnableDisable();
                ReFillLongitudeComboBoxes();
                if (cmbLongitude.Items.Contains(cmbLatitude.SelectedItem))
                {
                    
                    cmbLongitude.Items.Remove(cmbLatitude.SelectedItem);
                    PropertyChanged_EnableDisable();
                }
            }

        }

        public void cmbLongitude_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PropertyChanged_EnableDisable();

        }
  } 
}
