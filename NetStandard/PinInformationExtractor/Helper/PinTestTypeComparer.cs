using ProMik.SmartIct.Interfaces.Helper;
using System.Collections.Generic;

namespace ProMik.SmartIct.JtagPinInformationExtractor.Helper
{
    public class PinTestTypeComparer : Comparer<PinTestTypeContainer>
    {
        public override int Compare(PinTestTypeContainer x, PinTestTypeContainer y)
        {
            return string.Compare(x.PinTestObject.PinNumber, y.PinTestObject.PinNumber);
        }
    }
}
