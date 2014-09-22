using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_DLL_Statement : AnalysisRule
    {
        string Identifier = null;
        string ClassName = null;
        bool isNETDLL = false;

        public Rule_DLL_Statement(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            // DEFINE Identifier DLLOBJECT String
            //DEFINE <variable> DLLOBJECT "<ActiveX name>.<class>" com object
            //DEFINE Week DLLOBJECT "EIEpiwk.Epiweek" 

            //DEFINE <variable> DLLOBJECT "<filename>" windows scripting host
            //DEFINE Global_ID DLLOBJECT "GetGlobalUniqueID.WSC"

            this.Identifier = GetCommandElement(pToken.Tokens, 1);
            this.ClassName = GetCommandElement(pToken.Tokens, 3).Trim('"');
            if (GetCommandElement(pToken.Tokens, 2) == "NETOBJECT")
            {
                this.isNETDLL = true;
            }
        }

                /// <summary>
        /// peforms an assign rule by assigning an expression to a variable.  return the variable that was assigned
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            if(this.isNETDLL)
            {
                cNETDLL o = new cNETDLL(Identifier, this.ClassName);
                if (this.Context.DLLClassList.ContainsKey(this.Identifier.ToLower()))
                {
                    this.Context.DLLClassList.Remove(this.Identifier.ToLower());
                }

                this.Context.DLLClassList.Add(this.Identifier.ToLower(), o);
            }
            else
            {
                cCOMDLL o = new cCOMDLL(Identifier, this.ClassName);
                if (this.Context.DLLClassList.ContainsKey(this.Identifier.ToLower()))
                {
                    this.Context.DLLClassList.Remove(this.Identifier.ToLower());
                }

                this.Context.DLLClassList.Add(this.Identifier.ToLower(), o);
            }
            
            return null;
        }
    }
}
