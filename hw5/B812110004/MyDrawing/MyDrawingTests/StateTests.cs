using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyDrawing.States;
using MyDrawing.Shapes;
using System.Drawing;
using System.Windows.Forms;
using System;
using System.Linq;

namespace MyDrawing.Tests
{
    [TestClass]
    public class DrawingStateTests
    {
        private MyDrawingModel _model;
        private MyDrawingPresenter _presenter;
        private DrawingState _drawingState;
        private Cursor _lastSetCursor;

        [TestInitialize]
        public void Setup()
        {
            _model = new MyDrawingModel();
            _presenter = new MyDrawingPresenter(_model, cursor => _lastSetCursor = cursor);
            _drawingState = new DrawingState(_model, _presenter, cursor => _lastSetCursor = cursor);
            _lastSetCursor = null;
        }

        [TestMethod]
        public void MouseDown_ShouldSetDrawingStateAndCursor()
        {
            // Arrange
            Point testLocation = new Point(100, 200);
            _presenter.UpdateToolSelection("Start", false);

            // Act
            _drawingState.MouseDown(testLocation);

            // Assert
            Assert.IsTrue(_drawingState.IsDrawing);
            Assert.AreEqual(Cursors.Cross, _lastSetCursor);
        }

        [TestMethod]
        public void MouseMove_WhileNotDrawing_ShouldNotUpdateTempShape()
        {
            // Arrange
            var initialTempShape = _drawingState.GetTempShape();

            // Act
            _drawingState.MouseMove(new Point(100, 200));

            // Assert
            var currentTempShape = _drawingState.GetTempShape();
            Assert.AreEqual(initialTempShape.Width, currentTempShape.Width);
            Assert.AreEqual(initialTempShape.Height, currentTempShape.Height);
        }

        [TestMethod]
        public void MouseUp_WithValidShapeCreation_ShouldResetState()
        {
            // Arrange
            _presenter.UpdateToolSelection("Process", false);
            _drawingState.MouseDown(new Point(0, 0));
            _drawingState.MouseMove(new Point(100, 100));

            // Act
            _drawingState.MouseUp(new Point(100, 100));

            // Assert
            Assert.IsFalse(_drawingState.IsDrawing);
            Assert.AreEqual(Cursors.Default, _lastSetCursor);
        }

        [TestMethod]
        public void MouseUp_WithStartPointLessThanEndPoint_ShouldCalculateCorrectCoordinates()
        {
            // Arrange
            _presenter.UpdateToolSelection("Process", false);
            Point startPoint = new Point(100, 100);
            Point endPoint = new Point(50, 50);

            // Act
            _drawingState.MouseDown(startPoint);
            _drawingState.MouseMove(endPoint);
            _drawingState.MouseUp(endPoint);

            // Assert
            var addedShape = _model.GetShapes().LastOrDefault();
            Assert.IsNotNull(addedShape);

            Assert.AreEqual(50, addedShape.PositionX, "X coordinate should be the smaller point");
            Assert.AreEqual(50, addedShape.PositionY, "Y coordinate should be the smaller point");
            Assert.AreEqual(50, addedShape.Width, "Width should be the absolute difference");
            Assert.AreEqual(50, addedShape.Height, "Height should be the absolute difference");
        }

        [TestMethod]
        public void MouseUp_WithEndPointLessThanStartPoint_ShouldCalculateCorrectCoordinates()
        {
            // Arrange
            _presenter.UpdateToolSelection("Process", false);
            Point startPoint = new Point(50, 50);
            Point endPoint = new Point(100, 100);

            // Act
            _drawingState.MouseDown(startPoint);
            _drawingState.MouseMove(endPoint);
            _drawingState.MouseUp(endPoint);

            // Assert
            var addedShape = _model.GetShapes().LastOrDefault();
            Assert.IsNotNull(addedShape);

            Assert.AreEqual(50, addedShape.PositionX, "X coordinate should be the smaller point");
            Assert.AreEqual(50, addedShape.PositionY, "Y coordinate should be the smaller point");
            Assert.AreEqual(50, addedShape.Width, "Width should be the absolute difference");
            Assert.AreEqual(50, addedShape.Height, "Height should be the absolute difference");
        }

        [TestMethod]
        public void MouseUp_WithNegativeStraightLineLength_ShouldStillCreateShape()
        {
            // Arrange
            _presenter.UpdateToolSelection("Terminator", false);
            Point startPoint = new Point(100, 100);
            Point endPoint = new Point(50, 50);

            // Act
            _drawingState.MouseDown(startPoint);
            _drawingState.MouseMove(endPoint);
            _drawingState.MouseUp(endPoint);

            // Assert
            var addedShape = _model.GetShapes().LastOrDefault();
            Assert.IsNotNull(addedShape);
            Assert.AreEqual("Terminator", addedShape.GetShapeType());
            Assert.AreEqual(50, addedShape.PositionX);
            Assert.AreEqual(50, addedShape.PositionY);
            Assert.AreEqual(50, addedShape.Width);
            Assert.AreEqual(50, addedShape.Height);
        }

        [TestMethod]
        public void GetCursor_ReturnsCorrectCursor()
        {
            // Arrange
            _presenter.UpdateToolSelection("Start", false);

            // Act & Assert
            Assert.AreEqual(Cursors.Cross, _drawingState.GetCursor());

            // Reset
            _presenter.UpdateToolSelection(null, true);
            Assert.AreEqual(Cursors.Default, _drawingState.GetCursor());
        }

        [TestMethod]
        public void DrawTemporaryShape_TerminatorWithNegativeStraightLineLength_ShouldDrawCorrectly()
        {
            // Arrange
            Graphics graphics = Graphics.FromImage(new Bitmap(200, 200));
            _presenter.UpdateToolSelection("Terminator", false);
            _drawingState.MouseDown(new Point(100, 100));

            // Act - create a scenario where straightLineLength is negative
            _drawingState.MouseMove(new Point(50, 150));
            _drawingState.DrawTemporaryShape(graphics);

            // Assert - no exception should be thrown
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void DrawTemporaryShape_ForAllShapeTypes_ShouldWorkWithoutErrors()
        {
            // Arrange
            string[] shapeTypes = { "Start", "Terminator", "Process", "Decision" };
            Graphics graphics = Graphics.FromImage(new Bitmap(200, 200));

            foreach (var shapeType in shapeTypes)
            {
                // Setup
                _presenter.UpdateToolSelection(shapeType, false);
                _drawingState.MouseDown(new Point(0, 0));
                _drawingState.MouseMove(new Point(100, 100));

                // Act
                _drawingState.DrawTemporaryShape(graphics);

                // Reset
                _drawingState = new DrawingState(_model, _presenter, cursor => _lastSetCursor = cursor);
            }

            // Assert - just ensuring no exceptions were thrown
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void DrawTemporaryShape_TerminatorWithVerySmallWidth_ShouldDrawCorrectly()
        {
            // Arrange
            Graphics graphics = Graphics.FromImage(new Bitmap(200, 200));
            _presenter.UpdateToolSelection("Terminator", false);

            // Act - create a scenario where width is smaller than height
            _drawingState.MouseDown(new Point(100, 100));
            _drawingState.MouseMove(new Point(90, 150)); // Very small width

            // Assert - no exception should be thrown and verify drawing behavior
            try
            {
                _drawingState.DrawTemporaryShape(graphics);
                Assert.IsTrue(true, "DrawTemporaryShape should handle small width without exception");
            }
            catch (Exception ex)
            {
                Assert.Fail($"DrawTemporaryShape threw an unexpected exception: {ex.Message}");
            }
        }

        [TestMethod]
        public void DrawTemporaryShape_TerminatorWithNegativeCoordinates_ShouldDrawCorrectly()
        {
            // Arrange
            Graphics graphics = Graphics.FromImage(new Bitmap(200, 200));
            _presenter.UpdateToolSelection("Terminator", false);

            // Act - create a scenario with negative coordinate differences
            _drawingState.MouseDown(new Point(150, 150));
            _drawingState.MouseMove(new Point(100, 50)); // Move to top-left, creating negative width and height

            // Assert - no exception should be thrown and verify drawing behavior
            try
            {
                _drawingState.DrawTemporaryShape(graphics);
                Assert.IsTrue(true, "DrawTemporaryShape should handle negative coordinates without exception");
            }
            catch (Exception ex)
            {
                Assert.Fail($"DrawTemporaryShape threw an unexpected exception: {ex.Message}");
            }
        }

        [TestMethod]
        public void DrawTemporaryShape_TerminatorWithPositiveStraightLineLength_ShouldDrawHorizontalLines()
        {
            // Arrange
            Bitmap bitmap = new Bitmap(200, 200);
            Graphics graphics = Graphics.FromImage(bitmap);
            _presenter.UpdateToolSelection("Terminator", false);

            // Act - create a scenario with a positive straightLineLength
            _drawingState.MouseDown(new Point(100, 100));
            _drawingState.MouseMove(new Point(200, 150)); // Creates a wider shape with positive straightLineLength

            // Perform drawing
            _drawingState.DrawTemporaryShape(graphics);

            // Assert - no exception should be thrown
            // Since we can't easily verify exact line drawing, we'll check for no exceptions
            Assert.IsTrue(true, "DrawTemporaryShape should handle positive straightLineLength without errors");

            // Cleanup
            graphics.Dispose();
            bitmap.Dispose();
        }
    }

    [TestClass]
    public class SelectionStateTests : SelectionState
    {
        private MyDrawingModel _model;
        private MyDrawingPresenter _presenter;
        private SelectionState _selectionState;
        private IShape _mockShape;

        public SelectionStateTests() : base(new MyDrawingModel(), new MyDrawingPresenter(new MyDrawingModel(), null))
        {
            // 這裡呼叫基底類別的構造函數
        }

        [TestInitialize]
        public void Setup()
        {
            _model = new MyDrawingModel();
            _presenter = new MyDrawingPresenter(_model, null);
            _selectionState = new SelectionState(_model, _presenter);

            // Use the model's method to add a shape with required parameters
            var presenter = new MyDrawingPresenter(_model, cursor => { /* 測試時不需特別操作 */ });
            _model.AddShape("Process", "Test Shape", 100, 100, 50, 50, presenter);
            _mockShape = _model.GetShapes().Last();
        }

        [TestMethod]
        public void MouseDown_ShapeExists_ShouldSelectShape()
        {
            // Arrange
            Point location = new Point(125, 125);

            // Act
            _selectionState.MouseDown(location);

            // Assert
            Assert.AreEqual(_mockShape, _presenter.SelectedShape);
        }

        [TestMethod]
        public void MouseDown_NoShapeAtLocation_ShouldClearSelection()
        {
            // Arrange
            Point location = new Point(200, 200);

            // Act
            _selectionState.MouseDown(location);

            // Assert
            Assert.IsNull(_presenter.SelectedShape);
        }

        [TestMethod]
        public void MouseMove_DraggingShape_ShouldUpdateShapePosition()
        {
            // Arrange
            Point initialLocation = new Point(125, 125);
            Point newLocation = new Point(150, 160);

            // Select the shape first
            _selectionState.MouseDown(initialLocation);

            // Act
            _selectionState.MouseMove(newLocation);

            // Assert
            Assert.AreEqual(125, _mockShape.PositionX);
            Assert.AreEqual(135, _mockShape.PositionY);
        }

        [TestMethod]
        public void MouseMove_DraggingText_ShouldUpdateTextPosition()
        {
            Point initialLocation = new Point(125, 125);
            Point newLocation = new Point(150, 160);

            _selectionState.MouseDown(initialLocation);
            _selectionState.MouseMove(newLocation);

            Assert.AreEqual(125, _mockShape.TextPositionX);
            Assert.AreEqual(135, _mockShape.TextPositionY);
        }

        [TestMethod]
        public void MouseUp_WhenDragging_ShouldTriggerShapesChanged()
        {
            // Arrange
            bool shapesChangedCalled = false;
            _model.ShapesChanged += (sender, args) => shapesChangedCalled = true;

            Point initialLocation = new Point(125, 125);
            Point newLocation = new Point(150, 160);

            // Select and move the shape
            _selectionState.MouseDown(initialLocation);
            _selectionState.MouseMove(newLocation);

            // Act
            _selectionState.MouseUp(new Point(150, 160));

            // Assert
            Assert.IsTrue(shapesChangedCalled);
        }

        [TestMethod]
        public void GetCursor_ShouldReturnDefaultCursor()
        {
            // Act
            Cursor cursor = _selectionState.GetCursor();

            // Assert
            Assert.AreEqual(Cursors.Default, cursor);
        }



        [TestMethod]
        public void UpdateOrangeDotPosition_ShouldUpdateCorrectlyWhenTextPositionChanges()
        {
            Point initialLocation = new Point(125, 125);
            _selectionState.MouseDown(initialLocation);

            // Move text within shape
            _selectionState.MouseMove(new Point(150, 160));

            // Verify that the orange dot position updates accordingly
            Assert.AreNotEqual(_mockShape.OrangeDotPosition, new PointF(0, 0));
        }

        [TestMethod]
        public void MouseDown_OrangeDotClicked_ShouldInitiateDraggingText()
        {
            // Arrange
            // Manually set the orange dot position to match the test location
            _mockShape.OrangeDotPosition = new PointF(130, 122);
            Point location = new Point(130, 122);

            // Act
            _selectionState.MouseDown(location);

            // Assert
            // Use reflection to check private field _isDraggingText
            var isDraggingTextField = typeof(SelectionState).GetField("_isDraggingText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var isDraggingText = (bool)isDraggingTextField.GetValue(_selectionState);
            Assert.IsTrue(isDraggingText);
        }

        [TestMethod]
        public void MouseDown_NoShapeAtLocation_ShouldNotThrowException()
        {
            // Arrange
            Point location = new Point(500, 500); // Far outside any shape

            // Act & Assert
            try
            {
                _selectionState.MouseDown(location);
            }
            catch (Exception ex)
            {
                Assert.Fail($"MouseDown threw an unexpected exception: {ex.Message}");
            }
        }

        [TestMethod]
        public void MouseMove_NoSelectedShape_ShouldNotThrowException()
        {
            // Arrange
            Point location = new Point(200, 200);

            // Act & Assert
            try
            {
                _selectionState.MouseMove(location);
            }
            catch (Exception ex)
            {
                Assert.Fail($"MouseMove threw an unexpected exception: {ex.Message}");
            }
        }

        

        [TestMethod]
        public void MouseMove_TextDragging_BoundaryConditions()
        {
            // Arrange
            Point initialLocation = new Point(125, 125);
            _selectionState.MouseDown(initialLocation);

            // Test dragging text to extreme positions
            Point[] testLocations = new Point[]
            {
            new Point(50, 50),     // Far top-left
            new Point(200, 200),   // Far bottom-right
            new Point(0, 0),       // Extreme top-left
            new Point(1000, 1000)  // Extreme bottom-right
            };

            foreach (var location in testLocations)
            {
                // Act
                _selectionState.MouseMove(location);

                // Assert
                // Verify that text position remains within shape boundaries
                Assert.IsTrue(_mockShape.TextPositionX >= _mockShape.PositionX - (_mockShape.Width / 2));
                Assert.IsTrue(_mockShape.TextPositionX <= _mockShape.PositionX + (_mockShape.Width / 2));
                Assert.IsTrue(_mockShape.TextPositionY >= _mockShape.PositionY - (_mockShape.Height / 2));
                Assert.IsTrue(_mockShape.TextPositionY <= _mockShape.PositionY + (_mockShape.Height / 2));
            }
        }

        [TestMethod]
        public void UpdateOrangeDotPosition_NoText_ShouldNotThrowException()
        {
            // Arrange
            _mockShape.Text = null;

            // Act & Assert
            try
            {
                // Use reflection to call private method
                var method = typeof(SelectionState).GetMethod("UpdateOrangeDotPosition",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method.Invoke(_selectionState, null);
            }
            catch (Exception ex)
            {
                Assert.Fail($"UpdateOrangeDotPosition threw an unexpected exception: {ex.Message}");
            }
        }

        [TestMethod]
        public void MouseUp_MultipleInvocations_ShouldResetState()
        {
            // Arrange
            Point initialLocation = new Point(125, 125);
            _selectionState.MouseDown(initialLocation);
            _selectionState.MouseMove(new Point(150, 160));

            // Act
            _selectionState.MouseUp(new Point(150, 160));
            _selectionState.MouseUp(new Point(150, 160)); // Double invocation

            // Assert
            // Use reflection to check private fields
            var isDraggingField = typeof(SelectionState).GetField("_isDragging",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var isDraggingTextField = typeof(SelectionState).GetField("_isDraggingText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var isDragging = (bool)isDraggingField.GetValue(_selectionState);
            var isDraggingText = (bool)isDraggingTextField.GetValue(_selectionState);

            Assert.IsFalse(isDragging);
            Assert.IsFalse(isDraggingText);
        }

        [TestMethod]
        public void MouseMove_TextDragging_VerifyTextPositionAndOrangeDot()
        {
            // Arrange
            // Create a shape with text
            Point initialLocation = new Point(125, 125);
            _mockShape.Text = "Test Text"; // Ensure text exists

            // Manually set initial text position
            _mockShape.TextPositionX = (int)_mockShape.PositionX;
            _mockShape.TextPositionY = (int)_mockShape.PositionY;

            // Simulate selecting the shape and preparing for text drag
            _selectionState.MouseDown(initialLocation);

            // Use reflection to set _isDraggingText to true
            var isDraggingTextField = typeof(SelectionState).GetField("_isDraggingText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            isDraggingTextField.SetValue(_selectionState, true);

            // Simulate text offset
            var textOffsetField = typeof(SelectionState).GetField("_textOffset",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            textOffsetField.SetValue(_selectionState, new PointF(0, 0));

            // Act
            Point newLocation = new Point(150, 160);
            _selectionState.MouseMove(newLocation);

            // Assert
            // Verify text position has changed
            Assert.AreNotEqual(initialLocation.X, _mockShape.TextPositionX);
            Assert.AreNotEqual(initialLocation.Y, _mockShape.TextPositionY);

            // Verify orange dot position has been updated
            Assert.AreNotEqual(new PointF(0, 0), _mockShape.OrangeDotPosition);

            // Verify text stays within shape boundaries
            Assert.IsTrue(_mockShape.TextPositionX >= _mockShape.PositionX - (_mockShape.Width / 2));
            Assert.IsTrue(_mockShape.TextPositionX <= _mockShape.PositionX + (_mockShape.Width / 2));
            Assert.IsTrue(_mockShape.TextPositionY >= _mockShape.PositionY - (_mockShape.Height / 2));
            Assert.IsTrue(_mockShape.TextPositionY <= _mockShape.PositionY + (_mockShape.Height / 2));
        }

    }
}