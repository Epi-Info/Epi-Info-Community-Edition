using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Epi;
using Epi.Data.Services;
using Epi.Windows;
using Epi.Fields;
using Epi.Windows.Dialogs;
using Epi.Windows.Enter.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Reflection;

using Epi.Windows.Controls;

namespace Epi.Windows.Enter.Dialogs
{
    /// <summary>
    /// Dialog for finding records
    /// </summary>
    public partial class FindRecords : DialogBase
    {
        #region Private Class Members
        private View view;
        private new EnterMainForm mainForm;
        DataGrid dataGrid1;
        DataTable data;        
        private int labelLinePosition = 31;
        private int textLinePosition = 25;
        private int patternLinePosition = 31;
        private Collection<string> searchFields;
        
//        private Collection<object> searchFieldValues;

        private ArrayList searchFieldValues;      
                        
        private Collection<SearchListBoxItem> searchFieldsCollection = new Collection<SearchListBoxItem>();
        private Collection<string> searchFieldItemTypes = new Collection<string>();
        private Collection<string> comparisonTypes;
        private Dictionary<string, int> OrFieldCount;
        private Dictionary<string, Control> myControlDictionary = new Dictionary<string, Control>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, object> myControlDictionaryData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<Control, SearchListBoxItem> myControlItemDictionary = new Dictionary<Control, SearchListBoxItem>();

        #endregion  //Private Class Members

        #region Constructors

        /// <summary>
        /// Default constructor for Find Records dialog
        /// </summary>
        public FindRecords()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for FindRecords dialog
        /// </summary>
        /// <param name="view">The current view</param>
        /// <param name="mainForm">Enter module's main form</param>
        public FindRecords(View view, EnterMainForm mainForm)
            : base(mainForm)
        {
            #region Input Validation
            if (view == null)
            {
                {
                    throw new ArgumentNullException("view");
                }
            }
            #endregion Input Validation

            InitializeComponent();
            this.view = view;
            this.mainForm = mainForm;
            this.KeyPreview = true;
        }

        #endregion  //Constructors

        #region Private Methods

        /// <summary>
        /// Fills the listbox with controls to be searched for
        /// </summary>
        private void BuildControlsList()
        {
            lbxSearchFields.Items.Clear();

            if (view.Pages.Count > 0)
            {
                foreach (Page page in view.Pages)
                {
                    foreach (Epi.Fields.Field field in page.Fields)
                    {
                         if (!(field is UniqueKeyField) && !(field is RecStatusField) && !(field is MirrorField) && !(field is RelatedViewField) && !(field is LabelField) && !(field is GridField) && !(field is GroupField) && !(field is CommandButtonField))
                           {
                            if (field is IPatternable)
                            {
                                lbxSearchFields.Items.Add(new SearchListBoxItem(field.Name.ToString(), field.Id, field.FieldType.ToString(), ((IPatternable)field).Pattern, string.Empty));
                            }
                            else
                            {
                                lbxSearchFields.Items.Add(new SearchListBoxItem(field.Name.ToString(), field.Id, field.FieldType.ToString(), string.Empty, string.Empty));
                            }
                        }
                    }
                 }
                //--Ei-139
                RecStatusField recstatus = new RecStatusField(view);
                lbxSearchFields.Items.Add(new SearchListBoxItem(recstatus.Name.ToString(), recstatus.Id, recstatus.FieldType.ToString(), string.Empty, string.Empty));
                GlobalRecordIdField globalfld = new GlobalRecordIdField(view);
                lbxSearchFields.Items.Add(new SearchListBoxItem(globalfld.Name.ToString(), globalfld.Id, globalfld.FieldType.ToString(), string.Empty, string.Empty));
                //--
              }
        }

        /// <summary>
        /// Adds controls to panel
        /// </summary>
        private void AddControlsToPanel()
        {
            ResetForm();
            LoadControls();
        }

        /// <summary>
        /// Loads controls on the panel
        /// </summary>
        private void LoadControls()
        {
            foreach (SearchListBoxItem item in searchFieldsCollection)
            {
                    //add masked textbox or textbox to the panel
                    if (!string.IsNullOrEmpty(item.Pattern))
                    {
                        //Add masked textbox to the panel                    
                        MaskedTextBox fieldTextBox = new MaskedTextBox();
                        fieldTextBox.Location = new System.Drawing.Point(300, textLinePosition);
                        fieldTextBox.Size = new System.Drawing.Size(286, 20);
                        fieldTextBox.BringToFront();
                        fieldTextBox.Leave += new EventHandler(fieldTextBox_Leave);
                        //                    fieldTextBox.MaskInputRejected += new MaskInputRejectedEventHandler(maskedTextBox_MaskInputRejected); 

                        string mask = AppData.Instance.DataPatternsDataTable.GetMaskByPattern(item.Pattern);
                        //fieldTextBox.Mask = AppData.Instance.DataPatternsDataTable.GetExpressionByMask(mask, item.Pattern);
                        fieldTextBox.Tag = item.Type;
                        fieldTextBox.Name = item.Name;
                        //fieldTextBox.Mask = mask;

                        if (item.Type.Equals("Number") || item.Type.Equals("PhoneNumber"))
                        {
                            //    fieldTextBox.PromptChar = '_';
                        }
                        else if (item.Type.Equals("Date") || item.Type.Equals("DateTime"))
                        {
                            //    fieldTextBox.PromptChar = '_';                                                
                            fieldTextBox.Text = String.Format("{0}", string.Empty);
                        }
                        else if (item.Type.Equals("Time"))
                        {
                            //    fieldTextBox.PromptChar = '_';
                            fieldTextBox.Text = String.Format("{0:T}", string.Empty);
                        }

                        splitContainer1.Panel2.Controls.Add(fieldTextBox);
                        textLinePosition += 38;
                        if (myControlDictionaryData.ContainsKey(item.Name.ToString()))
                        {
                            fieldTextBox.Text = myControlDictionaryData[item.Name.ToString()].ToString();
                        }

                        myControlDictionary.Add(item.Name.ToString(), fieldTextBox);
                        myControlItemDictionary.Add(fieldTextBox, item);
                        fieldTextBox.Focus();
                    }
                    else
                    {
                        //if (item.Type.Equals("YesNo"))
                        //{
                        //    ComboBox cbxYesNo = new ComboBox();
                        //    Configuration config = Configuration.GetNewInstance();
                        //    cbxYesNo.Items.AddRange(new object[] { config.Settings.RepresentationOfYes, config.Settings.RepresentationOfNo, config.Settings.RepresentationOfMissing });
                        //    cbxYesNo.Location = new System.Drawing.Point(300, textLinePosition);
                        //    cbxYesNo.Size = new System.Drawing.Size(286, 20);
                        //    cbxYesNo.BringToFront();
                        //    cbxYesNo.Leave += new EventHandler(fieldTextBox_Leave);
                        //    splitContainer1.Panel2.Controls.Add(cbxYesNo);
                        //    textLinePosition += 38;

                        //    myControlDictionary.Add(item.Name.ToString(), cbxYesNo);
                        //    myControlItemDictionary.Add(cbxYesNo, item);
                        //    cbxYesNo.Focus();
                        //}                    
                        //else
                        {
                            ////add textbox to panel            
                            TextBox fieldTextBox = new TextBox();
                            fieldTextBox.Name = item.Name;
                            fieldTextBox.Location = new System.Drawing.Point(300, textLinePosition);
                            fieldTextBox.Size = new System.Drawing.Size(286, 20);
                            fieldTextBox.BringToFront();
                            fieldTextBox.Leave += new EventHandler(fieldTextBox_Leave);
                            splitContainer1.Panel2.Controls.Add(fieldTextBox);
                            textLinePosition += 38;

                            myControlDictionary.Add(item.Name.ToString(), fieldTextBox);

                            myControlItemDictionary.Add(fieldTextBox, item);
                            fieldTextBox.Focus();
                        }
                    }

                    //add field name label to panel
                    Label fieldName = new Label();
                    fieldName.Tag = item.Name.ToString().ToUpperInvariant();
                    fieldName.Location = new System.Drawing.Point(26, labelLinePosition);
                    fieldName.Size = new System.Drawing.Size(35, 13);
                    fieldName.AutoSize = true;
                    fieldName.FlatStyle = FlatStyle.System;
                    fieldName.Text = item.Name.ToString();
                    splitContainer1.Panel2.Controls.Add(fieldName);
                    labelLinePosition += 38;

                    //add pattern label to panel
                    Label lblPattern = new Label();
                    lblPattern.Tag = item.Name.ToString().ToUpperInvariant();
                    lblPattern.Location = new System.Drawing.Point(620, patternLinePosition);
                    lblPattern.Size = new System.Drawing.Size(35, 13);
                    lblPattern.AutoSize = true;
                    lblPattern.FlatStyle = FlatStyle.System;
                    splitContainer1.Panel2.Controls.Add(lblPattern);
                    patternLinePosition += 38;
                    if (!string.IsNullOrEmpty(item.Pattern))
                    {
                        lblPattern.Text = item.Pattern.ToString();
                    }

                    //myControlDictionary.Add(item.Name.ToString(), fieldTextBox);
                    //myControlItemDictionary.Add(fieldTextBox, item);
                }
            
        }   

        /// <summary>
        /// Resets the Find Records form
        /// </summary>
        private void ResetForm()
        {
            ResetYCoordinates();
            ClearAll();
        }

        /// <summary>
        /// Clears all collections and controls
        /// </summary>
        private void ClearAll()
        {
            myControlDictionary.Clear();
            splitContainer1.Panel2.Controls.Clear();
        }

        /// <summary>
        /// Sets the initial control positions for y-coordinates
        /// </summary>
        private void ResetYCoordinates()
        {
            labelLinePosition = 31;
            textLinePosition = 25;
            patternLinePosition = 31;
        }

        /// <summary>
        /// Add fields to search fields collection
        /// </summary>
        private void AddFieldsToCollection()
        {
            if (lbxSearchFields.SelectedItems.Count > 0)
            {
                DisableEnableSearchFeatures(true);

                foreach (SearchListBoxItem item in lbxSearchFields.SelectedItems)
                {
                    //Only add the item to the collection if it does not already exist
                    if (!searchFieldsCollection.Contains(item))
                    {
                        if (lbxSearchFields.SelectedItems.Count <= 6)
                        {
                            searchFieldsCollection.Add(item);
                            myControlDictionaryData.Add(item.Name, ""); 
                        }
                        else
                        {
                            lbxSearchFields.SetSelected(lbxSearchFields.Items.IndexOf(item), false);   //unselect current index   
                            MsgBox.ShowError(SharedStrings.SEARCH_LIMIT_EXCEEDED);
                            return;
                        }
                    }
                }
            }
            else
            {
                DisableEnableSearchFeatures(false);
            }
        }

        /// <summary>
        /// Enable or disable search features
        /// </summary>
        /// <param name="enable">True or false to enable/disable search features</param>
        private void DisableEnableSearchFeatures(bool enable)
        {
            searchToolStripMenuItem.Enabled = enable;
            toolStripSearchButton.Enabled = enable;
        }

        /// <summary>
        /// Removes deselected fields from collection
        /// </summary>
        private void RemoveDeselectedFields()
        {
            foreach (SearchListBoxItem item in lbxSearchFields.Items)
            {
                if (myControlDictionary.ContainsKey(item.Name) && lbxSearchFields.GetSelected(lbxSearchFields.Items.IndexOf(item)) == false)
                {
                    myControlItemDictionary.Remove(myControlDictionary[item.Name.ToString()]);
                    myControlDictionary.Remove(item.Name.ToString());
                    myControlDictionaryData.Remove(item.Name.ToString());
                    searchFieldsCollection.Remove(item);
                }
            }
        }

        /// <summary>
        /// Sets the data entered
        /// </summary>
        /// <param name="control">The control whose data is be saved</param>
        private void SetFieldData(Control control)
        {
            #region Input Validation
            if (control == null)
            {
                throw new ArgumentNullException("Control");
            }
            #endregion  //Input Validation

            SearchListBoxItem item = GetAssociatedItem(control);
            if (control is TextBox || control is RichTextBox || control is MaskedTextBox)
            {
                if (!String.IsNullOrEmpty(control.Text.Trim()))
                {
                    item.ItemValue = control.Text.Trim();
                }
                else
                {
                    item.ItemValue = string.Empty;
                }
            }
            else if (control is ComboBox)
            {
                if (((ComboBox)control).SelectedItem != null)
                {
                    item.ItemValue = (string)((ComboBox)control).SelectedItem;
                }
            }
        }

        /// <summary>
        /// Returns the associated item to a given control
        /// </summary>
        /// <param name="control">The control needed to get it's associated item</param>
        /// <returns>The item associated with the control</returns>
        private SearchListBoxItem GetAssociatedItem(Control control)
        {
            #region Input Validation
            if (control == null)
            {
                throw new ArgumentNullException("Control");
            }
            #endregion  //Input Validation

            return myControlItemDictionary[control];
        }

        /// <summary>
        /// Builds the collections that will contain search fields and their values
        /// </summary>
        private bool BuildParameterCollections()
        {
            searchFields = new Collection<string>();
            searchFieldValues = new ArrayList();
            searchFieldItemTypes = new Collection<string>();
            comparisonTypes = new Collection<string>();
            OrFieldCount = new Dictionary<string, int>();
            string[] orValues = null;

            foreach (SearchListBoxItem item in searchFieldsCollection)
            {
                int i;

                if (item.Type.Equals("Number") || item.Type.Equals("Option")) // Option fields are numeric
                {
                    if (string.IsNullOrEmpty(item.ItemValue))
                    {
                        if (OrFieldCount.ContainsKey(item.Name.ToLowerInvariant()))
                        {
                            OrFieldCount[item.Name.ToLowerInvariant()]++;
                        }
                        else
                        {
                            OrFieldCount.Add(item.Name.ToLowerInvariant(), 1);
                        }

                        searchFields.Add(item.Name);
                        searchFieldItemTypes.Add(item.Type);
                        searchFieldValues.Add(null);
                        comparisonTypes.Add("");
                    }
                    else
                    {
                        orValues = item.ItemValue.Split(new[] {" or "," OR "," Or ", " oR "}, StringSplitOptions.RemoveEmptyEntries);
                        for (i = 0; i < orValues.Length; i++)
                        {
                            if (OrFieldCount.ContainsKey(item.Name.ToLowerInvariant()))
                            {
                                OrFieldCount[item.Name.ToLowerInvariant()]++;
                            }
                            else
                            {
                                OrFieldCount.Add(item.Name.ToLowerInvariant(), 1);
                            }
                            try
                            {
                                searchFields.Add(item.Name);
                                searchFieldItemTypes.Add(item.Type);                               
                                searchFieldValues.Add(Double.Parse(this.ParseFieldValue(orValues[i])));
                                comparisonTypes.Add(this.ParseComparisonType(orValues[i]));
                            }
                            catch (FormatException)
                            {
                                MsgBox.Show(string.Format(SharedStrings.INVALID_SEARCH_QUERY_NUMERIC, item.Name), SharedStrings.ENTER);
                                return false;
                            }
                        }
                    }
                }
                else if (item.Type.Equals("Date"))
                {                    
                    if (string.IsNullOrEmpty(item.ItemValue))
                    {
                        if (OrFieldCount.ContainsKey(item.Name.ToLowerInvariant()))
                        {
                            OrFieldCount[item.Name.ToLowerInvariant()]++;
                        }
                        else
                        {
                            OrFieldCount.Add(item.Name.ToLowerInvariant(), 1);
                        }

                        searchFields.Add(item.Name);
                        searchFieldItemTypes.Add(item.Type);
                        searchFieldValues.Add(null);
                        comparisonTypes.Add("");
                    }
                    else
                    {
                        orValues = item.ItemValue.Split(new[] { " or ", " OR ", " Or ", " oR " }, StringSplitOptions.RemoveEmptyEntries);                       
                        for (i = 0; i < orValues.Length; i++)
                        {
                            if (OrFieldCount.ContainsKey(item.Name.ToLowerInvariant()))
                            {
                                OrFieldCount[item.Name.ToLowerInvariant()]++;
                            }
                            else
                            {
                                OrFieldCount.Add(item.Name.ToLowerInvariant(), 1);
                            }
                            try
                            {                              
                                comparisonTypes.Add(this.ParseComparisonType(orValues[i]));                                                                
                                DateTime dateTime = DateTime.Parse(this.ParseFieldValue(orValues[i]));
                                searchFields.Add(item.Name);
                                searchFieldItemTypes.Add(item.Type);
                                searchFieldValues.Add(dateTime);                                                                
                            }
                            catch (System.Exception)
                            {
                                MsgBox.Show(string.Format(SharedStrings.INVALID_SEARCH_QUERY_DATE, item.Name), SharedStrings.ENTER);
                                return false;
                            }
                        }
                    }
                }
                else if (item.Type.Equals("Checkbox"))
                {
                    if (string.IsNullOrEmpty(item.ItemValue))
                    {
                        MsgBox.Show(string.Format(SharedStrings.INVALID_SEARCH_QUERY_CHECKBOX, item.Name), SharedStrings.ENTER);
                        return false;
                    }
                    else
                    {
                        orValues = item.ItemValue.Split(new[] { " or ", " OR ", " Or ", " oR " }, StringSplitOptions.RemoveEmptyEntries);
                        for (i = 0; i < orValues.Length; i++)
                        {
                            if (OrFieldCount.ContainsKey(item.Name.ToLowerInvariant()))
                            {
                                OrFieldCount[item.Name.ToLowerInvariant()]++;
                            }
                            else
                            {
                                OrFieldCount.Add(item.Name.ToLowerInvariant(), 1);
                            }
                            try
                            {
                                searchFields.Add(item.Name);
                                searchFieldItemTypes.Add(item.Type);
                                
                                string value = orValues[i];
                                Configuration config = Configuration.GetNewInstance();

                                if (value != config.Settings.RepresentationOfYes && value != config.Settings.RepresentationOfNo && value != "(+)" && value != "(-)")
                                {
                                    throw new FormatException();
                                }

                                searchFieldValues.Add(this.ParseFieldValue(orValues[i]));
                                comparisonTypes.Add(this.ParseComparisonType(orValues[i]));
                            }
                            catch (FormatException)
                            {
                                MsgBox.Show(string.Format(SharedStrings.INVALID_SEARCH_QUERY_CHECKBOX, item.Name), SharedStrings.ENTER);
                                return false;
                            }
                        }
                    }
                }
                else if (item.Type.Equals("YesNo"))
                {
                    if (string.IsNullOrEmpty(item.ItemValue))
                    {
                        if (OrFieldCount.ContainsKey(item.Name.ToLowerInvariant()))
                        {
                            OrFieldCount[item.Name.ToLowerInvariant()]++;
                        }
                        else
                        {
                            OrFieldCount.Add(item.Name.ToLowerInvariant(), 1);
                        }

                        searchFields.Add(item.Name);
                        searchFieldItemTypes.Add(item.Type);
                        searchFieldValues.Add(null);
                        comparisonTypes.Add("");
                    }
                    else
                    {
                        orValues = item.ItemValue.Split(new[] { " or ", " OR ", " Or ", " oR " }, StringSplitOptions.RemoveEmptyEntries);
                        for (i = 0; i < orValues.Length; i++)
                        {
                            if (OrFieldCount.ContainsKey(item.Name.ToLowerInvariant()))
                            {
                                OrFieldCount[item.Name.ToLowerInvariant()]++;
                            }
                            else
                            {
                                OrFieldCount.Add(item.Name.ToLowerInvariant(), 1);
                            }
                            try
                            {
                                searchFields.Add(item.Name);
                                searchFieldItemTypes.Add(item.Type);

                                string value = orValues[i];
                                Configuration config = Configuration.GetNewInstance();

                                if (value != config.Settings.RepresentationOfYes && value != config.Settings.RepresentationOfNo && value != config.Settings.RepresentationOfMissing && value != "(+)" && value != "(-)" && value != "(.)")
                                {
                                    throw new FormatException();
                                }

                                searchFieldValues.Add(this.ParseFieldValue(orValues[i]));
                                comparisonTypes.Add(this.ParseComparisonType(orValues[i]));
                            }
                            catch (FormatException)
                            {
                                MsgBox.Show(string.Format(SharedStrings.INVALID_SEARCH_QUERY_YESNO, item.Name), SharedStrings.ENTER);
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    orValues = item.ItemValue.Split(new[] { " or ", " OR ", " Or ", " oR " }, StringSplitOptions.RemoveEmptyEntries);

                    try
                    {
                        if (orValues.Length <= 0)
                        {
                            throw new FormatException();
                        }
                        for (i = 0; i < orValues.Length; i++)
                        {
                            if (OrFieldCount.ContainsKey(item.Name.ToLowerInvariant()))
                            {
                                OrFieldCount[item.Name.ToLowerInvariant()]++;
                            }
                            else
                            {
                                OrFieldCount.Add(item.Name.ToLowerInvariant(), 1);
                            }
                            searchFields.Add(item.Name);
                            searchFieldItemTypes.Add(item.Type);
                            //searchFieldValues.Add(Double.Parse(this.ParseFieldValue(item.ItemValue)));
                            //comparisonTypes.Add(this.ParseComparisonType(item.ItemValue));
                            searchFieldValues.Add(this.ParseFieldValue(orValues[i]));
                            comparisonTypes.Add(this.ParseComparisonType(orValues[i]));
                        }
                    }
                    catch (FormatException)
                    {
                        MsgBox.Show(string.Format(SharedStrings.INVALID_SEARCH_QUERY, item.Name), SharedStrings.ENTER);
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Builds data grid to display search records, if any are found
        /// </summary>
        private void BuildDataGrid()
        {
            if (!splitContainer1.Panel2.Controls.Contains(dataGrid1))
            {
                dataGrid1 = new DataGrid();                
                dataGrid1.Size = new System.Drawing.Size(648, 261);
                dataGrid1.Location = new Point(-1, 327);
                dataGrid1.Click += new EventHandler(dataGrid1_Click);
                dataGrid1.DoubleClick += new System.EventHandler(dataGrid1_DoubleClick);
                dataGrid1.Dock = DockStyle.Bottom;
                dataGrid1.ReadOnly = true;          //Removes blank line from appearing at the bottom of the data grid
                // DEFECT# 265 EI3 had a tooltip text on the datagrid.
                toolTip1.SetToolTip(dataGrid1, "Doubleclick row indicator in leftmost column to select record");
            }
            else
            {
                splitContainer1.Panel2.Controls.Remove(dataGrid1);
            }

            Project project = this.view.GetProject();

            bool tableExists = project.CollectedData.TableExists(this.view.TableName);

            if (tableExists)
            {
                dataGrid1.DataSource = project.CollectedData.GetSearchRecords(this.view, OrFieldCount, searchFields, searchFieldItemTypes, comparisonTypes, searchFieldValues);

                data = (DataTable)dataGrid1.DataSource;

                if (data != null && data.Rows.Count > 0)
                {
                    for (int i = 0; i <= data.Columns.Count - 1; i++)
                    {
                        //hide the "Unique Key" and "RecStatus" columns                    
                        if (data.Columns[i].ColumnName == "UniqueKey" || data.Columns[i].ColumnName == "RecStatus")
                        {
                            data.Columns[i].ColumnMapping = MappingType.Hidden;
                        }
                    }
                    splitContainer1.Panel2.Controls.Add(dataGrid1);
                }
                else
                {
                    MsgBox.ShowInformation(SharedStrings.SEARCH_RECORDS_NOT_FOUND);
                }
            }
            else
            {
                MsgBox.ShowInformation(string.Format(SharedStrings.DATA_TABLE_NOT_FOUND, this.view.Name));
            }
        }

        /// <summary>
        /// Parses a string to find out what comparison is being made.
        /// </summary>
        /// <param name="pValue">The comparison statement to be parsed</param>
        /// <returns>string representing the comparison type of the statement</returns>
        private string ParseComparisonType(string pValue)
        {
            if (pValue.StartsWith("<="))
            {
                return "<=";
            }

            if (pValue.StartsWith(">="))
            {
                return ">=";
            }

            if (pValue.StartsWith("<>"))
            {
                return "<>";
            }

            if (pValue.StartsWith(">"))
            {
                return ">";
            }

            if (pValue.StartsWith("<"))
            {
                return "<";
            }

            return "=";
        }
        
        /// <summary>
        /// Parses a string, removing any comparison operators and returning only data
        /// </summary>
        /// <param name="pValue">The comparison statement to be parsed</param>
        /// <returns>A string representing the value of the field</returns>
        private string ParseFieldValue(string pValue)
        {
            if (pValue.StartsWith(">="))
            {
                return pValue.Substring(2, pValue.Length - 2);
            }

            if (pValue.StartsWith("<="))
            {
                return pValue.Substring(2, pValue.Length - 2);
            }

            if (pValue.StartsWith("<>"))
            {
                return pValue.Substring(2, pValue.Length - 2);
            }

            if (pValue.StartsWith(">"))
            {
                return pValue.Substring(1, pValue.Length - 1);
            }

            if (pValue.StartsWith("<"))
            {
                return pValue.Substring(1, pValue.Length - 1);
            }

            return pValue;
        }

        #endregion  //Private Methods

        #region Private Events

        /// <summary>
        /// Loads the form
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event args.</param>
        private void FindRecords_Load(object sender, EventArgs e)
        {
            BuildControlsList();
            lbxSearchFields.SelectedIndex = -1;
            // DEFECT# 265 The ToolTip control will activate only once if only one control is associated with it.
            toolTip1.SetToolTip(this.lbxSearchFields, "Choose Search Fields");
        }

        /// <summary>
        /// Handles the Double Click event on the data grid
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void dataGrid1_DoubleClick(object sender, System.EventArgs e)
        {
            int currentRow = dataGrid1.CurrentRowIndex;
            string record = data.Rows[currentRow]["UniqueKey"].ToString();      //gets the record Id
            this.RecordId = record;                                         //set the record Id in the Enter Main Form            
            globalRecordId = new Guid(data.Rows[currentRow]["GlobalRecordId"].ToString());      //gets the record Id
            this.DialogResult = DialogResult.OK;
        }

        private string recordId;
        private Guid globalRecordId;

        public string RecordId
        {
            get
            {
                return recordId;
            }
            set
            {
                recordId = value;
            }
        }

        public Guid GlobalRecordId
        {
            get
            {
                return globalRecordId;
            }
            set
            {
                globalRecordId = value;
            }
        }

        /// <summary>
        /// Handles the Click event on the data grid
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void dataGrid1_Click(object sender, System.EventArgs e)
        {
            int currentRow = dataGrid1.CurrentRowIndex;
            dataGrid1.Select(currentRow);                   //Select the entire row
        }

        /// <summary>
        /// Handles the Leave event of the text box once data is entered
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void fieldTextBox_Leave(object sender, EventArgs e)
        {
            SetFieldData((Control)sender);
        }

        /// <summary>
        /// Handles the Mouse Down event
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event args.</param>
        private void lbxSearchFields_MouseDown(object sender, MouseEventArgs e)
        {
            //AddFieldsToCollection();
            //RemoveDeselectedFields();
            //AddControlsToPanel();
        }

        /// <summary>
        /// Handles the Click event of the Reset button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BuildControlsList();
            ResetYCoordinates();
            ClearAll();
            searchFieldsCollection.Clear();
            this.myControlDictionaryData.Clear();
            DisableEnableSearchFeatures(false);
        }

        /// <summary>
        /// Handles the Click event of the Exit button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void exitFindRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Handles the Click event of the Search button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (BuildParameterCollections())
            {
                BuildDataGrid();
            }
        }

        /// <summary>
        /// Handles the Click event of the contents menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void contentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            base.DisplayFeatureNotImplementedMessage();
        }

        /// <summary>
        /// Handles the Click event of the command reference menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void commandReferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            base.DisplayFeatureNotImplementedMessage();
        }

        /// <summary>
        /// Handles the Click event of the About Epi Info menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void aboutEpiInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //OnAboutClicked();
        }

        /// <summary>
        /// Handles the Click event of the tool strip
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            toolStrip1.Focus();
        }

        /// <summary>
        /// Handles the Click event of the menu strip
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void menuStrip2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            menuStrip2.Focus();
        }

        ///// <summary>
        ///// Handles the event if the user inputs invalid characters in masked text boxes
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">Mask input rejected supplied event parameters</param>
        //private void maskedTextBox_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        //{
        //    Field field = GetAssociatedField((Control)sender);
        //    if (field is DateTimeField || field is TimeField || field is DateField)
        //    {
        //        if (((Control)sender).Text != string.Empty)
        //        {
        //            //if ((((MaskedTextBox)((Control)sender)).MaskFull) && e.Position != ((MaskedTextBox)((Control)(sender))).Mask.Length)
        //            if ((((MaskedTextBox)((Control)sender)).MaskFull))
        //            {
        //                Assembly assembly = Assembly.GetExecutingAssembly();
        //                CultureInfo ci = assembly.GetName().CultureInfo;
        //                DateTimeFormatInfo dateTimeInfo = new DateTimeFormatInfo();

        //                ((MaskedTextBox)((Control)sender)).TextMaskFormat = MaskFormat.IncludePromptAndLiterals;
        //                string format = AppData.Instance.DataPatternsDataTable.GetExpressionByMask(((MaskedTextBox)((Control)sender)).Mask.ToString(), ((IPatternable)field).Pattern);

        //                bool result;
        //                DateTime dateOrTime;
        //                result = DateTime.TryParseExact(((MaskedTextBox)((Control)sender)).Text, format, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out dateOrTime);
        //                if (!result)
        //                {
        //                    if (field is DateTimeField)
        //                    {
        //                        MsgBox.ShowError(SharedStrings.ENTER_VALID_DATE_AND_TIME);
        //                    }
        //                    else if (field is DateField)
        //                    {
        //                        MsgBox.ShowError(SharedStrings.ENTER_VALID_DATE);
        //                    }
        //                    else if (field is TimeField)
        //                    {
        //                        MsgBox.ShowError(SharedStrings.ENTER_VALID_TIME);
        //                    }
        //                    ((Control)sender).Text = string.Empty;
        //                    ((Control)sender).Focus();
        //                }
        //            }
        //            else
        //            {
        //                ((MaskedTextBox)((Control)sender)).Focus();
        //            }
        //        }
        //    }
        //    else if (field is PhoneNumberField)
        //    {
        //        if (((Control)sender).Text != string.Empty)
        //        {
        //            if ((((MaskedTextBox)((Control)sender)).MaskFull) && e.Position != ((MaskedTextBox)((Control)(sender))).Mask.Length)
        //            //                    if ((((MaskedTextBox)((Control)sender)).MaskFull)
        //            {
        //                MsgBox.ShowError(SharedStrings.ENTER_VALID_PHONE_NUMBER);
        //                ((Control)sender).Text = string.Empty;
        //                ((Control)sender).Focus();
        //            }
        //        }
        //    }
        //    else if (field is NumberField)
        //    {
        //        if (((Control)sender).Text != string.Empty)
        //        {
        //            if ((((MaskedTextBox)((Control)sender)).MaskFull) && e.Position != ((MaskedTextBox)((Control)(sender))).Mask.Length)
        //            //if ((((MaskedTextBox)((Control)sender)).MaskFull))
        //            {
        //                MsgBox.ShowError(SharedStrings.ENTER_VALID_NUMBER);
        //                ((Control)sender).Text = string.Empty;
        //                ((Control)sender).Focus();
        //            }
        //        }
        //    }
        //}


        private void lbxSearchFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (Control control in splitContainer1.Panel2.Controls)
            {
                if (myControlDictionaryData.ContainsKey(control.Name))
                {
                    myControlDictionaryData[control.Name] = control.Text;

                }
            }

            AddFieldsToCollection();
            RemoveDeselectedFields();
            AddControlsToPanel();
        }


        #endregion  //Private Events

    }
}
