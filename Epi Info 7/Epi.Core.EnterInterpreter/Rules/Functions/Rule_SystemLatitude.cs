using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Reduction to get the latitude.
    /// </summary>
    public partial class Rule_SystemLatitude : EnterRule
    {
        public Rule_SystemLatitude(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            //SYSTEMLATITUDE
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the latitude.</returns>
        public override object Execute()
        {
            return "implement_latitude";
        }
    }
}
