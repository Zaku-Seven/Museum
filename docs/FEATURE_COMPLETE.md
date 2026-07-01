# Feature complete — gameplay checklist

After **Game → Setup Full Museum** and a clean **Validate Museum Scene** (0 errors), this prototype is **gameplay-complete**. Remaining work is **visual and audio polish** only (see `docs/VISUAL_POLISH.md`).

## Core loop (done)

- Pick up / stack / scroll or LB·RB select active item  
- Wing + optional slot validation on mounts  
- Place, drop, throw, undo  
- Sorting table staging with save  
- Section completion glow + museum win  
- Save v2 + Continue / New game  

## UI & flow (done)

- Main menu, pause, settings, controls help, collection journal  
- Win overlay, progress HUD, wing guide, compass, placement ghost  
- EventSystem + Input System UI module (created by setup)  
- Time freeze on pause / menus  

## Input (done)

| Keyboard | Gamepad |
|----------|---------|
| WASD, Shift sprint, Space jump | Sticks, LT sprint, L3 jump |
| E, Q, click, right-click, scroll | A, B, X, Y, LB/RB |
| Tab inspect, J journal, Esc pause | View inspect, R3 journal, Start pause |

Gamepad **Collection**: pause → **Collection**. Close overlays: **Esc / Start / B**.

## Editor one-time setup (required once per machine / after pull)

1. Open `Assets/Scenes/SampleScene.unity`  
2. **Game → Setup Full Museum**  
3. **Game → Validate Museum Scene** → 0 errors  
4. **Test Runner** → EditMode + PlayMode → Run All  

## Explicitly out of scope (not needed for “visual polish only”)

- Multiplayer / VoIP (`docs/MULTIPLAYER_VOIP.md`)  
- Real 3D art, lighting rigs, baked GI  
- Licensed audio (hooks exist; clips are optional)  
- Input rebinding UI  

## Human sign-off

Run `docs/MORNING_TEST.md` (~15 min). If Console is clean and all sections complete to win, ship art/audio pass next.
