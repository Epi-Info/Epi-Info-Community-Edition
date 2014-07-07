using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;using com.calitha.goldparser;

namespace Epi
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class TokenException : GeneralException
	{
        /// <summary>
        /// Default Constructor
        /// </summary>
        public TokenException()
            : this(null)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="args"></param>
		public TokenException(TokenErrorEventArgs args)
		{
            this.tokenList = args.Token.Text;
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        public TokenException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private string tokenList = string.Empty;

        /// <summary>
        /// Message
        /// </summary>
        public override string Message
        {
            get
            {
                return String.Format(SharedStrings.TOKEN_EXCEPTION, tokenList);
            }
        }
 	}
}
