using System;
using System.Collections.Generic;
using System.Text;

namespace Epi
{
    /// <summary>
    /// Execution Environment Enumeration
    /// </summary>
    public enum ExecutionEnvironment
    {
        /// <summary>
        /// Environment not specified
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Windows (GUI) application
        /// </summary>
        WindowsApplication,
        /// <summary>
        /// ASP.NET or Classic ASP application
        /// using Internet Information Services
        /// </summary>
        InternetInformationServices,
        /// <summary>
        /// Windows (DOS) application
        /// </summary>
        Console,
        /// <summary>
        /// Windows (TSR) service
        /// </summary>
        WindowsService
    }

}
