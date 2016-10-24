using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Reduction to get a barcode reading.
    /// </summary>
    public partial class Rule_SystemBarcode : EnterRule
    {
        public Rule_SystemBarcode(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            //SYSBARCODE
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns a barcode reading.</returns>
        public override object Execute()
        {
            return null;
        }
    }
}
