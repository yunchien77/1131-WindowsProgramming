using MyDrawing.Shapes;
using System.Drawing;
using System;
using System.Windows.Forms;
using System.Linq;

namespace MyDrawing.States
{
    public class DrawingState : IDrawingState
    {
        private readonly MyDrawingModel _model;
        private readonly MyDrawingPresenter _presenter;
        private bool _isDrawing;
        private Point _startPoint;
        private int _tempWidth, _tempHeight;
        private Action<Cursor> _setCursor;

        public DrawingState(MyDrawingModel model, MyDrawingPresenter presenter, Action<Cursor> setCursorAction)
        {
            _model = model;
            _presenter = presenter;
            _setCursor = setCursorAction;
        }

        public void MouseDown(Point location)
        {
            _isDrawing = true;
            _startPoint = location;

            _setCursor(Cursors.Cross);
        }

        public void MouseMove(Point location)
        {
            if (_isDrawing)
            {
                _tempWidth = location.X - _startPoint.X;
                _tempHeight = location.Y - _startPoint.Y;
                _presenter.RefreshDrawingPanel();
            }
        }

        public void MouseUp(Point location)
        {
            if (_isDrawing)
            {
                int width = Math.Abs(location.X - _startPoint.X);
                int height = Math.Abs(location.Y - _startPoint.Y);
                int finalX = _startPoint.X;
                int finalY = _startPoint.Y;
                if (location.X < _startPoint.X)
                    finalX = _startPoint.X - width;
                if (location.Y < _startPoint.Y)
                    finalY = _startPoint.Y - height;

                string randomText = _model.GenerateRandomText();
                _model.AddShape(_presenter.CurrentShapeType, randomText, finalX, finalY, height, width, _presenter);
                _isDrawing = false;
                _presenter.ResetToolState();

                _setCursor(Cursors.Default);

                var newShape = _model.GetShapes().LastOrDefault();
                if (newShape != null)
                {
                    _presenter.SelectedShape = newShape;
                }
            }
        }

        public Cursor GetCursor()
        {
            return _isDrawing || _presenter.CurrentShapeType != null ? Cursors.Cross : Cursors.Default;
        }

        public (int Width, int Height, Point Location) GetTempShape()
        {
            return (_tempWidth, _tempHeight, _startPoint);
        }

        public void DrawTemporaryShape(Graphics g)
        {
            if (_isDrawing)
            {
                int width = Math.Abs(_tempWidth);
                int height = Math.Abs(_tempHeight);
                int x = _tempWidth >= 0 ? _startPoint.X : _startPoint.X + _tempWidth;
                int y = _tempHeight >= 0 ? _startPoint.Y : _startPoint.Y + _tempHeight;

                var graphics = new GraphicsAdapter(g);

                // Draw the actual shape type instead of a rectangle
                switch (_presenter.CurrentShapeType)
                {
                    case "Start":
                        graphics.DrawEllipse(x, y, width, height);
                        break;
                    case "Terminator":
                        // Terminator shape drawing logic
                        int arcHeight = height;
                        int arcWidth = height;
                        int straightLineLength = width - height;

                        if (straightLineLength < 0)
                        {
                            arcHeight = height;
                            arcWidth = width / 2;
                            straightLineLength = 0;
                        }

                        graphics.DrawArc(x, y, arcWidth, arcHeight, 90, 180);
                        graphics.DrawArc(x + straightLineLength, y, arcWidth, arcHeight, 270, 180);

                        if (straightLineLength > 0)
                        {
                            graphics.DrawLine(
                                x + arcWidth / 2,
                                y,
                                x + straightLineLength + arcWidth / 2,
                                y
                            );
                            graphics.DrawLine(
                                x + arcWidth / 2,
                                y + height,
                                x + straightLineLength + arcWidth / 2,
                                y + height
                            );
                        }
                        break;
                    case "Process":
                        graphics.DrawRectangle(x, y, width, height);
                        break;
                    case "Decision":
                        int midX = x + width / 2;
                        int midY = y + height / 2;

                        graphics.DrawLine(midX, y, x + width, midY);
                        graphics.DrawLine(x + width, midY, midX, y + height);
                        graphics.DrawLine(midX, y + height, x, midY);
                        graphics.DrawLine(x, midY, midX, y);
                        break;
                }
            }
        }

        public bool IsDrawing => _isDrawing;
    }
}