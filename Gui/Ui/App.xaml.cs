using System.Windows;
using GrpcClientParser.Implementations;
using GrpcClientParser.Interfaces;
using Interfaces.Gui;
using PinInformationExtractor.Interfaces;
using Prism.Ioc;
using Prism.Modularity;
using ProMik.BSDL;
using ProMik.BSDL.Interfaces;
using ProMik.Core.Interfaces.Events;
using ProMik.Core.Interfaces.Settings;
using ProMik.Core.Services.EventService;
using SVFHelper.Implementations;
using SVFHelper.Interfaces;
using TestCoverage.Implementations;
using TestCoverage.Interfaces;
using Ui.Modules.ModuleName;
using Ui.Modules.ModuleName.Implementations;
using Ui.Modules.ModuleName.Interfaces;
using Ui.Modules.ModuleName.Model;
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
            containerRegistry.RegisterSingleton<IManifestHandler, ManifestHandler>();
            containerRegistry.RegisterSingleton<IDialogSelector, DialogSelector>();
            containerRegistry.RegisterSingleton<IProjectHandler, GeneralProjectHandler>();
            containerRegistry.RegisterSingleton<IEventService, PrismEventService>();
            containerRegistry.RegisterSingleton<ISettingsData, ProMikSettingsData>();
            containerRegistry.RegisterSingleton<ISettingsStorageManager, ProMikSettingsStorageManager>();
            containerRegistry.RegisterSingleton<IBomDataModel, BomDataModel>();
            containerRegistry.RegisterSingleton<ISettingsService, ProMik.Core.Services.Settings.LiteDBSettingsService>();
            containerRegistry.RegisterSingleton<IGrpcClientParserHandler, GrpcClientParserHandler>();
            containerRegistry.RegisterSingleton<ITestCoverageDeterminer, TestCoverageBoundaryScanDeterminer>();
            containerRegistry.RegisterSingleton<IPinInformationExtractor, PinInformationExtractor.Implementations.PinInformationExtractor>();
            containerRegistry.RegisterSingleton<ILogger, Logger>();
            containerRegistry.RegisterSingleton<ISVFHelper, SVFHelper.Implementations.SVFHelper>();
            containerRegistry.RegisterSingleton<ISvfPlayer, SvfPlayer>();
            containerRegistry.RegisterSingleton<IPackageService, PackageService>();
            containerRegistry.RegisterSingleton<IBSDLProcessor, BSDLProcessor>();
            containerRegistry.RegisterSingleton<ISettingsHandler, SettingsHandler>();
            containerRegistry.RegisterSingleton<IResultModel, ResultModel>();
            containerRegistry.RegisterSingleton<IProjectLoadHandler, ProjectLoadHandler>();
            containerRegistry.RegisterSingleton<ISvfHandler, SvfHandler>();
            containerRegistry.RegisterSingleton<ITestCoverageDataModel, TestCoverageDataModel>();
            containerRegistry.RegisterSingleton<IBomHandler, BomHandler>();
            containerRegistry.RegisterSingleton<IProjectFileHandler, ProjectFileHandler>();
            containerRegistry.RegisterSingleton<IDropHandler, DropHandler>();
            containerRegistry.RegisterSingleton<IPinInformationExtractorHandler, PinInformationExtractorHandler>();
            containerRegistry.RegisterSingleton<ITestCoverageHandler, TestCoverageHandler>();
        }
    }
}
