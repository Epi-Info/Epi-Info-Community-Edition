using System;
using System.Data;

namespace Epi.Fields
{
	/// <summary>
	/// Patternable Column.
	/// </summary>
	public abstract class PatternableColumn : GridColumnBase, IPatternable
	{

		#region Private Members

		private string pattern = string.Empty;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for the class
		/// </summary>
		public PatternableColumn(GridField grid) : base(grid)
		{
		}

		/// <summary>
		/// Constructor for the class
		/// </summary>
		/// <param name="gridRow">A DataRow containing the grid row's data</param>
        /// <param name="grid">The grid that contains the gridrow</param>
		public PatternableColumn(DataRow gridRow, GridField grid) : base(gridRow, grid)
		{
			this.pattern = gridRow[ColumnNames.PATTERN].ToString();
		}

		#endregion

		#region IPatternable Members

		/// <summary>
		/// Gets/sets the pattern
		/// </summary>
		public string Pattern
		{
			get
			{
				return pattern;
			}
			set
			{
				pattern = value;
			}
		}

		#endregion

	}
}
