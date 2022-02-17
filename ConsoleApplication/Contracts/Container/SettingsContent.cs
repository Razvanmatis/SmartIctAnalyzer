using ProMik.SmartIct.Svf.SvfInterfaces.Container;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProMik.SmartIct.Console.Contracts.Container
{
    public class SettingsContent
    {
        public GeneralSettings General { get; set; } = new GeneralSettings();

        public PcbInvestigatorSettings PcbInvestigator { get; set; } = new PcbInvestigatorSettings();

        public GrpcServerSettings GrpcServer { get; set; } = new GrpcServerSettings();

        public TestCoverageSettings TestCoverage { get; set; } = new TestCoverageSettings();

        public WrappedProgrammerSettings Programmer { get; set; } = new WrappedProgrammerSettings();
    }
}
