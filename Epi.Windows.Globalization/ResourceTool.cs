using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using Epi.Data;
using System.Drawing;
using System.Collections;
using System.IO;
using System.Resources;
using System.ComponentModel;
using System.Globalization;

namespace Epi.Windows.Globalization
{
      /// <summary>
    /// Class ResourceTool
    /// </summary>
    public class ResourceTool
    {
        /// <summary>
        /// The Default Culture
        /// </summary>
        public const string DEFAULT_CULTURE = "en-US";
        /// <summary>
        /// Database factory
        /// </summary>
        public static Epi.Data.IDbDriverFactory dbFactory;

        /// <summary>
        /// Extract text resources from assemblies in specified folder
        /// </summary>
        /// <param name="applicationPath">Path of application.</param>
        /// <param name="sourceCulture">Culture code of the source.</param>
        /// <param name="targetCulture">Culture code of the target.</param>
        /// <param name="translator">Translation component.</param>
        /// <param name="exportFilePath">Path of file to export.</param>
        /// <param name="callback">Address pointer for <see cref="Epi.Windows.Localization.AppendLogCallback"/>.</param>
        public static void ExportLanguage(string applicationPath, string sourceCulture, string targetCulture, ITranslator translator, string exportFilePath, AppendLogCallback callback)
        {
            ResourceTool.dbFactory = Epi.Data.DbDriverFactoryCreator.GetDbDriverFactory(Epi.Configuration.AccessDriver);
            ResourceTool instance = new ResourceTool(applicationPath, callback);

            //IDbDriver db = DatabaseFactoryCreator.CreateDatabaseInstanceByFileExtension(exportFilePath);
            OleDbConnectionStringBuilder dbCnnStringBuilder = new OleDbConnectionStringBuilder();
            dbCnnStringBuilder.DataSource = exportFilePath;
            IDbDriver db = dbFactory.CreateDatabaseObject(dbCnnStringBuilder);

            GlobalizationDataSet ds = instance.ExtractResourcesToDataSet(sourceCulture, targetCulture, translator);
            instance.WriteResourcesToDatabase(ds, db);
            
        }

        private ResourceTool(string applicationPath, AppendLogCallback callback)
        {
            this.callback = callback;
            this.applicationPath = applicationPath;
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }


        GlobalizationDataSet ReadResourcesFromDatabase(IDbDriver dataSource, string tableName)
        {
            AppendToLog("Reading database");

          
            GlobalizationDataSet ds = new GlobalizationDataSet();

            if (string.IsNullOrEmpty(tableName))
            {
                // default table name
                tableName = ds.CulturalResources.TableName;
            }

            DataTable dt = dataSource.GetTableData(tableName);

            try
            {
                DataTableReader reader = new DataTableReader(dt);
                ds.CulturalResources.BeginLoadData();
                ds.CulturalResources.Load(reader);
                ds.CulturalResources.EndLoadData();
            }
            catch (Exception ex)
            {
                throw new GeneralException("Import database format is invalid.", ex);
            }

            return ds;
        }

        void WriteResourcesToDatabase(GlobalizationDataSet ds, IDbDriver db)
        {
            AppendToLog("Please wait...");

            List<TableColumn> columns = new List<TableColumn>();
            
            columns.Add(new TableColumn("AssemblyName", GenericDbColumnType.String, 255, false));
            columns.Add(new TableColumn("ManifestResourceName", GenericDbColumnType.String, 255, false));
            columns.Add(new TableColumn("ResourceName", GenericDbColumnType.String, 255, false));
            columns.Add(new TableColumn("ResourceValue", GenericDbColumnType.StringLong, false));
            columns.Add(new TableColumn("ResourceType", GenericDbColumnType.String, 255, false));
            columns.Add(new TableColumn("ResourceVersion", GenericDbColumnType.String, 255, false));
            columns.Add(new TableColumn("SourceCultureName", GenericDbColumnType.String, 255, false));
            columns.Add(new TableColumn("SourceValue", GenericDbColumnType.StringLong, 255, false));
            
            //columns.Add(new TableColumn("CreationDate", GenericDbColumnType.String, 255, false));

            DbDriverInfo dbInfo = new DbDriverInfo();
            dbInfo.DBName = "CulturalResources";
            dbInfo.DBCnnStringBuilder = new System.Data.OleDb.OleDbConnectionStringBuilder();
            dbInfo.DBCnnStringBuilder.ConnectionString = db.ConnectionString; 
            dbFactory.CreatePhysicalDatabase(dbInfo);
//            db.CreateDatabase("CulturalResources");

            string tableName = ds.CulturalResources.TableName;

            db.CreateTable(tableName, columns);

            StringBuilder sb = new StringBuilder();
            sb.Append("insert into ");
            sb.Append(tableName);
            sb.Append(" (");
            for (int x = 0; x < columns.Count; x++)
            {
                sb.Append(columns[x].Name);
                if (x < columns.Count - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(") values ");
            sb.Append(" (");
            for (int x = 0; x < columns.Count; x++)
            {
                sb.Append("@");
                sb.Append(columns[x].Name);
                if (x < columns.Count - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(")");
            Query insertQuery = db.CreateQuery(sb.ToString());
            for (int x = 0; x < columns.Count; x++)
            {
                insertQuery.Parameters.Add(new QueryParameter("@" + columns[x].Name, DbType.String, columns[x].Name, columns[x].Name));
            }

            sb.Remove(0, sb.Length);

            sb.Append("update [").Append(tableName);
            sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET).Append(StringLiterals.SPACE);
            sb.Append("set").Append(StringLiterals.SPACE);
            for (int x = 0; x < columns.Count; x++)
            {
                sb.Append(StringLiterals.LEFT_SQUARE_BRACKET);
                sb.Append(columns[x].Name);
                sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET);
                sb.Append(StringLiterals.EQUAL);
                sb.Append("@NewValue").Append(StringLiterals.SPACE);
                sb.Append(", ");
            }
            sb.Append("where ");
            for (int x = 0; x < columns.Count; x++)
            {
                sb.Append(columns[x].Name).Append(StringLiterals.SPACE);
                sb.Append(StringLiterals.EQUAL);
                sb.Append("@OldValue");
                if (columns.Count > 1)
                {
                    sb.Append(" and");
                }
            }
            Query updateQuery = db.CreateQuery(sb.ToString());
            for (int x = 0; x < columns.Count; x++)
            {
                updateQuery.Parameters.Add(new QueryParameter("@NewValue", DbType.String, columns[x].Name, columns[x].Name));
                updateQuery.Parameters.Add(new QueryParameter("@OldValue", DbType.String, columns[x].Name, columns[x].Name));
                updateQuery.Parameters[1].SourceVersion = DataRowVersion.Original;
            }


            db.Update(ds.Tables[0], tableName, insertQuery, null);
        }

        bool IsTextResource(DictionaryEntry resourceEntry)
        {
            char[] letters = new char[]
            {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z'};

            try
            {
                if (resourceEntry.Value is string)
                {
                    string value = resourceEntry.Value.ToString().Trim();

                    bool isText = false;
                    if (resourceEntry.Key.ToString().EndsWith(".Text"))
                    {
                        isText = true;
                    }
                    else if (resourceEntry.Key.ToString().EndsWith(".ToolTipText"))
                    {
                        isText = true;
                    }

                    if (isText)
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            if (value.IndexOfAny(letters) > -1)
                                return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }
        }

        Version GetSatelliteContractVersion(Assembly a)
        {
            Version version;
            
            object[] customAttributes = a.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
            if (customAttributes.Length == 0)
            {
                return a.GetName().Version;
            }
            string str = ((AssemblyInformationalVersionAttribute)customAttributes[0]).InformationalVersion;
            try
            {
                version = new Version(str);
            }
            catch (Exception exception)
            {
                if (a != typeof(object).Assembly)
                {
                    throw new ArgumentException("bad", exception);
                }
                return null;
            }
            return version;
        }

       

        Assembly ResolveResourceAssembly(string path, string cultureName)
        {
            Assembly resourceAssembly;
            
            Assembly mainAssembly = Assembly.LoadFrom(path);
            string mainAssemblyCultureName = mainAssembly.GetName().CultureInfo.Name;

            //Pull satellite assemblies if necesary to extract resources from source Culture
            if (string.Compare(mainAssemblyCultureName, cultureName, true) != 0) //|| string.Compare(cultureName, DEFAULT_CULTURE, true) != 0
            {
                //   System.IO.FileNotFoundException:
                //     The assembly cannot be found.
                //
                //   System.IO.FileLoadException:
                //     The satellite assembly with a matching file name was found, but the CultureInfo
                //     did not match the one specified.

                try
                {
                    //use satellite assembly for resources
                    resourceAssembly = mainAssembly.GetSatelliteAssembly(new CultureInfo(cultureName));
                }
                catch (System.IO.FileNotFoundException)
                {
                    // no satellite is available for desired culture, this is ok
                    return null;
                }
            }
            else
            {
                resourceAssembly = mainAssembly;
            }
            
            return resourceAssembly;
        }

        List<string> GetAssemblyFileList(string applicationPath)
        {
            StringBuilder builder = new StringBuilder();
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(applicationPath, "epi*.dll", SearchOption.TopDirectoryOnly));
            files.AddRange(Directory.GetFiles(applicationPath, "*.exe", SearchOption.TopDirectoryOnly));
            return files;
        }

       
        string GetString(System.Collections.Specialized.StringCollection col)
        {
            string returnString = "";
            foreach (string str in col)
            {
                returnString = returnString + str + "\n";
            }
            return returnString;
        }

        GlobalizationDataSet ExtractResourcesToDataSet(string sourceCulture, string targetCulture, ITranslator translator)
        {
            int resourceCount = 0;

            GlobalizationDataSet ds = new GlobalizationDataSet();

          
            List<string> files = GetAssemblyFileList(applicationPath);

            foreach (string file in files)
            {
                int fileResourceCount = 0;

                GlobalizationDataSet.CulturalResourcesRow newrow;

                Assembly resourceAssembly = ResolveResourceAssembly(file, sourceCulture);

                // skip missing satellite assemblies
                if (resourceAssembly == null) continue;

                string[] resourceNames = resourceAssembly.GetManifestResourceNames();
                foreach (string resourceName in resourceNames)
                {

                    this.AppendToLog("Processing resource '{0}'", resourceName);

                    string manifestResourceName = resourceName;

                    // trim off any culture specification from satellite manifest resource name
                    if (manifestResourceName.EndsWith("." + sourceCulture + ".resources", true, CultureInfo.InvariantCulture))
                    {
                        manifestResourceName = manifestResourceName.Substring(0, manifestResourceName.Length - (1 + sourceCulture.Length + ".resources".Length));
                        manifestResourceName += ".resources";
                    }

                    if (manifestResourceName.EndsWith(".resources"))
                    {
                        using (ResourceReader reader = new ResourceReader(resourceAssembly.GetManifestResourceStream(resourceName)))
                        {
                            bool isSharedStrings = manifestResourceName.EndsWith("SharedStrings.resources");

                            IDictionaryEnumerator enumerator = reader.GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                if (isSharedStrings || IsTextResource(enumerator.Entry))
                                {
                                    fileResourceCount++;

                                    string assemblyName = resourceAssembly.GetName().Name;

                                    // discard any .resource suffix on the end (satellite assemblies)
                                    if (assemblyName.EndsWith(".resources", true, CultureInfo.InvariantCulture))
                                    {
                                        assemblyName = assemblyName.Substring(0, assemblyName.Length - ".resources".Length);
                                    }

                                    string sourceText = enumerator.Value.ToString();

                                    this.AppendToLog(SharedStrings.TRANSLATING, sourceText);

                                    string translatedText = translator.Translate(sourceCulture, targetCulture, sourceText);

                                 
                                    newrow = ds.CulturalResources.NewCulturalResourcesRow();
                                    newrow.AssemblyName = assemblyName;
                                    newrow.ResourceVersion = this.GetSatelliteContractVersion(resourceAssembly).ToString();
                                    newrow.ManifestResourceName = manifestResourceName;
                                    newrow.ResourceName = enumerator.Key.ToString();
                                    newrow.ResourceValue = translatedText;
                                    newrow.ResourceType = enumerator.Value.GetType().FullName;
                                    newrow.SourceCultureName = sourceCulture;
                                    newrow.SourceValue = sourceText;

                                    ds.CulturalResources.AddCulturalResourcesRow(newrow);

                                }
                            }
                        }
                    }
                    else
                    {
                        // embedded resource does not end with .resources
                        continue;
                    }
                }


                this.AppendToLog(SharedStrings.EXTRACTION_COUNT, file, fileResourceCount);
                resourceCount += fileResourceCount;
            }

            this.AppendToLog(SharedStrings.EXTRACTION_COMPLETE, resourceCount);

            return ds;

        }

        private string applicationPath;
        private AppendLogCallback callback;
        void AppendToLog(string message)
        {
            if (callback != null)
            {
                callback.Invoke(message);
            }
        }

         void AppendToLog(string format, params object[] args)
        {
            this.AppendToLog(string.Format(format, args));
        }


        /// <summary>
        /// ImportLanguage()
        /// </summary>
        /// <param name="applicationPath"></param>
        /// <param name="targetCulture"></param>
        /// <param name="dataSource"></param>
        /// <param name="tableName"></param>
        /// <param name="callback"></param>
        public static void ImportLanguage(string applicationPath, string targetCulture, IDbDriver dataSource, string tableName, AppendLogCallback callback)
        {
            ResourceTool instance = new ResourceTool(applicationPath, callback);
            
            GlobalizationDataSet ds = instance.ReadResourcesFromDatabase(dataSource, tableName);
            
            instance.CompileSatelliteAssemblies(targetCulture, ds);
          
        }

       /// <summary>
        /// CompileSatelliteAssemblies()
       /// </summary>
       /// <param name="targetCulture"></param>
       /// <param name="ds"></param>
        public void CompileSatelliteAssemblies(string targetCulture, GlobalizationDataSet ds)
        {
            //all columns are required except SourceValue

            string tempPath = Path.GetTempPath();
            applicationPath = applicationPath.Trim().ToLower();

            if (applicationPath[applicationPath.Length - 1] != Path.DirectorySeparatorChar)
                applicationPath += Path.DirectorySeparatorChar;

            this.AppendToLog(SharedStrings.INSTALLING_LANGUAGE, targetCulture);

            GlobalizationDataSet.CulturalResourcesDataTable cultureTable = ds.CulturalResources;//.SelectFilter("CultureName='" + targetCulture + "'");

            GlobalizationDataSet.CulturalResourcesDataTable assembliesTable = cultureTable.SelectDistinct("AssemblyName");
            foreach (GlobalizationDataSet.CulturalResourcesRow assemblyRow in assembliesTable)
            {
                List<string> resourceList = new List<string>();

                GlobalizationDataSet.CulturalResourcesDataTable resourceTable = ds.CulturalResources.SelectFilter("AssemblyName='" + assemblyRow.AssemblyName + "'").SelectDistinct("ManifestResourceName");
                foreach (GlobalizationDataSet.CulturalResourcesRow resourceRow in resourceTable)
                {
                    
                    string resourceFilename;
                    resourceFilename = resourceRow.ManifestResourceName.Substring(0, resourceRow.ManifestResourceName.Length - "resources".Length);
                    resourceFilename = Path.Combine(tempPath, resourceFilename + targetCulture + ".resources");

                    try
                    {
                        using (ResourceWriter writer = new ResourceWriter(resourceFilename))
                        {
                            GlobalizationDataSet.CulturalResourcesDataTable itemTable = ds.CulturalResources.SelectFilter("AssemblyName='" + assemblyRow.AssemblyName + "'").SelectFilter("ManifestResourceName='" + resourceRow.ManifestResourceName + "'");
                            foreach (GlobalizationDataSet.CulturalResourcesRow itemRow in itemTable)
                            {
                                writer.AddResource(itemRow.ResourceName, itemRow.ResourceValue);
                            }
                            writer.Close();
                        }

                        resourceList.Add(resourceFilename);
                    }
                    catch
                    {
                        foreach (string tempResourceFile in resourceList)
                        {
                            try
                            {
                                File.Delete(tempResourceFile);
                            }
                            catch
                            { 
                                //eat
                            }
                        }

                        // kickout
                        throw;
                    }
                }

                string assemblyFilename = targetCulture + Path.DirectorySeparatorChar + assemblyRow.AssemblyName + ".resources.dll";
                assemblyFilename = Path.Combine(applicationPath, assemblyFilename);

                this.AppendToLog("Compiling " + assemblyRow.AssemblyName + " satellite assembly.");

                if (!Directory.Exists(Path.GetDirectoryName(assemblyFilename)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(assemblyFilename));
                }
                else if (File.Exists(assemblyFilename))
                {
                    File.Delete(assemblyFilename);
                }

                CSharpCodeProvider prov = new CSharpCodeProvider();
                CompilerParameters p = new CompilerParameters();
                p.EmbeddedResources.AddRange(resourceList.ToArray());
                p.GenerateInMemory = false;
                p.OutputAssembly = assemblyFilename;

                // setup references
                p.ReferencedAssemblies.Add("System.dll");

                
                Assembly mainAssembly = Assembly.Load(assemblyRow.AssemblyName);

                string version = GetSatelliteContractVersion(mainAssembly).ToString();
                if (version != assemblyRow.ResourceVersion.Trim())
                { 
                    // warning! resource mismatch
                }

                string code = "[assembly: System.Reflection.AssemblyVersion(\"" + version + "\")] " +
                "[assembly: System.Reflection.AssemblyFileVersion(\"" + version + "\")] " +
                "[assembly: System.Reflection.AssemblyCulture(\"" + targetCulture + "\")]";
                CompilerResults res = prov.CompileAssemblyFromSource(p, code);

                foreach (string tempResourceFile in resourceList)
                {
                    File.Delete(tempResourceFile);
                }

                if (res.Errors.HasErrors)
                {
                    throw new GeneralException(GetString(res.Output));
                }
            }
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Directory.SetCurrentDirectory(applicationPath);
            try
            {
                System.Diagnostics.Debug.WriteLine("Looking for " + args.Name);
                return null;
            }
            catch
            {
                return null;
                //eat
            }
        }
    }

    /// <summary>
    /// AppendLogCallback()
    /// </summary>
    /// <param name="message"></param>
    public delegate void AppendLogCallback(string message);

    /// <summary>
    /// ExtractionMode enumeration
    /// </summary>
    public enum ExtractionMode
    {
        /// <summary>
        /// Reserved for future Epi 3.x translation database based text export
        /// </summary>
        DatabaseTranslation,

        /// <summary>
        /// Use web service to lookup text export
        /// </summary>
        WebTranslation,

        /// <summary>
        /// Duplicate resources from source culture 
        /// </summary>
        Copy,

        /// <summary>
        /// Reverse all characters in source culture (may not work for unicode text)
        /// </summary>
        Reverse,

        /// <summary>
        /// Double resources from source culture
        /// </summary>
        Double,

        /// <summary>
        /// Default case  
        /// </summary>
        None = Copy
    }
}
