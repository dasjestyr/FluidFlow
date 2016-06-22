namespace FluidFlow.Sample1
{
    partial class TestForm
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
            this.btnCreateChangeRequest = new System.Windows.Forms.Button();
            this.btnQAApprove = new System.Windows.Forms.Button();
            this.btnManagerDeny = new System.Windows.Forms.Button();
            this.btnManagerApprove = new System.Windows.Forms.Button();
            this.btnQADeny = new System.Windows.Forms.Button();
            this.btnBoardApprove = new System.Windows.Forms.Button();
            this.btnBoardDeny = new System.Windows.Forms.Button();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnCreateChangeRequest
            // 
            this.btnCreateChangeRequest.Location = new System.Drawing.Point(12, 31);
            this.btnCreateChangeRequest.Name = "btnCreateChangeRequest";
            this.btnCreateChangeRequest.Size = new System.Drawing.Size(144, 23);
            this.btnCreateChangeRequest.TabIndex = 0;
            this.btnCreateChangeRequest.Text = "Create Change Request";
            this.btnCreateChangeRequest.UseVisualStyleBackColor = true;
            this.btnCreateChangeRequest.Click += new System.EventHandler(this.btnCreateChangeRequest_Click);
            // 
            // btnQAApprove
            // 
            this.btnQAApprove.Location = new System.Drawing.Point(12, 89);
            this.btnQAApprove.Name = "btnQAApprove";
            this.btnQAApprove.Size = new System.Drawing.Size(144, 23);
            this.btnQAApprove.TabIndex = 1;
            this.btnQAApprove.Text = "QA Approved";
            this.btnQAApprove.UseVisualStyleBackColor = true;
            // 
            // btnManagerDeny
            // 
            this.btnManagerDeny.Location = new System.Drawing.Point(162, 60);
            this.btnManagerDeny.Name = "btnManagerDeny";
            this.btnManagerDeny.Size = new System.Drawing.Size(144, 23);
            this.btnManagerDeny.TabIndex = 2;
            this.btnManagerDeny.Text = "Manager Denied";
            this.btnManagerDeny.UseVisualStyleBackColor = true;
            // 
            // btnManagerApprove
            // 
            this.btnManagerApprove.Location = new System.Drawing.Point(12, 60);
            this.btnManagerApprove.Name = "btnManagerApprove";
            this.btnManagerApprove.Size = new System.Drawing.Size(144, 23);
            this.btnManagerApprove.TabIndex = 3;
            this.btnManagerApprove.Text = "Manager Approved";
            this.btnManagerApprove.UseVisualStyleBackColor = true;
            // 
            // btnQADeny
            // 
            this.btnQADeny.Location = new System.Drawing.Point(162, 89);
            this.btnQADeny.Name = "btnQADeny";
            this.btnQADeny.Size = new System.Drawing.Size(144, 23);
            this.btnQADeny.TabIndex = 4;
            this.btnQADeny.Text = "QA Denied";
            this.btnQADeny.UseVisualStyleBackColor = true;
            // 
            // btnBoardApprove
            // 
            this.btnBoardApprove.Location = new System.Drawing.Point(12, 118);
            this.btnBoardApprove.Name = "btnBoardApprove";
            this.btnBoardApprove.Size = new System.Drawing.Size(144, 23);
            this.btnBoardApprove.TabIndex = 5;
            this.btnBoardApprove.Text = "Board Approved";
            this.btnBoardApprove.UseVisualStyleBackColor = true;
            // 
            // btnBoardDeny
            // 
            this.btnBoardDeny.Location = new System.Drawing.Point(162, 118);
            this.btnBoardDeny.Name = "btnBoardDeny";
            this.btnBoardDeny.Size = new System.Drawing.Size(144, 23);
            this.btnBoardDeny.TabIndex = 6;
            this.btnBoardDeny.Text = "Board Denied";
            this.btnBoardDeny.UseVisualStyleBackColor = true;
            // 
            // txtOutput
            // 
            this.txtOutput.Location = new System.Drawing.Point(348, 12);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.Size = new System.Drawing.Size(345, 485);
            this.txtOutput.TabIndex = 7;
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(712, 509);
            this.Controls.Add(this.txtOutput);
            this.Controls.Add(this.btnBoardDeny);
            this.Controls.Add(this.btnBoardApprove);
            this.Controls.Add(this.btnQADeny);
            this.Controls.Add(this.btnManagerApprove);
            this.Controls.Add(this.btnManagerDeny);
            this.Controls.Add(this.btnQAApprove);
            this.Controls.Add(this.btnCreateChangeRequest);
            this.Name = "TestForm";
            this.Text = "Workflow Tester";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCreateChangeRequest;
        private System.Windows.Forms.Button btnQAApprove;
        private System.Windows.Forms.Button btnManagerDeny;
        private System.Windows.Forms.Button btnManagerApprove;
        private System.Windows.Forms.Button btnQADeny;
        private System.Windows.Forms.Button btnBoardApprove;
        private System.Windows.Forms.Button btnBoardDeny;
        private System.Windows.Forms.TextBox txtOutput;
    }
}

