using Epi.Windows.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EpiDashboard.Dialogs
{
    public partial class ReportURL : DialogBase
    {
        public ReportURL(string URL)
        {
            
            InitializeComponent();
            textBox1.Text = URL;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(this.textBox1.Text);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox1.Text);
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ReportURL_Load(object sender, EventArgs e)
        {

        }
    }
}
