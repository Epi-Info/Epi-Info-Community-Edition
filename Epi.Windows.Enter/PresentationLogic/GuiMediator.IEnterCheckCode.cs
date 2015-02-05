using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Epi.Fields;
using EpiInfo.Plugin;
using Epi.Windows.Enter.PresentationLogic;
using Epi.Windows.Controls;

namespace Epi.Windows.Enter.PresentationLogic
{
    public partial class GuiMediator : IEnterInterpreterHost
    {
        #region IEnterCheckCode Members
        private bool enableCheckCodeExecution = true;
        private bool enableSuppressErorrs = true;
        private string latName;
        private string longName;


        public bool Register(IEnterInterpreter enterInterpreter)
        {
            enterInterpreter.Host = this;
            return true;
        }

        public bool Geocode(string address, string latName, string longName)
        {
            ESRI.ArcGIS.Client.Bing.Geocoder geocoder = new ESRI.ArcGIS.Client.Bing.Geocoder(Configuration.GetNewInstance().Settings.MapServiceKey);
            this.latName = latName;
            this.longName = longName;
            geocoder.Geocode(address, Geocode_Completed);
            return true;
        }

        private void Geocode_Completed(object sender, ESRI.ArcGIS.Client.Bing.GeocodeService.GeocodeCompletedEventArgs e)
        {
            try
            {
                if (e.Result.Results.Count > 0)
                {
                    Epi.Enter.Dialogs.GeocodeSelectionDialog dialog = new Epi.Enter.Dialogs.GeocodeSelectionDialog();
                    dialog.Results = e.Result.Results;
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Assign(latName, dialog.Latitude);
                        Assign(longName, dialog.Longitude);
                    }
                }
                else
                {
                    Dialog("No matching coordinates found for the specified address.", "Geocoder");
                }
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException is System.ServiceModel.EndpointNotFoundException)
                    {
                        Dialog("There was a problem running the geocode command. Please ensure the computer is connected to the Internet and try agian.", "Geocoder");
                    }
                }                
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException is System.ServiceModel.FaultException<ESRI.ArcGIS.Client.Bing.GeocodeService.ResponseSummary>)
                    {
                        string message = ((System.ServiceModel.FaultException<ESRI.ArcGIS.Client.Bing.GeocodeService.ResponseSummary>)ex.InnerException).Message;
                        if (message.ToLower().Contains("credential"))
                        {
                            Dialog("The Map Service Key is invalid. Please update it from the Tools > Options dialog", "Geocoder");
                        }
                    }
                }
            }
        }


        public bool AssignGrid(string pName, object pValue, int pIndex0, object pIndex1)
        {
            if (string.IsNullOrEmpty(pName)) return false;

            bool result = false;

            try
            {
                if (this.view == null)
                {
                    this.view = this.EnterCheckCodeEngine.CurrentView.View;
                }
                Epi.Fields.GridField gf = this.view.Fields[pName] as Epi.Fields.GridField;
                MetaFieldType mft = MetaFieldType.Text;
                if (pIndex1 is string)
                {
                    foreach (GridColumnBase gcb in gf.Columns)
                    {
                        if (gcb.Name.Equals(pIndex1.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            mft = gcb.GridColumnType;
                            break;
                        }
                    }
                }
                else
                {

                    GridColumnBase gcb = gf.Columns[(int)pIndex1];
                    mft = gcb.GridColumnType;
                }

                if (pValue is TimeSpan)
                {
                    pValue = new DateTime(1900, 1, 1, ((TimeSpan)pValue).Hours, ((TimeSpan)pValue).Minutes, ((TimeSpan)pValue).Seconds);
                }

                if (mft == MetaFieldType.CommentLegal)
                {
                    pValue = pValue.ToString();
                }

                if (gf.DataSource.Rows.Count > 0)
                {
                    if (pIndex1 is string)
                    {
                        gf.DataSource.Rows[pIndex0][pIndex1.ToString()] = pValue;
                    }
                    else
                    {
                        gf.DataSource.Rows[pIndex0][(int)pIndex1] = pValue;
                    }
                }
                /*
                ((IDataField)field).CurrentRecordValueObject = pValue;

                result = true;

                if (this.currentPage != null && this.currentPage.Fields.Contains(pName))
                {
                    try
                    {
                        ControlFactory factory = ControlFactory.Instance;
                        List<Control> controls = factory.GetAssociatedControls(this.view.Fields[pName]);
                        foreach (Control control in controls)
                        {
                            if (control is Label) continue;
                            if (control is FieldGroupBox) continue;

                            field = factory.GetAssociatedField(control);
                            field = this.currentPage.Fields[field.Name];
                            if (field is ImageField)
                            {
                                GetFieldDataIntoControl(field as ImageField, control as PictureBox);
                            }
                            else if (field is YesNoField)
                            {
                                GetFieldDataIntoControl(field as YesNoField, control as ComboBox);
                            }

                            else if (control is TextBox || control is RichTextBox || control is ComboBox || control is CheckBox || control is MaskedTextBox || control is DataGridView || control is GroupBox || control is DateTimePicker)
                            {
                                if (field is MirrorField)
                                {
                                    GetMirrorData(field as MirrorField, control);
                                }
                                else if (field is IDataField || control is DataGridView)
                                {
                                    if (control is TextBox || control is RichTextBox)
                                    {
                                        GetTextData(field, control);
                                    }
                                    else if (field is DateField)
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
                                    else if (control is MaskedTextBox)
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

                        result = true;
                    }
                    catch (Exception e)
                    {
                        //result = false;
                    }
                }// end if*/
            }
            catch (Exception e2)
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Assign
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="setValue">value</param>
        /// <returns>boolean</returns>
        public bool Assign(string name, object setValue)
        {
            if (string.IsNullOrEmpty(name)) return false;

            bool result = false;

            try
            {
                if (this.view == null)
                {
                    this.view = this.EnterCheckCodeEngine.CurrentView.View;
                }

                Field field = this.view.Fields[name];

                if (setValue is TimeSpan)
                {
                    setValue = new DateTime(1900, 1, 1, ((TimeSpan)setValue).Hours, ((TimeSpan)setValue).Minutes, ((TimeSpan)setValue).Seconds);
                }

                if (field is DDLFieldOfCommentLegal)
                {
                     setValue = setValue.ToString();
                }

                if (field is IDataField)
                {
                    ((IDataField)field).CurrentRecordValueObject = setValue;
                    this.IsDirty = true;
                    result = true;
                }
                else if (field is LabelField)
                {
                    result = true;
                }

                if (this.currentPage != null && this.currentPage.Fields.Contains(name))
                {
                    try
                    {
                        ControlFactory factory = ControlFactory.Instance;
                        List<Control> controls = factory.GetAssociatedControls(this.view.Fields[name]);
                        foreach (Control control in controls)
                        {
                            if (control is Label)
                            {
                                if (field is LabelField)
                                {
                                    if (setValue != null)
                                    {
                                        control.Text = setValue.ToString();
                                    }
                                    else
                                    {
                                        control.Text = "";
                                    }
                                }
                                continue;
                            }
                            if (control is FieldGroupBox) continue;

                            field = factory.GetAssociatedField(control);
                            field = this.currentPage.Fields[field.Name];
                            if (field is ImageField)
                            {
                                GetFieldDataIntoControl(field as ImageField, control as PictureBox);
                            }
                            else if (field is YesNoField)
                            {
                                GetFieldDataIntoControl(field as YesNoField, control as ComboBox);
                            }

                            else if (control is TextBox || control is RichTextBox || control is ComboBox || control is CheckBox || control is MaskedTextBox || control is DataGridView || control is GroupBox || control is DateTimePicker)
                            {
                                if (field is MirrorField)
                                {
                                    GetMirrorData(field as MirrorField, control);
                                }
                                else if (field is IDataField || control is DataGridView)
                                {
                                    if (control is TextBox || control is RichTextBox)
                                    {
                                        GetTextData(field, control);
                                    }
                                    else if (field is DateField)
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
                                    else if (control is MaskedTextBox)
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

                        result = true;
                    }
                    catch (Exception e)
                    {
                        //result = false;
                    }
                }// end if
            }
            catch (Exception e2)
            {
                result = false;
            }

            return result;
        }

        public int RecordCount()
        {
            return this.EnterCheckCodeEngine.CurrentView.View.GetRecordCount();
        }

        /// <summary>
        /// Try Get Field Info
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="dataType">Type</param>
        /// <param name="value">Value</param>
        /// <returns>boolean</returns>
        public bool TryGetFieldInfo(string name, out EpiInfo.Plugin.DataType dataType, out string value)
        {
            bool fieldExists = false;
            IVariable dataField;
            dataType = EpiInfo.Plugin.DataType.Unknown;
            value = string.Empty;

            if (this.view == null)
            {
                this.view = this.EnterCheckCodeEngine.CurrentView.View;
            }

            // check if field is on current page and get data from the page control
            // if not on current page then pull from view.Fields
            if (this.currentPage != null && this.currentPage.Fields.Contains(name))
            {
                List<System.Windows.Forms.Control> Controls = ControlFactory.Instance.GetAssociatedControls(this.currentPage.Fields[name]);
                dataField = (IVariable)this.view.Fields[name];

                dataType = (EpiInfo.Plugin.DataType)dataField.DataType;

                if (Controls.Count > 1)
                {
                    if (Controls[1] is CheckBox)
                    {
                        value = ((CheckBox)Controls[1]).Checked.ToString();
                    }
                    else if (Controls[1] is Epi.Windows.Enter.Controls.LegalValuesComboBox)
                    {
                        Epi.Windows.Enter.Controls.LegalValuesComboBox legalValue = (Epi.Windows.Enter.Controls.LegalValuesComboBox)Controls[1];
                        // codes do NOT use the SelectedValue
                        //if (legalValue.SelectedValue == null)
                        //{
                        //    value = Controls[1].Text;
                        //}
                        //else
                        //{
                        //    if (legalValue.SelectedIndex > -1)
                        //    {
                        //        if (legalValue.Items[legalValue.SelectedIndex] is System.Data.DataRowView)
                        //        {
                        //            value = ((System.Data.DataRowView)legalValue.SelectedValue).Row[0].ToString();
                        //        }
                        //        else
                        //        {
                        //                value = legalValue.SelectedValue.ToString();
                        //        }
                        //    }
                        //    else
                        //    {
                        //        value = legalValue.SelectedValue.ToString();
                        //    }

                        //}

                        if (dataField is DDLFieldOfCommentLegal)
                        {
                            /*if (!string.IsNullOrEmpty(legalValue.SelectedText) && legalValue.SelectedText.Trim().Length > 0)
                            {
                                value = legalValue.SelectedText.Split('-')[0].Trim();
                            }
                            else
                            {*/
                                if (!string.IsNullOrEmpty(Controls[1].Text) && Controls[1].Text.Trim().Length > 0)
                                {
                                    value = Controls[1].Text.Split('-')[0].Trim();
                                }
                            //}
                        }
                        else if (dataField is YesNoField)
                        {
                            if (legalValue.SelectedIndex == 1)
                                value = "0";
                            else if (legalValue.SelectedIndex == 0)
                                value = "1";
                            else
                                value = "";
                        }
                        else
                        {
                            value = Controls[1].Text;
                        }
                    }
                    else
                    {
                        value = Controls[1].Text;
                    }
                }
                else
                {
                    if (Controls[0] is CheckBox)
                    {
                        value = ((CheckBox)Controls[0]).Checked.ToString(); 
                    }
                    else if (Controls[0] is GroupBox)
                    {
                        string checkedRadioButtonName = string.Empty;
                        
                        foreach (Control control in ((GroupBox)Controls[0]).Controls)
                        {
                            if (((RadioButton)control).Checked)
                            {
                                checkedRadioButtonName = control.Text;
                                break;
                            }
                        }

                        int index = 0;
                        foreach (string option in ((OptionField)dataField).Options)
                        {
                            if (checkedRadioButtonName == option)
                            {
                                value = index.ToString();
                                break;
                            }
                            index++;
                        }
                    }
                    else if (Controls[0] is Epi.Windows.Enter.Controls.LegalValuesComboBox)
                    {

                        // codes do NOT use the SelectedValue
                        if (((Epi.Windows.Enter.Controls.LegalValuesComboBox)Controls[0]).SelectedValue == null)
                        {
                            value = Controls[0].Text;
                        }
                        else
                        {
                            if (((Epi.Windows.Enter.Controls.LegalValuesComboBox)Controls[0]).SelectedValue is System.Data.DataRowView)
                            {
                                value = ((System.Data.DataRowView)((Epi.Windows.Enter.Controls.LegalValuesComboBox)Controls[0]).SelectedValue).Row[0].ToString();
                            }
                            else
                            {
                                value = ((Epi.Windows.Enter.Controls.LegalValuesComboBox)Controls[0]).SelectedValue.ToString();
                            }
                        }

                        if (dataField is DDLFieldOfCommentLegal)
                        {
                            value = value.Split('-')[0].Trim();
                        }
                    }
                    else
                    {
                        value = Controls[0].Text;
                    }
                }
            }
            else if (this.view.Fields.Contains(name))
            {
                try
                {
                    dataField = (IVariable)this.view.Fields[name];
                    dataType = (EpiInfo.Plugin.DataType) dataField.DataType;
                    if (dataField is InputFieldWithSeparatePrompt)
                    {
                        if (dataField is DDLFieldOfCommentLegal)
                        {
                            value = ((Epi.Fields.InputFieldWithSeparatePrompt)(dataField)).CurrentRecordValueObject.ToString();
                        }
                        else
                        {
                            value = ((Epi.Fields.InputFieldWithSeparatePrompt)(dataField)).CurrentRecordValueString;
                        } 
                    }
                    else if (dataField is InputFieldWithoutSeparatePrompt)
                    {
                            value = ((Epi.Fields.InputFieldWithoutSeparatePrompt)(dataField)).CurrentRecordValueString;
                    }
                    else if (dataField is PredefinedDataField)
                    {
                        value = ((Epi.Fields.PredefinedDataField)(dataField)).CurrentRecordValueString;
                    }
                    fieldExists = true;
                }
                catch
                {
                    fieldExists = false;
                }
            }
            else if (this.view.IsRelatedView)
            {
                bool isparentexists = false;
                View parentview = null;
                try
                {
                    isparentexists = GetParentfieldvalue(name, view, out parentview);
                    if (isparentexists)
                    {
                        dataField = (IVariable)this.EnterCheckCodeEngine.CurrentView.View.Project.Views[parentview.Name].Fields[name];
                        dataType = (EpiInfo.Plugin.DataType)dataField.DataType;
                        if (dataField is InputFieldWithSeparatePrompt)
                        {
                            if (dataField is DDLFieldOfCommentLegal)
                            {
                                value = ((Epi.Fields.InputFieldWithSeparatePrompt)(dataField)).CurrentRecordValueObject.ToString();
                            }
                            else
                            {
                                value = ((Epi.Fields.InputFieldWithSeparatePrompt)(dataField)).CurrentRecordValueString;
                            }
                        }
                        else if (dataField is InputFieldWithoutSeparatePrompt)
                        {
                            value = ((Epi.Fields.InputFieldWithoutSeparatePrompt)(dataField)).CurrentRecordValueString;
                        }
                        else if (dataField is PredefinedDataField)
                        {
                            value = ((Epi.Fields.PredefinedDataField)(dataField)).CurrentRecordValueString;
                        }
                        fieldExists = true;
                    }
                }
                catch
                {
                    fieldExists = false;
                }
            }
            return fieldExists;
        }


        public bool GetParentfieldvalue(string name, View curview, out View parentview)
        {
            bool isparentexists = false;
            isparentexists = GetParentView(curview, out parentview);
            if (isparentexists)
            {
                if (parentview.Fields.Contains(name))
                {
                    return isparentexists;
                }
                else
                {
                    curview = parentview;
                    isparentexists = GetParentfieldvalue(name, curview, out parentview);
                }
            }
            return isparentexists;
        }

        public bool GetParentView(View v, out View parentview)
        {
            parentview = null;
            if (v.ParentView != null)
            {
                parentview = v.ParentView;
                return true;
            }
            else return false;
        }
        

        public EpiInfo.Plugin.IVariable GetGridValue(string pName, int pIndex0, object pIndex1)
        {
            Dictionary<MetaFieldType, EpiInfo.Plugin.DataType> MapGridColumnTypeToDataType = new Dictionary<MetaFieldType, EpiInfo.Plugin.DataType>();

            MapGridColumnTypeToDataType.Add(MetaFieldType.Text,EpiInfo.Plugin.DataType.Text);
            MapGridColumnTypeToDataType.Add(MetaFieldType.LabelTitle,EpiInfo.Plugin.DataType.Unknown);
            MapGridColumnTypeToDataType.Add(MetaFieldType.TextUppercase,EpiInfo.Plugin.DataType.Text);
            MapGridColumnTypeToDataType.Add(MetaFieldType.Multiline,EpiInfo.Plugin.DataType.Text);
            MapGridColumnTypeToDataType.Add(MetaFieldType.Number,EpiInfo.Plugin.DataType.Number);
            MapGridColumnTypeToDataType.Add(MetaFieldType.PhoneNumber,EpiInfo.Plugin.DataType.PhoneNumber);
            MapGridColumnTypeToDataType.Add(MetaFieldType.Date,EpiInfo.Plugin.DataType.Date);
            MapGridColumnTypeToDataType.Add(MetaFieldType.Time,EpiInfo.Plugin.DataType.Time);
            MapGridColumnTypeToDataType.Add(MetaFieldType.DateTime,EpiInfo.Plugin.DataType.DateTime);
            MapGridColumnTypeToDataType.Add(MetaFieldType.Checkbox,EpiInfo.Plugin.DataType.Boolean);
            MapGridColumnTypeToDataType.Add(MetaFieldType.YesNo,EpiInfo.Plugin.DataType.YesNo);
            MapGridColumnTypeToDataType.Add(MetaFieldType.Option,EpiInfo.Plugin.DataType.Text);
            MapGridColumnTypeToDataType.Add(MetaFieldType.CommandButton,EpiInfo.Plugin.DataType.Unknown);
            MapGridColumnTypeToDataType.Add(MetaFieldType.Image,EpiInfo.Plugin.DataType.Unknown);
            MapGridColumnTypeToDataType.Add(MetaFieldType.Mirror,EpiInfo.Plugin.DataType.Unknown);
            MapGridColumnTypeToDataType.Add(MetaFieldType.Grid,EpiInfo.Plugin.DataType.Unknown);
            MapGridColumnTypeToDataType.Add(MetaFieldType.LegalValues,EpiInfo.Plugin.DataType.Text);
            MapGridColumnTypeToDataType.Add(MetaFieldType.Codes,EpiInfo.Plugin.DataType.Text);
            MapGridColumnTypeToDataType.Add(MetaFieldType.CommentLegal,EpiInfo.Plugin.DataType.Text);
            MapGridColumnTypeToDataType.Add(MetaFieldType.Relate,EpiInfo.Plugin.DataType.Unknown);
            MapGridColumnTypeToDataType.Add(MetaFieldType.Group,EpiInfo.Plugin.DataType.Unknown);
            MapGridColumnTypeToDataType.Add(MetaFieldType.RecStatus,EpiInfo.Plugin.DataType.Number);
            MapGridColumnTypeToDataType.Add(MetaFieldType.UniqueKey,EpiInfo.Plugin.DataType.Number);
            MapGridColumnTypeToDataType.Add(MetaFieldType.ForeignKey,EpiInfo.Plugin.DataType.GUID);
            MapGridColumnTypeToDataType.Add(MetaFieldType.GUID,EpiInfo.Plugin.DataType.GUID);
            MapGridColumnTypeToDataType.Add(MetaFieldType.GlobalRecordId,EpiInfo.Plugin.DataType.GUID);
            MapGridColumnTypeToDataType.Add(MetaFieldType.List,EpiInfo.Plugin.DataType.Text);
            MapGridColumnTypeToDataType.Add(MetaFieldType.UniqueRowId, EpiInfo.Plugin.DataType.GUID);


            Epi.PluginVariable result = new  PluginVariable();

            result.Name = pName;
            result.DataType = EpiInfo.Plugin.DataType.Unknown;

            Epi.Fields.GridField gf = this.view.Fields[pName] as Epi.Fields.GridField;
            if (pIndex1 is string)
            {
                foreach (GridColumnBase gcb in gf.Columns)
                {
                    if (gcb.Name.Equals(pIndex1.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        result.DataType = MapGridColumnTypeToDataType[gcb.GridColumnType];
                        break;
                    }
                }
            }
            else
            {

                GridColumnBase gcb = gf.Columns[(int)pIndex1];
                result.DataType = MapGridColumnTypeToDataType[gcb.GridColumnType];

            }

            // if not on current page then pull from view.Fields
            if (this.currentPage.Fields.Contains(pName))
            {
                List<System.Windows.Forms.Control> Controls = ControlFactory.Instance.GetAssociatedControls(this.currentPage.Fields[pName]);

                DataGridView dgv = null;

                if (Controls.Count > 1)
                {
                    dgv = Controls[1] as DataGridView;
                }
                else
                {
                    dgv = Controls[0] as DataGridView;

                }

                if (pIndex1 is string)
                {
                    result.Expression = dgv.Rows[pIndex0].Cells[pIndex1.ToString()].Value.ToString();
                }
                else
                {
                    result.Expression = dgv.Rows[pIndex0].Cells[(int)pIndex1].Value.ToString();
                }
            }
            else if (this.view.Fields.Contains(pName))
            {

                if (gf.DataSource.Rows.Count > 0)
                {

                    if (pIndex1 is string)
                    {
                        result.Expression = gf.DataSource.Rows[pIndex0][pIndex1.ToString()].ToString();
                    }
                    else
                    {
                        result.Expression = gf.DataSource.Rows[pIndex0][(int)pIndex1].ToString();
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Get Value
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>string</returns>
        public string GetValue(string name)
        {
            string result = null;


            // if not on current page then pull from view.Fields
            if (this.currentPage.Fields.Contains(name))
            {
                List<System.Windows.Forms.Control> Controls = ControlFactory.Instance.GetAssociatedControls(this.currentPage.Fields[name]);

                if (Controls.Count > 1)
                {
                    result = Controls[1].Text;
                }
                else
                {
                    result = Controls[0].Text;
                }
            }
            else
            if (this.view.Fields.Contains(name))
            {
                result = ((IDataField)this.view.Fields[name]).CurrentRecordValueString;
            }
            return result;
        }

        /// <summary>
        /// Dialog
        /// </summary>
        /// <param name="pTextPrompt">Text Prompt</param>
        /// <param name="pTitleText">Title Text</param>
        public void Dialog(string pTextPrompt, string pTitleText)
        {
            MsgBox.Show(pTextPrompt, pTitleText);
        }

        /// <summary>
        /// Dialog
        /// </summary>
        /// <param name="pTextPrompt">Text Prompt</param>
        /// <param name="pVariable">Variable</param>
        /// <param name="pListType">List Type</param>
        /// <param name="pTitleText">Title Text</param>
        public void Dialog(string pTextPrompt, ref object pVariable, string pListType, string pTitleText)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Displays a textbox input dialog
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="caption">Caption</param>
        /// <param name="mask">Mask</param>
        /// <param name="modifier">Modifier</param>
        /// <param name="input">Input</param>
        public bool Dialog(string text, string caption, string mask, string modifier, ref object input, EpiInfo.Plugin.DataType dataType)
        {
            bool ret = false;

            if (modifier.ToUpper().Equals("READ") || modifier.ToUpper().Equals("WRITE"))
            {
                string filePath;
                if (TryGetFileDialog((string)input, caption, modifier.ToUpper().Equals("READ"), out filePath))
                {
                    ret = true;
                    if (input is StringBuilder)
                    {
                        ((StringBuilder)input).Append(filePath);
                    }
                    else
                    {
                        input = filePath;
                    }
                }
            }
            else
            {
                Epi.Windows.Dialogs.InputDialog dialog = new Epi.Windows.Dialogs.InputDialog(text, caption, mask, input, dataType);
                DialogResult result = dialog.ShowDialog();
                if (input is StringBuilder)
                {
                    ((StringBuilder)input).Append(dialog.Input);
                }
                else if (dataType == EpiInfo.Plugin.DataType.Date)
                {
                    DateTime dateTime;
                    bool canParse = DateTime.TryParse(input.ToString(), out dateTime);
                    if (((DateField)this.EnterCheckCodeEngine.CurrentView.CurrentField).IsInRange(dateTime) == false)
                    {
                        
                        return false;
                    }
                }
                else
                {
                    input = dialog.Input;
                }
                ret = result == DialogResult.OK ? true : false;
            }
            return ret;
        }

        /// <summary>
        /// Displays open file dialog
        /// </summary>
        /// <param name="caption">The caption of the dialog - aka title </param>
        /// <param name="filter">The filter for the dialog</param>
        /// <param name="isReadOnly">Sets the CheckFileExists property for the file dialog</param>
        /// <param name="filePath">The file path returned by the dialog</param>
        public bool TryGetFileDialog(string filter, string caption, bool isReadOnly, out string filePath)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = caption;
            openFileDialog.ShowHelp = false;
            filter = filter.Replace("\"", "");
            filter = filter.Trim();
            openFileDialog.Filter = filter == null ? string.Empty : filter;
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;
            openFileDialog.CheckFileExists = isReadOnly;
            DialogResult dialogResult = openFileDialog.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                filePath = openFileDialog.FileName.Trim();
                return true;
            }
            else
            {
                filePath = string.Empty;
                return false;
            }
        }

        /// <summary>
        /// AutoSearch
        /// </summary>
        /// <param name="pIdentifierList">Identified List</param>
        public void AutoSearch(string[] pIdentifierList, string[] pDisplayList, bool pAlwaysShow)
        {
            Collection<string> checkCodeList = new Collection<string>();
            for (int i = 0; i < pIdentifierList.Length; i++)
            {
                checkCodeList.Add(pIdentifierList[i]);
            }

            Collection<string> autoSearchFields = new Collection<string>();
            ArrayList autoSearchFieldValues = new ArrayList();
            Collection<string> autoSearchFieldItemTypes = new Collection<string>();
            Collection<string> comparisonTypes = new Collection<string>();
            ControlFactory factory = ControlFactory.Instance;
            Dictionary<string,int> OrFieldCount = new Dictionary<string,int>();
            bool ShowContinueNewDialog = false;
            bool isNewRecord = false;

            foreach (String fieldName in checkCodeList)
            {
                controlsList = GetAssociatedControls(fieldName);
                foreach (Control control in controlsList)
                {
                    if (!(control is Label))
                    {
                        if(OrFieldCount.ContainsKey(fieldName.ToLower()))
                        {
                            OrFieldCount[fieldName.ToLower()]++;
                        }
                        else
                        {
                            OrFieldCount.Add(fieldName.ToLower(),1);
                        }

                        autoSearchFields.Add(fieldName);
                        autoSearchFieldValues.Add(control.Text.Trim());

                        Field field = factory.GetAssociatedField(control);
                        autoSearchFieldItemTypes.Add(field.FieldType.ToString());
                        comparisonTypes.Add("=");
                    }
                }
            }

            DataTable data = this.view.GetProject().CollectedData.GetSearchRecords(this.view, OrFieldCount, autoSearchFields, autoSearchFieldItemTypes, comparisonTypes, autoSearchFieldValues);
            if (data != null && data.Rows.Count > 0)
            {
                if (data.Select(string.Format("GlobalRecordId = '{0}'", this.view.CurrentGlobalRecordId)).Length != 1)
                {
                    isNewRecord = true;
                }

                if (pAlwaysShow || isNewRecord)
                {
                    DataTable TempTable = new DataTable();
                    if (pDisplayList != null)
                    {
                        foreach (string s in pDisplayList)
                        {
                            if (s.Equals("CONTINUENEW"))
                            {
                                ShowContinueNewDialog = true;
                            }
                            else
                            {
                                TempTable.Columns.Add(new DataColumn(s, data.Columns[s].DataType));
                            }
                        }
                        if (TempTable.Columns.Count == 0)
                        {
                            TempTable = data.Clone();
                        }
                    }
                    else
                    {
                        TempTable = data.Clone();
                    }

                    if (!TempTable.Columns.Contains("UniqueKey"))
                    {
                        TempTable.Columns.Add(new DataColumn("UniqueKey", data.Columns["UniqueKey"].DataType));
                    }

                    DataTable ResultTable = data.Clone();
                    foreach (DataRow R in data.Rows)
                    {
                        ResultTable.ImportRow(R);
                    }

                    for (int i = 0; i < data.Columns.Count; i++)
                    {

                        if (data.Columns[i].ColumnName.ToLower().Contains("globalrecordid"))
                        {
                            ResultTable.Columns.Remove(data.Columns[i].ColumnName.ToString());
                        }
                        else if (pDisplayList != null)
                        {
                            bool isFound = false;
                            foreach (string s in pDisplayList)
                            {
                                if (data.Columns[i].ColumnName.Equals(s, StringComparison.OrdinalIgnoreCase))
                                {
                                    isFound = true;
                                    break;
                                }
                            }

                            if (!isFound && !data.Columns[i].ColumnName.Equals("UniqueKey", StringComparison.OrdinalIgnoreCase))
                            {
                                ResultTable.Columns.Remove(data.Columns[i].ColumnName.ToString());
                            }
                            else
                            {

                            }
                        }
                    }

                    foreach (DataRow R in ResultTable.Rows)
                    {
                        TempTable.ImportRow(R);
                    }

                    Dialogs.AutoSearchResults dialog = new Dialogs.AutoSearchResults(this.view, this.mainForm, TempTable, ShowContinueNewDialog);
                    DialogResult result = dialog.ShowDialog();
                    if (result == DialogResult.OK && false == ShowContinueNewDialog)
                    {
                        this.dirty = false;
                        this.view.IsDirty = false;
                        this.GoToRecordHandler(this, new GoToRecordEventArgs(this.MainForm.RecordId));
                    }
                }
            }
        }

        /// <summary>
        /// AutoSearch the Function
        /// </summary>
        /// <param name="pIdentifierList">Identified List</param>
        /// <param name="pDisplayList">Display List</param>
        /// <param name="pAlwaysShow">Always</param>
        /// <returns>Returns an integer with the number of records matching the search variable(s).</returns>
      
        public int AutosearchFunction(string[] pIdentifierList, string[] pDisplayList, bool pAlwaysShow)
        {
            Collection<string> checkCodeList = new Collection<string>();
            for (int i = 0; i < pIdentifierList.Length; i++)
            {
                checkCodeList.Add(pIdentifierList[i]);
            }

            Collection<string> autoSearchFields = new Collection<string>();
            ArrayList autoSearchFieldValues = new ArrayList();
            Collection<string> autoSearchFieldItemTypes = new Collection<string>();
            Collection<string> comparisonTypes = new Collection<string>();
            ControlFactory factory = ControlFactory.Instance;
            Dictionary<string, int> OrFieldCount = new Dictionary<string, int>();
            bool ShowContinueNewDialog = false;
            int ReturnRecordCount = -1;
            bool isNewRecord = false;

            foreach (String fieldName in checkCodeList)
            {
                controlsList = GetAssociatedControls(fieldName);
                foreach (Control control in controlsList)
                {
                    if (!(control is Label))
                    {
                        if (OrFieldCount.ContainsKey(fieldName.ToLower()))
                        {
                            OrFieldCount[fieldName.ToLower()]++;
                        }
                        else
                        {
                            OrFieldCount.Add(fieldName.ToLower(), 1);
                        }

                        autoSearchFields.Add(fieldName);
                        autoSearchFieldValues.Add(control.Text.Trim());

                        Field field = factory.GetAssociatedField(control);
                        autoSearchFieldItemTypes.Add(field.FieldType.ToString());
                        comparisonTypes.Add("=");
                    }
                }
            }

            DataTable data = this.view.GetProject().CollectedData.GetSearchRecords(this.view, OrFieldCount, autoSearchFields, autoSearchFieldItemTypes, comparisonTypes, autoSearchFieldValues);
            if (data != null && data.Rows.Count > 0)
            {
                if (data.Select(string.Format("GlobalRecordId = '{0}'", this.view.CurrentGlobalRecordId)).Length != 1)
                {
                    isNewRecord = true;
                }

                if (pAlwaysShow || isNewRecord)
                {
                    ReturnRecordCount = data.Rows.Count;
                    DataTable TempTable = new DataTable();
                    if (pDisplayList != null)
                    {
                        foreach (string s in pDisplayList)
                        {
                            if (s.Equals("CONTINUENEW"))
                            {
                                ShowContinueNewDialog = true;
                            }
                            else
                            {
                                TempTable.Columns.Add(new DataColumn(s, data.Columns[s].DataType));
                            }
                        }
                        if (TempTable.Columns.Count == 0)
                        {
                            TempTable = data.Clone();
                        }

                    }
                    else
                    {
                        TempTable = data.Clone();
                    }

                    if (!TempTable.Columns.Contains("UniqueKey"))
                    {
                        TempTable.Columns.Add(new DataColumn("UniqueKey", data.Columns["UniqueKey"].DataType));
                    }


                    DataTable ResultTable = data.Clone();
                    foreach (DataRow R in data.Rows)
                    {
                        ResultTable.ImportRow(R);
                    }

                    for (int i = 0; i < data.Columns.Count; i++)
                    {

                        if (data.Columns[i].ColumnName.ToLower().Contains("globalrecordid"))
                        {
                            ResultTable.Columns.Remove(data.Columns[i].ColumnName.ToString());
                        }
                        else if (pDisplayList != null)
                        {
                            bool isFound = false;
                            foreach (string s in pDisplayList)
                            {
                                if (data.Columns[i].ColumnName.Equals(s, StringComparison.OrdinalIgnoreCase))
                                {
                                    isFound = true;
                                    if (s.Equals("CONTINUENEW")) ShowContinueNewDialog = false;
                                    break;
                                }
                            }

                            if (!isFound && !data.Columns[i].ColumnName.Equals("UniqueKey", StringComparison.OrdinalIgnoreCase))
                            {
                                ResultTable.Columns.Remove(data.Columns[i].ColumnName.ToString());
                            }
                        }
                    }

                    foreach (DataRow R in ResultTable.Rows)
                    {
                        TempTable.ImportRow(R);
                    }

                    Dialogs.AutoSearchResults dialog = new Dialogs.AutoSearchResults(this.view, this.mainForm, TempTable, ShowContinueNewDialog);
                    DialogResult result = dialog.ShowDialog();
                    if (result == DialogResult.OK && false == ShowContinueNewDialog)
                    {
                        this.dirty = false;
                        this.view.IsDirty = false;
                        this.GoToRecordHandler(this, new GoToRecordEventArgs(this.MainForm.RecordId));
                    }
                }
            }
            return ReturnRecordCount;
        }

        /// <summary>
        /// IsUnique
        /// </summary>
        /// <param name="pIdentifierList">Identified List</param>
        public bool IsUnique(string[] pIdentifierList)
        {
            Collection<string> checkCodeList = new Collection<string>();
            for (int i = 0; i < pIdentifierList.Length; i++)
            {
                checkCodeList.Add(pIdentifierList[i]);
            }

            Collection<string> autoSearchFields = new Collection<string>();
            ArrayList autoSearchFieldValues = new ArrayList();
            Collection<string> autoSearchFieldItemTypes = new Collection<string>();
            Collection<string> comparisonTypes = new Collection<string>();
            ControlFactory factory = ControlFactory.Instance;
            Dictionary<string, int> OrFieldCount = new Dictionary<string, int>();
            bool isUnique = false;

            foreach (String fieldName in checkCodeList)
            {
                controlsList = GetAssociatedControls(fieldName);
                foreach (Control control in controlsList)
                {
                    if (!(control is Label))
                    {
                        if (OrFieldCount.ContainsKey(fieldName.ToLower()))
                        {
                            OrFieldCount[fieldName.ToLower()]++;
                        }
                        else
                        {
                            OrFieldCount.Add(fieldName.ToLower(), 1);
                        }

                        autoSearchFields.Add(fieldName);
                        autoSearchFieldValues.Add(control.Text.Trim());

                        Field field = factory.GetAssociatedField(control);
                        autoSearchFieldItemTypes.Add(field.FieldType.ToString());
                        comparisonTypes.Add("=");
                    }
                }
            }

            DataTable data = this.view.GetProject().CollectedData.GetSearchRecords(this.view, OrFieldCount, autoSearchFields, autoSearchFieldItemTypes, comparisonTypes, autoSearchFieldValues);
            if (data != null)
            {
                if ((data.Rows.Count == 0) || (data.Rows.Count == 1 && (data.Select(string.Format("GlobalRecordId = '{0}'", this.view.CurrentGlobalRecordId)).Length == 1)))
                {
                    isUnique = true;
                }
                else
                {
                    isUnique = false;
                }
            }
            return isUnique;
        }


        /// <summary>
        /// Quit
        /// </summary>
        public void Quit()
         {
            //--for 2327
            if (this.mainForm.IsRecordCloseable) 
            {
            this.canvas.UnsubscribeControlEventHandlers();
            this.mainForm.Close();
            this.CloseFormEventHandler(this, new EventArgs());
            }
        }

        /// <summary>
        /// New Record
        /// </summary>
        public void NewRecord()
        {
            //--2326
            if (this.mainForm.IsRecordCloseable)
            { 
            this.canvas.UnsubscribeControlEventHandlers();
            this.SaveRecord();

            if (this.View.IsRelatedView && this.View.ReturnToParent)
            {
                this.mainForm.CloseView();
                return;
            }
            ResetViewDataFields();
            
            this.canvas.UnsubscribeControlEventHandlers();
            this.CurrentRecordId = this.EnterCheckCodeEngine.CurrentView.CurrentRecordNumber;
            this.canvas.SetNewRecordValues();
            this.mainForm.UpdateAppSpecificInfo(SharedStrings.NEW_RECORD);
            this.mainForm.AddNewRecordSettings();
            this.viewExplorer.GoToFirstPage();
             
            this.Render();
            this.canvas.SubscribeControlEventHandlers();
            SetFocusToFirstControl(currentPage, _fieldPanel);
            }
        }

        private void ResetViewDataFields()
        {
            foreach (IDataField dataField in this.View.Fields.DataFields)
            {
                if (dataField is IInputField && ((IInputField)dataField).ShouldRepeatLast)
                {
                    this.View.IsDirty = true;
                }
                else
                {
                    dataField.CurrentRecordValueObject = null;
                }
            }

            if (View.IsRelatedView)
            {
                View.ForeignKeyField.CurrentRecordValueString = mainForm.RunTimeView.ParentGlobalRecordId;
            }

            View.GlobalRecordIdField.NewValue(); 
        }
        
        /// <summary>
        /// is ExecutionEnabled
        /// </summary>
        public bool IsExecutionEnabled
        {
            get { return this.enableCheckCodeExecution; }
            set { this.enableCheckCodeExecution = value; }
        }

        /// <summary>
        /// is SupressErrorsEnabled
        /// </summary>
        public bool IsSuppressErrorsEnabled
        {
            get { return this.enableSuppressErorrs; }
            set { this.enableSuppressErorrs = value; }
        }

        #endregion


        /// <summary>
        /// UnHide
        /// </summary>
        /// <param name="pNameList">Name List</param>
        /// <param name="pIsAnExceptList">Is An Except List</param>
        public void UnHide(string[] pNameList, bool pIsAnExceptList)
        {

            string[] FieldNameList = null;

            if (pNameList.Length == 1 && pNameList[0] == "*")
            {
                FieldNameList = new string[this.view.Fields.Count];

                int i = 0;
                foreach (Field fieldName in this.view.Fields)
                {
                    FieldNameList[i] = fieldName.Name;
                    i++;
                }
            }
            else
            {
                FieldNameList = pNameList;
            }

            if (this.view == null)
            {
                this.view = this.EnterCheckCodeEngine.CurrentView.View;
            }

            if (pIsAnExceptList)
            {
                foreach (Field fieldName in this.view.Fields)
                {
                    fieldName.IsVisible = true;
                }

                foreach (string fieldName in FieldNameList)
                {
                    this.view.Fields[fieldName].IsVisible = false;
                    if (this.view.Fields[fieldName] is GroupField)
                    {
                        ProcessHideUnhideGroupfield(fieldName, false);
                    } 
                }
            }
            else
            {
                foreach (string fieldName in FieldNameList)
                {
                    this.view.Fields[fieldName].IsVisible = true;
                    if (this.view.Fields[fieldName] is GroupField)
                    {
                        ProcessHideUnhideGroupfield(fieldName, true);
                    } 
                }
            }

            Collection<string> List = new Collection<string>();
            for (int i = 0; i < FieldNameList.Length; i++)
            {
                List.Add(FieldNameList[i]);
                if (view.Fields[FieldNameList[i].ToString()] is GroupField)
                {
                    String[] names = ((GroupField)(this.view.Fields[FieldNameList[i].ToString()])).ChildFieldNames.Split(new char[] { ',' });
                    foreach (String name in names)
                    {
                        List.Add(name);
                    }
                }     
            }

            try
            {
                if (pIsAnExceptList)
                {
                    ProcessUnhideExceptCommand(List);
                }
                else
                {
                    ProcessUnhideCommand(List);
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Processes the Unhide command for check code execution
        /// </summary>
        /// <param name="checkCodeList">A list of fields to unhide</param>
        public void ProcessUnhideCommand(Collection<string> checkCodeList)
        {
            #region Input Validation
            if (checkCodeList == null)
            {
                throw new ArgumentNullException("CheckCodeList");
            }
            #endregion  //Input Validation

            controlsList = GetAssociatedControls(checkCodeList);
            this.canvas.UnhideCheckCodeItems(controlsList);
        }

        /// <summary>
        /// Processes the Unhide Except command for check code execution
        /// </summary>
        /// <param name="checkCodeList">A list of fields</param>
        public void ProcessUnhideExceptCommand(Collection<string> checkCodeList)
        {
            #region Input Validation
            if (checkCodeList == null)
            {
                throw new ArgumentNullException("CheckCodeList");
            }
            #endregion  //Input Validation

            controlsList = GetAssociatedControls(checkCodeList);
            this.canvas.UnhideExceptCheckCodeItems(controlsList);
        }

        /// <summary>
        /// Clear
        /// </summary>
        /// <param name="pIdentifierList">Identifier List</param>
        public void Clear(string[] pIdentifierList)
        {
            if (this.view == null)
            {
                this.view = this.EnterCheckCodeEngine.CurrentView.View;
            }

            foreach (string fieldName in pIdentifierList)
            {
                if (this.view.Fields[fieldName] is IInputField)
                {
                    ((IInputField)this.view.Fields[fieldName]).CurrentRecordValueString = "";
                }

                if (this.view.Fields[fieldName] is GroupField)
                {
                    string[] childFieldNameArray = ((GroupField)this.view.Fields[fieldName]).ChildFieldNameArray;

                    if (childFieldNameArray.Length > 0)
                    {
                        for (int j = 0; j < childFieldNameArray.Length; j++)
                        {
                            if (this.view.Fields[childFieldNameArray[j]] is IInputField)
                            {
                                IInputField field = ((IInputField)this.view.Fields[childFieldNameArray[j]]);

                                field.CurrentRecordValueString = "";
                                field.CurrentRecordValueObject = null;
                            }
                        }
                    }
                }
            }
            
            Collection<string> List = new Collection<string>();
            for (int i = 0; i < pIdentifierList.Length; i++)
            {
                List.Add(pIdentifierList[i]);

                if (this.view.Fields[pIdentifierList[i]] is GroupField)
                {
                    string[] childFieldNameArray = ((GroupField)this.view.Fields[pIdentifierList[i]]).ChildFieldNameArray;

                    if (childFieldNameArray.Length > 0)
                    {
                        for (int j = 0; j < childFieldNameArray.Length; j++)
                        {
                            if (List.Contains(childFieldNameArray[j]) == false)
                            {
                                List.Add(childFieldNameArray[j]);
                            }
                        }
                    }
                }
            }

            try
            {
                ProcessClearCommand(List);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Processes the Clear command for check code execution
        /// </summary>
        /// <param name="checkCodeList">A list of fields to clear</param>
        public void ProcessClearCommand(Collection<string> checkCodeList)
        {
            #region Input Validation
            if (checkCodeList == null)
            {
                throw new ArgumentNullException("CheckCodeList");
            }
            #endregion  //Input Validation

            controlsList = GetAssociatedControls(checkCodeList);
            this.canvas.ClearCheckCodeItems(controlsList);
            //SetFieldData();
        }

        /// <summary>
        /// GoTo
        /// </summary>
        /// <param name="pDestination">Destination</param>
        public void GoTo(string pDestination, string targetPage = "", string targetForm = "")
        {
            if (pDestination == "" && targetPage == "" && targetForm != "")
            {
                 //---2225(gotoform parent from relate)
                SetFieldData();
                SaveRecord();
                Epi.View cView = view.GetProject().Metadata.GetViewByFullName(targetForm);
                if (View.IsRelatedView)
                {
                    if (cView.Id == 1)
                    {
                        this.CloseViewHandler(true, new EventArgs());
                    }
                    else
                    {
                        this.CloseViewHandler(false, new EventArgs());
                    }
                }
            }
            //--------

            else if (pDestination == "" && targetPage != "")
            {
                SetFieldData();
                SaveRecord();
                this.mainForm.OpenPage(targetPage, targetForm);
            }
            else if (pDestination != "")
            {
                Collection<string> checkCodeList = new Collection<string>();
                checkCodeList.Add(pDestination);
                int pagePosition;
                bool result;
                ControlFactory factory = ControlFactory.Instance;
                List<Control> CurrentControl = factory.GetAssociatedControls(EnterCheckCodeEngine.CurrentView.CurrentField);

                if (CurrentControl.Count > 1)
                {
                    SetFieldData(CurrentControl[1]);
                }
                else
                {
                    if (CurrentControl.Count > 0 && !(CurrentControl[0] is Label || EnterCheckCodeEngine.CurrentView.CurrentField is Epi.Fields.CommandButtonField))
                    {
                        SetFieldData(CurrentControl[0]);
                    }
                }

                //Go to next page
                if (checkCodeList[0].ToString() == "+1")
                {
                    SetFieldData();
                    SaveRecord();
                    GoToNextPage();
                }
                //Go to previous page
                else if (checkCodeList[0].ToString() == "-1")
                {
                    SetFieldData();
                    SaveRecord();
                    GoToPreviousPage();
                }
                else
                {
                    result = int.TryParse(checkCodeList[0].ToString(), out pagePosition);
                    //Go to specific page
                    if (result)
                    {
                        SetFieldData();
                        if (pagePosition == 0 || pagePosition > view.Pages.Count)
                        {
                            //Since this page does not exist, only navigate to the next control.
                            canvas.EnableTabToNextControl = true;
                        }
                        else
                        {
                            //Since page position starts with index 0, reset the page position to be one less
                            //than it is because when the check code was saved it was saved starting with a 1 based index 
                            //instead of a 0 based index.
                            pagePosition -= 1;//
                            for (int i = 0; i < view.Pages.Count; i++)
                            {
                                if (view.Pages[i].Position == pagePosition)
                                {
                                    OpenPageHandler(this, new PageSelectedEventArgs(view.Pages[i]));
                                    break;
                                }
                            }
                        }
                    }
                    else  //Go to specific field
                    {
                        // find page for field
                        foreach (Page page in view.Pages)
                        {
                            if (page.Fields.Contains(checkCodeList[0].ToString()))
                            {
                                pagePosition = page.Position;
                                break;
                            }
                        }
                        if (currentPage.Position == pagePosition)
                        {
                            Field field = null;
                            controlsList = GetAssociatedControls(checkCodeList);
                            foreach (Control control in controlsList)
                            {
                                field = ControlFactory.Instance.GetAssociatedField(control);
                                if (!Util.IsEmpty(field))
                                {
                                    if (!currentPage.Fields.Contains((RenderableField)field))
                                    {
                                        SaveRecord();
                                    }
                                }
                            }
                            canvas.EnableTabToNextControl = false;
                            if (field != null)
                            {
                                canvas.GoToField(controlsList, field);
                                foreach (Control control in controlsList)
                                {
                                    if (!(control is Label))
                                        canvas.GotoPageControl = control;
                                    canvas.GotoPageField = field;
                                }
                            }
                        }
                        else  //Go To specific field in other Page
                        {
                            SetFieldData();
                            // SaveRecord();
                            canvas.EnableTabToNextControl = false;
                            Field field = null;
                            canvas.IsGotoPageField = true;
                            OpenPageHandler(this, new PageSelectedEventArgs(view.Pages[pagePosition]));
                            controlsList = GetAssociatedControls(checkCodeList);
                            foreach (Control control in controlsList)
                            {
                                field = ControlFactory.Instance.GetAssociatedField(control);
                            }
                            canvas.GoToField(controlsList, field);
                            canvas.IsGotoPageField = false;
                        }
                    }
                }
                canvas.GotoPageControl = null; canvas.GotoPageField = null;
            }
        }

        /// <summary>
        /// Hide
        /// </summary>
        /// <param name="pNameList">Name List</param>
        /// <param name="pIsAnExceptList">Is An Except List</param>
        public void Hide(string[] pNameList, bool pIsAnExceptList)
        {
            if (this.view == null)
            {
                this.view = this.EnterCheckCodeEngine.CurrentView.View;
            }

            if (pIsAnExceptList)
            {
                foreach (Field fieldName in this.view.Fields)
                {
                    fieldName.IsVisible = false;
                }

                foreach (string fieldName in pNameList)
                {
                    this.view.Fields[fieldName].IsVisible = true;
                    if (this.view.Fields[fieldName] is GroupField)
                    {
                        ProcessHideUnhideGroupfield(fieldName, true);
                    }   
                }
            }
            else
            {
                foreach (string fieldName in pNameList)
                {
                    this.view.Fields[fieldName].IsVisible = false;
                    if (this.view.Fields[fieldName] is GroupField)
                    {
                        ProcessHideUnhideGroupfield(fieldName, false);
                    }   
                }
            }


            Collection<string> List = new Collection<string>();
            for (int i = 0; i < pNameList.Length; i++)
            {
                List.Add(pNameList[i]);
                if (view.Fields[pNameList[i].ToString()] is GroupField)
                {
                    String[] names = ((GroupField)(this.view.Fields[pNameList[i].ToString()])).ChildFieldNames.Split(new char[] { ',' });
                    foreach (String name in names)
                    {
                        List.Add(name);
                    }
                }
            }
            try
            {
                if (pIsAnExceptList)
                {
                    ProcessHideExceptCommand(List);
                }
                else
                {
                    ProcessHideCommand(List);
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Processes the Hide command for check code execution
        /// </summary>
        /// <param name="checkCodeList">A list of fields to hide</param>
        public void ProcessHideCommand(Collection<string> checkCodeList)
        {
            #region Input Validation
            if (checkCodeList == null)
            {
                throw new ArgumentNullException("CheckCodeList");
            }
            #endregion  //Input Validation

            controlsList = GetAssociatedControls(checkCodeList);
            this.canvas.HideCheckCodeItems(controlsList);
        }

        /// <summary>
        /// Processed the Hide Except command for check code execution
        /// </summary>
        /// <param name="checkCodeList">A list of fields</param>
        public void ProcessHideExceptCommand(Collection<string> checkCodeList)
        {
            #region Input Validation
            if (checkCodeList == null)
            {
                throw new ArgumentNullException("CheckCodeList");
            }
            #endregion  //Input Validation

            controlsList = GetAssociatedControls(checkCodeList);
            this.canvas.HideExceptCheckCodeItems(controlsList);
        }

        public void ProcessHideUnhideGroupfield(string fieldName, bool isvisible)
        {
            String[] names = ((GroupField)(this.view.Fields[fieldName])).ChildFieldNames.Split(new char[] { ',' });
            foreach (String name in names)
            {
                this.view.Fields[name].IsVisible = isvisible;
            }
        }

        public void Highlight(string[] pNameList, bool pIsAnExceptList)
        {
            if (this.view == null)
            {
                this.view = this.EnterCheckCodeEngine.CurrentView.View;
            }

            if (pIsAnExceptList)
            {
                foreach (Field fieldName in this.view.Fields)
                {
                    fieldName.IsHighlighted = true;
                }

                foreach (string fieldName in pNameList)
                {
                    this.view.Fields[fieldName].IsHighlighted = false;
                }
            }
            else
            {
                foreach (string fieldName in pNameList)
                {
                    this.view.Fields[fieldName].IsHighlighted = true;
                }
            }

            Collection<string> List = new Collection<string>();
            for (int i = 0; i < pNameList.Length; i++)
            {
                List.Add(pNameList[i]);
            }
            try
            {
                if (pIsAnExceptList)
                {
                    controlsList = GetAssociatedControls(List);
                    this.canvas.HighlightExceptCheckCodeItems(controlsList);
                }
                else
                {
                    controlsList = GetAssociatedControls(List);
                    this.canvas.HighlightCheckCodeItems(controlsList);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void UnHighlight(string[] pNameList, bool pIsAnExceptList)
        {
            if (this.view == null)
            {
                this.view = this.EnterCheckCodeEngine.CurrentView.View;
            }

            if (pIsAnExceptList)
            {
                foreach (Field fieldName in this.view.Fields)
                {
                    fieldName.IsHighlighted = false;
                }

                foreach (string fieldName in pNameList)
                {
                    this.view.Fields[fieldName].IsHighlighted = true;
                }
            }
            else
            {
                foreach (string fieldName in pNameList)
                {
                    this.view.Fields[fieldName].IsHighlighted = false;
                }
            }

            Collection<string> List = new Collection<string>();
            for (int i = 0; i < pNameList.Length; i++)
            {
                List.Add(pNameList[i]);
            }

            try
            {
                if (pIsAnExceptList)
                {
                    controlsList = GetAssociatedControls(List);
                    this.canvas.UnHighlightExceptCheckCodeItems(controlsList);
                }
                else
                {
                    controlsList = GetAssociatedControls(List);
                    this.canvas.UnHighlightCheckCodeItems(controlsList);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void Enable(string[] pNameList, bool pIsAnExceptList)
        {
            if (this.view == null)
            {
                this.view = this.EnterCheckCodeEngine.CurrentView.View;
            }

            if (pIsAnExceptList)
            {
                foreach (Field field in this.view.Fields)
                {
                    ChangeIsEnabled(field.Name, true);
                }

                foreach (string fieldName in pNameList)
                {
                    ChangeIsEnabled(fieldName, false);
                }
            }
            else
            {
                foreach (string fieldName in pNameList)
                {
                    ChangeIsEnabled(fieldName, true);
                }
            }

            Collection<string> list = new Collection<string>();
            for (int i = 0; i < pNameList.Length; i++)
            {
                list.Add(pNameList[i]);

                if (this.view.Fields[pNameList[i]] is GroupField)
                {
                    string[] childFieldNameArray = ((GroupField)this.view.Fields[pNameList[i]]).ChildFieldNameArray;

                    if (childFieldNameArray.Length > 0)
                    {
                        for (int j = 0; j < childFieldNameArray.Length; j++)
                        {
                            if (list.Contains(childFieldNameArray[j]) == false)
                            {
                                list.Add(childFieldNameArray[j]);
                            }
                        }
                    }
                }
            }

            try
            {
                if (pIsAnExceptList)
                {
                    controlsList = GetAssociatedControls(list);
                    this.canvas.EnableExceptCheckCodeItems(controlsList);
                }
                else
                {
                    controlsList = GetAssociatedControls(list);
                    this.canvas.EnableCheckCodeItems(controlsList);
                }
            }
            catch
            {

            }
        }

        public void Disable(string[] pNameList, bool pIsAnExceptList)
        {
            if (this.view == null)
            {
                this.view = this.EnterCheckCodeEngine.CurrentView.View;
            }

            if (pIsAnExceptList)
            {
                foreach (Field field in this.view.Fields)
                {
                    ChangeIsEnabled(field.Name, false);
                }

                foreach (string fieldName in pNameList)
                {
                    ChangeIsEnabled(fieldName, true);
                }
            }
            else
            {
                foreach (string fieldName in pNameList)
                {
                    ChangeIsEnabled(fieldName, false);
                }
            }

            Collection<string> list = new Collection<string>();
            for (int i = 0; i < pNameList.Length; i++)
            {
                list.Add(pNameList[i]);

                if (this.view.Fields[pNameList[i]] is GroupField)
                {
                    string[] childFieldNameArray = ((GroupField)this.view.Fields[pNameList[i]]).ChildFieldNameArray;

                    if (childFieldNameArray.Length > 0)
                    {
                        for (int j = 0; j < childFieldNameArray.Length; j++)
                        {
                            if (list.Contains(childFieldNameArray[j]) == false)
                            { 
                                list.Add(childFieldNameArray[j]);
                            }
                        }
                    }
                }
            }

            try
            {
                if (pIsAnExceptList)
                {
                    controlsList = GetAssociatedControls(list);
                    this.canvas.DisableExceptCheckCodeItems(controlsList);
                }
                else
                {
                    controlsList = GetAssociatedControls(list);
                    this.canvas.DisableCheckCodeItems(controlsList);
                }
            }
            catch
            {

            }
        }

        private void ChangeIsEnabled(string fieldName, bool isEnabled)
        {
            this.view.Fields[fieldName].IsEnabled = isEnabled;

            if (this.view.Fields[fieldName] is GroupField)
            {
                foreach (Panel panel in this.canvas.Panels)
                {
                    foreach (Control control in panel.Controls)
                    {
                        if (control.Name == fieldName)
                        {
                            control.Enabled = isEnabled;
                            break;
                        }
                    }
                }

                string[] childFieldNameArray = ((GroupField)this.view.Fields[fieldName]).ChildFieldNameArray;

                if (childFieldNameArray.Length > 0)
                {
                    for (int i = 0; i < childFieldNameArray.Length; i++)
                    {
                        this.view.Fields[childFieldNameArray[i]].IsEnabled = isEnabled;
                    }
                }
            }
        }


        public void SetRequired(string[] pNameList)
        {
            for (int i = 0; i < pNameList.Length; i++)
            {

                if (this.view.Fields[pNameList[i]] is InputFieldWithSeparatePrompt)
                {
                    InputFieldWithSeparatePrompt field = (InputFieldWithSeparatePrompt)this.view.Fields[pNameList[i]];
                    field.IsRequired = true;
                    List<Control> controlList = this.GetAssociatedControls(field.Name);
                    foreach (Control c in controlList)
                    {
                        this.canvas.DrawBorderRect(c);
                    }/**/
 
                }
            }
        }
        public void SetNotRequired(string[] pNameList)
        {
            for (int i = 0; i < pNameList.Length; i++)
            {

                if (this.view.Fields[pNameList[i]] is InputFieldWithSeparatePrompt)
                {
                    InputFieldWithSeparatePrompt field = (InputFieldWithSeparatePrompt)this.view.Fields[pNameList[i]];
                    field.IsRequired = false;
                    
                    List<Control> controlList = this.GetAssociatedControls(field.Name);
                    foreach (Control c in controlList)
                    {
                        this.canvas.DrawBorderRect(c);
                    }/**/
                }
            }
        }

        public List<string> GetDbListValues(string ptablename,string pvariablename)
        {
            List<string> list = new List<string>();
            Epi.Data.Services.CollectedDataProvider collectedData = this.EnterCheckCodeEngine.Project.CollectedData;
            DataTable dt = collectedData.GetTableData(ptablename);

            if (dt != null)
            {                             
                foreach (DataRow row in dt.Rows)
                {
                    string value = row[pvariablename].ToString();

                    if (list.Contains(value) == false && string.IsNullOrEmpty(value) == false)
                    {
                        list.Add(value);
                    }
                }                
            }               
            return list;
        }

    }
}
