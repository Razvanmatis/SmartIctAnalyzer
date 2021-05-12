namespace SVFHelper.Interfaces
{
    public interface ISvfPlayer
    {
        byte[] GetDefaultVector(uint scanChainLength, uint instructionRegisterLength, string pgmIp, uint pgmPort, uint supplyVoltageMv, uint ioVoltageMv);

        bool PlaySvfFile(uint scanChainLength, uint instructionRegisterLength, string[] svfFilePath, string logFilePath, string pgmIp, uint pgmPort, uint supplyVoltageMv, uint ioVoltageMv);
    }
}
