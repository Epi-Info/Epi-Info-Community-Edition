using System;
using System.Collections.Generic;
using System.Text;

namespace EpiInfo.Plugin
{
    public enum DataType
    {
        /// <summary>
        /// Unknown data type
        /// </summary>
        /// <remarks>may be assigned any type</remarks>
        Unknown = 9,
        /// <summary>
        /// Object data type (DLL)
        /// </summary>
        Object = 0,
        /// <summary>
        /// Number data type 
        /// </summary>
        /// <remarks>integer or float</remarks>
        Number = 1,
        /// <summary>
        /// Text data type
        /// </summary>
        Text = 2,
        /// <summary>
        /// Date data type 
        /// </summary>
        Date = 3,
        /// <summary>
        /// Time data type
        /// </summary>
        Time = 4,
        /// <summary>
        /// DateTime data type
        /// </summary>
        DateTime = 5,
        /// <summary>
        /// Boolean data type 
        /// </summary>
        /// <remarks>same as yes/no</remarks>
        Boolean = 6,
        /// <summary>
        /// Phone number data type
        /// </summary>
        PhoneNumber = 7,
        /// <summary>
        /// Yes No data type
        /// </summary>
        YesNo = 8,
        /// <summary>
        /// GUID data type
        /// </summary>
        GUID = 10,
        /// <summary>
        /// Class data type
        /// </summary>
        Class = 11,
        /// <summary>
        /// Function data type
        /// </summary>
        Function = 12
    }
}
