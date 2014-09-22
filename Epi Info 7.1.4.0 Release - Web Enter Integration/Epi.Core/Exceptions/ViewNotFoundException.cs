using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
namespace Epi
{
    /// <summary>
    /// Table Not Found Exception
    /// </summary>
    [Serializable]
    public class ViewNotFoundException : GeneralException
    {
        #region Private Members
        private string viewName = string.Empty;
        #endregion Private Members

        #region Public properties
        /// <summary>
        /// Returns the name of the table that is not found.
        /// </summary>
        public string ViewName
        {
            get
            {
                return viewName;
            }
        }

        /// <summary>
        /// Returns a default message. This message is fully translated for display.
        /// </summary>
        public override string Message
        {
            get
            {
                return Util.CombineMessageParts(SharedStrings.VIEW_NOT_FOUND, ViewName);
            }
        }

        #endregion Public properties

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ViewNotFoundException():this(string.Empty)
        { 
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="table">Name of table.</param>
        public ViewNotFoundException(string view)
        {
            #region Input validation
            if (string.IsNullOrEmpty(view))
            {
                throw new ArgumentNullException("view");
            }
            #endregion Input validation
            this.viewName = view;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        public ViewNotFoundException(SerializationInfo info, StreamingContext context)
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
    }
}