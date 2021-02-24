using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Interfaces.Gui;
using Interfaces.PcbInvestigator;

namespace GrpcClientParser.Helper
{
    public class CsvReader
    {
        private Dictionary<string, string> content = new Dictionary<string, string>();
        private ILogger logger;

        public CsvReader(ILogger logger)
        {
            this.logger = logger;
        }

        public Dictionary<string, string> ReadContentFromLine(string path, string separator, int columnRef, int columnValue)
        {
            if (!File.Exists(path))
            {
                logger.LogMessage("No file available under the path " + path, LogCategory.ERROR);
                return content;
            }

            content.Clear();
            try
            {
                List<string> readContent = File.ReadAllLines(path).ToList();
                foreach (string text in readContent)
                {
                    string[] values = text.Split(separator);
                    if (values.Length >= columnRef && values.Length >= columnValue)
                    {
                        content.Add(values[columnRef], values[columnValue]);
                    }
                }
            }
            catch (IOException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
            }
            catch (ArgumentException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
            }
            catch (UnauthorizedAccessException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
            }
            catch (NotSupportedException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
            }
            catch (System.Security.SecurityException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
            }

            return content;
        }

        public IList<IPCBComponent> SetValuesToObjectsFromCsv(IList<IPCBComponent> components, Dictionary<string, string> contentToUse = null)
        {
            if (content.Count == 0 && (contentToUse == null || (contentToUse != null && contentToUse.Count == 0)))
            {
                logger.LogMessage("Perform the reading of the file content before executing this method or provide a valid object!", LogCategory.ERROR);
                return null;
            }

            Dictionary<string, string> contentForAction = contentToUse;
            if (contentForAction == null)
            {
                contentForAction = content;
            }

            IList<IPCBComponent> listWithNotFoundElements = new List<IPCBComponent>();
            foreach (IPCBComponent comp in components)
            {
                if (contentForAction.ContainsKey(comp.FunctionalAttributes.Ref))
                {
                    comp.FunctionalAttributes.Value = content[comp.FunctionalAttributes.Ref];
                }
                else
                {
                    comp.FunctionalAttributes.Value = string.Empty;
                    listWithNotFoundElements.Add(comp);
                }
            }

            return listWithNotFoundElements;
        }
    }
}
