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
    }


}