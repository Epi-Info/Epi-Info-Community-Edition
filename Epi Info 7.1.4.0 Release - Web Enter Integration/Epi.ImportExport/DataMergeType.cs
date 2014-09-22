using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epi.ImportExport
{
    /// <summary>
    /// Enumeration for data merge types.
    /// </summary>
    public enum DataMergeType
    {
        /// <summary>
        /// Records with matching keys will be updated and records with no matching key will be appended.
        /// </summary>
        UpdateAndAppend = 0,

        /// <summary>
        /// Records with matching keys will be updated. Records with no matching key will be ignored.
        /// </summary>
        UpdateOnly = 1,

        /// <summary>
        /// Records with matching keys will be ignored. Records with no matching key will be appended.
        /// </summary>
        AppendOnly = 2
    }
}
