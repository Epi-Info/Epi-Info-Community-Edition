using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epi.Core.EnterInterpreter
{
    public enum DLLClassEnum
    {
        COM,
        NET
    }

    public interface IDLLClass
    {

        string Identifier { get; }
        string Class { get; }
        DLLClassEnum Type { get; }
        object Execute(string pMethod, object[] pArgumentList);
    }
}
