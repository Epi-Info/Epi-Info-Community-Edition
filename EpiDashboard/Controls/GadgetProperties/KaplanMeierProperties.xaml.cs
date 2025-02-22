﻿using System;
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
using EpiDashboard;
using Epi;

namespace EpiDashboard.Controls.GadgetProperties
{
	/// <summary>
	/// Interaction logic for KaplanMeierProperties.xaml
	/// </summary>
	public partial class KaplanMeierProperties : GadgetPropertiesPanelBase
    {
        public KaplanMeierProperties(
            DashboardHelper dashboardHelper, 
            IGadget gadget,
            KaplanMeierParameters parameters
            )
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            this.Gadget = gadget;
            this.Parameters = parameters;


            List<string> fields = new List<string>();
            List<FieldInfo> items = new List<FieldInfo>();
            List<string> crosstabFields = new List<string>();
            List<string> strataFields = new List<string>();

            crosstabFields.Add(string.Empty);
            items.Add(new FieldInfo()
            {
                Name = "",
                DataType = "",
                VariableCategory = VariableCategory.Field
            });

            foreach (string fieldName in DashboardHelper.GetFieldsAsList())
            {
                items.Add(new FieldInfo()
                {
                    Name = fieldName,
                    DataType = DashboardHelper.GetColumnDbType(fieldName).ToString(),
                    VariableCategory = VariableCategory.Field
                });

                fields.Add(fieldName);
                crosstabFields.Add(fieldName);
                strataFields.Add(fieldName);
            }
            fields.Sort();
            crosstabFields.Sort();

            cbxFieldOutcome.ItemsSource = fields;
            cbxFieldWeight.ItemsSource = fields;
            cbxFieldMatch.ItemsSource = crosstabFields;
//            lbxOtherFields.ItemsSource = strataFields;
            cbxFields.ItemsSource = fields;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(cbxFieldOutcome.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("VariableCategory");
            view.GroupDescriptions.Add(groupDescription);

            RowFilterControl = new RowFilterControl(this.DashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, (gadget as KaplanMeierControl).DataFilters, true);
            RowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            panelFilters.Children.Add(RowFilterControl);



            #region Translation

            lblConfigExpandedTitleTxt.Text= "Kaplan-Meier Survival Analysis";
            tbtnVariables.Title = DashboardSharedStrings.GADGET_TABBUTTON_VARIABLES;
            tbtnVariables.Description = DashboardSharedStrings.GADGET_TABDESC_LOGISTIC_REGRESSION;
            tbtnDisplay.Title = DashboardSharedStrings.GADGET_TABBUTTON_DISPLAY;
            tbtnDisplay.Description = DashboardSharedStrings.GADGET_TABDESC_DISPLAY;
            tbtnFilters.Title = DashboardSharedStrings.GADGET_TABBUTTON_FILTERS;
            tbtnFilters.Description = DashboardSharedStrings.GADGET_TABDESC_FILTERS;
            tblockPanelVariablesTxt.Text= DashboardSharedStrings.GADGET_PANELHEADER_VARIABLES;
            tblockOutcomeVariableTxt.Text= "Censored Variable:";
            tblockWeightVariableTxt.Text= DashboardSharedStrings.GADGET_WEIGHT_VARIABLE;
            checkboxIncludeMissingTxt.Text= DashboardSharedStrings.GADGET_INCLUDE_MISSING;
            tblockPanelSortingTxt.Text= DashboardSharedStrings.GADGET_PANELHEADER_SORTING;
            tblockGroupingSubheaderTxt.Text= DashboardSharedStrings.GADGET_PANELSUBHEADER_GROUPING;
            tblockSortingSubheaderTxt.Text= DashboardSharedStrings.GADGET_PANELSUBHEADER_SORTING;
            tblockSortMethodTxt.Text= DashboardSharedStrings.GADGET_SORT_METHOD;
            tblockPanelDisplayTxt.Text= DashboardSharedStrings.GADGET_PANELHEADER_DISPLAY;
            tblockTitleNDescSubheaderTxt.Text= DashboardSharedStrings.GADGET_PANELSUBHEADER_TITLENDESC;
            tblockTitleTxt.Text= DashboardSharedStrings.GADGET_GADET_TITLE;
            tblockDescTxt.Text= DashboardSharedStrings.GADGET_DESCRIPTION;
            checkboxListLabelsTxt.Text= DashboardSharedStrings.GADGET_DISPLAY_LIST_LABELS;
            tblockPanelDataFilterTxt.Text= DashboardSharedStrings.GADGET_PANELHEADER_DATA_FILTER;
            tblockAnyFilterGadgetOnlyTxt.Text= DashboardSharedStrings.GADGET_FILTER_GADGET_ONLY;
            checkboxShowANOVATxt.Text= DashboardSharedStrings.GADGET_DISPLAY_ANOVA;
            tblockGroupbyTxt.Text= DashboardSharedStrings.GADGET_GROUP_BY;
            tblockSubGroupByTxt.Text= DashboardSharedStrings.GADGET_SUBGROUP_BY;
            tblockAvailableVariablesTxt.Text= DashboardSharedStrings.GADGET_AVAILABLE_VARIABLES;
            tblockSortOrderTxt.Text= DashboardSharedStrings.GADGET_SORT_ORDER;
            checkboxLineColumnTxt.Text= DashboardSharedStrings.GADGET_SHOW_LINE_COLUMN;
            checkboxColumnHeadersTxt.Text= DashboardSharedStrings.GADGET_SHOW_COLUMN_HEADINGS;
            checkboxShowNullsTxt.Text= DashboardSharedStrings.GADGET_SHOW_MISSING_REP;
            tblockPrecision.Text = DashboardSharedStrings.GADGET_DECIMALS_TO_DISPLAY;
            tblockOutputColumns.Text = DashboardSharedStrings.GADGET_OUTPUT_COLUMNS_DISPLAY;
            btnMakeDummyTxt.Text= DashboardSharedStrings.GADGET_MAKE_DUMMY;
            btnClearInteractionTermsTxt.Text= DashboardSharedStrings.GADGET_CLEAR_TERMS;
            tblockConfidenceLimitsTxt.Text= DashboardSharedStrings.GADGET_CONFIDENCE_LIMITS;
            tblockDummyVariables.Text = DashboardSharedStrings.GADGET_DUMMY_VARIABLES;
            tblockInteractionTerms.Text = DashboardSharedStrings.GADGET_INTERACTION_TERMS;
            checkboxNoInterceptTxt.Text= DashboardSharedStrings.GADGET_NO_INTERCEPT;
            tblockIndependentVariablesTxt.Text= "Test Group Variable:";
            tblockMatchVariableTxt.Text= "Time Variable:";
            btnRunTxt.Text= DashboardSharedStrings.BUTTON_OK;
            btnCancelTxt.Text= DashboardSharedStrings.BUTTON_CANCEL;

            #endregion // Translation
        }

        public bool HasSelectedFields
        {
            get
            {
                if (cbxFieldOutcome.SelectedIndex > -1)
                {
                    return true;
                }
                return false;
            }
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            EpiDashboard.KaplanMeierControl lrc = new EpiDashboard.KaplanMeierControl(this.DashboardHelper);
            lrc = (EpiDashboard.KaplanMeierControl)Gadget;
            lrc.cbxFieldOutcome = this.cbxFieldOutcome;
            lrc.lbxOtherFields = this.lbxOtherFields;
			lrc.cbxFields = this.cbxFields;
            lrc.lbxDummyTerms = this.lbxDummyTerms;
            lrc.cbxFieldWeight = this.cbxFieldWeight;
			lrc.cbxFieldUncensored = this.cbxFieldUncensored;
			lrc.cbxFieldLink = this.cbxFieldLink;
			lrc.checkboxNoIntercept = this.checkboxNoIntercept;
            lrc.checkboxIncludeMissing = this.checkboxIncludeMissing;
            lrc.cbxFieldMatch = this.cbxFieldMatch;
            lrc.cbxConf = this.cbxConf;
            lrc.lbxInteractionTerms = this.lbxInteractionTerms;
            lrc.txtFilterString = null;
            lrc.DataFilters = RowFilterControl.DataFilters;
            lrc.descriptionPanel.Text = this.txtDesc.Text;
            if (!String.IsNullOrEmpty(this.txtTitle.Text))
                lrc.headerPanel.Text = this.txtTitle.Text;
            else
                lrc.headerPanel.Text = "Kaplan-Meier Survival";
            this.Parameters.GadgetDescription = lrc.descriptionPanel.Text;
            this.Parameters.GadgetTitle = lrc.headerPanel.Text;
            lrc.RefreshResults();
            btnCancel_Click(sender, e);
//            RefreshResults();
        }

        private void btnClearInteractionTerms_Click(object sender, RoutedEventArgs e)
        {
            lbxInteractionTerms.Items.Clear();
        }

        private void btnMakeDummy_Click(object sender, RoutedEventArgs e)
        {
            if (lbxOtherFields.SelectedItems.Count == 1)
            {
                if (!string.IsNullOrEmpty(lbxOtherFields.SelectedItem.ToString()))
                {
                    string columnName = lbxOtherFields.SelectedItem.ToString();
                    lbxOtherFields.Items.Remove(columnName);
                    lbxDummyTerms.Items.Add(columnName);
                }
            }
            else if (lbxOtherFields.SelectedItems.Count == 2)
            {
                string term = lbxOtherFields.SelectedItems[0] + "*" + lbxOtherFields.SelectedItems[1];
                if (!lbxInteractionTerms.Items.Contains(term))
                {
                    lbxInteractionTerms.Items.Add(term);
                }
            }
			else if (lbxOtherFields.SelectedItems.Count > 2)
			{
				string term = lbxOtherFields.SelectedItems[0] + "*" + lbxOtherFields.SelectedItems[1];
				for (int i = 2; i < lbxOtherFields.SelectedItems.Count; i++)
					term = term + "*" + lbxOtherFields.SelectedItems[i];
				if (!lbxInteractionTerms.Items.Contains(term))
				{
					lbxInteractionTerms.Items.Add(term);
				}
			}
		}

		private void lbxOtherFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbxOtherFields.SelectedItems.Count == 1 || lbxOtherFields.SelectedItems.Count >= 2)
            {
                btnMakeDummy.IsEnabled = true;
            }
            else
            {
                btnMakeDummy.IsEnabled = false;
            }

            if (lbxOtherFields.SelectedItems.Count == 1)
            {
                btnMakeDummyTxt.Text= "Make Dummy";
            }
            else if (lbxOtherFields.SelectedItems.Count >= 2)
            {
                btnMakeDummyTxt.Text= "Make Interaction";
            }
		}



		private List<string> GetValueLists(string fieldName)
		{
			if (!String.IsNullOrEmpty(fieldName))
			{
				List<string> distinctValueList = new List<string>();
				if (DashboardHelper.GetAllGroupsAsList().Contains(fieldName))
				{
					foreach (string var in DashboardHelper.GetVariablesInGroup(fieldName))
					{
						distinctValueList = DashboardHelper.GetDistinctValuesAsList(var);
					}
					return distinctValueList;
				}
				else
				{
					distinctValueList = DashboardHelper.GetDistinctValuesAsList(fieldName);
					return distinctValueList;
				}
			}
			return new List<string>();
		}

		void cbxFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxFields.SelectedIndex > -1)
            {
				lbxOtherFields.Items.Clear();
                if (!lbxOtherFields.Items.Contains(cbxFields.SelectedItem.ToString()))
                {
                    lbxOtherFields.Items.Add(cbxFields.SelectedItem.ToString());
                }
            }
        }

        private void lbxOtherFields_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbxOtherFields.SelectedItems.Count == 1)
            {
                lbxOtherFields.Items.Remove(lbxOtherFields.SelectedItem);
            }
        }

        private void lbxColumns_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void lbxDummyTerms_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbxDummyTerms.SelectedItems.Count == 1)
            {
                string s = lbxDummyTerms.SelectedItem.ToString();
                lbxDummyTerms.Items.Remove(lbxDummyTerms.SelectedItem);
                lbxOtherFields.Items.Add(s);
            }
        }

        private void lbxInteractionTerms_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbxInteractionTerms.SelectedItems.Count == 1)
            {
                lbxInteractionTerms.Items.Remove(lbxInteractionTerms.SelectedItem);
            }
        }

        public KaplanMeierParameters Parameters { get; private set; }

        /// <summary>
        /// Used to generate the list of variables and options for the GadgetParameters object
        /// </summary> 
        protected override void CreateInputVariableList()
        {
            // Set data filters!
            this.DataFilters = RowFilterControl.DataFilters;

            Dictionary<string, string> inputVariableList = new Dictionary<string, string>();
            Parameters.ColumnNames = new List<string>();

            Parameters.SortVariables = new Dictionary<string, SortOrder>();

            Parameters.GadgetTitle = txtTitle.Text;
            Parameters.GadgetDescription = txtDesc.Text;

            List<string> listFields = new List<string>();

            Parameters.ShowCommentLegalLabels = checkboxListLabels.IsChecked.Value;
            Parameters.InputVariableList = inputVariableList;

            if (cbxFieldOutcome.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldOutcome.SelectedItem.ToString()))
            {
                if (Parameters.ColumnNames.Count > 0)
                {
                    Parameters.ColumnNames[0] = cbxFieldOutcome.SelectedItem.ToString();
                }
                else
                {
                    Parameters.ColumnNames.Add(cbxFieldOutcome.SelectedItem.ToString());
                }
            }
            else
            {
                return;
            }

            if (cbxFieldWeight.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldWeight.SelectedItem.ToString()))
            {
                Parameters.WeightVariableName = cbxFieldWeight.SelectedItem.ToString();
            }

            if (lbxOtherFields.SelectedItems.Count > 0)
            {
                Parameters.StrataVariableNames = new List<string>();
                foreach (String s in lbxOtherFields.SelectedItems)
                {
                    if (!string.IsNullOrEmpty(s))
                        Parameters.StrataVariableNames.Add(s);
                }
            }
            else
                Parameters.StrataVariableNames.Clear();

            if (cbxFieldMatch.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldMatch.SelectedItem.ToString()))
            {
                Parameters.CrosstabVariableName = cbxFieldMatch.SelectedItem.ToString();
            }
            else
            {
                Parameters.CrosstabVariableName = String.Empty;
            }

            if (checkboxShowANOVA.IsChecked == true)
            {
                inputVariableList.Add("showanova", "true");
                Parameters.ShowANOVA = true;
            }
            else
            {
                inputVariableList.Add("showanova", "false");
                Parameters.ShowANOVA = false;
            }

            if (cbxFieldPrecision.SelectedIndex >= 0)
            {
                inputVariableList.Add("precision", cbxFieldPrecision.SelectedIndex.ToString());
                Parameters.Precision = cbxFieldPrecision.SelectedIndex.ToString();
            }
            Parameters.columnsToHide.Clear();
            if (lbxColumns.SelectedItems.Count > 0)
            {
                if (!lbxColumns.SelectedItems.Contains("Observations")) Parameters.columnsToHide.Add(1);
                if (!lbxColumns.SelectedItems.Contains("Total")) Parameters.columnsToHide.Add(2);
                if (!lbxColumns.SelectedItems.Contains("Mean")) Parameters.columnsToHide.Add(3);
                if (!lbxColumns.SelectedItems.Contains("Variance")) Parameters.columnsToHide.Add(4);
                if (!lbxColumns.SelectedItems.Contains("Std. Dev.")) Parameters.columnsToHide.Add(5);
                if (!lbxColumns.SelectedItems.Contains("Minimum")) Parameters.columnsToHide.Add(6);
                if (!lbxColumns.SelectedItems.Contains("25%")) Parameters.columnsToHide.Add(7);
                if (!lbxColumns.SelectedItems.Contains("Median")) Parameters.columnsToHide.Add(8);
                if (!lbxColumns.SelectedItems.Contains("75%")) Parameters.columnsToHide.Add(9);
                if (!lbxColumns.SelectedItems.Contains("Maximum")) Parameters.columnsToHide.Add(10);
                if (!lbxColumns.SelectedItems.Contains("Mode")) Parameters.columnsToHide.Add(11);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Dictionary<string, string> inputVariableList = Parameters.InputVariableList;

            //Just one column for Frequency, .ColumnNames should have only one item
            txtTitle.Text = Parameters.GadgetTitle;
            txtDesc.Text = Parameters.GadgetDescription;
            EpiDashboard.KaplanMeierControl lrc = (EpiDashboard.KaplanMeierControl)Gadget;
            if (Parameters.ColumnNames.Count > 0)
            {
                cbxFieldOutcome.SelectedItem = Parameters.ColumnNames[0];
            }
            cbxConf.ItemsSource = null;
            cbxConf.Items.Clear();

            cbxConf.Items.Add("90%");
            cbxConf.Items.Add("95%");
            cbxConf.Items.Add("99%");
            cbxConf.SelectedIndex = 1;

			cbxFieldUncensored.ItemsSource = null;
			cbxFieldUncensored.Items.Clear();

			cbxFieldLink.ItemsSource = null;
			cbxFieldLink.Items.Clear();

			cbxFieldUncensored.Items.Add("");
			cbxFieldUncensored.SelectedIndex = 0;

			cbxFieldLink.Items.Add("Logit");
			cbxFieldLink.Items.Add("Log (For Evaluation)");
			cbxFieldLink.SelectedIndex = 0;

			cbxFieldWeight.SelectedItem = lrc.cbxFieldWeight.SelectedItem;
            cbxFieldMatch.SelectedItem = lrc.cbxFieldMatch.SelectedItem;
            cbxConf.SelectedItem = lrc.cbxConf.SelectedItem;
			if (lrc.cbxFieldUncensored.SelectedItem != null)
				cbxFieldUncensored.SelectedItem = lrc.cbxFieldUncensored.SelectedItem;
			if (lrc.cbxFieldLink.SelectedItem != null)
				cbxFieldLink.SelectedItem = lrc.cbxFieldLink.SelectedItem;
			checkboxNoIntercept.IsChecked = lrc.checkboxNoIntercept.IsChecked;
            checkboxIncludeMissing.IsChecked = lrc.checkboxIncludeMissing.IsChecked;
            checkboxShowANOVA.IsChecked = Parameters.ShowANOVA;
            cbxFieldPrecision.SelectedIndex = Convert.ToInt32(Parameters.Precision);
            lbxOtherFields.MaxHeight = lbxOtherFields.MaxHeight + (System.Windows.SystemParameters.PrimaryScreenHeight - 768.0);
            scrollViewerDisplay.Height = scrollViewerDisplay.Height + (System.Windows.SystemParameters.PrimaryScreenHeight - 768.0);
            if (lrc.lbxOtherFields.Items.Count > 0)
                foreach (string s in lrc.lbxOtherFields.Items)
                    lbxOtherFields.Items.Add(s.ToString());
            if (lrc.lbxDummyTerms.Items.Count > 0)
                foreach (string s in lrc.lbxDummyTerms.Items)
                    lbxDummyTerms.Items.Add(s.ToString());
            if (lrc.lbxInteractionTerms.Items.Count > 0)
                foreach (string s in lrc.lbxInteractionTerms.Items)
                    lbxInteractionTerms.Items.Add(s.ToString());

            if (lbxColumns.Items.Count == 0)
            {
                lbxColumns.Items.Add("Observations");
                lbxColumns.Items.Add("Total");
                lbxColumns.Items.Add("Mean");
                lbxColumns.Items.Add("Variance");
                lbxColumns.Items.Add("Std. Dev.");
                lbxColumns.Items.Add("Minimum");
                lbxColumns.Items.Add("25%");
                lbxColumns.Items.Add("Median");
                lbxColumns.Items.Add("75%");
                lbxColumns.Items.Add("Maximum");
                lbxColumns.Items.Add("Mode");
                lbxColumns.SelectAll();
            }

            cbxFieldOutcome.SelectedItem = lrc.cbxFieldOutcome.SelectedItem;
			cbxFields.SelectedItem = lrc.cbxFields.SelectedItem;
			if (lrc.cbxFields.SelectedItem == null && lrc.lbxOtherFields.Items.Count > 0)
				cbxFields.SelectedItem = lrc.lbxOtherFields.Items[0];
			if (cbxFieldOutcome.SelectedItem != null && !String.IsNullOrEmpty(cbxFieldOutcome.SelectedItem.ToString()))
			{
				List<string> censoredValues = GetValueLists(cbxFieldOutcome.SelectedValue.ToString());
				cbxFieldUncensored.Items.Clear();
				cbxFieldUncensored.Items.Add("");
				foreach (string s in censoredValues)
				{
					cbxFieldUncensored.Items.Add(s);
				}
			}
			cbxFieldUncensored.SelectedValue = lrc.cbxFieldUncensored.SelectedValue;
            //checkboxShowAllListValues.IsChecked = Parameters.ShowAllListValues;
            //checkboxShowListLabels.IsChecked = Parameters.ShowListLabels;
            //checkboxSortHighLow.IsChecked = Parameters.SortHighToLow;
            //checkboxIncludeMissing.IsChecked = Parameters.IncludeMissing;

            //checkboxUsePrompts.IsChecked = Parameters.UseFieldPrompts;
            //checkboxDrawBorders.IsChecked = Parameters.DrawBorders;
            //checkboxDrawHeader.IsChecked = Parameters.DrawHeaderRow;
            //checkboxDrawTotal.IsChecked = Parameters.DrawTotalRow;

            //cbxFieldPrecision.SelectedItem = Parameters.Precision;

            //tblockBarWidth.Text = Parameters.PercentBarWidth.ToString();

            //checkboxColumnFrequency.IsChecked = Parameters.ShowFrequencyCol;
            //checkboxColumnPercent.IsChecked = Parameters.ShowPercentCol;
            //checkboxColumnCumulativePercent.IsChecked = Parameters.ShowCumPercentCol;
            //checkboxColumn95CILower.IsChecked = Parameters.Show95CILowerCol;
            //checkboxColumn95CIUpper.IsChecked = Parameters.Show95CIUpperCol;
            //checkboxColumnPercentBars.IsChecked = Parameters.ShowPercentBarsCol;
        }

        public class FieldInfo { public string Name { get; set; } public string DataType { get; set; } public VariableCategory VariableCategory { get; set; } }

        private void tbtnVariables_Checked(object sender, RoutedEventArgs e)
        {
            if (panelVariables == null) return;

            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Visible;
            panelSorting.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnSorting_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelSorting.Visibility = System.Windows.Visibility.Visible;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnDisplay_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelSorting.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Visible;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnFilters_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelSorting.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Visible;
        }

        private void lbxAvailableVariables_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (lbxAvailableVariables.SelectedItems.Count == 1)
            //{
            //    string fieldName = lbxAvailableVariables.SelectedItem.ToString();

            //    lbxAvailableVariables.Items.Remove(fieldName);

            //    string method = " (ascending)";
            //    if (cmbSortMethod.SelectedIndex == 1)
            //    {
            //        method = " (descending)";
            //    }
            //    lbxSortOrder.Items.Add(fieldName + method);
            //}
        }

        private void lbxSortOrder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (lbxSortOrder.SelectedItems.Count == 1)
            //{
            //    string fieldName = lbxSortOrder.SelectedItem.ToString();

            //    lbxSortOrder.Items.Remove(fieldName);

            //    string originalFieldName = fieldName.Replace(" (ascending)", String.Empty).Replace(" (descending)", String.Empty);

            //    lbxAvailableVariables.Items.Add(originalFieldName);

            //    List<string> items = new List<string>();

            //    foreach (string item in lbxAvailableVariables.Items)
            //    {
            //        items.Add(item);
            //    }

            //    items.Sort();

            //    lbxAvailableVariables.Items.Clear();

            //    foreach (string item in items)
            //    {
            //        lbxAvailableVariables.Items.Add(item);
            //    }
            //}
        }

		private void cbxOutcome_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			List<string> censoredValues = GetValueLists(cbxFieldOutcome.SelectedValue.ToString());
			cbxFieldUncensored.Items.Clear();
			cbxFieldUncensored.Items.Add("");
			foreach (string s in censoredValues)
			{
				cbxFieldUncensored.Items.Add(s);
			}
		}
	}
}
