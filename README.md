# My project

First-person museum sorting prototype (Unity 6 / URP).

## Setup

1. Open in Unity 6000.5.1f1 or newer.
2. Open `Assets/Scenes/SampleScene.unity`.
3. Run **Game → Setup Full Museum** (one click), or the step-by-step menus below.
4. Press Play.

## Editor menu items

**Quick start:** **Game → Setup Full Museum** — runs all four steps below in order.

Or run individually on an empty scene:

1. **Game → Setup Zero-to-One Prototype** — room, player, lighting
2. **Game → Setup Art Pickup Test** — interactable layer, pickup, test painting
3. **Game → Setup Museum Gameplay** — north-wall mounts + `GallerySection`, HUD (banner, pause, progress), aim highlights
4. **Game → Setup Gallery Wings** — full sorting content: 3 wings, 9 wall mounts, 9 floor paintings, 3 sections

- **Game → Fix Carry Settings** — reset hold point position

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
| Left click | Pick up / place on wall / drop |
| Space | Jump |
| Esc | Pause / resume (unlocks cursor, shows overlay) |
