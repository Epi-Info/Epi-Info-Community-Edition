using System;
using System.Collections.Generic;
using System.Text;

namespace Epi.Data.REDCap.Models
{
	/// <summary>
	/// raw [default], label - export the raw coded values or labels for the options of multiple choice fields
	/// </summary>
	public enum RawOrLabel
	{
		/// <summary>
		/// raw coded values
		/// </summary>
		raw = 0,
		/// <summary>
		/// labels for the values
		/// </summary>
		label = 1
	}
}
