using System;
using System.Collections.Generic;
using System.Text;

namespace EpiInfo.Plugin
{
    public interface IAnalysisInterpreter
    {
        string Name { get; }
        IAnalysisInterpreterHost Host { get; set; }
        void Parse(string pCommandText);
        void Execute(string pCommandText);
        void CancelProcessing();
        void PauseProcessing();
        void ContinueProcess();
        bool IsOneCommandMode { get; set; }
    }
}
