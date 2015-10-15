using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Epi;
using Epi.Data;
using Epi.Fields;
using Epi.Web.Common;
using Epi.Web.Common.Message;
using Epi.ImportExport;

namespace Epi.Enter.Forms
{
    /// <summary>
    /// A user interface element to update or append records to the currently-open form from another similar, or identical, Epi Info 7 form.
    /// </summary>
    public partial class ImportWebDataForm : Form
    {
        #region Private Members
        private bool isBatchImport = false; // currently unused
        private Project sourceProject;
        //private View sourceView;
        private Project destinationProject;
        private View destinationView;
        private IDbDriver sourceProjectDataDriver;
        private IDbDriver destinationProjectDataDriver;
        private Configuration config;
        private int lastRecordId;
        private BackgroundWorker importWorker;
        private BackgroundWorker requestWorker;
        private static object syncLock = new object();
        private Stopwatch stopwatch;
        private bool update = true;
        private bool append = true;
        private bool importFinished = false;
        private int Pages = 0;
        private int PageSize = 0;
        private string SurveyId = string.Empty;
        private string OrganizationKey = string.Empty;
        private string PublishKey = string.Empty;
        private SurveyManagerServiceV2.ManagerServiceV2Client client;
        private Dictionary<string, Dictionary<string, WebFieldData>> wfList;
        private bool IsDraftMode;
        private int SurveyStatus;
        #endregion // Private Members

        #region Delegates
        private delegate void SetStatusDelegate(string statusMessage);
        #endregion // Delegates

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public ImportWebDataForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor
        /// </summary>        
        /// <param name="destinationView">The destination form; should be the currently-open view</param>
        public ImportWebDataForm(View destinationView)
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
            //try
            //{
            this.destinationProjectDataDriver = destinationProject.CollectedData.GetDbDriver();
            this.config = Configuration.GetNewInstance();

            this.importWorker = new BackgroundWorker();
            this.importWorker.WorkerSupportsCancellation = true;

            this.requestWorker = new BackgroundWorker();
            this.requestWorker.WorkerSupportsCancellation = true;

            this.IsBatchImport = false;

            rdbDraftMode.Checked = true;
            rdbSubmittedIncremental.Checked = true;

            if (!string.IsNullOrEmpty(config.Settings.WebServiceEndpointAddress.ToLower()) && (
                config.Settings.WebServiceEndpointAddress.ToLower().Contains(Epi.Constants.surveyManagerservice) ||
                config.Settings.WebServiceEndpointAddress.ToLower().Contains(Epi.Constants.surveyManagerservicev2)))
            {
                lblIncrementalImport.Visible = false;
                rdbSubmittedIncremental.Visible = false;
                lblFullImport.Location = new Point(6,19);
                rdbSubmittedFull.Checked = true;
                rdbSubmittedFull.Location = new Point(26, 34);
                rdbAllRecords.Location = new Point(26, 78);              
            }
            else
            {
                rdbSubmittedIncremental.Checked = true;
            }
                
           
        }

        /// <summary>
        /// Gets a request object
        /// </summary>
        private void GetRequest()
        {
            if (requestWorker.WorkerSupportsCancellation)
            {
                requestWorker.CancelAsync();
            }

            this.Cursor = Cursors.WaitCursor;

            requestWorker = new BackgroundWorker();
            requestWorker.WorkerSupportsCancellation = true;
            requestWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(requestWorker_DoWork);
            requestWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(requestWorker_WorkerCompleted);
            requestWorker.RunWorkerAsync();
        }

        private QueryParameter GetQueryParameterForField(WebFieldData fieldData, Page sourcePage)
        {
            Field dataField = destinationView.Fields[fieldData.FieldName];
            if (!(
                dataField is GroupField ||
                dataField is RelatedViewField ||
                dataField is UniqueKeyField ||
                dataField is RecStatusField ||
                dataField is GlobalRecordIdField ||
                dataField is ImageField ||                
                fieldData.FieldValue == null ||
                string.IsNullOrEmpty(fieldData.FieldValue.ToString())
                ))
            {
                String fieldName = ((Epi.INamedObject)dataField).Name;
                 try
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
                    case MetaFieldType.LegalValues:
                    case MetaFieldType.Codes:
                    case MetaFieldType.Text:
                    case MetaFieldType.TextUppercase:
                    case MetaFieldType.PhoneNumber:
                    case MetaFieldType.UniqueRowId:
                    case MetaFieldType.ForeignKey:
                    case MetaFieldType.GlobalRecordId:
                        if (fieldData.FieldValue.ToString().Length > 255)
                        {
                            fieldData.FieldValue = fieldData.FieldValue.ToString().Substring(0, 255);
                            AddStatusMessage("The field data for " + fieldData.FieldName + " in record " + fieldData.RecordGUID + " has been truncated because it exceeds 255 characters.");
                        }
                        return new QueryParameter("@" + fieldName, DbType.String, fieldData.FieldValue);
                    case MetaFieldType.Multiline:
                        return new QueryParameter("@" + fieldName, DbType.String, fieldData.FieldValue);
                    case MetaFieldType.Number:
                    case MetaFieldType.YesNo:
                    case MetaFieldType.RecStatus:
                        return new QueryParameter("@" + fieldName, DbType.Single, fieldData.FieldValue);
                    case MetaFieldType.Option:
                        return new QueryParameter("@" + fieldName, DbType.Int16, fieldData.FieldValue);
                    case MetaFieldType.Image:
                        this.BeginInvoke(new SetStatusDelegate(AddWarningMessage), "The data for " + fieldName + " was not imported. This field type is not supported.");
                        break;
                    default:
                        throw new ApplicationException("Not a supported field type");
                }
                    }
                 catch (Exception ex)
                     {

                     this.BeginInvoke(new SetStatusDelegate(AddWarningMessage), "Record GUID:" + fieldData.RecordGUID + "  Field Name:" + fieldName + "  Field Type:" + dataField.FieldType + "  Field Value:" + fieldData.FieldValue + "  Error Message :" + ex.Message);
                     // Logger.Log("Record GUID:" + fieldData.RecordGUID + "  Field Name:" + fieldName + "  Field Type:" + dataField.FieldType + "  Field Value:" + fieldData.FieldValue + "  Error Message :" + ex.Message);
                     return null;
                     }
            }

            return null;
        }

        /// <summary>
        /// Stops an import
        /// </summary>
        private void StopImport()
        {
            btnCancel.Enabled = true;
            btnOK.Enabled = true;
            //cmbImportType.Enabled = true;
            textProject.Enabled = true;
            textData.Enabled = true;
            textOrganization.Enabled = true;
            progressBar.Visible = false;
            progressBar.Style = ProgressBarStyle.Continuous;

            importFinished = true;

            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Initiates a full import on a single form
        /// </summary>
        private void DoImport(SurveyManagerService.SurveyAnswerRequest Request)
        {            
            {
                try
                {
                    stopwatch = new Stopwatch();
                    stopwatch.Start();

                        int recordCount = 0; // Result.SurveyResponseList.Count; // sourceView.GetRecordCount();
                        int gridRowCount = 0;

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
                        progressBar.Maximum = progressBar.Maximum + gridRowCount;

                        string importTypeDescription = "Records with matching ID fields will be updated and unmatched records will be appended.";
                        //if (cmbImportType.SelectedIndex == 0)
                        //{
                        //    update = true;
                        //    append = true;
                        //}
                        //else if (cmbImportType.SelectedIndex == 1)
                        //{
                        //    update = true;
                        //    append = false;
                        //    importTypeDescription = "Records with matching ID fields will be updated. Unmatched records will be ignored.";
                        //}
                        //else
                        //{
                        //    update = false;
                        //    append = true;
                        //    importTypeDescription = "Records with no matching ID fields will be appended. Records with matching ID fields will be ignored.";
                        //}

                        update = false;
                        append = true;
                        importTypeDescription = "Records with no matching ID fields will be appended. Records with matching ID fields will be ignored.";
                       // DownLoadType = cmbImportType.SelectedIndex;
                        AddStatusMessage("Import initiated for form " + textProject.Text + ". " + importTypeDescription);

                        btnCancel.Enabled = false;
                        btnOK.Enabled = false;
                        //cmbImportType.Enabled = false;
                        textProject.Enabled = false;
                        textData.Enabled = false;
                        textOrganization.Enabled = false;

                        if (importWorker.WorkerSupportsCancellation)
                        {
                            importWorker.CancelAsync();
                        }

                        this.Cursor = Cursors.WaitCursor;

                        importWorker = new BackgroundWorker();
                        importWorker.WorkerSupportsCancellation = true;
                        importWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                        importWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_WorkerCompleted);
                        importWorker.RunWorkerAsync(Request);
                        //}
                }
                catch (System.ServiceModel.CommunicationException ex)
                {
                    this.BeginInvoke(new SetStatusDelegate(AddErrorStatusMessage), "Couldn't properly communicate with web service. Import halted.");

                    if (stopwatch != null)
                    {
                        stopwatch.Stop();
                    }

                    btnCancel.Enabled = true;
                    btnOK.Enabled = true;
                    //cmbImportType.Enabled = true;
                    textProject.Enabled = true;
                    textData.Enabled = true;
                    textOrganization.Enabled = true;
                    progressBar.Visible = false;

                    importFinished = true;

                    this.Cursor = Cursors.Default;
                }
                catch (Exception ex)
                {
                    this.BeginInvoke(new SetStatusDelegate(AddErrorStatusMessage), "Import from web failed.");

                    if (stopwatch != null)
                    {
                        stopwatch.Stop();
                    }

                    btnCancel.Enabled = true;
                    btnOK.Enabled = true;
                    //cmbImportType.Enabled = true;
                    textProject.Enabled = true;
                    progressBar.Visible = false;

                    importFinished = true;

                    this.Cursor = Cursors.Default;
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
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Increment((int)value);            
        }

        /// <summary>
        /// Sets the progress bar's max value
        /// </summary>
        /// <param name="value">The max value</param>
        private void SetProgressBarMaximum(double value)
        {
            progressBar.Maximum = ((int)value);
        }

        /// <summary>
        /// Processes a form's base table
        /// </summary>
        /// <param name="result">The web survey result</param>
        /// <param name="destinationView">The destination form</param>
        /// <param name="destinationGUIDList">The list of GUIDs that exist in the destination</param>
        private void ProcessBaseTable(Dictionary<string, Dictionary<string, WebFieldData>> result, View destinationView, List<string> destinationGUIDList)
        {
            //sourceView.LoadFirstRecord();
            this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), "Processing records on base table...");

            int recordsInserted = 0;
            int recordsUpdated = 0;

            //string sourceTable = sourceView.TableName;
            string destinationTable = destinationView.TableName;

            foreach (KeyValuePair<string,Dictionary<string, WebFieldData>> surveyAnswer in result)
            {                    
                    QueryParameter paramRecordStatus = new QueryParameter("@RECSTATUS", DbType.Int32, 1);

                    if (importWorker.CancellationPending)
                    {
                        this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), "Import cancelled.");
                        return;
                    }

                    WordBuilder fieldNames = new WordBuilder(StringLiterals.COMMA);
                    WordBuilder fieldValues = new WordBuilder(StringLiterals.COMMA);
                    List<QueryParameter> fieldValueParams = new List<QueryParameter>();

                    fieldNames.Append("GlobalRecordId");
                    fieldValues.Append("@GlobalRecordId");

                    string GUID = surveyAnswer.Key; // sourceReader["GlobalRecordId"].ToString();
                    string FKEY = string.Empty; // sourceReader["FKEY"].ToString(); // FKEY not needed, no related forms to process

                    QueryParameter paramFkey = new QueryParameter("@FKEY", DbType.String, FKEY); // don't add this yet
                    QueryParameter paramGUID = new QueryParameter("@GlobalRecordId", DbType.String, GUID);
                    fieldValueParams.Add(paramGUID);

                    if (destinationGUIDList.Contains(GUID.ToUpper()))
                    {
                    update = true;
                    append = false;
                    }
                    else
                    {
                        append = true;
                        update = false;
                        
                     }
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
                    //}
                    //else
                    //{
                        if (append)
                        {
                        try
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

                            System.Diagnostics.Debug.Print(insertQuery.SqlStatement);
                            destinationProjectDataDriver.ExecuteNonQuery(insertQuery);

                            foreach (Page page in destinationView.Pages)
                            {
                                sb = new StringBuilder();
                                sb.Append(" insert into ");
                                sb.Append(destinationProjectDataDriver.InsertInEscape(page.TableName));
                                sb.Append(StringLiterals.SPACE);
                                sb.Append("([GlobalRecordId])");
                                sb.Append(" values (");
                                sb.Append("'" + GUID + "'");
                                sb.Append(") ");
                                insertQuery = destinationProjectDataDriver.CreateQuery(sb.ToString());
                                destinationProjectDataDriver.ExecuteNonQuery(insertQuery);
                            }
                            
                            recordsInserted++;
                            }
                            catch(Exception ex)
                            {
                             throw ex;
                                
                            }
                       // }
                    }
                    this.BeginInvoke(new SetProgressBarDelegate(IncrementProgressBarValue), 1);
                }
                //sourceReader.Close();
                //sourceReader.Dispose();
            //}
            //catch (Exception ex)
            //{
            //    this.BeginInvoke(new SetStatusDelegate(AddErrorStatusMessage), ex.Message);
            //    return false;
            //}
            //finally
            //{
            //}
            this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), "On base table '" + destinationTable + "', " + recordsInserted.ToString() + " record(s) inserted and " + recordsUpdated.ToString() + " record(s) updated.");
        }

        private object FormatWebFieldData(string fieldName, object value)
        {
            if (destinationView.Fields.Contains(fieldName))
            {
                Field field = destinationView.Fields[fieldName];

                if (field is CheckBoxField || field is YesNoField)
                {
                    if (value.ToString().ToLower().Equals("yes"))
                    {
                        value = true;
                    }
                    else if (value.ToString().ToLower().Equals("no"))
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
        /// <param name="result">The parsed results in dictionary format</param>
        private Dictionary<string, Dictionary<string, WebFieldData>> ParseXML(SurveyManagerService.SurveyAnswerResponse pSurveyAnswer)
        {
            Dictionary<string, Dictionary<string, WebFieldData>> result = new Dictionary<string, Dictionary<string, WebFieldData>>(StringComparer.OrdinalIgnoreCase);
           // SetFilterProperties(DownLoadType);

            foreach (SurveyManagerService.SurveyAnswerDTO surveyAnswer in pSurveyAnswer.SurveyResponseList)
            {
                if (SurveyStatus == 0)
                {
                    if ((surveyAnswer.IsDraftMode == IsDraftMode))
                    {
                        AddSurveyAnswerResult(result, surveyAnswer);
                    }
                }
                else
                {
                    if ((surveyAnswer.IsDraftMode == IsDraftMode) && (surveyAnswer.Status == SurveyStatus))
                    {
                        AddSurveyAnswerResult(result, surveyAnswer);
                    }
                }
            }

            return result;
        }
        private Dictionary<string, Dictionary<string, WebFieldData>> ParseXMLV3(SurveyManagerServiceV3.SurveyAnswerResponse pSurveyAnswer)
        {
            Dictionary<string, Dictionary<string, WebFieldData>> result = new Dictionary<string, Dictionary<string, WebFieldData>>(StringComparer.OrdinalIgnoreCase);
            // SetFilterProperties(DownLoadType);

            foreach (SurveyManagerServiceV3.SurveyAnswerDTO surveyAnswer in pSurveyAnswer.SurveyResponseList)
            {
                if (SurveyStatus == 0)
                {
                   
                        AddSurveyAnswerResultV3(result, surveyAnswer);
                  
                }
                else
                {
                    if ((surveyAnswer.IsDraftMode == IsDraftMode) && (surveyAnswer.Status == SurveyStatus))
                    {
                        AddSurveyAnswerResultV3(result, surveyAnswer);
                    }
                }
            }

            return result;
        }
        private void AddSurveyAnswerResult(Dictionary<string, Dictionary<string, WebFieldData>> result, SurveyManagerService.SurveyAnswerDTO surveyAnswer)
        {
            result.Add(surveyAnswer.ResponseId, new Dictionary<string, WebFieldData>(StringComparer.OrdinalIgnoreCase));
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(surveyAnswer.XML);

            foreach (XmlNode docElement in doc.SelectNodes("//ResponseDetail"))
            {
                string fieldName = docElement.Attributes.GetNamedItem("QuestionName").Value;
                object fieldValue = FormatWebFieldData(fieldName, docElement.InnerText);

                WebFieldData wfData = new WebFieldData();
                wfData.RecordGUID = surveyAnswer.ResponseId;
                wfData.FieldName = fieldName;
                wfData.FieldValue = fieldValue;
                wfData.Status = surveyAnswer.Status;

                if (result[surveyAnswer.ResponseId].Keys.Contains(wfData.FieldName) == false)
                {
                    result[surveyAnswer.ResponseId].Add(wfData.FieldName, wfData);
                }
            }
        }
        private void AddSurveyAnswerResultV3(Dictionary<string, Dictionary<string, WebFieldData>> result, SurveyManagerServiceV3.SurveyAnswerDTO surveyAnswer)
        {
            result.Add(surveyAnswer.ResponseId, new Dictionary<string, WebFieldData>(StringComparer.OrdinalIgnoreCase));
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(surveyAnswer.XML);

            foreach (XmlNode docElement in doc.SelectNodes("//ResponseDetail"))
            {
                string fieldName = docElement.Attributes.GetNamedItem("QuestionName").Value;
                object fieldValue = FormatWebFieldData(fieldName, docElement.InnerText);

                WebFieldData wfData = new WebFieldData();
                wfData.RecordGUID = surveyAnswer.ResponseId;
                wfData.FieldName = fieldName;
                wfData.FieldValue = fieldValue;
                wfData.Status = surveyAnswer.Status;

                if (result[surveyAnswer.ResponseId].Keys.Contains(wfData.FieldName) == false)
                {
                    result[surveyAnswer.ResponseId].Add(wfData.FieldName, wfData);
                }
            }
        }
        /// <summary>
        /// Parses XML from the web survey
        /// </summary>
        /// <param name="result">The parsed results in dictionary format</param>
        private int ParseXMLForProgressBar(SurveyAnswerResponse result)
        {
            int count = 0;

            foreach (Epi.Web.Common.DTO.SurveyAnswerDTO surveyAnswer in result.SurveyResponseList)
            {
                WebFieldData wfData = new WebFieldData();

                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.LoadXml(surveyAnswer.XML);

                foreach (XmlElement docElement in doc.ChildNodes)
                {
                    if (docElement.Name.ToLower().Equals("surveyresponse"))
                    {
                        foreach (XmlElement surveyElement in docElement.ChildNodes)
                        {
                            if (surveyElement.Name.ToLower().Equals("page") && surveyElement.Attributes.Count > 0 && surveyElement.Attributes[0].Name.ToLower().Equals("pagenumber"))
                            {
                                foreach (XmlElement pageElement in surveyElement.ChildNodes)
                                {
                                    if (pageElement.Name.ToLower().Equals("responsedetail"))
                                    {
                                        if (wfData.Status == 3)
                                        {
                                            count++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return count;
        }

        private WebFieldData FindWebFieldData(List<WebFieldData> surveyResponses, string GUID, string fieldName, int pageNumber)
        {
            foreach (WebFieldData webFieldData in surveyResponses)
            {
                if (webFieldData.RecordGUID == GUID && webFieldData.FieldName == fieldName && webFieldData.Page == pageNumber)
                {
                    return webFieldData;
                }
            }

            return new WebFieldData(); // this is bad, fix
        }

        /// <summary>
        /// Processes all of the fields on a given form, page-by-page, except for the fields on the base table.
        /// </summary>
        /// <param name="result">The results from the web request</param>        
        /// <param name="destinationView">The destination form</param>
        /// <param name="destinationGUIDList">The list of GUIDs that exist in the destination</param>
        private void ProcessPages(Dictionary<string, Dictionary<string, WebFieldData>> pFieldDataList, View destinationView, List<string> destinationGUIDList)
        {
            Dictionary<int, List<WebFieldData>> pagedFieldDataDictionary = new Dictionary<int, List<WebFieldData>>();
            List<string> GUIDList = new List<string>();
            for (int i = 0; i < destinationView.Pages.Count; i++)
            {
                this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), "Processing records on page " + (i + 1).ToString() + " of " + destinationView.Pages.Count.ToString() + "...");

                int recordsInserted = 0;

                Page destinationPage = destinationView.Pages[i];
               
                foreach(KeyValuePair<string, Dictionary<string, WebFieldData>> kvp in pFieldDataList)
                {
                    string currentGUID = string.Empty;
                    string lastGUID = string.Empty;

                    WordBuilder fieldNames = new WordBuilder(StringLiterals.COMMA);
                    WordBuilder fieldValues = new WordBuilder(StringLiterals.COMMA);
                    List<QueryParameter> fieldValueParams = new List<QueryParameter>();

                   

                    foreach (Field PageField in destinationPage.Fields)
                    {

                        if (PageField is IDataField)
                        {
                            IDataField field = PageField as IDataField;
                            string FieldName = ((Epi.INamedObject)field).Name;

                            currentGUID = kvp.Key;//fieldDataList[FieldName].RecordGUID;

                            if (importWorker.CancellationPending)
                            {
                                this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), "Import cancelled.");
                                return;
                            }

                            if (kvp.Value.ContainsKey(FieldName))
                            {
                                string GUID = kvp.Value[FieldName].RecordGUID;

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
                                sb.Append(FieldName);
                                sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET);
                                sb.Append(StringLiterals.EQUAL);

                                sb.Append(StringLiterals.COMMERCIAL_AT);
                                sb.Append(FieldName);

                                QueryParameter param = GetQueryParameterForField(kvp.Value[FieldName], destinationPage);
                                if (param != null)
                                {
                                    Query updateQuery = destinationProjectDataDriver.CreateQuery(updateHeader + StringLiterals.SPACE + sb.ToString() + StringLiterals.SPACE + whereClause);
                                    updateQuery.Parameters.Add(param);
                                    destinationProjectDataDriver.ExecuteNonQuery(updateQuery);

                                    if (!GUIDList.Contains(GUID))
                                    {
                                        GUIDList.Add(GUID);
                                        recordsInserted++;
                                        this.BeginInvoke(new SetProgressBarDelegate(IncrementProgressBarValue), 1);
                                    }
                                }
                            }
                        }
                    }
                //}
                //catch (Exception ex)
                //{
                //    this.BeginInvoke(new SetStatusDelegate(AddErrorStatusMessage), ex.Message);
                //}
                //finally
                //{
                //}
                //this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), "On page '" + destinationPage.Name + "', " + recordsInserted.ToString() + " record(s) inserted and " + recordsUpdated.ToString() + " record(s) updated.");                
                }
            }
          //  var tempClient = this.client;
            if (GUIDList.Count > 0)
                {
                 UpdateRecordStatus(GUIDList);
                }
        }

        private void UpdateRecordStatus(List<string> GUIDList)
            {
                Epi.SurveyManagerServiceV3.SurveyAnswerRequest Request = new Epi.SurveyManagerServiceV3.SurveyAnswerRequest();
                List<Epi.SurveyManagerServiceV3.SurveyAnswerDTO> DTOList = new List<Epi.SurveyManagerServiceV3.SurveyAnswerDTO>();
            foreach (var Id in GUIDList)
                {

                    Epi.SurveyManagerServiceV3.SurveyAnswerDTO DTO = new SurveyManagerServiceV3.SurveyAnswerDTO();

                DTO.ResponseId = Id;
                DTO.Status = 4;
                DTOList.Add(DTO);
                }
            Request.SurveyAnswerList = DTOList.ToArray();
            var MyClient = Epi.Core.ServiceClient.ServiceClient.GetClientV3();
            MyClient.UpdateRecordStatus(Request);
           // client.UpdateRecordStatus(Request);
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
            string message = DateTime.Now + ": Warning: " + statusMessage;
            lbxStatus.Items.Add(message);            
            Logger.Log(message);
        }

        /// <summary>
        /// Adds a status error message to the status list box.
        /// </summary>
        /// <param name="statusMessage"></param>
        private void AddErrorStatusMessage(string statusMessage)
        {
            string message = DateTime.Now + ": Error: " + statusMessage;
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
            //    if (Epi.Data.Services.AppData.Instance.IsReservedWord(sourceField.Name) && (sourceField.Name.ToLower() != "uniquekey" && sourceField.Name.ToLower() != "recstatus" && sourceField.Name.ToLower() != "fkey"))
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
            if (!string.IsNullOrEmpty(textProject.Text))
            {
                progressBar.Style = ProgressBarStyle.Marquee;
                progressBar.Visible = true;
                progressBar.Value = 0;
                progressBar.Minimum = 0;

                this.SurveyId = textProject.Text;
                this.OrganizationKey = textOrganization.Text;
                this.PublishKey = textData.Text;

                lbxStatus.Items.Clear();
                //sourceView = sourceProject.Views[cmbFormName.SelectedItem.ToString()];
                //lastRecordId = sourceView.GetLastRecordId();

                textProgress.Text = string.Empty;
                AddStatusMessage(SharedStrings.IMPORT_DATA_WEB_REQUESTED + " " + System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString());

                if (importWorker.WorkerSupportsCancellation)
                {
                    importWorker.CancelAsync();
                }

                this.Cursor = Cursors.WaitCursor;
                //DownLoadType = cmbImportType.SelectedIndex;
                //cmbImportType options:
                //    0.  Completed records in draft mode
                //    1.  All records in draft mode
                //    2.  Completed records in final mode
                //    3.  All records in final mode
                IsDraftMode = rdbDraftMode.Checked;
                SurveyStatus = 3;

                if (rdbAllRecords.Checked) SurveyStatus = 0;

                if (rdbSubmittedIncremental.Checked) SurveyStatus = 4;

                requestWorker = new BackgroundWorker();
                requestWorker.WorkerSupportsCancellation = true;
                requestWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(requestWorker_DoWork);
                requestWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(requestWorker_WorkerCompleted);
                requestWorker.RunWorkerAsync(SurveyId);
            }
            //DoImport();
        }

        /// <summary>
        /// Handles the WorkerCompleted event for the request worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void requestWorker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
        if (e.Result != null && e.Result is SurveyManagerService.SurveyAnswerRequest)
            {
            SurveyManagerService.SurveyAnswerRequest Request = (SurveyManagerService.SurveyAnswerRequest)e.Result;
                AddStatusMessage(SharedStrings.IMPORT_DATA_COMPLETE);
                DoImport(Request);
            }
            else
            {
                AddErrorStatusMessage(SharedStrings.IMPORT_ERROR_COMM_FAILED);
                StopImport();
            }
        }

        /// <summary>
        /// Handles the DoWorker event for the request worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void requestWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                try
                {

                    var ServiceVersion = config.Settings.WebServiceEndpointAddress.ToLower();
                    if (!string.IsNullOrEmpty(ServiceVersion) && (ServiceVersion.Contains(Epi.Constants.surveyManagerservice)))
                    {
                        SurveyManagerService.SurveyAnswerRequest Request = SetMessageObject(true);

                        var MyClient = Epi.Core.ServiceClient.ServiceClient.GetClient();
                        var Result = MyClient.GetSurveyAnswer(Request);
                        Pages = Result.NumberOfPages;
                        PageSize = Result.PageSize;
                        e.Result = Request;
                    }
                    if (!string.IsNullOrEmpty(ServiceVersion) && (ServiceVersion.Contains(Epi.Constants.surveyManagerservicev2) ))
                    {
                        SurveyManagerServiceV2.SurveyAnswerRequest Request = SetMessageObjectV2(true);
                      
                        var MyClient = Epi.Core.ServiceClient.ServiceClient.GetClientV2();
                                      
                        var Result = MyClient.GetSurveyAnswer(Request);
                        Pages = Result.NumberOfPages;
                        PageSize = Result.PageSize;
                        e.Result = Request;
                    }
                    if (!string.IsNullOrEmpty(ServiceVersion) && (ServiceVersion.Contains(Epi.Constants.surveyManagerservicev3) ))
                    {
                        SurveyManagerServiceV3.SurveyAnswerRequest Request = SetMessageObjectV3(true);

                        var MyClient = Epi.Core.ServiceClient.ServiceClient.GetClientV3();
                        SurveyManagerServiceV3.SurveyAnswerResponse Result = MyClient.GetSurveyAnswer(Request);
                        Pages = Result.NumberOfPages;
                        PageSize = Result.PageSize;
                        e.Result = Request;
                    }

                    
                }
                catch (System.ServiceModel.CommunicationException ex)
                {
                    e.Result = null;                    
                }
                catch (Exception ex)
                {
                    e.Result = null;                    
                }
            }
        }

        private SurveyManagerService.SurveyAnswerRequest SetMessageObject(bool ReturnSizeInfoOnly)
        {
            SurveyManagerService.SurveyAnswerRequest Request = new SurveyManagerService.SurveyAnswerRequest();

            Request.Criteria = new SurveyManagerService.SurveyAnswerCriteria();
            Request.Criteria.SurveyId = SurveyId;
            Request.Criteria.UserPublishKey = new Guid(PublishKey);
            Request.Criteria.OrganizationKey = new Guid(OrganizationKey);
            Request.Criteria.ReturnSizeInfoOnly = ReturnSizeInfoOnly;
            Request.Criteria.StatusId = SurveyStatus;
            Request.SurveyAnswerList = new List<SurveyManagerService.SurveyAnswerDTO>().ToArray();

            
            return Request;
        }
        private SurveyManagerServiceV2.SurveyAnswerRequest SetMessageObjectV2(bool ReturnSizeInfoOnly)
        {
            SurveyManagerServiceV2.SurveyAnswerRequest Request = new SurveyManagerServiceV2.SurveyAnswerRequest();

            Request.Criteria = new SurveyManagerServiceV2.SurveyAnswerCriteria();
            Request.Criteria.SurveyId = SurveyId;
            Request.Criteria.UserPublishKey = new Guid(PublishKey);
            Request.Criteria.OrganizationKey = new Guid(OrganizationKey);
            Request.Criteria.ReturnSizeInfoOnly = ReturnSizeInfoOnly;
            Request.Criteria.StatusId = SurveyStatus;
            Request.SurveyAnswerList = new List<SurveyManagerServiceV2.SurveyAnswerDTO>().ToArray();


            return Request;
        }
        private SurveyManagerServiceV3.SurveyAnswerRequest SetMessageObjectV3(bool ReturnSizeInfoOnly)
        {
            SurveyManagerServiceV3.SurveyAnswerRequest Request = new SurveyManagerServiceV3.SurveyAnswerRequest();

            Request.Criteria = new SurveyManagerServiceV3.SurveyAnswerCriteria();
            Request.Criteria.SurveyId = SurveyId;
            Request.Criteria.UserPublishKey = new Guid(PublishKey);
            Request.Criteria.OrganizationKey = new Guid(OrganizationKey);
            Request.Criteria.ReturnSizeInfoOnly = ReturnSizeInfoOnly;
            Request.Criteria.StatusId = SurveyStatus;
            Request.Criteria.IsDraftMode = IsDraftMode;

           
            return Request;
        }

        /// <summary>
        /// Handles the WorkerCompleted event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            stopwatch.Stop();

            if (e.Result is Exception)
            {
                this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), SharedStrings.IMPORT_DATA_FAILED + " " + SharedStrings.IMPORT_DATA_TIME_ELAPSED + ": " + stopwatch.Elapsed.ToString());
                this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), SharedStrings.IMPORT_DATA_FAILED + " " + SharedStrings.IMPORT_DATA_TIME_ELAPSED + ": " + stopwatch.Elapsed.ToString());
            }
            else
            {
                this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), SharedStrings.IMPORT_DATA_COMPLETE + " " + SharedStrings.IMPORT_DATA_TIME_ELAPSED + ": " + stopwatch.Elapsed.ToString());
                this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), SharedStrings.IMPORT_DATA_COMPLETE + " " + SharedStrings.IMPORT_DATA_TIME_ELAPSED + ": " + stopwatch.Elapsed.ToString());
            }

            StopImport();            
        }

        /// <summary>
        /// Handles the DoWorker event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
           // SetFilterProperties(DownLoadType);
            lock (syncLock)
            {
            if (e.Argument is SurveyManagerService.SurveyAnswerRequest)
                {
                    var ServiceVersion = config.Settings.WebServiceEndpointAddress.ToLower();
                  

                    //List<SurveyManagerService.SurveyAnswerResponse> Results = new List<SurveyManagerService.SurveyAnswerResponse>();                    

                    for (int i = 1; i <= Pages; i++)
                    {
                       // List<SurveyManagerService.SurveyAnswerResponse> Results = new List<SurveyManagerService.SurveyAnswerResponse>();
                  
                        
                        try
                        {
                           
                           

                           if (!string.IsNullOrEmpty(ServiceVersion) && (ServiceVersion.Contains(Epi.Constants.surveyManagerservice) || ServiceVersion.Contains(Epi.Constants.surveyManagerservicev2)))
                           {
                               List<SurveyManagerService.SurveyAnswerResponse> Results = new List<SurveyManagerService.SurveyAnswerResponse>();
                               SurveyManagerService.SurveyAnswerRequest Request = SetMessageObject(false);

                               Results = GetSurveyAnswerResponse(i, Request);
                               this.BeginInvoke(new SetMaxProgressBarValueDelegate(SetProgressBarMaximum), Results.Count);

                               foreach (SurveyManagerService.SurveyAnswerResponse Result in Results)
                               {
                                   try
                                   {
                                       Query selectQuery = destinationProjectDataDriver.CreateQuery("SELECT [GlobalRecordId] FROM [" + destinationView.TableName + "]");
                                       IDataReader destReader = destinationProjectDataDriver.ExecuteReader(selectQuery);
                                       List<string> destinationGUIDList = new List<string>();
                                       while (destReader.Read())
                                       {
                                           destinationGUIDList.Add(destReader[0].ToString().ToUpper());
                                       }

                                       wfList = ParseXML(Result);

                                       ProcessBaseTable(wfList, destinationView, destinationGUIDList);
                                       ProcessPages(wfList, destinationView, destinationGUIDList);
                                       //ProcessGridFields(sourceView, destinationView);
                                       //ProcessRelatedForms(sourceView, destinationView, viewsToProcess);
                                   }
                                   catch (Exception ex)
                                   {
                                       this.BeginInvoke(new SetStatusDelegate(AddErrorStatusMessage), ex.Message);
                                       e.Result = ex;
                                       return;
                                   }
                               }
                           }
                           if (!string.IsNullOrEmpty(ServiceVersion) && (ServiceVersion.Contains(Epi.Constants.surveyManagerservicev3)))
                           {
                               List<SurveyManagerServiceV3.SurveyAnswerResponse> Results = new List<SurveyManagerServiceV3.SurveyAnswerResponse>();
                               SurveyManagerServiceV3.SurveyAnswerRequest Request = SetMessageObjectV3(false);

                               Results = GetSurveyAnswerResponseV3(i, Request);
                               this.BeginInvoke(new SetMaxProgressBarValueDelegate(SetProgressBarMaximum), Results.Count);

                               foreach (SurveyManagerServiceV3.SurveyAnswerResponse Result in Results)
                               {
                                   try
                                   {
                                       Query selectQuery = destinationProjectDataDriver.CreateQuery("SELECT [GlobalRecordId] FROM [" + destinationView.TableName + "]");
                                       IDataReader destReader = destinationProjectDataDriver.ExecuteReader(selectQuery);
                                       List<string> destinationGUIDList = new List<string>();
                                       while (destReader.Read())
                                       {
                                           destinationGUIDList.Add(destReader[0].ToString().ToUpper());
                                       }

                                       wfList = ParseXMLV3(Result);

                                       ProcessBaseTable(wfList, destinationView, destinationGUIDList);
                                       ProcessPages(wfList, destinationView, destinationGUIDList);
                                       //ProcessGridFields(sourceView, destinationView);
                                       //ProcessRelatedForms(sourceView, destinationView, viewsToProcess);
                                   }
                                   catch (Exception ex)
                                   {
                                       this.BeginInvoke(new SetStatusDelegate(AddErrorStatusMessage), ex.Message);
                                       e.Result = ex;
                                       return;
                                   }
                               }
                           }

                        

                     
                         }
                        catch (Exception ex)
                        {
                            this.BeginInvoke(new SetStatusDelegate(AddErrorStatusMessage), ex.Message);
                            e.Result = ex;
                            return;
                        }
                    }
                }
            }
        }

        private List<SurveyManagerService.SurveyAnswerResponse> GetSurveyAnswerResponse(int i, SurveyManagerService.SurveyAnswerRequest Request)
        {
            List<SurveyManagerService.SurveyAnswerResponse> Results = new List<SurveyManagerService.SurveyAnswerResponse>();
            Request.Criteria.PageNumber = i;
            Request.Criteria.PageSize = PageSize;
            var MyClient = Epi.Core.ServiceClient.ServiceClient.GetClient();
            SurveyManagerService.SurveyAnswerResponse Result = MyClient.GetSurveyAnswer(Request);
             
            Results.Add(Result);

            return Results;
        }
        private List<SurveyManagerServiceV3.SurveyAnswerResponse> GetSurveyAnswerResponseV3(int i, SurveyManagerServiceV3.SurveyAnswerRequest Request)
        {
            List<SurveyManagerServiceV3.SurveyAnswerResponse> Results = new List<SurveyManagerServiceV3.SurveyAnswerResponse>();
            Request.Criteria.PageNumber = i;
            Request.Criteria.PageSize = PageSize;
            var MyClient = Epi.Core.ServiceClient.ServiceClient.GetClientV3();
            SurveyManagerServiceV3.SurveyAnswerResponse Result = MyClient.GetSurveyAnswer(Request);
             
            Results.Add(Result);

            return Results;
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

        private void textProject_TextChanged(object sender, EventArgs e)
        {
            if (textProject.Text.Length == 0)
            {
                btnOK.Enabled = false;
            }
            else
            {
                btnOK.Enabled = true;
            }
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
    }
}
