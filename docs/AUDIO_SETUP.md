# Audio setup (optional)

Gameplay SFX play **out of the box** via tiny runtime-generated tones when clips are not assigned (`MuseumProceduralSfx`). Toggle **Settings → Placeholder SFX** (or disable on `MuseumAudioDirector` / `FootstepController` / `JumpLandAudio`) for silent mode until real assets are wired.

## Quick path

1. Pull latest and run **Game → Setup Full Museum** (wires `MuseumAudioDirector`, `FootstepController`, and `JumpLandAudio` on Player).
2. Press Play — you should hear pickup/place/wrong/footstep placeholders without assigning anything.
3. *(Optional polish)* Select **Player** and assign short one-shot `.wav` / `.ogg` clips on **Museum Audio Director**:

| Field | When it plays |
|-------|----------------|
| Pickup Clip | Pick up / stack a painting |
| Place Clip | Hang on a wall mount |
| Drop Clip | Drop on the floor |
| Wrong Wing Clip | Wrong gallery wing rejected |
| Wrong Slot Clip | Wrong reserved slot (falls back to Wrong Wing Clip) |
| Section Complete Clip | A wing section finishes |
| Museum Win Clip | Entire museum complete |
| Undo Clip | Undo last hang |
| Throw Clip | Throw painting (Q / B) |
| Stage Clip | Painting snaps onto sorting table |
| Ui Click Clip | Menu button clicks (via `MuseumUiActions`) |

4. On **Footstep Controller** and **Jump Land Audio**, assign **Footstep / Jump / Land** clips to override placeholders.

Assigned clips always take priority over synthesized fallbacks.

## Ambience loop

`MuseumAmbienceController` on Player loops a subtle procedural hum by default (child `AmbienceAudio`). Assign **Ambient Clip** to replace it. Stops during pause, menus, and win modal. Scales with master volume.

## Sprint audio & feel

- **Footstep Controller** — faster cadence + louder steps while sprinting (Shift / LT)
- **Sprint Camera Feel** on PlayerCamera — +4° FOV while sprinting (defers to throw/reject FOV kicks)
- **First Person Controller** — faster head bob while sprinting

## Volume & placeholder toggle

- **Settings → Master Volume** scales SFX through `PlayerSettingsStore.MasterVolume`.
- **Settings → Placeholder SFX** persists in `PlayerSettingsStore.UseSynthesizedSfx` (default **on**).

## Free placeholder clips

- Record 0.1s silence in any DAW and export as `.wav` to verify wiring.
- Unity Asset Store / freesound.org for CC0 UI blips and footsteps.
- Keep clips **mono**, **< 500 KB**, and **non-looping** for one-shots.

## Re-run after pulls

Editor setup menus re-add components but **do not** overwrite your clip assignments on existing Player objects unless you delete and recreate Player. Re-run **Setup Full Museum** to add new settings UI rows (e.g. Placeholder SFX toggle).
