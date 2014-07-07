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

namespace Epi.Data.Office
{   
    /// <summary>
    /// Concrete DBFactory for MS Access 
    /// </summary>
    public class Access2007DBFactory : IDbDriverFactory 
    {
        
        /// <summary>
        /// Default Constructor
        /// </summary>
        public Access2007DBFactory()
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
                if (filepath.EndsWith(".accdb", true, CultureInfo.InvariantCulture))
                {
                    ResourceLoader.ExtractAccess2007Template(filepath);
                    File.SetAttributes(filepath, FileAttributes.Normal);
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
            IDbDriver instance = new Access2007Database();
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
                cnnBuilder.Provider = "Microsoft.ACE.OLEDB.12.0";   //Zack: here just hard coded for debug  
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
                return new Forms.Access2007ExistingFileDialog();
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
                return null;
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
            cnnStringBuilder.Provider = "Microsoft.ACE.OLEDB.12.0";
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
                OleDbConnectionStringBuilder oleDbCnnStringBuilder = new OleDbConnectionStringBuilder(Access2007Database.BuildDefaultConnectionString(databaseName));
                oleDbCnnStringBuilder.Provider = "Microsoft.ACE.OLEDB.12.0";
                return oleDbCnnStringBuilder;
            }
            catch
            {
                //TEMP 
                //Linux can not use Access - null will be caught in another exception
                return null;
            }
        }

        public bool ArePrerequisitesMet()
        {
            return (Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("Microsoft.ACE.OLEDB.12.0") != null);
        }

        public string PrerequisiteMessage
        {
            get 
            {
                return "One or more components needed to open this file were not found on this computer. Please install the free \"2007 Office System Driver Data Connectivity Components\" found here: http://www.microsoft.com/download/en/details.aspx?id=23734"; 
            }
        }

        public bool CanClaimConnectionString(string connectionString)
        {
            if (connectionString.ToLower().Contains(".accdb") || (connectionString.ToLower().Contains("provider=microsoft.ace.oledb.12.0") && !connectionString.ToLower().Contains("excel")))
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
            string MSAccess2007 = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0}";
            string result = string.Empty;
            string Test = fileString.Trim(new char[] { '\'' });


            if (Test.ToLower().EndsWith(".accdb") && !Test.ToLower().Contains("provider=microsoft.ace.oledb.12.0"))
            {
                result = string.Format(MSAccess2007, Test);
            }
            else if (Test.ToLower().IndexOf("provider=microsoft.ace.oledb.12.0") > -1)
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
                if (column.ColumnName.ToLower() == "uniquekey")
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
                    return "REAL";

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
                    throw new Exception(type.ToString() + " not implemented.");
            }
        }
    }
 }
