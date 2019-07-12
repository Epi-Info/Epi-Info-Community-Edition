namespace Epi.Data.REDCap.Models
{
	/// <summary>
	/// The content that the response object should contain from Redcap API.
	/// ReturnContent, 0 = ids
	/// ReturnContent, 1 = count
	/// </summary>
	public enum ReturnContent
	{
		/// <summary>
		/// ids - a list of all record IDs that were imported
		/// </summary>
		ids = 0,
		/// <summary>
		///  count [default] - the number of records imported
		/// </summary>
		count = 1
	}
}
