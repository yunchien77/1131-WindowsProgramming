using MyDrawing.Shapes;
using MyDrawing.States;
using MyDrawing;
using System.Drawing;
using System.Windows.Forms;
using System;

public class SelectionState : IDrawingState
{
    private readonly MyDrawingModel _model;
    private readonly MyDrawingPresenter _presenter;
    private IShape _selectedShape;
    private bool _isDragging;
    private bool _isDraggingText;
    private PointF _textOffset;
    private PointF _initialOffset;  // 初始偏移量，用來計算拖動距離
    private PointF _textRelativeOffset;  // 文字相對於圖形的偏移量

    public SelectionState(MyDrawingModel model, MyDrawingPresenter presenter)
    {
        _model = model;
        _presenter = presenter;
    }

    // 更新橘色點位置
    private void UpdateOrangeDotPosition()
    {
        if (_selectedShape == null || string.IsNullOrEmpty(_selectedShape.Text)) return;

        using (Font font = new Font("Arial", 10))
        using (Graphics tempGraphics = Graphics.FromHwnd(IntPtr.Zero))
        {
            // 計算文字框的大小
            SizeF textSize = tempGraphics.MeasureString(_selectedShape.Text, font);

            // 橘色點的位置：文字框右邊 3px，向上偏移 3px
            _selectedShape.OrangeDotPosition = new PointF(
                _selectedShape.TextPositionX + textSize.Width + 3,
                _selectedShape.TextPositionY - 3
            );
        }
    }

    // 計算橘色點是否被點擊
    protected bool IsOrangeDotClicked(IShape shape, Point location)
    {
        const int dotSize = 8; // 橘色點大小
        // 計算橘色點的範圍
        RectangleF orangeDotRect = new RectangleF(
            shape.OrangeDotPosition.X - dotSize / 2,
            shape.OrangeDotPosition.Y - dotSize / 2,
            dotSize,
            dotSize
        );

        return orangeDotRect.Contains(location);
    }

    // 計算文本的範圍以確保它不會超出形狀的邊界
    public void MouseDown(Point location)
    {
        Console.WriteLine($"Mouse Down at: {location.X}, {location.Y}");
        _selectedShape = _model.FindShapeAtPosition(location);
        if (_selectedShape != null)
        {
            Console.WriteLine($"Shape Found at: {_selectedShape.PositionX}, {_selectedShape.PositionY}");
            Console.WriteLine($"Text Found at: {_selectedShape.TextPositionX}, {_selectedShape.TextPositionY}");

            if (IsOrangeDotClicked(_selectedShape, location))
            {
                _isDraggingText = true;
                _isDragging = false;

                // 計算文字框相對於滑鼠位置的偏移
                _textOffset = new PointF(
                    location.X - _selectedShape.TextPositionX,
                    location.Y - _selectedShape.TextPositionY
                );

                Console.WriteLine($"Text Offset: {_textOffset.X}, {_textOffset.Y}");
            }
            else
            {
                _isDragging = true;
                _isDraggingText = false;

                // 計算圖形相對於滑鼠位置的偏移
                _initialOffset = new PointF(
                    location.X - _selectedShape.PositionX,
                    location.Y - _selectedShape.PositionY
                );

                Console.WriteLine($"Initial Offset: {_initialOffset.X}, {_initialOffset.Y}");
            }

            _presenter.SelectedShape = _selectedShape;
        }
        else
        {
            _presenter.SelectedShape = null;
        }
        _presenter.RefreshDrawingPanel();
    }

    protected void ConstrainTextWithinShape(ref float newTextX, ref float newTextY)
    {
        if (_selectedShape == null || string.IsNullOrEmpty(_selectedShape.Text)) return;

        SizeF textSize;
        using (Font font = new Font("Arial", 10))
        using (Graphics tempGraphics = Graphics.FromHwnd(IntPtr.Zero))
        {
            textSize = tempGraphics.MeasureString(_selectedShape.Text, font);

            // 計算形狀的邊界
            float shapeLeft = _selectedShape.PositionX - (_selectedShape.Width / 2) + (textSize.Width / 2);
            float shapeRight = _selectedShape.PositionX + (_selectedShape.Width / 2) + (textSize.Width / 2);
            float shapeTop = _selectedShape.PositionY - (_selectedShape.Height / 2) + (textSize.Height / 2);
            float shapeBottom = _selectedShape.PositionY + (_selectedShape.Height / 2) + (textSize.Height / 2);

            // 確保文字不會超出形狀邊界
            newTextX = Math.Max(shapeLeft, Math.Min(shapeRight - textSize.Width, newTextX));
            newTextY = Math.Max(shapeTop, Math.Min(shapeBottom - textSize.Height, newTextY));

            Console.WriteLine($"Constrain Debug: " +
                              $"Shape({shapeLeft},{shapeTop},{shapeRight},{shapeBottom}) " +
                              $"Text({newTextX},{newTextY},{textSize.Width},{textSize.Height})");
        }
    }

    public void MouseMove(Point location)
    {
        Console.WriteLine($"Mouse Position: {location.X}, {location.Y}");

        if (_selectedShape != null)
        {
            Console.WriteLine($"Shape Position: {_selectedShape.PositionX}, {_selectedShape.PositionY}");
            Console.WriteLine($"Text Position: {_selectedShape.TextPositionX}, {_selectedShape.TextPositionY}");
        }

        if (_isDraggingText && _selectedShape != null)
        {
            // 根據滑鼠位置更新文字位置
            float newTextX = location.X - _textOffset.X;
            float newTextY = location.Y - _textOffset.Y;

            // 確保文字不會超出形狀的邊界
            ConstrainTextWithinShape(ref newTextX, ref newTextY);

            // 更新文字相對於圖形的偏移量
            _textRelativeOffset = new PointF(
                newTextX - _selectedShape.PositionX,
                newTextY - _selectedShape.PositionY
            );

            _selectedShape.TextPositionX = (int)newTextX;
            _selectedShape.TextPositionY = (int)newTextY;

            // 更新橘色點位置
            UpdateOrangeDotPosition();

            Console.WriteLine($"Updated Text Position: {_selectedShape.TextPositionX}, {_selectedShape.TextPositionY}");

            // 刷新繪圖面板
            _presenter.RefreshDrawingPanel();
        }
        else if (_isDragging && _selectedShape != null)
        {
            // 根據滑鼠移動來更新圖形的位置
            _selectedShape.PositionX = location.X - (int)_initialOffset.X;
            _selectedShape.PositionY = location.Y - (int)_initialOffset.Y;

            // 根據偏移量更新文字的位置
            _selectedShape.TextPositionX = (int)(_selectedShape.PositionX + _textRelativeOffset.X);
            _selectedShape.TextPositionY = (int)(_selectedShape.PositionY + _textRelativeOffset.Y);

            // 更新橘色點位置
            UpdateOrangeDotPosition();

            Console.WriteLine($"Updated Shape Position: {_selectedShape.PositionX}, {_selectedShape.PositionY}");
            Console.WriteLine($"Updated Text Position: {_selectedShape.TextPositionX}, {_selectedShape.TextPositionY}");

            // 刷新繪圖面板
            _presenter.RefreshDrawingPanel();
        }
    }




    public void MouseUp(Point location)
    {
        if (_isDragging || _isDraggingText)
        {
            _isDragging = false;
            _isDraggingText = false;
            _initialOffset = PointF.Empty;
            _model.OnShapesChanged();
        }
    }

    public Cursor GetCursor() => Cursors.Default;
}
