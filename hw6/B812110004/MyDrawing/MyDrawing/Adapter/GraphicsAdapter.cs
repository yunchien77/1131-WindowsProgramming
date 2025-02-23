using System;
using System.Drawing;

public class GraphicsAdapter : IGraphics
{
    private readonly Graphics _graphics;
    private readonly Font _font;

    public GraphicsAdapter(Graphics graphics)
    {
        _graphics = graphics;
        _font = new Font("Arial", 10);
    }

    public void DrawLine(float x1, float y1, float x2, float y2)
    {
        _graphics.DrawLine(Pens.Black, x1, y1, x2, y2);
    }

    public void DrawRectangle(float x, float y, float width, float height)
    {
        // 確保寬度和高度至少為1
        width = System.Math.Max(1, width);
        height = System.Math.Max(1, height);

        _graphics.DrawRectangle(Pens.Black, x, y, width, height);
    }

    public void DrawEllipse(float x, float y, float width, float height)
    {
        // 確保寬度和高度至少為1
        width = System.Math.Max(1, width);
        height = System.Math.Max(1, height);

        _graphics.DrawEllipse(Pens.Black, x, y, width, height);
    }

    public void DrawArc(float x, float y, float width, float height, float startAngle, float sweepAngle)
    {
        // 確保寬度和高度至少為1
        width = System.Math.Max(1, width);
        height = System.Math.Max(1, height);

        _graphics.DrawArc(Pens.Black, x, y, width, height, startAngle, sweepAngle);
    }


    public void DrawText(string text, float x, float y, float width, float height)
    {
        using (var brush = new SolidBrush(Color.Black)) { 
            _graphics.DrawString(text, _font, brush, x, y); 
        }
            
    }


    public void DrawTextBorder(string text, float x, float y)
    {
        // 根據文本計算矩形大小
        var size = _graphics.MeasureString(text, _font);

        // 繪製矩形，以 (x, y) 作為左上角起點，寬度和高度取決於文本的大小
        RectangleF borderRect = new RectangleF(x, y, size.Width, size.Height);
        using (var pen = new Pen(Color.Red))
        {
            _graphics.DrawRectangle(pen, Rectangle.Round(borderRect));
        }

        // 繪製文本，居中顯示在矩形內
        float textX = x + (borderRect.Width - size.Width) / 2;
        float textY = y + (borderRect.Height - size.Height) / 2;
        _graphics.DrawString(text, _font, Brushes.Black, textX, textY);

        // 在矩形上方中心繪製較大的橘色圓點
        using (var brush = new SolidBrush(Color.Orange))
        {
            float dotX = x + (size.Width / 2) - 4; // 調整橘點的水平偏移量，以便居中顯示
            float dotY = y - 8; // 調整橘點的垂直偏移量，以便在矩形上方顯示
            _graphics.FillEllipse(brush, dotX, dotY, 8, 8);  // 繪製8x8大小的圓點
        }
    }

}