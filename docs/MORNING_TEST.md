# Morning Test — Arcane Museum Feel (Night 1 + Night 2)

A ~10-minute human checklist. The cloud agent is **code-only** and cannot compile or enter Play mode.

## Setup

1. `git fetch && git checkout feature/arcane-museum-feel && git pull`
2. Open the project in Unity **6000.5.1f1**.
3. Open `Assets/Scenes/SampleScene.unity`.
4. Let scripts compile. Check the **Console** for red errors.
   - New scripts have no `.meta` in git — Unity generates them on first import.

## Editor menus

**Fast path:** **Game → Setup Full Museum** (runs all steps below).

Or run individually:

1. **Game → Setup Zero-to-One Prototype**
2. **Game → Setup Art Pickup Test**
3. **Game → Setup Museum Gameplay** — north section, HUD banner, pause overlay, aim highlighter
4. **Game → Setup Gallery Wings**

Re-running menus is safe (idempotent). If you had an old HUD canvas from Night 1, re-run **Setup Museum Gameplay** to add Banner + PausePanel (it upgrades in place).

## Play mode checklist

1. Press **Play**. Expect: `+` crosshair, bottom-left progress, top hint (`Esc — pause …`), cursor locked.
2. **Floor highlight:** look at a floor painting (empty-handed) → canvas **subtly pulses**.
3. **Pick up:** left-click a painting → instant snap to hands.
4. **Valid mount:** carry matching art to an empty mount → frame tints **green**, prompt **"Click to place on wall"** → click → **scale pop** on hang; progress increments.
5. **Wrong wing:** carry to wrong mount → frame **pulses red**, prompt **"This belongs in the [Wing] gallery"** → click does **not** place or drop.
6. **Occupied mount:** grey frame tint, **"Gallery spot taken"**.
7. **Section complete:** fill all 3 mounts in one section → **center banner** (`… wing complete!` ~4s), frames **glow blue**, progress shows `… - done`.
8. **North wall:** progress includes **`Modern (North): X/3`** after Museum Gameplay (separate from `Modern (South)`).
9. **Pause:** press **Esc** → dark overlay, cursor unlocks, movement/pickup stop → **Esc** again resumes.
10. **Drop:** while holding, aim at empty space and click → painting drops.

## Expected HUD prompts

| Situation | Prompt |
|-----------|--------|
| Looking at floor painting | `Click to pick up "<title>"` |
| Holding, valid empty mount | `Click to place on wall` |
| Holding, wrong-wing mount | `This belongs in the Modern gallery` (etc.) |
| Holding, occupied mount | `Gallery spot taken` |
| Holding, not on a mount | `Click to drop` |

## Wing layout

| Section label | Wall | Sample paintings |
|---------------|------|------------------|
| Modern (North) | North | Sunset Study, Blue Horizon (demo mounts) |
| Modern (South) | South | Cobalt Field, Steel Lines, Neon Dusk |
| Classical (East) | East | Gilded Saints, Marble Study, Old Masters |
| Impressionist (West) | West | Garden Light, Rose Morning, Water Lilies |

## Night 2 polish (verify)

- Mount aim highlights (green / red pulse / grey)
- Floor painting pulse when viewed
- Placement scale pop on hang
- Section-complete center banner
- Esc pause overlay
- **Game → Setup Full Museum** one-click bootstrap

## Known gaps

- Agent did not compile/Play-test. Report Console errors.
- Procedural materials may need re-run of setup if colors revert.
- No audio yet. Stack carry / win screen not implemented.
- `SortingTable` is visual-only (no snap-to-pile behavior).

## Manual fixes if wiring is missing

- **Banner / pause missing:** re-run **Setup Museum Gameplay** (upgrades existing `InteractionHUD` canvas).
- **`MountAimHighlighter` missing:** confirm `PlayerCamera` has the component (Museum Gameplay adds it).
- **Section not completing:** re-run **Setup Gallery Wings** or **Setup Museum Gameplay** for north section; check `GallerySection` mount lists in Inspector.
