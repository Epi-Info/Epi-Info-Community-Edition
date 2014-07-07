
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
//using System.Data.Sql;

namespace Epi.Data.SqlServer.Forms
{
    public partial class BrowseForServers : Form
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public BrowseForServers()
        {
            InitializeComponent();
        }

        private void BrowseForServers_Load(object sender, EventArgs e)
        {
            Application.UseWaitCursor = true;
            Application.DoEvents();
            //BrowseForServers.SendMessage(Form.ActiveForm.Handle, 0x20, Form.ActiveForm.Handle, (IntPtr)1);

            System.Data.Sql.SqlDataSourceEnumerator servers = System.Data.Sql.SqlDataSourceEnumerator.Instance;
            DataTable serversTable = servers.GetDataSources();

            foreach(DataRow row in serversTable.Rows)
            {
                if (row[1].Equals(DBNull.Value))
                {
                    string serverName = string.Format("{0}", row[0]);
                    listBox.Items.Add(serverName);
                }
                else
                {
                    string serverName = string.Format("{0}\\{1}", row[0], row[1]);
                }
            }



            //GetNetworkServerInstances();
            Application.UseWaitCursor = false;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        /// <summary>
        /// Get Network SQL Server Instances
        /// </summary>
        public void GetNetworkServerInstances()
        {
            DataTable dataSources = System.Data.Sql.SqlDataSourceEnumerator.Instance.GetDataSources();
            foreach (DataRow dataSource in dataSources.Rows)
            {
                string dataSourceName = dataSource["ServerName"].ToString();                
                if (!string.IsNullOrEmpty(dataSource["InstanceName"].ToString()))
                {
                    dataSourceName += Path.DirectorySeparatorChar + dataSource["InstanceName"].ToString();
                }
                listBox.Items.Add(dataSourceName);
            }

            //List<string> osqlPaths = new List<string>();

            //try
            //{
            //    DirSearch(@"c:\program files\microsoft sql server\", ref osqlPaths);
            //}
            //catch { }

            //if (osqlPaths.Count == 0)
            //{
            //    MessageBox.Show("Cannot browse for servers because SQL Server Client is not installed.");
            //    this.Close();
            //    return;
            //}

            //string filePath = osqlPaths[0];

            //Process cmd = new Process();
            //cmd.StartInfo.WorkingDirectory = Path.GetDirectoryName(filePath);
            //cmd.StartInfo.CreateNoWindow = true;
            //cmd.StartInfo.FileName = filePath;
            //cmd.StartInfo.Arguments = "-L";
            //cmd.StartInfo.UseShellExecute = false;
            //cmd.StartInfo.RedirectStandardOutput = true;
            //cmd.Start();

            //string output = cmd.StandardOutput.ReadToEnd();
            //cmd.WaitForExit(30 * 1000);

            //int startIndex = output.IndexOf("Servers:");
            //if (startIndex > -1)
            //{
            //    StringReader reader = new StringReader(output.Substring(startIndex + "Servers:".Length));
            //    //skip first line
            //    reader.ReadLine();
            //    while (true)
            //    {
            //        string line = reader.ReadLine();
            //        if (string.IsNullOrEmpty(line)) break;
            //        this.listBox.Items.Add(line.Trim());
            //    }
            //}
        }

        private static void DirSearch(string sDir, ref List<string> found)
        {
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
        /// <summary>
        /// Return all network servers
        /// </summary>
        /// <returns></returns>
        public static string BrowseNetworkServers()
        {
            BrowseForServers dialog = new BrowseForServers();
            if (DialogResult.OK == dialog.ShowDialog())
            {
                return dialog.listBox.Text;
            }

            return null;
        }

        private void listBox_DoubleClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(listBox.SelectedItem as string))
            {
                DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }

}