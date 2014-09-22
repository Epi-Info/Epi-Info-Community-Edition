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
    public partial class CreateEditDialog : DialogBase
	{
		/// <summary>
		/// Constructor for the class
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public CreateEditDialog()
		{
			InitializeComponent();
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="frm">The main form</param>
        public CreateEditDialog(MainForm frm) : base(frm)
        {
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

