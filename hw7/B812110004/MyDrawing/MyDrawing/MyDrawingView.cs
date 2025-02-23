using MyDrawing.Command;
using MyDrawing.Factories;
using MyDrawing.Shapes;
using MyDrawing.States;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyDrawing
{
    public partial class MyDrawing : Form
    {
        private MyDrawingModel _model;
        private MyDrawingPresenter _presenter;

        public MyDrawing()
        {
            InitializeComponent();
            InitializeDrawingPanel();
            InitializeToolStrip();

            _model = new MyDrawingModel();

            // Pass cursor setting action to DrawingState
            _presenter = new MyDrawingPresenter(_model, cursor =>
            {
                drawingPanel.Cursor = cursor;
            });
            _presenter.View = this;

            // Data bindings
            addButton.DataBindings.Add("Enabled", _presenter, "IsAddButtonEnabled");

            saveIcon.Enabled = !_presenter.IsSaving;

            // Subscribe to presenter's property changes
            _presenter.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_presenter.IsSaving))
                {
                    saveIcon.Enabled = !_presenter.IsSaving;
                }
                if (e.PropertyName == nameof(_presenter.CurrentShapeType))
                {
                    UpdateToolStripButtons();
                }

                if (e.PropertyName == nameof(_presenter.CurrentCursor))
                {
                    drawingPanel.Cursor = _presenter.CurrentCursor;
                }

                if (e.PropertyName == "DrawingPanelRefresh" || e.PropertyName == "SelectedShape")
                {
                    drawingPanel.Refresh();
                }

                if (e.PropertyName == nameof(_presenter.IsAutoSaving))
                {
                    UpdateFormTitle();
                }
            };

            shapeDataGridView.CellContentClick += ShapeDataGridView_DeleteShape;
            addButton.Click += AddButton_Click;

            InitializeDataGridView();
            _model.ShapesChanged += Model_ShapesChanged;

            // Setup TextBox validation events
            XText.TextChanged += (s, e) => ValidateInput();
            YText.TextChanged += (s, e) => ValidateInput();
            HText.TextChanged += (s, e) => ValidateInput();
            WText.TextChanged += (s, e) => ValidateInput();
            wordText.TextChanged += (s, e) => ValidateInput();
            comboBoxShapeType.SelectedIndexChanged += (s, e) => ValidateInput();

            // Wire up Undo/Redo buttons
            undoIcon.Click += UndoIcon_Click;
            redoIcon.Click += RedoIcon_Click;

            // Initial state of Undo/Redo buttons
            undoIcon.Enabled = false;
            redoIcon.Enabled = false;

            // Bind Undo/Redo button states to presenter
            _presenter.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_presenter.CanUndo))
                {
                    undoIcon.Enabled = _presenter.CanUndo;
                }
                if (e.PropertyName == nameof(_presenter.CanRedo))
                {
                    redoIcon.Enabled = _presenter.CanRedo;
                }
            };
        }

        private void UpdateFormTitle()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateFormTitle));
                return;
            }

            this.Text = _presenter.IsAutoSaving ?
                "MyDrawing - Auto saving..." :
                "MyDrawing";
        }

        private void UndoIcon_Click(object sender, EventArgs e)
        {
            _presenter.Undo();
        }

        private void RedoIcon_Click(object sender, EventArgs e)
        {
            _presenter.Redo();
        }

        private void UpdateToolStripButtons()
        {
            foreach (ToolStripButton item in shapeToolStrip.Items)
            {
                item.Checked = (item.Text == _presenter.CurrentShapeType) ||
                               (item == cursorIcon && _presenter.CurrentShapeType == null);
            }
        }

        private void InitializeDataGridView()
        {
            shapeDataGridView.AutoGenerateColumns = false;
            shapeDataGridView.ReadOnly = true;
            shapeDataGridView.AllowUserToAddRows = false;
            RefreshDataGridView();
        }

        private void InitializeToolStrip()
        {
            shapeToolStrip.Items.Clear();

            shapeToolStrip.Items.AddRange(new ToolStripItem[] {
                startIcon,
                decisionIcon,
                terminatorIcon,
                processIcon,
                lineIcon,
                cursorIcon,
                undoIcon,
                redoIcon,
                saveIcon,
                loadIcon
            });

            startIcon.Click += ToolStripButton_Click;
            decisionIcon.Click += ToolStripButton_Click;
            terminatorIcon.Click += ToolStripButton_Click;
            processIcon.Click += ToolStripButton_Click;
            lineIcon.Click += ToolStripButton_Click;
            cursorIcon.Click += ToolStripButton_Click;
            saveIcon.Click += SaveButton_Click;
            loadIcon.Click += LoadButton_Click;
        }

        private async void SaveButton_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Drawing Files|*.mydrawing";
                saveFileDialog.Title = "Save Drawing";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        await _presenter.SaveShapesAsync(saveFileDialog.FileName);
                        MessageBox.Show("Drawing saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to save drawing: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Drawing Files|*.mydrawing";
                openFileDialog.Title = "Load Drawing";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _presenter.LoadShapes(openFileDialog.FileName);
                        _presenter.RestorePresenterReferences(); // Add this line
                        MessageBox.Show("Drawing loaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to load drawing: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ToolStripButton_Click(object sender, EventArgs e)
        {
            var button = (ToolStripButton)sender;
            string shapeType = button == cursorIcon ? null : button.Text;

            _presenter.UpdateToolSelection(shapeType, button == cursorIcon);
        }

        private void InitializeDrawingPanel()
        {
            drawingPanel.Paint += DrawingPanel_Paint;
            drawingPanel.MouseDown += DrawingPanel_MouseDown;
            drawingPanel.MouseMove += DrawingPanel_MouseMove;
            drawingPanel.MouseUp += DrawingPanel_MouseUp;
            drawingPanel.MouseEnter += DrawingPanel_MouseEnter;
            drawingPanel.MouseLeave += DrawingPanel_MouseLeave;

            // Use this.SetStyle instead of drawingPanel.SetStyle
            drawingPanel.GetType().GetProperty("DoubleBuffered",
             System.Reflection.BindingFlags.Instance |
             System.Reflection.BindingFlags.NonPublic)
             ?.SetValue(drawingPanel, true, null);
        }

        private void DrawingPanel_Paint(object sender, PaintEventArgs e)
        {
            var graphics = new GraphicsAdapter(e.Graphics);
            foreach (var shape in _model.GetShapes())
            {
                shape.Draw(graphics);

                if (shape.GetShapeType() != "Line")
                {
                    // Draw selection rectangle if shape is selected
                    if (shape == _presenter.SelectedShape)
                    {
                        using (Pen pen = new Pen(Color.Red) { DashStyle = DashStyle.Dash })
                        {
                            e.Graphics.DrawRectangle(pen, shape.PositionX, shape.PositionY, shape.Width, shape.Height);
                        }
                    }
                }

            }

            // Draw temporary shape during drawing
            if (_presenter.CurrentState is DrawingState drawingState)
            {
                drawingState.DrawTemporaryShape(e.Graphics);
            }

            // 繪製臨時線段
            if (_presenter.CurrentState is LineDrawingState lineDrawingState)
            {
                lineDrawingState.DrawTemporaryLine(e.Graphics);
            }

            DrawConnectionPoints(e);
        }

        private void DrawConnectionPoints(PaintEventArgs e)
        {
            //Console.WriteLine("DrawConnectionPoints...");
            // 僅在線段模式時顯示連接點
            if (_presenter.CurrentState is LineDrawingState)
            {
                //Console.WriteLine("Drawing Connection Points...");
                // Check which shapes should show connection points
                foreach (var shape in _model.GetShapes())
                {
                    if (shape.GetShapeType() != "Line")
                    {
                        // Only show connection points if the presenter determines it should
                        if (_presenter.ShouldShowConnectionPoints(shape, this.drawingPanel.PointToClient(Cursor.Position)))
                        {
                            var points = new Point[]
                            {
                                new Point((int)(shape.PositionX + shape.Width / 2), (int)shape.PositionY),                   // Top
                                new Point((int)(shape.PositionX + shape.Width / 2), (int)(shape.PositionY + shape.Height)),  // Bottom
                                new Point((int)shape.PositionX, (int)(shape.PositionY + shape.Height / 2)),                 // Left
                                new Point((int)(shape.PositionX + shape.Width), (int)(shape.PositionY + shape.Height / 2))  // Right
                            };

                            // Draw connection points
                            foreach (var point in points)
                            {
                                e.Graphics.FillEllipse(
                                    Brushes.Blue,  // Blue dot
                                    point.X - 5,   // Center X coordinate 
                                    point.Y - 5,   // Center Y coordinate
                                    10,            // Width
                                    10             // Height
                                );
                            }
                        }

                    }

                }
            }
        }



        private void DrawingPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _presenter.HandleMouseDown(e.Location);
            }
        }

        private void DrawingPanel_MouseMove(object sender, MouseEventArgs e)
        {
            _presenter.HandleMouseMove(e.Location);
            _presenter.HandleMouseHover(e.Location);
        }

        private void DrawingPanel_MouseUp(object sender, MouseEventArgs e)
        {
            _presenter.HandleMouseUp(e.Location);
        }

        private void DrawingPanel_MouseEnter(object sender, EventArgs e)
        {
            drawingPanel.Cursor = _presenter.CurrentCursor;
        }


        private void DrawingPanel_MouseLeave(object sender, EventArgs e)
        {
            drawingPanel.Cursor = Cursors.Default;
        }

        private void ValidateInput()
        {
            _presenter.ValidateInputs(
                comboBoxShapeType.SelectedItem?.ToString(),
                wordText.Text,
                XText.Text,
                YText.Text,
                HText.Text,
                WText.Text
            );

            // Update label colors
            UpdateLabelColor(labelWord, wordText.Text);
            UpdateLabelColor(labelX, XText.Text);
            UpdateLabelColor(labelY, YText.Text);
            UpdateLabelColor(labelH, HText.Text);
            UpdateLabelColor(labelW, WText.Text);
        }

        private void UpdateLabelColor(Label label, string text)
        {
            if (label.Name == "labelWord" || label == labelWord)
            {
                label.ForeColor = string.IsNullOrEmpty(text)
                    ? Color.Red
                    : Color.Black;
            }
            else
            {
                label.ForeColor = string.IsNullOrEmpty(text) ||
                             !float.TryParse(text, out float value) ||
                             value <= 0
                ? Color.Red
                : Color.Black;
            }

        }

        private void Model_ShapesChanged(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Model_ShapesChanged(sender, e)));
                return;
            }

            RefreshDataGridView();
            drawingPanel.Refresh();
        }

        private void RefreshDataGridView()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(RefreshDataGridView));
                return;
            }

            shapeDataGridView.Rows.Clear();
            foreach (var shape in _model.GetShapes())
            {
                shapeDataGridView.Rows.Add(
                    "刪除",
                    shape.Id,
                    shape.GetShapeType(),
                    shape.Text,
                    shape.PositionX,
                    shape.PositionY,
                    shape.Height,
                    shape.Width
                );
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            //Console.WriteLine("add button click");
            string shapeType = comboBoxShapeType.SelectedItem?.ToString();
            string text = wordText.Text;

            if (!float.TryParse(XText.Text, out float positionX) ||
                !float.TryParse(YText.Text, out float positionY) ||
                !float.TryParse(HText.Text, out float height) ||
                !float.TryParse(WText.Text, out float width) ||
                string.IsNullOrEmpty(shapeType) ||
                string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Please fill in all fields with valid values.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (positionX < 0 || positionY < 0 || height <= 0 || width <= 0)
            {
                MessageBox.Show("The fields must be positive numbers.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            IShape newShape = ShapeFactory.CreateShape(shapeType);
            newShape.Id = _model.GenerateNewId(); // 或使用其他生成 ID 的機制
            newShape.Text = text;
            newShape.PositionX = positionX;
            newShape.PositionY = positionY;
            newShape.Height = height;
            newShape.Width = width;
            newShape.SetPresenter(_presenter);

            // 初始化文字位置
            InitializeTextPosition(newShape);

            var addCommand = new DrawShapeCommand(_model, newShape);
            _presenter.ExecuteCommand(addCommand);

            _presenter.SelectedShape = newShape;

            // 重置工具狀態和游標
            _presenter.ResetToolState();

            ClearInputFields();
        }

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

        // clear the textBox
        private void ClearInputFields()
        {
            comboBoxShapeType.SelectedIndex = -1;
            wordText.Clear();
            XText.Clear();
            YText.Clear();
            HText.Clear();
            WText.Clear();
        }

        // delete the shape based on id column
        private void ShapeDataGridView_DeleteShape(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == shapeDataGridView.Columns["DeleteButton"].Index && e.RowIndex >= 0)
            {
                int id = (int)shapeDataGridView.Rows[e.RowIndex].Cells["IDColumn"].Value;
                IShape shapeToDelete = _model.GetShapes().FirstOrDefault(s => s.Id == id);

                if (shapeToDelete != null)
                {
                    // Create a new delete command and execute it
                    var deleteCommand = new DeleteShapeCommand(_model, shapeToDelete);
                    _presenter.ExecuteCommand(deleteCommand);
                    drawingPanel.Refresh();
                }
            }
        }

        public string ShowTextEditDialog(string currentText)
        {
            using (var dialog = new TextEditDialog(currentText))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return dialog.TextValue;
                }
                return currentText;
            }
        }
    }
}