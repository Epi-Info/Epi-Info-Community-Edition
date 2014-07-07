using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpiInfo.Plugin
{
    public interface IScope
    {
        string Name { get; set; }
        Dictionary<string, EpiInfo.Plugin.IVariable> SymbolList { get; }
        
        IScope GetEnclosingScope();
        void Define(EpiInfo.Plugin.IVariable Symbol);
        void Undefine(string Name, string variableNamespace = null);
        void RemoveVariablesInScope(EpiInfo.Plugin.VariableScope scopeCombination, string variableNamespace = null);
        EpiInfo.Plugin.IVariable Resolve(string name, string variableNamespace = null);
        List<EpiInfo.Plugin.IVariable> FindVariables(EpiInfo.Plugin.VariableScope scopeCombination, string variableNamespace = null);
    }
}
