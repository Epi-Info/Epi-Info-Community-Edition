using System;
using System.Collections.Generic;
using System.Text;

namespace Epi.Data.MySQL
{
    /// <summary>
    /// Enumerates all column types used by MySQL
    /// </summary>
    public static class MySQLDbColumnType
    {
        /// <summary>
        /// TEXT
        /// </summary>
        public static string Text = @"text";
        /// <summary>
        /// Long Binary Large Object
        /// </summary>
        public static string Longblob = @"longblob";
    }
}
