#region Namespaces
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Epi.Windows.Dialogs;

#endregion  //Namespaces

namespace Epi.Windows.MakeView.Dialogs.CheckCodeCommandDialogs
{
	/// <summary>
	/// ***** This is a wireframe and currently contains no functionality *****
	/// </summary>
    public partial class ExecuteDialog : CheckCodeDesignDialog
    {
        #region Constructors        
        
        /// <summary>
		/// Constructor for the class
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public ExecuteDialog()
		{
			InitializeComponent();
            cbxWaitforExecution.Checked = true;
		}
        /// <summary>
        /// Constructor for the class
        /// </summary>
        public ExecuteDialog(MainForm frm) : base(frm)
        {
            InitializeComponent();
            cbxWaitforExecution.Checked = true;
        }

        #endregion  //Constructors

        #region Private Event Handlers

        /// <summary>
		/// Handles the Click event of the Cancel button
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
        
        /// <summary>
        /// Handles the Click event of the Browse button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "Executables (*.exe)|*.exe|All Files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
//                txtFileName.Text = "'" + dialog.FileName + "'";
                txtFileName.Text = dialog.FileName;
            }
        }

        /// <summary>
        /// Handles the Click event of the OK button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnOk_Click(object sender, System.EventArgs e)
        {
            Output = CommandNames.EXECUTE + StringLiterals.SPACE;

            if (cbxWaitforExecution.Checked)
            {
                Output += CommandNames.WAITFOREXIT + StringLiterals.SPACE;
            }
            else
            {
                Output += CommandNames.NOWAITFOREXIT + StringLiterals.SPACE;
            }
            
            Output += StringLiterals.DOUBLEQUOTES + txtFileName.Text + StringLiterals.DOUBLEQUOTES;            

            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        /// <summary>
        /// Handles the Text Changed event of the file name text box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtFileName_TextChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = !string.IsNullOrEmpty(txtFileName.Text);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtFileName.Text = String.Empty;
            cbxWaitforExecution.Checked = true;
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/check-commands-execute.html");
        }

        #endregion  //Private Event Handlers
    }
}

