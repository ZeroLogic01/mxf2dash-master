using Commons;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slave
{
    class Settings
    {
        public SharedSettings SharedSettings { get; set; }
        public int ListeningPort { get; set; }
        public int WorkPower { get; set; }
        public int CurrentWork { get; set; } = 0;
        //Singleton
        private static Lazy<Settings> Lazy = new Lazy<Settings>();

        private static Settings SettingsBackup = null;
        public static Settings Instance { get { return Lazy.Value; } }

        
    }
}
