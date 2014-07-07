using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Windows;
using Epi.Windows.Dialogs;

namespace Epi.Windows.Menu.Dialogs
{
    /// <summary>
    /// Select a language for translation
    /// </summary>
    public partial class SelectLanguage : DialogBase
    {
        /// <summary>
        /// Construct
        /// </summary>
        public SelectLanguage()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}