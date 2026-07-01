# Visual polish pass — what’s left

Gameplay systems are complete. This document is the art/audio/environment checklist.

## Art & environment

- [ ] Replace cube paintings with framed canvases / meshes (keep `InteractablePainting` + collider size similar)  
- [ ] Museum architecture: walls, floors, ceiling, molding, signage (replace greybox `Environment`)  
- [ ] Wing identity: color palette, floor materials, accent lights per wing  
- [ ] Sorting table: wood/marble slab mesh instead of flat cube  
- [ ] Player hands / gloves or carry anchor VFX (optional)  
- [ ] Mount frames: ornate vs modern vs classical variants per wing  

## Lighting & atmosphere

- [ ] Baked or real-time GI tuned for gallery readability  
- [ ] Spot/accent lights on hung art (completion glow can stay emission-based)  
- [ ] Subtle fog or post-processing (URP Volume) for “arcane museum” mood  
- [ ] Win / section-complete moment: optional particle burst (code hooks via `MuseumGameEvents`)  

## Audio (assign in Inspector to replace placeholders)

See `docs/AUDIO_SETUP.md`. **Synthesized one-shots play automatically** when clips are empty; assign real assets to replace them.

- [ ] Pickup, place, drop, wrong wing/slot, throw, stage *(placeholders ship in code)*
- [ ] Section complete, museum win, UI click *(placeholders ship in code)*
- [ ] Footsteps (surface variants optional) *(placeholder footstep tone)*
- [ ] Jump / land *(placeholder tones)*
- [ ] Optional ambient loop on Player *(procedural hum ships via `MuseumAmbienceController`)*

## UI polish

- [ ] Replace `LegacyRuntime.ttf` with project font  
- [ ] Button sprites / nine-slice panels (pause, main menu, journal)  
- [ ] Progress bar frame art  
- [ ] Compass needle icon instead of `▲` text  

## Content (optional)

- [ ] Expand `PaintingDefinition` catalog + run **Link Painting Definitions To Scene**  
- [ ] Swap `PaintingDefinition.prefab` for real meshes  
- [ ] Second room / wing (additive scene) — stretch goal  

## Not required for polish milestone

- Networking, VoIP, Steam, achievements  
- Localization  
- Rebindable controls  
