using System;
using System.Collections.Generic;
using System.Text;

namespace Epi
{
    /// <summary>
    /// Interface class for Scalar Variable
    /// </summary>
    public interface IScalarVariable : IVariable
    {
        /// <summary>
        /// Scalar Variable value.
        /// </summary>
        string Value { get;}
    }
}
