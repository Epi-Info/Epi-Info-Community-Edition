using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Program : EnterRule 
    {
        EnterRule CheckCodeBlocks = null;

        public Rule_Program(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            //<Program> ::= <CheckCodeBlocks> | !Eof
            if (pToken.Tokens.Length > 0)
            {
                CheckCodeBlocks = EnterRule.BuildStatments(pContext, pToken.Tokens[0]);
            }
        }

                /// <summary>
        /// executes program start
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            if (CheckCodeBlocks != null)
            {
                CheckCodeBlocks.Execute();
            }

            return null;
        }
    }
}
