using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Epi.Windows.Dialogs;

namespace Epi.Windows.MakeView.Dialogs
{
	/// <summary>
	/// ***** This is a wireframe and currently contains no functionality *****
	/// </summary>
    public partial class ImportCheckCodeDialog : DialogBase
	{

		/// <summary>
		/// Constructor for the class
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public ImportCheckCodeDialog()
		{
			InitializeComponent();	
		}

        /// <summary>
        /// Constructor of the Import Check Code dialog
        /// </summary>
        /// <param name="frm">The main form</param>
		public ImportCheckCodeDialog(MainForm frm): base(frm)
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();
        }
		
		/// <summary>
		/// Cancel button closes this dialog 
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}

