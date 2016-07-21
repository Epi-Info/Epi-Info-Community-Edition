using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using Epi.Data.SqlServer.Forms;
using System.IO;



namespace Epi.Data.SqlServer
{
    
    /// <summary>
    /// 
    /// </summary>
    public class SqlDBFactory : IDbDriverFactory
    {
        private SqlConnectionStringBuilder slqConnBuild = new SqlConnectionStringBuilder();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SqlDBFactory()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        public SqlDBFactory(string connectionString)
        {
            slqConnBuild.ConnectionString = connectionString;
        }
        /// <summary>
        /// Creates a physical database.
        /// </summary>
        /// <param name="dbInfo">Database driver information.</param>
        public void CreatePhysicalDatabase(DbDriverInfo dbInfo)
        {
            try
            {
                SqlConnectionStringBuilder masterBuilder = new SqlConnectionStringBuilder(dbInfo.DBCnnStringBuilder.ToString());
                masterBuilder.InitialCatalog = "Master";
                //masterBuilder.IntegratedSecurity = true;
                SqlConnection masterConnection = new SqlConnection(masterBuilder.ToString());
                IDbCommand command = masterConnection.CreateCommand();
                command.CommandText = string.Format("SELECT Count(*) FROM sysdatabases WHERE name='{0}'", dbInfo.DBName);
                masterConnection.Open();
                object result = command.ExecuteScalar();
                if ((int)result == 0)
                {
                    command.CommandText = "create database [" + dbInfo.DBName + "]";
                    //Logger.Log(command.CommandText);
                    command.ExecuteNonQuery();
                }
                masterConnection.Close();
                
            }
            catch (ApplicationException)
            {
                    throw new System.ApplicationException("Could not create new SQL database. Please contact your SQL server administrator.");
            }
            finally
            {
            }
        }
        /*
        /// <summary>
        /// Creates a database object.
        /// </summary>
        /// <param name="connectionStringBuilder">Database connection string builder object.</param>
        /// <returns>New SqlDatabase() instance.</returns>
        public IDbDriver CreateDatabaseObject(DbConnectionStringBuilder connectionStringBuilder)
        {
            IDbDriver instance = new SqlDatabase(true);
            instance.ConnectionString = connectionStringBuilder.ConnectionString;
            return instance;
        }*/

        /// <summary>
        /// Creates a database object.
        /// </summary>
        /// <param name="connectionStringBuilder">Database connection string builder object.</param>
        /// <returns>New SqlDatabase() instance.</returns>
        public IDbDriver CreateDatabaseObject(DbConnectionStringBuilder connectionStringBuilder)
        {
            IDbDriver instance = new SqlDatabase();
            instance.ConnectionString = connectionStringBuilder.ConnectionString;
            return instance;
        }

        /// <summary>
        /// Creates a database object by using a configured name.
        /// </summary>
        /// <param name="configDatabaseKey">Key for configured database.</param>
        /// <returns>New SqlDatabase() instance.</returns>
        public IDbDriver CreateDatabaseObjectByConfiguredName(string configDatabaseKey)
        {
            //may not use since AppDatab, PHIN are MDB
            IDbDriver instance = null;
            Configuration config = Configuration.GetNewInstance();
            DataRow[] result = config.DatabaseConnections.Select("Name='" + configDatabaseKey + "'");
            if (result.Length == 1)
            {
                Epi.DataSets.Config.DatabaseRow dbConnection = (Epi.DataSets.Config.DatabaseRow)result[0];
                SqlConnectionStringBuilder cnnBuilder = new SqlConnectionStringBuilder(dbConnection.ConnectionString);
                instance = CreateDatabaseObject(cnnBuilder);
            }
            else
            {
                throw new GeneralException("Database name is not configured.");
            }

            return instance;
        }

        /// <summary>
        /// Gets a database engine specific connection string builder dialog.
        /// </summary>
        /// <returns>IConnectionStringBuilder</returns>
        public IConnectionStringGui GetConnectionStringGuiForExistingDb()
        {
            if (Configuration.Environment == ExecutionEnvironment.WindowsApplication)
            {
                return new ExistingConnectionStringDialog();
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
                return (IConnectionStringGui) new NonExistingConnectionStringDialog();
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
            SqlConnectionStringBuilder cnnStringBuilder = new SqlConnectionStringBuilder();
            cnnStringBuilder.InitialCatalog = fileName;

            return cnnStringBuilder;
        }

        /// <summary>
        /// Gets a connection string (builder) using default parameters given a database name
        /// </summary>
        /// <remarks>In the case of SQL Server, it is not possible to build a default connection string using just a database name. You also need to provide a default server.</remarks>
        /// <param name="databaseName">Name of the database</param>
        /// <returns>A connection string</returns>
        public DbConnectionStringBuilder RequestDefaultConnection(string databaseName, string projectName = "")
        {
            return new SqlConnectionStringBuilder(SqlDatabase.BuildDefaultConnectionString(databaseName));
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
            if (connectionString.ToLowerInvariant().Contains("initial catalog"))
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
            return fileString;
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
                    sql += "[" + column.ColumnName + "] int Identity,\n";
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
                    return "VARCHAR(" + ((columnSize == -1) ? 255 : columnSize) + ")";

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
                    return "INT";

                case "System.DateTime":
                    return "DATETIME";
                case "System.Byte":
                    return "BIT";
                case "System.Guid":
                    return "VARCHAR(" + ((columnSize == -1) ? 255 : columnSize) + ")";
                default:
                    throw new Exception(type.ToString() + " not implemented.");
            }
        }

    }
}
