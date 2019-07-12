using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epi.Data.REDCap.Forms;

namespace Epi.Data.REDCap
{
    public class REDCapFactory : IDbDriverFactory
    {
        public string PrerequisiteMessage => throw new NotImplementedException();

        public bool ArePrerequisitesMet()
        {
            return true;
        }

        public bool CanClaimConnectionString(string connectionString)
        {
            string conn = connectionString.ToLowerInvariant();
            if (conn.Contains("epiweb"))
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

        public IDbDriver CreateDatabaseObject(System.Data.Common.DbConnectionStringBuilder connectionStringBuilder)
        {
			IDbDriver instance = new REDCapDataSource();
            instance.ConnectionString = connectionStringBuilder.ConnectionString;

            return instance;
        }

        public IDbDriver CreateDatabaseObjectByConfiguredName(string configDatabaseKey)
        {
            //may not use since PHIN is .MDB
            IDbDriver instance = null;
            Configuration config = Configuration.GetNewInstance();
            DataRow[] result = config.DatabaseConnections.Select("Name='" + configDatabaseKey + "'");
            if (result.Length == 1)
            {
                Epi.DataSets.Config.DatabaseRow dbConnection = (Epi.DataSets.Config.DatabaseRow)result[0];
                REDCapConnectionStringBuilder MongoDBConnectionBuilder = new REDCapConnectionStringBuilder(dbConnection.ConnectionString);
                instance = CreateDatabaseObject(MongoDBConnectionBuilder);
            }
            else
            {
                throw new GeneralException("Database name is not configured.");
            }

            return instance;
        }

        public void CreatePhysicalDatabase(DbDriverInfo dbInfo)
        {
            throw new NotImplementedException();
        }

        public IConnectionStringGui GetConnectionStringGuiForExistingDb()
        {
            if (Configuration.Environment == ExecutionEnvironment.WindowsApplication)
            {
                return new REDCapConnectionStringDialog();
            }
            else
            {
                throw new NotSupportedException("No GUI associated with current environment.");
            }
        }

        public IConnectionStringGui GetConnectionStringGuiForNewDb()
        {

            if (Configuration.Environment == ExecutionEnvironment.WindowsApplication)
            {
                return new REDCapConnectionStringDialog();

            }
            else
            {
                throw new NotSupportedException("No GUI associated with current environment.");
            }
        }

        public string GetCreateFromDataTableSQL(string tableName, DataTable table)
        {
            throw new NotImplementedException();
        }

        public DbConnectionStringBuilder RequestDefaultConnection(string databaseName, string projectName = "")
        {
            throw new NotImplementedException();
        }

        public DbConnectionStringBuilder RequestNewConnection(string fileName)
        {
            throw new NotImplementedException();
        }

        public string SQLGetType(object type, int columnSize, int numericPrecision, int numericScale)
        {
            throw new NotImplementedException();
        }
    }
}
