namespace Ui.Modules.ModuleName.Helper
{
    public static class ExtensionMethods
    {
        public static System.Windows.Media.Color ConvertToMediaColor(this System.Drawing.Color color)
        {
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
