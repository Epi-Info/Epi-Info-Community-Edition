using System;
using System.Data;


namespace Epi.Fields
{
	/// <summary>
	/// Option field item
	/// </summary>
	public class OptionFieldItem
	{

		#region Private Members

		private int id;
		private string val = string.Empty;
		private string text = string.Empty;
		private int position;
		private OptionField parent;

		#endregion

		#region Constructors

		/// <summary>
		/// Private constructor - not used
		/// </summary>
		private OptionFieldItem()
		{
		}

		/// <summary>
		/// Constructor for the class
		/// </summary>
		public OptionFieldItem(OptionField parent)
		{
			this.parent = parent;
		}

		/// <summary>
		/// Constructor for the class
		/// </summary>
		/// <param name="row">A DataRow containing the option row's data</param>
        /// <param name="parent">the object that contains the row</param>
        public OptionFieldItem(DataRow row, OptionField parent)
		{
			this.id = (int)row[ColumnNames.OPTION_ID];
			this.val = row[ColumnNames.NAME].ToString();
			this.text = row[ColumnNames.TEXT].ToString();
			this.position = (short) row[ColumnNames.POSITION];
			this.parent = parent;
		}

		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Gets/sets the parent option field
		/// </summary>
		public OptionField Parent
		{
			get
			{
				return this.parent;
			}
			set
			{
				this.parent = value;
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
		/// Gets/sets the value
		/// </summary>
		public string Value
		{
			get
			{
				return val;
			}
			set
			{
				val = value;
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

		#endregion

		#region Public Methods

        /// <summary>
        /// Save to the database
        /// </summary>
		public void SaveToDb()
		{

		}

		#endregion

	}
}