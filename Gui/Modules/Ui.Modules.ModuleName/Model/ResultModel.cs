using System.Linq;
using ProMik.SmartIct.Interfaces.GrpcClientParser;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.Model
{
    public class ResultModel : IResultModel
    {
        public IParsedResult Result { get; set; }

        public bool CheckIfValuesAreBeingUsed()
        {
            return Result.Components.FirstOrDefault(comp => !string.IsNullOrEmpty(comp.FunctionalAttributes.Value)) != null;
        }
    }
}
