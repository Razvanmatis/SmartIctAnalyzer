using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.PCBComponentParser.Implementations;
using Ui.Modules.ModuleName.Interfaces;
using Ui.Modules.ModuleName.ViewModels;

namespace Ui.Modules.ModuleName.Helper
{
    public static class ViewModelFactory
    {
        public static ViewModelPCBComponentBase GetViewModelObject(
            IPCBComponent component,
            ISettingsData settingsVm,
            IComponentHandler componentVm)
        {
            if (component is PCBResistor)
            {
                return new ViewModelResistor(component, settingsVm, componentVm);
            }
            else if (component is PCBCapacitor)
            {
                return new ViewModelCapacitor(component, settingsVm, componentVm);
            }
            else if (component is PCBInduction)
            {
                return new ViewModelInduction(component, settingsVm, componentVm);
            }
            else if (component is PCBTestpoint)
            {
                return new ViewModelTestpoint(component, settingsVm, componentVm);
            }
            else if (component is PCBIc)
            {
                return new ViewModelIc(component, settingsVm, componentVm);
            }
            else if (component is PCBConnector)
            {
                return new ViewModelConnector(component, settingsVm, componentVm);
            }
            else
            {
                return new ViewModelComponent(component, settingsVm, componentVm);
            }
        }
    }
}
