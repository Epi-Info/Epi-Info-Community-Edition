using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Epi
{
    /// <summary>
    /// Variable Exists Exception
    /// </summary>
    [Serializable]
    public class VariableExistsException : GeneralException
    {
        #region Private Members
        private IVariable variable = null;
        #endregion Private Members

        #region Public properties
        /// <summary>
        /// Returns the Variable that already exists
        /// </summary>
        public IVariable Variable
        {
            get
            {
                return variable;
            }
        }

        /// <summary>
        /// Returns a default message. This message is fully translated for display.
        /// </summary>
        public override string Message
        {
            get
            {
                return Util.CombineMessageParts(SharedStrings.DUPLICATE_VARIABLE_DEFINITION, variable.Name);
            }
        }

        #endregion Public properties

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public VariableExistsException()
            : this(null)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public VariableExistsException(IVariable var)
        {
            this.variable = var;
        }
         
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        public VariableExistsException(SerializationInfo info, StreamingContext context)
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