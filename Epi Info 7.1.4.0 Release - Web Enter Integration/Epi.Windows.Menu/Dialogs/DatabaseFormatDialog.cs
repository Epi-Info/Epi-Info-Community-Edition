using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Epi.Windows.Dialogs;

namespace Epi.Windows.Menu.Dialogs
{
	/// <summary>
	/// ***** This is a wireframe and currently contains no functionality *****
	/// </summary>
    public partial class DatabaseFormat : DialogBase 
	{

		/// <summary>
		/// Constructor for the class
		/// </summary>
		public DatabaseFormat()
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

