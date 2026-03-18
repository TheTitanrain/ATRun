using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace AddToAutorun
{
    /// <summary>All registry reads/writes for the autorun key.</summary>
    internal static class RegistryHelper
    {
        // ── Check whether a given path is already registered ──────────────────
        /// <returns>
        /// 0 — not found,
        /// 1 — found in HKCU,
        /// 2 — found in HKLM
        /// </returns>
        public static int FindExistingEntry(string fullPath)
        {
            if (ExistsInHive(Registry.CurrentUser, fullPath))
                return 1;
            if (ExistsInHive(Registry.LocalMachine, fullPath))
                return 2;
            return 0;
        }

        private static bool ExistsInHive(RegistryKey hive, string fullPath)
        {
            try
            {
                using var key = hive.OpenSubKey(FileConstants.AutorunRegKey, false);
                if (key == null) return false;
                foreach (var name in key.GetValueNames())
                {
                    var val = key.GetValue(name) as string;
                    if (val != null &&
                        string.Equals(val.Trim('"'), fullPath, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            catch { /* access denied etc. — treat as not found */ }
            return false;
        }

        // ── Read all entries from both hives ──────────────────────────────────
        public static List<AutorunEntry> ReadAllEntries()
        {
            var result = new List<AutorunEntry>();
            ReadFromHive(Registry.CurrentUser,  AutorunHive.CurrentUser,  result);
            ReadFromHive(Registry.LocalMachine, AutorunHive.LocalMachine, result);
            return result;
        }

        private static void ReadFromHive(RegistryKey hive, AutorunHive autorunHive,
                                         List<AutorunEntry> result)
        {
            try
            {
                using var key = hive.OpenSubKey(FileConstants.AutorunRegKey, false);
                if (key == null) return;
                foreach (var name in key.GetValueNames())
                {
                    var val = key.GetValue(name) as string;
                    if (!string.IsNullOrEmpty(val))
                        result.Add(new AutorunEntry(name, val, autorunHive));
                }
            }
            catch { /* no access — skip */ }
        }

        // ── Write ─────────────────────────────────────────────────────────────
        /// <exception cref="UnauthorizedAccessException">
        ///   Thrown when writing to HKLM without elevation.
        /// </exception>
        public static void WriteEntry(AutorunEntry entry)
        {
            var hive = entry.Hive == AutorunHive.LocalMachine
                ? Registry.LocalMachine
                : Registry.CurrentUser;

            using var key = hive.OpenSubKey(FileConstants.AutorunRegKey, true)
                         ?? hive.CreateSubKey(FileConstants.AutorunRegKey);
            var value = entry.FullPath.Contains(' ') ? $"\"{entry.FullPath}\"" : entry.FullPath;
            key.SetValue(entry.Name, value, RegistryValueKind.String);
        }

        // ── Delete ────────────────────────────────────────────────────────────
        public static bool DeleteEntry(AutorunEntry entry)
        {
            var hive = entry.Hive == AutorunHive.LocalMachine
                ? Registry.LocalMachine
                : Registry.CurrentUser;
            try
            {
                using var key = hive.OpenSubKey(FileConstants.AutorunRegKey, true);
                if (key == null) return false;
                key.DeleteValue(entry.Name, throwOnMissingValue: false);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
