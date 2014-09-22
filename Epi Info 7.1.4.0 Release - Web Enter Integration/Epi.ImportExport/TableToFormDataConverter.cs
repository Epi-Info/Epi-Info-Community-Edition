using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Epi;
using Epi.Core;
using Epi.Data;

namespace Epi.ImportExport
{
    /// <summary>
    /// A class for converting non-Epi Info 7 data sources into Epi Info 7 projects.
    /// </summary>
    /// <remarks>
    /// This class only imports the data after the data table has been converted into an Epi Info 7 form. In the long term,
    /// the code that does the conversion should be moved to this class.
    /// </remarks>
    public class TableToFormDataConverter : IDisposable
    {
        #region Private Members
        /// <summary>
        /// The data driver for the data to be converted
        /// </summary>
        private IDbDriver sourceDriver;

        /// <summary>
        /// The data driver for the project
        /// </summary>
        private IDbDriver destinationDriver;

        /// <summary>
        /// The project that will contain the converted data
        /// </summary>
        private Project project;

        /// <summary>
        /// The view in the project that will contain the converted data
        /// </summary>
        private View destinationView;

        /// <summary>
        /// Table name in the source data set
        /// </summary>
        private string tableName;

        /// <summary>
        /// List of columns in the source data set to import
        /// </summary>
        private List<string> columnNames;

        /// <summary>
        /// The column mappings set up in the user interface.
        /// </summary>
        List<ColumnConversionInfo> columnMapping;

        /// <summary>
        /// Used for diagnostic purposes.
        /// </summary>
        private System.Diagnostics.Stopwatch stopwatch;

        /// <summary>
        /// The top-two table used to get column data types
        /// </summary>
        private DataTable DT;

        /// <summary>
        /// Used for multi-threading
        /// </summary>
        private ManualResetEvent[] doneEvents;

        /// <summary>
        /// Connection to the database
        /// </summary>
        IDbConnection conn;
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
        /// <param name="pProject">The project that will contain the converted metadata</param>
        /// <param name="pDestinationView">The form within the project that will accept the converted data</param>
        /// <param name="pSourceDriver">The data driver for the external data source</param>
        /// <param name="pTableName">The name of the table within the external data source</param>
        /// <param name="pColumnMapping">The column mappings that determine how the fields in the form will be created</param>
        public TableToFormDataConverter(Project pProject, View pDestinationView, IDbDriver pSourceDriver, string pTableName, List<ColumnConversionInfo> pColumnMapping)
        {
            project = pProject;
            destinationView = pDestinationView;
            sourceDriver = pSourceDriver;
            tableName = pTableName;
            columnMapping = pColumnMapping;

            columnNames = new List<string>();
            foreach (ColumnConversionInfo cci in columnMapping)
            {
                columnNames.Add(cci.SourceColumnName);
            }

            destinationDriver = project.CollectedData.GetDatabase();
        }
        #endregion // Constructors

        #region Private Methods
        /// <summary>
        /// Gets the Access version of a generic DbType
        /// </summary>
        /// <returns>Access version of the generic DbType</returns>
        private OleDbType CovertToNativeDbType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                    return OleDbType.VarChar;
                case DbType.AnsiStringFixedLength:
                    return OleDbType.Char;
                case DbType.Binary:
                    return OleDbType.Binary;
                case DbType.Boolean:
                    return OleDbType.Boolean;
                case DbType.Byte:
                    return OleDbType.UnsignedTinyInt;
                case DbType.Currency:
                    return OleDbType.Currency;
                case DbType.Date:
                    return OleDbType.DBDate;
                case DbType.DateTime:
                    return OleDbType.DBTimeStamp;
                case DbType.Decimal:
                    return OleDbType.Decimal;
                case DbType.Double:
                    return OleDbType.Double;
                case DbType.Guid:
                    return OleDbType.Guid;
                case DbType.Int16:
                    return OleDbType.SmallInt;
                case DbType.Int32:
                    return OleDbType.Integer;
                case DbType.Int64:
                    return OleDbType.BigInt;
                case DbType.Object:
                    //  return OleDbType.VarChar;
                    return OleDbType.Binary;
                case DbType.SByte:
                    return OleDbType.TinyInt;
                case DbType.Single:
                    return OleDbType.Single;
                case DbType.String:
                    return OleDbType.VarWChar;
                case DbType.StringFixedLength:
                    return OleDbType.WChar;
                case DbType.Time:
                    return OleDbType.DBTimeStamp;
                case DbType.UInt16:
                    return OleDbType.UnsignedSmallInt;
                case DbType.UInt32:
                    return OleDbType.UnsignedInt;
                case DbType.UInt64:
                    return OleDbType.UnsignedBigInt;
                case DbType.VarNumeric:
                    return OleDbType.VarNumeric;
                default:
                    return OleDbType.VarChar;
            }
        }

        /// <summary>
        /// Returns a native equivalent of a DbParameter
        /// </summary>
        /// <returns>Native equivalent of a DbParameter</returns>
        private OleDbParameter ConvertToNativeParameter(QueryParameter parameter)
        {
            if (parameter.DbType.Equals(DbType.Guid))
            {
                parameter.Value = new Guid(parameter.Value.ToString());
            }

            OleDbParameter param = new OleDbParameter
                (
                    parameter.ParameterName,
                    CovertToNativeDbType(parameter.DbType),
                    parameter.Size,
                    parameter.Direction,
                    parameter.IsNullable,
                    parameter.Precision,
                    parameter.Scale,
                    parameter.SourceColumn,
                    parameter.SourceVersion,
                    parameter.Value
                );

            return param;
        }

        /// <summary>
        /// Gets a new command using an existing connection
        /// </summary>
        /// <param name="sqlStatement">The query to be executed against the database</param>
        /// <param name="connection">Parameters for the query to be executed</param>
        /// <param name="parameters">An OleDb command object</param>
        /// <returns></returns>
        private IDbCommand GetCommand(string sqlStatement, IDbConnection connection, List<QueryParameter> parameters)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(sqlStatement))
            {
                throw new ArgumentNullException("sqlStatement");
            }
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            #endregion

            IDbCommand command = connection.CreateCommand();
            command.CommandText = sqlStatement;

            foreach (QueryParameter parameter in parameters)
            {
                command.Parameters.Add(this.ConvertToNativeParameter(parameter));
            }

            return command;
        }

        /// <summary>
        /// Assigns GUIDs to the form's base table and all page tables
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        private void AssignGUIDs(int min, int max)
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                for (int i = min; i < max; i++)
                {
                    DataRow row = DT.Rows[i];
                    string GUID = row["GlobalRecordId"].ToString();
                    StringBuilder sb = new StringBuilder();
                    sb.Append(" insert into ");
                    sb.Append(destinationDriver.InsertInEscape(destinationView.TableName));
                    sb.Append(StringLiterals.SPACE);
                    sb.Append(StringLiterals.SPACE);
                    sb.Append("([GlobalRecordId])");
                    sb.Append(" values (");
                    sb.Append("'" + GUID + "'");
                    sb.Append(") ");
                    Epi.Data.Query insertQuery = destinationDriver.CreateQuery(sb.ToString());

                    if (project.CollectedDataDriver.ToLower().Contains("epi.data.office"))
                    {
                        IDbCommand command = GetCommand(insertQuery.SqlStatement, conn, insertQuery.Parameters);
                        object obj = command.ExecuteNonQuery();
                    }
                    else
                    {
                        destinationDriver.ExecuteNonQuery(insertQuery);
                    }

                    if (SetProgressBar != null)
                    {
                        SetProgressBar(1);
                    }

                    foreach (Page destinationPage in destinationView.Pages)
                    {
                        sb = new StringBuilder();
                        sb.Append(" insert into ");
                        sb.Append(destinationDriver.InsertInEscape(destinationPage.TableName));
                        sb.Append(StringLiterals.SPACE);
                        sb.Append("([GlobalRecordId])");
                        sb.Append(" values (");
                        sb.Append("'" + GUID + "'");
                        sb.Append(") ");
                        insertQuery = destinationDriver.CreateQuery(sb.ToString());

                        if (project.CollectedDataDriver.ToLower().Contains("epi.data.office"))
                        {
                            IDbCommand command = GetCommand(insertQuery.SqlStatement, conn, insertQuery.Parameters);
                            object obj = command.ExecuteNonQuery();
                        }
                        else
                        {
                            destinationDriver.ExecuteNonQuery(insertQuery);
                        }
                    }

                    if (CheckForCancellation != null)
                    {
                        bool cancelled = CheckForCancellation();
                        if (cancelled)
                        {
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                conn.Dispose();
                throw ex;
            }
            finally
            {

            }
        }

        /// <summary>
        /// Imports page data
        /// </summary>
        /// <param name="destinationPage">The page to import the data into</param>
        private void ImportPage(Page destinationPage)
        {
            foreach (DataRow row in DT.Rows)
            {
                if (CheckForCancellation != null)
                {
                    bool cancelled = CheckForCancellation();
                    if (cancelled)
                    {
                        return;
                    }
                }

                string GUID = row["GlobalRecordId"].ToString();

                StringBuilder sb = new StringBuilder();
                List<QueryParameter> paramList = new List<QueryParameter>();
                int fieldsInQuery = 0;

                sb.Append(SqlKeyWords.UPDATE);
                sb.Append(StringLiterals.SPACE);
                sb.Append(destinationDriver.InsertInEscape(destinationPage.TableName));
                sb.Append(StringLiterals.SPACE);
                sb.Append(SqlKeyWords.SET);
                sb.Append(StringLiterals.SPACE);

                foreach (ColumnConversionInfo cci in columnMapping)
                {
                    DataColumn dc = DT.Columns[cci.SourceColumnName];
                    if (dc.ColumnName.Equals("GlobalRecordId"))
                    {
                        continue;
                    }

                    string mappedField = cci.DestinationColumnName;

                    if (row[dc] == DBNull.Value || string.IsNullOrEmpty(row[dc].ToString()) || !destinationPage.Fields.Contains(mappedField))
                    {
                        continue;
                    }

                    QueryParameter param = GetQueryParameterForField(cci, row[dc]);
                    if (param != null)
                    {
                        paramList.Add(param);
                        fieldsInQuery++;

                        sb.Append(StringLiterals.SPACE);
                        sb.Append(StringLiterals.LEFT_SQUARE_BRACKET);
                        sb.Append(mappedField);
                        sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET);
                        sb.Append(StringLiterals.EQUAL);
                        sb.Append(StringLiterals.COMMERCIAL_AT);
                        sb.Append(mappedField);
                        sb.Append(StringLiterals.COMMA);
                    }
                }

                sb = new StringBuilder(sb.ToString().TrimEnd(','));

                sb.Append(StringLiterals.SPACE);
                sb.Append(SqlKeyWords.WHERE);
                sb.Append(StringLiterals.SPACE);
                sb.Append(destinationDriver.InsertInEscape(ColumnNames.GLOBAL_RECORD_ID));
                sb.Append(StringLiterals.EQUAL);
                sb.Append("'");
                sb.Append(GUID);
                sb.Append("'");

                if (fieldsInQuery > 0 && paramList != null && paramList.Count > 0)
                {
                    Query updateQuery = destinationDriver.CreateQuery(sb.ToString());
                    foreach (QueryParameter param in paramList)
                    {
                        updateQuery.Parameters.Add(param);
                    }
                    //destinationDriver.ExecuteNonQuery(updateQuery);

                    if (project.CollectedDataDriver.ToLower().Contains("epi.data.office"))
                    {
                        IDbCommand command = GetCommand(updateQuery.SqlStatement, conn, updateQuery.Parameters);
                        object obj = command.ExecuteNonQuery();
                    }
                    else
                    {
                        destinationDriver.ExecuteNonQuery(updateQuery);
                    }

                    if (SetProgressBar != null)
                    {
                        SetProgressBar(1);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the appropriate QueryParameter for a given column and value.
        /// </summary>
        /// <param name="cci">ColumnConversionInfo</param>
        /// <param name="value">Raw value</param>
        /// <returns>QueryParameter</returns>
        private QueryParameter GetQueryParameterForField(ColumnConversionInfo cci, object value)
        {
            String fieldName = cci.DestinationColumnName;

            // Scenario 1:
            // Source column is read as text
            // User elects to convert this column to numeric
            // Must specify numeric query param...

            // Thus, do some conversion based on allowable conversions.

            switch (cci.FieldType)
            {
                    // User can elect to use checkbox fields for boolean and number columns
                case MetaFieldType.Checkbox:
                    if (value is bool)
                    {
                        return new QueryParameter("@" + fieldName, DbType.Boolean, value);
                    }
                    else if (value.ToString() == "1" || value.ToString() == "-1")
                    {
                        return new QueryParameter("@" + fieldName, DbType.Boolean, true);
                    }
                    else
                    {
                        return new QueryParameter("@" + fieldName, DbType.Boolean, false);
                    }
                    // User can elect to use Yes/No fields for boolean and number columns
                case MetaFieldType.YesNo:
                    if (value.ToString() == "1" || value.ToString() == "-1" || value.ToString().ToLower() == "true")
                    {
                        return new QueryParameter("@" + fieldName, DbType.Byte, 1);
                    }
                    else if (value.ToString() == "0" || value.ToString().ToLower() == "false")
                    {
                        return new QueryParameter("@" + fieldName, DbType.Byte, 0);
                    }
                    else
                    {
                        return new QueryParameter("@" + fieldName, DbType.Byte, DBNull.Value);
                    }
                    // User can elect to use Text fields for boolean, number, DateTime and text columns
                case MetaFieldType.Text:
                    if (value is double || value is int || value is DateTime || value is bool)
                    {                        
                        return new QueryParameter("@" + fieldName, DbType.String, value.ToString());
                    }
                    else
                    {
                        return new QueryParameter("@" + fieldName, DbType.String, value);
                    }                    
                case MetaFieldType.Multiline:
                    return new QueryParameter("@" + fieldName, DbType.String, value);
                case MetaFieldType.Date:
                case MetaFieldType.DateTime:
                case MetaFieldType.Time:
                    return new QueryParameter("@" + fieldName, DbType.DateTime, value);
                case MetaFieldType.LegalValues:
                    return new QueryParameter("@" + fieldName, DbType.String, value);
                case MetaFieldType.Number:
                    if (value is string)
                    {
                        double dValue;
                        bool success = double.TryParse(value.ToString(), out dValue);
                        if (success)
                        {
                            return new QueryParameter("@" + fieldName, DbType.Single, dValue);
                        }
                        else
                        {
                            return new QueryParameter("@" + fieldName, DbType.Single, DBNull.Value);
                        }
                    }
                    else
                    {
                        return new QueryParameter("@" + fieldName, DbType.Single, value);
                    }                    
                default:
                    throw new ApplicationException("Invalid field type");
                //break;
            }    

            //switch (cci.SourceColumnType.ToString())
            //{
            //    case "System.String":
            //        return new QueryParameter("@" + fieldName, DbType.String, value);
            //    case "System.Byte":
            //        return new QueryParameter("@" + fieldName, DbType.Byte, value);
            //    case "System.DateTime":
            //        return new QueryParameter("@" + fieldName, DbType.DateTime, value);
            //    case "System.Int16":
            //        return new QueryParameter("@" + fieldName, DbType.Int16, value);
            //    case "System.Int32":
            //        return new QueryParameter("@" + fieldName, DbType.Int32, value);
            //    case "System.Int64":
            //        return new QueryParameter("@" + fieldName, DbType.Int64, value);
            //    case "System.Single":
            //        return new QueryParameter("@" + fieldName, DbType.Single, value);
            //    case "System.Double":
            //        return new QueryParameter("@" + fieldName, DbType.Double, value);
            //    case "System.Decimal":
            //        return new QueryParameter("@" + fieldName, DbType.Decimal, value);
            //    case "System.Boolean":
            //        if (destinationView.Fields.Contains(fieldName) && destinationView.Fields[fieldName] is Epi.Fields.YesNoField)
            //        {
            //            Epi.Fields.Field field = destinationView.Fields[fieldName] as Epi.Fields.YesNoField;
            //            if ((bool)value == true)
            //            {
            //                return new QueryParameter("@" + fieldName, DbType.Byte, 1);
            //            }
            //            else if ((bool)value == false)
            //            {
            //                return new QueryParameter("@" + fieldName, DbType.Byte, 0);
            //            }
            //            else
            //            {
            //                return null;
            //            }
            //        }
            //        else
            //            return new QueryParameter("@" + fieldName, DbType.Boolean, value);
            //    //default:
            //        //throw new ApplicationException("Not a supported field type");
            //}

            //return null;
        }
        #endregion // Private Methods

        #region Public Methods
        /// <summary>
        /// Start the conversion process
        /// </summary>
        public void Convert()
        {
            #region Debug
            stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            System.Diagnostics.Debug.Print(DateTime.Now + " :  " + "Table-to-form GUID creation initiated. Elapsed: " + stopwatch.Elapsed.TotalSeconds.ToString());
            #endregion // Debug

            int artifacts = 0;

            if (SetStatus != null)
            {
                SetStatus("Getting data...");
            }

            DT = sourceDriver.GetTableData(tableName, columnNames);
            
            artifacts = (DT.Rows.Count * destinationView.Pages.Count) + DT.Rows.Count;

            if (SetMaxProgressBarValue != null)
            {
                SetMaxProgressBarValue((double)artifacts);
            }

            #region Set up global record ID values
            if (SetStatus != null)
            {
                SetStatus("Starting global record ID generation...");
            }

            DT.Columns.Add(new DataColumn("GlobalRecordId", typeof(string)));

            int currentRow = 0;
            foreach (DataRow row in DT.Rows)
            {
                string GUID = System.Guid.NewGuid().ToString();
                row["GlobalRecordId"] = GUID;
                currentRow++;
            }

            System.Diagnostics.Debug.Print(DateTime.Now + " :  " + "All GUIDs generated. Starting GUID assignment. Elapsed: " + stopwatch.Elapsed.TotalSeconds.ToString());            

            // set PK
            DataColumn[] parentPrimaryKeyColumns = new DataColumn[1];
            parentPrimaryKeyColumns[0] = DT.Columns["GlobalRecordId"];
            DT.PrimaryKey = parentPrimaryKeyColumns;            

            int min = 0;            
            int max = DT.Rows.Count;
            int q = max / 4;
            int Q1 = min + q;
            int Q2 = Q1 + q;
            int Q3 = max - q;

            conn = destinationDriver.GetConnection();
            conn.Open();

            doneEvents = new ManualResetEvent[4];

            Debug.Print("Launching 4 GUID assignment tasks...");
            GUIDState guidStateInfo;
            
            doneEvents[0] = new ManualResetEvent(false);
            guidStateInfo = new GUIDState(doneEvents[0], 0, 0, Q1);
            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolGUIDAssignCallback), guidStateInfo);

            doneEvents[1] = new ManualResetEvent(false);
            guidStateInfo = new GUIDState(doneEvents[1], 1, Q1, Q2);
            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolGUIDAssignCallback), guidStateInfo);

            doneEvents[2] = new ManualResetEvent(false);
            guidStateInfo = new GUIDState(doneEvents[2], 2, Q2, Q3);
            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolGUIDAssignCallback), guidStateInfo);

            doneEvents[3] = new ManualResetEvent(false);
            guidStateInfo = new GUIDState(doneEvents[3], 3, Q3, max);
            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolGUIDAssignCallback), guidStateInfo);            

            WaitHandle.WaitAll(doneEvents);
            #endregion // Set up global record ID values

            doneEvents = null;

            if (SetStatus != null)
            {
                SetStatus("Populating field data...");
            }

            System.Diagnostics.Debug.Print(DateTime.Now + " :  " + "All GUIDs assigned. Starting page data import. Elapsed: " + stopwatch.Elapsed.TotalSeconds.ToString());

            int pageCount = destinationView.Pages.Count;            

            doneEvents = new ManualResetEvent[pageCount];

            Debug.Print("Launch {0} tasks...", pageCount);
            ImportState stateInfo;
            for (int i = 0; i < pageCount; i++)
            {
                doneEvents[i] = new ManualResetEvent(false);
                stateInfo = new ImportState(destinationView.Pages[i], doneEvents[i], i);
                ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolPageImportCallback), stateInfo);
            }

            WaitHandle.WaitAll(doneEvents);

            conn.Close();
            conn.Dispose();
            conn = null;
            
            stopwatch.Stop();
            System.Diagnostics.Debug.Print(DateTime.Now + " :  " + "Processing complete. Elapsed: " + stopwatch.Elapsed.TotalSeconds.ToString());
        }

        public void ThreadPoolGUIDAssignCallback(Object threadContext)
        {
            GUIDState stateInfo = (GUIDState)threadContext;            
            ManualResetEvent manualEvent = stateInfo.manualEvent;
            int threadIndex = stateInfo.threadIndex;
            int min = stateInfo.min;
            int max = stateInfo.max;

            Debug.Print("Thread started for GUID assign...");
            AssignGUIDs(min, max);
            Debug.Print("Thread completed for GUID assign...");
            manualEvent.Set();
        }

        public void ThreadPoolPageImportCallback(Object threadContext)
        {
            ImportState stateInfo = (ImportState)threadContext;
            Page page = stateInfo.page; //destinationView.Pages[threadIndex];
            ManualResetEvent manualEvent = stateInfo.manualEvent;
            int threadIndex = stateInfo.threadIndex;
            
            Debug.Print("Thread started for page {0}...", page.Name);
            ImportPage(page);
            Debug.Print("Thread completed for page {0}...", page.Name);
            manualEvent.Set();
        }
        #endregion // Public Methods        

        public void Dispose()
        {
            if (conn != null)
            {
                conn.Close();
                conn.Dispose();
                conn = null;
            }
        }

        // Maintain state to pass
        class ImportState
        {
            public Page page;            
            public ManualResetEvent manualEvent;
            public int threadIndex;

            public ImportState(Page pPage, ManualResetEvent pManualEvent, int pThreadIndex)
            {
                this.page = pPage;
                this.threadIndex = pThreadIndex;
                this.manualEvent = pManualEvent;
            }
        }

        // Maintain state to pass
        class GUIDState
        {            
            public ManualResetEvent manualEvent;
            public int threadIndex;
            public int min;
            public int max;

            public GUIDState(ManualResetEvent pManualEvent, int pThreadIndex, int pMin, int pMax)
            {                
                this.threadIndex = pThreadIndex;
                this.manualEvent = pManualEvent;
                this.min = pMin;
                this.max = pMax;
            }
        }
    }
}
