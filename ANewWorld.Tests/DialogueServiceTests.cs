using Xunit;
using ANewWorld.Engine.Dialogue;
using FluentAssertions;
using System.Collections;
using System.Collections.Generic;

namespace ANewWorld.Tests
{
    public class DialogueServiceTests
    {
        [Fact]
        public void Substitute_Replaces_Tokens()
        {
            // Arrange
            var svc = new DialogueService();
            svc.Context.Vars["name"] = "Hero";

            // Act
            var res = svc.Substitute("Hello {name}!");

            // Assert
            res.Should().Be("Hello Hero!");
        }

        [Fact]
        public void Substitute_Multiple_Tokens()
        {
            // Arrange
            var svc = new DialogueService();
            svc.Context.Vars["name"] = "Hero";
            svc.Context.Vars["place"] = "Village";

            // Act
            var res = svc.Substitute("{name} from {place}");

            // Assert
            res.Should().Be("Hero from Village");
        }

        [Fact]
        public void Conditions_And_Actions_Work()
        {
            // Arrange
            var svc = new DialogueService();
            List<DialogueCondition> conds = [new DialogueCondition { Flag = "met", Equals = false }];

            // Act / Assert
            svc.CheckConditions(conds).Should().BeTrue();
            svc.ApplyActions([new DialogueAction { SetFlag = "met", Value = true }]);
            svc.CheckConditions(conds).Should().BeFalse();

            // Overwrite flag
            svc.ApplyActions([new DialogueAction { SetFlag = "met", Value = false }]);
            svc.CheckConditions(conds).Should().BeTrue();
        }
    }
}
