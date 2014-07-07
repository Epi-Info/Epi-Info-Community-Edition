using System;
using System.Collections.Generic;
using System.Text;

namespace EpiInfo.Plugin
{
    public interface IAnalysisInterpreterHost
    {
        bool Register(IAnalysisInterpreter analysisInterpreter);

        void Dialog(string pTextPrompt, string pTitleText);
        void Dialog(string pTextPrompt, ref object pVariable, string pListType, string pTitleText);
        bool Dialog(string text, string caption, string mask, string modifier, ref object input);
        void Display(Dictionary<string, string> pDisplayArgs);
        void DisplayStatusMessage(Dictionary<string, string> pStatusArgs);
        bool TryGetFileDialog(string filter, string caption, bool isReadOnly, out string filePath);
        void SetOutputFile(bool useAutomatedFileName);
        void SetOutputFile(string fileName);
        void ChangeOutput(string fileName, bool isReplace, bool useRouteOut);
        void Printout(string fileName);
        void RunProgram(string command);
        void Quit();
    }
}
