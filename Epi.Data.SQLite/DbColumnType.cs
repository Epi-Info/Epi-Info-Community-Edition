using System;
using System.Collections.Generic;
using System.Text;

namespace Epi.Data.SQLite
{
    internal class SQLiteColumnType : Epi.Data.DbColumnType
    {
        internal static string Currency = @"currency";
        internal static string OLEObject = @"OLEObject";
        internal static string Text = @"TEXT";
        internal static string YesNo = @"yesno";
    }
    internal class AccessColumnType : Epi.Data.DbColumnType
    {
        internal static string Currency = @"currency";
        internal static string OLEObject = @"OLEObject";
        internal static string Text = @"text";
        internal static string YesNo = @"yesno";
    }
}
