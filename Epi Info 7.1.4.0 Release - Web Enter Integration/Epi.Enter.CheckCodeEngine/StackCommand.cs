using System;
using System.Collections.Generic;
using System.Text;
using EpiInfo.Plugin;

namespace Epi.EnterCheckCodeEngine
{
    public class StackCommand
    {
        string QueryString; // level= & event= & identifier View | record | page | field
        IEnterInterpreter EnterInterpreter;


        public StackCommand(IEnterInterpreter pEnterInterpreter, string pLevel, string pEvent, string pIdentifier)
        {
            this.EnterInterpreter = pEnterInterpreter;
            this.QueryString = string.Format("level={0}&event={1}&identifier={2}", pLevel.ToLower(), pEvent.ToLower(), pIdentifier.ToLower());
        }

        public void Execute()
        {
            ICommand command = this.EnterInterpreter.Context.GetCommand(QueryString);
            if (command != null)
            {
                command.Execute();
            }
        }
    }
}
