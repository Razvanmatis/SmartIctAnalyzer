namespace GrpcClientParser.Helper.JsonObjects
{
    public class GeometricJson
    {
        public GeometricJson(RectangleJson bounds, PointJson point, float rotation)
        {
            Bounds = bounds;
            CenterPoint = point;
            Rotation = rotation;
        }

        public RectangleJson Bounds { get; set; }

        public PointJson CenterPoint { get; set; }

        public float Rotation { get; set; }
    }
}
