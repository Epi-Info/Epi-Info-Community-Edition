﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public partial class Rule_UpperCase : EnterRule
    {
        
        private List<EnterRule> ParameterList = new List<EnterRule>();


        public Rule_UpperCase(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.ParameterList = EnterRule.GetFunctionParameters(pContext, pToken);
            
        }
        /// <summary>
        /// returns a substring index is 1 based ie 1 = first character
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = this.ParameterList[0].Execute();

            if(result != null)
            {
                result = result.ToString().ToUpper();
            }

            return result;
        }
    }
}
