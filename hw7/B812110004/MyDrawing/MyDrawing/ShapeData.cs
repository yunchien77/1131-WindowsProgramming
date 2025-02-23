using System;
using System.Drawing;

namespace MyDrawing
{
    [Serializable]
    public class ShapeData
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float Height { get; set; }
        public float Width { get; set; }
        public float TextPositionX { get; set; }
        public float TextPositionY { get; set; }

        // For LineShape
        public int? StartShapeId { get; set; }
        public int? EndShapeId { get; set; }
        public PointF StartPoint { get; set; }
        public PointF EndPoint { get; set; }
    }
}