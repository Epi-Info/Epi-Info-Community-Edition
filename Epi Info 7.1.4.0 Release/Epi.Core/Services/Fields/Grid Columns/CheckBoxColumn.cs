using System;
using System.Data;
using Epi.Data;

namespace Epi.Fields
{
    /// <summary>
    /// Text column
    /// </summary>
    public class CheckboxColumn : GridColumnBase
    {
        #region Private Members
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor for the class
        /// </summary>
        public CheckboxColumn(GridField grid)
            : base(grid)
        {
            construct();
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="gridRow">A DataRow containing the grid row's data</param>
        /// <param name="grid">The GridField the row belongs to</param>
        public CheckboxColumn(DataRow gridRow, GridField grid)
            : base(gridRow, grid)
        {
            construct();
        }

        private void construct()
        {
            this.GridColumnType = MetaFieldType.Checkbox;
            genericDbColumnType = GenericDbColumnType.Boolean;
        }

        #endregion

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
