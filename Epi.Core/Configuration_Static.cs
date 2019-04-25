using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using Config = Epi.DataSets.Config;

namespace Epi
{
    /// <summary>
    /// Class Configuration
    /// </summary>
    public partial class Configuration
    {
        /// <summary>
        /// Event Handler for ConfigurationUpdated
        /// </summary>
        public static event EventHandler ConfigurationUpdated;
        /// <summary>
        /// Identifier for Access driver that is built into Epi Info
        /// </summary>
        public const string AccessDriver = "Epi.Data.Office.AccessDBFactory, Epi.Data.Office";  //will be removed, Zack

        public const string Access2007Driver = "Epi.Data.Office.Access2007DBFactory, Epi.Data.Office";

        /// <summary>
        /// Identifier for Sql Driver that is built into Epi Info
        /// </summary>
        public const string SqlDriver = "Epi.Data.SqlServer.SqlDBFactory, Epi.Data.SqlServer";  //will be removed, Zack

        /// <summary>
        /// Identifier for Excel driver that is built into Epi Info
        /// </summary>
        public const string ExcelDriver = "Epi.Data.Office.ExcelWBFactory, Epi.Data.Office";

        public const string Excel2007Driver = "Epi.Data.Office.Excel2007WBFactory, Epi.Data.Office";

        public const string CsvDriver = "Epi.Data.Office.CsvFileFactory, Epi.Data.Office";

        /// <summary>
        /// Identifier for SharePoint driver that is built into Epi Info
        /// </summary>
        public const string SharePointDriver = "Epi.Data.Office.SharePointListFactory, Epi.Data.Office";

        /// <summary>
        /// Identifier for MySQL driver that is built into Epi Info
        /// </summary>
        public const string MySQLDriver = "Epi.Data.MySQL.MySQLDBFactory, Epi.Data.MySQL";

        /// <summary>
        /// Identifier for MongoDB driver that is built into Epi Info
        /// </summary>
        public const string MongoDBDriver = "Epi.Data.MongoDB.MongoDBDBFactory, Epi.Data.MongoDB";


        /// <summary>
        /// Identifier for Epi Info Web driver that is built into Epi Info
        /// </summary>
        public const string EpiInfoWebDriver = "Epi.Data.EpiWeb.EpiWebFactory, Epi.Data.EpiWeb";

        /// <summary>
        /// Identifier for PostgreSQL driver that is built into Epi Info
        /// </summary>
        public const string PostgreSQLDriver = "Epi.Data.PostgreSQL.PostgreSQLDBFactory, Epi.Data.PostgreSQL";

        /// <summary>
        /// Identifier for Web driver that is built into Epi Info
        /// </summary>
        public const string WebDriver = "Epi.Data.WebDriver";

        private const string passPhrase = "80787d6053694493be171dd712e51c61";
        private const string saltValue = "476ba16073764022bc7f262c6d67ebef";
        private const string initVector = "0f8f*d5bd&cb4~9f";

        private const string saltValueAlt = "JrudNkxXEzUj3Ij9KhHfzlZyonwVW45b";
        private const string initVectorAlt = ":!0wn4f#;FHy>Yi;";

        private const string initVectorDroid = "00000000000000000000000000000000";
        private const string saltDroid = "00000000000000000000";

        private const string passPhraseDebug = "80787d6053694493be171dd712e51c61";
        private const string saltValueDebug = "476ba16073764022bc7f262c6d67ebef";
        private const string initVectorDebug = "0f8f*d5bd&cb4~9f";

        private const string saltValueAltDebug = "JrudNkxXEzUj3Ij9KhHfzlZyonwVW45b";
        private const string initVectorAltDebug = ":!0wn4f#;FHy>Yi;";

        private const string initVectorDroidDebug = "00000000000000000000000000000000";
        private const string saltDroidDebug = "00000000000000000000";

        private static object syncLock = new object();
        private static Configuration current;
        private static FileSystemWatcher watcher;
        private static IServiceProvider masterServiceProvider;
        private static ExecutionEnvironment environment = ExecutionEnvironment.Unknown;
        private static FileSystemEventHandler configUpdateCallback = new FileSystemEventHandler(OnConfigChanged);

        /// <summary>
        /// Environment
        /// </summary>
        public static ExecutionEnvironment Environment
        {
            get
            {
                return environment;
            }
            set
            {
                environment = value;
            }
        }

        /// <summary>
        /// Master Service Provider
        /// </summary>
        public static IServiceProvider MasterServiceProvider
        {
            get { return masterServiceProvider; }
            set
            {
                if (masterServiceProvider != null)
                {
                    throw new GeneralException(SharedStrings.ERROR_APPDOMAIN_MASTER_SERVC_PRVDR);
                }
                masterServiceProvider = value;
            }
        }

        /// <summary>
        /// Returns a new instance of the configuration class
        /// </summary>
        /// <returns></returns>
        public static Configuration GetNewInstance()
        {
            AssertConfigurationLoaded();
            Config configDataSet = (Config)current.configDataSet.Copy();
            string filePath = current.ConfigFilePath;
            return new Configuration(filePath, configDataSet);
        }



        

            /// <summary>
        /// Returns a new instance of the configuration class
        /// </summary>
        /// <returns></returns>
        public static Configuration GetNewInstance(Dictionary<string, string> pOverrideSettingList)
        {
            AssertConfigurationLoaded();
            Config configDataSet = (Config)current.configDataSet.Copy();
            string filePath = current.ConfigFilePath;
            return new Configuration(filePath, configDataSet);
        }

        /// <summary>
        /// What to do on project accessed
        /// </summary>
        /// <param name="project">Project that is accessed</param>
        public static void OnProjectAccessed(Project project)
        {
            #region Input validation
            AssertConfigurationLoaded();

            if (project == null)
            {
                throw new ArgumentNullException("project");
            }
            #endregion Input validation

            AssertConfigurationLoaded();
            Config.RecentProjectDataTable mruProjects = current.RecentProjects;

            // Check if this project is already in the list. 
            bool projectFound = false;
            foreach (Config.RecentProjectRow row in mruProjects)
            {
                if (row["Location"].ToString() == project.FilePath)
                {
                    //Found the project.
                    row.LastAccessed = DateTime.Now;
                    projectFound = true;
                }
            }
            if (projectFound == false)
            {
                // this is a new project.				
                if (mruProjects.Count >= current.Settings.MRUProjectsCount)
                {
                    // The list is full. Make room by removing the oldest project.
                    mruProjects.RemoveRecentProjectRow(GetOldestRecentProject(mruProjects));
                }

                mruProjects.AddRecentProjectRow(project.Name, project.FilePath, DateTime.Now, current.ParentRowRecentProjects);
            }

            List<string> viewNames = project.GetViewNames();
            List<View> viewsInProject = new List<View>();

            for (int viewIndex = 0; viewIndex < viewNames.Count; viewIndex++)
            {
                viewsInProject.Add(project.GetViewByName(viewNames[viewIndex]));
            }

            foreach (View view in viewsInProject)
            {
                try
                {
                    string checkcodeVars = view.CheckCodeVariableDefinitions;

                    string[] checkCodeLines = checkcodeVars.Split(Convert.ToChar("\n"));

                    if (checkCodeLines[0] != String.Empty)
                    {
                        for (int i = 0; i < checkCodeLines.Length; i++)
                        {
                            if (checkCodeLines[i].Trim().Length > 0)
                            {
                                int offset = (checkCodeLines[i].IndexOf(StringLiterals.SPACE, 7) - 7);
                                if (offset > 0)
                                {
                                    string variableName = checkCodeLines[i].Substring(7, offset);

                                    Config.PermanentVariableDataTable permVars = current.PermanentVariables;
                                    // Check if this var is already in the list. 
                                    bool variableFound = false;
                                    foreach (Config.PermanentVariableRow row in permVars)
                                    {
                                        if (row["Name"].ToString() == variableName)
                                        {
                                            //Found the variable.
                                            variableFound = true;
                                        }
                                    }
                                    if (!(variableFound))
                                    {
                                        current.ConfigDataSet.PermanentVariable.AddPermanentVariableRow(variableName, "Null", (int)DataType.Text, current.ParentRowPermanentVariables);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (ApplicationException aex)
                {
                    throw new ApplicationException(String.Format(SharedStrings.ERROR_LOADING_DEFINED_VARS, aex.Message));
                }
            }


            Save();
        }

        /// <summary>
        /// What to do on view accessed
        /// </summary>
        /// <param name="accessedView">View that is accessed</param>
        public static void OnViewAccessed(View accessedView)
        {
            if (! accessedView.IsRelatedView)
            {
                AssertConfigurationLoaded();
                #region Input Validation

                // All public methods should assert that configuration has been loaded
                AssertConfigurationLoaded();

                if (accessedView == null)
                {
                    throw new System.ArgumentNullException("accessedView");
                }
                #endregion Input Validation

                Config.RecentViewDataTable mruViews = current.RecentViews;

                // Check if this view is already in the list. 
                bool viewFound = false;
                foreach (Config.RecentViewRow mruViewRow in mruViews)
                {
                    if (mruViewRow.Name == accessedView.FullName)
                    {
                        //Found the view.
                        mruViewRow.LastAccessed = DateTime.Now;
                        viewFound = true;
                    }
                }
                if (viewFound == false)
                {
                    // this is a new view.				
                    if (mruViews.Count >= current.Settings.MRUViewsCount)
                    {
                        // The list is full. Make room by removing the oldest view.					
                        mruViews.RemoveRecentViewRow(GetOldestRecentView(mruViews));
                    }
                    mruViews.AddRecentViewRow(accessedView.FullName, accessedView.FullName, DateTime.Now, current.ParentRowRecentViews);
                }
                Save();
            }
        }

        /// <summary>
        /// What to do when a data source plug-in has been selected for import
        /// </summary>        
        public static void OnDataDriverImport(string displayName, string type, string fileName, bool isMetadataProvider, bool isDataProvider)
        {
            AssertConfigurationLoaded();
            #region Input Validation

            // All public methods should assert that configuration has been loaded
            AssertConfigurationLoaded();

            if (string.IsNullOrEmpty(displayName))
            {
                throw new System.ArgumentNullException("displayName");
            }
            if (string.IsNullOrEmpty(type))
            {
                throw new System.ArgumentNullException("type");
            }
            #endregion Input Validation

            Config.DataDriverDataTable mruDrivers = current.DataDrivers;

            // Check if this data driver is already in the config
            bool sourceFound = false;
            foreach (Config.DataDriverRow mruDataDriverRow in mruDrivers)
            {
                if (mruDataDriverRow.Type.ToLower().Equals(displayName.ToLower()))
                {
                    //Found the data driver.                    
                    sourceFound = true;
                }
            }

            if (sourceFound == false)
            {
                // this is a new data driver
                mruDrivers.AddDataDriverRow(displayName, type, fileName, isMetadataProvider, isDataProvider, null);
            }

            Save();
        }

        /// <summary>
        /// What to do when a gadget plug-in has been selected for import
        /// </summary>        
        public static void OnGadgetImport(string displayName, string type, string fileName, int menuSection)
        {
            AssertConfigurationLoaded();
            #region Input Validation

            // All public methods should assert that configuration has been loaded
            AssertConfigurationLoaded();

            if (string.IsNullOrEmpty(displayName))
            {
                throw new System.ArgumentNullException("displayName");
            }
            if (string.IsNullOrEmpty(type))
            {
                throw new System.ArgumentNullException("type");
            }
            if (string.IsNullOrEmpty(fileName))
            {
                throw new System.ArgumentNullException("fileName");
            }
            #endregion Input Validation

            Config configDataSet = new Config();

            Config.GadgetsDataTable mruGadgets = current.Gadgets;
            Config.GadgetDataTable mruGadget = current.Gadget;

            // Check if this gadget is already in the config
            bool sourceFound = false;
            foreach (Config.GadgetRow mruGadgetRow in mruGadget.Rows)
            {
                if (mruGadgetRow.Type.ToLower().Equals(type.ToLower()))
                {
                    //Found the gadget
                    sourceFound = true;
                }
            }

            if (sourceFound == false)
            {
                // Data Drivers

                if (configDataSet.Gadgets.Count < 1)
                {
                    Config.GadgetsRow newrow = mruGadgets.NewGadgetsRow();
                    //configDataSet.Gadgets.Rows.Add(newrow);
                    mruGadgets.AddGadgetsRow(newrow);
                }

                Config.GadgetsRow parentGadgetsRow = mruGadgets[0];

                // this is a new gadget
                Config.GadgetRow gadgetRow = mruGadget.NewGadgetRow();
                gadgetRow.GadgetsRow = parentGadgetsRow;
                gadgetRow.DisplayName = displayName;
                gadgetRow.Type = type;
                gadgetRow.FileName = fileName;
                gadgetRow.MenuSection = menuSection;
                mruGadget.AddGadgetRow(gadgetRow);                
            }

            Save();
        }

        /// <summary>
        /// What to do on data source accessed
        /// </summary>        
        public static void OnDataSourceAccessed(string databaseName, string connectionString, string dataProvider)
        {
            AssertConfigurationLoaded();
            #region Input Validation

            // All public methods should assert that configuration has been loaded
            AssertConfigurationLoaded();

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new System.ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new System.ArgumentNullException("databaseName");
            }
            #endregion Input Validation

            Config.RecentDataSourceDataTable mruSources = current.RecentDataSources;

            // Check if this data source is already in the list. 
            bool sourceFound = false;
            foreach (Config.RecentDataSourceRow mruDataSourceRow in mruSources)
            {
                if (mruDataSourceRow.Name == databaseName)
                {
                    //Found the data source.
                    mruDataSourceRow.LastAccessed = DateTime.Now;
                    sourceFound = true;
                }
            }
            if (sourceFound == false)
            {
                // this is a new view.				
                if (mruSources.Count >= current.Settings.MRUDataSourcesCount)
                {
                    // The list is full. Make room by removing the oldest view.					
                    mruSources.RemoveRecentDataSourceRow(GetOldestRecentDataSource(mruSources));
                }
                mruSources.AddRecentDataSourceRow(databaseName, connectionString, DateTime.Now, 0, dataProvider, current.ParentRowRecentDataSources);
            }
            Save();
        }

        /// <summary>
        /// Loads Config information from the XML file.
        /// </summary>
        public static void Load(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            Config configDataSet = new Config();
            string configFilePath = filePath;

            try
            {

                // prevent this method from being called concurrently 
                System.Threading.Monitor.Enter(syncLock);

                StopWatchingForChanges();

                if (!File.Exists(configFilePath))
                {
                    //throw new FileNotFoundException("Configuration file not found.", configFilePath);
                }
                else
                {

                    XmlDocument doc = new XmlDocument();
                    doc.Load(configFilePath);

                    XmlNode versionNode = doc.DocumentElement.SelectSingleNode("/Config/Version");
                    int configFileVersion = int.Parse(versionNode.Attributes.GetNamedItem("ConfigVersion").Value);

                    // Validate configuration ...
                    if (configFileVersion == Configuration.CurrentSchemaVersion)
                    {
                        configDataSet = new Config();
                        configDataSet.ReadXml(configFilePath);
                        current = new Configuration(configFilePath, configDataSet);
                    }
                    else if (configFileVersion > Configuration.CurrentSchemaVersion)
                    {
                        throw new ConfigurationException(ConfigurationException.ConfigurationIssue.HigherVersion);
                    }
                    else
                    {
                        throw new ConfigurationException(ConfigurationException.ConfigurationIssue.LowerVersion);
                    }
                }
            }
            catch (ConfigurationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                current = null;
                throw new ConfigurationException(ConfigurationException.ConfigurationIssue.ContentsInvalid, ex);
            }
            finally
            {
                BeginWatchingForChanges();
                System.Threading.Monitor.Exit(syncLock);
            }
        }

        /// <summary>
        /// Handle a change event from the FileSystemWatcher.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        static void OnConfigChanged(object sender, FileSystemEventArgs e)
        {
            /*
            Load(e.FullPath);
            if (ConfigurationUpdated != null)
            {
                ConfigurationUpdated(current, EventArgs.Empty);
            }*/
        }

        /// <summary>
        /// Creates default configuration. Typically when Configuration structure is modified.
        /// </summary>
        /// <returns></returns>
        public static Configuration CreateDefaultConfiguration()
        {
            Config configDataSet = new Config();

            Config.DirectoriesRow drow = configDataSet.Directories.NewDirectoriesRow();

            // TODO: lookup temp folder??
            drow.Working = "C:\\Temp\\";
            // drow.Project = "C:\\Projects\\Epi Info 7\\";

            string programFilesDirectoryName = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles).ToLower();
            string installFolder = AppDomain.CurrentDomain.BaseDirectory;
            string writableFilesFolder = AppDomain.CurrentDomain.BaseDirectory;

            if (writableFilesFolder.ToLower().StartsWith(programFilesDirectoryName))
            {
                string desktop = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory).ToLower();
                string localUserFolder = string.Empty;
                int index = desktop.LastIndexOf('\\');

                if (index > 0 && !(desktop.StartsWith("\\")))
                {
                    localUserFolder = desktop.Substring(0, index);
                }

                if (Directory.Exists(localUserFolder))
                {
                    writableFilesFolder = localUserFolder + "\\Epi Info 7\\";
                }
                else
                {
                    writableFilesFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\Epi Info 7\\";
                }
            }

            string configFolder = Path.Combine(writableFilesFolder, "Configuration\\");

            if (!Directory.Exists(configFolder))
            {
                Directory.CreateDirectory(configFolder);
            }

            drow.Configuration = configFolder;
            drow.Output = Path.Combine(writableFilesFolder, "Output\\");
            drow.Templates = Path.Combine(writableFilesFolder, "Templates\\");
            drow.Samples = Path.Combine(writableFilesFolder, "Resources\\Samples\\");
            drow.Project = Path.Combine(writableFilesFolder, "Projects\\");
            drow.LogDir = Path.Combine(writableFilesFolder, "Logs\\");
            configDataSet.Directories.Rows.Add(drow);

            if (!Directory.Exists(drow.Output) && Directory.Exists(Path.Combine(installFolder, "Output\\")))
            {
                Directory.CreateDirectory(drow.Output);
                DirectoryInfo sourceInfo = new DirectoryInfo(Path.Combine(installFolder, "Output\\"));
                DirectoryInfo targetInfo = new DirectoryInfo(drow.Output);
                Util.CopyDirectory(sourceInfo, targetInfo);
            }
            if (!Directory.Exists(drow.Templates) && Directory.Exists(Path.Combine(installFolder, "Templates\\")))
            {
                Directory.CreateDirectory(drow.Templates);
                DirectoryInfo sourceInfo = new DirectoryInfo(Path.Combine(installFolder, "Templates\\"));
                DirectoryInfo targetInfo = new DirectoryInfo(drow.Templates);
                Util.CopyDirectory(sourceInfo, targetInfo);
            }
            //if (!Directory.Exists(drow.Samples))
            //{
            //    Directory.CreateDirectory(drow.Samples);
            //    DirectoryInfo sourceInfo = new DirectoryInfo(Path.Combine(installFolder, "Resources\\Samples\\"));
            //    DirectoryInfo targetInfo = new DirectoryInfo(drow.Samples);
            //    Util.CopyDirectory(sourceInfo, targetInfo);
            //}
            if (!Directory.Exists(drow.Project) && Directory.Exists(Path.Combine(installFolder, "Projects\\")))
            {
                Directory.CreateDirectory(drow.Project);
                DirectoryInfo sourceInfo = new DirectoryInfo(Path.Combine(installFolder, "Projects\\"));
                DirectoryInfo targetInfo = new DirectoryInfo(drow.Project);
                Util.CopyDirectory(sourceInfo, targetInfo);
            }
            if (!Directory.Exists(drow.LogDir) && Directory.Exists(Path.Combine(installFolder, "Logs\\")))
            {
                Directory.CreateDirectory(drow.LogDir);
                DirectoryInfo sourceInfo = new DirectoryInfo(Path.Combine(installFolder, "Logs\\"));
                DirectoryInfo targetInfo = new DirectoryInfo(drow.LogDir);
                Util.CopyDirectory(sourceInfo, targetInfo);
            }            

            Config.VersionRow vrow = configDataSet.Version.NewVersionRow();
            vrow.EpiInfoVersion = typeof(Configuration).Assembly.GetName().Version.Major;
            vrow.ConfigVersion = CurrentSchemaVersion;
            configDataSet.Version.Rows.Add(vrow);
            
            Config.RecentViewRow recentviewrow1 = configDataSet.RecentView.NewRecentViewRow();
            recentviewrow1.Name = drow.Project + "Mumps\\Mumps.prj:Survey";
            recentviewrow1.Location = drow.Project + "Mumps\\Mumps.prj:Survey";
            recentviewrow1.LastAccessed = DateTime.Now;
            configDataSet.RecentView.Rows.Add(recentviewrow1);
          
            Config.RecentViewRow recentviewrow2 = configDataSet.RecentView.NewRecentViewRow();
            recentviewrow2.Name = drow.Project + "HIV\\HIV.prj:Case";
            recentviewrow2.Location = drow.Project + "HIV\\HIV.prj:Case";
            recentviewrow2.LastAccessed = DateTime.Now;
            configDataSet.RecentView.Rows.Add(recentviewrow2);

            Config.RecentViewRow recentviewrow3 = configDataSet.RecentView.NewRecentViewRow();
            recentviewrow3.Name = drow.Project + "EColi\\EColi.prj:FoodHistory";
            recentviewrow3.Location = drow.Project + "EColi\\EColi.prj:FoodHistory";
            recentviewrow3.LastAccessed = DateTime.Now;
            configDataSet.RecentView.Rows.Add(recentviewrow3);           

            Config.RecentProjectRow recentprojectrow = configDataSet.RecentProject.NewRecentProjectRow();
            recentprojectrow.Name = "Sample";
            recentprojectrow.Location = drow.Project + "Sample\\Sample.prj";
            recentprojectrow.LastAccessed = DateTime.Now;
            configDataSet.RecentProject.Rows.Add(recentprojectrow);            

            Config.SettingsRow row = configDataSet.Settings.NewSettingsRow();
            row.BackgroundImage = string.Empty;
            row.MRUProjectsCount = 4;
            row.MRUViewsCount = 4;
            row.MRUDataSourcesCount = 5;
            row.Language = "en-US";
            row.Republish_IsRepbulishable = true;
            row.Republish_RequireSecurityKey = false;
            row.RepresentationOfYes = "Yes";
            row.RepresentationOfNo = "No";
            row.RepresentationOfMissing = "Missing";
            row.DateFormat = @"MM-DD-YYYY";
            row.TimeFormat = @"HH:MM:SS AMPM";
            row.DateTimeFormat = @"MM-DD-YYYY HH:MM:SS AMPM";
            row.StatisticsLevel = 4;
            row.RecordProcessingScope = 1;
            row.ShowCompletePrompt = true;
            row.ShowSelection = true;
            row.ShowGraphics = true;
            row.ShowPercents = true;
            row.ShowHyperlinks = true;
            row.ShowTables = true;
            row.IncludeMissingValues = false;
            row.SnapToGrid = true;
            row.EditorFontName = "Segoe UI";
            row.EditorFontSize = 10M;
            row.ControlFontName = "Segoe UI";
            row.ControlFontSize = 10M;
            row.ShowStatusBar = true;
            row.PrecisionForStatistics = 2;
            row.MapServiceKey = "Aua5s8kFcEZMx5lsd8Vkerz3frboU1CwzvOyzX_vgSnzsnbqV7xlQ4WTRUlN19_Q";
            row.LastAlertDate = new DateTime(2011, 9, 23);
            row.WebServiceAuthMode = 0; // 0 = Anon, 1 = NT
            row.EWEServiceAuthMode = 0; // 0 = Anon, 1 = NT
            row.WebServiceEndpointAddress = string.Empty;
            row.EWEServiceEndpointAddress = string.Empty;
            row.WebServiceBindingMode = "BASIC";
            row.WebServiceMaxBufferPoolSize = 524288;
            row.WebServiceMaxReceivedMessageSize = 999999999;
            row.WebServiceReaderMaxDepth = 32;
            row.WebServiceReaderMaxStringContentLength = 2048000;
            row.WebServiceReaderMaxArrayLength = 16384;
            row.WebServiceReaderMaxBytesPerRead = 4096;
            row.WebServiceReaderMaxNameTableCharCount = 16384;
            row.EWEServiceBindingMode = "BASIC";
            row.EWEServiceMaxBufferPoolSize = 524288;
            row.EWEServiceMaxReceivedMessageSize = 999999999;
            row.EWEServiceReaderMaxDepth = 32;
            row.EWEServiceReaderMaxStringContentLength = 2048000;
            row.EWEServiceReaderMaxArrayLength = 16384;
            row.EWEServiceReaderMaxBytesPerRead = 4096;
            row.EWEServiceReaderMaxNameTableCharCount = 16384;
            row.DashboardFrequencyRowLimit = 200;
            row.DashboardFrequencyStrataLimit = 100;
            row.DashboardFrequencyCrosstabLimit = 100;
            row.DashboardCombinedFrequencyRowLimit = 250;
            row.DashboardAberrationRowLimit = 366;
            row.DashboardLineListRowLimit = 2000;


            #if LINUX_BUILD
            row.DefaultDataDriver = MySQLDriver;
            #else
            row.DefaultDataDriver = AccessDriver;
            #endif

            row.DefaultDataFormatForRead = 3;
            row.FrameworkTcpPort = 11532;
            row.IsVariableValidationEnable = false;
            row.AutoTouchKeyboard = false;
            row.SparseConnection = false;
            configDataSet.Settings.Rows.Add(row);

            // Data Drivers
            Config.DataDriversRow parentDataDriversRow = configDataSet.DataDrivers.NewDataDriversRow();
            configDataSet.DataDrivers.Rows.Add(parentDataDriversRow);

            Config.DataDriverRow dataDriverRowEpiWeb = configDataSet.DataDriver.NewDataDriverRow();
            dataDriverRowEpiWeb.DataDriversRow = parentDataDriversRow;
            dataDriverRowEpiWeb.DisplayName = "Epi Info Web & Cloud Services";
            dataDriverRowEpiWeb.Type = EpiInfoWebDriver;
            dataDriverRowEpiWeb.DataProvider = true;
            dataDriverRowEpiWeb.MetadataProvider = false;
            configDataSet.DataDriver.Rows.Add(dataDriverRowEpiWeb);

            // Access driver
            Config.DataDriverRow dataDriverRow1 = configDataSet.DataDriver.NewDataDriverRow();
            dataDriverRow1.DataDriversRow = parentDataDriversRow;
            dataDriverRow1.DisplayName = "Microsoft Access 2002-2003 (.mdb)";
            dataDriverRow1.Type = AccessDriver; 
            dataDriverRow1.DataProvider = true;
            dataDriverRow1.MetadataProvider = true;
            configDataSet.DataDriver.Rows.Add(dataDriverRow1);

            Config.DataDriverRow dataDriverRow3 = configDataSet.DataDriver.NewDataDriverRow();
            dataDriverRow3.DataDriversRow = parentDataDriversRow;
            dataDriverRow3.DisplayName = "Microsoft Access 2007 (.accdb)";
            dataDriverRow3.Type = Access2007Driver;
            dataDriverRow3.DataProvider = true;
            dataDriverRow3.MetadataProvider = false;
            configDataSet.DataDriver.Rows.Add(dataDriverRow3);

            // Excel driver
            Config.DataDriverRow dataDriverRowExcel = configDataSet.DataDriver.NewDataDriverRow();
            dataDriverRowExcel.DataDriversRow = parentDataDriversRow;
            // Excel Workbook (*.xlsx)
            // Microsoft Excel 5.0/95 Workbook (*.xls)
            // Excel 97 - Excel 2003 Workbook (*.xls)
            dataDriverRowExcel.DisplayName = "Microsoft Excel 97-2003 Workbook (.xls)";
            dataDriverRowExcel.Type = ExcelDriver;
            dataDriverRowExcel.DataProvider = true;
            dataDriverRowExcel.MetadataProvider = false;
            configDataSet.DataDriver.Rows.Add(dataDriverRowExcel);

            Config.DataDriverRow dataDriverRowExcel2007 = configDataSet.DataDriver.NewDataDriverRow();
            dataDriverRowExcel2007.DataDriversRow = parentDataDriversRow;
            dataDriverRowExcel2007.DisplayName = "Microsoft Excel 2007 Workbook (.xlsx)";
            dataDriverRowExcel2007.Type = Excel2007Driver;
            dataDriverRowExcel2007.DataProvider = true;
            dataDriverRowExcel2007.MetadataProvider = false;
            configDataSet.DataDriver.Rows.Add(dataDriverRowExcel2007);

            // SQL Server driver
            Config.DataDriverRow dataDriverRow2 = configDataSet.DataDriver.NewDataDriverRow();
            dataDriverRow2.DataDriversRow = parentDataDriversRow;
            dataDriverRow2.DisplayName = "Microsoft SQL Server Database";
            dataDriverRow2.Type = SqlDriver;
            dataDriverRow2.DataProvider = true;
            dataDriverRow2.MetadataProvider = true;
            configDataSet.DataDriver.Rows.Add(dataDriverRow2);

            Config.DataDriverRow dataDriverRowCsv = configDataSet.DataDriver.NewDataDriverRow();
            dataDriverRowCsv.DataDriversRow = parentDataDriversRow;
            dataDriverRowCsv.DisplayName = "CSV File";
            dataDriverRowCsv.Type = CsvDriver;
            dataDriverRowCsv.DataProvider = true;
            dataDriverRowCsv.MetadataProvider = false;
            configDataSet.DataDriver.Rows.Add(dataDriverRowCsv);

            Config.DataDriverRow dataDriverRowMySql = configDataSet.DataDriver.NewDataDriverRow();
            dataDriverRowMySql.DataDriversRow = parentDataDriversRow;
            dataDriverRowMySql.DisplayName = "MySQL Database";
            dataDriverRowMySql.Type = MySQLDriver;
            dataDriverRowMySql.DataProvider = true;
            dataDriverRowMySql.MetadataProvider = false;
            configDataSet.DataDriver.Rows.Add(dataDriverRowMySql);

            Config.DataDriverRow dataDriverRowMongoDB = configDataSet.DataDriver.NewDataDriverRow();
            dataDriverRowMongoDB.DataDriversRow = parentDataDriversRow;
            dataDriverRowMongoDB.DisplayName = "MongoDB Database";
            dataDriverRowMongoDB.Type = MongoDBDriver;
            dataDriverRowMongoDB.DataProvider = true;
            dataDriverRowMongoDB.MetadataProvider = false;
            configDataSet.DataDriver.Rows.Add(dataDriverRowMongoDB);

            Config.DataDriverRow dataDriverRowPostgreSql = configDataSet.DataDriver.NewDataDriverRow();
            dataDriverRowPostgreSql.DataDriversRow = parentDataDriversRow;
            dataDriverRowPostgreSql.DisplayName = "PostgreSQL Database";
            dataDriverRowPostgreSql.Type = PostgreSQLDriver;
            dataDriverRowPostgreSql.DataProvider = true;
            dataDriverRowPostgreSql.MetadataProvider = false;
            configDataSet.DataDriver.Rows.Add(dataDriverRowPostgreSql);

            //// MySQLDriver Driver
            //Config.DataDriverRow dataDriverRow3 = configDataSet.DataDriver.NewDataDriverRow();
            //dataDriverRow3.DataDriversRow = parentDataDriversRow;
            //dataDriverRow3.DisplayName = "MySQL Database";
            //dataDriverRow3.Type = MySQLDriver;
            //configDataSet.DataDriver.Rows.Add(dataDriverRow3);

            /*
             * KKM4: Disabled Excel drivers for the time being.
            dataDriverRow = configDataSet.DataDriver.NewDataDriverRow();
            dataDriverRow.DataDriversRow = parentDataDriversRow;
            dataDriverRow.DisplayName = "Microsoft Excel Spreadsheet";
            dataDriverRow.Type = "Epi.Data.Office.ExcelWorkbook, Epi.Data.Office";
            configDataSet.DataDriver.Rows.Add(dataDriverRow);
             */

            // Connections
            Config.ConnectionsRow connectionsRow = configDataSet.Connections.NewConnectionsRow();
            configDataSet.Connections.Rows.Add(connectionsRow);

            Config.FileRow fileRow;

            fileRow = configDataSet.File.NewFileRow();
            fileRow.ConnectionsRow = connectionsRow;
            fileRow.DataDriver = AccessDriver; // "Epi.Data.Office.AccessDatabase, Epi.Data.Office";
            fileRow.Extension = ".mdb";
            configDataSet.File.Rows.Add(fileRow);

            fileRow = configDataSet.File.NewFileRow();
            fileRow.ConnectionsRow = connectionsRow;
            fileRow.DataDriver = AccessDriver; // "Epi.Data.Office.AccessDatabase, Epi.Data.Office";
            fileRow.Extension = ".accdb";
            configDataSet.File.Rows.Add(fileRow);

            fileRow = configDataSet.File.NewFileRow();
            fileRow.ConnectionsRow = connectionsRow;
            fileRow.DataDriver = ExcelDriver; // "Epi.Data.Office.ExcelWorkbook, Epi.Data.Office";
            fileRow.Extension = ".xls";
            configDataSet.File.Rows.Add(fileRow);

            fileRow = configDataSet.File.NewFileRow();
            fileRow.ConnectionsRow = connectionsRow;
            fileRow.DataDriver = ExcelDriver; // "Epi.Data.Office.ExcelWorkbook, Epi.Data.Office";
            fileRow.Extension = ".xlsx";
            configDataSet.File.Rows.Add(fileRow);

            Config.DatabaseRow dbRow;

            // AppData MDB is not required any more.
            //dbRow = configDataSet.Database.NewDatabaseRow();
            //dbRow.ConnectionsRow = connectionsRow;
            //dbRow.Name = Epi.Data.DbDriverFactoryCreator.KnownDatabaseNames.AppData;
            //dbRow.DataDriver = AccessDriver; // "Epi.Data.Office.AccessDatabase, Epi.Data.Office";
            //dbRow.ConnectionString = Path.Combine(Path.Combine(installFolder, "Resources\\AppData\\"), "AppData.mdb");
            //configDataSet.Database.Rows.Add(dbRow);

            //PHIN VS database
            dbRow = configDataSet.Database.NewDatabaseRow();
            dbRow.ConnectionsRow = connectionsRow;
            dbRow.Name = Epi.Data.DbDriverFactoryCreator.KnownDatabaseNames.Phin;
            dbRow.DataDriver = AccessDriver; // "Epi.Data.Office.AccessDatabase, Epi.Data.Office";
            dbRow.ConnectionString = Path.Combine(Path.Combine(writableFilesFolder, "Resources\\PHIN\\"), "PHINVS.mdb");
            configDataSet.Database.Rows.Add(dbRow);

            if (!Directory.Exists(Path.Combine(writableFilesFolder, "Resources\\PHIN\\")))
            {
                Directory.CreateDirectory(Path.Combine(writableFilesFolder, "Resources\\PHIN\\"));
                DirectoryInfo sourceInfo = new DirectoryInfo(Path.Combine(installFolder, "Resources\\PHIN\\"));
                DirectoryInfo targetInfo = new DirectoryInfo(Path.Combine(writableFilesFolder, "Resources\\PHIN\\"));
                Util.CopyDirectory(sourceInfo, targetInfo);
            }

            // Translation database
            dbRow = configDataSet.Database.NewDatabaseRow();
            dbRow.ConnectionsRow = connectionsRow;
            dbRow.Name = Epi.Data.DbDriverFactoryCreator.KnownDatabaseNames.Translation;
            dbRow.DataDriver = AccessDriver; // "Epi.Data.Office.AccessDatabase, Epi.Data.Office";
            dbRow.ConnectionString = Path.Combine(configFolder, "Translations.mdb");
            configDataSet.Database.Rows.Add(dbRow);

            Config.ModulesRow parentModulesRow = configDataSet.Modules.NewModulesRow();
            configDataSet.Modules.Rows.Add(parentModulesRow);

            Config.ModuleRow moduleRow;

            moduleRow = configDataSet.Module.NewModuleRow();
            moduleRow.ModulesRow = parentModulesRow;
            moduleRow.Name = "MENU";
            moduleRow.Type = "Epi.Windows.Menu.MenuWindowsModule, Menu";
            configDataSet.Module.Rows.Add(moduleRow);

            moduleRow = configDataSet.Module.NewModuleRow();
            moduleRow.ModulesRow = parentModulesRow;
            moduleRow.Name = "MAKEVIEW";
            moduleRow.Type = "Epi.Windows.MakeView.MakeViewWindowsModule, MakeView";
            configDataSet.Module.Rows.Add(moduleRow);

            moduleRow = configDataSet.Module.NewModuleRow();
            moduleRow.ModulesRow = parentModulesRow;
            moduleRow.Name = "ENTER";
            moduleRow.Type = "Epi.Windows.Enter.EnterWindowsModule, Enter";
            configDataSet.Module.Rows.Add(moduleRow);

            moduleRow = configDataSet.Module.NewModuleRow();
            moduleRow.ModulesRow = parentModulesRow;
            moduleRow.Name = "ANALYSIS";
            moduleRow.Type = "Epi.Windows.Analysis.AnalysisWindowsModule, Analysis";
            configDataSet.Module.Rows.Add(moduleRow);

            moduleRow = configDataSet.Module.NewModuleRow();
            moduleRow.ModulesRow = parentModulesRow;
            moduleRow.Name = "EPIMAP";
            moduleRow.Type = "Epi.Windows.EpiMap.EpiMapWindowsModule, EpiMap";
            configDataSet.Module.Rows.Add(moduleRow);

            moduleRow = configDataSet.Module.NewModuleRow();
            moduleRow.ModulesRow = parentModulesRow;
            moduleRow.Name = "EPIREPORT";
            moduleRow.Type = "Epi.Windows.EpiReport.EpiReportWindowsModule, EpiReport";
            configDataSet.Module.Rows.Add(moduleRow);

            moduleRow = configDataSet.Module.NewModuleRow();
            moduleRow.ModulesRow = parentModulesRow;
            moduleRow.Name = "Localization";
            moduleRow.Type = "Epi.Windows.Localization.LocalizationWindowsModule, TSetup";
            configDataSet.Module.Rows.Add(moduleRow);
            
            return new Configuration(DefaultConfigurationPath, configDataSet);
        }

        private static void BeginWatchingForChanges()
        {
            StopWatchingForChanges();

            if (current == null) return;

            FileInfo configFile = new FileInfo(current.ConfigFilePath);
            watcher = new FileSystemWatcher(configFile.DirectoryName);
            watcher.Filter = configFile.Name;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += configUpdateCallback;
            watcher.EnableRaisingEvents = true;
        }
        private static void StopWatchingForChanges()
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Changed -= configUpdateCallback;
                watcher = null;
            }
        }

        private static Config.RecentProjectRow GetOldestRecentProject(Config.RecentProjectDataTable mruProjects)
        {

            Config.RecentProjectRow oldestProject = null;
            if (mruProjects.Count > 0)
            {
                oldestProject = mruProjects[0];
                for (int index = 1; index < mruProjects.Count; index++)
                {
                    Config.RecentProjectRow thisProject = mruProjects[index];
                    if (thisProject.LastAccessed < oldestProject.LastAccessed)
                    {
                        oldestProject = thisProject;
                    }
                }
            }
            return oldestProject;
        }

        private static Config.RecentViewRow GetOldestRecentView(Config.RecentViewDataTable mruViews)
        {
            Config.RecentViewRow oldestView = null;

            if (mruViews.Count > 0)
            {
                oldestView = mruViews[0];
                for (int index = 1; index < mruViews.Count; index++)
                {
                    Config.RecentViewRow thisView = mruViews[index];
                    if (thisView.LastAccessed < oldestView.LastAccessed)
                    {
                        oldestView = thisView;
                    }
                }
            }
            return oldestView;
        }

        private static Config.RecentDataSourceRow GetOldestRecentDataSource(Config.RecentDataSourceDataTable mruSources)
        {
            Config.RecentDataSourceRow oldestSource = null;

            if (mruSources.Count > 0)
            {
                oldestSource = mruSources[0];
                for (int index = 1; index < mruSources.Count; index++)
                {
                    Config.RecentDataSourceRow thisSource = mruSources[index];
                    if (thisSource.LastAccessed < oldestSource.LastAccessed)
                    {
                        oldestSource = thisSource;
                    }
                }
            }
            return oldestSource;
        }

        /// <summary>
        /// Returns the default path to the configuration file
        /// </summary>
        /// <returns>string</returns>
        public static string DefaultConfigurationPath
        {
            get
            {
                string programFilesDirectoryName = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles).ToLower();
                string baseFolder = AppDomain.CurrentDomain.BaseDirectory.ToLower();

                if (baseFolder.ToLower().StartsWith(programFilesDirectoryName))
                {
                    string desktop = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory).ToLower();
                    string localUserFolder = string.Empty;
                    int index = desktop.LastIndexOf('\\');

                    if (index > 0 && !(desktop.StartsWith("\\")))
                    {
                        localUserFolder = desktop.Substring(0, index);
                    }

                    if (Directory.Exists(localUserFolder))
                    {
                        baseFolder = localUserFolder + "\\Epi Info 7\\";
                    }
                    else
                    {
                        baseFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\Epi Info 7\\";
                    }
                }
                else
                {
                    baseFolder = typeof(Configuration).Assembly.Location;
                }

                string assemblyPath = Path.GetDirectoryName(baseFolder);
                string defaultPath = Path.Combine(assemblyPath, "Configuration\\EpiInfo.Config.xml");
                return defaultPath;
            }
        }

        /// <summary>
        /// All public methods should assert that configuration has been loaded. 
        /// Method throws an exception if configuration is not loaded.
        /// </summary>
        private static void AssertConfigurationLoaded()
        {
            if (current == null)
            {
                throw new GeneralException(SharedStrings.CONFIG_NOT_LOADED);
            }
        }

        /// <summary>
        /// Internally save changes
        /// </summary>
        private static void Save()
        {
            AssertConfigurationLoaded();
            Save(current);
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <param name="instance">Instance</param>
        public static void Save(Configuration instance)
        {
            try
            {
                StopWatchingForChanges();
                System.Threading.Monitor.Enter(syncLock);

                instance.configDataSet.WriteXml(instance.ConfigFilePath);

                // if the current configuration is being saved, reload 
                if (current != null && instance.configFilePath == current.configFilePath)
                {
                    Load(instance.configFilePath);
                }
            }
            catch (System.UnauthorizedAccessException uaex)
            {
                throw new ConfigurationException(ConfigurationException.ConfigurationIssue.AccessDenied, uaex);
            }
            catch (System.Security.SecurityException sex)
            {
                throw new ConfigurationException(ConfigurationException.ConfigurationIssue.AccessDenied, sex);
            }
            catch (System.IO.IOException ioex)
            {
                throw new ConfigurationException(ConfigurationException.ConfigurationIssue.AccessDenied, ioex);
            }
            finally
            {
                System.Threading.Monitor.Exit(syncLock);
                BeginWatchingForChanges();
            }
        }

        /// <summary>
        /// Encryption
        /// </summary>
        /// <param name="plainText">The plaintext to encrypt</param>
        /// <returns>The ciphertext</returns>
        public static string Encrypt(string plainText)
        {
            Configuration config = Configuration.GetNewInstance();
            if (config.TextEncryptionModule != null && !string.IsNullOrEmpty(config.TextEncryptionModule.FileName))
            {
                Assembly assemblyInstance = Assembly.LoadFrom(@config.TextEncryptionModule.FileName);
                Type[] types = assemblyInstance.GetTypes();
                foreach (Type t in types)
                {
                    MethodInfo mi = t.GetMethod("Encrypt");
                    if (mi != null)
                    {
                        string typeName = t.FullName;
                        object lateBoundObj = Activator.CreateInstance(t);

                        object[] parameter = new object[1];
                        parameter[0] = plainText;

                        object str = t.InvokeMember("Encrypt", BindingFlags.Default | BindingFlags.InvokeMethod, null, lateBoundObj, parameter);
                        return str.ToString();
                    }
                }
                throw new ApplicationException(SharedStrings.MAP_ERROR_CRYPTOGRAPHY);
            }
            else
            {
                byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
                byte[] saltValueBytes = Encoding.ASCII.GetBytes(saltValue);
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, saltValueBytes, "MD5", 1);
                byte[] keyBytes = password.GetBytes(16);
                RijndaelManaged symmetricKey = new RijndaelManaged();
                symmetricKey.Mode = CipherMode.CBC;
                ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                cryptoStream.FlushFinalBlock();
                byte[] cipherTextBytes = memoryStream.ToArray();
                memoryStream.Close();
                cryptoStream.Close();
                string cipherText = Convert.ToBase64String(cipherTextBytes);
                return cipherText;
            }
        }  

        /// <summary>
        /// Decryption
        /// </summary>
        /// <param name="cipherText">The ciphertext to decrypt</param>
        /// <returns>The plaintext</returns>
        public static string Decrypt(string cipherText)
        {
            Configuration config = Configuration.GetNewInstance();
            if (config.TextEncryptionModule != null && !string.IsNullOrEmpty(config.TextEncryptionModule.FileName))
            {
                Assembly assemblyInstance = Assembly.LoadFrom(@config.TextEncryptionModule.FileName);
                Type[] types = assemblyInstance.GetTypes();
                foreach (Type t in types)
                {
                    MethodInfo mi = t.GetMethod("Decrypt");
                    if (mi != null)
                    {
                        string typeName = t.FullName;
                        object lateBoundObj = Activator.CreateInstance(t);

                        object[] parameter = new object[1];
                        parameter[0] = cipherText;

                        object str = t.InvokeMember("Decrypt", BindingFlags.Default | BindingFlags.InvokeMethod, null, lateBoundObj, parameter);
                        return str.ToString();
                    }
                }
                throw new ApplicationException(SharedStrings.MAP_ERROR_CRYPTOGRAPHY);
            }
            else
            {
                try
                {
                    byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
                    byte[] saltValueBytes = Encoding.ASCII.GetBytes(saltValue);
                    byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
                    PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, saltValueBytes, "MD5", 1);
                    byte[] keyBytes = password.GetBytes(16);
                    RijndaelManaged symmetricKey = new RijndaelManaged();
                    symmetricKey.Mode = CipherMode.CBC;
                    ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
                    string plainText = string.Empty;
                    MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                        int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                        memoryStream.Close();
                        plainText = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                    }
                    return plainText;
                }
                catch
                {
                    byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVectorDebug);
                    byte[] saltValueBytes = Encoding.ASCII.GetBytes(saltValueDebug);
                    byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
                    PasswordDeriveBytes password = new PasswordDeriveBytes(passPhraseDebug, saltValueBytes, "MD5", 1);
                    byte[] keyBytes = password.GetBytes(16);
                    RijndaelManaged symmetricKey = new RijndaelManaged();
                    symmetricKey.Mode = CipherMode.CBC;
                    ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
                    string plainText = string.Empty;
                    MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                        int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                        memoryStream.Close();
                        plainText = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                    }
                    return plainText;
                }
            }
        }

        /// <summary>
        /// Decrypts a BASE64 encoded string of encrypted data, returns a plain string
        /// </summary>
        /// <param name="base64StringToDecrypt">an Aes encrypted AND base64 encoded string</param>
        /// <param name="passphrase">The passphrase.</param>
        /// <returns>returns a plain string</returns>
        public static string DecryptString(string base64StringToDecrypt, string passphrase)
        {
            //Set up the encryption objects
            using (AesCryptoServiceProvider acsp = GetProvider(Encoding.Default.GetBytes(passphrase)))
            {
               
                //base64StringToDecrypt = base64StringToDecrypt.Trim().Replace(" ", "+");
                //if (base64StringToDecrypt.Length % 4 > 0)
                //    base64StringToDecrypt = base64StringToDecrypt.PadRight(base64StringToDecrypt.Length + 4 - base64StringToDecrypt.Length % 4, '=');

                byte[] RawBytes = Convert.FromBase64String(base64StringToDecrypt);
//                RawBytes = Encoding.Unicode.GetBytes(base64StringToDecrypt);
//                byte[] RawBytes = (Encoding.Default.GetBytes(base64StringToDecrypt));
                ICryptoTransform ictD = acsp.CreateDecryptor();
                
                //RawBytes now contains original byte array, still in Encrypted state

                //Decrypt into stream
                MemoryStream msD = new MemoryStream(RawBytes, 0, RawBytes.Length);
                CryptoStream csD = new CryptoStream(msD, ictD, CryptoStreamMode.Read);
                //csD now contains original byte array, fully decrypted

                //int count = 0;
                //while (csD.ReadByte() != -1)
                //{
                //    count++;
                //    //System.Diagnostics.Debug.Print(csD.
                //}
                //System.Diagnostics.Debug.Print(

                //return the content of msD as a regular string
//                string jrc = (new StreamReader(csD)).ReadToEnd();
                return (new StreamReader(csD)).ReadToEnd();
            }
        }

        private static AesCryptoServiceProvider GetProvider(byte[] key)
        {
            AesCryptoServiceProvider result = new AesCryptoServiceProvider();
            result.BlockSize = 128;
            result.KeySize = 128;
            result.Mode = CipherMode.CBC;
            result.Padding = PaddingMode.PKCS7;

            result.GenerateIV();
            result.IV = new byte[] { 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48 };
            string ivString = "00000000";
            byte[] bytes = new byte[ivString.Length * sizeof(char)];
            System.Buffer.BlockCopy(ivString.ToCharArray(), 0, bytes, 0, bytes.Length);
//            result.IV = bytes;

            byte[] RealKey = GetKey(key, result);
            result.Key = RealKey;
            // result.IV = RealKey;
            return result;
        }

        private static byte[] GetKey(byte[] suggestedKey, SymmetricAlgorithm p)
        {
            byte[] kRaw = suggestedKey;
            List<byte> kList = new List<byte>();

            for (int i = 0; i < p.LegalKeySizes[0].MinSize; i += 8)
            {
                kList.Add(kRaw[(i / 8) % kRaw.Length]);
            }
            byte[] k = kList.ToArray();
            return k;
        }

        public static string DecryptJava(string cipherText, string password)
        {
            if (cipherText.Substring(0,5).Equals("APPLE"))
                return DecryptString(cipherText.Substring(5), password);
            

            int _keyLengthInBits = 128;

            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

            AesManaged aes = new AesManaged();
            aes = new AesManaged();

            aes.KeySize = _keyLengthInBits;
            aes.BlockSize = 128;

            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.IV = HexStringToByteArray(initVectorDroid);
            aes.Key = new System.Security.Cryptography.Rfc2898DeriveBytes(password, HexStringToByteArray(saltDroid), 1000).GetBytes(_keyLengthInBits / 8);

            ICryptoTransform transform = aes.CreateDecryptor();
            byte[] plainTextBytes = transform.TransformFinalBlock(cipherTextBytes, 0, cipherTextBytes.Length);

            return System.Text.Encoding.UTF8.GetString(plainTextBytes);
        }

        private static byte[] HexStringToByteArray(string s)
        {
            var r = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
            {
                r[i / 2] = (byte)Convert.ToInt32(s.Substring(i, 2), 16);
            }
            return r;
        }

        /// <summary>
        /// File Encryption
        /// </summary>
        /// <param name="inputFileName">The file to encrypt</param>
        /// <param name="outputFileName">The resulting encrypted file</param>
        /// <param name="password">The password</param>
        public static void EncryptFile(string inputFileName, string outputFileName, string password, string customInitVector = "", string customSalt = "", int iterations = 4)
        {
            Configuration config = Configuration.GetNewInstance();

            if (config.FileEncryptionModule != null && !string.IsNullOrEmpty(config.FileEncryptionModule.FileName))
            {
                Assembly assemblyInstance = Assembly.LoadFrom(@config.FileEncryptionModule.FileName);
                Type[] types = assemblyInstance.GetTypes();
                foreach (Type t in types)
                {
                    MethodInfo mi = t.GetMethod("EncryptFile");
                    if (mi != null)
                    {
                        string typeName = t.FullName;
                        object lateBoundObj = Activator.CreateInstance(t);

                        object[] parameter = new object[6];
                        parameter[0] = inputFileName;
                        parameter[1] = outputFileName;
                        parameter[2] = password;
                        parameter[3] = customInitVector;
                        parameter[4] = customSalt;
                        parameter[5] = iterations;

                        t.InvokeMember("EncryptFile", BindingFlags.Default | BindingFlags.InvokeMethod, null, lateBoundObj, parameter);
                        break;
                    }
                }
            }
            else
            {
                #region DEFAULT_ENCRYPT
                using(FileStream fsInput = new FileStream(inputFileName,
                    FileMode.Open,
                    FileAccess.Read)) 
                    {

                        using (FileStream fsEncrypted = new FileStream(outputFileName,
                                        FileMode.Create,
                                        FileAccess.Write))
                        {

                            string saltStr = string.Empty;
                            if (customSalt.Length == 32)
                            {
                                saltStr = customSalt;
                            }
                            else
                            {
                                saltStr = Configuration.saltValueAlt;
                            }

                            string initVectorStr = string.Empty;
                            if (customInitVector.Length == 16)
                            {
                                initVectorStr = customInitVector;
                            }
                            else
                            {
                                initVectorStr = Configuration.initVectorAlt;
                            }

                            byte[] salt = Encoding.ASCII.GetBytes(saltStr);
                            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, salt, iterations);
                            RijndaelManaged SymmetricKey = new RijndaelManaged();
                            SymmetricKey.Padding = PaddingMode.PKCS7;
                            SymmetricKey.KeySize = 256;
                            SymmetricKey.Mode = CipherMode.CBC;
                            SymmetricKey.Key = key.GetBytes(SymmetricKey.KeySize / 8);

                            GCHandle gch = GCHandle.Alloc(SymmetricKey.Key, GCHandleType.Pinned);

                            SymmetricKey.IV = ASCIIEncoding.ASCII.GetBytes(initVectorStr);
                            SymmetricKey.BlockSize = 128;

                            initVectorStr = string.Empty;
                            saltStr = string.Empty;

                            ICryptoTransform aesEncrypt = SymmetricKey.CreateEncryptor(SymmetricKey.Key, SymmetricKey.IV);
                            using (CryptoStream cryptostream = new CryptoStream(fsEncrypted,
                                                aesEncrypt,
                                                CryptoStreamMode.Write))
                            {
                                byte[] bytearrayinput = new byte[fsInput.Length - 1];
                                fsInput.Read(bytearrayinput, 0, bytearrayinput.Length);
                                cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);
                            }

                            ZeroMemory(gch.AddrOfPinnedObject(), 32);
                            gch.Free();

                            SymmetricKey.Clear();
                        }
                }
                
                #endregion // DEFAULT_ENCRYPT
            }
        }

        /// <summary>
        /// File Encryption
        /// </summary>
        /// <param name="plaintext">The plaintext to encrypt</param>
        /// <param name="outputFileName">The resulting encrypted file</param>
        /// <param name="password">The password</param>
        public static void EncryptStringToFile(string plaintext, string outputFileName, string password, string customInitVector = "", string customSalt = "", int iterations = 4)
        {
            Configuration config = Configuration.GetNewInstance();

            if (config.FileEncryptionModule != null && !string.IsNullOrEmpty(config.FileEncryptionModule.FileName))
            {
                Assembly assemblyInstance = Assembly.LoadFrom(@config.FileEncryptionModule.FileName);
                Type[] types = assemblyInstance.GetTypes();
                foreach (Type t in types)
                {
                    MethodInfo mi = t.GetMethod("EncryptStringToFile");
                    if (mi != null)
                    {
                        string typeName = t.FullName;
                        object lateBoundObj = Activator.CreateInstance(t);

                        object[] parameter = new object[6];
                        parameter[0] = plaintext;
                        parameter[1] = outputFileName;
                        parameter[2] = password;
                        parameter[3] = customInitVector;
                        parameter[4] = customSalt;
                        parameter[5] = iterations;

                        t.InvokeMember("EncryptStringToFile", BindingFlags.Default | BindingFlags.InvokeMethod, null, lateBoundObj, parameter);
                        break;
                    }
                }
            }
            else
            {
                #region DEFAULT_ENCRYPT
                using (MemoryStream msInput = new 
                    MemoryStream(Encoding.ASCII.GetBytes(plaintext)))
                {

                    using (FileStream fsEncrypted = new FileStream(outputFileName,
                                    FileMode.Create,
                                    FileAccess.Write))
                    {

                        string saltStr = string.Empty;
                        if (customSalt.Length == 32)
                        {
                            saltStr = customSalt;
                        }
                        else
                        {
                            saltStr = Configuration.saltValueAlt;
                        }

                        string initVectorStr = string.Empty;
                        if (customInitVector.Length == 16)
                        {
                            initVectorStr = customInitVector;
                        }
                        else
                        {
                            initVectorStr = Configuration.initVectorAlt;
                        }

                        byte[] salt = Encoding.ASCII.GetBytes(saltStr);
                        Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, salt, iterations);
                        RijndaelManaged SymmetricKey = new RijndaelManaged();
                        SymmetricKey.Padding = PaddingMode.PKCS7;
                        SymmetricKey.KeySize = 256;
                        SymmetricKey.Mode = CipherMode.CBC;
                        SymmetricKey.Key = key.GetBytes(SymmetricKey.KeySize / 8);

                        GCHandle gch = GCHandle.Alloc(SymmetricKey.Key, GCHandleType.Pinned);

                        SymmetricKey.IV = ASCIIEncoding.ASCII.GetBytes(initVectorStr);
                        SymmetricKey.BlockSize = 128;

                        initVectorStr = string.Empty;
                        saltStr = string.Empty;

                        ICryptoTransform aesEncrypt = SymmetricKey.CreateEncryptor(SymmetricKey.Key, SymmetricKey.IV);
                        using (CryptoStream cryptostream = new CryptoStream(fsEncrypted,
                                            aesEncrypt,
                                            CryptoStreamMode.Write))
                        {
                            byte[] bytearrayinput = new byte[msInput.Length/* - 1*/];
                            msInput.Read(bytearrayinput, 0, bytearrayinput.Length);
                            cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);
                        }

                        ZeroMemory(gch.AddrOfPinnedObject(), 32);
                        gch.Free();

                        SymmetricKey.Clear();
                    }
                }

                #endregion // DEFAULT_ENCRYPT
            }
        }

        /// <summary>
        /// File Decryption
        /// </summary>
        /// <param name="inputFileName">The file to decrypt</param>
        /// <param name="outputFileName">The resulting plaintext file</param>
        /// <param name="password">The password</param>
        public static void DecryptFile(string inputFileName, string outputFileName, string password, string customInitVector = "", string customSalt = "", int iterations = 4)
        {
            Configuration config = Configuration.GetNewInstance();

            if (config.FileEncryptionModule != null && !string.IsNullOrEmpty(config.FileEncryptionModule.FileName))
            {
                Assembly assemblyInstance = Assembly.LoadFrom(@config.FileEncryptionModule.FileName);
                Type[] types = assemblyInstance.GetTypes();
                foreach (Type t in types)
                {
                    MethodInfo mi = t.GetMethod("DecryptFile");
                    if (mi != null)
                    {
                        string typeName = t.FullName;
                        object lateBoundObj = Activator.CreateInstance(t);

                        object[] parameter = new object[6];
                        parameter[0] = inputFileName;
                        parameter[1] = outputFileName;
                        parameter[2] = password;
                        parameter[3] = customInitVector;
                        parameter[4] = customSalt;
                        parameter[5] = iterations;

                        t.InvokeMember("DecryptFile", BindingFlags.Default | BindingFlags.InvokeMethod, null, lateBoundObj, parameter);
                        break;
                    }
                }
            }
            else
            {
                #region DEFAULT_DECRYPT
                string saltStr = string.Empty;
                if (customSalt.Length == 32)
                {
                    saltStr = customSalt;
                }
                else
                {
                    saltStr = Configuration.saltValueAlt;
                }

                string initVectorStr = string.Empty;
                if (customInitVector.Length == 16)
                {
                    initVectorStr = customInitVector;
                }
                else
                {
                    initVectorStr = Configuration.initVectorAlt;
                }

                byte[] salt = Encoding.ASCII.GetBytes(saltStr);
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, salt, iterations);
                RijndaelManaged SymmetricKey = new RijndaelManaged();
                SymmetricKey.Padding = PaddingMode.PKCS7;
                SymmetricKey.KeySize = 256;
                SymmetricKey.Mode = CipherMode.CBC;
                SymmetricKey.Key = key.GetBytes(SymmetricKey.KeySize / 8);

                GCHandle gch = GCHandle.Alloc(SymmetricKey.Key, GCHandleType.Pinned);

                SymmetricKey.IV = ASCIIEncoding.ASCII.GetBytes(initVectorStr);
                SymmetricKey.BlockSize = 128;

                initVectorStr = string.Empty;
                saltStr = string.Empty;

                FileStream fsread = new FileStream(inputFileName,
                                       FileMode.Open,
                                       FileAccess.Read);

                ICryptoTransform aesDecrypt = SymmetricKey.CreateDecryptor(SymmetricKey.Key, SymmetricKey.IV);

                CryptoStream cryptostreamDecr = new CryptoStream(fsread,
                                                             aesDecrypt,
                                                             CryptoStreamMode.Read);
                if (File.Exists(outputFileName))
                {
                    File.Delete(outputFileName);
                }

                FileStream fsOut = new FileStream(outputFileName, FileMode.Create);

                try
                {
                    int data;
                    while ((data = cryptostreamDecr.ReadByte()) != -1)
                        fsOut.WriteByte((byte)data);

                    cryptostreamDecr.Flush();
                    cryptostreamDecr.Close();
                    cryptostreamDecr.Dispose();
                }
                catch (CryptographicException ex)
                {
                    throw new CryptographicException("There was a problem decrypting the file. Check the password and try again.", ex);
                }
                finally
                {
                    if (fsOut != null)
                    {
                        fsOut.Close();
                    }

                    fsread.Close();

                    ZeroMemory(gch.AddrOfPinnedObject(), 32);
                    gch.Free();

                    SymmetricKey.Clear();
                }
                #endregion DEFAULT_DECRYPT
            }
        }

        /// <summary>
        /// File Decryption
        /// </summary>
        /// <param name="inputFileName">The file to decrypt</param>
        /// <param name="outputFileName">The resulting plaintext file</param>
        /// <param name="password">The password</param>
        public static string DecryptFileToString(string inputFileName, string password, string customInitVector = "", string customSalt = "", int iterations = 4)
        {
            Configuration config = Configuration.GetNewInstance();

            if (config.FileEncryptionModule != null && !string.IsNullOrEmpty(config.FileEncryptionModule.FileName))
            {
                Assembly assemblyInstance = Assembly.LoadFrom(@config.FileEncryptionModule.FileName);
                Type[] types = assemblyInstance.GetTypes();
                foreach (Type t in types)
                {
                    MethodInfo mi = t.GetMethod("DecryptFileToString");
                    if (mi != null)
                    {
                        string typeName = t.FullName;
                        object lateBoundObj = Activator.CreateInstance(t);

                        object[] parameter = new object[5];
                        parameter[0] = inputFileName;                        
                        parameter[1] = password;
                        parameter[2] = customInitVector;
                        parameter[3] = customSalt;
                        parameter[4] = iterations;

                        object obj = t.InvokeMember("DecryptFileToString", BindingFlags.Default | BindingFlags.InvokeMethod, null, lateBoundObj, parameter);
                        return obj.ToString();
                    }
                }
                throw new ApplicationException(SharedStrings.MAP_ERROR_CRYPTOGRAPHY);
            }
            else
            {
                #region DEFAULT_DECRYPT
                string result = string.Empty;
                string saltStr = string.Empty;
                if (customSalt.Length == 32)
                {
                    saltStr = customSalt;
                }
                else
                {
                    saltStr = Configuration.saltValueAlt;
                }

                string initVectorStr = string.Empty;
                if (customInitVector.Length == 16)
                {
                    initVectorStr = customInitVector;
                }
                else
                {
                    initVectorStr = Configuration.initVectorAlt;
                }

                byte[] salt = Encoding.ASCII.GetBytes(saltStr);
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, salt, iterations);
                RijndaelManaged SymmetricKey = new RijndaelManaged();
                SymmetricKey.Padding = PaddingMode.PKCS7;
                SymmetricKey.KeySize = 256;
                SymmetricKey.Mode = CipherMode.CBC;
                SymmetricKey.Key = key.GetBytes(SymmetricKey.KeySize / 8);

                GCHandle gch = GCHandle.Alloc(SymmetricKey.Key, GCHandleType.Pinned);

                SymmetricKey.IV = ASCIIEncoding.ASCII.GetBytes(initVectorStr);
                SymmetricKey.BlockSize = 128;

                initVectorStr = string.Empty;
                saltStr = string.Empty;

                FileStream fsread = new FileStream(inputFileName,
                                       FileMode.Open,
                                       FileAccess.Read);

                ICryptoTransform aesDecrypt = SymmetricKey.CreateDecryptor(SymmetricKey.Key, SymmetricKey.IV);

                CryptoStream cryptostreamDecr = new CryptoStream(fsread,
                                                             aesDecrypt,
                                                             CryptoStreamMode.Read);

                MemoryStream msOut = new MemoryStream();

                try
                {
                    int data;
                    while ((data = cryptostreamDecr.ReadByte()) != -1)
                        msOut.WriteByte((byte)data);

                    cryptostreamDecr.Flush();
                    cryptostreamDecr.Close();
                    cryptostreamDecr.Dispose();
                }
                catch (CryptographicException ex)
                {
                    throw new CryptographicException("There was a problem decrypting the file. Check the password and try again.", ex);
                }
                finally
                {
                    //msOut.Position = 0;
                    //var sr = new StreamReader(msOut);
                    //result = sr.ReadToEnd();

                    result = Encoding.ASCII.GetString(msOut.ToArray());

                    if (msOut != null)
                    {
                        msOut.Close();
                    }

                    fsread.Close();

                    ZeroMemory(gch.AddrOfPinnedObject(), 32);
                    gch.Free();

                    SymmetricKey.Clear();
                }
                return result;
                #endregion DEFAULT_DECRYPT
            }
        }

        //  Call this function to remove a key from memory after use for security
        [System.Runtime.InteropServices.DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr Destination, int Length);
    }
}
