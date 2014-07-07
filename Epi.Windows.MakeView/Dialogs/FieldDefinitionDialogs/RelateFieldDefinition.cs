#region Namespaces
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Fields;
using Epi;
using EpiInfo.Plugin;
using VariableCollection = Epi.Collections.NamedObjectCollection<Epi.IVariable>;

#endregion
namespace Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs
{
    /// <summary>
    /// Field definition dialog for relate fields
    /// </summary>
    public partial class RelateFieldDefinition : CommandButtonFieldDefinition
    {
        #region Private Members
        private RelatedViewField field;
        private EpiInfo.Plugin.IEnterInterpreter  memoryRegion;
        private bool useExistingView = true;
        #endregion Private Members

        #region Constructors
        /// <summary>
        /// Default Constsructor, for exclusive use by the designer.
        /// </summary>
        public RelateFieldDefinition()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for the Relate Field Definition
        /// </summary>
        /// <param name="frm">The main form</param>
        public RelateFieldDefinition(Epi.Windows.MakeView.Forms.MakeViewMainForm frm)
            : base(frm)
        {
            InitializeComponent();
            this.memoryRegion = frm.EpiInterpreter;
        }

        /// <summary>
        /// Constructor for the Relate Field Definition
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="page">The page</param>
        public RelateFieldDefinition(Epi.Windows.MakeView.Forms.MakeViewMainForm frm, Page page)
            : base(frm)
        {
            InitializeComponent();
            this.memoryRegion = frm.EpiInterpreter;
            this.mode = FormMode.Create;
            this.page = page;
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="frm">The parent form</param>
        /// <param name="field">The fied to be edited</param>
        public RelateFieldDefinition(Epi.Windows.MakeView.Forms.MakeViewMainForm frm, RelatedViewField field)
            : base(frm)
        {
            InitializeComponent();
            this.mode = FormMode.Edit;
            this.field = field;
            this.page = field.Page;
            this.memoryRegion = frm.EpiInterpreter;
            LoadFormData();
        }
        #endregion // Constructors

        #region Private Methods
        /// <summary>
        /// Loads default settings into the form
        /// </summary>
        private void LoadFormData()
        {
            Configuration config = Configuration.GetNewInstance();
            FontStyle style = FontStyle.Regular;
            
            if (config.Settings.EditorFontBold)
            {
                style |= FontStyle.Bold;
            }
            
            if (config.Settings.EditorFontItalics)
            {
                style |= FontStyle.Italic;
            }
            
            if ((field.ControlFont == null) || ((field.ControlFont.Name == "Microsoft Sans Serif") && (field.ControlFont.Size == 8.5)))
            {
                field.ControlFont = new Font(config.Settings.EditorFontName, (float)config.Settings.EditorFontSize, style);
            }
            
            txtPrompt.Text = field.PromptText;
            txtPrompt.Font = field.ControlFont;
            txtFieldName.Text = field.Name;

            List<EpiInfo.Plugin.IVariable> vars = this.memoryRegion.Context.GetVariablesInScope();
            foreach (EpiInfo.Plugin.IVariable v in vars)
            {
                if (v.VariableScope == VariableScope.Standard || v.VariableScope == VariableScope.DataSource)
                {
                    cbxVariables.Items.Add(v.Name);
                    cbxVariables.SelectedIndex = -1;
                }
            }
            
            cbxVariables.Sorted = true;
            DataTable cbxDs = new DataTable();
            cbxDs.Columns.Add("RelatedFormId", typeof(int));
            cbxDs.Columns.Add("FormName", typeof(string));
            int SelectedIndex = -1;
            object[] row = new object[2];
            row[0] = -1;
            row[1] = "(create new form)";
            cbxDs.Rows.Add(row);

            Epi.Project project = field.GetView().Project;

            View viewPointer = field.GetView();
            int currentViewId = viewPointer.Id;

            List<int> lineage = new List<int>();

            lineage.Add(currentViewId);

            while (viewPointer.ParentView != null)
            {
                lineage.Add(viewPointer.ParentView.Id);
                viewPointer = viewPointer.ParentView;
            }

            foreach (Epi.View view in project.Views)
            {
                if (lineage.Contains(view.Id) == false)
                {
                    cbxRelatedView.Items.Add(view.Name);
                    row = new object[2];
                    row[0] = view.Id;
                    row[1] = view.Name;
                    cbxDs.Rows.Add(row);

                    if (view.Id == field.RelatedViewID)
                    {
                        SelectedIndex = cbxDs.Rows.Count - 1;
                    }
                }
            }

            cbxRelatedView.DataSource = cbxDs;
            cbxRelatedView.ValueMember = "RelatedFormId";
            cbxRelatedView.DisplayMember = "FormName";
            cbxRelatedView.SelectedIndex = SelectedIndex;

            if (field.ChildView != null)
            {
                cbxRelatedView.Text = field.ChildView.Name;
            }

            if (field.Condition.Length > 0)
            {
                rdbAccessibleWithConditions.Checked = true;
                txtCondition.Text = field.Condition;
            }
            else
            {
                rdbAccessibleAlways.Checked = true;
                txtCondition.Text = string.Empty;
            }

            controlFont = field.ControlFont;
            chkReturnToParent.Checked = field.ShouldReturnToParent;
        }

        /// <summary>
        /// Common event handler for build expression buttons
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void ClickHandler(object sender, System.EventArgs e)
        {
            ToolStripButton btn = (ToolStripButton)sender;
            
            if ((string)btn.Tag == null)
            {
                if (btn.Text.ToString() != "\"")
                {
                    txtCondition.Text += StringLiterals.SPACE;
                    txtCondition.Text += btn.Text;
                    txtCondition.Text += StringLiterals.SPACE;
                }
                else
                {
                    txtCondition.Text += btn.Text;
                }
            }
            else
            {
                txtCondition.Text += (string)btn.Tag;
                txtCondition.Text += StringLiterals.SPACE;
            }
            
            
        }

        /// <summary>
        /// Creates a view
        /// </summary>
        /// <returns>View that was created</returns>
        private View PromptUserToCreateView()
        {
            View newView = null;
            Project project = field.Page.GetProject();
            CreateViewDialog viewDialog = new CreateViewDialog(MainForm, project, txtFieldName.Text);
            viewDialog.ShowDialog();
            if (viewDialog.DialogResult == DialogResult.OK)
            {
                string newViewName = viewDialog.ViewName;
                //newView = ((ProjectNode)projectTree.Nodes[0]).Project.CreateView(newViewName);
                newView = project.CreateView(newViewName, true);
                viewDialog.Close();
            }
            return newView;
        }
        #endregion //Private Methods

        #region Protected Methods
        /// <summary>
        /// Validate Dialog Input flag
        /// </summary>
        /// <returns></returns>
        protected override bool ValidateDialogInput()
        {
            ErrorMessages.Clear();
            if (!string.IsNullOrEmpty(txtCondition.Text))
            {
                try
                {
                    //string fireControlName = "fireControlName";
                    //string relatedFieldName = "relatedFieldName";
                    //string condition = txtCondition.Text.Trim();
                    //string checkCode = string.Format("field {0} after if {1} then unhide {2} else hide {2} end-if end-before end-field", fireControlName, condition, relatedFieldName);
                    //((Epi.Windows.MakeView.Forms.MakeViewMainForm)this.mainForm).EpiInterpreter.Parse(checkCode);
                }
                catch (Exception ex)
                {
                    ErrorMessages.Add(ex.Message);
                }
            }
            else
            {
                if (rdbAccessibleWithConditions.Checked == true)
                {
                    ErrorMessages.Add(SharedStrings.WARNING_ADD_RELATE_CONDITION);
                }
            }
            // DEFECT# 321 & 384 fix.
            if (cbxRelatedView.Text != field.GetView().Name)
            {
                if (cbxRelatedView.SelectedItem == null || ((System.Data.DataRowView)cbxRelatedView.SelectedItem)[0].ToString() == "-1")
                {
                    View view = PromptUserToCreateView();
                    if (view != null)
                    {
                        DataTable cbxDs = (DataTable)cbxRelatedView.DataSource;
                        Page page = view.CreatePage("New Page", 0);
                        object[] row = new object[2];
                        row[0] = view.Id;
                        row[1] = view.Name;
                        cbxDs.Rows.Add(row);
                        cbxRelatedView.Text = view.Name;
                        UseExistingView = false;
                        view.IsRelatedView = true;
                    }
                    else
                    {
                        ErrorMessages.Add("The user elected not to create a related form.");
                    }
                    //                ErrorMessages.Add(SharedStrings.WARNING_ADD_RELATED_VIEW_NAME);
                }
                else
                {
                    View v = field.GetView().Project.Views[cbxRelatedView.Text];
                    v.IsRelatedView = true;
                }
            }
            else
            {
                ErrorMessages.Add("Warning. Cannot relate to the current form.");
            }
            return (ErrorMessages.Count.Equals(0));
        }

        /// <summary>
        /// Sets the field's properties based on GUI values
        /// </summary>
        protected override void SetFieldProperties()
        {
            field.PromptText = txtPrompt.Text;
            if (field.Id == 0)
            {
                field.Name = txtFieldName.Text;
            }

            if (controlFont != null)
            {
                field.ControlFont = controlFont;
            }
            if (promptFont != null)
            {
                field.PromptFont = promptFont;
            }
            // DEFECT# 321 fix.
            if (cbxRelatedView.SelectedValue != null)
            {
                field.RelatedViewID = int.Parse(cbxRelatedView.SelectedValue.ToString());
            }
            if (rdbAccessibleAlways.Checked == true)
            {
                field.Condition = string.Empty;
            }
            else
            {
                field.Condition = txtCondition.Text;
            }

            field.ShouldReturnToParent = chkReturnToParent.Checked;
        }
        #endregion //Protected Methods

        #region Public Methods
        /// <summary>
        /// Gets the field defined by this field definition dialog
        /// </summary>
        public override RenderableField Field
        {
            get
            {
                return field;
            }
        }

        public bool UseExistingView 
        {
            get 
            {
                return this.useExistingView;
            }
            set
            {
                this.useExistingView = value;
            }
        }
        #endregion	//Public Methods

        #region Event Handlers
        /// <summary>
        /// Handles the CheckChanged event for the conditional relate check box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void rdbAccessibleWithConditions_CheckedChanged(object sender, EventArgs e)
        {
            lblCondition.Enabled = rdbAccessibleWithConditions.Checked;
            txtCondition.Enabled = rdbAccessibleWithConditions.Checked;
            lblAvailableVariables.Enabled = rdbAccessibleWithConditions.Checked;
            cbxVariables.Enabled = rdbAccessibleWithConditions.Checked;
            grpOperators.Enabled = rdbAccessibleWithConditions.Checked;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event for the drop-down list of variables
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void cbxVariables_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cbxVariables.Text))
            {
                txtCondition.Text += cbxVariables.Text;
            }
        }

        #endregion // Event Handlers
    }
}

