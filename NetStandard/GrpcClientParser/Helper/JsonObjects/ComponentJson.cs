using System.Collections.Generic;

namespace ProMik.SmartIct.PCBComponentParser.Helper.JsonObjects
{
    public class ComponentJson
    {
        public ComponentJson(GeometricJson geometrics, FunctionalAttributesJson functional, IList<int> pins)
        {
            Geometrics = geometrics;
            Functionals = functional;
            Pins = pins;
        }

        public GeometricJson Geometrics { get; set; }

        public FunctionalAttributesJson Functionals { get; set; }

        public IList<int> Pins { get; set; }
    }
}
