using MyDrawing.Shapes;
using System.Drawing;
using System;
using System.Windows.Forms;
using System.Linq;
using MyDrawing.Command;
using MyDrawing.Factories;

namespace MyDrawing.States
{
    public class DrawingState : IDrawingState
    {
        private readonly MyDrawingModel _model;
        private readonly MyDrawingPresenter _presenter;
        private bool _isDrawing;
        private Point _startPoint;
        private float _tempWidth, _tempHeight;
        private Action<Cursor> _setCursor;
        private IShape _lastCreatedShape;

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
                float width = Math.Abs(location.X - _startPoint.X);
                float height = Math.Abs(location.Y - _startPoint.Y);
                float finalX = _startPoint.X;
                float finalY = _startPoint.Y;
                if (location.X < _startPoint.X)
                    finalX = _startPoint.X - width;
                if (location.Y < _startPoint.Y)
                    finalY = _startPoint.Y - height;

                string randomText = _model.GenerateRandomText();

                // 直接創建 Shape，不使用 AddShape
                IShape newShape = ShapeFactory.CreateShape(_presenter.CurrentShapeType);
                newShape.Id = _model.Shapes.Count + 1; // 或使用其他生成 ID 的機制
                newShape.Text = randomText;
                newShape.PositionX = finalX;
                newShape.PositionY = finalY;
                newShape.Height = height;
                newShape.Width = width;
                newShape.SetPresenter(_presenter);

                // 初始化文字位置
                InitializeTextPosition(newShape);

                var drawCommand = new DrawShapeCommand(_model, newShape);
                _presenter.ExecuteCommand(drawCommand);

                _lastCreatedShape = newShape;

                _isDrawing = false;
                _presenter.ResetToolState();
                _setCursor(Cursors.Default);

                if (_lastCreatedShape != null)
                {
                    _presenter.SelectedShape = _lastCreatedShape;
                }
            }
        }

        // 新增初始化文字位置的方法
        private void InitializeTextPosition(IShape shape)
        {
            using (Font font = new Font("Arial", 10))
            using (Graphics tempGraphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                // 計算文字框的大小
                SizeF textSize = tempGraphics.MeasureString(shape.Text, font);
                // 設置文字位置為形狀中心的絕對座標
                shape.TextPositionX = shape.PositionX + (shape.Width / 2) - (textSize.Width / 2);
                shape.TextPositionY = shape.PositionY + (shape.Height / 2) - (textSize.Height / 2);
            }
        }

        public Cursor GetCursor()
        {
            return _isDrawing || _presenter.CurrentShapeType != null ? Cursors.Cross : Cursors.Default;
        }

        public (float Width, float Height, Point Location) GetTempShape()
        {
            return (_tempWidth, _tempHeight, _startPoint);
        }

        public void DrawTemporaryShape(Graphics g)
        {
            if (_isDrawing)
            {
                float width = Math.Abs(_tempWidth);
                float height = Math.Abs(_tempHeight);
                float x = _tempWidth >= 0 ? _startPoint.X : _startPoint.X + _tempWidth;
                float y = _tempHeight >= 0 ? _startPoint.Y : _startPoint.Y + _tempHeight;

                var graphics = new GraphicsAdapter(g);

                // Draw the actual shape type instead of a rectangle
                switch (_presenter.CurrentShapeType)
                {
                    case "Start":
                        graphics.DrawEllipse(x, y, width, height);
                        break;
                    case "Terminator":
                        // Terminator shape drawing logic
                        float arcHeight = height;
                        float arcWidth = height;
                        float straightLineLength = width - height;

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
                        float midX = x + width / 2;
                        float midY = y + height / 2;

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