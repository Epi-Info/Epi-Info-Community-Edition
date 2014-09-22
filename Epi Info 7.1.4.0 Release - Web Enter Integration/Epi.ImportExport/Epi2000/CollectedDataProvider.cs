#region Namespaces

using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using Epi.Data;

using Epi.Data.Services;

#endregion //Namespaces

namespace Epi.Epi2000
{

    /// <summary>
    /// Class CollectedDataProvider
    /// </summary>
    public class CollectedDataProvider //: Epi.Data.Services.CollectedDataProvider
    {
        #region Fields
        private Epi.Data.Services.CollectedDataProvider collectedData = null;
        
        /// <summary>
        /// The underlying physical database
        /// </summary>
        protected IDbDriver db;
        #endregion Fields

        #region Constructors

        /// <summary>
        /// Constructor for CollectedDataProvider
        /// </summary>
        /// <param name="proj"></param>	
        public CollectedDataProvider(Project proj)
        {
            IDbDriverFactory dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.AccessDriver);
            OleDbConnectionStringBuilder cnnStrBuilder = new OleDbConnectionStringBuilder();
            cnnStrBuilder.DataSource = proj.FilePath;
            this.db = dbFactory.CreateDatabaseObject(cnnStrBuilder); 
            
            //this.db = dbFactory.CreateDatabaseObjectByConfiguredName(proj.FilePath);  
            //db = DatabaseFactoryCreator.CreateDatabaseInstanceByFileExtension(proj.FilePath);
        }

        #endregion Constructors

        /// <summary>
        /// Gets list of data tables in the EI 3.x metadata database
        /// </summary>
        /// <returns>DataRow of table names</returns>
        public DataRow[] GetDataTableList()
        {
            try
            {
                //DataTable tables = db.GetTableSchema();
                DataTable tables = collectedData.GetTableSchema();
                return tables.Select("TABLE_NAME not like 'code%' and TABLE_NAME not like 'view%'");
            }
            catch (Exception ex)
            {
                throw new System.ApplicationException("Could not retrieve data tables from legacy database", ex);
            }
        }

        /// <summary>
        /// Returns the count of tables
        /// </summary>
        /// <returns></returns>
        public int GetTableCount()
        {
            return db.GetTableCount();
        }


        /// <summary>
        /// Returns the database instance.
        /// </summary>
        /// <returns>Abstract data type for databases.</returns>
        public IDbDriver GetDatabase()
        {
            return this.db;
        }
        /// <summary>
        /// Returns the count of tables
        /// </summary>
        /// <returns></returns>
        public int GetRecordCount(Epi2000.View view)
        {
            try
            {
                #region Input Validation
                if (view == null)
                {
                    throw new System.ArgumentNullException("view");
                }
                #endregion Input Validation                
                
                string qryString = " select count(*) from " + db.InsertInEscape(view.TableNames[0]);
                Query query = db.CreateQuery(qryString);                
                return Int32.Parse((db.ExecuteScalar(query)).ToString());
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not retrieve record count.", ex); //TODO: move to shared strings
            }
            finally
            {
            }
        }

        /// <summary>
        /// returns true if table exists
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>bool</returns>
        public bool TableExists(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("tableName");
            }

            return db.TableExists(tableName);
        }

        /// <summary>
        /// returns an IDataReader on the named table
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>IDataReader</returns>
        public IDataReader GetTableDataReader(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("tableName");
            }

            return db.GetTableDataReader(tableName);
        }
    }
}
