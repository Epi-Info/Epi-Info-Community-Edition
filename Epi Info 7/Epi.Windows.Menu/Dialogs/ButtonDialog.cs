using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Windows.Dialogs;

namespace Epi.Windows.Menu.Dialogs
{
    /// <summary>
    /// Not sure what this class is used for.
    /// </summary>
		public partial class ButtonDialog : DialogBase
    {
        /// <summary>
        /// Gets command line from menu file with title and list of buttons
        /// </summary>
        /// <param name="CommandLine"></param>
        public ButtonDialog(string CommandLine)
        {
            InitializeComponent();
        }
        /// <summary>
        /// Handlers LOAD event of the dialog
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void ButtonDialog_Load(object sender, EventArgs e)
        {

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}