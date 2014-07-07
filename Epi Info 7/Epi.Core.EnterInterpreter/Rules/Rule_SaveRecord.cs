using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    class Rule_SaveRecord:EnterRule
    {
        public Rule_SaveRecord(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
        }

        /// <summary>
        /// saves a record via the EnterCheckCodeInterface.SaveRecord() method
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object results = null;
            this.Context.EnterCheckCodeInterface.SaveRecord();
            return results;
        }
    }
}
