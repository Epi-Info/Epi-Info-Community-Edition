using System;
using System.Collections;
using System.Text;

namespace Epi.Collections
{
    /// <summary>
    /// Key-Value Pair Collection class
    /// </summary>
	public class KeyValuePairCollection : CollectionBase
	{
		#region Private class members
		
		private char delimiter = CharLiterals.SEMI_COLON;
		
		#endregion Private class members

		#region Constructors
        /// <summary>
        /// Builds a KeyValuePairCollection from string and specified delimiter.
        /// </summary>
        /// <param name="str">Delimited string from which to build the collection.</param>
        /// <param name="delim">The delimiter to set and use.</param>
        public KeyValuePairCollection(string str, char delim)
        {
            ConstructFromString(str, delim);
        }
		/// <summary>
		/// Builds a KeyValuePairCollection from string
		/// </summary>
		public KeyValuePairCollection(string str)
		{
			ConstructFromString(str);			
		}

		/// <summary>
		/// Default Constructor
		/// </summary>
		public KeyValuePairCollection()
		{
		}
		
		#endregion Constructors

		#region Public Properties		
		/// <summary>
		/// Gets/sets character separator between key and value.
		/// </summary>
        public char Delimiter
		{
			get
			{
				return delimiter;
			}
			set
			{
				this.delimiter = value;
			}
		}
		#endregion Public Properties

        #region Private Methods
        /// <summary>
        /// Construct Key-Value Pairs From String
        /// </summary>
        /// <param name="str">Delimited string from which to build the collection.</param>
        private void ConstructFromString(string str)
        {
            string[] kvPairStrings = str.Split(delimiter);
            foreach (string kvPairString in kvPairStrings)
            {
                if (!Util.IsEmpty(kvPairString))
                {
                    Add(new KeyValuePair(kvPairString));
                }
            }
        }

        /// <summary>
        /// Construct Key-Value Pairs From String
        /// </summary>
        /// <param name="str">Delimited string from which to build the collection.</param>
        /// <param name="delim">The delimiter to set and use</param>
        private void ConstructFromString(string str, char delim)
        {
            this.Delimiter = delim;
            ConstructFromString(str);
        }
        #endregion Private Methods

        #region Public Methods


		/// <summary>
		/// Adds a KeyValuePair to the collection
		/// </summary>
		/// <param name="item">A KeyValuePair object</param>
		/// <returns>Integer indicating whether or not the KeyValuePair was added</returns>
		public int Add(KeyValuePair item)
		{
			return List.Add(item);
		}

		/// <summary>
		/// Removes a KeyValuePair from the collection
		/// </summary>
		/// <param name="item">A KeyValuePair object</param>
		public void Remove(KeyValuePair item)
		{
			List.Remove(item);
		} 

		/// <summary>
		/// Determines whether a particular KeyValuePair object exists in the collection
		/// </summary>
		/// <param name="item">A KeyValuePair object</param>
		/// <returns>Indication of whether the object exists in the collection</returns>
		public bool Contains(KeyValuePair item)
		{
			return List.Contains(item);
		}

		/// <summary>
		/// Determines the index of a particular KeyValuePair object in the collection
		/// </summary>
		/// <param name="item">A KeyValuePair object</param>
		/// <returns>Index of the KeyValuePair object</returns>
		public int IndexOf(KeyValuePair item)
		{
			return List.IndexOf(item);
		}

		/// <summary>
		/// Copies the contents of the collection to an array
		/// </summary>
		/// <param name="array">An array of KeyValuePairs</param>
		/// <param name="index">Starting index to start copying items to</param>
		public void CopyTo(KeyValuePair[] array, int index)
		{
			List.CopyTo(array, index);
		}

		/// <summary>
		/// Access a particular index of the collection
		/// </summary>
		public KeyValuePair this[int index]
		{
			get 
			{ 
				return (KeyValuePair)List[index]; 
			}
			set 
			{ 
				List[index] = value; 
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach (KeyValuePair pair in this)
			{
				sb.Append(pair.ToString());
				sb.Append(Delimiter);
			}
			return sb.ToString();
		}
		#endregion Public Methods
	}
}
