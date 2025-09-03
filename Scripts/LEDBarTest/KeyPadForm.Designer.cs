namespace LEDBarTest
{
    partial class KeyPadForm
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
            this.btnMuteAlarm = new System.Windows.Forms.Button();
            this.btnSelect = new System.Windows.Forms.Button();
            this.btnPurge = new System.Windows.Forms.Button();
            this.btnDock = new System.Windows.Forms.Button();
            this.btnFlow = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnMuteAlarm
            // 
            this.btnMuteAlarm.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnMuteAlarm.FlatAppearance.BorderSize = 4;
            this.btnMuteAlarm.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMuteAlarm.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnMuteAlarm.Location = new System.Drawing.Point(58, 56);
            this.btnMuteAlarm.Name = "btnMuteAlarm";
            this.btnMuteAlarm.Size = new System.Drawing.Size(121, 61);
            this.btnMuteAlarm.TabIndex = 0;
            this.btnMuteAlarm.Text = "MUTE ALARM";
            this.btnMuteAlarm.UseVisualStyleBackColor = false;
            this.btnMuteAlarm.Click += new System.EventHandler(this.btnMuteAlarm_Click);
            // 
            // btnSelect
            // 
            this.btnSelect.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnSelect.Enabled = false;
            this.btnSelect.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelect.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnSelect.Location = new System.Drawing.Point(185, 56);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(121, 61);
            this.btnSelect.TabIndex = 1;
            this.btnSelect.Text = "SELECT";
            this.btnSelect.UseVisualStyleBackColor = false;
            this.btnSelect.Click += new System.EventHandler(this.btnMuteAlarm_Click);
            // 
            // btnPurge
            // 
            this.btnPurge.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnPurge.Enabled = false;
            this.btnPurge.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPurge.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnPurge.Location = new System.Drawing.Point(439, 56);
            this.btnPurge.Name = "btnPurge";
            this.btnPurge.Size = new System.Drawing.Size(121, 61);
            this.btnPurge.TabIndex = 3;
            this.btnPurge.Text = "PURGE";
            this.btnPurge.UseVisualStyleBackColor = false;
            this.btnPurge.Click += new System.EventHandler(this.btnMuteAlarm_Click);
            // 
            // btnDock
            // 
            this.btnDock.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnDock.Enabled = false;
            this.btnDock.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDock.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnDock.Location = new System.Drawing.Point(312, 56);
            this.btnDock.Name = "btnDock";
            this.btnDock.Size = new System.Drawing.Size(121, 61);
            this.btnDock.TabIndex = 2;
            this.btnDock.Text = "DOCK";
            this.btnDock.UseVisualStyleBackColor = false;
            this.btnDock.Click += new System.EventHandler(this.btnMuteAlarm_Click);
            // 
            // btnFlow
            // 
            this.btnFlow.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnFlow.Enabled = false;
            this.btnFlow.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFlow.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnFlow.Location = new System.Drawing.Point(566, 56);
            this.btnFlow.Name = "btnFlow";
            this.btnFlow.Size = new System.Drawing.Size(121, 61);
            this.btnFlow.TabIndex = 4;
            this.btnFlow.Text = "FLOW";
            this.btnFlow.UseVisualStyleBackColor = false;
            this.btnFlow.Click += new System.EventHandler(this.btnMuteAlarm_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label1.Location = new System.Drawing.Point(58, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(302, 20);
            this.label1.TabIndex = 5;
            this.label1.Text = "Click on Below Enabled Button";
            // 
            // KeyPadForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(747, 161);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnFlow);
            this.Controls.Add(this.btnPurge);
            this.Controls.Add(this.btnDock);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.btnMuteAlarm);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "KeyPadForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "KeyPadForm";
            this.Load += new System.EventHandler(this.KeyPadForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnMuteAlarm;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Button btnPurge;
        private System.Windows.Forms.Button btnDock;
        private System.Windows.Forms.Button btnFlow;
        private System.Windows.Forms.Label label1;
    }
}