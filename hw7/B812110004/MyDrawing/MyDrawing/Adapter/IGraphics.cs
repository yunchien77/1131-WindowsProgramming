public interface IGraphics
{
    void DrawLine(float x1, float y1, float x2, float y2);
    void DrawRectangle(float x, float y, float width, float height);
    void DrawArc(float x, float y, float width, float height, float startAngle, float sweepAngle);
    void DrawEllipse(float x, float y, float width, float height);
    void DrawText(string text, float x, float y, float width, float height);

    void DrawTextBorder(string text, float x, float y);
}