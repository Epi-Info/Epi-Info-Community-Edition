using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Net;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_IOCode : EnterRule
    {
        private string industry_field_name;
        private string occupation_field_name;
        private string icode_field_name;
        private string ocode_field_name;
        private string ititle_field_name;
        private string otitle_field_name;
        private string scheme_field_name;

        public Rule_IOCode(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            // IOCode industry_field_name, occupation_field_name, icode_field_name, ocode_field_name, ititle_field_name, otitle_field_name, scheme_field_name
            //<IOCode_Statement> ::= IOCode Identifier ',' Identifier ',' Identifier ',' Identifier ',' Identifier ',' Identifier ',' Identifier
            this.industry_field_name = this.GetCommandElement(pToken.Tokens, 1);
            this.occupation_field_name = this.GetCommandElement(pToken.Tokens, 3);
            this.icode_field_name = this.GetCommandElement(pToken.Tokens, 5);
            this.ocode_field_name = this.GetCommandElement(pToken.Tokens, 7);
            this.ititle_field_name = this.GetCommandElement(pToken.Tokens, 9);
            this.otitle_field_name = this.GetCommandElement(pToken.Tokens, 11);
            this.scheme_field_name = this.GetCommandElement(pToken.Tokens, 13);
        }

        public override object Execute()
        {
            if (base.Context.EnterCheckCodeInterface.GetValue(industry_field_name) != null)
            {
                string iinput = base.Context.EnterCheckCodeInterface.GetValue(industry_field_name).Trim();
                string oinput = base.Context.EnterCheckCodeInterface.GetValue(occupation_field_name).Trim();
                base.Context.EnterCheckCodeInterface.IOCode(iinput, oinput, industry_field_name, occupation_field_name, icode_field_name, ocode_field_name, ititle_field_name, otitle_field_name, scheme_field_name);
            }
            else
            {
                EpiInfo.Plugin.IVariable varI = Context.CurrentScope.Resolve(industry_field_name, null);
                EpiInfo.Plugin.IVariable varO = Context.CurrentScope.Resolve(occupation_field_name, null);

                if (varI.VariableScope == EpiInfo.Plugin.VariableScope.Standard && varO.VariableScope == EpiInfo.Plugin.VariableScope.Standard)
                {
                    base.Context.EnterCheckCodeInterface.IOCode(varI.Expression, varI.Expression, industry_field_name, occupation_field_name, icode_field_name, ocode_field_name, ititle_field_name, otitle_field_name, scheme_field_name);
                }
            }

            return null;
        }
    }
}
