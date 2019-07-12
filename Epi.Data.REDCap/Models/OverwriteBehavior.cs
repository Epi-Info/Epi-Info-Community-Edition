namespace Epi.Data.REDCap.Models
{
	/// <summary>
	/// normal - blank/empty values will be ignored [default]
	/// overwrite - blank/empty values are valid and will overwrite data
	/// </summary>
	public enum OverwriteBehavior
	{
		/// <summary>
		/// blank/empty values will be ignored [default]
		/// </summary>
		normal = 0,
		/// <summary>
		/// blank/empty values are valid and will overwrite data
		/// </summary>
		overwrite = 1
	}
}
