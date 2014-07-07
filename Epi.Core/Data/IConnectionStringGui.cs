
namespace Epi.Data
{

    /// <summary>
    /// Common interface for connection string building graphical user interfaces (in both Windows and Web)
    /// </summary>
    public interface IConnectionStringGui
    {
        bool ShouldIgnoreNonExistance
        {
            set;
        }

        /// <summary>
        /// Gets the connection string
        /// </summary>
        System.Data.Common.DbConnectionStringBuilder DbConnectionStringBuilder
        {
            get;
        }
        /// <summary>
        /// Gets the preferred database name
        /// </summary>
        string PreferredDatabaseName
        {
            get;
        }
        /// <summary>
        /// Gets the connection string description
        /// </summary>
        string ConnectionStringDescription
        {
            get;
        }

        /// <summary>
        /// Gets whether or not the user entered a password
        /// </summary>
        bool UsesPassword
        {
            get;
        }

        /// <summary>
        /// Sets the database name for the dialog to use
        /// </summary>
        /// <param name="databaseName">The name of the database</param>
        void SetDatabaseName(string databaseName);

        /// <summary>
        /// Sets the server name for the dialog to use
        /// </summary>
        /// <param name="serverName">The name of the server</param>
        void SetServerName(string serverName);

        /// <summary>
        /// Sets the user name for the dialog to use
        /// </summary>
        /// <param name="serverName">The username</param>
        void SetUserName(string userName);

        /// <summary>
        /// Sets the password for the dialog to use
        /// </summary>
        /// <param name="password">The password</param>
        void SetPassword(string password);
    }
}
