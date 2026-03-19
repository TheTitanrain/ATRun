using System.Drawing;
using System.Windows.Forms;

namespace ATRun
{
    partial class AboutForm
    {
        private System.ComponentModel.IContainer components = null;

        private PictureBox picIcon;
        private Label      lblAppName;
        private Label      lblVersion;
        private Label      lblDescription;
        private Panel      pnlFooter;
        private Button     btnClose;
        private Button     btnCheckUpdates;

        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            SuspendLayout();

            // ── Form ──────────────────────────────────────────────────────────
            ClientSize      = new Size(360, 250);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            ShowInTaskbar   = false;
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = Color.FromArgb(243, 243, 243);
            DoubleBuffered  = true;
            Font            = new Font("Segoe UI", 9f);

            // ── App icon ──────────────────────────────────────────────────────
            picIcon = new PictureBox
            {
                Size     = new Size(48, 48),
                Location = new Point(156, 12),
                SizeMode = PictureBoxSizeMode.Zoom,
            };

            // ── App name ──────────────────────────────────────────────────────
            lblAppName = new Label
            {
                AutoSize  = false,
                Size      = new Size(360, 28),
                Location  = new Point(0, 66),
                TextAlign = ContentAlignment.MiddleCenter,
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 26, 26),
                BackColor = Color.Transparent,
            };

            // ── Version ───────────────────────────────────────────────────────
            lblVersion = new Label
            {
                AutoSize  = false,
                Size      = new Size(360, 18),
                Location  = new Point(0, 97),
                TextAlign = ContentAlignment.MiddleCenter,
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(115, 115, 115),
                BackColor = Color.Transparent,
            };

            // ── Description ───────────────────────────────────────────────────
            lblDescription = new Label
            {
                AutoSize  = false,
                Size      = new Size(328, 72),
                Location  = new Point(16, 122),
                TextAlign = ContentAlignment.TopCenter,
                Font      = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(80, 80, 80),
                BackColor = Color.Transparent,
            };

            // ── Footer ────────────────────────────────────────────────────────
            pnlFooter = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 52,
                BackColor = Color.White,
            };
            pnlFooter.Paint += pnlFooter_Paint;

            btnClose = new Button
            {
                Text      = "Close",
                Location  = new Point(12, 10),
                Size      = new Size(100, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(232, 232, 232),
                ForeColor = Color.FromArgb(40, 40, 40),
                Cursor    = Cursors.Hand,
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(215, 215, 215);
            btnClose.Click += (_, _) => Close();

            btnCheckUpdates = new Button
            {
                Text      = "Check for Updates",
                Location  = new Point(198, 10),
                Size      = new Size(150, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(232, 232, 232),
                ForeColor = Color.FromArgb(40, 40, 40),
                Cursor    = Cursors.Hand,
            };
            btnCheckUpdates.FlatAppearance.BorderSize = 0;
            btnCheckUpdates.FlatAppearance.MouseOverBackColor = Color.FromArgb(215, 215, 215);
            btnCheckUpdates.Click += BtnCheckUpdates_Click;

            pnlFooter.Controls.Add(btnClose);
            pnlFooter.Controls.Add(btnCheckUpdates);

            // ── Assemble ──────────────────────────────────────────────────────
            Controls.Add(picIcon);
            Controls.Add(lblAppName);
            Controls.Add(lblVersion);
            Controls.Add(lblDescription);
            Controls.Add(pnlFooter);

            ResumeLayout(false);
        }

        private void pnlFooter_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            using var p = new Pen(Color.FromArgb(225, 225, 225));
            e.Graphics.DrawLine(p, 0, 0, pnlFooter.Width, 0);
        }
    }
}
