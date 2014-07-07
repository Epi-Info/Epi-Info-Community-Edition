using System;
using System.Collections.Generic;

namespace Epi.ImportExport
{
    public enum ImportExportMessageType
    {
        /// <summary>
        /// Notifications are of interest to the user but won't cause any problems during the upgrade process.
        /// </summary>
        Notification = 0,

        /// <summary>
        /// Warnings may cause parts of the project to be skipped during the upgrade, but still allow the upgrade to proceed.
        /// </summary>
        Warning = 1,

        /// <summary>
        /// Errors are serious enough to force the upgrade to stop or will prevent it from starting if detected beforehand.
        /// </summary>
        Error = 2
    }
}
