using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public partial class Rule_ConcatExp : AnalysisRule
    {
        string operand = null;
        private List<AnalysisRule> parameterList = new List<AnalysisRule>();

        public Rule_ConcatExp(Rule_Context context, NonterminalToken token)
            : base(context)
        {
            //  ::= <Add Exp> '&' <Concat Exp> | <Add Exp>

            parameterList.Add(AnalysisRule.BuildStatments(context, token.Tokens[0]));

            if (token.Tokens.Length > 1)
            {
                operand = token.Tokens[1].ToString();
                parameterList.Add(AnalysisRule.BuildStatments(context, token.Tokens[2]));
            }
        }

        public override object Execute()
        {
            object result = null;

            if (operand == null)
            {
                result = parameterList[0].Execute();
            }
            else
            {
                object left = parameterList[0].Execute();
                object right = parameterList[1].Execute();

                if (left != null && right != null)
                {
                    result = left.ToString() + right.ToString();
                }
                else if (left != null)
                {
                    if (left.GetType() == typeof(string))
                    {
                        result = left;
                    }
                }
                else if (right != null)
                {    
                    if (right.GetType() == typeof(string))
                    {
                        result = right;
                    }
                }
            }

            return result;
        }
    }
}
