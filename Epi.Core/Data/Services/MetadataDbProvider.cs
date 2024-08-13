using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Epi.Collections;
using Epi;
using Epi.Data;
using Epi.DataSets;
using Epi.Fields;
using Epi.Resources;

namespace Epi.Data.Services
{

    /// <summary>
    /// Database implementation of Metadata provider
    /// </summary>
    public class MetadataDbProvider : IMetadataProvider
    {
        private bool isWebMode;

        #region MetadataProvider Database Members

        /// <summary>
        /// Creates a database.
        /// </summary>
        /// <param name="dbInfo">Database driver information.</param>
        protected void CreateDatabase(DbDriverInfo dbInfo)
        {
            dbFactory.CreatePhysicalDatabase(dbInfo);
            //db.CreateDatabase(databaseName);
        }

        protected void RemoveDatabase()
        {
            if (db.TableExists("metaBackgrounds"))
            {                 
                db.ExecuteNonQuery(db.CreateQuery("Delete From metaBackgrounds"));
                db.DeleteTable("metaBackgrounds");
            }

            if (db.TableExists("metaMapPoints"))
            {
                db.ExecuteNonQuery(db.CreateQuery("Delete From metaMapPoints"));
                db.DeleteTable("metaMapPoints");
            }

            if (db.TableExists("metaMapLayers"))
            {
                db.ExecuteNonQuery(db.CreateQuery("Delete From metaMapLayers"));
                db.DeleteTable("metaMapLayers");
            }

            if (db.TableExists("metaMaps"))
            {
                db.ExecuteNonQuery(db.CreateQuery("Delete From metaMaps"));
                db.DeleteTable("metaMaps");
            }

            if (db.TableExists("metaLayers"))
            {
                db.ExecuteNonQuery(db.CreateQuery("Delete From metaLayers"));
                db.DeleteTable("metaLayers");
            }

            if (db.TableExists("metaLayerRenderTypes"))
            {
                db.ExecuteNonQuery(db.CreateQuery("Delete from metaLayerRenderTypes"));
                db.DeleteTable("metaLayerRenderTypes");
            }

            if (db.TableExists("metaFieldGroups"))
            {
                db.ExecuteNonQuery(db.CreateQuery("Delete From metaFieldGroups"));
                db.DeleteTable("metaFieldGroups");
            }

            if (db.TableExists("metaGridColumns"))
            {
                db.ExecuteNonQuery(db.CreateQuery("Delete From metaGridColumns"));
                db.DeleteTable("metaGridColumns");
            }            

            if (db.TableExists("metaImages"))
            {
                db.ExecuteNonQuery(db.CreateQuery("Delete From metaImages"));
                db.DeleteTable("metaImages");
            }

            if (db.TableExists("metaFields"))
            {
                db.ExecuteNonQuery(db.CreateQuery("Delete From metaFields"));
                db.DeleteTable("metaFields");
            }

            if (db.TableExists("metaFieldTypes"))
            {
                db.ExecuteNonQuery(db.CreateQuery("Delete From metaFieldTypes"));
                db.DeleteTable("metaFieldTypes");
            }

            if (db.TableExists("metaPages"))
            {
                db.ExecuteNonQuery(db.CreateQuery("Delete From metaPages"));
                db.DeleteTable("metaPages");
            }

            if (db.TableExists("metaViews"))
            {
                db.ExecuteNonQuery(db.CreateQuery("Delete From metaViews"));
                db.DeleteTable("metaViews");
            }            
            
            if (db.TableExists("metaPatterns"))
            {
                db.ExecuteNonQuery(db.CreateQuery("Delete From metaPatterns"));
                db.DeleteTable("metaPatterns");
            }

            if (db.TableExists("metaDataTypes"))
            {
                db.ExecuteNonQuery(db.CreateQuery("Delete From metaDataTypes"));
                db.DeleteTable("metaDataTypes");
            }

            if (db.TableExists("metaPrograms"))
            {
                db.ExecuteNonQuery(db.CreateQuery("Delete From metaPrograms"));
                db.DeleteTable("metaPrograms");
            }

            if (db.TableExists("metaDbInfo"))
            {
                db.ExecuteNonQuery(db.CreateQuery("Delete From metaDbInfo"));
                db.DeleteTable("metaDbInfo");
            }

            if (db.TableExists("metaLinks"))
            {
                db.ExecuteNonQuery(db.CreateQuery("Delete From metaLinks"));
                db.DeleteTable("metaLinks");
            }
        }

        public void RemovePageOutlierFields()
        {
            try
            {
                Query query = db.CreateQuery("delete from metaFields where [ControlTopPositionPercentage] > 1 or [ControlLeftPositionPercentage] > 1 or [PromptTopPositionPercentage] > 1 or [PromptLeftPositionPercentage] > 1");

                db.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not delete fields in the database", ex);
            }
        }

        /// <summary>
        /// Gets data from all fields of code table.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <returns><see cref="System.Data.DataTable"/></returns>
        public DataTable GetCodeTableData(string tableName)
        {
            Util.Assert(tableName.StartsWith("meta") == false);
            return db.GetTableData(tableName);
        }

        /// <summary>
        /// Gets data from specified fields of code table.
        /// </summary>
        /// <param name="tableName">Name of code table.</param>
        /// <param name="columnNames">Names of columns of code table.</param>
        /// <returns><see cref="System.Data.DataTable"/></returns>
        public DataTable GetCodeTableData(string tableName, string columnNames)
        {
            Util.Assert(tableName.StartsWith("meta") == false);
            List<string> columnList = new List<string>(columnNames.Split(','));
            return this.db.GetTableData(tableName, columnList);
        }

        /// <summary>
        /// Gets data from specified fields of code table.
        /// </summary>
        /// <param name="tableName">Name of code table.</param>
        /// <param name="columnNames">Names of columns of code table.</param>
        /// <returns><see cref="System.Data.DataTable"/></returns>
        public DataTable GetCodeTableData(string tableName, List<string> columnNames)
        {
            Util.Assert(tableName.StartsWith("meta") == false);
            return this.db.GetTableData(tableName, columnNames);
        }

        /// <summary>
        /// Gets data from specified fields of code table and sorts the results.
        /// </summary>
        /// <param name="tableName">Name of code table.</param>
        /// <param name="columnNames">Names of columns of code table.</param>
        /// <param name="sortCriteria">Sorting criteria</param>
        /// <returns><see cref="System.Data.DataTable"/></returns>
        public DataTable GetCodeTableData(string tableName, string columnNames, string sortCriteria)
        {
            return this.db.GetTableData(tableName, columnNames, sortCriteria);
        }

        #endregion

        #region Fields

        /// <summary>
        /// The underlying physical databsae
        /// </summary>
        //protected IDbDriver db;
        public IDbDriver db;
        private IDbDriverFactory dbFactory;
        private bool populatedTablesVerified = false;
        private Project project;

        #endregion Fields

        #region Events
        /// <summary>
        /// Event raised when a progress report begins.
        /// </summary>
        public event ProgressReportBeginEventHandler ProgressReportBeginEvent;
        private void RaiseProgressReportBeginEvent(int min, int max, int step)
        {
            if (this.ProgressReportBeginEvent != null)
            {
                this.ProgressReportBeginEvent(min, max, step);
            }
        }

        /// <summary>
        /// Event raised when a progress report updates.
        /// </summary>
        public event ProgressReportUpdateEventHandler ProgressReportUpdateEvent;
        private void RaiseProgressReportUpdateEvent()
        {
            if (this.ProgressReportUpdateEvent != null)
            {
                this.ProgressReportUpdateEvent();
            }
        }

        /// <summary>
        /// Event raised when a progress report ends.
        /// </summary>
        public event SimpleEventHandler ProgressReportEndEvent;
        private void RaiseProgressReportEndEvent()
        {
            if (this.ProgressReportEndEvent != null)
            {
                this.ProgressReportEndEvent();
            }
        }
        #endregion Events

        #region Constructors
        /// <summary>
        /// Constructor for the class.
        /// </summary>
        /// <param name="proj">Project the metadata belongs to</param>
        //public MetadataDbProvider(Project proj, bool createDatabase) 
        public MetadataDbProvider(Project proj)
        {
            #region Input validation
            if (proj == null)
                throw new System.ArgumentNullException("proj");
            #endregion Input validation
            project = proj;
        }

        /// <summary>
        /// Constructor initialization.
        /// </summary>
        /// <param name="MetaDbInfo">Database driver information.</param>
        /// <param name="driver">Database driver name.</param>
        /// <param name="createDatabase">Create database flag.</param>
        public void Initialize(DbDriverInfo MetaDbInfo, string driver, bool createDatabase)
        {

            dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(driver);
            if (createDatabase)
            {
                this.CreateDatabase(MetaDbInfo); //\\ + Path.DirectorySeparatorChar + DbDriverInfo.PreferredDatabaseName);
                db = dbFactory.CreateDatabaseObject(MetaDbInfo.DBCnnStringBuilder);
                CreateMetadataTables();
            }
            else
            {

                db = dbFactory.CreateDatabaseObject(MetaDbInfo.DBCnnStringBuilder);

            }
            db.TestConnection();
        }

        #region Deprecated Code
        ///// <summary>
        ///// Creates database from the connectionstring.
        ///// </summary>
        ///// <param name="connectionString"></param>
        ///// <param name="driver"></param>
        ///// <param name="createDatabase"></param>
        //public void Initialize(string connectionString, string driver, bool createDatabase)
        //{

        //    dbFactory = DatabaseFactoryCreator.GetDbDriverFactory(driver);
        //    DbConnectionStringBuilder cnnStringBuilder = new DbConnectionStringBuilder();
        //    cnnStringBuilder.ConnectionString = connectionString;
        //    if (createDatabase)
        //    {
        //        this.CreateDatabase(new ConnectionStringInfo(connectionString).DbName);
        //        CreateMetadataTables();
        //    }
        //    else
        //    {

        //        db = dbFactory.CreateDatabaseObject(cnnStringBuilder);

        //    }
        //    //db = DatabaseFactoryCreator.CreateDatabaseInstance(driver, connectionString);




        //    //if (createDatabase)
        //    //{
        //    //    this.CreateDatabase(new ConnectionStringInfo(connectionString).DbName);
        //    //    CreateMetadataTables();
        //    //}
        //    db.TestConnection();

        //}
        #endregion

        #endregion Constructors

        #region Public Properties

        /// <summary>
        /// The project
        /// </summary>
        public Project Project
        {
            get
            {
                return project;
            }
        }
        /// <summary>
        /// MetaData DBFactory
        /// </summary>
        public IDbDriverFactory DBFactory
        {
            get { return dbFactory; }
        }


        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Get the next available data table name. The table does not have to
        /// exist with this method is called. If the maximum number of columns
        /// for a table been hit, it will go to the next table name following 
        /// this pattern:
        /// exampleTableName, exampleTableName_2, exampleTableName_3, ...
        /// </summary>
        /// <param name="viewName">Name of current view in Epi.Project.</param>
        /// <returns></returns>
        public string GetAvailDataTableName(string viewName)
        {
            int maxTableWidth = 10;
            int tableNameCount = 0;
            int tableNumberSuffix = 1;
            string tableName = viewName;
            DataTable dt;

            try
            {
                do
                {   // << build proposed data table name >>
                    if (tableNumberSuffix > 1)
                    {
                        tableName = viewName + "_" + tableNumberSuffix.ToString();
                    }

                    Query query = db.CreateQuery("SELECT COUNT(DataTableName) AS numTimesUsed "
                        + "FROM metaFields "
                        + "WHERE DataTableName = '"
                        + tableName
                        + "'");

                    dt = db.Select(query);
                    tableNameCount = (int)dt.Rows[0]["numTimesUsed"];

                    tableNumberSuffix++;
                }
                while (tableNameCount >= maxTableWidth);

                return tableName;
            }
            catch (Exception ex)
            {
                throw new System.ApplicationException("Could not retrieve an availble wide data table name.", ex);
            }
        }

        /// <summary>
        /// Gets list of code tables in the metadata database
        /// </summary>
        /// <returns>DataRow of table names</returns>
        public DataTable GetCodeTableList()
        {
            try
            {
                DataSets.TableSchema.TablesDataTable tables = db.GetTableSchema();

                //remove tables without prefix "code"
                DataRow[] rowsFiltered = tables.Select("TABLE_NAME not like 'code%'");
                foreach (DataRow rowFiltered in rowsFiltered)
                {
                    tables.Rows.Remove(rowFiltered);
                }
                DataRow[] rowsCode = tables.Select("TABLE_NAME like 'code%'");
                return tables;
            }
            catch (Exception ex)
            {
                throw new System.ApplicationException("Could not retrieve code tables from database", ex);
            }
        }

        /// <summary>
        /// Attaches a db driver object to this provider.
        /// </summary>
        /// <param name="dbDriver"></param>
        public void AttachDbDriver(IDbDriver dbDriver)
        {
            this.db = dbDriver;
        }

        #region Select Statements

        /// <summary>
        /// Gets all views belonging to a project
        /// </summary>
        /// <returns>Datatable containing view info</returns>
        public virtual DataTable GetViewsAsDataTable()
        {
            Query query = db.CreateQuery(
                "select [ViewId], [Name], [CheckCode], [CheckCodeBefore], [CheckCodeAfter], [RecordCheckCodeBefore], " + 
                "[RecordCheckCodeAfter], [CheckCodeVariableDefinitions], [IsRelatedView], [Width], [Height], [Orientation], [LabelAlign] " +
				"from metaViews" );
            return db.Select(query);
        }

        /// <summary>
        /// Gets all views belonging to an metadata Xml project
        /// </summary>
        /// <param name="currentViewElement">The view element</param>
        /// <param name="viewsNode">The view node</param>
        /// <returns></returns>
        public ViewCollection GetViews(XmlElement currentViewElement, XmlNode viewsNode)
        {
            return null;
        }

        /// <summary>
        /// Gets all views belonging to a project
        /// </summary>
        /// <returns>A collection of views</returns>
        public ViewCollection GetViews()
        {
            try
            {
                Collections.ViewCollection views = new Collections.ViewCollection();
                DataTable table = GetViewsAsDataTable();
                foreach (DataRow row in table.Rows)
                {
                    views.Add(new View(row, project));
                }
                return views;
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve view collection", ex);
            }
        }

        /// <summary>
        /// Gets all pages belonging to a view
        /// </summary>
        /// <param name="viewId">Id of the view</param>
        /// <returns>Datatable containing page info</returns>
        public DataTable GetPagesForView(int viewId)
        {
            try
            {
                #region Input Validation
                if (viewId < 1)
                {
                    throw new ArgumentOutOfRangeException("ViewId");
                }
                #endregion

                Query query = db.CreateQuery("select [PageId], [ViewId], [Name], [Position], [CheckCodeBefore], [CheckCodeAfter], [BackgroundId] " +
                    "from metaPages " +
                    "where [ViewId] = @ViewId " +
                    "order by [Position] asc");
                query.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, viewId));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve page", ex);
            }
        }

        /// <summary>
        /// Fetches the collection of pages of a view from metadata database.
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public virtual List<Page> GetViewPages(View view)
        {
            try
            {
                List<Page> pages = new List<Page>();
                DataTable table = GetPagesForView(view.Id);
                foreach (DataRow row in table.Rows)
                {
                    pages.Add(new Page(row, view));
                }
                return (pages);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve page collection", ex);
            }
        }

        /// <summary>
        /// Gets a view's check code for the "Before" event
        /// </summary>
        /// <param name="viewID">Id of the view</param>
        /// <returns>Datatable containing check code info</returns>
        [Obsolete("Use of DataTable in this context is no different than the use of a multidimensional System.Object array (not recommended).", false)]
        public DataTable GetViewCheckCode_Before(int viewID)
        {
            try
            {
                #region Input Validation
                if (viewID < 1)
                {
                    throw new ArgumentOutOfRangeException("ViewId");
                }
                #endregion

                Query query = db.CreateQuery("select V.[ViewID], V.[Name], V.[CheckCodeBefore] " +
                    "from metaViews V " +
                    "where V.[ViewId] = @ViewID");
                query.Parameters.Add(new QueryParameter("@ViewID", DbType.Int32, viewID));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve view's check code", ex);
            }
        }

        /// <summary>
        /// Gets a view's check code for the "After" event
        /// </summary>
        /// <param name="viewID">Id of the view</param>
        /// <returns>Datatable containing check code info</returns>
        [Obsolete("Use of DataTable in this context is no different than the use of a multidimensional System.Object array (not recommended).", false)]
        public DataTable GetViewCheckCode_After(int viewID)
        {
            try
            {
                #region Input Validation
                if (viewID < 1)
                {
                    throw new ArgumentOutOfRangeException("ViewId");
                }
                #endregion

                Query query = db.CreateQuery("select V.[ViewID], V.[Name], V.[CheckCodeAfter] " +
                    "from metaViews V " +
                    "where V.[ViewId] = @ViewID");
                query.Parameters.Add(new QueryParameter("@ViewID", DbType.Int32, viewID));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve view's check code", ex);
            }
        }

        /// <summary>
        /// Gets a view's variable check code
        /// </summary>
        /// <param name="viewID">Id of the view</param>
        /// <returns>Datatable containing variable check code info</returns>
        [Obsolete("Use of DataTable in this context is no different than the use of a multidimensional System.Object array (not recommended).", false)]
        public DataTable GetCheckCodeVariableDefinition(int viewID)
        {
            try
            {
                #region Input Validation
                if (viewID < 1)
                {
                    throw new ArgumentOutOfRangeException("ViewId");
                }
                #endregion

                Query query = db.CreateQuery("select V.[ViewID], V.[Name], V.[CheckCodeVariableDefinitions] " +
                    "from metaViews V " +
                    "where V.[ViewID] = @ViewID");
                query.Parameters.Add(new QueryParameter("@ViewID", DbType.Int32, viewID));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve check code variables", ex);
            }
        }

        /// <summary>
        /// Gets a view's record check code for the "Before" event
        /// </summary>
        /// <param name="viewID">Id of the view</param>
        /// <returns>Datatable containing record check code info</returns>
        [Obsolete("Use of DataTable in this context is no different than the use of a multidimensional System.Object array (not recommended).", false)]
        public DataTable GetRecordCheckCode_Before(int viewID)
        {
            try
            {
                #region Input Validation
                if (viewID < 1)
                {
                    throw new ArgumentOutOfRangeException("ViewId");
                }
                #endregion

                Query query = db.CreateQuery("select V.[ViewID], V.[Name], V.[RecordCheckCodeBefore] " +
                    "from metaViews V " +
                    "where v.[ViewID] = @ViewID");
                query.Parameters.Add(new QueryParameter("@ViewID", DbType.Int32, viewID));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve record check code", ex);
            }
        }

        /// <summary>
        /// Gets a FieldId based on a view name and a field name
        /// </summary>
        /// <param name="viewName">Name of the view</param>
        /// <param name="fieldName">Name of the field</param>
        /// <returns>Id of the control</returns>
        public DataRow GetFieldIdByNameAsDataRow(string viewName, string fieldName)
        {
            try
            {
                string sql = "select [FieldId] from (metaFields f inner join metaViews v on v.[ViewId] = f.[ViewId]) where v.[Name] = @ViewName and f.[Name] = @FieldName";
                Query query = db.CreateQuery(sql);
                query.Parameters.Add(new QueryParameter("@ViewName", DbType.String, viewName, int.MaxValue));
                query.Parameters.Add(new QueryParameter("@FieldName", DbType.String, fieldName, int.MaxValue));
                DataTable dt = db.Select(query);
                if (dt.Rows.Count == 0)
                {
                    throw new GeneralException("Field not found");
                }
                return dt.Rows[0];
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve control based on view and field name", ex);
            }
        }

        public DataRow GetFieldGUIDByNameAsDataRow(string viewName, string fieldName)
        {
            try
            {
                string sql = "select [UniqueId] from (metaFields f inner join metaViews v on v.[ViewId] = f.[ViewId]) where v.[Name] = @ViewName and f.[Name] = @FieldName";
                Query query = db.CreateQuery(sql);
                query.Parameters.Add(new QueryParameter("@ViewName", DbType.String, viewName));
                query.Parameters.Add(new QueryParameter("@FieldName", DbType.String, fieldName));
                DataTable dt = db.Select(query);
                if (dt.Rows.Count == 0)
                {
                    throw new GeneralException("Field not found");
                }
                return dt.Rows[0];
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve control based on view and field name", ex);
            }
        }

        /// <summary>
        /// Gets a Field based on a view name and a field name
        /// </summary>
        /// <param name="viewId">Id of the view</param>
        /// <param name="fieldName">Name of the field</param>
        /// <returns></returns>
        public DataRow GetFieldAsDataRow(int viewId, string fieldName)
        {
            try
            {
                string sql = "select * from metaFields where  [ViewId] = @ViewId and [Name] = @FieldName";
                Query query = db.CreateQuery(sql);
                query.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, viewId, int.MaxValue));
                query.Parameters.Add(new QueryParameter("@FieldName", DbType.String, fieldName, int.MaxValue));
                DataTable dt = db.Select(query);
                if (dt.Rows.Count == 0)
                {
                    throw new GeneralException("Field not found");
                }
                return dt.Rows[0];
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve control based on view and field name", ex);
            }
        }

        /// <summary>
        /// Gets a view object based on view id
        /// </summary>
        /// <param name="viewId">Id of the view</param>
        /// <returns>A view object</returns>
        public View GetViewById(int viewId)
        {
            try
            {
                Query query = db.CreateQuery("select [ViewId], [Name], [CheckCode], [CheckCodeBefore], [CheckCodeAfter], [RecordCheckCodeBefore], [RecordCheckCodeAfter], [CheckCodeVariableDefinitions], [IsRelatedView] " +
                    "from metaViews " +
                    "where [ViewId] = @ViewId");
                query.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, viewId));
                DataTable results = db.Select(query);
                if (results.Rows.Count > 0)
                {
                    return new View(results.Rows[0], Project);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve view", ex);
            }
        }



        /// <summary>
        /// Gets a view object based on view id
        /// </summary>
        /// <param name="viewId">Id of the view</param>
        /// <returns>A view object</returns>
        public DataTable GetPublishedViewKeys(int viewId)
        {
            try
                {
                Query query = db.CreateQuery("select [EIWSOrganizationKey] ,[EIWSFormId] ,[EWEOrganizationKey] ,[EWEFormId]  " +
                    "from metaViews " +
                    "where [ViewId] = @ViewId");
                query.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, viewId));
                DataTable results = db.Select(query);
                return results;
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// Gets a view's record check code for the "After" event
        /// </summary>
        /// <param name="viewID">Id of the view</param>
        /// <returns>Datatable containing record check code info</returns>
        [Obsolete("Use of DataTable in this context is no different than the use of a multidimensional System.Object array (not recommended).", false)]
        public DataTable GetRecordCheckCode_After(int viewID)
        {
            try
            {
                #region Input Validation
                if (viewID < 1)
                {
                    throw new ArgumentOutOfRangeException("ViewId");
                }
                #endregion

                Query query = db.CreateQuery("select V.[ViewID], V.[Name], V.[RecordCheckCodeAfter] " +
                    "from metaViews V " +
                    "where v.[ViewID] = @ViewID");
                query.Parameters.Add(new QueryParameter("@ViewID", DbType.Int32, viewID));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve record check code", ex);
            }
        }

        /// <summary>
        /// Gets a page's check code for the "Before" event
        /// </summary>
        /// <param name="page">The page of the view</param>
        /// <returns>Datatable containing page check code info</returns>
        public DataTable GetPageCheckCode_Before(Page page)
        {
            try
            {
                #region Input Validation
                if (page == null)
                {
                    throw new ArgumentNullException("Page");
                }
                #endregion

                Query query = db.CreateQuery("select P.[PageID], P.[Name], P.[CheckCodeBefore] " +
                    "from metaPages P " +
                    "where P.[PageID] = @PageID");
                query.Parameters.Add(new QueryParameter("@PageID", DbType.Int32, page.Id));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve page check code", ex);
            }
        }

        /// <summary>
        /// Gets a page's check code for the "After" event
        /// </summary>
        /// <param name="page">The page of the view</param>
        /// <returns>Datatable containing page check code info</returns>
        public DataTable GetPageCheckCode_After(Page page)
        {
            try
            {
                #region Input Validation
                if (page == null)
                {
                    throw new ArgumentNullException("Page");
                }
                #endregion

                Query query = db.CreateQuery("select P.[PageID], P.[Name], P.[CheckCodeAfter] " +
                    "from metaPages P " +
                    "where P.[PageID] = @PageID");
                query.Parameters.Add(new QueryParameter("@PageID", DbType.Int32, page.Id));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve page check code", ex);
            }
        }

        /// <summary>
        /// Gets a control's check code for the "Before" event
        /// </summary>
        /// <param name="fieldID">Id of the field</param>
        /// <returns>Datatable containing field check code info</returns>
        public DataTable GetFieldCheckCode_Before(int fieldID)
        {
            try
            {
                #region Input Validation
                if (fieldID < 1)
                {
                    throw new ArgumentOutOfRangeException("FieldId");
                }
                #endregion

                Query query = db.CreateQuery("select [CheckCodeBefore], [Name] from metaFields " +
                    "where [FieldID] = @FieldID");
                query.Parameters.Add(new QueryParameter("@FieldID", DbType.Int32, fieldID));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve control check code", ex);
            }
        }

        /// <summary>
        /// Gets a control's check code for the "After" event
        /// </summary>
        /// <param name="fieldID">Id of the field</param>
        /// <returns>Datatable containing field check code info</returns>
        public DataTable GetFieldCheckCode_After(int fieldID)
        {
            try
            {
                #region Input Validation
                if (fieldID < 1)
                {
                    throw new ArgumentOutOfRangeException("FieldId");
                }
                #endregion

                Query query = db.CreateQuery("select [CheckCodeAfter], [Name] from metaFields " +
                    "where [FieldID] = @FieldID");
                query.Parameters.Add(new QueryParameter("@FieldID", DbType.Int32, fieldID));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve control check code", ex);
            }
        }

        /// <summary>
        /// Returns check codes for views, records, and variables
        /// </summary>
        /// <param name="viewID">View ID</param>
        /// <returns>datatable</returns>
        public DataTable GetViewRecordVarCheckCodes(int viewID)
        {
            try
            {
                #region Input Validation
                if (viewID < 1)
                {
                    throw new ArgumentOutOfRangeException("ViewId");
                }
                #endregion

                Query query = db.CreateQuery("select [ViewID], [Name], [CheckCode], [CheckCodeBefore], [CheckCodeAfter], " +
                    "[RecordCheckCodeBefore], [RecordCheckCodeAfter], [CheckCodeVariableDefinitions] " +
                    "from metaViews " +
                    "where [ViewID] = @ViewID");
                query.Parameters.Add(new QueryParameter("@ViewID", DbType.Int32, viewID));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve check codes for views, records, and variables", ex);
            }
        }

        /// <summary>
        /// Gets a related view based on a relate button
        /// </summary>
        /// <param name="field">The relate button field</param>
        /// <returns>The related view</returns>
        public View GetChildView(RelatedViewField field)
        {
            try
            {
                Query query = db.CreateQuery("select V.[ViewId], V.[Name], V.[CheckCode], V.[CheckCodeBefore], V.[CheckCodeAfter], V.[RecordCheckCodeBefore], V.[RecordCheckCodeAfter], V.[CheckCodeVariableDefinitions], V.[IsRelatedView] from metaViews V " +
                    "inner join metaFields F On F.[RelatedViewId] = V.[ViewId] " +
                    "where F.[FieldId] = @FieldId");
                query.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, field.Id));
                DataTable table = db.Select(query);
                if (table.Rows.Count > 0)
                {
                    return new View(table.Rows[0], field.GetProject());
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve related view", ex);
            }
        }

        /// <summary>
        /// Gets all the controls belonging to a page
        /// </summary>
        /// <param name="pageId">Id of the page</param>
        /// <returns>Datatable containing control info</returns>
        public DataTable GetControlsForPage(int pageId)
        {
            try
            {
                #region Input Validation
                if (pageId < 1)
                {
                    throw new ArgumentOutOfRangeException("PageId");
                }
                #endregion

                Query query = db.CreateQuery("select [FieldTypeId], [Name] As FieldName, [ControlTopPositionPercentage] As ControlTop, " +
                    "[ControlLeftPositionPercentage] As ControlLeft, [ControlHeightPercentage], [ControlWidthPercentage], [PromptText], [PromptFontSize], " +
                    "[PromptTopPositionPercentage] AS LabelTop, [PromptLeftPositionPercentage] As LabelLeft, [FieldId] " +
                    "from metaFields " +
                    //				DbQuery query = db.CreateQuery("select F.[FieldTypeId], F.[Name] AS FieldName, C.[TopPosition] AS ControlTop, C.[LeftPosition] AS ControlLeft, C.[Height], C.[Width], L.[Text], L.[FontSize], L.[TopPosition] AS LabelTop, L.[LeftPosition] AS LabelLeft, C.[ControlId], L.[LabelId], F.[FieldId] " +
                    //					"from (metaFields F LEFT JOIN metaControls C on C.[FieldId] = F.[FieldId]) LEFT JOIN metaLabels L on L.[FieldId] = F.[FieldId] " +
                    "where [PageId] = @PageId " +
                    "order by [FieldTypeId]");
                query.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, pageId));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve controls", ex);
            }
        }

        /// <summary>
        /// Gets the system fields belonging to a view
        /// </summary>
        /// <param name="viewId">Id of the view</param>
        /// <returns>Datatable containing field info</returns>
        public DataTable GetSystemFields(int viewId)
        {
            try
            {
                #region Input Validation
                if (viewId < 1)
                {
                    throw new ArgumentOutOfRangeException("ViewId");
                }
                #endregion

                Query query = db.CreateQuery("select F.[Name], F.[PageId], F.[FieldId], F.[FieldTypeId], F.[CheckCodeAfter] As ControlAfterCheckCode, F.[CheckCodeBefore] As ControlBeforeCheckCode,  " +
                    "F.[ControlTopPositionPercentage], F.[ControlLeftPositionPercentage], F.[ControlHeightPercentage], F.[ControlWidthPercentage] , F.[ControlFontFamily], F.[ControlFontSize], F.[ControlFontStyle], F.[ControlScriptName], " +
                    "F.[PromptTopPositionPercentage], F.[PromptLeftPositionPercentage], F.[PromptText], F.[PromptFontFamily], F.[PromptFontSize], F.[PromptFontStyle], F.[ControlFontFamily], F.[ControlFontSize], F.[ControlFontStyle], F.[PromptScriptName], " +
                    "F.[ShouldRepeatLast], F.[IsRequired], F.[IsReadOnly],  " +
                    "F.[ShouldRetainImageSize], F.[Pattern], F.[MaxLength], F.[ShowTextOnRight], " +
                    "F.[Lower], F.[Upper], F.[RelateCondition], F.[ShouldReturnToParent], F.[RelatedViewId], " +
                    "F.[SourceTableName], F.[CodeColumnName], F.[TextColumnName], " +
                    "F.[Sort], F.[IsExclusiveTable], F.[TabIndex], F.[HasTabStop],F.[SourceFieldId] " +
                    "from metaFields F " +
                    "where F.[ViewId] = @ViewId and F.[PageId] is null");
                query.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, viewId));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve fields", ex);
            }
        }

        /// <summary>
        /// Gets all fields belonging to a page
        /// </summary>
        /// <param name="pageId">Id of the page</param>
        /// <returns>Datatable containing field info</returns>
        public DataTable GetFieldsOnPageAsDataTable(int pageId)
        {
            try
            {
                #region Input Validation
                if (pageId < 1)
                {
                    throw new ArgumentOutOfRangeException("PageId");
                }
                #endregion

                Query query = db.CreateQuery("select F.[Name], F.[PageId], F.[FieldId], F.[UniqueId], F.[FieldTypeId], F.[CheckCodeAfter] As ControlAfterCheckCode, F.[CheckCodeBefore] As ControlBeforeCheckCode,  " +
                    "P.[Name] AS PageName, P.[CheckCodeBefore] As PageBeforeCheckCode, P.[CheckCodeAfter] As PageAfterCheckCode, P.[Position], " +
                    "F.[ControlTopPositionPercentage], F.[ControlLeftPositionPercentage], F.[ControlHeightPercentage], F.[ControlWidthPercentage] , F.[ControlFontFamily], F.[ControlFontSize], F.[ControlFontStyle], F.[ControlScriptName], " +
                    "F.[PromptTopPositionPercentage], F.[PromptLeftPositionPercentage], F.[PromptText], F.[PromptFontFamily], F.[PromptFontSize], F.[PromptFontStyle], F.[ControlFontFamily], F.[ControlFontSize], F.[ControlFontStyle], F.[PromptScriptName], " +
                    "F.[ShouldRepeatLast], F.[IsRequired], F.[IsReadOnly], F.[IsEncrypted],  " +
                    "F.[ShouldRetainImageSize], F.[Pattern], F.[MaxLength], F.[ShowTextOnRight], " +
                    "F.[Lower], F.[Upper], F.[RelateCondition], F.[ShouldReturnToParent], F.[RelatedViewId], F.[List]," +
                    "F.[SourceTableName], F.[CodeColumnName], F.[TextColumnName],  F.[BackgroundColor], " +
                    "F.[Sort], F.[IsExclusiveTable], F.[TabIndex], F.[HasTabStop],F.[SourceFieldId] " +
                    "from ((metaFields F " +
                    "LEFT JOIN metaPages P on P.[PageId] = F.[PageId]) " +
                    "LEFT JOIN metaViews V on V.[ViewId] = P.[ViewId]) " +
                    "where F.[PageId] = @pageID " +
                    "order by F.[ControlTopPositionPercentage], F.[ControlLeftPositionPercentage]");
                query.Parameters.Add(new QueryParameter("@pageID", DbType.Int32, pageId));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve fields", ex);
            }
        }

        /// <summary>
        /// Returns a field as a data row
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public DataRow GetFieldAsDataRow(Field field)
        {
            string queryString =
                "select F.[FieldId], F.[UniqueId] As UniqueId, F.[Name] As Name, F.[PageId], F.[ViewId], F.[FieldTypeId], F.[CheckCodeAfter] As ControlAfterCheckCode, F.[CheckCodeBefore] As ControlBeforeCheckCode,  " +
                "F.[ControlTopPositionPercentage], F.[ControlLeftPositionPercentage], F.[ControlHeightPercentage], F.[ControlWidthPercentage], " +
                "F.[ControlFontFamily] As ControlFontFamily, F.[ControlFontSize] As ControlFontSize, F.[ControlFontStyle] As ControlFontStyle, F.[ControlScriptName], " +
                "F.[PromptTopPositionPercentage], F.[PromptLeftPositionPercentage], F.[PromptText], F.[PromptFontFamily], F.[PromptFontSize], F.[PromptFontStyle], F.[ControlFontFamily], F.[ControlFontSize], F.[ControlFontStyle], F.[PromptScriptName], " +
                "F.[ShouldRepeatLast], F.[IsRequired], F.[IsReadOnly],  " +
                "F.[ShouldRetainImageSize], F.[Pattern], F.[MaxLength], F.[ShowTextOnRight], " +
                "F.[Lower], F.[Upper], F.[RelateCondition], F.[ShouldReturnToParent], F.[RelatedViewId], " +
                "F.[SourceTableName], F.[CodeColumnName], F.[TextColumnName], " +
                "F.[List], F.[BackgroundColor], " +
                "F.[Sort], F.[IsExclusiveTable], F.[TabIndex], F.[HasTabStop],F.[SourceFieldId], F.[DataTableName] " +
                "from metaFields F where F.[ViewId] = @viewId AND F.[UniqueId] = @uniqueId " +
                "order by F.[ControlTopPositionPercentage], F.[ControlLeftPositionPercentage]";

            Query query = db.CreateQuery(queryString);
            query.Parameters.Add(new QueryParameter("@viewID", DbType.Int32, field.GetView().Id));
            query.Parameters.Add(new QueryParameter("@uniqueId", DbType.Guid, field.UniqueId));
            
            DataTable table = db.Select(query);
            if (table.Rows.Count > 0)
            {
                return table.Rows[0];
            }

            return null;
        }
        
        /// <summary>
        /// Returns view's fields as a data table
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public DataTable GetFieldsAsDataTable(View view)
        {
            try
            {
                string queryString =
                    "select F.[FieldId], F.[UniqueId], F.[Name] As Name, F.[PageId], F.[ViewId], F.[FieldTypeId], F.[CheckCodeAfter] As ControlAfterCheckCode, F.[CheckCodeBefore] As ControlBeforeCheckCode,  " +
                    "F.[ControlTopPositionPercentage], F.[ControlLeftPositionPercentage], F.[ControlHeightPercentage], F.[ControlWidthPercentage], " +
                    "F.[ControlFontFamily] As ControlFontFamily, F.[ControlFontSize] As ControlFontSize, F.[ControlFontStyle] As ControlFontStyle, F.[ControlScriptName], " +
                    "F.[PromptTopPositionPercentage], F.[PromptLeftPositionPercentage], F.[PromptText], F.[PromptFontFamily], F.[PromptFontSize], F.[PromptFontStyle], F.[ControlFontFamily], F.[ControlFontSize], F.[ControlFontStyle], F.[PromptScriptName], " +
                    "F.[ShouldRepeatLast], F.[IsRequired], F.[IsReadOnly], F.[IsEncrypted], " +
                    "F.[ShouldRetainImageSize], F.[Pattern], F.[MaxLength], F.[ShowTextOnRight], " +
                    "F.[Lower], F.[Upper], F.[RelateCondition], F.[ShouldReturnToParent], F.[RelatedViewId], " +
                    "F.[SourceTableName], F.[CodeColumnName], F.[TextColumnName], " +
                    "F.[List], F.[BackgroundColor], " +
                    "F.[Sort], F.[IsExclusiveTable], F.[TabIndex], F.[HasTabStop],F.[SourceFieldId], F.[DataTableName] " +
                    "from metaFields F where F.[ViewId] = @viewId " +
                    "order by F.[ControlTopPositionPercentage], F.[ControlLeftPositionPercentage]";

                Query query = db.CreateQuery(queryString);
                query.Parameters.Add(new QueryParameter("@viewId", DbType.Int32, view.Id));
                return db.Select(query);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Gets field metadata needed for synchronizing the view's data tables
        /// </summary>
        /// <param name="view">View</param>
        /// <returns>A DataTable containing [ColumnNames.NAME],[ColumnNames.FIELD_TYPE_ID],[ColumnNames.DATA_TABLE_NAME]</returns>
        public DataTable GetFieldMetadataSync(View view)
        {
            try
            {
                string queryString =
                    "SELECT "
                    + "mF.[" + ColumnNames.NAME + "], " 
                    + "mF.[" + ColumnNames.FIELD_TYPE_ID + "], "
                    + "mF.[" + ColumnNames.DATA_TABLE_NAME + "] " 
                    + "FROM metaFields mF "
                    + "WHERE mF.[ViewId] = @viewId";

                Query query = db.CreateQuery(queryString);
                query.Parameters.Add(new QueryParameter("@viewID", DbType.Int32, view.Id));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve fields", ex);
            }
        }


        /// <summary>
        /// Gets field metadata needed for synchronizing the view's data tables
        /// </summary>
        /// <param name="view">View</param>
        /// <returns>A DataTable containing [ColumnNames.NAME],[ColumnNames.FIELD_TYPE_ID],[ColumnNames.DATA_TABLE_NAME]</returns>
        public DataTable GetFieldMetadataSync(int pageId)
        {
            try
            {
                string queryString =
                    "SELECT "
                    + "mF.[" + ColumnNames.NAME + "], "
                    + "mF.[" + ColumnNames.FIELD_TYPE_ID + "], "
                    + "mF.[" + ColumnNames.DATA_TABLE_NAME + "] "
                    + "FROM metaFields mF "
                    + "WHERE mF.[PageId] = @PageId";

                Query query = db.CreateQuery(queryString);
                query.Parameters.Add(new QueryParameter("@PageID", DbType.Int32, pageId));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve fields", ex);
            }
        }


        /// <summary>
        /// Gets all the fields in a view
        /// </summary>
        /// <param name="view">A view object</param>
        /// <returns>A collection of fields</returns>
        public virtual FieldCollectionMaster GetFields(View view)
        {
            FieldCollectionMaster fields = new FieldCollectionMaster();
            try
            {
                DataTable table = GetFieldsAsDataTable(view);
                foreach (DataRow row in table.Rows)
                {
                    MetaFieldType fieldTypeId = (MetaFieldType)row[ColumnNames.FIELD_TYPE_ID];
                    Field field = null;
                    switch (fieldTypeId)
                    {
                        case MetaFieldType.Text:
                            field = new SingleLineTextField(view);
                            field.LoadFromRow(row);
                            break;
                        case MetaFieldType.LabelTitle:
                            field = new LabelField(view);
                            field.LoadFromRow(row);
                            break;
                        case MetaFieldType.TextUppercase:
                            field = new UpperCaseTextField(view);
                            field.LoadFromRow(row);
                            break;
                        case MetaFieldType.Multiline:
                            field = new MultilineTextField(view);
                            field.LoadFromRow(row);
                            break;
                        case MetaFieldType.Number:
                            field = new NumberField(view);
                            field.LoadFromRow(row);
                            break;
                        case MetaFieldType.PhoneNumber:
                            field = new PhoneNumberField(view);
                            field.LoadFromRow(row);
                            break;
                        case MetaFieldType.Date:
                            field = new DateField(view);
                            field.LoadFromRow(row);
                            break;
                        case MetaFieldType.Time:
                            field = new TimeField(view);
                            field.LoadFromRow(row);
                            break;
                        case MetaFieldType.DateTime:
                            field = new DateTimeField(view);
                            field.LoadFromRow(row);
                            break;
                        case MetaFieldType.Checkbox:
                            field = new CheckBoxField(view);
                            field.LoadFromRow(row);
                            break;
                        case MetaFieldType.YesNo:
                            field = new YesNoField(view);
                            field.LoadFromRow(row);
                            break;
                        case MetaFieldType.Option:
                            field = new OptionField(view);
                            field.LoadFromRow(row);
                            break;
                        case MetaFieldType.CommandButton:
                            field = new CommandButtonField(view);
                            field.LoadFromRow(row);
                            break;
                        case MetaFieldType.Image:
                            field = new ImageField(view);
                            field.LoadFromRow(row);
                            break;
                        case MetaFieldType.Mirror:
                            field = new MirrorField(view);
                            field.LoadFromRow(row);
                            break;
                        case MetaFieldType.Grid:
                            field = new GridField(view);
                            field.LoadFromRow(row);
                            break;
                        case MetaFieldType.Group:
                            field = new GroupField(view);
                            field.LoadFromRow(row);
                            break;
                        case MetaFieldType.GUID:
                            field = new GUIDField(view);
                            field.LoadFromRow(row);
                            break;
                        case MetaFieldType.LegalValues:
                            field = new DDLFieldOfLegalValues(view);
                            field.LoadFromRow(row);
                            break;
                        case MetaFieldType.Codes:
                            field = new DDLFieldOfCodes(view);
                            ((DDLFieldOfCodes)field).LoadFromRow(row);  //zack 
                            break;
                        case MetaFieldType.List:
                            field = new DDListField(view);
                            ((DDListField)field).LoadFromRow(row);
                            break;
                        case MetaFieldType.CommentLegal:
                            field = new DDLFieldOfCommentLegal(view);
                            field.LoadFromRow(row);
                            break;
                        case MetaFieldType.Relate:
                            field = new RelatedViewField(view);
                            field.LoadFromRow(row);
                            break;
                        case MetaFieldType.RecStatus:
                            field = new RecStatusField(view);
                            field.LoadFromRow(row);
                            break;
                        ////----123
                        //case MetaFieldType.FirstSaveTime:
                        //    field = new FirstSaveTimeField(view);
                        //    field.LoadFromRow(row);
                        //    break;
                        //case MetaFieldType.LastSaveTime:
                        //    field = new LastSaveTimeField(view);
                        //    field.LoadFromRow(row);
                        //    break;
                        ////----
                        case MetaFieldType.UniqueKey:
                            field = new UniqueKeyField(view);
                            field.LoadFromRow(row);
                            break;
                        case MetaFieldType.ForeignKey:
                            field = new ForeignKeyField(view);
                            field.LoadFromRow(row);
                            break;
                        case MetaFieldType.GlobalRecordId:
                            field = new GlobalRecordIdField(view);
                            field.LoadFromRow(row);
                            break;
                        default:
                            if ((int)fieldTypeId != 29 && (int)fieldTypeId != 30)
                            {
                                throw new ApplicationException("Invalid Field Type");
                            }
                            break;
                    }
                    // Note: This check ideally shouldn't be necessary, but Epi 3.5.1 and previous versions actually do allow duplicate field names for group fields.

                    if (field != null)
                    {
                        if (fields.Contains(field))
                        {
                            Logger.Log(DateTime.Now + ":  " + string.Format("The {0} field with name \"{1}\" already exists in {2}. This field has not been imported.", field.GetType().ToString(), field.Name, view.Name));
                        }
                        else
                        {
                            fields.Add(field);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                //
            }
            return (fields);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public List<GridColumnBase> GetGridColumnCollection(GridField field)
        {
            List<GridColumnBase> columns = new List<GridColumnBase>();
            DataTable table = GetGridColumns(field.Id);
            foreach (DataRow row in table.Rows)
            {
                switch ((MetaFieldType)row["FieldTypeId"])
                {
                    case MetaFieldType.GlobalRecordId:
                        columns.Add(new GlobalRecordIdColumn(row, field));
                        break;
                    case MetaFieldType.UniqueKey:
                        columns.Add(new UniqueKeyColumn(row, field));
                        break;
                    case MetaFieldType.UniqueRowId:
                        columns.Add(new UniqueRowIdColumn(row, field));
                        break;
                    case MetaFieldType.RecStatus:
                        columns.Add(new RecStatusColumn(row, field));
                        break;
                    case MetaFieldType.ForeignKey:
                        columns.Add(new ForeignKeyColumn(row, field));
                        break;
                    case MetaFieldType.Text:
                        columns.Add(new TextColumn(row, field));
                        break;
                    case MetaFieldType.Checkbox:
                        columns.Add(new CheckboxColumn(row, field));
                        break;
                    case MetaFieldType.YesNo:
                        columns.Add(new YesNoColumn(row, field));
                        break;
                    case MetaFieldType.Number:
                        columns.Add(new NumberColumn(row, field));
                        break;
                    case MetaFieldType.PhoneNumber:
                        columns.Add(new PhoneNumberColumn(row, field));
                        break;
                    case MetaFieldType.Date:
                        columns.Add(new DateColumn(row, field));
                        break;
                    case MetaFieldType.Time:
                        columns.Add(new TimeColumn(row, field));
                        break;
                    case MetaFieldType.DateTime:
                        columns.Add(new DateTimeColumn(row, field));
                        break;
                    case MetaFieldType.CommentLegal:
                        columns.Add(new DDLColumnOfCommentLegal(row, field));
                        break;
                    case MetaFieldType.LegalValues:
                        columns.Add(new DDLColumnOfLegalValues(row, field));
                        break;
                    default:
                        break;
                }
            }
            return columns;
        }

        /// <summary>
        /// Gets all columns belonging to a grid control
        /// </summary>
        /// <param name="fieldId">Id of the grid control</param>
        /// <returns>Datatable containing column info</returns>
        public DataTable GetGridColumns(int fieldId)
        {
            try
            {
                #region Input Validation
                if (fieldId < 1)
                {
                    throw new ArgumentOutOfRangeException("FieldId");
                }
                #endregion

                Query query = db.CreateQuery("select * from metaGridColumns G where G.[FieldId] = @FieldId");

                query.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, fieldId));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve grid column", ex);
            }
        }

        /// <summary>
        /// Returns the field's tab index
        /// </summary>
        /// <param name="fieldId">The id of the field</param>
        /// <param name="viewId">The id of the field's view</param>
        /// <param name="pageId">The id of the page the field belongs to</param>
        /// <returns>Integer of tab index</returns>
        public int GetFieldTabIndex(int fieldId, int viewId, int pageId)
        {
            Query selectQuery = db.CreateQuery("select TabIndex from metaFields where PageId = @pageId and ViewId = @viewId and FieldId = @fieldId");
            selectQuery.Parameters.Add(new QueryParameter("@pageId", DbType.Int32, pageId));
            selectQuery.Parameters.Add(new QueryParameter("@viewId", DbType.Int32, viewId));
            selectQuery.Parameters.Add(new QueryParameter("@fieldId", DbType.Int32, fieldId));
            object result = db.ExecuteScalar(selectQuery);
            if (result != DBNull.Value)
            {
                return (int)result;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Gets all field types
        /// </summary>
        /// <returns>Datatable containing field type info</returns>
        public DataTable GetFieldTypes()
        {
            try
            {
                Query query = db.CreateQuery("select F.[FieldTypeId], D.[DataTypeId], F.[Name], D.[HasPattern], D.[HasSize], F.[HasRepeatLast], F.[HasRequired], F.[HasReadOnly], F.[HasRetainImageSize], D.[HasRange], F.[HasFont], F.[IsGridColumn], F.[IsDropDown] " +
                    "from metaFieldTypes F LEFT OUTER JOIN metaDatatypes D on F.[DataTypeId] = D.[DataTypeId] " +
                    "where F.[IsSystem] = @IsSystem");
                query.Parameters.Add(new QueryParameter("@IsSystem", System.Data.DbType.Boolean, false));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve field types", ex);
            }
        }

        /// <summary>
        /// Gets all stored map layers
        /// </summary>
        /// <returns>Datatable</returns>
        public MapMetadata.metaLayersDataTable GetLayers()
        {
            try
            {
                MapMetadata.metaLayersDataTable table = new MapMetadata.metaLayersDataTable();
                Query query = db.CreateQuery("select [LayerId], [Name], [Description] from metaLayers");
                table.Merge(db.Select(query));
                return table;
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve map layers", ex);
            }
        }

        /// <summary>
        /// Gets a Map Layer
        /// </summary>
        /// <param name="mapLayerId">Map Layer Id</param>
        /// <returns>MapLayersRow</returns>
        public MapMetadata.metaMapLayersRow GetMapLayer(int mapLayerId)
        {
            try
            {
                MapMetadata map = new MapMetadata();
                Query query = db.CreateQuery("select [MapLayerId], [MapId], [LayerId], [LayerRenderTypeId], [RenderField], [MarkerColor], [RampBeginColor], [RampEndColor], [ClassBreaks] from metaMapLayers where [MapLayerId] = @MapLayerId");
                query.Parameters.Add(new QueryParameter("@MapLayerId", System.Data.DbType.Int32, mapLayerId));
                map.metaMapLayers.Merge(db.Select(query));
                if (map.metaMapLayers.Count > 0)
                {
                    map.metaLayers.ImportRow(GetLayer(map.metaMapLayers[0].LayerId));
                    return map.metaMapLayers[0];
                }
                else
                {
                    throw new GeneralException("Map layer not found");
                }
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve map layer", ex);
            }
        }

        /// <summary>
        /// Gets all the maps of a project
        /// </summary>
        /// <returns>MapsDataTable</returns>
        public MapMetadata.metaMapsDataTable GetMaps()
        {
            try
            {
                MapMetadata map = new MapMetadata();
                Query query = db.CreateQuery("select [MapId], [Name], [Title] from metaMaps");
                map.metaMaps.Merge(db.Select(query));
                return map.metaMaps;
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve maps", ex);
            }
        }

        /// <summary>
        /// Gets a map by its ID
        /// </summary>
        /// <param name="mapId">Map Id</param>
        /// <returns>MapsDataTable</returns>
        public MapMetadata.metaMapsDataTable GetMap(int mapId)
        {
            try
            {
                MapMetadata map = new MapMetadata();
                Query query = db.CreateQuery("select [MapId], [Name], [Title] from metaMaps where [MapId] = @MapId");
                query.Parameters.Add(new QueryParameter("@MapId", DbType.Int32, mapId));
                map.metaMaps.Merge(db.Select(query));
                return map.metaMaps;
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve map", ex);
            }
        }

        /// <summary>
        /// Creates a map record in the database
        /// </summary>
        /// <param name="name">Name of the map</param>
        /// <param name="description">Description of the map</param>
        /// <returns>The map's ID</returns>
        public int CreateMap(string name, string description)
        {
            try
            {
                Query insertQuery = db.CreateQuery("insert into metaMaps ([Name], [Title]) values (@Name, @Description)");
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, name));
                insertQuery.Parameters.Add(new QueryParameter("@Description", DbType.String, description));
                db.ExecuteNonQuery(insertQuery);
                return GetMaxMapId();
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not insert map record", ex);
            }
        }

        /// <summary>
        /// Creates a map layer in the database
        /// </summary>
        /// <param name="gml">GML document</param>
        /// <param name="gmlSchema">GML schema document</param>
        /// <param name="layerName">Name of the layer</param>
        /// <returns>The layer's ID</returns>
        public int CreateLayer(string gml, string gmlSchema, string layerName)
        {
            Query insertQuery = db.CreateQuery("insert into metaLayers ([Gml], [GmlSchema], [Name]) values (@Gml, @GmlSchema, @Name)");
            insertQuery.Parameters.Add(new QueryParameter("@Gml", DbType.String, gml));
            insertQuery.Parameters.Add(new QueryParameter("@GmlSchema", DbType.String, gmlSchema));
            insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, layerName));
            db.ExecuteNonQuery(insertQuery);
            return GetMaxLayerId();
        }

        /// <summary>
        /// Creates a map layer in the database
        /// </summary>
        /// <param name="fileName">Shapefile name</param>
        /// <returns>The layer's ID</returns>
        public int CreateLayer(string fileName)
        {
            Query insertQuery = db.CreateQuery("insert into metaLayers ([FileName], [Name]) values (@FileName, @Name)");
            insertQuery.Parameters.Add(new QueryParameter("@FileName", DbType.String, fileName));
            insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, Path.GetFileNameWithoutExtension(fileName)));
            db.ExecuteNonQuery(insertQuery);
            return GetMaxLayerId();
        }

        /// <summary>
        /// Gets all layers of a map
        /// </summary>
        /// <param name="mapId">Map Id</param>
        /// <returns>Map Layers Datatable</returns>
        public MapMetadata.metaMapLayersDataTable GetMapLayers(int mapId)
        {
            try
            {
                MapMetadata map = new MapMetadata();
                Query query = db.CreateQuery("select [MapLayerId], [MapId], [LayerId], [LayerRenderTypeId], [RenderField], [MarkerColor], [RampBeginColor], [RampEndColor], [ClassBreaks], [DataTableName], [DataTableKey], [FeatureKey], [LineColor], [FillColor], [PolygonOutlineColor] from metaMapLayers where [MapId] = @MapId order by [MapLayerId] asc");
                query.Parameters.Add(new QueryParameter("@MapId", System.Data.DbType.Int32, mapId));
                map.metaMapLayers.Merge(db.Select(query));
                foreach (MapMetadata.metaMapLayersRow mapLayer in map.metaMapLayers)
                {
                    map.metaLayers.ImportRow(GetLayer(mapLayer.LayerId));
                }
                return map.metaMapLayers;
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve map layers", ex);
            }
        }

        /// <summary>
        /// Gets all points of a map
        /// </summary>
        /// <param name="mapId">Map Id</param>
        /// <returns>Map Points Datatable</returns>
        public MapMetadata.metaMapPointsDataTable GetMapPoints(int mapId)
        {
            try
            {
                MapMetadata map = new MapMetadata();
                Query query = db.CreateQuery("select [MapPointId], [MapId], [DataSourceTableName], [DataSourceXCoordinateColumnName], [DataSourceYCoordinateColumnName], [DataSourceLabelColumnName], [Size], [Color] from metaMapPoints where [MapId] = @MapId");
                query.Parameters.Add(new QueryParameter("@MapId", DbType.Int32, mapId));
                map.metaMapPoints.Merge(db.Select(query));
                return map.metaMapPoints;
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve map points", ex);
            }
        }

        /// <summary>
        /// Gets a GML map layer based on ID
        /// </summary>
        /// <param name="layerId">MapLayerId</param>
        /// <returns>GML</returns>
        public MapMetadata.metaLayersRow GetLayer(int layerId)
        {
            try
            {
                MapMetadata.metaLayersDataTable table = new MapMetadata.metaLayersDataTable();
                Query query = db.CreateQuery("select [LayerId], [Gml], [GmlSchema], [Name], [FileName] from metaLayers where [LayerId] = @LayerId");
                query.Parameters.Add(new QueryParameter("@LayerId", System.Data.DbType.String, layerId));
                table.Merge(db.Select(query));
                if (table.Rows.Count > 0)
                {
                    return table[0];
                }
                else
                {
                    throw new GeneralException("Layer not found");
                }
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve layer", ex);
            }
        }

        /// <summary>
        /// Adds a layer to a map using default parameters
        /// </summary>
        /// <param name="mapId">Map Id</param>
        /// <param name="layerId">Layer Id</param>
        public void AddLayerToMap(int mapId, int layerId)
        {
            try
            {
                Query query = db.CreateQuery("insert into metaMapLayers ([MapId], [LayerId], [LayerRenderTypeId]) values (@MapId, @LayerId, @LayerRenderTypeId)");
                query.Parameters.Add(new QueryParameter("@MapId", DbType.Int32, mapId));
                query.Parameters.Add(new QueryParameter("@LayerId", DbType.Int32, layerId));
                query.Parameters.Add(new QueryParameter("@LayerRenderTypeId", DbType.Int32, 1));
                db.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not add layer", ex);
            }
        }
        
        /// <summary>
        /// Gets all field types for grid columns
        /// </summary>
        /// <returns>Datatable containing field type info</returns>
        public DataTable GetGridFieldTypes()
        {
            try
            {
                Query query = db.CreateQuery("select F.[FieldTypeId], D.[DataTypeId], F.[Name], D.[HasPattern], D.[HasSize], F.[HasRepeatLast], F.[HasRequired], F.[HasReadOnly], F.[HasRetainImageSize], D.[HasRange], F.[HasFont], F.[IsGridColumn], F.[IsDropDown] " +
                    "from metaFieldTypes F LEFT OUTER JOIN metaDatatypes D on F.[DataTypeId] = D.[DataTypeId] " +
                    "where F.[IsGridColumn] = @IsGridColumn");
                query.Parameters.Add(new QueryParameter("@IsGridColumn", System.Data.DbType.Boolean, true));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve field types", ex);
            }
        }

        /// <summary>
        /// Gets all groups for a page
        /// </summary>
        /// <param name="pageId">Id of the page</param>
        /// <returns>Datatable containing group info</returns>
        public DataTable GetGroupsForPage(int pageId)
        {
            try
            {
                #region Input Validation
                if (pageId < 1)
                {
                    throw new ArgumentOutOfRangeException("PageId");
                }
                #endregion

                Query query = db.CreateQuery(Util.InsertIn("select * from", StringLiterals.SPACE) + db.InsertInEscape("metaFields") +
                    Util.InsertIn("where", StringLiterals.SPACE) + db.InsertInEscape("PageId") + StringLiterals.EQUAL + "@PageId" +
                    Util.InsertIn("and", StringLiterals.SPACE) + db.InsertInEscape("FieldTypeId") + StringLiterals.EQUAL + "21");

                query.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, pageId));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve group", ex);
            }
        }

        /// <summary>
        /// Gets the number of fields currently defined in metaFields for a given view.
        /// </summary>
        /// <param name="viewId"></param>
        /// <returns></returns>
        public int GetCollectedFieldCount(int viewId)
        {
            try
            {
                Query query = db.CreateQuery("SELECT COUNT(ViewId) AS numTimesUsed "
                    + "FROM metaFields "
                    + "WHERE ViewId = "
                    + viewId.ToString()
                    + " AND ("
                    + "FieldTypeId = 1 OR "
                    //exclude 2<Label>
                    + "(FieldTypeId > 2 AND FieldTypeId < 13) OR "
                    //exclude 13<CommandButton>
                    + "FieldTypeId = 14 OR "
                    //exclude 15<Mirror>
                    //exclude 16<Grid>
                    + "(FieldTypeId > 16 AND FieldTypeId < 20) OR "
                    //exclude 20<Relate>
                    //exclude 21<Group>
                    + "(FieldTypeId > 21 AND FieldTypeId < 27) "
                    + ")");

                DataTable dt = db.Select(query);
                int tableNameCount = Convert.ToInt32(dt.Rows[0]["numTimesUsed"]);
                return tableNameCount;
            }
            catch (Exception ex)
            {
                throw new GeneralException("There are no existing tables.", ex);
            }
        }

        /// <summary>
        /// Get all groups on a given page as a NamedObjectCollection
        /// </summary>
        /// <param name="page">The page the field group belongs to</param>
        /// <returns>FieldGroup</returns>
        public NamedObjectCollection<GroupField> GetGroupFields(Page page)
        {
            NamedObjectCollection<GroupField> groups = new NamedObjectCollection<GroupField>();
            try
            {
                DataTable table = GetGroupsForPage(page.Id);
                foreach (DataRow row in table.Rows)
                {
                    groups.Add(new GroupField(row, page));
                }
                
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve field groups", ex);
            }
            return (groups);
        }

        /// <summary>
        /// Returns the parent view of a related view
        /// </summary>
        /// <param name="viewId">The ID of the related view</param>
        /// <returns>The View object of the parent view</returns>
        public View GetParentView(int viewId)
        {
            View RetVal = null;
            Query query = null;

            query = db.CreateQuery("SELECT DISTINCT f.ViewId FROM metaViews v INNER JOIN metaFields f ON f.RelatedViewId = v.ViewId WHERE v.ViewId = @viewId");
            query.Parameters.Add(new QueryParameter("@viewId", DbType.Int32, viewId));
            DataTable results = db.Select(query);

            if (results.Rows.Count > 0)
            {
                RetVal = this.GetViewById((int)results.Rows[0][0]);
            }

            return RetVal;
        }

        /// <summary>
        /// Get Patterns in metaPatterns table.
        /// </summary>
        /// <returns>DataTable of pattern information from metaPatterns table.</returns>
        public DataTable GetPatterns()
        {
            try
            {
                WordBuilder columnNames = new WordBuilder(StringLiterals.COMMA);
                columnNames.Append("PatternId");
                columnNames.Append("DataTypeId");
                columnNames.Append("Expression");
                columnNames.Append("Mask");
                columnNames.Append("FormattedExpression");
                Query query = db.CreateQuery("select" + Util.InsertIn(columnNames.ToString(), StringLiterals.SPACE) +
                   Util.InsertIn( "from", StringLiterals.SPACE) + db.InsertInEscape("metaPatterns"));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve patterns", ex);
            }
        }

        /// <summary>
        /// Get TextFields for a page
        /// </summary>
        /// <param name="viewId">View Id</param>
        /// <param name="pageId">Page Id</param>
        /// <returns></returns>
        public DataTable GetTextFieldsForPage(int viewId, int pageId)
        {
            try
            {
                #region Input Validation
                if (viewId < 1)
                {
                    throw new ArgumentOutOfRangeException("ViewId");
                }
                if (pageId < 1)
                {
                    throw new ArgumentOutOfRangeException("PageId");
                }
                #endregion

                Query query = db.CreateQuery("select [Name], [FieldId] " +
                    "from metaFields " +
                    "where [PageId] = @pageID and [FieldTypeId] in (1,3,4) " +
                    " and [ViewId] = @viewID " +
                    "order by [Name]");
                query.Parameters.Add(new QueryParameter("@pageID", DbType.Int32, pageId));
                query.Parameters.Add(new QueryParameter("@viewID", DbType.Int32, viewId));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve text fields for a page.", ex);
            }
        }

        /// <summary>
        /// Get Code Target Candidates
        /// </summary>
        /// <param name="pageId">pageId</param>
        /// <returns></returns>
        public DataTable GetCodeTargetCandidates(int pageId)
        {
            Query query = db.CreateQuery("select * from metaFields " +
                "where [PageId] = @pageID and [FieldTypeId] in (1,2,3,4,17,18) " +
                "order by [Name]");
            query.Parameters.Add(new QueryParameter("@pageID", DbType.Int32, pageId));
            return db.Select(query);
        }

        /// <summary>
        /// Get the tab order for fields on a page
        /// </summary>
        /// <param name="pageId">The page Id</param>
        /// <returns>A data table containing tab order info</returns>
        public DataSets.TabOrders.TabOrderDataTable GetTabOrderForFields(int pageId)
        {
            try
            {
                #region Input Validation
                if (pageId < 1)
                {
                    throw new ArgumentOutOfRangeException("PageId");
                }
                #endregion

                Query query = db.CreateQuery("select [FieldId],[Name],[PromptText], [TabIndex],[HasTabStop] " +
                    "from metaFields " +
                    "where [PageId] = @pageID and [FieldTypeId] not in (2,15,21) " +
                    "order by [TabIndex]");
                query.Parameters.Add(new QueryParameter("@pageID", DbType.Int32, pageId));
                DataTable table = db.Select(query);
                DataSets.TabOrders tabOrders = new TabOrders();
                tabOrders.TabOrder.Merge(table);
                return tabOrders.TabOrder;
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve text fields for a page.", ex);
            }
        }
        
        /// <summary>
        /// Gets a nonview table as a <see cref="System.Data.DataTable"/>
        /// </summary>
        /// <returns></returns>
        public virtual DataTable GetNonViewTablesAsDataTable()
        {
            try
            {
                DataRow dataRow;
                DataTable tables = db.GetTableSchema(); //now GetTableSchema only gets user tables  /zack 1/30/08

                DataTable viewsAndTables = new DataTable("ViewsAndTables");
                viewsAndTables.Columns.Add(ColumnNames.NAME);
                DataRow[] rows = tables.Select(ColumnNames.TABLE_NAME + " not like 'meta%'");
                foreach (DataRow row in rows)
                {
                    string tableName = row[ColumnNames.TABLE_NAME].ToString();
                    dataRow = viewsAndTables.NewRow();
                    dataRow[ColumnNames.NAME] = tableName;
                    viewsAndTables.Rows.Add(dataRow);
                }
                return (viewsAndTables);

            }
            finally
            {
            }
        }

        /// <summary>
        /// Gets a view object based on view name.
        /// </summary>
        /// <param name="viewFullName">Name of the view.</param>
        /// <returns>A view object.</returns>
        public View GetViewByFullName(string viewFullName)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(viewFullName))
            {
                throw new ArgumentNullException("View Full Name");
            }
            #endregion
            try
            {
                string viewName = viewFullName.Substring(viewFullName.LastIndexOf(":") + 1);
                Query query = db.CreateQuery("select [ViewId], [Name], [CheckCode], [CheckCodeBefore], [CheckCodeAfter], [RecordCheckCodeBefore], [RecordCheckCodeAfter], [CheckCodeVariableDefinitions], [IsRelatedView], [Width], [Height], [Orientation], [LabelAlign] " +
                    Util.InsertIn("from", StringLiterals.SPACE) + db.InsertInEscape("metaViews") +
                    Util.InsertIn("where", StringLiterals.SPACE) + db.InsertInEscape("Name") + StringLiterals.EQUAL + "@ViewName");
                
                QueryParameter parameter = new QueryParameter("@ViewName", DbType.String, viewName);
                parameter.Size = viewName.Length;
                query.Parameters.Add(parameter);
                
                DataTable results = db.Select(query);
                if (results.Rows.Count > 0)
                {
                    return new View(results.Rows[0], Project);
                }
                else
                {
                    return null;
                }
            }
            finally
            {
            }
        }
        /// <summary>
        /// Gets all the programs saved in a project
        /// </summary>
        /// <returns>DataTable containing a list of all programs in a project</returns>
        public virtual DataTable GetPgms()
        {
            try
            {
                WordBuilder columnNames = new WordBuilder(StringLiterals.COMMA);
                columnNames.Append("ProgramId");
                columnNames.Append("Name");
                columnNames.Append("Content");
                columnNames.Append("Comment");
                columnNames.Append("DateCreated");
                columnNames.Append("DateModified");
                columnNames.Append("Author");
                Query query = db.CreateQuery("select" + Util.InsertIn(columnNames.ToString(), StringLiterals.SPACE) +
                   Util.InsertIn("from", StringLiterals.SPACE) + db.InsertInEscape("metaPrograms"));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve programs", ex);
            }
        }

        /// <summary>
        /// Returns names of all views based on this data table.
        /// </summary>
        /// <param name="dataTableName">Name of the data table.</param>
        /// <returns>List of names of all <see cref="Epi.View"/>s in the data table.</returns>
        public List<string> GetViewsOfDataTable(string dataTableName)
        {
            string queryString = "select distinct (v.Name) from metaFields F inner join metaViews V on F.ViewId = V.ViewId where F.DataTableName = @dataTableName";
            Query query = db.CreateQuery(queryString);
            query.Parameters.Add(new QueryParameter("@dataTableName", DbType.String, dataTableName));
            DataTable dt = db.Select(query);

            List<string> viewNames = new List<String>();
            foreach (DataRow dr in dt.Rows)
            {
                viewNames.Add(dr["Name"].ToString());
            }

            return viewNames;
        }

        /// <summary>
        /// Returns all data tables used by metadata.
        /// </summary>
        /// <returns>List of data table names.</returns>
        public List<string> GetDataTableList()
        {
            try
            {
                List<string> tables = new List<string>();
                //Query query = db.CreateQuery("select distinct [DataTableName] as Name, [ViewId] " +
                //    "from [metaFields] where [DataTableName] is not null");
              
                //DataTable table = db.Select(query);
                //foreach (DataRow row in table.Rows)
                //{
                //    tables.Add(row["Name"].ToString());
                //}
                tables = db.GetTableNames();
                return  tables;
            }
            catch (Exception ex)
            {
                throw new GeneralException("There are no existing tables.", ex);
            }
        }

        /// <summary>
        /// Returns the names of all data tables used by metadata for a given view.
        /// </summary>
        /// <param name="viewId"></param>
        /// <returns>a DataTable containing all the data table names.</returns>
        public DataTable GetDataTableNames(int viewId)
        {
            try
            {
                List<string> tables = new List<string>();
                Query query = db.CreateQuery("select distinct [DataTableName] as name " +
                    "from [metaFields] where [DataTableName] is not null");
                query.Parameters.Add(new QueryParameter("@viewId", DbType.Int32, viewId));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("There are no existing tables.", ex);
            }
        }

        /// <summary>
        /// Get the data table name of a view
        /// </summary>
        /// <param name="viewId">Id of the <see cref="Epi.View"/></param>
        /// <returns>The data table name of the view</returns>
        public string GetDataTableName(int viewId)
        {
            #region Input Validation
            if (viewId <= 0)
            {
                throw new ArgumentException(SharedStrings.INVALID_VIEW_ID + ": \n" + viewId.ToString());
            }
            #endregion Input Validation

            try
            {
                Query query = db.CreateQuery("select distinct [DataTableName] from [metaFields] where [ViewId] = @viewId and [DataTableName] is not null");
                query.Parameters.Add(new QueryParameter("@viewId", DbType.Int32, viewId));
                DataTable results = db.Select(query);
                if (results.Rows.Count > 0)
                {
                   return results.Rows[0][ColumnNames.DATA_TABLE_NAME].ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve data table name", ex);
            }
        }

        /// <summary>
        /// Gets table column names for a view.
        /// </summary>
        /// <param name="viewId">Id of the <see cref="Epi.View"/></param>
        /// <returns>string</returns>
        public string GetTableColumnNames(int viewId)
        {
            try
            {
                Query selectQuery = db.CreateQuery("select [Name] from [metaFields] " +
                    "where [ViewId] = @ViewID and " +
                    "[FieldTypeId] not in (2,13,15,20) " +
                    "order by [Name]");
                selectQuery.Parameters.Add(new QueryParameter("@ViewID", DbType.Int32, viewId));
                WordBuilder list = new WordBuilder(StringLiterals.COMMA);

                DataTable columns = db.Select(selectQuery);
                foreach (DataRow row in columns.Rows)
                {
                    list.Add(row[ColumnNames.NAME].ToString());
                }
                return list.ToString();
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve table column names", ex);
            }
        }

        #endregion Select Statement

        #region Insert Statements

        /// <summary>
        /// Creates a view in a specified project
        /// </summary>
        /// <old-param name="isrelatedview">Whether or not this view is a related (child) view</old-param>
        /// <old-param name="viewname">Name of the view</old-param>
        public void InsertView(View view)
        {
            #region Input Validation
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            #endregion Input Validation

            try
            {
                Query insertQuery = db.CreateQuery("insert into metaViews([Name], [IsRelatedView], [CheckCode], [Width], [Height], [Orientation], [LabelAlign] ) values (@Name, @IsRelatedView, @CheckCode, @Width, @Height, @Orientation, @LabelAlign)");
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, view.Name));
                insertQuery.Parameters.Add(new QueryParameter("@IsRelatedView", DbType.Boolean, view.IsRelatedView));
                insertQuery.Parameters.Add(new QueryParameter("@CheckCode", DbType.String, view.CheckCode));
                insertQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, view.PageWidth));
                insertQuery.Parameters.Add(new QueryParameter("@Height", DbType.Int32, view.PageHeight));
                insertQuery.Parameters.Add(new QueryParameter("@Orientation", DbType.String, view.PageOrientation));
                insertQuery.Parameters.Add(new QueryParameter("@LabelAlign", DbType.String, view.PageLabelAlign));
                db.ExecuteNonQuery(insertQuery);
                view.Id = this.GetMaxViewId();

                RecStatusField recStatusField = new RecStatusField(view);
                UniqueKeyField uniqueKeyField = new UniqueKeyField(view);
                GlobalRecordIdField globalRecordIdField = new GlobalRecordIdField(view);

                uniqueKeyField.SaveToDb();
                recStatusField.SaveToDb();
                globalRecordIdField.SaveToDb();
                if (view.IsRelatedView)
                {
                    ForeignKeyField foreignKeyField = new ForeignKeyField(view);
                    foreignKeyField.SaveToDb();
                }
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create view in the database", ex);
            }
        }

        /// <summary>
        /// Creates a page in a specified view
        /// </summary>
        /// <old-param name="pageposition">Position of the page</old-param>
        /// <old-param name="viewid">Id of the view</old-param>
        /// <old-param name="pagename">Name of the page</old-param>
        public void InsertPage(Page page)
        {
            try
            {
                #region Input Validation
                if (page == null)
                {
                    throw new ArgumentNullException("page");
                }
                #endregion Input validation

                Query query = db.CreateQuery("insert into metaPages([Name], [Position], [ViewId], [CheckCodeBefore], [CheckCodeAfter], [BackgroundId]) values (@Name, @Position, @ViewId, @CheckCodeBefore, @CheckCodeAfter, @BackgroundId)");
                query.Parameters.Add(new QueryParameter("@Name", DbType.String, page.Name));
                query.Parameters.Add(new QueryParameter("@Position", DbType.Int32, page.Position));
                query.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, page.GetView().Id));
                query.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, page.CheckCodeBefore));
                query.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, page.CheckCodeAfter));
                query.Parameters.Add(new QueryParameter("@BackgroundId", DbType.Int32, page.BackgroundId));

                db.ExecuteNonQuery(query);
                page.Id = GetMaxPageId(page.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create page in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Creates a field in metaFields
        /// </summary>
        /// <old-param name="pageposition">Position of the page</old-param>
        /// <old-param name="viewid">Id of the view</old-param>
        /// <old-param name="pagename">Name of the page</old-param>
        public void InsertFields(DataTable fields)
        {
            try
            {
                foreach (DataRow row in fields.Rows)
                {
                    Query insertQuery = db.CreateQuery("insert into metaFields([ViewId], [UniqueId], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [Name], [PageId], [PromptText], [List], [BackgroundColor], [TabIndex]) " +
                    "values (@ViewId, @UniqueId, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @FieldTypeId, @HasTabStop, @Name, @PageId, @PromptText, @List, @BackgroundColor, @TabIndex)");

                    insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, row["ControlFontFamily"]));
                    insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, row["ControlFontStyle"]));
                    insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, row["ControlFontSize"]));
                    insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, row["ControlHeightPercentage"]));
                    insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, row["ControlLeftPositionPercentage"]));
                    insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, row["ControlTopPositionPercentage"]));
                    insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, row["ControlWidthPercentage"]));
                    insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, row["FieldTypeId"]));
                    insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, row["HasTabStop"]));
                    insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, row["Name"]));
                    insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, row["PageId"]));
                    insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, row["PromptText"]));
                    insertQuery.Parameters.Add(new QueryParameter("@List", DbType.String, row["List"]));
                    insertQuery.Parameters.Add(new QueryParameter("@BackgroundColor", DbType.Int32, row["BackgroundColor"]));
                    insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, row["TabIndex"]));
                    insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, Guid.NewGuid()));

                    db.ExecuteNonQuery(insertQuery);
                }
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create page in the database", ex);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Insert a UniqueKeyField record into the metaFields table.
        /// </summary>
        /// <param name="field">Unique Key field.</param>
        /// <returns>Returns the Id of the last UniqueKeyField added.</returns>
        public int CreateField(UniqueKeyField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("UniqueKeyField");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([DataTableName], [ViewId], [FieldTypeId], [Name]) " +
                    "values (@DataTableName, @ViewId, @FieldTypeId, @Name)");

                insertQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, field.TableName));
                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));

                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            finally
            {
            }
        }

        //zack todo: implement
        /// <summary>
        /// Insert a UniqueIdentifierField record into the metaFields table.
        /// </summary>
        /// <param name="field">Unique Key field.</param>
        /// <returns>Returns the Id of the last UniqueIdentifierField added.</returns>
        public int CreateField(UniqueIdentifierField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("UniqueKeyField");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([DataTableName], [ViewId], [UniqueId], [FieldTypeId], [Name]) " +
                    "values (@DataTableName, @ViewId, @UniqueId, @FieldTypeId, @Name)");

                insertQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, field.TableName));
                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));

                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Insert a GlobalRecordIdField record into the metaFields table.
        /// </summary>
        /// <param name="field">GlobalRecordIdField</param>
        /// <returns>Returns the Id of the last GlobalRecordIdField added.</returns>
        public int CreateField(GlobalRecordIdField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("GlobalRecordIdField");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([DataTableName], [ViewId], [UniqueId], [FieldTypeId], [Name]) " +
                    "values (@DataTableName, @ViewId, @UniqueId, @FieldTypeId, @Name)");

                insertQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, field.TableName));
                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));

                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Insert a RecStatusField record into the metaFields table.
        /// </summary>
        /// <param name="field">RecStatus field.</param>
        /// <returns>Returns the Id of the last RecStatusField added.</returns>
        public int CreateField(RecStatusField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("RecStatusField");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([DataTableName], [ViewId], [FieldTypeId], [Name]) " +
                    "values (@DataTableName, @ViewId, @FieldTypeId, @Name)");

                insertQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, field.TableName));
                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));


                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create RecStatus field in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Insert a ForeignKeyField record into the metaFields table.
        /// </summary>
        /// <param name="field">Foreign Key field of a Unique Key.</param>
        /// <returns>Returns the Id of the last ForeignKeyField added.</returns>
        public int CreateField(ForeignKeyField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("ForeignKeyField");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([DataTableName], [ViewId], [UniqueId], [FieldTypeId], [Name]) " +
                    "values (@DataTableName, @ViewId, @UniqueId, @FieldTypeId, @Name)");

                insertQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, field.TableName));
                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));

                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create ForeignKey field in the database", ex);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Insert a CheckBoxField record into the metaFields table.
        /// </summary>
        /// <param name="field">Checkbox field.</param>
        /// <returns>Returns the Id of the last CheckBoxField added.</returns>
        public int CreateField(CheckBoxField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("CheckBoxField");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([DataTableName], [ViewId], [UniqueId], [CheckCodeAfter], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [IsReadOnly], [IsRequired], [Name], [PageId], [Pattern], [PromptFontFamily], [PromptFontStyle], [PromptFontSize], [PromptText], [ShouldRepeatLast], [TabIndex]) " +
                    "values (@DataTableName, @ViewId, @UniqueId, @CheckCodeAfter, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @FieldTypeId, @HasTabStop, @IsReadOnly, @IsRequired, @Name, @PageId, @Pattern, @PromptFontFamily, @PromptFontStyle, @PromptFontSize, @PromptText, @ShouldRepeatLast, @TabIndex)");

                insertQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, field.TableName));
                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));
                if (field.CheckCodeAfter == null)
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                insertQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, (field.BoxOnRight ? "BoxOnRight" : "")));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));


                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create CheckBox field in the database", ex);
            }
            finally
            {

            }
        }
        #region OBSOLETE
        /*
		/// <summary>
		/// Creates a code field in the database
		/// </summary>
		/// <param name="field">A CodeField object</param>
		/// <returns>Id of the newly created field</returns>
        public int CreateField(CodeField field)
		{
			try
			{
		#region InputValidation
				if (field == null)
				{
					throw new ArgumentNullException("CodeField");
				}
		#endregion

				DbQuery insertQuery = db.CreateQuery("insert into metaFields([ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeight], [ControlLeftPosition], [ControlTopPosition], [ControlWidth], [HasTabStop], [Name], [PageId], [PromptFontFamily], [PromptFontStyle], [PromptFontSize], [PromptLeftPosition], [PromptText], [PromptTopPosition], [TabIndex]) " +
					"values (@ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeight, @ControlLeftPosition, @ControlTopPosition, @ControlWidth, @HasTabStop, @Name, @PageId, @PromptFontFamily, @PromptFontStyle, @PromptFontSize, @PromptLeftPosition, @PromptText, @PromptTopPosition, @TabIndex)");

				insertQuery.Parameters.Add(new DbParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
				insertQuery.Parameters.Add(new DbParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
				insertQuery.Parameters.Add(new DbParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));	
				insertQuery.Parameters.Add(new DbParameter("@ControlHeight", DbType.Double, field.ControlHeightPercentage));
				insertQuery.Parameters.Add(new DbParameter("@ControlLeftPosition", DbType.Double, field.ControlLeftPositionPercentage));
				insertQuery.Parameters.Add(new DbParameter("@ControlTopPosition", DbType.Double, field.ControlTopPositionPercentage));
				insertQuery.Parameters.Add(new DbParameter("@ControlWidth", DbType.Double, field.ControlWidthPercentage));
				insertQuery.Parameters.Add(new DbParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
				insertQuery.Parameters.Add(new DbParameter("@Name", DbType.String, field.Name));
				insertQuery.Parameters.Add(new DbParameter("@PageId", DbType.Int32, field.Page.Id));
				insertQuery.Parameters.Add(new DbParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
				insertQuery.Parameters.Add(new DbParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
				insertQuery.Parameters.Add(new DbParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
				insertQuery.Parameters.Add(new DbParameter("@PromptLeftPosition", DbType.Double, field.PromptLeftPositionPercentage));
				insertQuery.Parameters.Add(new DbParameter("@PromptText", DbType.String, field.PromptText));
				insertQuery.Parameters.Add(new DbParameter("@PromptTopPosition", DbType.Double, field.PromptTopPositionPercentage));
				insertQuery.Parameters.Add(new DbParameter("@TabIndex", DbType.Int32, field.TabIndex));

				DbQuery selectQuery = db.CreateQuery("select MAX(FieldId) from metaFields where PageId = @PageId");
				selectQuery.Parameters.Add(new DbParameter("@PageId", DbType.Int32, field.Page.Id));

				
				db.ExecuteNonQuery(insertQuery);
				

				DataTable result = db.Select(selectQuery);
				return (int)result.Rows[0][0];
			}
//			catch (Exception ex)
//			{
//				throw new GeneralException("Could not create field in the database", ex);
//			}
			finally
			{
				
			}
		}
				public int CreateField(Field field)
				{
					if (field is CommandButtonField)
					{
						return CreateField((CommandButtonField)field);
					}
					else if (field is DateField)
					{
						return CreateField((DateField)field);
					}
					else if (field is DateTimeField)
					{
						return CreateField((DateTimeField)field);
					}
					else if (field is DDLFieldOfCodes)
					{
						return CreateField((DDLFieldOfCodes)field);
					}
					else if (field is DDLFieldOfCommentLegal)
					{
						return CreateField((DDLFieldOfCommentLegal)field);
					}
					else if (field is DDLFieldOfLegalValues)
					{
						return CreateField((DDLFieldOfLegalValues)field);
					}
					else if (field is GridField)
					{
						return CreateField((GridField)field);
					}
					else if (field is ImageField)
					{
						return CreateField((ImageField)field);
					}
					else if (field is LabelField)
					{
						return CreateField((LabelField)field);
					}
					else if (field is MirrorField)
					{
						return CreateField((MirrorField)field);
					}
					else if (field is MultilineTextField)
					{
						return CreateField((MultilineTextField)field);
					}
					else if (field is NumberField)
					{
						return CreateField((NumberField)field);
					}
					else if (field is OptionField)
					{
						return CreateField((OptionField)field);
					}
					else if (field is PhoneNumberField)
					{
						return CreateField((PhoneNumberField)field);
					}
					else if (field is RelatedViewField)
					{
						return CreateField((RelatedViewField)field);
					}
					else if (field is SingleLineTextField)
					{
						return CreateField((SingleLineTextField)field);
					}
					else if (field is TimeField)
					{
						return CreateField((TimeField)field);
					}
					else if (field is UpperCaseTextField)
					{
						return CreateField((UpperCaseTextField)field);
					}
					else if (field is YesNoField)
					{
						return CreateField((YesNoField)field);
					}
					else
						return 0;
				}
		*/

        #endregion

        /// <summary>
        /// Insert a CommandButtonField record into the metaFields table.
        /// </summary>
        /// <param name="field">Command button field.</param>
        /// <returns>Returns the Id of the last CommandButtonField added.</returns>
        public int CreateField(CommandButtonField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("CommandButtonField");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([ViewId], [UniqueId], [CheckCodeAfter], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [Name], [PageId], [PromptText], [TabIndex]) " +
                    "values (@ViewId, @UniqueId, @CheckCodeAfter, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @FieldTypeId, @HasTabStop, @Name, @PageId, @PromptText, @TabIndex)");

                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));
                if (string.IsNullOrEmpty(field.CheckCodeClick))
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeClick));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));


                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create CommandButton field in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Insert a DateField record into the metaFields table.
        /// </summary>
        /// <param name="field">Date text field.</param>
        /// <returns>Returns the Id of the last DateField added.</returns>
        public int CreateField(DateField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("DateField");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([DataTableName], [ViewId], [UniqueId], [CheckCodeAfter], [CheckCodeBefore], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [IsReadOnly], [IsRequired], [Lower], [Name], [PageId], [Pattern], [PromptFontFamily], [PromptFontStyle], [PromptFontSize], [PromptLeftPositionPercentage], [PromptText], [PromptTopPositionPercentage], [ShouldRepeatLast], [TabIndex], [Upper]) " +
                    "values (@DataTableName, @ViewId, @UniqueId, @CheckCodeAfter, @CheckCodeBefore, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @FieldTypeId, @HasTabStop, @IsReadOnly, @IsRequired, @Lower, @Name, @PageId, @Pattern, @PromptFontFamily, @PromptFontStyle, @PromptFontSize, @PromptLeftPositionPercentage, @PromptText, @PromptTopPositionPercentage, @ShouldRepeatLast, @TabIndex, @Upper)");

                insertQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, field.TableName));
                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));
                if (field.CheckCodeAfter == null)
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));

                if (field.CheckCodeBefore == null)
                    insertQuery.Parameters.Add(new QueryParameter(ColumnNames.CHECK_CODE_BEFORE, DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeBefore));

                insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                if (field.Lower == null)
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Lower", DbType.String, DBNull.Value));
                }
                else
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Lower", DbType.String, field.Lower));
                }
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                if (field.Pattern == null)
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, DBNull.Value));
                }
                else
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, field.Pattern));
                }
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                insertQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                if (field.Upper == null)
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Upper", DbType.String, DBNull.Value));
                }
                else
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Upper", DbType.String, field.Upper));
                }


                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create field in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Insert a DateTimeField record into the metaFields table.
        /// </summary>
        /// <param name="field">Date and time text field.</param>
        /// <returns>Returns the Id of the last DateTimeField added.</returns>
        public int CreateField(DateTimeField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("DateTimeField");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([DataTableName], [ViewId], [UniqueId], [CheckCodeAfter], [CheckCodeBefore], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [IsReadOnly], [IsRequired], [Name], [PageId], [PromptFontFamily], [PromptFontStyle], [PromptFontSize], [PromptLeftPositionPercentage], [PromptText], [PromptTopPositionPercentage], [ShouldRepeatLast], [TabIndex]) " +
                    "values (@DataTableName, @ViewId, @UniqueId, @CheckCodeAfter, @CheckCodeBefore, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @FieldTypeId, @HasTabStop, @IsReadOnly, @IsRequired, @Name, @PageId,  @PromptFontFamily, @PromptFontStyle, @PromptFontSize, @PromptLeftPositionPercentage, @PromptText, @PromptTopPositionPercentage, @ShouldRepeatLast, @TabIndex)");

                insertQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, field.TableName));
                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));
                if (field.CheckCodeAfter == null)
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));

                if (field.CheckCodeBefore == null)
                    insertQuery.Parameters.Add(new QueryParameter(ColumnNames.CHECK_CODE_BEFORE, DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeBefore));

                insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                insertQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));


                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create field in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Insert a DDLFieldOfCodes record into the metaFields table.
        /// </summary>
        /// <param name="field">Dropdown list field of codes.</param>
        /// <returns>Returns the Id of the last DDLFieldOfCodes added.</returns>
        public int CreateField(DDLFieldOfCodes field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("DDLFieldOfCodes");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields(" +
                    " [DataTableName], [ViewId], [UniqueId], [CodeColumnName], [TextColumnName], [SourceTableName], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [IsExclusiveTable], [IsReadOnly], [IsRequired], [Name], [PageId], [PromptFontFamily], [PromptFontStyle], [PromptFontSize], [PromptLeftPositionPercentage], [PromptText], [PromptTopPositionPercentage], [ShouldRepeatLast], [TabIndex], [Sort], [CheckCodeBefore], [CheckCodeAfter], [RelateCondition])" +
                    "values ( @DataTableName,  @ViewId, @UniqueId,  @CodeColumnName,  @TextColumnName,  @SourceTableName,  @ControlFontFamily,  @ControlFontStyle,  @ControlFontSize,  @ControlHeightPercentage,  @ControlLeftPositionPercentage,  @ControlTopPositionPercentage,  @ControlWidthPercentage,  @FieldTypeId,  @HasTabStop,  @IsExclusiveTable,  @IsReadOnly,  @IsRequired,  @Name,  @PageId,  @PromptFontFamily,  @PromptFontStyle,   @PromptFontSize,   @PromptLeftPositionPercentage,  @PromptText,  @PromptTopPositionPercentage,  @ShouldRepeatLast,  @TabIndex, @Sort, @CheckCodeBefore, @CheckCodeAfter, @RelateCondition) ");
                // , @SourceFieldId
                //)");

                insertQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, field.TableName));
                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));
                insertQuery.Parameters.Add(new QueryParameter("@CodeColumnName", DbType.String, field.CodeColumnName));
                insertQuery.Parameters.Add(new QueryParameter("@TextColumnName", DbType.String, field.TextColumnName));
                insertQuery.Parameters.Add(new QueryParameter("@SourceTableName", DbType.String, field.SourceTableName));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                insertQuery.Parameters.Add(new QueryParameter("@IsExclusiveTable", DbType.Boolean, field.IsExclusiveTable));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                insertQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                insertQuery.Parameters.Add(new QueryParameter("@Sort", DbType.Boolean, field.ShouldSort));
                insertQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeBefore));
                insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));
                insertQuery.Parameters.Add(new QueryParameter("@RelateCondition", DbType.String, field.RelateConditionString));

                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create field in the database", ex);
            }
            finally
            {

            }
        }


        /// <summary>
        /// Insert a DDLFieldOfCodes record into the metaFields table.
        /// </summary>
        /// <param name="field">Dropdown list field of codes.</param>
        /// <returns>Returns the Id of the last DDLFieldOfCodes added.</returns>
        public int CreateField(DDListField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("DDLFieldOfCodes");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields(" +
                    " [DataTableName], [ViewId], [UniqueId], [CodeColumnName], [TextColumnName], [SourceTableName], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [IsExclusiveTable], [IsReadOnly], [IsRequired], [Name], [PageId], [PromptFontFamily], [PromptFontStyle], [PromptFontSize], [PromptLeftPositionPercentage], [PromptText], [PromptTopPositionPercentage], [ShouldRepeatLast], [TabIndex], [Sort], [CheckCodeBefore], [CheckCodeAfter], [RelateCondition])" +
                    "values ( @DataTableName,  @ViewId, @UniqueId,  @CodeColumnName,  @TextColumnName,  @SourceTableName,  @ControlFontFamily,  @ControlFontStyle,  @ControlFontSize,  @ControlHeightPercentage,  @ControlLeftPositionPercentage,  @ControlTopPositionPercentage,  @ControlWidthPercentage,  @FieldTypeId,  @HasTabStop,  @IsExclusiveTable,  @IsReadOnly,  @IsRequired,  @Name,  @PageId,  @PromptFontFamily,  @PromptFontStyle,   @PromptFontSize,   @PromptLeftPositionPercentage,  @PromptText,  @PromptTopPositionPercentage,  @ShouldRepeatLast,  @TabIndex, @Sort, @CheckCodeBefore, @CheckCodeAfter, @RelateCondition) ");
                // , @SourceFieldId
                //)");

                insertQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, field.TableName));
                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));
                insertQuery.Parameters.Add(new QueryParameter("@CodeColumnName", DbType.String, field.CodeColumnName));
                insertQuery.Parameters.Add(new QueryParameter("@TextColumnName", DbType.String, field.TextColumnName));
                insertQuery.Parameters.Add(new QueryParameter("@SourceTableName", DbType.String, field.SourceTableName));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                insertQuery.Parameters.Add(new QueryParameter("@IsExclusiveTable", DbType.Boolean, field.IsExclusiveTable));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                insertQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                insertQuery.Parameters.Add(new QueryParameter("@Sort", DbType.Boolean, field.ShouldSort));
                insertQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeBefore));
                insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));
                insertQuery.Parameters.Add(new QueryParameter("@RelateCondition", DbType.String, field.AssociatedFieldInformation));

                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create field in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Insert a DDLFieldOfCommentLegal record into the metaFields table.
        /// </summary>
        /// <param name="field">Dropdown list field of legal comments.</param>
        /// <returns>Returns the Id of the last DDLFieldOfCommentLegal added.</returns>
        public int CreateField(DDLFieldOfCommentLegal field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("DDLFieldOfCommentLegal");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([DataTableName], [ViewId], [UniqueId], [TextColumnName], [SourceTableName], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [IsExclusiveTable], [IsReadOnly], [IsRequired], [Name], [PageId], [PromptFontFamily], [PromptFontStyle], [PromptFontSize], [PromptLeftPositionPercentage], [PromptText], [PromptTopPositionPercentage], [ShouldRepeatLast], [TabIndex], [Sort], [CheckCodeBefore], [CheckCodeAfter]) " +
                    "values (@DataTableName, @ViewId, @UniqueId, @TextColumnName, @SourceTableName, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @FieldTypeId, @HasTabStop, @IsExclusiveTable, @IsReadOnly, @IsRequired, @Name, @PageId, @PromptFontFamily, @PromptFontStyle, @PromptFontSize, @PromptLeftPositionPercentage, @PromptText, @PromptTopPositionPercentage, @ShouldRepeatLast, @TabIndex, @Sort, @CheckCodeBefore, @CheckCodeAfter)");

                insertQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, field.TableName));
                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId)); 
                insertQuery.Parameters.Add(new QueryParameter("@TextColumnName", DbType.String, field.TextColumnName));
                insertQuery.Parameters.Add(new QueryParameter("@SourceTableName", DbType.String, field.SourceTableName));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                insertQuery.Parameters.Add(new QueryParameter("@IsExclusiveTable", DbType.Boolean, field.IsExclusiveTable));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                insertQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                insertQuery.Parameters.Add(new QueryParameter("@Sort", DbType.Boolean, field.ShouldSort));
                insertQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeBefore));
                insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));


                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create field in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Insert a DDLFieldOfLegalValues record into the metaFields table.
        /// </summary>
        /// <param name="field">Dropdown list field of legal values.</param>
        /// <returns>Returns the Id of the last DDLFieldOfLegalValues added.</returns>
        public int CreateField(DDLFieldOfLegalValues field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("DDLFieldOfLegalValues");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([DataTableName], [ViewId], [UniqueId], [CodeColumnName], [TextColumnName], [SourceTableName], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [IsExclusiveTable], [IsReadOnly], [IsRequired], [Name], [PageId], [PromptFontFamily], [PromptFontStyle], [PromptFontSize], [PromptLeftPositionPercentage], [PromptText], [PromptTopPositionPercentage], [ShouldRepeatLast], [TabIndex], [Sort], [CheckCodeBefore], [CheckCodeAfter]) " +
                    "values (@DataTableName, @ViewId, @UniqueId, @CodeColumnName, @TextColumnName, @SourceTableName, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @FieldTypeId, @HasTabStop, @IsExclusiveTable, @IsReadOnly, @IsRequired, @Name, @PageId, @PromptFontFamily, @PromptFontStyle, @PromptFontSize, @PromptLeftPositionPercentage, @PromptText, @PromptTopPositionPercentage, @ShouldRepeatLast, @TabIndex, @Sort, @CheckCodeBefore, @CheckCodeAfter)");

                insertQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, field.TableName));
                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId)); 
                insertQuery.Parameters.Add(new QueryParameter("@CodeColumnName", DbType.String, field.CodeColumnName));
                insertQuery.Parameters.Add(new QueryParameter("@TextColumnName", DbType.String, field.TextColumnName));
                insertQuery.Parameters.Add(new QueryParameter("@SourceTableName", DbType.String, field.SourceTableName));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                insertQuery.Parameters.Add(new QueryParameter("@IsExclusiveTable", DbType.Boolean, field.IsExclusiveTable));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                insertQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                insertQuery.Parameters.Add(new QueryParameter("@Sort", DbType.Boolean, field.ShouldSort));
                insertQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeBefore));
                insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));


                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create field in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Insert a GridField record into the metaFields table.
        /// </summary>
        /// <param name="field">Grid field.</param>
        /// <returns>Returns the Id of the last GridField added.</returns>
        public int CreateField(GridField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("GridField");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([ViewId], [UniqueId], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [Name], [PageId], [PromptFontFamily], [PromptFontStyle], [PromptFontSize], [PromptText],  [PromptLeftPositionPercentage], [PromptTopPositionPercentage], [TabIndex]) " +
                    "values (@ViewId, @UniqueId, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @FieldTypeId, @HasTabStop, @Name, @PageId, @PromptFontFamily, @PromptFontStyle, @PromptFontSize, @PromptText,@PromptLeftPositionPercentage, @PromptTopPositionPercentage, @TabIndex)");

                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId)); 
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                insertQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));


                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create field in the database", ex);
            }
            finally
            {

            }
        }
        /// <summary>
        /// Insert a GUIDField record into the metaFields table.
        /// </summary>
        /// <param name="field">GUID field.</param>
        /// <returns>Returns the Id of the last GUID added.</returns>
        public int CreateField(GUIDField field)
        {
            try
            {
                #region InputValidation

                if (field == null)
                {
                    throw new ArgumentNullException("GUIDField");
                }

                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([DataTableName], [ViewId], [UniqueId], [CheckCodeAfter], [CheckCodeBefore], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [IsReadOnly], [IsRequired], [MaxLength], [Name], [PageId], [PromptFontFamily], [PromptFontStyle], [PromptFontSize], [PromptLeftPositionPercentage], [PromptText], [PromptTopPositionPercentage], [ShouldRepeatLast], [TabIndex]) " +
                    "values (@DataTableName, @ViewId, @UniqueId, @CheckCodeAfter, @CheckCodeBefore, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @FieldTypeId, @HasTabStop, @IsReadOnly, @IsRequired, @MaxLength, @Name, @PageId, @PromptFontFamily, @PromptFontStyle, @PromptFontSize, @PromptLeftPositionPercentage, @PromptText, @PromptTopPositionPercentage, @ShouldRepeatLast, @TabIndex)");

                insertQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, field.TableName));
                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));
                if (field.CheckCodeAfter == null)
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));

                if (field.CheckCodeBefore == null)
                    insertQuery.Parameters.Add(new QueryParameter(ColumnNames.CHECK_CODE_BEFORE, DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeBefore));

                insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@MaxLength", DbType.Int16, field.MaxLength));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                insertQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));


                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create field in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Insert a ImageField record into the metaFields table.
        /// </summary>
        /// <param name="field">Image field.</param>
        /// <returns>Returns the Id of the last ImageField added.</returns>
        public int CreateField(ImageField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("ImageField");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([DataTableName], [ViewId], [UniqueId], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [IsReadOnly], [IsRequired], [Name], [PageId], [PromptFontFamily], [PromptFontStyle], [PromptFontSize], [PromptLeftPositionPercentage], [PromptText], [PromptTopPositionPercentage], [ShouldRepeatLast], [ShouldRetainImageSize], [TabIndex]) " +
                    "values (@DataTableName, @ViewId, @UniqueId, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @FieldTypeId, @HasTabStop, @IsReadOnly, @IsRequired, @Name, @PageId, @PromptFontFamily, @PromptFontStyle, @PromptFontSize, @PromptLeftPositionPercentage, @PromptText, @PromptTopPositionPercentage, @ShouldRepeatLast, @ShouldRetainImageSize , @TabIndex)");

                insertQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, field.TableName));
                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                insertQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRetainImageSize", DbType.Boolean, field.ShouldRetainImageSize));
                insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));


                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create field in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Insert a LabelField record into the metaFields table.
        /// </summary>
        /// <param name="field">Label field.</param>
        /// <returns>Returns the Id of the last SingleLineTextField added.</returns>
        public int CreateField(LabelField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("LabelField");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([ViewId], [UniqueId], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [Name], [PageId], [PromptText], [TabIndex]) " +
                    "values (@ViewId, @UniqueId, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @FieldTypeId, @HasTabStop, @Name, @PageId, @PromptText, @TabIndex)");

                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));


                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create field in the database", ex);
            }
            finally
            {

            }
        }
        /// <summary>
        /// Insert a GroupField record into the metaFields table.
        /// </summary>
        /// <param name="field">Group Filed</param>
        public int CreateField(GroupField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("GroupField");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([ViewId], [UniqueId], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [Name], [PageId], [PromptText], [List], [BackgroundColor], [TabIndex]) " +
                    "values (@ViewId, @UniqueId, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @FieldTypeId, @HasTabStop, @Name, @PageId, @PromptText, @List, @BackgroundColor, @TabIndex)");

                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                insertQuery.Parameters.Add(new QueryParameter("@List", DbType.String, field.ChildFieldNames));
                insertQuery.Parameters.Add(new QueryParameter("@BackgroundColor", DbType.Int32, field.BackgroundColor.ToArgb()));
                insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));

                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create field in the database", ex);
            }
        }

        /// <summary>
        /// Insert a MirrorField record into the metaFields table.
        /// </summary>
        /// <param name="field">Mirror field.</param>
        /// <returns>Returns the Id of the last MirrorField added.</returns>
        public int CreateField(MirrorField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("MirrorField");
                }
                #endregion

                string queryString;
                if (field.SourceFieldId != 0)
                {
                    queryString = "insert into metaFields([ViewId], [UniqueId], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [Name], [PageId], [PromptFontFamily], [PromptFontStyle], [PromptFontSize], [PromptLeftPositionPercentage], [PromptText], [PromptTopPositionPercentage], [TabIndex], [SourceFieldId]) " +
                    "values (@ViewId, @UniqueId, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @FieldTypeId, @HasTabStop, @Name, @PageId, @PromptFontFamily, @PromptFontStyle, @PromptFontSize, @PromptLeftPositionPercentage, @PromptText, @PromptTopPositionPercentage, @TabIndex, @SourceFieldId)";
                }
                else
                {
                    queryString = "insert into metaFields([ViewId], [UniqueId], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [Name], [PageId], [PromptFontFamily], [PromptFontStyle], [PromptFontSize], [PromptLeftPositionPercentage], [PromptText], [PromptTopPositionPercentage], [TabIndex]) " +
                    "values (@ViewId, @UniqueId, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @FieldTypeId, @HasTabStop, @Name, @PageId, @PromptFontFamily, @PromptFontStyle, @PromptFontSize, @PromptLeftPositionPercentage, @PromptText, @PromptTopPositionPercentage, @TabIndex)";
                }

                Query insertQuery = db.CreateQuery(queryString);

                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                insertQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                if (field.SourceFieldId != 0)
                {
                    insertQuery.Parameters.Add(new QueryParameter("@SourceFieldId", DbType.Int32, field.SourceFieldId));
                }

                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create field in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Insert a MultiLineTextField record into the metaFields table.
        /// </summary>
        /// <param name="field">Multiline text field.</param>
        /// <returns>Returns the Id of the last MultiLineTextField added.</returns>
        public int CreateField(MultilineTextField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("MultilineTextField");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([DataTableName], [ViewId], [UniqueId], [CheckCodeAfter], [CheckCodeBefore], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [IsReadOnly], [IsRequired], [MaxLength], [Name], [PageId], [PromptFontFamily], [PromptFontStyle], [PromptFontSize], [PromptLeftPositionPercentage], [PromptText], [PromptTopPositionPercentage], [ShouldRepeatLast], [TabIndex]) " +
                    "values (@DataTableName, @ViewId, @UniqueId, @CheckCodeAfter, @CheckCodeBefore, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @FieldTypeId, @HasTabStop, @IsReadOnly, @IsRequired, @MaxLength, @Name, @PageId, @PromptFontFamily, @PromptFontStyle, @PromptFontSize, @PromptLeftPositionPercentage, @PromptText, @PromptTopPositionPercentage, @ShouldRepeatLast, @TabIndex)");

                insertQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, field.TableName));
                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));
                if (field.CheckCodeAfter == null)
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));

                if (field.CheckCodeBefore == null)
                    insertQuery.Parameters.Add(new QueryParameter(ColumnNames.CHECK_CODE_BEFORE, DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeBefore));

                insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@MaxLength", DbType.Int16, field.MaxLength));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                insertQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));


                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create field in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Insert a NumberField record into the metaFields table.
        /// </summary>
        /// <param name="field">Number field.</param>
        /// <returns>Returns the Id of the last NumberField added.</returns>
        public int CreateField(NumberField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("NumberField");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([DataTableName], [ViewId], [UniqueId], [CheckCodeAfter], [CheckCodeBefore], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [IsReadOnly], [IsRequired], [Lower], [Name], [PageId], [Pattern], [PromptFontFamily], [PromptFontStyle], [PromptFontSize], [PromptLeftPositionPercentage], [PromptText], [PromptTopPositionPercentage], [ShouldRepeatLast], [TabIndex], [Upper]) " +
                    "values (@DataTableName, @ViewId, @UniqueId, @CheckCodeAfter, @CheckCodeBefore, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @FieldTypeId, @HasTabStop, @IsReadOnly, @IsRequired, @Lower, @Name, @PageId, @Pattern, @PromptFontFamily, @PromptFontStyle, @PromptFontSize, @PromptLeftPositionPercentage, @PromptText, @PromptTopPositionPercentage, @ShouldRepeatLast, @TabIndex, @Upper)");

                insertQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, field.TableName));
                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));
                if (field.CheckCodeAfter == null)
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));

                if (field.CheckCodeBefore == null)
                    insertQuery.Parameters.Add(new QueryParameter(ColumnNames.CHECK_CODE_BEFORE, DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeBefore));

                insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                if (field.Lower == null)
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Lower", DbType.String, DBNull.Value));
                }
                else
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Lower", DbType.String, field.Lower));
                }
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                if (field.Pattern == null)
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, DBNull.Value));
                }
                else
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, field.Pattern));
                }
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                insertQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                if (field.Upper == null)
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Upper", DbType.String, DBNull.Value));
                }
                else
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Upper", DbType.String, field.Upper));
                }

                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create field in the database", ex);
            }
        }

        /// <summary>
        /// Insert a OptionField record into the metaFields table.
        /// </summary>
        /// <param name="field">Option button field.</param>
        /// <returns>Returns the Id of the last OptionField added.</returns>
        public int CreateField(OptionField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("OptionField");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([DataTableName], [ViewId], [UniqueId], [CheckCodeAfter], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [IsReadOnly], [IsRequired], [Name], [PageId], [PromptFontFamily], [PromptFontStyle], [PromptFontSize], [PromptText], [List], [ShouldRepeatLast], [ShowTextOnRight], [TabIndex], [Pattern]) " +
                    "values (@DataTableName, @ViewId, @UniqueId, @CheckCodeAfter, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @FieldTypeId, @HasTabStop, @IsReadOnly, @IsRequired, @Name, @PageId, @PromptFontFamily, @PromptFontStyle, @PromptFontSize, @PromptText, @List, @ShouldRepeatLast, @ShowTextOnRight, @TabIndex, @Pattern)");

                insertQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, field.TableName));
                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));
                if (field.CheckCodeAfter == null)
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));

                insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                insertQuery.Parameters.Add(new QueryParameter("@List", DbType.String, field.GetOptionsString()));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@ShowTextOnRight", DbType.Boolean, field.ShowTextOnRight));
                insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                insertQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, field.Pattern));
                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create field in the database", ex);
            }
        }

        /// <summary>
        /// Insert a PhoneNumberField record into the metaFields table.
        /// </summary>
        /// <param name="field">Phone Number field.</param>
        /// <returns>Returns the Id of the last PhoneNumberField added.</returns>
        public int CreateField(PhoneNumberField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("PhoneNumberField");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([DataTableName], [ViewId], [UniqueId], [CheckCodeAfter], [CheckCodeBefore], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [IsReadOnly], [IsRequired], [Name], [PageId], [Pattern], [PromptFontFamily], [PromptFontStyle], [PromptFontSize], [PromptLeftPositionPercentage], [PromptText], [PromptTopPositionPercentage], [ShouldRepeatLast], [TabIndex]) " +
                    "values (@DataTableName, @ViewId, @UniqueId, @CheckCodeAfter, @CheckCodeBefore, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @FieldTypeId, @HasTabStop, @IsReadOnly, @IsRequired, @Name, @PageId, @Pattern, @PromptFontFamily, @PromptFontStyle, @PromptFontSize, @PromptLeftPositionPercentage, @PromptText, @PromptTopPositionPercentage, @ShouldRepeatLast, @TabIndex)");

                insertQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, field.TableName));
                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));
                if (field.CheckCodeAfter == null)
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));

                if (field.CheckCodeBefore == null)
                    insertQuery.Parameters.Add(new QueryParameter(ColumnNames.CHECK_CODE_BEFORE, DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeBefore));

                insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                if (field.Pattern == null)
                    insertQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, field.Pattern));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                insertQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));

                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create field in the database", ex);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Insert a RelatedViewField record into the metaFields table.
        /// </summary>
        /// <param name="field">Related view field.</param>
        /// <returns>Returns the Id of the last RelatedViewField added.</returns>
        public int CreateField(RelatedViewField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("RelatedViewField");
                }
                #endregion

                Boolean relatedViewIdExists = false;

                if (this.GetViewById(field.RelatedViewID) != null)
                {
                    relatedViewIdExists = true;
                }

                Query insertQuery = null;

                if (relatedViewIdExists)
                {
                    insertQuery = db.CreateQuery("insert into metaFields([ViewId], [UniqueId], [RelateCondition], [CheckCodeAfter], [CheckCodeBefore], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [Name], [PageId], [PromptText], [ShouldReturnToParent], [TabIndex], [RelatedViewId]) " +
                        "values (@ViewId, @UniqueId, @RelateCondition,@CheckCodeAfter, @CheckCodeBefore, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @FieldTypeId, @HasTabStop, @Name, @PageId, @PromptText, @ShouldReturnToParent, @TabIndex, @RelatedViewId)");
                }
                else
                {
                    insertQuery = db.CreateQuery("insert into metaFields([ViewId], [UniqueId], [RelateCondition], [CheckCodeAfter], [CheckCodeBefore], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [Name], [PageId], [PromptText], [ShouldReturnToParent], [TabIndex]) " +
                        "values (@ViewId, @UniqueId, @RelateCondition,@CheckCodeAfter, @CheckCodeBefore, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @FieldTypeId, @HasTabStop, @Name, @PageId, @PromptText, @ShouldReturnToParent, @TabIndex)");
                }

                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));
                if (field.Condition == null)
                    insertQuery.Parameters.Add(new QueryParameter("@RelateCondition", DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@RelateCondition", DbType.String, field.Condition));

                if (field.CheckCodeAfter == null)
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));

                if (field.CheckCodeClick == null)
                    insertQuery.Parameters.Add(new QueryParameter(ColumnNames.CHECK_CODE_BEFORE, DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeClick));

                insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldReturnToParent", DbType.Boolean, field.ShouldReturnToParent));
                insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));

                if (relatedViewIdExists)
                {
                    insertQuery.Parameters.Add(new QueryParameter("@RelatedViewId", DbType.Int32, field.RelatedViewID));
                }

                db.ExecuteNonQuery(insertQuery);

                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create field in the database", ex);
            }
        }

        /// <summary>
        /// Insert a SingleLineTextField record into the metaFields table.
        /// </summary>
        /// <param name="field">Single line text field.</param>
        /// <returns>Returns the Id of the last SingleLineTextField added.</returns>
        public int CreateField(SingleLineTextField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("SingleLineTextField");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([DataTableName], [ViewId], [UniqueId], [CheckCodeAfter], [CheckCodeBefore], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [IsReadOnly], [IsRequired], [IsEncrypted], [MaxLength], [Name], [PageId], [PromptFontFamily], [PromptFontStyle], [PromptFontSize], [PromptLeftPositionPercentage], [PromptText], [PromptTopPositionPercentage], [ShouldRepeatLast], [TabIndex]) " +
                    "values (@DataTableName, @ViewId, @UniqueId, @CheckCodeAfter, @CheckCodeBefore, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @FieldTypeId, @HasTabStop, @IsReadOnly, @IsRequired, @IsEncrypted, @MaxLength, @Name, @PageId, @PromptFontFamily, @PromptFontStyle, @PromptFontSize, @PromptLeftPositionPercentage, @PromptText, @PromptTopPositionPercentage, @ShouldRepeatLast, @TabIndex)");

                insertQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, field.TableName));
                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                if (field.CheckCodeAfter == null)
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));

                if (field.CheckCodeBefore == null)
                    insertQuery.Parameters.Add(new QueryParameter(ColumnNames.CHECK_CODE_BEFORE, DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeBefore));

                insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@IsEncrypted", DbType.Boolean, field.IsEncrypted));
                insertQuery.Parameters.Add(new QueryParameter("@MaxLength", DbType.Int16, field.MaxLength));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                insertQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));

                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create field in the database", ex);
            }
        }

        /// <summary>
        /// Insert a TimeField record into the metaFields table.
        /// </summary>
        /// <param name="field">Time field.</param>
        /// <returns>Returns the Id of the last TimeField added.</returns>
        public int CreateField(TimeField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("TimeField");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([DataTableName], [ViewId], [UniqueId], [CheckCodeAfter], [CheckCodeBefore], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [IsReadOnly], [IsRequired], [Name], [PageId], [PromptFontFamily], [PromptFontStyle], [PromptFontSize], [PromptLeftPositionPercentage], [PromptText], [PromptTopPositionPercentage], [ShouldRepeatLast], [TabIndex]) " +
                    "values (@DataTableName, @ViewId, @UniqueId, @CheckCodeAfter, @CheckCodeBefore, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @FieldTypeId, @HasTabStop, @IsReadOnly, @IsRequired, @Name, @PageId, @PromptFontFamily, @PromptFontStyle, @PromptFontSize, @PromptLeftPositionPercentage, @PromptText, @PromptTopPositionPercentage, @ShouldRepeatLast, @TabIndex)");

                insertQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, field.TableName));
                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                if (field.CheckCodeAfter == null)
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));

                if (field.CheckCodeBefore == null)
                    insertQuery.Parameters.Add(new QueryParameter(ColumnNames.CHECK_CODE_BEFORE, DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeBefore));

                insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                insertQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));


                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create field in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Insert a UpperCaseTextField record into the metaFields table.
        /// </summary>
        /// <param name="field">Upper-case text field.</param>
        /// <returns>Returns the Id of the last UpperCaseTextField added.</returns>
        public int CreateField(UpperCaseTextField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("UpperCaseTextField");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([DataTableName], [ViewId], [UniqueId], [CheckCodeAfter], [CheckCodeBefore], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [HasTabStop], [IsReadOnly], [IsRequired], [MaxLength], [Name], [PageId], [PromptFontFamily], [PromptFontStyle], [PromptFontSize], [PromptLeftPositionPercentage], [PromptText], [PromptTopPositionPercentage], [ShouldRepeatLast], [TabIndex]) " +
                    "values (@DataTableName, @ViewId, @UniqueId, @CheckCodeAfter, @CheckCodeBefore, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @HasTabStop, @IsReadOnly, @IsRequired, @MaxLength, @Name, @PageId, @PromptFontFamily, @PromptFontStyle, @PromptFontSize, @PromptLeftPositionPercentage, @PromptText, @PromptTopPositionPercentage, @ShouldRepeatLast, @TabIndex)");

                insertQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, field.TableName));
                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));
                if (field.CheckCodeAfter == null)
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));

                if (field.CheckCodeBefore == null)
                    insertQuery.Parameters.Add(new QueryParameter(ColumnNames.CHECK_CODE_BEFORE, DbType.String, DBNull.Value));
                else
                    insertQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeBefore));


                insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@MaxLength", DbType.Int16, field.MaxLength));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                insertQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));


                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create field in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Insert a YesNoField record into the metaFields table.
        /// </summary>
        /// <param name="field">Yes No field.</param>
        /// <returns>Returns the Id of the last YesNoField added.</returns>
        public int CreateField(YesNoField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("YesNoField");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([DataTableName], [ViewId], [UniqueId], [ControlFontFamily], [ControlFontStyle], [ControlFontSize], [ControlHeightPercentage], [ControlLeftPositionPercentage], [ControlTopPositionPercentage], [ControlWidthPercentage], [FieldTypeId], [HasTabStop], [IsReadOnly], [IsRequired], [Name], [PageId], [PromptFontFamily], [PromptFontStyle], [PromptFontSize], [PromptLeftPositionPercentage], [PromptText], [PromptTopPositionPercentage], [ShouldRepeatLast], [TabIndex], [CheckCodeBefore], [CheckCodeAfter]) " +
                    "values (@DataTableName, @ViewId, @UniqueId, @ControlFontFamily, @ControlFontStyle, @ControlFontSize, @ControlHeightPercentage, @ControlLeftPositionPercentage, @ControlTopPositionPercentage, @ControlWidthPercentage, @FieldTypeId, @HasTabStop, @IsReadOnly, @IsRequired, @Name, @PageId, @PromptFontFamily, @PromptFontStyle, @PromptFontSize, @PromptLeftPositionPercentage, @PromptText, @PromptTopPositionPercentage, @ShouldRepeatLast, @TabIndex, @CheckCodeBefore, @CheckCodeAfter)");

                insertQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, field.TableName));
                insertQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, field.GetView().Id));
                insertQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                insertQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                insertQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                insertQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                insertQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeBefore));
                insertQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));

                db.ExecuteNonQuery(insertQuery);
                return GetMaxFieldId(field.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create field in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.GlobalRecordIdColumn"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.GlobalRecordId"/></param>
        /// <returns>Id of the newly created column.</returns>
        public int CreateGridColumn(GlobalRecordIdColumn column)
        {
            try
            {
                #region InputValidation
                if (column == null)
                {
                    throw new ArgumentNullException("GlobalRecordIdColumn");
                }
                #endregion

                if (db.ColumnExists("metaGridColumns", "IsUniqueField") == false)
                {
                    TableColumn tableColumn = new TableColumn("IsUniqueField", GenericDbColumnType.Boolean, true);
                    db.AddColumn("metaGridColumns", tableColumn);
                }
                Query insertQuery = db.CreateQuery("insert into metaGridColumns([Name], [FieldTypeId], [Width], [Size], [Position], [Text], [ShouldRepeatLast], [IsRequired], [IsReadOnly], [FieldId], [IsUniqueField]) " +
                    "values (@Name, @FieldTypeId, @Width, @Size, @Position, @Text, @ShouldRepeatLast, @IsRequired, @IsReadOnly, @FieldId, @IsUniqueField)");

                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                insertQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, 0));
                insertQuery.Parameters.Add(new QueryParameter("@Size", DbType.Int32, 0));
                insertQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, 0));
                insertQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, string.Empty));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, false));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, false));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, true));
                insertQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, column.Grid.Id));
                insertQuery.Parameters.Add(new QueryParameter("@IsUniqueField", DbType.Boolean, true));

                db.ExecuteNonQuery(insertQuery);
                return GetMaxGridColumnId(column.Grid.Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create grid column in the database", ex);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.GlobalRecordIdColumn"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.GlobalRecordId"/></param>
        /// <returns>Id of the newly created column.</returns>
        public int CreateGridColumn(UniqueRowIdColumn column)
        {
            try
            {
                #region InputValidation
                if (column == null)
                {
                    throw new ArgumentNullException("UniqueRowIdColumn");
                }
                #endregion

                //Query query = db.CreateQuery("select F.[FieldTypeId],  F.[Name],  F.[HasRepeatLast], F.[HasRequired], F.[HasReadOnly], F.[HasRetainImageSize],  F.[HasFont], F.[IsGridColumn], F.[IsDropDown] " +
                //"from metaFieldTypes F " +
                //"where [F.FieldTypeId]=@FieldTypeId");
                Query query = db.CreateQuery("select [FieldTypeId],  [Name],  [HasRepeatLast], [HasRequired], [HasReadOnly], [HasRetainImageSize],  [HasFont],[IsGridColumn], [IsDropDown] " +
              "from metaFieldTypes  " +
              "where [FieldTypeId]=@FieldTypeId");

                query.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                DataTable dt = db.Select(query);
                if (dt.Rows.Count == 0)
                {
                    query = null;
                    query = db.CreateQuery("insert into metaFieldTypes ([FieldTypeId], [Name], [HasRepeatLast], [HasRequired], [HasReadOnly], [HasRetainImageSize], [HasFont], [IsDropDown], [IsGridColumn], [DataTypeId], [IsSystem],DefaultPatternId) values (@FieldTypeId, @Name, @HasRepeatLast, @HasRequired, @HasReadOnly, @HasRetainImageSize, @HasFont, @IsDropDown, @IsGridColumn, @DataTypeId, @IsSystem,@DefaultPatternId)");
                    query.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                    query.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                    query.Parameters.Add(new QueryParameter("@HasRepeatLast", DbType.Boolean, false));
                    query.Parameters.Add(new QueryParameter("@HasRequired", DbType.Boolean, false));
                    query.Parameters.Add(new QueryParameter("@HasReadOnly", DbType.Boolean, false));
                    query.Parameters.Add(new QueryParameter("@HasRetainImageSize", DbType.Boolean, false));
                    query.Parameters.Add(new QueryParameter("@HasFont", DbType.Boolean, false));
                    query.Parameters.Add(new QueryParameter("@IsDropDown", DbType.Boolean, false));
                    query.Parameters.Add(new QueryParameter("@IsGridColumn", DbType.Boolean, false));
                    query.Parameters.Add(new QueryParameter("@DataTypeId", DbType.Int32, 1));
                    query.Parameters.Add(new QueryParameter("@IsSystem", DbType.Boolean, true));
                    query.Parameters.Add(new QueryParameter("@DefaultPatternId", DbType.Int32, 0));
                    db.ExecuteNonQuery(query);
                }
                Query insertQuery = db.CreateQuery("insert into metaGridColumns([Name], [FieldTypeId], [Width], [Size], [Position], [Text], [ShouldRepeatLast], [IsRequired], [IsReadOnly], [FieldId]) " +
                    "values (@Name, @FieldTypeId, @Width, @Size, @Position, @Text, @ShouldRepeatLast, @IsRequired, @IsReadOnly, @FieldId)");

                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                insertQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, 0));
                insertQuery.Parameters.Add(new QueryParameter("@Size", DbType.Int32, 0));
                insertQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, 0));
                insertQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, string.Empty));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, false));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, false));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, true));
                insertQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, column.Grid.Id));

                db.ExecuteNonQuery(insertQuery);
                return GetMaxGridColumnId(column.Grid.Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create grid column in the database", ex);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.GlobalRecordIdColumn"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.GlobalRecordId"/></param>
        /// <returns>Id of the newly created column.</returns>
        public void AddGridColumn(DataRow gridColumnRow)
        {
            try
            {
                bool yesno = false;
                int result = 0;

                Query insertQuery = db.CreateQuery("insert into metaGridColumns([Name], [Size], [Position], [Text], [ShouldRepeatLast], [IsRequired], [IsReadOnly], [Pattern], [Upper], [Lower], [Width], [SourceTableName], [CodeColumnName], [TextColumnName], [Sort], [IsExclusiveTable], [DataTableName], [FieldId], [FieldTypeId], [IsUniqueField]) " +
                    "values (@Name, @Size, @Position, @Text, @ShouldRepeatLast, @IsRequired, @IsReadOnly, @Pattern, @Upper, @Lower, @Width, @SourceTableName, @CodeColumnName, @TextColumnName, @Sort, @IsExclusiveTable, @DataTableName, @FieldId, @FieldTypeId, @IsUniqueField)");

                //NAME
                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, (string)gridColumnRow["Name"]));
                
                //SIZE
                int size = 0;
                if(int.TryParse((string)gridColumnRow["Size"], out result))
                {
                    size = result;
                }
                insertQuery.Parameters.Add(new QueryParameter("@Size", DbType.Int32, size));

                //POSITION
                if (int.TryParse((string)gridColumnRow["Position"], out result))
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, result));
                }

                //TEXT
                insertQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, (string)gridColumnRow["Text"]));

                //SHOULD REPEAT LAST
                if (Boolean.TryParse((string)gridColumnRow["ShouldRepeatLast"], out yesno))
                {
                    insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, yesno));
                }

                //IS REQUIRED
                if (Boolean.TryParse((string)gridColumnRow["IsRequired"], out yesno))
                {
                    insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, yesno));
                }

                //IS READONLY
                if (Boolean.TryParse((string)gridColumnRow["IsReadOnly"], out yesno))
                {
                    insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, yesno));
                }

                // PATTERN
                insertQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, (string)gridColumnRow["Pattern"]));

                // UPPER
                insertQuery.Parameters.Add(new QueryParameter("@Upper", DbType.String, (string)gridColumnRow["Upper"]));

                // LOWER
                insertQuery.Parameters.Add(new QueryParameter("@Lower", DbType.String, (string)gridColumnRow["Lower"]));

                // WIDTH
                result = int.TryParse((string)gridColumnRow["Width"], out result) ? result : 32;
                insertQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, result));

                // SOURCE TABLE NAME
                insertQuery.Parameters.Add(new QueryParameter("@SourceTableName", DbType.String, (string)gridColumnRow["SourceTableName"]));
                
                // CODE COLUMN NAME
                insertQuery.Parameters.Add(new QueryParameter("@CodeColumnName", DbType.String, (string)gridColumnRow["CodeColumnName"]));

                // TEXT COLUMN NAME
                insertQuery.Parameters.Add(new QueryParameter("@TextColumnName", DbType.String, (string)gridColumnRow["TextColumnName"]));

                // SORT
                bool sort = true;
                if (Boolean.TryParse((string)gridColumnRow["Sort"], out yesno))
                {
                    sort = yesno;
                }
                insertQuery.Parameters.Add(new QueryParameter("@Sort", DbType.Boolean, sort));

                // IS EXCLUSIVE TABLE
                bool isExclusiveTable = false;
                if (Boolean.TryParse((string)gridColumnRow["IsExclusiveTable"], out yesno))
                {
                    isExclusiveTable = yesno;
                }
                insertQuery.Parameters.Add(new QueryParameter("@IsExclusiveTable", DbType.Boolean, isExclusiveTable));
                
                // DATA TABLE NAME
                insertQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, (string)gridColumnRow["DataTableName"]));

                // FIELD ID
                if (int.TryParse((string)gridColumnRow["FieldId"], out result))
                { 
                    insertQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, result));
                }

                // FIELD TYPE ID
                if (int.TryParse((string)gridColumnRow["FieldTypeId"], out result))
                {
                    insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, result));
                }

                // IS_UNIQUE_FIELD
                if (gridColumnRow.Table.Columns.Contains("IsUniqueField") && (bool.TryParse((string)gridColumnRow["IsUniqueField"], out yesno)))
                {
                    insertQuery.Parameters.Add(new QueryParameter("@IsUniqueField", DbType.Boolean, yesno));
                }
                else
                {
                    insertQuery.Parameters.Add(new QueryParameter("@IsUniqueField", DbType.Boolean, false));
                }

                db.ExecuteNonQuery(insertQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create grid column in the database", ex);
            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.UniqueKeyColumn"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.UniqueKeyColumn"/></param>
        /// <returns>Id of the newly created column.</returns>
        public int CreateGridColumn(UniqueKeyColumn column)
        {
            try
            {
                #region InputValidation
                if (column == null)
                {
                    throw new ArgumentNullException("UniqueKeyColumn");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaGridColumns([Name], [FieldTypeId], [Width], [Size], [Position], [Text], [ShouldRepeatLast], [IsRequired], [IsReadOnly], [FieldId]) " +
                    "values (@Name, @FieldTypeId, @Width, @Size, @Position, @Text, @ShouldRepeatLast, @IsRequired, @IsReadOnly, @FieldId)");

                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                insertQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, 0));
                insertQuery.Parameters.Add(new QueryParameter("@Size", DbType.Int32, 0));
                insertQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, 0));
                insertQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, string.Empty));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, false));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, false));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, false));
                insertQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, column.Grid.Id));

                db.ExecuteNonQuery(insertQuery);
                return GetMaxGridColumnId(column.Grid.Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create grid column in the database", ex);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Creates a RecStatus column in the database.
        /// </summary>
        /// <param name="column">A RecStatusColumn object</param>
        /// <returns>Id of the newly created column.</returns>
        public int CreateGridColumn(RecStatusColumn column)
        {
            try
            {
                #region InputValidation
                if (column == null)
                {
                    throw new ArgumentNullException("RecStatusColumn");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaGridColumns([Name], [FieldTypeId], [Width], [Size], [Position], [Text], [ShouldRepeatLast], [IsRequired], [IsReadOnly], [FieldId]) " +
                    "values (@Name, @FieldTypeId, @Width, @Size, @Position, @Text, @ShouldRepeatLast, @IsRequired, @IsReadOnly, @FieldId)");

                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                insertQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, 0));
                insertQuery.Parameters.Add(new QueryParameter("@Size", DbType.Int32, 0));
                insertQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, 1));
                insertQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, string.Empty));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, false));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, false));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, true));
                insertQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, column.Grid.Id));

                db.ExecuteNonQuery(insertQuery);
                return GetMaxGridColumnId(column.Grid.Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create grid RecStatus column in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.ForeignKeyColumn"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.ForeignKeyColumn"/></param>
        /// <returns>Id of the newly created ForeignKey column.</returns>
        public int CreateGridColumn(ForeignKeyColumn column)
        {
            try
            {
                #region InputValidation
                if (column == null)
                {
                    throw new ArgumentNullException("UniqueKeyColumn");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaGridColumns([Name], [FieldTypeId], [Width], [Size], [Position], [Text], [ShouldRepeatLast], [IsRequired], [IsReadOnly], [FieldId]) " +
                    "values (@Name, @FieldTypeId, @Width, @Size, @Position, @Text, @ShouldRepeatLast, @IsRequired, @IsReadOnly, @FieldId)");

                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                insertQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, 0));
                insertQuery.Parameters.Add(new QueryParameter("@Size", DbType.Int32, 0));
                insertQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, 0));
                insertQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, string.Empty));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, false));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, false));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, true));
                insertQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, column.Grid.Id));

                db.ExecuteNonQuery(insertQuery);
                return GetMaxGridColumnId(column.Grid.Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create grid ForeignKey column in the database", ex);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.NumberColumn"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.NumberColumn"/></param>
        /// <returns>Id of the newly created column.</returns>
        public int CreateGridColumn(NumberColumn column)
        {
            try
            {
                Query insertQuery = db.CreateQuery("insert into metaGridColumns([Name], [Width], [Position], [FieldTypeId], [Text], [ShouldRepeatLast], [IsRequired], [IsReadOnly], [Pattern], [Upper], [Lower], [FieldId]) " +
                    "values (@Name, @Width, @Position, @FieldTypeId, @Text, @ShouldRepeatLast, @IsRequired, @IsReadOnly, @Pattern, @Upper, @Lower, @FieldId)");

                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                insertQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, column.Width));
                insertQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, column.Position));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                insertQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, column.Text));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, column.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, column.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, column.IsReadOnly));
                if (column.Pattern == null)
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, DBNull.Value));
                }
                else
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, column.Pattern));
                }
                if (column.Upper == null)
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Upper", DbType.String, DBNull.Value));
                }
                else
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Upper", DbType.String, column.Upper));
                }
                if (column.Lower == null)
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Lower", DbType.String, DBNull.Value));
                }
                else
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Lower", DbType.String, column.Lower));
                }
                insertQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, column.Grid.Id));


                db.ExecuteNonQuery(insertQuery);
                return GetMaxGridColumnId(column.Grid.Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create grid column in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.DateColumn"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.DateColumn"/></param>
        /// <returns>Id of the newly created column.</returns>
        public int CreateGridColumn(ContiguousColumn column)
        {
            try
            {
                Query insertQuery = db.CreateQuery("insert into metaGridColumns([Name], [Width], [Position], [FieldTypeId], [Text], [ShouldRepeatLast], [IsRequired], [IsReadOnly], [Pattern], [Upper], [Lower], [FieldId]) " +
                    "values (@Name, @Width, @Position, @FieldTypeId, @Text, @ShouldRepeatLast, @IsRequired, @IsReadOnly, @Pattern, @Upper, @Lower, @FieldId)");

                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                insertQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, column.Width));
                insertQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, column.Position));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                insertQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, column.Text));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, column.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, column.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, column.IsReadOnly));
                if (column.Pattern == null)
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, DBNull.Value));
                }
                else
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, column.Pattern));
                }
                if (column.Upper == null)
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Upper", DbType.String, DBNull.Value));
                }
                else
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Upper", DbType.String, column.Upper));
                }
                if (column.Lower == null)
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Lower", DbType.String, DBNull.Value));
                }
                else
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Lower", DbType.String, column.Lower));
                }
                insertQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, column.Grid.Id));


                db.ExecuteNonQuery(insertQuery);
                return GetMaxGridColumnId(column.Grid.Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create grid column in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.PhoneNumberColumn"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.PhoneNumberColumn"/></param>
        /// <returns>Id of the newly created column.</returns>
        public int CreateGridColumn(PhoneNumberColumn column)
        {
            try
            {
                Query insertQuery = db.CreateQuery("insert into metaGridColumns([Name], [Width], [Position], [FieldTypeId], [Text], [ShouldRepeatLast], [IsRequired], [IsReadOnly], [Pattern], [FieldId]) " +
                    "values (@Name, @Width, @Position, @FieldTypeId, @Text, @ShouldRepeatLast, @IsRequired, @IsReadOnly, @Pattern, @FieldId)");

                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                insertQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, column.Width));
                insertQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, column.Position));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                insertQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, column.Text));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, column.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, column.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, column.IsReadOnly));
                if (column.Pattern == null)
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, DBNull.Value));
                }
                else
                {
                    insertQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, column.Pattern));
                }
                insertQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, column.Grid.Id));


                db.ExecuteNonQuery(insertQuery);
                return GetMaxGridColumnId(column.Grid.Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create grid column in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.TextColumn"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.TextColumn"/></param>
        /// <returns>Id of the newly created column.</returns>
        public int CreateGridColumn(TextColumn column)
        {
            try
            {
                Query insertQuery = db.CreateQuery("insert into metaGridColumns([Name], [Width], [Size], [Position], [FieldTypeId], [Text], [ShouldRepeatLast], [IsRequired], [IsReadOnly], [FieldId]) " +
                    "values (@Name, @Width, @Size, @Position, @FieldTypeId, @Text, @ShouldRepeatLast, @IsRequired, @IsReadOnly, @FieldId)");

                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                insertQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, column.Width));
                insertQuery.Parameters.Add(new QueryParameter("@Size", DbType.Int32, column.Size));
                insertQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, column.Position));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                insertQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, column.Text));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, column.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, column.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, column.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, column.Grid.Id));


                db.ExecuteNonQuery(insertQuery);
                return GetMaxGridColumnId(column.Grid.Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create grid column in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.TextColumn"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.TextColumn"/></param>
        /// <returns>Id of the newly created column.</returns>
        public int CreateGridColumn(CheckboxColumn column)
        {
            try
            {
                Query insertQuery = db.CreateQuery("insert into metaGridColumns([Name], [Width], [Position], [FieldTypeId], [Text], [ShouldRepeatLast], [IsRequired], [IsReadOnly], [FieldId]) " +
                    "values (@Name, @Width, @Position, @FieldTypeId, @Text, @ShouldRepeatLast, @IsRequired, @IsReadOnly, @FieldId)");

                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                insertQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, column.Width));
                insertQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, column.Position));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                insertQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, column.Text));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, column.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, column.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, column.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, column.Grid.Id));


                db.ExecuteNonQuery(insertQuery);
                int maxGridColumnId = GetMaxGridColumnId(column.Grid.Id);
                return maxGridColumnId; 
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create grid column in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.YesNoColumn"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.YesNoColumn"/></param>
        /// <returns>Id of the newly created column.</returns>
        public int CreateGridColumn(YesNoColumn column)
        {
            try
            {
                Query insertQuery = db.CreateQuery("insert into metaGridColumns([Name], [Width], [Position], [FieldTypeId], [Text], [ShouldRepeatLast], [IsRequired], [IsReadOnly], [FieldId]) " +
                    "values (@Name, @Width, @Position, @FieldTypeId, @Text, @ShouldRepeatLast, @IsRequired, @IsReadOnly, @FieldId)");

                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                insertQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, column.Width));
                insertQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, column.Position));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                insertQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, column.Text));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, column.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, column.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, column.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, column.Grid.Id));


                db.ExecuteNonQuery(insertQuery);
                int maxGridColumnId = GetMaxGridColumnId(column.Grid.Id);
                return maxGridColumnId;
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create grid column in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.DDLColumnOfCommentLegal"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.DDLColumnOfCommentLegal"/></param>
        /// <returns>Id of the newly created column.</returns>
        public int CreateGridColumn(DDLColumnOfCommentLegal column)
        {
            try
            {
                if (db.ColumnExists("metaGridColumns", "IsUniqueField") == false)
                {
                    TableColumn tableColumn = new TableColumn("IsUniqueField", GenericDbColumnType.Boolean, true);
                    db.AddColumn("metaGridColumns", tableColumn);
                }
                
                Query insertQuery = db.CreateQuery("insert into metaGridColumns([Name], [TextColumnName], [SourceTableName], [Width], [Position], [FieldTypeId], [Text], [ShouldRepeatLast], [IsExclusiveTable], [IsReadOnly], [IsRequired], [IsUniqueField], [FieldId], [Sort]) " +
                    "values (@Name, @TextColumnName, @SourceTableName, @Width, @Position, @FieldTypeId, @Text, @ShouldRepeatLast, @IsExclusiveTable, @IsReadOnly, @IsRequired, @IsUniqueField, @FieldId, @Sort)");

                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                insertQuery.Parameters.Add(new QueryParameter("@TextColumnName", DbType.String, column.TextColumnName));
                insertQuery.Parameters.Add(new QueryParameter("@SourceTableName", DbType.String, column.SourceTableName));
                insertQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, column.Width));
                insertQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, column.Position));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                insertQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, column.Text));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, column.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@IsExclusiveTable", DbType.Boolean, column.IsExclusiveTable));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, column.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, column.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@IsUniqueField", DbType.Boolean, column.IsUniqueField));
                insertQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, column.Grid.Id));
                insertQuery.Parameters.Add(new QueryParameter("@Sort", DbType.Boolean, column.ShouldSort));

                db.ExecuteNonQuery(insertQuery);
                return GetMaxGridColumnId(column.Grid.Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create grid column in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.DDLColumnOfLegalValues"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.DDLColumnOfLegalValues"/></param>
        /// <returns>Id of the newly created column.</returns>
        public int CreateGridColumn(DDLColumnOfLegalValues column)
        {
            try
            {
                if (db.ColumnExists("metaGridColumns", "IsUniqueField") == false)
                {
                    TableColumn tableColumn = new TableColumn("IsUniqueField", GenericDbColumnType.Boolean, true);
                    db.AddColumn("metaGridColumns", tableColumn);
                }

                Query insertQuery = db.CreateQuery("insert into metaGridColumns([Name], [TextColumnName], [SourceTableName], [Width], [Position], [FieldTypeId], [Text], [ShouldRepeatLast], [IsExclusiveTable], [IsReadOnly], [IsRequired], [IsUniqueField], [FieldId], [Sort]) " +
                    "values (@Name, @TextColumnName, @SourceTableName, @Width, @Position, @FieldTypeId, @Text, @ShouldRepeatLast, @IsExclusiveTable, @IsReadOnly, @IsRequired, @IsUniqueField, @FieldId, @Sort)");

                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                insertQuery.Parameters.Add(new QueryParameter("@TextColumnName", DbType.String, column.TextColumnName));
                insertQuery.Parameters.Add(new QueryParameter("@SourceTableName", DbType.String, column.SourceTableName));
                insertQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, column.Width));
                insertQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, column.Position));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                insertQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, column.Text));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, column.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@IsExclusiveTable", DbType.Boolean, column.IsExclusiveTable));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, column.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, column.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@IsUniqueField", DbType.Boolean, column.IsUniqueField));
                insertQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, column.Grid.Id));
                insertQuery.Parameters.Add(new QueryParameter("@Sort", DbType.Boolean, column.ShouldSort));

                db.ExecuteNonQuery(insertQuery);
                return GetMaxGridColumnId(column.Grid.Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create grid column in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Creates a label for a specified field
        /// </summary>
        /// <param name="fieldId">Id of the field</param>
        /// <param name="labelText">Text of the label</param>
        /// <param name="topPosition">Vertical coordinate of the label</param>
        /// <param name="leftPosition">Horizontal coordinate of the label</param>
        /// <param name="font">Font of the label's text</param>
        /// <param name="fontSize">Font size of the label's text</param>
        public void CreateLabel(int fieldId, string labelText, double topPosition, double leftPosition, string font, decimal fontSize)
        {
            try
            {

                #region Input Validation
                if (string.IsNullOrEmpty(labelText))
                {
                    throw new ArgumentNullException("LabelText");
                }
                if (string.IsNullOrEmpty(font))
                {
                    throw new ArgumentNullException("Font");
                }
                if (fieldId < 1)
                {
                    throw new ArgumentOutOfRangeException("FieldId");
                }
                if (topPosition < 0)
                {
                    throw new ArgumentOutOfRangeException("TopPosition");
                }
                if (leftPosition < 0)
                {
                    throw new ArgumentOutOfRangeException("LeftPosition");
                }
                if (fontSize <= 0)
                {
                    throw new ArgumentOutOfRangeException("FontSize");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([FieldId], [PromptText], [PromptTopPositionPercentage], " +
                    "[PromptLeftPositionPercentage], [PromptFontFamily], [PromptFontSize]) values (@FieldId, @Text, @TopPosition, @LeftPosition, @FontStyleId, @FontFamily, @FontSize)");
                //				DbQuery insertQuery = db.CreateQuery("insert into metaLabels([FieldId], [Text], [TopPosition], [LeftPosition], [FontStyleId], [FontFamily], [FontSize]) values (@FieldId, @Text, @TopPosition, @LeftPosition, @FontStyleId, @FontFamily, @FontSize)");
                insertQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, fieldId));
                insertQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, labelText));
                insertQuery.Parameters.Add(new QueryParameter("@TopPosition", DbType.Double, topPosition));
                insertQuery.Parameters.Add(new QueryParameter("@LeftPosition", DbType.Double, leftPosition));
                insertQuery.Parameters.Add(new QueryParameter("@FontFamily", DbType.String, font));
                insertQuery.Parameters.Add(new QueryParameter("@FontSize", DbType.Decimal, fontSize));


                db.ExecuteNonQuery(insertQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create label in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Creates a control for a specified field
        /// </summary>
        /// <param name="fieldId">Id of the field</param>
        /// <param name="topPosition">Vertical coordinate of the control</param>
        /// <param name="leftPosition">Horizontal coordinate of the control</param>
        /// <param name="height">Height of the control</param>
        /// <param name="width">Width of the control</param>
        /// <param name="isRepeatLast">Is this a Repeat Last control?</param>
        /// <param name="isRequired">Is this a Required control?</param>
        /// <param name="isReadOnly">Is this a Read Only control?</param>		
        /// <param name="isRetainImgSize">Is image size retained?</param>
        /// <param name="tabOrder">Tab order of the control</param>
        /// <returns>Id of the newly created control</returns>
        public int CreateControl(int fieldId, double topPosition, double leftPosition, double height, double width, bool isRepeatLast, bool isRequired, bool isReadOnly, bool isRetainImgSize, int tabOrder)
        {
            try
            {

                #region Input Validation
                if (fieldId < 1)
                {
                    throw new ArgumentOutOfRangeException("FieldId");
                }
                if (topPosition < 0)
                {
                    throw new ArgumentOutOfRangeException("TopPosition");
                }
                if (leftPosition < 0)
                {
                    throw new ArgumentOutOfRangeException("LeftPosition");
                }
                if (height < 0)
                {
                    throw new ArgumentOutOfRangeException("Height");
                }
                if (width < 0)
                {
                    throw new ArgumentOutOfRangeException("Width");
                }
                if (tabOrder < 0)
                {
                    throw new ArgumentOutOfRangeException("TabOrder");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaFields([FieldId], [ControlTopPositionPercentage], [ControlLeftPositionPercentage], " +
                    "[ControlHeightPercentage], [ControlWidthPercentage], [ShouldRepeatLast], [IsRequired], [IsReadOnly], [ShouldRetainImageSize], " +
                    "[TabIndex], [HasTabStop]) values (@FieldId, @TopPosition, @LeftPosition, @Height, @Width, @IsRepeatLast, @IsRequired, @IsReadOnly, @IsRetainImgSize, @TabOrder, @IsTabStop)");

                insertQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, fieldId));
                insertQuery.Parameters.Add(new QueryParameter("@TopPosition", DbType.Double, topPosition));
                insertQuery.Parameters.Add(new QueryParameter("@LeftPosition", DbType.Double, leftPosition));
                insertQuery.Parameters.Add(new QueryParameter("@Height", DbType.Double, height));
                insertQuery.Parameters.Add(new QueryParameter("@Width", DbType.Double, width));
                insertQuery.Parameters.Add(new QueryParameter("@IsRepeatLast", DbType.Boolean, isRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, isRequired));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, isReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@IsRetainImgSize", DbType.Boolean, isRetainImgSize));
                insertQuery.Parameters.Add(new QueryParameter("@TabOrder", DbType.Int32, tabOrder));
                insertQuery.Parameters.Add(new QueryParameter("@IsTabStop", DbType.Boolean, true));

                Query selectQuery = db.CreateQuery("select MAX(FieldId) from metaFields where FieldId = @FieldId");
                selectQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, fieldId));

                db.ExecuteNonQuery(insertQuery);

                DataTable result = db.Select(selectQuery);
                return (int)result.Rows[0][0];
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create control in the database", ex);
            }
        }

        /// <summary>
        /// Saves a view reocrd to database
        /// </summary>
        /// <param name="view"></param>
        public void UpdateView(View view)
        {
            try
            {
                StringBuilder queryString = new StringBuilder();

                queryString.Append("update metaViews set [CheckCodeVariableDefinitions] = @CheckCodeVariableDefinitions, [CheckCode] = @CheckCode, [CheckCodeBefore] = @CheckCodeBefore, [CheckCodeAfter] = @CheckCodeAfter, [RecordCheckCodeBefore] = @RecordCheckCodeBefore, [RecordCheckCodeAfter] = @RecordCheckCodeAfter, [IsRelatedView] = @IsRelatedView, [Width] = @Width, [Height] = @Height, [Orientation] = @Orientation, [LabelAlign] = @LabelAlign");
               

                if (db.ColumnExists("metaViews", "EIWSOrganizationKey") != false)
                {
                    queryString.Append(",[EIWSOrganizationKey]= @EIWSOrganizationKey");
                }
                else
                {
                    TableColumn tableColumn = new TableColumn("EIWSOrganizationKey", GenericDbColumnType.String , true);
                    db.AddColumn("metaViews", tableColumn);
                    queryString.Append(",[EIWSOrganizationKey]= @EIWSOrganizationKey");
                }
                ///
                if (db.ColumnExists("metaViews", "EIWSFormId") != false)
                {
                    queryString.Append(",[EIWSFormId]= @EIWSFormId");
                }
                else
                {
                    TableColumn tableColumn = new TableColumn("EIWSFormId", GenericDbColumnType.String, true);
                    db.AddColumn("metaViews", tableColumn);
                    queryString.Append(",[EIWSFormId]= @EIWSFormId");
                }
                ///
                if (db.ColumnExists("metaViews", "EWEOrganizationKey") != false)
                {
                    queryString.Append(",[EWEOrganizationKey]= @EWEOrganizationKey");
                }
                else
                {
                    TableColumn tableColumn = new TableColumn("EWEOrganizationKey", GenericDbColumnType.String, true);
                    db.AddColumn("metaViews", tableColumn);
                    queryString.Append(",[EWEOrganizationKey]= @EWEOrganizationKey");
                }
                ///
                if (db.ColumnExists("metaViews", "EWEFormId") != false)
                {
                    queryString.Append(",[EWEFormId]= @EWEFormId");
                }
                else
                {
                    TableColumn tableColumn = new TableColumn("EWEFormId", GenericDbColumnType.String, true);
                    db.AddColumn("metaViews", tableColumn);
                    queryString.Append(",[EWEFormId]= @EWEFormId");
                }

               
                queryString.Append(" where [ViewId] = @ViewId");

                Query query = db.CreateQuery(queryString.ToString());

                query.Parameters.Add(new QueryParameter("@CheckCodeVariableDefinitions", DbType.String, view.CheckCodeVariableDefinitions));
                query.Parameters.Add(new QueryParameter("@CheckCode", DbType.String, view.CheckCode));
                query.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, view.CheckCodeBefore));
                query.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, view.WebSurveyId));
                query.Parameters.Add(new QueryParameter("@RecordCheckCodeBefore", DbType.String, view.RecordCheckCodeBefore));
                query.Parameters.Add(new QueryParameter("@RecordCheckCodeAfter", DbType.String, view.RecordCheckCodeAfter));
                query.Parameters.Add(new QueryParameter("@IsRelatedView", DbType.Boolean, view.IsRelatedView));
                query.Parameters.Add(new QueryParameter("@Width", DbType.Int32, view.PageWidth));
                query.Parameters.Add(new QueryParameter("@Height", DbType.Int32, view.PageHeight));
                query.Parameters.Add(new QueryParameter("@Orientation", DbType.String, view.PageOrientation));
                query.Parameters.Add(new QueryParameter("@LabelAlign", DbType.String, view.PageLabelAlign));
                //get config data if available 
                var ConfigData = GetPublishedViewKeys(view.Id);
                DataRow ViewRow = null;
                if (ConfigData.Rows.Count>0) {
                     ViewRow = ConfigData.Rows[0];

                }
                if (db.ColumnExists("metaViews", "EIWSOrganizationKey") != false)
                {
                    string EIWSOrganizationKey = "";
                    if (ViewRow!= null) {
                        EIWSOrganizationKey = ViewRow.ItemArray[0].ToString();
                    }
                    if (!string.IsNullOrEmpty(EIWSOrganizationKey))
                    {
                        query.Parameters.Add(new QueryParameter("@EIWSOrganizationKey", DbType.String, EIWSOrganizationKey));
                    }
                    else {
                        query.Parameters.Add(new QueryParameter("@EIWSOrganizationKey", DbType.String, view.EIWSOrganizationKey));
                    }
                }
                
                ///
                if (db.ColumnExists("metaViews", "EIWSFormId") != false){


                    string EIWSFormId = "";
                    if (ViewRow != null)
                    {
                        EIWSFormId = ViewRow.ItemArray[1].ToString();
                    }
                    if (!string.IsNullOrEmpty(EIWSFormId))
                    {
                        query.Parameters.Add(new QueryParameter("@EIWSFormId", DbType.String, EIWSFormId));
                    }
                    else
                    {
                        query.Parameters.Add(new QueryParameter("@EIWSFormId", DbType.String, view.EIWSFormId));


                    }
                }
                 
                ///
                if (db.ColumnExists("metaViews", "EWEOrganizationKey") != false)
                {
                    string EWEOrganizationKey = "";
                    if (ViewRow != null)
                    {
                        EWEOrganizationKey = ViewRow.ItemArray[2].ToString();
                    }
                    if (!string.IsNullOrEmpty(EWEOrganizationKey))
                    {
                        query.Parameters.Add(new QueryParameter("@EWEOrganizationKey", DbType.String, EWEOrganizationKey));
                    }
                    else {
                        query.Parameters.Add(new QueryParameter("@EWEOrganizationKey", DbType.String, view.EWEOrganizationKey));
                    }
                }
               
                ///
                if (db.ColumnExists("metaViews", "EWEFormId") != false)
                {

                    string EWEFormId = "";
                    if (ViewRow != null)
                    {
                        EWEFormId = ViewRow.ItemArray[3].ToString();
                    }
                    if (!string.IsNullOrEmpty(EWEFormId))
                    {
                        query.Parameters.Add(new QueryParameter("@EWEFormId", DbType.String, EWEFormId));
                    }
                    else {
                        query.Parameters.Add(new QueryParameter("@EWEFormId", DbType.String, view.EWEFormId));

                    }
                  }
                 
                query.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, view.Id));
                
                int i = db.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update view in the database", ex);
            }
            finally
            {

            }
        }

        

        /// <summary>
        /// Create variable check code in the database
        /// </summary>
        /// <param name="viewId">Id of the view</param>
        /// <param name="checkCode">Text of the check code</param>
        public void CreateCheckCodeVariableDefinition(int viewId, string checkCode)
        {
            try
            {

                #region Input Validation
                //				if (string.IsNullOrEmpty(checkCode))				
                //				{
                //					throw new ArgumentNullException("CheckCode");
                //				}
                if (viewId < 1)
                {
                    throw new ArgumentOutOfRangeException("ViewId");
                }

                // Validate that the EXECUTE CheckCode is formed correctly. The form changed 
                // between Epi 3 and Epi 7.
                if (checkCode.StartsWith(CommandNames.EXECUTE + StringLiterals.SPACE + StringLiterals.DOUBLEQUOTES))
                {
                    string newString = string.Empty;

                    checkCode = checkCode.Replace(CommandNames.EXECUTE + StringLiterals.SPACE + StringLiterals.DOUBLEQUOTES, string.Empty);
                    newString = CommandNames.EXECUTE + StringLiterals.SPACE + CommandNames.NOWAITFOREXIT + StringLiterals.SPACE + StringLiterals.DOUBLEQUOTES + checkCode;

                    checkCode = newString;
                }

                #endregion

                Query updateQuery = db.CreateQuery("update metaViews set [CheckCodeVariableDefinitions] = @CheckCodeVariableDefinitions where [ViewId] = @ViewId");
                updateQuery.Parameters.Add(new QueryParameter("@CheckCodeVariableDefinitions", DbType.String, checkCode));
                updateQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, viewId));


                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create variable checkcode in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Create view before check code in the database
        /// </summary>
        /// <param name="viewId">Id of the view</param>
        /// <param name="checkCode">Text of the check code</param>
        public void CreateViewBeforeCheckCode(int viewId, string checkCode)
        {
            try
            {
                #region Input Validation
                //				if (string.IsNullOrEmpty(checkCode))				
                //				{
                //					throw new ArgumentNullException("CheckCode");
                //				}
                if (viewId < 1)
                {
                    throw new ArgumentOutOfRangeException("ViewId");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaViews set [CheckCodeBefore] = @ViewCheckCodeBefore where [ViewId] = @ViewId");
                updateQuery.Parameters.Add(new QueryParameter("@ViewCheckCodeBefore", DbType.String, checkCode));
                updateQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, viewId));


                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create view checkcode in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Create view after check code in the database
        /// </summary>
        /// <param name="viewId">Id of the view</param>
        /// <param name="checkCode">Text of the check code</param>
        public void CreateViewAfterCheckCode(int viewId, string checkCode)
        {
            try
            {
                #region Input Validation
                //				if (string.IsNullOrEmpty(checkCode))				
                //				{
                //					throw new ArgumentNullException("CheckCode");
                //				}
                if (viewId < 1)
                {
                    throw new ArgumentOutOfRangeException("ViewId");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaViews set [CheckCodeAfter] = @ViewCheckCodeAfter where [ViewId] = @ViewId");
                updateQuery.Parameters.Add(new QueryParameter("@ViewCheckCodeAfter", DbType.String, checkCode));
                updateQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, viewId));


                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create view checkcode in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Create record before check code in the database
        /// </summary>
        /// <param name="viewId">Id of the view</param>
        /// <param name="checkCode">Text of the check code</param>
        public void CreateRecordBeforeCheckCode(int viewId, string checkCode)
        {
            try
            {

                #region Input Validation
                //				if (string.IsNullOrEmpty(checkCode))				
                //				{
                //					throw new ArgumentNullException("CheckCode");
                //				}
                if (viewId < 1)
                {
                    throw new ArgumentOutOfRangeException("ViewId");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaViews set [RecordCheckCodeBefore] = @RecordCheckCodeBefore where [ViewId] = @ViewId");
                updateQuery.Parameters.Add(new QueryParameter("@RecordCheckCodeBefore", DbType.String, checkCode));
                updateQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, viewId));


                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create record checkcode in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Create record after check code in the database
        /// </summary>
        /// <param name="viewId">Id of the view</param>
        /// <param name="checkCode">Text of the check code</param>
        public void CreateRecordAfterCheckCode(int viewId, string checkCode)
        {
            try
            {

                #region Input Validation
                //				if (string.IsNullOrEmpty(checkCode))				
                //				{
                //					throw new ArgumentNullException("CheckCode");
                //				}
                if (viewId < 1)
                {
                    throw new ArgumentOutOfRangeException("ViewId");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaViews set [RecordCheckCodeAfter] = @RecordCheckCodeAfter where [ViewId] = @ViewId");
                updateQuery.Parameters.Add(new QueryParameter("@RecordCheckCodeAfter", DbType.String, checkCode));
                updateQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, viewId));


                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create record checkcode in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Removes the old way of setting a GUID in the CheckCode
        /// </summary>
        /// <param name="checkCode">CheckCode with the old GUID code in it</param>
        /// <returns>The CheckCode with the old GUID code removed</returns>
        private string RemoveOldGUIDCheckCode(string checkCode)
        {
            /*  Remove this CheckCode
             
                ALWAYS
                IF CDCUNIQUEID=(.) OR CDCUNIQUEID=""  THEN
                ASSIGN CDCUNIQUEID = GLOBAL_ID!GetGlobalUniqueID()
                ASSIGN CDC_UNIQUE_ID=CDCUNIQUEID
                END
                END
            */
            string RetVal = string.Empty;
            RetVal = checkCode.Replace(StringLiterals.OLD_GUID_CODE, string.Empty);

            return RetVal;
        }

        /// <summary>
        /// Updates a page in the database.
        /// </summary>
        /// <param name="page">Current <see cref="Epi.Page"/>.</param>
        public void UpdatePage(Page page)
        {
            #region Input Validation
            
            if (page.CheckCodeBefore.Contains("GLOBAL_ID!GetGlobalUniqueID()"))
            {
                page.CheckCodeBefore = RemoveOldGUIDCheckCode(page.CheckCodeBefore);
            }

            if(page.CheckCodeAfter.Contains("GLOBAL_ID!GetGlobalUniqueID()"))
            {
                page.CheckCodeAfter = RemoveOldGUIDCheckCode(page.CheckCodeAfter);
            }

            #endregion

            try
            {
                string queryString = "update metaPages set [Name] = @name, [Position] = @position, [CheckCodeBefore] = @CheckCodeBefore, [CheckCodeAfter] = @CheckCodeAfter, [BackgroundId] = @BackgroundId where [PageId] = @PageId";
                Query updateQuery = db.CreateQuery(queryString);
                updateQuery.Parameters.Add(new QueryParameter("@name", DbType.String, page.Name));
                updateQuery.Parameters.Add(new QueryParameter("@position", DbType.Int32, page.Position));
                updateQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, page.CheckCodeBefore));
                updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, page.CheckCodeAfter));
                updateQuery.Parameters.Add(new QueryParameter("@BackgroundId", DbType.String, page.BackgroundId));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, page.Id));

                if (db.ExecuteNonQuery(updateQuery) == 0)
                {
                    InsertPage(page);
                }
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create page checkcode in the database", ex);
            }
            finally
            {

            }
        }


        /// <summary>
        /// Create page before check code in the database
        /// </summary>
        /// <param name="pageId">Id of the page</param>
        /// <param name="checkCode">Text of the check code</param>
        /// <param name="view">Current view.</param>
        public void CreatePageBeforeCheckCode(int pageId, string checkCode, View view)
        {
            try
            {

                #region Input Validation
                //				if (string.IsNullOrEmpty(checkCode))				
                //				{
                //					throw new ArgumentNullException("CheckCode");
                //				}
                if (pageId < 1)
                {
                    throw new ArgumentOutOfRangeException("PageId");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaPages set [CheckCodeBefore] = @CheckCodeBefore where [PageId] = @PageId");
                updateQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, checkCode));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, pageId));


                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create page checkcode in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Create page after check code in the database
        /// </summary>
        /// <param name="pageId">Id of the page</param>
        /// <param name="checkCode">Text of the check code</param>
        /// <param name="view">Current view.</param>
        public void CreatePageAfterCheckCode(int pageId, string checkCode, View view)
        {
            try
            {

                #region Input Validation
                //				if (string.IsNullOrEmpty(checkCode))				
                //				{
                //					throw new ArgumentNullException("CheckCode");
                //				}
                if (pageId < 1)
                {
                    throw new ArgumentOutOfRangeException("PageId");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaPages set [CheckCodeAfter] = @CheckCodeAfter where [PageId] = @PageId");
                updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, checkCode));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, pageId));


                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create page checkcode in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Create control before check code in the database
        /// </summary>
        /// <param name="fieldId">Id of the field</param>
        /// <param name="checkCode">Text of the check code</param>
        /// <param name="view">Current view.</param>
        public void CreateControlBeforeCheckCode(int fieldId, string checkCode, View view)
        {
            try
            {

                #region Input Validation
                //				if (string.IsNullOrEmpty(checkCode))				
                //				{
                //					throw new ArgumentNullException("CheckCode");
                //				}
                if (fieldId < 1)
                {
                    throw new ArgumentOutOfRangeException("FieldId");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaFields set [CheckCodeBefore] = @CheckCodeBefore where [FieldId] = @FieldId");

                //				DbQuery updateQuery = db.CreateQuery("update metaControls set [CheckCodeBefore] = @CheckCodeBefore where [ControlId] = @ControlId");
                updateQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, checkCode));
                updateQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, fieldId));


                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create control checkcode in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Create control after check code in the database
        /// </summary>
        /// <param name="fieldId">Id of the control</param>
        /// <param name="checkCode">Text of the check code</param>
        /// <param name="view">Current view.</param>
        public void CreateControlAfterCheckCode(int fieldId, string checkCode, View view)
        {
            try
            {

                #region Input Validation
                //				if (string.IsNullOrEmpty(checkCode))				
                //				{
                //					throw new ArgumentNullException("CheckCode");
                //				}
                if (fieldId < 1)
                {
                    throw new ArgumentOutOfRangeException("FieldId");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaFields set [CheckCodeAfter] = @CheckCodeAfter where [FieldId] = @FieldId");
                updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, checkCode));
                updateQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, fieldId));


                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create control checkcode in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Inserts a record in a code table
        /// </summary>
        /// <param name="tableName">Name of the code table</param>
        /// <param name="columnNames">Column names of the code table</param>
        /// <param name="columnData">Data for each column</param>
        public void CreateCodeTableRecord(string tableName, string[] columnNames, string[] columnData)
        {
            Query insertQuery;
            StringBuilder sb = null;

            try
            {
                sb = new StringBuilder();

                sb.Append(SqlKeyWords.INSERT);
                sb.Append(StringLiterals.SPACE);
                sb.Append(SqlKeyWords.INTO);
                sb.Append(StringLiterals.LEFT_SQUARE_BRACKET);
                sb.Append(tableName);
                sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET);
                sb.Append(StringLiterals.PARANTHESES_OPEN);

                for (int i = 0; i < columnNames.Length; i++)
                {
                    sb.Append(StringLiterals.LEFT_SQUARE_BRACKET);
                    sb.Append(columnNames[i]);
                    sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET);

                    if (i < columnNames.Length - 1)
                    {
                        sb.Append(StringLiterals.COMMA);
                        sb.Append(StringLiterals.SPACE);
                    }
                }

                sb.Append(StringLiterals.PARANTHESES_CLOSE);
                sb.Append(StringLiterals.SPACE);
                sb.Append(SqlKeyWords.VALUES);
                sb.Append(StringLiterals.PARANTHESES_OPEN);

                for (int i = 0; i < columnNames.Length; i++)
                {
                    sb.Append(StringLiterals.COMMERCIAL_AT);
                    sb.Append(columnNames[i]);

                    if (i < columnNames.Length - 1)
                    {
                        sb.Append(StringLiterals.COMMA);
                        sb.Append(StringLiterals.SPACE);
                    }
                }

                sb.Append(StringLiterals.PARANTHESES_CLOSE);

                insertQuery = db.CreateQuery(sb.ToString());

                for (int i = 0; i < columnNames.Length; i++)
                {
                    insertQuery.Parameters.Add(new QueryParameter(StringLiterals.COMMERCIAL_AT + columnNames[i], DbType.String, columnData[i]));
                }

                db.ExecuteNonQuery(insertQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create record in code table", ex);
            }
            finally
            {
                insertQuery = null;
                sb = null;
            }
        }

        /// <summary>
        /// Create Image Table
        /// </summary>
        public void CreateImagesTable()
        {
            List<TableColumn> columns = new List<TableColumn>();
            columns.Add(new TableColumn("ImageId", GenericDbColumnType.Int32, false, true, true));
            columns.Add(new TableColumn("Image", GenericDbColumnType.Image, true));
            columns.Add(new TableColumn("ImageUniqueValue", GenericDbColumnType.Int32, true));
            db.CreateTable("metaImages", columns);
        }

        /// <summary>
        /// Saves data for a code table
        /// </summary>
        /// <param name="dataTable">DataTable containg data to be saved</param>
        /// <param name="tableName">Name of the code table</param>
        /// <param name="columnName">Column name of the code table</param>
        public void SaveCodeTableData(DataTable dataTable, string tableName, string columnName)
        {
            try
            {
                Query updateQuery = db.CreateQuery("update " + tableName + " set [" + columnName + "] = @NewValue where [" + columnName + "] = @OldValue");
                updateQuery.Parameters.Add(new QueryParameter("@NewValue", DbType.String, columnName, columnName));
                updateQuery.Parameters.Add(new QueryParameter("@OldValue", DbType.String, columnName, columnName));
                updateQuery.Parameters[1].SourceVersion = DataRowVersion.Original;


                Query insertQuery = db.CreateQuery("insert into " + tableName + "([" + columnName + "]) values (@Value)");
                insertQuery.Parameters.Add(new QueryParameter("@Value", DbType.String, columnName, columnName));
                db.Update(dataTable, tableName, insertQuery, updateQuery);
                
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not save data for code table", ex.StackTrace);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> DuplicateFieldNames()
        {
            Query selectQuery = db.CreateQuery(
                "select metaFields.Name, metaFields.ViewId, count(metaFields.Name) as NumOccurrences " +
                "from metaFields " +
                "group by metaFields.Name, metaFields.ViewId " +
                "having ( count(metaFields.Name) > 1 )");

            //System.Data.OleDb.OleDbDataReader reader = (System.Data.OleDb.OleDbDataReader)db.ExecuteReader(selectQuery);
            IDataReader reader = db.ExecuteReader(selectQuery);

            Dictionary<string, int> dups = new Dictionary<string, int>();

            while (reader.Read())
            {
                dups.Add((string)reader["Name"], (int)reader["ViewId"]);
            }

            return dups;
        }

        /// <summary>
        /// Insert Code table data.
        /// </summary>
        /// <param name="dataTable">Code table data.</param>
        /// <param name="tableName">Code table name.</param>
        /// <param name="columnNames">Code table name columns.</param>
        public void InsertCodeTableData(DataTable dataTable, string tableName, string[] columnNames)
        {
            Query insertQuery;
            bool reportProgress = false;
            int rowCount = 0;
            StringBuilder sb = null;
            int step = 0;

            try
            {
                sb = new StringBuilder();

                // Build the base INSERT statement. This command will be reused for each data row.
                sb.Append(SqlKeyWords.INSERT);
                sb.Append(StringLiterals.SPACE);
                sb.Append(SqlKeyWords.INTO);
                sb.Append(StringLiterals.LEFT_SQUARE_BRACKET);
                sb.Append(tableName);
                sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET);
                sb.Append(StringLiterals.PARANTHESES_OPEN);

                for (int i = 0; i < columnNames.Length; i++)
                {
                    sb.Append(StringLiterals.LEFT_SQUARE_BRACKET);
                    sb.Append(columnNames[i]);
                    sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET);

                    if (i < columnNames.Length - 1)
                    {
                        sb.Append(StringLiterals.COMMA);
                        sb.Append(StringLiterals.SPACE);
                    }
                }

                sb.Append(StringLiterals.PARANTHESES_CLOSE);
                sb.Append(StringLiterals.SPACE);
                sb.Append(SqlKeyWords.VALUES);
                sb.Append(StringLiterals.PARANTHESES_OPEN);

                for (int i = 0; i < columnNames.Length; i++)
                {
                    sb.Append(StringLiterals.COMMERCIAL_AT);
                    sb.Append(columnNames[i].Replace(" ", ""));

                    if (i < columnNames.Length - 1)
                    {
                        sb.Append(StringLiterals.COMMA);
                        sb.Append(StringLiterals.SPACE);
                    }
                }

                sb.Append(StringLiterals.PARANTHESES_CLOSE);

                insertQuery = db.CreateQuery(sb.ToString());

                // Determine if we need to report progress and what progress step to count.
                rowCount = dataTable.Rows.Count;
                step = (rowCount/10);

                // If row count is too small, skip progress reporting.
                reportProgress = (step > 0);

                if (reportProgress)
                {
                    RaiseProgressReportBeginEvent(0, rowCount, step);
                }

                for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
                {
                    DataRow row = dataTable.Rows[rowIndex];
                    insertQuery.Parameters.Clear();

                    for (int i = 0; i < columnNames.Length; i++)
                    {
                        string columnName = string.Empty; int intcolumn = 0;

                        if (row[columnNames[i]] is string)
                        {
                            columnName = (string)row[columnNames[i]];
                            string atColumnName = StringLiterals.COMMERCIAL_AT + columnNames[i].Replace(" ", "");
                            QueryParameter param = new QueryParameter(atColumnName, DbType.String, columnName);
                            insertQuery.Parameters.Add(param);
                        }
                        else if (row[columnNames[i]] is Int32)
                        {
                            intcolumn = Int32.Parse(row[columnNames[i]].ToString());
                            string atColumnName = StringLiterals.COMMERCIAL_AT + columnNames[i].Replace(" ", "");
                            QueryParameter param = new QueryParameter(atColumnName, DbType.Int32, intcolumn);
                            insertQuery.Parameters.Add(param);
                        }
                        else
                        {
                            string atColumnName = StringLiterals.COMMERCIAL_AT + columnNames[i].Replace(" ", "");
                            QueryParameter param = new QueryParameter(atColumnName, DbType.String, columnName);
                            insertQuery.Parameters.Add(param);
                        }                     
                    }

                    db.ExecuteNonQuery(insertQuery);
                    
                    if (reportProgress)
                    {
                        int rem = 0;

                        Math.DivRem(rowIndex, step, out rem);

                        if (rem.Equals(0))
                        {
                            RaiseProgressReportUpdateEvent();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not insert code into code table", ex);
            }
            finally
            {
                insertQuery = null;
                sb = null;

                if (reportProgress)
                {
                    RaiseProgressReportEndEvent();
                }
            }
        }

        /// <summary>
        /// Save Code table data.
        /// </summary>
        /// <param name="dataTable">Code table data.</param>
        /// <param name="tableName">Code table name.</param>
        /// <param name="columnNames">Code table column names.</param>
        public void SaveCodeTableData(DataTable dataTable, string tableName, string[] columnNames)
        {
            Query insertQuery;
            StringBuilder sb = null;
            Query updateQuery;

            try
            {
                sb = new StringBuilder();

                sb.Append(SqlKeyWords.INSERT);
                sb.Append(StringLiterals.SPACE);
                sb.Append(SqlKeyWords.INTO);
                sb.Append(StringLiterals.LEFT_SQUARE_BRACKET);
                sb.Append(tableName);
                sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET);
                sb.Append(StringLiterals.PARANTHESES_OPEN);

                for (int i = 0; i < columnNames.Length; i++)
                {
                    sb.Append(StringLiterals.LEFT_SQUARE_BRACKET);
                    sb.Append(columnNames[i]);
                    sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET);

                    if (i < columnNames.Length - 1)
                    {
                        sb.Append(StringLiterals.COMMA);
                        sb.Append(StringLiterals.SPACE);
                    }
                }

                sb.Append(StringLiterals.PARANTHESES_CLOSE);
                sb.Append(StringLiterals.SPACE);
                sb.Append(SqlKeyWords.VALUES);
                sb.Append(StringLiterals.PARANTHESES_OPEN);

                for (int i = 0; i < columnNames.Length; i++)
                {
                    sb.Append(StringLiterals.COMMERCIAL_AT);
                    sb.Append(columnNames[i].Replace(" ", ""));

                    if (i < columnNames.Length - 1)
                    {
                        sb.Append(StringLiterals.COMMA);
                        sb.Append(StringLiterals.SPACE);
                    }
                }

                sb.Append(StringLiterals.PARANTHESES_CLOSE);

                insertQuery = db.CreateQuery(sb.ToString());
                
                for (int i = 0; i < columnNames.Length; i++)
                {
                    QueryParameter insertParameter = new QueryParameter(StringLiterals.COMMERCIAL_AT + columnNames[i].Replace(" ", ""), DbType.String, columnNames[i], columnNames[i]);
                    insertQuery.Parameters.Add(insertParameter);
                }

                sb.Remove(0, sb.Length);


                sb.Append(SqlKeyWords.UPDATE);
                sb.Append(StringLiterals.SPACE);
                sb.Append(StringLiterals.LEFT_SQUARE_BRACKET);
                sb.Append(tableName);
                sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET);
                sb.Append(StringLiterals.SPACE);
                sb.Append(SqlKeyWords.SET);
                sb.Append(StringLiterals.SPACE);

                for (int i = 0; i < columnNames.Length; i++)
                {
                    sb.Append(StringLiterals.LEFT_SQUARE_BRACKET);
                    sb.Append(columnNames[i]);
                    sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET);
                    sb.Append(StringLiterals.SPACE);
                    sb.Append(StringLiterals.EQUAL);
                    sb.Append(StringLiterals.SPACE);
                    sb.Append(StringLiterals.COMMERCIAL_AT);
                    sb.Append(StringLiterals.NEW_VALUE);
                    sb.Append(i.ToString());

                    if (i < (columnNames.Length-1))
                    {
                        sb.Append(StringLiterals.COMMA);
                        sb.Append(StringLiterals.SPACE);
                    }
                }

                sb.Append(StringLiterals.SPACE);
                sb.Append(SqlKeyWords.WHERE);
                sb.Append(StringLiterals.SPACE);

                for (int i = 0; i < columnNames.Length; i++)
                {
                    sb.Append(StringLiterals.LEFT_SQUARE_BRACKET);
                    sb.Append(columnNames[i]);
                    sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET);
                    sb.Append(StringLiterals.SPACE);
                    sb.Append(StringLiterals.EQUAL);
                    sb.Append(StringLiterals.SPACE);
                    sb.Append(StringLiterals.COMMERCIAL_AT);
                    sb.Append(StringLiterals.OLD_VALUE);
                    sb.Append(i.ToString());

                    if (i < (columnNames.Length - 1))
                    {
                        sb.Append(StringLiterals.SPACE);
                        sb.Append(SqlKeyWords.AND);
                        sb.Append(StringLiterals.SPACE);
                    }
                }
                
                updateQuery = db.CreateQuery(sb.ToString());

                for (int i = 0; i < columnNames.Length; i++)
                {
                    updateQuery.Parameters.Add(new QueryParameter(StringLiterals.COMMERCIAL_AT + StringLiterals.NEW_VALUE + i.ToString(), DbType.String, columnNames[i], columnNames[i]));
                    updateQuery.Parameters.Add(new QueryParameter(StringLiterals.COMMERCIAL_AT + StringLiterals.OLD_VALUE + i.ToString(), DbType.String, columnNames[i], columnNames[i]));
                    updateQuery.Parameters[1].SourceVersion = DataRowVersion.Original;
                }

                db.Update(dataTable, tableName, insertQuery, updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not save data for code table", ex.Message + " " + ex.StackTrace);
            }
            finally
            {
                updateQuery = null;
                insertQuery = null;
                sb = null;
            }
        }

        public bool TableExists(string tableName)
        {
            return db.TableExists(tableName);
        }

        /// <summary>
        /// Create a code table in the metadata database
        /// </summary>
        /// <param name="prefixedTableName">Name of the code table</param>
        /// <param name="columnNames">Column names of the code table</param>
        /// <returns>A value indicating whether or not the code table was created</returns>
        public bool CreateCodeTable(string tableName, string[] columnNames)
        {
            //bool Retval = false;

            //Query createQuery;
            //StringBuilder sb = null;

            //string prefixedTableName;
            //if (db.GetType().ToString().Contains("SqlServer"))
            //{
            //    prefixedTableName = "DBO." + tableName;
            //}
            //else
            //{
            //    prefixedTableName = tableName;
            //}

            //try
            //{
                
            //    if (db.TableExists(prefixedTableName.ToLowerInvariant()))
            //    {
            //        // Needed so that code tables are not constantly appended if you import over
            //        // an existing project with the same code table names; may also cause problems
            //        // if importing over an existing database with the same code table names but
            //        // different code table structure.
            //        db.DeleteTable(prefixedTableName.ToLowerInvariant());
            //    }
            //        sb = new StringBuilder();

            //        sb.Append(SqlKeyWords.CREATE);
            //        sb.Append(StringLiterals.SPACE);
            //        sb.Append(SqlKeyWords.TABLE);
            //        sb.Append(StringLiterals.SPACE);
            //        sb.Append(StringLiterals.LEFT_SQUARE_BRACKET);
            //        sb.Append(prefixedTableName.ToLowerInvariant());
            //        sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET);
            //        sb.Append(StringLiterals.SPACE);
            //        sb.Append(StringLiterals.PARANTHESES_OPEN);

            //        for (int i = 0; i < columnNames.Length; i++)
            //        {
            //            sb.Append(StringLiterals.LEFT_SQUARE_BRACKET);
            //            sb.Append(columnNames[i]);
            //            sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET);
            //            sb.Append(StringLiterals.SPACE);
            //            sb.Append(SqlKeyWords.VARCHAR);
            //            sb.Append(StringLiterals.PARANTHESES_OPEN);
            //            sb.Append(StringLiterals.NUMBER_255);
            //            sb.Append(StringLiterals.PARANTHESES_CLOSE);
                       
            //            if (i < columnNames.Length - 1)
            //            {
            //                sb.Append(StringLiterals.COMMA);
            //                sb.Append(StringLiterals.SPACE);
            //            }
            //        }
            //        sb.Append(StringLiterals.PARANTHESES_CLOSE);
            //        string create = sb.ToString();
            //        createQuery = db.CreateQuery(create);

            //        db.ExecuteNonQuery(createQuery);

            //        Retval = true;
                
            //}
            //catch (Exception ex)
            //{
            //    throw new GeneralException("Could not create code table in the database", ex.StackTrace);
            //}
            //finally
            //{
            //    createQuery = null;
            //    sb = null;
            //}

            bool Retval = false;

            try
            {
                if (!db.TableExists(tableName.ToLowerInvariant()))
                {
                    List<TableColumn> columns = new List<TableColumn>();
                    foreach (string columnName in columnNames)
                    {
                        columns.Add(new TableColumn(columnName, GenericDbColumnType.String, 255, false));
                    }
                    db.CreateTable(tableName.ToLowerInvariant(), columns);
                }
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create code table in the database", ex.StackTrace);
            }
            return Retval;
        }

        /// <summary>
        /// Create a code table in the metadata database
        /// </summary>
        /// <param name="tableName">Name of the code table</param>
        /// <param name="columnName">Column name of the code table</param>
        public void CreateCodeTable(string tableName, string columnName)
        {
            try
            {
                if (!db.TableExists(tableName.ToLowerInvariant()))
                {
                    List<TableColumn> columns = new List<TableColumn>();
                    columns.Add(new TableColumn(columnName, GenericDbColumnType.String, 255, false));
                    db.CreateTable(tableName.ToLowerInvariant(), columns);
                }
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create code table in the database", ex.StackTrace);
            }
        }

        /// <summary>
        /// Inserts a program into metaPrograms
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <param name="comment"></param>
        /// <param name="author"></param>
        /// <returns></returns>
        public virtual void InsertPgm(string name, string content, string comment, string author)
        {
            try
            {
                Query insertQuery = db.CreateQuery("insert into metaPrograms([Name], [Content], [Comment],[DateCreated],[DateModified], [Author]) " +
                    "values (@Name, @Content, @Comment,@DateCreated,@DateModified,  @Author)");

                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, name));
                insertQuery.Parameters.Add(new QueryParameter("@Content", DbType.String, content));
                insertQuery.Parameters.Add(new QueryParameter("@Comment", DbType.String, comment));
                insertQuery.Parameters.Add(new QueryParameter("@DateCreated", DbType.String, System.DateTime.Now.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@DateModified", DbType.String, System.DateTime.Now.ToString()));
                insertQuery.Parameters.Add(new QueryParameter("@Author", DbType.String, author));

                db.ExecuteNonQuery(insertQuery);

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not insert programs", ex);
            }
            finally
            {

            }
        }
        #endregion

        #region Delete Statements
        /// <summary>
        /// Delete a View by Name from current database
        /// </summary>
        /// <param name="viewName"></param>
        public void DeleteView(string viewName)
        {
            try
            {
                #region Input Validation
                if (viewName.Trim().Length == 0)
                {
                    throw new NullReferenceException(SharedStrings.WARING_VIEW_NAME_MISSING);
                }
                #endregion

                Query query = db.CreateQuery("delete from metaViews where [Name] = '" + viewName + "'");

                db.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException(SharedStrings.WARNING_DELETE_VIEW_ERROR, ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Deletes a page from the database
        /// </summary>
        /// <param name="page">Id of the page</param>
        public void DeletePage(Page page)
        {
            try
            {
                #region Input Validation
                if (page.Id < 1)
                {
                    throw new ArgumentOutOfRangeException("PageId");
                }
                #endregion

                Query query = db.CreateQuery("delete from metaPages where [PageId] = @pageId");
                query.Parameters.Add(new QueryParameter("@pageId", DbType.Int32, page.Id));

                db.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException(Epi.SharedStrings.WARNING_CANNOT_DELETE_THIS_PAGE, ex);
            }
        }

        /// <summary>
        /// Deletes a field from the database
        /// </summary>
        /// <param name="field">A field object</param>
        public void DeleteField(string fieldName, int viewId)
        {
            try
            {
                Query query = db.CreateQuery("delete from metaFields where [Name] = @name and [ViewId] = @viewId");
                query.Parameters.Add(new QueryParameter("@name", DbType.String, fieldName));
                query.Parameters.Add(new QueryParameter("@viewId", DbType.Int32, viewId));
                db.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not delete field in the database", ex);
            }
        }

        /// <summary>
        /// Deletes a field from the database
        /// </summary>
        /// <param name="field">A field object</param>
        public void DeleteField(Field field)
        {
            try
            {
                Query query = db.CreateQuery("delete from metaFields where [UniqueId] = @uniqueId");
                query.Parameters.Add(new QueryParameter("@uniqueId", DbType.Guid, field.UniqueId));
                db.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not delete field in the database", ex);
            }
        }

        /// <summary>
        /// DeleteCodeTable
        /// </summary>
        /// <param name="tableName"></param>
        public void DeleteCodeTable(string tableName)
        {
            try
            {
                Query query = db.CreateQuery("DROP TABLE " + tableName);
                db.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not delete the code table from database", ex);
            }
        }

        /// <summary>
        /// Deletes a grid column from the database.
        /// </summary>
        /// <param name="column">A grid column.</param>
        public void DeleteGridColumn(GridColumnBase column)
        {
            try
            {
                Query query = db.CreateQuery("delete from metaGridColumns where [GridColumnId] = @fieldId and [Name] = @Name");
                query.Parameters.Add(new QueryParameter("@fieldId", DbType.Int32, column.Id));
                query.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));

                db.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not delete grid column in the database.", ex);
            }
        }

        /// <summary>
        /// Deletes fields on a page from the database
        /// </summary>
        /// <param name="page"></param>
        public void DeleteFields(Page page)
        {
            try
            {
                Query query = db.CreateQuery("delete from metaFields where [PageId] = @pageId");
                query.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, page.Id));

                db.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not delete fields in the database", ex);
            }
        }

        /// <summary>
        /// Deletes a program from the database
        /// </summary>
        /// <param name="pgmId">Id of the program to be deleted</param>
        public void DeletePgm(int pgmId)
        {
            try
            {
                #region Input Validation
                if (pgmId < 1)
                {
                    throw new ArgumentOutOfRangeException("ProgramId");
                }
                #endregion
                Query deleteQuery = db.CreateQuery("delete  from [metaPrograms] where [ProgramId] = @PgmId");
                deleteQuery.Parameters.Add(new QueryParameter("@PgmId", DbType.Int32, pgmId));
                db.ExecuteNonQuery(deleteQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not delete pgm.", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Deletes a program from the database
        /// </summary>
        /// <param name="pgmName">Name of the program to be deleted</param>
        public virtual void DeletePgm(string pgmName)
        {
            throw new System.ApplicationException("Not implemented");
        }

        #endregion Delete Statements

        #region Update Statements

        /// <summary>
        /// Updates the fields tab information
        /// </summary>
        /// <param name="FieldsTabOrder">A sorted table containing newly assigned tab order indices</param>
        /// <param name="view">The fields's view</param>
        /// <param name="currentPage">The field's page</param>
        public void UpdateTabOrder(DataTable FieldsTabOrder, View view, Page currentPage)
        {
            foreach (DataRow row in FieldsTabOrder.Rows)
            {
                Query updateQuery = db.CreateQuery("update metaFields set [TabIndex] = @TabIndex, [HasTabStop] = @HasTabStop where [FieldId] = @FieldId and [ViewId] = @ViewId and [PageId] = @PageId");
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, row["TabIndex"]));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Int32, row["HasTabStop"]));
                updateQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, row["FieldId"]));
                updateQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, view.Id));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, currentPage.Id));
                db.ExecuteNonQuery(updateQuery);
            }
        }

        /// <summary>
        /// Resets the tab indexes for fields on a page
        /// </summary>
        /// <param name="page">A page object</param>
        public void ResetTabIndexes(Page page)
        {
            try
            {
                Query query = db.CreateQuery("select [FieldId], [ControlTopPositionPercentage], [ControlLeftPositionPercentage] from metaFields where [PageId] = @PageId order by [ControlTopPositionPercentage], [ControlLeftPositionPercentage]");
                query.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, page.Id));
                DataTable table = db.Select(query);
                int tabIndex = 0;

                foreach (DataRow row in table.Rows)
                {
                    tabIndex++;
                    Query updateQuery = db.CreateQuery("update metaFields set [TabIndex] = @TabIndex where [FieldId] = @FieldId");
                    updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, tabIndex));
                    updateQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, (int)row[ColumnNames.FIELD_ID]));
                    db.ExecuteNonQuery(updateQuery);
                }
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not reset tab indexes", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Updates the prompt position
        /// </summary>
        /// <param name="field">Field prompt to update.</param>
        public void UpdatePromptPosition(FieldWithSeparatePrompt field)
        {
            try
            {
                Query query = db.CreateQuery("update metaFields set [PromptLeftPositionPercentage] = @LeftPosition, [PromptTopPositionPercentage] = @TopPosition where [UniqueId] = @UniqueId");
                query.Parameters.Add(new QueryParameter("@LeftPosition", DbType.Double, field.PromptLeftPositionPercentage));
                query.Parameters.Add(new QueryParameter("@TopPosition", DbType.Double, field.PromptTopPositionPercentage));
                query.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update prompt position", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Updates the control position
        /// </summary>
        /// <param name="field">the field</param>
        // public void UpdateControlPosition(int fieldId, double xCoordinate, double yCoordinate)
        public void UpdateControlPosition(RenderableField field) //int fieldId, double xCoordinate, double yCoordinate)
        {
            try
            {
                Query query = db.CreateQuery("update metaFields set [ControlLeftPositionPercentage] = @LeftPosition, [ControlTopPositionPercentage] = @TopPosition where [UniqueId] = @UniqueId");
                query.Parameters.Add(new QueryParameter("@LeftPosition", DbType.Double, field.ControlLeftPositionPercentage));
                query.Parameters.Add(new QueryParameter("@TopPosition", DbType.Double, field.ControlTopPositionPercentage));
                query.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update control position", ex);
            }
        }

        /// <summary>
        /// Updates the control size
        /// </summary>
        /// <param name="field">field</param>
        public void UpdateControlSize(RenderableField field)
        {
            try
            {
                Query query = db.CreateQuery("update metaFields set [ControlHeightPercentage] = @Height, [ControlWidthPercentage] = @Width where [FieldId] = @FieldId"); 
                query.Parameters.Add(new QueryParameter("@Height", DbType.Double, field.ControlHeightPercentage));
                query.Parameters.Add(new QueryParameter("@Width", DbType.Double, field.ControlWidthPercentage));
                query.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, field.Id));
                db.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update control size", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Change the field type by only changing the FieldTypeId in metaFields
        /// </summary>
        /// <param name="field">Epi.Fields.Field to be changed</param>
        /// <param name="fieldType">the fields new MetaFieldType</param>
        public void UpdateFieldType(Epi.Fields.Field field, Epi.MetaFieldType fieldType)
        {
            try
            {
                Query query = db.CreateQuery("update metaFields set [FieldTypeId] = @FieldTypeId where [UniqueId] = @UniqueId");
                query.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, fieldType));
                query.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));
                db.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update field type", ex);
            }
        }

        /// <summary>
        /// Update the data table name in the MetaFields table
        /// </summary>
        /// <param name="viewId"></param>
        /// <param name="tableName"></param>
        public void UpdateDataTableName(int viewId, string tableName)
        {
            try
            {
                Query query = db.CreateQuery("update metaFields set [DataTableName] = @tableName where [ViewId] = @viewId");
                query.Parameters.Add(new QueryParameter("@tableName", DbType.String, tableName));
                query.Parameters.Add(new QueryParameter("@viewId", DbType.Int32, viewId));

                db.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update data table name", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Changes the name of a page
        /// </summary>
        /// <param name="pageId">Id of the page</param>
        /// <param name="newPageName">New name of the page</param>
        public void RenamePage(int pageId, string newPageName)
        {
            try
            {
                #region Input Validation
                if (string.IsNullOrEmpty(newPageName))
                {
                    throw new ArgumentNullException("NewPageName");
                }
                if (pageId < 1)
                {
                    throw new ArgumentOutOfRangeException("PageId");
                }
                #endregion

                Query query = db.CreateQuery("update metaPages set [Name] = @newPageName where [PageId] = @pageId");
                query.Parameters.Add(new QueryParameter("@newPageName", DbType.String, newPageName));
                query.Parameters.Add(new QueryParameter("@pageId", DbType.Int32, pageId));

                db.ExecuteNonQuery(query);

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not rename page in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Updates the page position in the metaPage data table when a page is inserted
        /// </summary>
        /// <param name="view">The View to be synchronized</param>
        /// <param name="page">The Page that was just inserted</param>	
        public void SynchronizePageNumbersOnInsert(View view, Page page)
        {
            try
            {
                #region Input Validation
                if (page == null)
                {
                    throw new ArgumentNullException("Page");
                }
                if (view == null)
                {
                    throw new ArgumentNullException("View");
                }
                #endregion

                Query query = db.CreateQuery("update metaPages set [Position] = [Position] + 1 where [ViewId] = @viewId and [Position] >= @position");
                query.Parameters.Add(new QueryParameter("@viewId", DbType.Int32, view.Id));
                query.Parameters.Add(new QueryParameter("@position", DbType.Int32, page.Position));

                db.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update page position to insert a page in the database", ex);
            }
        }

        /// <summary>
        /// Updates the page position in the metaPage data table when a page is deleted
        /// </summary>
        /// <param name="view">The View to be synchronized</param>
        /// <param name="position">Position of the newly inserted page</param>		
        public void SynchronizePageNumbersOnDelete(View view, int position)
        {
            try
            {
                #region Input Validation
                if (position < 0)
                {
                    throw new ArgumentOutOfRangeException("Position");
                }
                if (view == null)
                {
                    throw new ArgumentNullException("View");
                }
                #endregion

                Query query = db.CreateQuery("update metaPages set [Position] = [Position] - 1 where [ViewId] = @viewID and [Position] >= @position");
                query.Parameters.Add(new QueryParameter("@viewID", DbType.Int32, view.Id));
                query.Parameters.Add(new QueryParameter("@position", DbType.Int32, position));

                db.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update page position to delete a page in the database", ex);
            }
        }

        /// <summary>
        /// Creates a relationship between a field and a child view
        /// </summary>
        /// <param name="fieldId">Id of the field</param>
        /// <param name="relatedViewId">Id of the view the field is related to</param>
        /// <param name="relateCondition">Conditions to show the related view</param>
        /// <param name="shouldReturnToParent">Whether or not the related view returns to parent</param>
        public void RelateFieldToView(Guid uniqueId, int relatedViewId, string relateCondition, bool shouldReturnToParent)
        {
            try
            {
                Query fieldQuery = db.CreateQuery("update metaFields set [RelatedViewId] = @RelatedViewId, [RelateCondition] = @RelateCondition, [ShouldReturnToparent] = @ShouldReturnToParent where UniqueId = @UniqueId");
                fieldQuery.Parameters.Add(new QueryParameter("@RelatedViewId", DbType.Int32, relatedViewId));
                fieldQuery.Parameters.Add(new QueryParameter("@RelateCondition", DbType.String, relateCondition));
                fieldQuery.Parameters.Add(new QueryParameter("@ShouldReturnToParent", DbType.Boolean, shouldReturnToParent));
                fieldQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, uniqueId));

                Query viewQuery = db.CreateQuery("update metaViews set [IsRelatedView] = @IsRelatedView where [ViewId] = @ViewId");
                viewQuery.Parameters.Add(new QueryParameter("@IsRelatedView", DbType.Boolean, true));
                viewQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, relatedViewId));

                db.ExecuteNonQuery(fieldQuery);
                db.ExecuteNonQuery(viewQuery);

                if (relatedViewId != 0)
                {
                    Epi.View view = this.GetViewById(relatedViewId);
                    ForeignKeyField foreignKeyField;
                    if (view.IsRelatedView)
                    {
                        try
                        {
                            foreignKeyField = (ForeignKeyField) view.Fields["FKEY"];
                        }
                        catch (System.Exception ex)
                        {
                            if (ex != null) // this line is to avoid compiler warning;
                            {
                                foreignKeyField = new ForeignKeyField(view);
                                foreignKeyField.SaveToDb();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create new relate info", ex);
            }
        }

        /// <summary>
        /// Update Checkbox field.
        /// </summary>
        /// <param name="field">Checkbox field to update.</param>
        public void UpdateField(CheckBoxField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("CheckBoxField");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaFields set [CheckCodeAfter] = @CheckCodeAfter, [ControlFontFamily] = @ControlFontFamily, " +
                    "[ControlFontStyle] = @ControlFontStyle, [ControlFontSize] = @ControlFontSize, [ControlHeightPercentage] = @ControlHeightPercentage, " +
                    "[ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, [ControlTopPositionPercentage] = @ControlTopPositionPercentage, [ControlWidthPercentage] = @ControlWidthPercentage, " +
                    "[HasTabStop] = @HasTabStop, [IsReadOnly] = @IsReadOnly, [IsRequired] = @IsRequired, [Name] = @Name, [PageId] = @PageId, [Pattern] = @Pattern, " +
                    "[PromptFontFamily] = @PromptFontFamily, [PromptFontStyle] = @PromptFontStyle, [PromptFontSize] = @PromptFontSize, " +
                    "[PromptText] = @PromptText, " +
                    "[ShouldRepeatLast] = @ShouldRepeatLast, [TabIndex] = @TabIndex, [FieldTypeId] = @FieldTypeId where [UniqueId] = @UniqueId");

                if (field.CheckCodeAfter == null)
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));

                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                updateQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, (field.BoxOnRight ? "BoxOnRight" : "")));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                updateQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update CheckBoxField in the database", ex);
            }
        }
     
        /// <summary>
        /// Update Command button field.
        /// </summary>
        /// <param name="field">Command button field to update.</param>
        public void UpdateField(CommandButtonField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("CommandButtonField");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaFields set [CheckCodeAfter] = @CheckCodeAfter, [ControlFontFamily] = @ControlFontFamily, " +
                    "[ControlFontStyle] = @ControlFontStyle, [ControlFontSize] = @ControlFontSize, [ControlHeightPercentage] = @ControlHeightPercentage, " +
                    "[ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, [ControlTopPositionPercentage] = @ControlTopPositionPercentage, " +
                    "[ControlWidthPercentage] = @ControlWidthPercentage, [FieldTypeId] = @FieldTypeId, [HasTabStop] = @HasTabStop, [Name] = @Name, " +
                    "[PageId] = @PageId, [PromptText] = @PromptText, [TabIndex] = @TabIndex where [UniqueId] = @UniqueId");

                updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeClick));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                updateQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                updateQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update CommandButtonField in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Update Date field.
        /// </summary>
        /// <param name="field">Date field to update.</param>
        public void UpdateField(DateField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("DateField");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaFields set [CheckCodeAfter] = @CheckCodeAfter, " +
                    "[CheckCodeBefore] = @CheckCodeBefore, [ControlFontFamily] = @ControlFontFamily, [ControlFontStyle] = @ControlFontStyle, " +
                    "[ControlFontSize] = @ControlFontSize, [ControlHeightPercentage] = @ControlHeightPercentage, " +
                    "[ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, [ControlTopPositionPercentage] = @ControlTopPositionPercentage, " +
                    "[ControlWidthPercentage] = @ControlWidthPercentage, [FieldTypeId] = @FieldTypeId, [HasTabStop] = @HasTabStop, " +
                    "[IsReadOnly] = @IsReadOnly, [IsRequired] = @IsRequired, [Lower] = @Lower, [Name] = @Name, [PageId] = @PageId, " +
                    "[Pattern] = @Pattern, [PromptFontFamily] = @PromptFontFamily, [PromptFontStyle] = @PromptFontStyle, " +
                    "[PromptFontSize] = @PromptFontSize, [PromptLeftPositionPercentage] = @PromptLeftPositionPercentage, " +
                    "[PromptText] = @PromptText, [PromptTopPositionPercentage] = @PromptTopPositionPercentage, " +
                    "[ShouldRepeatLast] = @ShouldRepeatLast, [TabIndex] = @TabIndex, [Upper] = @Upper where [UniqueId] = @UniqueId");

                if (field.CheckCodeAfter == null)
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));

                if (field.CheckCodeBefore == null)
                    updateQuery.Parameters.Add(new QueryParameter(ColumnNames.CHECK_CODE_BEFORE, DbType.String, DBNull.Value));
                else
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeBefore));

                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                if (field.Lower == null)
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Lower", DbType.String, DBNull.Value));
                }
                else
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Lower", DbType.String, field.Lower));
                }
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                if (field.Pattern == null)
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, DBNull.Value));
                }
                else
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, field.Pattern));
                }
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                if (field.Upper == null)
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Upper", DbType.String, DBNull.Value));
                }
                else
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Upper", DbType.String, field.Upper));
                }
                updateQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update DateField in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Update Date-time field.
        /// </summary>
        /// <param name="field">Date-time field to update.</param>
        public void UpdateField(DateTimeField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("DateTimeField");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaFields set [CheckCodeAfter] = @CheckCodeAfter, [CheckCodeBefore] = @CheckCodeBefore, " +
                    "[ControlFontFamily] = @ControlFontFamily, [ControlFontStyle] = @ControlFontStyle, [ControlFontSize] = @ControlFontSize, " +
                    "[ControlHeightPercentage] = @ControlHeightPercentage, [ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, " +
                    "[ControlTopPositionPercentage] = @ControlTopPositionPercentage, [ControlWidthPercentage] = @ControlWidthPercentage, " +
                    "[FieldTypeId] = @FieldTypeId, [HasTabStop] = @HasTabStop, [IsReadOnly] = @IsReadOnly, [IsRequired] = @IsRequired, " +
                    "[Name] = @Name, [PageID] = @PageId, [PromptFontFamily] = @PromptFontFamily, " +
                    "[PromptFontStyle] = @PromptFontStyle, [PromptFontSize] = @PromptFontSize, [PromptLeftPositionPercentage] = @PromptLeftPositionPercentage, " +
                    "[PromptText] = @PromptText, [PromptTopPositionPercentage] = @PromptTopPositionPercentage, [ShouldRepeatLast] = @ShouldRepeatLast, " +
                    "[TabIndex] = @TabIndex where [UniqueId] = @UniqueId");

                if (field.CheckCodeAfter == null)
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));

                if (field.CheckCodeBefore == null)
                    updateQuery.Parameters.Add(new QueryParameter(ColumnNames.CHECK_CODE_BEFORE, DbType.String, DBNull.Value));
                else
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeBefore));

                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                updateQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update DateTimeField in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Update Codes dropdown list field.
        /// </summary>
        /// <param name="field">Codes dropdown list field to update.</param>
        public void UpdateField(DDLFieldOfCodes field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("DDLFieldOfCodes");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaFields set [CodeColumnName] = @CodeColumnName, [TextColumnName] = @TextColumnName, " +
                    "[SourceTableName] = @SourceTableName, [ControlFontFamily] = @ControlFontFamily, [ControlFontStyle] = @ControlFontStyle, " +
                    "[ControlFontSize] = @ControlFontSize, [ControlHeightPercentage] = @ControlHeightPercentage, [ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, " +
                    "[ControlTopPositionPercentage] = @ControlTopPositionPercentage, [ControlWidthPercentage] = @ControlWidthPercentage, " +
                    "[FieldTypeId] = @FieldTypeId, [HasTabStop] = @HasTabStop, [IsExclusiveTable] = @IsExclusiveTable, [IsReadOnly] = @IsReadOnly, " +
                    "[IsRequired] = @IsRequired, [Name] = @Name, [PageId] = @PageId, [PromptFontFamily] = @PromptFontFamily, [PromptFontStyle] = @PromptFontStyle, " +
                    "[PromptFontSize] = @PromptFontSize, [PromptLeftPositionPercentage] = @PromptLeftPositionPercentage, [PromptText] = @PromptText, " +
                    "[PromptTopPositionPercentage] = @PromptTopPositionPercentage, [RelateCondition] = @RelateCondition, [ShouldRepeatLast] = @ShouldRepeatLast, [Sort] = @Sort, [TabIndex] = @TabIndex " +
                    "where [UniqueId] = @UniqueId");

                updateQuery.Parameters.Add(new QueryParameter("@CodeColumnName", DbType.String, field.CodeColumnName));
                updateQuery.Parameters.Add(new QueryParameter("@TextColumnName", DbType.String, field.TextColumnName));
                updateQuery.Parameters.Add(new QueryParameter("@SourceTableName", DbType.String, field.SourceTableName));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new QueryParameter("@IsExclusiveTable", DbType.Boolean, field.IsExclusiveTable));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@RelateCondition", DbType.String, field.RelateConditionString));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@Sort", DbType.Boolean, field.ShouldSort));
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                updateQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update DDLFieldOfCodes in the database", ex);
            }
            finally
            {

            }
        }


        /// <summary>
        /// Update Codes dropdown list field.
        /// </summary>
        /// <param name="field">Codes dropdown list field to update.</param>
        public void UpdateField(DDListField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("DDLFieldOfCodes");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaFields set [CodeColumnName] = @CodeColumnName, [TextColumnName] = @TextColumnName, " +
                    "[SourceTableName] = @SourceTableName, [ControlFontFamily] = @ControlFontFamily, [ControlFontStyle] = @ControlFontStyle, " +
                    "[ControlFontSize] = @ControlFontSize, [ControlHeightPercentage] = @ControlHeightPercentage, [ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, " +
                    "[ControlTopPositionPercentage] = @ControlTopPositionPercentage, [ControlWidthPercentage] = @ControlWidthPercentage, " +
                    "[FieldTypeId] = @FieldTypeId, [HasTabStop] = @HasTabStop, [IsExclusiveTable] = @IsExclusiveTable, [IsReadOnly] = @IsReadOnly, " +
                    "[IsRequired] = @IsRequired, [Name] = @Name, [PageId] = @PageId, [PromptFontFamily] = @PromptFontFamily, [PromptFontStyle] = @PromptFontStyle, " +
                    "[PromptFontSize] = @PromptFontSize, [PromptLeftPositionPercentage] = @PromptLeftPositionPercentage, [PromptText] = @PromptText, " +
                    "[PromptTopPositionPercentage] = @PromptTopPositionPercentage, [RelateCondition] = @RelateCondition, [ShouldRepeatLast] = @ShouldRepeatLast, [TabIndex] = @TabIndex " +
                    "where [UniqueId] = @UniqueId");

                updateQuery.Parameters.Add(new QueryParameter("@CodeColumnName", DbType.String, field.CodeColumnName));
                updateQuery.Parameters.Add(new QueryParameter("@TextColumnName", DbType.String, field.TextColumnName));
                updateQuery.Parameters.Add(new QueryParameter("@SourceTableName", DbType.String, field.SourceTableName));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new QueryParameter("@IsExclusiveTable", DbType.Boolean, field.IsExclusiveTable));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@RelateCondition", DbType.String, field.AssociatedFieldInformation));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                updateQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update DDLFieldOfCodes in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Update Comment legal dropdown list field.
        /// </summary>
        /// <param name="field">Comment legal dropdown list field to update.</param>
        public void UpdateField(DDLFieldOfCommentLegal field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("DDLFieldOfCommentLegal");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaFields set [TextColumnName] = @TextColumnName, [SourceTableName] = @SourceTableName, " +
                    "[ControlFontFamily] = @ControlFontFamily, [ControlFontStyle] = @ControlFontStyle, [ControlFontSize] = @ControlFontSize, " +
                    "[ControlHeightPercentage] = @ControlHeightPercentage, [ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, " +
                    "[ControlTopPositionPercentage] = @ControlTopPositionPercentage, [ControlWidthPercentage] = @ControlWidthPercentage, " +
                    "[FieldTypeId] = @FieldTypeId, [HasTabStop] = @HasTabStop, [IsExclusiveTable] = @IsExclusiveTable, [IsReadOnly] = @IsReadOnly, " +
                    "[IsRequired] = @IsRequired, [Name] = @Name, [PageId] = @PageId, [PromptFontFamily] = @PromptFontFamily, [PromptFontStyle] = @PromptFontStyle, " +
                    "[PromptFontSize] = @PromptFontSize, [PromptLeftPositionPercentage] = @PromptLeftPositionPercentage, [PromptText] = @PromptText, " +
                    "[PromptTopPositionPercentage] = @PromptTopPositionPercentage, [ShouldRepeatLast] = @ShouldRepeatLast, [Sort] = @Sort, [TabIndex] = @TabIndex " +
                    "where [UniqueId] = @UniqueId");

                updateQuery.Parameters.Add(new QueryParameter("@TextColumnName", DbType.String, field.TextColumnName));
                updateQuery.Parameters.Add(new QueryParameter("@SourceTableName", DbType.String, field.SourceTableName));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new QueryParameter("@IsExclusiveTable", DbType.Boolean, field.IsExclusiveTable));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@Sort", DbType.Boolean, field.ShouldSort));
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                updateQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update DDLFieldOfCommentLegal in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Update Legal values dropdown list field.
        /// </summary>
        /// <param name="field">Legal values dropdown list field to update.</param>
        public void UpdateField(DDLFieldOfLegalValues field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("DDLFieldOfLegalValues");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaFields set [CodeColumnName] = @CodeColumnName, [TextColumnName] = @TextColumnName, [SourceTableName] = @SourceTableName, " +
                    "[ControlFontFamily] = @ControlFontFamily, [ControlFontStyle] = @ControlFontStyle, [ControlFontSize] = @ControlFontSize, " +
                    "[ControlHeightPercentage] = @ControlHeightPercentage, [ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, " +
                    "[ControlTopPositionPercentage] = @ControlTopPositionPercentage, [ControlWidthPercentage] = @ControlWidthPercentage, " +
                    "[FieldTypeId] = @FieldTypeId, [HasTabStop] = @HasTabStop, [IsExclusiveTable] = @IsExclusiveTable, [IsReadOnly] = @IsReadOnly, " +
                    "[IsRequired] = @IsRequired, [Name] = @Name, [PageId] = @PageId, [PromptFontFamily] = @PromptFontFamily, " +
                    "[PromptFontStyle] = @PromptFontStyle, [PromptFontSize] = @PromptFontSize, [PromptLeftPositionPercentage] = @PromptLeftPositionPercentage, " +
                    "[PromptText] = @PromptText, [PromptTopPositionPercentage] = @PromptTopPositionPercentage, [ShouldRepeatLast] = @ShouldRepeatLast, " +
                    "[TabIndex] = @TabIndex , [Sort] = @Sort where [UniqueId] = @UniqueId");

                updateQuery.Parameters.Add(new QueryParameter("@CodeColumnName", DbType.String, field.CodeColumnName));
                updateQuery.Parameters.Add(new QueryParameter("@TextColumnName", DbType.String, field.TextColumnName));
                updateQuery.Parameters.Add(new QueryParameter("@SourceTableName", DbType.String, field.SourceTableName));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new QueryParameter("@IsExclusiveTable", DbType.Boolean, field.IsExclusiveTable));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                updateQuery.Parameters.Add(new QueryParameter("@Sort", DbType.Boolean, field.ShouldSort));
                updateQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update DDLFieldOfLegalValues in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Update Grid field.
        /// </summary>
        /// <param name="field">Grid field to update.</param>
        public void UpdateField(GridField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("GridField");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaFields set [ControlFontFamily] = @ControlFontFamily, " +
                    "[ControlFontStyle] = @ControlFontStyle, [ControlFontSize] = @ControlFontSize, [ControlHeightPercentage] = @ControlHeightPercentage, " +
                    "[ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, [ControlTopPositionPercentage] = @ControlTopPositionPercentage, " +
                    "[ControlWidthPercentage] = @ControlWidthPercentage, [FieldTypeId] = @FieldTypeId, [HasTabStop] = @HasTabStop, [Name] = @Name, " +
                    "[PageId] = @PageId, [PromptFontFamily] = @PromptFontFamily, [PromptFontStyle] = @PromptFontStyle, [PromptFontSize] = @PromptFontSize, " +
                    "[PromptLeftPositionPercentage] = @PromptLeftPositionPercentage, " +
                    "[PromptTopPositionPercentage] = @PromptTopPositionPercentage, " +
                    "[PromptText] = @PromptText, " +
                    "[TabIndex] = @TabIndex where [UniqueId] = @UniqueId");

                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));                
                updateQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                updateQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update GridField in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Update GUID field.
        /// </summary>
        /// <param name="field">GUID field to update.</param>
        public void UpdateField(GUIDField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("GUIDField");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaFields set [CheckCodeAfter] = @CheckCodeAfter, [CheckCodeBefore] = @CheckCodeBefore, " +
                    "[ControlFontFamily] = @ControlFontFamily, [ControlFontStyle] = @ControlFontStyle, [ControlFontSize] = @ControlFontSize, " +
                    "[ControlHeightPercentage] = @ControlHeightPercentage, [ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, " +
                    "[ControlTopPositionPercentage] = @ControlTopPositionPercentage, [ControlWidthPercentage] = @ControlWidthPercentage, " +
                    "[FieldTypeId] = @FieldTypeId, [HasTabStop] = @HasTabStop, [IsReadOnly] = @IsReadOnly, [IsRequired] = @IsRequired, " +
                    "[MaxLength] = @MaxLength, [Name] = @Name, [PageId] = @PageId, [PromptFontFamily] = @PromptFontFamily, " +
                    "[PromptFontStyle] = @PromptFontStyle, [PromptFontSize] = @PromptFontSize, [PromptLeftPositionPercentage] = @PromptLeftPositionPercentage, " +
                    "[PromptText] = @PromptText, [PromptTopPositionPercentage] = @PromptTopPositionPercentage, [ShouldRepeatLast] = @ShouldRepeatLast, " +
                    "[TabIndex] = @TabIndex where [UniqueId] = @UniqueId");

                if (field.CheckCodeAfter == null)
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));

                if (field.CheckCodeBefore == null)
                    updateQuery.Parameters.Add(new QueryParameter(ColumnNames.CHECK_CODE_BEFORE, DbType.String, DBNull.Value));
                else
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeBefore));

                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                updateQuery.Parameters.Add(new QueryParameter("@MaxLength", DbType.Int16, field.MaxLength));
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                updateQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(updateQuery);

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update GUIDField in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Update Image field.
        /// </summary>
        /// <param name="field">Image field to update.</param>
        public void UpdateField(ImageField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("ImageField");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaFields set [ControlFontFamily] = @ControlFontFamily, [ControlFontStyle] = @ControlFontStyle, " +
                    "[ControlFontSize] = @ControlFontSize, [ControlHeightPercentage] = @ControlHeightPercentage, [ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, " +
                    "[ControlTopPositionPercentage] = @ControlTopPositionPercentage, [ControlWidthPercentage] = @ControlWidthPercentage, [HasTabStop] = @HasTabStop, " +
                    "[IsReadOnly] = @IsReadOnly, [IsRequired] = @IsRequired, [Name] = @Name, [PageId] = @PageId, [PromptFontFamily] = @PromptFontFamily, " +
                    "[PromptFontStyle] = @PromptFontStyle, [PromptFontSize] = @PromptFontSize, [PromptLeftPositionPercentage] = @PromptLeftPositionPercentage, " +
                    "[PromptText] = @PromptText, [PromptTopPositionPercentage] = @PromptTopPositionPercentage, [ShouldRepeatLast] = @ShouldRepeatLast, " +
                    "[ShouldRetainImageSize] = @ShouldRetainImageSize, [TabIndex] = @TabIndex, [FieldTypeId] = @FieldTypeId where [UniqueId] = @UniqueId");

                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRetainImageSize", DbType.Boolean, field.ShouldRetainImageSize));
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                updateQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update ImageField in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Update Label field.
        /// </summary>
        /// <param name="field">Label field to update.</param>
        public void UpdateField(LabelField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("LabelField");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaFields set [ControlFontFamily] = @ControlFontFamily, [ControlFontStyle] = @ControlFontStyle, " +
                    "[ControlFontSize] = @ControlFontSize, [ControlHeightPercentage] = @ControlHeightPercentage, [ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, " +
                    "[ControlTopPositionPercentage] = @ControlTopPositionPercentage, [ControlWidthPercentage] = @ControlWidthPercentage, " +
                    "[FieldTypeId] = @FieldTypeId, [HasTabStop] = @HasTabStop, [Name] = @Name, [PageId] = @PageId, [PromptText] = @PromptText, " +
                    "[TabIndex] = @TabIndex where [UniqueId] = @UniqueId");

                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                updateQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                updateQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update LabelField in the database", ex);
            }
            finally
            {

            }
        }
        /// <summary>
        /// Update Group field.
        /// </summary>
        /// <param name="field">Group field to update.</param>
        public void UpdateField(GroupField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("GroupField");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaFields set [ControlFontFamily] = @ControlFontFamily, [ControlFontStyle] = @ControlFontStyle, " +
                    "[ControlFontSize] = @ControlFontSize, [ControlHeightPercentage] = @ControlHeightPercentage, [ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, " +
                    "[ControlTopPositionPercentage] = @ControlTopPositionPercentage, [ControlWidthPercentage] = @ControlWidthPercentage, " +
                    "[FieldTypeId] = @FieldTypeId, [HasTabStop] = @HasTabStop, [Name] = @Name, [PageId] = @PageId, [PromptText] = @PromptText, " +
                    "[List] = @List, [BackgroundColor] = @BackgroundColor, " +
                    "[TabIndex] = @TabIndex where [UniqueId] = @UniqueId");

                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                updateQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new QueryParameter("@List", DbType.String, field.ChildFieldNames));
                updateQuery.Parameters.Add(new QueryParameter("@BackgroundColor", DbType.Int32, field.BackgroundColor.ToArgb()));
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                updateQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update GroupField in the database", ex);
            }
        }

        /// <summary>
        /// Update Mirror field.
        /// </summary>
        /// <param name="field">Mirror field to update.</param>
        public void UpdateField(MirrorField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("MirrorField");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaFields set [SourceFieldId] = @SourceFieldId, [ControlFontFamily] = @ControlFontFamily, [ControlFontStyle] = @ControlFontStyle, " +
                    "[ControlFontSize] = @ControlFontSize, [ControlHeightPercentage] = @ControlHeightPercentage, [ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, " +
                    "[ControlTopPositionPercentage] = @ControlTopPositionPercentage, [ControlWidthPercentage] = @ControlWidthPercentage, [HasTabStop] = @HasTabStop, [Name] = @Name, " +
                    "[PageId] = @PageId, [PromptFontFamily] = @PromptFontFamily, [PromptFontStyle] = @PromptFontStyle, [PromptFontSize] = @PromptFontSize, " +
                    "[PromptLeftPositionPercentage] = @PromptLeftPositionPercentage, [PromptText] = @PromptText, [PromptTopPositionPercentage] = @PromptTopPositionPercentage, " +
                    "[TabIndex] = @TabIndex, [FieldTypeId] = @FieldTypeId where [UniqueId] = @UniqueId");

                updateQuery.Parameters.Add(new QueryParameter("@SourceFieldId", DbType.Int32, field.SourceFieldId));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                updateQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update MirrorField in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Update Multiline text field.
        /// </summary>
        /// <param name="field">Multiline text field to update.</param>
        public void UpdateField(MultilineTextField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("MultiLineTextField");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaFields set [CheckCodeAfter] = @CheckCodeAfter, [CheckCodeBefore] = @CheckCodeBefore, " +
                    "[ControlFontFamily] = @ControlFontFamily, [ControlFontStyle] = @ControlFontStyle, [ControlFontSize] = @ControlFontSize, " +
                    "[ControlHeightPercentage] = @ControlHeightPercentage, [ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, " +
                    "[ControlTopPositionPercentage] = @ControlTopPositionPercentage, [ControlWidthPercentage] = @ControlWidthPercentage, " +
                    "[FieldTypeId] = @FieldTypeId, [HasTabStop] = @HasTabStop, [IsReadOnly] = @IsReadOnly, [IsRequired] = @IsRequired, " +
                    "[MaxLength] = @MaxLength, [Name] = @Name, [PageId] = @PageId, [PromptFontFamily] = @PromptFontFamily, " +
                    "[PromptFontStyle] = @PromptFontStyle, [PromptFontSize] = @PromptFontSize, [PromptLeftPositionPercentage] = @PromptLeftPositionPercentage, " +
                    "[PromptText] = @PromptText, [PromptTopPositionPercentage] = @PromptTopPositionPercentage, [ShouldRepeatLast] = @ShouldRepeatLast, " +
                    "[TabIndex] = @TabIndex where [UniqueId] = @UniqueId");

                if (field.CheckCodeAfter == null)
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));

                if (field.CheckCodeBefore == null)
                    updateQuery.Parameters.Add(new QueryParameter(ColumnNames.CHECK_CODE_BEFORE, DbType.String, DBNull.Value));
                else
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeBefore));

                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                updateQuery.Parameters.Add(new QueryParameter("@MaxLength", DbType.Int16, field.MaxLength));
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                updateQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update MultiLineTextField in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Update Number field.
        /// </summary>
        /// <param name="field">Number field to update.</param>
        public void UpdateField(NumberField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("NumberField");
                }
                #endregion
                Query updateQuery = db.CreateQuery("update metaFields set [CheckCodeAfter] = @CheckCodeAfter, [CheckCodeBefore] = @CheckCodeBefore, " +
                    "[ControlFontFamily] = @ControlFontFamily, [ControlFontStyle] = @ControlFontStyle, [ControlFontSize] = @ControlFontSize, " +
                    "[ControlHeightPercentage] = @ControlHeightPercentage, [ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, " +
                    "[ControlTopPositionPercentage] = @ControlTopPositionPercentage, [ControlWidthPercentage] = @ControlWidthPercentage, " +
                    "[FieldTypeId] = @FieldTypeId, [HasTabStop] = @HasTabStop, [IsReadOnly] = @IsReadOnly, [IsRequired] = @IsRequired, " +
                    "[Lower] = @Lower, [Name] = @Name, [PageId] = @PageId, [Pattern] = @Pattern, [PromptFontFamily] = @PromptFontFamily, " +
                    "[PromptFontStyle] = @PromptFontStyle, [PromptFontSize] = @PromptFontSize, [PromptLeftPositionPercentage] = @PromptLeftPositionPercentage, " +
                    "[PromptText] = @PromptText, [PromptTopPositionPercentage] = @PromptTopPositionPercentage, [ShouldRepeatLast] = @ShouldRepeatLast, " +
                    "[TabIndex] = @TabIndex, [Upper]= @Upper where [UniqueId] = @UniqueId");

                if (field.CheckCodeAfter == null)
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));

                if (field.CheckCodeBefore == null)
                    updateQuery.Parameters.Add(new QueryParameter(ColumnNames.CHECK_CODE_BEFORE, DbType.String, DBNull.Value));
                else
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeBefore));

                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                if (field.Lower == null)
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Lower", DbType.String, DBNull.Value));
                }
                else
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Lower", DbType.String, field.Lower));
                }
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                if (field.Pattern == null)
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, DBNull.Value));
                }
                else
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, field.Pattern));
                }
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                if (field.Upper == null)
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Upper", DbType.String, DBNull.Value));
                }
                else
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Upper", DbType.String, field.Upper));
                }
                updateQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));


                db.ExecuteNonQuery(updateQuery);

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update NumberField in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Update Option field.
        /// </summary>
        /// <param name="field">Option field to update.</param>
        public void UpdateField(OptionField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("OptionField");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaFields set [CheckCodeAfter] = @CheckCodeAfter, [ControlFontFamily] = @ControlFontFamily, " +
                    "[ControlFontStyle] = @ControlFontStyle, [ControlFontSize] = @ControlFontSize, [ControlHeightPercentage] = @ControlHeightPercentage, " +
                    "[ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, [ControlTopPositionPercentage] = @ControlTopPositionPercentage, [ControlWidthPercentage] = @ControlWidthPercentage, " +
                    "[HasTabStop] = @HasTabStop, [IsReadOnly] = @IsReadOnly, [IsRequired] = @IsRequired, [Name] = @Name, [PageId] = @PageId, " +
                    "[PromptFontFamily] = @PromptFontFamily, [PromptFontStyle] = @PromptFontStyle, [PromptFontSize] = @PromptFontSize, " +
                    "[PromptText] = @PromptText, [List] = @List, [ShouldRepeatLast] = @ShouldRepeatLast, [ShowTextOnRight] = @ShowTextOnRight, [TabIndex] = @TabIndex, [FieldTypeId] = @FieldTypeId, [Pattern] = @Pattern " +
                    "where [UniqueId] = @UniqueId");

                if (field.CheckCodeAfter == null)
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));

                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new QueryParameter("@List", DbType.String, field.GetOptionsString()));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@ShowTextOnRight", DbType.Boolean, field.ShowTextOnRight));
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                updateQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, field.Pattern));
                updateQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(updateQuery);

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update OptionField in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Update Phone number field.
        /// </summary>
        /// <param name="field">Phone number field to update.</param>
        public void UpdateField(PhoneNumberField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("PhoneNumberField");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaFields set [CheckCodeAfter] = @CheckCodeAfter, [CheckCodeBefore] = @CheckCodeBefore, " +
                    "[ControlFontFamily] = @ControlFontFamily, [ControlFontStyle] = @ControlFontStyle, [ControlFontSize] = @ControlFontSize, " +
                    "[ControlHeightPercentage] = @ControlHeightPercentage, [ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, " +
                    "[ControlTopPositionPercentage] = @ControlTopPositionPercentage, [ControlWidthPercentage] = @ControlWidthPercentage, " +
                    "[FieldTypeId] = @FieldTypeId, [HasTabStop] = @HasTabStop, [IsReadOnly] = @IsReadOnly, [IsRequired] = @IsRequired, " +
                    "[Name] = @Name, [PageId] = @PageId, [Pattern] = @Pattern, [PromptFontFamily] = @PromptFontFamily, [PromptFontStyle] = @PromptFontStyle, " +
                    "[PromptFontSize] = @PromptFontSize, [PromptLeftPositionPercentage] = @PromptLeftPositionPercentage, [PromptText] = @PromptText, " +
                    "[PromptTopPositionPercentage] = @PromptTopPositionPercentage, [ShouldRepeatLast] = @ShouldRepeatLast, [TabIndex] = @TabIndex " +
                    "where [UniqueId] = @UniqueId");

                if (field.CheckCodeAfter == null)
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));

                if (field.CheckCodeBefore == null)
                    updateQuery.Parameters.Add(new QueryParameter(ColumnNames.CHECK_CODE_BEFORE, DbType.String, DBNull.Value));
                else
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeBefore));

                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                if (field.Pattern == null)
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, DBNull.Value));
                }
                else
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, field.Pattern));
                }

                updateQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                updateQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(updateQuery);

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update PhoneNumberField in the database", ex);
            }
        }

        /// <summary>
        /// Update Related View field.
        /// </summary>
        /// <param name="field">Related View field to update.</param>
        public void UpdateField(RelatedViewField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("RelatedViewField");
                }
                #endregion

                Boolean relatedViewIdExists = false;

                if (this.GetViewById(field.RelatedViewID) != null)
                {
                    relatedViewIdExists = true;
                }

                Query updateQuery = null;

                if (relatedViewIdExists)
                {
                    updateQuery = db.CreateQuery("update metaFields set [RelateCondition] = @RelateCondition, [ControlFontFamily] = @ControlFontFamily, " +
                        "[ControlFontStyle] = @ControlFontStyle, [ControlFontSize] = @ControlFontSize, [ControlHeightPercentage] = @ControlHeightPercentage, " +
                        "[ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, [ControlTopPositionPercentage] = @ControlTopPositionPercentage, " +
                        "[ControlWidthPercentage] = @ControlWidthPercentage, [FieldTypeId] = @FieldTypeId, [HasTabStop] = @HasTabStop, [Name] = @Name, " +
                        "[PageId] = @PageId, [PromptText] = @PromptText, [ShouldReturnToParent] = @ShouldReturnToParent, [TabIndex] = @TabIndex, " +
                        "[RelatedViewId] = @RelatedViewId where [UniqueId] = @UniqueId");
                }
                else
                {
                    updateQuery = db.CreateQuery("update metaFields set [RelateCondition] = @RelateCondition, [ControlFontFamily] = @ControlFontFamily, " +
                        "[ControlFontStyle] = @ControlFontStyle, [ControlFontSize] = @ControlFontSize, [ControlHeightPercentage] = @ControlHeightPercentage, " +
                        "[ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, [ControlTopPositionPercentage] = @ControlTopPositionPercentage, " +
                        "[ControlWidthPercentage] = @ControlWidthPercentage, [FieldTypeId] = @FieldTypeId, [HasTabStop] = @HasTabStop, [Name] = @Name, " +
                        "[PageId] = @PageId, [PromptText] = @PromptText, [ShouldReturnToParent] = @ShouldReturnToParent, [TabIndex] = @TabIndex " +
                        "where [UniqueId] = @UniqueId");
                }


                if (field.Condition == null)
                {
                    updateQuery.Parameters.Add(new QueryParameter("@RelateCondition", DbType.String, DBNull.Value));
                }
                else
                {
                    updateQuery.Parameters.Add(new QueryParameter("@RelateCondition", DbType.String, field.Condition));
                }

                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                updateQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldReturnToParent", DbType.Boolean, field.ShouldReturnToParent));
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));

                if (relatedViewIdExists)
                {
                    updateQuery.Parameters.Add(new QueryParameter("@RelatedViewId", DbType.Int32, field.RelatedViewID));
                }
                
                updateQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(updateQuery);

                if (relatedViewIdExists)
                {
                    RelateFieldToView(field.UniqueId, field.RelatedViewID, field.Condition, field.ShouldReturnToParent);
                }
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update RelatedViewField in the database", ex);
            }
        }

        /// <summary>
        /// Update Single line text field.
        /// </summary>
        /// <param name="field">Single line text field to update.</param>
        public void UpdateField(SingleLineTextField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("SingeLineTextField");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaFields set [CheckCodeAfter] = @CheckCodeAfter, [CheckCodeBefore] = @CheckCodeBefore, " +
                    "[ControlFontFamily] = @ControlFontFamily, [ControlFontStyle] = @ControlFontStyle, [ControlFontSize] = @ControlFontSize, " +
                    "[ControlHeightPercentage] = @ControlHeightPercentage, [ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, " +
                    "[ControlTopPositionPercentage] = @ControlTopPositionPercentage, [ControlWidthPercentage] = @ControlWidthPercentage, " +
                    "[FieldTypeId] = @FieldTypeId, [HasTabStop] = @HasTabStop, [IsReadOnly] = @IsReadOnly, [IsRequired] = @IsRequired, [IsEncrypted] = @IsEncrypted, " +
                    "[MaxLength] = @MaxLength, [Name] = @Name, [PageId] = @PageId, [PromptFontFamily] = @PromptFontFamily, " +
                    "[PromptFontStyle] = @PromptFontStyle, [PromptFontSize] = @PromptFontSize, [PromptLeftPositionPercentage] = @PromptLeftPositionPercentage, " +
                    "[PromptText] = @PromptText, [PromptTopPositionPercentage] = @PromptTopPositionPercentage, [ShouldRepeatLast] = @ShouldRepeatLast, " +
                    "[TabIndex] = @TabIndex where [UniqueId] = @UniqueId");

                if (field.CheckCodeAfter == null)
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));

                if (field.CheckCodeBefore == null)
                    updateQuery.Parameters.Add(new QueryParameter(ColumnNames.CHECK_CODE_BEFORE, DbType.String, DBNull.Value));
                else
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeBefore));

                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                updateQuery.Parameters.Add(new QueryParameter("@IsEncrypted", DbType.Boolean, field.IsEncrypted));
                updateQuery.Parameters.Add(new QueryParameter("@MaxLength", DbType.Int16, field.MaxLength));
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                updateQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update SingleLineTextField in the database", ex);
            }
        }

        /// <summary>
        /// Update Time field.
        /// </summary>
        /// <param name="field">Time field to update.</param>
        public void UpdateField(TimeField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("TimeField");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaFields set [CheckCodeAfter] = @CheckCodeAfter, [CheckCodeBefore] = @CheckCodeBefore, " +
                    "[ControlFontFamily] = @ControlFontFamily, [ControlFontStyle] = @ControlFontStyle, [ControlFontSize] = @ControlFontSize, " +
                    "[ControlHeightPercentage] = @ControlHeightPercentage, [ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, [ControlTopPositionPercentage] = @ControlTopPositionPercentage, " +
                    "[ControlWidthPercentage] = @ControlWidthPercentage, [HasTabStop] = @HasTabStop, [IsReadOnly] = @IsReadOnly, [IsRequired] = @IsRequired, " +
                    "[Name] = @Name, [PageId] = @PageId, [PromptFontFamily] = @PromptFontFamily, [PromptFontStyle] = @PromptFontStyle, " +
                    "[PromptFontSize] = @PromptFontSize, [PromptLeftPositionPercentage] = @PromptLeftPositionPercentage, [PromptText] = @PromptText, " +
                    "[PromptTopPositionPercentage] = @PromptTopPositionPercentage, [ShouldRepeatLast] = @ShouldRepeatLast, [TabIndex] = @TabIndex, [FieldTypeId] = @FieldTypeId " +
                    "where [UniqueId] = @UniqueId");

                if (field.CheckCodeAfter == null)
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));

                if (field.CheckCodeBefore == null)
                    updateQuery.Parameters.Add(new QueryParameter(ColumnNames.CHECK_CODE_BEFORE, DbType.String, DBNull.Value));
                else
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeBefore));

                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                updateQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(updateQuery);

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update TimeField in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Update Uppercase text field.
        /// </summary>
        /// <param name="field">Uppercase text field to update.</param>
        public void UpdateField(UpperCaseTextField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("UpperCaseTextField");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaFields set [CheckCodeAfter] = @CheckCodeAfter, [CheckCodeBefore] = @CheckCodeBefore, " +
                    "[ControlFontFamily] = @ControlFontFamily, [ControlFontStyle] = @ControlFontStyle, [ControlFontSize] = @ControlFontSize, " +
                    "[ControlHeightPercentage] = @ControlHeightPercentage, [ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, [ControlTopPositionPercentage] = @ControlTopPositionPercentage, " +
                    "[ControlWidthPercentage] = @ControlWidthPercentage, [HasTabStop] = @HasTabStop, [IsReadOnly] = @IsReadOnly, [IsRequired] = @IsRequired, " +
                    "[MaxLength] = @MaxLength, [Name] = @Name, [PageId] = @PageId, [PromptFontFamily] = @PromptFontFamily, [PromptFontStyle] = @PromptFontStyle, " +
                    "[PromptFontSize] = @PromptFontSize, [PromptLeftPositionPercentage] = @PromptLeftPositionPercentage, [PromptText] = @PromptText, " +
                    "[PromptTopPositionPercentage] = @PromptTopPositionPercentage, [ShouldRepeatLast] = @ShouldRepeatLast, [TabIndex] = @TabIndex, [FieldTypeId] = @FieldTypeId " +
                    "where [UniqueId] = @UniqueId");

                if (field.CheckCodeAfter == null)
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, DBNull.Value));
                else
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, field.CheckCodeAfter));

                if (field.CheckCodeBefore == null)
                    updateQuery.Parameters.Add(new QueryParameter(ColumnNames.CHECK_CODE_BEFORE, DbType.String, DBNull.Value));
                else
                    updateQuery.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, field.CheckCodeBefore));

                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                updateQuery.Parameters.Add(new QueryParameter("@MaxLength", DbType.Int16, field.MaxLength));
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                updateQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(updateQuery);

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update UpperCaseTextField in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Update Yes/No field.
        /// </summary>
        /// <param name="field">Yes/No field to update.</param>
        public void UpdateField(YesNoField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("YesNoField");
                }
                #endregion

                Query updateQuery = db.CreateQuery("update metaFields set [ControlHeightPercentage] = @ControlHeightPercentage, [ControlLeftPositionPercentage] = @ControlLeftPositionPercentage, " +
                    "[ControlTopPositionPercentage] = @ControlTopPositionPercentage, [ControlWidthPercentage] = @ControlWidthPercentage, " +
                    "[ControlFontFamily] = @ControlFontFamily, " +
                    "[ControlFontStyle] = @ControlFontStyle, " +
                    "[ControlFontSize] = @ControlFontSize, " +
                    "[HasTabStop] = @HasTabStop, [IsReadOnly] = @IsReadOnly, " +
                    "[IsRequired] = @IsRequired, [Name] = @Name, [PageId] = @PageId, [PromptFontFamily] = @PromptFontFamily, " +
                    "[PromptFontStyle] = @PromptFontStyle, [PromptFontSize] = @PromptFontSize, [PromptLeftPositionPercentage] = @PromptLeftPositionPercentage, " +
                    "[PromptText] = @PromptText, [PromptTopPositionPercentage] = @PromptTopPositionPercentage, [ShouldRepeatLast] = @ShouldRepeatLast, " +
                    "[TabIndex] = @TabIndex, [FieldTypeId] = @FieldTypeId where [UniqueId] = @UniqueId");

                updateQuery.Parameters.Add(new QueryParameter("@ControlHeightPercentage", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlLeftPositionPercentage", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlTopPositionPercentage", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlWidthPercentage", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, field.IsReadOnly));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, field.IsRequired));
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, field.Page.Id));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                updateQuery.Parameters.Add(new QueryParameter("@PromptLeftPositionPercentage", DbType.Double, field.PromptLeftPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new QueryParameter("@PromptTopPositionPercentage", DbType.Double, field.PromptTopPositionPercentage));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, field.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@TabIndex", DbType.Int32, field.TabIndex));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)field.FieldType));
                updateQuery.Parameters.Add(new QueryParameter("@UniqueId", DbType.Guid, field.UniqueId));

                db.ExecuteNonQuery(updateQuery);

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update YesNoField in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Updates a program saved in the database
        /// </summary>
        /// <param name="programId">Id of the program</param>
        /// <param name="name">Name of the program</param>
        /// <param name="content">Content of the program</param>
        /// <param name="comment">Comment for the program</param>
        /// <param name="author">Author of the program</param>
        public void UpdatePgm(int programId, string name, string content, string comment, string author)
        {
            try
            {
                #region Input Validation
                if (programId < 1)
                {
                    throw new ArgumentOutOfRangeException("ProgramId");
                }
                #endregion
                Query updateQuery = db.CreateQuery("update [metaPrograms] set [" +
                                      ColumnNames.PGM_NAME + "] = @Name, [" +
                                      ColumnNames.PGM_CONTENT + "] = @Content , [" +
                                      ColumnNames.PGM_COMMENT + "] = @Comment, [" +
                                      ColumnNames.PGM_AUTHOR + "] = @Author, [" +
                                      ColumnNames.PGM_MODIFY_DATE + "] = @DateModified " +
                                      "where [ProgramId] = @ProgramId");
                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, name));
                updateQuery.Parameters.Add(new QueryParameter("@Content", DbType.String, content));
                updateQuery.Parameters.Add(new QueryParameter("@Comment", DbType.String, comment));
                updateQuery.Parameters.Add(new QueryParameter("@Author", DbType.String, author));
                updateQuery.Parameters.Add(new QueryParameter("@DateModified", DbType.String, System.DateTime.Now.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ProgramId", DbType.Int32, programId));

                db.ExecuteNonQuery(updateQuery);

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update program.", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Update codes field sources in metaFields table.
        /// </summary>
        /// <param name="fieldId">Unique field Id number.</param>
        /// <param name="sourceFieldId">Unique source field Id number.</param>
        /// <param name="codeColumnName">Code column name.</param>
        /// <param name="sourceTableName">Source table name.</param>
        public void UpdateCodesFieldSources(int fieldId, int sourceFieldId, string codeColumnName, string sourceTableName)
        {
            try
            {
                #region Input Validation
                if (fieldId < 1)
                {
                    throw new ArgumentOutOfRangeException("FieldId");
                }
                if (sourceFieldId < 1)
                {
                    throw new ArgumentOutOfRangeException("SourceFieldId");
                }
                if (string.IsNullOrEmpty(codeColumnName))
                {
                    throw new ArgumentNullException("CodeColumnName");
                }
                if (string.IsNullOrEmpty(sourceTableName))
                {
                    throw new ArgumentNullException("SourceTableName");
                }
                #endregion
                Query updateQuery = db.CreateQuery("update [metaFields] set [SourceFieldId] = @SourceFieldId, " +
                    "[CodeColumnName] = @CodeColumnName, [SourceTableName] = @SourceTableName " +
                    " where [FieldId] = @FieldId");
                updateQuery.Parameters.Add(new QueryParameter("@SourceFieldId", DbType.Int32, sourceFieldId));
                updateQuery.Parameters.Add(new QueryParameter("@CodeColumnName", DbType.String, codeColumnName));
                updateQuery.Parameters.Add(new QueryParameter("@SourceTableName", DbType.String, sourceTableName));
                updateQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, fieldId));

                db.ExecuteNonQuery(updateQuery);

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update code field information.", ex);
            }
            finally
            {

            }

        }
        /// <summary>
        /// Updates a date column saved in the database
        /// </summary>
        /// <param name="column">Column to be updated</param>
        public void UpdateGridColumn(ContiguousColumn column)
        {
            #region InputValidation
            if (column == null)
            {
                throw new ArgumentNullException("DateColumn");
            }
            #endregion
            try
            {
                Query updateQuery = db.CreateQuery("update [metaGridColumns] set [Name] = @Name, [Width] = @Width, [Position] = @Position, [FieldTypeId] = @FieldTypeId, [Text] = @Text, [ShouldRepeatLast] = @ShouldRepeatLast, [IsRequired] = @IsRequired, [IsReadOnly] = @IsReadOnly, [Pattern] = @Pattern, [Upper] = @Upper , [Lower] = @Lower " +
                    "where [GridColumnId] = @GridColumnId");

                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                updateQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, column.Width));
                updateQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, column.Position));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                updateQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, column.Text));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, column.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, column.IsRequired));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, column.IsReadOnly));
                if (column.Pattern == null)
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, DBNull.Value));
                }
                else
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, column.Pattern));
                }
                if (column.Upper == null)
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Upper", DbType.String, DBNull.Value));
                }
                else
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Upper", DbType.String, column.Upper));
                }
                if (column.Lower == null)
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Lower", DbType.String, DBNull.Value));
                }
                else
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Lower", DbType.String, column.Lower));
                }
                updateQuery.Parameters.Add(new QueryParameter("@GridColumnId", DbType.Int32, column.Id));

                db.ExecuteNonQuery(updateQuery);

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update grid column in the database", ex);
            }
            finally
            {

            }
        }
        /// <summary>
        /// Updates a date column saved in the database
        /// </summary>
        /// <param name="column">Column to be updated</param>
        public void UpdateGridColumn(PhoneNumberColumn column)
        {
            #region InputValidation
            if (column == null)
            {
                throw new ArgumentNullException("PhoneNumberColumn");
            }
            #endregion
            try
            {
                Query updateQuery = db.CreateQuery("update [metaGridColumns] set [Name] = @Name, [Width] = @Width, [Position] = @Position, [FieldTypeId] = @FieldTypeId, [Text] = @Text, [ShouldRepeatLast] = @ShouldRepeatLast, [IsRequired] = @IsRequired, [IsReadOnly] = @IsReadOnly, [Pattern] = @Pattern " +
                    "where [GridColumnId] = @GridColumnId");

                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                updateQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, column.Width));
                updateQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, column.Position));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                updateQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, column.Text));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, column.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, column.IsRequired));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, column.IsReadOnly));
                if (column.Pattern == null)
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, DBNull.Value));
                }
                else
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, column.Pattern));
                }

                updateQuery.Parameters.Add(new QueryParameter("@GridColumnId", DbType.Int32, column.Id));


                db.ExecuteNonQuery(updateQuery);

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update grid column in the database", ex);
            }
            finally
            {

            }
        }
        /// <summary>
        /// Updates a date column saved in the database
        /// </summary>
        /// <param name="column">Column to be updated</param>
        public void UpdateGridColumn(NumberColumn column)
        {
            #region InputValidation
            if (column == null)
            {
                throw new ArgumentNullException("NumberColumn");
            }
            #endregion
            try
            {
                Query updateQuery = db.CreateQuery("update [metaGridColumns] set [Name] = @Name, [Width] = @Width, [Position] = @Position, [FieldTypeId] = @FieldTypeId, [Text] = @Text, [ShouldRepeatLast] = @ShouldRepeatLast, [IsRequired] = @IsRequired, [IsReadOnly] = @IsReadOnly, [Pattern] = @Pattern, [Upper] = @Upper , [Lower] = @Lower " +
                    "where [GridColumnId] = @GridColumnId");

                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                updateQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, column.Width));
                updateQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, column.Position));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                updateQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, column.Text));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, column.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, column.IsRequired));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, column.IsReadOnly));
                if (column.Pattern == null)
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, DBNull.Value));
                }
                else
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Pattern", DbType.String, column.Pattern));
                }
                if (column.Upper == null)
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Upper", DbType.String, DBNull.Value));
                }
                else
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Upper", DbType.String, column.Upper));
                }
                if (column.Lower == null)
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Lower", DbType.String, DBNull.Value));
                }
                else
                {
                    updateQuery.Parameters.Add(new QueryParameter("@Lower", DbType.String, column.Lower));
                }
                updateQuery.Parameters.Add(new QueryParameter("@GridColumnId", DbType.Int32, column.Id));


                db.ExecuteNonQuery(updateQuery);

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update grid column in the database", ex);
            }
            finally
            {

            }
        }
        /// <summary>
        /// Updates a date column saved in the database
        /// </summary>
        /// <param name="column">Column to be updated</param>
        public void UpdateGridColumn(TextColumn column)
        {
            #region InputValidation
            if (column == null)
            {
                throw new ArgumentNullException("TextColumn");
            }
            #endregion
            try
            {
                Query updateQuery = db.CreateQuery("update [metaGridColumns] set [Name] = @Name, [Width] = @Width, [Position] = @Position, [FieldTypeId] = @FieldTypeId, [Text] = @Text, [ShouldRepeatLast] = @ShouldRepeatLast, [IsRequired] = @IsRequired, [IsReadOnly] = @IsReadOnly, [Size] = @Size " +
                    "where [GridColumnId] = @GridColumnId");

                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                updateQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, column.Width));
                updateQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, column.Position));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                updateQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, column.Text));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, column.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, column.IsRequired));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, column.IsReadOnly));
                updateQuery.Parameters.Add(new QueryParameter("@Size", DbType.Int16, column.Size));

                updateQuery.Parameters.Add(new QueryParameter("@GridColumnId", DbType.Int32, column.Id));


                db.ExecuteNonQuery(updateQuery);

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update grid column in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Updates a column saved in the database
        /// </summary>
        /// <param name="column">Column to be updated</param>
        public void UpdateGridColumn(CheckboxColumn column)
        {
            #region InputValidation
            if (column == null)
            {
                throw new ArgumentNullException("CheckboxColumn");
            }
            #endregion
            try
            {
                Query updateQuery = db.CreateQuery("update [metaGridColumns] set [Name] = @Name, [Width] = @Width, [Position] = @Position, [FieldTypeId] = @FieldTypeId, [Text] = @Text, [ShouldRepeatLast] = @ShouldRepeatLast, [IsRequired] = @IsRequired, [IsReadOnly] = @IsReadOnly " +
                    "where [GridColumnId] = @GridColumnId");

                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                updateQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, column.Width));
                updateQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, column.Position));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                updateQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, column.Text));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, column.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, column.IsRequired));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, column.IsReadOnly));

                updateQuery.Parameters.Add(new QueryParameter("@GridColumnId", DbType.Int32, column.Id));

                db.ExecuteNonQuery(updateQuery);

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update grid column in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Updates a column saved in the database
        /// </summary>
        /// <param name="column">Column to be updated</param>
        public void UpdateGridColumn(YesNoColumn column)
        {
            #region InputValidation
            if (column == null)
            {
                throw new ArgumentNullException("YesNoColumn");
            }
            #endregion
            try
            {
                Query updateQuery = db.CreateQuery("update [metaGridColumns] set [Name] = @Name, [Width] = @Width, [Position] = @Position, [FieldTypeId] = @FieldTypeId, [Text] = @Text, [ShouldRepeatLast] = @ShouldRepeatLast, [IsRequired] = @IsRequired, [IsReadOnly] = @IsReadOnly " +
                    "where [GridColumnId] = @GridColumnId");

                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                updateQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, column.Width));
                updateQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, column.Position));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                updateQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, column.Text));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, column.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, column.IsRequired));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, column.IsReadOnly));

                updateQuery.Parameters.Add(new QueryParameter("@GridColumnId", DbType.Int32, column.Id));

                db.ExecuteNonQuery(updateQuery);

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update grid column in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Updates a DDLColumnOfCommentLegal saved in the database.
        /// </summary>
        /// <param name="column">Column to be updated</param>
        public void UpdateGridColumn(DDLColumnOfCommentLegal column)
        {
            #region InputValidation
            if (column == null)
            {
                throw new ArgumentNullException("DDLColumnOfCommentLegal");
            }
            #endregion
            try
            {
                Query updateQuery = db.CreateQuery("update [metaGridColumns] set [Name] = @Name, [TextColumnName] = @TextColumnName, [SourceTableName] = @SourceTableName, [Width] = @Width, [Position] = @Position, " +
                    "[FieldTypeId] = @FieldTypeId, [Text] = @Text, [ShouldRepeatLast] = @ShouldRepeatLast, [IsExclusiveTable] = @IsExclusiveTable, [IsRequired] = @IsRequired, [IsReadOnly] = @IsReadOnly, [Sort] = @Sort " +
                    "where [GridColumnId] = @GridColumnId");

                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                updateQuery.Parameters.Add(new QueryParameter("@TextColumnName", DbType.String, column.TextColumnName));
                updateQuery.Parameters.Add(new QueryParameter("@SourceTableName", DbType.String, column.SourceTableName));
                updateQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, column.Width));
                updateQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, column.Position));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                updateQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, column.Text));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, column.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@IsExclusiveTable", DbType.Boolean, column.IsExclusiveTable));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, column.IsReadOnly));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, column.IsRequired));
                updateQuery.Parameters.Add(new QueryParameter("@Sort", DbType.Boolean, column.ShouldSort));

                updateQuery.Parameters.Add(new QueryParameter("@GridColumnId", DbType.Int32, column.Id));

                db.ExecuteNonQuery(updateQuery);

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update grid column in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Updates a DDLColumnOfLegalValues saved in the database.
        /// </summary>
        /// <param name="column">Column to be updated</param>
        public void UpdateGridColumn(DDLColumnOfLegalValues column)
        {
            #region InputValidation
            if (column == null)
            {
                throw new ArgumentNullException("DDLColumnOfLegalValues");
            }
            #endregion
            try
            {
                Query updateQuery = db.CreateQuery("update [metaGridColumns] set [Name] = @Name, [TextColumnName] = @TextColumnName, [SourceTableName] = @SourceTableName, [Width] = @Width, [Position] = @Position, " +
                    "[FieldTypeId] = @FieldTypeId, [Text] = @Text, [ShouldRepeatLast] = @ShouldRepeatLast, [IsExclusiveTable] = @IsExclusiveTable, [IsRequired] = @IsRequired, [IsReadOnly] = @IsReadOnly, [Sort] = @Sort " +
                    "where [GridColumnId] = @GridColumnId");

                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                updateQuery.Parameters.Add(new QueryParameter("@TextColumnName", DbType.String, column.TextColumnName));
                updateQuery.Parameters.Add(new QueryParameter("@SourceTableName", DbType.String, column.SourceTableName));
                updateQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, column.Width));
                updateQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, column.Position));
                updateQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                updateQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, column.Text));
                updateQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, column.ShouldRepeatLast));
                updateQuery.Parameters.Add(new QueryParameter("@IsExclusiveTable", DbType.Boolean, column.IsExclusiveTable));
                updateQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, column.IsReadOnly));
                updateQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, column.IsRequired));
                updateQuery.Parameters.Add(new QueryParameter("@Sort", DbType.Boolean, column.ShouldSort));

                updateQuery.Parameters.Add(new QueryParameter("@GridColumnId", DbType.Int32, column.Id));

                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update grid column in the database", ex);
            }
            finally
            {

            }
        }

        public void UpdateFonts(Font controlFont, Font promptFont, float viewId = -1, float pageId = -1)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("update [metaFields] set ");
                
                sb.Append("[ControlFontFamily] = @ControlFontFamily, ");
                sb.Append("[ControlFontStyle] = @ControlFontStyle, ");
                sb.Append("[ControlFontSize] = @ControlFontSize, ");

                sb.Append("[PromptFontFamily] = @PromptFontFamily, ");
                sb.Append("[PromptFontStyle] = @PromptFontStyle, ");
                sb.Append("[PromptFontSize] = @PromptFontSize ");

                if (viewId != -1 && pageId != -1)
                {
                    sb.Append("where [ViewId] = @ViewId and [PageId] = @PageId ");
                }
                else if (viewId != -1 && pageId == -1)
                {
                    sb.Append("where [ViewId] = @ViewId ");
                }

                string queryString = sb.ToString();
                Query updateQuery = db.CreateQuery(queryString);

                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, controlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, controlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, controlFont.Size));

                updateQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, promptFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, promptFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, promptFont.Size));

                if (viewId != -1 && pageId != -1)
                {
                    updateQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, viewId));
                    updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, pageId));
                }
                else if (viewId != -1 && pageId == -1)
                {
                    updateQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, viewId));
                }

                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not apply default font.", ex);
            }
            finally
            {

            }
        }

        #endregion Update Statements

        #region Utility Methods

        /// <summary>
        /// Returns the latest view instered into the Database.
        /// </summary>
        /// <returns>Newest View.</returns>
        public View GetLatestViewInserted()
        {
            return GetViewById(GetMaxViewId());
        }

        /// <summary>
        /// Returns the Id of the last view added
        /// </summary>
        /// <returns>Maximum View Id.</returns>
        public int GetMaxViewId()
        {
            Query selectQuery = db.CreateQuery("select MAX(ViewId) from metaViews");
            return (int)db.ExecuteScalar(selectQuery);
        }

        /// <summary>
        /// Returns the Id of the last page added
        /// </summary>
        /// <param name="viewId">Unique View Id.</param>
        /// <returns>Maximum unique Page Id.</returns>
        public int GetMaxPageId(int viewId)
        {
            Query selectQuery = db.CreateQuery("select MAX(PageId) from metaPages where ViewId = @ViewId");
            selectQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, viewId));
            object result = db.ExecuteScalar(selectQuery);
            if (result != DBNull.Value)
            {
                return (int)result;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns the Id of the last field added
        /// </summary>
        /// <param name="viewId">Unique View Id.</param>
        /// <returns>Maximum unique field Id.</returns>
        public int GetMaxFieldId(int viewId)
        {
            Query selectQuery = db.CreateQuery("select MAX(FieldId) from metaFields where ViewId = @ViewId");
            selectQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, viewId));
            return (int)db.ExecuteScalar(selectQuery);
        }

        private int GetMaxMapId()
        {
            Query selectQuery = db.CreateQuery("select MAX(MapId) from metaMaps");
            return (int)db.ExecuteScalar(selectQuery);
        }

        private int GetMaxLayerId()
        {
            Query selectQuery = db.CreateQuery("select MAX(LayerId) from metaLayers");
            return (int)db.ExecuteScalar(selectQuery);
        }

        public int GetMaxBackgroundId()
        {
            Query selectQuery = db.CreateQuery("select MAX(BackgroundId) from metaBackgrounds" );
            return (int)db.ExecuteScalar(selectQuery);
        }

        public int GetMaxImageId()
        {
            Query selectQuery = db.CreateQuery("select MAX(ImageId) from metaImages");
            return (int)db.ExecuteScalar(selectQuery);
        }

        /// <summary>
        /// Returns the latest grid column Id inserted
        /// </summary>
        /// <param name="gridFieldId"></param>
        /// <returns>The maximum grid column Id</returns>
        public int GetMaxGridColumnId(int gridFieldId)
        {
            Query selectQuery = db.CreateQuery("select MAX(GridColumnId) from metaGridColumns where FieldId = @gridFieldId");
            selectQuery.Parameters.Add(new QueryParameter("@gridFieldId", DbType.Int32, gridFieldId));
            return (int)db.ExecuteScalar(selectQuery);
        }

        /// <summary>
        /// Returns the maximum tab index for a view's page
        /// </summary>
        /// <param name="pageId">The page id</param>
        /// <param name="viewId">The view id</param>
        /// <returns>The maximum tab index</returns>
        public double GetMaxTabIndex(int pageId, int viewId, bool? includeReadOnly = null)
        {
            Query selectQuery;

            if (includeReadOnly.HasValue)
            {
                selectQuery = db.CreateQuery("select MAX(TabIndex) from metaFields where PageId = @pageId and ViewId = @viewId and (IsReadOnly = @isReadOnly or IsReadOnly is null) and [FieldTypeId] not in (2,21,25)");
                selectQuery.Parameters.Add(new QueryParameter("@pageId", DbType.Int32, pageId));
                selectQuery.Parameters.Add(new QueryParameter("@viewId", DbType.Int32, viewId));
                selectQuery.Parameters.Add(new QueryParameter("@isReadOnly", DbType.Boolean, includeReadOnly.Value));
            }
            else
            {
                selectQuery = db.CreateQuery("select MAX(TabIndex) from metaFields where PageId = @pageId and ViewId = @viewId and [FieldTypeId] not in (2,21,25)");
                selectQuery.Parameters.Add(new QueryParameter("@pageId", DbType.Int32, pageId));
                selectQuery.Parameters.Add(new QueryParameter("@viewId", DbType.Int32, viewId));
            }
            
            object result = db.ExecuteScalar(selectQuery);

            if (result != DBNull.Value)
            {
                return Convert.ToDouble(result);
            }
            else
            {
                return -1;
            }
        }


        /// <summary>
        /// Returns the next tab index of a control for a view's page
        /// </summary>
        /// <param name="page">The view's page </param>
        /// <param name="view">The view</param>
        /// <param name="currentTabIndex">The current control's tab index</param>
        /// <returns>The next tab index</returns>
        public double GetNextTabIndex(Page page, View view, double currentTabIndex)
        {
            Query selectQuery = db.CreateQuery("select min(TabIndex) From metaFields where PageId = @pageId and ViewId = @viewId and TabIndex > @currentTabIndex and [FieldTypeId] not in (2,14,15,21) and @hasTabStop = [hasTabStop]");
            selectQuery.Parameters.Add(new QueryParameter("@pageId", DbType.Int32, page.Id));
            selectQuery.Parameters.Add(new QueryParameter("@viewId", DbType.Int32, view.Id));
            selectQuery.Parameters.Add(new QueryParameter("@currentTabIndex", DbType.Double, currentTabIndex));
            selectQuery.Parameters.Add(new QueryParameter("@hasTabStop", DbType.Boolean, true));
            object result = db.ExecuteScalar(selectQuery);

            if (result != DBNull.Value)
            {
                return Convert.ToDouble(result);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Returns the max page position of a view
        /// </summary>
        /// <param name="viewId">The view id</param>
        /// <returns>The maximum page position</returns>
        public int GetMaxViewPagesPosition(int viewId)
        {
            Query selectQuery = db.CreateQuery("select MAX(metaPages.[Position]) From metaPages where ViewId = @viewId");
            selectQuery.Parameters.Add(new QueryParameter("@viewId", DbType.Int32, viewId));
            object result = db.ExecuteScalar(selectQuery);

            if (result != DBNull.Value)
            {
                return (short)result;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Gets table column names for a view.
        /// </summary>
        /// <param name="viewId">Id of the <see cref="Epi.View"/></param>
        /// <returns>string</returns>
        public DataTable GetMetaFieldsSchema(int viewId)
        {
            try
            {
                Query selectQuery = db.CreateQuery("select * from [metaFields] where [ViewId] = @ViewID and [FieldTypeId] = 23");
                selectQuery.Parameters.Add(new QueryParameter("@ViewID", DbType.Int32, viewId));
                DataTable table = db.Select(selectQuery);
                table.Clear();
                return table;
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve table column names", ex);
            }
        }
        
        /// <summary>
        /// Gets the minimum tab index of data fields
        /// </summary>
        /// <param name="pageId">The page id</param>
        /// <param name="viewId">The view id</param>
        /// <returns>The minimum tab index; -1 is none exists</returns>
        public double GetMinTabIndex(int pageId, int viewId)
        {
            Query selectQuery = db.CreateQuery("select min(TabIndex) from metaFields where PageId = @pageId and ViewId = @viewId and [FieldTypeId] not in (2,15,21) and HasTabStop = @hasTabStop");
            selectQuery.Parameters.Add(new QueryParameter("@pageId", DbType.Int32, pageId));
            selectQuery.Parameters.Add(new QueryParameter("@viewId", DbType.Int32, viewId));
            selectQuery.Parameters.Add(new QueryParameter("@hasTabStop", DbType.Boolean, true));
            object result = db.ExecuteScalar(selectQuery);
            if (result != DBNull.Value)
            {
                return Convert.ToDouble(result);
            }
            else
            {
                return -1;
            }

        }

        #endregion Utility Methods


        /// <summary>
        /// Gets a code table by name
        /// </summary>
        /// <param name="tableName">Name of code table</param>
        /// <returns>Contents of the code table</returns>
        public virtual DataTable GetCodeTable(string tableName)
        {
            try
            {
                Query selectQuery = db.CreateQuery("select * from [" + tableName + "]");
                DataTable table = db.Select(selectQuery);
                return table;
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve the code table.", ex);
            }
        }
        /// <summary>
        /// Gets the field to view relations for a view
        /// </summary>
        /// <param name="viewTableName">Name of view table</param>
        /// <returns>DataTable containing view relations</returns>
        public virtual DataTable GetViewRelations(string viewTableName)
        {
            throw new System.ApplicationException("Not implemented");
        }

        /// <summary>
        /// Gets check code variables
        /// </summary>
        /// <param name="viewName">Name of view table</param>
        /// <returns>DataTable containing checkcode variables</returns>
        public virtual DataTable GetCheckCodeVariableDefinitions(string viewName)
        {
            throw new System.ApplicationException("Not implemented");
        }

        /// <summary>
        /// Gets view check code
        /// </summary>
        /// <param name="viewName">Name of view table</param>
        /// <returns>DataTable containing view check code</returns>
        public virtual DataTable GetViewCheckCode(string viewName)
        {
            throw new System.ApplicationException("Not implemented");
        }

        /// <summary>
        /// Gets the source field name of a mirror field
        /// </summary>
        /// <param name="fieldName">Name of the mirror field</param>
        /// <param name="viewName">Name of the view</param>
        /// <returns>Name of the source field</returns>
        public virtual string GetSourceFieldName(string fieldName, string viewName)
        {
            throw new System.ApplicationException("Not implemented");
        }
        /// <summary>
        /// Gets the fields in a page
        /// </summary>
        /// <param name="viewName">Name of the view table</param>
        /// <param name="pageNumber">Page number</param>
        /// <returns>DataTable containing fields</returns>
        public virtual DataTable GetFieldsOnPageAsDataTable(string viewName, int pageNumber)
        {
            throw new System.ApplicationException("Not implemented");
        }

        /// <summary>
        /// Gets the columns in a grid
        /// </summary>
        /// <param name="gridTableName">Name of grid table</param>
        /// <returns>DataTable containing grid columns</returns>
        public virtual DataTable GetGridColumns(string gridTableName)
        {
            throw new System.ApplicationException("Not implemented");
        }

        /// <summary>
        /// Extracts the code table name from a table variable
        /// </summary>
        /// <param name="viewTableName">Name of the view table</param>
        /// <param name="tableVariableName">Variable in the 3.x database containing code table information</param>
        /// <returns>DataTable containing the code table's name</returns>
        public virtual DataTable GetCodeTableName(string viewTableName, string tableVariableName)
        {
            throw new System.ApplicationException("Not implemented");
        }

        /// <summary>
        /// Updates a program saved in the database
        /// </summary>
        /// <param name="name">Name of the program</param>
        /// <param name="content">Content of the program</param>
        /// <param name="comment">Comment for the program</param>
        /// <param name="author">Author of the program</param>
        public virtual void UpdatePgm(string name, string content, string comment, string author)
        {
            throw new System.ApplicationException("Not implemented");
        }

        /// <summary>
        /// Creates metadata tables
        /// </summary>
        public void CreateMetadataTables()
        {
            CreateTables();
            PopulateTables();
            InsertProject();
        }

        public void UpdateMetaViewFields(View view)
        {
            try
            {
                if (db.ColumnExists("metaViews", ColumnNames.EWEOrganization_KEY) == false)
                {
                    TableColumn tableEWEOrganizationColumn = new TableColumn(ColumnNames.EWEOrganization_KEY, GenericDbColumnType.StringLong, true);
                    db.AddColumn("metaViews", tableEWEOrganizationColumn);
                }
                if (db.ColumnExists("metaViews", ColumnNames.EiWSOrganization_KEY) == false)
                {
                    TableColumn tableEWEOrganizationKeyColumn = new TableColumn(ColumnNames.EiWSOrganization_KEY, GenericDbColumnType.StringLong, true);
                    db.AddColumn("metaViews", tableEWEOrganizationKeyColumn);
                }
                if (db.ColumnExists("metaViews", ColumnNames.EIWsForm_ID) == false)
                {
                    TableColumn tableEIWSFormIdColumn = new TableColumn(ColumnNames.EIWsForm_ID, GenericDbColumnType.StringLong, true);
                    db.AddColumn("metaViews", tableEIWSFormIdColumn);
                }
                if (db.ColumnExists("metaViews", ColumnNames.EWEForm_ID) == false)
                {
                    TableColumn tableEWEFormIdColumn = new TableColumn(ColumnNames.EWEForm_ID, GenericDbColumnType.StringLong, true);
                    db.AddColumn("metaViews", tableEWEFormIdColumn);
                }
                if (db.ColumnExists("metaFields", ColumnNames.IS_ENCRYPTED) == false)
                {
                    TableColumn isEncryptedColumm = new TableColumn(ColumnNames.IS_ENCRYPTED, GenericDbColumnType.Boolean, false);
                    db.AddColumn("metaFields", isEncryptedColumm);
                }
            }
            catch (Exception ex)
            {
                throw new GeneralException("Unable to update metaViews Table", ex);
            }
        }

        #endregion Public Methods

        #region Static Methods
        /// <summary>
        /// Is version Epi Info 7 Metadata flag.
        /// </summary>
        /// <param name="db">Database driver</param>
        /// <returns>True/False on test of EI7 Metadata.</returns>
        public static bool IsEpi7Metadata(IDbDriver db)
        {
            #region Input Validation
            if (db == null)
            {
                throw new ArgumentNullException("DB");
            }
            #endregion

            try
            {
                // Open the database and look for dbInfo table, find Epi Version ...
                bool isEpi7Metadata = false;
                Query query = db.CreateQuery("select [EpiVersion], [Purpose] from metaDBInfo");

                DataTable results = db.Select(query);
                if (results.Rows.Count > 0)
                {
                    foreach (DataRow row in results.Rows)
                    {
                        isEpi7Metadata = (row["EpiVersion"].ToString().Substring(0, 1) == "7") && ((row["Purpose"].ToString() == "0") || (row["Purpose"].ToString() == "1"));
                    }
                }
                return isEpi7Metadata;
            }
            finally
            {
            }
        }
        #endregion Static Methods

        #region Private Methods
        /// <summary>
        /// Creates the project metadata tables
        /// </summary>
        private void CreateTables()
        {
            this.RemoveDatabase();

            CreateBackgroundsTable();
            CreateDataTypesTable();
            CreateDbInfoTable();
            CreateFieldTypesTable();
            CreateImagesTable();
            CreateLayerRenderTypesTable();
            CreateLayersTable();
            CreateMapsTable();
            CreatePatternsTable();
            CreateProgramsTable();
            CreateViewsTable();
            CreateMapLayersTable();
            CreateMapPointsTable();
            CreatePagesTable();
            CreateFieldsTable();
            CreateGridColumnsTable();
            CreateLinkingTable();
        }

        private void CreateMapPointsTable()
        {
            List<TableColumn> columns = new List<TableColumn>();
            columns.Add(new TableColumn("MapPointId", GenericDbColumnType.Int32, false, true, true));
            columns.Add(new TableColumn("DataSourceTableName", GenericDbColumnType.String, 64, false));
            columns.Add(new TableColumn("DataSourceXCoordinateColumnName", GenericDbColumnType.String, 64, false));
            columns.Add(new TableColumn("DataSourceYCoordinateColumnName", GenericDbColumnType.String, 64, false));
            columns.Add(new TableColumn("DataSourceLabelColumnName", GenericDbColumnType.String, 64, true));
            columns.Add(new TableColumn("Size", GenericDbColumnType.Int32, false));
            columns.Add(new TableColumn("Color", GenericDbColumnType.Int32, false));
            columns.Add(new TableColumn("MapId", GenericDbColumnType.Int32, false, "metaMaps", "MapId"));
            db.CreateTable("metaMapPoints", columns);
        }

        private void CreateMapsTable()
        {
            List<TableColumn> columns = new List<TableColumn>();
            columns.Add(new TableColumn("MapId", GenericDbColumnType.Int32, false, true, true));
            columns.Add(new TableColumn("Name", GenericDbColumnType.String, 64, false));
            columns.Add(new TableColumn("Title", GenericDbColumnType.String, 255, true));
            db.CreateTable("metaMaps", columns);
        }

        private void CreateMapLayersTable()
        {
            List<TableColumn> columns = new List<TableColumn>();
            columns.Add(new TableColumn("MapLayerId", GenericDbColumnType.Int32, false, true, true));
            columns.Add(new TableColumn("RenderField", GenericDbColumnType.String, 64, true));
            columns.Add(new TableColumn("MarkerColor", GenericDbColumnType.Int32, true));
            columns.Add(new TableColumn("RampBeginColor", GenericDbColumnType.Int32, true));
            columns.Add(new TableColumn("RampEndColor", GenericDbColumnType.Int32, true));
            columns.Add(new TableColumn("ClassBreaks", GenericDbColumnType.Int32, true));
            columns.Add(new TableColumn("DataTableName", GenericDbColumnType.String, 64, true));
            columns.Add(new TableColumn("DataTableKey", GenericDbColumnType.String, 64, true));
            columns.Add(new TableColumn("FeatureKey", GenericDbColumnType.String, 64, true));
            columns.Add(new TableColumn("LineColor", GenericDbColumnType.Int32, true));
            columns.Add(new TableColumn("FillColor", GenericDbColumnType.Int32, true));
            columns.Add(new TableColumn("PolygonOutlineColor", GenericDbColumnType.Int32, true));
            columns.Add(new TableColumn("MapId", GenericDbColumnType.Int32, false, "metaMaps", "MapId"));
            columns.Add(new TableColumn("LayerId", GenericDbColumnType.Int32, false, "metaLayers", "LayerId"));
            columns.Add(new TableColumn("LayerRenderTypeId", GenericDbColumnType.Int32, false, "metaLayerRenderTypes", "LayerRenderTypeId"));
            db.CreateTable("metaMapLayers", columns);
        }

        private void CreateLayersTable()
        {
            List<TableColumn> columns = new List<TableColumn>();
            columns.Add(new TableColumn("LayerId", GenericDbColumnType.Int32, false, true, true));
            columns.Add(new TableColumn("Gml", GenericDbColumnType.StringLong, true));
            columns.Add(new TableColumn("GmlSchema", GenericDbColumnType.StringLong, true));
            columns.Add(new TableColumn("Name", GenericDbColumnType.String, 64, false));
            columns.Add(new TableColumn("Description", GenericDbColumnType.String, 255, true));
            columns.Add(new TableColumn("FileName", GenericDbColumnType.String, true));
            db.CreateTable("metaLayers", columns);
        }

        private void CreateLayerRenderTypesTable()
        {
            List<TableColumn> columns = new List<TableColumn>();
            columns.Add(new TableColumn("LayerRenderTypeId", GenericDbColumnType.Int32, false, true, false));
            columns.Add(new TableColumn("Name", GenericDbColumnType.String, 30, false));
            db.CreateTable("metaLayerRenderTypes", columns);
        }

        private void CreateDbInfoTable()
        {
            List<TableColumn> columns = new List<TableColumn>();
            columns.Add(new TableColumn("ProjectId", GenericDbColumnType.Guid, false));
            columns.Add(new TableColumn("ProjectLocation", GenericDbColumnType.String, 255, false));
            columns.Add(new TableColumn("ProjectName", GenericDbColumnType.String, 64, false));
            columns.Add(new TableColumn("EpiVersion", GenericDbColumnType.String, 20, false));
            columns.Add(new TableColumn("Purpose", GenericDbColumnType.Int32, false));
            db.CreateTable("metaDbInfo", columns);
        }

        private void CreateProgramsTable()
        {
            List<TableColumn> columns = new List<TableColumn>();
            columns.Add(new TableColumn("ProgramId", GenericDbColumnType.Int32, false, true, true));
            columns.Add(new TableColumn("Name", GenericDbColumnType.String, 64, false));
            columns.Add(new TableColumn("Content", GenericDbColumnType.StringLong, false));
            columns.Add(new TableColumn("Comment", GenericDbColumnType.StringLong, true));
            columns.Add(new TableColumn("DateCreated", GenericDbColumnType.DateTime, false));
            columns.Add(new TableColumn("DateModified", GenericDbColumnType.DateTime, false));
            columns.Add(new TableColumn("Author", GenericDbColumnType.String, 64, true));
            db.CreateTable("metaPrograms", columns);
        }

        private void CreateDataTypesTable()
        {
            List<TableColumn> columns = new List<TableColumn>();
            columns.Add(new TableColumn("DataTypeId", GenericDbColumnType.Int32, false, true, false));
            columns.Add(new TableColumn("HasPattern", GenericDbColumnType.Boolean, false));
            columns.Add(new TableColumn("HasSize", GenericDbColumnType.Boolean, false));
            columns.Add(new TableColumn("HasRange", GenericDbColumnType.Boolean, false));
            columns.Add(new TableColumn("Name", GenericDbColumnType.String, 30, false));
            db.CreateTable("metaDataTypes", columns);
        }

        private void CreatePatternsTable()
        {
            List<TableColumn> columns = new List<TableColumn>();
            columns.Add(new TableColumn("PatternId", GenericDbColumnType.Int32, false, true, false));
            columns.Add(new TableColumn("Expression", GenericDbColumnType.String, 30, false));
            columns.Add(new TableColumn("Mask", GenericDbColumnType.String, 30, false));
            columns.Add(new TableColumn("FormattedExpression", GenericDbColumnType.String, 30, false));
            columns.Add(new TableColumn("DataTypeId", GenericDbColumnType.Int32, false, "metaDataTypes", "DataTypeId"));

            db.CreateTable("metaPatterns", columns);
        }

        private void CreateFieldTypesTable()
        {
            List<TableColumn> columns = new List<TableColumn>();
            columns.Add(new TableColumn("FieldTypeId", GenericDbColumnType.Int32, false, true, false));
            columns.Add(new TableColumn("Name", GenericDbColumnType.String, 50, false));
            columns.Add(new TableColumn("HasFont", GenericDbColumnType.Boolean, false));
            columns.Add(new TableColumn("HasRepeatLast", GenericDbColumnType.Boolean, false));
            columns.Add(new TableColumn("HasRequired", GenericDbColumnType.Boolean, false));
            columns.Add(new TableColumn("HasReadOnly", GenericDbColumnType.Boolean, false));
            columns.Add(new TableColumn("HasRetainImageSize", GenericDbColumnType.Boolean, false));
            columns.Add(new TableColumn("IsDropDown", GenericDbColumnType.Boolean, false));
            columns.Add(new TableColumn("IsGridColumn", GenericDbColumnType.Boolean, false));
            columns.Add(new TableColumn("IsSystem", GenericDbColumnType.Boolean, false));
            columns.Add(new TableColumn("DefaultPatternId", GenericDbColumnType.Int32, false));
            columns.Add(new TableColumn("DataTypeId", GenericDbColumnType.Int32, false, "metaDataTypes", "DataTypeId"));
            db.CreateTable("metaFieldTypes", columns);
        }

        private void CreateViewsTable()
        {
            List<TableColumn> columns = new List<TableColumn>();
            columns.Add(new TableColumn("ViewId", GenericDbColumnType.Int32, false, true, true));
            columns.Add(new TableColumn("Name", GenericDbColumnType.String, 64, false));
            columns.Add(new TableColumn("IsRelatedView", GenericDbColumnType.Boolean, false));
            columns.Add(new TableColumn("CheckCode", GenericDbColumnType.StringLong, true));
            columns.Add(new TableColumn("CheckCodeBefore", GenericDbColumnType.StringLong, true));
            columns.Add(new TableColumn("CheckCodeAfter", GenericDbColumnType.StringLong, true));
            columns.Add(new TableColumn("RecordCheckCodeBefore", GenericDbColumnType.StringLong, true));
            columns.Add(new TableColumn("RecordCheckCodeAfter", GenericDbColumnType.StringLong, true));
            columns.Add(new TableColumn("CheckCodeVariableDefinitions", GenericDbColumnType.StringLong, true));
            columns.Add(new TableColumn("Width", GenericDbColumnType.Int32, true, false));
            columns.Add(new TableColumn("Height", GenericDbColumnType.Int32, true, false));
            columns.Add(new TableColumn("Orientation", GenericDbColumnType.String, 16, false));
            columns.Add(new TableColumn("LabelAlign", GenericDbColumnType.String, 16, false));
            columns.Add(new TableColumn("EIWSOrganizationKey", GenericDbColumnType.String, 64, true));
            columns.Add(new TableColumn("EIWSFormId", GenericDbColumnType.String, 64, true));
            columns.Add(new TableColumn("EWEOrganizationKey", GenericDbColumnType.String, 64, true));
            columns.Add(new TableColumn("EWEFormId", GenericDbColumnType.String, 64, true));
            db.CreateTable("metaViews", columns);
        }

        private void CreatePagesTable()
        {
            List<TableColumn> columns = new List<TableColumn>();
            columns.Add(new TableColumn("PageId", GenericDbColumnType.Int32, false, true, true));
            columns.Add(new TableColumn("Name", GenericDbColumnType.String, 64, false));
            columns.Add(new TableColumn("Position", GenericDbColumnType.Int16, false));
            columns.Add(new TableColumn("CheckCodeBefore", GenericDbColumnType.StringLong, true));
            columns.Add(new TableColumn("CheckCodeAfter", GenericDbColumnType.StringLong, true));
            columns.Add(new TableColumn("BackgroundId", GenericDbColumnType.Int32, false));
            columns.Add(new TableColumn("ViewId", GenericDbColumnType.Int32, false, "metaViews", "ViewId", true));
            db.CreateTable("metaPages", columns);
        }

        private void CreateLinkingTable()
        {
            List<TableColumn> columns = new List<TableColumn>();
            columns.Add(new TableColumn("LinkId", GenericDbColumnType.Int32, false, true, true));
            columns.Add(new TableColumn("FromRecordGuid", GenericDbColumnType.String, 255, false));
            columns.Add(new TableColumn("ToRecordGuid", GenericDbColumnType.String, 255, false));
            columns.Add(new TableColumn("FromViewId", GenericDbColumnType.Int32, false));
            columns.Add(new TableColumn("ToViewId", GenericDbColumnType.Int32, false));
            db.CreateTable("metaLinks", columns);
        }

        private void CreateFieldsTable()
        {
            List<TableColumn> columns = new List<TableColumn>();
            columns.Add(new TableColumn("FieldId", GenericDbColumnType.Int32, false, true, true));
            columns.Add(new TableColumn("UniqueId", GenericDbColumnType.Guid, true));
            columns.Add(new TableColumn("Name", GenericDbColumnType.String, 64, false));
            columns.Add(new TableColumn("PromptText", GenericDbColumnType.StringLong, true));
            columns.Add(new TableColumn("ControlFontFamily", GenericDbColumnType.String, 127, true));
            columns.Add(new TableColumn("ControlFontStyle", GenericDbColumnType.String, 50, true));
            columns.Add(new TableColumn("ControlFontSize", GenericDbColumnType.Decimal, 2, true));
            columns.Add(new TableColumn("ControlTopPositionPercentage", GenericDbColumnType.Double, true));
            columns.Add(new TableColumn("ControlLeftPositionPercentage", GenericDbColumnType.Double, true));
            columns.Add(new TableColumn("ControlHeightPercentage", GenericDbColumnType.Double, true));
            columns.Add(new TableColumn("ControlWidthPercentage", GenericDbColumnType.Double, true));
            columns.Add(new TableColumn("TabIndex", GenericDbColumnType.Decimal, true));
            columns.Add(new TableColumn("HasTabStop", GenericDbColumnType.Boolean, true));
            columns.Add(new TableColumn("PromptFontFamily", GenericDbColumnType.String, 127, true));
            columns.Add(new TableColumn("PromptFontStyle", GenericDbColumnType.String, 50, true));
            columns.Add(new TableColumn("PromptFontSize", GenericDbColumnType.Decimal, 2, true));
            columns.Add(new TableColumn("PromptScriptName", GenericDbColumnType.String, 20, true));
            columns.Add(new TableColumn("PromptTopPositionPercentage", GenericDbColumnType.Double, true));
            columns.Add(new TableColumn("PromptLeftPositionPercentage", GenericDbColumnType.Double, true));
            columns.Add(new TableColumn("ControlScriptName", GenericDbColumnType.String, 20, true));
            columns.Add(new TableColumn("ShouldRepeatLast", GenericDbColumnType.Boolean, true));
            columns.Add(new TableColumn("IsRequired", GenericDbColumnType.Boolean, true));
            columns.Add(new TableColumn("IsReadOnly", GenericDbColumnType.Boolean, true));
            columns.Add(new TableColumn("IsEncrypted", GenericDbColumnType.Boolean, true));
            columns.Add(new TableColumn("ShouldRetainImageSize", GenericDbColumnType.Boolean, true));
            columns.Add(new TableColumn("MaxLength", GenericDbColumnType.Int16, true));
            columns.Add(new TableColumn("Lower", GenericDbColumnType.String, 20, true));
            columns.Add(new TableColumn("Upper", GenericDbColumnType.String, 20, true));
            columns.Add(new TableColumn("Pattern", GenericDbColumnType.String, 50, true));
            columns.Add(new TableColumn("ShowTextOnRight", GenericDbColumnType.Boolean, true));
            columns.Add(new TableColumn("CheckCodeBefore", GenericDbColumnType.StringLong, true));
            columns.Add(new TableColumn("CheckCodeAfter", GenericDbColumnType.StringLong, true));
            columns.Add(new TableColumn("RelateCondition", GenericDbColumnType.StringLong, true));
            columns.Add(new TableColumn("ShouldReturnToParent", GenericDbColumnType.Boolean, true));
            columns.Add(new TableColumn("SourceTableName", GenericDbColumnType.String, 255, true));
            columns.Add(new TableColumn("CodeColumnName", GenericDbColumnType.String, 255, true));
            columns.Add(new TableColumn("TextColumnName", GenericDbColumnType.String, 255, true));
            columns.Add(new TableColumn("Sort", GenericDbColumnType.Boolean, true));
            columns.Add(new TableColumn("IsExclusiveTable", GenericDbColumnType.Boolean, true));
            columns.Add(new TableColumn("DataTableName", GenericDbColumnType.String, 64, true));
            columns.Add(new TableColumn("SourceFieldId", GenericDbColumnType.Int32, true));
            columns.Add(new TableColumn("FieldTypeId", GenericDbColumnType.Int32, false, "metaFieldTypes", "FieldTypeId"));
            columns.Add(new TableColumn("RelatedViewId", GenericDbColumnType.Int32, true, "metaViews", "ViewId"));
            columns.Add(new TableColumn("List", GenericDbColumnType.StringLong, true));
            columns.Add(new TableColumn("BackgroundColor", GenericDbColumnType.Int32, true));
            columns.Add(new TableColumn("ViewId", GenericDbColumnType.Int32, false, "metaViews", "ViewId", true));
            columns.Add(new TableColumn("PageId", GenericDbColumnType.Int32, true, "metaPages", "PageId"));
            db.CreateTable("metaFields", columns);
        }

        private void CreateGridColumnsTable()
        {
            List<TableColumn> columns = new List<TableColumn>();
            columns.Add(new TableColumn("GridColumnId", GenericDbColumnType.Int32, false, true, true));
            columns.Add(new TableColumn("Name", GenericDbColumnType.String, 64, false));
            columns.Add(new TableColumn("Size", GenericDbColumnType.Int16, true));
            columns.Add(new TableColumn("Position", GenericDbColumnType.Int16, false));
            columns.Add(new TableColumn("Text", GenericDbColumnType.String, 64, false));
            columns.Add(new TableColumn("ShouldRepeatLast", GenericDbColumnType.Boolean, false));
            columns.Add(new TableColumn("IsRequired", GenericDbColumnType.Boolean, true));
            columns.Add(new TableColumn("IsReadOnly", GenericDbColumnType.Boolean, true));
            columns.Add(new TableColumn("Pattern", GenericDbColumnType.String, 64, true));
            columns.Add(new TableColumn("Upper", GenericDbColumnType.String, 20, true));
            columns.Add(new TableColumn("Lower", GenericDbColumnType.String, 20, true));
            columns.Add(new TableColumn("Width", GenericDbColumnType.Int32, false));
            columns.Add(new TableColumn("SourceTableName", GenericDbColumnType.String, 50, true));
            columns.Add(new TableColumn("CodeColumnName", GenericDbColumnType.String, 50, true));
            columns.Add(new TableColumn("TextColumnName", GenericDbColumnType.String, 50, true));
            columns.Add(new TableColumn("Sort", GenericDbColumnType.Boolean, true));
            columns.Add(new TableColumn("IsExclusiveTable", GenericDbColumnType.Boolean, true));
            columns.Add(new TableColumn("DataTableName", GenericDbColumnType.String, 64, true));
            columns.Add(new TableColumn("FieldId", GenericDbColumnType.Int32, false, false, false));
            columns.Add(new TableColumn("FieldTypeId", GenericDbColumnType.Int32, false, false, false));
            columns.Add(new TableColumn("IsUniqueField", GenericDbColumnType.Boolean, true));

            //columns.Add(new TableColumn("FieldId", GenericDbColumnType.Int32, false, "metaFields", "FieldId", true));  
            //columns.Add(new TableColumn("FieldTypeId", GenericDbColumnType.Int32, false, "metaFieldTypes", "FieldTypeId"));  
 
            db.CreateTable("metaGridColumns", columns);
        }

        /// <summary>
        /// Create Background Table
        /// </summary>
        public void CreateBackgroundsTable()
        {
            List<TableColumn> columns = new List<TableColumn>();
            columns.Add(new TableColumn("BackgroundId", GenericDbColumnType.Int32, false, true, true));
            columns.Add(new TableColumn("ImageId", GenericDbColumnType.Int32, true));
            columns.Add(new TableColumn("ImageLayout", GenericDbColumnType.String, 10, true));
            columns.Add(new TableColumn("Color", GenericDbColumnType.Int32, true, false));
            db.CreateTable("metaBackgrounds", columns);
        }

        /// <summary>
        /// Populates the project metadata tables
        /// </summary>
        private void PopulateTables()
        {
            Query query = null;

            try
            {
                foreach (AppDataSet.DataTypesRow dataType in AppData.Instance.DataTypesDataTable.Rows)
                {
                    query = db.CreateQuery("insert into metaDataTypes ([DataTypeId], [Name], [HasPattern], [HasSize], [HasRange]) values (@DataTypeId, @Name, @HasPattern, @HasSize, @HasRange)");
                    query.Parameters.Add(new QueryParameter("@DataTypeId", DbType.Int32, dataType.DataTypeId));
                    query.Parameters.Add(new QueryParameter("@Name", DbType.String, dataType.Name));
                    query.Parameters.Add(new QueryParameter("@HasPattern", DbType.Boolean, dataType.HasPattern));
                    query.Parameters.Add(new QueryParameter("@HasSize", DbType.Boolean, dataType.HasSize));
                    query.Parameters.Add(new QueryParameter("@HasRange", DbType.Boolean, dataType.HasRange));

                    db.ExecuteNonQuery(query);
                }

                foreach (AppDataSet.FieldTypesRow fieldType in AppData.Instance.FieldTypesDataTable.Rows)
                {
                    query = db.CreateQuery("insert into metaFieldTypes ([FieldTypeId], [Name], [HasRepeatLast], [HasRequired], [HasReadOnly], [HasRetainImageSize], [HasFont], [IsDropDown], [IsGridColumn], [DataTypeId], [IsSystem], [DefaultPatternId]) values (@FieldTypeId, @Name, @HasRepeatLast, @HasRequired, @HasReadOnly, @HasRetainImageSize, @HasFont, @IsDropDown, @IsGridColumn, @DataTypeId, @IsSystem, @DefaultPatternId)");
                    query.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, fieldType.FieldTypeId));
                    query.Parameters.Add(new QueryParameter("@Name", DbType.String, fieldType.Name));
                    query.Parameters.Add(new QueryParameter("@HasRepeatLast", DbType.Boolean, fieldType.HasRepeatLast));
                    query.Parameters.Add(new QueryParameter("@HasRequired", DbType.Boolean, fieldType.HasRequired));
                    query.Parameters.Add(new QueryParameter("@HasReadOnly", DbType.Boolean, fieldType.HasReadOnly));
                    query.Parameters.Add(new QueryParameter("@HasRetainImageSize", DbType.Boolean, fieldType.HasRetainImageSize));
                    query.Parameters.Add(new QueryParameter("@HasFont", DbType.Boolean, fieldType.HasFont));
                    query.Parameters.Add(new QueryParameter("@IsDropDown", DbType.Boolean, fieldType.IsDropDown));
                    query.Parameters.Add(new QueryParameter("@IsGridColumn", DbType.Boolean, fieldType.IsGridColumn));
                    if (fieldType.IsDataTypeIdNull())
                        query.Parameters.Add(new QueryParameter("@DataTypeId", DbType.Int32, DBNull.Value));
                    else
                        query.Parameters.Add(new QueryParameter("@DataTypeId", DbType.Int32, fieldType.DataTypeId));
                    query.Parameters.Add(new QueryParameter("@IsSystem", DbType.Boolean, fieldType.IsSystem));
                    query.Parameters.Add(new QueryParameter("@DefaultPatternId", DbType.Int32, fieldType.DefaultPatternId));

                    db.ExecuteNonQuery(query);
                }

                foreach (AppDataSet.DataPatternsRow pattern in AppData.Instance.DataPatternsDataTable.Rows)
                {
                    query = db.CreateQuery("insert into metaPatterns ([PatternId], [DataTypeId], [Expression], [Mask], [FormattedExpression]) values (@PatternId, @DataTypeId, @Expression, @Mask, @FormattedExpression)");
                    query.Parameters.Add(new QueryParameter("@PatternId", DbType.Int32, pattern.PatternId));
                    query.Parameters.Add(new QueryParameter("@DataTypeId", DbType.Int32, pattern.DataTypeId));
                    query.Parameters.Add(new QueryParameter("@Expression", DbType.String, pattern.Expression));
                    query.Parameters.Add(new QueryParameter("@Mask", DbType.String, pattern.Mask));
                    query.Parameters.Add(new QueryParameter("@FormattedExpression", DbType.String, pattern.FormattedExpression));

                    db.ExecuteNonQuery(query);
                }

                foreach (AppDataSet.LayerRenderTypesRow layerRenderType in AppData.Instance.LayerRenderTypesDataTable.Rows)
                {
                    query = db.CreateQuery("insert into metaLayerRenderTypes ([LayerRenderTypeId], [Name]) values (@LayerRenderTypeId, @Name)");
                    query.Parameters.Add(new QueryParameter("@LayerRenderTypeId", DbType.Int32, layerRenderType.LayerRenderTypeId));
                    query.Parameters.Add(new QueryParameter("@Name", DbType.String, layerRenderType.Name));
                    
                    db.ExecuteNonQuery(query);
                }
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not populate project tables", ex);
            }
            finally
            {
                query = null;
            }
        }

        /// <summary>
        /// Inserts the project parameters into the metadata database
        /// </summary>		
        private void InsertProject()
        {
            try
            {
                ApplicationIdentity appId = new ApplicationIdentity(typeof(Configuration).Assembly);

                Query query = db.CreateQuery("insert into metaDBInfo([ProjectId], [ProjectName], [ProjectLocation], [EpiVersion], [Purpose]) values (@ProjectId, @ProjectName, @ProjectLocation, @EpiVersion, @Purpose)");
                query.Parameters.Add(new QueryParameter("@ProjectId", DbType.Guid, this.Project.Id));
                query.Parameters.Add(new QueryParameter("@ProjectName", DbType.String, Project.Name));
                query.Parameters.Add(new QueryParameter("@ProjectLocation", DbType.String, Project.Location));
                query.Parameters.Add(new QueryParameter("@EpiVersion", DbType.String, appId.Version));

                if (Project.MetadataSource == MetadataSource.SameDb)
                    query.Parameters.Add(new QueryParameter("@Purpose", DbType.Int32, (int)DatabasePurpose.MetadataAndCollectedData));
                else
                    query.Parameters.Add(new QueryParameter("@Purpose", DbType.Int32, (int)DatabasePurpose.Metadata));

                db.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create new project: " + ex.ToString(), ex);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Extract prompt font from field row data.
        /// </summary>
        /// <param name="fieldRow">Field Row with prompt font data.</param>
        /// <returns>Font family name, size, and style.</returns>
        public Font ExtractPromptFont(DataRow fieldRow)
        {
            string fontFamilyName = Defaults.FontFamilyName;
            float fontSize = Defaults.FontSize; ;
            FontStyle fontStyle = Defaults.FontStyle;
            if (!(fieldRow["PromptFontFamily"] is DBNull))
            {
                fontFamilyName = fieldRow["PromptFontFamily"].ToString();
            }
            if (!(fieldRow["PromptFontSize"] is DBNull))
            {
                fontSize = float.Parse(fieldRow["PromptFontSize"].ToString());
            }
            if (!(fieldRow["PromptFontStyle"] is DBNull))
            {
                fontStyle = (FontStyle)System.Enum.Parse(typeof(FontStyle), fieldRow["PromptFontStyle"].ToString());
            }

            Font font = new Font(fontFamilyName, fontSize, fontStyle);
            return font;
        }

        #endregion

        #region Protected Methods
        // <summary>
        // Creates metadata tables
        // </summary>

        #endregion Protected Methods

        /// <summary>
        /// Creates a page in XML metadata
        /// </summary>
        /// <param name="view">The view the page belongs to</param>        
        /// <param name="name">The name of the page</param>
        /// <param name="position">The position of the page</param>
        public Page CreatePage(View view, string name, int position)
        {
            return null;
        }

        #region Public XMLNode Field Data methods
        /// <summary>
        /// Retrieves data for check box field from xml metadata.
        /// </summary>
        /// <param name="field">A check box field</param>
        /// <param name="fieldNode">XML node for check box field.</param>
        public void GetFieldData(CheckBoxField field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Retrieves data for command button field from xml metadata.
        /// </summary>
        /// <param name="field">A command button field</param>
        /// <param name="fieldNode">XML node for command button field.</param>
        public void GetFieldData(CommandButtonField field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Retrieves data for date field from xml metadata.
        /// </summary>
        /// <param name="field">A date field </param>
        /// <param name="fieldNode">XML node for date field.</param>
        public void GetFieldData(DateField field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Retrieves data for date time field from xml metadata.
        /// </summary>
        /// <param name="field">A date time field</param>
        /// <param name="fieldNode">XML node for date time field.</param>
        public void GetFieldData(DateTimeField field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Retrieves data for DDL Field of Codes from xml metadata.
        /// </summary>
        /// <param name="field">A DDL Field of Codes</param>
        /// <param name="fieldNode">XML node for DDL Field of Codes.</param>
        public void GetFieldData(DDLFieldOfCodes field, XmlNode fieldNode)
        {
        }


        /// <summary>
        /// Retrieves data for DDL Field of Codes from xml metadata.
        /// </summary>
        /// <param name="field">A DDL Field of Codes</param>
        /// <param name="fieldNode">XML node for DDL Field of Codes.</param>
        public void GetFieldData(DDListField field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Retrieves data for DDL Field of Comment Legal from xml metadata.
        /// </summary>
        /// <param name="field">A DDL Field of Comment Legal</param>
        /// <param name="fieldNode">XML node for DDL Field of Comment Legal.</param>
        public void GetFieldData(DDLFieldOfCommentLegal field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Retrieves data for DDL Field of Legal Values from xml metadata.
        /// </summary>
        /// <param name="field">A DDL Field of Legal Values</param>
        /// <param name="fieldNode">XML node for DDL Field of Legal Values.</param>
        public void GetFieldData(DDLFieldOfLegalValues field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Retrieves data for foreign key field from xml metadata.
        /// </summary>
        /// <param name="field">A foreign key field.</param>
        /// <param name="fieldNode">XML node for a foreign field.</param>
        public void GetFieldData(ForeignKeyField field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Retrieves data for grid field from xml metadata.
        /// </summary>
        /// <param name="field">A grid field.</param>
        /// <param name="fieldNode">XML node for grid field.</param>
        public void GetFieldData(GridField field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Retrieves data for GUID field from xml metadata.
        /// </summary>
        /// <param name="field">A GUID field.</param>
        /// <param name="fieldNode">XML node for GUID field.</param>
        public void GetFieldData(GUIDField field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Retrieves data for image field from xml metadata.
        /// </summary>
        /// <param name="field">An image field</param>
        /// <param name="fieldNode">XML node for image field.</param>
        public void GetFieldData(ImageField field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Retrieves data for label field from xml metadata.
        /// </summary>
        /// <param name="field">A label field</param>
        /// <param name="fieldNode">XML node for label field.</param>
        public void GetFieldData(LabelField field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Retrieves data for mirror field from xml metadata.
        /// </summary>
        /// <param name="field">A mirror field.</param>
        /// <param name="fieldNode">XML node for mirror field.</param>
        public void GetFieldData(MirrorField field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Retrieves data for multiline text field from xml metadata.
        /// </summary>
        /// <param name="field">A multiline text field.</param>
        /// <param name="fieldNode">XML node for multiline field.</param>
        public void GetFieldData(MultilineTextField field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Retrieves data for number field from xml metadata.
        /// </summary>
        /// <param name="field">A number field.</param>
        /// <param name="fieldNode">XML node for number field.</param>
        public void GetFieldData(NumberField field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Retrieves data for option field from xml metadata.
        /// </summary>
        /// <param name="field">An option field.</param>
        /// <param name="fieldNode">XML node for option field.</param>
        public void GetFieldData(OptionField field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Retrieves data for phone number field from xml metadata.
        /// </summary>
        /// <param name="field">A phone number field.</param>
        /// <param name="fieldNode">XML node for phone number field.</param>
        public void GetFieldData(PhoneNumberField field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Retrieves data for rec status field from xml metadata.
        /// </summary>
        /// <param name="field">A rec status field.</param>
        /// <param name="fieldNode">XML node for rec status field.</param>
        public void GetFieldData(RecStatusField field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Retrieves data for related view field from xml metadata.
        /// </summary>
        /// <param name="field">A related view field.</param>
        /// <param name="fieldNode">XML node for related view field.</param>
        public void GetFieldData(RelatedViewField field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Retrieves data for single line text field from xml metadata.
        /// </summary>
        /// <param name="field">A single line text field.</param>
        /// <param name="fieldNode">XML node for single line text field.</param>
        public void GetFieldData(SingleLineTextField field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Retrieves data for time field from xml metadata.
        /// </summary>
        /// <param name="field">A time field.</param>
        /// <param name="fieldNode">XML node for time field.</param>
        public void GetFieldData(TimeField field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Retrieves data for unique key field from xml metadata.
        /// </summary>
        /// <param name="field">A unique key field.</param>
        /// <param name="fieldNode">XML node for unique key field.</param>
        public void GetFieldData(UniqueKeyField field, XmlNode fieldNode)
        {
        }
        /// <summary>
        /// Retrieves data for UniqueKeyField field 
        /// </summary>
        /// <param name="field">A unique key field.</param>
        /// <param name="fieldNode">XML node for unique key field.</param>
        public void GetFieldData(UniqueIdentifierField field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Retrieves data for upper case text field from xml metadata.
        /// </summary>
        /// <param name="field">An upper case text field.</param>
        /// <param name="fieldNode">XML node for upper case text field.</param>
        public void GetFieldData(UpperCaseTextField field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Gets a code table list.
        /// </summary>
        /// <returns>Code data tables.</returns>
        DataSets.TableSchema.TablesDataTable IMetadataProvider.GetCodeTableList()
        {
            try
            {
                DataSets.TableSchema.TablesDataTable tables = db.GetCodeTableList(db);

                //query.Parameters.Add(new DbParameter("@ProjectId", DbType.Guid, Project.Id));			
                // return db.Select(query);
                return tables;

            }
            catch (Exception ex)
            {
                throw new System.ApplicationException("Could not retrieve code tables from database", ex);
            }
        }

        /// <summary>
        /// Retrieves data for yes no field from xml metadata.
        /// </summary>
        /// <param name="field">A yes no field</param>
        /// <param name="fieldNode">XML node for yes no field.</param>
        public void GetFieldData(YesNoField field, XmlNode fieldNode)
        {
        }

        /// <summary>
        /// Get the background data associated with the given page
        /// </summary>
        /// <param name="page">page</param>
        /// <returns>DataTable</returns>
        public DataTable GetPageBackgroundData(Page page)
        {
            DataTable returnTable = new DataTable();
            returnTable.Columns.Add("BackgroundId", System.Type.GetType("System.Int32"));
            returnTable.Columns.Add("Image", System.Type.GetType("System.Byte[]"));
            returnTable.Columns.Add("ImageId", System.Type.GetType("System.Int32"));
            returnTable.Columns.Add("ImageLayout", System.Type.GetType("System.String"));
            returnTable.Columns.Add("Color", System.Type.GetType("System.Int32"));
            DataRow row = returnTable.NewRow();
            
            Query selectBackgroundQuery = db.CreateQuery("select * from [metaBackgrounds] where [BackgroundId] = @BackgroundId");
            selectBackgroundQuery.Parameters.Add(new QueryParameter("@BackgroundId", DbType.Int32, page.BackgroundId));

            IDataReader backgroundReader = db.ExecuteReader(selectBackgroundQuery);
            if (backgroundReader.Read())
            {
                row["BackgroundId"] = backgroundReader.GetInt32(0);
                row["ImageId"] = backgroundReader.GetInt32(1);
                row["ImageLayout"] = backgroundReader.GetString(2);
                row["Color"] = backgroundReader.GetInt32(3);

                if ((int)row["ImageId"] > -1)
                {

                    BinaryWriter writer;
                    int bufferSize = 256;
                    byte[] outByte = new byte[bufferSize];
                    long retval;
                    long startIndex = 0;

                    Query selectImageQuery = db.CreateQuery("select * from [metaImages] where [ImageId]=@ImageId");
                    selectImageQuery.Parameters.Add(new QueryParameter("@ImageId", DbType.Int32, row["ImageId"]));
                    IDataReader reader = db.ExecuteReader(selectImageQuery, CommandBehavior.SequentialAccess);

                    while (reader.Read())
                    {
                        MemoryStream memStream = new MemoryStream();
                        writer = new BinaryWriter(memStream);

                        startIndex = 0;
                        retval = reader.GetBytes(1, startIndex, outByte, 0, bufferSize);

                        while (retval == bufferSize)
                        {
                            writer.Write(outByte);
                            startIndex += bufferSize;
                            retval = reader.GetBytes(1, startIndex, outByte, 0, bufferSize);
                        }

                        int count = (int)retval - 1;
                        if (count > 0)
                        {
                            writer.Write(outByte, 0, count);
                        }
                        memStream.Position = 0;

                        byte[] imageOut = new byte[(int)memStream.Length];
                        imageOut = memStream.ToArray();
                        row["Image"] = imageOut;

                        writer.Close();
                        memStream.Close();
                    }
                }
            }
            returnTable.Rows.Add(row);
            return returnTable;
        }

        /// <summary>
        /// Get Background Data
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable GetBackgroundData()
        {
            DataTable dt=null;
            Query selectQuery = db.CreateQuery("select * from [metaBackgrounds]");
            dt = db.Select(selectQuery);
            return dt;
        }

        /// <summary>
        /// Get Data Dictionary
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable GetDataDictionary(View view)
        {
            DataTable fieldsTable = null;
            Query fieldsQuery = db.CreateQuery("SELECT " +
                "metaPages.[Position] AS [Page Position], " +
                "metaPages.Name AS [Page Name], " +
                "metaFields.TabIndex AS [Tab Index], " +
                "metaFields.PromptText AS [Prompt], " +
                "metaFieldTypes.Name AS [Field Type], " +
                "metaFields.Name AS [Name], " +
                "metaDataTypes.Name AS [Variable Type], " +
                "metaFields.Pattern AS [Format], " +
                "metaFields.Isrequired, " +
                "metafields.IsReadOnly,  " +
                "metafields.ShouldRepeatLast, " +
                "metafields.ShouldRetainImageSize, " +
                "metafields.[lower],  " + 
                "metafields.[upper],  " +
                "metaFields.List AS [Special Info] " +
                "FROM ((metaFields  " +
                "INNER JOIN metaFieldTypes ON metaFields.FieldTypeId = metaFieldTypes.FieldTypeId ) " +
                "INNER JOIN metaDataTypes ON metaFieldTypes.DataTypeId = metaDataTypes.DataTypeId ) " +
                "INNER JOIN metaPages ON metaFields.PageId = metaPages.PageId  " +
                "WHERE metaFields.ViewId=@ViewId " +
                "AND metaFields.Name<>'UniqueKey' AND metaFields.Name<>'RecStatus'");
            fieldsQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, view.Id));
            fieldsTable = db.Select(fieldsQuery);
            return fieldsTable;
        }

        /// <summary>
        /// Insert Page Background Data
        /// </summary>
        /// <param name="page">page</param>
        /// <param name="imageId">primary key of metaImages</param>
        /// <param name="imageLayout">image layout type</param>
        /// <param name="color">color</param>
        public void InsertPageBackgroundData(Page page, Int32 imageId, string imageLayout, Int32 color)
        {
            Query insertQuery = db.CreateQuery
            (
                "insert into [metaBackgrounds] ([ImageId],[ImageLayout],[Color]) " +
                "VALUES (@ImageId,@ImageLayout,@Color)"
            );

            insertQuery.Parameters.Add(new QueryParameter("@ImageId", DbType.Int32, imageId));
            insertQuery.Parameters.Add(new QueryParameter("@ImageLayout", DbType.String, imageLayout));
            insertQuery.Parameters.Add(new QueryParameter("@Color", DbType.Int32, color));

            db.ExecuteNonQuery(insertQuery);
            page.BackgroundId = GetMaxBackgroundId();
            UpdatePage(page);
        }

        /// <summary>
        /// Update the page's background data
        /// </summary>
        /// <param name="page">page</param>
        /// <param name="imagePath">file path of the image</param>
        /// <param name="imageLayout">image layout</param>
        /// <param name="color">color</param>
        public void UpdatePageBackgroundData(Page page, string imagePath, string imageLayout, int color)
        {
            byte[] imageAsBytes;
            int imageId = -1;

            if (imagePath != null && imagePath != string.Empty)
            {
                imageAsBytes = Util.GetByteArrayFromImagePath(imagePath);
                int imageByteCount = imageAsBytes.Length;
                imageId = GetImageId(imageByteCount);

                if (imageId < 0)
                {
                    InsertMetaImage(imageAsBytes);
                    imageId = GetImageId(imageByteCount);
                }
            }
            
            StringBuilder queryString = new StringBuilder();
            queryString.Append("update [metaBackgrounds] set ");
            queryString.Append("[ImageId]=@ImageId, "); 
            queryString.Append("[ImageLayout]=@ImageLayout, [Color]=@Color where [BackgroundId]=@BackgroundId");
            
            Query updateQuery = db.CreateQuery(queryString.ToString());
            updateQuery.Parameters.Add(new QueryParameter("@ImageId", DbType.Int32, imageId));
            updateQuery.Parameters.Add(new QueryParameter("@ImageLayout", DbType.String, imageLayout));
            updateQuery.Parameters.Add(new QueryParameter("@Color", DbType.Int32, color));
            updateQuery.Parameters.Add(new QueryParameter("@BackgroundId", DbType.Int32, page.BackgroundId));

            int recordsAffected = db.ExecuteNonQuery(updateQuery);
            if (recordsAffected == 0)
            {
                InsertPageBackgroundData(page, imageId, imageLayout, color);
            }

            UpdatePage(page);
        }

        /// <summary>
        /// Update Page Setup Data
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="width">Width of the page in pixels.</param>
        /// <param name="height">Height of the page in pixels.</param>
        /// <param name="orientation">Orientation: Protrait/Landscape</param>
        /// <param name="labelAlign">The label-field alignment.</param>
        /// <param name="targetMedium">The target viewing medium of the questionare: Paper/Monitor</param>
        public void UpdatePageSetupData(View view, int width, int height, string orientation, string labelAlign, string targetMedium)
        {
            string queryString = "update metaViews set [Width]=@Width, [Height]=@Height, [Orientation]=@Orientation, [LabelAlign]=@LabelAlign where [ViewId]=@ViewId";
            Query updateQuery = db.CreateQuery(queryString);
            updateQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, width));
            updateQuery.Parameters.Add(new QueryParameter("@Height", DbType.Int32, height));
            updateQuery.Parameters.Add(new QueryParameter("@Orientation", DbType.String, orientation));
            updateQuery.Parameters.Add(new QueryParameter("@LabelAlign", DbType.String, labelAlign));
            updateQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, view.Id));

            int recordsAffected = db.ExecuteNonQuery(updateQuery);
        }

        /// <summary>
        /// Get Page Setup Data
        /// </summary>
        /// <returns>DataTable</returns>
        public DataRow GetPageSetupData(View view)
        {
            DataTable dt = null;
            Query selectQuery = db.CreateQuery("select * from metaViews where [ViewId]=@ViewId");
            selectQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, view.Id));
            dt = db.Select(selectQuery);
            return dt.Rows[0];
        }

        public Int32 InsertMetaImage(byte[] imageAsBytes)
        {
            Query insertQuery = db.CreateQuery
            (
                "insert into [metaImages] ([Image],[ImageUniqueValue]) " +
                "VALUES (@Image,@ImageUniqueValue)"
            );

            int compressedHash = ComputeCompressedHashFromImage(imageAsBytes);

            Data.QueryParameter imageParam = new QueryParameter("@Image", DbType.Object, imageAsBytes);
            imageParam.Size = imageAsBytes.Length + 1;
            insertQuery.Parameters.Add(imageParam );
            insertQuery.Parameters.Add(new QueryParameter("@ImageUniqueValue", DbType.Int32, compressedHash));
            db.ExecuteNonQuery(insertQuery);
            return GetMaxImageId();
        }

        private static int ComputeCompressedHashFromImage(byte[] imageAsBytes)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(imageAsBytes);

            int compressedHash = 1;
            foreach (byte item in hash)
            {
                compressedHash = compressedHash * item;
            }
            return compressedHash;
        }

        public Int32 InsertMetaImage(string imagePath)
        {
            byte[] imageAsBytes;
            int imageId = -1;

            if (imagePath != null && imagePath != string.Empty)
            {
                imageAsBytes = Util.GetByteArrayFromImagePath(imagePath);
                int compressedHash = ComputeCompressedHashFromImage(imageAsBytes);
                imageId = GetImageId(compressedHash);

                if (imageId < 0)
                {
                    InsertMetaImage(imageAsBytes);
                    imageId = GetImageId(compressedHash);
                }
            }
            return imageId;
        }

        private bool BackgroundThemeExists(Int32 imageId, string imageLayout, int color)
        {
            Query selectQuery = db.CreateQuery("SELECT * FROM [metaBackgrounds] WHERE [ImageId]=@ImageId AND [ImageLayout]=@ImageLayout AND [Color]=@Color");
            selectQuery.Parameters.Add(new QueryParameter("@ImageId", DbType.Int32, imageId));
            selectQuery.Parameters.Add(new QueryParameter("@ImageLayout", DbType.String, imageLayout));
            selectQuery.Parameters.Add(new QueryParameter("@Color", DbType.Int32, color));
            DataTable table = db.Select(selectQuery);
            if (table != null && table.Rows.Count > 0)
            {
                return true;
            }
            return false;
        }

        private Int32 GetImageId(Int32 imageUniqueValue)
        {
            DataTable table = null;
            Int32 imageId = -1;
            Query selectQuery = db.CreateQuery("SELECT [ImageId] FROM [metaImages] WHERE ([ImageUniqueValue]=@ImageUniqueValue)");
            selectQuery.Parameters.Add(new QueryParameter("@ImageUniqueValue", DbType.Int32, imageUniqueValue));
            table = db.Select(selectQuery);
            if(table.Rows.Count > 0)
            {
                imageId = (Int32)table.Rows[0]["ImageId"];
            }
            return imageId;
        }

        #endregion

        #region IMetadataProvider Members
        /// <summary>
        /// Gets a list of code tables.
        /// </summary>
        /// <param name="db">Database driver.</param>
        /// <returns>List of code tables.</returns>
        public TableSchema.TablesDataTable GetCodeTableList(IDbDriver db)
        {
            return db.GetCodeTableList(db);
        }

        /// <summary>
        /// Gets code table names for the current <see cref="Epi.Project"/>.
        /// </summary>
        /// <param name="project"><see cref="Epi.Project"/></param>
        /// <returns>Names of code tables in a <see cref="System.Data.DataTable"/>.</returns>
        public DataTable GetCodeTableNamesForProject(Project project)
        {
            return this.db.GetCodeTableNamesForProject(project);
        }

        /// <summary>
        /// Identifies which database system is in use.
        /// </summary>
        /// <returns>Friendly name of DBMS.</returns>
        public string IdentifyDatabase()
        {
            return this.db.IdentifyDatabase();
        }
        #endregion
    }
}


