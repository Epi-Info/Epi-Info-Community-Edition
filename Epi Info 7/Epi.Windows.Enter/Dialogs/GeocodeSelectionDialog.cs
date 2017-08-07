using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace Epi.Enter.Dialogs
{
    public partial class GeocodeSelectionDialog : Form
    {
        private string latitude;
        private string longitude;

        public GeocodeSelectionDialog()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        void resultControl_CoordinatesSelected(string latitude, string longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        public string Latitude 
        {
            get
            {
                return latitude;
            }
        }

        public string Longitude 
        {
            get
            {
                return longitude;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.microsoft.com/maps/");
        }
    }
}
