using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Epi.Fields;
using Epi.Windows;
using Epi.Windows.Controls;
using Epi.Windows.Enter.PresentationLogic;


namespace Epi.Windows.Enter
{
    public partial class Canvas
    {
        private bool _isDrawing = false;
        private bool _inMarkupMode = false;
        private Bitmap _imageBitmap;
        float _widthOrigin = 0f;
        float _heightOrigin = 0f;
        float _widthZoom = 0f;
        float _heightZoom = 0f;
        int? _click_x_last = null;
        int? _click_y_last = null;
        bool _enableTabToNextControl = false;
        public bool IsEventEnabled = false;
        public bool IsGotoPageField = false;
        public Field GotoPageField = null;
        public Control GotoPageControl = null;

        public bool EnableTabToNextControl
        {
            get { return _enableTabToNextControl; }
            set 
            { 
                _enableTabToNextControl = value; 
            }
        }

        private void control_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            ControlFactory factory = ControlFactory.Instance;            
            if (e.Button == MouseButtons.Left)
            {
                Field field = factory.GetAssociatedField((Control)sender);
                if (field != null)
                {
                    control = (Control)sender;
                    if (field is DateField || field is DateTimeField)
                    {
                            if (!factory.IsPopup)
                        {
                            MonthCalendar customMonthCalendar = new MonthCalendar();
                            customMonthCalendar.DateSelected += new DateRangeEventHandler(customMonthCalendar_DateSelected);
                            customMonthCalendar.Size = new Size(226, 160);
                            int panelbottom = control.Parent.Bottom;
                            int top = control.Location.Y; int right = control.Parent.Right; int left = control.Left;
                            if (panelbottom - top > 185)//checking to see if the Datefield at the bottom of the canvas
                            {
                                if (right - left < 228)//if field is at the right most part of the canvas.                                 
                                    customMonthCalendar.Location = new Point(right - 230, top + 25);
                                else
                                    customMonthCalendar.Location = new Point(control.Location.X - 10, top + 25);
                            }
                            else//if the datefield is very bottom of the canvas and no enough room for popup.
                                if (right - left < 228)  //if field is at the right and bottom most part of the canvas.                               
                                    customMonthCalendar.Location = new Point(right - 230, top - 165);
                            else
                                customMonthCalendar.Location = new Point(control.Location.X - 10, control.Top - 165);

                            try
                            {                               
                                control.Parent.Controls.Add(customMonthCalendar);
                                DateTime datetime ;
                                bool isdatetimeparse= DateTime.TryParse(control.Text, out datetime);
                                if (isdatetimeparse)
                                    customMonthCalendar.SetDate(datetime);
                                customMonthCalendar.Visible = true;
                                customMonthCalendar.BringToFront();
                                factory.IsPopup = true;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }
            }
        }
        void customMonthCalendar_DateSelected(object sender, DateRangeEventArgs e)
        {
            ControlFactory factory = ControlFactory.Instance;
            if (factory.IsPopup)
            {
                DateTime selection = e.Start;
                Field field = factory.GetAssociatedField(control);
                if (field is DateField)
                {
                    DateTime dateTime;
                    bool canParse = DateTime.TryParse(e.Start.Date.ToShortDateString(), out dateTime);
                    if (((DateField)field).IsInRange(dateTime) == false)
                    {
                        CloseFloatingForm(sender);
                        control.Focus();
                        return;
                    }
                    control.Text = e.Start.Date.ToShortDateString();
                }
                else if (field is DateTimeField)
                {
                    control.Text = e.Start.ToString();
                }
                CloseFloatingForm(sender);
                control.Focus();
            }
        }
        /// <summary>
        /// Handles the key press event of the control on the panel
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied key event parameters</param>
        private void control_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            for (int ix = ((Control)sender).Parent.Controls.Count - 1; ix >= 0; ix--)
                if (((Control)sender).Parent.Controls[ix] is MonthCalendar)
                {                  
                    ((Control)sender).Parent.Controls.Remove(((Control)sender).Parent.Controls[ix]);
                    ControlFactory factory = ControlFactory.Instance;
                    factory.IsPopup = false;
                }

            if (e.KeyCode == Keys.Tab || e.KeyCode == Keys.Enter)
            {
                if (this.CloseFieldEvent != null)
                {
                    ControlFactory factory = ControlFactory.Instance;

                    if (sender is RadioButton)
                    {
                        sender = ((Control)sender).Parent;
                    }

                    Field field = factory.GetAssociatedField((Control)sender);

                    if (field is MultilineTextField && sender is TextBoxBase && e.KeyCode != Keys.Tab)
                    {
                        int pos = ((TextBoxBase)sender).SelectionStart;
                        ((TextBoxBase)sender).Text = ((TextBoxBase)sender).Text + "\r\n";
                        ((TextBoxBase)sender).SelectionStart = pos;
                        return;
                    }

                    this.EnableTabToNextControl = true;
                    this.CloseFieldEvent(sender, new CloseFieldEventArg(field, true, "control_KeyDown"));
                    e.SuppressKeyPress = true;
                }
                e.Handled = true;
            }
            else
            {
                ControlFactory factory = ControlFactory.Instance;
                Field field = factory.GetAssociatedField((Control)sender);

                if (field is DateTimeField)
                {
                    if (((Control)sender).Text == ((DateTimeField)field).Watermark)
                    {
                        ((Control)sender).Text = "";
                        ((Control)sender).ForeColor = System.Drawing.Color.Gray;
                    }
                    else
                    {
                        ((Control)sender).ForeColor = System.Drawing.Color.Black;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the KeyPress event of the control to uppercase text in a multiline textbox
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void control_KeyPress(object sender, KeyPressEventArgs e)
        {
            ControlFactory factory = ControlFactory.Instance;
            Field field = factory.GetAssociatedField((Control)sender);

            if (field is UpperCaseTextField)
            {
                if (Char.IsLetter(e.KeyChar))
                {
                    //save the current caret position
                    int pos = ((TextBox)sender).SelectionStart;
                    int selectionLength = ((TextBox)sender).SelectionLength;

                    if (((TextBox)sender).SelectionLength > 0)
                    {
                        // Some text is highlighted so we need to delete it first

                        StringBuilder oldText = new StringBuilder();
                        oldText.Append(((TextBox)sender).Text);
                        oldText.Remove(pos, selectionLength);
                        ((TextBox)sender).Text = oldText.ToString();
                    }

                    if (((TextBox)sender).MaxLength > 0 &&
                        ((TextBox)sender).Text.Length == ((TextBox)sender).MaxLength)
                    {
                        // The textbox is already at its limit, DO NOT ADD ANOTHER CHARACTER
                        return;
                    }

                    //insert the upper case character
                    ((TextBox)sender).Text = ((TextBox)sender).Text.Insert(pos, Char.ToUpper(e.KeyChar).ToString());

                    //and update the current caret position
                    ((TextBox)sender).SelectionStart = pos + 1;
                    e.Handled = true;
                }
            }
            else if (field is NumberField)
            {
                Control control = control = ((MaskedTextBox)sender);

                System.Globalization.NumberFormatInfo numberFormatInfo = System.Globalization.CultureInfo.CurrentCulture.NumberFormat;
                string decimalSeparator = numberFormatInfo.NumberDecimalSeparator;
                string negativeSign = numberFormatInfo.NegativeSign;    // we don't technically need these for the input boxes, so I may remove them later

                string keyInput = e.KeyChar.ToString();

                if (keyInput.Equals("\t"))
                {
                    e.Handled = true;
                }
                else if (Char.IsDigit(e.KeyChar) || e.KeyChar == '\b' || Char.IsControl(e.KeyChar))
                {
                    // Digits and backspaces are okay
                }
                else if (keyInput.Equals(decimalSeparator))
                {
                    // Decimal separator is okay, but we want only one of them.
                    int separators = 0;
                    for (int i = 0; i < control.Text.Length; i++)
                    {
                        if (control.Text[i].ToString() == decimalSeparator)
                        {
                            separators++;
                        }
                    }

                    // Note: The first separator that is added does not get counted here. Anything more than zero means we have more than 1 separator; thus consume the event.
                    if (separators > 0)
                    {
                        e.Handled = true;
                    }

                }
                // user presses key to add a negative sign to the text box...
                else if (keyInput.Equals(negativeSign))
                {
                    // Check to see if the negative sign is in the right spot, assuming there is one.
                    if (control.Text.Length > 0 && control.Text.Substring(0, 1) == numberFormatInfo.NegativeSign)
                    {
                        // we detected there's already a negative sign, so we don't want to add another one. Consume the event.
                        e.Handled = true;
                    }
                }
                else
                {
                    e.Handled = true; // Invalid key, consume it
                }
            }
            else
            {
                e.Handled = true;
            }         
        }

        /// <summary>
        /// Previews the key pressed to see if is an acceptable input key
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void control_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
            {
                if (UserKeyboardInput.IsInputKey(e.KeyData))
                {
                    e.IsInputKey = true;
                }
            }
        }

        /// <summary>
        /// Handles the Change event of the selection of a combobox control
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void control_SelectionChangeCommitted(object sender, EventArgs e)
        {
            AssignLinkedFieldValues(sender as Control);
            SetDirtyFlag();
        }

        /// <summary>
        /// Sets control state when the text has changed
        /// </summary>
        /// <param name="sender">.NET supplied object</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void control_TextChanged(object sender, EventArgs e)
        {
            ControlFactory factory = ControlFactory.Instance;
            Field field = factory.GetAssociatedField((Control)sender);

            if (field is UpperCaseTextField)
            {
                TextBox control = (TextBox)sender;
                control.Text = control.Text.ToUpper();
            }
            else if (field is NumberField)
            {
                MaskedTextBox control = (MaskedTextBox)sender;
                if (control.Text.Length > 0
                    && control.Text[0].ToString() != System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator
                    && control.Text[0].ToString() != System.Globalization.NumberFormatInfo.CurrentInfo.NegativeSign)
                {
                    string TestNumber = control.Text.Replace(" ", "");

                    if (!TestNumber.Contains("-"))
                    {
                        if (!string.IsNullOrEmpty(TestNumber) && TestNumber != ".")
                        {
                            float temp;
                            try
                            {
                                if (!float.TryParse(TestNumber, out temp))// TestNumber.Length >= 12 
                                {
                                    MsgBox.ShowInformation(SharedStrings.MAX_NONE_MASK_DIGITS_EXCEEDED);
                                    control.SelectAll();
                                }
                                else
                                {
                                    temp = float.Parse(TestNumber, System.Globalization.NumberStyles.Any);
                                }
                            }
                            catch (FormatException)
                            {
                                MsgBox.ShowInformation(SharedStrings.ENTER_VALID_NUMBER);
                                control.SelectAll();
                            }
                        }
                    }
                    else
                    {
                        MsgBox.ShowInformation(SharedStrings.ENTER_VALID_NUMBER);
                        control.SelectAll();
                    }
                }
            }
            else if (field is DDLFieldOfCodes)
            {
                if (((ComboBox)sender).Items.Contains(((ComboBox)sender).Text))
                {
                    if (!GuiMediator.Instance.IsComboTextAssign)
                    {
                        AssignLinkedFieldValues(sender as Control);
                        GuiMediator.Instance.FieldChangeHandler(sender, field);
                    }
                }
            }
            else if (field is DDLFieldOfCommentLegal || field is DDLFieldOfLegalValues || field is YesNoField)
            {
                if (!GuiMediator.Instance.IsComboTextAssign)
                if (((ComboBox)sender).Items.Contains(((ComboBox)sender).Text))
                {                     
                    GuiMediator.Instance.FieldChangeHandler(sender, field);
                }
            }
            else if (field is DateTimeField)
            {
                string pattern = ((DateTimeField)field).Watermark;

                if (((Control)sender).Text == "")
                {
                    ((Control)sender).Text = pattern;
                    ((TextBoxBase)sender).SelectionStart = 0;
                    ((TextBoxBase)sender).SelectionLength = 0;
                }
                else if (((Control)sender).Text.Contains(pattern) && ((Control)sender).Text != pattern)
                {
                    string displayText = ((Control)sender).Text.Replace(pattern, "");
                    ((Control)sender).Text = displayText;
                    ((TextBoxBase)sender).SelectionStart = 1;
                    ((TextBoxBase)sender).SelectionLength = 0;
                }

                if (((Control)sender).Text == pattern)
                {
                    ((Control)sender).ForeColor = System.Drawing.Color.Gray;
                }
                else
                {
                    ((Control)sender).ForeColor = System.Drawing.Color.Black;
                }
            }

            SetDirtyFlag();
        }

        /// <summary>
        /// Handles the check changed event of a check box.
        /// </summary>
        /// <param name="sender">.NET supplied object</param>
        /// <param name="e">.NET supplied event parameters</param>
        public void control_CheckChanged(object sender, EventArgs e)
        {
            Control control = sender as Control;
            //ControlFactory factory = ControlFactory.Instance;
            //Field field = factory.GetAssociatedField(control);

            if (control is CheckBox || control is RadioButton)
            {
                //DirtyFieldEvent(sender, e);
                SetDirtyFlag();
            }
        }

        /// <summary>
        /// Handles the click event of the button control.
        /// </summary>
        /// <param name="sender">.NET supplied object</param>
        /// <param name="e">.NET supplied event parameters</param>
        public void control_Click(object sender, EventArgs e)
        {                     
            Control control = sender as Control;
            ControlFactory factory = ControlFactory.Instance;
            Field field = factory.GetAssociatedField(control);          
            // Erik K 9/1/2010 - Changed this so that the masked text boxes (numeric fields, date
            // fields) always show the cursor on the first position rather than where the click
            // event occured.
            if (control is MaskedTextBox)
            {
                MaskedTextBox maskedTBox = ((MaskedTextBox)control);
                if (maskedTBox.Text.Contains(" ") || maskedTBox.Text.Length == 0)
                {
                    ((MaskedTextBox)control).Select(0, 0);
                }
            }
            else if (field is ImageField)
            {
                this.ClickFieldEvent(control as PictureBox, new ClickFieldEventArg(field));
            }
            else if (this.ClickFieldEvent != null)
            {
                this.ClickFieldEvent(this, new ClickFieldEventArg(field));
            }
        }


        /// <summary>
        /// Handles the Enter event for controls
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void control_Enter(object sender, EventArgs e)
        {          
                          
             ControlFactory factory = ControlFactory.Instance;
            if (factory.IsPopup)
            {
                for (int ix = ((Control)sender).Parent.Controls.Count - 1; ix >= 0; ix--)
                    if (((Control)sender).Parent.Controls[ix] is MonthCalendar)
                    {
                        ((Control)sender).Parent.Controls.Remove(((Control)sender).Parent.Controls[ix]);                     
                        factory.IsPopup = false;
                    }
            }
            if (this.GotoFieldEvent != null)
            {
                Field field = ControlFactory.Instance.GetAssociatedField((Control)sender);
                GoToFieldEventArgs args = new GoToFieldEventArgs(field);
                this.GotoFieldEvent(sender, args);
            }
                    
        }

        ///<summary>
        /// Handles the Leave event for controls
        ///</summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void control_Leave(object sender, EventArgs e)
        {
            ControlFactory factory = ControlFactory.Instance;          
            Field field = factory.GetAssociatedField((Control)sender);
            if (field is NumberField)
            {
                MaskedTextBox control = ((MaskedTextBox)sender);
                int digits = 0;
                int symbols = 0;
                for (int i = 0; i < control.Text.Length; i++)
                {
                    if (control.Text[i].ToString() == System.Globalization.NumberFormatInfo.CurrentInfo.NegativeSign ||
                    control.Text[i].ToString() == System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                    {
                        symbols++;
                    }
                    else if (Char.IsDigit(control.Text[i]))
                    {
                        digits++;
                    }
                }

                if (digits == 0 && symbols > 0)
                {
                    control.Clear();
                }
            }
            else if (field is DDListField)
            {
            }
            else if (field is DateField)
            {
                if (!factory.IsPopup)
                {
                    DateTime dateTimeReturn = new DateTime();
                    TextBox control = ((TextBox)sender);
                    bool canParse = DateTime.TryParse(control.Text, out dateTimeReturn);

                    if (canParse)
                    {
                        ((IInputField)field).CurrentRecordValueObject = dateTimeReturn;
                        control.Text = ((DateField)field).CurrentRecordValueString;
                    }
                    else
                    {
                        control.Clear();
                    }
                }
                else return;
            }
            else if (field is TimeField)
            {
                DateTime dateTimeReturn = new DateTime();
                TextBox control = ((TextBox)sender);
                bool canParse = DateTime.TryParse(control.Text, out dateTimeReturn);

                if (canParse)
                {
                    ((IInputField)field).CurrentRecordValueObject = dateTimeReturn;
                    control.Text = ((TimeField)field).CurrentRecordValueString;
                }
                else
                {
                    control.Clear();
                }
            }
            else if (field is DateTimeField)
            {
                if (!factory.IsPopup)
                {
                    DateTime dateTimeReturn = new DateTime();
                    TextBox control = ((TextBox)sender);
                    bool canParse = DateTime.TryParse(control.Text, out dateTimeReturn);

                    if (canParse)
                    {
                        ((IInputField)field).CurrentRecordValueObject = dateTimeReturn;
                        control.Text = ((DateTimeField)field).CurrentRecordValueString;
                    }
                    else
                    {
                        control.Clear();
                    }
                }
                else return;
            }

            if (this.CloseFieldEvent != null)
            {
                this.CloseFieldEvent(sender, new CloseFieldEventArg(field, false, "control_Leave"));
            }
        }


        /// <summary>
        /// Validate controls's check code
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void control_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (GuiMediator.Instance.IsValidData((Control)sender))
            {
                if (GuiMediator.Instance.IsDirty)
                {
                    GuiMediator.Instance.SetFieldData();
                }
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Occurs when an external data-parsing or validation operation throws an exception, 
        /// or when an attempt to commit data to a data source fails. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void control_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            DataGridView grid = (DataGridView)sender;

            switch (e.Context)
            {
                case DataGridViewDataErrorContexts.CurrentCellChange:
                    MessageBox.Show("Cell change");
                    break;
                case DataGridViewDataErrorContexts.Parsing:
                    MessageBox.Show("Parsing error");
                    break;
                case DataGridViewDataErrorContexts.LeaveControl:
                    MessageBox.Show("Leave control error");
                    break;
                case DataGridViewDataErrorContexts.ClipboardContent:
                    MessageBox.Show("Copying to clipboard error");
                    break;
                case DataGridViewDataErrorContexts.Formatting:
                    MessageBox.Show("Formatting error");
                    break;
                case DataGridViewDataErrorContexts.Commit:
                    break;
                default:
                    int columnNumber = grid.CurrentCell.ColumnIndex;
                    Type dataType = grid.Columns[columnNumber].ValueType;
                    string message;
                    if(dataType.Name=="Double")
                       message = string.Format("The value [{0}] can not be saved as a {1}.", grid.CurrentCell.EditedFormattedValue.ToString(), "number");
                    else
                    message = string.Format("The value [{0}] can not be saved as a {1}.", grid.CurrentCell.EditedFormattedValue.ToString(), dataType.Name);
                    MessageBox.Show(message);
                    break;
            }
        }

        /// <summary>
        /// New Row Needed
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied DataGridView Row event parameters</param>
        private void control_RowAdded(object sender, DataGridViewRowEventArgs e)
        {
            if (this.DataGridRowAddedEvent != null)
            {
                this.DataGridRowAddedEvent(sender, e);
            }
            SetDirtyFlag();
        }

        /// <summary>
        /// Row Validated
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied DataGridView Row event parameters</param>
        private void control_RowValidated(object sender, DataGridViewCellEventArgs e)
        {
            //ControlFactory factory = ControlFactory.Instance;
            //Field field = factory.GetAssociatedField((Control)sender);
            //DataSet dataSet = new DataSet(view.Name + field.Name);
            //DataRow sourceDataRow = null, dataRow = null;
            //DataTable table = null;

            //if ((Control)sender is DataGridView)
            //{
            //    bool updateRow = false;
            //    DataGridView dgView = (DataGridView)sender;
            //    dgView.Rows[e.RowIndex].Selected = true;
            //    foreach (DataGridViewRow row in dgView.SelectedRows)
            //    {
            //        if ((DataRowView)row.DataBoundItem != null)
            //        {
            //            sourceDataRow = ((DataRowView)row.DataBoundItem).Row;
            //            if (Util.IsEmpty(table))
            //            {
            //                table = sourceDataRow.Table.Clone();
            //            }

            //            dataRow = table.NewRow();
            //            CopyDataRow(sourceDataRow, dataRow);
            //            table.Rows.Add(dataRow);
            //        }
            //    }

            //    if (table != null)
            //    {
            //        dataSet.Tables.Add(table);
            //        SaveRecord(view);
            //        try
            //        {
            //            if (!(String.IsNullOrEmpty(this.view.UniqueKeyField.CurrentRecordValueString)) && updateRow)
            //            {
            //                if (view.GetProject().CollectedData.SaveGridRecord(view, int.Parse(this.view.UniqueKeyField.CurrentRecordValueString), (GridField)field, table) == 1)
            //                {
            //                    DataTable tempGrid = view.GetProject().CollectedData.GetGridTableData(this.view, (GridField)field);
            //                    dgView.SelectedRows[0].Cells[ColumnNames.UNIQUE_KEY].Value = tempGrid.Rows[tempGrid.Rows.Count - 1][ColumnNames.UNIQUE_KEY].ToString();
            //                    tempGrid.Dispose();
            //                }
            //            }
            //        }
            //        catch (ApplicationException ex)
            //        {
            //            Logger.Log(DateTime.Now + ":  " + ex.Message);
            //        }
            //    }
            //}
            SetDirtyFlag();
        }


        private void gridView_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
            MaskedTextBoxEditingControl control = ((MaskedTextBoxEditingControl)sender);
            PatternableColumn column = ((PatternableColumn)control.GridColumn);
            control.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
            if (!control.MaskFull && !String.IsNullOrEmpty(control.Text))
            {
                control.TextMaskFormat = MaskFormat.IncludeLiterals;
                if (column.GridColumnType.Equals(MetaFieldType.Date))
                {
                    DateTime dateOrTime;
                    string format = Epi.Data.Services.AppData.Instance.DataPatternsDataTable.GetExpressionByMask(control.Mask.ToString(), column.Pattern);
                    bool result = DateTime.TryParseExact(control.Text, format, System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None, out dateOrTime);
                    if (!result)
                    {
                        MsgBox.ShowError(SharedStrings.ENTER_VALID_DATE);
                        control.Text = String.Empty;
                        control.Focus();
                    }
                }
                else if (column.GridColumnType.Equals(MetaFieldType.Number))
                {
                    control.Text = GuiMediator.Instance.FormatNumberInput(control.Text, control.Mask);

                }
                else if (column.GridColumnType.Equals(MetaFieldType.PhoneNumber))
                {
                    MsgBox.ShowError(SharedStrings.ENTER_VALID_PHONE_NUMBER);
                    control.Text = String.Empty;
                    control.Focus();
                }
            }
        }

        private void gridColumnControl_Validating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            DataGridView grid = (DataGridView)sender;
            if (grid.EditingControl is MaskedTextBoxEditingControl)
            {
                MaskedTextBoxEditingControl control = ((MaskedTextBoxEditingControl)grid.EditingControl);
                PatternableColumn column = ((PatternableColumn)control.GridColumn);
                control.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
                if (!String.IsNullOrEmpty(control.Text))
                {
                    if (!control.MaskFull)
                    {
                        control.TextMaskFormat = MaskFormat.IncludeLiterals;
                        if (column.GridColumnType.Equals(MetaFieldType.Date))
                        {
                            MsgBox.ShowError(SharedStrings.ENTER_VALID_DATE);
                            e.Cancel = true;
                        }
                        else if (column.GridColumnType.Equals(MetaFieldType.Number))
                        {
                            control.Text = GuiMediator.Instance.FormatNumberInput(control.Text, control.Mask);
                        }
                        else if (column.GridColumnType.Equals(MetaFieldType.PhoneNumber))
                        {
                            MsgBox.ShowError(SharedStrings.ENTER_VALID_PHONE_NUMBER);
                            e.Cancel = true;
                        }
                    }
                    else
                    {
                        control.TextMaskFormat = MaskFormat.IncludeLiterals;
                        if (control.MaskFull)
                        {
                            if (column.GridColumnType.Equals(MetaFieldType.Date))
                            {
                                DateTime dateOrTime;
                                bool canParse = DateTime.TryParse(control.Text, out dateOrTime);

                                if (canParse == false)
                                {
                                    MsgBox.ShowError(SharedStrings.ENTER_VALID_DATE);
                                    e.Cancel = true;
                                }
                                else
                                {
                                    if (dateOrTime.CompareTo(new DateTime(1800, 1, 1)) < 0)
                                    {
                                        MsgBox.ShowError(SharedStrings.ENTER_VALID_DATE);
                                        e.Cancel = true;
                                    }
                                    else
                                    {
                                        DateColumn checkColumn = (DateColumn)column;

                                        if (!string.IsNullOrEmpty(checkColumn.Lower) && !string.IsNullOrEmpty(checkColumn.Upper))
                                        {
                                            if (dateOrTime.CompareTo(DateTime.Parse(checkColumn.Lower)) < 0 || dateOrTime.CompareTo(DateTime.Parse(checkColumn.Upper)) > 0)
                                            {
                                                MsgBox.ShowError(String.Format(SharedStrings.INVALID_DATE_RANGE, checkColumn.Lower, checkColumn.Upper));
                                                e.Cancel = true;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (column.GridColumnType.Equals(MetaFieldType.Number))
                            {
                                string format = Epi.Data.Services.AppData.Instance.DataPatternsDataTable.GetExpressionByMask(control.Mask.ToString(), column.Pattern);
                                bool result;
                                double doubleNumber;
                                result = Double.TryParse(((MaskedTextBox)control).Text, out doubleNumber);
                                if (result)
                                {
                                    NumberColumn checkedColumn = (NumberColumn)column;
                                    if (!string.IsNullOrEmpty(checkedColumn.Lower) && !string.IsNullOrEmpty(checkedColumn.Upper))
                                    {
                                        if ((double.Parse(checkedColumn.Lower) > doubleNumber) || (doubleNumber > double.Parse(checkedColumn.Upper)))
                                        {
                                            MsgBox.ShowError(String.Format(SharedStrings.VALUE_NOT_IN_RANGE, checkedColumn.Lower, checkedColumn.Upper));
                                            e.Cancel = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void dataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            DataGridView grid = (DataGridView)sender;
            grid.EditingControl.KeyPress += new KeyPressEventHandler(EditingControl_KeyPress);
            if (grid.EditingControl is MaskedTextBoxEditingControl)
            {
                MaskedTextBoxEditingControl maskedtextEditingcontrol = ((MaskedTextBoxEditingControl)grid.EditingControl);

                PatternableColumn column = ((PatternableColumn)maskedtextEditingcontrol.GridColumn);

                if (column.GridColumnType.Equals(MetaFieldType.Date) || column.GridColumnType.Equals(MetaFieldType.DateTime))
                {
                    control = (Control)sender;
                    PopupCalendar(sender, maskedtextEditingcontrol.Location);
                }

            }
        }

        private void EditingControl_KeyPress(object sender, KeyPressEventArgs e)
        {
            ControlFactory factory = ControlFactory.Instance;
            if (factory.IsPopup)
            {
                if (((Control)sender) is DataGridViewTextBoxEditingControl)
                {
                    for (int ix = ((DataGridViewTextBoxEditingControl)((Control)sender)).EditingControlDataGridView.Parent.Controls.Count - 1; ix >= 0; ix--)
                        if (((DataGridViewTextBoxEditingControl)((Control)sender)).EditingControlDataGridView.Parent.Controls[ix] is MonthCalendar)
                        {
                            ((DataGridViewTextBoxEditingControl)((Control)sender)).EditingControlDataGridView.Parent.Controls.Remove(((DataGridViewTextBoxEditingControl)((Control)sender)).EditingControlDataGridView.Parent.Controls[ix]);
                            factory.IsPopup = false;
                        }
                }

                else if (((Control)sender) is DataGridViewComboBoxEditingControl)
                {
                    for (int ix = ((DataGridViewComboBoxEditingControl)((Control)sender)).EditingControlDataGridView.Parent.Controls.Count - 1; ix >= 0; ix--)
                        if (((DataGridViewComboBoxEditingControl)((Control)sender)).EditingControlDataGridView.Parent.Controls[ix] is MonthCalendar)
                        {
                            ((DataGridViewComboBoxEditingControl)((Control)sender)).EditingControlDataGridView.Parent.Controls.Remove(((DataGridViewComboBoxEditingControl)((Control)sender)).EditingControlDataGridView.Parent.Controls[ix]);
                            factory.IsPopup = false;
                        }
                }

                else if (((Control)sender) is MaskedTextBoxEditingControl)
                {
                    for (int ix = ((MaskedTextBoxEditingControl)((Control)sender)).EditingControlDataGridView.Parent.Controls.Count - 1; ix >= 0; ix--)
                        if (((MaskedTextBoxEditingControl)((Control)sender)).EditingControlDataGridView.Parent.Controls[ix] is MonthCalendar)
                        {
                            ((MaskedTextBoxEditingControl)((Control)sender)).EditingControlDataGridView.Parent.Controls.Remove(((MaskedTextBoxEditingControl)((Control)sender)).EditingControlDataGridView.Parent.Controls[ix]);
                            factory.IsPopup = false;
                        }
                }
            }
        }

        private void PopupCalendar(object sender, Point location)
        {
            ControlFactory factory = ControlFactory.Instance;
            if (!factory.IsPopup)
            {
                DataGridView grid = (DataGridView)control;
                MonthCalendar customMonthCalendar = new MonthCalendar();
                customMonthCalendar.DateSelected += new DateRangeEventHandler(dataGridview_customMonthCalendar_DateSelected);
                customMonthCalendar.Size = new Size(226, 160);
                int panelbottom = ((Control)sender).Parent.Bottom; int panelright = ((Control)sender).Parent.Right;                
                int top = grid.GetCellDisplayRectangle(grid.SelectedCells[0].ColumnIndex, grid.SelectedCells[0].RowIndex, true).Y;
                int x = grid.GetCellDisplayRectangle(grid.SelectedCells[0].ColumnIndex, grid.SelectedCells[0].RowIndex, true).X;
                if (panelbottom - grid.Top > 165)
                {
                    if(panelright-grid.Left<228)
                        customMonthCalendar.Location = new Point((grid.Location.X + x) - 226, grid.Location.Y + top + 25);
                    else
                        customMonthCalendar.Location = new Point((grid.Location.X + x) - 10, grid.Location.Y + top + 25);
                }
                else
                    if (panelright - grid.Left < 228)
                        customMonthCalendar.Location = new Point((grid.Location.X + x) - 226, grid.Location.Y + top + 25);
                    else
                    customMonthCalendar.Location = new Point((grid.Location.X + x) - 10, (grid.Location.Y + top) - 165);
                ((Control)sender).Parent.Controls.Add(customMonthCalendar);
                DateTime datetime;
                bool isdatetimeparse = DateTime.TryParse(grid.SelectedCells[0].Value.ToString(), out datetime);               
                if (isdatetimeparse)
                    customMonthCalendar.SetDate(datetime);
                customMonthCalendar.Visible = true;
                customMonthCalendar.BringToFront();
                factory.IsPopup = true;
            }
        }

        private void dataGridview_customMonthCalendar_DateSelected(object sender, DateRangeEventArgs e)
        {
            DataGridView grid = (DataGridView)control;
            if (grid.SelectedCells[0].EditType == typeof(MaskedTextBoxEditingControl))
            {
                grid.CurrentCell = grid.SelectedCells[0];
                grid.BeginEdit(true);
                MaskedTextBoxEditingControl control1 = ((MaskedTextBoxEditingControl)grid.EditingControl);
                if (control1 != null)
                {
                    PatternableColumn column = ((PatternableColumn)control1.GridColumn);
                    if (column.GridColumnType.Equals(MetaFieldType.Date))
                        control1.Text = e.Start.Date.ToShortDateString();
                    else if (column.GridColumnType.Equals(MetaFieldType.DateTime))
                        control1.Text = e.Start.Date.ToString();
                    grid.EndEdit();
                }
            }
            CloseFloatingForm(sender);
        }

        private void CloseFloatingForm(object sender)
        {
            MonthCalendar monthCalendarCustomized = (MonthCalendar)sender;
            Form floatingForm = monthCalendarCustomized.FindForm();
            monthCalendarCustomized.Parent.Controls.Remove(monthCalendarCustomized);
            ControlFactory factory = ControlFactory.Instance;
            factory.IsPopup = false;
        }

        public void image_MouseHover(object sender, EventArgs e)
        {
            if (sender is PictureBox == false) return;

            PictureBox pictureBox = ((PictureBox)sender);

            if (pictureBox.Image == null)
            {
                ((PictureBox)sender).Image = Epi.Enter.Properties.Resources.imageField_Icons;
                ((PictureBox)sender).SizeMode = PictureBoxSizeMode.Normal;
            }
            else
            {
                if (pictureBox.SizeMode == PictureBoxSizeMode.Normal)
                {
                    pictureBox.Image = OverlayImage
                        (
                            pictureBox.Image,
                            Epi.Enter.Properties.Resources.imageField_Icons,
                            pictureBox.Image.Width,
                            pictureBox.Image.Height
                        );
                }
                else // ZOOM
                {
                    pictureBox.Image = OverlayImage
                        (
                            pictureBox.Image,
                            Epi.Enter.Properties.Resources.imageField_Icons,
                            pictureBox.Width,
                            pictureBox.Height
                        );
                }
            }
        }

        // resise the base image to fit picturebox
        private Image OverlayImage(Image baseImage, Image overlayImage, int maxWidth, int maxHeight)
        {
            if (baseImage == null || overlayImage == null || maxWidth < 16 || maxHeight < 16)
            {
                return null;
            }

            float imageWidth = baseImage.PhysicalDimension.Width;
            float imageHeight = baseImage.PhysicalDimension.Height;
            float percentage = maxWidth / imageWidth;
            float newWidth = imageWidth * percentage;
            float newHeight = imageHeight * percentage;

            if (newHeight > maxHeight)
            {
                percentage = maxHeight / newHeight;
                newWidth = newWidth * percentage;
                newHeight = newHeight * percentage;
            }

            using (Bitmap b = new Bitmap((int)newWidth, (int)newHeight))
            {
                using (Graphics g = Graphics.FromImage(b))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                    g.DrawImage(baseImage, new Rectangle(0, 0, b.Width, b.Height));
                    int width = (int)(((float)overlayImage.Width) * 1.0);
                    int height = (int)(((float)overlayImage.Height) * 1.0);
                    g.DrawImage(overlayImage, new Rectangle(0, 0, width, height));

                    Image newImage = Image.FromHbitmap(b.GetHbitmap());

                    return newImage;
                }
            }
        }

        public void image_MouseLeave(object sender, EventArgs e)
        {
            if (sender is PictureBox == false) return;

            ControlFactory factory = ControlFactory.Instance;

            ImageField field = (ImageField)factory.GetAssociatedField((Control)sender);

            if (_imageBitmap != null)
            {
                byte[] byteArray = new byte[0];
                ImageConverter converter = new ImageConverter();
                byteArray = (byte[])converter.ConvertTo(_imageBitmap, typeof(byte[]));
                field.CurrentRecordValue = byteArray;
                _imageBitmap = null;
            }

            GetFieldDataIntoControl(field, (PictureBox)sender);

            if (field.ShouldRetainImageSize)
            {
                ((PictureBox)sender).SizeMode = PictureBoxSizeMode.Normal;
            }
            else
            {
                ((PictureBox)sender).SizeMode = PictureBoxSizeMode.Zoom;
            }

            _inMarkupMode = false;
            _isDrawing = false;
            this.Cursor = Cursors.Default;
        }

        public void image_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender is PictureBox == false)
            {
                return;
            }

            PictureBox pictureBox = ((PictureBox)sender);

            System.Reflection.PropertyInfo pInfo = pictureBox.GetType().GetProperty("ImageRectangle",
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            Rectangle rectangle = (Rectangle)pInfo.GetValue(pictureBox, null);

            _widthOrigin = rectangle.X;
            _heightOrigin = rectangle.Y;

            if (pictureBox.Image != null)
            {
                _widthZoom = pictureBox.Image.PhysicalDimension.Width / rectangle.Width;
                _heightZoom = pictureBox.Image.PhysicalDimension.Height / rectangle.Height;
            }
            int click_x = e.X;
            int click_y = e.Y;

            int click_x_overImage = ((int)
            (
                ((float)e.X) - _widthOrigin

            ));

            int click_y_overImage = ((int)
            (
                ((float)e.Y) - _heightOrigin
            ));

            click_x = ((int)
            (
                ((float)click_x_overImage) * _widthZoom
            ));

            click_y = ((int)
            (
                ((float)click_y_overImage) * _heightZoom
            ));

            ControlFactory factory = ControlFactory.Instance;
            ImageField imageField = (ImageField)factory.GetAssociatedField((Control)sender);

            if (_inMarkupMode == true)
            {
                _isDrawing = true;

                if (((PictureBox)sender).Image != null)
                {
                    _imageBitmap = new Bitmap(((PictureBox)sender).Image);
                }

                System.IO.MemoryStream cursorMemoryStream = new System.IO.MemoryStream(Epi.Enter.Properties.Resources.MarkupCursor);
                this.Cursor = new Cursor(cursorMemoryStream);
            }
            else if (click_y < 47 && 47 <= click_x && click_x < 93) // MARKUP
            {
                _inMarkupMode = true;

                if (imageField.CurrentRecordValueObject != null)
                {
                    GetFieldDataIntoControl(imageField, (PictureBox)sender);
                }
                else
                {
                    ((PictureBox)sender).Image = null;
                }

                if (imageField.ShouldRetainImageSize)
                {
                    ((PictureBox)sender).SizeMode = PictureBoxSizeMode.Normal;
                }
                else
                {
                    ((PictureBox)sender).SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
            else if (click_y < 47 && 93 <= click_x && click_x < 140) // DELETE
            {
                if (imageField.CurrentRecordValueObject != null)
                {
                    DialogResult clear = MsgBox.ShowQuestion("Would you like to clear the current image?", MessageBoxButtons.YesNoCancel);

                    if (clear == DialogResult.Yes)
                    {
                        imageField.CurrentRecordValueObject = null;
                        GetFieldDataIntoControl(imageField, (PictureBox)sender);
                        GuiMediator.Instance.IsDirty = true;
                        return;
                    }
                }
            }
            else
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image Files |*.bmp; *.jpg; *.gif";
                openFileDialog.FilterIndex = 1;
                openFileDialog.Multiselect = false;
                DialogResult result = openFileDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName.Trim();
                    byte[] imageAsBytes = Util.GetByteArrayFromImagePath(filePath);
                    imageField.CurrentRecordValue = imageAsBytes;
                    GuiMediator.Instance.IsDirty = true;

                    GetFieldDataIntoControl(imageField, (PictureBox)sender);
                }
            }
        }

        public void image_MouseUp(object sender, MouseEventArgs e)
        {
            if (sender is PictureBox == false)
            {
                return;
            }

            _click_x_last = null;
            _click_y_last = null;
            _isDrawing = false;
        }

        public void image_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                int click_x = e.X;
                int click_y = e.Y;

                int click_x_overImage = ((int)
                (
                    ((float)e.X) - _widthOrigin

                ));

                int click_y_overImage = ((int)
                (
                    ((float)e.Y) - _heightOrigin
                ));

                click_x = ((int)
                (
                    ((float)click_x_overImage) * _widthZoom
                ));

                click_y = ((int)
                (
                    ((float)click_y_overImage) * _heightZoom
                ));

                if (_click_x_last != null && _imageBitmap != null)
                {
                    int penWidth = ((int)(2.0 * _widthZoom));
                    Pen pen = new Pen(Color.Red, penWidth);
                    Graphics graphics = Graphics.FromImage(_imageBitmap);
                    graphics.DrawLine(pen, click_x, click_y, (int)_click_x_last, (int)_click_y_last);
                    ((PictureBox)sender).Image = _imageBitmap;
                    ControlFactory factory = ControlFactory.Instance;
                    GuiMediator.Instance.IsDirty = true;
                }

                _click_x_last = click_x;
                _click_y_last = click_y;
            }
        }

        public void image_Click(object sender, EventArgs e)
        {
        }
    }
}
