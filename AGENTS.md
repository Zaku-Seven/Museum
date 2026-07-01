# AGENTS.md

## Cursor Cloud specific instructions

This repo is a single **Unity 6 (`6000.5.1f1`) / URP** first-person museum prototype (C#).
There is no web/backend/DB stack — the "app" is the Unity project itself. See `README.md`
for controls and the in-editor `Game → …` setup menus.

### Toolchain (provisioned in the VM snapshot)
- Unity Editor `6000.5.1f1` lives at `/opt/unity/Editor/Unity`, symlinked as `unity-editor`
  on `PATH`. Verify with `unity-editor -version`.
- Unity's Linux runtime libraries and `xvfb` are already installed via `apt`.
- UPM packages (`Packages/manifest.json` / `packages-lock.json`) resolve automatically the
  first time the Editor opens the project; there is no separate package-install command.
- The Editor generates a gitignored `Library/` on first import (this is normal and can take
  several minutes on a cold open).

### Headless usage (no GPU/display in cloud)
Unity needs an X display even in `-batchmode`. Start a virtual one first:
```bash
Xvfb :99 -screen 0 1280x1024x24 >/tmp/xvfb.log 2>&1 &
export DISPLAY=:99
```

### License activation is REQUIRED before anything works
Importing, compiling, building, or entering Play mode all fail with
`No valid Unity Editor license found. Please activate your license.` until a license is
activated. Activation needs a Unity account, supplied as secrets (none are committed):
- Preferred (Personal, free): `UNITY_LICENSE` = full contents of a `.ulf` license file.
  Generate the request file with
  `unity-editor -batchmode -nographics -quit -createManualActivationFile` (writes
  `Unity_v6000.5.1f1.alf`), upload it at <https://license.unity3d.com/manual> while signed
  into a Unity account, then activate the returned `.ulf` with
  `unity-editor -batchmode -nographics -quit -manualLicenseFile <file>.ulf`.
- Pro/Plus alternative: `UNITY_EMAIL` + `UNITY_PASSWORD` + `UNITY_SERIAL`, then
  `unity-editor -batchmode -nographics -quit -username "$UNITY_EMAIL" -password "$UNITY_PASSWORD" -serial "$UNITY_SERIAL"`.

Do NOT commit `.alf`/`.ulf` files or credentials.

### Build / compile / run (after a license is active)
- Compile + import (also validates C# scripts):
  `unity-editor -batchmode -nographics -quit -projectPath /workspace -logFile -`
- Rebuild the prototype scene headlessly:
  `unity-editor -batchmode -nographics -quit -projectPath /workspace -executeMethod PrototypeSceneSetup.ExecuteFromCommandLine -logFile -`
- Interactive Play mode needs a graphical session; on this headless VM, prefer building a
  standalone Linux player and running it under `xvfb` for demos. There are no automated
  tests in the repo (the Test Framework package is present but unused).
