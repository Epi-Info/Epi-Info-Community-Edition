using System;
using System.Collections.Generic;
using System.ComponentModel;
using Epi;

namespace EpiDashboard
{
    public interface IGadgetParameters
    {
        #region Events
        event GadgetStatusUpdateHandler GadgetStatusUpdate;
        event GadgetCheckForCancellationHandler GadgetCheckForCancellation;
        event SetGadgetStatusHandler GadgetCheckForProgress;
        #endregion // Events

        #region Methods
        /// <summary>
        /// Sends a status message back to the gadget
        /// </summary>
        /// <param name="statusMessage">The status message to display</param>
        void UpdateGadgetStatus(string statusMessage);

        /// <summary>
        /// Sends a progress update back to the gadget
        /// </summary>
        /// <param name="progress">The amount of progress</param>
        void UpdateGadgetProgress(double progress);

        /// <summary>
        /// Used to check whether or not the request to generate output has been cancelled
        /// </summary>
        /// <returns>bool</returns>
        bool IsRequestCancelled();
        #endregion // Methods

        #region Properties
        /// <summary>
        /// Gets/sets the worker thread this gadget will be processed on.
        /// </summary>
        BackgroundWorker Worker { get; set; }

        /// <summary>
        /// Gets/sets the gadget's title
        /// </summary>
        string GadgetTitle { get; set; }

        /// <summary>
        /// Gets/sets the gadget's description
        /// </summary>
        string GadgetDescription { get; set; }

        /// <summary>
        /// Gets/sets the input variable list
        /// </summary>
        Dictionary<string, string> InputVariableList { get; set; }

        /// <summary>
        /// Gets/sets the names of the column names to use in processing this gadget
        /// </summary>
        List<string> ColumnNames { get; set; }

        /// <summary>
        /// Gets/sets the list of strata variable names
        /// </summary>
        List<string> StrataVariableNames { get; set; }

        /// <summary>
        /// Gets/sets the desired width of the gadget assuming no stratification
        /// </summary>
        double? Width { get; set; }

        /// <summary>
        /// Gets/sets the desired height of the gadget assuming no stratification
        /// </summary>
        double? Height { get; set; }

        /// <summary>
        /// Gets/sets whether the results should include the labels from comment legal fields
        /// </summary>
        bool ShowCommentLegalLabels { get; set; }

        /// <summary>
        /// Gets/sets the variables by which to sort, if applicable
        /// </summary>
        Dictionary<string, SortOrder> SortVariables { get; set; }

        /// <summary>
        /// Gets/sets a custom filter to use for this gadget (warning: only applicable to certain methods)
        /// </summary>
        /// <remarks>Only applicable to certain methods</remarks>
        string CustomFilter { get; set; }

        /// <summary>
        /// Gets/sets a custom sort order to use for this gadget (warning: only applicable to certain methods)
        /// </summary>
        /// <remarks>Only applicable to certain methods</remarks>
        string CustomSortColumnName { get; set; }

        /// <summary>
        /// Gets/sets whether or not to include missing values in the results and calculations
        /// </summary>
        bool IncludeMissing { get; set; }

        /// <summary>
        /// Gets/sets whether the data operation should ignore any built-in row limits. Generally, not recommended to set to true except for special cases.
        /// </summary>
        bool IgnoreRowLimits { get; set; }

        #endregion // Properties
    }
}
