using System;
using System.Collections.Generic;
using System.Text;

namespace EpiInfo.Plugin
{
    public interface ICommandPlugin
    {
        string Name { get; }
        ICommandPluginHost Host { get; set; }
        void Parse(string pCommandText);
        void Execute(string pCommandText);
    }
}
