using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Epi;

namespace EpiDashboard
{
    public class GadgetParametersBase : IGadgetParameters
    {

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public GadgetParametersBase()
        {
            GadgetTitle = String.Empty;
            GadgetDescription = String.Empty;
            InputVariableList = new Dictionary<string, string>();
            ColumnNames = new List<string>();
            StrataVariableNames = new List<string>();
            ShowCommentLegalLabels = false;
            CustomFilter = String.Empty;
            SortVariables = new Dictionary<string, SortOrder>();
            CustomSortColumnName = String.Empty;
            IgnoreRowLimits = false;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public GadgetParametersBase(GadgetParametersBase parameters)
        {
            GadgetTitle = parameters.GadgetTitle;
            GadgetDescription = parameters.GadgetDescription;
            InputVariableList = parameters.InputVariableList;
            ColumnNames = new List<string>();
            foreach (string columnName in parameters.ColumnNames)
            {
                ColumnNames.Add(columnName);
            }
            StrataVariableNames = new List<string>();
            foreach (string strataVar in parameters.StrataVariableNames)
            {
                StrataVariableNames.Add(strataVar);
            }
            ShowCommentLegalLabels = parameters.ShowCommentLegalLabels;
            CustomFilter = parameters.CustomFilter;
            SortVariables = parameters.SortVariables;
            CustomSortColumnName = parameters.CustomSortColumnName;
            IgnoreRowLimits = parameters.IgnoreRowLimits;
        }

        #endregion // Constructors

        #region Events
        public event GadgetStatusUpdateHandler GadgetStatusUpdate;
        public event GadgetCheckForCancellationHandler GadgetCheckForCancellation;
        public event SetGadgetStatusHandler GadgetCheckForProgress;
        #endregion // Events

        #region Properties
        /// <summary>
        /// Gets/sets the worker thread this gadget will be processed on.
        /// </summary>
        public BackgroundWorker Worker { get; set; }

        /// <summary>
        /// Gets/sets the gadget's title
        /// </summary>
        public string GadgetTitle { get; set; }

        /// <summary>
        /// Gets/sets the gadget's description
        /// </summary>
        public string GadgetDescription { get; set; }

        /// <summary>
        /// Gets/sets the input variable list
        /// </summary>
        public Dictionary<string, string> InputVariableList { get; set; }

        /// <summary>
        /// Gets/sets the names of the column names to use in processing this gadget
        /// </summary>
        public List<string> ColumnNames { get; set; }

        /// <summary>
        /// Gets/sets the list of strata variable names
        /// </summary>
        public List<string> StrataVariableNames { get; set; }

        /// <summary>
        /// Gets/sets the desired width of the gadget assuming no stratification
        /// </summary>
        public double? Width { get; set; }

        /// <summary>
        /// Gets/sets the desired height of the gadget assuming no stratification
        /// </summary>
        public double? Height { get; set; }

        /// <summary>
        /// Gets/sets whether the results should include the labels from comment legal fields
        /// </summary>
        public bool ShowCommentLegalLabels { get; set; }

        /// <summary>
        /// Gets/sets a custom filter to use for this gadget (warning: only applicable to certain methods)
        /// </summary>
        /// <remarks>Only applicable to certain methods</remarks>
        public string CustomFilter { get; set; }

        /// <summary>
        /// Gets/sets the variables by which to sort, if applicable
        /// </summary>
        public Dictionary<string, SortOrder> SortVariables { get; set; }

        /// <summary>
        /// Gets/sets a custom sort order to use for this gadget (warning: only applicable to certain methods)
        /// </summary>
        /// <remarks>Only applicable to certain methods</remarks>
        public string CustomSortColumnName { get; set; }

        /// <summary>
        /// Gets/sets whether or not to include missing values in the results and calculations
        /// </summary>
        public bool IncludeMissing { get; set; }

        /// <summary>
        /// Gets/sets whether the data operation should ignore any built-in row limits. Generally, not recommended to set to true except for special cases.
        /// </summary>
        public bool IgnoreRowLimits { get; set; }

        #endregion // Properties

        #region Internal Methods
        /// <summary>
        /// Sends a status message back to the gadget
        /// </summary>
        /// <param name="statusMessage">The status message to display</param>
        public void UpdateGadgetStatus(string statusMessage)
        {
            if (GadgetStatusUpdate != null)
            {
                GadgetStatusUpdate(statusMessage);
            }
        }

        /// <summary>
        /// Sends a progress update back to the gadget
        /// </summary>
        /// <param name="progress">The amount of progress</param>
        public void UpdateGadgetProgress(double progress)
        {
            if (GadgetCheckForProgress != null)
            {
                GadgetCheckForProgress("", progress);
            }
        }

        /// <summary>
        /// Used to check whether or not the request to generate output has been cancelled
        /// </summary>
        /// <returns>bool</returns>
        public bool IsRequestCancelled()
        {
            if (GadgetCheckForCancellation != null)
            {
                return GadgetCheckForCancellation();
            }
            return false;
        }
        #endregion // Internal Methods
    }
}
