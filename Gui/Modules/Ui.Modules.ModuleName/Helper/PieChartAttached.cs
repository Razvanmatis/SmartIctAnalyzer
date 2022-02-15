using System.Windows;
using System.Windows.Input;
using LiveCharts.Wpf;
using Prism.Commands;

namespace Ui.Modules.ModuleName.Helper
{
    public static class PieChartAttached
    {
        public static readonly DependencyProperty ChartsViewProperty =
            DependencyProperty.RegisterAttached(
                "ChartsViewProperty",
                typeof(ICommand),
                typeof(PieChartAttached),
                new PropertyMetadata(null, OnChangeCallback));

        public static void SetChartsViewProperty(DependencyObject element, ICommand value)
        {
            element.SetValue(ChartsViewProperty, value);
        }

        public static ICommand GetChartsViewProperty(DependencyObject element)
        {
            return (ICommand)element.GetValue(ChartsViewProperty);
        }

        private static void OnChangeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PieChart pie && e.NewValue is DelegateCommand<PieChart> command)
            {
                command.Execute(pie);
            }
        }
    }
}
