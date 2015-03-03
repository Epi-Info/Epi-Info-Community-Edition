using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Security;

namespace Epi.Core.ServiceClient
{
    public class ServiceClient
    {
        public enum IsValidOrganizationKeyEnum
        {
            Yes,
            No,
            EndPointNotFound,
            GeneralException
        }

        public static SurveyManagerService.ManagerServiceV3Client GetClient(string pEndPointAddress, bool pIsAuthenticated, bool pIsWsHttpBinding = true)
        {
            SurveyManagerService.ManagerServiceV3Client result = null;
            try
            {

                Configuration config = Configuration.GetNewInstance();
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

                    System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(pEndPointAddress);

                    result = new SurveyManagerService.ManagerServiceV3Client(binding, endpoint);
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

                        System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(pEndPointAddress);

                        result = new SurveyManagerService.ManagerServiceV3Client(binding, endpoint);

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

                        System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(pEndPointAddress);

                        result = new SurveyManagerService.ManagerServiceV3Client(binding, endpoint);
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

        public static SurveyManagerService.ManagerServiceV3Client GetClient()
        {
            Configuration config = Configuration.GetNewInstance();

            string pEndPointAddress = config.Settings.WebServiceEndpointAddress;
            bool pIsAuthenticated = false;
            bool pIsWsHTTPBinding = true;
            //string s = config.Settings.WebServiceAuthMode;// "Authentication_Use_Windows"];
            if (config.Settings.WebServiceAuthMode == 1)
            {
                    pIsAuthenticated = true;
            }

            string s = config.Settings.WebServiceBindingMode;// ConfigurationManager.AppSettings["WCF_BINDING_TYPE"];
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
                SurveyManagerService.ManagerServiceV3Client client = Epi.Core.ServiceClient.ServiceClient.GetClient();
                Configuration config = Configuration.GetNewInstance();

                SurveyManagerService.SurveyInfoRequest request = new SurveyManagerService.SurveyInfoRequest();
                
                //tbOrgKey
                if (!string.IsNullOrEmpty(pOrganizationKey))
                {
                    request.Criteria.OrganizationKey = new Guid(pOrganizationKey);
                }

                if (!string.IsNullOrWhiteSpace(pSurveyId))
                {
                    request.Criteria.SurveyIdList = new string[]{pSurveyId};
                }

                if (client.IsValidOrgKey(request))
                {
                    return IsValidOrganizationKeyEnum.Yes;
                }
                else
                {
                    return IsValidOrganizationKeyEnum.No;
                }
                
            }
            catch (System.ServiceModel.EndpointNotFoundException epnf)
            {
                return IsValidOrganizationKeyEnum.EndPointNotFound;
            }
            catch (Exception ex)
            {
                return IsValidOrganizationKeyEnum.GeneralException;
            }
        }
    }
}
