using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using MyDrawing.Shapes;
using System;

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

        protected MyDrawingPresenter _presenter;

        public int TextPositionX { get; set; }
        public int TextPositionY { get; set; }

        public PointF OrangeDotPosition { get; set; }

        public void SetPresenter(MyDrawingPresenter presenter)
        {
            _presenter = presenter;
        }

        public abstract void Draw(IGraphics graphics);
        public abstract string GetShapeType();

        protected void DrawCenteredTextWithConditionalBorder(IGraphics graphics, bool drawBorder)
        {
            int textX = TextPositionX == 0 ? PositionX : TextPositionX;
            int textY = TextPositionY == 0 ? PositionY : TextPositionY;

            graphics.DrawText(Text, textX, textY, Width, Height);

            if (drawBorder)
            {
                graphics.DrawTextBorder(Text, textX, textY, Width, Height);
            }
        }


    }




    public class StartShape : Shape
    {
        public override void Draw(IGraphics graphics)
        {
            graphics.DrawEllipse(PositionX, PositionY, Width, Height);
            // 根據 Presenter 的選擇狀態繪製文字外框
            bool isSelected = _presenter.SelectedShape == this;
            DrawCenteredTextWithConditionalBorder(graphics, isSelected);

            UpdateOrangeDotPosition();
        }


        public override string GetShapeType()
        {
            return "Start";
        }

        private void UpdateOrangeDotPosition()
        {
            if (string.IsNullOrEmpty(Text)) return;

            // 仅计算橘色点的位置，而不影响文本位置
            using (Font font = new Font("Arial", 10))
            using (Graphics tempGraphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                SizeF textSize = tempGraphics.MeasureString(Text, font);
                float centerX = PositionX + (Width - textSize.Width) / 2;
                float centerY = PositionY + (Height - textSize.Height) / 2;

                // 计算橘色点的位置
                OrangeDotPosition = new PointF(centerX + textSize.Width / 2 - 3, centerY - 3);
            }
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

            // 根據 Presenter 的選擇狀態繪製文字外框
            bool isSelected = _presenter.SelectedShape == this;
            DrawCenteredTextWithConditionalBorder(graphics, isSelected);

            UpdateOrangeDotPosition();
        }

        public override string GetShapeType()
        {
            return "Terminator";
        }

        private void UpdateOrangeDotPosition()
        {
            if (string.IsNullOrEmpty(Text)) return;

            // 仅计算橘色点的位置，而不影响文本位置
            using (Font font = new Font("Arial", 10))
            using (Graphics tempGraphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                SizeF textSize = tempGraphics.MeasureString(Text, font);
                float centerX = PositionX + (Width - textSize.Width) / 2;
                float centerY = PositionY + (Height - textSize.Height) / 2;

                // 计算橘色点的位置
                OrangeDotPosition = new PointF(centerX + textSize.Width / 2 - 3, centerY - 3);
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
        bool isSelected = _presenter.SelectedShape == this;
        DrawCenteredTextWithConditionalBorder(graphics, isSelected);

        UpdateOrangeDotPosition();
    }

    public override string GetShapeType()
    {
        return "Process";
    }

    private void UpdateOrangeDotPosition()
    {
        if (string.IsNullOrEmpty(Text)) return;

        // 仅计算橘色点的位置，而不影响文本位置
        using (Font font = new Font("Arial", 10))
        using (Graphics tempGraphics = Graphics.FromHwnd(IntPtr.Zero))
        {
            SizeF textSize = tempGraphics.MeasureString(Text, font);
            float centerX = PositionX + (Width - textSize.Width) / 2;
            float centerY = PositionY + (Height - textSize.Height) / 2;

            // 计算橘色点的位置
            OrangeDotPosition = new PointF(centerX + textSize.Width / 2 - 3, centerY - 3);
        }
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

        // 根據 Presenter 的選擇狀態繪製文字外框
        bool isSelected = _presenter.SelectedShape == this;
        DrawCenteredTextWithConditionalBorder(graphics, isSelected);

        UpdateOrangeDotPosition();
    }

    public override string GetShapeType()
    {
        return "Decision";
    }

    private void UpdateOrangeDotPosition()
    {
        if (string.IsNullOrEmpty(Text)) return;

        // 仅计算橘色点的位置，而不影响文本位置
        using (Font font = new Font("Arial", 10))
        using (Graphics tempGraphics = Graphics.FromHwnd(IntPtr.Zero))
        {
            SizeF textSize = tempGraphics.MeasureString(Text, font);
            float centerX = PositionX + (Width - textSize.Width) / 2;
            float centerY = PositionY + (Height - textSize.Height) / 2;

            // 计算橘色点的位置
            OrangeDotPosition = new PointF(centerX + textSize.Width / 2 - 3, centerY - 3);
        }
    }


}

