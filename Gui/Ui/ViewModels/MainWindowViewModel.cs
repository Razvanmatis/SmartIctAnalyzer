using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Interfaces.Gui;
using Prism.Commands;
using Prism.Mvvm;
using ProMik.Core.Interfaces.Events;
using ProMik.Core.Interfaces.Events.Enums;
using ProMik.UI.WPF.Panels.LogPanel.Wrappers;
using TestCoverage.Implementations;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private const string Name = "Smart ICT Tool";
        private static readonly string Path = Directory.GetCurrentDirectory() + "\\logs" + "\\" + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "\\SmartIctTool.log";
        private readonly ILogger logger;
        private readonly IManifestHandler manifestHandler;
        private bool isBusy;
        private bool menuEnabled;
        private Visibility testCoverageVisibility = Visibility.Hidden;
        private float testCoverage;
        private IEventService eventService;
        private ProMik.Core.Interfaces.Logging.ILogger logService;

        public MainWindowViewModel(
            IEventService eventService, ILogger logger, IManifestHandler manifestHandler)
        {
            this.manifestHandler = manifestHandler;
            EventService = eventService;
            this.logger = logger;
            eventService.Subscribe<SetBusyEvent>(HandleIsBusyEvent, ThreadOption.UIThread);
            eventService.Subscribe<SetCoverageValuesEvent>(HandleSetCoverageValuesEvent, ThreadOption.UIThread);
            MenuEnabled = true;
            WindowClosingCommand = new DelegateCommand<CancelEventArgs>(HandleExit);
            LogService = new ProMik.Core.Services.LogService.Log4Net.Log4NetLogService(Path).GetLogger(GetType());
            LogPanelLoaded = new DelegateCommand(LogPanelLoadedHandler);
            LogContext = new List<LogContextAction>
            {
                new LogContextAction("Open settings page", HandleLogContext, HandleEnabledStateOfContextMenu),
            };
        }

        public static string Title
        {
            get { return Name; }
        }

        public ICommand LogPanelLoaded { get; private set; }

        public List<LogContextAction> LogContext { get; }

        public ICommand WindowClosingCommand
        {
            get;
            private set;
        }

        public ProMik.Core.Interfaces.Logging.ILogger LogService
        {
            get
            {
                return logService;
            }

            set
            {
                SetProperty(ref logService, value);
            }
        }

        public IEventService EventService
        {
            get
            {
                return eventService;
            }

            set
            {
                SetProperty(ref eventService, value);
            }
        }

        public bool MenuEnabled
        {
            get
            {
                return menuEnabled;
            }

            set
            {
                SetProperty(ref menuEnabled, value);
            }
        }

        public bool IsBusy
        {
            get
            {
                return isBusy;
            }

            set
            {
                SetProperty(ref isBusy, value);
                MenuEnabled = !value;
            }
        }

        public float TestCoverage
        {
            get
            {
                return testCoverage;
            }

            set
            {
                SetProperty(ref testCoverage, value);
            }
        }

        public Visibility TestCoverageVisibilty
        {
            get
            {
                return testCoverageVisibility;
            }

            set
            {
                SetProperty(ref testCoverageVisibility, value);
            }
        }

        private void HandleIsBusyEvent(SetBusyEvent obj)
        {
            IsBusy = obj.IsBusy;
        }

        private bool HandleEnabledStateOfContextMenu(LogWrapper arg)
        {
            return string.Compare(arg.Message, TestCoverageBoundaryScanDeterminer.ErrorMessageDoublesFound, StringComparison.InvariantCulture) == 0;
        }

        private void HandleLogContext(LogWrapper obj)
        {
        }

        private void HandleSetCoverageValuesEvent(SetCoverageValuesEvent obj)
        {
            TestCoverage = obj.Value;
            TestCoverageVisibilty = obj.IsVisible ? Visibility.Visible : Visibility.Hidden;
        }

        private void HandleExit(CancelEventArgs obj)
        {
            LogService.Info("Exit application");
            manifestHandler.ResetValues();
            Environment.Exit(0);
        }

        private void LogPanelLoadedHandler()
        {
            logger.LogMessage("Logfile created at: " + Path, LogCategory.INFO);
        }
    }
}
