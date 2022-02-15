using System;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using ProMik.Core.Interfaces.Events;
using ProMik.SmartIct.Interfaces.Container;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.PCBComponentParser.Helper;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.Helper;
using Ui.Modules.ModuleName.Interfaces;
using Ui.Modules.ModuleName.Views;

namespace Ui.Modules.ModuleName.Implementations
{
    public class BomHandler : IBomHandler
    {
        private readonly IBomDataModel bomData;
        private readonly ILogger logger;
        private readonly IProjectHandler generalProjectHandler;
        private readonly IEventService eventService;
        private readonly IResultModel resultModel;
        private readonly ISettingsData settingsData;
        private readonly IManifestHandler manifestHandler;
        private SelectBomView bomView;

        public BomHandler(
            IBomDataModel bomData,
            ILogger logger,
            IProjectHandler generalProjectHandler,
            IEventService eventService,
            IResultModel resultModel,
            ISettingsData settingsData,
            IManifestHandler manifestHandler)
        {
            this.bomData = bomData;
            this.manifestHandler = manifestHandler;
            this.settingsData = settingsData;
            this.eventService = eventService;
            this.logger = logger;
            this.resultModel = resultModel;
            this.generalProjectHandler = generalProjectHandler;
        }

        public bool CheckAndPerformBomParsing()
        {
            if ((!string.IsNullOrEmpty(bomData.BomFile)
                || !string.IsNullOrEmpty(bomData.BomData))
                && !string.IsNullOrEmpty(bomData.ColumnRef)
                && !string.IsNullOrEmpty(bomData.ColumnValue))
            {
                if (!int.TryParse(bomData.ColumnRef, out int colRef))
                {
                    logger.LogMessage("Error parsing the column number for the REF column!", LogCategory.ERROR);
                    return false;
                }

                if (!int.TryParse(bomData.ColumnValue, out int colValue))
                {
                    logger.LogMessage("Error parsing the column number for the VALUE column!", LogCategory.ERROR);
                    return false;
                }

                if (string.IsNullOrEmpty(bomData.BomData))
                {
                    bomData.BomData = Encoding
                        .ASCII
                        .GetString(((GeneralProjectHandler)generalProjectHandler).GetBytesOfFile(bomData.BomFile));
                    if (string.IsNullOrEmpty(bomData.BomData))
                    {
                        return false;
                    }
                }

                CsvReader csvReader = new CsvReader(logger);
                var values = csvReader.ReadContentFromData(bomData.BomData, bomData.Separator, colRef, colValue);
                csvReader.SetValuesToObjectsFromCsv(resultModel.Result.Components, values);
                eventService.Publish<UpdateBomDataEvent>(new UpdateBomDataEvent());
                logger.LogMessage("Import of BOM file " + bomData.BomFile + " finished", LogCategory.INFO);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ResetBomView()
        {
            bomView = null;
        }

        public void OpenBomView(bool autoMode = false)
        {
            if (string.IsNullOrEmpty(bomData.BomData) || bomData.BomSettings == null)
            {
                bomView = new SelectBomView(eventService, false);
                bomView.Top = (Screen.PrimaryScreen.Bounds.Height / 2) - (bomView.Height / 2);
                bomView.Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (bomView.Width / 2);
                bomView.Show();
            }
            else
            {
                eventService.Publish<SelectBomFinishEvent>(new SelectBomFinishEvent(false, autoMode));
            }
        }

        public void SaveBomIntoProjectFile()
        {
            if (bomData == null || string.IsNullOrEmpty(bomData.BomData) || string.IsNullOrEmpty(bomData.BomFile))
            {
                return;
            }

            bool newBom = manifestHandler.UpdateBomFile(
                Encoding.ASCII.GetBytes(bomData.BomData),
                bomData.BomFile[(bomData.BomFile.LastIndexOf("\\", StringComparison.Ordinal) + 1)..]);
            if (newBom)
            {
                logger.LogMessage("Successfully saved BOM file in project file", LogCategory.INFO);
                if (manifestHandler.UpdateBomSettingsFile(
                    bomData.BomSettings,
                    bomData.BomFile[(bomData.BomFile.LastIndexOf("\\", StringComparison.Ordinal) + 1)..] + "_settings"))
                {
                    logger.LogMessage("Successfully saved BOM settings file in project file", LogCategory.INFO);
                }
            }
        }

        public void PerformAfterBomAction(SelectBomFinishEvent eventData)
        {
            if (bomView != null && (eventData == null || !eventData.WasManuallyClosed))
            {
                bomView.Visibility = Visibility.Hidden;
            }

            if (bomData.BomSettings == null)
            {
                bomData.SetBomSettings(new BomSettings(bomData.Separator, bomData.ColumnRef, bomData.ColumnValue));
            }

            if (!eventData.WasManuallyClosed)
            {
                bool result = CheckAndPerformBomParsing();
                settingsData.UseValues = result;
                if (!eventData.AutoModeEnabled && result && manifestHandler.IsManifestHandlingActive())
                {
                    SaveBomIntoProjectFile();
                }
            }
            else
            {
                settingsData.UseValues = false;
            }
        }
    }
}
