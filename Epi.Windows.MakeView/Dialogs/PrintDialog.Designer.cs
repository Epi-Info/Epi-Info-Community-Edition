namespace Epi.Windows.MakeView.Dialogs
{
    partial class Print
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private Epi.Windows.MakeView.PresentationLogic.GuiMediator mediator;

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
            this.btnCancel = new System.Windows.Forms.Button();
            this.buttonPrint = new System.Windows.Forms.Button();
            this.SelectPages = new System.Windows.Forms.GroupBox();
            this.pageEnd = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.pageStart = new System.Windows.Forms.TextBox();
            this.selectPages_All = new System.Windows.Forms.RadioButton();
            this.selectPages_Range = new System.Windows.Forms.RadioButton();
            this.SelectPages.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(123, 155);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(85, 24);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // buttonPrint
            // 
            this.buttonPrint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPrint.Location = new System.Drawing.Point(23, 155);
            this.buttonPrint.Name = "buttonPrint";
            this.buttonPrint.Size = new System.Drawing.Size(85, 24);
            this.buttonPrint.TabIndex = 0;
            this.buttonPrint.Text = "Print";
            this.buttonPrint.UseVisualStyleBackColor = true;
            this.buttonPrint.Click += new System.EventHandler(this.buttonPrint_Click_1);
            // 
            // SelectPages
            // 
            this.SelectPages.Controls.Add(this.pageEnd);
            this.SelectPages.Controls.Add(this.label3);
            this.SelectPages.Controls.Add(this.pageStart);
            this.SelectPages.Controls.Add(this.selectPages_All);
            this.SelectPages.Controls.Add(this.selectPages_Range);
            this.SelectPages.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SelectPages.Location = new System.Drawing.Point(12, 12);
            this.SelectPages.Name = "SelectPages";
            this.SelectPages.Size = new System.Drawing.Size(248, 137);
            this.SelectPages.TabIndex = 9;
            this.SelectPages.TabStop = false;
            this.SelectPages.Text = "Select Pages";
            // 
            // pageEnd
            // 
            this.pageEnd.Enabled = false;
            this.pageEnd.Location = new System.Drawing.Point(174, 45);
            this.pageEnd.MaxLength = 5;
            this.pageEnd.Name = "pageEnd";
            this.pageEnd.Size = new System.Drawing.Size(41, 20);
            this.pageEnd.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(158, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(10, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "-";
            // 
            // pageStart
            // 
            this.pageStart.Enabled = false;
            this.pageStart.Location = new System.Drawing.Point(111, 45);
            this.pageStart.MaxLength = 5;
            this.pageStart.Name = "pageStart";
            this.pageStart.Size = new System.Drawing.Size(41, 20);
            this.pageStart.TabIndex = 6;
            // 
            // selectPages_All
            // 
            this.selectPages_All.AutoSize = true;
            this.selectPages_All.Location = new System.Drawing.Point(11, 24);
            this.selectPages_All.Name = "selectPages_All";
            this.selectPages_All.Size = new System.Drawing.Size(69, 17);
            this.selectPages_All.TabIndex = 0;
            this.selectPages_All.Text = "All Pages";
            this.selectPages_All.UseVisualStyleBackColor = true;
            this.selectPages_All.CheckedChanged += new System.EventHandler(this.selectPages_Range_CheckedChanged);
            // 
            // selectPages_Range
            // 
            this.selectPages_Range.AutoSize = true;
            this.selectPages_Range.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.selectPages_Range.Location = new System.Drawing.Point(11, 47);
            this.selectPages_Range.Name = "selectPages_Range";
            this.selectPages_Range.Size = new System.Drawing.Size(85, 17);
            this.selectPages_Range.TabIndex = 2;
            this.selectPages_Range.TabStop = true;
            this.selectPages_Range.Text = "Page Range";
            this.selectPages_Range.UseVisualStyleBackColor = true;
            // 
            // Print
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(268, 191);
            this.Controls.Add(this.SelectPages);
            this.Controls.Add(this.buttonPrint);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Print";
            this.Text = "Print";
            this.SelectPages.ResumeLayout(false);
            this.SelectPages.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button buttonPrint;
        private System.Windows.Forms.GroupBox SelectPages;
        private System.Windows.Forms.TextBox pageEnd;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox pageStart;
        private System.Windows.Forms.RadioButton selectPages_All;
        private System.Windows.Forms.RadioButton selectPages_Range;
    }
}