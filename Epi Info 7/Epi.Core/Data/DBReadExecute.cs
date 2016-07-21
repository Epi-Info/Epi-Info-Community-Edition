using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
//using ADOX;

namespace Epi.Data
{
    /// <summary>
    /// DBReadExecute
    /// </summary>
    public class DBReadExecute
    {
        /// <summary>
        /// Database driver
        /// </summary>
        public IDbDriver db;


        /// <summary>
        /// Existance Status
        /// </summary>
        public enum enumExistanceStatus
        {
            /// <summary>
            /// File Does Not Exist
            /// </summary>
            FileDoesNotExist,
            /// <summary>
            /// Table Does Not Exist
            /// </summary>
            TableDoesNotExist
        }

        /// <summary>
        /// Project fild name
        /// </summary>
        static public string ProjectFileName;

        /// <summary>
        /// DataSource
        /// </summary>
        static public IDbDriverFactory DataSource;

        /// <summary>
        /// GetDataTable
        /// </summary>
        /// <param name="pFileString">File String</param>
        /// <param name="pSQL">SQL Statement</param>
        /// <returns>DataTable of results</returns>
        public static DataTable GetDataTable(string pFileString, string pSQL)
        {
            System.Data.DataTable result = new System.Data.DataTable();

            string connString = ParseConnectionString(pFileString);
            if (DataSource != null)
            {
                IDbDriver driver = DataSource.CreateDatabaseObject(new System.Data.Common.DbConnectionStringBuilder());
                driver.ConnectionString = connString;
                Query query = driver.CreateQuery(pSQL);
                driver.Select(query, result);
            }

            return result;
        }

        /// <summary>
        /// GetDataTable
        /// </summary>
        /// <param name="IDbDriver">Database object</param>
        /// <param name="pSQL">SQL Statement</param>
        /// <returns>DataTable of results</returns>
        public static DataTable GetDataTable(IDbDriver dbDriver, string pSQL)
        {
            System.Data.DataTable result = new System.Data.DataTable();

            dbDriver.Select(dbDriver.CreateQuery(pSQL), result);

            return result;
        }

        /// <summary>
        /// GetSchema
        /// </summary>
        /// <param name="pFileString">File String</param>
        /// <param name="pTableName">Table Name</param>
        /// <param name="pDataSet">DataSet</param>
        public static void GetSchema(string pFileString, string pTableName, ref DataSet pDataSet)
        {
            System.Data.Common.DbDataAdapter Adapter = null;

            string connString = ParseConnectionString(pFileString);

            if (DataSource != null)
            {
                IDbDriver driver = DataSource.CreateDatabaseObject(new System.Data.Common.DbConnectionStringBuilder());
                driver.ConnectionString = connString;
                Adapter = driver.GetDbAdapter("Select * From [" + pTableName + "]");
            }
            if (Adapter != null)
            {
                Adapter.FillSchema(pDataSet, SchemaType.Source, pTableName);
            }

        }

        /// <summary>
        /// Execute SQL
        /// </summary>
        /// <param name="pFileString">File String</param>
        /// <param name="pSQL">SQL Statement</param>
        /// <returns>bool</returns>
        public static bool ExecuteSQL(string pFileString, string pSQL)
        {
            bool result = false;

            string connString = ParseConnectionString(pFileString);

            if (DataSource != null)
            {
                IDbDriver driver = DataSource.CreateDatabaseObject(new System.Data.Common.DbConnectionStringBuilder());
                driver.ConnectionString = connString;
                driver.ExecuteNonQuery(driver.CreateQuery(pSQL));
                result = true;
            }
            return result;
        }

        public static object GetScalar(string pFileString, string pSQL)
        {
            object retval = null;

            string connString = ParseConnectionString(pFileString);

            if (DataSource != null)
            {
                IDbDriver driver = DataSource.CreateDatabaseObject(new System.Data.Common.DbConnectionStringBuilder());
                driver.ConnectionString = connString;
                retval = driver.ExecuteScalar(driver.CreateQuery(pSQL));
            }
            return retval;
        }

        /// <summary>
        /// Execute SQL
        /// </summary>
        /// <param name="pConnectionString">Connection string</param>
        /// <param name="pSQL">SQL statement</param>
        /// <param name="pTimeOut">Time out integer</param>
        /// <returns>bool</returns>
        public static bool ExecuteSQL(string pConnectionString, string pSQL, int pTimeOut)
        {
            bool result = false;

            string connString = ParseConnectionString(pConnectionString);

            if (DataSource != null)
            {
                IDbDriver driver = DataSource.CreateDatabaseObject(new System.Data.Common.DbConnectionStringBuilder());
                driver.ConnectionString = connString;
                driver.ExecuteNonQuery(driver.CreateQuery(pSQL));
                result = true;
            }
            return result;
        }

        ///// <summary>
        ///// Execute Async SQL
        ///// </summary>
        ///// <param name="pConnectionString">Connection string</param>
        ///// <param name="pSQL">SQL statement</param>
        ///// <param name="pTimeOut">Time out integer</param>
        ///// <returns></returns>
        //public static bool ExecuteAsyncSQL(string pConnectionString, string pSQL, int pTimeOut)
        //{
        //    bool result = false;



        //    string ConnectionString = ParseConnectionString(pConnectionString);

        //    switch (DBReadExecute.DataSource)
        //    {
        //        case DBReadExecute.enumDataSouce.SQLServer:
        //            System.Data.SqlClient.SqlConnection ConnSS = new System.Data.SqlClient.SqlConnection(ConnectionString + ";async=true");
        //            try
        //            {

        //                ConnSS.Open();
        //                System.Data.SqlClient.SqlCommand CmdSS = ConnSS.CreateCommand();
        //                CmdSS.CommandTimeout = pTimeOut;
        //                if (CmdSS != null)
        //                {
        //                    CmdSS.CommandText = pSQL;
        //                    CmdSS.CommandTimeout = 1500;
        //                    CmdSS.BeginExecuteNonQuery(HandleCallback, CmdSS);
        //                    result = true;
        //                }
        //            }
        //            finally
        //            {
        //                /* ConnSS.Close();*/
        //            }
        //            break;
        //        case DBReadExecute.enumDataSouce.MSAccess:
        //        case DBReadExecute.enumDataSouce.MSAccess2007:
        //        case DBReadExecute.enumDataSouce.MSExcel:
        //        case DBReadExecute.enumDataSouce.MSExcel2007:
        //            System.Data.Common.DbConnection Conn = null;
        //            System.Data.Common.DbCommand Cmd = null;
        //            Conn = new System.Data.OleDb.OleDbConnection(ConnectionString);
        //            try
        //            {
        //                Conn.Open();
        //                Cmd = Conn.CreateCommand();
        //                Cmd.CommandTimeout = pTimeOut;
        //                if (Cmd != null)
        //                {
        //                    Cmd.CommandText = pSQL;
        //                    Cmd.CommandTimeout = 1500;
        //                    Cmd.ExecuteNonQuery();
        //                    result = true;
        //                }
        //            }
        //            finally
        //            {
        //                Conn.Close();
        //            }
        //            break;
        //    }

        //    return result;
        //}

        private static void HandleCallback(IAsyncResult result)
        {

            System.Data.SqlClient.SqlCommand command = (System.Data.SqlClient.SqlCommand)result.AsyncState;

            int rowCount = command.EndExecuteNonQuery(result);

            System.Console.WriteLine(rowCount);

            if (command.Connection.State == ConnectionState.Open)
            {
                command.Connection.Close();
            }
            command.Connection = null;
        }

        ///// <summary>
        ///// Import Queue
        ///// </summary>
        //public static Queue<KeyValuePair<string, string>> ImportQueue = new Queue<KeyValuePair<string, string>>();

        ///// <summary>
        ///// Process Import Queue
        ///// </summary>
        //public static void ProcessImportQueue()
        //{
        //    System.Console.WriteLine("Started ProcessImport {0}", System.DateTime.Now);

        //    System.Data.Common.DbConnection Conn = null;
        //    System.Data.Common.DbCommand Cmd = null;
        //    string pConnectionString = "";

        //    if (ImportQueue.Count > 0)
        //    {
        //        pConnectionString = ImportQueue.Peek().Key;
        //    }

        //    string ConnectionString = ParseConnectionString(pConnectionString);

        //    switch (DBReadExecute.DataSource)
        //    {
        //        case DBReadExecute.enumDataSouce.SQLServer:
        //            Conn = new System.Data.SqlClient.SqlConnection(ConnectionString);
        //            break;
        //        case DBReadExecute.enumDataSouce.MSAccess:
        //        case DBReadExecute.enumDataSouce.MSAccess2007:
        //        case DBReadExecute.enumDataSouce.MSExcel:
        //        case DBReadExecute.enumDataSouce.MSExcel2007:
        //            Conn = new System.Data.OleDb.OleDbConnection(ConnectionString);
        //            break;
        //    }

        //    Conn.Open();
        //    Cmd = Conn.CreateCommand();
        //    Cmd.CommandTimeout = 0;


        //    try
        //    {
        //        while (ImportQueue.Count > 0)
        //        {
        //            KeyValuePair<string, string> S_C = ImportQueue.Dequeue();



        //            if (Cmd != null)
        //            {
        //                Cmd.CommandText = S_C.Value;
        //                Cmd.CommandTimeout = 1500;
        //                Cmd.ExecuteNonQuery();
        //            }



        //        }

        //    }
        //    finally
        //    {
        //        Conn.Close();
        //    }



        //    System.Console.WriteLine("Finished ProcessImport {0}", System.DateTime.Now);
        //}

        /// <summary>
        /// GetDataDriver
        /// </summary>
        /// <param name="fileString">File string</param>
        /// <returns>IDbDriver</returns>
        public static IDbDriver GetDataDriver(string fileString, bool pIsConnectionString = false)
        {
            string connString = null;
            connString = ParseConnectionString(fileString);

            if (DataSource != null)
            {
                IDbDriver driver = DataSource.CreateDatabaseObject(new System.Data.Common.DbConnectionStringBuilder());
                driver.ConnectionString = connString;
                return driver;
            }

            return null;
        }

        /// <summary>
        /// Parse Connection String
        /// </summary>
        /// <param name="pConnectionString">Connection String</param>
        /// <returns>string</returns>
        public static string ParseConnectionString(string pConnectionString)
        {
            string result = null;
            string Test = pConnectionString.Trim(new char[] { '\'' });

            DBReadExecute.ProjectFileName = "";

            if (Test.ToLowerInvariant().EndsWith(".prj"))
            {
                Project P = new Project(Test);

                Test = P.CollectedDataConnectionString;
                result = Test;

                DBReadExecute.DataSource = DbDriverFactoryCreator.GetDbDriverFactory(P.CollectedDataDriver);
                DBReadExecute.ProjectFileName = pConnectionString.Trim(new char[] { '\'' });
            }
            else
            {
                DataSets.Config.DataDriverDataTable dataDrivers = Configuration.GetNewInstance().DataDrivers;
                foreach (DataSets.Config.DataDriverRow dataDriver in dataDrivers)
                {
                    IDbDriverFactory dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(dataDriver.Type);
                    if (dbFactory.CanClaimConnectionString(Test))
                    {
                        DBReadExecute.DataSource = dbFactory;
                        result = dbFactory.ConvertFileStringToConnectionString(Test);
                        break;
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Check Database Table Existance
        /// </summary>
        /// <param name="pFileString">File String</param>
        /// <param name="pTableName">Table name</param>
        /// <returns>bool</returns>
        public static bool CheckDatabaseExistance(string pFileString, string pTableName, bool pIsConnectionString = false)
        {
            bool result = false;
            string connString = ParseConnectionString(pFileString);
            try
            {
                IDbDriver driver = DataSource.CreateDatabaseObject(new System.Data.Common.DbConnectionStringBuilder());
                if (driver != null)
                {
                    driver.ConnectionString = connString;

                }
            }
            catch (Exception ex)
            {
                // do nothing for now
            }
            
            return result;
        }

        /// <summary>
        /// Check Database Table Existance
        /// </summary>
        /// <param name="pFileString">File String</param>
        /// <param name="pTableName">Table name</param>
        /// <returns>bool</returns>
        public static bool CheckDatabaseTableExistance(string pFileString, string pTableName, bool pIsConnectionString = false)
        {

            
                DataSets.Config.DataDriverDataTable dataDrivers = Configuration.GetNewInstance().DataDrivers;
                IDbDriverFactory dbFactory = null;
                foreach (DataSets.Config.DataDriverRow dataDriver in dataDrivers)
                {
                    dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(dataDriver.Type);

                    if (dbFactory.CanClaimConnectionString(pFileString))
                    {
                        break;
                    }
                }

                IDbDriver dbDriver = DBReadExecute.GetDataDriver(pFileString, pIsConnectionString);

                return dbDriver.CheckDatabaseTableExistance(pFileString, pTableName, pIsConnectionString);

        }



        /// <summary>
        /// Create Insert Query
        /// </summary>
        /// <param name="pFileString">File String</param>
        /// <param name="pDataTable">Data Table</param>
        /// <param name="pRow">Row</param>
        /// <param name="pTableName">Table Name</param>
        /// <returns>Query</returns>
        public static Query CreateInsertQuery(string pFileString, DataTable pDataTable, DataRow pRow, string pTableName)
        {
            const string cs = "";
            IDbDriver I = null;
            using (System.Data.Common.DbConnection Conn = new System.Data.SqlClient.SqlConnection(cs))
            {
                Conn.Open();
                //System.Data.Common.DbCommand cmd = new 

            }

            return I.CreateQuery("");
        }

        /// <summary>
        /// Write Data Table
        /// </summary>
        /// <param name="pFileString">File String</param>
        /// <param name="pSelectSQL">Select statement</param>
        /// <param name="pDestinationTableName">Destination Table name</param>
        /// <param name="pDatatable">Data Table</param>
        /// <returns></returns>
        public static bool WriteDataTable(string pFileString, string pSelectSQL, string pDestinationTableName, DataTable pDatatable)
        {
            bool result = false;
            System.Data.Common.DbDataAdapter Adapter = null;
            System.Data.Common.DbCommandBuilder builder = null;
            DataSet dataSet = new DataSet();

            string connString = ParseConnectionString(pFileString);

            if (DataSource != null)
            {
                IDbDriver driver = DataSource.CreateDatabaseObject(new System.Data.Common.DbConnectionStringBuilder());
                driver.ConnectionString = connString;
                Adapter = driver.GetDbAdapter(pSelectSQL);
                builder = driver.GetDbCommandBuilder(Adapter);
            }

            //code to modify data in DataSet here
            builder.GetInsertCommand();

            //Without the SqlCommandBuilder this line would fail
            Adapter.Update(dataSet, pDestinationTableName);

            /*
                OleDbCommand insertCommand = dataAdapter.InsertCommand;
                foreach (DataRow row in origTable.Rows)
                {
                    foreach (OleDbParameter param in insertCommand.Parameters)
                    {
                        param.Value = row[param.ParameterName];
                    }
                    insertCommand->ExecuteNonQuery();
                } */

            result = true;
            return result;
        }

        /// <summary>
        /// Insert Data
        /// </summary>
        /// <param name="pFileString">File String</param>
        /// <param name="pSelectSQL">Select SQL</param>
        /// <param name="pDataReader">Data Reader</param>
        /// <returns>bool</returns>
        public static bool InsertData(string pFileString, string pSelectSQL, System.Data.Common.DbDataReader pDataReader)
        {
            bool result = false;

            System.Data.Common.DbConnection Conn = null;
            System.Data.Common.DbDataAdapter Adapter = null;
            System.Data.Common.DbCommandBuilder builder = null;
            System.Data.Common.DbCommand cmd = null;
            DataSet dataSet = new DataSet();
            DataTable Temp = new DataTable();

            string connString = ParseConnectionString(pFileString);

            if (DataSource != null)
            {
                IDbDriver driver = DataSource.CreateDatabaseObject(new System.Data.Common.DbConnectionStringBuilder());
                driver.ConnectionString = connString;
                Conn = (System.Data.Common.DbConnection)driver.GetConnection();
                Adapter = driver.GetDbAdapter(pSelectSQL);
                Adapter.FillSchema(dataSet, SchemaType.Source);
                Adapter.Fill(Temp);
                builder = driver.GetDbCommandBuilder(Adapter);
                builder.QuotePrefix = "[";
                builder.QuoteSuffix = "]";
                try
                {
                    Conn.Open();
                    
                    cmd = builder.GetInsertCommand(true);
                    cmd.CommandTimeout = 1500;
                    cmd.Connection = Conn;
                    //builder.GetInsertCommand((true);

                    while (pDataReader.Read())
                    {
                        foreach (System.Data.Common.DbParameter param in cmd.Parameters)
                        {
                            //string FieldName = param.ParameterName.TrimStart(new char[] { '@' });
                            string FieldName = param.SourceColumn;
                            if (FieldName.ToUpperInvariant() == "UNIQUEKEY")
                            {
                                param.Value = null;
                            }
                            else
                            {
                                param.Value = pDataReader[FieldName];
                            }
                        }
                        cmd.ExecuteNonQuery();
                    }
                }
                finally
                {
                    Conn.Close();
                }
            }

            result = true;
            return result;
        }

        /// <summary>
        /// Insert Data
        /// </summary>
        /// <param name="pFileString">File String</param>
        /// <param name="pSelectSQL">Select Statement</param>
        /// <param name="pDataRows">DataRows</param>
        /// <returns>bool</returns>
        public static bool InsertData(string pFileString, string pSelectSQL, System.Data.DataRow[] pDataRows)
        {
            bool result = false;

            System.Data.Common.DbConnection Conn = null;
            System.Data.Common.DbDataAdapter Adapter = null;
            System.Data.Common.DbCommandBuilder builder = null;
            System.Data.Common.DbCommand cmd = null;
            DataSet dataSet = new DataSet();
            DataTable Temp = new DataTable();

            string connString = ParseConnectionString(pFileString);

            if (DataSource != null)
            {
                IDbDriver driver = DataSource.CreateDatabaseObject(new System.Data.Common.DbConnectionStringBuilder());
                driver.ConnectionString = connString;
                Conn = (System.Data.Common.DbConnection)driver.GetConnection();
                Adapter = driver.GetDbAdapter(pSelectSQL);
                Adapter.FillSchema(dataSet, SchemaType.Source);
                Adapter.Fill(Temp);
                builder = driver.GetDbCommandBuilder(Adapter);
                builder.QuotePrefix = "[";
                builder.QuoteSuffix = "]";

                try
                {
                    Conn.Open();

                    //cmd = builder.GetInsertCommand(true);
                    cmd = Conn.CreateCommand();
                    cmd.CommandTimeout = 1500;

                    //builder.GetInsertCommand((true);

                    foreach (DataRow R in pDataRows)
                    {
                        //string SQL = GetInsertSQL(pSelectSQL.Substring(pSelectSQL.LastIndexOf (" From ") + 6, pSelectSQL.Length - pSelectSQL.LastIndexOf(" From ") - 6),R);
                        string SQL = GetInsertSQL(R.Table.TableName, R);
                        cmd.CommandText = SQL;
                        cmd.ExecuteNonQuery();
                    }
                }
                finally
                {
                    Conn.Close();
                }
            }

            result = true;
            return result;
        }

        /// <summary>
        /// Insert one row
        /// </summary>
        /// <param name="pFileString">File String</param>
        /// <param name="pSelectSQL">Select Statement</param>
        /// <param name="pDataReader">DataReader</param>
        /// <returns>bool</returns>
        public static bool Insert_1_Row(string pFileString, string pSelectSQL, System.Data.Common.DbDataReader pDataReader)
        {
            /*
             *  NOTE FROM EPI1: The original contents of this method have been moved to SqlDatabase.Insert_1_Row 
             *  and OleDbDatabase.Insert_1_Row.
             * 
             */ 

            bool result = false;

            string connString = ParseConnectionString(pFileString);

            if (DataSource != null)
            {
                IDbDriver driver = DataSource.CreateDatabaseObject(new System.Data.Common.DbConnectionStringBuilder());
                driver.ConnectionString = connString;
                result = driver.Insert_1_Row(pSelectSQL, pDataReader);
            }
            return result;
        }

        public static bool InsertBulkRows(string pFileString, string pSelectSQL, System.Data.Common.DbDataReader pDataReader, SetGadgetStatusHandler pStatusDelegate = null)
        {
            bool result = false;

            string connString = ParseConnectionString(pFileString);

            if (DataSource != null)
            {
                IDbDriver driver = DataSource.CreateDatabaseObject(new System.Data.Common.DbConnectionStringBuilder());
                driver.ConnectionString = connString;
                result = driver.InsertBulkRows(pSelectSQL, pDataReader, pStatusDelegate);
            }
            return result;
        }

        /// <summary>
        /// Update Data
        /// </summary>
        /// <param name="pFileString">File String</param>
        /// <param name="pSelectSQL">Select Statement</param>
        /// <param name="pDataReader">DataReader</param>
        /// <returns>bool</returns>
        public static bool UpdateData(string pFileString, string pSelectSQL, System.Data.Common.DbDataReader pDataReader)
        {
            bool result = false;

            System.Data.Common.DbConnection Conn = null;
            System.Data.Common.DbDataAdapter Adapter = null;
            System.Data.Common.DbCommandBuilder builder = null;
            System.Data.Common.DbCommand cmd = null;
            DataSet dataSet = new DataSet();
            DataTable Temp = new DataTable();

            string connString = ParseConnectionString(pFileString);

            if (DataSource != null)
            {
                IDbDriver driver = DataSource.CreateDatabaseObject(new System.Data.Common.DbConnectionStringBuilder());
                driver.ConnectionString = connString;
                Conn = (System.Data.Common.DbConnection)driver.GetConnection();
                Adapter = driver.GetDbAdapter(pSelectSQL);
                Adapter.FillSchema(dataSet, SchemaType.Source);
                Adapter.Fill(Temp);
                builder = driver.GetDbCommandBuilder(Adapter);
            }
            try
            {

                Conn.Open();
                cmd = builder.GetInsertCommand(true);
                cmd.CommandTimeout = 1500;

                while (pDataReader.Read())
                {
                    foreach (System.Data.Common.DbParameter param in cmd.Parameters)
                    {
                        param.Value = pDataReader[param.SourceColumn];
                    }
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                Conn.Close();
            }

            result = true;
            return result;
        }

        /// <summary>
        /// Update one row
        /// </summary>
        /// <param name="pFileString">File String</param>
        /// <param name="pUpdateSQL">Update SQL Statement</param>
        /// <param name="pCommand">Command</param>
        /// <returns>bool</returns>
        public static bool Update_1_Row(string pFileString, string pSelectSQL, string pKeyString, System.Data.Common.DbDataReader pDataReader)
        {

            /*
             *  NOTE FROM EPI1: The original contents of this method have been moved to SqlDatabase.Update_1_Row 
             *  and OleDbDatabase.Update_1_Row.
             * 
             */ 

            bool result = false;

            DataSet dataSet = new DataSet();
            DataTable Temp = new DataTable();

            string connString = ParseConnectionString(pFileString);

            if (DataSource != null)
            {
                IDbDriver driver = DataSource.CreateDatabaseObject(new System.Data.Common.DbConnectionStringBuilder());
                driver.ConnectionString = connString;
                result = driver.Update_1_Row(pSelectSQL, pKeyString, pDataReader);
            }

            return result;
        }

        /// <summary>
        /// GetCreateFromDataTableSQL
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="table">DataTable</param>
        /// <returns>string</returns>
        public static string GetCreateFromDataTableSQL(string tableName, DataTable table)
        {
            if (DataSource != null)
            {
                return DataSource.GetCreateFromDataTableSQL(tableName, table);
            }

            return null;
        }

        /// <summary>
        /// Return T-SQL data type definition, based on schema definition for a column 
        /// </summary>
        /// <param name="type">type</param>
        /// <param name="columnSize">Column size</param>
        /// <param name="numericPrecision">Precision</param>
        /// <param name="numericScale">Scale</param>
        /// <returns>string</returns>
        public static string SQLGetType(object type, int columnSize, int numericPrecision, int numericScale)
        {
            if (DataSource != null)
            {
                return DataSource.SQLGetType(type, columnSize, numericPrecision, numericScale);
            }

            return null;
        }

        /// <summary>
        /// Overload based on row from schema table
        /// </summary>
        /// <param name="schemaRow">schema row</param>
        /// <returns>string</returns>
        public static string SQLGetType(DataRow schemaRow)
        {
            return SQLGetType(schemaRow["DataType"],
                                int.Parse(schemaRow["ColumnSize"].ToString()),
                                int.Parse(schemaRow["NumericPrecision"].ToString()),
                                int.Parse(schemaRow["NumericScale"].ToString()));
        }

        /// <summary>
        /// Overload based on DataColumn from DataTable type
        /// </summary>
        /// <param name="column">column</param>
        /// <returns>string</returns>
        public static string SQLGetType(DataColumn column)
        {
            return SQLGetType(column.DataType, column.MaxLength, 10, 2);
        }

        /// <summary>
        /// Get Command
        /// </summary>
        /// <param name="pFileName">File name</param>
        /// <param name="pKeyString">Key String</param>
        /// <param name="pDataTable">Data Table</param>
        /// <returns>DbCommand</returns>
        public static System.Data.Common.DbCommand GetCommand(string pFileName, string pKeyString, DataTable pDataTable)
        {
            System.Data.Common.DbCommand result = null;

            string connString = DBReadExecute.ParseConnectionString(pFileName);

            if (DataSource != null)
            {
                IDbDriver driver = DataSource.CreateDatabaseObject(new System.Data.Common.DbConnectionStringBuilder());
                driver.ConnectionString = connString;
                result = driver.GetCommand(pKeyString, pDataTable);
            }
            
            return result;
        }

        private static string GetInsertSQL(string pTable, System.Data.Common.DbDataReader pDataReader)
        {
            StringBuilder InsertSQL = new StringBuilder();
            StringBuilder ValueSQL = new StringBuilder();
            InsertSQL.Append("Insert Into ");
            InsertSQL.Append(pTable);
            InsertSQL.Append(" (");
            ValueSQL.Append(" values (");
            foreach (System.Data.OleDb.OleDbParameter param in pDataReader)
            {
                //string FieldName = param.ParameterName.TrimStart(new char[] { '@' });
                string FieldName = param.SourceColumn;


                if (FieldName.ToUpperInvariant() == "UNIQUEKEY")
                {
                    continue;
                    //ValueSQL.Append("null");
                    //param.Value = DBNull.Value;
                }
                else
                {
                    InsertSQL.Append("[");
                    InsertSQL.Append(FieldName);
                    InsertSQL.Append("],");

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
                }

                ValueSQL.Append(",");

            }
            InsertSQL.Length = InsertSQL.Length - 1;
            ValueSQL.Length = ValueSQL.Length - 1;
            InsertSQL.Append(")");
            ValueSQL.Append(")");
            InsertSQL.Append(ValueSQL);

            return InsertSQL.ToString();
        }

        private static string GetInsertSQL(string pTable, DataRow pDataRow)
        {
            StringBuilder InsertSQL = new StringBuilder();
            StringBuilder ValueSQL = new StringBuilder();
            InsertSQL.Append("Insert Into [");
            InsertSQL.Append(pTable);
            InsertSQL.Append("] (");


            ValueSQL.Append(" values (");
            foreach (DataColumn column in pDataRow.Table.Columns)
            {
                //string FieldName = param.ParameterName.TrimStart(new char[] { '@' });
                string FieldName = column.ColumnName;


                if (FieldName.ToUpperInvariant() == "UNIQUEKEY")
                {
                    continue;
                    //ValueSQL.Append("null");
                    //param.Value = DBNull.Value;
                }
                else
                {
                    InsertSQL.Append("[");
                    InsertSQL.Append(FieldName);
                    InsertSQL.Append("],");

                    if (pDataRow[FieldName] == DBNull.Value)
                    {
                        ValueSQL.Append("null");
                    }
                    else
                    {
                        switch (pDataRow[FieldName].GetType().ToString())
                        {

                            case "System.Boolean":
                                if(Convert.ToBoolean(pDataRow[FieldName]) == true)
                                {
                                    ValueSQL.Append("1");
                                }
                                else
                                {
                                    ValueSQL.Append("0");
                                }
                                break;
                            case "System.Int32":
                            case "System.Decimal":
                            case "System.Double":
                            case "System.Single":
                            case "System.Byte":
                                ValueSQL.Append(pDataRow[FieldName].ToString().Replace("'", "''"));
                                break;
                            default:
                                ValueSQL.Append("'");
                                ValueSQL.Append(pDataRow[FieldName].ToString().Replace("'", "''"));
                                ValueSQL.Append("'");
                                break;
                        }
                    }
                }

                ValueSQL.Append(",");

            }
            InsertSQL.Length = InsertSQL.Length - 1;
            ValueSQL.Length = ValueSQL.Length - 1;
            InsertSQL.Append(")");
            ValueSQL.Append(")");
            InsertSQL.Append(ValueSQL);

            return InsertSQL.ToString();
        }
    }
}
