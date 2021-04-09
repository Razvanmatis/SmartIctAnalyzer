using GrpcClientParser.Interfaces;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface IResultModel
    {
        IParsedResult Result { get; set; }

        bool CheckIfValuesAreBeingUsed();
    }
}
