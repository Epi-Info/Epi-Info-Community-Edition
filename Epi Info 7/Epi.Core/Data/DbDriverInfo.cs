using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Text;

namespace Epi.Data
{
    /// <summary>
    /// Class for information on DbDriver
    /// </summary>
    public class DbDriverInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DbDriverInfo()
        {
            dbCnnStrBuilder = new DbConnectionStringBuilder();
        }

        #region private member
        private DbConnectionStringBuilder dbCnnStrBuilder;
        private string dbName;
        #endregion

        #region public property

        /// <summary>
        /// DBCnnStringBuilder
        /// </summary>
        public DbConnectionStringBuilder DBCnnStringBuilder
        {
            get { return dbCnnStrBuilder; }
            set { dbCnnStrBuilder = value; }
        }

        /// <summary>
        /// Database name
        /// </summary>
        public string DBName
        {
            get { return dbName; }
            set { dbName = value; }
        }
        #endregion

    }
}
