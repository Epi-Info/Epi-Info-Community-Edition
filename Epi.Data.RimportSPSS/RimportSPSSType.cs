using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epi.Data.RimportSPSS
{
    //
    // Summary:
    //     Specifies the data type of a field, a property, for use in an System.Data.OleDb.OleDbParameter.
    public enum RimportSPSSType
    {
        //
        // Summary:
        //     No value (DBTYPE_EMPTY).
        Empty = 0,
        //
        // Summary:
        //     A 16-bit signed integer (DBTYPE_I2). This maps to System.Int16.
        SmallInt = 2,
        //
        // Summary:
        //     A 32-bit signed integer (DBTYPE_I4). This maps to System.Int32.
        Integer = 3,
        //
        // Summary:
        //     A floating-point number within the range of -3.40E +38 through 3.40E +38 (DBTYPE_R4).
        //     This maps to System.Single.
        Single = 4,
        //
        // Summary:
        //     A floating-point number within the range of -1.79E +308 through 1.79E +308 (DBTYPE_R8).
        //     This maps to System.Double.
        Double = 5,
        //
        // Summary:
        //     A currency value ranging from -2 63 (or -922,337,203,685,477.5808) to 2 63 -1
        //     (or +922,337,203,685,477.5807) with an accuracy to a ten-thousandth of a currency
        //     unit (DBTYPE_CY). This maps to System.Decimal.
        Currency = 6,
        //
        // Summary:
        //     Date data, stored as a double (DBTYPE_DATE). The whole portion is the number
        //     of days since December 30, 1899, and the fractional portion is a fraction of
        //     a day. This maps to System.DateTime.
        Date = 7,
        //
        // Summary:
        //     A null-terminated character string of Unicode characters (DBTYPE_BSTR). This
        //     maps to System.String.
        BSTR = 8,
        //
        // Summary:
        //     A pointer to an IDispatch interface (DBTYPE_IDISPATCH). This maps to System.Object.
        IDispatch = 9,
        //
        // Summary:
        //     A 32-bit error code (DBTYPE_ERROR). This maps to System.Exception.
        Error = 10,
        //
        // Summary:
        //     A Boolean value (DBTYPE_BOOL). This maps to System.Boolean.
        Boolean = 11,
        //
        // Summary:
        //     A special data type that can contain numeric, string, binary, or date data, and
        //     also the special values Empty and Null (DBTYPE_VARIANT). This type is assumed
        //     if no other is specified. This maps to System.Object.
        Variant = 12,
        //
        // Summary:
        //     A pointer to an IUnknown interface (DBTYPE_UNKNOWN). This maps to System.Object.
        IUnknown = 13,
        //
        // Summary:
        //     A fixed precision and scale numeric value between -10 38 -1 and 10 38 -1 (DBTYPE_DECIMAL).
        //     This maps to System.Decimal.
        Decimal = 14,
        //
        // Summary:
        //     A 8-bit signed integer (DBTYPE_I1). This maps to System.SByte.
        TinyInt = 16,
        //
        // Summary:
        //     A 8-bit unsigned integer (DBTYPE_UI1). This maps to System.Byte.
        UnsignedTinyInt = 17,
        //
        // Summary:
        //     A 16-bit unsigned integer (DBTYPE_UI2). This maps to System.UInt16.
        UnsignedSmallInt = 18,
        //
        // Summary:
        //     A 32-bit unsigned integer (DBTYPE_UI4). This maps to System.UInt32.
        UnsignedInt = 19,
        //
        // Summary:
        //     A 64-bit signed integer (DBTYPE_I8). This maps to System.Int64.
        BigInt = 20,
        //
        // Summary:
        //     A 64-bit unsigned integer (DBTYPE_UI8). This maps to System.UInt64.
        UnsignedBigInt = 21,
        //
        // Summary:
        //     A 64-bit unsigned integer representing the number of 100-nanosecond intervals
        //     since January 1, 1601 (DBTYPE_FILETIME). This maps to System.DateTime.
        Filetime = 64,
        //
        // Summary:
        //     A globally unique identifier (or GUID) (DBTYPE_GUID). This maps to System.Guid.
        Guid = 72,
        //
        // Summary:
        //     A stream of binary data (DBTYPE_BYTES). This maps to an System.Array of type
        //     System.Byte.
        Binary = 128,
        //
        // Summary:
        //     A character string (DBTYPE_STR). This maps to System.String.
        Char = 129,
        //
        // Summary:
        //     A null-terminated stream of Unicode characters (DBTYPE_WSTR). This maps to System.String.
        WChar = 130,
        //
        // Summary:
        //     An exact numeric value with a fixed precision and scale (DBTYPE_NUMERIC). This
        //     maps to System.Decimal.
        Numeric = 131,
        //
        // Summary:
        //     Date data in the format yyyymmdd (DBTYPE_DBDATE). This maps to System.DateTime.
        DBDate = 133,
        //
        // Summary:
        //     Time data in the format hhmmss (DBTYPE_DBTIME). This maps to System.TimeSpan.
        DBTime = 134,
        //
        // Summary:
        //     Data and time data in the format yyyymmddhhmmss (DBTYPE_DBTIMESTAMP). This maps
        //     to System.DateTime.
        DBTimeStamp = 135,
        //
        // Summary:
        //     An automation PROPVARIANT (DBTYPE_PROP_VARIANT). This maps to System.Object.
        PropVariant = 138,
        //
        // Summary:
        //     A variable-length numeric value (System.Data.OleDb.OleDbParameter only). This
        //     maps to System.Decimal.
        VarNumeric = 139,
        //
        // Summary:
        //     A variable-length stream of non-Unicode characters (System.Data.OleDb.OleDbParameter
        //     only). This maps to System.String.
        VarChar = 200,
        //
        // Summary:
        //     A long string value (System.Data.OleDb.OleDbParameter only). This maps to System.String.
        LongVarChar = 201,
        //
        // Summary:
        //     A variable-length, null-terminated stream of Unicode characters (System.Data.OleDb.OleDbParameter
        //     only). This maps to System.String.
        VarWChar = 202,
        //
        // Summary:
        //     A long null-terminated Unicode string value (System.Data.OleDb.OleDbParameter
        //     only). This maps to System.String.
        LongVarWChar = 203,
        //
        // Summary:
        //     A variable-length stream of binary data (System.Data.OleDb.OleDbParameter only).
        //     This maps to an System.Array of type System.Byte.
        VarBinary = 204,
        //
        // Summary:
        //     A long binary value (System.Data.OleDb.OleDbParameter only). This maps to an
        //     System.Array of type System.Byte.
        LongVarBinary = 205
    }
}