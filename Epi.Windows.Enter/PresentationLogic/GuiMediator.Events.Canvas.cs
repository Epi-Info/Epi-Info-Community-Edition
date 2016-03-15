using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Epi.Windows.Controls;
using Epi.Fields;


namespace Epi.Windows.Enter.PresentationLogic
{
	public partial class GuiMediator
	{

        private List<int> DebugList = new List<int>();

        /// <summary>
        /// Field data is saved
        /// </summary>        
        public void SetFieldData()
        {
            ControlFactory factory = ControlFactory.Instance;

            if(canvas != null && canvas.Panels != null)
            foreach (Panel panel in canvas.Panels)
            {
                
                foreach (Control control in panel.Controls)
                {
                    try
                    {
                        // Skip prompts and field group boxes. They don't have any data.
                        if (control is Label) continue;
                        if (control is FieldGroupBox) continue;

                        Field field = factory.GetAssociatedField(control);
                        if (field == null) continue;

                        field = this.view.Fields[field.Name];
                        // Images fields populate the file name when the file is selected. No need to read from control any more.
                        if (field is ImageField) continue;

                        if (field is YesNoField)
                        {
                            YesNoField yesNoField = field as YesNoField;
                            ComboBox comboBox = control as ComboBox;
                            if (comboBox.SelectedIndex >= 0)
                            {

                                if (comboBox.SelectedValue != null)
                                {
                                    yesNoField.CurrentRecordValueObject = comboBox.SelectedValue;
                                }
                                else if (comboBox.SelectedIndex == 0)
                                {
                                    yesNoField.CurrentRecordValueObject = 1; // 1 = yes index 0 = yes
                                }
                                else
                                {
                                    yesNoField.CurrentRecordValueObject = 0; // 0 = no index 1 = no
                                }
                            }
                            else
                            {
                                yesNoField.CurrentRecordValueObject = null;
                            }
                        }
                        else if (field is GridField)
                        {
                            DataGridView dgv = (DataGridView)control;
                            if (dgv.DataSource is DataTable)
                            {
                                ((GridField)field).DataSource = (DataTable)dgv.DataSource;
                            }
                        }
                        else if (field is OptionField)
                        {
                            OptionField optionField = field as OptionField;
                            GroupBox groupBox = control as GroupBox;

                            foreach (Control rb in groupBox.Controls)
                            {
                                if (((RadioButton)rb).Checked)
                                {
                                    int index = 0;
                                    foreach (string option in optionField.Options)
                                    {
                                        if (option == rb.Text)
                                        {
                                            optionField.CurrentRecordValueObject = index;
                                        }
                                        index++;
                                    }
                                }
                            }
                        }
                        else if (control is TextBox || control is RichTextBox || control is ComboBox || control is CheckBox || control is MaskedTextBox || control is DateTimePicker)
                        {
                            if (field is IDataField)
                            {
                                if (field is DateTimeField)
                                {
                                    SetDateTimeData(field as DateTimeField, control);
                                }
                                else if (control is TextBox || control is RichTextBox)
                                {
                                    SetTextData(field, control);
                                }
                                else if (control is MaskedTextBox)
                                {
                                    if (field is NumberField || field is PhoneNumberField)
                                    {
                                        SetNumberData(field, control);
                                    }
                                    else
                                    {
                                        SetOtherMaskedData(field, control);
                                    }
                                }
                                else if (control is ComboBox)
                                {
                                    SetComboBoxData(field, control);
                                }
                                else if (control is CheckBox)
                                {
                                    SetCheckBoxData(field, control);
                                }
                                else if (control is GroupBox)
                                {
                                }
                                else if (control is DateTimePicker)
                                {

                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        // do nothing for now
                    }

                } // end foreach (Control control in panel.Controls)
            } // end foreach (Panel panel in canvas.Panels)
        }

        /// <summary>
        /// Field data is saved
        /// </summary>        
        public void SetFieldData(Control control)
        {
            ControlFactory factory = ControlFactory.Instance;
            // Skip prompts and field group boxes. They don't have any data.
            //if (control is Label) return;
            //if (control is FieldGroupBox) return;

            Field field = factory.GetAssociatedField(control);
            if (field == null)
                return;
            if (field.Name == null)
            {
                return;
            }
            field = this.view.Fields[field.Name];
            // Images fields populate the file name when the file is selected. No need to read from control any more.
            if (field is ImageField) return;

            if (field is YesNoField)
            {
                YesNoField yesNoField = field as YesNoField;
                ComboBox comboBox = control as ComboBox;
                if (comboBox.SelectedIndex >= 0)
                {
                    if (comboBox.SelectedValue != null)
                    {
                        yesNoField.CurrentRecordValueObject = comboBox.SelectedValue;
                    }
                    else if (comboBox.SelectedIndex == 0)
                    {
                        yesNoField.CurrentRecordValueObject = 1; // 1 = yes index 0 = yes
                    }
                    else
                    {
                        yesNoField.CurrentRecordValueObject = 0; // 0 = no index 1 = no
                    }
                    
                }
                else
                {
                    yesNoField.CurrentRecordValueObject = null;
                }
            }
            else if (field is GridField)
            {
                DataGridView dgv = (DataGridView)control;
                if (dgv.DataSource is DataTable)
                {
                    ((GridField)field).DataSource = (DataTable)dgv.DataSource;
                }
            }
            else if (field is OptionField)
            {
                OptionField optionField = field as OptionField;
                GroupBox groupBox = control as GroupBox;

                foreach (Control rb in groupBox.Controls)
                {
                    if (((RadioButton)rb).Checked)
                    {
                        int index = 0;
                        foreach (string option in optionField.Options)
                        {
                            if (option == rb.Text)
                            {
                                optionField.CurrentRecordValueObject = index;
                            }
                            index++;
                        }
                    }
                }
            }
            else if (control is TextBox || control is ComboBox || control is CheckBox || control is MaskedTextBox || control is DateTimePicker)
            {
                if (field is IDataField)
                {
                    if (field is DateTimeField)
                    {
                        SetDateTimeData(field as DateTimeField, control);
                    }
                    else if (control is TextBox)
                    {
                        SetTextData(field, control);
                    }
                    else if (control is MaskedTextBox)
                    {
                        if (field is NumberField || field is PhoneNumberField)
                        {
                            SetNumberData(field, control);
                        }
                        else
                        {
                            SetOtherMaskedData(field, control);
                        }
                    }
                    else if (control is ComboBox)
                    {
                        SetComboBoxData(field, control);
                    }
                    else if (control is CheckBox)
                    {
                        SetCheckBoxData(field, control);
                    }
                    else if (control is GroupBox)
                    {
                    }
                } 
            }
        }



        private void ClearPanelControls(Panel panel)
        {
            try
            {
                while (panel.Controls.Count > 0) panel.Controls[0].Dispose(); 
            }
            catch (Exception ex)
            {
                //
            }
            finally
            {
                GC.Collect();
            }
        }

        public void SetZeeOrderOfGroups(Panel panel)
        {
            SortedList<FieldGroupBox, System.Drawing.Point>  pointSortedGroups;
            SetZeeOrderOfGroups(panel, out pointSortedGroups);
        }

        public void SetZeeOrderOfGroups(Panel panel, out System.Collections.Generic.SortedList<FieldGroupBox, System.Drawing.Point> pointSortedGroups)
        {
            pointSortedGroups = new SortedList<FieldGroupBox, System.Drawing.Point>(new GroupZeeOrderComparer());

            foreach (Control possibleGroupControl in panel.Controls)
            {
                FieldGroupBox groupControl = possibleGroupControl as FieldGroupBox;
                if (groupControl != null)
                {
                    pointSortedGroups.Add(groupControl, ((Control)groupControl).Location);
                }
            }

            foreach (KeyValuePair<FieldGroupBox, System.Drawing.Point> kvp in pointSortedGroups)
            {
                ((Control)kvp.Key).SendToBack();
            }
        }

        /// <summary>
        /// Determines if the control is the first control on the page
        /// </summary>
        /// <param name="control">Control on panel</param>
        /// <returns>Boolean to indicate if control is the first on the page</returns>
        private bool IsFirstControlOnPage(Control control)
        {
            ControlFactory factory = ControlFactory.Instance;
            Field field = factory.GetAssociatedField(control);

            double minTabIndex = currentPage.GetMetadata().GetMinTabIndex(currentPage.Id, this.view.Id);
            // Changed "if not then false else true" to a positive test.
            return (((RenderableField)field).TabIndex == minTabIndex);
        }

        /// <summary>
        /// Determines if the control is the last control on the page
        /// </summary>
        /// <param name="control">Control on panel</param>
        /// <returns>Boolean to indicate if control is the last on the page</returns>
        public bool IsLastControlOnPage(Control control)
        {
            #region Input Validation
            if (control == null)
            {
                throw new ArgumentNullException("Control");
            }
            #endregion  //Input Validation

            int maxTabIndexFound = 0;         
            Control controltab=null;

            foreach (Control candidate in _fieldPanel.Controls)
            {
                if (candidate.TabStop && candidate.Enabled && candidate.Visible)
                {
                    if (candidate.TabIndex >= maxTabIndexFound)
                    {
                        maxTabIndexFound = candidate.TabIndex;
                        controltab = candidate;                                             
                    }
                }
            }

            if (maxTabIndexFound == control.TabIndex & control == controltab)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        /// <summary>
        /// Determines if the control is the last control in the view
        /// </summary>
        /// <param name="control">Control on panel</param>
        /// <returns>Boolean to indicate if control is the last control on the view </returns>
        private bool IsLastControlOnView(Control control)
        {
            #region Input Validation
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }

            #endregion  //Input Validation

            if (this.currentPage.Position == currentPage.GetMetadata().GetMaxViewPagesPosition(this.view.Id))
            {
                return IsLastControlOnPage(control);
            }
            else
            {
                return false;
            }
        }



        private void MarkRelatedRecoredsAsDeleted(Epi.Data.Services.CollectedDataProvider OutputDriver, View pView, List<int> pIDList)
        {

            if(pIDList.Count < 1)
            {
                return;
            }

            Dictionary<string, bool> VisitedViews = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            string SQL = null;
            StringBuilder InSQL = new StringBuilder();
            InSQL.Append("  (");
            foreach (int ID in pIDList)
            {
                pView.LoadRecord(ID);
                pView.RecStatusField.CurrentRecordValue = 0;
                pView.SaveRecord(ID);
                InSQL.Append("'");
                InSQL.Append(pView.CurrentGlobalRecordId);
                InSQL.Append("',");
            }
            InSQL.Length = InSQL.Length - 1;
            InSQL.Append(")");

            foreach (Field field in pView.Fields)
            {
                if (field is RelatedViewField)
                {
                    RelatedViewField rvf = field as RelatedViewField;
                    if (!VisitedViews.ContainsKey(rvf.ChildView.Name))
                    {
                        SQL = "Select UniqueKey From [" + rvf.ChildView.TableName + "] Where FKey In " + InSQL.ToString();
                        IDataReader reader = OutputDriver.ExecuteReader(OutputDriver.CreateQuery(SQL)); 
                        List<int> NewIdList = new List<int>();
                        while (reader.Read())
                        {
                            if( reader["UniqueKey"] != DBNull.Value)
                            {
                                NewIdList.Add((int)reader["UniqueKey"]);
                            }
                        }
                        VisitedViews.Add(rvf.ChildView.Name, true);
                        MarkRelatedRecoredsAsDeleted(OutputDriver, rvf.ChildView, NewIdList);
                    }
                }
                else if (field is Epi.Fields.GridField)
                {

                    Epi.Fields.GridField gf = field as Epi.Fields.GridField;

                    SQL = "Update  [" + gf.TableName + "] Set RecStatus = 0 Where FKey In " + InSQL.ToString();
                    OutputDriver.ExecuteNonQuery(OutputDriver.CreateQuery(SQL));

                }
            }
        }

        private void MarkRelatedRecoredsAsDeleted(int pUniqueKey)
        {
            List<int> IDList = new List<int>();
            IDList.Add(pUniqueKey);
            Epi.Data.Services.CollectedDataProvider OutputDriver = this.EnterCheckCodeEngine.Project.CollectedData;// Epi.Data.DBReadExecute.GetDataDriver(this.project.CollectedDataConnectionString, true);
            MarkRelatedRecoredsAsDeleted(OutputDriver, this.view, IDList);
        }


        private void UnMarkRelatedRecoredsAsDeleted(Epi.Data.Services.CollectedDataProvider OutputDriver, View pView, List<int> pIDList)
        {
            if (pIDList.Count < 1)
            {
                return;
            }

            Dictionary<string, bool> VisitedViews = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            string SQL = null;
            StringBuilder InSQL = new StringBuilder();
            InSQL.Append(" Where FKey In (");
            foreach (int ID in pIDList)
            {
                pView.LoadRecord(ID);
                pView.RecStatusField.CurrentRecordValue = 1;
                pView.SaveRecord(ID);
                InSQL.Append("'");
                InSQL.Append(pView.CurrentGlobalRecordId);
                InSQL.Append("',");
            }
            InSQL.Length = InSQL.Length - 1;
            InSQL.Append(")");

            foreach (Field field in pView.Fields)
            {
                if (field is RelatedViewField)
                {
                    RelatedViewField rvf = field as RelatedViewField;
                    if (!VisitedViews.ContainsKey(rvf.ChildView.Name))
                    {
                        SQL = "Select UniqueKey From [" + rvf.ChildView.TableName + "] " + InSQL.ToString();
                        IDataReader reader = OutputDriver.ExecuteReader(OutputDriver.CreateQuery(SQL));
                        List<int> NewIdList = new List<int>();
                        while (reader.Read())
                        {
                            if (reader["UniqueKey"] != DBNull.Value)
                            {
                                NewIdList.Add((int)reader["UniqueKey"]);
                            }
                        }
                        VisitedViews.Add(rvf.ChildView.Name, true);
                        UnMarkRelatedRecoredsAsDeleted(OutputDriver, rvf.ChildView, NewIdList);
                    }
                    else if (field is Epi.Fields.GridField)
                    {

                        Epi.Fields.GridField gf = field as Epi.Fields.GridField;

                        SQL = "Update  [" + gf.TableName + "] Set RecStatus = 1 Where FKey In " + InSQL.ToString();
                        OutputDriver.ExecuteNonQuery(OutputDriver.CreateQuery(SQL));

                    }
                }
            }
        }

        private void UnMarkRelatedRecoredsAsDeleted(int pUniqueKey)
        {
            List<int> IDList = new List<int>();
            IDList.Add(pUniqueKey);
            Epi.Data.Services.CollectedDataProvider OutputDriver = this.EnterCheckCodeEngine.Project.CollectedData;// Epi.Data.DBReadExecute.GetDataDriver(this.project.CollectedDataConnectionString, true);
            UnMarkRelatedRecoredsAsDeleted(OutputDriver, this.view, IDList);
        }

    }

    #region GroupZeeOrderComparer
    class GroupZeeOrderComparer : IComparer<FieldGroupBox>
    {
        public int Compare(FieldGroupBox x, FieldGroupBox y)
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
