using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AddToAutorun
{
    public partial class AutorunListForm : Form
    {
        private static readonly Color HkcuBlue   = Color.FromArgb(0,   103, 192);
        private static readonly Color HklmPurple = Color.FromArgb(135, 100, 184);
        private static readonly Color ErrorRed   = Color.FromArgb(196,  43,  28);

        private List<AutorunEntry> _entries = new();

        public AutorunListForm()
        {
            InitializeComponent();
            Resize += (_, _) => RefreshEntryWidths();
            LoadEntries();
        }

        // ── Load / refresh ────────────────────────────────────────────────────
        private void LoadEntries()
        {
            _entries = RegistryHelper.ReadAllEntries();
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
        }

        private void UpdateCountLabel()
        {
            lblCount.Text = _entries.Count switch
            {
                0 => "Нет записей",
                1 => "1 запись",
                _ => $"{_entries.Count} записей"
            };
        }

        private void RefreshEntryWidths()
        {
            int w = pnlScroll.ClientSize.Width - (pnlScroll.VerticalScroll.Visible ? 17 : 0);
            flowEntries.Width = w;
            foreach (Control c in flowEntries.Controls)
                c.Width = w;
        }

        // ── Build a single entry row ──────────────────────────────────────────
        private Panel BuildEntryPanel(AutorunEntry entry, int index)
        {
            int w = pnlScroll.ClientSize.Width;

            var pnl = new Panel
            {
                Size      = new Size(w, 58),
                BackColor = index % 2 == 0 ? Color.White : Color.FromArgb(248, 248, 250),
                Tag       = entry,
            };

            // Left color bar (HKCU=blue, HKLM=purple)
            var bar = new Panel
            {
                Location  = new Point(0, 0),
                Size      = new Size(5, 58),
                BackColor = entry.Hive == AutorunHive.LocalMachine ? HklmPurple : HkcuBlue,
            };

            var lblName = new Label
            {
                Text      = entry.Name,
                Location  = new Point(18, 8),
                AutoSize  = false,
                Size      = new Size(w - 120, 22),
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 26, 26),
                BackColor = Color.Transparent,
            };

            var lblPath = new Label
            {
                Text      = entry.FullPath,
                Location  = new Point(18, 32),
                AutoSize  = false,
                Size      = new Size(w - 130, 18),
                Font      = new Font("Segoe UI", 7.5f),
                ForeColor = Color.FromArgb(115, 115, 115),
                BackColor = Color.Transparent,
            };

            var lblHive = new Label
            {
                Text      = entry.Hive == AutorunHive.LocalMachine ? "HKLM" : "HKCU",
                Location  = new Point(w - 108, 32),
                AutoSize  = true,
                Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = entry.Hive == AutorunHive.LocalMachine ? HklmPurple : HkcuBlue,
                BackColor = Color.Transparent,
            };

            var btnDel = new Button
            {
                Text      = "Удалить",
                Location  = new Point(w - 90, 14),
                Size      = new Size(76, 28),
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
                $"Удалить «{appName}» из автозапуска?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (res != DialogResult.Yes) return;

            if (RegistryHelper.DeleteEntry(entry))
                LoadEntries();
            else
                MessageBox.Show("Не удалось удалить запись.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
