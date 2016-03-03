using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epi.ServiceClient
{
    public  class EWEConfigValues
    {
                private   int _EWEServiceAuthMode ;  
                private string _EWEServiceEndpointAddress;
                private string _EWEServiceBindingMode ;
                private   int _EWEServiceMaxBufferPoolSize ;
                private   int _EWEServiceMaxReceivedMessageSize;
                private   int _EWEServiceReaderMaxDepth;
                private   int _EWEServiceReaderMaxStringContentLength;
                private   int _EWEServiceReaderMaxArrayLength ;
                private   int _EWEServiceReaderMaxBytesPerRead ;
                private   int _EWEServiceReaderMaxNameTableCharCount ;
                private Configuration _config = Configuration.GetNewInstance();
              
                public string EWEServiceEndpointAddress
                {
                    get { return _EWEServiceEndpointAddress; }
                    set {
                        try
                        {
                            _EWEServiceEndpointAddress = value;
                        }
                        catch(Exception ex)
                        {
                            _config.Settings.EWEServiceEndpointAddress = "";

                            Configuration.Save(_config);
                        }
                        }
                }
                public string EWEServiceBindingMode
                {
                    get { return _EWEServiceBindingMode; }
                    set { 
                        try{
                        _EWEServiceBindingMode = value; 
                          }
                        catch(Exception ex)
                        {
                            _config.Settings.EWEServiceBindingMode = "BASIC";

                            Configuration.Save(_config);
                        }
                    
                    }
                }
                public int  EWEServiceAuthMode
                {
                    get { return _EWEServiceAuthMode; }
                    set { 
                       try{ 
                        _EWEServiceAuthMode = value; 
                      }
                        catch(Exception ex)
                        {
                            _config.Settings.EWEServiceAuthMode = 0;

                            Configuration.Save(_config);
                        }
                    
                    
                    }
                }
        
                public int EWEServiceMaxBufferPoolSize
                {
                    get { return _EWEServiceMaxBufferPoolSize; }
                    set { 
                        try{
                        _EWEServiceMaxBufferPoolSize = value;
                    
                     }
                        catch(Exception ex)
                        {
                            _config.Settings.EWEServiceMaxBufferPoolSize = 524288;

                            Configuration.Save(_config);
                        }
                    
                    }
                }
                public int EWEServiceMaxReceivedMessageSize
                {
                    get { return _EWEServiceMaxReceivedMessageSize; }
                    set { 
                        try{
                        _EWEServiceMaxReceivedMessageSize = value;
                    
                     }
                        catch(Exception ex)
                        {
                            _config.Settings.EWEServiceMaxReceivedMessageSize = 999999999;

                            Configuration.Save(_config);
                        }
                    
                    }
                }
                public int EWEServiceReaderMaxDepth
                {
                    get { return _EWEServiceReaderMaxDepth; }
                    set { 
                      
                        try{
                        
                        _EWEServiceReaderMaxDepth = value; 
                    }
                        catch(Exception ex)
                        {
                            _config.Settings.EWEServiceReaderMaxDepth = 32;

                            Configuration.Save(_config);
                        }
                    
                    }
                }
                public int EWEServiceReaderMaxStringContentLength
                {
                    get { return _EWEServiceReaderMaxStringContentLength; }
                    set { 
                        

                       try{ 
                        _EWEServiceReaderMaxStringContentLength = value; 
                    
                     }
                        catch(Exception ex)
                        {
                            _config.Settings.EWEServiceReaderMaxStringContentLength = 2048000;

                            Configuration.Save(_config);
                        }
                    
                    }
                }
                public int EWEServiceReaderMaxArrayLength
                {
                    get { return _EWEServiceReaderMaxArrayLength; }
                    set { 
                        
                        try{
                        _EWEServiceReaderMaxArrayLength = value; 
                    }
                        catch(Exception ex)
                        {
                            _config.Settings.EWEServiceReaderMaxArrayLength = 16384;

                            Configuration.Save(_config);
                        }
                    }
                }
                public int EWEServiceReaderMaxBytesPerRead
                {
                    get { return _EWEServiceReaderMaxBytesPerRead; }
                    set { 
                      try{  
                        _EWEServiceReaderMaxBytesPerRead = value;
                    
                    }
                        catch(Exception ex)
                        {
                            _config.Settings.EWEServiceReaderMaxBytesPerRead = 4096;

                            Configuration.Save(_config);
                        }
                    }
                }
                public int EWEServiceReaderMaxNameTableCharCount
                {
                    get { return _EWEServiceReaderMaxNameTableCharCount; }
                    set { 
                        try{
                        _EWEServiceReaderMaxNameTableCharCount = value; 
                     }
                        catch(Exception ex)
                        {
                            _config.Settings.EWEServiceReaderMaxNameTableCharCount = 16384;

                            Configuration.Save(_config);
                        }
                    
                    }
                }
        public   void SetConfigFile(){
               
              
            
                  
                        try
                        {
                            _EWEServiceEndpointAddress = _config.Settings.EWEServiceEndpointAddress;
                        }
                        catch(Exception ex)
                        {
                            _config.Settings.EWEServiceEndpointAddress = "";

                            Configuration.Save(_config);
                        }
                       
               
                        try{
                            EWEServiceBindingMode = _config.Settings.EWEServiceBindingMode;
                          }
                        catch(Exception ex)
                        {
                            _config.Settings.EWEServiceBindingMode = "BASIC";

                            Configuration.Save(_config);
                        }
                    
                     
                       try{
                           EWEServiceAuthMode = _config.Settings.EWEServiceAuthMode;
                      }
                        catch(Exception ex)
                        {
                            _config.Settings.EWEServiceAuthMode = 0;

                            Configuration.Save(_config);
                        }
                    
                    
                        try{
                            EWEServiceMaxBufferPoolSize = _config.Settings.EWEServiceMaxBufferPoolSize;
                    
                     }
                        catch(Exception ex)
                        {
                            _config.Settings.EWEServiceMaxBufferPoolSize = 524288;

                            Configuration.Save(_config);
                        }
                    
                     
                        try{
                            EWEServiceMaxReceivedMessageSize = _config.Settings.EWEServiceMaxReceivedMessageSize;
                    
                     }
                        catch(Exception ex)
                        {
                            _config.Settings.EWEServiceMaxReceivedMessageSize = 999999999;

                            Configuration.Save(_config);
                        }
                    
                   
                        try{

                            EWEServiceReaderMaxDepth = _config.Settings.EWEServiceReaderMaxDepth;
                    }
                        catch(Exception ex)
                        {
                            _config.Settings.EWEServiceReaderMaxDepth = 32;

                            Configuration.Save(_config);
                        }
                    
                   
                        

                       try{
                           EWEServiceReaderMaxStringContentLength = _config.Settings.EWEServiceReaderMaxStringContentLength;
                    
                     }
                        catch(Exception ex)
                        {
                            _config.Settings.EWEServiceReaderMaxStringContentLength = 2048000;

                            Configuration.Save(_config);
                        }
                    
                    
                        try{
                            EWEServiceReaderMaxArrayLength = _config.Settings.EWEServiceReaderMaxArrayLength;
                    }
                        catch(Exception ex)
                        {
                            _config.Settings.EWEServiceReaderMaxArrayLength = 16384;

                            Configuration.Save(_config);
                        }
                   
                      try{
                          EWEServiceReaderMaxBytesPerRead = _config.Settings.EWEServiceReaderMaxBytesPerRead;
                    
                    }
                        catch(Exception ex)
                        {
                            _config.Settings.EWEServiceReaderMaxBytesPerRead = 4096;

                            Configuration.Save(_config);
                        }
                     
                        try{
                            EWEServiceReaderMaxNameTableCharCount = _config.Settings.EWEServiceReaderMaxNameTableCharCount;
                     }
                        catch(Exception ex)
                        {
                            _config.Settings.EWEServiceReaderMaxNameTableCharCount = 16384;

                            Configuration.Save(_config);
                        }
                    
                    }
                
               
        
        
         
    }
}
