using Ui.Core.Mvvm;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class ViewModelPCBBase : ViewModelBase
    {
        public const double OPACITYMAX = 1;
        public const int OFFSET = 15;
        public const int ZINDEXMIN = 2;
        public const int ZINDEXDEF = 1000;
        private static int offsetX;
        private static int offsetY;
        private double opacity = OPACITYMAX;
        private double posXToUse;
        private double posYToUse;
        private int zindex = ZINDEXDEF;

        public ViewModelPCBBase(double posX, double posY)
        {
            posXToUse = posX;
            posYToUse = posY;
        }

        public static int OffsetX
        {
            get
            {
                return offsetX;
            }
        }

        public static int OffsetY
        {
            get
            {
                return offsetY;
            }
        }

        public virtual int ZIndex
        {
            get
            {
                return zindex;
            }

            set
            {
                SetProperty(ref zindex, value);
            }
        }

        public virtual double PosXWithOffset
        {
            get
            {
                return posXToUse + offsetX + OFFSET;
            }
        }

        public virtual double PosYWithOffset
        {
            get
            {
                return posYToUse + offsetY + OFFSET;
            }
        }

        public double Opacity
        {
            get
            {
                return opacity;
            }

            set
            {
                SetProperty(ref opacity, value);
            }
        }

        public int PosXToUse
        {
            get
            {
                return (int)posXToUse;
            }

            set
            {
                SetProperty(ref posXToUse, (double)value);
            }
        }

        public int PosYToUse
        {
            get
            {
                return (int)posYToUse;
            }

            set
            {
                SetProperty(ref posYToUse, (double)value);
            }
        }

        public static void SetOffsets(int x, int y)
        {
            offsetX = x;
            offsetY = y;
        }
    }
}
