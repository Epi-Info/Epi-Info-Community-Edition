using System;
using System.Data;

namespace Epi.Fields
{
    /// <summary>
    /// Text column
    /// </summary>
    public class DDLColumnOfCommentLegal : DDLColumnOfLegalValues
    {
        #region Private Members
        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for the class
        /// </summary>
        public DDLColumnOfCommentLegal(GridField grid)
            : base(grid)
        {
            this.GridColumnType = MetaFieldType.CommentLegal;
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="gridRow">A DataRow containing the grid row's data</param>
        /// <param name="grid">The GridField the row belongs to</param>
        public DDLColumnOfCommentLegal(DataRow gridRow, GridField grid)
            : base(gridRow, grid)
        {
            this.GridColumnType = MetaFieldType.CommentLegal;
        }

        #endregion

        #region Public Properties
        #endregion
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
