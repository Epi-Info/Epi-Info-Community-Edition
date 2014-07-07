using System;
using System.Data.SqlClient;
using System.IO;
using System.Xml;
using Epi.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace Epi.Data.SqlServer
{
    /// <summary>
    /// Concret SQL Server Database Class
    /// </summary>
    public partial class SqlDatabase : DbDriverBase
    {
        private bool isBulkOperation;

        /// <summary>
        /// SQL Database Constructor
        /// </summary>
        public SqlDatabase() : base() { }

        #region Native Driver Implementation
        /// <summary>
        /// Returns a native equivalent of a DbParameter
        /// </summary>
        /// <returns>A native equivalent of a DbParameter</returns>
        protected virtual SqlParameter ConvertToNativeParameter(QueryParameter parameter)
        {
            if(parameter.DbType.Equals(DbType.Guid))
            {
                parameter.Value = new Guid(parameter.Value.ToString());
            }

            return new SqlParameter(parameter.ParameterName, CovertToNativeDbType(parameter.DbType), parameter.Size, parameter.Direction, parameter.IsNullable, parameter.Precision, parameter.Scale, parameter.SourceColumn, parameter.SourceVersion, parameter.Value);
        }

        /// <summary>
        /// Gets a native connection instance based on current ConnectionString value
        /// </summary>
        /// <returns>Native connection object</returns>
        protected virtual SqlConnection GetNativeConnection()
        {
            return GetNativeConnection(connectionString);
        }

        /// <summary>
        /// Gets a native connection instance from supplied connection string
        /// </summary>
        /// <returns>Native connection object</returns>
        protected virtual SqlConnection GetNativeConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        /// <summary>
        /// Returns a native command object
        /// </summary>
        /// <param name="transaction">Null is not allowed. </param>
        /// <returns></returns>
        protected SqlCommand GetNativeCommand(IDbTransaction transaction)
        {
            SqlTransaction oleDbtransaction = transaction as SqlTransaction;

            #region Input Validation
            if (oleDbtransaction == null)
            {
                throw new ArgumentException("Transaction parameter must be a SqlTransaction.", "transaction");
            }
            #endregion

            return new SqlCommand(null, (SqlConnection)transaction.Connection, (SqlTransaction)transaction);

        }

        /// <summary>
        /// Executes a SELECT statement against the database and returns a disconnected data table. NOTE: Use this overload to work with Typed DataSets.
        /// </summary>
        /// <param name="selectQuery">The query to be executed against the database</param>
        /// <param name="dataTable">Table that will contain the result</param>
        /// <returns>A data table object</returns>
        public override DataTable Select(Query selectQuery, DataTable dataTable)
        {
            #region Input Validation
            if (selectQuery == null)
            {
                throw new ArgumentNullException("selectQuery");
            }
            if (dataTable == null)
            {
                throw new ArgumentNullException("dataTable");
            }
            #endregion Input Validation


            IDbConnection connection = GetConnection();
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.SelectCommand = (SqlCommand)GetCommand(selectQuery.SqlStatement, connection, selectQuery.Parameters);
            adapter.SelectCommand.CommandTimeout = 1500;

            try
            {

                adapter.Fill(dataTable);                
                try
                {
                    adapter.FillSchema(dataTable, SchemaType.Source);
                }
                catch (ArgumentException ex)
                {
                    // do nothing
                }
                return dataTable;
            }
            //SqlException being caught to handle denied permissions for SELECT, but other 
            //  exceptions may occur and will need to be handled.  -den4  11/23/2010
            catch (System.Data.SqlClient.SqlException ex)
            {
                throw new System.ApplicationException("Error executing select query against the database.", ex);
                /*Epi.Windows.MsgBox.Show("You may not have permission to access the database. \n\n" +
                    ex.Message,
                    "Permission Denied",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
                return dataTable;*/
            }
            catch (Exception ex)
            {
                throw new System.ApplicationException("Error executing select query against the database.", ex);
            }
        }

        /// <summary>
        /// Warning! This method does not support transactions!
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="tableName"></param>
        /// <param name="insertQuery"></param>
        /// <param name="updateQuery"></param>
        public override void Update(DataTable dataTable, string tableName, Query insertQuery, Query updateQuery)
        {

            #region Input Validation

            if (dataTable == null)
            {
                throw new ArgumentNullException("DataTable");
            }
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("TableName");
            }
            if (insertQuery == null)
            {
                throw new ArgumentNullException("InsertQuery");
            }
            if (updateQuery == null)
            {
                throw new ArgumentNullException("UpdateQuery");
            }
            #endregion Input Validation

            IDbConnection connection = GetConnection();
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.InsertCommand = (SqlCommand)GetCommand(insertQuery.SqlStatement, connection, insertQuery.Parameters);
            adapter.UpdateCommand = (SqlCommand)GetCommand(updateQuery.SqlStatement, connection, updateQuery.Parameters);
            adapter.InsertCommand.CommandTimeout = 1500;
            adapter.UpdateCommand.CommandTimeout = 1500;
            //Logger.Log(insertQuery);
            //Logger.Log(updateQuery);
            try
            {
                adapter.Update(dataTable);
            }
            catch (Exception ex)
            {
                throw new System.ApplicationException("Error updating data.", ex);
            }
        }

        /// <summary>
        /// Updates the GUIDs of a child table with those of the parent via a uniquekey/fkey relationship
        /// </summary>        
        public override void UpdateGUIDs(string childTableName, string parentTableName)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("UPDATE ");
            sb.Append(childTableName);
            sb.Append(" SET FKEY = ");
            sb.Append(parentTableName);
            sb.Append(".GlobalRecordId from ");
            sb.Append(childTableName);
            sb.Append(", ");
            sb.Append(parentTableName);
            sb.Append(" where ");
            sb.Append(childTableName);
            sb.Append(".OldFKEY = ");
            sb.Append(parentTableName);
            sb.Append(".OldUniqueKey");

            ExecuteNonQuery(CreateQuery(sb.ToString()));
        }

        /// <summary>
        /// Formats a table name to comply with SQL syntax
        /// </summary>
        /// <param name="tableName">The table name to format</param>
        /// <returns>string representing the formatted table name</returns>
        private string FormatTableName(string tableName)
        {
            string formattedTableName = string.Empty;

            if (tableName.Contains("."))
            {   
                string[] parts = tableName.Split('.');

                foreach (string part in parts)
                {
                    formattedTableName = formattedTableName + InsertInEscape(part) + ".";
                }

                formattedTableName = formattedTableName.TrimEnd('.');
            }
            else
            {
                formattedTableName = InsertInEscape(tableName);
            }
            return formattedTableName;
        }

        /// <summary>
        /// Updates the foreign and unique keys of a child table with those of the parent via the original keys that existed prior to an import from an Epi Info 3.5.x project.
        /// </summary>        
        public override void UpdateKeys(string childTableName, string parentTableName)
        {
            StringBuilder sb = new StringBuilder();

            //sb.Append("UPDATE ");
            //sb.Append(childTableName);
            //sb.Append(" SET FKEY = ");
            //sb.Append(parentTableName);
            //sb.Append(".UniqueKey from ");
            //sb.Append(childTableName);
            //sb.Append(", ");
            //sb.Append(parentTableName);
            //sb.Append(" where ");
            //sb.Append(childTableName);
            //sb.Append(".OldFKEY = ");
            //sb.Append(parentTableName);
            //sb.Append(".OldUniqueKey");

            //ExecuteNonQuery(CreateQuery(sb.ToString()));
        }

        #endregion

        #region Temporary junk pile
        /// <summary>
        /// Gets or sets the Database name
        /// </summary>
        public override string DbName // Implements IDbDriver.DbName
        {
            get
            {
                return dbName;
            }
            set
            {
                dbName = value;
            }
        }  private string dbName = string.Empty;


        /// <summary>
        /// Returns the full name of the data source
        /// </summary>
        public override string FullName //Implements DataSource.FullName
        {
            get
            {
                return DataSource + "." + DbName;
            }
        }


        private string connectionString;

        /// <summary>
        /// Gets the database-specific column data type.
        /// </summary>
        /// <param name="dataType">An extension of the System.Data.DbType that adds StringLong (Text, Memo) data type that Epi Info commonly uses.</param>
        /// <returns>Database-specific column data type.</returns>
        public override string GetDbSpecificColumnType(GenericDbColumnType dataType)
        {
            switch (dataType)
            {
                case GenericDbColumnType.AnsiString:
                case GenericDbColumnType.String:
                    return "nvarchar";

                case GenericDbColumnType.AnsiStringFixedLength:
                case GenericDbColumnType.StringFixedLength:
                    return "nchar";

                case GenericDbColumnType.Binary:
                    return "binary";

                case GenericDbColumnType.Boolean:
                    return "bit";

                case GenericDbColumnType.Byte:
                    return "tinyint";

                case GenericDbColumnType.Currency:
                    return "money";

                case GenericDbColumnType.Date:
                case GenericDbColumnType.DateTime:
                case GenericDbColumnType.Time:
                    return "datetime";

                case GenericDbColumnType.Decimal:
                    return "decimal";

                case GenericDbColumnType.Double:
                    return "float";

                case GenericDbColumnType.Guid:
                    return "uniqueidentifier";

                case GenericDbColumnType.Image:
                    return "image";

                case GenericDbColumnType.Int16:
                case GenericDbColumnType.UInt16:
                    return "smallint";

                case GenericDbColumnType.Int32:
                case GenericDbColumnType.UInt32:
                    return "int";

                case GenericDbColumnType.Int64:
                case GenericDbColumnType.UInt64:
                    return "bigint";

                case GenericDbColumnType.Object:
                    return "binary";

                case GenericDbColumnType.SByte:
                    return "tinyint";

                case GenericDbColumnType.Single:
                    return "real";
                
                case GenericDbColumnType.StringLong:
                    return "ntext";
                
                case GenericDbColumnType.VarNumeric:
                    return "decimal";

                case GenericDbColumnType.Xml:
                    return "xml";

                default:
                    throw new GeneralException("genericDbColumnType is unknown");
                    // return "nvarchar";
            }
        }
        /// <summary>
        /// Connection Description Attribute
        /// </summary>
        public override string ConnectionDescription
        {
            get { return "Microsoft SQL Server: " + this.DbName; }
        }
        /// <summary>
        /// Data Source Attribute
        /// </summary>
		public override string DataSource
        {
            get
            {
                SqlConnection sqlconn = GetConnection() as SqlConnection;
                if (sqlconn != null)
                {
                    return sqlconn.DataSource;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Returns the maximum number of columns a table can have.
        /// </summary>
        public override int TableColumnMax
        {
            get { return 1020; }
        }

        /// <summary>
        /// Connection String Attribute
        /// </summary>
        public override string ConnectionString
        {
            get
            {
                return connectionString;
            }
            set
            {
                this.connectionString = value;

                try
                {
                    IDbConnection conn = GetConnection();
                    this.DbName = conn.Database;
                }
                catch
                {
                    this.connectionString = null;
                }
            }
        }

        /// <summary>
        /// Gets an OLE-compatible connection string.
        /// This is needed by Epi Map, as ESRI does not understand .NET connection strings.
        /// </summary>
        public override string OleConnectionString
        {
            get
            {
                if (!string.IsNullOrEmpty(connectionString))
                {
                    return "Provider=sqloledb;" + connectionString;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Change the data type of the column in current database
        /// </summary>
        /// <param name="tableName">name of the table</param>
        /// <param name="columnName">name of the column</param>
        /// <param name="columnType">new data type of the column</param>
        /// <returns>Boolean</returns>
        public override bool AlterColumnType(string tableName, string columnName, string columnType)
        {
            // dpb todo
            return false;
        }

        /// <summary>
        /// Adds a column to the table
        /// </summary>
        /// <param name="tableName">name of the table</param>
        /// <param name="column">The column</param>
        /// <returns>Boolean</returns>
        public override bool AddColumn(string tableName, TableColumn column)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("ALTER TABLE ");
            sb.Append(tableName);
            sb.Append(" ADD ");
            sb.Append(column.Name);
            sb.Append(" ");
            sb.Append(GetDbSpecificColumnType(column.DataType));
            if (column.Length != null)
            {
                sb.Append("(");
                sb.Append(column.Length.Value.ToString());
                sb.Append(") ");
            }            

            ExecuteNonQuery(CreateQuery(sb.ToString()));
            return false;
        }

        
        
        /// <summary>
        /// Gets the names of all tables in the database
        /// </summary>
        /// <returns>Names of all tables in the database</returns>
        public override List<string> GetTableNames()
        {
            List<string> tableNames = new List<string>();
            DataTable schemaTable = GetTableSchema();

            foreach (DataRow row in schemaTable.Rows)
            {
                if (row[ColumnNames.SCHEMA_NAME].ToString().ToLower().Equals("dbo"))
                {
                    tableNames.Add(row[ColumnNames.SCHEMA_TABLE_NAME].ToString());
                }
                else
                {
                    tableNames.Add((row[ColumnNames.SCHEMA_NAME].ToString() + "." + row[ColumnNames.SCHEMA_TABLE_NAME].ToString()));
                }
            }
            return tableNames;
        }

        /// <summary>
        /// Gets the SQL version of a generic DbType
        /// </summary>
        /// <returns>SQL version of the generic DbType</returns>
        private SqlDbType CovertToNativeDbType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                    return SqlDbType.VarChar;
                case DbType.AnsiStringFixedLength:
                    return SqlDbType.Char;
                case DbType.Binary:
                    return SqlDbType.Binary;
                case DbType.Boolean:
                    return SqlDbType.Bit;
                case DbType.Byte:
                    return SqlDbType.TinyInt;
                case DbType.Currency:
                    return SqlDbType.Money;
                case DbType.Date:
                    return SqlDbType.DateTime;
                case DbType.DateTime:
                    return SqlDbType.DateTime;
                case DbType.Decimal:
                    return SqlDbType.Decimal;
                case DbType.Double:
                    return SqlDbType.Float;
                case DbType.Guid:
                    return SqlDbType.UniqueIdentifier;
                case DbType.Int16:
                    return SqlDbType.SmallInt;
                case DbType.Int32:
                    return SqlDbType.Int;
                case DbType.Int64:
                    return SqlDbType.BigInt;
                case DbType.Object:
                    return SqlDbType.Binary;
                case DbType.SByte:
                    return SqlDbType.TinyInt;
                case DbType.Single:
                    return SqlDbType.Real;
                case DbType.String:
                    return SqlDbType.NVarChar;
                case DbType.StringFixedLength:
                    return SqlDbType.NChar;
                case DbType.Time:
                    return SqlDbType.DateTime;
                case DbType.UInt16:
                    return SqlDbType.SmallInt;
                case DbType.UInt32:
                    return SqlDbType.Int;
                case DbType.UInt64:
                    return SqlDbType.BigInt;
                case DbType.VarNumeric:
                    return SqlDbType.Decimal;
                default:
                    return SqlDbType.VarChar;
            }
        }
        #endregion

        #region Schema and DDL Support

        public override string SchemaPrefix
        {
            get
            {
                return "DBO.";
            }
        }

        public override bool IsBulkOperation
        {
            get
            {
                return this.isBulkOperation;
            }
            set
            {
                this.isBulkOperation = value;
            }
        }

        /// <summary>
        /// Get Table Count
        /// </summary>
        /// <returns></returns>
        public override int GetTableCount()
        {
            DataTable dtSchema = GetTableSchema();
            return dtSchema.Rows.Count;
        }


        /// <summary>
        /// Creates a table with the given columns
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns">List of columns</param>
        public override void CreateTable(string tableName, List<TableColumn> columns)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("create table ");

            // force DBO schema if none specified
            if (!tableName.Contains("."))
            {
                sb.Append(SchemaPrefix);
            }

            sb.Append(tableName);
            sb.Append(" ( ");
            foreach (TableColumn column in columns)
            {
                sb.Append(column.Name);
                sb.Append(" ");
                if (GetDbSpecificColumnType(column.DataType).Equals("nvarchar") && column.Length.HasValue && column.Length.Value > 8000)
                {
                    sb.Append("Nvarchar(max)");
                }
                else
                {
                    sb.Append(GetDbSpecificColumnType(column.DataType));
                }
                if (column.Length != null && column.Length.HasValue && column.Length.Value <= 8000 && column.Length.Value > 0)
                {
                    sb.Append("(");
                    sb.Append(column.Length.Value.ToString());
                    sb.Append(") ");
                }

                if (column.AllowNull)
                {
                    sb.Append(" null ");
                }
                else
                {
                    sb.Append(" NOT null ");
                }

                if (column.IsIdentity)
                {
                    sb.Append(" identity ");
                }
                if (column.IsPrimaryKey)
                {
                    sb.Append(" constraint ");
                    sb.Append(" PK_");
                    sb.Append(column.Name);
                    sb.Append("_");
                    sb.Append(tableName);
                    sb.Append(" primary key ");
                }
                if (!string.IsNullOrEmpty(column.ForeignKeyColumnName) && !string.IsNullOrEmpty(column.ForeignKeyTableName))
                {
                    sb.Append(" references ");
                    sb.Append(column.ForeignKeyTableName);
                    sb.Append("(");
                    sb.Append(column.ForeignKeyColumnName);
                    sb.Append(") ");
                    if (column.CascadeDelete)
                    {
                        sb.Append(" on delete cascade");
                    }

                }
                sb.Append(", ");
            }
            sb.Remove(sb.Length - 2, 2);
            sb.Append(") ");

            ExecuteNonQuery(CreateQuery(sb.ToString()));

        }

        /////// <summary>
        /////// 
        /////// </summary>
        /////// <returns></returns>
        ////public bool TestConnection()
        ////{
        ////    //create a SQLConnection object to connect to the database, passing the connection
        ////    //string to the constructor
        ////    SqlConnection mySqlConnection = new SqlConnection(ConnectionString);

        ////    try
        ////    {
        ////        //open  the database connection using the 
        ////        //Open() method of the SqlConnection object
        ////        mySqlConnection.Open();
        ////        return true;
        ////    }
        ////    //			catch(Exception ex)
        ////    //			{
        ////    //				throw new Exception(SharedStrings.CONNECTION_NOT_OPEN + ex.ToString());
        ////    //			}
        ////    finally
        ////    {

        ////        mySqlConnection.Close();
        ////    }
        ////}

        ////public DataTable Select(DbQuery selectQuery)
        ////{
        ////    try
        ////    {

        ////        #region Input Validation
        ////        if (selectQuery == null)
        ////        {
        ////            throw new ArgumentNullException("SelectQuery");
        ////        }
        ////        #endregion

        ////        DataTable table = new DataTable();
        ////        return Select(selectQuery, table);
        ////    }
        ////    //	catch (Exception ex)
        ////    //	{
        ////    //		throw new System.ApplicationException("Error executing query against the database.", ex);
        ////    //	}
        ////    finally
        ////    {
        ////    }
        ////}
        /////// <summary>
        /////// Executes a select statement against the database and returns a disconnected data table. NOTE: Use this overload to work with Typed DataSets.
        /////// </summary>
        /////// <param name="selectQuery">The query to be executed against the database</param>
        /////// <param name="table">Table that will contain the result</param>
        /////// <returns>A data table object</returns>
        ////public DataTable Select(DbQuery selectQuery, DataTable table)
        ////{
        ////    #region Input Validation
        ////    if (selectQuery == null)
        ////    {
        ////        throw new ArgumentNullException("SelectQuery");
        ////    }
        ////    if (table == null)
        ////    {
        ////        throw new ArgumentNullException("Table");
        ////    }
        ////    #endregion Input Validation

        ////    try
        ////    {
        ////        GetSqlDataAdapter(selectQuery.SqlStatement, selectQuery.Parameters).Fill(table);

        ////        return table;
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        throw new System.ApplicationException("Error executing query against the database.", ex);
        ////    }
        ////    finally
        ////    {
        ////    }
        ////}


        ///// <summary>
        ///// Returns contents of a table.
        ///// </summary>
        ///// <param name="tableName"></param>
        ///// <param name="sortCriteria">Comma delimited string of column names and asc/DESC order</param>
        ///// <returns></returns>
        //public DataTable GetTableData(string tableName, string columnNames, string sortCriteria)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(columnNames))
        //        {
        //            columnNames = StringLiterals.STAR;
        //        }
        //        string queryString = "select " + columnNames + " from [" + tableName + "]";
        //        if (!string.IsNullOrEmpty(sortCriteria))
        //        {
        //            queryString += " order by " + sortCriteria;
        //        }
        //        DbQuery query = this.CreateQuery(queryString);
        //        return Select(query);
        //    }
        //    finally
        //    {
        //    }
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="tableName"></param>
        ///// <returns></returns>
        //public IDataReader GetTableDataReader(string tableName)
        //{
        //    DbQuery query = this.CreateQuery("select * from " + tableName);
        //    return this.ExecuteReader(query);
        //}




        ///// <summary>
        ///// Executes a query and returns a stream of rows
        ///// </summary>
        ///// <param name="selectQuery">The query to be executed against the database</param>
        ///// <returns>A data reader object</returns>
        //public IDataReader ExecuteReader(DbQuery selectQuery)
        //{
        //    try
        //    {

        //        #region Input Validation
        //        if (selectQuery == null)
        //        {
        //            throw new ArgumentNullException("SelectQuery");
        //        }
        //        #endregion
        //        return GetSqlCommand(selectQuery.SqlStatement, selectQuery.Parameters).ExecuteReader();
        //        //				return GetSpecializedCommand(selectQuery).ExecuteReader();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new System.ApplicationException("Could not execute reader", ex);
        //    }
        //}

        ///// <summary>
        ///// Executes a SQL statement that does not return anything. NOTE: The connection needs to be openned before entering this method.
        ///// </summary>
        ///// <param name="nonQueryStatement">The query to be executed against the database</param>
        //public int ExecuteNonQuery(DbQuery nonQueryStatement)
        //{

        //    #region Input Validation
        //    if (nonQueryStatement == null)
        //    {
        //        throw new ArgumentNullException("NonQueryStatement");
        //    }
        //    #endregion

        //    IDbCommand command = GetSqlCommand(nonQueryStatement.SqlStatement, nonQueryStatement.Parameters);
        //    int result;
        //    bool selfContained = false;
        //    if (this.sqlConnection.State != ConnectionState.Open)
        //    {
        //        OpenConnection();
        //        selfContained = true;
        //    }
        //    try
        //    {
        //        result = command.ExecuteNonQuery();
        //        return result;
        //    }
        //    catch (System.Data.SqlClient.SqlException ex)
        //    {
        //        throw new SqlException(nonQueryStatement.SqlStatement, ex);
        //    }
        //    finally
        //    {
        //        if (selfContained)
        //        {
        //            CloseConnection();
        //        }
        //    }
        //}

        ///// <summary>
        ///// Executes a SQL non-query within a transaction. NOTE: The connection needs to be openned before entering this method.
        ///// </summary>
        ///// <param name="nonQueryStatement">The query to be executed against the database</param>
        ///// <param name="transaction">The transaction object</param>
        //public int ExecuteNonQuery(DbQuery nonQueryStatement, IDbTransaction transaction)
        //{

        //    #region Input Validation
        //    if (nonQueryStatement == null)
        //    {
        //        throw new ArgumentNullException("NonQueryStatement");
        //    }
        //    if (transaction == null)
        //    {
        //        throw new ArgumentNullException("Transaction");
        //    }
        //    #endregion
        //    int result;
        //    IDbCommand command = GetSqlCommand(nonQueryStatement.SqlStatement, nonQueryStatement.Parameters);
        //    command.Transaction = transaction;
        //    result = command.ExecuteNonQuery();
        //    return result;
        //}


        ///// <summary>
        ///// Executes a scalar query against the database
        ///// </summary>
        ///// <param name="scalarStatement">The query to be executed against the database</param>
        ///// <returns></returns>
        //public object ExecuteScalar(DbQuery scalarStatement)
        //{

        //    #region Input Validation
        //    if (scalarStatement == null)
        //    {
        //        throw new ArgumentNullException("ScalarStatement");
        //    }
        //    #endregion

        //    object result;
        //    IDbCommand command = GetSqlCommand(scalarStatement.SqlStatement, scalarStatement.Parameters);
        //    try
        //    {
        //        OpenConnection(command.Connection);
        //        result = command.ExecuteScalar();
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //    finally
        //    {
        //        CloseConnection(command.Connection);
        //    }

        //    return result;
        //}

        ///// <summary>
        ///// Opens a new connection
        ///// </summary>
        //public void OpenConnection()
        //{
        //    try
        //    {
        //        if (this.sqlConnection.State != ConnectionState.Open)
        //        {
        //            this.sqlConnection.Open();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new System.ApplicationException("Error opening connection.", ex);
        //    }
        //}

        ///// <summary>
        ///// Closes the current connection
        ///// </summary>
        //public void CloseConnection()
        //{
        //    try
        //    {
        //        IDbConnection conn = this.sqlConnection;
        //        if (conn.State != ConnectionState.Closed)
        //        {
        //            conn.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new System.ApplicationException("Error closing connection", ex);
        //    }
        //}

        ///// <summary>
        ///// Begins a database transaction
        ///// </summary>
        ///// <returns>A specialized transaction object based on the current database engine type</returns>
        //public IDbTransaction BeginTransaction()
        //{
        //    return this.sqlConnection.BeginTransaction();
        //}

        ///// <summary>
        ///// Begins a database transaction
        ///// </summary>
        ///// <param name="isolationLevel">The transaction locking behavior for the connection</param>
        ///// <returns>A specialized transaction object based on the current database engine type</returns>
        //public IDbTransaction BeginTransaction(IsolationLevel isolationLevel)
        //{
        //    return this.sqlConnection.BeginTransaction(isolationLevel);
        //}


        /// <summary>
        /// Delete a specific table in the database
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>Boolean</returns>
        public override bool DeleteTable(string tableName)
        {
            Query query = this.CreateQuery("Drop Table " + tableName);
            //QueryParameter qr = new QueryParameter("@Name", DbType.String, tableName);
            return (ExecuteNonQuery(query)>0);
        }

        /// <summary>
        /// Delete a specific column in the database
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <param name="columnName">Name of the column</param>
        /// <returns>Boolean</returns>
        public override bool DeleteColumn(string tableName, string columnName)
        {
            Query query = this.CreateQuery("ALTER TABLE " + tableName + " DROP COLUMN " + columnName);
            return (ExecuteNonQuery(query) > 0);
        }

        /// <summary>
        /// Gets a value indicating whether or not a specific table exists in the database
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>Boolean</returns>
        public override bool TableExists(string tableName)
        {
            try
            {
                if (tableName.Contains("."))
                {
                    string[] splits = tableName.Split('.');
                    tableName = splits[splits.Length - 1];
                }
            }
            catch (Exception ex)
            {
                //
            }
            Query query = this.CreateQuery("select * from SysObjects O where ObjectProperty(O.ID,'IsUserTable')=1 and O.Name=@Name");
            query.Parameters.Add(new QueryParameter("@Name", DbType.String, tableName));
            return (Select(query).Rows.Count > 0);
        }

        /// <summary>
        /// Gets a value indicating whether or not a specific column exists for a table in the database
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <param name="columnName">Name of the column</param>
        /// <returns>Boolean</returns>
        public override bool ColumnExists(string tableName, string columnName)
        {
            Query query = this.CreateQuery("select * from SysObjects O inner join SysColumns C on O.ID=C.ID where ObjectProperty(O.ID,'IsUserTable')=1 and O.Name=@TableName and C.Name=@ColumnName");
            query.Parameters.Add(new QueryParameter("@TableName", DbType.String, tableName));
            query.Parameters.Add(new QueryParameter("@ColumnName", DbType.String, columnName));
            // TODO: Make sure this query returns only one row. If more than one columns is returned, it is an indication the query is wrong.
            return (Select(query).Rows.Count > 0);
        }

        /// <summary>
        /// Compact the database
        /// << may only apply to Access databases >>
        /// </summary>
        public override bool CompactDatabase()
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public override List<string> GetTableColumnNames(string tableName)
        {
            DataTable table = this.GetSchema("COLUMNS", tableName);
            List<string> list = new List<string>();
            foreach (DataRow row in table.Rows)
            {
                list.Add(row["COLUMN_NAME"].ToString());
            }
            return list;
        }

        /// <summary>
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public override Dictionary<string, int> GetTableColumnNameTypePairs(string tableName)
        {
            DataTable table = this.GetSchema("COLUMNS", tableName);
            Dictionary<string, int> dictionary = new Dictionary<string, int>();

            foreach (DataRow row in table.Rows)
            {
                dictionary.Add((string)row["COLUMN_NAME"], (int)row["DATA_TYPE"]);
            }
            return dictionary;
        }

        /// <summary>
        /// Return the number of colums in the specified table
        /// </summary>
        /// <remarks>
        /// Originaly intended to be used to keep view tables from getting to wide.
        /// </remarks>
        /// <param name="tableName"></param>
        /// <returns>the number of columns in the </returns>
        public override int GetTableColumnCount(string tableName)
        {
            DataTable table = this.GetSchema("COLUMNS", tableName);
            return table.Rows.Count;
        }

        /// <summary>
        /// Gets the selected schema of the table specified in tableName
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="tableName"></param>
        /// <returns>DataTable with schema information</returns>
        /// 
        private DataTable GetSchema(string collectionName, string tableName)
        {
            SqlConnection conn = this.GetNativeConnection();

            try
            {
                OpenConnection(conn);
                return conn.GetSchema(collectionName, new string[] { null, null, tableName });
            }
            finally
            {
                CloseConnection(conn);
            }
        }

        /// <summary>
        /// Gets the selected schema of the table specified in tableName
        /// </summary>
        /// <param name="collectionName"></param>
        /// <returns>DataTable with schema information</returns>
        private DataTable GetSchema(string collectionName)
        {
            SqlConnection conn = this.GetNativeConnection();

            try
            {
                OpenConnection(conn);
                return conn.GetSchema(collectionName, new string[] { null, null, null });
            }
            finally
            {
                CloseConnection(conn);
            }
        }

        /// <summary>
        /// Gets table schema information 
        /// </summary>
        /// <returns>DataTable with schema information</returns>
        public override DataSets.TableSchema.TablesDataTable GetTableSchema()
        {
            SqlConnection conn = this.GetNativeConnection();
            try
            {
                OpenConnection(conn);
                DataTable table = conn.GetSchema("Tables", new string[0]);
                DataSets.TableSchema tableSchema = new Epi.DataSets.TableSchema();
                tableSchema.Merge(table);
                return tableSchema._Tables;
            }
            finally
            {
                CloseConnection(conn);
            }
        }

        // dcs0 previous function relied upon OLEDb to get schema from SQL server
        //      coming up with the appropriate connection string proved problematic
        //      this is the Micro$oft recommended solution
        /// <summary>
        /// Gets Primary_Keys schema information about a SQL table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>DataTable with schema information</returns>
        public override DataSets.TableKeysSchema.Primary_KeysDataTable GetTableKeysSchema(string tableName)
        {
            DataSets.TableKeysSchema schema = new Epi.DataSets.TableKeysSchema();
            SqlConnection conn = this.GetNativeConnection();
            bool alreadyOpen = (conn.State != ConnectionState.Closed);
            try
            {
                if (!alreadyOpen)
                {
                    OpenConnection(conn);
                }
                string sql = "select KU.TABLE_CATALOG, KU.TABLE_SCHEMA, KU.TABLE_NAME, COLUMN_NAME, " +
                              "0 as COLUMN_PROPID, ORDINAL_POSITION as ORDINAL, KU.CONSTRAINT_NAME as PK_NAME " +
                              "from INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS TC " +
                              "inner join " +
                              "INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KU " +
                              "on TC.CONSTRAINT_TYPE = 'primary key' and " +
                              "TC.CONSTRAINT_NAME = KU.CONSTRAINT_NAME " +
                              "where KU.TABLE_NAME = '" + tableName +
                              "' order by KU.ORDINAL_POSITION;";
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                da.Fill(schema.Primary_Keys);
                da.Dispose();
            }
            catch (Exception ex)
            {
                throw new GeneralException("Unable to obtain primary keys schema.", ex);
            }
            finally
            {
                if (!alreadyOpen && conn.State != ConnectionState.Closed)
                {
                    CloseConnection(conn);
                }
            }
            return schema.Primary_Keys;
        }

        ///// <summary>
        ///// Gets Primary_Keys schema information about a SQL table
        ///// </summary>
        ///// <param name="tableName">Name of the table</param>
        ///// <returns>DataTable with schema information</returns>
        //public override DataSets.TableKeysSchema.Primary_KeysDataTable GetTableKeysSchema(string tableName)
        //{
        //    OleDbConnection conn = new OleDbConnection("Provider=SQLOleDb;" + connectionString);

        //    try
        //    {
        //        OpenConnection(conn);
        //        DataTable t = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Primary_Keys, new object[] { null, null, tableName });
        //        DataSets.TableKeysSchema schema = new Epi.DataSets.TableKeysSchema();
        //        schema.Merge(t);
        //        return schema.Primary_Keys;
        //    }
        //    finally
        //    {
        //        CloseConnection(conn);
        //    }
        //}

        /// <summary>
        /// Gets column schema information about an OLE table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>DataTable with schema information</returns>
        public override DataSets.TableColumnSchema.ColumnsDataTable GetTableColumnSchema(string tableName)
        {
            try
            {
                DataTable table = this.GetSchema("Columns", tableName);
                // IS_NULLABLE and DATA_TYPE are different data types for non OleDb DataTables.
                DataSets.TableColumnSchema tableColumnSchema = new Epi.DataSets.TableColumnSchema();
                tableColumnSchema.Merge(table, false, MissingSchemaAction.Ignore);
                return tableColumnSchema.Columns;

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not get table column schema for " + tableName + ".", ex);
            }
        }

        /// <summary>
        /// Gets column schema information about an OLE table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>DataTable with schema information</returns>
        public override DataSets.ANSI.TableColumnSchema.ColumnsDataTable GetTableColumnSchemaANSI(string tableName)
        {
            try
            {
                DataTable table = this.GetSchema("Columns", tableName);
                // IS_NULLABLE and DATA_TYPE are different data types for non OleDb DataTables.
                DataSets.ANSI.TableColumnSchema tableColumnSchema = new Epi.DataSets.ANSI.TableColumnSchema();
                tableColumnSchema.Merge(table, false, MissingSchemaAction.Ignore);
                return tableColumnSchema.Columns;
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not get table column schema for " + tableName + ".", ex);
            }
        }

        /// <summary>
        /// Get ColumnNames by table name
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <returns>DataView</returns>
        public override DataView GetTextColumnNames(string tableName)
        {
            return new DataView(this.GetSchema("Columns", tableName));
            //            throw new System.ApplicationException("Table column schema can't be retrieved from SQL databases.");
        }
        /// <summary>
        /// Create SQL query object
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <returns></returns>
        public override Query CreateQuery(string sqlStatement)
        {
            return new SqlQuery(sqlStatement);
        }
        #endregion

        #region Generic IDbTransaction Methods (mirror Epi.Data.Office/Epi.Data.SqlServer)

        /// <summary>
        /// Tests database connectivity using current ConnnectionString
        /// </summary>
        /// <returns></returns>
        public override bool TestConnection()
        {
            try
            {
                return TestConnection(connectionString);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not connect to data source.", ex);
            }
        }

        /// <summary>
        /// Tests database connectivity using supplied connection string 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        protected bool TestConnection(string connectionString)
        {

            IDbConnection testConnection = GetConnection(connectionString);
            try
            {
                OpenConnection(testConnection);
            }
            finally
            {
                CloseConnection(testConnection);
            }

            return true;

        }

        /// <summary>
        /// Gets an connection instance based on current ConnectionString value
        /// </summary>
        /// <returns>Connection instance</returns>
        public override IDbConnection GetConnection()
        {
            return GetNativeConnection(connectionString);
        }

        /// <summary>
        /// Gets a connection instance from supplied connection string
        /// </summary>
        /// <returns>Connection instance</returns>
        protected virtual IDbConnection GetConnection(string connectionString)
        {
            return GetNativeConnection(connectionString);
        }


        /// <summary>
        /// Gets a new command using an existing transaction
        /// </summary>
        /// <param name="sqlStatement">The query to be executed against the database</param>
        /// <param name="transaction"></param>
        /// <param name="parameters">parameters">Parameters for the query to be executed</param>
        /// <returns>An OleDb command object</returns>
        protected virtual IDbCommand GetCommand(string sqlStatement, IDbTransaction transaction, List<QueryParameter> parameters)
        {

            #region Input Validation
            if (string.IsNullOrEmpty(sqlStatement))
            {
                throw new ArgumentNullException("sqlStatement");
            }
            if (parameters == null)
            {
                throw new ArgumentNullException("Parameters");
            }
            #endregion

            IDbCommand command = this.GetNativeCommand(transaction);
            command.CommandText = sqlStatement;

            foreach (QueryParameter parameter in parameters)
            {
                command.Parameters.Add(this.ConvertToNativeParameter(parameter));
            }

            return command;
        }


        /// <summary>
        ///  Gets a new command using an existing connection
        /// </summary>
        /// <param name="sqlStatement">The query to be executed against the database</param>
        /// <param name="connection"></param>
        /// <param name="parameters">Parameters for the query to be executed</param>
        /// <returns></returns>
        protected virtual IDbCommand GetCommand(string sqlStatement, IDbConnection connection, List<QueryParameter> parameters)
        {

            #region Input Validation
            if (string.IsNullOrEmpty(sqlStatement))
            {
                throw new ArgumentNullException("sqlStatement");
            }
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            #endregion

            IDbCommand command = connection.CreateCommand();
            command.CommandText = sqlStatement;

            foreach (QueryParameter parameter in parameters)
            {
                command.Parameters.Add(this.ConvertToNativeParameter(parameter));
            }

            return command;
        }

        /// <summary>
        ///  Opens specific connection if state is not already opened
        /// </summary>
        /// <param name="conn"></param>
        protected void OpenConnection(IDbConnection conn)
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
            }
            catch (Exception ex)
            {
                throw new System.ApplicationException("Error opening connection.", ex);
            }
        }

        /// <summary>
        /// Close a specific connection if state is not already closed
        /// </summary>
        /// <param name="conn"></param>
        protected void CloseConnection(IDbConnection conn)
        {
            try
            {
                if (conn != null)
                {
                    if (conn.State != ConnectionState.Closed)
                    {
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                conn = null;
                throw new System.ApplicationException("Error closing connection.", ex);
            }
        }

        /// <summary>
        /// Closes a database transaction and associated connection 
        /// </summary>
        public override void CloseTransaction(IDbTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("Transaction cannot be null.", "transaction");
            }

            CloseConnection(transaction.Connection);
        }

        /// <summary>
        /// Opens a database connection and begins a new transaction
        /// </summary>
        /// <returns>A specialized transaction object based on the current database engine type</returns>
        public override IDbTransaction OpenTransaction()
        {
            IDbConnection connection = GetConnection();
            OpenConnection(connection);
            return connection.BeginTransaction();
        }

        /// <summary>
        /// Opens a database connection and begins a new transaction
        /// </summary>
        /// <param name="isolationLevel">The transaction locking behavior for the connection</param>
        /// <returns>A specialized transaction object based on the current database engine type</returns>
        public override IDbTransaction OpenTransaction(IsolationLevel isolationLevel)
        {
            IDbConnection connection = GetConnection();
            OpenConnection(connection);
            return connection.BeginTransaction(isolationLevel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public override IDataReader GetTableDataReader(string tableName)
        {
            Query query = this.CreateQuery("select * from " + FormatTableName(tableName));
            return this.ExecuteReader(query);
        }

        /// <summary>
        /// Creates a new connection and executes a select query 
        /// </summary>
        /// <param name="selectQuery"></param>
        /// <returns>Result set</returns>
        public override DataTable Select(Query selectQuery)
        {
            #region Input Validation
            if (selectQuery == null)
            {
                throw new ArgumentNullException("selectQuery");
            }
            #endregion

            DataTable table = new DataTable();
            return Select(selectQuery, table);
        }

        /// <summary>
        ///  Returns contents of a table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnNames"></param>
        /// <param name="sortCriteria">Comma delimited string of column names and asc/DESC order</param>
        /// <returns></returns>
        public override DataTable GetTableData(string tableName, string columnNames, string sortCriteria)
        {
            try
            {
                if (string.IsNullOrEmpty(columnNames))
                {
                    columnNames = Epi.StringLiterals.STAR;
                }

                string queryString = "select " + columnNames + " from " + FormatTableName(tableName);                
                
                if (!string.IsNullOrEmpty(sortCriteria))
                {
                    queryString += " order by " + sortCriteria;
                }
                Query query = this.CreateQuery(queryString);
                return Select(query);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Returns contents of a table with only the top two rows.
        /// </summary>
        /// <param name="tableName">The name of the table to query</param>
        /// <returns>DataTable</returns>
        public override DataTable GetTopTwoTable(string tableName)
        {
            try
            {
                string queryString = "select top 2 * from " + FormatTableName(tableName);
                Query query = this.CreateQuery(queryString);
                return Select(query);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Executes a query and returns a stream of rows
        /// </summary>
        /// <param name="selectQuery">The query to be executed against the database</param>
        /// <returns>A data reader object</returns>
        public override IDataReader ExecuteReader(Query selectQuery)
        {
            #region Input Validation
            if (selectQuery == null)
            {
                throw new ArgumentNullException("SelectQuery");
            }
            #endregion

            return ExecuteReader(selectQuery, CommandBehavior.Default);
        }

        /// <summary>
        /// Executes a query and returns a stream of rows
        /// </summary>
        /// <param name="selectQuery">The query to be executed against the database</param>
        /// <returns>A data reader object</returns>
        public override IDataReader ExecuteReader(Query selectQuery, CommandBehavior commandBehavior)
        {

            #region Input Validation
            if (selectQuery == null)
            {
                throw new ArgumentNullException("SelectQuery");
            }
            #endregion

            IDbConnection connection = GetConnection();

            try
            {
                OpenConnection(connection);
                IDbCommand command = GetCommand(selectQuery.SqlStatement, connection, selectQuery.Parameters);
                return command.ExecuteReader(commandBehavior);
            }

            catch (Exception ex)
            {
                throw new System.ApplicationException("Could not execute reader", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Executes a scalar query against the database using an existing transaction
        /// </summary>
        /// <param name="query">The query to be executed against the database</param>
        /// <param name="transaction">Null is allowed</param>
        /// <returns></returns>
        public override object ExecuteScalar(Query query, IDbTransaction transaction)
        {
            #region Input Validation
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            #endregion

            object result;
            IDbCommand command = GetCommand(query.SqlStatement, transaction, query.Parameters);

            try
            {
                // do not open connection, we are inside a transaction
                result = command.ExecuteScalar();
            }
            finally
            {
                // do not close connection, we are inside a transaction
            }

            return result;
        }

        /// <summary>
        /// Executes a scalar query against the database
        /// </summary>
        /// <param name="query">The query to be executed against the database</param>
        /// <returns></returns>
        public override object ExecuteScalar(Query query)
        {

            #region Input Validation
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            #endregion

            object result;
            IDbConnection conn = GetConnection();
            IDbCommand command = GetCommand(query.SqlStatement, conn, query.Parameters);

            try
            {
                OpenConnection(conn);
                result = command.ExecuteScalar();
            }
            finally
            {
                CloseConnection(conn);
            }

            return result;
        }

        /// <summary>
        /// Executes a SQL statement and returns total records affected. 
        /// </summary>
        /// <param name="query">The query to be executed against the database</param>
        public override int ExecuteNonQuery(Query query)
        {
            #region Input Validation
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            #endregion

            //Logger.Log(query);
            IDbConnection conn = this.GetConnection();
            IDbCommand command = GetCommand(query.SqlStatement, conn, query.Parameters);

            try
            {
                OpenConnection(conn);
                return command.ExecuteNonQuery();
            }
            finally
            {
                CloseConnection(conn);
            }
        }

        /// <summary>
        /// Executes a transacted SQL statement and returns total records affected. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="transaction"></param>
        /// <returns>returns total records affected.</returns>
        public override int ExecuteNonQuery(Query query, IDbTransaction transaction)
        {

            #region Input Validation
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            #endregion

            //Logger.Log(query);
            IDbCommand command = GetCommand(query.SqlStatement, transaction, query.Parameters);
            command.CommandTimeout = 1500;

            //int result;

            try
            {
                // do not try to open connection, we are inside a transaction
                return command.ExecuteNonQuery();
            }
            finally
            {
                // do not close connection, we are inside a transaction
            }
        }

        ///// <summary>
        ///// Disposes the object
        ///// </summary>
        //public void Dispose()
        //{
        //}

          #endregion

        #region Private Methods
        #endregion // Private Methods

        /// <summary>
        /// Determines the level of rights the user has on the SQL database
        /// </summary>
        public override ProjectPermissions GetPermissions()
        {
            ProjectPermissions permissions = new ProjectPermissions();
            permissions.FullPermissions = false;

            Query query = this.CreateQuery("SELECT HAS_PERMS_BY_NAME(QUOTENAME(SCHEMA_NAME(schema_id)) " +
                "+ '.' + QUOTENAME(name), 'OBJECT', 'SELECT') AS have_select, name FROM sys.tables");
            DataTable selectDt = Select(query);
            
            DataTable permissionsTable = new DataTable();
            permissionsTable.Columns.Add("name", typeof(string)); // table name
            permissionsTable.Columns.Add("can_select", typeof(bool));
            permissionsTable.Columns.Add("can_insert", typeof(bool));
            permissionsTable.Columns.Add("can_update", typeof(bool));
            permissionsTable.Columns.Add("can_delete", typeof(bool));

            permissionsTable.Columns["name"].Unique = true;

            DataColumn[] primaryKeyColumns = new DataColumn[1];
            primaryKeyColumns[0] = permissionsTable.Columns["name"];
            permissionsTable.PrimaryKey = primaryKeyColumns;

            List<string> tableNames = new List<string>();            
            
            foreach (DataRow row in selectDt.Rows)
            {
                foreach (DataColumn dc in selectDt.Columns)
                {
                    System.Diagnostics.Debug.Print(dc.ColumnName + " =  " + row[dc.ColumnName].ToString());
                }
                string tableName = row["name"].ToString();
                tableNames.Add(tableName);
                permissionsTable.Rows.Add(tableName);
                System.Diagnostics.Debug.Print("");
            }            

            selectDt = new DataTable();
            DataTable insertDt = new DataTable();
            DataTable updateDt = new DataTable();
            DataTable deleteDt = new DataTable();

            foreach (string tableName in tableNames)
            {
                query = this.CreateQuery("SELECT HAS_PERMS_BY_NAME('" + tableName + "', 'OBJECT', 'INSERT');");
                insertDt = Select(query);
                string value = insertDt.Rows[0][0].ToString();

                DataRow dr = permissionsTable.Rows.Find(tableName);
                if (value == "1")
                {
                    permissions.TablesWithInsertPermissions.Add(tableName);
                }

                #region Debug
                foreach (DataRow row in insertDt.Rows)
                {
                    foreach (DataColumn dc in insertDt.Columns)
                    {
                        System.Diagnostics.Debug.Print(dc.ColumnName + " =  " + row[dc.ColumnName].ToString());
                    }
                }
                #endregion // Debug
            }

            foreach (string tableName in tableNames)
            {
                query = this.CreateQuery("SELECT HAS_PERMS_BY_NAME('" + tableName + "', 'OBJECT', 'UPDATE');");
                updateDt = Select(query);

                string value = updateDt.Rows[0][0].ToString();

                DataRow dr = permissionsTable.Rows.Find(tableName);
                if (value == "1")
                {
                    permissions.TablesWithUpdatePermissions.Add(tableName);
                }

                #region Debug
                foreach (DataRow row in updateDt.Rows)
                {
                    foreach (DataColumn dc in updateDt.Columns)
                    {
                        System.Diagnostics.Debug.Print(dc.ColumnName + " =  " + row[dc.ColumnName].ToString());
                    }
                }
                #endregion // Debug
            }

            foreach (string tableName in tableNames)
            {
                query = this.CreateQuery("SELECT HAS_PERMS_BY_NAME('" + tableName + "', 'OBJECT', 'DELETE');");
                deleteDt = Select(query);

                string value = deleteDt.Rows[0][0].ToString();

                DataRow dr = permissionsTable.Rows.Find(tableName);
                if (value == "1")
                {
                    permissions.TablesWithDeletePermissions.Add(tableName);
                }

                #region Debug
                foreach (DataRow row in deleteDt.Rows)
                {
                    foreach (DataColumn dc in deleteDt.Columns)
                    {
                        System.Diagnostics.Debug.Print(dc.ColumnName + " =  " + row[dc.ColumnName].ToString());
                    }
                }
                #endregion // Debug
            }

            foreach (string tableName in tableNames)
            {
                query = this.CreateQuery("SELECT HAS_PERMS_BY_NAME('" + tableName + "', 'OBJECT', 'SELECT');");
                selectDt = Select(query);

                string value = selectDt.Rows[0][0].ToString();

                DataRow dr = permissionsTable.Rows.Find(tableName);
                if (value == "1")
                {
                    permissions.TablesWithSelectPermissions.Add(tableName);
                }

                #region Debug
                foreach (DataRow row in selectDt.Rows)
                {
                    foreach (DataColumn dc in selectDt.Columns)
                    {
                        System.Diagnostics.Debug.Print(dc.ColumnName + " =  " + row[dc.ColumnName].ToString());
                    }
                }
                #endregion // Debug
            }
            
            List<string> metaTableNames = new List<string>();
            List<string> formDataTableNames = new List<string>();

            foreach (string tableName in tableNames)
            {                
                if (tableName.StartsWith("meta"))
                {
                    metaTableNames.Add(tableName);
                }
            }

            permissions.CanDeleteRowsInMetaTables = true;
            permissions.CanInsertRowsInMetaTables = true;
            permissions.CanSelectRowsInMetaTables = true;
            permissions.CanUpdateRowsInMetaTables = true;

            foreach (string metaTableName in metaTableNames)
            {
                DataRow row = permissionsTable.Rows.Find(metaTableName);

                if (permissions.CanDeleteRowsInMetaTables)
                {
                    permissions.CanDeleteRowsInMetaTables = permissions.TablesWithDeletePermissions.Contains(metaTableName);
                }

                if (permissions.CanInsertRowsInMetaTables)
                {
                    permissions.CanInsertRowsInMetaTables = permissions.TablesWithInsertPermissions.Contains(metaTableName);
                }

                if (permissions.CanSelectRowsInMetaTables)
                {
                    permissions.CanSelectRowsInMetaTables = permissions.TablesWithSelectPermissions.Contains(metaTableName);
                }

                if (permissions.CanUpdateRowsInMetaTables)
                {
                    permissions.CanUpdateRowsInMetaTables = permissions.TablesWithUpdatePermissions.Contains(metaTableName);
                }
            }

            return permissions;
        }

        /// <summary>
        /// Gets the contents of a table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>Datatable containing the table data</returns>
        public override DataTable GetTableData(string tableName)
        {
            return GetTableData(tableName, string.Empty, string.Empty);
        }

        /// <summary>
        /// Returns contents of a table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnNames">Comma delimited string of column names and asc/DESC order</param>
        /// <returns></returns>
        public override DataTable GetTableData(string tableName, string columnNames)
        {
            return GetTableData(tableName, columnNames, string.Empty);
        }

        internal List<string> GetDatabaseNameList()
        {
            List<string> databases = new List<string>();
            SqlConnection sqlconn = new SqlConnection(ConnectionString);
            try
            {
                OpenConnection(sqlconn);
                SqlCommand cmd = sqlconn.CreateCommand();
                cmd.CommandText = "sp_databases";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 1500;
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    databases.Add(reader.GetString(0));
                }

            }
            finally
            {
                CloseConnection(sqlconn);
            }
            return databases;
        }

        /// <summary>
        /// Builds a connection string using default parameters given a database name
        /// </summary>
        /// <remarks>In the case of SQL Server, it is not possible to build a default connection string using just a database name. You also need to provide a default server.</remarks>
        /// <param name="databaseName">Name of the database</param>
        /// <returns>A connection string</returns>
        public static string BuildDefaultConnectionString(string databaseName)
        {
            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public override DataTable GetCodeTableNamesForProject(Project project)
        {
            List<string> tables = new List<string>(); //dpb//project.GetDataTableList();
            DataSets.TableSchema.TablesDataTable codeTables = project.GetCodeTableList();
            
            foreach (DataSets.TableSchema.TablesRow row in codeTables)
            {
                tables.Add(row.TABLE_NAME);
            }

            DataView dataView = codeTables.DefaultView;
            return codeTables;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public override Epi.DataSets.TableSchema.TablesDataTable GetCodeTableList(IDbDriver db)
        {
            Epi.DataSets.TableSchema.TablesDataTable table = db.GetTableSchema();

            //remove all databases that do not start with "code"
            DataRow[] rowsFiltered = table.Select("TABLE_NAME not like 'code%'");
            
            foreach (DataRow rowFiltered in rowsFiltered)
            {
                table.Rows.Remove(rowFiltered);
            }

            return table;
        }

        /// <summary>
        /// Identity MS SQL Server Database
        /// </summary>
        /// <returns>"SQLSERVER"</returns>
        public override string IdentifyDatabase()
        {
            return "SQLSERVER";
        }

        /// <summary>
        /// Inserts the string in escape characters. [] for SQL server and `` for MySQL etc.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public override string InsertInEscape(string str)
        {
            string newString = string.Empty;
            if (!str.StartsWith(StringLiterals.LEFT_SQUARE_BRACKET))
            {
                newString = StringLiterals.LEFT_SQUARE_BRACKET;
            }
            newString += str;
            if (!str.EndsWith(StringLiterals.RIGHT_SQUARE_BRACKET))
            {
                newString += StringLiterals.RIGHT_SQUARE_BRACKET;
            }
            return newString;
        }

        /// <summary>
        /// Provides a database frieldly string representation of date.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public override string FormatTime(DateTime dt) // TODO: Revisit it.
        {
            return Util.InsertInSingleQuotes(dt.ToString());
        }

        /// <summary>
        /// Convert Query to String format for EnterWebService
        /// </summary>
        /// <param name="pValue"></param>
        /// <returns>Sql query in string form</returns>
        private string ConvertQueryToString(Query pValue)
        {
            string result = pValue.SqlStatement;

            foreach (QueryParameter parameter in pValue.Parameters)
            {
                switch (parameter.DbType)
                {
                    case DbType.Currency:
                    case DbType.Byte:
                    case DbType.Decimal:
                    case DbType.Double:
                    case DbType.Int16:
                    case DbType.Int32:
                    case DbType.Int64:
                    case DbType.SByte:
                    case DbType.UInt16:
                    case DbType.UInt32:
                    case DbType.UInt64:
                    //case DbType.Boolean:
                        result = Regex.Replace(result, parameter.ParameterName, parameter.Value.ToString(), RegexOptions.IgnoreCase);
                        break;
                    default:
                        result = Regex.Replace(result, parameter.ParameterName, "'" + parameter.Value.ToString() + "'", RegexOptions.IgnoreCase);
                        break;
                }
            }

            return result;
        }

        public override string SyntaxTrue
        {
            get { return "'TRUE'"; }
        }

        public override string SyntaxFalse
        {
            get { return "'FALSE'"; }
        }

        public override bool InsertBulkRows(string pSelectSQL, System.Data.Common.DbDataReader pDataReader, SetGadgetStatusHandler pStatusDelegate = null, CheckForCancellationHandler pCancellationDelegate = null)
        {
            bool result = false;

            System.Data.SqlClient.SqlConnection Conn = null;
            System.Data.SqlClient.SqlDataAdapter Adapter = null;
            System.Data.SqlClient.SqlCommandBuilder builderSQL = null;
            System.Data.Common.DbCommand cmdSqL = null;

            DataSet dataSet = new DataSet();
            DataTable Temp = new DataTable();

            try
            {
                StringBuilder InsertSQL;
                StringBuilder ValueSQL;

                Conn = new System.Data.SqlClient.SqlConnection(ConnectionString);
                Adapter = new System.Data.SqlClient.SqlDataAdapter(pSelectSQL, Conn);
                Adapter.FillSchema(dataSet, SchemaType.Source);
                builderSQL = new System.Data.SqlClient.SqlCommandBuilder(Adapter);
                Conn.Open();
                cmdSqL = Conn.CreateCommand();
                
                cmdSqL.CommandTimeout = 1500;
                
                int rowCount = 0;
                int skippedRows = 0;
                int totalRows = 0;

                if (pStatusDelegate != null && dataSet.Tables.Count > 0)
                {                    
                    totalRows = dataSet.Tables[0].Rows.Count;
                }

                while (pDataReader.Read())
                {
                    cmdSqL = builderSQL.GetInsertCommand(true);

                    InsertSQL = new StringBuilder();
                    ValueSQL = new StringBuilder();

                    InsertSQL.Append("Insert Into ");
                    InsertSQL.Append(pSelectSQL.Replace("Select * From ", ""));
                    InsertSQL.Append(" (");
                    ValueSQL.Append(" values (");

                    List<System.Data.SqlClient.SqlParameter> ParameterList = new List<SqlParameter>();
                    foreach (System.Data.SqlClient.SqlParameter param in cmdSqL.Parameters)
                    {
                        string FieldName = param.SourceColumn;

                        InsertSQL.Append("[");
                        InsertSQL.Append(FieldName);
                        InsertSQL.Append("],");

                        ValueSQL.Append(param.ParameterName);
                        ValueSQL.Append(",");

                        param.Value = pDataReader[FieldName];
                        ParameterList.Add(param);
                    }
                    InsertSQL.Length = InsertSQL.Length - 1;
                    ValueSQL.Length = ValueSQL.Length - 1;
                    InsertSQL.Append(")");
                    ValueSQL.Append(")");
                    InsertSQL.Append(ValueSQL);
                    cmdSqL = null;
                    cmdSqL = Conn.CreateCommand();
                    cmdSqL.CommandText = InsertSQL.ToString();

                    foreach (System.Data.SqlClient.SqlParameter param in ParameterList)
                    {

                        DbParameter p2 = cmdSqL.CreateParameter();
                        p2.DbType = param.DbType;
                        p2.Value = pDataReader[param.SourceColumn];
                        p2.ParameterName = param.ParameterName;
                        cmdSqL.Parameters.Add(p2);
                    }

                    try
                    {
                        cmdSqL.ExecuteNonQuery();
                        rowCount++;
                    }
                    catch (Exception ex)
                    {
                        skippedRows++;                        
                        continue;
                    }

                    if (pStatusDelegate != null)
                    {
                        pStatusDelegate.Invoke(string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS, rowCount.ToString(), totalRows.ToString()), (double)rowCount);
                    }

                    if (pCancellationDelegate != null && pCancellationDelegate.Invoke())
                    {
                        pStatusDelegate.Invoke(string.Format(SharedStrings.DASHBOARD_EXPORT_CANCELLED, rowCount.ToString()));
                        break;
                    }
                }

                
                if (pStatusDelegate != null)
                {
                    pStatusDelegate.Invoke(string.Format(SharedStrings.DASHBOARD_EXPORT_SUCCESS, rowCount.ToString()));
                }
            }
            catch (System.Exception ex)
            {
                Logger.Log(DateTime.Now + ":  " + ex.Message);
            }
            finally
            {
                if (Conn != null)
                {
                    Conn.Close();
                }
            }

            result = true;
            return result;
        }

        public override bool Insert_1_Row(string pSelectSQL, System.Data.Common.DbDataReader pDataReader)
        {
            bool result = false;

            System.Data.SqlClient.SqlConnection Conn = null;
            System.Data.SqlClient.SqlDataAdapter Adapter = null;
            System.Data.SqlClient.SqlCommandBuilder builderSQL = null;
            System.Data.Common.DbCommand cmdSqL = null;

            DataSet dataSet = new DataSet();
            DataTable Temp = new DataTable();

            try
            {
                StringBuilder InsertSQL = new StringBuilder();
                StringBuilder ValueSQL = new StringBuilder();

                Conn = new System.Data.SqlClient.SqlConnection(ConnectionString);
                Adapter = new System.Data.SqlClient.SqlDataAdapter(pSelectSQL, Conn);
                //Adapter.FillSchema(dataSet, SchemaType.Source, pDestinationTableName);
                Adapter.FillSchema(dataSet, SchemaType.Source);
                builderSQL = new System.Data.SqlClient.SqlCommandBuilder(Adapter);
                Conn.Open();
                cmdSqL = Conn.CreateCommand();
                cmdSqL = builderSQL.GetInsertCommand(true);
                cmdSqL.CommandTimeout = 1500;


                InsertSQL.Append("Insert Into ");
                InsertSQL.Append(pSelectSQL.Replace("Select * From ", ""));
                InsertSQL.Append(" (");
                ValueSQL.Append(" values (");

                List<System.Data.SqlClient.SqlParameter> ParameterList = new List<SqlParameter>();
                foreach (System.Data.SqlClient.SqlParameter param in cmdSqL.Parameters)
                {
                    //string FieldName = param.ParameterName.TrimStart(new char[] { '@' });
                    string FieldName = param.SourceColumn;

                    InsertSQL.Append("[");
                    InsertSQL.Append(FieldName);
                    InsertSQL.Append("],");

                    //ValueSQL.Append("@");
                    ValueSQL.Append(param.ParameterName);
                    ValueSQL.Append(",");

                    param.Value = pDataReader[FieldName];
                    ParameterList.Add(param);
                    /*
                    if (pDataReader[FieldName] == DBNull.Value)
                    {
                        ValueSQL.Append("null");
                    }
                    else
                    {
                        switch (pDataReader[FieldName].GetType().ToString())
                        {

                            case "System.Boolean":
                                if (Convert.ToBoolean(pDataReader[FieldName]) == false)
                                {
                                    ValueSQL.Append("0");
                                }
                                else
                                {
                                    ValueSQL.Append("1");
                                }
                                break;
                            case "System.Int32":
                            case "System.Decimal":
                            case "System.Double":
                            case "System.Single":
                            case "System.Byte":
                                ValueSQL.Append(pDataReader[FieldName].ToString().Replace("'", "''"));
                                break;
                            default:
                                ValueSQL.Append("'");
                                ValueSQL.Append(pDataReader[FieldName].ToString().Replace("'", "''"));
                                ValueSQL.Append("'");
                                break;
                        }
                    }*/

                    //ValueSQL.Append(",");

                }
                InsertSQL.Length = InsertSQL.Length - 1;
                ValueSQL.Length = ValueSQL.Length - 1;
                InsertSQL.Append(")");
                ValueSQL.Append(")");
                InsertSQL.Append(ValueSQL);
                builderSQL = null;
                cmdSqL = null;
                cmdSqL = Conn.CreateCommand();
                cmdSqL.CommandText = InsertSQL.ToString();

                foreach (System.Data.SqlClient.SqlParameter param in ParameterList)
                {

                    DbParameter p2 = cmdSqL.CreateParameter();
                    p2.DbType = param.DbType;
                    p2.Value = pDataReader[param.SourceColumn];
                    p2.ParameterName = param.ParameterName;

                    cmdSqL.Parameters.Add(p2);
                }
                //DBReadExecute.ExecuteSQL(pFileString, InsertSQL.ToString());
                
                cmdSqL.ExecuteNonQuery();
            }
            catch (System.Exception ex)
            {
                Logger.Log(DateTime.Now + ":  " + ex.Message);
            }
            finally
            {
                if (Conn != null)
                {
                    Conn.Close();
                }
            }

            result = true;
            return result;
        }

        public override bool Update_1_Row(string pSelectSQL, string pKeyString, DbDataReader pDataReader)
        {
            bool result = false;

            System.Data.SqlClient.SqlConnection Conn = null;
            System.Data.SqlClient.SqlDataAdapter Adapter = null;
            System.Data.SqlClient.SqlCommandBuilder builderSQL = null;
            System.Data.SqlClient.SqlCommand cmdSqL = null;

            DataSet dataSet = new DataSet();
            DataTable Temp = new DataTable();

            try
            {
                StringBuilder UpdateSQL = new StringBuilder();

                Conn = new System.Data.SqlClient.SqlConnection(ConnectionString);
                Adapter = new System.Data.SqlClient.SqlDataAdapter(pSelectSQL, Conn);
                //Adapter.FillSchema(dataSet, SchemaType.Source, pDestinationTableName);
                Adapter.FillSchema(dataSet, SchemaType.Source);
                builderSQL = new System.Data.SqlClient.SqlCommandBuilder(Adapter);
                Conn.Open();

                cmdSqL = Conn.CreateCommand();
                cmdSqL = builderSQL.GetInsertCommand();
                cmdSqL.CommandTimeout = 1500;

                UpdateSQL.Append("Update ");
                UpdateSQL.Append(pSelectSQL.Replace("Select * From ", ""));
                UpdateSQL.Append(" Set ");
                foreach (System.Data.SqlClient.SqlParameter param in cmdSqL.Parameters)
                {
                    //string FieldName = param.ParameterName.TrimStart(new char[] { '@' });
                    string FieldName = param.SourceColumn;

                    if (pDataReader[FieldName] != DBNull.Value && !string.IsNullOrEmpty(pDataReader[FieldName].ToString()))
                    {
                        UpdateSQL.Append("[");
                        UpdateSQL.Append(FieldName);
                        UpdateSQL.Append("]=");

                        switch (pDataReader[FieldName].GetType().ToString())
                        {
                            case "System.Boolean":
                                if (Convert.ToBoolean(pDataReader[FieldName]) == false)
                                {
                                    UpdateSQL.Append("0");
                                }
                                else
                                {
                                    UpdateSQL.Append("1");
                                }
                                break;
                            case "System.Int32":
                            case "System.Decimal":
                            case "System.Double":
                            case "System.Single":
                            case "System.Byte":
                                UpdateSQL.Append(pDataReader[FieldName].ToString().Replace("'", "''"));
                                break;
                            default:
                                UpdateSQL.Append("'");
                                UpdateSQL.Append(pDataReader[FieldName].ToString().Replace("'", "''"));
                                UpdateSQL.Append("'");
                                break;
                        }
                        UpdateSQL.Append(",");
                    }


                }
                UpdateSQL.Length = UpdateSQL.Length - 1;
                UpdateSQL.Append(" Where ");
                UpdateSQL.Append(pKeyString);
                //builderOLE = null;
                cmdSqL = null;
                cmdSqL = Conn.CreateCommand();
                cmdSqL.CommandText = UpdateSQL.ToString();
                cmdSqL.ExecuteNonQuery();
            }
            catch (System.Exception ex)
            {
                Logger.Log(DateTime.Now + ":  " + ex.Message);
            }
            finally
            {
                if (Conn != null)
                {
                    Conn.Close();
                }
            }

            result = true;
            return result;
        }

        public override DbDataAdapter GetDbAdapter(string pSelectSQL)
        {
            IDbConnection connection = GetConnection();
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.SelectCommand = (SqlCommand)GetCommand(pSelectSQL, connection,new List<QueryParameter>());
            adapter.SelectCommand.CommandTimeout = 1500;
            return adapter;
        }

        public override DbCommand GetCommand(string pKeyString, DataTable pDataTable)
        {
            System.Data.Common.DbCommand result = null;
            System.Data.Common.DbParameter P = null;

            string[] KeySet = null;
            StringBuilder KeyMatch = new StringBuilder();

            result = new System.Data.SqlClient.SqlCommand();
            result.CommandTimeout = 1500;
            foreach (DataColumn C in pDataTable.Columns)
            {
                P = new System.Data.SqlClient.SqlParameter();
                P.ParameterName = C.ColumnName;
                switch (C.DataType.Name)
                {
                    case "Int16":
                        P.DbType = DbType.Int16;
                        break;
                    case "Int32":
                        P.DbType = DbType.Int32;
                        break;
                    case "Date":
                    case "DateTime":
                        P.DbType = DbType.DateTime;
                        break;
                    case "String":
                        P.DbType = DbType.String;
                        break;
                    default:
                        P.DbType = DbType.String;
                        break;
                }
                result.Parameters.Add(P);
            }
            KeySet = pKeyString.Split(new string[] { " And " }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < KeySet.Length; i++)
            {
                string[] temp = KeySet[i].Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                P = new System.Data.SqlClient.SqlParameter();
                P.ParameterName = temp[1].Trim();
                foreach (DataColumn C in pDataTable.Columns)
                {
                    if (P.ParameterName.ToLower() == C.ColumnName.ToLower())
                    {
                        switch (C.DataType.Name)
                        {
                            case "Int16":
                                P.DbType = DbType.Int16;
                                break;
                            case "Int32":
                                P.DbType = DbType.Int32;
                                break;
                            case "Date":
                            case "DateTime":
                                P.DbType = DbType.DateTime;
                                break;
                            case "String":
                                P.DbType = DbType.String;
                                break;
                            default:
                                P.DbType = DbType.String;
                                break;
                        }
                        break;
                    }
                }
                result.Parameters.Add(P);
            }

            return result;
        }

        public override DbCommandBuilder GetDbCommandBuilder(DbDataAdapter Adapter)
        {
            SqlCommandBuilder builderSQL = new SqlCommandBuilder((SqlDataAdapter)Adapter);
            return builderSQL;
        }


    }
}
