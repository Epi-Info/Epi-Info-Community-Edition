#region Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

#endregion //Namespaces

namespace Epi.Windows.MakeView.Dialogs
{
	/// <summary>
	/// Dialog for Dialog command
	/// </summary>
    public partial class DialogListDialog : Epi.Windows.Dialogs.DialogBase
    {
        #region Constructors

        /// <summary>
        /// Default constructor - NOT TO BE USED FOR INSTANTIATION
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public DialogListDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="frm">The main form</param>
        public DialogListDialog(MainForm frm) : base(frm)
        {
           InitializeComponent();
           Construct();
        }
        #endregion Constructors

        #region Public Properties
        /// <summary>
        /// Selections of the list dialog
        /// </summary>
        public string Selections
        {
            get
            {
                return selections;
            }
        }
        #endregion Public Properties

        #region Protected Methods
        /// <summary>
        /// Sets the enabled property of OK and SaveOnly button
        /// </summary>		
        public override void CheckForInputSufficiency()
        {
            btnOK.Enabled = (ValidateInput() && ListCount(((DataView)dgList.DataSource)) > 0);
        }

        /// <summary>
        /// Validates user input
        /// </summary>
        /// <returns>True/False depending upon whether error messages were generated from validation</returns>
        protected override bool ValidateInput()
        {
            base.ValidateInput();
            return (ErrorMessages.Count == 0);
        }

        #endregion Protected Methods

        #region Private Attributes
        DataTable selectionList = null;
        string selections = string.Empty;
        #endregion Private Attributes

        #region Private Methods

        /// <summary>
        /// Constructs the dialog
        /// </summary>
        private void Construct()
        {
            const string SELECTION = "Selection";
            selectionList = new DataTable(SELECTION);
            selectionList.Columns.Add(new DataColumn(SELECTION));
            DataView dgListView = new DataView( selectionList );
            dgListView.ListChanged  += 
                new System.ComponentModel.ListChangedEventHandler(dgListView_ListChanged);
            this.dgList.DataSource = dgListView;
            this.dgList.DataMember = "";
            DataGridTableStyle ts = new DataGridTableStyle();
            ts.MappingName = SELECTION;
            dgList.TableStyles.Clear();
            dgList.TableStyles.Add(ts);
            dgList.TableStyles[SELECTION].GridColumnStyles[SELECTION].Width = dgList.Width - dgList.RowHeaderWidth;
            dgListView.AddNew();
        }

        /// <summary>
        /// Returns the number of non-null rows in the view
        /// </summary>
        /// <param name="view">The view</param>
        /// <returns>Number of non-null rows within the view</returns>
        private int ListCount(DataView view)
        {
            int count = 0;
            foreach (DataRowView row in view)
            {
                if (!string.IsNullOrEmpty(row[0].ToString()))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-dialog.html");
        }

        #endregion Private Methods

        #region Event Handlers

        /// <summary>
        /// Handles ListChanged event of the datagrid's DataSource
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void dgListView_ListChanged( object sender, System.EventArgs e )
        {
            CheckForInputSufficiency();
        }

        /// <summary>
        /// Handles the Click event of the OK button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            string s = string.Empty;
            StringBuilder sb = new StringBuilder();
            foreach (DataRowView row in ((DataView)dgList.DataSource))
            {
                s = row[0].ToString();
                if (!string.IsNullOrEmpty(s))
                {
                    sb.Append(StringLiterals.DOUBLEQUOTES);
                    sb.Append(s).Append(StringLiterals.DOUBLEQUOTES).Append(", ");
                }
            }
            sb.Length-= 2;
            selections = sb.ToString();
        }

        /// <summary>
        /// Handles the Click event of the Insert button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnInsert_Click(object sender, EventArgs e)
        {
            DataView view = ((DataView)dgList.DataSource);
            DataRowView row = view.AddNew();
            row.EndEdit();
            for (int i = view.Count-1; i > dgList.CurrentCell.RowNumber; i--)
            {
                row = view[i];
                row.BeginEdit();
                row.Row[0] = view[i-1][0].ToString();
                row.EndEdit();
            }
            row = view[dgList.CurrentCell.RowNumber];
            row.BeginEdit();
            row[0] = string.Empty;
            row.EndEdit();
        }

        /// <summary>
        /// Handles the Click event of the Delete button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            DataView view = ((DataView)dgList.DataSource);
            view.Delete(dgList.CurrentCell.RowNumber);
        }

        /// <summary>
        /// Handles the Mouse Click event of the datagrid
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied mouse event parameters</param>
        private void dgList_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (e.X > dgList.Left && e.X < (dgList.Left + dgList.RowHeaderWidth))
                {
                    //TODO:  Popup menu (Insert, Delete, Cancel)
                }
            }
        }

        #endregion Event Handlers
    }
}


