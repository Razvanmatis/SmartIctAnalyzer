using ProMik.Core.Interfaces.Results;
using ProMik.SmartIct.Console.Contracts.Container;
using ProMik.SmartIct.Console.Contracts.Implementations;
using ProMik.SmartIct.Console.OdbProjectExtractor.Interfaces;
using ProMik.SmartIct.Interfaces.Container;
using ProMik.SmartIct.PCBComponentParser.Implementations;
using ProMik.SmartIct.Interfaces.GrpcClientParser;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.PCBComponentParser.Helper;
using ProMik.SmartIct.PCBComponentParser.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProMik.SmartIct.Console.OdbProjectExtractor.Implementations
{
    public class OdbProjectExtractor : IOdbProjectExtractor
    {
        private const string Ok = "OK";
        private readonly ILogger logger = new ConsoleLogger();
        private readonly IPCBComponentParser grpcParser;

        public OdbProjectExtractor()
        {
            grpcParser = new PCBComponentParserHandler(logger);
        }

        public async Task<Result<bool>> ChangeIpAdressOfClient(string ip)
        {
            bool success = true;
            string message = Ok;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((error, category) =>
            {
                success = false;
                message = error;
            });

            return new Result<bool>(message, success, await grpcParser.ChangeIpAdressOfClient(ip).ConfigureAwait(true));
        }

        public async Task<Result<string>> GetDataAsJsonString(IParsedResult result)
        {
            return await Task.Run<Result<string>>(() =>
            {
                bool success = true;
                string message = Ok;
                ((ConsoleLogger)logger).SetLoggerCallbackForError((error, category) =>
                {
                    success = false;
                    message = error;
                });

                return new Result<string>(message, success, grpcParser.GetDataAsString(result));
            }).ConfigureAwait(true);
        }

        public async Task<Result<IParsedResult>> GetParsedObjectsFromGrpcByZipFolder(SettingsContent settings, string pathToOdb)
        {
            var isConnected = await IsServerConnected().ConfigureAwait(true);
            if (!isConnected.Success)
            {
                return new Result<IParsedResult>(isConnected.Message, false, null);
            }

            bool success = true;
            string message = Ok;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((error, category) =>
            {
                success = false;
                message = error;
            });

            return new Result<IParsedResult>(message, success, await grpcParser.GetParsedObjectsFromGrpcByZipFolder(
                pathToOdb,
                settings.PcbInvestigator.StepsToReadOut,
                settings.General.ResistorIdentifier.Split(";").ToList(),
                settings.General.CapacitorIdentifier.Split(";").ToList(),
                settings.General.InductionIdentifer.Split(";").ToList(),
                settings.General.TestPointIdentifier.Split(";").ToList(),
                settings.General.IcIdentifier.Split(";").ToList(),
                settings.General.ConnectorIdentifier.Split(";").ToList(),
                settings.General.UseContains).ConfigureAwait(true));
        }

        public async Task<Result<IParsedResult>> GetParsedObjectsFromGrpcByZipFolder(SettingsContent settings, byte[] odbPathContentAsZip)
        {
            var isConnected = await IsServerConnected().ConfigureAwait(true);
            if (!isConnected.Success)
            {
                return new Result<IParsedResult>(isConnected.Message, false, null);
            }

            bool success = true;
            string message = Ok;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((error, category) =>
            {
                success = false;
                message = error;
            });

            return new Result<IParsedResult>(message, success, await grpcParser.GetParsedObjectsFromGrpcByZipFolder(
                odbPathContentAsZip,
                settings.PcbInvestigator.StepsToReadOut,
                settings.General.ResistorIdentifier.Split(";").ToList(),
                settings.General.CapacitorIdentifier.Split(";").ToList(),
                settings.General.InductionIdentifer.Split(";").ToList(),
                settings.General.TestPointIdentifier.Split(";").ToList(),
                settings.General.IcIdentifier.Split(";").ToList(),
                settings.General.ConnectorIdentifier.Split(";").ToList(),
                settings.General.UseContains).ConfigureAwait(true));
        }

        public async Task<Result<IParsedResult>> GetParsedObjectsFromJsonFile(GeneralSettings settings, string data)
        {
            bool success = true;
            string message = Ok;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((error, category) =>
            {
                success = false;
                message = error;
            });

            IParsedResult result = await Task.Run<IParsedResult>(() =>
            {
                return grpcParser.ImportComponentsFromFile(
                    data,
                    settings.ResistorIdentifier.Split(";").ToList(),
                    settings.CapacitorIdentifier.Split(";").ToList(),
                    settings.InductionIdentifer.Split(";").ToList(),
                    settings.TestPointIdentifier.Split(";").ToList(),
                    settings.IcIdentifier.Split(";").ToList(),
                    settings.ConnectorIdentifier.Split(";").ToList(),
                    settings.UseContains);
            }).ConfigureAwait(true);

            return new Result<IParsedResult>(message, success, result);
        }

        public async Task<Result<bool>> IsGrpcServerAvailable()
        {
            bool success = true;
            string message = Ok;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((error, category) =>
            {
                success = false;
                message = error;
            });

            return new Result<bool>(message, success, await grpcParser.IsGrpcServerAvailable().ConfigureAwait(true));
        }

        public async Task<ProMik.Core.Interfaces.Results.Result> UpdateBom(IParsedResult data, string bomFilePath, BomSettings bomSettings)
        {
            Result<string> fileContent = await GetFileContentFromFilePath(bomFilePath).ConfigureAwait(true);
            if (!fileContent.Success)
            {
                return new ProMik.Core.Interfaces.Results.Result(fileContent.Message, false);
            }

            return await HandleUpdateBom(data, fileContent.Data, bomSettings).ConfigureAwait(true);
        }

        public async Task<ProMik.Core.Interfaces.Results.Result> UpdateBom(IParsedResult data, byte[] bomFileContent, BomSettings bomSettings)
        {
            return await HandleUpdateBom(data, Encoding.ASCII.GetString(bomFileContent), bomSettings).ConfigureAwait(true);
        }

        private async Task<ProMik.Core.Interfaces.Results.Result> HandleUpdateBom(IParsedResult data, string bomContent, BomSettings bomSettings)
        {
            bool success = true;
            string message = Ok;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((error, category) =>
            {
                success = false;
                message = error;
            });

            int colRef = 0;
            if (!int.TryParse(bomSettings.ColRef, out colRef))
            {
                return new ProMik.Core.Interfaces.Results.Result("ColRef could not be parsed to int!", false);
            }

            int colValue = 0;
            if (!int.TryParse(bomSettings.ColValue, out colValue))
            {
                return new ProMik.Core.Interfaces.Results.Result("ColValue could not be parsed to int!", false);
            }

            await Task.Run(() =>
            {
                CsvReader csvReader = new CsvReader(logger);
                var values = csvReader.ReadContentFromData(bomContent, bomSettings.Separator, colRef, colValue);
                csvReader.SetValuesToObjectsFromCsv(data.Components, values);
            }).ConfigureAwait(true);

            return new ProMik.Core.Interfaces.Results.Result(message, success);
        }

        private async Task<Result<string>> GetFileContentFromFilePath(string bomFilePath)
        {
            return new Result<string>(Ok, true, await File.ReadAllTextAsync(bomFilePath, Encoding.ASCII).ConfigureAwait(true));
        }

        private async Task<ProMik.Core.Interfaces.Results.Result> IsServerConnected()
        {
            var result = await IsGrpcServerAvailable().ConfigureAwait(true);
            if (!result.Success)
            {
                return new ProMik.Core.Interfaces.Results.Result(result.Message, false);
            }

            if (!result.Data)
            {
                logger.LogMessage("GRPC server not available!", LogCategory.ERROR);
                return new ProMik.Core.Interfaces.Results.Result("GRPC server not available!", false);
            }

            return new ProMik.Core.Interfaces.Results.Result(Ok, true);
        }
    }
}
