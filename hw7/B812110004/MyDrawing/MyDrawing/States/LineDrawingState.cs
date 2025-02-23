using System;
using System.Drawing;
using System.Windows.Forms;
using MyDrawing.Shapes;
using MyDrawing.Command;
using MyDrawing.Factories;
using System.Collections.Generic;
using System.Linq;
using static DecisionShape;

namespace MyDrawing.States
{
    public class LineDrawingState : IDrawingState
    {
        private readonly MyDrawingModel _model;
        private readonly MyDrawingPresenter _presenter;
        private Action<Cursor> _setCursor;
        private IShape _startShape;
        private Point _startPoint;
        private IShape _currentEndShape;
        private Point _currentEndPoint;
        private Point _currentMouseLocation;
        private bool _isSelectingFirstPoint;
        private bool _isSelectingSecondPoint;
        private const int CONNECTION_POINT_RADIUS = 10; // Detection radius for connection points


        public LineDrawingState(MyDrawingModel model, MyDrawingPresenter presenter, Action<Cursor> setCursorAction)
        {
            _model = model;
            _presenter = presenter;
            _setCursor = setCursorAction;
            ResetState();
        }

        private void ResetState()
        {
            _startShape = null;
            _currentEndShape = null;
            _currentMouseLocation = Point.Empty;
            _isSelectingFirstPoint = true;
            _isSelectingSecondPoint = false;
            _presenter.RefreshDrawingPanel();
        }

        public void MouseDown(Point location)
        {
            var (shape, connectionPoint) = FindConnectionPoint(location);

            if (shape != null)
            {
                if (_isSelectingFirstPoint)
                {
                    _startShape = shape;
                    _startPoint = connectionPoint;
                    _isSelectingFirstPoint = false;
                    _isSelectingSecondPoint = true;
                }
                else if (_isSelectingSecondPoint)
                {
                    // 允许在第二个点选择时更换连接点
                    if (shape != _startShape || connectionPoint != _startPoint)
                    {
                        _currentEndShape = shape;
                        _currentEndPoint = connectionPoint;
                    }
                }
            }
        }

        public void MouseMove(Point location)
        {
            _currentMouseLocation = location;

            if (_isSelectingSecondPoint)
            {
                var (currentShape, connectionPoint) = FindConnectionPoint(location);

                // 更新可能的结束点
                if (currentShape != null &&
                    (currentShape != _startShape || connectionPoint != _startPoint))
                {
                    _currentEndShape = currentShape;
                    _currentEndPoint = connectionPoint;
                }

                _presenter.RefreshDrawingPanel();
            }
        }

        public void MouseUp(Point location)
        {
            // 檢查是否可以創建有效的線條
            if (_startShape != null && _currentEndShape != null &&
                (_currentEndShape != _startShape || _currentEndPoint != _startPoint))
            {
                LineShape line = (LineShape)ShapeFactory.CreateShape("Line");
                line.StartShape = _startShape;
                line.EndShape = _currentEndShape;
                line.StartPoint = _startPoint;
                line.EndPoint = _currentEndPoint;

                // 計算線段的位置、長寬
                line.PositionX = Math.Min(_startPoint.X, _currentEndPoint.X);
                line.PositionY = Math.Min(_startPoint.Y, _currentEndPoint.Y);
                line.Width = Math.Abs(_startPoint.X - _currentEndPoint.X);
                line.Height = Math.Abs(_startPoint.Y - _currentEndPoint.Y);

                line.Id = _model.Shapes.Count + 1;
                line.SetPresenter(_presenter);

                var drawLineCommand = new DrawLineCommand(_model, line, _startShape, _currentEndShape);
                _presenter.ExecuteCommand(drawLineCommand);
            }

            // 無論線段是否有效，都立即重置狀態
            ResetState();
            _presenter.ResetToolState();
        }


        public void DrawTemporaryLine(Graphics graphics)
        {
            if (_isSelectingSecondPoint)
            {
                Point drawEndPoint;

                // 如果找到連接點，使用連接點
                if (_currentEndShape != null)
                {
                    drawEndPoint = _currentEndPoint;
                }
                // 否則使用當前鼠標位置
                else
                {
                    drawEndPoint = _currentMouseLocation;
                }

                using (Pen pen = new Pen(Color.Blue, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                {
                    graphics.DrawLine(pen, _startPoint, drawEndPoint);
                }
            }
        }
        private (IShape shape, Point connectionPoint) FindConnectionPoint(Point mouseLocation)
        {
            foreach (var shape in _model.GetShapes())
            {
                var connectionPoints = GetConnectionPoints(shape);
                foreach (var point in connectionPoints)
                {
                    Console.WriteLine($"Mouse: {mouseLocation}, Point: {point}, Distance: {Distance(mouseLocation, point)}");
                    if (Distance(mouseLocation, point) <= CONNECTION_POINT_RADIUS)
                    {
                        return (shape, point);
                    }
                }
            }
            return (null, Point.Empty);
        }


        private Point[] GetConnectionPoints(IShape shape)
        {
            // 只為非線段的形狀提供連接點
            if (shape.GetShapeType() == "Line")
            {
                return new Point[0]; // 返回空數組
            }

            // Return the four connection points: top, bottom, left, right
            return new Point[]
            {
                // Top connection point
                new Point((int)(shape.PositionX + shape.Width / 2), (int)shape.PositionY),
                // Bottom connection point
                new Point((int)(shape.PositionX + shape.Width / 2), (int)(shape.PositionY + shape.Height)),
                // Left connection point
                new Point((int)shape.PositionX, (int)(shape.PositionY + shape.Height / 2)),
                // Right connection point
                new Point((int)(shape.PositionX + shape.Width), (int)(shape.PositionY + shape.Height / 2))
            };
        }

        private float Distance(Point a, Point b)
        {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        public Cursor GetCursor() => Cursors.Cross;
    }
}