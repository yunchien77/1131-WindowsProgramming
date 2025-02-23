using System.ComponentModel;
using MyDrawing.Shapes;
using System.Drawing;
using System.Windows.Forms;
using MyDrawing.States;
using System;
using MyDrawing.Command;
using static DecisionShape;
using System.Linq;
using System.Collections.Generic;

namespace MyDrawing
{
    public class MyDrawingPresenter : INotifyPropertyChanged
    {
        private readonly MyDrawingModel _model;
        private string _currentShapeType;
        private IDrawingState _currentState;
        private readonly DrawingState _drawingState;
        private readonly SelectionState _selectionState;
        private readonly LineDrawingState _lineDrawingState;
        private IShape _selectedShape;
        private bool _isAddButtonEnabled;
        private Cursor _currentCursor = Cursors.Default;
        private List<LineShape> _temporaryMovingLines;

        public IDrawingState CurrentState => _currentState;
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly CommandManager _commandManager;
        private bool _canUndo;
        private bool _canRedo;

        public bool CanUndo
        {
            get => _canUndo;
            private set
            {
                _canUndo = value;
                OnPropertyChanged(nameof(CanUndo));
            }
        }

        public bool CanRedo
        {
            get => _canRedo;
            private set
            {
                _canRedo = value;
                OnPropertyChanged(nameof(CanRedo));
            }
        }

        public MyDrawingPresenter(MyDrawingModel model, Action<Cursor> setCursorAction)
        {
            _model = model;
            _drawingState = new DrawingState(model, this, setCursorAction);
            _selectionState = new SelectionState(model, this);
            _lineDrawingState = new LineDrawingState(model, this, setCursorAction);
            _temporaryMovingLines = new List<LineShape>();

            _currentState = _selectionState;

            // Disable the Add button initially
            _isAddButtonEnabled = false;

            _commandManager = new CommandManager();
            _commandManager.UndoRedoStateChanged += (s, e) =>
            {
                CanUndo = _commandManager.CanUndo;
                CanRedo = _commandManager.CanRedo;
            };
        }

        public void ExecuteCommand(ICommand command)
        {
            _commandManager.Execute(command);
        }

        public void Undo()
        {
            if (_commandManager.CanUndo)
            {
                _commandManager.Undo();
                // Potentially refresh the view or selected state
                RefreshDrawingPanel();
            }
        }

        public void Redo()
        {
            if (_commandManager.CanRedo)
            {
                _commandManager.Redo();
                // Potentially refresh the view or selected state
                RefreshDrawingPanel();
            }
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

            // 如果是線段模式，切換到 LineDrawingState
            if (shapeType == "Line")
            {
                Console.WriteLine("line state...");
                _currentState = _lineDrawingState;
                RefreshDrawingPanel();
            }
            else
            {
                if (isCursorSelected)
                {
                    Console.WriteLine("select state...");
                    _currentState = _selectionState;
                }
                else
                {
                    Console.WriteLine("draw state...");
                    _currentState = _drawingState;
                }
            }


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

        public void RefreshDrawingPanel()
        {
            // 通知View刷新
            OnPropertyChanged("DrawingPanelRefresh");
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ValidateInputs(string shapeType, string text, string x, string y, string height, string width)
        {
            bool isValid = !string.IsNullOrEmpty(shapeType) &&
                          !string.IsNullOrEmpty(text) &&
                          float.TryParse(x, out float posX) && posX >= 0 &&
                          float.TryParse(y, out float posY) && posY >= 0 &&
                          float.TryParse(height, out float h) && h > 0 &&
                          float.TryParse(width, out float w) && w > 0;

            IsAddButtonEnabled = isValid;
        }

        public void HandleShapeMove(IShape movedShape, float deltaX, float deltaY)
        {
            // 記錄原始連線信息
            var connectedLines = _model.Shapes
                .OfType<LineShape>()
                .Where(line => line.StartShape == movedShape || line.EndShape == movedShape)
                .Select(line => new
                {
                    Line = line,
                    OriginalStartPoint = line.StartPoint,
                    OriginalEndPoint = line.EndPoint,
                    OriginalPositionX = line.PositionX,
                    OriginalPositionY = line.PositionY,
                    OriginalWidth = line.Width,
                    OriginalHeight = line.Height
                })
                .ToList();

            //foreach (var lineInfo in connectedLines)
            //{
            //    var line = lineInfo.Line;
            //    // 更新線段的連接點
            //    if (line.StartShape == movedShape)
            //    {
            //        line.StartPoint = UpdateConnectionPoint(line.StartPoint, deltaX, deltaY);
            //    }
            //    if (line.EndShape == movedShape)
            //    {
            //        line.EndPoint = UpdateConnectionPoint(line.EndPoint, deltaX, deltaY);
            //    }
            //}

            // 立即刷新繪圖面板，實現實時更新
            RefreshDrawingPanel();
        }

        //private PointF UpdateConnectionPoint(PointF originalPoint, float deltaX, float deltaY)
        //{
        //    return new PointF(
        //        originalPoint.X + deltaX,
        //        originalPoint.Y + deltaY
        //    );
        //}


        // Add this method to handle hovering and showing connection points
        public void HandleMouseHover(Point location)
        {
            // Check if we're in line drawing mode
            if (_currentState is LineDrawingState)
            {
                var nearestShape = _model.GetShapes()
                .FirstOrDefault(shape =>
                    !(shape is LineShape) && // 過濾掉 LineShape
                    location.X >= shape.PositionX &&
                    location.X <= shape.PositionX + shape.Width &&
                    location.Y >= shape.PositionY &&
                    location.Y <= shape.PositionY + shape.Height
                );


                // Trigger a redraw to show/hide connection points
                if (nearestShape != null)
                {
                    RefreshDrawingPanel();
                }
            }
        }

        // Modify the existing code to include this method in the presenter
        public bool ShouldShowConnectionPoints(IShape shape, Point mouseLocation)
        {
            // Only show connection points when in line drawing mode and mouse is near the shape
            if (_currentState is LineDrawingState)
            {
                return mouseLocation.X >= shape.PositionX &&
                       mouseLocation.X <= shape.PositionX + shape.Width &&
                       mouseLocation.Y >= shape.PositionY &&
                       mouseLocation.Y <= shape.PositionY + shape.Height;
            }
            return false;
        }


    }
}