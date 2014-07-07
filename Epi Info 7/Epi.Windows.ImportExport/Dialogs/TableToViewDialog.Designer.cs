namespace Epi.ImportExport.Dialogs
{
    partial class TableToViewDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TableToViewDialog));
            this.btnSave = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.dgvFormFields = new System.Windows.Forms.DataGridView();
            this.textViewName = new System.Windows.Forms.TextBox();
            this.textTableName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnImportListValues = new System.Windows.Forms.Button();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn12 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn13 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn14 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn15 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn16 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn17 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnSetControlFont = new System.Windows.Forms.Button();
            this.btnSetPromptFont = new System.Windows.Forms.Button();
            this.Import = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.SourceColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DestinationColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Prompt = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SourceColumnType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FieldType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.PageNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TabIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IsTabStop = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.IsReadOnly = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.IsRequired = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.IsRepeatLast = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.HasRange = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.LowerBound = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UpperBound = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ListSourceTableName = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.ListSourceTextColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ListSourceTable = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ControlFont = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PromptFont = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ControlLeftPosition = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ControlTopPosition = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PromptLeftPosition = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PromptTopPosition = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFormFields)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSave.Location = new System.Drawing.Point(696, 462);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 23);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "&Convert";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.Location = new System.Drawing.Point(792, 462);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(90, 23);
            this.btnExit.TabIndex = 1;
            this.btnExit.Text = "&Cancel";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // dgvFormFields
            // 
            this.dgvFormFields.AllowUserToAddRows = false;
            this.dgvFormFields.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFormFields.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Import,
            this.SourceColumnName,
            this.DestinationColumnName,
            this.Prompt,
            this.SourceColumnType,
            this.FieldType,
            this.PageNumber,
            this.TabIndex,
            this.IsTabStop,
            this.IsReadOnly,
            this.IsRequired,
            this.IsRepeatLast,
            this.HasRange,
            this.LowerBound,
            this.UpperBound,
            this.ListSourceTableName,
            this.ListSourceTextColumnName,
            this.ListSourceTable,
            this.ControlFont,
            this.PromptFont,
            this.ControlLeftPosition,
            this.ControlTopPosition,
            this.PromptLeftPosition,
            this.PromptTopPosition});
            this.dgvFormFields.Location = new System.Drawing.Point(12, 125);
            this.dgvFormFields.MultiSelect = false;
            this.dgvFormFields.Name = "dgvFormFields";
            this.dgvFormFields.Size = new System.Drawing.Size(870, 331);
            this.dgvFormFields.TabIndex = 16;
            this.dgvFormFields.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvFormFields_CellEndEdit);
            // 
            // textViewName
            // 
            this.textViewName.Enabled = false;
            this.textViewName.Location = new System.Drawing.Point(15, 71);
            this.textViewName.Name = "textViewName";
            this.textViewName.Size = new System.Drawing.Size(197, 20);
            this.textViewName.TabIndex = 15;
            // 
            // textTableName
            // 
            this.textTableName.Enabled = false;
            this.textTableName.Location = new System.Drawing.Point(15, 29);
            this.textTableName.Name = "textTableName";
            this.textTableName.Size = new System.Drawing.Size(197, 20);
            this.textTableName.TabIndex = 14;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(200, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Form Name:";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(174, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Table Name:";
            // 
            // btnImportListValues
            // 
            this.btnImportListValues.Location = new System.Drawing.Point(232, 70);
            this.btnImportListValues.Name = "btnImportListValues";
            this.btnImportListValues.Size = new System.Drawing.Size(178, 23);
            this.btnImportListValues.TabIndex = 6;
            this.btnImportListValues.Text = "Add List Source Table";
            this.btnImportListValues.UseVisualStyleBackColor = true;
            this.btnImportListValues.Click += new System.EventHandler(this.btnImportListValues_Click);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "SourceColumnName";
            this.dataGridViewTextBoxColumn1.HeaderText = "Column Name";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.DataPropertyName = "DestinationColumnName";
            this.dataGridViewTextBoxColumn2.HeaderText = "Field Name";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.DataPropertyName = "Prompt";
            this.dataGridViewTextBoxColumn3.HeaderText = "Prompt";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.DataPropertyName = "SourceColumnType";
            this.dataGridViewTextBoxColumn4.HeaderText = "Column Type";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.DataPropertyName = "PageNumber";
            this.dataGridViewTextBoxColumn5.HeaderText = "Page";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.DataPropertyName = "TabIndex";
            this.dataGridViewTextBoxColumn6.HeaderText = "Tab";
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            // 
            // dataGridViewTextBoxColumn7
            // 
            this.dataGridViewTextBoxColumn7.DataPropertyName = "LowerBound";
            this.dataGridViewTextBoxColumn7.HeaderText = "Lower";
            this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
            // 
            // dataGridViewTextBoxColumn8
            // 
            this.dataGridViewTextBoxColumn8.DataPropertyName = "UpperBound";
            this.dataGridViewTextBoxColumn8.HeaderText = "Upper";
            this.dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
            // 
            // dataGridViewTextBoxColumn9
            // 
            this.dataGridViewTextBoxColumn9.DataPropertyName = "ListSourceTableName";
            this.dataGridViewTextBoxColumn9.HeaderText = "ListSourceTableName";
            this.dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
            this.dataGridViewTextBoxColumn9.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn10
            // 
            this.dataGridViewTextBoxColumn10.DataPropertyName = "ListSourceTextColumnName";
            this.dataGridViewTextBoxColumn10.HeaderText = "ListSourceTextColumnName";
            this.dataGridViewTextBoxColumn10.Name = "dataGridViewTextBoxColumn10";
            this.dataGridViewTextBoxColumn10.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn11
            // 
            this.dataGridViewTextBoxColumn11.DataPropertyName = "ListSourceTable";
            this.dataGridViewTextBoxColumn11.HeaderText = "ListSourceTable";
            this.dataGridViewTextBoxColumn11.Name = "dataGridViewTextBoxColumn11";
            // 
            // dataGridViewTextBoxColumn12
            // 
            this.dataGridViewTextBoxColumn12.DataPropertyName = "ControlFont";
            this.dataGridViewTextBoxColumn12.HeaderText = "ControlFont";
            this.dataGridViewTextBoxColumn12.Name = "dataGridViewTextBoxColumn12";
            // 
            // dataGridViewTextBoxColumn13
            // 
            this.dataGridViewTextBoxColumn13.DataPropertyName = "PromptFont";
            this.dataGridViewTextBoxColumn13.HeaderText = "PromptFont";
            this.dataGridViewTextBoxColumn13.Name = "dataGridViewTextBoxColumn13";
            // 
            // dataGridViewTextBoxColumn14
            // 
            this.dataGridViewTextBoxColumn14.DataPropertyName = "ControlLeftPosition";
            this.dataGridViewTextBoxColumn14.HeaderText = "ControlLeftPosition";
            this.dataGridViewTextBoxColumn14.Name = "dataGridViewTextBoxColumn14";
            // 
            // dataGridViewTextBoxColumn15
            // 
            this.dataGridViewTextBoxColumn15.DataPropertyName = "ControlTopPosition";
            this.dataGridViewTextBoxColumn15.HeaderText = "ControlTopPosition";
            this.dataGridViewTextBoxColumn15.Name = "dataGridViewTextBoxColumn15";
            // 
            // dataGridViewTextBoxColumn16
            // 
            this.dataGridViewTextBoxColumn16.DataPropertyName = "PromptLeftPosition";
            this.dataGridViewTextBoxColumn16.HeaderText = "PromptLeftPosition";
            this.dataGridViewTextBoxColumn16.Name = "dataGridViewTextBoxColumn16";
            // 
            // dataGridViewTextBoxColumn17
            // 
            this.dataGridViewTextBoxColumn17.DataPropertyName = "PromptTopPosition";
            this.dataGridViewTextBoxColumn17.HeaderText = "PromptTopPosition";
            this.dataGridViewTextBoxColumn17.Name = "dataGridViewTextBoxColumn17";
            // 
            // btnSetControlFont
            // 
            this.btnSetControlFont.Location = new System.Drawing.Point(232, 41);
            this.btnSetControlFont.Name = "btnSetControlFont";
            this.btnSetControlFont.Size = new System.Drawing.Size(178, 23);
            this.btnSetControlFont.TabIndex = 17;
            this.btnSetControlFont.Text = "Set Field Font";
            this.btnSetControlFont.UseVisualStyleBackColor = true;
            this.btnSetControlFont.Click += new System.EventHandler(this.btnSetControlFont_Click);
            // 
            // btnSetPromptFont
            // 
            this.btnSetPromptFont.Location = new System.Drawing.Point(232, 12);
            this.btnSetPromptFont.Name = "btnSetPromptFont";
            this.btnSetPromptFont.Size = new System.Drawing.Size(178, 23);
            this.btnSetPromptFont.TabIndex = 18;
            this.btnSetPromptFont.Text = "Set Prompt Font";
            this.btnSetPromptFont.UseVisualStyleBackColor = true;
            this.btnSetPromptFont.Click += new System.EventHandler(this.btnSetPromptFont_Click);
            // 
            // Import
            // 
            this.Import.DataPropertyName = "Import";
            this.Import.HeaderText = "Import";
            this.Import.Name = "Import";
            this.Import.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Import.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // SourceColumnName
            // 
            this.SourceColumnName.DataPropertyName = "SourceColumnName";
            this.SourceColumnName.HeaderText = "Column Name";
            this.SourceColumnName.Name = "SourceColumnName";
            this.SourceColumnName.ReadOnly = true;
            // 
            // DestinationColumnName
            // 
            this.DestinationColumnName.DataPropertyName = "DestinationColumnName";
            this.DestinationColumnName.HeaderText = "Field Name";
            this.DestinationColumnName.Name = "DestinationColumnName";
            // 
            // Prompt
            // 
            this.Prompt.DataPropertyName = "Prompt";
            this.Prompt.HeaderText = "Prompt";
            this.Prompt.Name = "Prompt";
            // 
            // SourceColumnType
            // 
            this.SourceColumnType.DataPropertyName = "SourceColumnType";
            this.SourceColumnType.HeaderText = "Column Type";
            this.SourceColumnType.Name = "SourceColumnType";
            this.SourceColumnType.ReadOnly = true;
            // 
            // FieldType
            // 
            this.FieldType.DataPropertyName = "FieldType";
            this.FieldType.HeaderText = "Field Type";
            this.FieldType.Name = "FieldType";
            // 
            // PageNumber
            // 
            this.PageNumber.DataPropertyName = "PageNumber";
            this.PageNumber.HeaderText = "Page";
            this.PageNumber.Name = "PageNumber";
            // 
            // TabIndex
            // 
            this.TabIndex.DataPropertyName = "TabIndex";
            this.TabIndex.HeaderText = "Tab";
            this.TabIndex.Name = "TabIndex";
            // 
            // IsTabStop
            // 
            this.IsTabStop.DataPropertyName = "IsTabStop";
            this.IsTabStop.HeaderText = "Tab Stop";
            this.IsTabStop.Name = "IsTabStop";
            // 
            // IsReadOnly
            // 
            this.IsReadOnly.DataPropertyName = "IsReadOnly";
            this.IsReadOnly.HeaderText = "Read Only";
            this.IsReadOnly.Name = "IsReadOnly";
            // 
            // IsRequired
            // 
            this.IsRequired.DataPropertyName = "IsRequired";
            this.IsRequired.HeaderText = "Required";
            this.IsRequired.Name = "IsRequired";
            // 
            // IsRepeatLast
            // 
            this.IsRepeatLast.DataPropertyName = "IsRepeatLast";
            this.IsRepeatLast.HeaderText = "Repeat Last";
            this.IsRepeatLast.Name = "IsRepeatLast";
            // 
            // HasRange
            // 
            this.HasRange.DataPropertyName = "HasRange";
            this.HasRange.HeaderText = "Range";
            this.HasRange.Name = "HasRange";
            // 
            // LowerBound
            // 
            this.LowerBound.DataPropertyName = "LowerBound";
            this.LowerBound.HeaderText = "Lower";
            this.LowerBound.Name = "LowerBound";
            // 
            // UpperBound
            // 
            this.UpperBound.DataPropertyName = "UpperBound";
            this.UpperBound.HeaderText = "Upper";
            this.UpperBound.Name = "UpperBound";
            // 
            // ListSourceTableName
            // 
            this.ListSourceTableName.DataPropertyName = "ListSourceTableName";
            this.ListSourceTableName.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            this.ListSourceTableName.HeaderText = "ListSourceTableName";
            this.ListSourceTableName.MaxDropDownItems = 64;
            this.ListSourceTableName.Name = "ListSourceTableName";
            this.ListSourceTableName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // ListSourceTextColumnName
            // 
            this.ListSourceTextColumnName.DataPropertyName = "ListSourceTextColumnName";
            this.ListSourceTextColumnName.HeaderText = "ListSourceTextColumnName";
            this.ListSourceTextColumnName.Name = "ListSourceTextColumnName";
            this.ListSourceTextColumnName.ReadOnly = true;
            // 
            // ListSourceTable
            // 
            this.ListSourceTable.DataPropertyName = "ListSourceTable";
            this.ListSourceTable.HeaderText = "ListSourceTable";
            this.ListSourceTable.Name = "ListSourceTable";
            this.ListSourceTable.ReadOnly = true;
            // 
            // ControlFont
            // 
            this.ControlFont.DataPropertyName = "ControlFont";
            this.ControlFont.HeaderText = "ControlFont";
            this.ControlFont.Name = "ControlFont";
            this.ControlFont.Visible = false;
            // 
            // PromptFont
            // 
            this.PromptFont.DataPropertyName = "PromptFont";
            this.PromptFont.HeaderText = "PromptFont";
            this.PromptFont.Name = "PromptFont";
            this.PromptFont.Visible = false;
            // 
            // ControlLeftPosition
            // 
            this.ControlLeftPosition.DataPropertyName = "ControlLeftPosition";
            this.ControlLeftPosition.HeaderText = "ControlLeftPosition";
            this.ControlLeftPosition.Name = "ControlLeftPosition";
            this.ControlLeftPosition.Visible = false;
            // 
            // ControlTopPosition
            // 
            this.ControlTopPosition.DataPropertyName = "ControlTopPosition";
            this.ControlTopPosition.HeaderText = "ControlTopPosition";
            this.ControlTopPosition.Name = "ControlTopPosition";
            this.ControlTopPosition.Visible = false;
            // 
            // PromptLeftPosition
            // 
            this.PromptLeftPosition.DataPropertyName = "PromptLeftPosition";
            this.PromptLeftPosition.HeaderText = "PromptLeftPosition";
            this.PromptLeftPosition.Name = "PromptLeftPosition";
            this.PromptLeftPosition.Visible = false;
            // 
            // PromptTopPosition
            // 
            this.PromptTopPosition.DataPropertyName = "PromptTopPosition";
            this.PromptTopPosition.HeaderText = "PromptTopPosition";
            this.PromptTopPosition.Name = "PromptTopPosition";
            this.PromptTopPosition.Visible = false;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(417, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(465, 78);
            this.label3.TabIndex = 19;
            this.label3.Text = resources.GetString("label3.Text");
            // 
            // TableToViewDialog
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(894, 497);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnImportListValues);
            this.Controls.Add(this.btnSetPromptFont);
            this.Controls.Add(this.btnSetControlFont);
            this.Controls.Add(this.dgvFormFields);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.textViewName);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.textTableName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TableToViewDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Table-to-Form";
            ((System.ComponentModel.ISupportInitialize)(this.dgvFormFields)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textViewName;
        private System.Windows.Forms.TextBox textTableName;
        private System.Windows.Forms.DataGridView dgvFormFields;
        private System.Windows.Forms.Button btnImportListValues;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn10;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn11;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn12;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn13;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn14;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn15;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn16;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn17;
        private System.Windows.Forms.Button btnSetControlFont;
        private System.Windows.Forms.Button btnSetPromptFont;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Import;
        private System.Windows.Forms.DataGridViewTextBoxColumn SourceColumnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn DestinationColumnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Prompt;
        private System.Windows.Forms.DataGridViewTextBoxColumn SourceColumnType;
        private System.Windows.Forms.DataGridViewComboBoxColumn FieldType;
        private System.Windows.Forms.DataGridViewTextBoxColumn PageNumber;
        private System.Windows.Forms.DataGridViewTextBoxColumn TabIndex;
        private System.Windows.Forms.DataGridViewCheckBoxColumn IsTabStop;
        private System.Windows.Forms.DataGridViewCheckBoxColumn IsReadOnly;
        private System.Windows.Forms.DataGridViewCheckBoxColumn IsRequired;
        private System.Windows.Forms.DataGridViewCheckBoxColumn IsRepeatLast;
        private System.Windows.Forms.DataGridViewCheckBoxColumn HasRange;
        private System.Windows.Forms.DataGridViewTextBoxColumn LowerBound;
        private System.Windows.Forms.DataGridViewTextBoxColumn UpperBound;
        private System.Windows.Forms.DataGridViewComboBoxColumn ListSourceTableName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ListSourceTextColumnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ListSourceTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn ControlFont;
        private System.Windows.Forms.DataGridViewTextBoxColumn PromptFont;
        private System.Windows.Forms.DataGridViewTextBoxColumn ControlLeftPosition;
        private System.Windows.Forms.DataGridViewTextBoxColumn ControlTopPosition;
        private System.Windows.Forms.DataGridViewTextBoxColumn PromptLeftPosition;
        private System.Windows.Forms.DataGridViewTextBoxColumn PromptTopPosition;
        private System.Windows.Forms.Label label3;
    }
}