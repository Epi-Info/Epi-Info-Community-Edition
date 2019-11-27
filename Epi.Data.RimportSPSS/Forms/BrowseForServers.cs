using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime;
using System.Diagnostics;
using System.Data.OleDb;
using Epi.Data.RimportSPSS;
using RDotNet;


namespace Epi.Data.RimportSPSS.Forms
{
    public partial class BrowseForServers : Form
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BrowseForServers()
        {
            InitializeComponent();
        }

        private void BrowseForServers_Load(object sender, EventArgs e)
        {
            Application.UseWaitCursor = true;
            Application.DoEvents();
            BrowseForServers.SendMessage(Form.ActiveForm.Handle, 0x20, Form.ActiveForm.Handle, (IntPtr)1);
            GetNetworkServerInstances();
            Application.UseWaitCursor = false;
        }

        //TODO: remove user32
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        /// <summary>
        /// Get Network RimportSPSS Instances
        /// </summary>
        public void GetNetworkServerInstances()
        {
            //TODO: this needs a lot of work - in progress

            //DataTable dataSources = System.Data.Sql.SqlDataSourceEnumerator.Instance.GetDataSources();
            StringBuilder commandBuilder = new StringBuilder();

            commandBuilder.Append("SHOW DATABASES;");

            OleDbCommand command = new OleDbCommand(commandBuilder.ToString());

        }

        private static void DirSearch(string sDir, ref List<string> found)
        {
            //TODO: need to handle dir searching in Linux
            foreach (string d in Directory.GetDirectories(sDir))
            {
                foreach (string f in Directory.GetFiles(d, "osql.exe"))
                {
                    found.Add(f);
                }
                DirSearch(d, ref found);
            }
        }

        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.btnOK.Enabled = true;
        }

        public static string BrowseNetworkServers()
        {
            BrowseForServers dialog = new BrowseForServers();
            if (DialogResult.OK == dialog.ShowDialog())
            {
                // return dialog.listBox.Text;
                return null;
            }

            return null;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!(txtFileName.Text.Equals(String.Empty)))
            {
                RimportSPSSDatabase RimportSPSSDatabase = new RimportSPSSDatabase();
                RimportSPSSDatabase.SetDataSourceFilePath(this.txtFileName.Text);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please choose a server.");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.InitialDirectory = "c:\\";
                openFileDialog1.Filter = "SPSS files (*.sav)";
                openFileDialog1.FilterIndex = 1;  // default is 1--use if multiple types
                openFileDialog1.RestoreDirectory = true;

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog1.FileName;
                }
            }

            MessageBox.Show(fileContent, "File path: " + filePath, MessageBoxButtons.OK);
        }
    
    
    
    
    }
}