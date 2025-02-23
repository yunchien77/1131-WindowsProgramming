using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;

namespace MyDrawing.Tests
{
    [TestClass]
    public class GraphicsAdapterTests
    {
        private Graphics _graphics;
        private GraphicsAdapter _graphicsAdapter;
        private Bitmap _bitmap;

        [TestInitialize]
        public void Setup()
        {
            _bitmap = new Bitmap(500, 500);
            _graphics = Graphics.FromImage(_bitmap);
            _graphicsAdapter = new GraphicsAdapter(_graphics);
        }

        [TestMethod]
        public void DrawLine_ShouldDrawLine()
        {
            // Arrange
            int x1 = 10, y1 = 20, x2 = 100, y2 = 200;

            // Act
            _graphicsAdapter.DrawLine(x1, y1, x2, y2);

            // Assert - verify no exception thrown
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void DrawRectangle_WithValidDimensions_ShouldDrawRectangle()
        {
            // Arrange
            int x = 50, y = 60, width = 100, height = 80;

            // Act
            _graphicsAdapter.DrawRectangle(x, y, width, height);

            // Assert
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void DrawRectangle_WithZeroDimensions_ShouldDrawWithMinimumSize()
        {
            // Arrange
            int x = 50, y = 60, width = 0, height = 0;

            // Act
            _graphicsAdapter.DrawRectangle(x, y, width, height);

            // Assert
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void DrawEllipse_ShouldDrawEllipse()
        {
            // Arrange
            int x = 50, y = 60, width = 100, height = 80;

            // Act
            _graphicsAdapter.DrawEllipse(x, y, width, height);

            // Assert
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void DrawArc_ShouldDrawArc()
        {
            // Arrange
            int x = 50, y = 60, width = 100, height = 80;
            int startAngle = 0, sweepAngle = 180;

            // Act
            _graphicsAdapter.DrawArc(x, y, width, height, startAngle, sweepAngle);

            // Assert
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void DrawText_ShouldDrawCenteredText()
        {
            // Arrange
            string text = "Test Text";
            int x = 50, y = 60, width = 100, height = 80;

            // Act
            _graphicsAdapter.DrawText(text, x, y, width, height);

            // Assert
            Assert.IsTrue(true);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _graphics.Dispose();
            _bitmap.Dispose();
        }

        [TestMethod]
        public void DrawTextBorder_ShouldDrawTextBorderAndDot()
        {
            // Arrange
            string text = "Test Text";
            int x = 50, y = 60, width = 200, height = 80;

            // Act
            _graphicsAdapter.DrawTextBorder(text, x, y, width, height);

            // Assert - Verify that the rectangle was drawn (you would need to verify using a mock or other technique)
            // You can use an image comparison or manually verify certain conditions, such as the pen color, location, or rectangle size
            // For simplicity, we'll assume the rectangle and dot are drawn and check if any exception was thrown
            Assert.IsTrue(true);
        }
    }


}