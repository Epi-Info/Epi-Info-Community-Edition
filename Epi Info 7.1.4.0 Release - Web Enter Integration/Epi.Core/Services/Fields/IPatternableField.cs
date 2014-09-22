using System;

namespace Epi.Fields
{
    /// <summary>
    /// Patternable Field interface class
    /// </summary>
	public interface IPatternableField
	{
		#region Properties
        /// <summary>
        /// Gets/sets field pattern
        /// </summary>
		string Pattern
		{
			get;
			set;
		}	
		#endregion Properties		
	}
}
