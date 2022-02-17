namespace ProMik.SmartIct.PCBComponentParser.Helper.JsonObjects
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
