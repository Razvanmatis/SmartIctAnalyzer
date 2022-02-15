using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Prism.Commands;

namespace Ui.Modules.ModuleName.Helper
{
    public static class ScrollViewerAttached
    {
        public static readonly DependencyProperty WindowProperty =
            DependencyProperty.RegisterAttached(
                "ScrollViewerProperty",
                typeof(ICommand),
                typeof(ScrollViewerAttached),
                new PropertyMetadata(null, OnChangeCallback));

        public static void SetScrollViewerProperty(DependencyObject element, ICommand value)
        {
            element.SetValue(WindowProperty, value);
        }

        public static ICommand GetScrollViewerProperty(DependencyObject element)
        {
            return (ICommand)element.GetValue(WindowProperty);
        }

        private static void OnChangeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scroller && e.NewValue is DelegateCommand<ScrollViewer> command)
            {
                command.Execute(scroller);
            }
        }
    }
}
