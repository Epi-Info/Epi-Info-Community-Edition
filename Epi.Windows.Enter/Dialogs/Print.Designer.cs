namespace Epi.Windows.Enter.Dialogs
{
    partial class Print
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
        private Epi.Windows.Enter.PresentationLogic.GuiMediator mediator;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Print));
            this.buttonPrintPreview = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SelectRecords = new System.Windows.Forms.GroupBox();
            this.SelectTab_Order = new System.Windows.Forms.RadioButton();
            this.selectRecords_All = new System.Windows.Forms.RadioButton();
            this.recordEnd = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.recordStart = new System.Windows.Forms.TextBox();
            this.selectRecords_None = new System.Windows.Forms.RadioButton();
            this.selectRecords_Current = new System.Windows.Forms.RadioButton();
            this.selectRecords_Range = new System.Windows.Forms.RadioButton();
            this.buttonPrint = new System.Windows.Forms.Button();
            this.SelectPages = new System.Windows.Forms.GroupBox();
            this.pageEnd = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.pageStart = new System.Windows.Forms.TextBox();
            this.selectPages_All = new System.Windows.Forms.RadioButton();
            this.selectPages_Range = new System.Windows.Forms.RadioButton();
            this.SelectRecords.SuspendLayout();
            this.SelectPages.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonPrintPreview
            // 
            this.buttonPrintPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPrintPreview.Location = new System.Drawing.Point(448, 191);
            this.buttonPrintPreview.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonPrintPreview.Name = "buttonPrintPreview";
            this.buttonPrintPreview.Size = new System.Drawing.Size(113, 30);
            this.buttonPrintPreview.TabIndex = 1;
            this.buttonPrintPreview.Text = "Preview";
            this.buttonPrintPreview.UseVisualStyleBackColor = true;
            this.buttonPrintPreview.Click += new System.EventHandler(this.buttonPrintPreview_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(569, 191);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(113, 30);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // SelectRecords
            // 
            this.SelectRecords.Controls.Add(this.SelectTab_Order);
            this.SelectRecords.Controls.Add(this.selectRecords_All);
            this.SelectRecords.Controls.Add(this.recordEnd);
            this.SelectRecords.Controls.Add(this.label2);
            this.SelectRecords.Controls.Add(this.recordStart);
            this.SelectRecords.Controls.Add(this.selectRecords_None);
            this.SelectRecords.Controls.Add(this.selectRecords_Current);
            this.SelectRecords.Controls.Add(this.selectRecords_Range);
            this.SelectRecords.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SelectRecords.Location = new System.Drawing.Point(16, 15);
            this.SelectRecords.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.SelectRecords.Name = "SelectRecords";
            this.SelectRecords.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.SelectRecords.Size = new System.Drawing.Size(331, 169);
            this.SelectRecords.TabIndex = 3;
            this.SelectRecords.TabStop = false;
            this.SelectRecords.Text = "Select Records";
            // 
            // SelectTab_Order
            // 
            this.SelectTab_Order.AutoSize = true;
            this.SelectTab_Order.Location = new System.Drawing.Point(15, 138);
            this.SelectTab_Order.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.SelectTab_Order.Name = "SelectTab_Order";
            this.SelectTab_Order.Size = new System.Drawing.Size(95, 21);
            this.SelectTab_Order.TabIndex = 10;
            this.SelectTab_Order.TabStop = true;
            this.SelectTab_Order.Text = "Tab Order";
            this.SelectTab_Order.UseVisualStyleBackColor = true;
            this.SelectTab_Order.CheckedChanged += new System.EventHandler(this.SelectTab_Order_CheckedChanged);
            // 
            // selectRecords_All
            // 
            this.selectRecords_All.AutoSize = true;
            this.selectRecords_All.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.selectRecords_All.Location = new System.Drawing.Point(15, 114);
            this.selectRecords_All.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.selectRecords_All.Name = "selectRecords_All";
            this.selectRecords_All.Size = new System.Drawing.Size(101, 21);
            this.selectRecords_All.TabIndex = 9;
            this.selectRecords_All.TabStop = true;
            this.selectRecords_All.Text = "All Records";
            this.selectRecords_All.UseVisualStyleBackColor = true;
            this.selectRecords_All.CheckedChanged += new System.EventHandler(this.selectRecords_All_CheckedChanged);
            // 
            // recordEnd
            // 
            this.recordEnd.Enabled = false;
            this.recordEnd.Location = new System.Drawing.Point(232, 84);
            this.recordEnd.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.recordEnd.MaxLength = 5;
            this.recordEnd.Name = "recordEnd";
            this.recordEnd.Size = new System.Drawing.Size(53, 23);
            this.recordEnd.TabIndex = 8;
            this.recordEnd.TextChanged += new System.EventHandler(this.recordEnd_TextChanged);
            this.recordEnd.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.recordEnd_KeyPress);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(211, 89);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(13, 17);
            this.label2.TabIndex = 7;
            this.label2.Text = "-";
            // 
            // recordStart
            // 
            this.recordStart.Enabled = false;
            this.recordStart.Location = new System.Drawing.Point(148, 84);
            this.recordStart.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.recordStart.MaxLength = 5;
            this.recordStart.Name = "recordStart";
            this.recordStart.Size = new System.Drawing.Size(53, 23);
            this.recordStart.TabIndex = 6;
            this.recordStart.TextChanged += new System.EventHandler(this.recordStart_TextChanged);
            this.recordStart.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.recordStart_KeyPress);
            // 
            // selectRecords_None
            // 
            this.selectRecords_None.AutoSize = true;
            this.selectRecords_None.Location = new System.Drawing.Point(15, 30);
            this.selectRecords_None.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.selectRecords_None.Name = "selectRecords_None";
            this.selectRecords_None.Size = new System.Drawing.Size(189, 21);
            this.selectRecords_None.TabIndex = 0;
            this.selectRecords_None.TabStop = true;
            this.selectRecords_None.Text = "No Records (Blank Form)";
            this.selectRecords_None.UseVisualStyleBackColor = true;
            this.selectRecords_None.CheckedChanged += new System.EventHandler(this.selectRecords_None_CheckedChanged);
            // 
            // selectRecords_Current
            // 
            this.selectRecords_Current.AutoSize = true;
            this.selectRecords_Current.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.selectRecords_Current.Location = new System.Drawing.Point(15, 58);
            this.selectRecords_Current.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.selectRecords_Current.Name = "selectRecords_Current";
            this.selectRecords_Current.Size = new System.Drawing.Size(126, 21);
            this.selectRecords_Current.TabIndex = 1;
            this.selectRecords_Current.TabStop = true;
            this.selectRecords_Current.Text = "Current Record";
            this.selectRecords_Current.UseVisualStyleBackColor = true;
            this.selectRecords_Current.CheckedChanged += new System.EventHandler(this.selectRecords_Current_CheckedChanged);
            // 
            // selectRecords_Range
            // 
            this.selectRecords_Range.AutoSize = true;
            this.selectRecords_Range.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.selectRecords_Range.Location = new System.Drawing.Point(15, 86);
            this.selectRecords_Range.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.selectRecords_Range.Name = "selectRecords_Range";
            this.selectRecords_Range.Size = new System.Drawing.Size(121, 21);
            this.selectRecords_Range.TabIndex = 2;
            this.selectRecords_Range.TabStop = true;
            this.selectRecords_Range.Text = "Record Range";
            this.selectRecords_Range.UseVisualStyleBackColor = true;
            this.selectRecords_Range.CheckedChanged += new System.EventHandler(this.selectRecords_Range_CheckedChanged);
            // 
            // buttonPrint
            // 
            this.buttonPrint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPrint.Location = new System.Drawing.Point(327, 191);
            this.buttonPrint.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonPrint.Name = "buttonPrint";
            this.buttonPrint.Size = new System.Drawing.Size(113, 30);
            this.buttonPrint.TabIndex = 0;
            this.buttonPrint.Text = "Print";
            this.buttonPrint.UseVisualStyleBackColor = true;
            this.buttonPrint.Click += new System.EventHandler(this.buttonPrint_Click);
            // 
            // SelectPages
            // 
            this.SelectPages.Controls.Add(this.pageEnd);
            this.SelectPages.Controls.Add(this.label3);
            this.SelectPages.Controls.Add(this.pageStart);
            this.SelectPages.Controls.Add(this.selectPages_All);
            this.SelectPages.Controls.Add(this.selectPages_Range);
            this.SelectPages.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SelectPages.Location = new System.Drawing.Point(355, 15);
            this.SelectPages.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.SelectPages.Name = "SelectPages";
            this.SelectPages.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.SelectPages.Size = new System.Drawing.Size(331, 169);
            this.SelectPages.TabIndex = 9;
            this.SelectPages.TabStop = false;
            this.SelectPages.Text = "Select Pages";
            // 
            // pageEnd
            // 
            this.pageEnd.Enabled = false;
            this.pageEnd.Location = new System.Drawing.Point(232, 55);
            this.pageEnd.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pageEnd.MaxLength = 5;
            this.pageEnd.Name = "pageEnd";
            this.pageEnd.Size = new System.Drawing.Size(53, 23);
            this.pageEnd.TabIndex = 8;
            this.pageEnd.TextChanged += new System.EventHandler(this.pageEnd_TextChanged);
            this.pageEnd.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.pageEnd_KeyPress);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(211, 60);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(13, 17);
            this.label3.TabIndex = 7;
            this.label3.Text = "-";
            // 
            // pageStart
            // 
            this.pageStart.Enabled = false;
            this.pageStart.Location = new System.Drawing.Point(148, 55);
            this.pageStart.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pageStart.MaxLength = 5;
            this.pageStart.Name = "pageStart";
            this.pageStart.Size = new System.Drawing.Size(53, 23);
            this.pageStart.TabIndex = 6;
            this.pageStart.TextChanged += new System.EventHandler(this.pageStart_TextChanged);
            this.pageStart.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.pageStart_KeyPress);
            // 
            // selectPages_All
            // 
            this.selectPages_All.AutoSize = true;
            this.selectPages_All.Location = new System.Drawing.Point(15, 30);
            this.selectPages_All.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.selectPages_All.Name = "selectPages_All";
            this.selectPages_All.Size = new System.Drawing.Size(88, 21);
            this.selectPages_All.TabIndex = 0;
            this.selectPages_All.TabStop = true;
            this.selectPages_All.Text = "All Pages";
            this.selectPages_All.UseVisualStyleBackColor = true;
            this.selectPages_All.CheckedChanged += new System.EventHandler(this.selectPages_All_CheckedChanged);
            // 
            // selectPages_Range
            // 
            this.selectPages_Range.AutoSize = true;
            this.selectPages_Range.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.selectPages_Range.Location = new System.Drawing.Point(15, 58);
            this.selectPages_Range.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.selectPages_Range.Name = "selectPages_Range";
            this.selectPages_Range.Size = new System.Drawing.Size(108, 21);
            this.selectPages_Range.TabIndex = 2;
            this.selectPages_Range.TabStop = true;
            this.selectPages_Range.Text = "Page Range";
            this.selectPages_Range.UseVisualStyleBackColor = true;
            this.selectPages_Range.CheckedChanged += new System.EventHandler(this.selectPages_Range_CheckedChanged);
            // 
            // Print
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(699, 235);
            this.Controls.Add(this.SelectPages);
            this.Controls.Add(this.buttonPrint);
            this.Controls.Add(this.SelectRecords);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.buttonPrintPreview);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Print";
            this.Text = "Print";
            this.SelectRecords.ResumeLayout(false);
            this.SelectRecords.PerformLayout();
            this.SelectPages.ResumeLayout(false);
            this.SelectPages.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.Button buttonPrintPreview;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox SelectRecords;
        private System.Windows.Forms.RadioButton selectRecords_Current;
        private System.Windows.Forms.RadioButton selectRecords_Range;
        private System.Windows.Forms.RadioButton selectRecords_None;
        private System.Windows.Forms.Button buttonPrint;
        private System.Windows.Forms.RadioButton selectRecords_All;
        private System.Windows.Forms.TextBox recordEnd;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox recordStart;
        private System.Windows.Forms.GroupBox SelectPages;
        private System.Windows.Forms.TextBox pageEnd;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox pageStart;
        private System.Windows.Forms.RadioButton selectPages_All;
        private System.Windows.Forms.RadioButton selectPages_Range;
        private System.Windows.Forms.RadioButton SelectTab_Order;
	}
}
