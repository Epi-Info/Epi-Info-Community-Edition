#region Using
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
#endregion // Using

namespace Epi.ImportExport
{
    /// <summary>
    /// An interface for project packager classes. Project packagers are designed to de-identify data, filter records
    /// in the data, compress the data, and then encrypt the data for transport across untrusted and/or unsecured mediums.
    /// The packager will create a copy of the project. MS Access is the database type for the copy that will reside in
    /// the package file. The interface outlines the public methods and properties that packager classes must implement.
    /// -- E. Knudsen, 2012
    /// </summary>    
    public interface IProjectPackager
    {
        #region Events
        event SetProgressBarDelegate SetProgressBar;
        event UpdateStatusEventHandler SetStatus;
        event UpdateStatusEventHandler AddStatusMessage;
        event SetMaxProgressBarValueDelegate SetMaxProgressBarValue;
        #endregion // Events

        #region Methods
        bool PackageProject();
        void SetCustomEncryptionParameters(string salt, string initVector, int iterations);        
        #endregion // Methods

        #region Properties
        Dictionary<string, List<string>> ColumnsToNull { get; set; }
        Project SourceProject { get; }
        string PackagePath { get; }
        string PackageName { get; set; }
        Epi.Data.Query SelectQuery { get; set; }
        bool IncludeGridData { get; set; }
        bool IncludeCodeTables { get; set; }
        bool AppendTimestamp { get; set; }
        FormInclusionType FormInclusionType { get; set; }
        #endregion // Properties
    }
}
