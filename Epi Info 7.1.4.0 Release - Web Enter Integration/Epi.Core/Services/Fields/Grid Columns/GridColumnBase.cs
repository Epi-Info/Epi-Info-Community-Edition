using System;
using System.Data;
using Epi.Data.Services;
using Epi.Data;

namespace Epi.Fields
{
    /// <summary>
    /// Grid column
    /// </summary>
    public abstract class GridColumnBase : INamedObject
    {
        #region Fields Members
        private int id;
        private string name = string.Empty;
        private string text = string.Empty;
        private bool shouldRepeatLast;
        private bool isRequired;
        private bool isReadOnly;
        private bool isUniqueField;
        private int position;
        private int width;
        private MetaFieldType gridColumnType;
        private GridField grid;
        #endregion Fields

        #region Protected Data
        /// <summary>
        /// Generic db column type used to translated to specific column type for the DB engine.
        /// </summary>
        protected GenericDbColumnType genericDbColumnType;
        #endregion Protected Data

        #region Constructors

        /// <summary>
        /// Private constructor - not used
        /// </summary>
        private GridColumnBase()
        {
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        public GridColumnBase(GridField grid)
        {
            this.grid = grid;
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="gridRow">A DataRow containing the grid row's data</param>
        /// <param name="grid">The GridField that the row belongs to</param>
        public GridColumnBase(DataRow gridRow, GridField grid)
        {
            this.id = (int)gridRow[ColumnNames.GRID_COLUMN_ID];
            this.name = gridRow[ColumnNames.NAME].ToString();
            this.text = gridRow[ColumnNames.TEXT].ToString();
            this.shouldRepeatLast = (bool)gridRow[ColumnNames.SHOULD_REPEAT_LAST];
            this.isRequired = (bool)gridRow[ColumnNames.IS_REQUIRED];
            this.isReadOnly = (bool)gridRow[ColumnNames.IS_READ_ONLY];

            if (gridRow.Table.Columns.Contains(ColumnNames.IS_UNIQUE_FIELD) && gridRow[ColumnNames.IS_UNIQUE_FIELD] is bool)
            { 
                this.isUniqueField = (bool)gridRow[ColumnNames.IS_UNIQUE_FIELD];
            }
            else
            {
                this.isUniqueField = false;
            }
            
            this.position = (short)gridRow[ColumnNames.POSITION];
            this.width = int.Parse(gridRow["Width"].ToString());
            this.grid = grid;
        }

        #endregion

        #region Public Properties
        ///// <summary>
        ///// Gets the ansi-92 SQL data type of the grid column.
        ///// </summary>
        //public abstract String DataTypes
        //{
        //    get;
        //}
        /// <summary>
        /// Gets/sets the parent grid
        /// </summary>
        public GridField Grid
        {
            get
            {
                return this.grid;
            }
            set
            {
                this.grid = value;
            }
        }

        /// <summary>
        /// Gets/sets the Id
        /// </summary>
        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        /// <summary>
        /// Gets/sets the name
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// Gets/sets the display text
        /// </summary>
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }

        /// <summary>
        /// Gets/sets whether the column should repeat last
        /// </summary>
        public bool ShouldRepeatLast
        {
            get
            {
                return shouldRepeatLast;
            }
            set
            {
                shouldRepeatLast = value;
            }
        }

        /// <summary>
        /// Gets/sets whether the column is required
        /// </summary>
        public bool IsRequired
        {
            get
            {
                return isRequired;
            }
            set
            {
                isRequired = value;
            }
        }

        /// <summary>
        /// Gets/sets whether the column is read only
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return isReadOnly;
            }
            set
            {
                isReadOnly = value;
            }
        }

        /// <summary>
        /// Gets/sets whether the column is...
        /// </summary>
        public bool IsUniqueField
        {
            get
            {
                return isUniqueField;
            }
            set
            {
                isUniqueField = value;
            }
        }

        /// <summary>
        /// Gets/sets the column's position
        /// </summary>
        public int Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        /// <summary>
        /// Gets/sets the column's width
        /// </summary>
        public int Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
            }
        }

        /// <summary>
        /// Gets/sets the column's type
        /// </summary>
        public MetaFieldType GridColumnType
        {
            get
            {
                return gridColumnType;
            }
            set
            {
                gridColumnType = value;
            }
        }

        #endregion

        #region Protected Properties

        #endregion Protected Properties

        #region Protected Methods
        /// <summary>
        /// Event handler for On Column Inserted event.
        /// </summary>
        protected void OnColumnInserted()
        {
            this.Grid.GetView().MustRefreshFieldCollection = true;
        }

        /// <summary>
        /// Insert new grid column to database.
        /// </summary>
        protected abstract void InsertColumn();
        /// <summary>
        /// Delete grid column from database.
        /// </summary>
        protected abstract void DeleteColumn();
        /// <summary>
        /// Update existing grid column to database.
        /// </summary>
        protected abstract void UpdateColumn();

        #endregion Protected Methods

        #region Public Methods

        /// <summary>
        /// Shortcut to get Metadata
        /// </summary>
        /// <returns></returns>
        protected IMetadataProvider GetMetadata()
        {
            return Grid.GetMetadata();
        }

        /// <summary>
        /// Saves the grid column to the database.
        /// </summary>
        public void SaveToDb()
        {
            if (Id == 0)
            {
                InsertColumn();
            }
            else
            {
                UpdateColumn();
            }
        }

        /// <summary>
        /// Deletes the grid column from the database.
        /// </summary>
        public void DeleteFromDb()
        {
            DeleteColumn();
        }

        /// <summary>
        /// Releases all resources used by this GridColumnBase.
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Returns database sepecific column type
        /// </summary>        
        /// <returns></returns>
        ///       
        public virtual string GetDbSpecificColumnType()
        {
            return Grid.GetProject().CollectedData.GetDatabase().GetDbSpecificColumnType(genericDbColumnType);
        }

        #endregion Public Methods
    }
}
