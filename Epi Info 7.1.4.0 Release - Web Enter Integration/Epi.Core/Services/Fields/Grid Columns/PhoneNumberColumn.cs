using System;
using System.Data;
using Epi.Data;


namespace Epi.Fields
{
    /// <summary>
    /// Phone Number Column
    /// </summary>
    public class PhoneNumberColumn : PatternableColumn
    {
        #region Constructors
        /// <summary>
        /// Constructor for the Phone Number Column class
        /// </summary>
        public PhoneNumberColumn(GridField grid)
            : base(grid)
        {
            construct();
        }

        /// <summary>
        /// Constructor for the Phone Number Column class
        /// </summary>
        /// <param name="gridRow">A DataRow containing the grid row's data</param>
        /// <param name="grid">The grid that contains the gridrow</param>
        public PhoneNumberColumn(DataRow gridRow, GridField grid)
            : base(gridRow, grid)
        {
            construct();
        }

        private void construct()
        {
            this.GridColumnType = MetaFieldType.PhoneNumber;
            genericDbColumnType = GenericDbColumnType.String;
        }

        #endregion

        #region Public Properties
        ///// <summary>
        ///// PhoneColumn data type
        ///// </summary>
        //public override String DataTypes
        //{
        //    get { return SqlDataTypes.NVARCHAR; }
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
