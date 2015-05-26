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
    public partial class MapServerFeatureDialog : Form
    {
        public MapServerFeatureDialog()
        {
            InitializeComponent();
        }

        private void cbxMapServers_SelectedIndexChanged(object sender, EventArgs e)
        {

            btnOk.Enabled = cbxMapServers.SelectedIndex > -1;
            if(cbxMapServers.SelectedItem!=null)
            ServerName = cbxMapServers.SelectedItem.ToString();
        }

        public string ServerName { get; set; }
        public int VisibleLayer { get; set; }

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
                cbxVisibleLayer.DataSource = null;
            }
            lblExample.Enabled = rdbCustom.Checked;
            lblFeatures.Enabled = rdbCustom.Checked;
            lblUrl.Enabled = rdbCustom.Checked;
            txtURL.Enabled = rdbCustom.Checked;
            cbxVisibleLayer.Enabled = rdbCustom.Checked;
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
                cbxVisibleLayer.DataSource = rest.layers;
            }
            catch (Exception ex)
            {
                cbxVisibleLayer.DataSource = null;
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

        private void cbxVisibleLayer_DataSourceChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = cbxVisibleLayer.DataSource != null;
        }

        private void cbxVisibleLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = cbxVisibleLayer.Items.Count > 0;
            if (cbxVisibleLayer.Items.Count > 0)
            {
                this.ServerName = txtURL.Text;
                if (cbxVisibleLayer.SelectedIndex > -1)
                {
                    int visibleLayer = ((SubObject)cbxVisibleLayer.SelectedItem).id;
                    VisibleLayer = visibleLayer;
                }
                else
                {
                    VisibleLayer = -1;
                }
            }
        }

    }

    public class SubObject
    {
        public string name { get; set; }
        public int id { get; set; }
        public override string ToString()
        {
            return name;
        }
    }
    public class Rest
    {
        public Rest() { layers = new List<SubObject>(); }
        public List<SubObject> layers { get; set; }
    }
}
