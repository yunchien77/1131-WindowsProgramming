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
        // 確保寬度和高度至少為1
        width = System.Math.Max(1, width);
        height = System.Math.Max(1, height);

        using (var brush = new SolidBrush(Color.Black))
        {
            var size = _graphics.MeasureString(text, _font);
            float centerX = x + (width - size.Width) / 2;
            float centerY = y + (height - size.Height) / 2;
            _graphics.DrawString(text, _font, brush, centerX, centerY);
        }
    }
}