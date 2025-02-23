using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyDrawing.Command;
using System;

namespace MyDrawing.Tests
{
    [TestClass]
    public class CommandManagerTests
    {
        private CommandManager _commandManager;
        private MockCommand _mockCommand;

        [TestInitialize]
        public void SetUp()
        {
            _commandManager = new CommandManager();
            _mockCommand = new MockCommand();
        }

        [TestMethod]
        public void Execute_ShouldExecuteCommandAndPushToUndoStack()
        {
            _commandManager.Execute(_mockCommand);

            Assert.IsTrue(_mockCommand.ExecuteCalled);
            Assert.IsTrue(_commandManager.CanUndo);
            Assert.IsFalse(_commandManager.CanRedo);
        }

        [TestMethod]
        public void Undo_ShouldUndoCommandAndPushToRedoStack()
        {
            _commandManager.Execute(_mockCommand);
            _commandManager.Undo();

            Assert.IsTrue(_mockCommand.UndoCalled);
            Assert.IsFalse(_commandManager.CanUndo);
            Assert.IsTrue(_commandManager.CanRedo);
        }

        [TestMethod]
        public void Redo_ShouldRedoCommandAndPushToUndoStack()
        {
            _commandManager.Execute(_mockCommand);
            _commandManager.Undo();
            _commandManager.Redo();

            Assert.IsTrue(_mockCommand.ExecuteCalled);
            Assert.IsTrue(_commandManager.CanUndo);
            Assert.IsFalse(_commandManager.CanRedo);
        }

        [TestMethod]
        public void Execute_ShouldClearRedoStack()
        {
            _commandManager.Execute(_mockCommand);
            _commandManager.Undo();
            _commandManager.Execute(new MockCommand());

            Assert.IsFalse(_commandManager.CanRedo);
        }

        [TestMethod]
        public void Undo_ShouldNotThrowWhenNoCommandsExecuted()
        {
            _commandManager.Undo();
            Assert.IsFalse(_commandManager.CanUndo);
            Assert.IsFalse(_commandManager.CanRedo);
        }

        [TestMethod]
        public void Redo_ShouldNotThrowWhenNoCommandsExecuted()
        {
            _commandManager.Redo();
            Assert.IsFalse(_commandManager.CanUndo);
            Assert.IsFalse(_commandManager.CanRedo);
        }

        private class MockCommand : ICommand
        {
            public bool ExecuteCalled { get; private set; }
            public bool UndoCalled { get; private set; }

            public void Execute()
            {
                ExecuteCalled = true;
            }

            public void Undo()
            {
                UndoCalled = true;
            }
        }
    }
}
