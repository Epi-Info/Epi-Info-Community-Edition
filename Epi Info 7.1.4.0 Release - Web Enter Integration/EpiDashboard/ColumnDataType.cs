using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpiDashboard
{
    /// <summary>
    /// Column data types
    /// Caution: This is a flags enumeration. All enum values should be in powers of two.
    /// </summary>
    [System.Flags]
    public enum ColumnDataType
    {

        /// <summary>
        /// Text columns
        /// </summary>
        Text = 1,

        /// <summary>
        /// Numeric columns
        /// </summary>
        Numeric = 2,

        /// <summary>
        /// Boolean columns
        /// </summary>
        Boolean = 4,

        /// <summary>
        /// DateTime columns
        /// </summary>
        DateTime = 8,

        /// <summary>
        /// User-defined columns
        /// </summary>
        UserDefined = 16,
    }
}
