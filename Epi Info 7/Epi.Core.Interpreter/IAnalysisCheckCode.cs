using System;
using System.Data;
using System.Collections.Generic;
using System.Text;

namespace Epi.Core.AnalysisInterpreter
{
    public interface IAnalysisCheckCode
    {
        void Dialog(string pTextPrompt, string pTitleText);
        void Dialog(string pTextPrompt, ref object pVariable, string pListType, string pTitleText);
        bool Dialog(string text, string caption, string mask, string modifier, ref object input);
        void Display(Dictionary<string, string> pDisplayArgs);
        void DisplayStatusMessage(Dictionary<string, string> pStatusArgs);
        bool TryGetFileDialog(string filter, string caption, bool isReadOnly, out string filePath);
        //void SetOutputFile(bool useAutomatedFileName);
        void SetOutputFile(string fileName);
        void ChangeOutput(string fileName, bool isReplace, bool useRouteOut);
        void Printout(string fileName);
        void RunProgram(string command);
        void Quit();
        void ShowWaitDialog(string message);
        void HideWaitDialog();
        void ReportIndeterminateTaskStarted(string message);
        void ReportIndeterminateTaskEnded();
        void ShowGridTable(List<DataRow> table, List<string> identifierList, Epi.View epiView);
    }
}
