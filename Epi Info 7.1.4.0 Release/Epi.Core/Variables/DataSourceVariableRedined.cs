using System;
using System.Collections.Generic;
using System.Text;

namespace Epi
{
    /// <summary>
    /// Class DataSourceVariableRedefined
    /// </summary>
    public class DataSourceVariableRedefined : VariableBase, ISetBasedVariable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="dataType">Data Type</param>
        public DataSourceVariableRedefined(string name, DataType dataType)
            : base(name, dataType, VariableType.DataSourceRedefined)
        {

        }
        
        private string sourceColumn = string.Empty;

        /// <summary>
        /// The value of the current record
        /// </summary>
        protected object currentRecordValueObject = null;
        
        /// <summary>
        /// Returns the value of the current record
        /// </summary>
        public virtual object CurrentRecordValueObject
        {
            get
            {
                return currentRecordValueObject;
            }
            set
            {
                currentRecordValueObject = value;
            }
        }
    }
}
