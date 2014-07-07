using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EpiInfo.Plugin;

namespace Epi.Core.EnterInterpreter
{
    public interface IType
    {
           string Name { get;}
    }

    public class cSymbol
    {
        public string Name;
        public EpiInfo.Plugin.DataType Type;
        public EnterRule Rule;

        public cSymbol(string pName, EpiInfo.Plugin.DataType pType)
        {
            this.Name = pName;
            this.Type = pType;
        }

        public cSymbol(EpiInfo.Plugin.IVariable pVariable)
        {
            this.Name = pVariable.Name;
            this.Type = pVariable.DataType;
        }
    }

    public class VariableSymbol : cSymbol
    {
        public VariableSymbol(string pName, EpiInfo.Plugin.DataType pType) : base(pName, pType) { }
        public VariableSymbol(EpiInfo.Plugin.IVariable pVariable) : base(pVariable.Name, pVariable.DataType) { }
        
    }

    public class BuiltInTypeSymbol : cSymbol//, IType
    {
        public BuiltInTypeSymbol(EpiInfo.Plugin.DataType pType) : base(pType.ToString(), pType) { }
        public BuiltInTypeSymbol(EpiInfo.Plugin.IVariable pVariable) : base(pVariable.Name, pVariable.DataType) { }
        //public string Name { get { return this.Type.FullName; } }
    }
}
