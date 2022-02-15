using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Prism.Commands;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.TestCoverageDeterminer.Interfaces;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class ViewModelLine : ViewModelPCBBase
    {
        private static int offsetX;
        private static int offsetY;
        private readonly double posXLabel;
        private readonly double posyLabel;
        private readonly StreamGeometry geometry = new StreamGeometry();
        private readonly ITestCoverageDeterminer testCoverageDeterminer;
        private readonly ObservableCollection<IPCBComponent> connections = new ObservableCollection<IPCBComponent>();
        private string objectNames;
        private Visibility lineVisibility;
        private Brush lineColor = Brushes.Black;

        public ViewModelLine(
            PointCollection points,
            Point pointOfNetName,
            string netName,
            ISettingsData settingsVm,
            ITestCoverageDeterminer testCoverageDeterminer)
            : base(points[0].X, points[0].Y)
        {
            this.testCoverageDeterminer = testCoverageDeterminer;
            if (settingsVm.NetNamesChecked)
            {
                posXLabel = pointOfNetName.X;
                posyLabel = pointOfNetName.Y;
                NetName = netName;
            }

            geometry.FillRule = FillRule.EvenOdd;
            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(new Point(PosXWithOffset, PosYWithOffset), true, false);
                for (int p = 1; p < points.Count; p++)
                {
                    ctx.LineTo(new Point(points[p].X + offsetX + OFFSET, points[p].Y + offsetY + OFFSET), true, false);
                }
            }

            Hide = new DelegateCommand(HideLine);
            geometry.Freeze();
            ObjectNames = netName;
            connections.CollectionChanged += OnCollectionChanged;
            LineVisibility = Visibility.Visible;
        }

        public string ObjectNames
        {
            get
            {
                return objectNames;
            }

            set
            {
                SetProperty(ref objectNames, value);
            }
        }

        public ICommand Hide
        {
            get;
        }

        public Visibility LineVisibility
        {
            get
            {
                return lineVisibility;
            }

            set
            {
                SetProperty(ref lineVisibility, value);
            }
        }

        public double XPositionNetName
        {
            get
            {
                return posXLabel + offsetX + OFFSET;
            }
        }

        public double YPositionNetName
        {
            get
            {
                return posyLabel + offsetY + OFFSET;
            }
        }

        public string NetName
        {
            get;
        }

        public StreamGeometry Points
        {
            get
            {
                return geometry;
            }
        }

        public Brush LineColor
        {
            get
            {
                return lineColor;
            }

            set
            {
                SetProperty(ref lineColor, value);
            }
        }

        public ObservableCollection<IPCBComponent> ConnectedComponents
        {
            get
            {
                return connections;
            }
        }

        public static new void SetOffsets(int x, int y)
        {
            offsetX = x;
            offsetY = y;
        }

        public void CheckForTestCoverage()
        {
            if (ConnectedComponents.Count == 2)
            {
                if (testCoverageDeterminer.IsTestResultAvailableForComponentsAndType(
                    ConnectedComponents[0], ConnectedComponents[1]))
                {
                    LineColor = Brushes.Green;
                }
                else
                {
                    LineColor = Brushes.Black;
                }
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (IPCBComponent comp in e.NewItems)
            {
                ObjectNames += "\r\n" + comp.FunctionalAttributes.Ref;
            }
        }

        private void HideLine()
        {
            LineVisibility = lineVisibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
        }
    }
}