using System.Drawing;
using System.Windows.Forms;

namespace AddToAutorun
{
    partial class AutorunListForm
    {
        private System.ComponentModel.IContainer components = null;

        private Panel           pnlHeader;
        private Label           lblTitle;
        private Label           lblCount;
        private Panel           pnlScroll;
        private FlowLayoutPanel flowEntries;
        private Panel           pnlFooter;
        private Button          btnClose;
        private Label           lblEmpty;

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
            ClientSize      = new Size(520, 440);
            MinimumSize     = new Size(520, 340);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            Text            = "Управление автозапуском";
            BackColor       = Color.FromArgb(243, 243, 243);
            DoubleBuffered  = true;
            Font            = new Font("Segoe UI", 9f);

            // ── Header ────────────────────────────────────────────────────────
            pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 56,
                BackColor = Color.White,
            };
            pnlHeader.Paint += (_, e) =>
            {
                using var p = new Pen(Color.FromArgb(225, 225, 225));
                e.Graphics.DrawLine(p, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            };

            lblTitle = new Label
            {
                Text      = "Записи автозапуска",
                Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 26, 26),
                AutoSize  = true,
                Location  = new Point(16, 10),
                BackColor = Color.Transparent,
            };
            lblCount = new Label
            {
                AutoSize  = true,
                Location  = new Point(16, 34),
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(115, 115, 115),
                BackColor = Color.Transparent,
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblCount);

            // ── Scrollable entry list ─────────────────────────────────────────
            pnlScroll = new Panel
            {
                Dock       = DockStyle.Fill,
                AutoScroll = true,
                BackColor  = Color.FromArgb(243, 243, 243),
                Padding    = new Padding(0, 8, 0, 8),
            };

            flowEntries = new FlowLayoutPanel
            {
                Dock          = DockStyle.Top,
                AutoSize      = true,
                AutoSizeMode  = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents  = false,
                BackColor     = Color.Transparent,
                Padding       = new Padding(0),
            };

            lblEmpty = new Label
            {
                Text      = "Список автозапуска пуст.",
                AutoSize  = false,
                Size      = new Size(520, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                Font      = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(150, 150, 150),
                BackColor = Color.Transparent,
                Visible   = false,
            };

            pnlScroll.Controls.Add(flowEntries);
            pnlScroll.Controls.Add(lblEmpty);

            // ── Footer ────────────────────────────────────────────────────────
            pnlFooter = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 52,
                BackColor = Color.White,
            };
            pnlFooter.Paint += (_, e) =>
            {
                using var p = new Pen(Color.FromArgb(225, 225, 225));
                e.Graphics.DrawLine(p, 0, 0, pnlFooter.Width, 0);
            };

            btnClose = new Button
            {
                Text      = "Закрыть",
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

            pnlFooter.Controls.Add(btnClose);

            // ── Assemble ──────────────────────────────────────────────────────
            Controls.Add(pnlScroll);
            Controls.Add(pnlHeader);
            Controls.Add(pnlFooter);

            ResumeLayout(false);
        }
    }
}
