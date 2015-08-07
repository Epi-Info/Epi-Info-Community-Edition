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
using EpiDashboard;
using Epi;

namespace EpiDashboard.Controls.GadgetProperties
{
    /// <summary>
    /// Interaction logic for WordCloudProperties.xaml
    /// </summary>
    public partial class WordCloudProperties : GadgetPropertiesPanelBase
    {
        public WordCloudProperties(
            DashboardHelper dashboardHelper,
            IGadget gadget,
            IGadgetParameters parameters
            )
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            this.Gadget = gadget;
            this.Parameters = (parameters as WordCloudParameters);

            //List<FieldInfo> items = new List<FieldInfo>();
            //List<string> fields = new List<string>();

            List<string> fieldNames = new List<string>();
            ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.DateTime | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.UserDefined | ColumnDataType.UserDefined;
            fieldNames = DashboardHelper.GetFieldsAsList(columnDataType);

     ///////FOR ENABLING MULTI-SELECT USING THE LISTVIEW IN THE FUTURE (As done in CombinedFrequency Properties)  ////////////////////
            //foreach (string fieldName in DashboardHelper.GetFieldsAsList(columnDataType))
            //{
            //    items.Add(new FieldInfo()
            //    {
            //        Name = fieldName,
            //        DataType = DashboardHelper.GetColumnDbType(fieldName).ToString(),
            //        VariableCategory = VariableCategory.Field
            //    });

            //    fields.Add(fieldName);
            //}

            //foreach (string fieldName in DashboardHelper.GetAllGroupsAsList())
            //{
            //    FieldInfo fieldInfo = new FieldInfo()
            //    {
            //        Name = fieldName,
            //        DataType = String.Empty,
            //        VariableCategory = VariableCategory.Group
            //    };
            //    items.Add(fieldInfo);
            //}

            //if (DashboardHelper.IsUsingEpiProject)
            //{
            //    for (int i = 0; i < DashboardHelper.View.Pages.Count; i++)
            //    {
            //        items.Add(new FieldInfo()
            //        {
            //            Name = "Page " + (i + 1).ToString(),
            //            DataType = String.Empty,
            //            VariableCategory = VariableCategory.Page
            //        });
            //    }
            //}

            fieldNames.Sort();

            cmbField.ItemsSource = fieldNames;
            //cmbGroupField.ItemsSource = fields;

            //CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvTextVariables.ItemsSource);
            //PropertyGroupDescription groupDescription = new PropertyGroupDescription("VariableCategory");
            //view.GroupDescriptions.Add(groupDescription);

            RowFilterControl = new RowFilterControl(this.DashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, (gadget as WordCloudControl).DataFilters, true);
            RowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            panelFilters.Children.Add(RowFilterControl);

            lblConfigExpandedTitle.Content = DashboardSharedStrings.GADGET_CONFIG_TITLE_WORDCLOUD;
            tbtnVariables.Title = DashboardSharedStrings.GADGET_TABBUTTON_VARIABLES;
            tbtnVariables.Description = DashboardSharedStrings.GADGET_TABDESC_WORDCLOUD;
            tbtnDisplay.Title = DashboardSharedStrings.GADGET_TABBUTTON_DISPLAY;
            tbtnDisplay.Description = DashboardSharedStrings.GADGET_TABDESC_DISPLAY;
            tbtnFilters.Title = DashboardSharedStrings.GADGET_TABBUTTON_FILTERS;
            tbtnFilters.Description = DashboardSharedStrings.GADGET_TABDESC_FILTERS;
            tblockPanelVariables.Content = DashboardSharedStrings.GADGET_PANELHEADER_VARIABLES;
            tblockPanelDisplay.Content = DashboardSharedStrings.GADGET_PANELHEADER_DISPLAY;
            tblockPanelDataFilter.Content = DashboardSharedStrings.GADGET_PANELHEADER_DATA_FILTER;
            tblockTitleNDescSubheader.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_TITLENDESC;
            tblockAnyFilterGadgetOnly.Content = DashboardSharedStrings.GADGET_FILTER_GADGET_ONLY;
            tblockVariableToParse.Content = DashboardSharedStrings.GADGET_VARIABLE_PARSE;
            tblockWordsIgnore.Content = DashboardSharedStrings.GADGET_WORDS_IGNORE;
        }

        public bool HasSelectedFields
        {
            get
            {
                if (cmbField.SelectedIndex > -1)
                {
                    return true;
                }
                return false;
            }
        }
        public WordCloudParameters Parameters { get; private set; }
        private List<Grid> StrataGridList { get; set; }
        private List<string> ColumnOrder { get; set; }

        /// <summary>
        /// Used to generate the list of variables and options for the GadgetParameters object
        /// </summary> 
        protected override void CreateInputVariableList()
        {
            this.DataFilters = RowFilterControl.DataFilters;

            //Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            if (Parameters != null)
            {
                if (cmbField.SelectedIndex > -1)
                {
                    if (Parameters.ColumnNames.Count > 0)
                    {
                        Parameters.ColumnNames[0] = cmbField.SelectedItem.ToString();
                    }
                    else
                    {
                        Parameters.ColumnNames.Add(cmbField.SelectedItem.ToString());
                    }
                }
                Parameters.GadgetTitle = txtTitle.Text;
                Parameters.GadgetDescription = txtDesc.Text;
                Parameters.CommonWords = txtCommonWords.Text;
            }

      ///// MAY HAVE MULTI-SELECT IN THE FUTURE, LIKE 
            //List<string> listFields = new List<string>();

            //if (lvTextVariables.SelectedItems.Count > 0)
            //{
            //    foreach (FieldInfo fieldInfo in lvTextVariables.SelectedItems)
            //    {
            //        if (!string.IsNullOrEmpty(fieldInfo.Name))
            //        {
            //            listFields.Add(fieldInfo.Name);
            //        }
            //    }
            //}

            //listFields.Sort();

            //foreach (string field in listFields)
            //{
            //    Parameters.ColumnNames.Add(field);
            //}

        }

        //private void CreateInputVariableList()   //FROM WORD CLOUD CONTROL
        //{
        //    Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

        //    GadgetOptions.MainVariableName = string.Empty;
        //    GadgetOptions.WeightVariableName = string.Empty;
        //    GadgetOptions.StrataVariableNames = new List<string>();
        //    GadgetOptions.CrosstabVariableName = string.Empty;
        //    GadgetOptions.ColumnNames = new List<string>();

        //    if (cbxField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxField.SelectedItem.ToString()))
        //    {
        //        inputVariableList.Add("freqvar", cbxField.SelectedItem.ToString());
        //        GadgetOptions.MainVariableName = cbxField.SelectedItem.ToString();
        //    }
        //    else
        //    {
        //        return;
        //    }

        //    {
        //        inputVariableList.Add("allvalues", "false");
        //        GadgetOptions.ShouldUseAllPossibleValues = false;
        //    }

        //    {
        //        GadgetOptions.ShouldShowCommentLegalLabels = false;
        //    }

        //    {
        //        GadgetOptions.ShouldSortHighToLow = false;
        //    }

        //    {
        //        inputVariableList.Add("includemissing", "false");
        //        GadgetOptions.ShouldIncludeMissing = false;
        //    }

        //    if (!inputVariableList.ContainsKey("usepromptforfield"))
        //    {
        //        {
        //            inputVariableList.Add("usepromptforfield", "false");
        //        }
        //    }

        //    GadgetOptions.ShouldIncludeFullSummaryStatistics = false;
        //    GadgetOptions.InputVariableList = inputVariableList;
        //}

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //lvTextVariables.MaxHeight = this.ActualHeight - 200;

            //Dictionary<string, string> inputVariableList = Parameters.InputVariableList;
    /////////  MAY ONE DAY ENABLE MULTI-SELECT  //  FOR NOW, JUST SINGLE COMBOBOX /////////////
            //foreach (string columnName in Parameters.ColumnNames)
            //{
            //    foreach (FieldInfo info in lvTextVariables.Items)
            //    {
            //        if (info.Name == columnName)
            //        {
            //            lvTextVariables.SelectedItems.Add(info);
            //            continue;
            //        }
            //    }
            //}
            if (Parameters != null)
            {
                if (Parameters.ColumnNames.Count > 0)
                {
                    cmbField.SelectedItem = Parameters.ColumnNames[0];
                }
                txtTitle.Text = Parameters.GadgetTitle;
                txtDesc.Text = Parameters.GadgetDescription;
                txtCommonWords.Text = Parameters.CommonWords;
            }
        }

        public class FieldInfo { public string Name { get; set; } public string DataType { get; set; } public VariableCategory VariableCategory { get; set; } }

        private void tbtnVariables_Checked(object sender, RoutedEventArgs e)
        {
            if (panelVariables == null) return;

            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Visible;
            //panelSorting.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        //private void tbtnSorting_Checked(object sender, RoutedEventArgs e)
        //{
        //    CheckButtonStates(sender as SettingsToggleButton);
        //    panelVariables.Visibility = System.Windows.Visibility.Collapsed;
        //    panelSorting.Visibility = System.Windows.Visibility.Visible;
        //    panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
        //    panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        //}

        private void tbtnDisplay_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            //panelSorting.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Visible;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnFilters_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            //panelSorting.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
