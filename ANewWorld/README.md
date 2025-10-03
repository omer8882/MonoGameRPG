# A New World (MonoGame RPG prototype)

Short description
- 2D top-down prototype built with MonoGame (DesktopGL) on .NET 8
- ECS with DefaultEcs
- Tiled maps via TiledSharp + custom TMX renderer (animated tiles, culling)
- Player movement + 4-directional animation
- Interaction + Dialogue (choices, conditions/actions, variables, typewriter)
- HUD in virtual space; Debug overlay with minimap
- Basic audio system (one-shots + loop bus)

Controls
- Move: WASD / Arrow keys
- Interact: E
- Advance dialogue / Select choice: Space / Enter (Up/Down to change selection)
- Toggle debug overlay: F3

Build & run
- Requirements: .NET 8 SDK, MonoGame 3.8, MGCB Content Pipeline
- Open ANewWorld/ANewWorld.csproj in VS or run `dotnet build` then start the app
- Ensure Content.mgcb is built; add SFX to Content/Audio and reference by asset name

Project structure (high level)
- Engine/Tilemap/Tmx: TMX loading and renderer
- Engine/Systems: ECS systems (input, movement, animation, dialogue, audio, etc.)
- Engine/UI: HUD rendering
- Engine/Audio: SFX/events/bus
- Content: mgcb-managed content (maps, textures, audio)

TODO (working list)

Done
- Animated tiles (layers + tile objects) with per-gid atlas
- Player animation (enum-based clips, 4 directions)
- Frustum culling (tiles + entities) + minimap
- Object-layer rectangles/polygons as colliders
- Interaction system (prompt + events)
- Dialogue system (choices, conditions/actions, variables, typewriter, pause via GameState)
- Dialogue HUD in virtual space (wrapped text, name box)
- Audio: one-shots + loop bus, typewriter loop
- Cleanup/perf: renderer buffer reuse; basic unit tests

Next
- Robust tileset asset resolver (resolve relative to TMX/TSX; remove hardcoded paths)
- Draw pipeline by layers (render entities between named Tiled layers)
- Layer features (opacity/tint, offsets/parallax)
- External JSON for player animations (rows/frames/durations), remove hardcoded mapping
- Map object properties ? components (Portal, SpawnPoint, Trigger volumes)
- Interaction expansion (prompt UI polish, interaction gating/cooldowns)
- NPCs (NpcBrain idle/patrol, face player on interact)
- Audio polish (positional pan/rolloff via camera, music service with fades)
- More tests (dialogue branching, polygon collisions)
- Save/load of dialogue flags and simple game state
- UI polish (portraits, transitions, rich text)

Notes
- Virtual resolution currently 600x600; Dialogue HUD renders in virtual space before scaling
- Content paths use mgcb; SFX are loaded by asset name (e.g., Sounds/Effects/typewriter)
