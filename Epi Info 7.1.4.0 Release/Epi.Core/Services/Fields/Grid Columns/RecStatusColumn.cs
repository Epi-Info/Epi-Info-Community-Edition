
using System;
using System.Data;
using System.Drawing;

using Epi.Data;
using Epi.Data.Services;

namespace Epi.Fields
{
	/// <summary>
	/// RecStatus Grid Column
	/// </summary>
	public class RecStatusColumn : PredefinedColumn, IMirrorable
	{
		#region Private Members
		private string value = string.Empty;
        private string tableName = string.Empty;
		#endregion Private Members

		#region Constructors

		/// <summary>
		/// Instantiates a grid field
		/// </summary>
		/// <param name="grid">Grid field object.</param>
        public RecStatusColumn(GridField grid)
            : base(grid)
		{
            construct();
		}
        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="gridRow">A DataRow containing the grid row's data</param>
        /// <param name="grid">The GridField the row belongs to</param>
        public RecStatusColumn(DataRow gridRow, GridField grid)
            : base(gridRow, grid)
        {
            construct();
        }

        private void construct()
        {
            this.Name = ColumnNames.REC_STATUS;
            this.GridColumnType = MetaFieldType.RecStatus;
            genericDbColumnType = GenericDbColumnType.SByte;
        }
		
		#endregion Constructors

        #region Public Properties
        /// <summary>
        /// Returns the prompt for UI.
        /// </summary>
        public override string PromptText
        {
            get
            {
                return SharedStrings.REC_STATUS;
            }
            set
            {
                throw new GeneralException("PromptText for Rec Status is pre-defined.");
            }
        }

        ///// <summary>
        ///// RecStatusColumn data type
        ///// </summary>
        //public override String DataTypes
        //{
        //    get { return SqlDataTypes.INTEGER16; }
        //}
        #endregion Public Properties

		#region protected methods

        /// <summary>
        /// Inserts the grid column to the database
        /// </summary>
        protected override void InsertColumn()
        {
            if (this.Id == 0)
            {
                Id = GetMetadata().CreateGridColumn(this);
                base.OnColumnInserted();
            }
            else
            {
                throw new System.ApplicationException("Rec status field already exists");
            }
        }

        /// <summary>
        /// Deletes the grid column from the database.
        /// </summary>
        protected override void DeleteColumn()
        {
            GetMetadata().DeleteGridColumn(this);
        }

        /// <summary>
        /// Non implemented member to update of the Rec Status column.
        /// </summary>
        protected override void UpdateColumn()
        {
            return;
        }

		#endregion

        #region
        /// <summary>
        /// Returns the string value that is reflected my a mirror field.
        /// </summary>
        /// <returns>reflected value string</returns>
        public virtual string GetReflectedValue()
        {
            return this.CurrentRecordValueString;
        }
        #endregion

	}
}
