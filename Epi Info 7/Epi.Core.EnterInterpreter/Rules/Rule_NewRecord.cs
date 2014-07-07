using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    class Rule_NewRecord:EnterRule
    {
        public Rule_NewRecord(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
        }

        /// <summary>
        /// adds a new record via the EnterCheckCodeInterface.NewRecord() method
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object results = null;
            this.Context.EnterCheckCodeInterface.NewRecord();
            return results;
        }
    }
}
