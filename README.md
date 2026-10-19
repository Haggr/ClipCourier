# ClipCourier

**Send selected text from an app into a local file with a keyboard shortcut.**

Select text, press the shortcut, and choose one or more destinations. ClipCourier appends the selection to those files and creates them if needed. The destination folders must already exist. CSV and other structured formats are not yet supported, but soon should be added.

## What it does

- Opens a destination picker with a configurable global shortcut (default: `Ctrl+Alt+J`).
- Appends the selection to one or more configured text files; you do not need to open those files first.
- Lets you manage destination files and change the shortcut from the tray icon's Settings menu.
- Can start when you sign in.
- Runs locally and does not send selected text over the network.

## Requirements

- Windows 10 or 11 with .NET Framework 4.8.
- Unsigned x64 Windows desktop application.

## Run the portable build

1. Download `ClipCourier-win-x64.zip` from the project's GitHub Releases page.
2. Extract it and run `ClipCourier.exe`.
3. Open the ClipCourier tray icon menu and choose **Settings**.
4. Add one or more text files as destinations. Their picker labels use each file name without its extension.
5. Select text in another app and press `Ctrl+Alt+J`; choose one or more destinations and append.

Change the shortcut in Settings. Close the app from its tray icon menu. Settings and logs are stored in `%LOCALAPPDATA%\ClipCourier`.

## Build from source

On Windows, open PowerShell in the repository and run:

```powershell
.\src\Build.ps1
```

The script uses the C# compiler included with the Windows .NET Framework installation. The executable is written to `Portable\ClipCourier.exe`.

## Source layout

- `src/Program.cs` starts the app and handles top-level errors.
- `src/Models/` contains settings and destination data.
- `src/Infrastructure/` loads and saves settings and writes diagnostic logs.
- `src/Platform/` contains Windows API and sign-in startup integration.
- `src/Application/` coordinates the tray icon, shortcut, and selection flow.
- `src/Services/` handles destination file writes and encoding.
- `src/UI/` contains the destination picker and settings dialogs.

## Privacy and behavior

ClipCourier sends `Ctrl+C` to the active app after you press the shortcut, then shows the destination picker. Apps that block simulated keyboard input may not work. A destination file is changed only after you choose **Append**.

Entries are separated by a single line break, with no extra blank line. Files without a byte-order mark are treated as UTF-8. If appending fails, the app displays an error and keeps the copied text on the clipboard so you can retry.

## Known limitations

- Elevated, secure, and remote apps may block copying or simulated keyboard input.
- The Windows build is unsigned, so SmartScreen may show a reputation warning.
- ClipCourier has no automatic updater yet; download new versions from Releases.

## Contributing

Bug reports and focused improvements are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md). Please do not include private text or personal file paths in issues or screenshots.

## License

MIT. See [LICENSE](LICENSE).
