using MyDrawing.Shapes;
using MyDrawing.States;
using MyDrawing;
using System.Drawing;
using System.Windows.Forms;
using System;
using MyDrawing.Command;

public class SelectionState : IDrawingState
{
    private readonly MyDrawingModel _model;
    private readonly MyDrawingPresenter _presenter;
    private IShape _selectedShape;
    private bool _isDragging;
    private bool _isDraggingOrangeDot;
    private Point _lastMouseLocation;
    private float _totalDeltaX;
    private float _totalDeltaY;
    private bool _isDoubleClicked;
    private DateTime _lastClickTime;

    public SelectionState(MyDrawingModel model, MyDrawingPresenter presenter)
    {
        _model = model;
        _presenter = presenter;
        _lastClickTime = DateTime.MinValue;
    }

    public void MouseDown(Point location)
    {
        _selectedShape = _model.FindShapeAtPosition(location);
        if (_selectedShape != null)
        {
            // 檢查是否點擊橘色點
            if (IsOrangeDotClicked(location))
            {
                if (IsDoubleClicked(location))
                {
                    // 彈出文字編輯對話框
                    EditShapeText();
                }
                else
                {
                    _isDoubleClicked = false;
                    _isDraggingOrangeDot = true;
                    _isDragging = false;
                    _lastMouseLocation = location;
                }

            }
            else
            {
                // 點擊形狀本身，準備移動整個形狀
                _isDragging = true;
                _isDraggingOrangeDot = false;
                _lastMouseLocation = location;
            }

            _presenter.SelectedShape = _selectedShape;
        }
        else
        {
            _presenter.SelectedShape = null;
        }
        _presenter.RefreshDrawingPanel();
    }

    public void MouseMove(Point location)
    {
        if (_isDraggingOrangeDot && _selectedShape != null)
        {
            // 文字拖动逻辑...
            float deltaX = location.X - _lastMouseLocation.X;
            float deltaY = location.Y - _lastMouseLocation.Y;

            _totalDeltaX += deltaX;
            _totalDeltaY += deltaY;

            // 更新文字框位置
            _selectedShape.TextPositionX += deltaX;
            _selectedShape.TextPositionY += deltaY;

            // 确保文字不会超出形状边界
            float newTextX = _selectedShape.TextPositionX;
            float newTextY = _selectedShape.TextPositionY;
            ConstrainTextWithinShape(ref newTextX, ref newTextY);

            _selectedShape.TextPositionX = newTextX;
            _selectedShape.TextPositionY = newTextY;

            // 更新橘色点位置
            UpdateOrangeDotPosition();

            _presenter.RefreshDrawingPanel();
        }
        else if (_isDragging && _selectedShape != null)
        {
            // 形状拖动逻辑
            float deltaX = location.X - _lastMouseLocation.X;
            float deltaY = location.Y - _lastMouseLocation.Y;

            _totalDeltaX += deltaX;
            _totalDeltaY += deltaY;

            // 移动整个形状
            _selectedShape.PositionX += deltaX;
            _selectedShape.PositionY += deltaY;

            // 同时移动文字框
            _selectedShape.TextPositionX += deltaX;
            _selectedShape.TextPositionY += deltaY;

            // 实时更新连接线
            _presenter.HandleShapeMove(_selectedShape, deltaX, deltaY);

            // 更新橘色点位置
            UpdateOrangeDotPosition();

            _presenter.RefreshDrawingPanel();
        }

        // 更新最后鼠标位置
        _lastMouseLocation = location;
    }

    public void MouseUp(Point location)
    {
        if (_isDragging && _selectedShape != null)
        {
            Console.WriteLine($"-----shape & text moving");
            if (_totalDeltaX != 0 || _totalDeltaY != 0)
            {
                float oldX = _selectedShape.PositionX - _totalDeltaX;
                float oldY = _selectedShape.PositionY - _totalDeltaY;
                float oldTextX = _selectedShape.TextPositionX - _totalDeltaX;
                float oldTextY = _selectedShape.TextPositionY - _totalDeltaY;

                //_presenter.ExecuteCommand(new MoveShapeCommand(_selectedShape,
                //    oldX, oldY, oldTextX, oldTextY,
                //    _selectedShape.PositionX, _selectedShape.PositionY,
                //    _selectedShape.TextPositionX, _selectedShape.TextPositionY));
                _presenter.ExecuteCommand(new MoveShapeCommand(
                    _model,
                    _selectedShape,
                    oldX, oldY,
                    oldTextX, oldTextY,
                    _selectedShape.PositionX, _selectedShape.PositionY,
                    _selectedShape.TextPositionX, _selectedShape.TextPositionY
                ));
            }
        }
        if (_isDraggingOrangeDot && _selectedShape != null)
        {
            Console.WriteLine($"-----text moving");
            if (_totalDeltaX != 0 || _totalDeltaY != 0)
            {
                float oldTextX = _selectedShape.TextPositionX - _totalDeltaX;
                float oldTextY = _selectedShape.TextPositionY - _totalDeltaY;

                _presenter.ExecuteCommand(new MoveTextCommand(_selectedShape,
                    oldTextX, oldTextY,
                    _selectedShape.TextPositionX, _selectedShape.TextPositionY));
            }
        }

        _totalDeltaX = 0;
        _totalDeltaY = 0;
        _isDragging = false;
        _isDraggingOrangeDot = false;
        _model.OnShapesChanged();
    }

    // 確保文字框不會超出形狀邊界的方法
    protected void ConstrainTextWithinShape(ref float newTextX, ref float newTextY)
    {
        if (_selectedShape == null || string.IsNullOrEmpty(_selectedShape.Text)) return;

        SizeF textSize;
        using (Font font = new Font("Arial", 10))
        using (Graphics tempGraphics = Graphics.FromHwnd(IntPtr.Zero))
        {
            textSize = tempGraphics.MeasureString(_selectedShape.Text, font);

            // 計算形狀的邊界
            float shapeLeft = _selectedShape.PositionX;
            float shapeRight = _selectedShape.PositionX + _selectedShape.Width;
            float shapeTop = _selectedShape.PositionY;
            float shapeBottom = _selectedShape.PositionY + _selectedShape.Height;

            // 確保文字不會超出形狀邊界
            newTextX = Math.Max(shapeLeft, Math.Min(shapeRight - textSize.Width, newTextX));
            newTextY = Math.Max(shapeTop, Math.Min(shapeBottom - textSize.Height, newTextY));
        }
    }

    private void UpdateOrangeDotPosition()
    {
        if (_selectedShape == null || string.IsNullOrEmpty(_selectedShape.Text))
        {
            //Console.WriteLine("Text is null or empty, skipping update.");
            return;
        }

        using (Font font = new Font("Arial", 10))
        using (Graphics tempGraphics = Graphics.FromHwnd(IntPtr.Zero))
        {
            SizeF textSize = tempGraphics.MeasureString(_selectedShape.Text, font);
            _selectedShape.OrangeDotPosition = new PointF(
                _selectedShape.TextPositionX + (textSize.Width / 2) - 4,
                _selectedShape.TextPositionY - 8
            );
        }
    }

    // 計算橘色點是否被點擊的方法
    public bool IsOrangeDotClicked(Point location)
    {
        const int dotSize = 8; // 橘色點大小
        // 計算橘色點的範圍
        RectangleF orangeDotRect = new RectangleF(
            _selectedShape.OrangeDotPosition.X,
            _selectedShape.OrangeDotPosition.Y,
            dotSize,
            dotSize
        );

        return orangeDotRect.Contains(location);
    }

    public bool IsDoubleClicked(Point location)
    {
        TimeSpan timeDiff = DateTime.Now - _lastClickTime;
        if (timeDiff.TotalMilliseconds <= 500 && location == _lastMouseLocation)
        {
            _lastClickTime = DateTime.MinValue;
            return true;
        }
        else
        {
            _lastClickTime = DateTime.Now;
            _lastMouseLocation = location;
            return false;
        }
    }

    private void EditShapeText()
    {
        // 備份舊文字
        string oldText = _selectedShape.Text;

        // 彈出文字編輯對話框
        using (var dialog = new TextEditDialog(_selectedShape.Text))
        {
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                // 如果文字有變更
                if (dialog.TextValue != oldText)
                {
                    // 執行文字修改命令，這樣可以支援 undo 和 redo
                    _presenter.ExecuteCommand(new ModifyTextCommand((Shape)_selectedShape, oldText, dialog.TextValue));
                }

                // 更新橘色點位置
                UpdateOrangeDotPosition();

                // 通知模型更新
                _model.OnShapesChanged();
            }
        }
    }

    public Cursor GetCursor() => Cursors.Default;

    public void SetLastMouseLocationForTesting(Point location)
    {
        _lastMouseLocation = location;
    }

    public void SetLastClickTimeForTesting(DateTime time)
    {
        _lastClickTime = time;
    }
}