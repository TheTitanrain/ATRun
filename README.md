# ATRun

ATRun is a small Windows Forms utility for managing Windows autorun entries through the registry.

## Features

- Add `.exe` files to autorun with drag and drop or the file picker.
- Choose whether the entry is created for the current user (`HKCU`) or all users (`HKLM`).
- Review and delete existing autorun entries in the startup management window.
- Register the application in the Windows `Send to` menu.
- Switch the UI between English and Russian from the main window.

## Localization

The application now supports both English and Russian.

- On the first launch, the UI language follows the current Windows UI culture.
- The language can be changed from the selector in the main window.
- The selected language is saved in the user's local application settings and reused on the next launch.
- The `Send to` shortcut is recreated with the active language name while still recognizing the previous Russian shortcut name for compatibility.

## Recent UI Update

The autorun management window recalculates entry widths after the form is shown and whenever the scroll area changes size. Entry rows, labels, and action buttons stay aligned with the full available window width instead of remaining narrow after the initial layout pass.

## Verification

Use the standard .NET CLI commands from the repository root:

```powershell
dotnet build ATRun.sln -v minimal
dotnet test ATRun.sln -v minimal
```
