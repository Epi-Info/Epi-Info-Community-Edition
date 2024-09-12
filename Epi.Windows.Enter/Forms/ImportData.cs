using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Epi;
using Epi.Data;
using Epi.Fields;
using Epi.Windows.Dialogs;

namespace Epi.Enter.Forms
{
    /// <summary>
    /// A user interface element to update or append records to the currently-open form from another similar, or identical, Epi Info 7 form.
    /// </summary>
    public partial class ImportDataForm : DialogBase
    {
        #region Private Members
        private bool isBatchImport = false; // currently unused
        private Project sourceProject;
        private View sourceView;
        private Project destinationProject;
        private View destinationView;
        private IDbDriver sourceProjectDataDriver;
        private IDbDriver destinationProjectDataDriver;
        private Configuration config;
        private int lastRecordId;
        private BackgroundWorker importWorker;
        private static object syncLock = new object();
        private Stopwatch stopwatch;
        private bool update = true;
        private bool append = true;
        private bool importFinished = false;
        #endregion // Private Members

        #region Delegates
        private delegate void SetStatusDelegate(string statusMessage);
        #endregion // Delegates

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public ImportDataForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor
        /// </summary>        
        /// <param name="destinationView">The destination form; should be the currently-open view</param>
        public ImportDataForm(View destinationView)
        {
            InitializeComponent();

            this.destinationProject = destinationView.Project;
            this.destinationView = destinationView;

            Construct();
        }
        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Gets/sets whether this is a batch import.
        /// </summary>
        public bool IsBatchImport
        {
            get
            {
                return this.isBatchImport;
            }
            set
            {
                this.isBatchImport = value;
            }
        }
        #endregion // Public Properties

        #region Private Methods
        /// <summary>
        /// Construct method
        /// </summary>
        private void Construct()
        {
            this.destinationProjectDataDriver = destinationProject.CollectedData.GetDbDriver();
            this.config = Configuration.GetNewInstance();
            //this.destinationGUIDList = new List<string>();

            this.importWorker = new BackgroundWorker();
            this.importWorker.WorkerSupportsCancellation = true;

            this.IsBatchImport = false;
            this.rdbUpdateAndAppend.Checked = true;
        }

        /// <summary>
        /// Initiates a full import on a single form
        /// </summary>
        private void DoImport()
        {
            string importTypeDescription;
            if (rdbUpdateAndAppend.Checked)
            {
                update = true;
                append = true;
                importTypeDescription = SharedStrings.IMPORT_DATA_UPDATE_MATCHING + " " + SharedStrings.IMPORT_DATA_APPEND_NONMATCHING;
            }
            else if (rdbUpdate.Checked)
            {
                update = true;
                append = false;
                importTypeDescription = SharedStrings.IMPORT_DATA_UPDATE_MATCHING + " " + SharedStrings.IMPORT_DATA_IGNORE_NONMATCHING;
            }
            else
            {
                update = false;
                append = true;
                importTypeDescription = SharedStrings.IMPORT_DATA_APPEND_NONMATCHING + " " + SharedStrings.IMPORT_DATA_IGNORE_MATCHING;
            }
            
            if (cmbFormName.SelectedIndex >= 0)
            {
                lbxStatus.Items.Clear();
                sourceView = sourceProject.Views[cmbFormName.SelectedItem.ToString()];
                lastRecordId = sourceView.GetLastRecordId();

                if (!CheckForProblems())
                {
                    return;
                }

                textProgress.Text = string.Empty;
                progressBar.Value = 0;
                progressBar.Minimum = 0;

                int recordCount = sourceView.GetRecordCount();
                int gridRowCount = 0;

                foreach (GridField gridField in sourceView.Fields.GridFields)
                {
                    IDataReader reader = sourceProjectDataDriver.GetTableDataReader(gridField.TableName);
                    while (reader.Read())
                    {
                        gridRowCount++;
                    }
                }
                progressBar.Maximum = recordCount * (sourceView.Pages.Count + 1);
                progressBar.Maximum = progressBar.Maximum + gridRowCount;

                if (FormsAreAlike())
                {
                    List<View> viewsToProcess = new List<View>();
                    
                    foreach (View view in sourceProject.Views)
                    {
                        try
                        {
                            if (view.IsRelatedView && !string.IsNullOrEmpty(view.TableName) && sourceProjectDataDriver.TableExists(view.TableName) && destinationProject.Views.Contains(view.Name))
                            {
                                if (IsDescendant(view, sourceView) && !viewsToProcess.Contains(view))
                                {
                                    viewsToProcess.Add(view);
                                    progressBar.Maximum = progressBar.Maximum + (view.GetRecordCount() * (view.Pages.Count + 1));
                                    int relatedGridRowCount = 0;
                                    foreach (GridField relatedGridField in view.Fields.GridFields)
                                    {
                                        IDataReader relatedGridReader = sourceProjectDataDriver.GetTableDataReader(relatedGridField.TableName);
                                        while (relatedGridReader.Read())
                                        {
                                            relatedGridRowCount++;
                                        }
                                    }
                                    progressBar.Maximum = progressBar.Maximum + relatedGridRowCount;
                                }
                            }
                        }
                        catch (NullReferenceException)
                        {
                            continue;
                        }
                    }

                    progressBar.Visible = true;

                    stopwatch = new Stopwatch();
                    stopwatch.Start();

                    AddStatusMessage(SharedStrings.IMPORT_DATA_STARTED + " " + cmbFormName.SelectedItem.ToString() + "; " + importTypeDescription);

                    btnBrowse.Enabled = false;
                    btnCancel.Enabled = false;
                    btnOK.Enabled = false;
                    cmbFormName.Enabled = false;
                    rdbUpdateAndAppend.Enabled = false;
                    rdbUpdate.Enabled = false;
                    rdbAppend.Enabled = false;
                    textProjectFile.Enabled = false;

                    if (importWorker.WorkerSupportsCancellation)
                    {
                        importWorker.CancelAsync();
                    }

                    this.Cursor = Cursors.WaitCursor;

                    importWorker.WorkerSupportsCancellation = true;
                    importWorker = new BackgroundWorker();
                    importWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                    importWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_WorkerCompleted);
                    importWorker.RunWorkerAsync(viewsToProcess);
                }
            }
        }

        /// <summary>
        /// Sets the status message of the import form
        /// </summary>
        /// <param name="message">The status message to display</param>
        private void SetStatusMessage(string message)
        {
            textProgress.Text = message;
        }

        /// <summary>
        /// Increments the progress bar by a given value
        /// </summary>
        /// <param name="value">The value by which to increment</param>
        private void IncrementProgressBarValue(double value)
        {
            progressBar.Increment((int)value);
        }

        /// <summary>
        /// Processes all related forms
        /// </summary>
        /// <param name="sourceView">The source form</param>
        /// <param name="destinationView">The destination form</param>
        private void ProcessRelatedForms(View sourceView, View destinationView, List<View> viewsToProcess)
        {
            foreach (View view in viewsToProcess)
            {
                if (destinationProjectDataDriver.TableExists(view.TableName))
                {
                    Query selectQuery = destinationProjectDataDriver.CreateQuery("SELECT [GlobalRecordId] FROM [" + view.TableName + "]");
                    IDataReader destReader = destinationProjectDataDriver.ExecuteReader(selectQuery);
                    List<string> destinationRelatedGUIDList = new List<string>();

                    View relatedDestinationView = destinationProject.Views[view.Name];

                    while (destReader.Read())
                    {
                        destinationRelatedGUIDList.Add(destReader[0].ToString());
                    }
                    ProcessBaseTable(view, relatedDestinationView, destinationRelatedGUIDList);
                    ProcessPages(view, relatedDestinationView, destinationRelatedGUIDList);
                    ProcessGridFields(view, relatedDestinationView);
                    // Do not process related forms again, that's all being done in this loop
                }
                else
                {
                    this.BeginInvoke(new SetStatusDelegate(AddWarningMessage), string.Format(SharedStrings.IMPORT_ERROR_NO_DATA_TABLE ,view.Name) + " " + SharedStrings.IMPORT_ERROR_CREATE_DATA_TABLE);
                }
            }
        }

        /// <summary>
        /// Recursive function used to find if a view is a descendant of a given parent
        /// </summary>
        /// <param name="view">The view to check</param>
        /// <param name="parentView">The parent view</param>
        /// <returns>bool</returns>
        private bool IsDescendant(View view, View parentView)
        {
            if (view.ParentView == null)
            {
                if (view.Name == parentView.Name)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (view.ParentView.Name == parentView.Name)
            {
                return true;
            }
            else
            {
                return IsDescendant(view.ParentView, parentView);
            }
        }

        /// <summary>
        /// Processes all of the grid fields on a given form
        /// </summary>
        /// <param name="sourceView">The source form</param>
        /// <param name="destinationView">The destination form</param>        
        private void ProcessGridFields(View sourceView, View destinationView)
        {
            foreach (GridField gridField in sourceView.Fields.GridFields)
            {
                List<string> gridGUIDList = new List<string>();

                if (destinationView.Fields.GridFields.Contains(gridField.Name))
                {
                    int recordsUpdated = 0;
                    int recordsInserted = 0;
                    this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), string.Format(SharedStrings.IMPORT_DATA_PROCESSING_GRID, gridField.Name));

                    try
                    {
                        List<string> gridColumnsToSkip = new List<string>();

                        string destinationGridTableName = destinationView.Fields.GridFields[gridField.Name].TableName;
                        GridField destinationGridField = destinationView.Fields.GridFields[gridField.Name];
                        IDataReader destinationGridTableReader = destinationProjectDataDriver.GetTableDataReader(destinationGridTableName);
                        
                        foreach (GridColumnBase gridColumn in gridField.Columns)
                        {
                            bool found = false;
                            foreach (GridColumnBase destinationGridColumn in destinationGridField.Columns)
                            {
                                if (destinationGridColumn.Name.ToLowerInvariant().Equals(gridColumn.Name.ToLowerInvariant()))
                                {
                                    found = true;
                                }
                            }
                            if (!found)
                            {
                                gridColumnsToSkip.Add(gridColumn.Name);
                            }
                        }

                        while (destinationGridTableReader.Read())
                        {
                            gridGUIDList.Add(destinationGridTableReader["UniqueRowId"].ToString());
                        }

                        IDataReader sourceGridTableReader = sourceProjectDataDriver.GetTableDataReader(gridField.TableName);
                        while (sourceGridTableReader.Read())
                        {
                            string GUID = sourceGridTableReader["UniqueRowId"].ToString();
                            if (gridGUIDList.Contains(GUID) && update)
                            {
                                int columns = 0;
                                StringBuilder sb = new StringBuilder();
                                List<QueryParameter> fieldValueParams = new List<QueryParameter>();
                                sb.Append("UPDATE " + destinationProjectDataDriver.InsertInEscape(destinationGridTableName) + " SET ");
                                foreach (GridColumnBase gridColumn in gridField.Columns)
                                {
                                    object data = sourceGridTableReader[gridColumn.Name];
                                    if (data == null || string.IsNullOrEmpty(data.ToString()) || data == DBNull.Value)
                                    {
                                        continue; // don't update current data with null values (according to product requirements)
                                    }
                                    else if (gridColumnsToSkip.Contains(gridColumn.Name))
                                    {
                                        continue; // don't try and update a grid row that may not exist
                                    }

                                    sb.Append(destinationProjectDataDriver.InsertInEscape(gridColumn.Name));
                                    sb.Append(StringLiterals.EQUAL);
                                    sb.Append("@" + gridColumn.Name);
                                    switch (gridColumn.GridColumnType)
                                    {
                                        case MetaFieldType.Date:
                                        case MetaFieldType.DateTime:
                                        case MetaFieldType.Time:
                                            //sb.Append(destinationProjectDataDriver.FormatDateTime((DateTime)data));                                        
                                            fieldValueParams.Add(new QueryParameter("@" + gridColumn.Name, DbType.DateTime, data));
                                            break;
                                        case MetaFieldType.CommentLegal:
                                        case MetaFieldType.LegalValues:
                                        case MetaFieldType.Codes:
                                        case MetaFieldType.Text:
                                        case MetaFieldType.TextUppercase:
                                        case MetaFieldType.PhoneNumber:
                                        case MetaFieldType.UniqueRowId:
                                        case MetaFieldType.ForeignKey:
                                        case MetaFieldType.GlobalRecordId:
                                            fieldValueParams.Add(new QueryParameter("@" + gridColumn.Name, DbType.String, data));
                                            break;
                                        case MetaFieldType.Number:
                                            fieldValueParams.Add(new QueryParameter("@" + gridColumn.Name, DbType.Double, data));
                                            break;
                                        case MetaFieldType.RecStatus:
                                        case MetaFieldType.Checkbox:
                                        case MetaFieldType.YesNo:
                                            fieldValueParams.Add(new QueryParameter("@" + gridColumn.Name, DbType.Single, data));
                                            break;
                                        default:
                                            throw new ApplicationException(SharedStrings.IMPORT_ERROR_INVALID_GRID_TYPE);
                                    }
                                    sb.Append(StringLiterals.COMMA);
                                    columns++;
                                }

                                if (columns == 0)
                                {
                                    continue;
                                }

                                sb.Length = sb.Length - 1;

                                sb.Append(" WHERE ");
                                sb.Append("[UniqueRowId] = ");
                                sb.Append("'" + GUID + "'");

                                Query updateQuery = destinationProjectDataDriver.CreateQuery(sb.ToString());
                                updateQuery.Parameters = fieldValueParams;
                                destinationProjectDataDriver.ExecuteNonQuery(updateQuery);
                                recordsUpdated++;
                                this.BeginInvoke(new SetProgressBarDelegate(IncrementProgressBarValue), 1);

                            }
                            else if (!gridGUIDList.Contains(GUID) && append)
                            {
                                int columns = 0;
                                List<QueryParameter> fieldValueParams = new List<QueryParameter>();
                                WordBuilder fieldNames = new WordBuilder(",");
                                WordBuilder fieldValues = new WordBuilder(",");
                                foreach (GridColumnBase gridColumn in gridField.Columns)
                                {
                                    object data = sourceGridTableReader[gridColumn.Name];
                                    if (data == null || string.IsNullOrEmpty(data.ToString()) || data == DBNull.Value)
                                    {
                                        continue; // don't update current data with null values (according to product requirements)
                                    }
                                    else if (gridColumnsToSkip.Contains(gridColumn.Name))
                                    {
                                        continue; // don't try and update a grid row that may not exist
                                    }

                                    fieldNames.Add(gridColumn.Name);
                                    fieldValues.Add("@" + gridColumn.Name);

                                    switch (gridColumn.GridColumnType)
                                    {
                                        case MetaFieldType.Date:
                                        case MetaFieldType.DateTime:
                                        case MetaFieldType.Time:                                     
                                            fieldValueParams.Add(new QueryParameter("@" + gridColumn.Name, DbType.DateTime, data));
                                            break;
                                        case MetaFieldType.CommentLegal:
                                        case MetaFieldType.LegalValues:
                                        case MetaFieldType.Codes:
                                        case MetaFieldType.Text:
                                        case MetaFieldType.TextUppercase:
                                        case MetaFieldType.PhoneNumber:
                                        case MetaFieldType.UniqueRowId:
                                        case MetaFieldType.ForeignKey:
                                        case MetaFieldType.GlobalRecordId:
                                            fieldValueParams.Add(new QueryParameter("@" + gridColumn.Name, DbType.String, data));
                                            break;
                                        case MetaFieldType.Number:
                                            fieldValueParams.Add(new QueryParameter("@" + gridColumn.Name, DbType.Double, data));
                                            break;
                                        case MetaFieldType.RecStatus:
                                        case MetaFieldType.Checkbox:
                                        case MetaFieldType.YesNo:
                                            fieldValueParams.Add(new QueryParameter("@" + gridColumn.Name, DbType.Single, data));
                                            break;
                                        default:
                                            throw new ApplicationException(SharedStrings.IMPORT_ERROR_INVALID_GRID_TYPE);
                                    }
                                    columns++;
                                }

                                if (columns == 0)
                                {
                                    continue;
                                }

                                StringBuilder sb = new StringBuilder();
                                sb.Append("INSERT INTO " + destinationProjectDataDriver.InsertInEscape(destinationGridTableName));
                                sb.Append(StringLiterals.SPACE);
                                sb.Append(Util.InsertInParantheses(fieldNames.ToString()));
                                sb.Append(" values (");
                                sb.Append(fieldValues.ToString());
                                sb.Append(") ");
                                Query insertQuery = destinationProjectDataDriver.CreateQuery(sb.ToString());
                                insertQuery.Parameters = fieldValueParams;

                                destinationProjectDataDriver.ExecuteNonQuery(insertQuery);
                                this.BeginInvoke(new SetProgressBarDelegate(IncrementProgressBarValue), 1);
                                recordsInserted++;
                            }
                        } // end while source data reader reads

                        sourceGridTableReader.Close();
                        sourceGridTableReader.Dispose();

                        this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), string.Format(SharedStrings.IMPORT_DATA_GRID_PROCESSING_COMPLETE, gridField.Name) + "; " + string.Format(SharedStrings.IMPORT_DATA_RECS_INSERTED, recordsInserted.ToString()) + "; " + string.Format(SharedStrings.IMPORT_DATA_RECS_UPDATED, recordsUpdated.ToString() + "."));
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(new SetStatusDelegate(AddErrorStatusMessage), ex.Message);
                    }
                } // end if contains
            }
        }

        /// <summary>
        /// Processes a form's base table
        /// </summary>
        /// <param name="sourceView">The source form</param>
        /// <param name="destinationView">The destination form</param>
        /// <param name="destinationGUIDList">The list of GUIDs that exist in the destination</param>
        private void ProcessBaseTable(View sourceView, View destinationView, List<string> destinationGUIDList)
        {
            sourceView.LoadFirstRecord();
            this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), SharedStrings.IMPORT_DATA_PROCESSING_RECS);

            int recordsInserted = 0;
            int recordsUpdated = 0;

            string sourceTable = sourceView.TableName;
            string destinationTable = destinationView.TableName;

            try
            {
                IDataReader sourceReader = sourceProjectDataDriver.GetTableDataReader(sourceView.TableName);
                while (sourceReader.Read())                
                {
                    object recordStatus = sourceReader["RECSTATUS"];
                    QueryParameter paramRecordStatus = new QueryParameter("@RECSTATUS", DbType.Int32, recordStatus);

                    if (importWorker.CancellationPending)
                    {
                        this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), SharedStrings.IMPORT_DATA_CANCELLED);
                        return;
                    }

                    WordBuilder fieldNames = new WordBuilder(StringLiterals.COMMA);
                    WordBuilder fieldValues = new WordBuilder(StringLiterals.COMMA);
                    List<QueryParameter> fieldValueParams = new List<QueryParameter>();

                    fieldNames.Append("GlobalRecordId");
                    fieldValues.Append("@GlobalRecordId");

                    string GUID = sourceReader["GlobalRecordId"].ToString();
                    string FKEY = sourceReader["FKEY"].ToString();
                    QueryParameter paramFkey = new QueryParameter("@FKEY", DbType.String, FKEY); // don't add this yet
                    QueryParameter paramGUID = new QueryParameter("@GlobalRecordId", DbType.String, GUID);
                    fieldValueParams.Add(paramGUID);

                    if (destinationGUIDList.Contains(GUID))
                    {
                        if (update)
                        {
                            // UPDATE matching records
                            string updateHeader = string.Empty;
                            string whereClause = string.Empty;
                            fieldValueParams = new List<QueryParameter>();
                            StringBuilder sb = new StringBuilder();

                            // Build the Update statement which will be reused
                            sb.Append(SqlKeyWords.UPDATE);
                            sb.Append(StringLiterals.SPACE);
                            sb.Append(destinationProjectDataDriver.InsertInEscape(destinationTable));
                            sb.Append(StringLiterals.SPACE);
                            sb.Append(SqlKeyWords.SET);
                            sb.Append(StringLiterals.SPACE);

                            updateHeader = sb.ToString();

                            sb.Remove(0, sb.ToString().Length);

                            // Build the WHERE caluse which will be reused
                            sb.Append(SqlKeyWords.WHERE);
                            sb.Append(StringLiterals.SPACE);
                            sb.Append(destinationProjectDataDriver.InsertInEscape(ColumnNames.GLOBAL_RECORD_ID));
                            sb.Append(StringLiterals.EQUAL);
                            sb.Append("'");
                            sb.Append(GUID);
                            sb.Append("'");
                            whereClause = sb.ToString();

                            sb.Remove(0, sb.ToString().Length);

                            //if (sourceView.ForeignKeyFieldExists)
                            if (!string.IsNullOrEmpty(FKEY))
                            {
                                sb.Append(StringLiterals.LEFT_SQUARE_BRACKET);
                                sb.Append("FKEY");
                                sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET);
                                sb.Append(StringLiterals.EQUAL);

                                sb.Append(StringLiterals.COMMERCIAL_AT);
                                sb.Append("FKEY");                               
                                fieldValueParams.Add(paramFkey);

                                Query updateQuery = destinationProjectDataDriver.CreateQuery(updateHeader + StringLiterals.SPACE + sb.ToString() + StringLiterals.SPACE + whereClause);
                                updateQuery.Parameters = fieldValueParams;

                                destinationProjectDataDriver.ExecuteNonQuery(updateQuery);

                                sb.Remove(0, sb.ToString().Length);
                                fieldValueParams.Clear();

                                recordsUpdated++;
                            }
                        }
                    }
                    else
                    {
                        if (append)
                        {
                            if (!string.IsNullOrEmpty(FKEY))
                            {
                                fieldNames.Append("FKEY");
                                fieldValues.Append("@FKEY");
                                fieldValueParams.Add(paramFkey);
                            }
                            fieldNames.Append("RECSTATUS");
                            fieldValues.Append("@RECSTATUS");
                            fieldValueParams.Add(paramRecordStatus);

                            // Concatenate the query clauses into one SQL statement.
                            StringBuilder sb = new StringBuilder();
                            sb.Append(" insert into ");
                            sb.Append(destinationProjectDataDriver.InsertInEscape(destinationTable));
                            sb.Append(StringLiterals.SPACE);
                            sb.Append(Util.InsertInParantheses(fieldNames.ToString()));
                            sb.Append(" values (");
                            sb.Append(fieldValues.ToString());
                            sb.Append(") ");
                            Query insertQuery = destinationProjectDataDriver.CreateQuery(sb.ToString());
                            insertQuery.Parameters = fieldValueParams;

                            destinationProjectDataDriver.ExecuteNonQuery(insertQuery);
                            recordsInserted++;
                        }
                    }
                    this.BeginInvoke(new SetProgressBarDelegate(IncrementProgressBarValue), 1);
                }
                sourceReader.Close();
                sourceReader.Dispose();
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new SetStatusDelegate(AddErrorStatusMessage), ex.Message);
            }
            finally
            {
            }
            this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), string.Format(SharedStrings.IMPORT_DATA_PAGE_PROCESSING_COMPLETE, destinationTable + "; " + string.Format(SharedStrings.IMPORT_DATA_RECS_INSERTED, recordsInserted.ToString()) + "; " + string.Format(SharedStrings.IMPORT_DATA_RECS_UPDATED, recordsUpdated.ToString()) + "."));
        }

        /// <summary>
        /// Processes all of the fields on a given form, page-by-page, except for the fields on the base table.
        /// </summary>
        /// <param name="sourceView">The source form</param>
        /// <param name="destinationView">The destination form</param>
        /// <param name="destinationGUIDList">The list of GUIDs that exist in the destination</param>
        private void ProcessPages(View sourceView, View destinationView, List<string> destinationGUIDList)
        {
            for (int i = 0; i < sourceView.Pages.Count; i++)
            {
                sourceView.LoadFirstRecord();
                this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), string.Format(SharedStrings.IMPORT_DATA_PROCESSING_RECS_PAGE, (i + 1).ToString(), sourceView.Pages.Count.ToString()));

                int recordsInserted = 0;
                int recordsUpdated = 0;

                Page sourcePage = sourceView.Pages[i];
                Page destinationPage = destinationView.Pages[i];

                try
                {
                    List<string> fieldsToSkip = new List<string>();
                    foreach (Field sourceField in sourceView.Fields)
                    {
                        bool found = false;
                        foreach (Field destinationField in destinationView.Fields)
                        {
                            if (destinationField.Name.ToLowerInvariant().Equals(sourceField.Name.ToLowerInvariant()))
                            {
                                found = true;
                            }
                        }
                        if (!found)
                        {
                            fieldsToSkip.Add(sourceField.Name);
                        }
                    }

                    IDataReader sourceReader = sourceProjectDataDriver.GetTableDataReader(sourcePage.TableName);
                    while (sourceReader.Read())
                    {
                        if (importWorker.CancellationPending)
                        {
                            this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), SharedStrings.IMPORT_DATA_CANCELLED);
                            return;
                        }

                        WordBuilder fieldNames = new WordBuilder(StringLiterals.COMMA);
                        WordBuilder fieldValues = new WordBuilder(StringLiterals.COMMA);
                        List<QueryParameter> fieldValueParams = new List<QueryParameter>();
                        string GUID = sourceReader["GlobalRecordId"].ToString();

                        if (destinationGUIDList.Contains(GUID) && update)
                        {
                            // UPDATE matching records
                            string updateHeader = string.Empty;
                            string whereClause = string.Empty;
                            fieldValueParams = new List<QueryParameter>();
                            StringBuilder sb = new StringBuilder();
                            int columnIndex = 0;

                            // Build the Update statement which will be reused
                            sb.Append(SqlKeyWords.UPDATE);
                            sb.Append(StringLiterals.SPACE);
                            sb.Append(destinationProjectDataDriver.InsertInEscape(destinationPage.TableName));
                            sb.Append(StringLiterals.SPACE);
                            sb.Append(SqlKeyWords.SET);
                            sb.Append(StringLiterals.SPACE);

                            updateHeader = sb.ToString();

                            sb.Remove(0, sb.ToString().Length);

                            // Build the WHERE caluse which will be reused
                            sb.Append(SqlKeyWords.WHERE);
                            sb.Append(StringLiterals.SPACE);
                            sb.Append(destinationProjectDataDriver.InsertInEscape(ColumnNames.GLOBAL_RECORD_ID));
                            sb.Append(StringLiterals.EQUAL);
                            sb.Append("'");
                            sb.Append(GUID);
                            sb.Append("'");
                            whereClause = sb.ToString();

                            sb.Remove(0, sb.ToString().Length);

                            int fieldsInQuery = 0;
                            // Now build the field update statements in 100 field chunks
                            foreach (RenderableField renderableField in sourcePage.Fields)
                            {
                                if (renderableField is GridField || renderableField is GroupField || renderableField is ImageField || fieldsToSkip.Contains(renderableField.Name)) // TODO: Someday, allow image fields
                                {
                                    continue;
                                }
                                else if (renderableField is IDataField)
                                {
                                    IDataField dataField = (IDataField)renderableField;
                                    if (dataField.FieldType != MetaFieldType.UniqueKey && dataField is RenderableField)
                                    {
                                        columnIndex += 1;

                                        //if (dataField.CurrentRecordValueObject == null)
                                        if(sourceReader[renderableField.Name] == DBNull.Value || string.IsNullOrEmpty(sourceReader[renderableField.Name].ToString()))
                                        {
                                            //sb.Append(SqlKeyWords.NULL);
                                        }
                                        else
                                        {
                                            switch (dataField.FieldType)
                                            {
                                                case MetaFieldType.Date:
                                                case MetaFieldType.DateTime:
                                                case MetaFieldType.Time:                                       
                                                    fieldValueParams.Add(new QueryParameter("@" + renderableField.Name, DbType.DateTime, Convert.ToDateTime(sourceReader[renderableField.Name])));
                                                    break;
                                                case MetaFieldType.Checkbox:
                                                    fieldValueParams.Add(new QueryParameter("@" + renderableField.Name, DbType.Boolean, Convert.ToBoolean(sourceReader[renderableField.Name])));
                                                    break;
                                                case MetaFieldType.CommentLegal:
                                                case MetaFieldType.LegalValues:
                                                case MetaFieldType.Codes:
                                                case MetaFieldType.Text:
                                                case MetaFieldType.TextUppercase:
                                                case MetaFieldType.PhoneNumber:
                                                case MetaFieldType.UniqueRowId:
                                                case MetaFieldType.ForeignKey:
                                                case MetaFieldType.GlobalRecordId:
                                                case MetaFieldType.Multiline:
                                                    fieldValueParams.Add(new QueryParameter("@" + renderableField.Name, DbType.String, sourceReader[renderableField.Name]));
                                                    break;
                                                case MetaFieldType.Number:
                                                    fieldValueParams.Add(new QueryParameter("@" + renderableField.Name, DbType.Double, sourceReader[renderableField.Name]));
                                                    break;
                                                case MetaFieldType.RecStatus:
                                                case MetaFieldType.YesNo:
                                                    fieldValueParams.Add(new QueryParameter("@" + renderableField.Name, DbType.Single, sourceReader[renderableField.Name]));
                                                    break;
                                                case MetaFieldType.Option:
                                                case MetaFieldType.Image:
                                                    this.BeginInvoke(new SetStatusDelegate(AddWarningMessage), string.Format(SharedStrings.IMPORT_ERROR_FORM_FIELD_UNSUPPORTED, renderableField.Name));
                                                    continue;
                                                default:
                                                    throw new ApplicationException(SharedStrings.IMPORT_ERROR_INVALID_TYPE);
                                            }
                                            sb.Append(StringLiterals.LEFT_SQUARE_BRACKET);
                                            sb.Append(((Epi.INamedObject)dataField).Name);
                                            sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET);
                                            sb.Append(StringLiterals.EQUAL);

                                            sb.Append(StringLiterals.COMMERCIAL_AT);
                                            sb.Append(((Epi.INamedObject)dataField).Name);
                                            sb.Append(StringLiterals.COMMA);
                                        }
                                    }

                                    if ((columnIndex % 100) == 0 && columnIndex > 0)
                                    {
                                        if (sb.ToString().LastIndexOf(StringLiterals.COMMA).Equals(sb.ToString().Length - 1))
                                        {
                                            sb.Remove(sb.ToString().LastIndexOf(StringLiterals.COMMA), 1);
                                        }

                                        Query updateQuery = destinationProjectDataDriver.CreateQuery(updateHeader + StringLiterals.SPACE + sb.ToString() + StringLiterals.SPACE + whereClause);
                                        updateQuery.Parameters = fieldValueParams;

                                        destinationProjectDataDriver.ExecuteNonQuery(updateQuery);

                                        columnIndex = 0;
                                        sb.Remove(0, sb.ToString().Length);
                                        fieldValueParams.Clear();
                                    }
                                }
                                fieldsInQuery++;
                            }

                            if (fieldsInQuery == 0)
                            {
                                continue;
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

                                Query updateQuery = destinationProjectDataDriver.CreateQuery(updateHeader + StringLiterals.SPACE + sb.ToString() + StringLiterals.SPACE + whereClause);
                                updateQuery.Parameters = fieldValueParams;

                                destinationProjectDataDriver.ExecuteNonQuery(updateQuery);

                                columnIndex = 0;
                                sb.Remove(0, sb.ToString().Length);
                                fieldValueParams.Clear();
                            }

                            recordsUpdated++;
                        }
                        else if(!destinationGUIDList.Contains(GUID) && append)
                        {
                            fieldNames.Append("GlobalRecordId");
                            fieldValues.Append("@GlobalRecordId");
                            fieldValueParams.Add(new QueryParameter("@GlobalRecordId", DbType.String, GUID));

                            int fieldsInQuery = 0;
                            // INSERT unmatched records
                            foreach (RenderableField renderableField in sourcePage.Fields)
                            {
                                if (renderableField is GridField || renderableField is GroupField || fieldsToSkip.Contains(renderableField.Name))
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
                                        if (sourceReader[renderableField.Name] == DBNull.Value || string.IsNullOrEmpty(sourceReader[renderableField.Name].ToString()))
                                        //if (dataField.CurrentRecordValueObject == null)
                                        {
                                            //fieldValues.Append(" null "); // TODO: Check to make sure we shouldn't be using this
                                        }
                                        else
                                        {
                                            String fieldName = ((Epi.INamedObject)dataField).Name;                                            
                                            //fieldValueParams.Add(dataField.CurrentRecordValueAsQueryParameter);
                                            switch (dataField.FieldType)
                                            {
                                                case MetaFieldType.Date:
                                                case MetaFieldType.DateTime:
                                                case MetaFieldType.Time:                                      
                                                    fieldValueParams.Add(new QueryParameter("@" + fieldName, DbType.DateTime, Convert.ToDateTime(sourceReader[fieldName])));
                                                    break;
                                                case MetaFieldType.Checkbox:
                                                    fieldValueParams.Add(new QueryParameter("@" + fieldName, DbType.Boolean, Convert.ToBoolean(sourceReader[fieldName])));
                                                    break;
                                                case MetaFieldType.CommentLegal:
                                                case MetaFieldType.LegalValues:
                                                case MetaFieldType.Codes:
                                                case MetaFieldType.Text:                                                        
                                                case MetaFieldType.TextUppercase:
                                                case MetaFieldType.PhoneNumber:
                                                case MetaFieldType.UniqueRowId:
                                                case MetaFieldType.ForeignKey:
                                                case MetaFieldType.GlobalRecordId:                                                        
                                                case MetaFieldType.Multiline:
                                                    fieldValueParams.Add(new QueryParameter("@" + fieldName, DbType.String, sourceReader[fieldName]));
                                                    break;
                                                case MetaFieldType.Number:
                                                    fieldValueParams.Add(new QueryParameter("@" + fieldName, DbType.Double, sourceReader[fieldName]));
                                                    break;
                                                case MetaFieldType.YesNo:
                                                case MetaFieldType.RecStatus:
                                                    fieldValueParams.Add(new QueryParameter("@" + fieldName, DbType.Single, sourceReader[fieldName]));
                                                    break;
                                                case MetaFieldType.Option:
                                                case MetaFieldType.Image:
                                                    this.BeginInvoke(new SetStatusDelegate(AddWarningMessage), string.Format(SharedStrings.IMPORT_ERROR_FORM_FIELD_UNSUPPORTED, fieldName));
                                                    continue;
                                                default:
                                                    throw new ApplicationException(SharedStrings.IMPORT_ERROR_INVALID_TYPE);
                                            }
                                            fieldNames.Append(destinationProjectDataDriver.InsertInEscape(((Epi.INamedObject)dataField).Name));
                                            fieldValues.Append("@" + fieldName);
                                        }
                                    }
                                }
                                fieldsInQuery++;
                            }

                            if (fieldsInQuery == 0)
                            {
                                continue;
                            }

                            // Concatenate the query clauses into one SQL statement.
                            StringBuilder sb = new StringBuilder();
                            sb.Append(" insert into ");
                            sb.Append(destinationProjectDataDriver.InsertInEscape(destinationPage.TableName));
                            sb.Append(StringLiterals.SPACE);
                            sb.Append(Util.InsertInParantheses(fieldNames.ToString()));
                            sb.Append(" values (");
                            sb.Append(fieldValues.ToString());
                            sb.Append(") ");
                            Query insertQuery = destinationProjectDataDriver.CreateQuery(sb.ToString());
                            insertQuery.Parameters = fieldValueParams;

                            destinationProjectDataDriver.ExecuteNonQuery(insertQuery);
                            recordsInserted++;
                        }
                        this.BeginInvoke(new SetProgressBarDelegate(IncrementProgressBarValue), 1);
                    }
                    sourceReader.Close();
                    sourceReader.Dispose();
                }
                catch (Exception ex)
                {
                    this.BeginInvoke(new SetStatusDelegate(AddErrorStatusMessage), ex.Message);
                }
                finally
                {
                }
                this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), string.Format(SharedStrings.IMPORT_DATA_PAGE_PROCESSING_COMPLETE, destinationPage.Name + "; " + string.Format(SharedStrings.IMPORT_DATA_RECS_INSERTED, recordsInserted.ToString()) + "; " + string.Format(SharedStrings.IMPORT_DATA_RECS_UPDATED, recordsUpdated.ToString()) + "."));
            }
        }

        /// <summary>
        /// Adds a status message to the status list box
        /// </summary>
        /// <param name="statusMessage"></param>
        private void AddStatusMessage(string statusMessage)
        {
            string message = DateTime.Now + ": " + statusMessage;
            lbxStatus.Items.Add(message);
            Logger.Log(message);
        }

        /// <summary>
        /// Adds a status warning message to the status list box.
        /// </summary>
        /// <param name="statusMessage"></param>
        private void AddWarningMessage(string statusMessage)
        {
            string message = DateTime.Now + ": " + SharedStrings.IMPORT_DATA_WARNING + " " + statusMessage;
            lbxStatus.Items.Add(message);            
            Logger.Log(message);
        }

        /// <summary>
        /// Adds a status error message to the status list box.
        /// </summary>
        /// <param name="statusMessage"></param>
        private void AddErrorStatusMessage(string statusMessage)
        {
            string message = DateTime.Now + ": " + SharedStrings.IMPORT_DATA_ERROR + " " + statusMessage;
            lbxStatus.Items.Add(message);
            Logger.Log(message);
        }

        /// <summary>
        /// Checks to see whether or not the two forms (source and destination) are alike enough for the import to proceed.
        /// </summary>
        /// <returns></returns>
        private bool FormsAreAlike()
        {
            int errorCount = 0;
            int warningCount = 0;

            if (sourceView.Pages.Count != destinationView.Pages.Count)
            {
                AddErrorStatusMessage(SharedStrings.IMPORT_ERROR_PAGE_COUNT_DIFFERS);
                errorCount++;
                return false;
            }

            // TODO: This works okay for the main form to be imported, but it should incorporate checks
            //   on related forms and grid fields too.

            //for (int i = 0; i < sourceView.Pages.Count; i++)
            //{
            //    if (sourceView.Pages[i].Fields.Count != destinationView.Pages[i].Fields.Count)
            //    {
            //        return false;
            //    }
            //}

            foreach (Field sourceField in sourceView.Fields)
            {
                if (destinationView.Fields.Contains(sourceField.Name))
                {
                    Field destinationField = destinationView.Fields[sourceField.Name];

                    if (destinationField.FieldType != sourceField.FieldType)
                    {
                        AddErrorStatusMessage(string.Format(SharedStrings.IMPORT_ERROR_FIELD_TYPE_DIFFERS, destinationField.Name));
                        errorCount++;
                    }
                }
                else
                {
                    AddWarningMessage(string.Format(SharedStrings.IMPORT_ERROR_SOURCE_FIELD_NOT_FOUND, sourceField.Name ));
                    warningCount++;
                }

                if (sourceField is ImageField)
                {
                    AddWarningMessage(string.Format(SharedStrings.IMPORT_ERROR_IMAGE_FIELD, sourceField.Name));                    
                }
            }

            foreach (Field destinationField in destinationView.Fields)
            {
                if (!sourceView.Fields.Contains(destinationField.Name))
                {
                    AddWarningMessage(string.Format(SharedStrings.IMPORT_ERROR_DESTINATION_FIELD_NOT_FOUND, destinationField.Name));
                    warningCount++;
                }
            }

            // sanity check, especially for projects imported from Epi Info 3, where the forms may have untold amounts of corruption and errors
            foreach (Field sourceField in sourceView.Fields)
            {
                if (!Util.IsFirstCharacterALetter(sourceField.Name))
                {
                    AddErrorStatusMessage(string.Format(SharedStrings.IMPORT_ERROR_SOURCE_FIELD_NAME_INVALID, sourceField.Name));
                    errorCount++;
                }
                if (Epi.Data.Services.AppData.Instance.IsReservedWord(sourceField.Name) && (sourceField.Name.ToLowerInvariant() != "uniquekey" && sourceField.Name.ToLowerInvariant() != "recstatus" && sourceField.Name.ToLowerInvariant() != "fkey"))
                {
                    AddWarningMessage(string.Format(SharedStrings.IMPORT_ERROR_SOURCE_FIELD_NAME_RESERVED, sourceField.Name));
                }
            }

            if (!sourceProjectDataDriver.TableExists(sourceView.TableName))
            {
                AddErrorStatusMessage(string.Format(SharedStrings.DATA_TABLE_NOT_FOUND, sourceView.Name));
                errorCount++;
            }

            if (warningCount > ((double)destinationView.Fields.Count / 1.7)) // User may have selected to import the wrong form with this many differences?
            {
                AddErrorStatusMessage(SharedStrings.IMPORT_ERROR_TOO_DIFFERENT);
                return false;
            }

            if (errorCount >= 1)
            {
                AddErrorStatusMessage(string.Format(SharedStrings.IMPORT_ERROR_COUNT, errorCount.ToString()));
                return false;
            }

            return true;
        }
        #endregion // Private Methods

        #region Event Handlers
        private void rbtnSingleImport_CheckedChanged(object sender, EventArgs e)
        {
            //if (rbtnBatchImport.Checked)
            //{
            //    IsBatchImport = true;
            //}
            //else if (rbtnSingleImport.Checked)
            //{
            //    IsBatchImport = false;
            //}
            IsBatchImport = false;

            //if (IsBatchImport)
            //{
            //    textProjectFile.Text = "Directory to recursively search for project files:";
            //}
            //else
            //{
            //    textProjectFile.Text = "Project containing the data to import:";
            //}
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = SharedStrings.SELECT_DATA_SOURCE;
            openFileDialog.Filter = "Epi Info " + SharedStrings.PROJECT_FILE + " (*.prj)|*.prj";

            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;

            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName.Trim();
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        sourceProject = new Project(filePath);
                        textProjectFile.Text = filePath;
                        cmbFormName.Items.Clear();
                        foreach (View sourceView in sourceProject.Views)
                        {
                            cmbFormName.Items.Add(sourceView.Name);
                        }

                        if (cmbFormName.Items.Contains(destinationView.Name))
                        {
                            cmbFormName.SelectedIndex = cmbFormName.Items.IndexOf(destinationView.Name);
                        }

                        sourceProjectDataDriver = DBReadExecute.GetDataDriver(sourceProject.FilePath);                        
                    }
                    catch (Exception ex)
                    {
                        Epi.Windows.MsgBox.ShowError(SharedStrings.ERROR_LOADING_PROJECT, ex);
                        textProjectFile.Text = string.Empty;
                        cmbFormName.Items.Clear();
                        return;
                    }
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DoImport();
        }

        /// <summary>
        /// Handles the WorkerCompleted event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            stopwatch.Stop();

            this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), SharedStrings.IMPORT_DATA_COMPLETE + "; " + SharedStrings.IMPORT_DATA_TIME_ELAPSED + " " + stopwatch.Elapsed.ToString());
            this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), SharedStrings.IMPORT_DATA_COMPLETE + "; " + SharedStrings.IMPORT_DATA_TIME_ELAPSED + " " + stopwatch.Elapsed.ToString());

            btnBrowse.Enabled = true;
            btnCancel.Enabled = true;
            btnOK.Enabled = true;
            cmbFormName.Enabled = true;
            rdbUpdateAndAppend.Enabled = true;
            rdbUpdate.Enabled = true;
            rdbAppend.Enabled = true;
            textProjectFile.Enabled = true;
            progressBar.Visible = false;

            importFinished = true;

            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Handles the DoWorker event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                List<View> viewsToProcess = (List<View>)e.Argument;
                Query selectQuery = destinationProjectDataDriver.CreateQuery("SELECT [GlobalRecordId] FROM [" + destinationView.TableName + "]");
                IDataReader destReader = destinationProjectDataDriver.ExecuteReader(selectQuery);
                List<string> destinationGUIDList = new List<string>();
                while (destReader.Read())
                {
                    destinationGUIDList.Add(destReader[0].ToString());
                }
                ProcessBaseTable(sourceView, destinationView, destinationGUIDList);
                ProcessPages(sourceView, destinationView, destinationGUIDList);
                ProcessGridFields(sourceView, destinationView);
                ProcessRelatedForms(sourceView, destinationView, viewsToProcess);
            }
        }

        /// <summary>
        /// Checks for problems
        /// </summary>
        private bool CheckForProblems()
        {
            try
            {
                string formName = cmbFormName.SelectedItem.ToString();

                View sourceView = sourceProject.Views[formName];
                IDbDriver driver = sourceProject.CollectedData.GetDatabase();

                // Check #1 - Make sure the base table exists and that it has a Global Record Id field, Record status field, and Unique key field.
                DataTable dt = driver.GetTableData(sourceView.TableName, "GlobalRecordId, RECSTATUS, UniqueKey");
                int baseTableRowCount = dt.Rows.Count;

                // Check #2a - Make sure GlobalRecordId is a string.
                if (!dt.Columns[0].DataType.ToString().Equals("System.String"))
                {
                    throw new ApplicationException(SharedStrings.IMPORT_ERROR_ID_INVALID);
                }

                // Check #2b - Make sure RECSTATUS is a number
                if (!(dt.Columns[1].DataType.ToString().Equals("System.Byte") || dt.Columns[1].DataType.ToString().Equals("System.Int16") || dt.Columns[1].DataType.ToString().Equals("System.Int32")))
                {
                    throw new ApplicationException(SharedStrings.IMPORT_ERROR_RECSTATUS_INVALID);
                }

                // Check #3 - Make sure GlobalRecordId values haven't been replaced with something that isn't actually a GUID. 
                //      For performance reasons only the first few values are checked.
                if (baseTableRowCount >= 1)
                {
                    string value = dt.Rows[0][0].ToString();
                    System.Guid guid = new Guid(value);

                    if (baseTableRowCount >= 30)
                    {
                        for (int i = 0; i < 30; i++)
                        {
                            value = dt.Rows[i][0].ToString();
                            guid = new Guid(value);
                        }
                    }
                }

                // Check #4a - See if global record ID values are distinct on the base table.
                Query selectDistinctQuery = driver.CreateQuery("SELECT DISTINCT [GlobalRecordId] FROM [" + sourceView.TableName + "]");
                DataTable distinctTable = driver.Select(selectDistinctQuery);
                if (distinctTable.Rows.Count != baseTableRowCount)
                {
                    throw new ApplicationException(SharedStrings.IMPORT_ERROR_ID_NOT_UNIQUE);
                }

                // Check #4b - See if global record ID values are distinct on each page table.
                foreach (Page page in sourceView.Pages)
                {
                    selectDistinctQuery = driver.CreateQuery("SELECT DISTINCT [GlobalRecordId] FROM [" + page.TableName + "]");
                    distinctTable = driver.Select(selectDistinctQuery);
                    if (distinctTable.Rows.Count != baseTableRowCount)
                    {
                        throw new ApplicationException(string.Format(SharedStrings.IMPORT_ERROR_PAGE_ID_NOT_UNIQUE, page.TableName ));
                    }
                }

                // Check #5 - Make sure RECSTATUS has valid values.
                selectDistinctQuery = driver.CreateQuery("SELECT DISTINCT [RecStatus] FROM [" + sourceView.TableName + "]");
                distinctTable = driver.Select(selectDistinctQuery);
                foreach (DataRow row in distinctTable.Rows)
                {
                    if (!row[0].ToString().Equals("1") && !row[0].ToString().Equals("0"))
                    {
                        throw new ApplicationException(SharedStrings.IMPORT_ERROR_RECSTATUS_INVALID_DATA);
                    }
                }

                // Check #6 - Make sure nobody has used a form's table as a code table for drop-down lists
                List<string> dataTableNames = new List<string>();
                foreach (View view in sourceProject.Views)
                {
                    dataTableNames.Add(view.TableName);
                    foreach (Page page in view.Pages)
                    {
                        dataTableNames.Add(page.TableName);
                    }
                }
                DataTable codeTableNames = driver.GetCodeTableNamesForProject(sourceProject);
                foreach (DataRow row in codeTableNames.Rows)
                {
                    if (codeTableNames.Columns.Count >= 2)
                    {
                        if (dataTableNames.Contains(row[2].ToString()))
                        {
                            throw new ApplicationException(SharedStrings.IMPORT_ERROR_CODETABLE_AS_FORM);
                        }
                    }
                    else
                    {
                        if (dataTableNames.Contains(row[0].ToString()))
                        {
                            throw new ApplicationException(SharedStrings.IMPORT_ERROR_CODETABLE_AS_FORM);
                        }
                    }
                }

                // Check #7 - Should never get here because the UI should prevent it, but do a check just in case
                if (sourceView.IsRelatedView == true)
                {
                    throw new ApplicationException(SharedStrings.IMPORT_ERROR_RELATED_SOURCE);
                }

                distinctTable = null;
                selectDistinctQuery = null;
                driver.Dispose();
                driver = null;
            }
            catch (Exception ex)
            {
                AddErrorStatusMessage(ex.Message);
                return false;
            }

            return true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (importFinished)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            else
            {
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            }
            this.Close();
        }

        private void ImportDataForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (importWorker.IsBusy)
            {
                DialogResult result = Epi.Windows.MsgBox.ShowQuestion(SharedStrings.IMPORT_DATA_CANCEL_IMPORT);
                if (result == DialogResult.Yes)
                {
                    importWorker.CancelAsync();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void cmbFormName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbFormName.SelectedIndex == -1)
            {
                btnOK.Enabled = false;
            }
            else
            {
                btnOK.Enabled = true;
            }
        }

        private void ImportDataForm_Load(object sender, EventArgs e)
        {
            AddStatusMessage(SharedStrings.IMPORT_DATA_READY);
        }

        private void cmsStatus_Click(object sender, EventArgs e)
        {
            if (lbxStatus.Items.Count > 0)
            {
                string StatusText = string.Empty;
                foreach (string item in lbxStatus.Items)
                {
                    StatusText = StatusText + System.Environment.NewLine + item;
                }
                Clipboard.SetText(StatusText);
            }

        }
        #endregion // Event Handlers
    }
}
