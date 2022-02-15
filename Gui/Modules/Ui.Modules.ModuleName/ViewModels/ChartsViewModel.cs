using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.Interfaces.TestCoverage;
using ProMik.SmartIct.TestCoverageDeterminer.Interfaces;
using Ui.Modules.ModuleName.Helper;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class ChartsViewModel : BindableBase
    {
        private const string ALL = "For all";
        private readonly ITestCoverageDeterminer testCoverageDeterminer;
        private readonly ITestCoverageDataModel testCoverageDataModel;
        private readonly IResultModel result;
        private readonly ContentToImageExporter contentToImageExporter;
        private Func<ChartPoint, string> pointLabelPullUp;
        private Func<ChartPoint, string> pointLabelPullDown;
        private Func<ChartPoint, string> pointLabelOthers;
        private Func<ChartPoint, string> pointLabelNone;
        private float coverage;
        private string[] items;
        private string selectedItem;
        private ChartValues<ObservableValue> pullUpValues;
        private ChartValues<ObservableValue> pullDownValues;
        private ChartValues<ObservableValue> otherValues;
        private ChartValues<ObservableValue> noneValues;

        public ChartsViewModel(
            ITestCoverageDeterminer testCoverageDeterminer,
            ITestCoverageDataModel testCoverageDataModel,
            IResultModel result,
            IDialogSelector dialogSelector,
            ILogger logger)
        {
            this.testCoverageDeterminer = testCoverageDeterminer;
            this.testCoverageDataModel = testCoverageDataModel;
            this.result = result;
            contentToImageExporter = new ContentToImageExporter(logger, dialogSelector);
            SaveCommand = new DelegateCommand(() => contentToImageExporter.TakeTheChart(Chartpoint));
            SetPieChartCommand = new DelegateCommand<PieChart>(SetPieChart);
            InitValues();
        }

        public ICommand SetPieChartCommand { get; }

        public Func<ChartPoint, string> PointLabelPullUp
        {
            get
            {
                return pointLabelPullUp;
            }

            set
            {
                SetProperty(ref pointLabelPullUp, value);
            }
        }

        public ICommand SaveCommand
        {
            get;

            private set;
        }

        public Func<ChartPoint, string> PointLabelPullDown
        {
            get
            {
                return pointLabelPullDown;
            }

            set
            {
                SetProperty(ref pointLabelPullDown, value);
            }
        }

        public Func<ChartPoint, string> PointLabelOthers
        {
            get
            {
                return pointLabelOthers;
            }

            set
            {
                SetProperty(ref pointLabelOthers, value);
            }
        }

        public Func<ChartPoint, string> PointLabelNone
        {
            get
            {
                return pointLabelNone;
            }

            set
            {
                SetProperty(ref pointLabelNone, value);
            }
        }

        public ChartValues<ObservableValue> PullUpValues
        {
            get
            {
                return pullUpValues;
            }

            set
            {
                SetProperty(ref pullUpValues, value);
            }
        }

        public ChartValues<ObservableValue> PullDownValues
        {
            get
            {
                return pullDownValues;
            }

            set
            {
                SetProperty(ref pullDownValues, value);
            }
        }

        public ChartValues<ObservableValue> OtherValues
        {
            get
            {
                return otherValues;
            }

            set
            {
                SetProperty(ref otherValues, value);
            }
        }

        public ChartValues<ObservableValue> NoneValues
        {
            get
            {
                return noneValues;
            }

            set
            {
                SetProperty(ref noneValues, value);
            }
        }

        public string[] Items
        {
            get
            {
                return items;
            }

            set
            {
                SetProperty(ref items, value);
            }
        }

        public string SelectedItem
        {
            get
            {
                return selectedItem;
            }

            set
            {
                SetProperty(ref selectedItem, value);
                InitValuesForRepresentation();
            }
        }

        public float Coverage
        {
            get
            {
                return coverage;
            }

            set
            {
                SetProperty(ref coverage, value);
            }
        }

        private PieChart Chartpoint
        {
            get;

            set;
        }

        public static void HandleOnDataClick(ChartPoint obj)
        {
            var chart = (PieChart)obj.ChartView;

            // clear selected slice.
            foreach (PieSeries series in chart.Series)
            {
                series.PushOut = 0;
            }

            var selectedSeries = (PieSeries)obj.SeriesView;
            selectedSeries.PushOut = 8;
        }

        public void InitValues()
        {
            Items = GetItems();
            SelectedItem = Items[0];
        }

        private static Func<ChartPoint, string> GetLabelFunc(int value)
        {
            return chartPoint =>
                string.Format(CultureInfo.CurrentCulture, "{0} ({1:P})", value, chartPoint.Participation);
        }

        private void SetPieChart(PieChart obj)
        {
            Chartpoint = obj;
        }

        private void InitValuesForRepresentation()
        {
            PullUpValues = new ChartValues<ObservableValue>();
            PullDownValues = new ChartValues<ObservableValue>();
            OtherValues = new ChartValues<ObservableValue>();
            NoneValues = new ChartValues<ObservableValue>();

            List<IPCBComponent> comps = GetComponentsToConsider();
            float ups = Task.Run(async () => await testCoverageDeterminer.GetTestCoverageValue(
                TestCoverageType.BOUNDARY_SCAN_PULL_UPS, comps).ConfigureAwait(true)).Result;
            PullUpValues.Add(new ObservableValue(ups));
            float downs = Task.Run(async () => await testCoverageDeterminer.GetTestCoverageValue(
                TestCoverageType.BOUNDARY_SCAN_PULL_DOWNS, comps).ConfigureAwait(true)).Result;
            PullDownValues.Add(new ObservableValue(downs));
            float others = Task.Run(async () => await testCoverageDeterminer.GetTestCoverageValue(
                TestCoverageType.BOUNDARY_SCAN_OTHERS, comps).ConfigureAwait(true)).Result;
            OtherValues.Add(new ObservableValue(others));
            float none = 100 - (others + downs + ups);
            if (none < 0)
            {
                none = 0;
            }

            NoneValues.Add(new ObservableValue(none));
            int amountPullUps = testCoverageDeterminer.GetAmountOfTotalDistinctNets(
                TestCoverageType.BOUNDARY_SCAN_PULL_UPS, true, comps);
            int amountPullDowns = testCoverageDeterminer.GetAmountOfTotalDistinctNets(
                TestCoverageType.BOUNDARY_SCAN_PULL_DOWNS, true, comps);
            int amountOthers = testCoverageDeterminer.GetAmountOfTotalDistinctNets(
                TestCoverageType.BOUNDARY_SCAN_OTHERS, true, comps);
            int amountNone = 0;
            if (amountPullUps > 0)
            {
                amountNone = (int)(amountPullUps / ups * none);
            }
            else if (amountPullDowns > 0)
            {
                amountNone = (int)(amountPullDowns / downs * none);
            }
            else if (amountOthers > 0)
            {
                amountNone = (int)(amountOthers / others * none);
            }

            PointLabelPullUp = GetLabelFunc(amountPullUps);
            PointLabelPullDown = GetLabelFunc(amountPullDowns);
            PointLabelOthers = GetLabelFunc(amountOthers);
            PointLabelNone = GetLabelFunc(amountNone);
            Coverage = GetCoverage(amountPullUps + amountPullDowns + amountOthers);

            // clear selected slice.
            if (Chartpoint != null)
            {
                foreach (PieSeries series in Chartpoint.Series)
                {
                    series.PushOut = 0;
                }
            }
        }

        private List<IPCBComponent> GetComponentsToConsider()
        {
            return new List<IPCBComponent>(testCoverageDataModel.Jtags.Where(comp =>
                SelectedItem.Equals(ALL) || SelectedItem.Equals(comp.FunctionalAttributes.Ref)));
        }

        private string[] GetItems()
        {
            List<string> items = new List<string>
            {
                ALL,
            };

            items.AddRange(testCoverageDataModel.Jtags.Select(comp => comp.FunctionalAttributes.Ref));
            return items.ToArray();
        }

        private float GetCoverage(int baseValue)
        {
            HashSet<INetComponent> netsToConsider = new HashSet<INetComponent>();
            foreach (var comp in result.Result.Components)
            {
                foreach (var pin in comp.Connections)
                {
                    foreach (var net in pin.Nets)
                    {
                        netsToConsider.Add(net);
                    }
                }
            }

            return (float)baseValue / netsToConsider.Count * 100;
        }
    }
}
