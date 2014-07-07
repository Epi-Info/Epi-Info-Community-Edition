#region Namespaces

using System;
using System.Data;
using System.Windows.Forms;

#endregion

namespace Epi.Windows.Controls
{
	/// <summary>
	/// ComboBox Of Database Formats
	/// </summary>
	public class ComboBoxOfDatabaseFormats : Epi.Windows.Controls.LocalizedComboBox
	{
		#region Private Class Members
        //private DbFormatType selectedDbFormatType;
		#endregion

		#region Constructors
		
		/// <summary>
		/// Constructor for the class
		/// </summary>
		public ComboBoxOfDatabaseFormats()
		{
			if (!this.DesignMode)
			{				
				DisplayMember = ColumnNames.NAME;
				ValueMember = ColumnNames.ID;
                //TODO: Attach list from configuration
				//DataView dv = AppData.Instance.DatabaseFormatsDataTable.DefaultView;
				//dv.Sort = ColumnNames.POSITION;
				//DataSource = dv;				
				SkipTranslation = false;
			}			
		}

		#endregion Constructors

        #region Public Properties

        ///// <summary>
        ///// The database format selected from the combo box
        ///// </summary>
        //public DbFormatType SelectedDbFormatType
        //{
        //    get
        //    {
        //        return (selectedDbFormatType);
        //    }
        //    set
        //    {
        //        selectedDbFormatType = value;
        //    }
        //}

		#endregion  //Public Properties

		#region Event Handlers

		/// <summary>
		/// Handles the selected index change event of the combo box
		/// </summary>
		/// <param name="e">.NET supplied event parameter</param>
		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			//SelectedDbFormatType = (DbFormatType)(int.Parse(this.SelectedValue.ToString()));
			base.OnSelectedIndexChanged (e);
		}

		#endregion
		
	}
}
