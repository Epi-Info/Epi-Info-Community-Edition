using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Expression : EnterRule 
    {
        EnterRule And_Exp = null;
        string op = null;
        EnterRule Expression = null;
        public string CommandText = null;
        public Rule_Expression(Rule_Context pContext, NonterminalToken pTokens) : base(pContext)
        {
            /*::= <And Exp> OR <Expression>
		  						| <And Exp> XOR <Expression>
               							| <And Exp> */

            this.CommandText = this.ExtractTokens(pTokens.Tokens);

            And_Exp = EnterRule.BuildStatments(pContext, pTokens.Tokens[0]);
            if(pTokens.Tokens.Length > 1)
            {
                    op = pTokens.Tokens[1].ToString().ToUpperInvariant();
                    Expression = EnterRule.BuildStatments(pContext, pTokens.Tokens[2]);
            }
        }

        /// <summary>
        /// performs execution of an 'OR' or 'XOR' expression
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;
            bool AndResult = false;

            if(op == null)
            {
                result = this.And_Exp.Execute();
            }
            else
            {
                if (this.And_Exp.Execute().ToString().Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    result = AndResult = true;
                }

                if (op != null)
                {

                    bool ExpressionResult = false;


                    if (this.Expression.Execute().ToString().Equals("true", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ExpressionResult = true;
                    }

                    switch (op)
                    {
                        case "OR":
                            AndResult = AndResult || ExpressionResult;
                            break;
                        case "XOR":
                            AndResult = AndResult != ExpressionResult;
                            break;
                    }
                    result = BoolVal(AndResult);
                }
            }

            return result;
        }



    }
}
