using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Security.Permissions;

namespace Epi.WPF.Dashboard.Rules
{
    public class DefectiveRuleException : System.Exception
    {
        #region Private Members
        private RuleIssue issue;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="issue">Configuration Issue</param>
        public DefectiveRuleException(RuleIssue issue)
            : this(issue, string.Empty)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="issue">Configuration Issue</param>
        /// <param name="message">A message that describes the error.</param>
        public DefectiveRuleException(RuleIssue issue, string message)
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
        public DefectiveRuleException(RuleIssue issue, Exception innerException)
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
        public DefectiveRuleException(RuleIssue issue, string message, Exception innerException)
            : base(message, innerException)
        {
            this.issue = issue;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">The serialized object data about the exception being thrown.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected DefectiveRuleException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion Constructors

        #region Public Methods
        #endregion Public Methods

        #region Public Properties
        /// <summary>
        /// Gets/sets the Configuration Issue.
        /// </summary>
        public RuleIssue Issue
        {
            get { return issue; }
            set { issue = value; }
        }
        #endregion Public Properties

        #region Public Enums
        /// <summary>
        /// Configuration Issue enumeration.
        /// </summary>
        public enum RuleIssue
        {
            /// <summary>
            /// A substring rule is invalid due to an invalid start position, e.g. starting at a negative number
            /// </summary>
            SubstringInvalidStartPosition
        }
        #endregion Public Enums
    }
}
