using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Epi.Data;

namespace Epi.ImportExport
{
    /// <summary>
    /// Row filter condition for boolean conditions, e.g. BeanSprouts = true
    /// </summary>
    public class CheckboxRowFilterCondition : RowFilterConditionBase
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public CheckboxRowFilterCondition() :
            base()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public CheckboxRowFilterCondition(string sql, string columnName, string paramName, object value) :
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
                !(Sql.Contains("=")) ||
                (string.IsNullOrEmpty(ParameterName.Trim())) ||
                !(ParameterName.Contains("@"))
                )
            {
                throw new InvalidInputException();
            }

            bool result = false;
            bool success = bool.TryParse(Value.ToString(), out result);

            if (!success)
            {
                throw new InvalidInputException();
            }
        }

        /// <summary>
        /// Constructs the object
        /// </summary>
        protected override void Construct()
        {            
            Parameter = new QueryParameter(ParameterName, System.Data.DbType.Int16, bool.Parse(Value.ToString()));
        }
        #endregion // Protected Methods
    }
}
