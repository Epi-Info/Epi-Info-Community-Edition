#region Namespaces

using System;
using System.IO;
#endregion //Namespaces

namespace Epi
{
    /// <summary>
    /// Files class 
    /// </summary>
    public static class Files
    {
        #region Public Properties

        /// <summary>
        /// Returns the last program
        /// </summary>
		public static string LastPgm
        {
            get
            {
                return Path.Combine(Configuration.GetNewInstance().Directories.Working, "LASTPGM.Pgm");
            }
        }
        
        /// <summary>
        /// Returns the file path of Epi Info's menu
        /// </summary>
		public static string MnuFilePath
        {
            get
            {
                return Path.Combine(Configuration.GetNewInstance().Directories.Configuration, "EpiInfo.mnu");
            }
        }

        #endregion  //Public Properties

    }
}