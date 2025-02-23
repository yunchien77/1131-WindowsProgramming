public interface IGraphics
{
    void DrawLine(int x1, int y1, int x2, int y2);
    void DrawRectangle(int x, int y, int width, int height);
    void DrawArc(int x, int y, int width, int height, int startAngle, int sweepAngle);
    void DrawEllipse(int x, int y, int width, int height);
    void DrawText(string text, int x, int y, int width, int height);

    void DrawTextBorder(string text, int x, int y, int width, int height);
}