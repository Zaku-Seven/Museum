# Morning Test — Arcane Museum Feel (Night 1–4)

A ~15-minute human checklist. The cloud agent is **code-only** and cannot compile or enter Play mode.

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
3. **Game → Setup Museum Gameplay** — north section, HUD (banner, stack line, win overlay, tutorials), pause, aim highlighter, progress + save on Player
4. **Game → Setup Gallery Wings**

Re-running menus is safe (idempotent). Re-run **Setup Museum Gameplay** to upgrade an old HUD canvas (Banner, PausePanel, WinPanel, StackLabel, etc.).

**Sanity check:** **Game → Validate Museum Scene** — should report 0 errors after full setup.

**Edit-mode tests:** **Window → General → Test Runner → EditMode → Run All** (5 tests in `MuseumGameplayTests`).

## Play mode checklist

1. Press **Play**. Expect: `+` crosshair, bottom-left progress, top hint (includes scroll + right-click undo), cursor locked.
2. **Floor highlight:** look at a floor painting (empty-handed) → canvas **subtly pulses**.
3. **Pick up:** press **E** on a painting → snaps to hands.
4. **Stack:** pick up 2+ floor paintings with **E** → they fan to your right; **scroll wheel** changes which one is forward (label above prompt: `Placing: "…" (2/3) · scroll to change`).
5. **Active item glow:** with 2+ in stack, the forward painting **pulses lightly** (MaterialPropertyBlock emission).
6. **Valid mount:** carry matching art to an empty mount → frame tints **green** → **click** places the **active** stack item (not necessarily the last picked).
7. **Wrong wing:** wrong mount → frame **pulses red**, prompt **"This belongs in the [Wing] gallery"** → click does **not** place or drop.
8. **Occupied mount:** grey frame tint, **"Gallery spot taken"**.
9. **Section complete:** fill all mounts in one section → **center banner** (~4s), frames **glow blue**, progress shows `… - done`.
10. **Sorting table:** drop a painting on the center table → it **snaps to a grid**; bottom-right shows `Sorting table: N staged`.
11. **Undo:** after hanging one, **right-click** → painting returns to your stack (mount clears).
12. **Save/load:** hang a few paintings, stop Play, Play again → mounts restore (PlayerPrefs `MuseumMountSave_v1`).
13. **Win state:** fill **every** section (all 4 wings) → **win overlay** appears, cursor unlocks, pickup stops.
14. **Pause:** **Esc** → dark overlay, cursor unlocks → **Esc** resumes.

## Expected HUD prompts

| Situation | Prompt |
|-----------|--------|
| Looking at floor painting | `E — pick up "<title>"` |
| Holding, valid empty mount | `Click to place on wall` (with active title if stack > 1) |
| Holding, wrong-wing mount | `This belongs in the Modern gallery` (etc.) |
| Holding, occupied mount | `Gallery spot taken` |
| Holding, not on a mount | `Click to drop` |
| Stack > 1 | Bottom stack line: `Placing: "…" (n/N) · scroll to change` |

## Wing layout

| Section label | Wall | Sample paintings |
|---------------|------|------------------|
| Modern (North) | North | Sunset Study, Blue Horizon (demo mounts) |
| Modern (South) | South | Cobalt Field, Steel Lines, Neon Dusk |
| Classical (East) | East | Gilded Saints, Marble Study, Old Masters |
| Impressionist (West) | West | Garden Light, Rose Morning, Water Lilies |

## Night 3 + 4 features (verify)

- Scroll wheel stack selection (primary / forward item)
- Right-click undo last wall placement
- Sorting table grid snap + staged count HUD
- One-shot tutorial tips (first pickup, wrong wing, scroll)
- Museum win overlay when all sections complete
- Mount save/load via `MuseumSaveManager`
- **Game → Validate Museum Scene**
- EditMode tests in Test Runner

## Known gaps

- Agent did not compile/Play-test. Report Console errors.
- Procedural materials may need re-run of setup if colors revert.
- No audio clips yet (events are wired in `MuseumGameEvents` for future SFX).

## Manual fixes if wiring is missing

- **Banner / pause / win missing:** re-run **Setup Museum Gameplay**.
- **`MountAimHighlighter` missing:** confirm `PlayerCamera` has the component.
- **No save/restore:** confirm `Player` has `MuseumSaveManager`.
- **Section not completing:** re-run **Setup Gallery Wings**; check `GallerySection` mount lists in Inspector.
