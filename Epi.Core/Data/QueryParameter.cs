using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;

namespace Epi.Data
{
	/// <summary>
	/// A database-independent parameter that can be used with Epi Info data access queries
	/// </summary>
	public class QueryParameter : IDbDataParameter 
	{

		#region Private Members

		private int size;
		private byte scale;
		private byte precision;
		private DbType dbType;
		private ParameterDirection direction;
		private bool isNullable;
		private string parameterName = string.Empty;
		private string sourceColumn = string.Empty;
		private DataRowVersion sourceVersion;
		private object parameterValue;

		#endregion

		#region Constructors

		/// <summary>
		/// Private constructor - not accessible
		/// </summary>
		private QueryParameter()
		{

		}

		/// <summary>
		/// Constructor for the class. Initializes a database parameter.
		/// </summary>
		/// <param name="paramName">Name of the parameter prefixed with a '@' symbol.</param>
		/// <param name="paramType">Generic type of the parameter.</param>
		/// <param name="paramValue">Value of the parameter.</param>
		public QueryParameter(string paramName, DbType paramType, object paramValue)
		{
			this.ParameterName = paramName;
			this.DbType = paramType;
			this.Value = paramValue;
			this.Direction = ParameterDirection.Input;
			this.SourceVersion = DataRowVersion.Default;
		}

        /// <summary>
        /// Constructor for the class. Initializes a database parameter.
        /// </summary>
        /// <param name="paramName">Name of the parameter prefixed with a '@' symbol.</param>
        /// <param name="paramType">Generic type of the parameter.</param>
        /// <param name="paramValue">Value of the parameter.</param>
        /// <param name="sourceCol">The name of the source column.</param>
		public QueryParameter(string paramName, DbType paramType,object paramValue,  string sourceCol)
		{
			this.ParameterName = paramName;
			this.DbType = paramType;
			this.Value =  paramValue;
			this.Direction = ParameterDirection.Input;
			this.SourceColumn = sourceCol;
			this.SourceVersion = DataRowVersion.Default;
			
		}

        /// <summary>
        /// Constructor for the class. Initializes a database parameter.
        /// </summary>
        /// <param name="paramName">Name of the parameter prefixed with a '@' symbol.</param>
        /// <param name="paramType">Generic type of the parameter.</param>
        /// <param name="paramValue">Value of the parameter.</param>
        public QueryParameter(string paramName, DbType paramType, object paramValue, int size)
        {
            this.ParameterName = paramName;
            this.DbType = paramType;
            this.Value = paramValue;
            this.Direction = ParameterDirection.Input;
            this.SourceVersion = DataRowVersion.Default;
            this.Size = size;
        }

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets/sets the maximum size, in bytes, of the data within the column.  
		/// </summary>
		public int Size
		{
			get
			{
				return this.size;
			}
			set
			{
				this.size = value;
			}
		}

		/// <summary>
		/// Gets/sets the number of decimal places to which the Value is resolved.  
		/// </summary>
		public byte Scale
		{
			get
			{
				return this.scale;
			}
			set
			{
				this.scale = value;
			}
		}
		
		/// <summary>
		/// Gets/sets the maximum number of digits used to represent the Value property. 
		/// </summary>
		public byte Precision
		{
			get
			{
				return this.precision;
			}
			set
			{
				this.precision = value;
			}
		}


		/// <summary>
		///  Gets/sets the DbType of the parameter.  
		/// </summary>
		public System.Data.DbType DbType
		{
			get
			{
				return this.dbType;
			}
			set
			{
				this.dbType = value;
			}
		}

		/// <summary>
		/// Gets/sets a value indicating whether the parameter is input-only, output-only, bidirectional, or a stored procedure return value parameter.
		/// </summary>
		public ParameterDirection Direction
		{
			get
			{
				return this.direction;
			}
			set
			{
				this.direction = value;
			}
		}

		/// <summary>
		/// Gets/sets whether the parameter is nullable.
		/// </summary>
		public bool IsNullable
		{
			get
			{
				return this.isNullable;
			}
			set
			{
				this.isNullable = value;
			}
		}

		/// <summary>
		/// Gets/sets the name of the parameter. 
		/// </summary>
		public string ParameterName
		{
			get
			{
				return this.parameterName;
			}
			set
			{
				this.parameterName = value;
			}
		}

		/// <summary>
		/// Gets/sets the name of the source column that is mapped to the DataSet and used for loading or returning the Value.
		/// </summary>
		public string SourceColumn
		{
			get
			{
				return this.sourceColumn;
			}
			set
			{
				this.sourceColumn = value;
			}
		}

		/// <summary>
		/// Gets/sets the DataRowVersion to use when loading the Value.
		/// </summary>
		public DataRowVersion SourceVersion
		{
			get
			{
				return this.sourceVersion;
			}
			set
			{
				this.sourceVersion = value;
			}
		}

		/// <summary>
		/// Gets/sets the value of the parameter.
		/// </summary>
		public object Value
		{
			get
			{
				return this.parameterValue;
			}
			set
			{
				this.parameterValue = value;
			}
		}

		#endregion

	}
}
