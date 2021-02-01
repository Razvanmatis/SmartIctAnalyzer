using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Ui.Core;
using Ui.Modules.ModuleName.Interfaces;
using Ui.Modules.ModuleName.ViewModels;
using Ui.Modules.ModuleName.Views;

namespace Ui.Modules.ModuleName
{
    public class ModuleNameModule : IModule
    {
        private readonly IRegionManager regionManager;

        public ModuleNameModule(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            regionManager.RequestNavigate(RegionNames.ContentRegion, "ComponentView");
            regionManager.RequestNavigate(RegionNames.LayerRegion, "LayerView");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ComponentView>();
            containerRegistry.RegisterForNavigation<LayerView>();
            containerRegistry.RegisterSingleton<IComponentViewModel, ComponentViewModel>();
        }
    }
}