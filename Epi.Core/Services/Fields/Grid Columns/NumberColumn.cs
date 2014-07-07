using System;
using System.Data;
using Epi.Data;

namespace Epi.Fields
{
    /// <summary>
    /// Number Column
    /// </summary>
    /// <remarks>Similar to Number Field.</remarks>
    public class NumberColumn : ContiguousColumn
    {

        #region Constructors

        /// <summary>
        /// Constructor for the class
        /// </summary>
        public NumberColumn(GridField grid)
            : base(grid)
        {
            construct();
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="gridRow">A DataRow containing the grid row's data</param>
        /// <param name="grid">The grid that contains the gridrow</param>
        public NumberColumn(DataRow gridRow, GridField grid)
            : base(gridRow, grid)
        {
            construct();
        }

        private void construct()
        {
            this.GridColumnType = MetaFieldType.Number;
            genericDbColumnType = GenericDbColumnType.Double;
        }
        #endregion

        #region Public Properties
        ///// <summary>
        ///// NumberColumn data type
        ///// </summary>
        //public override String DataTypes
        //{
        //    get { return SqlDataTypes.FLOAT; }
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
