using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GrpcClientParser.Implementations;
using Interfaces;
using Interfaces.Gui;
using Interfaces.PCBApiObjects;
using Interfaces.PcbInvestigator;
using Interfaces.TestCoverage;
using Prism.Commands;
using Prism.Regions;
using ProMik.Services.Interfaces;
using ProMik.Services.Interfaces.Enums;
using Ui.Core.Mvvm;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.Helper;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class ComponentViewModel : RegionViewModelBase, IComponentViewModel
    {
        private readonly double offsetScrollbar = 25;
        private string message;
        private ObservableCollection<ViewModelPCBBase> componentViews;
        private double overallWidth;
        private double overallHeight;
        private double zoomFactor = 1;
        private double originalWidth;
        private double originalHeight;
        private double actualXPos;
        private double actualYPos;
        private bool isClicked;
        private IEventService eventService;
        private double realXPos;
        private double realYPos;
        private IList<IPCBComponent> allObjects;
        private IList<INetComponent> allNets;
        private List<string> actualLayers = new List<string>();
        private List<string> actualTestCoverageObjects = new List<string>();
        private IList<ViewModelPCBBase> allViewModelsForPainting = new List<ViewModelPCBBase>();
        private IGeneralSettingsData settingsVm;
        private Dictionary<TestCoverageObject, IList<IPCBComponent>> mapTestCoverageObjects = new Dictionary<TestCoverageObject, IList<IPCBComponent>>();
        private ITestCoverageDeterminer testCoverageDeterminer;

        public ComponentViewModel(
            IRegionManager regionManager,
            IMessageService messageService,
            IEventService eventService,
            IGeneralSettingsData settingsVm,
            ITestCoverageDeterminer testCoverageDeterminer)
            : base(regionManager)
        {
            this.settingsVm = settingsVm;
            this.testCoverageDeterminer = testCoverageDeterminer;
            eventService.Subscribe<AddPcbObjectsEvent>(async (x) => await AddPCBComponent(x).ConfigureAwait(true), ThreadOption.UIThread);
            eventService.Subscribe<ResetViewEvent>(ResetView, ThreadOption.UIThread);
            eventService.Subscribe<AddTestCoverageObjectsEvent>(AddTestCoverageObjectToView, ThreadOption.UIThread);
            eventService.Subscribe<ShowObjectsEvent>(ShowTestCoverageObjects, ThreadOption.UIThread);
            eventService.Subscribe<RefreshTestcoverageResultObjects>(RefreshTestCoverageResults);
            eventService.Subscribe<CloseGeneralSettingsEvent>(CloseGeneralSettingsEventHandling);
            Message = messageService.GetMessage();
            eventService.Subscribe<UpdateBomDataEvent>(HandleUpdateBomEvent);
            this.eventService = eventService;
            componentViews = new ObservableCollection<ViewModelPCBBase>();
            MouseWheelCommand = new DelegateCommand<MouseWheelEventArgs>(MouseWheelHandler);
            MouseMove = new DelegateCommand<MouseEventArgs>(MoveMouseHandler);
            MouseMoveStart = new DelegateCommand<MouseButtonEventArgs>((x) => MouseMoveStartEndHandler(true));
            MouseMoveEnd = new DelegateCommand<MouseButtonEventArgs>((x) => MouseMoveStartEndHandler(false));
            MouseLeave = new DelegateCommand<MouseEventArgs>((x) => MouseMoveStartEndHandler(false));
        }

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

        public ScrollViewer Scrollviewer
        {
            get;
            set;
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

        public async Task AddPCBComponent(AddPcbObjectsEvent comp)
        {
            await Task.Run(() =>
            {
                ZoomFactor = 1.0;
                OverallWidth = 0;
                OverallHeight = 0;
            }).ConfigureAwait(true);

            Scrollviewer.UpdateLayout();
            this.allObjects = comp.ComponentList;
            this.allNets = comp.Nets;
            actualLayers.Clear();
            allViewModelsForPainting.Clear();
            ComponentViews.Clear();
            foreach (IPCBComponent compInner in this.allObjects)
            {
                allViewModelsForPainting.Add(ViewModelFactory.GetViewModelObject(compInner, eventService, settingsVm, this));
            }

            DefineRanges();
            eventService.Publish<SetBusyEvent>(new SetBusyEvent(false));
            eventService.Publish<ChangeExportMenuItemEnabledStateEvent>(new ChangeExportMenuItemEnabledStateEvent(true));
            eventService.Publish<ComponentsImportFinishedEvent>(new ComponentsImportFinishedEvent());
        }

        public async Task ShowLayerObjects(IList<string> layersToShow)
        {
            await Task.Run(() =>
            {
                List<string> layersToRemove = GetLayersToRemove(layersToShow, actualLayers);
                List<string> layersToAdd = GetLayersToAdd(layersToShow, actualLayers);
                Application.Current.Dispatcher.Invoke(() => RemoveLayerObjectsFromView(layersToRemove));
                AddLayerObjectsToView(layersToAdd);
                actualLayers.Clear();
                actualLayers.AddRange(layersToShow);
                actualTestCoverageObjects.Clear();
            }).ConfigureAwait(false);
        }

        public async Task ToggleCompleteVisibility(Type typeToHide)
        {
            await Task.Run(() =>
            {
                foreach (ViewModelPCBBase comp in componentViews)
                {
                    if (comp is ViewModelPCBComponentBase compToTrigger && compToTrigger.GetType().Equals(typeToHide))
                    {
                        compToTrigger.ToggleVisiblity.Execute(null);
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task ToggleShowConnections(ViewModelPCBComponentBase comp, bool shouldShow)
        {
            await Task.Run(() =>
            {
                if (shouldShow)
                {
                    List<ViewModelLine> lines = new List<ViewModelLine>();

                    foreach (var net in allNets)
                    {
                        if (net.Components.Contains(comp.BaseComponent))
                        {
                            foreach (var compOut in net.Components)
                            {
                                if (compOut != comp.BaseComponent)
                                {
                                    ViewModelPCBComponentBase compWithLines = GetVmObject(compOut);
                                    if (compWithLines != null)
                                    {
                                        PositionHelper pOut = new PositionHelper(comp.GeometricAttributes.Bounds);
                                        PositionHelper pIn = new PositionHelper(compOut.GeometricAttributes.Bounds);
                                        PointCollection points = pOut.GetPointsForObject(pIn);
                                        ViewModelLine lineVm = new ViewModelLine(points, points[points.Count - 1], net.NetName, settingsVm, testCoverageDeterminer);
                                        lineVm.ConnectedComponents.Add(comp.BaseComponent);
                                        lineVm.ConnectedComponents.Add(compOut);
                                        lineVm.CheckForTestCoverage();
                                        lines.Add(lineVm);
                                        comp.Lines.Add(lineVm);
                                        compWithLines.Lines.Add(lineVm);
                                    }
                                }
                            }
                        }
                    }

                    if (lines.Count > 0)
                    {
                        Application.Current.Dispatcher.Invoke(() => componentViews.AddRange(lines));
                    }
                }
                else
                {
                    foreach (var line in comp.Lines)
                    {
                        Application.Current.Dispatcher.Invoke(() => componentViews.Remove(line));
                    }

                    comp.Lines.Clear();
                }
            }).ConfigureAwait(false);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            // do something
        }

        private static List<string> GetLayersToAdd(IList<string> layersToShow, IList<string> layersBase)
        {
            List<string> toAdd = new List<string>();
            foreach (var layer in layersToShow)
            {
                if (!layersBase.Contains(layer))
                {
                    toAdd.Add(layer);
                }
            }

            return toAdd;
        }

        private static List<string> GetLayersToRemove(IList<string> layersToShow, IList<string> layersBase)
        {
            List<string> toRemove = new List<string>();
            foreach (var layer in layersBase)
            {
                if (!layersToShow.Contains(layer))
                {
                    toRemove.Add(layer);
                }
            }

            return toRemove;
        }

        private static TestCoverageObject GetTestCoverageObjectType(string layer)
        {
            if (layer.Equals("Pullups"))
            {
                return TestCoverageObject.PULLUP;
            }
            else if (layer.Equals("Pulldowns"))
            {
                return TestCoverageObject.PULLDOWN;
            }
            else
            {
                return TestCoverageObject.JTAG;
            }
        }

        private void ResetView(ResetViewEvent obj)
        {
            ZoomFactor = 1.0;
            OverallHeight = 0;
            originalWidth = 0;
            originalHeight = 0;
            OverallWidth = 0;
            ComponentViews?.Clear();
            actualLayers?.Clear();
            allNets?.Clear();
            allObjects?.Clear();
            allViewModelsForPainting?.Clear();
            eventService.Publish<ChangeExportMenuItemEnabledStateEvent>(new ChangeExportMenuItemEnabledStateEvent(false));
        }

        private void AddLayerObjectsToView(List<string> layersToAdd)
        {
            List<ViewModelPCBBase> convertedList = new List<ViewModelPCBBase>();
            foreach (var layer in layersToAdd)
            {
                foreach (ViewModelPCBBase compInner in this.allViewModelsForPainting)
                {
                    if (compInner is ViewModelPCBComponentBase compInnerBase && compInnerBase.BaseComponent.FunctionalAttributes.LayerName.Equals(layer))
                    {
                        convertedList.Add(compInner);
                    }
                }
            }

            InsertObjectsIntoView(convertedList);
        }

        private void RemoveLayerObjectsFromView(List<string> layersToRemove)
        {
            List<ViewModelPCBComponentBase> compsToRemove = new List<ViewModelPCBComponentBase>();
            foreach (var layerToRem in layersToRemove)
            {
                foreach (var comp in ComponentViews)
                {
                    if (comp is ViewModelPCBComponentBase compBase && compBase.BaseComponent.FunctionalAttributes.LayerName.Equals(layerToRem))
                    {
                        compsToRemove.Add(compBase);
                    }
                }
            }

            RemoveObjectsFromView(compsToRemove);
        }

        private void ShowTestCoverageObjects(ShowObjectsEvent obj)
        {
            List<string> layersToRemove = GetLayersToRemove(obj.GetObjects(), actualTestCoverageObjects);
            List<string> layersToAdd = GetLayersToAdd(obj.GetObjects(), actualTestCoverageObjects);
            RemoveTestCoverageObjectsFromView(layersToRemove);
            AddTestCoverageObjectsToView(layersToAdd);
            actualLayers.Clear();
            actualTestCoverageObjects.Clear();
            actualTestCoverageObjects.AddRange(obj.GetObjects());
        }

        private void AddTestCoverageObjectsToView(List<string> layersToAdd)
        {
            foreach (string layer in layersToAdd)
            {
                TestCoverageObject objectType = GetTestCoverageObjectType(layer);
                foreach (var objToAdd in mapTestCoverageObjects[objectType])
                {
                    List<ViewModelPCBBase> convertedList = new List<ViewModelPCBBase>();
                    foreach (ViewModelPCBBase compInner in this.allViewModelsForPainting)
                    {
                        if (compInner is ViewModelPCBComponentBase compInnerBase && compInnerBase.BaseComponent == objToAdd)
                        {
                            convertedList.Add(compInner);
                        }
                    }

                    InsertObjectsIntoView(convertedList);
                }
            }
        }

        private void InsertObjectsIntoView(List<ViewModelPCBBase> convertedList)
        {
            foreach (var comp in convertedList)
            {
                if (!ComponentViews.Contains(comp))
                {
                    Application.Current.Dispatcher.Invoke(() => ComponentViews.Add(comp));
                }
            }
        }

        private void RemoveTestCoverageObjectsFromView(List<string> layersToRemove)
        {
            foreach (string layer in layersToRemove)
            {
                TestCoverageObject objectType = GetTestCoverageObjectType(layer);
                foreach (var objToRemove in mapTestCoverageObjects[objectType])
                {
                    List<ViewModelPCBComponentBase> compsToRemove = new List<ViewModelPCBComponentBase>();
                    foreach (var comp in ComponentViews)
                    {
                        if (comp is ViewModelPCBComponentBase compBase && compBase.BaseComponent == objToRemove)
                        {
                            compsToRemove.Add(compBase);
                        }
                    }

                    RemoveObjectsFromView(compsToRemove);
                }
            }
        }

        private void RemoveObjectsFromView(List<ViewModelPCBComponentBase> compsToRemove)
        {
            foreach (var rem in compsToRemove)
            {
                if (rem.AreConnectionShowed)
                {
                    rem.ToggleConnections.Execute(null);
                }

                Application.Current.Dispatcher.Invoke(() => ComponentViews.Remove(rem));
            }
        }

        private void AddTestCoverageObjectToView(AddTestCoverageObjectsEvent objects)
        {
            mapTestCoverageObjects.Clear();
            foreach (var obj in objects.GetObjects())
            {
                mapTestCoverageObjects.Add(obj.Key, obj.Value);
            }
        }

        private void HandleUpdateBomEvent(UpdateBomDataEvent obj)
        {
            foreach (var comp in allViewModelsForPainting)
            {
                if (comp is ViewModelPCBComponentBase baseVm)
                {
                    baseVm.Value = baseVm.BaseComponent.FunctionalAttributes.Value;
                    baseVm.InitValues(true);
                }
            }
        }

        private ViewModelPCBComponentBase GetVmObject(IPCBComponent compOut)
        {
            foreach (var obj in ComponentViews)
            {
                if (obj is ViewModelPCBComponentBase comp && comp.BaseComponent == compOut)
                {
                    return comp;
                }
            }

            return null;
        }

        private void MoveMouseHandler(MouseEventArgs obj)
        {
            if (!isClicked)
            {
                actualXPos = obj.GetPosition(Scrollviewer).X + Scrollviewer.HorizontalOffset;
                actualYPos = obj.GetPosition(Scrollviewer).Y + Scrollviewer.VerticalOffset;
                XPosition = actualXPos / ZoomFactor;
                YPosition = actualYPos / ZoomFactor;
            }
            else if (obj.GetPosition(Scrollviewer).X < Scrollviewer.ActualWidth - offsetScrollbar && obj.GetPosition(Scrollviewer).Y < Scrollviewer.ActualHeight - offsetScrollbar)
            {
                double deltaX = actualXPos - obj.GetPosition(Scrollviewer).X;
                double deltaY = actualYPos - obj.GetPosition(Scrollviewer).Y;
                Scrollviewer.ScrollToHorizontalOffset(deltaX);
                Scrollviewer.ScrollToVerticalOffset(deltaY);
            }
        }

        private void MouseMoveStartEndHandler(bool isBegin)
        {
            isClicked = isBegin;
        }

        private void MouseWheelHandler(MouseWheelEventArgs obj)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && componentViews.Count > 0)
            {
                ZoomFactor = obj.Delta >= 0 ? ZoomFactor + 0.1 : ZoomFactor - 0.1;
                OverallWidth = originalWidth * ZoomFactor;
                OverallHeight = originalHeight * ZoomFactor;
                obj.Handled = true;
                return;
            }
        }

        private void DefineRanges()
        {
            int maxWidth = 0;
            int maxHeight = 0;
            int minPosX = 0;
            int minPosY = 0;
            foreach (ViewModelPCBBase comp in allViewModelsForPainting)
            {
                if (comp is ViewModelPCBComponentBase compBase)
                {
                    int xValue = compBase.GeometricAttributes.Bounds.X + compBase.GeometricAttributes.Bounds.Width;
                    if (xValue > maxWidth)
                    {
                        maxWidth = xValue;
                    }

                    int posX = compBase.GeometricAttributes.Bounds.X;
                    if (posX < 0 && posX < minPosX)
                    {
                        minPosX = posX;
                    }

                    int posY = compBase.GeometricAttributes.Bounds.Y;
                    if (posY < 0 && posY < minPosY)
                    {
                        minPosY = posY;
                    }

                    int yValue = compBase.GeometricAttributes.Bounds.Y + compBase.GeometricAttributes.Bounds.Height;
                    if (yValue > maxHeight)
                    {
                        maxHeight = yValue;
                    }
                }
            }

            int offsetX = minPosX * -1;
            int offsetY = minPosY * -1;
            ViewModelPCBBase.SetOffsets(offsetX, offsetY);
            ViewModelLine.SetOffsets(offsetX, offsetY);
            OverallWidth = (maxWidth + offsetX + (2 * ViewModelPCBBase.OFFSET)) * ZoomFactor;
            originalWidth = OverallWidth;
            OverallHeight = (maxHeight + offsetY + (2 * ViewModelPCBBase.OFFSET)) * ZoomFactor;
            originalHeight = OverallHeight;
            Scrollviewer.UpdateLayout();
        }

        private void RefreshTestCoverageResults(RefreshTestcoverageResultObjects obj)
        {
            foreach (var comp in componentViews)
            {
                if (comp is ViewModelLine lineVm)
                {
                    lineVm.CheckForTestCoverage();
                }
            }
        }

        private void CloseGeneralSettingsEventHandling(CloseGeneralSettingsEvent obj)
        {
            if (!obj.WasClosedManually && componentViews != null && componentViews.Count > 0)
            {
                foreach (var comp in componentViews)
                {
                    if (comp is ViewModelPCBComponentBase compVm)
                    {
                        compVm.InitSettings();
                    }
                }
            }
        }
    }
}
