using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using Epi;
using Epi.Data.Services;
using Epi.Windows.MakeView.Utils;
using Epi.Fields;
using Epi.Windows.Docking;
using EpiInfo.Plugin;

namespace Epi.Windows.MakeView.Forms
{   
    /// <summary>
    /// The text editor portion of the check code editor
    /// </summary>
    public partial class EditorToolWindow : DockWindow
    {
        #region Public Event Handlers

        /// <summary>
        /// Occurs when the cancel button is clicked
        /// </summary>
        public event EventHandler CancelButtonClicked;      

        #endregion  //Public Event Handlers

        #region Fields

        private bool isDefineCommand = false;
        private View view;
        private Collection<string> declaredVariables = null;
        private bool SuppressTabOut = false;
        private int currentFieldItem;
        private List<string> sortedFieldNames = new List<string>();
        private EpiInfo.Plugin.IEnterInterpreter memoryRegion;
        #endregion  Fields

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="frm">The main form</param>
        public EditorToolWindow(Epi.Windows.MakeView.Forms.MakeViewMainForm frm)
            : base(frm)
        {
            InitializeComponent();
            declaredVariables = new Collection<string>();
            this.txtTextArea.AcceptsTab = true;
            this.memoryRegion = frm.EpiInterpreter;
        }

        #endregion  //Constructors

        #region Public Properties

        /// <summary>
        /// The View for which the check code is being loaded
        /// </summary>
        public View View
        {
            get
            {
                return view;
            }
            set
            {
                view = value;
                BuildComboBox();
                cbxEvents.SelectedIndex = 1;             
            }
        }

        public bool IsDefineCommand
        {
            set { this.isDefineCommand = value; }
        }

        /// <summary>
        /// Block Leave Focus
        /// </summary>
        public bool BlockLeaveFocus
        {
            get { return this.SuppressTabOut; }
        }
        #endregion  //Public Properties

        #region Public Methods

        /// <summary>
        /// Adds the check code to the text area
        /// </summary>
        /// <param name="commandText">The check code text</param>
        public void AddCommand(string commandText)
        {
            if (commandText.ToUpperInvariant().StartsWith("DEFINE "))
            {
                cbxFields.SelectedIndex = cbxFields.Items.Count - 1;
            }

            if (!string.IsNullOrEmpty(txtTextArea.Text.Trim()))
            {
                commandText = Environment.NewLine + commandText;
            }
            else
            {
                txtTextArea.Text = txtTextArea.Text.Trim();
            }
            txtTextArea.AppendText(commandText);
            
            
        }

        /// <summary>
        /// Preselects the view in the check code combo box
        /// </summary>
        public void SelectView()
        {
            foreach (object item in cbxFields.Items)
            {
                if (((EditorComboBoxItem)item).Type.Equals("View"))
                {
                    cbxFields.SelectedItem = item;
                    break;
                }
            }
        }

        /// <summary>
        /// Preselects a page in the check code combo box
        /// </summary>
        /// <param name="page">The page to select</param>
        public void SelectPage(Page page)
        {
            foreach (object item in cbxFields.Items)
            {
                if (((EditorComboBoxItem)item).IDVal is Page)
                {
                    if (((Page)((EditorComboBoxItem)item).IDVal).Id.Equals(page.Id))
                    {
                        cbxFields.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Preselects a field in the check code combo box
        /// </summary>
        /// <param name="field">The field to select</param>
        public void SelectField(Field field)
        {
            foreach (object item in cbxFields.Items)
            {
                if (((EditorComboBoxItem)item).IDVal is Field)
                {
                    if (((Field)((EditorComboBoxItem)item).IDVal).Id.Equals(field.Id))
                    {
                        cbxFields.SelectedItem = item;                        
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Saves the contents of the editor
        /// </summary>
        /// <param name="indexToSave">Index of the item to save.</param>
        public void Save(int indexToSave)
        {
            EditorComboBoxItem selectedItem = (EditorComboBoxItem)cbxFields.Items[indexToSave];
            string checkCode = txtTextArea.Text.Replace("\n", "\r\n");
            checkCode = checkCode.Trim(Environment.NewLine.ToCharArray());

            IMetadataProvider metadata = GetMetadata();
            if (cbxEvents.SelectedIndex == 0 && (selectedItem).IDVal is IFieldWithCheckCodeBefore)
            {
                Field beforeField = (selectedItem).IDVal as Field;
                ((IFieldWithCheckCodeBefore)beforeField).CheckCodeBefore = checkCode;
                metadata.CreateControlBeforeCheckCode(beforeField.Id, ((IFieldWithCheckCodeBefore)beforeField).CheckCodeBefore, this.view);
            }
            else if(cbxEvents.SelectedIndex == 1 && (selectedItem).IDVal is IFieldWithCheckCodeAfter)
            {
                Field afterField = (selectedItem).IDVal as Field;
                ((IFieldWithCheckCodeAfter)afterField).CheckCodeAfter = checkCode;
                metadata.CreateControlAfterCheckCode(afterField.Id, ((IFieldWithCheckCodeAfter)afterField).CheckCodeAfter, this.view);
            }
            else if (cbxEvents.SelectedIndex == 1 && (selectedItem).IDVal is IFieldWithCheckCodeClick)
            {
                Field afterField = (selectedItem).IDVal as Field;
                ((IFieldWithCheckCodeClick)afterField).CheckCodeClick = checkCode;
                metadata.CreateControlAfterCheckCode(afterField.Id, ((IFieldWithCheckCodeClick)afterField).CheckCodeClick, this.view);
            }

            #region Replaced this code with the code above using the interfaces
            //if (((EditorComboBoxItem)cbxFields.SelectedItem).IDVal is InputTextBoxField)
            //{
            //    if (cbxEvents.SelectedIndex == 0)
            //    {
            //        ((InputTextBoxField)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).CheckCodeBefore = txtTextArea.Text;
            //        metadata.CreateControlBeforeCheckCode(((InputTextBoxField)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).Id, txtTextArea.Text, this.View);         
            //    }
            //    else
            //    {
            //        ((InputTextBoxField)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).CheckCodeAfter = txtTextArea.Text;
            //        metadata.CreateControlAfterCheckCode(((InputTextBoxField)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).Id, txtTextArea.Text, this.View);
            //    }                
            //}
            //else if (((EditorComboBoxItem)cbxFields.SelectedItem).IDVal is TableBasedDropDownField)
            //{
            //    if (cbxEvents.SelectedIndex == 0)
            //    {
            //        ((TableBasedDropDownField)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).CheckCodeBefore = txtTextArea.Text;
            //        metadata.CreateControlBeforeCheckCode(((TableBasedDropDownField)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).Id, txtTextArea.Text, this.View);
            //    }
            //    else
            //    {
            //        ((TableBasedDropDownField)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).CheckCodeAfter = txtTextArea.Text;
            //        metadata.CreateControlAfterCheckCode(((TableBasedDropDownField)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).Id, txtTextArea.Text, this.View);
            //    }
            //}
            //else if (((EditorComboBoxItem)cbxFields.SelectedItem).IDVal is CommandButtonField)
            //{
            //    ((CommandButtonField)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).CheckCodeClick = txtTextArea.Text;
            //    metadata.CreateControlAfterCheckCode(((CommandButtonField)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).Id, txtTextArea.Text, this.View);
            //}
            //else if (((EditorComboBoxItem)cbxFields.SelectedItem).IDVal is CheckBoxField)
            //{
            //    ((CheckBoxField)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).CheckCodeAfter = txtTextArea.Text;
            //    metadata.CreateControlAfterCheckCode(((CheckBoxField)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).Id, txtTextArea.Text, this.View);
            //}
            //else if (((EditorComboBoxItem)cbxFields.SelectedItem).IDVal is OptionField)
            //{
            //    ((OptionField)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).CheckCodeAfter = txtTextArea.Text;
            //    metadata.CreateControlAfterCheckCode(((OptionField)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).Id, txtTextArea.Text, this.View);
            //}
            //else if (((EditorComboBoxItem)cbxFields.SelectedItem).IDVal is YesNoField)
            //{
            //    ((YesNoField)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).CheckCodeAfter = txtTextArea.Text;
            //    metadata.CreateControlAfterCheckCode(((YesNoField)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).Id, txtTextArea.Text, this.View);
            //}
            #endregion 

            else if ((selectedItem).IDVal is Page)
            {
                if (cbxEvents.SelectedIndex == 0)
                {
                    ((Page)(selectedItem).IDVal).CheckCodeBefore = checkCode;
                    metadata.CreatePageBeforeCheckCode(((Page)(selectedItem).IDVal).Id, checkCode, this.View);
                }
                else
                {
                    ((Page)(selectedItem).IDVal).CheckCodeAfter = checkCode;
                    metadata.CreatePageAfterCheckCode(((Page)(selectedItem).IDVal).Id, checkCode, this.View);
                }
            }
            else if ((selectedItem).Type.Equals("View"))
            {
                if (cbxEvents.SelectedIndex == 0)
                {
                    ((View)(selectedItem).IDVal).CheckCodeBefore = checkCode;
                    metadata.CreateViewBeforeCheckCode(((View)(selectedItem).IDVal).Id, checkCode);
                }
                else
                {
                    ((View)(selectedItem).IDVal).WebSurveyId = checkCode;
                    metadata.CreateViewAfterCheckCode(((View)(selectedItem).IDVal).Id, checkCode);
                }
            }
            else if ((selectedItem).Type.Equals("Record"))
            {
                if (cbxEvents.SelectedIndex == 0)
                {
                    ((View)(selectedItem).IDVal).RecordCheckCodeBefore = checkCode;
                    metadata.CreateRecordBeforeCheckCode(((View)(selectedItem).IDVal).Id, checkCode);
                }
                else
                {
                    ((View)(selectedItem).IDVal).RecordCheckCodeAfter = checkCode;
                    metadata.CreateRecordAfterCheckCode(((View)(selectedItem).IDVal).Id, checkCode);
                }
            }
            else
            {
                ((View)(selectedItem).IDVal).CheckCodeVariableDefinitions = checkCode;
                metadata.CreateCheckCodeVariableDefinition(((View)(selectedItem).IDVal).Id, checkCode);
                //if (ShouldUndefineVariables())
                //{
                //    UndefineVariables();                       
                //}
                //LoadDeclaredVariables();
            }
        }

        #endregion  Public Methods

        #region Private Methods

        private IMetadataProvider GetMetadata()
        {
            return View.GetMetadata();
        }

        /// <summary>
        /// Loads the items in the combo box
        /// </summary>
        private void BuildComboBox()
        {
            cbxFields.Items.Clear();
            sortedFieldNames.Clear();
            foreach (Page page in view.Pages)
            {
                if (!string.IsNullOrEmpty(page.CheckCodeAfter.ToString().Trim()) || (!string.IsNullOrEmpty(page.CheckCodeBefore.ToString().Trim())))
                {
                    cbxFields.Items.Add(new EditorComboBoxItem("*" + page.Name, page, "Page"));
                }
                else
                {
                    cbxFields.Items.Add(new EditorComboBoxItem(page.Name, page, "Page"));
                }

                foreach (Epi.Fields.Field field in page.Fields)
                {               
                    sortedFieldNames.Add(field.Name);
                }
                sortedFieldNames.Sort();

                foreach (string name in sortedFieldNames)
                {
                    foreach (Epi.Fields.Field field in page.Fields)
                    {
                        if (field.Name == name)
                        {
                            if (field is IFieldWithCheckCodeAfter || field is IFieldWithCheckCodeBefore || field is IFieldWithCheckCodeClick)
                            {
                                AddEditorComboBoxItem(field);
                            }
                        }
                    }
                }               
            }

            if (!string.IsNullOrEmpty(view.WebSurveyId.ToString().Trim()) || (!string.IsNullOrEmpty(view.CheckCodeBefore.ToString().Trim())))
            {
                cbxFields.Items.Add(new EditorComboBoxItem("*View", view, "View"));
            }
            else
            {
                cbxFields.Items.Add(new EditorComboBoxItem("View", view, "View"));
            }


            if (!string.IsNullOrEmpty(view.RecordCheckCodeAfter.ToString().Trim()) || (!string.IsNullOrEmpty(view.RecordCheckCodeBefore.ToString().Trim())))
            {
                cbxFields.Items.Add(new EditorComboBoxItem("*Record", view, "Record"));
            }
            else
            {
                cbxFields.Items.Add(new EditorComboBoxItem("Record", view, "Record"));
            }

            if (!string.IsNullOrEmpty(view.CheckCodeVariableDefinitions))
            {
                cbxFields.Items.Add(new EditorComboBoxItem("*DefinedVariables", view, "DefinedVariables"));
            }
            else
            {
                cbxFields.Items.Add(new EditorComboBoxItem("DefinedVariables", view, "DefinedVariables"));             
            }
         
        }

        private void AddEditorComboBoxItem(Epi.Fields.Field field)
        {
            string lead = "     ";
            bool hasValue = false;
            if (field is IFieldWithCheckCodeBefore || field is IFieldWithCheckCodeAfter || field is IFieldWithCheckCodeClick)
            {
                if (field is IFieldWithCheckCodeBefore && !String.IsNullOrEmpty(((IFieldWithCheckCodeBefore)field).CheckCodeBefore.ToString().Trim()))
                {
                    hasValue = true;
                }
                if (field is IFieldWithCheckCodeAfter && !String.IsNullOrEmpty(((IFieldWithCheckCodeAfter)field).CheckCodeAfter.ToString().Trim()))
                {
                    hasValue = true;
                }
                if (field is IFieldWithCheckCodeClick && !String.IsNullOrEmpty(((IFieldWithCheckCodeClick)field).CheckCodeClick.ToString().Trim()))
                {
                    hasValue = true;
                }

                if (hasValue)
                {
                    lead += "*";
                }                

                cbxFields.Items.Add(new EditorComboBoxItem(lead + field.Name, field, (field.GetType()).Name));                
            }
        }

        /// <summary>
        /// Gets the check code in the editor
        /// </summary>
        private string GetCheckCode()
        {
            if (!string.IsNullOrEmpty(txtTextArea.SelectedText))
            {
                return txtTextArea.SelectedText;
            }
            else
            {
                return txtTextArea.Text.Trim();
            }
        }

        /// <summary>
        /// Compiles check code
        /// </summary>
        /// <param name="checkCode">The check code to be compiled</param>
        /// <returns>Determines whether check code was compiled</returns>
        private bool CompileCheckCode(String checkCode)
        {
            bool isValidCommand = true;
            try
            {
                // Before doing another validation on the check code, ensure that all colors have been
                // cleared and re-set back to the default of blue.
                txtTextArea.SelectAll();
                txtTextArea.SelectionColor = Color.Blue;
                txtTextArea.DeselectAll();

                txtTextArea.SelectedText = String.Empty;
                ((MakeViewMainForm)this.mainForm).EpiInterpreter.Parse(checkCode);
            }
            catch (ParseException ex)
            {
                isValidCommand = false;
                MsgBox.ShowError(SharedStrings.INVALID_CHECKCODE);
                int start = txtTextArea.GetFirstCharIndexFromLine(ex.UnexpectedToken.Location.LineNr);
                int length = txtTextArea.Lines[ex.UnexpectedToken.Location.LineNr].Length;
                txtTextArea.Select(start, length);
                txtTextArea.SelectionColor = Color.Red;
                //Logger.Log(DateTime.Now + ":  " + ex.Message);
            }
            catch (Exception ex)
            {
                isValidCommand = false;
                MsgBox.ShowError(SharedStrings.INVALID_CHECKCODE);
                //Logger.Log(DateTime.Now + ":  " + ex.Message);
            }
            return isValidCommand;
        }

        /// <summary>
        /// Selects the after event for checkbox, command button and option fields, if selected in the fields combo box 
        /// </summary>
        private void SelectAfterEvent()
        {
            if (cbxFields.SelectedItem != null)
            {
                if (((EditorComboBoxItem)cbxFields.SelectedItem).IDVal is CheckBoxField || ((EditorComboBoxItem)cbxFields.SelectedItem).IDVal is CommandButtonField ||
                    ((EditorComboBoxItem)cbxFields.SelectedItem).IDVal is OptionField)
                {
                    cbxEvents.SelectedIndex = 1;
                }
                
            }
        }

        /// <summary>
        /// Reloads the fields drop down and updates the check code indicator
        /// </summary>
        private void ResetCheckCodeIndicator()
        {
            int currentlySelectedIndex = cbxFields.SelectedIndex;
            BuildComboBox();
            cbxFields.SelectedIndex = currentlySelectedIndex;
           // cbxEvents.SelectedIndex = 0;
            
        }

        /// <summary>
        /// Denotes whether a given command can be generated
        /// </summary>
        public void CanCommandBeGenerated()
        {
            if (((EditorComboBoxItem)cbxFields.SelectedItem).IDVal is Page || (((EditorComboBoxItem)cbxFields.SelectedItem).Type.Equals("View") ||
                ((EditorComboBoxItem)cbxFields.SelectedItem).Type.Equals("Record") || ((EditorComboBoxItem)cbxFields.SelectedItem).Type.Equals("DefinedVariables")))
            {
                ((MakeViewMainForm)this.mainForm).CanCommandBeGenerated = false;
            }
            else
            {
                ((MakeViewMainForm)this.mainForm).CanCommandBeGenerated = true;
            }
        }       

        /// <summary>
        /// Loads declared variables when declared variables in selected in the fields combo box
        /// </summary>
        private void LoadDeclaredVariables()
        {
            declaredVariables.Clear();
            cbxEvents.SelectedIndex = 0;
            //IMemoryRegion memoryRegion = Epi.Core.Interpreter.Reduction.MemoryRegion;
            foreach (IVariable variable in memoryRegion.Context.GetVariablesInScope())
            {
                if (variable.VarType.Equals(VariableType.Global) || variable.VarType.Equals(VariableType.Permanent) || variable.VarType.Equals(VariableType.Standard))
                {
                    if (!string.IsNullOrEmpty(txtTextArea.Text.Trim()))
                    {
                        txtTextArea.AppendText(StringLiterals.NEW_LINE);
                    }

                    StringBuilder sb = new StringBuilder();
                    sb.Append(CommandNames.DEFINE);
                    sb.Append(StringLiterals.SPACE).Append(variable.Name.ToString());

                    if (variable.VarType.Equals(VariableType.Global) || variable.VarType.Equals(VariableType.Permanent))
                    {
                        sb.Append(StringLiterals.SPACE).Append(variable.VarType.ToString().ToUpperInvariant());
                    }

                    if (!string.IsNullOrEmpty(variable.DataType.ToString()))
                    {
                        DataRow row = AppData.Instance.DataTypesDataTable.FindByDataTypeId((int)variable.DataType);
                        string expression = row[ColumnNames.EXPRESSION].ToString();
                        sb.Append(StringLiterals.SPACE).Append(expression.ToUpperInvariant());
                    }

                    if (variable.PromptText != null)
                    {
                        if (!string.IsNullOrEmpty(variable.PromptText.ToString()))
                        {
                            sb.Append(StringLiterals.SPACE).Append("(").Append(StringLiterals.DOUBLEQUOTES);
                            sb.Append(variable.PromptText.ToString().Trim()).Append(StringLiterals.DOUBLEQUOTES).Append(")");
                        }
                    }
                    txtTextArea.AppendText(sb.ToString());
                    declaredVariables.Add(variable.Name.Trim());
                }
            }
        }

        /// <summary>
        /// Determine if variable(s) should be undefined
        /// </summary>
        /// <returns></returns>
        private bool ShouldUndefineVariables()
        {
            string[] lines;
            if (!string.IsNullOrEmpty(txtTextArea.Text.Trim()))
            {
                lines = txtTextArea.Text.Trim().Split(StringLiterals.NEW_LINE.ToCharArray());
            }
            else
            {
                lines = new string[0];
            }
                       
            if (declaredVariables.Count != lines.Length)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Undefine variable(s) from the memory region
        /// </summary>
        private void UndefineVariables()
        {
            //IMemoryRegion memoryRegion = Epi.Core.Interpreter.Reduction.MemoryRegion;
            Collection<string> variablesinTextArea = new Collection<string>();

            if (!string.IsNullOrEmpty(txtTextArea.Text.Trim()))
            {
                string[] lines = txtTextArea.Text.Trim().Split(StringLiterals.NEW_LINE.ToCharArray());
                foreach (String line in lines)
                {
                    string[] content = line.Split(StringLiterals.SPACE.ToCharArray());
                    variablesinTextArea.Add(content[1].ToString().Trim());
                }

                if (declaredVariables.Count > 0)
                {
                    if (lines.Length != declaredVariables.Count)
                    {
                        foreach (String declaredVar in declaredVariables)
                        {
                            if (!(variablesinTextArea.Contains(declaredVar)))
                            {
                                memoryRegion.Context.UndefineVariable(declaredVar);
                            }
                        }
                    }
                }
            }
            else
            {
                if (declaredVariables.Count > 0)
                {
                    foreach (String declaredVar in declaredVariables)
                    {
                        memoryRegion.Context.UndefineVariable(declaredVar);
                    }
                }
            }
        }

        private bool ContinueOnCheckCodeChanged(string currentCheckCode)
        {
            string checkCodeFromField = string.Empty;
            EditorComboBoxItem selectedField = ((EditorComboBoxItem)cbxFields.Items[currentFieldItem]);

            if (cbxEvents.SelectedIndex == 0 && (selectedField).IDVal is IFieldWithCheckCodeBefore)
            {
                Field beforeField = (selectedField).IDVal as Field;
                checkCodeFromField = ((IFieldWithCheckCodeBefore)beforeField).CheckCodeBefore;
            }
            else if (cbxEvents.SelectedIndex == 1 && (selectedField).IDVal is IFieldWithCheckCodeAfter)
            {
                Field afterField = (selectedField).IDVal as Field;
                checkCodeFromField = ((IFieldWithCheckCodeAfter)afterField).CheckCodeAfter;
            }
            else if (cbxEvents.SelectedIndex == 1 && (selectedField).IDVal is IFieldWithCheckCodeClick)
            {
                Field afterField = (selectedField).IDVal as Field;
                checkCodeFromField = ((IFieldWithCheckCodeClick)afterField).CheckCodeClick;
            }
            else if (selectedField.IDVal is Page)
            {
                if (cbxEvents.SelectedIndex == 0)
                {
                    checkCodeFromField = ((Page)selectedField.IDVal).CheckCodeBefore;
                }
                else
                {
                    checkCodeFromField = ((Page)selectedField.IDVal).CheckCodeAfter;
                }
            }
            else
            {
                if (selectedField.Type.Equals("View"))
                {
                    if (cbxEvents.SelectedIndex == 0)
                    {
                        checkCodeFromField = ((View)selectedField.IDVal).CheckCodeBefore;
                    }
                    else
                    {
                        checkCodeFromField = ((View)selectedField.IDVal).WebSurveyId;
                    }
                }
                else if (selectedField.Type.Equals("Record"))
                {
                    if (cbxEvents.SelectedIndex == 0)
                    {
                        checkCodeFromField = ((View)selectedField.IDVal).RecordCheckCodeBefore;
                    }
                    else
                    {
                        checkCodeFromField = ((View)selectedField.IDVal).RecordCheckCodeAfter;
                    }
                }
                else if (selectedField.Type.Equals("DefinedVariables"))
                {
                    checkCodeFromField = ((View)selectedField.IDVal).CheckCodeVariableDefinitions;
                }
            }

            checkCodeFromField = checkCodeFromField.Trim().Trim(Environment.NewLine.ToCharArray());
            if (!checkCodeFromField.Equals(currentCheckCode.Trim().Trim(Environment.NewLine.ToCharArray())))
            {
                DialogResult result = MsgBox.ShowQuestion(SharedStrings.CHECKCODE_CHANGED, MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    if (string.IsNullOrEmpty(currentCheckCode) || CompileCheckCode(currentCheckCode))
                    {
                        txtTextArea.Text = currentCheckCode;
                        Save(cbxFields.SelectedIndex);
                        ResetCheckCodeIndicator();
                    }
                    else
                    {
                        cbxFields.SelectedItem = cbxFields.Items[currentFieldItem];
                        return false;
                    }
                }
            }
            return true;
        }

        #endregion  Private Methods

        #region Private Event Handlers

        /// <summary>
        /// Handles the Click event of the Save button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnSave_Click(object sender, EventArgs e)
        {
            string checkCodeValue = txtTextArea.Text;
            if (!string.IsNullOrEmpty(checkCodeValue.Trim()))
            {
                checkCodeValue = ModifyInvalidText(checkCodeValue);
                if (CompileCheckCode(checkCodeValue))
                {
                    txtTextArea.Text = checkCodeValue;
                    Save(cbxFields.SelectedIndex);
                    // If we get here, we assume that we have not received any syntax errors and thus
                    // can force the font color back to its default of blue
                    txtTextArea.SelectAll();
                    txtTextArea.SelectionColor = Color.Blue;
                }
                else
                {
                    return;
                }
            }
            else
            {
                Save(cbxFields.SelectedIndex);
            }                    
            ResetCheckCodeIndicator();
        }

        private string ModifyInvalidText(string checkCodeValue)
        {
            checkCodeValue = checkCodeValue.Replace("( + )", "(+)");
            checkCodeValue = checkCodeValue.Replace("( - )", "(-)");
            checkCodeValue = checkCodeValue.Replace("\n", "\r\n");
            return checkCodeValue;
        }        

        /// <summary>
        /// Handles the Selected Index Change event of the Fields combo box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void cbxFields_SelectedIndexChanged(object sender, EventArgs e)
        {
                OnSelectedObjectOrEventChanged();
                SelectAfterEvent();
                CanCommandBeGenerated();
        }

        /// <summary>
        /// Handles the Click event of the Cancel button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (CancelButtonClicked != null)
            {
                CancelButtonClicked(this, new EventArgs());
            }
        }

        /// <summary>
        /// Handles the Selected Index Change event of the Events combo box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void cbxEvents_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnSelectedObjectOrEventChanged();
            SelectAfterEvent();
        }


        /// <summary>
        /// Saves text area when a selection on the form has been made
        /// </summary>
        private void OnSelectedObjectOrEventChanged()
        {
            if (cbxFields.SelectedItem != null)
            {          
                Field currField = ((EditorComboBoxItem)cbxFields.SelectedItem).IDVal as Field;

                if (cbxEvents.SelectedIndex == 0 && currField is IFieldWithCheckCodeBefore)
                {
                    txtTextArea.Text = ((IFieldWithCheckCodeBefore)currField).CheckCodeBefore;
                }
                else if (cbxEvents.SelectedIndex == 1 && currField is IFieldWithCheckCodeAfter)
                {
                    txtTextArea.Text = ((IFieldWithCheckCodeAfter)currField).CheckCodeAfter;
                }
                else if (cbxEvents.SelectedIndex == 1 && currField is IFieldWithCheckCodeClick)
                {
                    txtTextArea.Text = ((IFieldWithCheckCodeClick)currField).CheckCodeClick;
                }
                else
                {
                    if (((EditorComboBoxItem)cbxFields.SelectedItem).Type.Equals("View"))
                    {
                        if (cbxEvents.SelectedIndex == 0)
                        {
                            txtTextArea.Text = ((View)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).CheckCodeBefore;
                        }
                        else
                        {
                            txtTextArea.Text = ((View)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).WebSurveyId;
                        }
                    }
                    else if (((EditorComboBoxItem)cbxFields.SelectedItem).Type.Equals("Record"))
                    {
                        if (cbxEvents.SelectedIndex == 0)
                        {
                            txtTextArea.Text = ((View)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).RecordCheckCodeBefore;
                        }
                        else
                        {
                            txtTextArea.Text = ((View)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).RecordCheckCodeAfter;
                        }
                    }
                    else if (((EditorComboBoxItem)cbxFields.SelectedItem).Type.Equals("DefinedVariables"))
                    {
                        txtTextArea.Text = ((View)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).CheckCodeVariableDefinitions;
                    }
                    else if (((EditorComboBoxItem)cbxFields.SelectedItem).Type.Equals("Page"))
                    {
                        if (cbxEvents.SelectedIndex == 0)
                        {
                            txtTextArea.Text = ((Page)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).CheckCodeBefore;
                        }
                        else
                        {
                            txtTextArea.Text = ((Page)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).CheckCodeAfter;
                        }
                    }
                }
                currentFieldItem = cbxFields.SelectedIndex;
            }
        }

        /// <summary>
        /// Handles the Text Changed Event of the text area
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtTextArea_TextChanged(object sender, EventArgs e)
        {          
            //btnSave.Enabled = !string.IsNullOrEmpty(txtTextArea.Text.ToString());
            btnSave.Enabled = true;
            btnPrint.Enabled = !string.IsNullOrEmpty(txtTextArea.Text.ToString());
        }

        /// <summary>
        /// Handles the Click event of the Print button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnPrint_Click(object sender, EventArgs e)
        {
            System.Drawing.Printing.PrintDocument docToPrint = new System.Drawing.Printing.PrintDocument();
            docToPrint.PrintPage += new PrintPageEventHandler(docToPrint_PrintPage);
            PrintDialog dialog = new PrintDialog();
            dialog.AllowSelection = true;
            dialog.Document = docToPrint;
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                try
                {
                    docToPrint.Print();
                }
                catch (Win32Exception ex)
                {
                    MsgBox.Show(ex.Message, SharedStrings.MAKE_VIEW);
                }

                catch (Exception ex)
                {
                    MsgBox.ShowException(ex);
                }
            }
        }

        /// <summary>
        /// Handles the Print Page event
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied print page event parameters</param>
        private void docToPrint_PrintPage(object sender, PrintPageEventArgs e)
        {
            string textToPrint = GetCheckCode();
            Font printFont = new Font("Courier New", 12);
            e.Graphics.DrawString(textToPrint, printFont, Brushes.Black, 0, 0);
        }

        /// <summary>
        /// Handles the click event of the Fields dropdown to check for changes and prompt the user to save.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxFields_Click(object sender, EventArgs e)
        {
            if (cbxFields.DroppedDown)
            {
                if (ContinueOnCheckCodeChanged(ModifyInvalidText(txtTextArea.Text)))
                {
                    cbxFields.DroppedDown = true;
                }
            }
        }

        /// <summary>
        /// Handles teh click event of the Events dropdown to check for changes and prompt the user to save.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxEvents_Click(object sender, EventArgs e)
        {
            if (cbxEvents.DroppedDown)
            {
                if (ContinueOnCheckCodeChanged(ModifyInvalidText(txtTextArea.Text)))
                {
                    cbxEvents.DroppedDown = true;
                }
            }
        }

        private Epi.Windows.MakeView.Forms.CheckCode checkCodeForm;

        /// <summary>
        /// Check Code Form
        /// </summary>
        public Epi.Windows.MakeView.Forms.CheckCode CheckCodeForm
        {
            get
            {
                return checkCodeForm;
            }
            set
            {
                checkCodeForm = value;
                checkCodeForm.FormClosing += new FormClosingEventHandler(checkCode_FormClosing);
            }
        }

        private void checkCode_FormClosing(Object sender, FormClosingEventArgs e)
        {
            if (!ContinueOnCheckCodeChanged(ModifyInvalidText(txtTextArea.Text)))
            {
                e.Cancel = true;
            }
        }

        #endregion  //Private Event Handlers       

        private void PopulateTextArea()
        {
            foreach (EditorComboBoxItem ECBI in cbxFields.Items)
            {

                if (cbxFields.SelectedItem != null)
                {
                    Field currField = ((EditorComboBoxItem)cbxFields.SelectedItem).IDVal as Field;

                    if (cbxEvents.SelectedIndex == 0 && currField is IFieldWithCheckCodeBefore)
                    {
                        txtTextArea.Text = ((IFieldWithCheckCodeBefore)currField).CheckCodeBefore;
                    }
                    else if (cbxEvents.SelectedIndex == 1 && currField is IFieldWithCheckCodeAfter)
                    {
                        txtTextArea.Text = ((IFieldWithCheckCodeAfter)currField).CheckCodeAfter;
                    }
                    else if (cbxEvents.SelectedIndex == 1 && currField is IFieldWithCheckCodeClick)
                    {
                        txtTextArea.Text = ((IFieldWithCheckCodeClick)currField).CheckCodeClick;
                    }
                    else
                    {
                        if (((EditorComboBoxItem)cbxFields.SelectedItem).Type.Equals("View"))
                        {
                            if (cbxEvents.SelectedIndex == 0)
                            {
                                txtTextArea.Text = ((View)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).CheckCodeBefore;
                            }
                            else
                            {
                                txtTextArea.Text = ((View)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).WebSurveyId;
                            }
                        }
                        else if (((EditorComboBoxItem)cbxFields.SelectedItem).Type.Equals("Record"))
                        {
                            if (cbxEvents.SelectedIndex == 0)
                            {
                                txtTextArea.Text = ((View)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).RecordCheckCodeBefore;
                            }
                            else
                            {
                                txtTextArea.Text = ((View)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).RecordCheckCodeAfter;
                            }
                        }
                        else if (((EditorComboBoxItem)cbxFields.SelectedItem).Type.Equals("DefinedVariables"))
                        {
                            txtTextArea.Text = ((View)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).CheckCodeVariableDefinitions;
                        }
                        else if (((EditorComboBoxItem)cbxFields.SelectedItem).Type.Equals("Page"))
                        {
                            if (cbxEvents.SelectedIndex == 0)
                            {
                                txtTextArea.Text = ((Page)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).CheckCodeBefore;
                            }
                            else
                            {
                                txtTextArea.Text = ((Page)((EditorComboBoxItem)cbxFields.SelectedItem).IDVal).CheckCodeAfter;
                            }
                        }
                    }
                    currentFieldItem = cbxFields.SelectedIndex;
                }
            }
        }

    }
}
