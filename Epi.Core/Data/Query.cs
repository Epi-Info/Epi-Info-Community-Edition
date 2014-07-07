using System;
using System.Collections;
using System.Collections.Generic;
using Epi.Data;

namespace Epi.Data
{
	/// <summary>
	/// Container for SQL statement and related parameters
	/// </summary>
	public abstract class Query : IQuery
	{
		#region Private Members
        /// <summary>
        /// SQL Statement
        /// </summary>
        protected string sqlStatement;
        /// <summary>
        /// Parameter collection
        /// </summary>
        protected List<QueryParameter> parameterCollection;  //zack: should be protected
		#endregion Private Members

		#region Constructors
        /// <summary>
        /// Construct an SQL statement container
        /// </summary>
        public Query()
        {
            parameterCollection = new List<QueryParameter>();
        }

		/// <summary>
		/// Query objects are constructed using ANSI SQL-92 syntax
		/// </summary>
        /// <param name="ansiSqlStatement">ANSI SQL-92 statement.</param>
		public Query(string ansiSqlStatement) : this()
		{
            this.sqlStatement = ansiSqlStatement;
		}
		#endregion Constructors

		#region Public Properties
		/// <summary>
        /// Gets/sets the parameter list for the DbQuery
		/// </summary>
        public virtual List<QueryParameter> Parameters
		{
			get
			{
				return parameterCollection;
			}
			set
			{
				parameterCollection = value;
			}
		}

		/// <summary>
        /// Gets/sets the ANSI SQL-92 statement for the DbQuery
		/// </summary>
		public virtual string SqlStatement
		{
			get
			{
                return this.sqlStatement; 
			}
            // set is not allowed
		}        
		#endregion Public Properties

        #region Public Methods
        ///// <summary>
        ///// Get recode case subquery
        ///// </summary>
        ///// <param name="variable"></param>
        ///// <param name="assignToVariable"></param>
        ///// <param name="recodeRanges"></param>
        ///// <returns></returns>
        //public abstract string GetRecodeCaseSubQuery(IVariable variable, IVariable assignToVariable, List<Epi.Core.Interpreter.Rules.RecodeRange> recodeRanges);

        //public abstract void SetRecodeFunction(IVariable variable, IVariable assignToVariable, List<Epi.Core.Interpreter.Rules.RecodeRange> recodeRanges);
        //public abstract IVariable GetRecodeValue(IVariable variable);
        
        /// <summary>
        /// Abstract class GetInsertValue
        /// </summary>
        /// <param name="pvalues">Semicolon delaminated string groups (value1, value2, ... ,valueN) </param>
        /// <returns></returns>
        public abstract string GetInsertValue(string pvalues);  //values format: csv1;csv2;...csvn  

        #endregion

        #region Protected Methods
        /// <summary>
        /// Get Boolean Expression
        /// </summary>
        /// <param name="pEpiBoolean">string</param>
        /// <returns>string</returns>
           protected string GetBooleanExpr(string pEpiBoolean)
            {
                switch (pEpiBoolean)
                {
                    case "(+)":
                        return " is True";
                    case "(-)":
                        return " is False";
                    case "(.)":
                        return " is Null";
                    default:
                        return " is Null";

                }
            }

        #endregion

	}


    /// <summary>
    /// Passing data for Recode 
    /// </summary>
    public struct RangeName
    {
        /// <summary>
        /// Minimum value in range.
        /// </summary>
        public string Low;
        /// <summary>
        /// Maximum value in range.
        /// </summary>
        public string  High;
        /// <summary>
        /// Name of range.
        /// </summary>
        public string Name;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Low">Minimum value in range.</param>
        /// <param name="High">Maximum value in range.</param>
        /// <param name="Name">Name of range.</param>
        public RangeName(string  Low, string  High, string Name)
        {
            this.Low = Low;
            this.High = High;
            this.Name = Name;
        }
    }
}
