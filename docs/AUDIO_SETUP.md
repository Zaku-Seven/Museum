# Audio setup (optional)

All gameplay audio hooks are **null-safe** until you assign clips in the Inspector. The game runs silently for SFX until then.

## Quick path

1. Pull latest and run **Game → Setup Full Museum** (wires `MuseumAudioDirector` on Player and `FootstepController`).
2. Select **Player** in the hierarchy.
3. On **Museum Audio Director**, assign short one-shot `.wav` / `.ogg` clips:

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
| Ui Click Clip | Menu button clicks (via `MuseumUiActions`) |

4. On **Footstep Controller** (same Player), assign a **Footstep Clip** — plays while walking on the ground.

## Volume

**Settings → Master Volume** scales SFX through `PlayerSettingsStore.MasterVolume`. Re-open settings or restart Play mode after changing volume in code.

## Free placeholder clips

- Record 0.1s silence in any DAW and export as `.wav` to verify wiring.
- Unity Asset Store / freesound.org for CC0 UI blips and footsteps.
- Keep clips **mono**, **&lt; 500 KB**, and **non-looping** for one-shots.

## Re-run after pulls

Editor setup menus re-add components but **do not** overwrite your clip assignments on existing Player objects unless you delete and recreate Player.
