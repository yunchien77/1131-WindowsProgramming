using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDrawing.Command
{
    public class CommandManager
    {
        private Stack<ICommand> _undoStack = new Stack<ICommand>();
        private Stack<ICommand> _redoStack = new Stack<ICommand>();

        public event EventHandler UndoRedoStateChanged;

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public void Execute(ICommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear(); // Clear redo stack when a new command is executed
            OnUndoRedoStateChanged();
        }

        public void Undo()
        {
            Console.WriteLine("command manager: undo...");
            if (!CanUndo) return;
            ICommand command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
            OnUndoRedoStateChanged();
        }

        public void Redo()
        {
            Console.WriteLine("command manager: redo...");
            if (!CanRedo) return;
            ICommand command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
            OnUndoRedoStateChanged();
        }

        protected virtual void OnUndoRedoStateChanged()
        {
            UndoRedoStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}