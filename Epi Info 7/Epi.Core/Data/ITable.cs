using System;
using System.Collections;
using Epi;
using System.Collections.Generic;

namespace Epi.Data
{
    /// <summary>
    /// Representation of an IDbDriver table
    /// </summary>
    public interface ITable
    {
        /// <summary>
        /// Gets database driver
        /// </summary>
        IDbDriver Database
        {
            get;

        }

        /// <summary>
        /// Gets name of table
        /// </summary>
        string TableName
        {
            get;

        }

        /// <summary>
        /// Gets list of column names of table.
        /// </summary>
        List<string> TableColumnNames
        {
            get;
        }
    }
}
