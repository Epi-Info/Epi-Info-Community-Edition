using System;
using System.Collections.Generic;
using System.Text;

namespace EpiInfo.Plugin
{
    public interface IApplicationPlugin
    {
        string Name { get;  }
        IApplicationPluginHost Host { get; set; }
        void Start(string[] args);
        void Open();
        void Close();

    }
}
