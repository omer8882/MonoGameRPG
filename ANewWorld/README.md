# A New World (MonoGame RPG prototype)


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
- NPC core system (data-driven spawning, behaviors: idle/patrol/wander, face player on interact, restore behavior)
- NPC tests (movement, brain, interaction, service - 28 tests)
- NPC animation integration (parse clips from npcs.json, build dictionaries, AnimationStateSystem integration)
- Interaction prompt indicator (bobbing speech bubble above interactable NPCs)

### Revised TODO (High-level roadmap)

**Inventory & World Items**
- **Goal:** Full item system with world-spawned items, visible pickups, and an inventory UI.
- **Design:** item definitions in `Content/Data/Items/*.json`; runtime `ItemService` + `ItemEntity` prefab that spawns a visible `SpriteComponent` + `Interactable`/pickup component.
- **Feature list:** item stacking, max stack, pick-up range, throw/drop from inventory, simple physics for dropped items (slide/settle).

**Quests & NPC Storylines**
- **Goal:** NPCs drive quests and story arcs that evolve the world and spawn/map content as the player progresses.
- **Design:** a `QuestService` and `NpcStoryService` hold quest states, triggers, and story stages. NPCs have `QuestGiver`/`Actor` components linking to quest graphs defined in `Content/Data/Quests/*.json`.
- **Feature list:** multi-step quests, branching choices (flag/condition based), NPC state changes (spawn/despawn, behavior swaps), and in-world consequences (open doors, spawn enemies/items).

**Progression & Level Staging (Map States)**
- **Goal:** Reuse maps while changing map content as the player advances (new objects, enemies, NPC states, blocked/unblocked areas).
- **Design:** map staging via `MapState` definitions. Each map has named layers or object-groups gated by story flags. The `MapStateService` applies diffs (spawn/despawn/modify entities) when story stage changes.
- **Feature list:** persistent map flags, staged spawn rules, UI indicators for unlocked areas, and deterministic transitions.

**Cutscenes**
- **Goal:** Scripted sequences (camera control, dialog, NPC movement/animation, input lock) usable for story beats.
- **Design:** lightweight cutscene timeline system: tracks actions (camera pan/zoom, entity actions, dialog lines, delays) defined in `Content/Data/Cutscenes/*.json` or embedded in quest definitions. A `CutscenePlayer` runs timelines, locking `GameState` and the input system, with the option to skip.
- **Feature list:** camera control, actor choreography, anim events, sound cues, skippable playback, and hooks to modify `MapState`/quest flags when finished.

**Content & Data Schemas**
- **Goal:** Clear, testable JSON schemas for items, NPCs, quests, map states, and cutscenes kept under `Content/Data/`.
- **Design:** add example schema files and sample content, e.g., `items.schema.json`, `quests.schema.json`, and `cutscene.schema.json` to help content creation and validation.

**Implementation Steps (short-term roadmap)**
- **Phase A (MVP):**
  - Implement `ItemService` + pickup/spawn entities and basic inventory UI.
  - Add one sample quest flow (give item → fetch → return) to validate quest wiring.
  - Add a minimal cutscene type: camera pan + dialogue.
- **Phase B (Feature complete):**
  - Expand quest system for branching and NPC-driven stages.
  - Implement `MapStateService` to apply staged changes.
  - Implement dropped-item physics and item visuals.
- **Phase C (Polish):**
  - Add content tooling (schemas, examples), unit tests, save/load for quests/map states, and debug overlays for story state.

**Tests & Validation**
- **Goal:** Add headless tests for `ItemService`, `QuestService`, `MapStateService`, and `CutscenePlayer` to prevent regressions.
- **Design:** Extend `ANewWorld.Tests` with data-driven tests that load sample JSON and validate state transitions.

**Next Steps**
- Draft concrete JSON schemas for `items`, `quests`, `map_states`, and `cutscenes`.
- Prototype `ItemService` + a visible pickup in a small test scene.
- Prototype a one-off cutscene example and a sample quest to iterate on tooling and data layout.

Notes
- Virtual resolution matches backbuffer; Dialogue HUD renders in virtual space before scaling.
- Content paths use mgcb; SFX are loaded by asset name (e.g., Sounds/Effects/typewriter).
- Keep new content files under `Content/Data/**` so `ContentLoader.LoadJson` resolves them correctly.

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
