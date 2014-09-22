using System;
using System.Collections.Generic;
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

namespace Epi.ImportExport
{
    /// <summary>
    /// A class used to unpackage Epi Info 7 projects from their compressed, encrypted form.
    /// </summary>
    public class ProjectUnpackager
    {
        #region Private Members
        private View destinationView;
        private Project sourceProject;
        private Project destinationProject;
        private List<string> packagePaths;        
        private string password;
        private GCHandle gch;
        private string customSalt = "";
        private string customInitVector = "";
        private int customIterations = 4;
        private bool update = false;
        private bool append = true;
        private int runningCount;
        #endregion //Private Members

        #region Events
        public event SetMaxProgressBarValueDelegate SetMaxProgressBarValue;
        public event SetProgressBarDelegate SetProgressBar;
        public event UpdateStatusEventHandler SetStatus;
        public event UpdateStatusEventHandler AddStatusMessage;
        public event SimpleEventHandler FinishUnpackage;
        #endregion // Events

        #region Constructors
        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="destinationView">The destination form.</param>
        /// <param name="packagePaths">The file paths for the packages to be imported.</param>
        /// <param name="password">The password for the encryption.</param>
        public ProjectUnpackager(View destinationView, List<string> packagePaths, string password)
        {
            this.destinationView = destinationView;
            this.destinationProject = destinationView.Project;
            this.packagePaths = packagePaths;
            this.password = password;
            gch = GCHandle.Alloc(password, GCHandleType.Pinned);
        }
        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Gets/sets whether to append unmatched records during the import.
        /// </summary>
        public bool Append
        {
            get
            {
                return this.append;
            }
            set
            {
                this.append = value;
            }
        }

        /// <summary>
        /// Gets/sets whether to update matching records during the import.
        /// </summary>
        public bool Update
        {
            get
            {
                return this.update;
            }
            set
            {
                this.update = value;
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
        public List<string> PackagePaths
        {
            get
            {
                return this.packagePaths;
            }
        }
        #endregion // Public Properties

        #region Public Methods
        /// <summary>
        /// Sets custom parameters for decrypting the package.
        /// </summary>
        /// <param name="salt">The user-specified salt value</param>
        /// <param name="initVector">The user-specified initialization vector</param>
        /// <param name="iterations">The number of password iterations</param>
        public void SetCustomDecryptionParameters(string salt, string initVector, int iterations)
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
        /// Unpackages a project
        /// </summary>
        /// <returns></returns>
        //public bool UnpackageProject()
        //{
        //    bool success = false;

        //    success = DeryptPackage();
        //    success = DecompressProject();            

        //    string mdbFilePath = compressedPackagePath.Substring(0, compressedPackagePath.Length - 3) + ".mdb";
        //    string prjFilePath = compressedPackagePath.Substring(0, compressedPackagePath.Length - 3) + ".prj";
        //    sourceProject = Util.CreateProjectFileFromDatabase(mdbFilePath, true);
        //    sourceProject = new Project(prjFilePath);            

        //    View parentSourceView = sourceProject.Views[destinationView.Name];

        //    List<View> viewsToProcess = new List<View>();

        //    foreach (View view in sourceProject.Views)
        //    {
        //        if (view.Name == parentSourceView.Name)
        //        {
        //            continue;
        //        }

        //        if (ImportExportHelper.IsFormDescendant(view, parentSourceView))
        //        {
        //            viewsToProcess.Add(view);
        //        }
        //    }

        //    FormDataImporter fdi = new FormDataImporter(sourceProject, destinationProject, destinationView, viewsToProcess);
        //    fdi.SetProgressBar += new SetProgressBarDelegate(OnSetProgress);
        //    fdi.SetStatus += new UpdateStatusEventHandler(OnSetStatusMessage);
        //    fdi.AddStatusMessage += new UpdateStatusEventHandler(OnAddStatusMessage);
        //    fdi.SetMaxProgressBarValue += new SetMaxProgressBarValueDelegate(OnSetMaxProgressBarValue);
        //    fdi.Update = this.Update;
        //    fdi.Append = this.Append;
        //    fdi.ImportFormData();
        //    fdi.Dispose();

        //    return success;
        //}

        /// <summary>
        /// Unpackages a series of projects
        /// </summary>
        /// <returns></returns>
        public bool UnpackageProjects()
        {
            bool success = false;
            
            runningCount = 1;

            foreach (string filePath in PackagePaths)
            {
                string compressedPackagePath = DeryptPackage(filePath);
                success = DecompressProject(compressedPackagePath);

                string mdbFilePath = compressedPackagePath.Substring(0, compressedPackagePath.Length - 3) + ".mdb";
                string prjFilePath = compressedPackagePath.Substring(0, compressedPackagePath.Length - 3) + ".prj";
                sourceProject = Util.CreateProjectFileFromDatabase(mdbFilePath, true);
                sourceProject = new Project(prjFilePath);

                View parentSourceView = sourceProject.Views[destinationView.Name];

                List<View> viewsToProcess = new List<View>();

                foreach (View view in sourceProject.Views)
                {
                    if (view.Name == parentSourceView.Name)
                    {
                        continue;
                    }

                    if (ImportExportHelper.IsFormDescendant(view, parentSourceView))
                    {
                        viewsToProcess.Add(view);
                    }
                }

                FormDataImporter fdi = new FormDataImporter(sourceProject, destinationProject, destinationView, viewsToProcess);
                fdi.SetProgressBar += new SetProgressBarDelegate(OnSetProgress);
                fdi.SetStatus += new UpdateStatusEventHandler(OnSetStatusMessage);
                fdi.AddStatusMessage += new UpdateStatusEventHandler(OnAddStatusMessage);
                fdi.SetMaxProgressBarValue += new SetMaxProgressBarValueDelegate(OnSetMaxProgressBarValue);
                fdi.Update = this.Update;
                fdi.Append = this.Append;
                fdi.ImportFormData();
                fdi.Dispose();

                if (FinishUnpackage != null)
                {
                    FinishUnpackage();
                }

                try
                {
                    File.Delete(compressedPackagePath);
                    File.Delete(mdbFilePath);
                }
                catch (IOException ex)
                {
                    OnAddStatusMessage("There was a problem deleting the unencrypted (plaintext) data from disk. Exception: " + ex);
                }
                runningCount++;
            }

            return success;
        }
        #endregion // Public Methods

        #region Private Methods

        private bool DecompressProject(string compressedPackagePath)
        {
            try
            {
                FileInfo fi = new FileInfo(compressedPackagePath);
                DecompressDataPackage(fi);
            }
            catch
            {
                return false;
            }

            return true;
        }        

        /// <summary>
        /// Decrypts an edp7 file
        /// </summary>
        /// <param name="packagePath">The path to the edp7 file</param>
        /// <returns>string</returns>
        private string DeryptPackage(string packagePath)
        {
            if (PackagePaths.Count == 1)
            {
                OnSetStatusMessage(ImportExportSharedStrings.PACKAGE_DECRYPTION_START);
            }
            else
            {
                OnSetStatusMessage(string.Format(ImportExportSharedStrings.PACKAGE_DECRYPTION_START_N_OF_N,
                    runningCount.ToString(), PackagePaths.Count));
            }
            
            string gzFileName = packagePath.Substring(0, packagePath.Length - 5) + ".gz";

            string inputFileName = packagePath; // destinationProject.CollectedData.DataSource.Substring(0, destinationProject.CollectedData.DataSource.Length - 4) + ".edp7";
            string outputFileName = gzFileName;

            if (string.IsNullOrEmpty(customInitVector) || string.IsNullOrEmpty(customSalt))
            {
                Configuration.DecryptFile(inputFileName, outputFileName, password);
            }
            else
            {
                Configuration.DecryptFile(inputFileName, outputFileName, password, customInitVector, customSalt, customIterations);
            }

            //File.Delete(inputFileName);
            string compressedPackagePath = outputFileName;

            if (PackagePaths.Count == 1)
            {
                OnSetStatusMessage(ImportExportSharedStrings.PACKAGE_DECRYPTION_END);
            }
            else
            {
                OnSetStatusMessage(string.Format(ImportExportSharedStrings.PACKAGE_DECRYPTION_END_N_OF_N,
                    runningCount.ToString(), PackagePaths.Count));
            }

            return compressedPackagePath;
        }
        #endregion // Private Methods

        #region StaticMethods
        /// <summary>
        /// Compresses an Epi Info 7 data package.
        /// </summary>
        /// <param name="fi">The file info for the raw data file that will be compressed.</param>
        public static void CompressDataPackage(FileInfo fi)
        {
            using (FileStream inFile = fi.OpenRead())
            {
                // Prevent compressing hidden and already compressed files.
                if ((File.GetAttributes(fi.FullName) & FileAttributes.Hidden)
                        != FileAttributes.Hidden & fi.Extension != ".gz")
                {
                    using (FileStream outFile = File.Create(fi.FullName + ".gz"))
                    {
                        using (GZipStream Compress = new GZipStream(outFile,
                                CompressionMode.Compress))
                        {
                            // Copy the source file into the compression stream.
                            byte[] buffer = new byte[4096];
                            int numRead;
                            while ((numRead = inFile.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                Compress.Write(buffer, 0, numRead);
                            }
                            Compress.Close();
                            Compress.Dispose();
                        }                        
                    }
                }
            }
            
        }

        /// <summary>
        /// Decompresses an Epi Info 7 data package.
        /// </summary>
        /// <param name="fi">The file info for the compressed file to be decompressed.</param>
        public static void DecompressDataPackage(FileInfo fi)
        {
            ImportExportHelper.DecompressDataPackage(fi);            
        }

        //  Call this function to remove the key from memory after use for security
        [System.Runtime.InteropServices.DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr Destination, int Length);

        #endregion // Static Methods

        #region Protected Methods
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
        /// Sets progess bar max value
        /// </summary>
        /// <param name="maxProgress">The max progress</param>
        protected virtual void OnSetMaxProgressBarValue(double maxProgress)
        {
            if (SetMaxProgressBarValue != null)
            {
                SetMaxProgressBarValue(maxProgress * PackagePaths.Count);
            }
        }
        #endregion // Protected Methods
    }
}
