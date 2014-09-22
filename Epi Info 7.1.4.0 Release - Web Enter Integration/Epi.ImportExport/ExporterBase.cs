using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Epi.Data;

namespace Epi.ImportExport
{
    /// <summary>
    /// A base class used for data exporting.
    /// </summary>
    public abstract class ExporterBase : IDataExporter
    {
        #region Protected Members
        protected View view;
        protected DataView dataView;
        protected List<TableColumn> columnList;
        protected ColumnSortOrder columnSortOrder;
        protected bool exportAllFields;
        #endregion // Protected Members

        #region Events
        public event SetProgressBarDelegate SetProgressBar;
        public event UpdateStatusEventHandler SetStatus;
        public event SetProgressAndStatusHandler SetProgressAndStatus;
        public event CheckForCancellationHandler CheckForCancellation;
        #endregion // Events

        #region Public Properties
        /// <summary>
        /// Gets/sets the order in which to sort columns.
        /// </summary>
        public ColumnSortOrder ColumnSortOrder
        {
            get
            {
                return this.columnSortOrder;
            }
            set
            {
                this.columnSortOrder = value;
            }
        }

        /// <summary>
        /// Gets the current data view that will be exported.
        /// </summary>
        public DataView DataView
        {
            get
            {
                return this.dataView;
            }
        }

        /// <summary>
        /// Gets/sets whether to export all current fields.
        /// </summary>
        public bool ExportAllFields
        {
            get
            {
                return this.exportAllFields;
            }
            set
            {
                this.exportAllFields = value;
            }
        }
        #endregion // Public Properties

        #region Public Methods
        /// <summary>
        /// Attaches an Epi Info 7 form to the exporter.
        /// </summary>
        /// <param name="view">The Epi Info 7 form to attach</param>
        public void AttachView(View view)
        {
            this.view = view;
        }

        /// <summary>
        /// Initiates an export of the data.
        /// </summary>
        public abstract void Export();
        #endregion // Public Methods

        #region Protected Methods
        /// <summary>
        /// Adds a status message
        /// </summary>
        /// <param name="message">The message</param>
        protected void OnSetStatusMessage(string message)
        {
            if (SetStatus != null)
            {
                SetStatus(message);
            }
        }

        /// <summary>
        /// Adds a status message and updates the progress count
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="progress">The amount of progress to increase</param>
        protected void OnSetStatusMessageAndProgressCount(string message, double progress)
        {
            if (SetProgressAndStatus != null)
            {
                SetProgressAndStatus(message, progress);
            }
        }
        #endregion // Protected Methods
    }
}
