using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Interfaces.PcbInvestigator;

namespace GrpcClientParser.Helper
{
    public class CsvReader
    {
        private Dictionary<string, string> content = new Dictionary<string, string>();

        public Dictionary<string, string> ReadContentFromLine(string path, string separator, int columnRef, int columnValue)
        {
            if (!File.Exists(path))
            {
                Debug.WriteLine("No file available under the path " + path);
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
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            return content;
        }

        public IList<IPCBComponent> SetValuesToObjectsFromCsv(IList<IPCBComponent> components, Dictionary<string, string> contentToUse = null)
        {
            if (content.Count == 0 && (contentToUse == null || (contentToUse != null && contentToUse.Count == 0)))
            {
                Debug.WriteLine("Perform the reading of the file content before executing this method or provide a valid object!");
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
