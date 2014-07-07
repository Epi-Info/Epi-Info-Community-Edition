using System;

namespace Epi.Data.SqlServer
{
    /// <summary>
    /// SqlException class
    /// </summary>
	public class SqlException : System.ApplicationException
	{
		#region Private data
		private string sqlStatement;
		#endregion Private data

		#region Constructors
        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="stmt"> Sql statement</param>
        /// <param name="inner">System Exception</param>
		public SqlException(string stmt, System.Exception inner) : base(inner.Message, inner)
		{
			sqlStatement = stmt;
		}
		#endregion Constructors

		#region Public Properties
        /// <summary>
        /// Read only property SqlStatement
        /// </summary>
		public string SqlStatement
		{
			get
			{
				return sqlStatement;
			}
		}
		#endregion Public Properties
	}
}
