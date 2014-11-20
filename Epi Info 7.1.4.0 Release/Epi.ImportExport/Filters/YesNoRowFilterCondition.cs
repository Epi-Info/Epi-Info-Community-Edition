using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Epi.Data;

namespace Epi.ImportExport
{
    /// <summary>
    /// Row filter condition for yes/no conditions, e.g. ILL = 1
    /// </summary>
    public class YesNoRowFilterCondition : RowFilterConditionBase
    {
        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public YesNoRowFilterCondition(string sql, string columnName, string paramName, object value) :
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

            if (Value.ToString() == "1")
            {
                Value = "true";
            }
            else if (Value.ToString() == "0")
            {
                Value = "false";
            }

            bool result = false;
            bool success = bool.TryParse(Value.ToString(), out result);

            if (!success)
            {
                throw new InvalidInputException();
            }
            else
            {
                if (result)
                {
                    Value = 1;
                }
                else
                {
                    Value = 0;
                }
            }

            //Int16 result = -999;

            //bool success = Int16.TryParse(Value.ToString(), out result);

            //if (!success || !(result == 1 || result == 0 || result == -1))
            //{
            //    throw new InvalidInputException();
            //}
        }

        /// <summary>
        /// Constructs the object
        /// </summary>
        protected override void Construct()
        {
            Parameter = new QueryParameter(ParameterName, System.Data.DbType.Int16, Int16.Parse(Value.ToString()));
        }
        #endregion // Protected Methods
    }
}
