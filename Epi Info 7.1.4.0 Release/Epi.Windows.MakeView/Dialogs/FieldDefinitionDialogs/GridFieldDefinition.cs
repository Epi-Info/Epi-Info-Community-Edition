using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Fields;
using Epi.DataSets;

namespace Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs
{
    /// <summary>
    /// Field definition dialog for grid fields
    /// </summary>
    public partial class GridFieldDefinition : GenericFieldDefinition
    {
        #region Private Members

        private GridField field = null;
        private DataGridTableStyle tableStyle = null;
        private List<GridColumnBase> gridColumns = null;
        private DataGrid.HitTestInfo hti = null;

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor, for exclusive use by the designer.
        /// </summary>
        public GridFieldDefinition()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor of the Grid Field Definition
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="page">The current page</param>
        public GridFieldDefinition(MainForm frm, Page page)
            : base(frm)
        {
            InitializeComponent();
            this.mode = FormMode.Create;
            this.page = page;
        }

        /// <summary>
        /// Constructor of the Grid Field Definition
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="field">A grid field</param>
        public GridFieldDefinition(MainForm frm, GridField field)
            : base(frm)
        {
            InitializeComponent();
            this.mode = FormMode.Edit;
            this.field = field;
            this.page = field.Page;
            Configuration config = Configuration.GetNewInstance();
            FontStyle style = FontStyle.Regular;
            if (config.Settings.EditorFontBold)
            {
                style |= FontStyle.Bold;
            }
            if (config.Settings.EditorFontItalics)
            {
                style |= FontStyle.Italic;
            }
            if ((field.PromptFont == null) || ((field.PromptFont.Name == "Microsoft Sans Serif") && (field.PromptFont.Size == 8.5)))
            {
                field.PromptFont = new Font(config.Settings.EditorFontName, (float)config.Settings.EditorFontSize, style);
            }

            style = FontStyle.Regular;
            if (config.Settings.ControlFontBold)
            {
                style |= FontStyle.Bold;
            }
            if (config.Settings.ControlFontItalics)
            {
                style |= FontStyle.Italic;
            }
            
            field.ControlFont = new Font(config.Settings.ControlFontName, (float)config.Settings.ControlFontSize, style);

            txtPrompt.Font = field.PromptFont;
            promptFont = field.PromptFont;
            controlFont = field.PromptFont;
            field.ControlFont = field.PromptFont;
            dgColumns.Font = field.PromptFont;
            dgColumns.HeaderFont = field.PromptFont;

            if (Util.IsEmpty(gridColumns))
            {
                if (field.Columns != null)
                {
                    gridColumns = new List<GridColumnBase>(field.Columns);
                }
            }

            if (!this.field.Id.Equals(0))
            {
                LoadFormData();
            }
        }

        void delete_Click(object sender, EventArgs e)
        {
            DialogResult confirmDelete = MsgBox.ShowQuestion(SharedStrings.CONFIRM_DELETE_GRID_COLUMN, MessageBoxButtons.YesNo);
            if (confirmDelete.Equals(DialogResult.Yes))
            {
                string columnName = string.Empty;
                List<GridColumnBase> tempGrid = null;
                if (Util.IsEmpty(gridColumns))
                {
                    gridColumns = new List<GridColumnBase>(field.Columns);
                }

                if (!Util.IsEmpty(dgColumns.DataSource))
                {
                    columnName = ((DataTable)dgColumns.DataSource).Columns[hti.Column].ColumnName;
                    tempGrid = new List<GridColumnBase>(gridColumns);

                    foreach (GridColumnBase col in tempGrid)
                    {
                        if (col.Name.Equals(columnName))
                        {
                            gridColumns.Remove(col);
                            break;
                        }
                    }

                    DataGridColumnStyle[] temp = new DataGridColumnStyle[dgColumns.TableStyles["GridColumns"].GridColumnStyles.Count];
                    dgColumns.TableStyles["GridColumns"].GridColumnStyles.CopyTo(temp, 0);
                    List<DataGridColumnStyle> styles = new List<DataGridColumnStyle>(temp);
                    styles.RemoveAt(hti.Column);

                    dgColumns.TableStyles["GridColumns"].GridColumnStyles.Clear();
                    dgColumns.TableStyles["GridColumns"].GridColumnStyles.AddRange(styles.ToArray());

                    tempGrid = null;

                    if (dgColumns.DataSource is DataTable)
                    {
                        if (dgColumns.TableStyles["GridColumns"].GridColumnStyles.Count.Equals(0))
                        {
                            btnOk.Enabled = false;
                        }
                        else
                        {
                            btnOk.Enabled = true;
                        }
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        private void LoadFormData()
        {
            txtPrompt.Text = field.PromptText;
            txtFieldName.Text = field.Name;

            DataTable columnTable = new DataTable("GridColumns");

            tableStyle = new DataGridTableStyle();
            tableStyle.MappingName = columnTable.TableName;
            tableStyle.ReadOnly = true;
            tableStyle.DataGrid = dgColumns;
            tableStyle.RowHeadersVisible = false;

            List<GridColumnBase> cols = new List<GridColumnBase>(field.Columns);

            cols.Sort(Util.SortByPosition);

            foreach (GridColumnBase gridColumn in cols)
            {
                if (!(gridColumn is PredefinedColumn))
                {
                    DataColumn dc = new DataColumn(gridColumn.Name);                    
                    columnTable.Columns.Add(dc);

                    DataGridTextBoxColumn textBoxColumn = new DataGridTextBoxColumn();
                    textBoxColumn.MappingName = gridColumn.Name;
                    textBoxColumn.HeaderText = gridColumn.Text;
                    textBoxColumn.Width = (gridColumn.Width > 0 ? gridColumn.Width:75);
                    textBoxColumn.WidthChanged += new EventHandler(textColumn_WidthChanged);
                    tableStyle.GridColumnStyles.Add(textBoxColumn);
                }
            }
            
            dgColumns.DataSource = columnTable;
            dgColumns.TableStyles.Clear();
            dgColumns.TableStyles.Add(tableStyle);

            if (Util.IsEmpty(field.Columns))
            {
                btnOk.Enabled = false;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the field's properties based on GUI values
        /// </summary>
        protected override void SetFieldProperties()
        {
            field.PromptText = txtPrompt.Text;
            field.Name = txtFieldName.Text;
            if (field.Columns == null)
            {
                field.Columns = new List<GridColumnBase>();
                //UniqueKeyColumn uniqueKeyColumn = new UniqueKeyColumn(field);
                UniqueRowIdColumn uniqueRowIdColumn = new UniqueRowIdColumn(field);
                RecStatusColumn recStatusColumn = new RecStatusColumn(field);
                ForeignKeyColumn foreignKeyColumn = new ForeignKeyColumn(field);
                GlobalRecordIdColumn globalRecordIdColumn = new GlobalRecordIdColumn(field);

                field.Columns.AddRange (new List<GridColumnBase> { 
                    //uniqueKeyColumn, 
                    uniqueRowIdColumn,
                    recStatusColumn, 
                    foreignKeyColumn, 
                    globalRecordIdColumn 
                });
            }
        }

        /// <summary>
        /// Gets the field defined by this field definition dialog
        /// </summary>
        public override RenderableField Field
        {
            get
            {
                return field;
            }
        }
        #endregion	

        #region Event Handlers

        /// <summary>
        /// Shows the GridColumnsDialog
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!txtFieldName.TextLength.Equals(0))
            {
                SetFieldProperties();

                List<GridColumnBase> tempColumns;
                
                if (Util.IsEmpty(gridColumns))
                {
                    tempColumns = new List<GridColumnBase>(field.Columns);
                }
                else
                {
                    tempColumns = new List<GridColumnBase>(gridColumns);
                }

                DataView patternsDataView = field.GetMetadata().GetPatterns().DefaultView;

                GridColumnsDialog gridColumnsDialog = new GridColumnsDialog
                    (
                        this.mainForm,
                        page,
                        field,
                        patternsDataView,
                        mode,
                        tempColumns
                    );

                gridColumnsDialog.ShowDialog();

                if (gridColumnsDialog.DialogResult == DialogResult.OK)
                {
                    tableStyle = new DataGridTableStyle();
                    tableStyle.MappingName = "GridColumns";
                    tableStyle.RowHeadersVisible = false;
                    tableStyle.ReadOnly = true;

                    field = gridColumnsDialog.Grid;

                    DataTable columnTable = new DataTable("GridColumns");

                    tempColumns.Sort(Util.SortByPosition);

                    foreach (GridColumnBase gridColumn in tempColumns)
                    {
                        if (!(gridColumn is PredefinedColumn))
                        {
                            DataColumn dc = new DataColumn(gridColumn.Name);
                            columnTable.Columns.Add(dc);

                            DataGridTextBoxColumn textBoxColumn = new DataGridTextBoxColumn();

                            textBoxColumn.MappingName = gridColumn.Name;
                            textBoxColumn.HeaderText = gridColumn.Text;
                            textBoxColumn.Width = gridColumn.Width;
                            textBoxColumn.WidthChanged += new EventHandler(textColumn_WidthChanged);

                            tableStyle.GridColumnStyles.Add(textBoxColumn);
                        }
                    }

                    dgColumns.DataSource = columnTable;
                    dgColumns.TableStyles.Clear();
                    dgColumns.TableStyles.Add(tableStyle);
                    gridColumns = tempColumns;
                    btnOk.Enabled = true;
                }
                else
                {
                    if (Util.IsEmpty(gridColumns))
                    {
                        tempColumns = null;
                        btnOk.Enabled = false;
                    }
                }
            }
        }

        /// <summary>
        /// Saves any changes made to the field. If it is not a new field,
        /// it checks to see if any fields have been deleted or modified 
        /// and removes them from the database before updating with the new columns.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnOk_Click(object sender, EventArgs e)
        {
            if (!Util.IsEmpty(gridColumns))
            {
                if (!field.Id.Equals(0))
                {
                    List<GridColumnBase> baseCols = new List<GridColumnBase>(field.Columns);
                    foreach (GridColumnBase cols in baseCols)
                    {
                        if (!gridColumns.Contains(cols))
                        {
                            field.Columns[field.Columns.IndexOf(cols)].DeleteFromDb();
                        }
                    }
                }
                field.Columns = gridColumns;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// Event to set control state after the form has loaded
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void GridFieldDefinition_Load(object sender, EventArgs e)
        {
            // DEFECT# 476: The column Add button is disabled until the user enters some Prompt text.
            if (!string.IsNullOrEmpty(txtPrompt.Text))
            {
                btnAdd.Enabled = true;
            }
            else
            {
                btnAdd.Enabled = false;
            }

            if (dgColumns.VisibleColumnCount > 0)
            {
                btnAdd.Text = "Add/Edit";
            }
            else
            {
                btnAdd.Text = "Add";
            }
        }

        /// <summary>
        /// Update the column width of the field after the width has changed on the grid column
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void textColumn_WidthChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < field.Columns.Count; i++)
            {
                if (((DataGridTextBoxColumn)sender).MappingName == field.Columns[i].Name)
                {
                    field.Columns[i].Width = ((DataGridTextBoxColumn)sender).Width;
                    break;
                }
            }
        }

        /// <summary>
        /// Set control state after the Grid Name has been entered
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtPrompt_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPrompt.Text))
            {
                btnAdd.Enabled = true;
            }
            else
            {
                btnAdd.Enabled = false;
            }

            if (Util.IsEmpty(gridColumns))
            {
                btnOk.Enabled = false;
            }
        }


        private void txtFieldName_TextChanged(object sender, System.EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtFieldName.Text))
            {
                btnOk.Enabled = true;
            }
            else
            {
                btnOk.Enabled = false;
            }
            
            if (Util.IsEmpty(gridColumns))
            {
                btnOk.Enabled = false;
            }
            else
            {
                btnOk.Enabled = true;
            }
        }
        #endregion



        /// <summary>
        /// Handles the mouse down event of the columns grid
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event args</param>
        private void dgColumns_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            DataGrid gvCols = (DataGrid)sender;
            hti = gvCols.HitTest(e.X, e.Y);
            switch (hti.Type)
            {
                case DataGrid.HitTestType.ColumnHeader:
                    if (e.Button.Equals(MouseButtons.Right))
                    {
                        ContextMenu dgMenu = new ContextMenu();
                        MenuItem delete = new MenuItem(SharedStrings.DELETE);
                        delete.Click += new EventHandler(delete_Click);

                        dgMenu.MenuItems.Add(delete);

                        dgMenu.Show((Control)sender, e.Location);
                    }
                    break;
                case DataGrid.HitTestType.ColumnResize:
                    break;
                default:
                    break;
            }
        }
    }
}
