using System;
using System.Collections.Generic;
using System.Text;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class CoordinatesOriginViewModel : ViewModelPCBBase
    {
        private const double OFFSETX = 3d;
        private const double OFFSETY = -3.5d;
        private const int ZINDEXDEF = 1100;

        public CoordinatesOriginViewModel(double posX, double posY)
            : base(posX, posY)
        {
        }

        public override double PosXWithOffset
        {
            get
            {
                return ViewModelPCBBase.OffsetX + OFFSETX;
            }
        }

        public override double PosYWithOffset
        {
            get
            {
                return ViewModelPCBBase.OffsetY + OFFSETY;
            }
        }

        public override int ZIndex
        {
            get
            {
                return ZINDEXDEF;
            }
        }
    }
}
