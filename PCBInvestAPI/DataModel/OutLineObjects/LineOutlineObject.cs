using PCBI.MathUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace PCBInvestAPI.DataModel.OutLineObjects
{
    public class LineOutlineObject : BaseOutlineObject
    {   

        private double _startXPoint;

        public double X_StartPoint
        {
            get { return _startXPoint; }

        }

        private double _startYPoint;

        public double Y_StartPoint
        {
            get { return _startYPoint; }

        }

        private double _endXPoint;

        public double X_EndPoint
        {
            get { return _endXPoint; }

        }

        private double _endYPoint;

        public double Y_EndPoint
        {
            get { return _endYPoint; }

        }

        

        public LineOutlineObject(PointD StartPoint , PointD EndPoint, Conversions einheit = Conversions.mm)
        {
            double ConversionValue = 1.00000;
            switch (einheit)
            {
                case Conversions.mil:
                    ConversionValue = 1.00000;
                    break;
                case Conversions.mm:
                    ConversionValue = 0.00254;
                    break;
                default:
                    ConversionValue = 1.00000;
                    break;
            }

            _startXPoint = Math.Round(StartPoint.X * ConversionValue, 3);
            _startYPoint = Math.Round(StartPoint.Y * ConversionValue, 3);

            _endXPoint = Math.Round(EndPoint.X * ConversionValue, 3);
            _endYPoint = Math.Round(EndPoint.Y * ConversionValue, 3);
        }

    }
}
