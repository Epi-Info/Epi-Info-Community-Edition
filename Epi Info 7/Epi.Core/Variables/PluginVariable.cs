using System;
using System.Collections.Generic;
using System.Text;
using EpiInfo.Plugin;

namespace Epi
{
    public class PluginVariable : EpiInfo.Plugin.IVariable
    {
        private string _name;
        private EpiInfo.Plugin.DataType _dataType;
        private EpiInfo.Plugin.VariableScope _variableScope;
        private string _expression;
        private string _variableNamespace;
        private string _prompt;

        public PluginVariable() { }

        public PluginVariable(string name, EpiInfo.Plugin.DataType dataType, EpiInfo.Plugin.VariableScope variableScope, string expression, string variableNamespace = null, string prompt = null)
        {
            _name = name;
            _dataType = dataType;
            _variableScope = variableScope;
            _expression = expression;
            _variableNamespace = variableNamespace;
            _prompt = prompt;
        }

        public string Name 
        { 
            get { return _name; } 
            set { _name = value; } 
        }
        
        public EpiInfo.Plugin.DataType DataType 
        { 
            get { return _dataType; } 
            set { _dataType = value; } 
        }

        public EpiInfo.Plugin.VariableScope VariableScope 
        { 
            get { return _variableScope; } 
            set { _variableScope = value; } 
        }
        
        public string Expression 
        { 
            get { return _expression; } 
            set { _expression = value; } 
        }
        
        public string Namespace 
        {
            get { return _variableNamespace; }
            set { _variableNamespace = value; } 
        }

        public string Prompt 
        { 
            get { return _prompt; } 
            set { _prompt = value; } 
        }
    }
}
