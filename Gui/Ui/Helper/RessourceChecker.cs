using System;
using System.IO;
using System.Reflection;
using ProMik.SmartIct.Interfaces.Gui;

namespace Ui.Helper
{
    public class RessourceChecker
    {
        private readonly ILogger logger;

        public RessourceChecker(ILogger logger)
        {
            this.logger = logger;
        }

        public void CopyLatexRessources()
        {
            string basePath = Directory.GetCurrentDirectory() + "\\LaTex\\";
            string imagesBasePath = basePath + "images\\";
            string[] paths = new string[]
            {
                basePath + "build.bat",
                basePath + "Style.tex",
                basePath + "testCoverage.tex",
                basePath + "TestCoverageData.tex",
                imagesBasePath + "ProMik_R.pdf",
                imagesBasePath + "ProMik-Logo_4C.eps",
                imagesBasePath + "ProMik-Logo_4C-eps-converted-to.pdf",
            };
            string[] res = new string[]
            {
                "Ui.LaTex.build.bat",
                "Ui.LaTex.Style.tex",
                "Ui.LaTex.testCoverage.tex",
                "Ui.LaTex.TestCoverageData.tex",
                "Ui.LaTex.images.ProMik_R.pdf",
                "Ui.LaTex.images.ProMik-Logo_4C.eps",
                "Ui.LaTex.images.ProMik-Logo_4C-eps-converted-to.pdf",
            };
            if (!CheckPath(basePath) || !CheckPath(imagesBasePath))
            {
                logger.LogMessage("Error creating Latex folder!", LogCategory.ERROR);
                return;
            }

            for (int idx = 0; idx < paths.Length; idx++)
            {
                CheckAndWriteResourceToFile(res[idx], paths[idx]);
            }
        }

        private bool CheckPath(string basePath)
        {
            if (Directory.Exists(basePath))
            {
                return true;
            }

            try
            {
                Directory.CreateDirectory(basePath);
                return true;
            }
            catch (Exception e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return false;
            }
        }

        private void CheckAndWriteResourceToFile(string resourceName, string fileName)
        {
            try
            {
                using var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                if (resource == null)
                {
                    logger.LogMessage("Could not find embedded ressource: " + resourceName, LogCategory.ERROR);
                    return;
                }

                if (File.Exists(fileName))
                {
                    try
                    {
                        File.Delete(fileName);
                    }
                    catch (Exception e)
                    {
                        logger.LogMessage(e.Message, LogCategory.ERROR);
                        return;
                    }
                }

                using var file = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                resource.CopyTo(file);
                logger.LogMessage("Successfully copied ressource: " + fileName, LogCategory.INFO);
            }
            catch (Exception e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
            }
        }
    }
}
