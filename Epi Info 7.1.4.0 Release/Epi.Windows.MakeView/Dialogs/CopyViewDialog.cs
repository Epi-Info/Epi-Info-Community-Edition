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
    public partial class CopyViewDialog : DialogBase
	{
		/// <summary>
		/// Default Constructor - Design mode only
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public CopyViewDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="frm">The main form</param>
        public CopyViewDialog(MainForm frm) : base(frm)
        {
            InitializeComponent();
        }

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}