using System;
using System.Collections.Generic;
using System.Text;
using GrpcClientParser.Interfaces;

namespace GrpcClientParser.Helper.JsonObjects
{
    public class RectangleJson
    {
        public RectangleJson(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}
