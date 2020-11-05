using System;
using System.Collections.Generic;
using System.Text;

namespace Uniconverter.Core.DataModel.OutLineObjects
{
    public class BaseOutlineObject
    {

        public string Type { get; set; }

        public double X_Center { get; set; }

        public double Y_Center { get; set; }

        /// <summary>
        /// The Diamter of the Arc (2x distance center to start)
        /// </summary>

        public double Diameter { get; set; }

        public double X_StartPoint { get; set; }

        public double Y_StartPoint { get; set; }

        public double X_EndPoint { get; set; }

        public double Y_EndPoint { get; set; }

        public bool Clockwise { get; set; }


    }
}
