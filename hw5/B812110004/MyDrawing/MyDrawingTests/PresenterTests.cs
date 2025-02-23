using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Windows.Forms;
using System;
using MyDrawing.Shapes;
using MyDrawing.States;

namespace MyDrawing.Tests
{
    // 模擬 IShape 接口
    public class MockShape : IShape
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }

        public PointF OrangeDotPosition { get; set; }
        public int TextPositionX { get; set; } 
        public int TextPositionY { get; set; }

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

        [TestInitialize]
        public void Setup()
        {
            _mockModel = new MockMyDrawingModel();
            _presenter = new MyDrawingPresenter(_mockModel, cursor => { });
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
            Assert.IsFalse(_presenter.IsAddButtonEnabled);

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
    }
}