# Thorough museum setup

One-click path after pulling latest:

1. Open `Assets/Scenes/SampleScene.unity`.
2. **Game → Build Museum Now (Recommended)** — or **Game → Setup Full Museum** (same content, runs all setup steps).
3. **Game → Validate Museum Scene** — target 0 errors
4. Press Play

## What Full Museum now includes

| Layer | Content |
|-------|---------|
| **Architecture** | Ceiling, atrium pillars, welcome sign, wing banners, floor inlay |
| **Expansion** | North archive wing (3 mounts), east/west alcoves, corridor, wainscoting, vitrines, 2 sorting tables, accent lighting |
| **Atmosphere** | Global URP Volume (`Assets/Settings/MuseumAtmosphereProfile.asset`) |
| **Gameplay** | 5 sections, 15 wall mounts, sorting tables, full HUD guidance |
| **Audio** | Procedural SFX + ambience until real clips assigned |
| **Guidance** | Next objective line, wing breakdown, wrong-wing compass hints |
| **Accessibility** | Settings → Reduce motion (less bob, FOV, particles, pulses) |

## Optional individual menus

- **Game → Expand Museum Building** — north archive, alcoves, props, lighting (also runs inside Full Museum)
- **Game → Setup Museum Architecture** — greybox ceiling/signage only
- **Game → Setup Museum Atmosphere** — volume profile only
- **Game → Create Painting Definition Assets** — refresh catalog SOs
- **Game → Link Painting Definitions To Scene** — apply lore/colors to cubes

## Dev QA (Editor / Development builds)

| Key | Action |
|-----|--------|
| F5 | Spawn test painting |
| F6 | Teleport to nearest valid mount |
| F7 | Debug overlay (progress + stats) |
| F8 | Manual save |
| F9 | Fill one wing section |
| F10 | Auto-hang all matching |
| F11 | Force museum win flow |

## Journal filters

Open journal (**J** / pause → Collection). Use **All · Unhung · Hung · Staged** buttons (added by setup upgrade).

## Still human art pass

Replace greybox cubes, tune Volume profile, assign real audio — see `docs/VISUAL_POLISH.md`.
