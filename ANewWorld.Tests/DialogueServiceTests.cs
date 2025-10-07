using Xunit;
using ANewWorld.Engine.Dialogue;
using FluentAssertions;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System;

namespace ANewWorld.Tests
{
    public class DialogueServiceTests : IDisposable
    {
        private readonly string _testRootPath;
        private const string TestDialoguesPath = "Content/Data/NPCs/dialogues.json";
        private readonly string _originalDirectory;

        public DialogueServiceTests()
        {
            // Save original directory to restore later
            _originalDirectory = Directory.GetCurrentDirectory();
            
            // Use unique directory per test instance to avoid parallel test conflicts
            _testRootPath = $"TestContent_DialogueService_{Guid.NewGuid():N}";
            var basePath = Path.GetFullPath(Path.Combine(_testRootPath));
            
            // Create test content directory structure (DialogueService expects Data/NPCs subdirectory)
            Directory.CreateDirectory(basePath);
            Directory.CreateDirectory(Path.Combine(basePath, "Content", "Data"));
            Directory.CreateDirectory(Path.Combine(basePath, "Content", "Data", "NPCs"));
            
            // Create test dialogues file
            var dialogues = new
            {
                Dialogues = new Dictionary<string, object>
                {
                    { 
                        "test", 
                        new 
                        { 
                            Id = "test",
                            Start = "start",
                            Nodes = new[] 
                            {
                                new { Id = "start", Text = "Hello" }
                            }
                        } 
                    }
                }
            };
            
            var dialoguesPath = Path.Combine(basePath, TestDialoguesPath);
            File.WriteAllText(dialoguesPath, JsonSerializer.Serialize(dialogues));

            // Set the current directory to the test root so DialogueService can find the files
            Directory.SetCurrentDirectory(basePath);
        }

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

            // Act
            var result = svc.Get("test");

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be("test");
        }

        public void Dispose()
        {
            // Restore original directory
            try
            {
                Directory.SetCurrentDirectory(_originalDirectory);
            }
            catch
            {
                // Ignore errors restoring directory
            }
            
            // Clean up test directory
            try
            {
                var basePath = Path.GetFullPath(_testRootPath);
                if (Directory.Exists(basePath))
                    Directory.Delete(basePath, recursive: true);
            }
            catch (IOException)
            {
                // Ignore cleanup errors
            }
        }
    }
}
