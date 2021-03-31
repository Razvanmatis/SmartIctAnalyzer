using System.Drawing;

namespace Interfaces.PcbInvestigator.Implementations
{
    public class GeometricAttributes : IGeometricAttributes
    {
        private readonly Rectangle bounds;
        private readonly PointF centerPoint;
        private readonly float rotation;

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
