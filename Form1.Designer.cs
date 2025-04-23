namespace Bachelor_Testing_V1
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            rtbEquipment = new RichTextBox();
            rtbDescription = new RichTextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            lblStepName = new Label();
            btnNextStep = new Button();
            btnPrevStep = new Button();
            clbRequirements = new CheckedListBox();
            label5 = new Label();
            groupBox1 = new GroupBox();
            lsbSafetyReq = new ListBox();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // rtbEquipment
            // 
            rtbEquipment.Font = new Font("Segoe UI", 11F);
            rtbEquipment.Location = new Point(490, 207);
            rtbEquipment.Name = "rtbEquipment";
            rtbEquipment.ReadOnly = true;
            rtbEquipment.Size = new Size(201, 148);
            rtbEquipment.TabIndex = 2;
            rtbEquipment.Text = "Equipment needed will show here";
            // 
            // rtbDescription
            // 
            rtbDescription.Font = new Font("Segoe UI", 11F);
            rtbDescription.Location = new Point(86, 74);
            rtbDescription.Name = "rtbDescription";
            rtbDescription.ReadOnly = true;
            rtbDescription.Size = new Size(605, 112);
            rtbDescription.TabIndex = 3;
            rtbDescription.Text = "The step description will show here";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(88, 56);
            label1.Name = "label1";
            label1.Size = new Size(67, 15);
            label1.TabIndex = 5;
            label1.Text = "Description";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(87, 189);
            label2.Name = "label2";
            label2.Size = new Size(80, 15);
            label2.TabIndex = 6;
            label2.Text = "Requirements";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(490, 189);
            label3.Name = "label3";
            label3.Size = new Size(65, 15);
            label3.TabIndex = 7;
            label3.Text = "Equipment";
            // 
            // lblStepName
            // 
            lblStepName.AutoSize = true;
            lblStepName.Font = new Font("Segoe UI", 20F);
            lblStepName.Location = new Point(87, 15);
            lblStepName.Name = "lblStepName";
            lblStepName.Size = new Size(414, 37);
            lblStepName.TabIndex = 8;
            lblStepName.Text = "Step : \"Text is changed with code\"";
            lblStepName.TextAlign = ContentAlignment.TopCenter;
            // 
            // btnNextStep
            // 
            btnNextStep.Location = new Point(697, 68);
            btnNextStep.Name = "btnNextStep";
            btnNextStep.Size = new Size(75, 287);
            btnNextStep.TabIndex = 10;
            btnNextStep.Text = "Next Step ->";
            btnNextStep.UseVisualStyleBackColor = true;
            btnNextStep.Click += btnNextStep_Click;
            // 
            // btnPrevStep
            // 
            btnPrevStep.Location = new Point(6, 68);
            btnPrevStep.Name = "btnPrevStep";
            btnPrevStep.Size = new Size(75, 287);
            btnPrevStep.TabIndex = 11;
            btnPrevStep.Text = "Previous Step <-";
            btnPrevStep.UseVisualStyleBackColor = true;
            btnPrevStep.Click += btnPrevStep_Click;
            // 
            // clbRequirements
            // 
            clbRequirements.FormattingEnabled = true;
            clbRequirements.ImeMode = ImeMode.On;
            clbRequirements.Items.AddRange(new object[] { "Requirement 1", "Requirement 2", "Requirement 3" });
            clbRequirements.Location = new Point(88, 207);
            clbRequirements.Name = "clbRequirements";
            clbRequirements.Size = new Size(396, 148);
            clbRequirements.TabIndex = 14;
            clbRequirements.ThreeDCheckBoxes = true;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 14F);
            label5.Location = new Point(301, 9);
            label5.Name = "label5";
            label5.Size = new Size(183, 25);
            label5.TabIndex = 15;
            label5.Text = "Safety Requirements";
            // 
            // groupBox1
            // 
            groupBox1.BackColor = SystemColors.AppWorkspace;
            groupBox1.Controls.Add(btnNextStep);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(lblStepName);
            groupBox1.Controls.Add(rtbDescription);
            groupBox1.Controls.Add(btnPrevStep);
            groupBox1.Controls.Add(clbRequirements);
            groupBox1.Controls.Add(rtbEquipment);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label2);
            groupBox1.Location = new Point(12, 168);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(778, 362);
            groupBox1.TabIndex = 16;
            groupBox1.TabStop = false;
            groupBox1.Text = "Steps";
            // 
            // lsbSafetyReq
            // 
            lsbSafetyReq.FormattingEnabled = true;
            lsbSafetyReq.ItemHeight = 15;
            lsbSafetyReq.Location = new Point(180, 50);
            lsbSafetyReq.Name = "lsbSafetyReq";
            lsbSafetyReq.Size = new Size(443, 94);
            lsbSafetyReq.TabIndex = 17;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(802, 542);
            Controls.Add(lsbSafetyReq);
            Controls.Add(label5);
            Controls.Add(groupBox1);
            KeyPreview = true;
            Name = "Form1";
            Text = "Test Procedure";
            FormClosed += Form1_FormClosed;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private RichTextBox rtbEquipment;
        private RichTextBox rtbDescription;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label lblStepName;
        private Button btnNextStep;
        private Button btnPrevStep;
        private CheckedListBox clbRequirements;
        private Label label5;
        private GroupBox groupBox1;
        private ListBox lsbSafetyReq;
    }
}
