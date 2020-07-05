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
            this.Log = new System.Windows.Forms.TextBox();
            this.DrawPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // Log
            // 
            this.Log.Dock = System.Windows.Forms.DockStyle.Left;
            this.Log.Location = new System.Drawing.Point(0, 0);
            this.Log.MinimumSize = new System.Drawing.Size(200, 200);
            this.Log.Multiline = true;
            this.Log.Name = "Log";
            this.Log.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Log.Size = new System.Drawing.Size(450, 536);
            this.Log.TabIndex = 0;
            // 
            // DrawPanel
            // 
            this.DrawPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DrawPanel.Location = new System.Drawing.Point(450, 0);
            this.DrawPanel.MinimumSize = new System.Drawing.Size(200, 200);
            this.DrawPanel.Name = "DrawPanel";
            this.DrawPanel.Size = new System.Drawing.Size(726, 536);
            this.DrawPanel.TabIndex = 1;
            this.DrawPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.DrawPanel_Paint);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1176, 536);
            this.Controls.Add(this.DrawPanel);
            this.Controls.Add(this.Log);
            this.MinimumSize = new System.Drawing.Size(1200, 600);
            this.Name = "Form1";
            this.Text = "Windows Process Clock";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Log;
        private System.Windows.Forms.Panel DrawPanel;
    }
}

