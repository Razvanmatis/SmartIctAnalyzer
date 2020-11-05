using System;
using System.Collections.Generic;
using System.Text;

namespace PCBInvestAPI.DataModel.OutLineObjects
{
    public abstract class BaseOutlineObject
    {
        private BaseOutlineObject _baseOutline;

        public BaseOutlineObject()
        {

        }

        public BaseOutlineObject Clone()
        {
            return _baseOutline;
        }
    }
}
