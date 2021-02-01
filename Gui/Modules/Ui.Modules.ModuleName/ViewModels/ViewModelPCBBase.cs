using System;
using System.Collections.Generic;
using System.Text;
using Ui.Core.Mvvm;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class ViewModelPCBBase : ViewModelBase
    {
        public const double OPACITYMAX = 1;
        public const int OFFSET = 15;
        private static int offsetX;
        private static int offsetY;
        private double posX;
        private double posY;
        private double opacity = OPACITYMAX;

        public ViewModelPCBBase(double posX, double posY)
        {
            this.posX = posX;
            this.posY = posY;
        }

        public double PosXWithOffset
        {
            get
            {
                return posX + offsetX + OFFSET;
            }
        }

        public double PosYWithOffset
        {
            get
            {
                return posY + offsetY + OFFSET;
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

        public static void SetOffsets(int x, int y)
        {
            offsetX = x;
            offsetY = y;
        }
    }
}
