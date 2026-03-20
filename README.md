[Support Titanrain](https://donatr.ee/titanrain)

<!-- GitHub badge -->
[![Support Titanrain](https://img.shields.io/badge/Donate-donatr.ee-6C5CE7?style=for-the-badge)](https://donatr.ee/titanrain)

# ATRun

ATRun is a small Windows Forms utility (.NET 10) for managing Windows startup registry entries.

## Install

**Online installer — receives automatic updates on every launch:**
[https://TheTitanrain.github.io/ATRun/ATRun.application](https://TheTitanrain.github.io/ATRun/ATRun.application)

Or download `setup.exe` from the [latest release](../../releases/latest).

> `setup.exe` is a **web bootstrapper**: it downloads .NET 10 Desktop Runtime (if not already installed) and the application itself from GitHub Pages. An internet connection is required.

## Features

- Add `.exe` files to startup via drag & drop or the file picker.
- Choose between current user (`HKCU`) and all users (`HKLM`) registry hives. When `HKLM` is selected and the app is not running as administrator, it offers to restart elevated via a UAC prompt.
- Review and delete existing startup entries from the management window.
- Register ATRun in the Windows **Send To** menu for one-click registration from Explorer.
- UI in **English** and **Russian** — auto-detected from Windows on first launch, changeable from the main window.

## Two Operating Modes

**GUI mode** (no arguments): opens the main window.

**Silent/CLI mode**: registers a file without showing any UI.

```cmd
ATRun.exe <filePath> [/hklm]
```

- Silently exits if the file is already registered or if an error occurs.
- `.lnk` shortcuts are resolved to their target automatically.
- Omit `/hklm` to register for the current user only.

## Requirements

- **OS:** Windows 10 or later
- **Runtime:** [.NET 10 Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Privileges:** Standard user for `HKCU`; administrator required for `HKLM` (the app will offer to self-elevate via UAC)

## Build

```bash
dotnet build
dotnet run
dotnet run -- "C:\path\to\app.exe" [/hklm]
```

Output: `bin/Debug/net10.0-windows/ATRun.exe`

## Notes

- Writing to `HKLM` requires administrator privileges. In GUI mode the app offers to restart itself elevated via a UAC prompt; in silent/CLI mode the caller is responsible for elevation.
- Language preference is saved to `%LOCALAPPDATA%\ATRun\settings.json`.
