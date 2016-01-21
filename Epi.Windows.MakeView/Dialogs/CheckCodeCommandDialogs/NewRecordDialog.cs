#region  Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

#endregion  //Namespaces

namespace Epi.Windows.MakeView.Dialogs.CheckCodeCommandDialogs
{
    /// <summary>
    /// The New Record Dialog
    /// </summary>
    public partial class NewRecordDialog : Epi.Windows.MakeView.Dialogs.CheckCodeCommandDialogs.CheckCodeDesignDialog
    {
        #region Constructors

        /// <summary>
		/// Constructor for the class
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public NewRecordDialog()
		{
			InitializeComponent();
		}
        
        /// <summary>
        /// Constructor for the class
        /// </summary>
        public NewRecordDialog(MainForm frm) : base(frm)
        {
            InitializeComponent();
        }

        #endregion  //Constructors

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
        /// Saves the current record and begins a new record
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnOk_Click(object sender, System.EventArgs e)
        {
            Output = CommandNames.NEWRECORD;            
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/check-commands-newrecord.html");
        }

        #endregion  //Private Event Handlers
    }
}

