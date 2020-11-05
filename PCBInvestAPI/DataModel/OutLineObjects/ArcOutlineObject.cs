using PCBI.MathUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace PCBInvestAPI.DataModel.OutLineObjects
{
    public class ArcOutlineObject : BaseOutlineObject
    {
        public string Type { get; set; } = "Arc";



        private double _xcenter;

        public double X_Center 
        {
            get { return _xcenter; }

        }

        private double _ycenter;

        public double Y_Center 
        {
            get { return _ycenter; }

        }
        /// <summary>
        /// The Diamter of the Arc (2x distance center to start)
        /// </summary>
        private double _diamter;

        public double Diameter
        {
            get { return _diamter; }
        }

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

        private bool _clockwise;

        public bool Clockwise
        {
            get { return _clockwise; }

        }


     

        public ArcOutlineObject(PointD Center, PointD StartPoint, PointD EndPoint, bool Clockwise, Conversions einheit = Conversions.mm)
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
            _xcenter = Math.Round(Center.X * ConversionValue, 3 );
            _ycenter = _xcenter = Math.Round(Center.Y * ConversionValue, 3);

            _startXPoint = Math.Round(StartPoint.X * ConversionValue, 3);
            _startYPoint = Math.Round(StartPoint.Y * ConversionValue, 3);

            _endXPoint = Math.Round(EndPoint.X * ConversionValue, 3);
            _endYPoint = Math.Round(EndPoint.Y * ConversionValue, 3);

            _clockwise = Clockwise;

            _diamter = Math.Round(Math.Sqrt(Math.Pow(Math.Abs(_startXPoint - _endXPoint), 2) + Math.Pow(Math.Abs(_startYPoint - _endYPoint), 2)),3);


        }





    }
}
