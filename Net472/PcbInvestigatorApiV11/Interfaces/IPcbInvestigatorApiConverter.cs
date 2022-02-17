using System.Collections.Generic;
using GrpcApi.Helper;
using GrpcApi.Interfaces;
using PCBI.Automation.Interfaces;

namespace PcbInvestigatorApiV11.Interfaces
{
    public interface IPcbInvestigatorApiConverter : IPcbInvestigatorApiBaseConverter
    {
        IGrpcResult GetConvertedObjects(
            IList<StepComponentContainer> pcbObjects,
            IList<InterfaceNet> allNets,
            AbstractDataProvider dataProvider,
            int stepAmount);
    }
}
