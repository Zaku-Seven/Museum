# Night 1 Master Prompt — Arcane Museum Feel

**Repository:** `Zaku-Seven/Museum`  
**Branch:** `feature/arcane-museum-feel`  
**Unity (local only):** 6000.5.1f1, URP, Input System only (`activeInputHandler: 1`)  
**Scene:** `Assets/Scenes/SampleScene.unity`  
**Human workflow:** Windows + Unity Editor. Cloud agent is **code-only** — no Unity on VM.

---

## Autonomy contract (read first)

- **Do NOT ask the user questions.** Make reasonable decisions and document them in `OVERNIGHT_LOG.md` under **Assumptions**.
- **Do NOT stop** after environment setup, licensing errors, or “cannot verify in Unity” messages. Those are expected on the cloud VM.
- **Do NOT install Unity**, activate a license, or run batchmode compile on the cloud VM.
- Work until **P0 phases (1–3) are complete** and **P1 phase (4) is done or partially done with clear notes**, OR you hit a hard blocker documented in `OVERNIGHT_LOG.md`.
- If P0–P1 finish early, proceed to **Phase 5 stretch goals** in ranked order.
- If you run low on context, **commit, push, update `OVERNIGHT_LOG.md`, and continue** the next subtask.

---

## NEVER (instant violation)

- Install Unity, activate license, or run `Unity -batchmode` on the cloud VM
- Modify `Library/`, `Temp/`, `Logs/`, `UserSettings/`
- Hand-edit `SampleScene.unity` YAML at scale — use **Editor setup scripts** only
- Create `.meta` files with hand-made GUIDs (omit `.meta` for new scripts; Unity regenerates locally)
- Force-push `main` or delete remote branches
- Commit secrets, `.env`, license files, credentials
- **Rewrite from scratch:** `FirstPersonController.cs`, `ArtPickup.cs` — **extend only**
- Add Asset Store or paid packages
- Spend **>15 minutes** on any single blocker — log it, skip or stub, move on
- Block tonight’s work on merging PR #1 (see below)

---

## Design north star: Arcane Library → Museum

Inspired by **Librarian: Tidy Up the Arcane Library** — cozy first-person sorting, not combat.

| Arcane Library | Museum equivalent |
|----------------|-------------------|
| Pick up books | Pick up floor paintings |
| Read title / clues | Painting title + wing label (color-coded) |
| Carry / reorder stack | Single carry (stack is stretch) |
| Precise shelf placement | Snap to `PaintingMount` on wall |
| Wrong shelf rejected | Wrong wing → HUD message, no snap |
| Row/section complete glow | `GallerySection` complete → blue emission pulse |
| Staging table | Floor pile / sorting zone |
| Progress satisfaction | HUD progress line (“2/3 Modern wing”) |
| No hard timer | No fail state tonight |

**Feel targets** (human verifies in Play mode):

- Pickup snaps to hands immediately; no floaty lag behind mouse
- Canvas readable while carried (faces player)
- Wrong mount shows **“Wrong gallery”** (or similar) — never silent fail
- Correct empty mount shows **“Click to place on wall”**
- Completing a wing feels rewarding (visible room change)
- Cozy pace: no timer, no combat, no fail state tonight

---

## Architecture (extend, don't replace)

```
Player (CharacterController + FirstPersonController)
  └── PlayerCamera (Camera + ArtPickup + InteractionHUD)
        └── HoldPoint (local ~0, -0.22, 0.68)

InteractablePainting  — title, wing, CurrentMount
PaintingMount         — wall frame, SnapPoint, occupancy, required wing
GallerySection (NEW)  — groups mounts, tracks completion, triggers glow
SortingTable (NEW)    — optional floor zone for unpainted pile

Flow:
  Raycast (Interactable layer, 3.5m) → pick up → carry (LateUpdate lerp) →
  raycast mount → validate wing → place OR drop (click empty space)
  GallerySection.CheckComplete() → glow + progress HUD update
```

### Existing scripts (read before editing)

| File | Role |
|------|------|
| `Assets/Scripts/FirstPersonController.cs` | FPS move/look/jump — **do not rewrite** |
| `Assets/Scripts/ArtPickup.cs` | Pickup, carry, place, drop, prompts |
| `Assets/Scripts/InteractablePainting.cs` | Title metadata, mount tracking |
| `Assets/Scripts/PaintingMount.cs` | Wall snap, occupancy |
| `Assets/Scripts/InteractionHUD.cs` | Crosshair + prompt text |
| `Assets/Scripts/Editor/PrototypeSceneSetup.cs` | Room + player bootstrap |
| `Assets/Scripts/Editor/ArtPickupSceneSetup.cs` | Interactable layer + test painting |
| `Assets/Scripts/Editor/MuseumGameplaySetup.cs` | Mounts, HUD, extra paintings |

**Interactable layer** is index **6** in `ProjectSettings/TagManager.asset`. Editor scripts must ensure it exists before assigning layers.

---

## Default decisions (use unless strong reason not to)

| Decision | Default |
|----------|---------|
| Carry model | Single painting at a time |
| Drop | Left click while not on valid target = drop (keep current) |
| Wrong placement | Reject + HUD message; do not snap |
| Section complete | All mounts in `GallerySection` occupied with **correct wing** |
| Wings | `Modern`, `Classical`, `Impressionist` (enum or serializable) |
| Test content | **9 paintings** (3 per wing), **3 mounts per wing** (9 mounts total) |
| Feedback | Material emission pulse on section complete + HUD text (no audio tonight) |
| Progress UI | Screen-space Text, bottom area — extend `InteractionHUD` |
| Branch | `feature/arcane-museum-feel` |
| PR | Open to `main` when P0 complete minimum |

---

## Phases & time budget

| Phase | Priority | Hours (approx) | Must complete? |
|-------|----------|----------------|----------------|
| 1 — Carry feel polish | P0 | 1–2 | Yes |
| 2 — Sorting rules (wings) | P0 | 2–3 | Yes |
| 3 — Section completion feedback | P0 | 1–2 | Yes |
| 4 — Staging + content | P1 | 2 | Should |
| 5 — Stretch goals | P2 | remainder | If time |

**If behind schedule:** cut Phase 5 entirely; reduce to **6 paintings / 2 wings**; keep P0–P1.

---

### Phase 1 — Carry feel polish (P0)

**Goal:** Tight, readable carry — no regression from current fix.

- Review `ArtPickup.cs` carry path: `LateUpdate` lerp, Rigidbody disabled while held, immediate snap on pickup
- Tune only if needed: `carryFollowSharpness` (~28), `carryLocalEuler`, hold point defaults
- Ensure `GetInteractionPrompt()` stays accurate for all states
- Run **Game → Fix Carry Settings** logic remains idempotent in `MuseumGameplaySetup.cs`

**Done when:** Code comments + `OVERNIGHT_LOG.md` note that carry behavior is unchanged or improved with parameter values documented.

---

### Phase 2 — Sorting rules (P0)

**Goal:** Paintings belong to a wing; mounts accept only matching wing.

**Create:**

- `GalleryWing` enum (Modern, Classical, Impressionist)
- Extend `InteractablePainting` with `[SerializeField] GalleryWing wing` (+ public getter)
- Extend `PaintingMount` with `[SerializeField] GalleryWing requiredWing`

**Modify:**

- `ArtPickup.TryPlaceOnMount()` — call `mount.CanAccept(painting)` before place
- `ArtPickup.GetInteractionPrompt()` — show **“Wrong gallery”** when mount wing mismatches
- Reject placement without consuming click wrongly (drop only when not looking at occupied/wrong mount — use judgment; document behavior)

**Done when:** Validation logic exists in code; Editor setup assigns wings to test paintings and mounts.

---

### Phase 3 — Section completion feedback (P0)

**Goal:** When all mounts in a wing section are correctly filled, player gets satisfying feedback.

**Create:**

- `Assets/Scripts/GallerySection.cs`
  - References list of `PaintingMount` in this section
  - `GalleryWing sectionWing`
  - `CheckComplete()` — all mounts occupied AND occupant painting wing matches
  - On complete: trigger glow (enable emission on frame renderers or dedicated highlight material)
  - Event or callback for progress UI (`OnSectionCompleted`)

**Modify:**

- `PaintingMount.PlacePainting()` — notify parent `GallerySection` after successful place
- `InteractionHUD` — progress line: e.g. `"Modern: 2/3 hung"` per wing or total

**Done when:** Completion logic is wired; glow uses simple URP-compatible emission (no custom shaders required tonight).

---

### Phase 4 — Staging + content (P1)

**Goal:** Room reads as “museum getting organized,” not empty grey box.

**Create:**

- `Assets/Scripts/Editor/GalleryContentSetup.cs` with menu **Game → Setup Gallery Wings**
  - Idempotent: safe to run twice (check `GameObject.Find` before create)
  - Creates 3 wing zones (empty parent transforms + labels in object names)
  - Creates 9 floor paintings with distinct colors per wing
  - Creates 9 wall mounts grouped into 3 `GallerySection` objects
  - Wires `GallerySection` → mount lists via SerializedObject

**Modify:**

- `MuseumGameplaySetup.cs` — call into shared helpers or document run order: Zero-to-One → Museum Gameplay → Gallery Wings
- `README.md` — new menu item + wing sorting explanation

**Optional:**

- `SortingTable.cs` — simple collider zone marking floor staging area (visual cube, muted color)

**Done when:** One menu command builds wing content; README updated.

---

### Phase 5 — Stretch goals (P2, strict order)

Only after P0–P1 complete. Stop when tired.

1. HUD hint: “Future: scroll to reorder stack” (no implementation required)
2. Simple highlight on wrong-wing floor paintings (material color pulse or outline — keep minimal)
3. `SortingTable` visual + trigger zone
4. Escape key unlocks cursor / simple pause (Input System)
5. Second room or additive scene — **only if everything else done**

---

## Expected files

### New (minimum)

```
Assets/Scripts/GalleryWing.cs          (enum)
Assets/Scripts/GallerySection.cs
Assets/Scripts/Editor/GalleryContentSetup.cs
OVERNIGHT_LOG.md                     (create/update throughout night)
docs/MORNING_TEST.md                 (human 10-minute checklist — fill before final commit)
```

### Modify (expected)

```
Assets/Scripts/ArtPickup.cs
Assets/Scripts/InteractablePainting.cs
Assets/Scripts/PaintingMount.cs
Assets/Scripts/InteractionHUD.cs
Assets/Scripts/Editor/MuseumGameplaySetup.cs
README.md
```

### Do not touch unless necessary

```
Assets/Scripts/FirstPersonController.cs
ProjectSettings/* (except TagManager if Interactable layer missing — prefer Editor script)
Packages/manifest.json
Library/, Temp/, Logs/, UserSettings/
```

---

## Editor menu pattern

All scene changes go through **Game/** menu items:

| Menu item | Purpose |
|-----------|---------|
| Game → Setup Zero-to-One Prototype | Room, player, sun (existing) |
| Game → Setup Art Pickup Test | Layer + first painting (existing) |
| Game → Setup Museum Gameplay | Mounts, HUD (existing) |
| Game → Fix Carry Settings | Hold point reset (existing) |
| Game → Setup Gallery Wings | **NEW** — wings, sections, 9 paintings, 9 mounts |

**Idempotency rules:**

- Use `GameObject.Find(name)` — skip if exists
- Use `EditorPrefs` keys like existing `MuseumGameplaySetupComplete` pattern
- Mark scene dirty + save open scenes after setup
- Log clear success message with what was created vs skipped

---

## Git & PR workflow

1. **Checkout/create branch:** `feature/arcane-museum-feel` from latest `main`
2. **Pull** before starting if branch exists remotely
3. **Commit cadence:** after every phase OR every ~90 minutes — whichever comes first
4. **Commit message style:** short imperative — e.g. `Add gallery wing validation to wall mounts`
5. **Push** after every commit
6. **Open PR to `main`** when P0 (phases 1–3) is complete — title: `Arcane Library-style museum sorting (night 1)`
7. PR body must include:
   - Summary bullets
   - Link to `docs/MORNING_TEST.md`
   - Known gaps / assumptions
   - **Test plan** checklist for human

**Do not:** force-push, amend unpushed commits unnecessarily, or commit binary Library assets.

---

## PR #1 (existing environment setup)

- PR #1 may add `AGENTS.md` for Unity-on-Linux — **we are NOT using that path**
- Do not add Unity credentials or pursue license activation
- Optional: add a note in PR #1 comment that local Windows workflow is primary — do not block on merging
- Your deliverable is a **separate PR** for gameplay work

---

## Progress protocol

After **every phase** (or every **90 minutes**):

1. Update `OVERNIGHT_LOG.md`:

```markdown
## [UTC timestamp] Phase N — Title

**Status:** done | partial | skipped

**Files changed:**
- ...

**Assumptions:**
- ...

**Blockers:**
- ...

**Morning test steps:**
1. ...
```

2. `git add` relevant files → commit → push

**Final commit message:** `Night 1 complete: [one-line summary]`

---

## Self-check before each commit (no Unity required)

- All new `.cs` files: valid C# syntax, correct namespace (none / global matching existing scripts)
- No duplicate class names
- `[SerializeField]` names in Editor setup match actual private fields
- New public methods referenced by setup scripts exist
- No references to missing layers without Editor script creating them
- README lists any new **Game/** menu items
- `docs/MORNING_TEST.md` updated with numbered Play steps

---

## docs/MORNING_TEST.md (required deliverable)

Write this for a human with **10 minutes**. Include:

1. Branch to checkout: `feature/arcane-museum-feel`
2. Unity version: 6000.5.1f1
3. Menu items to run (in order)
4. Play mode steps (numbered): walk, pick up, place, wrong-wing test, section complete
5. Expected HUD prompts and glow behavior
6. Known bugs / not implemented yet
7. If setup script couldn't wire something: manual Inspector steps

---

## Failure recovery

| Blocker | Action |
|---------|--------|
| Can't compile/test Unity on VM | Ship code; note in `MORNING_TEST.md` |
| Scene YAML conflict | Revert scene changes; Editor script only |
| Editor script won't compile | Fix before next phase |
| Unsure of Arcane mechanic | Simplest version + log assumption |
| SerializedObject field not found | Use public setter or Awake default |
| Low on context | Commit, push, log, continue |

---

## Definition of done (night 1)

**Minimum (ship PR):**

- [ ] Phases 1–3 complete
- [ ] Wing validation works in code
- [ ] `GallerySection` completion + glow wired
- [ ] `GalleryContentSetup` Editor menu exists
- [ ] `OVERNIGHT_LOG.md` has phase-by-phase entries
- [ ] `docs/MORNING_TEST.md` complete
- [ ] PR open to `main`

**Ideal:**

- [ ] Phase 4 complete (9 paintings, 9 mounts, 3 sections)
- [ ] Progress HUD shows wing counts
- [ ] README updated

**Stretch:**

- [ ] Phase 5 items as time allows

---

## Kickoff reminder

If you were started with a short cloud prompt, re-read this entire file now. It is the source of truth for tonight's work.
