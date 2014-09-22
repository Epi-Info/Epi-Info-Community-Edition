namespace Utilities.Grammar
{
    partial class ConstantsGenerator
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
            this.btnProcessXml = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnProcessXml
            // 
            this.btnProcessXml.Location = new System.Drawing.Point(12, 12);
            this.btnProcessXml.Name = "btnProcessXml";
            this.btnProcessXml.Size = new System.Drawing.Size(123, 23);
            this.btnProcessXml.TabIndex = 0;
            this.btnProcessXml.Text = "Generate Constants";
            this.btnProcessXml.UseVisualStyleBackColor = true;
            this.btnProcessXml.Click += new System.EventHandler(this.btnProcessXml_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(12, 41);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(858, 552);
            this.textBox1.TabIndex = 1;
            // 
            // ConstantsGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(882, 605);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnProcessXml);
            this.Name = "ConstantsGenerator";
            this.Text = "Grammar Constants Generator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnProcessXml;
        private System.Windows.Forms.TextBox textBox1;
    }
}

