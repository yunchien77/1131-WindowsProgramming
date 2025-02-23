using System;
using System.Collections.Generic;
using System.Linq;
using MyDrawing.Factories;
using MyDrawing.Shapes;

namespace MyDrawing
{
    public class MyDrawingModel
    {
        private readonly List<IShape> _shapes;
        private int _nextId;
        private readonly Random _random;

        public event EventHandler ShapesChanged;

        public MyDrawingModel()
        {
            _shapes = new List<IShape>();
            _nextId = 1;
            _random = new Random();
        }

        public void AddShape(string shapeType, string text, int positionX, int positionY, int height, int width)
        {
            IShape shape = ShapeFactory.CreateShape(shapeType);
            shape.Id = _nextId++;
            shape.Text = text;
            shape.PositionX = positionX;
            shape.PositionY = positionY;
            shape.Height = height;
            shape.Width = width;

            _shapes.Add(shape);
            OnShapesChanged();
        }

        public void DeleteShape(int id)
        {
            _shapes.RemoveAll(shape => shape.Id == id);
            OnShapesChanged();
        }

        public List<IShape> GetShapes()
        {
            return new List<IShape>(_shapes);
        }

        protected virtual void OnShapesChanged()
        {
            ShapesChanged?.Invoke(this, EventArgs.Empty);
        }

        public string GenerateRandomText()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            int length = _random.Next(3, 11);
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}