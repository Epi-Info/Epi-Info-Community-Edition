#region namespaces
using System;
#endregion

namespace Epi.Windows.MakeView.Utils
{
    /// <summary>
    /// The Editor Combobox Item
    /// </summary>
	public class EditorComboBoxItem
	{	

		#region Constructors
		/// <summary>
		/// Constructor for the class
		/// </summary>
		public EditorComboBoxItem()
		{
		}
		#endregion

		#region Private Members

		private string name = string.Empty;
		private string type = string.Empty;
		private object idVal;	

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets and sets the name property 
		/// of the object being returned from the database
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
		/// Gets and sets the Id of the object being 
		/// returned from the database
		/// </summary>
		public object IDVal
		{
			get
			{
				return idVal;
			}
			set
			{
				idVal = value;
			}
		}

		/// <summary>
		/// Gets and set the Type of the object being
		/// returned from the database
		/// </summary>
		public string Type
		{
			get
			{
				return type;
			}
			set
			{
				type = value;
			}
		}	
		#endregion

		#region Public Methods

		/// <summary>
		/// Sets the value of the name, idVal and type
		/// of the object being returned from the database
		/// </summary>
		/// <param name="name">Name of the object</param>
		/// <param name="idVal">Id of the object</param>
		/// <param name="type">Type of object</param>
		public EditorComboBoxItem(string name, object idVal, string type)
		{
			this.name = name;
			this.idVal = idVal;
			this.type = type;			
		}

		/// <summary>
		/// Returns the name of the object
		/// </summary>
		/// <returns>Name of the object</returns>
		public override string ToString()
		{
			return name;
		}

		#endregion
	}
}
