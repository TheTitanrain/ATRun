using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace AddToAutorun
{
    public partial class MainForm : Form
    {
        // ── Colors ────────────────────────────────────────────────────────────
        private static readonly Color AccentBlue    = Color.FromArgb(0, 103, 192);
        private static readonly Color AccentHover   = Color.FromArgb(0,  90, 158);
        private static readonly Color SuccessGreen  = Color.FromArgb(16, 124,  16);
        private static readonly Color ErrorRed      = Color.FromArgb(196, 43,  28);
        private static readonly Color BorderGray    = Color.FromArgb(189, 189, 189);
        private static readonly Color HklmPurple    = Color.FromArgb(135, 100, 184);

        // ── State ─────────────────────────────────────────────────────────────
        private string?       _filePath;
        private bool          _isDragOver;
        private bool          _updatingLanguageSelector;
        private AutorunHive   _hive = AutorunHive.CurrentUser;
        private readonly Timer _notifyTimer = new() { Interval = 3000 };

        // ── Constructor ───────────────────────────────────────────────────────
        public MainForm()
        {
            InitializeComponent();

            LocalizationManager.LanguageChanged += HandleLanguageChanged;
            InitializeLanguageSelector();
            ApplyLocalization();

            _notifyTimer.Tick += (_, _) => { _notifyTimer.Stop(); pnlNotify.Visible = false; };

            DragEnter += MainForm_DragEnter;
            DragOver  += MainForm_DragOver;
            DragLeave += MainForm_DragLeave;
            DragDrop  += MainForm_DragDrop;

            foreach (Control c in new Control[] { pnlDropZone, lblDropIcon, lblDropHint, lnkBrowse })
            {
                c.DragEnter += MainForm_DragEnter;
                c.DragOver  += MainForm_DragOver;
                c.DragLeave += MainForm_DragLeave;
                c.DragDrop  += MainForm_DragDrop;
            }

            Resize += (_, _) => LayoutAdaptiveSections();

            RefreshHiveButtons();
            RefreshAddButton();
            LayoutAdaptiveSections();
        }

        // ── Form lifecycle ───────────────────────────────────────────────────
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && File.Exists(args[1]))
                ProcessFile(args[1]);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            LocalizationManager.LanguageChanged -= HandleLanguageChanged;
            base.OnFormClosed(e);
        }

        private void HandleLanguageChanged(object? sender, EventArgs e)
        {
            if (ShellHelper.IsRegisteredInSendTo())
            {
                try { ShellHelper.RegisterInSendTo(Application.ExecutablePath); }
                catch { }
            }

            ApplyLocalization();
        }

        // ── Language selector ────────────────────────────────────────────────
        private void InitializeLanguageSelector()
        {
            cmbLanguage.SelectedIndexChanged += CmbLanguage_SelectedIndexChanged;
            RefreshLanguageSelector();
        }

        private void CmbLanguage_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_updatingLanguageSelector || cmbLanguage.SelectedItem is not LanguageOption option)
                return;

            LocalizationManager.SetLanguage(option.Language);
        }

        private void RefreshLanguageSelector()
        {
            _updatingLanguageSelector = true;
            cmbLanguage.Items.Clear();

            foreach (var language in LocalizationManager.Supported)
                cmbLanguage.Items.Add(new LanguageOption(language, LocalizationManager.GetLanguageDisplayName(language)));

            for (int i = 0; i < cmbLanguage.Items.Count; i++)
            {
                if (cmbLanguage.Items[i] is LanguageOption option && option.Language == LocalizationManager.CurrentLanguage)
                {
                    cmbLanguage.SelectedIndex = i;
                    break;
                }
            }

            _updatingLanguageSelector = false;
        }

        private void ApplyLocalization()
        {
            Text = LocalizationManager.Get("App.Title");
            lblTitle.Text = LocalizationManager.Get("App.Title");
            lblSubtitle.Text = LocalizationManager.Get("MainForm.Subtitle");
            lblDropHint.Text = LocalizationManager.Get("MainForm.DropHint");
            lnkBrowse.Text = LocalizationManager.Get("MainForm.BrowseLink");
            lblHiveLabel.Text = LocalizationManager.Get("MainForm.HiveLabel");
            btnHkcu.Text = LocalizationManager.Get("MainForm.Hkcu");
            btnHklm.Text = LocalizationManager.Get("MainForm.Hklm");
            btnAdd.Text = LocalizationManager.Get("MainForm.AddButton");
            btnManage.Text = LocalizationManager.Get("MainForm.ManageButton");
            lblLanguage.Text = LocalizationManager.Get("MainForm.LanguageLabel");

            RefreshLanguageSelector();
            RefreshHiveButtons();
            RefreshSendToButton();
            LayoutAdaptiveSections();
        }

        // ── Drag & drop ───────────────────────────────────────────────────────
        private void MainForm_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            {
                e.Effect     = DragDropEffects.Copy;
                _isDragOver  = true;
                pnlDropZone.Invalidate();
            }
        }

        private void MainForm_DragOver(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                e.Effect = DragDropEffects.Copy;
        }

        private void MainForm_DragLeave(object? sender, EventArgs e)
        {
            _isDragOver = false;
            pnlDropZone.Invalidate();
        }

        private void MainForm_DragDrop(object? sender, DragEventArgs e)
        {
            _isDragOver = false;
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
                ProcessFile(files[0]);
        }

        // ── Drop zone painting ────────────────────────────────────────────────
        private void PnlDropZone_Paint(object? sender, PaintEventArgs e)
        {
            var g      = e.Graphics;
            var rect   = new Rectangle(1, 1, pnlDropZone.Width - 3, pnlDropZone.Height - 3);
            var color  = _isDragOver ? AccentBlue : BorderGray;
            float width = _isDragOver ? 2f : 1.5f;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(color, width) { DashStyle = DashStyle.Dash };
            DrawRoundedRect(g, pen, rect, 8);
        }

        private void PnlFileCard_Paint(object? sender, PaintEventArgs e)
        {
            var rect = new Rectangle(0, 0, pnlFileCard.Width - 1, pnlFileCard.Height - 1);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(Color.FromArgb(210, 210, 210));
            DrawRoundedRect(e.Graphics, pen, rect, 6);
        }

        // ── File browse ───────────────────────────────────────────────────────
        private void PnlDropZone_Click(object? sender, EventArgs e) => OpenFileBrowser();

        private void OpenFileBrowser()
        {
            using var dlg = new OpenFileDialog
            {
                Title  = LocalizationManager.Get("MainForm.FileDialogTitle"),
                Filter = LocalizationManager.Get("MainForm.FileDialogFilter"),
            };
            if (dlg.ShowDialog(this) == DialogResult.OK)
                ProcessFile(dlg.FileName);
        }

        // ── Process file ──────────────────────────────────────────────────────
        private void ProcessFile(string path)
        {
            // Resolve .lnk shortcuts
            if (string.Equals(Path.GetExtension(path), ".lnk", StringComparison.OrdinalIgnoreCase))
            {
                try   { path = ShellHelper.ResolveShortcut(path); }
                catch { ShowNotification(LocalizationManager.Get("MainForm.ShortcutReadError"), success: false); return; }
            }

            if (!File.Exists(path))
            {
                ShowNotification(LocalizationManager.Get("MainForm.FileNotFound"), success: false);
                return;
            }

            if (!FileConstants.SupportedExtensions.Contains(Path.GetExtension(path)))
            {
                ShowNotification(LocalizationManager.Get("MainForm.InvalidFile"), success: false);
                return;
            }

            _filePath = path;

            // Icon (48×48 from system image list)
            try
            {
                var icon = ShellHelper.ExtractLargeIcon(path);
                picFileIcon.Image = icon?.ToBitmap() ?? SystemIcons.Application.ToBitmap();
            }
            catch { picFileIcon.Image = SystemIcons.Application.ToBitmap(); }

            lblFileName.Text = ShellHelper.GetFileDescription(path);
            lblFilePath.Text = path;

            pnlDropZone.Visible = false;
            pnlFileCard.Visible = true;

            RefreshAddButton();
        }

        // ── Clear file ────────────────────────────────────────────────────────
        private void BtnClear_Click(object? sender, EventArgs e)
        {
            _filePath           = null;
            picFileIcon.Image   = null;
            pnlFileCard.Visible = false;
            pnlDropZone.Visible = true;
            pnlNotify.Visible   = false;
            RefreshAddButton();
        }

        // ── Hive toggle ───────────────────────────────────────────────────────
        private void BtnHkcu_Click(object? sender, EventArgs e) { _hive = AutorunHive.CurrentUser;  RefreshHiveButtons(); }
        private void BtnHklm_Click(object? sender, EventArgs e) { _hive = AutorunHive.LocalMachine; RefreshHiveButtons(); }

        private void LayoutAdaptiveSections()
        {
            LayoutHiveButtons();
            LayoutFooterButtons();
        }

        private void LayoutHiveButtons()
        {
            const int buttonTop = 34;
            const int buttonGap = 8;
            int buttonWidth = (pnlHive.ClientSize.Width - buttonGap) / 2;

            btnHkcu.Location = new Point(0, buttonTop);
            btnHkcu.Size = new Size(buttonWidth, btnHkcu.Height);

            btnHklm.Location = new Point(btnHkcu.Right + buttonGap, buttonTop);
            btnHklm.Size = new Size(pnlHive.ClientSize.Width - btnHklm.Left, btnHklm.Height);
        }

        private void LayoutFooterButtons()
        {
            const int horizontalPadding = 12;
            const int buttonGap = 8;
            int availableWidth = pnlFooter.ClientSize.Width - (horizontalPadding * 2);
            int buttonWidth = (availableWidth - buttonGap) / 2;

            btnManage.Location = new Point(horizontalPadding, btnManage.Top);
            btnManage.Size = new Size(buttonWidth, btnManage.Height);

            btnSendTo.Location = new Point(btnManage.Right + buttonGap, btnSendTo.Top);
            btnSendTo.Size = new Size(pnlFooter.ClientSize.Width - horizontalPadding - btnSendTo.Left, btnSendTo.Height);
        }

        private void RefreshHiveButtons()
        {
            StyleToggleButton(btnHkcu, _hive == AutorunHive.CurrentUser,  AccentBlue);
            StyleToggleButton(btnHklm, _hive == AutorunHive.LocalMachine, HklmPurple);
        }

        private static void StyleToggleButton(Button btn, bool selected, Color activeColor)
        {
            if (selected)
            {
                btn.BackColor = activeColor;
                btn.ForeColor = Color.White;
                btn.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
                btn.FlatAppearance.BorderColor        = activeColor;
                btn.FlatAppearance.MouseOverBackColor = activeColor;
            }
            else
            {
                btn.BackColor = Color.FromArgb(232, 232, 232);
                btn.ForeColor = Color.FromArgb(60, 60, 60);
                btn.Font      = new Font("Segoe UI", 9f);
                btn.FlatAppearance.BorderColor        = Color.FromArgb(200, 200, 200);
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(218, 218, 218);
            }
        }

        // ── Add button ────────────────────────────────────────────────────────
        private void RefreshAddButton()
        {
            bool hasFile = _filePath != null;
            btnAdd.Enabled   = hasFile;
            btnAdd.BackColor = hasFile ? AccentBlue : Color.FromArgb(180, 180, 180);
            btnAdd.ForeColor = Color.White;
            btnAdd.FlatAppearance.MouseOverBackColor = hasFile ? AccentHover : Color.FromArgb(180, 180, 180);
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (_filePath == null) return;

            int existing = RegistryHelper.FindExistingEntry(_filePath);
            if (existing == 1)
            {
                ShowNotification(LocalizationManager.Get("MainForm.AlreadyCurrentUser"), success: false);
                return;
            }
            if (existing == 2)
            {
                ShowNotification(LocalizationManager.Get("MainForm.AlreadyAllUsers"), success: false);
                return;
            }

            var entry = new AutorunEntry(
                Path.GetFileNameWithoutExtension(_filePath),
                _filePath,
                _hive);

            try
            {
                RegistryHelper.WriteEntry(entry);
                ShowNotification(LocalizationManager.Get("MainForm.AddSuccess"), success: true);
                // Reset to accept another file
                BtnClear_Click(null, EventArgs.Empty);
            }
            catch (UnauthorizedAccessException)
            {
                ShowNotification(LocalizationManager.Get("MainForm.RegistryAccessDenied"), success: false);
            }
            catch (Exception ex)
            {
                ShowNotification(LocalizationManager.Format("MainForm.ErrorPrefix", ex.Message), success: false);
            }
        }

        // ── Notification bar ──────────────────────────────────────────────────
        private void ShowNotification(string message, bool success)
        {
            _notifyTimer.Stop();
            lblNotify.Text         = message;
            pnlNotify.BackColor    = success ? SuccessGreen : ErrorRed;
            pnlNotify.Visible      = true;
            _notifyTimer.Start();
        }

        // ── Manage autorun ────────────────────────────────────────────────────
        private void BtnManage_Click(object? sender, EventArgs e)
        {
            using var form = new AutorunListForm();
            form.ShowDialog(this);
        }

        // ── SendTo ────────────────────────────────────────────────────────────
        private void BtnSendTo_Click(object? sender, EventArgs e)
        {
            if (ShellHelper.IsRegisteredInSendTo())
            {
                ShellHelper.UnregisterFromSendTo();
            }
            else
            {
                try   { ShellHelper.RegisterInSendTo(Application.ExecutablePath); }
                catch (Exception ex)
                {
                    ShowNotification(LocalizationManager.Format("MainForm.SendToError", ex.Message), success: false);
                    return;
                }
            }
            RefreshSendToButton();
        }

        private void RefreshSendToButton()
        {
            btnSendTo.Text = ShellHelper.IsRegisteredInSendTo()
                ? LocalizationManager.Get("MainForm.SendToRemoveButton")
                : LocalizationManager.Get("MainForm.SendToAddButton");
        }

        // ── About ─────────────────────────────────────────────────────────────
        private void BtnAbout_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                LocalizationManager.Get("MainForm.AboutMessage"),
                LocalizationManager.Get("MainForm.AboutTitle"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // ── Helper: rounded rectangle ─────────────────────────────────────────
        private static void DrawRoundedRect(Graphics g, Pen pen, Rectangle rect, int r)
        {
            using var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, r * 2, r * 2, 180, 90);
            path.AddArc(rect.Right - r * 2, rect.Y, r * 2, r * 2, 270, 90);
            path.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();
            g.DrawPath(pen, path);
        }

        private sealed class LanguageOption
        {
            public LanguageOption(AppLanguage language, string displayName)
            {
                Language = language;
                DisplayName = displayName;
            }

            public AppLanguage Language { get; }

            public string DisplayName { get; }

            public override string ToString() => DisplayName;
        }
    }
}

