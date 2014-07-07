#region Namespaces

using System;

#endregion //Namespaces

namespace Epi.Windows
{
    /// <summary>
    /// Interface that defines methods and properties of all windows modules
    /// </summary>
    public interface IWindowsModule : Epi.IModule
    {

        /// <summary>
        /// Event fired when the main form is closed
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event arguments</param>
        void MainForm_Closed(object sender, EventArgs e);

        /// <summary>
        /// Event fired when the main form is disposed of
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void MainForm_Disposed(object sender, EventArgs e);
     }
}