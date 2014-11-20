using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Epi.Data;

namespace Epi.ImportExport
{
    /// <summary>
    /// Row filter condition for text conditions, e.g. LastName = 'Smith'
    /// </summary>
    public class TextRowFilterCondition : RowFilterConditionBase
    {
        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public TextRowFilterCondition(string sql, string columnName, string paramName, object value) :
            base(sql, columnName, paramName, value)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public TextRowFilterCondition(Epi.ImportExport.Filters.ConditionOperators conditionOperator, string columnName, string paramName, object value) :
            base(conditionOperator, columnName, paramName, value)
        {
        }
        #endregion // Constructors

        #region Protected Methods

        /// <summary>
        /// Validates the condition
        /// </summary>
        protected override void ValidateCondition()
        {
            if (
                (Value == null) ||                
                !(Value is string || Value is String) ||
                (string.IsNullOrEmpty(Value.ToString().Trim())) ||
                (string.IsNullOrEmpty(ParameterName.Trim())) ||
                !(ParameterName.Contains("@"))
                )
            {
                throw new InvalidInputException();
            }
        }

        /// <summary>
        /// Constructs the object
        /// </summary>
        protected override void Construct()
        {
            Parameter = new QueryParameter(ParameterName, System.Data.DbType.String, Value);
        }
        #endregion // Protected Methods
    }
}
