using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Epi.Analysis;
using Epi;
using Epi.Windows;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Dialog for Printout command
	/// </summary>
    public partial class ReportDialog : CommandDesignDialog
	{
		#region Constructor

        /// <summary>
        /// Constructor for the Printout dialog
        /// </summary>
        /// <param name="frm"></param>
        public ReportDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
		{
			InitializeComponent();
            Construct();
            radioButtonDisplay.Checked = true;
            textBoxFileSavePath.Text = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Reports");

            foreach (String printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                comboBoxSelectPrinter.Items.Add(printer.ToString());
            }

            System.Drawing.Printing.PrintDocument prtdoc = new System.Drawing.Printing.PrintDocument();
            string defaultPrinter = prtdoc.PrinterSettings.PrinterName;
            comboBoxSelectPrinter.Text = defaultPrinter;
            GenerateCommand();
        }
		#endregion Constructors

		#region Event Handlers

		/// <summary>
		/// Clear button clears the text
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnClear_Click(object sender, System.EventArgs e)
		{
            textBoxReportName.Text = string.Empty;
            textBoxFileSavePath.Text = string.Empty;
		}
		#endregion //Event Handlers

		#region Protected Methods
		/// <summary>
		/// Generate command text
		/// </summary>
		protected override void GenerateCommand()
		{
            if (textBoxReportName.Text.Trim().Length > 0)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(CommandNames.REPORT).Append(StringLiterals.SPACE);

                if (textBoxReportName.Text.Trim().Length != 0)
                {
                    sb.Append(Util.InsertInSingleQuotes(textBoxReportName.Text.Trim()));
                }

                sb.Append(StringLiterals.SPACE);

                if (radioButtonPrinter.Checked)
                {
                    sb.Append(Util.InsertInSingleQuotes(comboBoxSelectPrinter.Text.Trim()));
                }
                else if (radioButtonFile.Checked)
                {
                    sb.Append("TO").Append(StringLiterals.SPACE);
                    sb.Append(Util.InsertInSingleQuotes(textBoxFileSavePath.Text.Trim()));
                }
                else
                {
                    sb.Append("DISPLAY");
                }

                CommandText = sb.ToString();
                textBoxCommandPreview.Text = CommandText;
                btnOK.Enabled = true;
            }
            else
            {
                btnOK.Enabled = false;
            }
        }
		#endregion //Protected Methods

        #region Private Methods

        private void Construct()
        {
            if (!this.DesignMode)
            {
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            }
        }

        #endregion Private Methods

        private void btnEditReport_Click(object sender, EventArgs e)
        {
            string epiReportExecutablePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            epiReportExecutablePath = System.IO.Path.Combine(epiReportExecutablePath, "EpiReport.exe");

            String commandText = epiReportExecutablePath;

            string commandlineString = string.Empty;

            commandlineString = commandText + " " + string.Format("/template:\"{0}\"", textBoxReportName.Text.Trim());

            Process process = null;
            STARTUPINFO startupInfo = new STARTUPINFO();
            PROCESS_INFORMATION processInfo = new PROCESS_INFORMATION();
                   
            bool created = CreateProcess(
                null,
                commandlineString, 
                IntPtr.Zero, 
                IntPtr.Zero, 
                false, 
                0, 
                IntPtr.Zero, 
                null, 
                ref startupInfo, 
                out processInfo);
                    
            if (created)
            {
                process = Process.GetProcessById((int)processInfo.dwProcessId);
            }
            else
            {
                throw new GeneralException("Could not execute the command: '" + commandlineString + "'");
            }
        }

        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        public struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        public struct SECURITY_ATTRIBUTES
        {
            public int length;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        // Using interop call here since this will allow us to execute the entire command line string.
        // The System.Diagnostics.Process.Start() method will not allow this.
        [DllImport("kernel32.dll")]
        public static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        private void buttonEllipsisReportName_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Report Files(*.ept)|" + "*.ept|All files (*.*)|*.*";
            dialog.Title = "Select Report Template";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBoxReportName.Text = dialog.FileName;
            }
            GenerateCommand();
        }

        private void buttonEllipsisSaveFileOutputPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            dialog.Description = "Select the directory that the report will be placed in.";
            dialog.ShowNewFolderButton = false;
            dialog.SelectedPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            dialog.SelectedPath = System.IO.Path.Combine(dialog.SelectedPath, "Reports");
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBoxFileSavePath.Text = dialog.SelectedPath;
            }
            GenerateCommand();
        }

        private void radioButtonDisplay_CheckedChanged(object sender, EventArgs e)
        {
            textBoxFileSavePath.Enabled = !radioButtonDisplay.Checked;
            buttonEllipsisSaveFileOutputPath.Enabled = !radioButtonDisplay.Checked;
            comboBoxSelectPrinter.Enabled = !radioButtonDisplay.Checked;
            GenerateCommand();
        }

        private void radioButtonFile_CheckedChanged(object sender, EventArgs e)
        {
            textBoxFileSavePath.Enabled = !radioButtonDisplay.Checked;
            buttonEllipsisSaveFileOutputPath.Enabled = !radioButtonDisplay.Checked;
            comboBoxSelectPrinter.Enabled = radioButtonDisplay.Checked;
            GenerateCommand();
        }

        private void radioButtonPrinter_CheckedChanged(object sender, EventArgs e)
        {
            textBoxFileSavePath.Enabled = radioButtonDisplay.Checked;
            buttonEllipsisSaveFileOutputPath.Enabled = radioButtonDisplay.Checked;
            comboBoxSelectPrinter.Enabled = !radioButtonDisplay.Checked;
            GenerateCommand();
        }

        private void comboBoxSelectPrinter_SelectedIndexChanged(object sender, EventArgs e)
        {
            GenerateCommand();
        }

        private void textBoxReportName_TextChanged(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(textBoxReportName.Text.Trim()))
            {
                btnEditReport.Enabled = true;
            }
            else
            {
                btnEditReport.Enabled = false;
            }
        }
    }
}