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
    public partial class HelpConfigDialog : CheckCodeDesignDialog
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public HelpConfigDialog()
        {
            InitializeComponent();
        }
		/// <summary>
		/// Constructor for the class
		/// </summary>
        public HelpConfigDialog(MainForm frm) : base(frm)
		{
			// This call is required by the Windows Form Designer.
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
        /// Handles the Click event of the OK button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            Output = CommandNames.HELP + StringLiterals.SPACE + TextBox1.Text + StringLiterals.SPACE + ComboBox1.Text;            
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        #endregion  //Private Event Handlers
    }
}

