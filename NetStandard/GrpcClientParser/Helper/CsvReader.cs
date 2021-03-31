using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Interfaces.Gui;
using Interfaces.PcbInvestigator;

namespace GrpcClientParser.Helper
{
    public class CsvReader
    {
        private readonly Dictionary<string, string> content = new Dictionary<string, string>();
        private readonly ILogger logger;

        public CsvReader(ILogger logger)
        {
            this.logger = logger;
        }

        public Dictionary<string, string> ReadContentFromData(string data, string separator, int columnRef, int columnValue)
        {
            content.Clear();
            List<string> readContent = new List<string>(Regex.Split(data, Environment.NewLine));
            foreach (string text in readContent)
            {
                string[] values = text.Split(separator);
                if (values.Length >= columnRef && values.Length >= columnValue)
                {
                    content.Add(values[columnRef], values[columnValue]);
                }
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
