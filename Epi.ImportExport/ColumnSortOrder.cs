using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epi.ImportExport
{
    /// <summary>
    /// For exporting purposes, specifies how the columns will be sorted.
    /// </summary>
    public enum ColumnSortOrder
    {
        /// <summary>
        /// No sorting. Output columns in the order they are in the list of columns to be exported.
        /// </summary>
        None = 0,

        /// <summary>
        /// Sort the columns alphabetically.
        /// </summary>
        Alphabetical = 1,

        /// <summary>
        /// If possible, sort by the form's tab order. This should fallback to None (0) if the data source isn't an Epi Info 7 project for any reason.
        /// </summary>
        TabOrder = 2
    }
}
