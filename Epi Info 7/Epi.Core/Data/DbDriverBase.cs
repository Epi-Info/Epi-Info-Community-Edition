using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Epi.Data
{
    /// <summary>
    /// Provides base implementation of IDbDriver implementation
    /// </summary>
    public abstract class DbDriverBase : IDbDriver
    {
        #region Public Events
      
        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Gets the default schema prefix if applicable (ex. dbo for SQL Server)
        /// </summary>
        public abstract string SchemaPrefix
        {
            get;
        }

        /// <summary>
        /// Property to specify if the same operation will be called multiple times. Data driver should use this to optimize code.
        /// </summary>
        public abstract bool IsBulkOperation
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the full name of the data source. Typically used for display purposes
        /// </summary>
        public abstract string FullName
        {
            get;
        }

        /// <summary>
        /// Gets/sets the Database name
        /// </summary>
        public abstract string DbName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the database connection string
        /// </summary>
        public abstract string ConnectionString
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the maximum number of columns a table can have.
        /// </summary>
        public abstract int TableColumnMax
        {
            get;
        }

        /// <summary>
        /// Gets an OLE-compatible connection string.
        /// This is needed by Epi Map, as ESRI does not understand .NET connection strings.
        /// See http://www.ConnectionStrings.com for an OLE-compatible connection string for
        /// your database.
        /// </summary>
        public abstract string OleConnectionString
        {
            get;
        }

        /// <summary>
        /// What is this?
        /// </summary>
        public abstract string DataSource
        {
            get;
        }

        /// <summary>
        /// Gets a user-friendly description of the otherwise bewildering connection string
        /// </summary>
        public abstract string ConnectionDescription
        {
            get;
        }
        #endregion Public Properties

        #region Public Methods
        /// <summary>
        /// Gets whether or not the database format is valid. Used for error checking on file-based database types.
        /// </summary>
        /// <param name="validationMessage">Any associated validation error messages</param>
        /// <returns>bool</returns>
        public virtual bool IsDatabaseFormatValid(ref string validationMessage)
        {
            return true;
        }

        /// <summary>
        /// Change the data type of the column in current database
        /// </summary>
        /// <param name="tableName">name of the table</param>
        /// <param name="columnName">name of the column</param>
        /// <param name="newColumnType">new data type of the column</param>
        /// <returns>Boolean</returns>
        public abstract bool AlterColumnType(string tableName, string columnName, string newColumnType);

        /// <summary>
        /// Adds a column to the table
        /// </summary>
        /// <param name="tableName">name of the table</param>
        /// <param name="column">The column</param>
        /// <returns>Boolean</returns>
        public abstract bool AddColumn(string tableName, TableColumn column);

        /// <summary>
        /// Test database connectivity
        /// </summary>
        /// <returns>Returns true if connection can be made successfully</returns>
        public abstract bool TestConnection();

        /// <summary>
        /// Creates a table with the given columns
        /// </summary>
        /// <param name="tableName">The table to be created</param>
        /// <param name="columns">List of columns</param>
        public abstract void CreateTable(string tableName, List<TableColumn> columns);

        ///// <summary>
        ///// Gets a database engine specific connection string builder dialog for a database that already exists.
        ///// </summary>
        ///// <returns>IConnectionStringBuilder</returns>
        //public abstract IConnectionStringGui GetConnectionStringGuiForExistingDb();

        ///// <summary>
        ///// Gets a database engine specific connection string builder dialog for a database that does not exist yet.
        ///// </summary>
        ///// <returns>IConnectionStringBuilder</returns>
        //public abstract IConnectionStringGui GetConnectionStringGuiForNewDb();

        ///// <summary>
        ///// Gets a database engine specific connection
        ///// </summary>
        ///// <param name="fileName">File name and path of the project</param>
        ///// <returns>IConnectionStringBuilder</returns>
        //public abstract ConnectionStringInfo RequestNewConnection(string fileName);

        ///// <summary>
        ///// Creates a physical database
        ///// </summary>
        //public abstract void CreateDatabase(string databaseName);

        /// <summary>
        /// GetTableSchema()
        /// </summary>
        /// <returns>Table Schema</returns>
        public abstract DataSets.TableSchema.TablesDataTable GetTableSchema();

        /// <summary>
        /// Returns the count of tables
        /// </summary>
        /// <returns>int</returns>
        public abstract int GetTableCount();

        /// <summary>
        /// Return the number of colums in the specified table
        /// </summary>
        /// <remarks>
        /// Originaly intended to be used to keep view tables from getting to wide.
        /// </remarks>
        /// <param name="tableName"></param>
        /// <returns>the number of columns in the </returns>
        public abstract int GetTableColumnCount(string tableName);

        /// <summary>
        /// Executes a sql query to select records into a data table
        /// </summary>
        /// <param name="selectQuery"></param>
        /// <returns></returns>
        public abstract DataTable Select(Query selectQuery);

        /// <summary>
        /// Executes a SELECT statement against the database and returns a disconnected data table. NOTE: Use this overload to work with Typed DataSets.
        /// </summary>
        /// <param name="selectQuery">The query to be executed against the database</param>
        /// <param name="table">Table that will contain the result</param>
        /// <returns>A DataTable containing the results of the query</returns>
        public abstract DataTable Select(Query selectQuery, DataTable table);

        /// <summary>
        /// Gets a value indicating whether or not a specific column exists for a table in the database
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <param name="columnName">Name of the column</param>
        /// <returns>Boolean</returns>
        public abstract bool ColumnExists(string tableName, string columnName);

        /// <summary>
        /// Compact the database
        /// << may only apply to Access databases >>
        /// </summary>
        public abstract bool CompactDatabase();
        
        /// <summary>
        /// Executes a SQL statement that does not return anything. 
        /// </summary>
        /// <param name="nonQueryStatement">The query to be executed against the database</param>
        public abstract int ExecuteNonQuery(Query nonQueryStatement);

        /// <summary>
        /// Executes a SQL non-query within a transaction. 
        /// </summary>
        /// <param name="nonQueryStatement">The query to be executed against the database</param>
        /// <param name="transaction">The transaction object</param>
        public abstract int ExecuteNonQuery(Query nonQueryStatement, IDbTransaction transaction);

        /// <summary>
        /// Executes a scalar query against the database
        /// </summary>
        /// <param name="scalarStatement">The query to be executed against the database</param>
        /// <returns>object</returns>
        public abstract object ExecuteScalar(Query scalarStatement);

        /// <summary>
        /// Executes a scalar query against the database using an existing transaction
        /// </summary>
        /// <param name="scalarStatement">The query to be executed against the database</param>
        /// <param name="transaction">The existing transaction within which to execute</param>
        /// <returns>object</returns>
        public abstract object ExecuteScalar(Query scalarStatement, IDbTransaction transaction);

        /// <summary>
        /// Delete a specific table in current database
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>Boolean</returns>
        public abstract bool DeleteTable(string tableName);

        /// <summary>
        /// Delete a specific column in the database
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <param name="columnName">Name of the column</param>
        /// <returns>Boolean</returns>
        public abstract bool DeleteColumn(string tableName, string columnName);

        /// <summary>
        /// Gets a value indicating whether or not a specific table exists in the database
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>Boolean</returns>
        public abstract bool TableExists(string tableName);

        /// <summary>
        /// Gets primary_key schema information about an OLE table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>DataTable with schema information</returns>
        public abstract DataSets.TableColumnSchema.ColumnsDataTable GetTableColumnSchema(string tableName);


        /// <summary>
        /// Gets primary_key schema information about an OLE table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>DataTable with schema information</returns>
        public abstract DataSets.ANSI.TableColumnSchema.ColumnsDataTable GetTableColumnSchemaANSI(string tableName);

        /// <summary>
        /// Gets Primary_Keys schema information about an OLE table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>DataTable with schema information</returns>
        public abstract DataSets.TableKeysSchema.Primary_KeysDataTable GetTableKeysSchema(string tableName);

        /// <summary>
        /// Gets the contents of a table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>Datatable containing the table data</returns>
        public abstract DataTable GetTableData(string tableName);

        /// <summary>
        /// Returns contents of a table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnNames">
        /// Comma delimited string of column names and ASC/DESC order
        /// </param>
        /// <returns>DataTable that has been sorted by criteria specified</returns>
        public abstract DataTable GetTableData(string tableName, string columnNames);

        /// <summary>
        /// Returns contents of a table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnNames"></param>
        /// <param name="sortCriteria">
        /// Comma delimited string of column names and ASC/DESC order
        /// </param>
        /// <returns>Ordered DataTable</returns>
        public abstract DataTable GetTableData(string tableName, string columnNames, string sortCriteria);

        /// <summary>
        /// Returns contents of a table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnNames">List of column names to select. Column names should not be bracketed; this method will add brackets.</param>
        /// <returns>DataTable</returns>
        public virtual DataTable GetTableData(string tableName, List<string> columnNames)
        {
            WordBuilder wb = new WordBuilder(",");
            foreach (string s in columnNames)
            {                
                wb.Add(this.InsertInEscape(s));
            }
            return GetTableData(tableName, wb.ToString(), string.Empty);
        }

        /// <summary>
        /// Returns contents of a table with only the top two rows.
        /// </summary>
        /// <param name="tableName">The name of the table to query</param>
        /// <returns>DataTable</returns>
        public abstract DataTable GetTopTwoTable(string tableName);

        /// <summary>
        /// Create a DataReader on the specified table
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>An instance of an object that implements IDataReader</returns>
        public abstract IDataReader GetTableDataReader(string tableName);

        /// <summary>
        /// Create a DataReader on the specified table
        /// </summary>
        /// <param name="selectQuery"></param>
        /// <param name="commandBehavior"></param>
        /// <returns>An instance of an object that implements IDataReader</returns>
        public abstract IDataReader ExecuteReader(Query selectQuery, CommandBehavior commandBehavior);

        /// <summary>
        /// Create a DataReader on the specified table
        /// </summary>
        /// <param name="selectQuery"></param>
        /// <returns>An instance of an object that implements IDataReader</returns>
        public abstract IDataReader ExecuteReader(Query selectQuery);

        /// <summary>
        /// return the column names of the specified table as a generic List&lt;string&gt;
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>Generic list of column names</returns>
        public abstract List<string> GetTableColumnNames(string tableName);

        public abstract Dictionary<string, int> GetTableColumnNameTypePairs(string tableName);

        /// <summary>
        /// Returns a DataView that contains the names of columns that have a dtatype of "text"
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>DataView</returns>
        public abstract DataView GetTextColumnNames(string tableName);

        /// <summary>
        /// Begins a database transaction
        /// </summary>
        /// <returns>A specialized transaction object based on the current database engine type</returns>
        public abstract IDbTransaction OpenTransaction();

        /// <summary>
        /// Begins a database transaction
        /// </summary>
        /// <param name="isolationLevel">The transaction locking behavior for the connection</param>
        /// <returns>A specialized transaction object based on the current database engine type</returns>
        public abstract IDbTransaction OpenTransaction(IsolationLevel isolationLevel);

        /// <summary>
        /// Closes a database transaction connection. Developer should commit or rollback transaction prior to calling this method.
        /// </summary>
        /// <returns>A specialized transaction object based on the current database engine type</returns>
        public abstract void CloseTransaction(IDbTransaction transaction);

        /// <summary>
        /// TODO: Add method description here
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="tableName"></param>
        /// <param name="insertQuery"></param>
        /// <param name="updateQuery"></param>
        public abstract void Update(DataTable dataTable, string tableName, Query insertQuery, Query updateQuery);

        /// <summary>
        /// Updates the GUIDs of a child table with those of the parent via a uniquekey/fkey relationship
        /// </summary>        
        public abstract void UpdateGUIDs(string childTableName, string parentTableName);

        /// <summary>
        /// Updates the foreign and unique keys of a child table with those of the parent via the original keys that existed prior to an import from an Epi Info 3.5.x project.
        /// </summary>        
        public abstract void UpdateKeys(string childTableName, string parentTableName);

        /// <summary>
        /// Disposes the object
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Gets the names of all tables in the database
        /// </summary>
        /// <returns>Names of all tables in the database</returns>
        public abstract List<string> GetTableNames();

        /// <summary>
        /// Query class abstract factory which returns different type of query instance for different type of database
        /// </summary>
        /// <param name="ansiSqlStatement"></param>
        /// <returns></returns>
        public abstract Query CreateQuery(string ansiSqlStatement);
        #endregion Public Methods

        #region IDbDriver Members

        /// <summary>
        /// Determines the level of rights the user has on the SQL database
        /// </summary>
        public virtual ProjectPermissions GetPermissions()
        {
            return new ProjectPermissions();
        }

        /// <summary>
        /// Get the code table names for the project
        /// </summary>
        /// <param name="project">The project</param>
        /// <returns>DataTable of code table names</returns>
        public abstract DataTable GetCodeTableNamesForProject(Project project);

        /// <summary>
        /// Get the code table list
        /// </summary>
        /// <param name="db">IDbDriver</param>
        /// <returns>Epi.DataSets.TableSchema.TablesDataTable</returns>
        public abstract Epi.DataSets.TableSchema.TablesDataTable GetCodeTableList(IDbDriver db);

        /// <summary>
        /// Determine the type of database in use.
        /// Warning: This is not the ideal OO way of handling this.  Once drivers are updated, this may be removed.
        /// </summary>
        /// <returns>String representation of the database type</returns>
        public abstract string IdentifyDatabase();

        /// <summary>
        /// Inserts the string in escape characters. [] for SQL server and `` for MySQL etc.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public abstract string InsertInEscape(string str);

        /// <summary>
        /// Inserts all strings in the list in escape sequence
        /// </summary>
        /// <param name="strings"></param>
        /// <returns></returns>
        public List<string> InsertInEscape(List<string> strings)
        {
            List<string> newList = new List<string>();
            foreach (string str in strings)
            {
                newList.Add(InsertInEscape(str));
            }
            return newList;
        }

        /// <summary>
        /// Provides a database frieldly string representation of date.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public virtual string FormatDate(DateTime dt)
        {
            return Util.InsertInSingleQuotes(dt.ToShortDateString());
        }

        /// <summary>
        /// Provides a database frieldly string representation of date.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public virtual string FormatDateTime(DateTime dt)
        {
            return Util.InsertInSingleQuotes(dt.ToString());
        }

        /// <summary>
        /// Provides a database frieldly string representation of date.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public virtual string FormatTime(DateTime dt)
        {
            return Util.InsertInSingleQuotes(dt.ToString());
        }

        /// <summary>
        /// Returns database sepecific column type
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public abstract string GetDbSpecificColumnType(GenericDbColumnType dataType);
        
        public abstract string SyntaxTrue
        {
            get;
        }

        public abstract string SyntaxFalse
        {
            get;
        }

        public abstract System.Data.Common.DbDataAdapter GetDbAdapter(string p);

        public abstract System.Data.IDbConnection GetConnection();

        public abstract System.Data.Common.DbCommand GetCommand(string pKeyString, DataTable pDataTable);

        public abstract System.Data.Common.DbCommandBuilder GetDbCommandBuilder(System.Data.Common.DbDataAdapter Adapter);

        public abstract bool InsertBulkRows(string pSelectSQL, System.Data.Common.DbDataReader pDataReader, SetGadgetStatusHandler pStatusDelegate = null, CheckForCancellationHandler pCancellationDelegate = null);
        public abstract bool Insert_1_Row(string pSelectSQL, System.Data.Common.DbDataReader pDataReader);

        public abstract bool Update_1_Row(string pSelectSQL, string pKeyString, System.Data.Common.DbDataReader pDataReader);

        public abstract bool CheckDatabaseTableExistance(string pFileString, string pTableName, bool pIsConnectionString = false);
        public abstract bool CreateDataBase(string pFileString);
        public abstract bool CheckDatabaseExistance(string pFileString, string pTableName, bool pIsConnectionString = false);

        #endregion
    }
}
