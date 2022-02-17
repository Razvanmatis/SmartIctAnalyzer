using System.Drawing;

namespace ProMik.SmartIct.Interfaces.PcbInvestigator.Implementations
{
    public class GeometricAttributes : IGeometricAttributes
    {
        private readonly Rectangle bounds;
        private readonly PointF centerPoint;
        private readonly float rotation;
        private readonly double compHeight;

        public GeometricAttributes(Rectangle bounds, PointF centerPoint, float rotation, double compHeight)
        {
            this.bounds = bounds;
            this.centerPoint = centerPoint;
            this.rotation = rotation;
            this.compHeight = compHeight;
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

        public double CompHeight
        {
            get
            {
                return compHeight;
            }
        }
    }
}
