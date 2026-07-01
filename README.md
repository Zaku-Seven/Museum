# My project

First-person museum sorting prototype (Unity 6 / URP).

## Setup

1. Open in Unity 6000.5.1f1 or newer.
2. Open `Assets/Scenes/SampleScene.unity`.
3. Run **Game → Setup Full Museum** (one click), or the step-by-step menus below.
4. Press Play — **main menu** offers Continue (if save exists), Enter museum, New game, Settings.

## Editor menu items

**Quick start:** **Game → Build Museum Now (Recommended)** — builds the full sorting museum in one click (same as Setup Full Museum below).

Or run individually on an empty scene:

1. **Game → Setup Zero-to-One Prototype** — room, player, lighting
2. **Game → Setup Art Pickup Test** — interactable layer, pickup, test painting
3. **Game → Setup Museum Gameplay** — north-wall mounts + `GallerySection`, HUD (banner, pause, progress, win overlay, stack/staging lines), aim highlights, progress + save on Player
4. **Game → Setup Gallery Wings** — full sorting content: 3 wings, 9 wall mounts, 9 floor paintings, 3 sections, sorting table

- **Game → Fix Carry Settings** — reset hold point position
- **Game → Validate Museum Scene** — read-only wiring check
- **Game → Clear Museum Save** — wipe PlayerPrefs progress
- **Game → Create Painting Definition Assets** — ScriptableObject catalog under `Assets/Data/Paintings/`
- **Game → Link Painting Definitions To Scene** — apply SO data to scene paintings
- **Game → Add Mount Wing Placards** — world-space wing labels on frames
- **Game → Add Player Bean Visual** — optional capsule mesh on Player
- **Game → Setup Museum Architecture** — ceiling, pillars, wing banners, welcome sign, archive ceiling extension
- **Game → Setup Museum Atmosphere** — global URP Volume mood profile
- **Game → Expand Museum Building** — north archive room, corridor, alcoves, vitrines, overflow art, gallery lighting

See `docs/MUSEUM_THOROUGH_SETUP.md` for the full one-click museum pass.

## Gallery wings (sorting)

Each painting belongs to a **wing** (`Modern`, `Classical`, `Impressionist`) and each wall mount
only accepts a painting of its required wing:

- Aim at a wrong-wing mount → **"This belongs in the [Wing] gallery"**; click is rejected (painting stays in hand). Frame **pulses red**.
- Some gallery wing mounts reserve a **specific painting** (placard shows title). Wrong painting on correct wing → **"Spot reserved for …"**; frame pulses red.
- Aim at a matching empty mount → frame tints **green**; placement plays a brief **scale pop**.
- Aim at a filled mount → **"Gallery spot taken"** (grey frame tint).
- Floor paintings **pulse subtly** when viewed empty-handed.

Filling every mount in a section completes its `GallerySection`: frames **glow blue**, a **center banner** announces completion (e.g. `Modern (South) wing complete!`), and the HUD progress line reads e.g. `Modern (North): 3/3 hung - done`.

While carrying, the **wing guide** (top-left, green) shows how many open mounts remain for that wing. A **compass** above the crosshair points to the nearest valid mount. Hold **Tab** / **View** to inspect art lore. Press **J** (or pause → **Collection**) for the full curator log. A **progress bar** under the objective tracks overall completion. Valid mounts show a **placement ghost** when you aim at them.

Wing layout: **Modern (South)** = south wall, **Classical (East)** = east, **Impressionist (West)** = west. **Modern (North)** = original north-wall mounts (3 frames). **Modern (Archive)** = north annex past the corridor (3 reserved-slot mounts + archive sorting table).

## Controls

| Input | Action |
|-------|--------|
| WASD | Move |
| Mouse | Look |
| E | Pick up / add to stack (max 5, fans to your right) |
| Scroll wheel | Change which stack item is forward (when carrying 2+) |
| Q | Throw active painting (physics arc; sorting table catches slow lands) |
| Left click / **X** (gamepad) | Place **active** stack item on wall / drop |
| Right click / **Y** (gamepad) | Undo last wall placement |
| LB / RB or D-pad | Change stack item (gamepad) |
| Space / **L3** (stick click) | Jump |
| Shift / **LT** (hold) | Sprint |
| Tab / **View** (hold) | Inspect painting or mount |
| **J** / **R3** | Collection journal (pause → **Collection** on gamepad) |
| Esc / **Start** (gamepad) | Pause / resume (Settings, Controls, New Game) |

**Dev cheats** (Editor / Development builds only): **F5** spawn test painting · **F6** teleport to nearest mount · **F7** debug overlay · **F8** save · **F9** fill one section · **F10** auto-hang all · **F11** force win.

When every gallery section is complete, a **win overlay** appears with **Continue exploring** or **New game**. Pickup stays disabled after win unless you start a new game. Progress (mounts, sorting table staging, win state) persists via **save v2** (`MuseumSaveManager` + stable `MuseumEntityId`s). Pause menu includes **mouse sensitivity**, **FOV**, **invert Y**, and **master volume**. Optional SFX hooks live on `MuseumAudioDirector` (assign clips in Inspector).

See `docs/FEATURE_COMPLETE.md` for the gameplay-complete checklist and `docs/VISUAL_POLISH.md` for the art/audio pass. Multiplayer / VoIP: `docs/MULTIPLAYER_VOIP.md` (future phase).
