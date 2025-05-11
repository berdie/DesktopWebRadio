namespace DesktopWebRadio
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
            stationComboBox = new ComboBox();
            materialPlayBtn = new MaterialSkin.Controls.MaterialButton();
            materialStopBtn = new MaterialSkin.Controls.MaterialButton();
            materialThemeBtn = new MaterialSkin.Controls.MaterialButton();
            volumeTrackBar = new TrackBar();
            visualizerModeComboBox = new ComboBox();
            visualizerPanel = new Panel();
            labelMetadata = new Label();
            pictureAlbumBox = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)volumeTrackBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureAlbumBox).BeginInit();
            SuspendLayout();
            // 
            // stationComboBox
            // 
            stationComboBox.FormattingEnabled = true;
            stationComboBox.Location = new Point(18, 81);
            stationComboBox.Name = "stationComboBox";
            stationComboBox.Size = new Size(262, 23);
            stationComboBox.Sorted = true;
            stationComboBox.TabIndex = 0;
            // 
            // materialPlayBtn
            // 
            materialPlayBtn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            materialPlayBtn.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            materialPlayBtn.Depth = 0;
            materialPlayBtn.HighEmphasis = true;
            materialPlayBtn.Icon = null;
            materialPlayBtn.Location = new Point(19, 124);
            materialPlayBtn.Margin = new Padding(4, 6, 4, 6);
            materialPlayBtn.MouseState = MaterialSkin.MouseState.HOVER;
            materialPlayBtn.Name = "materialPlayBtn";
            materialPlayBtn.NoAccentTextColor = Color.Empty;
            materialPlayBtn.Size = new Size(64, 36);
            materialPlayBtn.TabIndex = 1;
            materialPlayBtn.Text = "PLAY";
            materialPlayBtn.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            materialPlayBtn.UseAccentColor = false;
            materialPlayBtn.UseVisualStyleBackColor = true;
            // 
            // materialStopBtn
            // 
            materialStopBtn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            materialStopBtn.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            materialStopBtn.Depth = 0;
            materialStopBtn.HighEmphasis = true;
            materialStopBtn.Icon = null;
            materialStopBtn.Location = new Point(91, 124);
            materialStopBtn.Margin = new Padding(4, 6, 4, 6);
            materialStopBtn.MouseState = MaterialSkin.MouseState.HOVER;
            materialStopBtn.Name = "materialStopBtn";
            materialStopBtn.NoAccentTextColor = Color.Empty;
            materialStopBtn.Size = new Size(64, 36);
            materialStopBtn.TabIndex = 1;
            materialStopBtn.Text = "PLAY";
            materialStopBtn.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            materialStopBtn.UseAccentColor = false;
            materialStopBtn.UseVisualStyleBackColor = true;
            // 
            // materialThemeBtn
            // 
            materialThemeBtn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            materialThemeBtn.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            materialThemeBtn.Depth = 0;
            materialThemeBtn.HighEmphasis = true;
            materialThemeBtn.Icon = null;
            materialThemeBtn.Location = new Point(210, 124);
            materialThemeBtn.Margin = new Padding(4, 6, 4, 6);
            materialThemeBtn.MouseState = MaterialSkin.MouseState.HOVER;
            materialThemeBtn.Name = "materialThemeBtn";
            materialThemeBtn.NoAccentTextColor = Color.Empty;
            materialThemeBtn.Size = new Size(70, 36);
            materialThemeBtn.TabIndex = 1;
            materialThemeBtn.Text = "THEME";
            materialThemeBtn.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            materialThemeBtn.UseAccentColor = false;
            materialThemeBtn.UseVisualStyleBackColor = true;
            // 
            // volumeTrackBar
            // 
            volumeTrackBar.Location = new Point(18, 169);
            volumeTrackBar.Name = "volumeTrackBar";
            volumeTrackBar.Size = new Size(262, 45);
            volumeTrackBar.TabIndex = 2;
            // 
            // visualizerModeComboBox
            // 
            visualizerModeComboBox.FormattingEnabled = true;
            visualizerModeComboBox.Location = new Point(18, 231);
            visualizerModeComboBox.Name = "visualizerModeComboBox";
            visualizerModeComboBox.Size = new Size(121, 23);
            visualizerModeComboBox.TabIndex = 3;
            // 
            // visualizerPanel
            // 
            visualizerPanel.BackColor = Color.MidnightBlue;
            visualizerPanel.Location = new Point(19, 273);
            visualizerPanel.Name = "visualizerPanel";
            visualizerPanel.Size = new Size(261, 81);
            visualizerPanel.TabIndex = 4;
            // 
            // labelMetadata
            // 
            labelMetadata.AutoSize = true;
            labelMetadata.Font = new Font("Roboto Condensed", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelMetadata.Location = new Point(18, 369);
            labelMetadata.Name = "labelMetadata";
            labelMetadata.Size = new Size(76, 19);
            labelMetadata.TabIndex = 5;
            labelMetadata.Text = "Loading...";
            labelMetadata.Click += labelMetadata_Click;
            // 
            // pictureAlbumBox
            // 
            pictureAlbumBox.Location = new Point(54, 409);
            pictureAlbumBox.Name = "pictureAlbumBox";
            pictureAlbumBox.Size = new Size(185, 163);
            pictureAlbumBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureAlbumBox.TabIndex = 6;
            pictureAlbumBox.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(299, 604);
            Controls.Add(pictureAlbumBox);
            Controls.Add(labelMetadata);
            Controls.Add(visualizerPanel);
            Controls.Add(visualizerModeComboBox);
            Controls.Add(volumeTrackBar);
            Controls.Add(materialThemeBtn);
            Controls.Add(materialStopBtn);
            Controls.Add(materialPlayBtn);
            Controls.Add(stationComboBox);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)volumeTrackBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureAlbumBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox stationComboBox;
        private MaterialSkin.Controls.MaterialButton materialPlayBtn;
        private MaterialSkin.Controls.MaterialButton materialStopBtn;
        private MaterialSkin.Controls.MaterialButton materialThemeBtn;
        private TrackBar volumeTrackBar;
        private ComboBox visualizerModeComboBox;
        private Panel visualizerPanel;
        private Label labelMetadata;
        private PictureBox pictureAlbumBox;
    }
}
