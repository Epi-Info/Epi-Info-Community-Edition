using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Epi.Windows.Dialogs;

namespace Epi.Windows.MakeView.Dialogs
{
	/// <summary>
	/// ***** This is a wireframe and currently contains no functionality *****
	/// </summary>
    public partial class TableSelectionDialog : DialogBase
	{
        private Epi.Project project;

        /// <summary>
        /// MetadataProvider
        /// </summary>
        public Epi.Data.Services.IMetadataProvider MetaDataProvider = null;
        /// <summary>
        /// SelectedTable
        /// </summary>
        public string SelectedTable = null;
		/// <summary>
		/// Constructor for the class
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public TableSelectionDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Dialog to select a tble 
        /// </summary>       
        /// <param name="frm">The main form</param>
        public TableSelectionDialog(Epi.Windows.MakeView.Forms.MakeViewMainForm frm)
            : base(frm)
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();


            if (frm.projectExplorer != null)
            {
                if (frm.projectExplorer.currentPage != null)
                {
                    this.project = frm.projectExplorer.currentPage.view.Project;
                    this.MetaDataProvider = this.project.Metadata;
                }
            }


        }

        /// <summary>
        /// OnLoad
        /// </summary>
        /// <param name="e">e</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.project != null)
            {
                this.Text = "Selected Project: " + this.project.Name;
                System.Collections.Generic.List<string> TableList = this.project.Metadata.GetDataTableList();
                foreach (string t in TableList)
                {
                    ListBox1.Items.Add(t);
                }
            }
        }

		/// <summary>
		/// Cancel button closes this dialog 
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
            this.project = null;
            this.MetaDataProvider = null;
            this.SelectedTable = null;

			this.Close();
		}

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox LB = ((ListBox)sender);
            if (LB.SelectedIndex > -1)
            {
                Button1.Enabled = true;
                this.SelectedTable = LB.SelectedItem.ToString();
            }
            else
            {
                Button1.Enabled = false;
                this.SelectedTable = null;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // openFileDialog.Filter = "Epi2000 and Epi7 Project Files (*.mdb;*.prj)|*.mdb;*.prj|Epi2000 Project files (*.mdb)|*.mdb |Epi7 Project Files (*.prj)|*.prj";
#if LINUX_BUILD
            openFileDialog.Filter = "*.prj";
#else
            openFileDialog.Filter = "Epi2000 and Epi7 Project Files (*.mdb;*.prj)|*.mdb;*.prj|Epi2000 Project files (*.mdb)|*.mdb|Epi7 Project Files (*.prj)|*.prj";
#endif

            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;
            DialogResult res = openFileDialog.ShowDialog();
            if (res == DialogResult.OK)
            {

                string filePath = openFileDialog.FileName.Trim();
                System.Collections.Generic.List<string> TableList;

                if (filePath.EndsWith(".prj"))
                {
                    this.project = new Project(filePath);
                    this.MetaDataProvider = this.project.Metadata;
                    TableList = this.project.Metadata.GetDataTableList();
                }
                else
                {
                    // to do 
                    TableList = new System.Collections.Generic.List<string>();
                    TableList.Add("to do add datatables for non epi7 projects");
                }
                ListBox1.Items.Clear();
                this.Text = "Selected Project: " + this.project.Name;
                foreach (string t in TableList)
                {
                    if (!string.IsNullOrEmpty(t))
                    {
                        ListBox1.Items.Add(t);
                    }
                }

            }
        }
	}
}

