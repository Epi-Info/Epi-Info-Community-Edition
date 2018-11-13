using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Epi.Data;
using Epi.Web.Common;
using Epi.Web.Common.Message;

namespace Epi.ImportExport
{
    public class WebSurveyCSVExporter : ExporterBase
    {
        #region Private Members
        private string fileName;
        private Guid surveyKey;
        private Guid orgKey;
        private Guid secToken;
        private int rowsExported;
        private Configuration config;
        private SurveyManagerService.ManagerServiceClient client;
        private List<WebFieldData> wfList;
        private int Pages = 0;
        private int PageSize = 0;
        #endregion // Private Members

        #region Constants
        private const string SEPARATOR = StringLiterals.COMMA;
        #endregion // Constants

        #region Events
        public event SetMaxProgressBarValueDelegate SetMaxProgressBarValue;
        public event SimpleEventHandler FinishExport;
        public event UpdateStatusEventHandler ExportFailed;
        #endregion // Events

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="pSurveyKey">The web survey's survey key</param>
        /// <param name="pOrganizationKey">The organization key for web publishing</param>
        /// <param name="pSecurityToken">The security token associated with this survey</param>
        /// <param name="pFileName">The CSV file to be created. Use a full path and name.</param>
        public WebSurveyCSVExporter(Guid pSurveyKey, Guid pOrganizationKey, Guid pSecurityToken, string pFileName)
        {
            this.fileName = pFileName;
            this.secToken = pSecurityToken;
            this.orgKey = pOrganizationKey;
            this.surveyKey = pSurveyKey;
            this.rowsExported = 0;
            Construct();
        }
        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Gets the current file name and path that will contain the exported data.
        /// </summary>
        public string FileName
        {
            get
            {
                return this.fileName;
            }
        }
        #endregion // Public Properties

        #region Public Methods
        /// <summary>
        /// Initiates an export of the data to the specified file.
        /// </summary>
        public override void Export()
        {
            OnSetStatusMessage(ImportExportSharedStrings.WEB_CSV_EXPORT_CONNECTING);

            rowsExported = 0;
            WordBuilder wb = new WordBuilder(SEPARATOR);
            StreamWriter sw = null;

            SurveyManagerService.SurveyAnswerRequest Request = new SurveyManagerService.SurveyAnswerRequest();
            Request.Criteria.SurveyId = surveyKey.ToString();
            Request.Criteria.UserPublishKey = secToken;
            Request.Criteria.OrganizationKey = orgKey;
            Request.Criteria.ReturnSizeInfoOnly = true;
            SurveyManagerService.SurveyAnswerResponse Result = client.GetSurveyAnswer(Request);
            Pages = Result.NumberOfPages;
            PageSize = Result.PageSize;

            Request.Criteria.ReturnSizeInfoOnly = false;

            int count = 0;

            List<SurveyManagerService.SurveyAnswerResponse> Results = new List<SurveyManagerService.SurveyAnswerResponse>();

            OnSetStatusMessage(ImportExportSharedStrings.WEB_CSV_EXPORT_BUILDING_COLUMN_HEADINGS);

            for (int i = 1; i <= Pages; i++)
            {
                Request.Criteria.PageNumber = i;
                Request.Criteria.PageSize = PageSize;

                Result = client.GetSurveyAnswer(Request);
                Results.Add(Result);

                foreach (SurveyManagerService.SurveyAnswerDTO surveyAnswer in Result.SurveyResponseList)
                {
                    if (surveyAnswer.Status == 3)
                    {
                        count++;
                    }
                }
            }


            if (SetMaxProgressBarValue != null)
            {
                SetMaxProgressBarValue((double)count);
            }

            List<string> columnHeaders = new List<string>();

            foreach (SurveyManagerService.SurveyAnswerResponse R in Results)
            {
                wfList = ParseXML(R);

                foreach (WebFieldData wfData in wfList)
                {
                    if (!columnHeaders.Contains(wfData.FieldName))
                    {
                        columnHeaders.Add(wfData.FieldName);
                    }
                    else
                    {
                        break;
                    }
                }

                break;
            }

            try
            {
                OnSetStatusMessage(ImportExportSharedStrings.WEB_CSV_EXPORTING);

                sw = File.CreateText(fileName);

                foreach (string s in columnHeaders)
                {
                    wb.Add(s);
                }

                sw.WriteLine(wb.ToString());
                rowsExported = 0;


                foreach (SurveyManagerService.SurveyAnswerResponse R in Results)
                {
                    string currentGUID = string.Empty;
                    string lastGUID = string.Empty;

                    wfList = ParseXML(R);

                    wb = new WordBuilder(SEPARATOR);

                    foreach (WebFieldData wfData in wfList)
                    {
                        currentGUID = wfData.RecordGUID;

                        if (!string.IsNullOrEmpty(currentGUID) && !string.IsNullOrEmpty(lastGUID) && currentGUID != lastGUID)
                        {
                            sw.WriteLine(wb.ToString());
                            wb = new WordBuilder(SEPARATOR);

                            OnSetStatusMessageAndProgressCount("", 1);
                        }

                        string rowValue = wfData.FieldValue.ToString().Replace("\r\n", " ");
                        if (rowValue.Contains(",") || rowValue.Contains("\""))
                        {
                            rowValue = rowValue.Replace("\"", "\"\"");
                            rowValue = Util.InsertIn(rowValue, "\"");
                        }
                        wb.Add(rowValue);

                        lastGUID = wfData.RecordGUID;
                    }

                    sw.WriteLine(wb.ToString());
                }

                OnSetStatusMessage(ImportExportSharedStrings.WEB_CSV_EXPORT_COMPLETE);

                if (FinishExport != null)
                {
                    FinishExport();
                }
            }

            catch (Exception ex)
            {
                OnSetStatusMessage(ex.Message);

                if (ExportFailed != null)
                {
                    ExportFailed(ex.Message);
                }
            }
            finally
            {
                // Clean up
                if (sw != null)
                {
                    sw.Close();
                    sw.Dispose();
                    sw = null;
                }
            }
        }
        #endregion // Public Methods

        #region Private Methods
        /// <summary>
        /// Parses XML from the web survey
        /// </summary>
        /// <param name="result">The parsed results in dictionary format</param>
        //  private List<WebFieldData> ParseXML(SurveyAnswerResponse result)
        private List<WebFieldData> ParseXML(SurveyManagerService.SurveyAnswerResponse result)
        {
            List<WebFieldData> surveyResponses = new List<WebFieldData>();

            foreach (SurveyManagerService.SurveyAnswerDTO surveyAnswer in result.SurveyResponseList)
            {
                WebFieldData wfData = new WebFieldData();

                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.LoadXml(surveyAnswer.XML);

                foreach (XmlElement docElement in doc.ChildNodes)
                {
                    if (docElement.Name.ToLowerInvariant().Equals("surveyresponse"))
                    {
                        foreach (XmlElement surveyElement in docElement.ChildNodes)
                        {
                            if (surveyElement.Name.ToLowerInvariant().Equals("page") && surveyElement.Attributes.Count > 0 && surveyElement.Attributes[0].Name.ToLowerInvariant().Equals("pageid"))
                            {
                                foreach (XmlElement pageElement in surveyElement.ChildNodes)
                                {
                                    if (pageElement.Name.ToLowerInvariant().Equals("responsedetail"))
                                    {
                                        string fieldName = string.Empty;
                                        if (pageElement.Attributes.Count > 0)
                                        {
                                            fieldName = pageElement.Attributes[0].Value;
                                        }
                                        object fieldValue = pageElement.InnerText;

                                        wfData = new WebFieldData();
                                        wfData.RecordGUID = surveyAnswer.ResponseId;
                                        wfData.Page = Convert.ToInt32(surveyElement.Attributes[0].Value);
                                        wfData.FieldName = fieldName;
                                        wfData.FieldValue = fieldValue;
                                        wfData.Status = surveyAnswer.Status;
                                        if (wfData.Status == 3)
                                        {
                                            surveyResponses.Add(wfData);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return surveyResponses;
        }

        /// <summary>
        /// Construct method
        /// </summary>
        private void Construct()
        {
            try
            {
                this.config = Configuration.GetNewInstance();

                //this.importWorker = new BackgroundWorker();
                //this.importWorker.WorkerSupportsCancellation = true;

                //this.requestWorker = new BackgroundWorker();
                //this.requestWorker.WorkerSupportsCancellation = true;

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

                    System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(config.Settings.WebServiceEndpointAddress);

                    client = new SurveyManagerService.ManagerServiceClient(binding, endpoint);

                    client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
                    client.ChannelFactory.Credentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
                }
                else
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

                    System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(config.Settings.WebServiceEndpointAddress);

                    client = new SurveyManagerService.ManagerServiceClient(binding, endpoint);
                }
                this.wfList = new List<WebFieldData>();
            }
            catch (Exception ex)
            {
                //SetStatusMessage("Error: Web service information was not found.");
            }
        }
        #endregion // Private Methods
    }
}
