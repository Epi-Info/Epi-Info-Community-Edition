using System;
using System.Data;
using Epi.Data;

namespace Epi.Fields
{
	/// <summary>
	/// Date Column
	/// </summary>
	public class DateTimeColumn : ContiguousColumn
	{

		#region Constructors

		/// <summary>
		/// Constructor for the class
		/// </summary>
		public DateTimeColumn(GridField grid) : base(grid)
		{
			Construct();
		}

		/// <summary>
		/// Constructor for the class
		/// </summary>
		/// <param name="gridRow">A DataRow containing the grid row's data</param>
        /// <param name="grid">The grid that contains the DataRow</param>
        public DateTimeColumn(DataRow gridRow, GridField grid)
            : base(gridRow, grid)
		{	
			Construct();
		}

		private void Construct()
		{
			this.GridColumnType = MetaFieldType.DateTime;
            genericDbColumnType = GenericDbColumnType.DateTime;
		}

		#endregion Constructors

        #region Public Properties

        #endregion Public Properties

        #region protected methods
        /// <summary>
        /// Inserts the grid column to the database
        /// </summary>
        protected override void InsertColumn()
        {
            Id = GetMetadata().CreateGridColumn(this);
            base.OnColumnInserted();
        }

        /// <summary>
        /// Deletes the grid column from the database.
        /// </summary>
        protected override void DeleteColumn()
        {
            GetMetadata().DeleteGridColumn(this);
        }

        /// <summary>
        /// Update the grid column to the database
        /// </summary>
        protected override void UpdateColumn()
        {
            GetMetadata().UpdateGridColumn(this);
        }
        
        #endregion
	}
}