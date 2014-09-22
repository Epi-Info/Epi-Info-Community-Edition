using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Epi.Core.EnterInterpreter
{
    public class SymbolTable : EpiInfo.Plugin.IScope 
    {
        private string _name;
        private EpiInfo.Plugin.IScope _parent;
        private Dictionary<string, EpiInfo.Plugin.IVariable> _symbolList;

        public SymbolTable(EpiInfo.Plugin.IScope parent)
        {
            _name = null;
            _parent = parent;
            _symbolList = new Dictionary<string, EpiInfo.Plugin.IVariable>(StringComparer.OrdinalIgnoreCase);
        }

        public SymbolTable(string pName, EpiInfo.Plugin.IScope parent)
        {
            _name = pName;
            _parent = parent;
            _symbolList = new Dictionary<string, EpiInfo.Plugin.IVariable>(StringComparer.OrdinalIgnoreCase);
        }

        public string Name
        {
            get { return _name; }
            set 
            {
                if (string.IsNullOrEmpty(_name))
                {
                    _name = value;
                }
            }
        }

        public EpiInfo.Plugin.IScope GetEnclosingScope()
        {
            return _parent;
        }

        public void Define(EpiInfo.Plugin.IVariable symbol)
        {
            if 
            (
                (symbol.VariableScope == EpiInfo.Plugin.VariableScope.Permanent | symbol.VariableScope == EpiInfo.Plugin.VariableScope.Global) && !_name.Equals("global", StringComparison.OrdinalIgnoreCase)
                || !string.IsNullOrEmpty(symbol.Namespace) && (!string.IsNullOrEmpty(_name) && !_name.Equals(symbol.Namespace, StringComparison.OrdinalIgnoreCase))
            )
            {
                if (_parent != null)
                {
                    _parent.Define(symbol);
                }
            }
            else if (!string.IsNullOrEmpty(symbol.Namespace) && !_name.Equals(symbol.Namespace))
            {
                if (_parent != null)
                {
                    _parent.Define(symbol);
                }
            }
            else
            {
                if (_symbolList.ContainsKey(symbol.Name))
                {
                    _symbolList[symbol.Name] = symbol;
                }
                else
                {
                    _symbolList.Add(symbol.Name, symbol);

                    if (symbol.VariableScope == EpiInfo.Plugin.VariableScope.Permanent)
                    {
                        PermanentVariable permanentVariable = new PermanentVariable(symbol.Name, (Epi.DataType)symbol.DataType);
                        permanentVariable.Expression = symbol.Expression;
                        Epi.MemoryRegion.UpdatePermanentVariable(permanentVariable);
                    }
                }
            }
        }

        public void Undefine(string pName, string pNamespace = null)
        {
            if(!string.IsNullOrEmpty(pNamespace) && (!string.IsNullOrEmpty(_name) && !_name.Equals(pNamespace, StringComparison.OrdinalIgnoreCase)))
            {
                if (_parent != null)
                {
                    _parent.Undefine(pName, pNamespace);
                }
            }
            else if (_symbolList.ContainsKey(pName))
            {
                _symbolList.Remove(pName);
            }
            else if (_parent == null)
            {
                // maybe throw error  symbol not in scope
            }
            else
            {
                _parent.Undefine(pName, pNamespace);
            }
        }

        public EpiInfo.Plugin.IVariable Resolve(string pName, string pNamespace = null)
        {
            EpiInfo.Plugin.IVariable result = null;

            if(!string.IsNullOrEmpty(pNamespace) && (!string.IsNullOrEmpty(_name) && !_name.Equals(pNamespace, StringComparison.OrdinalIgnoreCase)))
            {
                if (_parent != null)
                {
                    result = _parent.Resolve(pName, pNamespace);
                }
            }
            else if (_symbolList.ContainsKey(pName))
            {
                result = _symbolList[pName];
            }
            else
            {
                if (_parent != null)
                {
                    result = _parent.Resolve(pName, pNamespace);
                }
            }

            return result;
        }

        public bool SymbolIsInScope(string name)
        {
            return _symbolList.ContainsKey(name);
        }

        public Dictionary<string, EpiInfo.Plugin.IVariable> SymbolList { get { return _symbolList; } }

        public List<EpiInfo.Plugin.IVariable> FindVariables(EpiInfo.Plugin.VariableScope pScopeCombination, string pNamespace = null)
        {
            List<EpiInfo.Plugin.IVariable> result = new List<EpiInfo.Plugin.IVariable>();

            if (!string.IsNullOrEmpty(pNamespace) && (!string.IsNullOrEmpty(_name) && !_name.Equals(pNamespace, StringComparison.OrdinalIgnoreCase)))
            {
                if (_parent != null)
                {
                    result.AddRange(_parent.FindVariables(pScopeCombination, pNamespace));
                }
            }
            else
            {
                foreach (KeyValuePair<string, EpiInfo.Plugin.IVariable> kvp in _symbolList)
                {
                    if ((kvp.Value.VariableScope & pScopeCombination) > 0)
                    {
                        result.Add(kvp.Value);
                    }
                }

                if (_parent != null)
                {
                    result.AddRange(_parent.FindVariables(pScopeCombination, pNamespace));
                }
            }

            return result;
        }

        public void RemoveVariablesInScope(EpiInfo.Plugin.VariableScope scopeCombination, string variableNamespace = null)
        {
            if (!string.IsNullOrEmpty(variableNamespace) && (!string.IsNullOrEmpty(_name) && !_name.Equals(variableNamespace, StringComparison.OrdinalIgnoreCase)))
            {
                if (_parent != null)
                {
                    _parent.RemoveVariablesInScope(scopeCombination);
                }
            }
            else
            {

                List<string> kvplist = new List<string>(); 
                for (int i = 0; i < _symbolList.Count; i++)
                {
                    KeyValuePair<string, EpiInfo.Plugin.IVariable> kvp = _symbolList.ElementAt(i);

                    if ((kvp.Value.VariableScope & scopeCombination) > 0)
                    {
                        kvplist.Add(kvp.Key); 
                    }
                }
                foreach (string kvpkey in kvplist)
                {
                    Undefine(kvpkey, variableNamespace);
                }

                if (_parent != null)
                {
                    _parent.RemoveVariablesInScope(scopeCombination, variableNamespace);
                }
            }
        }
    }
}
