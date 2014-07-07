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
		public partial class ListDialog : DialogBase
    {
        /// <summary>
        /// Gets command line from menu file with title and list of values
        /// </summary>
        /// <param name="CommandLine"></param>
        public ListDialog(string CommandLine)
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}