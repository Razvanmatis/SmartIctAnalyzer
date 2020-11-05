using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using PCBInvestAPI.DataModel;
using System.Globalization;

namespace PCBInvestAPI.CCZHandling
{
    public class CCZHandler
    {
        private string _filepath;


        private List<TestPoint> _testPoints;

        public List<TestPoint> TestPointsList
        {
            get { return _testPoints; }
            set { _testPoints = value; }
        }




        public CCZHandler(string Path)
        {
            _filepath = Path;           
            _testPoints = new List<TestPoint>();
            this.readInDatas();
        }

        private void readInDatas()
        {
            string[] lines = File.ReadAllLines(_filepath);

            for (int i = 0; i < lines.Length; i++)
            {

                if (lines[i].Contains("ICT--POINT") && lines[i].Contains("Attrib"))
                {
                    string[] values = lines[i - 3].Split(' ');

                    string tpName = "";
                    string tpDes = "";
                    double x = 0.00;
                    double y = 0.00;
                    string pos = "";


                    for (int z = 0; z < values.Length; z++)
                    {
                        if (values[z].Contains("refName"))
                        {
                            string[] value = values[z].Split('=');
                            var st = value[1].Replace('\"',' ');
                            tpName = st;
                        }

                        if (values[z].Contains("x="))
                        {
                            string[] value = values[z].Split('=');
                            var st = value[1].Replace('\"', ' ');
                            x = objecttoDouble(st);

                        }

                        if (values[z].Contains("y="))
                        {
                            string[] value = values[z].Split('=');
                            var st = value[1].Replace('\"', ' ');
                            y = objecttoDouble(st);
                        }

                        if (values[z].Contains("entityNum"))
                        {
                            string[] value = values[z].Split('=');
                            var st = value[1].Replace('\"', ' ');
                            tpDes = st;
                        }

                        if (values[z].Contains("placeBottom"))
                        {
                            string[] value = values[z].Split('=');
                            var st = value[1].Replace('\"', ' ');
                            if (st.Contains("0"))
                            {
                                pos = "top";

                            };
                            if (st.Contains("1"))
                            {
                                pos = "bottom";
                            };

                        }

                    }
                    TestPoint testPoint = new TestPoint(tpName, tpDes, x, y, pos);

                    _testPoints.Add(testPoint);
                }


            }

        }
        private double objecttoDouble(string value)
        {
            double returnvalue;
            bool convertok;

            if (value != null)
            {


                //value.Replace('.', ',');

                convertok = double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out returnvalue);

                if (convertok)
                {
                    return Math.Round(returnvalue, 2);
                }
                return 0.00;

            }

            return 0.00;
        }

    }
}
