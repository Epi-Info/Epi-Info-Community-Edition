using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;

namespace Epi
{
	/// <summary>
	/// Builds a list of words with the provided delimiter
	/// </summary>
    public class WordBuilder : List<string>, IDisposable 
	{
		#region Fields		
		private string delimiter = StringLiterals.SPACE;		
		#endregion Fields

        #region Constructors
        /// <summary>
        /// Default constructor.
        /// </summary>
		public WordBuilder()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="delim">String used as the word separator.</param>
		public WordBuilder(string delim)
        {
            this.delimiter = delim;
        }
        #endregion Constructors

        #region Public Properties
        /// <summary>
        /// String used as the word separator.
        /// </summary>
		public string Delimitter
		{
			set
			{
				delimiter = value;
			}
		}
		
		#endregion Public Properties

		#region Public Methods
        /// <summary>
        /// Adds a string to the end of the System.Collections.Generic.List&lt;string&gt;
        /// </summary>
        /// <param name="str"></param>
		public void Append(string str)
		{
			base.Add(str);
		}

		/// <summary>
        /// Adds an array of strings to the end of the System.Collections.Generic.List&lt;string[]&gt;
		/// </summary>
		/// <param name="strings">An array of strings to append.</param>
		public void Append(string[] strings)
		{
			foreach (string str in strings)
			{
				Append(str);
			}
		}

        /// <summary>
        /// Adds an object to the end of the System.Collections.Generic.List&lt;object&gt;
        /// </summary>
        /// <param name="obj"></param>
		public void Append(object obj)
		{
			if (obj != null)
				Append(obj.ToString());
		}

		/// <summary>
		/// Returns a System.String that represents the current Epi.WordBuilder.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
            // Create a stringbuilder that can hold twice the number of items to accommodate delimiter chars
            if (Count > 0)
            {
                StringBuilder sb = new StringBuilder(Count * 2);
                for (int index = 0; index < Count - 1; index++)
                {
                    sb.Append(this[index].ToString());
                    sb.Append(delimiter);
                }
                sb.Append(this[Count - 1]);
                return sb.ToString();
            }
            else
            {
                return string.Empty;
            }
		}

        /// <summary>
        /// Terminate resources acquired for object instance.
        /// </summary>
		public void Dispose()
        {
        }

		#endregion Public Properties

	}
}