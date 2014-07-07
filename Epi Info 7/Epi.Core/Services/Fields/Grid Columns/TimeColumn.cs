using System;
using System.Data;
using Epi.Data;

namespace Epi.Fields
{
	/// <summary>
	/// Date Column
	/// </summary>
	public class TimeColumn : ContiguousColumn
	{

		#region Constructors

		/// <summary>
		/// Constructor for the class
		/// </summary>
		public TimeColumn(GridField grid) : base(grid)
		{
			Construct();
		}

		/// <summary>
		/// Constructor for the class
		/// </summary>
		/// <param name="gridRow">A DataRow containing the grid row's data</param>
        /// <param name="grid">The grid that contains the DataRow</param>
        public TimeColumn(DataRow gridRow, GridField grid)
            : base(gridRow, grid)
		{	
			Construct();
		}

		private void Construct()
		{
			this.GridColumnType = MetaFieldType.Time;
            genericDbColumnType = GenericDbColumnType.Time;
		}

		#endregion Constructors

        #region Public Properties
        ///// <summary>
        ///// NumberColumn data type
        ///// </summary>
        //public override String DataTypes
        //{
        //    get { return SqlDataTypes.DATE_TIME; }
        //}
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