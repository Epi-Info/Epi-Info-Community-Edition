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
    /// Concret DBFactory for MS SharePoint
    /// </summary>
    public class SharePointListFactory : IDbDriverFactory
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public SharePointListFactory()
        {
        }

        /// <summary>
        /// Create Physical Database
        /// </summary>
        /// <param name="dbInfo">Database driver information.</param>
        public void CreatePhysicalDatabase(DbDriverInfo dbInfo)
        {
            CreateDatabaseObject(dbInfo.DBCnnStringBuilder);
        }


        /// <summary>
        /// Create a database object.
        /// </summary>
        /// <param name="connectionStringBuilder">Database-specific connection string</param>
        /// <returns>Database driver.</returns>
        public IDbDriver CreateDatabaseObject(DbConnectionStringBuilder connectionStringBuilder)
        {
            IDbDriver instance = new SharePointList();
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
                OleDbConnectionStringBuilder cnnBuilder = new OleDbConnectionStringBuilder("Provider=Microsoft.ACE.OLEDB.12.0;WSS;IMEX=1;RetrieveIds=Yes;");
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
                return new Forms.SharePointExistingListDialog();
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
                return new Forms.SharePointExistingListDialog();
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
            OleDbConnectionStringBuilder cnnStringBuilder = new OleDbConnectionStringBuilder("Provider=Microsoft.ACE.OLEDB.12.0;WSS;IMEX=1;RetrieveIds=Yes;");
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
                OleDbConnectionStringBuilder oleDbCnnStringBuilder = new OleDbConnectionStringBuilder("Provider=Microsoft.ACE.OLEDB.12.0;WSS;IMEX=1;RetrieveIds=Yes;");
                return oleDbCnnStringBuilder;
            }
            catch
            {
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
            return false;
        }

        public string ConvertFileStringToConnectionString(string fileString)
        {
            return fileString;
        }

        public string GetCreateFromDataTableSQL(string tableName, DataTable table)
        {
            throw new NotImplementedException();
        }

        public string SQLGetType(object type, int columnSize, int numericPrecision, int numericScale)
        {
            throw new NotImplementedException();
        }
    }
}