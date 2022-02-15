using ProMik.SmartIct.Console.BoundaryScanObjectDetermination.Implementations;
using ProMik.SmartIct.Console.BoundaryScanObjectDetermination.Interfaces;
using ProMik.SmartIct.Console.Contracts.Container;
using ProMik.SmartIct.Console.JtagPinInformationExtractor.Implementations;
using ProMik.SmartIct.Console.JtagPinInformationExtractor.Interfaces;
using ProMik.SmartIct.Console.OdbProjectExtractor.Implementations;
using ProMik.SmartIct.Console.OdbProjectExtractor.Interfaces;
using ProMik.SmartIct.Console.ProjectFileHandler.Implementations;
using ProMik.SmartIct.Console.ProjectFileHandler.Interfaces;
using ProMik.SmartIct.Console.SvfFileCreator.Implementations;
using ProMik.SmartIct.Console.SvfFileCreator.Interfaces;
using ProMik.SmartIct.Interfaces.Container;
using ProMik.SmartIct.Svf.SvfInterfaces.Container;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TestConsoleApplication
{
    public class SvfFileCreationHandlerTests
    {
        [Fact]
        public async Task TestSvfFileCreation()
        {
            // create a valid settings content
            SettingsContent settingsContent = new SettingsContent()
            {
                General = new GeneralSettings()
                {
                    CapacitorIdentifier = "c",
                    ConnectorIdentifier = "x",
                    IcIdentifier = "i",
                    InductionIdentifer = "l",
                    ResistorIdentifier = "r",
                    TestPointIdentifier = "tp",
                    UseContains = true,
                },
                GrpcServer = new GrpcServerSettings()
                {
                    IpAddress = "127.0.0.1",
                },
                PcbInvestigator = new PcbInvestigatorSettings()
                {
                    StepsToReadOut = "-1",
                },
                Programmer = new WrappedProgrammerSettings()
                {
                    CableCompensation = 0,
                    Frequency = 15000,
                    Id = 0,
                    IoVoltage = 5000,
                    Ip = "192.168.1.2",
                    Port = 5005,
                    Slot = 0,
                    Target = 0,
                    SupplyVoltage = 3000,
                },
                TestCoverage = new TestCoverageSettings()
                {
                    GndBlacklist = "",
                    GndIdentifier = "gnd",
                    JtagBlacklist = "",
                    JtagIdentifier = "tms;tdi;tdo",
                    PowerBlacklist = "",
                    PowerIdentifier = "4v;1v;3.3v",
                }
            };

            // read out the ODB++ project path content
            string odbProjectPath = @"C:\Repositories\smart_ict_analyser\Testdaten\ADC418\odb\new_vom_25.11.21";
            IOdbProjectExtractor projectLoadHandler = new OdbProjectExtractor();
            var projectLoadResult = await projectLoadHandler.GetParsedObjectsFromGrpcByZipFolder(
                settingsContent, odbProjectPath).ConfigureAwait(true);
            if (!projectLoadResult.Success)
            {
                return;
            }

            // update the BOM file content
            string bomFilePath = @"C:\Repositories\smart_ict_analyser\Testdaten\ADC418\BOM\BOM_PES_Var_ADC418AI12_DDR8Gb_C1_new_0_4.CSV";
            BomSettings bomSettings = new BomSettings(";", "0", "4");
            var bomResult = await projectLoadHandler.UpdateBom(
             projectLoadResult.Data,
             bomFilePath,
             bomSettings).ConfigureAwait(true);
            if (!bomResult.Success)
            {
                Debug.WriteLine("Error in bom update!");
            }

            // read out the boundary scan related objects
            TestCoverageSettings testCoverageSettings = settingsContent.TestCoverage;
            IBoundaryScanObjectDetermination boundaryScanDeterminationHandler = new BoundaryScanObjectDetermination();
            var boundaryScanDeterminationResult = await boundaryScanDeterminationHandler.GetBoundaryScanRelatedObjects(
                projectLoadResult.Data.Nets,
                testCoverageSettings,
                false).ConfigureAwait(true);

            // create the pin JTAG information
            IJtagPinInformationExtractor pinInformationHandler = new JtagPinInformationExtractor();
            var pinInfosResult = await pinInformationHandler.GetAllPinInformation(
                boundaryScanDeterminationResult,
                testCoverageSettings,
                projectLoadResult.Data.Components.ToList(),
                (message, category) => Debug.WriteLine(message)).ConfigureAwait(true);

            ISvfFileCreator svfHandler = new SvfFileCreator();
            string bsdlFile = @"C:\Repositories\smart_ict_analyser\Testdaten\ADC418\BSDL\S32G274A_SBGA_1830101D_v2p0.bsdl";

            // read out the default vector
            var defVectorResult = await svfHandler.GetDefaultVectorOfDevice(
                settingsContent.Programmer,
                File.OpenRead(bsdlFile),
                (msg, cat) =>
                {
                    Debug.WriteLine(msg);
                });

            if (!defVectorResult.Success)
            {
                return;
            }

            // create all svf files
            var svfCreationResult = await svfHandler.GetSvfFilesForJtagDevice(
                pinInfosResult[0],
                File.OpenRead(bsdlFile),
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

            // Don't forget to place the programmer settings into the newly created BSDL container
            svfCreationResult.Data.BsdlParameters.ProgrammerSettings = settingsContent.Programmer;

            // transform the read out data into a JSON valid string data
            var projectDataTransformResult = await projectLoadHandler.GetDataAsJsonString(projectLoadResult.Data).ConfigureAwait(true);
            if (!projectDataTransformResult.Success)
            {
                return;
            }

            // write the project file
            byte[] jsonProjectContent = Encoding.ASCII.GetBytes(projectDataTransformResult.Data);
            List<SvfDataWrappedBsdlContainer> bsdl = new List<SvfDataWrappedBsdlContainer>();
            bsdl.Add(new SvfDataWrappedBsdlContainer(svfCreationResult.Data.BsdlParameters, bsdlFile, svfCreationResult.Data.SvfFiles));
            string destinationFileName = @"C:\Repositories\smart_ict_analyser\Testdaten\Project files\new_created_project.svfproj";
            IProjectFileHandler projectFileHandler = new ProjectFileHandler();
            var resultProjectCreate = await projectFileHandler.CreateProjectFile(
                odbProjectPath, jsonProjectContent, settingsContent, bomFilePath, bomSettings, bsdl, destinationFileName).ConfigureAwait(true);

        }
    }
}
