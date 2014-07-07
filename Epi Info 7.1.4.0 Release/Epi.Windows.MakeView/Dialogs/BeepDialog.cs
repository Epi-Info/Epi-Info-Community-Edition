using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using Epi;
using Epi.Windows;
//using Epi.Analysis;
//using Epi.Windows.Analysis;

namespace Epi.Windows.MakeView.Dialogs
{
	/// <summary>
	/// Dialog for Beep command
	/// </summary>
    public partial class BeepDialog : CommandDesignDialog
	{
		#region Constructor

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public BeepDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="frm">The main form</param>
        public BeepDialog(Epi.Windows.MakeView.Forms.MakeViewMainForm frm)
            : base(frm)
        {
            InitializeComponent();
            Construct();
        }

		#endregion Constructors

		#region Protected Methods
		/// <summary>
		/// Generates command text
		/// </summary>
		protected override void GenerateCommand()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(CommandNames.BEEP);
			CommandText = sb.ToString();
		}

		#endregion Protected Methods		

        #region Private Methods
        private void Construct()
        {
            if (!this.DesignMode)
            {
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
            }
        }
        #endregion Private Methods
    }
}

