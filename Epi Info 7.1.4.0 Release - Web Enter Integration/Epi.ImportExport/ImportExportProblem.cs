using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epi.ImportExport
{
    /// <summary>
    /// Simple data type used to represent a single problem encountered during an import/export operation, or a preprocessing operation such as a validation check.
    /// </summary>
    public struct ImportExportMessage
    {
        /// <summary>
        /// The ID of the problem. Typically optional.
        /// </summary>
        public int ID;

        /// <summary>
        /// What the type of problem is.
        /// </summary>
        public ImportExportMessageType MessageType;

        /// <summary>
        /// The code associated with the problem.
        /// </summary>
        public string Code;

        /// <summary>
        /// The short message describing the problem.
        /// </summary>
        public string Message;

        /// <summary>
        /// An optional longer description of the problem.
        /// </summary>
        public string Description;
    }
}
