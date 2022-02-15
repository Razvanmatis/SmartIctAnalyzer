using ProMik.SmartIct.Console.Contracts.Container;
using ProMik.SmartIct.Console.ProjectFileHandler.Interfaces;
using ProMik.SmartIct.Interfaces.Container;
using ProMik.SmartIct.Svf.SvfInterfaces.Container;
using ProMik.Svf.Contracts;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TestConsoleApplication
{
    public class ProjectFileHandlerTests
    {
        [Fact]
        public async Task TestUpdateSvfFiles()
        {
            IProjectFileHandler projectFileHandler = new ProMik.SmartIct.Console.ProjectFileHandler.Implementations.ProjectFileHandler();
            var result = await projectFileHandler.UpdateSvfFiles(
                @"C:\Repositories\smart_ict_analyser\Testdaten\Project files\new_created_project.svfproj",
                "S32P",
                new SvfPath()
                {
                    PullDowns = @"C:\Repositories\smart_ict_analyser\Testdaten\SVF\svfGeneration\ADCAM\from 30.11.21_readout\U10_Eyeq\pullDowns",
                    PullUps = @"C:\Repositories\smart_ict_analyser\Testdaten\SVF\svfGeneration\ADCAM\from 30.11.21_readout\U10_Eyeq\pullUps",
                    UnknownControls = @"C:\Repositories\smart_ict_analyser\Testdaten\SVF\svfGeneration\ADCAM\from 30.11.21_readout\U10_Eyeq\unknownsControl",
                    UnknownInputs = @"C:\Repositories\smart_ict_analyser\Testdaten\SVF\svfGeneration\ADCAM\from 30.11.21_readout\U10_Eyeq\unknownsInput",
                }).ConfigureAwait(true);
        }

        [Fact]
        public async Task TestCreateProject()
        {
            IProjectFileHandler projectFileHandler = new ProMik.SmartIct.Console.ProjectFileHandler.Implementations.ProjectFileHandler();
            string odbProjectPath = @"C:\Repositories\smart_ict_analyser\Testdaten\ADC418\odb\alt_vom_08.07.21";
            byte[] jsonProject = File.ReadAllBytes(@"C:\Repositories\smart_ict_analyser\Testdaten\JSON Exports\ADC418 new_25.11.21_DataWithValues.json");
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
            string bomFilePath = @"C:\Repositories\smart_ict_analyser\Testdaten\ADC418\BOM\BOM_PES_Var_ADC418AI12_DDR8Gb_C1_new_0_4.CSV";
            string bsdlFile = @"C:\Repositories\smart_ict_analyser\Testdaten\ADC418\BSDL\S32G274A_SBGA_1830101D_v2p0.bsdl";
            BomSettings bomSettings = new BomSettings(";", "0", "4");
            List<SvfPathWrappedBsdlContainer> bsdl = new List<SvfPathWrappedBsdlContainer>()
            {
                new SvfPathWrappedBsdlContainer(new BsdlContainer()
                {
                    IdCodeInstr = 1122,
                    InstructionLength = 80,
                    JtagIdCode = 234234,
                    JTAGName = "P32G",
                    PreloadInstr = 3434,
                    ScanChainLength = 6566,
                    ProgrammerSettings = new ProgrammerSettings()
                    {
                        CableCompensation = 0,
                        Frequency = 1000,
                        Id = 0,
                        IoVoltage = 3000,
                        Port = 5005,
                        Slot = 0,
                        SupplyVoltage = 5000,
                        Target = 0,
                    }
                },
                bsdlFile,
                new SvfPath()
                {
                    PullUps = @"C:\Repositories\smart_ict_analyser\Testdaten\SVF\svfGeneration\ADC418\S32G275A\pullUps",
                    PullDowns = @"C:\Repositories\smart_ict_analyser\Testdaten\SVF\svfGeneration\ADC418\S32G275A\pullDowns",
                    PullUpsAndDowns = @"C:\Repositories\smart_ict_analyser\Testdaten\SVF\svfGeneration\ADC418\S32G275A\pullUpsAndDowns",
                    UnknownControls = @"C:\Repositories\smart_ict_analyser\Testdaten\SVF\svfGeneration\ADC418\S32G275A\unknownsControl",
                    UnknownInputs = @"C:\Repositories\smart_ict_analyser\Testdaten\SVF\svfGeneration\ADC418\S32G275A\unknownsInput",
                }),
            };
            string destinationFileName = @"C:\Repositories\smart_ict_analyser\Testdaten\Project files\new_created_project.svfproj";
            var resultCreate = await projectFileHandler.CreateProjectFile(odbProjectPath, jsonProject, settingsContent, bomFilePath, bomSettings, bsdl, destinationFileName).ConfigureAwait(true);
            var result = await projectFileHandler.GetProjectFileContent(@"C:\Repositories\smart_ict_analyser\Testdaten\Project files\TEST.svfproj").ConfigureAwait(true);
        }
    }
}