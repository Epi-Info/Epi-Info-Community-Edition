using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using Epi.Collections;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using Epi;
using Epi.Data;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Linq;
using System.Data.SQLite;
using System.Data.SqlClient;

namespace Epi.Data.SQLite
{
    /// <summary>
    /// Jet based databases
    /// </summary>
    public abstract class OleDbDatabase : DbDriverBase
    {
        private bool isBulkOperation;
        private DataTable schemaCols;

        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public OleDbDatabase(){ }

        #endregion Constructors

        /// <summary>
        /// Determines the level of rights the user has on the SQL database
        /// </summary>
        public override ProjectPermissions GetPermissions()
        {
            return null;
        }

        #region Native Driver Implementation
        /// <summary>
        /// Returns a native equivalent of a DbParameter
        /// </summary>
        /// <returns>Native equivalent of a DbParameter</returns>
        protected virtual OleDbParameter ConvertToNativeParameter(QueryParameter parameter)
        {
            if (parameter.DbType.Equals(DbType.Guid))
            {
                parameter.Value = new Guid(parameter.Value.ToString());
            }

            OleDbParameter param = new OleDbParameter
                (
                    parameter.ParameterName, 
                    CovertToNativeDbType(parameter.DbType), 
                    parameter.Size, 
                    parameter.Direction, 
                    parameter.IsNullable, 
                    parameter.Precision, 
                    parameter.Scale, 
                    parameter.SourceColumn, 
                    parameter.SourceVersion, 
                    parameter.Value
                );

            return param; 
        }

        /// <summary>
        /// Gets a native connection instance based on current ConnectionString value
        /// </summary>
        /// <returns>Native connection object</returns>
        protected virtual OleDbConnection GetNativeConnection()
        {
            return GetNativeConnection(connectionString);
        }

        /// <summary>
        /// Gets a native connection instance from supplied connection string
        /// </summary>
        /// <returns>Native connection object</returns>
        protected virtual OleDbConnection GetNativeConnection(string connectionString)
        {
            OleDbConnectionStringBuilder oleDBCnnStrBuilder = new OleDbConnectionStringBuilder(connectionString);
            oleDBCnnStrBuilder.Provider = "Epi.Data.SQLite.1.0.0.0";

            return new OleDbConnection(oleDBCnnStrBuilder.ToString());
        }

        /// <summary>
        /// Returns a native command object
        /// </summary>
        /// <param name="transaction">Null is not allowed.</param>
        /// <returns></returns>
        protected OleDbCommand GetNativeCommand(IDbTransaction transaction)
        {
            OleDbTransaction oleDbtransaction = transaction as OleDbTransaction;

            #region Input Validation
            if (oleDbtransaction == null)
            {
                throw new ArgumentException("Transaction parameter must be a OleDbTransaction.", "transaction");
            }
            #endregion

            return new OleDbCommand(null, (OleDbConnection)transaction.Connection, (OleDbTransaction)transaction);

        }

        /// <summary>
        /// Executes a SELECT statement against the database and returns a disconnected data table. NOTE: Use this overload to work with Typed DataSets.
        /// </summary>
        /// <param name="selectQuery">The query to be executed against the database</param>
        /// <param name="dataTable">Table that will contain the result.</param>
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

            try
            {
                string filestring = this.ConnectionString.Substring(this.ConnectionString.IndexOf("Source=") + 7);
                using (SQLiteConnection sqlite = new SQLiteConnection("Data Source=" + filestring))
                {
                    sqlite.Open();
                    SQLiteCommand sqlite_command = sqlite.CreateCommand();
                    sqlite_command.CommandText = selectQuery.SqlStatement;
                    foreach (QueryParameter oparam in selectQuery.Parameters)
                    {
                        sqlite_command.Parameters.Add(new SQLiteParameter(oparam.ParameterName, oparam.Value));
                    }
                    try
                    {
                        SQLiteDataReader reader = sqlite_command.ExecuteReader();
                        try
                        {
                            dataTable.Load(reader);
                        }
                        catch (Exception rex)
                        {
                            throw rex;
                        }
                        finally
                        {
                            reader.Close();
                        }
                        DataTable dtClone = dataTable.Clone();
                        foreach (DataColumn dc in dtClone.Columns)
                        {
                            if (dc.DataType == typeof(Int64))
                            {
                                dc.DataType = typeof(Int32);
                            }
                        }
                        foreach (DataRow dr in dataTable.Rows)
                            dtClone.ImportRow(dr);
                        return dtClone;
                    }
                    catch (SQLiteException sqex)
                    {
                        throw sqex;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        sqlite.Close();
                    }
                }
            }
            catch (SQLiteException sqlex)
            {
                throw sqlex;
            }

            IDbConnection connection = GetConnection();

            if (connection.ConnectionString.Contains("Provider=Microsoft.Jet.OLEDB.4.0"))
            {
                string newString = connection.ConnectionString.Replace("HDR=Yes", "HDR=Yes;IMEX=1");
                connection.ConnectionString = newString;
            }

            OleDbDataAdapter adapter = new OleDbDataAdapter();
            adapter.SelectCommand = (OleDbCommand)GetCommand(selectQuery.SqlStatement, connection, selectQuery.Parameters);

            try
            {
                adapter.Fill(dataTable);
                try
                {
                    adapter.FillSchema(dataTable, SchemaType.Source);
                }
                catch { }

                return dataTable;
            }
            catch (OleDbException oleDbException)
            {
                Configuration config0 = Configuration.GetNewInstance();
                string wd = config0.Directories.Working;
                connection.ConnectionString = connection.ConnectionString.Replace("Data Source=", "Data Source=" + wd + "\\");
                adapter.SelectCommand = (OleDbCommand)GetCommand(selectQuery.SqlStatement, connection, selectQuery.Parameters);
                try
                {
                    adapter.Fill(dataTable);
                    try
                    {
                        adapter.FillSchema(dataTable, SchemaType.Source);
                    }
                    catch { }

                    return dataTable;
                }
                catch (OleDbException oleDbException2)
                {
                    throw oleDbException2;
                }
                catch (Exception ex)
                {
                    throw new System.ApplicationException(SharedStrings.ERROR_SELECT_QUERY_DATA_SOURCE, ex);
                }
            }
            catch (Exception ex)
            {
                throw new System.ApplicationException(SharedStrings.ERROR_SELECT_QUERY_DATA_SOURCE, ex);
            }
        }

        /// <summary>
        /// Warning! This method does not support transactions!
        /// </summary>
        /// <param name="dataTable">Table that will contain the result.</param>
        /// <param name="tableName">Name of table to update.</param>
        /// <param name="insertQuery">SQL statement and parameters for inserting row.</param>
        /// <param name="updateQuery">SQL statement and parameters for updating row.</param>
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

            #endregion Input Validation


            IDbConnection connection = GetConnection();
            OleDbDataAdapter adapter = new OleDbDataAdapter();

            if (insertQuery != null)
            {                    
                adapter.InsertCommand = (OleDbCommand)GetCommand(insertQuery.SqlStatement, connection, insertQuery.Parameters);
            }
                
            if (updateQuery != null)
            {
                adapter.UpdateCommand = (OleDbCommand)GetCommand(updateQuery.SqlStatement, connection, updateQuery.Parameters);
            }

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
            sb.Append(" INNER JOIN ");
            sb.Append(parentTableName);
            sb.Append(" ON [");
            sb.Append(childTableName);
            sb.Append("].[OldFKEY] = [");
            sb.Append(parentTableName);
            sb.Append("].[OldUniqueKey]");
            sb.Append(" SET [");
            sb.Append(childTableName);
            sb.Append("].[FKEY] = [");
            sb.Append(parentTableName);
            sb.Append("].[GlobalRecordId]");

            ExecuteNonQuery(CreateQuery(sb.ToString())); 
        }

        /// <summary>
        /// Updates the foreign and unique keys of a child table with those of the parent via the original keys that existed prior to an import from an Epi Info 3.5.x project.
        /// </summary>        
        public override void UpdateKeys(string childTableName, string parentTableName)
        {
            //StringBuilder sb = new StringBuilder();

            //sb.Append("UPDATE ");
            //sb.Append(childTableName);
            //sb.Append(" LEFT JOIN ");            
            //sb.Append(parentTableName);
            //sb.Append(" ON ");
            //sb.Append(childTableName);
            //sb.Append(".OldFKEY = ");
            //sb.Append(parentTableName);
            //sb.Append(".OldUniqueKey");
            //sb.Append(" SET ");
            //sb.Append(childTableName);
            //sb.Append(".FKEY = [");
            //sb.Append(parentTableName);
            //sb.Append("].[UniqueKey];");

            //ExecuteNonQuery(CreateQuery(sb.ToString())); 
        }

        #endregion

        #region Temporary junk pile

        /// <summary>
        /// returns value to indicate if connection string has a password associated with it
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        protected virtual bool HasPassword(string connectionString)
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder(this.connectionString);
                return builder.ContainsKey("Jet OLEDB:Database Password");
            }
            else
            {
                return false;
            }
        }
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
        }  
        
        private string dbName = string.Empty;
        private string location;
        private string connectionString;

        /// <summary>
        /// Gets or sets the location of the database
        /// </summary>
        protected string Location
        {
            get
            {
                return location;
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
        /// Data Source location
        /// </summary>
        public override string DataSource
        {
            get
            {
                string[] strings = ConnectionString.Split(';');
                string dataSource;
                if (strings.Length > 1)
                {
                    dataSource = strings[1].Replace("Data Source=", "");
                }
                else
                {
                    dataSource = ConnectionString.Replace("Data Source=", "");
                }
                dataSource = dataSource.Replace("\"", "");
                return dataSource;
            }
        }
        /// <summary>
        /// Connection string attribute
        /// </summary>
        public override string ConnectionString
        {
            get
            {
                // Asad (4/22/2010): Why have this logic?
                //if (HasPassword(this.connectionString))
                //{
                //    return this.connectionString;
                //}
                //else
                //{
                //    if (this.location == null)
                //    {
                //        return this.connectionString;
                //    }
                //    else
                //    {
                //        return this.location;
                //    }
                //}

                return this.connectionString;
            }
            set
            {
                this.connectionString = value;
                ProcessConnectionString();
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
                if (ConnectionString.Contains("="))
                {
                    return ConnectionString;
                }
                else
                {
                    return "Provider=Epi.Data.SQLite.1.0.0.0;Data Source=" + ConnectionString + ";";
                }
            }
        }

        /// <summary>
        /// Returns the maximum number of columns a table can have.
        /// </summary>
        public override int TableColumnMax
        {
            get { return 250; }
        }

        /// <summary>
        /// Extracts file location and replaces relative file paths with full file paths by updating ConnectionString value
        /// </summary>
        protected void ProcessConnectionString()
        {
            this.location = null;
            if (this.connectionString == null) return;

            OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder(this.connectionString);

            string filename = builder.DataSource;
            if (File.Exists(filename))
            {
                if (!Path.IsPathRooted(filename))
                {
                    filename = Path.GetFullPath(filename);
                }
                this.location = filename;
                this.DbName = Path.GetFileNameWithoutExtension(this.location);
                this.connectionString = builder.ToString();
            }
        }
        /// <summary>
        /// Gets the Access version of a generic DbType
        /// </summary>
        /// <returns>Access version of the generic DbType</returns>
        protected virtual OleDbType CovertToNativeDbType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                    return OleDbType.VarChar;
                case DbType.AnsiStringFixedLength:
                    return OleDbType.Char;
                case DbType.Binary:
                    return OleDbType.Binary;
                case DbType.Boolean:
                    return OleDbType.Boolean;
                case DbType.Byte:
                    return OleDbType.UnsignedTinyInt;
                case DbType.Currency:
                    return OleDbType.Currency;
                case DbType.Date:
                    return OleDbType.DBDate;
                case DbType.DateTime:
                    return OleDbType.DBTimeStamp;
                case DbType.Decimal:
                    return OleDbType.Decimal;
                case DbType.Double:
                    return OleDbType.Double;
                case DbType.Guid:
                    return OleDbType.Guid;
                case DbType.Int16:
                    return OleDbType.SmallInt;
                case DbType.Int32:
                    return OleDbType.Integer;
                case DbType.Int64:
                    return OleDbType.BigInt;
                case DbType.Object:
                //  return OleDbType.VarChar;
                    return OleDbType.Binary;
                case DbType.SByte:
                    return OleDbType.TinyInt;
                case DbType.Single:
                    return OleDbType.Single;
                case DbType.String:
                    return OleDbType.VarWChar;
                case DbType.StringFixedLength:
                    return OleDbType.WChar;
                case DbType.Time:
                    return OleDbType.DBTimeStamp;
                case DbType.UInt16:
                    return OleDbType.UnsignedSmallInt;
                case DbType.UInt32:
                    return OleDbType.UnsignedInt;
                case DbType.UInt64:
                    return OleDbType.UnsignedBigInt;
                case DbType.VarNumeric:
                    return OleDbType.VarNumeric;
                default:
                    return OleDbType.VarChar;
            }
        }


        /// <summary>
        /// Set Access mdb file path
        /// </summary>
        /// <param name="filePath"></param>
        public abstract void SetDataSourceFilePath(string filePath);

        /// <summary>
        /// Create a Jet database specific query object.
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <returns></returns>
        public override Query CreateQuery(string sqlStatement)
        {
            if (sqlStatement.ToLowerInvariant().StartsWith("select top 2"))
                sqlStatement = sqlStatement.Substring(0, 6) + sqlStatement.Substring(12) + " LIMIT 2";
            return new JetQuery(sqlStatement);
        }

        #endregion Public Methods

        #region Schema and DDL Support
        /// <summary>
        /// Get total count of table
        /// </summary>
        /// <returns></returns>
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
        /// Get column names of a table
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public override List<string> GetTableColumnNames(string tableName)
        {
            DataTable table = this.GetSchema("Columns", tableName);
            List<string> list = new List<string>();
            foreach (DataRow row in table.Rows)
            {
                list.Add(row["COLUMN_NAME"].ToString());
            }
            if (this.ConnectionDescription.ToLowerInvariant().Contains("json"))
            {
                table = GetTableData(tableName);
                list.Clear();
                foreach (DataColumn jcol in table.Columns)
                    list.Add(jcol.ColumnName);
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
        /// Delete a specific table in the database
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>Boolean</returns>
        public override bool DeleteTable(string tableName)
        {
            Query query = this.CreateQuery("Drop Table [" + tableName + "]");
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
            Query query = this.CreateQuery("ALTER TABLE " + tableName + " DROP COLUMN [" + columnName + "]");
            return (ExecuteNonQuery(query) > 0);
        }

        /// <summary>
        /// Gets a value indicating whether or not a specific table exists in the database
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>Boolean</returns>
        public override bool TableExists(string tableName)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }
            #endregion
            
            // The following query is used instead of the schema
            // in order to handle linked tables in Access.
            
            try
            {
                Select(this.CreateQuery("SELECT * FROM [" + tableName + "] LIMIT 1;"));
            }
            catch (SQLiteException)
            {
                return false; 
            }
            catch (Exception) 
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets a value indicating whether or not a specific column exists for a table in the database
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <param name="columnName">Name of the column</param>
        /// <returns>Boolean</returns>
        public override bool ColumnExists(string tableName, string columnName)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentNullException("columnName");
            }
            bool retval = false;
            string filestring = this.ConnectionString.Substring(this.ConnectionString.IndexOf("Source=") + 7);
            if (filestring.EndsWith(";user id=admin"))
                filestring = filestring.Replace(";user id=admin", "");
            using (SQLiteConnection sqlite = new SQLiteConnection("Data Source=" + filestring))
            {
                sqlite.Open();
                SQLiteCommand sqlite_command = sqlite.CreateCommand();
                sqlite_command.CommandText = "SELECT " + columnName + " FROM " + tableName + " LIMIT 1;";
                try
                {
                    SQLiteDataReader reader = sqlite_command.ExecuteReader();
                    while (reader.Read()) { }
                    reader.Close();
                    retval = true;
                }
                catch (SQLiteException)
                {
                    retval = false;
                }
                finally
                {
                    sqlite.Close();
                }
            }
            return retval;
            #endregion
            OleDbConnection conn = this.GetNativeConnection();

            try
            {
                OpenConnection(conn);

                object[] objTable;
                objTable = new object[] { null, null, tableName, null };
                if (schemaCols == null || !IsBulkOperation)
                {
                    schemaCols = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, objTable);
                }
                bool exists = schemaCols.Select("COLUMN_NAME = '" + columnName.Replace("'", "''") + "'").Length > 0;
                return exists;
            }
            finally
            {
                CloseConnection(conn);
            }
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
        /// Return column names of table as dataview 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public override DataView GetTextColumnNames(string tableName)
        {
            return new DataView(this.GetSchema("Columns", tableName));
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
            #region Input Validation
            if (string.IsNullOrEmpty(collectionName))
            {
                throw new ArgumentNullException("collectionName");
            }
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }
            #endregion
            string filestring = this.ConnectionString.Substring(this.ConnectionString.IndexOf("Source=") + 7);
            using (SQLiteConnection sqlite = new SQLiteConnection("Data Source=" + filestring))
            {
                sqlite.Open();
                try
                {
                    DataTable table = sqlite.GetSchema("Columns", new string[] { null, null, tableName, null });
                    DataSets.TableSchema tableSchema = new Epi.DataSets.TableSchema();
                    tableSchema.Merge(table);
                    return table;
                }
                catch (SQLiteException sqex)
                {
                    throw sqex;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    sqlite.Close();
                }
            }

            OleDbConnection conn = this.GetNativeConnection();

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

            OleDbConnection conn = this.GetNativeConnection();

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
        /// Gets table schema information about an OLE database
        /// </summary>
        /// <returns>DataTable with schema information</returns>
        public override DataSets.TableSchema.TablesDataTable GetTableSchema()
        {
            string filestring = this.ConnectionString.Substring(this.ConnectionString.IndexOf("Source=") + 7);
            using (SQLiteConnection sqlite = new SQLiteConnection("Data Source=" + filestring))
            {
                sqlite.Open();
                try
                {
                    DataTable table = sqlite.GetSchema("Tables");
                    DataSets.TableSchema tableSchema = new Epi.DataSets.TableSchema();
                    tableSchema.Merge(table);
                    return tableSchema._Tables;
                }
                catch (SQLiteException sqex)
                {
                    throw sqex;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    sqlite.Close();
                }
            }
                OleDbConnection conn = this.GetNativeConnection();

            try
            {
                OpenConnection(conn);
                DataTable table = conn.GetSchema("Tables", new string[] { null, null, null, "Table" });
                DataSets.TableSchema tableSchema = new Epi.DataSets.TableSchema();
                tableSchema.Merge(table);
                return tableSchema._Tables;
            }
            finally
            {
                CloseConnection(conn);
            }
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
                string tableNameCandidate = row[ColumnNames.SCHEMA_TABLE_NAME].ToString();
                tableNames.Add(row[ColumnNames.SCHEMA_TABLE_NAME].ToString());
            }
            return tableNames;
        }

        /// <summary>
        /// Gets Primary_Keys schema information about an OLE table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>DataTable with schema information</returns>
        public override DataSets.TableKeysSchema.Primary_KeysDataTable GetTableKeysSchema(string tableName)
        {

            OleDbConnection conn = this.GetNativeConnection();

            try
            {
                OpenConnection(conn);
                DataTable tb = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Primary_Keys, new object[] { null, null, tableName });
                DataSets.TableKeysSchema schema = new Epi.DataSets.TableKeysSchema();
                schema.Merge(tb);
                return schema.Primary_Keys;
            }
            finally
            {
                CloseConnection(conn);
            }
        }

        /// <summary>
        /// Gets whether or not the database format is valid. Used for error checking on file-based database types.
        /// </summary>
        /// <returns>bool</returns>
        public override bool IsDatabaseFormatValid(ref string exceptionMessage)
        {
            OleDbConnection conn = this.GetNativeConnection();

            try
            {
                OpenConnection(conn);                                
            }
            catch (Exception ex)
            {
                exceptionMessage = ex.Message;
                return false;
            }
            finally
            {
                CloseConnection(conn);
            }

            exceptionMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Gets column schema information about an OLE table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>DataTable with schema information</returns>
        public override DataSets.TableColumnSchema.ColumnsDataTable GetTableColumnSchema(string tableName)
        {
            OleDbConnection conn = this.GetNativeConnection();

            try
            {
                OpenConnection(conn);
                DataTable dt = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { null, null, tableName, null });
                DataSets.TableColumnSchema schema = new Epi.DataSets.TableColumnSchema();
                schema.Merge(dt);
                return schema.Columns;
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not get table column schema for." + tableName, ex);
            }
            finally
            {
                CloseConnection(conn);
            }
        }

        /// <summary>
        /// Gets column schema information about an OLE table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>DataTable with schema information</returns>
        public override DataSets.ANSI.TableColumnSchema.ColumnsDataTable GetTableColumnSchemaANSI(string tableName)
        {
            OleDbConnection conn = this.GetNativeConnection();

            try
            {
                OpenConnection(conn);
                DataTable dt = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { null, null, tableName, null });
                DataSets.ANSI.TableColumnSchema schema = new Epi.DataSets.ANSI.TableColumnSchema();
                schema.Merge(dt);
                return schema.Columns;
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not get table column schema for." + tableName, ex);
            }
            finally
            {
                CloseConnection(conn);
            }
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
                throw new GeneralException(SharedStrings.ERROR_CONNECT_DATA_SOURCE, ex);
            }
        }

        /// <summary>
        /// Tests database connectivity using supplied connection string 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        protected bool TestConnection(string connectionString)
        {
            string filestring = connectionString.Substring(connectionString.IndexOf("Source=") + 7);
            using (SQLiteConnection sqlite = new SQLiteConnection("Data Source=" + filestring))
            {
                try
                {
                    sqlite.Open();
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    sqlite.Close();
                }
                return true;
            }
            IDbConnection testConnection = GetConnection(connectionString);
            try
            {
                OpenConnection(testConnection);
            }
            finally
            {
                CloseConnection(testConnection);
            }
            return false;
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
        /// <param name="sqlStatement">The sql to be executed against the database</param>
        /// <param name="transaction"></param>
        /// <param name="parameters">Parameters for the query to be executed</param>
        /// <returns></returns>
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
        /// Gets a new command using an existing connection
        /// </summary>
        /// <param name="sqlStatement">The query to be executed against the database</param>
        /// <param name="connection">Parameters for the query to be executed</param>
        /// <param name="parameters">An OleDb command object</param>
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
                Logger.Log(DateTime.Now + ":  " + ex.Message);
                if (ex.Message.Contains("Unrecognized database format"))
                {
                    throw new GeneralException(ex.Message);
                }
                else
                {
                    throw new System.ApplicationException(SharedStrings.ERROR_OPENING_CONNECTION);
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
            Query query = this.CreateQuery("ALTER TABLE " + tableName + " ALTER COLUMN " + columnName + " " + columnType);
            return (ExecuteNonQuery(query) > 0);
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
            sb.Append(" ADD COLUMN ");
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
                throw new System.ApplicationException(SharedStrings.ERROR_CLOSING_CONNECTION, ex);
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
            Query query = this.CreateQuery("select * from " + tableName );
            return this.ExecuteReader(query);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public override IDataReader GetTableDataReader(string tableName, string sortColumnName)
        {
            Query query = this.CreateQuery("select * from " + tableName + " ORDER BY " + sortColumnName);
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
        /// <param name="sortCriteria">sortCriteria">Comma delimited string of column names and ASC/DESC order</param>
        /// <returns></returns>
        public override DataTable GetTableData(string tableName, string columnNames, string sortCriteria)
        {
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

                if (tableName.ToLowerInvariant().EndsWith(".json") || GetConnection().ConnectionString.Contains("FMT=JSON"))
                {
                    string[] jsonsplit = connectionString.Split('=');
                    string jsonpath = "";
                    if (jsonsplit.Length > 2)
                    {
                        jsonpath = jsonsplit[2];
                        if (jsonpath.ToLowerInvariant().EndsWith(";extended properties"))
                        {
                            int extpropindex = jsonpath.ToLowerInvariant().IndexOf(";extended properties");
                            jsonpath = jsonpath.Substring(0, extpropindex);
                        }
                    }
                    else if (jsonsplit.Length > 1)
                        jsonpath = jsonsplit[1];
                    else
                        jsonpath = jsonsplit[0];
                    if (jsonpath.ToCharArray()[0] == '"')
                    {
                        int jlength = jsonpath.Length;
                        if (jsonpath.ToCharArray()[jlength - 1] == '"')
                        {
                            jsonpath = jsonpath.Substring(1, jlength - 2);
                        }
                    }
                    string[] separator = new string[] { "|json|" };
                    string[] tableNames = tableName.Split(separator, StringSplitOptions.None);
                    string jsonstring = File.ReadAllText(jsonpath + "\\" + tableNames[0]);
                    if (String.IsNullOrEmpty(jsonstring))
                    {
                        Epi.Windows.MsgBox.ShowInformation("File " + tableNames[0] + " is empty.");
                    }
                    else if (tableNames.Length > 1)
                    {
                        if (jsonstring.First<char>() == '[' && jsonstring.Last<char>() == ']')
                            jsonstring = jsonstring.Substring(1, jsonstring.Length - 2);
                    }
                    for (int jsoni = 1; jsoni < tableNames.Length; jsoni++)
                    {
                        string morejsonstring = File.ReadAllText(jsonpath + "\\" + tableNames[jsoni]);
                        if (String.IsNullOrEmpty(morejsonstring))
                        {
                            Epi.Windows.MsgBox.ShowInformation("File " + tableNames[jsoni] + " is empty.");
                            continue;
                        }
                        if (morejsonstring.First<char>() == '[' && morejsonstring.Last<char>() == ']')
                            morejsonstring = morejsonstring.Substring(1, morejsonstring.Length - 2);
                        if (!String.IsNullOrEmpty(jsonstring))
                            jsonstring += ",";
                        jsonstring = jsonstring + morejsonstring;
                    }
                    // DataTable dt = JSONtoDataTable(jsonstring);
                    if (String.IsNullOrEmpty(jsonstring))
                    {
                        jsonstring = "[]";
                    }
                    if (jsonstring.First<char>() != '[' && jsonstring.Last<char>() != ']')
                        jsonstring = "[" + jsonstring + "]";
                    DataTable dt = Newtonsoft.Json.JsonConvert.DeserializeObject<DataTable>(jsonstring);
                    return dt;
                }

                DataTable returnTable = Select(query);
                foreach (DataColumn column in returnTable.Columns)//EI-665
                {
                   column.ColumnName=column.ColumnName.Trim();
                }
                return returnTable;
            }
            finally
            {
            }
        }

        public DataTable JSONtoDataTable(string jsonstring)
        {
            DataTable dt = new DataTable();
            string smashedjson = Regex.Replace(jsonstring, "\\\\| |\n|\r|\t|\\[|\\]|\"", "");
            string[] dicts = Regex.Split(smashedjson, "},{");
            for (int i = 0; i < dicts.Length; i++)
                dicts[i] = dicts[i].Replace("{", "").Replace("}", "");
            var dtcolumns = Regex.Split(dicts[0], ",");
            foreach (string dtcol in dtcolumns)
            {
                var colparts = Regex.Split(dtcol, ":");
                dt.Columns.Add(colparts[0].Trim());
            }
            for (int i = 0; i < dicts.Length; i++)
            {
                DataRow row = dt.NewRow();
                var rowitems = Regex.Split(dicts[i], ",");
                for (int j = 0; j < rowitems.Length; j++)
                {
                    var itemparts = Regex.Split(rowitems[j], ":");
                    if (int.TryParse(itemparts[1], out int tmp))
                        row[j] = tmp;
                    else if (float.TryParse(itemparts[1], out float tmpf))
                    {
                        row[j] = tmpf;
                    }
                    else if (bool.TryParse(itemparts[1], out bool tmpb))
                    {
                        row[j] = tmpb;
                    }
                    else
                    {
                        dt.Columns[j].DataType = typeof(string);
                        row[j] = itemparts[1].Trim();
                    }
                }
                dt.Rows.Add(row);
            }
            return dt;
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
                if (GetConnection().ConnectionString.Contains("FMT=JSON"))
                {
                    return GetTableData(tableName);
                }
                string queryString = "select top 2 * from [" + tableName + "]";                
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

            try
            {
                string filestring = this.ConnectionString.Substring(this.ConnectionString.IndexOf("Source=") + 7);
                SQLiteConnection sqlite = new SQLiteConnection("Data Source=" + filestring);
                sqlite.Open();
                SQLiteCommand sqlite_command = sqlite.CreateCommand();
                sqlite_command.CommandText = selectQuery.SqlStatement;
                foreach (QueryParameter oparam in selectQuery.Parameters)
                {
                    sqlite_command.Parameters.Add(new SQLiteParameter(oparam.ParameterName, oparam.Value));
                }
                SQLiteDataReader reader = sqlite_command.ExecuteReader();
                IDataReader ireader = reader as IDataReader;
                //sqlite.Close();
                return ireader;
            }
            catch (SQLiteException sqlex)
            {
                throw sqlex;
            }
            #endregion

            IDbCommand command = null;
            IDbConnection connection = null;

            try
            {
                connection = GetConnection();
                OpenConnection(connection);
                command = GetCommand(selectQuery.SqlStatement, connection, selectQuery.Parameters);

                return command.ExecuteReader(commandBehavior);
            }
            catch (Exception ex)
            {
                throw new System.ApplicationException("Could not execute reader", ex);
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
            try
            {
                string filestring = this.ConnectionString.Substring(this.ConnectionString.IndexOf("Source=") + 7);
                using (SQLiteConnection sqlite = new SQLiteConnection("Data Source=" + filestring))
                {
                    sqlite.Open();
                    try
                    {
                        using (SQLiteCommand sqlite_command = sqlite.CreateCommand())
                        {
                            sqlite_command.CommandText = query.SqlStatement;
                            foreach (QueryParameter oparam in query.Parameters)
                            {
                                sqlite_command.Parameters.Add(new SQLiteParameter(oparam.ParameterName, oparam.Value));
                            }
                            result = sqlite_command.ExecuteScalar();
                            if (result.GetType() == typeof(Int64))
                                result = Convert.ToInt32((Int64)result);
                        }
                    }
                    catch (InvalidCastException ivex)
                    {
                        throw ivex;
                    }
                    catch (SQLiteException sqex)
                    {
                        throw sqex;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        sqlite.Close();
                    }
                }
            }
            catch (InvalidCastException)
            {
                return 0;
            }
            catch (SQLiteException sqlex)
            {
                throw sqlex;
            }
            return result;
        }

        /// <summary>
        /// Executes a SQL statement and returns total records affected. 
        /// </summary>
        /// <param name="query">query object</param>
        /// <returns>total records affected</returns>
        public override int ExecuteNonQuery(Query query)
        {
            #region Input Validation
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            #endregion

            try
            {
                int retint = 0;
                string filestring = this.ConnectionString.Substring(this.ConnectionString.IndexOf("Source=") + 7);
                using (SQLiteConnection sqlite = new SQLiteConnection("Data Source=" + filestring))
                {
                    IDbCommand sqlcommand = GetCommand(query.SqlStatement.Replace("COUNTER", "INTEGER").Replace("GUID", "TEXT").Replace(
                        "MEMO", "TEXT").Replace("DATETIME", "TEXT").Replace("datetime", "TEXT").Replace("int IDENTITY(1,1)", "INTEGER").Replace(
                        "nvarchar", "TEXT"), sqlite, new List<QueryParameter>());
                    foreach (QueryParameter oparam in query.Parameters)
                    {
                        sqlcommand.Parameters.Add(new SQLiteParameter(oparam.ParameterName, oparam.Value));
                    }
                    sqlite.Open();
                    try
                    {
                        retint = sqlcommand.ExecuteNonQuery();
                    }
                    catch (SQLiteException sqlex)
                    {
                        throw sqlex;
                    }
                    finally
                    {
                        sqlite.Close();
                    }
                }
                return retint;
            }
            finally
            {
                //CloseConnection(conn);
            }
        }

        /// <summary>
        /// Executes a transacted SQL statement and returns total records affected.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="transaction"></param>
        /// <returns>total records affected</returns>
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

        #endregion


        #region IDbDriver Members


        /// <summary>
        /// Encodes a value string to be OleDb ConnectionString compliant
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EncodeOleDbConnectionStringValue(string value)
        {
            // any preceding or trailing spaces in the string value must be 
            // enclosed in quotations marks. trailing or leading spaces
            // are ignored around integer, boolean, or enumerated values 
            // even if quoted 
            string result = value.Trim();

            // if the value contans a semicolon, single-quote character, 
            // or double-quote character, the value must be enclosed in 
            // quotation marks. The quoation-mark character used to enclose the
            // value must be doubled everytime it occures within the value

            if (value.IndexOfAny(new char[] { ';', '\'', '\"' }) > -1)
            {
                result.Replace("'", "''");
                result = "'" + result + "'";
            }

            // to include an equal sign (=) in a keyword or value, 
            // it must be preceded by another equal sign
            result = value.Replace("=", "==");

            return result;
        }

        //public abstract string ConnectionDescription
        //{
        //    get;
        //}

        //public abstract void CreateDatabase(string databaseName);

        // public abstract void CreateTable(string tableName, List<TableColumn> columns);

        // public abstract string FullName { get;}



        #endregion

        /// <summary>
        /// Gets the contents of a table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>Datatable containing the table data</returns>
        public override DataTable GetTableData(string tableName)
        {
            return GetTableData(tableName, string.Empty, string.Empty);
        }

        public override string SchemaPrefix
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns contents of a table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnNames">Comma delimited string of column names and ASC/DESC order</param>
        /// <returns></returns>
        public override DataTable GetTableData(string tableName, string columnNames)
        {
            return GetTableData(tableName, columnNames, string.Empty);
        }

        /// <summary>
        /// Returns contents of a table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnNames">List of column names to select. Column names should not be bracketed; this method will add brackets.</param>
        /// <returns>DataTable</returns>
        //public override DataTable GetTableData(string tableName, List<string> columnNames)
        //{
        //    WordBuilder wb = new WordBuilder(",");
        //    foreach (string s in columnNames)
        //    {                
        //        wb.Add(string.Format("[{0}]", s));
        //    }
        //    return GetTableData(tableName, wb.ToString(), string.Empty);
        //}

        /// <summary>
        /// Full name
        /// </summary>
        public override string FullName
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        /// <summary>
        /// Description of the connection
        /// </summary>
        public override string ConnectionDescription
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        /// <summary>
        /// Create Table
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columns">List of TableColumns</param>
        public override void CreateTable(string tableName, List<TableColumn> columns)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Get Code Table Names for project
        /// </summary>
        /// <param name="project">The project</param>
        /// <returns>DataTable</returns>
        public override DataTable GetCodeTableNamesForProject(Project project)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public override Epi.DataSets.TableSchema.TablesDataTable GetCodeTableList(IDbDriver db)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Identify Database
        /// </summary>
        /// <returns>string</returns>
        public override string IdentifyDatabase()
        {
            throw new Exception("The method or operation is not implemented.");
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


        public override string GetDbSpecificColumnType(GenericDbColumnType dataType)
        {
            throw new NotImplementedException();
        }

        public override string SyntaxTrue
        {
            get { return "TRUE"; }
        }

        public override string SyntaxFalse
        {
            get { return "FALSE"; }
        }

        public override bool InsertBulkRows(string pSelectSQL, System.Data.Common.DbDataReader pDataReader,  SetGadgetStatusHandler pStatusDelegate = null, CheckForCancellationHandler pCancellationDelegate = null)
        {
            bool result = false;

            System.Data.OleDb.OleDbConnection ConnOle = null;
            System.Data.OleDb.OleDbDataAdapter AdapterOle = null;
            System.Data.OleDb.OleDbCommandBuilder builderOLE = null;
            System.Data.Common.DbCommand cmdOle = null;

            DataSet dataSet = new DataSet();
            DataTable Temp = new DataTable();

            try
            {
                StringBuilder InsertSQL;
                StringBuilder ValueSQL;


                ConnOle = new System.Data.OleDb.OleDbConnection(ConnectionString.Replace(";IMEX=1", ""));
                AdapterOle = new System.Data.OleDb.OleDbDataAdapter(pSelectSQL, ConnOle);
                AdapterOle.FillSchema(dataSet, SchemaType.Source);
                AdapterOle.Fill(Temp);
                builderOLE = new System.Data.OleDb.OleDbCommandBuilder();
                builderOLE.DataAdapter = AdapterOle;

                ConnOle.Open();
                cmdOle = ConnOle.CreateCommand();
                
                cmdOle.CommandTimeout = 1500;

                int rowCount = 0;
                int skippedRows = 0;
                int totalRows = 0;
                int truncatedCellCount = 0;
                bool numericFieldOverflow = false;

                while (pDataReader.Read())
                {                   
                    cmdOle = builderOLE.GetInsertCommand();
                    InsertSQL = new StringBuilder();
                    ValueSQL = new StringBuilder();

                    InsertSQL.Append("Insert Into ");
                    InsertSQL.Append(pSelectSQL.Replace("Select * From ", ""));
                    InsertSQL.Append(" (");
                    ValueSQL.Append(" values (");
                    int CheckLength = 0;
                    List<OleDbParameter> ParameterList = new List<OleDbParameter>();
                    foreach (System.Data.OleDb.OleDbParameter param in cmdOle.Parameters)
                    {
                        string FieldName = param.SourceColumn;

                        InsertSQL.Append("[");
                        InsertSQL.Append(FieldName);
                        InsertSQL.Append("],");

                        ValueSQL.Append(param.ParameterName);
                        ValueSQL.Append(",");

                        try
                        {
                            param.Value = pDataReader[FieldName];
                        }
                        catch (Exception ex)
                        {
                            param.Value = DBNull.Value;
                        }
                        ParameterList.Add(param);

                    }
                    InsertSQL.Length = InsertSQL.Length - 1;
                    ValueSQL.Length = ValueSQL.Length - 1;
                    InsertSQL.Append(")");
                    ValueSQL.Append(")");
                    InsertSQL.Append(ValueSQL);
                    
                    cmdOle = null;
                    cmdOle = ConnOle.CreateCommand();
                    cmdOle.CommandText = InsertSQL.ToString();

                    foreach (OleDbParameter param in ParameterList)
                    {
                        DbParameter p2 = cmdOle.CreateParameter();
                        p2.DbType = param.DbType;
                        try
                        {
                            p2.Value = pDataReader[param.SourceColumn];
                            CheckLength = p2.Value.ToString().Length;
                            if (CheckLength > 255)
                            {
                                p2.Value = p2.Value.ToString().Substring(0, 255);
                                truncatedCellCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            p2.Value = DBNull.Value;
                        }
                        p2.ParameterName = param.ParameterName;

                        cmdOle.Parameters.Add(p2);
                    }

                    try
                    {
                        cmdOle.ExecuteNonQuery();
                        rowCount++;
                    }
                    catch (OleDbException ex)
                    {
                        skippedRows++;
                        if (ex.Message.ToLowerInvariant().Contains("numeric field overflow"))
                        {
                            numericFieldOverflow = true;
                        }
                        continue;
                    }

                   

                    if (pStatusDelegate != null)
                    {
                        totalRows = rowCount + skippedRows;
                        string messageString = String.Empty;

                        if (skippedRows == 0)
                        {                           
                            messageString = string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS, rowCount.ToString(), totalRows.ToString());
                        }
                        else
                        {                          
                            messageString = string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS_INCLUDE_SKIPPED, rowCount.ToString(), totalRows.ToString(), skippedRows.ToString());
                        }
                        pStatusDelegate.Invoke(messageString, (double)rowCount);
                    }

                    if (pCancellationDelegate != null && pCancellationDelegate.Invoke())
                    {
                        pStatusDelegate.Invoke(string.Format(SharedStrings.DASHBOARD_EXPORT_CANCELLED, rowCount.ToString()));
                        break;
                    }
                }

                if (pStatusDelegate != null)
                {
                    totalRows = rowCount + skippedRows;
                    string messageString = String.Empty;

                    if (skippedRows == 0)
                    {                      
                        messageString = string.Format(SharedStrings.DASHBOARD_EXPORT_SUCCESS, totalRows.ToString());
                    }
                    else if (skippedRows > 0 && !numericFieldOverflow)
                    {                                             
                        messageString = string.Format(SharedStrings.DASHBOARD_EXPORT_SUCCESS_SOME_SKIPPED, rowCount.ToString(), totalRows.ToString(), skippedRows.ToString());
                    }
                    else if (skippedRows > 0 && numericFieldOverflow)
                    {                        
                        messageString = string.Format(SharedStrings.DASHBOARD_EXPORT_SUCCESS_SOME_SKIPPED_NUMERIC_FIELD_OVERFLOW, rowCount.ToString(), totalRows.ToString(), skippedRows.ToString());
                    }
                    if (truncatedCellCount > 0)
                    {
                        messageString = messageString + string.Format("; {0} cells truncated to 255 maximum character limit.", truncatedCellCount);
                    }
                    pStatusDelegate.Invoke(messageString);
                }
            }
            //catch (System.Exception ex)
            //{
            //    Logger.Log(DateTime.Now + ":  " + ex.Message);
            //}
            finally
            {
                if (ConnOle != null)
                {
                    ConnOle.Close();
                    ConnOle.Dispose();
                }
                if (AdapterOle != null)
                {
                    AdapterOle.Dispose();
                }
                if (builderOLE != null)
                {
                    builderOLE.Dispose();
                }
                if (cmdOle != null)
                {
                    cmdOle.Cancel();
                    cmdOle.Dispose();
                }
            }

            GC.Collect();

            result = true;
            return result;
        }

        public override bool Insert_1_Row(string pSelectSQL, System.Data.Common.DbDataReader pDataReader)
        {
            bool result = false;

            System.Data.OleDb.OleDbConnection ConnOle = null;
            System.Data.OleDb.OleDbDataAdapter AdapterOle = null;
            System.Data.OleDb.OleDbCommandBuilder builderOLE = null;
            System.Data.Common.DbCommand cmdOle = null;

            DataSet dataSet = new DataSet();
            DataTable Temp = new DataTable();

            try
            {
                StringBuilder InsertSQL = new StringBuilder();
                StringBuilder ValueSQL = new StringBuilder();


                ConnOle = new System.Data.OleDb.OleDbConnection(ConnectionString.Replace(";IMEX=1",""));
                AdapterOle = new System.Data.OleDb.OleDbDataAdapter(pSelectSQL, ConnOle);
                //Adapter.FillSchema(dataSet, SchemaType.Source, pDestinationTableName);
                AdapterOle.FillSchema(dataSet, SchemaType.Source);
                AdapterOle.Fill(Temp);
                builderOLE = new System.Data.OleDb.OleDbCommandBuilder();
                builderOLE.DataAdapter = AdapterOle;

                ConnOle.Open();
                cmdOle = ConnOle.CreateCommand();
                cmdOle = builderOLE.GetInsertCommand();
                cmdOle.CommandTimeout = 1500;


                InsertSQL.Append("Insert Into ");
                InsertSQL.Append(pSelectSQL.Replace("Select * From ", ""));
                InsertSQL.Append(" (");
                ValueSQL.Append(" values (");

                List<OleDbParameter> ParameterList = new List<OleDbParameter>();
                foreach (System.Data.OleDb.OleDbParameter param in cmdOle.Parameters)
                {
                    //string FieldName = param.ParameterName.TrimStart(new char[] { '@' });
                    string FieldName = param.SourceColumn;

                    InsertSQL.Append("[");
                    InsertSQL.Append(FieldName);
                    InsertSQL.Append("],");

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

                            case "System.Int32":
                            case "System.Decimal":
                            case "System.Boolean":
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
                    }


                    ValueSQL.Append(",");*/

                }
                InsertSQL.Length = InsertSQL.Length - 1;
                ValueSQL.Length = ValueSQL.Length - 1;
                InsertSQL.Append(")");
                ValueSQL.Append(")");
                InsertSQL.Append(ValueSQL);
                builderOLE = null;
                cmdOle = null;
                cmdOle = ConnOle.CreateCommand();
                cmdOle.CommandText = InsertSQL.ToString();

                foreach (OleDbParameter param in ParameterList)
                {

                    DbParameter p2 = cmdOle.CreateParameter();
                    p2.DbType = param.DbType;
                    p2.Value = pDataReader[param.SourceColumn];
                    p2.ParameterName = param.ParameterName;

                    cmdOle.Parameters.Add(p2);
                }

                cmdOle.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                Logger.Log(DateTime.Now + ":  " + ex.Message);
            }
            finally
            {
                if (ConnOle != null)
                {
                    ConnOle.Close();
                }

            }

            result = true;
            return result;
        }

        public override bool Update_1_Row(string pSelectSQL, string pKeyString, DbDataReader pDataReader)
        {
            bool result = false;

            System.Data.OleDb.OleDbConnection ConnOle = null;
            System.Data.OleDb.OleDbDataAdapter AdapterOle = null;
            System.Data.OleDb.OleDbCommandBuilder builderOLE = null;
            System.Data.Common.DbCommand cmdOle = null;

            DataSet dataSet = new DataSet();
            DataTable Temp = new DataTable();

            try
            {
                StringBuilder UpdateSQL = new StringBuilder();

                ConnOle = new System.Data.OleDb.OleDbConnection(ConnectionString);
                AdapterOle = new System.Data.OleDb.OleDbDataAdapter(pSelectSQL, ConnOle);
                //Adapter.FillSchema(dataSet, SchemaType.Source, pDestinationTableName);
                AdapterOle.FillSchema(dataSet, SchemaType.Source);
                AdapterOle.Fill(Temp);
                builderOLE = new System.Data.OleDb.OleDbCommandBuilder();
                builderOLE.DataAdapter = AdapterOle;

                ConnOle.Open();
                cmdOle = ConnOle.CreateCommand();
                cmdOle = builderOLE.GetInsertCommand();
                cmdOle.CommandTimeout = 1500;



                UpdateSQL.Append("Update ");
                UpdateSQL.Append(pSelectSQL.Replace("Select * From ", ""));
                UpdateSQL.Append(" Set ");
                foreach (System.Data.OleDb.OleDbParameter param in cmdOle.Parameters)
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

                            case "System.Int32":
                            case "System.Decimal":
                            case "System.Boolean":
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
                builderOLE = null;
                cmdOle = null;
                cmdOle = ConnOle.CreateCommand();
                cmdOle.CommandText = UpdateSQL.ToString();

                //DBReadExecute.ExecuteSQL(pFileString, InsertSQL.ToString());

                cmdOle.ExecuteNonQuery();
            }
            catch (System.Exception ex)
            {
                Logger.Log(DateTime.Now + ":  " + ex.Message);
            }
            finally
            {
                if (ConnOle != null)
                {
                    ConnOle.Close();
                }
            }

            result = true;
            return result;
        }

        public override DbDataAdapter GetDbAdapter(string pSelectSQL)
        {
            IDbConnection connection = GetConnection();
            OleDbDataAdapter adapter = new OleDbDataAdapter();
            adapter.SelectCommand = (OleDbCommand)GetCommand(pSelectSQL, connection, new List<QueryParameter>());
            return adapter;
        }

        public override DbCommand GetCommand(string pKeyString, DataTable pDataTable)
        {
            System.Data.Common.DbCommand result = null;
            System.Data.Common.DbParameter P = null;

            string[] KeySet = null;
            StringBuilder KeyMatch = new StringBuilder();

            result = new System.Data.OleDb.OleDbCommand();
            
            foreach (DataColumn C in pDataTable.Columns)
            {
                P = new System.Data.OleDb.OleDbParameter();
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
                P = new System.Data.OleDb.OleDbParameter();
                P.ParameterName = temp[1].Trim();
                foreach (DataColumn C in pDataTable.Columns)
                {
                    if (P.ParameterName.ToLowerInvariant() == C.ColumnName.ToLowerInvariant())
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

        public override DbCommandBuilder GetDbCommandBuilder(DbDataAdapter adapter)
        {
            OleDbCommandBuilder builderOLE = new OleDbCommandBuilder();
            builderOLE.DataAdapter = (OleDbDataAdapter)adapter;
            return builderOLE;
        }
    }
}
