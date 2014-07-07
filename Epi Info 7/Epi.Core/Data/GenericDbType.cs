namespace Epi.Data
{
    /// <summary>
    /// An extension of the System.Data.DbType that adds StringLong (Text, Memo) data type that Epi Info commonly uses.
    /// </summary>
    public enum GenericDbColumnType
    {
        /// <summary>
        /// Default value
        /// </summary>
        Unknown,
        ///<summary>AnsiString Type</summary>
        AnsiString,
        ///<summary>AnsiStringFixedLength Type</summary>
        AnsiStringFixedLength,
        ///<summary>Binary Type</summary>
        Binary,
        ///<summary>Boolean Type</summary>
        Boolean,
        ///<summary>Byte Type</summary>
        Byte,
        ///<summary>Currency Type</summary>
        Currency,
        ///<summary>Date Type</summary>
        Date,
        ///<summary>DateTime Type</summary>
        DateTime,
        ///<summary>Decimal Type</summary>
        Decimal,
        ///<summary>Double Type</summary>
        Double,
        ///<summary>Guid Type</summary>
        Guid,
        ///<summary>Int16 (Short) Type</summary>
        Int16,
        ///<summary>Int32 (Integer) Type</summary>
        Int32,
        ///<summary>Int64 (Long) Type</summary>
        Int64,
        ///<summary>Object Type</summary>
        Object,
        ///<summary>SByte Type</summary>
        SByte,
        ///<summary>Single Type</summary>
        Single,
        ///<summary>String Type</summary>
        String,
        ///<summary>StringFixedLength Type</summary>
        StringFixedLength,
        ///<summary>StringLong Type</summary>
        StringLong,
        ///<summary>Time Type</summary>
        Time,
        ///<summary>UInt16 Type</summary>
        UInt16,
        ///<summary>UInt32 Type</summary>
        UInt32,
        ///<summary>UInt64 UInt64Type</summary>
        UInt64,
        ///<summary>VarNumeric Type</summary>
        VarNumeric,
        ///<summary>Xml Type</summary>
        Xml,
        /// <summary>
        /// For storing Images
        /// </summary>
        Image
    }
}