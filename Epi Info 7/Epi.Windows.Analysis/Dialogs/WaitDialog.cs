using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Epi.Analysis.Dialogs
{
    public partial class WaitDialog : Form
    {
        public WaitDialog()
        {
            InitializeComponent();
        }

        public string Prompt
        {
            set
            {
                lblPrompt.Text = value;
            }
        }

        private void WaitDialog_Load(object sender, EventArgs e)
        {
            this.Invalidate();
            Application.DoEvents();
        }
    }
}
