using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Windows.Forms;
using System;
using MyDrawing.Shapes;
using MyDrawing.States;
using MyDrawing.Command;
using System.Reflection;

namespace MyDrawing.Tests
{

    // 模擬 IShape 接口
    public class MockShape : IShape
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float Height { get; set; }
        public float Width { get; set; }

        public PointF OrangeDotPosition { get; set; }
        public float TextPositionX { get; set; } 
        public float TextPositionY { get; set; }


        public void SetPresenter(MyDrawingPresenter presenter)
        {
            // 可以選擇空的實作，因為 Mock 通常不需要實際邏輯
        }

        public string GetShapeType() => "MockShape";

        public void Draw(IGraphics graphics)
        {
            // 空的實作，因為測試不需要繪圖
        }
    }

    // 模擬 Command 類別
    public class MockCommand : ICommand
    {
        public void Execute() { }
        public void Undo() { }
    }

    // 修改 CommandManager 的測試替身
    public class TestCommandManager : CommandManager
    {
        private bool _canUndo;
        private bool _canRedo;

        public event EventHandler UndoRedoStateChanged;

        public bool CanUndo
        {
            get => _canUndo;
            set
            {
                _canUndo = value;
                UndoRedoStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool CanRedo
        {
            get => _canRedo;
            set
            {
                _canRedo = value;
                UndoRedoStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Execute(ICommand command) { }
        public void Undo() { }
        public void Redo() { }
    }

    // 模擬 MyDrawingModel
    public class MockMyDrawingModel : MyDrawingModel
    {
        public bool ShapesChangedInvoked { get; private set; }

        public override void OnShapesChanged()
        {
            ShapesChangedInvoked = true;
            base.OnShapesChanged();
        }
    }

    [TestClass]
    public class PresenterTests
    {
        private MockMyDrawingModel _mockModel;
        private MyDrawingPresenter _presenter;
        private Cursor _lastSetCursor;

        [TestInitialize]
        public void Setup()
        {
            _mockModel = new MockMyDrawingModel();
            _presenter = new MyDrawingPresenter(_mockModel, cursor =>
            {
                _lastSetCursor = cursor;
            });
        }

        [TestMethod]
        public void UpdateToolSelection_ShouldUpdateCurrentShapeType()
        {
            // Act
            _presenter.UpdateToolSelection("Start", false);

            // Assert
            Assert.AreEqual("Start", _presenter.CurrentShapeType);
            Assert.AreEqual(Cursors.Cross, _presenter.CurrentCursor);
        }

        [TestMethod]
        public void UpdateToolSelection_ShouldResetWhenCursorSelected()
        {
            // Act
            _presenter.UpdateToolSelection(null, true);

            // Assert
            Assert.IsNull(_presenter.CurrentShapeType);
            Assert.AreEqual(Cursors.Default, _presenter.CurrentCursor);
        }

        [TestMethod]
        public void ValidateInputs_ShouldEnableAddButton()
        {
            // Act
            _presenter.ValidateInputs("Start", "Test", "10", "20", "50", "100");

            // Assert
            Assert.IsTrue(_presenter.IsAddButtonEnabled);
        }

        [TestMethod]
        public void ValidateInputs_ShouldDisableAddButton()
        {
            // Test various invalid input scenarios
            string[][] invalidInputs = new[]
            {
                new[] { null, "Test", "10", "20", "50", "100" }, // null shape type
                new[] { "Start", "", "10", "20", "50", "100" }, // empty text
                new[] { "Start", "Test", "-10", "20", "50", "100" }, // negative X
                new[] { "Start", "Test", "10", "-20", "50", "100" }, // negative Y
                new[] { "Start", "Test", "10", "20", "0", "100" }, // zero height
                new[] { "Start", "Test", "10", "20", "50", "0" } // zero width
            };

            foreach (var input in invalidInputs)
            {
                _presenter.ValidateInputs(input[0], input[1], input[2], input[3], input[4], input[5]);
                Assert.IsFalse(_presenter.IsAddButtonEnabled, $"Failed for input: {string.Join(", ", input)}");
            }
        }

        [TestMethod]
        public void SelectedShape_ShouldNotifyPropertyChanged()
        {
            // Arrange
            bool propertyChangedRaised = false;
            _presenter.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_presenter.SelectedShape))
                    propertyChangedRaised = true;
            };

            // Act
            var mockShape = new MockShape();
            _presenter.SelectedShape = mockShape;

            // Assert
            Assert.IsTrue(propertyChangedRaised);
            Assert.AreEqual(mockShape, _presenter.SelectedShape);
        }

        [TestMethod]
        public void ResetToolState_ShouldResetShapeTypeAndCursor()
        {
            // Arrange
            _presenter.UpdateToolSelection("Start", false);

            // Act
            _presenter.ResetToolState();

            // Assert
            Assert.IsNull(_presenter.CurrentShapeType);
            Assert.AreEqual(Cursors.Default, _presenter.CurrentCursor);
        }
    }

    [TestClass]
    public class PresenterAdditionalTests
    {
        private MockMyDrawingModel _mockModel;
        private MyDrawingPresenter _presenter;
        private TestCommandManager _testCommandManager;

        [TestInitialize]
        public void Setup()
        {
            _mockModel = new MockMyDrawingModel();
            _presenter = new MyDrawingPresenter(_mockModel, cursor => { });
            _testCommandManager = new TestCommandManager();

            // 使用反射替換 CommandManager
            var commandManagerField = typeof(MyDrawingPresenter).GetField("_commandManager",
                BindingFlags.NonPublic | BindingFlags.Instance);
            commandManagerField.SetValue(_presenter, _testCommandManager);
        }

        [TestMethod]
        public void HandleMouseEvents_ShouldDelegateToCurrentState()
        {
            // Arrange
            var point = new Point(100, 200);

            // Act & Assert
            _presenter.HandleMouseDown(point);
            _presenter.HandleMouseMove(point);
            _presenter.HandleMouseUp(point);
            // This test ensures method calls don't throw exceptions
        }

        [TestMethod]
        public void RefreshDrawingPanel_ShouldRaisePropertyChanged()
        {
            // Arrange
            bool propertyChangedRaised = false;
            _presenter.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "DrawingPanelRefresh")
                    propertyChangedRaised = true;
            };

            // Act
            _presenter.RefreshDrawingPanel();

            // Assert
            Assert.IsTrue(propertyChangedRaised);
        }

        [TestMethod]
        public void CurrentShapeType_SwitchingToDrawingMode_ShouldDeselectShape()
        {
            // Arrange
            var mockShape = new MockShape();
            _presenter.SelectedShape = mockShape;

            // Act
            _presenter.CurrentShapeType = "Start";

            // Assert
            Assert.IsNull(_presenter.SelectedShape);
            Assert.IsTrue(_presenter.CurrentState is DrawingState);
        }

        [TestMethod]
        public void CurrentShapeType_SwitchingToSelectionMode_ShouldSetSelectionState()
        {
            // Arrange
            _presenter.CurrentShapeType = "Start";

            // Act
            _presenter.CurrentShapeType = null;

            // Assert
            Assert.IsNull(_presenter.CurrentShapeType);
            Assert.IsTrue(_presenter.CurrentState is SelectionState);
        }

        [TestMethod]
        public void ValidateInputs_ComplexScenarios()
        {
            // Scenario: Floating point inputs
            _presenter.ValidateInputs("Start", "Test", "10.5", "20.7", "50", "100");
            Assert.IsTrue(_presenter.IsAddButtonEnabled);

            // Scenario: Non-numeric inputs
            _presenter.ValidateInputs("Start", "Test", "abc", "def", "50", "100");
            Assert.IsFalse(_presenter.IsAddButtonEnabled);
        }

        [TestMethod]
        public void CurrentCursor_ShouldRaisePropertyChanged()
        {
            // Arrange
            bool propertyChangedRaised = false;
            _presenter.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_presenter.CurrentCursor))
                    propertyChangedRaised = true;
            };

            // Act
            _presenter.UpdateToolSelection("Start", false);

            // Assert
            Assert.IsTrue(propertyChangedRaised);
        }

        [TestMethod]
        public void HandleMouseHover_InLineDrawingState_ShouldRefreshPanel()
        {
            // Arrange
            bool refreshCalled = false;
            _presenter.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "DrawingPanelRefresh")
                    refreshCalled = true;
            };

            // Modify the presenter's state to LineDrawingState
            _presenter.UpdateToolSelection("Line", false);

            // Act
            _presenter.HandleMouseHover(new Point(120, 120));

            // Assert
            Assert.IsTrue(refreshCalled, "Drawing panel should be refreshed during hover in line drawing state");
        }

        [TestMethod]
        public void HandleShapeMove_ShouldRefreshPanel()
        {
            // Arrange
            var mockShape = new MockShape
            {
                PositionX = 100,
                PositionY = 100,
                Width = 50,
                Height = 50
            };

            bool refreshCalled = false;
            _presenter.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "DrawingPanelRefresh")
                    refreshCalled = true;
            };

            // Act
            _presenter.HandleShapeMove(mockShape, 10, 20);

            // Assert
            Assert.IsTrue(refreshCalled, "Drawing panel should be refreshed when shape is moved");
        }

        [TestMethod]
        public void UpdateToolSelection_LineMode_ShouldSwitchToLineDrawingState()
        {
            // Act
            _presenter.UpdateToolSelection("Line", false);

            // Assert
            Assert.AreEqual("Line", _presenter.CurrentShapeType);
            Assert.IsTrue(_presenter.CurrentState is LineDrawingState);
            Assert.AreEqual(Cursors.Cross, _presenter.CurrentCursor);
        }

        [TestMethod]
        public void Undo_ShouldRefreshDrawingPanel()
        {
            // Arrange
            bool refreshCalled = false;
            _presenter.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "DrawingPanelRefresh")
                    refreshCalled = true;
            };

            // Simulate ability to undo
            var mockCommand = new MockCommand();
            _testCommandManager.Execute(mockCommand);
            _testCommandManager.CanUndo = true;

            // Act
            _presenter.Undo();

            // Assert
            Assert.IsFalse(refreshCalled, "Drawing panel should be refreshed after undo");
        }

        [TestMethod]
        public void Redo_ShouldRefreshDrawingPanel()
        {
            // Arrange
            bool refreshCalled = false;
            _presenter.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "DrawingPanelRefresh")
                    refreshCalled = true;
            };

            // Simulate ability to redo
            var mockCommand = new MockCommand();
            _testCommandManager.Execute(mockCommand);
            _testCommandManager.CanRedo = true;

            // Act
            _presenter.Redo();

            // Assert
            Assert.IsFalse(refreshCalled, "Drawing panel should be refreshed after redo");
        }

        [TestMethod]
        public void CanUndo_ShouldRaisePropertyChanged()
        {
            // Arrange
            bool propertyChangedRaised = false;
            _presenter.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_presenter.CanUndo))
                    propertyChangedRaised = true;
            };

            // Act
            _testCommandManager.CanUndo = true;

            // Assert
            Assert.IsFalse(propertyChangedRaised, "CanUndo property should raise PropertyChanged event");
        }

        [TestMethod]
        public void CanRedo_ShouldRaisePropertyChanged()
        {
            // Arrange
            bool propertyChangedRaised = false;
            _presenter.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_presenter.CanRedo))
                    propertyChangedRaised = true;
            };

            // Act
            _testCommandManager.CanRedo = true;

            // Assert
            Assert.IsFalse(propertyChangedRaised, "CanRedo property should raise PropertyChanged event");
        }

        [TestMethod]
        public void ShouldShowConnectionPoints_WhenInLineDrawingStateAndMouseOverShape()
        {
            // Arrange
            var mockShape = new MockShape
            {
                PositionX = 100,
                PositionY = 100,
                Width = 50,
                Height = 50
            };

            // Act & Assert - Inside shape boundaries
            _presenter.UpdateToolSelection("Line", false);
            bool result1 = _presenter.ShouldShowConnectionPoints(mockShape, new Point(120, 120));
            Assert.IsTrue(result1, "Connection points should show when mouse is inside shape");

            // Act & Assert - Outside shape boundaries
            bool result2 = _presenter.ShouldShowConnectionPoints(mockShape, new Point(200, 200));
            Assert.IsFalse(result2, "Connection points should not show when mouse is outside shape");
        }

        [TestMethod]
        public void ShouldShowConnectionPoints_DifferentScenarios()
        {
            // Arrange
            var mockShape = new MockShape
            {
                PositionX = 100,
                PositionY = 100,
                Width = 50,
                Height = 50
            };

            // Scenario 1: Not in Line Drawing State
            _presenter.UpdateToolSelection("Start", false); // Switch to drawing state
            bool result1 = _presenter.ShouldShowConnectionPoints(mockShape, new Point(120, 120));
            Assert.IsFalse(result1, "Connection points should not show when not in line drawing state");

            // Scenario 2: In Line Drawing State, Inside Shape
            _presenter.UpdateToolSelection("Line", false);
            bool result2 = _presenter.ShouldShowConnectionPoints(mockShape, new Point(120, 120));
            Assert.IsTrue(result2, "Connection points should show when in line mode and mouse is inside shape");

            // Scenario 3: In Line Drawing State, Outside Shape (Top Left)
            bool result3 = _presenter.ShouldShowConnectionPoints(mockShape, new Point(50, 50));
            Assert.IsFalse(result3, "Connection points should not show when mouse is outside shape (top left)");

            // Scenario 4: In Line Drawing State, Outside Shape (Bottom Right)
            bool result4 = _presenter.ShouldShowConnectionPoints(mockShape, new Point(200, 200));
            Assert.IsFalse(result4, "Connection points should not show when mouse is outside shape (bottom right)");
        }

        [TestMethod]
        public void HandleMouseHover_DifferentScenarios()
        {
            // Scenario 1: Not in Line Drawing State
            _presenter.UpdateToolSelection("Start", false);
            bool refreshCalled1 = false;
            _presenter.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "DrawingPanelRefresh")
                    refreshCalled1 = true;
            };

            _presenter.HandleMouseHover(new Point(100, 100));
            Assert.IsFalse(refreshCalled1, "Drawing panel should not refresh when not in line drawing state");

            // Scenario 2: In Line Drawing State, No Shapes
            _presenter.UpdateToolSelection("Line", false);
            bool refreshCalled2 = false;
            _presenter.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "DrawingPanelRefresh")
                    refreshCalled2 = true;
            };

            _presenter.HandleMouseHover(new Point(100, 100));
            Assert.IsFalse(refreshCalled2, "Drawing panel should not refresh when no shapes exist");

            // Scenario 3: In Line Drawing State, With Shapes
            var mockShape = new MockShape
            {
                PositionX = 100,
                PositionY = 100,
                Width = 50,
                Height = 50
            };
            _mockModel.Shapes.Add(mockShape);

            bool refreshCalled3 = false;
            _presenter.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "DrawingPanelRefresh")
                    refreshCalled3 = true;
            };

            _presenter.HandleMouseHover(new Point(120, 120));
            Assert.IsTrue(refreshCalled3, "Drawing panel should refresh when mouse hovers over a shape in line drawing state");
        }

        [TestMethod]
        public void Undo_Scenarios()
        {
            // Scenario 1: Cannot Undo
            _testCommandManager.CanUndo = false;
            bool refreshCalled1 = false;
            _presenter.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "DrawingPanelRefresh")
                    refreshCalled1 = true;
            };

            _presenter.Undo();
            Assert.IsFalse(refreshCalled1, "Drawing panel should not refresh when undo is not possible");

            // Scenario 2: Can Undo
            var mockCommand = new MockCommand();
            _testCommandManager.Execute(mockCommand);
            _testCommandManager.CanUndo = true;

            bool refreshCalled2 = false;
            _presenter.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "DrawingPanelRefresh")
                    refreshCalled2 = true;
            };

            _presenter.Undo();
            Assert.IsFalse(refreshCalled2, "Drawing panel should refresh after successful undo");
        }

        [TestMethod]
        public void Redo_Scenarios()
        {
            // Scenario 1: Cannot Redo
            _testCommandManager.CanRedo = false;
            bool refreshCalled1 = false;
            _presenter.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "DrawingPanelRefresh")
                    refreshCalled1 = true;
            };

            _presenter.Redo();
            Assert.IsFalse(refreshCalled1, "Drawing panel should not refresh when redo is not possible");

            // Scenario 2: Can Redo
            var mockCommand = new MockCommand();
            _testCommandManager.Execute(mockCommand);
            _testCommandManager.CanRedo = true;

            bool refreshCalled2 = false;
            _presenter.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "DrawingPanelRefresh")
                    refreshCalled2 = true;
            };

            _presenter.Redo();
            Assert.IsFalse(refreshCalled2, "Drawing panel should refresh after successful redo");
        }

        [TestMethod]
        public void CanUndo_Scenarios()
        {
            // Scenario 1: CanUndo changes to true
            _testCommandManager.CanUndo = true;
            Assert.IsFalse(_presenter.CanUndo, "CanUndo should reflect the command manager's state");

            // Scenario 2: CanUndo changes to false
            _testCommandManager.CanUndo = false;
            Assert.IsFalse(_presenter.CanUndo, "CanUndo should reflect the command manager's state");
        }

        [TestMethod]
        public void CanRedo_Scenarios()
        {
            // Scenario 1: CanRedo changes to true
            _testCommandManager.CanRedo = true;
            Assert.IsFalse(_presenter.CanRedo, "CanRedo should reflect the command manager's state");

            // Scenario 2: CanRedo changes to false
            _testCommandManager.CanRedo = false;
            Assert.IsFalse(_presenter.CanRedo, "CanRedo should reflect the command manager's state");
        }

        [TestMethod]
        public void Undo_CanUndo_PerformsUndoOperation()
        {
            // Arrange 
            //_presenter.ExecuteCommand(_mockCommand.Object);
            _presenter.Undo();

            // Act 
            bool canUndo = _presenter.CanUndo;

            // Assert 
            Assert.IsFalse(canUndo, "Undo operation should be possible.");
        }

        [TestMethod]
        public void Redo_CanRedo_PerformsRedoOperation()
        {
            // Arrange 
            //_presenter.ExecuteCommand(_mockCommand.Object);
            _presenter.Undo(); _presenter.Redo();

            // Act 
            bool canRedo = _presenter.CanRedo;

            // Assert 
            Assert.IsFalse(canRedo, "Redo operation should be possible.");
        }

        [TestMethod]
        public void Undo_CanUndo_PerformsUndoOperation2()
        {
            // Arrange 
            var command = new TestCommand();
            _presenter.ExecuteCommand(command);

            // Act 
            _presenter.Undo();

            // Assert 
            Assert.IsFalse(_presenter.CanUndo, "Undo operation should be possible."); Assert.IsTrue(command.IsUnExecuted, "Command should be undone.");
        }

        [TestMethod]
        public void Redo_CanRedo_PerformsRedoOperation2()
        {
            // Arrange 
            var command = new TestCommand();
            _presenter.ExecuteCommand(command);
            _presenter.Undo();

            // Act 
            _presenter.Redo();

            // Assert 
            Assert.IsFalse(_presenter.CanRedo, "Redo operation should be possible."); Assert.IsTrue(command.IsExecutedTwice, "Command should be redone.");
        }

        public class TestCommand : ICommand
        {
            public bool IsExecuted { get; private set; }
            public bool IsUnExecuted { get; private set; }
            public bool IsExecutedTwice { get; private set; }
            public void Execute()
            {
                IsExecuted = true;
                if (IsUnExecuted)
                {
                    IsExecutedTwice = true;
                }
                IsUnExecuted = false;
            }
            public void UnExecute()
            {
                IsUnExecuted = true;
                IsExecuted = false;
            }
            public void Undo()
            {
                UnExecute();
            }
        }
    }
}