using System;
using System.Data;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

namespace Epi.Data
{
    /// <summary>
    /// Abstract data type for databases
    /// </summary>
    public interface IDbDriver
    {
        #region Properties

        /// <summary>
        /// Gets the default schema prefix if applicable (ex. dbo for SQL Server)
        /// </summary>
        string SchemaPrefix
        {
            get;
        }

        /// <summary>
        /// Property to specify if the same operation will be called multiple times. Data driver should use this to optimize code.
        /// </summary>
        bool IsBulkOperation
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the full name of the data source. Typically used for display purposes
        /// </summary>
        string FullName
        {
            get;
        }

        /// <summary>
        /// Gets/sets the Database name
        /// </summary>
        string DbName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the database connection string
        /// </summary>
        string ConnectionString
        {
            get;
            set;
        }

        /// <summary>
        /// Gets an OLE-compatible connection string.
        /// This is needed by Epi Map, as ESRI does not understand .NET connection strings.
        /// See http://www.ConnectionStrings.com for an OLE-compatible connection string for
        /// your database.
        /// </summary>
        string OleConnectionString
        {
            get;
        }

        /// <summary>
        /// What is this?
        /// </summary>
        string DataSource
        {
            get;
        }

        /// <summary>
        /// Gets a user-friendly description of the otherwise bewildering connection string
        /// </summary>
        string ConnectionDescription
        {
            get;
        }

        /// <summary>
        /// Returns the maximum number of columns a table can have.
        /// </summary>
        int TableColumnMax
        {
            get;
        }

        string SyntaxTrue
        {
            get;
        }

        string SyntaxFalse
        {
            get;
        }

        #endregion Properties

        #region Methods
        /// <summary>
        /// Gets whether or not the database format is valid. Used for error checking on file-based database types.
        /// </summary>
        /// <param name="validationMessage">Any associated validation error messages</param>
        /// <returns>bool</returns>
        bool IsDatabaseFormatValid(ref string validationMessage);

        /// <summary>
        /// Gets permissions on this database
        /// </summary>
        /// <returns>ProjectPermissions</returns>
        ProjectPermissions GetPermissions();

        /// <summary>
        /// Test database connectivity
        /// </summary>
        /// <returns>Returns true if connection can be made successfully</returns>
        bool TestConnection();

        /// <summary>
        /// Creates a table with the given columns
        /// </summary>
        /// <param name="tableName">The name of the table to create</param>
        /// <param name="columns">List of columns</param>
        void CreateTable(string tableName, List<TableColumn> columns);

        /// <summary>
        /// Gets the names of all tables in the database
        /// </summary>
        /// <returns>Names of all tables in the database</returns>
        List<string> GetTableNames();

        /// <summary>
        /// GetTableSchema
        /// </summary>
        /// <returns>Table Schema</returns>
        DataSets.TableSchema.TablesDataTable GetTableSchema();

        /// <summary>
        /// Gets the number of tables in the database
        /// </summary>
        /// <returns>The number of tables in the database</returns>
        int GetTableCount();

        /// <summary>
        /// Return the number of colums in the specified table
        /// </summary>
        /// <remarks>
        /// Originaly intended to be used to keep view tables from getting to wide.
        /// </remarks>
        /// <param name="tableName"></param>
        /// <returns>the number of columns in the </returns>
        int GetTableColumnCount(string tableName);

        /// <summary>
        /// Change the data type of the column in current database
        /// </summary>
        /// <param name="tableName">name of the table</param>
        /// <param name="columnName">name of the column</param>
        /// <param name="columnType">new data type of the column</param>
        /// <returns>Boolean</returns>
        bool AlterColumnType(string tableName, string columnName, string columnType);

        /// <summary>
        /// Adds a column to the table
        /// </summary>
        /// <param name="tableName">name of the table</param>
        /// <param name="column">The column</param>
        /// <returns>Boolean</returns>
        bool AddColumn(string tableName, TableColumn column);
        
        /// <summary>
        /// Executes a sql query to select records into a data table
        /// </summary>
        /// <param name="selectQuery"></param>
        /// <returns></returns>
        DataTable Select(Query selectQuery);

        /// <summary>
        /// Executes a SELECT statement against the database and returns a disconnected data table. NOTE: Use this overload to work with Typed DataSets.
        /// </summary>
        /// <param name="selectQuery">The query to be executed against the database</param>
        /// <param name="table">Table that will contain the result</param>
        /// <returns>A data table object</returns>
        DataTable Select(Query selectQuery, DataTable table);

        /// <summary>
        /// Gets a value indicating whether or not a specific column exists for a table in the database
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <param name="columnName">Name of the column</param>
        /// <returns>Boolean</returns>
        bool ColumnExists(string tableName, string columnName);

        /// <summary>
        /// Compact the database
        /// Note: May only apply to Access databases
        /// </summary>
        /// <returns>returns false on error</returns>
        bool CompactDatabase();
        
        /// <summary>
        /// Executes a SQL statement that does not return any records. 
        /// Returns the number of records affected
        /// </summary>
        /// <param name="nonQueryStatement">The query to be executed against the database</param>
        int ExecuteNonQuery(Query nonQueryStatement);

        /// <summary>
        /// Executes a SQL non-query within a transaction. 
        /// </summary>
        /// <param name="nonQueryStatement">The query to be executed against the database</param>
        /// <param name="transaction">The transaction through which to execute the non query</param>
        int ExecuteNonQuery(Query nonQueryStatement, IDbTransaction transaction);

        /// <summary>
        /// Executes a scalar query against the database
        /// </summary>
        /// <param name="scalarStatement">The query to be executed against the database</param>
        /// <returns></returns>
        object ExecuteScalar(Query scalarStatement);

        /// <summary>
        /// Executes a scalar query against the database using an existing transaction
        /// </summary>
        /// <param name="scalarStatement">The query to be executed against the database</param>
        /// <param name="transaction">The transaction through which to execute the scalar query</param>
        /// <returns></returns>
        object ExecuteScalar(Query scalarStatement, IDbTransaction transaction);

        /// <summary>
        /// Delete a specific table in current database
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>Boolean</returns>
        bool DeleteTable(string tableName);

        /// <summary>
        /// Delete a specific column in current database
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <param name="columnName">Name of the column</param>
        /// <returns>Boolean</returns>
        bool DeleteColumn(string tableName, string columnName);

        /// <summary>
        /// Gets a value indicating whether or not a specific table exists in the database
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>Boolean</returns>
        bool TableExists(string tableName);

        /// <summary>
        /// Gets primary_key schema information about an OLE table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>DataTable with schema information</returns>
        DataSets.TableColumnSchema.ColumnsDataTable GetTableColumnSchema(string tableName);

        /// <summary>
        /// Gets primary_key schema information about an OLE table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>DataTable with schema information</returns>
        DataSets.ANSI.TableColumnSchema.ColumnsDataTable GetTableColumnSchemaANSI(string tableName);

        /// <summary>
        /// Gets Primary_Keys schema information about an OLE table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>DataTable with schema information</returns>
        DataSets.TableKeysSchema.Primary_KeysDataTable GetTableKeysSchema(string tableName);

        /// <summary>
        /// Gets the contents of a table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>Datatable containing the table data</returns>
        DataTable GetTableData(string tableName);

        /// <summary>
        /// Returns contents of a table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnNames">Comma delimited string of column names and ASC/DESC order</param>
        /// <returns></returns>
        DataTable GetTableData(string tableName, string columnNames);

        /// <summary>
        /// Returns contents of a table.
        /// </summary>
        /// <param name="tableName">The name of the table to query</param>
        /// <param name="columnNames">Comma delimited string of column names and ASC/DESC order</param>
        /// <param name="sortCriteria">The criteria to sort by</param>
        /// <returns></returns>
        DataTable GetTableData(string tableName, string columnNames, string sortCriteria);

        /// <summary>
        /// Returns contents of a table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnNames">List of column names to select. Column names should not be bracketed; this method will add brackets.</param>
        /// <returns>DataTable</returns>
        DataTable GetTableData(string tableName, List<string> columnNames);

        /// <summary>
        /// Returns contents of a table with only the top two rows.
        /// </summary>
        /// <param name="tableName">The name of the table to query</param>
        /// <returns>DataTable</returns>
        DataTable GetTopTwoTable(string tableName);

        /// <summary>
        /// TODO: Add method description here.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        IDataReader GetTableDataReader(string tableName);

        /// <summary>
        /// TODO: Add method description here
        /// </summary>
        /// <param name="selectQuery"></param>
        /// <returns></returns>
        IDataReader ExecuteReader(Query selectQuery, CommandBehavior commandBehavior);

        /// <summary>
        /// TODO: Add method description here
        /// </summary>
        /// <param name="selectQuery"></param>
        /// <returns></returns>
        IDataReader ExecuteReader(Query selectQuery);

        /// <summary>
        /// TODO: Add method description here
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        List<string> GetTableColumnNames(string tableName);

        /// <summary>
        /// TODO: Add method description here
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        Dictionary<string, int> GetTableColumnNameTypePairs(string tableName);

        /// <summary>
        /// TODO: Add method description here
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        DataView GetTextColumnNames(string tableName);

        /// <summary>
        /// Begins a database transaction
        /// </summary>
        /// <returns>A specialized transaction object based on the current database engine type</returns>
        IDbTransaction OpenTransaction();

        /// <summary>
        /// Begins a database transaction
        /// </summary>
        /// <param name="isolationLevel">The transaction locking behavior for the connection</param>
        /// <returns>A specialized transaction object based on the current database engine type</returns>
        IDbTransaction OpenTransaction(IsolationLevel isolationLevel);

        /// <summary>
        /// Closes a database transaction connection. Developer should commit or rollback transaction prior to calling this method.
        /// </summary>
        /// <returns>A specialized transaction object based on the current database engine type</returns>
        void CloseTransaction(IDbTransaction transaction);

        /// <summary>
        /// TODO: Add method description here
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="tableName"></param>
        /// <param name="insertQuery"></param>
        /// <param name="updateQuery"></param>
        void Update(DataTable dataTable, string tableName, Query insertQuery, Query updateQuery);

        /// <summary>
        /// Updates the GUIDs of a child table with those of the parent via a uniquekey/fkey relationship
        /// </summary>        
        void UpdateGUIDs(string childTableName, string parentTableName);

        /// <summary>
        /// Updates the foreign and unique keys of a child table with those of the parent via the original keys that existed prior to an import from an Epi Info 3.5.x project.
        /// </summary>        
        void UpdateKeys(string childTableName, string parentTableName);

        /// <summary>
        /// Disposes the object
        /// </summary>
        void Dispose();

        /// <summary>
        /// Creates an ANSI SQL-92 statement container
        /// </summary>
        /// <param name="ansiSqlStatement">A SQL Query following ANSI SQL-92 standards</param>
        /// <returns>A Query object</returns>
        Query CreateQuery(string ansiSqlStatement);

        /// <summary>
        /// Get a list of code table names for a given project
        /// </summary>
        /// <param name="project">Project</param>
        /// <returns>DataTable</returns>
        DataTable GetCodeTableNamesForProject(Project project);

        /// <summary>
        /// Get a list of code tables for a project
        /// </summary>
        /// <param name="db">IDbDriver db</param>
        /// <returns>DataTable</returns>
        DataSets.TableSchema.TablesDataTable GetCodeTableList(IDbDriver db);

        /// <summary>
        /// Identify the database in use.  Used for code table management.
        /// </summary>
        /// <returns></returns>
        string IdentifyDatabase();

        /// <summary>
        /// Returns Db specific column type
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        string GetDbSpecificColumnType(GenericDbColumnType dataType);

        /// <summary>
        /// Inserts the string in escape characters. [] for SQL server and `` for MySQL etc.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string InsertInEscape(string str);

        /// <summary>
        /// Inserts all strings in the list in escape characters.
        /// </summary>
        /// <param name="strings"></param>
        /// <returns></returns>
        List<string> InsertInEscape(List<string> strings);

        /// <summary>
        /// Provides a database frieldly string representation of date.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        string FormatDate(DateTime dt);

        /// <summary>
        /// Provides a database friendly string representation of date and time
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        string FormatDateTime(DateTime dt);

        /// <summary>
        /// Provides a database friendly string representation of time
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        string FormatTime(DateTime time);

        System.Data.Common.DbDataAdapter GetDbAdapter(string p);
        System.Data.IDbConnection GetConnection();
        System.Data.Common.DbCommand GetCommand(string pKeyString, DataTable pDataTable);
        System.Data.Common.DbCommandBuilder GetDbCommandBuilder(System.Data.Common.DbDataAdapter Adapter);
        bool InsertBulkRows(string pSelectSQL, System.Data.Common.DbDataReader pDataReader, SetGadgetStatusHandler pStatusDelegate = null, CheckForCancellationHandler pCancellationDelegate = null);
        bool Insert_1_Row(string pSelectSQL, System.Data.Common.DbDataReader pDataReader);
        bool Update_1_Row(string pSelectSQL, string pKeyString, System.Data.Common.DbDataReader pDataReader);
        bool CheckDatabaseTableExistance(string pFileString, string pTableName, bool pIsConnectionString = false);
        bool CreateDataBase(string pFileString);
        bool CheckDatabaseExistance(string pFileString, string pTableName, bool pIsConnectionString = false);
        #endregion Methods


    }
}