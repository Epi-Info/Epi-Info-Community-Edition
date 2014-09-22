using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Reduction to get the current user id from Windows
    /// </summary>
    public partial class Rule_CurrentUser : EnterRule
    {
        public Rule_CurrentUser(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            // UserId
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the current system date.</returns>
        public override object Execute()
        {
            return System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString();
        }
    }
}
