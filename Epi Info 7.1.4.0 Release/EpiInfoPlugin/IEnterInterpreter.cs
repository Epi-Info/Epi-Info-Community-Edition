using System;
using System.Collections.Generic;
using System.Text;

namespace EpiInfo.Plugin
{
    public interface IEnterInterpreter
    {
        string Name { get; }
        IEnterInterpreterHost Host { get; set; }
        void Parse(string pCommandText);
        void Execute(string pCommandText);
        ICommandContext Context { get; set; }

        /*
        List<IVariable> GetVariablesInScope();
        List<IVariable> GetVariablesInScope(VariableScope scopeCombination);
        void RemoveVariablesInScope(VariableScope varTypes);
        void DefineVariable(IVariable variable);
        void UndefineVariable(string varName);
        bool TryGetVariable(string p, out IVariable var);
        string GetCodeBlock(string pLevel, string pIdentifier);
        int[,] GetCodeBlockLineNumbers(string pLevel, string pEvent, string pIdentifier);
        void ExecuteCodeBlock(string pLevel, string pEvent, string pIdentifier);
        */

    }
}
