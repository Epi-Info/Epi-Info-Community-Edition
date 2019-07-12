using System.ComponentModel.DataAnnotations;

namespace Epi.Data.REDCap.Models
{
	/// <summary>
	/// The format that the response object should be when returned from the http request.
	/// Format, 0 = JSON
	/// Format, 1 = CSV
	/// Format, 2 = XML
	/// </summary>
	public enum ReturnFormat
	{
		/// <summary>
		/// Default Javascript Notation
		/// </summary>
		/// 
		[Display(Name = "json")]
		json = 0,
		/// <summary>
		/// Comma Seperated Values
		/// </summary>
		/// 
		[Display(Name = "csv")]
		csv = 1,
		/// <summary>
		/// Extensible Markup Language
		/// </summary>
		/// 
		[Display(Name = "xml")]
		xml = 2,
		/// <summary>
		/// CDISC ODM XML format, specifically ODM version 1.3.1
		/// Only usable on Project Create 
		/// </summary>
		/// 
		[Display(Name = "odm")]
		odm = 3
	}
}
