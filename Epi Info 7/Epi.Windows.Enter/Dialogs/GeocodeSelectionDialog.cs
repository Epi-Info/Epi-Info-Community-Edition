using System;
using System.Drawing;
using System.Windows.Forms;

using System.Xml;

namespace Epi.Enter.Dialogs
{
    public partial class GeocodeSelectionDialog : Form
    {
        private string latitude;
        private string longitude;
        private XmlNamespaceManager _nsmgr;

        public GeocodeSelectionDialog(XmlNamespaceManager nsmgr)
        {
            InitializeComponent();
            _nsmgr = nsmgr;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public XmlNodeList Results 
        {
            set
            {
                int counter = 0;
                XmlNode nodeName;
                XmlNode nodePoint;
                XmlNode nodeConfidence;
                XmlNode nodeMatchCodes;
                XmlNode nodeAddress;

                foreach (XmlNode node in value)
                {
                    nodeName = node["Name"];
                    nodePoint = node["Point"];
                    nodeConfidence = node["Confidence"];
                    nodeMatchCodes = node["MatchCode"];
                    nodeAddress = node["Address"];

                    if (nodeName != null && nodePoint != null && nodeConfidence != null && nodeMatchCodes != null && nodeAddress != null)
                    {
                        GeocodeResultControl resultControl = new GeocodeResultControl(
                            nodeAddress["FormattedAddress"].InnerText,
                            nodeConfidence.InnerText,
                            nodeMatchCodes.InnerText,
                            double.Parse(nodePoint["Latitude"].InnerText),
                            double.Parse(nodePoint["Longitude"].InnerText)
                        );

                        resultControl.Location = new Point(3, 153 * counter);
                        resultControl.CoordinatesSelected += new CoordinatesSelectedHandler(resultControl_CoordinatesSelected);
                        pnlContainer.Controls.Add(resultControl);
                        counter++;
                    }
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
