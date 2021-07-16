namespace EpiDashboard.Dialogs
{
    partial class ValueMapperDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ValueMapperDialog));
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOK = new System.Windows.Forms.Button();
			this.lbxYesValues = new System.Windows.Forms.ListBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.lbxNoValues = new System.Windows.Forms.ListBox();
			this.lbxAllValues = new System.Windows.Forms.ListBox();
			this.label3 = new System.Windows.Forms.Label();
			this.btnAddYes = new System.Windows.Forms.Button();
			this.btnAddNo = new System.Windows.Forms.Button();
			this.btnRemoveYes = new System.Windows.Forms.Button();
			this.btnRemoveNo = new System.Windows.Forms.Button();
			this.btnClear = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnCancel
			// 
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// btnOK
			// 
			resources.ApplyResources(this.btnOK, "btnOK");
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Name = "btnOK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// lbxYesValues
			// 
			resources.ApplyResources(this.lbxYesValues, "lbxYesValues");
			this.lbxYesValues.FormattingEnabled = true;
			this.lbxYesValues.Name = "lbxYesValues";
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// label2
			// 
			resources.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			// 
			// lbxNoValues
			// 
			resources.ApplyResources(this.lbxNoValues, "lbxNoValues");
			this.lbxNoValues.FormattingEnabled = true;
			this.lbxNoValues.Name = "lbxNoValues";
			// 
			// lbxAllValues
			// 
			resources.ApplyResources(this.lbxAllValues, "lbxAllValues");
			this.lbxAllValues.FormattingEnabled = true;
			this.lbxAllValues.Name = "lbxAllValues";
			// 
			// label3
			// 
			resources.ApplyResources(this.label3, "label3");
			this.label3.Name = "label3";
			// 
			// btnAddYes
			// 
			resources.ApplyResources(this.btnAddYes, "btnAddYes");
			this.btnAddYes.Name = "btnAddYes";
			this.btnAddYes.UseVisualStyleBackColor = true;
			this.btnAddYes.Click += new System.EventHandler(this.btnAddYes_Click);
			// 
			// btnAddNo
			// 
			resources.ApplyResources(this.btnAddNo, "btnAddNo");
			this.btnAddNo.Name = "btnAddNo";
			this.btnAddNo.UseVisualStyleBackColor = true;
			this.btnAddNo.Click += new System.EventHandler(this.btnAddNo_Click);
			// 
			// btnRemoveYes
			// 
			resources.ApplyResources(this.btnRemoveYes, "btnRemoveYes");
			this.btnRemoveYes.Name = "btnRemoveYes";
			this.btnRemoveYes.UseVisualStyleBackColor = true;
			this.btnRemoveYes.Click += new System.EventHandler(this.btnRemoveYes_Click);
			// 
			// btnRemoveNo
			// 
			resources.ApplyResources(this.btnRemoveNo, "btnRemoveNo");
			this.btnRemoveNo.Name = "btnRemoveNo";
			this.btnRemoveNo.UseVisualStyleBackColor = true;
			this.btnRemoveNo.Click += new System.EventHandler(this.btnRemoveNo_Click);
			// 
			// btnClear
			// 
			resources.ApplyResources(this.btnClear, "btnClear");
			this.btnClear.Name = "btnClear";
			this.btnClear.UseVisualStyleBackColor = true;
			this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
			// 
			// ValueMapperDialog
			// 
			this.AcceptButton = this.btnOK;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.Controls.Add(this.btnClear);
			this.Controls.Add(this.btnRemoveNo);
			this.Controls.Add(this.btnRemoveYes);
			this.Controls.Add(this.btnAddNo);
			this.Controls.Add(this.btnAddYes);
			this.Controls.Add(this.lbxAllValues);
			this.Controls.Add(this.lbxNoValues);
			this.Controls.Add(this.lbxYesValues);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.label3);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ValueMapperDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ListBox lbxYesValues;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox lbxNoValues;
        private System.Windows.Forms.ListBox lbxAllValues;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnAddYes;
        private System.Windows.Forms.Button btnAddNo;
        private System.Windows.Forms.Button btnRemoveYes;
        private System.Windows.Forms.Button btnRemoveNo;
        private System.Windows.Forms.Button btnClear;
    }
}