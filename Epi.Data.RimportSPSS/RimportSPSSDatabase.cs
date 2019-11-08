using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using Epi.Data.RimportSPSS.Wrappers;
using Epi.Data;


namespace Epi.Data.RimportSPSS
{
    /// <summary>
    /// RimportSPSS implementation of DbDriverBase implementation of IDbDriver implementation.
    /// </summary>
    public partial class RimportSPSSDatabase : DbDriverBase
    {
        /// <summary>
        /// RimportSPSS Database Constructor
        /// </summary>
        public RimportSPSSDatabase():base(){}

        /// <summary>
        /// Set RimportSPSS path
        /// </summary>
        /// <param name="filePath"></param>
        public void SetDataSourceFilePath(string filePath)
        {
            this.ConnectionString = filePath;
        }

        /// <summary>
        /// Returns the full name of the data source
        /// </summary>
        public override string FullName // Implements Database.FullName
        {
            get
            {
                return DataSource + "." + DbName;
            }
        }

        /// <summary>
        /// Connection String attribute
        /// </summary>
        public override string ConnectionDescription
        {
            get { return "RimportSPSS Database: " + this.DbName; }
        }

        /// <summary>
        /// Returns the maximum number of columns a table can have.
        /// </summary>
        public override int TableColumnMax
        {
            get { return 1020; }
        }

        /// <summary>
        /// Adds a column to the table
        /// </summary>
        /// <param name="tableName">name of the table</param>
        /// <param name="column">The column</param>
        /// <returns>Boolean</returns>
        public override bool AddColumn(string tableName, TableColumn column)
        {
            // E. Knudsen - TODO
            return false;
        }

        public override string SchemaPrefix
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Create a table
        /// </summary>
        /// <param name="tableName">Name of the table to create</param>
        /// <param name="columns">Collection of columns for the table</param>
        public override void CreateTable(string tableName, List<TableColumn> columns)
        {

            new RimportSPSSWrapper(connectionString).CreateCollection(tableName);

            /*
            StringBuilder sb = new StringBuilder();

            sb.Append("create table if not exists ");
            sb.Append(tableName);
            sb.Append(" (");

            AddColumns(sb, tableName, columns);

            sb.Append("); ");

            #region Input Validation
            if (sb == null)
            {
                throw new ArgumentNullException("Query is null");
            }
            #endregion

            RimportSPSSCommand command = new RimportSPSSCommand(sb.ToString());
            RimportSPSSConnection conn = new RimportSPSSConnection(this.ConnectionString);

            try
            {
                command.Connection = conn;
                OpenConnection(conn);
                command.ExecuteNonQuery();
            }
            finally
            {
                CloseConnection(conn);
            }
            */
        }

        #region Private Members

        // Add columns to a RimportSPSS Database
        private StringBuilder AddColumns(StringBuilder sb, string tableName, List<TableColumn> columns)
        {
            foreach (TableColumn column in columns)
            {
                string columnType = GetDbSpecificColumnType(column.DataType);
                sb.Append(column.Name);
                sb.Append(" ");
                sb.Append(columnType);

                if (column.Length != null)
                {
                    if (columnType.Equals("text") ||
                        (columnType.Equals("integer")) ||
                        (columnType.Equals("bit")) ||
                        (columnType.Equals("tinyint")) ||
                        (columnType.Equals("smallint")) ||
                        (columnType.Equals("mediumint")) ||
                        (columnType.Equals("mediumtext")) ||
                        (columnType.Equals("bigint")) ||
                        (columnType.Equals("binary"))
                        )
                    {
                        if (column.Length.Value.ToString() != null)
                        {
                            sb.Append("(");
                            sb.Append(column.Length.Value.ToString());
                            sb.Append(")");
                        }
                    }
                    else if ((columnType.Equals("double")) ||
                        (columnType.Equals("float")) ||
                        (columnType.Equals("decimal"))
                        )
                    {
                        sb.Append("(");
                        sb.Append(column.Length.Value.ToString());
                        if (column.Precision != null)
                        {
                            sb.Append(", ");
                            sb.Append(column.Precision.ToString());
                        }
                        sb.Append(") ");
                    }
                }

                if (!column.AllowNull)
                {
                    sb.Append(" not");
                }
                sb.Append(" null");
                if (column.IsIdentity)
                {
                    sb.Append(" auto_increment");
                }
                if (column.IsPrimaryKey)
                {
                    sb.Append(" primary key");
                }
                sb.Append(", ");
            }
            //remove trailing comma and space
            sb.Remove(sb.Length - 2, 2);
            return sb;
        }

        /// <summary>
        /// Set RimportSPSS Data Type mapping
        /// </summary>
        /// <param name="dataType">DataType</param>
        /// <returns>String</returns>
        public override string GetDbSpecificColumnType(GenericDbColumnType dataType)
        {
            switch (dataType)
            {
                case GenericDbColumnType.AnsiString:
                case GenericDbColumnType.AnsiStringFixedLength:
                case GenericDbColumnType.Byte:
                case GenericDbColumnType.Guid:
                case GenericDbColumnType.String:
                case GenericDbColumnType.StringFixedLength:
                    return "text";              
                case GenericDbColumnType.Binary:
                    return "binary";
                case GenericDbColumnType.Boolean:
                    return "bool";                
                case GenericDbColumnType.Currency:
                case GenericDbColumnType.Double:
                    return "double";
                case GenericDbColumnType.Date:
                    return "date";
                case GenericDbColumnType.DateTime:
                    return "datetime";
                case GenericDbColumnType.Decimal:
                    return "decimal";
                case GenericDbColumnType.Image:
                    return RimportSPSSColumnType.Longblob;
                case GenericDbColumnType.Int16:
                case GenericDbColumnType.UInt16:
                    return "smallint";
                case GenericDbColumnType.Int32:
                case GenericDbColumnType.UInt32:
                    return "integer";
                case GenericDbColumnType.Int64:
                case GenericDbColumnType.UInt64:
                    return "bigint";
                case GenericDbColumnType.Object:
                    return "blob";
                case GenericDbColumnType.SByte:
                    return "tinyint";
                case GenericDbColumnType.Single:
                    return "float";
                case GenericDbColumnType.StringLong:
                    return "longtext";
                case GenericDbColumnType.Time:
                    return "time";
                case GenericDbColumnType.VarNumeric:
                    return "decimal";
                case GenericDbColumnType.Xml:
                    return "mediumtext";
                default:
                    throw new GeneralException("genericDbColumnType is unknown");
            }
        }

        #endregion

        private string connectionString;
        /// <summary>
        /// Connection String attribute
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
                if (value != null && value.Contains("~"))
                {
                    string[] splits = value.Split('~');
                    this.DbName = splits[splits.Length - 1];
                }
                /*try
                {
                    IDbConnection conn = GetConnection();
                    this.DbName = conn.Database;
                }
                catch
                {
                    this.connectionString = null;
                }*/
            }
        }

        /// <summary>
        /// Build connection string
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <param name="password">Password</param>
        /// <returns>Connection string</returns>
        public static string BuildConnectionString(string filePath, string password)
        {
            //This method may need to be deprecated
            //Windows
            //   "Persist Security Info=False;database=myDB;server=myHost;Connect Timeout=30;user id=myUser; pwd=myPass";
            //Linux with MONO: filepath is all of the below statement
            // "database=myDB;server=/var/lib/RimportSPSS/RimportSPSS.sock;user id=myUser; pwd=myPass"; 

            return string.Empty;
        }

        /// <summary>
        /// Builds a connection string using default parameters given a database name
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        /// <returns>A connection string</returns>
        public static string BuildDefaultConnectionString(string databaseName)
        {

            //This method may need to be deprecated
            //Linux: return BuildConnectionString("database=" + databaseName + ";server=//var/lib/RimportSPSS/RimportSPSS.sock;user id=myUser; pwd=myPass","");
            return null;

        }

        /// <summary>
        /// Creates initial connection string from user-supplied database, server, user, and password input.
        /// </summary>
        /// <param name="database">Data store.</param>
        /// <param name="server">Server location of database.</param>
        /// <param name="user">User account login Id.</param>
        /// <param name="password">User login password.</param>
        /// <returns></returns>
        public string BuildDefaultConnectionString(string database, string server, string user, string password)
        {
            RimportSPSSConnectionStringBuilder RimportSPSSConnBuild = new RimportSPSSConnectionStringBuilder();
            RimportSPSSConnBuild.AutoCache = false; //.PersistSecurityInfo = false;
            RimportSPSSConnBuild.Database = database;
            RimportSPSSConnBuild.Server = server;
            RimportSPSSConnBuild.User = user;
            RimportSPSSConnBuild.Password = password;

            return RimportSPSSConnBuild.ToString();
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
        /// Tests database connectivity using current ConnnectionString
        /// </summary>
        /// <returns></returns>
        public override bool TestConnection()
        {
            try
            {
                return TestConnection(this.ConnectionString);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not connect to RimportSPSS Database.", ex);
            }
        }

        /// <summary>
        /// Tests database connectivity using supplied connection string 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        protected bool TestConnection(string connectionString)
        {
            RimportSPSSConnection testConnection = GetConnection(connectionString);
            return new RimportSPSSWrapper(connectionString).TestConnection();
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
        /// Gets an connection instance based on current ConnectionString value
        /// </summary>
        /// <returns>Connection instance</returns>
        protected RimportSPSSConnection GetConnection(string connectionString)
        {
            RimportSPSSConnectionStringBuilder RimportSPSSConnectionStringBuilder = new RimportSPSSConnectionStringBuilder(connectionString);
            return new RimportSPSSConnection(RimportSPSSConnectionStringBuilder.ToString());
        }

        public override bool IsBulkOperation
        {
            get
            {
                return true;
            }
            set
            {
                
            }
        }

        private string dbName = string.Empty;
        /// <summary>
        /// Gets or sets the Database name
        /// </summary>
        public override string DbName
        {
            get
            {
                return dbName;
            }
            set
            {
                dbName = value;
            }
        }

        /// <summary>
        /// Gets an OLE-compatible connection string.
        /// This is needed by Epi Map, as ESRI does not understand .NET connection strings.
        /// </summary>
        public override string OleConnectionString
        {
            get { return null; }
        }

        /// <summary>
        /// RimportSPSS data source.
        /// </summary>
        public override string DataSource
        {
            get
            {
                RimportSPSSConnection sqlconn = GetConnection(connectionString) as RimportSPSSConnection;
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
        /// Gets table schema information 
        /// </summary>
        /// <returns>DataTable with schema information</returns>
        public override Epi.DataSets.TableSchema.TablesDataTable GetTableSchema()
        {

            RimportSPSSConnection conn = this.GetNativeConnection();

            try
            {
                OpenConnection(conn);
                DataTable table = conn.GetSchema("Tables");
                DataSets.TableSchema tableSchema = new Epi.DataSets.TableSchema();
                tableSchema.Merge(table);
                return tableSchema._Tables;
            }
            finally
            {
                CloseConnection(conn);
            }
        }

        #region Native Driver Implementation
        /// <summary>
        /// Returns a native equivalent of a DbParameter
        /// </summary>
        /// <returns>A native equivalent of a DbParameter</returns>
        protected virtual Epi.Data.RimportSPSS.RimportSPSSParameter ConvertToNativeParameter(QueryParameter parameter)
        {
            //TODO: Test this when RimportSPSS comes back on the radar.
            if (parameter.DbType.Equals(DbType.Guid))
            {
                parameter.Value = new Guid(parameter.Value.ToString());
            }

            //return new RimportSPSSParameter(parameter.ParameterName, CovertToNativeDbType(parameter.DbType), parameter.Size, parameter.Direction, parameter.IsNullable, parameter.Precision, parameter.Scale, parameter.SourceColumn, parameter.SourceVersion, parameter.Value);
            return new RimportSPSSParameter(
                parameter.ParameterName, 
                parameter.DbType, 
                parameter.Size, 
                parameter.SourceColumn);
        }

        /// <summary>
        /// Gets a native connection instance based on current ConnectionString value
        /// </summary>
        /// <returns>Native connection object</returns>
        protected virtual RimportSPSSConnection GetNativeConnection()
        {
            return GetNativeConnection(connectionString);
        }

        /// <summary>
        /// Gets a native connection instance from supplied connection string
        /// </summary>
        /// <returns>Native connection object</returns>
        protected virtual RimportSPSSConnection GetNativeConnection(string connectionString)
        {
            return new RimportSPSSConnection(connectionString);
        }

        /// <summary>
        /// Returns a native command object
        /// </summary>
        /// <param name="transaction">Null is not allowed. </param>
        /// <returns></returns>
        protected RimportSPSSCommand GetNativeCommand(IDbTransaction transaction)
        {
            RimportSPSSTransaction RimportSPSStransaction = transaction as RimportSPSSTransaction;

            #region Input Validation
            if (RimportSPSStransaction == null)
            {
                throw new ArgumentException("Transaction parameter must be a RimportSPSSTransaction.", "transaction");
            }
            #endregion

            return new RimportSPSSCommand(null, (RimportSPSSConnection)transaction.Connection, (RimportSPSSTransaction)transaction);

        }

        /// <summary>
        /// Executes a SELECT statement against the database and returns a disconnected data table. NOTE: Use this overload to work with Typed DataSets.
        /// </summary>
        /// <param name="selectQuery">The query to be executed against the database</param>
        /// <param name="dataTable">Table that will contain the result</param>
        /// <returns>A data table object</returns>
        public override DataTable Select(Query selectQuery, DataTable dataTable)
        {
            /*#region Input Validation
            if (selectQuery == null)
            {
                throw new ArgumentNullException("selectQuery");
            }
            if (dataTable == null)
            {
                throw new ArgumentNullException("dataTable");
            }
            #endregion Input Validation

            if (selectQuery.SqlStatement.Contains("TOP 2"))
            {
                selectQuery = CreateQuery(selectQuery.SqlStatement.Replace("TOP 2 ", string.Empty).Replace(";", string.Empty) + " LIMIT 2");
            }

            IDbConnection connection = GetConnection(connectionString);
            RimportSPSSDataAdapter adapter = new RimportSPSSDataAdapter();
            adapter.SelectCommand = (RimportSPSSCommand)GetCommand(selectQuery.SqlStatement, connection, selectQuery.Parameters);

            try
            {
                //Logger.Log(selectQuery);
                adapter.Fill(dataTable.DataSet);
                adapter.FillSchema(dataTable.DataSet, SchemaType.Source);
                return dataTable;
            }
            catch (Exception ex)
            {
                throw new System.ApplicationException("Error executing select query against the database.", ex);
            }*/

            string tableName = selectQuery.SqlStatement.Substring(selectQuery.SqlStatement.ToLower().IndexOf(" from ") + 6).Split(' ')[0].Replace("`", "").Replace("'", "").Replace(";", "").Replace("[", "").Replace("]", "");

            if (selectQuery.SqlStatement.ToLower().Contains("top 2") || selectQuery.SqlStatement.ToLower().Contains("limit 2"))
            {
                //dataTable.Merge(new RimportSPSSWrapper(connectionString).GetFirstDataRow(tableName));
            }
            else
            {
                //dataTable.Merge(new RimportSPSSWrapper(connectionString).GetDataTableAsync(tableName).Result);
            }

            return dataTable;
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

            IDbConnection connection = GetConnection(connectionString);
            RimportSPSSDataAdapter adapter = new RimportSPSSDataAdapter();

            string edittedUpdateQuery = updateQuery.SqlStatement;
            //edittedUpdateQuery = updateQuery.SqlStatement.Replace("@OldValue", "`@OldValue`");
            //edittedUpdateQuery = edittedUpdateQuery.Replace("@NewValue", "`@NewValue`");


            adapter.InsertCommand = (RimportSPSSCommand)GetCommand(insertQuery.SqlStatement, connection, insertQuery.Parameters);
            adapter.UpdateCommand = (RimportSPSSCommand)GetCommand(edittedUpdateQuery, connection, updateQuery.Parameters);

            try
            {
                //Logger.Log(insertQuery);
                //Logger.Log(updateQuery);
                adapter.Update(dataTable.DataSet);
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
        }

        /// <summary>
        /// Updates the foreign and unique keys of a child table with those of the parent via the original keys that existed prior to an import from an Epi Info 3.5.x project.
        /// </summary>        
        public override void UpdateKeys(string childTableName, string parentTableName)
        {
        }

        #endregion

        /// <summary>
        /// Get a count of the number of tables
        /// </summary>
        /// <returns>Integer count of tables</returns>
        public override int GetTableCount()
        {
            DataTable dtSchema = GetTableSchema();
            return dtSchema.Rows.Count;
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
        /// Select statement
        /// </summary>
        /// <param name="selectQuery">Query statement</param>
        /// <returns>DataTable result from query</returns>
        public override System.Data.DataTable Select(Query selectQuery)
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
        /// Checks to see if a given column exists in a given table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <param name="columnName">Name of the column</param>
        /// <returns></returns>
        public override bool ColumnExists(string tableName, string columnName)
        {
            Query query = this.CreateQuery("select * from information_schema.COLUMNS where TABLE_NAME=@TableName and COLUMN_NAME=@ColumnName and table_schema=@dbName;");
            query.Parameters.Add(new QueryParameter("@TableName", DbType.String, tableName));
            query.Parameters.Add(new QueryParameter("@ColumnName", DbType.String, columnName));
            query.Parameters.Add(new QueryParameter("@dbName", DbType.String, this.dbName));
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
        /// Executes a non-query statement
        /// </summary>
        /// <param name="nonQueryStatement">Query to execute</param>
        /// <returns>An integer</returns>
        public override int ExecuteNonQuery(Query nonQueryStatement)
        {
            return 1;
        }

        /// <summary>
        /// Executes a SQL statement and returns total records affected. 
        /// </summary>
        /// <param name="nonQueryStatement">The query to be executed against the database.</param>
        /// <param name="transaction">The transaction to be performed at a data source.</param>
        public override int ExecuteNonQuery(Query nonQueryStatement, System.Data.IDbTransaction transaction)
        {
            return 1;
        }

        /// <summary>
        /// Executes a scalar query against the database
        /// </summary>
        /// <param name="scalarStatement">The query to be executed against the database</param>
        /// <returns></returns>
        public override object ExecuteScalar(Query scalarStatement)
        {
            /*#region Input Validation
            if (scalarStatement == null)
            {
                throw new ArgumentNullException("query");
            }
            #endregion

            object result;
            IDbConnection conn = GetConnection(connectionString);
            IDbCommand command = GetCommand(scalarStatement.SqlStatement, conn, scalarStatement.Parameters)

            try
            {
                OpenConnection(conn);
                //Logger.Log(scalarStatement);
                result = command.ExecuteScalar();
            }
            finally
            {
                CloseConnection(conn);
            }

            return result;*/

            string tableName = scalarStatement.SqlStatement.Substring(scalarStatement.SqlStatement.ToLower().IndexOf(" from ") + 6).Split(' ')[0].Replace("`", "").Replace("'", "").Replace(";", "").Replace("[", "").Replace("]", "");

            if (scalarStatement.SqlStatement.ToLower().Contains(" count(") || scalarStatement.SqlStatement.ToLower().Contains(" count "))
            {
                return 0;// new RimportSPSSWrapper(connectionString).GetCollectionSize(tableName).Result;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Executes a scalar query against the database using an existing transaction
        /// </summary>
        /// <param name="scalarStatement">The query to be executed against the database</param>
        /// <param name="transaction">Null is allowed</param>
        /// <returns></returns>
        public override object ExecuteScalar(Query scalarStatement, System.Data.IDbTransaction transaction)
        {
            #region Input Validation
            if (scalarStatement == null)
            {
                throw new ArgumentNullException("query");
            }
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            #endregion

            object result;
            IDbCommand command = GetCommand(scalarStatement.SqlStatement, transaction, scalarStatement.Parameters);

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
        /// Delete a specific table in the database
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>Boolean</returns>
        public override bool DeleteTable(string tableName)
        {
            Query query = this.CreateQuery("Drop Table @Name");
            query.Parameters.Add(new QueryParameter("@Name", DbType.String, tableName));
            return (ExecuteNonQuery(query) > 0);
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
            return new RimportSPSSWrapper(connectionString).GetTableNames().Contains(tableName);
        }

        /// <summary>
        /// Gets column schema information about an RimportSPSS table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>DataTable with schema information</returns>
        public override Epi.DataSets.TableColumnSchema.ColumnsDataTable GetTableColumnSchema(string tableName)
        {
            DataTable table = this.GetSchema("Columns", tableName);
            DataTable tableToMerge = table.Copy();
            tableToMerge.Rows.Clear();

            string isNullableCol = table.Rows[0].ItemArray[6].ToString();
            if (isNullableCol.Equals("YES"))
            {
                
            }
            else
            {
                tableToMerge.Rows[0].ItemArray[6] = "false";
                tableToMerge.Rows[0].ItemArray[6] = Boolean.Parse(tableToMerge.Rows[0].ItemArray[6].ToString());
                
            }

            //string datatype = table.Rows[0].ItemArray[7].ToString();
            DataSets.TableColumnSchema tableColumnSchema = new Epi.DataSets.TableColumnSchema();
            

            //string dc = tableColumnSchema.Columns.IS_NULLABLEColumn.DataType.FullName;

            tableColumnSchema.Merge(tableToMerge);
            return tableColumnSchema.Columns;
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
                throw new GeneralException("Could not get table column schema for." + tableName, ex);
            }
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

            RimportSPSSConnection conn = this.GetNativeConnection();

            try
            {
                OpenConnection(conn);
                return conn.GetSchema(collectionName, new string[] { null, null, tableName, null });
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

            RimportSPSSConnection conn = this.GetNativeConnection();

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
        /// Gets Primary_Keys schema information about a RimportSPSS table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>DataTable with schema information</returns>
        public override Epi.DataSets.TableKeysSchema.Primary_KeysDataTable GetTableKeysSchema(string tableName)
        {
            DataSets.TableKeysSchema schema = new Epi.DataSets.TableKeysSchema();
            RimportSPSSConnection conn = this.GetNativeConnection();
            bool alreadyOpen = (conn.State != ConnectionState.Closed);
            try
            {
                if (!alreadyOpen)
                {
                    OpenConnection(conn);
                }
                string query = "select KU.TABLE_CATALOG, KU.TABLE_SCHEMA, KU.TABLE_NAME, COLUMN_NAME, " +
                              "0 as COLUMN_PROPID, ORDINAL_POSITION as ORDINAL, KU.CONSTRAINT_NAME as PK_NAME " +
                              "from INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS TC " +
                              "inner join " +
                              "INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KU " +
                              "on TC.CONSTRAINT_TYPE = 'primary key' and " +
                              "TC.CONSTRAINT_NAME = KU.CONSTRAINT_NAME " +
                              "where KU.TABLE_NAME = '" + tableName +
                              "' order by KU.ORDINAL_POSITION;";
                RimportSPSSDataAdapter da = new RimportSPSSDataAdapter(query, conn);
                //Logger.Log(query);
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

        /// <summary>
        /// Gets the contents of a table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>Datatable containing the table data</returns>
        public override System.Data.DataTable GetTableData(string tableName)
        {
            return GetTableData(tableName, string.Empty, string.Empty);
        }

        /// <summary>
        /// Returns contents of a table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnNames">Comma delimited string of column names and ASC/DESC order</param>
        public override System.Data.DataTable GetTableData(string tableName, string columnNames)
        {
            return GetTableData(tableName, columnNames, string.Empty);
        }

        /// <summary>
        ///  Returns contents of a table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnNames"></param>
        /// <param name="sortCriteria">Comma delimited string of column names and ASC/DESC order</param>
        /// <returns>DataTable</returns>
        public override System.Data.DataTable GetTableData(string tableName, string columnNames, string sortCriteria)
        {
            /*
            #region Input Validation
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }
            #endregion
            try
            {
                if (string.IsNullOrEmpty(columnNames))
                {
                    columnNames = Epi.StringLiterals.STAR;
                }
                string queryString = "select " + columnNames + " from [" + tableName + "]";
                if (!string.IsNullOrEmpty(sortCriteria))
                {
                    queryString += " order by " + sortCriteria;
                }
                Query query = this.CreateQuery(queryString);
                return Select(query);
            }
            finally
            {
            }*/

            //return new RimportSPSSWrapper(connectionString).GetDataTableAsync(tableName).Result;

            throw new NotImplementedException("Jeremy");
        }

        /// <summary>
        /// Returns contents of a table with only the top two rows.
        /// </summary>
        /// <param name="tableName">The name of the table to query</param>
        /// <returns>DataTable</returns>
        public override DataTable GetTopTwoTable(string tableName)
        {
            #region Input Validation
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }
            #endregion
            try
            {   
                string queryString = "select * from [" + tableName + "] limit 2";                
                Query query = this.CreateQuery(queryString);
                return Select(query);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Create a reader from a table
        /// </summary>
        /// <param name="tableName">name of table</param>
        /// <returns>IDataReader reader</returns>
        public override System.Data.IDataReader GetTableDataReader(string tableName)
        {
            #region Input Validation
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }
            #endregion
            Query query = this.CreateQuery("select * from " + tableName);
            return this.ExecuteReader(query);
        }

        /// <summary>
        /// Executes a query and returns a stream of rows
        /// </summary>
        /// <param name="selectQuery">The query to be executed against the database</param>
        /// <returns>A data reader object</returns>
        public override System.Data.IDataReader ExecuteReader(Query selectQuery)
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
        /// <param name="commandBehavior"></param>
        /// <returns>A data reader object</returns>
        public override System.Data.IDataReader ExecuteReader(Query selectQuery, CommandBehavior commandBehavior)
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
        /// Get the column names for a table
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>List of column names</returns>
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
        /// Get ColumnNames by table name
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <returns>DataView</returns>
        public override System.Data.DataView GetTextColumnNames(string tableName)
        {
            return new DataView(this.GetSchema("Columns", tableName));
        }

        /// <summary>
        /// Opens a database connection and begins a new transaction
        /// </summary>
        /// <returns>A specialized transaction object based on the current database engine type</returns>
        public override System.Data.IDbTransaction OpenTransaction()
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
        public override System.Data.IDbTransaction OpenTransaction(System.Data.IsolationLevel isolationLevel)
        {
            IDbConnection connection = GetConnection();
            OpenConnection(connection);
            return connection.BeginTransaction(isolationLevel);
        }

        /// <summary>
        /// Close a specific connection if state is not already closed
        /// </summary>
        /// <param name="transaction"></param>
        public override void CloseTransaction(System.Data.IDbTransaction transaction)
        {
            #region Input Validation
            if (transaction == null)
            {
                throw new ArgumentNullException("Transaction cannot be null.", "transaction");
            }
            #endregion

            CloseConnection(transaction.Connection);
        }


        /// <summary>
        /// Gets the names of all tables in the database
        /// </summary>
        /// <returns>Names of all tables in the database</returns>
        public override List<string> GetTableNames()
        {
            /*List<string> tableNames = new List<string>();

            Epi.DataSets.TableSchema.TablesDataTable tableSchema = GetTableSchema();

            //DataTable schemaTable = Select(this.CreateQuery("use " + this.DbName + "; SELECT * FROM sys_catalogs;"));

            foreach (DataRow row in tableSchema.Rows)
            {
                tableNames.Add(row[tableSchema.TABLE_NAMEColumn].ToString());
            }
            return tableNames;*/
            return new RimportSPSSWrapper(connectionString).GetTableNames();

        }

        /// <summary>
        /// Create RimportSPSS query object
        /// </summary>
        /// <param name="ansiSqlStatement">Query string</param>
        /// <returns>Query</returns>
        public override Query CreateQuery(string ansiSqlStatement)
        {
            return new RimportSPSSQuery(ansiSqlStatement);
        }

        /// <summary>
        ///  Opens specific connection if state is not already opened
        /// </summary>
        /// <param name="conn"></param>
        protected void OpenConnection(IDbConnection conn)
        {
            /*
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
            */
        }

        /// <summary>
        /// Close a specific connection if state is not already closed
        /// </summary>
        /// <param name="conn"></param>
        protected void CloseConnection(IDbConnection conn)
        {
            /*
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
            */
        }



        /// <summary>
        /// Gets a new command using an existing transaction
        /// </summary>
        /// <param name="sqlStatement">The query to be executed against the database</param>
        /// <param name="transaction">Transction</param>
        /// <param name="parameters">parameters">Parameters for the query to be executed</param>
        /// <returns>An RimportSPSS command object</returns>
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
        /// <param name="connection">Connection</param>
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
        /// Gets the code table names for the project.
        /// </summary>
        /// <param name="project"><see cref="Epi.Project"/></param>
        /// <returns><see cref="System.Data.DataTable"/></returns>
        public override DataTable GetCodeTableNamesForProject(Project project)
        {
            List<string> tables = project.GetDataTableList();
            DataSets.TableSchema.TablesDataTable codeTables = project.GetCodeTableList();
            foreach (DataSets.TableSchema.TablesRow row in codeTables)
            {
                tables.Add(row.TABLE_NAME);
            }

            DataView dataView = codeTables.DefaultView;

            return codeTables;
        }

        /// <summary>
        /// Gets a list of code tables.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public override Epi.DataSets.TableSchema.TablesDataTable GetCodeTableList(IDbDriver db)
        {
            Epi.DataSets.TableSchema.TablesDataTable table = db.GetTableSchema();
            DataRow[] rowsFiltered = table.Select("TABLE_NAME not like 'code%'");
            //remove tables that don't start with "code"
            foreach (DataRow rowFiltered in rowsFiltered)
            {
                table.Rows.Remove(rowFiltered);
            }
            //remove the code tables that are not for the current database
            DataRow[] rowsFilteredSchema = table.Select("TABLE_SCHEMA<>'" + db.DbName + "'");

            foreach (DataRow rowFilteredSchema in rowsFilteredSchema)
            {
                table.Rows.Remove(rowFilteredSchema);
            }

            return table;
        }
        /// <summary>
        /// Identity which type of database drive is in use.
        /// </summary>
        /// <returns>RimportSPSS</returns>
        public override string IdentifyDatabase()
        {
            return "RimportSPSS";
        }

        /// <summary>
        /// Inserts the string in escape characters. [] for SQL server and `` for RimportSPSS etc.
        /// </summary>
        /// <param name="str">string</param>
        /// <returns>string</returns>
        public override string InsertInEscape(string str)
        {
            string newString = string.Empty;
            if (!str.StartsWith(StringLiterals.BACK_TICK))
            {
                newString = StringLiterals.BACK_TICK;
            }
            newString += str;
            if (!str.EndsWith(StringLiterals.BACK_TICK))
            {
                newString += StringLiterals.BACK_TICK;
            }
            return newString;
        }

        /// <summary>
        /// Provides a database frieldly string representation of date.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public override string FormatDate(DateTime dt)
        {
            string formattedString = dt.ToString("u");
            // Remove time part of the string.
            formattedString = formattedString.Remove(formattedString.IndexOf(StringLiterals.SPACE));
            return Util.InsertInSingleQuotes(formattedString);
        }

        /// <summary>
        /// Provides a database frieldly string representation of date.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public override string FormatTime(DateTime dt)
        {
            string formattedString = dt.ToString("u");
            // Remove the date part
            formattedString = formattedString.Remove(0, formattedString.IndexOf(StringLiterals.SPACE));
            // Remove the trailing 'Z' character.
            formattedString = formattedString.Remove(formattedString.Length - 1);
            return Util.InsertInSingleQuotes(formattedString);
        }

        /// <summary>
        /// Provides a database frieldly string representation of date.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public override string FormatDateTime(DateTime dt)
        {
            string formattedString = dt.ToString("u");
            // Remove the trailing 'Z' character.
            formattedString = formattedString.Remove(formattedString.Length - 1);
            return Util.InsertInSingleQuotes(formattedString);
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
                    case DbType.Boolean:
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

        public override System.Data.Common.DbDataAdapter GetDbAdapter(string p)
        {
            throw new NotImplementedException();
        }

        public override System.Data.Common.DbCommandBuilder GetDbCommandBuilder(System.Data.Common.DbDataAdapter Adapter)
        {
            throw new NotImplementedException();
        }

        public override bool InsertBulkRows(string tableName, System.Data.Common.DbDataReader reader, SetGadgetStatusHandler pStatusDelegate = null, CheckForCancellationHandler pCancellationDelegate = null)
        {

            bool result = false;
            RimportSPSSWrapper wrapper = new RimportSPSSWrapper(connectionString);
            DataSet dataSet = new DataSet();
            DataTable Temp = new DataTable();

            try
            {
                int rowCount = 0;
                int skippedRows = 0;
                int totalRows = 0;

                if (pStatusDelegate != null && dataSet.Tables.Count > 0)
                {
                    totalRows = dataSet.Tables[0].Rows.Count;
                }

                while (reader.Read())
                {
                    //BsonDocument document = new BsonDocument();

                    //for (int y = 0; y < reader.FieldCount; y++)
                    //{
                    //    if (!reader.IsDBNull(y))
                    //    {
                    //        if (reader.GetFieldType(y) == typeof(double) || reader.GetFieldType(y) == typeof(Single) || reader.GetFieldType(y) == typeof(decimal))
                    //            document.Add(reader.GetName(y), new BsonDouble(double.Parse(reader[y].ToString())));
                    //        else if (reader.GetFieldType(y) == typeof(int))
                    //            document.Add(reader.GetName(y), new BsonInt32((int)reader[y]));
                    //        else if (reader.GetFieldType(y) == typeof(bool))
                    //            document.Add(reader.GetName(y), new BsonBoolean((bool)reader[y]));
                    //        else if (reader.GetFieldType(y) == typeof(DateTime))
                    //            document.Add(reader.GetName(y), new BsonDateTime((DateTime)reader[y]));
                    //        else
                    //            document.Add(reader.GetName(y), new BsonString(reader[y].ToString()));
                    //    }
                    //}

                    try
                    {
                        //wrapper.Insert(document, tableName);
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

            result = true;
            return result;
        }


        public override bool Insert_1_Row(string pSelectSQL, System.Data.Common.DbDataReader pDataReader)
        {
            throw new NotImplementedException();
        }

        public override bool Update_1_Row(string pSelectSQL, string pKeyString, System.Data.Common.DbDataReader pDataReader)
        {
            throw new NotImplementedException();
        }

        public override System.Data.Common.DbCommand GetCommand(string pKeyString, DataTable pDataTable)
        {
            throw new NotImplementedException();
        }
    }
}