namespace ProMik.SmartIct.PCBComponentParser.Helper.JsonObjects
{
    public class GeometricJson
    {
        public GeometricJson(RectangleJson bounds, PointJson point, float rotation, double compHeight)
        {
            Bounds = bounds;
            CenterPoint = point;
            Rotation = rotation;
            CompHeight = compHeight;
        }

        public RectangleJson Bounds { get; set; }

        public PointJson CenterPoint { get; set; }

        public float Rotation { get; set; }

        public double CompHeight { get; set; }
    }
}
