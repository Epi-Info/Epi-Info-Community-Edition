using System;
using System.Data;
using Epi.Data;

namespace Epi.Fields
{
    /// <summary>
    /// Text column
    /// </summary>
    public class TextColumn : GridColumnBase
    {
        #region Private Members
        private int size;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor for the class
        /// </summary>
        public TextColumn(GridField grid)
            : base(grid)
        {
            construct();
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="gridRow">A DataRow containing the grid row's data</param>
        /// <param name="grid">The GridField the row belongs to</param>
        public TextColumn(DataRow gridRow, GridField grid)
            : base(gridRow, grid)
        {
            construct();
            this.size = (short)gridRow["Size"];
        }

        private void construct()
        {
            this.GridColumnType = MetaFieldType.Text;
            genericDbColumnType = GenericDbColumnType.String;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets/sets the size
        /// </summary>
        public int Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
            }
        }
        
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
