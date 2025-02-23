using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using MyDrawing.Shapes;
using System;
using MyDrawing;

namespace MyDrawing.Shapes
{
    public abstract class Shape : IShape
    {
        private MyDrawingPresenter _presenter;

        public int Id { get; set; }
        public string Text { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public float TextPositionX { get; set; }
        public float TextPositionY { get; set; }

        public PointF OrangeDotPosition { get; set; }

        public MyDrawingPresenter Presenter => _presenter;

        public bool IsSelected => _presenter?.SelectedShape == this;

        public void SetPresenter(MyDrawingPresenter presenter)
        {
            _presenter = presenter;
        }

        public abstract void Draw(IGraphics graphics);
        public abstract string GetShapeType();

        protected void DrawCenteredTextWithConditionalBorder(IGraphics graphics, bool drawBorder)
        {
            graphics.DrawText(Text, TextPositionX, TextPositionY, Width, Height);
            if (_presenter != null && drawBorder)
            {
                graphics.DrawTextBorder(Text, TextPositionX, TextPositionY);
            }
        }
    }


    public class StartShape : Shape
    {
        public override void Draw(IGraphics graphics)
        {
            graphics.DrawEllipse(PositionX, PositionY, Width, Height);
            DrawCenteredTextWithConditionalBorder(graphics, IsSelected);
            UpdateOrangeDotPosition();
        }


        public override string GetShapeType()
        {
            return "Start";
        }

        private void UpdateOrangeDotPosition()
        {
            if (string.IsNullOrEmpty(Text)) return; using (Font font = new Font("Arial", 10)) using (Graphics tempGraphics = Graphics.FromHwnd(IntPtr.Zero))
            { // 計算文字框的大小
                SizeF textSize = tempGraphics.MeasureString(Text, font);
                // 橘色點的位置：文字框的水平中心，向上偏移8px
                OrangeDotPosition = new PointF(

                    TextPositionX + (textSize.Width / 2) - 4,
                    // 調整橘點的水平偏移量，以便居中顯示
                    TextPositionY - 8
                    // 調整橘點的垂直偏移量，以便在矩形上方顯示
                    );
            }
        }
    }


    public class TerminatorShape : Shape
    {
        public override void Draw(IGraphics graphics)
        {
            // Calculate dimensions
            float arcHeight = Height;
            float arcWidth = Height; // Use height for both dimensions to maintain circular arcs
            float straightLineLength = Width - Height;

            if (straightLineLength < 0)
            {
                // If width is less than height, adjust the arc dimensions
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

            DrawCenteredTextWithConditionalBorder(graphics, IsSelected);

            UpdateOrangeDotPosition();
        }

        public override string GetShapeType()
        {
            return "Terminator";
        }

        private void UpdateOrangeDotPosition()
        {
            if (string.IsNullOrEmpty(Text)) return; using (Font font = new Font("Arial", 10)) using (Graphics tempGraphics = Graphics.FromHwnd(IntPtr.Zero))
            { // 計算文字框的大小
                SizeF textSize = tempGraphics.MeasureString(Text, font);
                // 橘色點的位置：文字框的水平中心，向上偏移8px
                OrangeDotPosition = new PointF(

                    TextPositionX + (textSize.Width / 2) - 4,
                    // 調整橘點的水平偏移量，以便居中顯示
                    TextPositionY - 8
                    // 調整橘點的垂直偏移量，以便在矩形上方顯示
                    );
            }
        }
    }
}

public class ProcessShape : Shape
{
    public override void Draw(IGraphics graphics)
    {
        graphics.DrawRectangle(PositionX, PositionY, Width, Height);
        // 根據 Presenter 的選擇狀態繪製文字外框
        DrawCenteredTextWithConditionalBorder(graphics, IsSelected);

        UpdateOrangeDotPosition();
    }

    public override string GetShapeType()
    {
        return "Process";
    }

    private void UpdateOrangeDotPosition()
    {
        if (string.IsNullOrEmpty(Text)) return; using (Font font = new Font("Arial", 10)) using (Graphics tempGraphics = Graphics.FromHwnd(IntPtr.Zero))
        { // 計算文字框的大小
            SizeF textSize = tempGraphics.MeasureString(Text, font);
            // 橘色點的位置：文字框的水平中心，向上偏移8px
            OrangeDotPosition = new PointF(

                TextPositionX + (textSize.Width / 2) - 4,
                // 調整橘點的水平偏移量，以便居中顯示
                TextPositionY - 8
                // 調整橘點的垂直偏移量，以便在矩形上方顯示
                );
        }
    }
}


public class DecisionShape : Shape
{
    public override void Draw(IGraphics graphics)
    {
        // Draw diamond shape
        float midX = PositionX + Width / 2;
        float midY = PositionY + Height / 2;

        graphics.DrawLine(midX, PositionY, PositionX + Width, midY);
        graphics.DrawLine(PositionX + Width, midY, midX, PositionY + Height);
        graphics.DrawLine(midX, PositionY + Height, PositionX, midY);
        graphics.DrawLine(PositionX, midY, midX, PositionY);

        // 根據 Presenter 的選擇狀態繪製文字外框
        DrawCenteredTextWithConditionalBorder(graphics, IsSelected);

        UpdateOrangeDotPosition();
    }

    public override string GetShapeType()
    {
        return "Decision";
    }

    private void UpdateOrangeDotPosition()
    {
        if (string.IsNullOrEmpty(Text)) return; using (Font font = new Font("Arial", 10)) using (Graphics tempGraphics = Graphics.FromHwnd(IntPtr.Zero))
        { // 計算文字框的大小
            SizeF textSize = tempGraphics.MeasureString(Text, font);
            // 橘色點的位置：文字框的水平中心，向上偏移8px
            OrangeDotPosition = new PointF(

                TextPositionX + (textSize.Width / 2) - 4,
                // 調整橘點的水平偏移量，以便居中顯示
                TextPositionY - 8
                // 調整橘點的垂直偏移量，以便在矩形上方顯示
                );
        }
    }

    public class LineShape : Shape
    {
        public IShape StartShape { get; set; }
        public IShape EndShape { get; set; }
        public PointF StartPoint { get; set; }
        public PointF EndPoint { get; set; }

        public override void Draw(IGraphics graphics)
        {
            //Console.WriteLine($"Line Draw");
            // 繪製直線
            graphics.DrawLine(StartPoint.X, StartPoint.Y, EndPoint.X, EndPoint.Y);
        }

        public override string GetShapeType()
        {
            return "Line";
        }
    }
}

