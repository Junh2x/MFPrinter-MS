# Repository Guidelines

## Project Structure & Module Organization
`Scanlink/` contains the Windows WPF client, organized into `Views/`, `ViewModels/`, `Services/`, `Drivers/`, `Models/`, and `Assets/`. `auth-server/` holds Firebase Hosting, Firestore, and Cloud Functions configuration. `python-func/` contains reusable printer helpers grouped by vendor, while `remote-tools/` is for one-off device scripts and captured results. Use `docs/` for design notes, test records, and workflow documentation. Treat `publish/`, `Scanlink/logs/`, and `remote-tools/**/results/` as generated output.

## Build, Test, and Development Commands
- `dotnet build JA-solution.sln` builds the desktop app and is the baseline pre-PR check.
- `dotnet run --project Scanlink/Scanlink.csproj` launches the WPF app locally on Windows.
- `dotnet publish Scanlink/Scanlink.csproj -c Release -o publish/Scanlink` creates a release build.
- `npm --prefix auth-server/functions install` installs Firebase function dependencies.
- `npm --prefix auth-server/functions run serve` starts the local Firebase emulator for functions.
- `python python-func/common/discovery.py --scan` runs device discovery for manual verification against the local subnet.

## Coding Style & Naming Conventions
Use 4 spaces in C# and Python; existing Firebase JavaScript uses 2 spaces and CommonJS modules. Follow the established C# style: file-scoped namespaces, nullable reference types, `PascalCase` for types and properties, and `_camelCase` for private fields. Keep XAML files role-based, for example `DeviceListPage.xaml`, `ManualConnectDialog.xaml`, and matching `*ViewModel.cs` files. Python modules and helper functions should remain `snake_case`. No repo-wide formatter is checked in, so rely on IDE formatting and keep diffs focused.

## Testing Guidelines
There is no dedicated automated test project yet. Minimum validation is `dotnet build`, a focused local run of the changed `Scanlink` workflow, and any needed Firebase emulator or Python script check. Record manual regression details in `docs/test-*.md` when behavior changes. If you add automated tests later, place them in a separate `tests/` project and name files `<Feature>Tests.cs`.

## Commit & Pull Request Guidelines
Recent history uses short imperative messages such as `update` and `test logs`; keep that tone but make the scope explicit, for example `scanlink: handle auth refresh failure`. Avoid using `push.sh` or `push.bat` for reviewable work because they stage everything and always commit as `update`. PRs should include a short summary, affected areas, validation steps, and screenshots for XAML UI changes. Exclude generated logs, `bin/`, `obj/`, `publish/`, and raw device capture output.

## Security & Configuration Tips
Keep secrets in environment variables, especially `ADMIN_KEY` for Firebase functions. Do not commit customer device IPs, local auth tokens, or unsanitized capture files. Sanitize examples before adding them to `docs/` or `remote-tools/`.
