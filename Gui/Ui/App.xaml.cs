using System.Windows;
using GrpcClientParser;
using GrpcClientParser.Interfaces;
using Interfaces;
using Interfaces.Gui;
using Interfaces.TestCoverage;
using Prism.Ioc;
using Prism.Modularity;
using ProMik.Services;
using ProMik.Services.Interfaces;
using TestCoverage;
using Ui.Modules.ModuleName;
using Ui.Modules.ModuleName.Helper;
using Ui.Modules.ModuleName.Interfaces;
using Ui.Modules.ModuleName.Model;
using Ui.Modules.ModuleName.ViewModels;
using Ui.Services;
using Ui.Views;

namespace Ui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<ModuleNameModule>();
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IMessageService, MessageService>();
            containerRegistry.RegisterSingleton<IEventService, EventService>();
            containerRegistry.RegisterSingleton<IGeneralSettingsData, GeneralSettingsData>();
            containerRegistry.RegisterSingleton<ISettingsStorageManager, SettingsStorageManager>();
            containerRegistry.RegisterSingleton<IBomDataModel, BomDataModel>();
            containerRegistry.RegisterSingleton<IGrpcClientParserHandler, GrpcClientParserHandler>();
            containerRegistry.RegisterSingleton<ITestCoverageDeterminer, TestCoverageDeterminer>();
        }
    }
}
