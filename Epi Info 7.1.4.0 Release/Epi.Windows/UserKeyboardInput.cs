#region Namespaces

using System;
using System.Collections.Generic;
using System.Text;

#endregion //Namespaces

namespace Epi.Windows
{
    /// <summary>
    /// Class containing common methods for user keyboard input
    /// </summary>
    public static class UserKeyboardInput
    {
        /// <summary>
        /// Determines whether the specified key is a regular input key that the control wants and sends it directly to the control
        /// </summary>
        /// <param name="keyData">The key's value</param>
        /// <returns>Boolean indicating if key is a regular input key</returns>
        public static bool IsInputKey(System.Windows.Forms.Keys keyData)
        {
            return (keyData == System.Windows.Forms.Keys.Tab);
        }
    }
}
