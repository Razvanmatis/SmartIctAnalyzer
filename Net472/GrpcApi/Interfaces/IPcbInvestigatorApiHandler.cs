using System;
using System.Collections.Generic;
using System.Text;

namespace GrpcApi.Interfaces
{
    public interface IPcbInvestigatorApiHandler
    {
        IGrpcResult GetAllComponents();
    }
}
