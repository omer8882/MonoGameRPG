# Copilot Instructions
## Project Overview
- MonoGame DesktopGL client targeting `net10.0`; `Game1` wires MonoGame services, DefaultEcs systems, and custom render/audio/dialogue stacks.
- Everything runs inside a single DefaultEcs `World`; most systems are updated manually in `Game1.Update` with explicit ordering tied to `GameStateService` (`Playing` vs `Dialogue`).
## Architecture & Systems
- Movement flow: `PlayerMovementInputSystem` writes `Velocity`, `CollisionSystem` clips movement against `CollisionGridService`, `MovementSystem` integrates `Transform`, then `FacingDirectionSystem`, `AnimationStateSystem`, and `AnimationSystem` update sprites.
- `RenderSystem` queries all `Transform`+`SpriteComponent` pairs, culls via `CameraService` bounds, sorts by `SortOffsetY`, and draws inside the world pass; HUD is rendered later without the camera matrix.
- Tilemaps are loaded once via `TmxLoader`/`TmxRenderer` with animated tile tracking and frustum culling; `TileObjectSpawner` spawns object-layer sprites + optional interactables/dialogues from TMX data.
- Audio is event-driven: gameplay code publishes `PlaySfx`/`StartLoop` on `AudioBus`, and `AudioSystem` drains the bus plus entity-tagged events each frame.
- ECS helper methods in `Engine/Extensions` use the new C# `extension` syntax (`LangVersion` is `latestmajor`); do not rewrite them as static classes unless you keep language features enabled.
- Whenever you allocate an `EntitySet` in a long-lived system, mirror existing implementations by disposing it in `Dispose()` to avoid DefaultEcs leaks.
## Content & Data
- `ContentLoader.LoadJson` resolves paths relative to `Content/`; every data file (items, NPCs, dialogues, bindings) lives under `Content/Data/**` or `Content/Settings`, so keep new JSON there or loading will throw.
- Tile assets rely on the path rewriting inside `TmxRenderer.TryResolveAssetName`; if you move maps or tilesets, update the prefix logic or the renderer cannot locate textures.
- `Content/Content.mgcb` must be rebuilt anytime you change assets; run MGCB before `dotnet build` so the runtime can load compiled content.
- `Settings/input_bindings.json` drives `InputActionService`; add new actions there and map them in systems instead of hardcoding keys.
- Keep map-specific constants (like `maps` in `Game1`) synchronized with actual TMX asset folders; the loader currently expects `.tmx` under `Content/Maps/**` and still contains an absolute fallback path.
## Interaction & Dialogue
- `InteractionSystem` scans for the closest enabled `Interactable` and emits an `InteractionStarted` entity when the `Interact` action fires; downstream systems must dispose those event entities after handling.
- `DialogueSystem` consumes the interaction events, pulls graphs from `DialogueService`, runs the typewriter loop (which pushes a `StartLoop` event to `AudioBus`), and toggles `GameState` between `Playing` and `Dialogue`.
- Dialogue graphs live in `Content/Data/NPCs/dialogues.json`; nodes can set/check `DialogueContext.Flags` via `Actions`/`Conditions`, so prefer that mechanism over bespoke state.
- The HUD layer uses `DialogueHud` in `Engine/UI`—text wrapping and choice rendering already exist, so reuse those helpers if you add new dialogue UIs.
- Debug prompts (the bobbing icon in `InteractionPromptRenderer`) assume interactables set a `SpriteComponent`; if you spawn invisible interactables, provide your own icon positioning logic.
## NPCs & Items
- NPC definitions (`Content/Data/NPCs/npcs.json`) declare sprite sheets and per-direction animation rows; `NpcAnimationBuilder` converts them to `MovementAnimationKey` clips, so match the naming (`idleDown`, `walkRight`, etc.).
- Spawn rules (`npc_spawns.json`) feed `NpcSpawnerSystem`, which enforces flag conditions against `DialogueService.Context`; when adding rules, remember spawns are deduplicated by `mapId+npcId+coords`.
- `NpcBrainSystem`/`NpcMovementSystem` expect behavior-specific components (`PatrolPath`, `WanderBehavior`) to exist before the state is toggled; keep those in sync when you edit NPC JSON.
- Items are defined in `Content/Data/Items/items.json`; `InventoryService`/`InventorySystem` sanitize stacks each frame, so new gameplay code should request items by `itemId` and respect `MaxStack` limits rather than storing raw quantities elsewhere.
## Build, Tests & Debugging
- Full rebuild + content generation from the repo root (MGCB must be available via `dotnet tool restore` triggered by the csproj):
```powershell
dotnet build ANewWorld.sln
```
- Run the headless unit suite (covers systems like movement, camera, dialogue, collisions) before submitting gameplay changes:
```powershell
dotnet test ANewWorld.Tests/ANewWorld.Tests.csproj
```
- Use `F3` (the `ToggleOverlay` action) to show `DebugOverlayService`; it reports entity counts, camera bounds, collision info, and the last culled rectangle from `TmxRenderer`.
- When chasing input-related bugs, remember `Game1.Update` already calls `_inputActions.Update()` and `EndFrame()`; avoid double-updating `InputActionService` inside gameplay code unless you mirror the existing pattern.
## Implementation Tips
- System order in `Game1.Update` matters (input → collision → movement → NPC → interaction → inventory → animation). Keep new systems in the appropriate slot or gate them with `GameState` if they should pause during dialogue.
- Keep diagnostics lightweight: reuse `DebugOverlayService` or `RenderSystem.LastVisibleCount` instead of adding new SpriteBatch calls in the main draw loop.
- For Tiled integration, prefer encoding metadata as object-layer properties (e.g., `Interactable`, `Prompt`, `DialogueId`) so `TileObjectSpawner` can auto-wire components instead of adding hardcoded factory code.
- Audio code should publish to `AudioBus` rather than loading sounds directly; `SoundService` caches `SoundEffect` instances and manages loops, so you avoid duplicate loads or unmanaged `SoundEffectInstance`s.