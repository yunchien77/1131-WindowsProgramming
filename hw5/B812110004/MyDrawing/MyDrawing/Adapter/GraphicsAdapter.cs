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

    public void DrawLine(int x1, int y1, int x2, int y2)
    {
        _graphics.DrawLine(Pens.Black, x1, y1, x2, y2);
    }

    public void DrawRectangle(int x, int y, int width, int height)
    {
        // 確保寬度和高度至少為1
        width = System.Math.Max(1, width);
        height = System.Math.Max(1, height);

        _graphics.DrawRectangle(Pens.Black, x, y, width, height);
    }

    public void DrawEllipse(int x, int y, int width, int height)
    {
        // 確保寬度和高度至少為1
        width = System.Math.Max(1, width);
        height = System.Math.Max(1, height);

        _graphics.DrawEllipse(Pens.Black, x, y, width, height);
    }

    public void DrawArc(int x, int y, int width, int height, int startAngle, int sweepAngle)
    {
        // 確保寬度和高度至少為1
        width = System.Math.Max(1, width);
        height = System.Math.Max(1, height);

        _graphics.DrawArc(Pens.Black, x, y, width, height, startAngle, sweepAngle);
    }

    public void DrawText(string text, int x, int y, int width, int height)
    {
        var size = _graphics.MeasureString(text, _font);
        float centerX = x + (width - size.Width) / 2;
        float centerY = y + (height - size.Height) / 2;

        using (var brush = new SolidBrush(Color.Black))
        {
            _graphics.DrawString(text, _font, brush, centerX, centerY);
        }
    }

    public void DrawTextBorder(string text, int x, int y, int width, int height)
    {
        var size = _graphics.MeasureString(text, _font);
        float centerX = x + (width - size.Width) / 2;
        float centerY = y + (height - size.Height) / 2;

        RectangleF borderRect = new RectangleF(centerX, centerY, size.Width, size.Height);
        using (var pen = new Pen(Color.Red))
        {
            _graphics.DrawRectangle(pen, Rectangle.Round(borderRect));
        }

        // Draw the larger orange dot above the rectangle in the center
        using (var brush = new SolidBrush(Color.Orange))
        {
            float dotX = centerX + size.Width / 2 - 4; // Increase the offset to center the larger dot
            float dotY = centerY - 4; // Adjust for the larger size
            _graphics.FillEllipse(brush, dotX, dotY, 8, 8);  // Increase size of the dot (8x8)
        }
    }


}