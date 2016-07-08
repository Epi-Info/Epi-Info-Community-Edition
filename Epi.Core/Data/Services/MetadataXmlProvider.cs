using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using Epi.Collections;
using Epi;
using Epi.Data;
using Epi.DataSets;
using Epi.Fields;
using Epi.Resources;

namespace Epi.Data.Services
{

    /// <summary>
    /// Xml implementation of Metadata provider
    /// </summary>
    public class MetadataXmlProvider : IMetadataProvider
    {
        #region MetadataProvider Database Members
        /// <summary>
        /// Get the schema for a code table
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <returns>DataTable</returns>
        [Obsolete("Use of DataTable in this context is no different than the use of a multidimensional System.Object array (not recommended).", false)]
        public DataTable GetCodeTableColumnSchema(string tableName)
        {
            Util.Assert(tableName.StartsWith("meta") == false);
            return db.GetTableColumnSchema(tableName);
        }

        /// <summary>
        /// Get the data for a code table
        /// </summary>
        /// <param name="tableName">Code table name</param>
        /// <returns>DataTable</returns>
        public DataTable GetCodeTableData(string tableName)
        {
            Util.Assert(tableName.StartsWith("meta") == false);
            return db.GetTableData(tableName);
        }

        /// <summary>
        /// Get code table data for given columns
        /// </summary>
        /// <param name="tableName">Code table name</param>
        /// <param name="columnNames">Column names</param>
        /// <returns>DataTable</returns>
        public DataTable GetCodeTableData(string tableName, string columnNames)
        {
            Util.Assert(tableName.StartsWith("meta") == false);
            return this.db.GetTableData(tableName, columnNames);
        }

        /// <summary>
        /// Get code table data for given columns
        /// </summary>
        /// <param name="tableName">Code table name</param>
        /// <param name="columnNames">Column names</param>
        /// <returns>DataTable</returns>
        public DataTable GetCodeTableData(string tableName, List<string> columnNames)
        {
            Util.Assert(tableName.StartsWith("meta") == false);
            return this.db.GetTableData(tableName, columnNames);
        }

        /// <summary>
        /// Get code table data given columns and sort criteria
        /// </summary>
        /// <param name="tableName">Code table name</param>
        /// <param name="columnNames">Column names to filter on</param>
        /// <param name="sortCriteria">Sort criteria string</param>
        /// <returns>DataTable</returns>
        public DataTable GetCodeTableData(string tableName, string columnNames, string sortCriteria)
        {
            return this.db.GetTableData(tableName, columnNames, sortCriteria);
        }

        #endregion

        #region Fields
        private Project project;
        private IDbDriverFactory dbFactory = null;
        /// <summary>
        /// The underlying physical databsae
        /// </summary>
        protected IDbDriver db;
        #endregion Fields

        #region Events
        /// <summary>
        /// Event for the beginning of a progress report
        /// </summary>
        public event ProgressReportBeginEventHandler ProgressReportBeginEvent;

        /// <summary>
        /// Event for the updating of a progress report
        /// </summary>
        public event ProgressReportUpdateEventHandler ProgressReportUpdateEvent;

        /// <summary>
        /// Event for the ending of a progress report
        /// </summary>
        public event SimpleEventHandler ProgressReportEndEvent;
        #endregion Events

        #region Constructors

        /// <summary>
        /// Constructor for the class.
        /// </summary>
        /// <param name="proj">Project the metadata belongs to</param>
        public MetadataXmlProvider(Project proj)
        {
            #region Input validation
            if (proj == null)
                throw new System.ArgumentNullException("proj");
            #endregion Input validation

            project = proj;

            // Create Views node in the XML file.
            XmlNode root = GetRootNode();
            root.AppendChild(GetXmlDocument().CreateElement("Views"));
            Save();
        }

        #endregion Constructors

        #region Public Properties

        /// <summary>
        /// Project
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
        /// exampleTableName, exampleTableNam2, exampleTableNam3, ...
        /// </summary>
        /// <param name="viewName">Name of the current Id of view in Epi.Project.</param>
        /// <returns></returns>
        public string GetAvailDataTableName(string viewName)
        {
            string name = string.Empty;
            return name;
        }
        
        /// <summary>
        /// Get List of code tables
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable GetCodeTableList()
        {
            try
            {
                DataSets.TableSchema.TablesDataTable tables = db.GetTableSchema();

                DataRow[] rowsFiltered = tables.Select("TABLE_NAME not like 'code%'");
                foreach (DataRow rowFiltered in rowsFiltered)
                {
                    tables.Rows.Remove(rowFiltered);
                }
                return tables;
            }
            catch (Exception ex)
            {
                throw new System.ApplicationException("Could not retrieve code tables from database", ex);
            }
        }

        public void UpdateMetaViewFields(View view)
        {
        }

        #region Select Statements

        /// <summary>
        /// Gets all views belonging to a project
        /// </summary>
        /// <returns>Datatable containing view info</returns>
        public virtual DataTable GetViewsAsDataTable()
        {
            try
            {
                Query query = db.CreateQuery(
                    @"select [ViewId], [Name], [CheckCodeBefore], [CheckCodeAfter], [RecordCheckCodeBefore], 
                    [RecordCheckCodeAfter], [CheckCodeVariableDefinitions], [IsRelatedView] 
					from metaViews"
                    );
                //query.Parameters.Add(new DbParameter("@ProjectId", DbType.Guid, Project.Id));			
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve view data table", ex);
            }
        }

        /// <summary>
        /// Gets all views belonging to an metadata Xml project
        /// </summary>
        /// <param name="currentViewElement">The view element</param>
        /// <param name="viewsNode">The view node</param>
        /// <returns></returns>
        public ViewCollection GetViews(XmlElement currentViewElement, XmlNode viewsNode)
        {
            ViewCollection views = new Collections.ViewCollection();
            foreach (XmlNode childNode in viewsNode.ChildNodes)
            {
                if (currentViewElement == null)
                {
                    //currentViewElement = childNode.OwnerDocument.DocumentElement["View"];
                    currentViewElement = (XmlElement)childNode;
                }

                View newView = new View(this.Project, currentViewElement);
                newView.Name = childNode.Attributes["Name"].Value;
                views.Add(newView);
            }
            return views;
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
                    views.Add(new View(row, this.Project));
                }
                return views;
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve view collection", ex);
            }
        }
        /*
                /// <summary>
                /// Gets all pages belonging to the project
                /// </summary>
                /// <returns>Datatable containing page info</returns>
                public DataTable GetPagesForProject()
                {
                    try
                    {
                        DbQuery query = db.CreateQuery("select P.[PageId], V.[ViewId], P.[Name] AS PageName, V.[Name] AS ViewName, P.[Position] " +
                            "from metaViews V inner join metaPages P on V.[ViewId] = P.[ViewId] " +
                            "order by V.[Name], P.[Position]");
                        return db.Select(query);
                    }
                    catch (Exception ex)
                    {
                        throw new GeneralException("Could not retrieve page", ex);
                    }
                }
        */

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

                Query query = db.CreateQuery("select [PageId], [ViewId], [Name], [Position], [CheckCodeBefore], [CheckCodeAfter] " +
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
                XmlNode pagesNode = view.ViewElement.SelectSingleNode("Pages");
                foreach (XmlNode pageNode in pagesNode.ChildNodes)
                {
                    Page page = new Page(view);
                    page.Id = int.Parse(pageNode.Attributes["PageId"].Value);
                    page.Position = int.Parse(pageNode.Attributes["Position"].Value);
                    page.Name = pageNode.Attributes["Name"].Value;
                    pages.Add(new Page(view, page.Id));
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
                Query query = db.CreateQuery("select [FieldId] from (metaFields f inner join metaViews v on v.[ViewId] = f.[ViewId]) where v.[Name] = @ViewName and f.[Name] = @FieldName");
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
        /// Returns a field as a data row
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public DataRow GetFieldAsDataRow(Field field)
        {
            string queryString =
                "select F.[FieldId], F.[Name] As Name, F.[PageId], F.[ViewId], F.[FieldTypeId], F.[CheckCodeAfter] As ControlAfterCheckCode, F.[CheckCodeBefore] As ControlBeforeCheckCode,  " +
                "F.[ControlTopPositionPercentage], F.[ControlLeftPositionPercentage], F.[ControlHeightPercentage], F.[ControlWidthPercentage], " +
                "F.[ControlFontFamily] As ControlFontFamily, F.[ControlFontSize] As ControlFontSize, F.[ControlFontStyle] As ControlFontStyle, F.[ControlScriptName], " +
                "F.[PromptTopPositionPercentage], F.[PromptLeftPositionPercentage], F.[PromptText], F.[PromptFontFamily], F.[PromptFontSize], F.[PromptFontStyle], F.[ControlFontFamily], F.[ControlFontSize], F.[ControlFontStyle], F.[PromptScriptName], " +
                "F.[ShouldRepeatLast], F.[IsRequired], F.[IsReadOnly],  " +
                "F.[ShouldRetainImageSize], F.[Pattern], F.[MaxLength], F.[ShowTextOnRight], " +
                "F.[Lower], F.[Upper], F.[RelateCondition], F.[ShouldReturnToParent], F.[RelatedViewId], " +
                "F.[SourceTableName], F.[CodeColumnName], F.[TextColumnName], " +
                "F.[Sort], F.[IsExclusiveTable], F.[TabIndex], F.[HasTabStop],F.[SourceFieldId], F.[DataTableName] " +
                "from metaFields F where F.[ViewId] = @viewId AND F.[FieldId] = @fieldID " +
                "order by F.[ControlTopPositionPercentage], F.[ControlLeftPositionPercentage]";

            Query query = db.CreateQuery(queryString);
            query.Parameters.Add(new QueryParameter("@viewID", DbType.Int32, field.GetView().Id));
            query.Parameters.Add(new QueryParameter("@fieldID", DbType.Int32, field.Id));
            return db.Select(query).Rows[0];
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
                query.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, viewId));
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
        /// Gets a view object based on view id
        /// </summary>
        /// <param name="viewId">Id of the view</param>
        /// <returns>A view object</returns>
        public View GetViewById(int viewId)
        {
            try
            {
                Query query = db.CreateQuery("select [ViewId], [Name], [CheckCodeBefore], [CheckCodeAfter], [RecordCheckCodeBefore], [RecordCheckCodeAfter], [CheckCodeVariableDefinitions], [IsRelatedView] " +
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
            catch (Exception ex)
                {
                throw new GeneralException("Could not retrieve view", ex);
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
                //				DbQuery query = db.CreateQuery("select T.[ControlID], T.[CheckCodeBefore], F.[Name] " +
                //					"from metaControls T " +
                //					"LEFT JOIN metaFields F On F.[FieldID] = T.[FieldID] " +
                //					"where T.[FieldID] = @FieldID");
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
                    //				DbQuery query = db.CreateQuery("select T.[ControlID], T.[CheckCodeAfter], F.[Name] " +
                    //					"from metaControls T " +
                    //					"LEFT JOIN metaFields F On F.[FieldID] = T.[FieldID] " +
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

                Query query = db.CreateQuery("select [ViewID], [Name], [CheckCodeBefore], [CheckCodeAfter], " +
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

                View currentView = field.GetView();
                XmlNode viewsNode = GetViewsNode();
                XmlNode currentViewNode = viewsNode.SelectSingleNode("//View[@ViewId = '" + currentView.Id + "']");
                XmlNode fieldNode = currentViewNode.SelectSingleNode("//View/Fields[@FieldId = '" + field.Id + "']");
                if (fieldNode == null) return null; //Test field Node
                // Success continue to get Child View.
                string relatedViewId = fieldNode.Attributes["RelatedViewId"].Value;
                XmlNode relatedViewNode = viewsNode.SelectSingleNode("View[@ViewId = '" + relatedViewId + "']");

                XmlElement currentViewElement = (XmlElement)relatedViewNode;
                View newView = new View(this.Project, currentViewElement);
                newView.Name = relatedViewNode.Attributes["Name"].Value;
                return newView;
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

        ///// <summary>
        ///// Gets the system fields in a view
        ///// </summary>
        ///// <param name="view">A view object</param>
        ///// <returns>A collection of fields</returns>
        //public  NamedObjectCollection<Field> GetSystemFields(View view)
        //{
        //    try
        //    {
        //        NamedObjectCollection<Field> fields = new NamedObjectCollection<Field>();
        //        DataTable table = GetSystemFields(view.Id);
        //        foreach (DataRow row in table.Rows)
        //        {
        //            switch((MetaFieldType)row["FieldTypeId"])
        //            {
        //                case MetaFieldType.RecStatus:
        //                    fields.Add(new RecStatusField(row, view));
        //                    break;
        //                case MetaFieldType.UniqueKey:
        //                    fields.Add(new UniqueKeyField(row, view));
        //                    break;
        //                default:
        //                    break;
        //            }

        //        }
        //        return (fields);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new GeneralException("Could not retrieve Field collection", ex);
        //    }
        //}

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

                Query query = db.CreateQuery("select F.[Name], F.[PageId], F.[FieldId], F.[FieldTypeId], F.[CheckCodeAfter] As ControlAfterCheckCode, F.[CheckCodeBefore] As ControlBeforeCheckCode,  " +
                    "P.[Name] AS PageName, P.[CheckCodeBefore] As PageBeforeCheckCode, P.[CheckCodeAfter] As PageAfterCheckCode, P.[Position], " +
                    "F.[ControlTopPositionPercentage], F.[ControlLeftPositionPercentage], F.[ControlHeightPercentage], F.[ControlWidthPercentage] , F.[ControlFontFamily], F.[ControlFontSize], F.[ControlFontStyle], F.[ControlScriptName], " +
                    "F.[PromptTopPositionPercentage], F.[PromptLeftPositionPercentage], F.[PromptText], F.[PromptFontFamily], F.[PromptFontSize], F.[PromptFontStyle], F.[ControlFontFamily], F.[ControlFontSize], F.[ControlFontStyle], F.[PromptScriptName], " +
                    "F.[ShouldRepeatLast], F.[IsRequired], F.[IsReadOnly],  " +
                    "F.[ShouldRetainImageSize], F.[Pattern], F.[MaxLength], F.[ShowTextOnRight], " +
                    "F.[Lower], F.[Upper], F.[RelateCondition], F.[ShouldReturnToParent], F.[RelatedViewId], " +
                    "F.[SourceTableName], F.[CodeColumnName], F.[TextColumnName], " +
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
        /// Returns view's fields as a data table
        /// </summary>
        /// <param name="view">The view</param>
        /// <returns>DataTable</returns>
        public DataTable GetFieldsAsDataTable(View view)
        {
            try
            {
                #region Input Validation
                if (view == null)
                {
                    throw new ArgumentNullException("View");
                }
                #endregion  //Input Validation

                string queryString =
                    "select F.[FieldId], F.[Name] As Name, F.[PageId], F.[ViewId], F.[FieldTypeId], F.[CheckCodeAfter] As ControlAfterCheckCode, F.[CheckCodeBefore] As ControlBeforeCheckCode,  " +
                    "F.[ControlTopPositionPercentage], F.[ControlLeftPositionPercentage], F.[ControlHeightPercentage], F.[ControlWidthPercentage], " +
                    "F.[ControlFontFamily] As ControlFontFamily, F.[ControlFontSize] As ControlFontSize, F.[ControlFontStyle] As ControlFontStyle, F.[ControlScriptName], " +
                    "F.[PromptTopPositionPercentage], F.[PromptLeftPositionPercentage], F.[PromptText], F.[PromptFontFamily], F.[PromptFontSize], F.[PromptFontStyle], F.[ControlFontFamily], F.[ControlFontSize], F.[ControlFontStyle], F.[PromptScriptName], " +
                    "F.[ShouldRepeatLast], F.[IsRequired], F.[IsReadOnly],  " +
                    "F.[ShouldRetainImageSize], F.[Pattern], F.[MaxLength], F.[ShowTextOnRight], " +
                    "F.[Lower], F.[Upper], F.[RelateCondition], F.[ShouldReturnToParent], F.[RelatedViewId], " +
                    "F.[SourceTableName], F.[CodeColumnName], F.[TextColumnName], " +
                    "F.[Sort], F.[IsExclusiveTable], F.[TabIndex], F.[HasTabStop],F.[SourceFieldId], F.[DataTableName] " +
                    "from metaFields F where F.[ViewId] = @viewId " +
                    "order by F.[ControlTopPositionPercentage], F.[ControlLeftPositionPercentage]";

                Query query = db.CreateQuery(queryString);
                query.Parameters.Add(new QueryParameter("@viewID", DbType.Int32, view.Id));
                return db.Select(query);
            }
            //catch (Exception ex)
            //{
            //	throw new GeneralException("Could not retrieve fields", ex);
            //}
            finally
            {
            }
        }

        /// <summary>
        /// Gets field metadata needed for synchronizing the view's data tables
        /// </summary>
        /// <param name="view">View</param>
        /// <returns>A DataTable containing fieldName, dataTableName, fieldTypeId</returns>
        public DataTable GetFieldMetadataSync(View view)
        {
            return null;
        }


        /// <summary>
        /// Gets field metadata needed for synchronizing the view's data tables
        /// </summary>
        /// <param name="page">Page</param>
        /// <returns>A DataTable containing fieldName, dataTableName, fieldTypeId</returns>
        public DataTable GetFieldMetadataSync(int pageId)
        {
            return null;
        }

        /// <summary>
        /// Gets all the fields in a view
        /// </summary>
        /// <param name="view">the view object</param>
        /// <returns>A collection of fields</returns>
        public virtual FieldCollectionMaster GetFields(View view)
        {
            try
            {
                FieldCollectionMaster fields = new FieldCollectionMaster();

                //DataTable table = GetFieldsAsDataTable(view);
                //foreach (DataRow row in table.Rows)
                //{
                //    MetaFieldType fieldTypeId = (MetaFieldType)row[ColumnNames.FIELD_TYPE_ID];
                //    Field field = null;

                XmlNode fieldsNode = view.ViewElement.SelectSingleNode("Fields");
                foreach (XmlNode fieldNode in fieldsNode.ChildNodes)
                {
                    //MetaFieldType fieldTypeId = (MetaFieldType)(int.Parse(fieldNode.Attributes["FieldTypeId"].Value.ToString()));   
                    MetaFieldType fieldTypeId = (MetaFieldType)Enum.Parse(typeof(MetaFieldType), fieldNode.Attributes["FieldTypeId"].Value.ToString());

                    Field field = null;
                    switch (fieldTypeId)
                    {
                        case MetaFieldType.Text:
                            field = new SingleLineTextField(view, fieldNode);
                            break;
                        case MetaFieldType.LabelTitle:
                            field = new LabelField(view, fieldNode);
                            break;
                        case MetaFieldType.TextUppercase:
                            field = new UpperCaseTextField(view, fieldNode);
                            break;
                        case MetaFieldType.Multiline:
                            field = new MultilineTextField(view, fieldNode);
                            break;
                        case MetaFieldType.Number:
                            field = new NumberField(view, fieldNode);
                            break;
                        case MetaFieldType.PhoneNumber:
                            field = new PhoneNumberField(view, fieldNode);
                            break;
                        case MetaFieldType.Date:
                            field = new DateField(view, fieldNode);
                            break;
                        case MetaFieldType.Time:
                            field = new TimeField(view, fieldNode);
                            break;
                        case MetaFieldType.DateTime:
                            field = new DateTimeField(view, fieldNode);
                            break;
                        case MetaFieldType.Checkbox:
                            field = new CheckBoxField(view, fieldNode);
                            break;
                        case MetaFieldType.YesNo:
                            field = new YesNoField(view, fieldNode);
                            break;
                        case MetaFieldType.Option:
                            field = new OptionField(view, fieldNode);
                            break;
                        case MetaFieldType.CommandButton:
                            field = new CommandButtonField(view, fieldNode);
                            break;
                        case MetaFieldType.Image:
                            field = new ImageField(view, fieldNode);
                            break;
                        case MetaFieldType.Mirror:
                            field = new MirrorField(view, fieldNode);
                            break;
                        case MetaFieldType.Grid:
                            field = new GridField(view, fieldNode);
                            break;
                        case MetaFieldType.LegalValues:
                            field = new DDLFieldOfLegalValues(view, fieldNode);
                            break;
                        case MetaFieldType.Codes:
                            field = new DDLFieldOfCodes(view, fieldNode);
                            break;
                        case MetaFieldType.List:
                            field = new DDListField(view, fieldNode);
                            break;
                        case MetaFieldType.CommentLegal:
                            field = new DDLFieldOfCommentLegal(view, fieldNode);
                            break;
                        case MetaFieldType.Relate:
                            field = new RelatedViewField(view, fieldNode);
                            break;
                        case MetaFieldType.RecStatus:
                            field = new RecStatusField(view);
                            break;
                        case MetaFieldType.UniqueKey:
                            field = new UniqueKeyField(view);
                            break;
                        case MetaFieldType.ForeignKey:
                            field = new ForeignKeyField(view);
                            break;
                        default:
                            throw new GeneralException("Invalid Field Type");
                    }
                    //field.LoadFromRow(row);                    
                    fields.Add(field);
                }
                return (fields);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Get Grid column collection
        /// </summary>
        /// <param name="field">The field</param>
        /// <returns>List of GridColumns</returns>
        public List<GridColumnBase> GetGridColumnCollection(GridField field)
        {
            List<GridColumnBase> columns = new List<GridColumnBase>();
            DataTable table = GetGridColumns(field.Id);
            foreach (DataRow row in table.Rows)
            {
                switch ((MetaFieldType)row["FieldTypeId"])
                {
                    case MetaFieldType.UniqueKey:
                        columns.Add(new UniqueKeyColumn(row, field));
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
                    case MetaFieldType.Number:
                        columns.Add(new NumberColumn(row, field));
                        break;
                    case MetaFieldType.PhoneNumber:
                        columns.Add(new PhoneNumberColumn(row, field));
                        break;
                    case MetaFieldType.Date:
                        columns.Add(new DateColumn(row, field));
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

                Query query = db.CreateQuery("select G.[GridColumnId], G.[Name], G.[Width], G.[Size], G.[Position], G.[FieldTypeId], " +
                    "G.[Text], G.[ShouldRepeatLast], G.[IsRequired], G.[IsReadOnly], G.[Pattern], G.[Upper], G.[Lower], " +
                    "G.[DataTableName], G.[IsExclusiveTable], G.[SourceTableName], G.[TextColumnName], G.[CodeColumnName], G.[Sort] " +
                    "from metaGridColumns G " +
                    "where G.[FieldId] = @FieldId");
                query.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, fieldId));
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve grid column", ex);
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
        /// Returns a field's view element
        /// </summary>
        /// <param name="field">The field whose view element is to be obtained</param>
        /// <returns>The view element of the field from the xml project file</returns>
        public XmlElement GetFieldViewElement(Field field)
        {
            XmlNode viewsNode = GetViewsNode();
            XmlNode viewNode = viewsNode.SelectSingleNode("//View[@ViewId= '" + field.GetView().Id + "']");
            XmlElement viewElement = (XmlElement)viewNode;
            return viewElement;
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
                Query query = db.CreateQuery("select [LayerId], [Gml], [GmlSchema], [Name] from metaLayers where [LayerId] = @LayerId");
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
        /// Attaches a db driver object to this provider.
        /// </summary>
        /// <param name="dbDriver"></param>
        public void AttachDbDriver(IDbDriver dbDriver)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.GlobalRecordIdColumn"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.GlobalRecordId"/></param>
        public void AddGridColumn(DataRow gridColumnRow)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Change the field type by only changing the FieldTypeId in metaFields
        /// </summary>
        /// <param name="field">Epi.Fields.Field to be changed</param>
        /// <param name="fieldType">the fields new MetaFieldType</param>
        public void UpdateFieldType(Epi.Fields.Field field, Epi.MetaFieldType fieldType)
        {
            throw new NotImplementedException();
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

                Query query = db.CreateQuery("select * from metaFields where PageId = @PageId and FieldTypeId = 21");
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
                Query query = db.CreateQuery("SELECT COUNT('ViewId') AS numTimesUsed "
                    + "FROM metaFields "
                    + "WHERE ViewId = '"
                    + viewId.ToString()
                    + "'");

                DataTable dt = db.Select(query);
                int tableNameCount = (int)dt.Rows[0]["numTimesUsed"];
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
        /// <param name="page">The page</param>
        /// <returns>A NamedObjectCollection of GroupFields</returns>
        public NamedObjectCollection<GroupField> GetGroupFields(Page page)
        {
            try
            {
                NamedObjectCollection<GroupField> groups = new NamedObjectCollection<GroupField>();
                DataTable table = GetGroupsForPage(page.Id);
                foreach (DataRow row in table.Rows)
                {
                //    groups.Add(new GroupField(row, page));
                }
                return (groups);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve field groups", ex);
            }
        }

        /// <summary>
        /// Get patterns
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable GetPatterns()
        {
            try
            {
                Query query = db.CreateQuery("select [PatternId], [DataTypeId], [Expression], [Mask], [FormattedExpression] " +
                    "from metaPatterns ");
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
                "where [PageId] = @pageID and [FieldTypeId] in (1,3,4,17,18) " +
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

        ///// <summary>
        ///// Gets the tab index for a field name in metaFields
        ///// </summary>
        ///// <param name="viewId">The Id of the view</param>
        ///// <param name="fieldName">The field name in MetaFields</param>
        ///// <returns></returns>
        //public int GetColumnTabIndex(int viewId, string fieldName)
        //{
        //    try
        //    {
        //        #region Input Validation
        //        if (viewId < 0)
        //        {
        //            throw new ArgumentOutOfRangeException("View ID");
        //        }
        //        if (string.IsNullOrEmpty(fieldName))
        //        {
        //            throw new ArgumentNullException("Field Name");
        //        }
        //        #endregion  //Input Validation

        //        DbQuery query = db.CreateQuery("select [TabIndex] from metaFields where [ViewId] = @viewId and [Name] = @FieldName");
        //        query.Parameters.Add(new DbParameter("@viewId", DbType.Int32, viewId));
        //        query.Parameters.Add(new DbParameter("@fieldName", DbType.String, fieldName));
        //        return (int)db.ExecuteScalar(query);

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new GeneralException("Could not retrieve tab index for a column", ex);
        //    }           
        //}


        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="viewID"></param>
        ///// <param name="fieldName"></param>
        ///// <returns></returns>
        //public   bool FieldExists(int viewID, string fieldName)
        //{
        //    try
        //    {
        //        #region Input Validation
        //        if (viewID < 1)
        //        {
        //            throw new ArgumentOutOfRangeException("ViewId");
        //        }
        //        if (string.IsNullOrEmpty(fieldName))				
        //        {
        //            throw new ArgumentNullException("Field Name");
        //        }
        //        #endregion

        //        DbQuery query = db.CreateQuery("select F.[Name] " +
        //            "from (metaViews V inner join metaPages P on V.[ViewId] = P.[ViewId]) inner join metaFields F on P.[PageId] = F.[PageId] " +
        //            "where V.[ViewID] = @ViewID and " +
        //            "F.[Name] = @FieldName") ;
        //        query.Parameters.Add(new DbParameter("@ViewID", DbType.Int32, viewID));
        //        query.Parameters.Add(new DbParameter("@FieldName", DbType.String, fieldName));
        //        DataTable result = db.Select(query);
        //        if (result.Rows.Count == 0)
        //        {
        //            return (false);
        //        }
        //        else
        //        {
        //            return (true);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new GeneralException("Could not retrieve field names.", ex);
        //    }

        //}

        /// <summary>
        /// Get tables that are not view tables 
        /// </summary>
        /// <returns>DataTable</returns>
        public virtual DataTable GetNonViewTablesAsDataTable()
        {
            try
            {
                DataRow dataRow;
                DataTable tables = db.GetTableSchema(); //now GetTableSchema only gets user tables  /zack 1/30/08

                DataTable viewsAndTables = new DataTable("ViewsAndTables");
                viewsAndTables.Columns.Add(ColumnNames.NAME);
                DataRow[] rows = tables.Select("TABLE_NAME not like 'meta%'");
                foreach (DataRow row in rows)
                {
                    string tableName = row["Table_Name"].ToString();

                    //if (!tableName.ToLower().StartsWith("sys"))   //This probably only works with Microsoft database, zack 1/29/08
                    //{
                    dataRow = viewsAndTables.NewRow();
                    dataRow[ColumnNames.NAME] = tableName;
                    viewsAndTables.Rows.Add(dataRow);
                    //}
                }

                return (viewsAndTables);

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve Views and Tables.", ex);
            }
        }

        /// <summary>
        /// Gets a view object based on view name
        /// </summary>
        /// <param name="viewFullName">Name of the view</param>
        /// <returns>A view object</returns>
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
                XmlDocument doc = GetXmlDocument();
                XmlNode viewsNode = GetViewsNode();
                string viewName = viewFullName.Substring(viewFullName.LastIndexOf(":") + 1);
                XmlNode viewNode = viewsNode.SelectSingleNode("//View[@Name = '" + viewName + "']");
                XmlElement viewElement = (XmlElement)viewNode;

                return new View(project, viewElement);


                //Query query = db.CreateQuery("select [ViewId], [Name],[CheckCodeBefore], [CheckCodeAfter], [RecordCheckCodeBefore], [RecordCheckCodeAfter], [CheckCodeVariableDefinitions], [IsRelatedView] " +
                //    "from metaViews " +
                //    "where [Name] = @ViewName");
                //query.Parameters.Add(new QueryParameter("@ViewName", DbType.String, viewName));
                //DataTable results = db.Select(query);
                //if (results.Rows.Count > 0)
                //{
                //    return new View(results.Rows[0], Project);
                //}
                //else
                //{
                //    return null;
                //}
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve view", ex);
            }
        }

        /// <summary>
        /// Gets all the programs saved in a project
        /// </summary>
        /// <returns>DataTable containing a list of all programs in current project</returns>
        public virtual DataTable GetPgms()
        {
            try
            {
                Query query = db.CreateQuery("select [ProgramId], [Name], [Content], [Comment], [DateCreated], [DateModified], [Author] " +
                    "from [metaPrograms]");
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
        /// <param name="dataTableName"></param>
        /// <returns></returns>
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
        /// Get Data Dictionary
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable GetDataDictionary(View view)
        {
            DataTable table = null;
            Query query = db.CreateQuery("select [PageId], [PromptText], [Name] " +
                "from [metaFields] " +
                "where [ViewId]=@ViewId");
            query.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, view.Id));
            table = db.Select(query);
            return table;
        }
        
        /// <summary>
        /// Returns all datatables used by metadata.
        /// </summary>
        /// <returns></returns>
        public List<string> GetDataTableList()
        {
            try
            {
                List<string> tables = new List<string>();
                Query query = db.CreateQuery("select distinct [DataTableName] as Name, [ViewId] " +
                    "from [metaFields] where [DataTableName] is not null ");
                DataTable table = db.Select(query);
                foreach (DataRow row in table.Rows)
                {
                    tables.Add(row["Name"].ToString());
                }
                return tables;
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve DataTableName", ex);
            }
        }

        /// <summary>
        /// Get the data table name of a view
        /// </summary>
        /// <param name="viewId">The view id</param>
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
                Query query = db.CreateQuery("select distinct [DataTableName] from [metaFields] where [ViewId] = @viewId and [DataTableName] is not null and [DataTableName] <> '' ");
                query.Parameters.Add(new QueryParameter("@viewId", DbType.Int32, viewId));
                DataTable results = db.Select(query);
                if (results.Rows.Count > 0)
                {
                    if (results.Rows.Count > 1)
                    {
                        throw new GeneralException("More than one datatable found for view");
                    }
                    else
                    {
                        return results.Rows[0][ColumnNames.DATA_TABLE_NAME].ToString();
                    }
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
        /// Returns the names of all data tables used by metadata for a given view.
        /// </summary>
        /// <param name="viewId"></param>
        /// <returns>a DataTable containing all the data table names.</returns>
        public DataTable GetDataTableNames(int viewId)
        {
            return null;
        }
            
        /// <summary>
        /// returns the column names in the table referenced by a view
        /// </summary>
        /// <param name="viewId"></param>
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
                WordBuilder list = new WordBuilder();
                list.Delimitter = StringLiterals.COMMA;
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

        /*
        /// <summary>
        /// Gets list of code tables in the metadata database
        /// </summary>
        /// <returns>DataRow of table names</returns>
        public DataRow[] GetCodeTableList()
        {
            try
            {
				
                DataTable tables = db.GetTableSchema();
				
                return tables.Select("TABLE_NAME like 'code%'");
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve code tables from database", ex);
            }
        }
        */
        #endregion Select Statement

        #region Insert Statements

        /// <summary>
        /// Creates a page in XML metadata
        /// </summary>
        /// <param name="view">The view the page belongs to</param>        
        /// <param name="name">The name of the page</param>
        /// <param name="position">The position of the page</param>
        public Page CreatePage(View view, string name, int position)
        {
            Page page;
            XmlDocument xmlDoc = GetXmlDocument();
            XmlNode pagesNode = GetPagesNode(view.ViewElement);
            XmlElement pageElement = xmlDoc.CreateElement("Page");
            XmlAttribute pageId = xmlDoc.CreateAttribute("PageId");
            //int currentPageId = GetMaxPageId(view.Id);            
            int currentPageId = 0;
            pageId.Value = currentPageId.ToString();
            pageElement.Attributes.Append(pageId);
            XmlAttribute pageName = xmlDoc.CreateAttribute("Name");
            pageName.Value = name;
            pageElement.Attributes.Append(pageName);
            XmlAttribute pagePosition = xmlDoc.CreateAttribute("Position");
            pagePosition.Value = position.ToString();
            pageElement.Attributes.Append(pagePosition);

            page = new Page(view, int.Parse(pageId.Value));
            page.Id = int.Parse(pageId.Value);
            page.Name = pageName.Value;
            page.Position = int.Parse(pagePosition.Value);

            XmlElement checkCodeBeforeElement = xmlDoc.CreateElement("CheckCodeBefore");
            XmlAttribute checkCodeBefore = xmlDoc.CreateAttribute("CheckCodeBefore");
            checkCodeBefore.Value = page.CheckCodeBefore.ToString();
            checkCodeBeforeElement.Attributes.Append(checkCodeBefore);
            pageElement.AppendChild(checkCodeBeforeElement);

            XmlElement checkCodeAfterElement = xmlDoc.CreateElement("CheckCodeAfter");
            XmlAttribute checkCodeAfter = xmlDoc.CreateAttribute("CheckCodeAfter");
            checkCodeAfter.Value = page.CheckCodeAfter.ToString();
            checkCodeAfterElement.Attributes.Append(checkCodeAfter);
            pageElement.AppendChild(checkCodeAfterElement);

            pagesNode.AppendChild(pageElement);
            Save();
            return page;
        }

        /// <summary>
        /// Creates a new view in XML metadata
        /// </summary>
        /// <param name="view">View to be inserted in metadata</param>        
        public void InsertView(View view)
        {
            XmlDocument xmlDoc = GetXmlDocument();
            XmlNode viewsNode = GetViewsNode();
            XmlElement viewElement = xmlDoc.CreateElement("View");
            viewsNode.AppendChild(viewElement);
            view.ViewElement = viewElement;
            XmlAttribute viewId = xmlDoc.CreateAttribute("ViewId");
            viewId.Value = GetMaxViewId().ToString();
            view.Id = int.Parse(viewId.Value);
            viewElement.Attributes.Append(viewId);
            XmlAttribute viewNameAttribute = xmlDoc.CreateAttribute("Name");
            viewNameAttribute.Value = view.Name;
            viewElement.Attributes.Append(viewNameAttribute);
            XmlAttribute isRelatedView = xmlDoc.CreateAttribute("IsRelatedView");
            isRelatedView.Value = view.IsRelatedView.ToString();
            viewElement.Attributes.Append(isRelatedView);
            viewElement.AppendChild(xmlDoc.CreateElement("CheckCodeBefore"));
            viewElement.AppendChild(xmlDoc.CreateElement("CheckCodeAfter"));
            viewElement.AppendChild(xmlDoc.CreateElement("RecordCheckCodeBefore"));
            viewElement.AppendChild(xmlDoc.CreateElement("RecordCheckCodeAfter"));
            viewElement.AppendChild(xmlDoc.CreateElement("CheckCodeVariableDefinitions"));
            viewElement.AppendChild(xmlDoc.CreateElement("Pages"));
            viewElement.AppendChild(xmlDoc.CreateElement("Fields"));

            //Insert system fields .. RecStatus and UniqueKey
            RecStatusField recStatusField = new RecStatusField(view, viewElement);
            UniqueKeyField uniqueKeyField = new UniqueKeyField(view, viewElement);
            recStatusField.SaveToDb();
            uniqueKeyField.SaveToDb();
            if (view.IsRelatedView)
            {
                ForeignKeyField foreignKeyField = new ForeignKeyField(view, viewElement);
                foreignKeyField.SaveToDb();
            }
            Save();


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

                XmlDocument doc = GetXmlDocument();
                XmlNode pagesNode = GetPagesNode(page.GetView().ViewElement);

                CreatePage(page.GetView(), page.Name, page.Position);
                XmlNode pageNode = pagesNode.SelectSingleNode("//Page[@PageId= '" + page.Id + "']");

                page.Id = GetMaxPageId(page.GetView().Id) + 1;
                pageNode.Attributes["PageId"].Value = page.Id.ToString();
                Save();

                //Query query = db.CreateQuery("insert into metaPages([Name], [Position], [ViewId], [CheckCodeBefore], [CheckCodeAfter]) values (@Name, @Position, @ViewId, @CheckCodeBefore, @CheckCodeAfter)");
                //query.Parameters.Add(new QueryParameter("@Name", DbType.String, page.Name));
                //query.Parameters.Add(new QueryParameter("@Position", DbType.Int32, page.Position));
                //query.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, page.GetView().Id));
                //query.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, page.CheckCodeBefore));
                //query.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, page.CheckCodeAfter));

                //db.ExecuteNonQuery(query);
                //page.Id = GetMaxPageId(page.GetView().Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create page in the database", ex);
            }
            finally
            {

            }
        }

        public Int32 InsertMetaImage(byte[] imageAsBytes)
        {
            Query insertQuery = db.CreateQuery
            (
                "insert into [metaImages] ([Image],[ImageUniqueValue]) " +
                "VALUES (@Image,@ImageUniqueValue)"
            );

            Data.QueryParameter imageParam = new QueryParameter("@Image", DbType.Object, imageAsBytes);
            imageParam.Size = imageAsBytes.Length + 1;
            insertQuery.Parameters.Add(imageParam);
            insertQuery.Parameters.Add(new QueryParameter("@ImageUniqueValue", DbType.Int32, imageAsBytes.Length));
            db.ExecuteNonQuery(insertQuery);
            return GetMaxImageId();
        }

        public Int32 InsertMetaImage(string imagePath)
        {
            int imageId = -1;
            return imageId;
        }

        /// <summary>
        /// Create Unique Key field.
        /// </summary>
        /// <param name="field">Unique Key field to create.</param>
        /// <returns>Id of the newly created field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldViewId = xmlDoc.CreateAttribute("ViewId");
                fieldViewId.Value = view.Id.ToString();
                fieldElement.Attributes.Append(fieldViewId);

                XmlAttribute dataTableName = xmlDoc.CreateAttribute("DataTableName");
                dataTableName.Value = field.TableName;
                fieldElement.Attributes.Append(dataTableName);

                fieldsNode.AppendChild(fieldElement);
                view.Project.Save();
                return field.Id;
            }
            finally
            {
            }
        }

        //zack todo: implement
        /// <summary>
        /// Create UniqueIdentifier field.
        /// </summary>
        /// <param name="field">UniqueIdentifier field to create.</param>
        /// <returns>Id of the newly created field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldViewId = xmlDoc.CreateAttribute("ViewId");
                fieldViewId.Value = view.Id.ToString();
                fieldElement.Attributes.Append(fieldViewId);

                XmlAttribute dataTableName = xmlDoc.CreateAttribute("DataTableName");
                dataTableName.Value = field.TableName;
                fieldElement.Attributes.Append(dataTableName);

                fieldsNode.AppendChild(fieldElement);
                view.Project.Save();
                return field.Id;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Insert a GlobalRecordIdField record into the metaFields table.
        /// </summary>
        /// <param name="field">Global Record Id Field.</param>
        /// <returns>Returns the Id of the last GlobalRecordIdField added.</returns>
        public int CreateField(GlobalRecordIdField field)
        {
            throw new System.ApplicationException("Not implemented");
        }

        /// <summary>
        /// Create Foreign Key field.
        /// </summary>
        /// <param name="field">Foreign Key field to create.</param>
        /// <returns>Id of the newly created ForeignKey field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldViewId = xmlDoc.CreateAttribute("ViewId");
                fieldViewId.Value = view.Id.ToString();
                fieldElement.Attributes.Append(fieldViewId);

                XmlAttribute dataTableName = xmlDoc.CreateAttribute("DataTableName");
                dataTableName.Value = field.TableName;
                fieldElement.Attributes.Append(dataTableName);

                fieldsNode.AppendChild(fieldElement);
                view.Project.Save();
                return field.Id;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Create Record Status field.
        /// </summary>
        /// <param name="field">Record Status field to create.</param>
        /// <returns>Id of the newly created field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldViewId = xmlDoc.CreateAttribute("ViewId");
                fieldViewId.Value = view.Id.ToString();
                fieldElement.Attributes.Append(fieldViewId);

                XmlAttribute dataTableName = xmlDoc.CreateAttribute("DataTableName");
                dataTableName.Value = field.TableName;
                fieldElement.Attributes.Append(dataTableName);

                fieldsNode.AppendChild(fieldElement);
                view.Project.Save();
                return field.Id;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Create Check box field.
        /// </summary>
        /// <param name="field">Check box field to create.</param>
        /// <returns>Id of the newly created field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldPromptText = xmlDoc.CreateAttribute("PromptText");
                fieldPromptText.Value = field.PromptText;
                fieldElement.Attributes.Append(fieldPromptText);

                XmlAttribute fieldControlFontFamily = xmlDoc.CreateAttribute("ControlFontFamily");
                fieldControlFontFamily.Value = field.ControlFont.FontFamily.Name;
                fieldElement.Attributes.Append(fieldControlFontFamily);

                XmlAttribute controlFontStyle = xmlDoc.CreateAttribute("ControlFontStyle");
                controlFontStyle.Value = field.ControlFont.Style.ToString();
                fieldElement.Attributes.Append(controlFontStyle);

                XmlAttribute controlFontSize = xmlDoc.CreateAttribute("ControlFontSize");
                controlFontSize.Value = field.ControlFont.Size.ToString();
                fieldElement.Attributes.Append(controlFontSize);

                XmlAttribute controlTopPositionPercentage = xmlDoc.CreateAttribute("ControlTopPositionPercentage");
                controlTopPositionPercentage.Value = field.ControlTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlTopPositionPercentage);

                XmlAttribute controlLeftPositionPercentage = xmlDoc.CreateAttribute("ControlLeftPositionPercentage");
                controlLeftPositionPercentage.Value = field.ControlLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlLeftPositionPercentage);

                XmlAttribute controlHeightPercentage = xmlDoc.CreateAttribute("ControlHeightPercentage");
                controlHeightPercentage.Value = field.ControlHeightPercentage.ToString();
                fieldElement.Attributes.Append(controlHeightPercentage);

                XmlAttribute controlWidthPercentage = xmlDoc.CreateAttribute("ControlWidthPercentage");
                controlWidthPercentage.Value = field.ControlWidthPercentage.ToString();
                fieldElement.Attributes.Append(controlWidthPercentage);

                XmlAttribute tabIndex = xmlDoc.CreateAttribute("TabIndex");
                tabIndex.Value = field.TabIndex.ToString();
                fieldElement.Attributes.Append(tabIndex);

                XmlAttribute hasTabStop = xmlDoc.CreateAttribute("HasTabStop");
                hasTabStop.Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldElement.Attributes.Append(hasTabStop);

                XmlAttribute promptFontFamily = xmlDoc.CreateAttribute("PromptFontFamily");
                promptFontFamily.Value = field.PromptFont.FontFamily.Name;
                fieldElement.Attributes.Append(promptFontFamily);

                XmlAttribute promptFontStyle = xmlDoc.CreateAttribute("PromptFontStyle");
                promptFontStyle.Value = field.PromptFont.Style.ToString();
                fieldElement.Attributes.Append(promptFontStyle);

                XmlAttribute promptFontSize = xmlDoc.CreateAttribute("PromptFontSize");
                promptFontSize.Value = field.PromptFont.Size.ToString();
                fieldElement.Attributes.Append(promptFontSize);

                XmlAttribute promptScriptName = xmlDoc.CreateAttribute("PromptScriptName");
                promptScriptName.Value = field.PromptFont.Name;
                fieldElement.Attributes.Append(promptScriptName);

                XmlAttribute controlScriptName = xmlDoc.CreateAttribute("ControlScriptName");
                controlScriptName.Value = field.ControlFont.Name;
                fieldElement.Attributes.Append(controlScriptName);

                XmlAttribute shouldRepeatLast = xmlDoc.CreateAttribute("ShouldRepeatLast");
                shouldRepeatLast.Value = bool.Parse(field.ShouldRepeatLast.ToString()).ToString();
                fieldElement.Attributes.Append(shouldRepeatLast);

                XmlAttribute isRequired = xmlDoc.CreateAttribute("IsRequired");
                isRequired.Value = (bool.Parse(field.IsRequired.ToString()).ToString());
                fieldElement.Attributes.Append(isRequired);

                XmlAttribute isReadOnly = xmlDoc.CreateAttribute("IsReadOnly");
                isReadOnly.Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldElement.Attributes.Append(isReadOnly);

                XmlAttribute shouldRetainImageSize = xmlDoc.CreateAttribute("ShouldRetainImageSize");
                //shouldRetainImageSize.Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();
                fieldElement.Attributes.Append(shouldRetainImageSize);

                XmlAttribute dataTableName = xmlDoc.CreateAttribute("DataTableName");
                dataTableName.Value = field.TableName;
                fieldElement.Attributes.Append(dataTableName);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute pageId = xmlDoc.CreateAttribute("PageId");
                pageId.Value = field.Page.Id.ToString();
                fieldElement.Attributes.Append(pageId);

                //XmlElement checkCodeBeforeElement = xmlDoc.CreateElement("CheckCodeBefore");
                //XmlAttribute checkCodeBefore = xmlDoc.CreateAttribute("CheckCodeBefore");
                //checkCodeBefore.Value = field.CheckCodeBefore.ToString();
                //checkCodeBeforeElement.Attributes.Append(checkCodeBefore);
                //fieldElement.AppendChild(checkCodeBeforeElement);

                XmlElement checkCodeAfterElement = xmlDoc.CreateElement("CheckCodeAfter");
                XmlAttribute checkCodeAfter = xmlDoc.CreateAttribute("CheckCodeAfter");
                checkCodeAfter.Value = field.CheckCodeAfter.ToString();
                checkCodeAfterElement.Attributes.Append(checkCodeAfter);
                fieldElement.AppendChild(checkCodeAfterElement);

                fieldsNode.AppendChild(fieldElement);
                view.Project.Save();
                return field.Id;
            }
            finally
            {
            }
        }

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
        */
        /*
                /// <summary>
                /// Create a strongly typed field.
                /// </summary>
                /// <param name="field">Field to create</param>
                /// <returns>Id of the newly created field.</returns>
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

        /// <summary>
        /// Create Command Button field.
        /// </summary>
        /// <param name="field">Command Button field to create.</param>
        /// <returns>Id of the newly created field.</returns>
        public int CreateField(CommandButtonField field)
        {
            try
            {
                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldPromptText = xmlDoc.CreateAttribute("PromptText");
                fieldPromptText.Value = field.PromptText;
                fieldElement.Attributes.Append(fieldPromptText);

                XmlAttribute fieldControlFontFamily = xmlDoc.CreateAttribute("ControlFontFamily");
                fieldControlFontFamily.Value = field.ControlFont.FontFamily.Name;
                fieldElement.Attributes.Append(fieldControlFontFamily);

                XmlAttribute controlFontStyle = xmlDoc.CreateAttribute("ControlFontStyle");
                controlFontStyle.Value = field.ControlFont.Style.ToString();
                fieldElement.Attributes.Append(controlFontStyle);

                XmlAttribute controlFontSize = xmlDoc.CreateAttribute("ControlFontSize");
                controlFontSize.Value = field.ControlFont.Size.ToString();
                fieldElement.Attributes.Append(controlFontSize);

                XmlAttribute controlTopPositionPercentage = xmlDoc.CreateAttribute("ControlTopPositionPercentage");
                controlTopPositionPercentage.Value = field.ControlTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlTopPositionPercentage);

                XmlAttribute controlLeftPositionPercentage = xmlDoc.CreateAttribute("ControlLeftPositionPercentage");
                controlLeftPositionPercentage.Value = field.ControlLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlLeftPositionPercentage);

                XmlAttribute controlHeightPercentage = xmlDoc.CreateAttribute("ControlHeightPercentage");
                controlHeightPercentage.Value = field.ControlHeightPercentage.ToString();
                fieldElement.Attributes.Append(controlHeightPercentage);

                XmlAttribute controlWidthPercentage = xmlDoc.CreateAttribute("ControlWidthPercentage");
                controlWidthPercentage.Value = field.ControlWidthPercentage.ToString();
                fieldElement.Attributes.Append(controlWidthPercentage);

                XmlAttribute tabIndex = xmlDoc.CreateAttribute("TabIndex");
                tabIndex.Value = field.TabIndex.ToString();
                fieldElement.Attributes.Append(tabIndex);

                XmlAttribute hasTabStop = xmlDoc.CreateAttribute("HasTabStop");
                hasTabStop.Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldElement.Attributes.Append(hasTabStop);

                XmlAttribute promptFontFamily = xmlDoc.CreateAttribute("PromptFontFamily");
                promptFontFamily.Value = field.PromptFont.FontFamily.Name;
                fieldElement.Attributes.Append(promptFontFamily);

                XmlAttribute promptFontStyle = xmlDoc.CreateAttribute("PromptFontStyle");
                promptFontStyle.Value = field.PromptFont.Style.ToString();
                fieldElement.Attributes.Append(promptFontStyle);

                XmlAttribute promptFontSize = xmlDoc.CreateAttribute("PromptFontSize");
                promptFontSize.Value = field.PromptFont.Size.ToString();
                fieldElement.Attributes.Append(promptFontSize);

                XmlAttribute promptScriptName = xmlDoc.CreateAttribute("PromptScriptName");
                promptScriptName.Value = field.PromptFont.Name;
                fieldElement.Attributes.Append(promptScriptName);

                XmlAttribute controlScriptName = xmlDoc.CreateAttribute("ControlScriptName");
                controlScriptName.Value = field.ControlFont.Name;
                fieldElement.Attributes.Append(controlScriptName);

                XmlAttribute shouldRetainImageSize = xmlDoc.CreateAttribute("ShouldRetainImageSize");
                //shouldRetainImageSize.Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();
                fieldElement.Attributes.Append(shouldRetainImageSize);

                //XmlAttribute dataTableName = xmlDoc.CreateAttribute("DataTableName");
                //dataTableName.Value = this.TableName;
                //fieldElement.Attributes.Append(dataTableName);            

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute pageId = xmlDoc.CreateAttribute("PageId");
                pageId.Value = field.Page.Id.ToString();
                fieldElement.Attributes.Append(pageId);
                fieldsNode.AppendChild(fieldElement);

                view.Project.Save();
                return field.Id;

            }
            finally
            {
            }
        }

        /// <summary>
        /// Create Date field.
        /// </summary>
        /// <param name="field">Date field to create.</param>
        /// <returns>Id of the newly created field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldPromptText = xmlDoc.CreateAttribute("PromptText");
                fieldPromptText.Value = field.PromptText;
                fieldElement.Attributes.Append(fieldPromptText);

                XmlAttribute fieldControlFontFamily = xmlDoc.CreateAttribute("ControlFontFamily");
                fieldControlFontFamily.Value = field.ControlFont.FontFamily.Name;
                fieldElement.Attributes.Append(fieldControlFontFamily);

                XmlAttribute controlFontStyle = xmlDoc.CreateAttribute("ControlFontStyle");
                controlFontStyle.Value = field.ControlFont.Style.ToString();
                fieldElement.Attributes.Append(controlFontStyle);

                XmlAttribute controlFontSize = xmlDoc.CreateAttribute("ControlFontSize");
                controlFontSize.Value = field.ControlFont.Size.ToString();
                fieldElement.Attributes.Append(controlFontSize);

                XmlAttribute controlTopPositionPercentage = xmlDoc.CreateAttribute("ControlTopPositionPercentage");
                controlTopPositionPercentage.Value = field.ControlTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlTopPositionPercentage);

                XmlAttribute controlLeftPositionPercentage = xmlDoc.CreateAttribute("ControlLeftPositionPercentage");
                controlLeftPositionPercentage.Value = field.ControlLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlLeftPositionPercentage);

                XmlAttribute controlHeightPercentage = xmlDoc.CreateAttribute("ControlHeightPercentage");
                controlHeightPercentage.Value = field.ControlHeightPercentage.ToString();
                fieldElement.Attributes.Append(controlHeightPercentage);

                XmlAttribute controlWidthPercentage = xmlDoc.CreateAttribute("ControlWidthPercentage");
                controlWidthPercentage.Value = field.ControlWidthPercentage.ToString();
                fieldElement.Attributes.Append(controlWidthPercentage);

                XmlAttribute tabIndex = xmlDoc.CreateAttribute("TabIndex");
                tabIndex.Value = field.TabIndex.ToString();
                fieldElement.Attributes.Append(tabIndex);

                XmlAttribute hasTabStop = xmlDoc.CreateAttribute("HasTabStop");
                hasTabStop.Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldElement.Attributes.Append(hasTabStop);

                XmlAttribute promptFontFamily = xmlDoc.CreateAttribute("PromptFontFamily");
                promptFontFamily.Value = field.PromptFont.FontFamily.Name;
                fieldElement.Attributes.Append(promptFontFamily);

                XmlAttribute promptFontStyle = xmlDoc.CreateAttribute("PromptFontStyle");
                promptFontStyle.Value = field.PromptFont.Style.ToString();
                fieldElement.Attributes.Append(promptFontStyle);

                XmlAttribute promptFontSize = xmlDoc.CreateAttribute("PromptFontSize");
                promptFontSize.Value = field.PromptFont.Size.ToString();
                fieldElement.Attributes.Append(promptFontSize);

                XmlAttribute promptScriptName = xmlDoc.CreateAttribute("PromptScriptName");
                promptScriptName.Value = field.PromptFont.Name;
                fieldElement.Attributes.Append(promptScriptName);

                XmlAttribute promptTopPositionPercentage = xmlDoc.CreateAttribute("PromptTopPositionPercentage");
                promptTopPositionPercentage.Value = field.PromptTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptTopPositionPercentage);

                XmlAttribute promptLeftPositionPercentage = xmlDoc.CreateAttribute("PromptLeftPositionPercentage");
                promptLeftPositionPercentage.Value = field.PromptLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptLeftPositionPercentage);

                XmlAttribute controlScriptName = xmlDoc.CreateAttribute("ControlScriptName");
                controlScriptName.Value = field.ControlFont.Name;
                fieldElement.Attributes.Append(controlScriptName);

                XmlAttribute shouldRepeatLast = xmlDoc.CreateAttribute("ShouldRepeatLast");
                shouldRepeatLast.Value = bool.Parse(field.ShouldRepeatLast.ToString()).ToString();
                fieldElement.Attributes.Append(shouldRepeatLast);

                XmlAttribute isRequired = xmlDoc.CreateAttribute("IsRequired");
                isRequired.Value = (bool.Parse(field.IsRequired.ToString()).ToString());
                fieldElement.Attributes.Append(isRequired);

                XmlAttribute isReadOnly = xmlDoc.CreateAttribute("IsReadOnly");
                isReadOnly.Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldElement.Attributes.Append(isReadOnly);

                XmlAttribute shouldRetainImageSize = xmlDoc.CreateAttribute("ShouldRetainImageSize");
                //shouldRetainImageSize.Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();
                fieldElement.Attributes.Append(shouldRetainImageSize);

                XmlAttribute lower = xmlDoc.CreateAttribute("Lower");
                lower.Value = field.Lower.ToString();
                fieldElement.Attributes.Append(lower);

                XmlAttribute upper = xmlDoc.CreateAttribute("Upper");
                upper.Value = field.Upper.ToString();
                fieldElement.Attributes.Append(upper);

                XmlAttribute dataTableName = xmlDoc.CreateAttribute("DataTableName");
                dataTableName.Value = field.TableName;
                fieldElement.Attributes.Append(dataTableName);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute pageId = xmlDoc.CreateAttribute("PageId");
                pageId.Value = field.Page.Id.ToString();
                fieldElement.Attributes.Append(pageId);

                XmlElement checkCodeBeforeElement = xmlDoc.CreateElement("CheckCodeBefore");
                XmlAttribute checkCodeBefore = xmlDoc.CreateAttribute("CheckCodeBefore");
                checkCodeBefore.Value = field.CheckCodeBefore.ToString();
                checkCodeBeforeElement.Attributes.Append(checkCodeBefore);
                fieldElement.AppendChild(checkCodeBeforeElement);

                XmlElement checkCodeAfterElement = xmlDoc.CreateElement("CheckCodeAfter");
                XmlAttribute checkCodeAfter = xmlDoc.CreateAttribute("CheckCodeAfter");
                checkCodeAfter.Value = field.CheckCodeAfter.ToString();
                checkCodeAfterElement.Attributes.Append(checkCodeAfter);
                fieldElement.AppendChild(checkCodeAfterElement);

                fieldsNode.AppendChild(fieldElement);
                view.Project.Save();
                return field.Id;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Create Date Time field.
        /// </summary>
        /// <param name="field">Date Time field to create.</param>
        /// <returns>Id of the newly created field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldPromptText = xmlDoc.CreateAttribute("PromptText");
                fieldPromptText.Value = field.PromptText;
                fieldElement.Attributes.Append(fieldPromptText);

                XmlAttribute fieldControlFontFamily = xmlDoc.CreateAttribute("ControlFontFamily");
                fieldControlFontFamily.Value = field.ControlFont.FontFamily.Name;
                fieldElement.Attributes.Append(fieldControlFontFamily);

                XmlAttribute controlFontStyle = xmlDoc.CreateAttribute("ControlFontStyle");
                controlFontStyle.Value = field.ControlFont.Style.ToString();
                fieldElement.Attributes.Append(controlFontStyle);

                XmlAttribute controlFontSize = xmlDoc.CreateAttribute("ControlFontSize");
                controlFontSize.Value = field.ControlFont.Size.ToString();
                fieldElement.Attributes.Append(controlFontSize);

                XmlAttribute controlTopPositionPercentage = xmlDoc.CreateAttribute("ControlTopPositionPercentage");
                controlTopPositionPercentage.Value = field.ControlTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlTopPositionPercentage);

                XmlAttribute controlLeftPositionPercentage = xmlDoc.CreateAttribute("ControlLeftPositionPercentage");
                controlLeftPositionPercentage.Value = field.ControlLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlLeftPositionPercentage);

                XmlAttribute controlHeightPercentage = xmlDoc.CreateAttribute("ControlHeightPercentage");
                controlHeightPercentage.Value = field.ControlHeightPercentage.ToString();
                fieldElement.Attributes.Append(controlHeightPercentage);

                XmlAttribute controlWidthPercentage = xmlDoc.CreateAttribute("ControlWidthPercentage");
                controlWidthPercentage.Value = field.ControlWidthPercentage.ToString();
                fieldElement.Attributes.Append(controlWidthPercentage);

                XmlAttribute tabIndex = xmlDoc.CreateAttribute("TabIndex");
                tabIndex.Value = field.TabIndex.ToString();
                fieldElement.Attributes.Append(tabIndex);

                XmlAttribute hasTabStop = xmlDoc.CreateAttribute("HasTabStop");
                hasTabStop.Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldElement.Attributes.Append(hasTabStop);

                XmlAttribute promptFontFamily = xmlDoc.CreateAttribute("PromptFontFamily");
                promptFontFamily.Value = field.PromptFont.FontFamily.Name;
                fieldElement.Attributes.Append(promptFontFamily);

                XmlAttribute promptFontStyle = xmlDoc.CreateAttribute("PromptFontStyle");
                promptFontStyle.Value = field.PromptFont.Style.ToString();
                fieldElement.Attributes.Append(promptFontStyle);

                XmlAttribute promptFontSize = xmlDoc.CreateAttribute("PromptFontSize");
                promptFontSize.Value = field.PromptFont.Size.ToString();
                fieldElement.Attributes.Append(promptFontSize);

                XmlAttribute promptScriptName = xmlDoc.CreateAttribute("PromptScriptName");
                promptScriptName.Value = field.PromptFont.Name;
                fieldElement.Attributes.Append(promptScriptName);

                XmlAttribute promptTopPositionPercentage = xmlDoc.CreateAttribute("PromptTopPositionPercentage");
                promptTopPositionPercentage.Value = field.PromptTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptTopPositionPercentage);

                XmlAttribute promptLeftPositionPercentage = xmlDoc.CreateAttribute("PromptLeftPositionPercentage");
                promptLeftPositionPercentage.Value = field.PromptLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptLeftPositionPercentage);

                XmlAttribute controlScriptName = xmlDoc.CreateAttribute("ControlScriptName");
                controlScriptName.Value = field.ControlFont.Name;
                fieldElement.Attributes.Append(controlScriptName);

                XmlAttribute shouldRepeatLast = xmlDoc.CreateAttribute("ShouldRepeatLast");
                shouldRepeatLast.Value = (bool.Parse(field.ShouldRepeatLast.ToString()).ToString());
                fieldElement.Attributes.Append(shouldRepeatLast);

                XmlAttribute isRequired = xmlDoc.CreateAttribute("IsRequired");
                isRequired.Value = (bool.Parse(field.IsRequired.ToString()).ToString());
                fieldElement.Attributes.Append(isRequired);

                XmlAttribute isReadOnly = xmlDoc.CreateAttribute("IsReadOnly");
                isReadOnly.Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldElement.Attributes.Append(isReadOnly);

                XmlAttribute shouldRetainImageSize = xmlDoc.CreateAttribute("ShouldRetainImageSize");
                //shouldRetainImageSize.Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();
                fieldElement.Attributes.Append(shouldRetainImageSize);

                XmlAttribute dataTableName = xmlDoc.CreateAttribute("DataTableName");
                dataTableName.Value = field.TableName;
                fieldElement.Attributes.Append(dataTableName);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute pageId = xmlDoc.CreateAttribute("PageId");
                pageId.Value = field.Page.Id.ToString();
                fieldElement.Attributes.Append(pageId);

                XmlElement checkCodeBeforeElement = xmlDoc.CreateElement("CheckCodeBefore");
                XmlAttribute checkCodeBefore = xmlDoc.CreateAttribute("CheckCodeBefore");
                checkCodeBefore.Value = field.CheckCodeBefore.ToString();
                checkCodeBeforeElement.Attributes.Append(checkCodeBefore);
                fieldElement.AppendChild(checkCodeBeforeElement);

                XmlElement checkCodeAfterElement = xmlDoc.CreateElement("CheckCodeAfter");
                XmlAttribute checkCodeAfter = xmlDoc.CreateAttribute("CheckCodeAfter");
                checkCodeAfter.Value = field.CheckCodeAfter.ToString();
                checkCodeAfterElement.Attributes.Append(checkCodeAfter);
                fieldElement.AppendChild(checkCodeAfterElement);

                fieldsNode.AppendChild(fieldElement);
                view.Project.Save();
                return field.Id;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Create Codes dropdown list field.
        /// </summary>
        /// <param name="field">Codes field to create.</param>
        /// <returns>Id of the newly created field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldPromptText = xmlDoc.CreateAttribute("PromptText");
                fieldPromptText.Value = field.PromptText;
                fieldElement.Attributes.Append(fieldPromptText);

                XmlAttribute fieldControlFontFamily = xmlDoc.CreateAttribute("ControlFontFamily");
                fieldControlFontFamily.Value = field.ControlFont.FontFamily.Name;
                fieldElement.Attributes.Append(fieldControlFontFamily);

                XmlAttribute controlFontStyle = xmlDoc.CreateAttribute("ControlFontStyle");
                controlFontStyle.Value = field.ControlFont.Style.ToString();
                fieldElement.Attributes.Append(controlFontStyle);

                XmlAttribute controlFontSize = xmlDoc.CreateAttribute("ControlFontSize");
                controlFontSize.Value = field.ControlFont.Size.ToString();
                fieldElement.Attributes.Append(controlFontSize);

                XmlAttribute controlTopPositionPercentage = xmlDoc.CreateAttribute("ControlTopPositionPercentage");
                controlTopPositionPercentage.Value = field.ControlTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlTopPositionPercentage);

                XmlAttribute controlLeftPositionPercentage = xmlDoc.CreateAttribute("ControlLeftPositionPercentage");
                controlLeftPositionPercentage.Value = field.ControlLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlLeftPositionPercentage);

                XmlAttribute controlHeightPercentage = xmlDoc.CreateAttribute("ControlHeightPercentage");
                controlHeightPercentage.Value = field.ControlHeightPercentage.ToString();
                fieldElement.Attributes.Append(controlHeightPercentage);

                XmlAttribute controlWidthPercentage = xmlDoc.CreateAttribute("ControlWidthPercentage");
                controlWidthPercentage.Value = field.ControlWidthPercentage.ToString();
                fieldElement.Attributes.Append(controlWidthPercentage);

                XmlAttribute tabIndex = xmlDoc.CreateAttribute("TabIndex");
                tabIndex.Value = field.TabIndex.ToString();
                fieldElement.Attributes.Append(tabIndex);

                XmlAttribute hasTabStop = xmlDoc.CreateAttribute("HasTabStop");
                hasTabStop.Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldElement.Attributes.Append(hasTabStop);

                XmlAttribute promptFontFamily = xmlDoc.CreateAttribute("PromptFontFamily");
                promptFontFamily.Value = field.PromptFont.FontFamily.Name;
                fieldElement.Attributes.Append(promptFontFamily);

                XmlAttribute promptFontStyle = xmlDoc.CreateAttribute("PromptFontStyle");
                promptFontStyle.Value = field.PromptFont.Style.ToString();
                fieldElement.Attributes.Append(promptFontStyle);

                XmlAttribute promptFontSize = xmlDoc.CreateAttribute("PromptFontSize");
                promptFontSize.Value = field.PromptFont.Size.ToString();
                fieldElement.Attributes.Append(promptFontSize);

                XmlAttribute promptScriptName = xmlDoc.CreateAttribute("PromptScriptName");
                promptScriptName.Value = field.PromptFont.Name;
                fieldElement.Attributes.Append(promptScriptName);

                XmlAttribute promptTopPositionPercentage = xmlDoc.CreateAttribute("PromptTopPositionPercentage");
                promptTopPositionPercentage.Value = field.PromptTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptTopPositionPercentage);

                XmlAttribute promptLeftPositionPercentage = xmlDoc.CreateAttribute("PromptLeftPositionPercentage");
                promptLeftPositionPercentage.Value = field.PromptLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptLeftPositionPercentage);

                XmlAttribute controlScriptName = xmlDoc.CreateAttribute("ControlScriptName");
                controlScriptName.Value = field.ControlFont.Name;
                fieldElement.Attributes.Append(controlScriptName);

                XmlAttribute shouldRepeatLast = xmlDoc.CreateAttribute("ShouldRepeatLast");
                shouldRepeatLast.Value = bool.Parse(field.ShouldRepeatLast.ToString()).ToString();
                fieldElement.Attributes.Append(shouldRepeatLast);

                XmlAttribute isRequired = xmlDoc.CreateAttribute("IsRequired");
                isRequired.Value = (bool.Parse(field.IsRequired.ToString()).ToString());
                fieldElement.Attributes.Append(isRequired);

                XmlAttribute isReadOnly = xmlDoc.CreateAttribute("IsReadOnly");
                isReadOnly.Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldElement.Attributes.Append(isReadOnly);

                XmlAttribute shouldRetainImageSize = xmlDoc.CreateAttribute("ShouldRetainImageSize");
                //shouldRetainImageSize.Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();
                fieldElement.Attributes.Append(shouldRetainImageSize);

                XmlAttribute sourceTableName = xmlDoc.CreateAttribute("SourceTableName");
                //sourceTableName.Value = this.source
                fieldElement.Attributes.Append(sourceTableName);

                XmlAttribute codeColumnName = xmlDoc.CreateAttribute("CodeColumnName");
                //codeColumnName.Value = this.
                fieldElement.Attributes.Append(codeColumnName);

                XmlAttribute textColumnName = xmlDoc.CreateAttribute("TextColumnName");
                //textColumnName.Value = this.
                fieldElement.Attributes.Append(textColumnName);

                XmlAttribute sort = xmlDoc.CreateAttribute("Sort");
                //sort.Value = this.sort
                fieldElement.Attributes.Append(sort);

                XmlAttribute isExclusiveTable = xmlDoc.CreateAttribute("IsExclusiveTable");
                //isExclusiveTable.Value = this.
                fieldElement.Attributes.Append(isExclusiveTable);

                XmlAttribute dataTableName = xmlDoc.CreateAttribute("DataTableName");
                dataTableName.Value = field.TableName;
                fieldElement.Attributes.Append(dataTableName);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute pageId = xmlDoc.CreateAttribute("PageId");
                pageId.Value = field.Page.Id.ToString();
                fieldElement.Attributes.Append(pageId);

                XmlElement checkCodeBeforeElement = xmlDoc.CreateElement("CheckCodeBefore");
                XmlAttribute checkCodeBefore = xmlDoc.CreateAttribute("CheckCodeBefore");
                checkCodeBefore.Value = field.CheckCodeBefore.ToString();
                checkCodeBeforeElement.Attributes.Append(checkCodeBefore);
                fieldElement.AppendChild(checkCodeBeforeElement);

                XmlElement checkCodeAfterElement = xmlDoc.CreateElement("CheckCodeAfter");
                XmlAttribute checkCodeAfter = xmlDoc.CreateAttribute("CheckCodeAfter");
                checkCodeAfter.Value = field.CheckCodeAfter.ToString();
                checkCodeAfterElement.Attributes.Append(checkCodeAfter);
                fieldElement.AppendChild(checkCodeAfterElement);

                fieldsNode.AppendChild(fieldElement);
                view.Project.Save();
                return field.Id;
            }
            finally
            {

            }
        }


        /// <summary>
        /// Create List dropdown list field.
        /// </summary>
        /// <param name="field">Codes field to create.</param>
        /// <returns>Id of the newly created field.</returns>
        public int CreateField(DDListField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("DDListField");
                }
                #endregion

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldPromptText = xmlDoc.CreateAttribute("PromptText");
                fieldPromptText.Value = field.PromptText;
                fieldElement.Attributes.Append(fieldPromptText);

                XmlAttribute fieldControlFontFamily = xmlDoc.CreateAttribute("ControlFontFamily");
                fieldControlFontFamily.Value = field.ControlFont.FontFamily.Name;
                fieldElement.Attributes.Append(fieldControlFontFamily);

                XmlAttribute controlFontStyle = xmlDoc.CreateAttribute("ControlFontStyle");
                controlFontStyle.Value = field.ControlFont.Style.ToString();
                fieldElement.Attributes.Append(controlFontStyle);

                XmlAttribute controlFontSize = xmlDoc.CreateAttribute("ControlFontSize");
                controlFontSize.Value = field.ControlFont.Size.ToString();
                fieldElement.Attributes.Append(controlFontSize);

                XmlAttribute controlTopPositionPercentage = xmlDoc.CreateAttribute("ControlTopPositionPercentage");
                controlTopPositionPercentage.Value = field.ControlTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlTopPositionPercentage);

                XmlAttribute controlLeftPositionPercentage = xmlDoc.CreateAttribute("ControlLeftPositionPercentage");
                controlLeftPositionPercentage.Value = field.ControlLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlLeftPositionPercentage);

                XmlAttribute controlHeightPercentage = xmlDoc.CreateAttribute("ControlHeightPercentage");
                controlHeightPercentage.Value = field.ControlHeightPercentage.ToString();
                fieldElement.Attributes.Append(controlHeightPercentage);

                XmlAttribute controlWidthPercentage = xmlDoc.CreateAttribute("ControlWidthPercentage");
                controlWidthPercentage.Value = field.ControlWidthPercentage.ToString();
                fieldElement.Attributes.Append(controlWidthPercentage);

                XmlAttribute tabIndex = xmlDoc.CreateAttribute("TabIndex");
                tabIndex.Value = field.TabIndex.ToString();
                fieldElement.Attributes.Append(tabIndex);

                XmlAttribute hasTabStop = xmlDoc.CreateAttribute("HasTabStop");
                hasTabStop.Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldElement.Attributes.Append(hasTabStop);

                XmlAttribute promptFontFamily = xmlDoc.CreateAttribute("PromptFontFamily");
                promptFontFamily.Value = field.PromptFont.FontFamily.Name;
                fieldElement.Attributes.Append(promptFontFamily);

                XmlAttribute promptFontStyle = xmlDoc.CreateAttribute("PromptFontStyle");
                promptFontStyle.Value = field.PromptFont.Style.ToString();
                fieldElement.Attributes.Append(promptFontStyle);

                XmlAttribute promptFontSize = xmlDoc.CreateAttribute("PromptFontSize");
                promptFontSize.Value = field.PromptFont.Size.ToString();
                fieldElement.Attributes.Append(promptFontSize);

                XmlAttribute promptScriptName = xmlDoc.CreateAttribute("PromptScriptName");
                promptScriptName.Value = field.PromptFont.Name;
                fieldElement.Attributes.Append(promptScriptName);

                XmlAttribute promptTopPositionPercentage = xmlDoc.CreateAttribute("PromptTopPositionPercentage");
                promptTopPositionPercentage.Value = field.PromptTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptTopPositionPercentage);

                XmlAttribute promptLeftPositionPercentage = xmlDoc.CreateAttribute("PromptLeftPositionPercentage");
                promptLeftPositionPercentage.Value = field.PromptLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptLeftPositionPercentage);

                XmlAttribute controlScriptName = xmlDoc.CreateAttribute("ControlScriptName");
                controlScriptName.Value = field.ControlFont.Name;
                fieldElement.Attributes.Append(controlScriptName);

                XmlAttribute shouldRepeatLast = xmlDoc.CreateAttribute("ShouldRepeatLast");
                shouldRepeatLast.Value = bool.Parse(field.ShouldRepeatLast.ToString()).ToString();
                fieldElement.Attributes.Append(shouldRepeatLast);

                XmlAttribute isRequired = xmlDoc.CreateAttribute("IsRequired");
                isRequired.Value = (bool.Parse(field.IsRequired.ToString()).ToString());
                fieldElement.Attributes.Append(isRequired);

                XmlAttribute isReadOnly = xmlDoc.CreateAttribute("IsReadOnly");
                isReadOnly.Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldElement.Attributes.Append(isReadOnly);

                XmlAttribute shouldRetainImageSize = xmlDoc.CreateAttribute("ShouldRetainImageSize");
                //shouldRetainImageSize.Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();
                fieldElement.Attributes.Append(shouldRetainImageSize);

                XmlAttribute sourceTableName = xmlDoc.CreateAttribute("SourceTableName");
                //sourceTableName.Value = this.source
                fieldElement.Attributes.Append(sourceTableName);

                XmlAttribute codeColumnName = xmlDoc.CreateAttribute("CodeColumnName");
                //codeColumnName.Value = this.
                fieldElement.Attributes.Append(codeColumnName);

                XmlAttribute textColumnName = xmlDoc.CreateAttribute("TextColumnName");
                //textColumnName.Value = this.
                fieldElement.Attributes.Append(textColumnName);

                XmlAttribute sort = xmlDoc.CreateAttribute("Sort");
                //sort.Value = this.sort
                fieldElement.Attributes.Append(sort);

                XmlAttribute isExclusiveTable = xmlDoc.CreateAttribute("IsExclusiveTable");
                //isExclusiveTable.Value = this.
                fieldElement.Attributes.Append(isExclusiveTable);

                XmlAttribute dataTableName = xmlDoc.CreateAttribute("DataTableName");
                dataTableName.Value = field.TableName;
                fieldElement.Attributes.Append(dataTableName);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute pageId = xmlDoc.CreateAttribute("PageId");
                pageId.Value = field.Page.Id.ToString();
                fieldElement.Attributes.Append(pageId);

                XmlElement checkCodeBeforeElement = xmlDoc.CreateElement("CheckCodeBefore");
                XmlAttribute checkCodeBefore = xmlDoc.CreateAttribute("CheckCodeBefore");
                checkCodeBefore.Value = field.CheckCodeBefore.ToString();
                checkCodeBeforeElement.Attributes.Append(checkCodeBefore);
                fieldElement.AppendChild(checkCodeBeforeElement);

                XmlElement checkCodeAfterElement = xmlDoc.CreateElement("CheckCodeAfter");
                XmlAttribute checkCodeAfter = xmlDoc.CreateAttribute("CheckCodeAfter");
                checkCodeAfter.Value = field.CheckCodeAfter.ToString();
                checkCodeAfterElement.Attributes.Append(checkCodeAfter);
                fieldElement.AppendChild(checkCodeAfterElement);

                fieldsNode.AppendChild(fieldElement);
                view.Project.Save();
                return field.Id;
            }
            finally
            {

            }
        }

        /// <summary>
        /// Create Comment Legal dropdown list field.
        /// </summary>
        /// <param name="field">Comment Legal field to create.</param>
        /// <returns>Id of the newly created field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldPromptText = xmlDoc.CreateAttribute("PromptText");
                fieldPromptText.Value = field.PromptText;
                fieldElement.Attributes.Append(fieldPromptText);

                XmlAttribute fieldControlFontFamily = xmlDoc.CreateAttribute("ControlFontFamily");
                fieldControlFontFamily.Value = field.ControlFont.FontFamily.Name;
                fieldElement.Attributes.Append(fieldControlFontFamily);

                XmlAttribute controlFontStyle = xmlDoc.CreateAttribute("ControlFontStyle");
                controlFontStyle.Value = field.ControlFont.Style.ToString();
                fieldElement.Attributes.Append(controlFontStyle);

                XmlAttribute controlFontSize = xmlDoc.CreateAttribute("ControlFontSize");
                controlFontSize.Value = field.ControlFont.Size.ToString();
                fieldElement.Attributes.Append(controlFontSize);

                XmlAttribute controlTopPositionPercentage = xmlDoc.CreateAttribute("ControlTopPositionPercentage");
                controlTopPositionPercentage.Value = field.ControlTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlTopPositionPercentage);

                XmlAttribute controlLeftPositionPercentage = xmlDoc.CreateAttribute("ControlLeftPositionPercentage");
                controlLeftPositionPercentage.Value = field.ControlLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlLeftPositionPercentage);

                XmlAttribute controlHeightPercentage = xmlDoc.CreateAttribute("ControlHeightPercentage");
                controlHeightPercentage.Value = field.ControlHeightPercentage.ToString();
                fieldElement.Attributes.Append(controlHeightPercentage);

                XmlAttribute controlWidthPercentage = xmlDoc.CreateAttribute("ControlWidthPercentage");
                controlWidthPercentage.Value = field.ControlWidthPercentage.ToString();
                fieldElement.Attributes.Append(controlWidthPercentage);

                XmlAttribute tabIndex = xmlDoc.CreateAttribute("TabIndex");
                tabIndex.Value = field.TabIndex.ToString();
                fieldElement.Attributes.Append(tabIndex);

                XmlAttribute hasTabStop = xmlDoc.CreateAttribute("HasTabStop");
                hasTabStop.Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldElement.Attributes.Append(hasTabStop);

                XmlAttribute promptFontFamily = xmlDoc.CreateAttribute("PromptFontFamily");
                promptFontFamily.Value = field.PromptFont.FontFamily.Name;
                fieldElement.Attributes.Append(promptFontFamily);

                XmlAttribute promptFontStyle = xmlDoc.CreateAttribute("PromptFontStyle");
                promptFontStyle.Value = field.PromptFont.Style.ToString();
                fieldElement.Attributes.Append(promptFontStyle);

                XmlAttribute promptFontSize = xmlDoc.CreateAttribute("PromptFontSize");
                promptFontSize.Value = field.PromptFont.Size.ToString();
                fieldElement.Attributes.Append(promptFontSize);

                XmlAttribute promptScriptName = xmlDoc.CreateAttribute("PromptScriptName");
                promptScriptName.Value = field.PromptFont.Name;
                fieldElement.Attributes.Append(promptScriptName);

                XmlAttribute promptTopPositionPercentage = xmlDoc.CreateAttribute("PromptTopPositionPercentage");
                promptTopPositionPercentage.Value = field.PromptTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptTopPositionPercentage);

                XmlAttribute promptLeftPositionPercentage = xmlDoc.CreateAttribute("PromptLeftPositionPercentage");
                promptLeftPositionPercentage.Value = field.PromptLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptLeftPositionPercentage);

                XmlAttribute controlScriptName = xmlDoc.CreateAttribute("ControlScriptName");
                controlScriptName.Value = field.ControlFont.Name;
                fieldElement.Attributes.Append(controlScriptName);

                XmlAttribute shouldRepeatLast = xmlDoc.CreateAttribute("ShouldRepeatLast");
                shouldRepeatLast.Value = bool.Parse(field.ShouldRepeatLast.ToString()).ToString();
                fieldElement.Attributes.Append(shouldRepeatLast);

                XmlAttribute isRequired = xmlDoc.CreateAttribute("IsRequired");
                isRequired.Value = (bool.Parse(field.IsRequired.ToString()).ToString());
                fieldElement.Attributes.Append(isRequired);

                XmlAttribute isReadOnly = xmlDoc.CreateAttribute("IsReadOnly");
                isReadOnly.Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldElement.Attributes.Append(isReadOnly);

                XmlAttribute shouldRetainImageSize = xmlDoc.CreateAttribute("ShouldRetainImageSize");
                //shouldRetainImageSize.Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();
                fieldElement.Attributes.Append(shouldRetainImageSize);

                XmlAttribute sourceTableName = xmlDoc.CreateAttribute("SourceTableName");
                //sourceTableName.Value = this.source
                fieldElement.Attributes.Append(sourceTableName);

                XmlAttribute codeColumnName = xmlDoc.CreateAttribute("CodeColumnName");
                //codeColumnName.Value = this.
                fieldElement.Attributes.Append(codeColumnName);

                XmlAttribute textColumnName = xmlDoc.CreateAttribute("TextColumnName");
                //textColumnName.Value = this.
                fieldElement.Attributes.Append(textColumnName);

                XmlAttribute sort = xmlDoc.CreateAttribute("Sort");
                //sort.Value = this.sort
                fieldElement.Attributes.Append(sort);

                XmlAttribute isExclusiveTable = xmlDoc.CreateAttribute("IsExclusiveTable");
                //isExclusiveTable.Value = this.
                fieldElement.Attributes.Append(isExclusiveTable);

                XmlAttribute dataTableName = xmlDoc.CreateAttribute("DataTableName");
                dataTableName.Value = field.TableName;
                fieldElement.Attributes.Append(dataTableName);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute pageId = xmlDoc.CreateAttribute("PageId");
                pageId.Value = field.Page.Id.ToString();
                fieldElement.Attributes.Append(pageId);

                XmlElement checkCodeBeforeElement = xmlDoc.CreateElement("CheckCodeBefore");
                XmlAttribute checkCodeBefore = xmlDoc.CreateAttribute("CheckCodeBefore");
                checkCodeBefore.Value = field.CheckCodeBefore.ToString();
                checkCodeBeforeElement.Attributes.Append(checkCodeBefore);
                fieldElement.AppendChild(checkCodeBeforeElement);

                XmlElement checkCodeAfterElement = xmlDoc.CreateElement("CheckCodeAfter");
                XmlAttribute checkCodeAfter = xmlDoc.CreateAttribute("CheckCodeAfter");
                checkCodeAfter.Value = field.CheckCodeAfter.ToString();
                checkCodeAfterElement.Attributes.Append(checkCodeAfter);
                fieldElement.AppendChild(checkCodeAfterElement);

                fieldsNode.AppendChild(fieldElement);
                view.Project.Save();
                return field.Id;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Create Legal Values dropdown list field.
        /// </summary>
        /// <param name="field">Legal Values field to create.</param>
        /// <returns>Id of the newly created field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldPromptText = xmlDoc.CreateAttribute("PromptText");
                fieldPromptText.Value = field.PromptText;
                fieldElement.Attributes.Append(fieldPromptText);

                XmlAttribute fieldControlFontFamily = xmlDoc.CreateAttribute("ControlFontFamily");
                fieldControlFontFamily.Value = field.ControlFont.FontFamily.Name;
                fieldElement.Attributes.Append(fieldControlFontFamily);

                XmlAttribute controlFontStyle = xmlDoc.CreateAttribute("ControlFontStyle");
                controlFontStyle.Value = field.ControlFont.Style.ToString();
                fieldElement.Attributes.Append(controlFontStyle);

                XmlAttribute controlFontSize = xmlDoc.CreateAttribute("ControlFontSize");
                controlFontSize.Value = field.ControlFont.Size.ToString();
                fieldElement.Attributes.Append(controlFontSize);

                XmlAttribute controlTopPositionPercentage = xmlDoc.CreateAttribute("ControlTopPositionPercentage");
                controlTopPositionPercentage.Value = field.ControlTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlTopPositionPercentage);

                XmlAttribute controlLeftPositionPercentage = xmlDoc.CreateAttribute("ControlLeftPositionPercentage");
                controlLeftPositionPercentage.Value = field.ControlLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlLeftPositionPercentage);

                XmlAttribute controlHeightPercentage = xmlDoc.CreateAttribute("ControlHeightPercentage");
                controlHeightPercentage.Value = field.ControlHeightPercentage.ToString();
                fieldElement.Attributes.Append(controlHeightPercentage);

                XmlAttribute controlWidthPercentage = xmlDoc.CreateAttribute("ControlWidthPercentage");
                controlWidthPercentage.Value = field.ControlWidthPercentage.ToString();
                fieldElement.Attributes.Append(controlWidthPercentage);

                XmlAttribute tabIndex = xmlDoc.CreateAttribute("TabIndex");
                tabIndex.Value = field.TabIndex.ToString();
                fieldElement.Attributes.Append(tabIndex);

                XmlAttribute hasTabStop = xmlDoc.CreateAttribute("HasTabStop");
                hasTabStop.Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldElement.Attributes.Append(hasTabStop);

                XmlAttribute promptFontFamily = xmlDoc.CreateAttribute("PromptFontFamily");
                promptFontFamily.Value = field.PromptFont.FontFamily.Name;
                fieldElement.Attributes.Append(promptFontFamily);

                XmlAttribute promptFontStyle = xmlDoc.CreateAttribute("PromptFontStyle");
                promptFontStyle.Value = field.PromptFont.Style.ToString();
                fieldElement.Attributes.Append(promptFontStyle);

                XmlAttribute promptFontSize = xmlDoc.CreateAttribute("PromptFontSize");
                promptFontSize.Value = field.PromptFont.Size.ToString();
                fieldElement.Attributes.Append(promptFontSize);

                XmlAttribute promptScriptName = xmlDoc.CreateAttribute("PromptScriptName");
                promptScriptName.Value = field.PromptFont.Name;
                fieldElement.Attributes.Append(promptScriptName);

                XmlAttribute promptTopPositionPercentage = xmlDoc.CreateAttribute("PromptTopPositionPercentage");
                promptTopPositionPercentage.Value = field.PromptTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptTopPositionPercentage);

                XmlAttribute promptLeftPositionPercentage = xmlDoc.CreateAttribute("PromptLeftPositionPercentage");
                promptLeftPositionPercentage.Value = field.PromptLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptLeftPositionPercentage);

                XmlAttribute controlScriptName = xmlDoc.CreateAttribute("ControlScriptName");
                controlScriptName.Value = field.ControlFont.Name;
                fieldElement.Attributes.Append(controlScriptName);

                XmlAttribute shouldRepeatLast = xmlDoc.CreateAttribute("ShouldRepeatLast");
                shouldRepeatLast.Value = bool.Parse(field.ShouldRepeatLast.ToString()).ToString();
                fieldElement.Attributes.Append(shouldRepeatLast);

                XmlAttribute isRequired = xmlDoc.CreateAttribute("IsRequired");
                isRequired.Value = (bool.Parse(field.IsRequired.ToString()).ToString());
                fieldElement.Attributes.Append(isRequired);

                XmlAttribute isReadOnly = xmlDoc.CreateAttribute("IsReadOnly");
                isReadOnly.Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldElement.Attributes.Append(isReadOnly);

                XmlAttribute shouldRetainImageSize = xmlDoc.CreateAttribute("ShouldRetainImageSize");
                //shouldRetainImageSize.Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();
                fieldElement.Attributes.Append(shouldRetainImageSize);

                XmlAttribute sourceTableName = xmlDoc.CreateAttribute("SourceTableName");
                sourceTableName.Value = field.SourceTableName.ToString();
                fieldElement.Attributes.Append(sourceTableName);

                XmlAttribute codeColumnName = xmlDoc.CreateAttribute("CodeColumnName");
                codeColumnName.Value = field.CodeColumnName.ToString();
                fieldElement.Attributes.Append(codeColumnName);

                XmlAttribute textColumnName = xmlDoc.CreateAttribute("TextColumnName");
                textColumnName.Value = field.TextColumnName.ToString();
                fieldElement.Attributes.Append(textColumnName);

                XmlAttribute sort = xmlDoc.CreateAttribute("Sort");
                sort.Value = (bool.Parse(field.ShouldSort.ToString())).ToString();
                fieldElement.Attributes.Append(sort);

                XmlAttribute isExclusiveTable = xmlDoc.CreateAttribute("IsExclusiveTable");
                isExclusiveTable.Value = (bool.Parse(field.IsExclusiveTable.ToString())).ToString();
                fieldElement.Attributes.Append(isExclusiveTable);

                XmlAttribute dataTableName = xmlDoc.CreateAttribute("DataTableName");
                dataTableName.Value = field.TableName;
                fieldElement.Attributes.Append(dataTableName);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute pageId = xmlDoc.CreateAttribute("PageId");
                pageId.Value = field.Page.Id.ToString();
                fieldElement.Attributes.Append(pageId);

                XmlElement checkCodeBeforeElement = xmlDoc.CreateElement("CheckCodeBefore");
                XmlAttribute checkCodeBefore = xmlDoc.CreateAttribute("CheckCodeBefore");
                checkCodeBefore.Value = field.CheckCodeBefore.ToString();
                checkCodeBeforeElement.Attributes.Append(checkCodeBefore);
                fieldElement.AppendChild(checkCodeBeforeElement);

                XmlElement checkCodeAfterElement = xmlDoc.CreateElement("CheckCodeAfter");
                XmlAttribute checkCodeAfter = xmlDoc.CreateAttribute("CheckCodeAfter");
                checkCodeAfter.Value = field.CheckCodeAfter.ToString();
                checkCodeAfterElement.Attributes.Append(checkCodeAfter);
                fieldElement.AppendChild(checkCodeAfterElement);

                fieldsNode.AppendChild(fieldElement);
                view.Project.Save();
                return field.Id;
            }
            finally
            {

            }
        }

        /// <summary>
        /// Create Grid field.
        /// </summary>
        /// <param name="field">Grid field to create.</param>
        /// <returns>Id of the newly created field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldPromptText = xmlDoc.CreateAttribute("PromptText");
                fieldPromptText.Value = field.PromptText;
                fieldElement.Attributes.Append(fieldPromptText);

                XmlAttribute fieldControlFontFamily = xmlDoc.CreateAttribute("ControlFontFamily");
                fieldControlFontFamily.Value = field.ControlFont.FontFamily.Name;
                fieldElement.Attributes.Append(fieldControlFontFamily);

                XmlAttribute controlFontStyle = xmlDoc.CreateAttribute("ControlFontStyle");
                controlFontStyle.Value = field.ControlFont.Style.ToString();
                fieldElement.Attributes.Append(controlFontStyle);

                XmlAttribute controlFontSize = xmlDoc.CreateAttribute("ControlFontSize");
                controlFontSize.Value = field.ControlFont.Size.ToString();
                fieldElement.Attributes.Append(controlFontSize);

                XmlAttribute controlTopPositionPercentage = xmlDoc.CreateAttribute("ControlTopPositionPercentage");
                controlTopPositionPercentage.Value = field.ControlTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlTopPositionPercentage);

                XmlAttribute controlLeftPositionPercentage = xmlDoc.CreateAttribute("ControlLeftPositionPercentage");
                controlLeftPositionPercentage.Value = field.ControlLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlLeftPositionPercentage);

                XmlAttribute controlHeightPercentage = xmlDoc.CreateAttribute("ControlHeightPercentage");
                controlHeightPercentage.Value = field.ControlHeightPercentage.ToString();
                fieldElement.Attributes.Append(controlHeightPercentage);

                XmlAttribute controlWidthPercentage = xmlDoc.CreateAttribute("ControlWidthPercentage");
                controlWidthPercentage.Value = field.ControlWidthPercentage.ToString();
                fieldElement.Attributes.Append(controlWidthPercentage);

                XmlAttribute tabIndex = xmlDoc.CreateAttribute("TabIndex");
                tabIndex.Value = field.TabIndex.ToString();
                fieldElement.Attributes.Append(tabIndex);

                XmlAttribute hasTabStop = xmlDoc.CreateAttribute("HasTabStop");
                hasTabStop.Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldElement.Attributes.Append(hasTabStop);

                XmlAttribute promptFontFamily = xmlDoc.CreateAttribute("PromptFontFamily");
                promptFontFamily.Value = field.PromptFont.FontFamily.Name;
                fieldElement.Attributes.Append(promptFontFamily);

                XmlAttribute promptFontStyle = xmlDoc.CreateAttribute("PromptFontStyle");
                promptFontStyle.Value = field.PromptFont.Style.ToString();
                fieldElement.Attributes.Append(promptFontStyle);

                XmlAttribute promptFontSize = xmlDoc.CreateAttribute("PromptFontSize");
                promptFontSize.Value = field.PromptFont.Size.ToString();
                fieldElement.Attributes.Append(promptFontSize);

                XmlAttribute promptScriptName = xmlDoc.CreateAttribute("PromptScriptName");
                promptScriptName.Value = field.PromptFont.Name;
                fieldElement.Attributes.Append(promptScriptName);

                XmlAttribute controlScriptName = xmlDoc.CreateAttribute("ControlScriptName");
                controlScriptName.Value = field.ControlFont.Name;
                fieldElement.Attributes.Append(controlScriptName);

                XmlAttribute shouldRetainImageSize = xmlDoc.CreateAttribute("ShouldRetainImageSize");
                //shouldRetainImageSize.Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();
                fieldElement.Attributes.Append(shouldRetainImageSize);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute pageId = xmlDoc.CreateAttribute("PageId");
                pageId.Value = field.Page.Id.ToString();
                fieldElement.Attributes.Append(pageId);
                fieldsNode.AppendChild(fieldElement);

                //TODO:  save columns to xml project file
                view.Project.Save();
                return field.Id;
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
                return field.Id;
            }
            finally
            {

            }
        }

        
        /// <summary>
        /// Create GUID field.
        /// </summary>
        /// <param name="field">GUID field to create.</param>
        /// <returns>Id of the newly created field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                //fieldIdAttribute.Value = Guid.NewGuid().ToString();
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldPromptText = xmlDoc.CreateAttribute("PromptText");
                fieldPromptText.Value = field.PromptText;
                fieldElement.Attributes.Append(fieldPromptText);

                XmlAttribute fieldControlFontFamily = xmlDoc.CreateAttribute("ControlFontFamily");
                fieldControlFontFamily.Value = field.ControlFont.FontFamily.Name;
                fieldElement.Attributes.Append(fieldControlFontFamily);

                XmlAttribute controlFontStyle = xmlDoc.CreateAttribute("ControlFontStyle");
                controlFontStyle.Value = field.ControlFont.Style.ToString();
                fieldElement.Attributes.Append(controlFontStyle);

                XmlAttribute controlFontSize = xmlDoc.CreateAttribute("ControlFontSize");
                controlFontSize.Value = field.ControlFont.Size.ToString();
                fieldElement.Attributes.Append(controlFontSize);

                XmlAttribute controlTopPositionPercentage = xmlDoc.CreateAttribute("ControlTopPositionPercentage");
                controlTopPositionPercentage.Value = field.ControlTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlTopPositionPercentage);

                XmlAttribute controlLeftPositionPercentage = xmlDoc.CreateAttribute("ControlLeftPositionPercentage");
                controlLeftPositionPercentage.Value = field.ControlLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlLeftPositionPercentage);

                XmlAttribute controlHeightPercentage = xmlDoc.CreateAttribute("ControlHeightPercentage");
                controlHeightPercentage.Value = field.ControlHeightPercentage.ToString();
                fieldElement.Attributes.Append(controlHeightPercentage);

                XmlAttribute controlWidthPercentage = xmlDoc.CreateAttribute("ControlWidthPercentage");
                controlWidthPercentage.Value = field.ControlWidthPercentage.ToString();
                fieldElement.Attributes.Append(controlWidthPercentage);

                XmlAttribute tabIndex = xmlDoc.CreateAttribute("TabIndex");
                tabIndex.Value = field.TabIndex.ToString();
                fieldElement.Attributes.Append(tabIndex);

                XmlAttribute hasTabStop = xmlDoc.CreateAttribute("HasTabStop");
                hasTabStop.Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldElement.Attributes.Append(hasTabStop);

                XmlAttribute promptFontFamily = xmlDoc.CreateAttribute("PromptFontFamily");
                promptFontFamily.Value = field.PromptFont.FontFamily.Name;
                fieldElement.Attributes.Append(promptFontFamily);

                XmlAttribute promptFontStyle = xmlDoc.CreateAttribute("PromptFontStyle");
                promptFontStyle.Value = field.PromptFont.Style.ToString();
                fieldElement.Attributes.Append(promptFontStyle);

                XmlAttribute promptFontSize = xmlDoc.CreateAttribute("PromptFontSize");
                promptFontSize.Value = field.PromptFont.Size.ToString();
                fieldElement.Attributes.Append(promptFontSize);

                XmlAttribute promptScriptName = xmlDoc.CreateAttribute("PromptScriptName");
                promptScriptName.Value = field.PromptFont.Name;
                fieldElement.Attributes.Append(promptScriptName);

                XmlAttribute promptTopPositionPercentage = xmlDoc.CreateAttribute("PromptTopPositionPercentage");
                promptTopPositionPercentage.Value = field.PromptTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptTopPositionPercentage);

                XmlAttribute promptLeftPositionPercentage = xmlDoc.CreateAttribute("PromptLeftPositionPercentage");
                promptLeftPositionPercentage.Value = field.PromptLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptLeftPositionPercentage);

                XmlAttribute controlScriptName = xmlDoc.CreateAttribute("ControlScriptName");
                controlScriptName.Value = field.ControlFont.Name;
                fieldElement.Attributes.Append(controlScriptName);

                XmlAttribute shouldRepeatLast = xmlDoc.CreateAttribute("ShouldRepeatLast");
                shouldRepeatLast.Value = bool.Parse(field.ShouldRepeatLast.ToString()).ToString();
                fieldElement.Attributes.Append(shouldRepeatLast);

                XmlAttribute isRequired = xmlDoc.CreateAttribute("IsRequired");
                isRequired.Value = (bool.Parse(field.IsRequired.ToString()).ToString());
                fieldElement.Attributes.Append(isRequired);

                XmlAttribute isReadOnly = xmlDoc.CreateAttribute("IsReadOnly");
                isReadOnly.Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldElement.Attributes.Append(isReadOnly);

                XmlAttribute shouldRetainImageSize = xmlDoc.CreateAttribute("ShouldRetainImageSize");
                fieldElement.Attributes.Append(shouldRetainImageSize);

                XmlAttribute maxLength = xmlDoc.CreateAttribute("MaxLength");
                maxLength.Value = field.MaxLength.ToString();
                fieldElement.Attributes.Append(maxLength);

                XmlAttribute dataTableName = xmlDoc.CreateAttribute("DataTableName");
                dataTableName.Value = field.TableName;
                fieldElement.Attributes.Append(dataTableName);

                XmlAttribute sourceFieldId = xmlDoc.CreateAttribute("SourceFieldId");
                sourceFieldId.Value = field.SourceFieldId.ToString();
                fieldElement.Attributes.Append(sourceFieldId);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute pageId = xmlDoc.CreateAttribute("PageId");
                pageId.Value = field.Page.Id.ToString();

                fieldElement.Attributes.Append(pageId);

                XmlElement checkCodeBeforeElement = xmlDoc.CreateElement("CheckCodeBefore");
                XmlAttribute checkCodeBefore = xmlDoc.CreateAttribute("CheckCodeBefore");
                checkCodeBefore.Value = field.CheckCodeBefore.ToString();
                checkCodeBeforeElement.Attributes.Append(checkCodeBefore);
                fieldElement.AppendChild(checkCodeBeforeElement);

                XmlElement checkCodeAfterElement = xmlDoc.CreateElement("CheckCodeAfter");
                XmlAttribute checkCodeAfter = xmlDoc.CreateAttribute("CheckCodeAfter");
                checkCodeAfter.Value = field.CheckCodeAfter.ToString();
                checkCodeAfterElement.Attributes.Append(checkCodeAfter);
                fieldElement.AppendChild(checkCodeAfterElement);

                fieldsNode.AppendChild(fieldElement);
                view.Project.Save();
                return field.Id;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Create Image field.
        /// </summary>
        /// <param name="field">Image field to create.</param>
        /// <returns>Id of the newly created field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldPromptText = xmlDoc.CreateAttribute("PromptText");
                fieldPromptText.Value = field.PromptText;
                fieldElement.Attributes.Append(fieldPromptText);

                XmlAttribute fieldControlFontFamily = xmlDoc.CreateAttribute("ControlFontFamily");
                fieldControlFontFamily.Value = field.ControlFont.FontFamily.Name;
                fieldElement.Attributes.Append(fieldControlFontFamily);

                XmlAttribute controlFontStyle = xmlDoc.CreateAttribute("ControlFontStyle");
                controlFontStyle.Value = field.ControlFont.Style.ToString();
                fieldElement.Attributes.Append(controlFontStyle);

                XmlAttribute controlFontSize = xmlDoc.CreateAttribute("ControlFontSize");
                controlFontSize.Value = field.ControlFont.Size.ToString();
                fieldElement.Attributes.Append(controlFontSize);

                XmlAttribute controlTopPositionPercentage = xmlDoc.CreateAttribute("ControlTopPositionPercentage");
                controlTopPositionPercentage.Value = field.ControlTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlTopPositionPercentage);

                XmlAttribute controlLeftPositionPercentage = xmlDoc.CreateAttribute("ControlLeftPositionPercentage");
                controlLeftPositionPercentage.Value = field.ControlLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlLeftPositionPercentage);

                XmlAttribute controlHeightPercentage = xmlDoc.CreateAttribute("ControlHeightPercentage");
                controlHeightPercentage.Value = field.ControlHeightPercentage.ToString();
                fieldElement.Attributes.Append(controlHeightPercentage);

                XmlAttribute controlWidthPercentage = xmlDoc.CreateAttribute("ControlWidthPercentage");
                controlWidthPercentage.Value = field.ControlWidthPercentage.ToString();
                fieldElement.Attributes.Append(controlWidthPercentage);

                XmlAttribute tabIndex = xmlDoc.CreateAttribute("TabIndex");
                tabIndex.Value = field.TabIndex.ToString();
                fieldElement.Attributes.Append(tabIndex);

                XmlAttribute hasTabStop = xmlDoc.CreateAttribute("HasTabStop");
                hasTabStop.Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldElement.Attributes.Append(hasTabStop);

                XmlAttribute promptFontFamily = xmlDoc.CreateAttribute("PromptFontFamily");
                promptFontFamily.Value = field.PromptFont.FontFamily.Name;
                fieldElement.Attributes.Append(promptFontFamily);

                XmlAttribute promptFontStyle = xmlDoc.CreateAttribute("PromptFontStyle");
                promptFontStyle.Value = field.PromptFont.Style.ToString();
                fieldElement.Attributes.Append(promptFontStyle);

                XmlAttribute promptFontSize = xmlDoc.CreateAttribute("PromptFontSize");
                promptFontSize.Value = field.PromptFont.Size.ToString();
                fieldElement.Attributes.Append(promptFontSize);

                XmlAttribute promptScriptName = xmlDoc.CreateAttribute("PromptScriptName");
                promptScriptName.Value = field.PromptFont.Name;
                fieldElement.Attributes.Append(promptScriptName);

                XmlAttribute promptTopPositionPercentage = xmlDoc.CreateAttribute("PromptTopPositionPercentage");
                promptTopPositionPercentage.Value = field.PromptTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptTopPositionPercentage);

                XmlAttribute promptLeftPositionPercentage = xmlDoc.CreateAttribute("PromptLeftPositionPercentage");
                promptLeftPositionPercentage.Value = field.PromptLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptLeftPositionPercentage);

                XmlAttribute controlScriptName = xmlDoc.CreateAttribute("ControlScriptName");
                controlScriptName.Value = field.ControlFont.Name;
                fieldElement.Attributes.Append(controlScriptName);

                XmlAttribute shouldRepeatLast = xmlDoc.CreateAttribute("ShouldRepeatLast");
                shouldRepeatLast.Value = bool.Parse(field.ShouldRepeatLast.ToString()).ToString();
                fieldElement.Attributes.Append(shouldRepeatLast);

                XmlAttribute isRequired = xmlDoc.CreateAttribute("IsRequired");
                isRequired.Value = (bool.Parse(field.IsRequired.ToString()).ToString());
                fieldElement.Attributes.Append(isRequired);

                XmlAttribute isReadOnly = xmlDoc.CreateAttribute("IsReadOnly");
                isReadOnly.Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldElement.Attributes.Append(isReadOnly);

                XmlAttribute shouldRetainImageSize = xmlDoc.CreateAttribute("ShouldRetainImageSize");
                //shouldRetainImageSize.Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();
                fieldElement.Attributes.Append(shouldRetainImageSize);

                XmlAttribute dataTableName = xmlDoc.CreateAttribute("DataTableName");
                dataTableName.Value = field.TableName;
                fieldElement.Attributes.Append(dataTableName);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute pageId = xmlDoc.CreateAttribute("PageId");
                pageId.Value = field.Page.Id.ToString();
                fieldElement.Attributes.Append(pageId);
                fieldsNode.AppendChild(fieldElement);

                view.Project.Save();
                return field.Id;
            }
            finally
            {

            }
        }

        /// <summary>
        /// Create Label field.
        /// </summary>
        /// <param name="field">Label field to create.</param>
        /// <returns>Id of the newly created field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldPromptText = xmlDoc.CreateAttribute("PromptText");
                fieldPromptText.Value = field.PromptText;
                fieldElement.Attributes.Append(fieldPromptText);

                XmlAttribute fieldControlFontName = xmlDoc.CreateAttribute("ControlScriptName");
                fieldControlFontName.Value = field.ControlFont.Name;
                fieldElement.Attributes.Append(fieldControlFontName);

                XmlAttribute fieldControlFontFamily = xmlDoc.CreateAttribute("ControlFontFamily");
                fieldControlFontFamily.Value = field.ControlFont.FontFamily.Name;
                fieldElement.Attributes.Append(fieldControlFontFamily);

                XmlAttribute controlFontStyle = xmlDoc.CreateAttribute("ControlFontStyle");
                controlFontStyle.Value = field.ControlFont.Style.ToString();
                fieldElement.Attributes.Append(controlFontStyle);

                XmlAttribute controlFontSize = xmlDoc.CreateAttribute("ControlFontSize");
                controlFontSize.Value = field.ControlFont.Size.ToString();
                fieldElement.Attributes.Append(controlFontSize);

                XmlAttribute controlTopPositionPercentage = xmlDoc.CreateAttribute("ControlTopPositionPercentage");
                controlTopPositionPercentage.Value = field.ControlTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlTopPositionPercentage);

                XmlAttribute controlLeftPositionPercentage = xmlDoc.CreateAttribute("ControlLeftPositionPercentage");
                controlLeftPositionPercentage.Value = field.ControlLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlLeftPositionPercentage);

                XmlAttribute controlHeightPercentage = xmlDoc.CreateAttribute("ControlHeightPercentage");
                controlHeightPercentage.Value = field.ControlHeightPercentage.ToString();
                fieldElement.Attributes.Append(controlHeightPercentage);

                XmlAttribute controlWidthPercentage = xmlDoc.CreateAttribute("ControlWidthPercentage");
                controlWidthPercentage.Value = field.ControlWidthPercentage.ToString();
                fieldElement.Attributes.Append(controlWidthPercentage);

                XmlAttribute tabIndex = xmlDoc.CreateAttribute("TabIndex");
                tabIndex.Value = field.TabIndex.ToString();
                fieldElement.Attributes.Append(tabIndex);

                XmlAttribute hasTabStop = xmlDoc.CreateAttribute("HasTabStop");
                hasTabStop.Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldElement.Attributes.Append(hasTabStop);

                XmlAttribute promptFontFamily = xmlDoc.CreateAttribute("PromptFontFamily");
                promptFontFamily.Value = field.PromptFont.FontFamily.Name;
                fieldElement.Attributes.Append(promptFontFamily);

                XmlAttribute promptFontStyle = xmlDoc.CreateAttribute("PromptFontStyle");
                promptFontStyle.Value = field.PromptFont.Style.ToString();
                fieldElement.Attributes.Append(promptFontStyle);

                XmlAttribute promptFontSize = xmlDoc.CreateAttribute("PromptFontSize");
                promptFontSize.Value = field.PromptFont.Size.ToString();
                fieldElement.Attributes.Append(promptFontSize);

                XmlAttribute promptScriptName = xmlDoc.CreateAttribute("PromptScriptName");
                promptScriptName.Value = field.PromptFont.Name;
                fieldElement.Attributes.Append(promptScriptName);

                XmlAttribute shouldRetainImageSize = xmlDoc.CreateAttribute("ShouldRetainImageSize");
                //shouldRetainImageSize.Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();
                fieldElement.Attributes.Append(shouldRetainImageSize);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute pageId = xmlDoc.CreateAttribute("PageId");
                pageId.Value = field.Page.Id.ToString();
                fieldElement.Attributes.Append(pageId);
                fieldsNode.AppendChild(fieldElement);

                view.Project.Save();
                return field.Id;
            }
            finally
            {

            }
        }

        /// <summary>
        /// Create Mirror field.
        /// </summary>
        /// <param name="field">Mirror field to create.</param>
        /// <returns>Id of the newly created field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldPromptText = xmlDoc.CreateAttribute("PromptText");
                fieldPromptText.Value = field.PromptText;
                fieldElement.Attributes.Append(fieldPromptText);

                XmlAttribute fieldControlFontFamily = xmlDoc.CreateAttribute("ControlFontFamily");
                fieldControlFontFamily.Value = field.ControlFont.FontFamily.Name;
                fieldElement.Attributes.Append(fieldControlFontFamily);

                XmlAttribute controlFontStyle = xmlDoc.CreateAttribute("ControlFontStyle");
                controlFontStyle.Value = field.ControlFont.Style.ToString();
                fieldElement.Attributes.Append(controlFontStyle);

                XmlAttribute controlFontSize = xmlDoc.CreateAttribute("ControlFontSize");
                controlFontSize.Value = field.ControlFont.Size.ToString();
                fieldElement.Attributes.Append(controlFontSize);

                XmlAttribute controlTopPositionPercentage = xmlDoc.CreateAttribute("ControlTopPositionPercentage");
                controlTopPositionPercentage.Value = field.ControlTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlTopPositionPercentage);

                XmlAttribute controlLeftPositionPercentage = xmlDoc.CreateAttribute("ControlLeftPositionPercentage");
                controlLeftPositionPercentage.Value = field.ControlLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlLeftPositionPercentage);

                XmlAttribute controlHeightPercentage = xmlDoc.CreateAttribute("ControlHeightPercentage");
                controlHeightPercentage.Value = field.ControlHeightPercentage.ToString();
                fieldElement.Attributes.Append(controlHeightPercentage);

                XmlAttribute controlWidthPercentage = xmlDoc.CreateAttribute("ControlWidthPercentage");
                controlWidthPercentage.Value = field.ControlWidthPercentage.ToString();
                fieldElement.Attributes.Append(controlWidthPercentage);

                XmlAttribute tabIndex = xmlDoc.CreateAttribute("TabIndex");
                tabIndex.Value = field.TabIndex.ToString();
                fieldElement.Attributes.Append(tabIndex);

                XmlAttribute hasTabStop = xmlDoc.CreateAttribute("HasTabStop");
                hasTabStop.Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldElement.Attributes.Append(hasTabStop);

                XmlAttribute promptFontFamily = xmlDoc.CreateAttribute("PromptFontFamily");
                promptFontFamily.Value = field.PromptFont.FontFamily.Name;
                fieldElement.Attributes.Append(promptFontFamily);

                XmlAttribute promptFontStyle = xmlDoc.CreateAttribute("PromptFontStyle");
                promptFontStyle.Value = field.PromptFont.Style.ToString();
                fieldElement.Attributes.Append(promptFontStyle);

                XmlAttribute promptFontSize = xmlDoc.CreateAttribute("PromptFontSize");
                promptFontSize.Value = field.PromptFont.Size.ToString();
                fieldElement.Attributes.Append(promptFontSize);

                XmlAttribute promptScriptName = xmlDoc.CreateAttribute("PromptScriptName");
                promptScriptName.Value = field.PromptFont.Name;
                fieldElement.Attributes.Append(promptScriptName);

                XmlAttribute promptTopPositionPercentage = xmlDoc.CreateAttribute("PromptTopPositionPercentage");
                promptTopPositionPercentage.Value = field.PromptTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptTopPositionPercentage);

                XmlAttribute promptLeftPositionPercentage = xmlDoc.CreateAttribute("PromptLeftPositionPercentage");
                promptLeftPositionPercentage.Value = field.PromptLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptLeftPositionPercentage);

                XmlAttribute controlScriptName = xmlDoc.CreateAttribute("ControlScriptName");
                controlScriptName.Value = field.ControlFont.Name;
                fieldElement.Attributes.Append(controlScriptName);

                XmlAttribute shouldRetainImageSize = xmlDoc.CreateAttribute("ShouldRetainImageSize");
                //shouldRetainImageSize.Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();
                fieldElement.Attributes.Append(shouldRetainImageSize);

                XmlAttribute sourceFieldId = xmlDoc.CreateAttribute("SourceFieldId");
                sourceFieldId.Value = field.SourceFieldId.ToString();
                fieldElement.Attributes.Append(sourceFieldId);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute pageId = xmlDoc.CreateAttribute("PageId");
                pageId.Value = field.Page.Id.ToString();
                fieldElement.Attributes.Append(pageId);

                fieldsNode.AppendChild(fieldElement);
                view.Project.Save();
                return field.Id;
            }
            finally
            {

            }
        }

        /// <summary>
        /// Create Multiline text field.
        /// </summary>
        /// <param name="field">Multiline text field to create.</param>
        /// <returns>Id of the newly created field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldPromptText = xmlDoc.CreateAttribute("PromptText");
                fieldPromptText.Value = field.PromptText;
                fieldElement.Attributes.Append(fieldPromptText);

                XmlAttribute fieldControlFontFamily = xmlDoc.CreateAttribute("ControlFontFamily");
                fieldControlFontFamily.Value = field.ControlFont.FontFamily.Name;
                fieldElement.Attributes.Append(fieldControlFontFamily);

                XmlAttribute controlFontStyle = xmlDoc.CreateAttribute("ControlFontStyle");
                controlFontStyle.Value = field.ControlFont.Style.ToString();
                fieldElement.Attributes.Append(controlFontStyle);

                XmlAttribute controlFontSize = xmlDoc.CreateAttribute("ControlFontSize");
                controlFontSize.Value = field.ControlFont.Size.ToString();
                fieldElement.Attributes.Append(controlFontSize);

                XmlAttribute controlTopPositionPercentage = xmlDoc.CreateAttribute("ControlTopPositionPercentage");
                controlTopPositionPercentage.Value = field.ControlTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlTopPositionPercentage);

                XmlAttribute controlLeftPositionPercentage = xmlDoc.CreateAttribute("ControlLeftPositionPercentage");
                controlLeftPositionPercentage.Value = field.ControlLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlLeftPositionPercentage);

                XmlAttribute controlHeightPercentage = xmlDoc.CreateAttribute("ControlHeightPercentage");
                controlHeightPercentage.Value = field.ControlHeightPercentage.ToString();
                fieldElement.Attributes.Append(controlHeightPercentage);

                XmlAttribute controlWidthPercentage = xmlDoc.CreateAttribute("ControlWidthPercentage");
                controlWidthPercentage.Value = field.ControlWidthPercentage.ToString();
                fieldElement.Attributes.Append(controlWidthPercentage);

                XmlAttribute tabIndex = xmlDoc.CreateAttribute("TabIndex");
                tabIndex.Value = field.TabIndex.ToString();
                fieldElement.Attributes.Append(tabIndex);

                XmlAttribute hasTabStop = xmlDoc.CreateAttribute("HasTabStop");
                hasTabStop.Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldElement.Attributes.Append(hasTabStop);

                XmlAttribute promptFontFamily = xmlDoc.CreateAttribute("PromptFontFamily");
                promptFontFamily.Value = field.PromptFont.FontFamily.Name;
                fieldElement.Attributes.Append(promptFontFamily);

                XmlAttribute promptFontStyle = xmlDoc.CreateAttribute("PromptFontStyle");
                promptFontStyle.Value = field.PromptFont.Style.ToString();
                fieldElement.Attributes.Append(promptFontStyle);

                XmlAttribute promptFontSize = xmlDoc.CreateAttribute("PromptFontSize");
                promptFontSize.Value = field.PromptFont.Size.ToString();
                fieldElement.Attributes.Append(promptFontSize);

                XmlAttribute promptScriptName = xmlDoc.CreateAttribute("PromptScriptName");
                promptScriptName.Value = field.PromptFont.Name;
                fieldElement.Attributes.Append(promptScriptName);

                XmlAttribute promptTopPositionPercentage = xmlDoc.CreateAttribute("PromptTopPositionPercentage");
                promptTopPositionPercentage.Value = field.PromptTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptTopPositionPercentage);

                XmlAttribute promptLeftPositionPercentage = xmlDoc.CreateAttribute("PromptLeftPositionPercentage");
                promptLeftPositionPercentage.Value = field.PromptLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptLeftPositionPercentage);

                XmlAttribute controlScriptName = xmlDoc.CreateAttribute("ControlScriptName");
                controlScriptName.Value = field.ControlFont.Name;
                fieldElement.Attributes.Append(controlScriptName);

                XmlAttribute shouldRepeatLast = xmlDoc.CreateAttribute("ShouldRepeatLast");
                shouldRepeatLast.Value = bool.Parse(field.ShouldRepeatLast.ToString()).ToString();
                fieldElement.Attributes.Append(shouldRepeatLast);

                XmlAttribute isRequired = xmlDoc.CreateAttribute("IsRequired");
                isRequired.Value = (bool.Parse(field.IsRequired.ToString()).ToString());
                fieldElement.Attributes.Append(isRequired);

                XmlAttribute isReadOnly = xmlDoc.CreateAttribute("IsReadOnly");
                isReadOnly.Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldElement.Attributes.Append(isReadOnly);

                XmlAttribute shouldRetainImageSize = xmlDoc.CreateAttribute("ShouldRetainImageSize");
                //shouldRetainImageSize.Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();
                fieldElement.Attributes.Append(shouldRetainImageSize);

                XmlAttribute maxLength = xmlDoc.CreateAttribute("MaxLength");
                maxLength.Value = field.MaxLength.ToString();
                fieldElement.Attributes.Append(maxLength);

                XmlAttribute dataTableName = xmlDoc.CreateAttribute("DataTableName");
                dataTableName.Value = field.TableName;
                fieldElement.Attributes.Append(dataTableName);

                XmlAttribute sourceFieldId = xmlDoc.CreateAttribute("SourceFieldId");
                sourceFieldId.Value = field.SourceFieldId.ToString();
                fieldElement.Attributes.Append(sourceFieldId);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute pageId = xmlDoc.CreateAttribute("PageId");
                pageId.Value = field.Page.Id.ToString();
                fieldElement.Attributes.Append(pageId);

                XmlElement checkCodeBeforeElement = xmlDoc.CreateElement("CheckCodeBefore");
                XmlAttribute checkCodeBefore = xmlDoc.CreateAttribute("CheckCodeBefore");
                checkCodeBefore.Value = field.CheckCodeBefore.ToString();
                checkCodeBeforeElement.Attributes.Append(checkCodeBefore);
                fieldElement.AppendChild(checkCodeBeforeElement);

                XmlElement checkCodeAfterElement = xmlDoc.CreateElement("CheckCodeAfter");
                XmlAttribute checkCodeAfter = xmlDoc.CreateAttribute("CheckCodeAfter");
                checkCodeAfter.Value = field.CheckCodeAfter.ToString();
                checkCodeAfterElement.Attributes.Append(checkCodeAfter);
                fieldElement.AppendChild(checkCodeAfterElement);

                fieldsNode.AppendChild(fieldElement);
                view.Project.Save();
                return field.Id;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Create Number field.
        /// </summary>
        /// <param name="field">Number field to create.</param>
        /// <returns>Id of the newly created field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldPromptText = xmlDoc.CreateAttribute("PromptText");
                fieldPromptText.Value = field.PromptText;
                fieldElement.Attributes.Append(fieldPromptText);

                XmlAttribute fieldControlFontFamily = xmlDoc.CreateAttribute("ControlFontFamily");
                fieldControlFontFamily.Value = field.ControlFont.FontFamily.Name;
                fieldElement.Attributes.Append(fieldControlFontFamily);

                XmlAttribute controlFontStyle = xmlDoc.CreateAttribute("ControlFontStyle");
                controlFontStyle.Value = field.ControlFont.Style.ToString();
                fieldElement.Attributes.Append(controlFontStyle);

                XmlAttribute controlFontSize = xmlDoc.CreateAttribute("ControlFontSize");
                controlFontSize.Value = field.ControlFont.Size.ToString();
                fieldElement.Attributes.Append(controlFontSize);

                XmlAttribute controlTopPositionPercentage = xmlDoc.CreateAttribute("ControlTopPositionPercentage");
                controlTopPositionPercentage.Value = field.ControlTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlTopPositionPercentage);

                XmlAttribute controlLeftPositionPercentage = xmlDoc.CreateAttribute("ControlLeftPositionPercentage");
                controlLeftPositionPercentage.Value = field.ControlLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlLeftPositionPercentage);

                XmlAttribute controlHeightPercentage = xmlDoc.CreateAttribute("ControlHeightPercentage");
                controlHeightPercentage.Value = field.ControlHeightPercentage.ToString();
                fieldElement.Attributes.Append(controlHeightPercentage);

                XmlAttribute controlWidthPercentage = xmlDoc.CreateAttribute("ControlWidthPercentage");
                controlWidthPercentage.Value = field.ControlWidthPercentage.ToString();
                fieldElement.Attributes.Append(controlWidthPercentage);

                XmlAttribute tabIndex = xmlDoc.CreateAttribute("TabIndex");
                tabIndex.Value = field.TabIndex.ToString();
                fieldElement.Attributes.Append(tabIndex);

                XmlAttribute hasTabStop = xmlDoc.CreateAttribute("HasTabStop");
                hasTabStop.Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldElement.Attributes.Append(hasTabStop);

                XmlAttribute promptFontFamily = xmlDoc.CreateAttribute("PromptFontFamily");
                promptFontFamily.Value = field.PromptFont.FontFamily.Name;
                fieldElement.Attributes.Append(promptFontFamily);

                XmlAttribute promptFontStyle = xmlDoc.CreateAttribute("PromptFontStyle");
                promptFontStyle.Value = field.PromptFont.Style.ToString();
                fieldElement.Attributes.Append(promptFontStyle);

                XmlAttribute promptFontSize = xmlDoc.CreateAttribute("PromptFontSize");
                promptFontSize.Value = field.PromptFont.Size.ToString();
                fieldElement.Attributes.Append(promptFontSize);

                XmlAttribute promptScriptName = xmlDoc.CreateAttribute("PromptScriptName");
                promptScriptName.Value = field.PromptFont.Name;
                fieldElement.Attributes.Append(promptScriptName);

                XmlAttribute promptTopPositionPercentage = xmlDoc.CreateAttribute("PromptTopPositionPercentage");
                promptTopPositionPercentage.Value = field.PromptTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptTopPositionPercentage);

                XmlAttribute promptLeftPositionPercentage = xmlDoc.CreateAttribute("PromptLeftPositionPercentage");
                promptLeftPositionPercentage.Value = field.PromptLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptLeftPositionPercentage);

                XmlAttribute controlScriptName = xmlDoc.CreateAttribute("ControlScriptName");
                controlScriptName.Value = field.ControlFont.Name;
                fieldElement.Attributes.Append(controlScriptName);

                XmlAttribute shouldRepeatLast = xmlDoc.CreateAttribute("ShouldRepeatLast");
                shouldRepeatLast.Value = bool.Parse(field.ShouldRepeatLast.ToString()).ToString();
                fieldElement.Attributes.Append(shouldRepeatLast);

                XmlAttribute isRequired = xmlDoc.CreateAttribute("IsRequired");
                isRequired.Value = (bool.Parse(field.IsRequired.ToString()).ToString());
                fieldElement.Attributes.Append(isRequired);

                XmlAttribute isReadOnly = xmlDoc.CreateAttribute("IsReadOnly");
                isReadOnly.Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldElement.Attributes.Append(isReadOnly);

                XmlAttribute shouldRetainImageSize = xmlDoc.CreateAttribute("ShouldRetainImageSize");
                //shouldRetainImageSize.Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();
                fieldElement.Attributes.Append(shouldRetainImageSize);

                XmlAttribute lower = xmlDoc.CreateAttribute("Lower");
                lower.Value = field.Lower.ToString();
                fieldElement.Attributes.Append(lower);

                XmlAttribute upper = xmlDoc.CreateAttribute("Upper");
                upper.Value = field.Upper.ToString();
                fieldElement.Attributes.Append(upper);

                XmlAttribute pattern = xmlDoc.CreateAttribute("Pattern");
                pattern.Value = field.Pattern.ToString();
                fieldElement.Attributes.Append(pattern);

                XmlAttribute dataTableName = xmlDoc.CreateAttribute("DataTableName");
                dataTableName.Value = field.TableName;
                fieldElement.Attributes.Append(dataTableName);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute pageId = xmlDoc.CreateAttribute("PageId");
                pageId.Value = field.Page.Id.ToString();
                fieldElement.Attributes.Append(pageId);

                XmlElement checkCodeBeforeElement = xmlDoc.CreateElement("CheckCodeBefore");
                XmlAttribute checkCodeBefore = xmlDoc.CreateAttribute("CheckCodeBefore");
                checkCodeBefore.Value = field.CheckCodeBefore.ToString();
                checkCodeBeforeElement.Attributes.Append(checkCodeBefore);
                fieldElement.AppendChild(checkCodeBeforeElement);

                XmlElement checkCodeAfterElement = xmlDoc.CreateElement("CheckCodeAfter");
                XmlAttribute checkCodeAfter = xmlDoc.CreateAttribute("CheckCodeAfter");
                checkCodeAfter.Value = field.CheckCodeAfter.ToString();
                checkCodeAfterElement.Attributes.Append(checkCodeAfter);
                fieldElement.AppendChild(checkCodeAfterElement);

                fieldsNode.AppendChild(fieldElement);
                view.Project.Save();
                return field.Id;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Create Option field.
        /// </summary>
        /// <param name="field">Option field to create.</param>
        /// <returns>Id of the newly created field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldPromptText = xmlDoc.CreateAttribute("PromptText");
                fieldPromptText.Value = field.PromptText;
                fieldElement.Attributes.Append(fieldPromptText);

                XmlAttribute fieldControlFontFamily = xmlDoc.CreateAttribute("ControlFontFamily");
                fieldControlFontFamily.Value = field.ControlFont.FontFamily.Name;
                fieldElement.Attributes.Append(fieldControlFontFamily);

                XmlAttribute controlFontStyle = xmlDoc.CreateAttribute("ControlFontStyle");
                controlFontStyle.Value = field.ControlFont.Style.ToString();
                fieldElement.Attributes.Append(controlFontStyle);

                XmlAttribute controlFontSize = xmlDoc.CreateAttribute("ControlFontSize");
                controlFontSize.Value = field.ControlFont.Size.ToString();
                fieldElement.Attributes.Append(controlFontSize);

                XmlAttribute controlTopPositionPercentage = xmlDoc.CreateAttribute("ControlTopPositionPercentage");
                controlTopPositionPercentage.Value = field.ControlTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlTopPositionPercentage);

                XmlAttribute controlLeftPositionPercentage = xmlDoc.CreateAttribute("ControlLeftPositionPercentage");
                controlLeftPositionPercentage.Value = field.ControlLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlLeftPositionPercentage);

                XmlAttribute controlHeightPercentage = xmlDoc.CreateAttribute("ControlHeightPercentage");
                controlHeightPercentage.Value = field.ControlHeightPercentage.ToString();
                fieldElement.Attributes.Append(controlHeightPercentage);

                XmlAttribute controlWidthPercentage = xmlDoc.CreateAttribute("ControlWidthPercentage");
                controlWidthPercentage.Value = field.ControlWidthPercentage.ToString();
                fieldElement.Attributes.Append(controlWidthPercentage);

                XmlAttribute tabIndex = xmlDoc.CreateAttribute("TabIndex");
                tabIndex.Value = field.TabIndex.ToString();
                fieldElement.Attributes.Append(tabIndex);

                XmlAttribute hasTabStop = xmlDoc.CreateAttribute("HasTabStop");
                hasTabStop.Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldElement.Attributes.Append(hasTabStop);

                XmlAttribute promptFontFamily = xmlDoc.CreateAttribute("PromptFontFamily");
                promptFontFamily.Value = field.PromptFont.FontFamily.Name;
                fieldElement.Attributes.Append(promptFontFamily);

                XmlAttribute promptFontStyle = xmlDoc.CreateAttribute("PromptFontStyle");
                promptFontStyle.Value = field.PromptFont.Style.ToString();
                fieldElement.Attributes.Append(promptFontStyle);

                XmlAttribute promptFontSize = xmlDoc.CreateAttribute("PromptFontSize");
                promptFontSize.Value = field.PromptFont.Size.ToString();
                fieldElement.Attributes.Append(promptFontSize);

                XmlAttribute promptScriptName = xmlDoc.CreateAttribute("PromptScriptName");
                promptScriptName.Value = field.PromptFont.Name;
                fieldElement.Attributes.Append(promptScriptName);

                XmlAttribute controlScriptName = xmlDoc.CreateAttribute("ControlScriptName");
                controlScriptName.Value = field.ControlFont.Name;
                fieldElement.Attributes.Append(controlScriptName);

                XmlAttribute shouldRepeatLast = xmlDoc.CreateAttribute("ShouldRepeatLast");
                shouldRepeatLast.Value = bool.Parse(field.ShouldRepeatLast.ToString()).ToString();
                fieldElement.Attributes.Append(shouldRepeatLast);

                XmlAttribute isRequired = xmlDoc.CreateAttribute("IsRequired");
                isRequired.Value = (bool.Parse(field.IsRequired.ToString()).ToString());
                fieldElement.Attributes.Append(isRequired);

                XmlAttribute isReadOnly = xmlDoc.CreateAttribute("IsReadOnly");
                isReadOnly.Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldElement.Attributes.Append(isReadOnly);

                XmlAttribute shouldRetainImageSize = xmlDoc.CreateAttribute("ShouldRetainImageSize");
                //shouldRetainImageSize.Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();
                fieldElement.Attributes.Append(shouldRetainImageSize);

                XmlAttribute showTextOnRight = xmlDoc.CreateAttribute("ShowTextOnRight");
                showTextOnRight.Value = (bool.Parse(field.ShowTextOnRight.ToString())).ToString();
                fieldElement.Attributes.Append(showTextOnRight);

                XmlAttribute dataTableName = xmlDoc.CreateAttribute("DataTableName");
                dataTableName.Value = field.TableName;
                fieldElement.Attributes.Append(dataTableName);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute pageId = xmlDoc.CreateAttribute("PageId");
                pageId.Value = field.Page.Id.ToString();
                fieldElement.Attributes.Append(pageId);

                XmlElement checkCodeAfterElement = xmlDoc.CreateElement("CheckCodeAfter");
                XmlAttribute checkCodeAfter = xmlDoc.CreateAttribute("CheckCodeAfter");
                checkCodeAfter.Value = field.CheckCodeAfter.ToString();
                checkCodeAfterElement.Attributes.Append(checkCodeAfter);
                fieldElement.AppendChild(checkCodeAfterElement);

                fieldsNode.AppendChild(fieldElement);
                view.Project.Save();
                return field.Id;

                //TODO:  Save option items in xml project file - EJ
            }
            finally
            {
            }
        }

        /// <summary>
        /// Create Phone number field.
        /// </summary>
        /// <param name="field">Phone number field to create.</param>
        /// <returns>Id of the newly created field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldPromptText = xmlDoc.CreateAttribute("PromptText");
                fieldPromptText.Value = field.PromptText;
                fieldElement.Attributes.Append(fieldPromptText);

                XmlAttribute fieldControlFontFamily = xmlDoc.CreateAttribute("ControlFontFamily");
                fieldControlFontFamily.Value = field.ControlFont.FontFamily.Name;
                fieldElement.Attributes.Append(fieldControlFontFamily);

                XmlAttribute controlFontStyle = xmlDoc.CreateAttribute("ControlFontStyle");
                controlFontStyle.Value = field.ControlFont.Style.ToString();
                fieldElement.Attributes.Append(controlFontStyle);

                XmlAttribute controlFontSize = xmlDoc.CreateAttribute("ControlFontSize");
                controlFontSize.Value = field.ControlFont.Size.ToString();
                fieldElement.Attributes.Append(controlFontSize);

                XmlAttribute controlTopPositionPercentage = xmlDoc.CreateAttribute("ControlTopPositionPercentage");
                controlTopPositionPercentage.Value = field.ControlTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlTopPositionPercentage);

                XmlAttribute controlLeftPositionPercentage = xmlDoc.CreateAttribute("ControlLeftPositionPercentage");
                controlLeftPositionPercentage.Value = field.ControlLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlLeftPositionPercentage);

                XmlAttribute controlHeightPercentage = xmlDoc.CreateAttribute("ControlHeightPercentage");
                controlHeightPercentage.Value = field.ControlHeightPercentage.ToString();
                fieldElement.Attributes.Append(controlHeightPercentage);

                XmlAttribute controlWidthPercentage = xmlDoc.CreateAttribute("ControlWidthPercentage");
                controlWidthPercentage.Value = field.ControlWidthPercentage.ToString();
                fieldElement.Attributes.Append(controlWidthPercentage);

                XmlAttribute tabIndex = xmlDoc.CreateAttribute("TabIndex");
                tabIndex.Value = field.TabIndex.ToString();
                fieldElement.Attributes.Append(tabIndex);

                XmlAttribute hasTabStop = xmlDoc.CreateAttribute("HasTabStop");
                hasTabStop.Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldElement.Attributes.Append(hasTabStop);

                XmlAttribute promptFontFamily = xmlDoc.CreateAttribute("PromptFontFamily");
                promptFontFamily.Value = field.PromptFont.FontFamily.Name;
                fieldElement.Attributes.Append(promptFontFamily);

                XmlAttribute promptFontStyle = xmlDoc.CreateAttribute("PromptFontStyle");
                promptFontStyle.Value = field.PromptFont.Style.ToString();
                fieldElement.Attributes.Append(promptFontStyle);

                XmlAttribute promptFontSize = xmlDoc.CreateAttribute("PromptFontSize");
                promptFontSize.Value = field.PromptFont.Size.ToString();
                fieldElement.Attributes.Append(promptFontSize);

                XmlAttribute promptScriptName = xmlDoc.CreateAttribute("PromptScriptName");
                promptScriptName.Value = field.PromptFont.Name;
                fieldElement.Attributes.Append(promptScriptName);

                XmlAttribute promptTopPositionPercentage = xmlDoc.CreateAttribute("PromptTopPositionPercentage");
                promptTopPositionPercentage.Value = field.PromptTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptTopPositionPercentage);

                XmlAttribute promptLeftPositionPercentage = xmlDoc.CreateAttribute("PromptLeftPositionPercentage");
                promptLeftPositionPercentage.Value = field.PromptLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptLeftPositionPercentage);

                XmlAttribute controlScriptName = xmlDoc.CreateAttribute("ControlScriptName");
                controlScriptName.Value = field.ControlFont.Name;
                fieldElement.Attributes.Append(controlScriptName);

                XmlAttribute shouldRepeatLast = xmlDoc.CreateAttribute("ShouldRepeatLast");
                shouldRepeatLast.Value = bool.Parse(field.ShouldRepeatLast.ToString()).ToString();
                fieldElement.Attributes.Append(shouldRepeatLast);

                XmlAttribute isRequired = xmlDoc.CreateAttribute("IsRequired");
                isRequired.Value = (bool.Parse(field.IsRequired.ToString()).ToString());
                fieldElement.Attributes.Append(isRequired);

                XmlAttribute isReadOnly = xmlDoc.CreateAttribute("IsReadOnly");
                isReadOnly.Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldElement.Attributes.Append(isReadOnly);

                XmlAttribute shouldRetainImageSize = xmlDoc.CreateAttribute("ShouldRetainImageSize");
                //shouldRetainImageSize.Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();
                fieldElement.Attributes.Append(shouldRetainImageSize);

                XmlAttribute pattern = xmlDoc.CreateAttribute("Pattern");
                pattern.Value = field.Pattern.ToString();
                fieldElement.Attributes.Append(pattern);

                XmlAttribute dataTableName = xmlDoc.CreateAttribute("DataTableName");
                dataTableName.Value = field.TableName;
                fieldElement.Attributes.Append(dataTableName);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute pageId = xmlDoc.CreateAttribute("PageId");
                pageId.Value = field.Page.Id.ToString();
                fieldElement.Attributes.Append(pageId);

                XmlElement checkCodeBeforeElement = xmlDoc.CreateElement("CheckCodeBefore");
                XmlAttribute checkCodeBefore = xmlDoc.CreateAttribute("CheckCodeBefore");
                checkCodeBefore.Value = field.CheckCodeBefore.ToString();
                checkCodeBeforeElement.Attributes.Append(checkCodeBefore);
                fieldElement.AppendChild(checkCodeBeforeElement);

                XmlElement checkCodeAfterElement = xmlDoc.CreateElement("CheckCodeAfter");
                XmlAttribute checkCodeAfter = xmlDoc.CreateAttribute("CheckCodeAfter");
                checkCodeAfter.Value = field.CheckCodeAfter.ToString();
                checkCodeAfterElement.Attributes.Append(checkCodeAfter);
                fieldElement.AppendChild(checkCodeAfterElement);

                fieldsNode.AppendChild(fieldElement);
                view.Project.Save();
                return field.Id;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Create Related view field.
        /// </summary>
        /// <param name="field">Related view field to create.</param>
        /// <returns>Id of the newly created field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldPromptText = xmlDoc.CreateAttribute("PromptText");
                fieldPromptText.Value = field.PromptText;
                fieldElement.Attributes.Append(fieldPromptText);

                XmlAttribute controlScriptName = xmlDoc.CreateAttribute("ControlScriptName");
                controlScriptName.Value = field.ControlFont.Name;
                fieldElement.Attributes.Append(controlScriptName);

                XmlAttribute fieldControlFontFamily = xmlDoc.CreateAttribute("ControlFontFamily");
                fieldControlFontFamily.Value = field.ControlFont.FontFamily.Name;
                fieldElement.Attributes.Append(fieldControlFontFamily);

                XmlAttribute controlFontStyle = xmlDoc.CreateAttribute("ControlFontStyle");
                controlFontStyle.Value = field.ControlFont.Style.ToString();
                fieldElement.Attributes.Append(controlFontStyle);

                XmlAttribute controlFontSize = xmlDoc.CreateAttribute("ControlFontSize");
                controlFontSize.Value = field.ControlFont.Size.ToString();
                fieldElement.Attributes.Append(controlFontSize);

                XmlAttribute controlTopPositionPercentage = xmlDoc.CreateAttribute("ControlTopPositionPercentage");
                controlTopPositionPercentage.Value = field.ControlTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlTopPositionPercentage);

                XmlAttribute controlLeftPositionPercentage = xmlDoc.CreateAttribute("ControlLeftPositionPercentage");
                controlLeftPositionPercentage.Value = field.ControlLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlLeftPositionPercentage);

                XmlAttribute controlHeightPercentage = xmlDoc.CreateAttribute("ControlHeightPercentage");
                controlHeightPercentage.Value = field.ControlHeightPercentage.ToString();
                fieldElement.Attributes.Append(controlHeightPercentage);

                XmlAttribute controlWidthPercentage = xmlDoc.CreateAttribute("ControlWidthPercentage");
                controlWidthPercentage.Value = field.ControlWidthPercentage.ToString();
                fieldElement.Attributes.Append(controlWidthPercentage);

                XmlAttribute tabIndex = xmlDoc.CreateAttribute("TabIndex");
                tabIndex.Value = field.TabIndex.ToString();
                fieldElement.Attributes.Append(tabIndex);

                XmlAttribute hasTabStop = xmlDoc.CreateAttribute("HasTabStop");
                hasTabStop.Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldElement.Attributes.Append(hasTabStop);

                XmlAttribute promptFontFamily = xmlDoc.CreateAttribute("PromptFontFamily");
                promptFontFamily.Value = field.PromptFont.FontFamily.Name;
                fieldElement.Attributes.Append(promptFontFamily);

                XmlAttribute promptFontStyle = xmlDoc.CreateAttribute("PromptFontStyle");
                promptFontStyle.Value = field.PromptFont.Style.ToString();
                fieldElement.Attributes.Append(promptFontStyle);

                XmlAttribute promptFontSize = xmlDoc.CreateAttribute("PromptFontSize");
                promptFontSize.Value = field.PromptFont.Size.ToString();
                fieldElement.Attributes.Append(promptFontSize);

                XmlAttribute promptScriptName = xmlDoc.CreateAttribute("PromptScriptName");
                promptScriptName.Value = field.PromptFont.Name;
                fieldElement.Attributes.Append(promptScriptName);

                XmlAttribute shouldRetainImageSize = xmlDoc.CreateAttribute("ShouldRetainImageSize");
                //shouldRetainImageSize.Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();
                fieldElement.Attributes.Append(shouldRetainImageSize);

                XmlAttribute relateCondition = xmlDoc.CreateAttribute("RelateCondition");
                relateCondition.Value = field.Condition.ToString();
                fieldElement.Attributes.Append(relateCondition);

                XmlAttribute shouldReturnToParent = xmlDoc.CreateAttribute("ShouldReturnToParent");
                shouldReturnToParent.Value = (bool.Parse(field.ShouldReturnToParent.ToString())).ToString();
                fieldElement.Attributes.Append(shouldReturnToParent);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute relatedViewId = xmlDoc.CreateAttribute("RelatedViewId");
                relatedViewId.Value = field.RelatedViewID.ToString();
                fieldElement.Attributes.Append(relatedViewId);

                XmlAttribute pageId = xmlDoc.CreateAttribute("PageId");
                pageId.Value = field.Page.Id.ToString();
                fieldElement.Attributes.Append(pageId);

                fieldsNode.AppendChild(fieldElement);
                view.Project.Save();
                return field.Id;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Create single line text field.
        /// </summary>
        /// <param name="field">Single line text field to create.</param>
        /// <returns>Id of the newly created field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                //fieldIdAttribute.Value = Guid.NewGuid().ToString();
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldPromptText = xmlDoc.CreateAttribute("PromptText");
                fieldPromptText.Value = field.PromptText;
                fieldElement.Attributes.Append(fieldPromptText);

                XmlAttribute fieldControlFontFamily = xmlDoc.CreateAttribute("ControlFontFamily");
                fieldControlFontFamily.Value = field.ControlFont.FontFamily.Name;
                fieldElement.Attributes.Append(fieldControlFontFamily);

                XmlAttribute controlFontStyle = xmlDoc.CreateAttribute("ControlFontStyle");
                controlFontStyle.Value = field.ControlFont.Style.ToString();
                fieldElement.Attributes.Append(controlFontStyle);

                XmlAttribute controlFontSize = xmlDoc.CreateAttribute("ControlFontSize");
                controlFontSize.Value = field.ControlFont.Size.ToString();
                fieldElement.Attributes.Append(controlFontSize);

                XmlAttribute controlTopPositionPercentage = xmlDoc.CreateAttribute("ControlTopPositionPercentage");
                controlTopPositionPercentage.Value = field.ControlTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlTopPositionPercentage);

                XmlAttribute controlLeftPositionPercentage = xmlDoc.CreateAttribute("ControlLeftPositionPercentage");
                controlLeftPositionPercentage.Value = field.ControlLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlLeftPositionPercentage);

                XmlAttribute controlHeightPercentage = xmlDoc.CreateAttribute("ControlHeightPercentage");
                controlHeightPercentage.Value = field.ControlHeightPercentage.ToString();
                fieldElement.Attributes.Append(controlHeightPercentage);

                XmlAttribute controlWidthPercentage = xmlDoc.CreateAttribute("ControlWidthPercentage");
                controlWidthPercentage.Value = field.ControlWidthPercentage.ToString();
                fieldElement.Attributes.Append(controlWidthPercentage);

                XmlAttribute tabIndex = xmlDoc.CreateAttribute("TabIndex");
                tabIndex.Value = field.TabIndex.ToString();
                fieldElement.Attributes.Append(tabIndex);

                XmlAttribute hasTabStop = xmlDoc.CreateAttribute("HasTabStop");
                hasTabStop.Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldElement.Attributes.Append(hasTabStop);

                XmlAttribute promptFontFamily = xmlDoc.CreateAttribute("PromptFontFamily");
                promptFontFamily.Value = field.PromptFont.FontFamily.Name;
                fieldElement.Attributes.Append(promptFontFamily);

                XmlAttribute promptFontStyle = xmlDoc.CreateAttribute("PromptFontStyle");
                promptFontStyle.Value = field.PromptFont.Style.ToString();
                fieldElement.Attributes.Append(promptFontStyle);

                XmlAttribute promptFontSize = xmlDoc.CreateAttribute("PromptFontSize");
                promptFontSize.Value = field.PromptFont.Size.ToString();
                fieldElement.Attributes.Append(promptFontSize);

                XmlAttribute promptScriptName = xmlDoc.CreateAttribute("PromptScriptName");
                promptScriptName.Value = field.PromptFont.Name;
                fieldElement.Attributes.Append(promptScriptName);

                XmlAttribute promptTopPositionPercentage = xmlDoc.CreateAttribute("PromptTopPositionPercentage");
                promptTopPositionPercentage.Value = field.PromptTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptTopPositionPercentage);

                XmlAttribute promptLeftPositionPercentage = xmlDoc.CreateAttribute("PromptLeftPositionPercentage");
                promptLeftPositionPercentage.Value = field.PromptLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptLeftPositionPercentage);

                XmlAttribute controlScriptName = xmlDoc.CreateAttribute("ControlScriptName");
                controlScriptName.Value = field.ControlFont.Name;
                fieldElement.Attributes.Append(controlScriptName);

                XmlAttribute shouldRepeatLast = xmlDoc.CreateAttribute("ShouldRepeatLast");
                shouldRepeatLast.Value = bool.Parse(field.ShouldRepeatLast.ToString()).ToString();
                fieldElement.Attributes.Append(shouldRepeatLast);

                XmlAttribute isRequired = xmlDoc.CreateAttribute("IsRequired");
                isRequired.Value = (bool.Parse(field.IsRequired.ToString()).ToString());
                fieldElement.Attributes.Append(isRequired);

                XmlAttribute isReadOnly = xmlDoc.CreateAttribute("IsReadOnly");
                isReadOnly.Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldElement.Attributes.Append(isReadOnly);

                XmlAttribute shouldRetainImageSize = xmlDoc.CreateAttribute("ShouldRetainImageSize");
                //    shouldRetainImageSize.Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();
                fieldElement.Attributes.Append(shouldRetainImageSize);

                XmlAttribute maxLength = xmlDoc.CreateAttribute("MaxLength");
                maxLength.Value = field.MaxLength.ToString();
                fieldElement.Attributes.Append(maxLength);

                XmlAttribute dataTableName = xmlDoc.CreateAttribute("DataTableName");
                dataTableName.Value = field.TableName;
                fieldElement.Attributes.Append(dataTableName);

                XmlAttribute sourceFieldId = xmlDoc.CreateAttribute("SourceFieldId");
                sourceFieldId.Value = field.SourceFieldId.ToString();
                fieldElement.Attributes.Append(sourceFieldId);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute pageId = xmlDoc.CreateAttribute("PageId");
                pageId.Value = field.Page.Id.ToString();

                fieldElement.Attributes.Append(pageId);

                XmlElement checkCodeBeforeElement = xmlDoc.CreateElement("CheckCodeBefore");
                XmlAttribute checkCodeBefore = xmlDoc.CreateAttribute("CheckCodeBefore");
                checkCodeBefore.Value = field.CheckCodeBefore.ToString();
                checkCodeBeforeElement.Attributes.Append(checkCodeBefore);
                fieldElement.AppendChild(checkCodeBeforeElement);

                XmlElement checkCodeAfterElement = xmlDoc.CreateElement("CheckCodeAfter");
                XmlAttribute checkCodeAfter = xmlDoc.CreateAttribute("CheckCodeAfter");
                checkCodeAfter.Value = field.CheckCodeAfter.ToString();
                checkCodeAfterElement.Attributes.Append(checkCodeAfter);
                fieldElement.AppendChild(checkCodeAfterElement);

                fieldsNode.AppendChild(fieldElement);
                view.Project.Save();
                return field.Id;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Create Time field.
        /// </summary>
        /// <param name="field">Time field to create.</param>
        /// <returns>Id of the newly created field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldPromptText = xmlDoc.CreateAttribute("PromptText");
                fieldPromptText.Value = field.PromptText;
                fieldElement.Attributes.Append(fieldPromptText);

                XmlAttribute fieldControlFontFamily = xmlDoc.CreateAttribute("ControlFontFamily");
                fieldControlFontFamily.Value = field.ControlFont.FontFamily.Name;
                fieldElement.Attributes.Append(fieldControlFontFamily);

                XmlAttribute controlFontStyle = xmlDoc.CreateAttribute("ControlFontStyle");
                controlFontStyle.Value = field.ControlFont.Style.ToString();
                fieldElement.Attributes.Append(controlFontStyle);

                XmlAttribute controlFontSize = xmlDoc.CreateAttribute("ControlFontSize");
                controlFontSize.Value = field.ControlFont.Size.ToString();
                fieldElement.Attributes.Append(controlFontSize);

                XmlAttribute controlTopPositionPercentage = xmlDoc.CreateAttribute("ControlTopPositionPercentage");
                controlTopPositionPercentage.Value = field.ControlTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlTopPositionPercentage);

                XmlAttribute controlLeftPositionPercentage = xmlDoc.CreateAttribute("ControlLeftPositionPercentage");
                controlLeftPositionPercentage.Value = field.ControlLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlLeftPositionPercentage);

                XmlAttribute controlHeightPercentage = xmlDoc.CreateAttribute("ControlHeightPercentage");
                controlHeightPercentage.Value = field.ControlHeightPercentage.ToString();
                fieldElement.Attributes.Append(controlHeightPercentage);

                XmlAttribute controlWidthPercentage = xmlDoc.CreateAttribute("ControlWidthPercentage");
                controlWidthPercentage.Value = field.ControlWidthPercentage.ToString();
                fieldElement.Attributes.Append(controlWidthPercentage);

                XmlAttribute tabIndex = xmlDoc.CreateAttribute("TabIndex");
                tabIndex.Value = field.TabIndex.ToString();
                fieldElement.Attributes.Append(tabIndex);

                XmlAttribute hasTabStop = xmlDoc.CreateAttribute("HasTabStop");
                hasTabStop.Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldElement.Attributes.Append(hasTabStop);

                XmlAttribute promptFontFamily = xmlDoc.CreateAttribute("PromptFontFamily");
                promptFontFamily.Value = field.PromptFont.FontFamily.Name;
                fieldElement.Attributes.Append(promptFontFamily);

                XmlAttribute promptFontStyle = xmlDoc.CreateAttribute("PromptFontStyle");
                promptFontStyle.Value = field.PromptFont.Style.ToString();
                fieldElement.Attributes.Append(promptFontStyle);

                XmlAttribute promptFontSize = xmlDoc.CreateAttribute("PromptFontSize");
                promptFontSize.Value = field.PromptFont.Size.ToString();
                fieldElement.Attributes.Append(promptFontSize);

                XmlAttribute promptScriptName = xmlDoc.CreateAttribute("PromptScriptName");
                promptScriptName.Value = field.PromptFont.Name;
                fieldElement.Attributes.Append(promptScriptName);

                XmlAttribute promptTopPositionPercentage = xmlDoc.CreateAttribute("PromptTopPositionPercentage");
                promptTopPositionPercentage.Value = field.PromptTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptTopPositionPercentage);

                XmlAttribute promptLeftPositionPercentage = xmlDoc.CreateAttribute("PromptLeftPositionPercentage");
                promptLeftPositionPercentage.Value = field.PromptLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptLeftPositionPercentage);

                XmlAttribute controlScriptName = xmlDoc.CreateAttribute("ControlScriptName");
                controlScriptName.Value = field.ControlFont.Name;
                fieldElement.Attributes.Append(controlScriptName);

                XmlAttribute shouldRepeatLast = xmlDoc.CreateAttribute("ShouldRepeatLast");
                shouldRepeatLast.Value = bool.Parse(field.ShouldRepeatLast.ToString()).ToString();
                fieldElement.Attributes.Append(shouldRepeatLast);

                XmlAttribute isRequired = xmlDoc.CreateAttribute("IsRequired");
                isRequired.Value = (bool.Parse(field.IsRequired.ToString()).ToString());
                fieldElement.Attributes.Append(isRequired);

                XmlAttribute isReadOnly = xmlDoc.CreateAttribute("IsReadOnly");
                isReadOnly.Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldElement.Attributes.Append(isReadOnly);

                XmlAttribute shouldRetainImageSize = xmlDoc.CreateAttribute("ShouldRetainImageSize");
                //shouldRetainImageSize.Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();
                fieldElement.Attributes.Append(shouldRetainImageSize);

                XmlAttribute dataTableName = xmlDoc.CreateAttribute("DataTableName");
                dataTableName.Value = field.TableName;
                fieldElement.Attributes.Append(dataTableName);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute pageId = xmlDoc.CreateAttribute("PageId");
                pageId.Value = field.Page.Id.ToString();
                fieldElement.Attributes.Append(pageId);

                XmlElement checkCodeBeforeElement = xmlDoc.CreateElement("CheckCodeBefore");
                XmlAttribute checkCodeBefore = xmlDoc.CreateAttribute("CheckCodeBefore");
                checkCodeBefore.Value = field.CheckCodeBefore.ToString();
                checkCodeBeforeElement.Attributes.Append(checkCodeBefore);
                fieldElement.AppendChild(checkCodeBeforeElement);

                XmlElement checkCodeAfterElement = xmlDoc.CreateElement("CheckCodeAfter");
                XmlAttribute checkCodeAfter = xmlDoc.CreateAttribute("CheckCodeAfter");
                checkCodeAfter.Value = field.CheckCodeAfter.ToString();
                checkCodeAfterElement.Attributes.Append(checkCodeAfter);
                fieldElement.AppendChild(checkCodeAfterElement);

                fieldsNode.AppendChild(fieldElement);
                view.Project.Save();
                return field.Id;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Create Uppercase text field.
        /// </summary>
        /// <param name="field">Uppercase text field to create.</param>
        /// <returns>Id of the newly created field.</returns>
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

                return field.Id;
            }
            finally
            {

            }
        }

        /// <summary>
        /// Create Yes/No field.
        /// </summary>
        /// <param name="field">Yes/No field to create.</param>
        /// <returns>Id of the newly created field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.ViewElement);
                View view = field.GetView();
                XmlElement fieldElement = xmlDoc.CreateElement("Field");

                XmlAttribute fieldIdAttribute = xmlDoc.CreateAttribute("FieldId");
                fieldIdAttribute.Value = view.GetFieldId(field.ViewElement).ToString();
                fieldElement.Attributes.Append(fieldIdAttribute);
                field.Id = Int32.Parse(fieldIdAttribute.Value);

                XmlAttribute fieldNameAttribute = xmlDoc.CreateAttribute("Name");
                fieldNameAttribute.Value = field.Name;
                fieldElement.Attributes.Append(fieldNameAttribute);

                XmlAttribute fieldPromptText = xmlDoc.CreateAttribute("PromptText");
                fieldPromptText.Value = field.PromptText;
                fieldElement.Attributes.Append(fieldPromptText);

                XmlAttribute fieldControlFontFamily = xmlDoc.CreateAttribute("ControlFontFamily");
                fieldControlFontFamily.Value = field.ControlFont.FontFamily.Name;
                fieldElement.Attributes.Append(fieldControlFontFamily);

                XmlAttribute controlFontStyle = xmlDoc.CreateAttribute("ControlFontStyle");
                controlFontStyle.Value = field.ControlFont.Style.ToString();
                fieldElement.Attributes.Append(controlFontStyle);

                XmlAttribute controlFontSize = xmlDoc.CreateAttribute("ControlFontSize");
                controlFontSize.Value = field.ControlFont.Size.ToString();
                fieldElement.Attributes.Append(controlFontSize);

                XmlAttribute controlTopPositionPercentage = xmlDoc.CreateAttribute("ControlTopPositionPercentage");
                controlTopPositionPercentage.Value = field.ControlTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlTopPositionPercentage);

                XmlAttribute controlLeftPositionPercentage = xmlDoc.CreateAttribute("ControlLeftPositionPercentage");
                controlLeftPositionPercentage.Value = field.ControlLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(controlLeftPositionPercentage);

                XmlAttribute controlHeightPercentage = xmlDoc.CreateAttribute("ControlHeightPercentage");
                controlHeightPercentage.Value = field.ControlHeightPercentage.ToString();
                fieldElement.Attributes.Append(controlHeightPercentage);

                XmlAttribute controlWidthPercentage = xmlDoc.CreateAttribute("ControlWidthPercentage");
                controlWidthPercentage.Value = field.ControlWidthPercentage.ToString();
                fieldElement.Attributes.Append(controlWidthPercentage);

                XmlAttribute tabIndex = xmlDoc.CreateAttribute("TabIndex");
                tabIndex.Value = field.TabIndex.ToString();
                fieldElement.Attributes.Append(tabIndex);

                XmlAttribute hasTabStop = xmlDoc.CreateAttribute("HasTabStop");
                hasTabStop.Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldElement.Attributes.Append(hasTabStop);

                XmlAttribute promptFontFamily = xmlDoc.CreateAttribute("PromptFontFamily");
                promptFontFamily.Value = field.PromptFont.FontFamily.Name;
                fieldElement.Attributes.Append(promptFontFamily);

                XmlAttribute promptFontStyle = xmlDoc.CreateAttribute("PromptFontStyle");
                promptFontStyle.Value = field.PromptFont.Style.ToString();
                fieldElement.Attributes.Append(promptFontStyle);

                XmlAttribute promptFontSize = xmlDoc.CreateAttribute("PromptFontSize");
                promptFontSize.Value = field.PromptFont.Size.ToString();
                fieldElement.Attributes.Append(promptFontSize);

                XmlAttribute promptScriptName = xmlDoc.CreateAttribute("PromptScriptName");
                promptScriptName.Value = field.PromptFont.Name;
                fieldElement.Attributes.Append(promptScriptName);

                XmlAttribute promptTopPositionPercentage = xmlDoc.CreateAttribute("PromptTopPositionPercentage");
                promptTopPositionPercentage.Value = field.PromptTopPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptTopPositionPercentage);

                XmlAttribute promptLeftPositionPercentage = xmlDoc.CreateAttribute("PromptLeftPositionPercentage");
                promptLeftPositionPercentage.Value = field.PromptLeftPositionPercentage.ToString();
                fieldElement.Attributes.Append(promptLeftPositionPercentage);

                XmlAttribute controlScriptName = xmlDoc.CreateAttribute("ControlScriptName");
                controlScriptName.Value = field.ControlFont.Name;
                fieldElement.Attributes.Append(controlScriptName);

                XmlAttribute shouldRepeatLast = xmlDoc.CreateAttribute("ShouldRepeatLast");
                shouldRepeatLast.Value = bool.Parse(field.ShouldRepeatLast.ToString()).ToString();
                fieldElement.Attributes.Append(shouldRepeatLast);

                XmlAttribute isRequired = xmlDoc.CreateAttribute("IsRequired");
                isRequired.Value = (bool.Parse(field.IsRequired.ToString()).ToString());
                fieldElement.Attributes.Append(isRequired);

                XmlAttribute isReadOnly = xmlDoc.CreateAttribute("IsReadOnly");
                isReadOnly.Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldElement.Attributes.Append(isReadOnly);

                XmlAttribute shouldRetainImageSize = xmlDoc.CreateAttribute("ShouldRetainImageSize");
                //shouldRetainImageSize.Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();
                fieldElement.Attributes.Append(shouldRetainImageSize);

                XmlAttribute dataTableName = xmlDoc.CreateAttribute("DataTableName");
                dataTableName.Value = field.TableName;
                fieldElement.Attributes.Append(dataTableName);

                XmlAttribute fieldTypeId = xmlDoc.CreateAttribute("FieldTypeId");
                fieldTypeId.Value = field.FieldType.ToString();
                fieldElement.Attributes.Append(fieldTypeId);

                XmlAttribute pageId = xmlDoc.CreateAttribute("PageId");
                pageId.Value = field.Page.Id.ToString();
                fieldElement.Attributes.Append(pageId);

                fieldsNode.AppendChild(fieldElement);
                view.Project.Save();
                return field.Id;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.GlobalRecordIdColumn"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.GlobalRecordIdColumn"/> to create.</param>
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

                Query insertQuery = db.CreateQuery("insert into metaGridColumns([Name], [FieldTypeId]) " +
                    "values (@Name, @FieldTypeId,)");

                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));

                db.ExecuteNonQuery(insertQuery);
                return GetMaxGridColumnId(column.Grid.Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create GlobalRecordIdColumn grid column in the database.", ex);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.UniqueRowIdColumn"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.UniqueRowIdColumn"/> to create.</param>
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

                Query insertQuery = db.CreateQuery("insert into metaGridColumns([Name], [FieldTypeId]) " +
                    "values (@Name, @FieldTypeId,)");

                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));

                db.ExecuteNonQuery(insertQuery);
                return GetMaxGridColumnId(column.Grid.Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create UniqueRowIdColumn grid column in the database.", ex);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.UniqueKeyColumn"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.UniqueKeyColumn"/> to create.</param>
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

                Query insertQuery = db.CreateQuery("insert into metaGridColumns([Name], [FieldTypeId]) " +
                    "values (@Name, @FieldTypeId,)");

                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));

                db.ExecuteNonQuery(insertQuery);
                return GetMaxGridColumnId(column.Grid.Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create UniqueKey grid column in the database.", ex);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.RecStatusColumn"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.RecStatusColumn"/> to create.</param>
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

                Query insertQuery = db.CreateQuery("insert into metaGridColumns([DataTableName], [ViewId], [FieldTypeId], [Name]) " +
                    "values (@Name, @FieldTypeId,)");

                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));


                db.ExecuteNonQuery(insertQuery);
                return GetMaxGridColumnId(column.Grid.Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create RecStatus grid column in the database.", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.ForeignKeyColumn"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.ForeignKeyColumn"/> to create.</param>
        /// <returns>Id of the newly created column.</returns>
        public int CreateGridColumn(ForeignKeyColumn column)
        {
            try
            {
                #region InputValidation
                if (column == null)
                {
                    throw new ArgumentNullException("ForeignKeyColumn");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaGridColumns([Name], [FieldTypeId]) " +
                    "values (@Name, @FieldTypeId,)");

                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));

                db.ExecuteNonQuery(insertQuery);
                return GetMaxGridColumnId(column.Grid.Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create ForeignKey grid column in the database.", ex);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.NumberColumn"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.NumberColumn"/> to create.</param>
        /// <returns>Id of the newly created column.</returns>
        public int CreateGridColumn(NumberColumn column)
        {
            try
            {
                #region InputValidation
                if (column == null)
                {
                    throw new ArgumentNullException("NumberColumn");
                }
                #endregion

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
                throw new GeneralException("Could not create grid Number column in the database.", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.DateColumn"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.DateColumn"/> to create.</param>
        /// <returns>Id of the newly created column.</returns>
        public int CreateGridColumn(ContiguousColumn column)
        {
            try
            {
                #region InputValidation
                if (column == null)
                {
                    throw new ArgumentNullException("DateColumn");
                }
                #endregion

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
                throw new GeneralException("Could not create grid Date column in the database.", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.PhoneNumberColumn"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.PhoneNumberColumn"/> to create.</param>
        /// <returns>Id of the newly created column.</returns>
        public int CreateGridColumn(PhoneNumberColumn column)
        {
            try
            {
                #region InputValidation
                if (column == null)
                {
                    throw new ArgumentNullException("PhoneNumberColumn");
                }
                #endregion

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
                throw new GeneralException("Could not create grid PhoneNumber column in the database.", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.TextColumn"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.TextColumn"/> to create.</param>
        /// <returns>Id of the newly created column.</returns>
        public int CreateGridColumn(TextColumn column)
        {
            try
            {
                #region InputValidation
                if (column == null)
                {
                    throw new ArgumentNullException("TextColumn");
                }
                #endregion

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
                throw new GeneralException("Could not create grid Text column in the database.", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.CheckboxColumn"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.CheckboxColumn"/> to create.</param>
        /// <returns>Id of the newly created column.</returns>
        public int CreateGridColumn(CheckboxColumn column)
        {
            try
            {
                #region InputValidation
                if (column == null)
                {
                    throw new ArgumentNullException("CheckboxColumn");
                }
                #endregion

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
                return GetMaxGridColumnId(column.Grid.Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create grid Text column in the database.", ex);
            }
            finally
            {

            }
        }


        /// <summary>
        /// Creates a new <see cref="Epi.Fields.CheckboxColumn"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.CheckboxColumn"/> to create.</param>
        /// <returns>Id of the newly created column.</returns>
        public int CreateGridColumn(YesNoColumn column)
        {
            try
            {
                #region InputValidation
                if (column == null)
                {
                    throw new ArgumentNullException("YesNoColumn");
                }
                #endregion

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
                return GetMaxGridColumnId(column.Grid.Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create grid Text column in the database.", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.DDLColumnOfCommentLegal"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.DDLColumnOfCommentLegal"/> to create.</param>
        /// <returns>Id of the newly created column.</returns>
        public int CreateGridColumn(DDLColumnOfCommentLegal column)
        {
            try
            {
                #region InputValidation
                if (column == null)
                {
                    throw new ArgumentNullException("DDLColumnOfCommentLegal");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaGridColumns([Name], [TextColumnName], [SourceTableName], [Width], [Position], [FieldTypeId], [Text], [ShouldRepeatLast], [IsExclusiveTable], [IsReadOnly], [IsRequired], [FieldId], [Sort]) " +
                    "values (@Name, @TextColumnName, @SourceTableName, @Width, @Position, @FieldTypeId, @Text, @ShouldRepeatLast, @IsExclusiveTable, @IsReadOnly, @IsRequired, @FieldId, @Sort)");

                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                insertQuery.Parameters.Add(new QueryParameter("@TextColumnName", DbType.String, column.TextColumnName));
                insertQuery.Parameters.Add(new QueryParameter("@SourceTableName", DbType.String, column.SourceTableName));
                insertQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, column.Width));
                insertQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, column.Position));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                insertQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, column.Text));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, column.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@IsExclusiveTable", DbType.Boolean, column.IsExclusiveTable));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, column.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, column.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, column.Grid.Id));
                insertQuery.Parameters.Add(new QueryParameter("@Sort", DbType.Boolean, column.ShouldSort));

                db.ExecuteNonQuery(insertQuery);
                return GetMaxGridColumnId(column.Grid.Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create grid CommentLegal column in the database.", ex);
            }

        }

        /// <summary>
        /// Creates a new <see cref="Epi.Fields.DDLColumnOfLegalValues"/> in <see cref="Epi.Fields.GridField"/>.
        /// </summary>
        /// <param name="column"><see cref="Epi.Fields.DDLColumnOfLegalValues"/> to create.</param>
        /// <returns>Id of the newly created column.</returns>
        public int CreateGridColumn(DDLColumnOfLegalValues column)
        {
            try
            {
                #region InputValidation
                if (column == null)
                {
                    throw new ArgumentNullException("DDLColumnOfLegalValues");
                }
                #endregion

                Query insertQuery = db.CreateQuery("insert into metaGridColumns([Name], [TextColumnName], [SourceTableName], [Width], [Position], [FieldTypeId], [Text], [ShouldRepeatLast], [IsExclusiveTable], [IsReadOnly], [IsRequired], [FieldId], [Sort]) " +
                    "values (@Name, @TextColumnName, @SourceTableName, @Width, @Position, @FieldTypeId, @Text, @ShouldRepeatLast, @IsExclusiveTable, @IsReadOnly, @IsRequired, @FieldId, @Sort)");

                insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));
                insertQuery.Parameters.Add(new QueryParameter("@TextColumnName", DbType.String, column.TextColumnName));
                insertQuery.Parameters.Add(new QueryParameter("@SourceTableName", DbType.String, column.SourceTableName));
                insertQuery.Parameters.Add(new QueryParameter("@Width", DbType.Int32, column.Width));
                insertQuery.Parameters.Add(new QueryParameter("@Position", DbType.Int32, column.Position));
                insertQuery.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, (int)column.GridColumnType));
                insertQuery.Parameters.Add(new QueryParameter("@Text", DbType.String, column.Text));
                insertQuery.Parameters.Add(new QueryParameter("@ShouldRepeatLast", DbType.Boolean, column.ShouldRepeatLast));
                insertQuery.Parameters.Add(new QueryParameter("@IsExclusiveTable", DbType.Boolean, column.IsExclusiveTable));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, column.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@IsReadOnly", DbType.Boolean, column.IsReadOnly));
                insertQuery.Parameters.Add(new QueryParameter("@IsRequired", DbType.Boolean, column.IsRequired));
                insertQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, column.Grid.Id));
                insertQuery.Parameters.Add(new QueryParameter("@Sort", DbType.Boolean, column.ShouldSort));

                db.ExecuteNonQuery(insertQuery);
                return GetMaxGridColumnId(column.Grid.Id);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create grid LegalValues column in the database.", ex);
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

                //				DbQuery insertQuery = db.CreateQuery("insert into metaControls([FieldId], [TopPosition], [LeftPosition], [Height], [Width], [IsRepeatLast], [IsRequired], [IsReadOnly], [IsSoundex], [IsRetainImgSize], [TabOrder], [IsTabStop]) values (@FieldId, @TopPosition, @LeftPosition, @Height, @Width, @IsRepeatLast, @IsRequired, @IsReadOnly, @IsSoundex, @IsRetainImgSize, @TabOrder, @IsTabStop)");
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

                //				DbQuery selectQuery = db.CreateQuery("select MAX(ControlId) from metaControls where FieldId = @FieldId");
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
            finally
            {

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
                string queryString = "update metaViews set [CheckCodeVariableDefinitions] = @CheckCodeVariableDefinitions, [CheckCode] = @CheckCode, [CheckCodeBefore] = @CheckCodeBefore, [CheckCodeAfter] = @CheckCodeAfter, [RecordCheckCodeBefore] = @RecordCheckCodeBefore, [RecordCheckCodeAfter] = @RecordCheckCodeAfter, [IsRelatedView] = @IsRelatedView  where [ViewId] = @ViewId";
                Query query = db.CreateQuery(queryString);
                query.Parameters.Add(new QueryParameter("@CheckCodeVariableDefinitions", DbType.String, view.CheckCodeVariableDefinitions));
                query.Parameters.Add(new QueryParameter("@CheckCode", DbType.String, view.CheckCode));
                query.Parameters.Add(new QueryParameter("@CheckCodeBefore", DbType.String, view.CheckCodeBefore));
                query.Parameters.Add(new QueryParameter("@CheckCodeAfter", DbType.String, view.WebSurveyId));
                query.Parameters.Add(new QueryParameter("@RecordCheckCodeBefore", DbType.String, view.RecordCheckCodeBefore));
                query.Parameters.Add(new QueryParameter("@RecordCheckCodeAfter", DbType.String, view.RecordCheckCodeAfter));
                query.Parameters.Add(new QueryParameter("@IsRelatedView", DbType.Boolean, view.IsRelatedView));
                query.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, view.Id));

                db.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update view in the database", ex);
            }
        }

        /// <summary>
        /// Updates the fields tab index 
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
        /// Updates a page in the database
        /// </summary>
        /// <param name="page">The page to be updated</param>
        public void UpdatePage(Page page)
        {
            try
            {
                XmlDocument doc = this.Project.GetXmlDocument();
                XmlNode viewsNode = GetViewsNode();
                XmlNode pagesNode = viewsNode.SelectSingleNode("//View/Pages");
                XmlNode pageNode = pagesNode.SelectSingleNode("//Page[@PageId= '" + page.Id + "']");

                pageNode.Attributes["Name"].Value = page.Name.ToString();
                pageNode.Attributes["Position"].Value = page.Position.ToString();
                pageNode.FirstChild.Attributes["CheckCodeBefore"].Value = page.CheckCodeBefore.ToString();
                pageNode.LastChild.Attributes["CheckCodeAfter"].Value = page.CheckCodeAfter.ToString();

                this.Project.Save();
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update page in the database", ex);
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

                char[] noiseChars = { '\r', '\n', '\t', ' ', '"', '\'' };

                XmlNode viewsNode = GetViewsNode();
                XmlNode viewNode = viewsNode.SelectSingleNode("//View[@ViewId = '" + view.Id + "']");
                XmlNode fieldNode = viewNode.SelectSingleNode("//Fields/Field[@FieldId = '" + fieldId + "']");
                fieldNode.LastChild.Attributes["CheckCodeBefore"].Value = checkCode.Trim(noiseChars);
                view.Project.Save();
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create control checkcode in the Xml project file", ex);
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

                char[] noiseChars = { '\r', '\n', '\t', ' ', '"', '\'' };

                XmlNode viewsNode = GetViewsNode();
                XmlNode viewNode = viewsNode.SelectSingleNode("//View[@ViewId = '" + view.Id + "']");
                XmlNode fieldNode = viewNode.SelectSingleNode("//Fields/Field[@FieldId = '" + fieldId + "']");
                fieldNode.LastChild.Attributes["CheckCodeAfter"].Value = checkCode.Trim(noiseChars);
                view.Project.Save();
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create control checkcode in the Xml project file", ex);
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
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("insert into ");
                sb.Append(tableName);
                sb.Append(" (");
                for (int x = 0; x < columnNames.Length; x++)
                {
                    sb.Append(columnNames[x]);
                    if (x < columnNames.Length - 1)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append(") values ");
                sb.Append(" (");
                for (int x = 0; x < columnNames.Length; x++)
                {
                    sb.Append("@");
                    sb.Append(columnNames[x]);
                    if (x < columnNames.Length - 1)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append(")");
                Query insertQuery = db.CreateQuery(sb.ToString());
                for (int x = 0; x < columnNames.Length; x++)
                {
                    insertQuery.Parameters.Add(new QueryParameter("@" + columnNames[x], DbType.String, columnData[x]));
                }

                db.ExecuteNonQuery(insertQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create record in code table", ex);
            }
            finally
            {

            }
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

                Query updateQuery = db.CreateQuery("update " + tableName + " set " + columnName + " = @NewValue where " + columnName + " = @OldValue");
                updateQuery.Parameters.Add(new QueryParameter("@NewValue", DbType.String, columnName, columnName));
                updateQuery.Parameters.Add(new QueryParameter("@OldValue", DbType.String, columnName, columnName));
                updateQuery.Parameters[1].SourceVersion = DataRowVersion.Original;

                Query insertQuery = db.CreateQuery("insert into " + tableName + "(" + columnName + ") values (@Value)");
                insertQuery.Parameters.Add(new QueryParameter("@Value", DbType.String, columnName, columnName));

                //				DbQuery deleteQuery = db.CreateQuery("delete from " + tableName + " where " + columnName + " = @Value");
                //				deleteQuery.Parameters.Add(new DbParameter("@Value", DbType.String, columnName,columnName));


                db.Update(dataTable, tableName, insertQuery, updateQuery);

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not save data for code table", ex);
            }
            finally
            {

            }
        }

        public bool TableExists(string tableName)
        {
            return db.TableExists(tableName);
        }

        /// <summary>
        /// Constructor initialization.
        /// </summary>
        /// <param name="MetaDbInfo">Database driver information.</param>
        /// <param name="driver">Database driver name.</param>
        /// <param name="createDatabase">Create database flag.</param>
        public void Initialize(DbDriverInfo MetaDbInfo, string driver, bool createDatabase)
        {
            throw new NotImplementedException("Not implemented");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> DuplicateFieldNames()
        {
            return new Dictionary<string, int>();
        }

        /// <summary>
        /// Insert code table data
        /// </summary>
        /// <param name="dataTable">DataTable</param>
        /// <param name="tableName">Table Name</param>
        /// <param name="columnNames">String array of columnNames</param>
        public void InsertCodeTableData(DataTable dataTable, string tableName, string[] columnNames)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("insert into [");
                sb.Append(tableName);
                sb.Append("] (");
                for (int x = 0; x < columnNames.Length; x++)
                {
                    sb.Append(columnNames[x]);
                    if (x < columnNames.Length - 1)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append(") values ");
                sb.Append(" (");
                for (int x = 0; x < columnNames.Length; x++)
                {
                    sb.Append("@");
                    sb.Append(columnNames[x]);
                    if (x < columnNames.Length - 1)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append(")");
                Query insertQuery = db.CreateQuery(sb.ToString());
                foreach (DataRow row in dataTable.Rows)
                {
                    insertQuery.Parameters.Clear();
                    for (int x = 0; x < columnNames.Length; x++)
                    {
                        insertQuery.Parameters.Add(new QueryParameter("@" + columnNames[x], DbType.String, row[columnNames[x]]));
                    }

                    db.ExecuteNonQuery(insertQuery);
                }
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not insert code into code table", ex);
            }
            finally
            {

            }
        }

       /// <summary>
        /// Creates a field in metaFields
        /// </summary>
        public void InsertFields(DataTable fields)
        {
            throw new System.ApplicationException("Not implemented");
        }
        
        /// <summary>
        /// Save Code Table Data
        /// </summary>
        /// <param name="dataTable">DataTable</param>
        /// <param name="tableName">Table Name</param>
        /// <param name="columnNames">String array of column names</param>
        public void SaveCodeTableData(DataTable dataTable, string tableName, string[] columnNames)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("insert into ");
                sb.Append(tableName);
                sb.Append(" (");
                for (int x = 0; x < columnNames.Length; x++)
                {
                    sb.Append(columnNames[x]);
                    if (x < columnNames.Length - 1)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append(") values ");
                sb.Append(" (");
                for (int x = 0; x < columnNames.Length; x++)
                {
                    sb.Append("@");
                    sb.Append(columnNames[x]);
                    if (x < columnNames.Length - 1)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append(")");
                Query insertQuery = db.CreateQuery(sb.ToString());
                for (int x = 0; x < columnNames.Length; x++)
                {
                    insertQuery.Parameters.Add(new QueryParameter("@" + columnNames[x], DbType.String, columnNames[x], columnNames[x]));
                }

                sb.Remove(0, sb.Length);

                sb.Append("update [").Append(tableName);
                sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET).Append(StringLiterals.SPACE);
                sb.Append("set").Append(StringLiterals.SPACE);
                for (int x = 0; x < columnNames.Length; x++)
                {
                    sb.Append(StringLiterals.LEFT_SQUARE_BRACKET);
                    sb.Append(columnNames[x]);
                    sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET);
                    sb.Append(StringLiterals.EQUAL);
                    sb.Append("@NewValue").Append(StringLiterals.SPACE);
                    sb.Append(", ");
                }
                sb.Append("where ");
                for (int x = 0; x < columnNames.Length; x++)
                {
                    sb.Append(columnNames[x]).Append(StringLiterals.SPACE);
                    sb.Append(StringLiterals.EQUAL);
                    sb.Append("@OldValue");
                    if (columnNames.Length > 1)
                    {
                        sb.Append(" and");
                    }
                }
                Query updateQuery = db.CreateQuery(sb.ToString());
                for (int x = 0; x < columnNames.Length; x++)
                {
                    updateQuery.Parameters.Add(new QueryParameter("@NewValue", DbType.String, columnNames[x], columnNames[x]));
                    updateQuery.Parameters.Add(new QueryParameter("@OldValue", DbType.String, columnNames[x], columnNames[x]));
                    updateQuery.Parameters[1].SourceVersion = DataRowVersion.Original;
                }


                db.Update(dataTable, tableName, insertQuery, updateQuery);

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not save data for code table", ex);
            }
            finally
            {

            }
        }
        /// <summary>
        /// Create a code table in the metadata database
        /// </summary>
        /// <param name="tableName">Name of the code table</param>
        /// <param name="columnNames">Column names of the code table</param>
        /// <returns>A value indicating whether or not the code table was created</returns>
        public bool CreateCodeTable(string tableName, string[] columnNames)
        {
            try
            {
                if (!db.TableExists(tableName))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("create table [");
                    sb.Append(tableName);
                    sb.Append("] (");
                    for (int x = 0; x < columnNames.Length; x++)
                    {
                        sb.Append(columnNames[x]);
                        sb.Append(" varchar(255)");
                        if (x < columnNames.Length - 1)
                        {
                            sb.Append(", ");
                        }
                    }
                    sb.Append(")");
                    Query createQuery = db.CreateQuery(sb.ToString());

                    db.ExecuteNonQuery(createQuery);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            //			catch (Exception ex)
            //			{
            //				throw new GeneralException("Could not create code table in the database", ex);
            //			}
            finally
            {

            }
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
                if (!db.TableExists(tableName))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("create table ");
                    sb.Append(tableName);
                    sb.Append(" (");
                    sb.Append(columnName);
                    sb.Append(" varchar(255)");
                    sb.Append(")");
                    Query createQuery = db.CreateQuery(sb.ToString());

                    db.ExecuteNonQuery(createQuery);
                }
            }
            finally
            {

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
                insertQuery.Parameters.Add(new QueryParameter("@DateCreated", DbType.String, System.DateTime.Now.ToShortDateString()));
                insertQuery.Parameters.Add(new QueryParameter("@DateModified", DbType.String, System.DateTime.Now.ToShortDateString()));
                insertQuery.Parameters.Add(new QueryParameter("@Author", DbType.String, author));

                db.ExecuteNonQuery(insertQuery);

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not insert programs", ex);
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

                Query query = db.CreateQuery("delete from metaViews where [metaViews.Name] = '" + viewName + "'");

                db.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException(SharedStrings.WARNING_DELETE_VIEW_ERROR, ex);
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
        /// Deletes a page from the Xml metadata project file
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

                XmlDocument doc = GetXmlDocument();
                XmlNode pagesNode = GetPagesNode(page.GetView().ViewElement);
                XmlNode pageNode = pagesNode.SelectSingleNode("//Page[@PageId= '" + page.Id + "']");
                pageNode.ParentNode.RemoveChild(pageNode);

                Save();
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not delete page in the database", ex);
            }
            finally
            {

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
        /// Deletes a field from the xml metadata project file
        /// </summary>
        /// <param name="field">A field object</param>
        public void DeleteField(Field field)
        {
            try
            {
                XmlDocument doc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(field.GetView().ViewElement);
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + field.Id + "']");
                fieldNode.ParentNode.RemoveChild(fieldNode);

                Save();
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not delete field in the Xml metadata file", ex);
            }
            finally
            {

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
                Query query = db.CreateQuery("delete from metaGridColumns where [FieldId] = @fieldId and [Name] = @Name");
                query.Parameters.Add(new QueryParameter("@fieldId", DbType.Int32, column.Id));
                query.Parameters.Add(new QueryParameter("@Name", DbType.String, column.Name));

                db.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not delete grid column in the database.", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Deletes fields on a page from the Xml metadata project file
        /// </summary>
        /// <param name="page"></param>
        public void DeleteFields(Page page)
        {
            try
            {
                XmlDocument doc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(page.GetView().ViewElement);
                XmlNodeList nodeList = fieldsNode.SelectNodes("Field[@PageId = '" + page.Id + "']");
                foreach (XmlNode node in nodeList)
                {
                    node.ParentNode.RemoveChild(node);
                }
                Save();
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not delete fields in the database", ex);
            }
            finally
            {

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
        /// <param name="field">The field to be updated</param>
        public void UpdatePromptPosition(FieldWithSeparatePrompt field)
        {
            try
            {
                XmlDocument doc = this.Project.GetXmlDocument();
                XmlNode viewsNode = GetViewsNode();
                XmlNode fieldsNode = viewsNode.SelectSingleNode("//View/Fields");
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + field.Id + "']");

                fieldNode.Attributes["PromptLeftPositionPercentage"].Value = field.PromptLeftPositionPercentage.ToString();
                fieldNode.Attributes["PromptTopPositionPercentage"].Value = field.PromptTopPositionPercentage.ToString();

                this.Project.Save();

                //Query query = db.CreateQuery("update metaFields set [PromptLeftPositionPercentage] = @LeftPosition, [PromptTopPositionPercentage] = @TopPosition where [FieldId] = @FieldId");
                //query.Parameters.Add(new QueryParameter("@LeftPosition", DbType.Double, field.PromptLeftPositionPercentage));
                //query.Parameters.Add(new QueryParameter("@TopPosition", DbType.Double, field.PromptTopPositionPercentage));
                //query.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, field.Id));

                //db.ExecuteNonQuery(query);
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
        public void UpdateControlPosition(RenderableField field) //int fieldId, double xCoordinate, double yCoordinate)
        {
            try
            {
                XmlDocument doc = this.Project.GetXmlDocument();
                XmlNode viewsNode = GetViewsNode();
                XmlNode fieldsNode = viewsNode.SelectSingleNode("//View/Fields");
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + field.Id + "']");

                fieldNode.Attributes["ControlLeftPositionPercentage"].Value = field.ControlLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlTopPositionPercentage"].Value = field.ControlTopPositionPercentage.ToString();

                this.Project.Save();
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update control position", ex);
            }
            finally
            {

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
                XmlDocument doc = this.Project.GetXmlDocument();
                XmlNode viewsNode = GetViewsNode();
                XmlNode fieldsNode = viewsNode.SelectSingleNode("//View/Fields");
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + field.Id + "']");

                fieldNode.Attributes["ControlHeightPercentage"].Value = field.ControlHeightPercentage.ToString();
                fieldNode.Attributes["ControlWidthPercentage"].Value = field.ControlWidthPercentage.ToString();

                this.Project.Save();
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

                XmlDocument doc = GetXmlDocument();
                XmlNode pagesNode = GetPagesNode(view.ViewElement);
                XmlNodeList nodeList = pagesNode.SelectNodes("Page[@Position>= '" + page.Position + "' and @PageId!= '" + page.Id + "']");
                foreach (XmlNode node in nodeList)
                {
                    int pagePosition = int.Parse(node.Attributes["Position"].Value) + 1;
                    node.Attributes["Position"].Value = pagePosition.ToString();
                }
                Save();
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update page position to insert a page in the database", ex);
            }
            finally
            {

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

                XmlDocument doc = GetXmlDocument();
                XmlNode pagesNode = GetPagesNode(view.ViewElement);
                XmlNodeList nodeList = pagesNode.SelectNodes("Page[@Position>= '" + position + "']");
                foreach (XmlNode node in nodeList)
                {
                    int pagePosition = int.Parse(node.Attributes["Position"].Value) - 1;
                    node.Attributes["Position"].Value = pagePosition.ToString();
                }
                Save();

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update page position to delete a page in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Creates a relationship between a field and a child view
        /// </summary>
        /// <param name="fieldId">Id of the field</param>
        /// <param name="relatedViewId">Id of the view the field is related to</param>
        /// <param name="relateCondition">Conditions to show the related view</param>
        /// <param name="shouldReturnToParent">Whether or not the related view returns to parent</param>
        public void RelateFieldToView(Guid fieldId, int relatedViewId, string relateCondition, bool shouldReturnToParent)
        {
            try
            {
                Query fieldQuery = db.CreateQuery("update metaFields set [RelatedViewId] = @RelatedViewId, [RelateCondition] = @RelateCondition, [ShouldReturnToparent] = @ShouldReturnToParent where FieldId = @FieldId");
                fieldQuery.Parameters.Add(new QueryParameter("@RelatedViewId", DbType.Int32, relatedViewId));
                fieldQuery.Parameters.Add(new QueryParameter("@RelateCondition", DbType.String, relateCondition));
                fieldQuery.Parameters.Add(new QueryParameter("@ShouldReturnToParent", DbType.Boolean, shouldReturnToParent));
                fieldQuery.Parameters.Add(new QueryParameter("@FieldId", DbType.Int32, fieldId));

                Query viewQuery = db.CreateQuery("update metaViews set [IsRelatedView] = @IsRelatedView where [ViewId] = @ViewId");
                viewQuery.Parameters.Add(new QueryParameter("@IsRelatedView", DbType.Boolean, true));
                viewQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, relatedViewId));


                db.ExecuteNonQuery(fieldQuery);
                db.ExecuteNonQuery(viewQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create new relate info", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Update Check Box field.
        /// </summary>
        /// <param name="field">Check Box field to update.</param>
        /// <returns>Id of the updated field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(GetFieldViewElement(field));
                View view = field.GetView();
                string fieldId = field.Id.ToString();
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + fieldId + "']");

                fieldNode.Attributes["Name"].Value = field.Name.ToString();
                fieldNode.Attributes["PromptText"].Value = field.PromptText.ToString();
                fieldNode.Attributes["ControlFontFamily"].Value = field.ControlFont.ToString();
                fieldNode.Attributes["ControlFontStyle"].Value = field.ControlFont.Style.ToString();
                fieldNode.Attributes["ControlFontSize"].Value = field.ControlFont.Size.ToString();
                fieldNode.Attributes["ControlTopPositionPercentage"].Value = field.ControlTopPositionPercentage.ToString();
                fieldNode.Attributes["ControlLeftPositionPercentage"].Value = field.ControlLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlHeightPercentage"].Value = field.ControlHeightPercentage.ToString();
                fieldNode.Attributes["ControlWidthPercentage"].Value = field.ControlWidthPercentage.ToString();
                fieldNode.Attributes["TabIndex"].Value = field.TabIndex.ToString();
                fieldNode.Attributes["HasTabStop"].Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldNode.Attributes["PromptFontFamily"].Value = field.PromptFont.FontFamily.Name.ToString();
                fieldNode.Attributes["PromptFontStyle"].Value = field.PromptFont.Style.ToString();
                fieldNode.Attributes["PromptFontSize"].Value = field.PromptFont.Size.ToString();
                fieldNode.Attributes["PromptScriptName"].Value = field.PromptFont.Name.ToString();
                fieldNode.Attributes["ControlScriptName"].Value = field.ControlFont.Name.ToString();
                //fieldNode.Attributes["ShouldRetainImageSize"].Value = (bool.Parse(field.IsControlResizable)).ToString();
                fieldNode.Attributes["ShouldRepeatLast"].Value = (bool.Parse(field.ShouldRepeatLast.ToString())).ToString();
                fieldNode.Attributes["IsRequired"].Value = (bool.Parse(field.IsRequired.ToString())).ToString();
                fieldNode.Attributes["IsReadOnly"].Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldNode.Attributes["DataTableName"].Value = field.TableName.ToString();

                //fieldNode.FirstChild.Attributes["CheckCodeBefore"].Value = field.CheckCodeBefore.ToString();
                fieldNode.LastChild.Attributes["CheckCodeAfter"].Value = field.CheckCodeAfter.ToString();

                view.Project.Save();
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update CheckBoxField in the database", ex);
            }
            finally
            {

            }
        }

        /* private void UpdateField(CodeField field)
        {
            try
            {
        #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("CodeField");
                }
        #endregion

                DbQuery updateQuery = db.CreateQuery("update metaFields set [ControlFontFamily] = @ControlFontFamily, [ControlFontStyle] = @ControlFontStyle, " +
                    "[ControlFontSize] = @ControlFontSize, [ControlHeight] = @ControlHeight, [ControlLeftPosition] = @ControlLeftPosition, " +
                    "[ControlTopPosition] = @ControlTopPosition, [ControlWidth] = @ControlWidth, [HasTabStop] = @HasTabStop, [Name] = @Name, " +
                    "[PageId] = @PageId, [PromptFontFamily] = @PromptFontFamily, [PromptFontStyle] = @PromptFontStyle, [PromptFontSize] = @PromptFontSize, " +
                    "[PromptLeftPosition] = @PromptLeftPosition, [PromptText] = @PromptText, [PromptTopPosition] = @PromptTopPosition, " +
                    "[TabIndex] = @TabIndex where [FieldId] = @FieldId");
				
                updateQuery.Parameters.Add(new DbParameter("@ControlFontFamily", DbType.String, field.ControlFont.Name));
                updateQuery.Parameters.Add(new DbParameter("@ControlFontStyle", DbType.String, field.ControlFont.Style.ToString()));
                updateQuery.Parameters.Add(new DbParameter("@ControlFontSize", DbType.Double, field.ControlFont.Size));	
                updateQuery.Parameters.Add(new DbParameter("@ControlHeight", DbType.Double, field.ControlHeightPercentage));
                updateQuery.Parameters.Add(new DbParameter("@ControlLeftPosition", DbType.Double, field.ControlLeftPositionPercentage));
                updateQuery.Parameters.Add(new DbParameter("@ControlTopPosition", DbType.Double, field.ControlTopPositionPercentage));
                updateQuery.Parameters.Add(new DbParameter("@ControlWidth", DbType.Double, field.ControlWidthPercentage));
                updateQuery.Parameters.Add(new DbParameter("@HasTabStop", DbType.Boolean, field.HasTabStop));
                updateQuery.Parameters.Add(new DbParameter("@Name", DbType.String, field.Name));
                updateQuery.Parameters.Add(new DbParameter("@PageId", DbType.Int32, field.Page.Id));
                updateQuery.Parameters.Add(new DbParameter("@PromptFontFamily", DbType.String, field.PromptFont.Name));
                updateQuery.Parameters.Add(new DbParameter("@PromptFontStyle", DbType.String, field.PromptFont.Style.ToString()));
                updateQuery.Parameters.Add(new DbParameter("@PromptFontSize", DbType.Double, field.PromptFont.Size));
                updateQuery.Parameters.Add(new DbParameter("@PromptLeftPosition", DbType.Double, field.PromptLeftPositionPercentage));
                updateQuery.Parameters.Add(new DbParameter("@PromptText", DbType.String, field.PromptText));
                updateQuery.Parameters.Add(new DbParameter("@PromptTopPosition", DbType.Double, field.PromptTopPositionPercentage));
                updateQuery.Parameters.Add(new DbParameter("@TabIndex", DbType.Int32, field.TabIndex));
                updateQuery.Parameters.Add(new DbParameter("@FieldId", DbType.Int32, field.Id));

				
                db.ExecuteNonQuery(updateQuery);
				
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update CodeField in the database", ex);
            }
            finally
            {
				
            }
        }
        */

        /// <summary>
        /// Update Command Button field.
        /// </summary>
        /// <param name="field">Command Button field to update.</param>
        /// <returns>Id of the updated field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(GetFieldViewElement(field));
                View view = field.GetView();
                string fieldId = field.Id.ToString();
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + fieldId + "']");

                fieldNode.Attributes["Name"].Value = field.Name.ToString();
                fieldNode.Attributes["PromptText"].Value = field.PromptText.ToString();
                fieldNode.Attributes["ControlFontFamily"].Value = field.ControlFont.ToString();
                fieldNode.Attributes["ControlFontStyle"].Value = field.ControlFont.Style.ToString();
                fieldNode.Attributes["ControlFontSize"].Value = field.ControlFont.Size.ToString();
                fieldNode.Attributes["ControlTopPositionPercentage"].Value = field.ControlTopPositionPercentage.ToString();
                fieldNode.Attributes["ControlLeftPositionPercentage"].Value = field.ControlLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlHeightPercentage"].Value = field.ControlHeightPercentage.ToString();
                fieldNode.Attributes["ControlWidthPercentage"].Value = field.ControlWidthPercentage.ToString();
                fieldNode.Attributes["TabIndex"].Value = field.TabIndex.ToString();
                fieldNode.Attributes["HasTabStop"].Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldNode.Attributes["PromptFontFamily"].Value = field.PromptFont.FontFamily.Name.ToString();
                fieldNode.Attributes["PromptFontStyle"].Value = field.PromptFont.Style.ToString();
                fieldNode.Attributes["PromptFontSize"].Value = field.PromptFont.Size.ToString();
                fieldNode.Attributes["PromptScriptName"].Value = field.PromptFont.Name.ToString();
                fieldNode.Attributes["ControlScriptName"].Value = field.ControlFont.Name.ToString();
                //fieldNode.Attributes["ShouldRetainImageSize"].Value = (bool.Parse(field.IsControlResizable)).ToString();

                view.Project.Save();
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
        /// <returns>Id of the updated field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(GetFieldViewElement(field));
                View view = field.GetView();
                string fieldId = field.Id.ToString();
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + fieldId + "']");

                fieldNode.Attributes["Name"].Value = field.Name.ToString();
                fieldNode.Attributes["PromptText"].Value = field.PromptText.ToString();
                fieldNode.Attributes["ControlFontFamily"].Value = field.ControlFont.ToString();
                fieldNode.Attributes["ControlFontStyle"].Value = field.ControlFont.Style.ToString();
                fieldNode.Attributes["ControlFontSize"].Value = field.ControlFont.Size.ToString();
                fieldNode.Attributes["ControlTopPositionPercentage"].Value = field.ControlTopPositionPercentage.ToString();
                fieldNode.Attributes["ControlLeftPositionPercentage"].Value = field.ControlLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlHeightPercentage"].Value = field.ControlHeightPercentage.ToString();
                fieldNode.Attributes["ControlWidthPercentage"].Value = field.ControlWidthPercentage.ToString();
                fieldNode.Attributes["TabIndex"].Value = field.TabIndex.ToString();
                fieldNode.Attributes["HasTabStop"].Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldNode.Attributes["PromptFontFamily"].Value = field.PromptFont.FontFamily.Name.ToString();
                fieldNode.Attributes["PromptFontStyle"].Value = field.PromptFont.Style.ToString();
                fieldNode.Attributes["PromptFontSize"].Value = field.PromptFont.Size.ToString();
                fieldNode.Attributes["PromptScriptName"].Value = field.PromptFont.Name.ToString();
                fieldNode.Attributes["PromptTopPositionPercentage"].Value = field.PromptTopPositionPercentage.ToString();
                fieldNode.Attributes["PromptLeftPositionPercentage"].Value = field.PromptLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlScriptName"].Value = field.ControlFont.Name.ToString();
                fieldNode.Attributes["ShouldRepeatLast"].Value = (bool.Parse(field.ShouldRepeatLast.ToString())).ToString();
                fieldNode.Attributes["IsRequired"].Value = (bool.Parse(field.IsRequired.ToString())).ToString();
                fieldNode.Attributes["IsReadOnly"].Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                //fieldNode.Attributes["ShouldRetainImageSize"].Value = (bool.Parse(field.IsControlResizable));
                fieldNode.Attributes["Lower"].Value = field.Lower.ToString();
                fieldNode.Attributes["Upper"].Value = field.Upper.ToString();
                fieldNode.Attributes["DataTableName"].Value = field.TableName.ToString();
                fieldNode.FirstChild.Attributes["CheckCodeBefore"].Value = field.CheckCodeBefore.ToString();
                fieldNode.LastChild.Attributes["CheckCodeAfter"].Value = field.CheckCodeAfter.ToString();

                view.Project.Save();
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
        /// Update Date Time field.
        /// </summary>
        /// <param name="field">Date Time field to update.</param>
        /// <returns>Id of the updated field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(GetFieldViewElement(field));
                View view = field.GetView();
                string fieldId = field.Id.ToString();
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + fieldId + "']");

                fieldNode.Attributes["Name"].Value = field.Name.ToString();
                fieldNode.Attributes["PromptText"].Value = field.PromptText.ToString();
                fieldNode.Attributes["ControlFontFamily"].Value = field.ControlFont.ToString();
                fieldNode.Attributes["ControlFontStyle"].Value = field.ControlFont.Style.ToString();
                fieldNode.Attributes["ControlFontSize"].Value = field.ControlFont.Size.ToString();
                fieldNode.Attributes["ControlTopPositionPercentage"].Value = field.ControlTopPositionPercentage.ToString();
                fieldNode.Attributes["ControlLeftPositionPercentage"].Value = field.ControlLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlHeightPercentage"].Value = field.ControlHeightPercentage.ToString();
                fieldNode.Attributes["ControlWidthPercentage"].Value = field.ControlWidthPercentage.ToString();
                fieldNode.Attributes["TabIndex"].Value = field.TabIndex.ToString();
                fieldNode.Attributes["HasTabStop"].Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldNode.Attributes["PromptFontFamily"].Value = field.PromptFont.FontFamily.Name.ToString();
                fieldNode.Attributes["PromptFontStyle"].Value = field.PromptFont.Style.ToString();
                fieldNode.Attributes["PromptFontSize"].Value = field.PromptFont.Size.ToString();
                fieldNode.Attributes["PromptScriptName"].Value = field.PromptFont.Name.ToString();
                fieldNode.Attributes["PromptTopPositionPercentage"].Value = field.PromptTopPositionPercentage.ToString();
                fieldNode.Attributes["PromptLeftPositionPercentage"].Value = field.PromptLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlScriptName"].Value = field.ControlFont.Name.ToString();
                fieldNode.Attributes["ShouldRepeatLast"].Value = (bool.Parse(field.ShouldRepeatLast.ToString())).ToString();
                fieldNode.Attributes["IsRequired"].Value = (bool.Parse(field.IsRequired.ToString())).ToString();
                fieldNode.Attributes["IsReadOnly"].Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldNode.Attributes["DataTableName"].Value = field.TableName.ToString();
                fieldNode.FirstChild.Attributes["CheckCodeBefore"].Value = field.CheckCodeBefore.ToString();
                fieldNode.LastChild.Attributes["CheckCodeAfter"].Value = field.CheckCodeAfter.ToString();

                view.Project.Save();
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
        /// <returns>Id of the updated field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(GetFieldViewElement(field));
                View view = field.GetView();
                string fieldId = field.Id.ToString();
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + fieldId + "']");

                fieldNode.Attributes["Name"].Value = field.Name.ToString();
                fieldNode.Attributes["PromptText"].Value = field.PromptText.ToString();
                fieldNode.Attributes["ControlFontFamily"].Value = field.ControlFont.ToString();
                fieldNode.Attributes["ControlFontStyle"].Value = field.ControlFont.Style.ToString();
                fieldNode.Attributes["ControlFontSize"].Value = field.ControlFont.Size.ToString();
                fieldNode.Attributes["ControlTopPositionPercentage"].Value = field.ControlTopPositionPercentage.ToString();
                fieldNode.Attributes["ControlLeftPositionPercentage"].Value = field.ControlLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlHeightPercentage"].Value = field.ControlHeightPercentage.ToString();
                fieldNode.Attributes["ControlWidthPercentage"].Value = field.ControlWidthPercentage.ToString();
                fieldNode.Attributes["TabIndex"].Value = field.TabIndex.ToString();
                fieldNode.Attributes["HasTabStop"].Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldNode.Attributes["PromptFontFamily"].Value = field.PromptFont.FontFamily.Name.ToString();
                fieldNode.Attributes["PromptFontStyle"].Value = field.PromptFont.Style.ToString();
                fieldNode.Attributes["PromptFontSize"].Value = field.PromptFont.Size.ToString();
                fieldNode.Attributes["PromptScriptName"].Value = field.PromptFont.Name.ToString();
                fieldNode.Attributes["PromptTopPositionPercentage"].Value = field.PromptTopPositionPercentage.ToString();
                fieldNode.Attributes["PromptLeftPositionPercentage"].Value = field.PromptLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlScriptName"].Value = field.ControlFont.Name.ToString();
                fieldNode.Attributes["ShouldRepeatLast"].Value = (bool.Parse(field.ShouldRepeatLast.ToString())).ToString();
                fieldNode.Attributes["IsRequired"].Value = (bool.Parse(field.IsRequired.ToString())).ToString();
                fieldNode.Attributes["IsReadOnly"].Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                //fieldNode.Attributes["ShouldRetainImageSize"].Value = (bool.Parse(field.IsControlResizable)).ToString();
                fieldNode.Attributes["CodeColumnName"].Value = field.CodeColumnName.ToString();
                fieldNode.Attributes["IsExclusiveTable"].Value = (bool.Parse(field.IsExclusiveTable.ToString())).ToString();
                fieldNode.Attributes["Sort"].Value = (bool.Parse(field.ShouldSort.ToString())).ToString();
                fieldNode.Attributes["SourceTableName"].Value = field.SourceTableName.ToString();
                fieldNode.Attributes["TextColumnName"].Value = field.TextColumnName.ToString();
                fieldNode.Attributes["DataTableName"].Value = field.TableName.ToString();
                fieldNode.FirstChild.Attributes["CheckCodeBefore"].Value = field.CheckCodeBefore;
                fieldNode.LastChild.Attributes["CheckCodeAfter"].Value = field.CheckCodeAfter;

                view.Project.Save();
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
        /// Update List dropdown list field.
        /// </summary>
        /// <param name="field">Codes dropdown list field to update.</param>
        /// <returns>Id of the updated field.</returns>
        public void UpdateField(DDListField field)
        {
            try
            {
                #region InputValidation
                if (field == null)
                {
                    throw new ArgumentNullException("DDListField");
                }
                #endregion

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(GetFieldViewElement(field));
                View view = field.GetView();
                string fieldId = field.Id.ToString();
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + fieldId + "']");

                fieldNode.Attributes["Name"].Value = field.Name.ToString();
                fieldNode.Attributes["PromptText"].Value = field.PromptText.ToString();
                fieldNode.Attributes["ControlFontFamily"].Value = field.ControlFont.ToString();
                fieldNode.Attributes["ControlFontStyle"].Value = field.ControlFont.Style.ToString();
                fieldNode.Attributes["ControlFontSize"].Value = field.ControlFont.Size.ToString();
                fieldNode.Attributes["ControlTopPositionPercentage"].Value = field.ControlTopPositionPercentage.ToString();
                fieldNode.Attributes["ControlLeftPositionPercentage"].Value = field.ControlLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlHeightPercentage"].Value = field.ControlHeightPercentage.ToString();
                fieldNode.Attributes["ControlWidthPercentage"].Value = field.ControlWidthPercentage.ToString();
                fieldNode.Attributes["TabIndex"].Value = field.TabIndex.ToString();
                fieldNode.Attributes["HasTabStop"].Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldNode.Attributes["PromptFontFamily"].Value = field.PromptFont.FontFamily.Name.ToString();
                fieldNode.Attributes["PromptFontStyle"].Value = field.PromptFont.Style.ToString();
                fieldNode.Attributes["PromptFontSize"].Value = field.PromptFont.Size.ToString();
                fieldNode.Attributes["PromptScriptName"].Value = field.PromptFont.Name.ToString();
                fieldNode.Attributes["PromptTopPositionPercentage"].Value = field.PromptTopPositionPercentage.ToString();
                fieldNode.Attributes["PromptLeftPositionPercentage"].Value = field.PromptLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlScriptName"].Value = field.ControlFont.Name.ToString();
                fieldNode.Attributes["ShouldRepeatLast"].Value = (bool.Parse(field.ShouldRepeatLast.ToString())).ToString();
                fieldNode.Attributes["IsRequired"].Value = (bool.Parse(field.IsRequired.ToString())).ToString();
                fieldNode.Attributes["IsReadOnly"].Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                //fieldNode.Attributes["ShouldRetainImageSize"].Value = (bool.Parse(field.IsControlResizable)).ToString();
                fieldNode.Attributes["CodeColumnName"].Value = field.CodeColumnName.ToString();
                fieldNode.Attributes["IsExclusiveTable"].Value = (bool.Parse(field.IsExclusiveTable.ToString())).ToString();
                fieldNode.Attributes["Sort"].Value = (bool.Parse(field.ShouldSort.ToString())).ToString();
                fieldNode.Attributes["SourceTableName"].Value = field.SourceTableName.ToString();
                fieldNode.Attributes["TextColumnName"].Value = field.TextColumnName.ToString();
                fieldNode.Attributes["DataTableName"].Value = field.TableName.ToString();
                fieldNode.FirstChild.Attributes["CheckCodeBefore"].Value = field.CheckCodeBefore;
                fieldNode.LastChild.Attributes["CheckCodeAfter"].Value = field.CheckCodeAfter;

                view.Project.Save();
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update DDListField in the database", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Update Command dropdown list field.
        /// </summary>
        /// <param name="field">Command dropdown list field to update.</param>
        /// <returns>Id of the updated field.</returns>
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


                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(GetFieldViewElement(field));
                View view = field.GetView();
                string fieldId = field.Id.ToString();
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + fieldId + "']");

                fieldNode.Attributes["Name"].Value = field.Name.ToString();
                fieldNode.Attributes["PromptText"].Value = field.PromptText.ToString();
                fieldNode.Attributes["ControlFontFamily"].Value = field.ControlFont.ToString();
                fieldNode.Attributes["ControlFontStyle"].Value = field.ControlFont.Style.ToString();
                fieldNode.Attributes["ControlFontSize"].Value = field.ControlFont.Size.ToString();
                fieldNode.Attributes["ControlTopPositionPercentage"].Value = field.ControlTopPositionPercentage.ToString();
                fieldNode.Attributes["ControlLeftPositionPercentage"].Value = field.ControlLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlHeightPercentage"].Value = field.ControlHeightPercentage.ToString();
                fieldNode.Attributes["ControlWidthPercentage"].Value = field.ControlWidthPercentage.ToString();
                fieldNode.Attributes["TabIndex"].Value = field.TabIndex.ToString();
                fieldNode.Attributes["HasTabStop"].Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldNode.Attributes["PromptFontFamily"].Value = field.PromptFont.FontFamily.Name.ToString();
                fieldNode.Attributes["PromptFontStyle"].Value = field.PromptFont.Style.ToString();
                fieldNode.Attributes["PromptFontSize"].Value = field.PromptFont.Size.ToString();
                fieldNode.Attributes["PromptScriptName"].Value = field.PromptFont.Name.ToString();
                fieldNode.Attributes["PromptTopPositionPercentage"].Value = field.PromptTopPositionPercentage.ToString();
                fieldNode.Attributes["PromptLeftPositionPercentage"].Value = field.PromptLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlScriptName"].Value = field.ControlFont.Name.ToString();
                fieldNode.Attributes["ShouldRepeatLast"].Value = (bool.Parse(field.ShouldRepeatLast.ToString())).ToString();
                fieldNode.Attributes["IsRequired"].Value = (bool.Parse(field.IsRequired.ToString())).ToString();
                fieldNode.Attributes["IsReadOnly"].Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldNode.Attributes["CodeColumnName"].Value = field.CodeColumnName.ToString();
                fieldNode.Attributes["IsExclusiveTable"].Value = (bool.Parse(field.IsExclusiveTable.ToString())).ToString();
                fieldNode.Attributes["Sort"].Value = (bool.Parse(field.ShouldSort.ToString())).ToString();
                fieldNode.Attributes["SourceTableName"].Value = field.SourceTableName.ToString();
                fieldNode.Attributes["TextColumnName"].Value = field.TextColumnName.ToString();
                fieldNode.Attributes["DataTableName"].Value = field.TableName.ToString();
                fieldNode.FirstChild.Attributes["CheckCodeBefore"].Value = field.CheckCodeBefore.ToString();
                fieldNode.LastChild.Attributes["CheckCodeAfter"].Value = field.CheckCodeAfter.ToString();

                view.Project.Save();
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
        /// Update Legal Values dropdown list field.
        /// </summary>
        /// <param name="field">Legal Values dropdown list field to update.</param>
        /// <returns>Id of the updated field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(GetFieldViewElement(field));
                View view = field.GetView();
                string fieldId = field.Id.ToString();
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + fieldId + "']");

                fieldNode.Attributes["Name"].Value = field.Name.ToString();
                fieldNode.Attributes["PromptText"].Value = field.PromptText.ToString();
                fieldNode.Attributes["ControlFontFamily"].Value = field.ControlFont.ToString();
                fieldNode.Attributes["ControlFontStyle"].Value = field.ControlFont.Style.ToString();
                fieldNode.Attributes["ControlFontSize"].Value = field.ControlFont.Size.ToString();
                fieldNode.Attributes["ControlTopPositionPercentage"].Value = field.ControlTopPositionPercentage.ToString();
                fieldNode.Attributes["ControlLeftPositionPercentage"].Value = field.ControlLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlHeightPercentage"].Value = field.ControlHeightPercentage.ToString();
                fieldNode.Attributes["ControlWidthPercentage"].Value = field.ControlWidthPercentage.ToString();
                fieldNode.Attributes["TabIndex"].Value = field.TabIndex.ToString();
                fieldNode.Attributes["HasTabStop"].Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldNode.Attributes["PromptFontFamily"].Value = field.PromptFont.FontFamily.Name.ToString();
                fieldNode.Attributes["PromptFontStyle"].Value = field.PromptFont.Style.ToString();
                fieldNode.Attributes["PromptFontSize"].Value = field.PromptFont.Size.ToString();
                fieldNode.Attributes["PromptScriptName"].Value = field.PromptFont.Name.ToString();
                fieldNode.Attributes["PromptTopPositionPercentage"].Value = field.PromptTopPositionPercentage.ToString();
                fieldNode.Attributes["PromptLeftPositionPercentage"].Value = field.PromptLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlScriptName"].Value = field.ControlFont.Name.ToString();
                fieldNode.Attributes["ShouldRepeatLast"].Value = (bool.Parse(field.ShouldRepeatLast.ToString())).ToString();
                fieldNode.Attributes["IsRequired"].Value = (bool.Parse(field.IsRequired.ToString())).ToString();
                fieldNode.Attributes["IsReadOnly"].Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldNode.Attributes["CodeColumnName"].Value = field.CodeColumnName.ToString();
                fieldNode.Attributes["IsExclusiveTable"].Value = (bool.Parse(field.IsExclusiveTable.ToString())).ToString();
                fieldNode.Attributes["Sort"].Value = (bool.Parse(field.ShouldSort.ToString())).ToString();
                fieldNode.Attributes["SourceTableName"].Value = field.SourceTableName.ToString();
                fieldNode.Attributes["TextColumnName"].Value = field.TextColumnName.ToString();
                fieldNode.Attributes["DataTableName"].Value = field.TableName.ToString();
                fieldNode.FirstChild.Attributes["CheckCodeBefore"].Value = field.CheckCodeBefore.ToString();
                fieldNode.LastChild.Attributes["CheckCodeAfter"].Value = field.CheckCodeAfter.ToString();

                view.Project.Save();
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
        /// <returns>Id of the updated field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(GetFieldViewElement(field));
                View view = field.GetView();
                string fieldId = field.Id.ToString();
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + fieldId + "']");

                fieldNode.Attributes["Name"].Value = field.Name.ToString();
                fieldNode.Attributes["PromptText"].Value = field.PromptText.ToString();
                fieldNode.Attributes["ControlFontFamily"].Value = field.ControlFont.ToString();
                fieldNode.Attributes["ControlFontStyle"].Value = field.ControlFont.Style.ToString();
                fieldNode.Attributes["ControlFontSize"].Value = field.ControlFont.Size.ToString();
                fieldNode.Attributes["ControlTopPositionPercentage"].Value = field.ControlTopPositionPercentage.ToString();
                fieldNode.Attributes["ControlLeftPositionPercentage"].Value = field.ControlLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlHeightPercentage"].Value = field.ControlHeightPercentage.ToString();
                fieldNode.Attributes["ControlWidthPercentage"].Value = field.ControlWidthPercentage.ToString();
                fieldNode.Attributes["TabIndex"].Value = field.TabIndex.ToString();
                fieldNode.Attributes["HasTabStop"].Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldNode.Attributes["PromptFontFamily"].Value = field.PromptFont.FontFamily.Name.ToString();
                fieldNode.Attributes["PromptFontStyle"].Value = field.PromptFont.Style.ToString();
                fieldNode.Attributes["PromptFontSize"].Value = field.PromptFont.Size.ToString();
                fieldNode.Attributes["PromptScriptName"].Value = field.PromptFont.Name.ToString();
                fieldNode.Attributes["ControlScriptName"].Value = field.ControlFont.Name.ToString();
                //fieldNode.Attributes["ShouldRetainImageSize"].Value = (bool.Parse(field.IsControlResizable)).ToString();

                //TODO:  Update Columns

                view.Project.Save();
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
        /// Update Group field.
        /// </summary>
        /// <param name="field">Group field to update.</param>
        public void UpdateField(GroupField field)
        {
        }

        /// <summary>
        /// Update GUID field.
        /// </summary>
        /// <param name="field">GUID field to update.</param>
        /// <returns>Id of the updated field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(GetFieldViewElement(field));
                View view = field.GetView();
                string fieldId = field.Id.ToString();
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + fieldId + "']");

                fieldNode.Attributes["Name"].Value = field.Name;
                fieldNode.Attributes["PromptText"].Value = field.PromptText;
                fieldNode.Attributes["ControlFontFamily"].Value = field.ControlFont.FontFamily.Name;
                fieldNode.Attributes["ControlFontStyle"].Value = field.ControlFont.Style.ToString();
                fieldNode.Attributes["ControlFontSize"].Value = field.ControlFont.Size.ToString();
                fieldNode.Attributes["ControlTopPositionPercentage"].Value = field.ControlTopPositionPercentage.ToString();
                fieldNode.Attributes["ControlLeftPositionPercentage"].Value = field.ControlLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlHeightPercentage"].Value = field.ControlHeightPercentage.ToString();
                fieldNode.Attributes["ControlWidthPercentage"].Value = field.ControlWidthPercentage.ToString();
                fieldNode.Attributes["TabIndex"].Value = field.TabIndex.ToString();
                fieldNode.Attributes["HasTabStop"].Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldNode.Attributes["PromptFontFamily"].Value = field.PromptFont.FontFamily.Name;
                fieldNode.Attributes["PromptFontStyle"].Value = field.PromptFont.Style.ToString();
                fieldNode.Attributes["PromptFontSize"].Value = field.PromptFont.Size.ToString();
                fieldNode.Attributes["PromptScriptName"].Value = field.PromptFont.Name;
                fieldNode.Attributes["PromptTopPositionPercentage"].Value = field.PromptTopPositionPercentage.ToString();
                fieldNode.Attributes["PromptLeftPositionPercentage"].Value = field.PromptLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlScriptName"].Value = field.ControlFont.Name;
                fieldNode.Attributes["ShouldRepeatLast"].Value = bool.Parse(field.ShouldRepeatLast.ToString()).ToString();
                fieldNode.Attributes["IsRequired"].Value = (bool.Parse(field.IsRequired.ToString()).ToString());
                fieldNode.Attributes["IsReadOnly"].Value = (bool.Parse(field.IsReadOnly.ToString()).ToString());
                fieldNode.Attributes["MaxLength"].Value = field.MaxLength.ToString();
                fieldNode.Attributes["DataTableName"].Value = field.TableName;
                fieldNode.Attributes["SourceFieldId"].Value = field.SourceFieldId.ToString();

                fieldNode.FirstChild.Attributes["CheckCodeBefore"].Value = field.CheckCodeBefore;
                fieldNode.LastChild.Attributes["CheckCodeAfter"].Value = field.CheckCodeAfter;

                view.Project.Save();
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update SingleLineTextField in the metadata project file", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Update Image field.
        /// </summary>
        /// <param name="field">Image field to update.</param>
        /// <returns>Id of the updated field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(GetFieldViewElement(field));
                View view = field.GetView();
                string fieldId = field.Id.ToString();
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + fieldId + "']");

                fieldNode.Attributes["Name"].Value = field.Name.ToString();
                fieldNode.Attributes["PromptText"].Value = field.PromptText.ToString();
                fieldNode.Attributes["ControlFontFamily"].Value = field.ControlFont.ToString();
                fieldNode.Attributes["ControlFontStyle"].Value = field.ControlFont.Style.ToString();
                fieldNode.Attributes["ControlFontSize"].Value = field.ControlFont.Size.ToString();
                fieldNode.Attributes["ControlTopPositionPercentage"].Value = field.ControlTopPositionPercentage.ToString();
                fieldNode.Attributes["ControlLeftPositionPercentage"].Value = field.ControlLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlHeightPercentage"].Value = field.ControlHeightPercentage.ToString();
                fieldNode.Attributes["ControlWidthPercentage"].Value = field.ControlWidthPercentage.ToString();
                fieldNode.Attributes["TabIndex"].Value = field.TabIndex.ToString();
                fieldNode.Attributes["HasTabStop"].Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldNode.Attributes["PromptFontFamily"].Value = field.PromptFont.FontFamily.Name.ToString();
                fieldNode.Attributes["PromptFontStyle"].Value = field.PromptFont.Style.ToString();
                fieldNode.Attributes["PromptFontSize"].Value = field.PromptFont.Size.ToString();
                fieldNode.Attributes["PromptScriptName"].Value = field.PromptFont.Name.ToString();
                fieldNode.Attributes["PromptTopPositionPercentage"].Value = field.PromptTopPositionPercentage.ToString();
                fieldNode.Attributes["PromptLeftPositionPercentage"].Value = field.PromptLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlScriptName"].Value = field.ControlFont.Name;
                fieldNode.Attributes["ShouldRepeatLast"].Value = bool.Parse(field.ShouldRepeatLast.ToString()).ToString();
                fieldNode.Attributes["IsRequired"].Value = (bool.Parse(field.IsRequired.ToString()).ToString());
                fieldNode.Attributes["IsReadOnly"].Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();

                //fieldNode.Attributes["ShouldRetainImageSize"].Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();
                field.TableName = fieldNode.Attributes["DataTableName"].Value;

                view.Project.Save();
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
        /// <returns>Id of the updated field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(GetFieldViewElement(field));
                View view = field.GetView();
                string fieldId = field.Id.ToString();
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + fieldId + "']");



                fieldNode.Attributes["Name"].Value = field.Name.ToString();
                fieldNode.Attributes["PromptText"].Value = field.PromptText.ToString();
                fieldNode.Attributes["ControlFontFamily"].Value = field.ControlFont.FontFamily.Name.ToString();
                fieldNode.Attributes["ControlFontStyle"].Value = field.ControlFont.Style.ToString();
                fieldNode.Attributes["ControlFontSize"].Value = field.ControlFont.Size.ToString();
                fieldNode.Attributes["ControlTopPositionPercentage"].Value = field.ControlTopPositionPercentage.ToString();
                fieldNode.Attributes["ControlLeftPositionPercentage"].Value = field.ControlLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlHeightPercentage"].Value = field.ControlHeightPercentage.ToString();
                fieldNode.Attributes["ControlWidthPercentage"].Value = field.ControlWidthPercentage.ToString();
                fieldNode.Attributes["TabIndex"].Value = field.TabIndex.ToString();
                fieldNode.Attributes["HasTabStop"].Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldNode.Attributes["PromptFontFamily"].Value = field.PromptFont.FontFamily.Name.ToString();
                fieldNode.Attributes["PromptFontStyle"].Value = field.PromptFont.Style.ToString();
                fieldNode.Attributes["PromptFontSize"].Value = field.PromptFont.Size.ToString();
                fieldNode.Attributes["PromptScriptName"].Value = field.PromptFont.Name.ToString();

                fieldNode.Attributes["ControlScriptName"].Value = field.ControlFont.Name.ToString();
                //fieldNode.Attributes["ShouldRetainImageSize"].Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();

                view.Project.Save();
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
        /// Update Mirror field.
        /// </summary>
        /// <param name="field">Mirror field to update.</param>
        /// <returns>Id of the updated field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(GetFieldViewElement(field));
                View view = field.GetView();
                string fieldId = field.Id.ToString();
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + fieldId + "']");

                fieldNode.Attributes["Name"].Value = field.Name.ToString();
                fieldNode.Attributes["PromptText"].Value = field.PromptText.ToString();
                fieldNode.Attributes["ControlFontFamily"].Value = field.ControlFont.FontFamily.Name.ToString();
                fieldNode.Attributes["ControlFontStyle"].Value = field.ControlFont.Style.ToString();
                fieldNode.Attributes["ControlFontSize"].Value = field.ControlFont.Size.ToString();
                fieldNode.Attributes["ControlTopPositionPercentage"].Value = field.ControlTopPositionPercentage.ToString();
                fieldNode.Attributes["ControlLeftPositionPercentage"].Value = field.ControlLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlHeightPercentage"].Value = field.ControlHeightPercentage.ToString();
                fieldNode.Attributes["ControlWidthPercentage"].Value = field.ControlWidthPercentage.ToString();
                fieldNode.Attributes["TabIndex"].Value = field.TabIndex.ToString();
                fieldNode.Attributes["HasTabStop"].Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldNode.Attributes["PromptFontFamily"].Value = field.PromptFont.FontFamily.Name.ToString();
                fieldNode.Attributes["PromptFontStyle"].Value = field.PromptFont.Style.ToString();
                fieldNode.Attributes["PromptFontSize"].Value = field.PromptFont.Size.ToString();
                fieldNode.Attributes["PromptScriptName"].Value = field.PromptFont.Name.ToString();
                fieldNode.Attributes["PromptTopPositionPercentage"].Value = field.PromptTopPositionPercentage.ToString();
                fieldNode.Attributes["PromptLeftPositionPercentage"].Value = field.PromptLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlScriptName"].Value = field.ControlFont.Name.ToString();

                //fieldNode.Attributes["ShouldRetainImageSize"].Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();
                fieldNode.Attributes["SourceFieldId"].Value = field.SourceFieldId.ToString();

                view.Project.Save();
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
        /// <returns>Id of the updated field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(GetFieldViewElement(field));
                View view = field.GetView();
                string fieldId = field.Id.ToString();
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + fieldId + "']");

                fieldNode.Attributes["Name"].Value = field.Name.ToString();
                fieldNode.Attributes["PromptText"].Value = field.PromptText.ToString();
                fieldNode.Attributes["ControlFontFamily"].Value = field.ControlFont.FontFamily.Name.ToString();
                fieldNode.Attributes["ControlFontStyle"].Value = field.ControlFont.Style.ToString();
                fieldNode.Attributes["ControlFontSize"].Value = field.ControlFont.Size.ToString();
                fieldNode.Attributes["ControlTopPositionPercentage"].Value = field.ControlTopPositionPercentage.ToString();
                fieldNode.Attributes["ControlLeftPositionPercentage"].Value = field.ControlLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlHeightPercentage"].Value = field.ControlHeightPercentage.ToString();
                fieldNode.Attributes["ControlWidthPercentage"].Value = field.ControlWidthPercentage.ToString();
                fieldNode.Attributes["TabIndex"].Value = field.TabIndex.ToString();
                fieldNode.Attributes["HasTabStop"].Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldNode.Attributes["PromptFontFamily"].Value = field.PromptFont.FontFamily.Name.ToString();
                fieldNode.Attributes["PromptFontStyle"].Value = field.PromptFont.Style.ToString();
                fieldNode.Attributes["PromptFontSize"].Value = field.PromptFont.Size.ToString();
                fieldNode.Attributes["PromptScriptName"].Value = field.PromptFont.Name.ToString();
                fieldNode.Attributes["PromptTopPositionPercentage"].Value = field.PromptTopPositionPercentage.ToString();
                fieldNode.Attributes["PromptLeftPositionPercentage"].Value = field.PromptLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlScriptName"].Value = field.ControlFont.Name.ToString();
                fieldNode.Attributes["ShouldRepeatLast"].Value = bool.Parse(field.ShouldRepeatLast.ToString()).ToString();
                fieldNode.Attributes["IsRequired"].Value = (bool.Parse(field.IsRequired.ToString()).ToString());
                fieldNode.Attributes["IsReadOnly"].Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                //fieldNode.Attributes["ShouldRetainImageSize"].Value = (bool.Parse(field.IsControlResizable.ToString())).ToString();
                fieldNode.Attributes["MaxLength"].Value = field.MaxLength.ToString();
                fieldNode.Attributes["DataTableName"].Value = field.TableName.ToString();
                fieldNode.Attributes["SourceFieldId"].Value = field.SourceFieldId.ToString();

                fieldNode.FirstChild.Attributes["CheckCodeBefore"].Value = field.CheckCodeBefore.ToString();
                fieldNode.LastChild.Attributes["CheckCodeAfter"].Value = field.CheckCodeAfter.ToString();

                view.Project.Save();
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
        /// <returns>Id of the updated field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(GetFieldViewElement(field));
                View view = field.GetView();
                string fieldId = field.Id.ToString();
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + fieldId + "']");

                fieldNode.Attributes["Name"].Value = field.Name.ToString();
                fieldNode.Attributes["PromptText"].Value = field.PromptText.ToString();
                fieldNode.Attributes["ControlFontFamily"].Value = field.ControlFont.ToString();
                fieldNode.Attributes["ControlFontStyle"].Value = field.ControlFont.Style.ToString();
                fieldNode.Attributes["ControlFontSize"].Value = field.ControlFont.Size.ToString();
                fieldNode.Attributes["ControlTopPositionPercentage"].Value = field.ControlTopPositionPercentage.ToString();
                fieldNode.Attributes["ControlLeftPositionPercentage"].Value = field.ControlLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlHeightPercentage"].Value = field.ControlHeightPercentage.ToString();
                fieldNode.Attributes["ControlWidthPercentage"].Value = field.ControlWidthPercentage.ToString();
                fieldNode.Attributes["TabIndex"].Value = field.TabIndex.ToString();
                fieldNode.Attributes["HasTabStop"].Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldNode.Attributes["PromptFontFamily"].Value = field.PromptFont.FontFamily.Name.ToString();
                fieldNode.Attributes["PromptFontStyle"].Value = field.PromptFont.Style.ToString();
                fieldNode.Attributes["PromptFontSize"].Value = field.PromptFont.Size.ToString();
                fieldNode.Attributes["PromptScriptName"].Value = field.PromptFont.Name.ToString();
                fieldNode.Attributes["PromptTopPositionPercentage"].Value = field.PromptTopPositionPercentage.ToString();
                fieldNode.Attributes["PromptLeftPositionPercentage"].Value = field.PromptLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlScriptName"].Value = field.ControlFont.Name.ToString();
                fieldNode.Attributes["ShouldRepeatLast"].Value = (bool.Parse(field.ShouldRepeatLast.ToString())).ToString();
                fieldNode.Attributes["IsRequired"].Value = (bool.Parse(field.IsRequired.ToString())).ToString();
                fieldNode.Attributes["IsReadOnly"].Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                //fieldNode.Attributes["ShouldRetainImageSize"].Value = (bool.Parse(field.IsControlResizable));
                fieldNode.Attributes["Lower"].Value = field.Lower.ToString();
                fieldNode.Attributes["Upper"].Value = field.Upper.ToString();
                fieldNode.Attributes["Pattern"].Value = field.Pattern.ToString();
                fieldNode.Attributes["DataTableName"].Value = field.TableName.ToString();
                fieldNode.FirstChild.Attributes["CheckCodeBefore"].Value = field.CheckCodeBefore.ToString();
                fieldNode.LastChild.Attributes["CheckCodeAfter"].Value = field.CheckCodeAfter.ToString();

                view.Project.Save();
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
        /// <returns>Id of the updated field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(GetFieldViewElement(field));
                View view = field.GetView();
                string fieldId = field.Id.ToString();
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + fieldId + "']");

                fieldNode.Attributes["Name"].Value = field.Name;
                fieldNode.Attributes["PromptText"].Value = field.PromptText;
                fieldNode.Attributes["ControlFontFamily"].Value = field.ControlFont.FontFamily.Name;
                fieldNode.Attributes["ControlFontStyle"].Value = field.ControlFont.Style.ToString();
                fieldNode.Attributes["ControlFontSize"].Value = field.ControlFont.Size.ToString();
                fieldNode.Attributes["ControlTopPositionPercentage"].Value = field.ControlTopPositionPercentage.ToString();
                fieldNode.Attributes["ControlLeftPositionPercentage"].Value = field.ControlLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlHeightPercentage"].Value = field.ControlHeightPercentage.ToString();
                fieldNode.Attributes["ControlWidthPercentage"].Value = field.ControlWidthPercentage.ToString();
                fieldNode.Attributes["TabIndex"].Value = field.TabIndex.ToString();
                fieldNode.Attributes["HasTabStop"].Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldNode.Attributes["PromptFontFamily"].Value = field.PromptFont.FontFamily.Name;
                fieldNode.Attributes["PromptFontStyle"].Value = field.PromptFont.Style.ToString();
                fieldNode.Attributes["PromptFontSize"].Value = field.PromptFont.Size.ToString();
                fieldNode.Attributes["PromptScriptName"].Value = field.PromptFont.Name;
                fieldNode.Attributes["ControlScriptName"].Value = field.ControlFont.Name;
                fieldNode.Attributes["ShouldRepeatLast"].Value = bool.Parse(field.ShouldRepeatLast.ToString()).ToString();
                fieldNode.Attributes["IsRequired"].Value = (bool.Parse(field.IsRequired.ToString()).ToString());
                fieldNode.Attributes["IsReadOnly"].Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldNode.Attributes["ShowTextOnRight"].Value = (bool.Parse(field.ShowTextOnRight.ToString())).ToString();
                //field.IsControlResizable = bool.Parse(fieldNode.Attributes["ShouldRetainImageSize"].Value);
                fieldNode.Attributes["DataTableName"].Value = field.TableName;
                fieldNode.LastChild.Attributes["CheckCodeAfter"].Value = field.CheckCodeAfter;

                view.Project.Save();
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
        /// Update Phone Number field.
        /// </summary>
        /// <param name="field">Phone Number field to update.</param>
        /// <returns>Id of the updated field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();

                XmlNode fieldsNode = GetFieldsNode(GetFieldViewElement(field));
                View view = field.GetView();
                string fieldId = field.Id.ToString();
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + fieldId + "']");

                fieldNode.Attributes["Name"].Value = field.Name.ToString();
                fieldNode.Attributes["PromptText"].Value = field.PromptText.ToString();
                fieldNode.Attributes["ControlFontFamily"].Value = field.ControlFont.ToString();
                fieldNode.Attributes["ControlFontStyle"].Value = field.ControlFont.Style.ToString();
                fieldNode.Attributes["ControlFontSize"].Value = field.ControlFont.Size.ToString();
                fieldNode.Attributes["ControlTopPositionPercentage"].Value = field.ControlTopPositionPercentage.ToString();
                fieldNode.Attributes["ControlLeftPositionPercentage"].Value = field.ControlLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlHeightPercentage"].Value = field.ControlHeightPercentage.ToString();
                fieldNode.Attributes["ControlWidthPercentage"].Value = field.ControlWidthPercentage.ToString();
                fieldNode.Attributes["TabIndex"].Value = field.TabIndex.ToString();
                fieldNode.Attributes["HasTabStop"].Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldNode.Attributes["PromptFontFamily"].Value = field.PromptFont.FontFamily.Name.ToString();
                fieldNode.Attributes["PromptFontStyle"].Value = field.PromptFont.Style.ToString();
                fieldNode.Attributes["PromptFontSize"].Value = field.PromptFont.Size.ToString();
                fieldNode.Attributes["PromptScriptName"].Value = field.PromptFont.Name.ToString();
                fieldNode.Attributes["PromptTopPositionPercentage"].Value = field.PromptTopPositionPercentage.ToString();
                fieldNode.Attributes["PromptLeftPositionPercentage"].Value = field.PromptLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlScriptName"].Value = field.ControlFont.Name.ToString();
                fieldNode.Attributes["ShouldRepeatLast"].Value = (bool.Parse(field.ShouldRepeatLast.ToString())).ToString();
                fieldNode.Attributes["IsRequired"].Value = (bool.Parse(field.IsRequired.ToString())).ToString();
                fieldNode.Attributes["IsReadOnly"].Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                //fieldNode.Attributes["ShouldRetainImageSize"].Value = (bool.Parse(field.IsControlResizable));
                fieldNode.Attributes["Pattern"].Value = field.Pattern.ToString();
                fieldNode.Attributes["DataTableName"].Value = field.TableName.ToString();
                fieldNode.FirstChild.Attributes["CheckCodeBefore"].Value = field.CheckCodeBefore.ToString();
                fieldNode.LastChild.Attributes["CheckCodeAfter"].Value = field.CheckCodeAfter.ToString();

                view.Project.Save();
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update PhoneNumberField in the database", ex);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Update Related View field.
        /// </summary>
        /// <param name="field">Related View field to update.</param>
        /// <returns>Id of the updated field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(GetFieldViewElement(field));
                View view = field.GetView();
                string fieldId = field.Id.ToString();
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + fieldId + "']");

                fieldNode.Attributes["Name"].Value = field.Name.ToString();
                fieldNode.Attributes["PromptText"].Value = field.PromptText.ToString();
                fieldNode.Attributes["ControlFontFamily"].Value = field.ControlFont.ToString();
                fieldNode.Attributes["ControlFontStyle"].Value = field.ControlFont.Style.ToString();
                fieldNode.Attributes["ControlFontSize"].Value = field.ControlFont.Size.ToString();
                fieldNode.Attributes["ControlTopPositionPercentage"].Value = field.ControlTopPositionPercentage.ToString();
                fieldNode.Attributes["ControlLeftPositionPercentage"].Value = field.ControlLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlHeightPercentage"].Value = field.ControlHeightPercentage.ToString();
                fieldNode.Attributes["ControlWidthPercentage"].Value = field.ControlWidthPercentage.ToString();
                fieldNode.Attributes["TabIndex"].Value = field.TabIndex.ToString();
                fieldNode.Attributes["HasTabStop"].Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldNode.Attributes["PromptFontFamily"].Value = field.PromptFont.FontFamily.Name.ToString();
                fieldNode.Attributes["PromptFontStyle"].Value = field.PromptFont.Style.ToString();
                fieldNode.Attributes["PromptFontSize"].Value = field.PromptFont.Size.ToString();
                fieldNode.Attributes["PromptScriptName"].Value = field.PromptFont.Name.ToString();
                fieldNode.Attributes["ControlScriptName"].Value = field.ControlFont.Name.ToString();
                fieldNode.Attributes["RelateCondition"].Value = field.Condition.ToString();
                fieldNode.Attributes["RelatedViewId"].Value = field.RelatedViewID.ToString();
                fieldNode.Attributes["ShouldReturnToParent"].Value = (bool.Parse(field.ShouldReturnToParent.ToString())).ToString();

                XmlNode viewNode = fieldsNode.ParentNode.SelectSingleNode("//View[@ViewId= '" + view.Id + "']");
                view.IsRelatedView = true;
                viewNode.Attributes["IsRelatedView"].Value = "true";

                view.Project.Save();
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update RelatedViewField in the database", ex);
            }
        }

        /// <summary>
        /// Update Single Line text field.
        /// </summary>
        /// <param name="field">Single Line text field to update.</param>
        /// <returns>Id of the updated field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(GetFieldViewElement(field));
                View view = field.GetView();
                string fieldId = field.Id.ToString();
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + fieldId + "']");

                fieldNode.Attributes["Name"].Value = field.Name;
                fieldNode.Attributes["PromptText"].Value = field.PromptText;
                fieldNode.Attributes["ControlFontFamily"].Value = field.ControlFont.FontFamily.Name;
                fieldNode.Attributes["ControlFontStyle"].Value = field.ControlFont.Style.ToString();
                fieldNode.Attributes["ControlFontSize"].Value = field.ControlFont.Size.ToString();
                fieldNode.Attributes["ControlTopPositionPercentage"].Value = field.ControlTopPositionPercentage.ToString();
                fieldNode.Attributes["ControlLeftPositionPercentage"].Value = field.ControlLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlHeightPercentage"].Value = field.ControlHeightPercentage.ToString();
                fieldNode.Attributes["ControlWidthPercentage"].Value = field.ControlWidthPercentage.ToString();
                fieldNode.Attributes["TabIndex"].Value = field.TabIndex.ToString();
                fieldNode.Attributes["HasTabStop"].Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldNode.Attributes["PromptFontFamily"].Value = field.PromptFont.FontFamily.Name;
                fieldNode.Attributes["PromptFontStyle"].Value = field.PromptFont.Style.ToString();
                fieldNode.Attributes["PromptFontSize"].Value = field.PromptFont.Size.ToString();
                fieldNode.Attributes["PromptScriptName"].Value = field.PromptFont.Name;
                fieldNode.Attributes["PromptTopPositionPercentage"].Value = field.PromptTopPositionPercentage.ToString();
                fieldNode.Attributes["PromptLeftPositionPercentage"].Value = field.PromptLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlScriptName"].Value = field.ControlFont.Name;
                fieldNode.Attributes["ShouldRepeatLast"].Value = bool.Parse(field.ShouldRepeatLast.ToString()).ToString();
                fieldNode.Attributes["IsRequired"].Value = (bool.Parse(field.IsRequired.ToString()).ToString());
                fieldNode.Attributes["IsReadOnly"].Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldNode.Attributes["MaxLength"].Value = field.MaxLength.ToString();
                fieldNode.Attributes["DataTableName"].Value = field.TableName;
                fieldNode.Attributes["SourceFieldId"].Value = field.SourceFieldId.ToString();

                fieldNode.FirstChild.Attributes["CheckCodeBefore"].Value = field.CheckCodeBefore;
                fieldNode.LastChild.Attributes["CheckCodeAfter"].Value = field.CheckCodeAfter;

                view.Project.Save();
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update SingleLineTextField in the metadata project file", ex);
            }
        }

        /// <summary>
        /// Update Time field.
        /// </summary>
        /// <param name="field">Time field to update.</param>
        /// <returns>Id of the updated field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(GetFieldViewElement(field));
                View view = field.GetView();
                string fieldId = field.Id.ToString();
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + fieldId + "']");

                fieldNode.Attributes["Name"].Value = field.Name.ToString();
                fieldNode.Attributes["PromptText"].Value = field.PromptText.ToString();
                fieldNode.Attributes["ControlFontFamily"].Value = field.ControlFont.ToString();
                fieldNode.Attributes["ControlFontStyle"].Value = field.ControlFont.Style.ToString();
                fieldNode.Attributes["ControlFontSize"].Value = field.ControlFont.Size.ToString();
                fieldNode.Attributes["ControlTopPositionPercentage"].Value = field.ControlTopPositionPercentage.ToString();
                fieldNode.Attributes["ControlLeftPositionPercentage"].Value = field.ControlLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlHeightPercentage"].Value = field.ControlHeightPercentage.ToString();
                fieldNode.Attributes["ControlWidthPercentage"].Value = field.ControlWidthPercentage.ToString();
                fieldNode.Attributes["TabIndex"].Value = field.TabIndex.ToString();
                fieldNode.Attributes["HasTabStop"].Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldNode.Attributes["PromptFontFamily"].Value = field.PromptFont.FontFamily.Name.ToString();
                fieldNode.Attributes["PromptFontStyle"].Value = field.PromptFont.Style.ToString();
                fieldNode.Attributes["PromptFontSize"].Value = field.PromptFont.Size.ToString();
                fieldNode.Attributes["PromptScriptName"].Value = field.PromptFont.Name.ToString();
                fieldNode.Attributes["PromptTopPositionPercentage"].Value = field.PromptTopPositionPercentage.ToString();
                fieldNode.Attributes["PromptLeftPositionPercentage"].Value = field.PromptLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlScriptName"].Value = field.ControlFont.Name.ToString();
                fieldNode.Attributes["ShouldRepeatLast"].Value = (bool.Parse(field.ShouldRepeatLast.ToString())).ToString();
                fieldNode.Attributes["IsRequired"].Value = (bool.Parse(field.IsRequired.ToString())).ToString();
                fieldNode.Attributes["IsReadOnly"].Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldNode.Attributes["DataTableName"].Value = field.TableName.ToString();
                fieldNode.FirstChild.Attributes["CheckCodeBefore"].Value = field.CheckCodeBefore.ToString();
                fieldNode.LastChild.Attributes["CheckCodeAfter"].Value = field.CheckCodeAfter.ToString();

                view.Project.Save();
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update TimeField in the database", ex);
            }
        }

        /// <summary>
        /// Update Uppercase text field.
        /// </summary>
        /// <param name="field">Uppercase text field to update.</param>
        /// <returns>Id of the updated field.</returns>
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
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update UpperCaseTextField in the database", ex);
            }
        }

        /// <summary>
        /// Update Yes/No field.
        /// </summary>
        /// <param name="field">Yes/No field to update.</param>
        /// <returns>Id of the updated field.</returns>
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

                XmlDocument xmlDoc = GetXmlDocument();
                XmlNode fieldsNode = GetFieldsNode(GetFieldViewElement(field));
                View view = field.GetView();
                string fieldId = field.Id.ToString();
                XmlNode fieldNode = fieldsNode.SelectSingleNode("//Field[@FieldId= '" + fieldId + "']");

                fieldNode.Attributes["Name"].Value = field.Name.ToString();
                fieldNode.Attributes["PromptText"].Value = field.PromptText.ToString();
                fieldNode.Attributes["ControlFontFamily"].Value = field.ControlFont.ToString();
                fieldNode.Attributes["ControlFontStyle"].Value = field.ControlFont.Style.ToString();
                fieldNode.Attributes["ControlFontSize"].Value = field.ControlFont.Size.ToString();
                fieldNode.Attributes["ControlTopPositionPercentage"].Value = field.ControlTopPositionPercentage.ToString();
                fieldNode.Attributes["ControlLeftPositionPercentage"].Value = field.ControlLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlHeightPercentage"].Value = field.ControlHeightPercentage.ToString();
                fieldNode.Attributes["ControlWidthPercentage"].Value = field.ControlWidthPercentage.ToString();
                fieldNode.Attributes["TabIndex"].Value = field.TabIndex.ToString();
                fieldNode.Attributes["HasTabStop"].Value = (bool.Parse(field.HasTabStop.ToString())).ToString();
                fieldNode.Attributes["PromptFontFamily"].Value = field.PromptFont.FontFamily.Name.ToString();
                fieldNode.Attributes["PromptFontStyle"].Value = field.PromptFont.Style.ToString();
                fieldNode.Attributes["PromptFontSize"].Value = field.PromptFont.Size.ToString();
                fieldNode.Attributes["PromptScriptName"].Value = field.PromptFont.Name.ToString();
                fieldNode.Attributes["PromptTopPositionPercentage"].Value = field.PromptTopPositionPercentage.ToString();
                fieldNode.Attributes["PromptLeftPositionPercentage"].Value = field.PromptLeftPositionPercentage.ToString();
                fieldNode.Attributes["ControlScriptName"].Value = field.ControlFont.Name.ToString();
                fieldNode.Attributes["ShouldRepeatLast"].Value = bool.Parse(field.ShouldRepeatLast.ToString()).ToString();
                fieldNode.Attributes["IsRequired"].Value = (bool.Parse(field.IsRequired.ToString()).ToString());
                fieldNode.Attributes["IsReadOnly"].Value = (bool.Parse(field.IsReadOnly.ToString())).ToString();
                fieldNode.Attributes["DataTableName"].Value = field.TableName.ToString();

                view.Project.Save();
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update YesNoField in the database", ex);
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
                updateQuery.Parameters.Add(new QueryParameter("@DateModified", DbType.String, System.DateTime.Now.ToShortDateString()));
                updateQuery.Parameters.Add(new QueryParameter("@ProgramId", DbType.Int32, programId));

                db.ExecuteNonQuery(updateQuery);

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not update program.", ex);
            }
        }

        /// <summary>
        /// Update Codes Field Sources to database.
        /// </summary>
        /// <param name="fieldId">Id of Codes field.</param>
        /// <param name="sourceFieldId">Id of source field.</param>
        /// <param name="codeColumnName">Column name of code column.</param>
        /// <param name="sourceTableName">Name of source table.</param>
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
        }

        /// <summary>
        /// Updates a date column saved in the database
        /// </summary>
        /// <param name="column">Column to be updated</param>
        public void UpdateGridColumn(ContiguousColumn column)
        {
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
        }

        /// <summary>
        /// Updates a date column saved in the database
        /// </summary>
        /// <param name="column">Column to be updated</param>
        public void UpdateGridColumn(PhoneNumberColumn column)
        {
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
        }

        /// <summary>
        /// Updates a date column saved in the database
        /// </summary>
        /// <param name="column">Column to be updated</param>
        public void UpdateGridColumn(NumberColumn column)
        {
            try
            {
                Query updateQuery = db.CreateQuery("update [metaGridColumns] set [Name] = @Name, [Width] = @Width, [Position] = @Position, [FieldTypeId] = @FieldTypeId, [Text] = @Text, [ShouldRepeatLast] = @ShouldRepeatLast, [IsRequired] = @IsRequired, [IsReadOnly] = @IsReadOnly, [Pattern] = @Pattern, [Upper] = @Upper, [Lower] = @Lower " +
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
        }
        /// <summary>
        /// Updates a TextColumn saved in the database.
        /// </summary>
        /// <param name="column">Column to be updated</param>
        public void UpdateGridColumn(TextColumn column)
        {
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
        }

        /// <summary>
        /// Updates a CheckboxColumn saved in the database.
        /// </summary>
        /// <param name="column">Column to be updated</param>
        public void UpdateGridColumn(CheckboxColumn column)
        {
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
        }

        /// <summary>
        /// Updates a YesNoColumn saved in the database.
        /// </summary>
        /// <param name="column">Column to be updated</param>
        public void UpdateGridColumn(YesNoColumn column)
        {
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
        }

        /// <summary>
        /// Updates a DDLColumnOfCommentLegal saved in the database.
        /// </summary>
        /// <param name="column">Column to be updated</param>
        public void UpdateGridColumn(DDLColumnOfCommentLegal column)
        {
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
        }

        /// <summary>
        /// Updates a DDLColumnOfLegalValues saved in the database.
        /// </summary>
        /// <param name="column">Column to be updated</param>
        public void UpdateGridColumn(DDLColumnOfLegalValues column)
        {
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
        }
        #endregion Update Statements

        #region Utility Methods

        /// <summary>
        /// Returns the latest view instered into the Database.
        /// </summary>
        /// <returns></returns>
        public View GetLatestViewInserted()
        {
            return GetViewById(GetMaxViewId());
        }

        /// <summary>
        /// Returns the Id of the last view added
        /// </summary>
        /// <returns></returns>
        public int GetMaxViewId()
        {
            int maxViewId = 0;
            XmlDocument doc = this.Project.GetXmlDocument();
            XmlNode viewsNode = GetViewsNode();
            XmlNode node = viewsNode.SelectSingleNode(@"//View[not(../View/@ViewId > @ViewId)]/@ViewId");

            if (node == null || node.Value.Trim().Equals(string.Empty))
            {
                maxViewId = 1;
            }
            else if (!node.Value.Equals(string.Empty))
            {
                maxViewId = int.Parse(node.Value) + 1;
            }
            return maxViewId;
        }

        /// <summary>
        /// Returns the Id of the last page added
        /// </summary>
        /// <param name="viewId">The view id of the page</param>
        /// <returns>The maximum page id</returns>
        public int GetMaxPageId(int viewId)
        {
            int maxPageId = 0;
            XmlNode viewsNode = GetViewsNode();
            XmlNode viewNode = viewsNode.SelectSingleNode("//View[@ViewId = '" + viewId + "']");
            XmlNode pageNode = viewNode.SelectSingleNode(@"//Page[not(../Page/@PageId > @PageId)]/@PageId");

            if (pageNode == null || pageNode.Value.Trim().Equals(string.Empty))
            {
                maxPageId = 0;
            }
            else if (!pageNode.Value.Equals(string.Empty))
            {
                maxPageId = int.Parse(pageNode.Value);
            }
            return maxPageId;
        }

        /// <summary>
        /// Returns the Id of the last field added
        /// </summary>
        /// <returns>The maximum BackgroundId</returns>
        public int GetMaxBackgroundId()
        {
            throw new NotImplementedException("Not implemented - GetMaxBackgroundId");
        }
        
        /// <summary>
        /// Returns the Id of the last Image added
        /// </summary>
        /// <returns>The maximum ImageId</returns>
        public int GetMaxImageId()
        {
            throw new NotImplementedException("Not implemented - GetMaxImageId");
        }
        
        /// <summary>
        /// Returns the Id of the last field added
        /// </summary>
        /// <param name="viewId">The view the field belongs to</param>
        /// <returns>The maximum field ID</returns>
        public int GetMaxFieldId(int viewId)
        {
            int maxFieldId = 0;
            XmlNode viewsNode = GetViewsNode();
            XmlNode viewNode = viewsNode.SelectSingleNode("//View[@ViewId = '" + viewId + "']");
            XmlNode fieldNode = viewNode.SelectSingleNode(@"//Field[not(../Field/@FieldId > @FieldId)]/@FieldId");

            if (fieldNode == null || fieldNode.Value.Trim().Equals(string.Empty))
            {
                maxFieldId = 0;
            }
            else if (!fieldNode.Value.Equals(string.Empty))
            {
                maxFieldId = int.Parse(fieldNode.Value);
            }
            return maxFieldId;
        }

        /// <summary>
        /// Returns the maximum map Id
        /// </summary>
        /// <returns>The maximum map Id</returns>
        private int GetMaxMapId()
        {
            int maxMapId = 0;
            XmlNode mapsNode = GetMapsNode();
            XmlNode mapNode = mapsNode.SelectSingleNode(@"//Map[not(../Map/@MapId > @MapId)]/@MapId");

            if (mapNode == null || mapNode.Value.Trim().Equals(string.Empty))
            {
                maxMapId = 0;
            }
            else if (!mapNode.Value.Equals(string.Empty))
            {
                maxMapId = int.Parse(mapNode.Value) + 1;
            }
            return maxMapId;
        }

        /// <summary>
        /// Returns the maximum layer id
        /// </summary>
        /// <returns>The maximum layer id</returns>
        private int GetMaxLayerId()
        {
            int maxLayerId = 0;
            XmlNode layersNode = GetLayersNode();
            XmlNode layerNode = layersNode.SelectSingleNode(@"//Layer[not(../Layer/@LayerId > @LayerId)]/@LayerId");

            if (layerNode == null || layerNode.Value.Trim().Equals(string.Empty))
            {
                maxLayerId = 0;
            }
            else if (!layerNode.Value.Equals(string.Empty))
            {
                maxLayerId = int.Parse(layerNode.Value) + 1;
            }
            return maxLayerId;
        }

        /// <summary>
        /// Returns the latest grid column Id inserted.
        /// </summary>
        /// <param name="gridFieldId"></param>
        /// <returns></returns>
        public int GetMaxGridColumnId(int gridFieldId)
        {
            Query selectQuery = db.CreateQuery("select MAX(GridColumnId) from metaGridColumns where FieldId = @gridFieldId");
            selectQuery.Parameters.Add(new QueryParameter("@gridFieldId", DbType.Int32, gridFieldId));
            return (int)db.ExecuteScalar(selectQuery);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="viewId"></param>
        /// <returns></returns>
        public double GetMaxTabIndex(int pageId, int viewId, bool? includeReadOnly = null)
        {
            double maxTabIndex = 0;
            XmlNode viewsNode = GetViewsNode();
            XmlNode viewNode = viewsNode.SelectSingleNode("//View[@ViewId = '" + viewId + "']");
            XmlNode fieldsNode = viewNode.SelectSingleNode("//Fields");
            XmlNodeList fieldNodeList = fieldsNode.SelectNodes(@"//Field[@PageId = '" + pageId + "']");

            foreach (XmlNode fieldNode in fieldNodeList)
            {
                XmlNode node = fieldNode.SelectSingleNode(@"//Field[not(../Field/@TabIndex > @TabIndex)]/@TabIndex");
                if (node != null)
                {
                    maxTabIndex = double.Parse(node.Value);
                    break;
                }
            }
            return maxTabIndex;
        }

        /// <summary>
        /// Gets the schema for metaFields
        /// </summary>
        /// <param name="viewId">Id of the <see cref="Epi.View"/></param>
        /// <returns>string</returns>
        public DataTable GetMetaFieldsSchema(int viewId)
        {
            throw new System.ApplicationException("Not implemented");
        }

        /// <summary>
        /// Gets the minimum tab index of data fields
        /// </summary>
        /// <param name="pageId">The page id</param>
        /// <param name="viewId">The view id</param>
        /// <returns>The minimum tab index; -1 is none exists</returns>
        public double GetMinTabIndex(int pageId, int viewId)
        {
            double minTabIndex;
            XmlDocument doc = this.Project.GetXmlDocument();

            //XmlNode fieldsNode = GetFieldsNode();            
            XmlNode node = doc.SelectSingleNode(@"//Field[not(../Field/@FieldId < @FieldId)]/@FieldId");

            if (node.Value.Trim() == string.Empty || node == null)
            {
                minTabIndex = -1;
            }
            else
            {
                minTabIndex = double.Parse(node.Value.ToString());
            }

            return minTabIndex;
        }

        /// <summary>
        /// Returns the next tab index of a control for a view's page
        /// </summary>
        /// <param name="page">The view's page</param>
        /// <param name="view">The view</param>
        /// <param name="currentTabIndex">The current control's tab index</param>
        /// <returns></returns>
        public double GetNextTabIndex(Page page, View view, double currentTabIndex)
        {
            double nextTabIndex = 0;
            XmlNodeList fieldNodeList = GetFieldsNode(view.ViewElement).SelectNodes(@"//Field[not(@FieldTypeId = 'Label') and not (@FieldTypeId = 'Mirror') and not (@FieldTypeId = 'Group') and not (@FieldTypeId = 'RecStatus') and not (@FieldTypeId = 'UniqueKey')]");

            foreach (XmlNode node in fieldNodeList)
            {
                XmlNode fieldNode = node.SelectSingleNode(@"//Field[@TabIndex > '" + currentTabIndex + "']/@TabIndex");
                if (fieldNode != null || (!fieldNode.Value.Trim().Equals(string.Empty)))
                {
                    nextTabIndex = double.Parse(fieldNode.Value);
                    break;
                }
                else
                {
                    nextTabIndex = -1;
                }
            }
            return nextTabIndex;
        }

        /// <summary>
        /// Returns the max page position of a view
        /// </summary>
        /// <param name="viewId">The view id</param>
        /// <returns>The maximum page position; -1 is none exists</returns>
        public int GetMaxViewPagesPosition(int viewId)
        {
            int maxViewPagePosition = 0;
            XmlNode viewsNode = GetViewsNode();
            XmlNode viewNode = viewsNode.SelectSingleNode("//View[@ViewId = '" + viewId + "']");
            XmlNode pagesNode = viewsNode.SelectSingleNode("//Pages");
            XmlNode pageNode = pagesNode.SelectSingleNode(@"//Page[not(../Page/@Position > @Position)]/@Position");

            if (pageNode == null || pageNode.Value.Trim().Equals(string.Empty))
            {
                maxViewPagePosition = -1;
            }
            else if (!pageNode.Value.Equals(string.Empty))
            {
                maxViewPagePosition = int.Parse(pageNode.Value);
            }
            return maxViewPagePosition;
        }

        #endregion Utility Methods

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="viewName"></param>
        ///// <returns></returns>
        //public virtual List<string> GetTableColumnNames(string viewName)
        //{
        //    throw new NotImplementedException("Not implemented");
        //}

        /// <summary>
        /// Gets a code table by name
        /// </summary>
        /// <param name="tableName">Name of code table</param>
        /// <returns>Contents of the code table</returns>
        public virtual DataTable GetCodeTable(string tableName)
        {
            throw new System.ApplicationException("Not implemented");
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

        #endregion Public Methods

        #region Static Methods
        /// <summary>
        /// Check if this db is Epi 7 Metadata
        /// </summary>
        /// <param name="db">IDbDriver</param>
        /// <returns>Boolean</returns>
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
        /// Saves XML metadata
        /// </summary>
        private void Save()
        {
            Project.Save();
        }

        /// <summary>
        /// Returns the XML document
        /// </summary>
        /// <returns></returns>
        private XmlDocument GetXmlDocument()
        {
            return project.GetXmlDocument();
        }

        /// <summary>
        /// Gets the root node of XML metadata
        /// </summary>
        /// <returns></returns>
        private XmlNode GetRootNode()
        {
            return project.GetMetadataNode();
        }

        /// <summary>
        /// Returns the Views node
        /// </summary>
        /// <returns>The views node of the Xml metadata file</returns>
        private XmlNode GetViewsNode()
        {
            return GetRootNode().SelectSingleNode("Views");
        }

        /// <summary>
        /// Returns the Maps node
        /// </summary>
        /// <returns>The maps node of the Xml metadata file</returns>
        private XmlNode GetMapsNode()
        {
            return GetRootNode().SelectSingleNode("Maps");
        }

        /// <summary>
        /// Returns the Maps' Layers node
        /// </summary>
        /// <returns>The layers node of the Xml metadata file</returns>
        private XmlNode GetLayersNode()
        {
            return GetRootNode().SelectSingleNode("Layers");
        }


        /// <summary>
        /// Returns the Pages node
        /// </summary>
        /// <param name="viewElement">The view element</param>
        /// <returns>The node for pages</returns>
        private XmlNode GetPagesNode(XmlElement viewElement)
        {
            //return GetRootNode().SelectSingleNode("Pages");
            return viewElement.SelectSingleNode("Pages");
        }

        /// <summary>
        /// Returns the Fields node of the xml metadata project file document
        /// </summary>
        /// <param name="viewElement">The view element</param>
        /// <returns>The node for fields</returns>
        public XmlNode GetFieldsNode(XmlElement viewElement)
        {
            return viewElement.SelectSingleNode("Fields");
        }

        private void RaiseProgressReportBeginEvent(int min, int max, int step)
        {
            if (this.ProgressReportBeginEvent != null)
            {
                this.ProgressReportBeginEvent(min, max, step);
            }
        }

        private void RaiseProgressReportUpdateEvent()
        {
            if (this.ProgressReportUpdateEvent != null)
            {
                this.ProgressReportUpdateEvent();
            }
        }

        private void RaiseProgressReportEndEvent()
        {
            if (this.ProgressReportEndEvent != null)
            {
                this.ProgressReportEndEvent();
            }
        }

        /// <summary>
        /// Creates the project metadata tables
        /// </summary>
        private void CreateTables()
        {
            //try
            //{
            //    string[] SqlLine;
            //    Regex regex = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            //    string txtSQL = ResourceLoader.GetMetaDataTableScripts();
            //    SqlLine = regex.Split(txtSQL);


            //    foreach (string line in SqlLine)
            //    {
            //        if (line.Length > 0)
            //        {
            //            Query query = db.CreateQuery(line);
            //            db.ExecuteNonQuery(query);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw new GeneralException("Could not create project tables", ex);
            //}
            //finally
            //{

            //}

            CreateDbInfoTable();
            CreateProgramsTable();
            CreateDataTypesTable();
            CreatePatternsTable();
            CreateFieldTypesTable();
            CreateViewsTable();
            CreatePagesTable();
            CreateFieldsTable();
            CreateGridColumnsTable();
            CreateLayerRenderTypesTable();
            CreateLayersTable();
            CreateMapsTable();
            CreateMapLayersTable();
            CreateMapPointsTable();
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
            columns.Add(new TableColumn("DataTypeId", GenericDbColumnType.Int32, false, "metaDataTypes", "DataTypeId"));
            db.CreateTable("metaFieldTypes", columns);
        }

        private void CreateViewsTable()
        {
            List<TableColumn> columns = new List<TableColumn>();
            columns.Add(new TableColumn("ViewId", GenericDbColumnType.Int32, false, true, true));
            columns.Add(new TableColumn("Name", GenericDbColumnType.String, 64, false));
            columns.Add(new TableColumn("IsRelatedView", GenericDbColumnType.Boolean, false));
            columns.Add(new TableColumn("CheckCodeBefore", GenericDbColumnType.StringLong, true));
            columns.Add(new TableColumn("CheckCodeAfter", GenericDbColumnType.StringLong, true));
            columns.Add(new TableColumn("RecordCheckCodeBefore", GenericDbColumnType.StringLong, true));
            columns.Add(new TableColumn("RecordCheckCodeAfter", GenericDbColumnType.StringLong, true));
            columns.Add(new TableColumn("CheckCodeVariableDefinitions", GenericDbColumnType.StringLong, true));
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
            columns.Add(new TableColumn("ViewId", GenericDbColumnType.Int32, false, "metaViews", "ViewId"));
            columns.Add(new TableColumn("Width", GenericDbColumnType.Int32, true, false));
            columns.Add(new TableColumn("Height", GenericDbColumnType.Int32, true, false));
            columns.Add(new TableColumn("Orientation", GenericDbColumnType.String, true, false));
            columns.Add(new TableColumn("LabelAlign", GenericDbColumnType.String, true, false));
            columns.Add(new TableColumn("TargetMedium", GenericDbColumnType.String, true, false));
            db.CreateTable("metaPages", columns);
        }

        private void CreateFieldsTable()
        {
            List<TableColumn> columns = new List<TableColumn>();
            columns.Add(new TableColumn("FieldId", GenericDbColumnType.Int32, false, true, true));
            columns.Add(new TableColumn("Name", GenericDbColumnType.String, 64, false));
            columns.Add(new TableColumn("PromptText", GenericDbColumnType.String, 255, true));
            columns.Add(new TableColumn("ControlFontFamily", GenericDbColumnType.String, 20, true));
            columns.Add(new TableColumn("ControlFontStyle", GenericDbColumnType.String, 50, true));
            columns.Add(new TableColumn("ControlFontSize", GenericDbColumnType.Decimal, 2, true));
            columns.Add(new TableColumn("ControlTopPositionPercentage", GenericDbColumnType.Double, true));
            columns.Add(new TableColumn("ControlLeftPositionPercentage", GenericDbColumnType.Double, true));
            columns.Add(new TableColumn("ControlHeightPercentage", GenericDbColumnType.Double, true));
            columns.Add(new TableColumn("ControlWidthPercentage", GenericDbColumnType.Double, true));
            columns.Add(new TableColumn("TabIndex", GenericDbColumnType.Double, true));
            columns.Add(new TableColumn("HasTabStop", GenericDbColumnType.Boolean, true));
            columns.Add(new TableColumn("PromptFontFamily", GenericDbColumnType.String, 20, true));
            columns.Add(new TableColumn("PromptFontStyle", GenericDbColumnType.String, 50, true));
            columns.Add(new TableColumn("PromptFontSize", GenericDbColumnType.Decimal, 2, true));
            columns.Add(new TableColumn("PromptScriptName", GenericDbColumnType.String, 20, true));
            columns.Add(new TableColumn("PromptTopPositionPercentage", GenericDbColumnType.Double, true));
            columns.Add(new TableColumn("PromptLeftPositionPercentage", GenericDbColumnType.Double, true));
            columns.Add(new TableColumn("ControlScriptName", GenericDbColumnType.String, 20, true));
            columns.Add(new TableColumn("ShouldRepeatLast", GenericDbColumnType.Boolean, true));
            columns.Add(new TableColumn("IsRequired", GenericDbColumnType.Boolean, true));
            columns.Add(new TableColumn("IsReadOnly", GenericDbColumnType.Boolean, true));
            columns.Add(new TableColumn("ShouldRetainImageSize", GenericDbColumnType.Boolean, true));
            columns.Add(new TableColumn("MaxLength", GenericDbColumnType.Int16, true));
            columns.Add(new TableColumn("Lower", GenericDbColumnType.String, 20, true));
            columns.Add(new TableColumn("Upper", GenericDbColumnType.String, 20, true));
            columns.Add(new TableColumn("Pattern", GenericDbColumnType.String, 50, true));
            columns.Add(new TableColumn("ShowTextOnRight", GenericDbColumnType.Boolean, true));
            columns.Add(new TableColumn("CheckCodeBefore", GenericDbColumnType.StringLong, true));
            columns.Add(new TableColumn("CheckCodeAfter", GenericDbColumnType.StringLong, true));
            columns.Add(new TableColumn("RelateCondition", GenericDbColumnType.String, 255, true));
            columns.Add(new TableColumn("ShouldReturnToParent", GenericDbColumnType.Boolean, true));
            columns.Add(new TableColumn("SourceTableName", GenericDbColumnType.String, 50, true));
            columns.Add(new TableColumn("CodeColumnName", GenericDbColumnType.String, 50, true));
            columns.Add(new TableColumn("TextColumnName", GenericDbColumnType.String, 50, true));
            columns.Add(new TableColumn("Sort", GenericDbColumnType.Boolean, true));
            columns.Add(new TableColumn("IsExclusiveTable", GenericDbColumnType.Boolean, true));
            columns.Add(new TableColumn("DataTableName", GenericDbColumnType.String, 64, true));
            columns.Add(new TableColumn("SourceFieldId", GenericDbColumnType.Int32, true));
            columns.Add(new TableColumn("FieldTypeId", GenericDbColumnType.Int32, false, "metaFieldTypes", "FieldTypeId"));
            columns.Add(new TableColumn("RelatedViewId", GenericDbColumnType.Int32, true, "metaViews", "ViewId"));
            columns.Add(new TableColumn("ViewId", GenericDbColumnType.Int32, false, "metaViews", "ViewId"));
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
            columns.Add(new TableColumn("FieldId", GenericDbColumnType.Int32, false, "metaFields", "FieldId", true));
            columns.Add(new TableColumn("FieldTypeId", GenericDbColumnType.Int32, false, "metaFieldTypes", "FieldTypeId"));
            db.CreateTable("metaGridColumns", columns);
        }

        /// <summary>
        /// Populates the project metadata tables
        /// </summary>
        private void PopulateTables()
        {
            try
            {

                foreach (AppDataSet.DataTypesRow dataType in AppData.Instance.DataTypesDataTable.Rows)
                {
                    Query dataTypeSelectExists = db.CreateQuery("SELECT 1 FROM metaDataTypes WHERE [DataTypeId] = @DataTypeId AND [Name] = @Name");
                    dataTypeSelectExists.Parameters.Add(new QueryParameter("@DataTypeId", DbType.Int32, dataType.DataTypeId));
                    dataTypeSelectExists.Parameters.Add(new QueryParameter("@Name", DbType.String, dataType.Name));

                    IDataReader reader = db.ExecuteReader(dataTypeSelectExists);

                    if (!reader.Read())
                    {
                        Query dataTypeInsert = db.CreateQuery("insert into metaDataTypes ([DataTypeId], [Name], [HasPattern], [HasSize], [HasRange]) values (@DataTypeId, @Name, @HasPattern, @HasSize, @HasRange)");
                        dataTypeInsert.Parameters.Add(new QueryParameter("@DataTypeId", DbType.Int32, dataType.DataTypeId));
                        dataTypeInsert.Parameters.Add(new QueryParameter("@Name", DbType.String, dataType.Name));
                        dataTypeInsert.Parameters.Add(new QueryParameter("@HasPattern", DbType.Boolean, dataType.HasPattern));
                        dataTypeInsert.Parameters.Add(new QueryParameter("@HasSize", DbType.Boolean, dataType.HasSize));
                        dataTypeInsert.Parameters.Add(new QueryParameter("@HasRange", DbType.Boolean, dataType.HasRange));

                        db.ExecuteNonQuery(dataTypeInsert);
                    }
                }
                foreach (AppDataSet.FieldTypesRow fieldType in AppData.Instance.FieldTypesDataTable.Rows)
                {
                    Query fieldaTypeSelectExists = db.CreateQuery("SELECT 1 FROM metaFieldTypes WHERE [FieldTypeId] = @FieldTypeId AND [Name] = @Name");
                    fieldaTypeSelectExists.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, fieldType.FieldTypeId));
                    fieldaTypeSelectExists.Parameters.Add(new QueryParameter("@Name", DbType.String, fieldType.Name));

                    IDataReader reader = db.ExecuteReader(fieldaTypeSelectExists);

                    if (!reader.Read())
                    {
                        Query fieldTypeInsert = db.CreateQuery("insert into metaFieldTypes ([FieldTypeId], [Name], [HasRepeatLast], [HasRequired], [HasReadOnly], [HasRetainImageSize], [HasFont], [IsDropDown], [IsGridColumn], [DataTypeId], [IsSystem], [DefaultPatternId]) values (@FieldTypeId, @Name, @HasRepeatLast, @HasRequired, @HasReadOnly, @HasRetainImageSize, @HasFont, @IsDropDown, @IsGridColumn, @DataTypeId, @IsSystem, @DefaultPatternId)");
                        fieldTypeInsert.Parameters.Add(new QueryParameter("@FieldTypeId", DbType.Int32, fieldType.FieldTypeId));
                        fieldTypeInsert.Parameters.Add(new QueryParameter("@Name", DbType.String, fieldType.Name));
                        fieldTypeInsert.Parameters.Add(new QueryParameter("@HasRepeatLast", DbType.Boolean, fieldType.HasRepeatLast));
                        fieldTypeInsert.Parameters.Add(new QueryParameter("@HasRequired", DbType.Boolean, fieldType.HasRequired));
                        fieldTypeInsert.Parameters.Add(new QueryParameter("@HasReadOnly", DbType.Boolean, fieldType.HasReadOnly));
                        fieldTypeInsert.Parameters.Add(new QueryParameter("@HasRetainImageSize", DbType.Boolean, fieldType.HasRetainImageSize));
                        fieldTypeInsert.Parameters.Add(new QueryParameter("@HasFont", DbType.Boolean, fieldType.HasFont));
                        fieldTypeInsert.Parameters.Add(new QueryParameter("@IsDropDown", DbType.Boolean, fieldType.IsDropDown));
                        fieldTypeInsert.Parameters.Add(new QueryParameter("@IsGridColumn", DbType.Boolean, fieldType.IsGridColumn));
                        if (fieldType.IsDataTypeIdNull())
                            fieldTypeInsert.Parameters.Add(new QueryParameter("@DataTypeId", DbType.Int32, DBNull.Value));
                        else
                            fieldTypeInsert.Parameters.Add(new QueryParameter("@DataTypeId", DbType.Int32, fieldType.DataTypeId));
                        fieldTypeInsert.Parameters.Add(new QueryParameter("@IsSystem", DbType.Boolean, fieldType.IsSystem));
                        fieldTypeInsert.Parameters.Add(new QueryParameter("@DefaultPatternId", DbType.Int32, fieldType.DefaultPatternId));

                        db.ExecuteNonQuery(fieldTypeInsert);
                    }
                }
                foreach (AppDataSet.DataPatternsRow pattern in AppData.Instance.DataPatternsDataTable.Rows)
                {
                    Query patternSelectExists = db.CreateQuery("SELECT 1 FROM metaPatterns WHERE [PatternId] = @PatternId AND [DataTypeId] = @DataTypeId");
                    patternSelectExists.Parameters.Add(new QueryParameter("@PatternId", DbType.Int32, pattern.PatternId));
                    patternSelectExists.Parameters.Add(new QueryParameter("@DataTypeId", DbType.Int32, pattern.PatternId));

                    IDataReader reader = db.ExecuteReader(patternSelectExists);

                    if (!reader.Read())
                    {
                        Query patternInsert = db.CreateQuery("insert into metaPatterns ([PatternId], [DataTypeId], [Expression], [Mask], [FormattedExpression]) values (@PatternId, @DataTypeId, @Expression, @Mask, @FormattedExpression)");
                        patternInsert.Parameters.Add(new QueryParameter("@PatternId", DbType.Int32, pattern.PatternId));
                        patternInsert.Parameters.Add(new QueryParameter("@DataTypeId", DbType.Int32, pattern.DataTypeId));
                        patternInsert.Parameters.Add(new QueryParameter("@Expression", DbType.String, pattern.Expression));
                        patternInsert.Parameters.Add(new QueryParameter("@Mask", DbType.String, pattern.Mask));
                        patternInsert.Parameters.Add(new QueryParameter("@FormattedExpression", DbType.String, pattern.FormattedExpression));

                        db.ExecuteNonQuery(patternInsert);
                    }
                }
                foreach (AppDataSet.LayerRenderTypesRow layerRenderType in AppData.Instance.LayerRenderTypesDataTable.Rows)
                {
                    Query layerRenderSelectExists = db.CreateQuery("SELECT 1 FROM metaLayerRenderTypes WHERE [LayerRenderTypeId] = @LayerRenderTypeId AND [Name] = @Name");
                    layerRenderSelectExists.Parameters.Add(new QueryParameter("@LayerRenderTypeId", DbType.Int32, layerRenderType.LayerRenderTypeId));
                    layerRenderSelectExists.Parameters.Add(new QueryParameter("@Name", DbType.String, layerRenderType.Name));

                    IDataReader reader = db.ExecuteReader(layerRenderSelectExists);

                    if (!reader.Read())
                    {
                        Query layerRenderInsert = db.CreateQuery("insert into metaLayerRenderTypes ([LayerRenderTypeId], [Name]) values (@LayerRenderTypeId, @Name)");
                        layerRenderInsert.Parameters.Add(new QueryParameter("@LayerRenderTypeId", DbType.Int32, layerRenderType.LayerRenderTypeId));
                        layerRenderInsert.Parameters.Add(new QueryParameter("@Name", DbType.String, layerRenderType.Name));
                        db.ExecuteNonQuery(layerRenderInsert);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not populate project tables", ex);
            }
            finally
            {

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

                if (Project.UseMetadataDbForCollectedData)
                    query.Parameters.Add(new QueryParameter("@Purpose", DbType.Int32, (int)DatabasePurpose.MetadataAndCollectedData));
                else
                    query.Parameters.Add(new QueryParameter("@Purpose", DbType.Int32, (int)DatabasePurpose.Metadata));


                db.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create new project", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Extract prompt font from field's data row.
        /// </summary>
        /// <param name="fieldRow">Field row in a DataTable.</param>
        /// <returns>Font family, size, and style.</returns>
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

        /// <summary>
        /// Creates metadata tables
        /// </summary>
        protected void CreateMetadataTables()
        {
            CreateTables();
            PopulateTables();
            InsertProject();
        }

        /// <summary>
        /// Retrieves data for check box field from xml metadata
        /// </summary>
        /// <param name="field">A check box field</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(CheckBoxField field, XmlNode fieldNode)
        {
            field.Id = Int32.Parse(fieldNode.Attributes["FieldId"].Value);
            field.Name = fieldNode.Attributes["Name"].Value;
            field.PromptText = fieldNode.Attributes["PromptText"].Value;
            //field.ControlFont = System.Drawing.Font(fieldNode.Attributes["ControlFontFamily"].Value);
            //field.ControlFont.Style = fieldNode.Attributes["ControlFontStyle"].Value;
            //field.ControlFont.Size = float.Parse(fieldNode.Attributes["ControlFontSize"].Value);
            field.ControlTopPositionPercentage = Double.Parse(fieldNode.Attributes["ControlTopPositionPercentage"].Value);
            field.ControlLeftPositionPercentage = Double.Parse(fieldNode.Attributes["ControlLeftPositionPercentage"].Value);
            field.ControlHeightPercentage = Double.Parse(fieldNode.Attributes["ControlHeightPercentage"].Value);
            field.ControlWidthPercentage = Double.Parse(fieldNode.Attributes["ControlWidthPercentage"].Value);
            field.TabIndex = Int32.Parse(fieldNode.Attributes["TabIndex"].Value);
            field.HasTabStop = bool.Parse(fieldNode.Attributes["HasTabStop"].Value);
            //field.PromptFont.FontFamily.Name = fieldNode.Attributes["PromptFontFamily"].Value;
            //field.PromptFont.Style = fieldNode.Attributes["PromptFontStyle"].Value;
            //field.PromptFont.Size = fieldNode.Attributes["PromptFontSize"].Value;
            //field.PromptFont.Name = fieldNode.Attributes["PromptScriptName"].Value;               
            //field.ControlFont.Name = fieldNode.Attributes["ControlScriptName"].Value;                                                
            //field.IsControlResizable = bool.Parse(fieldNode.Attributes["ShouldRetainImageSize"].Value);
            field.ShouldRepeatLast = bool.Parse(fieldNode.Attributes["ShouldRepeatLast"].Value);
            field.IsRequired = bool.Parse(fieldNode.Attributes["IsRequired"].Value);
            field.IsReadOnly = bool.Parse(fieldNode.Attributes["IsReadOnly"].Value);
            field.TableName = fieldNode.Attributes["DataTableName"].Value;
            //field.CheckCodeAfter = fieldNode.Attributes["CheckCodeAfter"].Value.ToString();
            //field.CheckCodeBefore = fieldNode.Attributes["CheckCodeBefore"].Value.ToString();
            //field. = fieldNode.Attributes["FieldTypeId"].Value;

            field.Page = new Page(field.GetView());
            //field.Page.Name = 
            field.Page.Id = int.Parse(fieldNode.Attributes["PageId"].Value);
        }

        /// <summary>
        /// Retrieves data for command button field from xml metadata
        /// </summary>
        /// <param name="field">A command button field</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(CommandButtonField field, XmlNode fieldNode)
        {
            field.Id = Int32.Parse(fieldNode.Attributes["FieldId"].Value);
            field.Name = fieldNode.Attributes["Name"].Value;
            field.PromptText = fieldNode.Attributes["PromptText"].Value;
            //field.ControlFont = System.Drawing.Font(fieldNode.Attributes["ControlFontFamily"].Value);
            //field.ControlFont.Style = fieldNode.Attributes["ControlFontStyle"].Value;
            //field.ControlFont.Size = float.Parse(fieldNode.Attributes["ControlFontSize"].Value);
            field.ControlTopPositionPercentage = Double.Parse(fieldNode.Attributes["ControlTopPositionPercentage"].Value);
            field.ControlLeftPositionPercentage = Double.Parse(fieldNode.Attributes["ControlLeftPositionPercentage"].Value);
            field.ControlHeightPercentage = Double.Parse(fieldNode.Attributes["ControlHeightPercentage"].Value);
            field.ControlWidthPercentage = Double.Parse(fieldNode.Attributes["ControlWidthPercentage"].Value);
            field.TabIndex = Int32.Parse(fieldNode.Attributes["TabIndex"].Value);
            field.HasTabStop = bool.Parse(fieldNode.Attributes["HasTabStop"].Value);
            //field.PromptFont.FontFamily.Name = fieldNode.Attributes["PromptFontFamily"].Value;
            //field.PromptFont.Style = fieldNode.Attributes["PromptFontStyle"].Value;
            //field.PromptFont.Size = fieldNode.Attributes["PromptFontSize"].Value;
            //field.PromptFont.Name = fieldNode.Attributes["PromptScriptName"].Value;               
            //field.ControlFont.Name = fieldNode.Attributes["ControlScriptName"].Value;
            //field.IsControlResizable = bool.Parse(fieldNode.Attributes["ShouldRetainImageSize"].Value);
            //field.TableName = fieldNode.Attributes["DataTableName"].Value;                
            //field. = fieldNode.Attributes["FieldTypeId"].Value;

            field.Page = new Page(field.GetView());
            //field.Page.Name = 
            field.Page.Id = int.Parse(fieldNode.Attributes["PageId"].Value);
        }

        /// <summary>
        /// Retrieves data for date field from xml metadata
        /// </summary>
        /// <param name="field">A date field </param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(DateField field, XmlNode fieldNode)
        {
            field.Id = Int32.Parse(fieldNode.Attributes["FieldId"].Value);
            field.Name = fieldNode.Attributes["Name"].Value;
            field.PromptText = fieldNode.Attributes["PromptText"].Value;
            //field.ControlFont = System.Drawing.Font(fieldNode.Attributes["ControlFontFamily"].Value);
            //field.ControlFont.Style = fieldNode.Attributes["ControlFontStyle"].Value;
            //field.ControlFont.Size = float.Parse(fieldNode.Attributes["ControlFontSize"].Value);
            field.ControlTopPositionPercentage = Double.Parse(fieldNode.Attributes["ControlTopPositionPercentage"].Value);
            field.ControlLeftPositionPercentage = Double.Parse(fieldNode.Attributes["ControlLeftPositionPercentage"].Value);
            field.ControlHeightPercentage = Double.Parse(fieldNode.Attributes["ControlHeightPercentage"].Value);
            field.ControlWidthPercentage = Double.Parse(fieldNode.Attributes["ControlWidthPercentage"].Value);
            field.TabIndex = Int32.Parse(fieldNode.Attributes["TabIndex"].Value);
            field.HasTabStop = bool.Parse(fieldNode.Attributes["HasTabStop"].Value);
            //field.PromptFont.FontFamily.Name = fieldNode.Attributes["PromptFontFamily"].Value;
            //field.PromptFont.Style = fieldNode.Attributes["PromptFontStyle"].Value;
            //field.PromptFont.Size = fieldNode.Attributes["PromptFontSize"].Value;
            //field.PromptFont.Name = fieldNode.Attributes["PromptScriptName"].Value;
            field.PromptTopPositionPercentage = Double.Parse(fieldNode.Attributes["PromptTopPositionPercentage"].Value);
            field.PromptLeftPositionPercentage = Double.Parse(fieldNode.Attributes["PromptLeftPositionPercentage"].Value);
            //field.ControlFont.Name = fieldNode.Attributes["ControlScriptName"].Value;
            field.ShouldRepeatLast = bool.Parse(fieldNode.Attributes["ShouldRepeatLast"].Value);
            field.IsRequired = bool.Parse(fieldNode.Attributes["IsRequired"].Value);
            field.IsReadOnly = bool.Parse(fieldNode.Attributes["IsReadOnly"].Value);
            //field.IsControlResizable = bool.Parse(fieldNode.Attributes["ShouldRetainImageSize"].Value);
            field.Lower = fieldNode.Attributes["Lower"].Value;
            field.Upper = fieldNode.Attributes["Upper"].Value;
            field.TableName = fieldNode.Attributes["DataTableName"].Value;

            //field. = fieldNode.Attributes["FieldTypeId"].Value;

            field.Page = new Page(field.GetView());
            //field.Page.Name = 
            field.Page.Id = int.Parse(fieldNode.Attributes["PageId"].Value);

            //field.CheckCodeAfter = fieldNode.Attributes["CheckCodeAfter"].Value.ToString();
            //field.CheckCodeBefore = fieldNode.Attributes["CheckCodeBefore"].Value.ToString();
        }

        /// <summary>
        /// Retrieves data for date time field from xml metadata
        /// </summary>
        /// <param name="field">A date time field</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(DateTimeField field, XmlNode fieldNode)
        {
            field.Id = Int32.Parse(fieldNode.Attributes["FieldId"].Value);
            field.Name = fieldNode.Attributes["Name"].Value;
            field.PromptText = fieldNode.Attributes["PromptText"].Value;
            //field.ControlFont = System.Drawing.Font(fieldNode.Attributes["ControlFontFamily"].Value);
            //field.ControlFont.Style = fieldNode.Attributes["ControlFontStyle"].Value;
            //field.ControlFont.Size = float.Parse(fieldNode.Attributes["ControlFontSize"].Value);
            field.ControlTopPositionPercentage = Double.Parse(fieldNode.Attributes["ControlTopPositionPercentage"].Value);
            field.ControlLeftPositionPercentage = Double.Parse(fieldNode.Attributes["ControlLeftPositionPercentage"].Value);
            field.ControlHeightPercentage = Double.Parse(fieldNode.Attributes["ControlHeightPercentage"].Value);
            field.ControlWidthPercentage = Double.Parse(fieldNode.Attributes["ControlWidthPercentage"].Value);
            field.TabIndex = Int32.Parse(fieldNode.Attributes["TabIndex"].Value);
            field.HasTabStop = bool.Parse(fieldNode.Attributes["HasTabStop"].Value);
            //field.PromptFont.FontFamily.Name = fieldNode.Attributes["PromptFontFamily"].Value;
            //field.PromptFont.Style = fieldNode.Attributes["PromptFontStyle"].Value;
            //field.PromptFont.Size = fieldNode.Attributes["PromptFontSize"].Value;
            //field.PromptFont.Name = fieldNode.Attributes["PromptScriptName"].Value;
            field.PromptTopPositionPercentage = Double.Parse(fieldNode.Attributes["PromptTopPositionPercentage"].Value);
            field.PromptLeftPositionPercentage = Double.Parse(fieldNode.Attributes["PromptLeftPositionPercentage"].Value);
            //field.ControlFont.Name = fieldNode.Attributes["ControlScriptName"].Value;
            field.ShouldRepeatLast = bool.Parse(fieldNode.Attributes["ShouldRepeatLast"].Value);
            field.IsRequired = bool.Parse(fieldNode.Attributes["IsRequired"].Value);
            field.IsReadOnly = bool.Parse(fieldNode.Attributes["IsReadOnly"].Value);
            //field.IsControlResizable = bool.Parse(fieldNode.Attributes["ShouldRetainImageSize"].Value);
            field.TableName = fieldNode.Attributes["DataTableName"].Value;

            //field. = fieldNode.Attributes["FieldTypeId"].Value;

            field.Page = new Page(field.GetView());
            //field.Page.Name = 
            field.Page.Id = int.Parse(fieldNode.Attributes["PageId"].Value);

            //field.CheckCodeAfter = fieldNode.Attributes["CheckCodeAfter"].Value.ToString();
            //field.CheckCodeBefore = fieldNode.Attributes["CheckCodeBefore"].Value.ToString();
        }

        /// <summary>
        /// Retrieves data for DDL Field of Codes from xml metadata
        /// </summary>
        /// <param name="field">A DDL Field of Codes</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(DDLFieldOfCodes field, XmlNode fieldNode)
        {
            field.Id = Int32.Parse(fieldNode.Attributes["FieldId"].Value);
            field.Name = fieldNode.Attributes["Name"].Value;
            field.PromptText = fieldNode.Attributes["PromptText"].Value;
            //field.ControlFont = System.Drawing.Font(fieldNode.Attributes["ControlFontFamily"].Value);
            //field.ControlFont.Style = fieldNode.Attributes["ControlFontStyle"].Value;
            //field.ControlFont.Size = float.Parse(fieldNode.Attributes["ControlFontSize"].Value);
            field.ControlTopPositionPercentage = Double.Parse(fieldNode.Attributes["ControlTopPositionPercentage"].Value);
            field.ControlLeftPositionPercentage = Double.Parse(fieldNode.Attributes["ControlLeftPositionPercentage"].Value);
            field.ControlHeightPercentage = Double.Parse(fieldNode.Attributes["ControlHeightPercentage"].Value);
            field.ControlWidthPercentage = Double.Parse(fieldNode.Attributes["ControlWidthPercentage"].Value);
            field.TabIndex = Int32.Parse(fieldNode.Attributes["TabIndex"].Value);
            field.HasTabStop = bool.Parse(fieldNode.Attributes["HasTabStop"].Value);
            //field.PromptFont.FontFamily.Name = fieldNode.Attributes["PromptFontFamily"].Value;
            //field.PromptFont.Style = fieldNode.Attributes["PromptFontStyle"].Value;
            //field.PromptFont.Size = fieldNode.Attributes["PromptFontSize"].Value;
            //field.PromptFont.Name = fieldNode.Attributes["PromptScriptName"].Value;
            field.PromptTopPositionPercentage = Double.Parse(fieldNode.Attributes["PromptTopPositionPercentage"].Value);
            field.PromptLeftPositionPercentage = Double.Parse(fieldNode.Attributes["PromptLeftPositionPercentage"].Value);
            //field.ControlFont.Name = fieldNode.Attributes["ControlScriptName"].Value;
            field.ShouldRepeatLast = bool.Parse(fieldNode.Attributes["ShouldRepeatLast"].Value);
            field.IsRequired = bool.Parse(fieldNode.Attributes["IsRequired"].Value);
            field.IsReadOnly = bool.Parse(fieldNode.Attributes["IsReadOnly"].Value);
            //field.IsControlResizable = bool.Parse(fieldNode.Attributes["ShouldRetainImageSize"].Value);
            field.CodeColumnName = fieldNode.Attributes["CodeColumnName"].Value;
            field.IsExclusiveTable = bool.Parse(fieldNode.Attributes["IsExclusiveTable"].Value);
            field.ShouldSort = bool.Parse(fieldNode.Attributes["Sort"].Value);
            field.SourceTableName = fieldNode.Attributes["SourceTableName"].Value;
            field.TextColumnName = fieldNode.Attributes["TextColumnName"].Value;
            field.TableName = fieldNode.Attributes["DataTableName"].Value;

            //field. = fieldNode.Attributes["FieldTypeId"].Value;

            field.Page = new Page(field.GetView());
            //field.Page.Name = 
            field.Page.Id = int.Parse(fieldNode.Attributes["PageId"].Value);

            //field.CheckCodeAfter = fieldNode.Attributes["CheckCodeAfter"].Value.ToString();
            //field.CheckCodeBefore = fieldNode.Attributes["CheckCodeBefore"].Value.ToString();
        }


        /// <summary>
        /// Retrieves data for DDL Field of Codes from xml metadata
        /// </summary>
        /// <param name="field">A DDL Field of Codes</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(DDListField field, XmlNode fieldNode)
        {
            field.Id = Int32.Parse(fieldNode.Attributes["FieldId"].Value);
            field.Name = fieldNode.Attributes["Name"].Value;
            field.PromptText = fieldNode.Attributes["PromptText"].Value;
            //field.ControlFont = System.Drawing.Font(fieldNode.Attributes["ControlFontFamily"].Value);
            //field.ControlFont.Style = fieldNode.Attributes["ControlFontStyle"].Value;
            //field.ControlFont.Size = float.Parse(fieldNode.Attributes["ControlFontSize"].Value);
            field.ControlTopPositionPercentage = Double.Parse(fieldNode.Attributes["ControlTopPositionPercentage"].Value);
            field.ControlLeftPositionPercentage = Double.Parse(fieldNode.Attributes["ControlLeftPositionPercentage"].Value);
            field.ControlHeightPercentage = Double.Parse(fieldNode.Attributes["ControlHeightPercentage"].Value);
            field.ControlWidthPercentage = Double.Parse(fieldNode.Attributes["ControlWidthPercentage"].Value);
            field.TabIndex = Int32.Parse(fieldNode.Attributes["TabIndex"].Value);
            field.HasTabStop = bool.Parse(fieldNode.Attributes["HasTabStop"].Value);
            //field.PromptFont.FontFamily.Name = fieldNode.Attributes["PromptFontFamily"].Value;
            //field.PromptFont.Style = fieldNode.Attributes["PromptFontStyle"].Value;
            //field.PromptFont.Size = fieldNode.Attributes["PromptFontSize"].Value;
            //field.PromptFont.Name = fieldNode.Attributes["PromptScriptName"].Value;
            field.PromptTopPositionPercentage = Double.Parse(fieldNode.Attributes["PromptTopPositionPercentage"].Value);
            field.PromptLeftPositionPercentage = Double.Parse(fieldNode.Attributes["PromptLeftPositionPercentage"].Value);
            //field.ControlFont.Name = fieldNode.Attributes["ControlScriptName"].Value;
            field.ShouldRepeatLast = bool.Parse(fieldNode.Attributes["ShouldRepeatLast"].Value);
            field.IsRequired = bool.Parse(fieldNode.Attributes["IsRequired"].Value);
            field.IsReadOnly = bool.Parse(fieldNode.Attributes["IsReadOnly"].Value);
            //field.IsControlResizable = bool.Parse(fieldNode.Attributes["ShouldRetainImageSize"].Value);
            field.CodeColumnName = fieldNode.Attributes["CodeColumnName"].Value;
            field.IsExclusiveTable = bool.Parse(fieldNode.Attributes["IsExclusiveTable"].Value);
            field.ShouldSort = bool.Parse(fieldNode.Attributes["Sort"].Value);
            field.SourceTableName = fieldNode.Attributes["SourceTableName"].Value;
            field.TextColumnName = fieldNode.Attributes["TextColumnName"].Value;
            field.TableName = fieldNode.Attributes["DataTableName"].Value;

            //field. = fieldNode.Attributes["FieldTypeId"].Value;

            field.Page = new Page(field.GetView());
            //field.Page.Name = 
            field.Page.Id = int.Parse(fieldNode.Attributes["PageId"].Value);

            //field.CheckCodeAfter = fieldNode.Attributes["CheckCodeAfter"].Value.ToString();
            //field.CheckCodeBefore = fieldNode.Attributes["CheckCodeBefore"].Value.ToString();
        }


        /// <summary>
        /// Retrieves data for DDL Field of Comment Legal from xml metadata
        /// </summary>
        /// <param name="field">A DDL Field of Comment Legal</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(DDLFieldOfCommentLegal field, XmlNode fieldNode)
        {
            field.Id = Int32.Parse(fieldNode.Attributes["FieldId"].Value);
            field.Name = fieldNode.Attributes["Name"].Value;
            field.PromptText = fieldNode.Attributes["PromptText"].Value;
            //field.ControlFont = System.Drawing.Font(fieldNode.Attributes["ControlFontFamily"].Value);
            //field.ControlFont.Style = fieldNode.Attributes["ControlFontStyle"].Value;
            //field.ControlFont.Size = float.Parse(fieldNode.Attributes["ControlFontSize"].Value);
            field.ControlTopPositionPercentage = Double.Parse(fieldNode.Attributes["ControlTopPositionPercentage"].Value);
            field.ControlLeftPositionPercentage = Double.Parse(fieldNode.Attributes["ControlLeftPositionPercentage"].Value);
            field.ControlHeightPercentage = Double.Parse(fieldNode.Attributes["ControlHeightPercentage"].Value);
            field.ControlWidthPercentage = Double.Parse(fieldNode.Attributes["ControlWidthPercentage"].Value);
            field.TabIndex = Int32.Parse(fieldNode.Attributes["TabIndex"].Value);
            field.HasTabStop = bool.Parse(fieldNode.Attributes["HasTabStop"].Value);
            //field.PromptFont.FontFamily.Name = fieldNode.Attributes["PromptFontFamily"].Value;
            //field.PromptFont.Style = fieldNode.Attributes["PromptFontStyle"].Value;
            //field.PromptFont.Size = fieldNode.Attributes["PromptFontSize"].Value;
            //field.PromptFont.Name = fieldNode.Attributes["PromptScriptName"].Value;
            field.PromptTopPositionPercentage = Double.Parse(fieldNode.Attributes["PromptTopPositionPercentage"].Value);
            field.PromptLeftPositionPercentage = Double.Parse(fieldNode.Attributes["PromptLeftPositionPercentage"].Value);
            //field.ControlFont.Name = fieldNode.Attributes["ControlScriptName"].Value;
            field.ShouldRepeatLast = bool.Parse(fieldNode.Attributes["ShouldRepeatLast"].Value);
            field.IsRequired = bool.Parse(fieldNode.Attributes["IsRequired"].Value);
            field.IsReadOnly = bool.Parse(fieldNode.Attributes["IsReadOnly"].Value);
            //field.IsControlResizable = bool.Parse(fieldNode.Attributes["ShouldRetainImageSize"].Value);
            field.CodeColumnName = fieldNode.Attributes["CodeColumnName"].Value;
            field.IsExclusiveTable = bool.Parse(fieldNode.Attributes["IsExclusiveTable"].Value);
            field.ShouldSort = bool.Parse(fieldNode.Attributes["Sort"].Value);
            field.SourceTableName = fieldNode.Attributes["SourceTableName"].Value;
            field.TextColumnName = fieldNode.Attributes["TextColumnName"].Value;
            field.TableName = fieldNode.Attributes["DataTableName"].Value;

            //field. = fieldNode.Attributes["FieldTypeId"].Value;

            field.Page = new Page(field.GetView());
            //field.Page.Name = 
            field.Page.Id = int.Parse(fieldNode.Attributes["PageId"].Value);

            //field.CheckCodeAfter = fieldNode.Attributes["CheckCodeAfter"].Value.ToString();
            //field.CheckCodeBefore = fieldNode.Attributes["CheckCodeBefore"].Value.ToString();
        }

        /// <summary>
        /// Retrieves data for DDL Field of Legal Values from xml metadata
        /// </summary>
        /// <param name="field">A DDL Field of Legal Values</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(DDLFieldOfLegalValues field, XmlNode fieldNode)
        {
            field.Id = Int32.Parse(fieldNode.Attributes["FieldId"].Value);
            field.Name = fieldNode.Attributes["Name"].Value;
            field.PromptText = fieldNode.Attributes["PromptText"].Value;
            //field.ControlFont = System.Drawing.Font(fieldNode.Attributes["ControlFontFamily"].Value);
            //field.ControlFont.Style = fieldNode.Attributes["ControlFontStyle"].Value;
            //field.ControlFont.Size = float.Parse(fieldNode.Attributes["ControlFontSize"].Value);
            field.ControlTopPositionPercentage = Double.Parse(fieldNode.Attributes["ControlTopPositionPercentage"].Value);
            field.ControlLeftPositionPercentage = Double.Parse(fieldNode.Attributes["ControlLeftPositionPercentage"].Value);
            field.ControlHeightPercentage = Double.Parse(fieldNode.Attributes["ControlHeightPercentage"].Value);
            field.ControlWidthPercentage = Double.Parse(fieldNode.Attributes["ControlWidthPercentage"].Value);
            field.TabIndex = Int32.Parse(fieldNode.Attributes["TabIndex"].Value);
            field.HasTabStop = bool.Parse(fieldNode.Attributes["HasTabStop"].Value);
            //field.PromptFont.FontFamily.Name = fieldNode.Attributes["PromptFontFamily"].Value;
            //field.PromptFont.Style = fieldNode.Attributes["PromptFontStyle"].Value;
            //field.PromptFont.Size = fieldNode.Attributes["PromptFontSize"].Value;
            //field.PromptFont.Name = fieldNode.Attributes["PromptScriptName"].Value;
            field.PromptTopPositionPercentage = Double.Parse(fieldNode.Attributes["PromptTopPositionPercentage"].Value);
            field.PromptLeftPositionPercentage = Double.Parse(fieldNode.Attributes["PromptLeftPositionPercentage"].Value);
            //field.ControlFont.Name = fieldNode.Attributes["ControlScriptName"].Value;
            field.ShouldRepeatLast = bool.Parse(fieldNode.Attributes["ShouldRepeatLast"].Value);
            field.IsRequired = bool.Parse(fieldNode.Attributes["IsRequired"].Value);
            field.IsReadOnly = bool.Parse(fieldNode.Attributes["IsReadOnly"].Value);
            //field.IsControlResizable = bool.Parse(fieldNode.Attributes["ShouldRetainImageSize"].Value);
            field.CodeColumnName = fieldNode.Attributes["CodeColumnName"].Value;
            field.IsExclusiveTable = bool.Parse(fieldNode.Attributes["IsExclusiveTable"].Value);
            field.ShouldSort = bool.Parse(fieldNode.Attributes["Sort"].Value);
            field.SourceTableName = fieldNode.Attributes["SourceTableName"].Value;
            field.TextColumnName = fieldNode.Attributes["TextColumnName"].Value;
            field.TableName = fieldNode.Attributes["DataTableName"].Value;

            //field. = fieldNode.Attributes["FieldTypeId"].Value;

            field.Page = new Page(field.GetView());
            //field.Page.Name = 
            field.Page.Id = int.Parse(fieldNode.Attributes["PageId"].Value);

            //field.CheckCodeAfter = fieldNode.Attributes["CheckCodeAfter"].Value.ToString();
            //field.CheckCodeBefore = fieldNode.Attributes["CheckCodeBefore"].Value.ToString();
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
        /// Retrieves data for grid field from xml metadata
        /// </summary>
        /// <param name="field">A grid field</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(GridField field, XmlNode fieldNode)
        {
            field.Id = Int32.Parse(fieldNode.Attributes["FieldId"].Value);
            field.Name = fieldNode.Attributes["Name"].Value;
            field.PromptText = fieldNode.Attributes["PromptText"].Value;
            //field.ControlFont = System.Drawing.Font(fieldNode.Attributes["ControlFontFamily"].Value);
            //field.ControlFont.Style = fieldNode.Attributes["ControlFontStyle"].Value;
            //field.ControlFont.Size = float.Parse(fieldNode.Attributes["ControlFontSize"].Value);
            field.ControlTopPositionPercentage = Double.Parse(fieldNode.Attributes["ControlTopPositionPercentage"].Value);
            field.ControlLeftPositionPercentage = Double.Parse(fieldNode.Attributes["ControlLeftPositionPercentage"].Value);
            field.ControlHeightPercentage = Double.Parse(fieldNode.Attributes["ControlHeightPercentage"].Value);
            field.ControlWidthPercentage = Double.Parse(fieldNode.Attributes["ControlWidthPercentage"].Value);
            field.TabIndex = Int32.Parse(fieldNode.Attributes["TabIndex"].Value);
            field.HasTabStop = bool.Parse(fieldNode.Attributes["HasTabStop"].Value);
            //field.PromptFont.FontFamily.Name = fieldNode.Attributes["PromptFontFamily"].Value;
            //field.PromptFont.Style = fieldNode.Attributes["PromptFontStyle"].Value;
            //field.PromptFont.Size = fieldNode.Attributes["PromptFontSize"].Value;
            //field.PromptFont.Name = fieldNode.Attributes["PromptScriptName"].Value;               
            //field.ControlFont.Name = fieldNode.Attributes["ControlScriptName"].Value;                
            //field.IsControlResizable = bool.Parse(fieldNode.Attributes["ShouldRetainImageSize"].Value);
            //field. = fieldNode.Attributes["FieldTypeId"].Value;

            field.Page = new Page(field.GetView());
            //field.Page.Name = 
            field.Page.Id = int.Parse(fieldNode.Attributes["PageId"].Value);

            //TODO:  Get Grid Column information
        }

       
        /// <summary>
        /// Retrieves data for GUID field from xml metadata
        /// </summary>
        /// <param name="field">A GUID field</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(GUIDField field, XmlNode fieldNode)
        {
            field.Id = Int32.Parse(fieldNode.Attributes["FieldId"].Value);
            field.Name = fieldNode.Attributes["Name"].Value;
            field.PromptText = fieldNode.Attributes["PromptText"].Value;
            field.ControlTopPositionPercentage = Double.Parse(fieldNode.Attributes["ControlTopPositionPercentage"].Value);
            field.ControlLeftPositionPercentage = Double.Parse(fieldNode.Attributes["ControlLeftPositionPercentage"].Value);
            field.ControlHeightPercentage = Double.Parse(fieldNode.Attributes["ControlHeightPercentage"].Value);
            field.ControlWidthPercentage = Double.Parse(fieldNode.Attributes["ControlWidthPercentage"].Value);
            field.TabIndex = Int32.Parse(fieldNode.Attributes["TabIndex"].Value);
            field.HasTabStop = bool.Parse(fieldNode.Attributes["HasTabStop"].Value);
            field.PromptTopPositionPercentage = Double.Parse(fieldNode.Attributes["PromptTopPositionPercentage"].Value);
            field.PromptLeftPositionPercentage = Double.Parse(fieldNode.Attributes["PromptLeftPositionPercentage"].Value);
            field.ShouldRepeatLast = bool.Parse(fieldNode.Attributes["ShouldRepeatLast"].Value);
            field.IsRequired = bool.Parse(fieldNode.Attributes["IsRequired"].Value);
            field.TableName = fieldNode.Attributes["DataTableName"].Value;
            field.SourceFieldId = Int32.Parse(fieldNode.Attributes["SourceFieldId"].Value);
       
            field.Page = new Page(field.GetView());
            field.Page.Id = int.Parse(fieldNode.Attributes["PageId"].Value);                  
        }

        /// <summary>
        /// Retrieves data for image field from xml metadata
        /// </summary>
        /// <param name="field">An image field</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(ImageField field, XmlNode fieldNode)
        {
            field.Id = Int32.Parse(fieldNode.Attributes["FieldId"].Value);
            field.Name = fieldNode.Attributes["Name"].Value;
            field.PromptText = fieldNode.Attributes["PromptText"].Value;
            //field.ControlFont = System.Drawing.Font(fieldNode.Attributes["ControlFontFamily"].Value);
            //field.ControlFont.Style = fieldNode.Attributes["ControlFontStyle"].Value;
            //field.ControlFont.Size = float.Parse(fieldNode.Attributes["ControlFontSize"].Value);
            field.ControlTopPositionPercentage = Double.Parse(fieldNode.Attributes["ControlTopPositionPercentage"].Value);
            field.ControlLeftPositionPercentage = Double.Parse(fieldNode.Attributes["ControlLeftPositionPercentage"].Value);
            field.ControlHeightPercentage = Double.Parse(fieldNode.Attributes["ControlHeightPercentage"].Value);
            field.ControlWidthPercentage = Double.Parse(fieldNode.Attributes["ControlWidthPercentage"].Value);
            field.TabIndex = Int32.Parse(fieldNode.Attributes["TabIndex"].Value);
            field.HasTabStop = bool.Parse(fieldNode.Attributes["HasTabStop"].Value);
            //field.PromptFont.FontFamily.Name = fieldNode.Attributes["PromptFontFamily"].Value;
            //field.PromptFont.Style = fieldNode.Attributes["PromptFontStyle"].Value;
            //field.PromptFont.Size = fieldNode.Attributes["PromptFontSize"].Value;
            //field.PromptFont.Name = fieldNode.Attributes["PromptScriptName"].Value;
            field.PromptTopPositionPercentage = Double.Parse(fieldNode.Attributes["PromptTopPositionPercentage"].Value);
            field.PromptLeftPositionPercentage = Double.Parse(fieldNode.Attributes["PromptLeftPositionPercentage"].Value);
            //field.ControlFont.Name = fieldNode.Attributes["ControlScriptName"].Value;
            field.ShouldRepeatLast = bool.Parse(fieldNode.Attributes["ShouldRepeatLast"].Value);
            field.IsRequired = bool.Parse(fieldNode.Attributes["IsRequired"].Value);
            field.IsReadOnly = bool.Parse(fieldNode.Attributes["IsReadOnly"].Value);
            //field.IsControlResizable = bool.Parse(fieldNode.Attributes["ShouldRetainImageSize"].Value);
            field.TableName = fieldNode.Attributes["DataTableName"].Value;
            //field. = fieldNode.Attributes["FieldTypeId"].Value;

            field.Page = new Page(field.GetView());
            //field.Page.Name = 
            field.Page.Id = int.Parse(fieldNode.Attributes["PageId"].Value);
        }

        /// <summary>
        /// Retrieves data for label field from xml metadata
        /// </summary>
        /// <param name="field">A label field</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(LabelField field, XmlNode fieldNode)
        {
            field.Id = Int32.Parse(fieldNode.Attributes["FieldId"].Value);
            field.Name = fieldNode.Attributes["Name"].Value;
            field.PromptText = fieldNode.Attributes["PromptText"].Value;
            //field.ControlFont = System.Drawing.Font(fieldNode.Attributes["ControlFontFamily"].Value);
            //field.ControlFont.Style = fieldNode.Attributes["ControlFontStyle"].Value;
            //field.ControlFont.Size = float.Parse(fieldNode.Attributes["ControlFontSize"].Value);
            field.ControlTopPositionPercentage = Double.Parse(fieldNode.Attributes["ControlTopPositionPercentage"].Value);
            field.ControlLeftPositionPercentage = Double.Parse(fieldNode.Attributes["ControlLeftPositionPercentage"].Value);
            field.ControlHeightPercentage = Double.Parse(fieldNode.Attributes["ControlHeightPercentage"].Value);
            field.ControlWidthPercentage = Double.Parse(fieldNode.Attributes["ControlWidthPercentage"].Value);
            field.TabIndex = Int32.Parse(fieldNode.Attributes["TabIndex"].Value);
            field.HasTabStop = bool.Parse(fieldNode.Attributes["HasTabStop"].Value);
            //field.PromptFont.FontFamily.Name = fieldNode.Attributes["PromptFontFamily"].Value;
            //field.PromptFont.Style = fieldNode.Attributes["PromptFontStyle"].Value;
            //field.PromptFont.Size = fieldNode.Attributes["PromptFontSize"].Value;
            //field.PromptFont.Name = fieldNode.Attributes["PromptScriptName"].Value;               
            //field.ControlFont.Name = fieldNode.Attributes["ControlScriptName"].Value;                                                
            //field.IsControlResizable = bool.Parse(fieldNode.Attributes["ShouldRetainImageSize"].Value);
            //field. = fieldNode.Attributes["FieldTypeId"].Value;

            field.Page = new Page(field.GetView());
            //field.Page.Name = 
            field.Page.Id = int.Parse(fieldNode.Attributes["PageId"].Value);
        }

        /// <summary>
        /// Retrieves data for mirror field from xml metadata
        /// </summary>
        /// <param name="field">A mirror field</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(MirrorField field, XmlNode fieldNode)
        {
            field.Id = Int32.Parse(fieldNode.Attributes["FieldId"].Value);
            field.Name = fieldNode.Attributes["Name"].Value;
            field.PromptText = fieldNode.Attributes["PromptText"].Value;
            //field.ControlFont = System.Drawing.Font(fieldNode.Attributes["ControlFontFamily"].Value);
            //field.ControlFont.Style = fieldNode.Attributes["ControlFontStyle"].Value;
            //field.ControlFont.Size = float.Parse(fieldNode.Attributes["ControlFontSize"].Value);
            field.ControlTopPositionPercentage = Double.Parse(fieldNode.Attributes["ControlTopPositionPercentage"].Value);
            field.ControlLeftPositionPercentage = Double.Parse(fieldNode.Attributes["ControlLeftPositionPercentage"].Value);
            field.ControlHeightPercentage = Double.Parse(fieldNode.Attributes["ControlHeightPercentage"].Value);
            field.ControlWidthPercentage = Double.Parse(fieldNode.Attributes["ControlWidthPercentage"].Value);
            field.TabIndex = Int32.Parse(fieldNode.Attributes["TabIndex"].Value);
            field.HasTabStop = bool.Parse(fieldNode.Attributes["HasTabStop"].Value);
            //field.PromptFont.FontFamily.Name = fieldNode.Attributes["PromptFontFamily"].Value;
            //field.PromptFont.Style = fieldNode.Attributes["PromptFontStyle"].Value;
            //field.PromptFont.Size = fieldNode.Attributes["PromptFontSize"].Value;
            //field.PromptFont.Name = fieldNode.Attributes["PromptScriptName"].Value;
            field.PromptTopPositionPercentage = Double.Parse(fieldNode.Attributes["PromptTopPositionPercentage"].Value);
            field.PromptLeftPositionPercentage = Double.Parse(fieldNode.Attributes["PromptLeftPositionPercentage"].Value);
            //field.ControlFont.Name = fieldNode.Attributes["ControlScriptName"].Value;                                                
            //field.IsControlResizable = bool.Parse(fieldNode.Attributes["ShouldRetainImageSize"].Value);
            field.SourceFieldId = Int32.Parse(fieldNode.Attributes["SourceFieldId"].Value);
            //field. = fieldNode.Attributes["FieldTypeId"].Value;

            field.Page = new Page(field.GetView());
            //field.Page.Name = 
            field.Page.Id = int.Parse(fieldNode.Attributes["PageId"].Value);
        }

        /// <summary>
        /// Retrieves data for multiline text field from xml metadata
        /// </summary>
        /// <param name="field">A multiline text field</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(MultilineTextField field, XmlNode fieldNode)
        {
            field.Id = Int32.Parse(fieldNode.Attributes["FieldId"].Value);
            field.Name = fieldNode.Attributes["Name"].Value;
            field.PromptText = fieldNode.Attributes["PromptText"].Value;
            //field.ControlFont = System.Drawing.Font(fieldNode.Attributes["ControlFontFamily"].Value);
            //field.ControlFont.Style = fieldNode.Attributes["ControlFontStyle"].Value;
            //field.ControlFont.Size = float.Parse(fieldNode.Attributes["ControlFontSize"].Value);
            field.ControlTopPositionPercentage = Double.Parse(fieldNode.Attributes["ControlTopPositionPercentage"].Value);
            field.ControlLeftPositionPercentage = Double.Parse(fieldNode.Attributes["ControlLeftPositionPercentage"].Value);
            field.ControlHeightPercentage = Double.Parse(fieldNode.Attributes["ControlHeightPercentage"].Value);
            field.ControlWidthPercentage = Double.Parse(fieldNode.Attributes["ControlWidthPercentage"].Value);
            field.TabIndex = Int32.Parse(fieldNode.Attributes["TabIndex"].Value);
            field.HasTabStop = bool.Parse(fieldNode.Attributes["HasTabStop"].Value);
            //field.PromptFont.FontFamily.Name = fieldNode.Attributes["PromptFontFamily"].Value;
            //field.PromptFont.Style = fieldNode.Attributes["PromptFontStyle"].Value;
            //field.PromptFont.Size = fieldNode.Attributes["PromptFontSize"].Value;
            //field.PromptFont.Name = fieldNode.Attributes["PromptScriptName"].Value;
            field.PromptTopPositionPercentage = Double.Parse(fieldNode.Attributes["PromptTopPositionPercentage"].Value);
            field.PromptLeftPositionPercentage = Double.Parse(fieldNode.Attributes["PromptLeftPositionPercentage"].Value);
            //field.ControlFont.Name = fieldNode.Attributes["ControlScriptName"].Value;
            field.ShouldRepeatLast = bool.Parse(fieldNode.Attributes["ShouldRepeatLast"].Value);
            field.IsRequired = bool.Parse(fieldNode.Attributes["IsRequired"].Value);
            field.IsReadOnly = bool.Parse(fieldNode.Attributes["IsReadOnly"].Value);
            //field.IsControlResizable = bool.Parse(fieldNode.Attributes["ShouldRetainImageSize"].Value);
            field.MaxLength = Int32.Parse(fieldNode.Attributes["MaxLength"].Value);
            field.TableName = fieldNode.Attributes["DataTableName"].Value;
            field.SourceFieldId = Int32.Parse(fieldNode.Attributes["SourceFieldId"].Value);
            //field. = fieldNode.Attributes["FieldTypeId"].Value;

            field.Page = new Page(field.GetView());
            //field.Page.Name = 
            field.Page.Id = int.Parse(fieldNode.Attributes["PageId"].Value);

            //field.CheckCodeAfter = fieldNode.Attributes["CheckCodeAfter"].Value.ToString();
            //field.CheckCodeBefore = fieldNode.Attributes["CheckCodeBefore"].Value.ToString();
        }

        /// <summary>
        /// Retrieves data for number field from xml metadata
        /// </summary>
        /// <param name="field">A number field</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(NumberField field, XmlNode fieldNode)
        {
            field.Id = Int32.Parse(fieldNode.Attributes["FieldId"].Value);
            field.Name = fieldNode.Attributes["Name"].Value;
            field.PromptText = fieldNode.Attributes["PromptText"].Value;
            //field.ControlFont = System.Drawing.Font(fieldNode.Attributes["ControlFontFamily"].Value);
            //field.ControlFont.Style = fieldNode.Attributes["ControlFontStyle"].Value;
            //field.ControlFont.Size = float.Parse(fieldNode.Attributes["ControlFontSize"].Value);
            field.ControlTopPositionPercentage = Double.Parse(fieldNode.Attributes["ControlTopPositionPercentage"].Value);
            field.ControlLeftPositionPercentage = Double.Parse(fieldNode.Attributes["ControlLeftPositionPercentage"].Value);
            field.ControlHeightPercentage = Double.Parse(fieldNode.Attributes["ControlHeightPercentage"].Value);
            field.ControlWidthPercentage = Double.Parse(fieldNode.Attributes["ControlWidthPercentage"].Value);
            field.TabIndex = Int32.Parse(fieldNode.Attributes["TabIndex"].Value);
            field.HasTabStop = bool.Parse(fieldNode.Attributes["HasTabStop"].Value);
            //field.PromptFont.FontFamily.Name = fieldNode.Attributes["PromptFontFamily"].Value;
            //field.PromptFont.Style = fieldNode.Attributes["PromptFontStyle"].Value;
            //field.PromptFont.Size = fieldNode.Attributes["PromptFontSize"].Value;
            //field.PromptFont.Name = fieldNode.Attributes["PromptScriptName"].Value;
            field.PromptTopPositionPercentage = Double.Parse(fieldNode.Attributes["PromptTopPositionPercentage"].Value);
            field.PromptLeftPositionPercentage = Double.Parse(fieldNode.Attributes["PromptLeftPositionPercentage"].Value);
            //field.ControlFont.Name = fieldNode.Attributes["ControlScriptName"].Value;
            field.ShouldRepeatLast = bool.Parse(fieldNode.Attributes["ShouldRepeatLast"].Value);
            field.IsRequired = bool.Parse(fieldNode.Attributes["IsRequired"].Value);
            field.IsReadOnly = bool.Parse(fieldNode.Attributes["IsReadOnly"].Value);
            //field.IsControlResizable = bool.Parse(fieldNode.Attributes["ShouldRetainImageSize"].Value);
            field.Lower = fieldNode.Attributes["Lower"].Value;
            field.Upper = fieldNode.Attributes["Upper"].Value;
            field.Pattern = fieldNode.Attributes["Pattern"].Value;
            field.TableName = fieldNode.Attributes["DataTableName"].Value;
            //field. = fieldNode.Attributes["FieldTypeId"].Value;

            field.Page = new Page(field.GetView());
            //field.Page.Name = 
            field.Page.Id = int.Parse(fieldNode.Attributes["PageId"].Value);

            //field.CheckCodeAfter = fieldNode.Attributes["CheckCodeAfter"].Value.ToString();
            //field.CheckCodeBefore = fieldNode.Attributes["CheckCodeBefore"].Value.ToString();

        }

        /// <summary>
        /// Retrieves data for option field from xml metadata
        /// </summary>
        /// <param name="field">An option field</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(OptionField field, XmlNode fieldNode)
        {
            field.Id = Int32.Parse(fieldNode.Attributes["FieldId"].Value);
            field.Name = fieldNode.Attributes["Name"].Value;
            field.PromptText = fieldNode.Attributes["PromptText"].Value;
            //field.ControlFont = System.Drawing.Font(fieldNode.Attributes["ControlFontFamily"].Value);
            //field.ControlFont.Style = fieldNode.Attributes["ControlFontStyle"].Value;
            //field.ControlFont.Size = float.Parse(fieldNode.Attributes["ControlFontSize"].Value);
            field.ControlTopPositionPercentage = Double.Parse(fieldNode.Attributes["ControlTopPositionPercentage"].Value);
            field.ControlLeftPositionPercentage = Double.Parse(fieldNode.Attributes["ControlLeftPositionPercentage"].Value);
            field.ControlHeightPercentage = Double.Parse(fieldNode.Attributes["ControlHeightPercentage"].Value);
            field.ControlWidthPercentage = Double.Parse(fieldNode.Attributes["ControlWidthPercentage"].Value);
            field.TabIndex = Int32.Parse(fieldNode.Attributes["TabIndex"].Value);
            field.HasTabStop = bool.Parse(fieldNode.Attributes["HasTabStop"].Value);
            //field.PromptFont.FontFamily.Name = fieldNode.Attributes["PromptFontFamily"].Value;
            //field.PromptFont.Style = fieldNode.Attributes["PromptFontStyle"].Value;
            //field.PromptFont.Size = fieldNode.Attributes["PromptFontSize"].Value;
            //field.PromptFont.Name = fieldNode.Attributes["PromptScriptName"].Value;           
            //field.ControlFont.Name = fieldNode.Attributes["ControlScriptName"].Value;
            field.ShouldRepeatLast = bool.Parse(fieldNode.Attributes["ShouldRepeatLast"].Value);
            field.IsRequired = bool.Parse(fieldNode.Attributes["IsRequired"].Value);
            field.IsReadOnly = bool.Parse(fieldNode.Attributes["IsReadOnly"].Value);
            field.ShowTextOnRight = bool.Parse(fieldNode.Attributes["ShowTextOnRight"].Value);
            //field.IsControlResizable = bool.Parse(fieldNode.Attributes["ShouldRetainImageSize"].Value);
            field.TableName = fieldNode.Attributes["DataTableName"].Value;

            //field. = fieldNode.Attributes["FieldTypeId"].Value;

            field.Page = new Page(field.GetView());
            //field.Page.Name = 
            field.Page.Id = int.Parse(fieldNode.Attributes["PageId"].Value);

            //field.CheckCodeAfter = fieldNode.Attributes["CheckCodeAfter"].Value.ToString();                        
        }

        /// <summary>
        /// Retrieves data for phone number field from xml metadata
        /// </summary>
        /// <param name="field">A phone number field</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(PhoneNumberField field, XmlNode fieldNode)
        {
            field.Id = Int32.Parse(fieldNode.Attributes["FieldId"].Value);
            field.Name = fieldNode.Attributes["Name"].Value;
            field.PromptText = fieldNode.Attributes["PromptText"].Value;
            //field.ControlFont = System.Drawing.Font(fieldNode.Attributes["ControlFontFamily"].Value);
            //field.ControlFont.Style = fieldNode.Attributes["ControlFontStyle"].Value;
            //field.ControlFont.Size = float.Parse(fieldNode.Attributes["ControlFontSize"].Value);
            field.ControlTopPositionPercentage = Double.Parse(fieldNode.Attributes["ControlTopPositionPercentage"].Value);
            field.ControlLeftPositionPercentage = Double.Parse(fieldNode.Attributes["ControlLeftPositionPercentage"].Value);
            field.ControlHeightPercentage = Double.Parse(fieldNode.Attributes["ControlHeightPercentage"].Value);
            field.ControlWidthPercentage = Double.Parse(fieldNode.Attributes["ControlWidthPercentage"].Value);
            field.TabIndex = Int32.Parse(fieldNode.Attributes["TabIndex"].Value);
            field.HasTabStop = bool.Parse(fieldNode.Attributes["HasTabStop"].Value);
            //field.PromptFont.FontFamily.Name = fieldNode.Attributes["PromptFontFamily"].Value;
            //field.PromptFont.Style = fieldNode.Attributes["PromptFontStyle"].Value;
            //field.PromptFont.Size = fieldNode.Attributes["PromptFontSize"].Value;
            //field.PromptFont.Name = fieldNode.Attributes["PromptScriptName"].Value;
            field.PromptTopPositionPercentage = Double.Parse(fieldNode.Attributes["PromptTopPositionPercentage"].Value);
            field.PromptLeftPositionPercentage = Double.Parse(fieldNode.Attributes["PromptLeftPositionPercentage"].Value);
            //field.ControlFont.Name = fieldNode.Attributes["ControlScriptName"].Value;
            field.ShouldRepeatLast = bool.Parse(fieldNode.Attributes["ShouldRepeatLast"].Value);
            field.IsRequired = bool.Parse(fieldNode.Attributes["IsRequired"].Value);
            field.IsReadOnly = bool.Parse(fieldNode.Attributes["IsReadOnly"].Value);
            //field.IsControlResizable = bool.Parse(fieldNode.Attributes["ShouldRetainImageSize"].Value);
            field.Pattern = fieldNode.Attributes["Pattern"].Value;
            field.TableName = fieldNode.Attributes["DataTableName"].Value;

            //field. = fieldNode.Attributes["FieldTypeId"].Value;

            field.Page = new Page(field.GetView());
            //field.Page.Name = 
            field.Page.Id = int.Parse(fieldNode.Attributes["PageId"].Value);

            //field.CheckCodeAfter = fieldNode.Attributes["CheckCodeAfter"].Value.ToString();
            //field.CheckCodeBefore = fieldNode.Attributes["CheckCodeBefore"].Value.ToString();        
        }

        /// <summary>
        /// Retrieves data for rec status field from xml metadata
        /// </summary>
        /// <param name="field">A rec status field</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(RecStatusField field, XmlNode fieldNode)
        {
        }
        /// <summary>
        /// Retrieves data for related view field from xml metadata
        /// </summary>
        /// <param name="field">A related view field</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(RelatedViewField field, XmlNode fieldNode)
        {
            field.Id = Int32.Parse(fieldNode.Attributes["FieldId"].Value);
            field.Name = fieldNode.Attributes["Name"].Value;
            field.PromptText = fieldNode.Attributes["PromptText"].Value;
            //field.ControlFont = System.Drawing.Font(fieldNode.Attributes["ControlFontFamily"].Value);
            //field.ControlFont.Style = fieldNode.Attributes["ControlFontStyle"].Value;
            //field.ControlFont.Size = float.Parse(fieldNode.Attributes["ControlFontSize"].Value);
            field.ControlTopPositionPercentage = Double.Parse(fieldNode.Attributes["ControlTopPositionPercentage"].Value);
            field.ControlLeftPositionPercentage = Double.Parse(fieldNode.Attributes["ControlLeftPositionPercentage"].Value);
            field.ControlHeightPercentage = Double.Parse(fieldNode.Attributes["ControlHeightPercentage"].Value);
            field.ControlWidthPercentage = Double.Parse(fieldNode.Attributes["ControlWidthPercentage"].Value);
            field.TabIndex = Int32.Parse(fieldNode.Attributes["TabIndex"].Value);
            field.HasTabStop = bool.Parse(fieldNode.Attributes["HasTabStop"].Value);
            //field.PromptFont.FontFamily.Name = fieldNode.Attributes["PromptFontFamily"].Value;
            //field.PromptFont.Style = fieldNode.Attributes["PromptFontStyle"].Value;
            //field.PromptFont.Size = fieldNode.Attributes["PromptFontSize"].Value;
            //field.PromptFont.Name = fieldNode.Attributes["PromptScriptName"].Value;              
            //field.ControlFont.Name = fieldNode.Attributes["ControlScriptName"].Value;                              
            //field.IsControlResizable = bool.Parse(fieldNode.Attributes["ShouldRetainImageSize"].Value);
            field.Condition = fieldNode.Attributes["RelateCondition"].Value;
            field.RelatedViewID = Int32.Parse(fieldNode.Attributes["RelatedViewId"].Value);
            field.ShouldReturnToParent = bool.Parse(fieldNode.Attributes["ShouldReturnToParent"].Value);

            //field. = fieldNode.Attributes["FieldTypeId"].Value;

            field.Page = new Page(field.GetView());
            //field.Page.Name = 
            field.Page.Id = int.Parse(fieldNode.Attributes["PageId"].Value);
        }

        /// <summary>
        /// Retrieves data for single line text field from xml metadata
        /// </summary>
        /// <param name="field">A single line text field</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(SingleLineTextField field, XmlNode fieldNode)
        {
            field.Id = Int32.Parse(fieldNode.Attributes["FieldId"].Value);
            field.Name = fieldNode.Attributes["Name"].Value;
            field.PromptText = fieldNode.Attributes["PromptText"].Value;
            //field.ControlFont = System.Drawing.Font(fieldNode.Attributes["ControlFontFamily"].Value);
            //field.ControlFont.Style = fieldNode.Attributes["ControlFontStyle"].Value;
            //field.ControlFont.Size = float.Parse(fieldNode.Attributes["ControlFontSize"].Value);
            field.ControlTopPositionPercentage = Double.Parse(fieldNode.Attributes["ControlTopPositionPercentage"].Value);
            field.ControlLeftPositionPercentage = Double.Parse(fieldNode.Attributes["ControlLeftPositionPercentage"].Value);
            field.ControlHeightPercentage = Double.Parse(fieldNode.Attributes["ControlHeightPercentage"].Value);
            field.ControlWidthPercentage = Double.Parse(fieldNode.Attributes["ControlWidthPercentage"].Value);
            field.TabIndex = Int32.Parse(fieldNode.Attributes["TabIndex"].Value);
            field.HasTabStop = bool.Parse(fieldNode.Attributes["HasTabStop"].Value);
            //field.PromptFont.FontFamily.Name = fieldNode.Attributes["PromptFontFamily"].Value;
            //field.PromptFont.Style = fieldNode.Attributes["PromptFontStyle"].Value;
            //field.PromptFont.Size = fieldNode.Attributes["PromptFontSize"].Value;
            //field.PromptFont.Name = fieldNode.Attributes["PromptScriptName"].Value;
            field.PromptTopPositionPercentage = Double.Parse(fieldNode.Attributes["PromptTopPositionPercentage"].Value);
            field.PromptLeftPositionPercentage = Double.Parse(fieldNode.Attributes["PromptLeftPositionPercentage"].Value);
            //field.ControlFont.Name = fieldNode.Attributes["ControlScriptName"].Value;
            field.ShouldRepeatLast = bool.Parse(fieldNode.Attributes["ShouldRepeatLast"].Value);
            field.IsRequired = bool.Parse(fieldNode.Attributes["IsRequired"].Value);
            field.IsReadOnly = bool.Parse(fieldNode.Attributes["IsReadOnly"].Value);
            //field.IsControlResizable = bool.Parse(fieldNode.Attributes["ShouldRetainImageSize"].Value);
            field.MaxLength = Int32.Parse(fieldNode.Attributes["MaxLength"].Value);
            field.TableName = fieldNode.Attributes["DataTableName"].Value;
            field.SourceFieldId = Int32.Parse(fieldNode.Attributes["SourceFieldId"].Value);

            //field. = fieldNode.Attributes["FieldTypeId"].Value;         

            field.Page = new Page(field.GetView());
            //                field.Page.Name = 
            field.Page.Id = int.Parse(fieldNode.Attributes["PageId"].Value);

            //field.CheckCodeAfter = fieldNode.Attributes["CheckCodeAfter"].Value.ToString();
            //field.CheckCodeBefore = fieldNode.Attributes["CheckCodeBefore"].Value.ToString();                   
        }

        /// <summary>
        /// Retrieves data for time field from xml metadata
        /// </summary>
        /// <param name="field">A time field</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(TimeField field, XmlNode fieldNode)
        {
            field.Id = Int32.Parse(fieldNode.Attributes["FieldId"].Value);
            field.Name = fieldNode.Attributes["Name"].Value;
            field.PromptText = fieldNode.Attributes["PromptText"].Value;
            //field.ControlFont = System.Drawing.Font(fieldNode.Attributes["ControlFontFamily"].Value);
            //field.ControlFont.Style = fieldNode.Attributes["ControlFontStyle"].Value;
            //field.ControlFont.Size = float.Parse(fieldNode.Attributes["ControlFontSize"].Value);
            field.ControlTopPositionPercentage = Double.Parse(fieldNode.Attributes["ControlTopPositionPercentage"].Value);
            field.ControlLeftPositionPercentage = Double.Parse(fieldNode.Attributes["ControlLeftPositionPercentage"].Value);
            field.ControlHeightPercentage = Double.Parse(fieldNode.Attributes["ControlHeightPercentage"].Value);
            field.ControlWidthPercentage = Double.Parse(fieldNode.Attributes["ControlWidthPercentage"].Value);
            field.TabIndex = Int32.Parse(fieldNode.Attributes["TabIndex"].Value);
            field.HasTabStop = bool.Parse(fieldNode.Attributes["HasTabStop"].Value);
            //field.PromptFont.FontFamily.Name = fieldNode.Attributes["PromptFontFamily"].Value;
            //field.PromptFont.Style = fieldNode.Attributes["PromptFontStyle"].Value;
            //field.PromptFont.Size = fieldNode.Attributes["PromptFontSize"].Value;
            //field.PromptFont.Name = fieldNode.Attributes["PromptScriptName"].Value;
            field.PromptTopPositionPercentage = Double.Parse(fieldNode.Attributes["PromptTopPositionPercentage"].Value);
            field.PromptLeftPositionPercentage = Double.Parse(fieldNode.Attributes["PromptLeftPositionPercentage"].Value);
            //field.ControlFont.Name = fieldNode.Attributes["ControlScriptName"].Value;
            field.ShouldRepeatLast = bool.Parse(fieldNode.Attributes["ShouldRepeatLast"].Value);
            field.IsRequired = bool.Parse(fieldNode.Attributes["IsRequired"].Value);
            field.IsReadOnly = bool.Parse(fieldNode.Attributes["IsReadOnly"].Value);
            //field.IsControlResizable = bool.Parse(fieldNode.Attributes["ShouldRetainImageSize"].Value);
            field.TableName = fieldNode.Attributes["DataTableName"].Value;

            //field. = fieldNode.Attributes["FieldTypeId"].Value;

            field.Page = new Page(field.GetView());
            //field.Page.Name = 
            field.Page.Id = int.Parse(fieldNode.Attributes["PageId"].Value);

            //field.CheckCodeAfter = fieldNode.Attributes["CheckCodeAfter"].Value.ToString();
            //field.CheckCodeBefore = fieldNode.Attributes["CheckCodeBefore"].Value.ToString();           
        }

        /// <summary>
        /// Retrieves data for unique key field from xml metadata
        /// </summary>
        /// <param name="field">A unique key field</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(UniqueKeyField field, XmlNode fieldNode)
        {
        }


        /// <summary>
        /// Retrieves data for UniqueIdentifierField from xml metadata
        /// </summary>
        /// <param name="field">A UniqueIdentifier field</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(UniqueIdentifierField field, XmlNode fieldNode)
        {
        }
        /// <summary>
        /// Retrieves data for upper case text field from xml metadata
        /// </summary>
        /// <param name="field">An upper case text field</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(UpperCaseTextField field, XmlNode fieldNode)
        {

        }

        /// <summary>
        /// Retrieves data for yes no field from xml metadata
        /// </summary>
        /// <param name="field">A yes no field</param>
        /// <param name="fieldNode">The field node in the Xml metadata file</param>
        public void GetFieldData(YesNoField field, XmlNode fieldNode)
        {
            field.Id = Int32.Parse(fieldNode.Attributes["FieldId"].Value);
            field.Name = fieldNode.Attributes["Name"].Value;
            field.PromptText = fieldNode.Attributes["PromptText"].Value;
            //field.ControlFont = System.Drawing.Font(fieldNode.Attributes["ControlFontFamily"].Value);
            //field.ControlFont.Style = fieldNode.Attributes["ControlFontStyle"].Value;
            //field.ControlFont.Size = float.Parse(fieldNode.Attributes["ControlFontSize"].Value);
            field.ControlTopPositionPercentage = Double.Parse(fieldNode.Attributes["ControlTopPositionPercentage"].Value);
            field.ControlLeftPositionPercentage = Double.Parse(fieldNode.Attributes["ControlLeftPositionPercentage"].Value);
            field.ControlHeightPercentage = Double.Parse(fieldNode.Attributes["ControlHeightPercentage"].Value);
            field.ControlWidthPercentage = Double.Parse(fieldNode.Attributes["ControlWidthPercentage"].Value);
            field.TabIndex = Int32.Parse(fieldNode.Attributes["TabIndex"].Value);
            field.HasTabStop = bool.Parse(fieldNode.Attributes["HasTabStop"].Value);
            //field.PromptFont.FontFamily.Name = fieldNode.Attributes["PromptFontFamily"].Value;
            //field.PromptFont.Style = fieldNode.Attributes["PromptFontStyle"].Value;
            //field.PromptFont.Size = fieldNode.Attributes["PromptFontSize"].Value;
            //field.PromptFont.Name = fieldNode.Attributes["PromptScriptName"].Value;
            field.PromptTopPositionPercentage = Double.Parse(fieldNode.Attributes["PromptTopPositionPercentage"].Value);
            field.PromptLeftPositionPercentage = Double.Parse(fieldNode.Attributes["PromptLeftPositionPercentage"].Value);
            //field.ControlFont.Name = fieldNode.Attributes["ControlScriptName"].Value;
            field.ShouldRepeatLast = bool.Parse(fieldNode.Attributes["ShouldRepeatLast"].Value);
            field.IsRequired = bool.Parse(fieldNode.Attributes["IsRequired"].Value);
            field.IsReadOnly = bool.Parse(fieldNode.Attributes["IsReadOnly"].Value);
            //field.IsControlResizable = bool.Parse(fieldNode.Attributes["ShouldRetainImageSize"].Value);
            field.TableName = fieldNode.Attributes["DataTableName"].Value;
            //field. = fieldNode.Attributes["FieldTypeId"].Value;

            field.Page = new Page(field.GetView());
            //field.Page.Name = 
            field.Page.Id = int.Parse(fieldNode.Attributes["PageId"].Value);
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
            int fieldTabIndex = 0;
            XmlNode viewsNode = GetXmlDocument().SelectSingleNode("//Views");
            XmlNode viewNode = viewsNode.SelectSingleNode("//View[@ViewId = '" + viewId + "']");
            XmlNode fieldsNode = viewNode.SelectSingleNode("//Fields");
            XmlNode fieldNode = fieldsNode.SelectSingleNode("Field[@FieldId = '" + fieldId + "' and @PageId = '" + pageId + "']");

            if (fieldNode == null)
            {
                fieldTabIndex = -1;
            }
            else
            {
                fieldTabIndex = int.Parse(fieldNode.Attributes["TabIndex"].Value.ToString());
            }

            return fieldTabIndex;
        }

        /// <summary>
        /// Get Code table list.
        /// </summary>
        /// <returns>Code table list in a DataTable.</returns>
        TableSchema.TablesDataTable IMetadataProvider.GetCodeTableList()
        {
            try
            {
                DataSets.TableSchema.TablesDataTable tables = db.GetTableSchema();

                DataRow[] rowsFiltered = tables.Select("TABLE_NAME not like 'code%'");
                foreach (DataRow rowFiltered in rowsFiltered)
                {
                    tables.Rows.Remove(rowFiltered);
                }
                foreach (DataRow row in tables)
                {
                    if (String.IsNullOrEmpty(row.ItemArray[0].ToString()))
                    {
                        //remove a row with an empty string
                        tables.Rows.Remove(row);
                    }
                }
                return tables;
            }
            catch (Exception ex)
            {
                throw new System.ApplicationException("Could not retrieve code tables from database", ex);
            }
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

            query = db.CreateQuery("SELECT DISTINCT f.ViewId FROM dbo.metaViews v INNER JOIN dbo.metaFields f ON f.RelatedViewId = v.ViewId WHERE v.ViewId = @viewId");
            query.Parameters.Add(new QueryParameter("@viewId", DbType.Int32, viewId));
            DataTable results = db.Select(query);

            if (results.Rows.Count > 0)
            {
                RetVal = this.GetViewById((int)results.Rows[0][0]);
            }

            return RetVal;
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
                sb.Append("[PromptFontSize] = @PromptFontSize, ");

                sb.Append("where ");

                if (viewId == -1)
                {
                    sb.Append("[ViewId] > @ViewId");
                }
                else
                {
                    sb.Append("[ViewId] = @ViewId");
                }

                sb.Append("and ");

                if (pageId == -1)
                {
                    sb.Append("[PageId] > @PageId");
                }
                else
                {
                    sb.Append("[PageId] = @PageId");
                }

                Query updateQuery = db.CreateQuery(sb.ToString());

                updateQuery.Parameters.Add(new QueryParameter("@ControlFontFamily", DbType.String, controlFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontStyle", DbType.String, controlFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@ControlFontSize", DbType.Double, controlFont.Size));

                updateQuery.Parameters.Add(new QueryParameter("@PromptFontFamily", DbType.String, promptFont.Name));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontStyle", DbType.String, promptFont.Style.ToString()));
                updateQuery.Parameters.Add(new QueryParameter("@PromptFontSize", DbType.Double, promptFont.Size));

                updateQuery.Parameters.Add(new QueryParameter("@ViewId", DbType.Int32, viewId));
                updateQuery.Parameters.Add(new QueryParameter("@PageId", DbType.Int32, pageId));

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

        #region IMetadataProvider Members
        /// <summary>
        /// Gets a list of code tables.
        /// </summary>
        /// <param name="db">Database driver.</param>
        /// <returns>List of code tables.</returns>
        public TableSchema.TablesDataTable GetCodeTableList(IDbDriver db)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Gets code table names for the current <see cref="Epi.Project"/>.
        /// </summary>
        /// <param name="project"><see cref="Epi.Project"/></param>
        /// <returns>Names of code tables in a <see cref="System.Data.DataTable"/>.</returns>
        public DataTable GetCodeTableNamesForProject(Project project)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Identifies which database system is in use.
        /// </summary>
        /// <returns>Friendly name of DBMS.</returns>
        public string IdentifyDatabase()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// GetBackgroundData
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable GetBackgroundData()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get Page Setup Data
        /// </summary>
        /// <returns>DataTable</returns>
        public DataRow GetPageSetupData(View view)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// GetPageBackgroundData
        /// </summary>
        /// <param name="page">page</param>
        /// <returns>DataTable</returns>
        public DataTable GetPageBackgroundData(Page page)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// InsertPageBackgroundData
        /// </summary>
        /// <param name="page">page</param>
        /// <param name="imagePath">file path for image</param>
        /// <param name="imageLayout">image layout</param>
        /// <param name="color">color</param>
        public void InsertPageBackgroundData(Page page, int imageId, string imageLayout, int color)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// UpdatePageBackgroundData
        /// </summary>
        /// <param name="page">page</param>
        /// <param name="imagePath">file path of image</param>
        /// <param name="imageLayout">image layout</param>
        /// <param name="color">color</param>
        public void UpdatePageBackgroundData(Page page, string imagePath, string imageLayout, int color)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// CreateBackgroundsTable
        /// </summary>
        public void CreateBackgroundsTable()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// CreateBackgroundsTable
        /// </summary>
        public void CreateImagesTable()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IMetadataProvider Members


        public DataRow GetFieldGUIDByNameAsDataRow(string viewName, string fieldName)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

