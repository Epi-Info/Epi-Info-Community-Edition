using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Reduction to insert a line break into a text 
    /// </summary>
    public partial class Rule_LineBreak : EnterRule
    {
        public Rule_LineBreak(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            // LineBreak
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns a single line break character.</returns>
        public override object Execute()
        {
            return Environment.NewLine;
        }
    }
}
