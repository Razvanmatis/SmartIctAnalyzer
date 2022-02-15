using ProMik.SmartIct.Interfaces.SvfHelper;
using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;
using System;
using System.Collections.Generic;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter
{
    public abstract class MultiPinWriterBase : ISvfWriter
    {
        public const string NEIGHBOURS = SvfConstant.NEIGHBOURS;

        /// <summary>
        /// apply all OR combined statements of an svf file in dependency on its neighbours
        /// </summary>
        /// <param name="svfData">the svf file to consider</param>
        protected virtual void ApplyCombinedStrings(ISvfData svfData)
        {
            // get the new OR combinde statements
            string newTdi = GetCombindedString(svfData.TdiString, svfData.Neighbours, (x) => x.TdiString);
            string newTdo = GetCombindedString(svfData.TdoString, svfData.Neighbours, (x) => x.TdoString);
            string newMask = GetCombindedString(svfData.Mask, svfData.Neighbours, (x) => x.Mask);

            // save the old ones
            string oldTdi = svfData.TdiString;
            string oldTdo = svfData.TdoString;
            string oldMask = svfData.Mask;

            // set the new properties of the svf with the new values
            svfData.TdiString = newTdi;
            svfData.TdoString = newTdo;
            svfData.Mask = newMask;
            int idx = 0;
            Dictionary<int, string> newValues = new Dictionary<int, string>();

            // search within the contents list after the old values to replace them with the new ones
            foreach (var text in svfData.Content)
            {
                string newValue = string.Empty;
                if (text.Contains(oldTdi))
                {
                    newValue = text.Replace(oldTdi, newTdi);
                }
                else if (text.Contains(oldTdo))
                {
                    newValue = text.Replace(oldTdo, newTdo);
                }
                else if (text.Contains(oldMask))
                {
                    newValue = text.Replace(oldMask, newMask);
                }

                if (!string.IsNullOrEmpty(newValue))
                {
                    newValues.Add(idx, newValue);
                }

                idx++;
            }

            foreach (var (key, value) in newValues)
            {
                svfData.Content[key] = value;
            }
        }

        /// <summary>
        /// get OR combined string of a list
        /// </summary>
        /// <param name="baseValue">the base string to be used</param>
        /// <param name="svfList">the list containing all svfs to be OR added</param>
        /// <param name="func">function to get a specific string of a svf object</param>
        /// <returns></returns>
        protected virtual string GetCombindedString(string baseValue, List<ISvfData> svfList, Func<ISvfData, string> func)
        {
            byte[] source = StringToByteArray(baseValue);

            // iterate all svf of list and apply the OR combinated string values
            foreach (var svf in svfList)
            {
                byte[] destination = StringToByteArray(func(svf));
                source = GetOrCombindedValues(source, destination);
            }

            return ByteArrayToString(source);
        }

        /// <summary>
        /// get OR combined byte array
        /// </summary>
        /// <param name="source">soirce array</param>
        /// <param name="destination">destination array</param>
        /// <returns></returns>
        protected virtual byte[] GetOrCombindedValues(byte[] source, byte[] destination)
        {
            byte[] data = new byte[source.Length];
            for (int idx = 0; idx < source.Length; idx++)
            {
                data[idx] = (byte)(source[idx] | destination[idx]);
            }

            return data;
        }
    }
}
