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
    public partial class ImportWebEnterDataForm : Form
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
        
        private EWEManagerService.EWEManagerServiceV2Client client;
        private Dictionary<string, Dictionary<string, WebFieldData>> _webFieldDataList;
        private bool IsDraftMode;
        private int SurveyStatus;
        private int DownLoadType;
        #endregion // Private Members

        #region Delegates
        private delegate void SetStatusDelegate(string statusMessage);
        #endregion // Delegates

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public ImportWebEnterDataForm()
            {
            InitializeComponent();
            }

        /// <summary>
        /// Constructor
        /// </summary>        
        /// <param name="destinationView">The destination form; should be the currently-open view</param>
        public ImportWebEnterDataForm(View destinationView)
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
            try
                {
                this.destinationProjectDataDriver = destinationProject.CollectedData.GetDbDriver();
                this.config = Configuration.GetNewInstance();

                this.importWorker = new BackgroundWorker();
                this.importWorker.WorkerSupportsCancellation = true;

                this.requestWorker = new BackgroundWorker();
                this.requestWorker.WorkerSupportsCancellation = true;

                this.IsBatchImport = false;

                this.cmbImportType.SelectedIndex = 0;

                if (config.Settings.WebServiceAuthMode == 1) // Windows Authentication
                    {
                    System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();
                    binding.Name = "BasicHttpBinding";
                    binding.CloseTimeout = new TimeSpan(0, 1, 0);
                    binding.OpenTimeout = new TimeSpan(0, 1, 0);
                    binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
                    binding.SendTimeout = new TimeSpan(0, 1, 0);
                    binding.AllowCookies = false;
                    binding.BypassProxyOnLocal = false;
                    binding.HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
                    binding.MaxBufferPoolSize = config.Settings.WebServiceMaxBufferPoolSize;
                    binding.MaxReceivedMessageSize = config.Settings.WebServiceMaxReceivedMessageSize;
                    binding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
                    binding.TextEncoding = System.Text.Encoding.UTF8;
                    binding.TransferMode = System.ServiceModel.TransferMode.Buffered;
                    binding.UseDefaultWebProxy = true;
                    binding.ReaderQuotas.MaxDepth = config.Settings.WebServiceReaderMaxDepth;
                    binding.ReaderQuotas.MaxStringContentLength = config.Settings.WebServiceReaderMaxStringContentLength;
                    binding.ReaderQuotas.MaxArrayLength = config.Settings.WebServiceReaderMaxArrayLength;
                    binding.ReaderQuotas.MaxBytesPerRead = config.Settings.WebServiceReaderMaxBytesPerRead;
                    binding.ReaderQuotas.MaxNameTableCharCount = config.Settings.WebServiceReaderMaxNameTableCharCount;

                    binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly;
                    binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;
                    binding.Security.Transport.ProxyCredentialType = System.ServiceModel.HttpProxyCredentialType.None;
                    binding.Security.Transport.Realm = string.Empty;

                    binding.Security.Message.ClientCredentialType = System.ServiceModel.BasicHttpMessageCredentialType.UserName;

                    System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(config.Settings.EWEServiceEndpointAddress);

                    client = new EWEManagerService.EWEManagerServiceV2Client(binding, endpoint);

                    client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
                    client.ChannelFactory.Credentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
                    }
                else
                    {


                    if (config.Settings.WebServiceBindingMode.Equals("wshttp", StringComparison.OrdinalIgnoreCase))
                        {
                        System.ServiceModel.WSHttpBinding binding = new System.ServiceModel.WSHttpBinding();
                        binding.Name = "WSHttpBinding";
                        binding.CloseTimeout = new TimeSpan(0, 1, 0);
                        binding.OpenTimeout = new TimeSpan(0, 1, 0);
                        binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
                        binding.SendTimeout = new TimeSpan(0, 1, 0);
                        binding.BypassProxyOnLocal = false;
                        binding.TransactionFlow = false;
                        binding.HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
                        binding.MaxBufferPoolSize = config.Settings.WebServiceMaxBufferPoolSize;
                        binding.MaxReceivedMessageSize = config.Settings.WebServiceMaxReceivedMessageSize;
                        binding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
                        binding.TextEncoding = System.Text.Encoding.UTF8;
                        binding.UseDefaultWebProxy = true;
                        binding.AllowCookies = false;

                        binding.ReaderQuotas.MaxDepth = config.Settings.WebServiceReaderMaxDepth;
                        binding.ReaderQuotas.MaxStringContentLength = config.Settings.WebServiceReaderMaxStringContentLength;
                        binding.ReaderQuotas.MaxArrayLength = config.Settings.WebServiceReaderMaxArrayLength;
                        binding.ReaderQuotas.MaxBytesPerRead = config.Settings.WebServiceReaderMaxBytesPerRead;
                        binding.ReaderQuotas.MaxNameTableCharCount = config.Settings.WebServiceReaderMaxNameTableCharCount;

                        binding.ReliableSession.Ordered = true;
                        binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 10, 0);
                        binding.ReliableSession.Enabled = false;

                        binding.Security.Mode = System.ServiceModel.SecurityMode.Message;
                        binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;
                        binding.Security.Transport.ProxyCredentialType = System.ServiceModel.HttpProxyCredentialType.None;
                        binding.Security.Transport.Realm = string.Empty;
                        binding.Security.Message.ClientCredentialType = System.ServiceModel.MessageCredentialType.Windows;
                        binding.Security.Message.NegotiateServiceCredential = true;

                        System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(config.Settings.EWEServiceEndpointAddress);

                        client = new EWEManagerService.EWEManagerServiceV2Client(binding, endpoint);

                        }
                    else
                        {
                        System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();
                        binding.Name = "BasicHttpBinding";
                        binding.CloseTimeout = new TimeSpan(0, 1, 0);
                        binding.OpenTimeout = new TimeSpan(0, 1, 0);
                        binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
                        binding.SendTimeout = new TimeSpan(0, 1, 0);
                        binding.AllowCookies = false;
                        binding.BypassProxyOnLocal = false;
                        binding.HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
                        binding.MaxBufferPoolSize = config.Settings.WebServiceMaxBufferPoolSize;
                        binding.MaxReceivedMessageSize = config.Settings.WebServiceMaxReceivedMessageSize;
                        binding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
                        binding.TextEncoding = System.Text.Encoding.UTF8;
                        binding.TransferMode = System.ServiceModel.TransferMode.Buffered;
                        binding.UseDefaultWebProxy = true;
                        binding.ReaderQuotas.MaxDepth = config.Settings.WebServiceReaderMaxDepth;
                        binding.ReaderQuotas.MaxStringContentLength = config.Settings.WebServiceReaderMaxStringContentLength;
                        binding.ReaderQuotas.MaxArrayLength = config.Settings.WebServiceReaderMaxArrayLength;
                        binding.ReaderQuotas.MaxBytesPerRead = config.Settings.WebServiceReaderMaxBytesPerRead;
                        binding.ReaderQuotas.MaxNameTableCharCount = config.Settings.WebServiceReaderMaxNameTableCharCount;


                        binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly;
                        binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;
                        binding.Security.Transport.ProxyCredentialType = System.ServiceModel.HttpProxyCredentialType.None;
                        binding.Security.Transport.Realm = string.Empty;

                        System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(config.Settings.EWEServiceEndpointAddress);

                        client = new EWEManagerService.EWEManagerServiceV2Client(binding, endpoint);
                        }

                    }
                this._webFieldDataList = new Dictionary<string, Dictionary<string, WebFieldData>>(StringComparer.OrdinalIgnoreCase);
                }
            catch (Exception ex)
                {
                SetStatusMessage("Error: Web service information was not found.");
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
            Field dataField = sourcePage.view.Fields[fieldData.FieldName];
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
        private void DoImport(EWEManagerService.SurveyAnswerRequest Request)
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
                    //DownLoadType = cmbImportType.SelectedIndex;
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
           // string destinationTable = destinationView.TableName;
            string destinationTable = "";

            foreach (KeyValuePair<string, Dictionary<string, WebFieldData>> surveyAnswer in result)
                {
                int ViewId = surveyAnswer.Value.Select(x => x.Value.ViewId).ToList().First();

                View NewView = destinationProject.GetViewById(ViewId);
                if (NewView != null)
                { 
                destinationView = NewView;

                }
                destinationTable = destinationView.TableName;
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
               // string FKEY = string.Empty; // sourceReader["FKEY"].ToString(); // FKEY not needed, no related forms to process
                string FKEY = surveyAnswer.Value.Select(x => x.Value.ParentId).ToList().First() ;
               
                QueryParameter paramFkey = new QueryParameter("@FKEY", DbType.String, FKEY); // don't add this yet
                QueryParameter paramGUID = new QueryParameter("@GlobalRecordId", DbType.String, GUID);
                fieldValueParams.Add(paramGUID);


                fieldNames.Append("FirstSaveLogonName");
                fieldValues.Append("@FirstSaveLogonName");
                QueryParameter paramFirstSaveLogonName = new QueryParameter("@FirstSaveLogonName", DbType.String, System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString());
                fieldValueParams.Add(paramFirstSaveLogonName);

                fieldNames.Append("LastSaveLogonName");
                fieldValues.Append("@LastSaveLogonName");
                QueryParameter paramLastSaveLogonName = new QueryParameter("@LastSaveLogonName", DbType.String, System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString());
                fieldValueParams.Add(paramLastSaveLogonName);

                fieldNames.Append("FirstSaveTime");
                fieldValues.Append("@FirstSaveTime");
                QueryParameter paramFirstSaveTime = new QueryParameter("@FirstSaveTime", DbType.DateTime2, DateTime.Now);
                fieldValueParams.Add(paramFirstSaveTime);


                fieldNames.Append("LastSaveTime");
                fieldValues.Append("@LastSaveTime");
                QueryParameter paramLastSaveTime = new QueryParameter("@LastSaveTime", DbType.DateTime2, DateTime.Now);
                fieldValueParams.Add(paramLastSaveTime);

                if (destinationGUIDList.Contains(GUID))
                    {
                    recordsUpdated++;
                    if (update)
                        {
                        // UPDATE matching records
                        //    string updateHeader = string.Empty;
                        //    string whereClause = string.Empty;
                        //    fieldValueParams = new List<QueryParameter>();
                        //    StringBuilder sb = new StringBuilder();

                        //    // Build the Update statement which will be reused
                        //    sb.Append(SqlKeyWords.UPDATE);
                        //    sb.Append(StringLiterals.SPACE);
                        //    sb.Append(destinationProjectDataDriver.InsertInEscape(destinationTable));
                        //    sb.Append(StringLiterals.SPACE);
                        //    sb.Append(SqlKeyWords.SET);
                        //    sb.Append(StringLiterals.SPACE);

                        //    updateHeader = sb.ToString();

                        //    sb.Remove(0, sb.ToString().Length);

                        //    // Build the WHERE caluse which will be reused
                        //    sb.Append(SqlKeyWords.WHERE);
                        //    sb.Append(StringLiterals.SPACE);
                        //    sb.Append(destinationProjectDataDriver.InsertInEscape(ColumnNames.GLOBAL_RECORD_ID));
                        //    sb.Append(StringLiterals.EQUAL);
                        //    sb.Append("'");
                        //    sb.Append(GUID);
                        //    sb.Append("'");
                        //    whereClause = sb.ToString();

                        //    sb.Remove(0, sb.ToString().Length);

                        //    //if (sourceView.ForeignKeyFieldExists)
                        //    if (!string.IsNullOrEmpty(FKEY))
                        //    {
                        //        sb.Append(StringLiterals.LEFT_SQUARE_BRACKET);
                        //        sb.Append("FKEY");
                        //        sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET);
                        //        sb.Append(StringLiterals.EQUAL);

                        //        sb.Append(StringLiterals.COMMERCIAL_AT);
                        //        sb.Append("FKEY");                               
                        //        fieldValueParams.Add(paramFkey);

                        //        Query updateQuery = destinationProjectDataDriver.CreateQuery(updateHeader + StringLiterals.SPACE + sb.ToString() + StringLiterals.SPACE + whereClause);
                        //        updateQuery.Parameters = fieldValueParams;

                        //        //destinationProjectDataDriver.ExecuteNonQuery(updateQuery);

                        //        sb.Remove(0, sb.ToString().Length);
                        //        fieldValueParams.Clear();

                        //        recordsUpdated++;
                        //    }
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
        private Dictionary<string, Dictionary<string, WebFieldData>> ParseXML(Epi.EWEManagerService.SurveyAnswerResponse pSurveyAnswer)
            {
            Dictionary<string, Dictionary<string, WebFieldData>> result = new Dictionary<string, Dictionary<string, WebFieldData>>(StringComparer.OrdinalIgnoreCase);
           // SetFilterProperties(DownLoadType);

            foreach (Epi.EWEManagerService.SurveyAnswerDTO surveyAnswer in pSurveyAnswer.SurveyResponseList)
                {
                if (SurveyStatus == 0)
                    {
                    if ((surveyAnswer.IsDraftMode == IsDraftMode))
                        {
                       // AddSurveyAnswerResult(result, surveyAnswer);
                        foreach (var item in surveyAnswer.ResponseHierarchyIds)
                            {
                            AddSurveyAnswerResult(result, item);
                            }
                        }
                    }
                else
                    {
               //     if ((surveyAnswer.IsDraftMode == IsDraftMode) && (surveyAnswer.Status == SurveyStatus))
                    if ((surveyAnswer.IsDraftMode == IsDraftMode)  )
                        {
                        // AddSurveyAnswerResult(result, surveyAnswer);
                        foreach (var item in surveyAnswer.ResponseHierarchyIds)
                            {
                            AddSurveyAnswerResult(result, item);
                            }
                        }
                    }
                }

            return result;
            }

        private void AddSurveyAnswerResult(Dictionary<string, Dictionary<string, WebFieldData>> result, Epi.EWEManagerService.SurveyAnswerDTO surveyAnswer)
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
                wfData.ViewId = surveyAnswer.ViewId;
                wfData.ParentId = surveyAnswer.RelateParentId;
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
          //  Dictionary<int, List<WebFieldData>> pagedFieldDataDictionary = new Dictionary<int, List<WebFieldData>>();

           

            //for (int i = 0; i < destinationView.Pages.Count; i++)
            //    {
            //    this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), "Processing records on page " + (i + 1).ToString() + " of " + destinationView.Pages.Count.ToString() + "...");

            //    int recordsInserted = 0;

            //    Page destinationPage = destinationView.Pages[i];
            List<string> GUIDList = new List<string>();
                foreach (KeyValuePair<string, Dictionary<string, WebFieldData>> kvp in pFieldDataList)
                    {
                    int ViewId = kvp.Value.Select(x => x.Value.ViewId).ToList().First();

                    View NewView = destinationProject.GetViewById(ViewId);
                    if (NewView != null)
                        {
                    destinationView = NewView;
                        }
                    string currentGUID = string.Empty;
                    string lastGUID = string.Empty;

                    WordBuilder fieldNames = new WordBuilder(StringLiterals.COMMA);
                    WordBuilder fieldValues = new WordBuilder(StringLiterals.COMMA);
                    List<QueryParameter> fieldValueParams = new List<QueryParameter>();

                  
                    for (int i = 0; i < destinationView.Pages.Count; i++)
                        {
                        this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), "Processing records on page " + (i + 1).ToString() + " of " + destinationView.Pages.Count.ToString() + "...");

                        int recordsInserted = 0;

                        Page destinationPage = destinationView.Pages[i];
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
               // }
                if (GUIDList.Count > 0)
                    {
                     UpdateRecordStatus(GUIDList);
                    }
            }
        private void UpdateRecordStatus(List<string> GUIDList)
            {
            Epi.EWEManagerService.SurveyAnswerRequest Request = new Epi.EWEManagerService.SurveyAnswerRequest();
            List<Epi.EWEManagerService.SurveyAnswerDTO> DTOList = new List<Epi.EWEManagerService.SurveyAnswerDTO>();
            foreach (var Id in GUIDList)
                {

                Epi.EWEManagerService.SurveyAnswerDTO DTO = new EWEManagerService.SurveyAnswerDTO();

                DTO.ResponseId = Id;
                DTO.Status = 4;
                DTOList.Add(DTO);
                }
            Request.SurveyAnswerList = DTOList.ToArray();

            client.UpdateRecordStatus(Request);
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

        private void SetFilterProperties(int UserSelection)
            {

            switch (UserSelection)
                {
                //case 0:
                //    IsDraftMode = true;
                //    SurveyStatus = 3;//Complete 
                //    break;
                //case 1:
                //    IsDraftMode = true;
                //    SurveyStatus = 0;//All
                //    break;
                case 0:
                    IsDraftMode =true;
                    //SurveyStatus = 3;//Complete 
                    break;
                case 1:
                    IsDraftMode = false;
                   // SurveyStatus = 3;//All 
                    break;


                }

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

            DownLoadType = cmbImportType.SelectedIndex;
            SetFilterProperties(DownLoadType);

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
                AddStatusMessage("Request for web data initiated by user " + System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString());

                if (importWorker.WorkerSupportsCancellation)
                    {
                    importWorker.CancelAsync();
                    }

                this.Cursor = Cursors.WaitCursor;

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
            if (e.Result != null && e.Result is EWEManagerService.SurveyAnswerRequest)
                {
                EWEManagerService.SurveyAnswerRequest Request = (EWEManagerService.SurveyAnswerRequest)e.Result;
                AddStatusMessage("Request for web data completed.");
                DoImport(Request);
                }
            else
                {
                AddErrorStatusMessage("Communication with the web service failed.");
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
                  //  Epi.Web.Common.Message.SurveyAnswerRequest Request = new Epi.Web.Common.Message.SurveyAnswerRequest();
                    EWEManagerService.SurveyAnswerRequest Request = new EWEManagerService.SurveyAnswerRequest();
                    EWEManagerService.SurveyAnswerCriteria Criteria = new EWEManagerService.SurveyAnswerCriteria();
                    Criteria.SurveyId = SurveyId;
                    Criteria.IsDraftMode = IsDraftMode;
                    Criteria.UserPublishKey = new Guid(PublishKey);
                    Criteria.OrganizationKey = new Guid(OrganizationKey);
                    Criteria.ReturnSizeInfoOnly = true;
                    Criteria.SurveyAnswerIdList = new  string[0];
                    Request.Criteria = Criteria; 

                    EWEManagerService.SurveyAnswerResponse Result = client.GetSurveyAnswer(Request);
                    Pages = Result.NumberOfPages;
                    PageSize = Result.PageSize;

                    e.Result = Request;
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
                this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), "Import failed. Time elapsed: " + stopwatch.Elapsed.ToString());
                this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), "Import failed. Time elapsed: " + stopwatch.Elapsed.ToString());
                }
            else
                {
                this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), "Import complete. Time elapsed: " + stopwatch.Elapsed.ToString());
                this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), "Import complete. Time elapsed: " + stopwatch.Elapsed.ToString());
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
            lock (syncLock)
                {
                if (e.Argument is EWEManagerService.SurveyAnswerRequest)
                    {
                    //Epi.Enter.EWEManagerService.SurveyAnswerRequest Request = e.Argument as Epi.Enter.EWEManagerService.SurveyAnswerRequest;
                    //Request.Criteria.SurveyId = SurveyId;
                    //Request.Criteria.UserPublishKey = new Guid(PublishKey);
                    //Request.Criteria.OrganizationKey = new Guid(OrganizationKey);
                    //Request.Criteria.ReturnSizeInfoOnly = false;

                    EWEManagerService.SurveyAnswerResponse Result = null;

                    EWEManagerService.SurveyAnswerRequest Request = new EWEManagerService.SurveyAnswerRequest();
                    EWEManagerService.SurveyAnswerCriteria Criteria = new EWEManagerService.SurveyAnswerCriteria();
                    Criteria.SurveyId = SurveyId;
                    Criteria.UserPublishKey = new Guid(PublishKey);
                    Criteria.OrganizationKey = new Guid(OrganizationKey);
                    Criteria.ReturnSizeInfoOnly = false;
                    Criteria.SurveyAnswerIdList = new string[0];
                    Criteria.IsDraftMode = IsDraftMode;
                    Request.Criteria = Criteria; 

                    for (int i = 1; i <= Pages; i++)
                        {
                        Request.Criteria.PageNumber = i; // batch index
                        Request.Criteria.PageSize = PageSize; // number of records per batch

                        Result = null;

                        try
                            {
                            Result = client.GetSurveyAnswer(Request);
                            }
                        catch (Exception ex)
                            {
                            this.BeginInvoke(new SetStatusDelegate(AddErrorStatusMessage), ex.Message);
                            e.Result = ex;
                            return;
                            }

                        this.BeginInvoke(new SetMaxProgressBarValueDelegate(SetProgressBarMaximum), i);

                        try
                            {
                            Query selectQuery = destinationProjectDataDriver.CreateQuery("SELECT [GlobalRecordId] FROM [" + destinationView.TableName + "]");
                            IDataReader destReader = destinationProjectDataDriver.ExecuteReader(selectQuery);
                            List<string> destinationGUIDList = new List<string>();
 
                            while (destReader.Read())
                                {
                                destinationGUIDList.Add(destReader[0].ToString());
                                }
                            foreach (View v in destinationView.GetDescendantViews())
                            {
                                selectQuery = destinationProjectDataDriver.CreateQuery("SELECT [GlobalRecordId] FROM [" + v.TableName + "]");
                                destReader = destinationProjectDataDriver.ExecuteReader(selectQuery);
                                while (destReader.Read())
                                {
                                    destinationGUIDList.Add(destReader[0].ToString());
                                }
                            }
                            _webFieldDataList = ParseXML(Result);
                            
                            ProcessBaseTable(_webFieldDataList, destinationView, destinationGUIDList);
                            ProcessPages(_webFieldDataList, destinationView, destinationGUIDList);
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
                DialogResult result = Epi.Windows.MsgBox.ShowQuestion("Aborting the import process and may cause impartially-updated or incomplete records to exist. Proceed with abort?");
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
            AddStatusMessage("Loaded data import dialog. Ready.");
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
        }
    }
