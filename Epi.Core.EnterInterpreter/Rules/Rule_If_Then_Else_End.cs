using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_If_Then_Else_End : EnterRule 
    {
        EnterRule IfClause;
        EnterRule ThenClause;
        EnterRule ElseClause;

        public Rule_If_Then_Else_End(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            /*
              
            <If_Statement>                  ::=   IF <Expression> THEN  <Statements>  END-IF 
                                                | IF <Expression> THEN  <Statements>  END
            <If_Else_Statement>              ::=  IF <Expression> THEN  <Statements> <Else_If_Statement>  END-IF 
                                                | IF <Expression> THEN  <Statements>  <Else_If_Statement>  END
                                                    IF <Expression> THEN <Statements> ELSE  <Statements>  END-IF 
                                                | IF <Expression> THEN <Statements> ELSE  <Statements>  END
             */

            IfClause = EnterRule.BuildStatments(pContext, pToken.Tokens[1]);
            var IdentifierList = this.GetCommandElement(pToken.Tokens, 1).ToString().Split(' ');
            IdentifierList = RemoveOp(IdentifierList);

            if (IdentifierList.Length > 0)
            {
                foreach (var item in IdentifierList)
                {
                    if (!this.Context.CommandVariableCheck.ContainsKey(item.ToLower()))
                    {
                        this.Context.CommandVariableCheck.Add(item, "If");
                    }
                }
            }




            ThenClause = EnterRule.BuildStatments(pContext, pToken.Tokens[3]);
            if (this.GetCommandElement(pToken.Tokens, 4).Equals("Else", StringComparison.OrdinalIgnoreCase))
            {
                ElseClause = EnterRule.BuildStatments(pContext, pToken.Tokens[5]);
            }

          //var temp =   (( Rule_Value)((( Rule_CompareExp)(IfClause)).CompareExp)).Id;
                /*
            else
            {
                ElseClause = EnterRule.BuildStatments(pContext, pToken.Tokens[4]);
            }*/
        }

        private string[] RemoveOp(string[] IdentifierList)
        {
            List<string>  NewList= new List<string>();
            List<string> OpList = new List<string>();
            
             
           
            OpList.Add("(");
            OpList.Add(")");
            OpList.Add("/");
            OpList.Add("(.)");
            OpList.Add("(+)");
            OpList.Add("(-)");
            OpList.Add("=");
            OpList.Add("+");
            OpList.Add("-");
            OpList.Add(">");
            OpList.Add("<");
            OpList.Add(">=");
            OpList.Add("<=");
            OpList.Add("<>");
            OpList.Add("^");
            OpList.Add("&");
            OpList.Add("*");
            OpList.Add("%");
            OpList.Add("mod");
            OpList.Add("(.)");
            OpList.Add("not");
            OpList.Add("or");
            OpList.Add("and");
            OpList.Add("xor"); 
            OpList.Add("Yes");
            OpList.Add("No");
            OpList.Add("Missing");
            OpList.Add("AND");
            OpList.Add("OR");
            foreach (var item in IdentifierList)
            {
                int number;
                bool isNumeric = int.TryParse(item, out number);
                if (!OpList.Contains(item) && !isNumeric)
                {

                    NewList.Add(item);
                }
            }

            return NewList.ToArray();
        }

        /// <summary>
        /// performs execution of the If...Then...Else command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            if (IfClause.Execute().ToString().ToLower() == "true")
            {
                result = ThenClause.Execute();
            }
            else
            {
                if (ElseClause != null)
                {
                    result = ElseClause.Execute();
                }
            }
            
            return result;
        }

    }
}
