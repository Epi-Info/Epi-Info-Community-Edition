using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = System.Windows.Forms.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace EpiDashboard.Mapping
{
    /// <summary>
    /// Interaction logic for TimeLapse.xaml
    /// </summary>
    public partial class TimeLapse : UserControl
    {

        private ClusterLayerProvider provider;
        private IMapControl mapControl;
        public event EventHandler Cancelled;
        public event EventHandler ChangesAccepted;

        List<DashboardHelper> dashboardHelpers = new List<DashboardHelper>();


        public string TimeVariable
        {
            get
            {
                if (cmbVariable.SelectedIndex != -1)
                    return cmbVariable.SelectedItem.ToString();
                else
                    return null;
            }
        }

        # region Constructors
        public TimeLapse(List<DashboardHelper> dashboardHelpers, StandaloneMapControl mapControl, ESRI.ArcGIS.Client.Map myMap)
        {
            InitializeComponent();
            provider = new ClusterLayerProvider(myMap);
            this.mapControl = mapControl;
            mapControl.TimeVariableSet += new TimeVariableSetHandler(mapControl_TimeVariableSet);
            this.dashboardHelpers = dashboardHelpers;
            ApplyTranslation(); 
            FillComboBox(dashboardHelpers);           
        }

        public TimeLapse(Epi.View view)
        {
            InitializeComponent();
            ApplyTranslation();
            FillComboBox(view);
        }

        #endregion

        # region Private methods

        void mapControl_TimeVariableSet(string timeVariable)
        {
            provider.TimeVar = timeVariable;
        }

        private void ApplyTranslation()
        {
            lblTitle.Content = DashboardSharedStrings.MAP_TIMELAPSE;
            lblTimeVariable.Content = DashboardSharedStrings.MAP_TIME_FIELD;
            btnOK.Content = DashboardSharedStrings.BUTTON_OK;
            btnCancel.Content = DashboardSharedStrings.BUTTON_CANCEL;
        }
        private void FillComboBox(List<DashboardHelper> dashboardHelpers)
        {
            cmbVariable.Items.Clear();
            foreach (DashboardHelper dashboardHelper in dashboardHelpers)
            {
                ColumnDataType columnDataType = ColumnDataType.DateTime;
                List<string> dateFields = dashboardHelper.GetFieldsAsList(columnDataType);
                foreach (string dateField in dateFields)
                {
                    if (!cmbVariable.Items.Contains(dateField))
                    {
                        cmbVariable.Items.Add(dateField);
                    }
                }
            }
        }

        private void FillComboBox(Epi.View view)
        {
            cmbVariable.Items.Clear();
            foreach (Epi.Fields.Field f in view.Fields)
            {
                if ((f is Epi.Fields.DateField) || (f is Epi.Fields.DateTimeField) || (f is Epi.Fields.TimeField))
                {
                    cmbVariable.Items.Add(f.Name);
                }
            }
        }
      
        #endregion

        private void cmbVariable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            long count;

            foreach (DashboardHelper dashboardHelper in dashboardHelpers)
            {
                count = MapUIHelper.CountTimeStopsByTimeInterval(dashboardHelper, cmbVariable.SelectedItem.ToString());

                if (count > 1000)
                {
                    MessageBox.Show("There are too many Time Stops associated with the selected field " + 
                        cmbVariable.SelectedItem.ToString() , "Error",  MessageBoxButtons.OK);
                    cmbVariable.SelectedIndex = 0;
                    return;
                }
            }

            btnOK.IsEnabled = (cmbVariable.SelectedIndex != -1);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }       

        public void Closepopup()
        {

  
            if (ChangesAccepted != null)
            {
                ChangesAccepted(this, new EventArgs());
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }
    }
}
