
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;
using System.Drawing;
using System;
using System.Threading;
using System.Linq;
using System.IO;
using MyDrawing;
using System.Reflection;
using MyDrawing.Shapes;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyDrawing.Command;
using static DecisionShape;

namespace MyDrawingTests
{
    [TestClass]
    public class GUITest
    {
        private MyDrawing.MyDrawing _form;
        private Thread _uiThread;
        private const int WAIT_TIME = 1000;

        [TestInitialize]
        public void Setup()
        {
            _uiThread = new Thread(() =>
            {
                _form = new MyDrawing.MyDrawing();
                Application.Run(_form);
            });
            _uiThread.SetApartmentState(ApartmentState.STA);
            _uiThread.Start();
            Thread.Sleep(1000);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_form != null)
            {
                if (_form.InvokeRequired)
                    _form.Invoke(new Action(() => _form.Close()));
                else
                    _form.Close();
            }
            if (_uiThread != null && _uiThread.IsAlive)
                _uiThread.Join(1000);
        }

        [TestMethod]
        public void TestToolbarButtonSelection()
        {
            InvokeOnForm(() =>
            {
                var toolStrip = _form.Controls.Find("shapeToolStrip", true)[0] as ToolStrip;
                var startButton = toolStrip.Items["startIcon"] as ToolStripButton;
                var processButton = toolStrip.Items["processIcon"] as ToolStripButton;
                var terminateButton = toolStrip.Items["terminatorIcon"] as ToolStripButton;
                var lineButton = toolStrip.Items["lineIcon"] as ToolStripButton;
                var cursorButton = toolStrip.Items["cursorIcon"] as ToolStripButton;
                var decisionButton = toolStrip.Items["decisionIcon"] as ToolStripButton;

                startButton.PerformClick();
                Thread.Sleep(WAIT_TIME);
                Assert.IsTrue(startButton.Checked);
                Assert.IsFalse(processButton.Checked);
                Assert.IsFalse(terminateButton.Checked);
                Assert.IsFalse(lineButton.Checked);
                Assert.IsFalse(cursorButton.Checked);
                Assert.IsFalse(decisionButton.Checked);
            });
        }

        [TestMethod]
        public void TestDrawShapes()
        {
            var shapeTests = new[]
            {
                ("startIcon", "Start", new Point(100, 100), new Point(200, 200)),
                ("processIcon", "Process", new Point(150, 150), new Point(250, 250)),
                ("decisionIcon", "Decision", new Point(200, 200), new Point(300, 300)),
                ("terminatorIcon", "Terminator", new Point(250, 250), new Point(350, 350))
            };

            foreach (var (buttonName, shapeType, startPoint, endPoint) in shapeTests)
            {
                InvokeOnForm(() =>
                {
                    // Click shape button
                    var toolStrip = _form.Controls.Find("shapeToolStrip", true)[0] as ToolStrip;
                    var shapeButton = toolStrip.Items[buttonName] as ToolStripButton;
                    shapeButton.PerformClick();
                    Thread.Sleep(WAIT_TIME);

                    // Verify button states
                    foreach (ToolStripButton button in toolStrip.Items)
                    {
                        if (button.Name == buttonName)
                            Assert.IsTrue(button.Checked, $"{button.Name} should be checked");
                        else if (button is ToolStripButton)
                            Assert.IsFalse(button.Checked, $"{button.Name} should not be checked");
                    }

                    // Draw shape
                    var drawingPanel = _form.Controls.Find("drawingPanel", true)[0] as Panel;
                    SimulateMouseDrawing(drawingPanel, startPoint, endPoint);
                    Thread.Sleep(WAIT_TIME);

                    // Verify shape in grid
                    var dataGridView = _form.Controls.Find("shapeDataGridView", true)[0] as DataGridView;
                    var lastRow = dataGridView.Rows[dataGridView.Rows.Count - 1];
                    Assert.AreEqual(shapeType, lastRow.Cells["ShapeColumn"].Value);
                    Assert.AreEqual(Math.Min(startPoint.X, endPoint.X), (float)lastRow.Cells["XColumn"].Value);
                    Assert.AreEqual(Math.Min(startPoint.Y, endPoint.Y), (float)lastRow.Cells["YColumn"].Value);
                });
            }
        }

        [TestMethod]
        public void TestDrawLine()
        {
            InvokeOnForm(() =>
            {
                // Create two shapes to connect
                CreateShapeViaForm("Process", "Shape1", 100, 100, 50, 50);
                CreateShapeViaForm("Process", "Shape2", 300, 100, 50, 50);
                Thread.Sleep(WAIT_TIME);

                // Switch to line mode
                var toolStrip = _form.Controls.Find("shapeToolStrip", true)[0] as ToolStrip;
                var lineButton = toolStrip.Items["lineIcon"] as ToolStripButton;
                lineButton.PerformClick();
                Thread.Sleep(WAIT_TIME);

                // Draw line between shapes
                var drawingPanel = _form.Controls.Find("drawingPanel", true)[0] as Panel;
                SimulateMouseDrag(drawingPanel, new Point(125, 100), new Point(325, 100));
                Thread.Sleep(WAIT_TIME);

                // Verify line in grid
                var dataGridView = _form.Controls.Find("shapeDataGridView", true)[0] as DataGridView;
                var lastRow = dataGridView.Rows[dataGridView.Rows.Count - 1];
                Assert.AreEqual("Line", lastRow.Cells["ShapeColumn"].Value);
            });
        }

        [TestMethod]
        public void TestUndoRedo()
        {
            InvokeOnForm(() =>
            {
                // Draw shape
                var toolStrip = _form.Controls.Find("shapeToolStrip", true)[0] as ToolStrip;
                var startButton = toolStrip.Items["startIcon"] as ToolStripButton;
                startButton.PerformClick();
                Thread.Sleep(WAIT_TIME);

                var drawingPanel = _form.Controls.Find("drawingPanel", true)[0] as Panel;
                SimulateMouseDrawing(drawingPanel, new Point(100, 100), new Point(200, 200));

                var dataGridView = _form.Controls.Find("shapeDataGridView", true)[0] as DataGridView;
                int initialCount = dataGridView.Rows.Count;

                // Test Undo
                var undoButton = toolStrip.Items["undoIcon"] as ToolStripButton;
                undoButton.PerformClick();
                Thread.Sleep(WAIT_TIME);
                Assert.AreEqual(initialCount - 1, dataGridView.Rows.Count);

                // Test Redo
                var redoButton = toolStrip.Items["redoIcon"] as ToolStripButton;
                redoButton.PerformClick();
                Thread.Sleep(WAIT_TIME);
                Assert.AreEqual(initialCount, dataGridView.Rows.Count);
            });
        }

        [TestMethod]
        public void TestDataGridViewAddShape_Process()
        {
            InvokeOnForm(() =>
            {
                var comboBox = _form.Controls.Find("comboBoxShapeType", true)[0] as ComboBox;
                var wordText = _form.Controls.Find("wordText", true)[0] as TextBox;
                var xText = _form.Controls.Find("XText", true)[0] as TextBox;
                var yText = _form.Controls.Find("YText", true)[0] as TextBox;
                var hText = _form.Controls.Find("HText", true)[0] as TextBox;
                var wText = _form.Controls.Find("WText", true)[0] as TextBox;
                var addButton = _form.Controls.Find("addButton", true)[0] as Button;
                var dataGridView = _form.Controls.Find("shapeDataGridView", true)[0] as DataGridView;

                comboBox.SelectedItem = "Process";
                Thread.Sleep(WAIT_TIME);
                wordText.Text = "Test Shape";
                xText.Text = "150";
                yText.Text = "150";
                hText.Text = "100";
                wText.Text = "100";
                Thread.Sleep(WAIT_TIME);

                int initialCount = dataGridView.Rows.Count;
                addButton.PerformClick();
                Thread.Sleep(WAIT_TIME);

                Assert.AreEqual(initialCount + 1, dataGridView.Rows.Count);
                Assert.AreEqual("Process", dataGridView.Rows[dataGridView.Rows.Count - 1].Cells["ShapeColumn"].Value);
            });
        }

        [TestMethod]
        public void TestDataGridViewAddShape_Terminator()
        {
            InvokeOnForm(() =>
            {
                var comboBox = _form.Controls.Find("comboBoxShapeType", true)[0] as ComboBox;
                var wordText = _form.Controls.Find("wordText", true)[0] as TextBox;
                var xText = _form.Controls.Find("XText", true)[0] as TextBox;
                var yText = _form.Controls.Find("YText", true)[0] as TextBox;
                var hText = _form.Controls.Find("HText", true)[0] as TextBox;
                var wText = _form.Controls.Find("WText", true)[0] as TextBox;
                var addButton = _form.Controls.Find("addButton", true)[0] as Button;
                var dataGridView = _form.Controls.Find("shapeDataGridView", true)[0] as DataGridView;

                comboBox.SelectedItem = "Terminator";
                Thread.Sleep(WAIT_TIME);
                wordText.Text = "Test Shape";
                xText.Text = "150";
                yText.Text = "150";
                hText.Text = "100";
                wText.Text = "100";
                Thread.Sleep(WAIT_TIME);

                int initialCount = dataGridView.Rows.Count;
                addButton.PerformClick();
                Thread.Sleep(WAIT_TIME);

                Assert.AreEqual(initialCount + 1, dataGridView.Rows.Count);
                Assert.AreEqual("Terminator", dataGridView.Rows[dataGridView.Rows.Count - 1].Cells["ShapeColumn"].Value);
            });
        }

        [TestMethod]
        public void TestDataGridViewAddShape_start()
        {
            InvokeOnForm(() =>
            {
                var comboBox = _form.Controls.Find("comboBoxShapeType", true)[0] as ComboBox;
                var wordText = _form.Controls.Find("wordText", true)[0] as TextBox;
                var xText = _form.Controls.Find("XText", true)[0] as TextBox;
                var yText = _form.Controls.Find("YText", true)[0] as TextBox;
                var hText = _form.Controls.Find("HText", true)[0] as TextBox;
                var wText = _form.Controls.Find("WText", true)[0] as TextBox;
                var addButton = _form.Controls.Find("addButton", true)[0] as Button;
                var dataGridView = _form.Controls.Find("shapeDataGridView", true)[0] as DataGridView;

                comboBox.SelectedItem = "Start";
                Thread.Sleep(WAIT_TIME);
                wordText.Text = "Test Shape";
                xText.Text = "150";
                yText.Text = "150";
                hText.Text = "100";
                wText.Text = "100";
                Thread.Sleep(WAIT_TIME);

                int initialCount = dataGridView.Rows.Count;
                addButton.PerformClick();
                Thread.Sleep(WAIT_TIME);

                Assert.AreEqual(initialCount + 1, dataGridView.Rows.Count);
                Assert.AreEqual("Start", dataGridView.Rows[dataGridView.Rows.Count - 1].Cells["ShapeColumn"].Value);
            });
        }

        [TestMethod]
        public void TestDataGridViewAddShape_Decision()
        {
            InvokeOnForm(() =>
            {
                var comboBox = _form.Controls.Find("comboBoxShapeType", true)[0] as ComboBox;
                var wordText = _form.Controls.Find("wordText", true)[0] as TextBox;
                var xText = _form.Controls.Find("XText", true)[0] as TextBox;
                var yText = _form.Controls.Find("YText", true)[0] as TextBox;
                var hText = _form.Controls.Find("HText", true)[0] as TextBox;
                var wText = _form.Controls.Find("WText", true)[0] as TextBox;
                var addButton = _form.Controls.Find("addButton", true)[0] as Button;
                var dataGridView = _form.Controls.Find("shapeDataGridView", true)[0] as DataGridView;

                comboBox.SelectedItem = "Decision";
                Thread.Sleep(WAIT_TIME);
                wordText.Text = "Test Shape";
                xText.Text = "150";
                yText.Text = "150";
                hText.Text = "100";
                wText.Text = "100";
                Thread.Sleep(WAIT_TIME);

                int initialCount = dataGridView.Rows.Count;
                addButton.PerformClick();
                Thread.Sleep(WAIT_TIME);

                Assert.AreEqual(initialCount + 1, dataGridView.Rows.Count);
                Assert.AreEqual("Decision", dataGridView.Rows[dataGridView.Rows.Count - 1].Cells["ShapeColumn"].Value);
            });
        }

        [TestMethod]
        public void TestDragShape()
        {
            InvokeOnForm(() =>
            {
                // Create shape first
                var comboBox = _form.Controls.Find("comboBoxShapeType", true)[0] as ComboBox;
                comboBox.SelectedItem = "Process";
                FillShapeForm("Test Shape", "100", "100", "50", "50");
                var addButton = _form.Controls.Find("addButton", true)[0] as Button;
                addButton.PerformClick();
                Thread.Sleep(WAIT_TIME);

                // Switch to selection mode
                var toolStrip = _form.Controls.Find("shapeToolStrip", true)[0] as ToolStrip;
                var cursorButton = toolStrip.Items["cursorIcon"] as ToolStripButton;
                cursorButton.PerformClick();
                Thread.Sleep(WAIT_TIME);

                // Perform drag
                var drawingPanel = _form.Controls.Find("drawingPanel", true)[0] as Panel;
                SimulateMouseDrag(drawingPanel, new Point(125, 125), new Point(200, 200));

                // Verify new position
                var dataGridView = _form.Controls.Find("shapeDataGridView", true)[0] as DataGridView;
                var lastRow = dataGridView.Rows[dataGridView.Rows.Count - 1];
                Assert.AreEqual(175f, (float)lastRow.Cells["XColumn"].Value);
                Assert.AreEqual(175f, (float)lastRow.Cells["YColumn"].Value);
            });
        }

        [TestMethod]
        public void TestDragText()
        {
            InvokeOnForm(() =>
            {
                // Create a shape first
                var comboBox = _form.Controls.Find("comboBoxShapeType", true)[0] as ComboBox;
                comboBox.SelectedItem = "Process";
                FillShapeForm("Test Text", "100", "100", "50", "50");
                var addButton = _form.Controls.Find("addButton", true)[0] as Button;
                addButton.PerformClick();
                Thread.Sleep(WAIT_TIME);

                // Switch to selection mode
                var toolStrip = _form.Controls.Find("shapeToolStrip", true)[0] as ToolStrip;
                var cursorButton = toolStrip.Items["cursorIcon"] as ToolStripButton;
                cursorButton.PerformClick();
                Thread.Sleep(WAIT_TIME);

                // Get the shape and simulate text drag
                var drawingPanel = _form.Controls.Find("drawingPanel", true)[0] as Panel;
                var shape = _form.GetType().GetField("_model", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(_form) as MyDrawingModel;

                // Simulate text drag in the middle of the shape
                Point textCenter = new Point(125, 115);
                Point dragEnd = new Point(150, 150);
                SimulateMouseDrag(drawingPanel, textCenter, dragEnd);
                Thread.Sleep(WAIT_TIME);

                // Verify text position changed
                var lastShape = shape.GetShapes().Last();
                Assert.AreNotEqual(125, lastShape.TextPositionX);
                Assert.AreNotEqual(125, lastShape.TextPositionY);
            });
        }

        [TestMethod]
        public void TestEditText()
        {
            InvokeOnForm(() =>
            {
                // Create a shape first
                var comboBox = _form.Controls.Find("comboBoxShapeType", true)[0] as ComboBox;
                comboBox.SelectedItem = "Process";
                FillShapeForm("Original Text", "100", "100", "50", "50");
                var addButton = _form.Controls.Find("addButton", true)[0] as Button;
                addButton.PerformClick();
                Thread.Sleep(WAIT_TIME);

                // Switch to selection mode
                var toolStrip = _form.Controls.Find("shapeToolStrip", true)[0] as ToolStrip;
                var cursorButton = toolStrip.Items["cursorIcon"] as ToolStripButton;
                cursorButton.PerformClick();
                Thread.Sleep(WAIT_TIME);

                // Simulate double click on shape center
                var drawingPanel = _form.Controls.Find("drawingPanel", true)[0] as Panel;
                Point shapeCenter = new Point(125, 115);

                // Get the shape
                var model = _form.GetType().GetField("_model", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(_form) as MyDrawingModel;
                var shape = model.GetShapes().Last();

                // Simulate editing through TextEditDialog
                var dialog = new TextEditDialog(shape.Text);
                dialog.SetTextValue("Modified Text");

                // Verify text was changed
                Assert.AreEqual("Modified Text", dialog.TextValue);
            });
        }

        [TestMethod]
        public void TestSaveAndLoad()
        {
            InvokeOnForm(async () =>
            {
                // Create test shapes and line
                CreateShapeViaForm("Process", "Shape1", 100, 100, 50, 50);
                CreateShapeViaForm("Decision", "Shape2", 300, 300, 50, 50);
                DrawLine(new Point(125, 100), new Point(325, 300));
                Thread.Sleep(WAIT_TIME);

                var initialShapesCount = GetShapeCount();
                string tempFile = Path.GetTempFileName();

                // Test save
                var toolStrip = _form.Controls.Find("shapeToolStrip", true)[0] as ToolStrip;
                var saveButton = toolStrip.Items["saveIcon"] as ToolStripButton;
                //saveButton.PerformClick();
                //Thread.Sleep(WAIT_TIME);

                Assert.IsTrue(saveButton.Enabled, "Save button should be enabled before saving");
                await SaveDrawing(tempFile);
                Assert.IsFalse(saveButton.Enabled, "Save button should be disabled during save");
                Thread.Sleep(WAIT_TIME * 2);
                Assert.IsTrue(saveButton.Enabled, "Save button should be enabled after save");

                // Verify UI responsiveness during save
                var drawingPanel = _form.Controls.Find("drawingPanel", true)[0] as Panel;
                Assert.IsTrue(drawingPanel.Enabled, "Drawing panel should be enabled during save");

                // Delete all shapes
                DeleteAllShapes();
                Assert.AreEqual(0, GetShapeCount(), "All shapes should be deleted");

                //var loadButton = toolStrip.Items["loadIcon"] as ToolStripButton;
                //loadButton.PerformClick();
                //Thread.Sleep(WAIT_TIME);

                // Test load
                LoadDrawing(tempFile);
                Thread.Sleep(WAIT_TIME);

                Assert.AreEqual(initialShapesCount, GetShapeCount(), "Should restore original number of shapes");

                // Verify loaded shapes
                var shapes = GetAllShapes();
                Assert.IsTrue(shapes.Any(s => s.GetShapeType() == "Process" && s.Text == "Shape1"));
                Assert.IsTrue(shapes.Any(s => s.GetShapeType() == "Decision" && s.Text == "Shape2"));
                Assert.IsTrue(shapes.Any(s => s.GetShapeType() == "Line"));

                File.Delete(tempFile);
            });
        }

        [TestMethod]
        public void TestAutoSave()
        {
            InvokeOnForm(async () =>
            {
                // Create test shape
                var comboBox = _form.Controls.Find("comboBoxShapeType", true)[0] as ComboBox;
                comboBox.SelectedItem = "Process";
                FillShapeForm("Auto Save Test", "100", "100", "50", "50");
                var addButton = _form.Controls.Find("addButton", true)[0] as Button;
                addButton.PerformClick();

                // Wait for auto save (30 seconds)
                Thread.Sleep(31000); // Wait slightly longer than 30 seconds

                // Check backup folder exists
                string backupDir = Path.Combine(
                    Path.GetDirectoryName(Application.ExecutablePath),
                    "drawing_backup"
                );
                Assert.IsTrue(Directory.Exists(backupDir), "Backup directory should exist");

                // Check backup file exists
                var backupFiles = Directory.GetFiles(backupDir, "*_bak.mydrawing");
                Assert.IsTrue(backupFiles.Length > 0, "Should have at least one backup file");

                // Check form title for auto saving text
                Assert.IsTrue(_form.Text.Contains("Auto saving"), "Form title should show auto saving status");

                // Wait for auto save to complete
                Thread.Sleep(WAIT_TIME * 2);

                // Verify UI is responsive during auto save
                var drawingPanel = _form.Controls.Find("drawingPanel", true)[0] as Panel;
                Assert.IsTrue(drawingPanel.Enabled, "Drawing panel should be enabled during auto save");

                // Clean up backup files
                foreach (var file in backupFiles)
                {
                    File.Delete(file);
                }
                Directory.Delete(backupDir);
            });
        }

        [TestMethod]
        public void TestShapeMovementUndoRedo()
        {
            InvokeOnForm(() =>
            {
                // Create initial shape
                var comboBox = _form.Controls.Find("comboBoxShapeType", true)[0] as ComboBox;
                comboBox.SelectedItem = "Process";
                FillShapeForm("Test Shape", "100", "100", "50", "50");
                var addButton = _form.Controls.Find("addButton", true)[0] as Button;
                addButton.PerformClick();
                Thread.Sleep(WAIT_TIME);

                // Switch to selection mode
                var toolStrip = _form.Controls.Find("shapeToolStrip", true)[0] as ToolStrip;
                var cursorButton = toolStrip.Items["cursorIcon"] as ToolStripButton;
                cursorButton.PerformClick();
                Thread.Sleep(WAIT_TIME);

                // Get initial position
                var dataGridView = _form.Controls.Find("shapeDataGridView", true)[0] as DataGridView;
                var initialX = (float)dataGridView.Rows[0].Cells["XColumn"].Value;
                var initialY = (float)dataGridView.Rows[0].Cells["YColumn"].Value;

                // Perform drag
                var drawingPanel = _form.Controls.Find("drawingPanel", true)[0] as Panel;
                SimulateMouseDrag(drawingPanel, new Point(125, 125), new Point(200, 200));
                Thread.Sleep(WAIT_TIME);

                // Get new position
                var newX = (float)dataGridView.Rows[0].Cells["XColumn"].Value;
                var newY = (float)dataGridView.Rows[0].Cells["YColumn"].Value;

                // Verify movement occurred
                Assert.AreNotEqual(initialX, newX, "Shape X position should have changed");
                Assert.AreNotEqual(initialY, newY, "Shape Y position should have changed");

                // Perform Undo
                var undoButton = toolStrip.Items["undoIcon"] as ToolStripButton;
                undoButton.PerformClick();
                Thread.Sleep(WAIT_TIME);

                // Verify position returned to initial
                var undoX = (float)dataGridView.Rows[0].Cells["XColumn"].Value;
                var undoY = (float)dataGridView.Rows[0].Cells["YColumn"].Value;
                Assert.AreEqual(initialX, undoX, "Shape X position should return to initial position");
                Assert.AreEqual(initialY, undoY, "Shape Y position should return to initial position");

                // Perform Redo
                var redoButton = toolStrip.Items["redoIcon"] as ToolStripButton;
                redoButton.PerformClick();
                Thread.Sleep(WAIT_TIME);

                // Verify position returned to moved position
                var redoX = (float)dataGridView.Rows[0].Cells["XColumn"].Value;
                var redoY = (float)dataGridView.Rows[0].Cells["YColumn"].Value;
                Assert.AreEqual(newX, redoX, "Shape X position should return to moved position");
                Assert.AreEqual(newY, redoY, "Shape Y position should return to moved position");
            });
        }

        [TestMethod]
        public void TestShapeDrawingUndoRedo()
        {
            InvokeOnForm(() =>
            {
                var model = GetModel();

                // 1. Add a shape
                CreateShapeViaForm("Process", "Test1", 100, 100, 50, 100);
                Assert.AreEqual(1, model.GetShapes().Count, "Shape should be added");

                // 2. Undo shape addition
                PerformUndo();
                Assert.AreEqual(0, model.GetShapes().Count, "Shape should be removed after undo");

                // 3. Redo shape addition
                PerformRedo();
                Assert.AreEqual(1, model.GetShapes().Count, "Shape should be restored after redo");
            });
        }

        [TestMethod]
        public void TestShapeMoveUndoRedo()
        {
            InvokeOnForm(() =>
            {
                // 1. Add a shape
                CreateShapeViaForm("Process", "Test1", 100, 100, 50, 100);
                var shape = GetLastShape();
                float originalX = shape.PositionX;
                float originalY = shape.PositionY;

                // 2. Move the shape
                SelectAndDragShape(new Point((int)originalX + 25, (int)originalY + 25),
                                 new Point((int)originalX + 75, (int)originalY + 75));
                Assert.AreNotEqual(originalX, GetLastShape().PositionX, "Shape X position should change");
                Assert.AreNotEqual(originalY, GetLastShape().PositionY, "Shape Y position should change");

                // 3. Undo move
                PerformUndo();
                Assert.AreEqual(originalX, GetLastShape().PositionX, "Shape X position should be restored");
                Assert.AreEqual(originalY, GetLastShape().PositionY, "Shape Y position should be restored");

                // 4. Redo move
                PerformRedo();
                Assert.AreEqual(originalX + 50, GetLastShape().PositionX, "Shape X position should be moved again");
                Assert.AreEqual(originalY + 50, GetLastShape().PositionY, "Shape Y position should be moved again");
            });
        }

        [TestMethod]
        public void TestTextModificationUndoRedo()
        {
            InvokeOnForm(() =>
            {
                // 1. Add a shape with initial text
                CreateShapeViaForm("Process", "InitialText", 100, 100, 50, 100);
                var shape = GetLastShape();

                // 2. Get the presenter and model through reflection
                var presenter = _form.GetType()
                    .GetField("_presenter", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(_form) as MyDrawingPresenter;

                var model = GetModel();

                // 3. Select the shape
                presenter.SelectedShape = shape;

                // 4. Simulate text modification through ModifyTextCommand
                string newText = "ModifiedText";
                presenter.ExecuteCommand(new ModifyTextCommand(model, shape as Shape, shape.Text, newText));
                Thread.Sleep(WAIT_TIME);

                Assert.AreEqual(newText, GetLastShape().Text, "Text should be modified");

                // 5. Undo text modification
                PerformUndo();
                Assert.AreEqual("InitialText", GetLastShape().Text, "Text should be restored after undo");

                // 6. Redo text modification
                PerformRedo();
                Assert.AreEqual(newText, GetLastShape().Text, "Text should be modified again after redo");
            });
        }

        [TestMethod]
        public void TestTextModificationWithOrangeDot()
        {
            InvokeOnForm(() =>
            {
                // 1. Create a test shape
                CreateShapeViaForm("Process", "Original Text", 100, 100, 50, 100);
                Thread.Sleep(WAIT_TIME);

                // 2. Get required objects through reflection
                var presenter = _form.GetType()
                    .GetField("_presenter", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(_form) as MyDrawingPresenter;

                var model = GetModel();
                var shape = GetLastShape();

                // 3. Switch to selection mode and select shape
                var toolStrip = _form.Controls.Find("shapeToolStrip", true)[0] as ToolStrip;
                var cursorButton = toolStrip.Items["cursorIcon"] as ToolStripButton;
                cursorButton.PerformClick();
                Thread.Sleep(WAIT_TIME);

                presenter.SelectedShape = shape;

                // 4. Directly simulate text modification command
                string newText = "Modified Text";
                presenter.ExecuteCommand(new ModifyTextCommand(model, shape as Shape, shape.Text, newText));
                Thread.Sleep(WAIT_TIME);

                // 5. Verify text was modified
                Assert.AreEqual(newText, GetLastShape().Text, "Text should be modified");

                // Verify undo/redo still works
                PerformUndo();
                Assert.AreEqual("Original Text", GetLastShape().Text, "Text should be restored after undo");

                PerformRedo();
                Assert.AreEqual(newText, GetLastShape().Text, "Text should be modified again after redo");
            });
        }

        [TestMethod]
        public void TestTextMoveUndoRedo()
        {
            InvokeOnForm(() =>
            {
                // 1. Add a shape
                CreateShapeViaForm("Process", "Test1", 100, 100, 50, 50);
                var shape = GetLastShape();
                float originalTextX = shape.TextPositionX;
                float originalTextY = shape.TextPositionY;

                // 2. Move text by simulating drag on text position
                var drawingPanel = _form.Controls.Find("drawingPanel", true)[0] as Panel;
                Point textStart = new Point(125, 115);
                Point textEnd = new Point(130, 120);
                SimulateMouseDrag(drawingPanel, textStart, textEnd);
                Thread.Sleep(WAIT_TIME);

                Assert.AreNotEqual(originalTextX, GetLastShape().TextPositionX, "Text X position should change");
                Assert.AreNotEqual(originalTextY, GetLastShape().TextPositionY, "Text Y position should change");

                // 3. Undo text move
                PerformUndo();
                Assert.AreEqual(originalTextX, GetLastShape().TextPositionX, "Text X position should be restored");
                Assert.AreEqual(originalTextY, GetLastShape().TextPositionY, "Text Y position should be restored");

                // 4. Redo text move
                PerformRedo();
                var movedShape = GetLastShape();
                Assert.AreNotEqual(originalTextX, movedShape.TextPositionX, "Text X position should be moved again");
                Assert.AreNotEqual(originalTextY, movedShape.TextPositionY, "Text Y position should be moved again");
            });
        }

        [TestMethod]
        public void TestLineDrawingUndoRedo()
        {
            InvokeOnForm(() =>
            {
                // 1. Add two shapes for line connection
                CreateShapeViaForm("Process", "Shape1", 100, 100, 50, 50);
                CreateShapeViaForm("Process", "Shape2", 300, 100, 50, 50);
                var model = GetModel();
                Assert.AreEqual(2, model.GetShapes().Count, "Two shapes should be added");

                // 2. Draw line between shapes
                DrawLine(new Point(125, 100), new Point(325, 100));
                Assert.AreEqual(3, model.GetShapes().Count, "Line should be added");
                Assert.IsTrue(model.GetShapes().Any(s => s.GetShapeType() == "Line"), "Line shape should exist");

                // 3. Undo line drawing
                PerformUndo();
                Assert.AreEqual(2, model.GetShapes().Count, "Line should be removed after undo");
                Assert.IsFalse(model.GetShapes().Any(s => s.GetShapeType() == "Line"),
                              "Line shape should not exist after undo");

                // 4. Redo line drawing
                PerformRedo();
                Assert.AreEqual(3, model.GetShapes().Count, "Line should be restored after redo");
                Assert.IsTrue(model.GetShapes().Any(s => s.GetShapeType() == "Line"),
                             "Line shape should exist after redo");
            });
        }

        

        [TestMethod]
        public void TestTextModificationCancellation()
        {
            InvokeOnForm(() =>
            {
                // 1. Create a test shape
                CreateShapeViaForm("Process", "Original Text", 100, 100, 50, 100);
                Thread.Sleep(WAIT_TIME);

                // 2. Switch to selection mode
                var toolStrip = _form.Controls.Find("shapeToolStrip", true)[0] as ToolStrip;
                var cursorButton = toolStrip.Items["cursorIcon"] as ToolStripButton;
                cursorButton.PerformClick();
                Thread.Sleep(WAIT_TIME);

                // 3. Get the shape and calculate orange dot position
                var shape = GetLastShape();
                using (Font font = new Font("Arial", 10))
                using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    SizeF textSize = g.MeasureString("Original Text", font);
                    Point orangeDotLocation = new Point(
                        (int)(shape.TextPositionX + (textSize.Width / 2)),
                        (int)(shape.TextPositionY - 8)
                    );

                    // 4. Select the shape
                    var drawingPanel = _form.Controls.Find("drawingPanel", true)[0] as Panel;
                    SimulateMouseDrawing(drawingPanel, new Point((int)shape.PositionX + 10, (int)shape.PositionY + 10),
                        new Point((int)shape.PositionX + 10, (int)shape.PositionY + 10));
                    Thread.Sleep(WAIT_TIME);

                    // 5. Simulate double-click on orange dot
                    SimulateMouseDrawing(drawingPanel, orangeDotLocation, orangeDotLocation);
                    Thread.Sleep(100);
                    SimulateMouseDrawing(drawingPanel, orangeDotLocation, orangeDotLocation);
                    Thread.Sleep(WAIT_TIME);

                    // 6. Create and simulate text edit dialog with cancellation
                    var dialog = new TextEditDialog(shape.Text);
                    dialog.DialogResult = DialogResult.Cancel;
                    Thread.Sleep(WAIT_TIME);

                    // 7. Verify text remains unchanged
                    Assert.AreEqual("Original Text", GetLastShape().Text);
                }
            });
        }

        // Helper methods
        private void CreateShapeViaForm(string shapeType, string text, float x, float y, float height, float width)
        {
            var comboBox = _form.Controls.Find("comboBoxShapeType", true)[0] as ComboBox;
            comboBox.SelectedItem = shapeType;
            FillShapeForm(text, x.ToString(), y.ToString(), height.ToString(), width.ToString());
            var addButton = _form.Controls.Find("addButton", true)[0] as Button;
            addButton.PerformClick();
            Thread.Sleep(WAIT_TIME);
        }

        private void CreateShapeViaDrawing(string shapeType, Point start, Point end)
        {
            var toolStrip = _form.Controls.Find("shapeToolStrip", true)[0] as ToolStrip;
            var button = toolStrip.Items[$"{shapeType.ToLower()}Icon"] as ToolStripButton;
            button.PerformClick();
            Thread.Sleep(WAIT_TIME);

            var drawingPanel = _form.Controls.Find("drawingPanel", true)[0] as Panel;
            SimulateMouseDrawing(drawingPanel, start, end);
            Thread.Sleep(WAIT_TIME);
        }

        private void DrawLine(Point start, Point end)
        {
            var toolStrip = _form.Controls.Find("shapeToolStrip", true)[0] as ToolStrip;
            var lineButton = toolStrip.Items["lineIcon"] as ToolStripButton;
            lineButton.PerformClick();
            Thread.Sleep(WAIT_TIME);

            var drawingPanel = _form.Controls.Find("drawingPanel", true)[0] as Panel;
            SimulateMouseDrag(drawingPanel, start, end);
            Thread.Sleep(WAIT_TIME);
        }

        private void SelectAndDragShape(Point start, Point end)
        {
            var toolStrip = _form.Controls.Find("shapeToolStrip", true)[0] as ToolStrip;
            var cursorButton = toolStrip.Items["cursorIcon"] as ToolStripButton;
            cursorButton.PerformClick();
            Thread.Sleep(WAIT_TIME);

            var drawingPanel = _form.Controls.Find("drawingPanel", true)[0] as Panel;
            SimulateMouseDrag(drawingPanel, start, end);
            Thread.Sleep(WAIT_TIME);
        }

        private void SimulateTextEdit(string newText)
        {
            var dialog = new TextEditDialog(GetLastShape().Text);
            dialog.SetTextValue(newText);
            Thread.Sleep(WAIT_TIME);
        }

        private void PerformUndo()
        {
            var toolStrip = _form.Controls.Find("shapeToolStrip", true)[0] as ToolStrip;
            var undoButton = toolStrip.Items["undoIcon"] as ToolStripButton;
            undoButton.PerformClick();
            Thread.Sleep(WAIT_TIME);
        }

        private void PerformRedo()
        {
            var toolStrip = _form.Controls.Find("shapeToolStrip", true)[0] as ToolStrip;
            var redoButton = toolStrip.Items["redoIcon"] as ToolStripButton;
            redoButton.PerformClick();
            Thread.Sleep(WAIT_TIME);
        }

        private async Task SaveDrawing(string filePath)
        {
            var model = GetModel();
            await model.SaveShapesAsync(filePath);
        }

        private void LoadDrawing(string filePath)
        {
            var model = GetModel();
            model.LoadShapes(filePath);
        }

        private void DeleteAllShapes()
        {
            while (GetShapeCount() > 0)
            {
                var dataGridView = _form.Controls.Find("shapeDataGridView", true)[0] as DataGridView;
                SimulateDataGridViewCellClick(dataGridView, 0, "DeleteButton");
                Thread.Sleep(WAIT_TIME);
            }
        }

        private MyDrawingModel GetModel()
        {
            return _form.GetType().GetField("_model", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(_form) as MyDrawingModel;
        }

        private IShape GetLastShape()
        {
            return GetModel().GetShapes().LastOrDefault();
        }

        private int GetShapeCount()
        {
            return GetModel().GetShapes().Count;
        }

        private List<IShape> GetAllShapes()
        {
            return GetModel().GetShapes();
        }

        private void FillShapeForm(string text, string x, string y, string h, string w)
        {
            var wordText = _form.Controls.Find("wordText", true)[0] as TextBox;
            var xText = _form.Controls.Find("XText", true)[0] as TextBox;
            var yText = _form.Controls.Find("YText", true)[0] as TextBox;
            var hText = _form.Controls.Find("HText", true)[0] as TextBox;
            var wText = _form.Controls.Find("WText", true)[0] as TextBox;

            wordText.Text = text;
            xText.Text = x;
            yText.Text = y;
            hText.Text = h;
            wText.Text = w;
            Thread.Sleep(WAIT_TIME);
        }

        private void SimulateMouseDrawing(Control control, Point start, Point end)
        {
            SendMouseEvent(control, start, MouseButtons.Left, true);
            Thread.Sleep(WAIT_TIME);
            SendMouseEvent(control, end, MouseButtons.Left, false);
            Thread.Sleep(WAIT_TIME);
        }

        private void SimulateMouseDrag(Control control, Point start, Point end)
        {
            // 滑鼠按下
            SendMouseEvent(control, start, MouseButtons.Left, true);
            Thread.Sleep(WAIT_TIME);

            // 模擬多個移動點
            int steps = 5;
            for (int i = 1; i <= steps; i++)
            {
                Point movePoint = new Point(
                    start.X + (end.X - start.X) * i / steps,
                    start.Y + (end.Y - start.Y) * i / steps
                );
                SimulateMouseMove(control, movePoint);
                Thread.Sleep(WAIT_TIME / 2);
            }

            // 滑鼠放開
            SendMouseEvent(control, end, MouseButtons.Left, false);
            Thread.Sleep(WAIT_TIME);
        }

        private void SimulateMouseMove(Control control, Point location)
        {
            MouseEventArgs args = new MouseEventArgs(MouseButtons.Left, 0, location.X, location.Y, 0);
            control.GetType().GetMethod("OnMouseMove",
                BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(control, new object[] { args });
        }

        private void SendMouseEvent(Control control, Point location, MouseButtons button, bool isDown)
        {
            MouseEventArgs args = new MouseEventArgs(button, 1, location.X, location.Y, 0);
            string methodName = isDown ? "OnMouseDown" : "OnMouseUp";
            control.GetType().GetMethod(methodName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(control, new object[] { args });
        }

        private void SimulateDataGridViewCellClick(DataGridView dgv, int rowIndex, string columnName)
        {
            var args = new DataGridViewCellEventArgs(dgv.Columns[columnName].Index, rowIndex);
            typeof(DataGridView).GetMethod("OnCellClick",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(dgv, new object[] { args });
        }

        private void InvokeOnForm(Action action)
        {
            if (_form.InvokeRequired)
                _form.Invoke(action);
            else
                action();
        }

        [TestMethod]
        public void TestDrawShapesFlowChart()
        {
            var shapeTests = new[]
            {
                ("startIcon", "Start", new Point(250, 50), new Point(350, 100)),
                ("processIcon", "Process", new Point(250, 150), new Point(350, 200)),
                ("decisionIcon", "Decision", new Point(200, 250), new Point(400, 280)),
                ("processIcon", "Process", new Point(150, 350), new Point(250, 400)),
                ("processIcon", "Process", new Point(350, 350), new Point(450, 400)),
                //("terminatorIcon", "Terminator", new Point(250, 450), new Point(350, 500))
            };

            foreach (var (buttonName, shapeType, startPoint, endPoint) in shapeTests)
            {
                InvokeOnForm(() =>
                {
                    // Click shape button
                    var toolStrip = _form.Controls.Find("shapeToolStrip", true)[0] as ToolStrip;
                    var shapeButton = toolStrip.Items[buttonName] as ToolStripButton;
                    shapeButton.PerformClick();
                    //Thread.Sleep(WAIT_TIME);

                    // Verify button states
                    foreach (ToolStripButton button in toolStrip.Items)
                    {
                        if (button.Name == buttonName)
                            Assert.IsTrue(button.Checked, $"{button.Name} should be checked");
                        else if (button is ToolStripButton)
                            Assert.IsFalse(button.Checked, $"{button.Name} should not be checked");
                    }

                    // Draw shape
                    var drawingPanel = _form.Controls.Find("drawingPanel", true)[0] as Panel;
                    SimulateMouseDrawing(drawingPanel, startPoint, endPoint);
                    //Thread.Sleep(WAIT_TIME);

                    // Verify shape in grid
                    var dataGridView = _form.Controls.Find("shapeDataGridView", true)[0] as DataGridView;
                    var lastRow = dataGridView.Rows[dataGridView.Rows.Count - 1];
                    Assert.AreEqual(shapeType, lastRow.Cells["ShapeColumn"].Value);
                    Assert.AreEqual(Math.Min(startPoint.X, endPoint.X), (float)lastRow.Cells["XColumn"].Value);
                    Assert.AreEqual(Math.Min(startPoint.Y, endPoint.Y), (float)lastRow.Cells["YColumn"].Value);
                });
            }
        }

        [TestMethod]
        public void TestDragShapeFlowChart()
        {
            InvokeOnForm(() =>
            {
                // Create shape first
                var comboBox = _form.Controls.Find("comboBoxShapeType", true)[0] as ComboBox;
                comboBox.SelectedItem = "Terminator";
                FillShapeForm("End", "250", "450", "50", "100");
                var addButton = _form.Controls.Find("addButton", true)[0] as Button;
                addButton.PerformClick();
                //Thread.Sleep(WAIT_TIME);

                // Switch to selection mode
                var toolStrip = _form.Controls.Find("shapeToolStrip", true)[0] as ToolStrip;
                var cursorButton = toolStrip.Items["cursorIcon"] as ToolStripButton;
                cursorButton.PerformClick();
                //Thread.Sleep(WAIT_TIME);

                // Perform drag
                var drawingPanel = _form.Controls.Find("drawingPanel", true)[0] as Panel;
                SimulateMouseDrag(drawingPanel, new Point(270, 460), new Point(270, 500));

                // Verify new position
                var dataGridView = _form.Controls.Find("shapeDataGridView", true)[0] as DataGridView;
                var lastRow = dataGridView.Rows[dataGridView.Rows.Count - 1];
                Assert.AreEqual(250f, (float)lastRow.Cells["XColumn"].Value);
                Assert.AreEqual(490f, (float)lastRow.Cells["YColumn"].Value);
            });
        }

        [TestMethod]
        public void TestDrawLineFlowChart()
        {
            InvokeOnForm(() =>
            {
                TestDrawShapesFlowChart();

                // Switch to line mode
                var toolStrip = _form.Controls.Find("shapeToolStrip", true)[0] as ToolStrip;
                var lineButton = toolStrip.Items["lineIcon"] as ToolStripButton;
                lineButton.PerformClick();
                //Thread.Sleep(WAIT_TIME);

                // Draw line between shapes
                var drawingPanel = _form.Controls.Find("drawingPanel", true)[0] as Panel;
                SimulateMouseDrag(drawingPanel, new Point(300, 100), new Point(300, 150));
                //Thread.Sleep(WAIT_TIME);

                // Verify line in grid
                var dataGridView = _form.Controls.Find("shapeDataGridView", true)[0] as DataGridView;
                var lastRow = dataGridView.Rows[dataGridView.Rows.Count - 1];
                Assert.AreEqual("Line", lastRow.Cells["ShapeColumn"].Value);

                // Switch to line mode
                lineButton.PerformClick();
                //Thread.Sleep(WAIT_TIME);

                // Draw line between shapes
                SimulateMouseDrag(drawingPanel, new Point(300, 200), new Point(300, 250));

                // Switch to line mode
                lineButton.PerformClick();

                // Draw line between shapes
                SimulateMouseDrag(drawingPanel, new Point(300, 280), new Point(200, 350));

                // Switch to line mode
                lineButton.PerformClick();

                // Draw line between shapes
                SimulateMouseDrag(drawingPanel, new Point(300, 280), new Point(400, 350));

                // Switch to line mode
                lineButton.PerformClick();

                // Draw line between shapes
                SimulateMouseDrag(drawingPanel, new Point(200, 400), new Point(300, 490));

                // Switch to line mode
                lineButton.PerformClick();

                // Draw line between shapes
                SimulateMouseDrag(drawingPanel, new Point(400, 400), new Point(300, 490));
            });
        }

        [TestMethod]
        public void UserScenario_BasicShapeDrawingAndEditing()
        {
            InvokeOnForm(() =>
            {
                // 測試繪製多個形狀
                //TestDrawShapesFlowChart();

                // 測試形狀拖曳
                TestDragShapeFlowChart();

                // 測試連線功能
                TestDrawLineFlowChart();

                // 測試移動已連線的形狀
                TestShapeMoveUndoRedo();

            });
        }

        // 使用者情境：文件操作與備份
        [TestMethod]
        public void UserScenario_FileOperationsAndBackup()
        {
            InvokeOnForm(async () =>
            {
                // 建立測試流程圖
                TestDrawShapes();
                TestDrawLine();

                // 測試存檔與讀檔
                TestSaveAndLoad();

                // 測試自動儲存
                TestAutoSave();
            });
        }

        // 使用者情境：詳細屬性編輯
        [TestMethod]
        public void UserScenario_DetailedPropertyEditing()
        {
            InvokeOnForm(() =>
            {
                // 測試透過表單新增各種形狀
                TestDataGridViewAddShape_Process();
                TestDataGridViewAddShape_Decision();
                TestDataGridViewAddShape_start();
                TestDataGridViewAddShape_Terminator();

                // 測試文字位置調整
                TestDragText();

                // 測試文字修改
                TestTextModificationWithOrangeDot();
            });
        }

        // 最上層整合測試：完整UI功能測試
        [TestMethod]
        public void CompleteUIIntegrationTest()
        {
            InvokeOnForm(async () =>
            {
                // 基本形狀繪製與編輯情境
                await Task.Run(() => UserScenario_BasicShapeDrawingAndEditing());
                Thread.Sleep(WAIT_TIME);

                // 文件操作與備份情境
                await Task.Run(() => UserScenario_FileOperationsAndBackup());
                Thread.Sleep(WAIT_TIME);

                // 詳細屬性編輯情境
                await Task.Run(() => UserScenario_DetailedPropertyEditing());
                Thread.Sleep(WAIT_TIME);

                // 最終驗證
                ValidateCompleteIntegration();
            });
        }

        private void ValidateCompleteIntegration()
        {
            var model = GetModel();

            // 驗證最終狀態
            Assert.IsTrue(model.GetShapes().Count > 0, "Should have shapes after all operations");

            // 驗證存檔功能
            string backupDir = Path.Combine(
                Path.GetDirectoryName(Application.ExecutablePath),
                "drawing_backup"
            );
            Assert.IsTrue(Directory.Exists(backupDir), "Backup directory should exist");

            // 驗證 UI 狀態
            var toolStrip = _form.Controls.Find("shapeToolStrip", true)[0] as ToolStrip;
            Assert.IsTrue(toolStrip.Items["undoIcon"].Enabled, "Undo should be available");

            // 驗證資料網格
            var dataGridView = _form.Controls.Find("shapeDataGridView", true)[0] as DataGridView;
            Assert.IsTrue(dataGridView.Rows.Count > 0, "DataGridView should contain shapes");
        }
    }
}