using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Prism.Commands;

namespace Ui.Modules.ModuleName.Helper
{
    public static class WindowAttached
    {
        public static readonly DependencyProperty WindowProperty =
            DependencyProperty.RegisterAttached(
                "WindowProperty",
                typeof(ICommand),
                typeof(WindowAttached),
                new PropertyMetadata(null, OnChangeCallback));

        public static void SetWindowProperty(DependencyObject element, ICommand value)
        {
            element.SetValue(WindowProperty, value);
        }

        public static ICommand GetWindowProperty(DependencyObject element)
        {
            return (ICommand)element.GetValue(WindowProperty);
        }

        private static void OnChangeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window && e.NewValue is DelegateCommand<Window> command)
            {
                command.Execute(window);
            }
        }
    }
}
