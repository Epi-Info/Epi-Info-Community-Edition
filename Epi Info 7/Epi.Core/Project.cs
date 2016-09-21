using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Security.Cryptography;
using Epi;
using System.Collections.Generic;
using Epi.Collections;
using Epi.Data;
using Epi.Fields;
using Epi.Resources;
using Epi.Data.Services;

namespace Epi
{
    /// <summary>
    /// Class Project
    /// </summary>
    public class Project : INamedObject, IDisposable // Project
    {
        #region Public Events
        /// <summary>
        /// Event Handler for TableCopyStatusEvent
        /// </summary>
        public event TableCopyStatusEventHandler TableCopyStatusEvent;
        
        /// <summary>
        /// Raise the TableCopyStatus Event
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="recordCount">Record Count</param>
        public void RaiseEventTableCopyStatus(string tableName, int recordCount)
        {
            if (this.TableCopyStatusEvent != null)
            {
                this.TableCopyStatusEvent(this, new TableCopyStatusEventArgs(tableName, recordCount));
            }
        }
        #endregion Public Events

        #region Private class members

        /// <summary>
        /// Collection of project views.
        /// </summary>
        public ViewCollection views = null;

        /// <summary>
        /// List of project pages.
        /// </summary>
        /// 
        public List<Page> pages = null;

        private Guid id;
        private XmlDocument xmlDoc = null;
        private const int currentSchemaVersion = 102;
        private XmlElement currentViewElement;
        private string collectedDataConnectionString;
        bool _useMetaDataSet = false;

        #endregion Private class members

        #region Protected Class Members
        /// <summary>
        /// Project metadata accessor.
        /// </summary>
        protected IMetadataProvider metadata = null;
        /// <summary>
        /// Project collected data accessor.
        /// </summary>
        protected CollectedDataProvider collectedData = null;

        #endregion Protected Class Members

        #region Constructors

        /// <summary>
        /// Default Constructors
        /// </summary>
        public Project()
        {
            //isNew = true;
            PreConstruct();

            // Add root element and attributes
            XmlElement root = xmlDoc.CreateElement("Project");
            xmlDoc.AppendChild(root);

            // Add attributes of the root node
            XmlAttribute attr = xmlDoc.CreateAttribute("id");
            root.Attributes.Append(attr);

            attr = xmlDoc.CreateAttribute("name");
            root.Attributes.Append(attr);

            attr = xmlDoc.CreateAttribute("location");
            root.Attributes.Append(attr);

            attr = xmlDoc.CreateAttribute("description");
            root.Attributes.Append(attr);

            attr = xmlDoc.CreateAttribute("schemaVersion");
            attr.Value = currentSchemaVersion.ToString();
            root.Attributes.Append(attr);

            attr = xmlDoc.CreateAttribute("epiVersion");
            root.Attributes.Append(attr = xmlDoc.CreateAttribute("epiVersion"));

            attr = xmlDoc.CreateAttribute("createDate");
            attr.Value = DateTime.Now.ToString(CultureInfo.InvariantCulture.DateTimeFormat);
            root.Attributes.Append(attr);

            attr = xmlDoc.CreateAttribute("useMetadataDbForCollectedData");
            attr.Value = string.Empty;
            root.Attributes.Append(attr);

            attr = xmlDoc.CreateAttribute("useBackgroundOnAllPages");
            attr.Value = Epi.Defaults.UseBackgroundOnAllPages.ToString();
            root.Attributes.Append(attr);

            // Add Collected data node
            XmlElement xCollectedData = xmlDoc.CreateElement("CollectedData");
            root.AppendChild(xCollectedData);
            XmlElement xDb = xmlDoc.CreateElement("Database");
            xCollectedData.AppendChild(xDb);
            xDb.Attributes.Append(xmlDoc.CreateAttribute("connectionString"));
            xDb.Attributes.Append(xmlDoc.CreateAttribute("dataDriver"));

            // Add Metadata node.
            XmlElement xMetadata = xmlDoc.CreateElement("Metadata");
            root.AppendChild(xMetadata);
            attr = xmlDoc.CreateAttribute("source");
            attr.Value = ((int)MetadataSource.Unknown).ToString();
            xMetadata.Attributes.Append(attr);

            // Add Enter and MakeView Interpreter node.
            XmlElement xEnter_MakeViewInterpreter = xmlDoc.CreateElement("EnterMakeviewInterpreter");
            root.AppendChild(xEnter_MakeViewInterpreter);
            attr = xmlDoc.CreateAttribute("source");
            attr.Value = "Epi.Core.EnterInterpreter";
            xEnter_MakeViewInterpreter.Attributes.Append(attr);
        }

        public Project(string filePath)
        {
            Construct(filePath);
        }
        
        public Project(string filePath, bool useMetaDataTable)
        {
            _useMetaDataSet = useMetaDataTable;
            Construct(filePath);
        }

        private void PreConstruct()
        {
            xmlDoc = new XmlDocument();
            metaDbInfo = new DbDriverInfo();
            collectedDataDbInfo = new DbDriverInfo();
            collectedData = new CollectedDataProvider(this);
        }

        private void Construct(string filePath)
        {
            PreConstruct();

            try
            {
                filePath = Environment.ExpandEnvironmentVariables(filePath);

                xmlDoc.Load(filePath);
                ValidateXmlDoc();

                FileInfo fileInfo = new FileInfo(filePath);

                if (string.IsNullOrEmpty(Location))
                {
                    Location = fileInfo.DirectoryName;
                    Save();
                }
                else
                {
                    if (string.Compare(fileInfo.DirectoryName, Location, true) != 0)
                    {
                        Location = fileInfo.DirectoryName;
                        Save();
                    }
                }

                string[] Driver = this.CollectedDataDriver.Split(',');

                if (Driver[1].Trim().ToLowerInvariant() == Configuration.WebDriver.ToLowerInvariant())
                {
                    this.collectedData.IsWebMode = true;
                    switch (Driver[0].Trim())
                    {
                        case "Epi.Data.MySQL.MySQLDBFactory":
                            this.CollectedDataDriver = Configuration.MySQLDriver;
                            break;
                        case "Epi.Data.Office.AccessDBFactory":
                            this.CollectedDataDriver = Configuration.AccessDriver;
                            break;
                        case "Epi.Data.SqlServer.SqlDBFactory":
                        default:
                            this.CollectedDataDriver = Configuration.SqlDriver;
                            break;
                    }
                }

                this.collectedDataDbInfo.DBCnnStringBuilder.ConnectionString = this.CollectedDataConnectionString;
                collectedData.Initialize(this.collectedDataDbInfo, this.CollectedDataDriver, false);

                if (MetadataSource == MetadataSource.Xml)
                {
                    metadata = new MetadataXmlProvider(this);
                }
                else
                {
                    if (_useMetaDataSet)
                    {
                        metadata = new MetadataDataSet(this);
                    }
                    else
                    {
                        metadata = new MetadataDbProvider(this);
                        if (MetadataSource == MetadataSource.SameDb)
                        {
                            metadata.AttachDbDriver(CollectedData.GetDbDriver());
                        }
                        else
                        {
                            this.metaDbInfo.DBCnnStringBuilder.ConnectionString = this.MetadataConnectionString;
                            metadata.Initialize(this.metaDbInfo, this.MetadataDriver, false);
                        }
                    }
                }
            }
            finally
            {
            }
        }

        #endregion Constructors

        #region Public Properties

        public bool UseMetaDataSet
        {
            set
            {
                _useMetaDataSet = value;
            }
        }

        /// <summary>
        /// Gets/sets the path name of project file.
        /// </summary>
        public string Location
        {
            get
            {
                return xmlDoc.DocumentElement.Attributes["location"].Value;
            }
            set
            {
                xmlDoc.DocumentElement.Attributes["location"].Value = value;
            }
        }

        public string EnterMakeviewIntepreter
        {
            get
            {
                return xmlDoc.DocumentElement["EnterMakeviewInterpreter"].Attributes["source"].Value;
            }
            set
            {
                xmlDoc.DocumentElement["EnterMakeviewInterpreter"].Attributes["source"].Value = value;
            }
        }

        /// <summary>
        /// Gets project display name.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return Name;
            }
        }

        /// <summary>
        /// Project name.
        /// </summary>
        public string Name
        {
            get
            {
                return xmlDoc.DocumentElement.Attributes["name"].Value;
            }
            set
            {
                xmlDoc.DocumentElement.Attributes["name"].Value = value;
            }
        }

        /// <summary>
        /// The width of the panel that contains the controls.
        /// </summary>
        public string PageWidth
        {
            get
            {
                return xmlDoc.DocumentElement.Attributes["pageWidth"].Value;
            }
            set
            {
                xmlDoc.DocumentElement.Attributes["pageWidth"].Value = value;
            }
        }

        /// <summary>
        /// The height of the panel that contains the controls.
        /// </summary>
        public string PageHeight
        {
            get
            {
                return xmlDoc.DocumentElement.Attributes["pageHeight"].Value;
            }
            set
            {
                xmlDoc.DocumentElement.Attributes["pageHeight"].Value = value;
            }
        }
        
        /// <summary>
        /// Returns the file name of the project.
        /// </summary>
        public string FileName
        {
            get
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    return Name.Replace(FileExtension, string.Empty) + FileExtension;
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        /// <summary>
        /// Returns the full name of the data source.
        /// </summary>
        public string FilePath
        {
            get
            {
                if (string.IsNullOrEmpty(Location) || string.IsNullOrEmpty(FileName))
                {
                    return string.Empty;
                }
                else
                {
                    return Path.Combine(Location, FileName);
                }
            }
        }

        /// <summary>
        /// Returns the full name of the data source.
        /// </summary>
        public string FullName
        {
            get
            {
                return FilePath;
            }
        }

        /// <summary>
        /// Returns use metadata for collected data flag.
        /// </summary>
        public virtual bool UseMetadataDbForCollectedData
        {
            get
            {
                return bool.Parse(xmlDoc.DocumentElement.Attributes["useMetadataDbForCollectedData"].Value);
            }
            set
            {
                xmlDoc.DocumentElement.Attributes["useMetadataDbForCollectedData"].Value = value.ToString();
            }
        }

        /// <summary>
        /// Determines if the project is empty.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return (string.IsNullOrEmpty(FullName));
            }
        }

        /// <summary>
        /// Project metadata.
        /// </summary>
        public IMetadataProvider Metadata
        {
            get
            {
                return metadata;
            }
        }

        /// <summary>
        /// Project collected data.
        /// </summary>
        public CollectedDataProvider CollectedData
        {
            get
            {
                return collectedData;
            }
        }

        /// <summary>
        /// Project metadata.
        /// </summary>
        public IMetadataProvider CodeData
        {
            get
            {
                return Metadata;
            }
        }

        /// <summary>
        /// Determines if this data source is actually an Epi (2000 or 7)collected data.
        /// </summary>
        public virtual bool IsEpiCollectedData
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a globally unique identifier for the project.
        /// </summary>
        public System.Guid Id
        {
            get
            {
                if (id.Equals(Guid.Empty))
                {
                    if (string.IsNullOrEmpty(FilePath))
                    {
                        //return Guid.Empty;
                        id = Guid.NewGuid();
                    }
                    else
                    {
                        id = Util.GetFileGuid(FilePath);
                    }
                }
                return id;
            }
            set
            {
                xmlDoc.DocumentElement.Attributes["id"].Value = value.ToString();
            }
        }

        /// <summary>
        /// Views of the project.
        /// </summary>
        public ViewCollection Views
        {
            get
            {
                if (views == null)
                {
                    LoadViews();
                }
                return views;
            }
        }

        /// <summary>
        /// Returns the original Epi Info version of the project.
        /// </summary>
        public string EpiVersion
        {
            get
            {
                return xmlDoc.DocumentElement.Attributes["epiVersion"].Value;
            }
        }

        /// <summary>
        /// Returns date the project was created.
        /// </summary>
        public DateTime CreateDate
        {
            get
            {
                return DateTime.Parse(xmlDoc.DocumentElement.Attributes["createDate"].Value, CultureInfo.InvariantCulture.DateTimeFormat);
            }
        }

        /// <summary>
        /// Project description.
        /// </summary>
        public string Description
        {
            get
            {
                return xmlDoc.DocumentElement.Attributes["description"].Value;
            }
            set
            {
                xmlDoc.DocumentElement.Attributes["description"].Value = value;
            }
        }

        private XmlNode GetMetadataDbNode()
        {
            return xmlDoc.DocumentElement.SelectSingleNode("/Project/Metadata/Database");
        }

        private XmlNode GetCollectedDataDbNode()
        {
            return xmlDoc.DocumentElement.SelectSingleNode("/Project/CollectedData/Database");
        }

        /// <summary>
        /// Connection string for the Metadata database.
        /// </summary>
        public string MetadataConnectionString
        {
            get
            {
                return GetMetadataDbNode().Attributes["connectionString"].Value;
            }
            set
            {
                GetMetadataDbNode().Attributes["connectionString"].Value = value;
            }
        }

        /// <summary>
        /// Driver name for the Metadata database.
        /// </summary>
        public string MetadataDriver
        {
            get
            {
                return GetMetadataDbNode().Attributes["dataDriver"].Value;
            }
            set
            {
                GetMetadataDbNode().Attributes["dataDriver"].Value = value.ToString();
            }
        }
        private DbDriverInfo metaDbInfo;
        /// <summary>
        /// Information for the Metadata database.
        /// </summary>
        public DbDriverInfo MetaDbInfo
        {
            get
            {
                return metaDbInfo;
            }
            set
            {
                metaDbInfo = value;
            }
        }

        private DbDriverInfo collectedDataDbInfo;
        /// <summary>
        /// Information for the Collected database.
        /// </summary>
        public DbDriverInfo CollectedDataDbInfo
        {
            get
            {
                return collectedDataDbInfo;
            }
            set
            {
                collectedDataDbInfo = value;
            }
        }

        /// <summary>
        /// Gets/sets the metadata source. Possible values are Database and Xml.
        /// </summary>
        public Epi.MetadataSource MetadataSource
        {
            get
            {
                XmlNode metadataNode = GetMetadataNode();
                XmlAttribute sourceAttribute = metadataNode.Attributes.GetNamedItem("source") as XmlAttribute;
                if (sourceAttribute == null)
                {
                    return MetadataSource.Unknown;
                }
                else
                {
                    return (MetadataSource)int.Parse(sourceAttribute.Value);
                }
            }
            set
            {
                XmlNode metadataNode = GetMetadataNode();
                metadataNode.Attributes["source"].Value = ((int)value).ToString();
                switch (value)
                {
                    case MetadataSource.Xml:
                        metadata = new MetadataXmlProvider(this);
                        break;
                    case MetadataSource.SameDb:
                        metadata = new MetadataDbProvider(this);
                        break;
                    case MetadataSource.DifferentDb:
                        metadata = new MetadataDbProvider(this);
                        XmlElement xDb = xmlDoc.CreateElement("Database");
                        GetMetadataNode().AppendChild(xDb);
                        xDb.Attributes.Append(xmlDoc.CreateAttribute("connectionString"));
                        xDb.Attributes.Append(xmlDoc.CreateAttribute("dataDriver"));
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Connection string for the Collected database.
        /// </summary>
        public string CollectedDataConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(collectedDataConnectionString))
                {
                    collectedDataConnectionString = Configuration.Decrypt(GetCollectedDataDbNode().Attributes["connectionString"].Value);
                }
                if (this.CollectedDataDriver == "Epi.Data.Office.AccessDBFactory, Epi.Data.Office")
                    return this.SetOleDbDatabaseFilePath(collectedDataConnectionString);
                else
                    return collectedDataConnectionString;

            }
            set
            {
                GetCollectedDataDbNode().Attributes["connectionString"].Value = Configuration.Encrypt(value);
                collectedDataConnectionString = value;
            }
        }

        

        /// <summary>
        /// Data Driver for the Collected database.
        /// </summary>
        public string CollectedDataDriver
        {
            get
            {
                return GetCollectedDataDbNode().Attributes["dataDriver"].Value;
            }
            set
            {
                GetCollectedDataDbNode().Attributes["dataDriver"].Value = value.ToString();
            }
        }

        #endregion Public Properties

        #region Static Methods

        /// <summary>
        /// Checks the name of a project to make sure the syntax is valid.
        /// </summary>
        /// <param name="projectName">The name of the project to validate</param>
        /// <param name="validationStatus">The message that is passed back to the calling method regarding the status of the validation attempt</param>
        /// <returns>Whether or not the name passed validation; true for a valid name, false for an invalid name</returns>
        public static bool IsValidProjectName(string projectName, ref string validationStatus)
        {
            // assume valid by default
            bool valid = true;

            if (string.IsNullOrEmpty(projectName.Trim()))
            {
                // if the project name is empty, or just a series of spaces, invalidate it
                validationStatus = SharedStrings.MISSING_PROJECT_NAME;
                valid = false;
            }
            else if (projectName.Contains("'"))
            {
                // if the project name contains an apostrophe.
                validationStatus = SharedStrings.INVALID_PROJECT_NAME;
                valid = false;
                
            }
            else if (AppData.Instance.IsReservedWord(projectName))
            {
                // if the project name is a reserved word, invalidate it
                validationStatus = SharedStrings.INVALID_PROJECT_NAME_RESERVED_WORD;
                valid = false;
            }
            else if (projectName.Length > 64)
            {
                validationStatus = SharedStrings.INVALID_PROJECT_NAME_TOO_LONG;
                valid = false;
            }
            else
            {
                // if the project name is not empty or in the list of reserved words...
                System.Text.RegularExpressions.Match numMatch = System.Text.RegularExpressions.Regex.Match(projectName.Substring(0, 1), "[0-9]");

                if (numMatch.Success)
                {
                    // if the project name has numbers for the first character, invalidate it
                    validationStatus = SharedStrings.PROJECT_NAME_BEGIN_NUMERIC;
                    valid = false;
                }
                // if the project name doesn't have a number as the first character...
                else
                {
                    // iterate over all of the characters in the project name
                    for (int i = 0; i < projectName.Length; i++)
                    {
                        string viewChar = projectName.Substring(i, 1);
                        System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(viewChar, "[A-Za-z0-9_]");
                        // if the project name does not consist of only letters and numbers...
                        if (!m.Success)
                        {
                            // we found an invalid character; invalidate the project name
                            validationStatus = SharedStrings.INVALID_PROJECT_NAME;
                            valid = false;
                            break; // stop the for loop here, no point in continuing
                        }
                    }
                }
            }

            return valid;
        }

        #endregion // Static Methods

        #region Public Methods
        /// <summary>
        /// Returns the Project Id
        /// </summary>
        /// <returns>The GUID representation of the project Id</returns>
        public Guid GetProjectId()
        {
            return this.Id;
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public virtual void Dispose()
        {
            if (metadata != null)
            {
                //metadata.Dispose();
                metadata = null;
            }
            if (collectedData != null)
            {
                //collectedData.Dispose();
                collectedData = null;
            }
            if (views != null)
            {
                //views.Dispose();
                views = null;
            }
        }

        /// <summary>
        /// Saves the XML document for the project using the specified <see cref="System.Xml.XmlWriter"/>.
        /// </summary>
        public virtual void Save()
        {
            try
            {
                xmlDoc.Save(FilePath);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw ex;
            }
            catch (XmlException xmlEx)
            {
                throw xmlEx;
            }
        }

        /// <summary>
        /// Returns the Xml document for the project.
        /// </summary>
        /// <returns>Xml Document object.</returns>
        public XmlDocument GetXmlDocument()
        {
            return xmlDoc;
        }

        /// <summary>
        /// Returns all views in the current project as a DataTable
        /// </summary>
        /// <returns>Contents of view's data table.</returns>
        public virtual DataTable GetViewsAsDataTable()
        {
            return (Metadata.GetViewsAsDataTable());
        }

        /// <summary>
        /// Returns list of tables that are <see cref="Epi.View"/>s.
        /// </summary>
        /// <returns>Listof view names</returns>
        public virtual List<string> GetViewNames()
        {
            DataTable dt = Metadata.GetViewsAsDataTable();
            List<String> list = new List<String>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(row[ColumnNames.NAME].ToString());
            }
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual List<string> GetParentViewNames()
        {
            DataTable dt = Metadata.GetViewsAsDataTable();
            List<String> list = new List<String>();
            //If SQL permissions denied, returns dt with no rows--checked here.  den4 11/23/2010
            if (dt == null || dt.Rows.Count == 0 )
            {
                return list;
            }
            DataRow[] rows = dt.Select(ColumnNames.IS_RELATED_VIEW + "=false");
            foreach (DataRow row in rows)
            {
                list.Add(row[ColumnNames.NAME].ToString());
            }
            return list;
        }

        /// <summary>
        /// Is View (by name) flag.
        /// </summary>
        /// <remarks>
        /// returns true if the named object is (or has) a view
        /// </remarks>
        /// <param name="name">Name of view to check.</param>
        /// <returns>True/False</returns>
        public bool IsView(string name)
        {
            // dcs0 If it's not in MetaViews - it's not a view - period!
            //if (name.ToLowerInvariant().StartsWith("view"))
            //{
            //    return true;
            //}
            //else
            //{
            //List<string> list = GetViewNames();
            // dcs0 was case sensitive
            foreach (string s in GetViewNames())
            {
                if (string.Compare(s, name, true) == 0)
                {
                    return true;
                }

                //return (list.Contains(name));
            }
            return false;
            //            return GetViewNames().Contains(name);
        }

        /// <summary>
        /// Returns a view by it's name
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns>Project <see cref="Epi.View"/></returns>
        public View GetViewByName(string viewName)
        {
            //foreach (View view in GetViews())
            foreach (View view in Views)
            {
                if (string.Compare(view.Name, viewName, true) == 0)
                {
                    return view;
                }
            }
            throw new System.ApplicationException(string.Format(SharedStrings.ERROR_LOADING_VIEW, viewName));
        }

        /// <summary>
        /// Returns table column names.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <returns>Listof table column names.</returns>
        public List<string> GetTableColumnNames(string tableName)
        {
            return CollectedData.GetTableColumnNames(tableName);
        }

        /// <summary>
        /// Returns Primary key names.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <returns>List of primary key names.</returns>
        public List<string> GetPrimaryKeyNames(string tableName)
        {
            DataTable dt = CollectedData.GetPrimaryKeysAsDataTable(tableName);
            List<string> list = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(row[ColumnNames.COLUMN_NAME].ToString());
            }
            return list;
        }

        /// <summary>
        /// Returns contents of all nonview (collected data) tables.
        /// </summary>
        /// <returns>Contents of nonview tables.</returns>
        public virtual DataTable GetNonViewTablesAsDataTable()
        {
            return CollectedData.GetNonViewTablesAsDataTable();
        }

        /// <summary>
        /// Returns names of all nonview (collected data) tables.
        /// </summary>
        /// <returns>List of nonview table names.</returns>
        public List<string> GetNonViewTableNames()
        {
            DataTable dt = Metadata.GetNonViewTablesAsDataTable();
            List<string> list = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(row[ColumnNames.NAME].ToString());
            }
            return list;
        }

        /// <summary>
        /// Gets <see cref="Epi.View"/> by view Id.
        /// </summary>
        /// <param name="viewId">Id of <see cref="Epi.View"/> to get.</param>
        /// <returns>Project <see cref="Epi.View"/></returns>
        public View GetViewById(int viewId)
        {            
            return (Views.GetViewById(viewId));
        }

        /// <summary>
        /// Saves project information
        /// </summary>
        public virtual Project CreateProject(string projectName, string projectDescription, string projectLocation, string collectedDataDriver, DbDriverInfo collectedDataDBInfo)
        {
            Project newProject = new Project();
            newProject.Name = projectName;

            newProject.Location = Path.Combine(projectLocation, projectName);

            if (collectedDataDBInfo.DBCnnStringBuilder.ContainsKey("Provider") && (collectedDataDBInfo.DBCnnStringBuilder["Provider"].ToString() == "Microsoft.Jet.OLEDB.4.0"))
            {
                collectedDataDBInfo.DBCnnStringBuilder["Data Source"] = newProject.FilePath.Substring(0, newProject.FilePath.Length - 4) + ".mdb";
            }

            if (!Directory.Exists(newProject.Location))
            {
                Directory.CreateDirectory(newProject.Location);
            }

            newProject.Id = newProject.GetProjectId();
            if (File.Exists(newProject.FilePath))
            {
                DialogResult dr = MessageBox.Show(string.Format(SharedStrings.PROJECT_ALREADY_EXISTS, newProject.FilePath), SharedStrings.PROJECT_ALREADY_EXISTS_TITLE, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                switch (dr)
                {
                    case DialogResult.Yes:
                        break;
                    case DialogResult.No:
                        return null;
                }
            }

            newProject.Description = projectDescription;

            // Collected data ...
            newProject.CollectedDataDbInfo = collectedDataDBInfo;
            newProject.CollectedDataConnectionString = collectedDataDBInfo.DBCnnStringBuilder.ToString();
            
            newProject.CollectedDataDriver = collectedDataDriver;
            newProject.CollectedData.Initialize(collectedDataDBInfo, collectedDataDriver, true);

            // Check that there isn't an Epi 7 project already here.
            if (newProject.CollectedDataDriver != "Epi.Data.Office.AccessDBFactory, Epi.Data.Office")
            {
                List<string> tableNames = new List<string>();
                tableNames.Add("metaBackgrounds");
                tableNames.Add("metaDataTypes");
                tableNames.Add("metaDbInfo");
                tableNames.Add("metaFields");
                tableNames.Add("metaFieldTypes");
                tableNames.Add("metaGridColumns");
                tableNames.Add("metaImages");
                tableNames.Add("metaLayerRenderTypes");
                tableNames.Add("metaLayers");
                tableNames.Add("metaMapLayers");
                tableNames.Add("metaMapPoints");
                tableNames.Add("metaMaps");
                tableNames.Add("metaPages");
                tableNames.Add("metaPatterns");
                tableNames.Add("metaPrograms");
                tableNames.Add("metaViews");

                bool projectExists = false;
                foreach (string s in tableNames)
                {
                    if (newProject.CollectedData.TableExists(s))
                    {
                        projectExists = true;
                        break;
                    }
                }

                if (projectExists)
                {
                    DialogResult result = MessageBox.Show(SharedStrings.WARNING_PROJECT_MAY_ALREADY_EXIST, SharedStrings.WARNING_PROJECT_MAY_ALREADY_EXIST_SHORT, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.Cancel)
                    {
                        Logger.Log(DateTime.Now + ":  " + "Project creation aborted by user [" + System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString() + "] after being prompted to overwrite existing Epi Info 7 project metadata.");
                        return null;
                    }
                    else
                    {
                        Logger.Log(DateTime.Now + ":  " + "Project creation proceeded by user [" + System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString() + "] after being prompted to overwrite existing Epi Info 7 project metadata.");
                    }
                }
            }

            Logger.Log(DateTime.Now + ":  " + string.Format("Project [{0}] created in {1} by user [{2}].", newProject.Name, newProject.Location, System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString()));

            // Metadata ..
            newProject.MetadataSource = MetadataSource.SameDb;
            MetadataDbProvider typedMetadata = newProject.Metadata as MetadataDbProvider;
            typedMetadata.AttachDbDriver(newProject.CollectedData.GetDbDriver());
            typedMetadata.CreateMetadataTables();

            try
            {
                newProject.Save();
                return newProject;
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(ex.Message);
                return newProject;
            }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns>New project <see cref="Epi.View"/></returns>
        public View CreateView(string viewName)
        {
            return CreateView(viewName, false);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="viewName">New <see cref="Epi.View"/> name.</param>
        /// <param name="isChildView">Is Related (child) view flag.</param>
        /// <returns>New project <see cref="Epi.View"/></returns>
        public View CreateView(string viewName, bool isChildView)
        {
            View newView = new View(this);
            newView.Name = viewName;
            newView.SetTableName(newView.Name);
            newView.IsRelatedView = isChildView;

            if (!Views.Contains(newView))
            {
                Views.Add(newView);
            }
            
            Metadata.InsertView(newView);
            currentViewElement = newView.ViewElement;
            LoadViews();
            return newView;
        }

        /// <summary>
        /// Returns a list of programs saved in the project.
        /// </summary>
        /// <returns>DataTable containing a list of programs</returns>
        public virtual DataTable GetPgms()
        {
            return (Metadata.GetPgms());
        }

        /// <summary>
        /// Returns a list of program names.
        /// </summary>
        /// <returns>List of program names.</returns>
        public List<string> GetPgmNames()
        {
            DataTable dt = Metadata.GetPgms();
            List<string> list = new List<String>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(row[ColumnNames.NAME].ToString());
            }
            return list;
        }

        /// <summary>
        /// Inserts a program into the database
        /// </summary>
        /// <param name="name">Name of the program</param>
        /// <param name="content">Content of the program</param>
        /// <param name="comment">Comment for the program</param>
        /// <param name="author">Author of the program</param>
        public virtual void InsertPgm(string name, string content, string comment, string author)
        {
            Metadata.InsertPgm(name, content, comment, author);
        }
        /// <summary>
        /// Inserts a program into the database
        /// </summary>
        /// <param name="pgmRow">A DataRow with all of the parameters</param>
        public virtual void InsertPgm(DataRow pgmRow)
        {
            // dcs TODO temporary; overload MetaData method
            Metadata.InsertPgm(pgmRow[ColumnNames.PGM_NAME].ToString(), pgmRow[ColumnNames.PGM_CONTENT].ToString(),
                                   pgmRow[ColumnNames.PGM_COMMENT].ToString(), pgmRow[ColumnNames.PGM_AUTHOR].ToString());
        }
        /// <summary>
        /// Deletes a program from the database
        /// </summary>
        /// <param name="programName">Name of the program to be deleted</param>
        /// <param name="programId">Id of the program to be deleted</param>
        public virtual void DeletePgm(string programName, int programId)
        {
            Metadata.DeletePgm(programId);
        }

        /// <summary>
        /// Updates a program saved in the database
        /// </summary>
        /// <param name="programId">Id of the program</param>
        /// <param name="name">Name of the program</param>
        /// <param name="content">Content of the program</param>
        /// <param name="comment">Comment for the program</param>
        /// <param name="author">Author of the program</param>
        public virtual void UpdatePgm(int programId, string name, string content, string comment, string author)
        {
            Metadata.UpdatePgm(programId, name, content, comment, author);
        }

        /// <summary>
        /// Returns a list of data table names.
        /// </summary>
        /// <remarks>Make same call as Project.GetDataTableNames().</remarks>
        /// <returns>List of data table names.</returns>
        public virtual List<string> GetDataTableList()
        {
            return (Metadata.GetDataTableList());
        }

        /// <summary>
        /// Returns a list of data table names.
        /// </summary>
        /// <remarks>Makes same call as Project.GetDataTableList().</remarks>
        /// <returns>List of data table names.</returns>
        public List<string> GetDataTableNames()
        {
            return Metadata.GetDataTableList();
        }

        /// <summary>
        /// Create a new code table.
        /// </summary>
        /// <param name="tableName">Name of new code table.</param>
        /// <param name="columnNames">List of new columns to create in new code table.</param>
        public virtual void CreateCodeTable(string tableName, string[] columnNames)
        {
            CodeData.CreateCodeTable(tableName, columnNames);
        }

        /// <summary>
        /// Create a new code table.
        /// </summary>
        /// <param name="tableName">Name of new code table.</param>
        /// <param name="columnName">Name of column to create in new code table.</param>
        public virtual void CreateCodeTable(string tableName, string columnName)
        {
            CodeData.CreateCodeTable(tableName, columnName);
        }

        /// <summary>
        /// Save code table data.
        /// </summary>
        /// <param name="dataTable"><see cref="System.Data.DataTable"/> containing code table data.</param>
        /// <param name="tableName">Name of code table.</param>
        /// <param name="columnName">Name of code table column.</param>
        public virtual void SaveCodeTableData(DataTable dataTable, string tableName, string columnName)
        {
            CodeData.SaveCodeTableData(dataTable, tableName, columnName);
        }

        /// <summary>
        /// Save code table data
        /// </summary>
        /// <param name="dataTable"><see cref="System.Data.DataTable"/> containing code table data.</param>
        /// <param name="tablename">Name of code table.</param>
        /// <param name="columnNames">List of code table column names.</param>
        public virtual void SaveCodeTableData(DataTable dataTable, string tablename, string[] columnNames)
        {
            CodeData.SaveCodeTableData(dataTable, tablename, columnNames);
        }


        /// <summary>
        /// Insert code table data
        /// </summary>
        /// <param name="dataTable"><see cref="System.Data.DataTable"/> containing code table data.</param>
        /// <param name="tablename">Name of code table.</param>
        /// <param name="columnNames">List of code table column names.</param>
        public virtual void InsertCodeTableData(DataTable dataTable, string tablename, string[] columnNames)
        {
            CodeData.InsertCodeTableData(dataTable, tablename, columnNames);
        }

        /// <summary>
        /// Obsolete calls to return code table data by code table name.
        /// </summary>
        /// <param name="codeTableName">Code table name</param>
        /// <returns>Code table data.</returns>
        [Obsolete("Use of DataTable in this context is no different than the use of a multidimensional System.Object array (not recommended).", false)]
        public virtual DataTable GetCodeTableData(string codeTableName)
        {
            return CodeData.GetCodeTableData(codeTableName);
        }
        /// <summary>
        /// Returns table data
        /// </summary>
        /// <param name="tableName">Name of data table.</param>
        /// <returns>Contents of data table.</returns>
        public virtual DataTable GetTableData(string tableName)
        {
            if (tableName.StartsWith("code", StringComparison.InvariantCultureIgnoreCase)) // it is a code table
            {
                return CodeData.GetCodeTableData(tableName);
            }
            else // It is a data table
            {
                return CollectedData.GetTableData(tableName);
            }
        }

        /// <summary>
        /// Returns table data
        /// </summary>
        /// <param name="tableName">Name of data table.</param>
        /// <param name="columnNames">List of column names in data table.</param>
        /// <returns>Contents of data table.</returns>
        public virtual DataTable GetTableData(string tableName, string columnNames)
        {
            if (tableName.StartsWith("code", StringComparison.InvariantCultureIgnoreCase)) // it is a code table
            {
                return CodeData.GetCodeTableData(tableName, columnNames);
            }
            else // It is a data table
            {
                return CollectedData.GetTableData(tableName, columnNames);
            }
        }

        /// <summary>
        /// Returns table data
        /// </summary>
        /// <param name="tableName">Name of data table.</param>
        /// <param name="columnNames">Name of column in data table.</param>
        /// <param name="sortCriteria"></param>
        /// <returns>Contents of data table.</returns>
        public virtual DataTable GetTableData(string tableName, string columnNames, string sortCriteria)
        {
            if (tableName.StartsWith("code", StringComparison.InvariantCultureIgnoreCase)) // it is a code table
            {
                return CodeData.GetCodeTableData(tableName, columnNames, sortCriteria);
            }
            else // It is a data table
            {
                return CollectedData.GetTableData(tableName, columnNames, sortCriteria);
            }
        }

        /// <summary>
        /// Creates a link to a table from this project
        /// </summary>
        /// <param name="linkName">Name of link to make.</param>
        /// <param name="tableName">Name of table to link.</param>
        /// <param name="connectionString">Remote table connection information.</param>
        public virtual void CreateLinkTable(string linkName, string tableName, string connectionString)
        {

            // ??? Add the link to the project's XML file
        }

        /// <summary>
        /// Deletes link table from the project.
        /// </summary>
        /// <param name="linkName">Name of link to delete.</param>
        public void DeleteLinkTable(string linkName)
        {
            // ??? Delete the link from the XML file
        }

        /// <summary>
        /// Compares this project against the other and determines if they are same.
        /// </summary>
        /// <param name="other">Epi.Project to compare.</param>
        /// <returns>True/False</returns>
        public virtual bool Equals(Project other)
        {
            return (this.Id == other.Id);
        }

        /// <summary>
        /// Retrieves a list of all code tables.
        /// </summary>
        /// <returns>List of all code tables</returns>
        public DataSets.TableSchema.TablesDataTable GetCodeTableList()
        {
            return CodeData.GetCodeTableList();
        }

        /// <summary>
        /// Returns a list of CodeTableNames
        /// </summary>
        /// <returns>List of code table names.</returns>
        public List<String> GetCodeTableNames()
        {
            DataTable dt = CodeData.GetCodeTableList();
            List<string> list = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                if (dt.Columns.Contains(ColumnNames.TABLE_NAME))
                {
                    list.Add(row[ColumnNames.TABLE_NAME].ToString());
                }
                else
                {
                    list.Add(row[ColumnNames.NAME].ToString());
                }
            }
            return list;
        }

        /// <summary>
        /// Load Views
        /// </summary>
        public virtual void LoadViews()
        {
            if (MetadataSource == MetadataSource.Unknown)
            {
                throw new GeneralException(SharedStrings.ERROR_LOADING_METADATA_UNKNOWN_SOURCE);
            }
            if (MetadataSource != MetadataSource.Xml)
            {
                views = Metadata.GetViews();
            }
            else
            {
                XmlNode viewsNode = GetViewsNode();
                views = Metadata.GetViews(currentViewElement, viewsNode);
            }
        }

        #region OBSOLETE
        //        public void CopyCodeTablesTo(Project destination)
        //        {
        //            List<String> codeTableList = GetCodeTableNames();
        ////            DataTable codeTableList = GetCodeTableList();
        ////            foreach (DataRow codeTableRow in codeTableList.Rows)
        //            foreach (string codeTableName in codeTableList)
        //            {                
        ////                string codeTableName = codeTableRow["TABLE_NAME"].ToString();

        //                // Raise event indicating the copy has begun.
        //                if (TableCopyBeginEvent != null)
        //                {
        //                    TableCopyBeginEvent(this, new MessageEventArgs(codeTableName));
        //                }

        //                DataTable columns = CodeData.GetCodeTableColumnSchema(codeTableName);
        //                string[] columnNames = new string[columns.Rows.Count];
        //                for (int x = 0; x < columns.Rows.Count; x++)
        //                {
        //                    columnNames[x] = columns.Rows[x]["COLUMN_NAME"].ToString();
        //                }
        //                destination.CreateCodeTable(codeTableName, columnNames);

        //                DataTable CodeTable = CodeData.GetCodeTableData(codeTableName);
        //                int rowIndex = 0;
        //                foreach (DataRow CodeRow in CodeTable.Rows)
        //                {
        //                    rowIndex++;
        //                    string[] columnData = new string[columnNames.Length];
        //                    for (int x = 0; x < columnNames.Length; x++)
        //                    {
        //                        columnData[x] = CodeRow[columnNames[x]].ToString();
        //                    }
        //                    destination.Metadata.CreateCodeTableRecord(codeTableName, columnNames, columnData);
        //                    RaiseEventTableCopyStatus(codeTableName, rowIndex);
        //                    // RaiseEventImportStatus(codeTableName + " (" + rowIndex + " reocrds copied)");
        //                }
        //                if (this.TableCopyEndEvent != null)
        //                {
        //                    TableCopyEndEvent(this, new MessageEventArgs(codeTableName));
        //                }
        //            }
        //        }      

        ///// <summary>
        ///// Creates project's relevant databases
        ///// </summary>
        //public void Initialize()
        //{
        //    //this is a hack to ensure that relative file paths are read 
        //    //correctly

        //    string oldCurrentDirectory = Directory.GetCurrentDirectory();
        //    string tempCurrentDirectory = this.Location;

        //    Directory.SetCurrentDirectory(tempCurrentDirectory);

        //    //if (!this.metadataSource.Equals(MetadataSource.Xml))
        //    //{
        //        if (metadata is MetadataDbProvider)
        //        {
        //            if (!string.IsNullOrEmpty(this.MetadataDriver))
        //            {
        //                ((MetadataDbProvider)metadata).Initialize(this.MetaDbInfo, this.MetadataDriver, true);
        //            }
        //        }
        //    //}
        //    //else
        //    //{
        //    //}
        //    if (!UseMetadataDbForCollectedData)
        //    {
        //        collectedData.Initialize(this.collectedDataDbInfo, this.CollectedDataDriver, true);
        //    }
        //    else
        //    {
        //        collectedData.Initialize(this.metaDbInfo, this.MetadataDriver, false);
        //    }

        //    Directory.SetCurrentDirectory(oldCurrentDirectory);
        //}

        // public void SetMetadataDbInfo(string ConnectionString, string driver);
        #endregion OBSOLETE
        #endregion Public Methods

        #region Protected properties
        /// <summary>
        /// Project file extension.
        /// </summary>
        protected virtual string FileExtension
        {
            get
            {
                return Epi.FileExtensions.EPI_PROJ;
            }
        }
        #endregion Protected properties

        #region Protected Methods

        #endregion Protected Methods

        #region Private properties

        private bool IsNew
        {
            get
            {
                return (id.Equals(Guid.Empty));
            }
        }
        #endregion Private properties

        #region Private Methods

        #region Deprecated
        //private void FillXmlDoc()
        //{
        //    XmlNode root = xmlDoc.DocumentElement;
        //    if (IsNew) // This is a newly created project.
        //    {
        //        ApplicationIdentity appId = new ApplicationIdentity(typeof(Configuration).Assembly);
        //        // id = Guid.NewGuid();
        //        id = Util.GetFileGuid(FilePath);
        //        EpiVersion = appId.Version;	 			
        //        createDate = System.DateTime.Now;

        //        // If Metadata Db is used for Collected data, remove the collected data Db node.
        //        if (UseMetadataDbForCollectedData)
        //        {
        //            XmlNode	collectedDataNode = root.SelectSingleNode("/Project/CollectedData");
        //            if (collectedDataNode != null)
        //            {
        //                root.RemoveChild(collectedDataNode);
        //            }
        //        }
        //    }
        //    root.Attributes["id"].Value = Id.ToString();
        //    root.Attributes["name"].Value = Name;
        //    root.Attributes["location"].Value = Location;			
        //    root.Attributes["useMetadataDbForCollectedData"].Value = useMetadataDbForCollectedData.ToString();			
        //    //root.Attributes["databaseFormat"].Value = ((short)DbFormatType).ToString();
        //    root.Attributes["description"].Value = Description ;
        //    root.Attributes["epiVersion"].Value = EpiVersion ;
        //    root.Attributes["createDate"].Value = CreateDate.ToString(CultureInfo.InvariantCulture.DateTimeFormat);
        //    XmlNode metadataDbNode = root.SelectSingleNode("/Project/Metadata/Database");
        //    Metadata.Db.FillXmlDoc(metadataDbNode);
        //    if (UseMetadataDbForCollectedData == false)
        //    {
        //        XmlNode collectedDataDbNode = root.SelectSingleNode("/Project/CollectedData/Database");
        //        CollectedData.FillXmlDoc(collectedDataDbNode);				
        //    }
        //}

        //private void LoadFromXml(XmlNode rootNode)
        //{
        //    //id = new Guid(rootNode.Attributes["id"].Value);
        //    Name = rootNode.Attributes["name"].Value;
        //    Location = rootNode.Attributes["location"].Value;
        //    epiVersion = rootNode.Attributes["epiVersion"].Value;
        //    string createDateString = rootNode.Attributes["createDate"].Value;
        //    createDate = DateTime.Parse(createDateString, CultureInfo.InvariantCulture.DateTimeFormat);
        //    //DbFormatType = (DbFormatType)(short.Parse(rootNode.Attributes["databaseFormat"].Value));
        //    UseMetadataDbForCollectedData = bool.Parse(rootNode.Attributes["useMetadataDbForCollectedData"].Value);			
        //    Description = rootNode.Attributes["description"].Value;

        //    XmlNode	dbNode = rootNode.SelectSingleNode("/Project/Metadata/Database");
        //    string dataDriver = dbNode.Attributes["dataDriver"].Value;
        //    string connString = dbNode.Attributes["connectionString"].Value;

        //    ConnectionStringInfo connInfo = new ConnectionStringInfo(connString);
        //    string fileName = connInfo.DataSource;
        //    // Metadata.Db = DbProvider.GetFileDatabase(fileName);
        //    Metadata.Db = DbProvider.GetDatabaseInstance(dataDriver);
        //    Metadata.Db.ConnectionString = connString;

        //    if (this.UseMetadataDbForCollectedData == false)
        //    {
        //        dbNode = rootNode.SelectSingleNode("/Project/CollectedData/Database");
        //        dataDriver = dbNode.Attributes["dataDriver"].Value;
        //        connString = dbNode.Attributes["connectionString"].Value;
        //        // connInfo = new ConnectionStringInfo(connString);
        //        // fileName = connInfo.DataSource;
        //        CollectedData = DbProvider.GetDatabaseInstance(dataDriver);
        //        CollectedData.ConnectionString = connString;
        //    }
        //}
        #endregion

        /// <summary>
        /// Get the metadata node
        /// </summary>
        /// <returns>Xml node</returns>
        public XmlNode GetMetadataNode()
        {
            XmlNode metadataNode = xmlDoc.DocumentElement.SelectSingleNode("/Project/Metadata");
            return metadataNode;
        }

        private XmlNode GetViewsNode()
        {
            //            return xmlDoc.DocumentElement.SelectSingleNode("/Project/Views");
            return xmlDoc.DocumentElement.SelectSingleNode("/Project/Metadata/Views");
        }

        private XmlNode GetFieldsNode()
        {
            return xmlDoc.DocumentElement.SelectSingleNode("/Project/Metadata/Views/View/Fields");
        }

        /// <summary>
        /// Validates the XML doc read. Looks for schema differences and rejects if the schema is out of date.
        /// </summary>
        private void ValidateXmlDoc()
        {
            // Check schema version. If the schema is old, can't read the project.
            if (xmlDoc.DocumentElement.HasAttribute("schemaVersion"))
            {
                int schemaVersion = int.Parse(xmlDoc.DocumentElement.Attributes["schemaVersion"].Value.ToString());
                if (schemaVersion < currentSchemaVersion)
                {
                    throw new GeneralException(SharedStrings.PROJECT_SCHEMA_OUT_OF_DATE);
                }
            }
            else
            {
                throw new GeneralException(SharedStrings.PROJECT_SCHEMA_OUT_OF_DATE);
            }
        }

        /// <summary>
        /// Gets the Pages Node of the Project file
        /// </summary>
        /// <returns></returns>
        private XmlNode GetPagesNode()
        {
            return xmlDoc.DocumentElement.SelectSingleNode("/Project/Views/View/Pages");
        }

        private String SetOleDbDatabaseFilePath(string pConnectionString)
        {
            System.Data.OleDb.OleDbConnectionStringBuilder connectionBuilder = new System.Data.OleDb.OleDbConnectionStringBuilder(pConnectionString);

            connectionBuilder.DataSource = this.FilePath.Replace(".prj", ".mdb");

            return connectionBuilder.ToString();

        }

        #endregion Private Methods
    }
}
