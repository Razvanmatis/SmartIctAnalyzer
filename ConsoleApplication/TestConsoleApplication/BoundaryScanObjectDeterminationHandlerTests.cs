using ProMik.SmartIct.Console.BoundaryScanObjectDetermination.Implementations;
using ProMik.SmartIct.Console.Contracts.Container;
using ProMik.SmartIct.Console.OdbProjectExtractor.Implementations;
using ProMik.SmartIct.Console.OdbProjectExtractor.Interfaces;
using ProMik.SmartIct.Interfaces.Container;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TestConsoleApplication
{
    public class BoundaryScanObjectDeterminationHandlerTests
    {
        [Fact]
        public async Task TestGetTestRelatedObjects()
        {
            IOdbProjectExtractor handler = new OdbProjectExtractor();
            string odbProjectPath = @"C:\Repositories\smart_ict_analyser\Testdaten\ADC418\odb\new_vom_25.11.21";
            var result = await handler.GetParsedObjectsFromGrpcByZipFolder(new SettingsContent()
            {
                PcbInvestigator = new PcbInvestigatorSettings()
                {
                    StepsToReadOut = "-1",
                },
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
            }, odbProjectPath).ConfigureAwait(true);

            var bomResult = await handler.UpdateBom(
               result.Data,
               @"C:\Repositories\smart_ict_analyser\Testdaten\ADC418\BOM\BOM_PES_Var_ADC418AI12_DDR8Gb_C1_new_0_4.CSV",
               new BomSettings(";", "0", "4")).ConfigureAwait(true);

            var handlerBoundary
                = new BoundaryScanObjectDetermination();
            var testResult = await handlerBoundary.GetBoundaryScanRelatedObjects(result.Data.Nets, new TestCoverageSettings()
            {
                GndBlacklist = "",
                GndIdentifier = "gnd",
                JtagBlacklist = "",
                JtagIdentifier = "tms;tdi;tdo",
                PowerBlacklist = "",
                PowerIdentifier = "4v;1v;3.3v",
            }, true).ConfigureAwait(true);
        }
    }
}
