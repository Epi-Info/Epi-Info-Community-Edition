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
           
            if (pContext.IsVariableValidationEnable)
            {
                var IdentifierList = this.GetCommandElement(pToken.Tokens, 1).ToString().Split(' ');

                IdentifierList = RemoveOp(IdentifierList);
                List<string> NewList = new List<string>();
                if (IdentifierList.Length > 0)
                {//if 1
                    try
                    {
                        for (int i = 0; IdentifierList.Length > i ; i++)
                        { // for 2
                            int IndexStart = 0;
                            int IndexEnd = 0;
                            bool StartsAndEnds = false;
                            if (IdentifierList[i].StartsWith("\"") && IdentifierList[i].EndsWith("\"")) {
                                NewList.Add(IdentifierList[i]);
                                StartsAndEnds = true; 
                            }
                            if (!StartsAndEnds)
                            {
                                if (IdentifierList[i].Contains("\""))
                                {// if 2
                                    IndexStart = i;
                                    for (int j = i + 1; IdentifierList.Length > j; j++)
                                    {
                                        if (IdentifierList[j].Contains("\""))
                                        {
                                            IndexEnd = j;
                                            break;
                                        }

                                    }
                                    

                                    i = IndexEnd;

                                }//if 2
                                if (IndexEnd > 0)
                                {
                                    for (int k = IndexStart; IdentifierList.Length > k; k++)
                                    {
                                        if (IndexEnd >= k)
                                        {
                                            NewList.Add(IdentifierList[k]);
                                        }
                                    }

                                }
                            }
                        }// for 2
                        foreach (var item in IdentifierList)
                        {
                            if (IdentifierList.Length > 0)
                            {
                                if (!string.IsNullOrEmpty(item) && !this.Context.CommandVariableCheck.ContainsKey(item.ToLower()) && !NewList.Contains(item))
                                {
                                    this.Context.CommandVariableCheck.Add(item, "If");
                                }
                            }
                        }
                    }
                    catch(Exception ex){
                        throw ex;
                    }
                }//if 1
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
