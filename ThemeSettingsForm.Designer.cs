namespace DesktopWebRadio
{
    partial class ThemeSettingsForm
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
            this.cmbTheme = new System.Windows.Forms.ComboBox();
            this.cmbPrimaryColor = new System.Windows.Forms.ComboBox();
            this.cmbAccentColor = new System.Windows.Forms.ComboBox();
            this.btnApply = new MaterialSkin.Controls.MaterialButton();
            this.lblTheme = new MaterialSkin.Controls.MaterialLabel();
            this.lblPrimaryColor = new MaterialSkin.Controls.MaterialLabel();
            this.lblAccentColor = new MaterialSkin.Controls.MaterialLabel();
            this.SuspendLayout();
            // 
            // cmbTheme
            // 
            this.cmbTheme.FormattingEnabled = true;
            this.cmbTheme.Location = new System.Drawing.Point(148, 91);
            this.cmbTheme.Name = "cmbTheme";
            this.cmbTheme.Size = new System.Drawing.Size(180, 23);
            this.cmbTheme.TabIndex = 0;
            // 
            // cmbPrimaryColor
            // 
            this.cmbPrimaryColor.FormattingEnabled = true;
            this.cmbPrimaryColor.Location = new System.Drawing.Point(148, 136);
            this.cmbPrimaryColor.Name = "cmbPrimaryColor";
            this.cmbPrimaryColor.Size = new System.Drawing.Size(180, 23);
            this.cmbPrimaryColor.TabIndex = 1;
            // 
            // cmbAccentColor
            // 
            this.cmbAccentColor.FormattingEnabled = true;
            this.cmbAccentColor.Location = new System.Drawing.Point(148, 181);
            this.cmbAccentColor.Name = "cmbAccentColor";
            this.cmbAccentColor.Size = new System.Drawing.Size(180, 23);
            this.cmbAccentColor.TabIndex = 2;
            // 
            // btnApply
            // 
            this.btnApply.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnApply.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnApply.Depth = 0;
            this.btnApply.HighEmphasis = true;
            this.btnApply.Icon = null;
            this.btnApply.Location = new System.Drawing.Point(138, 230);
            this.btnApply.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnApply.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnApply.Name = "btnApply";
            this.btnApply.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnApply.Size = new System.Drawing.Size(77, 36);
            this.btnApply.TabIndex = 3;
            this.btnApply.Text = "Apply";
            this.btnApply.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnApply.UseAccentColor = false;
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // lblTheme
            // 
            this.lblTheme.AutoSize = true;
            this.lblTheme.Depth = 0;
            this.lblTheme.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblTheme.Location = new System.Drawing.Point(23, 91);
            this.lblTheme.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblTheme.Name = "lblTheme";
            this.lblTheme.Size = new System.Drawing.Size(54, 19);
            this.lblTheme.TabIndex = 4;
            this.lblTheme.Text = "Theme:";
            // 
            // lblPrimaryColor
            // 
            this.lblPrimaryColor.AutoSize = true;
            this.lblPrimaryColor.Depth = 0;
            this.lblPrimaryColor.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblPrimaryColor.Location = new System.Drawing.Point(23, 136);
            this.lblPrimaryColor.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblPrimaryColor.Name = "lblPrimaryColor";
            this.lblPrimaryColor.Size = new System.Drawing.Size(100, 19);
            this.lblPrimaryColor.TabIndex = 5;
            this.lblPrimaryColor.Text = "Primary Color:";
            // 
            // lblAccentColor
            // 
            this.lblAccentColor.AutoSize = true;
            this.lblAccentColor.Depth = 0;
            this.lblAccentColor.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblAccentColor.Location = new System.Drawing.Point(23, 181);
            this.lblAccentColor.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblAccentColor.Name = "lblAccentColor";
            this.lblAccentColor.Size = new System.Drawing.Size(97, 19);
            this.lblAccentColor.TabIndex = 6;
            this.lblAccentColor.Text = "Accent Color:";
            // 
            // ThemeSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(352, 290);
            this.Controls.Add(this.lblAccentColor);
            this.Controls.Add(this.lblPrimaryColor);
            this.Controls.Add(this.lblTheme);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.cmbAccentColor);
            this.Controls.Add(this.cmbPrimaryColor);
            this.Controls.Add(this.cmbTheme);
            this.MaximizeBox = false;
            this.Name = "ThemeSettingsForm";
            this.Text = "Theme Settings";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ComboBox cmbTheme;
        private System.Windows.Forms.ComboBox cmbPrimaryColor;
        private System.Windows.Forms.ComboBox cmbAccentColor;
        private MaterialSkin.Controls.MaterialButton btnApply;
        private MaterialSkin.Controls.MaterialLabel lblTheme;
        private MaterialSkin.Controls.MaterialLabel lblPrimaryColor;
        private MaterialSkin.Controls.MaterialLabel lblAccentColor;
    }
}