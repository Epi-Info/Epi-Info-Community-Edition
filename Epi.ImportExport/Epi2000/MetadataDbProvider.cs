using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;

using Epi;
using Epi.Collections;
using Epi.Data;
using Epi.Fields;
using Epi.Data.Services;
using Epi.DataSets;

namespace Epi.Epi2000
{
    /// <summary>
    /// Data access class for Epi Info 2000 metadata
    /// </summary>
    public class MetadataDbProvider //: Epi.Data.Services.MetadataDbProvider
    {
        #region Fields
//        private Epi.Data.Services.MetadataDbProvider metadata = null;
        
        /// <summary>
        /// The underlying physical database
        /// </summary>
        protected IDbDriver db;
        #endregion Fields

        #region Constructors

        /// <summary>
        /// Constructor for the class for Epi 2000 projects
        /// </summary>
        /// <param name="proj">Project the metadata belongs to</param>
        public MetadataDbProvider(Project proj)
        {
            IDbDriverFactory dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.AccessDriver);
            OleDbConnectionStringBuilder cnnStrBuilder = new OleDbConnectionStringBuilder();
            cnnStrBuilder.DataSource = proj.FilePath;
            this.db = dbFactory.CreateDatabaseObject(cnnStrBuilder);
        }

        ///// <summary>
        ///// Constructor for the class.
        ///// </summary>
        ///// <param name="proj">Project the metadata belongs to</param>
        //public MetadataDbProvider(Project proj) : base(proj)
        //{			
        //}

        #endregion Constructors

        #region Public Methods
        public List<string> GetTableNames()
        {
            List<string> tableNames = db.GetTableNames();
            return tableNames;
        }

        private List<string> GetViewNames()
        {
            List<string> viewNames = new List<string>();
            try
            {
                DataTable AllTables = db.GetTableSchema();
                DataRow[] Views = AllTables.Select("TABLE_NAME like 'view%'");
                foreach (DataRow viewRow in Views)
                {
                    string viewName = viewRow["TABLE_NAME"].ToString();
                    viewNames.Add(viewName);
                }

                ////zack should check EpiTable = "View" as a View
                ////use DAO,  //only in Import
                //List<string> tableNames = db.GetTableNames();
                //dao.DBEngineClass dbEngine = new dao.DBEngineClass();
                //dao.Database dbDao = dbEngine.OpenDatabase(db.DataSource, false, false, ";PWD=admin");
                //foreach (string sName in tableNames)
                //{
                //    try
                //    {                        
                //        dao.TableDef table = dbDao.TableDefs[sName];
                //        if(table.Name.StartsWith("meta")) { continue; }
                //        if(table.Name.ToLowerInvariant().StartsWith("view") == false) { continue; }
                //        string s = table.Properties["EpiTable"].Value.ToString().ToLowerInvariant();
                //        //Zack and EJ
                //        //temporaryly, filter out grid view for now 9/17/2008
                //        //when importing, we need to check if it is a grid view.
                //        // if yes, need put them into grid meta data and collected data tables
                //        if (s == "view" && table.Name.StartsWith("view", StringComparison.CurrentCultureIgnoreCase)) 
                //        {
                //            viewNames.Add(table.Name ); 
                //        }
                //    }
                //    catch
                //    {
                //    }

                //}
                

            }
            finally
            {

            }
            return viewNames;
        }

        /// <summary>
        /// Gets a code table by name
        /// </summary>
        /// <param name="tableName">Name of code table</param>
        /// <returns>Contents of the code table</returns>
        //public override DataTable GetCodeTable(string tableName)
        public DataTable GetCodeTable(string tableName)
        {
            Query query = db.CreateQuery("select * from " + tableName);
            return db.Select(query);

        }

        /// <summary>
        /// Gets the field to view relations for a view
        /// </summary>
        /// <param name="viewTableName">Name of view table</param>
        /// <returns>DataTable containing view relations</returns>
        //public override DataTable GetViewRelations(string viewTableName)
        public DataTable GetViewRelations(string viewTableName)
        {
            //**MTG 10/8/2009 - modified query to look at PfontType field instead of Datatable field
            //  to retrieve the names of the related views
            Query query = db.CreateQuery("select v1.[Name] AS FieldName, v1.[FormatString] As RelateCondition, v1.[Lists] As ShouldReturnToParent, v2.[DataTable] AS RelateTable, v2.[Name] AS RelateName from ([" + viewTableName + "] v1 inner join [" + viewTableName + "] v2 on v1.[PfontType] = v2.[Name]) where v2.[Name] like 'RELVIEW%' and v1.[Type] = 'RELATE'");
            return db.Select(query);
        }

        /// <summary>
        /// Gets the view's related views, if any
        /// </summary>
        /// <param name="viewTableName">Name of view table</param>
        /// <returns>List of table names corresponding to all child views linked to this view</returns>        
        public List<string> GetRelatedViewNames(string viewTableName)
        {
            List<string> tableNames = new List<string>();

            Query query = db.CreateQuery("select Datatable from [" + viewTableName + "] where [Name] LIKE 'RELVIEW%'");
            DataTable relateTable = db.Select(query);
            foreach (DataRow row in relateTable.Rows)
            {
                tableNames.Add(row[0].ToString());
            }
            return tableNames;
        }

        /// <summary>
        /// Returns a collection of pages of the view
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        //public override List<Page> GetViewPages(View view)
        public List<Page> GetViewPages(View view)
        {
            List<Page> pages = new List<Page>();
            Query query = db.CreateQuery("select [PageNumber], [Name], [Checkcode] from " + view.Name + " where [PageNumber] < 0 order by [PageNumber] desc");
            DataTable pagesTable = db.Select(query);
            foreach (DataRow pageRow in pagesTable.Rows)
            {
                Page page = new Page(pageRow, view);
                pages.Add(page);
            }
            return pages;
        }

        /// <summary>
        /// Returns a collection of pages of the view
        /// </summary>
        /// <param name="view">The view</param>
        /// <param name="pageNumber">The page number</param>
        /// <returns>string</returns>
        //public override List<Page> GetViewPages(View view)
        public string GetPageCheckCode(View view, int pageNumber)
        {
            pageNumber++;
            Query query = db.CreateQuery("select [Checkcode] from " + view.Name + " where [PageNumber] = " + (pageNumber * -1).ToString());
            DataTable pagesTable = db.Select(query);
            if (pagesTable.Rows.Count > 0)
            {
                string checkCode = pagesTable.Rows[0]["Checkcode"].ToString();
                return checkCode;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets check code variables
        /// </summary>
        /// <param name="viewName">Name of view table</param>
        /// <returns>DataTable containing checkcode variables</returns>
        //public override DataTable GetCheckCodeVariableDefinitions(string viewName)
        public DataTable GetCheckCodeVariableDefinitions(string viewName)
        {
            Query query = db.CreateQuery("select [Checkcode] from " + viewName + " where [Name] = 'DEFINEDVARIABLES'");
            return db.Select(query);
        }

        /// <summary>
        /// Gets view check code
        /// </summary>
        /// <param name="viewName">Name of view table</param>
        /// <returns>DataTable containing view check code</returns>
        //public override DataTable GetViewCheckCode(string viewName)
        public DataTable GetViewCheckCode(string viewName)
        {
            Query query = db.CreateQuery("select [Checkcode] from " + viewName + " where [Name] = 'VIEW'");
            return db.Select(query);
        }

        /// <summary>
        /// Gets the source field name of a mirror field
        /// </summary>
        /// <param name="fieldName">Name of the mirror field</param>
        /// <param name="viewName">Name of the view</param>
        /// <returns>Name of the source field</returns>
        //public override string GetSourceFieldName(string fieldName, string viewName)
        public string GetSourceFieldName(string fieldName, string viewName)
        {
            Query query = db.CreateQuery("select [Lists] from " + viewName + " where [Name] = @Name");
            query.Parameters.Add(new QueryParameter("@Name", DbType.String, fieldName));
            DataTable results = db.Select(query);
            if (results.Rows.Count > 0)
            {
                return results.Rows[0]["Lists"].ToString();
            }
            else
            {
                throw new System.ApplicationException("Source field is not specified for field \"" + fieldName + "\" in view \"" + viewName + "\".");
            }
        }

        /// <summary>
        /// Gets list of tables in the EI 3.x metadata database
        /// </summary>
        /// <returns>DataRow of table names</returns>
        //public override DataTable GetViewsAsDataTable()
        public DataTable GetViewsAsDataTable()
        {
            try
            {
                DataTable viewTable = GetDataTableTemplateForViewInfo();
                foreach (string viewName in this.GetViewNames())
                {
                    DataRow viewRow = viewTable.NewRow();
                    viewTable.Rows.Add(viewRow);

                    string queryText = string.Empty;
                    Query query;
                    DataTable tempTable;

                    // Name ...
                    viewRow[ColumnNames.NAME] = viewName;

                    List<string> tableNames = new List<string>();
                    Query selectQuery = db.CreateQuery("SELECT DISTINCT Datatable FROM " + viewName + "");                    
                    IDataReader reader = db.ExecuteReader(selectQuery);
                             
                    reader = db.ExecuteReader(selectQuery);

                    while (reader.Read())
                    {
                        string name = reader["DATATABLE"].ToString();

                        if (name.StartsWith("DATA") && !name.Contains(","))
                        {
                            tableNames.Add(name);
                        }
                    }

                    if (tableNames.Count <= 1)
                    {
                        // Data Table
                        queryText = "select [DATATABLE] from " + viewName + " where [Name] = 'DATA1'";
                        query = db.CreateQuery(queryText);
                        tempTable = db.Select(query);
                        if (tempTable.Rows.Count > 0)
                        {
                            viewRow[ColumnNames.DATA_TABLE_NAME] = tempTable.Rows[0]["DATATABLE"].ToString();
                        }
                    }
                    else if (tableNames.Count > 1)
                    {
                        viewRow[ColumnNames.DATA_TABLE_NAME] = string.Empty;
                        foreach (string s in tableNames)
                        {
                            // Data Table
                            queryText = "select [DATATABLE] from " + viewName + " where [Name] = @Name";
                            query = db.CreateQuery(queryText);
                            query.Parameters.Add(new QueryParameter("@Name", DbType.String, s));
                            tempTable = db.Select(query);
                            if (tempTable.Rows.Count > 0)
                            {
                                viewRow[ColumnNames.DATA_TABLE_NAME] = viewRow[ColumnNames.DATA_TABLE_NAME].ToString() + tempTable.Rows[0]["DATATABLE"].ToString() + ";";
                            }
                        }
                        viewRow[ColumnNames.DATA_TABLE_NAME] = viewRow[ColumnNames.DATA_TABLE_NAME].ToString().TrimEnd(';');
                    }

                    // CheckCode variable definitions
                    query = db.CreateQuery("select [Checkcode] from " + viewName + " where [Name] = 'DEFINEDVARIABLES'");
                    tempTable = db.Select(query);
                    if (tempTable.Rows.Count > 0)
                    {
                        viewRow[ColumnNames.CHECK_CODE_VARIABLE_DEFINITIONS] = tempTable.Rows[0][ColumnNames.CHECK_CODE].ToString();
                    }

                    // CheckCode Before and After
                    query = db.CreateQuery("select [Checkcode] from " + viewName + " where [Name] = 'VIEW'");
                    tempTable = db.Select(query);
                    if (tempTable.Rows.Count > 0)
                    {
                        string checkCode = tempTable.Rows[0][ColumnNames.CHECK_CODE].ToString();
                        string checkCodeBefore = string.Empty;
                        string checkCodeAfter = string.Empty;
                        SplitCheckCode(checkCode, ref checkCodeBefore, ref checkCodeAfter);
                        viewRow[ColumnNames.CHECK_CODE_BEFORE] = checkCodeBefore;
                        viewRow[ColumnNames.CHECK_CODE_AFTER] = checkCodeAfter;
                    }

                    // Record Check code Before and Afters
                    query = db.CreateQuery("select [Checkcode] from " + viewName + " where [Name] = 'RECORD'");
                    tempTable = db.Select(query);
                    if (tempTable.Rows.Count > 0)
                    {
                        string recordCheckCode = tempTable.Rows[0][ColumnNames.CHECK_CODE].ToString();
                        string recordCheckCodeBefore = string.Empty;
                        string recordCheckCodeAfter = string.Empty;
                        SplitCheckCode(recordCheckCode, ref recordCheckCodeBefore, ref recordCheckCodeAfter);
                        viewRow[ColumnNames.RECORD_CHECK_CODE_BEFORE] = recordCheckCodeBefore;
                        viewRow[ColumnNames.RECORD_CHECK_CODE_AFTER] = recordCheckCodeAfter;
                    }
                }
                return viewTable;
            }
            finally
            {

            }
        }

        ///// <summary>
        ///// Returns all fields of the view as a data table.
        ///// </summary>
        ///// <param name="view"></param>
        ///// <returns></returns>
        //public override DataTable GetFieldsAsDataTable(View view)
        //{
        //    DbQuery query = db.CreateQuery("select [Name],[Prompt],[Type],[Index],[Dsize],[Fsize],[Formatstring],[Plocx],[Plocy],[Flocx],[Flocy],[Taborder],[Pfont],[Pfontsize],[Pfonttype],[Ffont],[Ffontsize],[Ffonttype],[Lists],[Checkcode],[Database],[Datafield],[Datatable],[Webcode] from " + view.Name + " where [PageNumber] > 0");
        //    return db.Select(query);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="view"></param>
        ///// <returns></returns>
        //public override FieldCollectionMaster GetFields(View view)
        //public FieldCollectionMaster GetFields(View view)
        //{
        //    FieldCollectionMaster fields = new FieldCollectionMaster();
        //    Query query = db.CreateQuery("select [Name],[Prompt],[Type],[Index],[Dsize],[Fsize],[Formatstring],[Plocx],[Plocy],[Flocx],[Flocy],[Taborder],[Pfont],[Pfontsize],[Pfonttype],[Ffont],[Ffontsize],[Ffonttype],[Lists],[Checkcode],[Database],[Datafield],[Datatable],[Webcode] from " + view.Name + " where [PageNumber] > 0");
        //    DataTable fieldsTable = db.Select(query);
        //    foreach (DataRow fieldRow in fieldsTable.Rows)
        //    {
        //        MetaFieldType fieldType = InferFieldType(fieldRow);
        //        if (fieldType != MetaFieldType.Group)
        //        {
        //            // RenderableField field = page.CreateNewOpenField(fieldType);
        //        }
        //    }
        //    return fields;
        //}

        /// <summary>
        /// Gets the fields in a page
        /// </summary>
        /// <param name="viewName">Name of the view table</param>
        /// <param name="pageNumber">Page number</param>
        /// <returns>DataTable containing fields</returns>
        //public override DataTable GetFieldsOnPageAsDataTable(string viewName, int pageNumber)
        public DataTable GetFieldsOnPageAsDataTable(string viewName, int pageNumber)
        {
            pageNumber++; // Epi 7 uses zero-based page positioning, so this variable will initially come in with a value of 0 for page 1. This will cause a problem here because page 0 in Epi 3 is not a real page, and we'll receive errors on import.
            Query query = db.CreateQuery("select [Name],[Prompt],[Type],[Index],[Dsize],[Fsize],[Formatstring],[Plocx],[Plocy],[Flocx],[Flocy],[Taborder],[Pfont],[Pfontsize],[Pfonttype],[Ffont],[Ffontsize],[Ffonttype],[Lists],[Checkcode],[Database],[Datafield],[Datatable],[Webcode] from " + viewName + " where [PageNumber] = @PageNumber");
            query.Parameters.Add(new QueryParameter("@PageNumber", DbType.Int32, pageNumber));            
            return db.Select(query);
        }

        /// <summary>
        /// Gets the fields in a view
        /// </summary>
        /// <param name="viewName">Name of the view table</param>
        /// <returns>DataTable containing fields</returns>        
        public DataTable GetFieldsAsDataTable(string viewName)
        {
            Query query = db.CreateQuery("select [Name],[Prompt],[Type],[Index],[Dsize],[Fsize],[Formatstring],[Plocx],[Plocy],[Flocx],[Flocy],[Taborder],[Pfont],[Pfontsize],[Pfonttype],[Ffont],[Ffontsize],[Ffonttype],[Lists],[Checkcode],[Database],[Datafield],[Datatable],[Webcode] from " + viewName + " where [PageNumber] > 0");
            return db.Select(query);
        }  

        /// <summary>
        /// Gets the columns in a grid
        /// </summary>
        /// <param name="gridTableName">Name of grid table</param>
        /// <returns>DataTable containing grid columns</returns>
        //		public override DataTable GetGridColumns(string gridTableName)
        public DataTable GetGridColumns(string gridTableName)
        {
            Query query = db.CreateQuery("select [Name], [Prompt], [Type], [Dsize], [Lists] from " + gridTableName + " where [PageNumber] > 0");
            return db.Select(query);
        }

        /// <summary>
        /// Extracts the code table name from a table variable
        /// </summary>
        /// <param name="viewTableName">Name of the view table</param>
        /// <param name="tableVariableName">Variable in the 3.x database containing code table information</param>
        /// <returns>DataTable containing the code table's name</returns>
        //		public override DataTable GetCodeTableName(string viewTableName, string tableVariableName)
        public DataTable GetCodeTableName(string viewTableName, string tableVariableName)
        {
            Query query = db.CreateQuery("select [Datatable] from " + viewTableName + " where [Name] = @Name");
            query.Parameters.Add(new QueryParameter("@Name", DbType.String, tableVariableName));
            return db.Select(query);
        }
        /// <summary>
        /// Gets list of tables in the EI 3.x metadata database
        /// </summary>
        /// <returns>DataTable containing a list of tables</returns>
        //        public override DataTable GetNonViewTablesAsDataTable()
        public DataTable GetNonViewTablesAsDataTable()
        {
            throw new NotImplementedException("This is broken. Method changed from Get Views and Tables to only Tables excluding Views.");
            //try
            //{

            //    DataTable tables;// = db.GetTableSchema();

            //    tables.Columns["TABLE_NAME"].ColumnName = ColumnNames.NAME;
            //    return tables;
            //}
            //catch (Exception ex)
            //{
            //    throw new System.ApplicationException("Could not retrieve tables from legacy database", ex);
            //}
        }
        /// <summary>
        /// Gets all the programs saved in a project
        /// </summary>
        /// <returns>DataTable containing a list of all programs in a project</returns>
        //		public override DataTable GetPgms()
        public DataTable GetPgms()
        {
            try
            {
                Query query = db.CreateQuery("select  [Name], [Content], [Comment], [DateCreated], [DateModified], [Author] " +
                "from [Programs]");
                return db.Select(query);
            }
            catch (Exception ex)
            {
                throw new System.ApplicationException("Could not retrieve pgms", ex);
            }
        }
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="tableName"></param>
        ///// <returns></returns>
        //public override List<string> GetTableColumnNames(string viewName)
        //{
        //    try
        //    {
        //        Query query = db.CreateQuery("select  [Name] from " + viewName +
        //            " where [Type] in ('TEXT', 'TEXTBOX','UPPERCASE','MULTILINE', 'DATE','DATETIME','TIME','PHONENUMBER','NUMBER','COMBO','GROUP','OPTFRAME','AUTOIDNUM','CHECKBOX','IMAGE','YES/NO') ") ;

        //        DataTable columnsTable = db.Select(query);
        //        List<string> columnsList = new List<string>();
        //        foreach (DataRow row in columnsTable.Rows)
        //        {
        //            columnsList.Add(row["Name"].ToString());
        //        }
        //        return columnsList;
        //    }
        //    finally
        //    {
        //    }

        //}
        /// <summary>
        /// Inserts a pgm into metaPrograms
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <param name="comment"></param>
        /// <param name="author"></param>
        /// <returns></returns>
        //public override void InsertPgm(string name, string content, string comment, string author )
        public void InsertPgm(string name, string content, string comment, string author)
        {
            try
            {
                Query insertQuery = db.CreateQuery("insert into Programs([Name], [Content], [Comment],[DateCreated],[DateModified], [Author]) " +
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
                throw new System.ApplicationException("Could not insert pgm", ex);
            }
            finally
            {

            }
        }
        /// <summary>
        /// Deletes a program from the database
        /// </summary>
        /// <param name="pgmName">Name of the program to be deleted</param>
        //public override void DeletePgm(string pgmName)
        public void DeletePgm(string pgmName)
        {
            try
            {
                #region Input Validation

                if (string.IsNullOrEmpty(pgmName))
                {
                    throw new ArgumentOutOfRangeException("ProgramName");
                }
                #endregion
                Query deleteQuery = db.CreateQuery("delete  from [Programs] where [Name] = @Name");
                deleteQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, pgmName));

                db.ExecuteNonQuery(deleteQuery);
            }
            catch (Exception ex)
            {
                throw new System.ApplicationException("Could not delete pgm.", ex);
            }
            finally
            {

            }
        }
        /// <summary>
        /// Updates a program saved in the database
        /// </summary>
        /// <param name="name">Name of the pgm</param>
        /// <param name="content">Content of the program</param>
        /// <param name="comment">Comment for the program</param>
        /// <param name="author">Author of the program</param>
        //		public override void UpdatePgm(string name, string content, string comment, string author)
        public void UpdatePgm(string name, string content, string comment, string author)
        {
            try
            {
                #region Input Validation
                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentOutOfRangeException("Name");
                }
                #endregion
                Query updateQuery = db.CreateQuery("update [Programs] set [Content] = @Content , " +
                    "[Comment] = @Comment, [Author] = @Author" +
                    "where [Name] = @Name");

                updateQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, name));
                updateQuery.Parameters.Add(new QueryParameter("@Content", DbType.String, content));
                updateQuery.Parameters.Add(new QueryParameter("@Comment", DbType.String, comment));
                updateQuery.Parameters.Add(new QueryParameter("@DateModified", DbType.String, System.DateTime.Now.ToShortDateString()));
                updateQuery.Parameters.Add(new QueryParameter("@Author", DbType.String, author));

                db.ExecuteNonQuery(updateQuery);
            }
            catch (Exception ex)
            {
                throw new System.ApplicationException("Could not update pgm.", ex);
            }
            finally
            {

            }
        }

        #endregion Public Methods

        #region Private Methods
        private DataTable GetDataTableTemplateForViewInfo()
        {
            DataTable viewTableTemplate = new DataTable();
            viewTableTemplate.Columns.Add(ColumnNames.NAME, typeof(string));
            viewTableTemplate.Columns.Add(ColumnNames.DATA_TABLE_NAME, typeof(string));
            viewTableTemplate.Columns.Add(ColumnNames.CHECK_CODE_VARIABLE_DEFINITIONS, typeof(string));
            viewTableTemplate.Columns.Add(ColumnNames.CHECK_CODE_BEFORE, typeof(string));
            viewTableTemplate.Columns.Add(ColumnNames.CHECK_CODE_AFTER, typeof(string));
            viewTableTemplate.Columns.Add(ColumnNames.RECORD_CHECK_CODE_BEFORE, typeof(string));
            viewTableTemplate.Columns.Add(ColumnNames.RECORD_CHECK_CODE_AFTER, typeof(string));
            return viewTableTemplate;
        }
        #endregion Private Methods

        #region Static Methods

        /// <summary>
        /// Infers field type
        /// </summary>
        /// <param name="fieldRow"></param>
        /// <returns>MetaFieldType enumeration</returns>
        public static MetaFieldType InferFieldType(DataRow fieldRow)
        {
            string fieldType = fieldRow[ColumnNames.TYPE].ToString();
            string list = fieldRow[ColumnNames.LISTS].ToString();

            switch (fieldType.ToLowerInvariant())
            {
                case "autoidnum":
                    return MetaFieldType.Number;
                case "textbox":
                    return MetaFieldType.Text;
                case "text":
                    return MetaFieldType.Text;
                case "combo":
                    if (!string.IsNullOrEmpty(list))
                    {
                        if (list.ToLowerInvariant().StartsWith("w"))
                        {
                            return MetaFieldType.LegalValues;
                        }
                        else if (list.ToLowerInvariant().StartsWith("c"))
                        {
                            return MetaFieldType.Codes;
                        }
                        else
                        {
                            return MetaFieldType.CommentLegal;
                        }
                    }
                    else
                    {
                        throw new System.ApplicationException("Combo field is not one of the three types");
                    }
                case "textonly":
                    return MetaFieldType.LabelTitle;
                case "uppercase":
                    return MetaFieldType.TextUppercase;
                case "multiline":
                    return MetaFieldType.Multiline;
                case "number":
                    return MetaFieldType.Number;
                case "phonenumber":
                    return MetaFieldType.PhoneNumber;
                case "date":
                    return MetaFieldType.Date;
                case "time":
                    return MetaFieldType.Time;
                case "datetime":
                    return MetaFieldType.DateTime;
                case "checkbox":
                    return MetaFieldType.Checkbox;
                case "yes/no":
                    return MetaFieldType.YesNo;
                case "optframe":
                    return MetaFieldType.Option;
                case "commandbutton":
                    return MetaFieldType.CommandButton;
                case "image":
                    return MetaFieldType.Image;
                case "mirror":
                    return MetaFieldType.Mirror;
                case "grid":
                    return MetaFieldType.Grid;
                case "relate":
                    return MetaFieldType.Relate;
                case "group":
                    return MetaFieldType.Group;
                default:
                    throw new System.ApplicationException("Unknown field found in Epi 2000 database: " + fieldType);
            }
        }

        /// <summary>
        /// Splits a block of check code into before and after
        /// </summary>
        /// <param name="originalCheckCode"></param>
        /// <param name="checkCodeBefore"></param>
        /// <param name="checkCodeAfter"></param>
        public static void SplitCheckCode(string originalCheckCode, ref string checkCodeBefore, ref string checkCodeAfter)
        {
            originalCheckCode = originalCheckCode.Trim();
            if (!string.IsNullOrEmpty(originalCheckCode))
            {
                Regex regex = new Regex("^ENDBEFORE", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (regex.IsMatch(originalCheckCode))
                {
                    string[] codeBlocks = regex.Split(originalCheckCode);
                    if (codeBlocks.Length >= 1)
                    {
                        checkCodeBefore = codeBlocks[0].Trim();
                    }
                    if (codeBlocks.Length == 2)
                    {
                        checkCodeAfter = codeBlocks[1].Trim();
                    }
                }
                else
                {
                    checkCodeAfter = originalCheckCode;
                }
            }
        }
        #endregion Static Methods


        /// <summary>
        /// Gets list of code table names in the metadata database
        /// </summary>
        /// <returns>DataRow of table names</returns>
        public List<string> GetCodeTableNames()
        {
            try
            {
                DataTable tables = db.GetTableSchema();
                List<string> retval = null;
                int count = tables.Rows.Count;
                if (count > 0)
                {
                    retval = new List<string>();
                    DataRow[] rowsFiltered = tables.Select("TABLE_NAME like 'code%'");
                    int index = tables.Columns.IndexOf("TABLE_NAME");
                    foreach (DataRow row in rowsFiltered)
                    {
                        retval.Add(row[index].ToString());
                    }
                }
                return retval;
            }
            catch (Exception ex)
            {
                throw new System.ApplicationException("Could not retrieve code tables from database", ex);
            }
        }

        /// <summary>
        /// Gets list of code tables in the metadata database
        /// </summary>
        /// <returns>DataRow of table names</returns>
        [Obsolete("Use of DataTable in this context is no different than the use of a multidimensional System.Object array (not recommended).", false)]
        public DataTable GetCodeTableList()
        {
            try
            {
                DataTable tables = db.GetTableSchema();

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

        /// <summary>
        /// Get the Code Table Column Names
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public string[] GetCodeTableColumnNames(string tableName)
        {
            string[] retval = null;
            Util.Assert(tableName.StartsWith("meta") == false);
            DataTable dt = db.GetTableColumnSchema(tableName);

            int count = dt.Rows.Count;
            if (dt.Rows.Count > 0)
            {
                int index = dt.Columns.IndexOf("COLUMN_NAME");
                retval = new string[count];
                for (int i = 0; i < count; i++)
                {
                    retval[i] = dt.Rows[i][index].ToString();
                }
            }
            return retval;
        }
       // /// <summary>
       // /// Returns the Code Table Columns Schema as a datatable
       // /// </summary>
       // /// <param name="tableName"></param>
       // /// <returns></returns>
       //[Obsolete("Use of DataTable in this context is no different than the use of a multidimensional System.Object array (not recommended).", false)]
       // public DataTable GetCodeTableColumnSchema(string tableName)
       // {
       //     Util.Assert(tableName.StartsWith("meta") == false);
       //     return db.GetTableColumnSchema(tableName);
       // }

        /// <summary>
        /// Returns the data in the code table as a DataTable
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DataTable GetCodeTableData(string tableName)
        {
            Util.Assert(tableName.StartsWith("meta") == false);
            return db.GetTableData(tableName);
        }

        ///// <summary>
        ///// Creates a view in a specified project
        ///// </summary>
        ///// <old-param name="isrelatedview">Whether or not this view is a related (child) view</old-param>
        ///// <old-param name="viewname">Name of the view</old-param>
        //public void InsertView(View view)
        //{
        //    #region Input Validation
        //    if (view == null)
        //    {
        //        throw new ArgumentNullException("view");
        //    }
        //    #endregion Input Validation

        //    try
        //    {

        //        Query insertQuery = db.CreateQuery("insert into metaViews([Name], [IsRelatedView]) values (@Name, @IsRelatedView)");

        //        insertQuery.Parameters.Add(new QueryParameter("@Name", DbType.String, view.Name));
        //        insertQuery.Parameters.Add(new QueryParameter("@IsRelatedView", DbType.Boolean, view.IsRelatedView));


        //        db.ExecuteNonQuery(insertQuery);
        //        view.Id = this.GetMaxViewId();

        //        // Insert system fields .. RECSTATUS and UNIQUEKEY
        //        RecStatusField recStatusField = new RecStatusField(view);
        //        UniqueKeyField uniqueKeyField = new UniqueKeyField(view);
        //        uniqueKeyField.SaveToDb();
        //        recStatusField.SaveToDb();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new GeneralException("Could not create view in the database", ex);
        //    }
        //    finally
        //    {

        //    }
        //}

        /// <summary>
        /// Returns the Id of the last view added
        /// </summary>
        /// <returns></returns>
        public int GetMaxViewId()
        {
            Query selectQuery = db.CreateQuery("select MAX(ViewId) from metaViews");
            return (int)db.ExecuteScalar(selectQuery);
        }

    }
}
