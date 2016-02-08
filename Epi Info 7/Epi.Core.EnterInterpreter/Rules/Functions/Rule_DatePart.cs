using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_DatePart reduction
    /// </summary>
    public abstract partial class Rule_DatePart : EnterRule
    {
        #region Private Variables

        private List<EnterRule> ParameterList = new List<EnterRule>();

        private EnterRule functionCall = null;
        private Rule_FunctionParameterList ParamList = null;
        private FunctionUtils.DateInterval currentInterval;
        #endregion
        #region Constructors
        public Rule_DatePart(Rule_Context pContext, NonterminalToken pToken, FunctionUtils.DateInterval interval)
            : base(pContext)
        {
            currentInterval = interval;

            this.ParameterList = EnterRule.GetFunctionParameters(pContext, pToken);
            if (ParameterList.Count > 0)
            {
                foreach (var item in ParameterList)
                {
                    if (item is Rule_Value)
                    {
                        var id = ((Epi.Core.EnterInterpreter.Rules.Rule_Value)(item)).Id;
                        if (!this.Context.CommandVariableCheck.ContainsKey(id.ToLower()))
                        {
                            this.Context.CommandVariableCheck.Add(id.ToLower(), "datapart");
                        }
                    }
                }
            }
            /*
            NonterminalToken T = (NonterminalToken)pToken.Tokens[0];
            string type = pToken.Rule.Lhs.ToString();
            switch (type)
            {
                case "<FunctionParameterList>":
                    this.ParamList = new Rule_FunctionParameterList(pContext, T);
                    break;
                case "<FunctionCall>":
                    this.functionCall = new Rule_FunctionCall(pContext, T);
                    break;
                default:
                    break;
            }*/
        }
        #endregion
        #region Public Methods
        /// <summary>
        /// returns DatePart
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            DateTime date1;
            if (DateTime.TryParse(this.ParameterList[0].Execute().ToString(), out date1))
            {
                result = FunctionUtils.GetDatePart(currentInterval, date1);
            }

            return result;
        }

        #endregion
    }

}
