using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Epi.Data.Services;

using Epi;
using Epi.Enter.Controls;
using Epi.Fields;
using Epi.Windows.Controls;

namespace Epi.Windows.Enter.PresentationLogic
{
    class ControlFactory : Epi.Core.IControlFactory  
    {
        #region Private Members

        private static Object classLock = typeof(ControlFactory);
        private Dictionary<Control, Field> _controlFields;
        private List<Control> controlsList;
        private static ControlFactory factory;
        private static BorderStyle borderStyle = BorderStyle.FixedSingle;
        private static FlatStyle flatStyle = FlatStyle.Standard;
        private Dictionary<Field, List<Control>> fieldControls;
        private Configuration config;

        #endregion

        #region Constructors

        private ControlFactory()
        {
            _controlFields = new Dictionary<Control, Field>();
            fieldControls = new Dictionary<Field, List<Control>>();
            controlsList = new List<Control>();
            config = Configuration.GetNewInstance();
        }

        #endregion

        #region Public Properties

        public bool IsPopup
        {
            get;
            set;
        }
      

        /// <summary>
        /// Gets an instance of the control factory
        /// </summary>
        public static ControlFactory Instance
        {
            get
            {
                lock (classLock)
                {
                    if (factory == null)
                    {
                        factory = new ControlFactory();
                    }
                    return factory;
                }
            }
        }

        #endregion

        #region Public Methods

        public void ResetControlFields()
        {
            foreach (KeyValuePair<Control, Field> kvp in _controlFields)
            {
                if (kvp.Key is ComboBox)
                {
                    ((ComboBox)kvp.Key).DataSource = null;
                }
                
                ((Control)kvp.Key).Dispose();
            }
            
            GC.Collect();
            _controlFields = new Dictionary<Control, Field>();
        }

        /// <summary>
        /// Gets a control's associated field
        /// </summary>
        /// <param name="control">Control</param>
        /// <returns>An Epi Info field</returns>
        public Field GetAssociatedField(Control control)
        {
            //DEFECT #537 isu6: Added validation that the control exists in the Dictionary.
            if (_controlFields.ContainsKey(control))
            {
                return _controlFields[control];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a fields's associated controls
        /// </summary>
        /// <param name="field">Field</param>
        /// <returns>A list of controls</returns>
        public List<Control> GetAssociatedControls(Field field)
        {
            #region Input Validation
            if (field == null)
            {
                throw new ArgumentNullException("Field");
            }
            #endregion  //Input Validation

            if (fieldControls.ContainsKey(field))
            {
                return fieldControls[field];
            }
            else
            {
                return new List<Control>();
            }
        }

        public List<Control> GetPageControls(Page page, Size canvasSize)
        {
            List<Control> controls = new List<Control>();

            foreach (Field field in page.Fields)
            {
                List<Control> fields = GetFieldControls(field, canvasSize);
                controls.AddRange(fields);
            }

            return controls;
        }

        /// <summary>
        /// Gets a list of controls for a field
        /// </summary>
        /// <param name="field">The field object</param>
        /// <param name="canvasSize">The size of the page canvas</param>
        /// <returns>A list of controls</returns>
        public List<Control> GetFieldControls(Field field, Size canvasSize)
        {
            if (field is CheckBoxField)
            {
                return GetControls((CheckBoxField)field, canvasSize);
            }
            else if (field is CommandButtonField)
            {
                return GetControls((CommandButtonField)field, canvasSize);
            }
            else if (field is LabelField)
            {
                return GetControls((LabelField)field, canvasSize);
            }
            else if (field is YesNoField)
            {
                return GetControls((YesNoField)field, canvasSize);
            }
            else if (field is GUIDField)
            {
                return GetControls((GUIDField)field, canvasSize);
            }
            else if (field is InputTextBoxField)
            {
                return GetControls((InputTextBoxField)field, canvasSize);
            }
            else if (field is TableBasedDropDownField)
            {
                return GetControls((TableBasedDropDownField)field, canvasSize);
            }
            else if (field is RelatedViewField)
            {
                return GetControls((RelatedViewField)field, canvasSize);
            }
            else if (field is MirrorField)
            {
                return GetControls((MirrorField)field, canvasSize);
            }
            else if (field is ImageField)
            {
                return GetControls((ImageField)field, canvasSize);
            }
            else if (field is GridField)
            {
                return GetControls((GridField)field, canvasSize);
            }
            else if (field is OptionField)
            {
                return GetControls((OptionField)field, canvasSize);
            }
            else if (field is GroupField)
            {
                return GetControls((GroupField)field, canvasSize);
            }
            else
            {
                throw new ArgumentException("Field does not have rendering capability.", field.ToString());
            }
        }

        #endregion  //Public Methods

        #region Private Methods

        private List<Control> GetControls(CheckBoxField field, Size canvasSize)
        {
            CheckBox checkBox = new CheckBox();
            SetControlProperties(checkBox, field, canvasSize);
            checkBox.Text = field.PromptText;
            checkBox.FlatStyle = flatStyle;
            checkBox.BackColor = System.Drawing.Color.Transparent;
            checkBox.Font = field.PromptFont;
            checkBox.AutoSize = true;

            if (field.BoxOnRight)
            {
                checkBox.CheckAlign = ContentAlignment.MiddleRight;
            }

            List<Control> controls = new List<Control>();
            controls.Add(checkBox);
            if (!fieldControls.ContainsKey(field))
            {
                fieldControls.Add(field, controls);
            }
            else
            {
                fieldControls.Remove(field);
                fieldControls.Add(field, controls);
            }
            return controls;
        }

        private List<Control> GetControls(CommandButtonField field, Size canvasSize)
        {
            Button button = new Button();
            SetControlProperties(button, field, canvasSize);
            
            button.Text = field.PromptText;
            button.UseMnemonic = false;
            button.FlatStyle = FlatStyle.Standard;//flatStyle;
            
            List<Control> controls = new List<Control>();
            controls.Add(button);
            if (!fieldControls.ContainsKey(field))
            {
                fieldControls.Add(field, controls);
            }
            else
            {
                fieldControls.Remove(field);
                fieldControls.Add(field, controls);
            }
            return controls;
        }

        private List<Control> GetControls(RelatedViewField field, Size canvasSize)
        {
            Button button = new Button();
            SetControlProperties(button, field, canvasSize);
            button.Text = field.PromptText;
            button.UseMnemonic = false;
            button.FlatStyle = flatStyle;
            List<Control> controls = new List<Control>();
            controls.Add(button);
            if (!fieldControls.ContainsKey(field))
            {
                fieldControls.Add(field, controls);
            }
            else
            {
                fieldControls.Remove(field);
                fieldControls.Add(field, controls);
            }
            return controls;
        }

        private List<Control> GetControls(LabelField field, Size canvasSize)
        {
            TransparentLabel label = new TransparentLabel();
            SetControlProperties(label, field, canvasSize);
            label.Text = field.PromptText.Replace("\t","    ");
            label.AutoSize = false;
            label.Name = field.Name;

            if (field.Page.FlipLabelColor)
            {
                label.ForeColor = Color.White;
            }

            List<Control> controls = new List<Control>();
            controls.Add(label);
            if (!fieldControls.ContainsKey(field))
            {
                fieldControls.Add(field, controls);
            }
            else
            {
                fieldControls.Remove(field);
                fieldControls.Add(field, controls);
            }

            field.Control = label;
            return controls;
        }

        private List<Control> GetControls(YesNoField field, Size canvasSize)
        {
            ComboBox comboBox = new Epi.Windows.Enter.Controls.LegalValuesComboBox();//ComboBox();
            SetControlProperties(comboBox, field, canvasSize);

            comboBox.DropDownStyle = ComboBoxStyle.DropDown;//ComboBoxStyle.DropDownList;
            comboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
            comboBox.FlatStyle = FlatStyle.Standard;

            DataTable dt = new DataTable();
            dt.Columns.Add("name", typeof(string));
            dt.Columns.Add("value", typeof(int));
            DataRow dr;
            dr = dt.NewRow();
            dr["name"] = config.Settings.RepresentationOfYes;
            dr["value"] = Constants.YES;
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["name"] = config.Settings.RepresentationOfNo;
            dr["value"] = Constants.NO;
            dt.Rows.Add(dr);

            comboBox.ValueMember = "value";
            comboBox.DisplayMember = "name";
            comboBox.DataSource = dt;

            // Create Prompt
            Label prompt = GetPrompt(comboBox, field, canvasSize);
            List<Control> controls = new List<Control>();
            controls.Add(prompt);
            controls.Add(comboBox);
            if (fieldControls.ContainsKey(field))
            {
                fieldControls[field] = controls;
            }
            else
            {
                fieldControls.Add(field, controls);
            }

            if (field.CurrentRecordValueObject == null || string.IsNullOrEmpty(field.CurrentRecordValueString))
            {
                comboBox.SelectedIndex = -1;
            }
            else
            {
                comboBox.SelectedItem = field.CurrentRecordValueString;
            }

            return controls;
        }

        private List<Control> GetControls(InputTextBoxField field, Size canvasSize)
        {
            TextBoxBase textBoxBase;
            List<Control> controls = new List<Control>();

            if (field is IPatternable)
            {
                textBoxBase = new MaskedTextBox();
                ((MaskedTextBox)textBoxBase).HidePromptOnLeave = true;
                textBoxBase.Text = string.Empty;
                textBoxBase.BorderStyle = borderStyle;
                ((MaskedTextBox)textBoxBase).Mask = AppData.Instance.DataPatternsDataTable.GetMaskByPattern(((IPatternable)field).Pattern);

                if (field is PhoneNumberField)
                {
                    if(((MaskedTextBox)textBoxBase).Mask == string.Empty)
                    {
                        ((MaskedTextBox)textBoxBase).Mask = "CCCCCCCCCCCCCCCCCCCC";
                    }
                }

                SetControlProperties(textBoxBase, field, canvasSize);
                Label prompt = GetPrompt(textBoxBase, field, canvasSize);
                prompt.Name = field.Name;
                controls.Add(prompt);
            }
            else if(field is MultilineTextField)
            {
                textBoxBase = new RichTextBox();
                textBoxBase.BorderStyle = borderStyle;
                textBoxBase.Multiline = true;
               
                SetControlProperties(textBoxBase, field, canvasSize);
                TransparentLabel prompt = GetPrompt(textBoxBase, field, canvasSize);
                prompt.Name = field.Name;
                controls.Add(prompt);
            }
            else if (field is DateTimeField)
            {
                textBoxBase = new TextBox();
                textBoxBase.BorderStyle = borderStyle;

                SetControlProperties(textBoxBase, field, canvasSize);
                TransparentLabel prompt = GetPrompt(textBoxBase, field, canvasSize);
                prompt.Name = field.Name;

                controls.Add(prompt);
            }
            else
            {
                textBoxBase = new TextBox();
                textBoxBase.BorderStyle = borderStyle;
                textBoxBase.MaxLength = ((SingleLineTextField)field).MaxLength;

                SetControlProperties(textBoxBase, field, canvasSize);
                TransparentLabel prompt = GetPrompt(textBoxBase, field, canvasSize);
                prompt.Name = field.Name;
                controls.Add(prompt);
            }

            controls.Add(textBoxBase);

            if (!fieldControls.ContainsKey(field))
            {
                fieldControls.Add(field, controls);
            }
            else
            {
                fieldControls.Remove(field);
                fieldControls.Add(field, controls);
            }
            return controls;
        }

        private List<Control> GetControls(MirrorField field, Size canvasSize)
        {
            TextBox textBox = new TextBox();
            SetControlProperties(textBox, field, canvasSize);
            textBox.ReadOnly = true;
            textBox.Enabled = false;
            textBox.BorderStyle = borderStyle;
            Label prompt = GetPrompt(textBox, field, canvasSize);
            List<Control> controls = new List<Control>();
            controls.Add(prompt);
            controls.Add(textBox);
            if (!fieldControls.ContainsKey(field))
            {
                fieldControls.Add(field, controls);
            }
            else
            {
                fieldControls.Remove(field);
                fieldControls.Add(field, controls);
            }
            return controls;
        }

        private Dictionary<string, DataTable> cachedListValues;

        private List<Control> GetControls(TableBasedDropDownField field, Size canvasSize)
        {
            string displayMember = field.TextColumnName.Trim();
            string valueMember;

            if (field is DDLFieldOfCodes)
            {
                valueMember = field.CodeColumnName.Trim();
                ((DDLFieldOfCodes)field).ControlFactory = this;
            }
            else if (field is DDListField)
            {
                valueMember = field.CodeColumnName.Trim();
                ((DDListField)field).ControlFactory = this;
            }
            else
            {
                valueMember = field.TextColumnName.Trim();
            }

            ComboBox comboBox = new Epi.Windows.Enter.Controls.LegalValuesComboBox();
            
            SetControlProperties(comboBox, field, canvasSize);
            
            if (field is DDListField)
            {
                comboBox.DropDownStyle = ComboBoxStyle.Simple;
            }
            else
            {
                comboBox.DropDownStyle = ComboBoxStyle.DropDown;
            }
            
            comboBox.Sorted = field.ShouldSort;

            if (!string.IsNullOrEmpty(displayMember))
            {
                comboBox.DisplayMember = null;
                comboBox.ValueMember = null;

                if (cachedListValues == null)
                {
                    cachedListValues = new Dictionary<string, DataTable>();
                }
                DataTable displayTable;
                if (field.SourceTableName.Contains("-"))
                {
                    string tablename = field.SourceTableName.Substring(0, field.SourceTableName.IndexOf('-'));
                    displayTable = field.GetDisplayTable("", "", displayMember, tablename);
                }
                else
                {
                    if (cachedListValues.ContainsKey(displayMember + "," + field.SourceTableName))
                    {
                        displayTable = cachedListValues[displayMember + "," + field.SourceTableName];
                    }
                    else
                    {
                        displayTable = field.GetDisplayTable("", "", displayMember);
                        cachedListValues.Add(displayMember + "," + field.SourceTableName, displayTable);
                    }
                }

                if (displayTable != null)
                {
                    comboBox.BeginUpdate();
                    comboBox.Items.AddRange(displayTable.AsEnumerable().Select(row => row.Field<string>("Item") ?? string.Empty).ToArray());
                    comboBox.EndUpdate();

                    if (displayTable.Rows.Count > 0)
                    {
                        if (field.CurrentRecordValueObject == null || string.IsNullOrEmpty(field.CurrentRecordValueString))
                        {
                            comboBox.SelectedIndex = -1;
                        }
                        else
                        {
                            comboBox.SelectedItem = field.CurrentRecordValueString;
                        }
                    }
                }
            }

            Label prompt = GetPrompt(comboBox, field, canvasSize);
            List<Control> controls = new List<Control>();
            controls.Add(prompt);
            controls.Add(comboBox);

            field.Control = comboBox;

            if (!fieldControls.ContainsKey(field))
            {
                fieldControls.Add(field, controls);
            }
            else
            {
                fieldControls.Remove(field);
                fieldControls.Add(field, controls);
            }

            return controls;
        }

        private List<Control> GetControls(ImageField field, Size canvasSize)
        {
            PictureBox pictureBox = new PictureBox();

            // Set PictuceBox properties
            SetControlProperties(pictureBox, field, canvasSize);
            pictureBox.BorderStyle = borderStyle;

            if (field.ShouldRetainImageSize)
            {
                pictureBox.SizeMode = PictureBoxSizeMode.Normal;
            }
            else
            {
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            }

            Label prompt = GetPrompt(pictureBox, field, canvasSize);
            List<Control> controls = new List<Control>();
            controls.Add(prompt);
            controls.Add(pictureBox);

            if (!fieldControls.ContainsKey(field))
            {
                fieldControls.Add(field, controls);
            }
            else
            {
                fieldControls.Remove(field);
                fieldControls.Add(field, controls);
            }

            return controls;
        }

        private List<Control> GetControls(GroupField field, Size canvasSize)
        {
            DragableGroupBox group = new DragableGroupBox();
            SetControlProperties(group, field, canvasSize);
            group.ForeColor = Color.Black;
            group.BackColor = field.BackgroundColor;
            group.Text = field.PromptText;
            group.AutoSize = true;
            group.Tag = field.ChildFieldNames;

            List<Control> controls = new List<Control>();
            controls.Add(group);
            if (!fieldControls.ContainsKey(field))
            {
                fieldControls.Add(field, controls);
            }
            else
            {
                fieldControls.Remove(field);
                fieldControls.Add(field, controls);
            }
            return controls;
        }

        private List<Control> GetControls(GridField gridField, Size canvasSize)
        {
            DataGridView gridView = new DataGridView();
            gridView.AutoGenerateColumns = false;
            gridView.Width = 160;
            gridView.Height = 150;
            SetControlProperties(gridView, gridField, canvasSize);
            gridView.TabStop = false;
            gridView.BorderStyle = borderStyle;
            gridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            gridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridView.EnableHeadersVisualStyles = true;
            gridView.ColumnHeadersDefaultCellStyle.Font = gridField.PromptFont; 

            RectangleF rectF = new RectangleF(gridView.Left, gridView.Top, gridView.Width, ((float)1.75 * (int)gridField.PromptFont.Size));

            gridView.Top = WinUtil.GetControlTop(gridField, canvasSize.Height);
            List<GridColumnBase> columns = new List<GridColumnBase>(gridField.Columns);
            columns.Sort(Util.SortByPosition);

            foreach (GridColumnBase gridCol in columns)
            {
                if (gridCol is TableBasedDropDownColumn)
                {
                    DataGridViewComboBoxColumn comboBoxColumn = new DataGridViewComboBoxColumn();
                    try
                    {
                        comboBoxColumn.MinimumWidth = 25;
                        comboBoxColumn.Name = gridCol.Name;
                        comboBoxColumn.DataPropertyName = gridCol.Name;
                        comboBoxColumn.HeaderText = gridCol.Text;
                        comboBoxColumn.ReadOnly = gridCol.IsReadOnly;
                        comboBoxColumn.Width = gridCol.Width;
                        comboBoxColumn.FlatStyle = FlatStyle.Flat;
                        comboBoxColumn.Tag = gridCol;

                        string displayMember = ((TableBasedDropDownColumn)gridCol).TextColumnName.Trim();

                        if (gridCol is YesNoColumn)
                        {
                            DataTable dataTable = new DataTable();
                            dataTable.Columns.Add("name", typeof(string));
                            dataTable.Columns.Add("value", typeof(byte));
                            DataRow dataRow;

                            dataRow = dataTable.NewRow();
                            dataRow["name"] = config.Settings.RepresentationOfYes;
                            dataRow["value"] = Constants.YES;
                            dataTable.Rows.Add(dataRow);

                            dataRow = dataTable.NewRow();
                            dataRow["name"] = config.Settings.RepresentationOfNo;
                            dataRow["value"] = Constants.NO;
                            dataTable.Rows.Add(dataRow);

                            dataRow = dataTable.NewRow();
                            dataRow["name"] = config.Settings.RepresentationOfMissing;
                            dataRow["value"] = DBNull.Value;
                            dataTable.Rows.Add(dataRow);

                            comboBoxColumn.ValueMember = "value";
                            comboBoxColumn.DisplayMember = "name";
                            comboBoxColumn.DataSource = dataTable;
                        }
                        else
                        {
                            if (!displayMember.Equals(string.Empty))
                            {
                                comboBoxColumn.DisplayMember = displayMember;
                                comboBoxColumn.ValueMember = displayMember;

                                if (((TableBasedDropDownColumn)gridCol).ShouldSort)
                                {
                                    DataTable dataTable = ((TableBasedDropDownColumn)gridCol).GetSourceData();
                                    dataTable.Select(null, dataTable.Columns[0].ColumnName);
                                    comboBoxColumn.DataSource = dataTable;
                                }
                                else
                                {
                                    comboBoxColumn.DataSource = ((TableBasedDropDownColumn)gridCol).GetSourceData();
                                }
                            }
                        }
                    }
                    catch (InvalidOperationException ioEx)
                    {
                        MsgBox.ShowException(ioEx);
                    }
                    gridView.Columns.Add(comboBoxColumn);
                }
                else if (gridCol is PatternableColumn)
                {
                    bool isTimeBasedColumn = false;

                    if (gridCol is DateColumn || gridCol is DateTimeColumn || gridCol is TimeColumn)
                    {
                        isTimeBasedColumn = true;
                    }
                    
                    MaskedTextBoxColumn maskedTextBoxColumn = new MaskedTextBoxColumn();
                    maskedTextBoxColumn.MinimumWidth = 25;

                    if (isTimeBasedColumn == false)
                    {
                        maskedTextBoxColumn.Mask = AppData.Instance.DataPatternsDataTable.GetMaskByPattern(((PatternableColumn)gridCol).Pattern);
                    }
                    
                    maskedTextBoxColumn.HidePromptOnLeave = DataGridViewTriState.True;
                    maskedTextBoxColumn.Name = gridCol.Name;
                    maskedTextBoxColumn.HeaderText = gridCol.Text;
                    maskedTextBoxColumn.ReadOnly = gridCol.IsReadOnly;
                    maskedTextBoxColumn.Width = gridCol.Width;
                    maskedTextBoxColumn.GridColumn = ((PatternableColumn)gridCol);
                    maskedTextBoxColumn.DataPropertyName = gridCol.Name;

                    if (isTimeBasedColumn == false)
                    {
                        maskedTextBoxColumn.DefaultCellStyle.Format = AppData.Instance.DataPatternsDataTable.GetExpressionByMask(maskedTextBoxColumn.Mask, ((PatternableColumn)gridCol).Pattern);
                    }

                    System.Globalization.DateTimeFormatInfo formatInfo = System.Globalization.DateTimeFormatInfo.CurrentInfo;

                    if (gridCol is DateColumn)
                    {
                        ((System.Windows.Forms.DataGridViewColumn)(maskedTextBoxColumn)).DefaultCellStyle.Format = formatInfo.ShortDatePattern;
                    }
                    else if (gridCol is TimeColumn)
                    {
                        ((System.Windows.Forms.DataGridViewColumn)(maskedTextBoxColumn)).DefaultCellStyle.Format = formatInfo.ShortTimePattern;
                    }

                    gridView.Columns.Add(maskedTextBoxColumn);
                }
                else if (gridCol is CheckboxColumn)
                {
                    DataGridViewColumn column = new DataGridViewCheckBoxColumn();

                    try
                    {
                        column.MinimumWidth = 25;

                        column.Name = gridCol.Name;
                        column.HeaderText = gridCol.Text;
                        column.ReadOnly = gridCol.IsReadOnly;
                        column.DataPropertyName = gridCol.Name;
                        column.Width = gridCol.Width;
                    }
                    catch (InvalidOperationException ioEx)
                    {
                        MsgBox.ShowException(ioEx);
                    }

                    gridView.Columns.Add(column);
                }
                else
                {
                    DataGridViewTextBoxColumn textBoxColumn = new DataGridViewTextBoxColumn();
                    try
                    {
                        textBoxColumn.MinimumWidth = 25;
                        textBoxColumn.Name = gridCol.Name;
                        textBoxColumn.HeaderText = gridCol.Text;
                        textBoxColumn.ReadOnly = gridCol.IsReadOnly;
                        textBoxColumn.DataPropertyName = gridCol.Name;

                        if (gridCol is PredefinedColumn)
                        {
                            textBoxColumn.ReadOnly = true;
                        }
                        textBoxColumn.Width = gridCol.Width;

                        if (gridCol is TextColumn)
                        {
                            textBoxColumn.MaxInputLength = ((TextColumn)gridCol).Size;
                        }

                    }
                    catch (InvalidOperationException ioEx)
                    {
                        MsgBox.ShowException(ioEx);
                    }

                    gridView.Columns.Add(textBoxColumn);
                }
            }

            try
            {
                gridView.Columns[ColumnNames.REC_STATUS].ReadOnly = true;
                gridView.Columns[ColumnNames.REC_STATUS].Visible = false;
                gridView.Columns[ColumnNames.UNIQUE_ROW_ID].ReadOnly = true;
                gridView.Columns[ColumnNames.UNIQUE_ROW_ID].Visible = false;
                gridView.Columns[ColumnNames.FOREIGN_KEY].ReadOnly = true;
                gridView.Columns[ColumnNames.FOREIGN_KEY].Visible = false;

                if (gridView.Columns.Contains(ColumnNames.GLOBAL_RECORD_ID))
                { 
                    gridView.Columns[ColumnNames.GLOBAL_RECORD_ID].ReadOnly = true;
                    gridView.Columns[ColumnNames.GLOBAL_RECORD_ID].Visible = false;
                }
                gridView.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
                gridView.Tag = gridField;

                gridView.BackgroundColor = Color.White;
                gridView.RowHeadersVisible = false;
                gridView.MultiSelect = true;

                ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
                ToolStripMenuItem deleteRowStripMenuItem = new ToolStripMenuItem();
                contextMenuStrip.SuspendLayout();
                deleteRowStripMenuItem.Name = "deleteRow";
                deleteRowStripMenuItem.Text = "Delete Row";
                deleteRowStripMenuItem.Image = global::Epi.Enter.Properties.Resources.delete_icon;
                deleteRowStripMenuItem.ImageTransparentColor = Color.White;
                deleteRowStripMenuItem.Tag = gridView;
                deleteRowStripMenuItem.Click += new EventHandler(deleteRowStripMenuItem_Click);
                contextMenuStrip.Items.Add(deleteRowStripMenuItem);
                contextMenuStrip.ResumeLayout();

                gridView.ContextMenuStrip = contextMenuStrip;
                gridView.Refresh();
            }
            catch (InvalidOperationException ioEx)
            {
                MsgBox.ShowException(ioEx);
            }
            catch (Exception ex)
            {
                MsgBox.ShowException(ex);
            }

            TransparentLabel prompt = GetPrompt(gridView, gridField, canvasSize);
            List<Control> controls = new List<Control>();
            controls.Add(prompt);
            controls.Add(gridView);

            if (!fieldControls.ContainsKey(gridField))
            {
                fieldControls.Add(gridField, controls);
            }
            return controls;
        }

        void deleteRowStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridView dataGridView = (DataGridView)((ToolStripMenuItem)sender).Tag;
            
            foreach (DataGridViewCell selectedCell in dataGridView.SelectedCells)
            {
                int rowIndex = selectedCell.RowIndex;

                if (rowIndex >= 0 && rowIndex < dataGridView.Rows.Count)
                { 
                    bool isNew = dataGridView.Rows[rowIndex].IsNewRow;

                    if (isNew)
                    {
                    }
                    else
                    {
                        dataGridView.Rows.RemoveAt(rowIndex);
                    }
                }
            }

            ((Epi.Fields.GridField)dataGridView.Tag).View.IsDirty = true;
        }

        private List<Control> GetControls(GUIDField field, Size canvasSize)
        {
            TextBox textBox = new TextBox();
            MaskedTextBox maskedTextBox = new MaskedTextBox();

            if (field is IPatternable && !(string.IsNullOrEmpty(((IPatternable)field).Pattern)))
            {
                textBox.Dispose();
                maskedTextBox.HidePromptOnLeave = true;
                maskedTextBox.Text = string.Empty;
                maskedTextBox.BorderStyle = borderStyle;
                //maskedTextBox.MaskInputRejected += new MaskInputRejectedEventHandler(maskedTextBox_MaskInputRejected);
                maskedTextBox.Mask = AppData.Instance.DataPatternsDataTable.GetMaskByPattern(((IPatternable)field).Pattern);

                SetControlProperties(maskedTextBox, field, canvasSize);
                Label maskedPrompt = GetPrompt(maskedTextBox, field, canvasSize);
                List<Control> maskedControls = new List<Control>();
                maskedControls.Add(maskedPrompt);
                maskedControls.Add(maskedTextBox);
                if (!fieldControls.ContainsKey(field))
                {
                    fieldControls.Add(field, maskedControls);
                }
                else
                {
                    fieldControls.Remove(field);
                    fieldControls.Add(field, maskedControls);
                }
                return maskedControls;
            }
            else
            {
                maskedTextBox.Dispose();

                if (field is GUIDField)
                {
                    if (field.CurrentRecordValueString.Equals(string.Empty))
                    {
                        //textBox.Text = field.NewGuid().ToString();
                    }
                    else
                    {
                        textBox.Text = field.CurrentRecordValueString.Replace(StringLiterals.CURLY_BRACE_LEFT,string.Empty).Replace(StringLiterals.CURLY_BRACE_RIGHT, string.Empty);
                    }
                    textBox.ReadOnly = ((GUIDField)field).IsReadOnly;
                    textBox.MaxLength = ((GUIDField)field).MaxLength;
                }

                SetControlProperties(textBox, field, canvasSize);
                textBox.ReadOnly = true;
                textBox.BorderStyle = borderStyle;
                Label prompt = GetPrompt(textBox, field, canvasSize);
                List<Control> controls = new List<Control>();
                controls.Add(prompt);
                controls.Add(textBox);
                if (!fieldControls.ContainsKey(field))
                {
                    fieldControls.Add(field, controls);
                }
                else
                {
                    fieldControls.Remove(field);
                    fieldControls.Add(field, controls);
                }
                return controls;
            }
        }

        private List<Control> GetControls(OptionField field, Size canvasSize)
        {
            GroupBox groupBox = new GroupBox();
            groupBox.Text = field.PromptText;
            groupBox.Font = field.PromptFont;
            groupBox.FlatStyle = flatStyle;
            groupBox.TabIndex = (int)field.TabIndex;
            groupBox.BackColor = Color.Transparent;
            int groupWidthEstimate;

            if (field.ControlWidthPercentage == 0)
            {
                groupWidthEstimate = groupBox.Width - 40;
            }
            else
            {
                groupBox.Width = (int)(field.ControlWidthPercentage * canvasSize.Width);
                groupWidthEstimate = groupBox.Width - 30;
            }

            Size proposedSize = new Size(groupWidthEstimate, int.MaxValue);
            Bitmap tempImage = new Bitmap(1, 1);
            Graphics graphics = Graphics.FromImage(tempImage);
            SizeF groupPromptSize = graphics.MeasureString(groupBox.Text, groupBox.Font, proposedSize.Width);

            if (field.Options.Count < 1)
            {
                CustomRadioButton rdb = new CustomRadioButton();
                rdb.Text = "Option 1";
                rdb.Left = 5;
                rdb.Top = 20;
                rdb.Enabled = false;
                groupBox.Controls.Add(rdb);
            }
            else
            {
                int tallestControlHeight = 10;
                int widestOptionWidth = 16;
                int leftAlign = 12;


                foreach (string item in field.Options)
                {
                    CustomRadioButton radioButton = new CustomRadioButton();
                    radioButton.Text = item;
                    radioButton.AutoSize = true;

                    radioButton.TabStop = true;

                    radioButton.Enabled = true;
                    radioButton.Visible = false;
                    radioButton.Font = field.ControlFont;
                    groupBox.Controls.Add(radioButton);

                    this._controlFields.Add(radioButton, field);
  
                    if (radioButton.Width > widestOptionWidth)
                    {
                        widestOptionWidth = (int)radioButton.Width;
                    }

                    if (radioButton.Height > tallestControlHeight)
                    {
                        tallestControlHeight = (int)radioButton.Height;
                    }
                }

                widestOptionWidth += 10;

                double div = (groupBox.Width - (1.2 * leftAlign)) / widestOptionWidth;
                div = div < 1 ? 1 : div;
                int columnCount = (int)Math.Floor(div);
                div = (double)field.Options.Count / (double)columnCount;
                int rowCount = (int)Math.Ceiling(div);

                bool isVertical = ((OptionField)field).Pattern.Contains("Vertical");
                bool startOnLeft = ((OptionField)field).Pattern.Contains("Left");

                int topMargin = (int)groupPromptSize.Height + 10;

                int column = 0;
                int row = 0;

                if (startOnLeft == false)
                {
                    column = columnCount - 1;
                }

                int topOfLastControlDown = 0;

                foreach (Control control in groupBox.Controls)
                {
                    control.Top = row * (tallestControlHeight) + topMargin;

                    if (row == (rowCount - 1) || isVertical == false)
                    {
                        topOfLastControlDown = control.Top;
                    }

                    if (field.ShowTextOnRight)
                    {
                        control.RightToLeft = RightToLeft.No;
                        control.Left = column * (widestOptionWidth) + leftAlign;
                    }
                    else
                    {
                        control.RightToLeft = RightToLeft.Yes;
                        control.Left = column * (widestOptionWidth) + leftAlign + widestOptionWidth - control.Width;
                    }

                    groupBox.AutoSizeMode = AutoSizeMode.GrowOnly;
                    groupBox.AutoSize = false;
                    control.Visible = true;

                    if (isVertical)
                    {
                        row++;
                        if (row >= rowCount)
                        {
                            row = 0;
                            column = startOnLeft ? ++column : --column;
                        }
                    }
                    else
                    {
                        if (startOnLeft)
                        {
                            column++;
                            if (column >= columnCount)
                            {
                                column = 0;
                                row += 1;
                            }
                        }
                        else
                        {
                            column--;
                            if (column < 0)
                            {
                                column = columnCount - 1;
                                row += 1;
                            }
                        }
                    }
                }

                groupBox.Height = topOfLastControlDown + (int)(tallestControlHeight * 1.9);
                field.ControlHeightPercentage = 1.0 * groupBox.Height / canvasSize.Height;
            }

            SetControlProperties(groupBox, field, canvasSize);

            List<Control> controls = new List<Control>();
            controls.Add(groupBox);
            if (!fieldControls.ContainsKey(field))
            {
                fieldControls.Add(field, controls);
            }
            else
            {
                fieldControls.Remove(field);
                fieldControls.Add(field, controls);
            }
            return controls;
        }

        private void SetControlProperties(Control control, RenderableField field, Size canvasSize)
        {
            control.Left = WinUtil.GetControlLeft(field, canvasSize.Width);
            control.Top = WinUtil.GetControlTop(field, canvasSize.Height);

            if (field.ControlHeightPercentage > 0)
            {
                control.Height = WinUtil.GetControlHeight(field, canvasSize.Height);
            }
            if (field.ControlWidthPercentage > 0)
            {
                control.Width = WinUtil.GetControlWidth(field, canvasSize.Width);
            }

            control.TabStop = field.HasTabStop;
            control.TabIndex = (int)field.TabIndex;
            
            if ((field is OptionField) == false)
            {
                control.Font = field.ControlFont;
            }
            
            control.Name = field.Name;

            if (control is DragableLabel == false)
            {
                control.BackColor = SystemColors.Window;
            }
            else
            {
                control.CausesValidation = true;
            }

            if (field is InputFieldWithSeparatePrompt)
            {
                control.CausesValidation = true;

                if (((InputFieldWithSeparatePrompt)field).IsReadOnly)
                {
                    control.Enabled = false;
                }
                else
                {
                    control.Enabled = field.IsEnabled;
                }
            } 
            else if (field is InputFieldWithoutSeparatePrompt)
            {
                control.CausesValidation = true;

                if (((InputFieldWithoutSeparatePrompt)field).IsReadOnly)
                {
                    control.Enabled = false;
                }
                else
                {
                    control.Enabled = field.IsEnabled;
                }
            }
            else
            {
                control.Enabled = field.IsEnabled;
            }

            control.Visible = field.IsVisible;

            if (field.IsHighlighted)
            {
                control.BackColor = Canvas.HighlightColor;
            }

            _controlFields.Add(control, field);
        }

        private void SetControlProperties(System.Windows.Forms.MaskedTextBox control, RenderableField field, Size canvasSize)
        {
            control.Left = WinUtil.GetControlLeft(field, canvasSize.Width);
            control.Top = WinUtil.GetControlTop(field, canvasSize.Height);

            if (field.ControlHeightPercentage > 0)
            {
                control.Height = WinUtil.GetControlHeight(field, canvasSize.Height);
            }
            if (field.ControlWidthPercentage > 0)
            {
                control.Width = WinUtil.GetControlWidth(field, canvasSize.Width);
            }
            if (field is FieldWithSeparatePrompt)
            {
                FieldWithSeparatePrompt promptField = (FieldWithSeparatePrompt)field;
                int promptWidth = TextRenderer.MeasureText(field.PromptText, field.PromptFont).Width;
                int promptRight = WinUtil.GetPromptLeft(promptField, canvasSize.Width) + promptWidth;
                
            }

            
            if(((InputFieldWithSeparatePrompt)field).IsReadOnly)
            {
                control.Enabled = false;
            }
            else
            {
                control.Enabled = field.IsEnabled;
            }

            control.Visible = field.IsVisible;

            if (field.IsHighlighted)
            {
                control.BackColor = Canvas.HighlightColor;
            }

       
            control.TabStop = field.HasTabStop;
            control.TabIndex = (int)field.TabIndex;
            control.Font = field.ControlFont;
            control.CausesValidation = true;
            _controlFields.Add(control, field);
        }

        private TransparentLabel GetPrompt(Control control, FieldWithSeparatePrompt field, Size canvasSize)
        {
            TransparentLabel prompt = new TransparentLabel();
            prompt.AutoSize = true;
            prompt.Font = field.PromptFont;
            prompt.Text = field.PromptText;
            prompt.Left = WinUtil.GetPromptLeft(field, canvasSize.Width);
            prompt.Top = WinUtil.GetPromptTop(field, canvasSize.Height);
            prompt.Visible = field.IsVisible;
            prompt.UseMnemonic = false;

            if (field.Page.FlipLabelColor)
            {
                prompt.ForeColor = Color.White;
            }

            _controlFields.Add(prompt, field);
            return prompt;
        }

        private void DataBind(ComboBox comboBox, DataTable sourceData, string displayMember, string valueMember)
        {
            comboBox.DisplayMember = displayMember;
            comboBox.ValueMember = valueMember;
            comboBox.DataSource = sourceData;
            if (sourceData.Rows.Count > 0)
            {
                comboBox.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Assign Tab Index of the field
        /// </summary>
        /// <param name="field">The field whose tab index is to be assigned</param>
        /// <param name="page">The page the field belongs to</param>
        private void AssignTabIndex(Field field, Page page)
        {
            ((RenderableField)field).TabIndex = field.GetMetadata().GetFieldTabIndex(field.Id, field.GetView().Id, page.Id);
        }

        #endregion  Private Methods

        #region Private Event Handlers


        /*
        /// <summary>
        /// Handles the event if the user inputs invalid characters in masked text boxes
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">Mask input rejected supplied event parameters</param>
        private void maskedTextBox_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
            Field field = GetAssociatedField((Control)sender);
            if (field is DateTimeField)
            {
                if (((Control)sender).Text != string.Empty && ((MaskedTextBox)((Control)sender)).Mask.Replace("#","_") != ((Control)sender).Text)
                {
                    //if ((((MaskedTextBox)((Control)sender)).MaskFull) && e.Position != ((MaskedTextBox)((Control)(sender))).Mask.Length)
                    if ((((MaskedTextBox)((Control)sender)).MaskFull))
                    {
                        //Assembly assembly = Assembly.GetExecutingAssembly();
                        //CultureInfo ci = assembly.GetName().CultureInfo;
                        //DateTimeFormatInfo dateTimeInfo = new DateTimeFormatInfo();

                        //((MaskedTextBox)((Control)sender)).TextMaskFormat = MaskFormat.IncludePromptAndLiterals;
                        //string format = AppData.Instance.DataPatternsDataTable.GetExpressionByMask(((MaskedTextBox)((Control)sender)).Mask.ToString(), ((IPatternable)field).Pattern);

                        //bool result;
                        //DateTime dateOrTime;
                        //result = DateTime.TryParseExact(((MaskedTextBox)((Control)sender)).Text, format, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out dateOrTime);
                        //if (!result)
                        //{
                        //    if (field is DateField)
                        //    {
                        //        MsgBox.ShowWarning(SharedStrings.ENTER_VALID_DATE);
                        //    }
                        //    else if (field is TimeField)
                        //    {
                        //        MsgBox.ShowWarning(SharedStrings.ENTER_VALID_TIME);
                        //    }
                        //    else if (field is DateTimeField)
                        //    {
                        //        MsgBox.ShowWarning(SharedStrings.ENTER_VALID_DATE_AND_TIME);
                        //    }

                        //    ((Control)sender).Text = string.Empty;
                        //    ((Control)sender).Focus();
                        //}
                    }
                    else
                    {
                        //((MaskedTextBox)((Control)sender)).Focus();
                    }
                }
            }
            else if (field is PhoneNumberField)
            {
                if (((Control)sender).Text != string.Empty)
                {
                    if (!(((MaskedTextBox)((Control)sender)).MaskFull) )
                    //                    if ((((MaskedTextBox)((Control)sender)).MaskFull)
                    {
                        //MsgBox.ShowError(SharedStrings.ENTER_VALID_PHONE_NUMBER);
                        //((Control)sender).Text = string.Empty;
                        //((Control)sender).Focus();                        
                    }
                }
            }
            else if (field is NumberField)
            {
                if (((Control)sender).Text != string.Empty)
                {
                    if (!(((MaskedTextBox)((Control)sender)).MaskFull)) //&& e.Position != ((MaskedTextBox)((Control)(sender))).Mask.Length)
                    //if ((((MaskedTextBox)((Control)sender)).MaskFull))
                    {
                        //MsgBox.ShowError(SharedStrings.ENTER_VALID_NUMBER);
                        //((Control)sender).Text = string.Empty;
                        //((Control)sender).Focus();
                    }
                }
            }
        }*/
        /*
        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.Equals('\r') || e.KeyChar.Equals('\t'))
            {
                e.Handled = true;
            }
        }*/

        #endregion //Private Event Handlers



    }
}
