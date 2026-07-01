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
