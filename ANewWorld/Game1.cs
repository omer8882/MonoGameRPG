using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Extensions.DependencyInjection;
using DefaultEcs;
using ANewWorld.Engine.Ecs;
using ANewWorld.Engine.Systems;
using ANewWorld.Engine.Components;
using ANewWorld.Engine.Rendering;
using ANewWorld.Engine.Input;
using ANewWorld.Engine.Game;
using ANewWorld.Engine.Debug;
using ANewWorld.Engine.Tilemap;
using ANewWorld.Engine.Tilemap.Tmx;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System.Linq;
using TiledSharp;
using ANewWorld.Engine.Dialogue;
using ANewWorld.Engine.UI;
using ANewWorld.Engine.Audio;
using ANewWorld.Engine.Npc;

namespace ANewWorld
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private World? _ecsWorld;
        private PlayerMovementInputSystem? _inputSystem;
        private MovementSystem? _movementSystem;
        private RenderSystem? _renderSystem;
        private CollisionSystem? _collisionSystem;
        private RenderTarget2D? _virtualTarget;
        private int _virtualWidth = 600;
        private int _virtualHeight = 600;

        private Texture2D? _playerTexture;
        private Rectangle _playerSourceRect;
        private int _playerFrameWidth = 64;
        private int _playerFrameHeight = 64;
        private int _screenWidth;
        private int _screenHeight;

        private CameraService? _camera;
        private InputActionService? _inputActions;
        private SpriteFont? _debugFont;
        private DebugOverlayService? _debugOverlay;
        private TmxRenderer? _tmxRenderer;
        private CollisionGridService? _collisionGrid;
        private Dictionary<string, string> maps = new()
        {
            { "beginning_fields", "Maps/The Fan-tasy Tileset (Free)/Tiled/Tilemaps/Beginning Fields" },
            { "passway", "Maps/The Fan-tasy Tileset (Free)/Tiled/Tilemaps/Passway" }
        };
        private string _collisionLayerName = "Collisions";

        private ObjectTileAnimationSystem? _objectTileAnimSystem;
        private AnimationSystem? _animationSystem;
        private FacingDirectionSystem? _facingSystem;
        private AnimationStateSystem? _animStateSystem;
        private InteractionSystem? _interactionSystem;
        private DialogueService? _dialogueService;
        private DialogueSystem? _dialogueSystem;
        private DialogueHud? _dialogueHud;
        private AudioSystem? _audioSystem;
        private SoundService? _soundService;
        private AudioBus? _audioBus;

        // NPC systems
        private NpcService? _npcService;
        private NpcSpawnerSystem? _npcSpawner;
        private NpcMovementSystem? _npcMovementSystem;
        private NpcBrainSystem? _npcBrainSystem;
        private NpcInteractionSystem? _npcInteractionSystem;

        // UI
        private InteractionPromptRenderer? _interactionPrompt;

        private GameStateService _gameState = new GameStateService();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            TargetElapsedTime = System.TimeSpan.FromSeconds(1.0 / 60.0);
            IsFixedTimeStep = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // create virtual render target
            _virtualTarget = new RenderTarget2D(GraphicsDevice, _virtualWidth, _virtualHeight);

            // Load player sprite sheet
            _playerTexture = Content.Load<Texture2D>("Character/DemoPlayer/naked_player");
            _playerSourceRect = new Rectangle(0, 0, _playerFrameWidth, _playerFrameHeight);

            // load debug font (add default if not present)
            _debugFont = Content.Load<SpriteFont>("Fonts/default_font");
            _debugOverlay = new DebugOverlayService(_debugFont);

            // ECS
            _ecsWorld = new World();

            // Input actions
            _inputActions = new InputActionService("Content/input_bindings.json");

            _inputSystem = new PlayerMovementInputSystem(_ecsWorld, _inputActions);
            _movementSystem = new MovementSystem(_ecsWorld);
            _renderSystem = new RenderSystem(_ecsWorld, _spriteBatch);
            _animationSystem = new AnimationSystem(_ecsWorld!);
            _facingSystem = new FacingDirectionSystem(_ecsWorld!);
            _animStateSystem = new AnimationStateSystem(_ecsWorld!);
            _interactionSystem = new InteractionSystem(_ecsWorld!, _inputActions);

            // Load tilemap via custom TMX loader and build atlas
            var currentMap = "passway";
            var tmxPath = System.IO.Path.Combine("C:\\Users\\omer8\\Omer\\Dev\\Gaming\\A New World\\ANewWorld\\ANewWorld\\Content", maps[currentMap] + ".tmx");
            var tmxMap = TmxLoader.LoadFromFile(tmxPath);
            _tmxRenderer = new TmxRenderer(GraphicsDevice, _spriteBatch, tmxMap);
            _tmxRenderer.BuildAtlas(Content);

            _objectTileAnimSystem = new ObjectTileAnimationSystem(_ecsWorld!, _tmxRenderer);

            // Spawn objects from TiledSharp map: build dictionaries from renderer
            var objectSpawner = new TileObjectSpawner(_ecsWorld!, new Dictionary<int, Texture2D>(_tmxRenderer.GidToTexture), new Dictionary<int, Rectangle>(_tmxRenderer.GidToSourceRect), _tmxRenderer);
            objectSpawner.SpawnObjects(tmxMap);

            // Camera based on map size
            _camera = new CameraService(_virtualWidth, _virtualHeight, tmxMap.Width * tmxMap.TileWidth, tmxMap.Height * tmxMap.TileHeight, 2f);

            // Collision grid from TMX layer
            _collisionGrid = new CollisionGridService(tmxMap, _collisionLayerName);
            _collisionSystem = _collisionGrid is not null && _ecsWorld is not null ? new CollisionSystem(_ecsWorld, _collisionGrid!) : null;

            // create player entity via factory
            PlayerFactory.CreatePlayer(
                _ecsWorld!,
                _playerTexture!,
                _playerSourceRect,
                new Vector2(_virtualWidth / 2f, _virtualHeight / 2f)
            );

            _renderSystem.Camera = _camera;

            // Audio system & sound service
            _soundService = new SoundService(Content);
            _audioBus = new AudioBus();
            _audioSystem = new AudioSystem(_ecsWorld!, _soundService, _audioBus);

            // Dialogue service and system
            _dialogueService = new DialogueService();
            _dialogueService.Load("Content/dialogues.json");
            _dialogueService.Context.Vars["playerName"] = "Omer"; // test substitution
            _dialogueSystem = new DialogueSystem(_ecsWorld!, _dialogueService, _inputActions, _audioBus);
            _dialogueHud = new DialogueHud(_debugFont);

            // Interaction prompt renderer
            _interactionPrompt = new InteractionPromptRenderer();
            _interactionPrompt.LoadContent(Content);

            // NPC system
            _npcService = new NpcService();
            _npcService.LoadDefinitions("Content/npcs.json");
            _npcService.LoadSpawnRules("Content/npc_spawns.json");
            _npcSpawner = new NpcSpawnerSystem(_ecsWorld!, _npcService, _dialogueService, Content);
            _npcMovementSystem = new NpcMovementSystem(_ecsWorld!);
            _npcBrainSystem = new NpcBrainSystem(_ecsWorld!);
            _npcInteractionSystem = new NpcInteractionSystem(_ecsWorld!);
            _npcInteractionSystem.SetDialogueSystem(_dialogueSystem);

            // Spawn NPCs for current map
            _npcSpawner.SpawnNpcsForMap(currentMap);

            // initial state
            _gameState.Set(GameState.Playing);

            _screenWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            _screenHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update input state once per frame
            _inputActions?.Update();

            if (_gameState.Is(GameState.Playing))
            {
                _inputSystem?.Update(dt);           // 1. Player input
                _collisionSystem?.Update(dt);       // 2. Collision checks
                _movementSystem?.Update(dt);        // 3. Player movement
                _npcMovementSystem?.Update(dt);     // 4. NPC movement (sets velocity)
                _facingSystem?.Update(dt);          // 5. Updates facing from velocity (automatic)
                _interactionSystem?.Update(dt);     // 6. Creates InteractionStarted events
                _npcInteractionSystem?.Update(dt);  // 7. Processes events, overwrites NPC facing (manual)
                _animStateSystem?.Update(dt);       // 8. Updates animation keys based on facing/velocity
                _animationSystem?.Update(dt);       // 9. Animates frames
            }

            _dialogueSystem?.Update(dt);          // Starts dialogue and disposes InteractionStarted events
            _npcBrainSystem?.Update(dt);          // NPC AI decision-making


            // Switch state based on dialogue activity
            if (_dialogueSystem?.IsActive == true && !_gameState.Is(GameState.Dialogue))
                _gameState.Set(GameState.Dialogue);
            else if (_dialogueSystem?.IsActive == false && _gameState.Is(GameState.Dialogue))
                _gameState.Set(GameState.Playing);

            // Map/objects continue animating regardless of state
            _tmxRenderer?.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            _objectTileAnimSystem?.Update(dt);

            // Update interaction prompt bobbing animation
            _interactionPrompt?.Update(dt);

            // Camera follows only in Playing
            if (_gameState.Is(GameState.Playing))
            {
                var set = _ecsWorld?.GetEntities().With<Transform>().With<Velocity>().AsSet();
                if (set != null)
                {
                    foreach (var entity in set.GetEntities())
                    {
                        var t = entity.Get<Transform>();
                        _camera?.Update(t.Position);
                        break;
                    }
                }
            }
            // Clamp camera always
            if (_camera is not null && _tmxRenderer is not null)
            {
                float halfW = _camera.VirtualWidth / 2f / _camera.Zoom;
                float halfH = _camera.VirtualHeight / 2f / _camera.Zoom;
                float worldW = _tmxRenderer.Map.Width * _tmxRenderer.Map.TileWidth;
                float worldH = _tmxRenderer.Map.Height * _tmxRenderer.Map.TileHeight;
                float minX = halfW;
                float maxX = worldW - halfW - 1f;
                float minY = halfH;
                float maxY = worldH - halfH - 1f;
                var pos = _camera.Position;
                pos.X = MathHelper.Clamp(pos.X, minX, maxX);
                pos.Y = MathHelper.Clamp(pos.Y, minY, maxY);
                _camera.Update(pos);
            }

            // Update audio system
            _audioSystem?.Update(dt);

            base.Update(gameTime);
            _inputActions?.EndFrame();
        }

        protected override void Draw(GameTime gameTime)
        {
            // Ensure virtual matches backbuffer
            int bbW = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int bbH = GraphicsDevice.PresentationParameters.BackBufferHeight;
            if (bbW != _virtualWidth || bbH != _virtualHeight)
            {
                _virtualWidth = bbW;
                _virtualHeight = bbH;
                _virtualTarget?.Dispose();
                _virtualTarget = new RenderTarget2D(GraphicsDevice, _virtualWidth, _virtualHeight);
                _camera?.UpdateViewport(_virtualWidth, _virtualHeight);
            }

            GraphicsDevice.SetRenderTarget(_virtualTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // World pass (camera)
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera?.GetViewMatrix());
            if (_tmxRenderer != null && _camera != null)
                _tmxRenderer.Draw(_spriteBatch, _camera);
            else
                _tmxRenderer?.Draw(_spriteBatch, _camera?.GetViewMatrix());
            _renderSystem?.Draw((float)gameTime.ElapsedGameTime.TotalSeconds);

            // Draw interaction prompt in world space (with camera transform)
            if (_gameState.Is(GameState.Playing))
                _interactionPrompt?.Draw(_spriteBatch, _interactionSystem);
            
            _spriteBatch.End();

            // HUD in virtual space (no transform)
            if (_dialogueSystem is not null && _dialogueHud is not null)
            {
                _dialogueHud.VirtualWidth = _virtualWidth;
                _dialogueHud.VirtualHeight = _virtualHeight;
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
                _dialogueHud.Draw(_spriteBatch, _dialogueSystem);
                _spriteBatch.End();
            }

            GraphicsDevice.SetRenderTarget(null);

            // integer scaling to window size (now 1:1 since virtual==backbuffer)
            _screenWidth = bbW;
            _screenHeight = bbH;

            int scale = 1;
            int scaledWidth = _virtualWidth * scale;
            int scaledHeight = _virtualHeight * scale;
            int offsetX = 0;
            int offsetY = 0;

            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            _spriteBatch.Draw(_virtualTarget, new Rectangle(offsetX, offsetY, scaledWidth, scaledHeight), Color.White);
            _spriteBatch.End();

            // Debug overlay in window space
            if (_inputActions!.OverlayActive && _debugOverlay != null && _ecsWorld != null)
            {
                float fps = 1f / (float)gameTime.ElapsedGameTime.TotalSeconds;
                _debugOverlay.Draw(
                    _spriteBatch,
                    _ecsWorld,
                    fps,
                    _collisionGrid!,
                    _tmxRenderer,
                    _camera?.Position ?? Vector2.Zero,
                    _camera?.Zoom ?? 1f,
                    _renderSystem?.LastVisibleCount ?? 0,
                    _renderSystem?.LastCulledCount ?? 0,
                    _interactionSystem,
                    _virtualWidth,
                    _virtualHeight);
            }

            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            _ecsWorld?.Dispose();
            base.UnloadContent();
        }
    }
}
