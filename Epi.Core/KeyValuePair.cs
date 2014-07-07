using System;
using System.Collections;

namespace Epi
{
    /// <summary>
    /// Key Value Pair
    /// </summary>
	public class KeyValuePair
	{
		#region Public Fields
        /// <summary>
        /// Key
        /// </summary>
		public string Key = string.Empty;
        /// <summary>
        /// Value
        /// </summary>
		public string Value = string.Empty;
		#endregion Public Fields

		#region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="val">Value</param>
		public KeyValuePair(string key, string val)
		{
			Key = key;
			Value = val;
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyValuePairString">Key value pair</param>
		public KeyValuePair(string keyValuePairString)
		{
			string[] keyVal = keyValuePairString.Split('=');
			Key = keyVal[0];
			Value = keyVal[1];
		}
		#endregion Constructors

		#region Public Methods
        /// <summary>
        /// ToString
        /// </summary>
        /// <returns>string representation</returns>
		public override string ToString()
		{
			return (Key + StringLiterals.EQUAL + Value);
		}
		#endregion Public Methods
	}
}