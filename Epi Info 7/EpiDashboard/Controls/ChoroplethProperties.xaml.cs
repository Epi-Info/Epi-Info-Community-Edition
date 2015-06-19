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

namespace EpiDashboard.Controls
{
    /// <summary>
    /// Interaction logic for DashboardProperties.xaml
    /// </summary>
    public partial class ChoroplethProperties : UserControl
    {
        private EpiDashboard.Mapping.StandaloneMapControl _mapControl;
        private ESRI.ArcGIS.Client.Map _myMap;
        private DashboardHelper _dashboardHelper;
        private EpiDashboard.Mapping.ChoroplethLayerProvider _provider;
        private int _currentStratCount;
        private SolidColorBrush _currentColor_rampStart;
        private SolidColorBrush _currentColor_rampEnd;
        private bool _initialRampCalc;

        public ChoroplethLayerProperties layerprop;
        IDictionary<string, object> shapeAttributes;

        string _shapeKey;
        string _dataKey;
        string _value;

        public ChoroplethProperties(EpiDashboard.Mapping.StandaloneMapControl mapControl, ESRI.ArcGIS.Client.Map myMap)
        {
            InitializeComponent();
            _mapControl = mapControl;
            _myMap = myMap;
            _provider = new Mapping.ChoroplethLayerProvider(_myMap);
            _provider.DashboardHelper = _dashboardHelper;

            Epi.ApplicationIdentity appId = new Epi.ApplicationIdentity(typeof(Configuration).Assembly);
            tblockCurrentEpiVersion.Text = "Epi Info " + appId.Version;

            if (int.TryParse(cmbClasses.Text, out _currentStratCount) == false)
            {
                _currentStratCount = 4;
            }

            _currentColor_rampStart = (SolidColorBrush)rctLowColor.Fill;
            _currentColor_rampEnd = (SolidColorBrush)rctHighColor.Fill;
            _initialRampCalc = true;
            
            ResetLegend_Click(new object(), new RoutedEventArgs());
            OnQuintileOptionChanged();
        }

        public event EventHandler Cancelled;
        public event EventHandler ChangesAccepted;

        public DashboardHelper DashboardHelper { get; private set; }
        
        public string DataSource { get; set; }
        
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
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Visible;
        }

        private void tbtnCharts_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Visible;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnHTML_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelHTML.Visibility = System.Windows.Visibility.Visible;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnDataSource_Checked(object sender, RoutedEventArgs e)
        {
            if (panelDataSource == null) return;
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Visible;
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
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

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            RenderMap();
            layerprop.SetValues(txtShapePath.Text, cmbShapeKey.Text, cmbDataKey.Text, cmbValue.Text, cmbClasses.Text, rctHighColor.Fill, rctLowColor.Fill, shapeAttributes);
            if (ChangesAccepted != null)
            {
                ChangesAccepted(this, new EventArgs());
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
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
                _provider = layerprop.provider;
                txtProjectPath.Text = _dashboardHelper.Database.DbName;
                FillComboBoxes();
            }
        }

        private void FillComboBoxes()
        {
            cmbDataKey.Items.Clear();
            cmbValue.Items.Clear();
            List<string> fields = _dashboardHelper.GetFieldsAsList(); // dashboardHelper.GetFormFields();
            ColumnDataType columnDataType = ColumnDataType.Numeric;
            List<string> numericFields = _dashboardHelper.GetFieldsAsList(columnDataType); //dashboardHelper.GetNumericFormFields();
            foreach (string field in fields)
            {
                cmbDataKey.Items.Add(field);
            }
            foreach (string field in numericFields)
            {
                cmbValue.Items.Add(field);
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
            // provider = new Mapping.ChoroplethLayerProvider(myMap);
            object[] shapeFileProperties = _provider.LoadShapeFile();
            if (shapeFileProperties != null)
            {
                if (shapeFileProperties.Length == 2)
                {
                    txtShapePath.Text = shapeFileProperties[0].ToString();
                    layerprop.SetdashboardHelper(_dashboardHelper);
                    shapeAttributes = (IDictionary<string, object>)shapeFileProperties[1];
                    if (shapeAttributes != null)
                    {
                        cmbShapeKey.Items.Clear();
                        foreach (string key in shapeAttributes.Keys)
                        {
                            cmbShapeKey.Items.Add(key);
                        }
                    }
                }
            }
        }

        private void RenderMap()
        {
            if (cmbDataKey.SelectedIndex != -1 && cmbShapeKey.SelectedIndex != -1 && cmbValue.SelectedIndex != -1)
            {
                string shapeKey = cmbShapeKey.SelectedItem.ToString();
                string dataKey = cmbDataKey.SelectedItem.ToString();
                string value = cmbValue.SelectedItem.ToString();
                
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
                    (SolidColorBrush)rctColor10.Fill };

                int classCount;
                if (int.TryParse(cmbClasses.Text, out classCount))
                {
                    classCount = 4;
                }

                _provider.SetShapeRangeValues(_dashboardHelper, 
                    cmbShapeKey.SelectedItem.ToString(), 
                    cmbDataKey.SelectedItem.ToString(), 
                    cmbValue.SelectedItem.ToString(),
                    brushList, 
                    classCount);
            }
        }
        public StackPanel LegendStackPanel
        {
            get
            {
                return _provider.LegendStackPanel;
            }
        }
        private void rctColor1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor01.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor2_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor02.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor3_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor03.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor4_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor04.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor5_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor05.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor6_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor06.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor7_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor07.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor08_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor08.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor09_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor09.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor10_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor10.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }


        private void rctLowColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctLowColor.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctHighColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctHighColor.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
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

            _provider.ResetRangeValues();

            SolidColorBrush rampStart = (SolidColorBrush)rctLowColor.Fill;
            SolidColorBrush rampEnd = (SolidColorBrush)rctHighColor.Fill;

            bool isNewColorRamp = true;

            if (rampStart == _currentColor_rampStart && rampEnd == _currentColor_rampEnd && stratCount == _currentStratCount)
            {
                if (_initialRampCalc == false)
                {
                    isNewColorRamp = false;
                }
            }

            _currentStratCount = stratCount;
            _currentColor_rampStart = rampStart;
            _currentColor_rampEnd = rampEnd;

            int rd = rampStart.Color.R - rampEnd.Color.R;
            int gd = rampStart.Color.G - rampEnd.Color.G;
            int bd = rampStart.Color.B - rampEnd.Color.B;

            byte ri = (byte)(rd / (stratCount - 1));
            byte gi = (byte)(gd / (stratCount - 1));
            byte bi = (byte)(bd / (stratCount - 1));

            if (isNewColorRamp) rctColor01.Fill = rampStart;
            rampStart01.Text = _provider.RangeValues[0, 0];
            rampEnd01.Text = _provider.RangeValues[0, 1];
            quintile01.Text = _provider.QuantileValues[0].ToString();

            Color coo = Color.FromArgb(240, (byte)(rampStart.Color.R - ri), (byte)(rampStart.Color.G - gi), (byte)(rampStart.Color.B - bi));
            if (isNewColorRamp) rctColor02.Fill = new SolidColorBrush(coo);
            rampStart02.Text = _provider.RangeValues[1, 0];
            rampEnd02.Text = _provider.RangeValues[1, 1];
            quintile02.Text = _provider.QuantileValues[1].ToString();

            int i = 3;

            coo = Color.FromArgb(240, (byte)(rampStart.Color.R - ri * 2), (byte)(rampStart.Color.G - gi * 2), (byte)(rampStart.Color.B - bi * 2));
            rctColor03.Visibility = System.Windows.Visibility.Visible;
            rampStart03.Visibility = System.Windows.Visibility.Visible;
            centerText03.Visibility = System.Windows.Visibility.Visible;
            rampEnd03.Visibility = System.Windows.Visibility.Visible;
            quintile03.Visibility = System.Windows.Visibility.Visible;
            legendText03.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(240, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor03.Visibility = System.Windows.Visibility.Hidden;
                rctColor03.Visibility = System.Windows.Visibility.Hidden;
                rampStart03.Visibility = System.Windows.Visibility.Hidden;
                centerText03.Visibility = System.Windows.Visibility.Hidden;
                rampEnd03.Visibility = System.Windows.Visibility.Hidden;
                quintile03.Visibility = System.Windows.Visibility.Hidden;
                legendText03.Visibility = System.Windows.Visibility.Hidden;
            }
            if(isNewColorRamp) rctColor03.Fill = new SolidColorBrush(coo);
            rampStart03.Text = _provider.RangeValues[i - 2, 0];
            rampEnd03.Text = _provider.RangeValues[i - 2, 1];
            quintile03.Text = _provider.QuantileValues[i - 2].ToString();

            coo = Color.FromArgb(240, (byte)(rampStart.Color.R - ri * 3), (byte)(rampStart.Color.G - gi * 3), (byte)(rampStart.Color.B - bi * 3));
            rctColor04.Visibility = System.Windows.Visibility.Visible;
            rampStart04.Visibility = System.Windows.Visibility.Visible;
            centerText04.Visibility = System.Windows.Visibility.Visible;
            rampEnd04.Visibility = System.Windows.Visibility.Visible;
            quintile04.Visibility = System.Windows.Visibility.Visible;
            legendText04.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(240, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor04.Visibility = System.Windows.Visibility.Hidden;
                rampStart04.Visibility = System.Windows.Visibility.Hidden;
                centerText04.Visibility = System.Windows.Visibility.Hidden;
                rampEnd04.Visibility = System.Windows.Visibility.Hidden;
                quintile04.Visibility = System.Windows.Visibility.Hidden;
                legendText04.Visibility = System.Windows.Visibility.Hidden;
            }
            if (isNewColorRamp) rctColor04.Fill = new SolidColorBrush(coo);
            rampStart04.Text = _provider.RangeValues[i - 2, 0];
            rampEnd04.Text = _provider.RangeValues[i - 2, 1];
            quintile04.Text = _provider.QuantileValues[i - 2].ToString();

            coo = Color.FromArgb(240, (byte)(rampStart.Color.R - ri * 4), (byte)(rampStart.Color.G - gi * 4), (byte)(rampStart.Color.B - bi * 4));
            rctColor05.Visibility = System.Windows.Visibility.Visible;
            rampStart05.Visibility = System.Windows.Visibility.Visible;
            centerText05.Visibility = System.Windows.Visibility.Visible;
            rampEnd05.Visibility = System.Windows.Visibility.Visible;
            quintile05.Visibility = System.Windows.Visibility.Visible;
            legendText05.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(240, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor05.Visibility = System.Windows.Visibility.Hidden;
                rampStart05.Visibility = System.Windows.Visibility.Hidden;
                centerText05.Visibility = System.Windows.Visibility.Hidden;
                rampEnd05.Visibility = System.Windows.Visibility.Hidden;
                quintile05.Visibility = System.Windows.Visibility.Hidden;
                legendText05.Visibility = System.Windows.Visibility.Hidden;
            }
            if (isNewColorRamp) rctColor05.Fill = new SolidColorBrush(coo);
            rampStart05.Text = _provider.RangeValues[i - 2, 0];
            rampEnd05.Text = _provider.RangeValues[i - 2, 1];
            quintile05.Text = _provider.QuantileValues[i - 2].ToString();

            coo = Color.FromArgb(240, (byte)(rampStart.Color.R - ri * 5), (byte)(rampStart.Color.G - gi * 5), (byte)(rampStart.Color.B - bi * 5));
            rctColor06.Visibility = System.Windows.Visibility.Visible;
            rampStart06.Visibility = System.Windows.Visibility.Visible;
            centerText06.Visibility = System.Windows.Visibility.Visible;
            rampEnd06.Visibility = System.Windows.Visibility.Visible;
            quintile06.Visibility = System.Windows.Visibility.Visible;
            legendText06.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(240, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor06.Visibility = System.Windows.Visibility.Hidden;
                rampStart06.Visibility = System.Windows.Visibility.Hidden;
                centerText06.Visibility = System.Windows.Visibility.Hidden;
                rampEnd06.Visibility = System.Windows.Visibility.Hidden;
                quintile06.Visibility = System.Windows.Visibility.Hidden;
                legendText06.Visibility = System.Windows.Visibility.Hidden;
            }
            if (isNewColorRamp) rctColor06.Fill = new SolidColorBrush(coo);
            rampStart06.Text = _provider.RangeValues[i - 2, 0];
            rampEnd06.Text = _provider.RangeValues[i - 2, 1];
            quintile06.Text = _provider.QuantileValues[i - 2].ToString();

            coo = Color.FromArgb(240, (byte)(rampStart.Color.R - ri * 6), (byte)(rampStart.Color.G - gi * 6), (byte)(rampStart.Color.B - bi * 6));
            rctColor07.Visibility = System.Windows.Visibility.Visible;
            rampStart07.Visibility = System.Windows.Visibility.Visible;
            centerText07.Visibility = System.Windows.Visibility.Visible;
            rampEnd07.Visibility = System.Windows.Visibility.Visible;
            quintile07.Visibility = System.Windows.Visibility.Visible;
            legendText07.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(240, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor07.Visibility = System.Windows.Visibility.Hidden;
                rampStart07.Visibility = System.Windows.Visibility.Hidden;
                centerText07.Visibility = System.Windows.Visibility.Hidden;
                rampEnd07.Visibility = System.Windows.Visibility.Hidden;
                quintile07.Visibility = System.Windows.Visibility.Hidden;
                legendText07.Visibility = System.Windows.Visibility.Hidden;
            }
            if (isNewColorRamp) rctColor07.Fill = new SolidColorBrush(coo);
            rampStart07.Text = _provider.RangeValues[i - 2, 0];
            rampEnd07.Text = _provider.RangeValues[i - 2, 1];
            quintile07.Text = _provider.QuantileValues[i - 2].ToString();

            coo = Color.FromArgb(240, (byte)(rampStart.Color.R - ri * 7), (byte)(rampStart.Color.G - gi * 7), (byte)(rampStart.Color.B - bi * 7));
            rctColor08.Visibility = System.Windows.Visibility.Visible;
            rampStart08.Visibility = System.Windows.Visibility.Visible;
            centerText08.Visibility = System.Windows.Visibility.Visible;
            rampEnd08.Visibility = System.Windows.Visibility.Visible;
            quintile08.Visibility = System.Windows.Visibility.Visible;
            legendText08.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(240, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor08.Visibility = System.Windows.Visibility.Hidden;
                rampStart08.Visibility = System.Windows.Visibility.Hidden;
                centerText08.Visibility = System.Windows.Visibility.Hidden;
                rampEnd08.Visibility = System.Windows.Visibility.Hidden;
                quintile08.Visibility = System.Windows.Visibility.Hidden;
                legendText08.Visibility = System.Windows.Visibility.Hidden;
            }
            if (isNewColorRamp) rctColor08.Fill = new SolidColorBrush(coo);
            rampStart08.Text = _provider.RangeValues[i - 2, 0];
            rampEnd08.Text = _provider.RangeValues[i - 2, 1];
            quintile08.Text = _provider.QuantileValues[i - 2].ToString();

            coo = Color.FromArgb(240, (byte)(rampStart.Color.R - ri * 8), (byte)(rampStart.Color.G - gi * 8), (byte)(rampStart.Color.B - bi * 8));
            rctColor09.Visibility = System.Windows.Visibility.Visible;
            rampStart09.Visibility = System.Windows.Visibility.Visible;
            centerText09.Visibility = System.Windows.Visibility.Visible;
            rampEnd09.Visibility = System.Windows.Visibility.Visible;
            quintile09.Visibility = System.Windows.Visibility.Visible;
            legendText09.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(240, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor09.Visibility = System.Windows.Visibility.Hidden;
                rampStart09.Visibility = System.Windows.Visibility.Hidden;
                centerText09.Visibility = System.Windows.Visibility.Hidden;
                rampEnd09.Visibility = System.Windows.Visibility.Hidden;
                quintile09.Visibility = System.Windows.Visibility.Hidden;
                legendText09.Visibility = System.Windows.Visibility.Hidden;
            }
            if (isNewColorRamp) rctColor09.Fill = new SolidColorBrush(coo);
            rampStart09.Text = _provider.RangeValues[i - 2, 0];
            rampEnd09.Text = _provider.RangeValues[i - 2, 1];
            quintile09.Text = _provider.QuantileValues[i - 2].ToString();

            coo = Color.FromArgb(240, (byte)(rampStart.Color.R - ri * 6), (byte)(rampStart.Color.G - gi * 9), (byte)(rampStart.Color.B - bi * 9));
            rctColor10.Visibility = System.Windows.Visibility.Visible;
            rampStart10.Visibility = System.Windows.Visibility.Visible;
            centerText10.Visibility = System.Windows.Visibility.Visible;
            rampEnd10.Visibility = System.Windows.Visibility.Visible;
            quintile10.Visibility = System.Windows.Visibility.Visible;
            legendText10.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(240, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor10.Visibility = System.Windows.Visibility.Hidden;
                rampStart10.Visibility = System.Windows.Visibility.Hidden;
                centerText10.Visibility = System.Windows.Visibility.Hidden;
                rampEnd10.Visibility = System.Windows.Visibility.Hidden;
                quintile10.Visibility = System.Windows.Visibility.Hidden;
                legendText10.Visibility = System.Windows.Visibility.Hidden;
            }
            if (isNewColorRamp) rctColor10.Fill = new SolidColorBrush(coo);
            rampStart10.Text = _provider.RangeValues[i - 2, 0];
            rampEnd10.Text = _provider.RangeValues[i - 2, 1];
            quintile10.Text = _provider.QuantileValues[i - 2].ToString();

            _initialRampCalc = false;
        }

        private void CheckBox_Quantiles_Click(object sender, RoutedEventArgs e)
        {
            OnQuintileOptionChanged();
        }

        private void OnQuintileOptionChanged()
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

        private void cmbShapeKey_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _shapeKey = cmbShapeKey.SelectedItem.ToString();
        }

        private void cmbDataKey_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _dataKey = cmbDataKey.SelectedItem.ToString();
        }

        private void cmbValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _value = cmbValue.SelectedItem.ToString();
        }
    }
    
}
        