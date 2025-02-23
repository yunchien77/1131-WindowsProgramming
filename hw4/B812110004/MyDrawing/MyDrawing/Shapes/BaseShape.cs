using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using MyDrawing.Shapes;

namespace MyDrawing.Shapes
{
    public abstract class Shape : IShape
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public abstract void Draw(IGraphics graphics);
        public abstract string GetShapeType();

        protected void DrawCenteredText(IGraphics graphics)
        {
            graphics.DrawText(Text, PositionX, PositionY, Width, Height);
        }

    }




    public class StartShape : Shape
    {
        public override void Draw(IGraphics graphics)
        {
            graphics.DrawEllipse(PositionX, PositionY, Width, Height);
            DrawCenteredText(graphics);
        }

        public override string GetShapeType()
        {
            return "Start";
        }
    }


    public class TerminatorShape : Shape
    {
        public override void Draw(IGraphics graphics)
        {
            // Calculate dimensions
            int arcHeight = Height;
            int arcWidth = Height; // Use height for both dimensions to maintain circular arcs
            int straightLineLength = Width - Height;

            if (straightLineLength < 0)
            {
                // If width is less than height, adjust the arc dimensions
                arcHeight = Height;
                arcWidth = Width / 2;
                straightLineLength = 0;
            }

            // Draw left arc
            graphics.DrawArc(PositionX, PositionY, arcWidth, arcHeight, 90, 180);

            // Draw right arc
            graphics.DrawArc(PositionX + straightLineLength, PositionY, arcWidth, arcHeight, 270, 180);

            // Draw connecting lines only if there's space between arcs
            if (straightLineLength > 0)
            {
                // Top line
                graphics.DrawLine(
                    PositionX + arcWidth / 2,
                    PositionY,
                    PositionX + straightLineLength + arcWidth / 2,
                    PositionY
                );

                // Bottom line
                graphics.DrawLine(
                    PositionX + arcWidth / 2,
                    PositionY + Height,
                    PositionX + straightLineLength + arcWidth / 2,
                    PositionY + Height
                );
            }

            DrawCenteredText(graphics);
        }

        public override string GetShapeType()
        {
            return "Terminator";
        }
    }
}

public class ProcessShape : Shape
{
    public override void Draw(IGraphics graphics)
    {
        graphics.DrawRectangle(PositionX, PositionY, Width, Height);
        DrawCenteredText(graphics);
    }

    public override string GetShapeType()
    {
        return "Process";
    }
}


public class DecisionShape : Shape
{
    public override void Draw(IGraphics graphics)
    {
        // Draw diamond shape
        int midX = PositionX + Width / 2;
        int midY = PositionY + Height / 2;

        graphics.DrawLine(midX, PositionY, PositionX + Width, midY);
        graphics.DrawLine(PositionX + Width, midY, midX, PositionY + Height);
        graphics.DrawLine(midX, PositionY + Height, PositionX, midY);
        graphics.DrawLine(PositionX, midY, midX, PositionY);

        DrawCenteredText(graphics);
    }

    public override string GetShapeType()
    {
        return "Decision";
    }
}

