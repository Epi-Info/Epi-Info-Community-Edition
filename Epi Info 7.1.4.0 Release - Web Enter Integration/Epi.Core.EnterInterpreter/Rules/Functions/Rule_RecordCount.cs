using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{

    public partial class Rule_RecordCount : EnterRule
    {
        public Rule_RecordCount(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            //RECORDCOUNT()

        }


        /// <summary>
        /// returns the number of records in the current data set
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = this.Context.EnterCheckCodeInterface.RecordCount();

            return result;
        }
    }
}
