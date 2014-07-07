using System;

namespace Epi
{
    /// <summary>
    /// Interface for all classes that have a name
    /// </summary>
    public interface INamedObject
    {
        /// <summary>
        /// Gets the name of the object.
        /// </summary>
        string Name { get;set;}
    }
}
