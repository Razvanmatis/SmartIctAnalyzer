namespace ProMik.SmartIct.Interfaces.Helper
{
    public class PinTestTypeContainer
    {
        public PinConnectionTypeContainer PinConnectionType { get; set; }

        public PinTestObject PinTestObject { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is PinTestTypeContainer containter))
            {
                return false;
            }

            return PinTestObject.Equals(containter.PinTestObject) && PinConnectionType.Equals(containter.PinConnectionType);
        }

        public override int GetHashCode()
        {
            return PinConnectionType.GetHashCode() + PinTestObject.GetHashCode();
        }
    }
}
