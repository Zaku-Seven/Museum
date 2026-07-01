# Overnight Log — Night 1

Agent: fill this in after each phase (see `NIGHT1_MASTER.md` → Progress protocol).

---

## Session start

**Branch:** `feature/arcane-museum-feel`  
**Started:** 2026-07-01T05:51Z  
**Agent model:** Composer 2.5  
**Environment:** Cloud VM, code-only (no Unity install / no license activation / no batchmode compile per NIGHT1_MASTER.md).

### Initial state

- Existing pickup/carry system in `ArtPickup.cs`
- Editor menus: Zero-to-One, Art Pickup Test, Museum Gameplay, Fix Carry Settings
- Cloud VM: code-only (no Unity compile)

---

<!-- Agent: append phase entries below this line -->

## 2026-07-01T05:52Z Phase 1 — Carry feel polish (P0)

**Status:** done

**Files changed:**
- `Assets/Scripts/ArtPickup.cs` — added a documentation comment recording the verified carry model and tuned parameter values. No behavior change.

**What I reviewed (no regression):**
- Pickup path snaps the painting to the hold point immediately (`SetPositionAndRotation` in `PickUpObject`) — no floaty pop-in.
- `LateUpdate` lerp (`carryFollowSharpness = 28`, exponential smoothing) keeps the canvas glued to the hands after `FirstPersonController` rotates the camera.
- Rigidbody is disabled + kinematic while held; restored on place/drop.
- `MuseumGameplaySetup.FixExistingHoldPoint()` (Game → Fix Carry Settings) is idempotent: it only re-applies the hold point local pos/rot, safe to run repeatedly.

**Assumptions:**
- Existing carry values are already good; the master says "tune only if needed," so I left `carryFollowSharpness`, `carryLocalEuler`, and hold point defaults unchanged and documented them instead.
- Cannot verify feel in Play mode on the cloud VM (no Unity) — recorded expected behavior for the human in `docs/MORNING_TEST.md`.

**Blockers:** none.

**Morning test steps:** pick up a floor painting; it should snap to hands instantly and stay readable/centered while you look around (no lag, no physics jitter).

## 2026-07-01T06:00Z Phase 2 — Sorting rules / wings (P0)

**Status:** done

**Files changed:**
- `Assets/Scripts/GalleryWing.cs` (new) — enum `Modern, Classical, Impressionist`.
- `Assets/Scripts/InteractablePainting.cs` — added `[SerializeField] GalleryWing wing` + `Wing` getter.
- `Assets/Scripts/PaintingMount.cs` — added `requiredWing`, `RequiredWing`, `Occupant`, and `CanAccept(InteractablePainting)`.
- `Assets/Scripts/ArtPickup.cs` — track `heldPainting`; new `HandleHeldClick()` gates placement with `CanAccept` and rejects (never drops) on wrong/occupied mounts; `GetInteractionPrompt()` now returns "Wrong gallery" / "Gallery spot taken" / "Click to place on wall".
- `Assets/Scripts/Editor/MuseumGameplaySetup.cs` — assigns wings to the existing content: north `WallMount_1..3` = Modern; `TestPainting` (Sunset Study) = Modern; `TestPainting_2` (Blue Horizon) = Classical.

**Behavior decisions (assumptions):**
- **Reject, don't drop:** clicking while aimed at a wrong-wing or occupied mount keeps the painting in hand (cozy/forgiving). Dropping only happens when clicking while NOT aimed at any mount. This diverges from the old "click = place-or-drop" so a mis-aimed click near a wall can't accidentally drop art. Documented in code.
- Removed the now-unused private `TryPlaceOnMount()` (replaced by `HandleHeldClick`).
- `GallerySection` wiring intentionally deferred to Phase 3 so this commit compiles standalone (no reference to a not-yet-created type).
- Prompt wording: "Wrong gallery" chosen per master's example; "Gallery spot taken" for occupied mounts.

**Blockers:** none.

**Morning test steps:** carry the Classical "Blue Horizon" to a Modern north-wall mount → HUD shows "Wrong gallery" and the click does not place or drop it. Carry Modern "Sunset Study" to the same mount → "Click to place on wall" → it snaps on.

## 2026-07-01T06:12Z Phase 3 — Section completion feedback (P0)

**Status:** done

**Files changed:**
- `Assets/Scripts/GallerySection.cs` (new) — groups mounts by wing; `CheckComplete()` sets `IsComplete` when all member mounts hold a correctly-sorted painting; pulsing URP emission glow; static `AllSections` registry + static `OnSectionCompleted` event; `CorrectlyFilledCount()` / `MountCount` for the HUD.
- `Assets/Scripts/PaintingMount.cs` — re-added `section` reference, `SetSection()`, and `NotifySectionChanged()` (calls `section.CheckComplete()` on place/clear).
- `Assets/Scripts/InteractionHUD.cs` — added `progressText` + `BuildProgressText()` producing lines like "Modern: 2/3 hung    Classical: 3/3 hung - done".
- `Assets/Scripts/Editor/MuseumGameplaySetup.cs` — HUD builder now creates a bottom-left "Progress" Text and wires it to `InteractionHUD.progressText`.

**Assumptions / decisions:**
- Glow uses runtime material emission (`_EMISSION` keyword + `_EmissionColor`) — URP Lit compatible, no custom shader. `GallerySection` reads `renderer.material` (instance) only in play mode (no `[ExecuteAlways]`), so it never leaks materials in the editor.
- Progress line polls `GallerySection.AllSections` each frame (simple + robust) rather than event-driven UI wiring; completion glow is event/edge-driven inside the section.
- Completion is reversible: removing a painting un-completes the section and stops the glow.
- Glow renderers default to each mount's child frame renderer when `glowRenderers` is not explicitly wired.
- The existing north-wall `WallMount_1..3` (from Museum Gameplay) are NOT grouped into a section — sections are created by Phase 4's Gallery Wings setup. So the progress HUD only shows once Gallery Wings is run.

**Blockers:** none (cannot verify glow visually on VM — documented for morning test).

**Morning test steps:** run Game → Setup Gallery Wings (Phase 4), fill one wing's 3 mounts with matching paintings → that wing's frames pulse blue and the HUD line shows "X: 3/3 hung - done".

## 2026-07-01T06:28Z Phase 4 — Staging + content (P1)

**Status:** done

**Files changed:**
- `Assets/Scripts/Editor/GalleryContentSetup.cs` (new) — menu **Game → Setup Gallery Wings**. Builds 3 wings (Modern/Classical/Impressionist), each = 3 wall mounts + 3 matching floor paintings grouped under a `GallerySection`. Wires section `sectionWing`, `mounts`, and `glowRenderers` via `SerializedObject`. Idempotent (every object guarded by unique name; re-run reports created/skipped counts).
- `Assets/Scripts/Editor/MuseumGameplaySetup.cs` — documented the recommended run order (Zero-to-One → Art Pickup → Museum Gameplay → Gallery Wings).
- `README.md` — added the new menu item and a "Gallery wings (sorting)" section.

**Layout decisions (assumptions):**
- Wings placed on separate walls to avoid colliding with the existing north-wall Museum Gameplay mounts: Modern = south (z=-24), Classical = east (x=+24), Impressionist = west (x=-24). Mount rotations chosen so frames face into the room.
- 9 distinct colors; each painting/frame gets its own `new Material(URP/Lit)` instance (falls back to Standard) to avoid the shared-default-material color-bleed bug and to give the glow an independent emission target. **Assumption:** Unity serializes these referenced non-asset materials inline into the scene on save (standard behaviour for procedural editor content); if a material ever reverts, re-run the menu.
- Floor paintings scattered in a front pile (avoiding the existing TestPainting spots) so the room reads as "mid-sorting."
- `SortingTable` left as an optional Phase 5 item.

**Blockers:** none (cannot compile/verify in Unity on the VM).

**Morning test steps:** Game → Setup Gallery Wings; confirm 3 walls each show 3 empty frames and ~9 colored slabs on the floor; sort each pile onto its wall; each completed wing glows and the HUD shows all three at "3/3 hung - done".

## 2026-07-01T06:40Z Phase 5 — Stretch goals (P2)

**Status:** partial (ranked items #1 and #3 done; #2/#4/#5 intentionally skipped)

**Files changed:**
- #1 HUD hint — `Assets/Scripts/InteractionHUD.cs` gained `hintText` + `hintMessage` ("Future: scroll to reorder stack"), shown top-center; `MuseumGameplaySetup.cs` creates + wires the Hint text.
- #3 SortingTable — `Assets/Scripts/SortingTable.cs` (new): a floor staging marker with a trigger `BoxCollider` and `Contains()` helper. `GalleryContentSetup.CreateSortingTable()` adds a thin muted slab at room-center (Default layer so pickup ignores it; trigger so it never blocks physics).

**Ranked stretch items NOT done (with reasons):**
- #2 Wrong-wing floor-painting highlight — skipped: needs runtime material pulsing that I cannot verify visually on the code-only VM; higher regression risk for low value tonight.
- #4 Escape unlocks cursor — skipped: the Unity **editor** already releases the cursor on Escape during Play mode, so this is largely redundant for the human's workflow; avoided touching `FirstPersonController` (which owns cursor lock).
- #5 Second room / additive scene — skipped: largest item, only "if everything else done," and not worth the scene-scale risk without in-Unity verification.

**Assumptions:**
- Hint is a static placeholder string only (master: "no implementation required").
- `SortingTable` is a marker (no scoring) — intentionally minimal.

**Blockers:** none.

**Morning test steps:** confirm the top-center hint text appears in Play mode; confirm a muted slab sits under the floor-painting pile and you can walk over it / drop paintings on it without being blocked.

## 2026-07-01T06:45Z Night 1 wrap-up

**Delivered:** P0 (phases 1–3) complete, P1 (phase 4) complete, P2 (phase 5) partial (#1, #3).

**New scripts:** `GalleryWing.cs`, `GallerySection.cs`, `SortingTable.cs`, `Editor/GalleryContentSetup.cs`.
**Modified:** `ArtPickup.cs`, `InteractablePainting.cs`, `PaintingMount.cs`, `InteractionHUD.cs`, `Editor/MuseumGameplaySetup.cs`, `README.md`.
**Untouched (as required):** `FirstPersonController.cs`, `Packages/manifest.json`, `Library/ Temp/ Logs/ UserSettings/`. `ProjectSettings/TagManager.asset` not hand-edited (Interactable layer already exists at index 6; editor scripts ensure it).

**Self-check performed (no Unity available):**
- Brace/paren/bracket balance verified across all 12 `.cs` files.
- No hand-made `.meta` files for new scripts.
- SerializedObject field names checked against actual private fields (`snapPoint`, `requiredWing`, `wing`, `paintingTitle`, `sectionWing`, `mounts`, `glowRenderers`, `artPickup`, `promptText`, `crosshairText`, `progressText`, `hintText`).
- Public methods referenced by setup scripts exist (`PaintingMount.CanAccept/SetSection`, `GallerySection.CheckComplete`, etc.).
- README lists the new **Game → Setup Gallery Wings** menu item.

**Cannot verify (VM is code-only):** compilation, Play-mode feel, glow visuals. Full human checklist in `docs/MORNING_TEST.md`.

**Open blocker for the human:** first Unity import is the source of truth — check the Console for compile errors and run the four setup menus in order.

---

## 2026-07-01 Night 2 — Polish bundle

**Status:** done

**Delivered (5 items):**
1. **Mount + floor aim highlights** — `MountAimHighlighter.cs`: green valid mount, red pulse wrong mount, grey occupied, subtle floor painting pulse (MaterialPropertyBlock, no material mutation).
2. **Placement pop + section banner** — `PlacementPopFeedback.cs` on hang; `InteractionHUD` listens to `GallerySection.OnSectionCompleted` for ~4s center banner.
3. **North-wall section** — `MuseumGameplaySetup.WireNorthWallGallerySection()` groups `WallMount_1..3` into `Section_ModernNorth` with label `Modern (South)` siblings get `Modern (South)` / `Classical (East)` / `Impressionist (West)` via `GalleryContentSetup`.
4. **Escape pause** — `PauseMenuController.cs` on Player: Esc toggles overlay, unlocks cursor, disables FPC/ArtPickup/Highlighter (no FPC rewrite).
5. **Setup Full Museum** — `Editor/FullMuseumSetup.cs` menu runs all four setup steps.

**Also:** clearer wrong-wing prompt (`This belongs in the … gallery`); HUD hint updated; Museum Gameplay HUD setup now **upgrades** existing canvas (Banner, PausePanel) instead of skipping.

**Assumptions:** Highlights need URP Lit + `_EmissionColor` in MaterialPropertyBlock (standard URP). Pause does not set `Time.timeScale = 0` so glow/banner timers keep working. Placement pop uses `Time.deltaTime` (normal play only).

**Cannot verify on VM:** highlight colors, pop timing, banner visibility — see updated `docs/MORNING_TEST.md`.

---

## 2026-07-01 Night 3 + 4 — Win state, stack select, staging, save, tests

**Status:** done (code-only)

**Delivered:**
1. **Scroll wheel stack selection** — `ArtPickup`: when carrying 2+, mouse wheel cycles `activeStackIndex`; active item scales forward; HUD stack line + prompts show `(n/N)`.
2. **Right-click undo** — last wall placement returns painting to stack; `PaintingMount.TakeDownPainting()`; `MuseumGameEvents.PlacementUndone`.
3. **Win state** — `MuseumProgress` + `InteractionHUD` win overlay when all `GallerySection`s complete; pickup blocked after win.
4. **Sorting table staging** — `SortingTable` grid snap on drop; `TotalStagedCount` on HUD bottom-right.
5. **Tutorial hints** — `TutorialHints` one-shot PlayerPrefs tips (pickup, wrong wing, scroll); HUD tutorial banner.
6. **Save/load** — `MuseumSaveManager` persists mount↔painting names in PlayerPrefs JSON.
7. **Events** — `MuseumGameEvents` static hooks for SFX/UI extension.
8. **Active carry highlight** — `MountAimHighlighter` pulses the forward stack item when count > 1.
9. **Validator + tests** — `Game → Validate Museum Scene`; EditMode `MuseumGameplayTests` (CanAccept + section completion).

**Setup menu updates:** `MuseumGameplaySetup` adds WinPanel, StackLabel, StagingLabel, TutorialTip; wires `MuseumProgress` + `MuseumSaveManager` on Player.

**Assumptions:** Scroll uses Input System `Mouse.current.scroll`; save keys objects by `GameObject.name` (stable after setup menus). Undo stack is in-memory (not persisted across sessions).

**Cannot verify on VM:** compile, Play mode, Test Runner — see `docs/MORNING_TEST.md`.

---

## 2026-07-01 Night 5 — Game loop closure + save v2 + settings + audio hooks

**Status:** done (code-only)

**Delivered:**
1. **New game / reset** — `MuseumStateReset`, pause + win **New game** buttons, `MuseumGameFlowController`.
2. **Win flow** — modal win blocks movement; **Continue exploring** or **New game**; win state saved in v2 save.
3. **Save v2** — stable `MuseumEntityId` on mounts/paintings; staged sorting-table items + `museumComplete` flag; v1 migration.
4. **PaintingDefinition SO** + **Game → Create Painting Definition Assets** catalog.
5. **Settings** — pause submenu: sensitivity, FOV, invert Y, volume (`PlayerSettingsStore`).
6. **SFX stub** — `MuseumAudioDirector` on `MuseumGameEvents` (clips optional in Inspector).
7. **Take down from wall** — HUD prompt `E — take down "…"` for hung paintings.
8. **North demo** — third Modern floor painting `TestPainting_3` (Amber Grid) for 3 north mounts.
9. **Menus** — **Clear Museum Save**; validator checks flow/audio/settings/entity ids.

**Assumptions:** UI buttons wired via persistent UnityEvents during setup menu. Sliders use minimal Unity UI (may need graphic tuning in Editor). Audio clips not bundled — assign when ready.

**Cannot verify on VM:** Play mode, button wiring, settings sliders — re-run **Setup Museum Gameplay** or **Setup Full Museum** after pull.

---

## 2026-07-01 Night 7 — Controls help, gamepad settings, objective HUD, footsteps

**Delivered:**
- **Controls help panel** — pause + main menu; keyboard + gamepad reference
- **Gamepad look sensitivity** slider + **Reset defaults** in settings
- **Objective line** on HUD (wings completed count)
- **Richer mount prompts** — wing name, painting title when hanging
- **Mount placards billboard** toward player
- **FootstepController** hook (assign clip on Player)
- **UI click SFX** hook on MuseumAudioDirector

---

## 2026-07-01 Night 6 — Gamepad, placards, throw juice, dev cheats

**Delivered:**
- **MuseumInput** — keyboard + gamepad (A pick up, B throw, X place, Y undo, bumpers stack, Start pause, right stick look)
- **Mount wing placards** — world-space labels on frames; auto on setup
- **Staged painting highlight** — cyan pulse + HUD prompt on sorting table items
- **Throw camera kick** — brief FOV punch on throw
- **Dev cheats** — F9/F10/F8 in Editor/Development builds
- **PaintingDefinition linker** menu; Full Museum runs catalog + link + placards
