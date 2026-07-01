# My project

First-person museum sorting prototype (Unity 6 / URP).

## Setup

1. Open in Unity 6000.5.1f1 or newer.
2. Open `Assets/Scenes/SampleScene.unity`.
3. Run **Game → Setup Full Museum** (one click), or the step-by-step menus below.
4. Press Play — **main menu** offers Continue (if save exists), Enter museum, New game, Settings.

## Editor menu items

**Quick start:** **Game → Setup Full Museum** — runs all four steps below in order.

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

## Gallery wings (sorting)

Each painting belongs to a **wing** (`Modern`, `Classical`, `Impressionist`) and each wall mount
only accepts a painting of its required wing:

- Aim at a wrong-wing mount → **"This belongs in the [Wing] gallery"**; click is rejected (painting stays in hand). Frame **pulses red**.
- Aim at a matching empty mount → frame tints **green**; placement plays a brief **scale pop**.
- Aim at a filled mount → **"Gallery spot taken"** (grey frame tint).
- Floor paintings **pulse subtly** when viewed empty-handed.

Filling every mount in a section completes its `GallerySection`: frames **glow blue**, a **center banner** announces completion (e.g. `Modern (South) wing complete!`), and the HUD progress line reads e.g. `Modern (North): 3/3 hung - done`.

Wing layout: **Modern (South)** = south wall, **Classical (East)** = east, **Impressionist (West)** = west. **Modern (North)** = original north-wall mounts (3 frames).

## Controls

| Input | Action |
|-------|--------|
| WASD | Move |
| Mouse | Look |
| E | Pick up / add to stack (max 5, fans to your right) |
| Scroll wheel | Change which stack item is forward (when carrying 2+) |
| Q | Throw active painting (physics arc; sorting table catches slow lands) |
| Left click | Place **active** stack item on wall / drop |
| Right click | Undo last wall placement (returns painting to stack) |
| Space | Jump |
| Esc | Pause / resume (Settings, New Game) |

When every gallery section is complete, a **win overlay** appears with **Continue exploring** or **New game**. Pickup stays disabled after win unless you start a new game. Progress (mounts, sorting table staging, win state) persists via **save v2** (`MuseumSaveManager` + stable `MuseumEntityId`s). Pause menu includes **mouse sensitivity**, **FOV**, **invert Y**, and **master volume**. Optional SFX hooks live on `MuseumAudioDirector` (assign clips in Inspector).

See `docs/MULTIPLAYER_VOIP.md` for multiplayer / voice chat scope (not implemented yet).
