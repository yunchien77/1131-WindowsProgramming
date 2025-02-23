using System.Drawing;
using System.Windows.Forms;

namespace MyDrawing.States
{
    public interface IDrawingState
    {
        void MouseDown(Point location);
        void MouseMove(Point location);
        void MouseUp(Point location);
        Cursor GetCursor();
    }
}