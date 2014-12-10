using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Text;
using Epi;
using Epi.Data;
using Epi.Fields;
using Epi.DataSets;
using System.Text.RegularExpressions;
using Epi.Resources;

namespace Epi.Data.Services
{
    /// <summary>
    /// Data access class for Epi Info collected data
    /// </summary>
    /// 
    public class CollectedDataProvider
    {
        private bool isWebMode;

        public bool IsWebMode
        {
            get { return this.isWebMode; }
            set { this.isWebMode = value; }
        }

        public int StartingId
        {
            get;
            set;
        }

        #region Fields
        private Project project;
        /// <summary>
        /// The underlying physical database
        /// </summary>
        protected IDbDriver dbDriver;
        protected IDbDriver dbDriverInitialRead;
        private IDbDriverFactory dbFactory;
        //private DbConnectionStringBuilder cnnStringBuilder;
        #endregion Fields

        #region Constructors

        /// <summary>
        /// Constructor for the class.
        /// </summary>
        /// <param name="proj">Project the collected data belongs to</param>
        // public CollectedDataProvider(Project proj, bool createDatabase)
        public CollectedDataProvider(Project proj)
        {
            if (proj == null)
            {
                throw new System.ArgumentNullException("proj");
            }

            project = proj;
        }

        #endregion  //Constructors

        #region Public Methods
        /// <summary>
        /// Creates database 
        /// </summary>
        /// <param name="collectDbInfo">Database information.</param>
        /// <param name="driver">Database driver.</param>
        /// <param name="createDatabase">Create database.</param>
        public void Initialize(DbDriverInfo collectDbInfo, string driver, bool createDatabase)
        {
            dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(driver);
            dbDriver = dbFactory.CreateDatabaseObject(collectDbInfo.DBCnnStringBuilder);
            dbDriverInitialRead = dbFactory.CreateDatabaseObject(collectDbInfo.DBCnnStringBuilder);

            if (createDatabase)
            {
                this.CreateDatabase(collectDbInfo);
            }
        }

        /// <summary>
        /// Returns the current db driver.
        /// </summary>
        /// <returns></returns>
        public IDbDriver GetDbDriver()
        {
            return this.dbDriver;
        }

        #endregion

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

        #endregion //Public Properties

        #region Public Methods
        
        /// <summary>
        /// Is there a column in collected data for each field in metadata with matching [Name] and [Type].
        /// </summary>
        /// <param name="view">View of a table.</param>
        [Obsolete("call the appropriate synchronize method instead", true)]
        public bool IsSynchronized(View view)
        {
            if (dbDriver.TableExists(view.TableName) == false)
            {
                return false;
            }

            DataTable pages = view.GetMetadata().GetPagesForView(view.Id);
            string pageTableName = string.Empty;
            int pageId = 0;

            foreach (DataRow page in pages.Rows)
            {
                pageId = (int)page["PageId"];

                pageTableName = string.Format("{0}{1}", view.Name, pageId);

                DataTable metadataTable = view.GetMetadata().GetFieldMetadataSync(pageId);
                DataTable fieldTypesTable = view.GetMetadata().GetFieldTypes();
                List<string> columnNames = dbDriver.GetTableColumnNames(pageTableName);
                int fieldType;
                int dataTypeId;
                string query = string.Empty;
                DataRow[] fieldTypesRows;

                foreach (DataRow row in metadataTable.Rows)
                {
                    fieldType = (int)row["FieldTypeId"];
                    query = string.Format("FieldTypeId = {0}", (int)row["FieldTypeId"]);
                    fieldTypesRows = fieldTypesTable.Select(query);
                
                    if (fieldTypesRows.Length == 0)
                    {
                        continue;
                    }
                
                    dataTypeId = (int)fieldTypesRows[0]["DataTypeId"];
                
                    if ((fieldType == (int)MetaFieldType.LabelTitle) ||
                        (fieldType == (int)MetaFieldType.Group) ||
                        (fieldType == (int)MetaFieldType.CommandButton) ||
                        (fieldType == (int)MetaFieldType.Mirror))
                    {
                        continue; 
                    }

                    if (fieldType == (int)MetaFieldType.Grid)
                    {
                        IsGridSynchronized();
                    }

                    if (columnNames.Contains((string)row[ColumnNames.NAME]) == false &&
                        columnNames.Contains(((string)row[ColumnNames.NAME]).ToUpper()) == false)
                    {
                        return false; 
                    }
                }
            }

            return true;
        }

        public bool IsGridSynchronized()
        {
            return true;
        }

        /// <summary>
        /// Change the type of the column in current database
        /// </summary>
        /// <param name="tableName">name of the table</param>
        /// <param name="columnName">name of the column</param>
        /// <param name="columnType">new name of the column</param>
        /// <returns>Boolean</returns>
        public void AlterColumnType(string tableName, string columnName, string columnType)
        {
            dbDriver.AlterColumnType(tableName, columnName, columnType);
        }
        
        /// <summary>
        /// dpb - consider deprecating this method. While it may work with
        /// other databases, it may fail to return a valid bool when using
        /// Access unless when used right after Compact(...)
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool isViewTableFull(string tableName)
        {
            if ( GetTableColumnCount(tableName) >= TableColumnMax)
            {
                return true;
            }
            return false;
        }

        public bool CheckCollectedDataIntegrity(View view)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            string queryString = "select count(*) from " + dbDriver.InsertInEscape(view.TableName);
            Query query = dbDriver.CreateQuery(queryString);
            int masterRecordCount = Int32.Parse((dbDriver.ExecuteScalar(query)).ToString());

            foreach (Page page in view.Pages)
            {
                queryString = "select count(*) from " + dbDriver.InsertInEscape(page.TableName);
                query = dbDriver.CreateQuery(queryString);

                if (masterRecordCount != Int32.Parse((dbDriver.ExecuteScalar(query)).ToString()))
                {
                    return false;
                }
            }

            return true;
        }

        public void SuggestRenameFieldInCollectedData(Field field, string nameBeforeEdit)
        {
            if(TableExists(field.GetView().TableName))
            {
                if (dbDriver.ColumnExists(field.GetView().TableName, nameBeforeEdit))
                {
                    RenameColumn(nameBeforeEdit, field.Name);
                }
            }
        }
       
        /// <summary>
        /// Synchronizes metadata and data table 
        /// [drops and/or inserts data columns in the view table]
        /// </summary>
        public void SynchronizeDataTable(View view, bool tryCompact = true)
        {
            try
            {
                if (isWebMode)
                {
                    return; 
                }

                Boolean noDataTable = dbDriver.TableExists(view.TableName) == false;

                if (noDataTable)
                {
                    CreateDataTableForView(view, 1);
                }
                else
                {
                    foreach (Page page in view.Pages)
                    {
                        DataTable fieldMetadataSync = view.GetMetadata().GetFieldMetadataSync(page.Id);
                        DeleteUndefinedDataFields(page, fieldMetadataSync);
                    }

                    if (tryCompact)
                    {
                        try
                        {
                            dbDriver.CompactDatabase();
                            if (dbDriver.FullName.Contains("[MS Access]"))
                            {
                                string insertStatement = string.Format("insert into {0}([UniqueKey], [GlobalRecordId]) values (@UniqueKey, @GlobalRecordId)", view.TableName);
                                Query insertQuery = dbDriver.CreateQuery(insertStatement);
                                insertQuery.Parameters.Add(new QueryParameter("@UniqueKey", DbType.Int16, StartingId - 1));
                                insertQuery.Parameters.Add(new QueryParameter("@GlobalRecordId", DbType.String, ""));
                                dbDriver.ExecuteNonQuery(insertQuery);

                                string deleteStatement = string.Format("delete from {0} where UniqueKey={1} ", view.TableName, StartingId - 1);
                                Query deleteQuery = dbDriver.CreateQuery(deleteStatement);
                                dbDriver.ExecuteNonQuery(deleteQuery);
                            }
                        }
                        catch { }
                    }

                    view.SetTableName(view.TableName);
                    
                    foreach (Page page in view.Pages)
                    {
                        SynchronizePageTable(page);
                    }
                }
            }
            catch { }
        }

        public bool DeleteUndefinedDataFields(View view, DataTable fieldMetadataSync)
        {
            bool fieldDeleted = false;

            List<string> columnsToDrop = new List<string>();

            if (dbDriver.TableExists(view.Name))
            {
                try
                {
                    List<string> definedFields = new List<string>();

                    int max = view.Project.CollectedData.TableColumnMax;
                    int actualInDataTable = view.Project.CollectedData.GetTableColumnCount(view.Name);

                    // << build a list of the fields defined in metadata >>
                    foreach (DataRow row in fieldMetadataSync.Rows)
                    {
                        definedFields.Add(row["Name"].ToString().ToUpper());
                    }

                    List<string> columnNames = dbDriver.GetTableColumnNames(view.Name);

                    // << build a list of the fields that should be dropped from the view table >>
                    foreach (string columnName in columnNames)
                    {
                        if (definedFields.Contains(columnName.ToUpper()) == false)
                        {
                            columnsToDrop.Add(columnName);
                        }
                    }
                    
                    // << drop all the fields in the view table that are not defined in metadata >>
                    foreach(string drop in columnsToDrop)
                    {
                        if (dbDriver.DeleteColumn(view.Name, drop))
                        {
                            fieldDeleted = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new GeneralException("Could delete the field from the data table.", e);
                }
            }
            return fieldDeleted;
        }


        public bool DeleteUndefinedDataFields(Page page, DataTable fieldMetadataSync)
        {
            bool fieldDeleted = false;

            List<string> columnsToDrop = new List<string>();
            string TableName = page.TableName;
            if (dbDriver.TableExists(TableName))
            {
                try
                {
                    List<string> definedFields = new List<string>();

                    // << build a list of the fields defined in metadata >>
                    definedFields.Add("GLOBALRECORDID");
                    foreach (DataRow row in fieldMetadataSync.Rows)
                    {
                        definedFields.Add(row["Name"].ToString().ToUpper());
                    }

                    List<string> columnNames = dbDriver.GetTableColumnNames(TableName);

                    // << build a list of the fields that should be dropped from the view table >>
                    foreach (string columnName in columnNames)
                    {
                        if (definedFields.Contains(columnName.ToUpper()) == false)
                        {
                            columnsToDrop.Add(columnName);
                        }
                    }

                    // << drop all the fields in the view table that are not defined in metadata >>
                    foreach (string drop in columnsToDrop)
                    {
                        if (dbDriver.DeleteColumn(TableName, drop))
                        {
                            fieldDeleted = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new GeneralException("Could delete the field from the data table.", e);
                }
            }
            return fieldDeleted;
        }

        /// <summary>
        /// Creates the SysDataTable for collected data
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public void CreateSysDataTable(string tableName)
        {
            #region Input validation
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("Table Name");
            }
            #endregion

            Query createQuery = dbDriver.CreateQuery("create table " + dbDriver.SchemaPrefix + "sysDataTables " +
                " (TableName varchar(64) primary key not null, ViewId tinyint not null)");

            dbDriver.ExecuteNonQuery(createQuery);
        }

        /// <summary>
        /// Gets primary view's Id.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>View's Id</returns>
        public int GetPrimaryViewId(string tableName)
        {
            try
            {
                Query query = dbDriver.CreateQuery("select distinct [ViewId] from sysDataTables where tableName = @tableName");
                query.Parameters.Add(new QueryParameter("@tableName", DbType.String, tableName));

                return (int)dbDriver.ExecuteScalar(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve view Id", ex);
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

                Query query = dbDriver.CreateQuery("insert into metaDBInfo([ProjectId], [ProjectName], [ProjectLocation], [EpiVersion], [Purpose]) values (@ProjectId, @ProjectName, @ProjectLocation, @EpiVersion, @Purpose)");
                query.Parameters.Add(new QueryParameter("@ProjectId", DbType.Guid, this.Project.Id));
                query.Parameters.Add(new QueryParameter("@ProjectName", DbType.String, Project.Name));
                query.Parameters.Add(new QueryParameter("@ProjectLocation", DbType.String, Project.Location));
                query.Parameters.Add(new QueryParameter("@EpiVersion", DbType.String, appId.Version));

                if (Project.UseMetadataDbForCollectedData)
                    query.Parameters.Add(new QueryParameter("@Purpose", DbType.Int32, (int)DatabasePurpose.MetadataAndCollectedData));
                else
                    query.Parameters.Add(new QueryParameter("@Purpose", DbType.Int32, (int)DatabasePurpose.CollectedData));


                dbDriver.ExecuteNonQuery(query);
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
        /// Return the number of colums in the specified table
        /// </summary>
        /// <remarks>
        /// Originaly intended to be used to keep view tables from getting to wide.
        /// </remarks>
        /// <param name="tableName"></param>
        /// <returns>the number of columns in the </returns>
        public int GetTableColumnCount(string tableName)
        {
            return dbDriver.GetTableColumnCount(tableName);
        }

        /// <summary>
        /// Gets the number of tables in the database.
        /// </summary>
        /// <returns>The number of tables in the database.</returns>
        public int GetTableCount()
        {
            return dbDriver.GetTableCount();
        }

        /// <summary>
        /// Get table column names
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>List of column names.</returns>
        public List<System.String> GetTableColumnNames(string tableName)
        {
            return dbDriver.GetTableColumnNames(tableName);
        }

        /// <summary>
        /// Creates physical database.
        /// </summary>
        /// <param name="dbInfo">Database driver information.</param>
        public void CreateDatabase(DbDriverInfo dbInfo)
        {
            dbFactory.CreatePhysicalDatabase(dbInfo);
        }

        /// <summary>
        /// Deletes a specific column in the database.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="columnName">Name of the column</param>
        /// <returns>Results of a successful deletion.</returns>
        public bool DeleteColumn(string tableName, string columnName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("tableName");
            }
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentException("columnName");
            }

            if (dbDriver.ColumnExists(tableName, columnName))
            {
                return dbDriver.DeleteColumn(tableName, columnName);
            }

            return true;
        }
        
        /// <summary>
        /// Deletes a specific table in the database.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>Results of a successful deletion.</returns>
        public bool DeleteTable(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("tableName");
            }

            return dbDriver.DeleteTable(tableName);
        }

        /// <summary>
        /// Gets a value indicating whether or not a specific table exists in the database
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>Results of successful table existence test.</returns>
        public bool TableExists(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("tableName");
            }
            return dbDriver.TableExists(tableName);
        }

        /// <summary>
        /// Gets primary_key schema information about an OLE table.
        /// </summary>
        /// <param name="tableName">The name of the table</param>        
        /// <returns>Represents one table of in-memory data.</returns>
        public DataTable GetTableColumnSchema(string tableName)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("tableName");
            }
            #endregion
            DataTable dtColumnSchema;
            try
            {
                dtColumnSchema = dbDriver.GetTableColumnSchema(tableName);
            }
            catch (Exception)
            {
                dtColumnSchema = dbDriver.GetTableColumnSchemaANSI(tableName);
            }
            return dtColumnSchema;
        }


        /// <summary>
        /// Gets an OLE-table as a DataReader.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>Forward-only stream of result sets obtained by executing a command.</returns>
        public IDataReader GetTableDataReader(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("tableName");
            }

            return dbDriver.GetTableDataReader(tableName);
        }

        /// <summary>
        /// Gets column names as text in a System.Data.DataView.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>Represents a databindable, customized view of a <see cref="System.Data.DataTable"/></returns>
        public DataView GetTextColumnNames(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("tableName");
            }

            return dbDriver.GetTextColumnNames(tableName);
        }

        /// <summary>
        /// Returns the database instance.
        /// </summary>
        /// <returns>Abstract data type for databases.</returns>
        public IDbDriver GetDatabase()
        {
            return this.dbDriver;
        }

        /// <summary>
        /// Insert record into table.
        /// </summary>
        /// <param name="view">View of a table.</param>
        /// <param name="reader">Forward-only stream of result sets obtained by executing a command.</param>
        public void SaveRecord(View view, IDataReader reader)
        {
            try
            {
                bool hasData = false;
                WordBuilder fieldNames = new WordBuilder();
                WordBuilder fieldValues = new WordBuilder();
                fieldNames.Delimitter = StringLiterals.COMMA;
                fieldValues.Delimitter = StringLiterals.COMMA;

                foreach (Field field in view.Fields.TableColumnFields)
                {
                    // Eliminate UniqueKeyFields. They are not inserted explicitly.
                    if (!(field is UniqueKeyField))
                    {
                        if (!Util.IsEmpty(reader[field.Name]))
                        {
                            hasData = true;
                            fieldNames.Append(dbDriver.InsertInEscape(field.Name));
                            Object fieldValue = reader[field.Name];
                            string fieldValueString = string.Empty;
                            // string fieldValue = reader[field.Name].ToString();

                            // TODO: Determine if the value is null. If so, no further formatting required.

                            if (field is DateField)
                            {
                                fieldValueString = dbDriver.FormatDate((DateTime)fieldValue);
                            }
                            else if (field is TimeField)
                            {
                                fieldValueString = dbDriver.FormatTime((DateTime)fieldValue);
                            }
                            else if (field is DateTimeField)
                            {
                                fieldValueString = dbDriver.FormatDateTime((DateTime)fieldValue);
                            }
                            else if ((field is PhoneNumberField) || (field is TableBasedDropDownField) || (field is TextField))
                            {
                                fieldValueString = Util.InsertInSingleQuotes(fieldValue.ToString());
                            }
                            else if (field is CheckBoxField)
                            {
                                bool checkboxValue = bool.Parse(fieldValue.ToString());
                                if (checkboxValue)
                                {
                                    fieldValueString = "1";
                                }
                                else
                                {
                                    fieldValueString = "0";
                                }
                            }
                            else
                            {
                                fieldValueString = fieldValue.ToString();
                            }
                            fieldValues.Append(fieldValueString);
                        }
                    }
                }

                // Concatenate the query clauses into one SQL statement.
                string queryString = "insert into ";
                queryString += dbDriver.InsertInEscape(view.TableName) + StringLiterals.SPACE;
                queryString += Util.InsertInParantheses(fieldNames.ToString());
                queryString += StringLiterals.SPACE + "values" + StringLiterals.SPACE;
                queryString += Util.InsertInParantheses(fieldValues.ToString());

                Query insertQuery = dbDriver.CreateQuery(queryString);

                if (hasData)
                {
                    dbDriver.ExecuteNonQuery(insertQuery);
                }
            }
            finally
            {
            }
        }

        /// <summary>
        /// Insert record into table.
        /// </summary>
        /// <param name="view">View of a table.</param>
        /// <returns>Unique key of the record.</returns>
        public int SaveRecord(View view)
        {
            Configuration config = Configuration.GetNewInstance();

            try
            {
                WordBuilder fieldNames = new WordBuilder(StringLiterals.COMMA);
                WordBuilder fieldValues = new WordBuilder(StringLiterals.COMMA);
                List<QueryParameter> fieldValueParams = new List<QueryParameter>();
                foreach (IDataField dataField in view.Fields.DataFields)
                {
                    if (dataField is GlobalRecordIdField || dataField is ForeignKeyField)
                    {
                        fieldNames.Append(dbDriver.InsertInEscape(((Epi.INamedObject)dataField).Name));

                        if (dataField.CurrentRecordValueObject == null)
                        {
                            fieldValues.Append(" null ");
                        }
                        else if (string.IsNullOrEmpty(dataField.CurrentRecordValueObject.ToString()))
                        {
                            fieldValues.Append(" null ");
                        }
                        else
                        {
                            String fieldName = ((Epi.INamedObject)dataField).Name;
                            fieldValues.Append("@" + fieldName);
                            fieldValueParams.Add(dataField.CurrentRecordValueAsQueryParameter);
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

                System.Security.Principal.WindowsIdentity winId = System.Security.Principal.WindowsIdentity.GetCurrent();
                String winIdName = winId.Name;

                bool hasSaveColumns = dbDriver.ColumnExists(view.TableName, ColumnNames.RECORD_FIRST_SAVE_LOGON_NAME);

                StringBuilder sb = new StringBuilder();
                sb.Append(" insert into ");
                sb.Append(dbDriver.InsertInEscape(view.TableName));
                sb.Append(StringLiterals.SPACE);
                
                if (hasSaveColumns)
                {
                    sb.Append(Util.InsertInParantheses(fieldNames.ToString() + ", [" + ColumnNames.RECORD_FIRST_SAVE_LOGON_NAME + "], [" + ColumnNames.RECORD_FIRST_SAVE_TIME + "]"));
                    sb.Append(" values (");
                    sb.Append(fieldValues.ToString() + ", @" + ColumnNames.RECORD_FIRST_SAVE_LOGON_NAME +", @" + ColumnNames.RECORD_FIRST_SAVE_TIME );
                }
                else
                {
                    sb.Append(Util.InsertInParantheses(fieldNames.ToString()));
                    sb.Append(" values (");
                    sb.Append(fieldValues.ToString());
                }

                sb.Append(") ");
                Query insertQuery = dbDriver.CreateQuery(sb.ToString());

                insertQuery.Parameters = fieldValueParams;

                if (hasSaveColumns)
                {
                    QueryParameter name = new QueryParameter("@" + ColumnNames.RECORD_FIRST_SAVE_LOGON_NAME, DbType.String, winIdName);
                    insertQuery.Parameters.Add(name);
                    QueryParameter time = new QueryParameter("@" + ColumnNames.RECORD_FIRST_SAVE_TIME, DbType.DateTime, DateTime.Now.ToString());
                    insertQuery.Parameters.Add(time);
                }

                dbDriver.ExecuteNonQuery(insertQuery);

                if (hasSaveColumns)
                {
                    UpdateBaseTable(view);
                }

                string relatedViewFilter = string.Empty;
                if (view.IsRelatedView)
                {
                    relatedViewFilter = " where ";
                    relatedViewFilter += dbDriver.InsertInEscape(ColumnNames.FOREIGN_KEY);
                    relatedViewFilter += StringLiterals.EQUAL + "'" + view.ForeignKeyField.CurrentRecordValueString + "'";
                }

                Query selectQuery = dbDriver.CreateQuery(" select" +
                    Util.InsertIn("max" + Util.InsertInParantheses(dbDriver.InsertInEscape(ColumnNames.UNIQUE_KEY)), StringLiterals.SPACE) +
                    "from" + StringLiterals.SPACE + dbDriver.InsertInEscape(view.TableName) +
                    relatedViewFilter);

                foreach (Epi.Page page in view.Pages)
                {
                    this.SaveRecord(page);
                }

                int recordID; 
                int.TryParse(dbDriver.ExecuteScalar(selectQuery).ToString(), out recordID);
                foreach (GridField grid in view.Fields.GridFields)
                {
                    this.SaveGridRecord(view, recordID, grid, grid.DataSource);
                    grid.DataSource = null;
                }
                return recordID;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Insert record into table.
        /// </summary>
        /// <param name="view">View of a table.</param>
        /// <returns>Unique key of the record.</returns>
        public int SaveRecord(Page page)
        {
            if (TableExists(page.TableName) == false)
            {
                return 0;
            }
            
            Configuration config = Configuration.GetNewInstance();

            try
            {
                WordBuilder fieldNames = new WordBuilder(StringLiterals.COMMA);
                WordBuilder fieldValues = new WordBuilder(StringLiterals.COMMA);
                List<QueryParameter> fieldValueParams = new List<QueryParameter>();

                fieldNames.Append("GlobalRecordId");
                fieldValues.Append("@GlobalRecordId");
                fieldValueParams.Add(page.view.GlobalRecordIdField.CurrentRecordValueAsQueryParameter);

                foreach (RenderableField renderableField in page.Fields)
                {
                    if (renderableField is GridField || renderableField is GroupField)
                    {
                        continue;
                    }
                    else if (renderableField is IDataField)
                    {
                        IDataField dataField = (IDataField)renderableField;
                        if (dataField is UniqueKeyField)
                        {
                            continue;
                        }
                        else
                        {
                            fieldNames.Append(dbDriver.InsertInEscape(((Epi.INamedObject)dataField).Name));

                            if (dataField.CurrentRecordValueObject == null)
                            {
                                fieldValues.Append(" null ");
                            }
                            else if (string.IsNullOrEmpty(dataField.CurrentRecordValueObject.ToString()))
                            {
                                fieldValues.Append(" null ");
                            }
                            else
                            {
                                String fieldName = ((Epi.INamedObject)dataField).Name;
                                fieldValues.Append("@" + fieldName);
                                fieldValueParams.Add(dataField.CurrentRecordValueAsQueryParameter);
                            }
                        }
                    }
                    else
                    {

                    }
                }

                StringBuilder sb = new StringBuilder();
                sb.Append(" insert into ");
                sb.Append(dbDriver.InsertInEscape(page.TableName));
                sb.Append(StringLiterals.SPACE);
                sb.Append(Util.InsertInParantheses(fieldNames.ToString()));
                sb.Append(" values (");
                sb.Append(fieldValues.ToString());
                sb.Append(") ");
                Query insertQuery = dbDriver.CreateQuery(sb.ToString());
                insertQuery.Parameters = fieldValueParams;

                dbDriver.ExecuteNonQuery(insertQuery);

                return 0;
            }
            finally
            {

            }
        }

        /// <summary>
        /// Saves the current record
        /// </summary>
        /// <param name="view">View of a table.</param>
        /// <param name="recordID">Id of record.</param>
        public int SaveRecord(View view, int recordID)
        {
            string updateHeader = string.Empty;
            string whereClause = string.Empty;

            try
            {
                #region Input Validation
                if (view == null)
                {
                    throw new ArgumentOutOfRangeException("View");
                }
                if (recordID < 1)
                {
                    throw new ArgumentOutOfRangeException("Record ID");
                }
                #endregion 

                UpdateBaseTable(view);

                foreach (Page page in view.Pages)
                {
                    this.SaveRecord(page, view.CurrentGlobalRecordId);
                }

                foreach (GridField gridField in view.Fields.GridFields)
                {
                    if (gridField.DataSource != null)
                    {
                        gridField.DataSource.AcceptChanges();
                        this.SaveGridRecord(view, recordID, gridField, gridField.DataSource);
                    }
                    gridField.DataSource = null;
                }
                
                return recordID;
            }
            finally
            {
            }
        }
        
        void UpdateBaseTable(View view)
        {
            bool hasColumns = dbDriver.ColumnExists(view.TableName, ColumnNames.RECORD_LAST_SAVE_LOGON_NAME);

            if (hasColumns)
            {
                System.Security.Principal.WindowsIdentity winId = System.Security.Principal.WindowsIdentity.GetCurrent();
                String winIdName = winId.Name;
            
                StringBuilder sbUpdate = new StringBuilder();
                sbUpdate.Append("update " + view.TableName);
                sbUpdate.Append(Util.InsertIn("set", StringLiterals.SPACE));
                
                sbUpdate.Append(StringLiterals.LEFT_SQUARE_BRACKET);
                sbUpdate.Append(ColumnNames.RECORD_LAST_SAVE_LOGON_NAME);
                sbUpdate.Append(StringLiterals.RIGHT_SQUARE_BRACKET);
                sbUpdate.Append(StringLiterals.EQUAL);
                sbUpdate.Append(Util.InsertInSingleQuotes(winIdName));

                sbUpdate.Append(", ");

                sbUpdate.Append(StringLiterals.LEFT_SQUARE_BRACKET);
                sbUpdate.Append(ColumnNames.RECORD_LAST_SAVE_TIME);
                sbUpdate.Append(StringLiterals.RIGHT_SQUARE_BRACKET);
                sbUpdate.Append(StringLiterals.EQUAL);
                sbUpdate.Append("@" + ColumnNames.RECORD_LAST_SAVE_TIME);


                sbUpdate.Append(", ");

                sbUpdate.Append(StringLiterals.LEFT_SQUARE_BRACKET);
                sbUpdate.Append(ColumnNames.REC_STATUS);
                sbUpdate.Append(StringLiterals.RIGHT_SQUARE_BRACKET);
                sbUpdate.Append(StringLiterals.EQUAL);
                sbUpdate.Append("@" + ColumnNames.REC_STATUS);


                sbUpdate.Append(Util.InsertIn("where", StringLiterals.SPACE));
                sbUpdate.Append(ColumnNames.GLOBAL_RECORD_ID + StringLiterals.EQUAL);

                sbUpdate.Append(Util.InsertInSingleQuotes(view.CurrentGlobalRecordId));

                Query updateQuery = dbDriver.CreateQuery(sbUpdate.ToString());

                QueryParameter time = new QueryParameter("@" + ColumnNames.RECORD_LAST_SAVE_TIME, DbType.DateTime, DateTime.Now.ToString());
                updateQuery.Parameters.Add(time);

                QueryParameter RecStatus = new QueryParameter("@" + ColumnNames.REC_STATUS, DbType.Int32, view.RecStatusField.CurrentRecordValue);
                updateQuery.Parameters.Add(RecStatus);

                dbDriver.ExecuteNonQuery(updateQuery);
            }
        }

        /// <summary>
        /// Saves the current record
        /// </summary>
        public int SaveRecord(Page page, string GlobalRecordID)
        {
            int columnIndex = 0;
            List<QueryParameter> fieldValueParams = null;
            Query query = null;
            StringBuilder sb = null;
            string updateHeader = string.Empty;
            string whereClause = string.Empty;

            try
            {
                #region Input Validation
                if (page == null)
                {
                    throw new ArgumentOutOfRangeException("View");
                }
                if (GlobalRecordID == "")
                {
                    throw new ArgumentOutOfRangeException("Global Record ID");
                }
                #endregion //Input Validation

                fieldValueParams = new List<QueryParameter>();
                sb = new StringBuilder();

                // Build the Update statement which will be reused
                sb.Append(SqlKeyWords.UPDATE);
                sb.Append(StringLiterals.SPACE);
                sb.Append(dbDriver.InsertInEscape(page.TableName));
                sb.Append(StringLiterals.SPACE);
                sb.Append(SqlKeyWords.SET);
                sb.Append(StringLiterals.SPACE);

                updateHeader = sb.ToString();

                sb.Remove(0, sb.ToString().Length);

                // Build the WHERE caluse which will be reused
                sb.Append(SqlKeyWords.WHERE);
                sb.Append(StringLiterals.SPACE);
                sb.Append(dbDriver.InsertInEscape(ColumnNames.GLOBAL_RECORD_ID));
                sb.Append(StringLiterals.EQUAL);
                sb.Append("'");
                sb.Append(GlobalRecordID);
                sb.Append("'");
                whereClause = sb.ToString();

                sb.Remove(0, sb.ToString().Length);

                // Now build the field update statements in 100 field chunks
                foreach (RenderableField renderableField in page.Fields)
                {
                    if (renderableField is GridField || renderableField is GroupField)
                    {
                        continue;
                    }
                    else if (renderableField is IDataField)
                    {
                        IDataField dataField = (IDataField)renderableField;
                        if (dataField.FieldType != MetaFieldType.UniqueKey && dataField is RenderableField)
                        {
                            columnIndex += 1;

                            sb.Append(StringLiterals.LEFT_SQUARE_BRACKET);
                            sb.Append(((Epi.INamedObject)dataField).Name);
                            sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET);
                            sb.Append(StringLiterals.EQUAL);

                            if (dataField.CurrentRecordValueObject == null)
                            {
                                sb.Append(SqlKeyWords.NULL);
                            }
                            else if (string.IsNullOrEmpty(dataField.CurrentRecordValueObject.ToString()))
                            {
                                sb.Append(SqlKeyWords.NULL);
                            }
                            else
                            {
                                sb.Append(StringLiterals.COMMERCIAL_AT);
                                sb.Append(((Epi.INamedObject)dataField).Name);

                                if (   dataField.FieldType == MetaFieldType.Time 
                                    || dataField.FieldType == MetaFieldType.DateTime
                                    || dataField.FieldType == MetaFieldType.Date)
                                {
                                    if (!string.IsNullOrEmpty(dataField.CurrentRecordValueString))
                                    {
                                        DateTime org = (DateTime)dataField.CurrentRecordValueObject;
                                        DateTime truncatedDateTime = new DateTime(org.Year, org.Month, org.Day, org.Hour, org.Minute, org.Second);

                                        dataField.CurrentRecordValueObject = truncatedDateTime;
                                    }
                                }

                                fieldValueParams.Add(dataField.CurrentRecordValueAsQueryParameter);
                            }

                            sb.Append(StringLiterals.COMMA);
                        }

                        if ((columnIndex % 100) == 0 && columnIndex > 0)
                        {
                            if (sb.ToString().LastIndexOf(StringLiterals.COMMA).Equals(sb.ToString().Length - 1))
                            {
                                sb.Remove(sb.ToString().LastIndexOf(StringLiterals.COMMA), 1);
                            }

                            query = dbDriver.CreateQuery(updateHeader + StringLiterals.SPACE + sb.ToString() + StringLiterals.SPACE + whereClause);
                            query.Parameters = fieldValueParams;

                            dbDriver.ExecuteNonQuery(query);

                            columnIndex = 0;
                            sb.Remove(0, sb.ToString().Length);
                            fieldValueParams.Clear();
                        }
                    }
                }

                if (sb.Length > 0)
                {
                    if (sb.ToString().LastIndexOf(StringLiterals.COMMA).Equals(sb.ToString().Length - 1))
                    {
                        int startIndex = sb.ToString().LastIndexOf(StringLiterals.COMMA);
                        if (startIndex >= 0)
                        {
                            sb.Remove(startIndex, 1);
                        }
                    }

                    query = dbDriver.CreateQuery(updateHeader + StringLiterals.SPACE + sb.ToString() + StringLiterals.SPACE + whereClause);
                    query.Parameters = fieldValueParams;

                    dbDriver.ExecuteNonQuery(query);

                    columnIndex = 0;
                    sb.Remove(0, sb.ToString().Length);
                    fieldValueParams.Clear();
                }
                
                return 0;
            }
            finally
            {
            }
        }


        /// <summary>
        /// Updates all re
        /// </summary>
        /// <param name="field"></param>
        /// <param name="brokenLink"></param>
        /// <param name="newLink"></param>
        public void UpdateBrokenLinks(IDataField field, string brokenLink, string newLink)
        {
            string tableName = field.TableName;
            string columnName = dbDriver.InsertInEscape(((Epi.INamedObject)field).Name);
            string sqlStmt = " update " + tableName + " set " + columnName + " = @newLink";
            sqlStmt += " where " + columnName + " = @brokenLink ";
            Query query = dbDriver.CreateQuery(sqlStmt);
            query.Parameters.Add(new QueryParameter("@newLink", DbType.String, newLink));
            query.Parameters.Add(new QueryParameter("@brokenLink", DbType.String, brokenLink));
            dbDriver.ExecuteNonQuery(query);
        }

        #endregion  Public Methods

        #region Select Statements

        /// <summary>
        /// Returns the total number of records or related records from the view's table.
        /// </summary>
        /// <param name="view">View of a table.</param>
        /// <returns>Total number of records</returns>
        public int GetRecordCount(View view)
        {
            #region Input Validation
            if (view == null)
            {
                throw new System.ArgumentNullException("view");
            }
            #endregion Input Validation

            string relatedViewFilter = string.Empty;
            int count;
            
            try
            {
                if (view.IsRelatedView)
                {
                    relatedViewFilter = " where "
                        + dbDriver.InsertInEscape(ColumnNames.FOREIGN_KEY) 
                        + StringLiterals.EQUAL
                        + "'" + view.ForeignKeyField.CurrentRecordValueString + "'";
                }

                string qryString = " select count(*) from " 
                    + dbDriver.InsertInEscape(view.TableName) 
                    + relatedViewFilter;
                
                Query query = dbDriver.CreateQuery(qryString);

                object queryObject = dbDriver.ExecuteScalar(query);
                string countAsString = queryObject.ToString();
                
                count = Int32.Parse(countAsString);
                return count;
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
        /// Returns the first record Id from the view's table.
        /// </summary>
        /// <param name="view">View of a table.</param>
        /// <returns>First record Id</returns>
        public int GetFirstRecordId(View view)
        {
            try
            {
                #region Input Validation
                if (view == null)
                {
                    throw new System.ArgumentNullException("view");
                }
                #endregion Input Validation
                string relatedViewFilter = string.Empty;
                
                if (view.IsRelatedView)
                {
                    relatedViewFilter = " where "
                        + dbDriver.InsertInEscape(ColumnNames.FOREIGN_KEY)
                        + StringLiterals.EQUAL 
                        + "'" + view.ForeignKeyField.CurrentRecordValueString + "'";
                }
                
                string qryString = " select min(" 
                    + dbDriver.InsertInEscape(ColumnNames.UNIQUE_KEY) 
                    + ") from " 
                    + dbDriver.InsertInEscape(view.TableName) 
                    + relatedViewFilter;
                
                Query query = dbDriver.CreateQuery(qryString);

                object result = dbDriver.ExecuteScalar(query);
                if (result != DBNull.Value)
                {
                    return (int)result;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve record id for the first record.", ex);
            }
        }

        /// <summary>
        /// Returns the last record Id from the view's table.
        /// </summary>
        /// <param name="view">View of a table.</param>
        /// <returns>Last record Id</returns>
        public int GetLastRecordId(View view)
        {
            try
            {
                #region Input Validation
                if (view == null)
                {
                    throw new System.ArgumentNullException("view");
                }
                #endregion Input Validation
                string relatedViewFilter = StringLiterals.SPACE;

                if (view.IsRelatedView)
                {
                    relatedViewFilter = " where "
                        + dbDriver.InsertInEscape(ColumnNames.FOREIGN_KEY)
                        + StringLiterals.EQUAL
                        + "'" + view.ForeignKeyField.CurrentRecordValueString + "'";
                }

                string qryString = " select max(" 
                    + dbDriver.InsertInEscape(ColumnNames.UNIQUE_KEY) 
                    + ") from "
                    + dbDriver.InsertInEscape(view.TableName) 
                    + relatedViewFilter;

                Query query = dbDriver.CreateQuery(qryString);

                object result = dbDriver.ExecuteScalar(query);
                if (result != DBNull.Value)
                {
                    return (int)result;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve record id for the last record.", ex);
            }
        }

        /// <summary>
        /// Loads a record by it's Id
        /// </summary>
        /// <param name="view">View of a table.</param>
        /// <param name="recordId">Id of the record</param>
        public void LoadRecordIntoView(View view, int recordId)
        {
            #region Input Validation
            if (view == null)
            {
                throw new ArgumentOutOfRangeException("view");
            }
            if (recordId < 1)
            {
                throw new ArgumentOutOfRangeException("recordId");
            }
            #endregion Input Validation

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(" select * ");
                sb.Append(" from ");
                sb.Append(dbDriver.InsertInEscape(view.TableName));
                sb.Append(" where ");
                sb.Append(dbDriver.InsertInEscape(ColumnNames.UNIQUE_KEY));
                sb.Append(StringLiterals.EQUAL);
                sb.Append(recordId.ToString());

                Query selectQuery = dbDriver.CreateQuery(sb.ToString());

                DataTable dataTable = dbDriver.Select(selectQuery);
                dataTable.CaseSensitive = false;
                if (dataTable.Rows.Count > 0)
                {
                    DataRow dataRow = dataTable.Rows[0];
                    view.UniqueKeyField.CurrentRecordValueString = dataRow["UniqueKey"].ToString();
                   // view.RecStatusField.CurrentRecordValueObject = dataRow["RecStatus"];
                    view.RecStatusField.CurrentRecordValueObject = dataRow["RECSTATUS"];//To support differnet locales such as Hungarian,Turkish.
                    view.CurrentGlobalRecordId = dataRow["GlobalRecordId"].ToString();
                }

                foreach (Page page in view.Pages)
                {
                    LoadRecordIntoView(page, view.CurrentGlobalRecordId);
                }

                foreach (GridField gridField in view.Fields.GridFields)
                {
                    DataTable gridData = GetGridTableData(view, gridField);
                    gridData.TableName = view.Name + gridField.Page.Id.ToString() + gridField.Name;
                    gridField.DataSource = gridData;
                }
            }
            finally
            {
            }
        }

        /// <summary>
        /// Loads a record by it's Id
        /// </summary>
        /// <param name="page">page of a View</param>
        /// <param name="recordId">Id of the record</param>
        public void LoadRecordIntoView(Page page, string GlobalRecordId)
        {
            #region Input Validation
            if (page == null)
            {
                throw new ArgumentOutOfRangeException("view");
            }
            if (GlobalRecordId == "")
            {
                throw new ArgumentOutOfRangeException("recordId");
            }
            #endregion Input Validation

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(" select ");
                List<string> columnNames = new List<string>();
                
                foreach(string name in page.Fields.Names)
                {
                    if (page.Fields[name] is GridField || page.Fields[name] is GroupField)
                    {
                        continue;
                    }
                    else if (page.Fields[name] is IDataField)
                    {
                        columnNames.Add(name);
                    }
                }

                if (columnNames.Count > 0)
                {
                    columnNames = dbDriver.InsertInEscape(columnNames);

                    sb.Append(Util.ToString(columnNames, StringLiterals.COMMA));
                    sb.Append(" from ");
                    sb.Append(dbDriver.InsertInEscape(page.TableName));
                    sb.Append(" where ");
                    sb.Append(dbDriver.InsertInEscape(ColumnNames.GLOBAL_RECORD_ID));
                    sb.Append("='");
                    sb.Append(GlobalRecordId);
                    sb.Append("'");

                    Query selectQuery = dbDriver.CreateQuery(sb.ToString());

                    DataTable dataTable = dbDriver.Select(selectQuery);
                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow dataRow = dataTable.Rows[0];
                        foreach (RenderableField renderableField in page.Fields)
                        {
                            if (renderableField is GridField || renderableField is GroupField)
                            {
                                continue;
                            }
                            else
                            {
                                if (renderableField is IDataField)
                                {
                                    IDataField dataField = (IDataField)renderableField;
                                    Object val = dataRow[((Epi.INamedObject)dataField).Name];
                                    if (dataField is MirrorField)
                                    {
                                        MirrorField mirror = (MirrorField)dataField;
                                        dataField.CurrentRecordValueObject = dataRow[((Epi.INamedObject)mirror.SourceField).Name];
                                    }
                                    else if (dataField is DateTimeField)
                                    {
                                        DateTimeField typedField = dataField as DateTimeField;
                                        if (val == DBNull.Value)
                                        {
                                            typedField.CurrentRecordValueObject = null;
                                        }
                                        else
                                        {
                                            typedField.CurrentRecordValueObject = (DateTime)val;
                                        }
                                    }
                                    else
                                    {
                                        dataField.CurrentRecordValueObject = dataRow[((Epi.INamedObject)dataField).Name];
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
            }
        }

        /// <summary>
        /// Retrieves Id of the previous record.
        /// Returns 0 if this is the first record.
        /// </summary>
        /// <param name="view">View of a table.</param>
        /// <param name="currentRecordId">Id of current record.</param>
        public int GetPreviousRecordId(View view, int currentRecordId)
        {
            #region Input Validation
            if (view == null)
            {
                throw new ArgumentOutOfRangeException("View");
            }
            if (currentRecordId < 1)
            {
                throw new ArgumentOutOfRangeException("Record ID");
            }
            #endregion Input Validation

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Util.InsertIn("select", StringLiterals.SPACE));
                sb.Append("max");
                sb.Append(Util.InsertInParantheses(dbDriver.InsertInEscape(ColumnNames.UNIQUE_KEY)));
                sb.Append(Util.InsertIn("from", StringLiterals.SPACE));
                sb.Append(dbDriver.InsertInEscape(view.TableName));
                sb.Append(Util.InsertIn("where", StringLiterals.SPACE));
                sb.Append(dbDriver.InsertInEscape(ColumnNames.UNIQUE_KEY));
                sb.Append(StringLiterals.LESS_THAN);
                sb.Append(currentRecordId.ToString());

                if (view.IsRelatedView)
                {
                    sb.Append(Util.InsertIn("and", StringLiterals.SPACE));
                    sb.Append(dbDriver.InsertInEscape(ColumnNames.FOREIGN_KEY));
                    sb.Append(StringLiterals.EQUAL + "'" + view.ForeignKeyField.CurrentRecordValueString + "'");
                }

                Query selectQuery = dbDriver.CreateQuery(sb.ToString());

                object result = dbDriver.ExecuteScalar(selectQuery);
                if (result != DBNull.Value)
                {
                    return (int)result;
                }
                else
                {
                    return 0;
                }
            }
            finally
            {
            }
        }

        /// <summary>
        /// Returns the Id of the next record.
        /// Returns 0 if this is the last record.
        /// </summary>
        /// <param name="view">View of a table.</param>
        /// <param name="currentRecordId">Id of current record.</param>
        /// <returns>Id of next record.</returns>
        public int GetNextRecordId(View view, int currentRecordId)
        {
            #region Input Validation
            if (view == null)
            {
                throw new ArgumentOutOfRangeException("View");
            }
            if (currentRecordId < 1)
            {
                throw new ArgumentOutOfRangeException("Record ID");
            }
            #endregion Input Validation

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Util.InsertIn("select", StringLiterals.SPACE));
                sb.Append("min");
                sb.Append(Util.InsertInParantheses(dbDriver.InsertInEscape(ColumnNames.UNIQUE_KEY)));
                sb.Append(Util.InsertIn("from", StringLiterals.SPACE));
                sb.Append(dbDriver.InsertInEscape(view.TableName));
                sb.Append(Util.InsertIn("where", StringLiterals.SPACE));
                sb.Append(dbDriver.InsertInEscape(ColumnNames.UNIQUE_KEY));
                sb.Append(StringLiterals.GREATER_THAN);
                sb.Append(currentRecordId.ToString());

                if (view.IsRelatedView)
                {
                    sb.Append(Util.InsertIn("and", StringLiterals.SPACE));
                    sb.Append(dbDriver.InsertInEscape(ColumnNames.FOREIGN_KEY));
                    sb.Append(StringLiterals.EQUAL + "'" + view.ForeignKeyField.CurrentRecordValueString + "'");
                }

                Query selectQuery = dbDriver.CreateQuery(sb.ToString());

                object result = dbDriver.ExecuteScalar(selectQuery);
                if (result != DBNull.Value)
                {
                    return (int)result;
                }
                else
                {
                    return 0;
                }
            }
            finally
            {
            }
        }

        /// <summary>
        /// Returns records found for search criteria
        /// </summary>
        /// <param name="view">View of a table.</param>
        /// <param name="searchFields">A collection of search fields</param>
        /// <param name="searchFieldItemTypes"> A collection of the corresponding types of the search field items</param>
        /// <param name="searchFieldValues">A collection values for the search fields</param>
        /// <param name="comparisonTypes"></param>
        /// <returns>Records found meeting search criteria</returns>
        //        public DataTable GetSearchRecords(View view, Collection<string> searchFields, Collection<string> searchFieldValues)        
        public DataTable GetSearchRecords(View view, Dictionary<string,int> OrFieldCount, Collection<string> searchFields, Collection<string> searchFieldItemTypes, Collection<string> comparisonTypes, ArrayList searchFieldValues)
        {
            #region Input Validation

            if (view == null)
            {
                throw new ArgumentOutOfRangeException("View");
            }

            if (searchFields == null)
            {
                throw new ArgumentOutOfRangeException("Search Fields Collections");
            }

            if (searchFieldItemTypes == null)
            {
                throw new ArgumentOutOfRangeException("Search Fields Item Types");
            }


            if (comparisonTypes == null)
            {
                throw new ArgumentOutOfRangeException("Comparison Types");
            }

            if (searchFieldValues == null)
            {
                throw new ArgumentOutOfRangeException("Search Fields Values");
            }

            #endregion

            Configuration config;
            StringBuilder sb = null;
            Query selectQuery;
            List<QueryParameter> parameters = new List<QueryParameter>();
            int ParameterCount = 0;
            string CurrentParam = "";

            int CurrentCount = 0;
            string paramName = null;

            try
            {
                config = Configuration.GetNewInstance();
                sb = new StringBuilder();

                string selectText = "SELECT baseTable.GlobalRecordId FROM ";
                string joinText = Util.InsertInSquareBrackets(view.TableName) + " baseTable ";
                
                foreach (Page page in view.Pages)
                {
                    string tableVarName = " pageTable" + page.Id.ToString();
                    
                    string join = "("
                        + joinText
                        + " INNER JOIN "
                        + Util.InsertInSquareBrackets(page.TableName)
                        + tableVarName
                        + " ON baseTable.GlobalRecordId = "
                        + tableVarName
                        + ".GlobalRecordId) ";

                    joinText = join;
                }

                sb.Append(selectText + joinText + " WHERE ");

                for (int i = 0; i <= searchFields.Count - 1; ParameterCount++, i++)
                {
                    if (searchFields[i].ToLower() != CurrentParam)
                    {
                        CurrentCount = 0;
                        CurrentParam = searchFields[i].ToLower();

                        if (i == 0)
                        {
                            sb.Append("(");
                        }
                        else
                        {
                            sb.Append(" And ("); 
                        }
                    }
                    else
                    {
                            sb.Append(" OR ");
                    }

                    paramName = "@param" + ParameterCount.ToString();

                    if (searchFieldItemTypes[i].Equals("YesNo"))
                    {
                        if (searchFieldValues[i] == null || searchFieldValues[i] == "(.)" || searchFieldValues[i] == config.Settings.RepresentationOfMissing)
                        {
                            sb.Append(searchFields[i]);
                            sb.Append(StringLiterals.SPACE);
                            sb.Append(SqlKeyWords.IS);  
                            sb.Append(StringLiterals.SPACE);
                            sb.Append(SqlKeyWords.NULL);
                        }
                        else
                        {
                            if (searchFieldValues[i].Equals(config.Settings.RepresentationOfYes) || searchFieldValues[i].Equals("1") || searchFieldValues[i].Equals("(+)"))
                            {
                                sb.Append(Util.InsertInParantheses(searchFields[i] + StringLiterals.SPACE + "=" + StringLiterals.SPACE + "1"));
                            }
                            else if (searchFieldValues[i].Equals(config.Settings.RepresentationOfNo) || searchFieldValues[i].Equals("0") || searchFieldValues[i].Equals("(-)"))
                            {
                                sb.Append(Util.InsertInParantheses(searchFields[i] + StringLiterals.SPACE + "=" + StringLiterals.SPACE + "0"));
                            }
                            else
                            {
                                sb.Append(searchFields[i] + StringLiterals.SPACE + SqlKeyWords.IS + StringLiterals.SPACE + SqlKeyWords.NULL);
                            }
                        }
                    }
                    else if (searchFieldItemTypes[i].Equals("Checkbox"))
                    {
                        if (searchFieldValues[i] == null)
                        {
                            sb.Append(searchFields[i]);
                            sb.Append(StringLiterals.SPACE);
                            sb.Append(SqlKeyWords.IS);
                            sb.Append(StringLiterals.SPACE);
                            sb.Append(SqlKeyWords.NULL);
                        }
                        else
                        {                            
                            if (searchFieldValues[i].Equals(config.Settings.RepresentationOfYes) || searchFieldValues[i].Equals("1") || searchFieldValues[i].Equals("(+)"))
                            {
                                sb.Append(Util.InsertInParantheses(searchFields[i] + StringLiterals.SPACE + "=" + StringLiterals.SPACE + paramName));
                                parameters.Add(new QueryParameter(paramName, DbType.Boolean, true));
                            }
                            else if (searchFieldValues[i].Equals(config.Settings.RepresentationOfNo) || searchFieldValues[i].Equals("0") || searchFieldValues[i].Equals("(-)"))
                            {
                                sb.Append(Util.InsertInParantheses(searchFields[i] + StringLiterals.SPACE + "=" + StringLiterals.SPACE + paramName));
                                parameters.Add(new QueryParameter(paramName, DbType.Boolean, false));
                            }
                        }
                    }
                    else if (searchFieldItemTypes[i].Equals("Number"))                    
                    {
                        if (searchFieldValues[i] == null)
                        {
                            sb.Append(searchFields[i]);
                            sb.Append(StringLiterals.SPACE);
                            sb.Append(SqlKeyWords.IS);
                            sb.Append(StringLiterals.SPACE);
                            sb.Append(SqlKeyWords.NULL);
                        }
                        else
                        {
                            double num;
                            bool isNum = Double.TryParse(searchFieldValues[i].ToString(), out num);

                            if (isNum)
                            {
                                sb.Append(searchFields[i] + " " + comparisonTypes[i] + " " + searchFieldValues[i].ToString());
                            }
                        }
                    }
                    else if (searchFieldItemTypes[i].Equals("PhoneNumber"))
                    {
                        if (string.IsNullOrEmpty(searchFieldValues[i].ToString()))
                        {
                            sb.Append(searchFields[i]);
                            sb.Append(StringLiterals.SPACE);
                            sb.Append(SqlKeyWords.IS);
                            sb.Append(StringLiterals.SPACE);
                            sb.Append(SqlKeyWords.NULL);
                        }
                        else
                        {
                            sb.Append(searchFields[i] + " " + comparisonTypes[i] + " " + Util.InsertInSingleQuotes(searchFieldValues[i].ToString()));
                        }
                    }
                    else
                    {
                        if (searchFieldItemTypes[i].Equals("Date") || searchFieldItemTypes[i].Equals("DateTime") || searchFieldItemTypes[i].Equals("Time"))
                        {
                            if (searchFieldValues[i] == null || string.IsNullOrEmpty(searchFieldValues[i].ToString()))
                            {
                                sb.Append(searchFields[i]);
                                sb.Append(StringLiterals.SPACE);
                                sb.Append(SqlKeyWords.IS);
                                sb.Append(StringLiterals.SPACE);
                                sb.Append(SqlKeyWords.NULL);
                            }
                            else
                            {
                                DateTime date;
                                bool isDate = DateTime.TryParse(searchFieldValues[i].ToString(), out date);

                                if (isDate)
                                {
                                    if (searchFieldItemTypes[i].Equals("Time"))
                                    {   
                                        sb.Append(searchFields[i]);
                                        sb.Append(StringLiterals.SPACE);
                                        sb.Append(SqlKeyWords.LIKE);
                                        sb.Append(StringLiterals.SPACE);
                                        sb.Append(Util.InsertInSingleQuotes(StringLiterals.PERCENT + date.ToLongTimeString() + StringLiterals.PERCENT));                                        
                                    }
                                    else
                                    {
                                        sb.Append(searchFields[i] + 
                                            StringLiterals.SPACE + 
                                            comparisonTypes[i] + 
                                            StringLiterals.SPACE + 
                                            paramName);
                                        parameters.Add(new QueryParameter(paramName, DbType.DateTime, date));
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(searchFieldValues[i].ToString()))
                                    {
                                        sb.Append(searchFields[i]);
                                        sb.Append(StringLiterals.SPACE);
                                        sb.Append(SqlKeyWords.IS);
                                        sb.Append(StringLiterals.SPACE);
                                        sb.Append(SqlKeyWords.NULL);
                                    }
                                }
                            }
                        }
                        else
                        {
                            string searchValue = String.Empty;
                            if (string.IsNullOrEmpty(searchFieldValues[i].ToString()))       //Value is null or an empty string)
                            {
                                sb.Append(searchFields[i]);
                                sb.Append(StringLiterals.SPACE);
                                sb.Append(SqlKeyWords.IS);
                                sb.Append(StringLiterals.SPACE);
                                sb.Append(SqlKeyWords.NULL);
                            }
                            //for wild card searches - EJ
                            else if (
                                searchFieldValues[i].ToString().Contains(StringLiterals.PERCENT)
                                || searchFieldValues[i].ToString().Contains(StringLiterals.STAR)
                                || searchFieldValues[i].ToString().Contains("?")
                                )   //allows for wild card searches (i.e. "%T%")
                            {
                                sb.Append(searchFields[i]);
                                sb.Append(StringLiterals.SPACE);
                                sb.Append(SqlKeyWords.LIKE);
                                sb.Append(StringLiterals.SPACE);
                                sb.Append(string.Format("'{0}'", searchFieldValues[i].ToString().Replace("*", "%").Replace("'","")));
                                ParameterCount--;
                            }
                            else
                            {
                                // Multiline fields are stored as ntext which cannot be compared using the = operator. 
                                // Compare up to the first 255 characters for Multiline fields.
                                if (searchFieldItemTypes[i].Equals("Multiline"))
                                {
                                    if (this.GetDbDriver().GetType().Name == "AccessDatabase")
                                    {
                                        sb.Append("MID(" + searchFields[i] + ", 1, 255) = MID(" + paramName + ", 1, 255)");
                                    }
                                    else
                                    {
                                        sb.Append("SUBSTRING(" + searchFields[i] + ", 1, 255) = SUBSTRING(" + paramName + ", 1, 255)");
                                    }
                                    parameters.Add(new QueryParameter(paramName, DbType.AnsiString, searchFieldValues[i]));
                                }
                                else
                                {
                                    sb.Append(searchFields[i]);
                                    sb.Append(StringLiterals.SPACE);
                                    sb.Append(StringLiterals.EQUAL);
                                    sb.Append(StringLiterals.SPACE);
                                    sb.Append(paramName);
                                    parameters.Add(new QueryParameter(paramName, DbType.String, searchFieldValues[i]));
                                }
                            }
                        }
                    }

                    if (CurrentCount == OrFieldCount[CurrentParam] - 1)
                    {
                        sb.Append(")");
                    }
                    else
                    {
                        sb.Append(" ");
                    }
                    CurrentCount++;
                }

                sb.Append(" and RecStatus > 0");
                sb.Replace(" where  and ", " where ");
                selectQuery = dbDriver.CreateQuery(sb.ToString());

                selectQuery.Parameters.AddRange(parameters);
                DataTable Output = (DataTable)dbDriver.Select(selectQuery);

                if (Output.Rows.Count > 0)
                {
                    sb.Length = 0;
                    sb.Append("Select * From [" + view.TableName + "] As baseTable Where GlobalRecordId in (");
                    StringBuilder RecordBuilder = new StringBuilder();
                    foreach (DataRow r in Output.Rows)
                    {
                        RecordBuilder.Append("'");
                        RecordBuilder.Append(r["GlobalRecordId"].ToString());
                        RecordBuilder.Append("',");

                    }
                
                    RecordBuilder.Length = RecordBuilder.Length - 1;
                

                    sb.Append(RecordBuilder);
                    sb.Append(")");
                    selectQuery = dbDriver.CreateQuery(sb.ToString());
                    DataTable result = dbDriver.Select(selectQuery);

                    foreach (Page page in view.Pages)
                    {
                        sb.Length = 0;
                        sb.Append("Select * From [" + page.TableName + "] Where GlobalRecordId in (");
                        sb.Append(RecordBuilder);
                        sb.Append(")");
                        selectQuery = dbDriver.CreateQuery(sb.ToString());
                        Output = (DataTable)dbDriver.Select(selectQuery);
                        result = JoinTables(result, Output);
                    }

                    return result;
                }

                return Output;
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not retrieve search records", ex);
            }
            finally
            {
                config = null;
                sb = null;
                selectQuery = null;
            }
        }

        public DataTable JoinTables(DataTable parentTable, DataTable childTable)
        {
            DataTable result = new DataTable("Output");

            using (DataSet ds = new DataSet())
            {
                ds.Tables.AddRange(new DataTable[] { parentTable.Copy(), childTable.Copy() });
                DataColumn parentColumn = ds.Tables[0].Columns["GlobalRecordId"];
                DataColumn childColumn = ds.Tables[1].Columns["GlobalRecordId"];
                DataRelation dataRelation = new DataRelation(string.Empty, parentColumn, childColumn, false);
                ds.Relations.Add(dataRelation);

                for (int i = 0; i < parentTable.Columns.Count; i++)
                {
                    result.Columns.Add(parentTable.Columns[i].ColumnName, parentTable.Columns[i].DataType);
                }

                for (int i = 0; i < childTable.Columns.Count; i++)
                {
                    if (!(childTable.Columns[i].ColumnName.Equals("RecStatus", StringComparison.CurrentCultureIgnoreCase) || childTable.Columns[i].ColumnName.Equals("FKey", StringComparison.CurrentCultureIgnoreCase) || childTable.Columns[i].ColumnName.Equals("GlobalRecordId", StringComparison.CurrentCultureIgnoreCase)))
                    {
                        if (!result.Columns.Contains(childTable.Columns[i].ColumnName))
                        {
                            result.Columns.Add(childTable.Columns[i].ColumnName, childTable.Columns[i].DataType);
                        }
                        else
                        {
                            int count = 0;
                            foreach (DataColumn column in result.Columns)
                            {
                                if (column.ColumnName.StartsWith(childTable.Columns[i].ColumnName))
                                {
                                    count++;
                                }
                            }
                            result.Columns.Add(childTable.Columns[i].ColumnName + count.ToString(), childTable.Columns[i].DataType);
                        }
                    }
                }

                foreach (DataRow parentRow in ds.Tables[0].Rows)
                {
                    DataRow resultRow = result.NewRow();
                    DataRow[] childRow = parentRow.GetChildRows(dataRelation);

                    if (childRow != null && childRow.Length > 0)
                    {
                        foreach (DataColumn dataColumn in childTable.Columns)
                        {
                            resultRow[dataColumn.ColumnName] = childRow[0][dataColumn.ColumnName];
                        }

                        foreach (DataColumn dataColumn in parentTable.Columns)
                        {
                            resultRow[dataColumn.ColumnName] = parentRow[dataColumn.ColumnName];
                        }

                        result.Rows.Add(resultRow);
                    }
                }
                result.AcceptChanges();
            }

            return result;
        }

        #endregion Select Statements

        #region Insert Statements

        /// <summary>
        /// Inserts table name and Id into SysDataTables.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="viewId">Id of view.</param>
        public void InsertSysDataTableRow(string tableName, int viewId)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("Table Name");
            }
            if (viewId < 0)
            {
                throw new ArgumentOutOfRangeException("View Id");
            }
            #endregion

            try
            {
                Query query = dbDriver.CreateQuery("insert into sysDataTables([TableName], [ViewId]) values (@tableName, @viewId)");
                query.Parameters.Add(new QueryParameter("@tableName", DbType.String, tableName));
                query.Parameters.Add(new QueryParameter("@viewId", DbType.Int16, viewId));

                dbDriver.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not insert data table name", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// inserts a row in a table
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="row">Row</param>
        /// <param name="table">Table</param>
        public void InsertDataTableRow(string tableName, DataTable table, DataRow row)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("Table Name");
            }
            if (row == null)
            {
                throw new ArgumentOutOfRangeException("Data Row");
            }
            #endregion

            try
            {
                Query query = CreateInsertQuery(dbDriver, table, row, tableName);

                dbDriver.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not insert data table name", ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dataTable"></param>
        /// <param name="dRow"></param>
        /// <param name="sTableName"></param>
        /// <returns></returns>
        protected Query CreateInsertQuery(IDbDriver db, DataTable dataTable, DataRow dRow, string sTableName)
        {
            Query result;

            try
            {
                // Begin Insert clause
                StringBuilder sbInsert = new StringBuilder();
                WordBuilder fieldNames = new WordBuilder(StringLiterals.COMMA);
                WordBuilder fieldValues = new WordBuilder(StringLiterals.COMMA);
                sbInsert.Append("Insert Into ");
                sbInsert.Append(Util.InsertIn(dbDriver.InsertInEscape(sTableName), StringLiterals.SPACE));

                foreach (DataColumn column in dataTable.Columns)
                {
                    fieldNames.Append(dbDriver.InsertInEscape(column.ColumnName));

                    if (column.DataType.IsPrimitive)
                    {
                        if (dRow[column.ColumnName].ToString().Length == 0)
                        {
                            fieldValues.Append("null");
                        }
                        else
                            if (column.DataType.Name == "Boolean")
                            {
                                if (dRow[column.ColumnName] != null)
                                {
                                    if (dRow[column.ColumnName].ToString() == "True")
                                    {
                                        fieldValues.Append("1");
                                    }
                                    else
                                    {
                                        fieldValues.Append("0");
                                    }
                                }
                                else
                                {
                                    fieldValues.Append("null");
                                }
                            }
                            else
                            {
                                fieldValues.Append(dRow[column.ColumnName].ToString());
                            }
                    }
                    else
                    {
                        if (column.DataType.Name == "Boolean")
                        {
                            if (dRow[column.ColumnName] != null)
                            {
                                if (dRow[column.ColumnName].ToString() == "True")
                                {
                                    fieldValues.Append("1");
                                }
                                else
                                {
                                    fieldValues.Append("0");
                                }
                            }
                            else
                            {
                                fieldValues.Append("null");
                            }
                        }
                        else
                        {
                            fieldValues.Append(Util.InsertInSingleQuotes(dRow[column.ColumnName].ToString()));
                        }
                    }
                }

                sbInsert.Append(Util.InsertInParantheses(fieldNames.ToString()));
                sbInsert.Append(Util.InsertIn("Values", StringLiterals.SPACE));
                sbInsert.Append(Util.InsertInParantheses(fieldValues.ToString()));
                // End Values clause
                result = dbDriver.CreateQuery(sbInsert.ToString());

            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create insert query", ex);
            }

            return result;
        }

        #endregion Insert Statements

        #region Delete Statements
        #endregion Delete Statements

        #region Create Statements

       
        /// <summary>
        /// Creates a datatable corresponding to a view.
        /// </summary>
        /// <param name="view">View of a table.</param>
        /// <param name="startingId">Starting Id for new table.</param>
        public void CreateDataTableForView(View view, int startingId)
        {
            #region Input Validation
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            #endregion Input Validation

            string relatedViewInsert = string.Empty;

            string tableName = dbDriver.SchemaPrefix + dbDriver.InsertInEscape(view.TableName);

            StartingId = startingId;

            Query createQuery = dbDriver.CreateQuery("create table " + tableName +
                " ("

                + Util.InsertIn(ColumnNames.UNIQUE_KEY, StringLiterals.SPACE)
                + SqlDataTypes.INTEGER32 
                + " IDENTITY(" + startingId + ",1) primary key not null, "
                
                + Util.InsertIn(ColumnNames.REC_STATUS, StringLiterals.SPACE) 
                + SqlDataTypes.INTEGER16 
                + " not null default 1,"

                + Util.InsertIn(ColumnNames.GLOBAL_RECORD_ID, StringLiterals.SPACE)
                + SqlDataTypes.NVARCHAR
                + "(255) not null,"
                
                + Util.InsertIn(ColumnNames.RECORD_FIRST_SAVE_LOGON_NAME, StringLiterals.SPACE)
                + SqlDataTypes.NVARCHAR
                + "(255),"

                + Util.InsertIn(ColumnNames.RECORD_FIRST_SAVE_TIME, StringLiterals.SPACE)
                + "DATETIME,"

                + Util.InsertIn(ColumnNames.RECORD_LAST_SAVE_LOGON_NAME, StringLiterals.SPACE)
                + SqlDataTypes.NVARCHAR
                + "(255),"

                + Util.InsertIn(ColumnNames.RECORD_LAST_SAVE_TIME, StringLiterals.SPACE)
                + "DATETIME,"

                + Util.InsertIn(ColumnNames.FOREIGN_KEY, StringLiterals.SPACE)
                + SqlDataTypes.NVARCHAR
                + "(255)"

                +")");

            dbDriver.ExecuteNonQuery(createQuery);

            
            if (dbDriver.FullName.Contains("[MS Access]"))
            {
                string insertStatement = string.Format("insert into {0}([UniqueKey], [GlobalRecordId]) values (@UniqueKey, @GlobalRecordId)", tableName);
                Query insertQuery = dbDriver.CreateQuery(insertStatement);
                insertQuery.Parameters.Add(new QueryParameter("@UniqueKey", DbType.Int16, startingId - 1));
                insertQuery.Parameters.Add(new QueryParameter("@GlobalRecordId", DbType.String, ""));
                dbDriver.ExecuteNonQuery(insertQuery);

                string deleteStatement = string.Format("delete from {0} where UniqueKey={1} ", tableName, startingId - 1);
                Query deleteQuery = dbDriver.CreateQuery(deleteStatement);
                dbDriver.ExecuteNonQuery(deleteQuery);
            }/**/

            foreach (Epi.Page page in view.Pages)
            {
                CreatePageDataTableForView(page);
            }
        }

        /// <summary>
        /// Creates a datatable corresponding to a view.
        /// </summary>
        /// <param name="page">page of a view.</param>
        /// <param name="View_TableName">View table name</param>
        public void CreatePageDataTableForView(Page page)
        {
            #region Input Validation
            if (page == null)
            {
                throw new ArgumentNullException("view");
            }
            #endregion Input Validation

            string View_TableName = page.TableName;

            Query createQuery = dbDriver.CreateQuery("create table " + dbDriver.SchemaPrefix + dbDriver.InsertInEscape(View_TableName) +
                " ("
                + Util.InsertIn(ColumnNames.GLOBAL_RECORD_ID, StringLiterals.SPACE)
                + SqlDataTypes.NVARCHAR
                + "(255) primary key not null)"); 

            dbDriver.ExecuteNonQuery(createQuery);

            foreach (RenderableField renderableField in page.Fields)
            {
                if (renderableField is GridField)
                {
                    CreateDataTableForGrid(page, (GridField)renderableField);
                }
                else
                {
                    if (renderableField is IInputField)
                    {
                        IInputField field = (IInputField)renderableField;
                        CreateTableColumn(field, View_TableName);
                    }
                }
            }
        }

        /// Synchronizes a view's tables with its fields
        /// [currently only adds columns to the view data table]
        /// </summary>
        /// <param name="page">page of a View.</param>
        public void SynchronizePageTable(Page page)
        {
            string sTableName = string.Empty;

            if (dbDriver.TableExists(page.TableName) == false)
            {
                CreatePageDataTableForView(page);

                string viewName = page.view.Name;
                foreach (DataRow row in project.CollectedData.GetTableData(viewName).Rows)
                {
                    string id = row["GlobalRecordId"].ToString();
                    string queryString = string.Format("GlobalRecordId = '{0}'", id);
                    DataRow[] rows = project.CollectedData.GetTableData(viewName).Select(queryString);

                    queryString = string.Format("insert into [{0}] (GlobalRecordId) values ('{1}')",
                        page.TableName,
                        id);

                    Epi.Data.Query updateQuery = project.CollectedData.CreateQuery(queryString);
                    project.CollectedData.ExecuteNonQuery(updateQuery);
                }
            }

            List<string> columnNames = dbDriver.GetTableColumnNames(page.TableName);

            string View_TableName = page.TableName;

            try
            {
                foreach (Field field in page.Fields)
                {
                    if (field is IInputField)
                    {
                        if (!columnNames.Contains(field.Name))
                        {
                            CreateTableColumn((IInputField)field, View_TableName);
                        }
                    }
                    else if (field is GridField)
                    {
                        sTableName = View_TableName + field.Name;
                        if (dbDriver.TableExists(sTableName) == false)
                        {
                            CreateDataTableForGrid(page, (GridField)field);
                        }
                        else
                        {
                            foreach (GridColumnBase column in ((GridField)field).Columns)
                            {
                                if (dbDriver.ColumnExists(sTableName, column.Name) == false)
                                {
                                    CreateTableColumn(column, sTableName);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not synchronize table columns - " + ex.Message, ex);
            }
        }

        #endregion Create Statements

        #region Static Methods
        /// <summary>
        /// Finds Epi Version and Purpose of the database.
        /// </summary>
        /// <param name="db">Abstract data type for database.</param>
        /// <returns>Results of test.</returns>
        public static bool IsEpi7CollectedData(IDbDriver dbDriver)
        {
            #region Input Validation
            if (dbDriver == null)
            {
                throw new ArgumentNullException("DB");
            }
            #endregion

            try
            {
                // Open the database and look for dbInfo table, find Epi Version and purpose ...		
                bool isEpi7CollectedData = false;
                Query query = dbDriver.CreateQuery("select" + Util.InsertIn(dbDriver.InsertInEscape("EpiVersion"), StringLiterals.SPACE) + StringLiterals.COMMA + Util.InsertIn(dbDriver.InsertInEscape("Purpose"), StringLiterals.SPACE) +
                    "from" + StringLiterals.SPACE + "metaDBInfo");

                DataTable results = dbDriver.Select(query);

                if (results.Rows.Count > 0)
                {
                    foreach (DataRow row in results.Rows)
                    {
                        isEpi7CollectedData = (row["EpiVersion"].ToString().Substring(0, 1) == "7") && ((row["Purpose"].ToString() == "0") || (row["Purpose"].ToString() == "2"));
                    }
                }
                return isEpi7CollectedData;
            }
            finally
            {

            }
        }

        #endregion Static Methods

        #region Private Methods

        /// <summary>
        /// Creates a field column in a table.
        /// </summary>
        /// <param name="field">A field object</param>
        /// <param name="tableName">Name of the table.</param>
        public void CreateTableColumn(IInputField field, string tableName)
        {
            if (!dbDriver.ColumnExists(tableName, ((Epi.INamedObject)field).Name))
            {
                string fieldLength = string.Empty;
                string fieldType = field.GetDbSpecificColumnType();
                Query query;

                switch (fieldType)
                {
                    case "nvarchar":
                    case "nchar":
                        fieldLength = "(4000)";
                        break;
                    case "text":
                        fieldLength = "(255)";
                        break;
                    case "double":
                        break;
                    default:
                        break;
                }

                query = dbDriver.CreateQuery("alter table [" + tableName + "] add [" + ((Epi.INamedObject)field).Name + "] " + fieldType + fieldLength);

                dbDriver.ExecuteNonQuery(query);
            }
        }
        /// <summary>
        /// Creates a Gridfield column in a GridField table.
        /// </summary>
        /// <param name="column">Grid Column to use as a table field.</param>
        /// <param name="tableName">Name of the GridField table.</param>
        private void CreateTableColumn(GridColumnBase column, string tableName)
        {
            string length = string.Empty;

            if (column.GetDbSpecificColumnType().Equals("nvarchar"))
            {
                length = "(4000)";
            }
            if (column.GetDbSpecificColumnType().Equals("nchar"))
            {
                length = "(4000)";
            }
            if (column.GetDbSpecificColumnType().Equals("text"))
            {
                length = "(255)";
            }

            string columnType = column.GetDbSpecificColumnType();

            string queryText = "alter table [" + tableName + "] add [" + column.Name + "] " + columnType + length;
            if (column.IsRequired)
            {
                queryText += " not null";
            }
            Query query = dbDriver.CreateQuery(queryText);
            dbDriver.ExecuteNonQuery(query);
        }

        /// <summary>
        /// Creates tables for collected data database
        /// </summary>
        private void CreateTables()
        {
            try
            {
                string[] SqlLine;
                Regex regex = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);

                string txtSQL = ResourceLoader.GetCollectedDataTableScripts();
                SqlLine = regex.Split(txtSQL);

                foreach (string line in SqlLine)
                {
                    if (line.Length > 0)
                    {
                        Query query = dbDriver.CreateQuery(line);
                        dbDriver.ExecuteNonQuery(query);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not create collected data tables", ex);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Creates a datatable corresponding to a GridField.
        /// </summary>
        /// <param name="view">View of a table that references the original table.</param>
        /// <param name="startingId">Starting Id for new table.</param>
        /// <param name="field">GridField object.</param>
        private void CreateDataTableForGrid(View view, int startingId, GridField field)
        {
            #region Input Validation
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            #endregion Input Validation

            string tableName = field.TableName;

            if (TableExists(tableName))
            {
                DeleteTable(tableName);
            }

            Query createQuery = dbDriver.CreateQuery
            ( "create table " + dbDriver.InsertInEscape(tableName) +
                Util.InsertInParantheses
                (
                    Util.InsertIn(ColumnNames.UNIQUE_KEY, StringLiterals.SPACE) +
                    "INT identity (" + 
                    startingId + 
                    ",1) primary key not null" + 
                    StringLiterals.COMMA +

                    Util.InsertIn(ColumnNames.UNIQUE_ROW_ID, StringLiterals.SPACE) +
                    SqlDataTypes.NVARCHAR +
                    "(255)," +

                    Util.InsertIn(ColumnNames.GLOBAL_RECORD_ID, StringLiterals.SPACE) +
                    SqlDataTypes.NVARCHAR +
                    "(255)," +

                    Util.InsertIn(ColumnNames.REC_STATUS, StringLiterals.SPACE) + 
                    "INT not null default 1" + 
                    StringLiterals.COMMA +
                            
                    Util.InsertIn(ColumnNames.FOREIGN_KEY, StringLiterals.SPACE) + 
                    SqlDataTypes.NVARCHAR +
                    "(255)"
                )
            );

            try
            {
                dbDriver.ExecuteNonQuery(createQuery);
                foreach (GridColumnBase column in field.Columns)
                {
                    if (!(column is PredefinedColumn))
                    {
                        CreateTableColumn(column, tableName);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Creates a datatable corresponding to a GridField.
        /// </summary>
        /// <param name="view">View of a table that references the original table.</param>
        /// <param name="startingId">Starting Id for new table.</param>
        /// <param name="field">GridField object.</param>
        private void CreateDataTableForGrid(Page page, GridField field)
        {
            #region Input Validation
            if (page == null)
            {
                throw new ArgumentNullException("view");
            }
            #endregion Input Validation

            string tableName = field.TableName;

            if (TableExists(tableName))
            {
                return; // DeleteTable(tableName);
            }

            Query createQuery = dbDriver.CreateQuery
                ("create table " + dbDriver.InsertInEscape(tableName) +
                    Util.InsertInParantheses
                    (
                        Util.InsertIn(ColumnNames.UNIQUE_KEY, StringLiterals.SPACE) +
                        "INT identity (1,1) primary key not null" + 
                        StringLiterals.COMMA +

                        Util.InsertIn(ColumnNames.UNIQUE_ROW_ID, StringLiterals.SPACE) +
                        SqlDataTypes.NVARCHAR +
                        "(255)," +

                        Util.InsertIn(ColumnNames.GLOBAL_RECORD_ID, StringLiterals.SPACE) +
                        SqlDataTypes.NVARCHAR +
                        "(255)," +

                        Util.InsertIn(ColumnNames.REC_STATUS, StringLiterals.SPACE) + 
                        "INT default 1" + 
                        StringLiterals.COMMA +

                        Util.InsertIn(ColumnNames.FOREIGN_KEY, StringLiterals.SPACE) + 
                        SqlDataTypes.NVARCHAR +
                        "(255)"
                    )
                );
            try
            {
                dbDriver.ExecuteNonQuery(createQuery);
                foreach (GridColumnBase column in field.Columns)
                {
                    if (!(column is PredefinedColumn))
                    {
                        CreateTableColumn(column, tableName);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }

        /// <summary>
        /// DeleteDataTableForGrid
        /// </summary>
        /// <param name="view">view</param>
        /// <param name="field">grid field</param>
        public void DeleteDataTableForGrid(View view, GridField field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            string tableName = view.Name + field.Page.Id.ToString() + field.Name;

            if (TableExists(tableName))
            {
                DeleteTable(tableName);
            }
        }

        /// <summary>
        /// Change the name of the column in current database
        /// </summary>
        /// <param name="columnName">original name of the column</param>
        /// <param name="newColumnName">new name of the column</param>
        /// <returns>Boolean</returns>
        public void RenameColumn(string columnName, string newColumnName)
        {
            // dpb todo - throw new Exception("RenameColumn is not implemented.");
        }

        /// <summary>
        /// Saves rows in a GridField. NOTE: Must be called from a datagrid or datagridview event().
        /// </summary>
        /// <param name="view">Current View in Project.</param>
        /// <param name="recordID">Id of current Record.</param>
        /// <param name="field">GridField object.</param>
        /// <param name="dTable">DataTable to save.</param>
        /// <returns>Number rows affected.</returns>
        public int SaveGridRecord(View view, int recordID, GridField field, System.Data.DataTable fieldSourceTable)
        {
            int iRowsAffected = 0;
            if (fieldSourceTable == null) return iRowsAffected;

            string gridLevelGlobalRecordId = Guid.NewGuid().ToString();
            foreach (DataRow row in fieldSourceTable.Rows)
            {
                if (row[ColumnNames.GLOBAL_RECORD_ID].ToString().Length > 0)
                {
                    gridLevelGlobalRecordId = row[ColumnNames.GLOBAL_RECORD_ID].ToString();
                    break;
                }
            }

            DataTable existingGridRowTable = GetGridTableData(view, field);
            string tableName = string.Empty;
            tableName = Util.InsertIn(dbDriver.InsertInEscape(field.TableName), StringLiterals.SPACE);

            if (existingGridRowTable.Rows.Count > 0)
            {
                gridLevelGlobalRecordId = existingGridRowTable.Rows[0][ColumnNames.GLOBAL_RECORD_ID].ToString();
                
                DataRow[] deleteRowCandidates = existingGridRowTable.Select(
                    string.Format("GlobalRecordId='{0}'", 
                    gridLevelGlobalRecordId));

                foreach (DataRow row in deleteRowCandidates)
                {
                    string candidateUniqueRowId = row[ColumnNames.UNIQUE_ROW_ID].ToString();

                    DataRow[] matches = fieldSourceTable.Select(
                        string.Format("UniqueRowId='{0}'",
                        candidateUniqueRowId));

                    if (matches.Length == 0)
                    {
                        string deleteStatement = string.Format("DELETE FROM {0} WHERE UniqueRowId='{1}' ",
                            tableName,
                            candidateUniqueRowId);

                        Query deleteQuery = dbDriver.CreateQuery(deleteStatement);
                        iRowsAffected += dbDriver.ExecuteNonQuery(deleteQuery);
                    }
                }
            }

            foreach (DataRow row in fieldSourceTable.Rows)
            {
                if (row.RowState != DataRowState.Deleted)
                {
                    try
                    {
                        string uniqueRowId = row[ColumnNames.UNIQUE_ROW_ID].ToString();
                        bool isInsert = false;

                        isInsert = String.IsNullOrEmpty(uniqueRowId);

                        if (isInsert)
                        {
                            uniqueRowId = Guid.NewGuid().ToString();
                            
                            iRowsAffected += InsertGridRecord(
                                view.CurrentGlobalRecordId, 
                                gridLevelGlobalRecordId, 
                                uniqueRowId, 
                                field, 
                                row, 
                                tableName);
                        }
                        else
                        {
                            StringBuilder sbUpdate = new StringBuilder();
                            sbUpdate.Append("update " + tableName);
                            sbUpdate.Append(Util.InsertIn("set", StringLiterals.SPACE));
                            foreach (GridColumnBase column in field.Columns)
                            {
                                if (!(column is PredefinedColumn))
                                {
                                    sbUpdate.Append(StringLiterals.LEFT_SQUARE_BRACKET);
                                    sbUpdate.Append(column.Name);
                                    sbUpdate.Append(StringLiterals.RIGHT_SQUARE_BRACKET);
                                    sbUpdate.Append(StringLiterals.EQUAL);
                                    if (column is NumberColumn)
                                    {
                                        if (row[column.Name].ToString().Length == 0)
                                        {
                                            sbUpdate.Append("null");
                                        }
                                        else
                                        {
                                            sbUpdate.Append(row[column.Name].ToString());
                                        }
                                    }
                                    else if (column is DateColumn 
                                        || column is TimeColumn 
                                        || column is DateTimeColumn
                                        )
                                    {
                                        sbUpdate.Append((string.IsNullOrEmpty(row[column.Name].ToString()) ? "null" : Util.InsertInSingleQuotes(row[column.Name].ToString())));
                                    }
                                    else if ( column is CheckboxColumn)
                                    {
                                        string valueString = "0";

                                        if (row[column.Name].ToString().ToLower().Contains("true"))
                                        {
                                            valueString = "1";
                                        }

                                        sbUpdate.Append(valueString);
                                    }
                                    else if (column is YesNoColumn)
                                    {
                                        string valueString = "null";

                                        if (row[column.Name].ToString().Contains("1"))
                                        {
                                            valueString = "1";
                                        }
                                        else if (row[column.Name].ToString().Contains("0"))
                                        {
                                            valueString = "0";
                                        }

                                        sbUpdate.Append(valueString);
                                    }
                                    else
                                    {
                                        sbUpdate.Append(Util.InsertInSingleQuotes(row[column.Name].ToString()));
                                    }
                                    sbUpdate.Append(", ");
                                }
                            }
                            
                            sbUpdate.Remove(sbUpdate.Length - 2, 2);
                            sbUpdate.Append(Util.InsertIn("where", StringLiterals.SPACE));
                            sbUpdate.Append(ColumnNames.UNIQUE_ROW_ID + StringLiterals.EQUAL);
                            sbUpdate.Append(Util.InsertInSingleQuotes(row[ColumnNames.UNIQUE_ROW_ID].ToString()));

                            Query updateQuery = dbDriver.CreateQuery(sbUpdate.ToString());
                            iRowsAffected += dbDriver.ExecuteNonQuery(updateQuery);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new GeneralException("Could not save grid record.", ex);
                    }
                }
            }

            DataTable existingGridRowTablePostUpdate = GetGridTableData(view, field);
            field.DataSource = existingGridRowTablePostUpdate;

            return iRowsAffected;
        }

        /// <summary>
        /// Inserts a grid record if update grid record fails as attempted by SaveGridRecord().
        /// </summary>
        /// <param name="recordID">Id of current Record.</param>
        /// <param name="field">GridField object.</param>
        /// <param name="dRow">DataRow of DataTable to save.</param>
        /// <param name="sTableName">View name plus field name</param>
        /// <returns>Number rows affected.</returns>
        private int InsertGridRecord(string globalRecordId, string gridLevelGlobalRecordId, string uniqueRowId, GridField field, DataRow dRow, string sTableName)
        {
            try
            {
                StringBuilder sbInsert = new StringBuilder();
                WordBuilder fieldNames = new WordBuilder(StringLiterals.COMMA);
                WordBuilder fieldValues = new WordBuilder(StringLiterals.COMMA);
                sbInsert.Append("insert into ");
                sbInsert.Append(Util.InsertIn(sTableName, StringLiterals.SPACE));

                foreach (GridColumnBase column in field.Columns)
                {
                    if (column.Name != ColumnNames.UNIQUE_KEY)
                    {
                        fieldNames.Append(dbDriver.InsertInEscape(column.Name));
                    }
                }
                
                sbInsert.Append(Util.InsertInParantheses(fieldNames.ToString()));
                sbInsert.Append(Util.InsertIn("values", StringLiterals.SPACE));

                foreach (GridColumnBase column in field.Columns)
                {
                    if (column.Name != ColumnNames.UNIQUE_KEY)
                    {
                        if (column is NumberColumn)
                        {
                            if (dRow[column.Name].ToString().Length == 0)
                            {
                                fieldValues.Append("null");
                            }
                            else
                            {
                                fieldValues.Append(dRow[column.Name].ToString());
                            }
                        }
                        else
                        {
                            if (column.Name == ColumnNames.FOREIGN_KEY)
                            {
                                fieldValues.Append(Util.InsertInSingleQuotes(globalRecordId));
                            }
                            else if (column.Name == ColumnNames.GLOBAL_RECORD_ID)
                            {
                                fieldValues.Append(Util.InsertInSingleQuotes(gridLevelGlobalRecordId));
                            }
                            else if (column.Name == ColumnNames.UNIQUE_ROW_ID)
                            {
                                fieldValues.Append(Util.InsertInSingleQuotes(uniqueRowId));
                            }
                            else if (column is TimeColumn 
                                || column is DateColumn 
                                || column is DateTimeColumn )
                            {
                                string valueString = "null";

                                if(string.IsNullOrEmpty(dRow[column.Name].ToString()) == false)
                                {
                                    valueString = Util.InsertInSingleQuotes(dRow[column.Name].ToString());
                                }
                                
                                fieldValues.Append(valueString);
                            }
                            else if (column is CheckboxColumn)
                            {
                                string valueString = "0";

                                if (dRow[column.Name].ToString().ToLower().Contains("true"))
                                {
                                    valueString = "1";
                                }

                                fieldValues.Append(valueString);
                            }
                            else if (column is YesNoColumn)
                            {
                                string valueString = "null";

                                if (dRow[column.Name].ToString().Contains("1"))
                                {
                                    valueString = "1";
                                }
                                else if (dRow[column.Name].ToString().Contains("0"))
                                {
                                    valueString = "0";
                                }

                                fieldValues.Append(valueString);
                            }
                            else if (column.Name == ColumnNames.REC_STATUS)
                            {
                                fieldValues.Append("1");
                            }
                            else
                            {
                                fieldValues.Append(Util.InsertInSingleQuotes(dRow[column.Name].ToString()));
                            }
                        }
                    }
                }
                
                sbInsert.Append(Util.InsertInParantheses(fieldValues.ToString()));
                Query insertQuery = dbDriver.CreateQuery(sbInsert.ToString());
                return dbDriver.ExecuteNonQuery(insertQuery);
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not insert grid record", ex);
            }
        }
        #endregion Private Methods

        #region IDbDriver Members
        /// <summary>
        /// Returns the full name of the data source. Typically used for display purposes
        /// </summary>
        public string FullName
        {
            get
            {
                return dbDriver.FullName;
            }
        }

        /// <summary>
        /// Gets/sets the Database name.
        /// </summary>
        public string DbName
        {
            get
            {
                return this.project.CollectedDataDbInfo.DBName;   //dbDriver.DbName;
            }
            set
            {
                this.project.CollectedDataDbInfo.DBName = value; // dbDriver.DbName = value;
            }
        }

        /// <summary>
        /// Gets the database connection string.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return dbDriver.ConnectionString;
            }
            set
            {
                dbDriver.ConnectionString = value;
            }
        }

        /// <summary>
        /// Name of the source of the data.
        /// </summary>
        public string DataSource
        {
            get
            {
                return dbDriver.DataSource;
            }
        }

        /// <summary>
        /// Gets a user-friendly description of the otherwise bewildering connection string.
        /// </summary>
        public string ConnectionDescription
        {
            get
            {
                return dbDriver.ConnectionDescription;
            }
        }

        /// <summary>
        /// Returns the maximum number of columns a table can have.
        /// </summary>
        public int TableColumnMax
        {
            get { return dbDriver.TableColumnMax; }
        }

        /// <summary>
        /// Get nonview tables as datatable
        /// </summary>
        /// <returns>All table names of tables that are not metatables</returns>
        public DataTable GetNonViewTablesAsDataTable()
        {
            try
            {
                DataRow dataRow;
                DataTable tables = dbDriver.GetTableSchema(); //now GetTableSchema only gets user tables  /zack 1/30/08

                DataTable viewsAndTables = new DataTable("ViewsAndTables");
                viewsAndTables.Columns.Add(ColumnNames.NAME);
                DataRow[] rows = tables.Select(ColumnNames.TABLE_NAME + " not like 'meta%'");
                foreach (DataRow row in rows)
                {
                    string tableName = row[ColumnNames.TABLE_NAME].ToString();

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
        /// Gets Primary_Keys schema information about an OLE table as a DataTable class.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>DataTable with schema information.</returns>
        public DataTable GetPrimaryKeysAsDataTable(string tableName)
        {

            DataTable keys = dbDriver.GetTableKeysSchema(tableName);
            return keys;
        }

        /// <summary>
        /// Test database connectivity
        /// </summary>
        /// <returns>Returns true if connection can be made successfully</returns>
        public bool TestConnection()
        {
            return dbDriver.TestConnection();
        }

        /// <summary>
        /// Creates a table with the given columns
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="columns">List of columns</param>
        public void CreateTable(string tableName, List<Epi.Data.TableColumn> columns)
        {
            dbDriver.CreateTable(tableName, columns);
        }

        /// <summary>
        /// Creates a table with a System.Data.DataTable
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="dataTable">Table</param>
        public void CreateTable(string tableName, System.Data.DataTable dataTable)
        {
            List<Epi.Data.TableColumn> columns = new List<Epi.Data.TableColumn>();

            for (int i = 0; dataTable.Columns.Count > i; i++)
            {
                Epi.Data.TableColumn col = new Epi.Data.TableColumn(
                        dataTable.Columns[i].ColumnName,
                        GetGenericDbColumnType(dataTable.Columns[i]),
                        dataTable.Columns[i].AllowDBNull);

                columns.Add(col);
            }

            dbDriver.CreateTable(tableName, columns);

            foreach (DataRow row in dataTable.Rows)
            {
                InsertDataTableRow(tableName, dataTable, row);
            }
        }

        /// <summary>
        /// GetGenericDbColumnType
        /// </summary>
        /// <param name="dataColumn">Data Column</param>
        /// <returns>GenericDbColumnType</returns>
        public Epi.Data.GenericDbColumnType GetGenericDbColumnType(System.Data.DataColumn dataColumn)
        {
            switch (dataColumn.DataType.Name)
            {
                case "Int16":
                    return Epi.Data.GenericDbColumnType.Int16;
                case "Int32":
                    return Epi.Data.GenericDbColumnType.Int32;
                case "Int64":
                    return Epi.Data.GenericDbColumnType.Int64;
                case "String":
                    return Epi.Data.GenericDbColumnType.String;
                case "Byte":
                    return Epi.Data.GenericDbColumnType.Byte;
                case "Boolean":
                    return Epi.Data.GenericDbColumnType.Boolean;
                case "Decimal":
                    return Epi.Data.GenericDbColumnType.Decimal;
                case "Double":
                    return Epi.Data.GenericDbColumnType.Double;
                case "DateTime":
                    return Epi.Data.GenericDbColumnType.DateTime;
                case "UInt16":
                    return Epi.Data.GenericDbColumnType.UInt16;
                case "UInt32":
                    return Epi.Data.GenericDbColumnType.UInt32;
                case "UInt64":
                    return Epi.Data.GenericDbColumnType.UInt64;
                case "Single":
                    return Epi.Data.GenericDbColumnType.Single;
                case "SByte":
                    return Epi.Data.GenericDbColumnType.SByte;
                default:
                    return Epi.Data.GenericDbColumnType.Object;
            }
        }

        /// <summary>
        /// Get schema of DataTable
        /// </summary>
        /// <returns>Represents one table of in-memory data.</returns>
        public System.Data.DataTable GetTableSchema()
        {
            return dbDriver.GetTableSchema();
        }

        /// <summary>
        /// Returns the distinct values of a column from a data table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="columnName">Name of the column</param>
        /// <returns></returns>
        public ArrayList SelectDistinct(string tableName, string columnName)
        {
            string queryString = "select" + StringLiterals.SPACE + "distinct" + Util.InsertIn(dbDriver.InsertInEscape(columnName), StringLiterals.SPACE) +
                "from" + Util.InsertIn(dbDriver.InsertInEscape(tableName), StringLiterals.SPACE);
            Query query = dbDriver.CreateQuery(queryString);
            DataTable dt = dbDriver.Select(query);
            ArrayList arrayList = new ArrayList(dt.Rows.Count);
            foreach (DataRow dr in dt.Rows)
            {
                arrayList.Add(dr[0]);
            }
            return arrayList;
        }

        /// <summary>
        /// Returns the values of a column from a data table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="columnName">Name of the column</param>
        /// <returns></returns>
        public ArrayList SelectColumnData(string tableName, string columnName)
        {
            string queryString = "select" + Util.InsertIn(dbDriver.InsertInEscape(columnName), StringLiterals.SPACE) +
                "from" + Util.InsertIn(dbDriver.InsertInEscape(tableName), StringLiterals.SPACE);
            Query query = dbDriver.CreateQuery(queryString);
            DataTable dt = dbDriver.Select(query);
            ArrayList arrayList = new ArrayList(dt.Rows.Count);
            foreach (DataRow dr in dt.Rows)
            {
                arrayList.Add(dr[0]);
            }
            return arrayList;
        }

        /// <summary>
        /// Executes a SQL query to select records into a data table
        /// </summary>
        /// <param name="selectQuery">Container for SQL statement and related parameters</param>
        /// <returns>Represents one table of in-memory data.</returns>
        public System.Data.DataTable Select(Epi.Data.Query selectQuery)
        {
            return dbDriver.Select(selectQuery);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectQuery">Container for SQL statement and related parameters</param>
        /// <param name="table"></param>
        /// <returns>Represents one table of in-memory data.</returns>
        public System.Data.DataTable Select(Epi.Data.Query selectQuery, System.Data.DataTable table)
        {
            return dbDriver.Select(selectQuery, table);
        }

        /// <summary>
        /// Gets a value indicating whether or not a specific column exists for a table in the database
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="columnName">Name of the column</param>
        /// <returns>Test results of column's existence</returns>
        public bool ColumnExists(string tableName, string columnName)
        {
            return dbDriver.ColumnExists(tableName, columnName);
        }

        /// <summary>
        /// Executes a SQL statement that does not return anything
        /// </summary>
        /// <param name="nonQueryStatement">Container for DML SQL statement and related parameters</param>
        /// <returns>Number of rows affected</returns>
        public int ExecuteNonQuery(Epi.Data.Query nonQueryStatement)
        {
            return dbDriver.ExecuteNonQuery(nonQueryStatement);
        }

        /// <summary>
        /// Executes a SQL statement that does not return anything
        /// </summary>
        /// <param name="nonQueryStatement">The query to be executed against the database</param>
        /// <param name="transaction">The transaction through which to execute the nonquery</param>
        /// <returns>Number of rows affected</returns>
        public int ExecuteNonQuery(Epi.Data.Query nonQueryStatement, System.Data.IDbTransaction transaction)
        {
            return dbDriver.ExecuteNonQuery(nonQueryStatement, transaction);
        }

        /// <summary>
        /// Executes a scalar query against the database.
        /// </summary>
        /// <param name="scalarStatement">SQL statement and related parameters.</param>
        /// <returns>Scalar query results</returns>
        public object ExecuteScalar(Epi.Data.Query scalarStatement)
        {
            return dbDriver.ExecuteScalar(scalarStatement);
        }
        /// <summary>
        /// Executes a scalar query against the database.
        /// </summary>
        /// <param name="scalarStatement">SQL statement and related parameters.</param>
        /// <param name="transaction">Transaction performed at data source.</param>
        /// <returns>Scalar query results</returns>
        public object ExecuteScalar(Epi.Data.Query scalarStatement, System.Data.IDbTransaction transaction)
        {
            return dbDriver.ExecuteScalar(scalarStatement, transaction);
        }

        /// <summary>
        /// Get Grid field Table Data for current view record.
        /// </summary>
        /// <param name="view">Current view of project.</param>
        /// <param name="field">Grid field</param>
        /// <returns>Grid field table contents.</returns>
        public System.Data.DataTable GetGridTableData(View view, GridField field)
        {
            StringBuilder sbSelect = new StringBuilder();
            WordBuilder wbSelect = new WordBuilder(StringLiterals.COMMA);
            // Begin Select clause
            foreach (GridColumnBase column in ((GridField)field).Columns)
            {
                wbSelect.Append(dbDriver.InsertInEscape(column.Name));
            }
            sbSelect.Append("select");
            sbSelect.Append(StringLiterals.SPACE);
            sbSelect.Append(wbSelect.ToString());
            // End Select clause
            // Begin From clause
            sbSelect.Append(Util.InsertIn("from", StringLiterals.SPACE));
            sbSelect.Append(dbDriver.InsertInEscape(view.TableName + field.Page.Id.ToString() + field.Name));
            sbSelect.Append(Util.InsertIn("where", StringLiterals.SPACE));
            sbSelect.Append(dbDriver.InsertInEscape(ColumnNames.FOREIGN_KEY) + StringLiterals.EQUAL + "'" + view.CurrentGlobalRecordId + "'");
            // End From clause
            Data.Query selectQuery = dbDriver.CreateQuery(sbSelect.ToString());
            return dbDriver.Select(selectQuery);
        }
        /// <summary>
        /// Returns the table keys schema for a table.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <returns>Table Keys schema contents.</returns>
        public System.Data.DataTable GetTableKeysSchema(string tableName)
        {
            return dbDriver.GetTableKeysSchema(tableName);
        }
        /// <summary>
        /// Returns contents of a table.
        /// </summary>
        /// <param name="tableName">The name of the table to query.</param>
        /// <returns>Table data contents.</returns>
        public System.Data.DataTable GetTableData(string tableName)
        {
            return dbDriver.GetTableData(tableName);
        }
        /// <summary>
        /// Returns contents of a table.
        /// </summary>
        /// <param name="tableName">The name of the table to query.</param>
        /// <param name="columnNames">Comma delimited string of column names and asc/DESC order.</param>
        /// <returns>Table data contents.</returns>
        public System.Data.DataTable GetTableData(string tableName, string columnNames)
        {
            return dbDriver.GetTableData(tableName, columnNames);
        }
        /// <summary>
        /// Returns contents of a table.
        /// </summary>
        /// <param name="tableName">The name of the table to query.</param>
        /// <param name="columnNames">Comma delimited string of column names and asc/DESC order.</param>
        /// <param name="sortCriteria">The criteria to sort by.</param>
        /// <returns>Table data contents.</returns>
        public System.Data.DataTable GetTableData(string tableName, string columnNames, string sortCriteria)
        {
            return dbDriver.GetTableData(tableName, columnNames, sortCriteria);
        }

        /// <summary>
        /// Execute a command at a data source and return a forward-only result set.
        /// </summary>
        /// <param name="selectQuery">SQL statement and related parameters.</param>
        /// <returns>Forward-only result set.</returns>
        public System.Data.IDataReader ExecuteReader(Epi.Data.Query selectQuery)
        {
            return dbDriver.ExecuteReader(selectQuery);
        }

        /// <summary>
        /// Gets an OLE-compatible connection string.
        /// This is needed by EpiMap, as ESRI does not understand .NET connection strings.
        /// See http://www.connectionstrings.com for an OLE-compatible connection string for your database.
        /// </summary>
        public string OleConnectionString
        {
            get
            {
                return dbDriver.OleConnectionString;
            }
        }

        /// <summary>
        /// Begins a database transaction.
        /// </summary>
        /// <returns>Represents a transaction to be performed at a data source, and is implemented by .NET Framework data providers that access relational databases.</returns>
        public System.Data.IDbTransaction OpenTransaction()
        {
            return dbDriver.OpenTransaction();
        }

        /// <summary>
        /// Begins a database transaction.
        /// </summary>
        /// <param name="isolationLevel">Specifies the transaction locking behavior for the connection.</param>
        /// <returns>Represents a transaction to be performed at a data source, and is implemented by .NET Framework data providers that access relational databases.</returns>
        public System.Data.IDbTransaction OpenTransaction(System.Data.IsolationLevel isolationLevel)
        {
            return dbDriver.OpenTransaction(isolationLevel);
        }

        /// <summary>
        /// Closes a database transaction connection.
        ///  Developer should commit or rollback transaction prior to calling this method.
        /// </summary>
        /// <param name="transaction"></param>
        public void CloseTransaction(System.Data.IDbTransaction transaction)
        {
            dbDriver.CloseTransaction(transaction);
        }

        /// <summary>
        /// Modify an existing record in the table.
        /// </summary>
        /// <param name="dataTable">Represents the table of in-memory data to be modified.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="insertQuery">SQL Insert statement.</param>
        /// <param name="updateQuery">SQL Update statement.</param>
        public void Update(System.Data.DataTable dataTable, string tableName, Epi.Data.Query insertQuery, Epi.Data.Query updateQuery)
        {
            dbDriver.Update(dataTable, tableName, insertQuery, updateQuery);
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            dbDriver.Dispose();
        }

        /// <summary>
        /// Creates an ANSI SQL-92 statement container.
        /// </summary>
        /// <param name="ansiSqlStatement">A SQL query following ANSI SQL-92 standards</param>
        /// <returns>Container for SQL statements and related parameters</returns>
        public Epi.Data.Query CreateQuery(string ansiSqlStatement)
        {
            return dbDriver.CreateQuery(ansiSqlStatement);
        }

        
        #endregion  //IDbDriver Members

    }
}
