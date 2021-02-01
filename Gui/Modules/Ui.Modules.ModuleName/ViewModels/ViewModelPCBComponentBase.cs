using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Interfaces.PcbInvestigator;
using Prism.Commands;
using ProMik.Services.Interfaces;
using Ui.Core.Mvvm;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.ViewModels
{
    public abstract class ViewModelPCBComponentBase : ViewModelPCBBase
    {
        private const double OPACITYMIN = 0.1;
        private IGeneralSettingsData settingsVm;
        private IPCBComponent component;
        private bool connectionsShowed;
        private string name;
        private IEventService eventService;
        private ObservableCollection<ViewModelLine> lines = new ObservableCollection<ViewModelLine>();
        private SolidColorBrush brush;
        private string valueInner;
        private SolidColorBrush borderColor;
        private int borderThickness;
        private Visibility toolTipVisible;
        private IComponentViewModel componentVm;

        public ViewModelPCBComponentBase(IPCBComponent component, IEventService eventService, IGeneralSettingsData settingsVm, IComponentViewModel componentVm)
            : base(component.GeometricAttributes.Bounds.X, component.GeometricAttributes.Bounds.Y)
        {
            this.componentVm = componentVm;
            this.component = component;
            this.settingsVm = settingsVm;
            this.eventService = eventService;
            ToggleVisiblity = new DelegateCommand(ToggleVisibleState);
            ToggleVisiblityAll = new DelegateCommand(async () => await ToggleVisibiltyAll().ConfigureAwait(false));
            ToggleConnections = new DelegateCommand(async () => await ToggleConnectionsShowing().ConfigureAwait(false));
            Name = component?.FunctionalAttributes?.Ref;
            Value = component?.FunctionalAttributes?.Value;
            InitValues(false);
        }

        public IGeometricAttributes GeometricAttributes
        {
            get
            {
                return component?.GeometricAttributes;
            }
        }

        public SolidColorBrush BorderColor
        {
            get
            {
                return borderColor;
            }

            set
            {
                SetProperty(ref borderColor, value);
            }
        }

        public int BorderThickness
        {
            get
            {
                return borderThickness;
            }

            set
            {
                SetProperty(ref borderThickness, value);
            }
        }

        public SolidColorBrush BackgroundColor
        {
            get
            {
                return brush;
            }

            set
            {
                SetProperty(ref brush, value);
            }
        }

        public string Value
        {
            get
            {
                return valueInner;
            }

            set
            {
                SetProperty(ref valueInner, value);
            }
        }

        public Visibility ToolTipVisible
        {
            get
            {
                return toolTipVisible;
            }

            set
            {
                SetProperty(ref toolTipVisible, value);
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                SetProperty(ref name, value);
            }
        }

        public ObservableCollection<ViewModelLine> Lines
        {
            get
            {
                return lines;
            }
        }

        public IPCBComponent BaseComponent
        {
            get
            {
                return component;
            }
        }

        public ICommand ToggleVisiblity { get; set; }

        public ICommand ToggleVisiblityAll { get; set; }

        public ICommand ToggleConnections { get; set; }

        public int FontSize
        {
            get
            {
                return settingsVm.FontSizeNames;
            }
        }

        public bool AreConnectionShowed
        {
            get
            {
                return connectionsShowed;
            }
        }

        public abstract void InitSettings();

        public void InitValues(bool useValue)
        {
            if ((useValue || settingsVm.UseValues) && string.IsNullOrEmpty(Value))
            {
                BorderColor = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                BorderThickness = 3;
            }
            else
            {
                BorderColor = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                BorderThickness = 1;
            }

            if (string.IsNullOrEmpty(Value))
            {
                ToolTipVisible = Visibility.Hidden;
            }
            else
            {
                ToolTipVisible = Visibility.Visible;
            }
        }

        private async Task ToggleConnectionsShowing()
        {
            connectionsShowed = !connectionsShowed;
            await componentVm.ToggleShowConnections(this, connectionsShowed).ConfigureAwait(false);
        }

        private void ToggleVisibleState()
        {
            Opacity = Opacity == ViewModelPCBBase.OPACITYMAX ? OPACITYMIN : ViewModelPCBBase.OPACITYMAX;
        }

        private async Task ToggleVisibiltyAll()
        {
            await componentVm.ToggleCompleteVisibility(GetType()).ConfigureAwait(false);
        }
    }
}
