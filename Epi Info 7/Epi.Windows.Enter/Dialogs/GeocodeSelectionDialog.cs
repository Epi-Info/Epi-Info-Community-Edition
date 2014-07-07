using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using ESRI.ArcGIS.Client.Bing.GeocodeService;

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

        public ObservableCollection<GeocodeResult> Results 
        {
            set
            {
                int counter = 0;
                foreach (GeocodeResult result in value)
                {
                    GeocodeResultControl resultControl = new GeocodeResultControl(result.DisplayName, result.Confidence.ToString(), result.MatchCodes[0], result.Locations[0].Latitude, result.Locations[0].Longitude);
                    resultControl.Location = new Point(3, 153 * counter);
                    resultControl.CoordinatesSelected += new CoordinatesSelectedHandler(resultControl_CoordinatesSelected);
                    pnlContainer.Controls.Add(resultControl);
                    counter++;
                }
            }
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
