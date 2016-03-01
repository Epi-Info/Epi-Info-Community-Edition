#region Namespaces

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Messaging;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Forms;

using Epi.Windows;
using Epi.Collections;
using VariableCollection = Epi.Collections.NamedObjectCollection<Epi.IVariable>;
using EpiInfo.Plugin;

#endregion  //Namespaces

namespace Epi.Windows
{
    /// <summary>
    /// Epi Info application instance implementation
    /// </summary>
    public sealed class ApplicationManager : System.MarshalByRefObject, IModuleManager, IProjectManager
    {
        #region Private Static Members
        private static string[] commandLineArguments;
        private static ThreadExceptionEventHandler threadExceptionEventHandler;
        private static ExecutionMode Mode = ExecutionMode.NotSet;

        #endregion  //Private Static Members

        #region Public Methods

        /// <summary>
        /// Starts the Epi Info application
        /// </summary>
        /// <remarks>Does not throw exceptions</remarks>
        public static void Start(string[] args)
        {
            if (LoadConfiguration(args))
            {
                
                ApplicationManager.commandLineArguments = args;


                /*
                ApplicationManager server = new ApplicationManager();
                server.StartInternal(ApplicationManager.commandLineArguments);
                */

                new ApplicationManager().StartInternal(ApplicationManager.commandLineArguments);
                System.Windows.Forms.Application.Run();


                /*



                // WinUtil.ShowTraceMessage("Begin mutex");
                // mutex will be unique per configuration file
                string setupGuid = Util.GetFileGuid(Configuration.GetNewInstance().ConfigFilePath).ToString();
                string mutexName = "Local\\EI7" + setupGuid; // note: mutex name length <= 260

                Mutex mutex = null;
                bool mutexCreatedNew = false;

                // The following block of code prevents multiple copies of EpiInfo from 
                // running (and eliminates the empty error message dialog we were receiving). 
                // I don't think it's necessary to try to connect to the process that's 
                // already running - just prevent the new process from running.

                //BEGIN CODE CHANGE

                mutex = new Mutex(true, mutexName, out mutexCreatedNew);

                if (mutexCreatedNew)
                {
                    // Epi Info starts up here. Upon Application.Run, the process thread 
                    // creates a message thread and will block until the message thread terminates

                    ApplicationManager server = new ApplicationManager();
                    server.StartInternal(ApplicationManager.commandLineArguments);
                    //WinUtil.ShowTraceMessage("start message thread");
                    // start message thread
                    //if (!server.moduleCollection.Count.Equals(0))
                        System.Windows.Forms.Application.Run();
                    //GC.KeepAlive(mutex);
                }
                else
                {
                    MsgBox.ShowError(SharedStrings.WARNING_APPLICATION_RUNNING);
                    Application.Exit();
                }*/

                //END CODE CHANGE

                #region Deprecated Code
                //ORIGINAL CODE. UNCOMMENT THIS SECTION IF THE ABOVE CODE USAGE ISN"T WORKING.
                // create mutex instance, ensuring disposal afterward
                //using (mutex = new Mutex(true, mutexName, out mutexCreatedNew))
                //{
                //    try
                //    {
                //        // then application is not currently running
                //        if (mutexCreatedNew)
                //        {
                //            // Epi Info starts up here. Upon Application.Run, the process thread 
                //            // creates a message thread and will block until the message thread terminates

                //            ApplicationManager server = new ApplicationManager();
                //            server.StartInternal(ApplicationManager.commandLineArguments);
                //            //WinUtil.ShowTraceMessage("start message thread");
                //            // start message thread
                //            System.Windows.Forms.Application.Run();
                //        }
                //        // Application instance is already running, use interprocess communication 
                //        // to request module load
                //        else
                //        {

                //            try
                //            {
                //                // get a object reference to remoting server
                //                ApplicationManager client = ApplicationManager.CreateInterProcessCommunicationClient();
                //                client.StartRemotely(args);
                //            }
                //            catch (RemotingException rex)
                //            {
                //                System.Diagnostics.Debug.WriteLine(rex.Message);
                //                throw;
                //            }
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        System.Diagnostics.Debug.WriteLine(ex.Message);
                //        MsgBox.ShowException(ex);
                //        return;
                //    }
                //    finally
                //    {
                //        //WinUtil.ShowTraceMessage("release mutex");
                //        // release mutex before disposal to be safe
                //        try
                //        {
                //            if (mutex != null)
                //            {
                //                mutex.ReleaseMutex();
                //            }
                //        }
                //        catch
                //        {
                //            // eat any exceptions throw while trying to release mutex
                //        }
                //    }
                //}
                #endregion

            }
        }

        #endregion //Public Methods

        #region Private Methods

        /// <summary>
        /// Loads the configuration file
        /// </summary>
        /// <param name="args">Line command arguments </param>
        /// <returns>Boolean to indicate if the loading of the configuration file was successful</returns>
        private static bool LoadConfiguration(string[] args)
        {
            // TODO: parse command line to load configuration if specified
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
                    // create a default configuration file and save it
                    Configuration defaultConfig = Configuration.CreateDefaultConfiguration();
                    Configuration.Save(defaultConfig);
                }

                Configuration.Load(configFilePath);
            }
            catch (ConfigurationException)
            {
                configurationOk = RecoverFromConfigurationError(configFilePath);
            }
            catch (Exception ex)
            {
                configurationOk = ex.Message == "";
            }

            return configurationOk;
        }

        /// <summary>
        /// Catch unhandled thread exceptions and show message box
        /// </summary>
        /// <param name="ex">Unhandled exception</param>
        private static void OnUnhandledException(Exception ex)
        {
            //#if(!DEBUG)
            Epi.Windows.MsgBox.ShowException(ex);
            //#endif
        }

        /// <summary>
        /// Restores error-free configuration file
        /// </summary>
        /// <param name="configFilePath">The file path of the configuration file</param>
        /// <returns>Boolean indicating if the configuration file was restored</returns>
        private static bool RecoverFromConfigurationError(string configFilePath)
        {
            DialogResult result = MsgBox.ShowQuestion(SharedStrings.RESTORE_CONFIGURATION_Q, MessageBoxButtons.YesNo);
            if ((result == DialogResult.Yes) || (result == DialogResult.OK))
            {
                if (string.IsNullOrEmpty(configFilePath))
                {
                    throw new ArgumentNullException("configFilePath");
                }

                try
                {
                    if (File.Exists(configFilePath))
                    {
                        // Get the current time and format it to append it to the file name.
                        string timeStamp = DateTime.Now.ToString().Replace(StringLiterals.FORWARD_SLASH, StringLiterals.UNDER_SCORE);
                        timeStamp = timeStamp.Replace(StringLiterals.BACKWARD_SLASH, StringLiterals.UNDER_SCORE);
                        timeStamp = timeStamp.Replace(StringLiterals.COLON, StringLiterals.UNDER_SCORE);
                        timeStamp = timeStamp.Replace(StringLiterals.SPACE, StringLiterals.UNDER_SCORE);


                        string oldConfigFilePath = configFilePath + "." + timeStamp;
                        File.Copy(configFilePath, oldConfigFilePath);
                        File.Delete(configFilePath);
                    }

                    Configuration defaultConfig = Configuration.CreateDefaultConfiguration();
                    Configuration.Save(defaultConfig);
                    Configuration.Load(configFilePath);
                    return true;

                }
                catch (Exception)
                {
                    return false;
                }

            }
            return false;
        }

        #endregion  //Private Methods

        #region Private Fields
        /// <summary>
        /// Collection can only be set at construction because we use the object 
        /// reference for thread sychronization
        /// </summary>
        private readonly List<IModule> moduleCollection;

        /// <summary>
        /// Collection can only be set at construction because we use the object 
        /// reference for thread sychronization
        /// </summary>
        private readonly ProjectCollection projectCollection;

        /// <summary>
        /// Service locator master context
        /// </summary>
        private readonly IServiceContainer services;

        /// <summary>
        /// True only while we are processing a request to unload all modules 
        /// </summary>
        private bool unloading = false;
        #endregion  //Private Fields

        #region Contructors and Destructors

        /// <summary>
        /// Class is implemented as a singleton, a private constructor is desired
        /// </summary>
        private ApplicationManager()
        {
            this.moduleCollection = new List<IModule>();
            this.projectCollection = new ProjectCollection();
            this.services = new ServiceContainer();
        }

        /// <summary>
        /// <para>Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="T:System.ComponentModel.Component" /> is reclaimed by garbage collection. </para>
        /// </summary>
        ~ApplicationManager()
        {
            this.Dispose(false);
        }

        #endregion  //Constructors

        #region Project Management
        /// <summary>
        /// Open project from file path
        /// </summary>
        /// <param name="filePath">The file path of the project</param>
        /// <returns>Instance of project</returns>
        Project IProjectManager.OpenProject(string filePath)
        {
            Project project = null;

            if (this.projectCollection.Contains(filePath))
            {
                project = this.projectCollection[filePath];

            }
            else
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException(SharedStrings.FILE_NOT_FOUND, filePath);
                }

                project = new Project(filePath);

                lock (this.projectCollection.SyncRoot)
                {
                    this.projectCollection.Add(project);
                }
            }

            return project;
        }

        /// <summary>
        /// Creates new empty project 
        /// </summary>		
        /// <returns>Instance of project</returns>
        Project IProjectManager.CreateNewProject()
        {
            Project project = new Project();

            lock (this.projectCollection.SyncRoot)
            {
                this.projectCollection.Add(project);
            }
            return project;
        }

        #endregion  //Project Management

        #region Application Management
        /// <summary>
        /// Factory method creates or retrieves instantiated application modules
        /// </summary>
        /// <param name="typeName">
        ///     The assembly-qualified name of the type to get. See System.Type.AssemblyQualifiedName.
        ///     If the type is in the currently executing assembly or in Mscorlib.dll, it
        ///     is sufficient to supply the type name qualified by its namespace.
        ///</param>        
        public IModule CreateModuleInstance(string typeName)
        {
            IModule module;

            // locate type for specified Epi Info module  
            Type type = null;
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            DataRow[] result = Configuration.GetNewInstance().Modules.Select("Name='" + typeName + "'");
            if (result.Length == 1)
            {
                type = GetModuleType(((Epi.DataSets.Config.ModuleRow)result[0]).Type);
            }
            else
            {
                type = Type.GetType(typeName);
            }

            if (type == null)
                throw new GeneralException("Could not load module type."); // TODO: Hard coded string



            // if module is not already loaded in memory, create a new instance
            module = Activator.CreateInstance(type) as IModule;

            return module;
        }

        ///// <summary>
        ///// Initializes the application, starts module management remoting server, and launches 
        ///// module specified with the command line. If no command line options are given, 
        ///// application will load the menu module by default.
        ///// </summary>
        //private void OnMessageThreadStarted()
        //{
        //    IServiceContainer serviceContainer = (IServiceContainer) this.GetService(typeof(IServiceContainer));
        //    this.PublishServices(serviceContainer, false);
        //    this.StartRemotingServer(3);
        //    this.StartInternal(ApplicationManager.commandLineArguments);
        //}

        /// <summary>
        /// Starts unhandled exception handler
        /// </summary>
        private static void AttachExceptionHandler()
        {
            if (ApplicationManager.threadExceptionEventHandler == null)
            {
                ApplicationManager.threadExceptionEventHandler = new ThreadExceptionEventHandler(ApplicationManager.Application_ThreadException);
                System.Windows.Forms.Application.ThreadException += threadExceptionEventHandler;
            }
        }

        /// <summary>
        /// Removes unhandled exception handler
        /// </summary>
        private static void DetachExceptionHandler()
        {
            if (ApplicationManager.threadExceptionEventHandler != null)
            {
                System.Windows.Forms.Application.ThreadException -= threadExceptionEventHandler;
                ApplicationManager.threadExceptionEventHandler = null;
            }
        }

        /// <summary>
        /// Extracts default resources from various sources 
        /// </summary>
        private static void CreateWindowsResources()
        {
            Configuration config = Configuration.GetNewInstance();

            // Configuration Directory
            if (!Directory.Exists(config.Directories.Configuration))
            {
                Directory.CreateDirectory(config.Directories.Configuration);
            }

            // Templates Directory
            if (!Directory.Exists(config.Directories.Templates))
            {
                Directory.CreateDirectory(config.Directories.Templates);
            }

            // Output Directory
            if (!Directory.Exists(config.Directories.Output))
            {
                Directory.CreateDirectory(config.Directories.Output);
            }

            // Project Directory
            string projectsDirectory = config.Directories.Project;
            if (!string.IsNullOrEmpty(projectsDirectory))
            {
                if (!Directory.Exists(config.Directories.Project))
                {
                    Directory.CreateDirectory(config.Directories.Project);
                }
            }

            // Logs directory ..
            string logDir = config.Directories.LogDir;
            if (!string.IsNullOrEmpty(logDir))
            {
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
            }

            // Mnu file
          /*  if (!File.Exists(Files.MnuFilePath))
            {
                using (FileStream fs = new FileStream(Files.MnuFilePath, FileMode.CreateNew, FileAccess.Write))
                {
                    using (Stream resourceStream = Epi.Resources.ResourceLoader.GetMnuFile())
                    {
                        Util.CopyStream(resourceStream, fs);
                        fs.Close();
                    }
                }
            }*/

            // Sample7 prj and mdb ...
            string samplePrj = Path.Combine(config.Directories.Configuration, "Sample7.prj");
            if (File.Exists(samplePrj))
            {
                if ((File.GetAttributes(samplePrj) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    File.SetAttributes(samplePrj, File.GetAttributes(samplePrj) | FileAttributes.ReadOnly);
                }

                //using (FileStream fs = new FileStream(samplePrj, FileMode.CreateNew, FileAccess.Write))
                //{
                //    using (Stream resourceStream = Epi.Resources.ResourceLoader.GetSampleProject())
                //    {
                //        Util.CopyStream(resourceStream, fs);
                //        fs.Close();
                //    }
                //}
            }
            string sampleMdb = Path.Combine(config.Directories.Configuration, "Sample7.mdb");
            if (File.Exists(sampleMdb))
            {
                if ((File.GetAttributes(sampleMdb) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    File.SetAttributes(sampleMdb, File.GetAttributes(sampleMdb) | FileAttributes.ReadOnly);
                }

                //using (FileStream fs = new FileStream(sampleMdb, FileMode.CreateNew, FileAccess.Write))
                //{
                //    using (Stream resourceStream = Epi.Resources.ResourceLoader.GetSampleDatabase())
                //    {
                //        Util.CopyStream(resourceStream, fs);
                //        fs.Close();
                //    }
                //}
            }
        }

        /// <summary>
        /// Publish standard application manager services to container
        /// </summary>
        private void PublishServices(IServiceContainer serviceContainer, bool overwriteExistingServices)
        {
            PublishService(this.GetType(), this, serviceContainer, overwriteExistingServices);
            PublishService(typeof(IModuleManager), this, serviceContainer, overwriteExistingServices);
            PublishService(typeof(IProjectManager), this, serviceContainer, overwriteExistingServices);
            PublishService(typeof(IServiceContainer), serviceContainer, serviceContainer, overwriteExistingServices);
            PublishService(typeof(IServiceProvider), serviceContainer, serviceContainer, overwriteExistingServices);
        }

        /// <summary>
        /// Publish application service to container
        /// </summary>
        private static void PublishService(Type type, object service, IServiceContainer serviceContainer, bool overwriteExistingService)
        {
            if (serviceContainer.GetService(type) != null)
            {
                if (overwriteExistingService)
                {
                    serviceContainer.RemoveService(type, false);
                    serviceContainer.AddService(type, service);
                }
            }
            else
            {
                serviceContainer.AddService(type, service);
            }
        }

        /// <summary>
        /// Instantiate and load application module using supplied command line arguments
        /// </summary>
        /// <param name="commandLineArguments">The command line arguments</param>
        private void StartInternal(string[] commandLineArguments)
        {
            IServiceContainer serviceContainer = (IServiceContainer)this.GetService(typeof(IServiceContainer));
            this.PublishServices(serviceContainer, false);
            this.CreateInterProcessCommunicationServer(3);

            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.DoEvents();

            // heal application, create default folders, etc.
            ApplicationManager.CreateWindowsResources();

            //set thread culture
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Configuration.GetNewInstance().Settings.Language);

            // advertise the execution environment
            Configuration.Environment = ExecutionEnvironment.WindowsApplication;
            Configuration.MasterServiceProvider = this;

            ICommandLine commandLine = new CommandLine(commandLineArguments);

            //TODO: load default module from configuration
            string moduleToLoad = "Menu";

            // parse Command Line
            string moduleArgument = commandLine.GetArgument("l");
            if (moduleArgument != null)
            {
                moduleToLoad = moduleArgument;
            }

            try
            {
                // launch module
                IModule module = this.CreateModuleInstance(moduleToLoad);
                module.Load(this, commandLine);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(string.Format(SharedStrings.CANNOT_LOAD_APPLICATION, moduleToLoad), ex);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Late binds to specified assembly path and returns desired type
        /// </summary>		
        /// <param name="typeName">Name of Type to load from assembly</param>
        /// <returns>Type for module</returns>
        private static Type GetModuleType(string typeName)
        {
            try
            {
                Type type = Type.GetType(typeName);
                if (!typeof(IWindowsModule).IsAssignableFrom(type))
                {
                    type = null;
                }
                return type;
            }
            catch (Exception ex)
            {
                throw new GeneralException(string.Format(SharedStrings.CANNOT_LOAD_APPLICATION, typeName), ex);
            }
        }

        /// <summary>
        /// Check active module count. If active module count is less than one, close application down.
        /// </summary>
        private void CloseOnZeroActiveModules()
        {
            lock (this.moduleCollection)
            {
                if (this.moduleCollection.Count < 1)
                {
                    this.Dispose(true);
                }
            }
        }

        #endregion  //Application Management

        #region IModuleManager
        /// <summary>
        /// Formally load module into application
        /// </summary>
        /// <param name="module">Module to load</param>
        void IModuleManager.Attach(IModule module)
        {
            lock (this.moduleCollection)
            {
                this.moduleCollection.Add(module);
            }
        }

        /// <summary>
        /// Formally unload module from application
        /// </summary>
        /// <param name="module">Module to unload</param>
        void IModuleManager.Detach(IModule module)
        {
            try
            {
                // if we are unloading all the modules, then we've already locked the 
                // collection and waiting for unlock may result in a deadlock condition 
                // from an UnloadAll method invokation already on the call stack

                if (!unloading)
                {
                    lock (this.moduleCollection)
                    {
                        bool exists = this.moduleCollection.Remove(module);
                    }
                }
                else
                {
                    this.moduleCollection.Remove(module);
                }
            }
            catch
            {
                // eat exception
                // this is a unrecoverable error, terminate all threads
                this.Dispose();
                return;
            }

            // must be executed outside the context of a synchronization lock
            this.CloseOnZeroActiveModules();
        }

        /// <summary>
        /// Request that all modules unload
        /// </summary>
        void IModuleManager.UnloadAll()
        {
            try
            {
                unloading = true;
                lock (this.moduleCollection)
                {
                    IModule[] loadedModules = this.moduleCollection.ToArray();
                    foreach (IModule module in loadedModules)
                    {
                        module.Unload();
                    }
                }
            }
            finally
            {
                // must signal completion in finally 
                unloading = false;
            }
        }
        #endregion  //IModuleManager

        #region Remoting

        /// <summary>
        /// Provides access to the remote server URL
        /// </summary>
        /// <returns>URL of remote well-known object registration</returns>
        static public string RemoteServerUrl
        {
            get
            {
                return @"tcp://127.0.0.1:" + Configuration.GetNewInstance().Settings.FrameworkTcpPort + "/" + GetUniqueRemotingUri(Configuration.GetNewInstance().ConfigFilePath);
            }
        }

        /// <summary>
        /// Creates a new proxy to connect to previously published remote instance 
        /// </summary>
        /// <returns>Proxy client to remote server object</returns>
        static public ApplicationManager CreateInterProcessCommunicationClient()
        {
            if (Mode == ExecutionMode.Server)
            {
                throw new InvalidOperationException("Client proxy cannot be created in this AppDomain because a server has been created.");
            }

            Mode = ExecutionMode.Client;

            IDictionary properties = new Hashtable(3);
            properties.Add("bindTo", "127.0.0.1");
            properties.Add("port", 0);
            TcpChannel tcpChannel = new TcpChannel(properties, null, null);
            ChannelServices.RegisterChannel(tcpChannel, false);

            return RemotingServices.Connect(typeof(ApplicationManager), RemoteServerUrl) as ApplicationManager;
        }

        /// <summary>
        /// Overrides default lifetime service so that the lifetime is not limited
        /// </summary>
        /// <returns>null</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary>
        /// Initializes TCP channel and starts remoting server. Should only be called once!
        /// </summary>
        private void CreateInterProcessCommunicationServer(int maximumRetries)
        {
            ThreadExceptionEventHandler terminator = new ThreadExceptionEventHandler(Application_FatalError);
            Application.ThreadException += terminator;

            if (Mode == ExecutionMode.Client)
            {
                throw new InvalidOperationException("Server cannot be created in this AppDomain because a client has been created");
            }
            else if (Mode == ExecutionMode.Server)
            {
                return;
            }

            int retryCount = 0;

            try
            {
                Configuration config = Configuration.GetNewInstance();
                while (true)
                {
                    try
                    {
                        IDictionary properties = new Hashtable(3);
                        properties.Add("bindTo", "127.0.0.1");
                        properties.Add("port", config.Settings.FrameworkTcpPort);
                        TcpChannel tcpChannel = new TcpChannel(properties, null, null);
                        ChannelServices.RegisterChannel(tcpChannel, false);
                        //chnl.GetUrlsForUri(RemotingUri)[0];
                        RemotingServices.Marshal(this, GetUniqueRemotingUri(config.ConfigFilePath));

                        // exit retry while loop
                        break;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);

                        SocketException sex = ex as SocketException;
                        if (sex != null)
                        {
                            // port already in use
                            if (sex.ErrorCode == 10048)
                            {
                                // increment port number and for retry
                                config.Settings.FrameworkTcpPort += 1;
                                Configuration.Save(config);
                            }
                        }

                        if (++retryCount > maximumRetries)
                        {
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new GeneralException("Framework interprocess communication initalization failed. Please restart and try again.", ex);
            }

            Mode = ExecutionMode.Server;
            Application.ThreadException -= terminator;


            // the unhandled exception handler is disabled in debug builds 
            AttachExceptionHandler();
        }

        /// <summary>
        /// Creates a remoting url based on the configuration file path
        /// </summary>
        /// <returns></returns>
        private static string GetUniqueRemotingUri(string path)
        {
            int volumeSeperatorPos = path.IndexOf(Path.VolumeSeparatorChar);
            if (volumeSeperatorPos > -1)
            {
                path = path.Substring(volumeSeperatorPos + 1, path.Length - volumeSeperatorPos - 1);
            }

            path = path.Trim('\\').Replace("\\", "/") + ".rem";

            return path;
        }

        /// <summary>
        /// Signal remote process to load application module with supplied command line arguments
        /// </summary>
        //[OneWay]
        public void StartRemotely(string[] commandLineArguments)
        {
            this.StartInternal(commandLineArguments);
        }
        #endregion  //Remoting

        #region Private Event Handlers

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            ApplicationManager.OnUnhandledException(e.Exception);
        }

        private static void Application_FatalError(object sender, ThreadExceptionEventArgs e)
        {
            // TODO: hard coded string
            string msg = "A fatal error was encountered. The application cannot continue. \n\nCause: " + e.Exception.Message;
            Logger.Log(msg);
            MessageBox.Show(msg);

            // hard exit
            Application.ExitThread();
        }

        #endregion  //Private Event Handlers

        #region Nested Types
        enum ExecutionMode { NotSet, Server, Client }

        #endregion Nested Types

        /// <summary>
        /// The project collection
        /// </summary>
        private class ProjectCollection : ICollection, IEnumerable
        {
            #region Private Data Members

            private object syncRoot = null;

            //the open project collection is always anticipated to be small
            private System.Collections.Generic.List<Project> projects;

            #endregion  //Private Data Members

            #region Constructors
            public ProjectCollection()
            {
                syncRoot = new object();
                projects = new System.Collections.Generic.List<Project>();
            }
            #endregion  //Constructors

            #region Public Methods

            /// <summary>
            /// Determines if guid exists for project
            /// </summary>
            /// <param name="filePath">The file path of the project</param>
            /// <returns>Boolean indicating if guid exists for the project</returns>
            public bool Contains(string filePath)
            {
                Guid guid = Util.GetFileGuid(filePath);
                return this.Contains(guid);
            }

            /// <summary>
            /// Determines if project collection contains specified project
            /// </summary>
            /// <param name="project">An Epi project</param>
            /// <returns>Boolean indicating if project exists in the collection</returns>
            public bool Contains(Project project)
            {
                return this.projects.Contains(project);
            }

            /// <summary>
            /// Determines if guid exists in the projects collection
            /// </summary>
            /// <param name="guid">A guid</param>
            /// <returns>Boolean indicating if guid exists in the projects collection</returns>
            protected bool Contains(Guid guid)
            {
                foreach (Project project in projects)
                {
                    if (project.Id == guid)
                        return true;
                }
                return false;
            }

            /// <summary>
            /// Adds a project to the collection if it does not already exist
            /// </summary>
            /// <param name="project">An Epi project</param>
            public void Add(Project project)
            {
                if (this.Contains(project))
                {
                    throw new ArgumentException("Project already exists in collection.", "project");
                }
                this.projects.Add(project);
            }

            /// <summary>
            /// Remove a project if it does exist in the collection
            /// </summary>
            /// <param name="project"></param>
            public void Remove(Project project)
            {
                if (!this.Contains(project))
                {
                    throw new ArgumentException("Project does not exists in collection.", "project");
                }
                this.projects.Remove(project);
            }

            #endregion  //Public Methods

            #region Protected Properties

            /// <summary>
            /// Retrieves projects given a specified guid
            /// </summary>
            /// <param name="guid">A guid</param>
            /// <returns>Project with specified corresponding guid</returns>
            protected Project this[Guid guid]
            {
                get
                {
                    foreach (Project project in this.projects)
                    {
                        if (project.Id == guid)
                            return project;
                    }
                    throw new ArgumentException("Project guid does not exists in collection.", "guid");
                }
            }

            /// <summary>
            /// Returns the project with a specified file path
            /// </summary>
            /// <param name="filePath">The file path of the project</param>
            /// <returns>Project of specified corresponding file path</returns>
            public Project this[string filePath]
            {
                get
                {
                    Guid guid = Util.GetFileGuid(filePath);
                    return this[guid];
                }
            }

            #endregion //Protected Properties

            #region ICollection Members

            /// <summary>
            /// Determines if collection is synchronized
            /// </summary>
            public bool IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            /// <summary>
            /// Returns the number of projects in the collection
            /// </summary>
            public int Count
            {
                get
                {
                    return this.projects.Count;
                }
            }

            /// <summary>
            /// Copies to an array at the specified index
            /// </summary>
            /// <param name="array">Array to be copied to</param>
            /// <param name="index">Index in which copy starts</param>
            public void CopyTo(Array array, int index)
            {
                throw new NotSupportedException("CopyTo doesn't work here.");
            }

            public object SyncRoot
            {
                get
                {
                    return this.syncRoot;
                }
            }

            #endregion  //ICollection Members

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.projects.GetEnumerator();
            }

            #endregion  //IEnumerable Members
        }

        /// <summary>
        /// Declaration of delegate Void Delegate
        /// </summary>
        public delegate void VoidDelegate();

        #region IDisposable Members
        /// <summary>
        /// Releases all resources used
        /// </summary>        
        public void Dispose()
        {
            // the application closes on dispose, there is nothing left to do
            this.Dispose(true);

            //GC.SuppressFinalize(this);
        }

        /// <summary>
        /// <para>Releases the unmanaged resources used and optionally releases the managed resources.</para>
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // since this is the end of the line, we'll lock the collection to prevent
                // any more modules from loading, but we won't release the exclusive lock
                Monitor.TryEnter(this.moduleCollection);

                for (int i = 0; i < this.moduleCollection.Count; i++)
                {
                    this.moduleCollection[i].Dispose();
                }

                //Exit the message loop on the current thread and close all windows on the thread
                Application.ExitThread();
            }
        }
        #endregion  //IDisposable Members

        #region IServiceProvider Members

        /// <summary>
        /// <para>Gets the service object of the specified type, if it is available.</para>
        /// </summary>
        /// <param name="serviceType">The <see cref="T:System.Type" /> of the service to retrieve.</param>
        /// <returns>
        /// <para>An <see cref="T:System.Object" /> 
        /// implementing the requested service, or <see langword="null" /> if the service cannot be resolved.</para>
        /// </returns>
        [System.Diagnostics.DebuggerStepThrough()]
        public object GetService(Type serviceType)
        {
            if (serviceType == this.GetType())
            {
                return this;
            }
            else
            {
                return this.services.GetService(serviceType);
            }
        }

        #endregion  //IServiceProvider Members
    }
}
