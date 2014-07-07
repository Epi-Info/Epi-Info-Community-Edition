using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;
using System.Data;
using Epi;
using Epi.Data;
using Epi.Data.Services;
using Epi.DataSets;

namespace Epi.Core.EnterInterpreter.Rules
{
    class Rule_Undefine : EnterRule
    {
        private string _identifier = string.Empty;

        public Rule_Undefine(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            _identifier = this.GetCommandElement(pToken.Tokens, 1);
        }

        /// <summary>
        /// performs execution of the UNDEFINE command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object results = null;
            this.Context.CurrentScope.Undefine(_identifier);
            return results;
        }
    }
}
