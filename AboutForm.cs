using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ATRun
{
    public partial class AboutForm : Form
    {
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
            Text                = LocalizationManager.Get("About.Title");
            lblTitle.Text       = LocalizationManager.Get("About.Title");
            lblAppName.Text     = LocalizationManager.Get("App.Title");
            lblVersion.Text     = LocalizationManager.Format("About.Version", GetVersion());
            lblDescription.Text = LocalizationManager.Get("About.Description");
            btnClose.Text       = LocalizationManager.Get("About.CloseButton");
        }

        private static string GetVersion()
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            return v is null ? "1.0" : $"{v.Major}.{v.Minor}";
        }
    }
}
