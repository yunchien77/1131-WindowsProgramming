using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MyDrawing.Factories;
using MyDrawing.Shapes;
using static DecisionShape;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Threading;

namespace MyDrawing
{
    public class MyDrawingModel
    {
        private readonly List<IShape> _shapes;
        private readonly Random _random;
        private bool _hasUnsavedChanges;
        private int _lastUsedId = 0;

        public List<IShape> Shapes => _shapes;

        public event EventHandler ShapesChanged;

        private readonly SynchronizationContext _synchronizationContext;

        public MyDrawingModel()
        {
            _shapes = new List<IShape>();
            _random = new Random();
            _synchronizationContext = SynchronizationContext.Current;
        }

        public int GenerateNewId()
        {
            // 找出目前所有圖形中最大的 ID
            int maxCurrentId = _shapes.Any() ? _shapes.Max(s => s.Id) : 0;
            // 比較最後使用的 ID 和目前最大 ID，取較大值
            _lastUsedId = Math.Max(_lastUsedId, maxCurrentId);
            // 回傳新的 ID
            return ++_lastUsedId;
        }

        public IShape FindShapeAtPosition(Point location)
        {
            // 從列表開始處搜尋，這樣最上層的圖形會先被檢查到
            return _shapes
                .OrderByDescending(s => s.Id)
                .FirstOrDefault(shape =>
                    !(shape is LineShape) && // 過濾掉 LineShape
                    location.X >= shape.PositionX &&
                    location.X <= shape.PositionX + shape.Width &&
                    location.Y >= shape.PositionY &&
                    location.Y <= shape.PositionY + shape.Height
                );

        }

        public void DeleteShape(int id)
        {
            _shapes.RemoveAll(shape => shape.Id == id);
            //HasUnsavedChanges = true;
            OnShapesChanged();
        }

        public List<IShape> GetShapes()
        {
            return _shapes.OrderBy(s => s.Id).ToList();
        }

        public virtual void OnShapesChanged()
        {
            if (_synchronizationContext != null)
            {
                _synchronizationContext.Post(_ => ShapesChanged?.Invoke(this, EventArgs.Empty), null);
            }
            //else
            //{
            Console.WriteLine($"OnShapesChanged called, Shapes count: {Shapes.Count}");
            ShapesChanged?.Invoke(this, EventArgs.Empty);
            //}
        }

        public string GenerateRandomText()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            int length = _random.Next(3, 11);
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public async Task SaveShapesAsync(string filePath)
        {
            await Task.Run(() =>
            {
                try
                {
                    var shapeDataList = _shapes.Select(shape => new ShapeData
                    {
                        Id = shape.Id,
                        Type = shape.GetShapeType(),
                        Text = shape.Text,
                        PositionX = shape.PositionX,
                        PositionY = shape.PositionY,
                        Height = shape.Height,
                        Width = shape.Width,
                        TextPositionX = shape.TextPositionX,
                        TextPositionY = shape.TextPositionY,
                        StartShapeId = (shape as LineShape)?.StartShape?.Id,
                        EndShapeId = (shape as LineShape)?.EndShape?.Id,
                        StartPoint = (shape as LineShape)?.StartPoint ?? new PointF(),
                        EndPoint = (shape as LineShape)?.EndPoint ?? new PointF()
                    }).ToList();

                    // Add artificial delay
                    Thread.Sleep(3000);

                    using (var stream = File.Create(filePath))
                    {
                        var formatter = new BinaryFormatter();
                        formatter.Serialize(stream, shapeDataList);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to save shapes: " + ex.Message);
                }
            });
        }

        public void LoadShapes(string filePath)
        {
            try
            {
                List<ShapeData> shapeDataList;
                using (var stream = File.OpenRead(filePath))
                {
                    var formatter = new BinaryFormatter();
                    shapeDataList = (List<ShapeData>)formatter.Deserialize(stream);
                }

                _shapes.Clear();

                // First pass: Create all shapes except lines
                var shapeDict = new Dictionary<int, IShape>();
                foreach (var data in shapeDataList.Where(d => d.Type != "Line"))
                {
                    var shape = ShapeFactory.CreateShape(data.Type);
                    shape.Id = data.Id;
                    shape.Text = data.Text;
                    shape.PositionX = data.PositionX;
                    shape.PositionY = data.PositionY;
                    shape.Height = data.Height;
                    shape.Width = data.Width;
                    shape.TextPositionX = data.TextPositionX;
                    shape.TextPositionY = data.TextPositionY;

                    _shapes.Add(shape);
                    shapeDict[shape.Id] = shape;
                }

                // Second pass: Create lines with proper connections
                foreach (var data in shapeDataList.Where(d => d.Type == "Line"))
                {
                    var line = ShapeFactory.CreateShape(data.Type) as LineShape;
                    if (line != null)
                    {
                        line.Id = data.Id;
                        line.Text = data.Text;
                        line.PositionX = data.PositionX;
                        line.PositionY = data.PositionY;
                        line.Height = data.Height;
                        line.Width = data.Width;
                        line.StartPoint = data.StartPoint;
                        line.EndPoint = data.EndPoint;

                        if (data.StartShapeId.HasValue && shapeDict.ContainsKey(data.StartShapeId.Value))
                            line.StartShape = shapeDict[data.StartShapeId.Value];
                        if (data.EndShapeId.HasValue && shapeDict.ContainsKey(data.EndShapeId.Value))
                            line.EndShape = shapeDict[data.EndShapeId.Value];

                        _shapes.Add(line);
                    }
                }
                foreach (var shape in _shapes)
                {
                    if (shape.Presenter == null)  // Use the property instead
                    {
                        shape.SetPresenter(null);
                    }
                }

                OnShapesChanged();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load shapes: " + ex.Message);
            }
        }

        public void ReplaceShapes(List<IShape> shapes)
        {
            _shapes.Clear();
            foreach (var shape in shapes)
            {
                if (shape.Presenter == null)  // Use the property instead
                {
                    shape.SetPresenter(null);
                }
                _shapes.Add(shape);
            }
            OnShapesChanged();
        }

        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set
            {
                _hasUnsavedChanges = value;
                OnShapesChanged();
            }
        }
    }
}