using System;
using System.Collections.Generic;
using System.Text;
using GrpcClientParser.Interfaces;

namespace GrpcClientParser.Helper.JsonObjects
{
    public class PointJson
    {
        public PointJson(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; set; }

        public float Y { get; set; }
    }
}
