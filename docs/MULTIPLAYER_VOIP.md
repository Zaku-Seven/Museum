# Multiplayer & VoIP — scope for this project

This prototype is **single-player first**. Multiplayer and voice chat are possible later, but they are a **separate project phase** — not a quick add-on.

## Can the cloud agent implement them?

| Feature | Code scaffolding | Fully working without you |
|---------|------------------|---------------------------|
| **Throw / local physics** | Yes | You test in Unity Play mode |
| **Co-op multiplayer** | Partial (architecture + Netcode/Mirror setup) | No — needs Editor, builds, 2+ clients, networking |
| **VoIP** | Partial (integration stubs) | No — needs third-party accounts, mic permissions, builds |

The agent can write C# and add packages, but **cannot** run two Unity instances, authenticate Vivox/Discord, or verify latency and desync in Play mode.

## Multiplayer options (when you are ready)

### 1. Unity Netcode for GameObjects (recommended if you stay in Unity ecosystem)

- Host / client or dedicated server
- Sync player transforms, carried painting ownership, mount state
- **Hard parts:** who owns a painting in the stack, conflict when two players grab the same piece, save/load authority

### 2. Mirror / FishNet

- Similar to Netcode; mature community patterns for small co-op games

### 3. Photon / Unity Gaming Services

- Faster lobby + relay; less server ops
- Ongoing service dependency

### Suggested co-op design for *this* game

- **2–4 curators**, shared museum progress
- **Painting authority:** server assigns `NetworkObject` per painting; only owner simulates carry physics
- **Mounts:** server validates `CanAccept` before `PlacePainting`
- **Throw:** replicate impulse + Rigidbody; sorting table catch stays server-side
- **No competitive griefing** in v1 — shared win state only

Estimated engineering (real work, not calendar time): networking layer + authority refactor on `ArtPickup`, `PaintingMount`, `MuseumSaveManager`, and UI for lobby/invite.

## VoIP options

VoIP is **never “free” in Unity** — you integrate a SDK:

| SDK | Notes |
|-----|--------|
| **Vivox** (Unity) | Common in games; Unity dashboard setup |
| **Dissonance** | Works with Mirror/FishNet; positional voice fits museum rooms |
| **Discord / Steam** | External; players use overlay — zero in-game integration |
| **WebRTC** | Roll-your-own; high effort |

For a cozy museum, **Dissonance + positional audio** (louder near a friend in the east wing) matches the fantasy. Still requires package install, scene objects, and push-to-talk UI.

## What we did instead (this iteration)

- **Q — throw painting** (local physics, sorting table catch)
- **Main menu** with Continue / New game
- **Event hooks** (`MuseumGameEvents`) ready for future network fan-out

## If you want to proceed with multiplayer

Tell the agent explicitly:

1. **Player count** (2 co-op vs 4)
2. **Stack choice:** Netcode vs Mirror vs Photon
3. **VoIP:** yes/no, and which SDK if yes
4. **Dedicated server** or host-as-client only

The agent can then add a `feature/multiplayer` branch with package manifests, a `NetworkMuseumBootstrap`, and networked wrappers — still requiring your Unity machine for all testing.
