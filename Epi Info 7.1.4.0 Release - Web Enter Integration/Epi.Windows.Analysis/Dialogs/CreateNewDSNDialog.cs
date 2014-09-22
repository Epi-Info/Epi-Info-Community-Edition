using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Epi.Windows.Dialogs;

namespace Epi.Windows.Analysis.Dialogs
{

    /// <summary>
    /// CreateNewDSNDialog
    /// </summary>
    public partial class CreateNewDSNDialog : DialogBase
	{
		/// <summary>
		/// Default constructor - for exclusive use by the designer.
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public CreateNewDSNDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// CreateNewDSNDialog constructor
        /// </summary>
        /// <param name="frm"></param>
		public CreateNewDSNDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
        {
            InitializeComponent();
        }
	}
}