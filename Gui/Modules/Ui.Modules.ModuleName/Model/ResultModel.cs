using GrpcClientParser.Interfaces;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.Model
{
    public class ResultModel : IResultModel
    {
        public IParsedResult Result { get; set; }

        public bool CheckIfValuesAreBeingUsed()
        {
            foreach (var comp in Result.Components)
            {
                if (!string.IsNullOrEmpty(comp.FunctionalAttributes.Value))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
