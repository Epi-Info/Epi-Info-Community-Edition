#region Namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Epi.Windows.Dialogs;

#endregion

namespace Epi.Windows.Enter.Dialogs
{
	/// <summary>
	/// Dialog for Yes No
	/// </summary>
    public partial class YesNo : DialogBase
	{
		#region Private Class Members
		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor for YesNo dialog
		/// </summary>
		public YesNo()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for YesNo dialog
        /// </summary>
        /// <param name="mainForm">Enter module's main form</param>
        public YesNo(EnterMainForm mainForm)
            : base(mainForm)
        {
            InitializeComponent();
        }

		#endregion  //Constructors

		#region Event Handlers

		/// <summary>
		/// Cancel button closes this dialog 
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		#endregion

	}
}

