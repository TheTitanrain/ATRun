# ATRun

ATRun is a small Windows Forms utility for managing Windows autorun entries through the registry.

## Features

- Add `.exe` files to autorun with drag and drop or file picker.
- Choose whether the entry is created for the current user (`HKCU`) or all users (`HKLM`).
- Review and delete existing autorun entries in the `Управление автозапуском` window.
- Register the application in the Windows `Send to` menu.

## Recent UI Update

The autorun management window now recalculates entry widths after the form is shown and whenever the scroll area changes size. Entry rows, labels, and action buttons stay aligned with the full available window width instead of remaining narrow after the initial layout pass.

## Verification

Use the standard .NET CLI commands from the repository root:

```powershell
dotnet build ATRun.sln -v minimal
dotnet test ATRun.sln -v minimal
```
