using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Text;
using Epi;
using Epi.Data;

namespace Epi.ImportExport.Dialogs
{
    public partial class ImportMessagesDialog : Form
    {
        private string logFilePath = string.Empty;
        private TimeSpan timeElapsed;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ImportMessagesDialog()
        {
            InitializeComponent();
            logFilePath = Logger.GetLogFilePath();
            txtLogFilePath.Text = logFilePath;
            if (string.IsNullOrEmpty(logFilePath))
            {
                btnOpenLogFile.Enabled = false;
            }
            TimeElapsed = new TimeSpan();
            TextReader reader = File.OpenText(logFilePath);
            string currentLine = String.Empty;
            int line = 0;
            while (currentLine != null)
            {
                currentLine = reader.ReadLine();
                if (currentLine != null && (currentLine.ToLowerInvariant().Contains(SharedStrings.IMPORT_PREFIX_ERROR.ToLowerInvariant()) || currentLine.Contains(":  Project") || currentLine.ToLowerInvariant().Contains(SharedStrings.IMPORT_PREFIX_WARNING.ToLowerInvariant()) || currentLine.Contains(":  Import") || currentLine.ToLowerInvariant().Contains(SharedStrings.IMPORT_PREFIX_NOTICE.ToLowerInvariant()) || (currentLine.Contains(":  Project ") && currentLine.Contains("created in"))))
                {
                    listBox.Items.Add(currentLine);
                }
                line++;
            }
            reader.Close();
            reader.Dispose();
        }

        /// <summary>
        /// Gets/sets the amount of time it took for the import to complete
        /// </summary>
        public TimeSpan TimeElapsed
        {
            get
            {
                return timeElapsed;
            }
            set
            {
                timeElapsed = value;
            }
        }

        private void btnOpenLogFile_Click(object sender, EventArgs e)
        {
            if (File.Exists(logFilePath))
            {   
                Process exec = new Process();
                exec.StartInfo.FileName = logFilePath;                
                exec.Start();
            }
            else
            {
                //MsgBox.ShowError(SharedStrings.FILE_NOT_FOUND + ": " + logFilePath);
            }
        }

        private void ImportMessagesDialog_Shown(object sender, EventArgs e)
        {
            if (TimeElapsed != null)
            {
                txtElapsedTime.Text = TimeElapsed.ToString();
            }
        }
    }

}