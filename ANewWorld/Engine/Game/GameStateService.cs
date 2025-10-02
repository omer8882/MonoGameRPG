namespace ANewWorld.Engine.Game
{
    public enum GameState
    {
        Playing,
        Dialogue,
        Paused,
        Cutscene
    }

    public sealed class GameStateService
    {
        public GameState Current { get; private set; } = GameState.Playing;
        public void Set(GameState state) => Current = state;
        public bool Is(GameState s) => Current == s;
    }
}
