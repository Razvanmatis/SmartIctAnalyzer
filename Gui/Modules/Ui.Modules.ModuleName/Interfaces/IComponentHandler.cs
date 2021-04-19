using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using TestCoverage.Events;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.Helper;
using Ui.Modules.ModuleName.ViewModels;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface IComponentHandler
    {
        ObservableCollection<ViewModelPCBBase> GetComponentsView();

        bool ComponentsAreLoaded();

        void CloseAllSettingsEventHandling(CloseAllSettingsEvent obj, Action setShowButtonsVisibilityAction = null);

        Task AddPCBComponent(
            AddPcbObjectsEvent comp,
            Action defineRangesAction = null,
            Action setShowButtonsVisibility = null,
            Action resetObjectsAction = null);

        Task ToggleCompleteVisibility(Type typeToHide);

        Task ToggleShowConnections(ViewModelPCBComponentBase comp, bool shouldShow);

        Task ShowLayerObjects(IList<string> layersToShow);

        void ShowTestCoverageObjects(ShowObjectsEvent obj);

        void AddTestCoverageObjectToView(AddTestCoverageObjectsEvent objects);

        void HandleUpdateBomEvent(UpdateBomDataEvent obj);

        void RefreshTestCoverageResults(RefreshTestcoverageResultObjectsEvent obj);

        void MouseMoveStartEndHandler(bool isBegin);

        ComponentAttributes MoveMouseHandler(MouseEventArgs obj, ComponentAttributes attributes, ScrollViewer scrollViewer = null);

        ComponentAttributes MouseWheelHandler(MouseWheelEventArgs obj, ComponentAttributes attributes);

        ComponentAttributes HandleButtonScroll(bool scrollIn, ComponentAttributes attributes);

        void MirrorAxis(string xAxis, Action defineRangesAction = null);

        ComponentAttributes DefineRanges(ComponentAttributes attributes, ScrollViewer scrollViewer = null);

        void OnFilesDropped(string[] files);

        ComponentAttributes ResetView(ResetViewEvent obj, ComponentAttributes attributes, Action setShowButtonsVisibility = null);
    }
}
