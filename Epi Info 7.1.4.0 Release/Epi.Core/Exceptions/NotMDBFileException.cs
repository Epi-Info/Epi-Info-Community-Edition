using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Text;
using System.Security.Permissions;

namespace Epi
{
    /// <summary>
    /// Not an MDB File Exception class
    /// </summary>
    [Serializable]
    public class NotMDBFileException : GeneralException
    {
        #region Private Members
        private string fileName = string.Empty;
        #endregion Private Members

        #region Public properties

        /// <summary>
        /// Returns a default message. This message is fully translated for display.
        /// </summary>
        public override string Message
        {
            get
            {
                return Util.CombineMessageParts(SharedStrings.INVALID_MDB_FILE, this.fileName);
            }
        }
        #endregion Public properties

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public NotMDBFileException()
            : this(string.Empty)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mdbFile">Name of Microsoft Access file</param>
        public NotMDBFileException(string mdbFile)
        {
            #region Input validation
            if (string.IsNullOrEmpty(mdbFile))
            {
                throw new ArgumentNullException("table");
            }
            #endregion Input validation
            this.fileName = mdbFile;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        public NotMDBFileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Sets the SerializationInfo with information about the exception. 
        /// </summary>
        /// <param name="info">The serialized object data about the exception being thrown.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
        #endregion Public Methods
    }
}
