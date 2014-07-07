using System;

namespace Epi
{
	/// <summary>
	/// Defines the interface to get and set a current project as defined 
    /// by the implementing class. 
	/// </summary>
	public interface IProjectHost
	{
        /// <summary>
        /// Current project accessor
        /// </summary>
		Project CurrentProject { get; set; }
	}
}
