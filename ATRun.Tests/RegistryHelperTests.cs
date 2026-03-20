using System;
using System.Collections.Generic;
using System.Security.Principal;
using Microsoft.Win32;
using Xunit;

namespace ATRun.Tests
{
    // Redirects RegistryHelper to an isolated HKCU test key for the duration of the test class.
    // The real HKCU\...\Run key is never touched.
    public sealed class RegistryTestFixture : IDisposable
    {
        private const string Prefix = "ATRunTest_";
        private const string TestAutorunKey = @"Software\ATRunTests\Run";

        public RegistryTestFixture()
        {
            RegistryHelper.AutorunKeyOverride = TestAutorunKey;
            Registry.CurrentUser.CreateSubKey(TestAutorunKey)?.Dispose();
        }

        public string NewEntryName() => Prefix + Guid.NewGuid().ToString("N");

        public void Dispose()
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\ATRunTests", throwOnMissingSubKey: false);
            }
            catch { /* best-effort cleanup */ }
            finally
            {
                RegistryHelper.AutorunKeyOverride = null;
            }
        }
    }

    public class RegistryHelperTests : IClassFixture<RegistryTestFixture>
    {
        private readonly RegistryTestFixture _fixture;

        public RegistryHelperTests(RegistryTestFixture fixture)
        {
            _fixture = fixture;
        }

        // ── FindExistingEntry ─────────────────────────────────────────────────

        [Fact]
        public void FindExistingEntry_NotRegistered_ReturnsZero()
        {
            int result = RegistryHelper.FindExistingEntry(@"C:\ATRunNeverRegistered_" + Guid.NewGuid() + ".exe");
            Assert.Equal(0, result);
        }

        [Fact]
        public void FindExistingEntry_AfterHkcuWrite_ReturnsOne()
        {
            string name = _fixture.NewEntryName();
            string path = @"C:\TestApps\" + name + ".exe";
            var entry = new AutorunEntry(name, path, AutorunHive.CurrentUser);

            RegistryHelper.WriteEntry(entry);
            int result = RegistryHelper.FindExistingEntry(path);

            Assert.Equal(1, result);
        }

        [Fact]
        public void FindExistingEntry_CaseInsensitive_ReturnsOne()
        {
            string name = _fixture.NewEntryName();
            string pathLower = @"c:\testapps\" + name.ToLowerInvariant() + ".exe";
            string pathUpper = @"C:\TestApps\" + name.ToUpperInvariant() + ".EXE";

            var entry = new AutorunEntry(name, pathLower, AutorunHive.CurrentUser);
            RegistryHelper.WriteEntry(entry);

            int result = RegistryHelper.FindExistingEntry(pathUpper);

            Assert.Equal(1, result);
        }

        [Fact]
        public void FindExistingEntry_PathWithSpaces_QuotesAreStripped_ReturnsOne()
        {
            string name = _fixture.NewEntryName();
            // Path with a space — WriteEntry will wrap it in quotes when storing
            string path = @"C:\My Test Apps\" + name + ".exe";
            var entry = new AutorunEntry(name, path, AutorunHive.CurrentUser);

            RegistryHelper.WriteEntry(entry);
            // Search with the bare path (no surrounding quotes)
            int result = RegistryHelper.FindExistingEntry(path);

            Assert.Equal(1, result);
        }

        // ── WriteEntry value format ───────────────────────────────────────────

        [Fact]
        public void WriteEntry_PathWithoutSpaces_StoredWithoutQuotes()
        {
            string name = _fixture.NewEntryName();
            string path = @"C:\TestApps\" + name + ".exe";
            var entry = new AutorunEntry(name, path, AutorunHive.CurrentUser);

            RegistryHelper.WriteEntry(entry);

            using var key = Registry.CurrentUser.OpenSubKey(RegistryHelper.AutorunKeyOverride!)!;
            string? stored = key.GetValue(name) as string;
            Assert.Equal(path, stored);
        }

        [Fact]
        public void WriteEntry_PathWithSpaces_StoredWrappedInQuotes()
        {
            string name = _fixture.NewEntryName();
            string path = @"C:\My Test Apps\" + name + ".exe";
            var entry = new AutorunEntry(name, path, AutorunHive.CurrentUser);

            RegistryHelper.WriteEntry(entry);

            using var key = Registry.CurrentUser.OpenSubKey(RegistryHelper.AutorunKeyOverride!)!;
            string? stored = key.GetValue(name) as string;
            Assert.Equal($"\"{path}\"", stored);
        }

        // ── ReadAllEntries ────────────────────────────────────────────────────

        [Fact]
        public void ReadAllEntries_IncludesWrittenEntry()
        {
            string name = _fixture.NewEntryName();
            string path = @"C:\TestApps\" + name + ".exe";
            var entry = new AutorunEntry(name, path, AutorunHive.CurrentUser);

            RegistryHelper.WriteEntry(entry);
            List<AutorunEntry> all = RegistryHelper.ReadAllEntries();

            Assert.Contains(all, e => e.Name == name && e.Hive == AutorunHive.CurrentUser);
        }

        [Fact]
        public void ReadAllEntries_MultipleEntries_AllReturned()
        {
            var names = new[] { _fixture.NewEntryName(), _fixture.NewEntryName(), _fixture.NewEntryName() };
            foreach (var n in names)
                RegistryHelper.WriteEntry(new AutorunEntry(n, @"C:\TestApps\" + n + ".exe", AutorunHive.CurrentUser));

            List<AutorunEntry> all = RegistryHelper.ReadAllEntries();

            foreach (var n in names)
                Assert.Contains(all, e => e.Name == n);
        }

        // ── DeleteEntry ───────────────────────────────────────────────────────

        [Fact]
        public void DeleteEntry_ExistingEntry_ReturnsTrue()
        {
            string name = _fixture.NewEntryName();
            var entry = new AutorunEntry(name, @"C:\TestApps\" + name + ".exe", AutorunHive.CurrentUser);
            RegistryHelper.WriteEntry(entry);

            bool deleted = RegistryHelper.DeleteEntry(entry);

            Assert.True(deleted);
        }

        [Fact]
        public void DeleteEntry_ExistingEntry_EntryGone()
        {
            string name = _fixture.NewEntryName();
            string path = @"C:\TestApps\" + name + ".exe";
            var entry = new AutorunEntry(name, path, AutorunHive.CurrentUser);
            RegistryHelper.WriteEntry(entry);

            RegistryHelper.DeleteEntry(entry);

            Assert.Equal(0, RegistryHelper.FindExistingEntry(path));
        }

        [Fact]
        public void WriteEntry_ThenOverwrite_NewValueStored()
        {
            string name = _fixture.NewEntryName();
            string path1 = @"C:\TestApps\" + name + "_v1.exe";
            string path2 = @"C:\TestApps\" + name + "_v2.exe";

            RegistryHelper.WriteEntry(new AutorunEntry(name, path1, AutorunHive.CurrentUser));
            RegistryHelper.WriteEntry(new AutorunEntry(name, path2, AutorunHive.CurrentUser));

            using var key = Registry.CurrentUser.OpenSubKey(RegistryHelper.AutorunKeyOverride!)!;
            string? stored = key.GetValue(name) as string;
            Assert.Equal(path2, stored);
        }

        [Fact]
        public void DeleteEntry_NonExistentName_ReturnsTrue()
        {
            // Never written — DeleteValue uses throwOnMissingValue: false, so the key
            // opens successfully and returns true regardless of whether the value existed.
            string name = _fixture.NewEntryName();
            var entry = new AutorunEntry(name, @"C:\TestApps\" + name + ".exe", AutorunHive.CurrentUser);

            bool result = RegistryHelper.DeleteEntry(entry);

            Assert.True(result);
        }

        // ── HKLM without elevation ────────────────────────────────────────────

        [Fact]
        public void WriteEntry_HklmWithoutElevation_ThrowsSecurityOrUnauthorizedAccessException()
        {
            // GitHub Actions and other CI runners execute as admin, so HKLM writes succeed.
            bool isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);
            if (isAdmin)
                return;

            string name = _fixture.NewEntryName();
            var entry = new AutorunEntry(name, @"C:\TestApps\" + name + ".exe", AutorunHive.LocalMachine);

            // Windows throws SecurityException or UnauthorizedAccessException depending on OS version/policy.
            Exception? ex = Record.Exception(() => RegistryHelper.WriteEntry(entry));
            Assert.NotNull(ex);
            Assert.True(ex is UnauthorizedAccessException or System.Security.SecurityException,
                $"Expected SecurityException or UnauthorizedAccessException, got {ex.GetType().Name}");
        }
    }
}
