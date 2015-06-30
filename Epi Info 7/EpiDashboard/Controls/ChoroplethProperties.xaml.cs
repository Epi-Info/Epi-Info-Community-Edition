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
        public EpiDashboard.Mapping.ChoroplethLayerProvider _provider;
        private int _currentStratCount;
        private SolidColorBrush _currentColor_rampStart;
        private SolidColorBrush _currentColor_rampEnd;
        private bool _initialRampCalc;
        public ChoroplethLayerProperties layerprop;
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
            if (cmbShapeKey.SelectedItem != null &&
                cmbDataKey.SelectedItem != null &&
                cmbValue.SelectedItem != null)
            {
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

            if (!ValidateRangeInput())
                return;

            AddClassAttributes();

            int numclasses = Convert.ToInt32(cmbClasses.Text);
            bool flagquintiles = (bool)quintilesOption.IsChecked;

            layerprop.SetValues(txtShapePath.Text, cmbShapeKey.Text, cmbDataKey.Text, cmbValue.Text, cmbClasses.Text, rctHighColor.Fill, rctLowColor.Fill, shapeAttributes, ClassAttribList, flagquintiles, numclasses);
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

        private void AddClassAttributes()
        {

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

        private bool ValidateInputValue(TextBox textBox)
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
                    MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }

            if (_rangeCount >= 3)
            {
                if (!(ValidateInputValue(rampStart03) && ValidateInputValue(rampEnd03)))
                {
                    MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }

            if (_rangeCount >= 4)
            {
                if (!(ValidateInputValue(rampStart04) && ValidateInputValue(rampEnd04)))
                {
                    MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }
            if (_rangeCount >= 5)
            {
                if (!(ValidateInputValue(rampStart05) && ValidateInputValue(rampEnd05)))
                {
                    MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }
            if (_rangeCount >= 6)
            {
                if (!(ValidateInputValue(rampStart06) && ValidateInputValue(rampEnd06)))
                {
                    MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }
            if (_rangeCount >= 7)
            {
                if (!(ValidateInputValue(rampStart07) && ValidateInputValue(rampEnd07)))
                {
                    MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }
            if (_rangeCount >= 8)
            {
                if (!(ValidateInputValue(rampStart08) && ValidateInputValue(rampEnd08)))
                {
                    MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }
            if (_rangeCount >= 9)
            {
                if (!(ValidateInputValue(rampStart09) && ValidateInputValue(rampEnd09)))
                {
                    MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }
            if (_rangeCount >= 10)
            {
                if (!(ValidateInputValue(rampStart10) && ValidateInputValue(rampEnd10)))
                {
                    MessageBox.Show("Incorrect input value.");
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
                if (!int.TryParse(cmbClasses.Text, out classCount))
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

        private void rctColor8_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor08.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor9_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

            if (cmbShapeKey.SelectedItem != null &&
                cmbDataKey.SelectedItem != null &&
                cmbValue.SelectedItem != null)
            {
                SetRangeUISection();
            }
        }

        private void SetVisibility(int stratCount, SolidColorBrush rampStart, SolidColorBrush rampEnd)
        {
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
            if (isNewColorRamp) rctColor03.Fill = new SolidColorBrush(coo);
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

      
    

        private void SetRangeUISection()
        {
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
            if (!int.TryParse(((ComboBoxItem)cmbClasses.SelectedItem).Content.ToString(), out classCount))
            {
                classCount = 4;
            }

            _provider.PopulateRangeValues(_dashboardHelper,
                     cmbShapeKey.SelectedItem.ToString(),
                     cmbDataKey.SelectedItem.ToString(),
                     cmbValue.SelectedItem.ToString(),
                     brushList,
                     classCount);

            SolidColorBrush rampStart = (SolidColorBrush)rctLowColor.Fill;
            SolidColorBrush rampEnd = (SolidColorBrush)rctHighColor.Fill;

            SetVisibility(classCount, rampStart, rampEnd);
        }

    }

}
