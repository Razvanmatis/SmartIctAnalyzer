using System;
using System.Globalization;
using System.Windows.Input;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using ProMik.SmartIct.Interfaces.Gui;
using Ui.Modules.ModuleName.Helper;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class ChartsForExportViewModel : BindableBase
    {
        public const int SIZE = 800;
        private readonly ContentToImageExporter contentToImageExporter;
        private Func<ChartPoint, string> pointLabelPullUp;
        private Func<ChartPoint, string> pointLabelPullDown;
        private Func<ChartPoint, string> pointLabelOthers;
        private Func<ChartPoint, string> pointLabelNone;
        private ChartValues<ObservableValue> pullUpValues;
        private ChartValues<ObservableValue> pullDownValues;
        private ChartValues<ObservableValue> otherValues;
        private ChartValues<ObservableValue> noneValues;
        private string pullUpsTitle;
        private string pullDownsTitle;
        private string othersTitle;
        private string notCoveredNetsTitle;

        public ChartsForExportViewModel(
            IDialogSelector dialogSelector,
            ILogger logger)
        {
            contentToImageExporter = new ContentToImageExporter(logger, dialogSelector);
            SetPieChartCommand = new DelegateCommand<PieChart>(SetThePieChart);
            PullUpsTitle = "Pull ups";
            PullDownsTitle = "Pull downs";
            OthersTitle = "Others";
            NotCoveredNetsTitle = "Not covered nets";
            InitContent();
        }

        public static int Size
        {
            get
            {
                return SIZE;
            }
        }

        public ICommand SetPieChartCommand
        {
            get;
        }

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

        public string PullUpsTitle
        {
            get
            {
                return pullUpsTitle;
            }

            set
            {
                SetProperty(ref pullUpsTitle, value);
            }
        }

        public string PullDownsTitle
        {
            get
            {
                return pullDownsTitle;
            }

            set
            {
                SetProperty(ref pullDownsTitle, value);
            }
        }

        public string OthersTitle
        {
            get
            {
                return othersTitle;
            }

            set
            {
                SetProperty(ref othersTitle, value);
            }
        }

        public string NotCoveredNetsTitle
        {
            get
            {
                return notCoveredNetsTitle;
            }

            set
            {
                SetProperty(ref notCoveredNetsTitle, value);
            }
        }

        public float PullUpsPercentage
        {
            get;
            set;
        }

        public float PullDownsPercentage
        {
            get;
            set;
        }

        public float OthersPercentage
        {
            get;
            set;
        }

        public float NonePercentage
        {
            get;
            set;
        }

        public int PullUpsAmount
        {
            get;
            set;
        }

        public int PullDownsAmount
        {
            get;
            set;
        }

        public int OthersAmount
        {
            get;
            set;
        }

        public int NoneAmount
        {
            get;
            set;
        }

        public string PathToSave
        {
            get;
            set;
        }

        private PieChart Chartpoint
        {
            get;

            set;
        }

        public void ExportImage()
        {
            contentToImageExporter.TakeTheChart(Chartpoint, PathToSave);
        }

        public void InitValuesForRepresentation()
        {
            PullUpValues.Add(new ObservableValue(PullUpsPercentage));
            PullDownValues.Add(new ObservableValue(PullDownsPercentage));
            OtherValues.Add(new ObservableValue(OthersPercentage));
            NoneValues.Add(new ObservableValue(NonePercentage));
            PointLabelPullUp = GetLabelFunc(PullUpsAmount);
            PointLabelPullDown = GetLabelFunc(PullDownsAmount);
            PointLabelOthers = GetLabelFunc(OthersAmount);
            PointLabelNone = GetLabelFunc(NoneAmount);
        }

        private static Func<ChartPoint, string> GetLabelFunc(int value)
        {
            return chartPoint =>
                string.Format(CultureInfo.CurrentCulture, "{0} ({1:P})", value, chartPoint.Participation);
        }

        private void SetThePieChart(PieChart obj)
        {
            Chartpoint = obj;
        }

        private void InitContent()
        {
            PullUpValues = new ChartValues<ObservableValue>();
            PullDownValues = new ChartValues<ObservableValue>();
            OtherValues = new ChartValues<ObservableValue>();
            NoneValues = new ChartValues<ObservableValue>();
            PointLabelPullUp = GetLabelFunc(0);
            PointLabelPullDown = GetLabelFunc(0);
            PointLabelOthers = GetLabelFunc(0);
            PointLabelNone = GetLabelFunc(0);
        }
    }
}
