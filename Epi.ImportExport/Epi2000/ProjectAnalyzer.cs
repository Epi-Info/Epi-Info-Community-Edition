using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Epi;
using Epi.Core;
using Epi.Data;
using Epi.ImportExport;

namespace Epi.ImportExport.Epi2000
{
    /// <summary>
    /// The project analyzer will check the validity of an Epi Info 3.5.x project and determine what problems there may be if an upgrade on this project is attempted. It does
    /// not determine if the project should be upgraded or not, but rather, just finds things that may be invalid, incorrect, broken, or that are otherwise normal but for which
    /// the Epi Info 7 code is not designed to handle.
    /// </summary>
    public class ProjectAnalyzer
    {
        #region Members
        /// <summary>
        /// The Epi Info 3.5.x project that will be analyzed.
        /// </summary>
        private Epi.Epi2000.Project sourceProject;

        /// <summary>
        /// The list of errors that are found.
        /// </summary>
        private ImportExportErrorList errorList;
        #endregion // Members

        #region Events
        public event UpdateStatusEventHandler SetStatus;
        #endregion // Events

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pSourceProject">The Epi Info 3.5.x project that will be analyzed.</param>
        public ProjectAnalyzer(Epi.Epi2000.Project pSourceProject)
        {
            this.sourceProject = pSourceProject;
        }
        #endregion // Constructors

        #region Properties
        /// <summary>
        /// Gets the list of problems found during analysis
        /// </summary>
        public ImportExportErrorList ErrorList
        {
            get
            {
                return this.errorList;
            }
        }

        /// <summary>
        /// Gets the source project
        /// </summary>
        private Epi.Epi2000.Project SourceProject
        {
            get
            {
                return this.sourceProject;
            }
        }
        #endregion // Properties

        #region Methods
        /// <summary>
        /// Analyzes the Epi Info 3.5.x project.
        /// </summary>
        public void Analyze()
        {
            CheckSourceProjectForProblems();
        }

        /// <summary>
        /// Checks the Epi Info 3.5.x project for problems.
        /// </summary>
        private void CheckSourceProjectForProblems()
        {
            if (SetStatus != null)
            {
                SetStatus(string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_MESSAGE_ANALYZING_PROJECT, sourceProject.Name));
            }

            errorList = new ImportExportErrorList();


            try
            {

                Epi.Data.IDbDriver db = sourceProject.CollectedData.GetDatabase();
                string validationMessage = string.Empty;
                if (!db.IsDatabaseFormatValid(ref validationMessage))
                {
                    errorList.Add(ImportExportMessageType.Error, "1100", validationMessage);
                    return;
                }

                if (sourceProject.Views.Count == 0)
                {
                    errorList.Add(ImportExportMessageType.Error, "1020", ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1020);
                }

                foreach (Epi.Epi2000.View view in sourceProject.Views)
                {
                    // Check #1 - Does the view have a name?
                    if (string.IsNullOrEmpty(view.Name))
                    {
                        errorList.Add(ImportExportMessageType.Error, "1000", ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1000);
                    }

                    if (!view.Name.ToLower().StartsWith("view"))
                    {
                        errorList.Add(ImportExportMessageType.Error, "1021", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1021, view.Name));
                    }

                    if (view.Name.Contains(' '))
                    {
                        errorList.Add(ImportExportMessageType.Error, "1022", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1022, view.Name));
                    }

                    // Check #2 - Does the view have any pages?
                    if (view.Pages.Count <= 0)
                    {
                        errorList.Add(ImportExportMessageType.Warning, "5000", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_W5000, view.Name));
                    }

                    CheckViewSourceReferences(view);

                    if (!string.IsNullOrEmpty(view.TableName) && db.TableExists(view.TableName))
                    {
                        object result = db.ExecuteScalar(db.CreateQuery("SELECT Count(*) FROM [" + view.TableName + "]"));

                        CheckViewColumns(view);

                        int baseRowCount = 0;
                        if (result is Int32)
                        {
                            baseRowCount = (Int32)result;
                        }
                        else
                        {
                            errorList.Add(ImportExportMessageType.Error, "1001", "Something that should never fail has failed.");
                        }

                        bool hasUniqueKey = db.ColumnExists(view.TableName, "UniqueKey");
                        bool hasRecStatus = db.ColumnExists(view.TableName, "RecStatus");

                        // Check #3a - Does the table have a UniqueKey?
                        if (!hasUniqueKey)
                        {
                            errorList.Add(ImportExportMessageType.Error, "1002", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1002, view.Name));
                        }
                        // Check #3b - Does the table have a RECSTATUS column?
                        if (!hasRecStatus)
                        {
                            errorList.Add(ImportExportMessageType.Error, "1003", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1003, view.Name));
                        }
                        if (hasUniqueKey)
                        {
                            // Check #4a - Is the unique key implemented as a number?
                            DataTable dt = db.GetTableData(view.TableName, "UniqueKey");
                            string dataType = dt.Columns[0].DataType.ToString();
                            if (dataType != "System.Int32")
                            {
                                errorList.Add(ImportExportMessageType.Error, "1004", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1004, view.Name));
                            }

                            // Check #4b - Does the unique key have any missing / null values? (May occur if user modifies table by hand for various reasons)
                            Epi.Data.Query selectDistinctQuery = db.CreateQuery("SELECT UniqueKey FROM [" + view.TableName + "] WHERE UniqueKey = NULL OR UniqueKey is NULL");
                            dt = db.Select(selectDistinctQuery);
                            if (dt.Rows.Count > 0)
                            {
                                errorList.Add(ImportExportMessageType.Error, "1005", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1005, view.Name));
                            }
                        }

                        if (hasRecStatus)
                        {
                            // Check #4c - Is RECSTATUS implemented as a number?
                            DataTable dt = db.GetTableData(view.TableName, "RECSTATUS");
                            string dataType = dt.Columns[0].DataType.ToString();
                            if (dataType != "System.Int16" && dataType != "System.Byte")
                            {
                                errorList.Add(ImportExportMessageType.Error, "1006", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1006, view.Name));
                            }

                            // Check #4d - Does RECSTATUS have any values other than 0 or 1?
                            Epi.Data.Query selectDistinctQuery = db.CreateQuery("SELECT RECSTATUS FROM [" + view.TableName + "] WHERE RECSTATUS = NULL OR RECSTATUS is NULL OR RECSTATUS < 0 OR RECSTATUS > 1");
                            dt = db.Select(selectDistinctQuery);
                            if (dt.Rows.Count > 0)
                            {
                                errorList.Add(ImportExportMessageType.Error, "1007", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1007, view.Name));
                            }
                        }

                        List<string> reservedTableNames = new List<string>();
                        reservedTableNames.Add("metabackgrounds");
                        reservedTableNames.Add("metadatatypes");
                        reservedTableNames.Add("metadbinfo");
                        reservedTableNames.Add("metafields");
                        reservedTableNames.Add("metafieldtypes");
                        reservedTableNames.Add("metagridcolumns");
                        reservedTableNames.Add("metaimages");
                        reservedTableNames.Add("metalayerrendertypes");
                        reservedTableNames.Add("metalayers");
                        reservedTableNames.Add("metamaplayers");
                        reservedTableNames.Add("metamappoints");
                        reservedTableNames.Add("metamaps");
                        reservedTableNames.Add("metapages");
                        reservedTableNames.Add("metapatterns");
                        reservedTableNames.Add("metaprograms");
                        reservedTableNames.Add("metaviews");

                        // Check #5 - Does the view's table name conflict with Epi Info 7 meta table names?
                        if (reservedTableNames.Contains(view.TableName.ToLower()))
                        {
                            errorList.Add(ImportExportMessageType.Error, "1008", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1008, view.Name, view.TableName));
                        }

                        if (view.IsWideTableView)
                        {
                            foreach (string tableName in view.TableNames)
                            {
                                // Wide Table Check #1 - Does this wide table have a name?
                                if (string.IsNullOrEmpty(tableName))
                                {
                                    errorList.Add(ImportExportMessageType.Error, "1009", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1009, view.Name));
                                }
                                // Wide Table Check #2 - Does this wide table not exist? (We're in an IF that checks to see if the first table exists, so if this doesn't exist, there is a serious problem)
                                else if (!db.TableExists(tableName))
                                {
                                    errorList.Add(ImportExportMessageType.Error, "1010", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1010, tableName, view.Name));
                                }
                                else
                                {
                                    // Wide Table Check #3 - See if the wide table has a UniqueKey. Each wide table should have this.
                                    hasUniqueKey = db.ColumnExists(tableName, "UniqueKey");

                                    if (!hasUniqueKey)
                                    {
                                        errorList.Add(ImportExportMessageType.Error, "1011", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1011, tableName, view.Name));
                                    }
                                    if (hasUniqueKey)
                                    {
                                        DataTable dt = db.GetTableData(tableName, "UniqueKey");
                                        string dataType = dt.Columns[0].DataType.ToString();
                                        if (dataType != "System.Int32")
                                        {
                                            errorList.Add(ImportExportMessageType.Error, "1012", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1012, tableName, view.Name));
                                        }

                                        Epi.Data.Query selectDistinctQuery = db.CreateQuery("SELECT UniqueKey FROM [" + tableName + "] WHERE UniqueKey = NULL or UniqueKey is NULL");
                                        dt = db.Select(selectDistinctQuery);
                                        if (dt.Rows.Count > 0)
                                        {
                                            errorList.Add(ImportExportMessageType.Error, "1013", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1013, tableName, view.Name));
                                        }
                                    }

                                    // Wide Table Check #4 - Check to make sure the row counts match across wide tables.
                                    result = db.ExecuteScalar(db.CreateQuery("SELECT Count(*) FROM [" + tableName + "]"));
                                    if (result is Int32)
                                    {
                                        int tRowCount = (Int32)result;
                                        if (tRowCount != baseRowCount)
                                        {
                                            errorList.Add(ImportExportMessageType.Error, "1014", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1014, view.Name));
                                        }
                                    }

                                    // Wide Table Check #5 - Check all DATA references for duplicates or missing references
                                    DataTable generalCheckTable = db.Select(db.CreateQuery("SELECT [Datatable] FROM [" + view.Name + "] WHERE [Name] = 'GENERAL'"));
                                    if (generalCheckTable.Rows.Count == 0)
                                    {
                                        errorList.Add(ImportExportMessageType.Error, "1015", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1015, view.Name));
                                    }
                                    else if (generalCheckTable.Rows.Count > 1)
                                    {
                                        errorList.Add(ImportExportMessageType.Error, "1016", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1016, view.Name));
                                    }
                                    else
                                    {
                                        if (generalCheckTable.Rows[0][0].ToString().Contains(","))
                                        {
                                            string[] dataRefs = generalCheckTable.Rows[0][0].ToString().Split(',');
                                            List<string> dataRefsList = new List<string>();
                                            foreach (string dataRef in dataRefs)
                                            {
                                                if (!dataRefsList.Contains(dataRef))
                                                {
                                                    dataRefsList.Add(dataRef);
                                                }
                                                else
                                                {
                                                    errorList.Add(ImportExportMessageType.Error, "1017", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1017, view.Name));
                                                }
                                            }

                                            foreach (string dataRef in dataRefsList)
                                            {
                                                Query dataRefQuery = db.CreateQuery("SELECT [Name] FROM [" + view.Name + "] WHERE [Name] = @DataRef AND [Type] = 'SOURCE'");
                                                dataRefQuery.Parameters.Add(new QueryParameter("@DataRef", DbType.String, dataRef));
                                                DataTable dataRefTable = db.Select(dataRefQuery);
                                                if (dataRefTable.Rows.Count == 0)
                                                {
                                                    errorList.Add(ImportExportMessageType.Error, "1018", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1018, view.Name, dataRef));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            errorList.Add(ImportExportMessageType.Error, "1019", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1019, view.Name));
                                        }
                                    }
                                }
                            }
                        } // end if wide table
                    } // end if (!string.IsNullOrEmpty(view.TableName) && db.TableExists(view.TableName))

                    if (SetStatus != null)
                    {
                        SetStatus(string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_MESSAGE_ANALYZING_RELATIONS, view.Name));
                    }

                    Dictionary<string, string> relateButtonNames = new Dictionary<string, string>();
                    DataTable relateCheckTable1 = db.Select(db.CreateQuery("SELECT [Name], [Pfonttype] FROM [" + view.Name + "] WHERE [Type] = 'RELATE'"));
                    foreach (DataRow row in relateCheckTable1.Rows)
                    {
                        string relButtonName = row[0].ToString();
                        string relViewName = row[1].ToString();
                        if (!relateButtonNames.ContainsKey(relButtonName))
                        {
                            relateButtonNames.Add(relButtonName, relViewName);
                        }
                        else
                        {
                            errorList.Add(ImportExportMessageType.Error, "2000", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E2000, view.Name, relButtonName));
                        }
                    }

                    DataTable relateCheckTable2 = db.Select(db.CreateQuery("SELECT [Name], [Datatable] FROM [" + view.Name + "] WHERE [Name] LIKE 'RELVIE%' AND [Type] = 'SOURCE'"));
                    foreach (DataRow row in relateCheckTable2.Rows)
                    {
                        string relViewRefName = row[0].ToString();
                        string rViewName = row[1].ToString();
                        if (!relateButtonNames.ContainsValue(relViewRefName))
                        {
                            errorList.Add(ImportExportMessageType.Warning, "5001", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_W5001, relViewRefName, view.Name));
                        }

                        if (string.IsNullOrEmpty(rViewName))
                        {
                            errorList.Add(ImportExportMessageType.Error, "2001", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E2001, relViewRefName, view.Name));
                        }
                        else if (!db.TableExists(rViewName))
                        {
                            errorList.Add(ImportExportMessageType.Error, "2002", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E2002, relViewRefName, view.Name));
                        }
                        else
                        {
                            foreach (Epi.Epi2000.View iView in sourceProject.Views)
                            {
                                if (iView.Name == rViewName && !string.IsNullOrEmpty(iView.TableName))
                                {
                                    bool tableExists = db.TableExists(iView.TableName);
                                    if (tableExists && !db.ColumnExists(iView.TableName, "FKEY"))
                                    {
                                        errorList.Add(ImportExportMessageType.Error, "2003", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E2003, view.Name, rViewName));
                                    }
                                    else if (tableExists)
                                    {
                                        DataTable relateCheckTable3 = db.Select(db.CreateQuery("SELECT FKEY FROM [" + iView.TableName + "] WHERE [FKEY] is NULL"));
                                        if (relateCheckTable3.Rows.Count > 0)
                                        {
                                            errorList.Add(ImportExportMessageType.Error, "2004", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E2004, iView.Name));
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (SetStatus != null)
                    {
                        SetStatus(string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_MESSAGE_ANALYZING_FIELDS, view.Name));
                    }

                    foreach (Epi.Epi2000.Page page in view.Pages)
                    {
                        DataTable fieldsTable = sourceProject.Metadata.GetFieldsOnPageAsDataTable(view.Name, page.Position);

                        foreach (DataRow fieldRow in fieldsTable.Rows)
                        {
                            string fieldName = fieldRow["Name"].ToString();

                            if (Char.IsNumber(fieldName[0]))
                            {
                                errorList.Add(ImportExportMessageType.Error, "3000", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E3000, fieldName, view.Name));
                            }

                            if (fieldName.Contains(" "))
                            {
                                errorList.Add(ImportExportMessageType.Error, "3001", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E3001, fieldName, view.Name));
                            }

                            MetaFieldType fieldType = Epi.Epi2000.MetadataDbProvider.InferFieldType(fieldRow);

                            //if (view.IsWideTableView == false)
                            //{
                            //    if (fieldType != MetaFieldType.CommandButton && fieldType != MetaFieldType.Grid && fieldType != MetaFieldType.Group && fieldType != MetaFieldType.LabelTitle && fieldType != MetaFieldType.Mirror && fieldType != MetaFieldType.Relate &&
                            //    db.ColumnExists(view.TableName, fieldName) == false && db.TableExists(view.TableName))
                            //    {
                            //        problems.Add(new KeyValuePair<ProjectUpgradeProblemType, string>(ProjectUpgradeProblemType.Error, "3002: The field " + fieldName + " in view " + view.Name + " has no corresponding column in the data table."));
                            //    }
                            //}

                            if (fieldType == MetaFieldType.Grid)
                            {
                                errorList.Add(ImportExportMessageType.Warning, "5002", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_W5002, fieldName, view.Name));
                            }
                            else if (fieldType == MetaFieldType.Mirror)
                            {
                                string mirrorSource = fieldRow["Lists"].ToString();

                                if (mirrorSource == fieldName)
                                {
                                    errorList.Add(ImportExportMessageType.Error, "3003", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E3003, fieldName, view.Name));
                                }
                                if (string.IsNullOrEmpty(mirrorSource))
                                {
                                    errorList.Add(ImportExportMessageType.Warning, "5003", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_W5003, fieldName, view.Name));
                                }
                                else
                                {
                                    bool found = false;
                                    foreach (DataRow iRow in sourceProject.Metadata.GetFieldsAsDataTable(view.Name).Rows)
                                    {
                                        if (iRow["Name"].ToString().Equals(mirrorSource))
                                        {
                                            found = true;
                                            break;
                                        }
                                    }
                                    if (!found)
                                    {
                                        errorList.Add(ImportExportMessageType.Warning, "5004", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_W5004, fieldName, view.Name));
                                    }
                                }
                            }
                            else if (fieldType == MetaFieldType.LegalValues || fieldType == MetaFieldType.CommentLegal)
                            {
                                string lists = fieldRow["Lists"].ToString();
                                string valueFieldName = string.Empty;
                                string[] rightSide = lists.Split('(');
                                if (rightSide.Length > 1)
                                {
                                    string[] leftSide = rightSide[1].Split(',');
                                    if (leftSide.Length > 1)
                                    {
                                        valueFieldName = leftSide[1].Trim(')').Trim();
                                        string codeReference = leftSide[0];

                                        Query codeTableCheckQuery = db.CreateQuery("SELECT [Name], [Datatable] FROM [" + view.Name + "] WHERE [Name] = '" + codeReference + "'");
                                        codeTableCheckQuery.Parameters.Add(new QueryParameter("@CodeReference", DbType.String, codeReference));
                                        DataTable codeTableCheck = db.Select(codeTableCheckQuery);
                                        if (codeTableCheck.Rows.Count > 0)
                                        {
                                            string tableName = codeTableCheck.Rows[0]["Datatable"].ToString();
                                            foreach (Epi.Epi2000.View sView in sourceProject.Views)
                                            {
                                                if (!string.IsNullOrEmpty(sView.TableName) && sView.TableName.ToLower() == tableName.ToLower())
                                                {
                                                    errorList.Add(ImportExportMessageType.Error, "3013", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E3013, fieldName, view.Name));
                                                }
                                            }
                                        }
                                    }
                                }                                
                            }
                            else if (fieldType == MetaFieldType.Codes)
                            {
                                string lists = fieldRow["Lists"].ToString();
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

                                string tableName = string.Empty;
                                int indexOf = lists.IndexOf(',');
                                tableName = lists.Substring(indexOf + 1);
                                indexOf = tableName.IndexOf('*');
                                if (indexOf > 0)
                                {
                                    tableName = tableName.Substring(0, indexOf).Trim();
                                }

                                foreach (Epi.Epi2000.View sView in sourceProject.Views)
                                {
                                    if (!string.IsNullOrEmpty(sView.TableName) && sView.TableName.ToLower() == tableName.ToLower())
                                    {
                                        errorList.Add(ImportExportMessageType.Error, "3004", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E3004, fieldName, view.Name));
                                    }
                                }
                            }
                        }
                    }

                    CheckAllFields(view, db);
                }

                db.Dispose();
                db = null;
            }
            catch (Exception ex)
            {
                errorList.Add(ImportExportMessageType.Error, "4000", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E4000, ex.Message));
            }
            finally
            {
                if (SetStatus != null)
                {
                    SetStatus(string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_MESSAGE_FINISHED, sourceProject.Name));
                }
            }
        }

        /// <summary>
        /// Checks the field properties for a given field in an Epi 3.5.x view.
        /// </summary>
        /// <param name="view">The Epi Info 3.5.x view to check.</param>
        /// <param name="fieldRow">The row of field data that represents this field.</param>
        private void CheckFieldProperties(Epi.Epi2000.View view, DataRow fieldRow)
        {
            #region Input Validation
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            if (fieldRow == null)
            {
                throw new ArgumentNullException("fieldRow");
            }
            #endregion // Input Validation

            string fieldName = fieldRow["Name"].ToString();

            MetaFieldType fieldType = Epi.Epi2000.MetadataDbProvider.InferFieldType(fieldRow);

            if (!(fieldType == MetaFieldType.CommandButton ||
                    fieldType == MetaFieldType.Grid ||
                    fieldType == MetaFieldType.Group ||
                    fieldType == MetaFieldType.LabelTitle ||
                    fieldType == MetaFieldType.Mirror ||
                    fieldType == MetaFieldType.Relate) && fieldRow["Lists"] != DBNull.Value)
            {
                string lists = fieldRow["Lists"].ToString().Trim();
                if (fieldType == MetaFieldType.Codes || fieldType == MetaFieldType.LegalValues || fieldType == MetaFieldType.CommentLegal)
                {
                    if (lists.Contains("[") || lists.Contains("]"))
                    {
                        errorList.Add(ImportExportMessageType.Error, "3010", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E3010, fieldName, view.Name));
                    }
                    else if (!lists.Contains("(") || !lists.Contains(")"))
                    {
                        errorList.Add(ImportExportMessageType.Error, "3011", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E3011, fieldName, view.Name));
                    }
                    else
                    {
                        int parenIndex = lists.IndexOf('(');
                        string properties = lists.Substring(0, parenIndex);
                        if (properties.Contains("N") && properties.Contains("M"))
                        {
                            errorList.Add(ImportExportMessageType.Error, "3012", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E3012, fieldName, view.Name));
                        }
                    }
                }
            }

            if (fieldType == MetaFieldType.Relate)
            {
                if (fieldRow["FormatString"] != DBNull.Value && !string.IsNullOrEmpty(fieldRow["FormatString"].ToString().Trim()))
                {
                    errorList.Add(ImportExportMessageType.Notification, "6000", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_N6000, fieldName, view.Name));
                }
            }
        }

        /// <summary>
        /// Checks a given Epi Info 3.5.x view for source reference problems.
        /// </summary>
        /// <param name="view">The Epi Info 3.5.x view to check.</param>
        private void CheckViewSourceReferences(Epi.Epi2000.View view)
        {
            #region Input Validation
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            #endregion // Input Validation

            Epi.Data.IDbDriver db = sourceProject.CollectedData.GetDatabase();
            DataTable viewMetaTable = db.Select(db.CreateQuery("SELECT [Name], [Type], [Datatable] FROM [" + view.Name + "] WHERE [Type] = 'SOURCE' AND [Name] LIKE 'CODE%'"));

            foreach (DataRow row in viewMetaTable.Rows)
            {
                string dataTableName = row["Datatable"].ToString();
                string sourceName = row["Name"].ToString();

                DataTable viewMetaTable1 = db.Select(db.CreateQuery("SELECT [Name], [Lists] FROM [" + view.Name + "] WHERE [Lists] LIKE '%" + sourceName + ",%'"));
                if (viewMetaTable1.Rows.Count == 0)
                {
                    // There are a lot of these, and they have no effect on the upgrade process, so skip them... they're pointless to show to the user.
                    //problems.Add(new KeyValuePair<ProjectUpgradeProblemType, string>(ProjectUpgradeProblemType.Notification, "View " + view.Name + " contains a SOURCE reference called " + sourceName + " that does not have a corresponding field."));
                }
                else
                {
                    if (string.IsNullOrEmpty(dataTableName))
                    {
                        errorList.Add(ImportExportMessageType.Error, "1023", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1023, view.Name, sourceName));
                    }
                    else if (!db.TableExists(dataTableName))
                    {
                        errorList.Add(ImportExportMessageType.Error, "1024", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1024, view.Name, sourceName, dataTableName, dataTableName));
                    }
                }
            }
        }

        /// <summary>
        /// Checks the columns in the view table for problems.
        /// </summary>
        /// <param name="view">The Epi Info 3.5.x view to check</param>
        private void CheckViewColumns(Epi.Epi2000.View view)
        {
            #region Input Validation
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            #endregion // Input Validation

            Epi.Data.IDbDriver db = sourceProject.CollectedData.GetDatabase();
            DataTable viewMetaTable = db.Select(db.CreateQuery("SELECT * FROM [" + view.Name + "]"));

            if (!viewMetaTable.Columns.Contains("PageNumber"))
            {
                errorList.Add(ImportExportMessageType.Error, "1030", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1030, view.Name));
            }
            else if (!(viewMetaTable.Columns["PageNumber"].DataType.ToString().Equals("System.Byte") || viewMetaTable.Columns["PageNumber"].DataType.ToString().Equals("System.Int16") || viewMetaTable.Columns["PageNumber"].DataType.ToString().Equals("System.Int32")))
            {
                errorList.Add(ImportExportMessageType.Error, "1031", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1031, view.Name, viewMetaTable.Columns["PageNumber"].DataType.ToString()));
            }

            if (!viewMetaTable.Columns.Contains("Name"))
            {
                errorList.Add(ImportExportMessageType.Error, "1032", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1032, view.Name));
            }
            else if (!viewMetaTable.Columns["Name"].DataType.ToString().Equals("System.String"))
            {
                errorList.Add(ImportExportMessageType.Error, "1033", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1033, view.Name, viewMetaTable.Columns["Name"].DataType.ToString()));
            }

            if (!viewMetaTable.Columns.Contains("Prompt"))
            {
                errorList.Add(ImportExportMessageType.Error, "1034", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1034, view.Name));
            }
            else if (!viewMetaTable.Columns["Prompt"].DataType.ToString().Equals("System.String"))
            {
                errorList.Add(ImportExportMessageType.Error, "1035", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1035, view.Name, viewMetaTable.Columns["Prompt"].DataType.ToString()));
            }

            if (!viewMetaTable.Columns.Contains("Type"))
            {
                errorList.Add(ImportExportMessageType.Error, "1036", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1036, view.Name));
            }
            else if (!viewMetaTable.Columns["Type"].DataType.ToString().Equals("System.String"))
            {
                errorList.Add(ImportExportMessageType.Error, "1037", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1037, view.Name, viewMetaTable.Columns["Type"].DataType.ToString()));
            }

            if (!viewMetaTable.Columns.Contains("Database"))
            {
                errorList.Add(ImportExportMessageType.Error, "1070", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1070, view.Name));
            }
            else if (!viewMetaTable.Columns["Database"].DataType.ToString().Equals("System.String"))
            {
                errorList.Add(ImportExportMessageType.Error, "1071", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1071, view.Name, viewMetaTable.Columns["Database"].DataType.ToString()));
            }

            if (!viewMetaTable.Columns.Contains("Datafield"))
            {
                errorList.Add(ImportExportMessageType.Error, "1072", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1072, view.Name));
            }
            else if (!viewMetaTable.Columns["Datafield"].DataType.ToString().Equals("System.String"))
            {
                errorList.Add(ImportExportMessageType.Error, "1073", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1073, view.Name, viewMetaTable.Columns["Datafield"].DataType.ToString()));
            }

            if (!viewMetaTable.Columns.Contains("Datatable"))
            {
                errorList.Add(ImportExportMessageType.Error, "1074", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1074, view.Name));
            }
            else if (!viewMetaTable.Columns["Datatable"].DataType.ToString().Equals("System.String"))
            {
                errorList.Add(ImportExportMessageType.Error, "1075", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E1075, view.Name, viewMetaTable.Columns["Datatable"].DataType.ToString()));
            }
        }

        /// <summary>
        /// Checks all the fields in a given Epi Info 3.5.x view for generic problems.
        /// </summary>
        /// <param name="view">The Epi Info 3.5.x view</param>
        /// <param name="db">The data driver associated with the view's project</param>
        private void CheckAllFields(Epi.Epi2000.View view, Epi.Data.IDbDriver db)
        {
            #region Input Validation
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            #endregion // Input Validation

            List<DataTable> wideTables = new List<DataTable>();

            bool hasTables = false;

            foreach (string wideTableName in view.TableNames)
            {
                if (!string.IsNullOrEmpty(wideTableName) && db.TableExists(wideTableName))
                {
                    DataTable wTable = db.GetTopTwoTable(wideTableName);
                    wTable.TableName = wideTableName;
                    wideTables.Add(wTable);
                    hasTables = true;
                }
            }

            DataTable allFieldsTable = sourceProject.Metadata.GetFieldsAsDataTable(view.Name);
            foreach (DataRow fRow in allFieldsTable.Rows)
            {
                string fieldName = fRow["Name"].ToString();

                CheckFieldProperties(view, fRow);

                MetaFieldType fieldType = Epi.Epi2000.MetadataDbProvider.InferFieldType(fRow);
                if (fieldType == MetaFieldType.CommandButton ||
                    fieldType == MetaFieldType.Grid ||
                    fieldType == MetaFieldType.Group ||
                    fieldType == MetaFieldType.LabelTitle ||
                    fieldType == MetaFieldType.Mirror ||
                    fieldType == MetaFieldType.Relate)
                {
                    continue;
                }

                if (!hasTables)
                {
                    continue;
                }

                string columnDataType = string.Empty;
                string wideTableName = string.Empty;

                foreach (DataTable wideTable in wideTables)
                {
                    if (wideTable.Columns.Contains(fieldName))
                    {
                        columnDataType = wideTable.Columns[fieldName].DataType.ToString();
                        wideTableName = wideTable.TableName;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(columnDataType))
                {
                    if (fieldType != MetaFieldType.CommandButton && fieldType != MetaFieldType.Grid && fieldType != MetaFieldType.Group && fieldType != MetaFieldType.LabelTitle && fieldType != MetaFieldType.Mirror && fieldType != MetaFieldType.Relate)
                    {
                        errorList.Add(ImportExportMessageType.Error, "3002", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E3002, fieldName, view.Name));
                    }
                }
                else
                {
                    switch (fieldType)
                    {
                        case MetaFieldType.Checkbox:
                            if (!columnDataType.Equals("System.Boolean"))
                            {
                                errorList.Add(ImportExportMessageType.Error, "3005", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E3005, fieldName, view.Name, columnDataType));
                            }
                            break;
                        case MetaFieldType.YesNo:
                            if (!(columnDataType.Equals("System.Byte") || columnDataType.Equals("System.Byte")))
                            {
                                errorList.Add(ImportExportMessageType.Error, "3006", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E3006, fieldName, view.Name, columnDataType));
                            }
                            break;
                        case MetaFieldType.Codes:
                        case MetaFieldType.CommentLegal:
                        case MetaFieldType.LegalValues:
                        case MetaFieldType.Text:
                        case MetaFieldType.TextUppercase:
                            if (!columnDataType.Equals("System.String"))
                            {
                                errorList.Add(ImportExportMessageType.Error, "3007", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E3007, fieldName, view.Name, columnDataType));
                            }
                            break;
                        case MetaFieldType.Number:
                            if (!columnDataType.Equals("System.Single") && !columnDataType.Equals("System.Double"))
                            {
                                errorList.Add(ImportExportMessageType.Error, "3008", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E3008, fieldName, view.Name, columnDataType));
                            }
                            break;
                        case MetaFieldType.Date:
                        case MetaFieldType.DateTime:
                        case MetaFieldType.Time:
                            if (!columnDataType.Equals("System.DateTime"))
                            {
                                errorList.Add(ImportExportMessageType.Error, "3009", string.Format(ImportExportSharedStrings.UPGRADE_PROBLEM_CHECK_ERROR_E3009, fieldName, view.Name, columnDataType));
                            }
                            break;
                    }
                }
            }
        }
        #endregion // Methods
    }
}
