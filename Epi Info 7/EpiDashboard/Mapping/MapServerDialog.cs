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
    public partial class MapServerDialog : Form
    {
        public MapServerDialog()
        {
            InitializeComponent();
        }

        private void cbxMapServers_SelectedIndexChanged(object sender, EventArgs e)
        {

            btnOk.Enabled = cbxMapServers.SelectedIndex > -1;

            if (cbxMapServers.SelectedIndex == 0)
            {
                ServerName = "http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer";
                VisibleLayers = new int[] { 7, 19, 21, 22 };
            }
            else if (cbxMapServers.SelectedIndex == 1)
            {
                ServerName = "http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer";
                VisibleLayers = new int[] { 21, 22 };
            }
            else if (cbxMapServers.SelectedIndex == 2)
            {
                ServerName = "http://services.nationalmap.gov/ArcGIS/rest/services/TNM_Blank_US/MapServer";
                VisibleLayers = new int[] { 3, 10, 17 };
            }
            else if (cbxMapServers.SelectedIndex == 3)
            {
                ServerName = "http://rmgsc.cr.usgs.gov/ArcGIS/rest/services/nhss_haz/MapServer";
                VisibleLayers = new int[] { 0 };
            }
            else if (cbxMapServers.SelectedIndex == 4)
            {
                ServerName = "http://rmgsc.cr.usgs.gov/ArcGIS/rest/services/nhss_haz/MapServer";
                VisibleLayers = new int[] { 3 };
            }
            else if (cbxMapServers.SelectedIndex == 5)
            {
                ServerName = "http://rmgsc.cr.usgs.gov/ArcGIS/rest/services/nhss_haz/MapServer";
                VisibleLayers = new int[] { 4 };
            }
            else if (cbxMapServers.SelectedIndex == 6)
            {
                ServerName = "http://rmgsc.cr.usgs.gov/ArcGIS/rest/services/nhss_haz/MapServer";
                VisibleLayers = new int[] { 5, 6 };
            }
        }

        public string ServerName { get; set; }
        public int[] VisibleLayers { get; set; }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void rdbCustom_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbCustom.Checked)
            {
                cbxMapServers.SelectedIndex = -1;
            }
            else
            {
                txtURL.Text = string.Empty;
                lbxVisibleLayers.DataSource = null;
            }
            lblExample.Enabled = rdbCustom.Checked;
            lblFeatures.Enabled = rdbCustom.Checked;
            lblUrl.Enabled = rdbCustom.Checked;
            txtURL.Enabled = rdbCustom.Checked;
            lbxVisibleLayers.Enabled = rdbCustom.Checked;
            btnConnect.Enabled = (rdbCustom.Checked && txtURL.Text.Length > 0);
            cbxMapServers.Enabled = !rdbCustom.Checked;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                string message = GetMessage(txtURL.Text + "?f=json");
                System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();
                Rest rest = ser.Deserialize<Rest>(message);
                lbxVisibleLayers.DataSource = rest.layers;
            }
            catch (Exception ex)
            {
                lbxVisibleLayers.DataSource = null;
                MessageBox.Show("Invalid map server");
            }
        }

        public string GetMessage(string endPoint)
        {
            HttpWebRequest request = CreateWebRequest(endPoint);
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseValue = string.Empty;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }
                using (var responseStream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(responseStream))
                    {
                        responseValue = reader.ReadToEnd();
                    }
                }
                return responseValue;
            }
        }

        private HttpWebRequest CreateWebRequest(string endPoint)
        {
            var request = (HttpWebRequest)WebRequest.Create(endPoint);
            request.Method = "GET";
            request.ContentLength = 0;
            request.ContentType = "text/xml";
            return request;
        }

        private void txtURL_TextChanged(object sender, EventArgs e)
        {
            btnConnect.Enabled = txtURL.Text.Length > 0;
        }

        private void lbxVisibleLayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = lbxVisibleLayers.Items.Count > 0;
            if (lbxVisibleLayers.Items.Count > 0)
            {
                this.ServerName = txtURL.Text;
                if (lbxVisibleLayers.SelectedIndices.Count > 0)
                {
                    int[] visibleLayers = new int[lbxVisibleLayers.SelectedIndices.Count];
                    int counter = 0;
                    foreach (SubObject item in lbxVisibleLayers.SelectedItems)
                    {
                        visibleLayers[counter] = item.id;
                        counter++;
                    }
                    VisibleLayers = visibleLayers;
                }
                else
                {
                    VisibleLayers = null;
                }
            }
        }

        private void lbxVisibleLayers_DataSourceChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = lbxVisibleLayers.DataSource != null;
        }

    }
}
