using System;
using System.Globalization;
using System.Threading;
using Xunit;

namespace ATRun.Tests
{
    // Restores the original language after each test class run.
    public sealed class LocalizationStateFixture : IDisposable
    {
        private readonly AppLanguage _savedLanguage;
        private readonly CultureInfo _savedCulture;

        public LocalizationStateFixture()
        {
            _savedLanguage = LocalizationManager.CurrentLanguage;
            _savedCulture = LocalizationManager.CurrentCulture;
        }

        public void Dispose()
        {
            LocalizationManager.SetLanguage(_savedLanguage, persist: false);
            Thread.CurrentThread.CurrentCulture = _savedCulture;
            Thread.CurrentThread.CurrentUICulture = _savedCulture;
        }
    }

    public class LocalizationManagerTests : IClassFixture<LocalizationStateFixture>
    {
        // Reset to English before each test so they are independent.
        public LocalizationManagerTests()
        {
            LocalizationManager.SetLanguage(AppLanguage.English, persist: false);
        }

        // ── Get ───────────────────────────────────────────────────────────────

        [Fact]
        public void Get_KnownKey_English_ReturnsEnglishText()
        {
            Assert.Equal("ATRun", LocalizationManager.Get("App.Title"));
        }

        [Fact]
        public void Get_KnownKey_Russian_ReturnsRussianText()
        {
            LocalizationManager.SetLanguage(AppLanguage.Russian, persist: false);
            Assert.Equal("Добавить в автозапуск", LocalizationManager.Get("MainForm.AddButton"));
        }

        [Fact]
        public void Get_UnknownKey_ReturnsKeyItself()
        {
            Assert.Equal("No.Such.Key", LocalizationManager.Get("No.Such.Key"));
        }

        // ── Format ────────────────────────────────────────────────────────────

        [Fact]
        public void Format_English_SubstitutesArgument()
        {
            Assert.Equal("Version 1.0.0", LocalizationManager.Format("About.Version", "1.0.0"));
        }

        [Fact]
        public void Format_Russian_SubstitutesArgument()
        {
            LocalizationManager.SetLanguage(AppLanguage.Russian, persist: false);
            Assert.Equal("Версия 1.0.0", LocalizationManager.Format("About.Version", "1.0.0"));
        }

        // ── SetLanguage ───────────────────────────────────────────────────────

        [Fact]
        public void SetLanguage_ToRussian_UpdatesCurrentLanguage()
        {
            LocalizationManager.SetLanguage(AppLanguage.Russian, persist: false);
            Assert.Equal(AppLanguage.Russian, LocalizationManager.CurrentLanguage);
        }

        [Fact]
        public void SetLanguage_ToRussian_UpdatesCurrentCulture()
        {
            LocalizationManager.SetLanguage(AppLanguage.Russian, persist: false);
            Assert.Equal("ru-RU", LocalizationManager.CurrentCulture.Name);
        }

        [Fact]
        public void SetLanguage_ToEnglish_UpdatesCurrentCulture()
        {
            LocalizationManager.SetLanguage(AppLanguage.Russian, persist: false);
            LocalizationManager.SetLanguage(AppLanguage.English, persist: false);
            Assert.Equal("en-US", LocalizationManager.CurrentCulture.Name);
        }

        [Fact]
        public void SetLanguage_SameLanguage_DoesNotRaiseLanguageChangedEvent()
        {
            LocalizationManager.SetLanguage(AppLanguage.English, persist: false); // ensure English
            int callCount = 0;
            EventHandler handler = (_, _) => callCount++;
            LocalizationManager.LanguageChanged += handler;

            try
            {
                LocalizationManager.SetLanguage(AppLanguage.English, persist: false);
                Assert.Equal(0, callCount);
            }
            finally
            {
                LocalizationManager.LanguageChanged -= handler;
            }
        }

        [Fact]
        public void SetLanguage_DifferentLanguage_RaisesLanguageChangedEventOnce()
        {
            int callCount = 0;
            EventHandler handler = (_, _) => callCount++;
            LocalizationManager.LanguageChanged += handler;

            try
            {
                LocalizationManager.SetLanguage(AppLanguage.Russian, persist: false);
                Assert.Equal(1, callCount);
            }
            finally
            {
                LocalizationManager.LanguageChanged -= handler;
            }
        }

        // ── GetLanguageDisplayName ────────────────────────────────────────────

        [Theory]
        [InlineData((int)AppLanguage.English, "English")]
        [InlineData((int)AppLanguage.Russian, "Русский")]
        public void GetLanguageDisplayName_ReturnsCorrectName(int language, string expected)
        {
            Assert.Equal(expected, LocalizationManager.GetLanguageDisplayName((AppLanguage)language));
        }

        // ── GetEntryCountText — English ───────────────────────────────────────

        [Theory]
        [InlineData(0, "No entries")]
        [InlineData(1, "1 entry")]
        [InlineData(2, "2 entries")]
        [InlineData(5, "5 entries")]
        [InlineData(11, "11 entries")]
        [InlineData(100, "100 entries")]
        public void GetEntryCountText_English(int count, string expected)
        {
            LocalizationManager.SetLanguage(AppLanguage.English, persist: false);
            Assert.Equal(expected, LocalizationManager.GetEntryCountText(count));
        }

        // ── GetEntryCountText — Russian (full pluralization boundary matrix) ──

        [Theory]
        [InlineData(0,   "Нет записей")]   // zero
        [InlineData(1,   "1 запись")]      // mod10=1, mod100=1 → CountOne
        [InlineData(2,   "2 записи")]      // mod10=2           → CountFew
        [InlineData(3,   "3 записи")]      // mod10=3           → CountFew
        [InlineData(4,   "4 записи")]      // mod10=4           → CountFew
        [InlineData(5,   "5 записей")]     // mod10=5           → CountMany
        [InlineData(10,  "10 записей")]    // mod10=0           → CountMany
        [InlineData(11,  "11 записей")]    // mod100=11 → exception → CountMany
        [InlineData(12,  "12 записей")]    // mod100=12 → exception → CountMany
        [InlineData(13,  "13 записей")]    // mod100=13 → exception → CountMany
        [InlineData(14,  "14 записей")]    // mod100=14 → exception → CountMany
        [InlineData(15,  "15 записей")]    // mod10=5           → CountMany
        [InlineData(21,  "21 запись")]     // mod10=1, mod100=21 → CountOne
        [InlineData(22,  "22 записи")]     // mod10=2, mod100=22 → CountFew
        [InlineData(24,  "24 записи")]     // mod10=4, mod100=24 → CountFew
        [InlineData(25,  "25 записей")]    // mod10=5           → CountMany
        [InlineData(100, "100 записей")]   // mod10=0           → CountMany
        [InlineData(101, "101 запись")]    // mod10=1, mod100=1 → CountOne
        [InlineData(111, "111 записей")]   // mod100=11 → exception → CountMany
        [InlineData(112, "112 записей")]   // mod100=12 → exception → CountMany
        [InlineData(121, "121 запись")]    // mod10=1, mod100=21 → CountOne
        [InlineData(1001,"1001 запись")]   // mod10=1, mod100=1 → CountOne
        public void GetEntryCountText_Russian(int count, string expected)
        {
            LocalizationManager.SetLanguage(AppLanguage.Russian, persist: false);
            Assert.Equal(expected, LocalizationManager.GetEntryCountText(count));
        }

        // ── GetKnownSendToShortcutNames ───────────────────────────────────────

        [Fact]
        public void GetKnownSendToShortcutNames_HasExactlyThreeEntries()
        {
            Assert.Equal(3, LocalizationManager.GetKnownSendToShortcutNames().Count);
        }

        [Theory]
        [InlineData("Add to Startup.lnk")]
        [InlineData("Добавить в автозапуск.lnk")]
        [InlineData("Автозапуск приложений.lnk")]
        public void GetKnownSendToShortcutNames_ContainsExpectedNames(string name)
        {
            Assert.Contains(name, LocalizationManager.GetKnownSendToShortcutNames());
        }

        [Fact]
        public void GetKnownSendToShortcutNames_CaseInsensitiveLookup()
        {
            // The collection uses OrdinalIgnoreCase
            Assert.Contains("add to startup.lnk", LocalizationManager.GetKnownSendToShortcutNames());
        }
    }
}
