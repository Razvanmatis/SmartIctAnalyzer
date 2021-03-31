using System.Globalization;
using System.Windows.Media;

namespace Ui.Modules.ModuleName.Helper
{
    public class StorageColor
    {
        public StorageColor(int r, int g, int b)
        {
            R = r;
            G = g;
            B = b;
        }

        public int R { get; }

        public int G { get; }

        public int B { get; }

        public override string ToString()
        {
            return Color.FromRgb((byte)R, (byte)G, (byte)B).ToString(CultureInfo.CurrentCulture);
        }
    }
}
