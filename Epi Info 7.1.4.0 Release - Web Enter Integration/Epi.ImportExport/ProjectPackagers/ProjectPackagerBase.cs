#region Using
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using Epi;
using Epi.Core;
using Epi.Data;
#endregion // Using

namespace Epi.ImportExport
{
    /// <summary>
    /// A base class for project packager classes. Project packagers are designed to de-identify data, filter records
    /// in the data, compress the data, and then encrypt the data for transport across untrusted and/or unsecured mediums.
    /// The packager will create a copy of the project. MS Access is the database type for the copy that will reside in
    /// the package file. The interface outlines the public methods and properties that packager classes must implement.
    /// -- E. Knudsen, 2012
    /// </summary>
    /// <remarks>
    /// This class is a base class. A class hierarchy has been implemented to handle the different types of databases that
    /// may be used underneath the project layer. For example, different things may be needed if the underlying database
    /// is MS SQL server versus if it's Microsoft Access or MySQL. However, all resulting packages are in MS Access format.
    /// Implementing different destination data formats can be done by slightly modifying the GenericProjectPackager class 
    /// and modifying the FormDataImporter and FormDataCopier classes.
    /// </remarks>
    public abstract class ProjectPackagerBase : IProjectPackager
    {
        #region Private Members
        private string customSalt;
        private string customInitVector;
        private int customIterations;
        private bool includeCodeTables = false;
        private bool includeGridData = true;
        private bool appendTimestamp = false;
        private string packageName;
        private FormInclusionType formInclusionType = FormInclusionType.AllDescendants;
        #endregion //Private Members

        #region Protected Members
        protected Project sourceProject;
        protected Project destinationProject;
        protected string formName;
        protected string packagePath;
        protected string password;
        protected Configuration config;
        protected Dictionary<string, List<string>> columnsToNull;
        protected Dictionary<string, List<string>> gridColumnsToNull;
        protected string rowFilter;
        protected Query selectQuery;
        #endregion // Protected Members

        #region Events
        public event SetProgressBarDelegate SetProgressBar;
        public event UpdateStatusEventHandler SetStatus;
        public event UpdateStatusEventHandler AddStatusMessage;
        public event SetMaxProgressBarValueDelegate SetMaxProgressBarValue;
        #endregion // Events

        #region Constructors
        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="sourceProject">The project from which to create the package.</param>
        /// <param name="packagePath">The file path to where the package will reside.</param>
        /// <param name="formName">The name of the form within the project to package.</param>
        /// <param name="password">The password for the encryption.</param>
        public ProjectPackagerBase(Project sourceProject, string packagePath, string formName, string password)
        {
            this.sourceProject = sourceProject;
            this.packagePath = packagePath;
            this.formName = formName;
            this.password = password;
            this.columnsToNull = new Dictionary<string, List<string>>();
            this.gridColumnsToNull = new Dictionary<string, List<string>>();
            this.config = Configuration.GetNewInstance();
        }

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="sourceProject">The project from which to create the package.</param>
        /// <param name="columnsToNull">The columns in the table to null</param>
        /// <param name="gridColumnsToNull">The grid columns to null</param>
        /// <param name="packagePath">The file path to where the package will reside.</param>
        /// <param name="formName">The name of the form within the project to package.</param>
        /// <param name="password">The password for the encryption.</param>
        public ProjectPackagerBase(Project sourceProject, Dictionary<string, List<string>> columnsToNull, Dictionary<string, List<string>> gridColumnsToNull, string packagePath, string formName, string password)
        {
            this.sourceProject = sourceProject;
            this.packagePath = packagePath;
            this.formName = formName;
            this.password = password;
            this.columnsToNull = columnsToNull;
            this.gridColumnsToNull = gridColumnsToNull;
            this.config = Configuration.GetNewInstance();
        }
        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Gets/sets whether the package file name should have a timestamp appended to it.
        /// </summary>
        public bool AppendTimestamp
        {
            get
            {
                return this.appendTimestamp;
            }
            set
            {
                this.appendTimestamp = value;
            }
        }

        /// <summary>
        /// Gets/sets the name of the resulting package file.
        /// </summary>
        public string PackageName
        {
            get
            {
                return this.packageName;
            }
            set
            {
                this.packageName = value;
            }
        }

        /// <summary>
        /// Gets/sets whether or not to include descendant (related) forms. See the FormInclusionType.cs file for
        /// more details on the underlying data type.
        /// </summary>
        /// <remarks>
        /// This setting is used to determine whether related form data are included or not, and if so, to what level
        /// such form data should be included. This setting is useful if the sender of the data package has many child
        /// forms and doesn't feel the need to include all of that related data due to file size limitations, or
        /// perhaps that data is irrelevant to the recipient.
        /// </remarks>
        public FormInclusionType FormInclusionType
        {
            get
            {
                return this.formInclusionType;
            }
            set
            {
                this.formInclusionType = value;
            }
        }

        /// <summary>
        /// Gets/sets whether the package should include data for the code tables.
        /// </summary>
        /// <remarks>
        /// The word 'code table' refers to data tables within the database that store the possible
        /// choices users can make in drop-down list fields. For example, a drop-down list field for
        /// 'Gender' would typically have its values stored in a table called 'codeGender'. Including
        /// the code table data can be useful in determining differences between forms during the
        /// import process, but can also increase the file size if there happen to be a lot of code
        /// tables in the database.
        /// </remarks>
        public bool IncludeCodeTables
        {
            get
            {
                return this.includeCodeTables;
            }
            set
            {
                this.includeCodeTables = value;
            }
        }

        /// <summary>
        /// Gets/sets whether or not to include grid data.
        /// </summary>
        /// <remarks>
        /// Grid Data in this context refers to any data table in the database that stores information
        /// for Grid fields (see GridField.cs in Epi.Core). Grids may be used to store various types of
        /// information that may not be relevant to whomever is being sent the data package, and this
        /// setting allows that data to be excluded in order to space.
        /// </remarks>
        public bool IncludeGridData
        {
            get
            {
                return this.includeGridData;
            }
            set
            {
                this.includeGridData = value;
            }
        }

        /// <summary>
        /// Gets the source project associated with this instance of the packager.
        /// </summary>
        public Project SourceProject 
        {
            get
            {
                return this.sourceProject;
            }
        }

        /// <summary>
        /// Gets the path to the encrypted package file.
        /// </summary>
        public string PackagePath
        {
            get
            {
                return this.packagePath;
            }
        }

        /// <summary>
        /// Gets the name of the form to package.
        /// </summary>
        public string FormName
        {
            get
            {
                return this.formName;
            }
        }

        /// <summary>
        /// Sets the columns to null;
        /// </summary>
        public Dictionary<string, List<string>> ColumnsToNull
        {
            get
            {
                return this.columnsToNull;
            }
            set
            {
                this.columnsToNull = value;
            }
        }

        /// <summary>
        /// Sets the grid columns to null;
        /// </summary>
        public Dictionary<string, List<string>> GridColumnsToNull
        {
            get
            {
                return this.gridColumnsToNull;
            }
            set
            {
                this.gridColumnsToNull = value;
            }
        }

        /// <summary>
        /// Gets/sets the selection criteria for row filters
        /// </summary>
        public Query SelectQuery
        {
            get
            {
                return this.selectQuery;
            }
            set
            {
                this.selectQuery = value;
            }
        }
        #endregion // Public Properties

        #region Public Methods
        /// <summary>
        /// Sets custom parameters for encrypting the package.
        /// </summary>
        /// <param name="salt">The user-specified salt value</param>
        /// <param name="initVector">The user-specified initialization vector</param>
        /// <param name="iterations">The number of password iterations</param>
        public void SetCustomEncryptionParameters(string salt, string initVector, int iterations)
        {
            if (salt.Length != 32)
            {
                throw new ApplicationException(ImportExportSharedStrings.ERROR_SALT_LENGTH);
            }

            else if (initVector.Length != 16)
            {
                throw new ApplicationException(ImportExportSharedStrings.ERROR_INIT_VECTOR_LENGTH);
            }

            else if (iterations < 1 || iterations > 100)
            {
                throw new ApplicationException(ImportExportSharedStrings.ERROR_ITERATIONS_COUNT);
            }

            this.customSalt = salt;
            this.customInitVector = initVector;
            this.customIterations = iterations;
        }

        /// <summary>
        /// Packages the project for transport
        /// </summary>
        /// <returns>bool</returns>
        public bool PackageProject()
        {
            CheckForProblems();

            bool success = false;

            success = CreateProjectCopy();
            success = CompressProject();
            success = EncryptPackage();

            try
            {
                if (File.Exists(destinationProject.CollectedData.DataSource))
                {
                    File.Delete(destinationProject.CollectedData.DataSource);
                }

                if (File.Exists(destinationProject.CollectedData.DataSource + ".gz"))
                {
                    File.Delete(destinationProject.CollectedData.DataSource + ".gz");
                }
            }
            catch
            {
            }

            return success;
        }
        #endregion // Public Methods

        #region Private Methods
        /// <summary>
        /// Checks for problems
        /// </summary>
        private void CheckForProblems()
        {
            View sourceView = sourceProject.Views[FormName];
            IDbDriver driver = sourceProject.CollectedData.GetDatabase();

            // Check #1 - Make sure the base table exists and that it has a Global Record Id field, Record status field, and Unique key field.
            DataTable dt = driver.GetTableData(sourceView.TableName, "GlobalRecordId, RECSTATUS, UniqueKey");
            int baseTableRowCount = dt.Rows.Count;

            // Check #2a - Make sure GlobalRecordId is a string.
            if (!dt.Columns[0].DataType.ToString().Equals("System.String"))
            {
                throw new ApplicationException(ImportExportSharedStrings.ERROR_PACKAGER_CHECK_INVALID_GUID_COLUMN);
            }

            // Check #2b - Make sure RECSTATUS is a number
            if (!(dt.Columns[1].DataType.ToString().Equals("System.Byte") || dt.Columns[1].DataType.ToString().Equals("System.Int16") || dt.Columns[1].DataType.ToString().Equals("System.Int32")))
            {
                throw new ApplicationException(ImportExportSharedStrings.ERROR_PACKAGER_CHECK_INVALID_RECSTATUS_COLUMN);
            }

            // Check #3 - Make sure GlobalRecordId values haven't been replaced with something that isn't actually a GUID. 
            //      For performance reasons only the first few values are checked.
            if (baseTableRowCount >= 1)
            {
                string value = dt.Rows[0][0].ToString();
                System.Guid guid = new Guid(value);

                if (baseTableRowCount >= 30)
                {
                    for (int i = 0; i < 30; i++)
                    {
                        value = dt.Rows[i][0].ToString();
                        guid = new Guid(value);
                    }
                }
            }

            // Check #4a - See if global record ID values are distinct on the base table.
            Query selectDistinctQuery = driver.CreateQuery("SELECT DISTINCT [GlobalRecordId] FROM [" + sourceView.TableName + "]");
            DataTable distinctTable = driver.Select(selectDistinctQuery);
            if (distinctTable.Rows.Count != baseTableRowCount)
            {
                throw new ApplicationException(ImportExportSharedStrings.ERROR_PACKAGER_CHECK_GUID_NOT_UNIQUE);
            }

            // Check #4b - See if global record ID values are distinct on each page table.
            foreach (Page page in sourceView.Pages)
            {
                selectDistinctQuery = driver.CreateQuery("SELECT DISTINCT [GlobalRecordId] FROM [" + page.TableName + "]");
                distinctTable = driver.Select(selectDistinctQuery);
                if (distinctTable.Rows.Count != baseTableRowCount)
                {
                    throw new ApplicationException(string.Format(ImportExportSharedStrings.ERROR_PACKAGER_CHECK_GUID_NOT_UNIQUE_PAGE, page.TableName));
                }
            }

            // Check #5 - Make sure RECSTATUS has valid values.
            selectDistinctQuery = driver.CreateQuery("SELECT DISTINCT [RecStatus] FROM [" + sourceView.TableName + "]");
            distinctTable = driver.Select(selectDistinctQuery);
            foreach (DataRow row in distinctTable.Rows)
            {
                if (!row[0].ToString().Equals("1") && !row[0].ToString().Equals("0"))
                {
                    throw new ApplicationException(ImportExportSharedStrings.ERROR_PACKAGER_CHECK_RECSTATUS_VALUES_INVALID);
                }
            }

            // Check #6 - Make sure nobody has used a form's table as a code table for drop-down lists
            List<string> dataTableNames = new List<string>();
            foreach (View view in sourceProject.Views)
            {
                dataTableNames.Add(view.TableName);
                foreach (Page page in view.Pages)
                {
                    dataTableNames.Add(page.TableName);
                }
            }
            DataTable codeTableNames = driver.GetCodeTableNamesForProject(sourceProject);
            foreach (DataRow row in codeTableNames.Rows)
            {
                if (codeTableNames.Columns.Count >= 2)
                {
                    if (dataTableNames.Contains(row[2].ToString()))
                    {
                        throw new ApplicationException(ImportExportSharedStrings.ERROR_PACKAGER_CHECK_CODE_TABLE_FORM);
                    }
                }
                else
                {
                    if (dataTableNames.Contains(row[0].ToString()))
                    {
                        throw new ApplicationException(ImportExportSharedStrings.ERROR_PACKAGER_CHECK_CODE_TABLE_FORM);
                    }
                }
            }

            // Check #7 - Should never get here because the UI should prevent it, but do a check just in case
            if (sourceView.IsRelatedView == true)
            {
                throw new ApplicationException(ImportExportSharedStrings.ERROR_PACKAGER_CHECK_RELATED_FORM);
            }

            distinctTable = null;
            selectDistinctQuery = null;
            driver.Dispose();
            driver = null;
        }

        /// <summary>
        /// Constructs the object
        /// </summary>
        private void Construct()
        {
            config = Configuration.GetNewInstance();
        }
        #endregion // Private Methods

        #region Protected Methods
        /// <summary>
        /// Set column data to null as specified by the user
        /// </summary>
        /// <returns>bool</returns>
        protected virtual bool RemoveColumnData()
        {
            return false;
        }

        /// <summary>
        /// Set row data to null as specified by the user
        /// </summary>
        /// <returns>bool</returns>
        protected virtual bool RemoveRowData()
        {
            return false;
        }

        /// <summary>
        /// Adds a status message
        /// </summary>
        /// <param name="message">The message</param>
        protected virtual void OnAddStatusMessage(string message)
        {
            if (AddStatusMessage != null)
            {
                AddStatusMessage(message);                
            }
        }

        /// <summary>
        /// Sets status message
        /// </summary>
        /// <param name="message">The message</param>
        protected virtual void OnSetStatusMessage(string message)
        {
            if (AddStatusMessage != null && SetStatus != null)
            {
                AddStatusMessage(message);
                SetStatus(message);
            }
        }

        /// <summary>
        /// Sets progess bar value
        /// </summary>
        /// <param name="progress">The progress</param>
        protected virtual void OnSetProgress(double progress)
        {
            if (SetProgressBar != null)
            {
                SetProgressBar(progress);
            }
        }

        /// <summary>
        /// Sets max attainable progess bar value
        /// </summary>
        /// <param name="maxProgress">The max value to set</param>
        protected virtual void OnSetMaxProgressValue(double maxProgress)
        {
            if (SetMaxProgressBarValue != null)
            {
                SetMaxProgressBarValue(maxProgress);
            }
        }

        /// <summary>
        /// Compresses the copied project.
        /// </summary>
        /// <returns>bool</returns>
        protected bool CompressProject()
        {
            try
            {
                OnSetStatusMessage(ImportExportSharedStrings.PACKAGE_COMPRESSION_START);

                string destinationFileName = destinationProject.CollectedData.DataSource;
                FileInfo fi = new FileInfo(destinationFileName);
                ImportExportHelper.CompressDataPackage(fi);
                OnSetStatusMessage(ImportExportSharedStrings.PACKAGE_COMPRESSION_END);
            }
            catch
            {
                OnSetStatusMessage(ImportExportSharedStrings.PACKAGE_COMPRESSION_FAIL);
                return false;
            }

            return true;
        }        

        /// <summary>
        /// Encrypts the package.
        /// </summary>
        /// <returns>bool</returns>
        protected bool EncryptPackage()
        {
            OnSetStatusMessage(ImportExportSharedStrings.PACKAGE_ENCRYPTION_START);
            string inputFileName = destinationProject.CollectedData.DataSource + ".gz";

            FileInfo fi = new FileInfo(destinationProject.FilePath);
            string packageName = this.PackageName;

            // Check to see if permanent variables are referenced in the package name, and if so, replace the
            // variable name with its value. The @@ prefix is what specifies a permanent variable.
            if (packageName.Contains("@@"))
            {
                // TODO: @@Name and @@NameFirst, if both are used, may have some logic issues regarding getting replaced right... look into this
                System.Data.DataRow[] result = config.PermanentVariables.Select();
                foreach(System.Data.DataRow row in result) 
                {
                    if(packageName.Contains("@@" + (((DataSets.Config.PermanentVariableRow)row).Name))) 
                    {
                        packageName = packageName.Replace("@@" + (((DataSets.Config.PermanentVariableRow)row).Name),
                            (((DataSets.Config.PermanentVariableRow)row).DataValue));
                    }
                }
            }

            string outputFileName = fi.Directory.FullName + "\\" + packageName; // destinationProject.CollectedData.DataSource.Substring(0, destinationProject.CollectedData.DataSource.Length - 4) + ".edp7";
            
            if (AppendTimestamp)
            {
                // Append timestamp in UTC so that people sending packages across time zones don't need to worry about
                // what zone it was packaged in.
                DateTime dt = DateTime.UtcNow;
                string dateDisplayValue = string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:s}", dt);
                dateDisplayValue = dateDisplayValue.Replace(':', '-'); // The : must be replaced otherwise the encryption fails
                outputFileName = outputFileName + "_" + dateDisplayValue;
            }

            outputFileName = outputFileName + ".edp7";

            if (string.IsNullOrEmpty(customInitVector) || string.IsNullOrEmpty(customSalt))
            {
                Configuration.EncryptFile(inputFileName, outputFileName, password);
            }
            else
            {
                Configuration.EncryptFile(inputFileName, outputFileName, password, customInitVector, customSalt, customIterations);
            }

            OnSetStatusMessage(ImportExportSharedStrings.PACKAGE_ENCRYPTION_END);
            return true;
        }

        /// <summary>
        /// Creates the project copy for packaging. Override in dervied classes to handle different methods
        /// of copying the project for different data drivers.
        /// </summary>
        /// <returns>bool</returns>
        protected abstract bool CreateProjectCopy();        

        /// <summary>
        /// Creates a project in memory that will represent the destination data store.
        /// </summary>
        /// <returns>Project; represents the destination data store.</returns>
        protected Project CreateProject()
        {
            OnSetStatusMessage(ImportExportSharedStrings.PRJ_FILE_COPY_START);

            DbDriverInfo collectedDataDBInfo = new DbDriverInfo();
            IDbDriverFactory collectedDBFactory = DbDriverFactoryCreator.GetDbDriverFactory("Epi.Data.Office.AccessDBFactory, Epi.Data.Office");// GetSelectedCollectedDataDriver();
            string GUID = System.Guid.NewGuid().ToString();
            collectedDataDBInfo.DBCnnStringBuilder = collectedDBFactory.RequestDefaultConnection(GUID, GUID);
            //collectedDataDBInfo.DBCnnStringBuilder = collectedDBFactory.RequestNewConnection(txtCollectedData.Text.Trim());

            Project project = new Project();
            project.Name = SourceProject.Name;

            project.Location = PackagePath; //Path.Combine(txtProjectLocation.Text, txtProjectName.Text);

            if (collectedDataDBInfo.DBCnnStringBuilder.ContainsKey("Provider") && collectedDataDBInfo.DBCnnStringBuilder["Provider"].ToString().ToLower().Equals("microsoft.jet.oledb.4.0"))
            {
                collectedDataDBInfo.DBCnnStringBuilder["Data Source"] = project.FilePath.Substring(0, project.FilePath.Length - 4) + ".mdb";
            }

            if (!Directory.Exists(project.Location))
            {
                Directory.CreateDirectory(project.Location);
            }

            project.Id = project.GetProjectId();
            project.Description = SourceProject.Description;

            // Collected data ...
            project.CollectedDataDbInfo = collectedDataDBInfo;
            project.CollectedDataDriver = "Epi.Data.Office.AccessDBFactory, Epi.Data.Office";
            project.CollectedDataConnectionString = collectedDataDBInfo.DBCnnStringBuilder.ToString();
            try
            {
                project.CollectedData.Initialize(collectedDataDBInfo, "Epi.Data.Office.AccessDBFactory, Epi.Data.Office", true);
            }
            catch (Exception)
            {
                //MsgBox.ShowException(ex);
                return null;
            }

            Logger.Log(DateTime.Now + ":  " + string.Format(ImportExportSharedStrings.PROJECT_PACKAGE_CREATED_BY_USER, project.Name, project.Location, System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString()));

            // Metadata ..
            project.MetadataSource = MetadataSource.SameDb;

            try
            {
                //project.Save();
                OnSetStatusMessage(ImportExportSharedStrings.PRJ_FILE_COPY_END);
                return project;
            }
            catch (UnauthorizedAccessException)
            {                
                return null;
            }
        }
        #endregion // Protected Methods

        #region StaticMethods

        //  Call this function to remove the key from memory after use for security
        [System.Runtime.InteropServices.DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr Destination, int Length);

        #endregion // Static Methods
    }
}
