using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MyDrawing.Factories;
using MyDrawing.Shapes;
using static DecisionShape;

namespace MyDrawing
{
    public class MyDrawingModel
    {
        private readonly List<IShape> _shapes;
        //private int _nextId;
        private readonly Random _random;

        public List<IShape> Shapes => _shapes;

        public event EventHandler ShapesChanged;

        public MyDrawingModel()
        {
            _shapes = new List<IShape>();
            //_nextId = 1;
            _random = new Random();
        }

        public IShape FindShapeAtPosition(Point location)
        {
            // 從列表開始處搜尋，這樣最上層的圖形會先被檢查到
            return _shapes
                .OrderByDescending(s => s.Id)
                .FirstOrDefault(shape =>
                    !(shape is LineShape) && // 過濾掉 LineShape
                    location.X >= shape.PositionX &&
                    location.X <= shape.PositionX + shape.Width &&
                    location.Y >= shape.PositionY &&
                    location.Y <= shape.PositionY + shape.Height
                );

        }

        public void DeleteShape(int id)
        {
            _shapes.RemoveAll(shape => shape.Id == id);
            OnShapesChanged();
        }

        public List<IShape> GetShapes()
        {
            return _shapes.OrderBy(s => s.Id).ToList();
        }

        public virtual void OnShapesChanged()
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