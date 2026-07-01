# AGENTS.md

## Cursor Cloud specific instructions

This repo is a **Unity 6 (`6000.5.1f1`) / URP single-player game** ("My project" — a first-person museum sorting prototype). There is no backend, database, web server, or package-manager step; all dependencies are Unity UPM packages that the Editor restores automatically from `Packages/manifest.json` on first project open.

### Unity Editor location
- The matching Editor (`6000.5.1f1`) is installed at `/opt/unity/Editor/Unity` (also symlinked as `unity-editor`). It is baked into the VM snapshot, so the startup update script does NOT re-download it.
- If it is ever missing (snapshot reset), reinstall with:
  ```bash
  sudo mkdir -p /opt/unity && sudo chown "$USER" /opt/unity
  curl -L -o /opt/unity/Unity.tar.xz "https://download.unity3d.com/download_unity/0d9463e84828/LinuxEditorInstaller/Unity.tar.xz"
  tar -xf /opt/unity/Unity.tar.xz -C /opt/unity && rm /opt/unity/Unity.tar.xz
  sudo ln -sf /opt/unity/Editor/Unity /usr/local/bin/unity-editor
  ```
  System libs are already installed in the snapshot (GTK/GL/NSS/xvfb etc.). If missing, see the setup PR for the `apt-get` list (note: `libgconf-2-4` does NOT exist on Ubuntu 24.04 — omit it).

### License is REQUIRED before anything runs (non-obvious gotcha)
Unity 6 refuses **every** operation (import, compile, tests, build, Play mode) until a license is activated — even `-batchmode -nographics`. Without it the log ends with `No valid Unity Editor license found. Please activate your license.`

Activation needs Unity account secrets (add them in the Secrets panel). Then run once per fresh machine:
```bash
# Personal (free) license: UNITY_SERIAL optional. Pro/Plus: UNITY_SERIAL required.
/opt/unity/Editor/Unity -batchmode -nographics -logFile - -quit \
  -username "$UNITY_EMAIL" -password "$UNITY_PASSWORD" ${UNITY_SERIAL:+-serial "$UNITY_SERIAL"}
```
- Note: Unity accounts with 2FA cannot activate via username/password. In that case provide a pre-generated `.ulf` license file (game-ci style, as `UNITY_LICENSE`) and activate with `-manualLicenseFile <path>`.
- Return the license on shutdown if needed: `... -returnLicense -quit`.

### Compile / test / build / run (all require an active license)
Run all commands with `HOME=/home/ubuntu`. A GUI already exists on `DISPLAY=:1` (Mesa `llvmpipe` software GL, OpenGL 4.5) — use it for Play mode so screenshots/computer-use work.

- **Compile check (headless import):**
  ```bash
  /opt/unity/Editor/Unity -batchmode -nographics -quit -logFile - -projectPath /workspace
  ```
- **Build scene contents headlessly** (the Editor menu items are also exposed for CLI):
  ```bash
  /opt/unity/Editor/Unity -batchmode -nographics -quit -logFile - -projectPath /workspace \
    -executeMethod PrototypeSceneSetup.ExecuteFromCommandLine
  ```
  (Also: `ArtPickupSceneSetup.ExecuteFromCommandLine`, `MuseumGameplaySetup.ExecuteFromCommandLine`.)
- **Tests:** Unity Test Framework is installed but there are currently **no** EditMode/PlayMode tests. If added: `-runTests -testPlatform EditMode -testResults /tmp/results.xml`.
- **Run the game (Play mode, needs graphics):** run against the existing desktop, e.g.
  ```bash
  DISPLAY=:1 /opt/unity/Editor/Unity -projectPath /workspace -logFile -
  ```
  then use the desktop/computer-use to press Play. Do **not** pass `-nographics` for Play mode.

### Lint
No linter is configured; C# compile errors surface during the headless import above (via the Editor).
