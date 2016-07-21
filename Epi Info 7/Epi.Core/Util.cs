using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Runtime.Remoting;
using System.Security.Cryptography;
using Epi;
using Epi.Fields;
using Epi.Data;

namespace Epi
{


    /// <summary>
    /// We need to find a way to organize these methods to reduce the possibility
    /// of "Spaghetti Code" like maintenance problems. Please do not add any more members
    /// to this class. Thanks.
    /// </summary>
    public static class Util
    {

        #region public static properties
        private static DateTime nullDateTime = new DateTime();

        /// <summary>
        /// Gets a null value DateTime object.
        /// </summary>
        public static DateTime NullDateTime
        {
            get{return nullDateTime;}
        }
        #endregion public static properties

        #region Public static methods

        /// <summary>
        /// Determines whether a given field name is valid.
        /// </summary>
        /// <returns>bool</returns>
        public static bool IsValidFieldName(string fieldName, ref string errorMessage)
        {
            if (string.IsNullOrEmpty(fieldName.Trim()))
            {
                return false;
            }

            string strTestForSymbols = fieldName;
            Regex regex = new Regex("[\\w\\d]", RegexOptions.IgnoreCase);
            string strResultOfSymbolTest = regex.Replace(strTestForSymbols, string.Empty);

            if (strResultOfSymbolTest.Length > 0)
            {
                errorMessage = (string.Format(SharedStrings.INVALID_CHARS_IN_FIELD_NAME, strResultOfSymbolTest));
                return false;
            }
            else
            {
                if (fieldName.Length > 64)
                {
                    errorMessage = SharedStrings.FIELD_NAME_TOO_LONG;
                    return false;
                }

                if (!Util.IsFirstCharacterALetter(fieldName))
                {
                    errorMessage = SharedStrings.FIELD_NAME_BEGIN_NUMERIC;
                    return false;
                }

                if (Epi.Data.Services.AppData.Instance.IsReservedWord(fieldName))
                {
                    errorMessage = SharedStrings.FIELD_NAME_IS_RESERVED;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns true if the given database is a valid Epi Info 7 project.
        /// </summary>
        /// <param name="connectionInfo">The connection information to the database.</param>
        /// <param name="isConnectionString">Whether the connection info is a connection string</param>
        /// <returns>bool</returns>
        public static bool IsDatabaseEpiProject(string connectionInfo, bool isConnectionString)
        {
            //if (fileName.ToLowerInvariant().EndsWith(".mdb"))
            //{
            IDbDriver db = DBReadExecute.GetDataDriver(connectionInfo, isConnectionString);
            bool isProject = true;

            List<string> metaTableNames = new List<string>();

            metaTableNames.Add("metaBackgrounds");
            metaTableNames.Add("metaDataTypes");
            metaTableNames.Add("metaDbInfo");
            metaTableNames.Add("metaFields");
            metaTableNames.Add("metaFieldTypes");
            metaTableNames.Add("metaImages");
            metaTableNames.Add("metaLinks");
            metaTableNames.Add("metaPages");
            metaTableNames.Add("metaViews");

            foreach (string tableName in metaTableNames)
            {
                if (!db.TableExists(tableName))
                {
                    isProject = false;
                }
            }

            List<string> metaViewsColumnNames = db.GetTableColumnNames("metaViews");

            if (
                !metaViewsColumnNames.Contains("ViewId") ||
                !metaViewsColumnNames.Contains("Name") ||
                !metaViewsColumnNames.Contains("IsRelatedView") ||
                !metaViewsColumnNames.Contains("CheckCode") ||
                !metaViewsColumnNames.Contains("Width") ||
                !metaViewsColumnNames.Contains("Height") ||
                !metaViewsColumnNames.Contains("Orientation")
                )
            {
                isProject = false;
            }

            List<string> metaPagesColumnNames = db.GetTableColumnNames("metaPages");

            if (
                !metaPagesColumnNames.Contains("PageId") ||
                !metaPagesColumnNames.Contains("Name") ||
                !metaPagesColumnNames.Contains("Position") ||
                !metaPagesColumnNames.Contains("BackgroundId") ||
                !metaPagesColumnNames.Contains("ViewId")
                )
            {
                isProject = false;
            }

            List<string> metaFieldsColumnNames = db.GetTableColumnNames("metaFields");

            if (
                !metaFieldsColumnNames.Contains("FieldId") ||
                !metaFieldsColumnNames.Contains("UniqueId") ||
                !metaFieldsColumnNames.Contains("Name") ||
                !metaFieldsColumnNames.Contains("PromptText") ||
                !metaFieldsColumnNames.Contains("ControlFontFamily") ||
                !metaFieldsColumnNames.Contains("ControlFontSize") ||
                !metaFieldsColumnNames.Contains("ControlFontStyle") ||
                !metaFieldsColumnNames.Contains("ControlTopPositionPercentage") ||
                !metaFieldsColumnNames.Contains("ControlLeftPositionPercentage") ||
                !metaFieldsColumnNames.Contains("ControlHeightPercentage") ||
                !metaFieldsColumnNames.Contains("ControlWidthPercentage") ||
                !metaFieldsColumnNames.Contains("TabIndex") ||
                !metaFieldsColumnNames.Contains("HasTabStop") ||
                !metaFieldsColumnNames.Contains("Lower") ||
                !metaFieldsColumnNames.Contains("Upper") ||
                !metaFieldsColumnNames.Contains("Pattern") ||
                !metaFieldsColumnNames.Contains("PageId") ||
                !metaFieldsColumnNames.Contains("ViewId")
                )
            {

                isProject = false;
            }
            //}

            db.Dispose();
            db = null;

            return isProject;
        }

        /// <summary>
        /// Creates a project file and project object for a database file.
        /// </summary>
        /// <param name="databaseFileName">The database file name.</param>        
        /// <param name="createPRJFile">Whether or not to create the PRJ file on disk.</param>        
        /// <returns>An Epi Info project object</returns>
        public static Project CreateProjectFileFromDatabase(string databaseFileName, bool createPRJFile)
        {
            if (IsDatabaseEpiProject(databaseFileName, false) == false)
            {
                return null;
            }

            Project project = new Project();

            string fileName = databaseFileName;
            string directory = Path.GetDirectoryName(fileName);

            if (fileName.ToLowerInvariant().EndsWith(".mdb"))
            {
                DbDriverInfo collectedDataDBInfo = new DbDriverInfo();
                IDbDriverFactory collectedDBFactory = DbDriverFactoryCreator.GetDbDriverFactory("Epi.Data.Office.AccessDBFactory, Epi.Data.Office");// GetSelectedCollectedDataDriver();
                string GUID = System.Guid.NewGuid().ToString();
                collectedDataDBInfo.DBCnnStringBuilder = collectedDBFactory.RequestDefaultConnection(GUID, GUID);

                FileInfo fi = new FileInfo(fileName);

                project.Name = fi.Name.Substring(0, fi.Name.Length - 4);
                project.Location = directory;

                if (collectedDataDBInfo.DBCnnStringBuilder.ContainsKey("Provider"))
                {
                    collectedDataDBInfo.DBCnnStringBuilder["Data Source"] = project.FilePath.Substring(0, project.FilePath.Length - 4) + ".mdb";
                }

                if (!Directory.Exists(project.Location))
                {
                    Directory.CreateDirectory(project.Location);
                }

                project.Id = project.GetProjectId();
                project.Description = string.Empty;

                project.CollectedDataDbInfo = collectedDataDBInfo;
                project.CollectedDataDriver = "Epi.Data.Office.AccessDBFactory, Epi.Data.Office";
                project.CollectedDataConnectionString = collectedDataDBInfo.DBCnnStringBuilder.ToString();

                Logger.Log(DateTime.Now + ":  " + string.Format("Project [{0}] created in {1} by user [{2}].", project.Name, project.Location, System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString()));

                project.MetadataSource = MetadataSource.SameDb;

                if (createPRJFile)
                {
                    try
                    {
                        project.Save();
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        return null;
                    }
                }          
            }

            return project;
        }

        /// <summary>
        /// Creates a project file and project object for a given database.
        /// </summary>
        /// <param name="connectionString">The database file name or connection string.</param>        
        /// <param name="createPRJFile">Whether or not to create the PRJ file on disk.</param>
        /// <param name="prjFileName">The name of the desired PRJ file</param>
        /// <param name="prjFileLocation">The directory to the desired PRJ file</param>
        /// <returns>An Epi Info project object</returns>
        public static Project CreateProjectFileFromDatabase(string connectionString, bool createPRJFile, string prjFileLocation = "", string prjFileName = "")
        {
            if (IsDatabaseEpiProject(connectionString, true) == false)
            {
                return null;
            }

            Project project = new Project();

            DbDriverInfo collectedDataDBInfo = new DbDriverInfo();
            IDbDriverFactory collectedDBFactory = DbDriverFactoryCreator.GetDbDriverFactory("Epi.Data.SqlServer.SqlDBFactory, Epi.Data.SqlServer");
            string GUID = System.Guid.NewGuid().ToString();
            //collectedDataDBInfo.DBCnnStringBuilder = collectedDBFactory.RequestNewConnection(connectionInfo);

            string[] pieces = connectionString.Split(';');

            foreach (string piece in pieces)
            {
                if (!piece.ToLowerInvariant().StartsWith("password"))
                {
                    string[] parts = piece.Split('=');
                    collectedDataDBInfo.DBCnnStringBuilder[parts[0]] = parts[1];
                }
                else
                {
                    string[] parts = piece.Split('=');
                    int indexOf = piece.IndexOf("=");
                    string password = piece.Substring(indexOf + 1);
                    password = password.TrimStart('\"').TrimEnd('\"'); ;
                    collectedDataDBInfo.DBCnnStringBuilder[parts[0]] = password;
                }
            }
                                
            IDbDriver driver = collectedDBFactory.CreateDatabaseObject(collectedDataDBInfo.DBCnnStringBuilder);
            project.CollectedData.Initialize(collectedDataDBInfo, "Epi.Data.SqlServer.SqlDBFactory, Epi.Data.SqlServer", false);

            project.Name = prjFileName; //fi.Name.Substring(0, fi.Name.Length - 4);
            project.Location = prjFileLocation; //directory;

            project.Id = project.GetProjectId();
            project.Description = string.Empty;

            project.CollectedDataDbInfo = collectedDataDBInfo;
            project.CollectedDataDriver = "Epi.Data.SqlServer.SqlDBFactory, Epi.Data.SqlServer";
            project.CollectedDataConnectionString = connectionString;            

            project.MetadataSource = MetadataSource.SameDb;
            project.Metadata.AttachDbDriver(project.CollectedData.GetDbDriver());

            if (createPRJFile)
            {
                try
                {
                    project.Save();
                }
                catch (UnauthorizedAccessException ex)
                {
                    return null;
                }
            }            

            return project;
        }

        /// <summary>
        /// Recursive function used to find if a view is a descendant of a given parent
        /// </summary>
        /// <param name="view">The view to check</param>
        /// <param name="parentView">The parent view</param>
        /// <returns>bool</returns>
        public static bool IsFormDescendant(View view, View parentView)
        {
            if (view.ParentView == null)
            {
                if (view.Name == parentView.Name)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (view.ParentView.Name == parentView.Name)
            {
                return true;
            }
            else
            {
                return Util.IsFormDescendant(view.ParentView, parentView);
            }
        }

        public static bool DoesDataSourceExistForProject(Project project)
        {
            IDbDriver db = project.CollectedData.GetDatabase();
            if (db.FullName.Trim().StartsWith("[MS Access]", StringComparison.OrdinalIgnoreCase))
            {
                string databasePath = db.DataSource.StartsWith("data source=") ? db.DataSource.Substring(12) : db.DataSource;

                if (!System.IO.File.Exists(databasePath))
                {
                    return false;
                }
            }
            else if (db.ToString().Contains("Sql"))
            {
                try
                {
                    foreach (View view in project.Views)
                    {

                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if string is a valid file path
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <returns>True if is a file path; false if is not a file path.</returns>
        public static bool IsFilePath(string connectionString)
        {
            string filepath = connectionString;
            try
            {
                char[] invalidChars = Path.GetInvalidPathChars();
                string pattern = String.Join("|", invalidChars);
                Match match = new Regex(pattern).Match(filepath);

                if (match.Success == false && File.Exists(filepath))
                {
                    return true;
                }
                else
                {
                    if (!filepath.Contains("Provider="))
                    {
                        Regex fileRegex = new Regex("^(([a-zA-Z]:|\\\\)\\\\)?(((\\.)|(\\.\\.)|([^\\\\/:\\*\\?\"\\|<>\\. ](([^\\\\/:\\*\\?\"\\|<>\\. ])|([^\\\\/:\\*\\?\"\\|<>]*[^\\\\/:\\*\\?\"\\|<>\\. ]))?))\\\\)*[^\\\\/:\\*\\?\"\\|<>\\. ](([^\\\\/:\\*\\?\"\\|<>\\. ])|([^\\\\/:\\*\\?\"\\|<>]*[^\\\\/:\\*\\?\"\\|<>\\. ]))?$");
                        match = fileRegex.Match(filepath);
                        return match.Success;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Orders the columns in a data table by their corresponding tab order
        /// </summary>        
        /// <param name="table">The table whose columns should be re-ordered</param>
        /// <param name="view">The meta data from which to pull the tab order</param>
        public static void SortColumnsByTabOrder(DataTable table, View view)
        {
            Dictionary<string, int> columnNames = new Dictionary<string, int>();
            int runningTabIndex = 0;

            columnNames.Add("UniqueKey", runningTabIndex);
            runningTabIndex++;
            columnNames.Add("GlobalRecordId", runningTabIndex);
            runningTabIndex++;
            columnNames.Add("RecStatus", runningTabIndex);
            runningTabIndex++;

            if (view.IsRelatedView)
            {
                columnNames.Add("FKEY", runningTabIndex);
                runningTabIndex++;
            }

            foreach (Page page in view.Pages)
            {
                SortedDictionary<double, string> fieldsOnPage = new SortedDictionary<double, string>();

                foreach (Field field in page.Fields)
                {
                    if (field is RenderableField && field is IDataField)
                    {
                        RenderableField renderableField = field as RenderableField;
                        double tabIndex = renderableField.TabIndex;
                        while (fieldsOnPage.ContainsKey(tabIndex))
                        {
                            tabIndex = tabIndex + 0.1;
                        }

                        fieldsOnPage.Add(tabIndex, field.Name);
                    }
                }

                foreach (KeyValuePair<double, string> kvp in fieldsOnPage)
                {
                    columnNames.Add(kvp.Value, runningTabIndex);
                    runningTabIndex++;
                }
            }            

            int newRunningTabIndex = 0; // to prevent arg exceptions; TODO: Fix this better
            foreach (KeyValuePair<string, int> kvp in columnNames)
            {
                if (table.Columns.Contains(kvp.Key))
                {
                    table.Columns[kvp.Key].SetOrdinal(newRunningTabIndex);
                    newRunningTabIndex++;
                }
            }            
        }

        /// <summary>
        /// Sorts Grid columns by position
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static int SortByPosition(GridColumnBase A, GridColumnBase B)
        {
            if ((A is PredefinedColumn))
            {
                if (B is PredefinedColumn)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (B is PredefinedColumn)
                {
                    return 1;
                }
                else
                {
                    return A.Position.CompareTo(B.Position);
                }
            }
        }

        /// <summary>
        /// Copies stream from input to output.
        /// </summary>
        /// <param name="inStream">Sequence of bytes to read.</param>
        /// <param name="outStream">Sequence of bytes to write.</param>
        static public void CopyStream(Stream inStream, Stream outStream)
        {
            using (BinaryWriter writer = new BinaryWriter(outStream))
            {
                using (BinaryReader reader = new BinaryReader(inStream))
                {
                    while (true)
                    {
                        byte[] buffer = reader.ReadBytes(short.MaxValue);
                        writer.Write(buffer);

                        // if done
                        if (buffer.Length < short.MaxValue) break;
                    }
                }
            }
        }

        /// <summary>
        /// Copies a directory
        /// </summary>
        /// <param name="source">The source folder</param>
        /// <param name="target">The destination folder</param>
        public static void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
        {
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyDirectory(diSourceSubDir, nextTargetSubDir);
            }
        }



        /// <summary>
        /// Test value to find its data type.
        /// </summary>
        /// <param name="variableValue">Variable to test.</param>
        /// <returns>Data type</returns>
        public static DataType InferDataType(string variableValue)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(variableValue))
            {
                throw new ArgumentNullException("variableValue");
            }
            #endregion Input Validation

            //Check if it looks like a boolean.
            Boolean testBool;
            if (bool.TryParse(variableValue, out testBool))
            {
                return DataType.Boolean;
            }

            //Check if it looks like a number.
            Double testDbl;
            if (double.TryParse(variableValue, out testDbl))
            {
                return DataType.Number;
            }

            //Check if it looks like a Date or a DateTime.
            DateTime testDate;
            if (DateTime.TryParse(variableValue, out testDate))
            {
                if (testDate.Date.Equals(testDate))
                {
                    return DataType.Date;
                }
                else
                {
                    return DataType.DateTime;
                }
            }
            #region OBSOLETE
            //try
            //{
            //    bool.Parse(variableValue);
            //    validDataType = true;
            //}
            //catch (FormatException)
            //{

            //}
            //if (validDataType)
            //{
            //    return DataType.Boolean;
            //}
            //Check if it looks like a date
            //			try
            //			{
            //				DateTime.Parse(variableValue);
            //				validDataType = true;
            //			}
            //			catch(FormatException)
            //			{
            //
            //			}
            //			if (validDataType)
            //			{
            //				return Enums.DataType.Date ;
            //			}

            //Check if it looks like a datetime
            //try
            //{
            //    DateTime.Parse(variableValue);
            //    validDataType = true;
            //}
            //catch (FormatException)
            //{

            //}
            //if (validDataType)
            //{
            //    return DataType.DateTime;
            //}
            //return Enums.DataType.DATETIME;

            // Check if it looks like a number
            // return Enums.DataType.Number;

            //Check if it looks like a phone number
            //return Enums.DataType.PhoneNumber ;

            //Check if it looks like a time data type
            //return Enums.DataType.Time;

            // Check if it looks like a yesno
            // return Enums.DataType.YesNo;
            #endregion

            // ELSE
            return DataType.Text;
        }

        /// <summary>
        /// Returns a string based on the checked property of a checkbox
        /// </summary>
        /// <param name="val"><see cref="System.Boolean"/> boolean value</param>
        /// <returns><see cref="Epi.ShortHands"/> boolean value</returns>
        public static string GetShortHand(bool val)
        {
            if (val) return ShortHands.YES;
            else return ShortHands.NO;
        }

        /// <summary>
        /// Gets the assembly path given a partial assembly name
        /// </summary>
        /// <param name="partialAssemblyName">Name of the assembly without extention</param>
        /// <returns>Fully qualified file name and path of the assembly</returns>
        public static string GetAssemblyPath(string partialAssemblyName)
        {
            try
            {
                string fullyQualifiedAssembly = string.Empty;
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + partialAssemblyName + ".dll"))
                {
                    // If a the assembly exists in the current directory, we have found it.
                    fullyQualifiedAssembly = AppDomain.CurrentDomain.BaseDirectory + partialAssemblyName + ".dll";
                }
                else
                {
                    // If the assembly does not exist in the current directory, then we might be in debug mode.
                    // Must now look for it in the debug\bin directory.
                    string dir = AppDomain.CurrentDomain.BaseDirectory;
                    int index = dir.IndexOf("Epi.Menu");
                    if (index >= 0)
                    {
                        string parentDir = dir.Substring(0, index);
                        fullyQualifiedAssembly = parentDir + partialAssemblyName + @"\bin\Debug\" + partialAssemblyName + ".dll";
                    }
                    else
                    {
                        throw new GeneralException("Could not find Epi.MakeView folder");
                    }
                }

                if (!File.Exists(fullyQualifiedAssembly))
                {
                    // Assembly wasn't in the current directory or the debug\bin directory
                    throw new GeneralException(partialAssemblyName + " module could not be found");
                }

                return fullyQualifiedAssembly;
            }
            catch (Exception ex)
            {
                throw new GeneralException("Could not get assembly path", ex);
            }
        }

        public static byte[] GetByteArrayFromImagePath(string imagePath)
        {
            byte[] imageAsBytes;
            Image image = null;
            MemoryStream memStream = new MemoryStream();

            image = Image.FromFile(imagePath);
            System.Drawing.Imaging.ImageFormat rawFormat = image.RawFormat;
            image.Save(memStream, rawFormat);
            imageAsBytes = memStream.GetBuffer();

            memStream.Close();
            return imageAsBytes;
        }
        
        /// <summary>
        /// Import target DataTable rows into source DataTable rows.
        /// <remarks>Shouldn't this be the other way around?</remarks>
        /// </summary>
        /// <param name="sourceDataTable">In-memory source table of data.</param>
        /// <param name="targetDataTable">In-memory target table of data.</param>
        /// <returns></returns>
        public static DataTable AppendDataTable(DataTable sourceDataTable, DataTable targetDataTable)
        {
            #region Input Validation
            if (sourceDataTable == null)
            {
                throw new ArgumentNullException("sourceDataTable");
            }
            if (targetDataTable == null)
            {
                throw new ArgumentNullException("targetDataTable");
            }
            #endregion Input Validation

            foreach (DataRow dr in targetDataTable.Rows)
            {
                sourceDataTable.ImportRow(dr);
            }
            return sourceDataTable;
        }


        /// <summary>
        /// Combines three path strings.
        /// </summary>
        /// <param name="path">path 1</param>
        /// <param name="childDir1">path 2</param>
        /// <param name="ChildDir2">path 3</param>
        /// <returns></returns>
        public static string CombinePath(string path, string childDir1, string ChildDir2)
        {
            string temp = Path.Combine(path, childDir1);
            return Path.Combine(temp, ChildDir2);
        }

        /// <summary>
        /// Combines four path strings.
        /// </summary>
        /// <param name="path">path 1</param>
        /// <param name="childDir1">path 2</param>
        /// <param name="ChildDir2">path 3</param>
        /// <param name="ChildDir3">path 4</param>
        /// <returns></returns>
        public static string CombinePath(string path, string childDir1, string ChildDir2, string ChildDir3)
        {
            string temp = CombinePath(path, childDir1, ChildDir2);
            return Path.Combine(temp, ChildDir3);
        }


        /// <summary>
        /// Determines if the filepath includes directory name.
        /// </summary>
        /// <param name="filePath">Path of file.</param>
        /// <returns>Results of filepath directory name test.</returns>
        public static bool FilePathContainsDirectoryName(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }
            else
            {
                return (filePath.Contains("\\") || filePath.Contains("/"));
            }
        }

        ///// <summary>
        ///// Converts a list of strings to a single string
        ///// </summary>
        ///// <param name="array"></param>
        ///// <param name="delim"></param>
        ///// <returns></returns>
        //public static string ToString(List<string> array, string delim, bool insertInSquareBrackets)
        //    // TODO: Revisit this inserting in SQ BRACKETS business. It makes no sense here.
        //{
        //    StringBuilder sb = new StringBuilder();
        //    for (int index = 0; index < array.Count - 1; index++)
        //    {
        //        if (insertInSquareBrackets == true)
        //        {
        //            sb.Append(Util.InsertInSquareBrackets(array[index]));
        //        }
        //        else
        //        {
        //            sb.Append(array[index]);
        //        }
        //        sb.Append(delim);
        //    }
        //    sb.Append(array[array.Count - 1]);
        //    return sb.ToString();
        //}

        /// <summary>
        /// Converts a list of strings to a single string
        /// </summary>
        /// <param name="array">List of strings.</param>
        /// <param name="delim">list delimiter</param>
        /// <returns>Single string</returns>
        public static string ToString(List<string> array, string delim)
        {
            StringBuilder sb = new StringBuilder();
            for (int index = 0; index < array.Count - 1; index++)
            {
                //string.Join(delim, array); Wouldn't this work as well?
                sb.Append(array[index]);
                sb.Append(delim);
            }
            sb.Append(array[array.Count - 1]);
            return sb.ToString();
        }

        /// <summary>
        /// Dynamically loads object.
        /// </summary>
        /// <param name="assemblyFile">Reusable, versionable, and self-describing
        /// common language runtime application.</param>
        /// <param name="typeName"><see cref="System.Object"/> object with the specified name in the assembly.</param>
        /// <param name="parameters">List of assembly parameters</param>
        /// <returns>Instance of assembly.</returns>
        public static System.Object LoadObject(string assemblyFile, string typeName, object[] parameters)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(assemblyFile);
                Type moduleType = assembly.GetType(typeName);
                return Activator.CreateInstance(moduleType, parameters);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Creates a globally unique identifier for a filepath.
        /// </summary>
        /// <param name="filePath">Pathname of file.</param>
        /// <returns>Globally unique identifier.</returns>
        public static Guid GetFileGuid(string filePath)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(filePath.ToLowerInvariant()));
            Guid guid = new Guid(result);
            return guid;
        }

        /// <summary>
        /// Determines if the string object is empty.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsEmpty(string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// Determines if the datetime object is empty.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static bool IsEmpty(DateTime dt)
        {
            if (((object)dt) == DBNull.Value) return true;
            else if (DateTime.Equals(dt, nullDateTime)) return true;
            else return false;
        }

        /// <summary>
        /// Tests an object for empty or null.
        /// </summary>
        /// <param name="obj">Any object.</param>
        /// <returns>Results of empty or null.</returns>
        public static bool IsEmpty(object obj)
        {
            if (obj == null) return true;
            else if (obj == DBNull.Value) return true;
            else if (string.IsNullOrEmpty(obj.ToString())) return true;
            else return false;
        }

        /// <summary>
        /// Static method ClipAt
        /// </summary>
        /// <param name="mainString">Target string.</param>
        /// <param name="subString">Test string.</param>
        /// <returns>String clipped to target.</returns>
        public static string ClipAt(string mainString, string subString)
        {
            int index = mainString.IndexOf(subString);
            if (index >= 0)
            {
                string leftPart = mainString.Substring(0, index);
                return (leftPart + subString);
            }
            else
            {
                return (mainString);
            }
        }

        /// <summary>
        /// Remove all whitespace characters.
        /// </summary>
        /// <param name="str">Original string.</param>
        /// <returns>String without whitespace characters.</returns>
        public static string Squeeze(string str)
        {
            #region Input Validation
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            #endregion

            return new Regex(@"\s*").Replace(str, string.Empty);
        }


        /// <summary>
        /// Remove all nonaplhanumeric characters.
        /// </summary>
        /// <param name="text">Input string</param>
        /// <returns>Alphanumeric characters without spaces.</returns>
        public static string RemoveNonAlphaNumericCharacters(string text)
        {
            #region Input Validation
            if (text == null)
            {
                throw new ArgumentNullException("Text");
            }
            #endregion
            
            Regex regex = new Regex("[^A-Z0-9_]", RegexOptions.IgnoreCase);
            return (regex.Replace(text, string.Empty));
        }

        /// <summary>
        /// Removes all nonalphanumeric characters with the exception of spaces.
        /// </summary>
        /// <param name="text">Input string</param>
        /// <returns>Alphanumeric characters with spaces.</returns>
        public static string RemoveNonAlphaNumericExceptSpaces(string text)
        {
            #region Input Validation
            if (text == null)
            {
                throw new ArgumentNullException("Text");
            }
            #endregion

            Regex regex = new Regex("[^ A-Z0-9_]", RegexOptions.IgnoreCase);
            return (regex.Replace(text, string.Empty));
        }

        /// <summary>
        /// Indicates whether the first character in the string is an alphabetic character.
        /// </summary>
        /// <param name="text">String to evaluate.</param>
        /// <returns>Results of test on first character.</returns>
        public static bool IsFirstCharacterALetter(string text)
        {
            #region Input Validation
            if (text == null)
            {
                throw new ArgumentNullException("Text");
            }
            #endregion

            return (Char.IsLetter((Char)text[0]));
        }

        /// <summary>
        /// Indicates whether the string contains numeric data only
        /// </summary>
        /// <param name="text">String to evaluate.</param>
        /// <returns>Results of test on numeric data.</returns>
        public static bool IsNumeric(string text)
        {
            #region Input Validation
            if (text == null)
            {
                throw new ArgumentNullException("Text");
            }
            #endregion
            
            decimal result;
            return decimal.TryParse(text, out result);
        }

        /// <summary>
        /// Indicates whether the string contains a whole number
        /// </summary>
        /// <param name="text">String to evaluate.</param>
        /// <returns>Results of test on numeric data.</returns>
        public static bool IsWholeNumber(string text)
        {
            #region Input Validation
            if (text == null)
            {
                throw new ArgumentNullException("Text");
            }
            #endregion

            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if (!Char.IsDigit(ch))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Encloses a string with the same enclosing string.
        /// </summary>
        /// <param name="originalString">Original string.</param>
        /// <param name="enclosingString">Strings to surround the original string.</param>
        /// <returns>Enclosed string.</returns>
        public static string InsertIn(string originalString, string enclosingString)
        {
            return (enclosingString + originalString + enclosingString);
        }

        /// <summary>
        /// Encloses a string with <see cref="StringLiterals.SINGLEQUOTES"/>.
        /// </summary>
        /// <param name="originalString">Original string.</param>
        /// <returns>Enclosed string.</returns>
        public static string InsertInSingleQuotes(string originalString)
        {
            // Escape single quotes (') with two single quotes ('').
            return InsertIn(originalString.Replace(StringLiterals.SINGLEQUOTES, StringLiterals.SINGLEQUOTES + StringLiterals.SINGLEQUOTES),
                StringLiterals.SINGLEQUOTES);
        }

        /// <summary>
        /// Surrounds a string with <see cref="StringLiterals.DOUBLEQUOTES"/>.
        /// </summary>
        /// <param name="originalString">Orginial string.</param>
        /// <returns>Enclosed string.</returns>
        public static string InsertInDoubleQuotes(string originalString)
        {
            return InsertIn(originalString, StringLiterals.DOUBLEQUOTES);
        }

        /// <summary>
        /// Surrounds a string with <see cref="StringLiterals.PARANTHESES_OPEN"/>
        /// and <see cref="StringLiterals.PARANTHESES_CLOSE"/>.
        /// </summary>
        /// <param name="originalString">Original string.</param>
        /// <returns>Enclosed string.</returns>
        public static string InsertInParantheses(string originalString)
        {
            return (StringLiterals.PARANTHESES_OPEN + originalString + StringLiterals.PARANTHESES_CLOSE);
        }

        /// <summary>
        /// Surrounds a string with <see cref="StringLiterals.LEFT_SQUARE_BRACKET"/>
        /// and <see cref="StringLiterals.RIGHT_SQUARE_BRACKET"/>.
        /// </summary>
        /// <param name="originalString">Original string.</param>
        /// <returns>Enclosed string.</returns>
        public static string InsertInSquareBrackets(string originalString)
        {
            return (StringLiterals.LEFT_SQUARE_BRACKET + originalString + StringLiterals.RIGHT_SQUARE_BRACKET);
        }

        /// <summary>
        /// Joins to strings together, separated by a colon.
        /// </summary>
        /// <param name="first">First string.</param>
        /// <param name="second">Second string.</param>
        /// <returns>Joined string</returns>
        public static string CombineMessageParts(string first, string second)
        {
            return first + Environment.NewLine + Environment.NewLine + second;
        }

        /// <summary>
        /// Epi Info product description.
        /// </summary>
        /// <returns>Epi Info product description.</returns>
        public static string GetProductDescription()
        {
            StringBuilder descriptionBuilder = new StringBuilder();
            descriptionBuilder.Append(SharedStrings.ABOUT_EPIINFO_LINE1);
            descriptionBuilder.Append(Environment.NewLine);
            descriptionBuilder.Append(Environment.NewLine);
            descriptionBuilder.Append(SharedStrings.ABOUT_EPIINFO_LINE2);
            descriptionBuilder.Append(Environment.NewLine);
            descriptionBuilder.Append(Environment.NewLine);
            descriptionBuilder.Append(SharedStrings.ABOUT_EPIINFO_LINE3);
            return descriptionBuilder.ToString();
        }

        /// <summary>
        /// Asserts that a precondition is true.
        /// </summary>
        /// <param name="condition">Results of test.</param>
        public static void Assert(bool condition)
        {
            if (condition == false)
            {
                throw new GeneralException("Assert failed");
            }
        }

        /// <summary>
        /// Returns all the inner exception messages
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string GetComprehensiveExceptionMessage(Exception ex)
        {
            string msg = string.Empty;
            while (ex != null)
            {
                msg += ex.Message + Environment.NewLine;
                ex = ex.InnerException;
            }
            return msg;
        }

        /// <summary>
        /// Returns a string representation of the object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToString(Object obj)
        {
            if (IsEmpty(obj)) return string.Empty;
            else return obj.ToString();
        }

        #endregion public static methods

    }
}
