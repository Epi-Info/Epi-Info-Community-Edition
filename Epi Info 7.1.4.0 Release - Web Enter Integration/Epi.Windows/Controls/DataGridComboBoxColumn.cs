using System;
using System.Windows.Forms;
using System.Drawing;
using System.Data;

namespace Epi.Windows.Controls
{

    /// <summary>
    /// class DataGridComboBoxColumn
    /// </summary>
	public class DataGridComboBoxColumn : DataGridTextBoxColumn
	{
		
		#region Private Class Members
		private ComboBox comboBox;
		private CurrencyManager currencyManager;
		private int iCurrentRow;
		#endregion Private Class Members
		
		#region Constructor
    
        /// <summary>
        /// Constructor - create combobox
        /// </summary>
        /// <remarks>
        /// register selection change event handler,
        /// register lose focus event handler
        /// </remarks>
		public DataGridComboBoxColumn()
		{
			this.currencyManager = null;

			// Create combobox and force DropDownList style
			this.comboBox = new ComboBox();
			this.comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        
			// Add event handler for notification when combobox loses focus
			this.comboBox.Leave += new EventHandler(Control_Leave);
		}
		#endregion Constructor

		#region Public Properties

        /// <summary>
        /// Property to provide access to combobox 
        /// </summary>
		public ComboBox ComboBox
		{
			get 
			{ 
				return comboBox; 
			}
		}       
		#endregion Public Properties

		
        /// <summary>
        /// Method Edit()
        /// </summary>
        /// <remarks>
        /// On edit, add scroll event handler, and display combobox
        /// </remarks>
        /// <param name="source"></param>
        /// <param name="rowNum"></param>
        /// <param name="bounds"></param>
        /// <param name="readOnly"></param>
        /// <param name="instantText"></param>
        /// <param name="cellIsVisible"></param>
		protected override void Edit(System.Windows.Forms.CurrencyManager source, int rowNum, System.Drawing.Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			base.Edit(source, rowNum, bounds, readOnly, instantText,cellIsVisible);

			if (!readOnly && cellIsVisible)
			{
				// Save current row in the DataGrid and currency manager 
				// associated with the data source for the DataGrid
				this.iCurrentRow = rowNum;
				this.currencyManager = source;
    
				// Add event handler for DataGrid scroll notification
				this.DataGridTableStyle.DataGrid.Scroll += new EventHandler(DataGrid_Scroll);

				// Site the combobox control within the current cell
				this.comboBox.Parent = this.TextBox.Parent;
				Rectangle rect = this.DataGridTableStyle.DataGrid.GetCurrentCellBounds();
				this.comboBox.Location = rect.Location;
				this.comboBox.Size = new Size(this.TextBox.Size.Width,this.comboBox.Size.Height);

				// Set combobox selection to given text
				this.comboBox.SelectedIndex = this.comboBox.FindStringExact(this.TextBox.Text);

				// Make the combobox visible and place on top textbox control
				this.comboBox.Show();
				this.comboBox.BringToFront();
				this.comboBox.Focus();
			}
		}

        /// <summary>
        /// GetColumnValueAtRow
        /// </summary>
        /// <remarks>
        /// Given a row, get the value member associated with a row.  Use the
        /// value member to find the associated display member by iterating 
        /// over bound data source
        /// </remarks>
        /// <param name="source"></param>
        /// <param name="rowNum"></param>
        /// <returns></returns>
		protected override object GetColumnValueAtRow(System.Windows.Forms.CurrencyManager source,int rowNum)
		{
			// Given a row number in the DataGrid, get the display member
			object obj =  base.GetColumnValueAtRow(source, rowNum);
        
			// Iterate through the data source bound to the ColumnComboBox
			CurrencyManager currencyManager = (CurrencyManager)(this.DataGridTableStyle.DataGrid.BindingContext[this.comboBox.DataSource]);
			// Assumes the associated DataGrid is bound to a DataView or 
			// DataTable 
			DataView dataview = ((DataView)currencyManager.List);
                            
			int i;

			for (i = 0; i < dataview.Count; i++)
			{
				if (obj.Equals(dataview[i][this.comboBox.ValueMember]))
					break;
			}
        
			if (i < dataview.Count)
				return dataview[i][this.comboBox.DisplayMember];
        
			return DBNull.Value;
		}

        
		// Given a row and a display member, iterate over bound data source to 
		// find the associated value member.  Set this value member.
        /// <summary>
        /// SetColumnValueAtRow
        /// </summary>
        /// <param name="source"></param>
        /// <param name="rowNum"></param>
        /// <param name="value"></param>
		protected override void SetColumnValueAtRow(System.Windows.Forms.CurrencyManager source,int rowNum, object value)
		{
			object s = value;

			// Iterate through the data source bound to the ColumnComboBox
			CurrencyManager currencyManager = (CurrencyManager)(this.DataGridTableStyle.DataGrid.BindingContext[this.comboBox.DataSource]);
			// Assumes the associated DataGrid is bound to a DataView or DataTable 
			DataView dataview = ((DataView)currencyManager.List);
			int i;

			for (i = 0; i < dataview.Count; i++)
			{
				if (s.Equals(dataview[i][this.comboBox.DisplayMember]))
					break;
			}

			// If set item was found return corresponding value, 
			// otherwise return DbNull.Value
			if(i < dataview.Count)
				s =  dataview[i][this.comboBox.ValueMember];
			else
				s = DBNull.Value;
        
			base.SetColumnValueAtRow(source, rowNum, s);
		}

		// On DataGrid scroll, hide the combobox
		private void DataGrid_Scroll(object sender, EventArgs e)
		{
			this.comboBox.Hide();
		}

		// On combobox losing focus, set the column value, hide the combobox,
		// and unregister scroll event handler
		private void Control_Leave(object sender, EventArgs e)
		{
			DataRowView rowView = (DataRowView) this.comboBox.SelectedItem;
			string s = (string) rowView.Row[this.comboBox.DisplayMember];

			SetColumnValueAtRow(this.currencyManager, this.iCurrentRow, s);
			Invalidate();

			this.comboBox.Hide();
			this.DataGridTableStyle.DataGrid.Scroll -= 	new EventHandler(DataGrid_Scroll);            
		}
	}

}
