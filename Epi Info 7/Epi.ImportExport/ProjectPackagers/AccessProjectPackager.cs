#region Using
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Epi;
using Epi.Core;
using Epi.Data;
using Epi.Fields;
#endregion // Using

namespace Epi.ImportExport
{
    /// <summary>
    /// Used for packaging an Epi Info 7 project for transport whose collected data is stored in a 
    /// Microsoft Access database. The packager creates a copy of the MDB file on the disk and then
    /// removes any unneeded data from it. This method is generally far quicker than using the 
    /// generic project packager, but does open the possibility of leaving data signatures on disk
    /// as everything is initially written. If the intention is to never write anything to the
    /// destination that wasn't selected by the user, then the generic class should be used instead.
    /// -- E. Knudsen, 2012
    /// </summary>
    public class AccessProjectPackager : ProjectPackagerBase
    {
        #region Events
        #endregion // Events

        #region Constructors
        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="sourceProject">The project from which to create the package.</param>
        /// <param name="packagePath">The file path to where the package will reside.</param>
        /// <param name="formName">The name of the form within the project to package.</param>
        /// <param name="password">The password for the encryption.</param>
        public AccessProjectPackager(Project sourceProject, string packagePath, string formName, string password) 
            : base(sourceProject, packagePath, formName, password)
        {            
        }

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="sourceProject">The project from which to create the package.</param>
        /// <param name="columnsToNull">The columns in the table to null</param>
        /// <param name="gridColumnsToNull">The grid columns to null</param>
        /// <param name="packagePath">The file path to where the package will reside.</param>
        /// <param name="formName">The name of the form within the project to package.</param>
        /// <param name="password">The password for the encryption.</param>
        public AccessProjectPackager(Project sourceProject, Dictionary<string, List<string>> columnsToNull, Dictionary<string, List<string>> gridColumnsToNull, string packagePath, string formName, string password)
            : base(sourceProject, columnsToNull, gridColumnsToNull, packagePath, formName, password)
        {            
        }
        #endregion // Constructors

        #region Protected Methods
        /// <summary>
        ///  Creates a copy of the source project.
        /// </summary>
        /// <returns>bool; represents success or failure of the copy procedure.</returns>
        protected override bool CreateProjectCopy()
        {
            OnSetStatusMessage(ImportExportSharedStrings.COLLECTED_DATA_COPY_START);

            destinationProject = CreateProject();

            string sourceFileName = SourceProject.CollectedData.DataSource;
            string destinationFileName = destinationProject.CollectedData.DataSource;

            if (File.Exists(destinationFileName))
            {
                File.Delete(destinationFileName);
            }

            File.Copy(sourceFileName, destinationFileName);

            IDbDriver db = destinationProject.CollectedData.GetDbDriver();
            destinationProject.Metadata.AttachDbDriver(db);

            View destinationView = destinationProject.Views[FormName];

            // Create data tables for related forms that may not have them; this should techincally never be the case,
            // but it's possible that someone deleted a data table for a related form in Form Designer and never re-created it.
            // Not doing this has implications for grids...
            foreach (View view in destinationProject.Views)
            {
                if (ImportExportHelper.IsFormDescendant(view, destinationView))
                {
                    if (!view.Project.CollectedData.TableExists(view.TableName))
                    {
                        view.SetTableName(view.Name);
                        destinationProject.CollectedData.CreateDataTableForView(view, 1);
                        OnAddStatusMessage(string.Format(ImportExportSharedStrings.CREATED_DATA_TABLE_FOR_FORM_VERBOSE, view.Name));
                    }
                }
            }

            OnSetStatusMessage(ImportExportSharedStrings.COLLECTED_DATA_COPY_END);

            RemoveRowData();
            RemoveColumnData();

            if (!DeleteUnrelatedData())
            {
                OnSetStatusMessage(string.Format(ImportExportSharedStrings.WARNING_UNRELATED_FORMS, FormName));
            }

            if (!RemoveGridColumnData())
            {
                OnSetStatusMessage(ImportExportSharedStrings.WARNING_GRID_COLUMN_REMOVAL_FAILURE);
            }

            try
            {
                destinationProject.CollectedData.GetDbDriver().CompactDatabase();
            }
            catch (Exception ex)
            {
                OnAddStatusMessage(string.Format(ImportExportSharedStrings.WARNING_COMPACT_FAILURE, ex.Message));
            }

            return true;
        }

        /// <summary>
        /// Removes or clears all rows that meet the specified criteria
        /// </summary>
        /// <returns>bool; represents the success or failure of the method to remove row data</returns>
        protected override bool RemoveRowData()
        {
            #region Check Inputs
            if (SelectQuery == null)
            {
                return true;
            }
            #endregion // Check Inputs

            /* Importantly, the user has selected row data that they want to send. However, we already 
             * have a copy of all the data; therefore, we want to remove what the user didn't select. 
             * The condition has to be "notted" so to speak. E.g. if they picked OnsetDate > 1/1/2005,
             * the delete query would get everything NOT (OnsetDate > 1/1/2005)
             * 
             * This method should also recursively remove all related row data from both related forms
             * and from grid tables.
             */

            OnSetStatusMessage("Applying row filters...");

            // get Data driver
            IDbDriver db = destinationProject.CollectedData.GetDbDriver();
            View sourceView = sourceProject.Views[FormName];

            Query preRecordCountQuery = db.CreateQuery("SELECT Count(*) FROM [" + sourceView.TableName + "]");
            object preRecordCount = db.ExecuteScalar(preRecordCountQuery);

            Dictionary<View, int> formLevels = new Dictionary<View, int>();
            formLevels.Add(sourceView, 0);

            int maxLevel = 0;

            // Find at what level the forms are in the heirarchy. Parent = 0, child = 1, grandchild = 2, etc. A
            // value of -1 indicates the form isn't in the heirarchy. This is important because the row deletions
            // must cascade down from the parent in order.
            foreach (View view in sourceProject.Views)
            {
                int level = ImportExportHelper.GetFormDescendantLevel(view, sourceView, 0);
                if (level > 0)
                {
                    formLevels.Add(view, level);
                }

                if (level > maxLevel)
                {
                    maxLevel = level;
                }
            }

            List<View> viewsToProcess = new List<View>();            

            for (int i = 1; i <= maxLevel; i++)
            {
                foreach (KeyValuePair<View, int> kvp in formLevels)
                {
                    if (kvp.Value == i)
                    {
                        viewsToProcess.Add(kvp.Key);
                    }
                }
            }

            // This is messy, but the user will have selected the rows to "keep" and we need to delete the
            // rows they didn't select to keep - therefore we add a NOT in front of their condition.
            string sql = SelectQuery.SqlStatement;
            int indexOf = sql.ToLower().IndexOf(" where ");
            sql = sql.Insert(indexOf + 7, "not(");
            sql = sql + ")";
            
            // Create a new query since we can't change the query text of the original
            Query selectNotQuery = db.CreateQuery(sql);
            selectNotQuery.Parameters = SelectQuery.Parameters;

            // The GUID list will be instrumental in wiping rows from related forms and related grids
            List<string> parentGuidList = new List<string>();
            List<string> childGuidList = new List<string>();

            IDataReader reader = null;

            try
            {
                reader = db.ExecuteReader(selectNotQuery);

                // Add the GUIDs that aren't going to be kept to the GUID list
                while (reader.Read())
                {
                    parentGuidList.Add(reader["GlobalRecordId"].ToString());
                }

                reader.Close();
                reader.Dispose();
            }
            catch
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                    reader = null;
                    return false;
                }
            }

            foreach (string value in parentGuidList)
            {
                Query deleteQuery = db.CreateQuery("DELETE * FROM " + db.InsertInEscape(sourceView.TableName) + " WHERE [GlobalRecordId] = @Guid");
                deleteQuery.Parameters.Add(new QueryParameter("@Guid", DbType.String, value));
                db.ExecuteNonQuery(deleteQuery);

                foreach (Page page in sourceView.Pages)
                {
                    deleteQuery = db.CreateQuery("DELETE * FROM " + db.InsertInEscape(page.TableName) + " WHERE [GlobalRecordId] = @Guid");
                    deleteQuery.Parameters.Add(new QueryParameter("@Guid", DbType.String, value));
                    db.ExecuteNonQuery(deleteQuery);
                }
            }

            RemoveOrphanRecords(viewsToProcess);
            RemoveOrphanGridRecords(viewsToProcess);

            Query recordCountQuery = db.CreateQuery("SELECT Count(*) FROM [" + sourceView.TableName + "]");
            object recordCount = db.ExecuteScalar(recordCountQuery);

            OnSetStatusMessage(string.Format(ImportExportSharedStrings.ROW_FILTERS_END, recordCount.ToString(), preRecordCount.ToString()));

            return true;
        }

        /// <summary>
        /// Removes all orphaned grid records
        /// </summary>        
        /// <param name="relatedFormsToProcess">List of forms to process</param>
        private void RemoveOrphanGridRecords(List<View> relatedFormsToProcess)
        {
            #region Input Validation
            if (relatedFormsToProcess == null || relatedFormsToProcess.Count == 0)
            {
                return;
            }
            #endregion // Input Validation

            IDbDriver db = destinationProject.CollectedData.GetDbDriver();
            List<string> parentGUIDList = new List<string>();
            Dictionary<string, string> gridKeys = new Dictionary<string, string>();
            List<View> formsToProcess = relatedFormsToProcess;
            int runningDeletionTotal = 0;

            formsToProcess.Add(sourceProject.Views[FormName]);

            // iterate over all the views that we need to process, in hierarchal order (top to bottom)
            foreach (View view in formsToProcess)
            {
                foreach (GridField gridField in view.Fields.GridFields)
                {
                    OnSetStatusMessage(string.Format(ImportExportSharedStrings.ORPHAN_GRID_ROW_REMOVAL_START, gridField.Name));

                    gridKeys.Clear();
                    parentGUIDList.Clear();

                    Query gridGUIDQuery = db.CreateQuery("SELECT [FKEY], [GlobalRecordId] FROM " + db.InsertInEscape(gridField.TableName));
                    IDataReader gridReader = db.ExecuteReader(gridGUIDQuery);
                    while (gridReader.Read())
                    {
                        if (!gridKeys.ContainsKey(gridReader["GlobalRecordId"].ToString()))
                        {
                            gridKeys.Add(gridReader["GlobalRecordId"].ToString(), gridReader["FKEY"].ToString());
                        }
                    }

                    gridReader.Close();
                    gridReader.Dispose();

                    Query parentGUIDQuery = db.CreateQuery("SELECT [GlobalRecordId] FROM " + db.InsertInEscape(view.TableName));
                    IDataReader parentReader = db.ExecuteReader(parentGUIDQuery);
                    while (parentReader.Read())
                    {
                        parentGUIDList.Add(parentReader["GlobalRecordId"].ToString());
                    }

                    parentReader.Close();
                    parentReader.Dispose();

                    List<string> toRemove = new List<string>();

                    foreach (KeyValuePair<string, string> kvp in gridKeys)
                    {
                        if (!parentGUIDList.Contains(kvp.Value))
                        {
                            toRemove.Add(kvp.Key);
                        }
                    }

                    foreach (string s in toRemove)
                    {
                        Query deleteQuery = db.CreateQuery("DELETE * FROM " + db.InsertInEscape(gridField.TableName) + " WHERE [GlobalRecordId] = @Guid");
                        deleteQuery.Parameters.Add(new QueryParameter("@Guid", DbType.String, s));
                        runningDeletionTotal = runningDeletionTotal + db.ExecuteNonQuery(deleteQuery);
                    }
                }
            }

            if (runningDeletionTotal > 0)
            {
                OnSetStatusMessage(string.Format(ImportExportSharedStrings.ORPHAN_GRID_ROW_REMOVAL_END, runningDeletionTotal.ToString()));
            }
        }

        /// <summary>
        /// Removes all orphaned records
        /// </summary>
        /// <param name="relatedFormsToProcess">List of forms to process</param>
        private void RemoveOrphanRecords(List<View> relatedFormsToProcess)
        {
            #region Input Validation
            if(relatedFormsToProcess == null || relatedFormsToProcess.Count == 0) 
            {
                return;
            }
            #endregion // Input Validation

            IDbDriver db = destinationProject.CollectedData.GetDbDriver();
            List<string> parentGUIDList = new List<string>();
            Dictionary<string, string> childKeys = new Dictionary<string,string>();
            int runningDeletionTotal = 0;

            // iterate over all the views that we need to process, in hierarchal order (top to bottom)
            foreach (View view in relatedFormsToProcess)
            {
                View parentView = null;

                if (view.ParentView != null)
                {
                    OnSetStatusMessage(string.Format(ImportExportSharedStrings.ORPHAN_ROW_REMOVAL_START, view.Name));
                    parentView = view.ParentView;

                    childKeys.Clear();
                    parentGUIDList.Clear();

                    Query childGUIDQuery = db.CreateQuery("SELECT [FKEY], [GlobalRecordId] FROM " + db.InsertInEscape(view.TableName));
                    IDataReader childReader = db.ExecuteReader(childGUIDQuery);
                    while (childReader.Read())
                    {
                        childKeys.Add(childReader["GlobalRecordId"].ToString(), childReader["FKEY"].ToString());
                    }

                    childReader.Close();
                    childReader.Dispose();

                    Query parentGUIDQuery = db.CreateQuery("SELECT [GlobalRecordId] FROM " + db.InsertInEscape(parentView.TableName));
                    IDataReader parentReader = db.ExecuteReader(parentGUIDQuery);
                    while (parentReader.Read())
                    {
                        parentGUIDList.Add(parentReader["GlobalRecordId"].ToString());
                    }

                    parentReader.Close();
                    parentReader.Dispose();

                    List<string> toRemove = new List<string>();

                    foreach (KeyValuePair<string, string> kvp in childKeys)
                    {
                        if (!parentGUIDList.Contains(kvp.Value))
                        {
                            toRemove.Add(kvp.Key);
                        }
                    }

                    foreach (string s in toRemove)
                    {
                        Query deleteQuery = db.CreateQuery("DELETE * FROM " + db.InsertInEscape(view.TableName) + " WHERE [GlobalRecordId] = @Guid");
                        deleteQuery.Parameters.Add(new QueryParameter("@Guid", DbType.String, s));
                        runningDeletionTotal = runningDeletionTotal + db.ExecuteNonQuery(deleteQuery);

                        foreach (Page page in view.Pages)
                        {
                            deleteQuery = db.CreateQuery("DELETE * FROM " + db.InsertInEscape(page.TableName) + " WHERE [GlobalRecordId] = @Guid");
                            deleteQuery.Parameters.Add(new QueryParameter("@Guid", DbType.String, s));
                            db.ExecuteNonQuery(deleteQuery);
                        }
                    }
                }
            }

            if (runningDeletionTotal > 0)
            {
                OnSetStatusMessage(string.Format(ImportExportSharedStrings.ORPHAN_ROW_REMOVAL_END, runningDeletionTotal.ToString()));
            }
        }

        private List<string> GetParentGUIDList(View view)
        {
            // get Data driver
            IDbDriver db = destinationProject.CollectedData.GetDbDriver();
            View sourceView = sourceProject.Views[FormName];

            return null;
        }

        /// <summary>
        /// Sets to NULL all cells in the specified columns
        /// </summary>
        /// <returns>bool; represents the success or failure of the method to remove column data</returns>
        protected override bool RemoveColumnData()
        {
            /* The user may have specified that they want to null out certain columns. This feature is
             * included so that users can send de-identified data to other organizations or agencies.
             * That is, a local health department may wish to send CDC their data, but without personally-
             * identifiable information such as name, age, address, zip code, etc. 
             */

            OnSetStatusMessage(ImportExportSharedStrings.COLUMN_DATA_REMOVAL_START);

            int total = 0;
            int columnsNulled = 0;

            foreach (KeyValuePair<string, List<string>> kvp in this.ColumnsToNull)
            {
                foreach (string s in kvp.Value)
                {
                    total++;
                }
            }

            IDbDriver db = destinationProject.CollectedData.GetDbDriver();

            foreach (KeyValuePair<string, List<string>> kvp in this.ColumnsToNull)
            {
                View viewToProcess = sourceProject.Views[kvp.Key];
                List<string> columnNames = kvp.Value;                

                foreach (string columnName in columnNames)
                {
                    if (viewToProcess.Fields.Contains(columnName))
                    {
                        Epi.Fields.Field field = viewToProcess.Fields[columnName];                        
                        Page page = new Page();
                        
                        foreach (Page iPage in viewToProcess.Pages)
                        {
                            if (iPage.Fields.Contains(columnName))
                            {
                                page = iPage;
                                break;
                            }
                        }

                        if (page.view != null)
                        {
                            string updateQueryText = "UPDATE " + db.InsertInEscape(page.TableName) + " SET " + db.InsertInEscape(columnName) + " = null";
                            Query updateQuery = db.CreateQuery(updateQueryText);
                            int recordsAffected = db.ExecuteNonQuery(updateQuery);

                            int totalRecords = (int)db.ExecuteScalar(db.CreateQuery("SELECT Count(*) FROM " + db.InsertInEscape(page.TableName)));

                            // Verify the rows were removed. If they weren't, we should pop a warning to the user.
                            if (recordsAffected == totalRecords) 
                            {
                                columnsNulled++;
                                OnAddStatusMessage(string.Format(ImportExportSharedStrings.COLUMN_DATA_REMOVAL_SUCCESS, columnName, page.TableName));
                            }
                            else
                            {
                                OnAddStatusMessage(string.Format(ImportExportSharedStrings.COLUMN_DATA_REMOVAL_FAIL, columnName, page.TableName));
                            }
                        }
                    }
                }                
            }

            // If this condition is false, it indicates some columns were not nulled and this could present
            // a PII or security risk - especially since, once encrypted, the user cannot verify the data removal.
            // Therefore, fail in order to warn the user.
            if (columnsNulled == total)
            {
                OnSetStatusMessage(ImportExportSharedStrings.COLUMN_DATA_REMOVAL_END);
                return true;
            }
            else
            {
                OnSetStatusMessage(ImportExportSharedStrings.COLUMN_DATA_REMOVAL_END_WITH_WARNING);
                return false;
            }
        }
        #endregion // Protected Methods

        #region Private Methods
        /// <summary>
        /// Deletes selected grid column data
        /// </summary>
        /// <returns>bool; represents whether user-specified grid column data was removed.</returns>
        private bool RemoveGridColumnData()
        {
            #region Input Validation
            if (this.GridColumnsToNull == null || this.GridColumnsToNull.Count == 0)
            {
                return true;
            }
            #endregion // Input Validation

            int total = 0;
            int columnsNulled = 0;

            foreach (KeyValuePair<string, List<string>> kvp in this.GridColumnsToNull)
            {
                if (kvp.Key.Contains(':'))
                {
                    foreach (string s in kvp.Value)
                    {
                        total++;
                    }
                }
            }

            IDbDriver db = destinationProject.CollectedData.GetDbDriver();            

            foreach (KeyValuePair<string, List<string>> kvp in this.GridColumnsToNull)
            {
                if (kvp.Key.Contains(':'))
                {
                    string[] gridFieldInfo = kvp.Key.ToString().Split(':');

                    View view = destinationProject.Views[gridFieldInfo[0]];
                    GridField gridField = view.Fields.GridFields[gridFieldInfo[1]];

                    foreach (string gridColumnName in kvp.Value)
                    {
                        string updateQueryText = "UPDATE " + db.InsertInEscape(gridField.TableName) + " SET " + db.InsertInEscape(gridColumnName) + " = null";
                        Query updateQuery = db.CreateQuery(updateQueryText);
                        int recordsAffected = db.ExecuteNonQuery(updateQuery);

                        int totalRecords = (int)db.ExecuteScalar(db.CreateQuery("SELECT Count(*) FROM " + db.InsertInEscape(gridField.TableName)));

                        // Verify the rows were removed. If they weren't, we should pop a warning to the user.
                        if (recordsAffected == totalRecords)
                        {
                            columnsNulled++;
                            OnAddStatusMessage(string.Format(ImportExportSharedStrings.GRID_COLUMN_DATA_REMOVAL_SUCCESS, gridColumnName, gridField.Name));
                        }
                        else
                        {
                            OnAddStatusMessage(string.Format(ImportExportSharedStrings.GRID_COLUMN_DATA_REMOVAL_FAIL, gridColumnName, gridField.Name));                                
                        }
                    }
                }
            }

            if (columnsNulled != total)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Deletes any data left in the MDB that is not related to the selected form.
        /// </summary>      
        /// <returns>bool; represents whether unrelated data was removed or not</returns>
        private bool DeleteUnrelatedData()
        {
            /* The problem with creating a copy of the MDB file (as is done in this class) for use
             * as the actual package is that it may contain a lot of data unrelated to the selected
             * form. For example: Temporary output tables that the user may have created in Classic
             * Analysis. These tables may contain PII or sensitive data. If they aren't in any way
             * connected to the form the user selected in the UI, then they shouldn't be part of
             * the data package and can be safely removed. Rather than wiping out the table, the
             * rows are simply erased.
             * 
             * Some tables are known to be okay and should be kept, especially because some of them
             * may be needed during the import process. The 'meta' tables are a good example. And,
             * obviously, so are any tables associated with the selected form, any of its related
             * forms, and any grids on any of those forms.
             */

            List<View> unrelatedForms = new List<View>();
            int errors = 0;

            IDbDriver db = destinationProject.CollectedData.GetDbDriver();

            List<string> tableNames = db.GetTableNames();
            tableNames.Remove("metaBackgrounds");
            tableNames.Remove("metaDataTypes");
            tableNames.Remove("metaDbInfo");
            tableNames.Remove("metaFields");
            tableNames.Remove("metaFieldTypes");
            tableNames.Remove("metaGridColumns");
            tableNames.Remove("metaImages");
            tableNames.Remove("metaLayerRenderTypes");
            tableNames.Remove("metaLayers");
            tableNames.Remove("metaLinks");
            tableNames.Remove("metaMapLayers");
            tableNames.Remove("metaMapPoints");
            tableNames.Remove("metaMaps");
            tableNames.Remove("metaPages");
            tableNames.Remove("metaPatterns");
            tableNames.Remove("metaPrograms");
            tableNames.Remove("metaViews");

            // Should code table data be removed too? If not, here is the place to change that.

            if (IncludeCodeTables)
            {
                DataTable codeTableNames = db.GetCodeTableNamesForProject(sourceProject);
                foreach (DataRow row in codeTableNames.Rows)
                {
                    if (tableNames.Contains(row[0].ToString()))
                    {
                        tableNames.Remove(row[0].ToString());
                    }
                }
            }

            View destinationView = destinationProject.Views[FormName];
            foreach (View view in destinationProject.Views)
            {
                int formLevel = 0;
                formLevel = ImportExportHelper.GetFormDescendantLevel(view, destinationView, formLevel);

                if (formLevel >= 0)
                {
                    bool deleteFormData = true;

                    switch (FormInclusionType)
                    {
                        case ImportExport.FormInclusionType.CurrentFormOnly:
                            if (formLevel == 0)
                            {
                                deleteFormData = false;
                            }
                            else
                            {
                                deleteFormData = true;
                            }
                            break;
                        case ImportExport.FormInclusionType.DirectDescendants:
                            if (formLevel <= 1)
                            {
                                deleteFormData = false;
                            }
                            else
                            {
                                deleteFormData = true;
                            }
                            break;
                        case ImportExport.FormInclusionType.AllDescendants:
                            deleteFormData = false;
                            break;
                    }

                    if (!deleteFormData)
                    {
                        if (tableNames.Contains(view.TableName))
                        {
                            tableNames.Remove(view.TableName);
                        }

                        foreach (Page page in view.Pages)
                        {
                            if (tableNames.Contains(page.TableName))
                            {
                                tableNames.Remove(page.TableName);
                            }
                        }
                    }
                    
                    if (this.IncludeGridData)
                    {
                        // Very import that grid fields are checked; grids store their data in separate tables.
                        foreach (GridField gridField in view.Fields.GridFields)
                        {
                            if (tableNames.Contains(gridField.TableName))
                            {
                                tableNames.Remove(gridField.TableName);
                            }
                        }
                    }
                }
            }

            foreach (string tableName in tableNames)
            {
                try
                {
                    int totalRecords = (int)db.ExecuteScalar(db.CreateQuery("SELECT Count(*) FROM " + db.InsertInEscape(tableName)));
                    Query tableDeleteQuery = db.CreateQuery("DELETE * FROM " + db.InsertInEscape(tableName));
                    int affectedRecords = db.ExecuteNonQuery(tableDeleteQuery);

                    // Verify the deletion affected all the rows in this table.
                    if (totalRecords == affectedRecords)
                    {
                        OnAddStatusMessage(string.Format(ImportExportSharedStrings.TABLE_DATA_DELETION_SUCCESS, tableName));
                    }
                    else
                    {
                        OnAddStatusMessage(string.Format(ImportExportSharedStrings.TABLE_DATA_DELETION_FAIL, affectedRecords, totalRecords, tableName));                        
                        errors++;
                    }
                }
                catch (Exception ex)
                {
                    errors++;
                    OnAddStatusMessage(string.Format(ImportExportSharedStrings.ERROR_WITH_MESSAGE, ex.Message));
                }
            }

            if (errors > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion // Private Methods
    }
}
