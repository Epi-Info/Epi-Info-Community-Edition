
namespace Epi
{
    /// <summary>
    /// Category for record processing scope
    /// </summary>
    public enum RecordProcessingScope
    {
        /// <summary>
        /// Process all records
        /// </summary>
        Undeleted = 1,
        /// <summary>
        /// Process undeleted records only
        /// </summary>
        Deleted = 2,
        /// <summary>
        /// Process deleted records only
        /// </summary>
        Both = 3
    }
}
