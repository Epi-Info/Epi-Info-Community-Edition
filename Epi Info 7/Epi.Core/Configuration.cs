using System;
using System.Collections;
using System.Data;
using System.Security.Cryptography;

using Epi;
using Epi.Collections;
using Epi.DataSets;
using Epi.Resources;

namespace Epi
{
    /// <summary>
    /// Encapsulates all global information that persists in the config file.
    /// </summary>
    public partial class Configuration
    {
        private string configFilePath;
        private DataSets.Config configDataSet;
        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configFilePath">Configuration file path.</param>
        /// <param name="configDataSet">Configuration data.</param>
        public Configuration(string configFilePath, DataSets.Config configDataSet)
        {
            this.configDataSet = configDataSet;
            this.configFilePath = configFilePath;
        }
        #endregion Constructors

        #region Public Constants
        /// <summary>
        /// Version of the current configuration schema. Used to spot schema differences.
        /// </summary>
        public const int CurrentSchemaVersion = 117;
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets configuration file path.
        /// </summary>
        public string ConfigFilePath
        {
            get { return configFilePath; }
        }

        /// <summary>
        /// Gets configuration settings
        /// </summary>
        public Config.SettingsRow Settings
        {
            get
            {
                return configDataSet.Settings[0];
            }
        }

        /// <summary>
        /// Gets the text encryption module
        /// </summary>
        public Config.TextEncryptionModuleRow TextEncryptionModule
        {
            get
            {
                if (configDataSet.TextEncryptionModule == null || configDataSet.TextEncryptionModule.Count == 0)
                {
                    return null;
                }
                else
                {
                    return configDataSet.TextEncryptionModule[0];
                }
            }
        }

        /// <summary>
        /// Gets the file encryption module
        /// </summary>
        public Config.FileEncryptionModuleRow FileEncryptionModule
        {
            get
            {
                if (configDataSet.FileEncryptionModule == null || configDataSet.FileEncryptionModule.Count == 0)
                {
                    return null;
                }
                else
                {
                    return configDataSet.FileEncryptionModule[0];
                }
            }
        }

        /// <summary>
        /// Return configuration version information
        /// </summary>
        public Config.VersionRow Version
        {
            get
            {
                return configDataSet.Version[0];
            }
        }

        /// <summary>
        /// Gets/sets the full path of the current project file.
        /// </summary>
        public string CurrentProjectFilePath
        {
            get
            {
                return ParentRowRecentProjects.CurrentProjectLocation ?? string.Empty;
            }
            set
            {
                ParentRowRecentProjects.CurrentProjectLocation = value;
                //Save();
            }
        }

        /// <summary>
        /// Gets a datatable for recent projects
        /// </summary>
        public Config.RecentProjectDataTable RecentProjects
        {
            get
            {
                return configDataSet.RecentProject;
            }
        }

        /// <summary>
        /// Gets a datatable for recent views
        /// </summary>
        public Config.RecentViewDataTable RecentViews
        {
            get
            {
                return configDataSet.RecentView;
            }
        }

        /// <summary>
        /// Gets a datatable for recent views
        /// </summary>
        public Config.RecentDataSourceDataTable RecentDataSources
        {
            get
            {
                return configDataSet.RecentDataSource;
            }
        }

        /// <summary>
        /// Gets a datatable for data drivers
        /// </summary>
        public Config.DataDriverDataTable DataDrivers
        {
            get
            {
                return configDataSet.DataDriver;
            }
        }

        /// <summary>
        /// Gets a datatable for gadgets
        /// </summary>
        public Config.GadgetsDataTable Gadgets
        {
            get
            {
                return configDataSet.Gadgets;
            }
        }

        /// <summary>
        /// Gets a datatable for a gadget
        /// </summary>
        public Config.GadgetDataTable Gadget
        {
            get
            {
                return configDataSet.Gadget;
            }
        }

        /// <summary>
        /// Gets a datatable for file connections
        /// </summary>
        public Config.FileDataTable FileConnections
        {
            get
            {
                return configDataSet.File;
            }
        }

        /// <summary>
        /// Gets a datatable for data connections
        /// </summary>
        public Config.DatabaseDataTable DatabaseConnections
        {
            get
            {
                return configDataSet.Database;
            }
        }

        /// <summary>
        /// Gets a datatable for directories
        /// </summary>
        public Config.DirectoriesRow Directories
        {
            get
            {
                return configDataSet.Directories[0];
            }
        }

        /// <summary>
        /// Gets the collection of permanent variables
        /// If the in-memory collection is null, it loads them from persistence.
        /// </summary>
        public Config.PermanentVariableDataTable PermanentVariables
        {
            get
            {
                return ConfigDataSet.PermanentVariable;
            }
        }

        /// <summary>
        /// Gets the collection of installed Epi Info modules
        /// </summary>
        public Config.ModuleDataTable Modules
        {
            get
            {
                return configDataSet.Module;
            }
        }

        /// <summary>
        /// Gets the internal config dataset
        /// </summary>
        public DataSets.Config ConfigDataSet
        {
            get
            {
                return configDataSet;
            }
        }

        /// <summary>
        /// Gets the row from config typed dataset containing the parent row for module rows
        /// </summary>
        public Config.ModulesRow ParentRowModules
        {
            get
            {
                if (configDataSet.Modules.Count < 1)
                {
                    Config.ModulesRow newrow = configDataSet.Modules.NewModulesRow();
                    ConfigDataSet.Modules.Rows.Add(newrow);
                }
                return configDataSet.Modules[0];
            }
        }

        /// <summary>
        /// Parent row for connection rows
        /// </summary>
        public Config.ConnectionsRow ParentRowConnections
        {
            get
            {
                if (configDataSet.Projects.Count < 1)
                {
                    Config.ConnectionsRow newrow = configDataSet.Connections.NewConnectionsRow();
                    ConfigDataSet.Connections.Rows.Add(newrow);
                }
                return configDataSet.Connections[0];
            }
        }

        /// <summary>
        /// Parent row for project rows
        /// </summary>
        public Config.ProjectsRow ParentRowRecentProjects
        {
            get
            {
                if (configDataSet.Projects.Count < 1)
                {
                    Config.ProjectsRow newrow = configDataSet.Projects.NewProjectsRow();
                    newrow.CurrentProjectLocation = string.Empty;
                    ConfigDataSet.Projects.Rows.Add(newrow);
                }
                return configDataSet.Projects[0];
            }
        }

        /// <summary>
        /// Parent row for gadgets
        /// </summary>
        public Config.GadgetsRow ParentRowGadgets
        {
            get
            {
                if (configDataSet.Gadgets.Count < 1)
                {
                    Config.GadgetsRow newrow = configDataSet.Gadgets.NewGadgetsRow();
                    ConfigDataSet.Gadgets.Rows.Add(newrow);
                }
                return configDataSet.Gadgets[0];
            }
        }

        /// <summary>
        /// Parent row for recent view rows
        /// </summary>
        public Config.ViewsRow ParentRowRecentViews
        {
            get
            {
                if (configDataSet.Views.Count < 1)
                {
                    Config.ViewsRow newrow = configDataSet.Views.NewViewsRow();
                    ConfigDataSet.Views.Rows.Add(newrow);
                }
                return configDataSet.Views[0];
            }
        }

        /// <summary>
        /// Parent row for project rows
        /// </summary>
        public Config.DataSourcesRow ParentRowRecentDataSources
        {
            get
            {
                if (configDataSet.DataSources.Count < 1)
                {
                    Config.DataSourcesRow newrow = configDataSet.DataSources.NewDataSourcesRow();                    
                    ConfigDataSet.DataSources.Rows.Add(newrow);
                }
                return configDataSet.DataSources[0];
            }
        }

        /// <summary>
        /// Parent row for permanent vairables
        /// </summary>
        public Config.VariablesRow ParentRowPermanentVariables
        {
            get
            {
                if (configDataSet.Variables.Count < 1)
                {
                    Config.VariablesRow newrow = configDataSet.Variables.NewVariablesRow();
                    ConfigDataSet.Variables.Rows.Add(newrow);
                }
                return configDataSet.Variables[0];
            }
        }
        #endregion Public Properties
    }//end class
}//end namespace
