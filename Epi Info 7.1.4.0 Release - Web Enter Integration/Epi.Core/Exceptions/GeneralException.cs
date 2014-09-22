using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Epi
{
    /// <summary>
    /// General Exception class
    /// </summary>
    [Serializable]
    public class GeneralException : System.Exception
    {
        #region Private Members
        private string localizablePart;
        private string nonLocalizablePart;
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public GeneralException()
            : this(string.Empty, string.Empty, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizableMessage">A message that describes the error.</param>
        public GeneralException(string localizableMessage)
            : this(localizableMessage, string.Empty, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizableMessage">A message that describes the error.</param>
        /// <param name="nonLocalizablePart"></param>
        public GeneralException(string localizableMessage, string nonLocalizablePart)
            : this(localizableMessage, nonLocalizablePart, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizableMessage">A message that describes the error.</param>
        /// <param name="innerException"></param>
        public GeneralException(string localizableMessage, Exception innerException)
            : this(localizableMessage, string.Empty, innerException)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        public GeneralException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizableMessage">A message that describes the error.</param>
        /// <param name="nonLocalizablePart">Non localizable exception message part.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the innerException
        /// parameter is not a null reference, the current exception is raised in a catch
        /// block that handles the inner exception.</param>
        public GeneralException(string localizableMessage, string nonLocalizablePart, Exception innerException)
            : base(string.Empty, innerException)
        {
            this.localizablePart = localizableMessage;
            this.nonLocalizablePart = nonLocalizablePart;
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

        #region Public properties
        /// <summary>
        /// Gets the message that describes the error.
        /// </summary>
        /// <returns></returns>
        public override string Message
        {
            get
            {
                const string separator = ": \n";
                if (string.IsNullOrEmpty(nonLocalizablePart))
                {
                    return localizablePart;
                }
                else
                {
                    return localizablePart + separator + nonLocalizablePart;
                }
            }
        }
        #endregion Public properties
    }
}


