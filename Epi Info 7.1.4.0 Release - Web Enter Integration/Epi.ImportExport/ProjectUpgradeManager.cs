using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Text;

using Epi;
using Epi.Epi2000;
using Epi.Collections;
using Epi.Fields;
using Epi.Data;
using Epi.DataSets;
//using Epi.Windows.MakeView.Forms;

namespace Epi.ImportExport
{
	#region Delegate Definitions

	/// <summary>
	/// Delegate for the ImportStarted event
	/// </summary>
	public delegate void ImportStartedEventHandler(object o, ImportStartedEventArgs e);
    
    /// <summary>
    /// Delegate for the ImportStatus Event
    /// </summary>
    /// <param name="o"></param>
    /// <param name="e">.NET supplied event parameters</param>
	public delegate void ImportStatusEventHandler(object o, MessageEventArgs e);
    #endregion Delegate Definitions
	
	/// <summary>
	/// Imports Epi2000 data format to Epi Info 7
	/// </summary>
	public class ProjectUpgradeManager
	{
		#region Events

		/// <summary>
		/// Occurs when the Import process has just started. Event fires with the total number of views to be converted.
		/// </summary>
		public event ImportStartedEventHandler ImportStarted;

        /// <summary>
        /// Event hanndler for the ImportStatus event
        /// </summary>
		public event ImportStatusEventHandler ImportStatus;

        /// <summary>
        /// Marks end of Import process
        /// </summary>
        public event SimpleEventHandler ImportEnded;		
		
        /// <summary>
        /// Occurs when a view in the project has been Imported.
        /// </summary>
        public event EventHandler ViewImported;

        /// <summary>
        /// Occurs when an artifact in the project has been Imported.
        /// </summary>
        public event EventHandler ArtifactImported;

		#endregion

		#region Private Members

        private Epi.Epi2000.Project sourceProject = null;
		private Project destinationProject = null;
        //private Canvas canvas = null;

        private StringBuilder CheckCode = new StringBuilder();
        #endregion Private Members

		#region Constructors

		/// <summary>
		/// Private constructor cannot be used
		/// </summary>
		private ProjectUpgradeManager()
		{
		}

        /// <summary>
        /// Constructs and initializes a new Import manager object
        /// </summary>
        /// <param name="sourceProj">The Epi Info 3.x project</param>
        /// <param name="destProj">The resulting new project</param>
        public ProjectUpgradeManager(Epi.Epi2000.Project sourceProj, Project destProj)
        {
            this.sourceProject = sourceProj;
            this.destinationProject = destProj;
        }

		#endregion

		#region Public Methods

		/// <summary>
		/// Begins the Import process
		/// </summary>
		public bool Import(/*Canvas currentCanvas*/)
		{
            int count = 0;
            MessageEventHandler handlerImportStatus = null;
            TableCopyStatusEventHandler handlerTableCopyStatus = null;
            MessageEventHandler handlerTableCopyEnd = null;

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

			try
			{                
                //canvas = currentCanvas;

                Epi.Data.Services.CollectedDataProvider collectedDataProvider = destinationProject.CollectedData;
                Epi.Data.IDbDriver db = collectedDataProvider.GetDatabase();

                count = sourceProject.Views.Count;
                int numViewsToSkip = 0;
                
                // Check that we won't overwrite existing data, and if so, prompt the user and
                // explain which tables will be overwritten if they proceed. This should only
                // be necessary with non-Access projects.
                if (destinationProject.CollectedDataDriver != "Epi.Data.Office.AccessDBFactory, Epi.Data.Office")
                {
                    bool importWillOverwriteExistingTables = false;
                    string existingTables = string.Empty;
                    int existingTablesCount = 0;
                    List<string> tableNames = sourceProject.Metadata.GetTableNames();
                    tableNames.Remove("metaBackgrounds");
                    tableNames.Remove("metaDataTypes");
                    tableNames.Remove("metaDbInfo");
                    tableNames.Remove("metaFields");
                    tableNames.Remove("metaFieldTypes");
                    tableNames.Remove("metaGridColumns");
                    tableNames.Remove("metaImages");
                    tableNames.Remove("metaLayerRenderTypes");
                    tableNames.Remove("metaLayers");
                    tableNames.Remove("metaMapLayers");
                    tableNames.Remove("metaMapPoints");
                    tableNames.Remove("metaMaps");
                    tableNames.Remove("metaPages");
                    tableNames.Remove("metaPatterns");
                    tableNames.Remove("metaPrograms");
                    tableNames.Remove("metaViews");

                    foreach (string s in tableNames)
                    {
                        if (db.TableExists(s))
                        {
                            existingTablesCount++;
                            Logger.Log(DateTime.Now + ":  " + string.Format(SharedStrings.IMPORT_WARNING_TABLE_NAME, s));
                            if (existingTablesCount < 15)
                            {                                
                                existingTables = existingTables + s + ", ";
                                importWillOverwriteExistingTables = true;
                            }                            
                        }                        
                    }
                    if (importWillOverwriteExistingTables && existingTables.EndsWith(", "))
                    {
                        existingTables = existingTables.Remove(existingTables.Length - 2, 2);
                        if (existingTablesCount >= 15)
                        {
                            existingTables = existingTables + " and " + (existingTablesCount - 15).ToString() + " others.";
                        }
                        string dataExistsMessage = string.Format(SharedStrings.IMPORT_DATA_ALREADY_EXISTS, existingTables);

                        //DialogResult result = Epi.Windows.MsgBox.Show(dataExistsMessage, SharedStrings.IMPORT_OVERWRITE_DATA, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                        DialogResult result = MessageBox.Show(dataExistsMessage, SharedStrings.IMPORT_OVERWRITE_DATA, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);


                        if (result == DialogResult.Cancel)
                        {
                            Logger.Log(DateTime.Now + ":  " + string.Format(SharedStrings.IMPORT_NOTIFICATION_USER_WILL_NOT_OVERWRITE, System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString()));
                            return false;
                        }
                        else
                        {
                            Logger.Log(DateTime.Now + ":  " + string.Format(SharedStrings.IMPORT_NOTIFICATION_USER_WILL_OVERWRITE, System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString()));
                        }
                    }
                }

                // Check for wide tables and disallow their import under specific conditions
                Dictionary<string, int> viewsToSkip = new Dictionary<string, int>();

                foreach (Epi.Epi2000.View sourceView in sourceProject.Views)
                {
                    string importMessage = string.Empty;
                    if(ShouldProcessView(sourceView, out importMessage) == false)
                    {
                        numViewsToSkip++;
                        viewsToSkip.Add(sourceView.Name, GetDataFieldsInSourceView(sourceView));
                        Logger.Log(DateTime.Now + ":  " + importMessage);
                        continue;
                    }
                }

                if (numViewsToSkip > 0)
                {
                    string viewString = string.Empty;
                    foreach (KeyValuePair<string, int> viewToSkip in viewsToSkip)
                    {                        
                        viewString = viewString + viewToSkip.Key + " (" + viewToSkip.Value.ToString() + " fields), ";
                    }
                    if (viewString.EndsWith("), "))
                    {
                        viewString = viewString.Remove(viewString.Length - 2, 2);
                    }

                    MessageBox.Show(
                        string.Format(
                        SharedStrings.IMPORT_SKIPPED_VIEWS, viewString),
                        SharedStrings.INFORMATION, MessageBoxButtons.OK, MessageBoxIcon.Information);

                    //Epi.Windows.MsgBox.ShowInformation(
                    //    string.Format(
                    //    SharedStrings.IMPORT_SKIPPED_VIEWS, viewString));
                }

                if (numViewsToSkip >= count)
                {
                    // stop import because we no longer have any legitimate views to process.
                    //Epi.Windows.MsgBox.ShowInformation(SharedStrings.IMPORT_NO_VALID_VIEWS);

                    MessageBox.Show(
                        SharedStrings.IMPORT_NO_VALID_VIEWS,
                        SharedStrings.INFORMATION, MessageBoxButtons.OK, MessageBoxIcon.Information);

                    RaiseEventImportEnded();
                    return false;
                }

                // Get the # of artifacts present in a SQL server database.
                if (destinationProject.CollectedDataDriver != "Epi.Data.Office.AccessDBFactory, Epi.Data.Office")
                {
                    int artifacts = 0;
                    // TODO: GetTableCount is returning an error. After fixing the error, uncomment the following the line.
                    count = sourceProject.CollectedData.GetTableCount();
                    foreach (Epi.Epi2000.View sourceView in sourceProject.Views) 
                    {
                        artifacts = artifacts + sourceProject.Metadata.GetFieldsAsDataTable(sourceView.Name).Rows.Count;
                        if (string.IsNullOrEmpty(sourceView.TableName) == false)
                        {
                            artifacts = artifacts + (sourceView.GetRecordCount() / 1000);
                        }
                        if (sourceView.IsRelatedView == true)
                        {
                            artifacts++;
                        }
                    }
                    RaiseEventImportStarted(count + artifacts);
                }                

                handlerImportStatus = new MessageEventHandler(RaiseEventImportStatus);
                handlerTableCopyStatus = new TableCopyStatusEventHandler(ReportTableCopyStatus);
                handlerTableCopyEnd = new MessageEventHandler(ReportTableCopyEnd);
                
                // Set event handlers to capture code table copy status ...
                sourceProject.TableCopyBeginEvent += handlerImportStatus; // Subscribe
                sourceProject.TableCopyStatusEvent += handlerTableCopyStatus; // Subscribe
                sourceProject.TableCopyEndEvent += handlerTableCopyEnd; // Subscribe
                
                if (destinationProject.CollectedDataDriver != "Epi.Data.Office.AccessDBFactory, Epi.Data.Office")
                {
                    sourceProject.CopyCodeTablesTo(destinationProject);
                }
                else
                {
                    string[] destinationString = destinationProject.CollectedData.ConnectionString.Split(';');
                    string destinationDB = null;
                    if (destinationString.Length > 1)
                    {
                        for (int i = 0; i < destinationString.Length; i++)
                        {
                            string[] fieldValue = destinationString[i].Split('=');
                            if (fieldValue[0].ToUpper() == "DATA SOURCE")
                            {
                                destinationDB = fieldValue[1];
                                destinationDB = destinationDB.Trim('"');
                                break;
                            }
                        }

                        if (System.IO.File.Exists(destinationDB))
                        {
                            System.IO.File.Delete(destinationDB);
                        }

                        // copy source to destination
                        System.IO.File.Copy(sourceProject.FilePath, destinationDB);
                    }
                    else
                    {                        
                        // no need to copy the database 
                        // the source database will be upgraded in-place
                        destinationDB = destinationString[0];
                    }

                    // reassign source project to new source
                    // sourceProject = new Epi.Epi2000.Project(destinationDB);

                    try
                    {
                        Epi.Data.Services.MetadataDbProvider typedMetadata = destinationProject.Metadata as Epi.Data.Services.MetadataDbProvider;
                        // Get Collected data's Db driver and attach it to Metadata.
                        typedMetadata.AttachDbDriver(destinationProject.CollectedData.GetDbDriver());
                        typedMetadata.CreateMetadataTables();
                    }
                    catch (System.Exception ex)
                    {
                        System.Console.WriteLine(ex.ToString());
                    }
                    
                }
                CopyViews();

                if (CheckCode.Length > 0)
                {
                    Logger.Log(DateTime.Now + ":  " + SharedStrings.IMPORT_WARNING_CHECKCODE);
                }

                Logger.Log(DateTime.Now + ":  " + string.Format(SharedStrings.IMPORT_COMPLETED, destinationProject.Name, destinationProject.Views.Count.ToString(), sourceProject.Views.Count.ToString()));
			}
			finally
			{
                stopwatch.Stop();
                Logger.Log(DateTime.Now + ":  " + string.Format(SharedStrings.IMPORT_NOTIFICATION_ELAPSED_TIME, destinationProject.Name, stopwatch.Elapsed.ToString()));

                // Unsubscribe
                sourceProject.TableCopyBeginEvent -= handlerImportStatus; 
                sourceProject.TableCopyStatusEvent -= handlerTableCopyStatus;
                sourceProject.TableCopyEndEvent -= handlerTableCopyEnd;

                RaiseEventImportEnded();

                Epi.ImportExport.Dialogs.ImportMessagesDialog importResults = new ImportExport.Dialogs.ImportMessagesDialog();
                importResults.TimeElapsed = stopwatch.Elapsed;
                importResults.ShowDialog();          
                
			}
            return true;
		}
		#endregion Public Methods

		#region Private Methods

		/// <summary>
		/// Transfers the collected data into the new database
		/// </summary>
		/// <param name="view">A view object</param>
		private void CopyCollectedData(View view)
		{
            //System.Console.WriteLine("Started Copy Data {0}", System.DateTime.Now);
            Epi.Data.Services.CollectedDataProvider collectedDataProvider = destinationProject.CollectedData;
            
            Epi.Data.IDbDriver db = collectedDataProvider.GetDatabase();
            System.Collections.Generic.List<System.Threading.Thread> ThreadList = new System.Collections.Generic.List<System.Threading.Thread>();
            string ConnectionString = db.ConnectionString;

            try
            {
                RaiseEventImportStatus(view.TableName + StringLiterals.ELLIPSIS);

                if (collectedDataProvider.TableExists(view.TableName))
                    collectedDataProvider.DeleteTable(view.TableName);

                foreach (Page page in view.Pages)
                {
                    if (collectedDataProvider.TableExists(page.TableName))
                        collectedDataProvider.DeleteTable(page.TableName);
                }

                if (!collectedDataProvider.TableExists(view.TableName))
                    collectedDataProvider.CreateDataTableForView(view, 1);

                if (sourceProject.CollectedData.TableExists(view.TableName))
                {
                    // If the view has related views, we need to copy the old UniqueKey values
                    // so we can do a proper relationship later on. These will get stored in OldUniqueKey.                    
                    string viewName = view.Name;
                    if (view.Name.StartsWith("view") == false)
                    {
                        viewName = "view" + viewName;
                    }
                    //List<string> relatedViews = sourceProject.Metadata.GetRelatedViewNames(viewName);
                    //if (relatedViews.Count > 0)
                    //{
                        //hasRelatedViews = true;                
        
                        // TODO: Find some way of changing "NUMERIC" to db-agnostic value; using this
                        // simple method now just to get this working
                        //Query alterQuery = db.CreateQuery(
                        //    "alter table " + view.TableName + " add OldUniqueKey " + "NUMERIC");
                        TableColumn column = new TableColumn("OldUniqueKey", GenericDbColumnType.Int32, true);
                        db.AddColumn(view.TableName, column);

                    foreach(Page page in view.Pages) 
                    {
                        db.AddColumn(page.TableName, column);
                    }

                    //}

                    if (view.IsRelatedView)
                    {
                        //Query alterQuery = db.CreateQuery(
                        //    "alter table " + view.TableName + " add OldFKEY " + "NUMERIC");
                        //db.ExecuteNonQuery(alterQuery);

                        column = new TableColumn("OldFKEY", GenericDbColumnType.Int32, true);
                        db.AddColumn(view.TableName, column);

                        // Create FKEY in MetaFields
                        ForeignKeyField foreignKeyField;
                        try
                        {
                            foreignKeyField = (ForeignKeyField)view.Fields["FKEY"];
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

                    int recordCount = 0;

                    //////////////////////////////////////////////////
                    if (!string.IsNullOrEmpty(view.TableName))
                    {
                        IDataReader reader = sourceProject.CollectedData.GetTableDataReader(view.TableName);
                        StringBuilder InsertSQL = new StringBuilder();
                        InsertSQL.Append("Insert Into [");
                        InsertSQL.Append(view.TableName);
                        InsertSQL.Append("] (");

                        view.MustRefreshFieldCollection = true;
                        foreach (Field field in view.Fields.TableColumnFields)
                        {
                            if (field is UniqueKeyField)
                            {
                                InsertSQL.Append("[OldUniqueKey],");
                                break;
                            }
                        }

                        if (view.IsRelatedView)
                        {
                            foreach (Field field in view.Fields.TableColumnFields)
                            {
                                if (field is ForeignKeyField)
                                {
                                    InsertSQL.Append("[OldFKEY],");
                                    break;
                                }
                            }
                        }

                        InsertSQL.Append("[GlobalRecordId]");
                        //InsertSQL.Remove(InsertSQL.Length - 1, 1);
                        InsertSQL.Append(")\n");

                        StringBuilder fieldValues = new StringBuilder();
                        Epi.Data.Query query = db.CreateQuery("");

                        while (reader.Read())
                        {
                            recordCount++;

                            if (recordCount % 1000 == 0)
                            {
                                Epi.Data.DBReadExecute.ExecuteSQL(ConnectionString, InsertSQL.ToString() + query.GetInsertValue(fieldValues.ToString()), 0);
                                fieldValues.Length = 0;
                                RaiseEventImportStatus(string.Format(SharedStrings.IMPORT_PROCESSED_RECORDS_BASE_TABLE, recordCount, view.Name)); // TODO: Make sure to modify this so that it doesn't appear in the log.
                                RaiseEventProgressBarStep();
                            }
                            
                            foreach (Field field in view.Fields.TableColumnFields)
                            {
                                if (field is UniqueKeyField)
                                {
                                    Object fieldValue = reader[field.Name];
                                    string fieldValueString = string.Empty;
                                    fieldValueString = fieldValue.ToString();
                                    fieldValues.Append(fieldValueString);
                                    fieldValues.Append(",");
                                    break;
                                }
                            }
                            foreach (Field field in view.Fields.TableColumnFields)
                            {
                                if (field.Name.ToLower().Equals("fkey"))
                                {
                                    Object fieldValue = reader[field.Name];
                                    string fieldValueString = string.Empty;
                                    fieldValueString = fieldValue.ToString();
                                    fieldValues.Append(fieldValueString);
                                    fieldValues.Append(",");
                                    break;
                                }
                            }
                            foreach (Field field in view.Fields.TableColumnFields)
                            {
                                if (field is GlobalRecordIdField)
                                {
                                    string fieldValueString = Util.InsertInSingleQuotes(System.Guid.NewGuid().ToString());
                                    fieldValues.Append(fieldValueString);
                                    fieldValues.Append(",");
                                    break;
                                }
                            }

                            fieldValues.Remove(fieldValues.Length - 1, 1);
                            fieldValues.Append(";");
                        }

                        // execute the remaining statments
                        if (string.IsNullOrEmpty(fieldValues.ToString()) == false)
                        {
                            Epi.Data.DBReadExecute.ExecuteSQL(ConnectionString, InsertSQL.ToString() + query.GetInsertValue(fieldValues.ToString()), 0);
                        }
                    }
                    //////////////////////////////////////////////////

                    recordCount = 0;
                    Epi.Epi2000.View sourceView = null;

                    foreach (Epi.Epi2000.View epi2000View in sourceProject.Views)
                    {
                        if (epi2000View.NameWithoutPrefix.ToLower().Equals(view.Name.ToLower()))
                        {
                            sourceView = epi2000View;
                            break;
                        }
                    }

                    foreach (Page page in view.Pages)
                    {
                        recordCount = 0;
                        if (sourceView == null)
                        {
                            Logger.Log(DateTime.Now + ":  " + SharedStrings.IMPORT_ERROR_SOURCE_VIEW_NULL);
                            throw new ApplicationException(SharedStrings.IMPORT_ERROR_SOURCE_VIEW_NULL);
                        }

                        IDataReader reader = sourceProject.CollectedData.GetTableDataReader(view.TableName);
                        
                        string dataTableNames = string.Empty;
                        string joinClause = "WHERE ";
                        string fromClause = string.Empty;                        
                        
                        if (sourceView.IsWideTableView)
                        {
                            foreach (string s in sourceView.TableNames)
                            {
                                dataTableNames = dataTableNames + s + ",";
                                joinClause = joinClause + sourceView.TableNames[0] + ".UniqueKey = " + s + ".UniqueKey AND ";
                            }
                            dataTableNames = dataTableNames.TrimEnd(',');
                            joinClause = joinClause.Remove(joinClause.Length - 4, 4);
                            fromClause = " FROM " + dataTableNames + " " + joinClause;

                            string fieldNames = sourceView.TableNames[0] + ".UniqueKey AS UniqueKey,";

                            foreach (Field field in page.Fields)
                            {
                                if (field is IDataField && !(field is GlobalRecordIdField) )
                                {
                                    fieldNames = fieldNames + "[" + field.Name + "],";
                                }
                            }
                            fieldNames = fieldNames.TrimEnd(',');

                            string selectCommand = "SELECT " + fieldNames + " " + fromClause;
                            Query selectQuery = db.CreateQuery(selectCommand);
                            reader = sourceProject.CollectedData.GetDatabase().ExecuteReader(selectQuery);
                        }
                        
                        StringBuilder InsertSQL = new StringBuilder();
                        InsertSQL.Append("Insert Into [");
                        InsertSQL.Append(/*view.TableName*/ page.TableName);
                        InsertSQL.Append("] (");

                        // Insert Into [TableName] ( [f1], ... [fn])
                        view.MustRefreshFieldCollection = true;

                        InsertSQL.Append("[OldUniqueKey],[GlobalRecordId],");

                        foreach (Field field in page.Fields /*view.Fields.TableColumnFields*/)
                        {
                            // Eliminate UniqueKeyFields. They are not inserted explicitly.
                            if (!(field is UniqueKeyField))
                            {
                                if (field is InputFieldWithoutSeparatePrompt || field is InputFieldWithSeparatePrompt)
                                {
                                    InsertSQL.Append("[");
                                    InsertSQL.Append(field.Name);
                                    InsertSQL.Append("],");
                                }
                            }
                            //else if (/*hasRelatedViews &&*/ field is UniqueKeyField)
                            //{
                            //    InsertSQL.Append("[OldUniqueKey],");
                            //}
                        }

                        InsertSQL.Remove(InsertSQL.Length - 1, 1);
                        InsertSQL.Append(")\n");

                        StringBuilder fieldValues = new StringBuilder();
                        Epi.Data.Query query = db.CreateQuery("");
                        
                        while (reader.Read())
                        {
                            recordCount++;

                            if (recordCount % 1000 == 0)
                            {
                                Epi.Data.DBReadExecute.ExecuteSQL(ConnectionString, InsertSQL.ToString() + query.GetInsertValue(fieldValues.ToString()), 0);
                                fieldValues.Length = 0;
                                RaiseEventImportStatus(string.Format("Processed {0:n0} records on page {1}", recordCount, page.Name)); // TODO: Make sure to modify this so that it doesn't appear in the log.
                                RaiseEventProgressBarStep();
                            }

                            fieldValues = fieldValues.Append(reader["UniqueKey"].ToString());
                            fieldValues.Append(",");
                            //fieldValues = fieldValues.Append(Util.InsertInSingleQuotes(System.Guid.NewGuid().ToString()));
                            fieldValues = fieldValues.Append(Util.InsertInSingleQuotes("temp" + recordCount.ToString()));
                            fieldValues.Append(",");

                            foreach (Field field in /*view.Fields.TableColumnFields*/page.Fields)
                            {
                                // Eliminate UniqueKeyFields. They are not inserted explicitly.
                                if (!(field is UniqueKeyField) && !(field is GlobalRecordIdField) && (field is InputFieldWithoutSeparatePrompt || field is InputFieldWithSeparatePrompt))
                                {
                                    if (!Util.IsEmpty(reader[field.Name]))
                                    {
                                        Object fieldValue = reader[field.Name];
                                        string fieldValueString = string.Empty;

                                        if (field is DateField)
                                        {
                                            fieldValueString = db.FormatDate((DateTime)fieldValue);
                                        }
                                        else if (field is ImageField)
                                        {
                                            //fieldValueString = db.FormatTime((DateTime)fieldValue);
                                        }
                                        else if (field is TimeField)
                                        {
                                            fieldValueString = db.FormatTime((DateTime)fieldValue);
                                        }
                                        else if (field is DateTimeField)
                                        {
                                            fieldValueString = db.FormatDateTime((DateTime)fieldValue);
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
                                        else if (field is OptionField)
                                        {
                                            try
                                            {
                                                string viewNameWithPrefix = "view" + view.Name;
                                                string tableName = view.TableName;

                                                DataTable fieldsTable = sourceProject.Metadata.GetFieldsAsDataTable(viewNameWithPrefix);

                                                foreach (DataRow fieldRow in fieldsTable.Rows)
                                                {
                                                    string fieldName = fieldRow["Name"].ToString();
                                                    if (fieldName == field.Name)
                                                    {
                                                        //if(db.ColumnExists(tableName, fieldName) == false) { break; }
                                                        string[] items = fieldRow["Lists"].ToString().Split(';');
                                                        for (int i = 0; i < items.Length; i++)
                                                        {
                                                            if (items[i] == fieldValue.ToString())
                                                            {
                                                                fieldValueString = i.ToString();
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            catch
                                            {
                                            }
                                        }
                                        else
                                        {
                                            fieldValueString = fieldValue.ToString();
                                        }

                                        fieldValues.Append(fieldValueString);
                                        fieldValues.Append(",");
                                    }
                                    else
                                    {
                                        fieldValues.Append("null,");
                                    }
                                }
                                else if (field is GlobalRecordIdField)
                                {
                                    string fieldValueString = Util.InsertInSingleQuotes(System.Guid.NewGuid().ToString());
                                    if (view.IsRelatedView == false)
                                    {
                                        fieldValues.Append(fieldValueString);
                                    }
                                    else
                                    {
                                        fieldValues.Append("''");
                                    }
                                    fieldValues.Append(",");
                                }
                                //else if (field is UniqueKeyField/* && hasRelatedViews*/)
                                //{
                                //    //if (!Util.IsEmpty(reader[field.Name]))
                                //    //{
                                //    Object fieldValue = reader[field.Name];
                                //    string fieldValueString = string.Empty;
                                //    fieldValueString = fieldValue.ToString();
                                //    fieldValues.Append(fieldValueString);
                                //    fieldValues.Append(",");
                                //    //}
                                //}
                            }

                            fieldValues.Remove(fieldValues.Length - 1, 1);
                            fieldValues.Append(";");
                        }

                        // execute the remaining statments
                        if (string.IsNullOrEmpty(fieldValues.ToString()) == false)
                        {
                            Epi.Data.DBReadExecute.ExecuteSQL(ConnectionString, InsertSQL.ToString() + query.GetInsertValue(fieldValues.ToString()), 0);
                        }
                    }

                    UpdatePageTableGUIDs(view);

                    if (view.IsRelatedView)
                    {
                        //Query updateQuery = db.CreateQuery("update " + view.TableName + " set OldFKEY = FKEY");
                        //db.ExecuteNonQuery(updateQuery);
                    }

                    RaiseEventImportStatus(string.Format(SharedStrings.IMPORT_COMPLETE, recordCount));
                    //System.Console.WriteLine("Finished Copy Data {0}", System.DateTime.Now);
                }
            }
            catch (SqlException se)
            {
                if (se.Number == 296)
                {
                    throw new GeneralException(string.Format(SharedStrings.IMPORT_ERROR_BAD_DATE_DATA, view.Name));
                }
            }
			finally
			{
				
			}			
		}

        /// <summary>
        /// Updates the page table GUIDs with the GUID in the corresponding base table
        /// </summary>
        /// <param name="view">The view whose pages are to be processed</param>
        private void UpdatePageTableGUIDs(View view)
        {
            #region Input Validation
            #endregion // Input Validation

            Epi.Data.Services.CollectedDataProvider collectedDataProvider = destinationProject.CollectedData;
            Epi.Data.IDbDriver db = collectedDataProvider.GetDatabase();

            foreach (Page page in view.Pages)
            {
                Query updateGUIDQuery = db.CreateQuery(
                    "update " + page.TableName + " set GlobalRecordId = " + view.TableName + ".GlobalRecordId from " + page.TableName + " inner join " + view.TableName + " on " + view.TableName + ".OldUniqueKey=" + page.TableName + ".OldUniqueKey"
                    );
                db.ExecuteNonQuery(updateGUIDQuery);
            }
        }

        /// <summary>
        /// Returns the number of data entry fields in an Epi 3.5.x view. Data entry fields are defined as
        /// those that create a corresponding column in the view's data table.
        /// </summary>
        /// <param name="sourceView">The Epi 3.5.x view to process</param>
        /// <returns>The number of data entry fields in sourceView</returns>
        private int GetDataFieldsInSourceView(Epi.Epi2000.View sourceView)
        {
            int fieldCount = 0;
            DataTable fieldsTable = sourceProject.Metadata.GetFieldsAsDataTable(sourceView.Name);

            foreach (DataRow fieldRow in fieldsTable.Rows)
            {
                MetaFieldType fieldType = Epi.Epi2000.MetadataDbProvider.InferFieldType(fieldRow);
                if (!(fieldType == MetaFieldType.Relate || fieldType == MetaFieldType.Mirror || fieldType == MetaFieldType.LabelTitle || fieldType == MetaFieldType.Group || fieldType == MetaFieldType.Grid || fieldType == MetaFieldType.CommandButton || fieldType == MetaFieldType.UniqueKey))
                {
                    fieldCount++;
                }
            }

            return fieldCount;
        }

        /// <summary>
		/// Determines whether or not an Epi2000 View should be processed
		/// </summary>		
        private bool ShouldProcessView(Epi.Epi2000.View sourceView, out string message)
        {
            message = string.Empty;
            int fieldCount = GetDataFieldsInSourceView(sourceView);
            if (string.IsNullOrEmpty(sourceView.TableName) == true && fieldCount > 250 &&
                destinationProject.CollectedDataDriver == "Epi.Data.Office.AccessDBFactory, Epi.Data.Office")
            {
                //message = string.Format("The view [{0}] has been skipped for processing during the import as it contains {1} fields; the field limit for imported Views with no data table where 'Local Files' is selected is 250.", sourceView.Name, fieldCount.ToString());
                //return false;
                return true;
            }
            else if (string.IsNullOrEmpty(sourceView.TableName) == true && fieldCount > 999 &&
                destinationProject.CollectedDataDriver != "Epi.Data.Office.AccessDBFactory, Epi.Data.Office")
            {
                message = string.Format(SharedStrings.IMPORT_NOTIFICATION_VIEW_SKIPPED_FIELD_LIMIT, sourceView.Name, fieldCount.ToString());
                return false;
            }
            else if (string.IsNullOrEmpty(sourceView.TableName) == false && fieldCount > 200)
            {
                //message = string.Format("The view [{0}] has been skipped for processing during the import as it contains {1} fields; the field limit for imported Views with an associated data table is 200.", sourceView.Name, fieldCount.ToString());
                //return false;
                return true;
            }

            if (sourceView.Pages.Count <= 0)
            {
                message = string.Format(SharedStrings.IMPORT_NOTIFICATION_VIEW_SKIPPED_NO_PAGES, sourceView.Name);
                return false;
            }

            return true;
        }

		/// <summary>
		/// Converts the views in a project
		/// </summary>		
		private void CopyViews()
		{
			try
			{
                Epi.Data.Services.CollectedDataProvider collectedDataProvider = destinationProject.CollectedData;
                Epi.Data.IDbDriver db = collectedDataProvider.GetDatabase();

                if (destinationProject.CollectedDataDriver == "Epi.Data.Office.AccessDBFactory, Epi.Data.Office")
                {
                    // For Access projects, this is how we take an inventory of all the major
                    // artifacts to be imported - this then allows us to update the progress
                    // bar quite accurately.

                    // Because of the way the Access importer works, converting the fields 
                    // from Epi 3 metadata format to the Epi 7 metadata format take up a large
                    // chunk of the processing time. Thus, each field is one artifact. We also
                    // want each view to be an artifact, since those each take up about the same
                    // amount of time as a single field to update in metaViews. Related views
                    // get counted twice because they are processed twice - once with all non-
                    // Related views, but then again in CopyViewRelations().
                    //
                    // Each time a field has been imported or a view has been procesed, we run
                    // the ProgressBarStep event which increments the bar by one step. The sum
                    // total of 'artifacts' is the total number of steps for the progress bar,
                    // which is set when we run the RaiseEventImportStarted() event with that
                    // sum as a parameter.

                    int artifacts = 0;

                    foreach (Epi.Epi2000.View sourceView in sourceProject.Views)
                    {
                        string message;
                        if (ShouldProcessView(sourceView, out message) == false)
                        {
                            // If we are going to skip this view, make sure we don't add it to the artifact count.
                            continue;
                        }

                        if (sourceView.IsRelatedView)
                        {
                            artifacts = artifacts + 1; // double count related views since they get processed again in the 'View Relations' section.
                        }

                        artifacts = artifacts + sourceProject.Metadata.GetFieldsAsDataTable(sourceView.Name).Rows.Count;
                        if (string.IsNullOrEmpty(sourceView.TableName) == false && db.TableExists(sourceView.TableName))
                        {   
                            artifacts = artifacts + sourceView.GetRecordCount();                            
                        }
                    }
                    artifacts = artifacts + sourceProject.Views.Count;
                    RaiseEventImportStarted(artifacts);
                }

                foreach (Epi.Epi2000.View sourceView in sourceProject.Views)
				{
                    // Skip processing views that have 'wide data tables' (>200 fields) or that don't
                    // have data tables at all, but would still cause a wide table to occur in Epi 7
                    // should they be imported (>250 fields).

                    // NOTE: We've already warned the user that these views will be skipped.
                    string message;
                    if (ShouldProcessView(sourceView, out message) == false)
                    {
                        continue;
                    }

                    RaiseEventImportStatus(sourceView.NameWithPrefix + StringLiterals.ELLIPSIS);

                    

					// Create and save the view
					destinationProject.CreateView(sourceView.NameWithoutPrefix);
                    View destinationView = destinationProject.GetViewByName(sourceView.NameWithoutPrefix);

                    //  Get View, Record and Page Checkcode from the CheckCodeBefore block of the SourceView. 
                    
                    sourceView.CopyTo(destinationView);
                    // remove the checkcode blocks stored in the CheckCodeBefore field during the sourceView.CopyTo method.
                    
                    destinationView.PageOrientation = "Landscape";
                    destinationView.PageLabelAlign = "Horizontal";
                    destinationView.PageHeight = 993;
                    destinationView.PageWidth = 768;
                    destinationView.SaveToDb();
                    destinationView.SetTableName(sourceView.TableName);

                    if(string.IsNullOrEmpty(destinationView.TableName) == false && db.TableExists(destinationView.TableName))
                    {
                        TableColumn oldUniqueKeyColumn = new TableColumn("OldUniqueKey", GenericDbColumnType.Int32, true);

                        if (db.ColumnExists(destinationView.TableName, oldUniqueKeyColumn.Name))
                        {
                            db.DeleteColumn(destinationView.TableName, oldUniqueKeyColumn.Name);
                        }

                        db.AddColumn(destinationView.TableName, oldUniqueKeyColumn);

                        if (destinationView.IsRelatedView == true)
                        {
                            TableColumn oldFkeyColumn = new TableColumn("OldFKEY", GenericDbColumnType.Int32, true);

                            if (db.ColumnExists(destinationView.TableName, oldFkeyColumn.Name))
                            {
                                db.DeleteColumn(destinationView.TableName, oldFkeyColumn.Name);
                            }

                            db.AddColumn(destinationView.TableName, oldFkeyColumn);
                        }
                    }

                    /////////////////////////////////
                    /////////////////////////////////
                    /////////////////////////////////
                }

                foreach (Epi.Epi2000.View sourceView in sourceProject.Views)
                {   
                    string message;
                    if (ShouldProcessView(sourceView, out message) == false)
                    {
                        continue;
                    }

                    RaiseEventImportStatus(sourceView.NameWithPrefix + StringLiterals.ELLIPSIS);
                    
                    View destinationView = destinationProject.GetViewByName(sourceView.NameWithoutPrefix);
                    CheckCode.Length = 0;
                    CheckCode.Append(sourceView.CheckCodeBefore);
                    destinationView.CheckCodeBefore = "";

                    /////////////////////////////////
                    /////////////////////////////////
                    /////////////////////////////////

					// Copy pages
					CopyPages(sourceView, destinationView);
                    
                    if (destinationProject.CollectedDataDriver != "Epi.Data.Office.AccessDBFactory, Epi.Data.Office" && string.IsNullOrEmpty(sourceView.TableName) == false)
                    {
                        CopyCollectedData(destinationView);

                        // Update the metaFields table with the data table name the fields belong to
                        Query updateQuery = db.CreateQuery("UPDATE metaFields SET DataTableName = @DataTableName WHERE ViewId = @Id");
                        updateQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, destinationView.TableName));
                        updateQuery.Parameters.Add(new QueryParameter("@Id", DbType.Int32, destinationView.Id));
                        db.ExecuteNonQuery(updateQuery);
                    }
                    else if (collectedDataProvider.TableExists(destinationView.TableName) && destinationProject.CollectedDataDriver == "Epi.Data.Office.AccessDBFactory, Epi.Data.Office")
                    {
                        int rowCount = 0;
                        if (string.IsNullOrEmpty(destinationView.TableName) == false && db.TableExists(destinationView.TableName))
                        {
                            Query countQuery = db.CreateQuery("SELECT Count(*) FROM " + destinationView.TableName);
                            rowCount = ((int)db.ExecuteScalar(countQuery));

                            Query updateQuery = db.CreateQuery("update " + destinationView.TableName + " set OldUniqueKey = UniqueKey");
                            db.ExecuteNonQuery(updateQuery);

                            if (destinationView.IsRelatedView)
                            {
                                updateQuery = db.CreateQuery("update " + destinationView.TableName + " set OldFKEY = FKEY");
                                db.ExecuteNonQuery(updateQuery);
                            }
                        }

                        // Check table column for all fields in case view name and data table name do not match
                        if (destinationView.Name.EndsWith(destinationView.TableName) == false
                            && string.IsNullOrEmpty(destinationView.TableName) == false
                            && rowCount > 0)
                        {
                            Query updateQuery = db.CreateQuery("UPDATE metaFields SET DataTableName = @DataTableName WHERE ViewId = @Id");
                            updateQuery.Parameters.Add(new QueryParameter("@DataTableName", DbType.String, destinationView.TableName));
                            updateQuery.Parameters.Add(new QueryParameter("@Id", DbType.Int32, destinationView.Id));
                            db.ExecuteNonQuery(updateQuery);

                            TableColumn guidColumn = new TableColumn("GlobalRecordId", GenericDbColumnType.String, 255, true);
                            db.AddColumn(destinationView.TableName, guidColumn);
                        }
                        else if (rowCount == 0)
                        {
                            db.DeleteTable(destinationView.TableName);
                            destinationView.TableName = string.Empty;                            
                        }
                        else
                        {
                            TableColumn guidColumn = new TableColumn("GlobalRecordId", GenericDbColumnType.String, 255, true);
                            db.AddColumn(destinationView.TableName, guidColumn);
                        }

                        if (!string.IsNullOrEmpty(destinationView.TableName) && db.TableExists(destinationView.TableName) && rowCount > 0)
                        {
                            int recordCount = rowCount;

                            Query minQuery = db.CreateQuery("SELECT Min(UniqueKey) FROM " + destinationView.TableName);
                            Query maxQuery = db.CreateQuery("SELECT Max(UniqueKey) FROM " + destinationView.TableName);

                            int currentRecord = ((int)db.ExecuteScalar(minQuery)); //destinationView.GetFirstRecordId();
                            int lastRecord = ((int)db.ExecuteScalar(maxQuery)); //destinationView.GetLastRecordId();
                            
                            int recordsProcessed = 0;
                            RaiseEventImportStatus(string.Format(SharedStrings.IMPORT_ADD_GUIDS, destinationView.Name));
                            for (int i = currentRecord; i <= lastRecord; i++)
                            {
                                Query updateQuery = db.CreateQuery("UPDATE " + destinationView.TableName + " SET GlobalRecordId = " + Util.InsertInSingleQuotes(System.Guid.NewGuid().ToString())
                                    + " WHERE UniqueKey = " + i.ToString());

                                int recordsAffected = db.ExecuteNonQuery(updateQuery);

                                if (recordsAffected > 0)
                                {
                                    recordsProcessed++;
                                    RaiseEventImportStatus(string.Format(SharedStrings.IMPORT_PROCESSED_GUIDS, recordsProcessed.ToString(), recordCount, destinationView.Name), new MessageEventArgs(string.Format(SharedStrings.IMPORT_PROCESSED_GUIDS, recordsProcessed.ToString(), recordCount, destinationView.Name), false));
                                    RaiseEventProgressBarStep();
                                }
                                else if (recordsAffected <= 0 && i > 0)
                                {
                                    //i = destinationView.GetNextRecordId(i) - 1; // can't use this because ForeignKeyField is null at this point and won't be set until later
                                    Query nextQuery = db.CreateQuery("SELECT Min(UniqueKey) FROM [" + destinationView.TableName + "] WHERE UniqueKey > " + i.ToString());
                                    int nextRecord = ((int)db.ExecuteScalar(nextQuery)) - 1; 
                                }
                            }

                            if (recordCount > 0)
                            {
                                RaiseEventImportStatus(string.Format(SharedStrings.IMPORT_PROCESSED_ALL_GUIDS, recordCount.ToString(), destinationView.Name));
                            }

                            Query deleteQuery = db.CreateQuery("DELETE * FROM [" + destinationView.TableName + "] WHERE RecStatus < 1 OR RecStatus is null");
                            db.ExecuteNonQuery(deleteQuery);

                            Query dropColumnQuery = db.CreateQuery("ALTER TABLE [" + destinationView.TableName + "] DROP COLUMN RecStatus");
                            db.ExecuteNonQuery(dropColumnQuery);

                            Query addColumnQuery = db.CreateQuery("ALTER TABLE [" + destinationView.TableName + "] ADD COLUMN RecStatus SHORT DEFAULT 1");
                            db.ExecuteNonQuery(addColumnQuery);

                            Query updateRecstatusQuery = db.CreateQuery("UPDATE [" + destinationView.TableName + "] SET RecStatus = 1");
                            db.ExecuteNonQuery(updateRecstatusQuery);
                        }

                        if (rowCount > 0)
                        {
                            string dataTableNames = string.Empty;
                            string joinClause = "WHERE ";
                            foreach (string s in sourceView.TableNames)
                            {
                                dataTableNames = dataTableNames + s + ",";
                                joinClause = joinClause + sourceView.TableNames[0] + ".UniqueKey = " + s + ".UniqueKey AND ";
                            }

                            dataTableNames = dataTableNames.TrimEnd(',');
                            joinClause = joinClause.Remove(joinClause.Length - 4, 4);                            

                            string fromClause = " FROM " + dataTableNames + " " + joinClause;

                            foreach (Page page in destinationView.Pages)
                            {
                                if (sourceView.IsWideTableView)
                                {
                                    string fields = string.Empty;
                                    fields = "[GlobalRecordId],";
                                    int fieldCount = 1;

                                    if (sourceView.IsRelatedView)
                                    {
                                        fields = fields + "[FKEY],";
                                        fieldCount++;
                                    }

                                    foreach (RenderableField field in page.Fields)
                                    {
                                        if (field is InputFieldWithoutSeparatePrompt || field is InputFieldWithSeparatePrompt)
                                        {
                                            fieldCount++;
                                            fields = fields + "[" + field.Name + "],";
                                        }
                                    }

                                    fields = fields.TrimEnd(',');

                                    string selectCommand = "SELECT " + fields + " " + fromClause;
                                    string fullSelectCommand = "SELECT " + fields + " INTO [" + page.TableName + "] from (" + selectCommand + ")";
                                    Query createQuery = db.CreateQuery(fullSelectCommand);
                                    db.ExecuteNonQuery(createQuery);
                                }
                                else
                                {
                                    Query createQuery = db.CreateQuery("SELECT * INTO [" + page.TableName + "] from [" + destinationView.TableName + "] WHERE RecStatus = 1");
                                    db.ExecuteNonQuery(createQuery);

                                    // TODO: See if this can be implemented better
                                    if (db.ColumnExists(page.TableName, "RecStatus"))
                                    {
                                        db.DeleteColumn(page.TableName, "RecStatus");
                                    }
                                }
                            }
                        }

                        DeleteUnusedTableColumns(destinationView);
                    }

                    string fullCheckCode = CheckCode.ToString();
                    fullCheckCode = fullCheckCode.Replace("\t*", "\t//");

                    destinationView.CheckCode = "/* >>>Notice About Imported Check Code<<<\n" +
                                                "\tThis Check Code was imported from an Epi Info 3.x project.\n" +
                                                "\tReview and validate the new Check Code for proper Epi Info 7.x syntax.\n" +
                                                "\tSome commands and features are not yet available in this Epi Info 7 release.\n\n" +
                                                "\tClick 'Validate Check Code' at the top to identify and correct any errors.\n" +
                                                "\tIf you experience errors or need assistance,\n" +
                                                "\tcontact the Epi Info Help Desk at EpiInfo@cdc.gov.\n" +
                                                "*/\n" + fullCheckCode.ToString();
                    UpdateImageFields(destinationView);
                    destinationView.SaveToDb();
                    RaiseEventProgressBarStep();
                }

                string status = SharedStrings.IMPORT_NOTIFICATION_CONVERTING_VIEW_RELATIONS + StringLiterals.ELLIPSIS;                
                RaiseEventImportStatus(status, new MessageEventArgs(status, false));

                CopyViewRelations();

                UpdateGUIDs();
                
                RaiseEventImportEnded();                

                foreach (View view in destinationProject.Views)
                {
                    if (!string.IsNullOrEmpty(view.TableName) && db.TableExists(view.TableName) && !db.ColumnExists(view.TableName, "FKEY"))
                    {
                        //Query addFKEYColumnQuery = db.CreateQuery("ALTER TABLE [" + view.TableName + "] ADD COLUMN FKEY TEXT");
                        Query addFKEYColumnQuery = db.CreateQuery("ALTER TABLE [" + view.TableName + "] ADD COLUMN FKEY VARCHAR(255)");                        
                        db.ExecuteNonQuery(addFKEYColumnQuery);
                    }
                }
			}
			finally
			{
			}
		}

        /// <summary>
        /// Checks for unused columns in the data table and deletes them.
        /// </summary>
        /// <param name="destinationView">The view whose data table(s) will be checked for unused columns</param>
        private void DeleteUnusedTableColumns(View destinationView)
        {   
            // TODO: At some point, MakeView/Enter may be able to handle having excess and unused columns come over through an import - if so, 
            // revisit whether this method is needed. Perhaps it could be kept, but only log the discrepancy for the benefit of the user (rather 
            // than going through a deletion).

            Epi.Data.Services.CollectedDataProvider collectedDataProvider = destinationProject.CollectedData;
            Epi.Data.IDbDriver db = collectedDataProvider.GetDatabase();

            if (string.IsNullOrEmpty(destinationView.TableName) || collectedDataProvider.TableExists(destinationView.TableName) == false)
            {
                return;
            }

            // TODO: Check for more efficient way to do this
            Query selectQuery = db.CreateQuery("SELECT * FROM " + destinationView.TableName);            
            IDataReader reader = db.ExecuteReader(selectQuery);

            if (reader.Read())
            {
                List<string> tableColumnNames = new List<string>();

                // Iterate over all the column names in the table
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    // skip internal columns like 'uniquekey', but add the rest
                    if (!(reader.GetName(i).ToLower() == "uniquekey" || reader.GetName(i).ToLower() == "globalrecordid" || reader.GetName(i).ToLower() == "fkey" || reader.GetName(i).ToLower() == "recstatus" || reader.GetName(i).ToLower() == "oldfkey" || reader.GetName(i).ToLower() == "olduniquekey"))
                    {
                        tableColumnNames.Add(reader.GetName(i));
                    }
                }

                reader.Close();

                // for each column in the table, see if there's a matching field in metaFields
                foreach (string s in tableColumnNames)
                {
                    if (!(s.ToLower().Equals("globalrecordid") || s.ToLower().Equals("fkey") || s.ToLower().Equals("uniquekey") || s.ToLower().Equals("recstatus") || s.ToLower().Equals("oldfkey") || s.ToLower().Equals("olduniquekey")))
                    {                        
                        try
                        {
                            db.DeleteColumn(destinationView.TableName, s);
                        }
                        catch (OleDbException ex)
                        {
                            Logger.Log(DateTime.Now + ":  " + string.Format(SharedStrings.IMPORT_WARNING_DELETE_COLUMN, s, ex.Message));
                        }
                    }
                }
            }

            reader.Close();
                
            foreach(Page page in destinationView.Pages) 
            {       
                if(db.TableExists(page.TableName)) 
                {
                    selectQuery = db.CreateQuery("SELECT * FROM " + page.TableName);
                    reader = db.ExecuteReader(selectQuery);

                    if (reader.Read())
                    {
                        List<string> tableColumnNames = new List<string>();

                        // Iterate over all the column names in the table
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            // skip internal columns like 'uniquekey', but add the rest
                            if (!(reader.GetName(i).ToLower() == "uniquekey" || reader.GetName(i).ToLower() == "globalrecordid" || reader.GetName(i).ToLower() == "fkey" || reader.GetName(i).ToLower() == "recstatus"))
                            {
                                tableColumnNames.Add(reader.GetName(i));
                            }
                        }

                        reader.Close();

                        // for each column in the table, see if there's a matching field in metaFields
                        foreach (string s in tableColumnNames)
                        {
                            bool found = false;
                            foreach (Field field in page.Fields)
                            {
                                // if there's a match, and the field that is matched is a data field, then set 'found' to true
                                if (field.Name.ToLower() == s.ToLower() &&
                                    !(field is CommandButtonField) &&
                                    !(field is MirrorField) &&
                                    !(field is LabelField) &&
                                    !(field is GridField) &&
                                    !(field is GroupField) &&
                                    !(field is UniqueKeyField) &&
                                    !(field is RelatedViewField)
                                    )
                                {
                                    found = true;
                                }
                            }
                            if (found == false)
                            {
                                try
                                {
                                    db.DeleteColumn(page.TableName, s);
                                    Logger.Log(DateTime.Now + ":  " + string.Format(SharedStrings.IMPORT_NOTIFICATION_COLUMN_REMOVED_NO_MATCH, s, page.TableName));
                                }
                                catch (OleDbException ex)
                                {
                                    Logger.Log(DateTime.Now + ":  " + string.Format(SharedStrings.IMPORT_NOTIFICATION_COLUMN_NOT_REMOVED_BUT_ATTEMPTED, s, page.TableName, ex.Message));
                                }
                            }
                        }
                    }
                    reader.Close();

                }

                reader.Close();
                reader.Dispose();
            }
        }

        /// <summary>
        /// Recursive function used to find the height this related view takes in its position relative
        /// to the topmost parent view.
        /// </summary>
        /// <param name="view">The view to find the height of</param>
        /// <param name="count">The initial count (set to 0)</param>
        /// <returns>The height of the view</returns>
        private int FindPositionInTree(Epi.Epi2000.View view, int count)
        {
            if (view.ParentView == null)
            {
                return count;
            }
            else
            {
                return FindPositionInTree(view.ParentView, count);
            }
        }

        /// <summary>
        /// Updates the GlobalRecordId fields in all child views based on the FKEY/UniqueKey combos
        /// </summary>
        private void UpdateGUIDs()
        {
            bool keysNotFilled = false;
            
            Dictionary<string, int> viewsToProcess = new Dictionary<string, int>();
            Dictionary<string, string> viewRelationships = new Dictionary<string, string>();
            int highestPosition = 0;

            Epi.Data.Services.CollectedDataProvider collectedDataProvider = destinationProject.CollectedData;
            Epi.Data.IDbDriver db = collectedDataProvider.GetDatabase();

            // Update ParentView property for each Epi2000.View in the list of views coming 
            // in from the source project. We'll need each 'View' object to have a Parent set
            // if it's a related View later on in the process.
            foreach (Epi.Epi2000.View iView in sourceProject.Views)
            {
                List<string> relatedViews = new List<string>();
                string viewName = iView.Name;

                DataTable relateTable = sourceProject.Metadata.GetViewRelations(iView.Name);
                foreach (DataRow row in relateTable.Rows)
                {                    
                    string relatedViewName = row[3].ToString();
                    relatedViewName = relatedViewName.Remove(0, 4);
                    relatedViews.Add(relatedViewName);                    
                }

                // Set parents
                foreach (Epi.Epi2000.View jView in sourceProject.Views)
                {
                    foreach (string s in relatedViews)
                    {
                        string currentViewName = jView.Name.Remove(0, 4);
                        if (currentViewName == s)
                        {
                            jView.ParentView = iView;
                        }
                    }
                    if (jView.IsRelatedView == true)
                    {
                        keysNotFilled = true;
                    }
                }                
            }

            // Find the position the view takes in the tree structure of related Views. We have to
            // process down from the top in order to make the key population routine work.
            foreach (Epi.Epi2000.View iView in sourceProject.Views)
            {
                try
                {
                    if (iView.IsRelatedView && string.IsNullOrEmpty(iView.TableName) == false && collectedDataProvider.TableExists(iView.TableName) && collectedDataProvider.TableExists(iView.ParentView.TableName))
                    {
                        int position = FindPositionInTree(iView, 0);
                        viewsToProcess.Add(iView.TableName, position);
                        viewRelationships.Add(iView.TableName, iView.ParentView.TableName);
                        if (position > highestPosition)
                        {
                            highestPosition = position;
                        }
                    }
                }
                catch (NullReferenceException)
                {
                    Logger.Log(DateTime.Now + ":  " + string.Format(SharedStrings.IMPORT_WARNING_RELATED_TABLE_NOT_PROCESSED, iView.Name));                    
                    continue;
                }
            }

            // Populate keys in order, from views just below the parent (root) to the bottom
            if (keysNotFilled == true)
            {
                for (int i = 0; i <= highestPosition; i++)
                {
                    foreach (string s in viewsToProcess.Keys)
                    {
                        if (viewsToProcess[s] == i)
                        {
                            string parentTable = viewRelationships[s];
                            string currentTable = s;

                            // The reason we're checking if the column exists is because apparently,
                            // it's possible in Epi 3 to have a related view with a data table that
                            // does not have an FKEY. This happens under rare circumstances but we
                            // still want to catch it and contain it.
                            if (db.ColumnExists(currentTable, "FKEY"))
                            {
                                if (destinationProject.CollectedDataDriver == "Epi.Data.Office.AccessDBFactory, Epi.Data.Office")
                                {
                                    db.DeleteColumn(currentTable, "FKEY");
                                    TableColumn newFkeyColumn = new TableColumn("FKEY", GenericDbColumnType.String, true);
                                    db.AddColumn(currentTable, newFkeyColumn);
                                }

                                RaiseEventImportStatus(string.Format(SharedStrings.IMPORT_NOTIFICATION_UPDATE_GUIDS, s));
                                db.UpdateGUIDs(currentTable, parentTable);
                                RaiseEventImportStatus(string.Format(SharedStrings.IMPORT_NOTIFICATION_UPDATE_KEYS, s));
                                db.UpdateKeys(currentTable, parentTable);
                            }
                            else
                            {
                                Logger.Log(DateTime.Now + ":  " + string.Format(SharedStrings.IMPORT_WARNING_RELATED_TABLE_HAS_NO_FKEY, currentTable, parentTable, currentTable));
                            }
                        }
                    }
                }
            }

            // Clean up temp columns from non-Access imports where related Views existed in the source project file
            if (destinationProject.CollectedDataDriver != "Epi.Data.Office.AccessDBFactory, Epi.Data.Office")
            {
                foreach (View jView in destinationProject.Views)
                {
                    if (jView.IsRelatedView == true && db.ColumnExists(jView.TableName, "OldFKEY"))
                    {
                        db.DeleteColumn(jView.TableName, "OldFKEY");
                    }
                    if (db.ColumnExists(jView.TableName, "OldUniqueKey"))
                    {
                        db.DeleteColumn(jView.TableName, "OldUniqueKey");
                    }
                }
            }
        }
   
        private void RaiseEventImportStarted(int count)
        {
            if (ImportStarted != null)
            {
                ImportStarted(this, new ImportStartedEventArgs(count));
            }
        }

        private void RaiseEventImportStatus(object sender, MessageEventArgs args)
        {
            if (ImportStatus != null)
                ImportStatus(sender, args);
        }

		private void RaiseEventImportStatus(string message)
		{
			if (ImportStatus != null)
				ImportStatus(this, new MessageEventArgs(message));
		}

        private void RaiseEventImportEnded()
        {
            if (ImportEnded != null)
            {
                ImportEnded();
            }
        }


        private void RaiseEventProgressBarStep()
        {
            if (ArtifactImported != null)
                ArtifactImported(this, new EventArgs());
        }

        private void ReportTableCopyEnd(object sender, MessageEventArgs args)
        {
            RaiseEventProgressBarStep();
        }

        private void ReportTableCopyStatus(object sender, TableCopyStatusEventArgs args)
        {
            string status = string.Empty;
            string totalRecords = string.Empty;

            if (args.TotalRecords > 0)
            {
                totalRecords = StringLiterals.SPACE + SharedStrings.OUT_OF + StringLiterals.SPACE + args.TotalRecords;
            }

            status = string.Format(StringLiterals.TABLE_IMPORT_STATUS, args.TableName, args.RecordCount, SharedStrings.RECORDS_COPIED, totalRecords);

            RaiseEventImportStatus(status);
        }

		/// <summary>
		/// Updates the sources of all mirror fields in a view
		/// </summary>
		/// <param name="view">An Epi7 view</param>
		private void UpdateMirrorFields(View view)
		{
            List<Field> mirrorFieldsToRemove = new List<Field>();

			foreach (Field field in view.Fields)
			{
				if (field is MirrorField)
				{
                    if (view.Fields.Exists(GetSourceFieldName((MirrorField)field)))
                    {
                        UpdateMirrorFieldSource((MirrorField)field);
                    }
                    else
                    {
                        mirrorFieldsToRemove.Add(field);
                        Logger.Log(DateTime.Now + ":  " + string.Format(SharedStrings.IMPORT_WARNING_MIRROR_FIELD_NO_SOURCE, field.Name, view.Name, GetSourceFieldName((MirrorField)field)));
                    }
				}
			}

            foreach (Field mirrorField in mirrorFieldsToRemove)
            {
                view.GetMetadata().DeleteField(mirrorField);
            }
		}

        private void UpdateImageFields(View destinationView)
        {
            string status = string.Format(SharedStrings.IMPORT_NOTIFICATION_IMAGE_UPDATE_START, destinationView.Name);
            MessageEventArgs args = new MessageEventArgs(status, false);
            RaiseEventImportStatus(status, args);

            FieldCollectionMaster fields = destinationView.GetMetadata().GetFields(destinationView);
            List<string> imageFieldNames = new List<string>();
            bool hasImages = false;

            // << change columns >>
            foreach (Field field in fields)
            {
                if (field.FieldType == MetaFieldType.Image)
                {
                    imageFieldNames.Add(field.Name);
                    hasImages = true;
                    string pageTableName = destinationView.TableName;

                    foreach (Page page in destinationView.Pages)
                    {
                        if (page.Fields.Contains(field.Name))
                        {
                            pageTableName = page.TableName;
                        }
                    }

                    if (destinationProject.CollectedData.ColumnExists(pageTableName, field.Name))
                    {
                        destinationProject.CollectedData.DeleteColumn(pageTableName, field.Name);
                        destinationProject.CollectedData.CreateTableColumn((Epi.Fields.IInputField)field, pageTableName);
                    }
                }
            }

            // << convert to bytes - per record - per field >>
            Epi.Epi2000.CollectedDataProvider collectedDataProvider = sourceProject.CollectedData;
            Epi.Data.IDbDriver sourceData = collectedDataProvider.GetDatabase();

            if (string.IsNullOrEmpty(destinationView.TableName) || collectedDataProvider.TableExists(destinationView.TableName) == false || hasImages == false)
            {
                status = string.Format(SharedStrings.IMPORT_NOTIFICATION_IMAGE_UPDATE_NONE, destinationView.Name);
                RaiseEventImportStatus(status);
                return;
            }

            Query selectQuery = sourceData.CreateQuery("SELECT * FROM " + destinationView.TableName);
            DataTable sourceTable = sourceData.GetTableData(destinationView.TableName);
            string imagePath = string.Empty;
            int imageFieldsProcessed = 0;

            foreach (DataRow sourceRow in sourceTable.Rows)
            {
                int uniqueKey = ((int)sourceRow["UniqueKey"]);

                destinationProject.CollectedData.LoadRecordIntoView(destinationView, uniqueKey);                

                foreach (Field destField in destinationView.Fields)
                {
                    try
                    {
                        if (destField is ImageField)
                        {
                            // TODO: FIX!!!

                            imagePath = sourceRow[destField.Name].ToString();

                            if (imagePath.StartsWith(".."))
                            {
                                imagePath = imagePath.TrimStart('.');
                                imagePath = "C:" + imagePath;
                            }
                            //imagePath = Path.Combine(sourceProject.Location,Path.GetFileName(imagePath));
                            byte[] imageAsBytes = Util.GetByteArrayFromImagePath(imagePath);
                            ((ImageField)destField).CurrentRecordValue = imageAsBytes;
                            imageFieldsProcessed++;

                            //throw new ApplicationException("Image imports not supported.");
                        }
                    }
                    catch 
                    {
                        Logger.Log(DateTime.Now + ":  " + string.Format(SharedStrings.IMPORT_WARNING_NO_IMAGE_COPIED, imagePath, destinationView.Name)); 
                    }
                }

                // TODO: FIX!!!
                destinationView.SaveRecord(uniqueKey);
            }

            status = string.Format(SharedStrings.IMPORT_NOTIFICATION_IMAGE_UPDATE_END, imageFieldsProcessed.ToString(), destinationView.Name);
            args = new MessageEventArgs(status, false);
            RaiseEventImportStatus(status, args);
        }

        private void CopyPages(Epi.Epi2000.View sourceView, View destinationView)
		{
            try
            {
                foreach (Epi.Epi2000.Page sourcePage in sourceView.Pages)
                {
                    string validatedName = string.Empty;

                    // To prevent oddly-named pages from being imported and later causing errors.
                    for (int i = 0; i < sourcePage.Name.Length; i++)
                    {
                        string viewChar = sourcePage.Name.Substring(i, 1);
                        Match m = Regex.Match(viewChar, "[A-Za-z0-9 .]");
                        if (m.Success)
                        {
                            validatedName = validatedName + viewChar;
                        }
                    }

                    foreach (Epi.Epi2000.Page page in sourceView.Pages)
                    {
                        if (page.Position != sourcePage.Position && page.Name.Equals(sourcePage.Name))
                        {
                            validatedName = validatedName + " " + (sourcePage.Position + 1).ToString();
                            break;
                        }
                    }

                    if (sourcePage.Name != validatedName)
                    {
                        Logger.Log(DateTime.Now + ":  " + string.Format(SharedStrings.IMPORT_NOTIFICATION_PAGE_RENAME, sourcePage.Name, validatedName));
                        sourcePage.Name = validatedName;
                        // necessary to re-set the page name in the check code block to match the validatedName
                        // we just set in the above line of code
                        sourcePage.RebuildCheckCode(
                            sourceProject.Metadata.GetPageCheckCode(sourceView, sourcePage.Position), sourceView);
                    }
                                        
                    destinationView.CreatePage(sourcePage.Name, sourcePage.Position);
                    Page destinationPage = destinationView.GetPageByPosition(sourcePage.Position);
                    sourcePage.CopyTo(destinationPage);
                    CheckCode.Append(destinationPage.CheckCodeBefore);
                    destinationPage.CheckCodeBefore = "";
                    destinationPage.SaveToDb();
                    RaiseEventImportStatus(destinationPage.DisplayName);
                    CopyFields(destinationPage);
                }
                UpdateMirrorFields(destinationView);
            }
            catch (Exception ex)
            {
                throw ex;
            }
		}

		/// <summary>
		/// Converts related views
		/// </summary>
		private void CopyViewRelations()
		{
            foreach (Epi.Epi2000.View sourceView in sourceProject.Views)
			{
				DataTable relates = sourceProject.Metadata.GetViewRelations(sourceView.Name);

				foreach (DataRow relate in relates.Rows)
				{
                    string fieldName = relate[ColumnNames.FIELD_NAME].ToString().Trim();
                    string relatedViewName = Epi.Epi2000.View.StripViewNameOfPrefix(relate[ColumnNames.RELATE_TABLE].ToString().Trim());
	 				string relateCondition = relate[ColumnNames.RELATE_CONDITION].ToString().Trim();
                    
					bool shouldReturnToParent = (string.Compare(relate[ColumnNames.SHOULD_RETURN_TO_PARENT].ToString(), "One", true) == 0);
					
					if (!string.IsNullOrEmpty(relatedViewName))					
					{
						DataRow destinationFieldRow = destinationProject.Metadata.GetFieldGUIDByNameAsDataRow(sourceView.NameWithoutPrefix, fieldName);
                        if (destinationProject.Views.Contains(relatedViewName))
                        {
                            View relatedView = destinationProject.GetViewByName(relatedViewName);
                            Guid uniqueId = (Guid)destinationFieldRow["UniqueId"];
                            destinationProject.Metadata.RelateFieldToView(uniqueId, relatedView.Id, relateCondition, shouldReturnToParent);
                        }
                        else
                        {
                            Logger.Log(DateTime.Now + ":  " + string.Format(SharedStrings.IMPORT_WARNING_RELATE_FIELD, fieldName, relatedViewName));
                        }
					}
                    RaiseEventProgressBarStep();
				}
			}
		}

		/// <summary>
		/// Converts the fields in a page
		/// </summary>
		/// <param name="page">A page object</param>
		private void CopyFields(Page page)
		{
			try
			{
                string viewNameWithPrefix = "view" + page.GetView().Name;
                DataTable fieldsTable = sourceProject.Metadata.GetFieldsOnPageAsDataTable(viewNameWithPrefix, page.Position);
                bool hasOptionFields = false;

				foreach (DataRow fieldRow in fieldsTable.Rows)
				{
                    string fieldName = fieldRow["Name"].ToString();
                    
                    if (Char.IsNumber(fieldName[0]))
                    {
                        Logger.Log(DateTime.Now + ":  " + string.Format(SharedStrings.IMPORT_WARNING_FIELD_LEADING_DIGIT, fieldName));
                        continue;
                    }

                    MetaFieldType fieldType = Epi.Epi2000.MetadataDbProvider.InferFieldType(fieldRow);
                    Epi.Data.Services.CollectedDataProvider collectedDataProvider = destinationProject.CollectedData;
                    Epi.Data.IDbDriver db = collectedDataProvider.GetDatabase();

                    if (fieldType != MetaFieldType.CommandButton && fieldType != MetaFieldType.Grid && fieldType != MetaFieldType.Group && fieldType != MetaFieldType.LabelTitle && fieldType != MetaFieldType.Mirror && fieldType != MetaFieldType.Relate &&
                        db.ColumnExists(page.TableName, fieldName) == false)
                    {
                        if (db.TableExists(page.TableName))
                        {
                            Logger.Log(DateTime.Now + ":  " + string.Format(
                                SharedStrings.IMPORT_LOG_NO_DATA_COLUMN, fieldName, page.TableName));
                        }
                    }
                    
                    if (fieldType != MetaFieldType.Codes)
                    {
                        CopyField(fieldRow, page, viewNameWithPrefix);
                    }
                    if (fieldType == MetaFieldType.Option)
                    {
                        hasOptionFields = true;
                    }
				}

                DataRow[] codeRows = fieldsTable.Select(ColumnNames.TYPE + " = 'COMBO'");
                foreach (DataRow codeRow in codeRows)
                {
                    MetaFieldType fieldType = Epi.Epi2000.MetadataDbProvider.InferFieldType(codeRow);
                    if (fieldType == MetaFieldType.Codes)
                    {
                        CopyField(codeRow, page, viewNameWithPrefix);
                    }
                }

                if (hasOptionFields == true)
                {
                    UpdateOptionFields(page);
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
        /// Updates option field data in the destination view's data table to match
        /// Epi Info 7 format for option fields.
        /// </summary>
        /// <param name="page">A page in the destination view</param>
        private void UpdateOptionFields(Page page)
        {
            // Epi Info 7 stores data for option fields as integers, with 0 being the 1st option, 2 for the second option, etc. Epi Info 3 just
            // stores the literal text value that the user selected. We must convert from Epi 3 format to Epi 7 format otherwise we'll wind up with
            // errors on import. TODO: Check for SQL server compliance; likely SQL server will behave differently than Access-to-Access imports.
            try
            {                
                string viewNameWithPrefix = "view" + page.GetView().Name;
                string tableName = page.GetView().TableName;
                
                DataTable fieldsTable = sourceProject.Metadata.GetFieldsOnPageAsDataTable(viewNameWithPrefix, page.Position);

                // iterate over all fields in the page
                foreach (DataRow fieldRow in fieldsTable.Rows)
                {                    
                    // find the field type
                    MetaFieldType fieldType = Epi.Epi2000.MetadataDbProvider.InferFieldType(fieldRow);

                    // If it's an option field...
                    if (fieldType == MetaFieldType.Option)
                    {
                        string fieldName = fieldRow["Name"].ToString();

                        Epi.Data.Services.CollectedDataProvider collectedDataProvider = destinationProject.CollectedData;
                        Epi.Data.IDbDriver db = collectedDataProvider.GetDatabase();

                        // If the Epi 3 view is corrupted in such a way that the data 
                        // column for this field doesn't exist, we have to break here else we run into errors
                        if (db.ColumnExists(page.GetView().TableName, fieldName) == false)
                        {
                            break;
                        }
                        
                        // We start a queue. The queue will store each option in the option field.
                        Queue<string> options = new Queue<string>();

                        // Enqueue items to the queue in the order they came out of the list column
                        string[] items = fieldRow["Lists"].ToString().Split(';');
                        for (int i = 0; i < items.Length; i++)
                        {
                            options.Enqueue(items[i]);
                        }

                        // Create a temporary column that will store our translated data
                        string tempFieldName = "Epi7_import_" + fieldName;
                        TableColumn column = new TableColumn(tempFieldName, GenericDbColumnType.Int16, true);
                        db.AddColumn(tableName, column);

                        // For each option in our options queue...
                        for(int i = 0; options.Count > 0; i++) 
                        {
                            // ... run an UPDATE query that upates our temporary import column with an int16 value based on the Epi 3 string literal value
                            Query updateQuery = db.CreateQuery("UPDATE " + tableName + " SET " + tempFieldName + " = " + (i).ToString() + " WHERE " + fieldName + " = @Options"); // TODO: If Epi 7 option fields are ever changed from 0-based to 1-based, this MUST be updated, too!
                            updateQuery.Parameters.Add(new QueryParameter("@Options", DbType.String,  options.Dequeue()));
                            db.ExecuteNonQuery(updateQuery);
                        }
                        // Delete the original Epi 3 column with its string literal values
                        db.DeleteColumn(tableName, fieldName);
                        // Very important - we must compact to avoid 'Too many fields defined' error
                        db.CompactDatabase();

                        // We now need to re-create the data column for our field, which we just deleted
                        column = new TableColumn(fieldName, GenericDbColumnType.Int16, true);
                        db.AddColumn(tableName, column);

                        // Then we copy the data from the temp import field into the column we just added above
                        // (Note: We can't do an ALTER TABLE... RENAME COLUMN because Access does not support this)
                        Query copyQuery = db.CreateQuery("UPDATE " + tableName + " SET " + fieldName + " = " + tempFieldName);
                        db.ExecuteNonQuery(copyQuery);

                        // Finally, we delete the temporary import column.
                        db.DeleteColumn(tableName, tempFieldName);
                    }
                }
            }
            catch
            {
            }
        }


		/// <summary>
		/// Updates the sourceProject for a mirror field
		/// </summary>
		/// <param name="field">A mirror field</param>
		private void UpdateMirrorFieldSource(MirrorField field)
		{
            try
            {
                field.SourceFieldId = GetSourceFieldId(field);
                field.SaveToDb();                
            }
            //// KKM4 TODO: For now, ignore mirror field errors. Neet to revisit it ASAP.
            //catch (Exception ex)
            //{
            //}
            finally
            {
            }
		}

		/// <summary>
		/// Gets the source field id of a mirror field
		/// </summary>
		/// <param name="field">A mirror field</param>
		/// <returns>The source field id</returns>
		private int GetSourceFieldId(MirrorField field)
		{
			string sourceFieldName = GetSourceFieldName(field);            
			return GetFieldIdByName(sourceFieldName, field.GetView());
		}

		/// <summary>
		/// Gets the source field name of a mirror field
		/// </summary>
		/// <param name="field">A mirror field</param>
		/// <returns>The source field name</returns>
		private string GetSourceFieldName(MirrorField field)
		{
            // Epi2000 views are alwys stored with prefix.
            string viewName = "view" + field.GetView().Name;
			return sourceProject.Metadata.GetSourceFieldName(field.Name, viewName);
		}

		/// <summary>
		/// Gets a field id by its name
		/// </summary>
		/// <param name="fieldName">The field name</param>
		/// <param name="view">An Epi7 view</param>
		/// <returns>Field Id</returns>
		private int GetFieldIdByName(string fieldName, View view)
		{
			DataRow results = destinationProject.Metadata.GetFieldIdByNameAsDataRow(view.Name, fieldName);
			return (int) results["FieldId"];
		}

		/// <summary>
		/// Converts a field
		/// </summary>
		/// <param name="fieldRow">A field record from the legacy database</param>
		/// <param name="page">A page object</param>
		/// <param name="tableName">Name of the view table in EI 3.x</param>
		private void CopyField(DataRow fieldRow, Page page, string tableName)
		{
            RenderableField field = null;
            try
            {
                bool FieldHasBeforeCheckCode = false;
                bool FieldHasAfterCheckCode = false;
                bool FieldHasClickCheckCode = false;

                string ckBefore = String.Empty;
                string ckAfter = String.Empty;

                MetaFieldType fieldType = Epi.Epi2000.MetadataDbProvider.InferFieldType(fieldRow);
                field = (RenderableField)page.CreateField(fieldType);

                if (field is GridField)
                {
                    // Grids not supported in initial 7.0.7 release
                    Logger.Log(DateTime.Now + ":  " + string.Format(SharedStrings.IMPORT_WARNING_GRID_NOT_SUPPORTED, field.Name));
                    return;
                }

                // Field Name
                string fieldName = fieldRow[ColumnNames.NAME].ToString();
                // KKM4 - Commented out to test the requirement of Reserved words.
                //if (Epi.Data.Services.AppData.Instance.IsReservedWord(fieldName))
                //{
                //    throw new ReservedWordException(fieldName);
                //}
                //else
                //{
                //    field.Name = fieldName;
                //}
                field.Name = fieldName;

                // ControlFont
                if (!string.IsNullOrEmpty(fieldRow["Ffont"].ToString()))
                {
                    if (!string.IsNullOrEmpty(fieldRow["Ffontsize"].ToString()))
                    {
                        if (float.Parse(fieldRow["Ffontsize"].ToString()) > 0)
                        {
                            field.ControlFont = new System.Drawing.Font(fieldRow["Ffont"].ToString(), float.Parse(fieldRow["Ffontsize"].ToString()));
                        }
                    }
                }

                // ControlHeightPercentage
                if (fieldRow["Fsize"] != DBNull.Value)
                {
                    field.ControlHeightPercentage = (double)fieldRow["Fsize"];
                }

                // ControlLeftPositionPercentage
                if (fieldRow["Flocx"] != DBNull.Value)
                {
                    field.ControlLeftPositionPercentage = ((double)fieldRow["Flocx"]) / 100;
                }

                // ControlTopPositionPercentage
                if (fieldRow["Flocy"] != DBNull.Value)
                {
                    field.ControlTopPositionPercentage = ((double)fieldRow["Flocy"]) / 100;
                }

                // ControlWidthPercentage
                if (fieldRow["Dsize"] != DBNull.Value)
                {
                    field.ControlWidthPercentage = (double)fieldRow["Dsize"];
                }

                // HasTabStop
                field.HasTabStop = true;

                // Prompt
                if (fieldRow["Prompt"] != DBNull.Value)
                {
                    field.PromptText = fieldRow["Prompt"].ToString();
                }

                // TabIndex
                if (fieldRow["Taborder"] != DBNull.Value)
                {
                    field.TabIndex = int.Parse(fieldRow["Taborder"].ToString());
                }

                // Check Code
                if (fieldRow[ColumnNames.CHECK_CODE] != DBNull.Value)
                {
                    string checkCode = fieldRow[ColumnNames.CHECK_CODE].ToString();
                    Epi.Epi2000.MetadataDbProvider.SplitCheckCode(checkCode, ref ckBefore, ref ckAfter);

                    if (field is IFieldWithCheckCodeAfter)
                    {
                        if (!string.IsNullOrEmpty(ckAfter))
                        {
                            FieldHasAfterCheckCode = true;
                        }
                    }

                    if (field is IFieldWithCheckCodeBefore)
                    {
                        if (!string.IsNullOrEmpty(ckBefore))
                        {
                            FieldHasBeforeCheckCode = true;
                        }
                    }

                    if (field is CommandButtonField)
                    {
                        if (!string.IsNullOrEmpty(ckAfter))
                        {
                            FieldHasClickCheckCode = true;
                        }
                    }
                }

                if (FieldHasBeforeCheckCode || FieldHasAfterCheckCode || FieldHasClickCheckCode)
                {
                    CheckCode.Append("\nField ");
                    if (field.Name.Trim().IndexOf(' ') > -1)
                    {
                        CheckCode.Append("[");
                        CheckCode.Append(field.Name.Trim());
                        CheckCode.Append("]");
                    }
                    else
                    {
                        CheckCode.Append(field.Name.Trim());
                    }
                    if (FieldHasBeforeCheckCode)
                    {
                        CheckCode.Append("\n\tBefore\n\t\t");
                        CheckCode.Append(ckBefore.Replace("\n", "\n\t\t"));
                        CheckCode.Append("\n\tEnd-Before\n");
                    }
                    
                    if (FieldHasAfterCheckCode)
                    {
                        CheckCode.Append("\n\tAfter\n\t\t");
                        CheckCode.Append(ckAfter.Replace("\n", "\n\t\t"));
                        CheckCode.Append("\n\tEnd-After\n");
                    }
                    if (FieldHasClickCheckCode)
                    {
                        CheckCode.Append("\n\tClick\n\t\t");
                        CheckCode.Append(ckAfter.Replace("\n","\n\t\t"));
                        CheckCode.Append("\n\tEnd-Click\n");
                    }

                    CheckCode.Append("End-Field\n");
                }

                if (field is FieldWithSeparatePrompt)
                {
                    // PromptLeftPositionPercentage
                    if (fieldRow["Plocx"] != DBNull.Value)
                    {
                        ((FieldWithSeparatePrompt)field).PromptLeftPositionPercentage = double.Parse(fieldRow["Plocx"].ToString()) / 100;
                    }
                    // PromptTopPositionPercentage
                    if (fieldRow["Plocy"] != DBNull.Value)
                    {
                        ((FieldWithSeparatePrompt)field).PromptTopPositionPercentage = double.Parse(fieldRow["Plocy"].ToString()) / 100;
                    }
                    // PromptFont
                    if (fieldRow["Pfont"] != DBNull.Value)
                    {
                        ((FieldWithSeparatePrompt)field).PromptFont = new System.Drawing.Font(fieldRow["Pfont"].ToString(), float.Parse(fieldRow["Pfontsize"].ToString()));
                    }
                }
                if (field is IInputField)
                {
                    string lists = fieldRow["Lists"].ToString().Trim();
                    int parenPos = lists.IndexOf('(');
                    if (parenPos < 0)
                    {
                        parenPos = lists.Length;
                    }
                    lists = lists.Substring(0, parenPos).ToUpper().Trim();
                    if (!string.IsNullOrEmpty(lists))
                    {
                        ((IInputField)field).IsRequired = lists.Contains("M");
                        ((IInputField)field).IsReadOnly = lists.Contains("N");
                        ((IInputField)field).ShouldRepeatLast = lists.Contains("R");
                    }
                }
                if (field is ImageField)
                {
                    ((ImageField)field).ShouldRetainImageSize = false;
                }
                if (field is LabelField)
                {
                    if (fieldRow["Plocx"] != DBNull.Value)
                    {
                        field.ControlLeftPositionPercentage = double.Parse(fieldRow["Plocx"].ToString()) / 100;
                    }
                    if (fieldRow["Plocy"] != DBNull.Value)
                    {
                        field.ControlTopPositionPercentage = double.Parse(fieldRow["Plocy"].ToString()) / 100;
                    }
                    if ((fieldRow["Pfont"] != DBNull.Value) && (fieldRow["Pfontsize"] != DBNull.Value))
                    {
                        field.ControlFont = new System.Drawing.Font(fieldRow["Pfont"].ToString(), float.Parse(fieldRow["Pfontsize"].ToString()));
                    }
                }
                if (field is TableBasedDropDownField)
                {
                    ((TableBasedDropDownField)field).SourceTableName = GetCodeTableName(fieldRow["Lists"].ToString(), tableName);
                    ((TableBasedDropDownField)field).TextColumnName = GetCodeTableTextField(fieldRow["Lists"].ToString());
                }
                if (field is DDLFieldOfCodes)
                {
                    ((DDLFieldOfCodes)field).CodeColumnName = GetCodeTableValueField(fieldRow["Lists"].ToString());
                    if (!(field is DDLFieldOfCommentLegal) && !(field is DDLFieldOfLegalValues))
                    {
                        ((DDLFieldOfCodes)field).AssociatedFieldInformation = GetCodeTableAssociatedFields(fieldRow["Lists"].ToString(), field.GetView());
                    }
                }
                if (field is MirrorField)
                {
                    ((MirrorField)field).SourceFieldId = 1;
                }

                if (field is RelatedViewField)
                {
                    // Relate Condition
                    if (fieldRow["FormatString"] != DBNull.Value)
                    {
                        ((RelatedViewField)field).Condition = (fieldRow["FormatString"].ToString());
                    }
                    ((RelatedViewField)field).RelatedViewID = 1; // this will be re-set later during CopyViewRelations()
                }

                if (field is NumberField)
                {
                    if (fieldRow["FormatString"] != DBNull.Value)
                    {
                        Match m = Regex.Match(fieldRow["FormatString"].ToString(), "[;]");
                        if (m.Success)
                        {
                            string[] pattern = (fieldRow["FormatString"].ToString()).Split(';');
                            ((NumberField)field).Pattern = pattern[1];
                        }
                        else
                        {
                            ((NumberField)field).Pattern = (fieldRow["FormatString"].ToString());
                        }
                    }
                }
                else if (field is PhoneNumberField)
                {
                    if (fieldRow["FormatString"] != DBNull.Value)
                    {
                        Match m = Regex.Match(fieldRow["FormatString"].ToString(), "[;]");
                        if (m.Success)
                        {
                            string[] pattern = (fieldRow["FormatString"].ToString()).Split(';');
                            ((PhoneNumberField)field).Pattern = pattern[1];
                        }
                        else
                        {
                            ((PhoneNumberField)field).Pattern = (fieldRow["FormatString"].ToString());
                        }
                    }
                }
                else if (field is TextField)
                {
                    if (fieldRow["FormatString"] != DBNull.Value)
                    {
                        int maxLength = 0;
                        if (Int32.TryParse(fieldRow["FormatString"].ToString(), out maxLength) && maxLength > 0)
                        {
                            ((TextField)field).MaxLength = maxLength;
                        }
                    }
                }
                else if (field is GroupField)
                {
                    if (fieldRow["Plocx"] != DBNull.Value)
                    {
                        field.ControlLeftPositionPercentage = double.Parse(fieldRow["Plocx"].ToString()) / 100;
                    }
                    if (fieldRow["Plocy"] != DBNull.Value)
                    {
                        field.ControlTopPositionPercentage = double.Parse(fieldRow["Plocy"].ToString()) / 100;
                    }
                    if (fieldRow["Flocx"] != DBNull.Value)
                    {
                        field.ControlWidthPercentage = double.Parse(fieldRow["Flocx"].ToString());
                    }
                    if (fieldRow["Flocy"] != DBNull.Value)
                    {
                        field.ControlHeightPercentage = double.Parse(fieldRow["Flocy"].ToString());
                    }
                    
                    string[] items = fieldRow["Lists"].ToString().Split(';');
                    ((GroupField)field).ChildFieldNames = string.Join(",", items);
                }
                else if (field is OptionField)
                {
                    string[] items = fieldRow["Lists"].ToString().Split(';');
                    // TODO: We have to strip the commas out because that's how the values are delimited in Epi 7 now. Later, change how this is delimited (and thus we need to remove this code at that point).
                    if (fieldRow["Lists"].ToString().Contains(","))
                    {
                        for (int i = 0; i < items.Length; i++)
                        {
                            items[i] = items[i].Replace(",", string.Empty);
                        }
                    }
                    ((OptionField)field).Options = new System.Collections.Generic.List<string>();
                    ((OptionField)field).Options.AddRange(items);
                }

                if (field is CheckBoxField)
                {
                    AdjustCheckboxWidth(field);
                }

                field.SaveToDb();

                if (field is GridField)
                {
                    DataTable columns = sourceProject.Metadata.GetGridColumns(fieldRow["Ffont"].ToString());
                    foreach (DataRow column in columns.Rows)
                    {
                        MetaFieldType columnType = Epi.Epi2000.MetadataDbProvider.InferFieldType(column);
                        GridColumnBase gridColumn = ((GridField)field).CreateGridColumn(columnType);
                        gridColumn.Name = column[ColumnNames.NAME].ToString();
                        gridColumn.Text = column["Prompt"].ToString();
                        gridColumn.Width = int.Parse(column["Dsize"].ToString());
                        gridColumn.SaveToDb();
                    }
                }

                // Raise Import status event
                string fieldDisplayName = page.DisplayName + "::" + field.Name;
                RaiseEventProgressBarStep();
                MessageEventArgs args = new MessageEventArgs(fieldDisplayName + StringLiterals.ELLIPSIS, false);
                RaiseEventImportStatus(fieldDisplayName + StringLiterals.ELLIPSIS, args);
            }
            catch (Exception e)
            {
                Logger.Log(DateTime.Now + ":  " + string.Format(SharedStrings.IMPORT_ERROR_BAD_METADATA_IN_FIELD, field.Name, page.GetView().Name));
                string message = string.Format(SharedStrings.IMPORT_BAD_FIELD_DATA, field.Name, page.Name, page.GetView().Name);
                throw new GeneralException(message, e);
            }
			finally
			{
			} 
		}

        private void AdjustCheckboxWidth(RenderableField field)
        {
            //Graphics g = Graphics.FromHwnd(canvas.Handle);

            //int currentWidth = Convert.ToInt32(canvas.Width * field.ControlWidthPercentage);
            //Size textSize = TextRenderer.MeasureText(g, field.PromptText, field.PromptFont);
            ////add some width for the checkbox and make sure it doesn't wrap
            //textSize.Width += 40;

            //int leftPosition = (int)Math.Ceiling(canvas.PagePanel.Size.Width * field.ControlLeftPositionPercentage);
            //double adjustedWidthPercent = (double)textSize.Width / (double)canvas.PagePanel.Size.Width;

            //if (field.ControlWidthPercentage < adjustedWidthPercent)
            //{
            //    if (adjustedWidthPercent > 1)
            //        adjustedWidthPercent = 1.0;

            //    field.ControlWidthPercentage = adjustedWidthPercent;
            //}
        }

		/// <summary>
		/// Gets the name of a code table based on the "lists" column
		/// </summary>
		/// <param name="lists">The contents of the "lists" column</param>
		/// <param name="tableName">The name of the legacy table</param>
		/// <returns>Name of the code table</returns>
		private string GetCodeTableName(string lists, string tableName)
		{
			string[] rightSide = lists.Split('(');
			if (rightSide.Length > 1)
			{
				string[] leftSide = rightSide[1].Split(',');
				if (leftSide.Length > 1)
				{
					string tableVariableName = leftSide[0];
					DataTable codeTable = sourceProject.Metadata.GetCodeTableName(tableName, tableVariableName);
					if (codeTable.Rows.Count > 0)
					{
						return codeTable.Rows[0]["Datatable"].ToString();
					}
				}
			}
			return string.Empty;
		}

		/// <summary>
		/// Gets the name of the text field of a code table based on the "lists" column
		/// </summary>
		/// <param name="lists">The contents of the "lists" column</param>
		/// <returns>Name of the code table</returns>
		private string GetCodeTableTextField(string lists)
		{
			string textFieldName = string.Empty;
			string[] rightSide = lists.Split('(');
			if (rightSide.Length > 1)
			{
				string[] leftSide = rightSide[1].Split(',');
				if (leftSide.Length > 1)
				{
					textFieldName = leftSide[1].Trim(')');
					string[] asterisks = textFieldName.Split('*');
					if (asterisks.Length > 1)
					{
						textFieldName = asterisks[1];
					}
				}
			}
			return textFieldName;
		}

		/// <summary>
		/// Gets the name of the value field of a code table based on the "lists" column
		/// </summary>
		/// <param name="lists">The contents of the "lists" column</param>
		/// <returns>Name of the code table</returns>
		private string GetCodeTableValueField(string lists)
		{
			string valueFieldName = string.Empty;
			string[] rightSide = lists.Split('(');
			if (rightSide.Length > 1)
			{
				string[] leftSide = rightSide[1].Split(',');
				if (leftSide.Length > 2)
				{
					valueFieldName = leftSide[2].Trim(')').Trim();
					string[] asterisks = valueFieldName.Split('*');
					if (asterisks.Length > 1)
					{
						valueFieldName = asterisks[1];
					}
				}
			}
			return valueFieldName;
		}

        private string GetCodeTableAssociatedFields(string lists, View view)
        {
            System.Text.StringBuilder associatedFields = new System.Text.StringBuilder();

            string[] rightSide = lists.Split('(');
            if (rightSide.Length > 1)
            {
                string[] leftSide = rightSide[1].Trim(')').Trim().Split(',');

                int i = 0;
                foreach (string txtField in leftSide)
                {
                    if (i > 1)
                    {
                        string[] asterisks = txtField.Split('*');
                        if (asterisks.Length > 1)
                        {
                            if (associatedFields.Length > 0)
                            {
                                associatedFields.Append(StringLiterals.COMMA);
                            }
                            associatedFields.Append(asterisks[1].Trim());
                            associatedFields.Append(StringLiterals.COLON);
                            associatedFields.Append(GetFieldIdByName(asterisks[0].Trim(), view));

                        }
                    }
                    i++;
                }
            }

            return associatedFields.ToString();
        }

		/// <summary>
		/// Converts BGR color to RGB
		/// </summary>
		/// <param name="legacyEpiColor">Color in BGR format</param>
		/// <returns>Color in RBG format</returns>
		private int CopyColor(int legacyEpiColor)
		{
			try
			{
				string hex = legacyEpiColor.ToString("X");
				string red = "00";
				string green = "00";
				string blue = "00";
				switch (hex.Length)
				{
					case 1:
						red = "0" + hex;
						break;
					case 2: 
						red = hex;
						break;
					case 3: 
						red = hex.Substring(1,2);
						green = "0" + hex.Substring(0,1);
						break;
					case 4:
						red = hex.Substring(2,2);
						green = hex.Substring(0,2);
						break;
					case 5:
						red = hex.Substring(3,2);
						green = hex.Substring(1,2);
						blue = "0" + hex.Substring(0,1);
						break;
					case 6:
						red = hex.Substring(4,2);
						green = hex.Substring(2,2);
						blue = hex.Substring(0,2);
						break;
					default:
						break;
				}
				return int.Parse(red + green + blue, System.Globalization.NumberStyles.HexNumber);
			}
			catch (Exception ex)
			{
				throw new System.ApplicationException("Could not convert color data", ex);
			}
		}
		#endregion
	}
}
