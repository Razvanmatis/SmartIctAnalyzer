using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Interfaces.PcbInvestigator;

namespace Interfaces.PCBApiObjects
{
    public class GeometricAttributes : IGeometricAttributes
    {
        private Rectangle bounds;
        private PointF centerPoint;
        private float rotation;

        public GeometricAttributes(Rectangle bounds, PointF centerPoint, float rotation)
        {
            this.bounds = bounds;
            this.centerPoint = centerPoint;
            this.rotation = rotation;
        }

        public Rectangle Bounds
        {
            get
            {
                return bounds;
            }
        }

        public PointF CenterPoint
        {
            get
            {
                return centerPoint;
            }
        }

        public float Rotation
        {
            get
            {
                return rotation;
            }
        }
    }
}
