using System;
using System.Collections.Generic;
using System.Data;
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
using EpiDashboard;
using EpiDashboard.Rules;

namespace EpiDashboard.Controls.GadgetProperties
{
    /// <summary>
    /// Interaction logic for CSCrosstabProperties.xaml
    /// </summary>
    public partial class ComplexSampleCrosstabProperties : GadgetPropertiesPanelBase
    {
        #region Private Variables
        ////private RequestUpdateStatusDelegate requestUpdateStatus;
        ////private CheckForCancellationDelegate checkForCancellation;
        //private List<TextBlock> gridDisclaimerList;
        //private List<Grid> strataChiSquareGridList;
        //private List<GadgetTwoByTwoPanel> strata2x2GridList;
        //private List<StackPanel> groupList;

        //private List<GadgetStrataListPanel> strataListPanels;
        //private GadgetStrataListPanel strataGroupPanel;

        //private bool columnWarningShown;
        //private int rowCount = 1;
        //private int columnCount = 1;
        //private Dictionary<string, string> inputVariableList;
        //private bool exposureIsDropDownList = false;
        //private bool exposureIsCommentLegal = false;
        //private bool exposureIsOptionField = false;
        //private bool outcomeIsNumeric = false;
        //private bool smartTable = true;
        //private bool showPercents = true;
        //private bool? isRunningGrouped2x2 = null;

        private List<string> YesValues;
        private List<string> NoValues;

        #endregion

        public ComplexSampleCrosstabProperties(
            DashboardHelper dashboardHelper, 
            IGadget gadget, 
            ComplexSampleCrosstabParameters parameters, 
            List<Grid> strataGridList
            )
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            this.Gadget = gadget;
            this.Parameters = parameters;
            this.StrataGridList = strataGridList;

            List<string> fields = new List<string>();
            List<string> weightFields = new List<string>();
            List<string> strataItems = new List<string>();
            
            //Variable fields
            fields.Add(String.Empty);
            ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.DateTime | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.UserDefined;
            foreach (string fieldName in DashboardHelper.GetFieldsAsList(columnDataType))
            {
                if (DashboardHelper.IsUsingEpiProject)
                {
                    if (!(fieldName == "RecStatus")) 
                        fields.Add(fieldName);
                }
                else
                {
                    fields.Add(fieldName);
                }
            }

            if (fields.Contains("SYSTEMDATE"))
            {
                fields.Remove("SYSTEMDATE");
            }

            //Weight Fields
            weightFields.Add(String.Empty);
            columnDataType = ColumnDataType.Numeric | ColumnDataType.UserDefined;
            foreach (string fieldName in DashboardHelper.GetFieldsAsList(columnDataType))
            {
                if (DashboardHelper.IsUsingEpiProject)
                {
                    if (!(fieldName == "RecStatus")) weightFields.Add(fieldName);
                }
                else
                {
                    weightFields.Add(fieldName);
                }
            }
            weightFields.Sort();

            //Strata Fields 
            strataItems.Add(String.Empty);
            columnDataType = ColumnDataType.Numeric | ColumnDataType.Boolean | ColumnDataType.Text | ColumnDataType.UserDefined;
            foreach (string fieldName in DashboardHelper.GetFieldsAsList(columnDataType))
            {
                if (DashboardHelper.IsUsingEpiProject)
                {
                    if (!(fieldName == "RecStatus" || fieldName == "FKEY" || fieldName == "GlobalRecordId")) strataItems.Add(fieldName);
                }
                else
                {
                    strataItems.Add(fieldName);
                }
            }

            if (DashboardHelper.IsUsingEpiProject)
            {
                if (fields.Contains("RecStatus")) fields.Remove("RecStatus");
                if (weightFields.Contains("RecStatus")) weightFields.Remove("RecStatus");

                if (strataItems.Contains("RecStatus")) strataItems.Remove("RecStatus");
                if (strataItems.Contains("FKEY")) strataItems.Remove("FKEY");
                if (strataItems.Contains("GlobalRecordId")) strataItems.Remove("GlobalRecordId");
            }

            List<string> allFieldNames = new List<string>();
            allFieldNames.AddRange(fields);
            allFieldNames.AddRange(DashboardHelper.GetAllGroupsAsList());

            cbxExposureField.ItemsSource = allFieldNames;
            cbxOutcomeField.ItemsSource = fields;
            cbxFieldWeight.ItemsSource = weightFields;
            cbxFieldStrata.ItemsSource = strataItems;
            cbxFieldPSU.ItemsSource = strataItems;

            if (cbxExposureField.Items.Count > 0)
            {
                cbxExposureField.SelectedIndex = -1;
                cbxOutcomeField.SelectedIndex = -1;
            }

            if (cbxFieldWeight.Items.Count > 0)
            {
                cbxFieldWeight.SelectedIndex = -1;
            }

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(cbxExposureField.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("VariableCategory");
            view.GroupDescriptions.Add(groupDescription);

            RowFilterControl = new RowFilterControl(this.DashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, (gadget as ComplexSampleTablesControl).DataFilters, true);
            RowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            panelFilters.Children.Add(RowFilterControl);


            #region Translation

            lblConfigExpandedTitle.Content = DashboardSharedStrings.GADGET_CONFIG_TITLE_COMPLEX_TABLES;
            tbtnVariables.Title = DashboardSharedStrings.GADGET_TABBUTTON_VARIABLES;
            tbtnVariables.Description = DashboardSharedStrings.GADGET_TABDESC_COMPLEX_TABLES;
            tbtnDisplay.Title = DashboardSharedStrings.GADGET_TABBUTTON_DISPLAY;
            tbtnDisplay.Description = DashboardSharedStrings.GADGET_TABDESC_DISPLAY;
            tbtnFilters.Title = DashboardSharedStrings.GADGET_TABBUTTON_FILTERS;
            tbtnFilters.Description = DashboardSharedStrings.GADGET_TABDESC_FILTERS;
            tblockPanelVariables.Content = DashboardSharedStrings.GADGET_PANELHEADER_VARIABLES;
            tblockWeightVariable.Text = DashboardSharedStrings.GADGET_WEIGHT_VARIABLE;
            tblockPanelDisplay.Content = DashboardSharedStrings.GADGET_PANELHEADER_DISPLAY;
            tblockTitleNDescSubheader.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_TITLENDESC;
            tblockTitle.Content = DashboardSharedStrings.GADGET_GADET_TITLE;
            tblockDesc.Content = DashboardSharedStrings.GADGET_DESCRIPTION;
            tblockPanelDataFilter.Content = DashboardSharedStrings.GADGET_PANELHEADER_DATA_FILTER;
            tblockAnyFilterGadgetOnly.Content = DashboardSharedStrings.GADGET_FILTER_GADGET_ONLY;
            tblockStrataVariable.Text = DashboardSharedStrings.GADGET_STRATA_VARIABLE;
            tblockPSU.Text = DashboardSharedStrings.GADGET_PSU;
            tblockOutcomeField.Text = DashboardSharedStrings.GADGET_OUTCOME_VARIABLE;
            tblockExposureField.Text = DashboardSharedStrings.GADGET_EXPOSURE_VARIABLE;
            btnOK.Content = DashboardSharedStrings.BUTTON_OK;
            btnCancel.Content = DashboardSharedStrings.BUTTON_CANCEL;
            #endregion // Translation

        }

        public bool HasSelectedFields
        {
            get
            {
                if (cbxExposureField.SelectedIndex > -1 && cbxOutcomeField.SelectedIndex > -1)
                {
                    return true;
                }
                return false;
            }
        }

        public ComplexSampleCrosstabParameters Parameters { get; private set; }
        private List<Grid> StrataGridList { get; set; }

        /// <summary>
        /// Used to generate the list of variables and options for the GadgetParameters object
        /// </summary> 
        protected override void CreateInputVariableList()
        {
            // Set data filters!
            this.DataFilters = RowFilterControl.DataFilters;

            Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            //Variables settings
//            Parameters.ColumnNames = new List<string>();
            Parameters.StrataVariableNames = new List<string>();

            if (cbxExposureField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxExposureField.SelectedItem.ToString())
                &&
                cbxFieldPSU.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldPSU.SelectedItem.ToString())
                &&
                cbxOutcomeField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxOutcomeField.SelectedItem.ToString())
                )
            {
                inputVariableList.Add("EXPOSURE_VARIABLE", cbxExposureField.SelectedItem.ToString());
                if (Parameters.ColumnNames.Count > 0)
                {
                    Parameters.ColumnNames[0] = cbxExposureField.SelectedItem.ToString();
                }
                else
                {
                    Parameters.ColumnNames.Add(cbxExposureField.SelectedItem.ToString());
                } 
                inputVariableList.Add("PSUVar", cbxFieldPSU.SelectedItem.ToString());
                Parameters.PSUVariableName = cbxFieldPSU.SelectedItem.ToString();
                inputVariableList.Add("OUTCOME_VARIABLE", cbxOutcomeField.SelectedItem.ToString());
                Parameters.CrosstabVariableName = cbxOutcomeField.SelectedItem.ToString();
            }
            else
            {
                if (cbxFieldPSU.SelectedIndex < 0 || string.IsNullOrEmpty(cbxFieldPSU.SelectedItem.ToString()))
                    Parameters.PSUVariableName = "";
                if (cbxOutcomeField.SelectedIndex < 0 || string.IsNullOrEmpty(cbxOutcomeField.SelectedItem.ToString()))
                    Parameters.CrosstabVariableName = "";
                if (cbxExposureField.SelectedIndex < 0 || string.IsNullOrEmpty(cbxExposureField.SelectedItem.ToString()))
                    Parameters.ColumnNames = new List<string>();
                if (cbxFieldWeight.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldWeight.SelectedItem.ToString()))
                {
                    inputVariableList.Add("WeightVar", cbxFieldWeight.SelectedItem.ToString());
                    Parameters.WeightVariableName = cbxFieldWeight.SelectedItem.ToString();
                }
                else
                {
                    Parameters.WeightVariableName = String.Empty;
                    inputVariableList.Add("WeightVar", String.Empty);
                }
                if (cbxFieldStrata.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldStrata.SelectedItem.ToString()))
                {
                    inputVariableList.Add("StratvarList", cbxFieldStrata.SelectedItem.ToString());
                    Parameters.StrataVariableNames = new List<string>();
                    Parameters.StrataVariableNames.Add(cbxFieldStrata.SelectedItem.ToString());
                }
                return;
            }

            if (cbxFieldWeight.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldWeight.SelectedItem.ToString()))
            {
                inputVariableList.Add("WeightVar", cbxFieldWeight.SelectedItem.ToString());
                Parameters.WeightVariableName = cbxFieldWeight.SelectedItem.ToString();
            }
            else
            {
                Parameters.WeightVariableName = String.Empty;
                inputVariableList.Add("WeightVar", String.Empty);
            }

            if (cbxFieldStrata.SelectedIndex > -1)
            {
                if (Parameters.StrataVariableNames.Count > 0)
                {
                    Parameters.StrataVariableNames[0] = cbxFieldStrata.SelectedItem.ToString();
                }
                else
                {
                    Parameters.StrataVariableNames.Add(cbxFieldStrata.SelectedItem.ToString());
                }
            }

            if (cbxFieldStrata.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldStrata.SelectedItem.ToString()))
            {
                inputVariableList.Add("StratvarList", cbxFieldStrata.SelectedItem.ToString());
                Parameters.StrataVariableNames = new List<string>();
                Parameters.StrataVariableNames.Add(cbxFieldStrata.SelectedItem.ToString());
            }
            Parameters.IncludeFullSummaryStatistics = false;
            Parameters.InputVariableList = inputVariableList;

            //2x2 Value mapping settings
            if (lbxYesValues.Items.Count > 0)
            {
                foreach (string thisItem in lbxYesValues.Items)
                {
                    if (!Parameters.YesValues.Contains(thisItem))
                    {
                        Parameters.YesValues.Add(thisItem);
                    }
                }
            }
            if (lbxNoValues.Items.Count > 0)
            {
                foreach (string thisItem in lbxNoValues.Items)
                {
                    if (!Parameters.NoValues.Contains(thisItem))
                    {
                        Parameters.NoValues.Add(thisItem);
                    }
                }
            }

            //Display settings
            Parameters.GadgetTitle = txtTitle.Text;
            Parameters.GadgetDescription = txtDesc.Text;

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.DashboardHelper = DashboardHelper;
            ComplexSampleCrosstabParameters crosstabParameters = (ComplexSampleCrosstabParameters)Parameters;

            //rctLowColor.Fill = crosstabParameters.LoColorFill;
            //rctHighColor.Fill = crosstabParameters.HiColorFill;

            //Variables settings
            if (Parameters.ColumnNames.Count > 0)
            {
                cbxExposureField.SelectedItem = Parameters.ColumnNames[0];
            }
            cbxOutcomeField.SelectedItem = Parameters.CrosstabVariableName;
            cbxFieldPSU.SelectedItem = Parameters.PSUVariableName; 
            cbxFieldWeight.SelectedItem = Parameters.WeightVariableName;
            if (Parameters.StrataVariableNames.Count > 0)
            {
                cbxFieldStrata.SelectedItem = Parameters.StrataVariableNames[0].ToString();
            }

            //2 x 2 Value mapping settings
            Update2x2ValueMappings();
            if (crosstabParameters.ColumnNames.Count > 0)
            {
                //foreach (string s in yesValues)
                foreach (string s in crosstabParameters.YesValues)
                {
                    if (lbxAllValues.Items.Contains(s))
                    {
                        lbxYesValues.Items.Add(s);
                        lbxAllValues.Items.Remove(s);
                    }
                }

                foreach (string s in crosstabParameters.NoValues)
                {
                    if (lbxAllValues.Items.Contains(s))
                    {
                        lbxNoValues.Items.Add(s);
                        lbxAllValues.Items.Remove(s);
                    }
                }
            }

            //Display settings
            txtTitle.Text = Parameters.GadgetTitle;
            txtDesc.Text = Parameters.GadgetDescription;

            CheckVariables();
        }

        public class FieldInfo { public string Name { get; set; } public string DataType { get; set; } public VariableCategory VariableCategory { get; set; } }

        private void tbtnVariables_Checked(object sender, RoutedEventArgs e)
        {
            if (panelVariables == null) return;

            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Visible;
            panelValueMapping.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayColors.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnValueMapping_Checked(object sender, RoutedEventArgs e)
        {
            if (panelVariables == null) return;

            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelValueMapping.Visibility = System.Windows.Visibility.Visible;
            panelDisplayColors.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnDisplay_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelValueMapping.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayColors.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Visible;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnDisplayColors_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelValueMapping.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayColors.Visibility = System.Windows.Visibility.Visible;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnFilters_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelValueMapping.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayColors.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Handles the check / unchecked events
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void checkboxCheckChanged(object sender, RoutedEventArgs e)
        {
            //if (sender == checkboxOutcomeContinuous && checkboxOutcomeContinuous.IsChecked == true)
            //{
            //    checkboxIncludeMissing.IsChecked = false;
            //}
            //else if (sender == checkboxIncludeMissing && checkboxIncludeMissing.IsChecked == true)
            //{
            //    checkboxOutcomeContinuous.IsChecked = false;
            //}
        }

        /// <summary>
        /// Checks the selected variables and enables/disables checkboxes as appropriate
        /// </summary>
        private void CheckVariables()
        {
            cbxFieldStrata.IsEnabled = true;
            if (cbxExposureField.SelectedIndex >= 0)
            {
                string exposureFieldName = cbxExposureField.SelectedItem.ToString();
                if (!String.IsNullOrEmpty(exposureFieldName))
                {
                    if (DashboardHelper.GetAllGroupsAsList().Contains(exposureFieldName))
                    {
                        cbxFieldStrata.IsEnabled = false;
                        cbxFieldStrata.SelectedIndex = -1;
                        //btnValueMappings.IsEnabled = true;
                    }
                    else if (DashboardHelper.IsColumnText(exposureFieldName) || DashboardHelper.IsColumnNumeric(exposureFieldName))
                    {
                        //btnValueMappings.IsEnabled = true;
                    }
                }
                else
                {
                    //btnValueMappings.IsEnabled = false;
                    YesValues = new List<string>();
                    lbxYesValues.SelectedItems.Clear();
                    NoValues = new List<string>();
                    lbxNoValues.SelectedItems.Clear();
                }
            }
        }


        private void Update2x2ValueMappings()
        {
            this.DashboardHelper = DashboardHelper;
            List<string> YesItems = new List<string>();
            List<string> NoItems = new List<string>();
            lbxAllValues.Items.Clear();
            if (lbxYesValues.Items.Count > 0)
            {
                foreach (string thisItem in lbxYesValues.Items)
                {
                    YesItems.Add(thisItem);
                }
            }
            if (lbxNoValues.Items.Count > 0)
            {
                foreach (string thisItem in lbxNoValues.Items)
                {
                    NoItems.Add(thisItem);
                }
            }
            lbxYesValues.Items.Clear();
            lbxNoValues.Items.Clear();

            if (Parameters.ColumnNames.Count > 0 && !String.IsNullOrEmpty(Parameters.ColumnNames[0])) GetValueLists(Parameters.ColumnNames[0]);
            if (!String.IsNullOrEmpty(Parameters.CrosstabVariableName)) GetValueLists(Parameters.CrosstabVariableName);

            foreach (string YesItem in Parameters.YesValues)
            {
                if (lbxAllValues.Items.Contains(YesItem))
                {
                    lbxAllValues.Items.Remove(YesItem);
                    lbxYesValues.Items.Add(YesItem);
                }
            }
            foreach (string NoItem in Parameters.NoValues)
            {
                if (lbxAllValues.Items.Contains(NoItem))
                {
                    lbxAllValues.Items.Remove(NoItem);
                    lbxNoValues.Items.Add(NoItem);
                }
            }

            //Attempting to Sort the list boxes.
            lbxAllValues.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Content", System.ComponentModel.ListSortDirection.Ascending));
            lbxYesValues.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Content", System.ComponentModel.ListSortDirection.Ascending));
            lbxNoValues.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Content", System.ComponentModel.ListSortDirection.Ascending));
        }



        private void GetValueLists(string fieldName)
        {
            if (!String.IsNullOrEmpty(fieldName))
            {
                List<string> distinctValueList = new List<string>();
                if (DashboardHelper.GetAllGroupsAsList().Contains(fieldName))
                {
                    foreach (string var in DashboardHelper.GetVariablesInGroup(fieldName))
                    {
                        distinctValueList = DashboardHelper.GetDistinctValuesAsList(var);
                        AddRemoveValues(distinctValueList);
                    }
                }
                else
                {
                    distinctValueList = DashboardHelper.GetDistinctValuesAsList(fieldName);
                    AddRemoveValues(distinctValueList);
                }
            }
        }

        private void AddRemoveValues(List<string> distinctValueList)
        {
            foreach (string s in distinctValueList)
            {
                if (!lbxAllValues.Items.Contains(s) && !lbxYesValues.Items.Contains(s) && !lbxNoValues.Items.Contains(s) && !String.IsNullOrEmpty(s))
                {
                    lbxAllValues.Items.Add(s);
                }
            }
        }

        private void cmbBreakType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tblockPct5 != null)
            {
                if (cmbBreakType.SelectedIndex == 0)
                {
                    tblockPct5.Visibility = System.Windows.Visibility.Visible;
                    tblockPct4.Visibility = tblockPct3.Visibility = tblockPct2.Visibility = tblockPct1.Visibility = tblockPct5.Visibility;
                    txtPct1.Text = "0";
                    txtPct2.Text = "20";
                    txtPct3.Text = "40";
                    txtPct4.Text = "60";
                    txtPct5.Text = "80";
                }
                else
                {
                    tblockPct5.Visibility = System.Windows.Visibility.Hidden;
                    tblockPct4.Visibility = tblockPct3.Visibility = tblockPct2.Visibility = tblockPct1.Visibility = tblockPct5.Visibility;
                }
            }
        }

        private void cbxExposureField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (panelVariables == null) return;
            if (cbxExposureField.SelectedIndex > -1)
            {
                if (!String.IsNullOrEmpty(cbxExposureField.SelectedItem.ToString()))
                {
                    if (Parameters.ColumnNames.Count > 0)
                    {
                        Parameters.ColumnNames[0] = cbxExposureField.SelectedItem.ToString();
                    }
                    else
                    {
                        Parameters.ColumnNames.Add(cbxExposureField.SelectedItem.ToString());
                    }
                    Update2x2ValueMappings();
                }
            }
            CheckVariables();
        }

        private void cbxOutcomeField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (panelVariables == null) return;
            if (cbxOutcomeField.SelectedIndex > -1)
            {
                if (!String.IsNullOrEmpty(cbxOutcomeField.SelectedItem.ToString()))
                {
                    Parameters.CrosstabVariableName = cbxOutcomeField.SelectedItem.ToString();
                    Update2x2ValueMappings();
                }
            }
        }

        private void btnAddYes_Click(object sender, RoutedEventArgs e)
        {
            if (lbxAllValues.SelectedItems.Count > 0)
            {
                string selectedItem = lbxAllValues.SelectedItem.ToString();

                lbxAllValues.Items.Remove(selectedItem);
                lbxYesValues.Items.Add(selectedItem);
            }
        }

        private void btnRemoveYes_Click(object sender, RoutedEventArgs e)
        {
            if (lbxYesValues.SelectedItems.Count > 0)
            {
                string selectedItem = lbxYesValues.SelectedItem.ToString();

                lbxYesValues.Items.Remove(selectedItem);
                lbxAllValues.Items.Add(selectedItem);
            }
        }

        private void btnAddNo_Click(object sender, RoutedEventArgs e)
        {
            if (lbxAllValues.SelectedItems.Count > 0)
            {
                string selectedItem = lbxAllValues.SelectedItem.ToString();

                lbxAllValues.Items.Remove(selectedItem);
                lbxNoValues.Items.Add(selectedItem);
            }
        }

        private void btnRemoveNo_Click(object sender, RoutedEventArgs e)
        {
            if (lbxNoValues.SelectedItems.Count > 0)
            {
                string selectedItem = lbxNoValues.SelectedItem.ToString();

                lbxNoValues.Items.Remove(selectedItem);
                lbxAllValues.Items.Add(selectedItem);
            }
        }

        void rctLowColor_MouseUp(object sender, MouseButtonEventArgs e)
        {

            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctLowColor.Fill = new SolidColorBrush(Color.FromArgb(0xF0, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                //Parameters.StartColorRed = dialog.Color.R;
                //Parameters.StartColorGreen = dialog.Color.G;
                //Parameters.StartColorBlue = dialog.Color.B;
            }
        }

        void rctHighColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctHighColor.Fill = new SolidColorBrush(Color.FromArgb(0xF0, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                //Parameters.EndColorRed = dialog.Color.R;
                //Parameters.EndColorGreen = dialog.Color.G;
                //Parameters.EndColorBlue = dialog.Color.B;
            }
        }
    }
}
