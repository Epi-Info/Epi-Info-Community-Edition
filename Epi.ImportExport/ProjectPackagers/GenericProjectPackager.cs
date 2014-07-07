#region Using
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Epi;
using Epi.Core;
using Epi.Data;
using Epi.Fields;
#endregion // Using

namespace Epi.ImportExport
{
    /// <summary>
    /// A class for packaging projects in a generic fashion. This class should generally work
    /// regardless of the underlying database type, although because it handles the data in a
    /// generic way it may not take the most efficient route possible.
    /// </summary>    
    public class GenericProjectPackager : ProjectPackagerBase
    {
        #region Events
        #endregion // Events

        #region Constructors
        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="sourceProject">The project from which to create the package.</param>
        /// <param name="packagePath">The file path to where the package will reside.</param>
        /// <param name="formName">The name of the form within the project to package.</param>
        /// <param name="password">The password for the encryption.</param>
        public GenericProjectPackager(Project sourceProject, string packagePath, string formName, string password)
            : base(sourceProject, packagePath, formName, password)
        {
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
        public GenericProjectPackager(Project sourceProject, Dictionary<string, List<string>> columnsToNull, Dictionary<string, List<string>> gridColumnsToNull, string packagePath, string formName, string password)
            : base(sourceProject, columnsToNull, gridColumnsToNull, packagePath, formName, password)
        {
        }
        #endregion // Constructors

        #region Private Members
        #endregion // Private Members

        #region Protected Methods
        /// <summary>
        ///  Creates a copy of the source project. This method is the workhorse of the packager and will carry out the creation
        ///  of any needed metadata as well as copying the appropriate data.
        /// </summary>
        /// <returns>bool; represents success or failure of the copy procedure.</returns>
        protected override bool CreateProjectCopy()
        {
            OnSetStatusMessage(ImportExportSharedStrings.COLLECTED_DATA_COPY_START);
            View sourceView = sourceProject.Views[FormName];            

            // Create the destination project in memory.
            destinationProject = CreateProject();
            string destinationFileName = destinationProject.CollectedData.DataSource;

            // Get the DB driver for the destination project and attach it to the destination project's metadata.
            IDbDriver db = destinationProject.CollectedData.GetDbDriver();
            destinationProject.Metadata.AttachDbDriver(db);

            OnSetStatusMessage(ImportExportSharedStrings.FORM_COPY_START);
            // Create a new instance of the FormToFormDataCopier. We need this class because we're copying data from one
            // form to another; in this case, the form we selected (the source) to the one that will become the object
            // that gets packaged (the destination).
            FormCopier dataCopier = new FormCopier(sourceProject, destinationProject, sourceView);
            if (this.SelectQuery != null)
            {
                dataCopier.SelectQuery = SelectQuery; // The select query is what is used to filter out certain records, e.g. if the user wants to package only records where the case status is "C"
            }
            dataCopier.ColumnsToNull = ColumnsToNull; // Attach the list of columns to null. This is used for de-identification purposes and is considered extremely important.
            dataCopier.GridColumnsToNull = GridColumnsToNull; // Attach the list of grid-based columns to null. This is used for de-identification purposes and is considered extremely important.
            dataCopier.SetProgressBar += new SetProgressBarDelegate(OnSetProgress);
            dataCopier.SetStatus += new UpdateStatusEventHandler(OnSetStatusMessage);
            dataCopier.AddStatusMessage += new UpdateStatusEventHandler(OnAddStatusMessage);
            dataCopier.SetMaxProgressBarValue += new SetMaxProgressBarValueDelegate(OnSetMaxProgressValue);

            // Start the copying process.
            dataCopier.Copy();
            dataCopier.Dispose();

            // Alert the user that the copying process was successful and that the destination data repository was copied successfully.
            OnSetStatusMessage(ImportExportSharedStrings.COLLECTED_DATA_COPY_END);

            try
            {
                // Do a compact on the destination project to try and reduce file size
                destinationProject.CollectedData.GetDbDriver().CompactDatabase();
            }
            catch (Exception ex)
            {
                OnAddStatusMessage(string.Format(ImportExportSharedStrings.WARNING_COMPACT_FAILURE, ex.Message));
            }

            return true;
        }
        #endregion // Protected Methods

        #region Private Methods
        #endregion // Private Methods
    }
}
