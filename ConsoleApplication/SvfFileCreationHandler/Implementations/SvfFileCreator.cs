using ProMik.BSDL;
using ProMik.BSDL.Interfaces;
using ProMik.BSDLPackageAnalyzer;
using ProMik.Core.Interfaces.Bsdl;
using ProMik.Core.Interfaces.Results;
using ProMik.SmartIct.Console.Contracts.Container;
using ProMik.SmartIct.Console.Contracts.Implementations;
using ProMik.SmartIct.Console.SvfFileCreator.Interfaces;
using ProMik.SmartIct.Interfaces.Container;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Svf.SvfFileCreation.Implementations;
using ProMik.SmartIct.Svf.SvfFileCreation.Interfaces;
using ProMik.SmartIct.Svf.SvfInterfaces.Container;
using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;
using ProMik.Svf.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProMik.SmartIct.Console.SvfFileCreator.Implementations
{
    public class SvfFileCreator : ISvfFileCreator
    {
        private const bool CalculationOfDefaultVectorByBsdlFileIsAllowed = true;
        private const string Ok = "OK";
        private readonly ISvfDataCreator svfDataCreator;
        private readonly ILogger logger = new ConsoleLogger(false);
        private readonly IBSDLProcessor bsdlProcessor = new BSDLProcessor();
        private readonly IPackageAnalyzer packageAnalyzer = new PackageAnalyzer();
        private readonly ISvfPlayerHandler svfPlayer;
        public SvfFileCreator()
        {
            svfDataCreator = new SvfDataCreator(logger);
            svfPlayer = new SvfPlayerHandler(logger, null);
        }

        public async Task<Result<(byte[] defaultVector, uint idCode)>> GetDefaultVectorOfDevice(
            WrappedProgrammerSettings programmer,
            Stream bsdlContentStream,
            Action<string, LogCategory> loggerAction)
        {
            Result<IBSDLOutput> bsdlResult = await GetBsdlResult(bsdlContentStream).ConfigureAwait(true);
            if (!bsdlResult.Success)
            {
                return new Result<(byte[] defaulVector, uint idCode)>(bsdlResult.Message, false, (null, 0));
            }

            var bsdlContainer = await GetBsdlContainer(bsdlResult.Data).ConfigureAwait(true);
            if (!bsdlContainer.Success)
            {
                return new Result<(byte[] defaulVector, uint idCode)>(bsdlContainer.Message, false, (null, 0));
            }

            string message = Ok;
            bool success = true;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((msg, category) =>
            {
                success = false;
                message = msg;
                loggerAction?.Invoke(msg, category);
            });

            byte[] defaultVector = svfPlayer.GetDefaultVector(
                    bsdlContainer.Data.container.ScanChainLength,
                    bsdlContainer.Data.container.InstructionLength,
                    programmer.Ip,
                    programmer.Port,
                    programmer.SupplyVoltage,
                    programmer.IoVoltage,
                    out uint idCode,
                    bsdlContainer.Data.container.JtagIdCode,
                    programmer.Frequency,
                    programmer.CableCompensation,
                    bsdlContainer.Data.skipvalue,
                    programmer.Target,
                    programmer.Slot,
                    bsdlContainer.Data.container.IdCodeInstr,
                    bsdlContainer.Data.container.PreloadInstr);

            if (defaultVector == null || defaultVector.Length == 0)
            {
                if (!CalculationOfDefaultVectorByBsdlFileIsAllowed)
                {
                    return new Result<(byte[] defaulVector, uint idCode)>(
                        "Were not able to read out the default vector of the device. Further the calculation of it is not allowed!",
                        false,
                        (null, 0));
                }

                byte[] defVectorCreated = bsdlResult.Data.GetDefaultVector();
                loggerAction?.Invoke("Were not able to read out the default vector of the device. Used calculated one!", LogCategory.WARNING);
                message = "Were not able to read out the default vector of the device. Used calculated one!";
                success = true;
                defaultVector = defVectorCreated;
            }

            return new Result<(byte[] defaulVector, uint idCode)>(message, success, (defaultVector, idCode));
        }

        public async Task<Result<SvfHandleResult>> GetSvfFilesForJtagDevice(
            JtagConnectionInfo jtag,
            Stream bsdlContentStream,
            byte[] defaultVector,
            uint idCode,
            Action<string, LogCategory> loggerAction,
            string jtagAliasName = "")
        {
            if (!string.IsNullOrEmpty(jtagAliasName))
            {
                jtag.ComponentRealName = jtag.ComponentName;
                jtag.ComponentName = jtagAliasName;
            }

            ((ConsoleLogger)logger).SetLoggerCallbackForError(loggerAction);
            List<ISvfData> resultList = new List<ISvfData>();
            Result<IBSDLOutput> bsdlResult = await GetBsdlResult(bsdlContentStream).ConfigureAwait(true);
            if (!bsdlResult.Success)
            {
                return new Result<SvfHandleResult>(bsdlResult.Message, false, null);
            }

            if (!svfDataCreator.CheckPinConsistency(jtag, bsdlResult.Data))
            {
                loggerAction?.Invoke("PIN configuration does not match the BSDL file!", LogCategory.WARNING);
            }

            var bsdlContainer = await GetBsdlContainer(bsdlResult.Data);
            if (!bsdlContainer.Success)
            {
                return new Result<SvfHandleResult>(bsdlContainer.Message, false, null);
            }

            bsdlContainer.Data.container.JTAGName = jtag.ComponentName;
            svfDataCreator.PerformChecks(
                idCode,
                bsdlResult.Data,
                bsdlContainer.Data.container.JtagIdCode,
                bsdlContainer.Data.amountToSkip,
                bsdlContainer.Data.container.ScanChainLength,
                defaultVector);
            Result<IBSDLPackage> pinPackage = await GetPinPackage(bsdlResult.Data).ConfigureAwait(true);
            var svfCreationResult = svfDataCreator.GetAllSvfDataFiles(jtag, pinPackage.Data, bsdlResult.Data, defaultVector, "");
            return new Result<SvfHandleResult>(Ok, true, new SvfHandleResult(
                svfCreationResult.SvfFiles,
                svfCreationResult.AllPinInformation,
                bsdlContainer.Data.container));
        }

        public async Task<Result> SaveSvfFilesToPath(List<ISvfData> svfFiles, string destinationPath)
        {
            try
            {
                if (!Directory.CreateDirectory(destinationPath).Exists)
                {
                    return new Result("Directory could not be created: " + destinationPath, false);
                }
            }
            catch (Exception e)
            {
                return new Result(e.Message, false);
            }

            HashSet<string> errorMessages = new HashSet<string>();
            foreach (var entry in svfFiles)
            {
                entry.BasePath = destinationPath;
                try
                {
                    string destinationEntryPath = entry.GetCompleteFilePath();
                    string directoryToCheck = Directory.GetParent(destinationEntryPath).FullName;
                    if (!Directory.CreateDirectory(directoryToCheck).Exists)
                    {
                        errorMessages.Add("Directory could not be created: " + destinationEntryPath);
                    }
                    else
                    {
                        await File.WriteAllTextAsync(destinationEntryPath, entry.GetCompleteContentAsString()).ConfigureAwait(true);
                    }
                }
                catch (Exception e)
                {
                    errorMessages.Add(e.Message);
                }
            }

            if (errorMessages.Count == 0)
            {
                return new Result(Ok, true);
            }
            else
            {
                return new Result(String.Join(", ", errorMessages.ToList()), false);
            }
        }

        private Task<Result<IBSDLPackage>> GetPinPackage(IBSDLOutput data)
        {
            var resultPins = packageAnalyzer.GeneratePackage(data);
            if (!resultPins.Success)
            {
                return Task.FromResult(new Result<IBSDLPackage>(
                    "Neighbour detection couldn't be performed due to error: " + resultPins.ErrorMessage,
                    false,
                    null));
            }
            else
            {
                return Task.FromResult(new Result<IBSDLPackage>(Ok, true, resultPins.Result));
            }
        }

        private Task<Result<(BsdlContainer container, uint skipvalue, int amountToSkip)>> GetBsdlContainer(IBSDLOutput bsdlResult)
        {
            uint boundaryScanLength = bsdlResult.GetBoundaryScanChainLength();
            if (boundaryScanLength == 0)
            {
                return Task.FromResult(new Result<(BsdlContainer, uint, int)>("No valid scan chain length of BSDL package found!", false, (null, 0, 0)));
            }

            uint idCodeRegister = bsdlResult.GetIdCodeRegister(out int amountToSkip);
            BsdlContainer bsdlparameter = new BsdlContainer()
            {
                ScanChainLength = boundaryScanLength,
                InstructionLength = bsdlResult.InstructionLength,
                JtagIdCode = idCodeRegister,
                IdCodeInstr = bsdlResult.GetIdCode(),
                PreloadInstr = bsdlResult.GetPreload(),
            };

            uint skipValue = bsdlResult.GetValueMaskForSkippingIdCodeRegister(amountToSkip);
            return Task.FromResult(new Result<(BsdlContainer, uint, int)>(Ok, true, (bsdlparameter, skipValue, amountToSkip)));
        }

        private Task<Result<IBSDLOutput>> GetBsdlResult(Stream bsdlContentStream)
        {
            var bsdlContent = bsdlProcessor.ParseBSDLFile(bsdlContentStream);
            if (!bsdlContent.Success)
            {
                return Task.FromResult(new Result<IBSDLOutput>(bsdlContent.ErrorMessage, false, null));
            }
            else
            {
                return Task.FromResult(new Result<IBSDLOutput>(Ok, true, bsdlContent.Result));
            }
        }
    }
}
