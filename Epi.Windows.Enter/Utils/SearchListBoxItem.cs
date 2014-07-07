#region namespaces
using System;
using System.Collections.Generic;
using System.Text;
#endregion //namespaces

namespace Epi.Windows.Enter.Utils
{

    class SearchListBoxItem   
    {
        #region Private Members

        private string name = string.Empty;
        private string type = string.Empty;
        private string pattern = string.Empty;
        private object idVal;
        private string itemValue = string.Empty;

        #endregion
        
        #region Constructors
		/// <summary>
		/// Constructor for the class
		/// </summary>
		public SearchListBoxItem()
		{
		}
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

        public string Pattern
        {
            get
            {
                return pattern;
            }
            set
            {
                pattern = value;
            }
        }

        public string ItemValue
        {
            get
            {
                return itemValue;
            }
            set
            {
                itemValue = value;
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
        /// <param name="pattern"></param>
        /// <param name="itemValue"></param>
		public SearchListBoxItem(string name, object idVal, string type, string pattern, string itemValue)
		{
			this.name = name;
			this.idVal = idVal;
			this.type = type;
            this.pattern = pattern;
            this.itemValue = itemValue;
		}

        /// <summary>
        /// Returns the name of the object
        /// </summary>
        /// <returns>The name of the object</returns>
        public override string ToString()
        {
            return name;
        }

        #endregion
    }
}
