using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epi.CommandLine.Config
{
    public interface IVariableControl
    {
        event VariableDialogResultHandler VariableResultSet;
        string Prompt { set; }
    }
}
