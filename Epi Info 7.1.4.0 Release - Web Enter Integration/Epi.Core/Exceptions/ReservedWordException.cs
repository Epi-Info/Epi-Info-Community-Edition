using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace Epi
{
    /// <summary>
    /// Exception that is thrown when a reserved word is used to name an entity such as view, field or page.
    /// </summary>
    [Serializable]
    public class ReservedWordException : GeneralException
    {
        #region Fields
        private string reservedWordUsed;
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Constructs a ReservedWordException object with the word used incorrectly.
        /// </summary>
        /// <param name="word"></param>
        public ReservedWordException(string word)
        {
            this.reservedWordUsed = word;
        }
        #endregion Constructors

        #region Public Properties
        /// <summary>
        /// Returns the reserved word that is used incorrectly.
        /// </summary>
        public string ReservedWordUsed
        {
            get { return reservedWordUsed; }
        }
        #endregion Public Properties
    }
}
