using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Net;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Geocode : EnterRule
    {
        private string address_field_name;
        private string latitude_field_name;
        private string longitude_field_name;

        public Rule_Geocode(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            // Geocode address_field_name, latitude_field_name, longitude_field_name
            //<Geocode_Statement> ::= Geocode Identifier ',' Identifier ',' Identifier
            this.address_field_name = this.GetCommandElement(pToken.Tokens, 1);
            this.latitude_field_name = this.GetCommandElement(pToken.Tokens, 3);
            this.longitude_field_name = this.GetCommandElement(pToken.Tokens, 5);

            if (!string.IsNullOrEmpty(address_field_name))
            {

                if (!this.Context.CommandVariableCheck.ContainsKey(address_field_name.ToLower()))
                {
                    this.Context.CommandVariableCheck.Add(address_field_name, "Geocode");
                }

            }
            if (!string.IsNullOrEmpty(latitude_field_name))
            {

                if (!this.Context.CommandVariableCheck.ContainsKey(latitude_field_name.ToLower()))
                {
                    this.Context.CommandVariableCheck.Add(latitude_field_name, "Geocode");
                }

            }
            if (!string.IsNullOrEmpty(longitude_field_name))
            {

                if (!this.Context.CommandVariableCheck.ContainsKey(longitude_field_name.ToLower()))
                {
                    this.Context.CommandVariableCheck.Add(longitude_field_name, "Geocode");
                }

            }
        }

        public override object Execute()
        {
            if (base.Context.EnterCheckCodeInterface.GetValue(address_field_name) != null)
            {
                string address = base.Context.EnterCheckCodeInterface.GetValue(address_field_name).Trim();
                base.Context.EnterCheckCodeInterface.Geocode(address, latitude_field_name, longitude_field_name);
            }
            else
            {
                EpiInfo.Plugin.IVariable var = Context.CurrentScope.Resolve(address_field_name, null);

                if (var.VariableScope == EpiInfo.Plugin.VariableScope.Standard)
                {                    
                    base.Context.EnterCheckCodeInterface.Geocode(var.Expression, latitude_field_name, longitude_field_name);
                }
            }

            return null;
        }
    }
}
