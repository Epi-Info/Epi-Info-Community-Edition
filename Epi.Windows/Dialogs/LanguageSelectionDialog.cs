using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;

namespace Epi.Windows.Dialogs
{
	/// <summary>
	/// Dialog for Language selection
	/// </summary>
    public partial class LanguageSelectionDialog : DialogBase
	{
		#region Constructors

        /// <summary>
        ///
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public LanguageSelectionDialog()
        {
            InitializeComponent();
        }

		/// <summary>
		/// Constructor for LanguageSelection dialog
		/// </summary>
		public LanguageSelectionDialog(MainForm frm) : base(frm)
		{
			InitializeComponent();
		}

		#endregion
		
		#region Public Properties

		/// <summary>
		/// Read-only accessor method
		/// </summary>
		public CultureInfo SelectedCulture
		{
			get
			{
                return new CultureInfo(cbxLanguages.SelectedValue.ToString());
			}
		}

		#endregion

		#region Private Class

		/// <summary>
		/// Private class for comparing two cultures
		/// </summary>
		private class CultureComparer : IComparer 
		{

			/// <summary>
			/// Compares two cultures
			/// </summary>
			/// <param name="x">First culture</param>
			/// <param name="y">Second culture</param>
			/// <returns>Integer representing the lexical relationship between the two cultures</returns>
			public int Compare(object x, object y)
			{
				CultureInfo cultureX = (CultureInfo)x;
				CultureInfo cultureY = (CultureInfo)y;
				return string.Compare(cultureX.DisplayName, cultureY.DisplayName);
			}
		}

		#endregion
		
		#region Event Handlers

		/// <summary>
		/// OK button click event
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnOk_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Hide();
		}

		/// <summary>
		/// Cancel closes this dialog
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		/// <summary>
		/// Loads languages combo box with neutral culture languages
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void LanguageSelection_Load(object sender, System.EventArgs e)
		{
			CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures);
			Array.Sort(cultures, new CultureComparer());
			cbxLanguages.DataSource = cultures;
			cbxLanguages.DisplayMember = ColumnNames.DISPLAY_NAME;
			cbxLanguages.ValueMember = ColumnNames.NAME;
		}

		#endregion

	}
}

