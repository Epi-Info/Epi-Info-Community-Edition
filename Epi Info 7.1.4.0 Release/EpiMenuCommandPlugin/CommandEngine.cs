using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpiMenu.CommandPlugin
{
    public class CommandEngine : EpiInfo.Plugin.ICommandPlugin 
    {

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public EpiInfo.Plugin.ICommandPluginHost Host
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Parse(string pCommandText)
        {
            throw new NotImplementedException();
        }

        public void Execute(string pCommandText)
        {
            throw new NotImplementedException();
        }


    }
}
