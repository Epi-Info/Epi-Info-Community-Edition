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

        //public static SurveyManagerService.ManagerServiceV2Client GetClient(string pEndPointAddress, bool pIsAuthenticated, bool pIsWsHttpBinding = true)
        //{
        //    SurveyManagerService.ManagerServiceV2Client result = null;
        //    try
        //    {

        //        Configuration config = Configuration.GetNewInstance();
        //        if (pIsAuthenticated) // Windows Authentication
        //        {
        //            System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();
        //            binding.Name = "BasicHttpBinding";
        //            binding.CloseTimeout = new TimeSpan(0, 1, 0);
        //            binding.OpenTimeout = new TimeSpan(0, 1, 0);
        //            binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
        //            binding.SendTimeout = new TimeSpan(0, 1, 0);
        //            binding.AllowCookies = false;
        //            binding.BypassProxyOnLocal = false;
        //            binding.HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
        //            binding.MaxBufferPoolSize = config.Settings.WebServiceMaxBufferPoolSize;
        //            binding.MaxReceivedMessageSize = config.Settings.WebServiceMaxReceivedMessageSize;
        //            binding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
        //            binding.TextEncoding = System.Text.Encoding.UTF8;
        //            binding.TransferMode = System.ServiceModel.TransferMode.Buffered;
        //            binding.UseDefaultWebProxy = true;
        //            binding.ReaderQuotas.MaxDepth = config.Settings.WebServiceReaderMaxDepth;
        //            binding.ReaderQuotas.MaxStringContentLength = config.Settings.WebServiceReaderMaxStringContentLength;
        //            binding.ReaderQuotas.MaxArrayLength = config.Settings.WebServiceReaderMaxArrayLength;
        //            binding.ReaderQuotas.MaxBytesPerRead = config.Settings.WebServiceReaderMaxBytesPerRead;
        //            binding.ReaderQuotas.MaxNameTableCharCount = config.Settings.WebServiceReaderMaxNameTableCharCount;

        //            binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly;
        //            binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;
        //            binding.Security.Transport.ProxyCredentialType = System.ServiceModel.HttpProxyCredentialType.None;
        //            binding.Security.Transport.Realm = string.Empty;

        //            binding.Security.Message.ClientCredentialType = System.ServiceModel.BasicHttpMessageCredentialType.UserName;

        //            System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(pEndPointAddress);

        //            result = new SurveyManagerService.ManagerServiceV2Client(binding, endpoint);
        //            result.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
        //            result.ChannelFactory.Credentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
        //        }
        //        else
        //        {
        //            if (pIsWsHttpBinding)
        //            {
        //                System.ServiceModel.WSHttpBinding binding = new System.ServiceModel.WSHttpBinding();
        //                binding.Name = "WSHttpBinding";
        //                binding.CloseTimeout = new TimeSpan(0, 1, 0);
        //                binding.OpenTimeout = new TimeSpan(0, 1, 0);
        //                binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
        //                binding.SendTimeout = new TimeSpan(0, 1, 0);
        //                binding.BypassProxyOnLocal = false;
        //                binding.TransactionFlow = false;
        //                binding.HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
        //                binding.MaxBufferPoolSize = config.Settings.WebServiceMaxBufferPoolSize;
        //                binding.MaxReceivedMessageSize = config.Settings.WebServiceMaxReceivedMessageSize;
        //                binding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
        //                binding.TextEncoding = System.Text.Encoding.UTF8;
        //                binding.UseDefaultWebProxy = true;
        //                binding.AllowCookies = false;

        //                binding.ReaderQuotas.MaxDepth = config.Settings.WebServiceReaderMaxDepth;
        //                binding.ReaderQuotas.MaxStringContentLength = config.Settings.WebServiceReaderMaxStringContentLength;
        //                binding.ReaderQuotas.MaxArrayLength = config.Settings.WebServiceReaderMaxArrayLength;
        //                binding.ReaderQuotas.MaxBytesPerRead = config.Settings.WebServiceReaderMaxBytesPerRead;
        //                binding.ReaderQuotas.MaxNameTableCharCount = config.Settings.WebServiceReaderMaxNameTableCharCount;

        //                binding.ReliableSession.Ordered = true;
        //                binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 10, 0);
        //                binding.ReliableSession.Enabled = false;

        //                binding.Security.Mode = System.ServiceModel.SecurityMode.Message;
        //                binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;
        //                binding.Security.Transport.ProxyCredentialType = System.ServiceModel.HttpProxyCredentialType.None;
        //                binding.Security.Transport.Realm = string.Empty;
        //                binding.Security.Message.ClientCredentialType = System.ServiceModel.MessageCredentialType.Windows;
        //                binding.Security.Message.NegotiateServiceCredential = true;

        //                System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(pEndPointAddress);

        //                result = new SurveyManagerService.ManagerServiceV2Client(binding, endpoint);

        //            }
        //            else
        //            {
        //                System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();
        //                binding.Name = "BasicHttpBinding";
        //                binding.CloseTimeout = new TimeSpan(0, 1, 0);
        //                binding.OpenTimeout = new TimeSpan(0, 1, 0);
        //                binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
        //                binding.SendTimeout = new TimeSpan(0, 1, 0);
        //                binding.AllowCookies = false;
        //                binding.BypassProxyOnLocal = false;
        //                binding.HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
        //                binding.MaxBufferPoolSize = config.Settings.WebServiceMaxBufferPoolSize;
        //                binding.MaxReceivedMessageSize = config.Settings.WebServiceMaxReceivedMessageSize;
        //                binding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
        //                binding.TextEncoding = System.Text.Encoding.UTF8;
        //                binding.TransferMode = System.ServiceModel.TransferMode.Buffered;
        //                binding.UseDefaultWebProxy = true;
        //                binding.ReaderQuotas.MaxDepth = config.Settings.WebServiceReaderMaxDepth;
        //                binding.ReaderQuotas.MaxStringContentLength = config.Settings.WebServiceReaderMaxStringContentLength;
        //                binding.ReaderQuotas.MaxArrayLength = config.Settings.WebServiceReaderMaxArrayLength;
        //                binding.ReaderQuotas.MaxBytesPerRead = config.Settings.WebServiceReaderMaxBytesPerRead;
        //                binding.ReaderQuotas.MaxNameTableCharCount = config.Settings.WebServiceReaderMaxNameTableCharCount;

        //                binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly;
        //                binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;
        //                binding.Security.Transport.ProxyCredentialType = System.ServiceModel.HttpProxyCredentialType.None;
        //                binding.Security.Transport.Realm = string.Empty;

        //                System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(pEndPointAddress);

        //                result = new SurveyManagerService.ManagerServiceV2Client(binding, endpoint);
        //            }
        //        }
        //    }
        //    catch (FaultException fe)
        //    {
        //        throw fe;
        //    }
        //    catch (SecurityNegotiationException sne)
        //    {
        //        throw sne;
        //    }
        //    catch (CommunicationException ce)
        //    {
        //        throw ce;
        //    }
        //    catch (TimeoutException te)
        //    {
        //        throw te;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    return result;
        //}

        //public static SurveyManagerService.ManagerServiceV2Client GetClient()
        //{
        //    Configuration config = Configuration.GetNewInstance();

        //    string pEndPointAddress = config.Settings.WebServiceEndpointAddress;
        //    bool pIsAuthenticated = false;
        //    bool pIsWsHTTPBinding = true;
        //    //string s = config.Settings.WebServiceAuthMode;// "Authentication_Use_Windows"];
        //    if (config.Settings.WebServiceAuthMode == 1)
        //    {
        //            pIsAuthenticated = true;
        //    }

        //    string s = config.Settings.WebServiceBindingMode;// ConfigurationManager.AppSettings["WCF_BINDING_TYPE"];
        //    if (!String.IsNullOrEmpty(s))
        //    {
        //        if (s.ToUpperInvariant() == "WSHTTP")
        //        {
        //            pIsWsHTTPBinding = true;
        //        }
        //        else
        //        {
        //            pIsWsHTTPBinding = false;
        //        }
        //    }

        //    return GetClient(pEndPointAddress, pIsAuthenticated, pIsWsHTTPBinding);
        //}

        public static SurveyManagerService.ManagerServiceClient GetClient(string pEndPointAddress, bool pIsAuthenticated, bool pIsWsHttpBinding = true)
        {
            SurveyManagerService.ManagerServiceClient result = null;
            Configuration config = Configuration.GetNewInstance();
            try
            {
                if (pIsAuthenticated) // Windows Authentication
                {
                    System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();
                    binding.Name = "BasicHttpBinding";
                    binding.CloseTimeout = new TimeSpan(0, 10, 0);
                    binding.OpenTimeout = new TimeSpan(0, 10, 0);
                    binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
                    binding.SendTimeout = new TimeSpan(0, 10, 0);
                    binding.AllowCookies = false;
                    binding.BypassProxyOnLocal = false;
                    binding.HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
                    binding.MaxBufferPoolSize = config.Settings.WebServiceMaxBufferPoolSize;//524288;
                    binding.MaxReceivedMessageSize = config.Settings.WebServiceMaxReceivedMessageSize;
                    binding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
                    binding.TextEncoding = System.Text.Encoding.UTF8;
                    binding.TransferMode = System.ServiceModel.TransferMode.Buffered;
                    binding.UseDefaultWebProxy = true;
                    binding.ReaderQuotas.MaxDepth = config.Settings.WebServiceReaderMaxDepth;//32;
                   
                    binding.ReaderQuotas.MaxStringContentLength = config.Settings.WebServiceReaderMaxStringContentLength;
                    binding.ReaderQuotas.MaxArrayLength = config.Settings.WebServiceReaderMaxArrayLength;
                    binding.ReaderQuotas.MaxBytesPerRead = config.Settings.WebServiceReaderMaxBytesPerRead;
                    binding.ReaderQuotas.MaxNameTableCharCount = config.Settings.WebServiceReaderMaxNameTableCharCount;

                    System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(pEndPointAddress);
                    if (endpoint.Uri.Scheme == "http")
                    {
                        binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly;
                        binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;
                        binding.Security.Transport.ProxyCredentialType = System.ServiceModel.HttpProxyCredentialType.None;
                        binding.Security.Transport.Realm = string.Empty;
                    }
                    else
                    {
                        binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;
                        // binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.None;
                        binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;


                    }
                    binding.Security.Message.ClientCredentialType = System.ServiceModel.BasicHttpMessageCredentialType.UserName;

                    
                    result = new SurveyManagerService.ManagerServiceClient(binding, endpoint);
                    result.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
                    result.ChannelFactory.Credentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
                    System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                        (se, cert, chain, sslerror) =>
                        {
                            return true;
                        };

                }
                else
                {
                    if (pIsWsHttpBinding)
                    {
                        System.ServiceModel.WSHttpBinding binding = new System.ServiceModel.WSHttpBinding();
                        binding.Name = "WSHttpBinding";
                        binding.CloseTimeout = new TimeSpan(0, 10, 0);
                        binding.OpenTimeout = new TimeSpan(0, 10, 0);
                        binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
                        binding.SendTimeout = new TimeSpan(0, 10, 0);
                        binding.BypassProxyOnLocal = false;
                        binding.TransactionFlow = false;
                        binding.HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
                        binding.MaxBufferPoolSize = config.Settings.WebServiceMaxBufferPoolSize;//524288;
                        binding.MaxReceivedMessageSize = config.Settings.WebServiceMaxReceivedMessageSize;
                        binding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
                        binding.TextEncoding = System.Text.Encoding.UTF8;
                        binding.UseDefaultWebProxy = true;
                        binding.AllowCookies = false;

                        binding.ReaderQuotas.MaxDepth = config.Settings.WebServiceReaderMaxDepth;//32;

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

                        result = new SurveyManagerService.ManagerServiceClient(binding, endpoint);

                    }
                    else
                    {
                        System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();
                        binding.Name = "BasicHttpBinding";
                        binding.CloseTimeout = new TimeSpan(0, 10, 0);
                        binding.OpenTimeout = new TimeSpan(0, 10, 0);
                        binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
                        binding.SendTimeout = new TimeSpan(0, 10, 0);
                        binding.AllowCookies = false;
                        binding.BypassProxyOnLocal = false;
                        binding.HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
                        binding.MaxBufferPoolSize = config.Settings.WebServiceMaxBufferPoolSize;//524288;
                        binding.MaxReceivedMessageSize = config.Settings.WebServiceMaxReceivedMessageSize;
                        binding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
                        binding.TextEncoding = System.Text.Encoding.UTF8;
                        binding.TransferMode = System.ServiceModel.TransferMode.Buffered;
                        binding.UseDefaultWebProxy = true;
                        binding.ReaderQuotas.MaxDepth = config.Settings.WebServiceReaderMaxDepth;//32;

                        binding.ReaderQuotas.MaxStringContentLength = config.Settings.WebServiceReaderMaxStringContentLength;
                        binding.ReaderQuotas.MaxArrayLength = config.Settings.WebServiceReaderMaxArrayLength;
                        binding.ReaderQuotas.MaxBytesPerRead = config.Settings.WebServiceReaderMaxBytesPerRead;
                        binding.ReaderQuotas.MaxNameTableCharCount = config.Settings.WebServiceReaderMaxNameTableCharCount;

                        System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(pEndPointAddress);
                        if (endpoint.Uri.Scheme == "http")
                        {
                            binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly;
                            binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;
                            binding.Security.Transport.ProxyCredentialType = System.ServiceModel.HttpProxyCredentialType.None;
                            binding.Security.Transport.Realm = string.Empty;
                        }
                        else
                        {
                            binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;
                            // binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.None;
                            binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;


                        }
                       

                        result = new SurveyManagerService.ManagerServiceClient(binding, endpoint);
                        System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                        (se, cert, chain, sslerror) =>
                        {
                            return true;
                        };

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

        public static SurveyManagerService.ManagerServiceClient GetClient()
        {
            Configuration config = Configuration.GetNewInstance();

            string pEndPointAddress = config.Settings.WebServiceEndpointAddress;
            bool pIsAuthenticated = false;
            bool pIsWsHTTPBinding = true;
            //string s = config.Settings.WebServiceAuthMode;// "Authentication_Use_Windows"];r
            if (config.Settings.WebServiceAuthMode == 1)
            {
                pIsAuthenticated = true;
            }

            string s = config.Settings.WebServiceBindingMode;// ConfigurationManager.AppSettings["WCF_BINDING_TYPE"];
            if (!String.IsNullOrEmpty(s))
            {
                if (s.ToUpperInvariant() == "WSHTTP")
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

        public static SurveyManagerServiceV2.ManagerServiceV2Client GetClientV2()
        {
            Configuration config = Configuration.GetNewInstance();

            string pEndPointAddress = config.Settings.WebServiceEndpointAddress;
            bool pIsAuthenticated = false;
            bool pIsWsHTTPBinding = true;
            //string s = config.Settings.WebServiceAuthMode;// "Authentication_Use_Windows"];r
            if (config.Settings.WebServiceAuthMode == 1)
            {
                pIsAuthenticated = true;
            }

            string s = config.Settings.WebServiceBindingMode;// ConfigurationManager.AppSettings["WCF_BINDING_TYPE"];
            if (!String.IsNullOrEmpty(s))
            {
                if (s.ToUpperInvariant() == "WSHTTP")
                {
                    pIsWsHTTPBinding = true;
                }
                else
                {
                    pIsWsHTTPBinding = false;
                }
            }

            return GetClientV2(pEndPointAddress, pIsAuthenticated, pIsWsHTTPBinding);
        }
        public static SurveyManagerServiceV2.ManagerServiceV2Client GetClientV2(string pEndPointAddress, bool pIsAuthenticated, bool pIsWsHttpBinding = true)
        {
            Configuration config = Configuration.GetNewInstance();
            SurveyManagerServiceV2.ManagerServiceV2Client result = null;
            try
            {
                if (pIsAuthenticated) // Windows Authentication
                {
                    System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();
                    binding.Name = "BasicHttpBinding";
                    binding.CloseTimeout = new TimeSpan(0, 10, 0);
                    binding.OpenTimeout = new TimeSpan(0, 10, 0);
                    binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
                    binding.SendTimeout = new TimeSpan(0, 10, 0);
                    binding.AllowCookies = false;
                    binding.BypassProxyOnLocal = false;
                    binding.HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
                    binding.MaxBufferPoolSize = config.Settings.WebServiceMaxBufferPoolSize;//524288;
                    binding.MaxReceivedMessageSize = config.Settings.WebServiceMaxReceivedMessageSize;
                    binding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
                    binding.TextEncoding = System.Text.Encoding.UTF8;
                    binding.TransferMode = System.ServiceModel.TransferMode.Buffered;
                    binding.UseDefaultWebProxy = true;
                    binding.ReaderQuotas.MaxDepth = config.Settings.WebServiceReaderMaxDepth;//32;

                    binding.ReaderQuotas.MaxStringContentLength = config.Settings.WebServiceReaderMaxStringContentLength;
                    binding.ReaderQuotas.MaxArrayLength = config.Settings.WebServiceReaderMaxArrayLength;
                    binding.ReaderQuotas.MaxBytesPerRead = config.Settings.WebServiceReaderMaxBytesPerRead;
                    binding.ReaderQuotas.MaxNameTableCharCount = config.Settings.WebServiceReaderMaxNameTableCharCount;

                    System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(pEndPointAddress);
                    if (endpoint.Uri.Scheme == "http")
                    {
                        binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly;
                        binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;
                        binding.Security.Transport.ProxyCredentialType = System.ServiceModel.HttpProxyCredentialType.None;
                        binding.Security.Transport.Realm = string.Empty;
                    }
                    else
                    {
                        binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;
                        // binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.None;
                        binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;


                    }

                    binding.Security.Message.ClientCredentialType = System.ServiceModel.BasicHttpMessageCredentialType.UserName;

                   

                    result = new SurveyManagerServiceV2.ManagerServiceV2Client(binding, endpoint);
                    result.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
                    result.ChannelFactory.Credentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
                    System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                        (se, cert, chain, sslerror) =>
                        {
                            return true;
                        };

                }
                else
                {
                    if (pIsWsHttpBinding)
                    {
                        System.ServiceModel.WSHttpBinding binding = new System.ServiceModel.WSHttpBinding();
                        binding.Name = "WSHttpBinding";
                        binding.CloseTimeout = new TimeSpan(0, 10, 0);
                        binding.OpenTimeout = new TimeSpan(0, 10, 0);
                        binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
                        binding.SendTimeout = new TimeSpan(0, 10, 0);
                        binding.BypassProxyOnLocal = false;
                        binding.TransactionFlow = false;
                        binding.HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
                        binding.MaxBufferPoolSize = config.Settings.WebServiceMaxBufferPoolSize;//524288;
                        binding.MaxReceivedMessageSize = config.Settings.WebServiceMaxReceivedMessageSize;
                        binding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
                        binding.TextEncoding = System.Text.Encoding.UTF8;
                        binding.UseDefaultWebProxy = true;
                        binding.AllowCookies = false;

                        binding.ReaderQuotas.MaxDepth = config.Settings.WebServiceReaderMaxDepth;//32;

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

                        result = new SurveyManagerServiceV2.ManagerServiceV2Client(binding, endpoint);

                    }
                    else
                    {
                        System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();
                        binding.Name = "BasicHttpBinding";
                        binding.CloseTimeout = new TimeSpan(0, 10, 0);
                        binding.OpenTimeout = new TimeSpan(0, 10, 0);
                        binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
                        binding.SendTimeout = new TimeSpan(0, 10, 0);
                        binding.AllowCookies = false;
                        binding.BypassProxyOnLocal = false;
                        binding.HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
                        binding.MaxBufferPoolSize = config.Settings.WebServiceMaxBufferPoolSize;//524288;
                        binding.MaxReceivedMessageSize = config.Settings.WebServiceMaxReceivedMessageSize;
                        binding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
                        binding.TextEncoding = System.Text.Encoding.UTF8;
                        binding.TransferMode = System.ServiceModel.TransferMode.Buffered;
                        binding.UseDefaultWebProxy = true;
                        binding.ReaderQuotas.MaxDepth = config.Settings.WebServiceReaderMaxDepth;//32;

                        binding.ReaderQuotas.MaxStringContentLength = config.Settings.WebServiceReaderMaxStringContentLength;
                        binding.ReaderQuotas.MaxArrayLength = config.Settings.WebServiceReaderMaxArrayLength;
                        binding.ReaderQuotas.MaxBytesPerRead = config.Settings.WebServiceReaderMaxBytesPerRead;
                        binding.ReaderQuotas.MaxNameTableCharCount = config.Settings.WebServiceReaderMaxNameTableCharCount;

                        System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(pEndPointAddress);
                        if (endpoint.Uri.Scheme == "http")
                        {
                            binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly;
                            binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;
                            binding.Security.Transport.ProxyCredentialType = System.ServiceModel.HttpProxyCredentialType.None;
                            binding.Security.Transport.Realm = string.Empty;
                        }
                        else
                        {
                            binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;
                            // binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.None;
                            binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;


                        }

                       

                        result = new SurveyManagerServiceV2.ManagerServiceV2Client(binding, endpoint);
                        System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                        (se, cert, chain, sslerror) =>
                        {
                            return true;
                        };

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

        static public IsValidOrganizationKeyEnum IsValidOrgKey(string pOrganizationKey, string pSurveyId = null)
        {
            try
            {
                SurveyManagerServiceV3.ManagerServiceV3Client client = Epi.Core.ServiceClient.ServiceClient.GetClientV3();
                Configuration config = Configuration.GetNewInstance();

                Epi.SurveyManagerServiceV3.SurveyInfoRequest request = new Epi.SurveyManagerServiceV3.SurveyInfoRequest();
                request.Criteria = new SurveyManagerServiceV3.SurveyInfoCriteria();
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

        public static SurveyManagerServiceV3.ManagerServiceV3Client GetClientV3(string pEndPointAddress, bool pIsAuthenticated, bool pIsWsHttpBinding = true)
        {
            SurveyManagerServiceV3.ManagerServiceV3Client result = null;
            try
            {

                Configuration config = Configuration.GetNewInstance();
                if (pIsAuthenticated) // Windows Authentication
                {
                    System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();
                    binding.Name = "BasicHttpBinding";
                    binding.CloseTimeout = new TimeSpan(0, 10, 0);
                    binding.OpenTimeout = new TimeSpan(0, 10, 0);
                    binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
                    binding.SendTimeout = new TimeSpan(0, 10, 0);
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

                    System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(pEndPointAddress);
                    if (endpoint.Uri.Scheme == "http")
                    {
                        binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly;
                        binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;
                        binding.Security.Transport.ProxyCredentialType = System.ServiceModel.HttpProxyCredentialType.None;
                        binding.Security.Transport.Realm = string.Empty;
                    }
                    else
                    {
                        binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;
                        // binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.None;
                        binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;


                    }

                    binding.Security.Message.ClientCredentialType = System.ServiceModel.BasicHttpMessageCredentialType.UserName;

 

                    result = new SurveyManagerServiceV3.ManagerServiceV3Client(binding, endpoint);

                    result.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
                    result.ChannelFactory.Credentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
                    System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                        (se, cert, chain, sslerror) =>
                        {
                            return true;
                        };

                }
                else
                {
                    if (pIsWsHttpBinding)
                    {
                        System.ServiceModel.WSHttpBinding binding = new System.ServiceModel.WSHttpBinding();
                        binding.Name = "WSHttpBinding";
                        binding.CloseTimeout = new TimeSpan(0, 10, 0);
                        binding.OpenTimeout = new TimeSpan(0, 10, 0);
                        binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
                        binding.SendTimeout = new TimeSpan(0, 10, 0);
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

                        result = new SurveyManagerServiceV3.ManagerServiceV3Client(binding, endpoint);

                    }
                    else
                    {
                        System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();
                        binding.Name = "BasicHttpBinding";
                        binding.CloseTimeout = new TimeSpan(0, 10, 0);
                        binding.OpenTimeout = new TimeSpan(0, 10, 0);
                        binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
                        binding.SendTimeout = new TimeSpan(0, 10, 0);
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

                        System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(pEndPointAddress);
                        if (endpoint.Uri.Scheme == "http")
                        {
                            binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly;
                            binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;
                            binding.Security.Transport.ProxyCredentialType = System.ServiceModel.HttpProxyCredentialType.None;
                            binding.Security.Transport.Realm = string.Empty;
                        }
                        else
                        {
                             binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;
                            // binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.None;
                             binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;
                         
                        }

                     

                        result = new SurveyManagerServiceV3.ManagerServiceV3Client(binding, endpoint);
                        System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                        (se, cert, chain, sslerror) =>
                        {
                            return true;
                        };

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

        public static SurveyManagerServiceV3.ManagerServiceV3Client GetClientV3()
        {
            Configuration config = Configuration.GetNewInstance();

            string configServiceEndpoint = config.Settings.WebServiceEndpointAddress;
            //int length = configServiceEndpoint.IndexOf(Environment.NewLine);
            //string pEndPointAddress = configServiceEndpoint.Substring(0, length);
            string pEndPointAddress = configServiceEndpoint;
            bool pIsAuthenticated = false;
            bool pIsWsHTTPBinding = true;
            //string s = config.Settings.WebServiceAuthMode;// "Authentication_Use_Windows"];r
            if (config.Settings.WebServiceAuthMode == 1)
            {
                pIsAuthenticated = true;
            }

            string s = config.Settings.WebServiceBindingMode;// ConfigurationManager.AppSettings["WCF_BINDING_TYPE"];
            if (!String.IsNullOrEmpty(s))
            {
                if (s.ToUpperInvariant() == "WSHTTP")
                {
                    pIsWsHTTPBinding = true;
                }
                else
                {
                    pIsWsHTTPBinding = false;
                }
            }

            return GetClientV3(pEndPointAddress, pIsAuthenticated, pIsWsHTTPBinding);
        }


        public static SurveyManagerServiceV4.ManagerServiceV4Client GetClientV4(string pEndPointAddress, bool pIsAuthenticated, bool pIsWsHttpBinding = true)
        {
            SurveyManagerServiceV4.ManagerServiceV4Client result = null;
            try
            {

                Configuration config = Configuration.GetNewInstance();
                if (pIsAuthenticated) // Windows Authentication
                {
                    System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();
                    binding.Name = "BasicHttpBinding";
                    binding.CloseTimeout = new TimeSpan(0, 10, 0);
                    binding.OpenTimeout = new TimeSpan(0, 10, 0);
                    binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
                    binding.SendTimeout = new TimeSpan(0, 10, 0);
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

                    System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(pEndPointAddress);
                    if (endpoint.Uri.Scheme == "http")
                    {
                        binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly;
                        binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;
                        binding.Security.Transport.ProxyCredentialType = System.ServiceModel.HttpProxyCredentialType.None;
                        binding.Security.Transport.Realm = string.Empty;
                    }
                    else
                    {
                        binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;
                        // binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.None;
                        binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;


                    }

                    binding.Security.Message.ClientCredentialType = System.ServiceModel.BasicHttpMessageCredentialType.UserName;



                    result = new SurveyManagerServiceV4.ManagerServiceV4Client(binding, endpoint);

                    result.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
                    result.ChannelFactory.Credentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
                    System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                        (se, cert, chain, sslerror) =>
                        {
                            return true;
                        };

                }
                else
                {
                    if (pIsWsHttpBinding)
                    {
                        System.ServiceModel.WSHttpBinding binding = new System.ServiceModel.WSHttpBinding();
                        binding.Name = "WSHttpBinding";
                        binding.CloseTimeout = new TimeSpan(0, 10, 0);
                        binding.OpenTimeout = new TimeSpan(0, 10, 0);
                        binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
                        binding.SendTimeout = new TimeSpan(0, 10, 0);
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
                        binding.ReaderQuotas.MaxStringContentLength = 999999999; //config.Settings.WebServiceReaderMaxStringContentLength;
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

                        result = new SurveyManagerServiceV4.ManagerServiceV4Client(binding, endpoint);

                    }
                    else
                    {
                        System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();
                        binding.Name = "BasicHttpBinding";
                        binding.CloseTimeout = new TimeSpan(0, 10, 0);
                        binding.OpenTimeout = new TimeSpan(0, 10, 0);
                        binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
                        binding.SendTimeout = new TimeSpan(0, 10, 0);
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
                        binding.ReaderQuotas.MaxStringContentLength =999999999;// config.Settings.WebServiceReaderMaxStringContentLength;
                        binding.ReaderQuotas.MaxArrayLength = config.Settings.WebServiceReaderMaxArrayLength;
                        binding.ReaderQuotas.MaxBytesPerRead = config.Settings.WebServiceReaderMaxBytesPerRead;
                        binding.ReaderQuotas.MaxNameTableCharCount = config.Settings.WebServiceReaderMaxNameTableCharCount;

                        System.ServiceModel.EndpointAddress endpoint = new System.ServiceModel.EndpointAddress(pEndPointAddress);
                        if (endpoint.Uri.Scheme == "http")
                        {
                            binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly;
                            binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;
                            binding.Security.Transport.ProxyCredentialType = System.ServiceModel.HttpProxyCredentialType.None;
                            binding.Security.Transport.Realm = string.Empty;
                        }
                        else
                        {
                            binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;
                            // binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.None;
                            binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;


                        }



                        result = new SurveyManagerServiceV4.ManagerServiceV4Client(binding, endpoint);
                        System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                        (se, cert, chain, sslerror) =>
                        {
                            return true;
                        };

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

        public static SurveyManagerServiceV4.ManagerServiceV4Client GetClientV4()
        {
            Configuration config = Configuration.GetNewInstance();

            string configServiceEndpoint = config.Settings.WebServiceEndpointAddress;
            if(string.IsNullOrEmpty(configServiceEndpoint) == true)
            {
                return null;
            }
            
            bool pIsAuthenticated = false;
            bool pIsWsHTTPBinding = true;

            if (config.Settings.WebServiceAuthMode == 1)
            {
                pIsAuthenticated = true;
            }

            string s = config.Settings.WebServiceBindingMode;
            if (!String.IsNullOrEmpty(s))
            {
                if (s.ToUpperInvariant() == "WSHTTP")
                {
                    pIsWsHTTPBinding = true;
                }
                else
                {
                    pIsWsHTTPBinding = false;
                }
            }

            return GetClientV4(configServiceEndpoint, pIsAuthenticated, pIsWsHTTPBinding);
        }

    }
}
