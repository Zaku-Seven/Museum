# My project

First-person museum sorting prototype (Unity 6 / URP).

## Setup

1. Open in Unity 6000.5.1f1 or newer.
2. Open `Assets/Scenes/SampleScene.unity`.
3. Press Play.

## Editor menu items

Run in this order on an empty scene:

1. **Game → Setup Zero-to-One Prototype** — room, player, lighting
2. **Game → Setup Art Pickup Test** — interactable layer, pickup, test painting
3. **Game → Setup Museum Gameplay** — north-wall mounts (Modern), HUD + progress line, extra paintings
4. **Game → Setup Gallery Wings** — full sorting content: 3 wings, 9 wall mounts, 9 floor paintings, 3 `GallerySection`s

- **Game → Fix Carry Settings** — reset hold point position

## Gallery wings (sorting)

Each painting belongs to a **wing** (`Modern`, `Classical`, `Impressionist`) and each wall mount
only accepts a painting of its required wing:

- Aim at a matching empty mount → **"Click to place on wall"** → it snaps on.
- Aim at a wrong-wing mount → **"Wrong gallery"**; the click is rejected and the painting stays in hand (no accidental drop).
- Aim at a filled mount → **"Gallery spot taken"**.
- Click while not aiming at any mount → drop the painting.

Filling every mount in a wing with correctly-sorted paintings completes its `GallerySection`:
the frames glow with a pulsing blue emission and the HUD progress line (bottom-left) reads
e.g. `Modern: 3/3 hung - done`.

Wing layout: Modern = south wall, Classical = east wall, Impressionist = west wall (the north
wall keeps the original Museum Gameplay mounts).

## Controls

| Input | Action |
|-------|--------|
| WASD | Move |
| Mouse | Look |
| Left click | Pick up / place on wall / drop |
| Space | Jump |
