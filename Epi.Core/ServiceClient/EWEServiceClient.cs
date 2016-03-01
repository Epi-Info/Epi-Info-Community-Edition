using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.IO;
 

namespace Epi.Core.ServiceClient
{
    public class EWEServiceClient
    {

        private static Configuration ConfigFile;
        
        

        public enum IsValidOrganizationKeyEnum
        {
            Yes,
            No,
            EndPointNotFound,
            GeneralException
        }
        private static  bool LoadConfiguration()
        {
            string configFilePath = Configuration.DefaultConfigurationPath;

            bool configurationOk = true;
            try
            {
                string directoryName = Path.GetDirectoryName(configFilePath);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                if (!File.Exists(configFilePath))
                {
                    Configuration defaultConfig = Configuration.CreateDefaultConfiguration();
                    Configuration.Save(defaultConfig);
                    ConfigFile = defaultConfig;

                }
                else
                {
                    ConfigFile = Configuration.GetNewInstance();
                }
                 Configuration.Load(configFilePath);
            }
            catch (ConfigurationException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                configurationOk = ex.Message == "";
            }

            return configurationOk;
        }
        

	
        public static EWEManagerService.EWEManagerServiceClient GetClient(string pEndPointAddress, bool pIsAuthenticated, bool pIsWsHttpBinding = true)
        {
            EWEManagerService.EWEManagerServiceClient result = null;
            
            try
            {
                LoadConfiguration();
                Configuration config = ConfigFile;
                if (pIsAuthenticated) // Windows Authentication
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
                    binding.MaxBufferPoolSize = config.Settings.EWEServiceMaxBufferPoolSize;
                    binding.MaxReceivedMessageSize = config.Settings.EWEServiceMaxReceivedMessageSize;
                    binding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
                    binding.TextEncoding = System.Text.Encoding.UTF8;
                    binding.TransferMode = System.ServiceModel.TransferMode.Buffered;
                    binding.UseDefaultWebProxy = true;
                    binding.ReaderQuotas.MaxDepth = config.Settings.EWEServiceReaderMaxDepth;
                    binding.ReaderQuotas.MaxStringContentLength = config.Settings.EWEServiceReaderMaxStringContentLength;
                    binding.ReaderQuotas.MaxArrayLength = config.Settings.EWEServiceReaderMaxArrayLength;
                    binding.ReaderQuotas.MaxBytesPerRead = config.Settings.EWEServiceReaderMaxBytesPerRead;
                    binding.ReaderQuotas.MaxNameTableCharCount = config.Settings.EWEServiceReaderMaxNameTableCharCount;

                    binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly;
                    binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;
                    binding.Security.Transport.ProxyCredentialType = System.ServiceModel.HttpProxyCredentialType.None;
                    binding.Security.Transport.Realm = string.Empty;

                    binding.Security.Message.ClientCredentialType = System.ServiceModel.BasicHttpMessageCredentialType.UserName;

                    System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(pEndPointAddress);

                    result = new EWEManagerService.EWEManagerServiceClient(binding, endpoint);
                    result.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
                    result.ChannelFactory.Credentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
                }
                else
                {
                    if (pIsWsHttpBinding)
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
                        binding.MaxBufferPoolSize = config.Settings.EWEServiceMaxBufferPoolSize;
                        binding.MaxReceivedMessageSize = config.Settings.EWEServiceMaxReceivedMessageSize;
                        binding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
                        binding.TextEncoding = System.Text.Encoding.UTF8;
                        binding.UseDefaultWebProxy = true;
                        binding.AllowCookies = false;

                        binding.ReaderQuotas.MaxDepth = config.Settings.EWEServiceReaderMaxDepth;
                        binding.ReaderQuotas.MaxStringContentLength = config.Settings.EWEServiceReaderMaxStringContentLength;
                        binding.ReaderQuotas.MaxArrayLength = config.Settings.EWEServiceReaderMaxArrayLength;
                        binding.ReaderQuotas.MaxBytesPerRead = config.Settings.EWEServiceReaderMaxBytesPerRead;
                        binding.ReaderQuotas.MaxNameTableCharCount = config.Settings.EWEServiceReaderMaxNameTableCharCount;

                        binding.ReliableSession.Ordered = true;
                        binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 10, 0);
                        binding.ReliableSession.Enabled = false;

                        binding.Security.Mode = System.ServiceModel.SecurityMode.Message;
                        binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;
                        binding.Security.Transport.ProxyCredentialType = System.ServiceModel.HttpProxyCredentialType.None;
                        binding.Security.Transport.Realm = string.Empty;
                        binding.Security.Message.ClientCredentialType = System.ServiceModel.MessageCredentialType.Windows;
                        binding.Security.Message.NegotiateServiceCredential = true;

                        System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(pEndPointAddress);

                        result = new EWEManagerService.EWEManagerServiceClient(binding, endpoint);

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
                        binding.MaxBufferPoolSize = config.Settings.EWEServiceMaxBufferPoolSize;
                        binding.MaxReceivedMessageSize = config.Settings.EWEServiceMaxReceivedMessageSize;
                        binding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
                        binding.TextEncoding = System.Text.Encoding.UTF8;
                        binding.TransferMode = System.ServiceModel.TransferMode.Buffered;
                        binding.UseDefaultWebProxy = true;
                        binding.ReaderQuotas.MaxDepth = config.Settings.EWEServiceReaderMaxDepth;
                        binding.ReaderQuotas.MaxStringContentLength = config.Settings.EWEServiceReaderMaxStringContentLength;
                        binding.ReaderQuotas.MaxArrayLength = config.Settings.EWEServiceReaderMaxArrayLength;
                        binding.ReaderQuotas.MaxBytesPerRead = config.Settings.EWEServiceReaderMaxBytesPerRead;
                        binding.ReaderQuotas.MaxNameTableCharCount = config.Settings.EWEServiceReaderMaxNameTableCharCount;

                        binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly;
                        binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;
                        binding.Security.Transport.ProxyCredentialType = System.ServiceModel.HttpProxyCredentialType.None;
                        binding.Security.Transport.Realm = string.Empty;

                        System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(pEndPointAddress);

                        result = new EWEManagerService.EWEManagerServiceClient(binding, endpoint);
                    }
                }
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (SecurityNegotiationException sne)
            {
                throw sne;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public static EWEManagerService.EWEManagerServiceClient GetClient()
        {
            LoadConfiguration();
            Configuration config = ConfigFile;
            string pEndPointAddress = config.Settings.EWEServiceEndpointAddress;
            bool pIsAuthenticated = false;
            bool pIsWsHTTPBinding = true;

            if (config.Settings.EWEServiceAuthMode == 1)
            {
                pIsAuthenticated = true;
            }

            string s = config.Settings.EWEServiceBindingMode;// ConfigurationManager.AppSettings["WCF_BINDING_TYPE"];
            if (!String.IsNullOrEmpty(s))
            {
                if (s.ToUpper() == "WSHTTP")
                {
                    pIsWsHTTPBinding = true;
                }
                else
                {
                    pIsWsHTTPBinding = false;
                }
            }

            return GetClient(pEndPointAddress, pIsAuthenticated, pIsWsHTTPBinding);
        }


        static public IsValidOrganizationKeyEnum IsValidOrgKey(string pOrganizationKey, string pSurveyId = null)
        {
            try
            {
                LoadConfiguration();
                Configuration config = ConfigFile;
                EWEManagerService.EWEManagerServiceClient client = EWEServiceClient.GetClient();
               
                var Request = new EWEManagerService.SurveyInfoRequest(); 

                EWEManagerService.SurveyInfoCriteria Criteria = new EWEManagerService.SurveyInfoCriteria();
 
                if (!string.IsNullOrEmpty(pOrganizationKey))
                {
                    Criteria.OrganizationKey = new Guid(pOrganizationKey);
                    Request.Criteria = Criteria;
                }
                
                if (!string.IsNullOrWhiteSpace(pSurveyId))
                {
                    Criteria.SurveyIdList = new string[1];
                    Criteria.SurveyIdList[0] = pSurveyId.ToString();
                    Request.Criteria = Criteria;
                }

                if (client.IsValidOrgKey(Request))
                {
                    return IsValidOrganizationKeyEnum.Yes;
                }
                else
                {
                    return IsValidOrganizationKeyEnum.No;
                }
            }
            catch (System.ServiceModel.EndpointNotFoundException)
            {
                return IsValidOrganizationKeyEnum.EndPointNotFound;
            }
            catch 
            {
                return IsValidOrganizationKeyEnum.GeneralException;
            }
        }
    }
}
