using Xunit;
using ANewWorld.Engine.Dialogue;
using FluentAssertions;
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
        public void Substitute_Missing_Token_Leaves_Placeholder()
        {
            // Arrange
            var svc = new DialogueService();

            // Act
            var res = svc.Substitute("Hello {name}!");

            // Assert
            res.Should().Be("Hello {name}!");
        }

        [Fact]
        public void Substitute_Empty_String_Returns_Empty()
        {
            // Arrange
            var svc = new DialogueService();

            // Act
            var res = svc.Substitute("");

            // Assert
            res.Should().BeEmpty();
        }

        [Fact]
        public void Conditions_And_Actions_Work()
        {
            // Arrange
            var svc = new DialogueService();
            var conds = new List<DialogueCondition> { new DialogueCondition { Flag = "met", Equals = false } };

            // Act / Assert
            svc.CheckConditions(conds).Should().BeTrue();
            svc.ApplyActions(new List<DialogueAction> { new DialogueAction { SetFlag = "met", Value = true } });
            svc.CheckConditions(conds).Should().BeFalse();

            // Overwrite flag
            svc.ApplyActions(new List<DialogueAction> { new DialogueAction { SetFlag = "met", Value = false } });
            svc.CheckConditions(conds).Should().BeTrue();
        }

        [Fact]
        public void CheckConditions_Null_Or_Empty_Returns_True()
        {
            // Arrange
            var svc = new DialogueService();

            // Act / Assert
            svc.CheckConditions(null).Should().BeTrue();
            svc.CheckConditions(new List<DialogueCondition>()).Should().BeTrue();
        }

        [Fact]
        public void CheckConditions_Multiple_Flags_All_Must_Match()
        {
            // Arrange
            var svc = new DialogueService();
            svc.Context.Flags["flag1"] = true;
            svc.Context.Flags["flag2"] = false;

            var conds = new List<DialogueCondition>
            {
                new DialogueCondition { Flag = "flag1", Equals = true },
                new DialogueCondition { Flag = "flag2", Equals = false }
            };

            // Act / Assert
            svc.CheckConditions(conds).Should().BeTrue();

            // One flag doesn't match
            svc.Context.Flags["flag2"] = true;
            svc.CheckConditions(conds).Should().BeFalse();
        }

        [Fact]
        public void ApplyActions_Sets_Multiple_Flags()
        {
            // Arrange
            var svc = new DialogueService();
            var actions = new List<DialogueAction>
            {
                new DialogueAction { SetFlag = "quest1", Value = true },
                new DialogueAction { SetFlag = "quest2", Value = false }
            };

            // Act
            svc.ApplyActions(actions);

            // Assert
            svc.Context.Flags["quest1"].Should().BeTrue();
            svc.Context.Flags["quest2"].Should().BeFalse();
        }

        [Fact]
        public void Get_Returns_Loaded_Graph()
        {
            // Arrange
            var svc = new DialogueService();
            var graph = new DialogueGraph
            {
                Id = "test",
                Start = "start",
                Nodes = new List<DialogueNode>
                {
                    new DialogueNode { Id = "start", Text = "Hello" }
                }
            };

            // Act - manually add to simulate loading
            var data = new DialogueData();
            data.Dialogues["test"] = graph;
            typeof(DialogueService).GetField("_graphs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .SetValue(svc, new System.Collections.Generic.Dictionary<string, DialogueGraph> { ["test"] = graph });

            // Assert
            var result = svc.Get("test");
            result.Should().NotBeNull();
            result!.Id.Should().Be("test");
        }
    }
}
