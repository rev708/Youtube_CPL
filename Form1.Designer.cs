namespace yt_panel
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            header = new Panel();
            statusLabel = new Label();
            topMostButton = new Button();
            closeButton = new Button();
            albumArtBox = new PictureBox();
            trackLabel = new Label();
            previousButton = new Button();
            playPauseButton = new Button();
            nextButton = new Button();
            volumeSlider = new RoundVolumeSlider();
            header.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)albumArtBox).BeginInit();
            SuspendLayout();
            // 
            // header
            // 
            header.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            header.BackColor = Color.Black;
            header.Controls.Add(statusLabel);
            header.Controls.Add(topMostButton);
            header.Controls.Add(closeButton);
            header.Cursor = Cursors.SizeAll;
            header.Location = new Point(0, 0);
            header.Name = "header";
            header.Size = new Size(460, 40);
            header.TabIndex = 0;
            // 
            // statusLabel
            // 
            statusLabel.AutoEllipsis = true;
            statusLabel.AutoSize = false;
            statusLabel.Font = new Font("Segoe UI", 8F);
            statusLabel.ForeColor = Color.FromArgb(115, 115, 115);
            statusLabel.Location = new Point(16, 10);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(260, 20);
            statusLabel.TabIndex = 0;
            statusLabel.Text = "Ready";
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // topMostButton
            // 
            topMostButton.BackColor = Color.Black;
            topMostButton.Cursor = Cursors.Hand;
            topMostButton.FlatAppearance.BorderSize = 0;
            topMostButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(36, 36, 36);
            topMostButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(22, 22, 22);
            topMostButton.FlatStyle = FlatStyle.Flat;
            topMostButton.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            topMostButton.ForeColor = Color.White;
            topMostButton.Location = new Point(340, 5);
            topMostButton.Name = "topMostButton";
            topMostButton.Size = new Size(64, 30);
            topMostButton.TabIndex = 1;
            topMostButton.Text = "Pin";
            topMostButton.UseVisualStyleBackColor = false;
            // 
            // closeButton
            // 
            closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            closeButton.BackColor = Color.Black;
            closeButton.Cursor = Cursors.Hand;
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(36, 36, 36);
            closeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(22, 22, 22);
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            closeButton.ForeColor = Color.White;
            closeButton.Location = new Point(428, 5);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(28, 30);
            closeButton.TabIndex = 2;
            closeButton.Text = "X";
            closeButton.UseVisualStyleBackColor = false;
            // 
            // albumArtBox
            // 
            albumArtBox.BackColor = Color.FromArgb(8, 8, 8);
            albumArtBox.Cursor = Cursors.Hand;
            albumArtBox.Location = new Point(175, 44);
            albumArtBox.Name = "albumArtBox";
            albumArtBox.Size = new Size(110, 110);
            albumArtBox.SizeMode = PictureBoxSizeMode.Zoom;
            albumArtBox.TabIndex = 2;
            albumArtBox.TabStop = false;
            // 
            // trackLabel
            // 
            trackLabel.AutoEllipsis = true;
            trackLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            trackLabel.ForeColor = Color.White;
            trackLabel.Location = new Point(70, 162);
            trackLabel.Name = "trackLabel";
            trackLabel.Size = new Size(320, 28);
            trackLabel.TabIndex = 3;
            trackLabel.Text = "Click album to start";
            trackLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // previousButton
            // 
            previousButton.BackColor = Color.Black;
            previousButton.Cursor = Cursors.Hand;
            previousButton.FlatAppearance.BorderSize = 0;
            previousButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(36, 36, 36);
            previousButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(18, 18, 18);
            previousButton.FlatStyle = FlatStyle.Flat;
            previousButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            previousButton.ForeColor = Color.White;
            previousButton.Location = new Point(106, 200);
            previousButton.Name = "previousButton";
            previousButton.Size = new Size(72, 36);
            previousButton.TabIndex = 4;
            previousButton.Text = "<<";
            previousButton.UseVisualStyleBackColor = false;
            // 
            // playPauseButton
            // 
            playPauseButton.BackColor = Color.Black;
            playPauseButton.Cursor = Cursors.Hand;
            playPauseButton.FlatAppearance.BorderSize = 0;
            playPauseButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(36, 36, 36);
            playPauseButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(18, 18, 18);
            playPauseButton.FlatStyle = FlatStyle.Flat;
            playPauseButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            playPauseButton.ForeColor = Color.White;
            playPauseButton.Location = new Point(194, 200);
            playPauseButton.Name = "playPauseButton";
            playPauseButton.Size = new Size(72, 36);
            playPauseButton.TabIndex = 5;
            playPauseButton.Text = "\u25B6";
            playPauseButton.UseVisualStyleBackColor = false;
            // 
            // nextButton
            // 
            nextButton.BackColor = Color.Black;
            nextButton.Cursor = Cursors.Hand;
            nextButton.FlatAppearance.BorderSize = 0;
            nextButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(36, 36, 36);
            nextButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(18, 18, 18);
            nextButton.FlatStyle = FlatStyle.Flat;
            nextButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            nextButton.ForeColor = Color.White;
            nextButton.Location = new Point(282, 200);
            nextButton.Name = "nextButton";
            nextButton.Size = new Size(72, 36);
            nextButton.TabIndex = 6;
            nextButton.Text = ">>";
            nextButton.UseVisualStyleBackColor = false;
            // 
            // volumeSlider
            // 
            volumeSlider.BackColor = Color.Black;
            volumeSlider.FillColor = Color.White;
            volumeSlider.Location = new Point(124, 247);
            volumeSlider.MinimumSize = new Size(90, 28);
            volumeSlider.Name = "volumeSlider";
            volumeSlider.Size = new Size(212, 28);
            volumeSlider.TabIndex = 8;
            volumeSlider.ThumbBorderColor = Color.FromArgb(20, 20, 20);
            volumeSlider.ThumbColor = Color.White;
            volumeSlider.ThumbRadius = 9;
            volumeSlider.TrackColor = Color.FromArgb(42, 42, 42);
            volumeSlider.TrackHeight = 5;
            // 
            // Form1
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.Black;
            ClientSize = new Size(460, 282);
            Controls.Add(header);
            Controls.Add(albumArtBox);
            Controls.Add(trackLabel);
            Controls.Add(previousButton);
            Controls.Add(playPauseButton);
            Controls.Add(nextButton);
            Controls.Add(volumeSlider);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(440, 260);
            Name = "Form1";
            Opacity = 0.96D;
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "YT Panel";
            TopMost = true;
            header.ResumeLayout(false);
            header.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)albumArtBox).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel header;
        private Label statusLabel;
        private Button topMostButton;
        private Button closeButton;
        private PictureBox albumArtBox;
        private Label trackLabel;
        private Button previousButton;
        private Button playPauseButton;
        private Button nextButton;
        private RoundVolumeSlider volumeSlider;
    }
}
