# Whitebox Museum layout

Cozy curation puzzle architecture: **Grand Lobby** hub, **Pre-1900 Art** wing (left), **Fossil & Antiquity** wing (right).

## Build in Unity

1. Open your active scene (e.g. `SampleScene.unity`).
2. Ensure a **Player** exists (**Game → Setup Zero-to-One Prototype** if needed).
3. Run **Game → Build Whitebox Museum**.

The menu creates:

| Node | Contents |
|------|----------|
| `Museum_Architecture/Grand_Lobby` | 20×20 plane floor, 10-unit walls, 4-unit L/R archways, 4 pillars |
| `Museum_Architecture/Art_Wing_Pre1900` | 10×30 gallery (−X), warm walls, 5 partition walls |
| `Museum_Architecture/Fossil_Wing` | 25×25 cavern (+X), slate materials, central dais + ramps |
| `Museum_Architecture/Whitebox_Lighting` | Lobby chandelier point light + art wing spotlights |

Directional light is angled for dramatic archway shadows. Player spawns at lobby center `(0, 1, 0)`.

Re-run the menu anytime to rebuild (idempotent).

## MCP note

Live scene editing via Unity MCP requires the Unity MCP server connected in Cursor. This repo ships the **Editor builder** above as the reliable path when MCP is unavailable.
