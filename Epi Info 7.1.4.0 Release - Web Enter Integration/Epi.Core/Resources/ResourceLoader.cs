using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Resources;
using System.Drawing;
using System.Reflection;

namespace Epi.Resources
{
    /// <summary>
    /// Resource Loader static class
    /// </summary>
    public static class ResourceLoader
    {
        #region Private Attributes
        private static System.Resources.ResourceManager resourceMan;
        private static System.Globalization.CultureInfo resourceCulture;
        #endregion Private Attributes

        #region Private Enums and Constants
        const string RESOURCE_NAMESPACE_BASE = "Epi.Resources";
        const string RESOURCE_NAMESPACE_IMAGES = RESOURCE_NAMESPACE_BASE + ".Images";
        const string RESOURCE_NAMESPACE_TEMPLATES = RESOURCE_NAMESPACE_BASE + ".Templates";
        const string RESOURCE_NAMESPACE_CONFIG = RESOURCE_NAMESPACE_BASE + ".Configuration";
        const string RESOURCE_NAMESPACE_SCRIPTS = RESOURCE_NAMESPACE_BASE + ".Scripts";
        const string RESOURCE_NAMESPACE_SAMPLES = RESOURCE_NAMESPACE_BASE + ".Samples";
        
        const string RESOURCES_APP_DATA = RESOURCE_NAMESPACE_BASE + ".AppData.mdb";
        const string RESOURCES_LANGUAGE_RULES = RESOURCE_NAMESPACE_BASE + ".EpiInfoGrammar.cgt";
        const string RESOURCES_LANGUAGE_RULES_ENTER = RESOURCE_NAMESPACE_BASE + ".EpiInfo.Enter.Grammar.cgt";

        const string RESOURCES_CONFIG_TRANSLATIONS = RESOURCE_NAMESPACE_CONFIG + ".Translations.mdb";
        const string RESOURCES_CONFIG_MENU = RESOURCE_NAMESPACE_CONFIG + ".EpiInfo.mnu";
        //const string RESOURCES_CONFIG_MENU = RESOURCE_NAMESPACE_CONFIG + "." + Global.MNU_FILE;
        
        const string RESOURCES_SCRIPTS_COLLECTED_DATA_TABLE = RESOURCE_NAMESPACE_SCRIPTS + ".CollectedDataTableScript.sql";
        const string RESOURCES_SCRIPTS_METADATA_TABLE = RESOURCE_NAMESPACE_SCRIPTS + ".MetadataTableScript.sql";

        const string RESOURCES_TEMPLATES_ANALYSIS_OUTPUT = RESOURCE_NAMESPACE_TEMPLATES + ".Output.htmTemplate";
        const string RESOURCES_TEMPLATES_LIVE_ANALYTICS = RESOURCE_NAMESPACE_TEMPLATES + ".LiveAnalytics.htmTemplate";
        const string RESOURCES_TEMPLATES_ANALYSIS_SESSION_HISTORY = RESOURCE_NAMESPACE_TEMPLATES + ".IHistory.htmTemplate";
        const string RESOURCES_TEMPLATES_ANALYSIS_RESULTS_HISTORY = RESOURCE_NAMESPACE_TEMPLATES + ".IResults.htmTemplate";
        const string RESOURCES_TEMPLATES_REPORT = RESOURCE_NAMESPACE_TEMPLATES + ".ReportTemplate.EPT";
        const string RESOURCES_TEMPLATES_ANALYSIS_HOME = RESOURCE_NAMESPACE_TEMPLATES + ".AnalysisHomeTemplate.xml";
        const string RESOURCES_TEMPLATES_USER_DIALOG = RESOURCE_NAMESPACE_TEMPLATES + ".UserDialog.xml";
        const string RESOURCES_TEMPLATES_PROJECT = RESOURCE_NAMESPACE_TEMPLATES + ".Project.prjtemplate";

        const string RESOURCES_PROJECTS_SAMPLE_PRJ = RESOURCE_NAMESPACE_SAMPLES + ".Sample7.prj";
        const string RESOURCES_PROJECTS_SAMPLE_MDB = RESOURCE_NAMESPACE_SAMPLES + ".Sample7.mdb";
        #endregion Private Enums and Constants

        #region Public Constants
        /// <summary>
        /// Images background resource name
        /// </summary>
        public const string IMAGES_BACKGROUND = "BG_Final.png";

        static Assembly resourceAssembly = typeof(ResourceLoader).Assembly;
        #endregion Private Constants

        #region Public Methods
        /// <summary>
        /// Get Collected DataTable Scripts
        /// </summary>
        /// <returns>string</returns>
       public static string GetCollectedDataTableScripts()
       {
           using (Stream resourceStream = resourceAssembly.GetManifestResourceStream(RESOURCES_SCRIPTS_COLLECTED_DATA_TABLE))
           {
               StreamReader reader = new StreamReader(resourceStream);
               return reader.ReadToEnd();
           }
       }

       /// <summary>
       /// Get Metadata Table Scripts
       /// </summary>
       /// <returns>string</returns>
       public static string GetMetaDataTableScripts()
        {
            using (Stream resourceStream = resourceAssembly.GetManifestResourceStream(RESOURCES_SCRIPTS_METADATA_TABLE))
            {
                StreamReader reader = new StreamReader(resourceStream);
                return reader.ReadToEnd();
            }

        }

        //public static Stream GetSampleProject()
        //{
        //    Stream resourceStream = resourceAssembly.GetManifestResourceStream(RESOURCES_PROJECTS_SAMPLE_PRJ);
        //    return resourceStream;
        //}

        //public static Stream GetSampleDatabase()
        //{ 
        //    Stream resourceStream = resourceAssembly.GetManifestResourceStream(RESOURCES_PROJECTS_SAMPLE_MDB);
        //    return resourceStream;
        //}

        /// <summary>
        /// Get Menu File
        /// </summary>
        /// <returns>string</returns>
        public static Stream GetMnuFile()
        {
            Stream resourceStream = resourceAssembly.GetManifestResourceStream(RESOURCES_CONFIG_MENU);
            return resourceStream;
        }

        /// <summary>
        /// Returns Epi Info compiled grammar table as stream
        /// </summary>
        /// <returns></returns>
        public static Stream GetCompiledGrammarTable()
        {
            Stream resourceStream = resourceAssembly.GetManifestResourceStream(RESOURCES_LANGUAGE_RULES);
            return resourceStream;
        }


        /// <summary>
        /// Returns Epi Info compiled grammar table as stream
        /// </summary>
        /// <returns></returns>
        public static Stream GetEnterCompiledGrammarTable()
        {
            Stream resourceStream = resourceAssembly.GetManifestResourceStream(RESOURCES_LANGUAGE_RULES_ENTER);
            return resourceStream;
        }

        /// <summary>
        /// Get App Data Jet Database
        /// </summary>
        /// <returns>Stream</returns>
        public static Stream GetAppDataJetDatabase()
        {
            Stream resourceStream = resourceAssembly.GetManifestResourceStream(RESOURCES_APP_DATA);
            return resourceStream;

        }

        /// <summary>
        /// Get Translations Jet Database
        /// </summary>
        /// <returns>Stream</returns>
        public static Stream GetTranslationsJetDatabase()
        {
            Stream resourceStream = resourceAssembly.GetManifestResourceStream(RESOURCES_CONFIG_TRANSLATIONS);
            return resourceStream;
        }

        /// <summary>
        /// Get Image
        /// </summary>
        /// <returns>Image</returns>
        public static Image GetImage(string filename)
        {
            if (!filename.StartsWith(RESOURCE_NAMESPACE_IMAGES))
                filename = RESOURCE_NAMESPACE_IMAGES + "." + filename;

            Image image = null;
            using (Stream resourceStream = resourceAssembly.GetManifestResourceStream(filename))
            {
                image = Image.FromStream(resourceStream);
            }

            return image;
        }

        /// <summary>
        /// Get Analysis Output Template
        /// </summary>
        /// <returns>Beginning HTML of Output Window webpage.</returns>
        public static string GetAnalysisOutputTemplate()
        {
            try
            {
                Stream resourceStream = resourceAssembly.GetManifestResourceStream(RESOURCES_TEMPLATES_ANALYSIS_OUTPUT);
                StreamReader reader = new StreamReader(resourceStream);
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Get Live Analytics Template
        /// </summary>
        /// <returns>Beginning HTML of Output Window webpage.</returns>
        public static string GetLiveAnalyticsTemplate()
        {
            try
            {
                Stream resourceStream = resourceAssembly.GetManifestResourceStream(RESOURCES_TEMPLATES_LIVE_ANALYTICS);
                StreamReader reader = new StreamReader(resourceStream);
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Gets Analysis Session History Template
        /// </summary>
        /// <returns>Beginning HTML of Session History webpage.</returns>
        public static string GetAnalysisSessionHistoryTemplate()
        {
            try
            {
                Stream resourceStream = resourceAssembly.GetManifestResourceStream(RESOURCES_TEMPLATES_ANALYSIS_SESSION_HISTORY);
                StreamReader reader = new StreamReader(resourceStream);
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Gets Analysis Results History Template
        /// </summary>
        /// <returns>Beginning HTML of Results History webpage.</returns>
        public static string GetAnalysisResultsHistoryTemplate()
        {
            try
            {
                Stream resourceStream = resourceAssembly.GetManifestResourceStream(RESOURCES_TEMPLATES_ANALYSIS_RESULTS_HISTORY);
                StreamReader reader = new StreamReader(resourceStream);
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Get Project Template
        /// </summary>
        /// <returns>Xml Document</returns>
        public static XmlDocument GetProjectTemplate()
        {
            XmlDocument doc = new XmlDocument();
            using (Stream resourceStream = resourceAssembly.GetManifestResourceStream(RESOURCES_TEMPLATES_PROJECT))
            {
                doc.Load(resourceStream);
            }
            return doc;
        }

        /// <summary>
        /// Get Analysis Home Template
        /// </summary>
        /// <returns>Xml Document</returns>
        public static XmlDocument GetAnalysisHomeTemplate()
        {
            XmlDocument doc = new XmlDocument();
            using (Stream resourceStream = resourceAssembly.GetManifestResourceStream(RESOURCES_TEMPLATES_ANALYSIS_HOME))
            {
                doc.Load(resourceStream);
            }
            return doc;

        }

        /// <summary>
        /// GetEptTemplate
        /// </summary>
        /// <returns>XmlDocument</returns>
        public static XmlDocument GetEptTemplate()
        {
            XmlDocument doc = new XmlDocument();
            using (Stream resourceStream = resourceAssembly.GetManifestResourceStream(RESOURCES_TEMPLATES_REPORT))
            {
                doc.Load(resourceStream);
            }
            return doc;
        }

        /// <summary>
        /// GetDialogTemplate
        /// </summary>
        /// <returns>XmlDocument</returns>
        public static XmlDocument GetUserDialogTemplate()
        {
            XmlDocument doc = new XmlDocument();
            using (Stream resourceStream = resourceAssembly.GetManifestResourceStream(RESOURCES_TEMPLATES_USER_DIALOG))
            {
                doc.Load(resourceStream);
            }
            return doc;
        }
        #endregion Public Methods

        #region private Methods
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        private static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    System.Resources.ResourceManager temp = new System.Resources.ResourceManager(RESOURCE_NAMESPACE_BASE, typeof(ResourceLoader).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        private static System.Globalization.CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }
        #endregion Private Methods



    }
}
