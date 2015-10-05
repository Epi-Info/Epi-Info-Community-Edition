namespace Epi.Windows.MakeView.Dialogs
    {
    partial class UserAuthentication
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserAuthentication));
            this.PassWordTextBox1 = new System.Windows.Forms.MaskedTextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.LoginButton = new System.Windows.Forms.Button();
            this.lblEmailAddress = new System.Windows.Forms.Label();
            this.EmailAddresstextBox1 = new System.Windows.Forms.TextBox();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.lblWebEnterLoginInstructions = new System.Windows.Forms.Label();
            this.lblWebEnterLoginTitle = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // PassWordTextBox1
            // 
            resources.ApplyResources(this.PassWordTextBox1, "PassWordTextBox1");
            this.PassWordTextBox1.Name = "PassWordTextBox1";
            this.PassWordTextBox1.UseSystemPasswordChar = true;
            // 
            // lblPassword
            // 
            resources.ApplyResources(this.lblPassword, "lblPassword");
            this.lblPassword.Name = "lblPassword";
            // 
            // LoginButton
            // 
            resources.ApplyResources(this.LoginButton, "LoginButton");
            this.LoginButton.Name = "LoginButton";
            this.LoginButton.UseVisualStyleBackColor = true;
            this.LoginButton.Click += new System.EventHandler(this.LoginButton_Click);
            // 
            // lblEmailAddress
            // 
            resources.ApplyResources(this.lblEmailAddress, "lblEmailAddress");
            this.lblEmailAddress.Name = "lblEmailAddress";
            // 
            // EmailAddresstextBox1
            // 
            resources.ApplyResources(this.EmailAddresstextBox1, "EmailAddresstextBox1");
            this.EmailAddresstextBox1.Name = "EmailAddresstextBox1";
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // lblWebEnterLoginInstructions
            // 
            resources.ApplyResources(this.lblWebEnterLoginInstructions, "lblWebEnterLoginInstructions");
            this.lblWebEnterLoginInstructions.Name = "lblWebEnterLoginInstructions";
            // 
            // lblWebEnterLoginTitle
            // 
            resources.ApplyResources(this.lblWebEnterLoginTitle, "lblWebEnterLoginTitle");
            this.lblWebEnterLoginTitle.Name = "lblWebEnterLoginTitle";
            // 
            // UserAuthentication
            // 
            this.AcceptButton = this.LoginButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblWebEnterLoginInstructions);
            this.Controls.Add(this.lblWebEnterLoginTitle);
            this.Controls.Add(this.EmailAddresstextBox1);
            this.Controls.Add(this.lblEmailAddress);
            this.Controls.Add(this.LoginButton);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.PassWordTextBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UserAuthentication";
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

            }

        #endregion

        private System.Windows.Forms.MaskedTextBox PassWordTextBox1;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Button LoginButton;
        private System.Windows.Forms.Label lblEmailAddress;
        private System.Windows.Forms.TextBox EmailAddresstextBox1;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.Label lblWebEnterLoginInstructions;
        private System.Windows.Forms.Label lblWebEnterLoginTitle;
        }
    }