# A New World (MonoGame RPG prototype)

Short description
- 2D top-down prototype built with MonoGame (DesktopGL) on .NET 10
- ECS with DefaultEcs
- Tiled maps via TiledSharp + custom TMX renderer (animated tiles, culling)
- Player movement + 4-directional animation
- Interaction + Dialogue (choices, conditions/actions, variables, typewriter)
- HUD in virtual space; Debug overlay with minimap
- Basic audio system (one-shots + loop bus)
- NPC system (spawn rules, behaviors: idle/patrol/wander, face player on interact, animated sprites)

Controls
- Move: WASD / Arrow keys
- Interact: E
- Advance dialogue / Select choice: Space / Enter (Up/Down to change selection)
- Toggle debug overlay: F3

Build & run
- Requirements: .NET 10 SDK, MonoGame 3.8, MGCB Content Pipeline
- Open ANewWorld/ANewWorld.csproj in VS or run `dotnet build` then start the app
- Ensure Content.mgcb is built; add SFX to Content/Audio and reference by asset name

Project structure (high level)
- Engine/Tilemap/Tmx: TMX loading and renderer
- Engine/Systems: ECS systems (input, movement, animation, dialogue, audio, NPC AI, etc.)
- Engine/UI: HUD rendering
- Engine/Audio: SFX/events/bus
- Engine/Npc: NPC definitions, spawn rules, service, animation builder
- Content: mgcb-managed content (maps, textures, audio, NPCs, dialogues)

## TODO (working list)

Done
- Animated tiles (layers + tile objects) with per-gid atlas
- Player animation (enum-based clips, 4 directions)
- Frustum culling (tiles + entities) + minimap
- Object-layer rectangles/polygons as colliders
- Interaction system (prompt + events)
- Dialogue system (choices, conditions/actions, variables, typewriter, pause via GameState)
- Dialogue HUD in virtual space (wrapped text, name box)
- Audio: one-shots + loop bus, typewriter loop
- Cleanup/perf: renderer buffer reuse; comprehensive unit tests
- Virtual screen = backbuffer (dynamic resize)
- NPC core system (data-driven spawning, behaviors: idle/patrol/wander, face player, restore behavior)
- NPC tests (movement, brain, interaction, service - 28 tests)
- NPC animation integration (parse clips from JSON, build dictionaries, AnimationStateSystem integration)
- Interaction prompt indicator (bobbing speech bubble above interactable NPCs)

### NPC Todo
Phase 1: Core Visuals (Priority: HIGH)
- [V] Animation integration
  - Parse animationClips from npcs.json
  - Build animation dictionaries in NpcSpawnerSystem
  - Load sprite textures and create clips
  - NPCs animate via existing AnimationStateSystem
- [V] Interaction prompt indicator (speech bubble icon above interactable NPCs)
- [ ] Shadow sprites under NPCs

Phase 2: Behavior Depth (Priority: MEDIUM)
- [ ] Schedules (time-based behavior changes)
  - NPCs follow daily routines (morning patrol, evening idle, etc.)
  - JSON: schedule array with time/behavior/location
- [ ] Contextual dialogue (flag-based dialogue branches)
  - Different greetings based on quest progress/flags
- [ ] Smooth facing transitions (lerp rotation instead of instant snap)

Phase 3: Advanced Systems (Priority: MEDIUM)
- [ ] Time of day system
  - Track game time (day/night cycle)
  - Spawn/despawn NPCs based on time
  - Lighting/tint changes
- [ ] Improved spawn conditions
  - Multiple spawn points per NPC (random selection)
  - Quest stage integration (when quest system exists)
  - Player level checks (when level system exists)
- [ ] NPC reactions (flee, follow, alert others)

Phase 4: Polish & Performance (Priority: LOW)
- [ ] Visual polish
  - Exclamation mark for quest givers
  - Footstep sound effects
- [ ] Performance
  - Spatial partitioning (don't update off-screen NPCs)
  - Debug overlay: show NPC behavior/state/patrol paths
  - NPC count in debug HUD
- [ ] State persistence (depends on save system)
  - Save NPC positions if they moved
  - Save NPC dialogue flags/progress
  - Respawn rules (always vs. once-only)

General TODO (Non-NPC)
- Robust tileset asset resolver (resolve relative to TMX/TSX; remove hardcoded paths)
- Draw pipeline by layers (render entities between named Tiled layers)
- Layer features (opacity/tint, offsets/parallax)
- External JSON for player animations (rows/frames/durations), remove hardcoded mapping
- Map object properties ? components (Portal, SpawnPoint, Trigger volumes)
- Interaction expansion (prompt UI polish, interaction gating/cooldowns)
- Audio polish (positional pan/rolloff via camera, music service with fades)
- More tests (dialogue branching, polygon collisions)
- Save/load of dialogue flags and simple game state
- UI polish (portraits, transitions, rich text)

Notes
- Virtual resolution matches backbuffer; Dialogue HUD renders in virtual space before scaling
- Content paths use mgcb; SFX are loaded by asset name (e.g., Sounds/Effects/typewriter)
- NPCs defined in npcs.json, spawn rules in npc_spawns.json (separate, data-driven)
- NPC animations use row-based spritesheet layout (row 0-3 = down/up/left/right)
