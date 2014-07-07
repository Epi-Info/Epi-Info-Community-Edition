using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Clear :EnterRule
    {
        string[] IdentifierList = null;

        public Rule_Clear(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            //<Clear_Statement>	::= CLEAR <IdentifierList>
            this.IdentifierList = this.GetCommandElement(pToken.Tokens, 1).ToString().Split(' ');
        }

        /// <summary>
        /// uses the EnterCheckCodeInterface to perform a clear command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            this.Context.EnterCheckCodeInterface.Clear(this.IdentifierList);

            return null;
        }
    }
}
