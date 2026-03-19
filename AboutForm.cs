using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ATRun
{
    public partial class AboutForm : Form
    {
        private const string ReleasesApiUrl  = "https://api.github.com/repos/TheTitanrain/ATRun/releases/latest";
        private const string ReleasesPageUrl = "https://github.com/TheTitanrain/ATRun/releases";

        private static readonly HttpClient _http = new();

        public AboutForm()
        {
            InitializeComponent();

            var appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (appIcon != null)
            {
                Icon = appIcon;
                picIcon.Image = new Bitmap(appIcon.ToBitmap(), 48, 48);
            }

            LocalizationManager.LanguageChanged += HandleLanguageChanged;
            ApplyLocalization();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            LocalizationManager.LanguageChanged -= HandleLanguageChanged;
            base.OnFormClosed(e);
        }

        private void HandleLanguageChanged(object? sender, EventArgs e) => ApplyLocalization();

        private void ApplyLocalization()
        {
            Text                    = LocalizationManager.Get("About.Title");
            lblAppName.Text         = LocalizationManager.Get("App.Title");
            lblVersion.Text         = LocalizationManager.Format("About.Version", GetVersion());
            lblDescription.Text     = LocalizationManager.Get("About.Description");
            btnClose.Text           = LocalizationManager.Get("About.CloseButton");
            if (btnCheckUpdates.Enabled)
                btnCheckUpdates.Text = LocalizationManager.Get("About.CheckUpdatesButton");
        }

        private async void BtnCheckUpdates_Click(object? sender, EventArgs e)
        {
            btnCheckUpdates.Enabled = false;
            btnCheckUpdates.Text    = LocalizationManager.Get("About.UpdateChecking");

            try
            {
                var tag = await FetchLatestTagAsync();
                var latestVersion  = tag?.TrimStart('v');
                var currentVersion = GetVersion();

                if (latestVersion != null
                    && Version.TryParse(latestVersion, out var latest)
                    && Version.TryParse(currentVersion, out var current)
                    && latest > current)
                {
                    var msg    = LocalizationManager.Format("About.UpdateAvailable", tag ?? latestVersion);
                    var result = MessageBox.Show(msg,
                        LocalizationManager.Get("About.UpdateCheckTitle"),
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                        Process.Start(new ProcessStartInfo(ReleasesPageUrl) { UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show(
                        LocalizationManager.Get("About.UpdatesUpToDate"),
                        LocalizationManager.Get("About.UpdateCheckTitle"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch
            {
                MessageBox.Show(
                    LocalizationManager.Get("About.UpdateCheckFailed"),
                    LocalizationManager.Get("About.UpdateCheckTitle"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            finally
            {
                btnCheckUpdates.Enabled = true;
                btnCheckUpdates.Text    = LocalizationManager.Get("About.CheckUpdatesButton");
            }
        }

        private static async Task<string?> FetchLatestTagAsync()
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, ReleasesApiUrl);
            req.Headers.UserAgent.ParseAdd("ATRun");
            using var resp = await _http.SendAsync(req);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("tag_name").GetString();
        }

        private static string GetVersion()
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            return v is null ? "1.0.0" : $"{v.Major}.{v.Minor}.{v.Build}";
        }
    }
}
