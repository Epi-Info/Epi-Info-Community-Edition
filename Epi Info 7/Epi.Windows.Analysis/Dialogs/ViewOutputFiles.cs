using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Analysis;
using Epi.Windows;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
    /// <summary>
    /// Partial class View Output Files
    /// </summary>
    public partial class ViewOutputFiles : Form
    {
        private string currentFolder;
        private Configuration config;
        private bool showFlaggedFiles = true;

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewOutputFiles()
        {
            InitializeComponent();

            CreateHeaders();

            config = Configuration.GetNewInstance();
            currentFolder = config.Directories.Output;
            LoadResultsList();
            //RefreshFileList();
        }

        private void LoadResultsList()
        {
            TextReader reader = File.OpenText(Path.Combine(config.Directories.Output, "IHistory.html"));
            string currentLine = String.Empty;
            int line = 0;

            currentLine = reader.ReadLine();
            while (currentLine != null)
            {
                if (currentLine.Contains("<tr>"))
                {

                }
                line++;
                currentLine = reader.ReadLine();
            }
            reader.Close();

        }

        private void RefreshFileList()
        {
            listView1.Items.Clear();

            string[] files = Directory.GetFiles(currentFolder, config.Settings.OutputFilePrefix + "*.htm");
            foreach (string filename in files)
            {
                System.IO.FileInfo fileinfo = new FileInfo(filename);
                
                TextReader reader = File.OpenText(filename);
                string currentLine = String.Empty;
                int line = 0;
                while (currentLine!=null && !currentLine.Contains("<h3>") && line < 100)
                {
                    currentLine = reader.ReadLine();
                    line++;
                }
                reader.Close();

                string command = String.Empty;
                if (!String.IsNullOrEmpty(currentLine) && currentLine.Contains("<h3>") && currentLine.Contains("</h3>"))
                {
                    int pos = currentLine.IndexOf("<h3>");
                    pos += 4;
                    int endPos = currentLine.IndexOf("</h3>");
                    command = currentLine.Substring(pos, endPos - pos);
                }

                long kbSize = fileinfo.Length / (long)1024;
                string[] subitems = new string[4];
                subitems[0] = fileinfo.Name;
                subitems[1] = command;
                subitems[2] = kbSize.ToString();
                subitems[3] = fileinfo.LastWriteTime.ToShortDateString() + " " + fileinfo.LastWriteTime.ToShortTimeString();
                ListViewItem item = new ListViewItem(subitems);
                item.Name = fileinfo.Name;
                item.Text = fileinfo.Name;

                if (showFlaggedFiles)
                {
                    TimeSpan datediff = DateTime.Today - fileinfo.LastWriteTime;

                    if (kbSize >= config.Settings.OutputFileFlagSize || datediff.Days >= config.Settings.OutputFileFlagAge)
                    {
                        item.Selected = true;
                    }
                }

                listView1.Items.Add(item);
            }
            listView1.View = System.Windows.Forms.View.Details;
        }

        private void CreateHeaders()
        {
            ColumnHeader[] headers = new ColumnHeader[4];
            int index = 0;
            headers[index] = new ColumnHeader();
            headers[index].Name = "Filename";
            headers[index].Text = "Filename";
            headers[index].Width = 150;
            headers[index].TextAlign = HorizontalAlignment.Left;
            index++;
            headers[index] = new ColumnHeader();
            headers[index].Name = "Results";
            headers[index].Text = "Results";
            headers[index].TextAlign = HorizontalAlignment.Right;
            headers[index].Width = 200;
            index++;
            headers[index] = new ColumnHeader();
            headers[index].Name = "Size";
            headers[index].Text = "Size(KB)";
            headers[index].TextAlign = HorizontalAlignment.Right;
            headers[index].Width = 75;
            index++;
            headers[index] = new ColumnHeader();
            headers[index].Name = "LastModified";
            headers[index].Text = "Last Modified";
            headers[index].TextAlign = HorizontalAlignment.Right;
            headers[index].Width = 125;
            listView1.Columns.AddRange(headers);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
    }
}
