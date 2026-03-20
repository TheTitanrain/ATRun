using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace ATRun
{
    internal enum AppLanguage
    {
        English,
        Russian
    }

    internal static class LocalizationManager
    {
        private const string LegacyRussianSendToShortcutName = "Автозапуск приложений.lnk";

        private static readonly AppLanguage[] SupportedLanguages =
        {
            AppLanguage.English,
            AppLanguage.Russian
        };

        private static readonly IReadOnlyDictionary<string, string> EnglishTexts =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["App.Title"] = "ATRun",
                ["MainForm.Subtitle"] = "Add .exe files to the Windows startup list",
                ["MainForm.DropHint"] = "Drop a file here",
                ["MainForm.BrowseLink"] = "or choose a file",
                ["MainForm.HiveLabel"] = "Add for:",
                ["MainForm.Hkcu"] = "Current user",
                ["MainForm.Hklm"] = "All users",
                ["MainForm.AddButton"] = "Add to startup",
                ["MainForm.ManageButton"] = "Manage startup",
                ["MainForm.SendToAddButton"] = "Add to 'Send to' menu",
                ["MainForm.SendToRemoveButton"] = "Remove from 'Send to' menu",
                ["MainForm.LanguageLabel"] = "Language",
                ["MainForm.FileDialogTitle"] = "Select an executable file",
                ["MainForm.FileDialogFilter"] = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                ["MainForm.ShortcutReadError"] = "Could not read the shortcut.",
                ["MainForm.FileNotFound"] = "File not found.",
                ["MainForm.InvalidFile"] = "The file is not an executable (.exe) and cannot be added to startup.",
                ["MainForm.AlreadyCurrentUser"] = "Already added to startup for the current user.",
                ["MainForm.AlreadyAllUsers"] = "Already added to startup for all users.",
                ["MainForm.AddSuccess"] = "✓  Added to startup successfully!",
                ["MainForm.RegistryAccessDenied"] = "Not enough rights to write to the registry. Run as administrator.",
                ["MainForm.ElevatePrompt"] = "Administrator rights are required to add a program to startup for all users.\n\nRestart ATRun as administrator?",
                ["MainForm.ElevateTitle"]  = "Restart as Administrator",
                ["MainForm.ErrorPrefix"] = "Error: {0}",
                ["MainForm.SendToError"] = "Error adding to 'Send to': {0}",
                ["MainForm.AboutTitle"] = "About",
                ["MainForm.AboutMessage"] =
                    "Startup Applications\n\nAdd programs to Windows startup through the registry.\n\nDrag an .exe file into the window or use the 'Send to' menu.",
                ["About.Title"]       = "About",
                ["About.Version"]     = "Version {0}",
                ["About.Description"] = "Add programs to Windows startup through the registry.\nDrag an .exe file into the window or use the 'Send to' menu.",
                ["About.CloseButton"]        = "Close",
                ["About.CheckUpdatesButton"] = "Check for Updates",
                ["About.UpdateChecking"]     = "Checking...",
                ["About.UpdateCheckTitle"]   = "Check for Updates",
                ["About.UpdatesUpToDate"]    = "You have the latest version.",
                ["About.UpdateAvailable"]    = "Version {0} is available. Open the releases page?",
                ["About.UpdateCheckFailed"]  = "Could not check for updates.",
                ["About.Donate"] = "Support the project",
                ["AutorunList.Title"] = "Startup Entries",
                ["AutorunList.Empty"] = "The startup list is empty.",
                ["AutorunList.CountZero"] = "No entries",
                ["AutorunList.CountOne"] = "{0} entry",
                ["AutorunList.CountMany"] = "{0} entries",
                ["AutorunList.DeleteButton"] = "Delete",
                ["AutorunList.ConfirmDelete"] = "Remove '{0}' from startup?",
                ["AutorunList.ConfirmTitle"] = "Confirmation",
                ["AutorunList.DeleteError"] = "Could not delete the entry.",
                ["AutorunList.DeleteErrorTitle"] = "Error",
                ["AutorunList.CloseButton"] = "Close",
                ["Shell.SendToShortcutName"] = "Add to Startup.lnk",
                ["Shell.SendToShortcutDescription"] = "Add the selected application to Windows startup",
            };

        private static readonly IReadOnlyDictionary<string, string> RussianTexts =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["App.Title"] = "ATRun",
                ["MainForm.Subtitle"] = "Добавить .exe в список автозагрузки Windows",
                ["MainForm.DropHint"] = "Перетащите файл сюда",
                ["MainForm.BrowseLink"] = "или выберите файл",
                ["MainForm.HiveLabel"] = "Добавить для:",
                ["MainForm.Hkcu"] = "Текущего пользователя",
                ["MainForm.Hklm"] = "Всех пользователей",
                ["MainForm.AddButton"] = "Добавить в автозапуск",
                ["MainForm.ManageButton"] = "Управление автозапуском",
                ["MainForm.SendToAddButton"] = "Добавить в меню «Отправить»",
                ["MainForm.SendToRemoveButton"] = "Убрать из меню «Отправить»",
                ["MainForm.LanguageLabel"] = "Язык",
                ["MainForm.FileDialogTitle"] = "Выберите исполняемый файл",
                ["MainForm.FileDialogFilter"] = "Исполняемые файлы (*.exe)|*.exe|Все файлы (*.*)|*.*",
                ["MainForm.ShortcutReadError"] = "Не удалось прочитать ярлык.",
                ["MainForm.FileNotFound"] = "Файл не найден.",
                ["MainForm.InvalidFile"] = "Файл не является исполняемым (.exe) и не может быть добавлен в автозапуск.",
                ["MainForm.AlreadyCurrentUser"] = "Уже добавлено в автозапуск текущего пользователя.",
                ["MainForm.AlreadyAllUsers"] = "Уже добавлено в автозапуск для всех пользователей.",
                ["MainForm.AddSuccess"] = "✓  Успешно добавлено в автозапуск!",
                ["MainForm.RegistryAccessDenied"] = "Нет прав для записи в реестр. Запустите от имени администратора.",
                ["MainForm.ElevatePrompt"] = "Для добавления программы в автозапуск всех пользователей необходимы права администратора.\n\nПерезапустить ATRun от имени администратора?",
                ["MainForm.ElevateTitle"]  = "Перезапуск от имени администратора",
                ["MainForm.ErrorPrefix"] = "Ошибка: {0}",
                ["MainForm.SendToError"] = "Ошибка добавления в «Отправить»: {0}",
                ["MainForm.AboutTitle"] = "О программе",
                ["MainForm.AboutMessage"] =
                    "Автозапуск приложений\n\nДобавление программ в автозапуск Windows через реестр.\n\nДостаточно перетащить .exe файл в окно или использовать меню «Отправить».",
                ["About.Title"]       = "О программе",
                ["About.Version"]     = "Версия {0}",
                ["About.Description"] = "Добавление программ в автозапуск Windows через реестр.\nДостаточно перетащить .exe файл в окно или использовать меню «Отправить».",
                ["About.CloseButton"]        = "Закрыть",
                ["About.CheckUpdatesButton"] = "Проверить обновления",
                ["About.UpdateChecking"]     = "Проверка...",
                ["About.UpdateCheckTitle"]   = "Проверка обновлений",
                ["About.UpdatesUpToDate"]    = "У вас последняя версия.",
                ["About.UpdateAvailable"]    = "Доступна версия {0}. Открыть страницу релизов?",
                ["About.UpdateCheckFailed"]  = "Не удалось проверить обновления.",
                ["About.Donate"] = "Поддержать проект",
                ["AutorunList.Title"] = "Записи автозапуска",
                ["AutorunList.Empty"] = "Список автозапуска пуст.",
                ["AutorunList.CountZero"] = "Нет записей",
                ["AutorunList.CountOne"] = "{0} запись",
                ["AutorunList.CountFew"] = "{0} записи",
                ["AutorunList.CountMany"] = "{0} записей",
                ["AutorunList.DeleteButton"] = "Удалить",
                ["AutorunList.ConfirmDelete"] = "Удалить «{0}» из автозапуска?",
                ["AutorunList.ConfirmTitle"] = "Подтверждение",
                ["AutorunList.DeleteError"] = "Не удалось удалить запись.",
                ["AutorunList.DeleteErrorTitle"] = "Ошибка",
                ["AutorunList.CloseButton"] = "Закрыть",
                ["Shell.SendToShortcutName"] = "Добавить в автозапуск.lnk",
                ["Shell.SendToShortcutDescription"] = "Добавить выбранное приложение в автозапуск Windows",
            };

        public static event EventHandler? LanguageChanged;

        public static AppLanguage CurrentLanguage { get; private set; } = AppLanguage.English;

        public static CultureInfo CurrentCulture { get; private set; } = CultureInfo.GetCultureInfo("en-US");

        public static IReadOnlyList<AppLanguage> Supported => SupportedLanguages;

        public static void Initialize()
        {
            var language = AppSettingsStore.TryLoadLanguage(out var savedLanguage)
                ? savedLanguage
                : DetectSystemLanguage();

            ApplyLanguage(language, persist: false, raiseEvent: false);
        }

        public static void SetLanguage(AppLanguage language, bool persist = true)
        {
            bool languageChanged = language != CurrentLanguage;
            ApplyLanguage(language, persist, raiseEvent: languageChanged);
        }

        public static string Get(string key)
        {
            var currentTexts = GetCurrentTexts();
            if (currentTexts.TryGetValue(key, out string? value))
                return value;

            return EnglishTexts.TryGetValue(key, out value) ? value : key;
        }

        public static string Format(string key, params object[] args) =>
            string.Format(CurrentCulture, Get(key), args);

        public static string GetLanguageDisplayName(AppLanguage language) =>
            language switch
            {
                AppLanguage.Russian => "Русский",
                _ => "English",
            };

        public static string GetEntryCountText(int count)
        {
            if (count == 0)
                return Get("AutorunList.CountZero");

            if (CurrentLanguage == AppLanguage.Russian)
            {
                int mod10 = count % 10;
                int mod100 = count % 100;

                if (mod10 == 1 && mod100 != 11)
                    return Format("AutorunList.CountOne", count);

                if (mod10 is >= 2 and <= 4 && (mod100 < 12 || mod100 > 14))
                    return Format("AutorunList.CountFew", count);

                return Format("AutorunList.CountMany", count);
            }

            return count == 1
                ? Format("AutorunList.CountOne", count)
                : Format("AutorunList.CountMany", count);
        }

        public static string GetCurrentSendToShortcutName() => Get("Shell.SendToShortcutName");

        public static string GetCurrentSendToShortcutDescription() => Get("Shell.SendToShortcutDescription");

        public static IReadOnlyCollection<string> GetKnownSendToShortcutNames() =>
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                EnglishTexts["Shell.SendToShortcutName"],
                RussianTexts["Shell.SendToShortcutName"],
                LegacyRussianSendToShortcutName,
            };

        private static void ApplyLanguage(AppLanguage language, bool persist, bool raiseEvent)
        {
            CurrentLanguage = language;
            CurrentCulture = language == AppLanguage.Russian
                ? CultureInfo.GetCultureInfo("ru-RU")
                : CultureInfo.GetCultureInfo("en-US");

            Thread.CurrentThread.CurrentCulture = CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CurrentCulture;
            CultureInfo.DefaultThreadCurrentCulture = CurrentCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CurrentCulture;

            if (persist)
                AppSettingsStore.SaveLanguage(language);

            if (raiseEvent)
                LanguageChanged?.Invoke(null, EventArgs.Empty);
        }

        private static AppLanguage DetectSystemLanguage() =>
            CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("ru", StringComparison.OrdinalIgnoreCase)
                ? AppLanguage.Russian
                : AppLanguage.English;

        private static IReadOnlyDictionary<string, string> GetCurrentTexts() =>
            CurrentLanguage == AppLanguage.Russian ? RussianTexts : EnglishTexts;
    }

    internal static class AppSettingsStore
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        // Overridable in tests to redirect reads/writes away from the real %LOCALAPPDATA%\ATRun path.
        internal static string? SettingsDirectoryOverride { get; set; }

        private static string SettingsDirectory =>
            SettingsDirectoryOverride ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ATRun");

        private static string SettingsPath => Path.Combine(SettingsDirectory, "settings.json");

        public static bool TryLoadLanguage(out AppLanguage language)
        {
            language = default;

            try
            {
                if (!File.Exists(SettingsPath))
                    return false;

                var json = File.ReadAllText(SettingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings?.Language == null)
                    return false;

                return Enum.TryParse(settings.Language, ignoreCase: true, out language);
            }
            catch
            {
                return false;
            }
        }

        public static void SaveLanguage(AppLanguage language)
        {
            try
            {
                Directory.CreateDirectory(SettingsDirectory);
                var settings = new AppSettings
                {
                    Language = language.ToString()
                };

                string json = JsonSerializer.Serialize(settings, JsonOptions);
                File.WriteAllText(SettingsPath, json);
            }
            catch
            {
                // Ignore settings persistence errors and keep the app functional.
            }
        }

        private sealed class AppSettings
        {
            public string? Language { get; set; }
        }
    }
}
