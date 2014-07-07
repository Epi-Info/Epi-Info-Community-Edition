using System;

namespace Epi.Fields
{
    /// <summary>
    /// Patternable interface class
    /// </summary>
	public interface IPatternable
	{
		#region Properties
        /// <summary>
        /// Gets/sets field pattern.
        /// </summary>
		string Pattern
		{
			get;
			set;
		}	
		#endregion Properties		
	}
}