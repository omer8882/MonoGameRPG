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
using ANewWorld.Engine.Debug;

namespace ANewWorld
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private World? _ecsWorld;
        private InputSystem? _inputSystem;
        private MovementSystem? _movementSystem;
        private RenderSystem? _renderSystem;
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

            _inputSystem = new InputSystem(_ecsWorld, _inputActions);
            _movementSystem = new MovementSystem(_ecsWorld);
            _renderSystem = new RenderSystem(_ecsWorld, _spriteBatch);

            // create player entity
            var e = _ecsWorld.CreateEntity();
            e.Set(new Transform { Position = new Vector2(_virtualWidth / 2f, _virtualHeight / 2f), Rotation = 0f, Scale = Vector2.One });
            e.Set(new Velocity { Value = Vector2.Zero });
            e.Set(new SpriteComponent {
                Texture = _playerTexture,
                SourceRect = _playerSourceRect,
                Color = Color.White,
                Origin = new Vector2(_playerFrameWidth / 2f, _playerFrameHeight / 2f)
            });

            _screenWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            _screenHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;

            // camera: world size equals virtual size for now
            _camera = new CameraService(_virtualWidth, _virtualHeight, _virtualWidth, _virtualHeight, 2f);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _inputSystem?.Update(dt);
            _movementSystem?.Update(dt);

            // update camera to follow player
            var set = _ecsWorld?.GetEntities().With<Transform>().With<Velocity>().AsSet();
            if (set != null)
            {
                foreach (var entity in set.GetEntities())
                {
                    var t = entity.Get<Transform>();
                    _camera?.Update(t.Position);
                    break; // follow first entity
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_virtualTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera?.GetViewMatrix());
            _renderSystem?.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            // integer scaling to window size
            _screenWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            _screenHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;

            int scale = System.Math.Min(_screenWidth / _virtualWidth, _screenHeight / _virtualHeight);
            if (scale < 1) scale = 1;

            int scaledWidth = _virtualWidth * scale;
            int scaledHeight = _virtualHeight * scale;
            int offsetX = (_screenWidth - scaledWidth) / 2;
            int offsetY = (_screenHeight - scaledHeight) / 2;

            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            _spriteBatch.Draw(_virtualTarget, new Rectangle(offsetX, offsetY, scaledWidth, scaledHeight), Color.White);
            _spriteBatch.End();

            // Debug overlay drawn in window space (after scaling)
            if (_inputActions!.OverlayActive && _debugOverlay != null && _ecsWorld != null)
            {
                float fps = 1f / (float)gameTime.ElapsedGameTime.TotalSeconds;
                _debugOverlay.Draw(_spriteBatch, _ecsWorld, fps);
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
