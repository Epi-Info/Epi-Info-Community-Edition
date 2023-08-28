#region Using
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Epi.Data;
#endregion // Using

namespace Epi.ImportExport
{
    /// <summary>
    /// A class used for exporting Epi Info form data to a CSV file.
    /// </summary>
    public class CSVExporter : ExporterBase
    {
        #region Private Members
        private readonly string fileName;        
        #endregion // Private Members

        public int rowsExported;

        public bool IncludeDeletedRecords { get; set; }

        #region Constants
        private const string SEPARATOR = StringLiterals.COMMA;
        #endregion // Constants

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="viewToExport">The data view to be exported.</param>
        /// <param name="columnList">The list of columns to be exported.</param>
        /// <param name="fileName">The name of the file that will contain the exported data.</param>
        public CSVExporter(DataView viewToExport, List<TableColumn> columnList, string fileName)
        {
            this.dataView = viewToExport;
            this.fileName = fileName;
            this.columnList = columnList;
            this.ColumnSortOrder = ImportExport.ColumnSortOrder.None;
            this.IncludeDeletedRecords = true;
        }

        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Gets the current file name and path that will contain the exported data.
        /// </summary>
        public string FileName
        {
            get
            {
                return this.fileName;
            }
        }
		#endregion // Public Properties

		#region Private Methods
		/// <summary>
		/// Initiates an export of the data to the specified file.
		/// </summary>
		private void ExportJSON()
		{
			DataView dv = this.DataView;
			DataTable table = new DataTable(); // dv.ToTable(false);

			if (IncludeDeletedRecords == false)
			{
				if (table.Columns.Contains("RecStatus") || table.Columns.Contains("RECSTATUS"))
				{
					dv.RowFilter = "[RecStatus] > 0";
				}
			}

			try
			{
				if (ColumnSortOrder != ImportExport.ColumnSortOrder.None || !exportAllFields)
				{
					table = dv.ToTable(false);

					if (view != null)
					{
						ImportExportHelper.OrderColumns(table, ColumnSortOrder, view);
					}
					else
					{
						ImportExportHelper.OrderColumns(table, ColumnSortOrder);
					}

					List<DataColumn> columnsToRemove = new List<DataColumn>();

					foreach (DataColumn dc in table.Columns)
					{
						bool found = false;
						foreach (Epi.Data.TableColumn tc in columnList)
						{
							if (tc.Name.Equals(dc.ColumnName))
							{
								found = true;
								break;
							}
						}

						if (!found)
						{
							columnsToRemove.Add(dc);
						}
					}

					foreach (DataColumn dc in columnsToRemove)
					{
						table.Columns.Remove(dc);
					}
				}
				rowsExported = 0;
				int totalRows = 0;
				
				totalRows = table.Rows.Count;
				StringBuilder jsb = new StringBuilder("[");
				foreach (DataRow row in table.Rows)
				{
					jsb.Append("\n    {");
					for (int i = 0; i < table.Columns.Count; i++)
					{
						string rowValue = row[i].ToString().Replace("\r\n", " ");
						if (rowValue.Contains(",") || rowValue.Contains("\""))
						{
							rowValue = rowValue.Replace("\"", "\"\"");
							rowValue = Util.InsertIn(rowValue, "\"");
						}
						jsb.Append("\n        \"" + table.Columns[i].ColumnName + "\" : ");
						if (table.Columns[i].DataType.FullName.Equals("System.String") ||
							table.Columns[i].DataType.FullName.Equals("System.DateTime") ||
							table.Columns[i].DataType.FullName.Equals("System.Date") ||
							table.Columns[i].DataType.FullName.Equals("System.Time"))
						{
							jsb.Append("\"" + rowValue + "\",");
						}
						else if (table.Columns[i].DataType.FullName.Equals("System.Boolean"))
						{
							jsb.Append(rowValue.ToLower() + ",");
						}
						else
						{
							if (String.IsNullOrEmpty(rowValue))
								rowValue = "null";
							jsb.Append(rowValue + ",");
						}
					}
					jsb.Remove(jsb.Length - 1, 1);
					jsb.Append("\n    },");
					rowsExported++;
					if (rowsExported % 500 == 0)
					{
						OnSetStatusMessageAndProgressCount(string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS, rowsExported.ToString(), totalRows.ToString()), (double)rowsExported);
					}
				}
				jsb.Remove(jsb.Length - 1, 1);
				jsb.Append("\n]");
				String jsonstring = jsb.ToString();
				System.IO.File.WriteAllText(fileName, jsonstring);

				OnSetStatusMessage(string.Format(SharedStrings.DASHBOARD_EXPORT_SUCCESS, rowsExported.ToString()));
			}

			catch (Exception ex)
			{
				OnSetStatusMessage(ex.Message);
				//this.Dispatcher.BeginInvoke(new SetStatusDelegate(SetErrorMessage), ex.Message);
			}
			finally
			{
				//System.Diagnostics.Debug.Print("File I/O Export thread finished in " + stopWatch.Elapsed.ToString());
			}
        }
        private void ExportAndAppendJSON()
        {
            DataView dv = this.DataView;
            DataTable table = new DataTable(); // dv.ToTable(false);

            if (IncludeDeletedRecords == false)
            {
                if (table.Columns.Contains("RecStatus") || table.Columns.Contains("RECSTATUS"))
                {
                    dv.RowFilter = "[RecStatus] > 0";
                }
            }

            try
            {
                if (ColumnSortOrder != ImportExport.ColumnSortOrder.None || !exportAllFields)
                {
                    table = dv.ToTable(false);

                    if (view != null)
                    {
                        ImportExportHelper.OrderColumns(table, ColumnSortOrder, view);
                    }
                    else
                    {
                        ImportExportHelper.OrderColumns(table, ColumnSortOrder);
                    }

                    List<DataColumn> columnsToRemove = new List<DataColumn>();

                    foreach (DataColumn dc in table.Columns)
                    {
                        bool found = false;
                        foreach (Epi.Data.TableColumn tc in columnList)
                        {
                            if (tc.Name.Equals(dc.ColumnName))
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            columnsToRemove.Add(dc);
                        }
                    }

                    foreach (DataColumn dc in columnsToRemove)
                    {
                        table.Columns.Remove(dc);
                    }
                }
                rowsExported = 0;
                int totalRows = 0;

                totalRows = table.Rows.Count;
                StringBuilder jsb = new StringBuilder("[");
                foreach (DataRow row in table.Rows)
                {
                    jsb.Append("\n    {");
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        string rowValue = row[i].ToString().Replace("\r\n", " ");
                        if (rowValue.Contains(",") || rowValue.Contains("\""))
                        {
                            rowValue = rowValue.Replace("\"", "\"\"");
                            rowValue = Util.InsertIn(rowValue, "\"");
                        }
                        jsb.Append("\n        \"" + table.Columns[i].ColumnName + "\" : ");
                        if (table.Columns[i].DataType.FullName.Equals("System.String") ||
                            table.Columns[i].DataType.FullName.Equals("System.DateTime") ||
                            table.Columns[i].DataType.FullName.Equals("System.Date") ||
                            table.Columns[i].DataType.FullName.Equals("System.Time"))
                        {
                            jsb.Append("\"" + rowValue + "\",");
                        }
                        else if (table.Columns[i].DataType.FullName.Equals("System.Boolean"))
                        {
                            jsb.Append(rowValue.ToLower() + ",");
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(rowValue))
                                rowValue = "null";
                            jsb.Append(rowValue + ",");
                        }
                    }
                    jsb.Remove(jsb.Length - 1, 1);
                    jsb.Append("\n    },");
                    rowsExported++;
                    if (rowsExported % 500 == 0)
                    {
                        OnSetStatusMessageAndProgressCount(string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS, rowsExported.ToString(), totalRows.ToString()), (double)rowsExported);
                    }
                }
                jsb.Remove(jsb.Length - 1, 1);
                jsb.Append("\n]");
                String jsonstring = jsb.ToString();
                if (File.Exists(fileName))
                {
                    string oldjsonstring = File.ReadAllText(fileName).Replace("}\n]", "},");
                    System.IO.File.WriteAllText(fileName, oldjsonstring + jsonstring.Substring(1));
                }
                else
                    System.IO.File.WriteAllText(fileName, jsonstring);

                OnSetStatusMessage(string.Format(SharedStrings.DASHBOARD_EXPORT_SUCCESS, rowsExported.ToString()));
            }

            catch (Exception ex)
            {
                OnSetStatusMessage(ex.Message);
                //this.Dispatcher.BeginInvoke(new SetStatusDelegate(SetErrorMessage), ex.Message);
            }
            finally
            {
                //System.Diagnostics.Debug.Print("File I/O Export thread finished in " + stopWatch.Elapsed.ToString());
            }
        }

        #endregion // Private Methods

        #region Public Methods
        /// <summary>
        /// Initiates an export of the data to the specified file.
        /// </summary>
        public override void Export()
        {
			if (fileName.ToLowerInvariant().EndsWith(".json"))
			{
				this.ExportJSON();
				return;
			}
            DataView dv = this.DataView;
            DataTable table = new DataTable(); // dv.ToTable(false);
            WordBuilder wb = new WordBuilder(SEPARATOR);
            StreamWriter sw = null;

            if (IncludeDeletedRecords == false)
            {
                if (table.Columns.Contains("RecStatus") || table.Columns.Contains("RECSTATUS"))
                {
                    dv.RowFilter = "[RecStatus] > 0";
                }
            }

            try
            {
                sw = File.CreateText(fileName);

                if (ColumnSortOrder != ImportExport.ColumnSortOrder.None || !exportAllFields)
                {
                    table = dv.ToTable(false);

                    if (view != null)
                    {
                        ImportExportHelper.OrderColumns(table, ColumnSortOrder, view);
                    }
                    else
                    {
                        ImportExportHelper.OrderColumns(table, ColumnSortOrder);
                    }                    

                    List<DataColumn> columnsToRemove = new List<DataColumn>();

                    foreach (DataColumn dc in table.Columns)
                    {
                        bool found = false;
                        foreach (Epi.Data.TableColumn tc in columnList)
                        {
                            if (tc.Name.Equals(dc.ColumnName))
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            columnsToRemove.Add(dc);
                        }
                    }

                    foreach (DataColumn dc in columnsToRemove)
                    {
                        table.Columns.Remove(dc);
                    }
                }

                foreach (DataColumn dc in table.Columns)
                {
                    wb.Add(dc.ColumnName);
                }

                sw.WriteLine(wb.ToString());
                rowsExported = 0;
                int totalRows = 0;

                //if (useTabOrder || !exportAllFields)
                //{
                    totalRows = table.Rows.Count;
                    foreach (DataRow row in table.Rows)
                    {
                        wb = new WordBuilder(SEPARATOR);
                        for (int i = 0; i < table.Columns.Count; i++)
                        {
                            string rowValue = row[i].ToString().Replace("\r\n", " ");
                            if (rowValue.Contains(",") || rowValue.Contains("\""))
                            {
                                rowValue = rowValue.Replace("\"", "\"\"");
                                rowValue = Util.InsertIn(rowValue, "\"");
                            }
                            wb.Add(rowValue);
                        }
                        sw.WriteLine(wb);
                        rowsExported++;
                        if (rowsExported % 500 == 0)
                        {
                            //this.Dispatcher.BeginInvoke(new SetGadgetStatusHandler(RequestUpdateStatusMessage), string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS, rowsExported.ToString(), totalRows.ToString()), (double)rowsExported);
                            //RequestUpdateStatusMessage(string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS, rowsExported.ToString(), totalRows.ToString()), (double)rowsExported);
                            //SetProgressAndStatus(string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS, rowsExported.ToString(), totalRows.ToString()), (double)rowsExported);
                            OnSetStatusMessageAndProgressCount(string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS, rowsExported.ToString(), totalRows.ToString()), (double)rowsExported);
                        }
                    }
                //}
                //else
                //{
                //    totalRows = dv.Count;
                //    foreach (DataRowView rowView in dv)
                //    {
                //        wb = new WordBuilder(SEPARATOR);
                //        for (int i = 0; i < table.Columns.Count; i++)
                //        {
                //            DataRow row = rowView.Row;
                //            string rowValue = row[i].ToString().Replace("\r\n", " ");
                //            if (rowValue.Contains(",") || rowValue.Contains("\""))
                //            {
                //                rowValue = rowValue.Replace("\"", "\"\"");
                //                rowValue = Util.InsertIn(rowValue, "\"");
                //            }
                //            wb.Add(rowValue);
                //        }
                //        sw.WriteLine(wb);
                //        rowsExported++;
                //        if (rowsExported % 500 == 0)
                //        {
                //            //this.Dispatcher.BeginInvoke(new SetGadgetStatusHandler(RequestUpdateStatusMessage), string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS, rowsExported.ToString(), totalRows.ToString()), (double)rowsExported);
                //            //RequestUpdateStatusMessage(string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS, rowsExported.ToString(), totalRows.ToString()), (double)rowsExported);
                //            SetProgressAndStatus(string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS, rowsExported.ToString(), totalRows.ToString()), (double)rowsExported);
                //        }
                //    }
                //}

                //this.Dispatcher.BeginInvoke(new SetStatusDelegate(SetStatusMessage), string.Format(SharedStrings.DASHBOARD_EXPORT_SUCCESS, rowsExported.ToString()));                
                OnSetStatusMessage(string.Format(SharedStrings.DASHBOARD_EXPORT_SUCCESS, rowsExported.ToString()));
            }

            catch (Exception ex)
            {
                OnSetStatusMessage(ex.Message);
                //this.Dispatcher.BeginInvoke(new SetStatusDelegate(SetErrorMessage), ex.Message);
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                    sw.Dispose();
                    sw = null;
                }                

                //stopWatch.Stop();
                //System.Diagnostics.Debug.Print("File I/O Export thread finished in " + stopWatch.Elapsed.ToString());
            }
        }
        public void ExportAndAppend()
        {
            if (fileName.ToLowerInvariant().EndsWith(".json"))
            {
                this.ExportAndAppendJSON();
                return;
            }
            DataView dv = this.DataView;
            DataTable table = new DataTable(); // dv.ToTable(false);
            WordBuilder wb = new WordBuilder(SEPARATOR);
            StreamWriter sw = null;

            if (IncludeDeletedRecords == false)
            {
                if (table.Columns.Contains("RecStatus") || table.Columns.Contains("RECSTATUS"))
                {
                    dv.RowFilter = "[RecStatus] > 0";
                }
            }

            try
            {
                bool makinganewfile = false;
                if (!File.Exists(fileName))
                {
                    makinganewfile= true;
                    sw = File.CreateText(fileName);
                }
                else
                    sw = File.AppendText(fileName);

                if (ColumnSortOrder != ImportExport.ColumnSortOrder.None || !exportAllFields)
                {
                    table = dv.ToTable(false);

                    if (view != null)
                    {
                        ImportExportHelper.OrderColumns(table, ColumnSortOrder, view);
                    }
                    else
                    {
                        ImportExportHelper.OrderColumns(table, ColumnSortOrder);
                    }

                    List<DataColumn> columnsToRemove = new List<DataColumn>();

                    foreach (DataColumn dc in table.Columns)
                    {
                        bool found = false;
                        foreach (Epi.Data.TableColumn tc in columnList)
                        {
                            if (tc.Name.Equals(dc.ColumnName))
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            columnsToRemove.Add(dc);
                        }
                    }

                    foreach (DataColumn dc in columnsToRemove)
                    {
                        table.Columns.Remove(dc);
                    }
                }

                foreach (DataColumn dc in table.Columns)
                {
                    wb.Add(dc.ColumnName);
                }

                if (makinganewfile)
                    sw.WriteLine(wb.ToString());
                rowsExported = 0;
                int totalRows = 0;

                //if (useTabOrder || !exportAllFields)
                //{
                totalRows = table.Rows.Count;
                foreach (DataRow row in table.Rows)
                {
                    wb = new WordBuilder(SEPARATOR);
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        string rowValue = row[i].ToString().Replace("\r\n", " ");
                        if (rowValue.Contains(",") || rowValue.Contains("\""))
                        {
                            rowValue = rowValue.Replace("\"", "\"\"");
                            rowValue = Util.InsertIn(rowValue, "\"");
                        }
                        wb.Add(rowValue);
                    }
                    sw.WriteLine(wb);
                    rowsExported++;
                    if (rowsExported % 500 == 0)
                    {
                        //this.Dispatcher.BeginInvoke(new SetGadgetStatusHandler(RequestUpdateStatusMessage), string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS, rowsExported.ToString(), totalRows.ToString()), (double)rowsExported);
                        //RequestUpdateStatusMessage(string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS, rowsExported.ToString(), totalRows.ToString()), (double)rowsExported);
                        //SetProgressAndStatus(string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS, rowsExported.ToString(), totalRows.ToString()), (double)rowsExported);
                        OnSetStatusMessageAndProgressCount(string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS, rowsExported.ToString(), totalRows.ToString()), (double)rowsExported);
                    }
                }
                //}
                //else
                //{
                //    totalRows = dv.Count;
                //    foreach (DataRowView rowView in dv)
                //    {
                //        wb = new WordBuilder(SEPARATOR);
                //        for (int i = 0; i < table.Columns.Count; i++)
                //        {
                //            DataRow row = rowView.Row;
                //            string rowValue = row[i].ToString().Replace("\r\n", " ");
                //            if (rowValue.Contains(",") || rowValue.Contains("\""))
                //            {
                //                rowValue = rowValue.Replace("\"", "\"\"");
                //                rowValue = Util.InsertIn(rowValue, "\"");
                //            }
                //            wb.Add(rowValue);
                //        }
                //        sw.WriteLine(wb);
                //        rowsExported++;
                //        if (rowsExported % 500 == 0)
                //        {
                //            //this.Dispatcher.BeginInvoke(new SetGadgetStatusHandler(RequestUpdateStatusMessage), string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS, rowsExported.ToString(), totalRows.ToString()), (double)rowsExported);
                //            //RequestUpdateStatusMessage(string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS, rowsExported.ToString(), totalRows.ToString()), (double)rowsExported);
                //            SetProgressAndStatus(string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS, rowsExported.ToString(), totalRows.ToString()), (double)rowsExported);
                //        }
                //    }
                //}

                //this.Dispatcher.BeginInvoke(new SetStatusDelegate(SetStatusMessage), string.Format(SharedStrings.DASHBOARD_EXPORT_SUCCESS, rowsExported.ToString()));                
                OnSetStatusMessage(string.Format(SharedStrings.DASHBOARD_EXPORT_SUCCESS, rowsExported.ToString()));
            }

            catch (Exception ex)
            {
                OnSetStatusMessage(ex.Message);
                //this.Dispatcher.BeginInvoke(new SetStatusDelegate(SetErrorMessage), ex.Message);
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                    sw.Dispose();
                    sw = null;
                }

                //stopWatch.Stop();
                //System.Diagnostics.Debug.Print("File I/O Export thread finished in " + stopWatch.Elapsed.ToString());
            }
        }

        #endregion // Public Methods
    }
}
