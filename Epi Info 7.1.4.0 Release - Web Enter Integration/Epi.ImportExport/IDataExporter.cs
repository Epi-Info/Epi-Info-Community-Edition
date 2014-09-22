#region Using
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
#endregion // Using

namespace Epi.ImportExport
{
    /// <summary>
    /// An interface used for data exporting classes.
    /// </summary>
    public interface IDataExporter
    {
        #region Events
        event SetProgressBarDelegate SetProgressBar;
        event UpdateStatusEventHandler SetStatus;
        event SetProgressAndStatusHandler SetProgressAndStatus;
        event CheckForCancellationHandler CheckForCancellation;
        #endregion // Events

        #region Properties
        /// <summary>
        /// Gets/sets the order in which to sort columns.
        /// </summary>
        ColumnSortOrder ColumnSortOrder { get; set; }

        /// <summary>
        /// Gets the current data view that will be exported.
        /// </summary>
        DataView DataView { get; }

        /// <summary>
        /// Gets/sets whether to export all current fields.
        /// </summary>
        bool ExportAllFields { get; set; }
        #endregion // Properties

        #region Methods
        /// <summary>
        /// Attaches an Epi Info 7 form to the exporter.
        /// </summary>
        /// <param name="view">The Epi Info 7 form to attach</param>
        void AttachView(View view);

        /// <summary>
        /// Initiates an export of the data.
        /// </summary>
        void Export();
        #endregion // Methods
    }
}
