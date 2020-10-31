namespace ProcessClock
{
    partial class Form1
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
            this.DrawPanel = new System.Windows.Forms.Panel();
            this.InfoPanel = new System.Windows.Forms.Panel();
            this.LeftContainer = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.LeftContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // DrawPanel
            // 
            this.DrawPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DrawPanel.Location = new System.Drawing.Point(610, 0);
            this.DrawPanel.MinimumSize = new System.Drawing.Size(400, 208);
            this.DrawPanel.Name = "DrawPanel";
            this.DrawPanel.Size = new System.Drawing.Size(1327, 767);
            this.DrawPanel.TabIndex = 3;
            this.DrawPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.DrawPanel_Paint);
            // 
            // InfoPanel
            // 
            this.InfoPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.InfoPanel.Location = new System.Drawing.Point(0, 0);
            this.InfoPanel.MinimumSize = new System.Drawing.Size(200, 500);
            this.InfoPanel.Name = "InfoPanel";
            this.InfoPanel.Size = new System.Drawing.Size(610, 500);
            this.InfoPanel.TabIndex = 4;
            this.InfoPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.InfoPanel_Paint);
            // 
            // LeftContainer
            // 
            this.LeftContainer.Controls.Add(this.panel1);
            this.LeftContainer.Controls.Add(this.InfoPanel);
            this.LeftContainer.Dock = System.Windows.Forms.DockStyle.Left;
            this.LeftContainer.Location = new System.Drawing.Point(0, 0);
            this.LeftContainer.MinimumSize = new System.Drawing.Size(400, 208);
            this.LeftContainer.Name = "LeftContainer";
            this.LeftContainer.Size = new System.Drawing.Size(610, 767);
            this.LeftContainer.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 500);
            this.panel1.MinimumSize = new System.Drawing.Size(200, 267);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(610, 267);
            this.panel1.TabIndex = 5;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1937, 767);
            this.Controls.Add(this.DrawPanel);
            this.Controls.Add(this.LeftContainer);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(1743, 831);
            this.Name = "Form1";
            this.Text = "Windows Process Clock";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.LeftContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel DrawPanel;
        private System.Windows.Forms.Panel InfoPanel;
        private System.Windows.Forms.Panel LeftContainer;
        private System.Windows.Forms.Panel panel1;
    }
}

