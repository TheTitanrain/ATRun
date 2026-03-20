using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ATRun
{
    public partial class AutorunListForm : Form
    {
        private static readonly Color HkcuBlue   = Color.FromArgb(0,   103, 192);
        private static readonly Color HklmPurple = Color.FromArgb(135, 100, 184);
        private static readonly Color ErrorRed   = Color.FromArgb(196,  43,  28);

        private const int EntryHeight = 58;

        private List<AutorunEntry> _entries = new();

        public AutorunListForm()
        {
            InitializeComponent();
            LocalizationManager.LanguageChanged += HandleLanguageChanged;
            ApplyLocalization();
            Shown += (_, _) => RefreshEntryWidths();
            pnlScroll.ClientSizeChanged += (_, _) => RefreshEntryWidths();
            LoadEntries();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            LocalizationManager.LanguageChanged -= HandleLanguageChanged;
            base.OnFormClosed(e);
        }

        private void HandleLanguageChanged(object? sender, EventArgs e)
        {
            ApplyLocalization();
            LoadEntries();
        }

        private void ApplyLocalization()
        {
            Text = LocalizationManager.Get("AutorunList.Title");
            lblTitle.Text = LocalizationManager.Get("AutorunList.Title");
            lblEmpty.Text = LocalizationManager.Get("AutorunList.Empty");
            btnClose.Text = LocalizationManager.Get("AutorunList.CloseButton");
            UpdateCountLabel();
        }

        // ── Load / refresh ────────────────────────────────────────────────────
        private void LoadEntries()
        {
            _entries = RegistryHelper.ReadAllEntries()
                .OrderBy(e => e.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
            flowEntries.Controls.Clear();

            if (_entries.Count == 0)
            {
                lblEmpty.Visible = true;
                lblEmpty.BringToFront();
            }
            else
            {
                lblEmpty.Visible = false;
                for (int i = 0; i < _entries.Count; i++)
                    flowEntries.Controls.Add(BuildEntryPanel(_entries[i], i));
            }

            UpdateCountLabel();
            RefreshEntryWidths();
        }

        private void UpdateCountLabel()
        {
            lblCount.Text = LocalizationManager.GetEntryCountText(_entries.Count);
        }

        private void RefreshEntryWidths()
        {
            int scrollbarWidth = pnlScroll.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0;
            int width = Math.Max(0, pnlScroll.ClientSize.Width - scrollbarWidth);

            flowEntries.Width = width;
            lblEmpty.Width = width;

            foreach (Control control in flowEntries.Controls)
                control.Width = width;
        }

        // ── Build a single entry row ──────────────────────────────────────────
        private Panel BuildEntryPanel(AutorunEntry entry, int index)
        {
            int width = Math.Max(0, pnlScroll.ClientSize.Width);

            var pnl = new Panel
            {
                Size      = new Size(width, EntryHeight),
                BackColor = index % 2 == 0 ? Color.White : Color.FromArgb(248, 248, 250),
                Tag       = entry,
                Margin    = Padding.Empty,
            };

            // Left color bar (HKCU=blue, HKLM=purple)
            var bar = new Panel
            {
                Location  = new Point(0, 0),
                Size      = new Size(5, EntryHeight),
                BackColor = entry.Hive == AutorunHive.LocalMachine ? HklmPurple : HkcuBlue,
            };

            var lblName = new Label
            {
                Text         = entry.Name,
                Location     = new Point(18, 8),
                AutoSize     = false,
                AutoEllipsis = true,
                Size         = new Size(Math.Max(0, width - 120), 22),
                Anchor       = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font         = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor    = Color.FromArgb(26, 26, 26),
                BackColor    = Color.Transparent,
            };

            var lblPath = new Label
            {
                Text         = entry.FullPath,
                Location     = new Point(18, 32),
                AutoSize     = false,
                AutoEllipsis = true,
                Size         = new Size(Math.Max(0, width - 130), 18),
                Anchor       = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Font         = new Font("Segoe UI", 7.5f),
                ForeColor    = Color.FromArgb(115, 115, 115),
                BackColor    = Color.Transparent,
            };

            var lblHive = new Label
            {
                Text      = entry.Hive == AutorunHive.LocalMachine ? "HKLM" : "HKCU",
                Location  = new Point(Math.Max(18, width - 108), 32),
                AutoSize  = true,
                Anchor    = AnchorStyles.Right | AnchorStyles.Bottom,
                Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = entry.Hive == AutorunHive.LocalMachine ? HklmPurple : HkcuBlue,
                BackColor = Color.Transparent,
            };

            var btnDel = new Button
            {
                Text      = LocalizationManager.Get("AutorunList.DeleteButton"),
                Location  = new Point(Math.Max(18, width - 90), 14),
                Size      = new Size(76, 28),
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                ForeColor = ErrorRed,
                BackColor = Color.Transparent,
                Font      = new Font("Segoe UI", 8f),
                Cursor    = Cursors.Hand,
                Tag       = entry,
            };
            btnDel.FlatAppearance.BorderColor        = ErrorRed;
            btnDel.FlatAppearance.BorderSize         = 1;
            btnDel.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 235, 233);
            btnDel.Click += BtnDelete_Click;

            // Bottom divider
            pnl.Paint += (_, e) =>
            {
                using var p = new Pen(Color.FromArgb(225, 225, 225));
                e.Graphics.DrawLine(p, 0, pnl.Height - 1, pnl.Width, pnl.Height - 1);
            };

            pnl.Controls.Add(bar);
            pnl.Controls.Add(lblName);
            pnl.Controls.Add(lblPath);
            pnl.Controls.Add(lblHive);
            pnl.Controls.Add(btnDel);

            return pnl;
        }

        // ── Delete ────────────────────────────────────────────────────────────
        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not AutorunEntry entry) return;

            string appName = string.IsNullOrEmpty(entry.Name)
                ? Path.GetFileName(entry.FullPath)
                : entry.Name;

            var res = MessageBox.Show(
                LocalizationManager.Format("AutorunList.ConfirmDelete", appName),
                LocalizationManager.Get("AutorunList.ConfirmTitle"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (res != DialogResult.Yes) return;

            if (RegistryHelper.DeleteEntry(entry))
                LoadEntries();
            else
                MessageBox.Show(
                    LocalizationManager.Get("AutorunList.DeleteError"),
                    LocalizationManager.Get("AutorunList.DeleteErrorTitle"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
        }
    }
}
