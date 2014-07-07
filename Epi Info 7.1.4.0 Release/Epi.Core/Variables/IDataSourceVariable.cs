using System;
using System.Collections.Generic;
using System.Text;

namespace Epi
{
    /// <summary>
    /// Interface class for Data Source Set-based Variable
    /// </summary>
    public interface IDataSourceVariable : ISetBasedVariable
    {
        /// <summary>
        /// Gets/sets a table name.
        /// </summary>
        string TableName { get; set; }
    }
}
