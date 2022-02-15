using PCBI.Automation.Interfaces;

namespace GrpcApi.Helper
{
    public class StepComponentContainer
    {
        public StepComponentContainer(InterfaceCMPObject component, int stepNo)
        {
            Component = component;
            StepNo = stepNo;
        }

        public int StepNo { get; }

        public InterfaceCMPObject Component { get; }
    }
}
