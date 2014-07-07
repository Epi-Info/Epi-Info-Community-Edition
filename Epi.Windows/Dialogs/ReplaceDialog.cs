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
    /// Replace dialog for Program Editor.
    /// </summary>
    public partial class ReplaceDialog : Epi.Windows.Dialogs.DialogBase
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
        /// Default constructor for the ReplaceDialog
        /// </summary>
        public ReplaceDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for the replace dialog which takes a search string as an argument.
        /// </summary>
        /// <param name="searchString"></param>
        public ReplaceDialog(string searchString)
        {
            InitializeComponent();
            this.txtReplace.Text = searchString;
        }

        #endregion Constructors

        #region Public Properties

        /// <summary>
        /// Property - The string for which to search
        /// </summary>
        public string ReplaceString
        {
            get
            {
                return txtReplace.Text;
            }
        }


        /// <summary>
        /// Property - The replacement string.
        /// </summary>
        public string ReplacementString
        {
            get
            {
                return txtReplacement.Text;
            }
        }

        ///// <summary>
        ///// Property - The intended direction of the search (from beginning, forward, backward)
        ///// </summary>
        //public Direction SearchDirection
        //{
        //    get
        //    {
        //        if (rbForward.Checked)
        //        {
        //            return Direction.Forward;
        //        }
        //        else if (rbBackward.Checked)
        //        {
        //            return Direction.Backward;
        //        }
        //        else
        //        {
        //            return Direction.Beginning;
        //        }
        //    }
        //}

        /// <summary>
        /// Property - Is true for a case sensitive search
        /// </summary>
        public bool CaseSensitive
        {
            get
            {
                return chkReplaceAll.Checked;
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

        /// <summary>
        /// Property - Is true to replace all occurances of search string
        /// </summary>
        public bool ReplaceAll
        {
            get
            {
                return chkReplaceAll.Checked;
            }
        }

        #endregion Public Properties

        #region Event Handlers

        private void txtSearchString_Leave(object sender, EventArgs e)
        {
            btnOK.Enabled = ( (!string.IsNullOrEmpty(txtReplace.Text)) &&
                            (!string.IsNullOrEmpty(txtReplacement.Text)) );
        }

        #endregion Event Handlers
    }
}

