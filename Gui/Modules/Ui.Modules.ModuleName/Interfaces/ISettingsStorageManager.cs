namespace Ui.Modules.ModuleName.Interfaces
{
    public interface ISettingsStorageManager
    {
        void SaveStorageContent(ISettingsStorageModel content);

        ISettingsStorageModel GetStorageContent();

        byte[] IsValidSettingsFile(string fileName);

        void ImportStorageContent(byte[] data);

        byte[] GetSettingsContent();
    }
}
