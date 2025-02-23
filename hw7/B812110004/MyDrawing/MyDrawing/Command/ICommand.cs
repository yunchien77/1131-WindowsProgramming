using MyDrawing.Shapes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DecisionShape;

namespace MyDrawing.Command
{
    // Base command interface
    public interface ICommand
    {
        void Execute();
        void Undo();
    }

    // Shape Drawing Command
    public class DrawShapeCommand : ICommand
    {
        private MyDrawingModel _model;
        private IShape _shape;
        private bool _wasAlreadyInModel;

        public DrawShapeCommand(MyDrawingModel model, IShape shape)
        {
            _model = model;
            _shape = shape;
            _wasAlreadyInModel = _model.Shapes.Contains(shape);
        }

        public void Execute()
        {
            Console.WriteLine("draw shape command: execute");
            _model.Shapes.Add(_shape);
            _model.HasUnsavedChanges = true;
            Console.WriteLine($"Shape added to model, current count: {_model.Shapes.Count}");
            _model.OnShapesChanged();
            Console.WriteLine($"OnShapesChanged");
        }

        public void Undo()
        {
            Console.WriteLine("draw shape command: undo");
            _model.Shapes.Remove(_shape);
            _model.HasUnsavedChanges = true;
            _model.OnShapesChanged();
        }
    }

    public class DrawLineCommand : ICommand
    {
        private readonly MyDrawingModel _model;
        private readonly LineShape _line;
        private readonly IShape _startShape;
        private readonly IShape _endShape;

        public DrawLineCommand(MyDrawingModel model, LineShape line, IShape startShape, IShape endShape)
        {
            _model = model;
            _line = line;
            _startShape = startShape;
            _endShape = endShape;
        }

        public void Execute()
        {
            // Add the line to the model's shapes
            _model.Shapes.Add(_line);
            _model.HasUnsavedChanges = true;
            _model.OnShapesChanged();
        }

        public void Undo()
        {
            // Remove the line from the model
            _model.Shapes.Remove(_line);
            _model.HasUnsavedChanges = true;
            _model.OnShapesChanged();
        }
    }

    public class MoveShapeCommand : ICommand
    {
        private readonly MyDrawingModel _model;
        private readonly IShape _movedShape;
        private readonly float _oldShapeX, _oldShapeY;
        private readonly float _oldTextX, _oldTextY;
        private readonly float _newShapeX, _newShapeY;
        private readonly float _newTextX, _newTextY;

        // 記錄受影響的線段信息
        private readonly List<LineShapeSnapshot> _connectedLines;

        private class LineShapeSnapshot
        {
            public LineShape Line { get; set; }
            public PointF OldStartPoint { get; set; }
            public PointF OldEndPoint { get; set; }
            public float OldPositionX { get; set; }
            public float OldPositionY { get; set; }
            public float OldWidth { get; set; }
            public float OldHeight { get; set; }
            public bool IsStartShapeConnected { get; set; }
            public bool IsEndShapeConnected { get; set; }
        }

        public MoveShapeCommand(
            MyDrawingModel model,
            IShape movedShape,
            float oldShapeX, float oldShapeY,
            float oldTextX, float oldTextY,
            float newShapeX, float newShapeY,
            float newTextX, float newTextY)
        {
            _model = model;
            _movedShape = movedShape;
            _oldShapeX = oldShapeX;
            _oldShapeY = oldShapeY;
            _oldTextX = oldTextX;
            _oldTextY = oldTextY;
            _newShapeX = newShapeX;
            _newShapeY = newShapeY;
            _newTextX = newTextX;
            _newTextY = newTextY;

            // 找出所有連接到此形狀的線段，並記錄其當前位置
            _connectedLines = _model.Shapes
                .OfType<LineShape>()
                .Where(line => line.StartShape == movedShape || line.EndShape == movedShape)
                .Select(line => new LineShapeSnapshot
                {
                    Line = line,
                    OldStartPoint = line.StartPoint,
                    OldEndPoint = line.EndPoint,
                    OldPositionX = line.PositionX,
                    OldPositionY = line.PositionY,
                    OldWidth = line.Width,
                    OldHeight = line.Height,
                    IsStartShapeConnected = line.StartShape == movedShape,
                    IsEndShapeConnected = line.EndShape == movedShape
                })
                .ToList();
        }

        public void Execute()
        {
            Console.WriteLine("move shape command: execute");

            // 計算移動的增量
            float deltaX = _newShapeX - _oldShapeX;
            float deltaY = _newShapeY - _oldShapeY;

            // 移動形狀和文字
            _movedShape.PositionX = _newShapeX;
            _movedShape.PositionY = _newShapeY;
            _movedShape.TextPositionX = _newTextX;
            _movedShape.TextPositionY = _newTextY;

            // 更新連接線的位置
            UpdateLinePositions(deltaX, deltaY);
            _model.HasUnsavedChanges = true;
        }

        public void Undo()
        {
            Console.WriteLine("move shape command: undo");

            // 恢復形狀和文字位置
            _movedShape.PositionX = _oldShapeX;
            _movedShape.PositionY = _oldShapeY;
            _movedShape.TextPositionX = _oldTextX;
            _movedShape.TextPositionY = _oldTextY;

            // 恢復連接線的位置
            foreach (var lineSnapshot in _connectedLines)
            {
                var line = lineSnapshot.Line;

                // 恢復線段的起點和終點
                line.StartPoint = lineSnapshot.OldStartPoint;
                line.EndPoint = lineSnapshot.OldEndPoint;

                // 恢復線段的位置和大小
                line.PositionX = lineSnapshot.OldPositionX;
                line.PositionY = lineSnapshot.OldPositionY;
                line.Width = lineSnapshot.OldWidth;
                line.Height = lineSnapshot.OldHeight;
            }
            _model.HasUnsavedChanges = true;
        }

        private void UpdateLinePositions(float deltaX, float deltaY)
        {
            foreach (var lineSnapshot in _connectedLines)
            {
                var line = lineSnapshot.Line;

                // 根據連接點更新線段的起點或終點
                if (lineSnapshot.IsStartShapeConnected)
                {
                    line.StartPoint = new PointF(
                        line.StartPoint.X + deltaX,
                        line.StartPoint.Y + deltaY
                    );
                }

                if (lineSnapshot.IsEndShapeConnected)
                {
                    line.EndPoint = new PointF(
                        line.EndPoint.X + deltaX,
                        line.EndPoint.Y + deltaY
                    );
                }

                // 重新計算線段的位置和大小
                line.PositionX = Math.Min(line.StartPoint.X, line.EndPoint.X);
                line.PositionY = Math.Min(line.StartPoint.Y, line.EndPoint.Y);
                line.Width = Math.Abs(line.StartPoint.X - line.EndPoint.X);
                line.Height = Math.Abs(line.StartPoint.Y - line.EndPoint.Y);
            }
        }
    }


    // Text Modification Command
    public class ModifyTextCommand : ICommand
    {
        private readonly MyDrawingModel _model;
        private Shape _shape;
        private string _oldText;
        private string _newText;
        private float _oldTextX, _oldTextY;  // Track original text position
        private PointF _oldOrangeDotPosition;

        public ModifyTextCommand(MyDrawingModel model, Shape shape, string oldText, string newText)
        {
            _model = model;
            _shape = shape;
            _oldText = oldText;
            _newText = newText;

            // Capture original text position and orange dot position
            _oldTextX = _shape.TextPositionX;
            _oldTextY = _shape.TextPositionY;
            _oldOrangeDotPosition = _shape.OrangeDotPosition;
        }

        public void Execute()
        {
            Console.WriteLine("modify text command: execute");
            _shape.Text = _newText;
            // Recalculate text position based on new text
            UpdateTextPosition();
            _model.HasUnsavedChanges = true;
        }

        public void Undo()
        {
            Console.WriteLine("modify text command: undo");
            _shape.Text = _oldText;
            // Restore original text position
            _shape.TextPositionX = _oldTextX;
            _shape.TextPositionY = _oldTextY;
            _shape.OrangeDotPosition = _oldOrangeDotPosition;
            _model.HasUnsavedChanges = true;
        }

        private void UpdateTextPosition()
        {
            using (Font font = new Font("Arial", 10))
            using (Graphics tempGraphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                // Recalculate text position for new text
                SizeF textSize = tempGraphics.MeasureString(_shape.Text, font);

                float textX = _shape.PositionX + (_shape.Width / 2) - (textSize.Width / 2);
                float textY = _shape.PositionY + (_shape.Height / 2) - (textSize.Height / 2);

                _shape.TextPositionX = (float)textX;
                _shape.TextPositionY = (float)textY;

                // Update orange dot position
                _shape.OrangeDotPosition = new PointF(
                    _shape.TextPositionX + (textSize.Width / 2) - 4,
                    _shape.TextPositionY - 8
                );
            }
        }
    }

    // Text Movement Command (For moving text independently)
    public class MoveTextCommand : ICommand
    {
        private readonly MyDrawingModel _model;
        private IShape _shape;
        private float _oldTextX, _oldTextY;
        private float _newTextX, _newTextY;

        public MoveTextCommand(MyDrawingModel model, IShape shape,
            float oldTextX, float oldTextY,
            float newTextX, float newTextY)
        {
            _model = model;
            _shape = shape;
            _oldTextX = oldTextX;
            _oldTextY = oldTextY;
            _newTextX = newTextX;
            _newTextY = newTextY;
        }

        public void Execute()
        {
            Console.WriteLine("move text command: execute");
            _shape.TextPositionX = _newTextX;
            _shape.TextPositionY = _newTextY;
            UpdateOrangeDotPosition(_shape);
            _model.HasUnsavedChanges = true;
        }

        public void Undo()
        {
            Console.WriteLine("move text command: undo");
            _shape.TextPositionX = _oldTextX;
            _shape.TextPositionY = _oldTextY;
            UpdateOrangeDotPosition(_shape);
            _model.HasUnsavedChanges = true;
        }

        private void UpdateOrangeDotPosition(IShape shape)
        {
            using (Font font = new Font("Arial", 10))
            using (Graphics tempGraphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                SizeF textSize = tempGraphics.MeasureString(shape.Text, font);
                shape.OrangeDotPosition = new PointF(
                    shape.TextPositionX + (textSize.Width / 2) - 4,
                    shape.TextPositionY - 8
                );
            }
        }
    }

    public class DeleteShapeCommand : ICommand
    {
        private MyDrawingModel _model;
        private IShape _shape;
        private List<LineShape> _connectedLines;

        public DeleteShapeCommand(MyDrawingModel model, IShape shape)
        {
            _model = model;
            _shape = shape;
            // 在建構時記錄所有與此形狀相連的線段
            _connectedLines = _model.Shapes
                .OfType<LineShape>()
                .Where(line => line.StartShape == shape || line.EndShape == shape)
                .ToList();
        }

        public void Execute()
        {
            Console.WriteLine("delete shape command: execute");
            // 先刪除所有相連的線段
            foreach (var line in _connectedLines)
            {
                _model.Shapes.Remove(line);
            }
            // 再刪除形狀本身
            _model.DeleteShape(_shape.Id);
            _model.HasUnsavedChanges = true;
        }

        public void Undo()
        {
            Console.WriteLine("delete shape command: undo");
            // 先恢復形狀本身
            _model.Shapes.Add(_shape);
            // 再恢復所有相連的線段
            foreach (var line in _connectedLines)
            {
                _model.Shapes.Add(line);
            }
            _model.OnShapesChanged();
            _model.HasUnsavedChanges = true;
        }
    }
}



