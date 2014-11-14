#region Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Collections;
using Epi.Windows.Docking;
using Epi;
using Epi.Fields;

#endregion  //Namespaces

using Epi.Windows.Dialogs;

namespace Epi.Windows.MakeView.Forms
{
    /// <summary>
    /// The Data Dictionary form
    /// </summary>
    public partial class DataDictionary : DialogBase
    {
        #region Private Data Members
        private new MainForm mainForm;
        private new View view;
        private Project project;
        private DataTable displayTable;
        private string sortExp = "[Page Position] ASC, [Tab Index] ASC";
        private string sortColumnName = "";
        #endregion  

        #region Constructors		 
	
        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="givenView">The View to load check code for</param>
        /// <param name="givenMainForm">The main form</param>
        public DataDictionary(View givenView, MainForm givenMainForm)
        {
            view = givenView;
            project = view.Project;
            mainForm = givenMainForm;
            Construct();
        }

        private void Construct()
        {             
            InitializeComponent();
            DockManager.FastMoveDraw = false;
            DockManager.Style = DockVisualStyle.VS2005;

            Project project = view.GetProject();
            Epi.Collections.ViewCollection views = project.Views;

            viewSelect.Items.AddRange(views.Names.ToArray());

            DataSets.TableSchema.TablesDataTable tables = project.Metadata.GetCodeTableList();

            foreach (DataRow row in tables)
            {
                viewSelect.Items.Add((string)row[ColumnNames.TABLE_NAME]);
            }

            viewSelect.SelectedItem = view.Name;

            viewSelect_SelectedIndexChanged(viewSelect, new EventArgs());
        }

        #endregion  //Constructors

        #region Event Handlers
        private void btnAsHtml_Click(object sender, EventArgs e)
        {
            string filePath = System.IO.Path.GetTempPath() + "DataDictionary_" + Guid.NewGuid().ToString("N") + ".html"; 
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            System.IO.TextWriter textWriter = System.IO.File.CreateText(filePath);
            textWriter.WriteLine("<html xmlns=\"http://www.w3.org/1999/xhtml\"><head><title></title>");

            textWriter.WriteLine("<style type=\"text/css\">");
            textWriter.WriteLine("table.dataDictionary { border-width: 1px; border-collapse: collapse; border-color: black; width: 100%;}");
            textWriter.WriteLine("table.dataDictionary th { border-width: 1px; border-style: solid; padding: 4px; border-color: black; font-family: Verdana; font-size: x-small; font-weight:bold; background-color:#4a7ac9; color:#FFFFFF;}");
            textWriter.WriteLine("table.dataDictionary td { border-width: 1px; border-style: solid; padding: 4px; border-color: black; font-family: Verdana; font-size: x-small; font-weight:normal;}");
            textWriter.WriteLine(".styleTitle { font-family: \"Verdana\"; font-size: x-small; font-weight: normal; }");
            textWriter.WriteLine("</style>");

            textWriter.WriteLine("<br/>");
            if (displayTable.TableName.StartsWith("code") == false)
            {
                textWriter.WriteLine(string.Format("<span class=\"styleTitle\"><strong>Data Dictionary for View [ {0} ]</strong></span><br />", view.Name));
                textWriter.WriteLine(string.Format("<span class=\"styleTitle\">{0}</span><br />", view.FullName));
            }
            textWriter.WriteLine(string.Format("<span class=\"styleTitle\">{0}</span><br />", DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString()));
            textWriter.WriteLine("<br/>");
   
            textWriter.WriteLine("<table class=\"dataDictionary\">");

            if (displayTable.TableName.StartsWith("code") == false)
            {
                textWriter.WriteLine("<tr><th>Page Position</th><th>Page Name</th><th>Tab Index</th><th>Prompt</th><th>Field Type</th><th>Name</th><th>Variable Type</th><th>Format</th><th>Special Info</th></tr>");
                bool highlightRow = false;
                DataView dataView = new DataView(displayTable);
                dataView.Sort = sortExp;

                DataTable sortedTable = dataView.ToTable();

                foreach (DataRow row in sortedTable.Rows)
                {
                    textWriter.WriteLine(string.Format("<tr style=\"background-color: #{0}\">", highlightRow ? "EEEEEE" : "FFFFFF"));
                    textWriter.WriteLine(string.Format("<td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td><td>{7}</td><td>{8}</td>",
                        row[0].ToString(),
                        row[1].ToString(),
                        row[2].ToString(),
                        row[3].ToString(),
                        row[4].ToString(),
                        row[5].ToString(),
                        row[6].ToString(),
                        row[7].ToString(),
                        row[8].ToString()));
                    textWriter.WriteLine("</tr>");
                    highlightRow = !highlightRow;
                }
            }
            else
            {
                bool highlightRow = false;
                DataView dataView = new DataView(displayTable);
                DataTable table = dataView.ToTable();
                textWriter.WriteLine("<tr>");

                foreach (DataColumn column in table.Columns)
                {
                    textWriter.WriteLine("<th>{0}</th>", column.ColumnName);
                }

                foreach (DataRow row in table.Rows)
                {
                    textWriter.WriteLine(string.Format("<tr style=\"background-color: #{0}\">", highlightRow ? "EEEEEE" : "FFFFFF"));
                    
                    object[] itemArray = row.ItemArray;

                    foreach (object obj in itemArray)
                    {
                        textWriter.WriteLine(string.Format("<td>{0}</td>", (string)obj));
                    }
                    
                    textWriter.WriteLine("</tr>");
                    highlightRow = !highlightRow;
                }
            }

            textWriter.WriteLine("</table></body></html>");
            textWriter.Close();

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.EnableRaisingEvents = false;
            proc.StartInfo.FileName = filePath;
            proc.Start();
        }

        void dataGridView_ColumnHeaderMouseClick(object sender, System.Windows.Forms.DataGridViewCellMouseEventArgs e)
        {
            if (dataGridView.SortedColumn != null)
            {
                sortColumnName = dataGridView.SortedColumn.Name;

                string sortOrder = "ASC";

                if (dataGridView.SortOrder == System.Windows.Forms.SortOrder.Descending)
                {
                    sortOrder = "DESC";
                }

                sortExp = string.Format("[{0}] {1}", sortColumnName, sortOrder).Trim();

                if (sortColumnName.Equals("Page Position"))
                {
                    sortExp += string.Format(",[Tab Index] {1}", sortColumnName, sortOrder).Trim();
                }
            }

            ((BindingSource)this.dataGridView.DataSource).Sort = sortExp;
        }

        void dataGridView_CellDoubleClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            int rowIndex = ((System.Windows.Forms.DataGridViewCellEventArgs)e).RowIndex;

            if (rowIndex >= 0)
            {
                DataGridViewRow row = ((DataGridView)sender).Rows[rowIndex];
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        private void viewSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (viewSelect.SelectedItem == null) return;

            if (project.Views.Contains((string)viewSelect.SelectedItem))
            {
                view = project.Views[(string)viewSelect.SelectedItem];

                displayTable = this.view.Project.Metadata.GetDataDictionary(this.view);

                foreach (DataRow dr in displayTable.Rows)
                {
                    if (dr["Special Info"] is string)
                    { 
                        string info = (string)dr["Special Info"];

                        if (info.Contains("||"))
                        {
                            int index = info.IndexOf("||");
                            dr["Special Info"] = info.Substring(0, index);
                        }
                        else
                        {
                            dr["Special Info"] = info;
                        }
                    }

                    if (dr["Variable Type"].ToString().ToLower() == "unknown")
                    {
                        dr["Variable Type"] = "Text";
                    }

                    if (dr["Field Type"].ToString().ToLower() == "option")
                    {
                        dr["Variable Type"] = "Number";
                    }
                    else if (dr["Field Type"].ToString().ToLower() == "grid")
                    {
                        dr["Variable Type"] = "Data Table";
                    }
                    if (dr["Field Type"].ToString().ToLower() == "image")
                    {
                        dr["Variable Type"] = "Image";

                         //--2365
                        Boolean bshouldretainImage = (Boolean) dr[Constants.SHOULDRETAINIMAGESIZE];
                        if (bshouldretainImage == true)
                        {
                            dr[Constants.SPECIALINFO] =  dr[Constants.SPECIALINFO].ToString() + Constants.VARRETAINIMAGESIZE;
                        }
                        //--
	                }
                    //--2365
                    Boolean bshouldRepeatLast = (Boolean)dr[ColumnNames.SHOULD_REPEAT_LAST];
                    if (bshouldRepeatLast == true)
                    {
                        dr[Constants.SPECIALINFO] = dr[Constants.SPECIALINFO] .ToString() + Constants.VARREPEATLAST;
                    }
                    Boolean bIsRequired = (Boolean)dr[ColumnNames.IS_REQUIRED];
                    if (bIsRequired == true)
                    {
                        dr[Constants.SPECIALINFO] = dr[Constants.SPECIALINFO] .ToString() + Constants.VARREQUIRED;
                    }
                    Boolean bIsReadOnly = (Boolean)dr[ColumnNames.IS_READ_ONLY];
                    if (bIsReadOnly == true)
                    {
                        dr[Constants.SPECIALINFO] = dr[Constants.SPECIALINFO] + Constants.VARREADONLY;
                    }
                    if (dr[ColumnNames.UPPER].ToString().Length > 0 && dr[ColumnNames.LOWER].ToString().Length > 0)
                   {
                       dr[Constants.SPECIALINFO] = dr[Constants.SPECIALINFO] + Constants.VARRANGE + CharLiterals.LEFT_SQUARE_BRACKET + dr[ColumnNames.LOWER].ToString() + CharLiterals.COMMA + dr[ColumnNames.UPPER].ToString() + CharLiterals.RIGHT_SQUARE_BRACKET;
                   }
                    //---- 
                }
                //-- 2365
                displayTable.Columns.Remove(ColumnNames.IS_REQUIRED);
                displayTable.Columns.Remove(ColumnNames.SHOULD_REPEAT_LAST);
                displayTable.Columns.Remove(ColumnNames.IS_READ_ONLY);
                displayTable.Columns.Remove(Constants.SHOULDRETAINIMAGESIZE);
                displayTable.Columns.Remove(ColumnNames.UPPER);
                displayTable.Columns.Remove(ColumnNames.LOWER);

                //--
               ((MakeViewMainForm)this.mainForm).EpiInterpreter.Context.ClearState();
                ((MakeViewMainForm)this.mainForm).EpiInterpreter.Execute(this.view.CheckCode);

                EpiInfo.Plugin.VariableScope variableScope =
                    EpiInfo.Plugin.VariableScope.Global |
                    EpiInfo.Plugin.VariableScope.Standard |
                    EpiInfo.Plugin.VariableScope.Permanent;

                List<EpiInfo.Plugin.IVariable> vars = new List<EpiInfo.Plugin.IVariable>();
                if (((MakeViewMainForm)this.mainForm).EpiInterpreter != null)
                {
                    vars = ((MakeViewMainForm)this.mainForm).EpiInterpreter.Context.GetVariablesInScope(variableScope);
                }

                DataRow row;

                foreach (EpiInfo.Plugin.IVariable var in vars)
                {
                    if (!(var is Epi.Fields.PredefinedDataField))
                    {
                        row = displayTable.NewRow();
                        row["Name"] = var.Name.ToString();
                        row["Field Type"] = var.VariableScope.ToString();
                        row["Variable Type"] = var.DataType.ToString();
                        displayTable.Rows.Add(row);
                    }
                }

                BindingSource source = new BindingSource();
                source.DataSource = displayTable;
                source.Sort = sortExp;
                this.dataGridView.DataSource = source;
            }
            else
            {
                displayTable = this.view.Project.GetCodeTableData((string)viewSelect.SelectedItem);
                BindingSource source = new BindingSource();
                source.DataSource = displayTable;
                this.dataGridView.DataSource = source;
            }
        }
    }
}