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
		public partial class SimpleDialog : DialogBase
    {
        /// <summary>
        /// Gets command line from menu file with simple message
        /// </summary>
        public SimpleDialog(string CommandLine)
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}