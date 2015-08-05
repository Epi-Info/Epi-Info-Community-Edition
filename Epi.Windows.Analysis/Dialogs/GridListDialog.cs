using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Epi.Analysis.Dialogs
{
    public partial class GridListDialog : Form
    {
        private List<DataRow> dataSource;
        private List<string> identifierList;
        private Epi.View epiView;


        public GridListDialog()
        {
            InitializeComponent();
        }

        public List<DataRow> DataSource
        {
            set
            {
                dataSource = value;
            }
        }

        public List<string> IdentifierList
        {
            set
            {
                identifierList = value;
            }
        }

        public Epi.View EpiView
        {
            set
            {
                epiView = value;
            }
        }

        private void GridListDialog_Load(object sender, EventArgs e)
        {
            DataTable displayTable = new DataTable();
            int maxColumnCount = 512;

            if (dataSource.Count > 0)
            {
                displayTable = dataSource.CopyToDataTable();
                List<string> remove = new List<string>();
                int count = 0; 
                    
                foreach (DataColumn dataColumn in displayTable.Columns)
                {
                    count++;

                    if (identifierList.Contains(dataColumn.ColumnName.ToUpper()) == false || count > maxColumnCount)
                    {
                        remove.Add(dataColumn.ColumnName);
                    }
                }

                foreach (string rmvthsclmn in remove)
                {
                    displayTable.Columns.Remove(rmvthsclmn);
                }

                dataGridView1.DataSource = displayTable;
            }

            int displayIndex = 0;

            if (dataGridView1.Columns.Count > 0)
            { 
                foreach (string idendifier in identifierList)
                {
                    dataGridView1.Columns[idendifier].DisplayIndex = displayIndex++; 
                }
            }

            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                foreach (string identifier in identifierList)
                {
                    if (((DataTable)dataGridView1.DataSource).Columns[column.Name].DataType.ToString().Equals("System.Double")
                        || ((DataTable)dataGridView1.DataSource).Columns[column.Name].DataType.ToString().Equals("System.Single")
                        || ((DataTable)dataGridView1.DataSource).Columns[column.Name].DataType.ToString().Equals("System.Decimal")
                        || ((DataTable)dataGridView1.DataSource).Columns[column.Name].DataType.ToString().Equals("System.Int32")
                        || ((DataTable)dataGridView1.DataSource).Columns[column.Name].DataType.ToString().Equals("System.Int64")
                        || ((DataTable)dataGridView1.DataSource).Columns[column.Name].DataType.ToString().Equals("System.Int16")
                        || ((DataTable)dataGridView1.DataSource).Columns[column.Name].DataType.ToString().Equals("System.Byte")
                    )
                    {
                        DataGridViewCellStyle style = new DataGridViewCellStyle();
                        style.Alignment = DataGridViewContentAlignment.MiddleRight;
                        column.DefaultCellStyle = style;
                    }  

                    string currentColumn = column.Name.ToString();

                    if (this.dataGridView1.Columns[column.Name].ValueType == typeof(System.DateTime))
                    {
                        this.dataGridView1.Columns[column.Name].DefaultCellStyle.Format = "g";
                    }

                    if (epiView != null)
                    {
                        if (epiView.Fields.TableColumnFields.Contains(currentColumn))
                         {
                            switch(epiView.Fields.TableColumnFields[column.Name].FieldType.ToString().ToUpper())
                            {
                                case "TIME":
                                    this.dataGridView1.Columns[column.Name].DefaultCellStyle.Format = "t";
                                break;
                                case "DATETIME":
                                    this.dataGridView1.Columns[column.Name].DefaultCellStyle.Format = "g";
                                break;
                                case "DATE":
                                    this.dataGridView1.Columns[column.Name].DefaultCellStyle.Format = "d";
                                break;
                            }
                         }
                    }
                }
            }
        }


        public void Validate()
        {
            DataTable displayTable = new DataTable();
            int maxColumnCount = 512;

            if (dataSource.Count > 0)
            {
                displayTable = dataSource.CopyToDataTable();
                List<string> remove = new List<string>();
                int count = 0;

                foreach (DataColumn dataColumn in displayTable.Columns)
                {
                    count++;

                    if (identifierList.Contains(dataColumn.ColumnName.ToUpper()) == false || count > maxColumnCount)
                    {
                        remove.Add(dataColumn.ColumnName);
                    }
                }

                foreach (string rmvthsclmn in remove)
                {
                    displayTable.Columns.Remove(rmvthsclmn);
                }

                dataGridView1.DataSource = displayTable;
            }

            int displayIndex = 0;
            try
            {
                if (dataGridView1.Columns.Count > 0)
                {
                    foreach (string idendifier in identifierList)
                    {
                        dataGridView1.Columns[idendifier].DisplayIndex = displayIndex++;
                    }
                }

                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    foreach (string identifier in identifierList)
                    {
                        if (((DataTable)dataGridView1.DataSource).Columns[column.Name].DataType.ToString().Equals("System.Double")
                            || ((DataTable)dataGridView1.DataSource).Columns[column.Name].DataType.ToString().Equals("System.Single")
                            || ((DataTable)dataGridView1.DataSource).Columns[column.Name].DataType.ToString().Equals("System.Decimal")
                            || ((DataTable)dataGridView1.DataSource).Columns[column.Name].DataType.ToString().Equals("System.Int32")
                            || ((DataTable)dataGridView1.DataSource).Columns[column.Name].DataType.ToString().Equals("System.Int64")
                            || ((DataTable)dataGridView1.DataSource).Columns[column.Name].DataType.ToString().Equals("System.Int16")
                            || ((DataTable)dataGridView1.DataSource).Columns[column.Name].DataType.ToString().Equals("System.Byte")
                        )
                        {
                            DataGridViewCellStyle style = new DataGridViewCellStyle();
                            style.Alignment = DataGridViewContentAlignment.MiddleRight;
                            column.DefaultCellStyle = style;
                        }

                        string currentColumn = column.Name.ToString();

                        if (this.dataGridView1.Columns[column.Name].ValueType == typeof(System.DateTime))
                        {
                            this.dataGridView1.Columns[column.Name].DefaultCellStyle.Format = "g";
                        }

                        if (epiView != null)
                        {
                            if (epiView.Fields.TableColumnFields.Contains(currentColumn))
                            {
                                switch (epiView.Fields.TableColumnFields[column.Name].FieldType.ToString().ToUpper())
                                {
                                    case "TIME":
                                        this.dataGridView1.Columns[column.Name].DefaultCellStyle.Format = "t";
                                        break;
                                    case "DATETIME":
                                        this.dataGridView1.Columns[column.Name].DefaultCellStyle.Format = "g";
                                        break;
                                    case "DATE":
                                        this.dataGridView1.Columns[column.Name].DefaultCellStyle.Format = "d";
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Please Update the List Variables");
            }
        }      

    }
}
