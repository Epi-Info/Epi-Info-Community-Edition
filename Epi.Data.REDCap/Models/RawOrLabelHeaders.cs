using System;
using System.Collections.Generic;
using System.Text;

namespace Epi.Data.REDCap.Models
{
	/// <summary>
	/// raw [default], label - (for 'csv' format 'flat' type only) for the CSV headers, export the variable/field names (raw) or the field labels (label)
	/// </summary>
	public enum RawOrLabelHeaders
	{
		/// <summary>
		/// no labels
		/// </summary>
		raw = 0,
		/// <summary>
		/// label - (for 'csv' format 'flat' type only) for the CSV headers
		/// </summary>
		label = 1
	}
}
