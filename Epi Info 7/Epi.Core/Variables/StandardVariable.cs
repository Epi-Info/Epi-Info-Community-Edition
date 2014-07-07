using System;
using System.Collections.Generic;
using System.Text;

namespace Epi
{
    /// <summary>
    /// Class StandardVariable
    /// </summary>
    public class StandardVariable : VariableBase, ISetBasedVariable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="dataType">Data Type</param>
        public StandardVariable(string name, DataType dataType)
            : base(name, dataType, VariableType.Standard)
        {
        }

        private string sourceColumn = string.Empty; 
        /// <summary>
        /// The value of the current record
        /// </summary>
        protected object currentRecordValueObject = null;
        
        /// <summary>
        /// Returns the current record value
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
