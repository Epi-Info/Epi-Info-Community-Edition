using System;
using System.Data;
using Epi;
using Epi.Data;

namespace Epi.Fields
{
    /// <summary>
    /// Provides a text field in a view that enables users to enter text
    /// </summary>
	public abstract class TextField : InputTextBoxField
	{
		#region Private Class Members
		private int maxLength;
		private int sourceFieldId;
        private bool isEncrypted;
		#endregion //Private Class Members

		#region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">Page</param>
		public TextField(Page page) : base(page)
		{
            construct();
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">View</param>
		public TextField(View view) : base (view)
		{
            construct();
		}

        /// <summary>
        /// Load from row
        /// </summary>
        /// <param name="row">Row</param>
        public override void LoadFromRow(DataRow row)
        {
            base.LoadFromRow(row);
            if (row["MaxLength"].ToString().Length > 0)
            {
                maxLength = int.Parse(row["MaxLength"].ToString());
            }
            if (row["SourceFieldId"].ToString().Length > 0)
            {
                sourceFieldId = int.Parse(row["SourceFieldId"].ToString());
            }

            isEncrypted = false;

            if (row.Table.Columns.Contains(ColumnNames.IS_ENCRYPTED))
            {
                object encrypted = row[ColumnNames.IS_ENCRYPTED];

                if (encrypted != null && (encrypted is System.DBNull == false))//&& encrypted == true)
                {
                    isEncrypted = true;
                }
            }
        }

        public override void AssignMembers(Object field)
        {
            (field as TextField).maxLength = this.MaxLength;
            (field as TextField).sourceFieldId = this.sourceFieldId;
            base.AssignMembers(field);
        }

        private void construct()
        {
            genericDbColumnType = GenericDbColumnType.String;
            this.dbColumnType = DbType.String;
        }
		#endregion Constructors

		#region Public Properties

        /// <summary>
        /// Returns field type
        /// </summary>
        public override MetaFieldType FieldType
        {
            get
            {
                return MetaFieldType.Text;
            }
        }

        /// <summary>
        /// Returns a fully-typed current record value
        /// </summary>
        public string CurrentRecordValue
        {
            get
            {
                if (base.CurrentRecordValueObject == null) return string.Empty;
                else return CurrentRecordValueObject.ToString();
            }
            set
            {
                base.CurrentRecordValueObject = value;
            }
        }

        /// <summary>
        /// Returns a string representation of Current Record Value
        /// </summary>
        public virtual string CurrentRecordValueString
        {
            get
            {
                if (CurrentRecordValueObject == null) return string.Empty;
                else return CurrentRecordValueObject.ToString();
            }
            set
            {
                if (((TextField)this).MaxLength > 0 && value.Length > ((TextField)this).MaxLength)
                {
                    value = value.Substring(0, ((TextField)this).MaxLength);
                }

                CurrentRecordValueObject = value;
            }
        }

        /// <summary>
        /// Returns MaxLength
        /// </summary>
		public int MaxLength
		{
			get
			{
				return (maxLength);
			}
			set
			{
				maxLength = value;
			}
		}

        /// <summary>
        /// Returns Source Field Id
        /// </summary>
		public int SourceFieldId 
		{
			get
			{
				return sourceFieldId;
			}
			set
			{
				sourceFieldId = value;
			}
		}

        public bool IsEncrypted
        {
            get
            {
                return (isEncrypted);
            }
            set
            {
                isEncrypted = value;
            }
        }
        #endregion Public Properties

        #region Public Methods
        /// <summary>
        /// Returns specific data type for the target DB.
        /// </summary>
        /// <returns></returns>
        public override string GetDbSpecificColumnType()
        {
            return base.GetDbSpecificColumnType();
        }

        #endregion Public Methods
	}
}
