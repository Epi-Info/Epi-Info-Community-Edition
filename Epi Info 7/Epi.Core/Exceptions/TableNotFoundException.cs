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
    public class TableNotFoundException : GeneralException
    {
        #region Private Members
        private string tableName = string.Empty;
        #endregion Private Members

        #region Public properties
        /// <summary>
        /// Returns the name of the table that is not found.
        /// </summary>
        public string TableName
        {
            get
            {
                return tableName;
            }
        }

        /// <summary>
        /// Returns a default message. This message is fully translated for display.
        /// </summary>
        public override string Message
        {
            get
            {
                return Util.CombineMessageParts(SharedStrings.TABLE_NOT_FOUND, TableName);
            }
        }

        #endregion Public properties

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public TableNotFoundException():this(string.Empty)
        { 
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="table">Name of table.</param>
        public TableNotFoundException(string table)
        {
            #region Input validation
            if (string.IsNullOrEmpty(table))
            {
                throw new ArgumentNullException("table");
            }
            #endregion Input validation
            this.tableName = table;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        public TableNotFoundException(SerializationInfo info, StreamingContext context)
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