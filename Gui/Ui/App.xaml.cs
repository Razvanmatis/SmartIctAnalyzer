using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using PinInformationExtractor.Interfaces;
using Prism.Ioc;
using Prism.Modularity;
using ProMik.BSDL;
using ProMik.BSDL.Interfaces;
using ProMik.BSDLPackageAnalyzer;
using ProMik.Core.Interfaces.Events;
using ProMik.Core.Interfaces.Settings;
using ProMik.Core.Services.EventService;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.TestCoverage;
using ProMik.SmartIct.JtagPinInformationExtractor.Interfaces;
using ProMik.SmartIct.PCBComponentParser.Implementations;
using ProMik.SmartIct.PCBComponentParser.Interfaces;
using ProMik.SmartIct.Services.ReportCreator.Implementations;
using ProMik.SmartIct.Services.ReportCreator.Interfaces;
using ProMik.SmartIct.Svf.SvfFileCreation.Implementations;
using ProMik.SmartIct.Svf.SvfFileCreation.Interfaces;
using ProMik.SmartIct.TestCoverageDeterminer.Implementations;
using ProMik.SmartIct.TestCoverageDeterminer.Interfaces;
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
            SetupExceptionHandling();
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IManifestHandler, ManifestHandler>();
            containerRegistry.RegisterSingleton<IDialogSelector, DialogSelector>();
            containerRegistry.RegisterSingleton<IProjectHandler, GeneralProjectHandler>();
            containerRegistry.RegisterSingleton<IEventService, PrismEventService>();
            containerRegistry.RegisterSingleton<ISettingsData, ProMikSettingsData>();
            containerRegistry.RegisterSingleton<IBomUseValues>(() => Container.Resolve<ISettingsData>());
            containerRegistry.RegisterSingleton<ISettingsStorageManager, ProMikSettingsStorageManager>();
            containerRegistry.RegisterSingleton<IBomDataModel, BomDataModel>();
            containerRegistry.RegisterSingleton<ISettingsService, ProMik.Core.Services.Settings.LiteDBSettingsService>();
            containerRegistry.RegisterSingleton<IPCBComponentParser, PCBComponentParserHandler>();
            containerRegistry.RegisterSingleton<ITestCoverageDeterminer, TestCoverageBoundaryScanDeterminer>();
            containerRegistry.RegisterSingleton<IJtagPinInformationCreator, ProMik.SmartIct.JtagPinInformationExtractor.Implementations.JJtagPinInformationCreator>();
            containerRegistry.RegisterSingleton<ILogger, Logger>();
            containerRegistry.RegisterSingleton<ISvfPlayerHandler, SvfPlayerHandler>();
            containerRegistry.RegisterSingleton<IBSDLProcessor, BSDLProcessor>();
            containerRegistry.RegisterSingleton<ISettingsHandler, SettingsHandler>();
            containerRegistry.RegisterSingleton<IResultModel, ResultModel>();
            containerRegistry.RegisterSingleton<IProjectLoadHandler, ProjectLoadHandler>();
            containerRegistry.RegisterSingleton<ISvfDataCreator, SvfDataCreator>();
            containerRegistry.RegisterSingleton<ISvfHandler, SvfHandler>();
            containerRegistry.RegisterSingleton<ITestCoverageDataModel, TestCoverageDataModel>();
            containerRegistry.RegisterSingleton<IBomHandler, BomHandler>();
            containerRegistry.RegisterSingleton<IProjectFileHandler, ProjectFileHandler>();
            containerRegistry.RegisterSingleton<IDropHandler, DropHandler>();
            containerRegistry.RegisterSingleton<IPinInformationExtractorHandler, PinInformationExtractorHandler>();
            containerRegistry.RegisterSingleton<ICsvReportCreator, CsvReportCreator>();
            containerRegistry.RegisterSingleton<IPdfReportHandler, PdfReportHandler>();
            containerRegistry.RegisterSingleton<ITestCoverageHandler, TestCoverageHandler>();
            containerRegistry.RegisterSingleton<IComponentHandler, ComponentHandler>();
            containerRegistry.RegisterSingleton<IPackageAnalyzer, PackageAnalyzer>();
            containerRegistry.RegisterSingleton<ITestEventListener, TestEventListener>();
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
           LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
        }

        private void LogUnhandledException(Exception exception, string source)
        {
            string text = "Critical error. Unhandled exception catched in " + source + " " + exception.Message;
            File.WriteAllText("critical_error.log", "\n" + text + "\n" + exception.StackTrace);
            IEventService eventAggregator = Container.Resolve<IEventService>();
            eventAggregator.Publish(new ProMik.Core.Interfaces.Events.UIEvents.ProMikLogEvent(text) { Level = ProMik.Core.Interfaces.Events.UIEvents.LogLevelEnum.FATAL });
        }
    }
}
