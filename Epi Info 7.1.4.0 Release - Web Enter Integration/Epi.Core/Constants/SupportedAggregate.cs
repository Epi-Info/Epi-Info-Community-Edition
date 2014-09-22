
namespace Epi
{
    /// <summary>
    /// List of supported aggregates
    /// </summary>
    public enum SupportedAggregate
    {
        /// <summary>
        /// The arithmetic mean of a set of values contained in a specified field on a query
        /// </summary>
        Avg = 1,
        /// <summary>
        /// The number of records returned by a query
        /// </summary>
        Count = 2,
        /// <summary>
        /// The first record in the result set
        /// </summary>
        First = 3,
        /// <summary>
        /// The last record in the result set
        /// </summary>
        Last = 4,
        /// <summary>
        /// The maximum of a set of values contained in a specified field on a query
        /// </summary>
        Max = 5,
        /// <summary>
        /// The minimum of a set of values contained in a specified field on a query
        /// </summary>
        Min = 6,
        /// <summary>
        /// The standard deviation of a set of values contained in a specified field on a query
        /// </summary>
        StDev = 7,
        /// <summary>
        /// The standard deviation for a population represented as a set of values contained in a specified field on a query
        /// </summary>
        StDevP = 8,
        /// <summary>
        /// The sum of a set of values contained in a specified field on a query
        /// </summary>
        Sum = 9,
        /// <summary>
        /// The variance of a set of values contained in a specified field on a query
        /// </summary>
        Var = 10,
        /// <summary>
        /// The variance for a population represented as a set of values contained in a specified field on a query
        /// </summary>
        VarP = 11
    }
}
