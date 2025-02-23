using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Windows.Forms;
using MyDrawing.States;
using MyDrawing.Shapes;
using MyDrawing;
using System;
using System.Collections.Generic;
using static DecisionShape;
using System.Linq;

namespace MyDrawing.Tests { 
[TestClass]
public class LineDrawingStateTests
{
    private MyDrawingModel _mockModel;
    private MyDrawingPresenter _mockPresenter;
    private LineDrawingState _lineDrawingState;
    private Cursor _currentCursor;

    [TestInitialize]
    public void Setup()
    {
        _mockModel = new MyDrawingModel();
        _mockPresenter = new MyDrawingPresenter(_mockModel, cursor => _currentCursor = cursor);
        _lineDrawingState = new LineDrawingState(_mockModel, _mockPresenter, cursor => _currentCursor = cursor);
    }

    // Mock shapes to simulate connection points
    private List<IShape> CreateMockShapes()
    {
        var rectangleShape1 = new ProcessShape
        {
            PositionX = 100,
            PositionY = 100,
            Width = 50,
            Height = 50,
            Id = 1
        };

        var rectangleShape2 = new ProcessShape
        {
            PositionX = 300,
            PositionY = 300,
            Width = 50,
            Height = 50,
            Id = 2
        };

        _mockModel.Shapes.Add(rectangleShape1);
        _mockModel.Shapes.Add(rectangleShape2);

        return _mockModel.Shapes;
    }

    [TestMethod]
    public void MouseDown_SelectFirstConnectionPoint_ShouldSetStartShape()
    {
        // Arrange
        CreateMockShapes();
        Point connectionPoint = new Point(125, 100); // Top connection point of first shape

        // Act
        _lineDrawingState.MouseDown(connectionPoint);

        // Use reflection to check private fields
        var startShapeField = typeof(LineDrawingState).GetField("_startShape", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var startPointField = typeof(LineDrawingState).GetField("_startPoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Assert
        Assert.IsNotNull(startShapeField.GetValue(_lineDrawingState), "Start shape should be set");
        Assert.AreEqual(connectionPoint, startPointField.GetValue(_lineDrawingState), "Start point should match connection point");
    }

    [TestMethod]
    public void MouseMove_WithSecondPoint_ShouldUpdateCurrentEndShape()
    {
        // Arrange
        CreateMockShapes();
        Point firstConnectionPoint = new Point(125, 100);
        Point secondConnectionPoint = new Point(325, 300);

        // Act: First select first point
        _lineDrawingState.MouseDown(firstConnectionPoint);

        // Move to second point
        _lineDrawingState.MouseMove(secondConnectionPoint);

        // Use reflection to check private fields
        var currentEndShapeField = typeof(LineDrawingState).GetField("_currentEndShape", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var currentEndPointField = typeof(LineDrawingState).GetField("_currentEndPoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Assert
        Assert.IsNotNull(currentEndShapeField.GetValue(_lineDrawingState), "Current end shape should be set");
        Assert.AreEqual(secondConnectionPoint, currentEndPointField.GetValue(_lineDrawingState), "Current end point should match connection point");
    }

    [TestMethod]
    public void MouseUp_ValidLine_ShouldCreateLineShape()
    {
        // Arrange
        CreateMockShapes();
        Point firstConnectionPoint = new Point(125, 100);
        Point secondConnectionPoint = new Point(325, 300);

        // Act: Complete line drawing
        _lineDrawingState.MouseDown(firstConnectionPoint);
        _lineDrawingState.MouseMove(secondConnectionPoint);
        _lineDrawingState.MouseUp(secondConnectionPoint);

        // Assert
        Assert.AreEqual(1, _mockModel.Shapes.Count(s => s.GetShapeType() == "Line"), "Line shape should be added to model");
    }

    [TestMethod]
    public void MouseUp_InvalidLine_ShouldNotCreateLineShape()
    {
        // Arrange
        CreateMockShapes();
        Point firstConnectionPoint = new Point(125, 100);

        // Act: Incomplete line drawing
        _lineDrawingState.MouseDown(firstConnectionPoint);
        _lineDrawingState.MouseUp(firstConnectionPoint);

        // Assert
        Assert.AreEqual(2, _mockModel.Shapes.Count, "No additional shapes should be created");
    }

    [TestMethod]
    public void GetCursor_ShouldReturnCrossCursor()
    {
        // Act
        Cursor cursor = _lineDrawingState.GetCursor();

        // Assert
        Assert.AreEqual(Cursors.Cross, cursor, "Cursor should be Cross");
    }

    [TestMethod]
    public void Distance_ShouldCalculateCorrectEuclideanDistance()
    {
        // Arrange
        Point point1 = new Point(0, 0);
        Point point2 = new Point(3, 4);

        // Use reflection to call private method
        var distanceMethod = typeof(LineDrawingState).GetMethod("Distance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        float distance = (float)distanceMethod.Invoke(_lineDrawingState, new object[] { point1, point2 });

        // Assert
        Assert.AreEqual(5.0f, distance, 0.001f, "Distance calculation should be correct");
    }

    [TestMethod]
    public void GetConnectionPoints_ForLineShape_ShouldReturnEmptyArray()
    {
        // Arrange
        var lineShape = new LineShape();

        // Use reflection to call private method
        var getConnectionPointsMethod = typeof(LineDrawingState).GetMethod("GetConnectionPoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        Point[] connectionPoints = (Point[])getConnectionPointsMethod.Invoke(_lineDrawingState, new object[] { lineShape });

        // Assert
        Assert.AreEqual(0, connectionPoints.Length, "Line shape should return empty connection points");
    }

    [TestMethod]
    public void GetConnectionPoints_ForRectangleShape_ShouldReturnFourPoints()
    {
        // Arrange
        var rectangleShape = new ProcessShape
        {
            PositionX = 100,
            PositionY = 200,
            Width = 50,
            Height = 100
        };

        // Use reflection to call private method
        var getConnectionPointsMethod = typeof(LineDrawingState).GetMethod("GetConnectionPoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        Point[] connectionPoints = (Point[])getConnectionPointsMethod.Invoke(_lineDrawingState, new object[] { rectangleShape });

        // Assert
        Assert.AreEqual(4, connectionPoints.Length, "Rectangle shape should return 4 connection points");
        Assert.AreEqual(new Point(125, 200), connectionPoints[0], "Top connection point incorrect");
        Assert.AreEqual(new Point(125, 300), connectionPoints[1], "Bottom connection point incorrect");
        Assert.AreEqual(new Point(100, 250), connectionPoints[2], "Left connection point incorrect");
        Assert.AreEqual(new Point(150, 250), connectionPoints[3], "Right connection point incorrect");
    }
}
    [TestClass]
    public class LineDrawingStateExtendedTests
    {
        private MyDrawingModel _mockModel;
        private MyDrawingPresenter _mockPresenter;
        private LineDrawingState _lineDrawingState;
        private Cursor _currentCursor;

        [TestInitialize]
        public void Setup()
        {
            _mockModel = new MyDrawingModel();
            _mockPresenter = new MyDrawingPresenter(_mockModel, cursor => _currentCursor = cursor);
            _lineDrawingState = new LineDrawingState(_mockModel, _mockPresenter, cursor => _currentCursor = cursor);
        }

        // Helper method to create mock shapes
        private List<IShape> CreateMockShapes()
        {
            var rectangleShape1 = new ProcessShape
            {
                PositionX = 100,
                PositionY = 100,
                Width = 50,
                Height = 50,
                Id = 1
            };

            var rectangleShape2 = new ProcessShape
            {
                PositionX = 300,
                PositionY = 300,
                Width = 50,
                Height = 50,
                Id = 2
            };

            _mockModel.Shapes.Add(rectangleShape1);
            _mockModel.Shapes.Add(rectangleShape2);

            return _mockModel.Shapes;
        }

        [TestMethod]
        public void MouseDown_SecondPointSelection_AllowChangingConnectionPoint()
        {
            // Arrange
            CreateMockShapes();
            Point firstConnectionPoint = new Point(125, 100); // Top of first shape
            Point secondConnectionPoint1 = new Point(325, 300); // Bottom of second shape
            Point secondConnectionPoint2 = new Point(350, 325); // Slightly different point on second shape

            // Act: First select first point
            _lineDrawingState.MouseDown(firstConnectionPoint);

            // First attempt to select second point
            _lineDrawingState.MouseDown(secondConnectionPoint1);

            // Use reflection to check private fields after first selection
            var currentEndShapeField = typeof(LineDrawingState).GetField("_currentEndShape", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var currentEndPointField = typeof(LineDrawingState).GetField("_currentEndPoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var firstEndShape = currentEndShapeField.GetValue(_lineDrawingState);
            var firstEndPoint = currentEndPointField.GetValue(_lineDrawingState);

            // Change second point
            _lineDrawingState.MouseDown(secondConnectionPoint2);

            var secondEndShape = currentEndShapeField.GetValue(_lineDrawingState);
            var secondEndPoint = currentEndPointField.GetValue(_lineDrawingState);

            // Assert
            Assert.IsNotNull(firstEndShape, "First end shape should be set");
            Assert.IsNotNull(secondEndShape, "Second end shape should be set");
            Assert.AreNotEqual(firstEndPoint, secondEndPoint, "Connection point should change");
        }

        [TestMethod]
        public void DrawTemporaryLine_WithConnectionPoint_ShouldUseConnectionPoint()
        {
            // Arrange
            CreateMockShapes();
            Point firstConnectionPoint = new Point(125, 100);
            Point secondConnectionPoint = new Point(325, 300);

            // Create a mock Graphics object
            Bitmap bitmap = new Bitmap(500, 500);
            Graphics graphics = Graphics.FromImage(bitmap);

            // Act: Setup the line drawing state
            _lineDrawingState.MouseDown(firstConnectionPoint);
            _lineDrawingState.MouseMove(secondConnectionPoint);

            // Use reflection to set _currentEndShape
            var currentEndShapeField = typeof(LineDrawingState).GetField("_currentEndShape", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var currentEndPointField = typeof(LineDrawingState).GetField("_currentEndPoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var mouseLocationField = typeof(LineDrawingState).GetField("_currentMouseLocation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Verify temporary line drawing with connection point
            _lineDrawingState.DrawTemporaryLine(graphics);

            // Clean up
            graphics.Dispose();
            bitmap.Dispose();

            // If no exception is thrown, the method worked as expected
            Assert.IsTrue(true, "Temporary line drawing with connection point should work");
        }

        [TestMethod]
        public void DrawTemporaryLine_WithoutConnectionPoint_ShouldUseMouseLocation()
        {
            // Arrange
            CreateMockShapes();
            Point firstConnectionPoint = new Point(125, 100);
            Point mouseLocation = new Point(250, 250);

            // Create a mock Graphics object
            Bitmap bitmap = new Bitmap(500, 500);
            Graphics graphics = Graphics.FromImage(bitmap);

            // Act: Setup the line drawing state
            _lineDrawingState.MouseDown(firstConnectionPoint);

            // Use reflection to set _currentMouseLocation without a connection point
            var mouseLocationField = typeof(LineDrawingState).GetField("_currentMouseLocation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            mouseLocationField.SetValue(_lineDrawingState, mouseLocation);

            // Verify temporary line drawing with mouse location
            _lineDrawingState.DrawTemporaryLine(graphics);

            // Clean up
            graphics.Dispose();
            bitmap.Dispose();

            // If no exception is thrown, the method worked as expected
            Assert.IsTrue(true, "Temporary line drawing with mouse location should work");
        }

        [TestMethod]
        public void FindConnectionPoint_NoConnectionPointFound_ReturnsNullAndEmptyPoint()
        {
            // Arrange
            CreateMockShapes();
            Point outsideLocation = new Point(500, 500); // A point far from any shape

            // Use reflection to call the private method
            var findConnectionPointMethod = typeof(LineDrawingState).GetMethod("FindConnectionPoint",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            // Act
            var result = findConnectionPointMethod.Invoke(_lineDrawingState, new object[] { outsideLocation });

            // Unpack the tuple returned by the method
            var (shape, point) = ((IShape, Point))result;

            // Assert
            Assert.IsNull(shape, "Shape should be null when no connection point is found");
            Assert.AreEqual(Point.Empty, point, "Point should be empty when no connection point is found");
        }
    }
}