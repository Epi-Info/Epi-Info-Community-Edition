using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Reduction to get the longitude.
    /// </summary>
    public partial class Rule_SystemLongitude : EnterRule
    {
        public Rule_SystemLongitude(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            //SYSLONGITUDE
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the longitude.</returns>
        public override object Execute()
        {
            return "implement_longitude";
        }
    }
}
