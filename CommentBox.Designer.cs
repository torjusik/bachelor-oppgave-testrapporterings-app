namespace Bachelor_Testing_V1
{
    partial class CommentBox
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
            rtbComment = new RichTextBox();
            btnConfirm = new Button();
            btnCancel = new Button();
            lblRequirement = new Label();
            SuspendLayout();
            // 
            // rtbComment
            // 
            rtbComment.Location = new Point(12, 58);
            rtbComment.Name = "rtbComment";
            rtbComment.Size = new Size(319, 153);
            rtbComment.TabIndex = 0;
            rtbComment.Text = "";
            // 
            // btnConfirm
            // 
            btnConfirm.Location = new Point(256, 217);
            btnConfirm.Name = "btnConfirm";
            btnConfirm.Size = new Size(75, 23);
            btnConfirm.TabIndex = 1;
            btnConfirm.Text = "Confirm";
            btnConfirm.UseVisualStyleBackColor = true;
            btnConfirm.Click += btnConfirm_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(175, 217);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // lblRequirement
            // 
            lblRequirement.AutoSize = true;
            lblRequirement.Location = new Point(13, 20);
            lblRequirement.Name = "lblRequirement";
            lblRequirement.Size = new Size(38, 15);
            lblRequirement.TabIndex = 3;
            lblRequirement.Text = "label1";
            // 
            // CommentBox
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(342, 249);
            Controls.Add(lblRequirement);
            Controls.Add(btnCancel);
            Controls.Add(btnConfirm);
            Controls.Add(rtbComment);
            Name = "CommentBox";
            Text = "Comment on the requirement";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox rtbComment;
        private Button btnConfirm;
        private Button btnCancel;
        private Label lblRequirement;
    }
}