using System;
using System.Collections.Generic;
using System.Text;

namespace EpiInfo.Plugin
{
    public interface IEnterInterpreterHost
    {
        bool Register(IEnterInterpreter enterInterpreter);
        bool IsExecutionEnabled { get; }
        bool IsSuppressErrorsEnabled { get; }
        bool Assign(string pName, object pValue);
        bool AssignGrid(string pName, object pValue, int pIndex0, object pIndex1);
        bool Geocode(string address, string latName, string longName);
        void AutoSearch(string[] pIdentifierList, string[] pDisplayList, bool pAlwaysShow);
        void Clear(string[] pIdentifierList);
        void Dialog(string pTextPrompt, string pTitleText);
        void Dialog(string pTextPrompt, ref object pVariable, string pListType, string pTitleText);
        bool Dialog(string text, string caption, string mask, string modifier, ref object input, EpiInfo.Plugin.DataType dataType);
        string GetValue(string pName);
        IVariable GetGridValue(string pName, int pIndex0, object pIndex1);
        void GoTo(string pDesitnation, string targetPage = "", string targetForm = "");
        void Hide(string[] pNameList, bool pIsAnExceptList);
        void Highlight(string[] pNameList, bool pIsAnExceptList);
        void UnHighlight(string[] pNameList, bool pIsAnExceptList);
        void Enable(string[] pNameList, bool pIsAnExceptList);
        void Disable(string[] pNameList, bool pIsAnExceptList);
        void SetRequired(string[] pNameList);
        void SetNotRequired(string[] pNameList);
        void NewRecord();
        bool SaveRecord();
        int RecordCount();
        void UnHide(string[] pNameList, bool pIsAnExceptList);
        bool TryGetFieldInfo(string name, out DataType dataType, out string value);
        List<string> GetDbListValues(string ptablename, string pvariablename);
        void Quit();
    }
}
