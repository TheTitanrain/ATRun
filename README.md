# ATRun

ATRun is a small Windows Forms utility (.NET 8) for managing Windows startup registry entries.

## Features

- Add `.exe` files to startup via drag & drop or the file picker.
- Choose between current user (`HKCU`) and all users (`HKLM`) registry hives.
- Review and delete existing startup entries in the management window.
- Register ATRun in the Windows **Send to** menu for one-click registration from Explorer.
- UI available in **English** and **Russian** — language is auto-detected from Windows on first launch and can be changed from the main window.

## Two Operating Modes

**GUI mode** (no arguments): opens the main window.

**Silent/CLI mode**: registers a file without showing any UI.

```cmd
ATRun.exe <filePath> [/hklm]
```

- Silently exits if the file is already registered or if an error occurs.
- `.lnk` shortcuts are resolved to their target automatically.
- Omit `/hklm` to register for the current user only.

## Build & Run

```bash
dotnet build
dotnet run
dotnet run -- "C:\path\to\app.exe" [/hklm]
```

Output binary: `bin/Debug/net8.0-windows/ATRun.exe`

## Notes

- Writing to `HKLM` requires administrator privileges. The app runs as `asInvoker` — the caller is responsible for elevation.
- Language preference is saved to `%LOCALAPPDATA%\ATRun\settings.json`.
