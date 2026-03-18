using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace AddToAutorun
{
    internal static class ShellHelper
    {
        // ── .lnk resolution via IShellLink ────────────────────────────────────
        public static string ResolveShortcut(string lnkPath)
        {
            var link = new NativeMethods.ShellLink();
            try
            {
                var shellLink  = (NativeMethods.IShellLinkW)link;
                var persistFile = (NativeMethods.IPersistFile)link;
                persistFile.Load(lnkPath, 0 /* STGM_READ */);
                shellLink.Resolve(IntPtr.Zero, 1 /* SLR_NO_UI */);
                var sb = new StringBuilder(260);
                shellLink.GetPath(sb, sb.Capacity, IntPtr.Zero, 0);
                return sb.ToString();
            }
            finally
            {
                Marshal.ReleaseComObject(link);
            }
        }

        // ── Large (48×48) icon extraction ─────────────────────────────────────
        public static System.Drawing.Icon? ExtractLargeIcon(string filePath)
        {
            var sfi = new NativeMethods.SHFILEINFO();
            var result = NativeMethods.SHGetFileInfo(
                filePath, 0, ref sfi,
                (uint)Marshal.SizeOf(sfi),
                NativeMethods.SHGFI_SYSICONINDEX | NativeMethods.SHGFI_LARGEICON);

            if (result == IntPtr.Zero) return null;

            IntPtr hModule = NativeMethods.LoadLibrary("shell32.dll");
            if (hModule == IntPtr.Zero) return null;

            try
            {
                // SHGetImageList is exported only by ordinal 727
                IntPtr procAddr = NativeMethods.GetProcAddress(hModule, (IntPtr)727);
                if (procAddr == IntPtr.Zero) return null;

                var shGetImageList = Marshal.GetDelegateForFunctionPointer<
                    NativeMethods.SHGetImageListDelegate>(procAddr);

                var iid = NativeMethods.IID_IImageList;
                if (shGetImageList(NativeMethods.SHIL_EXTRALARGE, ref iid, out IntPtr pImageList) != 0)
                    return null;

                IntPtr hIcon = NativeMethods.ImageList_GetIcon(pImageList, sfi.iIcon, 0);
                if (hIcon == IntPtr.Zero) return null;

                // Clone the icon so we can safely destroy the native handle
                var icon = (System.Drawing.Icon)System.Drawing.Icon.FromHandle(hIcon).Clone();
                NativeMethods.DestroyIcon(hIcon);
                return icon;
            }
            finally
            {
                NativeMethods.FreeLibrary(hModule);
            }
        }

        // ── File description from version info ────────────────────────────────
        public static string GetFileDescription(string filePath)
        {
            try
            {
                var desc = FileVersionInfo.GetVersionInfo(filePath).FileDescription;
                if (!string.IsNullOrWhiteSpace(desc)) return desc;
            }
            catch { }
            return Path.GetFileNameWithoutExtension(filePath);
        }

        // ── SendTo shortcut ───────────────────────────────────────────────────
        public static string GetSendToLinkPath() => GetSendToLinkPath(LocalizationManager.GetCurrentSendToShortcutName());

        public static bool IsRegisteredInSendTo()
        {
            foreach (string path in GetKnownSendToLinkPaths())
            {
                if (File.Exists(path))
                    return true;
            }

            return false;
        }

        public static void RegisterInSendTo(string exePath)
        {
            string targetPath = GetSendToLinkPath();
            var link = new NativeMethods.ShellLink();
            try
            {
                var shellLink   = (NativeMethods.IShellLinkW)link;
                var persistFile = (NativeMethods.IPersistFile)link;

                shellLink.SetPath(exePath);
                shellLink.SetDescription(LocalizationManager.GetCurrentSendToShortcutDescription());
                shellLink.SetWorkingDirectory(Path.GetDirectoryName(exePath) ?? "");
                persistFile.Save(targetPath, false);
            }
            finally
            {
                Marshal.ReleaseComObject(link);
            }

            foreach (string path in GetKnownSendToLinkPaths())
            {
                if (!path.Equals(targetPath, StringComparison.OrdinalIgnoreCase) && File.Exists(path))
                    File.Delete(path);
            }
        }

        public static void UnregisterFromSendTo()
        {
            foreach (string path in GetKnownSendToLinkPaths())
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        private static string GetSendToLinkPath(string linkName)
        {
            var sendTo = Environment.GetFolderPath(Environment.SpecialFolder.SendTo);
            return Path.Combine(sendTo, linkName);
        }

        private static IEnumerable<string> GetKnownSendToLinkPaths()
        {
            foreach (string linkName in LocalizationManager.GetKnownSendToShortcutNames())
                yield return GetSendToLinkPath(linkName);
        }
    }
}
