using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Epi.Windows.MakeView.Dialogs
{
    /// <summary>
    /// Table to View Dialog
    /// </summary>
    public partial class TableToViewDialog : Form
    {
        private string TableName = null;
        private string ViewName = null;
        private Epi.Data.IDbDriver DbDriver = null;
        private Dictionary<string, string> columnMapping;

        /// <summary>
        /// Selected Columns
        /// </summary>
        public List<string> SelectedColumns = new List<string>();

        /// <summary>
        /// TableToViewDialog
        /// </summary>
        /// <param name="pTableName">table name</param>
        /// <param name="pViewName">view name</param>
        /// <param name="pDbDriver">IDBDriver</param>
        /// <param name="pColumnMapping">Column mapping scheme</param>
        public TableToViewDialog(string pTableName, string pViewName, Epi.Data.IDbDriver pDbDriver, Dictionary<string, string> pColumnMapping)
        {
            InitializeComponent();

            TableName = pTableName;
            ViewName = pViewName;
            DbDriver = pDbDriver;
            columnMapping = pColumnMapping;
        }

        /// <summary>
        /// On load
        /// </summary>
        /// <param name="e">eventargs</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            textTableName.Text = TableName;
            textViewName.Text = ViewName;

            DataTable DT = DbDriver.GetTopTwoTable(TableName);//.GetTableData(TableName);

            foreach (KeyValuePair<string, string> columnKvp in columnMapping)
            {
                string sourceColumnName = columnKvp.Key;
                string validColumName = columnKvp.Value;

                DataColumn column = DT.Columns[sourceColumnName];

                if (column.ColumnName.ToUpperInvariant() == "UNIQUEKEY" || column.ColumnName.ToUpperInvariant() == "RECSTATUS" || column.ColumnName.ToUpperInvariant() == "GLOBALRECORDID" || column.ColumnName.ToUpperInvariant() == "FKEY")
                {
                    continue;
                }
                
                string message = string.Empty;
                if (!Util.IsValidFieldName(validColumName, ref message))
                {
                    continue;
                }

                listTableFields.Items.Add(validColumName);
            }
        }

        private void btnAddField_Click(object sender, EventArgs e)
        {
            if (listTableFields.SelectedIndex > -1)
            {
                ListViewItem listViewItem = new ListViewItem(listTableFields.SelectedItem.ToString());
                if (!listViewFields.Items.Contains(listViewItem) && !listViewFields.Items.ContainsKey(listViewItem.Text))
                {
                    listViewFields.Items.Add(listViewItem.Text, listViewItem.Text, string.Empty);
                }

                if (listViewFields.Items.Count > 0)
                {
                    btnSave.Enabled = true;
                }
            }
        }

        private void btnRemoveField_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem listViewItem in listViewFields.SelectedItems)
            {
                listViewFields.Items.Remove(listViewItem);
            }

            if (listViewFields.Items.Count == 0)
            {
                btnSave.Enabled = false;
            }

        }

        private void btnAddAll_Click(object sender, EventArgs e)
        {
            DataTable DT = DbDriver.GetTopTwoTable(TableName);//.GetTableData(TableName);

            foreach (KeyValuePair<string, string> columnKvp in columnMapping)
            {
                string sourceColumnName = columnKvp.Key;
                string validColumName = columnKvp.Value;

                DataColumn column = DT.Columns[sourceColumnName];

                if (column.ColumnName.ToUpperInvariant() == "UNIQUEKEY" || column.ColumnName.ToUpperInvariant() == "RECSTATUS" || column.ColumnName.ToUpperInvariant() == "GLOBALRECORDID" || column.ColumnName.ToUpperInvariant() == "FKEY")
                {
                    continue;
                }
                string message = string.Empty;
                if (!Util.IsValidFieldName(validColumName, ref message))
                {
                    continue;
                }

                listViewFields.Items.Add(new ListViewItem(validColumName));
            }

            if (listViewFields.Items.Count > 0)
            {
                btnSave.Enabled = true;
            }
        }

        private void btnRemoveAll_Click(object sender, EventArgs e)
        {
            
            foreach (ListViewItem listViewItem in listViewFields.Items)
            {
                listViewFields.Items.Remove(listViewItem);
            }

            if (listViewFields.Items.Count == 0)
            {
                btnSave.Enabled = false;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem listViewItem in listViewFields.Items)
            {
                SelectedColumns.Add(listViewItem.Text);
            }

            this.Close();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            SelectedColumns.Clear();
            this.Close();
        }
    }
}
