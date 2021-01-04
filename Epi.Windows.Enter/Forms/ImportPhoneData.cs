using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Epi;
using Epi.Data;
using Epi.Fields;
using Epi.Web.Common;
using Epi.Web.Common.Message;
using PortableDevices;

namespace Epi.Enter.Forms
{
    /// <summary>
    /// A user interface element to update or append records to the currently-open form from data that came from an
    /// Android phone and fits in the epi 7 phone data package XML format.
    /// </summary>
    public partial class ImportPhoneDataForm : Form
    {
        #region Private Members
        private bool isBatchImport = false; // currently unused
        //private Project sourceProject;
        //private View sourceView;
        private Project destinationProject;
        private View destinationView;
        //private IDbDriver sourceProjectDataDriver;
        private IDbDriver destinationProjectDataDriver;
        private Configuration config;
        //private int lastRecordId;
        private BackgroundWorker importWorker;
        private static object syncLock = new object();
        private Stopwatch stopwatch;
        private bool update = true;
        private bool append = true;
        private bool importFinished = false;
        private Dictionary<string, List<PhoneFieldData>> _surveyResponses;
        private Dictionary<string, string> surveyGUIDs;
        private List<string> packagePaths;
        // private List<string> RelatedViews;
        #endregion // Private Members

        #region Delegates
        private delegate void SetStatusDelegate(string statusMessage);
        #endregion // Delegates

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public ImportPhoneDataForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor
        /// </summary>        
        /// <param name="destinationView">The destination form; should be the currently-open view</param>
        public ImportPhoneDataForm(View destinationView)
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
            this.stopwatch = new Stopwatch();
            this.destinationProjectDataDriver = destinationProject.CollectedData.GetDbDriver();
            this.config = Configuration.GetNewInstance();
            //this.destinationGUIDList = new List<string>();

            this.importWorker = new BackgroundWorker();
            this.importWorker.WorkerSupportsCancellation = true;

            this.IsBatchImport = false;

            List<View> thisList = new List<View>();
            var xViewDefault = new object();

            foreach (View xView in destinationProject.views)
            {

                thisList.Add(xView);

                if (xView.Name == destinationView.Name)
                    xViewDefault = xView;

                //  viewsList.SelectedItem = xView;


                // viewsList.Items.Add(xView);        
            }

            viewsList.DataSource = thisList;

            viewsList.DisplayMember = "Name";

            viewsList.SelectedItem = xViewDefault;


            //this.cmbImportType.SelectedIndex = 2;  //Append records only
            rdbUpdateAndAppend.Checked = true;

            //this.cmbImportType.Enabled = false;
          //  RelatedViews = new List<string>();
        }

        /// <summary>
        /// Stops an import
        /// </summary>
        private void StopImport()
        {
            btnCancel.Enabled = true;
            btnOK.Enabled = true;
            rdbUpdateAndAppend.Enabled = true;
            rdbUpdate.Enabled = true;
            rdbAppend.Enabled = true;
            txtPhoneDataFile.Enabled = true;
            progressBar.Visible = false;
            progressBar.Style = ProgressBarStyle.Continuous;

            importFinished = true;

            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Initiates a full import on a single form
        /// </summary>
        private void DoImport(int recordCount)
        {
            try
            {
                // int recordCount = Result.SurveyResponseList.Count; // sourceView.GetRecordCount();
                // int gridRowCount = 0;

                // Grids not supported

                //foreach (GridField gridField in sourceView.Fields.GridFields)
                //{
                //    IDataReader reader = sourceProjectDataDriver.GetTableDataReader(gridField.TableName);
                //    while (reader.Read())
                //    {
                //        gridRowCount++;
                //    }
                //}

                progressBar.Maximum = recordCount * (destinationView.Pages.Count + 1);
                progressBar.Maximum = progressBar.Maximum;// +gridRowCount;

                // Assume the web form == windows form for now
                //if (FormsAreAlike())
                //{
                //List<View> viewsToProcess = new List<View>();

                // No related data in web forms
                //foreach (View view in sourceProject.Views)
                //{
                //    try
                //    {
                //        if (view.IsRelatedView && !string.IsNullOrEmpty(view.TableName) && sourceProjectDataDriver.TableExists(view.TableName) && destinationProject.Views.Contains(view.Name))
                //        {
                //            if (IsDescendant(view, sourceView) && !viewsToProcess.Contains(view))
                //            {
                //                viewsToProcess.Add(view);
                //                progressBar.Maximum = progressBar.Maximum + (view.GetRecordCount() * (view.Pages.Count + 1));
                //                int relatedGridRowCount = 0;
                //                foreach (GridField relatedGridField in view.Fields.GridFields)
                //                {
                //                    IDataReader relatedGridReader = sourceProjectDataDriver.GetTableDataReader(relatedGridField.TableName);
                //                    while (relatedGridReader.Read())
                //                    {
                //                        relatedGridRowCount++;
                //                    }
                //                }
                //                progressBar.Maximum = progressBar.Maximum + relatedGridRowCount;
                //            }
                //        }
                //    }
                //    catch (NullReferenceException)
                //    {
                //        continue;
                //    }
                //}

                stopwatch = new Stopwatch();
                stopwatch.Start();

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


                AddStatusMessage(SharedStrings.IMPORT_DATA_DEVICE_STARTED + " " + txtPhoneDataFile.Text + ". " + importTypeDescription);

                btnCancel.Enabled = false;
                btnOK.Enabled = false;
                rdbUpdateAndAppend.Enabled = false;
                rdbUpdate.Enabled = false;
                rdbAppend.Enabled = false;
                txtPhoneDataFile.Enabled = false;

                if (importWorker.WorkerSupportsCancellation)
                {
                    importWorker.CancelAsync();
                }

                this.Cursor = Cursors.WaitCursor;

                importWorker = new BackgroundWorker();
                importWorker.WorkerSupportsCancellation = true;
                importWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                importWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_WorkerCompleted);
                importWorker.RunWorkerAsync(txtPhoneDataFile.Text);
                //}
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                this.BeginInvoke(new SetStatusDelegate(AddErrorStatusMessage), SharedStrings.IMPORT_ERROR_DEVICE_INTERUPT);

                if (stopwatch != null)
                {
                    stopwatch.Stop();
                }

                btnCancel.Enabled = true;
                btnOK.Enabled = true;
                rdbUpdateAndAppend.Enabled = true;
                rdbUpdate.Enabled = true;
                rdbAppend.Enabled = true;
                txtPhoneDataFile.Enabled = true;
                progressBar.Visible = false;

                importFinished = true;

                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new SetStatusDelegate(AddErrorStatusMessage), SharedStrings.IMPORT_DATA_FAILED);

                if (stopwatch != null)
                {
                    stopwatch.Stop();
                }

                btnCancel.Enabled = true;
                btnOK.Enabled = true;
                rdbUpdateAndAppend.Enabled = true;
                rdbUpdate.Enabled = true;
                rdbAppend.Enabled = true;
                txtPhoneDataFile.Enabled = true;
                progressBar.Visible = false;

                importFinished = true;

                this.Cursor = Cursors.Default;
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
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Increment((int)value);
        }

        /// <summary>
        /// Processes a form's base table
        /// </summary>
        /// <param name="destinationView">The destination form</param>
        /// <param name="destinationGUIDList">The list of GUIDs that exist in the destination</param>
        private void ProcessBaseTable(View destinationView, List<string> destinationGUIDList)
        {
            this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), SharedStrings.IMPORT_DATA_PROCESSING_RECS);

            int recordsInserted = 0;
            int recordsUpdated = 0;

            string destinationTable = destinationView.TableName;

            try
            {
                //IDataReader sourceReader = sourceProjectDataDriver.GetTableDataReader(sourceView.TableName);
                foreach (string surveyGUID in surveyGUIDs.Keys /*Epi.Web.Common.DTO.SurveyAnswerDTO surveyAnswer in result.SurveyResponseList*/)
                {
                    object recordStatus = 1; // no marking for deletion supported at this time.

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

                    string GUID = surveyGUID; // surveyAnswer.ResponseId; // sourceReader["GlobalRecordId"].ToString();
                    string FKEY = surveyGUIDs[surveyGUID]; // sourceReader["FKEY"].ToString(); 

                    QueryParameter paramFkey = new QueryParameter("@FKEY", DbType.String, FKEY);
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
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new SetStatusDelegate(AddErrorStatusMessage), ex.Message);
            }
            finally
            {
            }

            // this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), "On page '" + destinationTable + "', " + recordsInserted.ToString() + " record(s) inserted and " + recordsUpdated.ToString() + " record(s) updated.");
        }

        private struct PhoneFieldData
        {
            public string RecordGUID;
            public string FieldName;
            public object FieldValue;
            public int Page;
        }

        private object FormatWebFieldData(string fieldName, object value)
        {
            if (destinationView.Fields.Contains(fieldName))
            {
                Field field = destinationView.Fields[fieldName];

                if (field is CheckBoxField || field is YesNoField)
                {
                    if (value.ToString().ToLowerInvariant().Equals("yes"))
                    {
                        value = true;
                    }
                    else if (value.ToString().ToLowerInvariant().Equals("no"))
                    {
                        value = false;
                    }
                }

                if (field is NumberField && !string.IsNullOrEmpty(value.ToString()))
                {
                    double result = -1;
                    if (double.TryParse(value.ToString(), out result))
                    {
                        value = result;
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Parses XML from the web survey
        /// </summary>        
        private void ParseXML(string filePath)
        {
            _surveyResponses = new Dictionary<string, List<PhoneFieldData>>();
            surveyGUIDs = new Dictionary<string, string>();

            PhoneFieldData wfData = new PhoneFieldData();
            string xmlText;

            string password = txtPassword.Text;
            string encrypted = System.IO.File.ReadAllText(filePath);

            try
            {
                xmlText = Configuration.DecryptJava(encrypted, password);
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new SetStatusDelegate(AddErrorStatusMessage), SharedStrings.IMPORT_ERROR_DEVICE_INVALID_FILE);
                return;
            }

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(xmlText);

            if (doc.ChildNodes[0].Name.ToLowerInvariant().Equals("surveyresponses"))
            {
                foreach (XmlElement docElement in doc.ChildNodes[0].ChildNodes)
                {
                    if (docElement.Name.ToLowerInvariant().Equals("surveyresponse") && docElement.Attributes.Count > 0 && docElement.Attributes[0].Name.ToLowerInvariant().Equals("surveyresponseid"))
                    {
                        string surveyResponseId = docElement.Attributes[0].Value;

                        if (!surveyGUIDs.ContainsKey(surveyResponseId))
                        {
                            string fkey = string.Empty;
                            if (docElement.HasAttribute("fkey"))
                            {
                                fkey = docElement.Attributes["fkey"].Value;
                            }
                            surveyGUIDs.Add(surveyResponseId, fkey);
                        }

                        foreach (XmlElement surveyElement in docElement.ChildNodes)
                        {
                            if (surveyElement.Name.ToLowerInvariant().Equals("page") && surveyElement.Attributes.Count > 0 && surveyElement.Attributes[0].Name.ToLowerInvariant().Equals("pageid"))
                            {
                                List<PhoneFieldData> fieldValues = new List<PhoneFieldData>();

                                foreach (XmlElement pageElement in surveyElement.ChildNodes)
                                {
                                    if (pageElement.Name.ToLowerInvariant().Equals("responsedetail"))
                                    {
                                        string fieldName = string.Empty;
                                        if (pageElement.Attributes.Count > 0)
                                        {
                                            fieldName = pageElement.Attributes[0].Value;
                                        }
                                        object fieldValue = FormatWebFieldData(fieldName, pageElement.InnerText);

                                        wfData = new PhoneFieldData();
                                        wfData.RecordGUID = surveyResponseId;
                                        wfData.Page = Convert.ToInt32(surveyElement.Attributes[0].Value);
                                        wfData.FieldName = fieldName;
                                        wfData.FieldValue = fieldValue;
                                        fieldValues.Add(wfData);

                                        if (_surveyResponses.Keys.Contains(surveyResponseId))
                                        {
                                            _surveyResponses[surveyResponseId].Add(wfData);
                                        }
                                        else
                                        {
                                            List<PhoneFieldData> list = new List<PhoneFieldData>();
                                            list.Add(wfData);
                                            _surveyResponses.Add(surveyResponseId, list);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private PhoneFieldData FindWebFieldData(List<PhoneFieldData> surveyResponses, string GUID, string fieldName, int pageNumber)
        {
            foreach (PhoneFieldData webFieldData in surveyResponses)
            {
                if (webFieldData.RecordGUID == GUID && webFieldData.FieldName == fieldName && webFieldData.Page == pageNumber)
                {
                    return webFieldData;
                }
            }

            return new PhoneFieldData(); // this is bad, fix
        }

        /// <summary>
        /// Processes all of the fields on a given form, page-by-page, except for the fields on the base table.
        /// </summary>      
        /// <param name="destinationView">The destination form</param>
        /// <param name="destinationGUIDList">The list of GUIDs that exist in the destination</param>
        private void ProcessPages(View destinationView, List<string> destinationGUIDList, string syncFilePath)
        {
            int recordsProcessed = 0;

            foreach (KeyValuePair<string, List<PhoneFieldData>> kvp in _surveyResponses)
            {
                string recordKey = kvp.Key;
                Dictionary<int, List<PhoneFieldData>> pagedFieldDataDictionary = new Dictionary<int, List<PhoneFieldData>>();

                for (int i = 0; i < destinationView.Pages.Count; i++)
                {
                    pagedFieldDataDictionary.Add(destinationView.Pages[i].Id, new List<PhoneFieldData>());
                }

                foreach (PhoneFieldData fieldData in kvp.Value)
                {
                    int pageId = fieldData.Page;
                    if (fieldData.FieldName.ToLowerInvariant().Equals("globalrecordid") || destinationView.GetPageById(pageId).Fields.Contains(fieldData.FieldName))
                    {
                        pagedFieldDataDictionary[pageId].Add(fieldData);
                    }
                }

                for (int i = 0; i < destinationView.Pages.Count; i++)
                {
                    this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), string.Format(SharedStrings.IMPORT_DATA_PROCESSING_RECS_PAGE, (i + 1).ToString(), destinationView.Pages.Count.ToString()));

                    int recordsInserted = 0;
                    int recordsUpdated = 0;

                    Page destinationPage = destinationView.Pages[i];
                    string pageTableName = destinationPage.TableName;
                    List<PhoneFieldData> pagedFieldDataList = pagedFieldDataDictionary[destinationPage.Id];

                    Query selectQuery = destinationProjectDataDriver.CreateQuery("SELECT [GlobalRecordId] FROM [" + pageTableName + "]");
                    IDataReader destReader = destinationProjectDataDriver.ExecuteReader(selectQuery);
                    destinationGUIDList = new List<string>();

                    while (destReader.Read())
                    {
                        destinationGUIDList.Add(destReader[0].ToString());
                    }

                    destReader.Close();

                    try
                    {
                        if (pagedFieldDataList.Count == 0 && append)
                        {
                            InsertEmptyPageTableRecord(recordKey, pageTableName);
                        }

                        string currentGUID = string.Empty;
                        string lastGUID = string.Empty;

                        WordBuilder fieldNames = new WordBuilder(StringLiterals.COMMA);
                        WordBuilder fieldValues = new WordBuilder(StringLiterals.COMMA);
                        List<QueryParameter> fieldValueParams = new List<QueryParameter>();
                        int fieldsInQuery = 0;

                        PhoneFieldData wfData = new PhoneFieldData();
                        wfData.RecordGUID = "__LAST__"; // last field, acts as a flag
                        pagedFieldDataList.Add(wfData);

                        List<string> GUIDList = new List<string>();

                        foreach (PhoneFieldData fieldData in pagedFieldDataList)
                        {
                            currentGUID = fieldData.RecordGUID;

                            if (importWorker.CancellationPending)
                            {
                                this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), SharedStrings.IMPORT_DATA_CANCELLED);
                                return;
                            }

                            string GUID = fieldData.RecordGUID; // sourceReader["GlobalRecordId"].ToString();
                            List<string> updatedGUIDs = new List<string>();

                            if (destinationGUIDList.Contains(GUID) && update)
                            {
                                #region UPDATE

                                currentGUID = fieldData.RecordGUID;

                                if (importWorker.CancellationPending)
                                {
                                    this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), "Import cancelled.");
                                    return;
                                }

                                string updateHeader = string.Empty;
                                string whereClause = string.Empty;
                                fieldValueParams = new List<QueryParameter>();
                                StringBuilder sb = new StringBuilder();

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

                                sb.Append(StringLiterals.LEFT_SQUARE_BRACKET);
                                sb.Append(fieldData.FieldName);
                                sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET);
                                sb.Append(StringLiterals.EQUAL);

                                sb.Append(StringLiterals.COMMERCIAL_AT);
                                sb.Append(fieldData.FieldName);

                                QueryParameter param = GetQueryParameterForField(fieldData, destinationPage, syncFilePath);
                                if (param != null)
                                {
                                    Query updateQuery = destinationProjectDataDriver.CreateQuery(updateHeader + StringLiterals.SPACE + sb.ToString() + StringLiterals.SPACE + whereClause);
                                    updateQuery.Parameters.Add(param);
                                    destinationProjectDataDriver.ExecuteNonQuery(updateQuery);

                                    if (!GUIDList.Contains(GUID))
                                    {
                                        GUIDList.Add(GUID);
                                        recordsUpdated++;
                                        this.BeginInvoke(new SetProgressBarDelegate(IncrementProgressBarValue), 1);
                                    }
                                }

                                #endregion // UPDATE
                            }
                            else if (!destinationGUIDList.Contains(GUID) && append)
                            {
                                #region APPEND

                                if (!string.IsNullOrEmpty(lastGUID) && lastGUID != currentGUID)
                                {
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

                                    fieldNames = new WordBuilder(StringLiterals.COMMA);
                                    fieldValues = new WordBuilder(StringLiterals.COMMA);
                                    fieldValueParams = new List<QueryParameter>();
                                    fieldsInQuery = 0;
                                }

                                lastGUID = fieldData.RecordGUID;

                                if (lastGUID == "__LAST__")
                                {
                                    break;
                                }

                                if (!fieldNames.Contains("GlobalRecordId"))
                                {
                                    fieldNames.Append("GlobalRecordId");
                                    fieldValues.Append("@GlobalRecordId");
                                    fieldValueParams.Add(new QueryParameter("@GlobalRecordId", DbType.String, GUID));
                                    fieldsInQuery++;
                                }

                                Field dataField = destinationView.Fields[fieldData.FieldName]; // already checked for this above so should never fail...

                                if (!(
                                    dataField is GroupField ||
                                    dataField is RelatedViewField ||
                                    dataField is UniqueKeyField ||
                                    dataField is RecStatusField ||
                                    dataField is GlobalRecordIdField ||
                                    fieldData.FieldValue == null ||
                                    string.IsNullOrEmpty(fieldData.FieldValue.ToString())
                                    ))
                                {
                                    String fieldName = ((Epi.INamedObject)dataField).Name;

                                    switch (dataField.FieldType)
                                    {
                                        case MetaFieldType.Date:
                                        case MetaFieldType.DateTime:
                                        case MetaFieldType.Time:
                                            fieldValueParams.Add(new QueryParameter("@" + fieldName, DbType.DateTime, Convert.ToDateTime(fieldData.FieldValue)));
                                            break;
                                        case MetaFieldType.Checkbox:
                                            fieldValueParams.Add(new QueryParameter("@" + fieldName, DbType.Boolean, Convert.ToBoolean(fieldData.FieldValue)));
                                            break;
                                        case MetaFieldType.CommentLegal:
                                            fieldValueParams.Add(new QueryParameter("@" + fieldName, DbType.String, fieldData.FieldValue.ToString().Split('-')[0]));
                                            break;
                                        case MetaFieldType.LegalValues:
                                        case MetaFieldType.Codes:
                                        case MetaFieldType.Text:
                                        case MetaFieldType.TextUppercase:
                                        case MetaFieldType.PhoneNumber:
                                        case MetaFieldType.UniqueRowId:
                                        case MetaFieldType.ForeignKey:
                                        case MetaFieldType.GlobalRecordId:
                                        case MetaFieldType.Multiline:
                                            fieldValueParams.Add(new QueryParameter("@" + fieldName, DbType.String, fieldData.FieldValue));
                                            break;
                                        case MetaFieldType.Number:
                                            fieldValueParams.Add(new QueryParameter("@" + fieldName, DbType.Double, fieldData.FieldValue));
                                            break;
                                        case MetaFieldType.YesNo:
                                        case MetaFieldType.Option:
                                        case MetaFieldType.RecStatus:
                                            fieldValueParams.Add(new QueryParameter("@" + fieldName, DbType.Single, fieldData.FieldValue));
                                            break;
                                        case MetaFieldType.Image:
                                            byte[] imageBytes = null;
                                            if (syncFilePath.Contains("EpiInfo\\"))
                                            {
                                                string part1 = syncFilePath.Substring(0, syncFilePath.IndexOf("EpiInfo"));
                                                string part2 = fieldData.FieldValue.ToString().Substring(fieldData.FieldValue.ToString().IndexOf("EpiInfo"), fieldData.FieldValue.ToString().Length - fieldData.FieldValue.ToString().IndexOf("EpiInfo"));
                                                string fileName = part1 + part2;

                                                try
                                                {
                                                    if (File.Exists(fileName))
                                                    {
                                                        imageBytes = Util.GetByteArrayFromImagePath(fileName);
                                                    }
                                                }
                                                catch
                                                {
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                var devices = new PortableDevices.PortableDeviceCollection();
                                                try
                                                {
                                                    devices.Refresh();
                                                }
                                                catch (System.IndexOutOfRangeException)
                                                {
                                                    break;
                                                }

                                                if (devices.Count == 0)
                                                {
                                                    break;
                                                }

                                                var pd = devices.First();
                                                pd.Connect();
                                                PortableDeviceFolder root = pd.GetContents();

                                                PortableDeviceFolder download =
                                                    (from r in ((PortableDeviceFolder)root.Files[0]).Files
                                                     where r.Name.Equals("Download")
                                                     select r).First() as PortableDeviceFolder;

                                                PortableDeviceFolder epiinfo =
                                                    (from r in download.Files
                                                     where r.Name.Equals("EpiInfo")
                                                     select r).First() as PortableDeviceFolder;

                                                PortableDeviceFolder images =
                                                    (from r in epiinfo.Files
                                                     where r.Name.Equals("Images")
                                                     select r).First() as PortableDeviceFolder;

                                                if (images == null)
                                                {
                                                    pd.Disconnect();
                                                    break;
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        string imageFileName = fieldData.FieldValue.ToString().Substring(fieldData.FieldValue.ToString().IndexOf("Images/")).Split('/')[1].Split('.')[0];
                                                        string tempPath = Path.GetTempPath();
                                                        PortableDeviceFile existingFile =
                                                        (from r in images.Files
                                                         where r.Name.Equals(imageFileName)
                                                         select r).First() as PortableDeviceFile;
                                                        new FileInfo(tempPath + existingFile.Id).Create().Close();
                                                        pd.DownloadFile(existingFile, tempPath);
                                                        imageBytes = Util.GetByteArrayFromImagePath(tempPath + existingFile.Id);
                                                    }
                                                    catch { }
                                                }
                                            }
                                            if (imageBytes != null)
                                            {
                                                fieldValueParams.Add(new QueryParameter("@" + fieldName, DbType.Binary, imageBytes));
                                            }
                                            break;
                                        default:
                                            throw new ApplicationException("Not a supported field type");
                                    }
                                    fieldNames.Append(destinationProjectDataDriver.InsertInEscape(((Epi.INamedObject)dataField).Name));
                                    fieldValues.Append("@" + fieldName);
                                    fieldsInQuery++;
                                }

                                #endregion // APPEND
                            }
                            this.BeginInvoke(new SetProgressBarDelegate(IncrementProgressBarValue), 1);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(new SetStatusDelegate(AddErrorStatusMessage), ex.Message);
                    }
                    finally
                    {
                    }

                    // this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), "On page '" + destinationPage.Name + "', " + recordsInserted.ToString() + " record(s) inserted and " + recordsUpdated.ToString() + " record(s) updated.");
                }

                recordsProcessed++;
            }

            this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), recordsProcessed.ToString() + " record(s) processed.");
        }

        private void InsertEmptyPageTableRecord(string recordKey, string pageTableName)
        {
            Query selectQuery = destinationProjectDataDriver.CreateQuery("SELECT [GlobalRecordId] FROM [" + pageTableName + "] WHERE [GlobalRecordId] = '" + recordKey + "'");
            IDataReader destReader = destinationProjectDataDriver.ExecuteReader(selectQuery);

            if (destReader.Read())
            {
                return;
            }

            destReader.Close();

            StringBuilder sb = new StringBuilder();
            sb.Append(" insert into ");
            sb.Append(destinationProjectDataDriver.InsertInEscape(pageTableName));
            sb.Append(StringLiterals.SPACE);
            sb.Append(Util.InsertInParantheses("GlobalRecordId"));
            sb.Append(" values ('");
            sb.Append(recordKey);
            sb.Append("') ");
            Query insertQuery = destinationProjectDataDriver.CreateQuery(sb.ToString());

            destinationProjectDataDriver.ExecuteNonQuery(insertQuery);
        }

        private QueryParameter GetQueryParameterForField(PhoneFieldData fieldData, Page sourcePage, string syncFilePath)
        {
            Field dataField = destinationView.Fields[fieldData.FieldName];
            if (!(
                dataField is GroupField ||
                dataField is RelatedViewField ||
                dataField is UniqueKeyField ||
                dataField is RecStatusField ||
                dataField is GlobalRecordIdField
                ))
            {
                String fieldName = ((Epi.INamedObject)dataField).Name;
                if (fieldData.FieldValue == null || string.IsNullOrEmpty(fieldData.FieldValue.ToString()))
                {
                    switch (dataField.FieldType)
                    {
                        case MetaFieldType.Date:
                        case MetaFieldType.DateTime:
                        case MetaFieldType.Time:
                            return new QueryParameter("@" + fieldName, DbType.DateTime, DBNull.Value);
                        case MetaFieldType.Checkbox:
                            return new QueryParameter("@" + fieldName, DbType.Boolean, DBNull.Value);
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
                            return new QueryParameter("@" + fieldName, DbType.String, DBNull.Value);
                        case MetaFieldType.Number:
                            return new QueryParameter("@" + fieldName, DbType.Double, DBNull.Value);
                        case MetaFieldType.YesNo:
                        case MetaFieldType.Option:
                        case MetaFieldType.RecStatus:
                            return new QueryParameter("@" + fieldName, DbType.Single, DBNull.Value);
                        case MetaFieldType.Image:
                            return new QueryParameter("@" + fieldName, DbType.Binary, DBNull.Value);
                        default:
                            throw new ApplicationException("Not a supported field type");
                    }
                }
                else
                {
                    switch (dataField.FieldType)
                    {
                        case MetaFieldType.Date:
                        case MetaFieldType.DateTime:
                        case MetaFieldType.Time:
                            return new QueryParameter("@" + fieldName, DbType.DateTime, Convert.ToDateTime(fieldData.FieldValue));
                        case MetaFieldType.Checkbox:
                            return new QueryParameter("@" + fieldName, DbType.Boolean, Convert.ToBoolean(fieldData.FieldValue));
                        case MetaFieldType.CommentLegal:
                            return new QueryParameter("@" + fieldName, DbType.String, fieldData.FieldValue.ToString().Split('-')[0]);
                        case MetaFieldType.LegalValues:
                        case MetaFieldType.Codes:
                        case MetaFieldType.Text:
                        case MetaFieldType.TextUppercase:
                        case MetaFieldType.PhoneNumber:
                        case MetaFieldType.UniqueRowId:
                        case MetaFieldType.ForeignKey:
                        case MetaFieldType.GlobalRecordId:
                        case MetaFieldType.Multiline:
                            return new QueryParameter("@" + fieldName, DbType.String, fieldData.FieldValue);
                        case MetaFieldType.Number:
                            return new QueryParameter("@" + fieldName, DbType.Double, fieldData.FieldValue);
                        case MetaFieldType.YesNo:
                        case MetaFieldType.Option:
                        case MetaFieldType.RecStatus:
                            return new QueryParameter("@" + fieldName, DbType.Single, fieldData.FieldValue);
                        case MetaFieldType.Image:
                            byte[] imageBytes = null;
                            if (syncFilePath.Contains("EpiInfo\\"))
                            {
                                string part1 = syncFilePath.Substring(0, syncFilePath.IndexOf("EpiInfo"));
                                string part2 = fieldData.FieldValue.ToString().Substring(fieldData.FieldValue.ToString().IndexOf("EpiInfo"), fieldData.FieldValue.ToString().Length - fieldData.FieldValue.ToString().IndexOf("EpiInfo"));
                                string fileName = part1 + part2;

                                if (File.Exists(fileName))
                                {
                                    imageBytes = Util.GetByteArrayFromImagePath(fileName);
                                }
                            }
                            else
                            {
                                imageBytes = null;
                                var devices = new PortableDevices.PortableDeviceCollection();
                                try
                                {
                                    devices.Refresh();
                                }
                                catch (System.IndexOutOfRangeException)
                                {
                                    break;
                                }

                                if (devices.Count == 0)
                                {
                                    break;
                                }

                                var pd = devices.First();
                                pd.Connect();
                                PortableDeviceFolder root = pd.GetContents();

                                PortableDeviceFolder download =
                                    (from r in ((PortableDeviceFolder)root.Files[0]).Files
                                     where r.Name.Equals("Download")
                                     select r).First() as PortableDeviceFolder;

                                PortableDeviceFolder epiinfo =
                                    (from r in download.Files
                                     where r.Name.Equals("EpiInfo")
                                     select r).First() as PortableDeviceFolder;

                                PortableDeviceFolder images =
                                    (from r in epiinfo.Files
                                     where r.Name.Equals("Images")
                                     select r).First() as PortableDeviceFolder;

                                if (images == null)
                                {
                                    pd.Disconnect();
                                    break;
                                }
                                else
                                {
                                    try
                                    {
                                        string imageFileName = fieldData.FieldValue.ToString().Substring(fieldData.FieldValue.ToString().IndexOf("Images/")).Split('/')[1].Split('.')[0];
                                        string tempPath = Path.GetTempPath();
                                        PortableDeviceFile existingFile =
                                        (from r in images.Files
                                         where r.Name.Equals(imageFileName)
                                         select r).First() as PortableDeviceFile;
                                        new FileInfo(tempPath + existingFile.Id).Create().Close();
                                        pd.DownloadFile(existingFile, tempPath);
                                        imageBytes = Util.GetByteArrayFromImagePath(tempPath + existingFile.Id);
                                    }
                                    catch (Exception ex)
                                    {
                                        //
                                    }
                                }
                            }
                            if (imageBytes != null)
                            {
                                return new QueryParameter("@" + fieldName, DbType.Binary, imageBytes);
                            }
                            else
                            {
                                return new QueryParameter("@" + fieldName, DbType.Binary, null);
                            }
                        default:
                            throw new ApplicationException(SharedStrings.IMPORT_ERROR_INVALID_TYPE);
                    }
                }
            }

            return null;
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
            //int errorCount = 0;
            //int warningCount = 0;

            //if (sourceView.Pages.Count != destinationView.Pages.Count)
            //{
            //    AddErrorStatusMessage("The page count for each form is different. Process halted.");
            //    errorCount++;
            //    return false;
            //}

            // TODO: This works okay for the main form to be imported, but it should incorporate checks
            //   on related forms and grid fields too.

            //for (int i = 0; i < sourceView.Pages.Count; i++)
            //{
            //    if (sourceView.Pages[i].Fields.Count != destinationView.Pages[i].Fields.Count)
            //    {
            //        return false;
            //    }
            //}

            //foreach (Field sourceField in sourceView.Fields)
            //{
            //    if (destinationView.Fields.Contains(sourceField.Name))
            //    {
            //        Field destinationField = destinationView.Fields[sourceField.Name];

            //        if (destinationField.FieldType != sourceField.FieldType)
            //        {
            //            AddErrorStatusMessage("The field type for " + destinationField.Name + " in the destination form doesn't match the field type in the source form.");
            //            errorCount++;
            //        }
            //    }
            //    else
            //    {
            //        AddWarningMessage("The field " + sourceField.Name + " was not found in the destination form. It will be skipped during the import.");
            //        warningCount++;
            //    }

            //    if (sourceField is ImageField)
            //    {
            //        AddWarningMessage("The field " + sourceField.Name + " is an image field. Image fields are not supported for importing and will be skipped.");                    
            //    }
            //}

            //foreach (Field destinationField in destinationView.Fields)
            //{
            //    if (!sourceView.Fields.Contains(destinationField.Name))
            //    {
            //        AddWarningMessage("The field " + destinationField.Name + " was not found in the source form. It will be skipped during the import.");
            //        warningCount++;
            //    }
            //}

            //// sanity check, especially for projects imported from Epi Info 3, where the forms may have untold amounts of corruption and errors
            //foreach (Field sourceField in sourceView.Fields)
            //{
            //    if (!Util.IsFirstCharacterALetter(sourceField.Name))
            //    {
            //        AddErrorStatusMessage("The field name for " + sourceField.Name + " in the source form is invalid.");
            //        errorCount++;
            //    }
            //    if (Epi.Data.Services.AppData.Instance.IsReservedWord(sourceField.Name) && (sourceField.Name.ToLowerInvariant() != "uniquekey" && sourceField.Name.ToLowerInvariant() != "recstatus" && sourceField.Name.ToLowerInvariant() != "fkey"))
            //    {
            //        AddWarningMessage("The field name for " + sourceField.Name + " in the source form is a reserved word. Problems may be encountered during the import.");
            //    }
            //}

            //if (!sourceProjectDataDriver.TableExists(sourceView.TableName))
            //{
            //    AddErrorStatusMessage(string.Format(SharedStrings.DATA_TABLE_NOT_FOUND, sourceView.Name));
            //    errorCount++;
            //}

            //if (warningCount > ((double)destinationView.Fields.Count / 1.7)) // User may have selected to import the wrong form with this many differences?
            //{
            //    AddErrorStatusMessage("Too many differences were detected. The import process has been halted.");
            //    return false;
            //}

            //if (errorCount > 1)
            //{
            //    AddErrorStatusMessage(errorCount.ToString() + " errors were encountered while checking the forms for similarity. These issues must be fixed before the import can proceed.");
            //    return false;
            //}
            //else if (errorCount == 1)
            //{
            //    AddErrorStatusMessage(errorCount.ToString() + " error was encountered while checking the forms for similarity. This issues must be fixed before the import can proceed.");
            //    return false;
            //}

            return true;
        }
        #endregion // Private Methods

        #region Event Handlers
        private void txtPhoneDataFile_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPhoneDataFile.Text))
            {
                btnOK.Enabled = false;
            }
            else
            {
                btnOK.Enabled = true;
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (checkboxBatchImport.Checked)
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string directoryPath = folderBrowserDialog.SelectedPath;
                    if (System.IO.Directory.Exists(directoryPath))
                    {
                        txtPhoneDataFile.Text = directoryPath;
                    }
                }
            }
            else
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = SharedStrings.SELECT_DATA_SOURCE;
                openFileDialog.Filter = "\"Epi Info for Android\" sync files|*.epi7|Phone data sync files|*.xml";

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
                            txtPhoneDataFile.Text = filePath;
                        }
                        catch (Exception ex)
                        {
                            Epi.Windows.MsgBox.ShowError(SharedStrings.IMPORT_DATA_DEVICE_FILE_ERROR, ex);
                            txtPhoneDataFile.Text = string.Empty;
                            return;
                        }
                    }
                }
            }
        }

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

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPhoneDataFile.Text))
            {
                progressBar.Style = ProgressBarStyle.Marquee;
                progressBar.Visible = true;
                progressBar.Value = 0;
                progressBar.Minimum = 0;

                lbxStatus.Items.Clear();

                textProgress.Text = string.Empty;
                AddStatusMessage(SharedStrings.IMPORT_DATA_DEVICE_REQUESTED + " " + System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString());

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

                AddStatusMessage(SharedStrings.IMPORT_DATA_DEVICE_STARTED + " " + txtPhoneDataFile.Text + ". " + importTypeDescription);
                if (importWorker.WorkerSupportsCancellation)
                {
                    importWorker.CancelAsync();
                }

                packagePaths = new List<string>();

                if (checkboxBatchImport.Checked)
                {
                    System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(txtPhoneDataFile.Text);
                    foreach (System.IO.FileInfo f in dir.GetFiles("*.epi7"))
                    {
                        packagePaths.Add(f.FullName);
                    }
                    AddStatusMessage(string.Format(SharedStrings.START_BATCH_IMPORT, packagePaths.Count.ToString(), txtPhoneDataFile.Text));
                }
                else
                {
                    packagePaths.Add(txtPhoneDataFile.Text);
                    AddStatusMessage(string.Format(SharedStrings.START_SINGLE_IMPORT, txtPhoneDataFile.Text));
                }

                this.Cursor = Cursors.WaitCursor;

                importWorker = new BackgroundWorker();
                importWorker.WorkerSupportsCancellation = true;
                importWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                importWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_WorkerCompleted);
                importWorker.RunWorkerAsync(txtPhoneDataFile.Text);
            }
        }

        /// <summary>
        /// Handles the WorkerCompleted event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            stopwatch.Stop();

            this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), SharedStrings.IMPORT_DATA_COMPLETE + " " + SharedStrings.IMPORT_DATA_TIME_ELAPSED + " " + stopwatch.Elapsed.ToString());
            this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), SharedStrings.IMPORT_DATA_COMPLETE + " " + SharedStrings.IMPORT_DATA_TIME_ELAPSED + " " + stopwatch.Elapsed.ToString());

            StopImport();
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
                foreach (string packagePath in packagePaths)
                {
                    //foreach (var ViewName in this.RelatedViews)
                    //{
                    //  View View = destinationProject.GetViewByName(ViewName);
                    string syncFilePath = (string)e.Argument;
                    stopwatch = new Stopwatch();
                    stopwatch.Start();
                    Query selectQuery = destinationProjectDataDriver.CreateQuery("SELECT [GlobalRecordId] FROM [" + destinationView.TableName + "]");
                    //Query selectQuery = destinationProjectDataDriver.CreateQuery("SELECT [GlobalRecordId] FROM [" + ViewName + "]");
                    IDataReader destReader = destinationProjectDataDriver.ExecuteReader(selectQuery);
                    List<string> destinationGUIDList = new List<string>();
                    while (destReader.Read())
                    {
                        destinationGUIDList.Add(destReader[0].ToString());
                    }
                    destReader.Close();
                    ParseXML(packagePath);
                    ProcessBaseTable(destinationView, destinationGUIDList);
                    ProcessPages(destinationView, destinationGUIDList, syncFilePath);
                    //ProcessBaseTable(View, destinationGUIDList);
                    // ProcessPages(View, destinationGUIDList, syncFilePath);
                }
                //ProcessGridFields(sourceView, destinationView);
                //ProcessRelatedForms(sourceView, destinationView, viewsToProcess);
                //}
            }
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

        private void ImportDataForm_Load(object sender, EventArgs e)
        {
            AddStatusMessage(SharedStrings.IMPORT_DATA_READY);
        }
        #endregion // Event Handlers

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

        private void rdbUpdateAndAppend_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void viewsList_SelectedIndexChanged(object sender, EventArgs e)
        {


            var   c = sender as ComboBox;

            View thisView = (View) c.SelectedItem;


            destinationView = thisView;
            destinationProject = thisView.Project;



        }
    }
}
