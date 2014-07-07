#region Namespaces

using System;
using System.Collections.Generic;
using System.Text;
using Epi.Windows.Dialogs;

#endregion  //Namespaces

namespace Epi.Windows.MakeView.Dialogs.CheckCodeCommandDialogs
{
    /// <summary>
    /// Base class for all check code design dialogs
    /// </summary>
    public partial class CheckCodeDesignDialog : DialogBase
    {
        #region Private Attributes
        private string output;
        public EpiInfo.Plugin.IEnterInterpreter EpiInterpreter;
        #endregion Private Attributes

        #region Constructors
        /// <summary>
        /// Default Constructor - Design time only
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public CheckCodeDesignDialog()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="frm">The main form</param>
        public CheckCodeDesignDialog(MainForm frm)
            : base(frm)
        {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The view
        /// </summary>
		public virtual View View
        {
            set {/* Do Nothing */}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <summary>
        /// Gets or sets the output of the dialog
        /// </summary>
        public string Output
        {
            get
            {
                return this.output;
            }
            set
            {
                this.output = value;
            }
        }
        #endregion Properties
    }
}
