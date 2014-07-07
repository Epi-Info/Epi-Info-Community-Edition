using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;

namespace Epi.Data
{
    /// <summary>
    /// Interface class for the Database Driver Factory.
    /// </summary>
    public interface IDbDriverFactory
    {
        /// <summary>
        /// Creates a physical database.
        /// </summary>
        /// <param name="dbInfo">Database information.</param>
        void CreatePhysicalDatabase(DbDriverInfo dbInfo);
        /// <summary>
        /// Creates a database object.
        /// </summary>
        /// <param name="connectionStringBuilder">Connection string builder object.</param>
        /// <param name="isWebMode"></param>
        /// <returns>Instance of class derived from IDbDriver.</returns>
        IDbDriver CreateDatabaseObject(DbConnectionStringBuilder connectionStringBuilder);
        /// <summary>
        /// Creates a database object by configured name.
        /// </summary>
        /// <param name="configDatabaseKey">Key of configured database.</param>
        /// <returns>Instance of class derived from IDbDriver.</returns>
        IDbDriver CreateDatabaseObjectByConfiguredName(string configDatabaseKey);
        /// <summary>
        /// Gets a database engine specific connection string builder dialog for a database that already exists.
        /// </summary>
        /// <returns>Instance of class derived from IConnectionStringBuilder.</returns>
        IConnectionStringGui GetConnectionStringGuiForExistingDb();

        /// <summary>
        /// Gets a database engine specific connection string builder dialog for a database that does not exist yet.
        /// </summary>
        /// <returns>Instance of class derived from IConnectionStringBuilder.</returns>
        IConnectionStringGui GetConnectionStringGuiForNewDb();

        /// <summary>
        /// Gets a database engine specific connection string
        /// </summary>
        /// <param name="fileName">The project name and path</param>
        /// <returns>ConnectionStringInfo</returns>
        DbConnectionStringBuilder RequestNewConnection(string fileName);

        /// <summary>
        /// Gets a connection string (builder) using default parameters given a database name
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        /// <param name="projectName">Name of the project</param>
        /// <returns>A connection string</returns>
        DbConnectionStringBuilder RequestDefaultConnection(string databaseName, string projectName = "");

        bool ArePrerequisitesMet();
        string PrerequisiteMessage { get; }

        bool CanClaimConnectionString(string connectionString);

        string ConvertFileStringToConnectionString(string fileString);

        string GetCreateFromDataTableSQL(string tableName, System.Data.DataTable table);

        string SQLGetType(object type, int columnSize, int numericPrecision, int numericScale);
    }
}
