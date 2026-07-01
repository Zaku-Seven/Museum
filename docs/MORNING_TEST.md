# Morning Test — Arcane Museum Feel

A ~15-minute human checklist. Gameplay is **feature-complete** after setup; see `docs/FEATURE_COMPLETE.md` and `docs/VISUAL_POLISH.md`.

## Setup

1. `git fetch && git checkout feature/arcane-museum-feel && git pull`
2. Open the project in Unity **6000.5.1f1**.
3. Open `Assets/Scenes/SampleScene.unity`.
4. Let scripts compile. Check the **Console** for red errors.
5. **Game → Setup Full Museum** (runs validator at end).
6. **Game → Validate Museum Scene** — target **0 errors**.

## Tests

- **EditMode:** Test Runner → Run All (`MuseumGameplayTests`, 15+ tests)
- **PlayMode:** Test Runner → Run All (`MuseumPlayModeSmokeTests`, 3 tests)

## Play mode checklist

1. Press **Play** → main menu → **Continue** or **Enter museum**.
2. UI buttons work (EventSystem present).
3. Pick up, stack (scroll / LB RB), place, wrong wing reject, slot reject.
4. Throw onto sorting table (catch), journal (**J** / **R3**), compass, ghost preview.
5. Complete one section → banner + glow.
6. Save/load: hang paintings, stop Play, **Continue** restores.
7. Complete all **4 sections** → win overlay.
8. Pause → Settings, Collection, Controls; **B** resumes from pause.

## Gamepad row

- A pick up, B throw, X place, Y undo, LB/RB stack, L3 jump, LT sprint, View inspect, R3 journal, Start pause.

## Remaining work after this passes

**Visual/audio only** — assign clips (`docs/AUDIO_SETUP.md`), replace greybox art, lighting polish (`docs/VISUAL_POLISH.md`).

Multiplayer / VoIP: out of scope (`docs/MULTIPLAYER_VOIP.md`).
