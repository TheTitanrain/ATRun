using System;
using System.IO;
using System.Windows.Forms;

namespace ATRun
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            LocalizationManager.Initialize();

            // Silent mode: ATRun.exe <filePath> [/hklm]
            // GUI-elevated mode:  ATRun.exe <filePath> [/hklm] /gui  → skips silent path, opens GUI with file pre-loaded
            bool hasGuiFlag = Array.Exists(args, a => string.Equals(a, "/gui", StringComparison.OrdinalIgnoreCase));

            if (args.Length > 0 && !hasGuiFlag)
            {
                string filePath = args[0];

                // Resolve .lnk shortcuts
                if (string.Equals(Path.GetExtension(filePath), ".lnk", StringComparison.OrdinalIgnoreCase))
                {
                    try { filePath = ShellHelper.ResolveShortcut(filePath); }
                    catch { return; }
                }

                if (!File.Exists(filePath)) return;

                string ext = Path.GetExtension(filePath);
                if (!FileConstants.SupportedExtensions.Contains(ext)) return;

                // Check if already registered
                int existing = RegistryHelper.FindExistingEntry(filePath);
                if (existing != 0) return;  // already there — nothing to do

                bool useHklm = args.Length > 1 &&
                               string.Equals(args[1], "/hklm", StringComparison.OrdinalIgnoreCase);

                var hive = useHklm ? AutorunHive.LocalMachine : AutorunHive.CurrentUser;
                var name = Path.GetFileNameWithoutExtension(filePath);
                var entry = new AutorunEntry(name, filePath, hive);

                try { RegistryHelper.WriteEntry(entry); }
                catch { /* silent — callers expect immediate exit on error */ }
                return;
            }

            // GUI mode
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.Run(new MainForm());
        }
    }
}
