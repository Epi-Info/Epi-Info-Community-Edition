using System;
using System.Data;
using System.Drawing;
using Epi.Data;
using Epi.Data.Services;

namespace Epi.Fields
{

    public delegate void FieldSaveCompleteHandler(Field field);

	/// <summary>
	/// Field
	/// </summary>
	public abstract class Field : IField
	{
		#region Constructors

		/// <summary>
		/// Private constructor - cannot be used
		/// </summary>
		private Field()
		{
		}

		/// <summary>
		/// Instantiates a field
		/// </summary>
		/// <param name="view">The view that contains this field</param>
		public Field(View view)
		{
			this.view = view;
		}

		#endregion Constructors

        #region Events

        public event FieldSaveCompleteHandler FieldInsertComplete;
        public event FieldSaveCompleteHandler FieldUpdateComplete;

        #endregion

        #region Fields
        private string name = string.Empty;
		private int id;
        private string sourceTable;
        private Guid uniqueId = Guid.NewGuid();
        /// <summary>
        /// The parent view of the field.
        /// </summary>
        protected View view;
        public static int fieldsWaitingToUpdate;
        
        private bool isVisible = true;
        private bool isEnabled = true;
        private bool isHighlighted = false;
        private bool isRequired = false;

		#endregion Fields

		#region Public Properties

		/// <summary>
		/// Returns the type of the field.
		/// </summary>
        public abstract MetaFieldType FieldType { get; }

        /// <summary>
        /// Returns the parent View
        /// </summary>
        /// <returns></returns>
        public View GetView()
        {
            return view;
        }

        /// <summary>
        /// Returns the parent View
        /// </summary>
        /// <returns></returns>
        public View View
        {
			get
			{
				return view;
			}
			set
			{
				view = value;
			}
        }

        /// <summary>
        /// Gets/sets the fields's Id.
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
        /// Gets/sets the field name
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
        /// Gets/sets the source of the field
        /// </summary>
        public string SourceTable
        {
            get
            {
                return sourceTable;
            }
            set
            {
                sourceTable = value;
            }
        }

        /// <summary>
        /// Gets/sets the id used to diffentiate between fields in metadata
        /// </summary>
        public Guid UniqueId
        {
            get
            {
                return uniqueId;
            }
            set
            {
                uniqueId = value;
            }
        }


        /// <summary>
        /// attribute to control enable property
        /// </summary>
        public bool IsEnabled { get { return this.isEnabled; } set { this.isEnabled = value; } }
        /// <summary>
        /// attribute to control visibility
        /// </summary>
        public bool IsVisible { get { return this.isVisible; } set { this.isVisible = value; } }
        /// <summary>
        /// attribute to control Highlighting
        /// </summary>
        public bool IsHighlighted { get { return this.isHighlighted; } set { this.isHighlighted = value; } }

        /// <summary>
        /// attribute to control whether the field is required
        /// </summary>
        public bool IsRequired { get { return this.isRequired; } set { this.isRequired = value; } }


		#endregion Public Properties

		#region Public Methods

        /// <summary>
        /// Shortcut for getting project instance.
        /// </summary>
        /// <returns>Current Epi Project object.</returns>
        public Project GetProject()
        {
            return this.GetView().GetProject();
        }

        /// <summary>
        /// Returns the metadata provider for the project
        /// </summary>
        /// <returns>Metadata Provider object.</returns>
        public IMetadataProvider GetMetadata()
        {
            return GetProject().Metadata;
        }

        /// <summary>
        /// Populates the field object from the database row
        /// </summary>
        /// <param name="row">A row of data in <see cref="System.Data.DataTable"/>.</param>
		public virtual void LoadFromRow(DataRow row)
        {
            this.Id = (int)row[ColumnNames.FIELD_ID];
            this.Name = row[ColumnNames.NAME].ToString();
            this.SourceTable = row[ColumnNames.SOURCE_TABLE_NAME].ToString();
            if (row["UniqueId"] is Guid)
            {
                uniqueId = (Guid)row["UniqueId"];
            }
            else if (row["UniqueId"] is String)
            {
                Guid tryguid;
                if (Guid.TryParse(row["UniqueId"].ToString(), out tryguid))
                {
                    uniqueId = tryguid;
                }
            }
        }

        public virtual void AssignMembers(Object field)
        {
            (field as Field).Id = this.Id;
            (field as Field).Name = this.Name;
            (field as Field).SourceTable = this.SourceTable;
            (field as Field).uniqueId = this.uniqueId;
        }
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
        }
		
		/// <summary>
		/// Saves the field to the database
		/// </summary>
		public virtual void SaveToDb()
		{
			if (Id == 0)
			{
				InsertField();
			}
			else
			{
				throw new System.ApplicationException("Can't update this field");
			}
		}
        /// <summary>
        /// Deletes the field from the view
        /// </summary>
		public abstract void Delete();

		#endregion Public Methods

		#region Protected Methods

        /// <summary>
        /// Executes when a field is inserted into the view.
        /// </summary>
		protected void OnFieldAdded()
		{
			view.MustRefreshFieldCollection = true;
		}

		/// <summary>
		/// Inserts the field to the database
		/// </summary>
		protected abstract void InsertField();

        protected void OnFieldInserted(Field f)
        {
            if (FieldInsertComplete != null)
            {
                FieldInsertComplete(f);
            }
        }

        protected void OnFieldUpdated(Field f)
        {
            if (FieldUpdateComplete != null)
            {
                FieldUpdateComplete(f);
            }
        }

		#endregion Protected Methods
	}
}
