using MyDrawing.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyDrawing.Command
{
    public class LoadCommand : ICommand
    {
        private readonly MyDrawingModel _model;
        private readonly string _filePath;
        private readonly List<IShape> _previousShapes;

        private readonly MyDrawingPresenter _presenter;

        public LoadCommand(MyDrawingModel model, string filePath, MyDrawingPresenter presenter)
        {
            _model = model;
            _filePath = filePath;
            _presenter = presenter;
            _previousShapes = _model.GetShapes().ToList();
        }

        public void Execute()
        {
            _model.LoadShapes(_filePath);
            _presenter?.RestorePresenterReferences();
        }

        public void Undo()
        {
            _model.ReplaceShapes(_previousShapes);
        }

        public void Redo()
        {
            _model.LoadShapes(_filePath);
        }
    }
}