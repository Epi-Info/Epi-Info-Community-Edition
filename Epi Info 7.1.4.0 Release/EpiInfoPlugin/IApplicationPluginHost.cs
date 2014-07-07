using System;
using System.Collections.Generic;
using System.Text;

namespace EpiInfo.Plugin
{
    public interface IApplicationPluginHost
    {
        bool Register(IApplicationPlugin applicationPlugin);
    }
}
