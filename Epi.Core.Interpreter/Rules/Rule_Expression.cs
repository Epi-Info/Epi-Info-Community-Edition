using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public partial class Rule_Expression : AnalysisRule 
    {
        //AnalysisRule And_Exp = null;
        string op = null;
        //AnalysisRule Expression = null;

        private List<AnalysisRule> ParameterList = new List<AnalysisRule>();

        public string CommandText = null;
        public Rule_Expression(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            /*::= <And Exp> OR <Expression>
		  						| <And Exp> XOR <Expression>
               							| <And Exp> */

            this.CommandText = this.ExtractTokens(pToken.Tokens);

            //And_Exp = new Rule_AndExp(pContext, (NonterminalToken)pTokens.Tokens[0]);
            this.ParameterList.Add(AnalysisRule.BuildStatments(pContext, pToken.Tokens[0]));
            if(pToken.Tokens.Length > 1)
            {
                    op = pToken.Tokens[1].ToString().ToUpperInvariant();
                    //Expression = new Rule_Expression(pContext, (NonterminalToken)pTokens.Tokens[2]);
                    this.ParameterList.Add(AnalysisRule.BuildStatments(pContext, pToken.Tokens[2]));
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
                //result = this.And_Exp.Execute();
                result = this.ParameterList[0].Execute();
            }
            else
            {
                if (this.ParameterList[0].Execute().ToString().Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    result = AndResult = true;
                }

                if (op != null)
                {

                    bool ExpressionResult = false;


                    if (this.ParameterList[1].Execute().ToString().Equals("true", StringComparison.CurrentCultureIgnoreCase))
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
