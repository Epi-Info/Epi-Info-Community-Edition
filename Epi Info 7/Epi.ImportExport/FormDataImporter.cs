#region Using
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Epi;
using Epi.Core;
using Epi.Fields;
using Epi.Data;
#endregion // Using

namespace Epi.ImportExport
{
    /// <summary>
    /// A class used to handle importing (merging) data from another Epi Info 7 form.
    /// -- E. Knudsen, 2012
    /// </summary>
    public class FormDataImporter : IDisposable
    {
        #region Private Members
        private Project sourceProject;
        private Project destinationProject;
        private View sourceView;
        private View destinationView;
        private IDbDriver sourceProjectDataDriver;
        private IDbDriver destinationProjectDataDriver;
        private bool update = false;
        private bool append = true;
        private List<View> formsToProcess;
        private Query selectQuery;
        private Dictionary<string, List<string>> columnsToNull;
        private Dictionary<string, List<string>> gridColumnsToNull;
        private List<string> sourceGUIDs;
        private List<string> optionFieldsAsStrings; // option fields prior to certain updates may be string or Int16.
        private const double DIFF_TOLERANCE = 1.7;
        #endregion // Private Members

        #region Events
        public event SetMaxProgressBarValueDelegate SetMaxProgressBarValue;
        public event SetProgressBarDelegate SetProgressBar;
        public event UpdateStatusEventHandler SetStatus;
        public event UpdateStatusEventHandler AddStatusMessage;
        public event CheckForCancellationHandler CheckForCancellation;
        #endregion // Events

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public FormDataImporter()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public FormDataImporter(Project sourceProject, Project destinationProject, View destinationView, List<View> viewsToProcess)
        {
            this.formsToProcess = viewsToProcess;
            this.sourceProject = sourceProject;
            this.destinationProject = destinationProject;
            this.sourceView = sourceProject.Views[destinationView.Name];
            this.destinationView = destinationView;
            Construct();
        }
        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Gets/sets whether to update existing records
        /// </summary>
        public bool Update
        {
            get
            {
                return this.update;
            }
            set
            {
                update = value;
            }
        }

        /// <summary>
        /// Gets/sets whether to append unmatched records
        /// </summary>
        public bool Append
        {
            get
            {
                return this.append;
            }
            set
            {
                append = value;
            }
        }

        /// <summary>
        /// Sets the columns to null;
        /// </summary>
        public Dictionary<string, List<string>> ColumnsToNull
        {
            get
            {
                return this.columnsToNull;
            }
            set
            {
                this.columnsToNull = value;
            }
        }

        /// <summary>
        /// Sets the grid columns to null;
        /// </summary>
        public Dictionary<string, List<string>> GridColumnsToNull
        {
            get
            {
                return this.gridColumnsToNull;
            }
            set
            {
                this.gridColumnsToNull = value;
            }
        }

        /// <summary>
        /// Gets/sets the select query used to filter records during the copying process.
        /// </summary>
        public Query SelectQuery
        {
            get
            {
                return this.selectQuery;
            }
            set
            {
                if (!value.SqlStatement.ToLowerInvariant().Trim().StartsWith("select"))
                {
                    throw new ArgumentException(ImportExportSharedStrings.ERROR_INVALID_SELECT_QUERY);
                }
                else
                {
                    this.selectQuery = value;
                }
            }
        }
        #endregion Public Properties

        #region Public Methods
        /// <summary>
        /// Releases all resources used by the form data importer
        /// </summary>
        public void Dispose() // Implements IDisposable.Dispose
        {
            SetProgressBar = null;
            SetStatus = null;
            AddStatusMessage = null;

            if (formsToProcess != null)
            {
                formsToProcess.Clear();
                formsToProcess = null;
            }
            if (columnsToNull != null)
            {
                columnsToNull.Clear();
                columnsToNull = null;
            }
            if (gridColumnsToNull != null)
            {
                gridColumnsToNull.Clear();
                gridColumnsToNull = null;
            }
            if (selectQuery != null)
            {
                selectQuery = null;
            }
            if (sourceGUIDs != null)
            {
                sourceGUIDs.Clear();
                sourceGUIDs = null;
            }

            destinationProjectDataDriver = null;
            destinationProject = null;
            destinationView = null;      

            sourceProjectDataDriver.Dispose();
            sourceProjectDataDriver = null;
            sourceProject.Dispose();
            sourceProject = null;
            sourceView.Dispose();
            sourceView = null;

            GC.Collect();
        }

        /// <summary>
        /// Imports data from one project to another
        /// </summary>
        public void ImportFormData()
        {
            Query destinationSelectQuery = destinationProjectDataDriver.CreateQuery("SELECT [GlobalRecordId] FROM [" + destinationView.TableName + "]");
            IDataReader destReader = destinationProjectDataDriver.ExecuteReader(destinationSelectQuery);
            List<string> destinationGUIDList = new List<string>();
            while (destReader.Read())
            {
                destinationGUIDList.Add(destReader[0].ToString());
            }

            destReader.Close();
            destReader.Dispose();
            destReader = null;

            PopulateSourceGUIDs();

            int maxProgress = 100;

            if (SelectQuery == null)
            {
                int recordCount = sourceView.GetRecordCount();
                int gridRowCount = 0;

                foreach (GridField gridField in sourceView.Fields.GridFields)
                {
                    IDataReader reader = sourceProjectDataDriver.GetTableDataReader(gridField.TableName);
                    while (reader.Read())
                    {
                        gridRowCount++;
                    }
                    reader.Close();
                    reader.Dispose();
                    reader = null;
                }

                maxProgress = recordCount * (sourceView.Pages.Count + 1);
                maxProgress = maxProgress + gridRowCount;

                foreach (View form in formsToProcess)
                {
                    maxProgress = maxProgress + (form.GetRecordCount() * (form.Pages.Count + 1));

                    foreach (GridField gridField in form.Fields.GridFields)
                    {
                        IDataReader reader = sourceProjectDataDriver.GetTableDataReader(gridField.TableName);
                        while (reader.Read())
                        {
                            gridRowCount++;
                        }
                        reader.Close();
                        reader.Dispose();
                    }

                    maxProgress = maxProgress + gridRowCount;
                }
            }
            else
            {
                // This is only a rough estimate
                int recordCount = sourceGUIDs.Count;
                maxProgress = recordCount * (sourceView.Pages.Count + 1);
                OnSetStatusMessage(string.Format(ImportExportSharedStrings.ROW_FILTERS_IN_EFFECT, recordCount.ToString()));
            }

            CheckIfFormsAreAlike();
            
            OnSetMaxProgressValue(maxProgress);

            ProcessBaseTable(sourceView, destinationView, destinationGUIDList);
            ProcessPages(sourceView, destinationView, destinationGUIDList);
            ProcessGridFields(sourceView, destinationView);
            ProcessRelatedForms(sourceView, destinationView, formsToProcess);
        }
        #endregion // Public Methods

        #region Private Methods
        /// <summary>
        /// Checks to see if two given descendant forms are alike
        /// </summary>
        private void CheckIfDescendantFormsAreAlike(View sourceChildForm, View destChildForm)
        {
            //OnAddStatusMessage(string.Format(ImportExportSharedStrings.IMPORT_CHECK_DESCENDANT_FORM_START, sourceChildForm.Name));

            int warningCount = 0;

            if (sourceChildForm.Pages.Count != destChildForm.Pages.Count)
            {
                throw new ApplicationException(string.Format(ImportExportSharedStrings.ERROR_DESCENDANT_PAGE_COUNT_DIFFERENT, sourceChildForm.Name));
            }

            if (!string.IsNullOrEmpty(destChildForm.TableName) && destinationProjectDataDriver.TableExists(destChildForm.TableName))
            {
                if (!sourceChildForm.TableName.Equals(destChildForm.TableName))
                {
                    throw new ApplicationException(string.Format(ImportExportSharedStrings.ERROR_DESCENDANT_FORM_TABLE_NAMES_DIFFER, sourceChildForm.Name));
                }
            }

            foreach (Field sourceField in sourceChildForm.Fields)
            {
                if (destChildForm.Fields.Contains(sourceField.Name))
                {
                    Field destinationField = destChildForm.Fields[sourceField.Name];

                    if (destinationField.FieldType != sourceField.FieldType)
                    {
                        throw new ApplicationException(string.Format(ImportExportSharedStrings.ERROR_DESCENDANT_FIELD_MISMATCH_DEST, destinationField.Name, sourceChildForm.Name));
                    }
                    else
                    {
                        if (destinationField is IDataField && destinationField is RenderableField && sourceField is IDataField && sourceField is RenderableField)
                        {
                            RenderableField rfDstField = destinationField as RenderableField;
                            RenderableField rfSrcField = sourceField as RenderableField;

                            if (rfDstField == null || rfSrcField == null)
                            {
                                throw new InvalidOperationException("null fields detected in CheckIfDescendantFormsAreAlike");
                            }

                            if (rfDstField.Page.Position != rfSrcField.Page.Position)
                            {
                                throw new ApplicationException(string.Format(ImportExportSharedStrings.ERROR_DESCENDANT_FIELD_PAGE_ORDER_MISMATCH, rfSrcField.Name, sourceChildForm.Name));
                            }
                        }
                        if (destinationField is GridField && sourceField is GridField)
                        {
                            GridField destinationGridField = destinationField as GridField;
                            GridField sourceGridField = sourceField as GridField;

                            if (destinationGridField == null || sourceGridField == null)
                            {
                                throw new InvalidOperationException("null fields detected in CheckIfDescendantFormsAreAlike");
                            }

                            CheckIfGridsAreAlike(destinationGridField, sourceGridField);
                        }
                    }
                }
                else
                {
                    OnAddStatusMessage(string.Format(ImportExportSharedStrings.ERROR_DESCENDANT_FIELD_NOT_FOUND, sourceField.Name, sourceChildForm.Name));
                    warningCount++;
                }

                if (sourceField is ImageField)
                {
                    OnAddStatusMessage(string.Format(ImportExportSharedStrings.ERROR_DESCENDANT_IMAGE_FIELD, sourceField.Name, sourceChildForm.Name));
                }
            }

            foreach (Field destinationField in destChildForm.Fields)
            {
                if (!sourceChildForm.Fields.Contains(destinationField.Name))
                {
                    OnAddStatusMessage(string.Format(ImportExportSharedStrings.ERROR_DESCENDANT_FIELD_NOT_FOUND_SOURCE, destinationField.Name, sourceChildForm.Name));
                    warningCount++;
                }
            }

            // sanity check, especially for projects imported from Epi Info 3, where the forms may have untold amounts of corruption and errors
            foreach (Field sourceField in sourceChildForm.Fields)
            {
                if (!Util.IsFirstCharacterALetter(sourceField.Name))
                {
                    throw new ApplicationException(string.Format(ImportExportSharedStrings.ERROR_FIELD_NAME_INVALID, sourceField.Name));
                    //errorCount++;
                }
                if (Epi.Data.Services.AppData.Instance.IsReservedWord(sourceField.Name) && (sourceField.Name.ToLowerInvariant() != "uniquekey" && sourceField.Name.ToLowerInvariant() != "recstatus" && sourceField.Name.ToLowerInvariant() != "fkey"))
                {
                    //AddWarningMessage("The field name for " + sourceField.Name + " in the source form is a reserved word. Problems may be encountered during the import.");
                    throw new ApplicationException(string.Format(ImportExportSharedStrings.ERROR_FIELD_NAME_RESERVED_WORD, sourceField.Name));
                }
            }

            if (!sourceProjectDataDriver.TableExists(sourceChildForm.TableName))
            {
                throw new ApplicationException(string.Format(SharedStrings.DATA_TABLE_NOT_FOUND, sourceChildForm.Name));
            }

            if (warningCount > ((double)destChildForm.Fields.Count / DIFF_TOLERANCE)) // User may have selected to import the wrong form with this many differences?
            {
                throw new ApplicationException(ImportExportSharedStrings.ERROR_TOO_MANY_DIFFERENCES);
            }

            //OnAddStatusMessage(string.Format(ImportExportSharedStrings.IMPORT_CHECK_DESCENDANT_FORM_END, sourceChildForm.Name));
        }

        /// <summary>
        /// Checks to see if two given grid fields are alike
        /// </summary>
        private void CheckIfGridsAreAlike(GridField sourceGridField, GridField destGridField)
        {
            #region Input Validation
            if (sourceGridField == null) throw new ArgumentNullException("sourceGridField");
            if (destGridField == null) throw new ArgumentNullException("destGridField");
            #endregion // Input Validation

            foreach (GridColumnBase dgc in destGridField.Columns)
            {
                bool foundInSource = false;
                foreach (GridColumnBase sgc in sourceGridField.Columns)
                {
                    if (dgc.Name == sgc.Name)
                    {
                        if (dgc.GridColumnType != sgc.GridColumnType)
                        {
                            throw new ApplicationException(string.Format(ImportExportSharedStrings.ERROR_GRID_COLUMN_MISMATCH, destGridField.Name, dgc.Name, dgc.GridColumnType.ToString(), sgc.GridColumnType.ToString()));
                        }
                        else
                        {
                            foundInSource = true;
                            break;
                        }
                    }
                }

                if (!foundInSource)
                {
                    OnAddStatusMessage(string.Format(ImportExportSharedStrings.ERROR_GRID_COLUMN_MISSING, dgc.Name, destGridField.Name));
                }
            }

            //OnAddStatusMessage(string.Format(ImportExportSharedStrings.IMPORT_CHECK_GRID_END, sourceGridField.Name));
        }

        /// <summary>
        /// Checks to see whether or not the two forms (source and destination) are alike enough for the import to proceed.
        /// </summary>        
        private void CheckIfFormsAreAlike()
        {
            int warningCount = 0;

            OnAddStatusMessage(string.Format(ImportExportSharedStrings.IMPORT_CHECK_FORM_START, sourceView.Name));

            if (sourceView.Pages.Count != destinationView.Pages.Count)
            {
                throw new ApplicationException(ImportExportSharedStrings.ERROR_PAGE_COUNT_DIFFERENT);
            }

            if (!sourceProject.Views.Contains(destinationView.Name))
            {
                throw new ApplicationException(ImportExportSharedStrings.ERROR_FORM_NAMES_DIFFER);
            }

            foreach (View otherSourceView in sourceProject.Views)
            {
                if (ImportExportHelper.IsFormDescendant(otherSourceView, sourceView) && otherSourceView != sourceView)
                {
                    // the view is a descendant form
                    if (destinationProject.Views.Contains(otherSourceView.Name))
                    {
                        CheckIfDescendantFormsAreAlike(otherSourceView, destinationProject.Views[otherSourceView.Name]);
                    }
                    else
                    {
                        throw new ApplicationException(ImportExportSharedStrings.ERROR_DESCENDANT_FORM_NAMES_DIFFER);
                    }
                }
            }

            if (!String.IsNullOrEmpty(destinationView.TableName) && destinationProjectDataDriver.TableExists(destinationView.TableName))
            {
                if (!sourceView.TableName.Equals(destinationView.TableName))
                {
                    throw new ApplicationException(ImportExportSharedStrings.ERROR_FORM_TABLE_NAMES_DIFFER);
                }
            }

            foreach (Field sourceField in sourceView.Fields)
            {
                if (destinationView.Fields.Contains(sourceField.Name))
                {
                    Field destinationField = destinationView.Fields[sourceField.Name];

                    if (destinationField.FieldType != sourceField.FieldType)
                    {
                        throw new ApplicationException(String.Format(ImportExportSharedStrings.ERROR_FIELD_MISMATCH_DEST, destinationField.Name));
                    }
                    else
                    {
                        if (destinationField is IDataField && destinationField is RenderableField && sourceField is IDataField && sourceField is RenderableField)
                        {
                            RenderableField rfDstField = destinationField as RenderableField;
                            RenderableField rfSrcField = sourceField as RenderableField;

                            if (rfDstField.Page.Position != rfSrcField.Page.Position)
                            {
                                throw new ApplicationException(String.Format(ImportExportSharedStrings.ERROR_FIELD_PAGE_ORDER_MISMATCH, rfSrcField.Name));
                            }
                        }
                        if (destinationField is GridField && sourceField is GridField)
                        {
                            CheckIfGridsAreAlike(destinationField as GridField, sourceField as GridField);
                        }
                    }
                }
                else
                {
                    OnAddStatusMessage(string.Format(ImportExportSharedStrings.ERROR_FIELD_NOT_FOUND, sourceField.Name));                        
                    warningCount++;
                }

                if (sourceField is ImageField)
                {
                    OnAddStatusMessage(string.Format(ImportExportSharedStrings.ERROR_IMAGE_FIELD, sourceField.Name));
                }
            }

            foreach (Field destinationField in destinationView.Fields)
            {
                if (!sourceView.Fields.Contains(destinationField.Name))
                {
                    OnAddStatusMessage(string.Format(ImportExportSharedStrings.ERROR_FIELD_NOT_FOUND_SOURCE, destinationField.Name));
                    warningCount++;
                }
            }

            // sanity check, especially for projects imported from Epi Info 3, where the forms may have untold amounts of corruption and errors
            foreach (Field sourceField in sourceView.Fields)
            {
                if (!Util.IsFirstCharacterALetter(sourceField.Name))
                {
                    throw new ApplicationException(string.Format(ImportExportSharedStrings.ERROR_FIELD_NAME_INVALID, sourceField.Name));
                    //errorCount++;
                }
                if (Epi.Data.Services.AppData.Instance.IsReservedWord(sourceField.Name) && (sourceField.Name.ToLowerInvariant() != "uniquekey" && sourceField.Name.ToLowerInvariant() != "recstatus" && sourceField.Name.ToLowerInvariant() != "fkey"))
                {
                    //AddWarningMessage("The field name for " + sourceField.Name + " in the source form is a reserved word. Problems may be encountered during the import.");
                    throw new ApplicationException(string.Format(ImportExportSharedStrings.ERROR_FIELD_NAME_RESERVED_WORD, sourceField.Name));
                }
            }

            if (!sourceProjectDataDriver.TableExists(sourceView.TableName))
            {
                throw new ApplicationException(string.Format(SharedStrings.DATA_TABLE_NOT_FOUND, sourceView.Name));
            }

            if (warningCount > ((double)destinationView.Fields.Count / DIFF_TOLERANCE)) // User may have selected to import the wrong form with this many differences?
            {
                throw new ApplicationException(ImportExportSharedStrings.ERROR_TOO_MANY_DIFFERENCES);
            }

            OnAddStatusMessage(string.Format(ImportExportSharedStrings.IMPORT_CHECK_FORM_END, sourceView.Name));
        }

        /// <summary>
        /// Constructs the object
        /// </summary>
        private void Construct() 
        {
            if (destinationProject != null && sourceProject != null)
            {
                this.destinationProjectDataDriver = destinationProject.CollectedData.GetDbDriver();
                this.sourceProjectDataDriver = sourceProject.CollectedData.GetDbDriver();
            }

            optionFieldsAsStrings = new List<string>();
        }

        /// <summary>
        /// Populates the list of source GUIDs that should be used to do the import.
        /// </summary>
        private void PopulateSourceGUIDs()
        {
            sourceGUIDs = new List<string>();

            if (SelectQuery != null)
            {
                IDataReader reader = sourceProjectDataDriver.ExecuteReader(SelectQuery);
                while (reader.Read())
                {
                    sourceGUIDs.Add(reader[0].ToString());
                }

                reader.Close();
                reader.Dispose();
            }
            else
            {
                sourceGUIDs = null;
            }
        }

        /// <summary>
        /// Populates the list of source GUIDs that should be used to do the import on a specific view.
        /// </summary>
        /// <param name="relatedView">The related view to process</param>
        private void PopulateSourceGUIDs(View relatedView)
        {
            #region Input Validation
            if (!relatedView.IsRelatedView)
            {
                throw new ArgumentException(ImportExportSharedStrings.ERROR_FORM_NOT_DESCENDENT);
            }
            #endregion // Input Validation

            if (SelectQuery == null)
            {
                sourceGUIDs = null;
                return;
            }

            View parentView = relatedView.ParentView;
            List<string> parentGUIDs = new List<string>();

            sourceGUIDs = new List<string>();
            Query query = destinationProjectDataDriver.CreateQuery("SELECT [GlobalRecordId] FROM [" + parentView.TableName + "]");
            IDataReader parentReader = null;

            if (parentView.Name == destinationView.Name)
            {
                parentReader = sourceProjectDataDriver.ExecuteReader(SelectQuery);
            }
            else
            {
                parentReader = destinationProjectDataDriver.ExecuteReader(query);
            }

            while (parentReader.Read())
            {
                parentGUIDs.Add(parentReader[0].ToString());
            }

            parentReader.Close();
            parentReader.Dispose();
            parentReader = null;

            foreach (string GUID in parentGUIDs)
            {
                Query childQuery = sourceProjectDataDriver.CreateQuery("SELECT [FKEY], [GlobalRecordId] FROM [" + relatedView.TableName + "] WHERE [FKEY] = @FKEY");
                childQuery.Parameters.Add(new QueryParameter("@FKEY", DbType.String, GUID));
                IDataReader reader = sourceProjectDataDriver.ExecuteReader(childQuery);
                while (reader.Read())
                {
                    string FKEY = reader[0].ToString();
                    string childGUID = reader[1].ToString();
                    sourceGUIDs.Add(childGUID);
                }

                reader.Close();
                reader.Dispose();
                reader = null;
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
            OnAddStatusMessage(ImportExportSharedStrings.PROCESSING_BASE_TABLE);

            int recordsInserted = 0;
            int recordsUpdated = 0;

            string sourceTable = sourceView.TableName;
            string destinationTable = destinationView.TableName;

            optionFieldsAsStrings = new List<string>();
            // Check for string-based option fields.
            foreach (Field f in destinationView.Fields)
            {
                if (f is OptionField)
                {
                    OptionField optionField = f as OptionField;
                    if (optionField != null)
                    {
                        DataTable dt = destinationProjectDataDriver.GetTopTwoTable(optionField.Page.TableName);
                        if (dt.Columns[optionField.Name].DataType.ToString().Equals("System.String", StringComparison.OrdinalIgnoreCase))
                        {
                            optionFieldsAsStrings.Add(f.Name);
                        }
                    }
                }
            }

            try
            {
                List<string> newGUIDList = new List<string>();
                IDataReader sourceReader = sourceProjectDataDriver.GetTableDataReader(sourceView.TableName);
                while (sourceReader.Read())
                {
                    object recordStatus = sourceReader["RECSTATUS"];
                    QueryParameter paramRecordStatus = new QueryParameter("@RECSTATUS", DbType.Int32, recordStatus);

                    //if (importWorker.CancellationPending)
                    //{
                    //    this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), "Import cancelled.");
                    //    return;
                    //}

                    if (OnCheckForCancellation())
                    {
                        OnAddStatusMessage(ImportExportSharedStrings.IMPORT_CANCELLED);
                        sourceReader.Close();
                        sourceReader.Dispose();
                        sourceReader = null;
                        return;
                    }

                    WordBuilder fieldNames = new WordBuilder(StringLiterals.COMMA);
                    WordBuilder fieldValues = new WordBuilder(StringLiterals.COMMA);
                    List<QueryParameter> fieldValueParams = new List<QueryParameter>();

                    string GUID = sourceReader["GlobalRecordId"].ToString();

                    if (sourceGUIDs != null && !sourceGUIDs.Contains(GUID))
                    {
                        continue;
                    }

                    fieldNames.Append("GlobalRecordId");
                    fieldValues.Append("@GlobalRecordId");
                    
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
                    OnSetProgress(1);                    
                }
                sourceReader.Close();
                sourceReader.Dispose();
                sourceReader = null;
            }
            catch (Exception ex)
            {                
                OnAddStatusMessage(string.Format(ImportExportSharedStrings.ERROR_WITH_MESSAGE, ex.Message));
            }
            finally
            {
            }

            if (update && append)
            {
                OnAddStatusMessage(string.Format(ImportExportSharedStrings.IMPORT_TABLE_UPDATED_AND_APPENDED, destinationTable, recordsInserted.ToString(), recordsUpdated.ToString()));
            }
            else if (update && !append)
            {
                OnAddStatusMessage(string.Format(ImportExportSharedStrings.IMPORT_TABLE_UPDATED, destinationTable, recordsUpdated.ToString()));
            }
            else if (!update && append)
            {
                OnAddStatusMessage(string.Format(ImportExportSharedStrings.IMPORT_TABLE_APPENDED, destinationTable, recordsInserted.ToString()));
            }
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
                OnAddStatusMessage(string.Format(ImportExportSharedStrings.PROCESSING_PAGE, (i + 1).ToString(), sourceView.Pages.Count.ToString()));

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

                    if (ColumnsToNull != null && ColumnsToNull.ContainsKey(sourceView.Name))
                    {
                        List<string> toNull = ColumnsToNull[sourceView.Name];

                        foreach (string s in toNull)
                        {
                            if (!fieldsToSkip.Contains(s))
                            {
                                fieldsToSkip.Add(s);
                            }
                        }
                    }

                    IDataReader sourceReader = sourceProjectDataDriver.GetTableDataReader(sourcePage.TableName);
                    while (sourceReader.Read())
                    {
                        //if (importWorker.CancellationPending)
                        //{
                        //    this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), "Import cancelled.");
                        //    return;
                        //}
                        if (OnCheckForCancellation())
                        {
                            OnAddStatusMessage(ImportExportSharedStrings.IMPORT_CANCELLED);
                            sourceReader.Close();
                            sourceReader.Dispose();
                            return;
                        }

                        WordBuilder fieldNames = new WordBuilder(StringLiterals.COMMA);
                        WordBuilder fieldValues = new WordBuilder(StringLiterals.COMMA);
                        List<QueryParameter> fieldValueParams = new List<QueryParameter>();
                        string GUID = sourceReader["GlobalRecordId"].ToString();

                        if (sourceGUIDs != null && !sourceGUIDs.Contains(GUID))
                        {
                            continue;
                        }

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
                                        if (sourceReader[renderableField.Name] == DBNull.Value || string.IsNullOrEmpty(sourceReader[renderableField.Name].ToString()))
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
                                                case MetaFieldType.GUID:
                                                    fieldValueParams.Add(new QueryParameter("@" + renderableField.Name, DbType.Guid, sourceReader[renderableField.Name]));
                                                    break;
                                                case MetaFieldType.Option:
                                                    if (optionFieldsAsStrings.Contains(renderableField.Name))
                                                    {
                                                        fieldValueParams.Add(new QueryParameter("@" + renderableField.Name, DbType.String, sourceReader[renderableField.Name]));
                                                    }
                                                    else
                                                    {
                                                        fieldValueParams.Add(new QueryParameter("@" + renderableField.Name, DbType.Int16, sourceReader[renderableField.Name]));
                                                    }
                                                    break;
                                                case MetaFieldType.Image:                                                    
                                                    OnAddStatusMessage(string.Format(ImportExportSharedStrings.WARNING_FIELD_NOT_IMPORTED, renderableField.Name));
                                                    continue;
                                                default:
                                                    throw new ApplicationException(ImportExportSharedStrings.UNRECOGNIZED_FIELD_TYPE);
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
                        else if (!destinationGUIDList.Contains(GUID) && append)
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
                                                case MetaFieldType.GUID:
                                                    fieldValueParams.Add(new QueryParameter("@" + fieldName, DbType.Guid, sourceReader[fieldName]));
                                                    break;
                                                case MetaFieldType.Option:
                                                    if (optionFieldsAsStrings.Contains(renderableField.Name))
                                                    {
                                                        fieldValueParams.Add(new QueryParameter("@" + renderableField.Name, DbType.String, sourceReader[fieldName]));
                                                    }
                                                    else
                                                    {
                                                        fieldValueParams.Add(new QueryParameter("@" + renderableField.Name, DbType.Int16, sourceReader[fieldName]));
                                                    }
                                                    break;
                                                case MetaFieldType.Image:
                                                    OnAddStatusMessage(string.Format(ImportExportSharedStrings.WARNING_FIELD_NOT_IMPORTED, renderableField.Name));
                                                    continue;
                                                default:
                                                    throw new ApplicationException(ImportExportSharedStrings.UNRECOGNIZED_FIELD_TYPE);
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
                        //this.BeginInvoke(new SetProgressBarDelegate(IncrementProgressBarValue), 1);
                        OnSetProgress(1);
                    }
                    sourceReader.Close();
                    sourceReader.Dispose();
                }
                catch (Exception ex)
                {
                    OnAddStatusMessage(string.Format(ImportExportSharedStrings.ERROR_WITH_MESSAGE, ex.Message));                    
                }
                finally
                {
                }

                if (update && append)
                {
                    OnAddStatusMessage(string.Format(ImportExportSharedStrings.IMPORT_PAGE_UPDATED_AND_APPENDED, destinationPage.Name, recordsInserted.ToString(), recordsUpdated.ToString()));
                }
                else if (update && !append)
                {                    
                    OnAddStatusMessage(string.Format(ImportExportSharedStrings.IMPORT_PAGE_UPDATED, destinationPage.Name, recordsUpdated.ToString()));
                }
                else if (!update && append)
                {
                    OnAddStatusMessage(string.Format(ImportExportSharedStrings.IMPORT_PAGE_APPENDED, destinationPage.Name, recordsInserted.ToString()));
                }
            }
        }

        /// <summary>
        /// Processes all related forms
        /// </summary>
        /// <param name="sourceView">The source form</param>
        /// <param name="destinationView">The destination form</param>
        /// <param name="viewsToProcess">The list of forms to be processed</param>
        private void ProcessRelatedForms(View sourceView, View destinationView, List<View> viewsToProcess)
        {
            foreach (View view in viewsToProcess)
            {
                if (!destinationProjectDataDriver.TableExists(view.TableName))
                {
                    destinationProject.CollectedData.CreateDataTableForView(view, 1);
                    OnAddStatusMessage(string.Format(ImportExportSharedStrings.CREATED_DATA_TABLE_FOR_FORM, view.Name));
                }
                
                Query selectQuery = destinationProjectDataDriver.CreateQuery("SELECT [GlobalRecordId] FROM [" + view.TableName + "]");
                IDataReader destReader = destinationProjectDataDriver.ExecuteReader(selectQuery);
                List<string> destinationRelatedGUIDList = new List<string>();

                View relatedDestinationView = destinationProject.Views[view.Name];

                while (destReader.Read())
                {
                    destinationRelatedGUIDList.Add(destReader[0].ToString());
                }

                destReader.Close();
                destReader.Dispose();

                PopulateSourceGUIDs(view);

                ProcessBaseTable(view, relatedDestinationView, destinationRelatedGUIDList);
                ProcessPages(view, relatedDestinationView, destinationRelatedGUIDList);
                ProcessGridFields(view, relatedDestinationView);
                // Do not process related forms again, that's all being done in this loop                
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
                    OnAddStatusMessage(string.Format(ImportExportSharedStrings.PROCESSING_GRID, gridField.Name));

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

                        string gridReference = sourceView.Name + ":" + gridField.Name;

                        if (GridColumnsToNull != null && GridColumnsToNull.ContainsKey(gridReference))
                        {
                            List<string> toNull = GridColumnsToNull[gridReference];

                            foreach (string s in toNull)
                            {
                                if (!gridColumnsToSkip.Contains(s))
                                {
                                    gridColumnsToSkip.Add(s);
                                }
                            }
                        }

                        while (destinationGridTableReader.Read())
                        {
                            gridGUIDList.Add(destinationGridTableReader["UniqueRowId"].ToString());
                        }

                        destinationGridTableReader.Close();
                        destinationGridTableReader.Dispose();

                        IDataReader sourceGridTableReader = sourceProjectDataDriver.GetTableDataReader(gridField.TableName);
                        while (sourceGridTableReader.Read())
                        {
                            string GUID = sourceGridTableReader["UniqueRowId"].ToString();
                            string FKEY = sourceGridTableReader["FKEY"].ToString();

                            if (sourceGUIDs != null && !sourceGUIDs.Contains(FKEY))
                            {
                                continue;
                            }

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
                                            fieldValueParams.Add(new QueryParameter("@" + gridColumn.Name, DbType.Single, data));
                                            break;
                                        default:
                                            throw new ApplicationException(string.Format(ImportExportSharedStrings.ERROR_GRID_COLUMN, gridColumn.Name));
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
                                OnSetProgress(1);
                                //this.BeginInvoke(new SetProgressBarDelegate(IncrementProgressBarValue), 1);

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

                                    fieldNames.Add(destinationProjectDataDriver.InsertInEscape(gridColumn.Name));
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
                                            fieldValueParams.Add(new QueryParameter("@" + gridColumn.Name, DbType.Single, data));
                                            break;
                                        default:
                                            throw new ApplicationException(string.Format(ImportExportSharedStrings.ERROR_GRID_COLUMN, gridColumn.Name));
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
                                OnSetProgress(1);
                                //this.BeginInvoke(new SetProgressBarDelegate(IncrementProgressBarValue), 1);
                                recordsInserted++;
                            }
                            OnSetProgress(1);
                        } // end while source data reader reads

                        sourceGridTableReader.Close();
                        sourceGridTableReader.Dispose();

                        if (update && append)
                        {
                            OnAddStatusMessage(string.Format(ImportExportSharedStrings.IMPORT_GRID_UPDATED_AND_APPENDED, gridField.Name, recordsInserted.ToString(), recordsUpdated.ToString()));
                        }
                        else if (update && !append)
                        {
                            OnAddStatusMessage(string.Format(ImportExportSharedStrings.IMPORT_GRID_UPDATED, gridField.Name, recordsUpdated.ToString()));
                        }
                        else if (!update && append)
                        {
                            OnAddStatusMessage(string.Format(ImportExportSharedStrings.IMPORT_GRID_APPENDED, gridField.Name, recordsInserted.ToString()));
                        }
                    }
                    catch (Exception ex)
                    {
                        OnAddStatusMessage(string.Format(ImportExportSharedStrings.ERROR_WITH_MESSAGE, ex.Message));                        
                    }
                } // end if contains
            }
        }
        #endregion // Private Methods

        #region Protected Methods
        /// <summary>
        /// Checks for cancellation
        /// </summary>
        protected virtual bool OnCheckForCancellation()
        {
            if (CheckForCancellation != null)
            {
                return CheckForCancellation();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Adds a status message
        /// </summary>
        /// <param name="message">The message</param>
        protected virtual void OnAddStatusMessage(string message)
        {
            if (AddStatusMessage != null)
            {
                AddStatusMessage(message);
            }
        }

        /// <summary>
        /// Sets status message
        /// </summary>
        /// <param name="message">The message</param>
        protected virtual void OnSetStatusMessage(string message)
        {
            if (SetStatus != null)
            {
                SetStatus(message);
            }
        }

        /// <summary>
        /// Sets progess bar value
        /// </summary>
        /// <param name="progress">The message</param>
        protected virtual void OnSetProgress(double progress)
        {
            if (SetProgressBar != null)
            {
                SetProgressBar(progress);
            }
        }

        /// <summary>
        /// Sets max attainable progess bar value
        /// </summary>
        /// <param name="maxProgress">The max value to set</param>
        protected virtual void OnSetMaxProgressValue(double maxProgress)
        {
            if (SetMaxProgressBarValue != null)
            {
                SetMaxProgressBarValue(maxProgress);
            }
        }
        #endregion // Protected Methods
    }
}
