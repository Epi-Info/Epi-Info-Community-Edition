
namespace Epi
{
    /// <summary>
    /// SQL Server data type names
    /// </summary>
    public static class SqlDataTypes
    {
        /// <summary>
        /// An integer data type that can take a value of 1, 0, or null.
        /// The string values TRUE and FALSE can be converted to bit values:
        /// TRUE is converted to 1 and FALSE is converted to 0.
        /// </summary>
        public const string BIT = "bit";

        ///// <summary>
        ///// Defines a data that is combined with a time of day 
        ///// with fractional seconds that is based on a 24-hour clock.
        ///// </summary>
        //public const string DATE_TIME = "datetime";
        /// <summary>
        /// Approximate-number data type for use with floating point numeric data.
        /// Floating point data is approximate; therefore, not all values in the data type range can be represented exactly.
        /// Range: -1.79E+308 to -2.23E-308 and 2.23E-308 to 1.79E+308. Storage: Depends on value of n.
        /// </summary>
        public const string FLOAT = "float";
        /// <summary>
        /// Exact-number data type that uses integer data.
        /// Range: Storage: 1 Byte
        /// </summary>
        public const string INTEGER08 = "tinyint";
        /// <summary>
        /// Exact-number data type that uses integer data.
        /// Range: -2^15 to 2^15-1 Storage: 2 Bytes
        /// </summary>
        public const string INTEGER16 = "smallint";
        /// <summary>
        /// Exact-number data type that uses integer data.
        /// Range: -2^31 to 2^31-1 Storage: 4 Bytes
        /// </summary>
        public const string INTEGER32 = "int";
        /// <summary>
        /// Exact-number data type that uses integer data.
        /// Range: -2^63 to 2^63-1 Storage: 8 Bytes
        /// </summary>
        public const string INTEGER64 = "bigint";
        ///// <summary>
        ///// Fixed-length Unicode character data with a maximum length of 2^30-1 characters.
        ///// Range: 1 to 4,000.
        ///// Storage: Two times the number of characters entered.
        ///// <remarks>The ISO synonym for ntext is national text.</remarks>
        ///// Use nvarchar(max) instead as a future version of Microsoft SQL Server
        ///// will no longer use ntext.
        ///// </summary>
        //public const string NTEXT = "ntext";
        /// <summary>
        /// Variable-length Unicode character data with a maximum length of 2^31-1 bytes..
        /// Range: 0 to 4,000.
        /// Storage: Two times the number of characters entered + 2 bytes.
        /// <remarks>The ISO synonyms for nvarchar are national char varying 
        /// and national character varying.</remarks>
        /// </summary>
        public const string NVARCHAR = "nvarchar";
        /// <summary>
        /// Defines a date that is combined with a time of day.
        /// The time is based on a 24-hour day, with seconds always zero (:00) 
        /// and without fractional seconds.
        /// </summary>
        public const string SMALL_DATE_TIME = "smalldatetime";
        /// <summary>
        /// Exact-number data type that uses integer data.
        /// Range: 0 to 255. Storage: 1 Byte
        /// </summary>
        public const string TINY_INT = "tinyint";
    }
}
