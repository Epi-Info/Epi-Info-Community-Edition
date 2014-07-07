using System;
using System.Text;

namespace Epi.Web
{

    /// <summary>
    /// Contains utilities for building HTML code
    /// </summary>
    public class HtmlDocument
    {

        #region Private Attributes
        private StringBuilder docString = null;
        //private int bookmarkNumber = 0;
        //private int sectionNumber = 0;
//        private string docString = string.Empty;
        #endregion Private Attributes
        

        /// <summary>
        /// Creates a new HTMLDcoument
        /// </summary>
		public HtmlDocument()
		{
            docString = new StringBuilder();
		}

        /// <summary>
        /// Creates a new HTMLDcoument
        /// </summary>
        public HtmlDocument(string aString)
        {
            docString = new StringBuilder(aString);
        }

		#region Public Methods

        /// <summary>
        /// Returns the HTML Document as a string
        /// </summary>
        /// <returns></returns>
		public override string ToString()
		{
			return docString.ToString();
		}


        /// <summary>
        /// Insert Key and value in HTML Document
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
		public void InsertKeyValues(string key, string val)
		{
            docString.Append(InsertInEm(key)).Append(":");
            InsertSpace();
			docString.Append(InsertInStrong(val));
		}

        /// <summary>
        /// Insert Key and value in HTML Document
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void InsertKeyValues(string key, int val)
        {
            docString.Append(InsertInEm(key)).Append(":");
            InsertSpace();
            docString.Append(InsertInStrong(val.ToString()));
        }

        /// <summary>
        /// Insert count spaces in HTML Document
        /// </summary>
        /// <param name="count"></param>
		public void InsertSpaces(int count)
		{
			for (int index = 0; index < count; index++)
			{
				InsertSpace();
			}
		}

        /// <summary>
        /// Inserts a single space in HTML Document
        /// </summary>
        public void InsertSpace()
		{
			docString.Append("&nbsp");
		}

        /// <summary>
        /// Insert a Line Break in HTML Document
        /// </summary>
		public void InsertLineBreak()
		{
			docString.Append("<br />");
		}
		#endregion Public Methods

		#region Private Methods		
		private static string InsertInEm(string str)
		{
			return "<em>" + str + "</em>";
		}
		private static string InsertInStrong(string str)
		{
			return "<strong>" + str + "</strong>";
		}
		
		#endregion Private Methods
	}
}
