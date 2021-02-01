using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using Interfaces.PcbInvestigator;
using ProMik.Services.Interfaces;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class ViewModelInduction : ViewModelPCBComponentBase
    {
        private IGeneralSettingsData settingsVm;

        public ViewModelInduction(IPCBComponent component, IEventService eventService, IGeneralSettingsData settingsVm, IComponentViewModel componentVm)
            : base(component, eventService, settingsVm, componentVm)
        {
            this.settingsVm = settingsVm;
            InitSettings();
        }

        public override void InitSettings()
        {
            SolidColorBrush brush = new SolidColorBrush();
            brush.Color = settingsVm.InductionColor;
            BackgroundColor = brush;
            if (!settingsVm.InductionNamesChecked)
            {
                Name = "I";
            }
            else
            {
                Name = BaseComponent.FunctionalAttributes.Ref;
            }
        }
    }
}
