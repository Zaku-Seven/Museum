# Morning Test — Night 1 (Arcane Museum Feel)

A ~10-minute human checklist. All of this is verified on **Windows + Unity Editor** — the
overnight cloud agent is code-only and could not compile or enter Play mode.

## Setup

1. `git fetch && git checkout feature/arcane-museum-feel && git pull`
2. Open the project in Unity **6000.5.1f1**.
3. Open `Assets/Scenes/SampleScene.unity`.
4. Let scripts compile. Check the **Console** for red errors before continuing.
   - New scripts have no `.meta` files in git on purpose — Unity generates them on first import.

## Editor menus (run in order)

1. **Game → Setup Zero-to-One Prototype** — room, player, sun
2. **Game → Setup Art Pickup Test** — Interactable layer + first painting
3. **Game → Setup Museum Gameplay** — north-wall Modern mounts + HUD (crosshair, prompt, progress line, hint)
4. **Game → Setup Gallery Wings** — 3 wings, 9 wall mounts, 9 floor paintings, 3 sections, sorting-table slab

Each menu logs a summary line (created/skipped). Re-running is safe (idempotent).

## Play mode checklist

1. Press **Play**. Cursor locks; you see a `+` crosshair, a bottom-left progress line, and a faint top hint ("Future: scroll to reorder stack").
2. **Move/look:** WASD to walk, mouse to look, Space to jump. Confirm no camera jitter.
3. **Pick up:** look at a colored floor slab → prompt `Click to pick up "<title>"` → left-click.
   - The painting should **snap to your hands instantly** and stay centered/readable while you look around (no floaty lag, no physics fighting).
4. **Wrong wing (key test):** carry a painting to a mount of a **different** wing → prompt reads **"Wrong gallery"** → left-click → nothing happens (it is **not** placed and **not** dropped; you keep holding it).
5. **Occupied mount:** aim at a mount that already holds art → prompt **"Gallery spot taken"** → click is rejected.
6. **Correct placement:** carry a painting to a matching empty mount → prompt **"Click to place on wall"** → click → it snaps onto the frame; progress line increments (e.g. `Modern: 1/3 hung`).
7. **Drop:** while holding, aim at empty space (no mount) and click → the painting drops to the floor.
8. **Complete a wing:** fill all 3 mounts of one wing with correct paintings → that wing's frames **glow with a pulsing blue emission** and the progress line shows `<Wing>: 3/3 hung - done`.
9. **Re-pick a hung painting:** take one back off a completed wall → the glow turns off and the count drops (completion is reversible).

## Expected HUD prompts

| Situation | Prompt |
|-----------|--------|
| Looking at a floor painting (empty-handed) | `Click to pick up "<title>"` |
| Holding, aiming at matching empty mount | `Click to place on wall` |
| Holding, aiming at wrong-wing mount | `Wrong gallery` |
| Holding, aiming at occupied mount | `Gallery spot taken` |
| Holding, not aiming at a mount | `Click to drop` |

## Wing layout

- **Modern** → south wall, blue/steel paintings (Cobalt Field, Steel Lines, Neon Dusk)
- **Classical** → east wall, gold/brown paintings (Gilded Saints, Marble Study, Old Masters)
- **Impressionist** → west wall, pastel paintings (Garden Light, Rose Morning, Water Lilies)
- North wall keeps the original 3 Museum Gameplay mounts (also Modern).

## Known gaps / not implemented

- Not compiled or Play-tested by the agent (cloud VM is code-only). Report any compile errors.
- Materials for the 9 paintings + frames are created at edit time (`new Material(URP/Lit)`). They should serialize into the scene on save; if a color ever reverts to grey/pink-missing-shader, re-run **Setup Gallery Wings**.
- Stretch items skipped: wrong-wing floor-painting highlight (#2), Escape-to-unlock cursor (#4 — the editor already frees the cursor on Esc), second room (#5). See `OVERNIGHT_LOG.md`.
- `SortingTable` is a visual/trigger marker only (no scoring yet).

## If a setup script did not wire something (manual Inspector steps)

- **HUD progress/hint missing:** if `InteractionHUD` already existed from a previous run, `Setup Museum Gameplay` skips rebuilding it. Delete the `InteractionHUD` GameObject and re-run the menu, or manually drag the `Progress`/`Hint` Text objects into the `InteractionHUD` component's fields.
- **Section not completing:** select each `Section_<Wing>` under `GalleryWings/Wing_<Wing>` and confirm its `GallerySection` has `Section Wing` set and the 3 `Mounts` populated. Re-run **Setup Gallery Wings** to re-wire.
- **Painting won't pick up:** confirm it is on the **Interactable** layer (layer 6) and has a `Rigidbody` + `InteractablePainting` with a `Wing` set.
