using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Epi.Windows.Dialogs
{
    /// <summary>
    /// Search dialog useful for search for text withing some sort of editor.
    /// </summary>
    public partial class SearchDialog : Epi.Windows.Dialogs.DialogBase
    {

        #region Enumerated type
        /// <summary>
        /// Enumeration for the direction in which the search is to proceeed
        /// </summary>
        public enum Direction
        {
            /// <summary>
            /// Not specified
            /// </summary>
            None,
            /// <summary>
            /// Start from the beginning, going forward
            /// </summary>
            Beginning,
            /// <summary>
            /// Start from current location going forward
            /// </summary>
            Forward,
            /// <summary>
            /// Start from current location going backward
            /// </summary>
            Backward
        }
        #endregion Enumerated type

        #region Constructors

        /// <summary>
        /// Default constructor for the SearchDialog
        /// </summary>
        public SearchDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for the seach dialog which takes a search string as an argument.
        /// </summary>
        /// <param name="searchString"></param>
        public SearchDialog(string searchString)
        {
            InitializeComponent();
            this.txtSearchString.Text = searchString;
        }

        #endregion Constructors

        #region Public Properties

        /// <summary>
        /// Property - The string for which to search
        /// </summary>
        public string SearchString
        {
            get
            {
                return txtSearchString.Text;
            }
        }

        /// <summary>
        /// Property - The intended direction of the search (from beginning, forward, backward)
        /// </summary>
        public Direction SearchDirection
        {
            get
            {
                if (rbForward.Checked)
                {
                    return Direction.Forward;
                }
                else if (rbBackward.Checked)
                {
                    return Direction.Backward;
                }
                else
                {
                    return Direction.Beginning;
                }
            }
        }

        /// <summary>
        /// Property - Is true for a case sensitive search
        /// </summary>
        public bool CaseSensitive
        {
            get
            {
                return chkCaseSensitive.Checked;
            }
        }

        /// <summary>
        /// Property - Is true for search for whole word only
        /// </summary>
        public bool WholeWord
        {
            get
            {
                return chkWholeWord.Checked;
            }
        }

        #endregion Public Properties

        #region Event Handlers

        private void txtSearchString_Leave(object sender, EventArgs e)
        {
            btnOK.Enabled = (!string.IsNullOrEmpty(txtSearchString.Text));
        }

        #endregion Event Handlers
    }
}

