using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Epi.Windows.Analysis.Forms
{
    /// <summary>
    /// 
    /// </summary>
    public class OutputEventArg: EventArgs
    {
        public enum OutputEventArgEnum
        {
            DialogSimple, //Dialog(string pTextPrompt, string pTitleText)
            DialogInputList, //Dialog(string pTextPrompt, ref object pVariable, string pListType, string pTitleText)
            DialogInput, //Dialog(string text, string caption, string mask, string modifier, ref object input)
            Display, //Display(Dictionary<string, string> pDisplayArgs)
            DisplayStatusMessage, //DisplayStatusMessage(Dictionary<string, string> pStatusArgs)
            TryGetFileDialog,//TryGetFileDialog(string filter, string caption, bool isReadOnly, out string filePath)
            SetOutputFile, //SetOutputFile(string fileName)
            ChangeOutput, //ChangeOutput(string fileName, bool isReplace, bool useRouteOut)
            Printout, //Printout(string fileName)
            RunProgram, //RunProgram(string command)
            Quit, //Quit()
            ShowWaitDialog,//ShowWaitDialog(string message)
            HideWaitDialog, //HideWaitDialog()
            ReportIndeterminateTaskStarted,//ReportIndeterminateTaskStarted(string message);
            ReportIndeterminateTaskEnded, //ReportIndeterminateTaskEnded()
            ShowGridTable//ShowGridTable(List<System.Data.DataRow> table, List<string> identifierList)
        }

        OutputEventArgEnum outputEventArgEnum;
        string pTextPrompt;
        string pTitleText;
        public object Variable;
        string pListType;
        string text;
        string caption;
        string mask;
        string modifier;
        public object Input;
        Dictionary<string, string> pDisplayArgs;
        Dictionary<string, string> pStatusArgs;
        string filter;
        bool isReadOnly;
        public string FilePath;
        string fileName;
        bool isReplace;
        bool useRouteOut;
        string command;
        string message;
        List<System.Data.DataRow> table;
        List<string> identifierList;
        DataTable Datatable;
        public object FunctionResult;
        Epi.View epiView;
        /// <summary>
        /// 
        /// </summary>
        public OutputEventArgEnum OutputEventType
        {
            get { return outputEventArgEnum; }
            set { outputEventArgEnum = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string TextPrompt
        {
            get { return pTextPrompt; }
            set { pTextPrompt = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string TitleText
        {
            get { return pTitleText; }
            set { pTitleText = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /*
        public object Variable
        {
            get { return pVariable; }
            set { pVariable = value; }
        }*/

        /// <summary>
        /// 
        /// </summary>
        public string ListType
        {
            get { return pListType; }
            set { pListType = value; }
        }


        /// <summary>
        /// 
        /// </summary>
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Caption
        {
            get { return caption; }
            set { caption = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Mask
        {
            get { return mask; }
            set { mask = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Modifier
        {
            get { return modifier; }
            set { modifier = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /*
        public object Input
        {
            get { return input; }
            set { input = value; }
        }*/

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, string> DisplayArgs
        {
            get { return pDisplayArgs; }
            set { pDisplayArgs = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, string> StatusArgs
        {
            get { return pStatusArgs; }
            set { pStatusArgs = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Filter
        {
            get { return filter; }
            set { filter = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set { isReadOnly = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /*
        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }*/

        /// <summary>
        /// 
        /// </summary>
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsReplace
        {
            get { return isReplace; }
            set { isReplace = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool UseRouteOut
        {
            get { return useRouteOut; }
            set { useRouteOut = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Command
        {
            get { return command; }
            set { command = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<System.Data.DataRow> Table
        {
            get { return table; }
            set { table = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<string> IdentifierList
        {
            get { return identifierList; }
            set { identifierList = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pField">Field</param>
        public OutputEventArg()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        public DataTable dt
        {
            get { return Datatable; }
            set { Datatable = value; }
        }

        public Epi.View EpiView
        {
            get { return epiView; }           
            set { epiView = value;}
        }

    }
}
