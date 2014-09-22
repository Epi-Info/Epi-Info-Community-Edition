using System;
using System.Data;
using Epi.DataSets;
using System.Collections.Generic;
using System.Text;

namespace Epi
{
    /// <summary>
    /// Permanent Variable class
    /// </summary>
    public class PermanentVariable : VariableBase, IScalarVariable, EpiInfo.Plugin.IVariable
    {
        private Configuration config;

        //public string Name { get; set; }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Variable name.</param>
        /// <param name="dataType">Variable data type enumeration.</param>
        public PermanentVariable(string name, DataType dataType)
            : base(name, dataType, VariableType.Permanent)
        {
            config = Configuration.GetNewInstance();
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="row">Permanent variable <see cref="System.Data.DataRow"/>.</param>
        public PermanentVariable(Config.PermanentVariableRow row)
            : base(row.Name, (DataType)row.DataType, VariableType.Permanent)
        {
            config = Configuration.GetNewInstance();
            this.Expression = row.DataValue;            
        }

        /// <summary>
        /// Property get/set for the permanent variable's expression.
        /// </summary>
        public override string Expression
        {
            get
            {
                return base.Expression;
            }
            set
            {
                config = Configuration.GetNewInstance();
                DataRow[] result = config.PermanentVariables.Select("Name='" + Name + "'");
                if (result.Length == 1)
                {
                    ((Config.PermanentVariableRow)result[0]).DataValue = value;
                    Configuration.Save(config);
                }


                base.Expression = value;
            }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        public string Value
        {
            get
            {
                return Expression;
            }
        }

        public EpiInfo.Plugin.DataType DataType { get { return (EpiInfo.Plugin.DataType)this.dataType; } set { return; } }
        public EpiInfo.Plugin.VariableScope VariableScope { get { return EpiInfo.Plugin.VariableScope.Permanent; } set { return; } }
        public string Namespace { get { return "global"; } set { return; } }
        public string Prompt { get { return this.Name; } set { return; } }
    }
}
