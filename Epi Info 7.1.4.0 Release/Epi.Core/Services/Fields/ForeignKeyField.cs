using System;
using System.Data;
using System.Drawing;
using System.Xml;

using Epi.Data.Services;
using EpiInfo.Plugin;

namespace Epi.Fields
{
	/// <summary>
	/// Foreign Key Field to a Unique Key.
	/// </summary>
	public class ForeignKeyField : PredefinedDataField, IMirrorable, IInputField
	{
		#region Constructors

		/// <summary>
		/// Constructor for  the class. Instantiates a field
		/// </summary>
		/// <param name="view">The view this field belongs to.</param>
		public ForeignKeyField(View view) : base(view)
		{
			Construct();
		}

        /// <summary>
        /// Constructor for  the class.
        /// </summary>
        /// <param name="view">The view this field belongs to.</param>
        /// <param name="viewElement">Xml element of field.</param>
        public ForeignKeyField(View view, XmlElement viewElement)
            : this(view)          
        {
            this.viewElement = viewElement;            
        }

        /// <summary>
        /// Constructor for  the class.
        /// </summary>
        /// <param name="view">The view this field belongs to.</param>
        /// <param name="fieldNode">Xml node of field.</param>
        public ForeignKeyField(View view, XmlNode fieldNode)
            : this(view)
        {
            this.fieldNode = fieldNode;
            this.view.Project.Metadata.GetFieldData(this, fieldNode);           
        }

        /// <summary>
        /// Common constructor code to provide a name for the class.
        /// </summary>
		private void Construct()
		{
            this.Name = ColumnNames.FOREIGN_KEY;
		}		
		#endregion Constructors

        #region Private Data Members
        private XmlElement viewElement;
        private XmlNode fieldNode;
        #endregion  //Private Data Members

        #region Public Properties
        /// <summary>
        /// Returns field type
        /// </summary>
        public override MetaFieldType FieldType
        {
            get
            {
                return MetaFieldType.ForeignKey;
            }
        }

        /// <summary>
        /// Returns/sets the prompt text.
        /// </summary>
        public override string PromptText
        {
            get
            {
                return SharedStrings.FOREIGN_KEY;
            }
            set 
            {
                throw new GeneralException("Prompt for Foreign key is pre-defined");
            }
        }
        public string Prompt { get { return SharedStrings.FOREIGN_KEY; } set { return; } }


        /// <summary>
        /// Returns/sets the  is read only flag.
        /// </summary>
        public  bool IsReadOnly
        {
            get
            {
                return true;
            }
            set
            {
                throw new GeneralException("IsReadOnly for Foreign Key is pre-defined");
            }
        }

        /// <summary>
        /// Returns/sets the  is required flag.
        /// </summary>
        public  bool IsRequired
        {
            get
            {
                return false;
            }
            set
            {
                throw new GeneralException("IsRequired for Foreign Key is pre-defined");
            }
        }

        /// <summary>
        /// Returns/sets the should repeat last flag.
        /// </summary>
        public  bool ShouldRepeatLast
        {
            get
            {
                return false;
            }
            set
            {
                throw new GeneralException("ShouldRepeatLast for Foreign Key is pre-defined");
            }
        }

        /// <summary>
        /// Gets SQL data type of Foreign Key field.
        /// </summary>
        public string GetDbSpecificColumnType()
        {
            return SqlDataTypes.NVARCHAR;
        }
        /// <summary>
        /// The view element of the field
        /// </summary>
        public XmlElement ViewElement
        {
            get
            {
                return viewElement;
            }
            set
            {
                viewElement = value;
            }
        }


         #endregion Public Properties

        #region Public Methods

        /// <summary>
		/// Saves the field to the database
		/// </summary>
		protected override void InsertField()
		{
			if (this.Id == 0)
			{
                this.Id = GetMetadata().CreateField(this);                			
			}
			else
			{
				throw new System.ApplicationException("Foreign key field already exists");
			}
		}		

		/// <summary>
		/// Deletes the field
		/// </summary>
		public override void Delete()
		{
			GetMetadata().DeleteField(this);
		}

        /// <summary>
        /// Returns the string value that is reflected my a mirror field.
        /// </summary>
        /// <returns>reflected value string</returns>
        public virtual string GetReflectedValue()
        {
            return this.CurrentRecordValueString;
        }

		#endregion

        #region ICloneable Members

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
	}
}