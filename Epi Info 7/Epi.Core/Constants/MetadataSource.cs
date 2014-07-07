using System;
using System.Collections.Generic;
using System.Text;

namespace Epi
{
    /// <summary>
    /// Indicates where metadata is stored.
    /// </summary>
    public enum MetadataSource
    {
        /// <summary>
        /// Xml metadata
        /// </summary>
        Xml = 1,
        /// <summary>
        /// Same database as collected data.
        /// </summary>
        SameDb = 2,
        /// <summary>
        /// Different database than collected data.
        /// </summary>
        DifferentDb = 3,
        /// <summary>
        /// Metadata storage location no specified.
        /// </summary>
        Unknown = 1000
    }
}