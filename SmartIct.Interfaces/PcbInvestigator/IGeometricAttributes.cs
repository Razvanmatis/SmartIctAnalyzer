using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Interfaces.PcbInvestigator
{
    public interface IGeometricAttributes
    {
        Rectangle Bounds { get; }

        PointF CenterPoint { get; }

        float Rotation { get; }
    }
}
