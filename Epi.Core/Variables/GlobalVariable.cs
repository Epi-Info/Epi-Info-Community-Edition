using System;
using System.Collections.Generic;
using System.Text;

namespace Epi
{
    /// <summary>
    /// Class GlobalVariable
    /// </summary>
    public class GlobalVariable : VariableBase, IScalarVariable, EpiInfo.Plugin.IVariable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="dataType">Data Type</param>
        public GlobalVariable(string name, DataType dataType)
            : base(name, dataType, VariableType.Global)
        {
        }

        /// <summary>
        /// Value
        /// </summary>
        public string Value
        {
            get
            {
                return Expression;
            }
        }

        public EpiInfo.Plugin.DataType DataType { get { return (EpiInfo.Plugin.DataType)this.dataType; } set { return; } }
        public EpiInfo.Plugin.VariableScope VariableScope { get { return EpiInfo.Plugin.VariableScope.Global; } set { return; } }
        public string Namespace { get { return "global"; } set { return; } }
        public string Prompt { get { return this.Name; } set { return; } }
    }
}
