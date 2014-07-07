using System;
using System.Collections.Generic;
using System.Text;

namespace EpiInfo.Plugin
{
    public interface ICommandPluginHost
    {
        bool Register(ICommandPlugin commandPlugin);
    }
}
