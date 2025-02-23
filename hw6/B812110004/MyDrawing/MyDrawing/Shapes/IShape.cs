using System.Drawing;

namespace MyDrawing.Shapes
{
    public interface IShape
    {
        int Id { get; set; }
        string Text { get; set; }
        float PositionX { get; set; }
        float PositionY { get; set; }
        float Height { get; set; }
        float Width { get; set; }

        void Draw(IGraphics graphics);
        string GetShapeType();

        void SetPresenter(MyDrawingPresenter presenter);

        float TextPositionX { get; set; }
        float TextPositionY { get; set; }

        PointF OrangeDotPosition { get; set; }

    }
}