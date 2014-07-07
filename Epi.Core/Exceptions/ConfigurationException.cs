using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Security.Permissions;
namespace Epi
{
    /// <summary>
    /// Raised when errors detected in the config file.
    /// </summary>
    [Serializable]
    public class ConfigurationException : GeneralException
    {
        #region Private Members
        private ConfigurationIssue issue;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="issue">Configuration Issue</param>
        public ConfigurationException(ConfigurationIssue issue)
            : this(issue, string.Empty)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="issue">Configuration Issue</param>
        /// <param name="message">A message that describes the error.</param>
        public ConfigurationException(ConfigurationIssue issue, string message)
            : this(issue, message, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="issue">Configuration Issue</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the innerException
        /// parameter is not a null reference, the current exception is raised in a catch
        /// block that handles the inner exception.</param>
        public ConfigurationException(ConfigurationIssue issue, Exception innerException)
            : this(issue, innerException == null ? string.Empty : innerException.Message, innerException)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="issue">Configuration Issue</param>
        /// <param name="message">A message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the innerException
        /// parameter is not a null reference, the current exception is raised in a catch
        /// block that handles the inner exception.</param>
        public ConfigurationException(ConfigurationIssue issue, string message, Exception innerException)
            : base(message, innerException)
        {
            this.issue = issue;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">The serialized object data about the exception being thrown.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected ConfigurationException(SerializationInfo info, StreamingContext context)
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
        /// Gets/sets the Configuration Issue.
        /// </summary>
        public ConfigurationIssue Issue
        {
            get { return issue; }
            set { issue = value; }
        }
        #endregion Public Properties
        #region Public Enums
        /// <summary>
        /// Configuration Issue enumeration.
        /// </summary>
        public enum ConfigurationIssue
        {
            /// <summary>
            /// Higher Configuration Version
            /// </summary>
            HigherVersion,
            /// <summary>
            /// Lower Configuration Version
            /// </summary>
            LowerVersion,
            /// <summary>
            /// Invalid Contents Configuration
            /// </summary>
            ContentsInvalid,
            /// <summary>
            /// Access Denied Configuration
            /// </summary>
            AccessDenied
        }
        #endregion Public Enums
    }
}
