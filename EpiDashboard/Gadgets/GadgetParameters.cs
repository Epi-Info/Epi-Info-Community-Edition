using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Epi;

namespace EpiDashboard
{
    /// <summary>
    /// A class used to encompass input parameters, delegates, and events
    /// </summary>
    [Obsolete("Use gadget-specific parameters objects starting in 7.1.4.0", false)]
    public class GadgetParameters : GadgetParametersBase
    {
        #region Events
        public new event GadgetStatusUpdateHandler GadgetStatusUpdate;
        //public event GadgetCheckForCancellationHandler GadgetCheckForCancellation;
        //public event SetGadgetStatusHandler GadgetCheckForProgress;
        #endregion // Events

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public GadgetParameters() : base()
        {
            ColumnNames = new List<string>();
            GadgetStatusUpdate = null;
            InputVariableList = new Dictionary<string, string>();
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
            SortVariables = new Dictionary<string, SortOrder>();
        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        public GadgetParameters(GadgetParameters parameters)
        {
            MainVariableNames = parameters.MainVariableNames;
            MainVariableName = parameters.MainVariableName;
            ColumnNames = parameters.ColumnNames;
            GadgetStatusUpdate = parameters.GadgetStatusUpdate;
            InputVariableList = parameters.InputVariableList;
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
            SortVariables = parameters.SortVariables;
        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        public GadgetParameters(IGadgetParameters parameters)
        {
            InputVariableList = parameters.InputVariableList;
            CustomFilter = parameters.CustomFilter;
            CustomSortColumnName = parameters.CustomSortColumnName;
            StrataVariableNames = parameters.StrataVariableNames;
            SortVariables = parameters.SortVariables;
        }
        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Gets/sets the number of rows that should be displayed
        /// </summary>
        public int? RowsToDisplay { get; set; }

        /// <summary>
        /// Gets/sets the desired height of the gadget assuming no stratification
        /// </summary>
        public double? Height { get; set; }

        /// <summary>
        /// Gets/sets the desired width of the gadget assuming no stratification
        /// </summary>
        public double? Width { get; set; }

        /// <summary>
        /// Gets/sets the input variable list
        /// </summary>
        public Dictionary<string, string> InputVariableList { get; set; }

        /// <summary>
        /// Gets/sets the names of the main variables
        /// </summary>
        public List<string> MainVariableNames { get; set; }

        /// <summary>
        /// Gets/sets the name of the main variable
        /// </summary>
        public string MainVariableName { get; set; }

        /// <summary>
        /// Gets/sets the names of the column names to use in processing this gadget
        /// </summary>
        public List<string> ColumnNames { get; set; }

        /// <summary>
        /// Gets/sets the name of the crosstab variable
        /// </summary>
        public string CrosstabVariableName { get; set; }

        /// <summary>
        /// Gets/sets the name of the crosstab variable
        /// </summary>
        public string WeightVariableName { get; set; }

        /// <summary>
        /// Gets/sets the name of the PSU variable
        /// </summary>
        public string PSUVariableName { get; set; }

        /// <summary>
        /// Gets/sets the list of strata variable names
        /// </summary>
        public List<string> StrataVariableNames { get; set; }

        /// <summary>
        /// Gets/sets whether or not to include missing values in the results and calculations
        /// </summary>
        public bool ShouldIncludeMissing { get; set; }

        /// <summary>
        /// Gets/sets whether or not to sort results in high-to-low fashion
        /// </summary>
        public bool ShouldSortHighToLow { get; set; }

        /// <summary>
        /// Gets/sets whether the results should include all possible list values; applicable only for drop-down list fields in Epi Info 7 projects
        /// </summary>
        public bool ShouldUseAllPossibleValues { get; set; }

        /// <summary>
        /// Gets/sets whether the results should include the labels from comment legal fields
        /// </summary>
        public bool ShouldShowCommentLegalLabels { get; set; }

        /// <summary>
        /// Gets/sets whether the results should include the full set of summary statistics (median, mode, mean, variance, etc); if not, only the total number of observations will be calculated
        /// </summary>
        public bool ShouldIncludeFullSummaryStatistics { get; set; }

        /// <summary>
        /// Gets/sets whether the data operation should ignore any built-in row limits. Generally, not recommended to set to true except for special cases.
        /// </summary>
        public bool ShouldIgnoreRowLimits { get; set; }

        /// <summary>
        /// Gets/sets a custom filter to use for this gadget (warning: only applicable to certain methods)
        /// </summary>
        /// <remarks>Only applicable to certain methods</remarks>
        public string CustomFilter { get; set; }

        /// <summary>
        /// Gets/sets a custom sort order to use for this gadget (warning: only applicable to certain methods)
        /// </summary>
        /// <remarks>Only applicable to certain methods</remarks>
        public string CustomSortColumnName { get; set; }
        #endregion // Public Properties
    }
}
