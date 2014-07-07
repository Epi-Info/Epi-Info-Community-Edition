using System;
namespace Epi.Data
{
    /// <summary>
    /// Query interface class
    /// </summary>
    public interface IQuery
    {
        /// <summary>
        /// A list of database-independt parameters that can be used with Epi Info data access queries.
        /// </summary>
        System.Collections.Generic.List<QueryParameter> Parameters { get; set; }
        /// <summary>
        /// Gets a SQL Statement
        /// </summary>
        string SqlStatement { get; }
    }
}
