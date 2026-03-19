using System;
using System.IO;
using Xunit;

namespace ATRun.Tests
{
    // Redirects AppSettingsStore to an isolated temp directory for the duration of the test class.
    // The real %LOCALAPPDATA%\ATRun path is never touched.
    public sealed class AppSettingsFileFixture : IDisposable
    {
        private readonly string _tempDir;

        public AppSettingsFileFixture()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "ATRunTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
            AppSettingsStore.SettingsDirectoryOverride = _tempDir;
        }

        public void Dispose()
        {
            AppSettingsStore.SettingsDirectoryOverride = null;
            try
            {
                if (Directory.Exists(_tempDir))
                    Directory.Delete(_tempDir, recursive: true);
            }
            catch { /* best-effort cleanup */ }
        }

        // Helper to write arbitrary content to the settings file.
        public void WriteRaw(string content)
        {
            File.WriteAllText(Path.Combine(_tempDir, "settings.json"), content);
        }

        public void DeleteFile()
        {
            string path = Path.Combine(_tempDir, "settings.json");
            if (File.Exists(path))
                File.Delete(path);
        }

        public void DeleteDirectory()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }
    }

    public class AppSettingsStoreTests : IClassFixture<AppSettingsFileFixture>
    {
        private readonly AppSettingsFileFixture _fixture;

        public AppSettingsStoreTests(AppSettingsFileFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void SaveAndLoad_English_RoundTrips()
        {
            AppSettingsStore.SaveLanguage(AppLanguage.English);
            bool loaded = AppSettingsStore.TryLoadLanguage(out var language);

            Assert.True(loaded);
            Assert.Equal(AppLanguage.English, language);
        }

        [Fact]
        public void SaveAndLoad_Russian_RoundTrips()
        {
            AppSettingsStore.SaveLanguage(AppLanguage.Russian);
            bool loaded = AppSettingsStore.TryLoadLanguage(out var language);

            Assert.True(loaded);
            Assert.Equal(AppLanguage.Russian, language);
        }

        [Fact]
        public void TryLoadLanguage_FileDoesNotExist_ReturnsFalse()
        {
            _fixture.DeleteFile();
            bool loaded = AppSettingsStore.TryLoadLanguage(out _);
            Assert.False(loaded);
        }

        [Fact]
        public void TryLoadLanguage_CorruptJson_ReturnsFalse()
        {
            _fixture.WriteRaw("{ invalid json content !!!}");
            bool loaded = AppSettingsStore.TryLoadLanguage(out _);
            Assert.False(loaded);
        }

        [Fact]
        public void TryLoadLanguage_UnknownLanguageValue_ReturnsFalse()
        {
            _fixture.WriteRaw("{\"Language\":\"Klingon\"}");
            bool loaded = AppSettingsStore.TryLoadLanguage(out _);
            Assert.False(loaded);
        }

        [Fact]
        public void TryLoadLanguage_LowercaseValue_ReturnsTrueAndParsesCorrectly()
        {
            // Enum.TryParse is called with ignoreCase: true
            _fixture.WriteRaw("{\"Language\":\"english\"}");
            bool loaded = AppSettingsStore.TryLoadLanguage(out var language);

            Assert.True(loaded);
            Assert.Equal(AppLanguage.English, language);
        }

        [Fact]
        public void SaveLanguage_WhenDirectoryMissing_CreatesFileWithoutThrowing()
        {
            _fixture.DeleteDirectory();
            // Should not throw
            AppSettingsStore.SaveLanguage(AppLanguage.Russian);

            bool loaded = AppSettingsStore.TryLoadLanguage(out var language);
            Assert.True(loaded);
            Assert.Equal(AppLanguage.Russian, language);
        }
    }
}
