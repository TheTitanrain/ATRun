# AGENTS.md

This file provides guidance to Codex (Codex.ai/code) when working with code in this repository.

## Build & Run

```bash
# Build (Debug)
dotnet build

# Build (Release)
dotnet build -c Release

# Run
dotnet run

# Run with a file argument (silent mode)
dotnet run -- "C:\path\to\app.exe"

# Run with HKLM flag
dotnet run -- "C:\path\to\app.exe" /hklm
```

Output binary: `bin/Debug/net8.0-windows/ATRun.exe`

## Project Overview

**ATRun** is a Windows Forms app (.NET 8, `net8.0-windows`) that manages Windows startup registry entries under `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` and `HKLM\...`.

Supported file types: `.exe`

## Two Operating Modes

1. **GUI mode** (no args): Opens `MainForm` — drag & drop or browse for a file, pick HKCU/HKLM hive, click Add.
2. **Silent/CLI mode** (with args): `ATRun.exe <filePath> [/hklm]` — registers without showing UI; silently exits if already registered or on error.

## Architecture

| File | Purpose |
|------|---------|
| `Models.cs` | `AutorunEntry` record, `AutorunHive` enum, `FileConstants` (registry key path, supported extensions) |
| `RegistryHelper.cs` | All registry I/O: `FindExistingEntry`, `ReadAllEntries`, `WriteEntry`, `DeleteEntry` |
| `ShellHelper.cs` | Win32 shell helpers: `.lnk` shortcut resolution, large icon extraction (48×48 via `SHGetImageList` ordinal 727), file description from version info, SendTo shortcut management |
| `NativeMethods.cs` | P/Invoke declarations: `SHGetFileInfo`, `ImageList_GetIcon`, `IShellLinkW`/`IPersistFile` COM interfaces for `.lnk` handling |
| `LocalizationManager.cs` | `LocalizationManager` — static string lookup (`Get`/`Format`) and `LanguageChanged` event; `AppSettingsStore` — persists language choice to `%LOCALAPPDATA%\ATRun\settings.json`; `AppLanguage` enum |
| `MainForm.cs` / `MainForm.Designer.cs` | Main UI: drag-drop zone, file card, HKCU/HKLM toggle buttons, Add button, notification bar, "Manage" button, SendTo registration toggle, language selector |
| `AutorunListForm.cs` / `AutorunListForm.Designer.cs` | List of all current autorun entries from both hives with per-entry delete |
| `Program.cs` | Entry point — initializes `LocalizationManager`, then routes to silent mode or GUI mode |

## Key Design Notes

- HKLM writes require elevation; the app runs as `asInvoker` (see `app.manifest`) — callers must UAC-elevate themselves if needed.
- `.lnk` shortcuts are resolved to their target before registration (both in GUI and CLI modes).
- `RegistryHelper` silently swallows access-denied errors when reading; `WriteEntry` lets `UnauthorizedAccessException` bubble up so UI can show a message.
- UI strings are localized in English and Russian; language follows Windows UI culture on first launch and can be changed in the main window (persisted to `%LOCALAPPDATA%\ATRun\settings.json`).
- MainForm recalculates the HKCU/HKLM selector row and footer button widths at runtime so controls stay inside the client area across DPI/font changes.


