﻿using MyDrawing.Shapes;
using System;
using static DecisionShape;
using static MyDrawing.MyDrawingModel;

namespace MyDrawing.Factories
{
    public class ShapeFactory
    {
        public static IShape CreateShape(string shapeType)
        {
            switch (shapeType)
            {
                case "Start":
                    return new StartShape();
                case "Terminator":
                    return new TerminatorShape();
                case "Process":
                    return new ProcessShape();
                case "Decision":
                    return new DecisionShape();
                case "Line":
                    return new LineShape();
                default:
                    throw new ArgumentException("Invalid shape type");
            }
        }
    }
}