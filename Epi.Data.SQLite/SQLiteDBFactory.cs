using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data.OleDb;
using Epi.Data;
using Epi.Windows;
using System.Globalization;
using System.Data.SQLite;

namespace Epi.Data.SQLite
{   
    /// <summary>
    /// Concrete DBFactory for SQLite 
    /// </summary>
    public class SQLiteDBFactory : IDbDriverFactory 
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public SQLiteDBFactory()
        {
        }

        /// <summary>
        /// Create Physical Database
        /// </summary>
        /// <param name="dbInfo">Database driver information.</param>
        public void CreatePhysicalDatabase(DbDriverInfo dbInfo)
        {
            string filepath = ((OleDbConnectionStringBuilder)dbInfo.DBCnnStringBuilder).DataSource;

            //string filepath = dbInfo.DBName;
            if (!string.IsNullOrEmpty(filepath))
            {
                //TODO: refactor this, make one function for all: ExtractTemplate(FileType, FilePath) 
                if (filepath.EndsWith(".db", true, CultureInfo.InvariantCulture))
                {
                    using (SQLiteConnection sqlite = new SQLiteConnection("Data Source=" + filepath))
                    {
                        sqlite.Open();
                        SQLiteCommand cmd = sqlite.CreateCommand();
                        cmd.CommandText = "DROP TABLE IF EXISTS makeDBTable;";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "CREATE TABLE makeDBTable (GlobalRecordId TEXT);";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "insert into makeDBTable " +
                            "SELECT substr(u,1,8)||'-'||substr(u,9,4)||'-4'||substr(u,13,3)||'-'||v||substr(u,17,3)||'-'||substr(u,21,12) " +
                            "from (SELECT upper(hex(randomblob(16))) as u, substr('89AB',abs(random()) % 4 + 1, 1) as v);";
                        cmd.ExecuteNonQuery();
                        sqlite.Close();
                    }
                    return;
                }
            }
        }

        /// <summary>
        /// Create a database object.
        /// </summary>
        /// <param name="connectionStringBuilder">Database-specific connection string</param>
        /// <returns>Database driver.</returns>
        public IDbDriver CreateDatabaseObject(DbConnectionStringBuilder connectionStringBuilder)
        {
            IDbDriver instance = new SQLiteDatabase();
            instance.ConnectionString = connectionStringBuilder.ToString();
            return instance;
        }

        /// <summary>
        /// Create Database ObjectBy Configured Name
        /// </summary>
        /// <param name="configDatabaseKey">Configuration Database Key.</param>
        /// <returns>Database driver.</returns>
        public IDbDriver CreateDatabaseObjectByConfiguredName(string configDatabaseKey)
        {
            IDbDriver instance = null;
            Configuration config = Configuration.GetNewInstance();
            DataRow[] result = config.DatabaseConnections.Select("Name='" + configDatabaseKey + "'");
            if (result.Length == 1)
            {
                Epi.DataSets.Config.DatabaseRow dbConnection = (Epi.DataSets.Config.DatabaseRow)result[0];
                OleDbConnectionStringBuilder cnnBuilder = new OleDbConnectionStringBuilder();
                cnnBuilder.DataSource = dbConnection.ConnectionString;
                //ToDo: Change code to read the configuration file, need to add provider information into the configuration file. 
                cnnBuilder.Provider = "Epi.Data.SQLite.1.0.0.0";   //Zack: here just hard coded for debug  
                instance = CreateDatabaseObject(cnnBuilder);
            }
            else
            {
                throw new GeneralException("Database name is not configured.");
            }
            
            return instance;
        }

        /// <summary>
        /// Gets a database engine specific connection string builder dialog for a database that already exists.
        /// </summary>
        /// <returns>IConnectionStringBuilder</returns>
        public IConnectionStringGui GetConnectionStringGuiForExistingDb()
        {
            if (Configuration.Environment == ExecutionEnvironment.WindowsApplication)
            {
                return new Forms.SQLiteExistingFileDialog();
            }
            else
            {
                throw new NotSupportedException("No GUI associated with current environment.");
            }
        }

        /// <summary>
        /// Gets a database engine specific connection string builder dialog for a database that does not exist yet.
        /// </summary>
        /// <returns>IConnectionStringBuilder</returns>
        public IConnectionStringGui GetConnectionStringGuiForNewDb()
        {
            if (Configuration.Environment == ExecutionEnvironment.WindowsApplication)
            {
                return new Forms.SQLiteNewFileDialog();
            }
            else
            {
                throw new NotSupportedException("No GUI associated with current environment.");
            }
        }

        /// <summary>
        /// Gets a database engine specific connection for a database.
        /// </summary>
        /// <param name="fileName">The file name and path of the project</param>
        /// <returns>IConnectionString Builder</returns>
        public DbConnectionStringBuilder RequestNewConnection(string fileName)
        {
            OleDbConnectionStringBuilder cnnStringBuilder = new OleDbConnectionStringBuilder();
            cnnStringBuilder.DataSource = fileName;
            cnnStringBuilder.Provider = "Epi.Data.SQLite.1.0.0.0";
            return cnnStringBuilder;
        }

        /// <summary>
        /// Gets a connection string (builder) using default parameters given a database name
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        /// <returns>A connection string</returns>
        public DbConnectionStringBuilder RequestDefaultConnection(string databaseName, string projectName = "")
        {
            try
            {
                OleDbConnectionStringBuilder oleDbCnnStringBuilder = new OleDbConnectionStringBuilder(SQLiteDatabase.BuildDefaultConnectionString(databaseName, projectName));
                if (String.IsNullOrEmpty(projectName))
                    oleDbCnnStringBuilder = new OleDbConnectionStringBuilder(SQLiteDatabase.BuildDefaultConnectionString(databaseName, databaseName));
                oleDbCnnStringBuilder.Provider = "Epi.Data.SQLite.1.0.0.0";
                return oleDbCnnStringBuilder;
            }
            catch
            {
                //TEMP 
                //Linux can not use SQLite - null will be caught in another exception
                return null;
            }
        }

        public bool ArePrerequisitesMet()
        {
            return true;
        }

        public string PrerequisiteMessage
        {
            get { return string.Empty; }
        }

        public bool CanClaimConnectionString(string connectionString)
        {
            if (connectionString.ToLowerInvariant().Contains(".db") || (connectionString.ToLowerInvariant().Contains("provider=epi.data.sqlite") && !connectionString.ToLowerInvariant().Contains("excel") && !connectionString.ToLowerInvariant().Contains("fmt=delimited") && !connectionString.ToLowerInvariant().Contains("fmt=json")))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string ConvertFileStringToConnectionString(string fileString)
        {
            string SQLite = "Provider=Epi.Data.SQLite.1.0.0.0;Data Source={0}";
            string result = string.Empty;
            string Test = fileString.Trim(new char[] { '\'' });


            if (Test.ToLowerInvariant().EndsWith(".db") && !Test.ToLowerInvariant().Contains("provider=epi.data.sqlite"))
            {
                result = string.Format(SQLite, Test);
            }
            else if (Test.ToLowerInvariant().IndexOf("provider=epi.data.sqlite") > -1)
            {
                result = Test;
            }

            return result;
        }

        public string GetCreateFromDataTableSQL(string tableName, DataTable table)
        {
            string sql = "CREATE TABLE [" + tableName + "] (\n";
            bool HasUniqueKey = false;
            // columns
            foreach (DataColumn column in table.Columns)
            {
                if (column.ColumnName.ToLowerInvariant() == "uniquekey")
                {
                    HasUniqueKey = true;
                    sql += "[" + column.ColumnName + "] AutoIncrement,\n";
                }
                else
                {
                    if (!column.ColumnName.Contains("."))
                    {
                        sql += "[" + column.ColumnName + "] " + SQLGetType(column) + ",\n";
                    }
                }
            }
            sql = sql.TrimEnd(new char[] { ',', '\n' });
            // primary keys
            if (HasUniqueKey)
            {
                sql += ", CONSTRAINT [PK_" + tableName + "] PRIMARY KEY (UniqueKey))";
            }
            else
                if (table.PrimaryKey.Length > 0)
                {
                    sql += ", CONSTRAINT [PK_" + tableName + "] PRIMARY KEY (";
                    foreach (DataColumn column in table.PrimaryKey)
                    {
                        sql += "[" + column.ColumnName + "],";
                    }
                    sql = sql.TrimEnd(new char[] { ',' }) + "))\n";
                }
                else
                {
                    sql += "\n)";
                }


            return sql;
        }

        private string SQLGetType(DataColumn column)
        {
            return SQLGetType(column.DataType, column.MaxLength, 10, 2);
        }

        public string SQLGetType(object type, int columnSize, int numericPrecision, int numericScale)
        {
            switch (type.ToString())
            {
                case "System.String":
                    return "Text(" + ((columnSize == -1) ? 255 : columnSize) + ")";

                case "System.Decimal":
                    if (numericScale > 0)
                        return "REAL";
                    else if (numericPrecision > 10)
                        return "BIGINT";
                    else
                        return "INT";

                case "System.Double":
                case "System.Single":
                    return "FLOAT";

                case "System.Int64":
                    return "BIGINT";

                case "System.Boolean":
                    return "BIT";

                case "System.Int16":
                case "System.Int32":
                case "System.Byte":
                    return "INT";

                case "System.DateTime":
                    return "DATETIME";
                case "System.Guid":
                    return "Text(" + ((columnSize == -1) ? 255 : columnSize) + ")";
                default:
                    return "Text(" + ((columnSize == -1) ? 255 : columnSize) + ")";
                    //throw new Exception(type.ToString() + " not implemented.");
            }
        }
    }
 }
