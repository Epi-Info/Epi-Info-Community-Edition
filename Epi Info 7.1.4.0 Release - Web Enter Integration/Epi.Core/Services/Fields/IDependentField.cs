using System;

namespace Epi.Fields
{
    /// <summary>
    /// Dependent Field interface class
    /// </summary>
	public interface IDependentField
	{
		/// <summary>
		/// Source Data Field
        /// Example: A MirrorField has to know what field it is mirroring. 
		/// </summary>
		IDataField SourceField
		{
			get;
		}
	}
}