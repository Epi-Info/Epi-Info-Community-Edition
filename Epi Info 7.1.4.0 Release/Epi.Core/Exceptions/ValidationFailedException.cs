using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Epi
{
    /// <summary>
    /// Exception thrown when validation fails
    /// </summary>
    [Serializable]
    public class ValidationFailedException : GeneralException
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ValidationFailedException()
        {
        }
    }
}