using System.ComponentModel;
using MyDrawing.Shapes;
using System.Drawing;
using System.Windows.Forms;
using MyDrawing.States;

namespace MyDrawing
{
    public class MyDrawingPresenter : INotifyPropertyChanged
    {
        private readonly MyDrawingModel _model;
        private string _currentShapeType;
        private IDrawingState _currentState;
        private readonly DrawingState _drawingState;
        private readonly SelectionState _selectionState;
        private IShape _selectedShape;
        private bool _isAddButtonEnabled;
        private ToolStripButton _cursorButton;
        private Cursor _currentCursor = Cursors.Default;

        public IDrawingState CurrentState => _currentState;
        public event PropertyChangedEventHandler PropertyChanged;


        public MyDrawingPresenter(MyDrawingModel model)
        {
            _model = model;
            _drawingState = new DrawingState(model, this);
            _selectionState = new SelectionState(model, this);

            // Use null coalescing to ensure a default state
            _currentState = _selectionState;

            _isAddButtonEnabled = true;
        }

        public Cursor CurrentCursor
        {
            get => _currentCursor;
            private set
            {
                if (_currentCursor != value)
                {
                    _currentCursor = value;
                    OnPropertyChanged(nameof(CurrentCursor));
                }
            }
        }

        public void UpdateToolSelection(string shapeType, bool isCursorSelected)
        {
            CurrentShapeType = isCursorSelected ? null : shapeType;
            CurrentCursor = isCursorSelected ? Cursors.Default : Cursors.Cross;
            OnPropertyChanged(nameof(CurrentShapeType));
            OnPropertyChanged(nameof(CurrentCursor));
        }


        public void ResetToolState()
        {
            CurrentShapeType = null; // 清除當前選中的形狀類型
            CurrentCursor = Cursors.Default; // 重置游標為一般模式
            OnPropertyChanged(nameof(CurrentShapeType));
            OnPropertyChanged(nameof(CurrentCursor));
        }

        public string CurrentShapeType
        {
            get => _currentShapeType;
            set
            {
                _currentShapeType = value;
                _currentState = value == null ?
                    (IDrawingState)_selectionState :
                    (IDrawingState)_drawingState;

                // Deselect the shape when switching to drawing mode
                if (value != null)
                {
                    SelectedShape = null;
                }

                OnPropertyChanged(nameof(CurrentShapeType));
            }
        }

        public IShape SelectedShape
        {
            get => _selectedShape;
            set
            {
                _selectedShape = value;
                OnPropertyChanged(nameof(SelectedShape));
            }
        }

        public bool IsAddButtonEnabled
        {
            get => _isAddButtonEnabled;
            set
            {
                _isAddButtonEnabled = value;
                OnPropertyChanged(nameof(IsAddButtonEnabled));
            }
        }

        public void HandleMouseDown(Point location)
        {
            _currentState.MouseDown(location);
        }

        public void HandleMouseMove(Point location)
        {
            _currentState.MouseMove(location);
        }

        public void HandleMouseUp(Point location)
        {
            _currentState.MouseUp(location);
        }

        public Cursor GetCurrentCursor()
        {
            if (_currentState is DrawingState)
            {
                return Cursors.Cross;
            }
            return _currentState.GetCursor();
        }

        public void RefreshDrawingPanel()
        {
            // 通知View刷新
            OnPropertyChanged("DrawingPanelRefresh");
        }

        public void ResetToolStripButtons()
        {
            CurrentShapeType = null;
            _currentState = _selectionState;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ValidateInputs(string shapeType, string text, string x, string y, string height, string width)
        {
            bool isValid = !string.IsNullOrEmpty(shapeType) &&
                          !string.IsNullOrEmpty(text) &&
                          int.TryParse(x, out int posX) && posX >= 0 &&
                          int.TryParse(y, out int posY) && posY >= 0 &&
                          int.TryParse(height, out int h) && h > 0 &&
                          int.TryParse(width, out int w) && w > 0;

            IsAddButtonEnabled = isValid;
        }

        public (int Width, int Height, Point Location) GetTempShapeDetails()
        {
            if (_currentState is DrawingState drawingState)
            {
                return drawingState.GetTempShape();
            }
            return (0, 0, Point.Empty);
        }

        public void SetCursorButton(ToolStripButton cursorButton)
        {
            _cursorButton = cursorButton;
        }

        public ToolStripButton GetCursorButton()
        {
            return _cursorButton;
        }
    }
}