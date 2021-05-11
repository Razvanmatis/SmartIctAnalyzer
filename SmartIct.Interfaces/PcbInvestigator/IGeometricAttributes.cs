using System.Drawing;

namespace Interfaces.PcbInvestigator
{
    public interface IGeometricAttributes
    {
        Rectangle Bounds { get; }

        PointF CenterPoint { get; }

        float Rotation { get; }

        double CompHeight { get; }
    }
}
