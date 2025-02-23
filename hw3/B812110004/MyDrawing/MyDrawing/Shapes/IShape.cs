namespace MyDrawing.Shapes
{
    public interface IShape
    {
        int Id { get; set; }
        string Text { get; set; }
        int PositionX { get; set; }
        int PositionY { get; set; }
        int Height { get; set; }
        int Width { get; set; }

        void Draw(IGraphics graphics);
        string GetShapeType();
    }
}