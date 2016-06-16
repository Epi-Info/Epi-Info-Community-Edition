using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Epi.Data;

namespace Epi.ImportExport
{
    /// <summary>
    /// Row filter condition for number conditions, e.g. Age = 24
    /// </summary>
    public class NumberRowFilterCondition : RowFilterConditionBase
    {
        #region Constructors

        public NumberRowFilterCondition()
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public NumberRowFilterCondition(string sql, string columnName, string paramName, object value) :
            base(sql, columnName, paramName, value)
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
                (string.IsNullOrEmpty(Value.ToString().Trim())) ||
                (string.IsNullOrEmpty(ParameterName.Trim())) ||
                !(ParameterName.Contains("@"))
                )
            {
                throw new InvalidInputException();
            }
            else
            {
                double result = -1;
                bool success = double.TryParse(Value.ToString(), out result);
                if (!success)
                {
                    throw new InvalidInputException();
                }
            }
        }

        /// <summary>
        /// Constructs the object
        /// </summary>
        protected override void Construct()
        {
            Parameter = new QueryParameter(ParameterName, System.Data.DbType.Double, Value);
        }
        #endregion // Protected Methods
    }
}
