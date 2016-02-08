using System;
using System.Collections.Generic;
using System.Text;

namespace EpiInfo.Plugin
{
    public interface ICommandContext
    {
        ICommand GetCommand(string pSearchText);
        void ClearState();
        IScope Scope { get; }
        List<IVariable> GetVariablesInScope();
        List<IVariable> GetVariablesInScope(VariableScope scopeCombination);
        void RemoveVariablesInScope(VariableScope varTypes);
        void DefineVariable(IVariable variable);
        void UndefineVariable(string varName);
        bool TryGetVariable(string p, out IVariable var);
        IScope GetNewScope(string pName, IScope pParent);

        List<string> ParsedUndefinedVariables { get; }
        List<string> ParsedFieldNames { get; }
        List<string> ParsedPageNames { get; }

        string ExpectedTokens { get; set; }
        string NextToken { get; set; }
        string UnexpectedToken { get; set; }

        void AddToFieldList(List<string> CommandButtonList, List<string> GroupBoxList, List<string> MirrorList, List<string> GridList);
    }
}
