using ProMik.SmartIct.Console.BoundaryScanObjectDetermination.Implementations;
using ProMik.SmartIct.Console.BoundaryScanObjectDetermination.Interfaces;
using ProMik.SmartIct.Console.BoundaryScanReportCreator.Implementations;
using ProMik.SmartIct.Console.BoundaryScanReportCreator.Interfaces;
using ProMik.SmartIct.Console.Contracts.Container;
using ProMik.SmartIct.Console.JtagPinInformationExtractor.Implementations;
using ProMik.SmartIct.Console.JtagPinInformationExtractor.Interfaces;
using ProMik.SmartIct.Console.OdbProjectExtractor.Implementations;
using ProMik.SmartIct.Console.OdbProjectExtractor.Interfaces;
using ProMik.SmartIct.Console.ProjectFileHandler.Interfaces;
using ProMik.SmartIct.Console.SvfFileCreator.Implementations;
using ProMik.SmartIct.Console.SvfFileCreator.Interfaces;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TestConsoleApplication
{
    public class BoundaryScanReportHandlerTest
    {
        [Fact]
        public async Task TestCreateReport()
        {
            IProjectFileHandler projectFileHandler = new ProMik.SmartIct.Console.ProjectFileHandler.Implementations.ProjectFileHandler();
            var projectReadResult = await projectFileHandler.GetProjectFileContent(
                @"C:\Repositories\smart_ict_analyser\Testdaten\Project files\TEST3.svfproj").ConfigureAwait(true);
            if (!projectReadResult.Success)
            {
                return;
            }

            IOdbProjectExtractor projectLoadHandler = new OdbProjectExtractor();
            var projectLoadResult = await projectLoadHandler.GetParsedObjectsFromJsonFile(
                projectReadResult.Data.SettingsContent.General, Encoding.ASCII.GetString(projectReadResult.Data.JsonProjectFile)).ConfigureAwait(true);

            if (!projectLoadResult.Success)
            {
                return;
            }

            //var bomResult = await projectLoadHandler.UpdateBom(
            // projectLoadResult.Data,
            // projectReadResult.Data.BomFile,
            // projectReadResult.Data.BomSettingsContent).ConfigureAwait(true);

            TestCoverageSettings testCoverageSettings = projectReadResult.Data.SettingsContent.TestCoverage;
            IBoundaryScanObjectDetermination boundaryScanDeterminationHandler = new BoundaryScanObjectDetermination();
            var boundaryScanDeterminationResult = await boundaryScanDeterminationHandler.GetBoundaryScanRelatedObjects(
                projectLoadResult.Data.Nets,
                testCoverageSettings,
                true).ConfigureAwait(true);

            IJtagPinInformationExtractor pinInformationHandler = new JtagPinInformationExtractor();
            var pinInfosResult = await pinInformationHandler.GetAllPinInformation(
                boundaryScanDeterminationResult,
                testCoverageSettings,
                projectLoadResult.Data.Components.ToList(),
                (message, category) => Debug.WriteLine(message)).ConfigureAwait(true);

            ISvfFileCreator svfHandler = new SvfFileCreator();
            //Stream bsdlStream = projectReadResult.Data.BsdlContent[projectReadResult.Data.Manifest.BSDL[0].JTAGName];
            string bsdlFile = @"C:\Repositories\smart_ict_analyser\Testdaten\ADC418\BSDL\S32G274A_SBGA_1830101D_v2p0.bsdl";
            Stream bsdlStream = File.OpenRead(bsdlFile);
            byte[] result = new byte[150000];
            int size = bsdlStream.Read(result);
            MemoryStream memStream = new MemoryStream();
            memStream.Write(result, 0, size);
            memStream.Seek(0, SeekOrigin.Begin);
            memStream.Position = 0;
            var defVectorResult = await svfHandler.GetDefaultVectorOfDevice(
                projectReadResult.Data.SettingsContent.Programmer,
                memStream,
                (msg, cat) =>
                {
                    Debug.WriteLine(msg);
                });

            if (!defVectorResult.Success)
            {
                return;
            }

            memStream = new MemoryStream();
            memStream.Write(result, 0, size);
            memStream.Seek(0, SeekOrigin.Begin);
            memStream.Position = 0;
            var svfCreationResult = await svfHandler.GetSvfFilesForJtagDevice(
                pinInfosResult[0],
                memStream,
                defVectorResult.Data.defaultVector,
                defVectorResult.Data.idCode,
                (msg, cat) =>
                {
                    Debug.WriteLine(msg);
                },
                "S32P");

            if (!svfCreationResult.Success)
            {
                return;
            }

            svfCreationResult.Data.BsdlParameters.ProgrammerSettings = projectReadResult.Data.SettingsContent.Programmer;

            IBoundaryScanReportCreator reportHandler = new BoundaryScanReportCreator();
            var reportResult = await reportHandler.CreateCsvReport(
                pinInfosResult[0],
                svfCreationResult.Data.SvfFiles,
                svfCreationResult.Data.AllPinInformation,
                "P32G",
                @"C:\Repositories\smart_ict_analyser\Testdaten\newCsv.csv",
                (msg, cat) => Debug.WriteLine(msg)).ConfigureAwait(true);
        }
    }
}
