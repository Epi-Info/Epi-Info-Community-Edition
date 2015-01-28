using System;
using System.Collections.Generic;
using System.Text;

namespace Epi.Core.EnterInterpreter
{
    public interface IEnterCheckCode
    {
        bool Assign(string pName, object pValue);
        void AutoSearch(string[] pIdentifierList);
        void Clear(string[] pIdentifierList);
        void Dialog(string pTextPrompt, string pTitleText);
        void Dialog(string pTextPrompt,ref object pVariable, string pListType, string pTitleText);
        bool Dialog(string text, string caption, string mask, string modifier, ref object input);
        string GetValue(string pName);
        void GoTo(string pDesitnation, string targetPage = "", string targetForm = "");
        void Hide(string[] pNameList, bool pIsAnExceptList);
        void NewRecord();
        void UnHide(string[] pNameList, bool pIsAnExceptList);
        bool TryGetFieldInfo(string name, out DataType dataType, out string value);
        void Quit();
    }
}
