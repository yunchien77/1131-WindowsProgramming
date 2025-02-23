//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Drawing;
//using System.Linq;

//namespace MyDrawing.Tests
//{
//    [TestClass]
//    public class ModelTests
//    {
//        private MyDrawingModel _model;

//        [TestInitialize]
//        public void Setup()
//        {
//            _model = new MyDrawingModel();
//        }

//        [TestMethod]
//        public void GenerateRandomText_ShouldProduceDifferentTexts()
//        {
//            // Act
//            string text1 = _model.GenerateRandomText();
//            string text2 = _model.GenerateRandomText();

//            // Assert
//            Assert.IsTrue(text1.Length >= 3 && text1.Length <= 10);
//            Assert.IsTrue(text2.Length >= 3 && text2.Length <= 10);
//            Assert.AreNotEqual(text1, text2);
//        }

//    }


//}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using MyDrawing.Shapes;
using MyDrawing.Factories;
using static DecisionShape;
using System.Threading.Tasks;

namespace MyDrawing.Tests
{
    [TestClass]
    public class ModelTests
    {
        private MyDrawingModel _model;
        private string _testFilePath;

        [TestInitialize]
        public void Setup()
        {
            _model = new MyDrawingModel();
            _testFilePath = Path.Combine(Path.GetTempPath(), "test_shapes.bin");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        [TestMethod]
        public void GenerateRandomText_ShouldProduceDifferentTexts()
        {
            // Act
            string text1 = _model.GenerateRandomText();
            string text2 = _model.GenerateRandomText();

            // Assert
            Assert.IsTrue(text1.Length >= 3 && text1.Length <= 10);
            Assert.IsTrue(text2.Length >= 3 && text2.Length <= 10);
            Assert.AreNotEqual(text1, text2);
        }

        [TestMethod]
        public async Task SaveAndLoadShapes_ShouldPreserveShapeData()
        {
            // Arrange
            var shape1 = ShapeFactory.CreateShape("Decision");
            shape1.Id = 1;
            shape1.Text = "Test Shape 1";
            shape1.PositionX = 100;
            shape1.PositionY = 100;
            shape1.Width = 150;
            shape1.Height = 80;

            var shape2 = ShapeFactory.CreateShape("Process");
            shape2.Id = 2;
            shape2.Text = "Test Shape 2";
            shape2.PositionX = 300;
            shape2.PositionY = 200;
            shape2.Width = 120;
            shape2.Height = 60;

            _model.Shapes.Add(shape1);
            _model.Shapes.Add(shape2);

            // Act
            await _model.SaveShapesAsync(_testFilePath);
            _model.LoadShapes(_testFilePath);

            // Assert
            Assert.AreEqual(2, _model.Shapes.Count);
            var loadedShape1 = _model.Shapes.First(s => s.Id == 1);
            var loadedShape2 = _model.Shapes.First(s => s.Id == 2);

            Assert.AreEqual("Test Shape 1", loadedShape1.Text);
            Assert.AreEqual(100, loadedShape1.PositionX);
            Assert.AreEqual(100, loadedShape1.PositionY);
            Assert.AreEqual(150, loadedShape1.Width);
            Assert.AreEqual(80, loadedShape1.Height);

            Assert.AreEqual("Test Shape 2", loadedShape2.Text);
            Assert.AreEqual(300, loadedShape2.PositionX);
            Assert.AreEqual(200, loadedShape2.PositionY);
            Assert.AreEqual(120, loadedShape2.Width);
            Assert.AreEqual(60, loadedShape2.Height);
        }

        [TestMethod]
        public void ReplaceShapes_ShouldReplaceExistingShapes()
        {
            // Arrange
            var originalShape = ShapeFactory.CreateShape("Decision");
            originalShape.Id = 1;
            originalShape.Text = "Original Shape";
            _model.Shapes.Add(originalShape);

            var newShapes = new List<IShape>
            {
                ShapeFactory.CreateShape("Process"),
                ShapeFactory.CreateShape("Decision")
            };
            newShapes[0].Id = 2;
            newShapes[0].Text = "New Shape 1";
            newShapes[1].Id = 3;
            newShapes[1].Text = "New Shape 2";

            // Act
            _model.ReplaceShapes(newShapes);

            // Assert
            Assert.AreEqual(2, _model.Shapes.Count);
            Assert.IsFalse(_model.Shapes.Any(s => s.Id == 1));
            Assert.IsTrue(_model.Shapes.Any(s => s.Id == 2));
            Assert.IsTrue(_model.Shapes.Any(s => s.Id == 3));
            Assert.AreEqual("New Shape 1", _model.Shapes.First(s => s.Id == 2).Text);
            Assert.AreEqual("New Shape 2", _model.Shapes.First(s => s.Id == 3).Text);
        }

        [TestMethod]
        public async Task LoadShapes_WithLineConnections_ShouldPreserveConnections()
        {
            // Arrange
            var shape1 = ShapeFactory.CreateShape("Decision");
            shape1.Id = 1;
            var shape2 = ShapeFactory.CreateShape("Process");
            shape2.Id = 2;

            var line = ShapeFactory.CreateShape("Line") as LineShape;
            line.Id = 3;
            line.StartShape = shape1;
            line.EndShape = shape2;
            line.StartPoint = new PointF(100, 100);
            line.EndPoint = new PointF(200, 200);

            _model.Shapes.Add(shape1);
            _model.Shapes.Add(shape2);
            _model.Shapes.Add(line);

            // Act
            await _model.SaveShapesAsync(_testFilePath);
            _model.LoadShapes(_testFilePath);

            // Assert
            Assert.AreEqual(3, _model.Shapes.Count);
            var loadedLine = _model.Shapes.OfType<LineShape>().First();
            Assert.IsNotNull(loadedLine.StartShape);
            Assert.IsNotNull(loadedLine.EndShape);
            Assert.AreEqual(1, loadedLine.StartShape.Id);
            Assert.AreEqual(2, loadedLine.EndShape.Id);
            Assert.AreEqual(100, loadedLine.StartPoint.X);
            Assert.AreEqual(100, loadedLine.StartPoint.Y);
            Assert.AreEqual(200, loadedLine.EndPoint.X);
            Assert.AreEqual(200, loadedLine.EndPoint.Y);
        }
    }
}