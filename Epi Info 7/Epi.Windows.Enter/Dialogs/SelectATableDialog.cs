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
	/// Dialog for Selecting a table
	/// </summary>
    public partial class SelectATable : DialogBase
	{
		#region Private Class Members
		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor for SelectATable dialog
		/// </summary>
		public SelectATable()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for SelectATable dialog
        /// </summary>
        /// <param name="mainForm">Enter module's main form</param>
        public SelectATable(EnterMainForm mainForm)
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

