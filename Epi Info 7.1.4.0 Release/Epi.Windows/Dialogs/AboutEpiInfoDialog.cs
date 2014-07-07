using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Epi;

using System.Text;
using Epi.Windows;

namespace Epi.Windows.Dialogs
{
	/// <summary>
	/// About Epi Info Dialog
	/// </summary>
    public partial class AboutEpiInfoDialog : DialogBase
	{
		#region Constructors
        /// <summary>
        /// 
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public AboutEpiInfoDialog()
        {
            InitializeComponent();
        }

		/// <summary>
		/// Constructor for AboutEpiInfo dialog
		/// </summary>
		public AboutEpiInfoDialog(MainForm frm) : base(frm)
		{
			InitializeComponent();
		}

		#endregion Constructors
				
		#region Event Handlers

		/// <summary>
		/// Close the dialog
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnOK_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		/// <summary>
		/// AboutEpiInfo load event
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void AboutEpiInfo_Load(object sender, System.EventArgs e)
		{		
			try
			{
                ApplicationIdentity appId = new ApplicationIdentity(typeof(Configuration).Assembly);

				// App Name 
				WordBuilder appNameBuilder = new WordBuilder();
                appNameBuilder.Add(appId.SuiteName);
                appNameBuilder.Add(appId.Version);
				lblAppName.Text = appNameBuilder.ToString();
				
				// Release date
                lblReleaseDate.Text = appId.VersionReleaseDate;

				// Description
                txtDescription.Text = Util.GetProductDescription();
			}
			catch (FileNotFoundException ex)
			{	
				MsgBox.ShowException(ex);
			}
		}

		#endregion

	}
}


