using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Epi;
using Epi.Fields;

namespace EpiDashboard
{
    public class FrequencyParametersBase : GadgetParametersBase
    {
        #region Events

        #endregion // Events

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public FrequencyParametersBase()
            : base()
        {
            WeightVariableName = string.Empty;
            CrosstabVariableName = string.Empty;
            UseFieldPrompts = true;
            Precision = "2";
            ShowAllListValues = false;
            SortHighToLow = false;
            IncludeMissing = false;
            IncludeFullSummaryStatistics = false;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public FrequencyParametersBase(FrequencyParametersBase parameters)
            : base(parameters)
        {
            WeightVariableName = parameters.WeightVariableName;
            CrosstabVariableName = parameters.CrosstabVariableName;
            UseFieldPrompts = parameters.UseFieldPrompts;
            Precision = parameters.Precision;
            ShowAllListValues = parameters.ShowAllListValues;
            SortHighToLow = parameters.SortHighToLow;
            IncludeMissing = parameters.IncludeMissing;
            IncludeFullSummaryStatistics = parameters.IncludeFullSummaryStatistics;
        }

        #endregion     // Constructors

        #region Properties
        public string Precision { get; set; }
        public bool UseFieldPrompts { get; set; }
        public bool ShowAllListValues { get; set; }
        public bool SortHighToLow { get; set; }
        public string WeightVariableName { get; set; }
        public string CrosstabVariableName { get; set; }
        public bool IncludeFullSummaryStatistics { get; set; }
        #endregion  // Properties

        #region Internal Methods

        #endregion //Internal Methods
    }
}
