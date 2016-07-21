using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_Report : AnalysisRule
    {

        bool HasRun = false;

        string epiReportExecutablePath = String.Empty;
        string commandlineString = String.Empty;
        string templatePath = String.Empty;
        string option = String.Empty;
        string reportHtmlPath = String.Empty;
        string selectedPrinter = String.Empty;

        public Rule_Report(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            // <Report_Display_Statement>   ::= REPORT File DISPLAY
            // <Report_File_Statement>      ::= REPORT File TO File
            // <Report_Print_Statement>     ::= REPORT File String

            epiReportExecutablePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            epiReportExecutablePath = Path.Combine(epiReportExecutablePath, "EpiReport.exe");
            
            templatePath = this.GetCommandElement(pToken.Tokens, 1);
            option = this.GetCommandElement(pToken.Tokens, 2);
            selectedPrinter = String.Empty;

            if (option.ToUpperInvariant() == CommandNames.TO.ToUpperInvariant())
            {
                reportHtmlPath = this.GetCommandElement(pToken.Tokens, 3);
            }
            else if (option.ToUpperInvariant() != CommandNames.DISPLAY.ToUpperInvariant())
            {
                selectedPrinter = this.GetCommandElement(pToken.Tokens, 2);
                option = "PRINT";
            }

            string commandLineArguments = string.Format(" /template:\"{0}\"", templatePath);
            commandlineString = epiReportExecutablePath + " " + commandLineArguments;
        }

        public override object Execute()
        {
            object result = null;

            if (!this.HasRun)
            {
                Process process = null;
                STARTUPINFO startupInfo = new STARTUPINFO();
                PROCESS_INFORMATION processInfo = new PROCESS_INFORMATION();

                Context.AnalysisOut.PersistVariables();

                switch (option)
                {
                    case CommandNames.DISPLAY:

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
                        break;

                    case CommandNames.TO:
                        break;

                    case "PRINT":
                        break;
                }
                this.HasRun = true;
            }
            return result;
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
    }
}
