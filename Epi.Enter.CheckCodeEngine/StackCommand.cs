using System;
using System.Collections.Generic;
using System.Text;
using EpiInfo.Plugin;

namespace Epi.EnterCheckCodeEngine
{
    public class StackCommand
    {
        string QueryString; 
        string _level; // View | Record | Page | Field
        string _event; // Before | After | ...
        string _identifier;

        public string Level { get {return _level;} }
        public string Event { get { return _event; } }
        public string Identifier { get { return _identifier; } }
        
        IEnterInterpreter EnterInterpreter;
        
        public StackCommand(IEnterInterpreter EnterInterpreter, string Level, string Event, string Identifier)
        {
            this.EnterInterpreter = EnterInterpreter;
            _level = Level;
            _event = Event;
            _identifier = Identifier;
            this.QueryString = string.Format("level={0}&event={1}&identifier={2}", Level.ToLower(), Event.ToLower(), Identifier.ToLower());
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
