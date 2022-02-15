using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.Helper;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.Interfaces.TestCoverage;
using ProMik.SmartIct.JtagPinInformationExtractor.Helper;
using Ui.Modules.ModuleName.Interfaces;
using Ui.Modules.ModuleName.ViewModels;
using Ui.Modules.ModuleName.Views;

namespace Ui.Modules.ModuleName.Implementations
{
    public class PdfReportHandler : IPdfReportHandler
    {
        public const int SLEEPTIMEBEFOREEXPORTIMAGE = 1000;
        private const string DefaultLatexLocation = "Latex\\";
        private const string DefaultImagesLocations = "images\\";
        private const string LatexDocName = "testCoverage.pdf";
        private const int ColumnAmountPins = 8;
        private const int ColumnAmountNets = 2;
        private const int ColumnAmountComponents = 6;
        private readonly List<ChartsForExportView> chartViews = new List<ChartsForExportView>();
        private readonly Dictionary<string, string> jtagMapping = new Dictionary<string, string>();
        private readonly ILogger logger;
        private readonly IDialogSelector dialogSelector;
        private readonly ITestCoverageDataModel testCoverageDataModel;

        public PdfReportHandler(ILogger logger, IDialogSelector dialogSelector, ITestCoverageDataModel testCoverageDataModel)
        {
            this.logger = logger;
            this.testCoverageDataModel = testCoverageDataModel;
            this.dialogSelector = dialogSelector;
        }

        public Dictionary<string, List<PinTestTypeContainer>> ReportContent { get; }
            = new Dictionary<string, List<PinTestTypeContainer>>();

        public void CreateReportFile(string destinationFileName = "")
        {
            Thread currentThread = Thread.CurrentThread;

            if (ReportContent == null || ReportContent.Count == 0)
            {
                logger.LogMessage("No content to extract for PDF. Please generate SVF files first...", LogCategory.WARNING);
                return;
            }

            if (!dialogSelector.OpenGenericDialog(
                DialogType.SAVEFILE,
                TextRessources.PdfSaveFile,
                "No valid target to save selected!",
                out string fileName,
                TextRessources.PdfFilter))
            {
                return;
            }

            if (!fileName.ToLower(CultureInfo.CurrentCulture).EndsWith(".pdf", StringComparison.InvariantCulture))
            {
                fileName += ".pdf";
            }

            logger.LogMessage("Starting the export of a PDF file...", LogCategory.INFO);
            if (CreateNewLatexFile())
            {
                new Action(async () =>
                {
                    if (await ProcessLatexPdf().ConfigureAwait(true))
                    {
                        try
                        {
                            if (File.Exists(fileName))
                            {
                                File.Delete(fileName);
                            }

                            File.Move(
                                Path.Combine(
                                Directory.GetCurrentDirectory(),
                                DefaultLatexLocation) + LatexDocName,
                                fileName);
                            logger.LogMessage("Sucessfully exported PDF report to: " + fileName, LogCategory.INFO);
                        }
                        catch (Exception e)
                        {
                            logger.LogMessage(e.Message, LogCategory.ERROR);
                        }
                    }
                    else
                    {
                        logger.LogMessage("Exporting of PDF report failed!", LogCategory.ERROR);
                    }

                    Dispatcher.FromThread(currentThread).Invoke(() =>
                    {
                        chartViews.ForEach(view => view.Close());
                        chartViews.Clear();
                    });
                })();
            }
            else
            {
                logger.LogMessage("Error in generating latex document for later compilation!", LogCategory.ERROR);
            }
        }

        public void UpdateReportContent(
            string jtagDevice,
            List<PinTestTypeContainer> pinContainer,
            string jtagDeviceRealname,
            List<PinTestTypeContainer> pinContainerNotTested)
        {
            if (!string.IsNullOrEmpty(jtagDevice))
            {
                ReportContent.Remove(jtagDevice);
                if (pinContainer != null && pinContainer.Count > 0)
                {
                    ReportContent.Add(jtagDevice, pinContainer);
                    jtagMapping.Add(jtagDevice, jtagDeviceRealname);
                }
            }
            else
            {
                ReportContent.Clear();
                jtagMapping.Clear();
            }
        }

        private static List<string> GetPinsAsStrings(List<PinTestTypeContainer> pins)
        {
            List<string> pinStrings = new List<string>(pins.Select(pin => pin.PinTestObject.PinNumber).ToHashSet());
            return pinStrings;
        }

        private static void GetTableContent(
            string tableCaption,
            List<string> values,
            ref string text,
            string componentName,
            int columnAmount)
        {
            text += "\\subsection{" + tableCaption + "}\n";
            text += " \\begin{center} \n";
            text += "  \\begin{longtable}{|";
            for (int col = 1; col <= columnAmount; col++)
            {
                text += "c|";
            }

            text += "} \n";
            text += "   \\hline \n";
            text += "    \\multicolumn{" + columnAmount + "}{|c|}{All covered " + componentName + "}\\\\ \n";
            text += "   \\hline \n";
            int index = 1;
            foreach (var value in values)
            {
                if (index > columnAmount)
                {
                    text += " \\\\ \\hline \n";
                    index = 1;
                }
                else
                {
                    if (index > 1)
                    {
                        text += " & " + value.Replace("_", "\\_");
                    }
                    else
                    {
                        text += "    " + value.Replace("_", "\\_");
                    }

                    index++;
                }
            }

            if (index < columnAmount)
            {
                for (int idx = index; idx <= columnAmount; idx++)
                {
                    text += " & ";
                }
            }

            text += " \\\\ \\hline \n";
            text += "  \\end{longtable} \n";
            text += " \\end{center}\n";
        }

        private static List<IPCBComponent> GetComponents(List<PinTestTypeContainer> pins)
        {
            HashSet<IPCBComponent> comps = new HashSet<IPCBComponent>();
            foreach (var pin in pins)
            {
                foreach (var comp in pin.PinConnectionType.ConnectedComponents)
                {
                    comps.Add(comp);
                }
            }

            return comps.ToList();
        }

        private static List<string> GetNets(List<PinTestTypeContainer> pins)
        {
            HashSet<string> nets = new HashSet<string>();
            foreach (var pin in pins)
            {
                foreach (var net in pin.PinTestObject.Nets)
                {
                    nets.Add(net);
                }
            }

            return nets.ToList();
        }

        private static int GetDistinctPinAmount(List<PinTestTypeContainer> pins)
        {
            List<string> pinNames = new List<string>();
            foreach (var pin in pins)
            {
                if (!pinNames.Contains(pin.PinTestObject.PinNumber))
                {
                    pinNames.Add(pin.PinTestObject.PinNumber);
                }
            }

            return pinNames.Count;
        }

        private static int GetAllNetsAmountForType(List<PinTestTypeContainer> pins, List<PinConnectionType> types)
        {
            HashSet<string> nets = new HashSet<string>();
            foreach (var pin in pins.Where(pi => types.Contains(pi.PinConnectionType.PinConnectionType)))
            {
                foreach (var net in pin.PinTestObject.Nets)
                {
                    nets.Add(net);
                }
            }

            return nets.Count;
        }

        private async Task<bool> ProcessLatexPdf()
        {
            Process process = null;
            bool result = true;
            int counter = 0;
            try
            {
                using CancellationTokenSource currentCancellationToken = new CancellationTokenSource();
                var workingDirectory = Path.Combine(Directory.GetCurrentDirectory(), DefaultLatexLocation);
                process = Process.Start(
                    new ProcessStartInfo("cmd.exe", "/c " + "build.bat")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        WorkingDirectory = workingDirectory,
                    });
                string output;
                while ((output = await process.StandardOutput.ReadLineAsync().ConfigureAwait(false)) != null
                    && !currentCancellationToken.IsCancellationRequested)
                {
                    if (counter > 500)
                    {
                        currentCancellationToken.Cancel();
                    }

                    counter++;
                }

                if (currentCancellationToken.IsCancellationRequested)
                {
                    logger.LogMessage("Compilation aborted!", LogCategory.ERROR);
                    result = false;
                }
                else
                {
                    result = process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                logger.LogMessage(ex.Message, LogCategory.ERROR);
            }
            finally
            {
                process?.Kill();
            }

            return result;
        }

        private bool CreateNewLatexFile()
        {
            try
            {
                string destinationFolder = Path.Combine(Directory.GetCurrentDirectory(), DefaultLatexLocation);
                File.WriteAllText(Path.Combine(destinationFolder, "TestCoverageData.tex"), string.Empty);
                File.WriteAllText(Path.Combine(destinationFolder, "TestCoverageData.tex"), GetAllContentForLatexFile());
                return true;
            }
            catch (Exception e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return false;
            }
        }

        private string GetAllContentForLatexFile()
        {
            string text = string.Empty;
            foreach (var (jtag, pins) in ReportContent)
            {
                text += "\\section{JTAG device: " + jtag + "}\n";
                List<string> nets = GetNets(pins);
                List<IPCBComponent> components = GetComponents(pins);
                int distinctPinAmount = GetDistinctPinAmount(pins);
                pins.Sort(new PinTestTypeComparer());
                nets.Sort();
                components.Sort((x, y) =>
                    string.Compare(x.FunctionalAttributes.Ref, y.FunctionalAttributes.Ref, StringComparison.InvariantCultureIgnoreCase));
                text += "The previously generated SVF files are covering for the JTGA device "
                    + jtag
                    + " totally "
                    + distinctPinAmount
                    + " different pins, "
                    + nets.Count
                    + " different nets and "
                    + components.Count
                    + " different connected components\n";
                List<string> pinStrings = GetPinsAsStrings(pins);
                GetTableContent("Pin overview", pinStrings, ref text, "pins", ColumnAmountPins);
                GetTableContent("Net overview", nets, ref text, "nets", ColumnAmountNets);
                GetTableContent(
                    "Connected components overview",
                    components.Select(x => x.FunctionalAttributes.Ref).ToList(),
                    ref text,
                    "components",
                    ColumnAmountComponents);
                GetChart(jtag, pins, ref text);
            }

            return text;
        }

        private void GetChart(
            string jtag,
            List<PinTestTypeContainer> pins,
            ref string text)
        {
            ChartsForExportView chartsView = new ChartsForExportView
            {
                Visibility = System.Windows.Visibility.Hidden,
                Left = -ChartsForExportViewModel.SIZE,
                Top = -ChartsForExportViewModel.SIZE,
            };
            chartViews.Add(chartsView);
            ChartsForExportViewModel chartsViewModel = chartsView.DataContext as ChartsForExportViewModel;
            string pathToSave = Path.Combine(Directory.GetCurrentDirectory(), DefaultLatexLocation);
            pathToSave = Path.Combine(pathToSave, DefaultImagesLocations);
            pathToSave += jtag.Replace(":", "_").Replace(" ", string.Empty) + ".jpg";
            chartsViewModel.PathToSave = pathToSave;
            List<PinConnectionType> types = new List<PinConnectionType>
            {
                PinConnectionType.PULLUP,
                PinConnectionType.POWER,
            };
            int pullUpsAmount = GetAllNetsAmountForType(pins, types);
            float pullUpsPercentage = GetPercentageValue(pullUpsAmount, jtag);
            types.Clear();
            types.Add(PinConnectionType.GND);
            types.Add(PinConnectionType.PULLDOWN);
            int pullDownsAmount = GetAllNetsAmountForType(pins, types);
            float pullDownsPercentage = GetPercentageValue(pullDownsAmount, jtag);
            types.Clear();
            types.Add(PinConnectionType.OTHER);
            int othersAmount = GetAllNetsAmountForType(pins, types);
            float othersPercentage = GetPercentageValue(othersAmount, jtag);
            int noneAmount = GetAmountOfUnknownNets(jtag, pullUpsAmount, pullDownsAmount, othersAmount);
            float nonePercentag = 1.0f - pullUpsPercentage - pullDownsPercentage - othersPercentage;
            chartsViewModel.PullDownsAmount = pullDownsAmount;
            chartsViewModel.PullDownsPercentage = pullDownsPercentage;
            chartsViewModel.PullUpsAmount = pullUpsAmount;
            chartsViewModel.PullUpsPercentage = pullUpsPercentage;
            chartsViewModel.OthersAmount = othersAmount;
            chartsViewModel.OthersPercentage = othersPercentage;
            chartsViewModel.NoneAmount = noneAmount;
            chartsViewModel.NonePercentage = nonePercentag;
            chartsViewModel.PullUpsTitle += ": "
                + (pullUpsPercentage * 100).ToString("0.00", CultureInfo.CurrentCulture) + "% (" + pullUpsAmount + ")";
            chartsViewModel.PullDownsTitle += ": "
                + (pullDownsPercentage * 100).ToString("0.00", CultureInfo.CurrentCulture) + "% (" + pullDownsAmount + ")";
            chartsViewModel.OthersTitle += ": "
                + (othersPercentage * 100).ToString("0.00", CultureInfo.CurrentCulture) + "% (" + othersAmount + ")";
            chartsViewModel.NotCoveredNetsTitle += ": "
                + (nonePercentag * 100).ToString("0.00", CultureInfo.CurrentCulture) + "% (" + noneAmount + ")";
            chartsView.Show();
            text += "\\subsection{Pie chart of test coverage}\n";
            text += "\\begin{figure}[H]\n";
            text += " \\begin{center}\n";
            text += "  \\includegraphics[width=1\\textwidth]{images/"
                + jtag.Replace(":", "_").Replace(" ", string.Empty) + "}\n";
            text += " \\end{center}\n";
            text += " \\caption{The complete test coverage overview for device: " + jtag + "}\n";
            text += "\\end{figure}\n";
        }

        private int GetAmountOfUnknownNets(string jtag, int pullUps, int pullDowns, int others)
        {
            int baseNumber = 0;
            var compFound = testCoverageDataModel.Jtags.FirstOrDefault(comp =>
                jtagMapping[jtag].ToLower(CultureInfo.CurrentCulture)
                .Contains(comp.FunctionalAttributes.Ref.ToLower(CultureInfo.CurrentCulture)));
            if (compFound != null)
            {
                baseNumber += compFound.Connections.Count;
            }

            return baseNumber - pullUps - pullDowns - others;
        }

        private float GetPercentageValue(int baseNumber, string jtag)
        {
            int divider = GetPinAmountForJtag(jtag);
            if (divider > 0)
            {
                return (float)baseNumber / divider;
            }
            else
            {
                return 0;
            }
        }

        private int GetPinAmountForJtag(string jtag)
        {
            return testCoverageDataModel.Jtags.FirstOrDefault(comp =>
                jtagMapping[jtag].ToLower(CultureInfo.CurrentCulture)
                .Contains(comp.FunctionalAttributes.Ref.ToLower(CultureInfo.CurrentCulture)))?
                .Connections.Count ?? 0;
        }
    }
}
