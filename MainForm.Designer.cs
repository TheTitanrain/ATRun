using System.Drawing;
using System.Windows.Forms;

namespace AddToAutorun
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        // ── Header ────────────────────────────────────────────────────────────
        private Panel      pnlHeader;
        private Label      lblTitle;
        private Label      lblSubtitle;
        private Label      lblLanguage;
        private ComboBox   cmbLanguage;
        private Button     btnAbout;

        // ── Drop zone (shown when no file loaded) ─────────────────────────────
        private Panel      pnlDropZone;
        private Label      lblDropIcon;
        private Label      lblDropHint;
        private LinkLabel  lnkBrowse;

        // ── File card (shown when file is loaded) ─────────────────────────────
        private Panel      pnlFileCard;
        private PictureBox picFileIcon;
        private Label      lblFileName;
        private Label      lblFilePath;
        private Button     btnClear;

        // ── Hive selector ─────────────────────────────────────────────────────
        private Panel      pnlHive;
        private Label      lblHiveLabel;
        private Button     btnHkcu;
        private Button     btnHklm;

        // ── Action ────────────────────────────────────────────────────────────
        private Button     btnAdd;

        // ── Notification bar (auto-dismisses) ────────────────────────────────
        private Panel      pnlNotify;
        private Label      lblNotify;

        // ── Footer ────────────────────────────────────────────────────────────
        private Panel      pnlFooter;
        private Button     btnManage;
        private Button     btnSendTo;

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
            ClientSize      = new Size(460, 410);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            StartPosition   = FormStartPosition.CenterScreen;
            Text            = "Startup Applications";
            BackColor       = Color.FromArgb(243, 243, 243);
            DoubleBuffered  = true;
            AllowDrop       = true;
            Font            = new Font("Segoe UI", 9f);

            // ── Header ────────────────────────────────────────────────────────
            pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 64,
                BackColor = Color.White,
            };
            pnlHeader.Paint += (_, e) =>
            {
                using var p = new Pen(Color.FromArgb(225, 225, 225));
                e.Graphics.DrawLine(p, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            };

            lblTitle = new Label
            {
                Text      = "Startup Applications",
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 26, 26),
                AutoSize  = true,
                Location  = new Point(16, 10),
                BackColor = Color.Transparent,
            };
            lblSubtitle = new Label
            {
                Text      = "Add .exe files to the Windows startup list",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(115, 115, 115),
                AutoSize  = true,
                Location  = new Point(16, 38),
                BackColor = Color.Transparent,
            };
            lblLanguage = new Label
            {
                Text      = "Language",
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(115, 115, 115),
                AutoSize  = true,
                BackColor = Color.Transparent,
            };
            cmbLanguage = new ComboBox
            {
                DropDownStyle  = ComboBoxStyle.DropDownList,
                Size           = new Size(128, 23),
                Font           = new Font("Segoe UI", 9f),
                IntegralHeight = false,
            };
            btnAbout = MakeIconButton("ℹ", Point.Empty);
            btnAbout.Click += BtnAbout_Click;

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblLanguage);
            pnlHeader.Controls.Add(cmbLanguage);
            pnlHeader.Controls.Add(btnAbout);
            pnlHeader.ClientSizeChanged += (_, _) => LayoutHeader();

            // ── Drop zone ─────────────────────────────────────────────────────
            pnlDropZone = new Panel
            {
                Location  = new Point(16, 74),
                Size      = new Size(428, 100),
                BackColor = Color.Transparent,
                Cursor    = Cursors.Hand,
                AllowDrop = true,
            };
            pnlDropZone.Paint += PnlDropZone_Paint;
            pnlDropZone.Click += PnlDropZone_Click;

            lblDropIcon = new Label
            {
                Text      = "⬇",
                Font      = new Font("Segoe UI", 18f),
                AutoSize  = false,
                Size      = new Size(428, 36),
                Location  = new Point(0, 10),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(160, 160, 160),
                BackColor = Color.Transparent,
                Cursor    = Cursors.Hand,
                AllowDrop = true,
            };
            lblDropHint = new Label
            {
                Text      = "Drop a file here",
                AutoSize  = false,
                Size      = new Size(428, 24),
                Location  = new Point(0, 48),
                TextAlign = ContentAlignment.MiddleCenter,
                Font      = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(115, 115, 115),
                BackColor = Color.Transparent,
                Cursor    = Cursors.Hand,
                AllowDrop = true,
            };
            lnkBrowse = new LinkLabel
            {
                Text      = "or choose a file",
                AutoSize  = false,
                Size      = new Size(428, 20),
                Location  = new Point(0, 76),
                TextAlign = ContentAlignment.MiddleCenter,
                Font      = new Font("Segoe UI", 9f),
                LinkColor = Color.FromArgb(0, 103, 192),
                BackColor = Color.Transparent,
                AllowDrop = true,
            };

            lblDropIcon.Click += PnlDropZone_Click;
            lblDropHint.Click += PnlDropZone_Click;
            lnkBrowse.LinkClicked += (_, _) => OpenFileBrowser();

            pnlDropZone.Controls.Add(lblDropIcon);
            pnlDropZone.Controls.Add(lblDropHint);
            pnlDropZone.Controls.Add(lnkBrowse);

            // ── File card ─────────────────────────────────────────────────────
            pnlFileCard = new Panel
            {
                Location  = new Point(16, 74),
                Size      = new Size(428, 100),
                BackColor = Color.White,
                Visible   = false,
            };
            pnlFileCard.Paint += PnlFileCard_Paint;

            picFileIcon = new PictureBox
            {
                Size     = new Size(48, 48),
                Location = new Point(12, 26),
                SizeMode = PictureBoxSizeMode.Zoom,
            };
            lblFileName = new Label
            {
                AutoSize  = false,
                Size      = new Size(298, 26),
                Location  = new Point(72, 18),
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 26, 26),
                BackColor = Color.Transparent,
            };
            lblFilePath = new Label
            {
                AutoSize  = false,
                Size      = new Size(298, 40),
                Location  = new Point(72, 48),
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(115, 115, 115),
                BackColor = Color.Transparent,
            };
            // Clear button: fixed position, right-centre of the card
            btnClear = new Button
            {
                Text      = "✕",
                Size      = new Size(32, 32),
                Location  = new Point(428 - 32 - 10, (100 - 32) / 2),  // (386, 34)
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10f),
                Cursor    = Cursors.Hand,
                BackColor = Color.FromArgb(235, 235, 235),
                ForeColor = Color.FromArgb(80, 80, 80),
                TabStop   = false,
            };
            btnClear.FlatAppearance.BorderColor        = Color.FromArgb(200, 200, 200);
            btnClear.FlatAppearance.MouseOverBackColor = Color.FromArgb(215, 215, 215);
            btnClear.Click += BtnClear_Click;

            pnlFileCard.Controls.Add(picFileIcon);
            pnlFileCard.Controls.Add(lblFileName);
            pnlFileCard.Controls.Add(lblFilePath);
            pnlFileCard.Controls.Add(btnClear);

            // ── Hive selector ─────────────────────────────────────────────────
            pnlHive = new Panel
            {
                Location  = new Point(16, 183),
                Size      = new Size(428, 74),
                BackColor = Color.Transparent,
            };

            lblHiveLabel = new Label
            {
                Text      = "Add for:",
                AutoSize  = true,
                Location  = new Point(0, 0),
                ForeColor = Color.FromArgb(80, 80, 80),
                BackColor = Color.Transparent,
            };

            btnHkcu = new Button
            {
                Text      = "Current user",
                Size      = new Size(210, 32),
                Location  = new Point(0, 34),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
            };
            btnHkcu.FlatAppearance.BorderSize = 1;
            btnHkcu.Click += BtnHkcu_Click;

            btnHklm = new Button
            {
                Text      = "All users",
                Size      = new Size(210, 32),
                Location  = new Point(218, 34),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
            };
            btnHklm.FlatAppearance.BorderSize = 1;
            btnHklm.Click += BtnHklm_Click;

            pnlHive.Controls.Add(lblHiveLabel);
            pnlHive.Controls.Add(btnHkcu);
            pnlHive.Controls.Add(btnHklm);

            // ── Add button ────────────────────────────────────────────────────
            btnAdd = new Button
            {
                Text      = "Add to startup",
                Location  = new Point(16, 267),
                Size      = new Size(428, 42),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                Enabled   = false,
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;

            // ── Notification bar ──────────────────────────────────────────────
            pnlNotify = new Panel
            {
                Location  = new Point(16, 317),
                Size      = new Size(428, 32),
                Visible   = false,
            };
            lblNotify = new Label
            {
                AutoSize  = false,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
            };
            pnlNotify.Controls.Add(lblNotify);

            // ── Footer ────────────────────────────────────────────────────────
            pnlFooter = new Panel
            {
                Location  = new Point(0, 358),
                Size      = new Size(460, 52),
                BackColor = Color.White,
            };
            pnlFooter.Paint += (_, e) =>
            {
                using var p = new Pen(Color.FromArgb(225, 225, 225));
                e.Graphics.DrawLine(p, 0, 0, pnlFooter.Width, 0);
            };

            btnManage = new Button
            {
                Text      = "Manage startup",
                Location  = new Point(12, 10),
                Size      = new Size(204, 32),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(0, 103, 192),
                BackColor = Color.Transparent,
                Cursor    = Cursors.Hand,
            };
            btnManage.FlatAppearance.BorderSize = 0;
            btnManage.FlatAppearance.MouseOverBackColor = Color.FromArgb(230, 240, 255);
            btnManage.Click += BtnManage_Click;

            btnSendTo = new Button
            {
                Text      = "Add to 'Send to' menu",
                Location  = new Point(244, 10),
                Size      = new Size(204, 32),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(0, 103, 192),
                BackColor = Color.Transparent,
                Cursor    = Cursors.Hand,
            };
            btnSendTo.FlatAppearance.BorderSize = 0;
            btnSendTo.FlatAppearance.MouseOverBackColor = Color.FromArgb(230, 240, 255);
            btnSendTo.Click += BtnSendTo_Click;

            pnlFooter.Controls.Add(btnManage);
            pnlFooter.Controls.Add(btnSendTo);

            // ── Add everything ────────────────────────────────────────────────
            Controls.Add(pnlHeader);
            Controls.Add(pnlDropZone);
            Controls.Add(pnlFileCard);
            Controls.Add(pnlHive);
            Controls.Add(btnAdd);
            Controls.Add(pnlNotify);
            Controls.Add(pnlFooter);

            ResumeLayout(false);
        }

        private static Button MakeIconButton(string text, Point location) => new Button
        {
            Text      = text,
            Size      = new Size(28, 28),
            Location  = location,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 9f),
            Cursor    = Cursors.Hand,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(80, 80, 80),
            TabStop   = false,
        };
    }
}
