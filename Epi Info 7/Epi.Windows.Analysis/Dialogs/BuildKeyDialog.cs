using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using Epi.Analysis;
using Epi.Collections;
using Epi.Core.AnalysisInterpreter;
using Epi.Data;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
    public partial class BuildKeyDialog : CommandDesignDialog
    {
        #region Private Members
        string relatedTable = string.Empty;
        private object selectedDataSource = null;
        string callingDialog = string.Empty;

        List<string> relatedFields = new List<string>();
        List<string> currentFields = new List<string>();

        List<string> relatedFields2 = new List<string>();
        List<string> currentFields2 = new List<string>();

        bool LoadIsFinished = false;
        /// <summary>
        /// key variable
        /// </summary>
        public string key = string.Empty;
        #endregion

        #region Public Properties
        /// <summary>
        /// List of related table column names.
        /// </summary>
        public string RelatedTable
        {
            get
            {
                return relatedTable;
            }

            set
            {
                relatedTable = value;
            }
        }

        /// <summary>
        /// selectedDataSource from calling dialog. Used to get the column names for the related table.
        /// </summary>
        public object SelectedDataSource
        {
            get
            {
                return selectedDataSource;
            }
            set
            {
                selectedDataSource = value;
            }
        }

        /// <summary>
        /// CallingDialog from the dialog that called this dialog. Used to determine if BuildKeyDialog was called by the
        /// MERGE or RELATE command dialogs so the appropriate instructions can be dispalyed.
        /// </summary>
        public string CallingDialog
        {
            get
            {
                return callingDialog;
            }
            set
            {
                callingDialog = value;
            }
        }


        /// <summary>
        /// KEY statement
        /// </summary>
        public string Key
        {
            get
            {
                return key;
            }

            set
            {
                key = value;
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor - NOT TO BE USED FOR INSTANTIATION
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public BuildKeyDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for BuildKey dialog
        /// </summary>
        public BuildKeyDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
        {
            InitializeComponent();
            Construct();
            
        }

        private void Construct()
        {
            if (!this.DesignMode)
            {
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            }
        }
        #endregion

        #region Event Handlers
        private void ClickHandler(object sender, System.EventArgs e)
        {
            ToolStripButton btn = (ToolStripButton)sender;
            txtKeyComponent.Text += StringLiterals.SPACE;
            if ((string)btn.Tag == null)
            {
                txtKeyComponent.Text += btn.Text;
            }
            else
            {
                txtKeyComponent.Text += (string)btn.Tag;
            }
            txtKeyComponent.Text += StringLiterals.SPACE;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtKeyComponent.Text = string.Empty;
            currentFields.Clear();
            relatedFields.Clear();
            currentFields2.Clear();
            relatedFields2.Clear();

        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/classic-analysis/How-to-Manage-Data-Use-the-MERGE-Command.html");
        }


        private void BuildKeyDialog_Load(object sender, EventArgs e)
        {
            if (SelectedDataSource != null)
            {
                if (SelectedDataSource is IDbDriver)
                {
                    IDbDriver db = SelectedDataSource as IDbDriver;
                    //--EI-114
                    // relatedFields = db.GetTableColumnNames(RelatedTable);
                    if (RelatedTable.Contains(StringLiterals.SPACE))
                    {
                        string pstr = "Select TOP 2 * from [" + RelatedTable + "]";
                        DataTable relfields = DBReadExecute.GetDataTable(db, pstr);
                        foreach (DataColumn dc in relfields.Columns)
                        {
                            relatedFields.Add(dc.ColumnName);
                        }
                    }
                    else
                    {
                        relatedFields = db.GetTableColumnNames(RelatedTable); 
                    }
                    //---
                    
                }
                else if (SelectedDataSource is Project)
                {
                    Project project = SelectedDataSource as Project;

                    if(project.Views.Exists(relatedTable))
                    {
                        foreach(Epi.Fields.IField field in project.Views[RelatedTable].Fields)
                        {
                            if (!(field is Epi.Fields.LabelField) & !(field is Epi.Fields.CommandButtonField) & !(field is Epi.Fields.PhoneNumberField)//EI-705
                                & !(field is Epi.Fields.MultilineTextField) & !(field is Epi.Fields.GroupField) & !(field is Epi.Fields.CheckBoxField)
                                & !(field is Epi.Fields.ImageField) & !(field is Epi.Fields.OptionField) & !(field is Epi.Fields.GridField)
                                & !(field is Epi.Fields.MirrorField))

                                    relatedFields.Add(field.Name);
                        }
                    }
                    else
                    {
                        relatedFields = project.GetTableColumnNames(RelatedTable);
                    }
                }
                if (this.EpiInterpreter.Context.DataSet != null)
                {    
                     View CurrentView=null;
                     if (this.EpiInterpreter.Context.CurrentProject != null)
                     {
                         CurrentView = this.EpiInterpreter.Context.CurrentProject.GetViewByName(this.EpiInterpreter.Context.CurrentRead.Identifier);//EI-705
                         foreach (DataColumn column in this.EpiInterpreter.Context.DataSet.Tables["Output"].Columns)
                         {
                             if ((CurrentView.Fields.Exists(column.ColumnName) && !(CurrentView.Fields[column.ColumnName] is Epi.Fields.LabelField)) & (CurrentView.Fields.Exists(column.ColumnName) && !(CurrentView.Fields[column.ColumnName] is Epi.Fields.CommandButtonField)) & (CurrentView.Fields.Exists(column.ColumnName) && !(CurrentView.Fields[column.ColumnName] is Epi.Fields.PhoneNumberField))
                                & (CurrentView.Fields.Exists(column.ColumnName) && !(CurrentView.Fields[column.ColumnName] is Epi.Fields.MultilineTextField)) & (CurrentView.Fields.Exists(column.ColumnName) && !(CurrentView.Fields[column.ColumnName] is Epi.Fields.GroupField)) & (CurrentView.Fields.Exists(column.ColumnName) && !(CurrentView.Fields[column.ColumnName] is Epi.Fields.CheckBoxField))
                                & (CurrentView.Fields.Exists(column.ColumnName) && !(CurrentView.Fields[column.ColumnName] is Epi.Fields.ImageField)) & (CurrentView.Fields.Exists(column.ColumnName) && !(CurrentView.Fields[column.ColumnName] is Epi.Fields.OptionField)) & (CurrentView.Fields.Exists(column.ColumnName) && !(CurrentView.Fields[column.ColumnName] is Epi.Fields.GridField))
                                & (CurrentView.Fields.Exists(column.ColumnName) && !(CurrentView.Fields[column.ColumnName] is Epi.Fields.MirrorField)))

                                 currentFields.Add(column.ColumnName);
                         }
                     }
                     else
                     {                        
                         foreach (DataColumn column in this.EpiInterpreter.Context.DataSet.Tables["Output"].Columns)
                         {
                             currentFields.Add(column.ColumnName);
                         }
                     }
                }
                currentFields.Sort();
            }
            //rdbCurrentTable.Checked = true;

            relatedFields.Sort();
            currentFields.Sort();

            //cmbAvailableVariables2.DataSource = relatedFields;
            lbxRelatedTableFields.DataSource = relatedFields;
            //cmbAvailableVariables.DataSource = currentFields;
            lbxCurrentTableFields.DataSource = currentFields;
            
            lbxCurrentTableFields.SelectedIndex = -1;
            lbxRelatedTableFields.SelectedIndex = -1;

            if (CallingDialog == "RELATE")
            {
                lblBuildKeyInstructions.Text = SharedStrings.BUILDKEY_RELATE_INSTRUCTIONS;
            }
            else
            {
                lblBuildKeyInstructions.Text = SharedStrings.BUILDKEY_MERGE_INSTRUCTIONS;
            }

            this.LoadIsFinished = true;

        }

        //private void selectedTable_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (rdbRelatedTable.Checked)
        //    {
        //        if (!string.IsNullOrEmpty(txtKeyComponent.Text))
        //        {
        //            lbxCurrentTableFields.Items.Add(txtKeyComponent.Text);
        //        }
                
        //    }
        //    else
        //    {
        //        if (!string.IsNullOrEmpty(txtKeyComponent.Text))
        //        {
        //            lbxRelatedTableFields.Items.Add(txtKeyComponent.Text);
        //        }
                
        //    }

        //    txtKeyComponent.Text = string.Empty;
        //    cmbAvailableVariables.SelectedIndex = -1;
        //}

        //private void cmbAvailableVariables_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    if (cmbAvailableVariables.SelectedIndex != -1)
        //    {
        //        string strSelectedVar = cmbAvailableVariables.SelectedItem.ToString();
        //        txtKeyComponent.Text += FieldNameNeedsBrackets(strSelectedVar) ? Util.InsertInSquareBrackets(strSelectedVar) : strSelectedVar; 
        //        cmbAvailableVariables.SelectedIndex = -1;
        //    }
        //}


        //private void cmbAvailableVariables2_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    if (cmbAvailableVariables.SelectedIndex != -1)
        //    {
        //        string strSelectedVar = cmbAvailableVariables2.SelectedItem.ToString();
        //        txtKeyComponent.Text += FieldNameNeedsBrackets(strSelectedVar) ? Util.InsertInSquareBrackets(strSelectedVar) : strSelectedVar;
        //        cmbAvailableVariables2.SelectedIndex = -1;
        //    }
        //}



        /// <summary>
        /// Override of OK click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void btnOK_Click(object sender, EventArgs e)
        {
            if (currentFields2.Count == relatedFields2.Count)
            {
                StringBuilder builtKey = new StringBuilder();
                for (int i = 0; i < currentFields2.Count; i++)
                {
                    if (i > 0)
                    {
                        builtKey.Append(StringLiterals.SPACE);
                        builtKey.Append("AND");
                        builtKey.Append(StringLiterals.SPACE);
                    }
                    builtKey.Append(currentFields2[i]);
                    builtKey.Append(StringLiterals.SPACE);
                    builtKey.Append(StringLiterals.COLON);
                    builtKey.Append(StringLiterals.COLON);
                    builtKey.Append(StringLiterals.SPACE);
                    builtKey.Append(relatedFields2[i]);
                }
                this.Key = builtKey.ToString();
                this.DialogResult = DialogResult.OK;
                this.Hide();
            }
            else
            {
                MsgBox.ShowError(SharedStrings.ERROR_RELATE_KEYS);
            }
        }
        #endregion

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //ListBox btn = (ListBox)sender;
            //txtKeyComponent.Text += StringLiterals.SPACE;
            //switch(btn.Text)
            //{
            //    case "\"Yes\"":
            //        txtKeyComponent.Text += "(+)";
            //        break;
            //    case "\"No\"":
            //        txtKeyComponent.Text += "(-)";
            //        break;
            //    case "\"Missing\"":
            //        txtKeyComponent.Text += "(.)";
            //        break;
            //    default:
            //        txtKeyComponent.Text += btn.Text;
            //        break;
            //}
            
            //txtKeyComponent.Text += StringLiterals.SPACE;
        }

        private void lbxRelatedTableFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.LoadIsFinished &&
                lbxCurrentTableFields.SelectedIndex != -1 &&
                lbxRelatedTableFields.SelectedIndex != -1
                )
            {

                string strSelectedVar = lbxCurrentTableFields.SelectedItem.ToString();
                AddLabel.Text = FieldNameNeedsBrackets(strSelectedVar) ? Util.InsertInSquareBrackets(strSelectedVar) : strSelectedVar;


                AddLabel.Text += StringLiterals.SPACE;
                AddLabel.Text += " = ";
                AddLabel.Text += StringLiterals.SPACE;

                strSelectedVar = lbxRelatedTableFields.SelectedItem.ToString();
                AddLabel.Text += FieldNameNeedsBrackets(strSelectedVar) ? Util.InsertInSquareBrackets(strSelectedVar) : strSelectedVar;
            }
            else
            {
                AddLabel.Text = "";
            }
        }

        private void lbxCurrentTableFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.LoadIsFinished &&
                lbxCurrentTableFields.SelectedIndex != -1 &&
                lbxRelatedTableFields.SelectedIndex != -1)
            {

                string strSelectedVar = lbxCurrentTableFields.SelectedItem.ToString();
                AddLabel.Text = FieldNameNeedsBrackets(strSelectedVar) ? Util.InsertInSquareBrackets(strSelectedVar) : strSelectedVar;


                AddLabel.Text += StringLiterals.SPACE;
                AddLabel.Text += " = ";
                AddLabel.Text += StringLiterals.SPACE;

                strSelectedVar = lbxRelatedTableFields.SelectedItem.ToString();
                AddLabel.Text += FieldNameNeedsBrackets(strSelectedVar) ? Util.InsertInSquareBrackets(strSelectedVar) : strSelectedVar;
            }
            else
            {
                AddLabel.Text = "";
            }
        }

        private void BackCommandButton_Click(object sender, EventArgs e)
        {
            //txtKeyComponent.SelectionStart = txtKeyComponent.Text.Length;
            if(txtKeyComponent.Text.Length > 0)
            {
                txtKeyComponent.Text = txtKeyComponent.Text.Remove(txtKeyComponent.Text.Length - 1);
            }
        }

        private void EraseWordCommandButton_Click(object sender, EventArgs e)
        {
            if (txtKeyComponent.Text.Length > 0 && txtKeyComponent.Text.LastIndexOf(" ") > 0)
            {
                txtKeyComponent.Text = txtKeyComponent.Text.Remove(txtKeyComponent.Text.LastIndexOf(" "));
            }
        }

        private void AddCommandButton_Click(object sender, EventArgs e)
        {
            if (lbxCurrentTableFields.SelectedIndex != -1 && 
                lbxRelatedTableFields.SelectedIndex != -1 )
            {
            
                

                string strSelectedVar = lbxCurrentTableFields.SelectedItem.ToString();
                currentFields2.Add(FieldNameNeedsBrackets(strSelectedVar) ? Util.InsertInSquareBrackets(strSelectedVar) : strSelectedVar); 
                txtKeyComponent.AppendText(FieldNameNeedsBrackets(strSelectedVar) ? Util.InsertInSquareBrackets(strSelectedVar) : strSelectedVar);
                lbxCurrentTableFields.SelectedIndex = -1;
            
            
                txtKeyComponent.AppendText(StringLiterals.SPACE);
                txtKeyComponent.AppendText(" = ");
                /*
                switch (this.listBox1.Text)
                {
                    case "\"Yes\"":
                        txtKeyComponent.Text += "(+)";
                        break;
                    case "\"No\"":
                        txtKeyComponent.Text += "(-)";
                        break;
                    case "\"Missing\"":
                        txtKeyComponent.Text += "(.)";
                        break;
                    default:
                        txtKeyComponent.Text += this.listBox1.Text;
                        break;
                }*/

                txtKeyComponent.AppendText(StringLiterals.SPACE);

                strSelectedVar = lbxRelatedTableFields.SelectedItem.ToString();
                relatedFields2.Add(FieldNameNeedsBrackets(strSelectedVar) ? Util.InsertInSquareBrackets(strSelectedVar) : strSelectedVar);
                txtKeyComponent.AppendText(FieldNameNeedsBrackets(strSelectedVar) ? Util.InsertInSquareBrackets(strSelectedVar) : strSelectedVar);
                lbxRelatedTableFields.SelectedIndex = -1;

                txtKeyComponent.AppendText("\n");

                AddLabel.Text = "";
            }


        }

        #region Private Methods

        #endregion


    }
}