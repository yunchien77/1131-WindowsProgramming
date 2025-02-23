using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyDrawing.Command;
using MyDrawing.Shapes;
using System.Collections.Generic;
using System.Drawing;
using static DecisionShape;

namespace MyDrawing.Tests
{
    [TestClass]
    public class CommandTest
    {
        private MyDrawingModel _model;
        private IShape _shape;

        [TestInitialize]
        public void Setup()
        {
            _model = new MyDrawingModel();
            _shape = new ProcessShape { Id = 1, PositionX = 10, PositionY = 10, Width = 50, Height = 50, Text = "Test" };
        }

        [TestMethod]
        public void DrawShapeCommand_Execute_ShouldAddShapeToModel()
        {
            var command = new DrawShapeCommand(_model, _shape);
            command.Execute();
            Assert.IsTrue(_model.Shapes.Contains(_shape));
        }

        [TestMethod]
        public void DrawShapeCommand_Undo_ShouldRemoveShapeFromModel()
        {
            var command = new DrawShapeCommand(_model, _shape);
            command.Execute();
            command.Undo();
            Assert.IsFalse(_model.Shapes.Contains(_shape));
        }

        [TestMethod]
        public void DrawLineCommand_Execute_ShouldAddLineToModel()
        {
            var startShape = new ProcessShape { Id = 2 };
            var endShape = new ProcessShape { Id = 3 };
            var line = new LineShape { StartShape = startShape, EndShape = endShape };
            var command = new DrawLineCommand(_model, line, startShape, endShape);
            command.Execute();
            Assert.IsTrue(_model.Shapes.Contains(line));
        }

        [TestMethod]
        public void DrawLineCommand_Undo_ShouldRemoveLineFromModel()
        {
            var startShape = new ProcessShape { Id = 2 };
            var endShape = new ProcessShape { Id = 3 };
            var line = new LineShape { StartShape = startShape, EndShape = endShape };
            var command = new DrawLineCommand(_model, line, startShape, endShape);
            command.Execute();
            command.Undo();
            Assert.IsFalse(_model.Shapes.Contains(line));
        }

        [TestMethod]
        public void MoveShapeCommand_Execute_ShouldUpdateShapePosition()
        {
            var command = new MoveShapeCommand(_model, _shape, 10, 10, 20, 20, 50, 50, 100, 100);
            command.Execute();
            Assert.AreEqual(50, _shape.PositionX);
            Assert.AreEqual(50, _shape.PositionY);
        }

        [TestMethod]
        public void MoveShapeCommand_Undo_ShouldRevertShapePosition()
        {
            var command = new MoveShapeCommand(_model, _shape, 10, 10, 20, 20, 50, 50, 100, 100);
            command.Execute();
            command.Undo();
            Assert.AreEqual(10, _shape.PositionX);
            Assert.AreEqual(10, _shape.PositionY);
        }

        [TestMethod]
        public void ModifyTextCommand_Execute_ShouldChangeShapeText()
        {
            var command = new ModifyTextCommand(_model, (Shape)_shape, "Old Text", "New Text");
            command.Execute();
            Assert.AreEqual("New Text", _shape.Text);
        }

        [TestMethod]
        public void ModifyTextCommand_Undo_ShouldRevertShapeText()
        {
            var command = new ModifyTextCommand(_model, (Shape)_shape, "Old Text", "New Text");
            command.Execute();
            command.Undo();
            Assert.AreEqual("Old Text", _shape.Text);
        }

        [TestMethod]
        public void MoveTextCommand_Execute_ShouldChangeTextPosition()
        {
            var command = new MoveTextCommand(_model, _shape, 10, 10, 30, 30);
            command.Execute();
            Assert.AreEqual(30, _shape.TextPositionX);
            Assert.AreEqual(30, _shape.TextPositionY);
        }

        [TestMethod]
        public void MoveTextCommand_Undo_ShouldRevertTextPosition()
        {
            var command = new MoveTextCommand(_model, _shape, 10, 10, 30, 30);
            command.Execute();
            command.Undo();
            Assert.AreEqual(10, _shape.TextPositionX);
            Assert.AreEqual(10, _shape.TextPositionY);
        }

        [TestMethod]
        public void DeleteShapeCommand_Execute_ShouldRemoveShapeFromModel()
        {
            _model.Shapes.Add(_shape);
            var command = new DeleteShapeCommand(_model, _shape);
            command.Execute();
            Assert.IsFalse(_model.Shapes.Contains(_shape));
        }

        [TestMethod]
        public void DeleteShapeCommand_Undo_ShouldReaddShapeToModel()
        {
            var command = new DeleteShapeCommand(_model, _shape);
            command.Execute();
            command.Undo();
            Assert.IsTrue(_model.Shapes.Contains(_shape));
        }

        [TestMethod]
        public void MoveShapeCommand_ConnectedLines_Execute_ShouldUpdateLinePositions()
        {
            // Create a start and end shape
            var startShape = new ProcessShape { Id = 6, PositionX = 100, PositionY = 100 };
            var endShape = new ProcessShape { Id = 7, PositionX = 200, PositionY = 200 };
            _model.Shapes.Add(startShape);
            _model.Shapes.Add(endShape);

            // Create a line connecting these shapes
            var line = new LineShape
            {
                StartShape = startShape,
                EndShape = endShape,
                StartPoint = new PointF(100, 100),
                EndPoint = new PointF(200, 200),
                PositionX = 100,
                PositionY = 100,
                Width = 100,
                Height = 100
            };
            _model.Shapes.Add(line);

            // Create a move command for the start shape
            var moveCommand = new MoveShapeCommand(
                _model,
                startShape,
                100, 100, 100, 100,  // Old shape and text positions
                150, 150, 150, 150   // New shape and text positions
            );

            // Execute the move command
            moveCommand.Execute();

            // Check that the line's start point and position have been updated
            Assert.AreEqual(new PointF(150, 150), line.StartPoint);
            Assert.AreEqual(150, line.PositionX);
            Assert.AreEqual(150, line.PositionY);
            Assert.AreEqual(50, line.Width);   // Width changed due to movement
            Assert.AreEqual(50, line.Height);  // Height changed due to movement
        }

        [TestMethod]
        public void MoveShapeCommand_ConnectedLines_Undo_ShouldRestoreLinePositions()
        {
            // Create a start and end shape
            var startShape = new ProcessShape { Id = 8, PositionX = 100, PositionY = 100 };
            var endShape = new ProcessShape { Id = 9, PositionX = 200, PositionY = 200 };
            _model.Shapes.Add(startShape);
            _model.Shapes.Add(endShape);

            // Create a line connecting these shapes
            var line = new LineShape
            {
                StartShape = startShape,
                EndShape = endShape,
                StartPoint = new PointF(100, 100),
                EndPoint = new PointF(200, 200),
                PositionX = 100,
                PositionY = 100,
                Width = 100,
                Height = 100
            };
            _model.Shapes.Add(line);

            // Create a move command for the start shape
            var moveCommand = new MoveShapeCommand(
                _model,
                startShape,
                100, 100, 100, 100,  // Old shape and text positions
                150, 150, 150, 150   // New shape and text positions
            );

            // Execute the move command
            moveCommand.Execute();

            // Undo the move command
            moveCommand.Undo();

            // Check that the line's start point and position have been restored
            Assert.AreEqual(new PointF(100, 100), line.StartPoint);
            Assert.AreEqual(100, line.PositionX);
            Assert.AreEqual(100, line.PositionY);
            Assert.AreEqual(100, line.Width);
            Assert.AreEqual(100, line.Height);
        }

        [TestMethod]
        public void MoveShapeCommand_ConnectedLines_EndPoint_Execute_ShouldUpdateLinePositions()
        {
            // 创建两个形状
            var startShape = new ProcessShape { Id = 10, PositionX = 100, PositionY = 100 };
            var endShape = new ProcessShape { Id = 11, PositionX = 200, PositionY = 200 };
            _model.Shapes.Add(startShape);
            _model.Shapes.Add(endShape);

            // 创建一条连接这两个形状的线段
            var line = new LineShape
            {
                StartShape = startShape,
                EndShape = endShape,
                StartPoint = new PointF(100, 100),
                EndPoint = new PointF(200, 200),
                PositionX = 100,
                PositionY = 100,
                Width = 100,
                Height = 100
            };
            _model.Shapes.Add(line);

            // 创建一个移动终点形状的命令
            var moveCommand = new MoveShapeCommand(
                _model,
                endShape,
                200, 200, 200, 200,  // 旧的形状和文本位置
                250, 250, 250, 250   // 新的形状和文本位置
            );

            // 执行移动命令
            moveCommand.Execute();

            // 检查线段的终点是否已更新
            Assert.AreEqual(new PointF(250, 250), line.EndPoint);
            Assert.AreEqual(150, line.Width);   // 宽度应根据移动变化
            Assert.AreEqual(150, line.Height);  // 高度应根据移动变化
        }

        [TestMethod]
        public void MoveShapeCommand_ConnectedLines_EndPoint_Undo_ShouldRestoreLinePositions()
        {
            // 创建两个形状
            var startShape = new ProcessShape { Id = 12, PositionX = 100, PositionY = 100 };
            var endShape = new ProcessShape { Id = 13, PositionX = 200, PositionY = 200 };
            _model.Shapes.Add(startShape);
            _model.Shapes.Add(endShape);

            // 创建一条连接这两个形状的线段
            var line = new LineShape
            {
                StartShape = startShape,
                EndShape = endShape,
                StartPoint = new PointF(100, 100),
                EndPoint = new PointF(200, 200),
                PositionX = 100,
                PositionY = 100,
                Width = 100,
                Height = 100
            };
            _model.Shapes.Add(line);

            // 创建一个移动终点形状的命令
            var moveCommand = new MoveShapeCommand(
                _model,
                endShape,
                200, 200, 200, 200,  // 旧的形状和文本位置
                250, 250, 250, 250   // 新的形状和文本位置
            );

            // 执行移动命令
            moveCommand.Execute();

            // 撤销移动命令
            moveCommand.Undo();

            // 检查线段的终点是否已恢复到原始位置
            Assert.AreEqual(new PointF(200, 200), line.EndPoint);
            Assert.AreEqual(100, line.PositionX);
            Assert.AreEqual(100, line.PositionY);
            Assert.AreEqual(100, line.Width);
            Assert.AreEqual(100, line.Height);
        }

        [TestMethod]
        public void DeleteShapeCommand_Execute_ShouldRemoveShapeAndConnectedLines()
        {
            // Arrange
            var shape1 = new ProcessShape { Id = 14, PositionX = 100, PositionY = 100 };
            var shape2 = new ProcessShape { Id = 15, PositionX = 200, PositionY = 200 };
            var shape3 = new ProcessShape { Id = 16, PositionX = 300, PositionY = 300 };

            // Add shapes to model
            _model.Shapes.Add(shape1);
            _model.Shapes.Add(shape2);
            _model.Shapes.Add(shape3);

            // Create lines connecting shape1 to shape2 and shape3
            var line1 = new LineShape
            {
                StartShape = shape1,
                EndShape = shape2,
                StartPoint = new PointF(100, 100),
                EndPoint = new PointF(200, 200)
            };

            var line2 = new LineShape
            {
                StartShape = shape1,
                EndShape = shape3,
                StartPoint = new PointF(100, 100),
                EndPoint = new PointF(300, 300)
            };

            _model.Shapes.Add(line1);
            _model.Shapes.Add(line2);

            // Initial verification
            Assert.AreEqual(5, _model.Shapes.Count); // 3 shapes + 2 lines

            // Act
            var command = new DeleteShapeCommand(_model, shape1);
            command.Execute();

            // Assert
            Assert.IsFalse(_model.Shapes.Contains(shape1), "Shape1 should be removed");
            Assert.IsTrue(_model.Shapes.Contains(shape2), "Shape2 should remain");
            Assert.IsTrue(_model.Shapes.Contains(shape3), "Shape3 should remain");
            Assert.IsFalse(_model.Shapes.Contains(line1), "Line1 should be removed");
            Assert.IsFalse(_model.Shapes.Contains(line2), "Line2 should be removed");
            Assert.AreEqual(2, _model.Shapes.Count); // Only shape2 and shape3 should remain
        }

        [TestMethod]
        public void DeleteShapeCommand_Undo_ShouldRestoreShapeAndConnectedLines()
        {
            // Arrange
            var shape1 = new ProcessShape { Id = 17, PositionX = 100, PositionY = 100 };
            var shape2 = new ProcessShape { Id = 18, PositionX = 200, PositionY = 200 };
            var shape3 = new ProcessShape { Id = 19, PositionX = 300, PositionY = 300 };

            // Add shapes to model
            _model.Shapes.Add(shape1);
            _model.Shapes.Add(shape2);
            _model.Shapes.Add(shape3);

            // Create lines connecting shape1 to shape2 and shape3
            var line1 = new LineShape
            {
                StartShape = shape1,
                EndShape = shape2,
                StartPoint = new PointF(100, 100),
                EndPoint = new PointF(200, 200)
            };

            var line2 = new LineShape
            {
                StartShape = shape1,
                EndShape = shape3,
                StartPoint = new PointF(100, 100),
                EndPoint = new PointF(300, 300)
            };

            _model.Shapes.Add(line1);
            _model.Shapes.Add(line2);

            var command = new DeleteShapeCommand(_model, shape1);

            // Act
            command.Execute(); // First delete
            command.Undo();   // Then undo

            // Assert
            Assert.IsTrue(_model.Shapes.Contains(shape1), "Shape1 should be restored");
            Assert.IsTrue(_model.Shapes.Contains(shape2), "Shape2 should remain");
            Assert.IsTrue(_model.Shapes.Contains(shape3), "Shape3 should remain");
            Assert.IsTrue(_model.Shapes.Contains(line1), "Line1 should be restored");
            Assert.IsTrue(_model.Shapes.Contains(line2), "Line2 should be restored");
            Assert.AreEqual(5, _model.Shapes.Count); // All shapes and lines should be restored
        }
    }
}