using Xunit;
using ANewWorld.Engine.Game;
using FluentAssertions;

namespace ANewWorld.Tests
{
    public class GameStateServiceTests
    {
        [Fact]
        public void Initial_State_Is_Playing()
        {
            // Arrange / Act
            var svc = new GameStateService();

            // Assert
            svc.Current.Should().Be(GameState.Playing);
        }

        [Fact]
        public void Set_Changes_Current_State()
        {
            // Arrange
            var svc = new GameStateService();

            // Act
            svc.Set(GameState.Dialogue);

            // Assert
            svc.Current.Should().Be(GameState.Dialogue);
        }

        [Fact]
        public void Is_Returns_True_For_Current_State()
        {
            // Arrange
            var svc = new GameStateService();
            svc.Set(GameState.Paused);

            // Act / Assert
            svc.Is(GameState.Paused).Should().BeTrue();
            svc.Is(GameState.Playing).Should().BeFalse();
        }

        [Fact]
        public void State_Can_Transition_Between_All_States()
        {
            // Arrange
            var svc = new GameStateService();

            // Act / Assert
            svc.Set(GameState.Playing);
            svc.Current.Should().Be(GameState.Playing);

            svc.Set(GameState.Dialogue);
            svc.Current.Should().Be(GameState.Dialogue);

            svc.Set(GameState.Paused);
            svc.Current.Should().Be(GameState.Paused);

            svc.Set(GameState.Cutscene);
            svc.Current.Should().Be(GameState.Cutscene);
        }
    }
}
