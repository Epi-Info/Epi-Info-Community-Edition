using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Epi.Analysis;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Dialog for Bookmark command
	/// </summary>
    public partial class BookmarkDialog : CommandDesignDialog
	{
		#region Constructor

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public BookmarkDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Constructor for Bookmark dialog
		/// </summary>
        public BookmarkDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
		{
			InitializeComponent();
		}
		#endregion Constructor	

        #region Public Properties
        /// <summary>
        /// Gets/sets the bookmark property.
        /// </summary>
        public string Bookmark
        {
            get { return txtBookmark.Text; }
            set { txtBookmark.Text = value; }
        }
	
        #endregion Public Properties
    }
}

