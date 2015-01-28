using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_GoTo : EnterRule
    {
        string _goto_type = string.Empty;
        string _form = string.Empty;
        string _page = string.Empty;
        string _destination = string.Empty;

        public Rule_GoTo(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            _goto_type = this.GetCommandElement(pToken.Tokens, 0);
            //---2225
          switch (_goto_type.ToUpper())
            {
          case "GOTOFORM":
                    if (pToken.Tokens.Length == 2)
                    {
                        _form = this.GetCommandElement(pToken.Tokens, 1);
                    }
                    break;
            /*    
             case "GOTOPAGE":
                    if (pToken.Tokens.Length == 2)
                    {
                        _destination = this.GetCommandElement(pToken.Tokens, 1);
                        string[] split = _destination.Split(new char[] { '.' });
                        if (split.Length == 2)
                        {
                            _form = split[0];
                            _page = split[1];
                        }
                        else if (split.Length == 1)
                        {
                            _page = split[0];
                        }
                    }
                    else
                    {
                        _destination = this.GetCommandElement(pToken.Tokens, 1) + this.GetCommandElement(pToken.Tokens, 2);
                    }
                    break;
                 */ 
                case "GOTO":
                default:
                    if (pToken.Tokens.Length == 2)
                    {
                        _destination = this.GetCommandElement(pToken.Tokens, 1);
                        string[] split = _destination.Split(new char[] { '.' });
                        if (split.Length == 3)
                        {
                            _form = split[0];
                            _page = split[1];
                            _destination = split[2];
                        }
                        else if (split.Length == 2)
                        {
                            _page = split[0];
                            _destination = split[1];
                        }
                        else
                        {
                            _destination = split[0];
                        }
                    }
                    else
                    {
                        _destination = this.GetCommandElement(pToken.Tokens, 1) + this.GetCommandElement(pToken.Tokens, 2);
                    }
                    break;
            }
        }



        /// <summary>
        /// executes the GOTO command via the EnterCheckCodeInterface.GoTo method
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            this.Context.EnterCheckCodeInterface.GoTo(_destination, _page, _form);
            return null;
        }
    }
}
