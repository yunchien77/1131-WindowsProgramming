using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyDrawing.Factories;
using MyDrawing.Shapes;
using System;
using System.Windows.Forms;

namespace MyDrawing.Tests
{
    // Mock implementation of IGraphics interface
    public class MockGraphics : IGraphics
    {
        public int DrawEllipseCallCount { get; private set; }
        public int DrawRectangleCallCount { get; private set; }
        public int DrawLineCallCount { get; private set; }
        public int DrawArcCallCount { get; private set; }
        public int DrawTextCallCount { get; private set; }
        public int DrawTextBorderCallCount { get; private set; }

        public void DrawEllipse(int x, int y, int width, int height)
        {
            DrawEllipseCallCount++;
        }

        public void DrawRectangle(int x, int y, int width, int height)
        {
            DrawRectangleCallCount++;
        }

        public void DrawLine(int x1, int y1, int x2, int y2)
        {
            DrawLineCallCount++;
        }

        public void DrawArc(int x, int y, int width, int height, int startAngle, int sweepAngle)
        {
            DrawArcCallCount++;
        }

        public void DrawText(string text, int x, int y, int width, int height)
        {
            DrawTextCallCount++;
        }

        public void DrawTextBorder(string text, int x, int y, int width, int height)
        {
            DrawTextBorderCallCount++;
        }
    }

    [TestClass]
    public class ShapeFactoryTests
    {
        [TestMethod]
        public void CreateShape_ValidShapeTypes_ShouldCreateCorrectShapes()
        {
            Assert.IsInstanceOfType(ShapeFactory.CreateShape("Start"), typeof(StartShape));
            Assert.IsInstanceOfType(ShapeFactory.CreateShape("Terminator"), typeof(TerminatorShape));
            Assert.IsInstanceOfType(ShapeFactory.CreateShape("Process"), typeof(ProcessShape));
            Assert.IsInstanceOfType(ShapeFactory.CreateShape("Decision"), typeof(DecisionShape));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateShape_NullInput_ShouldThrowArgumentException()
        {
            ShapeFactory.CreateShape(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateShape_EmptyString_ShouldThrowArgumentException()
        {
            ShapeFactory.CreateShape("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateShape_WhitespaceInput_ShouldThrowArgumentException()
        {
            ShapeFactory.CreateShape("   ");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateShape_InvalidShapeType_ShouldThrowArgumentException()
        {
            ShapeFactory.CreateShape("Invalid");
        }

        [TestMethod]
        public void CreateShape_InvalidInput_ExceptionMessageShouldBeCorrect()
        {
            try
            {
                ShapeFactory.CreateShape("InvalidShape");
                Assert.Fail("Expected ArgumentException was not thrown");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid shape type", ex.Message);
            }
        }
    }

    [TestClass]
    public class ShapeTests
    {
        [TestMethod]
        public void StartShape_Draw_ShouldCallCorrectGraphicsMethods()
        {
            // Arrange
            var mockGraphics = new MockGraphics();
            var shape = new StartShape
            {
                PositionX = 10,
                PositionY = 20,
                Width = 100,
                Height = 50,
                Text = "Start"
            };

            // Mock parameters for MyDrawingPresenter
            var mockModel = new MyDrawingModel(); // 或使用 Mock 物件
            Action<Cursor> mockAction = cursor => { /* No operation */ };

            // Initialize presenter with mocked parameters
            var mockPresenter = new MyDrawingPresenter(mockModel, mockAction);
            shape.SetPresenter(mockPresenter);

            // Act
            shape.Draw(mockGraphics);

            // Assert
            Assert.AreEqual(1, mockGraphics.DrawEllipseCallCount);
            Assert.AreEqual(1, mockGraphics.DrawTextCallCount);
        }

        [TestMethod]
        public void StartShape_Draw_WhenSelected_ShouldDrawTextBorder()
        {
            // Arrange
            var mockGraphics = new MockGraphics();
            var shape = new StartShape
            {
                PositionX = 10,
                PositionY = 20,
                Width = 100,
                Height = 50,
                Text = "Start"
            };

            // Mock presenter and set the shape as selected
            var mockModel = new MyDrawingModel();
            Action<Cursor> mockAction = cursor => { };
            var presenter = new MyDrawingPresenter(mockModel, mockAction);
            shape.SetPresenter(presenter);
            presenter.SelectedShape = shape; // Simulate selection

            // Act
            shape.Draw(mockGraphics);

            // Assert
            Assert.AreEqual(1, mockGraphics.DrawTextCallCount, "Text should be drawn");
            Assert.AreEqual(1, mockGraphics.DrawTextBorderCallCount, "Text border should be drawn when selected");
        }


        [TestMethod]
        public void TerminatorShape_Draw_WhenWidthLessThanHeight_ShouldAdjustArcDimensions()
        {
            // Arrange
            var mockGraphics = new MockGraphics();
            var shape = new TerminatorShape
            {
                PositionX = 10,
                PositionY = 20,
                Width = 30,  // Less than Height
                Height = 50,
                Text = "Short Width"
            };

            // Mock parameters for MyDrawingPresenter
            var mockModel = new MyDrawingModel(); // 或使用 Mock 物件
            Action<Cursor> mockAction = cursor => { /* No operation */ };

            // Initialize presenter with mocked parameters
            var mockPresenter = new MyDrawingPresenter(mockModel, mockAction);
            shape.SetPresenter(mockPresenter);

            // Act
            shape.Draw(mockGraphics);

            // Assert
            Assert.AreEqual(2, mockGraphics.DrawArcCallCount, "Should draw two arcs");
            Assert.AreEqual(0, mockGraphics.DrawLineCallCount, "Should not draw connecting lines when width < height");
            Assert.AreEqual(1, mockGraphics.DrawTextCallCount, "Should draw centered text");
        }

        [TestMethod]
        public void ProcessShape_Draw_ShouldCallCorrectGraphicsMethods()
        {
            // Arrange
            var mockGraphics = new MockGraphics();
            var shape = new ProcessShape
            {
                PositionX = 10,
                PositionY = 20,
                Width = 100,
                Height = 50,
                Text = "Process"
            };

            // Mock parameters for MyDrawingPresenter
            var mockModel = new MyDrawingModel(); // 或使用 Mock 物件
            Action<Cursor> mockAction = cursor => { /* No operation */ };

            // Initialize presenter with mocked parameters
            var mockPresenter = new MyDrawingPresenter(mockModel, mockAction);
            shape.SetPresenter(mockPresenter);

            // Act
            shape.Draw(mockGraphics);

            // Assert
            Assert.AreEqual(1, mockGraphics.DrawRectangleCallCount);
            Assert.AreEqual(1, mockGraphics.DrawTextCallCount);
        }

        [TestMethod]
        public void TerminatorShape_Draw_ShouldCallCorrectGraphicsMethods()
        {
            // Arrange
            var mockGraphics = new MockGraphics();
            var shape = new TerminatorShape
            {
                PositionX = 10,
                PositionY = 20,
                Width = 200,
                Height = 50,
                Text = "Terminator"
            };

            // Mock parameters for MyDrawingPresenter
            var mockModel = new MyDrawingModel(); // 或使用 Mock 物件
            Action<Cursor> mockAction = cursor => { /* No operation */ };

            // Initialize presenter with mocked parameters
            var mockPresenter = new MyDrawingPresenter(mockModel, mockAction);
            shape.SetPresenter(mockPresenter);

            // Act
            shape.Draw(mockGraphics);

            // Assert
            Assert.AreEqual(2, mockGraphics.DrawArcCallCount);
            Assert.AreEqual(2, mockGraphics.DrawLineCallCount);
            Assert.AreEqual(1, mockGraphics.DrawTextCallCount);
        }


        [TestMethod]
        public void DecisionShape_Draw_ShouldCallCorrectGraphicsMethods()
        {
            // Arrange
            var mockGraphics = new MockGraphics();
            var shape = new DecisionShape
            {
                PositionX = 10,
                PositionY = 20,
                Width = 100,
                Height = 50,
                Text = "Decision"
            };

            // Mock parameters for MyDrawingPresenter
            var mockModel = new MyDrawingModel(); // 或使用 Mock 物件
            Action<Cursor> mockAction = cursor => { /* No operation */ };

            // Initialize presenter with mocked parameters
            var mockPresenter = new MyDrawingPresenter(mockModel, mockAction);
            shape.SetPresenter(mockPresenter);

            // Act
            shape.Draw(mockGraphics);

            // Assert
            Assert.AreEqual(4, mockGraphics.DrawLineCallCount);
            Assert.AreEqual(1, mockGraphics.DrawTextCallCount);
        }

        [TestMethod]
        public void GetShapeType_ReturnsCorrectTypeForEachShape()
        {
            // Arrange
            IShape startShape = new StartShape();
            IShape terminatorShape = new TerminatorShape();
            IShape processShape = new ProcessShape();
            IShape decisionShape = new DecisionShape();

            // Assert
            Assert.AreEqual("Start", startShape.GetShapeType());
            Assert.AreEqual("Terminator", terminatorShape.GetShapeType());
            Assert.AreEqual("Process", processShape.GetShapeType());
            Assert.AreEqual("Decision", decisionShape.GetShapeType());
        }
    }

    [TestClass]
    public class MockGraphicsTests
    {
        [TestMethod]
        public void DrawTextBorder_ShouldIncrementCallCount()
        {
            // Arrange
            var mockGraphics = new MockGraphics();

            // Act
            mockGraphics.DrawTextBorder("Test", 0, 0, 100, 50);

            // Assert
            Assert.AreEqual(1, mockGraphics.DrawTextBorderCallCount);
        }
    }


}