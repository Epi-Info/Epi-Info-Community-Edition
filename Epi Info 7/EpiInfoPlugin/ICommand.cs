using System;
using System.Collections.Generic;
using System.Text;

namespace EpiInfo.Plugin
{
    public interface ICommand
    {
        object Execute();
        string ToString();
        bool IsNull();
    }
}
