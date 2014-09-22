using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    /// <summary>
    /// Reduction to get the system date
    /// </summary>
    public partial class Rule_SystemDate : AnalysisRule
    {
        public Rule_SystemDate(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            //SYSTEMDATE
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the current system date.</returns>
        public override object Execute()
        {
            return DateTime.Today;
        }
    }
}
