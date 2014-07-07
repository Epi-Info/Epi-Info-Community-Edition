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
    /// Not sure what this class is about.
    /// </summary>
		public partial class SingleValueDialog : DialogBase
    {
        /// <summary>
        /// Gets command line from menu file for entering a value with or 
        /// without mask
        /// </summary>
        /// <param name="CommandLine"></param>
        public SingleValueDialog(string CommandLine)
        {
            InitializeComponent();
        }
        /// <summary>
        /// Cancel Event handler
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}