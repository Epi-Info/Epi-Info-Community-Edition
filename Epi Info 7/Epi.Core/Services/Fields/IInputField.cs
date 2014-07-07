using System;

namespace Epi.Fields
{

    /// <summary>
    /// Interface class for Input Data Field.
    /// </summary>
    public interface IInputField : IDataField
    {
        /// <summary>
        /// Gets/sets the "is read only" flag.
        /// </summary>
        bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets/sets the "is required" flag.
        /// </summary>
        bool IsRequired { get; set; }

        /// <summary>
        /// Gets/sets a "should repeat last" flag.
        /// </summary>
        bool ShouldRepeatLast { get; set; }

        /// <summary>
        /// Gets an SQL data type.
        /// </summary>
        string GetDbSpecificColumnType();
    }
}