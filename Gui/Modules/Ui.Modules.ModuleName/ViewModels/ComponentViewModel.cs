using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Prism.Commands;
using Prism.Regions;
using ProMik.Core.Interfaces.Events;
using ProMik.Core.Interfaces.Events.Enums;
using ProMik.SmartIct.TestCoverageDeterminer.Events;
using Ui.Core.Mvvm;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.Helper;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class ComponentViewModel : RegionViewModelBase, IFilesDropped
    {
        private readonly ISettingsData settingsVm;
        private string message;
        private ObservableCollection<ViewModelPCBBase> componentViews = new ObservableCollection<ViewModelPCBBase>();
        private double overallWidth;
        private double overallHeight;
        private double zoomFactor = 1;
        private double originalWidth;
        private double originalHeight;
        private double actualXPos;
        private double actualYPos;
        private double realXPos;
        private double realYPos;
        private Visibility buttonVisibility;
        private IComponentHandler componentHandler;

        public ComponentViewModel(
            IRegionManager regionManager,
            IEventService eventService,
            ISettingsData settingsVm,
            IComponentHandler componentHandler)
            : base(regionManager)
        {
            this.componentHandler = componentHandler;
            this.settingsVm = settingsVm;
            eventService.Subscribe<AddPcbObjectsEvent>(
                async (x) => await AddPCBComponent(x).ConfigureAwait(true), ThreadOption.UIThread);
            eventService.Subscribe<ResetViewEvent>(ResetView, ThreadOption.UIThread);
            eventService.Subscribe<AddTestCoverageObjectsEvent>(
                (objects) => componentHandler.AddTestCoverageObjectToView(objects), ThreadOption.UIThread);
            eventService.Subscribe<ShowObjectsEvent>(
                (obj) => componentHandler.ShowTestCoverageObjects(obj), ThreadOption.UIThread);
            eventService.Subscribe<RefreshTestcoverageResultObjectsEvent>(
                (obj) => componentHandler.RefreshTestCoverageResults(obj));
            eventService.Subscribe<UpdateBomDataEvent>((obj) => componentHandler.HandleUpdateBomEvent(obj));
            eventService.Subscribe<CloseAllSettingsEvent>(
                (x) => componentHandler.CloseAllSettingsEventHandling(x, SetShowButtonsVisibility));
            eventService.Subscribe<UpdateComponentsViewEvent>((x) => ComponentViews = componentHandler.GetComponentsView());
            componentViews = new ObservableCollection<ViewModelPCBBase>();
            MouseWheelCommand = new DelegateCommand<MouseWheelEventArgs>(MouseWheelHandler);
            MouseMove = new DelegateCommand<MouseEventArgs>(MoveMouseHandler);
            MouseMoveStart = new DelegateCommand<MouseButtonEventArgs>((x) => MouseMoveStartEndHandler(true));
            MouseMoveEnd = new DelegateCommand<MouseButtonEventArgs>((x) => MouseMoveStartEndHandler(false));
            MouseLeave = new DelegateCommand<MouseEventArgs>((x) => MouseMoveStartEndHandler(false));
            ButtonVisibility = Visibility.Hidden;
            ButtonScrollInCommand = new DelegateCommand(() => HandleButtonScroll(true));
            ButtonScrollOutCommand = new DelegateCommand(() => HandleButtonScroll(false));
            MirrorCommand = new DelegateCommand<string>(MirrorAxis);
            SetScrollViewerCommand = new DelegateCommand<ScrollViewer>(SetScrollViewer);
        }

        public ICommand SetScrollViewerCommand { get; }

        public double ZoomFactor
        {
            get
            {
                return zoomFactor;
            }

            set
            {
                SetProperty(ref zoomFactor, value);
            }
        }

        public DelegateCommand<MouseWheelEventArgs> MouseWheelCommand { get; set; }

        public string Message
        {
            get { return message; }
            set { SetProperty(ref message, value); }
        }

        public double OverallWidth
        {
            get
            {
                return overallWidth;
            }

            set
            {
                SetProperty(ref overallWidth, value);
            }
        }

        public DelegateCommand<MouseEventArgs> MouseMove { get; set; }

        public DelegateCommand<MouseEventArgs> MouseLeave { get; set; }

        public DelegateCommand<MouseButtonEventArgs> MouseMoveStart { get; set; }

        public DelegateCommand<MouseButtonEventArgs> MouseMoveEnd { get; set; }

        public ICommand ButtonScrollInCommand { get; private set; }

        public ICommand ButtonScrollOutCommand { get; private set; }

        public ICommand MirrorCommand { get; private set; }

        public Visibility ButtonVisibility
        {
            get
            {
                return buttonVisibility;
            }

            set
            {
                SetProperty(ref buttonVisibility, value);
            }
        }

        public double OverallHeight
        {
            get
            {
                return overallHeight;
            }

            set
            {
                SetProperty(ref overallHeight, value);
            }
        }

        public double XPosition
        {
            get
            {
                return realXPos;
            }

            set
            {
                SetProperty(ref realXPos, value);
            }
        }

        public double YPosition
        {
            get
            {
                return realYPos;
            }

            set
            {
                SetProperty(ref realYPos, value);
            }
        }

        public ObservableCollection<ViewModelPCBBase> ComponentViews
        {
            get
            {
                return componentViews;
            }

            set
            {
                SetProperty(ref componentViews, value);
            }
        }

        private ScrollViewer Scrollviewer
        {
            get;
            set;
        }

        public void OnFilesDropped(string[] files)
        {
            componentHandler.OnFilesDropped(files);
        }

        public async Task AddPCBComponent(AddPcbObjectsEvent comp)
        {
            await componentHandler.AddPCBComponent(
                comp, DefineRanges, SetShowButtonsVisibility, ResetValuesForComponentsInsertion).ConfigureAwait(false);
            await componentHandler.ShowLayerObjects(null, true).ConfigureAwait(false);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            // do something
        }

        private void SetScrollViewer(ScrollViewer obj)
        {
            Scrollviewer = obj;
        }

        private void ResetValuesForComponentsInsertion()
        {
            ZoomFactor = 1.0;
            OverallWidth = 0;
            OverallHeight = 0;
            Application.Current.Dispatcher.Invoke(() => Scrollviewer.UpdateLayout());
        }

        private void ResetView(ResetViewEvent obj)
        {
            SetComponentAttributes(componentHandler.ResetView(obj, GetComponentAttributes(), SetShowButtonsVisibility));
            ComponentViews = componentHandler.GetComponentsView();
        }

        private void MoveMouseHandler(MouseEventArgs obj)
        {
            SetComponentAttributes(componentHandler.MoveMouseHandler(obj, GetComponentAttributes(), Scrollviewer));
        }

        private void MouseMoveStartEndHandler(bool isBegin)
        {
            componentHandler.MouseMoveStartEndHandler(isBegin);
        }

        private void MouseWheelHandler(MouseWheelEventArgs obj)
        {
            SetComponentAttributes(componentHandler.MouseWheelHandler(obj, GetComponentAttributes()));
        }

        private void HandleButtonScroll(bool scrollIn)
        {
            SetComponentAttributes(componentHandler.HandleButtonScroll(scrollIn, GetComponentAttributes()));
        }

        private void MirrorAxis(string xAxis)
        {
            componentHandler.MirrorAxis(xAxis, DefineRanges);
        }

        private ComponentAttributes GetComponentAttributes()
        {
            return new ComponentAttributes(
                actualXPos,
                actualYPos,
                XPosition,
                YPosition,
                ZoomFactor,
                OverallWidth,
                OverallHeight,
                originalWidth,
                originalHeight);
        }

        private void SetComponentAttributes(ComponentAttributes attributes)
        {
            actualXPos = attributes.ActualXPos;
            actualYPos = attributes.ActualYPos;
            XPosition = attributes.XPosition;
            YPosition = attributes.YPosition;
            OverallWidth = attributes.OverallWidth;
            OverallHeight = attributes.OverallHeight;
            originalWidth = attributes.OriginalWidth;
            originalHeight = attributes.OriginalHeight;
            ZoomFactor = attributes.ZoomFactor;
        }

        private void DefineRanges()
        {
            SetComponentAttributes(componentHandler.DefineRanges(GetComponentAttributes(), Scrollviewer));
            Scrollviewer.UpdateLayout();
        }

        private void SetShowButtonsVisibility()
        {
            if (settingsVm.ShowButtons && componentHandler.ComponentsAreLoaded())
            {
                ButtonVisibility = Visibility.Visible;
            }
            else
            {
                ButtonVisibility = Visibility.Hidden;
            }
        }
    }
}
