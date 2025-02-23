using MyDrawing.Shapes;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MyDrawing.States
{
    public class SelectionState : IDrawingState
    {
        private readonly MyDrawingModel _model;
        private readonly MyDrawingPresenter _presenter;
        private IShape _selectedShape;
        private bool _isDragging;
        private Point _lastLocation;

        public SelectionState(MyDrawingModel model, MyDrawingPresenter presenter)
        {
            _model = model;
            _presenter = presenter;
        }

        public void MouseDown(Point location)
        {
            _selectedShape = _model.FindShapeAtPosition(location);
            if (_selectedShape != null)
            {
                _isDragging = true;
                _lastLocation = location;
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
            if (_isDragging && _selectedShape != null)
            {
                int deltaX = location.X - _lastLocation.X;
                int deltaY = location.Y - _lastLocation.Y;

                _selectedShape.PositionX += deltaX;
                _selectedShape.PositionY += deltaY;

                _lastLocation = location;
                _presenter.RefreshDrawingPanel();
            }
        }

        public void MouseUp(Point location)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _model.OnShapesChanged();
            }
        }

        public Cursor GetCursor() => Cursors.Default;
    }
}