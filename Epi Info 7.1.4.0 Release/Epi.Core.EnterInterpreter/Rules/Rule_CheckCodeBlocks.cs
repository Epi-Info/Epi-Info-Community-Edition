using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_CheckCodeBlocks : EnterRule 
    {
        EnterRule CheckCodeBlock = null;
        EnterRule CheckCodeBlocks = null;

        public Rule_CheckCodeBlocks(Rule_Context pContext, NonterminalToken pToken)
        {
            //<CheckCodeBlocks> ::= <CheckCodeBlock> <CheckCodeBlocks> | <CheckCodeBlock>
            if (pToken.Tokens.Length > 1)
            {
                CheckCodeBlock = EnterRule.BuildStatments(pContext, pToken.Tokens[0]);
                CheckCodeBlocks = EnterRule.BuildStatments(pContext, pToken.Tokens[1]);
            }
            else
            {
                CheckCodeBlock = EnterRule.BuildStatments(pContext, pToken.Tokens[0]);
            }
        }

        /// <summary>
        ///executes CheckCodeBlock statement
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            CheckCodeBlock.Execute();

            if (CheckCodeBlocks != null)
            {
                CheckCodeBlocks.Execute();
            }

            return null;
        }
    }
}
