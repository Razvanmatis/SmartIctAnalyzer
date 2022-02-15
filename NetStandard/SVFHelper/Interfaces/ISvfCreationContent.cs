using System.Collections.Generic;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Interfaces
{
    public interface ISvfCreationContent
    {
        List<(bool isInput, bool expectedValue)> SvfCreationContent { get; }
    }
}
