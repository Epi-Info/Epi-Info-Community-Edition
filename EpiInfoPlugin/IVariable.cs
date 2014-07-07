using System;
using System.Collections.Generic;
using System.Text;

namespace EpiInfo.Plugin
{
    public interface IVariable
    {
        string Name { get; set; }
        string Namespace { get; set; }
        DataType DataType { get; set; }
        VariableScope VariableScope { get; set; }
        string Expression { get; set; }
        string Prompt { get; set; }
        
        //string PromptText { get; set; }
        //bool IsVarType(VariableScope typeCombination);
    }
}
