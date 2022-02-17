using ProMik.Core.Interfaces.Results;
using ProMik.SmartIct.Console.Contracts.Container;
using ProMik.SmartIct.Interfaces.Container;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Svf.SvfInterfaces.Container;
using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ProMik.SmartIct.Console.SvfFileCreator.Interfaces
{
    public interface ISvfFileCreator
    {
        Task<Result<SvfHandleResult>> GetSvfFilesForJtagDevice(
            JtagConnectionInfo jtag,
            Stream bsdlContentStream,
            byte[] defaultVector,
            uint idCode,
            Action<string, LogCategory> loggerAction,
            string jtagAliasName = "");

        Task<Result<(byte[] defaultVector, uint idCode)>> GetDefaultVectorOfDevice(
            WrappedProgrammerSettings programmer,
            Stream bsdlContentStream,
            Action<string, LogCategory> loggerAction,
            bool allowCalculationOfVector = true,
            bool justUseCalculatedVector = false);

        Task<Result> SaveSvfFilesToPath(List<ISvfData> svfFiles, string destinationPath);
    }
}
