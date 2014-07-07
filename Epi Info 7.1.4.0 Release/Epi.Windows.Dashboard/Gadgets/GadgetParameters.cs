using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Epi;

namespace Epi.WPF.Dashboard
{
    /// <summary>
    /// A class used to encompass input parameters, delegates, and events
    /// </summary>
    public class GadgetParameters
    {
        #region Private Members
        /// <summary>
        /// The list of input variables for this operation
        /// </summary>
        private Dictionary<string, string> inputVariableList;

        /// <summary>
        /// The main (or exposure) variable names
        /// </summary>
        private List<string> mainVariableNames;

        /// <summary>
        /// The main (or exposure) variable name
        /// </summary>
        private string mainVariableName;

        /// <summary>
        /// The column names to select
        /// </summary>
        private List<string> columnNames;

        /// <summary>
        /// The cross-tab (or outcome) variable name
        /// </summary>
        private string crosstabVariableName;

        /// <summary>
        /// The list of strata variable names
        /// </summary>
        private List<string> strataVariableNames;

        /// <summary>
        /// The weight variable name
        /// </summary>
        private string weightVariableName;

        /// <summary>
        /// The variable name for a primary sampling unit; used for complex sampling only
        /// </summary>
        private string psuVariableName;

        /// <summary>
        /// Whether results should include missing values
        /// </summary>
        private bool shouldIncludeMissing;

        /// <summary>
        /// Whether results should be sorted high-to-low; if not, they will be sorted alphabetically
        /// </summary>
        private bool shouldSortHighToLow;

        /// <summary>
        /// Whether the results should include all possible list values; applicable only for drop-down list fields in Epi Info 7 projects
        /// </summary>
        private bool shouldUseAllPossibleValues;

        /// <summary>
        /// Whether the results should include label values from comment legal fields
        /// </summary>
        private bool shouldShowCommentLegalLabels;

        /// <summary>
        /// Whether the results should include the full set of summary statistics (median, mode, mean, variance, etc); if not, only the total number of observations will be calculated
        /// </summary>
        private bool shouldIncludeFullSummaryStatistics;

        /// <summary>
        /// Whether the results should ignore any built-in row limits. 
        /// </summary>
        private bool shouldIgnoreRowLimits;

        /// <summary>
        /// A custom data filter to use
        /// </summary>
        private string customFilter;

        /// <summary>
        /// A custom sort order to use
        /// </summary>
        private string customSortColumnName;
        #endregion // Private Members

        #region Events
        public event GadgetStatusUpdateHandler GadgetStatusUpdate;
        public event GadgetCheckForCancellationHandler GadgetCheckForCancellation;
        public event SetGadgetStatusHandler GadgetCheckForProgress;
        #endregion // Events

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public GadgetParameters()
        {
            columnNames = new List<string>();
            GadgetStatusUpdate = null;
            inputVariableList = new Dictionary<string, string>();
            CustomFilter = string.Empty;
            CustomSortColumnName = string.Empty;
            ShouldIncludeFullSummaryStatistics = false;
            ShouldUseAllPossibleValues = false;
            ShouldShowCommentLegalLabels = false;
            ShouldSortHighToLow = false;
            ShouldIncludeMissing = false;
            CrosstabVariableName = string.Empty;
            WeightVariableName = string.Empty;
            StrataVariableNames = new List<string>();
            ShouldIgnoreRowLimits = false;
        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        public GadgetParameters(GadgetParameters parameters)
        {
            MainVariableNames = parameters.MainVariableNames;
            MainVariableName = parameters.MainVariableName;
            columnNames = parameters.columnNames;
            GadgetStatusUpdate = parameters.GadgetStatusUpdate;
            inputVariableList = parameters.inputVariableList;
            CustomFilter = parameters.CustomFilter;
            CustomSortColumnName = parameters.CustomSortColumnName;
            ShouldIncludeFullSummaryStatistics = parameters.ShouldIncludeFullSummaryStatistics;
            ShouldUseAllPossibleValues = parameters.ShouldUseAllPossibleValues;
            ShouldShowCommentLegalLabels = parameters.ShouldShowCommentLegalLabels;
            ShouldSortHighToLow = parameters.ShouldSortHighToLow;
            ShouldIncludeMissing = parameters.ShouldIncludeMissing;
            CrosstabVariableName = parameters.CrosstabVariableName;
            WeightVariableName = parameters.WeightVariableName;
            StrataVariableNames = parameters.StrataVariableNames;
            ShouldIgnoreRowLimits = parameters.ShouldIgnoreRowLimits;
        }
        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Gets/sets the input variable list
        /// </summary>
        public Dictionary<string, string> InputVariableList
        {
            get
            {
                return this.inputVariableList;
            }
            set
            {
                this.inputVariableList = value;
            }
        }

        /// <summary>
        /// Gets/sets the names of the main variables
        /// </summary>
        public List<string> MainVariableNames
        {
            get
            {
                return this.mainVariableNames;
            }
            set
            {
                this.mainVariableNames = value;
            }
        }

        /// <summary>
        /// Gets/sets the name of the main variable
        /// </summary>
        public string MainVariableName
        {
            get
            {
                return this.mainVariableName;
            }
            set
            {
                this.mainVariableName = value;
            }
        }

        /// <summary>
        /// Gets/sets the names of the column names to use in processing this gadget
        /// </summary>
        public List<string> ColumnNames
        {
            get
            {
                return this.columnNames;
            }
            set
            {
                this.columnNames = value;
            }
        }

        /// <summary>
        /// Gets/sets the name of the crosstab variable
        /// </summary>
        public string CrosstabVariableName
        {
            get
            {
                return this.crosstabVariableName;
            }
            set
            {
                this.crosstabVariableName = value;
            }
        }

        /// <summary>
        /// Gets/sets the name of the crosstab variable
        /// </summary>
        public string WeightVariableName
        {
            get
            {
                return this.weightVariableName;
            }
            set
            {
                this.weightVariableName = value;
            }
        }

        /// <summary>
        /// Gets/sets the name of the PSU variable
        /// </summary>
        public string PSUVariableName
        {
            get
            {
                return this.psuVariableName;
            }
            set
            {
                this.psuVariableName = value;
            }
        }

        /// <summary>
        /// Gets/sets the list of strata variable names
        /// </summary>
        public List<string> StrataVariableNames
        {
            get
            {
                return this.strataVariableNames;
            }
            set
            {
                this.strataVariableNames = value;
            }
        }

        /// <summary>
        /// Gets/sets whether or not to include missing values in the results and calculations
        /// </summary>
        public bool ShouldIncludeMissing
        {
            get
            {
                return this.shouldIncludeMissing;
            }
            set
            {
                this.shouldIncludeMissing = value;
            }
        }

        /// <summary>
        /// Gets/sets whether or not to sort results in high-to-low fashion
        /// </summary>
        public bool ShouldSortHighToLow
        {
            get
            {
                return this.shouldSortHighToLow;
            }
            set
            {
                this.shouldSortHighToLow = value;
            }
        }

        /// <summary>
        /// Gets/sets whether the results should include all possible list values; applicable only for drop-down list fields in Epi Info 7 projects
        /// </summary>
        public bool ShouldUseAllPossibleValues
        {
            get
            {
                return this.shouldUseAllPossibleValues;
            }
            set
            {
                this.shouldUseAllPossibleValues = value;
            }
        }

        /// <summary>
        /// Gets/sets whether the results should include the labels from comment legal fields
        /// </summary>
        public bool ShouldShowCommentLegalLabels
        {
            get
            {
                return this.shouldShowCommentLegalLabels;
            }
            set
            {
                this.shouldShowCommentLegalLabels = value;
            }
        }

        /// <summary>
        /// Gets/sets whether the results should include the full set of summary statistics (median, mode, mean, variance, etc); if not, only the total number of observations will be calculated
        /// </summary>
        public bool ShouldIncludeFullSummaryStatistics
        {
            get
            {
                return this.shouldIncludeFullSummaryStatistics;
            }
            set
            {
                this.shouldIncludeFullSummaryStatistics = value;
            }
        }

        /// <summary>
        /// Gets/sets whether the data operation should ignore any built-in row limits. Generally, not recommended to set to true except for special cases.
        /// </summary>
        public bool ShouldIgnoreRowLimits
        {
            get
            {
                return this.shouldIgnoreRowLimits;
            }
            set
            {
                this.shouldIgnoreRowLimits = value;
            }
        }

        /// <summary>
        /// Gets/sets a custom filter to use for this gadget (warning: only applicable to certain methods)
        /// </summary>
        /// <remarks>Only applicable to certain methods</remarks>
        public string CustomFilter
        {
            get
            {
                return customFilter;
            }
            set
            {
                customFilter = value;
            }
        }

        /// <summary>
        /// Gets/sets a custom sort order to use for this gadget (warning: only applicable to certain methods)
        /// </summary>
        /// <remarks>Only applicable to certain methods</remarks>
        public string CustomSortColumnName
        {
            get
            {
                return customSortColumnName;
            }
            set
            {
                customSortColumnName = value;
            }
        }
        #endregion // Public Properties

        #region Internal Methods
        /// <summary>
        /// Sends a status message back to the gadget
        /// </summary>
        /// <param name="statusMessage">The status message to display</param>
        internal void UpdateGadgetStatus(string statusMessage)
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
        internal void UpdateGadgetProgress(double progress)
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
        internal bool IsRequestCancelled()
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
