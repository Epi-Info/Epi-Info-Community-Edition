using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Epi.Data;
using Epi.Fields;
using Epi.Windows.Controls;

namespace Epi.Windows.MakeView.PresentationLogic
{
    /// <summary>
    /// The control factory
    /// </summary>
    public class ControlFactory
    {
        #region Private Members

        private static ControlFactory factory;
        private static Object classLock = typeof(ControlFactory);
        private DataGridTableStyle tableStyle = null;
        private static BorderStyle borderStyle = BorderStyle.FixedSingle;
        private static FlatStyle flatStyle = FlatStyle.Standard;
        private static int defaultControlWidth = 200;
        #endregion

        #region Constructors
        private ControlFactory()
        {
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

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Gets a list of controls for a page
        /// </summary>
        /// <param name="page">The page object</param>
        /// <param name="canvasSize">The size of the page canvas</param>
        /// <returns>A list of controls</returns>
        public List<Control> GetPageControls(Page page, Size canvasSize)
        {
            List<Control> controls = new List<Control>();
            foreach (Field field in page.Fields)
            {
                controls.AddRange(GetFieldControls(field, canvasSize));
            }
            return controls;
        }

        /// <summary>
        /// Refreshes a control that has already been rendered
        /// </summary>
        /// <param name="control">The control to refresh</param>
        /// <param name="canvasSize">The size of the canvas</param>
        /// <returns>A list of controls</returns>
        public List<Control> RefreshControl(IFieldControl control, Size canvasSize)
        {
            List<Control> controls;
            if (control.Field is GroupField)
            {
                controls = new List<Control>();
                controls = GetFieldControls(control.Field, canvasSize);
            }
            else
            {
                controls = GetFieldControls(control.Field, canvasSize);
            }
            if (control is DragableLabel)
            {
                if (((DragableLabel)control).LabelFor != null)
                {
                    ((DragableLabel)control).LabelFor.Dispose();
                }
            }
            ((Control)control).Dispose();
            return controls;
        }

        /// <summary>
        /// Get Associated Field for a given control
        /// </summary>
        /// <param name="control">Control</param>
        /// <returns>Field associated with the control</returns>
        public Field GetAssociatedField(Control control)
        {
            if (control is IFieldControl)
            {
                return ((IFieldControl)control).Field;
            }
            else
            {
                return null;
            }
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

        #endregion

        #region Private Methods
       
        private string StripOutRightOfHyphen(string textToShow)
        {
            int firstHyphenIndex;
            if (textToShow.Contains("-"))
            {
                firstHyphenIndex = textToShow.IndexOf('-');
            }
            else
            {
                firstHyphenIndex = textToShow.Length;
            }
            textToShow = textToShow.Substring(0, (textToShow.Length - (textToShow.Length - firstHyphenIndex)));
            return textToShow;
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

        private List<Control> GetControls(OptionField field, Size canvasSize)
        {
            DragableGroupBox groupBox = new DragableGroupBox();
            groupBox.Text = field.PromptText;
            groupBox.Font = field.PromptFont;
            groupBox.BackColor = SystemColors.Window;
            int groupWidthEstimate;

            System.Text.StringBuilder pattern = new System.Text.StringBuilder();
            System.Text.StringBuilder locations = new System.Text.StringBuilder();

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
                RadioButton rdb = new RadioButton();
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
                    RadioButton radioButton = new RadioButton();
                    radioButton.Text = item;
                    radioButton.Font = field.ControlFont;
                    radioButton.MaximumSize = new Size(proposedSize.Width + 4, int.MaxValue);

                    proposedSize = new Size(groupWidthEstimate, int.MaxValue);
                    SizeF optionMeasuredSize = graphics.MeasureString(radioButton.Text, radioButton.Font, radioButton.MaximumSize.Width - 20);

                    radioButton.Width = (int)optionMeasuredSize.Width + 20;
                    radioButton.Height = (int)optionMeasuredSize.Height + 1;

                    radioButton.AutoSize = false;
                    radioButton.Enabled = true;
                    radioButton.Visible = false;

                    groupBox.Controls.Add(radioButton);

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

                bool isVertical = true;
                bool startOnLeft = true;

                if (((OptionField)field).Pattern.Contains(Enums.OptionLayout.Horizontal.ToString()))
                { 
                    isVertical = false; 
                }

                if (((OptionField)field).Pattern.Contains(Enums.OptionLayout.Right.ToString()))
                {
                    startOnLeft = false;
                }

                pattern.Append(string.Format("{0},", isVertical == true ? Enums.OptionLayout.Vertical.ToString() : Enums.OptionLayout.Horizontal.ToString()));
                pattern.Append(string.Format("{0},", startOnLeft == true ? Enums.OptionLayout.Left.ToString() : Enums.OptionLayout.Right.ToString()));

                int topMargin = (int)groupPromptSize.Height + 10;

                int column = 0;
                int row = 0;

                if (startOnLeft == false)
                {
                    column = columnCount - 1;
                }

                int topOfLastControlDown = 0;
                int bottomOfLastControlDown = 0;

                foreach (Control control in groupBox.Controls)
                {
                    if(columnCount == 1)
                    {
                        if(row == 0)
                        {
                            control.Top = bottomOfLastControlDown + topMargin;
                        }
                        else
                        {
                            control.Top = bottomOfLastControlDown + 6;
                        }

                        topOfLastControlDown = control.Top;
                        bottomOfLastControlDown = control.Top + control.Height;

                        control.Width = (int)control.MaximumSize.Width;
                    }
                    else
                    {
                        control.Top = row * (tallestControlHeight + 4) + topMargin;
                    }

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

                    string topPercent = ((float)control.Top / (float)canvasSize.Height).ToString("#.#####");
                    string leftPercent = ((float)control.Left / (float)canvasSize.Width).ToString("#.#####");
                    locations.Append(string.Format("{0}:{1},", topPercent, leftPercent));

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

            field.Pattern = pattern.ToString().TrimEnd(new char[] { ',' });
            field.Locations = locations.ToString().TrimEnd(new char[] { ',' });
            field.SaveToDb();

            SetControlProperties(groupBox, field, canvasSize);

            List<Control> controls = new List<Control>();
            controls.Add(groupBox);
            return controls;
        }
        private List<Control> GetControls(TableBasedDropDownField field, Size canvasSize)
        {
            string displayMember = field.TextColumnName.Trim();
            string valueMember;

            if (field is DDLFieldOfCodes)
            {
                valueMember = field.CodeColumnName.Trim();
            }
            else if (field is DDListField)
            {
                valueMember = field.CodeColumnName.Trim();
            }
            else
            {
                valueMember = field.TextColumnName.Trim();
            }

            DragableComboBox comboBox = new DragableComboBox();
            comboBox.Width = defaultControlWidth;
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
                DataTable dataTable = field.GetSourceData();

                if (dataTable != null)
                {
                    DataBind(comboBox, dataTable, displayMember, valueMember);
                }
            }
            DragableLabel prompt = GetPrompt(comboBox, field, canvasSize);
            List<Control> controls = new List<Control>();
            controls.Add(prompt);
            controls.Add(comboBox);
            return controls;
        }
        /// <summary>
        /// Returns grid field control.
        /// </summary>
        /// <param name="field">The grid field to get.</param>
        /// <param name="canvasSize">The size of the view canvas.</param>
        /// <returns>A list of the grid's controls.</returns>
        private List<Control> GetControls(GridField field, Size canvasSize)
        {
            DragableGrid gridView = new DragableGrid();
            gridView.AutoGenerateColumns = false;
            gridView.Width = 160;
            gridView.Height = 150;
            SetControlProperties(gridView, field, canvasSize);
            gridView.ReadOnly = true;
            gridView.TabStop = false;
            gridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridView.ColumnHeadersDefaultCellStyle.Font = field.PromptFont; 

            DataTable columnTable = new DataTable("GridColumns");
            List<GridColumnBase> cols = new List<GridColumnBase>(field.Columns);
            cols.Sort(Util.SortByPosition);

            foreach (GridColumnBase col in cols)
            {
                if (!(col is PredefinedColumn))
                {
                    DataGridViewColumn column;

                    if(col is CheckboxColumn)
                    {
                        column = new DataGridViewCheckBoxColumn();
                    }
                    else
                    {
                        column = new DataGridViewTextBoxColumn();
                    }
                    
                    try
                    {
                        column.MinimumWidth = 25;

                        column.Name = col.Name;
                        column.HeaderText = col.Text;
                        column.ReadOnly = col.IsReadOnly;
                        column.DataPropertyName = col.Name;
                        column.DefaultCellStyle.Font = field.ControlFont;

                        if (col is PredefinedColumn)
                        {
                            column.ReadOnly = true;
                        }

                        column.Width = col.Width;
                    }
                    catch (InvalidOperationException ioEx)
                    {
                        MsgBox.ShowException(ioEx);
                    }
                    
                    gridView.Columns.Add(column);
                }
            }

            gridView.RowHeadersVisible = false;
            gridView.Refresh();

            DragableLabel prompt = GetPrompt(gridView, field, canvasSize);
            List<Control> controls = new List<Control>();
            controls.Add(prompt);
            controls.Add(gridView);

            return controls;
        }

        private List<Control> GetControls(GUIDField field, Size canvasSize)
        {
            DragableGUIDField textBox = new DragableGUIDField();

            SetControlProperties(textBox, field, canvasSize);
            textBox.ReadOnly = true;
            textBox.Text = field.SampleGuid();
            textBox.BorderStyle = borderStyle;
            DragableLabel prompt = GetPrompt(textBox, field, canvasSize);
            List<Control> controls = new List<Control>();
            controls.Add(prompt);
            controls.Add(textBox);
            return controls;
        }
        private List<Control> GetControls(ImageField field, Size canvasSize)
        {
            DragablePictureBox pictureBox = new DragablePictureBox();
            pictureBox.Width = 256;
            pictureBox.Height = 128;
            SetControlProperties(pictureBox, field, canvasSize);
            pictureBox.BorderStyle = BorderStyle.Fixed3D;
            pictureBox.BackgroundImage = global::Epi.Windows.MakeView.Properties.Resources.imageControlIcon;
            pictureBox.BackgroundImageLayout = ImageLayout.Center;
            DragableLabel prompt = GetPrompt(pictureBox, field, canvasSize);
            List<Control> controls = new List<Control>();
            controls.Add(prompt);
            controls.Add(pictureBox);
            return controls;
        }
        private List<Control> GetControls(CheckBoxField field, Size canvasSize)
        {
            DragableCheckBox checkBox = new DragableCheckBox();
            checkBox.Width = defaultControlWidth;
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
            return controls;
        }
        private List<Control> GetControls(CommandButtonField field, Size canvasSize)
        {
            DragableButton button = new DragableButton();
            SetControlProperties(button, field, canvasSize);
            
            button.Text = field.PromptText;
            button.FlatStyle = flatStyle;
            button.UseMnemonic = false;

            List<Control> controls = new List<Control>();
            controls.Add(button);
            return controls;
        }
        /// <summary>
        /// Returns Related view field control.
        /// </summary>
        /// <param name="field">The related view field to get.</param>
        /// <param name="canvasSize">The size of the view canvas.</param>
        /// <returns>A list of the related view's controls.</returns>
        private List<Control> GetControls(RelatedViewField field, Size canvasSize)
        {
            DragableButton button = new DragableButton();
            SetControlProperties(button, field, canvasSize);
            button.Text = field.PromptText;
            button.FlatStyle = flatStyle;
            button.UseMnemonic = false;
            List<Control> controls = new List<Control>();
            controls.Add(button);
            return controls;
        }
        
        private List<Control> GetControls(LabelField field, Size canvasSize)
        {
            DragableLabel label = new DragableLabel();
            label.Width = defaultControlWidth;
            SetControlProperties(label, field, canvasSize);
            label.Text = field.PromptText.Replace("\t", "    ");

            if (field.Page.FlipLabelColor)
            {
                label.ForeColor = Color.White;
            }
            
            List<Control> controls = new List<Control>();
            controls.Add(label);
            return controls;
        }

        private List<Control> GetControls(GroupField field, Size canvasSize)
        {
            DragableGroupBox group = new DragableGroupBox();
            SetControlProperties(group, field, canvasSize);
            group.ForeColor = Color.Black;
            group.BackColor = field.BackgroundColor;
            group.Text = field.PromptText == ""? "blank prompt - click to drag" : field.PromptText;
            group.AutoSize = true;
            List<Control> controls = new List<Control>();
            controls.Add(group);
            return controls;
        }

        private List<Control> GetControls(YesNoField field, Size canvasSize)
        {
            Epi.Windows.Controls.DragableComboBox comboBox = new DragableComboBox();
            comboBox.Width = defaultControlWidth;
            SetControlProperties(comboBox, field, canvasSize);
            comboBox.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox.FlatStyle = flatStyle;
            Configuration config = Configuration.GetNewInstance();
            comboBox.Items.Add(config.Settings.RepresentationOfYes);
            comboBox.Items.Add(config.Settings.RepresentationOfNo);
            DragableLabel prompt = GetPrompt(comboBox, field, canvasSize);
            List<Control> controls = new List<Control>();
            controls.Add(prompt);
            controls.Add(comboBox);
            return controls;
        }
        private List<Control> GetControls(InputTextBoxField field, Size canvasSize)
        {
            DragableTextBox textBox = new DragableTextBox();
            textBox.Width = defaultControlWidth;
            
            if (field is MultilineTextField)
            {
                textBox.Multiline = true;
                textBox.AcceptsTab = false;
            }

            if (field is PhoneNumberField)
            {
                textBox.Text = ((IPatternable)field).Pattern;
            }
            
            SetControlProperties(textBox, field, canvasSize);
            textBox.ReadOnly = true;
            textBox.BorderStyle = borderStyle;
  
            DragableLabel prompt = GetPrompt(textBox, field, canvasSize);
            List<Control> controls = new List<Control>();
            controls.Add(prompt);
            controls.Add(textBox);
            return controls;

        }

        private List<Control> GetControls(MirrorField field, Size canvasSize)
        {
            DragableTextBox textBox = new DragableTextBox();
            SetControlProperties(textBox, field, canvasSize);
            textBox.ReadOnly = true;
            textBox.BorderStyle = borderStyle;
            DragableLabel prompt = GetPrompt(textBox, field, canvasSize);
            List<Control> controls = new List<Control>();
            controls.Add(prompt);
            controls.Add(textBox);
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
            
            if (field is GUIDField)
            {
                ((GUIDField)field).HasTabStop = false;
            }

            if (field is IInputField && ((IInputField)field).IsReadOnly)
            {
                control.BackColor = Color.WhiteSmoke;
            }
            else if (control is DragableLabel == false)
            {
                control.BackColor = SystemColors.Window;
            }  

            control.TabStop = field.HasTabStop;
            control.TabIndex = (int)field.TabIndex;

            if ((field is OptionField || field is GridField) == false)
            { 
                control.Font = field.ControlFont;
            }

            control.ContextMenu = new ContextMenu();

            if (field.ControlHeightPercentage == 0 || field.ControlWidthPercentage == 0)
            {
                field.ControlHeightPercentage = 1.0 * ((Control)control).Height / canvasSize.Height;
                field.ControlWidthPercentage = 1.0 * ((Control)control).Width / canvasSize.Width;
                field.SaveToDb();
            }

            if (control is IFieldControl)
            {
                ((IFieldControl)control).Field = field;
            }
        }

        private DragableLabel GetPrompt(Control control, FieldWithSeparatePrompt field, Size canvasSize)
        {
            DragableLabel prompt = new DragableLabel();

            prompt.AutoSize = true;
            prompt.Font = field.PromptFont;
            prompt.Text = field.PromptText;
            prompt.Left = WinUtil.GetPromptLeft(field, canvasSize.Width);
            prompt.Top = WinUtil.GetPromptTop(field, canvasSize.Height);
            prompt.LabelFor = control;
            prompt.Field = field;
            prompt.UseMnemonic = false;

            if (field.Page.FlipLabelColor)
            {
                prompt.ForeColor = Color.White; 
            }
            
            return prompt;
        }

        #endregion

        #region Event Handlers
        
        /// <summary>
        /// Update the column width of the field after the width has changed on the grid column
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void textColumn_WidthChanged(object sender, EventArgs e)
        {
            GridField field = (GridField)GetAssociatedField(((DataGridTextBoxColumn)sender).DataGridTableStyle.DataGrid);
            for (int i = 0; i < field.Columns.Count; i++)
            {
                if (((DataGridTextBoxColumn)sender).MappingName == field.Columns[i].Name)
                {
                    field.Columns[i].Width = ((DataGridTextBoxColumn)sender).Width;
                    field.SaveToDb();
                    break;
                }
            }
        }

        #endregion
    }
}
