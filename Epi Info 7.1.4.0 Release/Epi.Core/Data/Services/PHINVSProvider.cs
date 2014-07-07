using System;
using System.Data;
using System.Data.Common;
using Epi.Data;

namespace Epi.Data.Services
{
    /// <summary>
    /// Providers an interface into PHIN vocabulary definitions database.
    /// </summary>
	public class PHINVSProvider
    {
        #region Fields
        private IDbDriver db;
        private static PHINVSProvider instance;
        private static Object classLock = typeof(PHINVSProvider);
        #endregion Fields

        #region Constructors
        //public PHINVSProvider()
        //    : this(DatabaseFactory.CreateDatabaseInstanceByConfiguredName(DatabaseFactory.KnownDatabaseNames.Phin))
        //{

        //}

        //private PHINVSProvider(IDbDriver db)
        //{
        //    this.db = db;
        //}
        private PHINVSProvider()
        {
            IDbDriverFactory dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.AccessDriver);
            //this.db = DatabaseFactoryCreator.CreateDatabaseInstanceByConfiguredName(DatabaseFactoryCreator.KnownDatabaseNames.Phin);
            this.db = dbFactory.CreateDatabaseObjectByConfiguredName(DbDriverFactoryCreator.KnownDatabaseNames.Phin);  
        }
        #endregion Constructors

        #region Public Properties
        /// <summary>
        /// Gets an instance of this singleton class
        /// </summary>
        public static PHINVSProvider Instance
        {
            get
            {
                lock (classLock)
                {
                    if (instance == null)
                    {
                        instance = new PHINVSProvider();
                    }
                    return instance;
                }
            }
        }
        #endregion Public Properties

        #region Protected Properties
        /// <summary>
        /// Gets/sets the datatabase.
        /// </summary>
        protected virtual IDbDriver Db
        {
            get
            {
                return db;
            }
            set
            {
                db = value;
            }
        }
        #endregion Protected Properties

        #region Public Methods
        /// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        //[Obsolete("Use of DataTable in this context is no different than the use of a multidimensional System.Object array (not recommended).", false)]
		public DataTable GetDomains()
		{
			string queryString = " select [Code], [Name] from DOMAINS order by [Name]";
			Query query = Db.CreateQuery(queryString);
			return Db.Select(query);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="domainCode"></param>
		/// <returns></returns>
        //[Obsolete("Use of DataTable in this context is no different than the use of a multidimensional System.Object array (not recommended).", false)]
		public DataTable GetValueSets(string domainCode)
		{
			string queryString = " select [Code], [Name] from ValueSets V inner join ValueSets_Domains VD on V.Code = VD.ValueSetCode where [VD.DomainCode] = @DomainCode order by [Name]";
			Query query = Db.CreateQuery(queryString);
			query.Parameters.Add(new QueryParameter("@DomainCode", DbType.String, domainCode));
			return Db.Select(query);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="valueSetCode"></param>
		/// <returns></returns>
        //[Obsolete("Use of DataTable in this context is no different than the use of a multidimensional System.Object array (not recommended).", false)]
		public DataTable GetConcepts(string valueSetCode, string fieldName)
		{
            try
            {
                string queryString = " select [Code], [Name] from [" + valueSetCode + "] order by [Name] ";
                Query query = Db.CreateQuery(queryString);
                DataTable table = Db.Select(query);
                table.Columns[0].ColumnName = fieldName;
                return table;
            }
			finally
			{
			}
		}
		#endregion Public Methods
	}
}