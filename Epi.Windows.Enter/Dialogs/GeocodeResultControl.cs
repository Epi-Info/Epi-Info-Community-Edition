using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Epi.Enter.Dialogs
{
    public delegate void CoordinatesSelectedHandler(string latitude, string longitude);

    public partial class GeocodeResultControl : UserControl
    {
        public event CoordinatesSelectedHandler CoordinatesSelected;

        public GeocodeResultControl()
        {
            InitializeComponent();
        }

        public GeocodeResultControl(string address, string confidence, string quality, double latitude, double longitude)
        {
            InitializeComponent();
            lblAddress.Text = address;
            lblConfidence.Text = confidence;
            lblQuality.Text = quality;
            lblLatitude.Text = latitude.ToString();
            lblLongitude.Text = longitude.ToString();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (CoordinatesSelected != null)
            {
                CoordinatesSelected(lblLatitude.Text, lblLongitude.Text);
            }
        }
    }
}
