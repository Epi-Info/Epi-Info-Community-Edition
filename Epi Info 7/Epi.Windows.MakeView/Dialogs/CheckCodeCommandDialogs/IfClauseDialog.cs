#region Namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Epi;
using Epi.Data.Services;
using Epi.Windows.Dialogs;

#endregion //Namespaces

namespace Epi.Windows.MakeView.Dialogs.CheckCodeCommandDialogs
{
	/// <summary>
	/// ***** This is a wireframe and currently contains no functionality *****
	/// </summary>
    public partial class IfClauseDialog : CheckCodeDesignDialog
	{
        #region Fields
        private CommandContextMenuType CurrentContextMenuType;
        private View view;
        #endregion  //Fields

        #region Constructors

        /// <summary>
		/// Constructor for the class
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public IfClauseDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for If Clause dialog
        /// </summary>
        /// <param name="frm">The main form</param>
		public IfClauseDialog(MainForm frm) : base(frm)
        {
            InitializeComponent();
            this.EpiInterpreter = ((Epi.Windows.MakeView.Forms.MakeViewMainForm)frm).EpiInterpreter;
        }

        #endregion  //Constructors

        #region Public Properties

        /// <summary>
        /// Sets the View for the dialog
        /// </summary>
        public override View View
        {
            set
            {
                this.view = value;
                foreach (Fields.Field field in value.Fields)
                {
                    if (field is Fields.RenderableField && !(field is Fields.LabelField))
                    {
                        cbxVariables.Items.Add(field.Name);
                    }
                }

                foreach (EpiInfo.Plugin.IVariable var in this.EpiInterpreter.Context.GetVariablesInScope(EpiInfo.Plugin.VariableScope.Global | EpiInfo.Plugin.VariableScope.Permanent))
                {
                    cbxVariables.Items.Add(var.Name);
                }
            }
        }
        #endregion  //Public Properties


        #region Private Event Handlers        
        
        /// <summary>
		/// Cancel button closes this dialog 
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
        }

        /// <summary>
        /// Handles the Click event of the Ok button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnOk_Click(object sender, System.EventArgs e)
        {
            System.Text.StringBuilder thenBlockBuilder = new System.Text.StringBuilder();
            foreach (string line in txtThen.Lines)
            {
                thenBlockBuilder.Append("\t\t\t");
                thenBlockBuilder.Append(line + "\n");
            }
            System.Text.StringBuilder elseBlockBuilder = new System.Text.StringBuilder();
            foreach (string line in txtElse.Lines)
            {
                elseBlockBuilder.Append("\t\t\t");
                elseBlockBuilder.Append(line + "\n");
            }
            System.Text.StringBuilder outputBuilder = new System.Text.StringBuilder();
            outputBuilder.Append(CommandNames.IF + StringLiterals.SPACE);
            outputBuilder.Append(txtCondition.Text + StringLiterals.SPACE + CommandNames.THEN + "\n");
            outputBuilder.Append(thenBlockBuilder.ToString());
            if (txtElse.Lines.Length > 0)
            {
                outputBuilder.Append("\t\t" + CommandNames.ELSE + "\n");
                outputBuilder.Append(elseBlockBuilder.ToString());
            }
            outputBuilder.Append("\t\tEND-IF\n");
            Output = outputBuilder.ToString();
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        /// <summary>
        /// Handles the Click event of the Clear button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnClear_Click(object sender, EventArgs e)
        { //bug 952
            txtCondition.Text = string.Empty;
            txtThen.Text = string.Empty;
            txtElse.Text = string.Empty;
            cbxVariables.Text = "";
        }


        /// <summary>
        /// Handles the Text Change event of the Condition textbox
        /// </summary>
        /// <param name="sender">Object that fired the event </param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtCondition_TextChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = (txtCondition.Text.Length > 0 && txtThen.Text.Length > 0);
        }

        /// <summary>
        /// Handles the Text Change event of the Then textbox
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtThen_TextChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = (txtCondition.Text.Length > 0 && txtThen.Text.Length > 0);
        }

        /// <summary>
        /// Handles the Text Change event of the Else textbox
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtElse_TextChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = (txtCondition.Text.Length > 0 && txtThen.Text.Length > 0);
        }

        #region DeletedCode
        ///// <summary>
        ///// Handles the Click event of the Plus button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnPlus_Click(object sender, EventArgs e)
        //{
        //    int position = txtCondition.SelectionStart;
        //    txtCondition.Text = txtCondition.Text.Insert(position, "+ ");
        //    txtCondition.Select(position + 2, 0);
        //}

        ///// <summary>
        ///// Handles the Click event of the Minus button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnMinus_Click(object sender, EventArgs e)
        //{
        //    int position = txtCondition.SelectionStart;
        //    txtCondition.Text = txtCondition.Text.Insert(position, "- ");
        //    txtCondition.Select(position + 2, 0);
        //}

        ///// <summary>
        ///// Handles the Click event of the Multiplication button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnTimes_Click(object sender, EventArgs e)
        //{
        //    int position = txtCondition.SelectionStart;
        //    txtCondition.Text = txtCondition.Text.Insert(position, "* ");
        //    txtCondition.Select(position + 2, 0);
        //}

        ///// <summary>
        ///// Handles the Click event of the Divide button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnDivide_Click(object sender, EventArgs e)
        //{
        //    int position = txtCondition.SelectionStart;
        //    txtCondition.Text = txtCondition.Text.Insert(position, "/ ");
        //    txtCondition.Select(position + 2, 0);
        //}

        ///// <summary>
        ///// Handles the Click event of the Equal button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnEqual_Click(object sender, EventArgs e)
        //{
        //    //txtCondition.AppendText("= ");

        //    int position = txtCondition.SelectionStart;
        //    txtCondition.Text = txtCondition.Text.Insert(position, "= ");
        //    txtCondition.Select(position + 2, 0);

        //    int length = txtCondition.Text.Length;
        //    if ((length - 4 >= 0) && (
        //        txtCondition.Text[length - 4] == '>' || txtCondition.Text[length - 4] == '<')
        //        && txtCondition.Text[length - 3] == ' ')
        //    {
        //        txtCondition.Text = txtCondition.Text.Remove(length - 3, 1);
        //        txtCondition.Select(position + 1, 0);
        //    }


        //    //txtCondition.Select(txtCondition.Text.Length, 0);

        //    //txtCondition.Focus();
        //}

        ///// <summary>
        ///// Handles the Click event of the Less Than button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnLess_Click(object sender, EventArgs e)
        //{
        //    int position = txtCondition.SelectionStart;
        //    txtCondition.Text = txtCondition.Text.Insert(position, "< ");
        //    txtCondition.Select(position + 2, 0);
        //}

        ///// <summary>
        ///// Handles the Click event of the Greater Than button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnGreat_Click(object sender, EventArgs e)
        //{
        //    int position = txtCondition.SelectionStart;
        //    txtCondition.Text = txtCondition.Text.Insert(position, "> ");
        //    txtCondition.Select(position + 2, 0);

        //    int length = txtCondition.Text.Length;
        //    if (length - 4 >= 0 &&
        //        txtCondition.Text[length - 3] == ' ' &&
        //        txtCondition.Text[length - 4] == '<')
        //    {
        //        txtCondition.Text = txtCondition.Text.Remove(length - 3, 1);
        //        txtCondition.Select(position + 1, 0);
        //    }

        //}

        ///// <summary>
        ///// Handles the Click event of the Ampersand button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnAmp_Click(object sender, EventArgs e)
        //{
        //    int position = txtCondition.SelectionStart;
        //    txtCondition.Text = txtCondition.Text.Insert(position, "& ");
        //    txtCondition.Select(position + 2, 0);
        //}

        ///// <summary>
        ///// Handles the Click event of the double quotes mark button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event  parameters</param>
        //private void btnQuote_Click(object sender, EventArgs e)
        //{
        //    int position = txtCondition.SelectionStart;
        //    txtCondition.Text = txtCondition.Text.Insert(position, "\"");
        //    txtCondition.Select(position + 1, 0);
        //}

        ///// <summary>
        ///// Handles the Click event of the open parenthesis button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnOpenParen_Click(object sender, EventArgs e)
        //{
        //    int position = txtCondition.SelectionStart;
        //    txtCondition.Text = txtCondition.Text.Insert(position, "( ");
        //    txtCondition.Select(position + 2, 0);
        //}

        ///// <summary>
        ///// Handles the Click event of the closed parenthesis button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnCloseParen_Click(object sender, EventArgs e)
        //{
        //    int position = txtCondition.SelectionStart;
        //    txtCondition.Text = txtCondition.Text.Insert(position, ") ");
        //    txtCondition.Select(position + 2, 0);

        //    //int length = txtCondition.Text.Length;
        //    //if (length - 6 >= 0 && 
        //    //    txtCondition.Text[length - 3] == ' ' &&
        //    //    (txtCondition.Text[length - 4] == '.' || txtCondition.Text[length - 4] == '-' || txtCondition.Text[length - 4] == '+') &&
        //    //    txtCondition.Text[length - 5] == ' ' &&
        //    //    txtCondition.Text[length - 6] == '(')
        //    //{
        //    //    txtCondition.Text = txtCondition.Text.Remove(length - 3, 1);
        //    //    txtCondition.Text = txtCondition.Text.Remove(length - 5, 1);
        //    //}
        //    //txtCondition.Select(txtCondition.Text.Length, 0);

        //    //txtCondition.Focus();
        //}

        ///// <summary>
        ///// Handles the Click event of the And button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnAnd_Click(object sender, EventArgs e)
        //{
        //    int position = txtCondition.SelectionStart;
        //    txtCondition.Text = txtCondition.Text.Insert(position, "AND ");
        //    txtCondition.Select(position + 4, 0);
        //}

        ///// <summary>
        ///// Handles the Click event of the Or button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnOr_Click(object sender, EventArgs e)
        //{
        //    int position = txtCondition.SelectionStart;
        //    txtCondition.Text = txtCondition.Text.Insert(position, "OR ");
        //    txtCondition.Select(position + 3, 0);
        //}

        ///// <summary>
        ///// Handles the Click event of the Yes button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnYes_Click(object sender, EventArgs e)
        //{
        //    int position = txtCondition.SelectionStart;
        //    txtCondition.Text = txtCondition.Text.Insert(position, StringLiterals.EPI_REPRESENTATION_OF_TRUE + " ");
        //    txtCondition.Select(position + StringLiterals.EPI_REPRESENTATION_OF_TRUE.Length + 1, 0);
        //    //txtCondition.AppendText(StringLiterals.EPI_REPRESENTATION_OF_TRUE + " ");
        //    //txtCondition.Focus();
        //}

        ///// <summary>
        ///// Handles the Click event of the No button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnNo_Click(object sender, EventArgs e)
        //{
        //    int position = txtCondition.SelectionStart;
        //    txtCondition.Text = txtCondition.Text.Insert(position, StringLiterals.EPI_REPRESENTATION_OF_FALSE + " ");
        //    txtCondition.Select(position + StringLiterals.EPI_REPRESENTATION_OF_FALSE.Length + 1, 0);
        //}

        ///// <summary>
        ///// Handles the Click event of the Missing button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnMissing_Click(object sender, EventArgs e)
        //{
        //    int position = txtCondition.SelectionStart;
        //    txtCondition.Text = txtCondition.Text.Insert(position, StringLiterals.EPI_REPRESENTATION_OF_MISSING + " ");
        //    txtCondition.Select(position + StringLiterals.EPI_REPRESENTATION_OF_MISSING.Length + 1, 0);
        //}
        #endregion

        /// <summary>
        /// Handles the Selection Change event for variables in the combobox
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void cbxVariables_SelectedIndexChanged(object sender, EventArgs e)
        {
            TextBox txtTargetBox = new TextBox();
            switch (CurrentContextMenuType)
            {
                case CommandContextMenuType.If:
                    txtTargetBox = txtCondition;
                    break;
                case CommandContextMenuType.Then:
                    txtTargetBox = txtThen;
                    break;
                case CommandContextMenuType.Else:
                    txtTargetBox = txtElse;
                    break;
            }

            if (!string.IsNullOrEmpty(cbxVariables.Text))
            {
                string AvailableVarString = (FieldNameNeedsBrackets(cbxVariables.Text) ? Util.InsertInSquareBrackets(cbxVariables.Text) : cbxVariables.Text);
                if (!(txtTargetBox.TextLength.Equals(0)))
                {
                    AvailableVarString = StringLiterals.SPACE + AvailableVarString;
                }
                if (txtTargetBox.SelectionLength > 0)
                {
                    //Don't replace the trailing comma in 
                    //  multi-parameter functions like YEARS or 
                    //  trailing close paren if selected by mistake
                    while (txtTargetBox.SelectionLength > 0 &&
                        (txtTargetBox.SelectedText.EndsWith(StringLiterals.SPACE) ||
                        txtTargetBox.SelectedText.EndsWith(StringLiterals.COMMA) ||
                        txtTargetBox.SelectedText.EndsWith(StringLiterals.PARANTHESES_CLOSE)))
                    {
                        txtTargetBox.SelectionLength -= 1;
                    }
                }
                txtTargetBox.SelectedText = AvailableVarString;
                txtTargetBox.Focus();
                txtTargetBox.Text = ClearSpacesFromParens(txtTargetBox.Text);
                txtTargetBox.Select(txtTargetBox.Text.Length, 0);
            }
        }

        /// <summary>
        /// Method for testing whether or not a field name is a reserved word or 
        /// contains spaces requireing it be wrapped in square brackets
        /// </summary>
        protected virtual bool FieldNameNeedsBrackets(string strTestName)
        {
            Regex regex = new Regex("[\\w\\d]", RegexOptions.IgnoreCase);
            string strResultOfSymbolTest = regex.Replace(strTestName, string.Empty);
            return (strResultOfSymbolTest.Length > 0) | AppData.Instance.IsReservedWord(strTestName) | !(strTestName.Equals(Util.Squeeze(strTestName)));
        }


        /// <summary>
        /// Handles the MouseDown event of the Then button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnThen_MouseDown(object sender, MouseEventArgs e)
        {
            CurrentContextMenuType = CommandContextMenuType.Then;
            BuildCommandContextMenu().Show((Control)sender, e.Location);
        }

        /// <summary>
        /// Handles the Mouse Down event of the Else button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnElse_MouseDown(object sender, MouseEventArgs e)
        {
            CurrentContextMenuType = CommandContextMenuType.Else;
            BuildCommandContextMenu().Show((Control)sender, e.Location);
        }

        /// <summary>
        /// Handles the MouseDown event of the If Function button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnFunction_MouseDown(object sender, MouseEventArgs e)
        {
            CurrentContextMenuType = CommandContextMenuType.If;
            BuildFunctionContextMenu().Show((Control)sender, e.Location);
        }

        /// <summary>
        /// Handles the MouseDown event of the Then Function button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnFunctionThen_MouseDown(object sender, MouseEventArgs e)
        {
            CurrentContextMenuType = CommandContextMenuType.Then;
            BuildFunctionContextMenu().Show((Control)sender, e.Location);
        }

        /// <summary>
        /// Handles the MouseDown event of the Else Function button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnFunctionElse_MouseDown(object sender, MouseEventArgs e)
        {
            CurrentContextMenuType = CommandContextMenuType.Else;
            BuildFunctionContextMenu().Show((Control)sender, e.Location);
        }

        private void txtCondition_MouseDown(object sender, MouseEventArgs e)
        {
            CurrentContextMenuType = CommandContextMenuType.If;
        }

        private void txtThen_MouseDown(object sender, MouseEventArgs e)
        {
            CurrentContextMenuType = CommandContextMenuType.Then;
        }

        private void txtElse_MouseDown(object sender, MouseEventArgs e)
        {
            CurrentContextMenuType = CommandContextMenuType.Else;
        }

        /// <summary>
        /// Handles the Click event of the  Functions and Operators context menu
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void FXClickHandler(object sender, System.EventArgs e)
        {
            ToolStripMenuItem FXItem = (ToolStripMenuItem)sender;
            TextBox txtTargetBox = new TextBox();
            switch (CurrentContextMenuType)
            {
                case CommandContextMenuType.If:
                    txtTargetBox = txtCondition;
                    break;
                case CommandContextMenuType.Then:
                    txtTargetBox = txtThen;
                    break;
                case CommandContextMenuType.Else:
                    txtTargetBox = txtElse;
                    break;
            }

            if (txtTargetBox.SelectionLength > 0)
            {
                //Don't replace the trailing comma in 
                //  multi-parameter functions like YEARS or 
                //  trailing close paren if selected by mistake
                while (txtTargetBox.SelectionLength > 0 &&
                    (txtTargetBox.SelectedText.EndsWith(StringLiterals.SPACE) ||
                    txtTargetBox.SelectedText.EndsWith(StringLiterals.COMMA) ||
                    txtTargetBox.SelectedText.EndsWith(StringLiterals.PARANTHESES_CLOSE)))
                {
                    txtTargetBox.SelectionLength -= 1;
                }
            }
            txtTargetBox.SelectedText = StringLiterals.SPACE + FXItem.ToolTipText + StringLiterals.SPACE;
            txtTargetBox.Text = ClearSpacesFromParens(txtTargetBox.Text);
            txtTargetBox.Select(txtTargetBox.Text.Length, 0);
            txtTargetBox.Focus();
        }

        /// <summary>
        /// Handles the Click event of the Help menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuHelp_Click(object sender, EventArgs e)
        {
            MsgBox.ShowInformation("This functionality will be available in a later build."); // TODO: Hard coded string
        }

        /// <summary>
        /// Handles the Click event of the Dialog menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuDialog_Click(object sender, EventArgs e)
        {
            //DesignStatement(new DialogConfigDialog(MainForm));

            DesignStatement(new DialogConfigDialog(MainForm,this.view.GetProject()));

        }

        /// <summary>
        /// Handles the Click event of the Disable menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuDisable_Click(object sender, EventArgs e)
        {
            DesignStatement(new DisableDialog(MainForm));
        }

        /// <summary>
        /// Handles the Click event of the Enable menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuEnable_Click(object sender, EventArgs e)
        {
            DesignStatement(new EnableDialog(MainForm));
        }

        /// <summary>
        /// Handles the Click event of the Hightlight menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuHighlight_Click(object sender, EventArgs e)
        {
            DesignStatement(new HighlightDialog(MainForm));
        }

        /// <summary>
        /// Handles the Click event of the Unhightlight menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuUnhighlight_Click(object sender, EventArgs e)
        {
            DesignStatement(new UnhighlightDialog(MainForm));
        }


        /// <summary>
        /// Handles the Click event of the Assign menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event arguments</param>
        void mnuAssign_Click(object sender, EventArgs e)
        {
            DesignStatement(new VariableAssignmentDialog(MainForm));            
        }

        /// <summary>
        /// Handles the Click event of the Execute menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event arguments</param>
        void mnuExecute_Click(object sender, EventArgs e)
        {
            DesignStatement(new ExecuteDialog(MainForm));
        }

        /// <summary>
        /// Handles the Click event of the AutoSearch menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event arguments</param>
        void mnuAutosearch_Click(object sender, EventArgs e)
        {
            DesignStatement(new AutoSearchDialog(MainForm));
        }

        /// <summary>
        /// Handles the Click event of the GoTo menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event arguments</param>
        void mnuGoto_Click(object sender, EventArgs e)
        {
            DesignStatement(new GoToDialog(MainForm));
        }

        ///// <summary>
        ///// Handles the Click event of the Call menu item
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //void mnuCall_Click(object sender, EventArgs e)
        //{
        //    DesignStatement(new CallDialog(MainForm));
        //}

        /// <summary>
        /// Handles the Click event of the Clear menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuClear_Click(object sender, EventArgs e)
        {
            DesignStatement(new ClearDialog(MainForm));
        }

        /// <summary>
        /// Handles the Click event of the Unhide menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuUnhide_Click(object sender, EventArgs e)
        {
            DesignStatement(new UnhideDialog(MainForm));
        }

        /// <summary>
        /// Handles the Click event of the Hide menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuHide_Click(object sender, EventArgs e)
        {
            DesignStatement(new HideDialog(MainForm));
        }

        /// <summary>
        /// Handles the Click event of the Geocode menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuGeocode_Click(object sender, EventArgs e)
        {
            DesignStatement(new GeocodeDialog(MainForm));
        }

        /// <summary>
        /// Handles the Click event of the NewRecord menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuNewRecord_Click(object sender, EventArgs e)
        {
            DesignStatement(new NewRecordDialog(MainForm));
        }

        /// <summary>
        /// Handles the Click event of the If menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuIf_Click(object sender, EventArgs e)
        {
            DesignStatement(new IfClauseDialog(MainForm));           
        }

        /// <summary>
        /// Handles the Click event of the Set-Required menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuSetRequired_Click(object sender, EventArgs e)
        {
            DesignStatement(new SetRequiredDialog(MainForm));
        }

        /// <summary>
        /// Handles the Click event of the Set-Required menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuSetNotRequired_Click(object sender, EventArgs e)
        {
            DesignStatement(new SetNotRequiredDialog(MainForm));
        }
        ///// <summary>
        ///// Handles the Click event of the Quit menu item
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //void mnuQuit_Click(object sender, EventArgs e)
        //{
        //    DesignStatement(new QuitDialog(MainForm));
        //}

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/user-guide/command-reference/check-commands-if-then-else.html");
        }

        #endregion  //Private Event Handlers

        /// <summary>
        /// Builds a context menu for commands
        /// </summary>
        /// <returns>A commands context menu</returns>
        private ContextMenuStrip BuildCommandContextMenu()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem mnuAssign = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_ASSIGN);
            mnuAssign.Click += new EventHandler(mnuAssign_Click);
            ToolStripMenuItem mnuAutosearch = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_AUTOSEARCH);
            mnuAutosearch.Click += new EventHandler(mnuAutosearch_Click);
            //ToolStripMenuItem mnuCall = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_CALL);
            //mnuCall.Click += new EventHandler(mnuCall_Click);
            ToolStripMenuItem mnuClear = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_CLEAR);
            mnuClear.Click += new EventHandler(mnuClear_Click);
            ToolStripMenuItem mnuDialog = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_DIALOG);
            mnuDialog.Click += new EventHandler(mnuDialog_Click);
            ToolStripMenuItem mnuDisable = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_DISABLE);
            mnuDisable.Click += new EventHandler(mnuDisable_Click);
            ToolStripMenuItem mnuEnable = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_ENABLE);
            mnuEnable.Click += new EventHandler(mnuEnable_Click);
            ToolStripMenuItem mnuExecute = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_EXECUTE);
            mnuExecute.Click += new EventHandler(mnuExecute_Click);
            ToolStripMenuItem mnuGeocode = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_GEOCODE);
            mnuGeocode.Click += new EventHandler(mnuGeocode_Click);
            ToolStripMenuItem mnuGoto = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_GOTO);
            mnuGoto.Click += new EventHandler(mnuGoto_Click);
            ToolStripMenuItem mnuHelp = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_HELP);
            mnuHelp.Enabled = false;
            mnuHelp.Click += new EventHandler(mnuHelp_Click);
            ToolStripMenuItem mnuHide = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_HIDE);
            mnuHide.Click += new EventHandler(mnuHide_Click);
            ToolStripMenuItem mnuUnhide = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_UNHIDE);
            mnuUnhide.Click += new EventHandler(mnuUnhide_Click);
            ToolStripMenuItem mnuHighlight = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_HIGHLIGHT);
            mnuHighlight.Click += new EventHandler(mnuHighlight_Click);
            ToolStripMenuItem mnuUnhighlight = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_UNHIGHLIGHT);
            mnuUnhighlight.Click += new EventHandler(mnuUnhighlight_Click);
            ToolStripMenuItem mnuNewRecord = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_NEWRECORD);
            mnuNewRecord.Click += new EventHandler(mnuNewRecord_Click);
            ToolStripMenuItem mnuIf = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_IF);
            mnuIf.Click += new EventHandler(mnuIf_Click);
            ToolStripMenuItem mnuSetRequired = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_SET_REQUIRED);
            mnuSetRequired.Click += new EventHandler(mnuSetRequired_Click);
            ToolStripMenuItem mnuSetNotRequired = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_SET_NOT_REQUIRED);
            mnuSetNotRequired.Click += new EventHandler(mnuSetNotRequired_Click);
            contextMenu.Items.AddRange(new ToolStripMenuItem[] { mnuAssign, mnuAutosearch, mnuClear, mnuDialog, mnuDisable, mnuEnable, mnuExecute, mnuGeocode, mnuGoto, mnuHelp, mnuHide, mnuUnhide, mnuHighlight, mnuUnhighlight, mnuNewRecord, mnuIf, mnuSetRequired, mnuSetNotRequired });

            return contextMenu;
        }

        /// <summary>
        /// Builds a context menu for functions and operators
        /// </summary>
        /// <returns>A context menu for functions and operators</returns>
        private ContextMenuStrip BuildFunctionContextMenu()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            //==================================================================
            ToolStripMenuItem mnuOperators = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMCOPS);
            ToolStripMenuItem mnuExponent = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMCOPS_EXP);
            mnuExponent.ToolTipText = StringLiterals.CARET;
            mnuExponent.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuMOD = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMCOPS_MOD);
            mnuMOD.ToolTipText = CommandNames.MOD;
            mnuMOD.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuGT = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMCOPS_GT);
            mnuGT.ToolTipText = StringLiterals.GREATER_THAN;
            mnuGT.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuGTE = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMCOPS_GTE);
            mnuGTE.ToolTipText = StringLiterals.GREATER_THAN_OR_EQUAL;
            mnuGTE.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuEqual = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMCOPS_EQUAL);
            mnuEqual.ToolTipText = StringLiterals.EQUAL;
            mnuEqual.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuNE = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMCOPS_LTGT);
            mnuNE.ToolTipText = StringLiterals.LESS_THAN_OR_GREATER_THAN;
            mnuNE.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuLT = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMCOPS_LT);
            mnuLT.ToolTipText = StringLiterals.LESS_THAN;
            mnuLT.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuLTE = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMCOPS_LTE);
            mnuLTE.ToolTipText = StringLiterals.LESS_THAN_OR_EQUAL;
            mnuLTE.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuLIKE = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMCOPS_LIKE);
            mnuLIKE.ToolTipText = CommandNames.LIKE;
            mnuLIKE.Click += new EventHandler(FXClickHandler);
            //======================================================================
            ToolStripMenuItem mnuBools = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_LGOPS);
            ToolStripMenuItem mnuAND = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_LGOPS_AND);
            mnuAND.ToolTipText = CommandNames.AND;
            mnuAND.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuOR = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_LGOPS_OR);
            mnuOR.ToolTipText = CommandNames.OR;
            mnuOR.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuXOR = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_LGOPS_XOR);
            mnuXOR.ToolTipText = CommandNames.XOR;
            mnuXOR.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuNOT = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_LGOPS_NOT);
            mnuNOT.ToolTipText = CommandNames.NOT;
            mnuNOT.Click += new EventHandler(FXClickHandler);
            //=====================================================================
            ToolStripMenuItem mnuNums = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX);
            ToolStripMenuItem mnuEXP = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX_EXP);
            mnuEXP.ToolTipText = CommandNames.EXP + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuEXP.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuSIN = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX_SIN);
            mnuSIN.ToolTipText = CommandNames.SIN + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuSIN.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuCOS = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX_COS);
            mnuCOS.ToolTipText = CommandNames.COS + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuCOS.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuTAN = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX_TAN);
            mnuTAN.ToolTipText = CommandNames.TAN + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuTAN.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuLOG = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX_LOG);
            mnuLOG.ToolTipText = CommandNames.LOG + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuLOG.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuLN = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX_LN);
            mnuLN.ToolTipText = CommandNames.LN + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuLN.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuABS = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX_ABS);
            mnuABS.ToolTipText = CommandNames.ABS + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuABS.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuRND = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX_RND);
            mnuRND.ToolTipText = CommandNames.RND + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuRND.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuSTEP = new ToolStripMenuItem(CommandNames.STEP);
            mnuSTEP.ToolTipText = CommandNames.STEP + StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.SPACE + StringLiterals.PARANTHESES_CLOSE;
            mnuSTEP.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuTRUNC = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX_TRUNC);
            mnuTRUNC.ToolTipText = CommandNames.TRUNC + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuTRUNC.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuROUND = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX_ROUND);
            mnuROUND.ToolTipText = CommandNames.ROUND + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuROUND.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuNTD = new ToolStripMenuItem(CommandNames.NUMTODATE);
            mnuNTD.ToolTipText = CommandNames.NUMTODATE + SharedStrings.CNTXT_FXN_TMPLT_DATEPARTS;
            mnuNTD.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuNTT = new ToolStripMenuItem(CommandNames.NUMTOTIME);
            mnuNTT.ToolTipText = CommandNames.NUMTOTIME + SharedStrings.CNTXT_FXN_TMPLT_TIMEPARTS;
            mnuNTT.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuPFROMZ = new ToolStripMenuItem(CommandNames.PFROMZ);
            mnuPFROMZ.ToolTipText = CommandNames.PFROMZ + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuPFROMZ.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuZSCORE = new ToolStripMenuItem(CommandNames.ZSCORE);
            mnuZSCORE.ToolTipText = CommandNames.ZSCORE + SharedStrings.CNTXT_FXN_TMPLT_ZSCORE;
            mnuZSCORE.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuRECORDCOUNT = new ToolStripMenuItem(CommandNames.RECORDCOUNT);
            mnuRECORDCOUNT.ToolTipText = CommandNames.RECORDCOUNT;
            mnuRECORDCOUNT.Click += new EventHandler(FXClickHandler);

            //===================================================================
            ToolStripMenuItem mnuDates = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_DATFX);
            ToolStripMenuItem mnuYRS = new ToolStripMenuItem(CommandNames.YEARS);
            mnuYRS.ToolTipText = CommandNames.YEARS + SharedStrings.CNTXT_FXN_DATFX_TMPLT2;
            mnuYRS.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuMOS = new ToolStripMenuItem(CommandNames.MONTHS);
            mnuMOS.ToolTipText = CommandNames.MONTHS + SharedStrings.CNTXT_FXN_DATFX_TMPLT2;
            mnuMOS.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuDYS = new ToolStripMenuItem(CommandNames.DAYS);
            mnuDYS.ToolTipText = CommandNames.DAYS + SharedStrings.CNTXT_FXN_DATFX_TMPLT2;
            mnuDYS.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuYR = new ToolStripMenuItem(CommandNames.YEAR);
            mnuYR.ToolTipText = CommandNames.YEAR + SharedStrings.CNTXT_FXN_DATFX_TMPLT1;
            mnuYR.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuMO = new ToolStripMenuItem(CommandNames.MONTH);
            mnuMO.ToolTipText = CommandNames.MONTH + SharedStrings.CNTXT_FXN_DATFX_TMPLT1;
            mnuMO.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuDY = new ToolStripMenuItem(CommandNames.DAY);
            mnuDY.ToolTipText = CommandNames.DAY + SharedStrings.CNTXT_FXN_DATFX_TMPLT1;
            mnuDY.Click += new EventHandler(FXClickHandler);

            //===================================================================
            ToolStripMenuItem mnuSys = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_SYSFXN);
            ToolStripMenuItem mnuCurrUser = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_SYSFXN_CURRUSER);
            mnuCurrUser.ToolTipText = CommandNames.CURRENTUSER + StringLiterals.PARANTHESES_OPEN +
                StringLiterals.SPACE + StringLiterals.PARANTHESES_CLOSE;
            mnuCurrUser.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuEXISTS = new ToolStripMenuItem(CommandNames.EXISTS);
            mnuEXISTS.ToolTipText = CommandNames.EXISTS + SharedStrings.CNTXT_FXN_TMPLT_FILENAME;
            mnuEXISTS.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuFILEDATE = new ToolStripMenuItem(CommandNames.FILEDATE);
            mnuFILEDATE.ToolTipText = CommandNames.FILEDATE + SharedStrings.CNTXT_FXN_TMPLT_FILENAME;
            mnuFILEDATE.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuSYSTEMDATE = new ToolStripMenuItem(CommandNames.SYSTEMDATE);
            mnuSYSTEMDATE.ToolTipText = CommandNames.SYSTEMDATE;
            mnuSYSTEMDATE.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuSYSTEMTIME = new ToolStripMenuItem(CommandNames.SYSTEMTIME);
            mnuSYSTEMTIME.ToolTipText = CommandNames.SYSTEMTIME;
            mnuSYSTEMTIME.Click += new EventHandler(FXClickHandler);

            //ToolStripMenuItem mnuENVIRON = new ToolStripMenuItem("ENVIRON");
            //mnuENVIRON.ToolTipText = "ENVIRON( <name_of_env_variable> )";
            //mnuENVIRON.Click += new EventHandler(FXClickHandler);

            //===================================================================
            ToolStripMenuItem mnuTimes = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_TIMEFXN);
            ToolStripMenuItem mnuHOURS = new ToolStripMenuItem(CommandNames.HOURS);
            mnuHOURS.ToolTipText = CommandNames.HOURS + SharedStrings.CNTXT_FXN_DATFX_TMPLT2;
            mnuHOURS.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuMINUTES = new ToolStripMenuItem("MINUTES");
            mnuMINUTES.ToolTipText = CommandNames.MINUTES + SharedStrings.CNTXT_FXN_DATFX_TMPLT2;
            mnuMINUTES.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuSECONDS = new ToolStripMenuItem("SECONDS");
            mnuSECONDS.ToolTipText = CommandNames.SECONDS + SharedStrings.CNTXT_FXN_DATFX_TMPLT2;
            mnuSECONDS.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuHOUR = new ToolStripMenuItem("HOUR");
            mnuHOUR.ToolTipText = CommandNames.HOUR + SharedStrings.CNTXT_FXN_DATFX_TMPLT1;
            mnuHOUR.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuMINUTE = new ToolStripMenuItem("MINUTE");
            mnuMINUTE.ToolTipText = CommandNames.MINUTE + SharedStrings.CNTXT_FXN_DATFX_TMPLT1;
            mnuMINUTE.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuSECOND = new ToolStripMenuItem("SECOND");
            mnuSECOND.ToolTipText = CommandNames.SECOND + SharedStrings.CNTXT_FXN_DATFX_TMPLT1;
            mnuSECOND.Click += new EventHandler(FXClickHandler);

            //==================================================================
            ToolStripMenuItem mnuTxts = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_TEXTFXN);
            ToolStripMenuItem mnuTXTTONUM = new ToolStripMenuItem(CommandNames.TXTTONUM);
            mnuTXTTONUM.ToolTipText = CommandNames.TXTTONUM + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuTXTTONUM.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuTXTTODATE = new ToolStripMenuItem(CommandNames.TXTTODATE);
            mnuTXTTODATE.ToolTipText = CommandNames.TXTTODATE + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuTXTTODATE.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuSUBSTRING = new ToolStripMenuItem(CommandNames.SUBSTRING);
            mnuSUBSTRING.ToolTipText = CommandNames.SUBSTRING + SharedStrings.CNTXT_FXN_TMPLT_SUBSTRING;
            mnuSUBSTRING.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuSTRLEN = new ToolStripMenuItem(CommandNames.STRLEN);
            mnuSTRLEN.ToolTipText = CommandNames.STRLEN + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuSTRLEN.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuUPPERCASE = new ToolStripMenuItem(CommandNames.UPPERCASE);
            mnuUPPERCASE.ToolTipText = CommandNames.UPPERCASE + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuUPPERCASE.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuFINDTEXT = new ToolStripMenuItem(CommandNames.FINDTEXT);
            mnuFINDTEXT.ToolTipText = CommandNames.FINDTEXT + SharedStrings.CNTXT_FXN_TMPLT_FINDTEXT;
            mnuFINDTEXT.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuFORMAT = new ToolStripMenuItem(CommandNames.FORMAT);
            mnuFORMAT.ToolTipText = CommandNames.FORMAT + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuFORMAT.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuLINEBREAK = new ToolStripMenuItem(CommandNames.LINEBREAK);
            mnuLINEBREAK.ToolTipText = CommandNames.LINEBREAK + StringLiterals.PARANTHESES_OPEN +
                StringLiterals.SPACE + StringLiterals.PARANTHESES_CLOSE;
            mnuLINEBREAK.Click += new EventHandler(FXClickHandler);
            //=======================================================================
            ToolStripMenuItem mnuDATEFORMATS = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_DATES);
            ToolStripMenuItem mnuGeneralDate = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_DATES_GEN);
            mnuGeneralDate.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.GENERAL_DATE_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuGeneralDate.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuLongDate = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_DATES_LONGDATE);
            mnuLongDate.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.LONG_DATE_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuLongDate.Click += new EventHandler(FXClickHandler);

            //ToolStripMenuItem mnuMediumDate = new ToolStripMenuItem("Medium Date");
            //mnuMediumDate.ToolTipText = "FORMAT( <variable>, \"Medium Date\")";
            //mnuMediumDate.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuShortDate = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_DATES_SHORTDATE);
            mnuShortDate.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.SHORT_DATE_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuShortDate.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuLongTime = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_DATES_LONGTIME);
            mnuLongTime.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.LONG_TIME_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuLongTime.Click += new EventHandler(FXClickHandler);

            //ToolStripMenuItem mnuMediumTime = new ToolStripMenuItem("Medium Time");
            //mnuMediumTime.ToolTipText = "FORMAT( <variable>, \"Medium Time\")";
            //mnuMediumTime.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuShortTime = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_DATES_SHORTTIME);
            mnuShortTime.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.SHORT_TIME_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuShortTime.Click += new EventHandler(FXClickHandler);

            //===========================================================================
            ToolStripMenuItem mnuNumberFormats = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_NUM);
            ToolStripMenuItem mnuGeneralNumber = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_NUM_GEN);
            mnuGeneralNumber.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.GENERAL_NUMBER_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuGeneralNumber.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuCurrency = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_NUM_CURRENCY);
            mnuCurrency.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.CURRENCY_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuCurrency.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuFixed = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_NUM_FIXED);
            mnuFixed.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.FIXED_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuFixed.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuStandard = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_NUM_STANDARD);
            mnuStandard.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.STANDARD_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuStandard.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuPercent = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_NUM_PERCENT);
            mnuPercent.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.PERCENT_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuPercent.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuScientific = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_NUM_SCIENTIFIC);
            mnuScientific.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.SCIENTIFIC_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuScientific.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuYN = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_NUM_YN);
            mnuYN.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.YESNO_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuYN.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuTF = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_NUM_TF);
            mnuTF.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.TRUE_FALSE_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuTF.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuOnOff = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_NUM_ONOFF);
            mnuOnOff.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.ON_OFF_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuOnOff.Click += new EventHandler(FXClickHandler);

            contextMenu.Items.Add(mnuOperators);
            contextMenu.Items.Add(mnuBools);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.AddRange(new ToolStripMenuItem[] { mnuNums, mnuDates, mnuSys, mnuTimes, mnuTxts });

            mnuOperators.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuExponent, mnuMOD, mnuGT, mnuGTE, mnuEqual, mnuNE, mnuLT, mnuLTE, mnuLIKE });
            mnuBools.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuAND, mnuOR, mnuXOR, mnuNOT });
            mnuNums.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuABS, mnuEXP, mnuLN, mnuLOG, mnuNTD, mnuNTT, mnuRECORDCOUNT, mnuRND, mnuROUND, mnuSTEP, mnuSIN, mnuCOS, mnuTAN, mnuTRUNC, mnuPFROMZ, mnuZSCORE });
            mnuDates.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuYRS, mnuMOS, mnuDYS, mnuYR, mnuMO, mnuDY });
            mnuSys.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuCurrUser, mnuEXISTS, mnuFILEDATE, mnuSYSTEMDATE, mnuSYSTEMTIME });
            mnuTimes.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuHOURS, mnuMINUTES, mnuSECONDS, mnuHOUR, mnuMINUTE, mnuSECOND });
            mnuTxts.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuFINDTEXT, mnuFORMAT, mnuLINEBREAK, mnuSTRLEN, mnuSUBSTRING, mnuTXTTONUM, mnuTXTTODATE, mnuUPPERCASE });

            mnuFORMAT.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuDATEFORMATS, mnuNumberFormats });
            mnuDATEFORMATS.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuGeneralDate, mnuLongDate, mnuShortDate, mnuLongTime, mnuShortTime });
            mnuNumberFormats.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuGeneralNumber, mnuCurrency, mnuFixed, mnuStandard, mnuPercent, mnuScientific, mnuYN, mnuTF, mnuOnOff });
            return contextMenu;
        }


        /// <summary>
        /// Common event handler for build expression buttons
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void ClickHandler(object sender, System.EventArgs e)
        {
            Button btn = (Button)sender;
            string strInsert = StringLiterals.SPACE;
            TextBox txtTargetBox = new TextBox();
            switch (CurrentContextMenuType)
            {
                case CommandContextMenuType.If:
                    txtTargetBox = txtCondition;
                    break;
                case CommandContextMenuType.Then:
                    txtTargetBox = txtThen;
                    break;
                case CommandContextMenuType.Else:
                    txtTargetBox = txtElse;
                    break;
            }
            if (txtTargetBox.SelectionLength > 0)
            {
                //Don't replace the trailing comma in 
                //  multi-parameter functions like YEARS or 
                //  trailing close paren if selected by mistake
                while (txtTargetBox.SelectionLength > 0 &&
                    (txtTargetBox.SelectedText.EndsWith(StringLiterals.SPACE) ||
                    txtTargetBox.SelectedText.EndsWith(StringLiterals.COMMA) ||
                    txtTargetBox.SelectedText.EndsWith(StringLiterals.PARANTHESES_CLOSE)))
                {
                    txtTargetBox.SelectionLength -= 1;
                }
            }

            if ((string)btn.Tag == null)
            {
                if ((string)btn.Text == StringLiterals.DOUBLEQUOTES)
                {
                    //if the button is for a double quote, only add a leading space if there is 
                    //  an even number of quotes in the expression so far.

                    strInsert = ((QuoteCountIsEven(txtTargetBox.Text, txtTargetBox.SelectionStart)) ? StringLiterals.SPACE : string.Empty);
                }
                txtTargetBox.SelectedText = strInsert + (string)btn.Text;
            }
            else
            {
                if ((string)btn.Tag == StringLiterals.DOUBLEQUOTES)
                {
                    //if the button is for a double quote, only add a leading space if there is 
                    //  an odd number of quotes in the expression so far.
                    strInsert = ((QuoteCountIsEven(txtTargetBox.Text, txtTargetBox.SelectionStart)) ? StringLiterals.SPACE : string.Empty);
                }
                txtTargetBox.SelectedText = strInsert + (string)btn.Tag;
            }
            //Remove spaces within (+) (.) and (-) as any spaces within the parentheses
            //  will cause an error.
            txtTargetBox.Text = ClearSpacesFromParens(txtTargetBox.Text);
            txtTargetBox.Select(txtTargetBox.Text.Length, 0);
            txtTargetBox.Focus();
        }

        /// <summary>
        /// Removes any embedded spaces within parentheses that could cause errors with
        ///  yes (+), no (.), and missing (.) and trims leading/trailing spaces.
        ///  also from less than or equal, greater than or equal, and not equal;  
        /// </summary>
        /// <param name="input">Input string to search</param>
        private string ClearSpacesFromParens(string input)
        {
            string strCheckedA;
            string strCheckedB;
            strCheckedA = Regex.Replace(input, @"\s+\(\s+\+\s\)", " (+)"); //removes spaces within ( + ) to make (+) yes
            strCheckedB = Regex.Replace(strCheckedA, @"\s+\(\s+\-\s+\)", " (-)");  //removes spaces in (-) no
            strCheckedA = Regex.Replace(strCheckedB, @"\s+\(\s+\.\s+\)", " (.)");  //removes spaces in (.) missing
            strCheckedB = Regex.Replace(strCheckedA, @"\s+\<\s+\=", " <=");  //removes spaces in <= less than or equal
            strCheckedA = Regex.Replace(strCheckedB, @"\s+\>\s+\=", " >=");  // >  = becomes >=  greater than or equal
            strCheckedB = Regex.Replace(strCheckedA, @"\s+\<\s+\>", " <>");  //  <  >  becomes <> not equal
            return strCheckedB.Trim();
        }

        /// <summary>
        /// Returns TRUE if the count of doublequotes in a string is even.
        /// </summary>
        /// <param name="input">String input to check</param>
        /// <param name="stopPos">String position to end checking (start of selection to be replaced).</param>
        /// <returns> Returns TRUE if the count of doublequotes in the input string is even.</returns>
        private static bool QuoteCountIsEven(string input, int stopPos)
        {
            int result = 0;
            char q = '\"';
            for (int i = 0; i < stopPos; i++)
            {
                if (q == input[i]) result++;
            }
            return (0 == result % 2);
        }

        /// <summary>
        /// Handles text for the Then menu item
        /// </summary>
        /// <param name="dialog">A check code dialog</param>
        private void DesignStatement(CheckCodeDesignDialog dialog)
        {
            dialog.View = view;
            DialogResult result = ((Form)dialog).ShowDialog();
            if (result == DialogResult.OK)
            {
                if (CurrentContextMenuType == CommandContextMenuType.Then)
                {
                    if (txtThen.Lines.Length > 0)
                    {
                        txtThen.AppendText(Environment.NewLine);
                    }
                    txtThen.AppendText(dialog.Output);
                }
                else
                {
                    if (txtElse.Lines.Length > 0)
                    {
                        txtElse.AppendText(Environment.NewLine);
                    }
                    txtElse.AppendText(dialog.Output);
                }
                ((Form)dialog).Close();
            }
        }

        #region Private Enumerations        
        
        private enum CommandContextMenuType
        {
            If,
            Then,
            Else
        }

        #endregion  //Private Enumerations


    }
}

