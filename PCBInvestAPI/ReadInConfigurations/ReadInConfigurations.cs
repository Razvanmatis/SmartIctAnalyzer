using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace PCBInvestAPI.ReadInConfigurations
{
    public class ReadInConfigurations
    {
        private string _pathtoSettings;
        private bool _foundAppSettings;

        public bool FoundAppSettings
        {
            get { return _foundAppSettings; }
            set { _foundAppSettings = value; }
        }

        private ApplicationSettings _settings;

        public ApplicationSettings Settings
        {
            get { return _settings; }
            set { _settings = value; }
        }


        public ReadInConfigurations()
        {
            var pathToApplicationSettings = System.AppContext.BaseDirectory + "appsettings.json";
            _pathtoSettings = pathToApplicationSettings;

            if (File.Exists(pathToApplicationSettings))
            {
                _foundAppSettings = true;
                //Console.WriteLine("Find ApplicationSettings File in {0}", pathToApplicationSettings);
                var filecontent= File.ReadAllText(pathToApplicationSettings);

                ApplicationSettings appsettings = JsonConvert.DeserializeObject<ApplicationSettings>(filecontent);

                _settings = appsettings;
            }
            else
            {
                Console.WriteLine("Coud not find the ApplicationSettings file {0}", pathToApplicationSettings);
                Console.WriteLine("Create new file {0}", pathToApplicationSettings);


                _foundAppSettings = false;

                ApplicationSettings settings = new ApplicationSettings();
                settings.PathtoPCBExe = @"C:\Program Files(x86)\easylogix\PCB - Investigator\PCB - Investigator.exe";
                settings.PCBInvestigatorAvailable = true;
                settings.ExcelWorks = false;


                JsonSerializer serializer = new JsonSerializer();

                _settings = settings;
                using(StreamWriter sw = new StreamWriter(pathToApplicationSettings))
                using(JsonWriter writer = new JsonTextWriter(sw){ Formatting = Formatting.Indented })
                {
                    serializer.Serialize(writer, _settings);
                }  
            }
        }


        public void UpdateSettings(ApplicationSettings newSettings)
        {
            File.Delete(_pathtoSettings);

            _settings = newSettings;
            JsonSerializer serializer = new JsonSerializer();

            using (StreamWriter sw = new StreamWriter(_pathtoSettings))
            using (JsonWriter writer = new JsonTextWriter(sw) { Formatting = Formatting.Indented })
            {
                serializer.Serialize(writer, _settings);
            }
        }
    }
}
