using System;
using Epi;
using System.Collections;
using System.Collections.Generic;
using Epi.Collections;
using Epi.Data;

namespace Epi.Data
{
    /// <summary>
    /// Data table class
    /// </summary>
	public class Table : ITable
	{
		#region Private data

        IDbDriver database;
		private string tableName = string.Empty;
				
		#endregion Private data

		#region Constructors

		/// <summary>
		/// Instantiates table using a parent Database and the table's name
		/// </summary>
		/// <param name="db">Data source.</param>
		/// <param name="table">Table name.</param>
		public Table(IDbDriver db, ITable table)
		{
            this.TableName = table.TableName;
            this.Database = db;
		}

        /// <summary>
        /// Instantiates table using a parent Database and the table's name
        /// </summary>
        /// <param name="dataSource">Data source.</param>
        /// <param name="tableName">Table name.</param>
        public Table(IDbDriver dataSource, string tableName)
        {
            this.Database = dataSource;
            this.TableName = tableName;
        }
		
		#endregion Constructors

		#region Public Properties
        /// <summary>
        /// Gets/sets Name of Epi.Data.Table.
        /// </summary>
		public string TableName
		{
			get
			{
				return tableName;
			}
			set
			{
				tableName = value;
			}
		}

        /// <summary>
        /// Gets/sets database instance.
        /// </summary>
        public IDbDriver Database
        {
            get { return database; }
            set { database = value; }
        }
		
		/// <summary>
		/// Gets a list of column names for an Epi.Data.Table.
		/// </summary>
        public List<string> TableColumnNames
		{
			get
			{
				return this.Database.GetTableColumnNames(this.TableName);
			}
		}

		#endregion Public Properties

	}
}
