using System;
using System.Collections.Generic;
using System.Text;

using Epi;
using Epi.Collections;

namespace Epi
{
    /// <summary>
    /// List of fields to sort by in SQL command
    /// </summary>
    public class SortCriteria : TaggedList<IVariable, SortOrder>
    {
        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public SortCriteria()
        {
        }
        #endregion Constructor

        #region Public methods
        /// <summary>
        /// Adds a variable to the SortCriteria list
        /// </summary>
        /// <param name="var"></param>
        /// <param name="order"></param>
        public new void Add(IVariable var, SortOrder order)
        {
            base.Add(var, order);
        }

        /// <summary>
        /// Returns a SQL-friendly string representation of Sort criteria.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            WordBuilder builder = new WordBuilder(", ");
            for (int index = 0; index < this.Count; index++)
            {
                string var = string.Empty;
                if (this[index].IsVarType(VariableType.DataSourceRedefined))  
                {
                    string s = string.Empty;
                    var = ((DataSourceVariableRedefined)this[index]).Expression;
                }    
                else if (this[index].IsVarType(VariableType.Standard ))
                {
                        var = ((StandardVariable)this[index]).Expression;
                }
                else
                {
                    //defect 267
//                    var = this[index].Expression;                
                    var = this[index].Name;
                }
                if (tags[index] == SortOrder.Descending)
                {
                    var += CharLiterals.SPACE + "desc";
                }
                
                builder.Add(var);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Returns the sort order of the variable
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
        public SortOrder GetSortOrderForVariable(IVariable var)
        {
            return base.GetTag(var);
        }
        #endregion Public methods
    }
}