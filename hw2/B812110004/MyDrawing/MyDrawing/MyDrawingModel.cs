using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDrawing
{
    public class MyDrawingModel
    {
        public abstract class Shape
        {
            public int Id { get; set; }
            public string Text { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Height { get; set; }
            public int Width { get; set; }

            public abstract string GetShapeType();
        }

        public class Start : Shape
        {
            public override string GetShapeType() => "Start";
        }

        public class Terminator : Shape
        {
            public override string GetShapeType() => "Terminator";
        }

        public class Process : Shape
        {
            public override string GetShapeType() => "Process";
        }

        public class Decision : Shape
        {
            public override string GetShapeType() => "Decision";
        }

        public class ShapeFactory
        {
            public static Shape CreateShape(string shapeType)
            {
                switch (shapeType)
                {
                    case "Start":
                        return new Start();
                    case "Terminator":
                        return new Terminator();
                    case "Process":
                        return new Process();
                    case "Decision":
                        return new Decision();
                    default:
                        throw new ArgumentException("Invalid shape type");
                }
            }
        }

        // store all the shapes
        private List<Shape> shapes = new List<Shape>();
        private int nextId = 1;

        public event EventHandler ShapesChanged;

        // add a new shape to "shapes" list
        public void AddShape(string shapeType, string text, int x, int y, int height, int width)
        {
            Shape shape = ShapeFactory.CreateShape(shapeType);
            shape.Id = nextId++;
            shape.Text = text;
            shape.X = x;
            shape.Y = y;
            shape.Height = height;
            shape.Width = width;
            shapes.Add(shape);
            OnShapesChanged();
        }

        // removes a shape from the list based on its id
        public void DeleteShape(int id)
        {
            shapes.RemoveAll(s => s.Id == id);
            OnShapesChanged();
        }

        // returns a copy of the shapes list
        public List<Shape> GetShapes()
        {
            return new List<Shape>(shapes);
        }

        protected virtual void OnShapesChanged()
        {
            ShapesChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
