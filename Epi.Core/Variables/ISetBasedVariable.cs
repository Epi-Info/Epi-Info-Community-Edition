using System;
using System.Collections.Generic;
using System.Text;

namespace Epi
{
    /// <summary>
    /// Interface class for Set-based Variable.
    /// </summary>
    public interface ISetBasedVariable : IVariable
    {
        /// <summary>
        /// Gets/sets a current record value.
        /// </summary>
        // string CurrentRecordValueString { get; set; }
        object CurrentRecordValueObject { get; set; }
    }
}
