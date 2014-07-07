using System;
using System.Data;

namespace Epi.Fields
{
    /// <summary>
    /// Contiguous Column
    /// </summary>
    /// <remarks>For Single Value Grid Column Types.</remarks>
	public abstract class ContiguousColumn : PatternableColumn
	{

		#region Private Members

		private string lower = string.Empty;
		private string upper = string.Empty;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for the class
		/// </summary>
		public ContiguousColumn(GridField grid) : base(grid)
		{
		}

		/// <summary>
		/// Constructor for the class
		/// </summary>
        /// <param name="gridRow">A DataRow containing the grid row's data</param>
        /// <param name="grid">The grid that contains the gridrow</param>
        public ContiguousColumn(DataRow gridRow, GridField grid)
            : base(gridRow, grid)
		{
			this.lower = gridRow[ColumnNames.LOWER].ToString();
			this.upper = gridRow[ColumnNames.UPPER].ToString();
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets/sets the lower bounds
		/// </summary>
		public string Lower
		{
			get
			{
				return lower;
			}
			set
			{
				lower = value;
			}
		}

		/// <summary>
		/// Gets/sets the upper bounds
		/// </summary>
		public string Upper
		{
			get
			{
				return upper;
			}
			set
			{
				upper = value;
			}
		}

		#endregion

	}
}
