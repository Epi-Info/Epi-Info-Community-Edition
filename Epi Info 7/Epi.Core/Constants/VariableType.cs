
namespace Epi
{
    /// <summary>
    /// Scope of variables defined.
    /// Caution: This is a flags enumeration. All enum values should be in powers of two.
    /// </summary>
    [System.Flags]
    public enum VariableType
    {
        
        /// <summary>
        /// Variable undefined
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Permanent Variable
        /// </summary>
        Permanent = 1,

        /// <summary>
        /// Global Variable
        /// </summary>
        Global = 2,

        /// <summary>
        /// Standard Variables
        /// </summary>
        Standard = 4,

        /// <summary>
        /// System variables
        /// </summary>
        System = 8,

        /// <summary>
        /// Fields of a view
        /// </summary>
        DataSource = 16,

        /// <summary>
        /// after assign View variable, it becomes Redefined variable 
        /// </summary>
        DataSourceRedefined = 32
    }
}