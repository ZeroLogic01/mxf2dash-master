using Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using System.IO;

namespace Master
{
    public class Settings
    {
        public static string ConfigFilename = "config.json";
		private static readonly string ConfigurationException = "Configuration was unsuccessful due to {0}, please read the ErrorLogs.txt file";

		public SharedSettings SharedSettings { get; set; }
        public List<Slave> Slaves { get; set; }



        //Singleton
        private static Lazy<Settings> Lazy = new Lazy<Settings>(() => InitialLoad());

        private static Settings SettingsBackup = null;
        public static Settings Instance { get { return Lazy.Value; } }


		private static Settings InitialLoad()
		{
			Console.WriteLine("Loading " + ConfigFilename + " into memory");

			string json = File.ReadAllText(ConfigFilename);

			Settings settings = JsonConvert.DeserializeObject<Settings>(json);


			settings.SharedSettings.Validate();


			Console.WriteLine(ConfigFilename + " loaded successfully");

			Console.WriteLine(Program.InterfaceSeparator);
			SettingsBackup = settings;
			return settings;
		}

        public void PrintConfig()
        {
            string json = JsonConvert.SerializeObject(this,Formatting.Indented);

            Console.WriteLine(json);

        }
    }
}
