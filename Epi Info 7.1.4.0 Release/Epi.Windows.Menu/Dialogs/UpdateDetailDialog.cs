using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Epi.Windows.Menu.Dialogs
{
    public partial class UpdateDetailDialog : Form
    {
        public UpdateDetailDialog()
        {
            InitializeComponent();
        }

        public UpdateDetailDialog(List<System.Xml.XmlElement> elements)
        {
            InitializeComponent();
            txtCurrentVersion.Text = elements[0].Attributes["number"].Value;
            txtYourVersion.Text = new ApplicationIdentity(typeof(Configuration).Assembly).Version;
            StringBuilder sb = new StringBuilder();
            foreach (System.Xml.XmlElement element in elements)
            {
                foreach (System.Xml.XmlElement change in element.ChildNodes)
                {
                    sb.Append(change.InnerText + Environment.NewLine + Environment.NewLine);
                }
            }
            txtChanges.Text = sb.ToString();
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            try
            {
                string programFilesDirectoryName = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles).ToLower();
                string installFolder = AppDomain.CurrentDomain.BaseDirectory.ToLower();
                if (installFolder.StartsWith(programFilesDirectoryName))
                {
                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                    doc.Load("http://ftp.cdc.gov/pub/software/epi_info/sites.xml");
                    List<System.Xml.XmlElement> elements = new List<System.Xml.XmlElement>();
                    foreach (System.Xml.XmlElement element in doc.DocumentElement.ChildNodes)
                    {
                        if (element.Name.Equals("downloads"))
                        {
                            string url = element.InnerText;
                            System.Diagnostics.Process.Start(url);
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Process.Start("updater.exe");
                }
            }
            catch (Exception ex)
            {
                //
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
