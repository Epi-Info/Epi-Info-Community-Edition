using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;
using Epi.Fields;
using Epi.Windows;
using Epi.Windows.Controls;
using Epi.Windows.Enter.PresentationLogic;
using Epi.Data.Services;

namespace Epi.Windows.Enter
{
    public partial class Canvas : Epi.Windows.Docking.DockWindow
    {
        /*
        public event EventHandler ControlClickEvent;
        public event EventHandler ControlEnterEvent;
        public event KeyEventHandler ControlKeyDownEvent;
        public event PreviewKeyDownEventHandler ControlPreviewKeyDownEvent;*/

        #region Public Properties

        public Control control
        {
            get;
            set;
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Sets the data for text fields
        /// </summary>        
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        private void SetTextData(Field field, Control control)
        {
            if (control is TextBoxBase)
            {
                if (field is Epi.Fields.TextField)
                {
                    Epi.Fields.TextField textField = (Epi.Fields.TextField)field;

                    if (control.Text.Trim().Length <= textField.MaxLength || textField.MaxLength <= 0)
                    {
                        textField.CurrentRecordValueObject = control.Text.Trim();
                    }
                    else
                    {
                        string value = control.Text.Trim();
                        if (textField.CurrentRecordValue != null)
                        {
                            control.Text = textField.CurrentRecordValue;
                        }
                        else
                        {
                            control.Text = "";
                        }
                        throw new System.Exception(string.Format("Value {0}: exceeds maximum Length of {1} for field [{2}].", value, textField.MaxLength, textField.Name));
                    }
                }
                else
                {
                    IDataField ID = (IDataField)field;
                    ID.CurrentRecordValueObject = control.Text.Trim();
                }
            }
        }
        /// <summary>
        /// Sets the data for number and phone number fields
        /// </summary>        
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        private void SetNumberData(Field field, Control control)
        {
            ((MaskedTextBox)control).TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
            if (!string.IsNullOrEmpty(((MaskedTextBox)control).Text))
            {
                ((MaskedTextBox)control).TextMaskFormat = MaskFormat.IncludeLiterals;
                ((IDataField)field).CurrentRecordValueObject = ((MaskedTextBox)control).Text;
            }
            else
            {
                ((IDataField)field).CurrentRecordValueObject = null;
            }
        }

        /// <summary>
        /// Gets the data for text fields
        /// </summary>
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        public void GetTextData(Field field, Control control)
        {
            if (((IDataField)field).CurrentRecordValueString.Equals(string.Empty) && field is GUIDField)
            {
                control.Text = ((GUIDField)field).NewGuid().ToString();
                SetTextData(field, control);
            }
            else
            {
                control.Text = ((IDataField)field).CurrentRecordValueString;
            }
        }

        public void InitialMask(DateTimeField dateTimeField, Control control)
        {
            string pattern = string.Empty;
            string valueString = string.Empty;

            valueString = dateTimeField.CurrentRecordValueString;

            if (valueString == "")
            {
                control.Text = dateTimeField.Watermark;
                control.ForeColor = Color.Gray;
                ((TextBoxBase)control).SelectionLength = 0;
                ((TextBoxBase)control).SelectionStart = 0;
            }
            else
            {
                control.Text = valueString;
                control.ForeColor = Color.Black;
            }
        }

        /// <summary>
        /// Gets the data for date fields
        /// </summary>
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        public void GetDateData(DateField field, Control control)
        {
            control.Text = field.CurrentRecordValueString;
            InitialMask(field, control);
        }

        /// <summary>
        /// Gets the data for time fields
        /// </summary>
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        public void GetTimeData(TimeField field, Control control)
        {
            control.Text = field.CurrentRecordValueString;
            InitialMask(field, control);
        }

        /// <summary>
        /// Gets the data for date time fieldsGetTimeData
        /// </summary>
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        public void GetDateTimeData(DateTimeField field, Control control)
        {
            control.Text = field.CurrentRecordValueString;
            InitialMask(field, control);
        }

        /// <summary>
        /// Gets data for option fields
        /// </summary>
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        public void GetOptionData(OptionField field, Control control)
        {
            if (Util.IsEmpty(field.CurrentRecordValueObject))
            {   //no data, so clear all radiobuttons
                foreach (var item in control.Controls)
                {
                    ((RadioButton)item).Checked = false;
                }
            }
            else
            {
                if (!Util.IsEmpty(field.CurrentRecordValue))
                {
                    ((RadioButton)(control.Controls[Convert.ToInt16(field.CurrentRecordValue)])).Checked = true;
                }
            }
        }

        /// <summary>
        /// Gets the data for Mirror fields.
        /// </summary>
        /// <param name="mirrorField">The field that mirrors data of another field.</param>
        /// <param name="control">The control associated with the field</param>
        public void GetMirrorData(MirrorField mirrorField, Control control)
        {
            if (mirrorField.SourceField == null)
            {
                control.Text = string.Empty;
            }
            else
            {
                control.Text = ((IMirrorable)(mirrorField.SourceField)).GetReflectedValue();
            }
        }


        /// <summary>
        /// Gets the data for combobox items
        /// </summary>
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        public void GetComboboxData(Field field, Control control)
        {
            if (string.IsNullOrEmpty(((IDataField)field).CurrentRecordValueString))
            {
                ((ComboBox)control).SelectedIndex = -1;
                ((ComboBox)control).Text = string.Empty;
            }
            else
            {
                if (field is YesNoField)
                {
                    int num;
                    bool isNum = int.TryParse(((IDataField)field).CurrentRecordValueString, out num);
                    if (isNum)      //Value is a number
                    {
                        if (int.Parse(((IDataField)field).CurrentRecordValueString) == 1)    //Yes                                        
                        {
                            ((ComboBox)control).Text = config.Settings.RepresentationOfYes;
                        }
                        else //No
                        {
                            ((ComboBox)control).Text = config.Settings.RepresentationOfNo;
                        }
                    }
                    else
                    {
                        if (((IDataField)field).CurrentRecordValueString.Equals(config.Settings.RepresentationOfYes))
                        {
                            ((ComboBox)control).Text = config.Settings.RepresentationOfYes;
                        }
                        else if (((IDataField)field).CurrentRecordValueString.Equals(config.Settings.RepresentationOfNo))
                        {
                            ((ComboBox)control).Text = config.Settings.RepresentationOfNo;
                        }
                    }
                }
                else
                {
                    string val = ((IDataField)field).CurrentRecordValueString;

                    if (field is DDLFieldOfCommentLegal)
                    {
                        foreach (object item in ((ComboBox)control).Items)
                        {
                            if (item is String)
                            {
                                string[] parts = ((String)item).Split('-');
                                if (val == parts[0].Trim())
                                {
                                    ((ComboBox)control).Text = ((String)item);
                                    ((ComboBox)control).SelectedIndex = ((ComboBox)control).FindString(((String)item));
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        ((ComboBox)control).Text = val;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the data for combo box items
        /// </summary>
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        public void GetCheckBoxData(Field field, Control control)
        {
            if (((IDataField)field).CurrentRecordValueObject == null)
            {
                ((CheckBox)control).Checked = false;
            }
            else
            {
                bool checkedValue = false;
                bool.TryParse(((IDataField)field).CurrentRecordValueObject.ToString(), out checkedValue);
                //bool checkedValue = false;
                //bool.TryParse(((IDataField)field).CurrentRecordValueString, out checkedValue);
                ((CheckBox)control).Checked = checkedValue;
            }
        }

        /// <summary>
        /// Gets the data for data grid items
        /// </summary>
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        public void GetDataGridViewData(Field field, Control control)
        {
            if (field is GridField)
            {
                GridField gridField = (GridField)field;
                if (Util.IsEmpty((gridField).DataSource))
                {
                    DataTable dataTable = currentView.GetProject().CollectedData.GetGridTableData(currentView, gridField);
                    dataTable.TableName = currentView.Name + gridField.Name;
                    gridField.DataSource = dataTable;
                }
                ((DataGridView)control).DataSource = gridField.DataSource;
            }
        }

        /// <summary>
        /// Get the image Field Data into the control
        /// </summary>
        /// <param name="field">Image field</param>
        /// <param name="control">Picture Box</param>
        public void GetFieldDataIntoControl(ImageField field, PictureBox control)
        {
            if (field.CurrentRecordValueObject is byte[])
            {
                MemoryStream memStream = new MemoryStream();
                byte[] imageAsBytes = (byte[])field.CurrentRecordValueObject;
                memStream.Write(imageAsBytes, 0, imageAsBytes.Length);
                ((PictureBox)control).Image = Image.FromStream(memStream);
            }
            else
            {
                ((PictureBox)control).Image = null;
            }
        }

        /// <summary>
        /// Get the YesNo Field Data into the control
        /// </summary>
        /// <param name="field">YesNo Field</param>
        /// <param name="control">ComboBox</param>
        public void GetFieldDataIntoControl(YesNoField field, ComboBox control)
        {
            if (field.CurrentRecordValueObject == null || field.CurrentRecordValueObject == string.Empty)
            {
                control.SelectedIndex = -1;
            }
            else
            {
                control.SelectedValue = field.CurrentRecordValueObject;
            }
        }

        /// <summary>
        /// Sets controls' properties
        /// </summary>
        /// <param name="control">The control being added to the panel</param>
        private void SetControlProperties(Control control)
        {
            control.BringToFront();

            ControlFactory factory = ControlFactory.Instance;
            Field field = factory.GetAssociatedField(control);

            if (field is GUIDField)
            {
                control.TabStop = false;
            }
            
            if (control is TextBox)
            {
                ((TextBox)control).AcceptsReturn = true;
                ((TextBox)control).AcceptsTab = true;
            }
            else if (control is MaskedTextBox)
            {
                ((MaskedTextBox)control).AcceptsTab = true;
            }
        }

        /// <summary>
        /// Sets event handlers for controls added to canvas
        /// </summary>        
        internal void SubscribeControlEventHandlers()
        {
            foreach (Panel panel in Panels)
            {
                foreach (Control control in panel.Controls)
                {
                    if (control is Label) continue;
                    
                    ControlFactory factory = ControlFactory.Instance;
                    Field field = factory.GetAssociatedField(control);

                    control.Enter += new EventHandler(control_Enter);
                    control.KeyDown += new KeyEventHandler(control_KeyDown);
                    control.TextChanged += new EventHandler(control_TextChanged);
                    control.PreviewKeyDown += new PreviewKeyDownEventHandler(control_PreviewKeyDown);
                    control.Validating += new CancelEventHandler(control_Validating);
                    
                    if (
                        field is RelatedViewField
                        || field is CheckBoxField
                        || field is GridField
                        || field is CommandButtonField
                        )
                    {
                        control.Click += new EventHandler(control_Click);
                    }

                    if ( field is ImageField )
                    {
                        control.MouseHover += new EventHandler(image_MouseHover);
                        control.MouseLeave += new EventHandler(image_MouseLeave);
                        control.MouseDown += new MouseEventHandler(image_MouseDown);
                        control.MouseUp += new MouseEventHandler(image_MouseUp);
                        control.MouseMove += new MouseEventHandler(image_MouseMove);
                        control.Click += new EventHandler(image_Click);
                    }

                    if (field is CheckBoxField)
                    {
                        ((CheckBox)control).CheckedChanged += new EventHandler(control_CheckChanged);
                    }

                    if (field is OptionField)
                    {
                        GroupBox groupBox = ((GroupBox)control);
                        foreach (Control childControl in groupBox.Controls)
                        {
                            if (childControl is RadioButton)
                            {
                                ((RadioButton)childControl).CheckedChanged += new EventHandler(control_CheckChanged);
                                ((RadioButton)childControl).KeyDown += new KeyEventHandler(control_KeyDown);
                                ((RadioButton)childControl).Click += new EventHandler(control_Click);
                            }
                        }
                    }

                    if (field is IPatternable)
                    {
                        control.Click += new EventHandler(control_Click);
                    }

                    if (field is UpperCaseTextField || field is NumberField)
                    {
                        control.KeyPress += new KeyPressEventHandler(control_KeyPress);
                    }

                    if ((field is DateField || field is DateTimeField) && !(field is TimeField))
                    {
                        control.MouseDown += new MouseEventHandler(control_MouseDown);
                    }

                    if (field is DateField || field is NumberField || field is DateTimeField ||
                        field is TimeField || field is DDListField || field is DDLFieldOfCodes)
                    {
                        control.Leave += new EventHandler(control_Leave);
                    }

                    if (control is ComboBox && field is TableBasedDropDownField)
                    {
                        ((ComboBox)control).SelectionChangeCommitted += new EventHandler(control_SelectionChangeCommitted);
                        ((ComboBox)control).MouseWheel += new MouseEventHandler(Canvas_MouseWheel);
                    }
                    else if (control is ComboBox && field is YesNoField)
                    {
                        ((ComboBox)control).SelectionChangeCommitted += new EventHandler(control_SelectionChangeCommitted);
                    }

                    if (control is DataGridView)
                    {
                        DataGridView dataGridView = (DataGridView)control;
                        dataGridView.DataError += new DataGridViewDataErrorEventHandler(control_DataError);
                        dataGridView.RowValidated += new DataGridViewCellEventHandler(control_RowValidated);
                        dataGridView.NewRowNeeded += new DataGridViewRowEventHandler(control_RowAdded);
                        dataGridView.CurrentCellChanged += new EventHandler(dataGridView_CurrentCellChanged);
                        dataGridView.CellValidating += new DataGridViewCellValidatingEventHandler(gridColumnControl_Validating);
                        dataGridView.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView_EditingControlShowing);
                        if (dataGridView.ContextMenuStrip != null)
                        {
                            dataGridView.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler(ContextMenuStrip_ItemClicked);
                        }
                    }
                }
            }

            this.IsEventEnabled = true;
        }

        void Canvas_MouseWheel(object sender, MouseEventArgs e)
        {
            ((System.Windows.Forms.HandledMouseEventArgs)(e)).Handled = true;
        }

        void dataGridView_CurrentCellChanged(object sender, EventArgs e)
        {
            #region '[v] Unique Field'
            if (((DataGridView)sender).CurrentCell is DataGridViewComboBoxCell)
            {
                DataGridViewComboBoxCell selectedCell = (DataGridViewComboBoxCell)((DataGridView)sender).CurrentCell;
                DataGridViewComboBoxColumn selectedColumn = (DataGridViewComboBoxColumn)(((DataGridView)sender).CurrentCell.OwningColumn);
                DataGridViewRow selectedRow = (DataGridViewRow)(((DataGridView)sender).CurrentCell.OwningRow);
                TableBasedDropDownColumn currentTableBasedGridField = (TableBasedDropDownColumn)(selectedColumn.Tag);
                
                int rowCount = ((DataGridView)sender).Rows.Count;
                bool isUniqueField = currentTableBasedGridField.IsUniqueField;

                if (rowCount > 1 && isUniqueField) 
                {
                    int currentColumnIndex = selectedColumn.Index;
                    int currentRowIndex = ((DataGridView)sender).CurrentRow.Index;

                    DataTable table = currentTableBasedGridField.GetSourceData();

                    List<string> itemsInGrid = new List<string>();

                    foreach (DataGridViewRow eachRow in ((DataGridView)sender).Rows)
                    {
                        if (currentRowIndex != eachRow.Index)
                        {
                            DataGridViewComboBoxCell gridViewRowCell = (DataGridViewComboBoxCell)eachRow.Cells[currentColumnIndex];
                            itemsInGrid.Add((string)gridViewRowCell.FormattedValue);
                        }
                    }

                    List<string> availableItems = new List<string>();
                    foreach (DataRow dataRow in table.Rows)
                    {
                        string candidateValue = dataRow.ItemArray[0].ToString();

                        if (itemsInGrid.Contains(candidateValue) == false)
                        {
                            availableItems.Add(candidateValue);
                        }
                    }

                    table.Rows.Clear();
                    DataRow newRow;
                    foreach (string item in availableItems)
                    {
                        newRow = table.NewRow();
                        newRow[0] = item;
                        table.Rows.Add(newRow);
                    }

                    selectedCell.DataSource = table;
                }
            }
            #endregion
        }

        void ContextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            DataGridView dataGridView = null;

            if (e.ClickedItem.Tag != null)
            {
                dataGridView = (DataGridView)(e.ClickedItem).Tag;
            }

            if (dataGridView != null)
            {
                DataGridViewSelectedRowCollection selectedRows = dataGridView.SelectedRows;

                foreach (DataGridViewRow row in selectedRows)
                {
                    if (((DataGridViewRow)row).IsNewRow == false)
                    {
                        dataGridView.Rows.Remove(row);
                    }
                }

                dataGridView.Refresh();
            }
        }

        internal void UnsubscribeControlEventHandlers()
        {
            this.IsEventEnabled = false;

            foreach (Panel panel in canvasPanel.Controls)
            {
                foreach (Control control in panel.Controls)
                {
                    if (control is Label) continue;
                    
                    ControlFactory factory = ControlFactory.Instance;
                    Field field = factory.GetAssociatedField(control);

                    control.Enter -= new EventHandler(control_Enter);
                    control.KeyDown -= new KeyEventHandler(control_KeyDown);
                    control.TextChanged -= new EventHandler(control_TextChanged);
                    control.PreviewKeyDown -= new PreviewKeyDownEventHandler(control_PreviewKeyDown);
                    control.Validating -= new CancelEventHandler(control_Validating);

                    if (field is RelatedViewField
                        || field is ImageField
                        || field is CheckBoxField
                        || field is GridField
                        || field is CommandButtonField
                        )
                    {
                        control.Click -= new EventHandler(control_Click);
                    }

                    if (field is CheckBoxField)
                    {
                        ((CheckBox)control).CheckedChanged -= new EventHandler(control_CheckChanged);
                    }

                    if (field is OptionField)
                    {
                        GroupBox groupBox = ((GroupBox)control);
                        foreach (Control childControl in groupBox.Controls)
                        {
                            if (childControl is RadioButton)
                            {
                                ((RadioButton)childControl).CheckedChanged -= new EventHandler(control_CheckChanged);
                                ((RadioButton)childControl).KeyDown -= new KeyEventHandler(control_KeyDown);
                                ((RadioButton)childControl).Click -= new EventHandler(control_Click);
                            }
                        }
                    }

                    if (field is IPatternable)
                    {
                        control.Click -= new EventHandler(control_Click);
                    }

                    if (field is DateField || field is DateTimeField)
                    {
                        control.MouseDown -= new MouseEventHandler(control_MouseDown);
                    }

                    if (field is DateField || field is TimeField || field is DateTimeField)
                    {
                        control.Leave -= new EventHandler(control_Leave);
                    }

                    if (field is UpperCaseTextField || field is NumberField)
                    {
                        control.KeyPress -= new KeyPressEventHandler(control_KeyPress);
                    }

                    if (field is NumberField || field is DDLFieldOfCodes || field is DDListField)
                    {
                        control.Leave -= new EventHandler(control_Leave);
                    }

                    if (control is ComboBox && field is TableBasedDropDownField)
                    {
                        if (field is DDListField)
                        {
                            DDListField tmp = (DDListField)field;
                            foreach (System.Collections.Generic.KeyValuePair<string, int> kvp in tmp.pairAssociated)
                            {
                                Field field2 = this.currentView.Fields[kvp.Key];
                                List<Control> control2 = factory.GetAssociatedControls(field2);

                                Epi.Windows.Enter.Controls.LegalValuesComboBox ctrl3 = (Epi.Windows.Enter.Controls.LegalValuesComboBox)control;


                                if (control2.Count > 1)
                                {
                                    control2[1].Leave -= new EventHandler(ctrl3.FilterList);
                                }
                                else
                                {
                                    control2[0].Leave -= new EventHandler(ctrl3.FilterList);
                                }
                            }
                        }
                        else
                        {
                            ((ComboBox)control).TextChanged -= new EventHandler(control_TextChanged);
                            ((ComboBox)control).SelectionChangeCommitted -= new EventHandler(control_SelectionChangeCommitted);
                            ((ComboBox)control).MouseWheel -= new MouseEventHandler(Canvas_MouseWheel);
                        }
                    }
                    else if (control is ComboBox && field is YesNoField)
                    {
                        ((ComboBox)control).SelectionChangeCommitted -= new EventHandler(control_SelectionChangeCommitted);
                    }

                    if (control is DataGridView)
                    {
                        DataGridView dataGridView = (DataGridView)control;
                        dataGridView.DataError -= new DataGridViewDataErrorEventHandler(control_DataError);
                        dataGridView.RowValidated -= new DataGridViewCellEventHandler(control_RowValidated);
                        dataGridView.NewRowNeeded -= new DataGridViewRowEventHandler(control_RowAdded);
                        dataGridView.CellValidating -= new DataGridViewCellValidatingEventHandler(gridColumnControl_Validating);

                        if (dataGridView.ContextMenuStrip != null)
                        {
                            dataGridView.ContextMenuStrip.ItemClicked -= new ToolStripItemClickedEventHandler(ContextMenuStrip_ItemClicked);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetControlFactoryFields()
        {
            // TODO: Do this better somehow
            Epi.Windows.Enter.PresentationLogic.ControlFactory.Instance.ResetControlFields();
        }

        /// <summary>
        /// Assigns values to all linked fields based on a code table.
        /// </summary>
        /// <param name="sender">The control that contains the link references</param>
        private bool AssignLinkedFieldValues(Control sender)
        {
            if (currentPanel == null || currentPanel.Controls == null || (sender is ComboBox) == false)
            {
                return false;
            }

            ControlFactory factory = ControlFactory.Instance;
            Field codesfield = factory.GetAssociatedField(sender);
            if (codesfield is DDLFieldOfCodes)
            {
                DataTable codeTable = ((TableBasedDropDownField)codesfield).CodeTable;

                bool isSenderSelected = false;
                string selectedText = string.Empty;
                object selectedItem = ((System.Windows.Forms.ComboBox)sender).SelectedItem;
                
                if (codeTable.Rows.Count == 0) return false;

                if (selectedItem == "" || selectedItem == null)
                {
                    isSenderSelected = false;
                    selectedText = "";
                }
                else
                {
                    isSenderSelected = true;
                    selectedText = ((ComboBox)sender).SelectedItem as String;
                }

                List<string> candidates = new List<string>();

                if (isSenderSelected)
                {
                    candidates.Add(selectedText);
                }
                else
                {
                    foreach (object candidate in ((ComboBox)sender).Items)
                    {
                        candidates.Add((String)candidate);
                    }
                }

                string predicate = ((DDLFieldOfCodes)codesfield).FieldName;
                String filterExpression = string.Empty;

                if (string.IsNullOrEmpty(selectedText))
                {
                    if (string.IsNullOrEmpty(((DDLFieldOfCodes)codesfield).CascadeFilter) == false)
                    {
                        filterExpression = ((DDLFieldOfCodes)codesfield).CascadeFilter;
                    }
                }
                else
                {
                    filterExpression = string.Format("[{0}] = '{1}'", predicate, selectedText.Replace("'", "''"));

                    if (string.IsNullOrEmpty(((DDLFieldOfCodes)codesfield).CascadeFilter) == false)
                    {
                        filterExpression = ((DDLFieldOfCodes)codesfield).CascadeFilter + " AND " + filterExpression;
                    }
                }

                DataRow[] dataRows = codeTable.Select(filterExpression);
                DataTable filteredTable = codeTable.Clone();

                if (dataRows.Length > 0)
                {
                    filteredTable = dataRows.CopyToDataTable();
                }

                string keyColumnName = ((Epi.Fields.DDLFieldOfCodes)codesfield).TextColumnName;
                Dictionary<string, int> pairAssociated = ((Epi.Fields.DDLFieldOfCodes)codesfield).PairAssociated;

                string targetFieldName = string.Empty;

                foreach(KeyValuePair<string, int> kvp in pairAssociated)
                {
                    targetFieldName = kvp.Key;

                    Field targetField = this.CurrentView.GetFieldById(kvp.Value);

                    if (targetField == null)
                    {
                        continue;
                    }

                    List<Control> targetControls = factory.GetAssociatedControls(targetField);

                    foreach (Control targetControlCandidate in targetControls)
                    {
                        if (targetControlCandidate is ComboBox)
                        {
                            ComboBox targetCombo = targetControlCandidate as ComboBox;
                            targetCombo.Text = string.Empty;

                            if (filteredTable != null && filteredTable.Columns.Contains(targetFieldName))
                            {
                                targetCombo.BeginUpdate();
                                targetCombo.DisplayMember = null;
                                targetCombo.Items.Clear();

                                foreach (DataRow row in filteredTable.Rows)
                                {
                                    if (targetCombo.Items.Contains(row[targetFieldName]) == false)
                                    {
                                        targetCombo.Items.Add(row[targetFieldName]);
                                    }
                                }

                                targetCombo.SelectedIndex = -1;
                                targetCombo.SelectedItem = null;
                                targetCombo.EndUpdate();

                                Field codesFieldCandidate = factory.GetAssociatedField(targetCombo);
                                
                                if (codesFieldCandidate is DDLFieldOfCodes)
                                {
                                    ((DDLFieldOfCodes)codesFieldCandidate).CascadeFilter = filterExpression;
                                }

                                AssignLinkedFieldValues(targetCombo);
                            }
                        }
                        else if (targetControlCandidate is TextBox)
                        {
                            if (isSenderSelected == false)
                            {
                                targetControlCandidate.Text = string.Empty;
                            }
                            else
                            {
                                if (filteredTable != null && filteredTable.Columns.Contains(targetFieldName))
                                {
                                    if (filteredTable.Rows.Count >= 1)
                                    {
                                        String valueString = filteredTable.Rows[0][targetFieldName].ToString();
                                        ((TextField)targetField).CurrentRecordValueString = valueString;
                                        targetControlCandidate.Text = ((TextField)targetField).CurrentRecordValueString;
                                    }
                                }
                            }
                        }
                        else if (targetField is Epi.Fields.LabelField && targetControlCandidate is TransparentLabel)
                        {
                            targetControlCandidate.Visible = false;

                            if (isSenderSelected == false)
                            {
                                targetControlCandidate.Text = string.Empty;
                            }
                            else
                            {
                                if (filteredTable != null && filteredTable.Columns.Contains(targetFieldName))
                                {
                                    if (filteredTable.Rows.Count >= 1)
                                    {
                                        String valueString = filteredTable.Rows[0][targetFieldName].ToString();
                                        targetControlCandidate.Text = valueString;
                                    }
                                }
                            }

                            targetControlCandidate.Visible = true;
                        }
                    }
                }
            }

            return true;
        }
        #endregion // Private Methods

        #region Public Methods
        /// <summary>
        /// Gets the field data value
        /// </summary>        
        public void Render(bool? enable=true)
        {
            if (currentView == null || currentView.GetMetadata() == null)
            {
                return;
            }

            canvasPanel.Visible = false; // HACK HACK HACK - Needed to prevent downward 'creep' of panel designer in the canvas; see me if you have questions (E. Knudsen 10/14/2010)                        

            DataRow row = currentView.GetMetadata().GetPageSetupData(currentView);

            float dpiX;
            Graphics graphics = this.CreateGraphics(); 
            dpiX = graphics.DpiX;             

            int height = (int)row["Height"];
            int width = (int)row["Width"];

            if (dpiX != 96)
            {
                float scaleFactor = (dpiX * 1.041666666f) / 100;
                height = Convert.ToInt32(((float)height) * (float)scaleFactor);
                width = Convert.ToInt32(((float)width) * (float)scaleFactor);
            }

            if (row["Orientation"].ToString() == "Landscape")
            {
                canvasPanel.Size = new Size(height, width);
            }
            else
            {
                canvasPanel.Size = new Size(width, height);
            }

            int darkControlWidth = mainFrm.Size.Width - mainFrm.ViewExplorerWidth;
            int leftMargin = (darkControlWidth - canvasPanel.Size.Width) / 2;
            leftMargin = leftMargin < 5 ? 5 : leftMargin;

            int darkControlHeight = mainFrm.Size.Height - 120;
            int topMargin = (darkControlHeight - canvasPanel.Size.Height) / 2;
            topMargin = 0; // topMargin < 5 ? 5 : topMargin;
            topMargin = 0; // topMargin > 60 ? 60 : topMargin;

            canvasPanel.Location = new Point(leftMargin, topMargin);
            Size = canvasPanel.Size;

            ControlFactory factory = ControlFactory.Instance;
            RedrawCanvasBackground();
            Panel panel = this.currentPanel;

            canvasPanel.Visible = true;

            foreach (Control control in panel.Controls)
            {
                Field field = factory.GetAssociatedField(control);
                if (field != null)
                {
                    control.Visible = field.IsVisible;

                    if (field.IsHighlighted)
                    {
                        control.BackColor = Canvas.HighlightColor;
                    }

                    if (field is InputFieldWithoutSeparatePrompt)
                    {
                        if (((InputFieldWithoutSeparatePrompt)field).IsReadOnly || enable == false)
                        {
                            control.Enabled = false;
                        }
                        else
                        {
                            control.Enabled = field.IsEnabled;
                        }
                    }
                    else if (field is InputFieldWithSeparatePrompt)
                    {
                        if (((InputFieldWithSeparatePrompt)field).IsReadOnly || enable == false)
                        {
                            control.Enabled = false;
                        }
                        else
                        {
                            control.Enabled = field.IsEnabled;
                        }
                    }
                    else if (field is GridField)
                    {
                        if (((GridField)field).IsEnabled == false || enable == false)
                        {
                            control.Enabled = false;
                        }
                        else
                        {
                            control.Enabled = field.IsEnabled;
                        }
                    }
                    else if (enable == false && (field is LabelField || field is RelatedViewField || field is CommandButtonField))
                    {
                        control.Enabled = false;
                    }
                    else
                    {
                        control.Enabled = field.IsEnabled;
                    }

                    if (field is MirrorField)
                    {
                        control.Enabled = false;
                    }
                }

                if (control is Label || control is FieldGroupBox)
                {
                    continue;
                }
                
                field = this.currentView.Fields[field.Name];
                if (field is ImageField)
                {
                    GetFieldDataIntoControl(field as ImageField, control as PictureBox);
                }
                else if (field is YesNoField)
                {
                    GetFieldDataIntoControl(field as YesNoField, control as ComboBox);
                }
                else if (control is TextBoxBase || control is ComboBox || control is CheckBox || control is DataGridView || control is GroupBox || control is DateTimePicker)
                {
                    if (field is MirrorField)
                    {
                        GetMirrorData(field as MirrorField, control);
                    }
                    else if (field is IDataField || control is DataGridView)
                    {
                        if (control is MaskedTextBox)
                        {
                            if (((IDataField)field).CurrentRecordValueObject != null)
                            {
                                if (((MaskedTextBox)control).Mask != null)
                                {
                                     if (field is NumberField && !Util.IsEmpty(((IDataField)field).CurrentRecordValueString))
                                    {
                                        control.Text = FormatNumberInput(((NumberField)field).CurrentRecordValueString, ((NumberField)field).Pattern);
                                    }
                                    else
                                    {
                                        control.Text = ((IDataField)field).CurrentRecordValueString;
                                    }
                                }
                                else
                                {
                                    control.Text = ((IDataField)field).CurrentRecordValueString;
                                }
                            }
                            else
                            {
                                control.Text = string.Empty;
                            }
                        }
                        else if (control is TextBoxBase)
                        {
                            if (field is DateField)
                            {
                                GetDateData(field as DateField, control);
                            }
                            else if (field is TimeField)
                            {
                                GetTimeData(field as TimeField, control);
                            }
                            else if (field is DateTimeField)
                            {
                                GetDateTimeData(field as DateTimeField, control);
                            }
                            else
                            {
                                GetTextData(field, control);
                            }
                        }
                        else if (control is ComboBox)
                        {
                            GetComboboxData(field, control);
                        }
                        else if (control is CheckBox)
                        {
                            GetCheckBoxData(field, control);
                        }
                        else if (control is DataGridView)
                        {
                            GetDataGridViewData(field, control);
                        }
                        else if (field is OptionField)
                        {
                            GetOptionData((OptionField)field, control);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Formats a number string to the proper mask format.
        /// </summary>
        /// <param name="numberInput">Number to format</param>
        /// <param name="mask">Number format mask</param>
        /// <returns></returns>
        public string FormatNumberInput(string numberInput, string mask)
        {
            if (string.IsNullOrEmpty(mask) || mask.Equals("None"))
            {
                return numberInput;
            }
            
            if (mask.Contains("."))
            {
                mask = "{0:" + mask.Replace("#", "0") + "}";
                Match spaceMatch = Regex.Match(numberInput, @"\.*[\s][0-9]");
                if (spaceMatch.Success)
                {
                    numberInput = Regex.Replace(numberInput, @"[\s]", "0");
                }
                else
                {
                    numberInput = Regex.Replace(numberInput, @"[\s]", "");
                }
                if (numberInput.Contains("-"))
                {
                    mask = mask.Remove(3, 1);
                }
                numberInput = String.Format(mask, float.Parse(numberInput));
            }
            else
            {
                double parsedValue = Math.Round(double.Parse(numberInput), 0);
                string format = mask.Replace("#", "0");
                numberInput = parsedValue.ToString(format);
            }
            return numberInput;
        }        

        /// <summary>
        /// Adds controls to a panel
        /// </summary>
        /// <param name="controls">Controls to be added to the panel</param>
        /// <param name="panel">The panel in which controls will be added</param>
        public void AddControlsToPanel(List<Control> controls, Panel panel)
        {
            List<GroupBox> groups = new List<GroupBox>();
            foreach (Control control in controls)
            {
                if (control is GroupBox)
                {
                    groups.Add((GroupBox)control);
                }
            }

            foreach (Control control in controls)
            {
                SetControlProperties(control);

                if (control is CheckBox)
                {
                    foreach (GroupBox group in groups)
                    {
                        if (group.Top < control.Top && group.Bottom > control.Top && group.Left < control.Left && group.Right > control.Left)
                        {
                            control.BackColor = group.BackColor;
                        }
                    }
                }
                
                panel.Controls.Add(control);
            }
        }

        /// <summary>
        /// Sets the z-order of all group fields
        /// </summary>
        public void SetZeeOrderOfGroups()
        {
            System.Collections.Generic.SortedList<IFieldControl, Point> pointSortedGroups = new SortedList<IFieldControl, Point>(new GroupZeeOrderComparer());

            foreach (Control possibleGroupControl in currentPanel.Controls)
            {
                IFieldControl fieldControl = possibleGroupControl as IFieldControl;
                if (fieldControl != null && fieldControl.Field is GroupField)
                {
                    pointSortedGroups.Add(fieldControl, ((Control)fieldControl).Location);
                }
            }

            foreach (KeyValuePair<IFieldControl, Point> kvp in pointSortedGroups)
            {
                ((Control)kvp.Key).SendToBack();
            }
        }
        #endregion // Public Methods
    }
    #region GroupZeeOrderComparer
    class GroupZeeOrderComparer : IComparer<IFieldControl>
    {
        public int Compare(IFieldControl x, IFieldControl y)
        {
            if (((Control)x).Top < ((Control)y).Top + 2)
            {
                return 1;
            }
            else if (((Control)x).Top <= ((Control)y).Top)
            {
                if ((((Control)x).Left > ((Control)y).Left))
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                return -1;
            }
        }
    }
    #endregion
}
