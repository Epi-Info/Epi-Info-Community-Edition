using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Analysis;
using Epi.Windows;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
    public partial class StoringOutputDialog : CommandDesignDialog
    {
        private Configuration config;
        
        /// <summary>
        /// Constructor
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public StoringOutputDialog()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="frm">Main Form</param>
        public StoringOutputDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
		{
			InitializeComponent();
            Construct();
		}

        private void Construct()
        {
            this.config = Configuration.GetNewInstance();
            txtResultsFolder.Text = Convert.ToString(config.Directories.Output);
            txtArchiveFolder.Text = Convert.ToString(config.Directories.Archive);
            txtOutputPrefix.Text = Convert.ToString(config.Settings.OutputFilePrefix);
            txtOutputSequence.Text = config.Settings.OutputFileSequence.ToString();
            txtFlagAge.Text = config.Settings.OutputFileFlagAge.ToString();
            txtFlagNumber.Text = config.Settings.OutputFileFlagNumber.ToString();
            txtFlagSize.Text = config.Settings.OutputFileFlagSize.ToString();
        }

        private void btnResultsFolder_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(config.Directories.Output))
            {
                folderBrowserDialog1.SelectedPath = config.Directories.Output;
            }
            DialogResult result = folderBrowserDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                txtResultsFolder.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btnArchiveFolder_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                txtArchiveFolder.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            ValidateAndSave();
            this.Close();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            ValidateAndSave();
        }

        private void ValidateAndSave()
        {
            int errorCount = 0;
            string fullPath;
            if (ValidPath(txtResultsFolder.Text, out fullPath))
            {
                txtResultsFolder.Text = fullPath;
                config.Directories.Output = fullPath;
            }
            else
            {
                errorCount++;
                MessageBox.Show("Invalid path for Results Folder");
            }

            if (ValidPath(txtArchiveFolder.Text, out fullPath))
            {
                txtArchiveFolder.Text = fullPath;
                config.Directories.Archive = fullPath;
            }
            else
            {
                errorCount++;
                MessageBox.Show("Invalid path for Archive Folder");
            }

            if (ValidFileName(txtOutputPrefix.Text))
            {
                config.Settings.OutputFilePrefix = txtOutputPrefix.Text;
            }
            else
            {
                errorCount++;
                MessageBox.Show("Invalid value for Output File Prefix.");
            }

            int sequenceNumber = 1;
            if (Int32.TryParse(txtOutputSequence.Text, out sequenceNumber))
            {
                config.Settings.OutputFileSequence = sequenceNumber;
            }
            else
            {
                errorCount++;
                MessageBox.Show("Invalid Output File Sequence. Must be an integer value.");
            }

            int flagAge = 0;
            if (Int32.TryParse(txtFlagAge.Text, out flagAge))
            {
                config.Settings.OutputFileFlagAge = flagAge;
            }
            else
            {
                errorCount++;
                MessageBox.Show("Invalid value for Age In Days. Must be an integer value.");
            }

            int flagNumber = 0;
            if (Int32.TryParse(txtFlagNumber.Text, out flagNumber))
            {
                config.Settings.OutputFileFlagNumber = flagNumber;
            }
            else
            {
                errorCount++;
                MessageBox.Show("Invalid value for Number of Results. Must be an integer value.");
            }

            int flagSize = 0;
            if (Int32.TryParse(txtFlagSize.Text, out flagSize))
            {
                config.Settings.OutputFileFlagSize = flagSize;
            }
            else
            {
                errorCount++;
                MessageBox.Show("Invalid value for File Size. Must be an integer value.");
            }

            if (errorCount == 0)
            {
                Configuration.Save(config);
            }
        }

        private bool ValidFileName(string filename)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            for(int i=0; i<invalidChars.Length; i++)
            {
                int index = filename.IndexOf(invalidChars[i]);
                if (index > 0)
                {
                    return false;
                }
            }


            return true;
        }

        private bool ValidPath(string filePath, out string fullPath)
        {
            fullPath = null;

            try
            {
                fullPath = Path.GetFullPath(filePath);
                if (!Directory.Exists(fullPath))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(DateTime.Now + ":  " + ex.Message);
                return false;
            }

            return true;
        }

        private void btnDeleteResults_Click(object sender, EventArgs e)
        {
            
        }

        private void btnViewResults_Click(object sender, EventArgs e)
        {
            ViewOutputFiles resultsDialog = new ViewOutputFiles();
            resultsDialog.ShowDialog(this);
        }

        private void btnViewArchive_Click(object sender, EventArgs e)
        {

        }

        private void btnDeleteArchive_Click(object sender, EventArgs e)
        {

        }

        private void btnArchiveResults_Click(object sender, EventArgs e)
        {

        }

    }
}
