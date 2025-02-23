using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Linq;

namespace MyDrawing.Tests
{
    [TestClass]
    public class ModelTests
    {
        private MyDrawingModel _model;

        [TestInitialize]
        public void Setup()
        {
            _model = new MyDrawingModel();
        }

        [TestMethod]
        public void AddShape_ShouldAddShapeToModel()
        {
            // Arrange
            string shapeType = "Start";
            string text = "Test Shape";
            int positionX = 10;
            int positionY = 20;
            int height = 50;
            int width = 100;

            var presenter = new MyDrawingPresenter(_model, cursor => { /* 測試時不需特別操作 */ });
            // Act
            _model.AddShape(shapeType, text, positionX, positionY, height, width, presenter);

            // Assert
            var shapes = _model.GetShapes();
            Assert.AreEqual(1, shapes.Count);
            var addedShape = shapes.First();
            Assert.AreEqual(shapeType, addedShape.GetShapeType());
            Assert.AreEqual(text, addedShape.Text);
            Assert.AreEqual(positionX, addedShape.PositionX);
            Assert.AreEqual(positionY, addedShape.PositionY);
            Assert.AreEqual(height, addedShape.Height);
            Assert.AreEqual(width, addedShape.Width);
        }

        [TestMethod]
        public void FindShapeAtPosition_ShouldReturnCorrectShape()
        {
            var presenter = new MyDrawingPresenter(_model, cursor => { /* 測試時不需特別操作 */ });

            // Arrange
            _model.AddShape("Start", "Shape1", 10, 20, 50, 100, presenter);
            _model.AddShape("Process", "Shape2", 200, 300, 75, 150, presenter);

            // Act
            var foundShape = _model.FindShapeAtPosition(new Point(50, 45));
            var notFoundShape = _model.FindShapeAtPosition(new Point(500, 500));

            // Assert
            Assert.IsNotNull(foundShape);
            Assert.AreEqual("Shape1", foundShape.Text);
            Assert.IsNull(notFoundShape);
        }

        [TestMethod]
        public void DeleteShape_ShouldRemoveShape()
        {
            // Arrange
            var presenter = new MyDrawingPresenter(_model, cursor => { /* 測試時不需特別操作 */ });
            _model.AddShape("Start", "Shape1", 10, 20, 50, 100, presenter);
            _model.AddShape("Process", "Shape2", 200, 300, 75, 150, presenter);
            var shapes = _model.GetShapes();
            int shapeIdToDelete = shapes.First().Id;

            // Act
            _model.DeleteShape(shapeIdToDelete);

            // Assert
            shapes = _model.GetShapes();
            Assert.AreEqual(1, shapes.Count);
            Assert.AreNotEqual(shapeIdToDelete, shapes.First().Id);
        }

        [TestMethod]
        public void GetShapes_ShouldReturnShapesInOrder()
        {
            // Arrange
            var presenter = new MyDrawingPresenter(_model, cursor => { /* 測試時不需特別操作 */ });
            _model.AddShape("Start", "Shape1", 10, 20, 50, 100, presenter);
            _model.AddShape("Process", "Shape2", 200, 300, 75, 150, presenter);

            // Act
            var shapes = _model.GetShapes();

            // Assert
            Assert.AreEqual(2, shapes.Count);
            Assert.AreEqual("Shape1", shapes[0].Text);
            Assert.AreEqual("Shape2", shapes[1].Text);
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
        public void OnShapesChanged_ShouldTriggerEvent()
        {
            // Arrange
            bool eventRaised = false;
            _model.ShapesChanged += (s, e) => eventRaised = true;

            // Act
            var presenter = new MyDrawingPresenter(_model, cursor => { /* 測試時不需特別操作 */ });
            _model.AddShape("Start", "Shape1", 10, 20, 50, 100, presenter);

            // Assert
            Assert.IsTrue(eventRaised);
        }
    }


}