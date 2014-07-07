using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Epi;
using System.Security.Permissions;

namespace Epi
{
    /// <summary>
    /// Thrown when the current project is invalid
    /// </summary>
    [Serializable]
    public class CurrentProjectInvalidException : GeneralException
    {
        #region Private Members
        private string fileName;
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public CurrentProjectInvalidException()
            : base(string.Empty)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">The project filename.</param>
        public CurrentProjectInvalidException(string fileName)
        {
            #region Parameter Validation
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }
            #endregion Parameter Validation

            this.fileName = fileName;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        public CurrentProjectInvalidException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion Constructors

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

        #region Public Properties
        /// <summary>
        /// Gets the project filename.
        /// </summary>
        public string FileName
        {
            get
            {
                return fileName;
            }
        }

        /// <summary>
        /// Gets the message that describes the error.
        /// </summary>
        public override string Message
        {
            get
            {
                return SharedStrings.INVALID_CURRENT_PROJECT + ": \n" + fileName;
            }
        }
        #endregion "Public Properties"
    }
}
