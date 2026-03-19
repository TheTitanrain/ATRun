# ATRun

ATRun is a small Windows Forms utility (.NET 8) for managing Windows startup registry entries.

## Install

**Online installer — receives automatic updates on every launch:**
[https://TheTitanrain.github.io/ATRun/ATRun.application](https://TheTitanrain.github.io/ATRun/ATRun.application)

Or download `setup.exe` from the [latest release](../../releases/latest).

> Requires [.NET 8 Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/8.0) if installing from `setup.exe`.

## Features

- Add `.exe` files to startup via drag & drop or the file picker.
- Choose between current user (`HKCU`) and all users (`HKLM`) registry hives.
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
- **Runtime:** [.NET 8 Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Privileges:** Standard user for `HKCU`; administrator required for `HKLM`

## Build

```bash
dotnet build
dotnet run
dotnet run -- "C:\path\to\app.exe" [/hklm]
```

Output: `bin/Debug/net8.0-windows/ATRun.exe`

## Notes

- Writing to `HKLM` requires administrator privileges. The app runs as `asInvoker` — the caller is responsible for elevation.
- Language preference is saved to `%LOCALAPPDATA%\ATRun\settings.json`.
