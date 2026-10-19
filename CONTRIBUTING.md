# Contributing

Contributions are welcome. Open an issue to discuss a larger change before starting work.

## Report a problem

When reporting a bug, include:
- Windows version and the app where the selected text was copied.
- The shortcut and destination file type involved.
- What you expected and what happened.
- Relevant error text, with personal text and file paths removed.

## Build and submit a change

1. Fork the repository and create a branch.
2. Keep the change focused. Avoid new dependencies unless they solve a specific problem.
3. Build on Windows with `.\src\Build.ps1`.
4. Open a pull request describing the change and how you checked it.

Do not commit settings, logs, destination lists, or captured text. ClipCourier stores its local data in `%LOCALAPPDATA%\ClipCourier`.



