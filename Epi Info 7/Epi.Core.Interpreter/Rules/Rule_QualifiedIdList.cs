using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_QualifiedIdList : AnalysisRule
    {
        List<string> IdList;

        //<Qualified ID>
        public Rule_QualifiedIdList(Rule_Context pContext, Token pToken)
            : base(pContext)
        {
            IdList = new List<string>();

            this.SetQualifiedIdList(pToken);
            /*<QualifiedIdList> ::= <QualifiedIdList> <Qualified ID>
            | <Qualified ID>



<Qualified ID> ::= Identifier
            | <Fully_Qualified_Id>
<Fully_Qualified_Id> ::= Identifier '.' <Qualified ID>*/

            //<SortList> ::= <SortList> <Sort> |<Sort>
  


        }


        private void SetQualifiedIdList(Token pToken)
        {
            /*
                <QualifiedIdList> ::= <QualifiedIdList> <Qualified ID>
                                | <Qualified ID>
            */
            if (pToken is NonterminalToken)
            {
                NonterminalToken NT = (NonterminalToken)pToken;
                switch (NT.Symbol.ToString())
                {
                    case "<QualifiedIdList>":
                        this.SetQualifiedIdList((NonterminalToken)NT.Tokens[0]);
                        this.IdList.Add(this.SetQualifiedId(NT.Tokens[1]));
                        
                        break;
                    case "<Qualified ID>":
                        this.IdList.Add(SetQualifiedId(NT.Tokens[0]));
                        break;
                }
            }
            else
            {
                this.IdList.Add(this.SetQualifiedId(pToken));
            }


        }



        /// <summary>
        /// performs execution of retrieving the value of a variable or expression
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            //object result = null;


            return IdList;
        }
    }
}
