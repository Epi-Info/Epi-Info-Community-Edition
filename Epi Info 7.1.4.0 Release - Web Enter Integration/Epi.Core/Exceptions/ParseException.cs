using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using com.calitha.goldparser;
using System.Security.Permissions;

namespace Epi
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class ParseException : GeneralException
	{
		#region Private Members
		private ParseErrorEventArgs args;
		#endregion Private Members

		#region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ParseException()
            : base(null)
        {
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="arg">Parse Error Event Aruguments</param>
        public ParseException(ParseErrorEventArgs arg)
            : base(SharedStrings.WARNING_COMMAND_SYNTAX_ERROR + "\r\r" + SharedStrings.UNEXPECTED_TOKEN, arg.UnexpectedToken.Text)
		{
			args = arg;			
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        public ParseException(SerializationInfo info, StreamingContext context)
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

        #region Public properties
        /// <summary>
        /// Gets the Terminal parser token
        /// </summary>
		public TerminalToken UnexpectedToken
		{
			get
			{
				return args.UnexpectedToken;
			}
		}
		#endregion Public properties

	}
}
