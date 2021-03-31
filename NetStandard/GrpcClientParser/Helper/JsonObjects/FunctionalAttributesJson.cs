namespace GrpcClientParser.Helper.JsonObjects
{
    public enum ComponentTypeJson
    {
        /// <summary>
        /// UnknownComponentType
        /// </summary>
        UnknownComponentType = 0,

        /// <summary>
        /// Arc
        /// </summary>
        Arc = 1,

        /// <summary>
        /// Line
        /// </summary>
        Line = 2,

        /// <summary>
        /// Pad
        /// </summary>
        Pad = 3,

        /// <summary>
        /// Surface
        /// </summary>
        Surface = 4,

        /// <summary>
        /// Text
        /// </summary>
        Text = 5,

        /// <summary>
        /// Symbol
        /// </summary>
        Symbol = 6,

        /// <summary>
        /// Component
        /// </summary>
        Component = 7,
    }

    public class FunctionalAttributesJson
    {
        public FunctionalAttributesJson(
            ComponentTypeJson componentType,
            string refValue,
            string partName,
            string layerName,
            string packageName,
            string value,
            string normalizedName)
        {
            ComponentType = componentType;
            Ref = refValue;
            PartName = partName;
            LayerName = layerName;
            PackageName = packageName;
            Value = value;
            NormalizedName = normalizedName;
        }

        public ComponentTypeJson ComponentType { get; set; }

        public string Ref { get; set; }

        public string PartName { get; set; }

        public string LayerName { get; set; }

        public string PackageName { get; set; }

        public string Value { get; set; }

        public string NormalizedName { get; }
    }
}
