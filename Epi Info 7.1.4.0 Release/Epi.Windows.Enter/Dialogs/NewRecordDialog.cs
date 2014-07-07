#region Namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Epi;
using Epi.Windows;

using Epi.Windows.Dialogs;

#endregion

namespace Epi.Windows.Enter.Dialogs
{
	/// <summary>
	/// Dialog for New Record
	/// </summary>
    public partial class NewRecord : DialogBase
	{
		#region Constructor

		/// <summary>
		/// Default Constructor for NewRecord dialog
		/// </summary>
		public NewRecord()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for New Record dialog
        /// </summary>
        /// <param name="mainForm">Enter module's main form</param>
        public NewRecord(EnterMainForm mainForm)
            : base(mainForm)
        {
            InitializeComponent();
        }

		#endregion

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

