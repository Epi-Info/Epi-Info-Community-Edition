#region Namespaces
using System;
#endregion //Namespaces

namespace Epi.Windows
{
    /// <summary>
    /// Interface for any dialog that can be used in a wizard
    /// </summary>
    public interface IWizardStep
    {
        /// <summary>
        /// Declaration of the Back Requested event handler
        /// </summary>
        event EventHandler BackRequested;
    }
}