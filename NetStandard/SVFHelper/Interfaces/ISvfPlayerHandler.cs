using ProMik.Core.Interfaces.Bsdl;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Interfaces
{
    public interface ISvfPlayerHandler
    {
        byte[] GetDefaultVector(
            uint scanChainLength,
            uint instructionRegisterLength,
            string pgmIp,
            uint pgmPort,
            uint supplyVoltageMv,
            uint ioVoltageMv,
            out uint idCodeReadOut,
            uint idCodeRegister,
            uint frequency,
            uint cableCompensation,
            uint skipValue,
            int channel,
            int slot,
            uint idCodeInstr,
            uint preloadInstr);

        bool PlaySvfFile(
            IBSDLOutput package,
            uint scanChainLength,
            uint instructionRegisterLength,
            string[] svfFilePath,
            string logFilePath,
            string pgmIp,
            uint pgmPort,
            uint supplyVoltageMv,
            uint ioVoltageMv,
            uint idCodeRegister,
            uint frequency,
            uint cableCompensation,
            uint valueForMaskToSkip,
            int channel,
            int slot,
            uint idCodeInstr,
            uint preloadInstr);
    }
}
