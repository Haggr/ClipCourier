# ClipCourier

**Catch a useful bit of text before it slips away.**

ClipCourier is a small Windows helper for sending selected text straight to a file. Select something, press a shortcut, pick where it belongs, and get back to what you were doing.

## How it helps

- Bring up a destination picker with a configurable shortcut (default: `Ctrl+Alt+J`).
- Send a selection to one or more text files without opening them first.
- Add and manage destinations, or change the shortcut, from **Settings** in the tray menu.
- Start ClipCourier when you sign in, if you like.
- Keep everything local: selected text is not sent over the network.

## What could you use it for?

- Save unfamiliar words and phrases to a list, ready for when you make Anki cards.
- Drop questions you cannot answer yet into an Obsidian Markdown file, then come back to them later.
- Gather quotes, definitions, ideas, or other snippets in files you already use.

ClipCourier saves the selected text to your chosen file. It does not create Anki cards or connect to Obsidian directly.

## Requirements

- Windows 10 or 11 with .NET Framework 4.8.
- Unsigned x64 Windows desktop application.

## Get started

1. Download `ClipCourier-win-x64.zip` from the project's GitHub Releases page.
2. Extract it and run `ClipCourier.exe`.
3. Open the ClipCourier tray icon menu and choose **Settings**.
4. Add one or more text files as destinations. In the picker, each one appears by its file name without the extension.
5. Select text in another app, press `Ctrl+Alt+J`, and choose where to send it.

You can change the shortcut in Settings and close the app from its tray menu. Settings and logs live in `%LOCALAPPDATA%\ClipCourier`.

ClipCourier appends the selection to the chosen files and creates a file if needed. The destination folder must already exist. Entries are separated by one line break, with no blank line in between. CSV and other structured formats are not supported yet.

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

## A few things to know

After you press the shortcut, ClipCourier sends `Ctrl+C` to the active app and then shows the destination picker. Some apps block simulated keyboard input, so copying may not work in those apps. A destination file changes only after you choose **Append**.

Files without a byte-order mark are treated as UTF-8. If appending fails, ClipCourier shows an error and keeps the copied text on the clipboard so you can retry.

### Current limitations

- Elevated, secure, and remote apps may block copying or simulated keyboard input.
- The Windows build is unsigned, so SmartScreen may show a reputation warning.
- ClipCourier has no automatic updater yet; download new versions from Releases.

## Help improve ClipCourier

Found a bug or have a focused improvement in mind? Contributions are welcome; see [CONTRIBUTING.md](CONTRIBUTING.md). Please leave private text and personal file paths out of issues and screenshots.

## License

MIT. See [LICENSE](LICENSE).
