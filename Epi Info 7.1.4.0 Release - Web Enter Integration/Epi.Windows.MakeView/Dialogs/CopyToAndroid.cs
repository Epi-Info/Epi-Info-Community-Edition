using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Epi;
using PortableDevices;
using System.Linq;
using System.Text.RegularExpressions;

namespace Epi.Windows.MakeView.Dialogs
{
    public partial class CopyToAndroid : Epi.Windows.Dialogs.DialogBase
    {
        private PresentationLogic.GuiMediator mediator;

        public CopyToAndroid()
        {
            InitializeComponent();
        }

        public CopyToAndroid(View currentView, PresentationLogic.GuiMediator mediator)
        {
            InitializeComponent();
            this.mediator = mediator;
            txtFormName.Text = currentView.Name;
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {

            string[] drives = Directory.GetLogicalDrives();
            bool found = false;
            foreach (string drive in drives)
            {
                if (File.Exists(drive + "Download\\EpiInfo\\Handshake.xml"))
                {
                    Template template = new Template(mediator);
                    template.CreatePhoneTemplate(drive + "Download\\EpiInfo\\Questionnaires\\" + txtFormName.Text + ".xml");
                    found = true;
                }
            }
            if (!found)
            {
                //try portable device api

                try
                {
                    var devices = new PortableDevices.PortableDeviceCollection();

                    if (devices == null)
                    {
                        Epi.Windows.MsgBox.ShowInformation(SharedStrings.MOBILE_NO_DEVICES);
                        return;
                    }

                    try
                    {
                        devices.Refresh();
                    }
                    catch (System.IndexOutOfRangeException)
                    {
                        Epi.Windows.MsgBox.ShowInformation(SharedStrings.MOBILE_NO_DEVICES);
                        return;
                    }

                    if (devices.Count == 0)
                    {
                        Epi.Windows.MsgBox.ShowInformation(SharedStrings.MOBILE_NO_DEVICES);
                        return;
                    }

                    var pd = devices.First();
                    pd.Connect();
                    PortableDeviceFolder root = pd.GetContents();

                    PortableDeviceFolder download =
                        (from r in ((PortableDeviceFolder)root.Files[0]).Files
                         where r.Name.Equals("Download")
                         select r).First() as PortableDeviceFolder;

                    PortableDeviceFolder epiinfo =
                        (from r in download.Files
                         where r.Name.Equals("EpiInfo")
                         select r).First() as PortableDeviceFolder;

                    PortableDeviceFolder questionnaires =
                        (from r in epiinfo.Files
                         where r.Name.Equals("Questionnaires")
                         select r).First() as PortableDeviceFolder;

                    if (questionnaires == null)
                    {
                        Epi.Windows.MsgBox.ShowInformation(SharedStrings.MOBILE_NO_DEVICES);
                        pd.Disconnect();
                        return;
                    }
                    else
                    {
                        try
                        {
                            PortableDeviceFile existingFile =
                            (from r in questionnaires.Files
                             where r.Name.Equals(txtFormName.Text + ".xml")
                             select r).First() as PortableDeviceFile;
                            pd.DeleteFile(existingFile);
                        }
                        catch (Exception ex)
                        {
                            //
                        }

                        Template template = new Template(mediator);
                        template.CreatePhoneTemplate(Path.GetTempPath() + "\\" + txtFormName.Text + ".xml");

                        pd.TransferContentToDevice(Path.GetTempPath() + "\\" + txtFormName.Text + ".xml", questionnaires.Id);
                        found = true;
                    }

                    pd.Disconnect();
                }
                catch (Exception ex)
                {

                }
            }

            if (found)
            {
                MessageBox.Show(SharedStrings.MOBILE_FORM_COPIED, SharedStrings.MOBILE_FORM_COPY_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show(SharedStrings.MOBILE_NO_EI_EXT_FOUND, SharedStrings.ERROR, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtFormName_TextChanged(object sender, EventArgs e)
        {
            if (txtFormName.Text.Length > 0)
            {
                string strTestForSymbols = txtFormName.Text;
                Regex regex = new Regex("[\\w\\d]", RegexOptions.IgnoreCase);
                string strResultOfSymbolTest = regex.Replace(strTestForSymbols, string.Empty);

                if (strResultOfSymbolTest.Length > 0)
                {
                    btnCopy.Enabled = false;
                    txtFormName.ForeColor = Color.Red;
                }
                else
                {
                    if (!Util.IsFirstCharacterALetter(txtFormName.Text))
                    {
                        btnCopy.Enabled = false;
                        txtFormName.ForeColor = Color.Red;
                    }
                    else
                    {
                        txtFormName.ForeColor = SystemColors.ControlText;
                        btnCopy.Enabled = true;
                    }
                }
            }
            else
            {
                txtFormName.ForeColor = SystemColors.ControlText;
                btnCopy.Enabled = false;
            }
        }

    }
}
