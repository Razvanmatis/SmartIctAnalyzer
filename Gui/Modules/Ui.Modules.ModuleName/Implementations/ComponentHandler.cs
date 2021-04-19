using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Interfaces.Gui;
using Interfaces.PcbInvestigator;
using ProMik.Core.Interfaces.Events;
using TestCoverage.Events;
using TestCoverage.Interfaces;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.Helper;
using Ui.Modules.ModuleName.Interfaces;
using Ui.Modules.ModuleName.ViewModels;

namespace Ui.Modules.ModuleName.Implementations
{
    public class ComponentHandler : IComponentHandler
    {
        private const double OffsetScrollbar = 25;
        private readonly ISettingsData settingsVm;
        private readonly ITestCoverageDeterminer testCoverageDeterminer;
        private readonly IEventService eventService;
        private readonly ILogger logger;
        private readonly ObservableCollection<ViewModelPCBBase> componentViews = new ObservableCollection<ViewModelPCBBase>();
        private readonly IList<IPCBComponent> allObjects = new List<IPCBComponent>();
        private readonly IList<INetComponent> allNets = new List<INetComponent>();
        private readonly Dictionary<TestCoverageObject, IList<IPCBComponent>> mapTestCoverageObjects = new Dictionary<TestCoverageObject, IList<IPCBComponent>>();
        private readonly List<string> actualLayers = new List<string>();
        private readonly IList<ViewModelPCBBase> allViewModelsForPainting = new List<ViewModelPCBBase>();
        private readonly List<string> actualTestCoverageObjects = new List<string>();
        private bool isClicked;
        private bool componentsAreLoaded;

        public ComponentHandler(IEventService eventService, ISettingsData settingsVm, ILogger logger, ITestCoverageDeterminer testCoverageDeterminer)
        {
            this.eventService = eventService;
            this.settingsVm = settingsVm;
            this.testCoverageDeterminer = testCoverageDeterminer;
            this.logger = logger;
        }

        public ComponentAttributes HandleButtonScroll(bool scrollIn, ComponentAttributes attributes)
        {
            if (scrollIn)
            {
                attributes.ZoomFactor += 0.1;
            }
            else if (attributes.ZoomFactor > 0)
            {
                attributes.ZoomFactor -= 0.1;
            }

            attributes.OverallWidth = attributes.OriginalWidth * attributes.ZoomFactor;
            attributes.OverallHeight = attributes.OriginalHeight * attributes.ZoomFactor;
            return attributes;
        }

        public void MouseMoveStartEndHandler(bool isBegin)
        {
            isClicked = isBegin;
        }

        public ComponentAttributes MoveMouseHandler(MouseEventArgs obj, ComponentAttributes attributes, ScrollViewer scrollviewer = null)
        {
            if (!isClicked)
            {
                attributes.ActualXPos = obj.GetPosition(scrollviewer).X + scrollviewer.HorizontalOffset;
                attributes.ActualYPos = obj.GetPosition(scrollviewer).Y + scrollviewer.VerticalOffset;
                attributes.XPosition = (attributes.ActualXPos / attributes.ZoomFactor) - (ViewModelPCBBase.OffsetX + ViewModelPCBBase.OFFSET);
                attributes.YPosition = (attributes.ActualYPos / attributes.ZoomFactor) - (ViewModelPCBBase.OffsetY + ViewModelPCBBase.OFFSET);
            }
            else if (obj.GetPosition(scrollviewer).X < scrollviewer.ActualWidth - OffsetScrollbar && obj.GetPosition(scrollviewer).Y < scrollviewer.ActualHeight - OffsetScrollbar)
            {
                double deltaX = attributes.ActualXPos - obj.GetPosition(scrollviewer).X;
                double deltaY = attributes.ActualYPos - obj.GetPosition(scrollviewer).Y;
                scrollviewer.ScrollToHorizontalOffset(deltaX);
                scrollviewer.ScrollToVerticalOffset(deltaY);
            }

            return attributes;
        }

        public ComponentAttributes DefineRanges(ComponentAttributes attributes, ScrollViewer scrollViewer = null)
        {
            attributes.ZoomFactor = 1.0;
            int maxWidth = 0;
            int maxHeight = 0;
            int minPosX = 0;
            int minPosY = 0;
            bool initValue = false;
            foreach (ViewModelPCBBase comp in allViewModelsForPainting)
            {
                if (comp is ViewModelPCBComponentBase compBase)
                {
                    int posX = comp.PosXToUse;
                    if (posX < minPosX || !initValue)
                    {
                        minPosX = posX;
                    }

                    int xValue = posX + compBase.GeometricAttributes.Bounds.Width;
                    if (xValue > maxWidth)
                    {
                        maxWidth = xValue;
                    }

                    int posY = comp.PosYToUse;
                    if (posY < minPosY || !initValue)
                    {
                        minPosY = posY;
                        initValue = true;
                    }

                    int yValue = posY + compBase.GeometricAttributes.Bounds.Height;
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
            attributes.OverallWidth = (maxWidth + offsetX + (2 * ViewModelPCBBase.OFFSET)) * attributes.ZoomFactor;
            attributes.OriginalWidth = attributes.OverallWidth;
            attributes.OverallHeight = (maxHeight + offsetY + (2 * ViewModelPCBBase.OFFSET)) * attributes.ZoomFactor;
            attributes.OriginalHeight = attributes.OverallHeight;
            componentViews.Remove(allViewModelsForPainting[allViewModelsForPainting.Count - 1]);
            componentViews.Add(allViewModelsForPainting[allViewModelsForPainting.Count - 1]);
            scrollViewer.UpdateLayout();
            logger.LogMessage("Updated view sizes by width: " + attributes.OverallWidth + " and height: " + attributes.OverallHeight, LogCategory.INFO);
            return attributes;
        }

        public bool ComponentsAreLoaded()
        {
            return componentsAreLoaded;
        }

        public ComponentAttributes ResetView(ResetViewEvent obj, ComponentAttributes attributes, Action setShowButtonsVisibility = null)
        {
            attributes.ZoomFactor = 1.0;
            attributes.OverallHeight = 0;
            attributes.OriginalWidth = 0;
            attributes.OriginalHeight = 0;
            attributes.OverallWidth = 0;
            componentViews?.Clear();
            actualLayers?.Clear();
            allNets?.Clear();
            allObjects?.Clear();
            allViewModelsForPainting?.Clear();
            componentsAreLoaded = false;
            setShowButtonsVisibility?.Invoke();
            eventService.Publish<ChangeExportMenuItemEnabledStateEvent>(new ChangeExportMenuItemEnabledStateEvent(false));
            return attributes;
        }

        public void OnFilesDropped(string[] files)
        {
            if (files.Length > 1)
            {
                logger.LogMessage("Please just drop one file at the same time!", LogCategory.WARNING);
            }
            else
            {
                eventService.Publish(new HandleDropEvent(files[0]));
            }
        }

        public ObservableCollection<ViewModelPCBBase> GetComponentsView()
        {
            return componentViews;
        }

        public void MirrorAxis(string xAxis, Action defineRangesAction = null)
        {
            eventService.Publish(new ResetLayersSelectionEvent());
            foreach (ViewModelPCBBase comp in allViewModelsForPainting)
            {
                if (comp is ViewModelPCBComponentBase compBase)
                {
                    compBase.MirrorAxis(xAxis);
                }
            }

            ViewModelPCBComponentBase.MirrorAxis(bool.TrueString.Equals(xAxis));
            defineRangesAction?.Invoke();
        }

        public ComponentAttributes MouseWheelHandler(MouseWheelEventArgs obj, ComponentAttributes attributes)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && componentViews.Count > 0)
            {
                attributes.ZoomFactor = obj.Delta >= 0 ? attributes.ZoomFactor + 0.1 : attributes.ZoomFactor - 0.1;
                attributes.OverallWidth = attributes.OriginalWidth * attributes.ZoomFactor;
                attributes.OverallHeight = attributes.OriginalHeight * attributes.ZoomFactor;
                obj.Handled = true;
            }

            return attributes;
        }

        public void HandleUpdateBomEvent(UpdateBomDataEvent obj)
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

        public void RefreshTestCoverageResults(RefreshTestcoverageResultObjectsEvent obj)
        {
            foreach (var comp in componentViews)
            {
                if (comp is ViewModelLine lineVm)
                {
                    lineVm.CheckForTestCoverage();
                }
            }
        }

        public async Task AddPCBComponent(AddPcbObjectsEvent comp, Action defineRangesAction = null, Action setShowButtonsVisibility = null, Action resetObjectsAction = null)
        {
            await Task.Run(() =>
            {
                resetObjectsAction?.Invoke();
            }).ConfigureAwait(true);

            allObjects.Clear();
            foreach (var compToConsider in comp.ComponentList)
            {
                allObjects.Add(compToConsider);
            }

            allNets.Clear();
            foreach (var net in comp.Nets)
            {
                allNets.Add(net);
            }

            actualLayers.Clear();
            allViewModelsForPainting.Clear();
            componentViews.Clear();
            foreach (IPCBComponent compInner in allObjects)
            {
                allViewModelsForPainting.Add(ViewModelFactory.GetViewModelObject(compInner, settingsVm, this));
            }

            allViewModelsForPainting.Add(new CoordinatesOriginViewModel(0, 0));
            defineRangesAction?.Invoke();
            componentsAreLoaded = true;
            setShowButtonsVisibility?.Invoke();
            eventService.Publish<SetBusyEvent>(new SetBusyEvent(false));
            eventService.Publish<ChangeExportMenuItemEnabledStateEvent>(new ChangeExportMenuItemEnabledStateEvent(true));
            eventService.Publish<ComponentsImportFinishedEvent>(new ComponentsImportFinishedEvent());
        }

        public void CloseAllSettingsEventHandling(CloseAllSettingsEvent obj, Action setShowButtonsVisibilityAction = null)
        {
            if (!obj.WasManuallyClosed && componentViews != null && componentViews.Count > 0)
            {
                foreach (var comp in componentViews)
                {
                    if (comp is ViewModelPCBComponentBase compVm)
                    {
                        compVm.InitSettings();
                    }
                }
            }

            setShowButtonsVisibilityAction?.Invoke();
        }

        public void AddTestCoverageObjectToView(AddTestCoverageObjectsEvent objects)
        {
            mapTestCoverageObjects.Clear();
            foreach (var obj in objects.GetObjects())
            {
                mapTestCoverageObjects.Add(obj.Key, obj.Value);
            }
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

        public void ShowTestCoverageObjects(ShowObjectsEvent obj)
        {
            List<string> layersToRemove = GetLayersToRemove(obj.GetObjects(), actualTestCoverageObjects);
            List<string> layersToAdd = GetLayersToAdd(obj.GetObjects(), actualTestCoverageObjects);
            RemoveTestCoverageObjectsFromView(layersToRemove);
            AddTestCoverageObjectsToView(layersToAdd);
            actualLayers.Clear();
            actualTestCoverageObjects.Clear();
            actualTestCoverageObjects.AddRange(obj.GetObjects());
        }

        public async Task ToggleShowConnections(ViewModelPCBComponentBase comp, bool shouldShow)
        {
            await Task.Run(() =>
            {
                if (shouldShow)
                {
                    Stopwatch sw = Stopwatch.StartNew();
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
                                        PositionHelper pOut = new PositionHelper(GetTranslatedBounds(comp.GeometricAttributes.Bounds));
                                        PositionHelper pIn = new PositionHelper(GetTranslatedBounds(compOut.GeometricAttributes.Bounds));
                                        PointCollection points = pOut.GetPointsForObject(pIn);
                                        ViewModelLine lineVm = new ViewModelLine(points, points[^1], net.NetName, settingsVm, testCoverageDeterminer);
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
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            componentViews.AddRange(lines);
                            logger.LogMessage("Show connections for component " + comp.BaseComponent.FunctionalAttributes.Ref + " with total amount: " + lines.Count, LogCategory.INFO);
                        });
                    }

                    sw.Stop();
                    Debug.WriteLine("Showed connections: {0}ms", sw.Elapsed.TotalMilliseconds);
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
            else if (layer.Equals("JTAG"))
            {
                return TestCoverageObject.JTAG;
            }
            else
            {
                return TestCoverageObject.OTHERS;
            }
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

        private static Rectangle GetTranslatedBounds(Rectangle bounds)
        {
            if (!ViewModelPCBComponentBase.MirrorUsed(true) && !ViewModelPCBComponentBase.MirrorUsed(false))
            {
                return bounds;
            }

            int x = bounds.X;
            int y = bounds.Y;
            if (ViewModelPCBComponentBase.MirrorUsed(true))
            {
                y *= -1;
                if (bounds.Y < 0)
                {
                    y -= bounds.Height;
                }
                else
                {
                    y += bounds.Height;
                }
            }

            if (ViewModelPCBComponentBase.MirrorUsed(false))
            {
                x *= -1;
                if (bounds.X < 0)
                {
                    y += bounds.Width;
                }
                else
                {
                    y -= bounds.Width;
                }
            }

            return new Rectangle(x, y, bounds.Width, bounds.Height);
        }

        private ViewModelPCBComponentBase GetVmObject(IPCBComponent compOut)
        {
            foreach (var obj in componentViews)
            {
                if (obj is ViewModelPCBComponentBase comp && comp.BaseComponent == compOut)
                {
                    return comp;
                }
            }

            return null;
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
            if (convertedList.Count > 0)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    logger.LogMessage("Inserted total amount of objects to be visible: " + convertedList.Count, LogCategory.INFO);
                });
            }
        }

        private void InsertObjectsIntoView(List<ViewModelPCBBase> convertedList)
        {
            foreach (var comp in convertedList)
            {
                if (!componentViews.Contains(comp))
                {
                    Application.Current.Dispatcher.Invoke(() => componentViews.Add(comp));
                }
            }
        }

        private void RemoveLayerObjectsFromView(List<string> layersToRemove)
        {
            List<ViewModelPCBComponentBase> compsToRemove = new List<ViewModelPCBComponentBase>();
            foreach (var layerToRem in layersToRemove)
            {
                foreach (var comp in componentViews)
                {
                    if (comp is ViewModelPCBComponentBase compBase && compBase.BaseComponent.FunctionalAttributes.LayerName.Equals(layerToRem))
                    {
                        compsToRemove.Add(compBase);
                    }
                }
            }

            RemoveObjectsFromView(compsToRemove);
            if (compsToRemove.Count > 0)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    logger.LogMessage("Removed total amount of objects from view: " + compsToRemove.Count, LogCategory.INFO);
                });
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

                Application.Current.Dispatcher.Invoke(() => componentViews.Remove(rem));
            }
        }

        private void RemoveTestCoverageObjectsFromView(List<string> layersToRemove)
        {
            List<ViewModelPCBComponentBase> compsToRemoveTotal = new List<ViewModelPCBComponentBase>();
            foreach (string layer in layersToRemove)
            {
                TestCoverageObject objectType = GetTestCoverageObjectType(layer);
                foreach (var objToRemove in mapTestCoverageObjects[objectType])
                {
                    List<ViewModelPCBComponentBase> compsToRemove = new List<ViewModelPCBComponentBase>();
                    foreach (var comp in componentViews)
                    {
                        if (comp is ViewModelPCBComponentBase compBase && compBase.BaseComponent == objToRemove)
                        {
                            compsToRemove.Add(compBase);
                            if (!compsToRemoveTotal.Contains(compBase))
                            {
                                compsToRemoveTotal.Add(compBase);
                            }
                        }
                    }

                    RemoveObjectsFromView(compsToRemove);
                }
            }

            if (compsToRemoveTotal.Count > 0)
            {
                logger.LogMessage("Removed total amount of test coverage related objects from view: " + compsToRemoveTotal.Count, LogCategory.INFO);
            }
        }

        private void AddTestCoverageObjectsToView(List<string> layersToAdd)
        {
            List<ViewModelPCBBase> convertedListTotal = new List<ViewModelPCBBase>();
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
                            if (!convertedListTotal.Contains(compInner))
                            {
                                convertedListTotal.Add(compInner);
                            }
                        }
                    }

                    InsertObjectsIntoView(convertedList);
                }
            }

            if (convertedListTotal.Count > 0)
            {
                logger.LogMessage("Inserted total amount of test coverage related objects into view: " + convertedListTotal.Count, LogCategory.INFO);
            }
        }
    }
}
