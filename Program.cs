using System;
using System.IO;
using System.Windows.Forms;

namespace AddToAutorun
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Silent mode: AddToAutorun.exe <filePath> [/hklm]
            if (args.Length > 0)
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
                catch { /* silent — no elevation */ }
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
