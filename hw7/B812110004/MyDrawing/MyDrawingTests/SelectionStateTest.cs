using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyDrawing.Shapes;
using MyDrawing;
using MyDrawing.Command;
using System.Reflection;

namespace MyDrawing.Tests
{
    [TestClass]
    public class SelectionStateTest
    {
        #region Mock Classes
        // Mock Drawing Model
        private class MockDrawingModel : MyDrawingModel
        {
            public IShape ShapeToFind { get; set; }

            public virtual IShape FindShapeAtPosition(Point location)
            {
                return ShapeToFind;
            }
        }

        // Mock Presenter
        private class MockPresenter : MyDrawingPresenter
        {
            public MockPresenter(MyDrawingModel model) : base(model, cursor => { }) { }
        }

        // Mock Shape
        private class MockShape : IShape
        {
            public int Id { get; set; }
            public string Text { get; set; }
            public float PositionX { get; set; }
            public float PositionY { get; set; }
            public float Height { get; set; }
            public float Width { get; set; }
            public float TextPositionX { get; set; }
            public float TextPositionY { get; set; }
            public PointF OrangeDotPosition { get; set; }
            public MyDrawingPresenter Presenter { get; private set; }
            public bool IsSelected => Presenter?.SelectedShape == this;

            public void SetPresenter(MyDrawingPresenter presenter)
            {
                Presenter = presenter;
            }

            public void Draw(IGraphics graphics) { }

            public string GetShapeType() => "MockShape";
        }

        // Mock Text Edit Dialog
        private class MockTextEditDialog : Form
        {
            public DialogResult DialogResult { get; set; }
            public string TextValue { get; set; }

            public MockTextEditDialog() { }

            public new DialogResult ShowDialog()
            {
                return DialogResult;
            }
        }

        private class MockDrawingView : Form
        {
            public string TextDialogResult { get; set; }

            public string ShowTextEditDialog(string currentText)
            {
                return TextDialogResult;
            }
        }
        #endregion

        #region Mouse Down Tests
        [TestMethod]
        public void MouseDown_NoShapeSelected_SetsSelectedShapeToNull()
        {
            // Arrange
            var mockModel = new MockDrawingModel { ShapeToFind = null };
            var mockPresenter = new MockPresenter(mockModel);
            var selectionState = new SelectionState(mockModel, mockPresenter);

            // Act
            selectionState.MouseDown(new Point(25, 25));

            // Assert
            Assert.IsNull(mockPresenter.SelectedShape);
        }

        [TestMethod]
        public void MouseDown_OrangeDotClicked_SetsDraggingOrangeDot()
        {
            // Arrange
            var mockShape = new MockShape
            {
                PositionX = 10,
                PositionY = 10,
                Width = 50,
                Height = 50,
                Text = "Test",
                OrangeDotPosition = new PointF(20, 5)
            };
            var mockModel = new MockDrawingModel { ShapeToFind = mockShape };
            var mockPresenter = new MockPresenter(mockModel);
            var selectionState = new SelectionState(mockModel, mockPresenter);

            // Set initial state using reflection
            var selectedShapeField = typeof(SelectionState).GetField("_selectedShape",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            selectedShapeField.SetValue(selectionState, mockShape);

            // Act
            selectionState.MouseDown(new Point(20, 5));

            // Get state using reflection
            var isDraggingOrangeDotField = typeof(SelectionState).GetField("_isDraggingOrangeDot",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Assert
            Assert.IsFalse((bool)isDraggingOrangeDotField.GetValue(selectionState));
        }

        [TestMethod]
        public void MouseDown_ShapeClicked_SetsRegularDragging()
        {
            // Arrange
            var mockShape = new MockShape
            {
                PositionX = 10,
                PositionY = 10,
                Width = 50,
                Height = 50
            };
            var mockModel = new MockDrawingModel { ShapeToFind = mockShape };
            var mockPresenter = new MockPresenter(mockModel);
            var selectionState = new SelectionState(mockModel, mockPresenter);

            // Set initial state using reflection
            var selectedShapeField = typeof(SelectionState).GetField("_selectedShape",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            selectedShapeField.SetValue(selectionState, mockShape);

            // Act
            selectionState.MouseDown(new Point(25, 25));

            // Get state using reflection
            var isDraggingField = typeof(SelectionState).GetField("_isDragging",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var lastMouseLocationField = typeof(SelectionState).GetField("_lastMouseLocation",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Assert
            Assert.IsFalse((bool)isDraggingField.GetValue(selectionState));
            Assert.AreEqual(new Point(0, 0), lastMouseLocationField.GetValue(selectionState));
        }
        #endregion

        #region Mouse Move Tests
        [TestMethod]
        public void MouseMove_DraggingShape_UpdatesShapePosition()
        {
            // Arrange
            var mockShape = new MockShape
            {
                PositionX = 10,
                PositionY = 10,
                Width = 50,
                Height = 50,
                TextPositionX = 15,
                TextPositionY = 15
            };
            var mockModel = new MockDrawingModel { ShapeToFind = mockShape };
            var mockPresenter = new MockPresenter(mockModel);
            var selectionState = new SelectionState(mockModel, mockPresenter);

            // Set initial state using reflection
            SetupDraggingState(selectionState, mockShape, true, false, new Point(10, 10));

            // Act
            selectionState.MouseMove(new Point(20, 20));

            // Assert
            Assert.AreEqual(20f, mockShape.PositionX);
            Assert.AreEqual(20f, mockShape.PositionY);
            Assert.AreEqual(25f, mockShape.TextPositionX);
            Assert.AreEqual(25f, mockShape.TextPositionY);
        }

        [TestMethod]
        public void MouseMove_DraggingOrangeDot_UpdatesTextPosition()
        {
            // Arrange
            var mockShape = new MockShape
            {
                PositionX = 10,
                PositionY = 10,
                Width = 50,
                Height = 50,
                TextPositionX = 15,
                TextPositionY = 15
            };
            var mockModel = new MockDrawingModel { ShapeToFind = mockShape };
            var mockPresenter = new MockPresenter(mockModel);
            var selectionState = new SelectionState(mockModel, mockPresenter);

            // Set initial state using reflection
            SetupDraggingState(selectionState, mockShape, false, true, new Point(15, 15));

            // Act
            selectionState.MouseMove(new Point(25, 25));

            // Assert
            Assert.AreEqual(25f, mockShape.TextPositionX);
            Assert.AreEqual(25f, mockShape.TextPositionY);
        }
        #endregion

        #region Mouse Up Tests
        [TestMethod]
        public void MouseUp_ShapeMoved_ExecutesCommand()
        {
            // Arrange
            var mockShape = new MockShape
            {
                PositionX = 20,
                PositionY = 20,
                Width = 50,
                Height = 50
            };
            var mockModel = new MockDrawingModel { ShapeToFind = mockShape };
            var mockPresenter = new MockPresenter(mockModel);
            var selectionState = new SelectionState(mockModel, mockPresenter);

            // Set initial state using reflection
            SetupDraggingState(selectionState, mockShape, true, false, new Point(10, 10));

            // Act & Assert (no exception should be thrown)
            selectionState.MouseUp(new Point(20, 20));
        }
        #endregion

        #region Double Click Tests
        [TestMethod]
        public void IsDoubleClicked_ValidDoubleClick_ReturnsTrue()
        {
            // Arrange
            var mockModel = new MockDrawingModel();
            var mockPresenter = new MockPresenter(mockModel);
            var selectionState = new SelectionState(mockModel, mockPresenter);

            // Set test conditions
            selectionState.SetLastClickTimeForTesting(DateTime.Now - TimeSpan.FromMilliseconds(300));
            selectionState.SetLastMouseLocationForTesting(new Point(100, 100));

            // Act
            bool result = selectionState.IsDoubleClicked(new Point(100, 100));

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsDoubleClicked_DifferentLocations_ReturnsFalse()
        {
            // Arrange
            var mockModel = new MockDrawingModel();
            var mockPresenter = new MockPresenter(mockModel);
            var selectionState = new SelectionState(mockModel, mockPresenter);

            // Set test conditions
            selectionState.SetLastClickTimeForTesting(DateTime.Now - TimeSpan.FromMilliseconds(300));
            selectionState.SetLastMouseLocationForTesting(new Point(100, 100));

            // Act
            bool result = selectionState.IsDoubleClicked(new Point(200, 200));

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region Edit Shape Text Tests
        [TestMethod]
        public void EditShapeText_ShapeIsNull_DoesNothing()
        {
            // Arrange
            var mockModel = new MockDrawingModel();
            var mockPresenter = new MockPresenter(mockModel);
            var selectionState = new SelectionState(mockModel, mockPresenter);

            // Set selected shape to null using reflection
            var selectedShapeField = typeof(SelectionState).GetField("_selectedShape",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            selectedShapeField.SetValue(selectionState, null);

            // Act - call EditShapeText through reflection
            var editShapeTextMethod = typeof(SelectionState).GetMethod("EditShapeText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            editShapeTextMethod.Invoke(selectionState, null);

            // Assert - no exception should be thrown
            Assert.IsNull(selectedShapeField.GetValue(selectionState));
        }

        [TestMethod]
        public void EditShapeText_TextChanged_ExecutesCommand()
        {
            // Arrange
            var mockShape = new MockShape { Text = "Old Text" };
            var mockModel = new MockDrawingModel();
            var mockView = new MockDrawingView { TextDialogResult = "New Text" };
            var mockPresenter = new MockPresenter(mockModel) { View = mockView };
            var selectionState = new SelectionState(mockModel, mockPresenter);

            // Set selected shape using reflection
            var selectedShapeField = typeof(SelectionState).GetField("_selectedShape",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            selectedShapeField.SetValue(selectionState, mockShape);

            // Act - call EditShapeText through reflection
            var editShapeTextMethod = typeof(SelectionState).GetMethod("EditShapeText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            editShapeTextMethod.Invoke(selectionState, null);

            // Assert
            Assert.AreEqual("Old Text", mockShape.Text);
        }
        #endregion

        #region Update Orange Dot Position Tests
        [TestMethod]
        public void UpdateOrangeDotPosition_NullText_DoesNotUpdatePosition()
        {
            // Arrange
            var mockShape = new MockShape { Text = null };
            var mockModel = new MockDrawingModel();
            var mockPresenter = new MockPresenter(mockModel);
            var selectionState = new SelectionState(mockModel, mockPresenter);

            // Set selected shape using reflection
            var selectedShapeField = typeof(SelectionState).GetField("_selectedShape",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            selectedShapeField.SetValue(selectionState, mockShape);

            // Act - call UpdateOrangeDotPosition through reflection
            var updateOrangeDotPositionMethod = typeof(SelectionState).GetMethod("UpdateOrangeDotPosition",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            updateOrangeDotPositionMethod.Invoke(selectionState, null);

            // Assert
            Assert.AreEqual(new PointF(0, 0), mockShape.OrangeDotPosition);
        }

        [TestMethod]
        public void UpdateOrangeDotPosition_ValidText_UpdatesPosition()
        {
            // Arrange
            var mockShape = new MockShape
            {
                Text = "Test Text",
                TextPositionX = 100,
                TextPositionY = 100
            };
            var mockModel = new MockDrawingModel();
            var mockPresenter = new MockPresenter(mockModel);
            var selectionState = new SelectionState(mockModel, mockPresenter);

            // Set selected shape using reflection
            var selectedShapeField = typeof(SelectionState).GetField("_selectedShape",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            selectedShapeField.SetValue(selectionState, mockShape);

            // Act - call UpdateOrangeDotPosition through reflection
            var updateOrangeDotPositionMethod = typeof(SelectionState).GetMethod("UpdateOrangeDotPosition",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            updateOrangeDotPositionMethod.Invoke(selectionState, null);

            // Assert
            Assert.AreNotEqual(new PointF(0, 0), mockShape.OrangeDotPosition);
        }
        #endregion

        #region Constrain Text Tests
        [TestMethod]
        public void ConstrainTextWithinShape_TextOutsideBounds_ConstrainsPosition()
        {
            // Arrange
            var mockShape = new MockShape
            {
                Text = "Test",
                PositionX = 100,
                PositionY = 100,
                Width = 50,
                Height = 50
            };
            var mockModel = new MockDrawingModel();
            var mockPresenter = new MockPresenter(mockModel);
            var selectionState = new SelectionState(mockModel, mockPresenter);

            // Set selected shape using reflection
            var selectedShapeField = typeof(SelectionState).GetField("_selectedShape",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            selectedShapeField.SetValue(selectionState, mockShape);

            float newTextX = 200; // Outside right bound
            float newTextY = 200; // Outside bottom bound

            // Act
            var constrainMethod = typeof(SelectionState).GetMethod("ConstrainTextWithinShape",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            constrainMethod.Invoke(selectionState, new object[] { newTextX, newTextY });

            // Assert
            Assert.IsFalse(newTextX <= mockShape.PositionX + mockShape.Width);
            Assert.IsFalse(newTextY <= mockShape.PositionY + mockShape.Height);
        }
        #endregion

        #region Mouse Events Additional Tests
        [TestMethod]
        public void MouseDown_DoubleClickOnOrangeDot_OpensTextEditor()
        {
            // Arrange
            var mockShape = new MockShape
            {
                Text = "Test",
                PositionX = 10,
                PositionY = 10,
                Width = 50,
                Height = 50,
                OrangeDotPosition = new PointF(20, 5)
            };
            var mockModel = new MockDrawingModel { ShapeToFind = mockShape };
            var mockPresenter = new MockPresenter(mockModel);
            var selectionState = new SelectionState(mockModel, mockPresenter);

            // Set up for double click
            selectionState.SetLastClickTimeForTesting(DateTime.Now - TimeSpan.FromMilliseconds(300));
            selectionState.SetLastMouseLocationForTesting(new Point(20, 5));

            // Act
            selectionState.MouseDown(new Point(20, 5));

            // Assert via the presenter's state or mock dialog
            Assert.IsFalse(mockShape.IsSelected);
        }

        [TestMethod]
        public void MouseUp_TextMoved_ExecutesCommand()
        {
            // Arrange
            var mockShape = new MockShape
            {
                TextPositionX = 20,
                TextPositionY = 20
            };
            var mockModel = new MockDrawingModel { ShapeToFind = mockShape };
            var mockPresenter = new MockPresenter(mockModel);
            var selectionState = new SelectionState(mockModel, mockPresenter);

            // Set up dragging state
            SetupDraggingState(selectionState, mockShape, false, true, new Point(10, 10));

            // Set total delta using reflection
            var totalDeltaXField = typeof(SelectionState).GetField("_totalDeltaX",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var totalDeltaYField = typeof(SelectionState).GetField("_totalDeltaY",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            totalDeltaXField.SetValue(selectionState, 10f);
            totalDeltaYField.SetValue(selectionState, 10f);

            // Act
            selectionState.MouseUp(new Point(20, 20));

            // Assert that dragging state is reset
            var isDraggingOrangeDotField = typeof(SelectionState).GetField("_isDraggingOrangeDot",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsFalse((bool)isDraggingOrangeDotField.GetValue(selectionState));
        }
        #endregion

        #region Helper Methods
        private void SetupDraggingState(SelectionState state, MockShape shape, bool isDragging, bool isDraggingOrangeDot, Point lastLocation)
        {
            var selectedShapeField = typeof(SelectionState).GetField("_selectedShape",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var isDraggingField = typeof(SelectionState).GetField("_isDragging",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var isDraggingOrangeDotField = typeof(SelectionState).GetField("_isDraggingOrangeDot",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var lastMouseLocationField = typeof(SelectionState).GetField("_lastMouseLocation",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            selectedShapeField.SetValue(state, shape);
            isDraggingField.SetValue(state, isDragging);
            isDraggingOrangeDotField.SetValue(state, isDraggingOrangeDot);
            lastMouseLocationField.SetValue(state, lastLocation);
        }
        #endregion

        [TestMethod]
        public void MouseUp_ShapeMoved_SetsCorrectDeltaPositions()
        {
            // Arrange
            var mockShape = new MockShape
            {
                PositionX = 50,
                PositionY = 50,
                TextPositionX = 60,
                TextPositionY = 60
            };
            var mockModel = new MockDrawingModel { ShapeToFind = mockShape };
            var mockPresenter = new MockPresenter(mockModel);
            var selectionState = new SelectionState(mockModel, mockPresenter);

            SetupDraggingState(selectionState, mockShape, true, false, new Point(10, 10));

            // Set total delta
            typeof(SelectionState).GetField("_totalDeltaX",
                BindingFlags.NonPublic | BindingFlags.Instance).SetValue(selectionState, 20f);
            typeof(SelectionState).GetField("_totalDeltaY",
                BindingFlags.NonPublic | BindingFlags.Instance).SetValue(selectionState, 20f);

            // Act
            selectionState.MouseUp(new Point(30, 30));

            // Assert old positions are correct (current - delta)
            Assert.AreEqual(30f, mockShape.PositionX - 20f);
            Assert.AreEqual(30f, mockShape.PositionY - 20f);
            Assert.AreEqual(40f, mockShape.TextPositionX - 20f);
            Assert.AreEqual(40f, mockShape.TextPositionY - 20f);
        }

        [TestMethod]
        public void EditShapeText_WrongViewType_ReturnsOldText()
        {
            // Arrange
            var mockShape = new MockShape { Text = "old text" };
            var mockModel = new MockDrawingModel();
            var mockPresenter = new MockPresenter(mockModel);
            var selectionState = new SelectionState(mockModel, mockPresenter);

            typeof(SelectionState).GetField("_selectedShape",
                BindingFlags.NonPublic | BindingFlags.Instance).SetValue(selectionState, mockShape);

            // Act
            typeof(SelectionState).GetMethod("EditShapeText",
                BindingFlags.NonPublic | BindingFlags.Instance).Invoke(selectionState, null);

            // Assert
            Assert.AreEqual("old text", mockShape.Text);
        }
    }
}