using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace EpiDashboard.Mapping
{
    public partial class KmlDialog : Form
    {
        public KmlDialog()
        {
            InitializeComponent();
        }


        public string ServerName { get; set; }
        public int[] VisibleLayers { get; set; }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.ServerName = txtURL.Text;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void txtURL_TextChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = (txtURL.Text.Length > 0);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "KML/KMZ Files (*.kml/*.kmz)|*.kml;*.kmz";
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtURL.Text = dialog.FileName;
            }
        }
    }
}
