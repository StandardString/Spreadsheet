namespace SS
{
    partial class DebugForm
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
            this.Dialog = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // Dialog
            // 
            this.Dialog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Dialog.BackColor = System.Drawing.SystemColors.Window;
            this.Dialog.Location = new System.Drawing.Point(13, 12);
            this.Dialog.Multiline = true;
            this.Dialog.Name = "Dialog";
            this.Dialog.ReadOnly = true;
            this.Dialog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.Dialog.Size = new System.Drawing.Size(494, 171);
            this.Dialog.TabIndex = 0;
            this.Dialog.WordWrap = false;
            this.Dialog.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Dialog_KeyDown);
            // 
            // DebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(519, 195);
            this.Controls.Add(this.Dialog);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DebugForm";
            this.Text = "Debug";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DebugForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Dialog;
    }
}