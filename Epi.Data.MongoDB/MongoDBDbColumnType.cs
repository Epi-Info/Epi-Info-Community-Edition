using System;
using System.Collections.Generic;
using System.Text;

namespace Epi.Data.MongoDB
{
    /// <summary>
    /// Enumerates all column types used by MongoDB
    /// </summary>
    public static class MongoDBColumnType
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
