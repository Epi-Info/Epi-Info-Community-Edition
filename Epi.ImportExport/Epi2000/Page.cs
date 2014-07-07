using System;
using System.Data;

namespace Epi.Epi2000
{
	/// <summary>
	/// A page in an Epi2000 project
	/// </summary>
	public class Page
    {
        #region Fields
        private int position = 0;
        private string name = string.Empty;
        private string checkCodeBefore = string.Empty;
        private string checkCodeAfter = string.Empty;
        #endregion Fields

        #region Constructors
        /// <summary>
		/// Constructs a page linked to a view
		/// </summary>
		/// <param name="view">A view object</param>
		public Page(View view)
		{
		}

		/// <summary>
		/// Constructs a page from database table row
		/// </summary>
		/// <param name="row"></param>
		/// <param name="view"></param>
		public Page(DataRow row, View view)
		{
            if (row[ColumnNames.NAME] != DBNull.Value)
                this.Name = row[ColumnNames.NAME].ToString();
            // zero based Position in Epi 7
			this.Position = Math.Abs(short.Parse(row[ColumnNames.PAGE_NUMBER].ToString())) - 1;
            string checkCodeBlock = row[ColumnNames.CHECK_CODE].ToString();
            RebuildCheckCode(checkCodeBlock, view);
		}
		#endregion Constructors

        #region Public Properties

        /// <summary>
        /// Returns or sets the Id
        /// </summary>
        public int Id
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns the name of the page
        /// </summary>
        public string Name
        {
            get
            {
                return (name);
            }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// Position of the page in it's views
        /// </summary>
        public int Position
        {
            get
            {
                return (position);
            }
            set
            {
                position = value;
            }
        }

        /// <summary>
        /// Check code that executes after all the data is entered on the page
        /// </summary>
        public string CheckCodeAfter
        {
            get
            {
                return (checkCodeAfter);
            }
            set
            {
                checkCodeAfter = value;
            }
        }

        /// <summary>
        /// Check code that executes before the page is loaded for data entry
        /// </summary>
        public string CheckCodeBefore
        {
            get
            {
                return (checkCodeBefore);
            }
            set
            {
                checkCodeBefore = value;
            }
        }
        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Copies a page object into this one.
        /// </summary>
        /// <param name="other">The page</param>
        public void CopyTo(Epi.Page other)
        {
            other.Name = this.Name;
            other.Position = this.Position;
            other.CheckCodeBefore = this.CheckCodeBefore;
            other.CheckCodeAfter = this.CheckCodeAfter;
        }

        /// <summary>
        /// Rebuilds the check code for the page
        /// </summary>
        /// <param name="checkCodeBlock">The check code block</param>
        /// <param name="view">The view</param>
        public void RebuildCheckCode(string checkCodeBlock, View view)
        {
            if (checkCodeBlock == null)
            {
                return;
            }
            System.Text.StringBuilder CheckCode = new System.Text.StringBuilder();    

            bool PageHasBeforeCheckCode = false;
            bool PageHasAfterCheckCode = false;
            
            string ckBefore = string.Empty;
            string ckAfter = string.Empty;

            Epi2000.MetadataDbProvider.SplitCheckCode(checkCodeBlock, ref ckBefore, ref ckAfter);

            if (!string.IsNullOrEmpty(ckAfter))
            {
                PageHasAfterCheckCode = true;
            }
            if (!string.IsNullOrEmpty(ckBefore))
            {
                PageHasBeforeCheckCode = true;
            }

            if (PageHasBeforeCheckCode || PageHasAfterCheckCode)
            {
                CheckCode.Append("\nPage ");
                if (this.name.Trim().IndexOf(' ') > -1 || this.name.ToLower().Equals("page"))
                {
                    CheckCode.Append("[");
                    CheckCode.Append(this.name.Trim().Replace("&", string.Empty)); // Imported page names can't have & symbol
                    CheckCode.Append("]");
                }
                else
                {
                    CheckCode.Append(this.name.Trim().Replace("&", string.Empty)); // Imported page names can't have & symbol
                }


                if (PageHasBeforeCheckCode)
                {
                    CheckCode.Append("\n\tBefore\n\t\t");
                    CheckCode.Append(ckBefore.Replace("\n", "\n\t\t"));
                    CheckCode.Append("\n\tEnd-Before\n");
                }

                if (PageHasAfterCheckCode)
                {
                    CheckCode.Append("\n\tAfter\n\t\t");
                    CheckCode.Append(ckAfter.Replace("\n", "\n\t\t"));
                    CheckCode.Append("\n\tEnd-After\n");
                }


                CheckCode.Append("End-Page\n");
            }

            this.CheckCodeBefore = CheckCode.ToString();            
        }
        #endregion Public Methods
    }
}