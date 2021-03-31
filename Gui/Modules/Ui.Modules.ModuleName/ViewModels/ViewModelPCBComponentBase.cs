using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Interfaces.PcbInvestigator;
using Prism.Commands;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.ViewModels
{
    public abstract class ViewModelPCBComponentBase : ViewModelPCBBase
    {
        private const double OPACITYMIN = 0.1;
        private static bool xAxisMirror;
        private static bool yAxisMirror;
        private readonly ISettingsData settingsVm;
        private readonly IPCBComponent component;
        private readonly ObservableCollection<ViewModelLine> lines = new ObservableCollection<ViewModelLine>();
        private readonly IComponentViewModel componentVm;
        private bool connectionsShowed;
        private string name;
        private SolidColorBrush brush;
        private string valueInner;
        private SolidColorBrush borderColor;
        private int borderThickness;
        private Visibility toolTipVisible;

        public ViewModelPCBComponentBase(IPCBComponent component, ISettingsData settingsVm, IComponentViewModel componentVm)
            : base(component.GeometricAttributes.Bounds.X, component.GeometricAttributes.Bounds.Y)
        {
            this.componentVm = componentVm;
            this.component = component;
            this.settingsVm = settingsVm;
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

        public static bool MirrorUsed(bool xAxis)
        {
            if (xAxis)
            {
                return xAxisMirror;
            }
            else
            {
                return yAxisMirror;
            }
        }

        public static void MirrorAxis(bool xAxis)
        {
            if (xAxis)
            {
                xAxisMirror = !xAxisMirror;
            }
            else
            {
                yAxisMirror = !yAxisMirror;
            }
        }

        public void MirrorAxis(string xAxis)
        {
            if (bool.TrueString.Equals(xAxis))
            {
                if (PosYToUse != component.GeometricAttributes.Bounds.Y)
                {
                    PosYToUse = component.GeometricAttributes.Bounds.Y;
                }
                else
                {
                    PosYToUse *= -1;
                    if (component.GeometricAttributes.Bounds.Y < 0)
                    {
                        PosYToUse -= component.GeometricAttributes.Bounds.Height;
                    }
                    else
                    {
                        PosYToUse += component.GeometricAttributes.Bounds.Height;
                    }
                }
            }
            else
            {
                if (PosXToUse != component.GeometricAttributes.Bounds.X)
                {
                    PosXToUse = component.GeometricAttributes.Bounds.X;
                }
                else
                {
                    PosXToUse *= -1;
                    if (component.GeometricAttributes.Bounds.X < 0)
                    {
                        PosXToUse += component.GeometricAttributes.Bounds.Width;
                    }
                    else
                    {
                        PosXToUse -= component.GeometricAttributes.Bounds.Width;
                    }
                }
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
